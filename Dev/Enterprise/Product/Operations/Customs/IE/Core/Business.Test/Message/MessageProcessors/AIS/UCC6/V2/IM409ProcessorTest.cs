using System;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.IM409;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(IM409Processor))]
	sealed class IM409ProcessorTest : EntryHeaderMessageProcessorTest<IM409Processor, AISInboundEDIMessage, AISOutboundEDIMessage, IIM409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM409;

		protected override ZString MessageText => GetMessageText(true);

		ZString GetMessageText(bool invalidationDecision = false)
		{
			AISInterchangeProcessorTestHelper.CreateCL180ReferenceTestData(Factory);

			return Serialize(
				new Im409
				{
					ImportOperation = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MCciOperationType409
					{
						Mrn = "12MRN345CDEFG678R9",
						InvalidationDecision = invalidationDecision,
						InvalidationInitiatedByCustoms = true,
						InvalidationJustification = "Invalidation Justification",
						DateOfInvalidationDecision = new DateTime(2023, 08, 11, 14, 30, 45),
						DateOfInvalidationRequest = new DateTime(2023, 08, 10, 14, 30, 45),
						DateOfInvalidation = new DateTime(2023, 08, 09, 14, 30, 45),
					},
					CustomsOfficeLodgement = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MScoType { ReferenceNumber = "LCO12345" },
					Representative = new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MRepresentativeType { IdentificationNumber = "REP001", Status = "0" },
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
							OriginalAttributeValue = "Original Attribute Value 1"
						},
						new CargoWise.Customs.IE.MessageDefinitions.AISVersion2_0.ctypes.MFunctionalErrorType01
						{
							SequenceNumber = "2",
							ErrorPointer = "ErrorPointer002",
							ErrorCode = "52",
							ErrorReason = "ER2",
							Remarks = "Functional Error Remarks 2",
							OriginalAttributeValue = "Original Attribute Value 2"
						}
					}
				});
		}

		protected override ZString MessageFriendlyName => "IM409: Invalidation Request Decision";

		protected override IM409Processor Processor => new IM409Processor(logger, typeof(Im409));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Cancelled, messageAttachee.CH_EntryStatus);
			AssertMessageInterpretation(incomingMessage, IM409MessageInterpreterTest.GetExpectedInterpretation(true));

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "[IM409 - Invalidation Decision] has been received and linked to job B00001000. Decision: Invalidation Rejected" },
				new string[] { "staff1@where.com" });
		}

		public void TestAssertWhenInvalidationDecisionIsFalse()
		{
			(_, var messageAttachee, _, var incomingMessage) = CreateSetupData();
			var messageText = GetMessageText(false);
			incomingMessage.EM_MessageText = InterchangeProcessorTestHelper.GetMailboxItemText(TransactionID, messageText, includeResponseWrap: false);
			using (incomingMessage.Factory.AddDisposableService())
			{
				var processor = Processor;
				processor.PreProcessMessage(incomingMessage);
				processor.ProcessMessage(incomingMessage);

				AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);
				AssertEquals("CH_EntryStatus", string.Empty, messageAttachee.CH_EntryStatus);
				AssertMessageInterpretation(incomingMessage, IM409MessageInterpreterTest.GetExpectedInterpretation(false));

				MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
					new[] { "[IM409 - Invalidation Decision] has been received and linked to job B00001000. Decision: Invalidation Accepted" },
					new string[] { "staff1@where.com" });
			}
		}
	}
}
