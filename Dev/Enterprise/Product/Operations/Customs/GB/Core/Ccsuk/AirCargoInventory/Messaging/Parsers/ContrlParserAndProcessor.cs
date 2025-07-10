using System;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.BusinessObjects;
using Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging.GENRAL;
using Enterprise.Edifact.D04A.Messages.CONTRL;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.GB.Ccsuk.AirCargoInventory.Messaging
{
	class ContrlParserAndProcessor
	{
		public ContrlParserAndProcessor(CONTRLMessage contrl, EDIMessage incomingMessage, Integration.ILogger serviceLogger)
		{
			this.contrl = contrl;
			this.incomingMessage = incomingMessage;
			this.serviceLogger = serviceLogger;
		}

		internal bool Parse()
		{
			var outgoingMessage = GetOutgoingMessageFromSysCarOrUcmOrUciSegmentOfContrl(contrl, incomingMessage);
			if (outgoingMessage != null)
			{
				// The two standalone edimessage modules....
				if (outgoingMessage.EM_MessageType == GenralMessageGenerator.GenralMessageCodeShortForMessageType
					||
					(outgoingMessage.EM_MessageType == CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Code && outgoingMessage.EM_MessageSubType == CcsukTransmissionMessageFunction.CUKFSR.StandaloneFsrEnquiry.Subcode))
				{
					outgoingMessage.EM_LinkedObject = incomingMessage;
					incomingMessage.EM_LinkedObject = outgoingMessage;
				}
				else
				{   // messages on regular business objects
					incomingMessage.EM_LinkedObject = outgoingMessage.EM_LinkedObject;
				}
				incomingMessage.EM_Status = EDIMessage.Status.Received;
				var oneLineErrorMessage = ParseIncomingContrlToBeALittleMoreHelpful(outgoingMessage);
				UpdateUnderbondIfRelevant(outgoingMessage);
				UpdateChiefEntryIfFound();
				new ContrlParserAndProcessor_PresenceOnNetworkHelper().UpdateJobPresenceOnNetwork(outgoingMessage, oneLineErrorMessage.ToUpper());
				return true;
			}
			serviceLogger.Log(Integration.LogType.Warning, "Could not find outgoing message to which this CONTRL pertains.");
			return false;
		}

		void UpdateChiefEntryIfFound()
		{
			// If CHIEF is unreachable, e.g. fallback is in place, then the response to a CHIEF CUSDEC etc will be a CONTRL "message stored for later transmission"
			if (entryForWhenContrlRejectedChiefMessageNotInventoryMessage != null)
			{
				entryForWhenContrlRejectedChiefMessageNotInventoryMessage.CH_Status = IsPositiveReply ? Customs.Common.EU.MessageStatusList.Codes.OK : Customs.Common.EU.MessageStatusList.Codes.SentAndRejected;  // NB, do not yet update CH_EntryStatus or anythign on the declaration.
			}
		}

		void UpdateUnderbondIfRelevant(EDIMessage outgoingMessage)
		{
			if (!underbondReferenceNumber.IsEmpty && outgoingMessage != null && outgoingMessage.EM_LinkedObject is CusHAWB)
			{
				var hawb = (CusHAWB)outgoingMessage.EM_LinkedObject;
				foreach (CusUnderbond underbond in hawb.AllCusUnderbonds)
				{
					if (underbond.C4_SendersMessageReference == underbondReferenceNumber)
					{
						underbond.C4_Status = EDIMessage.Status.Rejected;
						break;
					}
				}
			}
		}

		EDIMessage GetOutgoingMessageFromSysCarOrUcmOrUciSegmentOfContrl(CONTRLMessage contrl, EDIMessage incomingMessage)
		{
			EDIMessage outgoingMessage = null;
			// CCSUK do not use the D04A, D96B, etc directories.  They use the 912:1 direcotry, which is so old that it is not supported by our webpage-to-c# engine.
			// Defined here: http://www.unece.org/fileadmin/DAM/trade/untdid/d91/91-2.zip (linked from http://www.unece.org/tradewelcome/areas-of-work/un-centre-for-trade-facilitation-and-e-business-uncefact/outputs/standards/unedifact/directories/download.html)
			// NB CONTRL was not introduced until D97, so there's no chance of the 912 definition being OK, pffff.
			// Although we can roughly load a D04A.CONTRL object from the text of a 912:1 message, the names of the nodes are all buggered.
			// So the below two lines do pull the correct elements, even if they look like totally the wrong heirarchy. 

			ZString interchangeNumberFromUciYesIRealiseThatThisLooksAFunnyElementName = contrl.UCI[0].InterchangeRecipient.RecipientIdentification;
			ZString messageNumberFromUcmYesIRealiseThatThisLooksAFunnyElementName = contrl.Group1[0].UCM[0].MessageReferenceNumber;
			ZString sysCar = contrl.UNH[0].CommonAccessReference;

			if (!sysCar.IsEmpty)
			{
				ZGuid primaryKeyGuid = ZGuid.Empty;
				EDIMessage outboundMessage = null;
				try
				{
					primaryKeyGuid = new ZGuid(new Guid(sysCar));
					outboundMessage = incomingMessage.Factory.Load<EDIMessage>(primaryKeyGuid);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					try
					{
						// For CUSDEC (IAR, FBK, etc) the SYSCAR will be messageNum/C4_SendersMessageReference, e.g. 101/U0000069
						var parts = Regex.Split(sysCar, "/");
						string messageNum = parts[0];
						if (parts.Length > 1)
						{
							underbondReferenceNumber = parts[1];
						}
						var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
						query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNum);
						query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
						outboundMessage = incomingMessage.Factory.LoadTop1<EDIMessage>(query);
					}
					catch (Exception e) when (!e.IsCriticalException()) { }
				}

				if (outboundMessage == null) // for inventory messages, outboundMessage will not be null; for CCSUK rejections to non-inventory (e.g. to chief) messages then SYSCAR might be a reference to an entry. 
				{
					entryForWhenContrlRejectedChiefMessageNotInventoryMessage = incomingMessage.Factory.Load<CusEntryHeader>(primaryKeyGuid);
					if (entryForWhenContrlRejectedChiefMessageNotInventoryMessage != null)
					{
						outboundMessage = entryForWhenContrlRejectedChiefMessageNotInventoryMessage.Messages.LastOutgoingMessage;
					}
				}

				if (outboundMessage != null)
				{
					return outboundMessage;
				}
			}

			if (!messageNumberFromUcmYesIRealiseThatThisLooksAFunnyElementName.IsEmpty)
			{
				// messageNumberBare - since we use D:00:A and not real 91:2 directory for the CONTROL, sometimes we see the string "123:123/U00001" in contrl.Group1[0].UCM[0].MessageReferenceNumber for rejected message number 123. So do some simple chopping. 
				var parts = Regex.Split(messageNumberFromUcmYesIRealiseThatThisLooksAFunnyElementName, ":");
				var messageNumberBare = parts.Length == 2 ? parts[0] : messageNumberFromUcmYesIRealiseThatThisLooksAFunnyElementName.ToString();
				if (parts.Length == 2)
				{
					var syscar = Regex.Split(parts[1], "/");
					if (syscar.Length > 1)
					{
						underbondReferenceNumber = syscar[1];
					}
				}
				var query = new ZQuery(EDIMessageSchema.EM_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
				query.AddToFilter(EDIMessageSchema.EM_ReceiveTransmit, EDIMessage.Direction.Transmit);
				query.AddToFilter(GbInterchangeSender.BritishBranches(incomingMessage.Factory));
				query.AddToFilter(EDIMessageSchema.EM_MessageNum, messageNumberBare);
				outgoingMessage = incomingMessage.Factory.LoadTop1<EDIMessage>(query);
			}
			else if (!interchangeNumberFromUciYesIRealiseThatThisLooksAFunnyElementName.IsEmpty)
			{
				var query = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, ApplicationCodeList.Codes.GbCcsuk);
				query.AddToFilter(EDIInterchangeSchema.EI_ReceiveTransmit, EDIInterchange.Direction.Transmit);
				query.AddToFilter(GbInterchangeSender.BritishBranches(incomingMessage.Factory, EDIInterchangeSchema.EI_GB));
				query.AddToFilter(EDIInterchangeSchema.EI_InterchangeNum, interchangeNumberFromUciYesIRealiseThatThisLooksAFunnyElementName);
				var interchange = incomingMessage.Factory.LoadTop1<EDIInterchange>(query);
				if (interchange != null && interchange.ContainedMessages.Count == 1)
				{
					outgoingMessage = interchange.ContainedMessages[0];
				}
			}
			return outgoingMessage;
		}

		string ParseIncomingContrlToBeALittleMoreHelpful(EDIMessage originalSentMessage)
		{
			// NB we do not try to parse a CONTRL's FTX segments because we get 912:1 contrls, but we only know about D04A, D96B, etc contrls. 
			bool isPositiveReply = IsPositiveReply;
			ZStringBuilder sb = new ZStringBuilder();
			var oneLineErrorMessage = ZString.Empty;
			sb.Append("<html> <h3>CONTRL</h3>");
			foreach (var segment in incomingMessage.EM_MessageText.Split(incomingMessage.CharacterSet.SegmentDelimiterChar))
			{
				if (segment.StartsWith("FTX"))
				{
					sb.Append("<b>");
					int elementIndex = 0;
					foreach (var element in segment.Split(incomingMessage.CharacterSet.ElementDelimiterChar))
					{
						if (elementIndex > 1)
						{
							sb.AppendIfNotEmpty(element);
							if (oneLineErrorMessage.IsEmpty)
							{
								oneLineErrorMessage = element;
							}
						}
						elementIndex++;
					}
					sb.Append("</b>");
				}
			}

			sb.Append(string.Format("<p>Outgoing message #{0} ({1}) was {2}</p>", originalSentMessage.EM_MessageNum, originalSentMessage.EM_MessageType, isPositiveReply ? "acknowledged" : "rejected"));
			sb.Append("</html>");
			incomingMessage.EM_MessageInterpretation = MessagePrettierCss.CSS + sb.ToStringWithNewLineBetweenAppends();
			incomingMessage.EM_MessageType = CcsukTransmissionMessageFunction.CONTRL.Code;
			incomingMessage.EM_MessageSubType = isPositiveReply ? EDIMessage.Status.Acknowledged : "NAK";
			originalSentMessage.EM_Status = isPositiveReply ? EDIMessage.Status.Acknowledged : EDIMessage.Status.Rejected;
			return oneLineErrorMessage;
		}

		bool IsPositiveReply
		{
			get
			{
				// Our definition of CONTRL contains no UCX segment so we use a simple 'contains' search for UCX+1'
				return incomingMessage.EM_MessageText.Contains(string.Format("UCX{0}1{1}", incomingMessage.CharacterSet.ElementDelimiter, incomingMessage.CharacterSet.SegmentDelimiter));
			}
		}

		readonly CONTRLMessage contrl;
		readonly EDIMessage incomingMessage;
		readonly Integration.ILogger serviceLogger;
		ZString underbondReferenceNumber;
		CusEntryHeader entryForWhenContrlRejectedChiefMessageNotInventoryMessage;
	}
}
