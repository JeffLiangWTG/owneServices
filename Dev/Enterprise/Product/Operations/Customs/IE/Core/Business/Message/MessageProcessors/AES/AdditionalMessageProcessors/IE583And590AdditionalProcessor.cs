using CargoWise.Customs.IE.MessageDefinitions;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business.Message.MessageProcessors.AdditionalMessageProcessors.AES
{
	public class IE583And590AdditionalProcessor : IAdditionalMessageProcessing
	{
		public void Process(Enterprise.Messaging.Business.EDIMessage originalMessage, Enterprise.Messaging.Business.EDIMessage targetMessage, ITransaction transactionProvider)
		{
			if (targetMessage.EM_LinkedObject is CusExitReport exitReport)
			{
				exitReport.CER_MessageStatus = Common.EU.LogicalStatusList.Codes.Accepted;
			}
		}
	}
}
