using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	class TraderWrapperTest : Customs.Business.Testing.DataProviderTestCase<TraderWrapper>
	{
		public void TestNew_JobDocAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("No JobDocAddress", TraderWrapper.New((JobDocAddress)null, true, false));
				AssertNotNull("JobDocAddress", TraderWrapper.New(Factory.NewWithValidTestData<JobDocAddress>(), true, false));
				AssertNull("Invalid JobDocAddress", TraderWrapper.New(Factory.New<JobDocAddress>(), true, false));
			});
		}

		public void TestNew_OrgAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("No OrgAddress", TraderWrapper.New((OrgAddress)null, true, false));
				AssertNotNull("OrgAddress", TraderWrapper.New(Factory.NewWithValidTestData<OrgAddress>(), true, false));
			});
		}

		public void TestName()
		{
			AssertEquals("SPORTIVA MACCHINA INTERNATIONALE PT", wrapper.Name);
		}

		public void TestCompanyName()
		{
			AssertEquals("COMPANY NAME TEST LTD", wrapper.CompanyName);
		}

		public void TestStreetAndNumber()
		{
			AssertEquals("10234-119 BOULEVARD FELIX FAURE STR", wrapper.StreetAndNumber);

			wrapper = TraderWrapper.New(orgHeader.MainAddress, false, true);
			AssertEquals("10234-119 BOULEVARD FELIX FAURE STREET MORE INFO TEST SECOND LINE 4910", wrapper.StreetAndNumber);
		}

		public void TestPostalCode()
		{
			AssertEquals("00937-061", wrapper.PostalCode);
		}

		public void TestCity()
		{
			AssertEquals("SAINT-REMY-EN-BOUZEMONT-SAINT-GENES", wrapper.City);
		}

		public void TestCountryCode()
		{
			AssertEquals(Constants.CountryCodes.France, wrapper.CountryCode);
		}

		public void TestNameAndAddressLanguage()
		{
			AssertEquals("EN", wrapper.NameAndAddressLanguage);
		}

		public void TestHolderIDTIR()
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "R/ABC1234", Constants.CountryCodes.Latvia);
			AssertEquals(ZString.Empty, wrapper.HolderIDTIR);
		}

		public void TestHolderIDTIR_True()
		{
			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.TIR_TransportsInternationauxRoutiers, "R/ABCD1234567890", Constants.CountryCodes.Latvia);
			var tirWrapper = TraderWrapper.New(orgHeader.MainAddress, true, false);
			AssertEquals("LVR/ABCD123456789", tirWrapper.HolderIDTIR);
		}

		public void TestRepresentativeCapacity()
		{
			AssertEquals(ZString.Empty, wrapper.RepresentativeCapacity);
		}

		public void TestRepresentativeCapacityLanguage()
		{
			AssertEquals(ZString.Empty, wrapper.RepresentativeCapacityLanguage);
		}

		public void TestTIN()
		{
			AssertEquals("FR325820751000809", wrapper.TIN);
		}

		public void TestJobDocAddressOverride()
		{
			var jobDocAddress = Factory.New<JobDocAddress>();
			jobDocAddress.E2_AddressOverride = true;
			jobDocAddress.E2_CompanyName = "SPORTIVA MACCHINA INTERNATIONALE PTY LTD";
			jobDocAddress.E2_Address1AndE2_Address2 = "10234-119 BOULEVARD FELIX FAURE STREET MORE INFORMATION FOR STREET AND";
			jobDocAddress.E2_City = "SAINT-REMY-EN-BOUZEMONT-SAINT-GENEST-ET-ISSON";
			jobDocAddress.E2_Postcode = "00937-0616";
			jobDocAddress.E2_RN_NKCountryCode = Constants.CountryCodes.France;
			var jobDocAddressWrapper = TraderWrapper.New(jobDocAddress, false, false);
			CombineAssertions(() =>
			{
				AssertEquals("Name", "SPORTIVA MACCHINA INTERNATIONALE PT", jobDocAddressWrapper.Name);
				AssertEquals("Company Name", "SPORTIVA MACCHINA INTERNATIONALE PT", jobDocAddressWrapper.CompanyName);
				AssertEquals("Address", "10234-119 BOULEVARD FELIX FAURE STR", jobDocAddressWrapper.StreetAndNumber);
				AssertEquals("Postal Code", "00937-061", jobDocAddressWrapper.PostalCode);
				AssertEquals("City", "SAINT-REMY-EN-BOUZEMONT-SAINT-GENES", jobDocAddressWrapper.City);
				AssertEquals("Country Code", Constants.CountryCodes.France, jobDocAddressWrapper.CountryCode);

				var jobDocAddressWrapperWithExtendedAddress = TraderWrapper.New(jobDocAddress, false, true);
				AssertEquals("Address", "10234-119 BOULEVARD FELIX FAURE STREET MORE INFORMATION FOR STREET AND", jobDocAddressWrapperWithExtendedAddress.StreetAndNumber);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "SPORTIVA MACCHINA INTERNATIONALE PTY LTD";
			orgHeader.MainAddress.Address1 = "10234-119 BOULEVARD FELIX FAURE STREET MORE INFO";
			orgHeader.MainAddress.Address2 = "TEST SECOND LINE 49100 ANGERS";
			orgHeader.MainAddress.City = "SAINT-REMY-EN-BOUZEMONT-SAINT-GENEST-ET-ISSON";
			orgHeader.MainAddress.Postcode = "00937-0616";
			orgHeader.MainAddress.OA_RN_NKCountryCode = Constants.CountryCodes.France;
			orgHeader.MainAddress.CompanyName = "COMPANY NAME TEST LTD";
			eori = orgHeader.CustomsCodes.AddNew();
			eori.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
			eori.OK_RN_NKCodeCountry = Constants.CountryCodes.France;
			eori.OK_CustomsRegNo = "32582075100080999";
			wrapper = TraderWrapper.New(orgHeader.MainAddress, false, false);
		}

		TraderWrapper wrapper;
		OrgHeader orgHeader;
		OrgCusCode eori;

		protected override TraderWrapper GetProvider() => wrapper;
	}
}
