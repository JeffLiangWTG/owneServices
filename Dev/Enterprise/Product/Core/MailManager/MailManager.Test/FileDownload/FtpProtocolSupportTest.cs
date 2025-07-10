using System;
using System.IO;
using CargoWise.IO.Testing;
using Enterprise.MailManager.FileDownload.Testing;

namespace Enterprise.MailManager.FileDownload
{
	sealed class FtpProtocolSupportTest : WebProtocolSupportTest
	{
		public override void TestIsResumeSupported()
		{
			AssertEquals("Resume is supported for Ftp", true, WebProtocolSupportForTest.IsResumeSupported);
		}

		#region Implementation

		protected override string TestUrl
		{
			get { return "ftp://localhost:" + ftpTestHelper.Port + "/" + TestFileName; }
		}

		protected override string TestFileName
		{
			get { return WebFileDownloaderTest.FtpTestFileName; }
		}

		protected override int TestFileSize
		{
			get { return WebFileDownloaderTest.FtpTestFileSize; }
		}

		protected override DateTime TestFileLastModified
		{
			get { return new DateTime(); } // Last modified time is not available
		}

		protected override WebProtocolSupport GetProtocolSupportForTest()
		{
			return new FtpProtocolSupport(TestUrl);
		}

		protected override void SetUp()
		{
			base.SetUp();
			ftpTestHelper = new FtpTestHelper("anonymous", "anonymous@");
			ftpTestHelper.Start();
			File.WriteAllBytes(Path.Combine(ftpTestHelper.LocalDirectory, TestFileName), (byte[])Array.CreateInstance(typeof(byte), 20001));
		}

		protected override void TearDown()
		{
			base.TearDown();
			ftpTestHelper.Dispose();
		}

		FtpTestHelper ftpTestHelper;

		#endregion
	}
}
