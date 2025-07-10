using System.Linq;
using System.Reflection;
using Enterprise.Messaging.Business;
using Enterprise.xTMessaging.Shared.Test;

namespace Enterprise.xTMessaging.Business.Test
{
	public class UniversalXmlMessageCreatorTest : XMLMessageCreatorTest<UniversalXmlMessageCreator>
	{
		public override void TestGenerateEDIMessages()
		{
			var sourceStream = TestUtils.GetEmbeddedResource(Assembly.GetExecutingAssembly(), "TestFiles.UniversalInterchange_Input.xml", EmbeddedResourcePath);
			AssertEquals("Precondition: No EDIMessage attached to Interchange", 0, Interchange.ContainedMessages.Count);
			EDIMessageCreator.CreateEDIMessagesForInterchange(sourceStream, Factory);

			AssertEquals("3 EDIMessages are created", 3, Interchange.ContainedMessages.Count);

			AssertEquals("Message Type should be XDC.", 3, Interchange.ContainedMessages.Where(msg => msg.EM_MessageType == "XDC").Count());
			AssertEquals("1 x Native Product XML Message is created", 1, Interchange.ContainedMessages.Where(msg => msg.EM_ApplicationCode == "NDM" && msg.EM_MessageSubType == "XNP").Count());
			AssertEquals("1 x UniversalShipment xml", 1, Interchange.ContainedMessages.Where(msg => msg.EM_ApplicationCode == "UDM" && msg.EM_MessageSubType == "XUS").Count());
			AssertEquals("1 x UniversalEvent xml", 1, Interchange.ContainedMessages.Where(msg => msg.EM_ApplicationCode == "UDM" && msg.EM_MessageSubType == "XUE").Count());
			AssertCollectionContains("Logger should contain message for unsupported element <Test>", "Unknown Payload - Message of ApplicationCode 'UDM' contains an unsupported element <Test>. Element will be ignored.", Logger.WarningLogs);
			AssertCollectionContains("Logger should contain message for unsupported element <UniversalDocumentRequest>", "Unknown Payload - Message of ApplicationCode 'UDM' contains an unsupported element <UniversalDocumentRequest>. Element will be ignored.", Logger.WarningLogs);
			AssertCollectionContains("Logger should contain message for unsupported Message subtype", "Unknown Message subType - Message of Application Code 'NDM', Message Type 'Native' contains an unsupported element <XXXX>. Element will be ignored.", Logger.WarningLogs);
		}

		protected override EDIInterchange GetEDIInterchange()
		{
			var interchange = TestUtils.CreateInterchangeForXT(Factory, EDIInterchange.Status.Queued, true, EDIInterchange.Direction.Receive, EDIInterchange.TransportType.xT);
			interchange.EI_ApplicationCode = "UDM";
			return interchange;
		}

		protected override IEDIMessageCreator GetMessageCreator()
		{
			return new UniversalXmlMessageCreator(Interchange, Logger);
		}

		const string EmbeddedResourcePath = "Enterprise.xTMessaging.Business.Test.";
	}
}
