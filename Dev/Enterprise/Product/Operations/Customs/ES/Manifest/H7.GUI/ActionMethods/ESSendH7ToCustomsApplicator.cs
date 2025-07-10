using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.Customs.ES.Manifest.H7.Business.BusinessObjects.Interfaces;
using Enterprise.Services.OperationalActions.Support;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	public class ESSendH7ToCustomsApplicator : EU.H7.GUI.SendH7ToCustomsApplicator
	{
		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			foreach (AsycudaManifestHeader header in targets)
			{
				var messageSendingParent = header.ApplicationBusinessProvider.GetNewMessageSendingObjectParent(header);
				foreach (H7MessageSendingObject sendingObject in messageSendingParent.SendingObjectsCollection)
				{
					sendingObject.Action = sendingObject.ActionForSendingCustomsDeclaration;
				}

				var sender = new H7MessageSender(messageSendingParent.SendingObjectsCollection.Cast<IH7CommonMessageSendingObject>());
				var messageBuildersToSend = sender.GetMessageBuildersData();
				var messagesInfo = sender.Send(messageBuildersToSend);

				if (messagesInfo.MessagesSent > 0)
				{
					log.NotifyFormat(OperationalActionLogErrorLevel.Success, "{0} : {1}.", GetAsycudaManifestHeaderIdLink(header), SubmitSucceeded);
				}
			}
		}
	}
}
