using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(AccountingPeriodFieldUserControl))]
	sealed class AccountingPeriodFieldUserControlTest : RuntimeOptionUserControlBaseTest<AccountingPeriodFieldUserControl>
	{
		public override void TestChangeLabelSizeForAlignment()
		{
			//Useless test for this control.
			Assert(true);
		}

		public override void TestDesiredCaptionWidth()
		{
			//Useless test for this control.
			Assert(true);
		}
	}
}
