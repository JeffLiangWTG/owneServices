using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEuOfficeCodeCollectionForDepartureGrid : NctsEuOfficeCodeCollection
	{
		public NctsEuOfficeCodeCollectionForDepartureGrid(NctsHeader master) : base(master)
		{
		}

		public NctsEuOfficeCodeCollectionForDepartureGrid(NctsCommonMovementHeader master) : base(master)
		{
		}

		protected override ZQuery CreateAdditionalFilter()
		{
			var result = base.CreateAdditionalFilter();
			var nctsEuOfficeCodeCollectionQuery = new ZQuery(CusCodeDataSchema.CY_Code, SQLComparisonOperator.NotEqual, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival);
			nctsEuOfficeCodeCollectionQuery.AddToFilter(CusCodeDataSchema.CY_ParentID, Master.PK);
			result.AddToFilter(nctsEuOfficeCodeCollectionQuery);
			return result;
		}
	}
}
