using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM405;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM405Processor))]
	class IM405ProcessorTest : EntryHeaderMessageProcessorTest<IM405Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM405Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM405;

		protected override ZString MessageText => Serialize(new Im405
		{
			Declaration = new DeclarationType
			{
				AmendmentRejectionDate = "20240229",
				AmendmentRejectionMotivationText = "Amendment Rejection Reason",
				CustomsOffices = new DeclarationTypeCustomsOffices
				{
					PresentationCustomsOffice526 = "IEDUB400",
					SupervisingCustomsOffice527 = "IEDUB100",
					CustomsOfficeLodgement = "IEDUB400",
				},
				Mrn = "12MRN345CDEFG678R9",
				Remarks = "Remarks001",
				Parties = { },
			},
			FunctionalError = AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
		});

		protected override ZString MessageFriendlyName => "IM405: Amendment Request Rejection";

		protected override IM405Processor Processor => new IM405Processor(logger, typeof(Im405));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, IM405MessageInterpreterTest.ExpectedInterpretation);

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "An Amendment Request Rejection (IM405) message has been received from customs for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override (JobDeclaration declaration, CusEntryHeader messageAttachee, EDIMessage outgoingMessage, AISUCC5InboundEDIMessage incomingMessage) CreateSetupData()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return base.CreateSetupData();
		}
	}
}
