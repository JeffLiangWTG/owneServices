using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;

namespace Enterprise.Customs.IT.NCTS.Business.MessageSending.AidaXml;

abstract class NctsHeaderMessageSendingObjectStrategy : INctsHeaderMessageSendingObjectStrategy
{
	protected NctsHeaderMessageSendingObjectStrategy(NctsHeaderMessageSendingObject messageSendingObject)
	{
		this.messageSendingObject = Argument.NotNull(messageSendingObject, nameof(messageSendingObject));
		validMessageStatesForSending = [];
	}

	public bool DoesStatusAllowSending() => Header.StatusAllowsSending || DoesMessageTypeAllowSending();

	protected virtual bool DoesMessageTypeAllowSending()
	{
		if (Header.MovementHeader is not NctsDepartureMovementHeader movementHeader)
		{
			return false;
		}

		var messageState = new MessageState(movementHeader.BM_Phase, movementHeader.BM_MessageStatus, movementHeader.BM_CustomsStatus);
		return validMessageStatesForSending.Any(DoesMessageStateAllowSending);

		bool DoesMessageStateAllowSending(MessageState validMessageState) =>
			(validMessageState.PhaseStatus is null || validMessageState.PhaseStatus == messageState.PhaseStatus) &&
			(validMessageState.MessageStatus is null || validMessageState.MessageStatus == messageState.MessageStatus) &&
			(validMessageState.DepartureStatus is null || validMessageState.DepartureStatus == messageState.DepartureStatus);
	}

	protected void AddValidMessageStateForSending(string phaseStatus, string messageStatus, string departureStatus)
	{
		validMessageStatesForSending.Add(new MessageState(phaseStatus, messageStatus, departureStatus));
	}

	protected NctsHeader Header => messageSendingObject.NctsHeader;

	readonly NctsHeaderMessageSendingObject messageSendingObject;
	readonly List<MessageState> validMessageStatesForSending;

	record struct MessageState(string PhaseStatus, string MessageStatus, string DepartureStatus);
}
