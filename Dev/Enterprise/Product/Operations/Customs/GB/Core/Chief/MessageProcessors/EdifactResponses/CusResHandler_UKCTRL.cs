using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Chief.ChiefExportConsolIntegration;
using Enterprise.Customs.GB.Chief.EdiFact.UKCINV;
using Enterprise.Customs.GB.Chief.EdiFact.UKCTRL;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact.Auto;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.GB.Chief.CusRes
{
	public class CusResHandler_UKCTRL : CusResHandler_BASE
	{
		public CusResHandler_UKCTRL(ILogger iLogger)
			: base(iLogger)
		{ }

		protected override UkResponseMsgIdNumber GetUniqueReferenceNumberFromInboundMessagePreferablySysCar()
		{
			ZString syscar = UkctrlMessage.Header.UCM_MSG_SYS_CAR;
			if (!syscar.IsEmpty)
			{
				return new UkResponseMsgIdNumber(syscar, UkResponseMsgIdNumberType.CommonAccessReference);
			}
			else
			{
				throw new NotSupportedException("Ukctrl processor can only handle UKCTRL messages that contain a SYS-CAR from the outbound message.");
			}
		}

		protected override void DoAllProcessingCore(EDIMessage inboundMessage)
		{
			PerformPreProcessingInitialisation(inboundMessage);
			GetEntryAndGbDeclarationToWhichThisPertainsFromMessageId();
			if (entry == null)
			{
				var consol = GetConsolFromInboundMessage();
				if (consol != null)
				{
					ChangeDefaultValuesOnIncomingMessageToSomethingUseful();
					if (originalOutgoingMessage != null)
					{
						originalOutgoingMessage.EM_Status = IsSuccessfulAcknowledgement ? EDIMessage.Status.Acknowledged : EDIMessage.Status.Rejected;
					}
					incomingMessage.EM_Status = EDIMessage.Status.Received;
					consol.Messages.Add(incomingMessage);
					if (IsSuccessfulAcknowledgement)
					{
						originalOutgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
						UpdateChiefConsolIsClosedFlag(consol);
						SendAckSuccessEmail(false, consol.HumanReadableName, consol, ControllerIDs.JobConsol);
					}
					else
					{
						if (IsChiefError3481MucrIsAlreadyShut)
						{
							UpdateChiefConsolIsClosedFlag(consol);
						}
						originalOutgoingMessage.EM_Status = EDIMessage.Status.Rejected;
						SendErrorEmailToUser(consol.HumanReadableName, consol, ControllerIDs.JobConsol, "Note, processing of this message has ensured that consol " + consol.JK_UniqueConsignRef + " shows as closed.");
					}
				}
			}
			else
			{
				originalOutgoingMessage = FindMatchingOutboundMessage();
				incomingMessage.EM_GB = entry.Declaration.Branch.PK;
				UpdateDefinitelyFoundEntryAndOutgoingMessageAndDoAllNotifications();
				ChangeDefaultValuesOnIncomingMessageToSomethingUseful();
				entry.Messages.Add(incomingMessage);
			}
		}

		bool OutboundRequestWasCloseRequest
		{
			get { return originalOutgoingMessage.EM_MessageType == GbDes242MessageFunction.Eac && originalOutgoingMessage.EM_MessageSubType == GbCusDecMessageFunctionsList.Codes.Close; }
		}

		protected override EDIMessage FindMatchingOutboundMessage()
		{
			ZString messageNum = UkctrlMessage.Header.UCM_MSG_MRN;
			if (messageNum.IsEmpty)
			{
				return base.FindMatchingOutboundMessage();
			}
			else
			{
				foreach (EDIMessage message in entry.Messages)
				{
					if (message.EM_ReceiveTransmit == EDIMessage.Direction.Transmit &&
						message.EM_MessageNum == messageNum)
					{
						return message;
					}
				}
			}
			return null;
		}

		protected override SegmentGroup GetEdifactMessage()
		{
			return new Edifact.D04A.Messages.CONTRL.CONTRLMessage(); // No real definition for UKCTRL
		}

		protected override void UpdateDefinitelyFoundEntryAndOutgoingMessageAndDoAllNotifications()
		{
			if (IsSuccessfulAcknowledgement)
			{
				originalOutgoingMessage.EM_Status = EDIMessage.Status.Acknowledged;
				SendAckSuccessEmail(FindConsolFromEntryAndCloseIt(), GetReferenceNumberForEmailAndHtml(), gbDeclaration, ControllerIDs.Customs.JobDeclaration);  // e.g. Chief said that association was fine.
				ClearMucrFromEntryIfRequestWasDisassociate();
				entry.CH_Status = MessageStatusList.Codes.OK;
			}
			else
			{
				// UKCTRL is only used by Chief for business rules failures, NOT syntax failures. 
				originalOutgoingMessage.EM_Status = EDIMessage.Status.Rejected;
				SendErrorEmailToUser(GetReferenceNumberForEmailAndHtml(), gbDeclaration, ControllerIDs.Customs.JobDeclaration);
				entry.CH_Status = MessageStatusList.Codes.SentAndRejected;
			}
			incomingMessage.EM_Status = EDIMessage.Status.Received;
		}

		bool FindConsolFromEntryAndCloseIt()
		{
			ForwardingConsol result = null;
			var shipment = entry.Declaration.Shipment;
			if (shipment != null)
			{
				result = (from ForwardingConsol c
						in shipment.Consols
						  where c.IsExport() && c.IsAir && c.JK_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.UnitedKingdom)
						  select c
						).FirstOrDefault();
			}
			if (result != null)
			{
				UpdateChiefConsolIsClosedFlag(result);
			}

			return result != null;
		}

		void UpdateChiefConsolIsClosedFlag(ForwardingConsol consol)
		{
			var wrapper = new CustomsExportConsolIntegrationWrapper(consol, new Customs.Business.SendsMessagesToCustomsShutterUpperer(false));
			if (OutboundRequestWasCloseRequest)
			{
				if (wrapper.MawbExportHelper != null)
				{
					wrapper.MawbExportHelper.ME_ChiefConsolIsClosed = true;
				}
			}
		}

		void ClearMucrFromEntryIfRequestWasDisassociate()
		{
			if (originalOutgoingMessage.EM_MessageSubType == GbCusDecMessageFunctionsList.Codes.Disassociate)
			{
				entry.Declaration.JE_MasterUCR = string.Empty;
			}
		}

		void SendAckSuccessEmail(bool foundConsol, string jobNumber, BusinessObject bizO, ControllerID controllerId)
		{
			string subject = string.Format("{0} - ACK received", jobNumber);
			string body = string.Format("ACK received. Message #{0} ({1}/{2}) has been accepted.", originalOutgoingMessage.EM_MessageNum, originalOutgoingMessage.EM_MessageType, originalOutgoingMessage.EM_MessageSubType);
			var entryRegistryBranchPK = (entry != null) ? entry.RegistryBranchPK : Guid.Empty;
			SendEmailAndSetReceivedMessageToBeBodyOfEmail(subject, body, bizO, controllerId,
				GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefPositiveResponsesUkctrlAck, "", Guid.Empty, entryRegistryBranchPK, Guid.Empty),
				GBCustomsDataRegistry.Instance.NotificationChiefPositiveResponsesUkctrlAck);
		}

		protected override bool IsSuccessfulAcknowledgement
		{
			get
			{
				return UkctrlMessage.IsOverallSuccess;  // looks at UCX's action code
			}
		}

		void SendErrorEmailToUser(string jobNumber, BusinessObject bizO, ControllerID controllerId, string supplementaryInformationFromWtg = null) // Do not send UkCtrl messages to CW, since these are NOT about duff cusdecs.  CONTRLs are, though. 
		{
			List<ZString> reasons = UkctrlMessage.GroupTwos.AllFreeText;
			var originalUcr = FindCitedUcrReferenceFromOutboundMessage();

			string reasonsAllTogether = "";
			foreach (string oneReason in reasons)
			{
				reasonsAllTogether += System.Environment.NewLine + "<BR/>" + System.Environment.NewLine + oneReason;
			}

			string subject = string.Format(CultureInfo.CurrentCulture, "{0} - NAK error received", jobNumber);
			string body = "A NAK <font color='red'>error</font> message was received for this job. This usually means that the recipient rejected the request due to business rule failures. The reasons for the failure are given here: <br><br> <b>"
				+ reasonsAllTogether + "</b>";
			if (supplementaryInformationFromWtg != null)
			{
				body += "<p>" + supplementaryInformationFromWtg + "</p>";
			}
			if (!string.IsNullOrEmpty(originalUcr))
			{
				body += "<p>Cited UCR: " + originalUcr + "</p>";
			}

			var branch = entry != null ? entry.RegistryBranchPK
										: originalOutgoingMessage != null && originalOutgoingMessage.UserWhoQueuedThisRecord != null && originalOutgoingMessage.UserWhoQueuedThisRecord.HomeBranch != null
											? originalOutgoingMessage.UserWhoQueuedThisRecord.HomeBranch.PK
											: originalOutgoingMessage != null
												? originalOutgoingMessage.Branch.PK
												: incomingMessage.Branch.PK;
			SendEmailAndSetReceivedMessageToBeBodyOfEmail(subject, body, bizO, controllerId,
				GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefNegativeResponsesUkctrlNak, "", Guid.Empty, branch.ToGuid(), Guid.Empty),
				GBCustomsDataRegistry.Instance.GetNotificationItem("", "", GBCustomsDataRegistry.Instance.NotificationChiefNegativeResponsesUkctrlNak));
		}

		string FindCitedUcrReferenceFromOutboundMessage()
		{
			if (originalOutgoingMessage.EM_MessageText.StartsWith(ChiefConstants.UNH, StringComparison.OrdinalIgnoreCase))
			{
				foreach (var grp2 in UkctrlMessage.GroupTwos)
				{
					if (grp2.UCR_SEG_NO != "0")
					{
						int segIndexOneBased = 0;
						if (int.TryParse(grp2.UCR_SEG_NO, out segIndexOneBased))
						{
							var elemIndexOneBased = grp2.UCD_ELEMENT_NO;
							var subElemIndexOneBased = grp2.UCD_COMPONENT_NO;
							var request = originalOutgoingMessage.GetAutoEdifactMessageUsingNamedFactory(GbEdiMessageFactory.Factory);
							if (request is UkCinvMessage cinv)
							{
								if (cinv.MessageSections.Length >= segIndexOneBased)
								{
									var segment = cinv.GetNthSegmentWhereNIsOneBased(segIndexOneBased);
									return EdifactSegmentPuller.GetOriginalDataFromCusDecSegment(segment, elemIndexOneBased, subElemIndexOneBased, originalOutgoingMessage.CharacterSet);
								}
							}
						}
					}
				}
			}
			return "";
		}

		UkctrlMessage ukctrlMessage;
		UkctrlMessage UkctrlMessage
		{
			get
			{
				if (ukctrlMessage == null)
				{
					UkctrlRegexParser parser = new UkctrlRegexParser();
					ukctrlMessage = parser.Parse(this.incomingMessage.EM_MessageText);
				}
				return ukctrlMessage;
			}
		}

		protected override ZString ClassOfInboundMessage
		{
			get
			{
				return "UKCTRL";
			}
		}

		protected override ZString MessageFunction
		{
			get
			{
				return "";
			}
		}

		protected override ZString MessageCode
		{
			get
			{
				return IsSuccessfulAcknowledgement ? "ACK" : "NAK";
			}
		}

		bool IsChiefError3481MucrIsAlreadyShut
		{
			get { return incomingMessage.EM_MessageText.Contains("E3481", StringComparison.OrdinalIgnoreCase); }
		}
	}
}
