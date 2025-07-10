using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusSCAHouseCollectionNonDependent : BusinessObjectCollection<CusSCAHouse>
	{
		public CusSCAHouseCollectionNonDependent(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public CusSCAHouseCollectionNonDependent(BusinessObjectFactory factory, ZQuery additionalFilter)
			: base(factory, additionalFilter)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(CusSCAHouse));
			var cusSCAOceanBillQuery = new ZDBOnlySubQuery(typeof(CusSCAOceanBill), CusSCAHouseSchema.CA_CB);
			cusSCAOceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_IsActive, true);
			cusSCAOceanBillQuery.AddToFilter(CusSCAOceanBillSchema.CB_ApplicationCode, CusSCAOceanBill.ApplicationCodes);
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), CusSCAOceanBillSchema.CB_GB);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK);
			cusSCAOceanBillQuery.AddSubQuery(branchQuery, JoinCondition.And);
			query.AddSubQuery(cusSCAOceanBillQuery, JoinCondition.And);
			result.AddToFilter(query);
			return result;
		}
	}
}
