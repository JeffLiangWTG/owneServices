using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.KR.Business
{
	public static class ExtendedHoursEntryDataRetriever
	{
		public static KREntryHeaderDetailsView[] GetEntryHeaderDetailsForExtendedHoursRequest(BusinessObjectFactory factory, ZGuid companyPk, IEnumerable<ZString> entryNumbers)
		{
			var query = new ZQuery(KREntryHeaderDetailsViewSchema.KEH_CompanyPK, companyPk);
			query.AddToFilter(KREntryHeaderDetailsViewSchema.KEH_EntryNum, entryNumbers);

			return factory.Load<KREntryHeaderDetailsView>(query);
		}
	}
}
