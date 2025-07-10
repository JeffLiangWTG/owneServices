using System;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM438;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business.CodeDescriptionPairLists;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM438Processor))]
	class IM438ProcessorTest : EntryHeaderMessageProcessorTest<IM438Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM438Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM438;

		protected override ZString MessageText => Serialize(new Im438
		{
			ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType01
			{
				Lrn = "LRN001",
				Mrn = "12MRN345ABCDE678R9",
			},
			SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "LCO12345" },
			Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MDeclarantType { IdentificationNumber = "DEC001" },
			ReminderDetails = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MReminderDetailsType
			{
				RequestDate = new DateTime(2023, 08, 10, 14, 30, 45),
				ExpirationDate = new DateTime(2023, 08, 11, 14, 30, 45),
			}
		});

		protected override ZString MessageFriendlyName => "IM438: Reminder for Providing Additional Documents";

		protected override IM438Processor Processor => new IM438Processor(logger, typeof(Im438));

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISInboundEDIMessage incomingMessage) CreateSetupData(string incomingMessageText = null)
		{
			var testData = base.CreateSetupData(incomingMessageText);
			var instruction = testData.messageAttachee.EntryInstruction;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.RequestOpened;
			instruction.RequestedDocuments.AddNew().CSI_Status = RequestedDocumentStatusList.Codes.RequestCancelled;
			return testData;
		}

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);

			var openRequestedDocuments = messageAttachee.EntryInstruction.RequestedDocuments.Where(x => x.IsOpen).ToArray();
			AssertEquals("Open RequestedDocument Count", 2, openRequestedDocuments.Length);
			var expirationDate = new DateTime(2023, 08, 11);
			Assert("Open RequestedDocument CSI_DateOfExpiry", openRequestedDocuments.All(x => x.CSI_DateOfExpiry == expirationDate));

			AssertMessageInterpretation(incomingMessage, @"A Reminder for Providing Additional Documents (IM438) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Request Date</td><td>10-Aug-23</td></tr><tr><td>Expiration Date</td><td>11-Aug-23</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Reminder for Providing Additional Documents (IM438) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
