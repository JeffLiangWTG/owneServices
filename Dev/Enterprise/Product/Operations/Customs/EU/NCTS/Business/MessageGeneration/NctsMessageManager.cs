using System.Collections.Specialized;
using System.Threading;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business.MessageBuilders;
using Enterprise.Messaging.MessageBuilders;

namespace Enterprise.Customs.EU.NCTS.Business.MessageGeneration
{
	public class NctsMessageManager : MessageManager
	{
		public NctsMessageManager(NctsHeader nctsHeader, IMessageGenerator<NctsHeader> generator)
			: base(true)
		{
			this.nctsHeader = nctsHeader;
			this.generator = generator;
		}

		protected override BusinessObject Master
		{
			get { return this.nctsHeader; }
		}

		protected override SendAmendmentIfNeededDelegate[] SendAmendmentIfNeededDelegates
		{
			get { return null; }
		}

		public bool SendNctsMessage(ISendsMessagesToCustoms sender)
		{
			var reasonsWeCantSend = new StringCollection();
			var warningsAboutSending = new StringCollection();
			var replaceThisTokenEventuallyQuestionMarkExclamationMark = CancellationToken.None;
			return SendMessage(sender,
				reasonsWeCantSend,
				warningsAboutSending,
				new MessageBuilderDelegate[] { GetMessageBuilder }, "NctsDeclaration",
				replaceThisTokenEventuallyQuestionMarkExclamationMark);
		}

		IMessageBuilder GetMessageBuilder(BusinessObject master)
		{
			return new NctsMessageBuilder(master as NctsHeader, generator);
		}

		protected readonly IMessageGenerator<NctsHeader> generator;
		readonly NctsHeader nctsHeader;
	}
}
