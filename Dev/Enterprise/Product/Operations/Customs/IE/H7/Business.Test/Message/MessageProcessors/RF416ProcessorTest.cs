using System.Collections.ObjectModel;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.COMPLEX;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF416;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business;
using Enterprise.Customs.IE.Business.AIS.UCC6.V1;
using Enterprise.Customs.IE.Messaging;
using NUnit.Framework;
using RF416Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.RF416Provider;

namespace Enterprise.Customs.IE.H7.Business.Test
{
	[TestedType(typeof(RF416Processor))]
	sealed class RF416ProcessorTest : AISH7MessageProcessorTest<RF416Processor, AISInboundEDIMessage, AISOutboundEDIMessage, RF416Provider>
	{
		protected override void AssertProcessResultCore(AsycudaBill messageAttachee, AISInboundEDIMessage incomingMessage)
		{
			base.AssertProcessResultCore(messageAttachee, incomingMessage);
			AssertEquals("Logical status", LogicalStatusList.Codes.Invalid, messageAttachee.ABL_MessageStatus);

			AssertMessageInterpretation(incomingMessage, @"A Refund Application Rejection (RF416) has been received for Job H7D00000001.<br />
<br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Application Reference ID</td><td>Abcd123aisdu3eh2u89764</td></tr>
<tr><td>Rejection Date and Time</td><td>20230802</td></tr>
<tr><td>Rejection Reason</td><td>Test Reason</td></tr>
<tr><td>Decision Taking Customs Authority</td><td>IE123456</td></tr>
<tr><td>Applicant/Holder of the authorization or decision identification</td><td>APP_3_2_Content</td></tr>
<tr><td>Representative Identification</td><td>REPR_ID_3_4_Value</td></tr></table><br />
<br />Functional Error: 1<br /><br /><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>Error Reason</td><td>ERRORREASO</td></tr><tr><td>Error Type</td><td>12</td></tr><tr><td>Error Type Description</td><td>&nbsp;</td></tr><tr><td>Error Message</td><td>Test_Functional_Error</td></tr><tr><td>Original Attribute Value</td><td>ATTR_VALUE</td></tr><tr><td>Error Pointer</td><td>POINTER</td></tr></table>
");
		}

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
				Parties = new CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.RF416.PartiesType
				{
					Applicant = "APP_3_2_Content",
					RepresentativeIdentification = "REPR_ID_3_4_Value",
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

		protected override ZString MessageFriendlyName => "RF416: Refund Application Rejection Message";

		protected override RF416Processor Processor => new RF416Processor(logger, typeof(Rf416));
	}
}
