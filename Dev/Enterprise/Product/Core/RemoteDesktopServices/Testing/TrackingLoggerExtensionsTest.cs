using System.Collections.Generic;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server.TrackingInfo;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing.TrackingInfo
{
	class TrackingLoggerExtensionsTest : TestCase
	{
		public void TestLogDragDropEvents()
		{
			// Arrange
			var logs = new List<string>();

			TrackingInfoLogger.Instance.OnNewLog += Instance_OnNewLog;
			using (new DisposableAction(() => TrackingInfoLogger.Instance.OnNewLog -= Instance_OnNewLog))
			{
				var expectedLog = @"##################################
EN Event Raised.
Control Type: CT
Control Name: CN
##################################";

				// Act
				TrackingInfoLogger.Instance.LogDragDropEvents("EN", "CT", "CN");

				// Assert
				AssertEquals(expectedLog, string.Join(System.Environment.NewLine, logs));
			}

			void Instance_OnNewLog(object sender, string e)
			{
				logs.Add(e);
			}
		}
	}
}
