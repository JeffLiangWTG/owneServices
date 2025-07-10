using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business;

class NctsHeaderMessageSendingObjectValidation : EU.NCTS.Business.NctsHeaderMessageSendingObjectValidation
{
	public NctsHeaderMessageSendingObjectValidation(AutoNctsHeaderMessageSendingObject parent) : base(parent)
	{
	}

	protected override void CheckMessageType()
	{
		base.CheckMessageType();

		var messageTypeInfo = Parent.MessageTypeInfo;

		if (Parent.NctsHeader.ArrivalMovementHeader is NctsArrivalMovementHeader arrivalMovementHeader)
		{
			var defaultMessageType = MessageSendingConfiguration.GetDefaultMessageType(arrivalMovementHeader);
			if (defaultMessageType != Parent.MessageType.ToString())
			{
				var message = defaultMessageType switch
				{
					NctsMessageTypeList.Codes.DESNOT => Res.GetString("952B3217-8470-4698-9A3C-F7D88C9D1539", "Current Status of Arrival Declaration only allows Message Type ‘E_DES_NOT’ (Arrival Notification Remarks)."),
					NctsMessageTypeList.Codes.DESREM => Res.GetString("1E718D44-B4E2-4516-929C-8563C47197FB", "Current Status of Arrival Declaration only allows Message Type ‘E_DES_REM’ (Unloading Remarks)."),
					_ => null,
				};

				if (message != null)
				{
					messageTypeInfo.AddMessageError(message);
				}
			}
		}
	}
}
