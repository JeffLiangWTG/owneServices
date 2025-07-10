using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.IT.NCTS.Business;

abstract class ResponseMessageCustomsStatusAnalyzer<TResponseMessage> : IResponseMessageCustomsStatusAnalyzer
{
	protected ResponseMessageCustomsStatusAnalyzer(NctsDepartureMovementHeader movementHeader, string messageType)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		this.messageType = Argument.NotNullOrEmpty(messageType, nameof(messageType));
	}

	#region Abstract

	protected abstract ZString GetStatusFromResponseMessage(TResponseMessage responseMessage);

	protected abstract bool IsValidResponseMessage(EDIMessage response);

	protected abstract TResponseMessage GetResponseMessage(EDIMessage response);

	#endregion

	#region IResponseMessageCustomsStatusAnalyzer

	bool IResponseMessageCustomsStatusAnalyzer.TryDetermineCustomsStatusBasedOnLatestMessageSent(out ZString customsStatus)
	{
	 customsStatus = ZString.Empty;
	 var sessionGuid = GetLatestSentSessionGuid();
		if (sessionGuid.IsEmpty)
		{
			return false;
		}

		var relatedMessages = GetAllRelatedResponseMessages(sessionGuid);
		var relatedStatus = relatedMessages
			.Select(msg => GetStatusFromResponseMessage(msg))
			.Where(status => !status.IsEmpty);
		var relatedValues = relatedStatus
			.Select(status => NctsDepartureSafeCustomsStatusSetter.LookupCustomsStatusValue(status))
			.Where(val => val != NctsDepartureSafeCustomsStatusSetter.CustomsStatusValueInvalid)
			.ToList();

		if (relatedValues.IsNullOrEmpty())
		{
			return false;
		}

		customsStatus = NctsDepartureSafeCustomsStatusSetter
			.GetCustomsStatusCodesForValue(relatedValues.Max())
			.FirstOrDefault();

		return true;
	}

	#endregion

	#region Implementation

	ZGuid GetLatestSentSessionGuid()
	{
		var message = movementHeader.Messages.GetLastMessage(ApplicationCodeList.Codes.ITCustomsXTrade, messageType);
		return message?.Interchange?.EI_SessionGUID ?? ZGuid.Empty;
	}

	IEnumerable<TResponseMessage> GetAllRelatedResponseMessages(ZGuid sessionGuid)
	{
		return movementHeader.Messages
			.Where(msg => IsValidResponse(msg, sessionGuid))
			.Select(msg => GetResponseMessage(msg));
	}

	bool IsValidResponse(EDIMessage responseMessage, ZGuid sessionGuid)
	{
		return responseMessage is not null
			&& responseMessage.Interchange?.EI_SessionGUID == sessionGuid
			&& responseMessage.EM_Status == EDIMessageStatusList.Codes.Received
			&& IsValidResponseMessage(responseMessage);
	}

	readonly NctsDepartureMovementHeader movementHeader;
	readonly string messageType;

	#endregion
}
