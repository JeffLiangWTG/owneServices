using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS316;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.CusTempStorage;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS316Processor))]
	sealed class TS316ProcessorTest : TemporaryStorageHeaderMessageProcessorTest<TS316Processor, AISInboundEDIMessage, AISOutboundEDIMessage, TS316Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS316;

		protected override ZString MessageText => Serialize(new Ts316()
		{
			Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarationType11()
			{
				Lrn = "LRN123456789",
				RejectionDate = new DateTime(2023, 09, 07),
				RejectionMotivationText = "Sample Text 123",
			},
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
			},
			SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.SupervisingcustomofficeType
			{
				ReferenceNumber = "IEDUB100",
			},
			CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType01
			{
				ReferenceNumber = "IEDUB100",
			},
			Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarantType03
			{
				IdentificationNumber = "TEST",
			}
		});

		protected override ZString MessageFriendlyName => "TS316 - Temporary Storage Declaration Invalidation Decision";

		protected override TS316Processor Processor => new TS316Processor(logger, typeof(Ts316));

		protected override void AssertProcessResultCore(TemporaryStorageHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("AMA_MessageStatus", LogicalStatusList.Codes.Invalid, messageAttachee.AMA_MessageStatus);
			AssertEquals("CustomsStatus", AISEntryStatusList.Codes.Rejected, messageAttachee.CustomsStatus);

			AssertMessageInterpretation(incomingMessage, @"A TSD Rejection (TS316) message has been received for Job MAN0001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td colspan=""2"">TS316 – Temporary Storage Declaration Rejection message</td></tr><tr><td>LRN</td><td>LRN123456789</td></tr><tr><td>Declaration Rejection Date</td><td>07-Sep-23</td></tr><tr><td>Declaration Rejection Reason</td><td>Sample Text 123</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr><tr><td>Error Code</td><td>52</td></tr><tr><td>Error Code Description</td><td>52</td></tr><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail("TS316: Temporary Storage Declaration Rejection",
				new[] { "A TSD Rejection (TS316) message has been received for Job MAN0001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
