using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	internal class CusSCAHouseFetchStrategy : EnterpriseBusinessObjectFetchStrategy
	{
		public CusSCAHouseFetchStrategy(CusSCAHouse house) : base(house)
		{
		}

		protected override void FetchForViewCore(TableColumn[] columns)
		{
			base.FetchForViewCore(columns);
			if (columns.Any(x => x.ColumnName == nameof(CusSCAHouse.ShipmentStatus)))
			{
				Factory.AddFetchHint(CusSCAPivotSchema.CV_CA, BusinessObject.PK);
			}
		}
	}
}
