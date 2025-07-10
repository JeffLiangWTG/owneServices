using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM456;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM456Processor))]
	class IM456ProcessorTest : EntryHeaderMessageProcessorTest<IM456Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IM456Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM456;

		protected override ZString MessageText => Serialize(new Im456
		{
			ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType44
			{
				Lrn = "LRN001",
				Mrn = "12MRN345ABCDE678R9",
				CustomsRegistrationNumber = "12CRN345ABCDE678R9",
				BusinessRejectionType = "BRT",
				RejectionDateAndTime = new DateTime(2023, 08, 11, 14, 30, 45),
				RejectionCode = "1",
				RejectionReason = "Rejection Reason",
			},
			SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "SCO12345" },
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

		protected override ZString MessageFriendlyName => "IM456: Rejection from SCI";

		protected override IM456Processor Processor => new IM456Processor(logger, typeof(Im456));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);

			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Rejected, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Rejection from SCI (IM456) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>LRN</td><td>LRN001</td></tr><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Customs Registration Number</td><td>12CRN345ABCDE678R9</td></tr><tr><td>Business Rejection Type</td><td>BRT</td></tr><tr><td>Rejection Date and Time</td><td>11-Aug-23 14:30</td></tr><tr><td>Rejection Code</td><td>1</td></tr><tr><td>Rejection Reason</td><td>Rejection Reason</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr><tr><td>Error Code</td><td>52</td></tr><tr><td>Error Code Description</td><td>52</td></tr><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Rejection from SCI (IM456) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
