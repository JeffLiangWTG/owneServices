using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class KREntryCustomsBillsViewCollection : ActiveBusinessObjectCollection<KREntryCustomsBillsView>
	{
		public KREntryCustomsBillsViewCollection(BusinessObjectFactory factory, ZQuery additionalQuery, ZGuid companyPK) : base(factory)
		{
			this.company = factory.Load<GlbCompany>(companyPK);
			this.additionalQuery = additionalQuery;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			if (company == null)
			{
				query = ZQuery.NoResultQuery;
			}
			else
			{
				query.AddToFilter(KREntryCustomsBillsViewSchema.KEB_GC, company.PK);
				query.AddToFilter(additionalQuery);
			}

			return query;
		}

		protected override bool AllowNew => false;

		readonly GlbCompany company;
		readonly ZQuery additionalQuery;
	}
}
