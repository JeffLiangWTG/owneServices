using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.DE.Business
{
	public class ExitNotificationMessageSendingActionValidation : ZValidation
	{
		public ExitNotificationMessageSendingActionValidation(ExitNotificationMessageSendingAction parent)
			: base(parent)
		{
			this.parent = parent;
		}
		readonly ExitNotificationMessageSendingAction parent;

		public override Type AutoValidationType => typeof(ExitNotificationMessageSendingActionValidation);

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

		public void ValidateIntendedExitCustomsOffice()
		{
			ValidateCalculatedProperty(parent.IntendedExitCustomsOfficeInfo);
		}

		protected void CheckIntendedExitCustomsOffice()
		{
			if (parent.Redirection)
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.IntendedExitCustomsOfficeInfo);
				ListValidation.MessageErrorIfInvalidCode(parent.IntendedExitCustomsOfficeInfo);
			}
		}
	}
}
