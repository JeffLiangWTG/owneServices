using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.Testing
{
	class DocumentAutoDeliveryNotificationBufferTest : TestCaseWithFactory
	{
		[TestDate(2005, 1, 2)]
		public void TestEmailBody()
		{
			TestDocumentAutoDeliveryNotificationBuffer notify = new TestDocumentAutoDeliveryNotificationBuffer("Document Delivery Notifications");
			notify.Notify(new ErrorNotification(ErrorType.Error, "Message"));
			AssertMultilineASCIIEquals("Correct EmailBody with default header/footer", @"
Document Delivery Notifications; Generated 02-Jan-05 00:00:00

Error: Message
".Trim(), notify.EmailBody);
		}

		class TestDocumentAutoDeliveryNotificationBuffer : DocumentAutoDeliveryNotificationBuffer
		{
			public TestDocumentAutoDeliveryNotificationBuffer(ZString heading) : base(heading)
			{
			}

			public new string EmailBody
			{
				get
				{
					return base.EmailBody;
				}
			}
		}
	}
}
