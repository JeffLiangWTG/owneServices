using System.Threading;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Business.Message.MessageBuilder;
using Enterprise.Messaging.MessageBuilders;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.IL.Business
{
	public class BasicMessageManager : MessageManager
	{
		public BasicMessageManager(EnterpriseBusinessObject header, ILMessageBuilderBase messageBuilder, string messageName) : base(true)
		{
			this.header = Argument.NotNull(header, nameof(header));
			this.messageBuilder = Argument.NotNull(messageBuilder, nameof(messageBuilder));
			this.messageName = Argument.NotNull(messageName, nameof(messageName));
		}

		public void SendMessage(ISendsMessagesToCustoms sender)
		{
			var reasonsWeCantSend = new System.Collections.Specialized.StringCollection();
			var warningsAboutSending = new System.Collections.Specialized.StringCollection();
			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			SendMessage(sender,
				reasonsWeCantSend,
				warningsAboutSending,
				new MessageBuilderDelegate[] { GetMessageBuilder },
				messageName,
				replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates => null;

		protected override BusinessObject Master => header;

		IMessageBuilder GetMessageBuilder(BusinessObject master)
		{
			return messageBuilder;
		}

		readonly EnterpriseBusinessObject header;
		readonly ILMessageBuilderBase messageBuilder;
		readonly string messageName;
	}
}
