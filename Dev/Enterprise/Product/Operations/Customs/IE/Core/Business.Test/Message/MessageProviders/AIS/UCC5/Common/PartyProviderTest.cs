using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	sealed class PartyProviderTest : DataProviderTestCase<PartyProvider>
	{
		public void TestNew()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null - OrgAddress", PartyProvider.New(null));
				AssertNull("Null - JobDocAddress", PartyProvider.New((JobDocAddress)null));
				AssertNotNull("Valid argument", Provider);
			});
		}
		public void TestNewWithIDNull_ReturnsNullWhenDocAddressIsNull()
		{
			AssertNull("Should return null with null docAddress", PartyProvider.NewWithIDNull(null));
		}

		public void TestNewWithIDNull_CreatesProviderWithCorrectValues()
		{
			var job = Factory.New<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG111", Core.Constants.CountryCodes.Greece);
			job.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			importer.MainAddress.OA_City = "TestCity";

			var provider = PartyProvider.NewWithIDNull(job.ImporterDocumentaryAddress);

			AssertNull("ID should be null", provider.ID);
			AssertEquals("Address", "TestCity", provider.Address.City);
		}

		public void TestID()
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG222", Core.Constants.CountryCodes.Ireland);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.BTW, "REG111", Core.Constants.CountryCodes.Ireland);
			AssertEquals("IEREG222", GetProvider().ID);
		}
				
		public void TestId_DocAddress()
		{
			var job = Factory.New<JobDeclaration>();
			job.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "REG111", Core.Constants.CountryCodes.Ireland);
			job.ImporterDocumentaryAddress.E2_OA_Address = importer.MainAddress.PK;
			job.ImporterDocumentaryAddress.E2_AddressOverride = true;
			job.ImporterDocumentaryAddress.E2_GovRegNum = "EOR001";

			var provider = PartyProvider.New(job.ImporterDocumentaryAddress);
			AssertEquals("Get ID from E2_GovRegNum when created from JobDocAddress with E2_AddressOverride", "EOR001", provider.ID);

			job.ImporterDocumentaryAddress.E2_AddressOverride = false;
			provider = PartyProvider.New(job.ImporterDocumentaryAddress);
			AssertEquals("Get ID from EORI when created from JobDocAddress without E2_AddressOverride", "IEREG111", provider.ID);
		}

		public void TestAddress()
		{
			orgHeader.MainAddress.OA_City = "TestCity";
			AssertEquals("Address", "TestCity", GetProvider().Address.City);
		}

		PartyProvider GetProvider(OrgAddress address) => PartyProvider.New(address);
		protected override PartyProvider GetProvider() => GetProvider(orgHeader.MainAddress);

		protected override void SetUp()
			{
			base.SetUp();

			orgHeader = Factory.New<OrgHeader>();
		}

		OrgHeader orgHeader;
	}
}
