using System;
using System.Text.RegularExpressions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS333;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS333Processor))]
	sealed class TS333ProcessorTest : EntryHeaderMessageProcessorTest<TS333Processor, AISInboundEDIMessage, AISOutboundEDIMessage, TS333Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS333;

		protected override ZString MessageText => Serialize(new Ts333
		{
			Declaration = new DeclarationType10
			{
				Mrn = "12MRN345CDEFG678R9",
				RejectionDate = new DateTime(2023, 08, 15, 14, 10, 59),
				RejectionReason = "Rejection Reason",
			},
			SupervisingCustomsOffice = new SupervisingcustomofficeType { ReferenceNumber = "SCO12345" },
			CustomsOfficeLodgement = new MScoType01 { ReferenceNumber = "LCO12345" },
			Declarant = new DeclarantType03 { IdentificationNumber = "ID1" },
			FunctionalError = new System.Collections.ObjectModel.Collection<MFunctionalErrorType01>
			{
				new MFunctionalErrorType01
				{
					SequenceNumber = "1",
					ErrorPointer = "ErrorPointer001",
					ErrorCode = "13",
					ErrorReason = "ER1",
					Remarks = "Functional Error Remarks 1",
					OriginalAttributeValue = "Original Attribute Value 1",
				},
				new MFunctionalErrorType01
				{
					SequenceNumber = "2",
					ErrorPointer = "ErrorPointer002",
					ErrorCode = "52",
					ErrorReason = "ER2",
					Remarks = "Functional Error Remarks 2",
					OriginalAttributeValue = "Original Attribute Value 2",
				}
			}
		});

		protected override ZString MessageFriendlyName => "TS333: Presentation Notification (G3) Rejection";

		protected override TS333Processor Processor => new TS333Processor(logger, typeof(Ts333));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_EntryStatus", CustomsWareEntryStatusList.Codes.Rejected, messageAttachee.CH_EntryStatus);
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, @"A Presentation Notification (G3) Rejection (TS333) message has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Rejection Date</td><td>15-Aug-23</td></tr><tr><td>Rejection Reason</td><td>Rejection Reason</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr><tr><td>Error Code</td><td>13</td></tr><tr><td>Error Code Description</td><td>13</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr></table><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr><tr><td>Error Code</td><td>52</td></tr><tr><td>Error Code Description</td><td>52</td></tr><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Remarks</td><td>Functional Error Remarks 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
	new[] { "A Presentation Notification (G3) Rejection (TS333) message has been received for Job B00001000." },
	new string[] { "staff1@where.com" });
		}

		public void TestFunctionalErrorsIsEmpty()
		{
			var (declaration, entry, outgoingMessage, incomingMessage) = CreateSetupData();
			incomingMessage.EM_MessageText = Regex.Replace(incomingMessage.EM_MessageText, @"<FunctionalError[^>]*>[\s\S]*?</FunctionalError>", string.Empty);
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertNullOrEmpty("CH_Status should be empty.", entry.CH_Status);
					AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Registered, entry.CH_EntryStatus);
				});
			}
		}
	}
}
