using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE007TraderAtDestinationProviderTest : Customs.Business.Testing.DataProviderTestCase<IE007TraderAtDestinationProvider>
	{
		protected override IE007TraderAtDestinationProvider GetProvider() => new IE007TraderAtDestinationProvider(nctsHeader);

		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE007TraderAtDestinationProvider(null));
		}

		public void TestIdentificationNumber()
		{
			var provider = GetProvider();
			AssertEquals("Identification Number", "IE0123456789000", provider.IdentificationNumber);
		}
		public void TestCommunicationLanguageAtDestination()
		{
			var provider = GetProvider();
			AssertEquals("Language at destination", "IE", provider.CommunicationLanguageAtDestination);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);

			traderAtDestination = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "0123456789000", "TIR123");
			var contact = traderAtDestination.Contacts.AddNew();
			contact.OC_ContactName = "Joe Bloggs";
			contact.OC_Phone = "5551234";
			contact.OC_Email = "test@example.com";
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			nctsHeader.DestinationTrader.OrganisationPK = traderAtDestination.PK;
		}
		NctsHeader nctsHeader;
		OrgHeader traderAtDestination;
	}
}
