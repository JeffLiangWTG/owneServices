using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RD415AdditionalInformationProvider : IRD415AdditionalInformation
	{
		public RD415AdditionalInformationProvider(DepositRefundApplicationMessageSendingAction sendingAction)
		{
			this.sendingAction = sendingAction;
		}
		readonly DepositRefundApplicationMessageSendingAction sendingAction;

		public string PeriodDischarge => sendingAction.PeriodForDischarge.ToString();

		public string RateOfYield => sendingAction.RateOfYield;
	}
}
