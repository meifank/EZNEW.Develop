﻿using EZNEW.Framework.Paging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using EZNEW.Framework.Extension;
using EZNEW.Develop.Entity;
using EZNEW.Framework.Fault;

namespace EZNEW.Develop.CQuery
{
    /// <summary>
    /// query factory
    /// </summary>
    public static class QueryFactory
    {
        #region methods

        /// <summary>
        /// create a new query instance
        /// </summary>
        /// <returns>IQuery object</returns>
        public static IQuery Create()
        {
            return new QueryInfo();
        }

        /// <summary>
        /// create a new query instance
        /// </summary>
        /// <param name="filter">pagingfilter</param>
        /// <returns>IQuery object</returns>
        public static IQuery Create(PagingFilter filter)
        {
            var query = Create();
            if (filter != null)
            {
                query.PagingInfo = filter;
            }
            return query;
        }

        /// <summary>
        /// create a new query instance
        /// </summary>
        /// <typeparam name="T">query model</typeparam>
        /// <returns>IQuery object</returns>
        public static IQuery Create<T>() where T : QueryModel<T>
        {
            QueryModel<T>.Init();
            var query = Create();
            var entityType = QueryManager.GetQueryModelRelationEntityType<T>();
            if (entityType == null)
            {
                throw new EZNEWException(string.Format("query model:{0} didn't relate any entity", typeof(T).FullName));
            }
            query.SetEntityType(entityType);
            return query;
        }

        /// <summary>
        /// create a new query instance
        /// </summary>
        /// <typeparam name="T">query model</typeparam>
        /// <returns>IQuery object</returns>
        public static IQuery Create<T>(PagingFilter filter) where T : QueryModel<T>
        {
            var query = Create<T>();
            if (filter != null)
            {
                query.PagingInfo = filter;
            }
            return query;
        }

        /// <summary>
        /// create a new query instance
        /// </summary>
        /// <typeparam name="T">data type</typeparam>
        /// <param name="criteria">condition expression</param>
        /// <returns>IQuery object</returns>
        public static IQuery Create<T>(Expression<Func<T, bool>> criteria) where T : QueryModel<T>
        {
            IQuery query = Create<T>();
            if (criteria != null)
            {
                query.And(criteria);
            }
            return query;
        }

        /// <summary>
        /// create a new query instance by entity
        /// </summary>
        /// <typeparam name="ET"></typeparam>
        /// <returns></returns>
        static IQuery CreateByEntity<ET>()
        {
            var query = Create();
            query.SetEntityType(typeof(ET));
            return query;
        }

        /// <summary>
        /// append entity identity condition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="datas">datas</param>
        /// <param name="originalQuery">original query</param>
        /// <param name="exclude">exclude</param>
        /// <returns></returns>
        public static IQuery AppendEntityIdentityCondition<T>(IEnumerable<T> datas, IQuery originalQuery = null, bool exclude = false) where T : BaseEntity<T>, new()
        {
            originalQuery = originalQuery ?? CreateByEntity<T>();
            if (datas == null || !datas.Any())
            {
                return originalQuery;
            }
            var entityType = typeof(T);
            var keys = EntityManager.GetPrimaryKeys(entityType);
            if (keys.IsNullOrEmpty())
            {
                throw new Exception(string.Format("type:{0} isn't set primary keys", entityType.FullName));
            }
            var firstData = datas.ElementAt(0).GetPropertyValue(keys.ElementAt(0));
            var dataType = firstData.GetType();
            dynamic keyValueList = Activator.CreateInstance(typeof(List<>).MakeGenericType(dataType));
            //List<dynamic> keyValueList = new List<dynamic>();
            foreach (T entity in datas)
            {
                if (keys.Count == 1)
                {
                    keyValueList.Add(entity.GetPropertyValue(keys.ElementAt(0)));
                }
                else
                {
                    IQuery entityQuery = Create();
                    foreach (var key in keys)
                    {
                        entityQuery.And(key, exclude ? CriteriaOperator.NotEqual : CriteriaOperator.Equal, entity.GetPropertyValue(key));
                    }
                    originalQuery.Or(entityQuery);
                }
            }
            if (keys.Count == 1)
            {
                if (exclude)
                {
                    originalQuery.NotIn(keys.ElementAt(0), keyValueList);
                }
                else
                {
                    originalQuery.In(keys.ElementAt(0), keyValueList);
                }
            }
            return originalQuery;
        }

        /// <summary>
        /// append entity identity condition
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        /// <param name="originalQuery"></param>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public static IQuery AppendEntityIdentityCondition<T>(T data, IQuery originalQuery = null, bool exclude = false) where T : BaseEntity<T>, new()
        {
            originalQuery = originalQuery ?? CreateByEntity<T>();
            if (data == null)
            {
                return originalQuery;
            }
            Type entityType = typeof(T);
            var keys = EntityManager.GetPrimaryKeys(entityType);
            if (keys.IsNullOrEmpty())
            {
                throw new Exception(string.Format("type:{0} is not set primary keys", entityType.FullName));
            }
            foreach (var key in keys)
            {
                originalQuery.And(key, exclude ? CriteriaOperator.NotEqual : CriteriaOperator.Equal, data.GetPropertyValue(key));
            }
            return originalQuery;
        }

        #endregion
    }
}
