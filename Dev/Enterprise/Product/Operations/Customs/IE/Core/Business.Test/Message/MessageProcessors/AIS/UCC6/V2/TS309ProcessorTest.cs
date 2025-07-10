using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.TS309;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(TS309Processor))]
	class TS309ProcessorTest : EntryHeaderMessageProcessorTest<TS309Processor, AISInboundEDIMessage, AISOutboundEDIMessage, TS309Provider>
	{
		public void TestNeedToSendEmailNotification()
		{
			(var relatedJob, var messageAttachee, var outgoingMessage, var incomingMessage) = CreateSetupData();
			var processor = new TS309ProcessorForTest(logger, typeof(Ts309));
			AssertEquals("NeedToSendEmailNotification", true, processor.NeedToSendEmailNotification_Exposed(incomingMessage));
		}

		public void TestMessageInterpreterType()
		{
			var processor = new TS309ProcessorForTest(logger, typeof(Ts309));
			AssertEquals("Interpreter Type", typeof(TS309MessageInterpreter), processor.MessageInterpreterType_Exposed);
		}

		protected override ZString MessageFriendlyName => "TS309: Temporary Storage Declaration Invalidation Decision";

		protected override ZString MessageType => AISInterchangeTypeList.Codes.TS309;

		protected override ZString MessageText => Serialize(new Ts309
		{
			Declaration = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.DeclarationType04
			{
				Mrn = "12MRN345ABCDE678R9",
				InvalidationDecision = true,
				InvalidationInitiatedByCustoms = true,
				InvalidationJustification = "Invalidation Justification Text",
				DateOfInvalidationDecision = new DateTime(2023, 08, 10, 14, 30, 45),
				DateOfInvalidationRequest = new DateTime(2023, 08, 11, 14, 30, 45),
				DateOfInvalidation = new DateTime(2023, 08, 12, 14, 30, 45)
			},
			SupervisingCustomsOffice = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.SupervisingcustomofficeType { ReferenceNumber = "SCO12345" },
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
				}
		});

		protected override TS309Processor Processor => new TS309Processor(logger, typeof(Ts309));

		protected override void AssertProcessResultCore(CusEntryHeader entry, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, entry.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Cancelled, entry.CH_EntryStatus);

			var summary = "Temporary Storage Declaration Invalidation Decision (TS309) has been received and linked to job B00001000. Decision: Invalidation Request Accepted.";
			AssertMessageInterpretation(incomingMessage, summary + GetExpectedMessageInterpretation("Y"));

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000", new[] { summary }, new string[] { "staff1@where.com" });
		}

		public void TestInvalidationDecisionIsFalse()
		{
			var (declaration, entry, outgoingMessage, incomingMessage) = CreateSetupData();
			incomingMessage.EM_MessageText = incomingMessage.EM_MessageText.Replace("<invalidationDecision>true</invalidationDecision>", "<invalidationDecision>false</invalidationDecision>");
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				AssertNoExceptionThrown(() => processor.ProcessMessage(incomingMessage));
				CombineAssertions("Process", () =>
				{
					AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, entry.CH_Status);
					AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Registered, entry.CH_EntryStatus);

					var summary = "Temporary Storage Declaration Invalidation Decision (TS309) has been received and linked to job B00001000. Decision: Invalidation Request Rejected.";
					AssertMessageInterpretation(incomingMessage, summary + GetExpectedMessageInterpretation("N"));
					MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000", new[] { summary }, new string[] { "staff1@where.com" });
				});
			}
		}

		string GetExpectedMessageInterpretation(string invalidation) => $@"
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<tr><td>MRN</td><td>12MRN345ABCDE678R9</td></tr>
<tr><td>Invalidation Decision</td><td>{invalidation}</td></tr>
<tr><td>Invalidation Initiated by Customs</td><td>Y</td></tr>
<tr><td>Invalidation Justification</td><td>Invalidation Justification Text</td></tr>
<tr><td>Date of Invalidation Decision</td><td>10-Aug-23</td></tr>
<tr><td>Date of Invalidation Request</td><td>11-Aug-23</td></tr>
<tr><td>Date of Invalidation</td><td>12-Aug-23</td></tr>
</table>
<br />
<br />
Functional Error: 1
<br />
<br />
<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
<tr><td>Error Pointer</td><td>ErrorPointer001</td></tr>
<tr><td>Error Code</td><td>13</td></tr>
<tr><td>Error Code Description</td><td>&nbsp;</td></tr>
<tr><td>Error Reason</td><td>ER1</td></tr>
<tr><td>Remarks</td><td>Functional Error Remarks 1</td></tr>
<tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr>
</table>";

		sealed class TS309ProcessorForTest : TS309Processor
		{
			public TS309ProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
			{
			}

			public bool NeedToSendEmailNotification_Exposed(EDIMessage message) => NeedToSendEmailNotification(message);

			public Type MessageInterpreterType_Exposed => MessageInterpreterType;
		}
	}
}
