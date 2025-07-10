using CargoWise.Customs.IE.MessageDefinitions.AISVersion2_2.RF416;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.AIS.Testing
{
	[TestedType(typeof(RF416Processor))]
	sealed class RF416ProcessorTest : EntryHeaderMessageProcessorTest<RF416Processor, AISInboundEDIMessage, AISOutboundEDIMessage, RF416Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RF416;

		protected override ZString MessageText
		{
			get
			{
				AISInterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
				return Serialize(GenerateMessage());
			}
		}

		protected override ZString MessageFriendlyName => "RF416: Refund Application Rejection Message";

		protected override RF416Processor Processor => new RF416Processor(logger, typeof(Rf416));

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, RF416MessageInterpreterTest.ExpectedInterpretation);

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Refund Application Rejection (RF416) has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		internal static Rf416 GenerateMessage()
		{
			return new Rf416
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "Abcd123aisdu3eh2u89764",
					RejectionDate = "20230802",
					RejectionReason = "Test Reason",
					DecisionTakingCustomsAuthority = "IE123456",
				},
				Parties = new Rf416Parties
				{
					Applicant32 = "APP_3_2_Content",
					RepresentativeIdentification34 = "REPR_ID_3_4_Value",
				},
				FunctionalError = AISInterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
			};
		}
	}
}
