using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.AIS_complex;
using CargoWise.Customs.IE.MessageDefinitions.AISVersion1_0.IM416;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using IM416Provider = Enterprise.Customs.IE.Messaging.UCC5.IM416Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC5.Testing
{
	[TestedType(typeof(IM416MessageInterpreter))]
	sealed class IM416MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISUCC5InboundEDIMessage, IM416MessageInterpreter, IM416Provider>
	{
		protected override AISUCC5InboundEDIMessage CreateIncomingMessageToTest()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);

			var message = IEXmlObjectSerializer.Serialize(new Im416
			{
				Declaration = new DeclarationType
				{
					DeclarationType11 = "EX",
					AdditionalDeclarationType12 = "A",
					Lrn25 = "LRN001",
					RejectionDate = "20240301",
					RejectionMotivationText = "Rejection Motivation Text",
					CustomsOffices = new DeclarationTypeCustomsOffices
					{
						CustomsOfficeLodgement = "LCO12345"
					},
					Parties = new DeclarationTypeParties
					{
						Declarant = new DeclarantType { },
						Representative = new RepresentativeOptionalType { },
					}
				},
				FunctionalError = AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeObjects(),
			});
			return AISInterchangeProcessorTestHelper.GetAISUCC5MailboxMessage(Factory, "B00000012", message);
		}

		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM416;

		protected override ZString GetExpectedInterpretation(AISUCC5InboundEDIMessage message) => @"A Customs Declaration Rejection (IM416) message has been received for Job B00000012.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Rejection Date</td><td>01-Mar-24</td></tr><tr><td>Rejection Motivation Text</td><td>Rejection Motivation Text</td></tr></table><br />
<br />Functional Error: 1<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>Missing value</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table><br />
<br />Functional Error: 2<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>40</td></tr><tr><td>Error Type Description</td><td>Element too short</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>";

		protected override IM416Provider GetProvider(TextReader reader) => new IM416Provider(new MailBoxItemProvider<Im416>(reader).Message);
	}
}
