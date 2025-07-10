using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;

namespace Enterprise.Customs.IE.Business.AES.Testing
{
	class OriginProviderTest : DataProviderTestCase<OriginProvider>
	{
		public void TestOriginCountry()
		{
			invoiceLine.JI_CountryOfOrigin = Core.Constants.CountryCodes.UnitedKingdom;
			AssertEquals("OriginCountry", Core.Constants.CountryCodes.UnitedKingdom, Provider.OriginCountry);
		}

		#region Overridings & inherits

		protected override OriginProvider GetProvider() => new OriginProvider(invoiceLine);

		protected override void SetUp()
		{
			base.SetUp();
			invoiceLine = MessageProviderTestHelper.SetupBasicTestBizObjs(Factory).entryLineWrapper.RandomInvoiceLine;
		}
		JobComInvoiceLine invoiceLine;

		#endregion
	}
}
