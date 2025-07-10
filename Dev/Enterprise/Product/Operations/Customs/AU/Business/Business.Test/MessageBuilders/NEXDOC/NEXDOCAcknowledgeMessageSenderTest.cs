using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class NEXDOCAcknowledgeMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendAcceptMessage()
		{
			var sender = new NEXDOCAcknowledgeMessageSender();
			AssertEquals("Unsaved item PK", "Could not find the selected Notification.", sender.SendAcceptMessage(ZGuid.NewZGuid()));

			var notification = Factory.New<QuarantineNexDocNotification>();
			notification.QN_SystemCreateTimeUtc = ZDateTime.Now;
			notification.QN_AcknowledgeStatus = "ACC";
			Factory.Save();

			AssertEquals("Wrong Acknowledge Status", "Acknowledge status must be NOT - Not Actioned or ERR - Error.", sender.SendAcceptMessage(notification.PK));

			notification.QN_AcknowledgeStatus = "NOT";
			notification.QN_NotificationType = "CRR";
			Factory.Save();
			AssertEquals("Wrong Acknowledge Status", "Acknowledge type must Forward or Transfer.", sender.SendAcceptMessage(notification.PK));
			AssertNull("Should not created any message", Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, notification.PK)));

			notification.QN_NotificationType = "FA";
			Factory.Save();
			AssertNullOrEmpty("Should return empty message", sender.SendAcceptMessage(notification.PK));
			AssertNotNull("Should have created a message", Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, notification.PK)));
		}

		public void TestRejectMessage()
		{
			var sender = new NEXDOCAcknowledgeMessageSender();
			var notification = Factory.New<QuarantineNexDocNotification>();
			notification.QN_AcknowledgeStatus = "NOT";
			notification.QN_NotificationType = "TA";
			notification.QN_SystemCreateTimeUtc = ZDateTime.Now;
			Factory.Save();
			AssertNullOrEmpty("Should return empty message", sender.SendAcceptMessage(notification.PK));
			AssertNotNull("Should have created a message", Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_LinkUniqueID, notification.PK)));
		}
	}
}
