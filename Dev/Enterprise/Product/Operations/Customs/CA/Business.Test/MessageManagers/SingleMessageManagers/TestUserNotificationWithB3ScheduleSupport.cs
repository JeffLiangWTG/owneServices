namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	public class TestUserNotificationWithB3ScheduleSupport : TestMessageInstructionUserNotification
	{
		public TestUserNotificationWithB3ScheduleSupport(bool defaultScheduleB3 = true)
			: base()
		{
			this.shouldScheduleB3 = defaultScheduleB3;
		}
		public bool shouldScheduleB3;
	}
}
