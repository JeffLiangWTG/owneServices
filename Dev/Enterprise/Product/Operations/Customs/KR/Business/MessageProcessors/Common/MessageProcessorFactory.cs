using System;
using Enterprise.BatchProcessor;
using Enterprise.Messaging.MessageProcessors;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class MessageProcessorFactory : ApplicationTypeMessageProcessor
	{
		public MessageProcessorFactory(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.KRCustoms;

		protected override string MessageFriendlyNameCore => Res.GetString("{663BF396-1149-4DFC-A91B-6244373D29F6}", "KR Customs Response Messages");

		protected override void ProcessMessageCore(Enterprise.Messaging.Business.EDIMessage ediMessage)
		{
			var message = ediMessage as EDIMessage
				?? throw new InvalidOperationException("The EDIMessage passed in should be of type KR.Business.EDIMessage");

			if (message.EM_MessageType == EDIInterchangeType.SSR)
			{
				message.EM_Status = EDIMessage.Status.Received;
			}
			else
			{
				var processor = new MessageProcessorProvider().GetProcessor(message.EM_MessageType);
				if (processor != null)
				{
					processor.Process(message);
					message.EM_Status = EDIMessage.Status.Received;
				}
			}
		}
	}
}
