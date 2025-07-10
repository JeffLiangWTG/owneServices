using System.Collections.ObjectModel;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF416;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.Customs.IE.Messaging.UCC5.Testing;
using NUnit.Framework;
using PartiesType = CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF416.PartiesType;
using RF416Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.RF416Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(RF416MessageInterpreter))]
	class RF416MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, RF416MessageInterpreter, RF416Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.RF416;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			AISUCC5InterchangeProcessorTestHelper.CreateFunctionalErrorTypeReferenceTestData(Factory);
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(GenerateMessage()));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message)
		{
			return @"A Refund Application Rejection (RF416) has been received for Job B00000012.<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Application Reference ID</td><td>APPREF_1234</td></tr><tr><td>Rejection Date and Time</td><td>2023-08-02</td></tr><tr><td>Rejection Reason</td><td>Test Reason</td></tr><tr><td>Decision Taking Customs Authority</td><td>Autority 123</td></tr><tr><td>Applicant/Holder of the authorization or decision identification</td><td>APP_3_2_Content</td></tr><tr><td>Representative Identification</td><td>REPR_ID_3_4_Value</td></tr></table><br/>
<br/>Functional Error: 1<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>Missing value</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table><br/>
<br/>Functional Error: 2<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>40</td></tr><tr><td>Error Type Description</td><td>Element too short</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>";
		}

		protected override RF416Provider GetProvider(TextReader reader) => new RF416Provider(new MailBoxItemProvider<Rf416>(reader).Message);

		Rf416 GenerateMessage()
		{
			return new Rf416
			{
				Header = new HeaderType
				{
					ApplicationReferenceId = "APPREF_1234",
					RejectionDate = "2023-08-02",
					RejectionReason = "Test Reason",
					DecisionTakingCustomsAuthority = "Autority 123",
				},
				Parties = new PartiesType
				{
					Applicant = "APP_3_2_Content",
					RepresentativeIdentification = "REPR_ID_3_4_Value",
				},
				FunctionalError = new Collection<FunctionalErrorType>
				{
					new FunctionalErrorType
					{
						ErrorPointer = "ErrorPointer001",
						ErrorType = "13",
						ErrorReason = "ER1",
						ErrorMessage = "Functional Error Message 1",
						OriginalAttributeValue = "Original Attribute Value 1",
					},
					new FunctionalErrorType
					{
						ErrorPointer = "ErrorPointer002",
						ErrorType = "40",
						ErrorReason = "ER2",
						ErrorMessage = "Functional Error Message 2",
						OriginalAttributeValue = "Original Attribute Value 2",
					},
				}
			};
		}
	}
}
