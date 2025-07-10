using System.Collections.Specialized;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class ManifestMessageManager : MessageManager
	{
		public ManifestMessageManager(AsycudaManifestHeader header, ILMAN170MessageBuilder messageBuilder) : base(true)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.messageBuilder = Argument.NotNull(messageBuilder, nameof(messageBuilder));
		}

		public void SendMessage(ISendsMessagesToCustoms sender)
		{
			var reasonsWeCantSend = new StringCollection();
			var warningsAboutSending = new StringCollection();
			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			SendMessage(sender,
				reasonsWeCantSend,
				warningsAboutSending,
				new MessageBuilderDelegate[] { GetMessageBuilder },
				"ILManifest",
				replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates => null;

		protected override BusinessObject Master => header;

		IMessageBuilder GetMessageBuilder(BusinessObject master)
		{
			return messageBuilder;
		}

		readonly AsycudaManifestHeader header;
		readonly ILMAN170MessageBuilder messageBuilder;
	}
}
