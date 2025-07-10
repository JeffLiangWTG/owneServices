using System;
using System.Collections.Specialized;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration
{
	public class ConsolMessageManager : MessageManager
	{
		public ConsolMessageManager(CustomsExportConsolIntegrationWrapper consolWrapper, GbDes242MessageFunction how, ISendsMessagesToCustoms sendMessagesToCustoms)
			: base(true)
		{
			this.consolWrapper = consolWrapper;
			this.how = how;
			this.sendMessagesToCustoms = sendMessagesToCustoms;
		}

		public bool SendToRecipient(CancellationToken token)
		{
			var reasonsWeCantSend = new StringCollection();
			var warningsAboutSending = new StringCollection();
			SendMessage(sendMessagesToCustoms,
				reasonsWeCantSend,
				warningsAboutSending,
				new MessageBuilderDelegate[] { GetMessageBuilder }, SubFunctionCode, token);
			return reasonsWeCantSend.Count == 0 && warningsAboutSending.Count == 0;
		}

		protected virtual string SubFunctionCode => how.SubFunctionCode;

		protected virtual IMessageBuilder GetMessageBuilder(BusinessObject master)
		{
			return new ConsolMessageBuilder(consolWrapper, how);
		}

		protected override BusinessObject Master => consolWrapper;

		protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates => throw new NotSupportedException();

		protected readonly CustomsExportConsolIntegrationWrapper consolWrapper;
		protected readonly GbDes242MessageFunction how;
		readonly ISendsMessagesToCustoms sendMessagesToCustoms;
	}
}
