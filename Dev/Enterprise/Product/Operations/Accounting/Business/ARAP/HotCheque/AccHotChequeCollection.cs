
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.HotCheque
{
	public class AccHotChequeCollection : BusinessObjectCollection<AccHotCheque>
	{
		public AccHotChequeCollection(BusinessObjectFactory factory, ZQuery query)
			: base(factory, query)
		{
		}

		public AccHotChequeCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			return GetChequeBookCollection();
		}

		protected ZQuery GetChequeBookCollection()
		{
			ZQuery chqBookFilter = new ZQuery();
			AccChequeBookCollection chqBookCollection = new AccChequeBookCollection(Factory, GenerateCompanyFilter(AccChequeBookSchema.AK_GB, Factory));
			chqBookCollection.Load();

			if (chqBookCollection.Count != 0)
			{
				chqBookFilter.AddToFilter(JoinCondition.Or, AccHotChequeSchema.AQ_AK, chqBookCollection.GetPKs());
			}
			else	// There are no HotCheques created by the current company so set the filter to never return any results
			{
				chqBookFilter.IsNoResultQuery = true;
			}
			return chqBookFilter;
		}

		public static ZQuery GenerateCompanyFilter(SchemaColumn columnToFilter, BusinessObjectFactory factory)
		{
			GlbBranchCollection branches = new GlbBranchCollection(factory, new ZQuery(GlbBranchSchema.GB_GC, GlbCompany.CurrentCompany.PK));
			branches.Load();
			return new ZQuery(columnToFilter, branches.GetPKs());
		}
	}
}
