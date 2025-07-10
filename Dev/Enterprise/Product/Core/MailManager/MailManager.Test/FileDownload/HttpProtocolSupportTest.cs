using System;
using System.IO;
using System.Net;

namespace Enterprise.MailManager.FileDownload.Testing
{
	sealed class HttpProtocolSupportTest : WebProtocolSupportTest
	{
		public override void TestGetWebRequestForDownload()
		{
			base.TestGetWebRequestForDownload();

			HttpWebRequest request = TestRequest as HttpWebRequest;
			AssertNotNull("SHould be Http request", request);
			AssertEquals("Url should match", TestUrl, request.Address.AbsoluteUri);
		}

		public override void TestGetWebRequestForDownloadWithStartPosition()
		{
			base.TestGetWebRequestForDownloadWithStartPosition();

			HttpWebRequest request = TestRequest as HttpWebRequest;
			AssertNotNull("SHould be Http request", request);
			AssertEquals("Url should match", TestUrl, request.Address.AbsoluteUri);
			string expectedRange = "bytes=" + (TestFileSize / 2).ToString() + "-";
			AssertEquals("Start Position", expectedRange, request.Headers["Range"]);
		}

		public override void TestGetWebRequestForFileInfo()
		{
			base.TestGetWebRequestForFileInfo();

			HttpWebRequest request = TestRequest as HttpWebRequest;
			AssertNotNull("SHould be Http request", request);
			AssertEquals("Url should match", TestUrl, request.Address.AbsoluteUri);
			AssertEquals("Request Method", WebRequestMethods.Http.Head, request.Method);
		}

		public override void TestResponseHasValidStatus()
		{
			base.TestResponseHasValidStatus();

			HttpWebResponse response = TestResponse as HttpWebResponse;
			AssertNotNull("SHould be Http response", response);
			AssertEquals("Url should match", TestUrl, response.ResponseUri.AbsoluteUri);
			AssertEquals("File Size should match", TestFileSize, response.ContentLength);
			AssertEquals("Last Modified should match", TestFileLastModified, response.LastModified);
		}

		public void TestResponseHasValidStatusForRerquestWithStartPosition()
		{
			TestRequest = WebProtocolSupportForTest.GetWebRequestForDownload(TestFileSize / 2);
			TestResponse = TestRequest.GetResponse();

			AssertNotNull("Response should be not null", TestResponse);

			HttpWebResponse response = TestResponse as HttpWebResponse;
			AssertNotNull("SHould be Http response", response);

			Assert("Response has valid status", WebProtocolSupportForTest.ResponseHasValidStatus(TestResponse, WebProtocolSupport.RequestMode.ResumeDownload));

			AssertEquals("Url should match", TestUrl, response.ResponseUri.AbsoluteUri);
			AssertEquals("Content Length should match the file part size", TestFileSize - (TestFileSize / 2), response.ContentLength);
			AssertEquals("Last Modified should match", TestFileLastModified, response.LastModified);
		}

		public void TestResponseHasValidStatusForFileInfoRequest()
		{
			TestRequest = WebProtocolSupportForTest.GetWebRequestForFileInfo();
			TestResponse = TestRequest.GetResponse();

			AssertNotNull("Response should be not null", TestResponse);

			HttpWebResponse response = TestResponse as HttpWebResponse;
			AssertNotNull("SHould be Http response", response);

			Assert("Response has valid status", WebProtocolSupportForTest.ResponseHasValidStatus(TestResponse, WebProtocolSupport.RequestMode.GetFileInfo));

			AssertEquals("Url should match", TestUrl, response.ResponseUri.AbsoluteUri);
			AssertEquals("Last Modified should match", TestFileLastModified, response.LastModified);
		}

		public override void TestIsResumeSupported()
		{
			AssertEquals("Resume is supported for Http", true, WebProtocolSupportForTest.IsResumeSupported);

			var test = new HttpProtocolSupport("https://www.ccf.border.gov.au/reference/production/main/P1-MAIN.tar.gz");
			AssertEquals("Resume is not supported for Https", false, test.IsResumeSupported);
		}

		#region Implementation

		protected override string TestUrl
		{
			get { return new Uri(httpTestHelper.ServerAddress, WebFileDownloaderTest.TestFileName).ToString(); }
		}

		protected override string TestFileName
		{
			get { return WebFileDownloaderTest.TestFileName; }
		}

		protected override int TestFileSize
		{
			get { return WebFileDownloaderTest.TestFileSize; }
		}

		protected override DateTime TestFileLastModified
		{
			get
			{
				var lastModified = new FileInfo(Path.Combine(httpTestHelper.LocalDirectory, WebFileDownloaderTest.TestFileName)).LastWriteTime;
				return new DateTime(lastModified.Year, lastModified.Month, lastModified.Day, lastModified.Hour, lastModified.Minute, lastModified.Second);
			}
		}

		protected override WebProtocolSupport GetProtocolSupportForTest()
		{
			return new HttpProtocolSupport(TestUrl);
		}

		protected override void SetUp()
		{
			httpTestHelper = new HttpTestHelper();
			httpTestHelper.Start();
			File.WriteAllBytes(Path.Combine(httpTestHelper.LocalDirectory, WebFileDownloaderTest.TestFileName), (byte[])Array.CreateInstance(typeof(byte), WebFileDownloaderTest.TestFileSize));

			base.SetUp();
		}

		protected override void TearDown()
		{
			httpTestHelper.Dispose();

			base.TearDown();
		}

		HttpTestHelper httpTestHelper;

		#endregion
	}
}
