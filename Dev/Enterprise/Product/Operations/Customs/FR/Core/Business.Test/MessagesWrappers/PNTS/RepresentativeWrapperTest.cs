using Enterprise.Customs.EU.Business.CusTempStorage;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.PNTS.Testing
{
	class RepresentativeWrapperTest : Customs.Business.Testing.DataProviderTestCase<RepresentativeWrapper>
	{
		public void TestStatus()
		{
			AssertEquals("Status should equal 3", (byte)3, Provider.Status);
			var storageHeader = SetUpTemporaryStorageHeader();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "DEF";
			orgHeader.OH_FullName = "ABCDE";
			orgAddress.OA_OH = orgHeader.PK;
			storageHeader.AMA_OA_Declarant = orgAddress.PK;
			var representativeWrapper = RepresentativeWrapper.New(storageHeader);
			AssertEquals("Status should equal 2", (byte)2, representativeWrapper.Status);
		}

		public void TestIdentificationNumber()
		{
			AssertEquals("IdentificationNumber should equal EORI from representative.", "GBDEF", Provider.IdentificationNumber);
			var storageHeader = SetUpTemporaryStorageHeader();
			storageHeader.Representative.Header.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", storageHeader.AMA_RN_NKCountry);
			storageHeader.Representative.Header.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", storageHeader.AMA_RN_NKCountry);
			var representativeWrapper = RepresentativeWrapper.New(storageHeader);
			AssertEquals("IdentificationNumber should equal EORI from representative address where country equals AMA_RN_NKCountry.", "FR12345678900001", representativeWrapper.IdentificationNumber);

			var storageHeader2 = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			orgHeader.OH_FullName = "ABCD";
			orgHeader.Addresses.MainAddress.Address1 = "ABCDE";
			orgAddress.OA_OH = orgHeader.PK;
			storageHeader2.AMA_OA_Representative = orgHeader.Addresses.MainAddress.PK;

			storageHeader2.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "DEF", Core.Constants.CountryCodes.UnitedKingdom);
			representativeWrapper = RepresentativeWrapper.New(storageHeader2);
			AssertEquals("IdentificationNumber should equal EORI from representative.", "GBDEF", Provider.IdentificationNumber);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", storageHeader.AMA_RN_NKCountry);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00002", storageHeader.AMA_RN_NKCountry);
			representativeWrapper = RepresentativeWrapper.New(storageHeader2);
			AssertEquals("IdentificationNumber should equal EORI from representative where country equals AMA_RN_NKCountry.", "FR12345678900002", representativeWrapper.IdentificationNumber);
		}

		public void TestName()
		{
			AssertEquals("Name should equal orgHeader.OH_FullName", "ABCD", Provider.Name);
		}

		public void TestCommunication()
		{
			AssertEquals("Identifier should equal contact2.OC_Phone", "123456", Provider.Communication.Identifier);
			AssertEquals("Type should equal TE", "TE", Provider.Communication.Type);
		}

		public void TestOrgHeader()
		{
			AssertEquals("OrgHeader.OH_Code should equal orgHeader.OH_Code", "ABC", Provider.OrgHeader.OH_Code);
		}

		protected override RepresentativeWrapper GetProvider()
		{
			return RepresentativeWrapper.New(SetUpTemporaryStorageHeader());
		}

		TemporaryStorageHeader SetUpTemporaryStorageHeader()
		{
			var storageHeader = Factory.NewWithValidTestData<TemporaryStorageHeader>();
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_Code = "ABC";
			orgHeader.OH_FullName = "ABCD";
			orgHeader.Addresses.MainAddress.Address1 = "ABCDE";
			orgAddress.OA_OH = orgHeader.PK;
			storageHeader.AMA_OA_Declarant = orgHeader.Addresses.MainAddress.PK;
			storageHeader.AMA_OA_Representative = orgHeader.Addresses.MainAddress.PK;

			storageHeader.AMA_RN_NKCountry = Core.Constants.CountryCodes.France;
			orgHeader.CustomsCodes.AddNew("EOR", "DEF", Core.Constants.CountryCodes.UnitedKingdom);

			var contact1 = orgHeader.Contacts.AddNew();
			contact1.OC_ContactName = "Test Contact 1";
			contact1.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.VAT;
			contact1.OC_Email = "abc@abc.com";

			var contact2 = orgHeader.Contacts.AddNew();
			contact2.OC_ContactName = "Test Contact 2";
			contact2.Allocations.AddNew().PC_Type = OrgConstants.ContactAllocationType.CUS;
			contact2.OC_Phone = "123456";

			return storageHeader;
		}
	}
}
