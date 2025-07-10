using System.IO;
using CargoWise.Customs.IE.MessageDefinitions;
using CargoWise.Customs.IE.MessageDefinitions.AIS_H7_Version1_0.IM460;
using CargoWise.Types;
using Enterprise.Customs.IE.Business.Testing;
using Enterprise.Customs.IE.Messaging;
using Enterprise.Customs.IE.Messaging.Testing;
using NUnit.Framework;
using IM460Provider = Enterprise.Customs.IE.Messaging.UCC6.V1.IM460Provider;

namespace Enterprise.Customs.IE.Business.AIS.UCC6.V1.Testing
{
	[TestedType(typeof(IM460MessageInterpreter))]
	class IM460MessageInterpreterTest : InboundMessageInterpreterAbstractTest<AISInboundEDIMessage, IM460MessageInterpreter, IIM460Provider>
	{
		protected override ZString MessageType => AISInterchangeTypeList.Codes.IM460;

		protected override AISInboundEDIMessage CreateIncomingMessageToTest()
		{
			return AISInterchangeProcessorTestHelper.GetAISMailboxMessage(Factory, "B00000012", IEXmlObjectSerializer.Serialize(new Im460
			{
				Declaration = new DeclarationType
				{
					Mrn = "12MRN345CDEFG678R9",
					ControlNotificationDate = "20240220",
					TimeLimitForControl = "202403071437",
				},
				OverallControlType = new OverAllControlsType
				{
					ControlTypeCoded = "Orange",
				}
			}));
		}

		protected override ZString GetExpectedInterpretation(AISInboundEDIMessage message) => @"A Control Notice (IM460) message has been received for Job B00000012.<br/>
<br/><table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table""><tr><td>MRN</td><td>12MRN345CDEFG678R9</td></tr><tr><td>Notification Date</td><td>20-Feb-24</td></tr><tr><td>Time Limit For Control</td><td>07-Mar-24 14:37</td></tr><tr><td>Overall Control Type Code</td><td>Orange</td></tr><tr><td>Overall Control Type Description</td><td>Documentary Control</td></tr></table>";

		protected override IIM460Provider GetProvider(TextReader reader) => new IM460Provider(new MailBoxItemProvider<Im460>(reader).Message);
	}
}
