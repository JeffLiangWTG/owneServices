using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public abstract class APApportionementLineTest : TestCaseWithFactory
	{
		public void TestDontRaiseAppLineModWhenSettingSameValueAsBase()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();

			ForwardingConsol consol = testDataFactory.NewWithValidTestData<ForwardingConsol>();
			TestObjectCreator testDataCreator = new TestObjectCreator(testDataFactory);
			ForwardingShipment shipment1 = testDataCreator.CreateShipment("S00001001");
			Job job1 = testDataCreator.CreateJob(shipment1);
			testDataFactory.Save();

			InvoicingBase invoice = GetInvoicingBase(Factory);
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			invoice.Lines.ApportionedInvoiceLineModified += new ApportionedInvoiceLineModifiedEventHandler(APInvoiceLineTest_ApportionedInvoiceLineModified);
			line.AL_JH = job1.PK;
			line.AL_JH = line.AL_JH;
			Assert("Should not have raised apportioned line modified", !AppLineModifiedRaised);
		}

		bool AppLineModifiedRaised;

		void APInvoiceLineTest_ApportionedInvoiceLineModified(InvoicingLineBase sender, EventArgs e)
		{
			AppLineModifiedRaised = true;
		}

		public void TestCantModifyLinesFromApportionments()
		{
			var testDataFactory = new BusinessObjectFactory();

			var consol = testDataFactory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			testDataFactory.Save();

			var objectCreator = new TestObjectCreator(Factory);

			var invoice = GetInvoicingBase(Factory);

			invoice.AH_OH = objectCreator.AALSHI.PK;

			try
			{
				var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = objectCreator.CC1.PK;
				cost.E6_AT_TaxRate = objectCreator.GST1.PK;
				cost.E6_OSCostAmount = 20m;
				cost.E6_OSGSTAmount_Calc = 2m;
				cost.E6_ApportionmentMethod = "SHP";
				cost.IsFinal = true;
				cost.SetIsUsedForApportionment();

				invoice.ImportSingleCost(cost, (InvoicingLineBase)invoice.Lines.AddNew());

				AssertEquals("Should have imported 2 lines", 2, invoice.Lines.Count);

				var line1 = invoice.Lines[0];
				AssertEquals("Should have imported first line correctly", 10.00m, line1.AL_OSExTaxAmount);

				var line2 = invoice.Lines[1];
				AssertEquals("Should have imported first line correctly", 10.00m, line2.AL_OSExTaxAmount);

				line1.AL_OSExTaxAmount = 200.00m;
				AssertEquals("Shouldn't have changed line", 10.00m, line1.AL_OSExTaxAmount);

				line1.GenericCharge = objectCreator.CC2.PK;
				AssertEquals("Should not have changed line - charge code", objectCreator.CC1.PK, line1.GenericCharge);

				var jobFK = line1.Job.JH_ParentID;
				line1.AL_JH = ZGuid.Empty;
				AssertNotNull("Job should not be null", line1.Job);
				AssertEquals("Should not have changed line - job", jobFK, line1.Job.JH_ParentID);

				line1.AL_GB = ZGuid.Empty;
				AssertEquals("Should not have changed line - branch", cost.ApportionmentCharges[0].JR_GB, line1.AL_GB);

				line1.AL_GE = ZGuid.Empty;
				AssertEquals("Should not have changed line - department", cost.ApportionmentCharges[0].JR_GE, line1.AL_GE);

				line1.AL_OSTaxAmount = 100m;
				AssertEquals("Should not have changed line - gst amount", 1.00m, line1.AL_OSTaxAmount);

				line1.AL_AT = ZGuid.Empty;
				AssertEquals("Shouldn't have changed line - tax rate", objectCreator.GST1.PK, line1.AL_AT);

				line1.AL_IsFinalCharge = false;
				AssertEquals("Should not have changed line - is final", true, line1.AL_IsFinalCharge);
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		[ExpectNoExceptions]
		public void TestCantModifyLinesFromApportionments_WhenConsolCostNotAlreadyCreated()
		{
			//Critical Validation Failure: Transaction Line OS Amount Total does not match the Transaction Header OS Amount. Transaction Line OS Amount Total is XX but Header OS Amount is YY.
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			var job1 = objectCreator.CreateJob(shipment1);

			Charge charge = objectCreator.CreateCharge(job1, objectCreator.CC2, "Charge 2", objectCreator.AUD, 1m, objectCreator.CreateOrgHeader("Two", true, false), objectCreator.AUD, 1m, objectCreator.CreateOrgHeader("AgentTwo", false, true));
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;

			InvoicingBase invoice = GetInvoicingBase(Factory);
			invoice.AH_OH = objectCreator.AALSHI.PK;
			invoice.AH_TransactionNum = "00001000";
			try
			{
				JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = objectCreator.CC1.PK;
				cost.E6_AT_TaxRate = objectCreator.GST1.PK;
				cost.E6_OSCostAmount = 20m;
				cost.IsFinal = true;
				cost.SetIsUsedForApportionment();
				cost.E6_InvoiceDate = charge.JR_APInvoiceDate;
				cost.E6_PaymentDate = charge.JR_PaymentDate;

				invoice.AH_InvoiceDate = charge.JR_APInvoiceDate;

				invoice.ImportSingleCost(cost, (InvoicingLineBase)invoice.Lines.AddNew());
				invoice.SubmittedFromInvoicingForm = true;
				invoice.SetContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer);

				AssertEquals("Should have imported 1 lines", 1, invoice.Lines.Count);

				InvoicingLineBase line1 = invoice.Lines[0];
				AssertEquals("Should have imported first line correctly", 20.00m, line1.AL_OSExTaxAmount);

				line1.AL_LocalExTaxAmount = 200.00m;

				Factory.Save();
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		[ExpectNoExceptions]
		public void TestCantModifyLinesFromApportionments_WithDiffExchangeRate_WhenConsolCostNotAlreadyCreated()
		{
			//Critical Validation Failure: Cost, Revenue or Unapproved Cost transaction line local amount that does not equal related job charge local amount.
			TestObjectCreator objectCreator = new TestObjectCreator(Factory);

			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			var job1 = objectCreator.CreateJob(shipment1);

			Charge charge = objectCreator.CreateCharge(job1, objectCreator.CC2, "Charge 2", objectCreator.AUD, 1m, objectCreator.CreateOrgHeader("Two", true, false), objectCreator.AUD, 1m, objectCreator.CreateOrgHeader("AgentTwo", false, true));
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;

			InvoicingBase invoice = GetInvoicingBase(Factory);
			invoice.AH_OH = objectCreator.AALSHI.PK;
			try
			{
				JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = objectCreator.CC1.PK;
				cost.E6_AT_TaxRate = objectCreator.GST1.PK;
				cost.E6_OSCostAmount = 20m;
				cost.IsFinal = true;
				cost.SetIsUsedForApportionment();
				cost.E6_RX_NKCurrency = "USD";
				cost.E6_ExchangeRate = 0.5;
				cost.E6_InvoiceDate = charge.JR_APInvoiceDate;
				cost.E6_PaymentDate = charge.JR_PaymentDate;

				invoice.AH_InvoiceDate = charge.JR_APInvoiceDate;
				invoice.ImportSingleCost(cost, (InvoicingLineBase)invoice.Lines.AddNew());
				invoice.SubmittedFromInvoicingForm = true;
				invoice.SetContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer);

				AssertEquals("Should have imported 1 lines", 1, invoice.Lines.Count);

				InvoicingLineBase line1 = invoice.Lines[0];
				AssertEquals("Should have imported first line correctly", 20.00m, line1.AL_OSExTaxAmount);

				line1.AL_LocalExTaxAmount = 200.00m;

				Factory.Save();
			}
			finally
			{
				invoice.ClearApportionmentJobMutexes();
			}
		}

		[ExpectNoExceptions]
		[DisableZeroExchangeRateOverriding]
		public void TestCantModifyLinesFromApportionments_WithDiffExchangeRate_WhenConsolCostAlreadyCreated()
		{
			//Critical Validation Failure: Local cost amount is not equal to the sum of the apportionment's local cost amount.
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			ForwardingConsol consol = testDataFactory.NewWithValidTestData<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			TestObjectCreator objectCreator = new TestObjectCreator(testDataFactory);

			var job1 = objectCreator.CreateJob(shipment1);

			Charge charge = objectCreator.CreateCharge(job1, objectCreator.CC2, "Charge 2", objectCreator.AUD, 1m, objectCreator.CreateOrgHeader("Two", true, false), objectCreator.AUD, 1m, objectCreator.CreateOrgHeader("AgentTwo", false, true));
			charge.JR_APInvoiceNum = "5";
			charge.JR_APInvoiceDate = ZDateTime.Now.AddDays(1);
			charge.JR_PaymentDate = ZDateTime.Now.AddDays(2);
			charge.JR_PaymentType = ZArchitecture.Core.ReceiptTypes.Cheque;

			testDataFactory.Save();

			InvoicingBase invoice = null;
			ApportionmentListing apps = new ApportionmentListing(testDataFactory, consol);

			try
			{
				JobConsolCost cost = apps.CostsCollection.TryAddNew();
				cost.E6_InvoiceNum = "000001";
				cost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				cost.E6_AC_ChargeCode = objectCreator.CC1.PK;
				cost.E6_OSCostAmount = 20m;
				cost.IsFinal = true;
				cost.SetIsUsedForApportionment();
				cost.E6_RX_NKCurrency = "USD";
				cost.E6_ExchangeRate = 0.5;
				cost.E6_InvoiceDate = charge.JR_APInvoiceDate;
				cost.E6_PaymentDate = charge.JR_PaymentDate;

				testDataFactory.Save();

				invoice = GetInvoicingBase(Factory);
				invoice.AH_OH = objectCreator.AALSHI.PK;
				invoice.AH_InvoiceDate = charge.JR_APInvoiceDate;

				cost.E6_OH_Creditor = invoice.AH_OH;
				cost.E6_ExchangeRate = 0.5;
				cost.E6_AH_APInvoice = invoice.PK;

				invoice.ConsolCosting.ConsolCosts.Add(cost);
				invoice.ImportAllApportionmentsFromCosting();
				invoice.SubmittedFromInvoicingForm = true;
				invoice.SetContext(BusinessContext.ImportMultipleInvoicesWithSameTransformer);

				AssertEquals("Should have imported 1 lines", 1, invoice.Lines.Count);

				InvoicingLineBase line1 = invoice.Lines[0];
				AssertEquals("Should have imported first line correctly", 20.00m, line1.AL_OSExTaxAmount);

				line1.AL_LocalExTaxAmount = 200.00m;

				Factory.Save();
			}
			finally
			{
				apps.ReleaseMutexes();
				invoice?.ClearApportionmentJobMutexes();
			}
		}

		public void TestImportApportionmentSplitChargeWithDifferentExRate()
		{
			BusinessObjectFactory testDataFactory = new BusinessObjectFactory();
			TestObjectCreator testDataCreator = new TestObjectCreator(testDataFactory);
			AccTaxRate gSTRate = testDataCreator.CreateTaxRate("GST10", "GST 10 percent", 10);
			AccChargeCode chargeCode = testDataCreator.CreateChargeCode("MRG100", "Margin 100", Core.Constants.ChargeType.Margin, 100m, gSTRate, null, "ALL");

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Factory.Save();

			ApportionmentListing costs = new ApportionmentListing(Factory, consol);

			try
			{
				JobConsolCost newCost = costs.CostsCollection.TryAddNew();
				newCost.E6_AC_ChargeCode = chargeCode.PK;
				newCost.E6_AT_TaxRate = gSTRate.PK;
				newCost.E6_OSCostAmount = 300m;
				newCost.E6_ApportionmentMethod = "SHP";

				testDataFactory.Save();

				OrgHeader localClient = ObjectCreator.CreateOrgHeader("ZUB", true, true, true, false, false, false);
				InvoicingBase invoice = GetInvoicingBase(Factory);

				InvoicingLineBase line1 = (InvoicingLineBase)invoice.Lines.AddNew();
				invoice.AH_RX_NKTransactionCurrency = ObjectCreator.USD.RX_Code;
				invoice.AH_ExchangeRate = 0.6m;
				line1.AL_RX_NKTransactionCurrency = invoice.AH_RX_NKTransactionCurrency;
				line1.AL_ExchangeRate = invoice.AH_ExchangeRate;

				JobConsolCost cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
				cost.E6_AC_ChargeCode = chargeCode.PK;
				cost.E6_OSCostAmount = 300m;
				cost.E6_AT_TaxRate = gSTRate.PK;
				cost.E6_ApportionmentMethod = "SHP";

				invoice.ImportSingleCost(cost, line1);

				AssertNotNull("Line should have jobheader set", line1.Job);
				AssertEquals("Line job should match split charge being imported", shipment.PK, line1.Job.JH_ParentID);

				AssertNotNull("Line should have charge code set", line1.ChargeCode);
				AssertEquals("Line Charge Code should be set", chargeCode.PK, line1.AL_AC);

				AssertEquals("Line should have amount set", 300.00m, line1.AL_OSExTaxAmount);
				AssertEquals("Line should have GST amount set", 30.00m, line1.AL_OSTaxAmount);

				AssertEquals("Local amount conversion should be done using exrate of invoice for local ex tax", 500.00m, line1.AL_LocalExTaxAmount);
				AssertEquals("Local amount conversion should be done using exrate of invoice for local tax", 50.00m, line1.AL_LocalTaxAmount);

				AssertEquals("Line overseas total", 330.00m, line1.AL_OverseasTotal);
				AssertEquals("Line Local Total", 550.00m, line1.AL_LocalTotalAmount);

				AssertEquals("Line should have GST rate from original apportionment", gSTRate.PK, line1.AL_AT);

				AssertNotNull("Line should have apportionment property set", line1.ApportionmentChargeImportedFrom);
				AssertEquals("Apportionment Split Charge should be set to correct split charge line", cost.ApportionmentCharges[0].PK, line1.ApportionmentChargeImportedFrom.PK);

				if (invoice is APInvoice)
				{
					AssertEquals("Line should have is final set to true", line1.ApportionmentChargeImportedFrom.IsFinal, line1.AL_IsFinalCharge);
					Assert("Is Final should NOT be read only", !line1.AL_IsFinalChargeInfo.ReadOnly);
					Assert("Is Final should not have any error", !line1.AL_IsFinalChargeInfo.HasErrors());
				}
			}
			finally
			{
				costs.ReleaseMutexes();
			}
		}

		public void TestLoadApportionSplitChargeFromSavedLine()
		{
			var factory = new BusinessObjectFactory();
			var creator = new TestObjectCreator(factory);
			var chargeCode = creator.CC1;
			var consol = factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "SGSIN";
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();

			factory.Save();
			factory = new BusinessObjectFactory();

			var invoice = GetInvoicingBase(factory);
			invoice.SubmittedFromInvoicingForm = true;

			var cost = invoice.ConsolCosting.ConsolCosts.TryAddNewForConsol_ForTestOnly(consol);
			cost.E6_AC_ChargeCode = creator.CC1.PK;
			cost.E6_OSCostAmount = 100m;
			cost.E6_ApportionmentMethod = "SHP";
			cost.SetIsUsedForApportionment();

			invoice.ImportSingleCost(cost, null);

			var line1 = invoice.Lines[0];
			var line2 = invoice.Lines[1];

			factory.Save();

			factory = new BusinessObjectFactory();
			invoice = factory.Load<InvoicingBase>(invoice.PK);
			line1 = (InvoicingLineBase)invoice.Lines.FindByPK(line1.PK);
			line2 = (InvoicingLineBase)invoice.Lines.FindByPK(line2.PK);

			AssertEquals("Should find consol number on loading", consol.JK_UniqueConsignRef, line1.ConsolIDFromApportionedCharge);
			AssertEquals("Should find consol number on loading", consol.JK_UniqueConsignRef, line2.ConsolIDFromApportionedCharge);
		}

		#region Auto Tick Final Flag

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirement()
		{
			var job = SetupChargesForAutoTickFinalFlag();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line2", 0m);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			line.AL_LocalExTaxAmount = 210M;
			AssertEquals("The cost variance requires 1st level approval, 210 - 100 is above 100.", ZBool.False, line.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 100M;
			AssertEquals("The cost variance requires 2nd level approval, 100 - 200 exceeds negative 50.", ZBool.False, line2.AL_IsFinalCharge);

			line.AL_LocalExTaxAmount = 180M;
			AssertEquals("The cost variance requires None approval, 180 - 100 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 280M;
			AssertEquals("The cost variance requires None approval, 280 - 200 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.Job, false, invoice);

			line.AL_LocalExTaxAmount = 170M;
			line2.AL_LocalExTaxAmount = 290M;
			AssertEquals("The cost variance requires 2nd level approval, (170 - 100) + (290 - 200) = 160 is above 150.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, (170 - 100) + (290 - 200) = 160 is above 150.", ZBool.False, line2.AL_IsFinalCharge);

			line.AL_LocalExTaxAmount = 220M;
			line2.AL_LocalExTaxAmount = 90M;
			AssertEquals("The cost variance requires None approval, (220 - 100) + (90 - 200) = 10 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires None approval, (220 - 100) + (90 - 200) = 10 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);
		}

		public void TestAutoTickFinalFlag_CostVarianceNoApprovalRequired()
		{
			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJob);

			var shipment = ObjectCreator.CreateShipment("S00000001", true);
			var job = ObjectCreator.CreateJob(shipment, false);
			var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "Desc", ObjectCreator.USD, 100m, null, ObjectCreator.USD, 0m, ObjectCreator.LocalClient);
			Factory.Save();

			var invoice = ObjectCreator.CreateAPInvoice<APInvoice>("INV001", ObjectCreator.USD, 0.2M, 100M, 100M, 0M, 500M, 500M, 0M, ObjectCreator.Creditor1);
			var invoiceLine = (InvoicingLineBase)invoice.Lines.AddNew();
			invoiceLine.AL_JH = job.PK;
			invoice.ImportJobChargesIntoInvoice(new[] { charge }, invoiceLine);
			invoiceLine.AL_OSExTaxAmount = 100M;
			invoiceLine.AL_LocalExTaxAmount = 500M;

			Assert(AccountingConfigurationRegistry.Instance.CostVarianceNoApprovalRequired.Value);
			Assert(invoiceLine.IsPopulatedFromImportedJobCharge);
			AssertNotEquals(invoiceLine.AL_RX_NKTransactionCurrency, GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency);
			AssertEquals(invoiceLine.AL_RX_NKTransactionCurrency, invoiceLine.OriginalJobCharge.JR_RX_NKCostCurrency);
			AssertEquals(invoiceLine.AL_OSExTaxAmount, invoiceLine.OriginalJobCharge.JR_OSCostAmt);
			AssertEquals("Both ACR and CST amount and currency matches, no need cost variance approval required, although 500 - 100 is above 100.", ZBool.True, invoiceLine.AL_IsFinalCharge);

			invoiceLine.AL_OSExTaxAmount = 200M;
			invoiceLine.AL_LocalExTaxAmount = 800M;
			AssertEquals("The cost variance requires 1st level approval, 800 - 200 is above 100.", ZBool.False, invoiceLine.AL_IsFinalCharge);

			invoiceLine.AL_OSExTaxAmount = 100M;
			invoiceLine.AL_RX_NKTransactionCurrency = "CNY";
			invoiceLine.AL_LocalExTaxAmount = 400M;
			AssertEquals("The cost variance requires 1st level approval, 400 - 200 is above 100.", ZBool.False, invoiceLine.AL_IsFinalCharge);
		}

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirementForJCR()
		{
			var job = ObjectCreator.CreateJob(ObjectCreator.CreateShipment("S00001001"));
			var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "",
				ObjectCreator.AUD, 100M, ObjectCreator.Creditor1,
				ObjectCreator.AUD, 0M, null);
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "",
				ObjectCreator.AUD, 200M, ObjectCreator.Creditor2,
				ObjectCreator.AUD, 0M, null);
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndCreditor);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line2", 0m);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var invoice1 = Factory.New<APInvoice>();
			invoice1.AH_OH = ObjectCreator.Creditor2.PK;

			var line3 = ObjectCreator.CreateAPInvoiceLine(invoice1, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line3", 0m);
			line3.AL_GB = GlbBranch.CurrentBranch.PK;
			line3.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line4 = ObjectCreator.CreateAPInvoiceLine(invoice1, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line4", 0m);
			line4.AL_GB = GlbBranch.CurrentBranch.PK;
			line4.AL_GE = GlbDepartment.CurrentDepartment.PK;

			line.AL_LocalExTaxAmount = 160M;
			line2.AL_LocalExTaxAmount = 70M;
			line3.AL_LocalExTaxAmount = 50M;
			line4.AL_LocalExTaxAmount = 80M;
			AssertEquals("The cost variance requires 1st level approval, 160 + 70 - 100 is above 100.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 1st level approval, 160 + 70 - 100 is above 100.", ZBool.False, line2.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, 50 + 80 - 200 exceeds negative 50.", ZBool.False, line3.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, 50 + 80 - 200 exceeds negative 50.", ZBool.False, line4.AL_IsFinalCharge);

			line.AL_LocalExTaxAmount = 60M;
			line2.AL_LocalExTaxAmount = 70M;
			line3.AL_LocalExTaxAmount = 130M;
			line4.AL_LocalExTaxAmount = 80M;
			AssertEquals("The cost variance requires 1st level approval, 60 + 70 - 100 is not above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 1st level approval, 60 + 70 - 100 is not above 100.", ZBool.True, line2.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, 130 + 80 - 200 = 10 is NOT above 100.", ZBool.True, line3.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, 130 + 80 - 200 = 10 is NOT above 100.", ZBool.True, line4.AL_IsFinalCharge);
		}

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirement_WithImportedChargeOption()
		{
			var job = SetupChargesForAutoTickFinalFlag();

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC3, "",
				ObjectCreator.AUD, 300M, ObjectCreator.Creditor1,
				ObjectCreator.AUD, 0M, null);
			charge3.JR_GB = GlbBranch.CurrentBranch.PK;
			charge3.JR_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJobChargeCode);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line2", 0m);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line3 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC3, ObjectCreator.AUD, 1m, "line3", 0m);
			line3.AL_GB = GlbBranch.CurrentBranch.PK;
			line3.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line3.OriginalJobCharge = charge3;

			line.AL_LocalExTaxAmount = 210M;
			AssertEquals("The cost variance requires 1st level approval, 210 - 100 is above 100.", ZBool.False, line.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 100M;
			AssertEquals("The cost variance requires 2nd level approval, 100 - 200 exceeds negative 50.", ZBool.False, line2.AL_IsFinalCharge);
			line3.AL_LocalExTaxAmount = 440M;
			AssertEquals("The cost variance requires 2nd level approval, 440 - 300 is above 100.", ZBool.False, line3.AL_IsFinalCharge);

			line.AL_LocalExTaxAmount = 180M;
			AssertEquals("The cost variance requires None approval, 180 - 100 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 280M;
			AssertEquals("The cost variance requires None approval, 280 - 200 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);
			line3.AL_LocalExTaxAmount = 380M;
			AssertEquals("The cost variance requires None approval, 380 - 300 is NOT above 100.", ZBool.True, line3.AL_IsFinalCharge);

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJob, false, invoice);

			line.AL_LocalExTaxAmount = 170M;
			line2.AL_LocalExTaxAmount = 290M;
			line3.AL_LocalExTaxAmount = 240M;
			AssertEquals("The cost variance requires 2nd level approval, (170 - 100) + (290 - 200) = 160 is above 150.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, (170 - 100) + (290 - 200) = 160 is above 150.", ZBool.False, line2.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, 240 - 300 exceeds negative 50.", ZBool.False, line3.AL_IsFinalCharge);

			line.AL_LocalExTaxAmount = 220M;
			line2.AL_LocalExTaxAmount = 90M;
			line3.AL_LocalExTaxAmount = 390M;
			AssertEquals("The cost variance requires None approval, (220 - 100) + (90 - 200) = 10 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires None approval, (220 - 100) + (90 - 200) = 10 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);
			AssertEquals("The cost variance requires None approval, 390 - 300 is NOT above 100.", ZBool.True, line3.AL_IsFinalCharge);
		}

		public void TestAutoTickFinalFlag_CostVarianceAuthorisationRequirementForJCR_WithImportedChargeOption()
		{
			var job = ObjectCreator.CreateJob(ObjectCreator.CreateShipment("S00001001"));
			var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "",
				ObjectCreator.AUD, 100M, ObjectCreator.Creditor1,
				ObjectCreator.AUD, 0M, null);
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "",
				ObjectCreator.AUD, 200M, ObjectCreator.Creditor2,
				ObjectCreator.AUD, 0M, null);
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;

			var charge3 = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "",
				ObjectCreator.AUD, 300M, ObjectCreator.Creditor3,
				ObjectCreator.AUD, 0M, null);
			charge3.JR_GB = GlbBranch.CurrentBranch.PK;
			charge3.JR_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.ImportedChargeOrCreditor);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor3.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line2", 0m);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line3 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line3", 0m);
			line3.AL_GB = GlbBranch.CurrentBranch.PK;
			line3.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line3.OriginalJobCharge = charge3;

			line.AL_LocalExTaxAmount = 410M;
			AssertEquals("The cost variance requires 1st level approval, 410 is above 100.", ZBool.False, line.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 10M;
			AssertEquals("The cost variance requires 1nd level approval, 400 + 10 is above 100.", ZBool.False, line2.AL_IsFinalCharge);
			line3.AL_LocalExTaxAmount = 480M;
			AssertEquals("The cost variance requires 2nd level approval, 480 - 300 is above 150.", ZBool.False, line3.AL_IsFinalCharge);

			line.AL_LocalExTaxAmount = 80M;
			AssertEquals("The cost variance requires None approval, 80 + 10 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 15M;
			AssertEquals("The cost variance requires None approval, 80 + 15 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);
			line3.AL_LocalExTaxAmount = 330M;
			AssertEquals("The cost variance requires None approval, 330 - 300 is NOT above 100.", ZBool.True, line3.AL_IsFinalCharge);
		}

		public void TestAutoTickFinalFlag_TotalAuthorisationRequirement()
		{
			var job = SetupChargesForAutoTickFinalFlag();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode, true);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line2", 0m);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			line.AL_LocalExTaxAmount = 180M;
			AssertEquals("The total cost variance requires 2st level approval, 0 - 200 = -200 is above -100.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The total cost variance requires 2st level approval, 0 - 200 = -200 is above -100.", ZBool.False, line2.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 280M;
			AssertEquals("The total cost variance requires 1st level approval, (180 - 100) + (280 - 200) = 160 is above 150.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The total cost variance requires 1st level approval, (180 - 100) + (280 - 200) = 160 is above 150.", ZBool.False, line2.AL_IsFinalCharge);

			line.AL_LocalExTaxAmount = 150M;
			AssertEquals("The total cost variance requires None approval, (150 - 100) + (280 - 200) = 130 is NOT above 150, 150 - 100 = 50 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertEquals("The total cost variance requires None approval, (150 - 100) + (280 - 200) = 130 is NOT above 150, 280 - 200 = 80 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 250M;
			AssertEquals("The total cost variance requires None approval, (150 - 100) + (280 - 200) = 130 is NOT above 150, 150 - 100 = 50 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertEquals("The total cost variance requires None approval, (150 - 100) + (250 - 200) = 100 is NOT above 150, 250 - 200 = 50 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);

			line.AL_LocalExTaxAmount = 100M;
			AssertEquals("The total cost variance requires None approval, (100 - 100) + (250 - 200) = 50 is NOT above 150, 100 - 100 = 0 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertEquals("The total cost variance requires None approval, (100 - 100) + (250 - 200) = 50 is NOT above 150, 250 - 200 = 50 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 320M;
			AssertEquals("The total cost variance requires None approval, (100 - 100) + (320 - 200) = 120 is NOT above 150, 100 - 100 = 0 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 1st level approval, 320 - 200 = 120 is above 100.", ZBool.False, line2.AL_IsFinalCharge);

			line.AL_LocalExTaxAmount = 210M;
			AssertEquals("The total cost variance requires 1st level approval, (210 - 100) + (320 - 200) = 230 is above 150.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The total cost variance requires 1st level approval, (210 - 100) + (320 - 200) = 230 is above 150.", ZBool.False, line.AL_IsFinalCharge);
			line2.AL_LocalExTaxAmount = 200M;
			AssertEquals("The total cost variance requires 1st level approval, 210 - 100 = 110 is above 100.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The total cost variance requires None approval, (210 - 100) + (200 - 200) = 110 is NOT above 150, 200 - 200 = 0 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);
		}

		public void TestAutoTickFinalFlag_FinalFlagInTheSameJob()
		{
			Job job2 = null;
			var job = SetupChargesForAutoTickFinalFlag(() =>
			{
				job2 = ObjectCreator.CreateJob(ObjectCreator.CreateShipment("S00001002"));
				var charge3 = ObjectCreator.CreateCharge(job2, ObjectCreator.CC3, "",
					ObjectCreator.AUD, 200M, ObjectCreator.Creditor1,
					ObjectCreator.AUD, 0M, null);
				charge3.JR_GB = GlbBranch.CurrentBranch.PK;
				charge3.JR_GE = GlbDepartment.CurrentDepartment.PK;
			});
			AssertNotNull("precondition: job2", job2);

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.Job);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 180M);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line2", 280M);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line3 = ObjectCreator.CreateAPInvoiceLine(invoice, job2, ObjectCreator.CC3, ObjectCreator.AUD, 1m, "line3", 380M);
			line3.AL_GB = GlbBranch.CurrentBranch.PK;
			line3.AL_GE = GlbDepartment.CurrentDepartment.PK;

			AssertEquals("The cost variance requires 2nd level approval, (180 - 100) + (280 - 200) = 160 is above 150.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, (180 - 100) + (280 - 200) = 160 is above 150.", ZBool.False, line2.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, 380 - 200 = 180 is above 150.", ZBool.False, line3.AL_IsFinalCharge);

			line.AL_IsFinalCharge = ZBool.True;
			AssertEquals("Manually changed.", ZBool.True, line.AL_IsFinalCharge);
			AssertEquals("Be changed for it's in the same job.", ZBool.True, line2.AL_IsFinalCharge);
			AssertEquals("Won't be changed for it's NOT in the same job.", ZBool.False, line3.AL_IsFinalCharge);
		}

		public void TestAutoTickFinalFlag_FinalFlagInTheSameJob_WithImportedCharge()
		{
			Job job2 = null;
			var job = SetupChargesForAutoTickFinalFlag(() =>
			{
				job2 = ObjectCreator.CreateJob(ObjectCreator.CreateShipment("S00001002"));
				var charge3 = ObjectCreator.CreateCharge(job2, ObjectCreator.CC3, "",
					ObjectCreator.AUD, 200M, ObjectCreator.Creditor1,
					ObjectCreator.AUD, 0M, null);
				charge3.JR_GB = GlbBranch.CurrentBranch.PK;
				charge3.JR_GE = GlbDepartment.CurrentDepartment.PK;
			});
			AssertNotNull("precondition: job2", job2);

			var charge4 = ObjectCreator.CreateCharge(job, ObjectCreator.CC4, "",
				ObjectCreator.AUD, 300M, ObjectCreator.Creditor1,
				ObjectCreator.AUD, 0M, null);
			charge4.JR_GB = GlbBranch.CurrentBranch.PK;
			charge4.JR_GE = GlbDepartment.CurrentDepartment.PK;
			Factory.Save();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJob);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 180M);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line2", 280M);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line3 = ObjectCreator.CreateAPInvoiceLine(invoice, job2, ObjectCreator.CC3, ObjectCreator.AUD, 1m, "line3", 380M);
			line3.AL_GB = GlbBranch.CurrentBranch.PK;
			line3.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line4 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC4, ObjectCreator.AUD, 1m, "line4", 480M);
			line4.AL_GB = GlbBranch.CurrentBranch.PK;
			line4.AL_GE = GlbDepartment.CurrentDepartment.PK;
			line4.OriginalJobCharge = charge4;

			AssertEquals("The cost variance requires 2nd level approval, (180 - 100) + (280 - 200) = 160 is above 150.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, (180 - 100) + (280 - 200) = 160 is above 150.", ZBool.False, line2.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, 380 - 200 = 180 is above 150.", ZBool.False, line3.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 2nd level approval, 480 - 300 = 180 is above 150.", ZBool.False, line4.AL_IsFinalCharge);

			line.AL_IsFinalCharge = ZBool.True;
			AssertEquals("Manually changed.", ZBool.True, line.AL_IsFinalCharge);
			AssertEquals("Be changed for it's in the same job.", ZBool.True, line2.AL_IsFinalCharge);
			AssertEquals("Won't be changed for it's NOT in the same job.", ZBool.False, line3.AL_IsFinalCharge);
			AssertEquals("Won't be changed for it's imported charge line.", ZBool.False, line4.AL_IsFinalCharge);
		}

		public void TestAutoTickFinalFlag_AllowPayablesInvoiceFinalFlag()
		{
			const string expectedError = @"You do not have appropriate security rights to tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Tick Final Flag on Payables Invoice";

			var job = SetupChargesForAutoTickFinalFlag();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = false;

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line2", 0m);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			line.AL_LocalExTaxAmount = 210M;
			AssertEquals("The cost variance requires 1st level approval, 210 - 100 is above 100.", ZBool.False, line.AL_IsFinalCharge);
			AssertNoErrors("Have the right to tick the flag.", line.AL_IsFinalChargeInfo);
			line2.AL_LocalExTaxAmount = 100M;
			AssertEquals("The cost variance requires 2nd level approval, 100 - 200 exceeds negative 50.", ZBool.False, line2.AL_IsFinalCharge);
			AssertNoErrors("Have the right to tick the flag.", line2.AL_IsFinalChargeInfo);

			line.AL_LocalExTaxAmount = 180M;
			AssertEquals("The cost variance requires None approval, 180 - 100 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertHasError("Don't have the right to tick the flag.", line.AL_IsFinalChargeInfo, expectedError);
			line2.AL_LocalExTaxAmount = 280M;
			AssertEquals("The cost variance requires None approval, 280 - 200 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);
			AssertHasError("Don't have the right to tick the flag.", line2.AL_IsFinalChargeInfo, expectedError);

			Env.Security.AllowPayablesInvoiceFinalFlag.IsAllowed = true;
			invoice.RunPreSaveValidation();
			AssertNoErrors("Have the right to tick the flag.", line.AL_IsFinalChargeInfo);
			AssertNoErrors("Have the right to tick the flag.", line2.AL_IsFinalChargeInfo);
		}

		public void TestAutoTickFinalFlag_AllowUntickAutoTickedFinalFlag()
		{
			const string expectedError = @"You do not have appropriate security rights to un-tick this flag.
Please contact your system administrator for the following security right: Manage > Payables > Payables Transactions > New Transactions > Invoice > Allow Untick Auto-ticked Final Flag";

			var job = SetupChargesForAutoTickFinalFlag();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = false;

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line2", 0m);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			line.AL_LocalExTaxAmount = 180M;
			AssertEquals("The cost variance requires None approval, 180 - 100 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);
			AssertNoErrors(line.AL_IsFinalChargeInfo);
			line2.AL_LocalExTaxAmount = 280M;
			AssertEquals("The cost variance requires None approval, 280 - 200 is NOT above 100.", ZBool.True, line2.AL_IsFinalCharge);
			AssertNoErrors(line.AL_IsFinalChargeInfo);

			line.AL_IsFinalCharge = false;
			AssertHasError("Don't have the right to untick the flag.", line.AL_IsFinalChargeInfo, expectedError);
			line2.AL_IsFinalCharge = false;
			AssertHasError("Don't have the right to untick the flag.", line2.AL_IsFinalChargeInfo, expectedError);

			Env.Security.AllowUntickAutoTickedFinalFlag.IsAllowed = true;
			invoice.RunPreSaveValidation();
			AssertNoErrors("Have the right to untick the flag.", line.AL_IsFinalChargeInfo);
			AssertNoErrors("Have the right to untick the flag.", line2.AL_IsFinalChargeInfo);
		}

		public void TestAutoTickFinalFlag_ChargeCode_Job_Branch_Department()
		{
			var job = SetupChargesForAutoTickFinalFlag();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.JobAndChargeCode);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			line.AL_LocalExTaxAmount = 150M;
			AssertEquals("The cost variance requires None approval, 150 - 100 is NOT above 100.", ZBool.True, line.AL_IsFinalCharge);

			//ChargeCode
			line.AL_AC = ObjectCreator.CC3.PK;
			AssertEquals("The cost variance requires 1st level approval, 150 - 0 is above 100.", ZBool.False, line.AL_IsFinalCharge);

			line.AL_AC = ObjectCreator.CC1.PK;
			AssertEquals("Reset the line status.", ZBool.True, line.AL_IsFinalCharge);

			//Job
			using (var job2 = ObjectCreator.CreateJob(ObjectCreator.CreateShipment("S00001002")))
			{
				line.AL_JH = job2.PK;
				AssertEquals("The cost variance requires 1st level approval, 150 - 0 is above 100.", ZBool.False, line.AL_IsFinalCharge);

				line.AL_JH = job.PK;
				AssertEquals("Reset the line status.", ZBool.True, line.AL_IsFinalCharge);
			}

			//Branch
			line.AL_GB = ObjectCreator.NonCurrentBranch.PK;
			AssertEquals("The cost variance requires 1st level approval, 150 - 0 is above 100.", ZBool.False, line.AL_IsFinalCharge);

			line.AL_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("Reset the line status.", ZBool.True, line.AL_IsFinalCharge);

			//Department
			line.AL_GE = ObjectCreator.NonCurrentDepartment.PK;
			AssertEquals("The cost variance requires 1st level approval, 150 - 0 is above 100.", ZBool.False, line.AL_IsFinalCharge);

			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals("Reset the line status.", ZBool.True, line.AL_IsFinalCharge);
		}

		public void TestAutoTickFinalFlag_ChangeFinalFlagForOriginalGroup()
		{
			var job = SetupChargesForAutoTickFinalFlag();

			TestObjectCreator.SetUpCostVarianceApprovalRegistry(Core.Constants.CostVarianceComparisonOption.Job);

			var invoice = Factory.New<APInvoice>();
			invoice.AH_OH = ObjectCreator.Creditor1.PK;

			var line = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC1, ObjectCreator.AUD, 1m, "line1", 0m);
			line.AL_GB = GlbBranch.CurrentBranch.PK;
			line.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var line2 = ObjectCreator.CreateAPInvoiceLine(invoice, job, ObjectCreator.CC2, ObjectCreator.AUD, 1m, "line2", 0m);
			line2.AL_GB = GlbBranch.CurrentBranch.PK;
			line2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			line.AL_LocalExTaxAmount = 160M;
			line2.AL_LocalExTaxAmount = 260M;
			AssertEquals("The cost variance requires 1st level approval, (160 - 100) + (260 - 200) = 120 is above 100.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("The cost variance requires 1st level approval, (160 - 100) + (260 - 200) = 120 is above 100.", ZBool.False, line2.AL_IsFinalCharge);

			//ChargeCode
			line.AL_AC = ObjectCreator.CC3.PK;
			AssertEquals("Have no effect to the line, since the Variance Comparison Option is Job.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("Have no effect to the line, since the Variance Comparison Option is Job.", ZBool.False, line2.AL_IsFinalCharge);

			//Job
			using (var job2 = ObjectCreator.CreateJob(ObjectCreator.CreateShipment("S00001002")))
			{
				line.AL_JH = job2.PK;
				AssertEquals("The cost variance requires None approval, 160 - 0 is above 60.", ZBool.False, line.AL_IsFinalCharge);
				AssertEquals("Should be recalculated, the cost variance requires None approval, 260 - 200 is NOT about 100.", ZBool.True, line2.AL_IsFinalCharge);

				line.AL_JH = job.PK;
				AssertEquals("Reset the line status.", ZBool.False, line.AL_IsFinalCharge);
				AssertEquals("Reset the line status.", ZBool.False, line2.AL_IsFinalCharge);
			}

			//Branch
			line.AL_GB = ObjectCreator.NonCurrentBranch.PK;
			AssertEquals("The cost variance requires None approval, 160 - 0 is above 60.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("Should be recalculated, the cost variance requires None approval, 260 - 200 is NOT above 60.", ZBool.True, line2.AL_IsFinalCharge);

			line.AL_GB = GlbBranch.CurrentBranch.PK;
			AssertEquals("Reset the line status.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("Reset the line status.", ZBool.False, line2.AL_IsFinalCharge);

			//Department
			line.AL_GE = ObjectCreator.NonCurrentDepartment.PK;
			AssertEquals("The cost variance requires None approval, 160 - 0 is above 60.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("Should be recalculated, the cost variance requires None approval, 260 - 200 is NOT above 60.", ZBool.True, line2.AL_IsFinalCharge);

			line.AL_GE = GlbDepartment.CurrentDepartment.PK;
			AssertEquals("Reset the line status.", ZBool.False, line.AL_IsFinalCharge);
			AssertEquals("Reset the line status.", ZBool.False, line2.AL_IsFinalCharge);

			//Delete
			invoice.Lines.RemoveAndDelete(line);
			AssertEquals("Should be recalculated, the cost variance requires None approval, 260 - 200 is NOT above 60.", ZBool.True, line2.AL_IsFinalCharge);
		}

		Job SetupChargesForAutoTickFinalFlag(Action action = null)
		{
			var job = ObjectCreator.CreateJob(ObjectCreator.CreateShipment("S00001001"));
			var charge = ObjectCreator.CreateCharge(job, ObjectCreator.CC1, "",
				ObjectCreator.AUD, 100M, ObjectCreator.Creditor1,
				ObjectCreator.AUD, 0M, null);
			charge.JR_GB = GlbBranch.CurrentBranch.PK;
			charge.JR_GE = GlbDepartment.CurrentDepartment.PK;

			var charge2 = ObjectCreator.CreateCharge(job, ObjectCreator.CC2, "",
				ObjectCreator.AUD, 200M, ObjectCreator.Creditor1,
				ObjectCreator.AUD, 0M, null);
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;

			if (action != null)
			{
				action();
			}

			Factory.Save();

			return job;
		}

		#endregion

		TestObjectCreator fObjectCreator;
		protected TestObjectCreator ObjectCreator
		{
			get
			{
				if (fObjectCreator == null)
				{
					fObjectCreator = new TestObjectCreator(Factory);
				}
				return fObjectCreator;
			}
		}

		protected abstract InvoicingBase GetInvoicingBase(BusinessObjectFactory factory);
		protected abstract InvoicingLineBase GetInvoicingLineBase(BusinessObjectFactory factory);
	}
}
