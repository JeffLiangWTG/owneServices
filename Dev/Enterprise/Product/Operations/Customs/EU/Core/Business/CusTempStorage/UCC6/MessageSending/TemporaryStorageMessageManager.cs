using System.Collections.Specialized;
using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageMessageManager : MessageManager
	{
		public TemporaryStorageMessageManager(TemporaryStorageHeader header, TemporaryStorageMessageBuilder messageBuilder) : base(true)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.messageBuilder = Argument.NotNull(messageBuilder, nameof(messageBuilder));
		}

		readonly TemporaryStorageHeader header;
		readonly TemporaryStorageMessageBuilder messageBuilder;

		protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates => null;

		protected override BusinessObject Master => header;

		public void SendTemporaryStorageMessage(ISendsMessagesToCustoms sender)
		{
			var reasonsWeCantSend = new StringCollection();
			var warningsAboutSending = new StringCollection();
			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			SendMessage(sender,
				reasonsWeCantSend,
				warningsAboutSending,
				new MessageBuilderDelegate[] { GetMessageBuilder },
				"TemporaryStorage",
				replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		IMessageBuilder GetMessageBuilder(BusinessObject master)
		{
			return messageBuilder;
		}
	}
}
