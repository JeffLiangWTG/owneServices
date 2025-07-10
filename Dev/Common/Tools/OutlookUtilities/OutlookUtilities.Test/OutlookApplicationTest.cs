using System.IO;
using System.Runtime.InteropServices;
using NUnit.Framework;

namespace Enterprise.Interop.OutlookIntegration.Testing
{
	sealed class OutlookApplicationTest : TestCase
	{
		#region TestCreateMailItem

		[DeveloperOnlyTest]
		public void TestCreateMailItem_WithRealOutlookApplication()
		{
			var item = new OutlookApplication().CreateMailItem(null);
			AssertNotNull(item);
		}

		public void TestCreateMailItem_WithMinimalVersionComInterfaces()
		{
			var item = new MockOutlookApplication().CreateMailItem(null);
			AssertNotNull(item);
		}

		[ExpectException(typeof(OutlookDialogBoxOpenException))]
		public void TestCreateMailItem_RethrowsCOMException()
		{
			var outlookApplication = new MockOutlookApplication();
			outlookApplication.ExceptionToThrowOnGetComOutlookApplication = new COMException("A dialog box is open");
			var item = outlookApplication.CreateMailItem(null);
		}

		[ExpectException(typeof(OutlookException))]
		public void TestCreateMailItem_RethrowsDirectoryNotFoundException()
		{
			var outlookApplication = new MockOutlookApplication();
			outlookApplication.ExceptionToThrowOnGetComOutlookApplication = new DirectoryNotFoundException();
			var item = outlookApplication.CreateMailItem(null);
		}

		[ExpectException(typeof(OutlookException))]
		public void TestCreateMailItem_RethrowsFileNotFoundException()
		{
			var outlookApplication = new MockOutlookApplication();
			outlookApplication.ExceptionToThrowOnGetComOutlookApplication = new FileNotFoundException();
			var item = outlookApplication.CreateMailItem(null);
		}

		[ExpectException(typeof(OutlookException))]
		public void TestCreateMailItem_RethrowsInvalidCastException()
		{
			var outlookApplication = new MockOutlookApplication();
			outlookApplication.ExceptionToThrowOnGetComOutlookApplication = new FileNotFoundException();
			var item = outlookApplication.CreateMailItem(null);
		}

		#endregion
	}
}
