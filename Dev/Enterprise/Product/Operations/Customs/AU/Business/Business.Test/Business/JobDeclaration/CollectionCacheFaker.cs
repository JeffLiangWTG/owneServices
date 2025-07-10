using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class CollectionCacheFaker : CollectionCache
	{
		public CollectionCacheFaker(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public bool HasHitDatabase;

		protected override DynamicBusinessObjectCollection GetDataFromDatabase(ZString queryString, ZSqlParameter[] parameters)
		{
			HasHitDatabase = true;
			return base.GetDataFromDatabase(queryString, parameters);
		}
	}
}
