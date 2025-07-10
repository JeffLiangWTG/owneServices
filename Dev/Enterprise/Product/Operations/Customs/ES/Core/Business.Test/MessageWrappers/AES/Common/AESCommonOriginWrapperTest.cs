using System;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class AESCommonOriginWrapperTest : WrapperHelperTest<AESCommonOriginWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown("Constructor Throws Exception if invoiceLine is null", typeof(ArgumentNullException),
				ArgumentExceptionMessageHelper.GetArgumentExceptionMessage("Value cannot be null.","invoiceLine"), () => new AESCommonOriginWrapper(null));
		}

		public void TestCountryOfOrigin()
		{
			invoiceLine.JI_CountryOfOrigin = "ES";
			AssertEquals("Expected filled CountryOfOrigin", "ES", wrapper.CountryOfOrigin);
		}

		public void TestStateOfOrigin()
		{
			invoiceLine.JI_StateOrRegionOfOrigin = "28";
			AssertEquals("Expected filled StateOfOrigin", "28", wrapper.StateOfOrigin);
		}

		protected override void SetUp()
		{
			base.SetUp();

			invoiceLine = Factory.New<JobComInvoiceLine>();

			wrapper = new AESCommonOriginWrapper(invoiceLine);
		}

		JobComInvoiceLine invoiceLine;
		AESCommonOriginWrapper wrapper;

		protected override AESCommonOriginWrapper GetProvider() => wrapper;
	}
}
