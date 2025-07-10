using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Business.MessageProcessors.ErrorReporting;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.CA.Business.MessageProcessors
{
	public class CACustomsMessageProcessor : ApplicationTypeMessageProcessor
	{
		public CACustomsMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string ApplicationCodeCore
		{
			get { return EDIMessage.ApplicationCodes.CACustoms; }
		}

		protected override string MessageFriendlyNameCore
		{
			get { return Res.GetString("E546E201-3550-4B93-8D43-C2E81B618ECA", "CA Customs"); }
		}

		protected override void ProcessMessageCore(Enterprise.Messaging.Business.EDIMessage message)
		{
			try
			{
				var messageProcessors = GetMessageProcessors(message.EM_MessageType).Where(x => x != null);
				if (messageProcessors.Any())
				{
					foreach (var processor in messageProcessors)
					{
						processor.ProcessMessage(message);
					}
				}
				else
				{
					message.EM_Status = EDIMessage.Status.Failed;
					Logger.Log("Can't find a valid processor for this message.");
				}
			}
			catch (MessageProcessorException ex)
			{
				message.EM_Status = EDIMessage.Status.Failed;
				Logger.Log("Processing message failed : " + ex.Message);
			}
		}

		IEnumerable<CustomsMessageProcessor> GetMessageProcessors(ZString messageType)
		{
			if (messageType == MessageTypeList.Codes.CARMDailyNotice)
			{
				yield return new CARMDailyNoticeMessageProcessor(Logger);
			}
			else if (messageType == MessageTypeList.Codes.CARMStatementOfAccount)
			{
				yield return new CARMStatementOfAccountMessageProcessor(Logger);
			}
		}
	}
}
