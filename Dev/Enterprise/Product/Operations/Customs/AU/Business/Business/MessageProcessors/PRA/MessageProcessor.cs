using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.AU;
using Enterprise.Edifact;
using Enterprise.Edifact.Auto;
using Enterprise.Edifact.D00A.Messages.APERAK;
using Enterprise.Edifact.D00A.Segments;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class MessageProcessor : Messaging.MessageProcessors.CustomsMessageProcessor
	{
		public MessageProcessor(LoggingInformation logger)
			: base(logger, EDIInterchange.ApplicationCodes.OneStop, "1-Stop Pre-Receival Advice")
		{
		}

		#region Process Returning Status

		protected override string DoProcessingReturningStatus(EDIMessage message)
		{
			factory = message.Factory;

			APERAKMessage aPERAK = new APERAKGetter(message).APERAK;
			if (aPERAK == null)
			{
				Logger.Log("Corrupted or Malformed Response Message. Message does not conform to UN-EDIFACT Standard. Cannot Process.");
				return EDIMessage.Status.Error;
			}

			var messageManager = GetWriterWithMessage(aPERAK);
			if (messageManager == null || (messageManager.Parent == null && messageManager.Container == null))
			{
				return EDIMessage.Status.Error;
			}

			EmailDef emailDef = GetEmailDef(message, aPERAK, messageManager);
			if (emailDef == null)
			{
				return EDIMessage.Status.Error;
			}

			return EDIMessage.Status.Received;
		}

		IPRAMessageManager GetWriterWithMessage(APERAKMessage aPERAK)
		{
			IPRAMessageManager msgWriter = ProcessGroup0(aPERAK);

			if (msgWriter == null || msgWriter.Container == null)
			{
				string emailBodyHeader = "FATAL PROCESSING ERROR: Could Not Create Container.\r\n\r\n";
				string messageText = (aPERAK == null) ? "" : "\r\n" + aPERAK.ToString(characterSet).Replace("'", "'\r\n");
				string subject = emailBodyHeader;

				string headerSummary = "PRA " + (messageReceivedWithoutError ? "Accepted" : "Rejected");
				string body = emailBodyHeader + PadCentre("Response Status: " + headerSummary, 70) + "\r\n"
					+ "Consol No         : Unknown\r\n"
					+ "Container No      : Unknown\r\n"
					+ "Response From     : \r\n"
					+ "".PadRight(70, '-') + "\r\n"
					+ messageText;

				EmailDef errorEmail = CreateEmail(subject, body);

				var emailParentObj = msgWriter != null
					? msgWriter.Container ?? msgWriter.Parent
					: null;

				if (emailParentObj != null)
				{
					SendErrorReport(emailParentObj, errorEmail);
				}
				return null;
			}

			return msgWriter;
		}

		EmailDef GetEmailDef(EDIMessage message, APERAKMessage aPERAK, IPRAMessageManager msgWriter)
		{
			EmailDef responseEmail = CompileResponseEmail(msgWriter);
			EmailDef errorEmail = null;
			if (responseEmail == null)
			{
				string messageText = (aPERAK == null) ? "" : "\r\n" + aPERAK.ToString(characterSet).Replace("'", "'\r\n");
				string subject = "PRA PROCESSING ERROR";
				string body = "responseEmail does not created with message. Cannot Process.\r\n" + messageText + "\r\n";

				errorEmail = CreateEmail(subject, body);
				SendErrorReport(msgWriter.Container, errorEmail);
				return null;
			}

			if (!SaveResponseEmail(message, msgWriter, responseEmail))
			{
				string messageText = (aPERAK == null) ? "" : "\r\n" + aPERAK.ToString(characterSet).Replace("'", "'\r\n");
				string subject = "PRA PROCESSING ERROR";
				string body = "Could Not Find Container in Database.\r\n" + messageText + "\r\n";

				errorEmail = CreateEmail(subject, body);
				SendErrorReport(msgWriter.Container, errorEmail);
				return null;
			}

			return responseEmail;
		}

		ZBool SaveResponseEmail(EDIMessage message, IPRAMessageManager messageManager, EmailDef responseEmail)
		{
			responseBody = responseEmail.Body;

			message.EM_MessageSubType = messageReceivedWithoutError
				? PRAConstants.MessageAcknowledged
				: PRAConstants.MessageRejected;

			messageManager.Messages.Add(message);

			try
			{
				ZExceptionReporting.ProcessWithSaveExceptionHandling(message.Factory.Save, null, true);
			}
			catch (ZSaveException)
			{
				return false;
			}

			AddContainerDetailsToResponseEmail(responseEmail, messageManager);

			if (message.EM_MessageSubType == PRAConstants.MessageAcknowledged)
			{
				SendAcknowledgementReport(messageManager.Container, responseEmail);

				var container = messageManager.Container as CommonContainer;
				if (container != null && container.LastPRAMessageSentWasCancellation)
				{
					AddContainerEventLog(messageManager.Container, Events.MessageWithdrawCancelAccepted);
				}
				else
				{
					AddContainerEventLog(messageManager.Container, Events.MessageAccepted);
				}
			}
			else if (message.EM_MessageSubType == PRAConstants.MessageRejected)
			{
				{
					SendImpedimentReport(messageManager.Container, responseEmail);
					AddContainerEventLog(messageManager.Container, Events.MessageRejected);
				}
			}

			return true;
		}

		void AddContainerEventLog(BusinessObject parent, Event eventType)
		{
			var container = parent as IStmALogParent;
			if (container != null && container.Logs != null)
			{
				container.Logs.AddNew(eventType, PRAMessageEvent.GetEventParameters(eventType));
			}
		}

		IPRAMessageManager ProcessGroup0(APERAKMessage aPERAK)
		{
			IPRAMessageManager typeWriter = null;

			UNHSegment uNH = aPERAK.UNH[0];
			CheckRequiredSegmentNotNull(uNH);

			BGMSegment bGM = aPERAK.BGM[0];
			CheckRequiredSegmentNotNull(bGM);
			string messageType = bGM.ResponseTypeCode.ToString();

			DTMSegment dTM = aPERAK.DTM[0];
			// Nothing done with the Message Date.

			ProcessGroup1(aPERAK.Group1[0]);

			foreach (SegmentGroup2 group2 in aPERAK.Group2) // Get Consol/Container Reference.
			{
				typeWriter = ProcessGroup2(group2);
			}

			if (typeWriter != null)
			{
				foreach (SegmentGroup3 group3 in aPERAK.Group3) // Who sent this response.
				{
					ProcessGroup3(group3);
				}

				foreach (SegmentGroup4 group4 in aPERAK.Group4) // Errors and other info.
				{
					ProcessGroup4(group4);
				}

				UNTSegment uNT = aPERAK.UNT[0];
				CheckRequiredSegmentNotNull(uNT);
			}

			return typeWriter;
		}

		void ProcessGroup1(SegmentGroup1 group1)
		{
			DOCSegment dOC = group1.DOC[0];
			// Nothing done with the Originating System ID.

			DTMSegment dTM = group1.DTM[0];
			// Nothing done with the Originating Message Date.
		}

		IPRAMessageManager ProcessGroup2(SegmentGroup2 group2)
		{
			IPRAMessageManager typeWriter = null;
			RFFSegment rFF = group2.RFF[0];

			CheckRequiredSegmentNotNull(rFF);
			if (rFF.Reference.ReferenceFunctionCodeQualifier.ToString() == "ERN")
			{
				ZString originatingMessageReference = rFF.Reference.ReferenceIdentifier;
				if (originatingMessageReference.Contains('-'))
				{
					ZString[] messageRefs = originatingMessageReference.Split(new char[] { '-' });

					if (messageRefs.Length == 3 && messageRefs[0] == "CON")
					{
						typeWriter = new ForwardingPRAMessageManager(messageRefs[1], messageRefs[2], factory);
					}

					if (messageRefs.Length == 3 && messageRefs[0] == "CUS")
					{
						typeWriter = new CustomsPRAMessageManager(messageRefs[1], messageRefs[2], factory);
					}
				}
			}
			return typeWriter;
		}

		void ProcessGroup3(SegmentGroup3 group3)
		{
			NADSegment nAD = group3.NAD[0];
			CheckRequiredSegmentNotNull(nAD);
			if (nAD.PartyFunctionCodeQualifier.ToString() == "MS")
			{
				string messageSender1StopCode = nAD.PartyIdentificationDetails.PartyIdentifier;
				responseSender = messageSender1StopCode + " - " + new MessageSenderPairList().GetDescriptionFromCode(messageSender1StopCode);
			}
		}

		void ProcessGroup4(SegmentGroup4 group4)
		{
			ERCSegment eRC = group4.ERC[0];
			CheckRequiredSegmentNotNull(eRC);
			string errorCode = eRC.ApplicationErrorDetail.ApplicationErrorCode;

			messageReceivedWithoutError = (errorCode == "ERA0100" || errorCode == "ERA0101");
			string errorName = (messageReceivedWithoutError ? "Response" : "Error");

			AddStatusLine("");
			AddStatusLine(errorName + " Status", errorCode + " - " + GetErrorDescription(errorCode));

			FTXSegment fTX = group4.FTX[0];
			CheckRequiredSegmentNotNull(fTX);
			AddStatusLine(errorName + " Message", fTX.TextLiteral.FreeTextValue1);
			AddStatusLine("", fTX.TextLiteral.FreeTextValue2);
			AddStatusLine("", fTX.TextLiteral.FreeTextValue3);
			AddStatusLine("", fTX.TextLiteral.FreeTextValue4);
			AddStatusLine("", fTX.TextLiteral.FreeTextValue5);

			foreach (SegmentGroup5 group5 in group4.Group5)
			{
				ProcessGroup5(group5);
			}
		}

		void ProcessGroup5(SegmentGroup5 group5)
		{
			RFFSegment rFF = group5.RFF[0];
			// Container Number already known.

			FTXSegment fTX = group5.FTX[0];
			if (fTX != null && fTX.TextLiteral.FreeTextValue1 != null && !string.IsNullOrEmpty(fTX.TextLiteral.FreeTextValue1))
			{
				AddStatusLine("");
				AddStatusLine("Additional Info", fTX.TextLiteral.FreeTextValue1);
				AddStatusLine("", fTX.TextLiteral.FreeTextValue2);
				AddStatusLine("", fTX.TextLiteral.FreeTextValue3);
				AddStatusLine("", fTX.TextLiteral.FreeTextValue4);
				AddStatusLine("", fTX.TextLiteral.FreeTextValue5);
			}
		}

		void CheckRequiredSegmentNotNull(Segment segment)
		{
			if (segment == null)
			{
				throw new InvalidFormatException("Expected 'segment.SegmentName'\r\nSegment Missing from Message - Cannot complete processing.");
			}
		}

		readonly StringBuilder statusString = new StringBuilder();
		BusinessObjectFactory factory;
		internal string responseBody = string.Empty;

		string responseSender = "";
		bool messageReceivedWithoutError;
		readonly UNOCCMRCharacterSet characterSet = new UNOCCMRCharacterSet();

		#endregion
		#region Code List Converters

		string GetErrorDescription(string errorCode)
		{
			var result = string.Empty;
			switch (errorCode)
			{
				case "ERA0001":
					result = "Segment count mismatch";
					break;
				case "ERA0002":
					result = "Message reference mismatch";
					break;
				case "ERA0003":
					result = "Recipient not supplied";
					break;
				case "ERA0004":
					result = "Message function not supplied";
					break;
				case "ERA0005":
					result = "Message function not defined";
					break;
				case "ERA0006":
					result = "Arrival mode of transport not supplied";
					break;
				case "ERA0007":
					result = "Arrival mode of transport not defined";
					break;
				case "ERA0009":
					result = "Load UNLOCODE not supplied";
					break;
				case "ERA0010":
					result = "Invalid carrier specified";
					break;
				case "ERA0011":
					result = "Lloyds number not supplied";
					break;
				case "ERA0012":
					result = "Incorrect Lloyds Number or incorrect CTO Address supplied, please fix one of these fields or contact One Stop for more information";
					break;
				case "ERA0013":
					result = "Voyage number not supplied";
					break;
				case "ERA0014":
					result = "Voyage number not defined";
					break;
				case "ERA0015":
					result = "Discharge port not supplied";
					break;
				case "ERA0016":
					result = "Discharge port not defined";
					break;
				case "ERA0017":
					result = "Vessel not calling at discharge port";
					break;
				case "ERA0018":
					result = "ISO size/type not supplied";
					break;
				case "ERA0019":
					result = "ISO size/type not defined";
					break;
				case "ERA0020":
					result = "Operator not supplied";
					break;
				case "ERA0021":
					result = "Operator not defined";
					break;
				case "ERA0022":
					result = "Operator not shipping on vessel/voyage";
					break;
				case "ERA0023":
					result = "Destination UNLOCODE not defined";
					break;
				case "ERA0024":
					result = "Weight not supplied";
					break;
				case "ERA0025":
					result = "Weight exceeds vessel/voyage maximum";
					break;
				case "ERA0026":
					result = "Commodity not supplied";
					break;
				case "ERA0027":
					result = "Commodity code not defined";
					break;
				case "ERA0028":
					result = "Temperature not supplied for REEFER cargo";
					break;
				case "ERA0029":
					result = "HAZARDOUS class not supplied for HAZARDOUS Commodity";
					break;
				case "ERA0030":
					result = "Over-dimensional details not supplied for OUT OF GAUGE cargo";
					break;
				case "ERA0031":
					result = "CAN/Exemption Code not supplied";
					break;
				case "ERA0032":
					result = "Invalid CAN/Exemption Code";
					break;
				case "ERA0037":
					result = "Container full status not supplied";
					break;
				case "ERA0038":
					result = "Container status not supplied";
					break;
				case "ERA0039":
					result = "Container not supplied";
					break;
				case "ERA0040":
					result = "Container already in terminal";
					break;
				case "ERA0041":
					result = "Container already loaded on vessel - data ignored";
					break;
				case "ERA0042":
					result = "Cancel denied - container not pre-advise";
					break;
				case "ERA0050":
					result = "Weight not numeric";
					break;
				case "ERA0051":
					result = "Over-dimension (top) not numeric";
					break;
				case "ERA0052":
					result = "Over-dimension (front) not numeric";
					break;
				case "ERA0053":
					result = "Over-dimension (back) not numeric";
					break;
				case "ERA0054":
					result = "Over-dimension (left) not numeric";
					break;
				case "ERA0055":
					result = "Over-dimension (right) not numeric";
					break;
				case "ERA0056":
					result = "Temperature must be numeric";
					break;
				case "ERA0057":
					result = "Terminal not defined";
					break;
				case "ERA0058":
					result = "Load UNLOCODE not defined";
					break;
				case "ERA0059":
					result = "Temperature supplied for non REEFER container";
					break;
				case "ERA0060":
					result = "Temperature too low for commodity";
					break;
				case "ERA0061":
					result = "Temperature too high for commodity";
					break;
				case "ERA0062":
					result = "User (carrier) not supplied";
					break;
				case "ERA0064":
					result = "Multiple records of voyage/Lloyds number";
					break;
				case "ERA0065":
					result = "Vessel cut off for receivals";
					break;
				case "ERA0066":
					result = "Container number is TBA";
					break;
				case "ERA0067":
					result = "Container allocated to another users booking";
					break;
				case "ERA0068":
					result = "Container allocated to a movement";
					break;
				case "ERA0069":
					result = "Container full status not defined";
					break;
				case "ERA0070":
					result = "Movement type code not defined";
					break;
				case "ERA0071":
					result = "Weight outside limits";
					break;
				case "ERA0072":
					result = "Weight for 20 foot container exceeded";
					break;
				case "ERA0073":
					result = "Empty container has commodity code other than MT or MTHZ";
					break;
				case "ERA0074":
					result = "Hazard not defined";
					break;
				case "ERA0075":
					result = "Container on another users ERA";
					break;
				case "ERA0076":
					result = "Add to existing ERA";
					break;
				case "ERA0077":
					result = "Change to non existing ERA";
					break;
				case "ERA0078":
					result = "Delete non existing ERA";
					break;
				case "ERA0079":
					result = "Hazardous net weight not supplied for hazardous cargo";
					break;
				case "ERA0080":
					result = "Hazardous flashpoint must be numeric";
					break;
				case "ERA0081":
					result = "Duplicate vent setting for reefer cargo";
					break;
				case "ERA0082":
					result = "Duplicate humidity for reefer cargo";
					break;
				case "ERA0083":
					result = "Seal number not supplied";
					break;
				case "ERA0090":
					result = "Port of Loading Invalid";
					break;
				case "ERA0100":
					result = "Message received without error";
					break;
				case "ERA0101":
					result = "Container cancelled as requested";
					break;
				case "ERA0102":
					result = "Discharge port cannot be the same as Load Port";
					break;
				case "ERA0110":
					result = "A vessel /voyage must be specified";
					break;
				case "ERA0111":
					result = "Shippers reference number cannot be blank";
					break;
				case "ERA0112":
					result = "Category must be entered";
					break;
				case "ERA0113":
					result = "Shipping line booking reference number can not be blank";
					break;
				case "ERA0114":
					result = "Final destination must be entered";
					break;
				case "ERA0115":
					result = "Container number cannot be blank";
					break;
				case "ERA0116":
					result = "Arrival mode at wharf must be entered";
					break;
				case "ERA0117":
					result = "Shippers reference number can only be alphanumeric and may contain dashes";
					break;
				case "ERA0118":
					result = "Booking reference number can only be alphanumeric and may contain dashes";
					break;
				case "ERA0119":
					result = "Commodity code must be set to MT or MTHZ when Container Status is EMPTY";
					break;
				case "ERA0120":
					result = "Commodity code must not be set to MT or MTHZ when Container Status is FULL";
					break;
				case "ERA0121":
					result = "Container ISO code must be 4 characters and is obtained from the actual container";
					break;
				case "ERA0122":
					result = "A REEFER setting must be specified";
					break;
				case "ERA0124":
					result = "Missing REEFER settings for ISO code";
					break;
				case "ERA0125":
					result = "Future use";
					break;
				case "ERA0126":
					result = "Dangerous Goods require Hazardous commodity setting";
					break;
				case "ERA0127":
					result = "Container Tare Weight + Cargo Gross Weight must equal Container Gross Weight";
					break;
				case "ERA0128":
					result = "At least one Seal Number must be entered when Container Status is full";
					break;
				case "ERA0129":
					result = "You must set a seal type";
					break;
				case "ERA0130":
					result = "Future use";
					break;
				case "ERA0131":
					result = "Packaging quantity must be numeric and contain no decimal points";
					break;
				case "ERA0132":
					result = "Reefer vent airflow value must be numeric";
					break;
				case "ERA0133":
					result = "Reefer vent airflow value must be positive";
					break;
				case "ERA0134":
					result = "Reefer Vent Airflow units must be selected; CFM; CMH or %";
					break;
				case "ERA0135":
					result = "Humidity value must be numeric";
					break;
				case "ERA0136":
					result = "Humidity value must not be greater than 100%";
					break;
				case "ERA0137":
					result = "Humidity value must be equal or greater than 1%";
					break;
				case "ERA0138":
					result = "Future use";
					break;
				case "ERA0139":
					result = "Future use";
					break;
				case "ERA0140":
					result = "HAZARDOUS Commodity code must not contain Reefer details";
					break;
				case "ERA0141":
					result = "HAZARDOUS Commodity code must not contain OutOfGauge details";
					break;
				case "ERA0142":
					result = "HAZREEFER Commodity code must contain Reefer details";
					break;
				case "ERA0144":
					result = "GENERAL Commodity code must not contain Reefer details";
					break;
				case "ERA0145":
					result = "GENERAL Commodity code must not contain OutOfGauge details";
					break;
				case "ERA0146":
					result = "Commodity MT must not have a CAN";
					break;
				case "ERA0147":
					result = "EMPTY Commodity code must not contain Reefer details";
					break;
				case "ERA0148":
					result = "EMPTYHAZ Commodity code must not contain Reefer details";
					break;
				case "ERA0149":
					result = "EMPTYHAZ Commodity must have empty container status";
					break;
				case "ERA0150":
					result = "EMPTYHAZ Commodity must not have a CAN";
					break;
				case "ERA0151":
					result = "EMPTYHAZ Commodity code must contain Hazardous details";
					break;
				case "ERA0152":
					result = "EMPTYHAZ Commodity code must not contain OutOfGauge details";
					break;
				case "ERA0153":
					result = "OUTOFGAUGE Commodity code must not contain Reefer details";
					break;
				case "ERA0154":
					result = "OUTOFGAUGE Commodity code must contain OutOfGauge details";
					break;
				case "ERA0155":
					result = "REEFER Commodity code must not contain OutOfGauge details";
					break;
				case "ERA0156":
					result = "EMPTY Commodity code must not contain OutOfGauge details";
					break;
				case "ERA0157":
					result = "OUTOFGAUGE Commodity code must not contain Hazardous details";
					break;
				case "ERA0200":
					result = "BREAKBULK Commodity code must not contain Empty details";
					break;
				case "ERA0201":
					result = "BREAKBULK Commodity code must not contain EmptyHaz details";
					break;
				case "ERA0203":
					result = "BREAKBULK Commodity code must not contain General details";
					break;
				case "ERA0204":
					result = "BREAKBULK Commodity code must not contain Hazardous details";
					break;
				case "ERA0205":
					result = "BREAKBULK Commodity code must not contain HazReefer details";
					break;
				case "ERA0206":
					result = "BREAKBULK Commodity code must not contain Insulated details";
					break;
				case "ERA0207":
					result = "BREAKBULK Commodity code must not contain OutOfGauge details";
					break;
				case "ERA0208":
					result = "BREAKBULK Commodity code must not contain Reefer details";
					break;
				case "ERA0209":
					result = "Commodity MT must have EMPTY container status";
					break;
				case "ERA0210":
					result = "Commodity MT or MTHZ must have EMPTY container status";
					break;
				case "ERA0211":
					result = "Commodity SNTU must have 3rd character of ISO code as “H”";
					break;
				case "ERA0212":
					result = "EMPTY Commodity code must not contain BreakBulk details";
					break;
				case "ERA0213":
					result = "EMPTY Commodity code must not contain EmptyHaz details";
					break;
				case "ERA0214":
					result = "EMPTY Commodity code must not contain General details";
					break;
				case "ERA0215":
					result = "EMPTY Commodity code must not contain Hazardous details";
					break;
				case "ERA0216":
					result = "EMPTY Commodity code must not contain HazReefer details";
					break;
				case "ERA0217":
					result = "EMPTY Commodity code must not contain Insulated details";
					break;
				case "ERA0218":
					result = "EMPTYHAZ Commodity code must not contain BreakBulk details";
					break;
				case "ERA0219":
					result = "EMPTYHAZ Commodity code must not contain General details";
					break;
				case "ERA0220":
					result = "EMPTYHAZ Commodity code must not contain HazReefer details";
					break;
				case "ERA0221":
					result = "EMPTYHAZ Commodity code must not contain Insulated details";
					break;
				case "ERA0222":
					result = "GENERAL Commodity code must contain General details";
					break;
				case "ERA0223":
					result = "GENERAL Commodity code must not contain BreakBulk details";
					break;
				case "ERA0224":
					result = "GENERAL Commodity code must not contain Empty details";
					break;
				case "ERA0225":
					result = "GENERAL Commodity code must not contain EmptyHaz details";
					break;
				case "ERA0226":
					result = "GENERAL Commodity code must not contain Hazardous details";
					break;
				case "ERA0227":
					result = "GENERAL Commodity code must not contain HazReefer details";
					break;
				case "ERA0228":
					result = "GENERAL Commodity code must not contain Insulated details";
					break;
				case "ERA0229":
					result = "HAZARDOUS Commodity code must contain Hazardous details";
					break;
				case "ERA0230":
					result = "HAZARDOUS Commodity code must not contain BreakBulk details";
					break;
				case "ERA0231":
					result = "HAZARDOUS Commodity code must not contain Empty details";
					break;
				case "ERA0232":
					result = "HAZARDOUS Commodity code must not contain EmptyHaz details";
					break;
				case "ERA0233":
					result = "HAZARDOUS Commodity code must not contain General details";
					break;
				case "ERA0234":
					result = "HAZARDOUS Commodity code must not contain HazReefer details";
					break;
				case "ERA0235":
					result = "HAZARDOUS Commodity code must not contain Insulated details";
					break;
				case "ERA0236":
					result = "HAZREEFER Commodity code must contain Hazardous details";
					break;
				case "ERA0237":
					result = "HAZREEFER Commodity code must not contain BreakBulk details";
					break;
				case "ERA0238":
					result = "HAZREEFER Commodity code must not contain Empty details";
					break;
				case "ERA0239":
					result = "HAZREEFER Commodity code must not contain EmptyHaz details";
					break;
				case "ERA0240":
					result = "HAZREEFER Commodity code must not contain General details";
					break;
				case "ERA0241":
					result = "HAZREEFER Commodity code must not contain Insulated details";
					break;
				case "ERA0242":
					result = "HAZREEFER Commodity code must not contain OutOfGauge details";
					break;
				case "ERA0243":
					result = "HAZREEFER ISO code must contain “3” or “R” in 3rd character";
					break;
				case "ERA0244":
					result = "Incorrect PACKING GROUP supplied for HAZARDOUS Commodity";
					break;
				case "ERA0245":
					result = "INSULATED Commodity code must not contain BreakBulk details";
					break;
				case "ERA0246":
					result = "INSULATED Commodity code must not contain Empty details";
					break;
				case "ERA0247":
					result = "INSULATED Commodity code must not contain EmptyHaz details";
					break;
				case "ERA0248":
					result = "INSULATED Commodity code must not contain General details";
					break;
				case "ERA0249":
					result = "INSULATED Commodity code must not contain Hazardous details";
					break;
				case "ERA0250":
					result = "INSULATED Commodity code must not contain HazReefer details";
					break;
				case "ERA0251":
					result = "INSULATED Commodity code must not contain OutOfGauge details";
					break;
				case "ERA0252":
					result = "INSULATED Commodity code must not contain VENT Settings";
					break;
				case "ERA0253":
					result = "INSULATED Commodity ISO code must contain “H” in the 3rd character";
					break;
				case "ERA0254":
					result = "OUTOFGAUGE Commodity code must not contain BreakBulk details";
					break;
				case "ERA0255":
					result = "OUTOFGAUGE Commodity code must not contain Empty details";
					break;
				case "ERA0256":
					result = "OUTOFGAUGE Commodity code must not contain EmptyHaz details";
					break;
				case "ERA0257":
					result = "OUTOFGAUGE Commodity code must not contain General details";
					break;
				case "ERA0258":
					result = "OUTOFGAUGE Commodity code must not contain HazReefer details";
					break;
				case "ERA0259":
					result = "OUTOFGAUGE Commodity code must not contain Insulated details";
					break;
				case "ERA0260":
					result = "PACKING GROUP not supplied for HAZARDOUS Commodity";
					break;
				case "ERA0261":
					result = "REEFER Commodity code must contain Reefer details";
					break;
				case "ERA0262":
					result = "REEFER Commodity code must not contain BreakBulk details";
					break;
				case "ERA0263":
					result = "REEFER Commodity code must not contain Empty details";
					break;
				case "ERA0264":
					result = "REEFER Commodity code must not contain EmptyHaz details";
					break;
				case "ERA0265":
					result = "REEFER Commodity code must not contain General details";
					break;
				case "ERA0266":
					result = "REEFER Commodity code must not contain Hazardous details";
					break;
				case "ERA0267":
					result = "REEFER Commodity code must not contain HazReefer details";
					break;
				case "ERA0268":
					result = "REEFER Commodity code must not contain Insulated details";
					break;
				case "ERA0269":
					result = "REEFER ISO must have 3rd character of ISO code as “R” or “3”";
					break;
				case "ERA0500":
					result = "Invalid CAN reference";
					break;
				case "ERA0501":
					result = "Arrival mode of transport not defined";
					break;
				case "ERA0502":
					result = "Unknown arrival transport carrier ABN - cannot use in autogate";
					break;
				case "ERA0503":
					result = "Origin code not defined";
					break;
				case "ERA0504":
					result = "Origin code not supplied";
					break;
				case "ERA0505":
					result = "Reefer container should have a reefer commodity";
					break;
				case "ERA0900":
					result = "Most recent PRA existed";
					break;
				case "ERA0901":
					result = "Invalid date supplied";
					break;
				case "ERA0902":
					result = "Container number with invalid characters";
					break;
				case "ERA0903":
					result = "Temperature supplied for non REEFER commodity code";
					break;
				case "ERA0904":
					result = "Weight exceeds container minimum";
					break;
				case "ERA0905":
					result = "Container is not REEFER ISO type";
					break;
				case "ERA0906":
					result = "Hazard details exist but commodity not Hazard";
					break;
				case "ERA0907":
					result = "Vessel already departed";
					break;
				case "ERA0908":
					result = "Pre-advise too early not allowed at this time";
					break;
				case "ERA0999":
					result = "Invalid shipping line booking reference";
					break;
				case "ERA9999":
					result = "Unspecified errors";
					break;
				case "PBL0001":
					result = "Could not find matching Booking List SL/Lloyds/Voyage";
					break;
				case "PBL0002":
					result = "Matching Booking Reference not found";
					break;
				case "PBL0003":
					result = "Booking number does not match Line Operator";
					break;
				case "PBL0004":
					result = "Booking Found all container slots are FULL";
					break;
				case "PBL0005":
					result = "STREQR received CAN is INVALID";
					break;
				case "PBL0006":
					result = "CANs are not being accepted";
					break;
				case "PBL0007":
					result = "ECNs are not being accepted";
					break;
				case "PBL0010":
					result = "Load Port does not match";
					break;
				case "PBL0011":
					result = "Discharge Port does not match";
					break;
				case "PBL0012":
					result = "Discharge Port and Final Discharge Port do not match";
					break;
				case "PBL0013":
					result = "Discharge Port does not match Booking Discharge or Final Discharge Port";
					break;
				case "PBL0014":
					result = "Could not match PRA to a Booking in the Booking List";
					break;
			}
			return result ?? "(unknown error code)";
		}

		#endregion

		#region Response Email

		void AddContainerDetailsToResponseEmail(EmailDef responseEmail, IPRAMessageManager messageWriter)
		{
			ContainerMessagingData containerMessagingData = messageWriter.ContainerMessagingData;
			if (containerMessagingData == null)
			{
				return;
			}

			RefContainer refContainer = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, containerMessagingData.ISOContainerType);
			ZString iSODescription = (refContainer != null && refContainer.ISOType != null ? refContainer.ISOType.Description.ToString() : "");

			RefUNLOCO refUNLOCO = factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, containerMessagingData.PortOfDischarge);
			ZString portOfDischargeDescription = (refUNLOCO != null ? refUNLOCO.RL_PortName.ToString() : "");

			StringBuilder containerDetails = new StringBuilder(responseEmail.Body + "\r\n");
			containerDetails.Append(PadCentre("Container Details", 70) + "\r\n");
			containerDetails.Append("Container No      : " + containerMessagingData.ContainerNumber + "\r\n");
			containerDetails.Append("ISO Container Type: " + containerMessagingData.ISOContainerType + (iSODescription.IsEmpty ? "" : " - " + iSODescription) + "\r\n");
			containerDetails.Append("Container Seal No : " + containerMessagingData.SealNumber + "\r\n");
			containerDetails.Append("Weight            : " + containerMessagingData.ContainerGrossWeight.ToString() + " Kg\r\n");
			containerDetails.Append("Commodity         : " + containerMessagingData.Commodity1StopCode + "-" + containerMessagingData.GoodsDescription + "\r\n");

			if (containerMessagingData.IsTempControlled)
			{
				containerDetails.Append("Temp. Setting     : " + containerMessagingData.TemperatureSettingFormatted + " Degrees Celcius." + "\r\n");
			}

			containerDetails.Append(PadCentre("Shipping Details", 70) + "\r\n");
			containerDetails.Append("Vessel            : " + containerMessagingData.VesselName + "\r\n");
			containerDetails.Append("Voyage            : " + containerMessagingData.Voyage + "\r\n");
			containerDetails.Append("Departure Date    : " + messageWriter.DepartureDate.ToString() + "\r\n");
			containerDetails.Append("Port of Discharge : " + containerMessagingData.PortOfDischarge + " - " + portOfDischargeDescription + "\r\n");
			containerDetails.Append("Receival Wharf    : " + containerMessagingData.LoadTerminal1StopCode + " - " + new MessageSenderPairList().GetDescriptionFromCode(containerMessagingData.LoadTerminal1StopCode) + "\r\n");

			containerDetails.Append(PadCentre("Clearance Numbers", 70) + "\r\n");
			containerDetails.Append("CAN               : " + containerMessagingData.ECNorCRN + "\r\n");

			containerDetails.Append("".PadRight(70, '-'));

			responseEmail.Body = containerDetails.ToString();
			responseBody = responseEmail.Body;
		}

		EmailDef CompileResponseEmail(IPRAMessageManager writer)
		{
			string pRASummary = "PRA " + writer.LastMessageTypeSent + (messageReceivedWithoutError ? "Accepted" : "Rejected");
			string subject = pRASummary + " for " + writer.JobType + ": " + writer.JobReference + " Container: " + writer.ContainerNumber;
			string body = PadCentre("Response Status: " + pRASummary, 70) + "\r\n"
				+ writer.JobType.PadRight(17) + " : " + writer.JobReference + "\r\n"
				+ "Container No      : " + writer.ContainerNumber + "\r\n"
				+ "Response From     : " + responseSender + "\r\n"
				+ statusString.ToString()
				+ "".PadRight(70, '-') + "\r\n";

			return CreateEmail(subject, body);
		}

		EmailDef CreateEmail(string subject, string body)
		{
			EmailDef responseEmail = new EmailDef();
			responseEmail.Subject = subject;
			responseEmail.Body = body;
			responseBody = body;
			return responseEmail;
		}

		string PadCentre(string input, int width)
		{
			string myInput = " " + input + " ";
			return ("".PadRight((width - myInput.Length) / 2, '-') + myInput).PadRight(width, '-');
		}

		void AddStatusLine(string bodyTag, ZString bodyText)
		{
			if (!bodyText.IsEmpty)
			{
				statusString.Append(bodyTag.PadRight(18) + "- " + bodyText + "\r\n");
			}
		}

		void AddStatusLine(string bodyLine)
		{
			statusString.Append(bodyLine + "\r\n");
		}

		protected override ZString ApplicationCodeForGetUserToNotify
		{
			get { return EDIMessage.ApplicationCodes.OneStop; }
		}

		protected override ZGuid AcknowledgementEmailGroup
		{
			get { return Env.Registry.Freight.PRAMessaging.AcknowledgementEmailGroup; }
		}

		protected override ZString AcknowledgementEmailMode
		{
			get { return Env.Registry.Freight.PRAMessaging.AcknowledgementEmailMode; }
		}

		protected override ZGuid ImpedimentEmailGroup
		{
			get { return Env.Registry.Freight.PRAMessaging.ImpedimentEmailGroup; }
		}

		protected override ZString ImpedimentEmailMode
		{
			get { return Env.Registry.Freight.PRAMessaging.ImpedimentEmailMode; }
		}

		protected override ZGuid ErrorEmailGroup
		{
			get { return Env.Registry.Freight.PRAMessaging.ErrorEmailGroup; }
		}

		protected override ZString ErrorEmailMode
		{
			get { return Env.Registry.Freight.PRAMessaging.ErrorEmailMode; }
		}

		#endregion
	}
}
