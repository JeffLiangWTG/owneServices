using System;
using CargoWise.Customs.FR.MessageDefinitions.TP5.Interfaces;

namespace Enterprise.Customs.FR.NCTS.Messaging.TP5
{
	public class GuaranteeQueryWrapper : IGuaranteeQuery
	{
		public GuaranteeQueryWrapper(TP5MessageSendingObject sendingObject)
		{
			this.sendingObject = sendingObject;
		}

		readonly TP5MessageSendingObject sendingObject;

		public static GuaranteeQueryWrapper New(TP5MessageSendingObject sendingObject) => sendingObject == null ? null : new GuaranteeQueryWrapper(sendingObject);

		public string QueryIdentifier => sendingObject.QueryIdentifier;

		public DateTime? PeriodFromDate => sendingObject.PeriodFrom.IsValid ? sendingObject.PeriodFrom.ToDateTime() : null;

		public DateTime? PeriodToDate => sendingObject.PeriodTo.IsValid ? sendingObject.PeriodTo.ToDateTime() : null;
	}
}
