using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Common;
using Enterprise.Customs.GB.Chief.EdiFact;
using Enterprise.Customs.GB.Registry;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D04A.Messages.CONTRL;
using Enterprise.Edifact.D04A.Segments;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.MessageProcessors;

namespace Enterprise.Customs.GB.Chief.CusRes
{
	/// <summary>
	/// A processor that can understand a UN:4:1 CONTRL message the we received
	/// </summary>
	class CusResHandler_CONTRL : CusResHandler_BASE
	{
		public CusResHandler_CONTRL(ILogger iLogger)
			: base(iLogger)
		{ }

		CONTRLMessage contrl;

		protected override UkResponseMsgIdNumber GetUniqueReferenceNumberFromInboundMessagePreferablySysCar()
		{
			// Normally CONTRLs will have the SYSCAR (our cusentryheader's PK) in the UNH...
			ZString sysCar = contrl.UNH[0].CommonAccessReference;
			if (!sysCar.IsEmpty)
			{
				Guid guid;
				if (Guid.TryParse(sysCar, out guid))
				{
					return new UkResponseMsgIdNumber(sysCar, UkResponseMsgIdNumberType.CommonAccessReference);
				}
			}

			var isReally912NotD04A = contrl.UNH.ToString(new UkCharSet()).Contains("912");
			ZString outboundInterchangeNumber =
					isReally912NotD04A
					? contrl.UCI[0].InterchangeRecipient.RecipientIdentification        // For UN:1:912 directory. We (wrongly) load a UN:1:912 CONTROL asd a D04A control, and the UCI elements are reversed.  Rather than defining 912 directories, use this hack.
					: contrl.UCI[0].InterchangeControlReference;                        // For other directories etc
			if (!outboundInterchangeNumber.IsEmpty)
			{   // ... but EDCS returns v2.2 CONTRLs which have no sysCar.  They do have our outbound interchange though]
				return new UkResponseMsgIdNumber(incomingMessage.EM_ApplicationCode + "/" + outboundInterchangeNumber,  // e.g. NES/13 or GBE/12345. Helps ensure we don't get mixed up between applications, countries, etc
													UkResponseMsgIdNumberType.InterchangeControlReference);
			}

			return null;
		}

		protected override SegmentGroup GetEdifactMessage()
		{
			contrl = (CONTRLMessage)this.incomingMessage.GetAutoEdifactMessageUsingNamedFactory(GbEdiMessageFactory.Factory);
			return contrl;
		}

		protected override bool IsSuccessfulAcknowledgement
		{
			get
			{
				switch (MessageFunction.Trim())
				{
					case "4":   // This level and all lower levels rejected. The response interchange contains a single message, ie the CONTRL message identifying that this action has been taken if at interchange level, or may contain other response messages (CUSRES and/or UKCTRL) if at message level
					case "G48": // The rest of the interchange is rejected. The response interchange contains the response messages (CUSRES or UKCTRL) for the messages that have been processed by CHIEF followed by a CONTRL message. It should be noted that all the messages may have been processed if the superfluous data being rejected is at the end of the interchange.
					case "G49": // One or more messages have been rejected. The response interchange contains the response messages (CUSRES and/or UKCTRL) for the message(s) that have been processed by CHIEF and the CONTRL message where a message has been rejected by EDCS.
					case "G53": // No action taken but see error code (WTF?!)
						return false;
					default:
						return true;
				}
			}
		}

