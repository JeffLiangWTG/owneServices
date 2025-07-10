using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class ImporterWrapperTest : Customs.Business.Testing.DataProviderTestCase<ImporterWrapper>
	{
		#region IdentificationNumber

		public void TestIdentificationNumber()
		{
			TestIdentificationNumberWhenImporterHasEORI();
			TestIdentificationNumberWhenImporterHasNoEORI();
		}

		void TestIdentificationNumberWhenImporterHasEORI()
		{
			AssertEquals("IdentificationNumber should equal importer EORI.", "FR12345678900001", GetAlternateProvider().IdentificationNumber);
		}

		void TestIdentificationNumberWhenImporterHasNoEORI()
		{
			AssertEquals("IdentificationNumber should not be mapped when importer has no EORI.", string.Empty, Provider.IdentificationNumber);
		}

		#endregion

		#region Name

		public void TestName()
		{
			TestNameWhenImporterHasEORI();
			TestNameWhenImporterHasNoEORI();
		}

		void TestNameWhenImporterHasEORI()
		{
			AssertEquals("Name not be mapped when importer has EORI.", null, GetAlternateProvider().Name);
		}

		void TestNameWhenImporterHasNoEORI()
		{
			AssertEquals("Name should equal importer OH_FullName.", "Name", Provider.Name);
		}

		#endregion

		#region Address

		public void TestAddress()
		{
			TestAddressWhenImporterHasEORI();
			TestAddressWhenImporterHasNoEORI();
		}

		void TestAddressWhenImporterHasEORI()
		{
			AssertNull("Address should not be mapped when importer has EORI", GetAlternateProvider().Address);
		}

		void TestAddressWhenImporterHasNoEORI()
		{
			var address = Provider.Address;
			AssertType<OrganisationAddressWrapper>("Address should be of type AddressWrapper.", address);
			AssertEquals("Postcode should be equal to importer.MainAddress.Postcode.", "24750", address.Postcode);
		}

		#endregion

		#region Setup

		protected override ImporterWrapper GetProvider()
		{
			var importer = GetImporter();
			return ImporterWrapper.New(importer, importer.MainAddress);
		}

		ImporterWrapper GetAlternateProvider()
		{
			var importer = GetImporter();
			importer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			importer.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			return ImporterWrapper.New(importer, importer.MainAddress);
		}

		OrgHeader GetImporter()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_FullName = "Name";
			importer.MainAddress.Postcode = "24750";
			return importer;
		}

		#endregion
	}
}
