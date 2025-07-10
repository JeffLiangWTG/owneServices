using System;
using CargoWise.Customs.IE.MessageContracts.AIS.Interfaces;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.AIS
{
	public class RD415HeaderTypeProvider : IRD415HeaderType
	{
		public RD415HeaderTypeProvider(DepositRefundApplicationMessageSendingAction sendingAction)
		{
			this.sendingAction = sendingAction;
		}
		readonly DepositRefundApplicationMessageSendingAction sendingAction;

		public string ApplicationReferenceId => AISOutboundEDIMessage.RD415ApplicationReferenceIdPlaceHolder;

		public DateTime Date => DateTimeProviderHelper.ConvertToUnspecifiedDateTimeKindIfPossible(ZDateTime.UtcNow, true);

		public string Applicant => sendingAction.EntryHeader.Declaration?.Declarant.GetEORI();
	}
}
