using CargoWise.ComponentModel;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ZNotificationsPropertyDescriptorTest : TestCaseWithDummy
	{
		public void TestGetNotificationMetaData()
		{
			using (Dummy.SuspendValidationTesting())
			{
				Dummy.Z0_VarCharMaxInfo.AddError("error");
				AssertEquals(true, MetaData.GetNotifications(Dummy, Dummy.Z0_VarCharMaxInfo.PropertyDescriptor).HasErrors());
			}
		}
	}
}
