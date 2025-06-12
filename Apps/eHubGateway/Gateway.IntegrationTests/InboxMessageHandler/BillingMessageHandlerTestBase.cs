using CargoWise.eHub.Adapter;
using CargoWise.eHub.Integration;
using NUnit.Framework;
using System.IO;
using System.Text;
using System;
using System.Threading.Tasks;
using CargoWise.eHub.Common;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	public abstract class BillingMessageHandlerTestBase : GatewayIntegrationTestBase
	{
		[Test]
		public async Task TestDirectSuccess_TSTClient()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(SuccessBillingMessageContent)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, SchemaName, messageStream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				Assert.That(await GetCountBillingTransaction(), Is.EqualTo(1), GetBillingStagingTransactionsInfo());
			}
		}


		[Test]
		public  async Task TestDirectSuccess_NonCW1_Client()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(SuccessBillingMessageContent)))
			{
				var message = new eHubMessage(Guid.NewGuid(), NonCW1_Client, TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, SchemaName, messageStream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				Assert.That(await GetCountBillingTransaction(), Is.EqualTo(1), GetBillingStagingTransactionsInfo());
			}
		}

		[Test]
		[Property("SendBillingToKafka", "true")]
		public  async Task TestKafkaSuccess_TSTClient()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(SuccessBillingMessageContent)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, SchemaName, messageStream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				WithGatewayServiceAttribute.BillingServiceController.StartConsumers("billing-topic");
				Assert.That(await GetCountBillingTransaction(), Is.EqualTo(1), GetBillingStagingTransactionsInfo());
			}
		}

		[Test]
		[Property("SendBillingToKafka", "true")]
		public  async Task TestKafkaSuccess_NonCW1_Client()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(SuccessBillingMessageContent)))
			{
				WithGatewayServiceAttribute.BillingServiceController.StartConsumers("billing-topic");
				var message = new eHubMessage(Guid.NewGuid(), NonCW1_Client, TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, SchemaName, messageStream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				Assert.That(await GetCountBillingTransaction(), Is.EqualTo(1), GetBillingStagingTransactionsInfo());
			}
		}

		[Test]
		public void TestDirectInvalidBillingXMLThrowsBillingTransactionValidationException()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(InvalidBillingMessageContent)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, SchemaName, messageStream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
				}
				catch (eHubAdapterException e)
				{
					Assert.IsTrue(e.Message.StartsWith("1 errors occured during processing send request:"));
					AssertXmlValidationException(e);
				}
			}
		}

		[Test]
		[Property("SendBillingToKafka", "true")]
		public void TestKafkaInvalidBillingXMLThrowsBillingTransactionValidationException()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(InvalidBillingMessageContent)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, SchemaName, messageStream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
				}
				catch (eHubAdapterException e)
				{
					Assert.IsTrue(e.Message.StartsWith("1 errors occured during processing send request:"));
					AssertXmlValidationException(e);
				}
			}
		}

		public override void TearDownCore()
		{
			base.TearDownCore();
			DeleteBillingStaging();
		}

		const string TestBillingClientID = "BILCLIENT";
		public abstract string SchemaName { get; }
		public abstract string SuccessBillingMessageContent { get; }
		public abstract string InvalidBillingMessageContent { get; }
		protected virtual void AssertXmlValidationException(eHubAdapterException e) => Assert.Fail("AssertXmlValidationException has not been implemented");
		protected abstract Task<int> GetCountBillingTransaction();
	}
}
