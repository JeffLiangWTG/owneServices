using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class QuarantineNexDocNotificationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestNEXDOCMessageStatusList()
		{
			AssertSame(Factory.GetCachedValue<NEXDOCMessageStatus>(), notification.Lookups.NEXDOCMessageStatusList);
		}

		public void TestNEXDOCAcknowledgeStatusList()
		{
			AssertSame(Factory.GetCachedValue<NEXDOCAcknowledgeStatus>(), notification.Lookups.NEXDOCAcknowledgeStatusList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			notification = Factory.New<QuarantineNexDocNotification>();
		}
		QuarantineNexDocNotification notification;
	}
}
