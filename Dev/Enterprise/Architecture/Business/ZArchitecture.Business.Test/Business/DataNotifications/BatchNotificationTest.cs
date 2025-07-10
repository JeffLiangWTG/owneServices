using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class BatchNotificationTest : TestCase
	{
		public void TestErrorType()
		{
			BatchNotification notification = new BatchNotification(ErrorType.IOError, "Test IO Error!");
			AssertEquals(ErrorType.IOError, notification.ErrorType);
			AssertEquals("Error: File system I/O error (Test IO Error!)", notification.Message);
		}
	}
}
