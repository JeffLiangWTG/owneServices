using System.Collections;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CollectionCache
	{
		public CollectionCache(BusinessObjectFactory factory)
		{
			this.factory = factory;
			cachedCollections = new Hashtable();
		}

		protected virtual DynamicBusinessObjectCollection GetDataFromDatabase(ZString queryString, ZSqlParameter[] parameters)
		{
			DynamicBusinessObjectCollection result = new DynamicBusinessObjectCollection(factory);
			result.Load(queryString, parameters);
			return result;
		}

		public DynamicBusinessObjectCollection Fetch(ZString queryString, ZSqlParameter[] parameters)
		{
			ZString cacheKey = queryString;
			if (parameters != null)
			{
				foreach (ZSqlParameter parameter in parameters)
				{
					cacheKey = cacheKey.Replace(parameter.ParameterName, parameter.Value.ToString());
				}
			}

			DynamicBusinessObjectCollection result = cachedCollections[cacheKey] as DynamicBusinessObjectCollection;
			if (result == null)
			{
				result = GetDataFromDatabase(queryString, parameters);
				cachedCollections.Add(cacheKey, result);
			}
			return result;
		}

		#region Implementation
		protected BusinessObjectFactory factory;
		protected Hashtable cachedCollections;
		#endregion
	}
}
