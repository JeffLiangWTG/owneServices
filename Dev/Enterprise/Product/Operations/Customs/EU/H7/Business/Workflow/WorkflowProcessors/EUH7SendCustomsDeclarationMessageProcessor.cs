using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.EU.H7.Business
{
	public class EUH7SendCustomsDeclarationMessageProcessor : IProcessor
	{
		public EUH7SendCustomsDeclarationMessageProcessor(AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		AsycudaManifestHeader Header { get; }

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var provider = Header.ApplicationBusinessProvider;
			var messageSendingParent = provider.GetNewMessageSendingObjectParent(Header);

			using (DisposableEnvironment.ForBranch(Header.AMA_GB.ToGuid()))
			{
				var messagesSent = SendCustomsDeclaration(messageSendingParent);

				if (messagesSent > 0)
				{
					notifications.Add(NotificationType.Information, $"Sent {messagesSent} Customs Declaration for {Header.HumanReadableName}");
				}
			}
		}

		public virtual int SendCustomsDeclaration(BaseMessageSendingObjectParent messageSendingParent)
		{
			var messagesSent = 0;

			foreach (MessageSendingObject sendingObject in messageSendingParent.SendingObjectsCollection)
			{
				sendingObject.Action = sendingObject.ActionForSendingCustomsDeclaration;
				var sender = sendingObject.CreateSender();

				if (sender.Send() != null)
				{
					messagesSent += 1;
				}
			}

			return messagesSent;
		}
	}
}
