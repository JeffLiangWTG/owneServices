using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class ExporterWrapperTest : Customs.Business.Testing.DataProviderTestCase<ExporterWrapper>
	{
		#region IdentificationNumber

		public void TestIdentificationNumber()
		{
			TestIdentificationNumberWhenExporterHasEORI();
			TestIdentificationNumberWhenExporterHasNoEORI();
		}

		void TestIdentificationNumberWhenExporterHasEORI()
		{
			AssertEquals("IdentificationNumber should equal exporter EORI.", "FR12345678900001", GetAlternateProvider().IdentificationNumber);
		}

		void TestIdentificationNumberWhenExporterHasNoEORI()
		{
			AssertEquals("IdentificationNumber should not be mapped when exporter has no EORI.", string.Empty, Provider.IdentificationNumber);
		}

		#endregion

		#region Name

		public void TestName()
		{
			TestNameWhenExporterHasEORI();
			TestNameWhenExporterHasNoEORI();
		}

		void TestNameWhenExporterHasEORI()
		{
			AssertEquals("Name not be mapped when exporter has EORI.", null, GetAlternateProvider().Name);
		}

		void TestNameWhenExporterHasNoEORI()
		{
			AssertEquals("Name should equal exporter OH_FullName.", "Name", Provider.Name);
		}

		#endregion

		#region Address

		public void TestAddress()
		{
			TestAddressWhenExporterHasEORI();
			TestAddressWhenExporterHasNoEORI();
		}

		void TestAddressWhenExporterHasEORI()
		{
			AssertNull("Address should not be mapped when exporter has EORI", GetAlternateProvider().Address);
		}

		void TestAddressWhenExporterHasNoEORI()
		{
			var address = Provider.Address;
			AssertType<OrganisationAddressWrapper>("Address should be of type AddressWrapper.", address);
			AssertEquals("Postcode should be equal to exporter.MainAddress.Postcode.", "24750", address.Postcode);
		}

		#endregion

		#region Setup

		protected override ExporterWrapper GetProvider()
		{
			var exporter = GetExporter();

			return ExporterWrapper.New(exporter.MainAddress);
		}

		ExporterWrapper GetAlternateProvider()
		{
			var exporter = GetExporter();
			exporter.MainAddress.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789", Core.Constants.CountryCodes.France);
			exporter.MainAddress.CustomsCodes.AddNew(OrgCusCode.FranceCodeTypes.EoriBranchSuffix, "00001", Core.Constants.CountryCodes.France);
			return ExporterWrapper.New(exporter.MainAddress);
		}

		OrgHeader GetExporter()
		{
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "Name";
			exporter.MainAddress.Postcode = "24750";
			return exporter;
		}

		#endregion
	}
}
