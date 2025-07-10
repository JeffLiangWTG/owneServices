using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.LogWalker;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	[Serializable]
	public class CMRCARSTMessageLogSubscriber : LogSubscriber
	{
		public override bool IsRequired
		{
			get
			{
#if DEBUG
				if (Globals.IsTest && isAlwaysRequiredForTest)
				{
					return true;
				}
#endif
				return AUCustomsDataRegistry.Instance.DefaultDischargePremiseIDs.Count > 0;
			}
		}

		public override bool HasDynamicProperties => true;

#if DEBUG
		public bool isAlwaysRequiredForTest = true;
#endif

		protected override void ProcessLogQueueItems(IQueuedLog[] queuedLogs)
		{
			foreach (IQueuedLog queuedLog in queuedLogs)
			{
				CMRCARSTMessage carstMessage = queuedLog.Factory.Load<CMRCARSTMessage>(queuedLog.SJ_ParentID);
				if (carstMessage != null && carstMessage.EM_ApplicationCode == EDIMessage.ApplicationCodes.CMR &&
					carstMessage.EM_MessageType == CMRMessage.CMRMessageTypes.CARST &&
					IsMessageLinkedToSubUnderbondMovementMAWB(carstMessage))
				{
					ProcessCMRCARSTMessage(carstMessage, queuedLog);
				}
			}
		}

		ZString destination = "";

		void ProcessCMRCARSTMessage(CMRCARSTMessage cARST, IQueuedLog queuedLog)
		{
			CusMAWB mAWB = queuedLog.Factory.Load<CusMAWB>(cARST.EM_LinkUniqueID);
			GlbBranch branch = queuedLog.Factory.LoadFromNaturalKey<GlbBranch>(GlbBranchSchema.GB_Code, AUCustomsDataRegistry.Instance.BranchForAutomaticUnderbonds.Value);
			string errorMessage = "";
			if (mAWB == null)
			{
				errorMessage = "Linked MAWB is null";
			}
			else if (mAWB.Consol != null && mAWB.Consol.IsCoLoad)
			{
				errorMessage = mAWB.Consol.IsSendingOrReceivingForwarderGateway
					? "Consol is Gateway Co-Load"
					: "Consol is Co-Load";
			}

			if (string.IsNullOrEmpty(errorMessage) && branch == null)
			{
				errorMessage = "Invalid (or no) Branch for Automatic Underbonds specified in the registry, see Registry->Customs->Australia->Air Cargo->Underbond Movement Request.";
			}

			var destinationPremiseID = ZString.Empty;
			if (string.IsNullOrEmpty(errorMessage))
			{
				destinationPremiseID = GetDefaultDestinationPremiseID(mAWB, cARST, out errorMessage);
				if (string.IsNullOrEmpty(errorMessage))
				{
					CusUnderbond underbond = null;
					if (mAWB.AllUnderbonds.Count == 0)
					{
						var carstToDetermineDischargeMovement = GetFirstHouseLevelCARST(mAWB.ChildBills) ?? cARST;
						var isMoveFromDischarge = GetReleasePremiseInDestinationValue(carstToDetermineDischargeMovement) == "YES";

						ZString dischargePremiseID = "";
						if (!isMoveFromDischarge)
						{
							dischargePremiseID = GetDefaultPremiseID(AUCustomsDataRegistry.Instance.DefaultDischargePremiseIDs.Cast<DefaultPremiseID>(), cARST.FlightNumber, cARST.PortOfDischarge);
							errorMessage = dischargePremiseID.IsEmpty ? "No default Discharge Premise ID found" : "";
						}

						if (string.IsNullOrEmpty(errorMessage))
						{
							underbond = CreateUnderbond(mAWB, cARST, destinationPremiseID, dischargePremiseID, isMoveFromDischarge, branch);
						}
					}
					else if (AUCustomsDataRegistry.Instance.AllowMultipleDCLUnderbondRequest.Value && cARST.PremiseID != destinationPremiseID && !HasDuplicateMovement(mAWB, cARST, destinationPremiseID))
					{
						underbond = CreateUnderbond(mAWB, cARST, destinationPremiseID, ZString.Empty, true, branch);
					}

					if (underbond != null)
					{
						CusUnderbondUBMREQManager manager = new CusUnderbondUBMREQManager(underbond);
						EDIMessage[] messages = manager.GenerateOriginalMessages(underbond);

						// TODO: the following may be replaced by message.EM_GB = MAWB.CM_GB
						// This would overcome the problem of multiple AU brances, however at the time of making this change CM_GB is not set or used anywhere
						// When implementingt this take care that MAWB.Branch is an AU branch and not ademo company branch (MAWB may have been created by data import, what was the crrent branch at that time?)
						// Part of this change is to set CM_GB so it populates for future use
						foreach (EDIMessage message in messages)
						{
							message.EM_GB = branch.PK;
							message.EM_IsTestMessage = (bool)Env.Registry.RawRegistry.CMRTestMode.GetFallBackValueAtAllLevels(branch.Company.PK.ToGuid(), Guid.Empty, Guid.Empty);
						}

						if (messages.Length > 0)
						{
							manager.OnOriginalSent();
						}
					}
				}
			}

			if (!string.IsNullOrEmpty(errorMessage))
			{
				ReportError(errorMessage + " for CARST message:", cARST, branch);
			}
		}

		#region HelpMethods

		bool HasDuplicateMovement(CusMAWB mawb, CMRCARSTMessage cARST, ZString destinationPremiseID)
		{
			return mawb.AllUnderbonds.Cast<CusUnderbond>().Any(x => x.C4_MovementReason == CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination &&
						x.C4_ModeOfMovement == CMRUnderbondModeOfMovement.Codes.Road && x.C4_OriginPremiseID == cARST.PremiseID && x.C4_DestinationPremiseID == destinationPremiseID);
		}

		bool IsMessageLinkedToSubUnderbondMovementMAWB(CMRCARSTMessage cARST)
		{
			bool result = (
				cARST != null &&
				cARST.EM_Status == EDIMessage.Status.Received &&
				cARST.EM_LinkTable == CusMAWB.Schema.TableName &&
				cARST.IsAir &&
				cARST.GetFTXSegmentInfo("CONSOLIDATED STATUS") == "SUBUBMOV"
				);

			return result;
		}

		string CheckDestinations(CusHAWBCollection houseBills)
		{
			string result = "";

			if (houseBills.Count > 0)
			{
				string destination = houseBills[0].CS_RL_NKDestination;
				for (int i = 1; i < houseBills.Count; i++)
				{
					if (houseBills[i].CS_RL_NKDestination != destination)
					{
						result = "HAWB's have different destinations";
						break;
					}
				}
			}
			else
			{
				result = "No HAWB found";
			}

			return result;
		}

		CMRCARSTMessage GetFirstHouseLevelCARST(CusHAWBCollection houseBills)
		{
			CMRCARSTMessage result = null;

			foreach (CusHAWB hAWB in houseBills)
			{
				hAWB.Messages.Sort(EDIMessage.Schema.EM_SystemCreateTimeUtc, System.ComponentModel.ListSortDirection.Ascending);
				foreach (EDIMessage message in hAWB.Messages)
				{
					CMRCARSTMessage cARST = message as CMRCARSTMessage;
					if (cARST != null && GetReleasePremiseInDestinationValue(cARST) != "")
					{
						result = cARST;
						break;
					}
				}

				if (result != null)
				{
					break;
				}
			}

			return result;
		}

		ZString GetReleasePremiseInDestinationValue(CMRCARSTMessage cARST)
		{
			return cARST.GetFTXSegmentInfo("RELEASE PREMISE IN DESTINATION");
		}

		CusUnderbond CreateUnderbond(CusMAWB mAWB, CMRCARSTMessage cARST, ZString destinationPremiseID, ZString dischargePremiseID, ZBool isMoveFromDischarge, GlbBranch branch)
		{
			CusUnderbond result = mAWB.AllUnderbonds.AddNew();

			result.C4_ResponsiblePartyID = branch.Company.OrgProxy.PrimaryRegistrationNumber.Number.Left(result.C4_ResponsiblePartyIDInfo.MaxLength);
			result.C4_ParentTableCode = CusMAWBSchema.Constants.Prefix;
			result.C4_ParentID = mAWB.PK;
			result.C4_MovementReason = CMRUnderbondRequestCodes.Codes.UnpackLclAtDestination;
			result.C4_ModeOfMovement = CMRUnderbondModeOfMovement.Codes.Road;
			result.C4_OriginPremiseID = cARST.PremiseID;
			result.C4_DestinationPremiseID = destinationPremiseID;
			result.C4_IsMoveFromDischarge = isMoveFromDischarge;
			result.C4_DischargePremiseID = isMoveFromDischarge ? result.C4_OriginPremiseID : dischargePremiseID;
			result.C4_FlightNo = cARST.FlightNumber;
			result.C4_ArrivalDate = cARST.ArrivalDate;
			result.C4_PiecesManifested = (ZShort)mAWB.PiecesManifestedForAllHAWBs;

			return result;
		}

		ZString GetDefaultDestinationPremiseID(CusMAWB mawb, CMRCARSTMessage carst, out string errorMessage)
		{
			var result = ZString.Empty;
			var depotAddress = mawb.UnpackDepotAddress;
			if (depotAddress != null)
			{
				result = depotAddress.LocalControlledPremisesID;
			}
			var defaultDestinationPremiseIDs = AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs.Cast<DefaultDestinationPremiseID>();
			if (result.IsEmpty)
			{
				result = AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs.GetPremiseIDMatchingDischargePort(carst.FlightNumber,
					carst.PortOfDischarge);
			}
			if (result.IsEmpty)
			{
				errorMessage = CheckDestinations(mawb.ChildBills);
				if (string.IsNullOrEmpty(errorMessage))
				{
					destination = mawb.ChildBills[0].CS_RL_NKDestination;
					result = AUCustomsDataRegistry.Instance.DefaultDestinationPremiseIDs.GetPremiseIDMatchingDestinationPort(carst.FlightNumber, destination);
				}
			}
			errorMessage = result.IsEmpty ? "No default Destination Premise ID found" : "";

			return result;
		}

		ZString GetDefaultPremiseID(IEnumerable<DefaultPremiseID> collection, ZString flightNumber, ZString port)
		{
			ZString result = ZString.Empty;

			foreach (var item in collection)
			{
				if (IsMatchedPremise(item, flightNumber, port))
				{
					result = item.PremiseID;
					break;
				}
			}

			return result;
		}

		ZBool IsMatchedPremise(DefaultPremiseID premise, ZString flightNumber, ZString port)
		{
			return flightNumber.StartsWith(premise.AirlineCode) && port == premise.PortOfDischarge;
		}

		void ReportError(string errorMessage, CMRCARSTMessage cARST, GlbBranch branch)
		{
			errorMessage += "\r\n\r\n\tFlight Number: " + cARST.FlightNumber;
			errorMessage += "\r\n\tArrival Date: " + cARST.ArrivalDate;
			errorMessage += "\r\n\tMAWB: " + cARST.MAWB;
			errorMessage += "\r\n\tPort Of Discharge: " + cARST.PortOfDischarge;
			if (!destination.IsEmpty)
			{
				errorMessage += "\r\n\tPort Of Destination: " + destination;
			}

			SendAirCargoEmail(cARST.Factory, branch, "Error in processing CARST message", errorMessage);
		}

		public static void SendAirCargoEmail(BusinessObjectFactory factory, GlbBranch branch, string subject, string bodyText)
		{
			AUBatchProcessorSupporter.SendEmail(factory, branch, subject, bodyText, Env.Registry.RawRegistry.AirCargoSendErrorsToGroup);
		}

		#endregion

		#region Implementation

		public override string Name
		{
			get { return "CMRCARSTMessage"; }
		}

		public override string FriendlyName
		{
			get { return "CMRCARST Message Log Walker"; }
		}

		public override string[] TableNames
		{
			get { return new string[] { EDIMessageSchema.Constants.TableName }; }
		}

		public override string[] EventTypes
		{
			get { return new string[] { Events.SubjectToUnderbondMovement.Code }; }
		}

		#endregion
	}
}
