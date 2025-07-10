using System;
using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.RF416;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;
using RF416Provider = Enterprise.Customs.IE.Messaging.UCC5.RF416Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(RF416Processor))]
	sealed class RF416ProcessorTest : EntryHeaderMessageProcessorTest<RF416Processor, AISUCC5InboundEDIMessage, AISUCC5OutboundEDIMessage, RF416Provider>
	{
		public void TestNeedToSendEmailNotification()
		{
			(var relatedJob, var messageAttachee, var outgoingMessage, var incomingMessage) = CreateSetupData();
			var processor = new RF416ProcessorForTest(logger, typeof(Rf416));
			AssertEquals("NeedToSendEmailNotification", true, processor.NeedToSendEmailNotification_Exposed(incomingMessage));
		}

		public void TestMessageInterpreterType()
		{
			var processor = new RF416ProcessorForTest(logger, typeof(Rf416));
			AssertEquals("Interpreter Type", typeof(RF416MessageInterpreter), processor.MessageInterpreterType_Exposed);
		}

		protected override void AssertProcessResultCore(CusEntryHeader messageAttachee, AISUCC5InboundEDIMessage incomingMessage)
		{
			AssertEquals("CH_Status", LogicalStatusList.Codes.Invalid, messageAttachee.CH_Status);

			AssertMessageInterpretation(incomingMessage, @"A Refund Application Rejection (RF416) has been received for Job B00001000.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Application Reference ID</td><td>Abcd123aisdu3eh2u89764</td></tr><tr><td>Rejection Date and Time</td><td>20230802</td></tr><tr><td>Rejection Reason</td><td>Test Reason</td></tr><tr><td>Decision Taking Customs Authority</td><td>IE123456</td></tr></table><br />
<br />Functional Error: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ERRORREASO</td></tr><tr><td>Error Type</td><td>12</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Test_Functional_Error</td></tr><tr><td>Original Attribute Value</td><td>ATTR_VALUE</td></tr><tr><td>Error Pointer</td><td>POINTER</td></tr></table>");

			MessageProcessorNotificationTestHelper.AssertEmail(incomingMessage.MessageTypeWithDescription + " Response for B00001000",
				new[] { "A Refund Application Rejection (RF416) has been received for Job B00001000." },
				new string[] { "staff1@where.com" });
		}

		protected override ZString MessageFriendlyName => "RF416: Refund Application Rejection Message";

		protected override RF416Processor Processor => new RF416Processor(logger, typeof(Rf416));

		protected override ZString MessageType => AISInterchangeTypeList.Codes.RF416;

		protected override ZString MessageText => Serialize(GenerateMessage());

		Rf416 GenerateMessage()
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
				FunctionalError = new Collection<FunctionalErrorType>
				{
					new FunctionalErrorType
					{
						ErrorMessage = "Test_Functional_Error",
						ErrorType = "12",
						ErrorPointer = "POINTER",
						ErrorReason = "ERRORREASO",
						OriginalAttributeValue = "ATTR_VALUE",
					}
				}
			};
		}
	}

	sealed class RF416ProcessorForTest : RF416Processor
	{
		public RF416ProcessorForTest(LoggingInformation logger, Type xmlObjectType) : base(logger, xmlObjectType)
		{
		}

		public bool NeedToSendEmailNotification_Exposed(EDIMessage message) => NeedToSendEmailNotification(message);

		public Type MessageInterpreterType_Exposed => MessageInterpreterType;
	}
}
