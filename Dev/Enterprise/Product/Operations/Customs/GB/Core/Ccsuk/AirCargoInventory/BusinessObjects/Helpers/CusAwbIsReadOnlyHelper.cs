using System;
using System.Collections.Generic;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Registry;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects
{
	// Taken from document:	 HMRC CSD Team- import ASI table, Version 0.2 dated 18 Jan 2010

	public enum FieldsThatCanBeReadOnly
	{
		FlightNumber,
		FlightDate,
		AirportOfOrigin,
		AirportOfDestination,
		Sdc,
		Npx,
		Mass,
		Npr,
		Description,
		Agent,
		OnwardTawbCarrier,
		AwbNumber
	}

	public enum Actions
	{
		Delete,
		CW_Create,
		CW_AddChildAwb,
		CW_CanSplit,
		CW_C1Release,
		CW_CanCreateUnderbond,
		CW_CanCreateDeclaration,
		CW_EcStatusRelease,
		CW_SetEcStatusReleaseFlag,
		CW_UnSetEcStatusReleaseFlag
	}

	public class CusAwbIsReadOnlyHelper
	{
		public CusAwbIsReadOnlyHelper(ICcsukCusAwb awb)
		{
			this.awb = awb;
			InitialiseLists();
		}

		public ZBool IsActionAllowed(Actions action)
		{
			switch (action)
			{
				case Actions.CW_AddChildAwb:
					return !(awb is SplitConsignment) && !awb.HasSplits && !CACsLocked_ColumnTwo.Contains(awb.CustomsActionCode) && !CACsTAWB_ColumnThree.Contains(awb.CustomsActionCode);

				case Actions.CW_CanCreateDeclaration:
					return !awb.HasSplits && !CACsLocked_ColumnTwo.Contains(awb.CustomsActionCode) && !IsConsolidation && !ShipmentDescriptionCodeIsEuropean && IsAgentProfileOrEtsfWithOwnAgentNominated;

				case Actions.CW_CanCreateUnderbond:
					return !IsConsolidation && !CACsLocked_ColumnTwo.Contains(awb.CustomsActionCode) && !CACsTAWB_ColumnThree.Contains(awb.CustomsActionCode);

				case Actions.CW_CanSplit:
					if (GlbStaff.CurrentUser.IsSupportUser || GlbStaff.CurrentUser.GS_IsController)
					{
						return !(awb is SplitConsignment)
						&& !CACsLocked_ColumnTwo.Contains(awb.CustomsActionCode)
						&& !CACsTAWB_ColumnThree.Contains(awb.CustomsActionCode)
						&& IsNotEuropeanSdcOrRegistryIsSetToAllowSplittingOfEuropeanJobs;
					}
					else
					{
						return !(awb is SplitConsignment)
							&& !CACsLocked_ColumnTwo.Contains(awb.CustomsActionCode)
							&& !CACsTAWB_ColumnThree.Contains(awb.CustomsActionCode)
							&& IsNotEuropeanSdcOrRegistryIsSetToAllowSplittingOfEuropeanJobs
							&& IsNotAttachedToSubmittedChiefEntry;
					}

				case Actions.CW_Create:
					return notOnNetworkStatuses.Contains(awb.PresenceOnNetworkStatus);

				case Actions.Delete:
					return !awb.HasSplits && CodePropertiesMeanCanDelete;

				case Actions.CW_C1Release:
					return CanReleaseAwbForC1;

				case Actions.CW_EcStatusRelease:  // Print RRA
					return IsReadyForEcStatusReleaseProcessingByShed && awb.CustomsActionCode == CustomsStatusCodes.Codes._CargoWise_ERTS_EcStatusRelease && awb.OutTurns.TotalDelivered < awb.NumberOfPiecesReceived && awb.NumberOfPiecesReceived > 0 && awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent) < awb.NumberOfPiecesReceived;

				case Actions.CW_SetEcStatusReleaseFlag: // Set EC flag ready to print RRAs
					return IsReadyForEcStatusReleaseProcessingByShed && cACsOpen_ColumnOne.Contains(awb.CustomsActionCode) && awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent) == 0;

				case Actions.CW_UnSetEcStatusReleaseFlag: // Unset EC flag
					return IsReadyForEcStatusReleaseProcessingByShed && awb.CustomsActionCode == CustomsStatusCodes.Codes._CargoWise_ERTS_EcStatusRelease && awb.OutTurns.TotalDelivered == 0;
			}
			return false;
		}

		ZBool IsConsolidation
		{
			get { return (awb is CusMAWB && !((CusMAWB)awb).IsBasic); }
		}

		ZBool CodePropertiesMeanCanDelete
		{
			get
			{
				if (ShipmentDescriptionCodeIsEuropeanAndNotDelivered)// column 4
				{
					return true;
				}
				else if (cACsOpen_ColumnOne.Contains(awb.CustomsActionCode))
				{
					return true;
				}
				else if (CACsLocked_ColumnTwo.Contains(awb.CustomsActionCode))
				{
					return false;
				}
				else if (CACsTAWB_ColumnThree.Contains(awb.CustomsActionCode))
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}

		public ZBool IsFieldReadOnly(FieldsThatCanBeReadOnly fieldMnemonic)
		{
			return IsFieldReadOnly(fieldMnemonic, false);
		}

		public ZBool IsFieldReadOnly(FieldsThatCanBeReadOnly fieldMnemonic, bool skipCheckOnIdentifyingFieldsForFiddleFingers)
		{
			if (!skipCheckOnIdentifyingFieldsForFiddleFingers && ShouldKeyIdentifyingFieldsBeLockedToPreventFiddleFingers(fieldMnemonic))
			{
				return true;
			}
			else if (ShipmentDescriptionCodeIsEuropeanAndNotDelivered && awb.CustomsActionCode == CustomsStatusCodes.Codes._CargoWise_ERTS_EcStatusRelease)
			{
				return fieldMnemonic == FieldsThatCanBeReadOnly.Sdc;
			}
			else if (cACsOpen_ColumnOne.Contains(awb.CustomsActionCode))
			{
				return false;
			}
			else if (AllowEditingOfPreArrivalFlightDetails)
			{
				return fieldMnemonic != FieldsThatCanBeReadOnly.FlightDate && fieldMnemonic != FieldsThatCanBeReadOnly.FlightNumber;
			}
			else if (CACsLocked_ColumnTwo.Contains(awb.CustomsActionCode))
			{
				return fieldMnemonic != FieldsThatCanBeReadOnly.Npr;
			}
			else if (CACsTAWB_ColumnThree.Contains(awb.CustomsActionCode))
			{
				return !GetIsAmendAllowedForThroughAwbReleased(fieldMnemonic);
			}
			else
			{
				return false;
			}
		}

		bool ShouldKeyIdentifyingFieldsBeLockedToPreventFiddleFingers(FieldsThatCanBeReadOnly fieldMnemonic)
		{
			var shouldBeReadOnly = false;
			if (GBCustomsDataRegistry.Instance.CcsukMakeAwbNumberAndNamedPartyFieldsReadOnly.Value)
			{
				if (fieldMnemonic == FieldsThatCanBeReadOnly.Agent || fieldMnemonic == FieldsThatCanBeReadOnly.AwbNumber)
				{
					var presence = awb.PresenceOnNetworkStatus;
					if (presence == PresenceOnNetworkList.Codes.OnCommDb || presence == PresenceOnNetworkList.Codes.CompletedOnCcsUk || presence == PresenceOnNetworkList.Codes.ArchivedOnCcsuk || presence == PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection)
					{
						shouldBeReadOnly = true;  // Identifying numbers (AWB# and airport/shed), record is on network,	should be locked....
						var isShed = LicenceAndPimaHelper.IsFullShed(awb) || LicenceAndPimaHelper.IsFallbackShed(awb);
						if (GlbStaff.CurrentUser.IsSupportUser
							|| GlbStaff.CurrentUser.GS_IsController
							|| (fieldMnemonic == FieldsThatCanBeReadOnly.Agent && isShed)
							)
						{
							shouldBeReadOnly = false;  // ...except for administrators
						}
					}
				}
			}
			return shouldBeReadOnly;
		}

		//Column 3 in doc
		ZBool GetIsAmendAllowedForThroughAwbReleased(FieldsThatCanBeReadOnly fieldMnemonic)
		{
			switch (fieldMnemonic)
			{
				case FieldsThatCanBeReadOnly.AirportOfOrigin:
				case FieldsThatCanBeReadOnly.AirportOfDestination:
				case FieldsThatCanBeReadOnly.Mass:
				case FieldsThatCanBeReadOnly.Description:
				case FieldsThatCanBeReadOnly.OnwardTawbCarrier:
				case FieldsThatCanBeReadOnly.AwbNumber:
				case FieldsThatCanBeReadOnly.Npr:
					return true;
				default:
					return false;
			}
		}

		bool ShipmentDescriptionCodeIsEuropeanAndNotDelivered
		{
			get { return ShipmentDescriptionCodeIsEuropean && !IsAwbFullyDelivered; }
		}

		bool ShipmentDescriptionCodeIsEuropean
		{
			get { return (awb.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.CommunityStatusFromECAirport || awb.ShipmentDescriptionCode == ShipmentDescriptionCodes.Codes.CommunityStatusFromOutsideEC); }
		}

		bool IsAwbFullyDelivered
		{
			get { return awb.OutTurns != null && awb.NumberOfPiecesReceived > 0 && awb.OutTurns.TotalDelivered == awb.NumberOfPiecesReceived; }
		}

		void InitialiseLists()
		{
			cACsOpen_ColumnOne = new List<ZString>() {
														ZString.Empty,
														CustomsStatusCodes.Codes.EntryOrRequestCancelled
													};

			CACsLocked_ColumnTwo = new List<ZString>() {
														CustomsStatusCodes.Codes.EntryOrRequestAccepted,
														CustomsStatusCodes.Codes.CustomsQueriedDetained,
														CustomsStatusCodes.Codes.ReleasedForInterShedRemoval,
														CustomsStatusCodes.Codes.ClearedByCustoms,
														CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval,
														CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval,
														CustomsStatusCodes.Codes.SeizedDestroyedOrRetainedByCustoms,
														"CZ", // for ASIs only 
													   };

			CACsTAWB_ColumnThree = new List<ZString>() { CustomsStatusCodes.Codes.ThroughAirWaybillReleased };

			notOnNetworkStatuses = new List<ZString>() {
														PresenceOnNetworkList.Codes.NotOnCommDbDeleted,
														PresenceOnNetworkList.Codes.NotOnCommDb,
														PresenceOnNetworkList.Codes.SendPendingCheckYourCukServiceTask,
														PresenceOnNetworkList.Codes.NoInformationSendAnFsrWithUpdateToCheck
														};

			cAC_ReleasedForRemovalForC1 = new List<ZString>() {
																CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval,
																CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval,
															};
			CAC_ReleasedForRemovalForShedRra = new List<ZString>() {
																CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval,
																CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval,
																CustomsStatusCodes.Codes.ReleasedForInterShedRemoval,
																CustomsStatusCodes.Codes.ClearedByCustoms // if shed is advised of agent's clearance then they can release the goods to them.  Test  IMP/30, WI00040458
															};
		}

		public static IEnumerable<ZString> FinalisedCustomsStatusCodes
		{
			get
			{
				yield return CustomsStatusCodes.Codes.ClearedByCustoms;
				yield return CustomsStatusCodes.Codes.ReleasedForInterAirportRemoval;
				yield return CustomsStatusCodes.Codes.ReleasedForTranshipmentRemoval;
				yield return CustomsStatusCodes.Codes.ThroughAirWaybillReleased;
				yield return CustomsStatusCodes.Codes.ReleasedForInterShedRemoval;
				yield return CustomsStatusCodes.Codes.SeizedDestroyedOrRetainedByCustoms;
				yield return CustomsStatusCodes.Codes._CargoWise_ERTS_EcStatusRelease;
			}
		}

		List<ZString> notOnNetworkStatuses;
		List<ZString> cACsOpen_ColumnOne;
		internal List<ZString> CACsLocked_ColumnTwo;
		internal static List<ZString> CACsTAWB_ColumnThree;
		List<ZString> cAC_ReleasedForRemovalForC1;
		public List<ZString> CAC_ReleasedForRemovalForShedRra;
		readonly ICcsukCusAwb awb;

		ZBool CanReleaseAwbForC1
		{
			get
			{
				return awb.NumberOfPiecesReceived > 0       // Has some pieces
						&& !awb.CustomsActionCode.IsEmpty   // Has a CAC
															//&& LicenceAndPimaHelper.IsSimpleAgentProfile(Awb)  //TODO
						&& cAC_ReleasedForRemovalForC1.Contains(awb.CustomsActionCode)  // Has a CAC that means it can be released via a C1
						&& awb.NumberOfPiecesReceived <= awb.NumberOfPiecesExpected // Have not received more pieces than expected
						&& awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.AgentC1Event) < awb.NumberOfPiecesReceived;   // Has some pieces still to be released
			}
		}

		public ZBool CanReleaseAwbForShedRRA(GbEDIMessage receivedMessage)
		{
			return awb.NumberOfPiecesReceived > 0       // Has some pieces
					&& LicenceAndPimaHelper.ShedEnabled
					&& (LicenceAndPimaHelper.IsFullShed(awb) || LicenceAndPimaHelper.IsFallbackShed(awb) || ReleasePrintHelper.IsSentToRightShed(receivedMessage, awb))
					&& !awb.CustomsActionCode.IsEmpty   // Has a CAC
					&& CAC_ReleasedForRemovalForShedRra.Contains(awb.CustomsActionCode) // Has a CAC that means it can be released via an RRA
					&& awb.NumberOfPiecesReceived <= awb.NumberOfPiecesExpected // Have not received more pieces than expected
					&& awb.NumberOfPiecesReleasedSoFarCumulative(NumberOfPiecesReleasedHelper.ShedEvent) < awb.NumberOfPiecesReceived;  // Has some pieces still to be released
		}

		ZBool IsReadyForEcStatusReleaseProcessingByShed
		{
			get
			{
				return CcsukUtilities.IsEuropeanShipmentDescriptionCode(awb)
						&& LicenceAndPimaHelper.IsFullShed(awb)
						&& awb.NumberOfPiecesReceived <= awb.NumberOfPiecesExpected; //no excess						
			}
		}

		public bool AwbIsProbablyOnNetwork
		{
			get
			{
				var p = awb.PresenceOnNetworkStatus;
				return p == PresenceOnNetworkList.Codes.ArchivedOnCcsuk || p == PresenceOnNetworkList.Codes.AssumedOnCommDbSentWithoutRejection || p == PresenceOnNetworkList.Codes.CompletedOnCcsUk || p == PresenceOnNetworkList.Codes.OnCommDb;
			}
		}

		bool AllowEditingOfPreArrivalFlightDetails
		{
			get
			{  // If the record is a pre-arrival, in status CA, then the shed needs to be able to edit the flight details to arrive it. Only allowed under these four criteria (agreed with Navinder Johal @ CSD 17/10/13)
				return awb.CustomsActionCode == CustomsStatusCodes.Codes.EntryOrRequestAccepted
					&& awb.IsPrearrival
					&& (LicenceAndPimaHelper.IsFullShed(awb) || LicenceAndPimaHelper.IsFallbackShed(awb))
					&& !awb.ProfileInfo.HasErrors()
					&& NominatedAgentIsETSFsOwnAgentAccordingToRegistry;
			}
		}

		bool NominatedAgentIsETSFsOwnAgentAccordingToRegistry
		{
			get
			{
				foreach (CredentialsSetting cred in GBCustomsDataRegistry.Instance.Credentials.GetFallBackValueAtAllLevels(awb.Branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty))
				{
					if (cred.IsCcskAgent && cred.Company == awb.AgentBadge)
					{
						return true;
					}
				}
				return false;
			}
		}

		bool IsNotAttachedToSubmittedChiefEntry
		{
			get { return !awb.HasEntryWithLodgedOrPrelodgedWithCustoms || awb.IsEntryCancelled; }
		}

		bool IsNotEuropeanSdcOrRegistryIsSetToAllowSplittingOfEuropeanJobs
		{
			// Either SDC is T or M, or the registry allows splitting of SDC=C/E
			get
			{
				var isSdcEorC = CcsukUtilities.IsEuropeanShipmentDescriptionCode(awb);
				return !isSdcEorC || (isSdcEorC && GBCustomsDataRegistry.Instance.CcsukAllowSplittingOfEcStatusJobs.Value);
			}
		}

		bool IsAgentProfileOrEtsfWithOwnAgentNominated
		{
			get
			{
				return LicenceAndPimaHelper.IsSimpleAgentProfile(awb)
					||
					(
						LicenceAndPimaHelper.IsFullShed(awb) // full shed
						&&
						LicenceAndPimaHelper.IsEtsfShed(awb)
						&&
						NominatedAgentIsETSFsOwnAgentAccordingToRegistry
					);
			}
		}
	}
}
