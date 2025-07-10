using CargoWise.EntityFramework;
using Enterprise.Customs.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsHeaderCollection : BusinessObjectCollection<NctsHeader>
	{
		public NctsHeaderCollection(BusinessObjectFactory factory)
			: this(factory, filter: null)
		{
		}

		public NctsHeaderCollection(BusinessObjectFactory factory, GlbCompany company)
			: this(factory, GetCompanyQuery(company))
		{
		}

		public NctsHeaderCollection(BusinessObjectFactory factory, GlbCompany company, ZQuery filter)
			: this(factory, GetCompanyQuery(company).AddToFilter(filter))
		{
		}

		NctsHeaderCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, GetApplicationFilter(filter))
		{
		}

		static ZDBOnlyQuery GetCompanyQuery(GlbCompany company)
		{
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), CusInBondHeaderSchema.BH_GB);
			branchQuery.AddToFilter(GlbBranchSchema.GB_GC, company.PK);
			var query = new ZDBOnlyQuery(typeof(NctsHeader));
			query.AddSubQuery(branchQuery, JoinCondition.And);
			return query;
		}

		static ZQuery GetApplicationFilter(ZQuery filter)
		{
			var result = new ZQuery(CusInBondHeaderSchema.BH_ApplicationCode, new string[] { CusInBondApplicationCodeList.Codes.NCTS4, CusInBondApplicationCodeList.Codes.NCTS5 });
			if (filter != null)
			{
				result.AddToFilter(filter);
			}
			return result;
		}

		protected override IBusinessObjectCollectionFetchStrategy GetFetchStrategy() => new NctsHeaderCollectionFetchStrategy(this);
	}
}
