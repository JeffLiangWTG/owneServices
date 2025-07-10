using System;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.EMCS.Version4_1.ie801;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.Customs.GB.EMCS.Messaging.Version4_1;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class EMCSJobDeclarationUpdateHelperTest : TestCaseWithFactory
	{
		public void TestCreateOrUpdateGuarantor_Null()
		{
			TestOldAddressIsDeletedAndNewAddressIsCreated(
				() => declaration.OwnerDocumentaryAddress,
				() => declaration.CreateOrUpdateGuarantor(null),
				ZString.Empty);
		}

		public void TestCreateOrUpdateGuarantor_MatchesExistingTEN()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "Existing address with matching TEN";
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, Core.Constants.CountryCodes.UnitedKingdom, "GBTEN001", orgAddress.PK);
			Factory.Save();

			var guarantorTrader = new GuarantorTraderType { TraderExciseNumber = "GBTEN001" };

			TestOldAddressIsDeletedAndNewAddressIsCreated(
				() => declaration.OwnerDocumentaryAddress,
				() => declaration.CreateOrUpdateGuarantor(IE801PartyGuarantorProvider.NewOrNull(guarantorTrader)),
				"Existing address with matching TEN");
		}

		public void TestCreateOrUpdateGuarantor_MatchesExistingVAT()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "Existing address with matching VAT";
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.FranceCodeTypes.TVA, Core.Constants.CountryCodes.France, "VAT001", orgAddress.PK);
			Factory.Save();

			var guarantorTrader = new GuarantorTraderType { VatNumber = "FRVAT001" };

			TestOldAddressIsDeletedAndNewAddressIsCreated(
				() => declaration.OwnerDocumentaryAddress,
				() => declaration.CreateOrUpdateGuarantor(IE801PartyGuarantorProvider.NewOrNull(guarantorTrader)),
				"Existing address with matching VAT");
		}

		public void TestCreateOrUpdateGuarantor_NotMatchExisting_TEN()
		{
			var guarantorTrader = new GuarantorTraderType
			{
				TraderExciseNumber = "GBTEN002",
				Language = "EN",
				City = "London",
				StreetName = "Street A",
				StreetNumber = "9",
				Postcode = "2000",
				TraderName = "MyComp"
			};

			TestOldAddressIsDeletedAndNewAddressIsCreatedWithCopiedDetails(
				() => declaration.OwnerDocumentaryAddress,
				() => declaration.CreateOrUpdateGuarantor(IE801PartyGuarantorProvider.NewOrNull(guarantorTrader)),
				"GBTEN002",
				OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber,
				"GB",
				"London",
				"Street A 9",
				"2000",
				"MyComp"
			);
		}

		public void TestCreateOrUpdateGuarantor_NotMatchExisting_VAT()
		{
			var guarantorTrader = new GuarantorTraderType
			{
				VatNumber = "ELVAT002",
				Language = "EN",
				City = "London",
				StreetName = "Street A",
				StreetNumber = "9",
				Postcode = "2000",
				TraderName = "MyComp"
			};

			TestOldAddressIsDeletedAndNewAddressIsCreatedWithCopiedDetails(
				() => declaration.OwnerDocumentaryAddress,
				() => declaration.CreateOrUpdateGuarantor(IE801PartyGuarantorProvider.NewOrNull(guarantorTrader)),
				"VAT002",
				OrgCusCode.GreeceCodeTypes.AFM,
				"GR",
				"London",
				"Street A 9",
				"2000",
				"MyComp");
		}

		public void TestCreateOrUpdateDeliveryPlace_Null()
		{
			TestOldAddressIsDeletedAndNewAddressIsCreated(
				() => declaration.DestinationWarehouseDocumentaryAddress,
				() => declaration.CreateOrUpdateDeliveryPlace(null),
				ZString.Empty);
		}

		public void TestCreateOrUpdateDeliveryPlace_MatchesExistingTID()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "Existing address with matching TID";
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, Core.Constants.CountryCodes.UnitedKingdom, "GBTID001", orgAddress.PK);
			Factory.Save();

			var deliveryPlace = new DeliveryPlaceTraderType { Traderid = "GBTID001" };

			TestOldAddressIsDeletedAndNewAddressIsCreated(
				() => declaration.DestinationWarehouseDocumentaryAddress,
				() => declaration.CreateOrUpdateDeliveryPlace(IE801PartyDeliveryPlaceProvider.NewOrNull(deliveryPlace)),
				"Existing address with matching TID");
		}

		public void TestCreateOrUpdateDeliveryPlace_NotMatchExisting()
		{
			var deliveryPlace = new DeliveryPlaceTraderType
			{
				Traderid = "GBTID001",
				Language = "EN",
				City = "London",
				StreetName = "Street A",
				StreetNumber = "9",
				Postcode = "2000",
				TraderName = "MyComp"
			};

			TestOldAddressIsDeletedAndNewAddressIsCreatedWithCopiedDetails(
				() => declaration.DestinationWarehouseDocumentaryAddress,
				() => declaration.CreateOrUpdateDeliveryPlace(IE801PartyDeliveryPlaceProvider.NewOrNull(deliveryPlace)),
				"GBTID001",
				OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID,
				"GB",
				"London",
				"Street A 9",
				"2000",
				"MyComp");
		}

		public void TestCreateOrUpdateCarrierAgent_Null()
		{
			TestOldAddressIsDeletedAndNewAddressIsCreated(
				() => declaration.CarrierAgentDocumentaryAddress,
				() => declaration.CreateOrUpdateCarrierAgent(null),
				ZString.Empty);
		}

		public void TestCreateOrUpdateCarrierAgent_MatchesExistingVAT()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "Existing address with matching VAT";
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.FranceCodeTypes.TVA, Core.Constants.CountryCodes.France, "VAT001", orgAddress.PK);
			Factory.Save();

			var carrierAgent = new TransportArrangerTraderType { VatNumber = "FRVAT001" };

			TestOldAddressIsDeletedAndNewAddressIsCreated(
				() => declaration.CarrierAgentDocumentaryAddress,
				() => declaration.CreateOrUpdateCarrierAgent(IE801PartyTransportArrangerProvider.NewOrNull(carrierAgent)),
				"Existing address with matching VAT");
		}

		public void TestCreateOrUpdateCarrierAgent_NotMatchExisting()
		{
			var carrierAgent = new TransportArrangerTraderType
			{
				VatNumber = "ELVAT001",
				Language = "EN",
				City = "London",
				StreetName = "Street A",
				StreetNumber = "9",
				Postcode = "2000",
				TraderName = "MyComp"
			};

			TestOldAddressIsDeletedAndNewAddressIsCreatedWithCopiedDetails(
				() => declaration.CarrierAgentDocumentaryAddress,
				() => declaration.CreateOrUpdateCarrierAgent(IE801PartyTransportArrangerProvider.NewOrNull(carrierAgent)),
				"VAT001",
				OrgCusCode.GreeceCodeTypes.AFM,
				"GR",
				"London",
				"Street A 9",
				"2000",
				"MyComp");
		}

		public void TestCreateOrUpdateTransporter_Null()
		{
			TestOldAddressIsDeletedAndNewAddressIsCreated(
				() => declaration.TransporterDocumentaryAddress,
				() => declaration.CreateOrUpdateTransporter(null),
				ZString.Empty);
		}

		public void TestCreateOrUpdateTransporter_MatchesExistingVAT()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgAddress = orgHeader.MainAddress;
			orgAddress.Address1 = "Existing address with matching VAT";
			var cusCode = Factory.NewWithValidTestData<OrgCusCode>();
			cusCode.ModifyOrgCusCode(orgHeader.PK, OrgCusCode.FranceCodeTypes.TVA, Core.Constants.CountryCodes.France, "VAT001", orgAddress.PK);
			Factory.Save();

			var transporter = new FirstTransporterTraderType { VatNumber = "FRVAT001" };

			TestOldAddressIsDeletedAndNewAddressIsCreated(
				() => declaration.TransporterDocumentaryAddress,
				() => declaration.CreateOrUpdateTransporter(IE801PartyFirstTransporterProvider.NewOrNull(transporter)),
				"Existing address with matching VAT");
		}

		public void TestCreateOrUpdateTransporter_NotMatchExisting()
		{
			var transporter = new FirstTransporterTraderType
			{
				VatNumber = "ELVAT001",
				Language = "EN",
				City = "London",
				StreetName = "Street A",
				StreetNumber = "9",
				Postcode = "2000",
				TraderName = "MyComp"
			};

			TestOldAddressIsDeletedAndNewAddressIsCreatedWithCopiedDetails(
				() => declaration.TransporterDocumentaryAddress,
				() => declaration.CreateOrUpdateTransporter(IE801PartyFirstTransporterProvider.NewOrNull(transporter)),
				"VAT001",
				OrgCusCode.GreeceCodeTypes.AFM,
				"GR",
				"London",
				"Street A 9",
				"2000",
				"MyComp");
		}

		public void TestCreateOrUpdateTransportDetails()
		{
			var oldDetail1 = declaration.CusContainers.AddNew();
			var oldDetail2 = declaration.CusContainers.AddNew();
			var oldDetail3 = declaration.CusContainers.AddNew();

			declaration.CreateOrUpdateTransportDetails(new[]
			{
				new IE801TransportDetailsProvider(new TransportDetailsType { IdentityOfTransportUnits = "NEW_TRANSPORT_1" }),
				new IE801TransportDetailsProvider(new TransportDetailsType { IdentityOfTransportUnits = "NEW_TRANSPORT_2" })
			});

			CombineAssertions(() =>
			{
				AssertEquals("Old transport detail #1 deleted", true, oldDetail1.IsDeleted);
				AssertEquals("Old transport detail #2 deleted", true, oldDetail2.IsDeleted);
				AssertEquals("Old transport detail #3 deleted", true, oldDetail3.IsDeleted);
				AssertContainsExactElementsInAnyOrder("New transport details", new ZString[] { "NEW_TRANSPORT_1", "NEW_TRANSPORT_2" }, declaration.CusContainers.Cast<EMCSCusContainer>().Select(x => x.CO_ContainerNumber));
			});
		}

		public void TestCreateOrUpdateTransportDetails_Empty()
		{
			declaration.CusContainers.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("CusContainers count before CreateOrUpdateTransportDetails", 1, declaration.CusContainers.Count);
				declaration.CreateOrUpdateTransportDetails(Array.Empty<IE801TransportDetailsProvider>());
				AssertEquals("CusContainers count after CreateOrUpdateTransportDetails", 0, declaration.CusContainers.Count);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<EMCSJobDeclaration>();
			declaration.OwnerDocumentaryAddress.Address1 = "OwnerDocumentaryAddress Address1";
			declaration.DestinationWarehouseDocumentaryAddress.Address1 = "DestinationWarehouseDocumentaryAddress Address1";
			declaration.CarrierAgentDocumentaryAddress.Address1 = "CarrierAgentDocumentaryAddress Address1";
			declaration.TransporterDocumentaryAddress.Address1 = "TransporterDocumentaryAddress Address1";
		}

		void TestOldAddressIsDeletedAndNewAddressIsCreated(Func<JobDocAddress> getAddress, Action createOrUpdateAddress, ZString expectedNewAddress)
		{
			var oldAddress = getAddress.Invoke();
			createOrUpdateAddress.Invoke();
			var newAddress = getAddress.Invoke();

			CombineAssertions(() =>
			{
				AssertEquals("Old address is deleted", true, oldAddress.IsDeleted);
				AssertNotEquals("New address is created", newAddress.PK, oldAddress.PK);
				AssertEquals("New address", expectedNewAddress, newAddress.Address1);
			});
		}

		void TestOldAddressIsDeletedAndNewAddressIsCreatedWithCopiedDetails(
			Func<JobDocAddress> getAddress,
			Action createOrUpdateAddress,
			ZString expectedE2_GovRegNum,
			ZString expectedE2_GovRegNumType,
			ZString expectedE2_RN_NKCountryCode,
			ZString expectedE2_City,
			ZString expectedE2_Address1,
			ZString expectedE2_Postcode,
			ZString expectedE2_CompanyName)
		{
			var oldAddress = getAddress.Invoke();
			createOrUpdateAddress.Invoke();
			var newAddress = getAddress.Invoke();

			CombineAssertions(() =>
			{
				AssertEquals("Old address is deleted", true, oldAddress.IsDeleted);
				AssertNotEquals("New address is created", newAddress.PK, oldAddress.PK);
				AssertEquals("E2_AddressOverride", true, newAddress.E2_AddressOverride);
				AssertEquals("E2_GovRegNum", expectedE2_GovRegNum, newAddress.E2_GovRegNum);
				AssertEquals("E2_GovRegNumType", expectedE2_GovRegNumType, newAddress.E2_GovRegNumType);
				AssertEquals("E2_RN_NKCountryCode", expectedE2_RN_NKCountryCode, newAddress.E2_RN_NKCountryCode);
				AssertEquals("E2_City", expectedE2_City, newAddress.E2_City);
				AssertEquals("E2_Address1", expectedE2_Address1, newAddress.E2_Address1);
				AssertEquals("E2_Postcode", expectedE2_Postcode, newAddress.E2_Postcode);
				AssertEquals("E2_CompanyName", expectedE2_CompanyName, newAddress.E2_CompanyName);
			});
		}

		EMCSJobDeclaration declaration;
	}
}
