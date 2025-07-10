using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ES.Business.Declaration.Testing
{
	public class AdditionalInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestKindList_InvoiceHeader()
		{
			var lookups = new AdditionalInfoLookups(Factory.New<AdditionalInfo>());
			var list = lookups.KindList;
			CombineAssertions(() =>
			{
				AssertArrayEqualsByElements("List", new[] { AdditionalDocList.Codes.AdditionalInformation, AdditionalDocList.Codes.AdditionalReference, AdditionalDocList.Codes.TransportDocuments }, list.GetAllCodes());
				AssertSame("Cached", list, lookups.KindList);
			});
		}
	}
}
