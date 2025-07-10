using Enterprise.Customs.KR.Messaging;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Customs.KR.Business
{
	[CodeAlive("An instance is created via reflection using the attribute MessageType")]
	[MessageType(Constants.EDIInterchangeType.DLT)]
	public class MessageDLTProcessor : IMessageProcessor
	{
		public void Process(EDIMessage message)
		{
			var messageData = message.EM_MessageData.ToUTF8().Split('\n');
			foreach (var item in messageData)
			{
				if (!string.IsNullOrWhiteSpace(item) && !string.IsNullOrEmpty(item))
				{
					var cusPollingTransaction = message.Factory.New<CusPollingTransaction>();
					cusPollingTransaction.CPT_ParentID = message.PK;
					cusPollingTransaction.CPT_ParentTableCode = message.TablePrefix;
					cusPollingTransaction.CPT_NumberOfAttempts = 1;
					cusPollingTransaction.CPT_Status = CusPollingTransactionStatusList.Codes.Opened;

					var transaction = item.Trim().Split(',');
					if (transaction.Length == 2)
					{
						cusPollingTransaction.CPT_TransactionID = transaction[0];
						cusPollingTransaction.CPT_Reference = transaction[1].Right(3);
					}
				}
			}
		}
	}
}
