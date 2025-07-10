using System.IO;
using System.Threading;
using CargoWise.IO;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Core.Testing
{
	sealed class TempFileWithDelayedDeleteTest : TempFileTest
	{
		[ExpectNoExceptions]
		public void TestFailsSilentlyIfNonIOExceptionOccurs()
		{
			TempFileWithDelayedDelete tempFile = null;
			using (tempFile = TempFileWithDelayedDelete.New())
			{
				tempFile.ThrowNonIOExceptionOnDelete = true;
			}
		}

		public void TestTempFileWaitsSpecifiedDelay()
		{
			using (TempFileWithDelayedDelete tempFile = TempFileWithDelayedDelete.NewWithFilename(Temp.GetTempFileName(), 3000))
			{
				AssertEquals("TempFile should exist", true, File.Exists(tempFile.Filename));
				tempFile.Dispose();
				Thread.Sleep(1000);
				AssertEquals("TempFile should still exist after the default wait time has elapsed", true, File.Exists(tempFile.Filename));
				Thread.Sleep(2200); // give a bit of extra time for filesystem to delete
				AssertEquals("TempFile should have been deleted after specified time", false, File.Exists(tempFile.Filename));
			}
		}

		public void TestTimerStopsIfUnableToDeleteWithin15Seconds()
		{
			using (new TempFileWithDelayedDelete.TimeoutOverride())
			{
				string fileName = "";
				FileStream stream = null;
				try
				{
					TempFileWithDelayedDelete tempFile;
					using (tempFile = TempFileWithDelayedDelete.New())
					{
						fileName = tempFile.ToString();
						stream = File.OpenRead(fileName);
					}

					Thread.Sleep(5000); // timer is cut from 15 to 3 secs (+1 for delay) for this test
					AssertEquals("Precondition - File should not have been deleted as it was locked.", true, File.Exists(fileName));
					AssertEquals("File could not be deleted within 10 seconds, the timer should have been disposed.", true, tempFile.TimerDisposedAfter15Seconds);
				}
				finally
				{
					if (stream != null)
					{
						stream.Close();
						File.Delete(fileName);
					}
				}
			}
		}

		[ExpectNoExceptions("this test shouldn't fail because the temp file listener should ignore any temp file with delayed delete")]
		public void TestTempFilesTestListenerShouldIgnoreTheseFiles()
		{
			TempFileWithDelayedDelete tempFile = TempFileWithDelayedDelete.New();
			tempFile.Dispose();
			// note: no sleep() at the end of this test
		}

		#region Implementation

		protected override void SleepForTestFileCreatedAndDeleted()
		{
			Thread.Sleep(3000);
		}

		protected override TempFile GetNewTempFile()
		{
			return TempFileWithDelayedDelete.New();
		}

		#endregion
	}
}
