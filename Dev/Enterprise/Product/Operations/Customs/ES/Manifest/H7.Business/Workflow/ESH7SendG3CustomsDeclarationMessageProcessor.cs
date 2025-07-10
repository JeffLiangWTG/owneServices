using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using NotificationType = CargoWise.ComponentModel.NotificationType;

namespace Enterprise.Customs.ES.Manifest.H7.Business
{
	public class ESH7SendG3CustomsDeclarationMessageProcessor : IProcessor
	{
		public ESH7SendG3CustomsDeclarationMessageProcessor(EU.H7.Business.AsycudaManifestHeader header)
		{
			Header = Argument.NotNull(header, nameof(header));
		}

		EU.H7.Business.AsycudaManifestHeader Header { get; }

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var sendingObjectParent = new G3MessageSendingObjectParent(Header);
			var messageSender = new G3MessageSender(sendingObjectParent);
			var messagesSent = messageSender.Send();

			if (messagesSent > 0)
			{
				notifications.Add(NotificationType.Information, $"Successfully sent G3 Customs Declaration for {Header.HumanReadableName}.");
			}
			else
			{
				notifications.AddError(Res.GetString("6b7f07ba-d769-4846-a399-e4ca389a866e", "Could not create G3 message for H7 Job."));
			}
		}
	}
}
