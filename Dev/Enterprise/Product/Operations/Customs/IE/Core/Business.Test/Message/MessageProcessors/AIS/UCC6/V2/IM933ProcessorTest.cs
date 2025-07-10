using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM933;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM933Processor))]
	class IM933ProcessorTest : EntryHeaderMessageProcessorTest<IM933Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM933Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM933;

		protected override ZString MessageText => GetMessageText();

		protected virtual ZString GetMessageText() => Serialize(new Im933
		{
			ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType433()
			{
				Mrn = "12MRN345CDEFG678R9",
				RejectionDate = new DateTime(2023, 09, 22, 14, 30, 45),
				RejectionReason = "Rejection Reason Text"
			},
			CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "LCO12345" },
			Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MDeclarantType { IdentificationNumber = "DEC001" },
			FunctionalError = new System.Collections.ObjectModel.Collection<CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01>
				{
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
					{
						SequenceNumber = "1",
						ErrorPointer = "ErrorPointer001",
						ErrorCode = "13",
						ErrorReason = "ER1",
						Remarks = "Functional Error Remarks 1",
						OriginalAttributeValue = "Original Attribute Value 1",
					},
					new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
					{
						SequenceNumber = "2",
						ErrorPointer = "ErrorPointer002",
						ErrorCode = "52",
						ErrorReason = "ER2",
						Remarks = "Functional Error Remarks 2",
						OriginalAttributeValue = "Original Attribute Value 2",
					},
				}
		});

		protected override ZString MessageFriendlyName => "IM933: Presentation Notification Rejection";

		protected override IM933Processor Processor => new IM933Processor(logger, typeof(Im933));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);

			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Prelodged, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Presentation Notification Rejection (IM933) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Rejection Date</td><td>22-Sep-23</td></tr><tr><td>Rejection Reason</td><td>Rejection Reason Text</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr><tr><td>Error Code</td><td>52</td></tr><tr><td>Error Code Description</td><td>52</td></tr><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Presentation Notification Rejection (IM933) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
