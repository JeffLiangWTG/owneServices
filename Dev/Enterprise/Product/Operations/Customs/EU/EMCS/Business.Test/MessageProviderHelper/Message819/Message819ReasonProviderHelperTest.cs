using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class Message819ReasonProviderHelperTest : TestCaseWithFactory
	{
		public void TestReasonCode()
		{
			AssertEquals("1", helper.ReasonCode);
		}

		protected override void SetUp()
		{
			base.SetUp();
			alertOrRejectReason = new AlertOrRejectReason(Factory);
			alertOrRejectReason.Reason = "1";
			helper = new Message819ReasonProviderHelper(alertOrRejectReason);
		}
		AlertOrRejectReason alertOrRejectReason;
		Message819ReasonProviderHelper helper;
	}
}
