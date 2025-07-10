using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.EU.EMCS.Business.Testing
{
	sealed class AlertOrRejectReasonLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestReasonList()
		{
			CombineAssertions(() =>
			{
				var reasonList = lookups.ReasonList;
				AssertEquals("List contents", "0, 1, 2, 3", reasonList.CodesAsString);
				AssertSame("cached", Factory.GetCachedValue<EMCSAlertRejectionCodeList>(), reasonList);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			lookups = new AlertOrRejectReasonLookups(new AlertOrRejectReason(Factory));
		}
		AlertOrRejectReasonLookups lookups;
	}
}
