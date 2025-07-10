using System.Linq;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.GB.Business.Testing;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class ConsignmentItemProviderTest : DataProviderTestCase<ConsignmentItemProvider>
	{
		public void TestGoodsItemNumber()
		{
			AssertEquals(3, Provider.GoodsItemNumber);
		}

		public void TestDeclarationGoodsItemNumber()
		{
			AssertEquals(4, Provider.DeclarationGoodsItemNumber);
		}

		public void TestDeclarationType()
		{
			AssertEquals("D", Provider.DeclarationType);
		}

		public void TestCountryOfDispatch()
		{
			AssertEquals(Core.Constants.CountryCodes.Belgium, Provider.CountryOfDispatch);
		}

		public void TestCountryOfDestination()
		{
			AssertEquals(Core.Constants.CountryCodes.Germany, Provider.CountryOfDestination);
		}

		public void TestReferenceNumberUCR()
		{
			AssertEquals("ref", Provider.ReferenceNumberUCR);
		}

		public void TestConsignee()
		{
			AssertNull(Provider.Consignee);

			var jda = NctsDataRetrieveMethods.GetJobDocAddress(Item, "CEA");
			jda.E2_AddressOverride = true;
			jda.E2_Address1 = "Address1";

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertNotNull("InTransitionPeriod: Consignee", provider.Consignee);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertNull("!InTransitionPeriod: Consignee", provider.Consignee);
			});
		}

		public void TestConsigneeContactPersonExcluded()
		{
			var address = Factory.CreateOrgAddress("Address1", "Address2", "Postcode", "City", Core.Constants.CountryCodes.UnitedKingdom);
			var jda = Factory.CreateJobDocAddress("CEA", "ConsigneeName", "ContactName", "ContactPhone", "ContactEmail", orgAddress: address, parent: Item);
			jda = NctsDataRetrieveMethods.GetJobDocAddress(Item, "CEA");
			jda.E2_AddressOverride = true;
			jda.E2_Address1 = "Address1";

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				AssertNull(Provider.Consignee.ContactPerson);
			});
		}

		public void TestAdditionalSupplyChainActors()
		{
			Factory.CreateCusReference<CusSupplyChainActorReference>("", "SCA", parent: Item);
			AssertEquals(1, Provider.AdditionalSupplyChainActors.Count);
		}
		public void TestCommodity()
		{
			AssertNotNull(Provider.Commodity);
		}

		public void TestPreviousDocuments()
		{
			Factory.CreateCusSupportingInfo("PRE", null, parent: Item);
			AssertEquals(1, Provider.PreviousDocuments.Count);
		}

		public void TestSupportingDocuments()
		{
			Factory.CreateCusSupportingInfo("SUP", null, parent: Item);
			AssertEquals(1, Provider.SupportingDocuments.Count);
		}

		public void TestAdditionalReferences()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: Item, reference: "Additional Reference");
			Factory.CreateCusSupportingInfo("OTH", "INF", parent: Item, reference: "Additional Information");
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: Item, reference: "Transport Document");
			AssertEquals("Additional Reference", Provider.AdditionalReferences.Single().ReferenceNumber);
		}

		public void TestAdditionalInformation()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: Item, reference: "Additional Reference");
			Factory.CreateCusSupportingInfo("OTH", "INF", parent: Item, reference: "Additional Information");
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: Item, reference: "Transport Document");
			AssertEquals("Additional Information", Provider.AdditionalInformation.Single().Text);
		}

		public void TestPackagings()
		{
			Item.Packages.AddNew();
			AssertEquals(1, Provider.Packagings.Count);
		}

		public void TestTransportDocuments()
		{
			Factory.CreateCusSupportingInfo("OTH", "REF", parent: Item, reference: "Additional Reference");
			Factory.CreateCusSupportingInfo("OTH", "INF", parent: Item, reference: "Additional Information");
			Factory.CreateCusSupportingInfo("OTH", "TRA", parent: Item, reference: "Transport Document");

			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: ReferenceNumber", "Transport Document", provider.TransportDocuments.Single().ReferenceNumber);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("!InTransitionPeriod: Count", 0, provider.TransportDocuments.Count);
			});
		}

		public void TestTransportChargesMethodOfPayment()
		{
			DeclarationTestHelper.RunAssertionsInPhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertEquals("InTransitionPeriod: TransportChargesMethodOfPayment", "C", provider.TransportChargesMethodOfPayment);
			});

			DeclarationTestHelper.RunAssertionsOutsidePhase5TransitionPeriod(() =>
			{
				var provider = GetProvider();
				AssertNull("!InTransitionPeriod: TransportChargesMethodOfPayment", provider.TransportChargesMethodOfPayment);
			});
		}

		protected override ConsignmentItemProvider GetProvider()
		{
			return new ConsignmentItemProvider(Item);
		}

		EU.NCTS.Business.NctsDepartureCargoDesc Item => item ??= GetItem();
		EU.NCTS.Business.NctsDepartureCargoDesc item;

		EU.NCTS.Business.NctsDepartureCargoDesc GetItem()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			goodsItem.BY_RN_NKCountryOfDestination = "DE";
			goodsItem.BY_RN_NKCountryOfDispatch = "BE";
			goodsItem.BY_CommercialReferenceNumber = "ref";
			goodsItem.BY_DeclarationGoodsItemNumber = 4;
			goodsItem.BY_LineNo = 3;
			goodsItem.BY_Type = "D";
			goodsItem.BY_TransportChargesMethodOfPayment = "C";
			return goodsItem;
		}
	}
}
