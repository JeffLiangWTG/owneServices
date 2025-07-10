using System;
using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.NCTS.Business;

sealed class NctsDeparturePhase5CustomsStatusAnalyzer
{
	public NctsDeparturePhase5CustomsStatusAnalyzer(NctsDepartureMovementHeader movementHeader)
	{
		this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		if (!movementHeader.IsPhase5)
		{
			throw new ArgumentException("The provided NctsDepartureMovementHeader object must be configured for a Phase 5. Ensure that 'IsPhase5' is set to true.");
		}
	}

	public bool TryDetermineCustomsStatusBasedOnLastMessageSent(out ZString customsStatus)
	{
		customsStatus = ZString.Empty;
		foreach (var msgType in messageTypesToEvaluate)
		{
			if (TryDetermineCustomsStatusBasedOnLatestMessageSent(msgType, out customsStatus))
			{
				return true;
			}
		}

		return false;
	}

	#region Implementation

	readonly NctsDepartureMovementHeader movementHeader;

	bool TryDetermineCustomsStatusBasedOnLatestMessageSent(ZString msgType, out ZString customsStatus)
	{
		customsStatus = ZString.Empty;
		return ResponseMessageCustomsStatusAnalyzerFactory.CreateAnalyzerForMessageType(movementHeader, msgType)
			.TryDetermineCustomsStatusBasedOnLatestMessageSent(out customsStatus);
	}

	/// <summary>
	/// This array consists of message types which should be evaluated while determining the customs status on the
	/// basis of messages received up till now. Lower indexed message types have a higher priority of evaluation.
	/// </summary>
	readonly string[] messageTypesToEvaluate =
	{
		IT.Business.EDIMessageTypeList.Codes.IrildesResponse,
		IT.Business.EDIMessageTypeList.Codes.NewDeclaration
	};

	#endregion
}
