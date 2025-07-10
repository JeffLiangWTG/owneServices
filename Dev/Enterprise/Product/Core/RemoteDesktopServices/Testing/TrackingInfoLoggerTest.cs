using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class TrackingInfoLoggerTest : TestCase
	{
		public void TestNewLogWithMessageFunc_FunctionDoesNotGetRunWhenNoListener()
		{
			// Arrange
			var i = 0;

			// Act
			TrackingInfoLogger.Instance.NewLog(() => $"{++i}");

			// Assert
			AssertEquals(0, i);
		}

		public void TestNewLogWithMessageFunc_FunctionDoesGetRunWhenListenerAdded()
		{
			// Arrange
			TrackingInfoLogger.Instance.OnNewLog += Instance_OnNewLog;
			using (new DisposableAction(() => TrackingInfoLogger.Instance.OnNewLog -= Instance_OnNewLog))
			{
				var i = 0;

				// Act
				TrackingInfoLogger.Instance.NewLog(() => $"{++i}");

				// Assert
				AssertEquals(1, i);
			}

			void Instance_OnNewLog(object sender, string e)
			{
			}
		}

		public void TestHasListenerReturnsFalseIfNoListener()
		{
			AssertEquals(false, TrackingInfoLogger.Instance.HasListener);
		}

		public void TestHasListenerReturnsTrueIfAnyListener()
		{
			// Arrange
			TrackingInfoLogger.Instance.OnNewLog += Instance_OnNewLog;
			using (new DisposableAction(() => TrackingInfoLogger.Instance.OnNewLog -= Instance_OnNewLog))
			{
				// Act
				// Assert
				AssertEquals(true, TrackingInfoLogger.Instance.HasListener);
			}

			void Instance_OnNewLog(object sender, string e)
			{
			}
		}

		public void TestHasListenerReturnsFalseAfterRemovingListener()
		{
			// Arrange
			TrackingInfoLogger.Instance.OnNewLog += Instance_OnNewLog;
			TrackingInfoLogger.Instance.OnNewLog -= Instance_OnNewLog;

			// Act
			// Assert
			AssertEquals(false, TrackingInfoLogger.Instance.HasListener);

			void Instance_OnNewLog(object sender, string e)
			{
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			Assert("TrackingInfoLogger is not clean to use, please check Test Sequence File to find out pollution.", !TrackingInfoLogger.Instance.HasListener);
		}
	}
}
