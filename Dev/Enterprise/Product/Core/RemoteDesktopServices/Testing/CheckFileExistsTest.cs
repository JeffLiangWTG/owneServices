using System;
using System.IO;
using CargoWise.Common;
using Enterprise.RemoteDesktopServices.Server;
using NUnit.Framework;

namespace Enterprise.RemoteDesktopServices.Testing
{
	class CheckFileExistsTest : RemoteDesktopServicesTest
	{
		public void TestCheckFileExists()
		{
			string filePath = TempForTest.GetTempFileName();
			try
			{
				var doesFileExists = EnterpriseChannel.Instance.SendMessage<string, bool>(EnterpriseChannelMessageTypes.CheckFileExists, filePath);
				Assert(string.Format("File {0} exists", filePath), doesFileExists);

				doesFileExists = EnterpriseChannel.Instance.SendMessage<string, bool>(EnterpriseChannelMessageTypes.CheckFileExists, filePath + "1");
				Assert("File does not exist", !doesFileExists);
			}
			finally
			{
				try
				{
					File.Delete(filePath);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
				}
			}
		}

		[ExpectNoExceptions]
		public void TestCheckFileStreamClose()
		{
			byte[] message = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			api.CloseFileHandle();
			EnterpriseChannel.Instance.Send(message);
			EnterpriseChannel.Instance.OnDisconnect(); // Forces any on going reads to be cancelled.
		}

		public void TestFilePathAccessIsDenied()
		{
			byte[] message = { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
			MockWtsApi api = (MockWtsApi)WtsApi.Instance;
			api.MakeFileHandleInaccessible();
			AssertExceptionThrown<OperationCanceledException>("Should throw the exception", () =>
				{
					EnterpriseChannel.Instance.Send(message);
					AssertNull(CargoWise.Common.ErrorReporter.LastMessageReported);
				});
			EnterpriseChannel.Instance.OnDisconnect(); // Forces any on going reads to be cancelled.
		}
	}
}
