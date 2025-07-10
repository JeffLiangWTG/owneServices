using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.MailManager.FileDownload.Testing
{
	sealed class WebFileDownloaderTest : TestCaseWithFactory
	{
		public void TestFileDownloaderSuccess()
		{
			using (WebFileDownloader down = new WebFileDownloader(Url))
			{
				AssertNotNull(down);
				AssertEquals("File does not exists", false, File.Exists(FileFullPath));

				down.StartFileDownload(TargetFolder);
				down.FinishedWaitHandle.WaitOne(120000, false);

				Assert("Download should complete. If not, web server could be down", down.HasDownloadCompleted);
				AssertEquals("File exists", true, File.Exists(FileFullPath));
				AssertEquals("No file existed", false, down.TargetFileAlreadyExists);
				AssertEquals("The whole file was downloaded", down.FileSize, down.TotalDownloaded);

				down.StartFileDownload(TargetFolder);
				down.FinishedWaitHandle.WaitOne(120000, false);

				Assert("Download should complete. If not, web server could be down", down.HasDownloadCompleted);
				Assert("Target file file existed before", down.TargetFileAlreadyExists);
				AssertEquals("File Name assigned", TestFileName, down.FileName);
				AssertEquals("Downloaded File Path", FileFullPath, down.TargetFilePath);
				AssertEquals("Nothing was downloaded", 0, down.TotalDownloaded);
			}
			using (WebFileDownloader down2 = new WebFileDownloader(Url))
			{
				down2.StartFileDownload(TargetFolder);
				down2.FinishedWaitHandle.WaitOne(120000, false);

				Assert("Download should complete. If not, web server could be down", down2.HasDownloadCompleted);
				Assert("Target file file existed before", down2.TargetFileAlreadyExists);
				AssertEquals("File Name assigned", TestFileName, down2.FileName);
				AssertEquals("Downloaded File Path", FileFullPath, down2.TargetFilePath);
				AssertEquals("Nothing was downloaded", 0, down2.TotalDownloaded);
				AssertEquals($"File already exists : {FileFullPath}", down2.StatusMessage);
			}
		}

		public void TestFileDownloaderResume()
		{
			using (WebFileDownloader down = new WebFileDownloader(Url))
			{
				AssertNotNull(down);
				AssertEquals("File does not exists", false, File.Exists(FileFullPath));

				down.StartFileDownload(TargetFolder);
				down.FinishedWaitHandle.WaitOne(120000, false);

				Assert("Download should complete. If not, web server could be down", down.HasDownloadCompleted);
				AssertEquals("File exists", true, File.Exists(FileFullPath));
				AssertEquals("No file existed", false, down.TargetFileAlreadyExists);
				AssertEquals("The whole file was downloaded", down.FileSize, down.TotalDownloaded);

				int part;
				using (FileStream readStream = File.OpenRead(FileFullPath))
				using (FileStream writeStream = File.OpenWrite(FileFullPath + WebFileDownloader.TempFileExtension))
				{
					byte[] buffer = new byte[2048];
					part = readStream.Read(buffer, 0, buffer.Length);
					writeStream.Write(buffer, 0, part);
				}
				File.Delete(FileFullPath);

				AssertEquals("File does not exists", false, File.Exists(FileFullPath));
				AssertEquals("File exists", true, File.Exists(FileFullPath + WebFileDownloader.TempFileExtension));

				if (!httpTestHelper.IsListening)
				{
					httpTestHelper.Start();
					File.WriteAllBytes(Path.Combine(httpTestHelper.LocalDirectory, TestFileName), (byte[])Array.CreateInstance(typeof(byte), TestFileSize));
					using (WebFileDownloader down2 = new WebFileDownloader(Url))
					{
						down2.StartFileDownload(TargetFolder);
						down2.FinishedWaitHandle.WaitOne(120000, false);

						Assert("httpTestHelper.HttpServer should be listening", httpTestHelper.IsListening);
						AssertEquals($"DownloadData should not have Exceptions.{down2.DownloadData.CaughtException?.ToString()}", false, down2.DownloadData.HasException);
						Assert("Download should complete. If not, web server could be down", down2.HasDownloadCompleted);
						AssertEquals("Target file file did not exist before", false, down2.TargetFileAlreadyExists);
						AssertEquals("The whole file was downloaded", down2.FileSize, down2.TotalDownloaded);
					}
				}
				else
				{
					down.StartFileDownload(TargetFolder);
					down.FinishedWaitHandle.WaitOne(120000, false);

					Assert("httpTestHelper.HttpServer should be listening", httpTestHelper.IsListening);
					AssertEquals($"DownloadData should not have Exceptions.{down.DownloadData.CaughtException?.ToString()}", false, down.DownloadData.HasException);
					Assert("Download should complete. If not, web server could be down", down.HasDownloadCompleted);
					AssertEquals("Target file file did not exist before", false, down.TargetFileAlreadyExists);
					AssertEquals("The whole file was downloaded", down.FileSize, down.TotalDownloaded);
				}
			}
		}

		public void TestFileDownloaderFailure()
		{
			using (WebFileDownloader down = new WebFileDownloader(BadUrl))
			{
				AssertNotNull(down);
				AssertEquals("File does not exists", false, File.Exists(FileFullPath));

				down.StartFileDownload(TargetFolder);
				down.FinishedWaitHandle.WaitOne(120000, false);

				AssertEquals("Download should complete. If not, web server could be down", false, down.HasDownloadCompleted);
				AssertEquals("File does not exist", false, File.Exists(FileFullPath));
				AssertEquals("No file existed", false, down.TargetFileAlreadyExists);
				AssertEquals("Nothing was downloaded", 0, down.TotalDownloaded);

				DisposableLeakListener.Instance.IgnoreObject(down);
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenResponseIsNull()
		{
			using (WebFileDownloader down = new WebFileDownloader(Url))
			{
				WebResponse response = null;
				Assert(!down.ResponseHasValidStatusExposed(response));
				AssertEquals("Response has invalid status. Protocol Type: HttpProtocolSupport Mode: StartDownload", down.StatusMessage);
			}
		}

		public void TestFileDownloaderCancelled()
		{
			using (WebFileDownloader down = new WebFileDownloader(Url))
			{
				AssertNotNull(down);
				AssertEquals("File does not exists", false, File.Exists(FileFullPath));

				down.Cancel();
				down.StartFileDownload(TargetFolder);

				AssertEquals("Download was cancelled", true, down.HasUserCancelled);
				AssertEquals("Download should complete. If not, web server could be down", false, down.HasDownloadCompleted);
				AssertEquals("File does not exist", false, File.Exists(FileFullPath));
				AssertEquals("No file existed", false, down.TargetFileAlreadyExists);
				AssertEquals("Nothing was downloaded", 0, down.TotalDownloaded);
				AssertEquals("Process canceled by user.", down.StatusMessage);
			}
		}

		[UseSnapshotProtection]
		[ExpectNoExceptions]
		public void TestFileDownloaderSuccessFromAnotherThread()
		{
			Exception exceptionFromThread = null;
			ThreadStart threadstart = new ThreadStart(delegate
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'user@abcd.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE = 'PM'");
					}

					using (var down = new WebFileDownloaderTestHelper(Url))
					{
						down.StartFileDownload(TargetFolder);
						down.FinishedWaitHandle.WaitOne(120000, false);
					}
				}
				catch (Exception ex)
				{
					exceptionFromThread = ex;
				}
			});
			var thread = new Thread(threadstart);
			thread.Start();
			thread.Join();

			if (exceptionFromThread != null)
			{
				throw exceptionFromThread;
			}
		}

		public void TestWebFileDownloaderWithDownloadDataExceptionMessage()
		{
			Exception exceptionFromThread = null;
			var statusMessage = string.Empty;
			DownloadDataForTest downloadData = null;
			try
			{
				ThreadStart threadstart = new ThreadStart(delegate
				{
					try
					{
						using (var down = new WebFileDownloaderTestHelper2(Url))
						{
							down.StartFileDownload(TargetFolder);
							downloadData = down.DownloadData as DownloadDataForTest;
							statusMessage = down.StatusMessage;
						}
					}
					catch (Exception ex)
					{
						exceptionFromThread = ex;
					}
				});
				var thread = new Thread(threadstart);
				thread.Start();
				thread.Join();

				if (exceptionFromThread != null)
				{
					throw exceptionFromThread;
				}

				AssertEquals(true, downloadData.HasException);
				AssertEquals("Test IOException Message", statusMessage);
				AssertEquals("Test IOException Message", downloadData.CaughtException.Message);

				var content = File.ReadAllText(downloadData.TempFileName);
				AssertEquals("Test1234", content);
			}
			finally
			{
				if (downloadData != null)
				{
					downloadData.DeleteTempFileForTest();
				}
			}
		}

		public void TestFileDownloaderTimeOut()
		{
			// Arrange
			httpTestHelper.Dispose();

			var timeOutMilliseconds = 2000;
#pragma warning disable SYSLIB0014 // WebRequest, HttpWebRequest, ServicePoint, and WebClient are obsolete. Use HttpClient instead.
			var webRequest = WebRequest.Create(Url);
#pragma warning restore SYSLIB0014
			webRequest.Timeout = timeOutMilliseconds;

			var downLoaderMock = new Mock<WebFileDownloader>(Url) { CallBase = true };
			downLoaderMock
				.Protected()
				.Setup<WebRequest>("GetRequest")
				.Returns(webRequest);

			using (var downloader = downLoaderMock.Object)
			using (var socket = CreateTimeOutSocketAndStartListen(httpTestHelper.Port))
			{
				// Act
				downloader.StartFileDownload(TargetFolder);
				downloader.FinishedWaitHandle.WaitOne(timeOutMilliseconds + 1000, false);

				// Assert
				Assert("Download should finish", downloader.HasFinished);
				Assert("Download should not complete", !downloader.HasDownloadCompleted);
				Assert("Status message should be Request Aborted ", downloader.StatusMessage.Equals("Request Aborted"));
			}

			Socket CreateTimeOutSocketAndStartListen(int port)
			{
				var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				socket.Bind(new IPEndPoint(IPAddress.Loopback, port));
				socket.Listen((int)SocketOptionName.MaxConnections);
				return socket;
			}
		}

		#region TestHelperClass

		public class WebFileDownloaderTestHelper : WebFileDownloader
		{
			public WebFileDownloaderTestHelper(string url) : base(url) { }

			protected override DownloadData GetDownloadData(WebResponse response)
			{
				byte[] data = GetTestDataArray();
				string fname = Path.Combine(Env.TempPath, "down.dat" + WebFileDownloader.TempFileExtension);
				var downloadData = new DownloadDataTest.DownloadDataTestHelper(data, fname, CancelEvent, FinishedEvent);
				downloadData.ThrowOutOfDiskSpaceException = true;
				return downloadData;
			}

			protected byte[] GetTestDataArray()
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
		}

		public class WebFileDownloaderTestHelper2 : WebFileDownloader
		{
			public WebFileDownloaderTestHelper2(string url) : base(url) { }

			protected override DownloadData GetDownloadData(WebResponse response)
			{
				return new DownloadDataForTest(response, TempFilePath, FileSize, 0, CancelEvent, FinishedEvent);
			}

			protected override WebRequest GetRequest()
			{
				var result = new Mock<IAsyncResult>();
				result.Setup(t => t.AsyncWaitHandle).Returns(new ManualResetEvent(false));
				var mock = new Mock<WebRequest>();
				mock.Setup(r => r.RequestUri).Returns(new Uri(TestUrl));
				mock.Setup(r => r.Timeout).Returns(500);
				mock.Setup(r => r.BeginGetResponse(It.IsAny<AsyncCallback>(), It.IsAny<RequestState>()))
					.Returns(result.Object)
					.Callback((AsyncCallback callback, object state) =>
					{
						callback(result.Object);
					});
				return mock.Object;
			}

			protected override WebResponse GetResponseFromAsyncResult(IAsyncResult requestResult)
			{
				var mock = new Mock<WebResponse>();
				mock.Setup(r => r.ResponseUri).Returns(new Uri(TestUrl));
				mock.Setup(r => r.ContentLength).Returns(100);
				mock.Setup(r => r.GetResponseStream()).Returns(new MemoryStream(Encoding.ASCII.GetBytes("Test1234")));
				return mock.Object;
			}

			protected override bool ResponseHasValidStatus(WebResponse response) => true;

			protected override void GetFileInfoFromResponse(WebResponse response)
			{
			}

			protected override void StartDataDownload(WebResponse response)
			{
				base.StartDataDownload(response);
				fDataDownloader.Join();
			}

			const string TestUrl = "http://localhost/test.text";
		}

		public class DownloadDataForTest : DownloadData
		{
			public DownloadDataForTest(WebResponse response, string fileName, long size, long start, WaitHandle cancelEvent, ManualResetEvent finishedEvent)
				: base(response, fileName, size, start, cancelEvent, finishedEvent)
			{
				TempFileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension((fileName)));
			}

			protected override void FinaliseDownload()
			{
				base.FinaliseDownload();
				throw new IOException("Test IOException Message");
			}

			public string TempFileName { get; }

			public void DeleteTempFileForTest()
			{
				DownloadStream.Dispose();
				if (File.Exists(TempFileName))
				{
					File.Delete(TempFileName);
				}
				if (File.Exists(TempFileName + WebFileDownloader.TempFileExtension))
				{
					File.Delete(TempFileName + WebFileDownloader.TempFileExtension);
				}
			}
		}

		#endregion

		#region Implementation

		public const string BadUrl = "http://www.cargowise.com/ftpmirro/ediEnterprise/TestFiles_DO_NOT_DELETE/wdm_fx00.pcx";
		public const string TestFileName = "wdm_fx00.pcx";
		public const int TestFileSize = 21041;

		public const string FtpTestFileName = "wdm_fx00.pcx";
		public const int FtpTestFileSize = -1;

		string Url
		{
			get { return new Uri(httpTestHelper.ServerAddress, TestFileName).ToString(); }
		}

		string FileFullPath
		{
			get { return Path.Combine(TargetFolder, TestFileName); }
		}

		string TargetFolder
		{
			get { return Env.TempPath; }
		}

		protected override void SetUp()
		{
			base.SetUp();

			httpTestHelper = new HttpTestHelper();
			httpTestHelper.Start();

			File.WriteAllBytes(Path.Combine(httpTestHelper.LocalDirectory, TestFileName), (byte[])Array.CreateInstance(typeof(byte), TestFileSize));

			if (File.Exists(FileFullPath))
			{
				File.Delete(FileFullPath);
			}

			if (File.Exists(FileFullPath + WebFileDownloader.TempFileExtension))
			{
				File.Delete(FileFullPath + WebFileDownloader.TempFileExtension);
			}
		}

		protected override void TearDown()
		{
			base.TearDown();

			httpTestHelper.Dispose();

			if (File.Exists(FileFullPath))
			{
				File.Delete(FileFullPath);
			}

			if (File.Exists(FileFullPath + WebFileDownloader.TempFileExtension))
			{
				File.Delete(FileFullPath + WebFileDownloader.TempFileExtension);
			}
		}

		HttpTestHelper httpTestHelper;

		#endregion
	}
}
