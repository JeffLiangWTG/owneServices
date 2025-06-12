using System;
using System.Globalization;
using System.IO;
using System.Text;
using CargoWise.eHub.Adapter;
using CargoWise.eHub.Common;
using CargoWise.eHub.Integration;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class BillingMessageWithBlacklistingHandlerTest : GatewayIntegrationTestBase
	{
		[Test]
		public void TestSuccess()
		{
			var adapter = CreateAdapter(TestClientID, TestClientPassword);
			var xml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <Reference4>REFERENCE 4</Reference4>
  <Reference5>REFERENCE 5</Reference5>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>{DateTime.UtcNow.AddYears(-4).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}</ServiceOccuredUTC>
</BillingTransaction>";
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				var message = new eHubMessage(Guid.NewGuid(), TestClientID, TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, "http://www.edi.com.au/EnterpriseService/#Billing_1.0", messageStream);
				adapter.Outbox.AddMessage(message);
				adapter.SendMessages();
				var count = GetCountBillingTransaction("DEF", 2, "XYZ", "ABCDEFXYZ", "98765432100123456789", "ABC", "REFERENCE 1", "REFERENCE 2", "REFERENCE 3", "REFERENCE 4", "REFERENCE 5", 0, "UNK", null, null);
				Assert.That(count, Is.EqualTo(1), "Should have 1 billing transaction");
			}
		}

		[Test]
		public void TestInvalidBillingXMLThrowsBillingTransactionValidationException()
		{
			var adapter = CreateAdapter("TSTCLIEN2", "TSTPASSWORD");
			var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>";
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(xml)))
			{
				var message = new eHubMessage(Guid.NewGuid(), "TSTCLIEN2", TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, "http://www.edi.com.au/EnterpriseService/#Billing_1.0", messageStream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
				}
				catch (eHubAdapterException e)
				{
					Assert.IsTrue(e.Message.StartsWith("1 errors occured during processing send request:"));
					Assert.IsTrue(e.Message.Contains("Billing transaction validation failed:") && e.Message.Contains("Unexpected end of file has occurred. The following elements are not closed: BillingTransaction. Line 12, position 61"));
				}
			}
		}

		[Test]
		public void TestShouldNotSendBillingInfoWhenCachedInInvalidClientDictionary()
		{
			var adapter = CreateAdapter("TSTCLIEN3", "TSTPASSWORD");
			var adapter2 = CreateAdapter("TSTCLIEN3", "TSTPASSWORD");
			var adapter3 = CreateAdapter("TSTCLIEN3", "TSTPASSWORD");
			var invalidXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>";
			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(invalidXml)))
			{
				var message = new eHubMessage(Guid.NewGuid(), "TSTCLIEN3", TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, "http://www.edi.com.au/EnterpriseService/#Billing_1.0", messageStream);
				adapter.Outbox.AddMessage(message);
				try
				{
					adapter.SendMessages();
				}
				catch (eHubAdapterException e)
				{
					Assert.IsTrue(e.Message.StartsWith("1 errors occured during processing send request:"));
					Assert.IsTrue(e.Message.Contains("Billing transaction validation failed:") && e.Message.Contains("Unexpected end of file has occurred. The following elements are not closed: BillingTransaction. Line 12, position 61"));
				}
			}

			var validXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(validXml)))
			{
				var message = new eHubMessage(Guid.NewGuid(), "TSTCLIEN3", TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, "http://www.edi.com.au/EnterpriseService/#Billing_1.0", messageStream);
				adapter2.Outbox.AddMessage(message);
				try
				{
					adapter2.SendMessages();
				}
				catch (eHubAdapterException e)
				{
					Assert.IsTrue(e.Message.StartsWith("1 errors occured during processing send request:"));
					Assert.IsTrue(e.Message.Contains("Billing transaction validation failed:") && e.Message.Contains("Unexpected end of file has occurred. The following elements are not closed: BillingTransaction. Line 12, position 61"));
				}
			}

			var validXml2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>01234567899876543210</ClientNumber>
  <ClientStaffCode>BBB</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Reference1>TTT 1</Reference1>
  <Reference2>SSS 2</Reference2>
  <Reference3>EEE 3</Reference3>
  <ReportingSource>AAA</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
</BillingTransaction>";

			using (var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(validXml2)))
			{
				var message = new eHubMessage(Guid.NewGuid(), "TSTCLIEN3", TestBillingClientID, MessageSchemaType.Xml, ApplicationCode.AgentScavenging, "http://www.edi.com.au/EnterpriseService/#Billing_1.0", messageStream);
				adapter3.Outbox.AddMessage(message);
				try
				{
					adapter3.SendMessages();
				}
				catch (eHubAdapterException e)
				{
					Assert.IsTrue(e.Message.StartsWith("1 errors occured during processing send request:"));
					Assert.IsTrue(e.Message.Contains("Billing transaction validation failed:") && e.Message.Contains("Unexpected end of file has occurred. The following elements are not closed: BillingTransaction. Line 12, position 61"));
				}
			}
		}

		static readonly Guid TestClient2PK = Guid.NewGuid();
		const string TestBillingClientPassword = "BILClient";
		const string TestBillingClientID = "BILCLIENT";
		static readonly Guid TestAUCustomsPK = Guid.NewGuid();

	}
}
