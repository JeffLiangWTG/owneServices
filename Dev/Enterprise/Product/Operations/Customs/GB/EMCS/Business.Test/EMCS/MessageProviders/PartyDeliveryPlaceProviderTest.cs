using CargoWise.Types;
using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.EMCS.Business.Testing
{
	sealed class PartyDeliveryPlaceProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyDeliveryPlaceProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyDeliveryPlaceProvider.NewOrNull(null));
			AssertNull(PartyDeliveryPlaceProvider.NewOrNull(Factory.New<JobDocAddress>()));
			AssertNotNull(PartyDeliveryPlaceProvider.NewOrNull(Factory.NewWithValidTestData<JobDocAddress>()));
		}

		public void TestTraderId()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;

			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "TID123", Core.Constants.CountryCodes.UnitedKingdom);
			orgAddress.Address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "TID234", Core.Constants.CountryCodes.UnitedKingdom);

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(orgAddress);
			AssertEquals("TraderId", "TID234", deliveryPlaceTrader.TraderId);
		}

		public void TestTraderId_NotIECountry()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;

			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "TID123", Core.Constants.CountryCodes.Italy);
			orgAddress.Address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "TID234", Core.Constants.CountryCodes.Italy);

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(orgAddress);
			AssertEquals("TraderId", "TID234", deliveryPlaceTrader.TraderId);
		}

		public void TestTraderId_Override()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "TVA123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.FranceCodeTypes.TVA;
			jobDocAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(jobDocAddress);
			AssertEquals("VatId", "TVA123", deliveryPlaceTrader.TraderId);
		}

		public void TestTraderId_JE_MessageSubType1()
		{
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "TID123", Core.Constants.CountryCodes.UnitedKingdom);
			orgAddress.Address.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderID, "TID234", Core.Constants.CountryCodes.UnitedKingdom);
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(orgAddress);
			AssertEquals("TraderId", "TID234", deliveryPlaceTrader.TraderId);
		}

		public void TestTraderId_JE_MessageSubTypeNot1()
		{
			organisation.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.TVA, "TVA123", Core.Constants.CountryCodes.France);
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(orgAddress);
			AssertEquals("VatId", "TVA123", deliveryPlaceTrader.TraderId);
		}

		public void TestCity()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(jobDocAddress);
			AssertEquals("City", "MELBOURNE", deliveryPlaceTrader.City);
		}

		public void TestCity_Empty()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(orgAddress);
			AssertEquals("City", ZString.Empty, deliveryPlaceTrader.City);
		}

		public void TestAddress()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(jobDocAddress);
			AssertEquals("Address", "23 CROWN ST", deliveryPlaceTrader.Address);
		}

		public void TestAddress_Empty()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(orgAddress);
			AssertEquals("Address", ZString.Empty, deliveryPlaceTrader.Address);
		}

		public void TestPostcode()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(jobDocAddress);
			AssertEquals("Postcode", "3000", deliveryPlaceTrader.Postcode);
		}

		public void TestPostcode_Empty()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationTaxWarehouse;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(orgAddress);
			AssertEquals("Postcode", ZString.Empty, deliveryPlaceTrader.Postcode);
		}

		public void TestName()
		{
			jobDocAddress.E2_AddressOverride = true;
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationDirectDelivery;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(jobDocAddress);
			AssertEquals("Name", "TEST OVERRIDE COMPANY", deliveryPlaceTrader.Name);
		}

		public void TestName_Empty()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationDirectDelivery;

			var deliveryPlaceTrader = PartyDeliveryPlaceProvider.NewOrNull(orgAddress);
			AssertEquals("Name", ZString.Empty, deliveryPlaceTrader.Name);
		}

		protected override void SetUp()
		{
			base.SetUp();

			jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
			jobDocAddress.E2_City = "MELBOURNE";
			jobDocAddress.E2_Address1 = "23 CROWN ST";
			jobDocAddress.E2_Postcode = "3000";
			jobDocAddress.E2_CompanyName = "TEST OVERRIDE COMPANY";

			organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "JOB ADDRESS TRADER NAME";

			var address = organisation.MainAddress;
			address.OA_Language = Core.SharedConstants.Languages.Tamil;
			address.OA_City = "DARWIN";
			address.OA_Address1 = "12 MITCHELL ST";
			address.OA_PostCode = "0800";
			address.OA_RN_NKCountryCode = Core.Constants.CountryCodes.France;

			orgAddress = Factory.New<JobDocAddress>();
			orgAddress.E2_OA_Address = address.PK;

			jobDeclaration = Factory.New<EMCSJobDeclaration>();
			jobDeclaration.DocAddresses.Add(jobDocAddress);
			jobDeclaration.DocAddresses.Add(orgAddress);
		}
		EMCSJobDeclaration jobDeclaration;
		JobDocAddress jobDocAddress;
		JobDocAddress orgAddress;
		OrgHeader organisation;

		protected override PartyDeliveryPlaceProvider GetProvider() => PartyDeliveryPlaceProvider.NewOrNull(jobDocAddress);
	}
}
