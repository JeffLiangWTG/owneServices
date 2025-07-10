using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE044HouseConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<IE044HouseConsignmentProvider>
	{
		protected override IE044HouseConsignmentProvider GetProvider() => new IE044HouseConsignmentProvider(nctsBill);

		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsBill missing", () => new IE044HouseConsignmentProvider(null));
			});
		}

		public void TestGrossMass()
		{
			nctsBill.B0_GrossWeightUnloaded = 1.234;
			AssertEquals(1.234m, Provider.GrossMass);
		}

		public void TestDepartureTransportMeans()
		{
			var transportMeans1 = nctsBill.DepartureTransportInfos.AddNew();
			transportMeans1.TPM_IdentificationNumber = "ID001";
			transportMeans1.TPM_TransportState = "NEW";
			var transportMeans2 = nctsBill.DepartureTransportInfos.AddNew();
			transportMeans2.TPM_IdentificationNumber = "ID002";
			transportMeans2.TPM_TransportState = "NEW";

			AssertEquals(2, Provider.DepartureTransportMeans.Count);
			AssertEquals("DepartureTransportMeans", "ID001", Provider.DepartureTransportMeans.First().IdentificationNumber);
			AssertEquals("DepartureTransportMeans", "ID002", Provider.DepartureTransportMeans.Last().IdentificationNumber);
		}

		public void TestSupportingDocuments()
		{
			var doc1 = nctsBill.SupportingDocuments.AddNew();
			doc1.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			doc1.CSI_Code = "OTH";
			doc1.CSI_ReferenceNumber = "REF111";
			doc1.CSI_ReferenceNumber2 = "Info 1";

			var doc2 = nctsBill.SupportingDocuments.AddNew();
			doc2.CSI_Code = "SUP";
			doc2.CSI_Status = NctsUnloadedStateList.Codes.NEW;
			doc2.CSI_ReferenceNumber = "REF222";
			doc2.CSI_ReferenceNumber2 = "Info 2";

			var provider = GetProvider();
			AssertEquals("Supporting document count", 2, provider.SupportingDocuments.Count);

			AssertEquals("Doc 1 Type", "OTH", provider.SupportingDocuments.First().Type);
			AssertEquals("Doc 1 Reference", "REF111", provider.SupportingDocuments.First().Reference);
			AssertEquals("Doc 1 Information", "Info 1", provider.SupportingDocuments.First().ComplementOfInformation);

			AssertEquals("Doc 2 Type", "SUP", provider.SupportingDocuments.Last().Type);
			AssertEquals("Doc 2 Reference", "REF222", provider.SupportingDocuments.Last().Reference);
			AssertEquals("Doc 2 Information", "Info 2", provider.SupportingDocuments.Last().ComplementOfInformation);
		}

		public void TestTransportDocuments()
		{
			var doc1 = nctsBill.SupportingDocuments.AddNew();
			doc1.CSI_SubType = "TRA";
			doc1.CSI_Code = "AAA";
			doc1.CSI_ReferenceNumber = "A1";

			var doc2 = nctsBill.SupportingDocuments.AddNew();
			doc2.CSI_SubType = "TRA";
			doc2.CSI_Code = "BBB";
			doc2.CSI_ReferenceNumber = "B2";

			var provider = GetProvider();
			AssertEquals("Transport Document Count", 2, provider.TransportDocuments.Count);

			AssertEquals("Doc 1 Type", "AAA", provider.TransportDocuments.First().Type);
			AssertEquals("Doc 1 Reference", "A1", provider.TransportDocuments.First().Reference);

			AssertEquals("Doc 2 Type", "BBB", provider.TransportDocuments.Last().Type);
			AssertEquals("Doc 2 Reference", "B2", provider.TransportDocuments.Last().Reference);
		}

		public void TestAdditionalReferences()
		{
			var doc1 = nctsBill.SupportingDocuments.AddNew();
			doc1.CSI_Code = "INV";
			doc1.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			doc1.CSI_ReferenceNumber = "1";

			var doc2 = nctsBill.SupportingDocuments.AddNew();
			doc2.CSI_Code = "OTH";
			doc2.CSI_SubType = AdditionalInfoSubTypeList.Codes.AdditionalReference;
			doc2.CSI_ReferenceNumber = "2";

			var provider = GetProvider();

			AssertEquals("Additional Reference Count", 2, provider.AdditionalReferences.Count);

			AssertEquals("Doc 1 Type", "INV", provider.AdditionalReferences.First().Type);
			AssertEquals("Doc 1 Reference", "1", provider.AdditionalReferences.First().Reference);

			AssertEquals("Doc 2 Type", "OTH", provider.AdditionalReferences.Last().Type);
			AssertEquals("Doc 2 Reference", "2", provider.AdditionalReferences.Last().Reference);
		}

		public void TestConsignmentItems()
		{
			var testItem = nctsBill.ArrivalGoodsItems.AddNew();
			testItem.FillWithValidTestData();
			testItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;
			var testItem2 = nctsBill.ArrivalGoodsItems.AddNew();
			testItem2.FillWithValidTestData();
			testItem2.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;

			var provider = GetProvider();
			AssertEquals("Should not send data elements with status=DEC", 1, provider.ConsignmentItems.Count);
		}

		protected override void SetUp()
		{
			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			nctsBill = header.Bills.AddNew();
		}

		NctsBill nctsBill;
	}
}