		void WriteHtmlTableForAllErrorsFromContrlMessage(HtmlTableCreator headTableCreator)
		{
			UkCharSet ukCharSet = new UkCharSet();

			// Interchabge errors 
			foreach (UCISegment uci in contrl.UCI)
			{
				WriteHtmlRowForSegment(headTableCreator, ukCharSet,
					uci.SyntaxErrorCoded,
					uci.DataElementIdentification.ErroneousDataElementPositionInSegment,
					uci.DataElementIdentification.ErroneousComponentDataElementPosition,
					uci.ServiceSegmentTagCoded);
			}

			// Segment Group1 (9999) 
			foreach (SegmentGroup1 segmentGroup1 in contrl.Group1)
			{
				// Message or Package Response errors
				var ucm = segmentGroup1.UCM[0];
				WriteHtmlRowForSegment(headTableCreator, ukCharSet,
					ucm.SyntaxErrorCoded,
					ucm.DataElementIdentification.ErroneousDataElementPositionInSegment,
					"",
					ucm.ServiceSegmentTagCoded);

				// Segment Group2 (999)
				foreach (SegmentGroup2 segmentGroup2 in segmentGroup1.Group2)
				{
					// Segment Errors
					foreach (UCSSegment ucs in segmentGroup2.UCS)
					{
						if (segmentGroup2.UCD != null && segmentGroup2.UCD.Count == 0)
						{
							WriteHtmlRowForSegment(headTableCreator, ukCharSet,
								ucs.SyntaxErrorCoded,
								"",
								"",
								"",
								ucs.SegmentPositionInMessage);
						}

						// Data Element Errors (99) relating to current segment
						foreach (UCDSegment ucd in segmentGroup2.UCD)
						{
							WriteHtmlRowForSegment(headTableCreator, ukCharSet,
								ucd.SyntaxErrorCoded,
								ucd.DataElementIdentification.ErroneousDataElementPositionInSegment,
								ucd.DataElementIdentification.ErroneousComponentDataElementPosition,
								"",
								ucs.SegmentPositionInMessage);
						}
					}
				}
			}

			// Add FTX segments for really REALLY low-level errors(e.g. Chief is down, invalid credentials, terrorist attack)
			WriteFtxMessageFromControl(headTableCreator);
		}

		void WriteHtmlRowForSegment(HtmlTableCreator headTableCreator, UkCharSet ukCharSet, string syntaxErrorCoded, string elementPosition, string componentNumber, string segmentTagCoded = "",
			string segementPosition = "")
		{
			if (syntaxErrorCoded != null && !string.IsNullOrEmpty(syntaxErrorCoded) && headTableCreator != null && ukCharSet != null)
			{
				var originalSegment = EdifactSegmentPuller.GetOriginalCusDecSegment(OutgoingCusdec, segementPosition, ukCharSet);
				if (string.IsNullOrEmpty(segmentTagCoded))
				{
					segmentTagCoded = originalSegment.Left(3);
				}

				headTableCreator.WriteRow(syntaxErrorCoded + " - " + new Edifact0085ErrorCodes().GetDescriptionFromCode(syntaxErrorCoded),
											EdifactSegmentPuller.GetOriginalDataFromCusDecSegment(originalSegment, elementPosition, componentNumber, ukCharSet),
											originalSegment,
											CusResEdifactParser.FormatPositionOfBadValue(segmentTagCoded, segementPosition, elementPosition, componentNumber)
										 );
			}
		}

		protected override ZString ClassOfInboundMessage
		{
			get
			{
				return "CONTRL";
			}
		}

		protected override ZString MessageFunction
		{
			get
			{
				ZString messageFunction = contrl.Group1[0].UCM[0].ActionCoded;
				if (!messageFunction.IsEmpty) // e.g. Chief sends contrls with errors at the message level, ie with UCM segments
				{
					return messageFunction; // e.g. 4
				}
				else
				{
					// e.g. EDCS may send controls with a bounced interchange
					return contrl.UCI[0].ActionCoded;  // e.g. 4, G48, G46, etc.
				}
			}
		}

		protected override void UpdateDefinitelyFoundEntryAndOutgoingMessageAndDoAllNotifications()
		{
			if (IsSuccessfulAcknowledgement)
			{
				UpdateOutgoingMessageStatusTo(EDIMessage.Status.Acknowledged);
				incomingMessage.EM_Status = EDIMessage.Status.Received;
				if (entry != null)
				{
					entry.CH_Status = MessageStatusList.Codes.OK;
				}
			}
			else
			{
				// Customs have said they've received our cusdec but it was in an invalid format
				MarkStatusesOfEntryAsFailedIfLastOutboundMessageWasNotEnquiry();
				UpdateOutgoingMessageStatusTo(EDIMessage.Status.Rejected);
				incomingMessage.EM_Status = EDIMessage.Status.Received;
				SendContrlErrorReportToUsers();
			}
		}

