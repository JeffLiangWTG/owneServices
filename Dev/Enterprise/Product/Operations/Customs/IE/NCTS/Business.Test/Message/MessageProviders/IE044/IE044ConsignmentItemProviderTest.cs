using System;
using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	class IE044ConsignmentItemProviderTest : Customs.Business.Testing.DataProviderTestCase<IE044ConsignmentItemProvider>
	{
		protected override IE044ConsignmentItemProvider GetProvider() => new IE044ConsignmentItemProvider(consignmentItem);

		public void TestConstructor()
		{
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentException>("NctsDepartureCargoDesc missing", () => new IE044ConsignmentItemProvider(null));
			});
		}

		public void TestGoodsItemNumber()
		{
			AssertEquals("Goods Item Number", "1", Provider.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals("Declaration Goods Item Number", "1", Provider.GoodsItemNumber);
		}

		public void TestCommodity()
		{
			AssertNotNull(Provider.Commodity);
		}

		public void TestPackages()
		{
			var package1 = consignmentItem.Packages.AddNew();
			package1.B5_SequenceNumber = 1;
			package1.B5_UnitType = "A";
			package1.B5_UnitCount = 10;
			package1.B5_MarksAndNumbers = "PACK1";
			package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;

			var package2 = consignmentItem.Packages.AddNew();
			package2.B5_SequenceNumber = 2;
			package2.B5_UnitType = "B";
			package2.B5_UnitCount = 50;
			package2.B5_MarksAndNumbers = "PACK2";
			package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

			var provider = GetProvider();
			AssertEquals("DEC will not include", 1, provider.Packages.Count);
			AssertEquals("Package 1 Type", "A", provider.Packages.First().PackageType);
			AssertEquals("Package 1 Qty", 10, provider.Packages.First().PackageQuantity);
			AssertEquals("Package 1 Shipping Marks", "PACK1", provider.Packages.First().ShippingMarks);
		}

		public void TestSupportingDocuments()
		{
			var doc1 = consignmentItem.SupportingDocuments.AddNew();
			doc1.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
			doc1.CSI_Code = CusSupportingInfoTypeList.Codes.ScreeningMethod;
			doc1.CSI_ItemNumber = 1;
			doc1.CSI_ReferenceNumber = "REF111";
			doc1.CSI_ReferenceNumber2 = "Info 1";
			doc1.CSI_Status = NctsUnloadedStateList.Codes.DIF;

			var doc2 = consignmentItem.SupportingDocuments.AddNew();
			doc2.CSI_Type = CusSupportingInfoTypeList.Codes.SupportingDocument;
			doc2.CSI_Code = CusSupportingInfoTypeList.Codes.FiscalReference;
			doc2.CSI_ItemNumber = 1;
			doc2.CSI_ReferenceNumber = "REF222";
			doc2.CSI_ReferenceNumber2 = "Info 2";
			doc2.CSI_Status = NctsUnloadedStateList.Codes.DEC;

			var provider = GetProvider();
			AssertEquals("DEC will not include", 1, provider.SupportingDocuments.Count);

			AssertEquals("Doc 1 Type", "SNM", provider.SupportingDocuments.First().Type);
			AssertEquals("Doc 1 Reference", "REF111", provider.SupportingDocuments.First().Reference);
			AssertEquals("Doc 1 Item Number", 1, provider.SupportingDocuments.First().LineItemNumber);
			AssertEquals("Doc 1 Information", "Info 1", provider.SupportingDocuments.First().ComplementOfInformation);
		}

		public void TestTransportDocuments()
		{
			var doc1 = consignmentItem.AdditionalInfos.AddNew();
			doc1.CSI_SubType = "TRA";
			doc1.CSI_LineNo = 1;
			doc1.CSI_Code = TransportTypeList.Codes.Road;
			doc1.CSI_ReferenceNumber = "A1";

			var doc2 = consignmentItem.AdditionalInfos.AddNew();
			doc2.CSI_SubType = "TRA";
			doc2.CSI_Code = TransportTypeList.Codes.Rail;
			doc2.CSI_ReferenceNumber = "B2";

			var provider = GetProvider();
			AssertEquals("Transport Document Count", 2, provider.TransportDocuments.Count);

			AssertEquals("Doc 1 Type", "ROA", provider.TransportDocuments.First().Type);
			AssertEquals("Doc 1 Reference", "A1", provider.TransportDocuments.First().Reference);

			AssertEquals("Doc 2 Type", "RAI", provider.TransportDocuments.Last().Type);
			AssertEquals("Doc 2 Reference", "B2", provider.TransportDocuments.Last().Reference);
		}

		public void TestAdditionalReferences()
		{
			var doc1 = consignmentItem.AdditionalInfos.AddNew();
			doc1.CSI_SubType = "REF";
			doc1.CSI_Code = "INV";
			doc1.CSI_ReferenceNumber = "1";

			var doc2 = consignmentItem.AdditionalInfos.AddNew();
			doc2.CSI_SubType = "REF";
			doc2.CSI_Code = "OTH";
			doc2.CSI_ReferenceNumber = "2";

			var provider = GetProvider();

			AssertEquals("Additional Reference Count", 2, provider.AdditionalReferences.Count);

			AssertEquals("Doc 1 Type", "INV", provider.AdditionalReferences.First().Type);
			AssertEquals("Doc 1 Reference", "1", provider.AdditionalReferences.First().Reference);

			AssertEquals("Doc 2 Type", "OTH", provider.AdditionalReferences.Last().Type);
			AssertEquals("Doc 2 Reference", "2", provider.AdditionalReferences.Last().Reference);
		}

		protected override void SetUp()
		{
			var header = Factory.NewWithValidTestData<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			var bill = header.Bills.AddNew();
			consignmentItem = bill.ArrivalGoodsItems.AddNew();
			consignmentItem.BY_LineNo = 1;
		}

		NctsArrivalCargoDesc consignmentItem;
	}
}
