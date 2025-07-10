using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.Environment;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class MultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestResetToOriginalWarning()
		{
			var manager = new MultiMessageManagerForTest(null);
			AssertEquals(@"Warning - Resetting a message to original is almost never correct unless you have withdrawn the entry using the CI. Any reset to original will NOT ever work if the CI has not been used.
Do not reset messages because of a message problem or because you wish to try again - without using the CI to remove/withdraw/delete any current declaration or report.
If you reset to original incorrectly the system will not work correctly - cargo may be delayed, storage incurred or the system may become unusable for this entry / report.", manager.ResetToOriginalWarning);
		}

		public void TestShouldSendMessagesInTestMode()
		{
			Env.Registry.CMRTestMode = false;
			var manager = new MultiMessageManagerForTest(null);
			Assert(!manager.ShouldSendMessagesInTestMode);
			Env.Registry.CMRTestMode = true;
			Assert(manager.ShouldSendMessagesInTestMode);
		}

		sealed class MultiMessageManagerForTest : MultiMessageManager
		{
			public MultiMessageManagerForTest(IMessageManageableBizObj topLevel)
				: base()
			{
				this.topLevel = topLevel;
			}

			readonly IMessageManageableBizObj topLevel;

			public override IMessageManageableBizObj TopLevelBizObjToManage => topLevel;

			protected override bool SendWheneverPossibleOnceMessagingActive => false;

			protected override SingleMessageManager[] GetAllMessageManagers() => null;

			internal new string ResetToOriginalWarning => base.ResetToOriginalWarning;
		}
	}
}
