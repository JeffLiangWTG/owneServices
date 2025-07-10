using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM409;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using IM409Provider = Enterprise.Customs.IE.Messaging.UCC5.IM409Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM409MessageInterpreter))]
	class IM409MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM409MessageInterpreter, IM409Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM409;

		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);

			var data = new Im409
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

			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage<AISUCC5InboundEDIMessage>(Factory, "B00001000", IEXmlObjectSerializer.Serialize(data));
		}

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => ExpectedInterpretation;

		internal const string ExpectedInterpretation = $@"An Invalidation Request Decision (IM409) message has been received from customs for Job B00001000.<br />
			<br />
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr>
				<tr><td>Invalidation Decision</td><td>Y</td></tr>
				<tr><td>Invalidation Initiated by Customs</td><td>N</td></tr>
				<tr><td>Invalidation Justification</td><td>Invalidation Justification</td></tr>
				<tr><td>Date of Invalidation Decision</td><td>08-Feb-24</td></tr>
				<tr><td>Date of Invalidation Request</td><td>09-Mar-24</td></tr>
				<tr><td>Date of Invalidation</td><td>07-Jan-24</td></tr>
			</table><br />
			<br />
			{AISUCC5InterchangeProcessorTestHelper.ExpectedFunctionalErrorInterpretation}";

		protected override IM409Provider GetProvider(TextReader reader) => new IM409Provider(new MailBoxItemProvider<Im409>(reader).Message);
	}
}
