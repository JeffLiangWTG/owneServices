using Enterprise.Customs.EU.EMCS.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.EMCS.Business.Testing
{
	sealed class PartyConsigneeProviderTest : Customs.Business.Testing.DataProviderTestCase<PartyConsigneeProvider>
	{
		public void TestNew()
		{
			AssertNull(PartyConsigneeProvider.NewOrNull(null));
			AssertNull(PartyConsigneeProvider.NewOrNull(Factory.New<JobDocAddress>()));
			AssertNotNull(PartyConsigneeProvider.NewOrNull(Factory.NewWithValidTestData<JobDocAddress>()));
		}

		public void TestTraderId()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TEN123", Core.Constants.CountryCodes.Ireland);

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(orgAddress);
			AssertEquals("Trader Excise Number", "TEN123", consigneeTrader.TraderId);
		}

		public void TestTraderId_NotIECountry()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TEN123", Core.Constants.CountryCodes.Italy);

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(orgAddress);
			AssertEquals("Trader Excise Number", "TEN123", consigneeTrader.TraderId);
		}

		public void TestTraderIdEmptyDestinationExemptedConsignee()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TEN123", Core.Constants.CountryCodes.Ireland);

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(orgAddress);
			AssertEquals("Trader Excise Number", string.Empty, consigneeTrader.TraderId);
		}

		public void TestTraderIdEmptyUnknownDestinationConsigneeUnknown()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber, "TEN123", Core.Constants.CountryCodes.Ireland);

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(orgAddress);
			AssertEquals("Trader Excise Number", string.Empty, consigneeTrader.TraderId);
		}

		public void TestTraderId_Override()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "TEN123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(jobDocAddress);
			AssertEquals("Trader Excise Number", "TEN123", consigneeTrader.TraderId);
		}

		public void TestTraderId_Override_EmptyDestinationExemptedConsignee()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "TEN123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(jobDocAddress);
			AssertEquals("Trader Excise Number", string.Empty, consigneeTrader.TraderId);
		}

		public void TestTraderId_Override_EmptyUnknownDestinationConsigneeUnknown()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.UnknownDestinationConsigneeUnknown;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "TEN123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(jobDocAddress);
			AssertEquals("Trader Excise Number", string.Empty, consigneeTrader.TraderId);
		}

		public void TestEoriNumber()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR123", Core.Constants.CountryCodes.Greece);

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(orgAddress);
			AssertEquals("EoriNumber", "GREOR123", consigneeTrader.EoriNumber);
		}

		public void TestEoriNumberEmptyWhenNoExport()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
			organisation.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "EOR123", Core.Constants.CountryCodes.Greece);

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(orgAddress);
			AssertEquals("EoriNumber", string.Empty, consigneeTrader.EoriNumber);
		}

		public void TestEoriNumber_Override()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExport;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "EOR123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(jobDocAddress);
			AssertEquals("EoriNumber", "EOR123", consigneeTrader.EoriNumber);
		}

		public void TestEoriNumber_OverrideEmptyWhenNoExport()
		{
			jobDeclaration.JE_MessageSubType = EMCSDestinationTypeList.Codes.DestinationExemptedConsignee;
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_GovRegNum = "EOR123";
			jobDocAddress.E2_GovRegNumType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;

			var consigneeTrader = PartyConsigneeProvider.NewOrNull(jobDocAddress);
			AssertEquals("EoriNumber", string.Empty, consigneeTrader.EoriNumber);
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

		protected override PartyConsigneeProvider GetProvider() => PartyConsigneeProvider.NewOrNull(jobDocAddress);
	}
}
