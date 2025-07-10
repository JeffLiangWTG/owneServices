using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class ExitSummaryMessageSendingActionValidation : ZValidation
	{
		public ExitSummaryMessageSendingActionValidation(ExitSummaryMessageSendingAction parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly ExitSummaryMessageSendingAction parent;

		public override Type AutoValidationType => typeof(ExitSummaryMessageSendingActionValidation);

		public override void ValidateAll()
		{
			ValidateMessageType();
		}

		public void ValidateMessageType()
		{
			ValidateCalculatedProperty(parent.MessageTypeInfo);
		}

		protected void CheckMessageType()
		{
			ListValidation.MessageErrorIfInvalidCode(parent.MessageTypeInfo);
		}
	}
}
