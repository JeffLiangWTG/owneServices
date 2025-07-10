using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	public class ReverseInvoicingTransformerTest : BaseIntegrationTransformerTest
	{
		#region Test Revenue

		public void TestClearRevenueLinks()
		{
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job1, USD, .7M);
			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, null, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, null, AUD, 250M, LocalClient);

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job2, USD, .7M);
			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 100M, null, AUD, 150M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", AUD, 300M, null, AUD, 350M, LocalClient);
			Factory.Save();

			ARInvoice invoice = CreateARInvoice("Z1000", Creditor1, USD, .7M, "Invoice Z1000");
			ARInvoiceLine line1 = CreateARInvoiceLine(invoice, job1, CC2, USD, .7M, "Charge Code 2", 200M);
			ARInvoiceLine line2 = CreateARInvoiceLine(invoice, job2, CC1, USD, .7M, "Charge Code 1", 105M);

			charge1_2.ReverseWIP(ZDateTime.Now);
			charge1_2.JR_AL_ARLine = line1.PK;
			charge2_1.ReverseWIP(ZDateTime.Now);
			charge2_1.JR_AL_ARLine = line2.PK;
			charge1_2.SetAmountsToLinkedLinesForTests();
			charge2_1.SetAmountsToLinkedLinesForTests();

			Factory.Save();

			ReverseInvoice(invoice, true);

			Factory.Save();

			AccTransactionLines wIP1_2 = charge1_2.WIP;
			AccTransactionLines wIP2_1 = charge2_1.WIP;

			AssertNull("No REV should be created for Charge1_2", charge1_2.Revenue);
			AssertNull("No REV should be created for Charge2_1", charge2_1.Revenue);
			AssertEquals(-250m, wIP1_2.AL_LineAmount);
			AssertEquals(-150m, wIP2_1.AL_LineAmount);
		}

		public void TestClearSellAddress()
		{
			var address = TestObjectCreator.CreateAddress(TestObjectCreator.Debtor, OrgAddressType.Receivables, false);
			TestObjectCreator.Debtor.Addresses.Add(address);

			var job1 = CreateJob("Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job1, USD, .7M);
			var charge1 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, null, AUD, 250M, LocalClient);
			charge1.JR_OA_SellInvoiceAddress = address.PK;

			var job2 = CreateJob("Z00001001", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job2, USD, .7M);
			var charge2 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 100M, null, AUD, 150M, LocalClient);
			charge2.JR_OA_SellInvoiceAddress = address.PK;
			Factory.Save();

			var invoice = CreateARInvoice("Z1000", TestObjectCreator.Debtor, USD, .7M, "Invoice Z1000");
			invoice.AH_OA_InvoiceAddressOverride = address.PK;
			var line1 = CreateARInvoiceLine(invoice, job1, CC2, USD, 1M, "Charge Code 2", 200M);
			var line2 = CreateARInvoiceLine(invoice, job2, CC1, USD, 1M, "Charge Code 1", 105M);

			charge1.ReverseWIP(ZDateTime.Now);
			charge1.JR_AL_ARLine = line1.PK;
			charge2.ReverseWIP(ZDateTime.Now);
			charge2.JR_AL_ARLine = line2.PK;
			charge1.SetAmountsToLinkedLinesForTests();
			charge2.SetAmountsToLinkedLinesForTests();

			Factory.Save();

			AssertEquals("Charge 1 should have a Sell Address", invoice.AH_OA_InvoiceAddressOverride, address.PK);
			AssertEquals("Charge 1 should have a Sell Address", charge1.JR_OA_SellInvoiceAddress, address.PK);
			AssertEquals("Charge 2 should have a sell address", charge2.JR_OA_SellInvoiceAddress, address.PK);

			AccountingConfigurationRegistry.Instance.ResetAddressInChargeWhenARInvoiceReversed.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
			ReverseInvoice(invoice, true);

			Factory.Save();

			AssertEquals("Charge 1 should not have a Sell Address", charge1.JR_OA_SellInvoiceAddress, ZGuid.Empty);
			AssertEquals("Charge 2 should not have a sell address", charge2.JR_OA_SellInvoiceAddress, ZGuid.Empty);
		}

		public void TestClearRevenueLinksWithCreditNote()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job1, USD, .7M);
			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, null, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, null, AUD, -250M, LocalClient);

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job2, USD, .7M);
			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 100M, null, AUD, -150M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", AUD, 300M, null, AUD, 350M, LocalClient);

			ARCreditNote creditNote = CreateARCreditNote("Z1000", Creditor1, USD, .7M, "CreditNote Z1000");
			ARCreditNoteLine line1 = CreateARCreditNoteLine(creditNote, job1, CC2, USD, .7M, "Charge Code 2", 200M);
			ARCreditNoteLine line2 = CreateARCreditNoteLine(creditNote, job2, CC1, USD, .7M, "Charge Code 1", 105M);

			if (charge1_2.ARLine != null)
			{
				charge1_2.ARLine.AL_ReverseDate = ZDateTime.Now;
			}

			charge1_2.JR_AL_ARLine = line1.PK;
			if (charge2_1.ARLine != null)
			{
				charge2_1.ARLine.AL_ReverseDate = ZDateTime.Now;
			}

			charge2_1.JR_AL_ARLine = line2.PK;
			charge1_2.SetAmountsToLinkedLinesForTests();
			charge2_1.SetAmountsToLinkedLinesForTests();

			Factory.Save();

			ReverseInvoice(creditNote, true);

			Factory.Save();

			AccTransactionLines wIP1_2 = charge1_2.WIP;
			AccTransactionLines wIP2_1 = charge2_1.WIP;

			AssertNull("No REV should be created for Charge1_2.", charge1_2.Revenue);
			AssertNull("No REV should be created for Charge2_1.", charge2_1.Revenue);

			AssertNull(wIP1_2);
			AssertNull(wIP2_1);

			AssertEquals(-250m, charge1_2.JR_OSSellAmt);
			AssertEquals(-150m, charge2_1.JR_OSSellAmt);
		}

		public void TestTaxExpenseRecoveryChargeOnJobDeleted_OnInvoiceReversal()
		{
			AccountingMasterFilesRegistry.Instance.RevenueTaxExpenseRecoveryChargeCode.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, CC2.PK.ToGuid());

			Job job = CreateJob("Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job, USD, .7M);
			Charge charge1 = CreateCharge(job, CC1, "Charge Code 1", AUD, 0M, null, AUD, 100M, LocalClient);
			Charge charge2 = CreateCharge(job, CC2, "Tax expense recovery charge", AUD, 20M, null, AUD, 20M, LocalClient);
			Charge charge3 = CreateCharge(job, CC2, "Charge Code 2", AUD, 0M, null, AUD, 10M, LocalClient);

			var invoice = CreateARInvoice("Z1000", TestObjectCreator.Debtor, USD, 0.7M, "Invoice Z1000");
			var line1 = CreateARInvoiceLine(invoice, job, CC1, USD, 1M, "Non expense recovery line", 100M);
			var line2 = CreateARInvoiceLine(invoice, job, CC2, USD, 1M, "Expense recovery line", 20M);

			charge1.JR_AL_ARLine = line1.PK;
			charge2.JR_AL_ARLine = line2.PK;

			Factory.Save();

			AssertEquals("Pre-condition: Number of charges on the job before invoice reversal", 3, job.Charges.Count);

			ReverseInvoice(invoice, true);

			var chargesOnJob = job.Charges;
			AssertEquals("Number of charges on the job after invoice reversal", 2, job.Charges.Count);
			AssertEquals(charge1.PK, chargesOnJob[0].PK);
			AssertEquals(charge3.PK, chargesOnJob[1].PK);
		}

		public void TestSurchargeWillNotCreateWipWhenReversing()
		{
			var jobHeader = new Job.Loader(TestObjectCreator.CreateShipment("1000")).TryLoadOrCreate();
			Factory.Save();

			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "A00001", organisation: TestObjectCreator.Debtor);
			var line1 = TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC1, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine1", 1000m, TestObjectCreator.GST1.PK);
			var line2 = TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC2, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine2", 1000m, TestObjectCreator.GST1.PK);
			var line6A = TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC6, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine6A", 1000m, TestObjectCreator.GST1.PK);
			var line6B = TestObjectCreator.CreateARInvoiceLineWithJobCharge((ARInvoice)invoice, jobHeader, TestObjectCreator.CC6, invoice.TransactionCurrency, invoice.AH_ExchangeRate, "ARInvoiceLine6B", 1000m, TestObjectCreator.GST1.PK);

			jobHeader.Charges.Load();
			AssertEquals("PreCondition", 4, jobHeader.Charges.Count);
			foreach (JobCharge charge in jobHeader.Charges)
			{
				charge.JR_OH_SellAccount = invoice.AH_OH;
				charge.ARLine.AL_OH = invoice.AH_OH;
			}
			Factory.Save();

			var mappingJobChargeAndInvoiceLine = jobHeader.Charges
				.Cast<JobCharge>()
				.ToDictionary(x => x.JR_AL_ARLine, x => x);
			var mockSurchargeCalculator = new Mock<ISurchargeCalculator>();
			mockSurchargeCalculator.Setup(x => x.GetSurchargeLines(invoice))
				.Returns(new[] { line1, line6A });
			using (ObjectFactory.Substitute(mockSurchargeCalculator.Object))
			{
				ReverseInvoice(invoice, true);

				AssertReversedJobChargeSellInfoBeingCleared(line1, true);
				AssertReversedJobChargeSellInfoBeingCleared(line2, false);
				AssertReversedJobChargeSellInfoBeingCleared(line6A, true);
				AssertReversedJobChargeSellInfoBeingCleared(line6B, false);

				Factory.Save();

				AssertWipCreation(line1, false);
				AssertWipCreation(line2, true);
				AssertWipCreation(line6A, false);
				AssertWipCreation(line6B, true);
			}

			void AssertReversedJobChargeSellInfoBeingCleared(InvoicingLineBase reversedLine, bool expectedBeingCleared)
			{
				var jobCharge = mappingJobChargeAndInvoiceLine[reversedLine.PK];
				AssertEquals("JR_OH_SellAccountInfo.HasChanges", expectedBeingCleared, jobCharge.JR_OH_SellAccountInfo.HasChanges);
				AssertEquals("JR_EstimatedRevenueInfo.HasChanges", expectedBeingCleared, jobCharge.JR_EstimatedRevenueInfo.HasChanges);
				AssertEquals("JR_LocalSellAmtInfo.HasChanges", expectedBeingCleared, jobCharge.JR_LocalSellAmtInfo.HasChanges);
				AssertEquals("JR_OSSellAmtInfo.HasChanges", expectedBeingCleared, jobCharge.JR_OSSellAmtInfo.HasChanges);
				if (expectedBeingCleared)
				{
					AssertEquals("JR_OH_SellAccount", ZGuid.Empty, jobCharge.JR_OH_SellAccount);
					AssertEquals("JR_EstimatedRevenue", 0m, jobCharge.JR_EstimatedRevenue);
					AssertEquals("JR_LocalSellAmt", 0m, jobCharge.JR_LocalSellAmt);
					AssertEquals("JR_OSSellAmt", 0m, jobCharge.JR_OSSellAmt);
				}

				AssertEquals(ZGuid.Empty, jobCharge.JR_AL_ARLine);
			}

			void AssertWipCreation(InvoicingLineBase reversedLine, bool expectedWipBeingCreated)
			{
				var jobCharge = mappingJobChargeAndInvoiceLine[reversedLine.PK];

				if (expectedWipBeingCreated)
				{
					AssertNotEquals("JR_AL_ARLine", ZGuid.Empty, jobCharge.JR_AL_ARLine);
					AssertEquals("ARLine.AL_LineType", "WIP", jobCharge.ARLine.AL_LineType);
				}
				else
				{
					AssertEquals("JR_AL_ARLine", ZGuid.Empty, jobCharge.JR_AL_ARLine);
				}
			}
		}

		#endregion

		public void TestReverseCFXJournal()
		{
			AccountingConfigurationRegistry.Instance.JobInvoicingCFXEnabled.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

			Job job1 = CreateJob("Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job1, USD, .7M);
			Charge charge1_1 = CreateCharge(job1, CC2, "Charge Code 2", AUD, 200M, null, USD, 250M, LocalClient);
			charge1_1.JR_OSSellAmt = 50.00m;

			Assert("CFX Amount should be set", charge1_1.JR_CFXAmt > 0);

			ZQuery cFXFilter = new ZQuery(AccTransactionHeaderSchema.AH_TransactionType, ZArchitecture.Core.TransactionTypes.Journal);
			cFXFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, SQLComparisonOperator.Equal, ZArchitecture.Core.LedgerTypes.JobCosting);
			cFXFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			ZQuery invoiceFilter = new ZQuery(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			invoiceFilter.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionType, SQLComparisonOperator.Equal, ZArchitecture.Core.TransactionTypes.Invoice);
			invoiceFilter.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);

			JCJournalHeader[] cFXHeaders = (JCJournalHeader[])Factory.Load(typeof(JCJournalHeader), cFXFilter);
			ARInvoice[] invoiceHeaders = (ARInvoice[])Factory.Load(typeof(ARInvoice), invoiceFilter);

			AssertEquals("Precondition - shouldn't be any CFX yet", 0, cFXHeaders.Length);
			AssertEquals("Precondition - shouldn't be any invoices yet", 0, invoiceHeaders.Length);

			new InvoicingPostManager(job1).CreateTransactions(JobInvoicingPostingOption.All);

			Factory.Save();

			cFXHeaders = (JCJournalHeader[])Factory.Load(typeof(JCJournalHeader), cFXFilter);
			invoiceHeaders = (ARInvoice[])Factory.Load(typeof(ARInvoice), invoiceFilter);

			AssertEquals("Should have only been one CFX Header created", 1, cFXHeaders.Length);
			AssertEquals("Should have had one invoice posted", 1, invoiceHeaders.Length);

			ARInvoice invoiceWithCFX = invoiceHeaders[0];

			new ReverseInvoicingTransformer(Factory).Transform(invoiceWithCFX);

			AssertNull("Charge should have CFX reset", charge1_1.CFXLine);

			Assert("CFX Transaction should be reversed as well", cFXHeaders[0].AH_IsCancelled);
		}

		#region Cost

		public void TestReverseApportionCost()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			Job job1 = CreateJob("Z00001000", LocalClient, 5M, null, 0M);
			job1.PlugInData = shipment1;
			CreateExchangeRate(job1, USD, .7M);
			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, null, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC3, "Charge Code 2", AUD, 200M, null, AUD, 250M, LocalClient);

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, null, 0M);
			job2.PlugInData = shipment2;
			CreateExchangeRate(job2, USD, .7M);
			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 100M, null, AUD, 150M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC3, "Charge Code 2", AUD, 300M, null, AUD, 350M, LocalClient);
			Factory.Save();

			APInvoice invoice = CreateAPInvoice("Z1000", Creditor1, USD, .7M, "Invoice Z1000");
			invoice.AH_ChequeOrReference = "ABC";
			APInvoiceLine line1 = CreateAPInvoiceLine(invoice, job1, CC3, USD, .7M, "Charge Code 2", 200M, false);
			APInvoiceLine line2 = CreateAPInvoiceLine(invoice, job2, CC1, USD, .7M, "Charge Code 1", 150M, false);
			var compatibleLineType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLineType(invoice.AH_Ledger, invoice.AH_TransactionType);
			line1.AL_LineType = compatibleLineType;
			line2.AL_LineType = compatibleLineType;

			var cost = AddNewConsolCost(consol, 200m, invoice);
			// Some clean up as we set too much
			cost.E6_InvoiceNum = "";
			cost.E6_InvoiceDate = ZDateTime.Empty;
			cost.E6_OH_Creditor = ZGuid.Empty;

			charge1_2.ReverseAccrual(ZDateTime.Now);
			charge1_2.JR_AL_APLine = line1.PK;
			charge2_1.ReverseAccrual(ZDateTime.Now);
			charge2_1.JR_AL_APLine = line2.PK;
			charge1_2.SetAmountsToLinkedLinesForTests();
			charge2_1.SetAmountsToLinkedLinesForTests();
			charge1_1.Job.JH_ProfitShareInvoice = ZGuid.NewZGuid();
			charge1_1.Job.JH_IsProfitSharePosted = true;
			charge1_2.JR_E6 = cost.PK;
			charge1_2.JR_CostReference = invoice.AH_ChequeOrReference;

			AssertEquals("Profit share invoice guid is set", false, charge1_1.Job.JH_ProfitShareInvoice.IsEmpty);
			AssertEquals("Profit share posted", true, charge1_1.Job.JH_IsProfitSharePosted);

			Factory.Save();

			ReverseInvoice(invoice);

			Assert(charge1_2.JR_IsApportioned);
			AssertEquals(200M, charge1_2.JR_LocalCostAmt);
			AssertEquals(200M, charge1_2.JR_OSCostAmt);

			AssertEquals("Supplier Cost Reference on Job Charge should be cleared", ZString.Empty, charge1_2.JR_CostReference);
			AssertEquals("Supplier Cost Reference on Consol Cost should be cleared", ZString.Empty, cost.E6_CostReference);
		}

		public void TestReverseApportionedCostCorrectlyAdjustsGST()
		{
			var localClientPK = LocalClient.PK;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			Factory.Save();
			var shipment1 = consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			var cost = AddNewConsolCost(consol, 0m, null, CC3, LocalClient, TestObjectCreator.GST1);
			cost.E6_IsTaxAmountOverridden = true;

			var calstrat = new JobConsolCost.InvoicingBaseConsolCostCalculationStrategy(cost);
			calstrat.UpdateApportionmentChargesListing();
			cost.E6_OSCostAmount = 200m;
			cost.E6_LocalCostAmount = 200m;
			AssertEquals(cost.ApportionmentCharges.Count, 3);

			cost.ApportionmentCharges.Sort(JobCharge.Schema.JR_OSCostGSTAmt_Calc, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("One charge will be adjusted to make rounded job charge total match cost amount. After sort this will be the first charge", 6.66m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("Other charges will be unadjusted ", 6.67m, cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc);
			AssertEquals("Other charges will be unadjusted ", 6.67m, cost.ApportionmentCharges[2].JR_OSCostGSTAmt_Calc);

			Factory.Save();

			var job1 = TestObjectCreator.CreateJob(shipment1);
			var invoice = CreateAPInvoice("Z1000", LocalClient, AUD, 200M, "Invoice Z1000");
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, AUD, 1m, cost.ApportionmentCharges[0].JR_OSCostAmt, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc, 0m, CC3.PK);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, AUD, 1m, cost.ApportionmentCharges[1].JR_OSCostAmt, cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc, 0m, CC3.PK);
			var line3 = TestObjectCreator.CreateInvoiceLine(invoice, AUD, 1m, cost.ApportionmentCharges[2].JR_OSCostAmt, cost.ApportionmentCharges[2].JR_OSCostGSTAmt_Calc, 0m, CC3.PK);

			cost.E6_AH_APInvoice = invoice.PK;
			cost.ApportionmentCharges[0].JR_JH = job1.PK;
			cost.ApportionmentCharges[0].ReverseAccrual(ZDateTime.Now);
			cost.ApportionmentCharges[0].JR_AL_APLine = line1.PK;
			line1.AL_JH = job1.PK;
			line1.AL_AT = cost.ApportionmentCharges[0].JR_AT_CostGSTRate;
			line1.AL_OSAmount = -(cost.ApportionmentCharges[0].JR_OSCostAmt + cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			line1.AL_LineAmount = -cost.ApportionmentCharges[0].JR_OSCostAmt;
			line1.AL_GSTVAT = -cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc;
			cost.ApportionmentCharges[1].JR_JH = job1.PK;
			cost.ApportionmentCharges[1].ReverseAccrual(ZDateTime.Now);
			cost.ApportionmentCharges[1].JR_AL_APLine = line2.PK;
			line2.AL_JH = job1.PK;
			line2.AL_AT = cost.ApportionmentCharges[1].JR_AT_CostGSTRate;
			line2.AL_OSAmount = -(cost.ApportionmentCharges[1].JR_OSCostAmt + cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc);
			line2.AL_LineAmount = -cost.ApportionmentCharges[1].JR_OSCostAmt;
			line2.AL_GSTVAT = -cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc;
			cost.ApportionmentCharges[2].JR_JH = job1.PK;
			cost.ApportionmentCharges[2].ReverseAccrual(ZDateTime.Now);
			cost.ApportionmentCharges[2].JR_AL_APLine = line3.PK;
			line3.AL_JH = job1.PK;
			line3.AL_AT = cost.ApportionmentCharges[2].JR_AT_CostGSTRate;
			line3.AL_OSAmount = -(cost.ApportionmentCharges[2].JR_OSCostAmt + cost.ApportionmentCharges[2].JR_OSCostGSTAmt_Calc);
			line3.AL_LineAmount = -cost.ApportionmentCharges[2].JR_OSCostAmt;
			line3.AL_GSTVAT = -cost.ApportionmentCharges[2].JR_OSCostGSTAmt_Calc;

			Factory.Save();

			ReverseInvoice(invoice);

			cost.ApportionmentCharges.Sort(JobCharge.Schema.JR_OSCostGSTAmt_Calc, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("Charge will be unadjusted as reversing reset E6_IsTaxAmountOverridden flag.", 6.67m, cost.ApportionmentCharges[0].JR_OSCostGSTAmt_Calc);
			AssertEquals("Other charges will be unadjusted ", 6.67m, cost.ApportionmentCharges[1].JR_OSCostGSTAmt_Calc);
			AssertEquals("Other charges will be unadjusted ", 6.67m, cost.ApportionmentCharges[2].JR_OSCostGSTAmt_Calc);
		}

		public void TestClearCostLinks()
		{
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job1, USD, .7M);
			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, null, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC3, "Charge Code 2", AUD, 200M, null, AUD, 250M, LocalClient);

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job2, USD, .7M);
			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, 100M, null, AUD, 150M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC3, "Charge Code 2", AUD, 300M, null, AUD, 350M, LocalClient);
			Factory.Save();

			APInvoice invoice = CreateAPInvoice("Z1000", Creditor1, USD, .7M, "Invoice Z1000");
			APInvoiceLine line1 = CreateAPInvoiceLine(invoice, job1, CC3, USD, .7M, "Charge Code 2", 200M, false);
			APInvoiceLine line2 = CreateAPInvoiceLine(invoice, job2, CC1, USD, .7M, "Charge Code 1", 150M, false);

			var compatibleLineType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLineType(invoice.AH_Ledger, invoice.AH_TransactionType);
			line1.AL_LineType = compatibleLineType;
			line2.AL_LineType = compatibleLineType;

			charge1_2.ReverseAccrual(ZDateTime.Now);
			charge1_2.JR_AL_APLine = line1.PK;

			charge2_1.ReverseAccrual(ZDateTime.Now);
			charge2_1.JR_AL_APLine = line2.PK;

			invoice.AH_DocumentReceivedDate = ZDateTime.Now;

			charge1_2.SetAmountsToLinkedLinesForTests();
			charge2_1.SetAmountsToLinkedLinesForTests();
			charge1_2.JR_APInvoiceNum = invoice.AH_TransactionNum;
			charge2_1.JR_APInvoiceNum = invoice.AH_TransactionNum;
			charge1_2.JR_APInvoiceDate = invoice.AH_InvoiceDate;
			charge2_1.JR_APInvoiceDate = invoice.AH_InvoiceDate;
			charge1_2.JR_PaymentDate = invoice.AH_DueDate;
			charge2_1.JR_PaymentDate = invoice.AH_DueDate;
			charge1_2.JR_APDocumentReceivedDate = invoice.AH_DocumentReceivedDate;
			charge2_1.JR_APDocumentReceivedDate = invoice.AH_DocumentReceivedDate;

			charge1_1.Job.JH_ProfitShareInvoice = ZGuid.NewZGuid();
			charge1_1.Job.JH_IsProfitSharePosted = true;

			CombineAssertions("PreCondition", () => {
				AssertEquals(nameof(charge1_2.JR_APInvoiceDate), false, charge1_2.JR_APInvoiceDate.IsEmpty);
				AssertEquals(nameof(charge1_2.JR_PaymentDate), false, charge1_2.JR_PaymentDate.IsEmpty);
				AssertEquals(nameof(charge1_2.JR_APDocumentReceivedDate), false, charge1_2.JR_APDocumentReceivedDate.IsEmpty);
			});

			AssertEquals("Profit share invoice guid is set", false, charge1_1.Job.JH_ProfitShareInvoice.IsEmpty);
			AssertEquals("Profit share posted", true, charge1_1.Job.JH_IsProfitSharePosted);

			Factory.Save();

			ReverseInvoice(invoice);

			if (invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				invoice.Delete(); //That makes it identical to what happens functionally on the Unapproved Transactions module.
			}

			Factory.Save();

			AccTransactionLines accrual1_2 = charge1_2.Accrual;
			AccTransactionLines accrual2_1 = charge2_1.Accrual;

			AssertEquals("no CST should be created for the ", null, charge1_2.Cost);
			AssertEquals("no CST should be created for the ", null, charge2_1.Cost);
			AssertEquals(200m, accrual1_2.AL_LineAmount);
			AssertEquals(100m, accrual2_1.AL_LineAmount);

			Assert("AP Invoice number should be empty", charge1_2.JR_APInvoiceNum.IsEmpty);
			Assert("AP Invoice date should be empty", charge1_2.JR_APInvoiceDate.IsEmpty);
			Assert("AP Invoice due date should be empty", charge1_2.JR_PaymentDate.IsEmpty);
			Assert("AP Invoice doc rec date should be empty", charge1_2.JR_APDocumentReceivedDate.IsEmpty);

			AssertEquals("Profit share invoice guid is cleared", true, charge1_1.Job.JH_ProfitShareInvoice.IsEmpty);
			AssertEquals("Profit share posted flag is cleared", false, charge1_1.Job.JH_IsProfitSharePosted);
		}

		public void TestClearCostLinksWithCreditNote()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			Job job1 = CreateJob("Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job1, USD, .7M);
			Charge charge1_1 = CreateCharge(job1, CC1, "Charge Code 1", AUD, 100M, null, AUD, 150M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC2, "Charge Code 2", AUD, -200M, null, AUD, 250M, LocalClient);

			Job job2 = CreateJob("Z00001001", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job2, USD, .7M);
			Charge charge2_1 = CreateCharge(job2, CC1, "Charge Code 1", AUD, -100M, null, AUD, 150M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC2, "Charge Code 2", AUD, 300M, null, AUD, 350M, LocalClient);
			Factory.Save();

			APCreditNote creditNote = CreateAPCreditNote("Z1000", Creditor1, USD, .7M, "CreditNote Z1000");
			APCreditNoteLine line1 = CreateAPCreditNoteLine(creditNote, job1, CC2, USD, .7M, "Charge Code 2", 200M, false);
			APCreditNoteLine line2 = CreateAPCreditNoteLine(creditNote, job2, CC1, USD, .7M, "Charge Code 1", 150M, false);

			var compatibleLineType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLineType(creditNote.AH_Ledger, creditNote.AH_TransactionType);
			line1.AL_LineType = compatibleLineType;
			line2.AL_LineType = compatibleLineType;

			charge1_2.ReverseAccrual(ZDateTime.Now);

			charge1_2.JR_AL_APLine = line1.PK;
			charge2_1.JR_AL_APLine = line2.PK;
			charge1_2.SetAmountsFromLinkedLinesForTests();
			charge2_1.SetAmountsFromLinkedLinesForTests();

			Factory.Save();

			ReverseInvoice(creditNote);

			if (creditNote.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				creditNote.Delete(); //That makes it identical to what happens functionally on the Unapproved Transactions module.
			}

			Factory.Save();

			AccTransactionLines accrual1_2 = charge1_2.Accrual;
			AccTransactionLines accrual2_1 = charge2_1.Accrual;

			AssertEquals("no CST should be created for the ", null, charge1_2.Cost);
			AssertEquals("no CST should be created for the ", null, charge2_1.Cost);
			AssertEquals("0 shouldn't create an accrual", null, accrual1_2);
			AssertEquals("0 shouldn't create an accrual", null, accrual2_1);

			AssertEquals(-200m, charge1_2.JR_OSCostAmt);
			AssertEquals(-150m, charge2_1.JR_OSCostAmt);
		}

		public void TestZeroAmountsAfterReversingNegativeApportionmentWithCreditNote()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			SetJobDetails(job1, "Z00001000", LocalClient, 5M, null, 0M);
			SetJobDetails(job2, "Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job2, USD, .7M);
			Charge charge1 = CreateCharge(job1, CC3, "Charge Code", AUD, -150M, null, AUD, 0M, LocalClient);
			Charge charge2 = CreateCharge(job2, CC3, "Charge Code", AUD, -50M, null, AUD, 0M, LocalClient);

			APCreditNote creditNote = CreateAPCreditNote("Z1000", Creditor1, USD, .7M, "CreditNote Z1000");
			APCreditNoteLine line1 = CreateAPCreditNoteLine(creditNote, job1, CC3, USD, .7M, "Charge Code", 150M, false);
			APCreditNoteLine line2 = CreateAPCreditNoteLine(creditNote, job2, CC3, USD, .7M, "Charge Code", 50M, false);

			line1.AL_LineType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLineType(creditNote.AH_Ledger, creditNote.AH_TransactionType);
			line2.AL_LineType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLineType(creditNote.AH_Ledger, creditNote.AH_TransactionType);

			var cost = AddNewConsolCost(consol, -200m, creditNote, CC3, creditNote.Header);

			charge1.JR_AL_APLine = line1.PK;
			charge1.JR_E6 = cost.PK;
			charge1.JR_APInvoiceNum = creditNote.AH_TransactionNum;
			charge1.JR_APInvoiceDate = creditNote.AH_InvoiceDate;
			charge1.JR_OH_CostAccount = creditNote.AH_OH;
			charge1.JR_PaymentDate = cost.E6_PaymentDate;
			charge1.SetAmountsToLinkedLinesForTests();

			charge2.JR_AL_APLine = line2.PK;
			charge2.JR_E6 = cost.PK;
			charge2.JR_APInvoiceNum = creditNote.AH_TransactionNum;
			charge2.JR_APInvoiceDate = creditNote.AH_InvoiceDate;
			charge2.JR_OH_CostAccount = creditNote.AH_OH;
			charge2.JR_PaymentDate = cost.E6_PaymentDate;
			charge2.SetAmountsToLinkedLinesForTests();

			Factory.Save();

			ReverseInvoice(creditNote);

			if (creditNote.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				creditNote.Delete(); //That makes it identical to what happens functionally on the Unapproved Transactions module.
			}

			Factory.Save();
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge1.JR_LocalCostAmt);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge1.JR_OSCostAmt);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge2.JR_LocalCostAmt);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge2.JR_OSCostAmt);
			AssertEquals("Negative ConsolCost should be zeroed.", 0M, cost.E6_LocalCostAmount);
			AssertEquals("Negative ConsolCost should be zeroed.", 0M, cost.E6_OSCostAmount);
		}

		public void TestZeroAmountsAfterReversingNegativeAndPositiveApportionmentsWithInvoice()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			SetJobDetails(job1, "Z00001000", LocalClient, 5M, null, 0M);
			SetJobDetails(job2, "Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job2, USD, .7M);
			Charge charge1_1 = CreateCharge(job1, CC3, "Charge Code", AUD, -150M, null, AUD, 0M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC3, "Charge Code", AUD, 200M, null, AUD, 0M, LocalClient);
			Charge charge2_1 = CreateCharge(job2, CC3, "Charge Code", AUD, -50M, null, AUD, 0M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC3, "Charge Code", AUD, 100M, null, AUD, 0M, LocalClient);
			Factory.Save();

			APInvoice invoice = CreateAPInvoice("Z1000", Creditor1, USD, .7M, "Invoice Z1000");
			var compatibleLineType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLineType(invoice.AH_Ledger, invoice.AH_TransactionType);
			APInvoiceLine line1_1 = CreateAPInvoiceLine(invoice, job1, CC3, USD, .7M, "Charge Code", -150M, false);
			APInvoiceLine line1_2 = CreateAPInvoiceLine(invoice, job1, CC3, USD, .7M, "Charge Code", 200M, false);
			APInvoiceLine line2_1 = CreateAPInvoiceLine(invoice, job2, CC3, USD, .7M, "Charge Code", -50M, false);
			APInvoiceLine line2_2 = CreateAPInvoiceLine(invoice, job2, CC3, USD, .7M, "Charge Code", 100M, false);
			line1_1.AL_LineType = compatibleLineType;
			line1_2.AL_LineType = compatibleLineType;
			line2_1.AL_LineType = compatibleLineType;
			line2_2.AL_LineType = compatibleLineType;

			var cost1 = AddNewConsolCost(consol, -200m, invoice);

			charge1_1.JR_AL_APLine = line1_1.PK;
			charge1_1.JR_E6 = cost1.PK;
			charge2_1.JR_AL_APLine = line2_1.PK;
			charge2_1.JR_E6 = cost1.PK;
			charge1_1.SetAmountsToLinkedLinesForTests();
			charge2_1.SetAmountsToLinkedLinesForTests();
			charge1_1.JR_APInvoiceNum = cost1.E6_InvoiceNum;
			charge1_1.JR_APInvoiceDate = cost1.E6_InvoiceDate;
			charge1_1.JR_OH_CostAccount = Creditor1.PK;
			charge1_1.JR_AT_CostGSTRate = ZGuid.Empty;
			charge1_1.JR_PaymentDate = cost1.E6_PaymentDate;
			charge2_1.JR_APInvoiceNum = cost1.E6_InvoiceNum;
			charge2_1.JR_APInvoiceDate = cost1.E6_InvoiceDate;
			charge2_1.JR_OH_CostAccount = Creditor1.PK;
			charge2_1.JR_AT_CostGSTRate = ZGuid.Empty;
			charge2_1.JR_PaymentDate = cost1.E6_PaymentDate;

			var cost2 = AddNewConsolCost(consol, 300m, invoice);

			charge1_2.ReverseAccrual(ZDateTime.Now);

			charge1_2.JR_AL_APLine = line1_2.PK;
			charge1_2.JR_E6 = cost2.PK;

			charge2_2.ReverseAccrual(ZDateTime.Now);

			charge2_2.JR_AL_APLine = line2_2.PK;
			charge2_2.JR_E6 = cost2.PK;
			charge1_2.SetAmountsToLinkedLinesForTests();
			charge2_2.SetAmountsToLinkedLinesForTests();
			charge1_2.JR_APInvoiceNum = cost2.E6_InvoiceNum;
			charge1_2.JR_APInvoiceDate = cost2.E6_InvoiceDate;
			charge1_2.JR_OH_CostAccount = Creditor1.PK;
			charge1_2.JR_AT_CostGSTRate = ZGuid.Empty;
			charge1_2.JR_PaymentDate = cost2.E6_PaymentDate;
			charge2_2.JR_APInvoiceNum = cost2.E6_InvoiceNum;
			charge2_2.JR_APInvoiceDate = cost2.E6_InvoiceDate;
			charge2_2.JR_OH_CostAccount = Creditor1.PK;
			charge2_2.JR_AT_CostGSTRate = ZGuid.Empty;
			charge2_2.JR_PaymentDate = cost2.E6_PaymentDate;

			Factory.Save();

			ReverseInvoice(invoice);

			if (invoice.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				invoice.Delete(); //That makes it identical to what happens functionally on the Unapproved Transactions module.
			}

			Factory.Save();

			Assert(charge1_1.JR_IsApportioned);
			Assert(charge2_1.JR_IsApportioned);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge1_1.JR_LocalCostAmt);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge1_1.JR_OSCostAmt);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge2_1.JR_LocalCostAmt);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge2_1.JR_OSCostAmt);
			AssertEquals("Negative ConsolCost should be zeroed.", 0M, cost1.E6_LocalCostAmount);
			AssertEquals("Negative ConsolCost should be zeroed.", 0M, cost1.E6_OSCostAmount);

			Assert(charge1_2.JR_IsApportioned);
			Assert(charge2_2.JR_IsApportioned);
			AssertEquals("Charge connected to positive ConsolCost should be remain a value.", 200M, charge1_2.JR_LocalCostAmt);
			AssertEquals("Charge connected to positive ConsolCost should be remain a value.", 200M, charge1_2.JR_OSCostAmt);
			AssertEquals("Charge connected to positive ConsolCost should be remain a value.", 100M, charge2_2.JR_LocalCostAmt);
			AssertEquals("Charge connected to positive ConsolCost should be remain a value.", 100M, charge2_2.JR_OSCostAmt);
			AssertEquals("Positive ConsolCost should be remain a value.", 300M, cost2.E6_LocalCostAmount);
			AssertEquals("Positive ConsolCost should be remain a value.", 300M, cost2.E6_OSCostAmount);
		}

		public void TestZeroAmountsAfterReversingNegativeAndPositiveApportionmentsWithCreditNote()
		{
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ForwardingShipment shipment2 = consol.Shipments.AddNew();

			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			SetJobDetails(job1, "Z00001000", LocalClient, 5M, null, 0M);
			SetJobDetails(job2, "Z00001000", LocalClient, 5M, null, 0M);
			CreateExchangeRate(job1, USD, .7M);
			CreateExchangeRate(job2, USD, .7M);
			Charge charge1_1 = CreateCharge(job1, CC3, "Charge Code", AUD, 150M, null, AUD, 0M, LocalClient);
			Charge charge1_2 = CreateCharge(job1, CC3, "Charge Code", AUD, -200M, null, AUD, 0M, LocalClient);
			Charge charge2_1 = CreateCharge(job2, CC3, "Charge Code", AUD, 50M, null, AUD, 0M, LocalClient);
			Charge charge2_2 = CreateCharge(job2, CC3, "Charge Code", AUD, -100M, null, AUD, 0M, LocalClient);
			Factory.Save();

			APCreditNote creditNote = CreateAPCreditNote("Z1000", Creditor1, USD, .7M, "CreditNote Z1000");
			var compatibleLineType = new AccTransactionLinesCompatibilityMatrixTestHelper().GetCompatibleLineType(creditNote.AH_Ledger, creditNote.AH_TransactionType);
			APCreditNoteLine line1_1 = CreateAPCreditNoteLine(creditNote, job1, CC3, USD, .7M, "Charge Code", 150M, false);
			APCreditNoteLine line1_2 = CreateAPCreditNoteLine(creditNote, job1, CC3, USD, .7M, "Charge Code", -300M, false);
			APCreditNoteLine line2_1 = CreateAPCreditNoteLine(creditNote, job2, CC3, USD, .7M, "Charge Code", 50M, false);
			APCreditNoteLine line2_2 = CreateAPCreditNoteLine(creditNote, job2, CC3, USD, .7M, "Charge Code", -300M, false);
			line1_1.AL_LineType = compatibleLineType;
			line1_2.AL_LineType = compatibleLineType;
			line2_1.AL_LineType = compatibleLineType;
			line2_2.AL_LineType = compatibleLineType;

			JobConsolCost cost1 = AddNewConsolCost(consol, 200m, creditNote, CC3, Creditor1);

			charge1_1.ReverseAccrual(ZDateTime.Now);
			charge1_1.JR_AL_APLine = line1_1.PK;
			charge1_1.JR_E6 = cost1.PK;
			charge2_1.ReverseAccrual(ZDateTime.Now);
			charge2_1.JR_AL_APLine = line2_1.PK;
			charge2_1.JR_E6 = cost1.PK;
			charge1_1.SetAmountsToLinkedLinesForTests();
			charge2_1.SetAmountsToLinkedLinesForTests();
			charge1_1.JR_APInvoiceNum = cost1.E6_InvoiceNum;
			charge1_1.JR_APInvoiceDate = cost1.E6_InvoiceDate;
			charge1_1.JR_OH_CostAccount = Creditor1.PK;
			charge1_1.JR_AT_CostGSTRate = ZGuid.Empty;
			charge1_1.JR_PaymentDate = cost1.E6_PaymentDate;
			charge2_1.JR_APInvoiceNum = cost1.E6_InvoiceNum;
			charge2_1.JR_APInvoiceDate = cost1.E6_InvoiceDate;
			charge2_1.JR_OH_CostAccount = Creditor1.PK;
			charge2_1.JR_AT_CostGSTRate = ZGuid.Empty;
			charge2_1.JR_PaymentDate = cost1.E6_PaymentDate;

			JobConsolCost cost2 = AddNewConsolCost(consol, -300m, creditNote);

			charge1_2.JR_AL_APLine = line1_2.PK;
			charge1_2.JR_E6 = cost2.PK;
			charge2_2.JR_AL_APLine = line2_2.PK;
			charge2_2.JR_E6 = cost2.PK;
			charge1_2.SetAmountsToLinkedLinesForTests();
			charge2_2.SetAmountsToLinkedLinesForTests();
			charge1_2.JR_APInvoiceNum = cost2.E6_InvoiceNum;
			charge1_2.JR_APInvoiceDate = cost2.E6_InvoiceDate;
			charge1_2.JR_OH_CostAccount = Creditor1.PK;
			charge1_2.JR_AT_CostGSTRate = ZGuid.Empty;
			charge1_2.JR_PaymentDate = cost2.E6_PaymentDate;
			charge2_2.JR_APInvoiceNum = cost2.E6_InvoiceNum;
			charge2_2.JR_APInvoiceDate = cost2.E6_InvoiceDate;
			charge2_2.JR_OH_CostAccount = Creditor1.PK;
			charge2_2.JR_AT_CostGSTRate = ZGuid.Empty;
			charge2_2.JR_PaymentDate = cost2.E6_PaymentDate;

			Factory.Save();

			ReverseInvoice(creditNote);

			if (creditNote.AH_Ledger == LedgerTypes.UnapprovedPayableTransactions)
			{
				creditNote.Delete(); //That makes it identical to what happens functionally on the Unapproved Transactions module.
			}

			Factory.Save();

			Assert(charge1_1.JR_IsApportioned);
			Assert(charge2_1.JR_IsApportioned);
			AssertEquals("Charge connected to positive ConsolCost should be remain a value.", 150M, charge1_1.JR_LocalCostAmt);
			AssertEquals("Charge connected to positive ConsolCost should be remain a value.", 150M, charge1_1.JR_OSCostAmt);
			AssertEquals("Charge connected to positive ConsolCost should be remain a value.", 50M, charge2_1.JR_LocalCostAmt);
			AssertEquals("Charge connected to positive ConsolCost should be remain a value.", 50M, charge2_1.JR_OSCostAmt);
			AssertEquals("Positive ConsolCost should be remain a value.", 200M, cost1.E6_LocalCostAmount);
			AssertEquals("Positive ConsolCost should be remain a value.", 200M, cost1.E6_OSCostAmount);

			Assert(charge1_2.JR_IsApportioned);
			Assert(charge2_2.JR_IsApportioned);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge1_2.JR_LocalCostAmt);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge1_2.JR_OSCostAmt);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge2_2.JR_LocalCostAmt);
			AssertEquals("Charge connected to negative ConsolCost should be zeroed.", 0M, charge2_2.JR_OSCostAmt);
			AssertEquals("Negative ConsolCost should be zeroed.", 0M, cost2.E6_LocalCostAmount);
			AssertEquals("Negative ConsolCost should be zeroed.", 0M, cost2.E6_OSCostAmount);
		}

		#endregion

		#region Job Revenue Journal Tests

		public void TestReverseManualJobRevenueJournal_ARCharges()
		{
			Job job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
			JobRevenueJournal journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			Factory.Save();

			Charge charge1 = job.Charges[0];
			Charge charge2 = job.Charges[1];

			AssertEquals("Precondition: charge must be linked to JR journal.", charge1.Revenue.TransactionHeader.PK, journal.PK);
			AssertEquals("Precondition: charge must be linked to JR journal.", charge2.Revenue.TransactionHeader.PK, journal.PK);
			charge2.JR_OSCostAmt = 100M;

			journal.AH_IsCancelled = true;
			GetTransformer(Factory).Transform(journal);
			Factory.Save();

			AssertEquals("Precondition: charge 2 must have cost amount value to test correct sell fields clearing.", 100M, charge2.JR_OSCostAmt);

			AssertEquals("charge1.IsDeleted", true, charge1.IsDeleted);
			AssertEquals("charge2.IsDeleted", false, charge2.IsDeleted);
			AssertNull("charge2.Revenue", charge2.Revenue);
			AssertNull("charge2.WIP", charge2.WIP);
			AssertEquals("charge2.JR_OSSellAmt", 0M, charge2.JR_OSSellAmt);
		}

		public void TestReverseManualJobRevenueJournal_APCharges_WhenChargeSellAmountIsNotZero()
		{
			var job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
			var journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			((JobRevenueJournalLine)journal.Lines[0]).CostRevenueType = TransactionLineTypes.Cost;
			((JobRevenueJournalLine)journal.Lines[1]).CostRevenueType = TransactionLineTypes.Cost;
			Factory.Save();

			var charge1 = job.Charges[0];
			var charge2 = job.Charges[1];

			AssertEquals(charge1.APLine.AL_AH, journal.PK);
			AssertEquals(charge1.APLine.AL_AH, journal.PK);
			charge2.JR_OSSellAmt = 100m;

			AssertEquals("Precondition", 100m, charge2.JR_OSSellAmt);

			journal.AH_IsCancelled = true;
			GetTransformer(Factory).Transform(journal);
			Factory.Save();

			Assert(charge1.IsDeleted);
			Assert(!charge2.IsDeleted);
			AssertEquals(ZGuid.Empty, charge2.JR_AL_APLine);
			AssertEquals(0m, charge2.JR_LocalCostAmt);
			AssertEquals(0m, charge2.JR_OSCostAmt);
		}

		public void TestReverseManualJobRevenueJournal_APCharges_WhenChargeRevenueIsPosted()
		{
			var job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
			var journal = TestObjectCreator.CreateJobRevenueJournal(TestObjectCreator.CC1, job, 100M);
			((JobRevenueJournalLine)journal.Lines[0]).CostRevenueType = TransactionLineTypes.Cost;
			((JobRevenueJournalLine)journal.Lines[1]).CostRevenueType = TransactionLineTypes.Cost;
			Factory.Save();

			var charge1 = job.Charges[0];
			var charge2 = job.Charges[1];

			AssertEquals(charge1.APLine.AL_AH, journal.PK);
			AssertEquals(charge1.APLine.AL_AH, journal.PK);

			var invoice = Factory.New<ARInvoice>();
			var line = (TransactionLine)invoice.Lines.AddNew();
			line.AL_LineType = TransactionLineTypes.Revenue;
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			invoice.AH_JH = job.PK;
			charge2.JR_AL_ARLine = line.PK;

			Assert("precondition", charge2.IsRevenuePosted);

			journal.AH_IsCancelled = true;
			GetTransformer(Factory).Transform(journal);
			Factory.Save();

			Assert(charge1.IsDeleted);
			Assert(!charge2.IsDeleted);
			AssertEquals(ZGuid.Empty, charge2.JR_AL_APLine);
			AssertEquals(0m, charge2.JR_LocalCostAmt);
			AssertEquals(0m, charge2.JR_OSCostAmt);
		}

		public void TestReverseAutoJobRevenueJournalWhenBothChargeAmtNonZero()
		{
			AssertReverseAutoJobRevenueJournalWhetherChargeIsDeleted(false, false);
		}

		public void TestReverseAutoJobRevenueJournalWhenBothChargeAmtZero()
		{
			AssertReverseAutoJobRevenueJournalWhetherChargeIsDeleted(true, true);
		}

		public void TestReverseAutoJobRevenueJournalWhenRevenueChargeCostAmtZero()
		{
			AssertReverseAutoJobRevenueJournalWhetherChargeIsDeleted(true, false);
		}

		public void TestReverseAutoJobRevenueJournalWhenCostChargeSellAmtZero()
		{
			AssertReverseAutoJobRevenueJournalWhetherChargeIsDeleted(false, true);
		}

		void AssertReverseAutoJobRevenueJournalWhetherChargeIsDeleted(bool isRevenueChargeCostAmtZero, bool isCostChargeSellAmtZero)
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
			charge.JR_OH_SellAccount = TestObjectCreator.NonCurrentBranch.GB_OH_OrgProxy;
			charge.JR_JH_InternalJob = job.PK;
			charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;

			Factory.Save();

			AssertEquals("Precondition: job should have created an auto JRJ and should have 2 charges", 2, job.Charges.Count);

			var charge1 = job.Charges[0];
			if (isRevenueChargeCostAmtZero)
			{
				charge1.JR_OSCostAmt = 0M;
				AssertEquals(0M, charge1.JR_OSCostAmt);
			}
			else
			{
				AssertEquals(100M, charge1.JR_OSCostAmt);
			}
			var charge2 = job.Charges[1];
			if (isCostChargeSellAmtZero)
			{
				charge2.JR_OSSellAmt = 0M;
				AssertEquals(0M, charge2.JR_OSSellAmt);
			}
			else
			{
				AssertEquals(100M, charge2.JR_OSSellAmt);
			}
			var journal = Factory.Load<JobRevenueJournal>(charge1.Revenue.TransactionHeader.PK);

			journal.AH_IsCancelled = true;
			GetTransformer(Factory).Transform(journal);
			Factory.Save();

			AssertEquals("charge1.IsDeleted", isRevenueChargeCostAmtZero, charge1.IsDeleted);
			AssertEquals("charge2.IsDeleted", isCostChargeSellAmtZero, charge2.IsDeleted);
		}

		public void TestReverseAutoJobRevenueJournalWhenChargeIsPosted()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());

			var job = CreateJob("Z00001000", LocalClient, 0M, null, 0M);
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = GlbDepartment.CurrentDepartment.PK;

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 100m, 100m);
			charge.JR_OH_SellAccount = TestObjectCreator.NonCurrentBranch.GB_OH_OrgProxy;
			charge.JR_JH_InternalJob = job.PK;
			charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;

			Factory.Save();

			AssertEquals("Precondition: job should have created an auto JRJ and should have 2 charges", 2, job.Charges.Count);
			Charge charge1 = job.Charges[0];
			Charge charge2 = job.Charges[1];

			var apInvoice = CreateAPInvoice("1234", LocalClient, TestObjectCreator.AUD, 1m, "");
			var apInvoiceLine = CreateAPInvoiceLine(apInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "", 100m, false);
			charge1.ClearCostLink();
			charge1.JR_AL_APLine = apInvoiceLine.PK;

			var arInvoice = CreateARInvoice("1234", LocalClient, TestObjectCreator.AUD, 1m, "");
			var arInvoiceLine = CreateARInvoiceLine(arInvoice, job, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, "", 100m);
			charge2.ClearRevenueLink();
			charge2.JR_AL_ARLine = arInvoiceLine.PK;

			var journal = Factory.Load<JobRevenueJournal>(charge1.Revenue.TransactionHeader.PK);

			journal.AH_IsCancelled = true;
			GetTransformer(Factory).Transform(journal);

			AssertEquals("charge1.IsDeleted", false, charge1.IsDeleted);
			AssertEquals("charge2.IsDeleted", false, charge2.IsDeleted);

			AssertEquals("charge 1 should still be cost posted", true, charge1.IsCostPosted);
			AssertEquals("charge 1 cost shouldn't have changed", 100m, charge1.JR_OSCostAmt);
			AssertEquals("charge 1 should NOT be revenue posted", false, charge1.IsRevenuePosted);
			AssertEquals("charge 1 sell account", Guid.Empty, charge1.JR_OH_SellAccount);
			AssertEquals("charge 1 sell amount", 0m, charge1.JR_LocalSellAmt);

			AssertEquals("charge 2 should still be revenue posted", true, charge2.IsRevenuePosted);
			AssertEquals("charge 2 sell shouldn't have changed", 100m, charge2.JR_OSSellAmt);
			AssertEquals("charge 2 should NOT be cost posted", false, charge2.IsCostPosted);
			AssertEquals("charge 2 cost account", Guid.Empty, charge2.JR_OH_CostAccount);
			AssertEquals("charge 2 cost amount", 0m, charge2.JR_OSCostAmt);
		}

		public void TestReverseAutoJobRevenueJournalWhenRevenueChargeIsPosted()
		{
			//Deleted Object Critical Validation Failure - Charge deleted with posted REV.
			using (var gatewayBillingJob = TestObjectCreator.SetupGatewayLegacyJobAndEnableJRJ())
			{
				var consol = TestObjectCreator.CreateGatewayConsol("AUSYD", "SGSIN", "C0001990", receivingGatewayCompany: GlbCompany.CurrentCompany);

				gatewayBillingJob.JH_ParentID = consol.PK;
				gatewayBillingJob.JH_ParentTableCode = "JK";

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_ActualChargeable = 500;
				shipment1.JS_ActualWeight = 500;
				shipment1.JS_RL_NKDestination = "SGSIN";
				shipment1.JS_RL_NKOrigin = "AUSYD";

				Factory.Save();

				var charge1 = gatewayBillingJob.Charges.AddNew();
				charge1.JR_AC = Env.Registry.FreightChargeCode;
				charge1.JR_OSSellAmt = 100m;
				charge1.JR_OH_SellAccount = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
				charge1.JR_JH_InternalJob = gatewayBillingJob.PK;
				charge1.JR_GB_InternalBranch = charge1.JR_GB;
				charge1.JR_GE_InternalDept = charge1.JR_GE;
				charge1.JR_OSCostAmt = 0M;

				Assert("Precondition", consol.IsGateway());

				Factory.Save();

				AssertEquals("One charge after saving", 1, gatewayBillingJob.Charges.Count);

				var shipmentFromJob = new Job.Loader(shipment1).Load();
				var chargeFromShipment = shipmentFromJob.Charges[0];
				chargeFromShipment.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;

				Factory.Save();

				Assert("Shipment charge is not revenue posted", !chargeFromShipment.IsRevenuePosted);
				Assert("Shipment charge is apportioned", chargeFromShipment.JR_IsApportioned);

				ChargePoster poster = new ChargePoster(Factory);
				poster.Post(chargeFromShipment);

				Factory.Save();
				Assert("Shipment charge is revenue posted", chargeFromShipment.IsRevenuePosted);
				Assert("Shipment charge is apportioned", chargeFromShipment.JR_IsApportioned);

				BusinessObjectFactory testDataFactory = new BusinessObjectFactory();

				ZQuery transactionFilter = new ZQuery(JobChargeSchema.JR_JH, shipmentFromJob.PK);
				var charge = testDataFactory.LoadTop1<JobCharge>(transactionFilter);
				var revenueLine = testDataFactory.Load<AccTransactionLines>(charge.JR_AL_APLine);
				var jrj = testDataFactory.Load<AccTransactionHeader>(revenueLine.AL_AH);

				var journal = testDataFactory.Load<JobRevenueJournal>(jrj.PK);
				journal.AH_TransactionCategory = Core.Constants.TransactionCategory.Codes.AutoJobRevenueJournal;

				var reversingFactory = new ReversingFactory();
				var reversing = reversingFactory.NewReversing(journal);
				reversing.Reverse();
				testDataFactory.Save();

				Assert("Shipment charge has not been deleted", !chargeFromShipment.IsDeleted);
				Assert("Shipment charge is revenue posted", chargeFromShipment.IsRevenuePosted);
				Assert("Shipment charge is not apportioned anymore", !chargeFromShipment.JR_IsApportioned);
				AssertEquals("0 charges after reversing the Job Revenue Journal", 0, gatewayBillingJob.Charges.Count);
			}
		}

		public void TestReverseAutoJobRevenueJournalWhenAPChargeIsApportioned()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C000001");
			var shipment = creator.CreateShipment("S00001001");
			Job job = creator.CreateJob(shipment, false);
			job.LocalChargesPK = LocalClient.PK;
			consol.Shipments.Add(shipment);
			var consolCost = creator.CreateConsolCost(consol, creator.CC1, GlbBranch.CurrentBranch.OrgProxy);
			consolCost.E6_OSCostAmount = 100m;
			consolCost.UpdateApportionmentChargesListing();
			Factory.Save();

			AssertEquals("One Charge", 1, job.Charges.Count);
			var charge1 = job.Charges[0];
			charge1.JR_JH_InternalJob = job.PK;
			charge1.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
			charge1.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			AssertEquals("Two Charge", 2, job.Charges.Count);
			var charge2 = job.Charges[1];
			charge2.JR_OSCostAmt = 0M;
			var journal = Factory.Load<JobRevenueJournal>(charge1.APLine.AL_AH);
			AssertNotNull("Journal", journal);

			journal.AH_IsCancelled = true;
			GetTransformer(Factory).Transform(journal);

			AssertEquals("charge1.IsDeleted", false, charge1.IsDeleted);
			AssertEquals("charge2.IsDeleted", true, charge2.IsDeleted);

			AssertEquals("charge 1 shouldn't be cost posted", false, charge1.IsCostPosted);
			AssertEquals("charge 1 cost account should not be cleared", GlbBranch.CurrentBranch.GB_OH_OrgProxy, charge1.JR_OH_CostAccount);
			AssertEquals("charge 1 cost shouldn't have changed", 100m, charge1.JR_OSCostAmt);
			AssertEquals("charge 1 shouldn't be revenue posted", false, charge1.IsRevenuePosted);
			AssertEquals("charge 1 sell account", LocalClient.PK, charge1.JR_OH_SellAccount);
			AssertEquals("charge 1 sell amount", 100m, charge1.JR_LocalSellAmt);

			AssertEquals("charge 1 Internal Job should be cleared to prevent creating JRJ on saving", ZGuid.Empty, charge1.JR_JH_InternalJob);
			AssertEquals("charge 1 Internal Branch should be cleared to prevent creating JRJ on saving", ZGuid.Empty, charge1.JR_GB_InternalBranch);
			AssertEquals("charge 1 Internal Department should be cleared to prevent creating JRJ on saving", ZGuid.Empty, charge1.JR_GE_InternalDept);
		}

		public void TestReverseAutoJobRevenueJournalClearsE6_AH_APInvoice()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C000001");
			var shipment = creator.CreateShipment("S00001001", consol);
			var job = creator.CreateJob(shipment, false, localClientOrg: LocalClient);
			var consolCost = creator.CreateConsolCost(consol, creator.CC1, GlbBranch.CurrentBranch.OrgProxy, 100m, true);
			Factory.Save();

			AssertEquals("One Charge", 1, job.Charges.Count);
			var charge = job.Charges[0];
			charge.JR_JH_InternalJob = job.PK;
			charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
			charge.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			AssertEquals("Two Charges", 2, job.Charges.Count);
			var journal = Factory.Load<JobRevenueJournal>(charge.APLine.AL_AH);
			AssertNotNull("Journal", journal);
			AssertEquals(journal.PK, consolCost.E6_AH_APInvoice);

			journal.AH_IsCancelled = true;
			GetTransformer(Factory).Transform(journal);
			AssertEquals(ZGuid.Empty, consolCost.E6_AH_APInvoice);
		}

		public void TestReverseAutoJobRevenueJournalReassignsE6_AH_APInvoice()
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly(GlbCompany.CurrentCompany.PK.ToGuid());
			var creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C000001");
			Job[] jobs = new Job[2];
			for (int i = 0; i < jobs.Length; i++)
			{
				var shipment = creator.CreateShipment("S0000100" + i, consol);
				jobs[i] = creator.CreateJob(shipment, false, localClientOrg: LocalClient);
			}
			var consolCost = creator.CreateConsolCost(consol, creator.CC1, GlbBranch.CurrentBranch.OrgProxy, 100m, true, AllocationMethod.Shipment);
			Factory.Save();

			for (int i = 0; i < jobs.Length; i++)
			{
				AssertEquals("One Charge " + i, 1, jobs[i].Charges.Count);
				var charge = jobs[i].Charges[0];
				charge.JR_JH_InternalJob = jobs[i].PK;
				charge.JR_GB_InternalBranch = TestObjectCreator.NonCurrentBranch.PK;
				charge.JR_GE_InternalDept = GlbDepartment.CurrentDepartment.PK;
			}
			Factory.Save();

			JobRevenueJournal journalToReverse = null;
			var newInvoicePK = ZGuid.Empty;
			for (int i = 0; i < jobs.Length; i++)
			{
				AssertEquals("Two Charges " + i, 2, jobs[i].Charges.Count);
				var journal = Factory.Load<JobRevenueJournal>(jobs[i].Charges[0].APLine.AL_AH);
				AssertNotNull("Journal " + i, journal);
				if (journal.PK == consolCost.E6_AH_APInvoice)
				{
					journalToReverse = journal;
				}
				else
				{
					newInvoicePK = journal.PK;
				}
			}
			AssertNotNull("Reverse the currently assigned journal", journalToReverse);
			Assert("The other journal to replace E6_AH_APInvoice", !newInvoicePK.IsEmpty);

			journalToReverse.AH_IsCancelled = true;
			GetTransformer(Factory).Transform(journalToReverse);
			AssertEquals("Reassigned", newInvoicePK, consolCost.E6_AH_APInvoice);
		}

		#endregion

		public void TestTransformOfAPInvoiceWithInvalidRelatedChargeDebtor()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			TestObjectCreator creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S00001001");
			Job job = creator.CreateJob(shipment, false);

			APInvoice invoice = Factory.New<APInvoice>();
			InvoicingLineBase line = creator.CreateAPInvoiceLine(invoice, job, creator.CC1, creator.AUD, 1.0m, "Desc", 100.0m);
			Charge charge = creator.CreateCharge(line, job, creator.CC1, creator.AUD);
			charge.JR_OH_SellAccount = creator.ABIGAS.PK;
			creator.ABIGAS.CompanyData.OB_IsDebtor = false;

			invoice.GenerateReverseTransaction(true);
			InvoicingBase reverseInvoice = invoice.ReverseInvoice;
			invoice.SetCancellationFlag(true);
			reverseInvoice.SetCancellationFlag(true);

			ReverseInvoicingTransformer transformer = GetTransformer(Factory) as ReverseInvoicingTransformer;
			AssertNotNull("Transformer", transformer);

			transformer.Transform(invoice);

			AssertEquals("charge JR_OH_SellAccount", creator.ABIGAS.PK, charge.JR_OH_SellAccount);
			Assert(!charge.IsDebtorValidToCreateWIPWhenWIPMustHaveDebtor);
		}

		public void TestTransformResetsProfiShareInfoOfARInvoice()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S00001001");
			Job job = creator.CreateJob(shipment, false);

			ARInvoice invoice = Factory.New<ARInvoice>();
			InvoicingLineBase line = creator.CreateARInvoiceLine(invoice, job, creator.CC1, creator.AUD, 1.0m, "Desc", 100.0m);
			Charge charge = creator.CreateCharge(line, job, creator.CC1, creator.AUD);
			InvoicingLineBase line2 = creator.CreateARInvoiceLine(invoice, job, creator.CC5, creator.AUD, 1.0m, "Desc", 200.0m);
			Charge charge2 = creator.CreateCharge(line2, job, creator.CC5, creator.AUD);
			job.JH_IsProfitSharePosted = true;
			job.JH_ProfitShareInvoice = invoice.PK;

			AssertEquals("JH_IsProfitSharePosted should be true", true, job.JH_IsProfitSharePosted);
			AssertEquals("JH_ProfitShareInvoice Should be Valid", true, job.JH_ProfitShareInvoice.IsValid);

			ReverseInvoicingTransformer transformer = GetTransformer(Factory) as ReverseInvoicingTransformer;
			AssertNotNull("Transformer", transformer);

			transformer.Transform(invoice);

			AssertEquals("JH_IsProfitSharePosted should be false", false, job.JH_IsProfitSharePosted);
			AssertEquals("JH_ProfitShareInvoice Should be Empty", true, job.JH_ProfitShareInvoice.IsEmpty);
		}

		[NUnit.Framework.TestDate(2020, 12, 12)]
		public void TestTransformOfAPInvoiceFromConsolCostWillResetConsolCostDateTime()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);

			var shipment = creator.CreateShipment("S00001001");
			var job = creator.CreateJob(shipment, false);
			job.JH_OA_AgentCollectAddr = Creditor1.MainAddress.PK;

			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C000001");
			consol.Shipments.Add(shipment);

			var consolCost = creator.CreateConsolCost(consol, creator.CC1, creator.ABIGAS);
			consolCost.E6_OSCostAmount = 100m;
			consolCost.UpdateApportionmentChargesListing();
			Factory.Save();

			consolCost.E6_InvoiceNum = "123456789";
			consolCost.E6_OH_Creditor = Creditor1.PK;
			consolCost.E6_DocumentReceivedDate = ZDateTime.Today;
			consolCost.E6_InvoiceDate = ZDateTime.Today;
			consolCost.E6_PaymentDate = ZDateTime.Today;
			Factory.Save();

			CombineAssertions("PreCondition" , () => {
				AssertEquals(nameof(consolCost.E6_DocumentReceivedDate), false, consolCost.E6_DocumentReceivedDate.IsEmpty);
				AssertEquals(nameof(consolCost.E6_InvoiceDate), false, consolCost.E6_InvoiceDate.IsEmpty);
				AssertEquals(nameof(consolCost.E6_PaymentDate), false, consolCost.E6_PaymentDate.IsEmpty);
			});

			TestObjectCreator.CreateTestPeriodsForEntireYear(2020);
			var consolPostingProcessor = new JobConsolCostOnlyPoster(consol);
			var notifications = new NotificationBuffer();
			consolPostingProcessor.Process(notifications);
			AssertEquals(false, consolCost.E6_AH_APInvoice.IsEmpty);

			var invoiceinLocalCache = Factory.LoadTop1<APInvoice>(new ZQuery(AccTransactionHeaderSchema.PK, consolCost.E6_AH_APInvoice) { FetchOnlyFromLocalCache = true });
			AssertNotNull(invoiceinLocalCache);
			Factory.Save(); //loading invoice from cache and doing save here, because the consolPostingProcessor Process method is now refactored to use main Factory, and that method will not save invoice in DB.

			var newFactory = new BusinessObjectFactory();
			var invoice = newFactory.Load<APInvoice>(consolCost.E6_AH_APInvoice);
			AssertNotNull(invoice);

			ReversingFactory reversingFactory = new ReversingFactory();
			var reversing = reversingFactory.NewReversing(invoice);
			reversing.Reverse();

			ReverseInvoicingTransformer transformer = GetTransformer(newFactory) as ReverseInvoicingTransformer;
			AssertNotNull("Transformer", transformer);
			transformer.Transform(invoice);

			consolCost = newFactory.Load<JobConsolCost>(consolCost.PK);

			AssertEquals("Doc Rev Date should be reset", true, consolCost.E6_DocumentReceivedDate.IsEmpty);
			AssertEquals("Invoice Date should be reset", true, consolCost.E6_InvoiceDate.IsEmpty);
			AssertEquals("Payment Date should be reset", true, consolCost.E6_PaymentDate.IsEmpty);
		}

		public void TestTransformOfAPInvoiceFromConsolCostWithUnusedApprotionmentCharge()
		{
			TestObjectCreator creator = new TestObjectCreator(Factory);
			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C000001");
			var shipment = creator.CreateShipment("S00001001");
			Job job = creator.CreateJob(shipment, false);
			consol.Shipments.Add(shipment);
			var consolCost = creator.CreateConsolCost(consol, creator.CC1, creator.ABIGAS);
			consolCost.E6_OSCostAmount = 100m;
			consolCost.UpdateApportionmentChargesListing();

			Factory.Save();

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.ConsolCosting.ConsolCosts.Add(consolCost);

			invoice.ImportAllApportionmentsFromCosting();
			consolCost.ApportionmentCharges[0].ReverseAccrual(ZDateTime.Now);
			consolCost.ApportionmentCharges[0].JR_AL_APLine = invoice.Lines[0].PK;
			consolCost.E6_AH_APInvoice = invoice.PK;

			Factory.Save();

			var newShipment = creator.CreateShipment("S00001002");
			Job newJob = creator.CreateJob(newShipment, false);
			consol.Shipments.Add(newShipment);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();

			invoice = newFactory.Load<APInvoice>(invoice.PK);

			invoice.GenerateReverseTransaction(true);
			InvoicingBase reverseInvoice = invoice.ReverseInvoice;
			invoice.SetCancellationFlag(true);
			reverseInvoice.SetCancellationFlag(true);

			ReverseInvoicingTransformer transformer = GetTransformer(newFactory) as ReverseInvoicingTransformer;
			AssertNotNull("Transformer", transformer);

			transformer.Transform(invoice);

			consolCost = newFactory.Load<JobConsolCost>(consolCost.PK);
			newJob = newFactory.Load<Job>(newJob.PK);

			newJob.Charges.Load();
			AssertEquals("No new charges added to second Shipment", 0, newJob.Charges.Count);
			AssertEquals("Apportionment should containg only original Charge for first Shipment", 1, consolCost.ApportionmentCharges.Count);
		}

		public void TestReversingAPTransactionRestoresCostReferenceInJobCharge()
		{
			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

			TestObjectCreator creator = new TestObjectCreator(Factory);
			var shipment = creator.CreateShipment("S00001001");
			Job job = creator.CreateJob(shipment, false);

			APInvoice invoice = Factory.New<APInvoice>();
			InvoicingLineBase line = creator.CreateAPInvoiceLine(invoice, job, creator.CC1, creator.AUD, 1.0m, "Desc", 100.0m);
			Charge charge = creator.CreateCharge(line, job, creator.CC1, creator.AUD);
			charge.JR_CostReference = "Supp. TB 001";
			line.AppendCostReferenceToTheLineDescription(charge);

			InvoicingLineBase line2 = creator.CreateAPInvoiceLine(invoice, job, creator.CC5, creator.AUD, 1.0m, "Desc", 200.0m);
			Charge charge2 = creator.CreateCharge(line2, job, creator.CC5, creator.AUD);
			charge2.JR_CostReference = string.Empty;
			line2.AppendCostReferenceToTheLineDescription(charge2);

			//when an AP Invoice is posted JR_CostReference field gets cleared. Here I'm trying to immitate that situation.
			charge.JR_CostReference = ZString.Empty;
			charge2.JR_CostReference = ZString.Empty;

			//Now reverse operation
			AssertEquals("charge JR_CostReference", ZString.Empty, charge.JR_CostReference);
			AssertEquals("charge2 JR_CostReference", ZString.Empty, charge2.JR_CostReference);

			invoice.GenerateReverseTransaction(true);
			InvoicingBase reverseInvoice = invoice.ReverseInvoice;
			invoice.SetCancellationFlag(true);
			reverseInvoice.SetCancellationFlag(true);

			ReverseInvoicingTransformer transformer = GetTransformer(Factory) as ReverseInvoicingTransformer;
			AssertNotNull("Transformer", transformer);

			transformer.Transform(invoice);

			AssertEquals("charge JR_CostReference", "Supp. TB 001", charge.JR_CostReference);
			AssertEquals("charge2 JR_CostReference", ZString.Empty, charge2.JR_CostReference);
		}

		#region Implementation

		JobConsolCost AddNewConsolCost(ForwardingConsol consol, decimal amount = 100m, InvoicingBase invoice = null, AccChargeCode chargeCode = null, OrgHeader creditor = null, AccTaxRate taxRate = null)
		{
			var result = Factory.New<JobConsolCost>();
			using (result.ReportSettingParentSuspender.GetSuspender())
			{
				result.SetE6_ParentIDAndE6_ParentTableCodeTogether(consol.PK, consol.TablePrefix);
			}
			result.E6_GC = GlbCompany.CurrentCompany.PK;
			result.E6_AC_ChargeCode = chargeCode != null ? chargeCode.PK : CC3.PK;
			result.E6_RX_NKCurrency = AUD.Code;
			result.E6_ExchangeRate = 1m;
			result.E6_OSCostAmount = amount;
			result.E6_LocalCostAmount = amount;
			result.E6_ApportionmentMethod = "SHP";
			result.E6_AH_APInvoice = invoice != null ? invoice.PK : ZGuid.Empty;
			result.E6_InvoiceNum = invoice != null && !invoice.AH_TransactionNum.IsEmpty ? invoice.AH_TransactionNum : new ZString("1234");
			result.E6_InvoiceDate = invoice != null ? invoice.AH_InvoiceDate : ZDateTime.Now;
			result.E6_OH_Creditor = creditor != null ? creditor.PK : Creditor1.PK;
			result.E6_AT_TaxRate = taxRate != null ? taxRate.PK : ZGuid.Empty;
			result.E6_CostReference = invoice != null ? invoice.AH_ChequeOrReference : ZString.Empty;
			return result;
		}

		protected override BaseIntegrationTransformer GetTransformer(BusinessObjectFactory factory)
		{
			return new ReverseInvoicingTransformer(factory);
		}

		void ReverseInvoice(InvoicingBase invoice, bool isAR = false)
		{
			invoice.AH_IsCancelled = true;
			((IMatching)invoice).CurrentMatchGroup.AddNew().AP_AH = invoice.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(invoice);
			if (!isAR)
			{
				invoice.GenerateReverseTransaction(false);
				IsCancellReverseInvoice(invoice);
			}
			GetTransformer(Factory).Transform(invoice);
		}

		protected virtual void IsCancellReverseInvoice(InvoicingBase invoicingBase)
		{
			invoicingBase.ReverseInvoice.AH_TransactionNum = "TEST_TRANSACTIONNUM_REVERSE";
			invoicingBase.ReverseInvoice.AH_IsCancelled = true;
			((IMatching)invoicingBase).CurrentMatchGroup.AddNew().AP_AH = invoicingBase.ReverseInvoice.PK;
			TestObjectCreator.SetupMatchLinkMatchDate(invoicingBase);
		}

		#endregion
	}

	public class ReverseInvoicingTransformerTestForUATransactions : ReverseInvoicingTransformerTest
	{
		#region Implementation

		protected override Type GetInvoiceType()
		{
			return typeof(UAInvoice);
		}

		protected override Type GetCreditNoteType()
		{
			return typeof(UACreditNote);
		}

		protected override void IsCancellReverseInvoice(InvoicingBase invoicingBase)
		{
		}

		#endregion
	}
}
