
namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	sealed class IE026AccessCodeProviderTest : Customs.Business.Testing.DataProviderTestCase<IE026AccessCodeProvider>
	{
		public void TestCurrent()
		{
			AssertEquals("AB12", Provider.Current);
		}

		public void TestNew()
		{
			AssertEquals("CD34", Provider.New);
		}

		protected override IE026AccessCodeProvider GetProvider() => new IE026AccessCodeProvider(sendingObject);

		protected override void SetUp()
		{
			base.SetUp();
			sendingObject = new GuaranteeAccessCodesSendingAction(NctsTestDataProvider.CreateNctsGuaranteeWithCusGuaranteeHeader(Factory).cusGuaranteeHeader);
			sendingObject.CurrentCode = "AB12";
			sendingObject.NewAccessCode = "CD34";
		}

		GuaranteeAccessCodesSendingAction sendingObject;
	}
}
