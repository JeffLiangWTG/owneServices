using System;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE044MessageProviderTest : Customs.Business.Testing.DataProviderTestCase<IE044MessageProvider>
	{
		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsHeader missing", () => new IE044MessageProvider(null));
			});
		}

		protected override IE044MessageProvider GetProvider() => new IE044MessageProvider(nctsHeader);

		public void TestTransitOperation()
		{
			nctsHeader.ArrivalMrnFromUser = "1234567";
			movementHeader.OtherThingsToReport = "Test data";
			var provider = GetProvider();
			AssertNotNull(provider);
			AssertEquals("MRN", "1234567", provider.TransitOperation.MRN);
			AssertEquals("Other things to report", "Test data", provider.TransitOperation.OtherThingsToReport);
		}

		public void TestDestinationOffice()
		{
			nctsHeader.ArrivalMovementHeader.CustomsOffices.AddNew("DSA", "IEABC222");

			var provider = GetProvider();
			AssertEquals("Office of Destination", "IEABC222", provider.DestinationOffice);
		}

		public void TestTraderAtDestination()
		{
			nctsHeader.DestinationTrader.OrganisationPK = traderAtDestination.PK;

			var provider = GetProvider();
			AssertEquals("Identification Number", "IE0123456789000", provider.TraderAtDestination);
		}

		public void TestUnloadingRemark()
		{
			movementHeader.BM_CustomsStatus = "UAP";
			movementHeader.BM_UnloadingRemarks = "Remarks";
			var provider = GetProvider();
			AssertEquals("Unloading remarks", "Remarks", provider.UnloadingRemark.UnloadingRemark);
		}

		public void TestConsignmentTransitConforming()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_NoChangesToReport = true;

			var provider = new IE044MessageProvider(nctsHeader);
			AssertNull("Conignment Not Included when transit conforms", provider.Consignment);
		}

		public void TestConsignment_TransitNotConforming()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			var movementHeader = nctsHeader.ArrivalMovementHeader;
			movementHeader.BM_NoChangesToReport = false;
			movementHeader.BM_GrossWeightUnloaded = 4.7;

			var provider = new IE044MessageProvider(nctsHeader);
			AssertEquals("Total Weight", 4.7m, provider.Consignment.GrossMass);
		}

		protected override void SetUp()
		{
			base.SetUp();
			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			movementHeader = nctsHeader.ArrivalMovementHeader;
			traderAtDestination = NCTSTestHelper.CreateJobDocAddressForTest(Factory, "PC1", nctsHeader.Principal, string.Empty, "Test Company Limited", "123 Test Street", "A12B3C4", "City", "IEXX", "IE", "0123456789000", "TIR123");
			var contact = traderAtDestination.Contacts.AddNew();
			contact.OC_ContactName = "Joe Bloggs";
			contact.OC_Phone = "5551234";
			contact.OC_Email = "test@example.com";
			contact.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
		}

		NctsHeader nctsHeader;
		NctsArrivalMovementHeader movementHeader;
		OrgHeader traderAtDestination;
	}
}
