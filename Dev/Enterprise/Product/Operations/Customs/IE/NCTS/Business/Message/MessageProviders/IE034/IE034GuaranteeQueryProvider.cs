using System;
using CargoWise.Customs.IE.MessageContracts.NCTS.Interfaces;

namespace Enterprise.Customs.IE.NCTS.Business
{
	public class IE034GuaranteeQueryProvider : IIE034GuaranteeQuery
	{
		public IE034GuaranteeQueryProvider(QueryOnGuaranteeSendingAction sendingAction)
		{
			this.sendingAction = sendingAction;
		}
		readonly QueryOnGuaranteeSendingAction sendingAction;

		public string QueryIdentifier => sendingAction.QueryIdentifier;

		public DateTime PeriodFromDate => sendingAction.PeriodFrom.IsValid ? sendingAction.PeriodFrom.ToDateTime() : DateTime.MinValue;

		public DateTime PeriodToDate => sendingAction.PeriodTo.IsValid ? sendingAction.PeriodTo.ToDateTime() : DateTime.MinValue;
	}
}
