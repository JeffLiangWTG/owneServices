using CargoWise.Customs.IE.MessageContracts.Interfaces.AIS.Common;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RD415DepositRefundDetailsProvider : IDepositRefundDetails
	{
		public RD415DepositRefundDetailsProvider(DepositRefundApplicationMessageSendingAction sendingAction)
		{
			this.sendingAction = sendingAction;
		}
		readonly DepositRefundApplicationMessageSendingAction sendingAction;

		public bool ImportedGoodsDischarged => sendingAction.ImportedGoodsDischarged;

		public decimal OutstandingbalanceOnImportedGoods => sendingAction.OutstandingBalance;

		public decimal AmountOfDepositRefundClaim => sendingAction.AmountOfDepositRefund;

		public string PayerEORIForRefund => sendingAction.PayerEori;
	}
}
