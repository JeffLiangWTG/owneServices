using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting.Testing
{
	class AgentChargePostingDetails_InnerTest : TestCaseWithFactory
	{
		public void TestAddProfitShareAPInvoice()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			RefCurrency invoiceCurrency = creator.USD;
			ZDecimal invoiceCurrencyExRate = 0.712341m;
			ProfitShareDetailCollection profitShares = new ProfitShareDetailCollection();
			AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, profitShares, invoiceCurrency, invoiceCurrencyExRate, new ChargePoster(Factory), null);

			APInvoice profitShareAPInvoice = Factory.New<APInvoice>();
			postingDetails.AgentInvoices.Add(profitShareAPInvoice);
			AssertEquals("Should be one AP Invoice in PS Invoices", 1, postingDetails.AgentInvoices.Count);
			AssertEquals("Should be invoice added to ps invoices", profitShareAPInvoice, postingDetails.AgentInvoices[0]);
		}

		public void TestGetReceivingAgentProfitShareApportionment()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			Factory.Save();
			TestObjectCreator creator = new TestObjectCreator(Factory);
			consol.Shipments.AddNew();
			consol.SetDefaultReceivingForwarderAddress(creator.Agent);

			JobConsolCostCollection costs = new JobConsolCostCollection(Factory, consol);
			JobConsolCost cost = costs.TryAddNew();

			cost.E6_OH_Creditor = creator.Agent.PK;

			ApportionmentListing apps = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost freightCost = apps.CostsCollection.TryAddNew();
				freightCost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
				freightCost.E6_RX_NKCurrency = creator.AUD.RX_Code;
				freightCost.E6_ExchangeRate = 1m;
				freightCost.E6_OSCostAmount = 2329.31m;
				freightCost.E6_OH_Creditor = creator.Agent.PK;

				RefCurrency invoiceCurrency = creator.USD;
				ZDecimal invoiceCurrencyExRate = 0.712341m;
				ProfitShareDetailCollection profitShares = new ProfitShareDetailCollection();
				AgentChargePostingDetails postingDetails = new AgentChargePostingDetails(consol, profitShares, invoiceCurrency, invoiceCurrencyExRate, new ChargePoster(Factory), null);

				postingDetails.AddProfitShareApportionment(cost);
				AssertEquals("Should get the correct App from list", cost, postingDetails.GetRecevingAgentProfitShareApportionment());
			}
			finally
			{
				apps.ReleaseMutexes();
			}
		}
	}
}
