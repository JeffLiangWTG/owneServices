using System.Collections.ObjectModel;
using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM416;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM416Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM416Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM416MessageInterpreter))]
	class IM416MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM416MessageInterpreter, IIM416Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM416;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(new Im416
			{
				Declaration = new DeclarationType
				{
					Additionaldeclarationtype = "A",
					Lrn = "LRN001",
					RejectionDate = "20230810",
					RejectionMotivationText = "UCC6 Rejection Motivation Text",
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
						ErrorType = "52",
						ErrorReason = "ER2",
						ErrorMessage = "Functional Error Message 2",
						OriginalAttributeValue = "Original Attribute Value 2",
					},
				}
			}));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Customs Declaration Rejection (IM416) message has been received for Job B00000012.<br/><br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Additional Declaration Type</td><td>A</td></tr><tr><td>LRN</td><td>LRN001</td></tr><tr><td>Rejection Date</td><td>10-Aug-23</td></tr><tr><td>Rejection Motivation Text</td><td>UCC6 Rejection Motivation Text</td></tr></table>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>&nbsp;</td></tr><tr><td>Error Reason</td><td>ER1</td></tr><tr><td>Error Type</td><td>13</td></tr><tr><td>Error Type Description</td><td>13</td></tr><tr><td>Error Message</td><td>Functional Error Message 1</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 1</td></tr><tr><td>Error Pointer</td><td>ErrorPointer001</td></tr></table>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Functional Error</td><td>&nbsp;</td></tr><tr><td>Error Reason</td><td>ER2</td></tr><tr><td>Error Type</td><td>52</td></tr><tr><td>Error Type Description</td><td>52</td></tr><tr><td>Error Message</td><td>Functional Error Message 2</td></tr><tr><td>Original Attribute Value</td><td>Original Attribute Value 2</td></tr><tr><td>Error Pointer</td><td>ErrorPointer002</td></tr></table>";

		protected override IIM416Provider GetProvider(TextReader reader) => new IM416Provider(new MailBoxItemProvider<Im416>(reader).Message);
	}
}
