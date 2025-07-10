using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	class ConsignmentStatusUpdater
	{
		public ConsignmentStatusUpdater(ICcsukCusAwb awb)
		{
			this.awb = awb;
		}

		internal bool Update(ZString newCustomsActionCode, ZDateTime newCustomsActionDate, ZString agentReference, ZString newCustomsActionText)
		{
			var couldUpdateOk = false;
			if (ConsignmentsStatusCanBeUpdatedTo(newCustomsActionCode))
			{
				if (!newCustomsActionText.IsEmpty)
				{
					awb.LatestCustomsActionText = newCustomsActionText;
				}
				if (IsDeletingApprovedRemoval(newCustomsActionCode, awb.CustomsActionCode))
				{
					NumberOfPiecesReleasedHelper.ResetAllReleaseCountsToZeroUponCxStatusUpdate(awb);  // When CT etc --> CX then wipe release counts so new CAC can allow releases
					MarkRelevantOutturnsAsEditableAgain();
				}
				if (IsDeletingRemoval(newCustomsActionCode, awb.CustomsActionCode))
				{
					ProcessCX(newCustomsActionCode);
				}
				awb.SetCustomsActionCode(newCustomsActionCode, newCustomsActionDate);
				UpdateCusEntryHeaderToClearedIfNecessary(newCustomsActionCode, agentReference);
				couldUpdateOk = true;
			}
			else if (newCustomsActionCode == awb.CustomsActionCode)  //e.g. FSN after CUSRES for CT/CW/CB
			{
				couldUpdateOk = true;
			}
			else
			{
				var bizO = awb as EnterpriseBusinessObject;
				if (bizO != null)
				{
					bizO.Logs.AddNew(Events.CustomsEntryStatus, string.Format("Status {0} cannot be updated to {1} via FSN", awb.CustomsActionCode, newCustomsActionCode), ZDateTimeOffset.Now);
				}
			}
			CcsukUtilities.UpdatePresenceToYesIfCurrentlyTransientOrNegative(awb);
			return couldUpdateOk;
		}

		void ProcessCX(ZString newCustomsActionCode)
		{
			MarkUnderbondAsCancelledIfNewStatusIsCX(newCustomsActionCode);
			new EDocsDeleter(awb).DeleteOldEdocs();
		}

		void MarkUnderbondAsCancelledIfNewStatusIsCX(ZString newCustomsActionCode)
		{
			// Cannot look-up underbond by serial number (U0001234) as this is not returned for ISR or CHIEF requests
			// Cannot use U-number or anything from commmon access reference because this will not come back in CX message
			// Instead find most recent underbond in ACK or PND status, for the split.  
			var underbond = new UnderbondFinder(awb).FindUnderbond(newCustomsActionCode);
			if (underbond != null)
			{
				underbond.C4_Status = EDIMessage.Status.Cancelled;
			}
		}

		bool ConsignmentsStatusCanBeUpdatedTo(ZString newCAC)
		{
			var currentCAC = awb.CustomsActionCode;
			return currentCAC.IsEmpty
					|| (CacIsTransient(currentCAC) && !newCAC.IsEmpty)
					|| IsDeletingApprovedRemoval(newCAC, currentCAC);  // status 3 can be revoked by customs if you write to them.  There will be no outbound message to request cancellation. 
		}

		bool IsDeletingApprovedRemoval(ZString newCAC, ZString currentCAC)
		{
			return CacIsRemovalApproved(currentCAC) && newCAC == CustomsStatusCodes.Codes.EntryOrRequestCancelled;
		}

		bool IsDeletingRemoval(ZString newCAC, ZString currentCAC)
		{
			// Something --> CX
			return currentCAC != CustomsStatusCodes.Codes.EntryOrRequestCancelled && newCAC == CustomsStatusCodes.Codes.EntryOrRequestCancelled;
		}

		bool CacIsTransient(string cac)
		{
			return cac == CustomsStatusCodes.Codes.CustomsQueriedDetained ||
					cac == CustomsStatusCodes.Codes.EntryOrRequestAccepted ||
					cac == CustomsStatusCodes.Codes.EntryOrRequestCancelled;
		}

		bool CacIsRemovalApproved(string cac)
		{
			return cac == CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval ||
					cac == CustomsStatusCodes.Codes.ReleasedForInterShedRemoval ||
					cac == CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval;
		}

		void UpdateCusEntryHeaderToClearedIfNecessary(ZString statusCode, ZString agentReference)
		{
			if (statusCode == CustomsStatusCodes.Codes.ClearedByCustoms)
			{
				var cusEntryHeader = new FindCusEntryHeaderFromAwbAndAgentReferenceHelper(awb, agentReference).GetEntryFromAwbAsBestWeCan();
				if (cusEntryHeader != null)
				{
					cusEntryHeader.CH_EntryStatus = EntryStatusList.Codes.Clear;
					cusEntryHeader.CH_Status = MessageStatusList.Codes.OK;

					if (cusEntryHeader.Declaration.JE_ApplicationCode != Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services)
					{
						GbExtensionHelpers.SendQueriesForCleared(cusEntryHeader);
					}
				}
			}
		}

		void MarkRelevantOutturnsAsEditableAgain()
		{
			foreach (var outTurn in awb.OutTurns)
			{
				if (outTurn.SplitReferenceToWhichThisPertains == awb.SplitReference
					&& outTurn.IsReleasedAlready
					&& !outTurn.IsDelivered) // NB, before customs approve a request to cancel, they query (with FSR/FSA) our record and will NOT approve if some pieces have already been delivered.  So this is just precautionary.
				{
					outTurn.IsReleasedAlready = false;
					outTurn.IsBeingReleasedNow = false;
				}
			}
		}
		readonly ICcsukCusAwb awb;
	}
}
