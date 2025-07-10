using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM405;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC6.V1.Testing;
using NUnit.Framework;
using IM405 = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM405.Im405;
using IM405Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM405Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM405MessageInterpreter))]
	class IM405MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM405MessageInterpreter, IIM405Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM405;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);

			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00001000", IEXmlObjectSerializer.Serialize(new IM405
			{
				Declaration = new DeclarationType
				{
					AmendmentRejectionDate = "20240229",
					AmendmentRejectionMotivationText = "Amendment Rejection Reason",
					CustomsOffices = new CustomsOffices02Type
					{
						CustomsOfficeLodgement = "IEDUB400",
					},
					Mrn = "12MRN345CDEFG678R9",
					Remarks = "Remarks001",
					Parties = { },
				},
				FunctionalError = AISUCC6V1ProviderTestHelper.CreateFunctionalErrorTypeObjects(),
			}));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => $@"An Amendment Request Rejection (IM405) message has been received from customs for Job B00001000.<br/>
			<br/>
			<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">
				<tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr>
				<tr><td>Amendment Rejection Date</td><td>29-Feb-24</td></tr>
				<tr><td>Amendment Rejection Motivation Text</td><td>Amendment Rejection Reason</td></tr>
				<tr><td>Remarks</td><td>Remarks001</td></tr>
			</table><br/>
			<br/>
			{AISUCC6V1ProviderTestHelper.ExpectedFunctionalErrorInterpretation}";

		protected override IIM405Provider GetProvider(TextReader reader) => new IM405Provider(new MailBoxItemProvider<IM405>(reader).Message);
	}
}
