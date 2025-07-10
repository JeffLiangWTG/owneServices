using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM433;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging.UCC5;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM433Processor))]
	sealed class IM433ProcessorTest : EntryHeaderMessageProcessorTest<IM433Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, IM433Provider>
	{
		protected override ZString MessageType => Messaging.AISInterchangeTypeList.Codes.IM433;

		protected override ZString MessageFriendlyName => "IM433: Presentation Notification Rejection";

		protected override IM433Processor Processor => new IM433Processor(logger, typeof(Im433));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);
			AssertEquals("CH_EntryStatus", AISEntryStatusList.Codes.Rejected, messageAttachee.CH_EntryStatus);

			AssertMessageInterpretation(incomingMessage, IM433MessageInterpreterTest.ExpectedInterpretation);

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Presentation Notification Rejection (IM433) message has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageText
		{
			get
			{
				AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
				return AISUCC5InterchangeProcessorTestHelper.GetStandardUCC5IM433Text();
			}
		}
	}
}
