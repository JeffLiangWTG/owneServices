using System;
using System.IO;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Integration;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class InvalidMessageHandlerTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestDoAbsolutelyNothing()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var xml = @"";
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, "LittleFinger", MessageSchemaType.Xml, ApplicationCode.AgentScavenging, "http://www.edi.com.au/Stormborn", messageStream);
				adapter.Outbox.AddMessage(message);
				AssertNoExceptionThrown(() => adapter.SendMessages());
			}
		}
	}
}
