using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RD416;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(RD416Processor))]
	sealed class RD416ProcessorTest : EntryHeaderMessageProcessorTest<RD416Processor, AISInboundEDIMessage, AISOutboundEDIMessage, RD416Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RD416;

		protected override ZString MessageText
		{
			get
			{
				AISInterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
				return Serialize(GenerateMessage());
			}
		}

		protected override ZString MessageFriendlyName => "RD416: Deposit Refund Application Rejection";

		protected override RD416Processor Processor => new RD416Processor(logger, typeof(Rd416Type));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, RD416MessageInterpreterTest.ExpectedInterpretation);

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
			   new[] { "A Deposit Refund Application Rejection (RD416) has been received for Job B00001000." },
			   new string[] { "staff1@where.com" });
		}

		internal static Rd416Type GenerateMessage()
		{
			return new Rd416Type
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "Abcd123aisdu3eh2u8",
					RejectionDate = "20230802",
					RejectionReason = "Test Reason",
					Applicant = "XY",
				},
				FunctionalError = AISInterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
			};
		}
	}
}
