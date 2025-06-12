using System;
using System.Globalization;
using System.Threading.Tasks;
using CargoWise.eHub.Adapter;
using NUnit.Framework;

namespace CargoWise.eHub.Gateway.IntegrationTests.InboxMessageHandler
{
	[TestFixture]
	public class BillingMessageHandlerV2Test : BillingMessageHandlerTestBase
	{
		protected override async Task<int> GetCountBillingTransaction()
		{
			return await GetCountBillingTransactionWithRetry("DEF", 2, "XYZ", "ABCDEFXYZ", "98765432100123456789", "ABC", "REFERENCE 1", "REFERENCE 2", "REFERENCE 3", "REFERENCE 4", "REFERENCE 5", 3, "TST", "KLM", null);
		}

		protected override void AssertXmlValidationException(eHubAdapterException e)
		{
			Assert.IsTrue(e.Message.Contains("Billing transaction validation failed:") && e.Message.Contains("The element 'BillingTransaction' in namespace 'http://www.edi.com.au/EnterpriseService/#Billing_1.2' has invalid child element 'Invalid' in namespace 'http://www.edi.com.au/EnterpriseService/#Billing_1.2'"));
		}

		public override string SchemaName => "http://www.edi.com.au/EnterpriseService/#Billing_1.2";
		public override string SuccessBillingMessageContent => $@"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <Branch>KLM</Branch>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Category>TST</Category>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <Reference4>REFERENCE 4</Reference4>
  <Reference5>REFERENCE 5</Reference5>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>{DateTime.UtcNow.AddYears(-4).ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)}</ServiceOccuredUTC>
  <Version>3</Version>
</BillingTransaction>";

		public override string InvalidBillingMessageContent => @"<?xml version=""1.0"" encoding=""utf-8""?>
<BillingTransaction xmlns=""http://www.edi.com.au/EnterpriseService/#Billing_1.2"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <BillableCount>2</BillableCount>
  <Branch>KLM</Branch>
  <ClientID>ABCDEFXYZ</ClientID>
  <ClientNumber>98765432100123456789</ClientNumber>
  <ClientStaffCode>ABC</ClientStaffCode>
  <PriceItemCode>DEF</PriceItemCode>
  <Category>TST</Category>
  <Reference1>REFERENCE 1</Reference1>
  <Reference2>REFERENCE 2</Reference2>
  <Reference3>REFERENCE 3</Reference3>
  <Reference4>REFERENCE 4</Reference4>
  <Reference5>REFERENCE 5</Reference5>
  <ReportingSource>XYZ</ReportingSource>
  <ServiceOccuredUTC>2014-10-02T08:50:30</ServiceOccuredUTC>
  <Version>3</Version>
  <Invalid>Something</Invalid>
</BillingTransaction>";
	}
}
