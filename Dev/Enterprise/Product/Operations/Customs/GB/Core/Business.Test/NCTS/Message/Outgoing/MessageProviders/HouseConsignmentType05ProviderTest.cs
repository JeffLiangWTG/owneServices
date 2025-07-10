using System;
using System.Linq;
using CargoWise.Customs.GB.MessageContracts.NCTS.Phase5;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.Business.Testing;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	sealed class HouseConsignmentType05ProviderTest : DataProviderTestCase<HouseConsignmentType05Provider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When 'bill' is null",
				() => new HouseConsignmentType05Provider(bill: null, 0));

			AssertExceptionThrown<ArgumentNullException>(
				"When 'bill.Header' is null",
				() => new HouseConsignmentType05Provider(bill: Factory.New<NctsBill>(), 0));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestGrossMass()
		{
			bill.B0_Weight = 10;
			AssertEquals(10M, Provider.GrossMass);
		}

		public void TestGrossMass_InTransitionPeriod()
		{
			bill.B0_Weight = 12345678.123456789m;
			bill.B0_WeightUQ = "KG";

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				AssertEquals("GrossMass is rounded to 6 digits", 12345678.123457m, GetProvider().GrossMass);
			});

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertEquals("GrossMass is rounded to 3 digits", 12345678.123m, new HouseConsignmentType05Provider(bill, 1).GrossMass);
			});
		}

		public void TestDepartureTransportMeans()
		{
			var transportMeans1 = bill.ArrivalTransportInfos.AddNew();
			transportMeans1.TPM_TransportState = "NEW";
			var transportMeans2 = bill.ArrivalTransportInfos.AddNew();
			transportMeans2.TPM_TransportState = "MIS";
			var transportMeans3 = bill.ArrivalTransportInfos.AddNew();
			transportMeans3.TPM_TransportState = "DEC";

			AssertEquals(2, Provider.DepartureTransportMeans.Count);
		}

		public void TestSupportingDocuments()
		{
			AssertEquals(0, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalReferences()
		{
			var additionalReference1 = Factory.New<CusSupportingInfo>();
			additionalReference1.CSI_ParentID = bill.PK;
			additionalReference1.CSI_Type = "OTH";
			additionalReference1.CSI_SubType = "REF";
			additionalReference1.CSI_Status = "NEW";
			additionalReference1.CSI_ReferenceNumber = "NEW";

			var additionalReference2 = Factory.New<CusSupportingInfo>();
			additionalReference2.CSI_ParentID = bill.PK;
			additionalReference2.CSI_Type = "OTH";
			additionalReference2.CSI_SubType = "REF";
			additionalReference2.CSI_LineNo = 1;
			additionalReference2.CSI_Status = "MIS";

			var additionalReference3 = Factory.New<CusSupportingInfo>();
			additionalReference3.CSI_ParentID = bill.PK;
			additionalReference3.CSI_Type = "OTH";
			additionalReference3.CSI_SubType = "REF";
			additionalReference3.CSI_LineNo = 2;
			additionalReference3.CSI_Status = "DEC";

			CombineAssertions(() =>
			{
				AssertEquals(Provider.AdditionalReferences, ((IHouseConsignmentType05)Provider).AdditionalReferences);
				AssertEquals("NEW included", expected: true, Provider.AdditionalReferences.Any(a => a.ReferenceNumber == "NEW"));
				AssertEquals("MIS included", expected: true, Provider.AdditionalReferences.Any(a => a.SequenceNumber == 1));
			});
		}

		public void TestConsignmentItems()
		{
			bill.ArrivalGoodsItems.AddNew();
			bill.ArrivalGoodsItems.AddNew();
			var item3 = bill.ArrivalGoodsItems.AddNew();
			item3.BY_UnloadedState = NctsUnloadedStateList.Codes.DEC;
			AssertEquals(2, Provider.ConsignmentItems.Count);
		}

		public void TestTransportDocuments()
		{
			var transportDocument1 = Factory.New<CusSupportingInfo>();
			transportDocument1.CSI_ParentID = bill.PK;
			transportDocument1.CSI_Type = "OTH";
			transportDocument1.CSI_SubType = "TRA";
			transportDocument1.CSI_Status = "NEW";
			transportDocument1.CSI_ReferenceNumber = "NEW";

			var transportDocument2 = Factory.New<CusSupportingInfo>();
			transportDocument2.CSI_ParentID = bill.PK;
			transportDocument2.CSI_Type = "OTH";
			transportDocument2.CSI_SubType = "TRA";
			transportDocument2.CSI_LineNo = 1;
			transportDocument2.CSI_Status = "MIS";

			var transportDocument3 = Factory.New<CusSupportingInfo>();
			transportDocument3.CSI_ParentID = bill.PK;
			transportDocument3.CSI_Type = "OTH";
			transportDocument3.CSI_SubType = "TRA";
			transportDocument3.CSI_LineNo = 2;
			transportDocument3.CSI_Status = "DEC";

			CombineAssertions(() =>
			{
				AssertEquals("NEW included", expected: true, Provider.TransportDocuments.Any(a => a.ReferenceNumber == "NEW"));
				AssertEquals("MIS included", expected: true, Provider.TransportDocuments.Any(a => a.SequenceNumber == 1));
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var header = Factory.New<NctsHeader>();
			header.SetMovementType(NctsMovementType.Codes.Arrival);
			bill = header.Bills.AddNew();

			provider = new HouseConsignmentType05Provider(bill, 1);
		}

		protected override HouseConsignmentType05Provider GetProvider() => provider;

		HouseConsignmentType05Provider provider;
		NctsBill bill;
	}
}
