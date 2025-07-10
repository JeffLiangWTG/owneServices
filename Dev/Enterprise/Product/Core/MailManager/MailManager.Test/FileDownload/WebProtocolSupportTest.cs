using System;
using System.Net;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MailManager.FileDownload
{
	public abstract class WebProtocolSupportTest : TestCaseWithFactory
	{
		public void TestCreateProtocolSupporter()
		{
			AssertNotNull("Protocol support should be not null", WebProtocolSupportForTest);
			AssertEquals("Should have correct URL", TestUrl, WebProtocolSupportForTest.Url);
		}

		public virtual void TestGetWebRequestForDownload()
		{
			TestRequest = WebProtocolSupportForTest.GetWebRequestForDownload();
			AssertNotNull("WebRequest should be not null", TestRequest);
		}

		public virtual void TestGetWebRequestForDownloadWithStartPosition()
		{
			TestRequest = WebProtocolSupportForTest.GetWebRequestForDownload(TestFileSize / 2);
			AssertNotNull("WebRequest should be not null", TestRequest);
		}

		public virtual void TestGetWebRequestForFileInfo()
		{
			TestRequest = WebProtocolSupportForTest.GetWebRequestForFileInfo();
			AssertNotNull("WebRequest should be not null", TestRequest);
		}

		public virtual void TestResponseHasValidStatus()
		{
			try
			{
				TestRequest = WebProtocolSupportForTest.GetWebRequestForDownload();
				TestResponse = TestRequest.GetResponse();
			}
			catch (WebException e)
			{
				Fail(String.Format("Test file at {0} could be temporary inaccessible due to the network or server problems. The caught exception is {1}", WebProtocolSupportForTest.Url, e.Message));
			}

			AssertNotNull("Response should be not null", TestResponse);
			Assert("Response has valid status", WebProtocolSupportForTest.ResponseHasValidStatus(TestResponse, WebProtocolSupport.RequestMode.StartDownload));
		}

		public void TestGetFileName()
		{
			ObtainAndAssertTestResponse();

			AssertEquals("Test file name", TestFileName, WebProtocolSupportForTest.GetFileName(TestResponse));
		}

		public void TestGetFileSize()
		{
			ObtainAndAssertTestResponse();

			AssertEquals("Test file size", TestFileSize, WebProtocolSupportForTest.GetFileSize(TestResponse));
		}

		public void TestGetFileLastModified()
		{
			ObtainAndAssertTestResponse();

			AssertEquals("Test file last modified date time", TestFileLastModified, WebProtocolSupportForTest.GetFileLastModified(TestResponse));
		}

		public void TestGetFileNameFromUri()
		{
			Uri testUri = new Uri(TestUrl);
			AssertNotNull("Uri is not null", testUri);

			AssertEquals("File name from Uri", TestFileName, WebProtocolSupportForTest.GetFileNameFromUri(testUri));
		}

		public virtual void TestIsResumeSupported()
		{
			AssertEquals("Resume is not supported by default", false, WebProtocolSupportForTest.IsResumeSupported);
		}

		#region Implementation

		protected void ObtainAndAssertTestResponse()
		{
			try
			{
				TestRequest = WebProtocolSupportForTest.GetWebRequestForDownload();
				TestResponse = TestRequest.GetResponse();
			}
			catch (WebException e)
			{
				Fail(String.Format("Test file at {0} could be temporary inaccessible due to the network or server problems. The caught exception is {1}", WebProtocolSupportForTest.Url, e.Message));
			}

			AssertNotNull("Response should be not null", TestResponse);
		}

		protected abstract string TestUrl
		{ get; }

		protected abstract string TestFileName
		{ get; }

		protected abstract int TestFileSize
		{ get; }

		protected abstract DateTime TestFileLastModified
		{ get; }

		protected WebProtocolSupport WebProtocolSupportForTest
		{
			get
			{
				if (fWebProtocolSupportForTest == null)
				{
					fWebProtocolSupportForTest = GetProtocolSupportForTest();
				}
				return fWebProtocolSupportForTest;
			}
		}

		WebProtocolSupport fWebProtocolSupportForTest;

		protected abstract WebProtocolSupport GetProtocolSupportForTest();

		protected WebRequest TestRequest
		{
			get { return fTestRequest; }
			set { fTestRequest = value; }
		}

		WebRequest fTestRequest;

		protected WebResponse TestResponse
		{
			get { return fTestResponse; }
			set { fTestResponse = value; }
		}

		WebResponse fTestResponse;

		protected override void TearDown()
		{
			base.TearDown();

			if (TestRequest != null)
			{
				TestRequest.Abort();
				TestRequest = null;
			}
			if (TestResponse != null)
			{
				TestResponse.Close();
				TestResponse = null;
			}
		}

		#endregion
	}
}
