using Enterprise.Customs.FR.Business.MessagesWrappers.DeltaIE;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.FR.Business.Testing.MessagesWrappers.DeltaIE.Testing
{
	class AdditionalReferenceByCusAccountWrapperTest : Customs.Business.Testing.DataProviderTestCase<AdditionalReferenceByCusAccountWrapper>
	{
		protected override AdditionalReferenceByCusAccountWrapper GetProvider()
		{
			var cusAccountReference = Factory.New<OrgCusAccount>();
			cusAccountReference.CZ_Code = "DEC";
			cusAccountReference.CZ_Account = "DEC111";
			return AdditionalReferenceByCusAccountWrapper.New(cusAccountReference, string.Empty);
		}

		public void TestCcQualifier()
		{
			AssertEquals("CcQualifier should be equal to Core.Constants.CountryCodes.France.", Core.Constants.CountryCodes.France, Provider.CcQualifier);

			var cusAccountReference = Factory.New<OrgCusAccount>();
			cusAccountReference.CZ_Code = "DEC";
			cusAccountReference.CZ_Account = "DEC111";
			var wrapper = AdditionalReferenceByCusAccountWrapper.New(cusAccountReference, Core.Constants.CountryCodes.France);
			AssertEquals("CcQualifier should be empty as customs office starts with FR.", string.Empty, wrapper.CcQualifier);

			wrapper = AdditionalReferenceByCusAccountWrapper.New(cusAccountReference, Core.Constants.CountryCodes.Ukraine);
			AssertEquals("CcQualifier should be equal to FR as customs office doesn't start with FR.", Core.Constants.CountryCodes.France, wrapper.CcQualifier);
		}

		public void TestType()
		{
			AssertEquals("Text should be equal to 1DEC.", "1DEC", Provider.Type);
		}

		public void TestReferenceNumber()
		{
			AssertEquals("Code should be equal to CZ_Account.", "DEC111", Provider.ReferenceNumber);
		}
	}
}
