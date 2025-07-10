using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	public class GOVCBRMessageWrapperTest : TestCaseWithFactory
	{
		public virtual void TestHasGOVCBRMessage()
		{
			AssertEquals("HasGOVCBRMessage", false, wrapper.HasGOVCBRMessage);
		}

		public virtual void TestDocumentName()
		{
			AssertEquals("DocumentName", ZString.Empty, wrapper.DocumentName);
		}

		public virtual void TestDocumentReference()
		{
			AssertEquals("DocumentReference", ZString.Empty, wrapper.DocumentReference);
		}

		public virtual void TestProcessingDate()
		{
			AssertEquals("ProcessingDate", ZDateTime.Empty, wrapper.ProcessingDate);
		}

		public virtual void TestIsMessageContentAccepted()
		{
			AssertEquals("IsMessageContentAccepted", false, wrapper.IsMessageContentAccepted);
		}

		public virtual void TestIsMessageContentAcceptedWithComments()
		{
			AssertEquals("IsMessageContentAcceptedWithComments", false, wrapper.IsMessageContentAcceptedWithComments);
		}

		public virtual void TestIsMessageContentRejectedWithComment()
		{
			AssertEquals("IsMessageContentRejectedWithComment", false, wrapper.IsMessageContentRejectedWithComment);
		}

		public virtual void TestIsMessageReceived()
		{
			AssertEquals("IsMessageReceived", false, wrapper.IsMessageReceived);
		}

		public virtual void TestIsErrorMessage()
		{
			AssertEquals("IsErrorMessage", false, wrapper.IsErrorMessage);
		}

		public virtual void TestOriginalMessageReference()
		{
			AssertEquals("OriginalMessageReference", ZString.Empty, wrapper.OriginalMessageReference);
		}

		public virtual void TestErrorComments()
		{
			AssertContainsExactElementsInAnyOrder("ErrorComments", Array.Empty<string>(), wrapper.ErrorComments);
		}

		public virtual void TestNotifications()
		{
			AssertContainsExactElementsInAnyOrder("Notifications", Array.Empty<Notification>(), wrapper.Notifications);
		}

		public virtual void TestNoticeStatusCode()
		{
			AssertEquals("NoticeStatusCode", ZString.Empty, wrapper.NoticeStatusCode);
		}

		GOVCBRMessageWrapper wrapper;
		protected override void SetUp()
		{
			var message = Factory.New<EDIMessage>();
			wrapper = new GOVCBRMessageWrapper(message);
		}
	}
}
