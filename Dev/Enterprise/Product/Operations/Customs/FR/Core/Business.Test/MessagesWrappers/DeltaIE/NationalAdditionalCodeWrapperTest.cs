using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE.Testing
{
	class NationalAdditionalCodeWrapperTest : DataProviderTestCase<NationalAdditionalCodeWrapper>
	{
		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should be equal to Core.Constants.CountryCodes.France as customs office is empty.", Core.Constants.CountryCodes.France, Provider.CcQualifier);

			var code = "V905";

			var wrapper = NationalAdditionalCodeWrapper.New(code, Core.Constants.CountryCodes.France);
			AssertEquals("CcQualifier should be empty as customsOffice starts with FR", string.Empty, wrapper.CcQualifier);

			wrapper = NationalAdditionalCodeWrapper.New(code, Core.Constants.CountryCodes.Ukraine);
			AssertEquals("CcQualifier should be equal to FR as customsOffice doesn't starts with FR", Core.Constants.CountryCodes.France, wrapper.CcQualifier);
		}

		public void TestNationalAdditionalCode()
		{
			AssertEquals("NationalAdditionalCode should equal code passed as parameter.", "V905", Provider.NationalAdditionalCode);
		}

		protected override NationalAdditionalCodeWrapper GetProvider()
		{
			var code = "V905";
			return NationalAdditionalCodeWrapper.New(code, string.Empty);
		}
	}
}
