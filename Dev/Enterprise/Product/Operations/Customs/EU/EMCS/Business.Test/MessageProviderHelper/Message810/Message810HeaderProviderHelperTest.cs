using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message810HeaderProviderHelperTest : TestCaseWithFactory
	{
		public void TestCancellationReasonCode()
		{
			AssertEquals("Empty string defaults to Zero", 0, helper.CancellationReasonCode);
			cancellationOfEad.Reason = "B";
			AssertEquals("Character defaults to Zero", 0, helper.CancellationReasonCode);
			cancellationOfEad.Reason = "2";
			AssertEquals("Numeric is parsed", 2, helper.CancellationReasonCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			emcsDeclaration = Factory.New<EMCSJobDeclaration>();
			cancellationOfEad = new CancellationSendingAction(emcsDeclaration);
			helper = new Message810HeaderProviderHelper(emcsDeclaration, cancellationOfEad);
		}
		EMCSJobDeclaration emcsDeclaration;
		CancellationSendingAction cancellationOfEad;
		Message810HeaderProviderHelper helper;
	}
}
