using System.IO;
using System.Net;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MailManager.FileDownload
{
	sealed class DownloadDataTest : TestCaseWithFactory
	{
		public void TestDownloadData()
		{
			byte[] data = GetTestDataArray();

			string fname = Path.Combine(Env.TempPath, "down.dat" + WebFileDownloader.TempFileExtension);
			DownloadDataTestHelper down = this.GetDownloadData(data, fname);
			AssertEquals("Part file does not exist", false, File.Exists(fname));
			AssertEquals("File does not exist", false, File.Exists(FileName));
			down.DoDownload();

			AssertEquals("Download Finished", true, HasFinishedSet());
			AssertEquals("Download Completed", true, down.Completed);

			AssertEquals("File exists", true, File.Exists(FileName));
			AssertEquals("Correct file length", data.Length, new FileInfo(FileName).Length);

			using (FileStream downFile = File.OpenRead(FileName))
			{
				byte[] downData = new byte[downFile.Length];
				downFile.Read(downData, 0, (int)downFile.Length);
				AssertEquals("Correct Data", data, downData);
			}
		}

		public void TestDownloadDataCancelled()
		{
			byte[] data = GetTestDataArray();

			string fname = Path.Combine(Env.TempPath, "down.dat" + WebFileDownloader.TempFileExtension);
			DownloadDataTestHelper down = this.GetDownloadData(data, fname);

			AssertEquals("Part file does not exist", false, File.Exists(fname));
			AssertEquals("File does not exist", false, File.Exists(FileName));

			this.UserCancelEvent.Set();
			down.DoDownload();

			AssertEquals("Download Finished", true, HasFinishedSet());
			AssertEquals("Download Not Completed", false, down.Completed);
			AssertEquals("Part file does not exist", false, File.Exists(fname));
			AssertEquals("File does not exist", false, File.Exists(FileName));
		}

		public void TestDownloadDataIOOutOfDiscSpaceException()
		{
			GlbStaff staff1 = Factory.New<GlbStaff>();
			staff1.GS_LoginName = "testPostMaster";
			staff1.GS_Code = "tpm";
			staff1.GS_EmailAddress = "testPostMaster@hotmail.com";

			GlbGroup postmastersGroup = Factory.Load<GlbGroup>(Env.Registry.PostMasterGroup);
			postmastersGroup.Staff.Add(staff1);

			Factory.Save();

			byte[] data = GetTestDataArray();
			string fname = Path.Combine(Env.TempPath, "down.dat" + WebFileDownloader.TempFileExtension);
			DownloadDataTestHelper downloadData = GetDownloadData(data, fname);
			downloadData.ThrowOutOfDiskSpaceException = true;

			downloadData.DoDownload();

			AssertEquals("Download Finished", true, HasFinishedSet());
			AssertEquals("Download Not Completed", false, downloadData.Completed);
			AssertEquals("Exception thrown", true, downloadData.HasException);
			AssertEquals("Exception Message", "There is not enough space on the disk", downloadData.CaughtException.Message);
			AssertEquals("Message Created", 1, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestDownloadDataIOException()
		{
			byte[] data = GetTestDataArray();

			string fname = Path.Combine(Env.TempPath, "down.dat" + WebFileDownloader.TempFileExtension);
			DownloadDataTestHelper down = this.GetDownloadData(data, fname);
			down.ThrowIOException = true;

			AssertEquals("Part file does not exist", false, File.Exists(fname));
			AssertEquals("File does not exist", false, File.Exists(FileName));

			down.DoDownload();

			AssertEquals("Download Finished", true, HasFinishedSet());
			AssertEquals("Download Not Completed", false, down.Completed);
			AssertEquals("Exception thrown", true, down.HasException);
			AssertEquals("Exception Message", "Test IOException", down.CaughtException.Message);
			AssertEquals("File does not exist", false, File.Exists(FileName));
		}

		public void TestDownloadDataWebException()
		{
			byte[] data = GetTestDataArray();

			string fname = Path.Combine(Env.TempPath, "down.dat" + WebFileDownloader.TempFileExtension);
			DownloadDataTestHelper down = this.GetDownloadData(data, fname);
			down.ThrowWebException = true;

			AssertEquals("Part file does not exist", false, File.Exists(fname));
			AssertEquals("File does not exist", false, File.Exists(FileName));

			down.DoDownload();

			AssertEquals("Download Finished", true, HasFinishedSet());
			AssertEquals("Download Not Completed", false, down.Completed);
			AssertEquals("Exception thrown", true, down.HasException);
			AssertEquals("Exception Message", "Test WebException", down.CaughtException.Message);
			AssertEquals("File does not exist", false, File.Exists(FileName));
		}

		#region Implementation

		#region TestHelperClass

		public class DownloadDataTestHelper : DownloadData
		{
			public DownloadDataTestHelper(byte[] testData, string fileName, WaitHandle cancelEvent, ManualResetEvent finishedEvent)
				: base(null, fileName, testData.Length, 0, cancelEvent, finishedEvent)
			{
				this.TestData = testData;
			}

			protected override Stream DownloadStream
			{
				get
				{
					if (ThrowWebException)
					{
						throw new WebException("Test WebException");
					}

					if (ThrowIOException)
					{
						throw new IOException("Test IOException");
					}

					if (ThrowOutOfDiskSpaceException)
					{
						throw new IOException("There is not enough space on the disk", ExceptionExtensions.HResultConstants.DiskFull);
					}

					if (TestStream == null)
					{
						TestStream = new MemoryStream(TestData);
					}
					return TestStream;
				}
			}

			public bool ThrowWebException;
			public bool ThrowIOException;
			public bool ThrowOutOfDiskSpaceException;

			readonly byte[] TestData;
			MemoryStream TestStream;
		}
		#endregion

		string FileName;
		ManualResetEvent UserCancelEvent;
		ManualResetEvent FinishedEvent;

		DownloadDataTestHelper GetDownloadData(byte[] testData, string fileName)
		{
			DownloadDataTestHelper result = new DownloadDataTestHelper(testData, fileName, UserCancelEvent, FinishedEvent);
			this.FileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
			return result;
		}

		byte[] GetTestDataArray()
		{
			byte[] result = new byte[3072];
			unchecked
			{
				for (int i = 0; i < result.Length; i++)
				{
					result[i] = (byte)i;
				}
			}
			return result;
		}

		public bool HasFinishedSet()
		{
			return (FinishedEvent != null && FinishedEvent.WaitOne(0, false));
		}

		protected override void SetUp()
		{
			base.SetUp();
			UserCancelEvent = new ManualResetEvent(false);
			FinishedEvent = new ManualResetEvent(false);
		}

		protected override void TearDown()
		{
			base.TearDown();
			UserCancelEvent.Close();
			FinishedEvent.Close();
			if (FileName != null && File.Exists(FileName))
			{
				File.Delete(FileName);
			}
		}

		#endregion
	}
}
