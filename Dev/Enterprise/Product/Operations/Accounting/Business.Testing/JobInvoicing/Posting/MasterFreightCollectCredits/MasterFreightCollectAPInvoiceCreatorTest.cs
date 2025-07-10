using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class MasterFreightCollectAPInvoiceCreatorTest : APInvoiceCreatorTest
	{
		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestChargeIsNotApplicableForManuallyEnteredProfitShareCharges()
		{
			TestObjectCreator testPoster = new TestObjectCreator(new BusinessObjectFactory());
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			Job shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();
			shipment1Job.Dispose();

			Charge job1Charge = shipment1Job.Charges.AddNew();
			job1Charge.JR_AC = Env.Registry.FreightChargeCode;
			job1Charge.JR_OSCostAmt = 200m;
			job1Charge.JR_OH_CostAccount = testPoster.AALSHI.PK;
			job1Charge.JR_APInvoiceNum = "MF 1000";
			job1Charge.JR_APInvoiceDate = ZDateTime.Now;
			job1Charge.JR_PaymentDate = ZDateTime.Now;

			AgentChargePostingDetails details = new AgentChargePostingDetails(consol, null, GlbCompany.CurrentCompany.LocalCurrency, 1m, new ChargePoster(Factory), null);
			MasterFreightCollectAPInvoiceCreator creator = new MasterFreightCollectAPInvoiceCreator(details, shipment1Job, consol, false, null);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			creator.CreateTransactions(transactions);

			AssertEquals("Should not have created an invoice", 0, transactions.APTransactionsCount);
			AssertEquals("Transactions Count", 0, transactions.Count);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAPInvoice()
		{
			TestObjectCreator testPoster = new TestObjectCreator(new BusinessObjectFactory());
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultReceivingForwarderAddress(testPoster.Agent);
			consol.JK_RL_NKDischargePort = "USLAX";

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			Job shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			Job shipment2Job = Job.CreateWithMutex(Factory, shipment2);
			shipment2Job.PlugInData = shipment2;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			shipment1Job.Dispose();
			shipment2Job.Dispose();

			AgentChargePostingDetails details = new AgentChargePostingDetails(consol, null, GlbCompany.CurrentCompany.LocalCurrency, 1m, new ChargePoster(Factory), null);
			JobConsolCostCollection costs = new JobConsolCostCollection(Factory, consol);
			JobConsolCost cost = costs.TryAddNew();

			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OH_Creditor = testPoster.Agent.PK;
			cost.E6_RX_NKCurrency = testPoster.USD.RX_Code;
			cost.E6_ExchangeRate = 0.72m;
			cost.E6_OSCostAmount = 500m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_InvoiceNum = "MSF 1000";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;

			Factory.Save();

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			var creator = new MasterFreightConsolAPInvoiceCreator(details, Factory, new[] { shipment1Job, shipment2Job }, false, consol, costs);
			creator.CreateTransactions(transactions);

			Assert("Should have setup a master freight invoice", details.AgentInvoices.Count > 0);
			AssertEquals("Invoice amount should be 500", 500m, details.AgentInvoices[0].AH_OSExTaxAmount);
			AssertEquals("Should be 2 lines on invoice", 2, details.AgentInvoices[0].Lines.Count);
			AssertEquals("Invoice number", "MSF 1000", details.AgentInvoices[0].AH_TransactionNum);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreateAPInvoiceExcludePostponedConsolCost()
		{
			TestObjectCreator testPoster = new TestObjectCreator(new BusinessObjectFactory());
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultReceivingForwarderAddress(testPoster.Agent);
			consol.JK_RL_NKDischargePort = "USLAX";

			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			Job shipment1Job = Job.CreateWithMutex(Factory, shipment1);
			shipment1Job.PlugInData = shipment1;
			Job shipment2Job = Job.CreateWithMutex(Factory, shipment2);
			shipment2Job.PlugInData = shipment2;
			shipment1Job.JH_GE = GlbDepartment.CurrentDepartment.PK;
			shipment2Job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			shipment1Job.Dispose();
			shipment2Job.Dispose();

			AgentChargePostingDetails details = new AgentChargePostingDetails(consol, null, GlbCompany.CurrentCompany.LocalCurrency, 1m, new ChargePoster(Factory), null);
			JobConsolCostCollection costs = new JobConsolCostCollection(Factory, consol);
			JobConsolCost cost = costs.TryAddNew();

			cost.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost.E6_OH_Creditor = testPoster.Agent.PK;
			cost.E6_RX_NKCurrency = testPoster.USD.RX_Code;
			cost.E6_ExchangeRate = 0.72m;
			cost.E6_OSCostAmount = 500m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_InvoiceNum = "MSF 1000";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;

			Factory.Save();

			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			List<ZGuid> postponedConsolCosts = new List<ZGuid>();
			postponedConsolCosts.Add(cost.PK);
			var creator = new MasterFreightConsolAPInvoiceCreator(details, Factory, new[] { shipment1Job, shipment2Job }, false, consol, costs, postponedConsolCosts);
			creator.CreateTransactions(transactions);

			Assert("Should not post a master freight invoice due to consol cost is postponed", details.AgentInvoices.Count == 0);
		}

		[TestDate(2004, 07, 15, 12, 00, 00)]
		public void TestCreatePSInvoices()
		{
			TestObjectCreator testPoster = new TestObjectCreator(new BusinessObjectFactory());
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.SetDefaultReceivingForwarderAddress(testPoster.Agent);
			consol.JK_RL_NKDischargePort = "USLAX";

			ForwardingShipment shipment = consol.Shipments.AddNew();

			Job shipmentJob = Job.CreateWithMutex(Factory, shipment);
			shipmentJob.PlugInData = shipment;
			shipmentJob.JH_GE = GlbDepartment.CurrentDepartment.PK;

			AccChargeCode pSChargeCode = Factory.Load<AccChargeCode>((ZGuid)AccountingConfigurationRegistry.Instance.ProfitShareChargeCode.Value);
			Charge pSCharge = CreateCharge(shipmentJob, pSChargeCode, "Charge Code", AUD, 100M, testPoster.Agent, AUD, 150M, LocalClient);

			Factory.Save();

			shipmentJob.Dispose();

			AgentChargePostingDetails details = new AgentChargePostingDetails(consol, null, GlbCompany.CurrentCompany.LocalCurrency, 1m, new ChargePoster(Factory), null);
			JobConsolCostCollection costs = new JobConsolCostCollection(Factory, consol);
			JobConsolCost cost = costs.TryAddNew();

			cost.E6_AC_ChargeCode = pSChargeCode.PK;
			cost.E6_OH_Creditor = testPoster.Agent.PK;
			cost.E6_RX_NKCurrency = testPoster.USD.RX_Code;
			cost.E6_ExchangeRate = 0.72m;
			cost.E6_OSCostAmount = 500m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.E6_InvoiceNum = "1";
			cost.E6_InvoiceDate = ZDateTime.Now;
			cost.E6_PaymentDate = ZDateTime.Now;

			SetAPInvoiceInfo(pSCharge, "1", ZDateTime.Today.AddDays(10), ZDateTime.Today.AddDays(20));

			Factory.Save();

			MasterFreightCollectAPInvoiceCreator creator = new MasterFreightCollectAPInvoiceCreator(details, shipmentJob, consol, false, costs);
			TransactionCreatorHashtable transactions = new TransactionCreatorHashtable();
			SetUpRegistryForTest();
			try
			{
				Env.Security.APUnapprovedInvoicesFirstApproval.IsAllowed = false;
				creator.CreateTransactions(transactions);
			}
			finally
			{
				ResetRegistryForTest();
			}

			AssertEquals("Invoice Count", 1, transactions.APTransactionsCount);
			AssertEquals("AR Transactions Count", 0, transactions.ARTransactionsCount);
			APInvoice creditorInv1 = transactions.RetrieveAPInvoice(testPoster.Agent, "1");
			AssertEquals("Invoice Line Count", 1, creditorInv1.Lines.Count);
			Assert("Shouldn't be UAInvoice", creditorInv1 is APInvoice);

			TransactionLine cCLine = creditorInv1.FindTransactionLine(ZArchitecture.Core.TransactionLineTypes.Cost, pSChargeCode, shipmentJob.PK);
			Assert("Shouldn't be UAInvoiceLine", cCLine is APInvoiceLine);
		}
	}
}