		void UpdateOutgoingMessageStatusTo(string newStatus)
		{
			if (originalOutgoingMessage != null)
			{
				originalOutgoingMessage.EM_Status = newStatus;
			}
		}

		void SendContrlErrorReportToUsers()
		{
			HtmlTableCreator headTableCreator = new HtmlTableCreator(new string[] { "Error Text", "Original Value", "Original Segment", "Position" });

			WriteHtmlTableForAllErrorsFromContrlMessage(headTableCreator);

			var originalMessageNo = contrl.Group1[0].UCM[0].MessageReferenceNumber;
			var msgType = contrl.Group1[0].UCM[0].MessageIdentifier.MessageType; // i.e. CUSDEC
			var recipientOfOutgoingMessage = contrl.UCI[0].InterchangeRecipient.RecipientIdentification;
			var body = "The following <font color='red'>CONTRL rejection</font> message was received from " + recipientOfOutgoingMessage;
			body += ".<br> The outgoing message, " + msgType + " " + originalMessageNo + ", was rejected for the following reasons. ";

			body += headTableCreator.ToHtml();

			var entryNo = string.Format("{0} for job {1}", this.entry.EntryNumber.ToString(), GetReferenceNumberForEmailAndHtml());
			var subject = string.Format("Customs rejected entry {0} in {1} message {2}", entryNo, msgType, originalMessageNo);

			SendEmailAndSetReceivedMessageToBeBodyOfEmail(subject, body, gbDeclaration,
				GBCustomsDataRegistry.Instance.GetRegistryItemGuid(GBCustomsDataRegistry.Instance.NotificationChiefNegativeResponsesContrl, "", Guid.Empty, this.entry.RegistryBranchPK, Guid.Empty),
				GBCustomsDataRegistry.Instance.NotificationChiefNegativeResponsesContrl);
		}

		void WriteFtxMessageFromControl(HtmlTableCreator headTableCreator)
		{
			List<string> errors = PullFreeTextSegmentsFromContrlMessage(this.incomingMessage.EM_MessageText, this.incomingMessage.CharacterSet);
			foreach (string error in errors)
			{
				headTableCreator.WriteRow(error, "", "", "");
			}
		}

		static List<string> PullFreeTextSegmentsFromContrlMessage(string messageText, UNCharacterSet charSet)
		{
			List<string> results = new List<string>();
			string elementDelim = charSet.ElementDelimiter;
			string segmentDelim = charSet.SegmentDelimiter;
			string pattern = string.Format(@"FTX\{0}...\{0}\{0}\{0}(.*?){1}", elementDelim, segmentDelim);  //FTX#AAO###FCP0103 60103 - Invalid UVI{  or FTX+AAI+++CNS CCMI authorisation failure. User not authorised for Agent role'      The escape slashes are because edifact symbol + is a special char to the regex engine 
			System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex(pattern);
			foreach (System.Text.RegularExpressions.Match match in regEx.Matches(messageText))
			{
				results.Add(match.Groups[1].Captures[0].Value);
			}
			return results;
		}

		Edifact.D04A.Messages.CUSDEC.CUSDECMessage outgoingCusdec;
		Edifact.D04A.Messages.CUSDEC.CUSDECMessage OutgoingCusdec
		{
			get
			{
				if (outgoingCusdec == null)
				{
					var outgoingMessageBeingRejected = (from EDIMessage m in entry.Messages where m.EM_ReceiveTransmit == EDIMessage.Direction.Transmit && m.EM_MessageNum == contrl.Group1[0].UCM[0].MessageReferenceNumber select m).FirstOrDefault();
					outgoingCusdec = CusResEdifactParser.CreateCusdecFromOutboundMessage(outgoingMessageBeingRejected);
				}
				return outgoingCusdec;
			}
		}
	}
}

