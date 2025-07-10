using Enterprise.DataTransfer.Common;
using Enterprise.DataTransfer.Native.Common.Logging;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Responses
{
	public class LogBufferExtensionTest : TestCase
	{
		public void TestNotificationsStatus()
		{
			var logger = new MemoryLogger();
			logger.Clear();
			AssertEquals(NativeResponseStatus.Accepted, logger.Buffer.GetStatus());

			logger.Information("Some Info");
			AssertEquals(NativeResponseStatus.Accepted, logger.Buffer.GetStatus());

			logger.Warning("Some Warning");
			AssertEquals(NativeResponseStatus.Warning, logger.Buffer.GetStatus());

			logger.Error("Some Error");
			AssertEquals(NativeResponseStatus.Rejected, logger.Buffer.GetStatus());
		}
	}
}
