using System;
using Enterprise.Customs.Common.AU.CMR;

namespace Enterprise.Client.UPE.Business.CMR
{
	internal static class UPECMRCodes
	{
		internal static string[] CMREntryStatusCodesForCRCompleted
		{
			get
			{
				if (fCMREntryStatusCodesForCRCompletedDecCompleted == null)
				{
					fCMREntryStatusCodesForCRCompletedDecCompleted = new string[]
					{
						CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased,
						CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed,
						CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus,
						CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn
					};
				}
				return fCMREntryStatusCodesForCRCompletedDecCompleted;
			}
		}
		[ThreadStatic]
		static string[] fCMREntryStatusCodesForCRCompletedDecCompleted;

		internal static string[] CMREntryStatusCodesForCRCompletedDecCustomsBonding
		{
			get
			{
				if (fCMREntryStatusCodesForCRCompletedDecCustomsBonding == null)
				{
					fCMREntryStatusCodesForCRCompletedDecCustomsBonding = new string[]
					{
						CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms,
						CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine
					};
				}
				return fCMREntryStatusCodesForCRCompletedDecCustomsBonding;
			}
		}
		[ThreadStatic]
		static string[] fCMREntryStatusCodesForCRCompletedDecCustomsBonding;

		internal static string[] CMREntryStatusCodesForCRCompletedDecQuarantineHold
		{
			get
			{
				if (fCMREntryStatusCodesForCRCompletedDecQuarantineHold == null)
				{
					fCMREntryStatusCodesForCRCompletedDecQuarantineHold = new string[]
						{
							CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement,
							CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement
						};
				}
				return fCMREntryStatusCodesForCRCompletedDecQuarantineHold;
			}
		}
		[ThreadStatic]
		static string[] fCMREntryStatusCodesForCRCompletedDecQuarantineHold;

		internal static string[] CMREntryStatusCodesWithAdditionalInfo
		{
			get
			{
				if (fCMREntryStatusCodesWithAdditionalInfo == null)
				{
					fCMREntryStatusCodesWithAdditionalInfo = new string[]
						{
							CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl,
							CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation
						};
				}
				return fCMREntryStatusCodesWithAdditionalInfo;
			}
		}
		[ThreadStatic]
		static string[] fCMREntryStatusCodesWithAdditionalInfo;

		internal static string[] CMRMessageStatusCodesForCRPending
		{
			get
			{
				if (fCMRMessageStatusCodesForCRPending == null)
				{
					fCMRMessageStatusCodesForCRPending = new string[]
						{
							CMRBaseStatuses.Codes.AwaitingResponseToAmendment,
							CMRBaseStatuses.Codes.AwaitingResponseToOriginal,
							CMRBaseStatuses.Codes.AwaitingResponseToWithdrawal
						};
				}
				return fCMRMessageStatusCodesForCRPending;
			}
		}
		[ThreadStatic]
		static string[] fCMRMessageStatusCodesForCRPending;

		internal static string[] CMRMessageStatusCodesForCRIntervention
		{
			get
			{
				if (fCMRMessageStatusCodesForCRIntervention == null)
				{
					fCMRMessageStatusCodesForCRIntervention = new string[]
						{
							CMRBaseStatuses.Codes.AmendmentRejected,
							CMRBaseStatuses.Codes.OriginalRejected,
							CMRBaseStatuses.Codes.WithdrawalRejected
						};
				}
				return fCMRMessageStatusCodesForCRIntervention;
			}
		}
		[ThreadStatic]
		static string[] fCMRMessageStatusCodesForCRIntervention;
	}
}
