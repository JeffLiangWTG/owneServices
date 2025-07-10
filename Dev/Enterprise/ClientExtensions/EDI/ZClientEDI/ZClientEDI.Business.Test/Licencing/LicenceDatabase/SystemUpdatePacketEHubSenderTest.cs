using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class SystemUpdatePacketEHubSenderTest : TestCaseWithFactory
	{
		public void TestSend()
		{
			var licHeader = Enterprise.Client.EDI.Billing.Business.Test.BillingTestHelper.CreateLicence(Factory, "AAA");
			SystemUpdatePacket packet = new SystemUpdatePacket();
			var sender = new SystemUpdatePacketEHubSenderForTest();
			sender.Send(licHeader.Database, packet);

			AssertEquals("CreateCalls", 1, DebugOnlyOutgoingSystemMessage.CreateCalls);
			AssertEquals(licHeader.LicenceCode, DebugOnlyOutgoingSystemMessage.RecipientId);
			AssertEquals("XmlMessageBody", packet.ToXMLString(), DebugOnlyOutgoingSystemMessage.XmlMessageBody);

			DebugOnlyOutgoingSystemMessage.Initialize();
		}
	}

	public class SystemUpdatePacketEHubSenderForTest : SystemUpdatePacketEHubSender
	{
		public override IOutgoingSystemMessage GetSender()
		{
			return new DebugOnlyOutgoingSystemMessage();
		}
	}
}