using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RD415AmountsHeldDepositProvider : IAmountsHeldDeposit
	{
		public RD415AmountsHeldDepositProvider(DepositRefundApplicationMessageSendingAction sendingAction)
		{
			this.sendingAction = sendingAction;
		}
		readonly DepositRefundApplicationMessageSendingAction sendingAction;

		public decimal CustomsDutyHeldDeposit => sendingAction.CustomsDuty;

		public decimal VATHeldDeposit => sendingAction.Vat;

		public decimal OtherDutiesHeldDeposit => sendingAction.OtherDuties;
	}
}
