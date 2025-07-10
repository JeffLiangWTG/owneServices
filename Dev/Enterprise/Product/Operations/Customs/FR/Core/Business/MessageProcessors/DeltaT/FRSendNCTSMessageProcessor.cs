using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.FR.Business.MessageSending;
using Enterprise.Customs.FR.Business.NCTS;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.FR.Business.MessageProcessors
{
	public class FRSendNCTSMessageProcessor : EU.NCTS.Business.AutoSendNCTSMessageProcessor
	{
		public FRSendNCTSMessageProcessor(NctsHeader nctsHeader) : base(nctsHeader)
		{
		}

		protected new NctsHeader Header => (NctsHeader)base.Header;

		protected override ZString MessageDescription => (NoResString)"Delta T (NCTS)";

		protected override ZBool SendNctsMessageCore(INotifications notifications)
		{
			var rule = AutoSendNctsMessageRules.FirstOrDefault(x => x.CanSendMessage(Header));
			if (rule != null)
			{
				var sent = MessageSender.CreateMessage(Header, new Customs.Business.SendsMessagesToCustomsShutterUpperer(), rule.NewMessageFunction);
				var reference = new ZString(rule.NewMessageFunction.Code).Right(3);
				if (sent)
				{
					Header.Logs.AddNew(AutoEvents.MessageSent, reference);
				}
				else
				{
					notifications.AddError($"FR AutoNCTSMessage failed to send for header: {Header.JobNumber}, Message Type: {reference}.");
					Header.Logs.AddNew(AutoEvents.MessageGenerationFailed, reference);
				}
				return sent;
			}
			return false;
		}

		protected virtual INctsMessageSender MessageSender => messageSender ?? (messageSender = new NctsMessageSender());
		INctsMessageSender messageSender;

		protected IAutoSendNCTSMessageRule[] AutoSendNctsMessageRules => AutoSendNctsMessages.ToArray();

		protected virtual IEnumerable<IAutoSendNCTSMessageRule> AutoSendNctsMessages
		{
			get
			{
				yield return new FromNEMTo15FRule();
			}
		}
	}
}
