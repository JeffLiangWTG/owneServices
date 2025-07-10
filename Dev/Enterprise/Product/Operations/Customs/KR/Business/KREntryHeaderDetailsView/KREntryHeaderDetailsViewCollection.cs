using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public class KREntryHeaderDetailsViewCollection : ActiveBusinessObjectCollection<KREntryHeaderDetailsView>
	{
		public KREntryHeaderDetailsViewCollection(BusinessObjectFactory factory, ZString entryNumType, ZQuery additionalQuery, ZGuid companyPK) : base(factory)
		{
			if (companyPK.IsEmpty)
			{
				throw new System.ArgumentException("CompanyPK must not be empty.");
			}
			if (entryNumType.IsEmpty)
			{
				throw new System.ArgumentException("EntryNumType must not be empty.");
			}
			this.companyPK = companyPK;
			this.entryNumType = entryNumType;
			this.additionalQuery = additionalQuery;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var query = base.CreateRelationshipFilter();
			query.AddToFilter(KREntryHeaderDetailsViewSchema.KEH_CompanyPK, companyPK);
			query.AddToFilter(KREntryHeaderDetailsViewSchema.KEH_EntryNumType, entryNumType);
			query.AddToFilter(additionalQuery);
			return query;
		}

		protected override bool AllowNew => false;

		readonly ZGuid companyPK;
		readonly ZString entryNumType;
		readonly ZQuery additionalQuery;
	}
}
