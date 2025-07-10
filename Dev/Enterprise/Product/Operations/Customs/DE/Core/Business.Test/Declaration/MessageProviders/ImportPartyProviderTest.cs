using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class ImportPartyProviderTest : Customs.Business.Testing.DataProviderTestCase<ImportPartyProvider>
	{
		public void TestConstructor_OrgAddress()
		{
			CombineAssertions(() =>
			{
				AssertNull("Null", ImportPartyProvider.NewOrNull(null));
				AssertNotNull("Not Null", ImportPartyProvider.NewOrNull(orgAddress));
			});
		}

		public void TestIdentification_HasEORI()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			AssertNotNull(Provider.Identification);
		}

		public void TestIdentification_HasNoEORI()
		{
			orgHeader.DeleteSingleEORINumber();
			AssertNull(Provider.Identification);
		}

		public void TestIdentification_HasNoEORIForEUCountry()
		{
			orgHeader.DeleteSingleEORINumber();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.UnitedKingdom);
			AssertNull(Provider.Identification);
		}

		public void TestIdentification_Has2EORINumbersForEUAndNotEUCounty()
		{
			CombineAssertions(() =>
			{
				TestHelper.CreateCL010CoutryList(Factory);
				orgHeader.DeleteSingleEORINumber();
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.UnitedKingdom);
				orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654", Core.Constants.CountryCodes.France);
				AssertNotNull(Provider.Identification);
				AssertEquals("EORI Number", "FR987654", Provider.Identification.EoriNumber);
			});
		}

		public void TestAddress_HasEORI()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			AssertNull(Provider.Address);
		}

		public void TestAddress_HasNoEORI()
		{
			orgHeader.DeleteSingleEORINumber();
			AssertNotNull(Provider.Address);
		}

		public void TestAddress_HasNoEORIForEUCounty()
		{
			orgHeader.DeleteSingleEORINumber();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.UnitedKingdom);
			AssertNotNull(Provider.Address);
		}

		public void TestAddress_Has2EORINumbersForEUAndNotEUCounty()
		{
			TestHelper.CreateCL010CoutryList(Factory);
			orgHeader.DeleteSingleEORINumber();
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456", Core.Constants.CountryCodes.UnitedKingdom);
			orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654", Core.Constants.CountryCodes.France);
			AssertNull(Provider.Address);
		}

		public void TestTaxNumber()
		{
			orgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Greece;
			CombineAssertions(() =>
			{
				AssertEquals("No VAT Registration Number", string.Empty, Provider.TaxNumber);

				orgHeader.CustomsCodes.AddNew(OrgCusCode.GreeceCodeTypes.AFM, "123456", Core.Constants.CountryCodes.Greece);
				AssertEquals("Has VAT Registration Number", "EL123456", Provider.TaxNumber);
			});
		}

		protected override ImportPartyProvider GetProvider() => ImportPartyProvider.NewOrNull(orgAddress);

		protected override void SetUp()
		{
			base.SetUp();

			orgHeader = Factory.GetOrgHeaderWithEori("OHTEST", "123456789", Core.Constants.CountryCodes.Greece);
		}

		OrgHeader orgHeader;
		OrgAddress orgAddress => orgHeader.MainAddress;

		new IImportParty Provider => base.Provider;
	}
}
