using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM409;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using IM409Provider = Enterprise.Customs.IE.Messaging.UCC5.IM409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM409Processor))]
	sealed class IM409ProcessorTest : EntryHeaderMessageProcessorTest<IM409Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM409;

		protected override ZString MessageText => Serialize(GetMessageObject());

		protected override ZString MessageFriendlyName => "IM409: Invalidation Request Decision";

		protected override IM409Processor Processor => new IM409Processor(logger, typeof(Im409));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Accepted, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Cancelled, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, IM409MessageInterpreterTest.ExpectedInterpretation);

			MessageProcessorNotificationTestHelper.AssertEmail(
				$"{incomingMessage.MessageTypeWithDescription} Response for {AISInterchangeProcessorTestHelper.Typical.JobNumber}",
				new[] { $"An Invalidation Request Decision (IM409) message has been received from customs for Job {AISInterchangeProcessorTestHelper.Typical.JobNumber}." },
				new string[] { "staff1@where.com" });
		}
		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return base.CreateSetupData();
		}

		Im409 GetMessageObject(bool isExistFunctionalError = false)
		{
			return new Im409
			{
				Declaration = new DeclarationType()
				{
					DateOfInvalidation = "20240107",
					DateOfInvalidationDecision = "20240208",
					DateOfInvalidationRequest = "20240309",
					InvalidationDecision = true,
					InvalidationInitiatedByCustoms = false,
					InvalidationJustification = "Invalidation Justification",
					Mrn = "12MRN345CDEFG678R9",
					CustomsOffices = new DeclarationTypeCustomsOffices()
					{
						CustomsOfficeLodgement = "IEDUB100"
					}
				},
				FunctionalError = AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
			};
		}
	}
}
