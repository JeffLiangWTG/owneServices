using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class NotificationToILoggerConversionTest : TestCaseWithFactory
	{
		public void TestNotificationTypes()
		{
			AssertConvert(LogType.Information, new InfoNotification("Fish").Type);
			AssertConvert(LogType.Debug, new VerboseInfoNotification("Fish").Type);
			AssertConvert(LogType.Error, CargoWise.EntityFramework.NotificationType.MessageError);
		}

		void AssertConvert(LogType expected, INotificationType input)
		{
			AssertEquals(expected, input.ToLogType());
		}
	}
}
