using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS376;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS376Processor))]
	class TS376ProcessorTest : EntryHeaderMessageProcessorTest<TS376Processor, AISInboundEDIMessage, AISOutboundEDIMessage, TS376Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS376;

		protected override ZString MessageText => Serialize(new Ts376
		{
			Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarationType22
			{
				Lrn = "LRN001",
				Mrn = "12MRN345ABCDE678R9",
				RejectionDate = new DateTime(2023, 08, 10, 14, 30, 45),
				RejectionMotivationText = "Rejection Motivation Text",
			},
			SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.ScoType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType01 { ReferenceNumber = "LCO12345" },
			Declarant = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarantType03 { IdentificationNumber = "ID1" },
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

		protected override ZString MessageFriendlyName => "TS376: Goods Status Report Declaration (ND4) Rejection";

		protected override TS376Processor Processor => new TS376Processor(logger, typeof(Ts376));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);

			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Rejected, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, @"A Goods Status Report Declaration (ND4) Rejection (TS376) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr><tr><td>Rejection Date</td><td>10-Aug-23</td></tr><tr><td>Rejection Motivation Text</td><td>Rejection Motivation Text</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr><tr><td>Error Code</td><td>52</td></tr><tr><td>Error Code Description</td><td>52</td></tr><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Goods Status Report Declaration (ND4) Rejection (TS376) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}
	}
}
