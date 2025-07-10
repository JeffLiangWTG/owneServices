using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business.Test
{
	sealed class AdditionalFiscalReferenceLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestAdditionalFiscalReferenceTypesList()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_ManifestType = EUICS2ManifestTypes.Codes.ENS;
			var bill = header.Bills.AddNew();
			var additionalFiscalReference = bill.AdditionalFiscalReferences.AddNew();
			var list = additionalFiscalReference.Lookups.CodeList;

			AssertSame("Accessing the list twice should get the exact same object as the list is cached", list, additionalFiscalReference.Lookups.CodeList);
			AssertType<EUICS2AdditionalFiscalReferenceTypes>(list);
			AssertEquals("FR5, FR6", list.CodesAsString);
		}
	}
}
