using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Business.MessageProcessors;
using Enterprise.Customs.CA.Business.MessageProcessors.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.Common.MessageBuilders.Testing;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.MessageManagers.Testing
{
	[TestedType(typeof(ACIEManifestForwaderStatusCalculator))]
	sealed class ACIEManifestForwaderStatusCalculatorTest : EDIFACTMessageStatusCalculatorTestCase
	{
		public void TestCalculateJobStatusErrorMessage()
		{
			var house = Factory.New<CusCAeMHHouse>();
			var message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageNum = "50";
			message.EM_MessageText = "message text";
			message.EM_MessageType = "AAA";
			IEnumerable<EDIMessage> messages = new[] { message };
			((ACIEManifestForwaderStatusCalculator)calculator).CalculateJobStatus(messages, house);
			AssertEquals("A received eManifest Forwarder message is not a valid ACIForwarderMessage, when it was expected to be. Job HBL-, message no 50, interchange , message type AAA, message text message text", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		#region TestCalculatedJobStatus

		public override void TestCalculatedJobStatus()
		{
			var master = Factory.New<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			AssertEquals("No status", ZString.Empty, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "50", EManifestResponseTest.DataErrorMessageText);
			AssertEquals("No status", ZString.Empty, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "55", EManifestResponseTest.ValidationErrorMessageText);
			AssertEquals("Error", EManifestForwarderJobStatusList.Codes.Error, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "60", EManifestResponseTest.ContentAcceptedMessageText);
			AssertEquals("Validated", EManifestForwarderJobStatusList.Codes.Validated, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "100", EManifestResponseTest.NotMatchedMessageText);
			AssertEquals("Not Matched", EManifestForwarderJobStatusList.Codes.NotMatched, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "110", EManifestResponseTest.MatchedMessageText);
			AssertEquals("Matched", EManifestForwarderJobStatusList.Codes.Clear, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "120", EManifestResponseTest.ContentAcceptedMessageText);
			AssertEquals("Still Matched", EManifestForwarderJobStatusList.Codes.Clear, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "130", EManifestResponseTest.ValidationErrorMessageText);
			AssertEquals("Error", EManifestForwarderJobStatusList.Codes.Error, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "135", EManifestResponseTest.ContentAcceptedMessageText);
			AssertEquals("Still Matched", EManifestForwarderJobStatusList.Codes.Clear, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "140", EManifestResponseTest.ContentAcceptedMessageText, MessageSubTypeCodes.Codes.Cancellation);
			AssertEquals("Cancelled", EManifestForwarderJobStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "150", EManifestResponseTest.ContentAcceptedMessageText);
			AssertEquals("Validated", EManifestForwarderJobStatusList.Codes.Validated, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "160", EManifestResponseTest.MatchedMessageText);
			AssertEquals("Matched", EManifestForwarderJobStatusList.Codes.Clear, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "180", EManifestResponseTest.ContentAcceptedMessageText);
			AssertEquals("Still Matched", EManifestForwarderJobStatusList.Codes.Clear, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "190", EManifestResponseTest.ContentAcceptedMessageText, MessageSubTypeCodes.Codes.Cancellation);
			AssertEquals("Cancelled", EManifestForwarderJobStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "200", EManifestResponseTest.NotMatchedMessageText);
			AssertEquals("Still Cancelled", EManifestForwarderJobStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "210", EManifestResponseTest.ContentAcceptedMessageText);
			AssertEquals("Validated", EManifestForwarderJobStatusList.Codes.Validated, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "220", EManifestResponseTest.ContentAcceptedMessageText, MessageSubTypeCodes.Codes.Cancellation);
			AssertEquals("Cancelled", EManifestForwarderJobStatusList.Codes.Cancelled, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "220", EManifestResponseTest.ContentAcceptedMessageText);
			AssertEquals("Validated", EManifestForwarderJobStatusList.Codes.Validated, calculator.CalculatedJobStatus(house));

			AddEDIMessage(house, "230", EManifestResponseTest.NotMatchedMessageText);
			AssertEquals("Not Matched", EManifestForwarderJobStatusList.Codes.NotMatched, calculator.CalculatedJobStatus(house));

			master.BP_CustomsStatus = EManifestForwarderJobStatusList.Codes.Clear;
			AddEDIMessage(master, "250", EManifestResponseTest.ContentAcceptedMessageText);
			AssertEquals("still CLR", EManifestForwarderJobStatusList.Codes.Clear, calculator.CalculatedJobStatus(master));
		}

		void AddEDIMessage(IEDIFACTMessageAttachee linkedObject, string messageNum, string messageText)
		{
			AddEDIMessage(linkedObject, messageNum, messageText, MessageSubTypeCodes.Codes.Original);
		}

		void AddEDIMessage(IEDIFACTMessageAttachee linkedObject, string messageNum, string messageText, string messageSubType)
		{
			var message = Factory.New<ACIForwarderMessage>();
			message.EM_ApplicationCode = Enterprise.Messaging.Business.EDIMessage.ApplicationCodes.CAACI;
			message.EM_MessageType = MessageTypeList.Codes.ACIHouseBill;
			message.EM_MessageSubType = messageSubType;
			message.EM_ReceiveTransmit = Enterprise.Messaging.Business.EDIMessage.Direction.Receive;
			message.EM_MessageNum = messageNum;
			message.EM_MessageText = messageText.Replace("\r\n", "'");

			linkedObject.AddMessage(message);
		}

		#endregion

		public override void TestMessageTypeDescription()
		{
			AssertEquals("MessageTypeDescription", MessageTypeList.Descriptions.ACIHouseBill, calculator.MessageTypeDescription);
		}

		public void TestGetMessageSubType()
		{
			var newFactory = new BusinessObjectFactory();
			var master = newFactory.NewWithValidTestData<CusCAeMHMaster>();
			var house = master.HouseBills.AddNew();
			house.BW_MessageReference = "803636474747";

			var dec = newFactory.NewWithValidTestData<JobDeclaration>();
			dec.JE_GC = GlbCompany.CurrentCompany.PK;
			dec.JE_MessageType = JobMessageTypeList.Codes.Import;
			dec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var invoice = dec.Invoices.AddNew();
			invoice.InvoiceLines.AddNew();
			var number = dec.AdditionalReferenceNumbers.AddNew();
			number.CE_RN_NKCountryCode = Core.Constants.CountryCodes.Canada;
			number.CE_EntryType = CanadaAdditionalReferenceNumberTypes.Codes.CCN;
			number.CE_EntryNum = "803636474747";
			dec.DoMerge();
			newFactory.Save();

			var message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+313+803636474747+11'DTM+9:201307311207:203'RFF+AGO:HBL-803636474747'RCS+11'";
			var wrapper = new EManifestResponseWrapper(message);
			var calculator = new ACIEManifestForwaderStatusCalculator(wrapper, MessageTypeList.Descriptions.ACIHouseBill);
			AssertEquals(MessageSubTypeCodes.Codes.Original, calculator.GetMessageSubType(MessageStatusList.Codes.AwaitingOriginal));
			AssertEquals(MessageSubTypeCodes.Codes.Change, calculator.GetMessageSubType(MessageStatusList.Codes.AwaitingChange));
			AssertEquals(MessageSubTypeCodes.Codes.Cancellation, calculator.GetMessageSubType(MessageStatusList.Codes.AwaitingDelete));

			message = Factory.New<ACIForwarderMessage>();
			message.EM_MessageType = "XYZ";
			message.EM_MessageText = "UNH+1+GOVCBR:D:11B:UN'BGM+313+803636474747+11'DTM+9:201307311207:203'RCS+11'";
			message.EM_GB = GlbBranch.CurrentBranch.PK;
			wrapper = new EManifestResponseWrapper(message);
			AssertEquals(dec.JE_DeclarationReference, wrapper.LinkedObject.JobIdentification);
			calculator = new ACIEManifestForwaderStatusCalculator(wrapper, MessageTypeList.Descriptions.ACIHouseBill);
			AssertEquals("XYZ", calculator.GetMessageSubType(MessageStatusList.Codes.AwaitingOriginal));
			AssertEquals("XYZ", calculator.GetMessageSubType(MessageStatusList.Codes.AwaitingChange));
			AssertEquals("XYZ", calculator.GetMessageSubType(MessageStatusList.Codes.AwaitingDelete));
		}

		protected override EDIFACTMessageStatusCalculator GetCalculator()
		{
			return new ACIEManifestForwaderStatusCalculator(MessageTypeList.Descriptions.ACIHouseBill);
		}
	}
}
