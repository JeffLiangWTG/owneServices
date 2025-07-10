using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MessageBuilders;
using Enterprise.Customs.GB.Business.MessageManagers.DocumentSending;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public class CDSH7SupportingDocSendingManager : GBSupportingDocSendingManager
	{
		public CDSH7SupportingDocSendingManager(UploadDocumentsSendingActionParent declarationWrapper, IMessageNotificationCollector notification) : base(declarationWrapper, notification)
		{
			factory = declarationWrapper.Factory;
		}
		readonly BusinessObjectFactory factory;

		public override void SendMessages()
		{
			factory.Saving += SetDefaultMessageInterpretation;
			messagesSent = 0;
			base.SendMessages();
			factory.Saving -= SetDefaultMessageInterpretation;
		}

		void SetDefaultMessageInterpretation(BusinessObjectFactory factory)
		{
			var newEdiMessages = factory.GetChanges().GetAddedObjects().OfType<EDIMessage>();
			foreach (var message in newEdiMessages)
			{
				_ = message.EM_MessageInterpretation; // This getter will set a default interpretation of UniversalDataMessaging messages
			}
		}

		protected override void NotifyQueuedForSending(ISupportingDocumentMessageDataProvider[] sendingObjects)
		{
			base.NotifyQueuedForSending(sendingObjects);
			messagesSent += sendingObjects.Length;
		}

		protected override ZString GetMessageSendingResultInformation(MessageSendingNotificationCollection notifications)
		{
			return Res.GetString("4ed2ae10-b0b7-401b-a089-1d66a2425601", "{0} documents(s) queued for sending.", messagesSent);
		}

		int messagesSent;
	}
}
