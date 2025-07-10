using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.BE.NCTS.Business.Testing
{
	[TestedType(typeof(NCTSHouseConsignmentProvider))]
	sealed class NCTSHouseConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<NCTSHouseConsignmentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new NCTSHouseConsignmentProvider(null, ZInt.Zero));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestCountryOfDispatch()
		{
			bill.B0_RN_NKCountryOfExport = "BE";
			AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.CountryOfDispatch);
		}

		public void TestGrossMass()
		{
			bill.B0_Weight = 12.666;
			AssertEquals(12.666m, Provider.GrossMass);
		}

		public void TestReferenceNumberUCR()
		{
			bill.B0_ReferenceID = "ReferenceNumberUCR";
			AssertEquals("ReferenceNumberUCR", Provider.ReferenceNumberUCR);
		}

		public void TestTransportChargesMethodOfPayment()
		{
			bill.B0_TransportPaymentMethod = "D";
			AssertEquals("D", Provider.TransportChargesMethodOfPayment);
		}

		public void TestAdditionalSupplyChainActors()
		{
			Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", parent: bill);
			Factory.CreateCusReference<CusSupplyChainActorReference>("FR1", "SCA", parent: bill);

			CombineAssertions(() =>
			{
				AssertEquals(2, Provider.AdditionalSupplyChainActors.Count);
				AssertEquals(1, Provider.AdditionalSupplyChainActors.First().SequenceNumber);
				AssertEquals(2, Provider.AdditionalSupplyChainActors.Skip(1).First().SequenceNumber);
			});
		}

		public void TestDepartureTransportMeans()
		{
			var departureTransportMeans1 = bill.DepartureTransportInfos.AddNew();
			var departureTransportMeans2 = bill.DepartureTransportInfos.AddNew();

			departureTransportMeans1.TPM_SequenceNumber = 1;
			departureTransportMeans2.TPM_SequenceNumber = 2;

			CombineAssertions(() =>
			{
				AssertEquals(2, Provider.DepartureTransportMeans.Count);
				AssertEquals(1, Provider.DepartureTransportMeans.First().SequenceNumber);
				AssertEquals(2, Provider.DepartureTransportMeans.Skip(1).First().SequenceNumber);
			});
		}

		public void TestPreviousDocuments()
		{
			var previousDoc1 = Factory.CreateCusSupportingInfo("PRE", null, parent: bill);
			var previousDoc2 = Factory.CreateCusSupportingInfo("PRE", null, parent: bill);

			previousDoc1.CSI_LineNo = 1;
			previousDoc2.CSI_LineNo = 2;

			CombineAssertions(() =>
			{
				AssertEquals(2, Provider.PreviousDocuments.Count);
				AssertEquals(1, Provider.PreviousDocuments.First().SequenceNumber);
				AssertEquals(2, Provider.PreviousDocuments.Skip(1).First().SequenceNumber);
			});
		}

		public void TestTransportDocuments()
		{
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: bill);
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: bill);
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: bill);
			AssertEquals(2, Provider.TransportDocuments.Count);
		}

		public void TestAdditionalReferences()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: bill);
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: bill);
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: bill);
			AssertEquals(2, Provider.AdditionalReferences.Count);
		}

		public void TestConsignor()
		{
			AssertNull(Provider.Consignor);
			Factory.CreateJobDocAddress("CRD", "ConsignorName", parent: bill);

			AssertEquals("ConsignorName", Provider.Consignor.Name);
		}

		public void TestConsignee()
		{
			AssertNull(Provider.Consignee);
			Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", parent: bill);

			CombineAssertions(() =>
			{
				AssertEquals("ConsigneeName", Provider.Consignee.Name);
				AssertNull("ConsigneeName", Provider.Consignee.ContactPerson);
			});
		}

		public void TestConsignmentItems()
		{
			bill.GoodsItems.AddNew();
			AssertEquals(1, Provider.ConsignmentItems.Count);
		}

		public void TestSupportingDocuments()
		{
			var supportingDoc1 = Factory.CreateCusSupportingInfo("SUP", null, parent: bill);
			var supportingDoc2 = Factory.CreateCusSupportingInfo("SUP", null, parent: bill);

			supportingDoc1.CSI_LineNo = 1;
			supportingDoc2.CSI_LineNo = 2;

			CombineAssertions(() =>
			{
				AssertEquals(2, Provider.SupportingDocuments.Count);
				AssertEquals(1, Provider.SupportingDocuments.First().SequenceNumber);
				AssertEquals(2, Provider.SupportingDocuments.Skip(1).First().SequenceNumber);
			});
		}

		public void TestAdditionalInformation()
		{
			var additionalDoc1 = Factory.CreateCusSupportingInfo("OTH", "INF", parent: bill);
			var additionalDoc2 = Factory.CreateCusSupportingInfo("OTH", "INF", parent: bill);

			additionalDoc1.CSI_LineNo = 1;
			additionalDoc2.CSI_LineNo = 2;

			CombineAssertions(() =>
			{
				AssertEquals("Additional Information Count", 2, Provider.AdditionalInformation.Count);
				AssertEquals("SequenceNumber of 1st Additional Information", 1, Provider.AdditionalInformation.First().SequenceNumber);
				AssertEquals("SequenceNumber of 2nd Additional Information", 2, Provider.AdditionalInformation.Skip(1).First().SequenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			bill = nctsHeader.Bills.AddNew();

			provider = new NCTSHouseConsignmentProvider(bill, 1);
		}

		NctsBill bill;
		NCTSHouseConsignmentProvider provider;

		protected override NCTSHouseConsignmentProvider GetProvider() => provider;
	}
}
