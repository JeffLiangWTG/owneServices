using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM405;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using IM405Provider = Enterprise.Customs.IE.Messaging.UCC5.IM405Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM405MessageInterpreter))]
	class IM405MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM405MessageInterpreter, IM405Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM405;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);

			var data = new Im405
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
			};

			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(Factory, "B00001000", IEXmlObjectSerializer.Serialize(data));
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => ExpectedInterpretation;

		internal const string ExpectedInterpretation = $@"An Amendment Request Rejection (IM405) message has been received from customs for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr>
				<tr><td>Amendment Rejection Date</td><td>29-Feb-24</td></tr>
				<tr><td>Amendment Rejection Reason</td><td>Amendment Rejection Reason</td></tr>
				<tr><td>Remarks</td><td>Remarks001</td></tr>
			</table><br />
			<br />
			{AISUCC5InterchangeProcessorTestHelper.ExpectedFunctionalErrorInterpretation}";

		protected override IM405Provider GetProvider(TextReader reader) => new IM405Provider(new MailBoxItemProvider<Im405>(reader).Message);
	}
}
