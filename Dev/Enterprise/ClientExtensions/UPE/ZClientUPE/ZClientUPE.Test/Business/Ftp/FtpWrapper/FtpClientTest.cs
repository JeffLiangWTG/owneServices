using System;
using System.IO;
using Enterprise.Environment;
using NUnit.Framework;
using Renci.SshNet;

namespace Enterprise.Client.UPE.Business.Ftp.Testing
{
	internal class FtpClientTest : TestCase
	{
		#region Exceptions Test
		public void TestConnectExceptionIsCaught()
		{
			FtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.Always;
			try
			{
				FtpClient.Connect();
				Fail("Exception should be thrown here");
			}
			catch (Exception ex)
			{
				AssertEquals("A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond", ex.Message);
			}
		}

		public void TestGetFileExceptionIsCaught()
		{
			FtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.Always;
			try
			{
				FtpClient.DownloadFile("", new MemoryStream());
				Fail("Exception should be thrown here");
			}
			catch (Exception ex)
			{
				AssertEquals("get", ex.Message);
			}
		}

		public void TestPutFileExceptionIsCaught()
		{
			FtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.Always;
			try
			{
				FtpClient.UploadFile(new MemoryStream(), "", true);
				Fail("Exception should be thrown here");
			}
			catch (Exception ex)
			{
				AssertEquals("put", ex.Message);
			}
		}

		public void TestRenameExceptionIsCaught()
		{
			FtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.Always;
			try
			{
				FtpClient.RenameFile("old", "new");
				Fail("Exception should be thrown here");
			}
			catch (Exception ex)
			{
				AssertEquals("rename", ex.Message);
			}
		}

		public void TestDeleteFileExceptionIsCaught()
		{
			FtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.Always;
			try
			{
				FtpClient.Delete("deletefile");
				Fail("Exception should be thrown here");
			}
			catch (Exception ex)
			{
				AssertEquals("delete", ex.Message);
			}
		}

		public void TestChangeDirectoryExceptionIsCaught()
		{
			FtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.Always;
			try
			{
				FtpClient.ChangeDirectory("newdir");
				Fail("Exception should be thrown here");
			}
			catch (Exception ex)
			{
				AssertEquals("Permission denied", ex.Message);
			}
		}

		public void TestCheckExistsExceptionIsCaught()
		{
			FtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.Always;
			try
			{
				FtpClient.Exists("newdir");
				Fail("Exception should be thrown here");
			}
			catch (Exception ex)
			{
				AssertEquals("checkexists", ex.Message);
			}
		}

		[ExpectNoExceptions()]
		public void TestCannotRun64Bit()
		{
			FtpClient.Connect();
		}

		#endregion
		public void TestCallingTheRightMethods()
		{
			FtpClient.ToBeThrown = FtpClientForTest.ExceptionToBeThrown.None;
			FtpClient.Connect();
			AssertEquals("Connected", FtpClient.LastLogEntry);
			var tempFile = Env.GetTempFileName();
			using (var file = File.OpenRead(tempFile))
			{
				FtpClient.DownloadFile("remotefile", file);
				AssertEquals(string.Format("Downloaded remotefile to {0}", tempFile), FtpClient.LastLogEntry);
			}

			using (var file = File.OpenRead(tempFile))
			{
				FtpClient.UploadFile(file, "remotedir/remotefile", true);
				AssertEquals(string.Format("Uploaded remotedir/remotefile from {0}", tempFile), FtpClient.LastLogEntry);
			}

			FtpClient.RenameFile("old", "new");
			AssertEquals("Renamed old to new", FtpClient.LastLogEntry);
			FtpClient.Delete("tobedeleted");
			AssertEquals("Deleted tobedeleted", FtpClient.LastLogEntry);
			FtpClient.Exists("haha you goober");
			AssertEquals("haha you goober not exists", FtpClient.LastLogEntry);
			File.Delete(tempFile);
		}

		public void TestCallingTheRightProperties()
		{
			FtpClient.TestIsConnected = true;
			FtpClient.ChangeDirectory("test");
			AssertEquals(true, FtpClient.IsConnected);
			AssertEquals("test", FtpClient.WorkingDirectory);
			AssertEquals("Current working directory is set to test", FtpClient.LastLogEntry);
			FtpClient.TestIsConnected = false;
			AssertEquals(false, FtpClient.IsConnected);
		}

		#region Implementation
		FtpClientForTest FtpClient
		{
			get
			{
				if (fFtpClient == null)
				{
					fFtpClient = new FtpClientForTest(new PasswordConnectionInfo("jay.li.com", "jay", "li"));
				}

				return fFtpClient;
			}
		}

		FtpClientForTest fFtpClient;
		#endregion
	}
}
