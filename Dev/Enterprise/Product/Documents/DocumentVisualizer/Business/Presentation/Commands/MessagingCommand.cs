using System;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Delivery;
using Enterprise.DocumentVisualizer.Integration;

namespace Enterprise.DocumentVisualizer.Presentation
{
	public abstract class MessagingCommand : ICommand, INotifiableDocumentInfoCreated
	{
		public abstract string Id { get; }

		public string Caption => CommandResources.Captions.GetCaptionForCommand(Id);

		public virtual object Image { get; }

		public bool IsEnabled => documentInfo != null && !hasMessageBeenSent;

		public bool IsVisible => true;

		public bool Invoke() => Invoke(null);

		public bool Invoke(MacroMap parameters)
		{
			if (documentInfo == null)
			{
				return false;
			}

			return Invoke(parameters, documentInfo);
		}

		public abstract bool Invoke(MacroMap map, IDocumentInfo info);

		IDocumentInfo documentInfo;
		bool hasMessageBeenSent;

		#region INotifiableDocumentInfoCreated members

		public void NotifyDocumentInfoCreated(IDocumentInfo documentInfo)
		{
			if (documentInfo == null
				|| this.documentInfo != null)
			{
				return;
			}

			this.documentInfo = documentInfo;

			var services = this.documentInfo.Services;
			var broker = services.Resolve<IEventBroker>();

			broker.GetEvent<MessageSentEvent>().Subscribe(_ => hasMessageBeenSent = true);
			broker.GetEvent<MessageWithdrawalSentEvent>().Subscribe(_ => hasMessageBeenSent = true);
		}

		#endregion

		protected IEDocsDeliveryParameters CreateEDocsDeliveryParameters(IDocumentDescriptor descriptor, IEDocsInstructions eDocsInstructions)
		{
			return new EDocsDeliveryParameters
			{
				DocumentName = descriptor.Name,
				DocumentTitle = descriptor.PrintInstructions?.Title,
				DocumentType = descriptor.DocumentType,
				AttachedFileName = descriptor.Name,
				BusinessObject = eDocsInstructions.Parent as IBusiness
			};
		}

		protected bool CheckAllowSendMessage(IServiceContainer services)
		{
			var security = services.Resolve<IDocumentSecurityService>();

			if (!security.CanSendMessage)
			{
				security.ShowSendMessageError();
				return false;
			}

			return true;
		}

		protected bool CheckHasChanges(IServiceContainer services, IDocument document, IVisualizerDocumentData documentData)
		{
			if ((document?.Data?.HasChanges ?? false)
				|| (documentData?.HasChanges ?? false))
			{
				services.Resolve<IUserNotificationService>().ShowMessage(
					Res.GetString("4e2afadd-2a72-4081-9e00-48bfc4301131", "Please save changes before sending message."),
					Res.GetString("49c1cab9-bf6a-43d5-a9b7-df1bec0da91d", "Unable to send message"));

				return true;
			}

			return false;
		}
	}
}
