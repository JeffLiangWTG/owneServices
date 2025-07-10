using System;
using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.Business.Testing;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class NCTSHouseConsignmentProviderTest : DataProviderTestCase<NCTSHouseConsignmentProvider>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(
				"When 'bill' is null",
				() => new NCTSHouseConsignmentProvider(bill: null, 0));

			AssertExceptionThrown<ArgumentNullException>(
				"When 'bill.Header' is null",
				() => new NCTSHouseConsignmentProvider(bill: Factory.New<NctsBill>(), 0));
		}

		public void TestSequenceNumber()
		{
			AssertEquals(1, Provider.SequenceNumber);
		}

		public void TestCountryOfDispatch()
		{
			bill.B0_RN_NKCountryOfExport = CountryCodes.UnitedKingdom;

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertNull("InTransitionPeriod: CountryOfDispatch", provider.CountryOfDispatch);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: CountryOfDispatch", CountryCodes.UnitedKingdom, provider.CountryOfDispatch);
			});
		}

		public void TestCountryOfDestination()
		{
			bill.B0_RN_NKCountryOfDestination = CountryCodes.Germany;

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertNull("InTransitionPeriod: CountryOfDestination", provider.CountryOfDestination);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: CountryOfDestination", CountryCodes.Germany, provider.CountryOfDestination);
			});
		}

		public void TestGrossMass()
		{
			bill.B0_Weight = 12.666;
			AssertEquals(12.666m, Provider.GrossMass);
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
				AssertEquals("GrossMass is rounded to 3 digits", 12345678.123m, GetProvider().GrossMass);
			});
		}

		public void TestReferenceNumberUCR()
		{
			bill.B0_ReferenceID = "ReferenceNumberUCR";
			AssertEquals("ReferenceNumberUCR", Provider.ReferenceNumberUCR);
		}

		public void TestTransportChargesMethodOfPayment()
		{
			bill.B0_TransportPaymentMethod = "D";

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertNull("InTransitionPeriod: TransportChargesMethodOfPayment", provider.TransportChargesMethodOfPayment);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: TransportChargesMethodOfPayment", "D", provider.TransportChargesMethodOfPayment);
			});
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

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.DepartureTransportMeans.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.DepartureTransportMeans.Count);
				AssertEquals(1, provider.DepartureTransportMeans.First().SequenceNumber);
				AssertEquals(2, provider.DepartureTransportMeans.Skip(1).First().SequenceNumber);
			});
		}

		public void TestPreviousDocuments()
		{
			var previousDoc1 = Factory.CreateCusSupportingInfo("PRE", null, parent: bill);
			var previousDoc2 = Factory.CreateCusSupportingInfo("PRE", null, parent: bill);

			previousDoc1.CSI_LineNo = 1;
			previousDoc2.CSI_LineNo = 2;

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.PreviousDocuments.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.PreviousDocuments.Count);
				AssertEquals(1, provider.PreviousDocuments.First().SequenceNumber);
				AssertEquals(2, provider.PreviousDocuments.Skip(1).First().SequenceNumber);
			});
		}

		public void TestTransportDocuments()
		{
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: bill);
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: bill);
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: bill);

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.TransportDocuments.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.TransportDocuments.Count);
			});
		}

		public void TestAdditionalReferences()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: bill);
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: bill);
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: bill);

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.AdditionalReferences.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.AdditionalReferences.Count);
			});
		}

		public void TestConsignor()
		{
			var address = Factory.CreateOrgAddress("Address1", "Address2", "Postcode", "City", CountryCodes.UnitedKingdom);
			Factory.CreateJobDocAddress("CRD", "ConsignorName", orgAddress: address, parent: bill);

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertNull("InTransitionPeriod: Consignor", provider.Consignor);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("ConsignorName", provider.Consignor.Name);
				AssertEquals("Address1 Address2", provider.Consignor.Address.StreetAndNumber);
				AssertEquals("Postcode", provider.Consignor.Address.Postcode);
				AssertEquals("City", provider.Consignor.Address.City);
				AssertEquals("GB", provider.Consignor.Address.Country);
			});
		}

		public void TestConsignorContactPersonExcluded()
		{
			var address = Factory.CreateOrgAddress("Address1", "Address2", "Postcode", "City", CountryCodes.UnitedKingdom);
			_ = Factory.CreateJobDocAddress("CRD", "ConsignorName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: address, parent: bill);
			var provider = new NCTSHouseConsignmentProvider(bill, 1);
			AssertNull(provider.Consignor.ContactPerson);
		}

		public void TestConsignorReturnsNullIfEmpty()
		{
			AssertNull(Provider.Consignor);
			var address = Factory.CreateOrgAddress("", "", "", "", "");
			var jda = Factory.CreateJobDocAddress("CRD", "", "", "", "", orgAddress: address, parent: bill);
			AssertNull(Provider.Consignor);
			address.Address1 = "Street";
			var provider = new NCTSHouseConsignmentProvider(bill, 1);
			AssertNotNull(provider.Consignor);
		}

		public void TestConsignee()
		{
			AssertNull(Provider.Consignee);
			var address = Factory.CreateOrgAddress("Address1", "Address2", "Postcode", "City", CountryCodes.UnitedKingdom);
			Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: address, parent: bill);

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertNull("InTransitionPeriod: Consignee", provider.Consignee);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("ConsigneeName", provider.Consignee.Name);
				AssertNull("ConsigneeName", provider.Consignee.ContactPerson);
				AssertEquals("Address1 Address2", provider.Consignee.Address.StreetAndNumber);
				AssertEquals("Postcode", provider.Consignee.Address.Postcode);
				AssertEquals("City", provider.Consignee.Address.City);
				AssertEquals("GB", provider.Consignee.Address.Country);
			});
		}

		public void TestConsigneeContactPersonExcluded()
		{
			var address = Factory.CreateOrgAddress("Address1", "Address2", "Postcode", "City", CountryCodes.UnitedKingdom);
			_ = Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: address, parent: bill);
			var provider = new NCTSHouseConsignmentProvider(bill, 1);
			AssertNull(provider.Consignee.ContactPerson);
		}

		public void TestConsigneeReturnsNullIfEmpty()
		{
			AssertNull(Provider.Consignee);
			var address = Factory.CreateOrgAddress("", "", "", "", "");
			var jda = Factory.CreateJobDocAddress("CEA", "", "", "", "", orgAddress: address, parent: bill);
			AssertNull(Provider.Consignee);
			address.Address1 = "Street";
			var provider = new NCTSHouseConsignmentProvider(bill, 1);
			AssertNotNull(provider.Consignee);
		}

		public void TestConsignmentItems()
		{
			bill.GoodsItems.AddNew().BY_LineNo = 3;
			bill.GoodsItems.AddNew().BY_LineNo = 1;
			AssertEquals(2, Provider.ConsignmentItems.Count);
			AssertEquals(1, Provider.ConsignmentItems.First().GoodsItemNumber);
			AssertEquals(3, Provider.ConsignmentItems.Skip(1).First().GoodsItemNumber);
		}

		public void TestSupportingDocuments()
		{
			var supportingDoc1 = Factory.CreateCusSupportingInfo("SUP", null, parent: bill);
			var supportingDoc2 = Factory.CreateCusSupportingInfo("SUP", null, parent: bill);

			supportingDoc1.CSI_LineNo = 1;
			supportingDoc2.CSI_LineNo = 2;

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.SupportingDocuments.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.SupportingDocuments.Count);
				AssertEquals(1, provider.SupportingDocuments.First().SequenceNumber);
				AssertEquals(2, provider.SupportingDocuments.Skip(1).First().SequenceNumber);
			});
		}

		public void TestAdditionalInformation()
		{
			var additionalDoc1 = Factory.CreateCusSupportingInfo("OTH", "INF", parent: bill);
			var additionalDoc2 = Factory.CreateCusSupportingInfo("OTH", "INF", parent: bill);

			additionalDoc1.CSI_LineNo = 1;
			additionalDoc2.CSI_LineNo = 2;

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: Count", 0, provider.AdditionalInformation.Count);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 2, provider.AdditionalInformation.Count);
				AssertEquals("SequenceNumber of 1st Additional Information", 1, provider.AdditionalInformation.First().SequenceNumber);
				AssertEquals("SequenceNumber of 2nd Additional Information", 2, provider.AdditionalInformation.Skip(1).First().SequenceNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			bill = nctsHeader.Bills.AddNew();
		}

		NctsBill bill;

		protected override NCTSHouseConsignmentProvider GetProvider() => new(bill, 1);
	}
}
