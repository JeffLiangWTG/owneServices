using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class DepotCusOutturnCollection : BusinessObjectCollection<DepotCusOutturn>
	{
		public DepotCusOutturnCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return new ZQuery(CusOutturnSchema.C5_C6, SQLComparisonOperator.NotEqual, null);
		}
	}
}
