namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using CargoWise.Application;
	using CargoWise.Common;
	using CargoWise.ComponentModel;
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Accounting.Business.ConsolCosting;
	using Enterprise.Accounting.Business.JobInvoicing;
	using Enterprise.Accounting.Registry.Business;
	using Enterprise.Core;
	using Enterprise.Environment;
	using Enterprise.Freight.Forwarding.Business;
	using Enterprise.Integration.Accounting;
	using Enterprise.MasterFiles.Business;
	using Enterprise.Registry.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Core.Diagnostics;
	using Enterprise.ZArchitecture.Core.Testing;
	using Enterprise.ZArchitecture.Environment;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;
	using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

	[TestedType(typeof(CASSBilling))]
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	public partial class CASSBillingTest : NonPersistentBusinessObjectTestCase
	{
		public void TestForceRecalculateDataRefreshesTotals()
		{
			int totalCASSCostAdjustedValueChangedCounter = 0;
			int totalCASSCostValueChangedCounter = 0;
			int totalCASSRejectedClaimValueChangedCounter = 0;
			int totalCostDifferenceValueChangedCounter = 0;
			int totalSystemCostAccrualValueChangedCounter = 0;
			int totalNetCASSCostValueChangedCounter = 0;
			int totalHiddenCASSCostAdjustedValueChangedCounter = 0;
			int totalHiddenCASSCostValueChangedCounter = 0;
			int totalHiddenCASSRejectedClaimValueChangedCounter = 0;
			int totalHiddenCostDifferenceValueChangedCounter = 0;
			int totalHiddenSystemCostAccrualValueChangedCounter = 0;
			int totalHiddenNetCASSCostValueChangedCounter = 0;
			int totalAllCASSCostAdjustedValueChangedCounter = 0;
			int totalAllCASSCostValueChangedCounter = 0;
			int totalAllCASSRejectedClaimValueChangedCounter = 0;
			int totalAllCostDifferenceValueChangedCounter = 0;
			int totalAllSystemCostAccrualValueChangedCounter = 0;
			int totalAllNetCASSCostValueChangedCounter = 0;

			TestCASSBilling.TotalCASSCostAdjustedValueInfo.ValueChanged += (sender, e) => totalCASSCostAdjustedValueChangedCounter++;
			TestCASSBilling.TotalCASSCostValueInfo.ValueChanged += (sender, e) => totalCASSCostValueChangedCounter++;
			TestCASSBilling.TotalCASSRejectedClaimValueInfo.ValueChanged += (sender, e) => totalCASSRejectedClaimValueChangedCounter++;
			TestCASSBilling.TotalCostDifferenceValueInfo.ValueChanged += (sender, e) => totalCostDifferenceValueChangedCounter++;
			TestCASSBilling.TotalSystemCostAccrualValueInfo.ValueChanged += (sender, e) => totalSystemCostAccrualValueChangedCounter++;
			TestCASSBilling.TotalNetCASSCostValueInfo.ValueChanged += (sender, e) => totalNetCASSCostValueChangedCounter++;
			TestCASSBilling.TotalHiddenCASSCostAdjustedValueInfo.ValueChanged += (sender, e) => totalHiddenCASSCostAdjustedValueChangedCounter++;
			TestCASSBilling.TotalHiddenCASSCostValueInfo.ValueChanged += (sender, e) => totalHiddenCASSCostValueChangedCounter++;
			TestCASSBilling.TotalHiddenCASSRejectedClaimValueInfo.ValueChanged += (sender, e) => totalHiddenCASSRejectedClaimValueChangedCounter++;
			TestCASSBilling.TotalHiddenCostDifferenceValueInfo.ValueChanged += (sender, e) => totalHiddenCostDifferenceValueChangedCounter++;
			TestCASSBilling.TotalHiddenSystemCostAccrualValueInfo.ValueChanged += (sender, e) => totalHiddenSystemCostAccrualValueChangedCounter++;
			TestCASSBilling.TotalHiddenNetCASSCostValueInfo.ValueChanged += (sender, e) => totalHiddenNetCASSCostValueChangedCounter++;
			TestCASSBilling.TotalAllCASSCostAdjustedValueInfo.ValueChanged += (sender, e) => totalAllCASSCostAdjustedValueChangedCounter++;
			TestCASSBilling.TotalAllCASSCostValueInfo.ValueChanged += (sender, e) => totalAllCASSCostValueChangedCounter++;
			TestCASSBilling.TotalAllCASSRejectedClaimValueInfo.ValueChanged += (sender, e) => totalAllCASSRejectedClaimValueChangedCounter++;
			TestCASSBilling.TotalAllCostDifferenceValueInfo.ValueChanged += (sender, e) => totalAllCostDifferenceValueChangedCounter++;
			TestCASSBilling.TotalAllSystemCostAccrualValueInfo.ValueChanged += (sender, e) => totalAllSystemCostAccrualValueChangedCounter++;
			TestCASSBilling.TotalAllNetCASSCostValueInfo.ValueChanged += (sender, e) => totalAllNetCASSCostValueChangedCounter++;

			TestCASSBilling.ForceRecalculateData();

			AssertEquals(1, totalCASSCostAdjustedValueChangedCounter);
			AssertEquals(1, totalCASSCostValueChangedCounter);
			AssertEquals(1, totalCASSRejectedClaimValueChangedCounter);
			AssertEquals(1, totalCostDifferenceValueChangedCounter);
			AssertEquals(1, totalSystemCostAccrualValueChangedCounter);
			AssertEquals(1, totalNetCASSCostValueChangedCounter);
			AssertEquals(1, totalHiddenCASSCostAdjustedValueChangedCounter);
			AssertEquals(1, totalHiddenCASSCostValueChangedCounter);
			AssertEquals(1, totalHiddenCASSRejectedClaimValueChangedCounter);
			AssertEquals(1, totalHiddenCostDifferenceValueChangedCounter);
			AssertEquals(1, totalHiddenSystemCostAccrualValueChangedCounter);
			AssertEquals(1, totalHiddenNetCASSCostValueChangedCounter);
			AssertEquals(1, totalAllCASSCostAdjustedValueChangedCounter);
			AssertEquals(1, totalAllCASSCostValueChangedCounter);
			AssertEquals(1, totalAllCASSRejectedClaimValueChangedCounter);
			AssertEquals(1, totalAllCostDifferenceValueChangedCounter);
			AssertEquals(1, totalAllSystemCostAccrualValueChangedCounter);
			AssertEquals(1, totalAllNetCASSCostValueChangedCounter);
		}

		public void TestCostHeaderSettingRefreshesRelatedFields()
		{
			int hotFileNameValueChangedCounter = 0;
			int billingPeriodStartValueChangedCounter = 0;
			int billingPeriodEndValueChangedCounter = 0;
			int billingDateValueChangedCounter = 0;

			TestCASSBilling.HOTFileNameInfo.ValueChanged += (sender, e) => hotFileNameValueChangedCounter++;
			TestCASSBilling.BillingPeriodStartInfo.ValueChanged += (sender, e) => billingPeriodStartValueChangedCounter++;
			TestCASSBilling.BillingPeriodEndInfo.ValueChanged += (sender, e) => billingPeriodEndValueChangedCounter++;
			TestCASSBilling.BillingDateInfo.ValueChanged += (sender, e) => billingDateValueChangedCounter++;

			var cassCostHeader = TestCASSBilling.CostHeader;

			AssertEquals(1, hotFileNameValueChangedCounter);
			AssertEquals(1, billingPeriodStartValueChangedCounter);
			AssertEquals(1, billingPeriodEndValueChangedCounter);
			AssertEquals(1, billingDateValueChangedCounter);

			TestCASSBilling.Initialize(new CASSCostHeader());
			AssertEquals(2, hotFileNameValueChangedCounter);
			AssertEquals(2, billingPeriodStartValueChangedCounter);
			AssertEquals(2, billingPeriodEndValueChangedCounter);
			AssertEquals(2, billingDateValueChangedCounter);
		}

		[TestDate(2017, 09, 10)]
		public void TestRemoveAllLines()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.RecordType = isAdjustment ? "DCO" : "AWM";
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = "67828073";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "EUR";
						costLine.WeightChargePP = isAdjustment ? 1015.68M : (i == 0 ? 680.18M : 900.00M);
						costLine.VATDueAirline = isAdjustment ? 101.56M : 68.01M;
						cassBillingLine.AddCostLine(costLine, "AUD");
					}
				}
			}

			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();
			Factory.Save();

			AssertEquals("Precondition: Lines.Count", 2, TestCASSBilling.Lines.Count);

			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 400);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);

			TestCASSBilling.ForceRecalculateData();

			AssertEquals("Precondition: Lines.Count", 1, TestCASSBilling.Lines.Count);
			AssertEquals("Precondition: HiddenLines.Count", 1, TestCASSBilling.HiddenLines.Count);

			var line = TestCASSBilling.Lines[0];
			var hiddenLine = TestCASSBilling.HiddenLines[0];

			TestCASSBilling.RemoveAllLines();

			AssertEquals(0, TestCASSBilling.Lines.Count);
			Assert(line.IsDeleted);
			AssertEquals(0, TestCASSBilling.HiddenLines.Count);
			Assert(hiddenLine.IsDeleted);
		}

		public void TestAllocateApportionedAmountsInCorrectCharges()
		{
			IncludeCurrentDepartmentForChargeCodeFRT();
			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);

			SetupAssociatedBizos();
			var shipment3 = TestObjectCreator.CreateShipmentWithCoLoadMaster("SHIP3", Shipment2.JS_RL_NKOrigin, Shipment2.JS_RL_NKDestination, Shipment2);
			var consolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, null, 1200, true, AllocationMethod.Shipment);
			var charge2 = consolCost.ApportionmentCharges.FindChargeForJob(Shipment2);
			charge2.JR_IsUsedForApportionment = false;
			var shipment2Job = (Job)Shipment2.Job;
			var shipmentLevelChargeForShipment2 = shipment2Job.Charges.Cast<Charge>().First(x => x.JR_E6.IsEmpty);
			shipmentLevelChargeForShipment2.Delete();
			shipment2Job.Delete();
			Factory.Save();

			AssertEquals("Precondition: charge count", 2, consolCost.ApportionmentCharges.Count);
			var charge1 = consolCost.ApportionmentCharges.FindChargeForJob(Shipment1);
			var charge3 = consolCost.ApportionmentCharges.FindChargeForJob(shipment3);
			AssertEquals("Precondition: charge1 JR_LocalCostAmt", 600m, charge1.JR_LocalCostAmt);
			AssertEquals("Precondition: charge3 JR_LocalCostAmt", 600m, charge3.JR_LocalCostAmt);

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();
			TestCASSBilling.PostAPTransactions();
			AssertEquals("Precondition: APTransactions must not have errors.", false, TestCASSBilling.APTransactions.HasErrors());
			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			var transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			var completeInvoice = TestCASSBilling.GenerateCompleteInvoice(transaction);
			Assert("Precondition: Invoice must be posted.", completeInvoice.IsInDatabase);

			var lineJobNumbers = completeInvoice.Lines.Cast<InvoicingLineBase>().Select(line => (string)line.Job.JH_JobNum);
			var expectedJobNumbers = new[] { "SHIP1", "SHIP3" };
			AssertContainsExactElementsInAnyOrder("Invoice must have lines only for expected shipments", expectedJobNumbers, lineJobNumbers);
			AssertEquals("Line 1 amount", 340.09m, completeInvoice.Lines[0].AL_LocalExTaxAmount);
			AssertEquals("Line 2 amount", 340.09m, completeInvoice.Lines[1].AL_LocalExTaxAmount);
		}

		void AssertCASSGstRegistryAndCallCreateInvoices()
		{
			CASSFileImportDefaultTaxID registryValue = new CASSFileImportDefaultTaxID();
			registryValue.StandardRatedTaxID = Guid.Empty;
			registryValue.ZeroRatedTaxID = Guid.Empty;

			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			AssertExceptionThrown("CreateInvoices should throw a specific exception rather than null reference exception", typeof(CASSGstRegistryNotSetException), () => TestCASSBilling.CreateInvoices());
			AssertEquals("No transaction will be created as a result", 0, TestCASSBilling.APTransactions.Count);

			registryValue = new CASSFileImportDefaultTaxID();
			registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
			registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;

			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

			AssertNoExceptionThrown("CreateInvoices should not throw any exception", () => TestCASSBilling.CreateInvoices());
		}

		#region TestAllocationOfInvoiceNumber Tests

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfInvoiceNumber_ARInvoice()
		{
			var nonClashingInvoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.AUD, 1, null);
			nonClashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			nonClashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(false);
			AssertEquals("Transaction Number Correct", "CASSAUD121101", transactionNumber);
		}

		[TestDate(2012, 11, 1)]
		public void TestAllocationOfCreditNoteNumber_ARCreditNote()
		{
			var nonClashingInvoice = TestObjectCreator.CreateInvoice(typeof(ARCreditNote), TestObjectCreator.AUD, 1, null);
			nonClashingInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			nonClashingInvoice.AH_TransactionNum = "CASSAUD121101";
			var transactionNumber = RunAllocationOfTransactionNumberScenario(true);
			AssertEquals("Transaction Number Correct", "CASSAUD121101", transactionNumber);
		}

		#endregion

		[TestDate(2017, 09, 10)]
		public void TestWipIsSetCorrectDebtor()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var overseasAgent = TestObjectCreator.CreateOrgHeader("OVRAGNT", false, false);
			var localClient = TestObjectCreator.CreateOrgHeader("LCLCLNT", false, true);
			Factory.Save();

			SetupAssociatedBizos(false);
			Shipment2.Job.Delete();
			Shipment2.Delete();

			Shipment1.JS_INCO = "EXW";
			Shipment1.JS_RL_NKOrigin = "AUBNE";
			Shipment1.JS_RL_NKDestination = "JPTYO";

			var address1 = TestObjectCreator.CreateAddress(localClient);
			var address2 = TestObjectCreator.CreateAddress(overseasAgent);

			Job1.JH_OA_LocalChargesAddr = address1.PK;
			Job1.JH_OA_AgentCollectAddr = address2.PK;

			var jobCharge1 = TestObjectCreator.CreateCharge(Job1, TestObjectCreator.FRT, "FRT", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			jobCharge1.JR_GE = TestObjectCreator.FISDepartment.PK;
			jobCharge1.JR_OH_SellAccount = localClient.PK;

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.RecordType = isAdjustment ? "DCO" : "AWM";
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = "67828073";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "EUR";
						costLine.WeightChargePP = isAdjustment ? 101568M : (i == 0 ? 281863M : 68018M);
						costLine.VATDueAirline = isAdjustment ? 10156M : (i == 0 ? 0M : 6801M);
						cassBillingLine.AddCostLine(costLine, i == 0 ? "AUD" : "EUR");
					}
				}
			}

			Factory.Save();

			AccountingConfigurationRegistry.Instance.WIPMustHaveDebtorCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

			AssertEquals(1, transaction.Lines.Count);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			AssertNoExceptionThrown(() => TestCASSBilling.PostAPTransactions());
		}

		public void TestCassImportWithOppositeInvoiceAndTaxAmount()
		{
			var testAirLinePrefix = "172";
			var airLineOrg = TestObjectCreator.AALSHI;
			var airline = TestObjectCreator.CreateAirLine(testAirLinePrefix);
			airLineOrg.MiscServ.OM_RM_Airline = airline.PK;

			Factory.Save();

			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: false);
			SetupAssociatedBizos(false);

			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 2818.63M;
			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).VATDueAgent = -16.00M;
			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			costLine.AirlinePrefix = testAirLinePrefix;
			TestCASSBilling.Lines[0].AddCostLine(costLine, "AUD");

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();

			AssertCASSGstRegistryAndCallCreateInvoices();

			var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

			AssertEquals("There should be 2 transaction lines, one with positive cost with 0 tax and the other with negative cost and tax", 2, transaction.Lines.Count);
			Assert("Line 1 has a positive cost amount", transaction.Lines[0].AL_OSExTaxAmount > 0);
			AssertEquals("But has a 0 tax amount", 0M, transaction.Lines[0].AL_OSTaxAmount);

			Assert("Line 2 has negative cost amount", transaction.Lines[1].AL_OSExTaxAmount < 0);
			Assert("Also a negative tax amount", transaction.Lines[1].AL_OSTaxAmount < 0);

			ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

			foreach (InvoicingLineBase line in transaction.Lines)
			{
				totalExTaxAmount += line.AL_OSExTaxAmount;
				totalTaxAmount += line.AL_OSTaxAmount;
			}

			AssertEquals(2818.63M, transaction.AH_OSExTaxAmount);
			AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

			AssertEquals(-16M, transaction.AH_OSTaxAmount);
			AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
		}

		public void TestCassCostDistributiontWithSameSignedInvoiceAndTaxAmount_NoAccrual()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 50.00M);
				SetupAssociatedBizos(TestCASSBilling.Lines[0], null);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 8, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(650M, 31.79M), Tuple.Create<ZDecimal, ZDecimal>(-32.5M, -15.89M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(1250M, 61.12M), Tuple.Create<ZDecimal, ZDecimal>(-62.5M, -30.56M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M, 5.38M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M, -2.69M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(35M, 1.71M), Tuple.Create<ZDecimal, ZDecimal>(-1.75M, -0.86M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(-50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(50M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);

				var actualTraceMsg = string.Join("", DummyTracer.Traces);
				AssertContains("Started Apportioning", actualTraceMsg);
				AssertNotContains("A job charge found in database for charge code", actualTraceMsg);
				AssertContains("There is no matching consol cost in database", actualTraceMsg);
				AssertContains("After completing Apportioning. Job cost details-->", actualTraceMsg);
				AssertContains("Traced @CreateInvoice after calling invoice.ImportAllApportionmentsFromCosting-->Invoice Number:", actualTraceMsg);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestCassCostDistributiontWithSameSignedInvoiceAndTaxAmount_HasAccrual()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 50.00M);

				var jobChargeInfo = new List<(AccChargeCode, ZDecimal, ZDecimal)>();
				jobChargeInfo.Add((TestObjectCreator.FRT, 900M, 900M));
				jobChargeInfo.Add((TestObjectCreator.CC1, 100M, 100M));
				jobChargeInfo.Add((TestObjectCreator.CC3, 15M, 15M));

				SetupAssociatedBizos(TestCASSBilling.Lines[0], jobChargeInfo);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 8, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(1650M, 80.69M), Tuple.Create<ZDecimal, ZDecimal>(-82.5M, -40.34M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(250M, 12.22M), Tuple.Create<ZDecimal, ZDecimal>(-12.5M, -6.11M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M, 5.38M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M, -2.69M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(35M, 1.71M), Tuple.Create<ZDecimal, ZDecimal>(-1.75M, -0.86M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(-50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(50M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);

				var actualTraceMsg = string.Join("", DummyTracer.Traces);
				AssertContains("Started Apportioning", actualTraceMsg);
				AssertContains("A job charge found in database for charge code", actualTraceMsg);
				AssertContains("Accrual in database", actualTraceMsg);
				AssertContains("After completing Apportioning. Job cost details-->", actualTraceMsg);
				AssertContains("Traced @CreateInvoice after calling invoice.ImportAllApportionmentsFromCosting-->Invoice Number:", actualTraceMsg);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestCassCostDistributiontWithSameSignedInvoiceAndTaxAmount_HasAccrual_CarryForward()
		{
			var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
			AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.FRT.PK);
			AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.FRT.PK);

			var approvalValue = new CostVarianceApproval();
			approvalValue.AutoTickFinalFlag = false;

			using (AccountingConfigurationRegistry.Instance.CreateWIPWhenCostIsGreaterThanAccrual.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.PayableFinalFlag.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.PayableFinalFlagForConsolCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, approvalValue))
			using (AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection))
			{
				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(40M, 0M, 0M, 0M, 0M, 0M, 0M, 0M);
				SetupAssociatedBizos(TestCASSBilling.Lines[0], null, false);
				var cost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, null, 100, true);
				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				TestCASSBilling.PostAPTransactions();

				var jobPKs = Consol.Shipments.OfType<ForwardingShipment>().Select(s => s.Job.PK);
				var reloadedJobs = new BusinessObjectFactory().Load<Job>(new ZQuery(JobHeaderSchema.PK, jobPKs.ToArray()));
				var reloadedApportionedCharges = reloadedJobs.SelectMany(j => j.Charges.OfType<Charge>())
															.Where(c => c.JR_IsApportioned && c.JR_OSCostAmt > 0)
															.OrderBy(c => c.JR_OSCostAmt)
															.ToArray();
				AssertEquals("Charge Count", 2, reloadedApportionedCharges.Length);
				AssertEquals("First Charge is Posted", true, reloadedApportionedCharges[0].JR_IsCostPosted);
				AssertEquals("Second Charge is not Posted", false, reloadedApportionedCharges[1].JR_IsCostPosted);
				AssertNotNull("Second Charge should have an accrual", reloadedApportionedCharges[1].APLine);
				AssertEquals("Second Charge should have an accrual", "ACR", reloadedApportionedCharges[1].APLine.AL_LineType);
			}
		}

		public void TestCassCostDistributiontWithSameSignedInvoiceAndTaxAmount_HasAccrual_ButNotAllChargeCode()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 50.00M);

				var jobChargeInfo = new List<(AccChargeCode, ZDecimal, ZDecimal)>();
				jobChargeInfo.Add((TestObjectCreator.FRT, 900M, 900M));

				SetupAssociatedBizos(TestCASSBilling.Lines[0], jobChargeInfo);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 4, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(1935M, 94.62M), Tuple.Create<ZDecimal, ZDecimal>(-96.75M, -47.31M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M, 5.38M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M, -2.69M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(-50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(50M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestCassCostDistributiontWithSameSignedInvoiceAndTaxAmount_HasNegativeAccrual()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var prevNegativeACR = AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var prevNegativeValidation = AccountingConfigurationRegistry.Instance.NegativeCostValidationEnforced.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				AccountingConfigurationRegistry.Instance.NegativeCostValidationEnforced.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 50.00M);

				var jobChargeInfo = new List<(AccChargeCode, ZDecimal, ZDecimal)>();
				jobChargeInfo.Add((TestObjectCreator.FRT, -900M, -900M));
				jobChargeInfo.Add((TestObjectCreator.CC1, -100M, -100M));
				jobChargeInfo.Add((TestObjectCreator.CC3, -15M, -15M));

				SetupAssociatedBizos(TestCASSBilling.Lines[0], jobChargeInfo);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 8, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(1650M, 80.69M), Tuple.Create<ZDecimal, ZDecimal>(-82.5M, -40.34M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(250M, 12.22M), Tuple.Create<ZDecimal, ZDecimal>(-12.5M, -6.11M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M, 5.38M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M, -2.69M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(35M, 1.71M), Tuple.Create<ZDecimal, ZDecimal>(-1.75M, -0.86M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(-50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(50M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
				AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevNegativeACR);
				AccountingConfigurationRegistry.Instance.NegativeCostValidationEnforced.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevNegativeValidation);
			}
		}

		public void TestCassCostDistributiontWithSameSignedInvoiceAndTaxAmount_NoAccrual_NegativeDiscount()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.CC4.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.CC5.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(48.0M, 0M, 122.08M, 0M, 2.40M, -6.40M, 33.53M, 0M, vatDueAgentAmount: 0.46M);

				SetupAssociatedBizos(TestCASSBilling.Lines[0], null);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 10, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(24M, 4.56M), Tuple.Create<ZDecimal, ZDecimal>(-1.2M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(24M, 4.56M), Tuple.Create<ZDecimal, ZDecimal>(-1.2M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(122.08M, 23.19M), Tuple.Create<ZDecimal, ZDecimal>(-6.10M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC4, Tuple.Create<ZDecimal, ZDecimal>(-2.4M, -0.46M), Tuple.Create<ZDecimal, ZDecimal>(0.12M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC5, Tuple.Create<ZDecimal, ZDecimal>(6.4M, 1.22M), Tuple.Create<ZDecimal, ZDecimal>(-0.32M, 0M));

				AssertEquals(174.08M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-8.7040M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(165.38M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(33.07M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(0M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(33.07M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestCASSToShipment()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 50.00M, currency: "USD");

				var jobChargeInfo = new List<(AccChargeCode, ZDecimal, ZDecimal)>();
				jobChargeInfo.Add((TestObjectCreator.FRT, 900M, 900M));
				jobChargeInfo.Add((TestObjectCreator.CC1, 100M, 100M));
				jobChargeInfo.Add((TestObjectCreator.CC3, 15M, 15M));

				SetupAssociatedBizos(TestCASSBilling.Lines[0], jobChargeInfo, false);

				TestObjectCreator.CreateExchangeRate(TestObjectCreator.USD, 1M);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				Factory.Save();

				var charges = Factory.Load<Job>(Shipment1.Job.PK).Charges;

				transaction.Factory.Save();

				AssertEquals("Consol Cost count", 8, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(1650M, 80.69M), Tuple.Create<ZDecimal, ZDecimal>(-82.5M, -40.34M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(250M, 12.22M), Tuple.Create<ZDecimal, ZDecimal>(-12.5M, -6.11M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M, 5.38M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M, -2.69M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(35M, 1.71M), Tuple.Create<ZDecimal, ZDecimal>(-1.75M, -0.86M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(-50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(50M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);

				// 8 Apportioned charge + 3 shipment level charge
				AssertEquals(11, Job1.Charges.Count);
				AssertEquals("Apportioned Charge Count", 8, Job1.Charges.Cast<Charge>().Count(x => x.JR_IsApportioned));
				AssertEquals("Unapportioned Charge Count", 3, Job1.Charges.Cast<Charge>().Count(x => !x.JR_IsApportioned));
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestCassCostDistributiontWithOppositeSignedInvoiceAndTaxAmount_NoAccrual()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 0M, 0M, vatDueAgentAmount: 100M, adjustedVatDueAgentAmount: 50M);
				SetupAssociatedBizos(TestCASSBilling.Lines[0], null);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 16, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(1332.15M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-682.14M, -68.21M), Tuple.Create<ZDecimal, ZDecimal>(-373.57M, 0M), Tuple.Create<ZDecimal, ZDecimal>(341.07M, 34.11M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(1638.75M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-388.76M, -38.88M), Tuple.Create<ZDecimal, ZDecimal>(-256.88M, 0M), Tuple.Create<ZDecimal, ZDecimal>(194.38M, 19.44M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(56.21M, 0M), Tuple.Create<ZDecimal, ZDecimal>(53.79M, 5.38M), Tuple.Create<ZDecimal, ZDecimal>(21.39M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-26.89M, -2.69M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(17.89M, 0M), Tuple.Create<ZDecimal, ZDecimal>(17.11M, 1.71M), Tuple.Create<ZDecimal, ZDecimal>(6.81M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-8.56M, -0.86M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(-100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(-50M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestCassCostDistributiontWithOppositeSignedInvoiceAndTaxAmount_HasAccrual()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			try
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 0M, 0M, vatDueAgentAmount: 100M, adjustedVatDueAgentAmount: 50M);

				var jobChargeInfo = new List<(AccChargeCode, ZDecimal, ZDecimal)>();
				jobChargeInfo.Add((TestObjectCreator.FRT, 900M, 900M));
				jobChargeInfo.Add((TestObjectCreator.CC1, 100M, 100M));
				jobChargeInfo.Add((TestObjectCreator.CC3, 15M, 15M));

				SetupAssociatedBizos(TestCASSBilling.Lines[0], jobChargeInfo);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 16, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(2643.15M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-993.15M, -99.31M), Tuple.Create<ZDecimal, ZDecimal>(-579.07M, 0M), Tuple.Create<ZDecimal, ZDecimal>(496.57M, 49.66M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(327.75M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-77.75M, -7.78M), Tuple.Create<ZDecimal, ZDecimal>(-51.38M, 0M), Tuple.Create<ZDecimal, ZDecimal>(38.88M, 3.89M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(56.21M, 0M), Tuple.Create<ZDecimal, ZDecimal>(53.79M, 5.38M), Tuple.Create<ZDecimal, ZDecimal>(21.39M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-26.89M, -2.69M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(17.89M, 0M), Tuple.Create<ZDecimal, ZDecimal>(17.11M, 1.71M), Tuple.Create<ZDecimal, ZDecimal>(6.81M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-8.56M, -0.86M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(-100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(-50M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
			}
		}

		public void TestTaxID_WhenCompanyAndCreditorIsNotTaxRegistered()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var prevGSTSetup = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
				TestObjectCreator.AALSHI.CompanyData.OB_APVATConfig = "NON";

				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 50.00M, vATIndicator: "Y");
				SetupAssociatedBizos(TestCASSBilling.Lines[0], null);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();

				var registryValue = new CASSFileImportDefaultTaxID();
				registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
				registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;

				AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

				AssertNoExceptionThrown("CreateInvoices should not throw any exception", () => TestCASSBilling.CreateInvoices());
				AssertHasRowWarningContaining(TestCASSBilling.APTransactions[0], InvoicingLineBaseValidation.GetTaxRecalculationWarningMessage());

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 8, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(650M + 31.79M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-32.5M - 15.89M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(1250M + 61.12M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-62.5M - 30.56M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M + 5.38M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M - 2.69M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(35M + 1.71M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-1.75M - 0.86M, 0M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M + 50M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);
				Assert("AL_AT is NULL", transaction.Lines.Cast<InvoicingLineBase>().All(x => !x.AL_AT.IsValid));

				AssertEquals(100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(-50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(0M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = prevGSTSetup;
			}
		}

		public void TestTaxID_WhenCompanyIsNotTaxRegisteredAndCreditorIsTaxRegistered()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var prevGSTSetup = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;

				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 50.00M, vATIndicator: "Y");
				SetupAssociatedBizos(TestCASSBilling.Lines[0], null);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();

				var registryValue = new CASSFileImportDefaultTaxID();
				registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
				registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;

				AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

				AssertNoExceptionThrown("CreateInvoices should not throw any exception", () => TestCASSBilling.CreateInvoices());
				AssertHasRowWarningContaining(TestCASSBilling.APTransactions[0], InvoicingLineBaseValidation.GetTaxRecalculationWarningMessage());

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 8, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(650M + 31.79M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-32.5M - 15.89M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(1250M + 61.12M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-62.5M - 30.56M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M + 5.38M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M - 2.69M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(35M + 1.71M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-1.75M - 0.86M, 0M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M + 50M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);
				Assert("AL_AT is NULL", transaction.Lines.Cast<InvoicingLineBase>().All(x => !x.AL_AT.IsValid));

				AssertEquals(100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(-50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(0M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = prevGSTSetup;
			}
		}

		public void TestTaxID_WhenCompanyIsTaxRegisteredAndCreditorIsNotTaxRegistered()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var prevGSTSetup = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				TestObjectCreator.AALSHI.CompanyData.OB_APVATConfig = "NON";

				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 50.00M, vATIndicator: "Y");
				SetupAssociatedBizos(TestCASSBilling.Lines[0], null);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();

				var registryValue = new CASSFileImportDefaultTaxID();
				registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
				registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;

				AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

				AssertNoExceptionThrown("CreateInvoices should not throw any exception", () => TestCASSBilling.CreateInvoices());
				AssertHasRowWarningContaining(TestCASSBilling.APTransactions[0], InvoicingLineBaseValidation.GetTaxRecalculationWarningMessage());

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 8, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(650M + 31.79M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-32.5M - 15.89M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(1250M + 61.12M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-62.5M - 30.56M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M + 5.38M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M - 2.69M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(35M + 1.71M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-1.75M - 0.86M, 0M));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M + 50M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);
				Assert("AL_AT is NULL", transaction.Lines.Cast<InvoicingLineBase>().All(x => !x.AL_AT.IsValid));

				AssertEquals(100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(-50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(0M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = prevGSTSetup;
			}
		}

		public void TestTaxID_WhenCompanyAndCreditorAreTaxRegistered()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var prevGSTSetup = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				TestObjectCreator.AALSHI.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 100.00M, 50.00M, vATIndicator: "Y");
				SetupAssociatedBizos(TestCASSBilling.Lines[0], null);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();

				var registryValue = new CASSFileImportDefaultTaxID();
				registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
				registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;

				AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

				AssertNoExceptionThrown("CreateInvoices should not throw any exception", () => TestCASSBilling.CreateInvoices());
				AssertNoRowWarningContaining(TestCASSBilling.APTransactions[0], InvoicingLineBaseValidation.GetTaxRecalculationWarningMessage());

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 8, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(650M, 31.79M), Tuple.Create<ZDecimal, ZDecimal>(-32.5M, -15.89M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(1250M, 61.12M), Tuple.Create<ZDecimal, ZDecimal>(-62.5M, -30.56M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M, 5.38M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M, -2.69M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(35M, 1.71M), Tuple.Create<ZDecimal, ZDecimal>(-1.75M, -0.86M));
				Assert("AL_AT is NOT NULL", transaction.Lines.Cast<InvoicingLineBase>().All(x => x.AL_AT.IsValid));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(100M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(-50M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(50M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = prevGSTSetup;
			}
		}

		public void TestTaxID_WhenCompanyAndCreditorAreTaxRegisteredButHotFileDoesntHaveVAT()
		{
			var prevValue = AccountingConfigurationRegistry.Instance.CASSChargeCodes.GetValueWithoutFallback(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			var prevGSTSetup = GlbCompany.CurrentCompany.GC_IsGSTRegistered;
			try
			{
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
				TestObjectCreator.AALSHI.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC3.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 0);
				AddExportCASSBillingLine(2500.00M, 110.00M, 35.00M, 560.00M, 15.00M, 25.00M, 0M, 0M, vATIndicator: "N");
				SetupAssociatedBizos(TestCASSBilling.Lines[0], null);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();

				var registryValue = new CASSFileImportDefaultTaxID();
				registryValue.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
				registryValue.StandardRatedTaxID = TestObjectCreator.GST1.PK;

				AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

				AssertNoExceptionThrown("CreateInvoices should not throw any exception", () => TestCASSBilling.CreateInvoices());
				AssertNoRowWarningContaining(TestCASSBilling.APTransactions[0], InvoicingLineBaseValidation.GetTaxRecalculationWarningMessage());

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals("Consol Cost count", 8, transaction.ConsolCosting.ConsolCosts.Count);
				AssertDistributedCost(transaction, TestObjectCreator.FRT, Tuple.Create<ZDecimal, ZDecimal>(650M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-32.5M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC1, Tuple.Create<ZDecimal, ZDecimal>(1250M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-62.5M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC2, Tuple.Create<ZDecimal, ZDecimal>(110M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-5.5M, 0M));
				AssertDistributedCost(transaction, TestObjectCreator.CC3, Tuple.Create<ZDecimal, ZDecimal>(35M, 0M), Tuple.Create<ZDecimal, ZDecimal>(-1.75M, 0M));
				Assert("AL_AT is GSTFREE1", transaction.Lines.Cast<InvoicingLineBase>().All(x => x.AL_AT == TestObjectCreator.GSTFREE1.PK));

				AssertEquals(2045M, TestCASSBilling.Lines[0].CASSCostValue);
				AssertEquals(-102.25M, TestCASSBilling.Lines[0].CASSCostAdjustedValue);
				AssertEquals(1942.75M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(0M, TestCASSBilling.Lines[0].CASSCostTaxValue);
				AssertEquals(0M, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);
				AssertEquals(0M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, prevValue);
				GlbCompany.CurrentCompany.GC_IsGSTRegistered = prevGSTSetup;
			}
		}

		public void TestCassImportWithOppositeInvoiceAndTaxAmountLinkedToConsol()
		{
			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);

			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

			SetupCASSBilling(TestCASSBilling, 1, setupAmount: false, setAdjustmentValues: false);
			SetupAssociatedBizos(false);

			var newLine = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
			newLine.RecordType = "AWM";
			newLine.WeightChargePP = 2818.63M;
			newLine.VATDueAirline = -16.00M;

			var costLine = (TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine);
			costLine.Merge(newLine);

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();

			AssertCASSGstRegistryAndCallCreateInvoices();

			var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

			AssertEquals("There should be 8 transaction lines, all with its corresponding opposite signed tax line", 8, transaction.Lines.Count);

			var positiveLines = transaction.Lines.Where(x => ((InvoicingLineBase)x).AL_OSExTaxAmount > 0);
			AssertEquals("There should be 4 postive transaction lines", 4, positiveLines.Count());

			var negativeLines = transaction.Lines.Where(x => ((InvoicingLineBase)x).AL_OSExTaxAmount < 0);
			AssertEquals("There should be 4 negative transaction lines", 4, negativeLines.Count());

			ZDecimal positiveExTaxAmount = 0, negativeExTaxAmount = 0, totalExTaxAmount = 0;
			ZDecimal positiveTotalTaxAmount = 0, negativeTotalTaxAmount = 0, totalTaxAmount = 0;

			AssertLinesAndSumTotal(positiveLines, 2, out positiveExTaxAmount, out positiveTotalTaxAmount);
			AssertLinesAndSumTotal(negativeLines, 2, out negativeExTaxAmount, out negativeTotalTaxAmount);

			totalExTaxAmount = positiveExTaxAmount + negativeExTaxAmount;
			totalTaxAmount = positiveTotalTaxAmount + negativeTotalTaxAmount;

			AssertEquals(2818.63M, transaction.AH_OSExTaxAmount);
			AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

			AssertEquals(-16.00M, transaction.AH_OSTaxAmount);
			AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
		}

		public void TestCassImportWithConsolCostWhenOrgIsNotDebtorOrOrgIsNotActive()
		{
			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
			GlbCompany company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			company.GC_IsGSTRegistered = true;

			SetupAssociatedBizos(false);
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, 1000, false, "CHG");
			consolCost.ApportionmentCharges[0].JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			consolCost.ApportionmentCharges[1].JR_OH_SellAccount = TestObjectCreator.AALSHI.PK;
			Factory.Save();

			TestObjectCreator.AALSHI.OH_IsActive = false;
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var testCassBilling = new CASSBilling(factory2);
			SetupCASSBilling(testCassBilling);
			testCassBilling.CreateInvoices();
			testCassBilling.PostAPTransactions();

			AssertEquals(false, testCassBilling.APTransactions[0].Notifications.Any(x => x.Message == "Error - Accounts Payable Invoice: Error - JR_OH_SellAccount: This Debtor is inactive - it may not be used."));
		}

		public void TestCassImportWhenOrgIsNotDebtorOrOrgIsNotActive()
		{
			var overseasAgent = TestObjectCreator.CreateOrgHeader("OVRAGNT", false, true);
			var localClient = TestObjectCreator.CreateOrgHeader("LCLCLNT", false, true);
			Factory.Save();

			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);
			SetupAssociatedBizos(false, setConsigneeDocAddressOrg: false);

			Shipment2.Job.Delete();
			Shipment2.Delete();

			// Set Shipment1 details to use overseasAgent as default Debtor
			Shipment1.JS_INCO = "EXW";
			Shipment1.JS_RL_NKOrigin = "AUBNE";
			Shipment1.JS_RL_NKDestination = "JPTYO";

			var address1 = TestObjectCreator.CreateAddress(localClient);
			var address2 = TestObjectCreator.CreateAddress(overseasAgent);

			Job1.JH_OA_LocalChargesAddr = address1.PK;
			Job1.JH_OA_AgentCollectAddr = address2.PK;

			var jobCharge1 = TestObjectCreator.CreateCharge(Job1, TestObjectCreator.FRT, "FRT", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			jobCharge1.JR_GE = TestObjectCreator.FISDepartment.PK;

			// Make overseasAgent inactive
			overseasAgent.IsCancelled = true;
			jobCharge1.ResetChargeDebtor();
			AssertEquals("Inactive Org set as default Debtor", overseasAgent.PK, jobCharge1.JR_OH_SellAccount);

			var costLine = (TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine);

			costLine.WeightChargePP = 281863M;
			costLine.ValuationChargePP = costLine.ChargesDueCarrierPP = costLine.ChargesDueAgentCC = costLine.Commission = costLine.Discount = 0M;
			costLine.VATDueAirline = costLine.VATDueAgent = 0M;

			Factory.Save();

			// Do CASS Import stuff
			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();
			var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

			AssertEquals("One line in transaction", 1, transaction.Lines.Count);
			AssertNotNull("Line is Job related", transaction.Lines[0].InvoicingJob);
			AssertEquals("Shipment1 Job", Job1.PK, transaction.Lines[0].InvoicingJob.PK);

			AssertEquals("Two Charges", 2, transaction.Lines[0].InvoicingJob.Charges.Count);
			var newCharge = transaction.Lines[0].InvoicingJob.Charges.Cast<JobCharge>().FirstOrDefault(x => !x.IsInDatabase);
			var oldCharge = transaction.Lines[0].InvoicingJob.Charges.Cast<JobCharge>().FirstOrDefault(x => x.IsInDatabase);
			AssertNotNull(newCharge);
			AssertNotNull(oldCharge);
			AssertEquals("Debtor is not set on newCharge created by CASS import", ZGuid.Empty, newCharge.JR_OH_SellAccount);
			AssertEquals("Debtor is still set on oldCharge", overseasAgent.PK, oldCharge.JR_OH_SellAccount);
		}

		[TestDate(2017, 09, 10)]
		public void TestCassImportWithOppositeInvoiceAndTaxAmountWithPropotionalApportionment()
		{
			var today = ZDateTime.Today;

			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: false);
			SetupAssociatedBizos(false);

			var jobCharge1 = TestObjectCreator.CreateCharge(Job1, TestObjectCreator.FRT, "FRT", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			var jobCharge2 = TestObjectCreator.CreateCharge(Job2, TestObjectCreator.FRT, "FRT", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);

			var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
			costLine.RecordType = "AWM";
			costLine.VATIndicator = "Y";
			costLine.AirlinePrefix = "172";
			costLine.AWBSerialNumber = "67828073";
			costLine.AgentCode = "23470068510";
			costLine.DateAWBExecution = today.AddMonths(-3);
			costLine.DateOfArrival = today.AddMonths(-2);
			costLine.DateOfDelivery = today.AddMonths(-1);
			costLine.Origin = "LEJ";
			costLine.Destination = "MEX";
			costLine.Weight = 2150M;
			costLine.WeightUnit = "KG";
			costLine.CurrencyCode = "AUD";
			costLine.WeightChargePP = 2818.63M;
			costLine.ValuationChargePP = costLine.ChargesDueCarrierPP = costLine.ChargesDueAgentCC = costLine.Discount = 0M;
			costLine.Commission = 200M;
			costLine.VATDueAirline = 0M;
			costLine.VATDueAgent = 16.00M;
			TestCASSBilling.Lines[0].AddCostLine(costLine, "AUD");

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();

			AssertCASSGstRegistryAndCallCreateInvoices();

			var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

			AssertEquals("There should be 4 transaction lines, all with its corresponding opposite signed tax line", 4, transaction.Lines.Count);

			var positiveLines = transaction.Lines.Where(x => ((InvoicingLineBase)x).AL_OSExTaxAmount > 0);
			AssertEquals("There should be 2 postive transaction lines", 2, positiveLines.Count());

			var negativeLines = transaction.Lines.Where(x => ((InvoicingLineBase)x).AL_OSExTaxAmount < 0);
			AssertEquals("There should be 2 negative transaction lines", 2, negativeLines.Count());

			ZDecimal positiveExTaxAmount = 0, negativeExTaxAmount = 0, totalExTaxAmount = 0;
			ZDecimal positiveTotalTaxAmount = 0, negativeTotalTaxAmount = 0, totalTaxAmount = 0;

			AssertLinesAndSumTotal(positiveLines, 1, out positiveExTaxAmount, out positiveTotalTaxAmount); //there should be only one group, as only one charge code - 'FRT' has been used in the consol
			AssertLinesAndSumTotal(negativeLines, 1, out negativeExTaxAmount, out negativeTotalTaxAmount); //there should be only one group, as only one charge code - 'FRT' has been used in the consol

			totalExTaxAmount = positiveExTaxAmount + negativeExTaxAmount;
			totalTaxAmount = positiveTotalTaxAmount + negativeTotalTaxAmount;

			AssertEquals(2618.63M, transaction.AH_OSExTaxAmount);
			AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

			AssertEquals(-16M, transaction.AH_OSTaxAmount);
			AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
		}

		public void TestCassImportWithOppositeInvoiceAndTaxAmountWithPropotionalApportionmentToTwoDifferentCharges()
		{
			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: false);

			var newLine = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
			newLine.RecordType = "AWM";
			newLine.WeightChargePP = 2818.63M;
			newLine.VATDueAirline = -16.00M;

			var costLine = (TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine);
			costLine.Merge(newLine);

			SetupAssociatedBizos(false);
			var jobCharge1 = TestObjectCreator.CreateCharge(Job1, TestObjectCreator.FRT, "FRT", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);
			var jobCharge2 = TestObjectCreator.CreateCharge(Job2, TestObjectCreator.MRG100, "MRG100", TestObjectCreator.AUD, 100M, null, TestObjectCreator.AUD, 100M, null);

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();

			AssertCASSGstRegistryAndCallCreateInvoices();

			var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

			AssertEquals("There should be 8 transaction lines, all with its corresponding opposite signed tax line", 8, transaction.Lines.Count);

			var positiveLines = transaction.Lines.Where(x => ((InvoicingLineBase)x).AL_OSExTaxAmount > 0);
			AssertEquals("There should be 4 postive transaction lines", 4, positiveLines.Count());

			var negativeLines = transaction.Lines.Where(x => ((InvoicingLineBase)x).AL_OSExTaxAmount < 0);
			AssertEquals("There should be 4 negative transaction lines", 4, negativeLines.Count());

			ZDecimal positiveExTaxAmount = 0, negativeExTaxAmount = 0, totalExTaxAmount = 0;
			ZDecimal positiveTotalTaxAmount = 0, negativeTotalTaxAmount = 0, totalTaxAmount = 0;

			AssertLinesAndSumTotal(positiveLines, 2, out positiveExTaxAmount, out positiveTotalTaxAmount);
			AssertLinesAndSumTotal(negativeLines, 2, out negativeExTaxAmount, out negativeTotalTaxAmount);

			totalExTaxAmount = positiveExTaxAmount + negativeExTaxAmount;
			totalTaxAmount = positiveTotalTaxAmount + negativeTotalTaxAmount;

			AssertEquals(2818.63M, transaction.AH_OSExTaxAmount);
			AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

			AssertEquals(-16.00M, transaction.AH_OSTaxAmount);
			AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
		}

		public void TestCassImportWithOppositeInvoiceAndTaxAmount_Gateway()
		{
			using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid() }.Set())
			{
				var testAirLinePrefix = "172";
				var airLineOrg = TestObjectCreator.AALSHI;
				var airline = TestObjectCreator.CreateAirLine(testAirLinePrefix);
				airLineOrg.MiscServ.OM_RM_Airline = airline.PK;

				Factory.Save();

				var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

				SetupCASSBilling(TestCASSBilling, numberOfLines: 1, setAdjustmentValues: false, setupAmount: false);
				SetupAssociatedBizos(false);

				var costLine = TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine;
				costLine.CurrencyCode = "AUD";
				costLine.WeightChargePP = 2818.63M;
				costLine.ValuationChargePP = costLine.ChargesDueCarrierPP = costLine.ChargesDueAgentCC = costLine.Commission = costLine.Discount = 0M;
				costLine.VATDueAgent = -16.00M;

				var otherCostLine = new CASSCostExportLine(Factory, CASSCostLineType.Default);
				otherCostLine.AirlinePrefix = testAirLinePrefix;
				TestCASSBilling.Lines[0].AddCostLine(otherCostLine, "AUD");

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();

				AssertCASSGstRegistryAndCallCreateInvoices();

				var transaction = TestCASSBilling.GenerateCompleteInvoice(TestCASSBilling.APTransactions[0]);

				AssertEquals("There should be 2 transaction lines, one with positive cost with 0 tax and the other with negative cost and tax", 2, transaction.Lines.Count);
				Assert("Line 1 has a positive cost amount", transaction.Lines[0].AL_OSExTaxAmount > 0);
				AssertEquals("But has a 0 tax amount", 0M, transaction.Lines[0].AL_OSTaxAmount);

				Assert("Line 2 has negative cost amount", transaction.Lines[1].AL_OSExTaxAmount < 0);
				Assert("Also a negative tax amount", transaction.Lines[1].AL_OSTaxAmount < 0);

				ZDecimal totalExTaxAmount = 0, totalTaxAmount = 0;

				foreach (InvoicingLineBase line in transaction.Lines)
				{
					totalExTaxAmount += line.AL_OSExTaxAmount;
					totalTaxAmount += line.AL_OSTaxAmount;
				}

				AssertEquals(2818.63M, transaction.AH_OSExTaxAmount);
				AssertEquals(totalExTaxAmount, transaction.AH_OSExTaxAmount);

				AssertEquals(-16M, transaction.AH_OSTaxAmount);
				AssertEquals(totalTaxAmount, transaction.AH_OSTaxAmount);
			}
		}

		void AssertLinesAndSumTotal(IEnumerable<BusinessObject> lines, int numberOfGroups, out ZDecimal totalExTaxAmount, out ZDecimal totalTaxAmount)
		{
			List<ZGuid> jobList = new List<ZGuid>();
			jobList.Add(Job1.PK);
			jobList.Add(Job2.PK);

			totalExTaxAmount = 0;
			totalTaxAmount = 0;

			var linesGroupbyChargeCode = lines.GroupBy(x => ((InvoicingLineBase)x).AL_AC);
			AssertEquals(string.Format("There should be {0} groups", numberOfGroups), numberOfGroups, linesGroupbyChargeCode.Count());

			foreach (var groupedByCharge in linesGroupbyChargeCode)
			{
				AssertCollectionContains("Each charge code should contain in Cass charge code registry", groupedByCharge.Key.ToString(), AccountingConfigurationRegistry.Instance.CASSChargeCodes.Value.Cast<CASSChargeCode>().Select(x => x.ChargeCodePK.ToString()).ToArray());

				AssertEquals("There should be 2 lines in each group, as there are two shipments in the consol", 2, groupedByCharge.Count());
				foreach (InvoicingLineBase invoiceLine in groupedByCharge)
				{
					AssertCollectionContains("Job should be in Job List Collection", invoiceLine.AL_JH, jobList);

					totalExTaxAmount += invoiceLine.AL_OSExTaxAmount;
					totalTaxAmount += invoiceLine.AL_OSTaxAmount;
				}
			}
		}

		public void TestAddConsolCostsWithProportionalApportionmentByChargeCodesForNonGSTCompany()
		{
			AssertAddConsolCostsForNonGSTCompany(true);
		}

		public void TestAddConsolCostsWithEquivalentApportionmentByChargeCodesForNonGSTCompany()
		{
			AssertAddConsolCostsForNonGSTCompany(false);
		}

		void AssertAddConsolCostsForNonGSTCompany(bool proportionalApportionment)
		{
			var companyWithNoTaxRates = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			companyWithNoTaxRates.GC_IsGSTRegistered = false;
			AssertCompanyHasNoTaxRates(companyWithNoTaxRates);
			var branchOnCompanyWithNoTaxRate = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, companyWithNoTaxRates.PK));

			using (branchOnCompanyWithNoTaxRate.SetAsTemporaryContext())
			{
				SetupCASSBilling(TestCASSBilling, proportionalApportionment ? -1 : 1);
				SetupAssociatedBizos(proportionalApportionment);
				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				TestCASSBilling.CreateInvoices();
			}
		}

		public void TestAddConsolCostsForItaly_AccrualWithTaxAndMessage()
		{
			AssertAddConsolCostsForItaly(setTaxRate: true, setTaxMessage: true);
		}

		public void TestAddConsolCostsForItaly_AccrualWithoutTaxAndMessage()
		{
			AssertAddConsolCostsForItaly(setTaxRate: false, setTaxMessage: false);
		}

		public void TestAddConsolCostsForItaly_AccrualWithTaxOnly()
		{
			AssertAddConsolCostsForItaly(setTaxRate: true, setTaxMessage: false);
		}

		void AssertAddConsolCostsForItaly(bool setTaxRate, bool setTaxMessage)
		{
			var countryCode = Constants.CountryCodes.Italy;
			var taxRate0 = TestObjectCreator.CreateTaxRate("GSTFREE", "Esente IVA", 0, 1, countryCode, AccTaxRate.Types.Rated);
			var taxRate10 = TestObjectCreator.CreateTaxRate("GST", "IVA 10%", 10, 1, countryCode, AccTaxRate.Types.Rated);

			var taxRate20 = TestObjectCreator.CreateTaxRate("GST2", "IVA 20%", 20, 1, countryCode, AccTaxRate.Types.Rated);
			taxRate20.AT_ReferenceRateType = "STD";
			taxRate20.AT_RN_NKCountry = countryCode;
			Factory.Save();

			var registryValue = new CASSFileImportDefaultTaxID();
			registryValue.ZeroRatedTaxID = taxRate0.PK;
			registryValue.StandardRatedTaxID = taxRate10.PK;

			var currCompany = GlbCompany.CurrentCompany;
			var prevGSTSetup = currCompany.GC_IsGSTRegistered;
			using (AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (currCompany.TemporarilySetCountry(countryCode))
			{
				currCompany.GC_IsGSTRegistered = true;

				SetupAssociatedBizos(true, "EUR");

				var taxMsg20 = TestObjectCreator.TaxMsg2;
				var chargeCode = TestObjectCreator.FRT;
				var creditor = TestObjectCreator.AALSHI;
				creditor.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;

				TestObjectCreator.CreateTaxOverride(chargeCode, taxRate20.PK, taxMsg20.PK);
				Factory.Save();

				var consolCost = TestObjectCreator.CreateConsolCost(Consol, chargeCode, creditor);
				consolCost.E6_LocalCostAmount = 1000;
				consolCost.E6_ApportionmentMethod = AllocationMethod.Shipment;
				consolCost.E6_AT_TaxRate = setTaxRate ? taxRate20.PK : ZGuid.Empty;
				consolCost.E6_A9_VATClass = setTaxMessage ? taxMsg20.PK : ZGuid.Empty;
				Factory.Save();

				SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, currencyCode: "EUR");
				AssertNotNull(TestCASSBilling.Lines[0].Creditor);

				TestCASSBilling.ForceRecalculateData();
				TestCASSBilling.CreateInvoices();
				TestCASSBilling.PostAPTransactions();

				var testCassAPTransactions = TestCASSBilling.APTransactions;
				AssertEquals(1, testCassAPTransactions.Count);
				var testAPTransaction = testCassAPTransactions[0];
				Assert(testAPTransaction.Factory.HasContext(BusinessContext.CASS));

				var completeInvoice = TestCASSBilling.GenerateCompleteInvoice(testAPTransaction);
				AssertNotNull(completeInvoice);

				foreach (APInvoiceLine invLine in completeInvoice.Lines)
				{
					AssertEquals(setTaxRate ? taxRate20.PK : taxRate0.PK, invLine.TaxRate?.PK ?? ZGuid.Empty);
					AssertEquals(setTaxMessage ? taxMsg20.PK : ZGuid.Empty, invLine.VATClass?.PK ?? ZGuid.Empty);
				}

				currCompany.GC_IsGSTRegistered = prevGSTSetup;
			}
		}

		public void TestAddConsolCostsWithProportionalApportionmentByChargeCodesUsesCorrectApportionmentMethod_GrossWeight()
		{
			TestAddConsolCostsWithProportionalApportionmentByChargeCodesUsesCorrectApportionmentMethod(AllocationMethod.GrossWeight);
		}

		public void TestAddConsolCostsWithProportionalApportionmentByChargeCodesUsesCorrectApportionmentMethod_Manual()
		{
			TestAddConsolCostsWithProportionalApportionmentByChargeCodesUsesCorrectApportionmentMethod(AllocationMethod.Manual);
		}

		void TestAddConsolCostsWithProportionalApportionmentByChargeCodesUsesCorrectApportionmentMethod(ZString existingCostAppMethod)
		{
			var now = ZDateTime.Now;
			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			company.GC_IsGSTRegistered = true;

			SetupAssociatedBizos(false);

			Shipment1.JS_UnitOfWeight = Shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
			Shipment1.JS_UnitOfVolume = Shipment2.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			Shipment1.JS_ActualWeight = 990;
			Shipment2.JS_ActualWeight = 10;
			Shipment1.JS_ActualVolume = 1000;
			Shipment2.JS_ActualVolume = 1000;
			Factory.Save();

			TestObjectCreator.FRT.AC_Code = "FRT Charge";
			var postedConsolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
			postedConsolCost.E6_LocalCostAmount = 500;
			postedConsolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
			Factory.Save();

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1m, 0m, 0m, 0m, 0m, 0m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines.RemoveAndDeleteAll();
			var invoiceLine1 = TestObjectCreator.CreateAPInvoiceLine(invoice, Job1, TestObjectCreator.FRT, TestObjectCreator.AUD, 1m, "", 250m);
			var invoiceLine2 = TestObjectCreator.CreateAPInvoiceLine(invoice, Job2, TestObjectCreator.FRT, TestObjectCreator.AUD, 1m, "", 250m);
			postedConsolCost.E6_AH_APInvoice = invoice.PK;
			var apportionCharges = postedConsolCost.ApportionmentCharges;
			apportionCharges[0].ReverseAccrual(now);
			apportionCharges[1].ReverseAccrual(now);
			apportionCharges[0].JR_AL_APLine = invoiceLine1.PK;
			apportionCharges[1].JR_AL_APLine = invoiceLine2.PK;
			apportionCharges[0].SetAmountsToLinkedLinesForTests();
			apportionCharges[1].SetAmountsToLinkedLinesForTests();

			var consolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
			consolCost.E6_LocalCostAmount = 1000;
			consolCost.E6_ApportionmentMethod = existingCostAppMethod;
			Factory.Save();

			var testCassBilling = new CASSBilling(Factory);
			SetupCASSBilling(testCassBilling, 1, setAdjustmentValues: false);
			testCassBilling.CreateInvoices();
			testCassBilling.PostAPTransactions();

			var factory3 = new BusinessObjectFactory();
			var job1Reloaded = factory3.Load<Job>(Consol.Shipments[0].Job.PK);
			var job2Reloaded = factory3.Load<Job>(Consol.Shipments[1].Job.PK);
			AssertEquals("First job has two charges", 2, job1Reloaded.Charges.Count);
			AssertEquals("Second job has two charges", 2, job2Reloaded.Charges.Count);
			var job1CassChargeReloaded = (Charge)job1Reloaded.Charges.Single(c => ((Charge)c).ParentConsolCost.PK == consolCost.PK);
			var job2CassChargeReloaded = (Charge)job2Reloaded.Charges.Single(c => ((Charge)c).ParentConsolCost.PK == consolCost.PK);

			if (existingCostAppMethod == AllocationMethod.GrossWeight)
			{
				AssertEquals("Apportionment Method set to Gross Weight", AllocationMethod.GrossWeight, job1CassChargeReloaded.ParentConsolCost.E6_ApportionmentMethod);
				AssertEquals("Apportionment Method Used is Based on Gross Weight ($680.18 - $6.80 = $673.38)", 673.38M, job1CassChargeReloaded.JR_LocalCostAmt);
				AssertEquals("Apportionment Method Used is Based on Gross Weight (1% of $680.18 = $6.80)", 6.80M, job2CassChargeReloaded.JR_LocalCostAmt);
			}
			else if (existingCostAppMethod == AllocationMethod.Manual)
			{
				AssertEquals("Apportionment Method set to Chargeable Units", AllocationMethod.ChargeableUnits, job1CassChargeReloaded.ParentConsolCost.E6_ApportionmentMethod);
				AssertEquals("Apportionment Method Used is Based on Chargeable Units ($340.09)", 340.09M, job1CassChargeReloaded.JR_LocalCostAmt);
				AssertEquals("Apportionment Method Used is Based on Chargeable Units ($340.09)", 340.09M, job2CassChargeReloaded.JR_LocalCostAmt);
			}
			else
			{
				Fail("No test defined");
			}
		}

		public void TestAddConsolCostsWithProportionalApportionmentByChargeCodesHandlesVerySmallCASSCosts_UnPosted()
		{
			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;

			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			company.GC_IsGSTRegistered = true;

			SetupAssociatedBizos(false);
			Factory.Save();

			var consolCostToApplyCASSCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
			consolCostToApplyCASSCost.E6_LocalCostAmount = 500;
			consolCostToApplyCASSCost.E6_ApportionmentMethod = AllocationMethod.Manual;

			var consolCostToBeDeleted = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.MRG100, TestObjectCreator.AALSHI);
			consolCostToBeDeleted.E6_LocalCostAmount = 5;
			consolCostToBeDeleted.E6_ApportionmentMethod = AllocationMethod.Manual;

			Factory.Save();

			var testCassBilling = new CASSBilling(Factory);
			SetupCASSBilling(testCassBilling, 0);

			var cassBillingLine = testCassBilling.Lines.AddNew();
			TestObjectCreator.SetupCASSCostComponent(cassBillingLine, false, 0.007M, 0.005M, 0.003M, 0.003M, 0.001M, 0.001M);
			TestObjectCreator.SetupCASSCostComponent(cassBillingLine, true, 0M, 0M, 0M, 0M, 0M, 0M);

			testCassBilling.CreateInvoices();
			testCassBilling.PostAPTransactions();

			var apTrans = testCassBilling.APTransactions[0];
			AssertNoErrors(apTrans);
			AssertEquals("The invoice has just one line.", 1, apTrans.Lines.Count);
			AssertEquals("The invoice line has the correct amount.", -0.01M, apTrans.Lines[0].AL_LineAmount);

			var costs = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_AH_APInvoice, SQLComparisonOperator.NotEqual, ZGuid.Empty));
			AssertEquals("The less significant consol cost is removed, leaving 1 cost.", 1, costs.Length);
			Assert("The original cost (1) is still in the database", Array.Exists(costs, c => c.E6_AC_ChargeCode == TestObjectCreator.FRT.PK));
			Assert("The original cost (2) is deleted", !Array.Exists(costs, c => c.E6_AC_ChargeCode == TestObjectCreator.MRG100.PK));
			var cost = costs[0];
			AssertEquals("This consol cost amount is set to 0.01", 0.01M, cost.E6_LocalCostAmount);
			AssertEquals("We are updating the correct consol cost (FRT)", TestObjectCreator.FRT.AC_Code, cost.ChargeCode.AC_Code);
		}

		public void TestAddConsolCostsWithProportionalApportionmentByChargeCodesHandlesVerySmallCASSCosts_Posted()
		{
			var now = ZDateTime.Now;
			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;

			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			company.GC_IsGSTRegistered = true;

			SetupAssociatedBizos(false);
			Factory.Save();

			var consolCostFRT = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
			consolCostFRT.E6_LocalCostAmount = 500;
			consolCostFRT.E6_ApportionmentMethod = AllocationMethod.Manual;

			var consolCostMRG100 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.MRG100, TestObjectCreator.AALSHI);
			consolCostMRG100.E6_LocalCostAmount = 5;
			consolCostMRG100.E6_ApportionmentMethod = AllocationMethod.Manual;

			Factory.Save();

			var invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("001", TestObjectCreator.AUD, 1m, 0m, 0m, 0m, 0m, 0m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines.RemoveAndDeleteAll();
			var invoiceLine1 = TestObjectCreator.CreateAPInvoiceLine(invoice, Job1, TestObjectCreator.FRT, TestObjectCreator.AUD, 1m, "", 250m);
			var invoiceLine2 = TestObjectCreator.CreateAPInvoiceLine(invoice, Job2, TestObjectCreator.FRT, TestObjectCreator.AUD, 1m, "", 250m);
			consolCostFRT.E6_AH_APInvoice = invoice.PK;
			consolCostFRT.ApportionmentCharges[0].ReverseAccrual(now);
			consolCostFRT.ApportionmentCharges[1].ReverseAccrual(now);
			consolCostFRT.ApportionmentCharges[0].JR_AL_APLine = invoiceLine1.PK;
			consolCostFRT.ApportionmentCharges[1].JR_AL_APLine = invoiceLine2.PK;
			consolCostFRT.ApportionmentCharges[0].SetAmountsToLinkedLinesForTests();
			consolCostFRT.ApportionmentCharges[1].SetAmountsToLinkedLinesForTests();

			invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("002", TestObjectCreator.AUD, 1m, 0m, 0m, 0m, 0m, 0m, 0m, TestObjectCreator.AALSHI);
			invoice.Lines.RemoveAndDeleteAll();
			invoiceLine1 = TestObjectCreator.CreateAPInvoiceLine(invoice, Job1, TestObjectCreator.MRG100, TestObjectCreator.AUD, 1m, "", 2.5m);
			invoiceLine2 = TestObjectCreator.CreateAPInvoiceLine(invoice, Job2, TestObjectCreator.MRG100, TestObjectCreator.AUD, 1m, "", 2.5m);
			consolCostMRG100.E6_AH_APInvoice = invoice.PK;
			consolCostMRG100.ApportionmentCharges[0].ReverseAccrual(now);
			consolCostMRG100.ApportionmentCharges[1].ReverseAccrual(now);
			consolCostMRG100.ApportionmentCharges[0].JR_AL_APLine = invoiceLine1.PK;
			consolCostMRG100.ApportionmentCharges[1].JR_AL_APLine = invoiceLine2.PK;
			consolCostMRG100.ApportionmentCharges[0].SetAmountsToLinkedLinesForTests();
			consolCostMRG100.ApportionmentCharges[1].SetAmountsToLinkedLinesForTests();

			Factory.Save();

			var testCassBilling = new CASSBilling(Factory);
			SetupCASSBilling(testCassBilling, 0);

			var cassBillingLine = testCassBilling.Lines.AddNew();
			TestObjectCreator.SetupCASSCostComponent(cassBillingLine, false, 0.007M, 0.005M, 0.003M, 0.003M, 0.001M, 0.001M);
			TestObjectCreator.SetupCASSCostComponent(cassBillingLine, true, 0M, 0M, 0M, 0M, 0M, 0M);

			testCassBilling.CreateInvoices();
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			testCassBilling.PostAPTransactions();

			var apTrans = testCassBilling.APTransactions[0];
			AssertNoErrors(apTrans);
			AssertEquals("The invoice has just one line.", 1, apTrans.Lines.Count);
			AssertEquals("The invoice line has the correct amount.", -0.01M, apTrans.Lines[0].AL_LineAmount);

			var costs = Factory.Load<JobConsolCost>(new ZQuery());
			AssertEquals("In addition to the two existing consol costs, we have added a third one", 3, costs.Length);
			Assert("The original cost (1) is still in the database", Array.Exists(costs, c => c.PK == consolCostFRT.PK));
			Assert("The original cost (2) is still in the database", Array.Exists(costs, c => c.PK == consolCostMRG100.PK));

			var newCost = costs.Single(c => c.PK != consolCostFRT.PK && c.PK != consolCostMRG100.PK);
			AssertEquals("This consol cost amount is set to 0.01", 0.01M, newCost.E6_LocalCostAmount);
			AssertEquals("This consol cost has valid Charge Code", TestObjectCreator.FRT.AC_Code, newCost.ChargeCode.AC_Code);
		}

		public void TestAddConsolCostsWithEquivalentApportionmentByChargeCodesHandlesVerySmallCASSCosts()
		{
			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;

			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			company.GC_IsGSTRegistered = true;

			SetupAssociatedBizos(false);
			Factory.Save();

			var testCassBilling = new CASSBilling(Factory);
			SetupCASSBilling(testCassBilling, 0);
			testCassBilling.ForceRecalculateData();

			var cassBillingLine = testCassBilling.Lines.AddNew();
			TestObjectCreator.SetupCASSCostComponent(cassBillingLine, false, 0.007M, 0.005M, 0.003M, 0.003M, 0.001M, 0.001M);
			TestObjectCreator.SetupCASSCostComponent(cassBillingLine, true, 0M, 0M, 0M, 0M, 0M, 0M);

			testCassBilling.CreateInvoices();
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			testCassBilling.PostAPTransactions();

			var apTrans = testCassBilling.APTransactions[0];
			AssertNoErrors(apTrans);
			AssertEquals("The invoice has just one line.", 1, apTrans.Lines.Count);
			AssertEquals("The invoice line has the correct amount.", -0.01M, apTrans.Lines[0].AL_LineAmount);

			var costs = Factory.Load<JobConsolCost>(new ZQuery());
			AssertEquals("A new consol cost is created", 1, costs.Length);
			AssertEquals("This consol cost amount is set to 0.01", 0.01M, costs[0].E6_LocalCostAmount);
		}

		public void TestAddConsolCostsWithProportionalApportionmentByChargeCodesHandlesVerySmallCASSCosts_OSCostAmountNonZeroButLocalCostAmountZero()
		{
			TestObjectCreator.CreateExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "IQD"), 100);

			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;

			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			company.GC_IsGSTRegistered = true;

			SetupAssociatedBizos(false);
			Factory.Save();

			var consolCostToApplyCASSCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
			consolCostToApplyCASSCost.E6_LocalCostAmount = 500;
			consolCostToApplyCASSCost.E6_ApportionmentMethod = AllocationMethod.Manual;
			var consolCostToBeDeleted = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.MRG100, TestObjectCreator.AALSHI);
			consolCostToBeDeleted.E6_LocalCostAmount = 5;
			consolCostToBeDeleted.E6_ApportionmentMethod = AllocationMethod.Manual;
			Factory.Save();

			var testCassBilling = new CASSBilling(Factory);
			SetupCASSBilling(testCassBilling, 0);

			var cassBillingLine = testCassBilling.Lines.AddNew();
			TestObjectCreator.SetupCASSCostComponent(cassBillingLine, false, 0.007M, 0.005M, 0.003M, 0.003M, 0.001M, 0.001M, currency: "IQD");
			testCassBilling.CreateInvoices();
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			testCassBilling.PostAPTransactions();
			AssertEquals("The consol costs are removed, so no lines on the invoice, so the invoice is not created.", 0, testCassBilling.APTransactions.Count);
		}

		public void TestInvoicePostingWhenThereAreMultipleConsolCostWithSameChargeCodeAndCreditor()
		{
			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
			using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
			{
				GlbCompany company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
				company.GC_IsGSTRegistered = true;

				SetupAssociatedBizos(false);

				Shipment1.JS_UnitOfWeight = Shipment2.JS_UnitOfWeight = Constants.Weight.Kilograms;
				Shipment1.JS_UnitOfVolume = Shipment2.JS_UnitOfVolume = Constants.Volume.CubicMetres;
				Shipment1.JS_ActualWeight = 990;
				Shipment2.JS_ActualWeight = 10;
				Shipment1.JS_ActualVolume = 1000;
				Shipment2.JS_ActualVolume = 1000;
				Factory.Save();

				TestObjectCreator.FRT.AC_Code = "FRT Charge";
				var cost1 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
				cost1.E6_LocalCostAmount = 500;
				cost1.E6_ApportionmentMethod = AllocationMethod.GrossWeight;

				var cost2 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, null);
				cost2.E6_LocalCostAmount = 600;
				cost2.E6_ApportionmentMethod = AllocationMethod.Manual;
				cost2.E6_InvoiceNum = Consol.JK_UniqueConsignRef + "_001";
				cost2.E6_InvoiceDate = DateTime.Today;
				cost2.E6_PaymentDate = DateTime.Today;

				var cost3 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, null);
				cost3.E6_LocalCostAmount = 700;
				cost3.E6_ApportionmentMethod = AllocationMethod.Manual;
				cost3.E6_InvoiceNum = Consol.JK_UniqueConsignRef + "_002";
				cost3.E6_InvoiceDate = DateTime.Today;
				cost3.E6_PaymentDate = DateTime.Today;

				var cost4 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
				cost4.E6_LocalCostAmount = 15000;
				cost4.E6_ApportionmentMethod = AllocationMethod.Shipment;

				var cost5 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, null);
				cost5.E6_LocalCostAmount = 150;
				cost5.E6_ApportionmentMethod = AllocationMethod.Manual;

				var cost6 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
				cost6.E6_LocalCostAmount = 15000;
				cost6.E6_ApportionmentMethod = AllocationMethod.Shipment;

				Factory.Save();

				var factory2 = new BusinessObjectFactory();
				var testCassBilling = new CASSBilling(factory2);
				SetupCASSBilling(testCassBilling, 1, setAdjustmentValues: false);
				testCassBilling.CreateInvoices();
				AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
				testCassBilling.PostAPTransactions();

				var factory3 = new BusinessObjectFactory();
				var job1Reloaded = factory3.Load<Job>(Consol.Shipments[0].Job.PK);
				var job2Reloaded = factory3.Load<Job>(Consol.Shipments[1].Job.PK);

				AssertEquals("First job has 6 charges", 6, job1Reloaded.Charges.Count);
				AssertEquals("Second job has 6 charges", 6, job2Reloaded.Charges.Count);

				var apps = new ApportionmentListing(factory3, Consol);
				AssertEquals("Consol Cost Count", 6, apps.CostsCollection.Count);
				AssertEquals("Cots Posted", true, apps.CostsCollection[0].IsPosted);
				AssertEquals("Cots Posted", false, apps.CostsCollection[1].IsPosted);
				AssertEquals("Cots Posted", false, apps.CostsCollection[2].IsPosted);
				AssertEquals("Cots Posted", false, apps.CostsCollection[3].IsPosted);
				AssertEquals("Cots Posted", false, apps.CostsCollection[4].IsPosted);
				AssertEquals("Cots Posted", false, apps.CostsCollection[5].IsPosted);
			}
		}

		void AssertCompanyHasNoTaxRates(GlbCompany company)
		{
			AccTaxRateCollection taxRates = new AccTaxRateCollection(Factory, new ZQuery(AccTaxRateSchema.AT_RN_NKCountry, company.GC_RN_NKCountryCode));
			taxRates.Load();
			AssertEquals(company.GC_Name + " is not expected to have tax rates ", 0, taxRates.Count);
		}

		[TestDate(2017, 09, 10)]
		public void TestInitialize()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var threeMonthsAgo = today.AddMonths(-3);

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			var newline = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			cassCostHeader.Lines.Add(newline);
			newline.RecordType = "AWB";
			newline.AirlinePrefix = "172";
			newline.AWBSerialNumber = "67828073";
			newline.AgentCode = "23470/068-510";
			newline.DateAWBExecution = threeMonthsAgo;
			newline.DateOfArrival = threeMonthsAgo;
			newline.DateOfDelivery = threeMonthsAgo;
			newline.Origin = "LEJ";
			newline.Destination = "MEX";

			var newline2 = new CASSCostExportLine(Factory, CASSCostLineType.Default);
			cassCostHeader.Lines.Add(newline2);
			newline2.RecordType = "AWB";
			newline2.AirlinePrefix = "172";
			newline2.AWBSerialNumber = "67828073";
			newline2.AgentCode = "23470/068-510";
			newline2.DateAWBExecution = threeMonthsAgo;
			newline2.DateOfArrival = threeMonthsAgo;
			newline2.DateOfDelivery = threeMonthsAgo;
			newline2.Origin = "LEJ";
			newline2.Destination = "MEX";

			CASSBilling cassBilling = new CASSBilling(Factory);
			AssertEquals(false, cassBilling.IsImportBilling);
			AssertEquals(false, cassBilling.IsExportBilling);
			AssertNull(cassBilling.CostHeader.Lines);

			cassBilling.Initialize(TestCASSBilling.CostHeader);

			AssertEquals(false, cassBilling.IsImportBilling);
			AssertEquals(true, cassBilling.IsExportBilling);
			AssertEquals("TestCASS.hot", cassBilling.HOTFileName);
			AssertEquals(cassCostHeader.DatePeriodStart, cassBilling.BillingPeriodStart);
			AssertEquals(cassCostHeader.DatePeriodEnd, cassBilling.BillingPeriodEnd);
			AssertEquals(cassCostHeader.DateOfBilling, cassBilling.BillingDate);

			AssertEquals(2, cassBilling.CostHeader.Lines.Count);

			var costLine = cassBilling.CostHeader.ExportLines[0];
			AssertEquals("172", costLine.AirlinePrefix);
			AssertEquals("67828073", costLine.AWBSerialNumber);
			AssertEquals("23470/068-510", costLine.AgentCode);
			AssertEquals(threeMonthsAgo, costLine.DateAWBExecution);
			AssertEquals(threeMonthsAgo, costLine.DateOfArrival);
			AssertEquals(threeMonthsAgo, costLine.DateOfDelivery);
			AssertEquals("LEJ", costLine.Origin);
			AssertEquals("MEX", costLine.Destination);

			costLine = cassBilling.CostHeader.ExportLines[1];
			AssertEquals("172", costLine.AirlinePrefix);
			AssertEquals("67828073", costLine.AWBSerialNumber);
			AssertEquals("23470/068-510", costLine.AgentCode);
			AssertEquals(threeMonthsAgo, costLine.DateAWBExecution);
			AssertEquals(threeMonthsAgo, costLine.DateOfArrival);
			AssertEquals(threeMonthsAgo, costLine.DateOfDelivery);
			AssertEquals("LEJ", costLine.Origin);
			AssertEquals("MEX", costLine.Destination);
		}

		[TestDate(2017, 09, 10)]
		public void TestHiddenLines()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.RecordType = isAdjustment ? "DCO" : "AWM";
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = "67828073";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "EUR";
						costLine.WeightChargePP = isAdjustment ? 1015.68M : (i == 0 ? 680.18M : 900.00M);
						costLine.VATDueAirline = isAdjustment ? 101.56M : 68.01M;
						cassBillingLine.AddCostLine(costLine, "AUD");
					}
				}
			}

			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();
			Factory.Save();

			AssertEquals("Precondition:", 2, TestCASSBilling.Lines.Count);

			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 400);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.Lines.Count);
			AssertEquals(680.18m, TestCASSBilling.Lines[0].CASSCostValue);
			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);

			transaction.Validation.ValidateAll();
			AssertNoRowErrors(transaction);

			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APInvoice);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));

			AssertEquals(6, transaction.Lines.Count);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[0].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[1].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[2].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[3].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[4].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[5].ApportionmentChargeImportedFrom);

			TestCASSBilling.RunPreSaveValidation();
			AssertNoErrors(TestCASSBilling);
			TestCASSBilling.APTransactions.Factory.Save();

			AssertEquals("FRT", transaction.Lines[0].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[1].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[2].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[3].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[4].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[5].ChargeCode.AC_Code);
			if (Job1.PK == transaction.Lines[0].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[1].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[0].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[1].AL_JH);
			}
			if (Job1.PK == transaction.Lines[2].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[3].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[2].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[3].AL_JH);
			}
			if (Job1.PK == transaction.Lines[4].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[5].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[4].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[5].AL_JH);
			}
			AssertEquals(-374.09M, transaction.Lines[0].AL_OSAmount);
			AssertEquals(-374.10M, transaction.Lines[1].AL_OSAmount);
			AssertEquals(-484M, transaction.Lines[2].AL_OSAmount);
			AssertEquals(-484.01M, transaction.Lines[3].AL_OSAmount);
			AssertEquals(558.62M, transaction.Lines[4].AL_OSAmount);
			AssertEquals(558.62M, transaction.Lines[5].AL_OSAmount);
		}

		[TestDate(2017, 09, 10)]
		public void TestHiddenLinesWithErrors()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.RecordType = isAdjustment ? "DCO" : "AWM";
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = "67828073";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "EUR";
						costLine.WeightChargePP = isAdjustment ? 1015.68M : (i == 0 ? 680.18M : 900.00M);
						costLine.VATDueAirline = isAdjustment ? 101.56M : 68.01M;
						cassBillingLine.AddCostLine(costLine, "AUD");
					}
				}
			}

			SetupAssociatedBizos();
			Factory.Save();

			AssertEquals("Precondition:", 2, TestCASSBilling.Lines.Count);

			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 400);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			TestCASSBilling.ForceRecalculateData();

			Assert("Precondition: must not have any notifications.", !TestCASSBilling.Lines[0].HasNotifications());
			Assert("Precondition: must not have any row notifications.", !TestCASSBilling.Lines[0].HasRowNotifications);

			AssertEquals("Only line that exceed discrepancy must be shown.", 1, TestCASSBilling.Lines.Count);
			AssertEquals(-480.18M, TestCASSBilling.Lines[0].CostDifference);

			ZString validAirlinePrefix = TestCASSBilling.HiddenLines[0].AirlinePrefix;
			CASSBillingLine hiddenLine = TestCASSBilling.HiddenLines[0];

			var costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
			costLine1.RecordType = "AWM";
			costLine1.VATIndicator = "Y";
			costLine1.AirlinePrefix = "0";
			costLine1.AWBSerialNumber = "67828073";
			costLine1.AgentCode = "23470/068-510";
			costLine1.DateAWBExecution = threeMonthsAgo;
			costLine1.DateOfArrival = twoMonthsAgo;
			costLine1.DateOfDelivery = oneMonthAgo;
			costLine1.Origin = "LEJ";
			costLine1.Destination = "MEX";
			costLine1.Weight = 2150M;
			costLine1.WeightUnit = "KG";
			costLine1.CurrencyCode = "EUR";
			costLine1.WeightChargePP = 0;
			costLine1.VATDueAirline = 0;
			TestCASSBilling.HiddenLines[0].AddCostLine(costLine1, "AUD");

			Assert("Precondition: must have error", hiddenLine.HasErrors());
			TestCASSBilling.ForceRecalculateData();
			AssertEquals("Line with validation error also must be shown.", 2, TestCASSBilling.Lines.Count);

			costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
			costLine1.RecordType = "AWM";
			costLine1.VATIndicator = "Y";
			costLine1.AirlinePrefix = validAirlinePrefix;
			costLine1.AWBSerialNumber = "67828073";
			costLine1.AgentCode = "23470/068-510";
			costLine1.DateAWBExecution = threeMonthsAgo;
			costLine1.DateOfArrival = twoMonthsAgo;
			costLine1.DateOfDelivery = oneMonthAgo;
			costLine1.Origin = "LEJ";
			costLine1.Destination = "MEX";
			costLine1.Weight = 2150M;
			costLine1.WeightUnit = "KG";
			costLine1.CurrencyCode = "EUR";
			costLine1.WeightChargePP = 0;
			costLine1.VATDueAirline = 0;
			TestCASSBilling.Lines[1].AddCostLine(costLine1, "AUD");

			Assert("Precondition: must not have errors.", !hiddenLine.HasErrors());
			TestCASSBilling.ForceRecalculateData();
			AssertEquals("Only line that exceed discrepancy must be shown.", 1, TestCASSBilling.Lines.Count);
			AssertEquals(-480.18M, TestCASSBilling.Lines[0].CostDifference);

			foreach (bool isAdjustment in new bool[] { false, true })
			{
				costLine1 = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
				costLine1.RecordType = isAdjustment ? "DCO" : "AWM";
				costLine1.VATIndicator = "Y";
				costLine1.AirlinePrefix = validAirlinePrefix;
				costLine1.AWBSerialNumber = "67828073";
				costLine1.AgentCode = "23470/068-510";
				costLine1.DateAWBExecution = threeMonthsAgo;
				costLine1.DateOfArrival = twoMonthsAgo;
				costLine1.DateOfDelivery = oneMonthAgo;
				costLine1.Origin = "LEJ";
				costLine1.Destination = "MEX";
				costLine1.Weight = 2150M;
				costLine1.WeightUnit = "KG";
				costLine1.CurrencyCode = "EUR";
				costLine1.WeightChargePP = isAdjustment ? hiddenLine.CASSCostAdjustedValue : (ZDecimal)(hiddenLine.CASSCostValue * (-1));
				costLine1.VATDueAirline = isAdjustment ? hiddenLine.CASSCostTaxAdjustedValue : (ZDecimal)(hiddenLine.CASSCostTaxValue * (-1));
				TestCASSBilling.HiddenLines[0].AddCostLine(costLine1, "AUD");
			}

			Assert("Precondition: must not have errors.", !TestCASSBilling.HiddenLines[0].HasErrors());
			Assert("Precondition: must have row warnings.", TestCASSBilling.HiddenLines[0].HasRowWarnings);
			TestCASSBilling.ForceRecalculateData();
			AssertEquals("Line with row warnings also must be shown.", 2, TestCASSBilling.Lines.Count);
		}

		/// <summary>
		/// Setup 1 CASS billing line with no validation errors
		/// </summary>
		void SetupCASSLineWithNoErrors()
		{
			var org = TestObjectCreator.ABIGAS;
			org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
			org.OH_IsCreditor = true;
			Factory.Save();

			CASSBillingLine cassLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestObjectCreator.SetupCASSCostComponent(cassLine, true, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");

			TestCASSBilling.Lines.Add(cassLine);
		}

		[TestDate(2017, 09, 10)]
		public void TestCanStillImportIfSomeInvoicesHaveErrors()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.RecordType = isAdjustment ? "DCO" : "AWM";
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = i == 0 ? "67828073" : "12345678";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "EUR";
						costLine.WeightChargePP = isAdjustment ? 101568M : (i == 0 ? 68018M : 90000M);
						costLine.VATDueAirline = isAdjustment ? 10156M : 6801M;
						cassBillingLine.AddCostLine(costLine, "AUD");
					}
				}
			}

			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();

			SetupCASSLineWithNoErrors();
			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals("3 CASS lines", 3, TestCASSBilling.Lines.Count);
			AssertEquals("should be 2 invoices ready for posting", 2, TestCASSBilling.APTransactions.Count);
			foreach (InvoicingBase invoice in TestCASSBilling.APTransactions)
			{
				AssertEquals("IsPostedToCASS should be false", false, invoice.IsPostedToCASSOrSaved);
			}
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			TestCASSBilling.PostAPTransactions();

			var query = new ZQuery();
			query.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, TestCASSBilling.APTransactions[0].AH_TransactionType);
			query.AddToFilter(AccTransactionHeaderSchema.AH_GC, TestCASSBilling.APTransactions[0].AH_GC);
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var invoicingbases = newFactory.Load<InvoicingBase>(query);

			foreach (InvoicingBase invoiceBase in TestCASSBilling.APTransactions)
			{
				AssertEquals("IsPostedToCASSOrSaved", !invoiceBase.HasErrors, invoiceBase.IsPostedToCASSOrSaved);
			}

			AssertEquals("1 Invoice should get saved to the database because 1 of the 2 invoices have errors", 1, invoicingbases.Length);
		}

		[TestDate(2017, 09, 10)]
		public void TestIsPostedColumnMaintainsStateOnRefresh()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			TestObjectCreator.CreateExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "EUR"), 1);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();

				var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
				costLine.RecordType = "AWM";
				costLine.VATIndicator = "Y";
				costLine.AirlinePrefix = "172";
				costLine.AWBSerialNumber = "67828073";
				costLine.AgentCode = "23470/068-510";
				costLine.DateAWBExecution = threeMonthsAgo;
				costLine.DateOfArrival = twoMonthsAgo;
				costLine.DateOfDelivery = oneMonthAgo;
				costLine.Origin = "LEJ";
				costLine.Destination = "MEX";
				costLine.Weight = 2150M;
				costLine.WeightUnit = "KG";
				costLine.CurrencyCode = i == 0 ? "AUD" : "EUR";
				costLine.WeightChargePP = 68018M;
				costLine.VATDueAirline = 6801M;
				cassBillingLine.AddCostLine(costLine, costLine.CurrencyCode);
			}
			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();

			var org = TestObjectCreator.ABIGAS;
			org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
			org.OH_IsCreditor = true;
			Factory.Save();

			var cassBillingLine1 = TestCASSBilling.Lines.AddNew();
			var costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
			costLine1.RecordType = "AWM";
			costLine1.VATIndicator = "Y";
			costLine1.AirlinePrefix = "081";
			costLine1.AWBSerialNumber = "67828074";
			costLine1.AgentCode = "23470/068-510";
			costLine1.DateAWBExecution = threeMonthsAgo;
			costLine1.DateOfArrival = twoMonthsAgo;
			costLine1.DateOfDelivery = oneMonthAgo;
			costLine1.Origin = "LEJ";
			costLine1.Destination = "MEX";
			costLine1.Weight = 210M;
			costLine1.WeightUnit = "KG";
			costLine1.CurrencyCode = "AUD";
			costLine1.WeightChargePP = 68018M;
			costLine1.VATDueAirline = 6801M;
			cassBillingLine1.AddCostLine(costLine1, "AUD");

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();  //refresh
			AssertCASSGstRegistryAndCallCreateInvoices(); //Calculate Invoices

			AssertEquals(3, TestCASSBilling.APTransactions.Count);
			AssertEquals(false, TestCASSBilling.AreAnyTransactionsPosted);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			TestCASSBilling.PostAPTransactions();
			List<string> invoiceList = new List<string>();
			for (int i = 0; i < TestCASSBilling.APTransactions.Count; i++)
			{
				AssertEquals(!TestCASSBilling.APTransactions[i].HasErrors, TestCASSBilling.APTransactions[i].IsPostedToCASSOrSaved);
				invoiceList.Add(TestCASSBilling.APTransactions[i].AH_TransactionNum);
			}
			AssertEquals(true, TestCASSBilling.AreAnyTransactionsPosted);

			TestCASSBilling.ForceRecalculateData();  //refresh

			costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
			costLine1.RecordType = "AWM";
			costLine1.VATIndicator = "Y";
			costLine1.AirlinePrefix = "172";
			costLine1.AWBSerialNumber = "67828073";
			costLine1.AgentCode = "23470/068-510";
			costLine1.DateAWBExecution = threeMonthsAgo;
			costLine1.DateOfArrival = twoMonthsAgo;
			costLine1.DateOfDelivery = oneMonthAgo;
			costLine1.Origin = "LEJ";
			costLine1.Destination = "MEX";
			costLine1.Weight = 2150M;
			costLine1.WeightUnit = "KG";
			costLine1.CurrencyCode = "AUD";
			costLine1.WeightChargePP = 0M;
			costLine1.VATDueAirline = 0M;
			TestCASSBilling.Lines[1].AddCostLine(costLine1, "AUD"); //user fixes the error

			TestCASSBilling.CreateInvoices(); //Calculate Invoices again

			for (int i = 0; i < TestCASSBilling.APTransactions.Count; i++)
			{
				if (invoiceList.Contains(TestCASSBilling.APTransactions[i].AH_TransactionNum))
				{
					AssertEquals("business entity maintains tracking of posted invoices", true, TestCASSBilling.APTransactions[i].IsPostedToCASSOrSaved);
				}
				else
				{
					AssertEquals("AP Invoice is not yet posted", false, TestCASSBilling.APTransactions[i].IsPostedToCASSOrSaved);
				}
			}
			AssertEquals(true, TestCASSBilling.AreAnyTransactionsPosted);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			TestCASSBilling.PostAPTransactions();

			Assert("no errors", TestCASSBilling.APTransactions.All(x => !x.HasErrors));
		}

		public void TestAddInvoiceLineForJobWithProportionalApportionmentByChargeCodesForNonGSTCompany()
		{
			GlbCompany companyWithNoTaxRates = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			companyWithNoTaxRates.GC_IsGSTRegistered = false;
			AssertCompanyHasNoTaxRates(companyWithNoTaxRates);
			GlbBranch branchOnCompanyWithNoTaxRate = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, companyWithNoTaxRates.PK));

			using (branchOnCompanyWithNoTaxRate.SetAsTemporaryContext())
			{
				using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid() }.Set())
				{
					SetupCASSBilling(TestCASSBilling, 1);
					SetupAssociatedBizos();
					Factory.Save();

					TestCASSBilling.ForceRecalculateData();
					TestCASSBilling.CreateInvoices();
				}
			}
		}

		public void TestCreateInvoicesForGatewayBillingWithGTT()
		{
			using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid() }.Set())
			{
				SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: false);
				(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;
				(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).VATDueAirline = 68.01M;

				SetupAssociatedBizos();
				SetupGatewayOnConsol();
				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				AssertEquals(1, TestCASSBilling.APTransactions.Count);
				InvoicingBase transaction = TestCASSBilling.APTransactions[0];
				AssertNotNull(transaction as APInvoice);

				transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
				AssertNotNull(transaction as APInvoice);
				Assert(transaction.Factory.HasContext(BusinessContext.CASS));

				AssertEquals(1, transaction.Lines.Count);
				AssertNull("There is no Apportionment Charge associated with the line.", transaction.Lines[0].ApportionmentChargeImportedFrom);

				TestCASSBilling.RunPreSaveValidation();
				AssertNoErrors(TestCASSBilling);
				TestCASSBilling.APTransactions.Factory.Save();

				AssertEquals("FRT", transaction.Lines[0].ChargeCode.AC_Code);
				AssertEquals(GatewayBillingJob.PK, transaction.Lines[0].AL_JH);
				AssertEquals(-748.19M, transaction.Lines[0].AL_OSAmount);
				AssertEquals(-680.18M, transaction.Lines[0].AL_LineAmount);
				AssertEquals(-68.01M, transaction.Lines[0].AL_GSTVAT);

				var costLine = TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine;
				costLine.VATDueAgent = costLine.VATDueAirline = 0M;
				TestCASSBilling.ForceRecalculateData();
				TestCASSBilling.CreateInvoices();

				AssertEquals(1, TestCASSBilling.APTransactions.Count);
				transaction = TestCASSBilling.APTransactions[0];
				AssertNotNull(transaction as APInvoice);

				transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
				AssertNotNull(transaction as APInvoice);
				Assert(transaction.Factory.HasContext(BusinessContext.CASS));

				AssertEquals(1, transaction.Lines.Count);
				AssertNull("There is no Apportionment Charge associated with the line.", transaction.Lines[0].ApportionmentChargeImportedFrom);

				TestCASSBilling.RunPreSaveValidation();
				AssertNoErrors(TestCASSBilling);
				TestCASSBilling.APTransactions.Factory.Save();

				AssertEquals("FRT", transaction.Lines[0].ChargeCode.AC_Code);
				AssertEquals(GatewayBillingJob.PK, transaction.Lines[0].AL_JH);
				AssertEquals(-680.18M, transaction.Lines[0].AL_OSAmount);
				AssertEquals(-680.18M, transaction.Lines[0].AL_LineAmount);
				AssertEquals(0M, transaction.Lines[0].AL_GSTVAT);
			}
		}

		public void TestCreateInvoicesForGatewayBillingWithGTT_HasConsolCost_NoJobCharges()
		{
			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);

			using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid() }.Set())
			{
				SetupAssociatedBizos(false);
				TestObjectCreator.FRT.AC_DepartmentFilterList = "ALL";

				var currentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
				var nonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));

				AssertEquals("sanity check", GlbCompany.CurrentCompany.PK, currentCompanyBranch.GB_GC);

				var currentCompanyOrgProxyAddressPK = currentCompanyBranch.OrgProxy.MainAddress.PK;
				var nonCurrentCompanyOrgProxyAddressPK = nonCurrentCompany.OrgProxy.MainAddress.PK;

				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_RL_NKLoadPort = "AUBNE";
				Consol.JK_RL_NKDischargePort = "SGSIN";
				Consol.JK_OA_SendingForwarderAddress = currentCompanyOrgProxyAddressPK;
				Consol.JK_OA_ReceivingForwarderAddress = nonCurrentCompanyOrgProxyAddressPK;

				var port1 = Consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				port1.O5_SeaAirCarrierOrForwarderType = "GTW";
				port1.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
				port1.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				port1.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				var port2 = Consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
				port2.O5_SeaAirCarrierOrForwarderType = "GTW";
				port2.O5_PortOrCountry = Consol.JK_RL_NKDischargePort;
				port2.O5_OA_AgentOfficeAddress = Consol.JK_OA_ReceivingForwarderAddress;
				port2.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				Consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				Assert(Consol.IsGatewayBillingEnabled());
				Assert(Consol.IsGatewayBillingEnabled(nonCurrentCompany));
				new JobHeader.Loader(Factory, Consol.Shipments.First() as ForwardingShipment).TryLoadOrCreateWithoutMutexForTestOnly();

				GatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
				GatewayBillingJob.JH_GE = GatewayDepartmentPK;

				var consolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, 1000, false);
				AssertNotNull(consolCost.ApportionmentCharges);
				AssertEquals(2, consolCost.ApportionmentCharges.Count);
				Factory.Save();

				SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: false);
				(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;

				TestCASSBilling.ForceRecalculateData();

				AssertEquals(0, TestCASSBilling.APTransactions.Count);
				AssertEquals(0, GatewayBillingJob.Charges.Count);
				var charges = new BusinessObjectFactory().Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, GatewayBillingJob.PK));
				AssertEquals(0, charges.Length);
				Assert(!consolCost.IsPosted);

				TestCASSBilling.PostAPTransactions();

				charges = new BusinessObjectFactory().Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, GatewayBillingJob.PK));
				AssertEquals("Local agent is GTT so costs imported as gateway billing job charges", 1, charges.Length);
				Assert("cost side of the job billing charge should be posted", charges[0].IsCostPosted);
				Assert("consol cost should not be posted", !consolCost.IsPosted);
				Assert("apportioned charge should not be posted", consolCost.ApportionmentCharges.Cast<ApportionSplitCharge>().All(x => !x.IsCostPosted));
			}
		}

		public void TestCreateInvoicesForGatewayBillingWithGTTandGTA()
		{
			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);

			using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid() }.Set())
			{
				SetupAssociatedBizos();
				TestObjectCreator.FRT.AC_DepartmentFilterList = "ALL";

				var currentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
				var nonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));

				AssertEquals("sanity check", GlbCompany.CurrentCompany.PK, currentCompanyBranch.GB_GC);

				var currentCompanyOrgProxyAddressPK = currentCompanyBranch.OrgProxy.MainAddress.PK;
				var nonCurrentCompanyOrgProxyAddressPK = nonCurrentCompany.OrgProxy.MainAddress.PK;

				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_RL_NKLoadPort = "AUBNE";
				Consol.JK_RL_NKDischargePort = "SGSIN";
				Consol.JK_OA_SendingForwarderAddress = currentCompanyOrgProxyAddressPK;
				Consol.JK_OA_ReceivingForwarderAddress = nonCurrentCompanyOrgProxyAddressPK;

				var port1 = Consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				port1.O5_SeaAirCarrierOrForwarderType = "GTW";
				port1.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
				port1.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				port1.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				var port2 = Consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
				port2.O5_SeaAirCarrierOrForwarderType = "GTW";
				port2.O5_PortOrCountry = Consol.JK_RL_NKDischargePort;
				port2.O5_OA_AgentOfficeAddress = Consol.JK_OA_ReceivingForwarderAddress;
				port2.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				Consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				Assert(Consol.IsGatewayBillingEnabled());
				Assert(Consol.IsGatewayBillingEnabled(nonCurrentCompany));
				new JobHeader.Loader(Factory, Consol.Shipments.First() as ForwardingShipment).TryLoadOrCreateWithoutMutexForTestOnly();

				GatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
				GatewayBillingJob.JH_GE = GatewayDepartmentPK;

				Factory.Save();

				SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: false);
				(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;

				TestCASSBilling.ForceRecalculateData();

				AssertEquals(0, TestCASSBilling.APTransactions.Count);
				Assert(!Consol.HasConsolCosts(GlbCompany.CurrentCompany));
				AssertEquals(0, GatewayBillingJob.Charges.Count);

				var charges = new BusinessObjectFactory().Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, GatewayBillingJob.PK));
				AssertEquals(0, charges.Length);

				TestCASSBilling.PostAPTransactions();

				charges = new BusinessObjectFactory().Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, GatewayBillingJob.PK));
				AssertEquals("Local agent is GTT so costs imported as gateway billing job charges", 2, charges.Length);
				Assert(!Consol.HasConsolCosts(GlbCompany.CurrentCompany));
			}
		}

		public void TestCreateInvoicesForGatewayBillingWithGTA()
		{
			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);

			using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid() }.Set())
			{
				SetupAssociatedBizos();
				TestObjectCreator.FRT.AC_DepartmentFilterList = "ALL";

				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_RL_NKLoadPort = "AUBNE";
				Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				var port = Consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				port.O5_SeaAirCarrierOrForwarderType = "GTW";
				port.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
				port.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				port.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

				Assert(Consol.IsGatewayBillingEnabled());
				new JobHeader.Loader(Factory, Consol.Shipments.First() as ForwardingShipment).TryLoadOrCreateWithoutMutexForTestOnly();

				Factory.Save();

				SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: false);
				(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;

				TestCASSBilling.ForceRecalculateData();

				AssertEquals(0, TestCASSBilling.APTransactions.Count);
				Assert(!Consol.HasConsolCosts(GlbCompany.CurrentCompany));

				TestCASSBilling.PostAPTransactions();

				Assert(Consol.HasConsolCosts(GlbCompany.CurrentCompany));
				AssertEquals(1, TestCASSBilling.APTransactions.Count);
			}
		}

		public void TestCreateInvoicesForGatewayBillingWithGTAandGTT()
		{
			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);

			using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid() }.Set())
			{
				SetupAssociatedBizos();
				TestObjectCreator.FRT.AC_DepartmentFilterList = "ALL";

				var currentCompanyBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE"));
				var nonCurrentCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));

				AssertEquals("sanity check", GlbCompany.CurrentCompany.PK, currentCompanyBranch.GB_GC);

				var currentCompanyOrgProxyAddressPK = currentCompanyBranch.OrgProxy.MainAddress.PK;
				var nonCurrentCompanyOrgProxyAddressPK = nonCurrentCompany.OrgProxy.MainAddress.PK;

				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_RL_NKLoadPort = "AUBNE";
				Consol.JK_RL_NKDischargePort = "SGSIN";
				Consol.JK_OA_SendingForwarderAddress = currentCompanyOrgProxyAddressPK;
				Consol.JK_OA_ReceivingForwarderAddress = nonCurrentCompanyOrgProxyAddressPK;

				var port1 = Consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				port1.O5_SeaAirCarrierOrForwarderType = "GTW";
				port1.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
				port1.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				port1.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

				var port2 = Consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
				port2.O5_SeaAirCarrierOrForwarderType = "GTW";
				port2.O5_PortOrCountry = Consol.JK_RL_NKDischargePort;
				port2.O5_OA_AgentOfficeAddress = Consol.JK_OA_ReceivingForwarderAddress;
				port2.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				Consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				Assert(Consol.IsGatewayBillingEnabled());
				new JobHeader.Loader(Factory, Consol.Shipments.First() as ForwardingShipment).TryLoadOrCreateWithoutMutexForTestOnly();

				GatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
				GatewayBillingJob.JH_GE = GatewayDepartmentPK;

				Factory.Save();

				SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: false);
				(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;

				TestCASSBilling.ForceRecalculateData();

				AssertEquals(0, TestCASSBilling.APTransactions.Count);
				Assert(!Consol.HasConsolCosts(GlbCompany.CurrentCompany));
				var charges = new BusinessObjectFactory().Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, GatewayBillingJob.PK));
				AssertEquals(0, charges.Length);

				TestCASSBilling.PostAPTransactions();

				Assert("Local agent is GTA so costs imported as as consol costs", Consol.HasConsolCosts(GlbCompany.CurrentCompany));
				AssertEquals(1, TestCASSBilling.APTransactions.Count);
				charges = new BusinessObjectFactory().Load<JobCharge>(new ZQuery(JobChargeSchema.JR_JH, GatewayBillingJob.PK));
				AssertEquals(0, charges.Length);
			}
		}

		public void TestCreateInvoicesForGatewayBillingWithNegativeAccrual()
		{
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G"));

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);
				using (AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: true);
					SetupAssociatedBizos();
					Factory.Save();

					Consol.JK_AgentType = Constants.AgentType.Agent;
					Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
					Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
					var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
					port.O5_SeaAirCarrierOrForwarderType = "GTW";
					port.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
					port.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
					port.O5_AgentDirection = AgentDirectionList.Codes.Both;
					port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
					Consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

					var gatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
					var gatewayCost = gatewayBillingJob.Charges.AddNew();
					gatewayCost.JR_AC = TestObjectCreator.FRT.PK;
					gatewayCost.JR_OH_CostAccount = Consol.SendingForwarderPK;
					gatewayCost.JR_RX_NKCostCurrency = "AUD";
					gatewayCost.JR_OSCostAmt = -200m;
					Factory.Save();

					TestCASSBilling.ForceRecalculateData();
					AssertEquals("Precondition: Negative accrual value", -200M, TestCASSBilling.Lines[0].SystemCostAccrualValue);
					AssertEquals("Precondition: Consol is gateway consol", true, TestCASSBilling.Lines[0].IsForGatewayBilling);
					AssertEquals("Precondition: Consol has gateway consol job", gatewayBillingJob.PK, TestCASSBilling.Lines[0].GatewayBillingJob.PK);

					TestCASSBilling.ForceRecalculateData();
					AssertNoExceptionThrown("No Exception should be thrown", () => TestCASSBilling.CreateInvoices());
					AssertEquals("Invoice should be created", 1, TestCASSBilling.APTransactions.Count);
					var transaction = TestCASSBilling.APTransactions[0];
					AssertNotNull(transaction as APInvoice);

					transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
					AssertNotNull(transaction as APInvoice);
					Assert(transaction.Factory.HasContext(BusinessContext.CASS));

					TestCASSBilling.RunPreSaveValidation();
					AssertNoErrors(TestCASSBilling);
					TestCASSBilling.APTransactions.Factory.Save();

					AssertEquals("Invoice should have 2 lines as there iare 2 export charge (FRT, CC1)", 2, transaction.Lines.Count);
					AssertEquals(gatewayBillingJob.PK, transaction.Lines[0].AL_JH);
					AssertEquals(TestObjectCreator.FRT.AC_Code, transaction.Lines[0].ChargeCode.AC_Code);
					AssertEquals(-480.18M, transaction.Lines[0].AL_OSAmount);
					AssertEquals(-480.18M, transaction.Lines[0].AL_LineAmount);
					AssertEquals(0M, transaction.Lines[0].AL_GSTVAT);
					AssertEquals(TestObjectCreator.CC1.AC_Code, transaction.Lines[1].ChargeCode.AC_Code);
					AssertEquals(-200M, transaction.Lines[1].AL_OSAmount);
					AssertEquals(-200M, transaction.Lines[1].AL_LineAmount);
					AssertEquals(0M, transaction.Lines[1].AL_GSTVAT);
				}
			}
		}

		public void TestCreateInvoicesForGatewayBillingWithoutGatewayJob()
		{
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G"));

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: true);
				SetupAssociatedBizos(false);
				Factory.Save();

				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
				port.O5_SeaAirCarrierOrForwarderType = "GTW";
				port.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
				port.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				port.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
				Consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

				Factory.Save();
				TestCASSBilling.ForceRecalculateData();
				Assert(Consol.IsGateway());

				AssertEquals("Precondition: Line has no accrual value", 0M, TestCASSBilling.Lines[0].SystemCostAccrualValue);
				AssertEquals("Precondition: Consol is gateway consol", true, TestCASSBilling.Lines[0].IsForGatewayBilling);
				AssertNull("Precondition: Consol has no gateway consol job", TestCASSBilling.Lines[0].GatewayBillingJob);

				TestCASSBilling.ForceRecalculateData();
				AssertNoExceptionThrown("No Exception should be thrown", () => TestCASSBilling.CreateInvoices());
				AssertEquals("Invoice should be created", 1, TestCASSBilling.APTransactions.Count);
				var transaction = TestCASSBilling.APTransactions[0];
				AssertNotNull(transaction as APInvoice);

				transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
				AssertNotNull(transaction as APInvoice);
				Assert(transaction.Factory.HasContext(BusinessContext.CASS));

				TestCASSBilling.RunPreSaveValidation();
				AssertNoErrors(TestCASSBilling);
				TestCASSBilling.APTransactions.Factory.Save();

				AssertEquals("Invoice should have 2 lines as there are 2 export charge (FRT, CC1)", 2, transaction.Lines.Count);
				Assert("Should be Empty as there is no existing Gateway Job", transaction.Lines[0].AL_JH.IsEmpty);
				AssertEquals(TestObjectCreator.FRT.AC_Code, transaction.Lines[0].ChargeCode.AC_Code);
				AssertEquals(-80.18M, transaction.Lines[0].AL_OSAmount);
				AssertEquals(-80.18M, transaction.Lines[0].AL_LineAmount);
				AssertEquals(0M, transaction.Lines[0].AL_GSTVAT);
				Assert("Should be Empty as there is no existing Gateway Job", transaction.Lines[1].AL_JH.IsEmpty);
				AssertEquals(TestObjectCreator.CC1.AC_Code, transaction.Lines[1].ChargeCode.AC_Code);
				AssertEquals(-600M, transaction.Lines[1].AL_OSAmount);
				AssertEquals(-600M, transaction.Lines[1].AL_LineAmount);
				AssertEquals(0M, transaction.Lines[1].AL_GSTVAT);
			}
		}

		public void TestCreateInvoicesForGatewayBillingWithoutAccruals()
		{
			var gatewayDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, SQLComparisonOperator.StartsWith, "G"));

			using (new TemporaryUserContext { DepartmentPK = gatewayDepartment.PK.ToGuid() }.Set())
			{
				var testCASSChargeCodeCollection = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Import.Code, CASSChargeCodeLookups.ALL, TestObjectCreator.CC2.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidWeightCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidValuationCharge.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.PrepaidChargesDueToTheCarrier.Code, TestObjectCreator.CC1.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.CollectChargesDueToTheAgentExport.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Commission.Code, TestObjectCreator.FRT.PK);
				AddCASSChargeCodeLine(testCASSChargeCodeCollection, CASSChargeCodeLookups.CASSTypes.Export.Code, CASSChargeCodeLookups.CASSComponents.Discount.Code, TestObjectCreator.FRT.PK);
				AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, testCASSChargeCodeCollection);

				SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: true);
				SetupAssociatedBizos(false);
				Factory.Save();

				Consol.JK_AgentType = Constants.AgentType.Agent;
				Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				var port = Factory.NewWithValidTestData<OrgAppointedAgentPorts>();
				port.O5_SeaAirCarrierOrForwarderType = "GTW";
				port.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
				port.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
				port.O5_AgentDirection = AgentDirectionList.Codes.Both;
				port.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
				Consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);
				var gatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);

				Factory.Save();
				TestCASSBilling.ForceRecalculateData();
				Assert(Consol.IsGateway());

				AssertEquals("Precondition: Line has no accrual value", 0M, TestCASSBilling.Lines[0].SystemCostAccrualValue);
				AssertEquals("Precondition: Consol is gateway consol", true, TestCASSBilling.Lines[0].IsForGatewayBilling);
				AssertNotNull("Precondition: Consol has a gateway consol job", TestCASSBilling.Lines[0].GatewayBillingJob);

				TestCASSBilling.ForceRecalculateData();
				AssertNoExceptionThrown("No Exception should be thrown", () => TestCASSBilling.CreateInvoices());
				AssertEquals("Invoice should be created", 1, TestCASSBilling.APTransactions.Count);
				var transaction = TestCASSBilling.APTransactions[0];
				AssertNotNull(transaction as APInvoice);

				transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
				AssertNotNull(transaction as APInvoice);
				Assert(transaction.Factory.HasContext(BusinessContext.CASS));

				TestCASSBilling.RunPreSaveValidation();
				AssertNoErrors(TestCASSBilling);
				TestCASSBilling.APTransactions.Factory.Save();

				AssertEquals("Invoice should have 2 lines as there are 2 export charge (FRT, CC1)", 2, transaction.Lines.Count);
				AssertEquals("Should be Gateway Job", gatewayBillingJob.PK, transaction.Lines[0].AL_JH);
				AssertEquals(TestObjectCreator.FRT.AC_Code, transaction.Lines[0].ChargeCode.AC_Code);
				AssertEquals(-80.18M, transaction.Lines[0].AL_OSAmount);
				AssertEquals(-80.18M, transaction.Lines[0].AL_LineAmount);
				AssertEquals(0M, transaction.Lines[0].AL_GSTVAT);
				AssertEquals("Should be Gateway Job", gatewayBillingJob.PK, transaction.Lines[1].AL_JH);
				AssertEquals(TestObjectCreator.CC1.AC_Code, transaction.Lines[1].ChargeCode.AC_Code);
				AssertEquals(-600M, transaction.Lines[1].AL_OSAmount);
				AssertEquals(-600M, transaction.Lines[1].AL_LineAmount);
				AssertEquals(0M, transaction.Lines[1].AL_GSTVAT);
			}
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateInvoicesForGatewayBillingWithGLAccount()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var gb = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "SYD"));

			var loginBranchPk = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_Code, "BNE")).PK;
			using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid(), BranchPK = loginBranchPk.ToGuid() }.Set())
			{
				var cassCostHeader = TestCASSBilling.CostHeader;
				cassCostHeader.HOTFileName = "TestCASS.hot";
				cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
				cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
				cassCostHeader.DateOfBilling = today;
				cassCostHeader.InitializeAsExportCASS();

				for (int i = 0; i < 2; i++)
				{
					var cassBillingLine = TestCASSBilling.Lines.AddNew();

					var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Billing);
					costLine.RecordType = "AWM";
					costLine.VATIndicator = "Y";
					costLine.AirlinePrefix = "172";
					costLine.AWBSerialNumber = i == 0 ? "67828073" : "12345678";
					costLine.AgentCode = "23470/068-510";
					costLine.DateAWBExecution = threeMonthsAgo;
					costLine.DateOfArrival = twoMonthsAgo;
					costLine.DateOfDelivery = oneMonthAgo;
					costLine.Origin = "LEJ";
					costLine.Destination = "MEX";
					costLine.Weight = 2150M;
					costLine.WeightUnit = "KG";
					costLine.CurrencyCode = "EUR";
					costLine.WeightChargePP = 68018M;
					costLine.VATDueAirline = 6801M;
					cassBillingLine.AddCostLine(costLine, "AUD");
				}
				SetupAssociatedBizos();
				SetupGatewayOnConsol();
				GatewayBillingJob.JH_GB = gb.PK;

				JobCharge1.JR_GE = TestObjectCreator.FISDepartment.PK;

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				AssertEquals(1, TestCASSBilling.APTransactions.Count);
				InvoicingBase transaction = TestCASSBilling.APTransactions[0];
				AssertNotNull(transaction as APInvoice);

				transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
				AssertNotNull(transaction as APInvoice);
				Assert(transaction.Factory.HasContext(BusinessContext.CASS));

				AssertEquals(2, transaction.Lines.Count);

				AssertEquals("The first transaction is linked to the Gateway Job", GatewayBillingJob.PK, transaction.Lines[0].AL_JH);
				AssertEquals("The second transaction does not have any linked Job", ZGuid.Empty, transaction.Lines[1].AL_JH);

				AssertEquals("The first line's branch comes from the job header", loginBranchPk, transaction.Lines[0].AL_GB);
				AssertEquals("The first line's department comes from the job header", GatewayDepartmentPK, transaction.Lines[0].AL_GE);

				AssertEquals("The second line's department comes from the job header too", GatewayDepartmentPK, transaction.Lines[1].AL_GE);
				AssertEquals("The second transaction is linked to the login branch", loginBranchPk, transaction.Lines[1].AL_GB);
			}
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateInvoicesForGatewayBillingWithAdjustedLines()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid() }.Set())
			{
				SetupAssociatedBizos(true, "EUR");
				SetupGatewayOnConsol();

				var cassCostHeader = TestCASSBilling.CostHeader;
				cassCostHeader.HOTFileName = "TestCASS.hot";
				cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
				cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
				cassCostHeader.DateOfBilling = today;
				cassCostHeader.InitializeAsExportCASS();

				for (int i = 0; i < 2; i++)
				{
					var cassBillingLine = TestCASSBilling.Lines.AddNew();
					foreach (bool isAdjustment in new bool[] { false, true })
					{
						if (!(i == 0 && isAdjustment))
						{
							var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
							costLine.RecordType = isAdjustment ? "DCO" : "AWM";
							costLine.VATIndicator = "Y";
							costLine.AirlinePrefix = "172";
							costLine.AWBSerialNumber = i == 0 ? "67828073" : "12345678";
							costLine.AgentCode = "23470/068-510";
							costLine.DateAWBExecution = threeMonthsAgo;
							costLine.DateOfArrival = twoMonthsAgo;
							costLine.DateOfDelivery = oneMonthAgo;
							costLine.Origin = "LEJ";
							costLine.Destination = "MEX";
							costLine.Weight = 2150M;
							costLine.WeightUnit = "KG";
							costLine.CurrencyCode = "EUR";
							costLine.WeightChargePP = isAdjustment ? 1015.68M : 680.18M;
							costLine.VATDueAirline = isAdjustment ? 101.56M : 68.01M;
							cassBillingLine.AddCostLine(costLine, "AUD");
						}
					}
				}
				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				AssertEquals(1, TestCASSBilling.APTransactions.Count);
				InvoicingBase transaction = TestCASSBilling.APTransactions[0];
				AssertNotNull(transaction as APInvoice);

				transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
				AssertNotNull(transaction as APInvoice);
				Assert(transaction.Factory.HasContext(BusinessContext.CASS));

				AssertEquals(3, transaction.Lines.Count);
				AssertNull("There is no Apportionment Charge associated with the line.", transaction.Lines[0].ApportionmentChargeImportedFrom);
				AssertNull("There is no Apportionment Charge associated with the line.", transaction.Lines[1].ApportionmentChargeImportedFrom);
				AssertNull("There is no Apportionment Charge associated with the line.", transaction.Lines[2].ApportionmentChargeImportedFrom);

				TestCASSBilling.RunPreSaveValidation();
				AssertNoErrors(TestCASSBilling);
				TestCASSBilling.APTransactions.Factory.Save();

				AssertEquals("FRT", transaction.Lines[0].ChargeCode.AC_Code);
				AssertEquals(TestObjectCreator.GLHeader1.PK, transaction.Lines[1].AL_AG);
				AssertEquals(TestObjectCreator.GLHeader1.PK, transaction.Lines[2].AL_AG);
				AssertEquals(GatewayBillingJob.PK, transaction.Lines[0].AL_JH);
				AssertEquals(ZGuid.Empty, transaction.Lines[1].AL_JH);
				AssertEquals(ZGuid.Empty, transaction.Lines[2].AL_JH);
				AssertEquals(-748.19M, transaction.Lines[0].AL_OSAmount);
				AssertEquals(-680.18M, transaction.Lines[0].AL_LineAmount);
				AssertEquals(-68.01M, transaction.Lines[0].AL_GSTVAT);
				AssertEquals(-748.19M, transaction.Lines[1].AL_OSAmount);
				AssertEquals(-680.18M, transaction.Lines[1].AL_LineAmount);
				AssertEquals(-68.01M, transaction.Lines[1].AL_GSTVAT);
				AssertEquals(1117.24M, transaction.Lines[2].AL_OSAmount);
				AssertEquals(1015.68M, transaction.Lines[2].AL_LineAmount);
				AssertEquals(101.56M, transaction.Lines[2].AL_GSTVAT);

				TestCASSBilling.Lines.RemoveAll();
				for (int i = 0; i < 2; i++)
				{
					var cassBillingLine = TestCASSBilling.Lines.AddNew();
					foreach (bool isAdjustment in new bool[] { false, true })
					{
						if (!(i == 0 && isAdjustment))
						{
							var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
							costLine.RecordType = isAdjustment ? "DCO" : "AWM";
							costLine.VATIndicator = "Y";
							costLine.AirlinePrefix = "172";
							costLine.AWBSerialNumber = i == 0 ? "67828073" : "12345678";
							costLine.AgentCode = "23470/068-510";
							costLine.DateAWBExecution = threeMonthsAgo;
							costLine.DateOfArrival = twoMonthsAgo;
							costLine.DateOfDelivery = oneMonthAgo;
							costLine.Origin = "LEJ";
							costLine.Destination = "MEX";
							costLine.Weight = 2150M;
							costLine.WeightUnit = "KG";
							costLine.CurrencyCode = "EUR";
							costLine.WeightChargePP = isAdjustment ? 1015.68M : 680.18M;
							costLine.VATDueAirline = (i == 0 || isAdjustment) ? 0M : 68.01M;
							cassBillingLine.AddCostLine(costLine, "AUD");
						}
					}
				}

				TestCASSBilling.ForceRecalculateData();
				TestCASSBilling.CreateInvoices();

				AssertEquals(1, TestCASSBilling.APTransactions.Count);
				transaction = TestCASSBilling.APTransactions[0];
				AssertNotNull(transaction as APInvoice);

				transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
				AssertNotNull(transaction as APInvoice);
				Assert(transaction.Factory.HasContext(BusinessContext.CASS));

				AssertEquals(3, transaction.Lines.Count);
				AssertNull("There is no Apportionment Charge associated with the line.", transaction.Lines[0].ApportionmentChargeImportedFrom);
				AssertNull("There is no Apportionment Charge associated with the line.", transaction.Lines[1].ApportionmentChargeImportedFrom);
				AssertNull("There is no Apportionment Charge associated with the line.", transaction.Lines[2].ApportionmentChargeImportedFrom);

				TestCASSBilling.RunPreSaveValidation();
				AssertNoErrors(TestCASSBilling);
				TestCASSBilling.APTransactions.Factory.Save();

				AssertEquals("FRT", transaction.Lines[0].ChargeCode.AC_Code);
				AssertEquals(TestObjectCreator.GLHeader1.PK, transaction.Lines[1].AL_AG);
				AssertEquals(TestObjectCreator.GLHeader1.PK, transaction.Lines[2].AL_AG);
				AssertEquals(GatewayBillingJob.PK, transaction.Lines[0].AL_JH);
				AssertEquals(ZGuid.Empty, transaction.Lines[1].AL_JH);
				AssertEquals(ZGuid.Empty, transaction.Lines[2].AL_JH);
				AssertEquals(-680.18M, transaction.Lines[0].AL_OSAmount);
				AssertEquals(-680.18M, transaction.Lines[0].AL_LineAmount);
				AssertEquals(0M, transaction.Lines[0].AL_GSTVAT);
				AssertEquals(-748.19M, transaction.Lines[1].AL_OSAmount);
				AssertEquals(-680.18M, transaction.Lines[1].AL_LineAmount);
				AssertEquals(-68.01M, transaction.Lines[1].AL_GSTVAT);
				AssertEquals(1015.68M, transaction.Lines[2].AL_OSAmount);
				AssertEquals(1015.68M, transaction.Lines[2].AL_LineAmount);
				AssertEquals(0M, transaction.Lines[2].AL_GSTVAT);
			}
		}

		public void TestAddInvoiceLineForGLAccountForNonGSTCompany()
		{
			GlbCompany companyWithNoTaxRates = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			companyWithNoTaxRates.GC_IsGSTRegistered = false;
			AssertCompanyHasNoTaxRates(companyWithNoTaxRates);
			GlbBranch branchOnCompanyWithNoTaxRate = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, companyWithNoTaxRates.PK));
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			using (branchOnCompanyWithNoTaxRate.SetAsTemporaryContext())
			{
				SetupCASSBilling(TestCASSBilling);
				SetupAssociatedBizos();

				var costLine = new CASSCostExportLine(Factory, CASSCostLineType.Default);
				costLine.AWBSerialNumber = "12345678";
				TestCASSBilling.Lines[1].AddCostLine(costLine, TestCASSBilling.Lines[1].CASSCostCurrencyCode);

				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				TestCASSBilling.CreateInvoices();
			}
		}

		[TestDate(2017, 09, 10)]
		public void TestGenerateCompleteInvoice_ShowErrorHandlerSet()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.RecordType = isAdjustment ? "DCO" : "AWM";
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = i == 0 ? "67828073" : "12345678";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "EUR";
						costLine.WeightChargePP = isAdjustment ? 101568M : 68018M;
						costLine.VATDueAirline = isAdjustment ? 10156M : 6801M;
						cassBillingLine.AddCostLine(costLine, "AUD");
					}
				}
			}

			SetupAssociatedBizos();
			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);

			InvoicingBase newTransaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			Assert(newTransaction.Factory.HasContext(BusinessContext.CASS));

			var chargeCode = new BusinessObjectFactory().Load<AccChargeCode>(TestObjectCreator.FRT.PK);
			chargeCode.AC_Code += "_1";
			chargeCode.Factory.Save();

			bool isShowErrorCalled = false;
			transaction.ShowError = (message, caption) => { isShowErrorCalled = true; };
			newTransaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			Assert(isShowErrorCalled);
			Assert(!newTransaction.IsValidationSuspended);
		}

		[TestDate(2017, 09, 10)]
		public void TestGetCompleteInvoiceIsPostedException()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.RecordType = isAdjustment ? "DCO" : "AWM";
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = i == 0 ? "67828073" : "12345678";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "EUR";
						costLine.WeightChargePP = isAdjustment ? 101568M : 68018M;
						costLine.VATDueAirline = isAdjustment ? 10156M : 6801M;
						cassBillingLine.AddCostLine(costLine, "AUD");
					}
				}
			}

			SetupAssociatedBizos();
			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);

			transaction.IsPostedToCASSOrSaved = true;
			bool isExceptionThrown = false;
			try
			{
				InvoicingBase newTransaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			}
			catch (TransactionNotFoundException)
			{
				isExceptionThrown = true;
				ExceptionReporterTestListener.Instance.Clear();
			}
			AssertEquals("Exception should be thrown when IsPostedToCASS is true and the transaction isn't posted", true, isExceptionThrown);
			AssertEquals("If Exception is thrown, then IsPostedToCASS state should be reset", false, transaction.IsPostedToCASSOrSaved);
		}

		[TestDate(2017, 09, 10)]
		public void TestGenerateCompleteInvoice_ShowMutexError()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.RecordType = isAdjustment ? "DCO" : "AWM";
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = i == 0 ? "67828073" : "12345678";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "EUR";
						costLine.WeightChargePP = isAdjustment ? 101568M : 68018M;
						costLine.VATDueAirline = isAdjustment ? 10156M : 6801M;
						cassBillingLine.AddCostLine(costLine, "AUD");
					}
				}
			}

			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();
			Job1.Delete();

			Factory.Save();

			string lastJobCreationErrorMessage = null;
			TestCASSBilling.JobCreationError += (sender, e) => lastJobCreationErrorMessage = e.ErrorMessage;

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();
			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			transaction.ShowError = (message, caption) => { };

			string expectedMessage =
@"You have created the job SHIP1 on another form, but haven't saved it yet.
Please close or save other forms that use job SHIP1 to continue.";
			var newFactory = new BusinessObjectFactory();
			var jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();

			lastJobCreationErrorMessage = null;
			TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertEquals("Mutex error should be shown", expectedMessage, lastJobCreationErrorMessage);
			AssertEquals("APTransactions must have errors if mutex error is happened", true, TestCASSBilling.APTransactions.HasErrors());

			TestCASSBilling.RunPreSaveValidation();
			AssertEquals("RunPreSaveValidation must not clear APTransactions errors", true, TestCASSBilling.APTransactions.HasErrors());

			jobWithMutex.Dispose();
			jobWithMutex.Delete();
			lastJobCreationErrorMessage = null;
			var completeInvoice = TestCASSBilling.GenerateCompleteInvoice(transaction);
			Assert(!completeInvoice.IsValidationSuspended);
			AssertNull("Mutex error should not be shown", lastJobCreationErrorMessage);
			AssertEquals("APTransactions must not have errors since mutex is released.", false, TestCASSBilling.APTransactions.HasErrors());

			jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();
			AssertNull("Job Mutex should not be here. It should be released by method that use created invoice.", jobWithMutex);

			completeInvoice.ReleaseAllMutexOnInvoice();
			EventHandler<NotificationsChangedEventArgs> transaction_NotificationsChanged = (sender, e) =>
			{
				throw new InvalidOperationException("test");
			};
			transaction.NotificationsChanged += transaction_NotificationsChanged;
			bool isExceptionRaised = false;
			try
			{
				TestCASSBilling.GenerateCompleteInvoice(transaction);
			}
			catch (InvalidOperationException e)
			{
				if (e.Message == "test")
				{
					isExceptionRaised = true;
				}
			}
			Assert("Exception should be raised.", isExceptionRaised);
			jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();
			AssertNotNull("Job Mutex should be released if exception was raised.", jobWithMutex);
			jobWithMutex.Dispose();
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateInvoices_ReleaseAllMutexOnInvoice()
		{
			var today = ZDateTime.Today;

			SetupCASSBilling(TestCASSBilling, setAdjustmentValues: false, setupAmount: false);
			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();

			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 68018M;
			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).VATDueAirline = 6801M;

			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 68018M;
			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).VATDueAirline = 6801M;

			var adjustmentLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			adjustmentLine.AirlinePrefix = "172";
			adjustmentLine.AWBSerialNumber = "67828073";
			adjustmentLine.AgentCode = "23470/068-510";
			adjustmentLine.DateAWBExecution = today.AddMonths(-3);
			adjustmentLine.DateOfArrival = today.AddMonths(-2);
			adjustmentLine.DateOfDelivery = today.AddMonths(-1);
			adjustmentLine.Origin = "LEJ";
			adjustmentLine.Destination = "MEX";
			adjustmentLine.Weight = 2150M;
			adjustmentLine.WeightUnit = "KG";
			adjustmentLine.CurrencyCode = "AUD";
			adjustmentLine.WeightChargePP = 101568M;
			adjustmentLine.VATDueAirline = 10156M;
			adjustmentLine.AWBSerialNumber = "12345678";
			TestCASSBilling.Lines[1].AddCostLine(adjustmentLine, TestCASSBilling.Lines[1].CASSCostCurrencyCode);

			Job1.Delete();
			Factory.Save();

			string lastJobCreationErrorMessage = null;
			TestCASSBilling.JobCreationError += (sender, e) => lastJobCreationErrorMessage = e.ErrorMessage;

			string expectedMessage =
@"You have created the job SHIP1 on another form, but haven't saved it yet.
Please close or save other forms that use job SHIP1 to continue.";
			var newFactory = new BusinessObjectFactory();
			var jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();

			lastJobCreationErrorMessage = null;
			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();
			AssertEquals("Mutex error should be shown", expectedMessage, lastJobCreationErrorMessage);
			AssertEquals("APTransactions must have errors if mutex error is happened", true, TestCASSBilling.APTransactions.HasErrors());

			TestCASSBilling.RunPreSaveValidation();
			AssertEquals("RunPreSaveValidation must not clear APTransactions errors", true, TestCASSBilling.APTransactions.HasErrors());

			lastJobCreationErrorMessage = null;
			jobWithMutex.Dispose();
			jobWithMutex.Delete();
			TestCASSBilling.ForceRecalculateData();
			TestCASSBilling.CreateInvoices();
			AssertNull("Mutex error should not be shown", lastJobCreationErrorMessage);
			AssertEquals("APTransactions must not have errors since mutex is released.", false, TestCASSBilling.APTransactions.HasErrors());

			jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();
			AssertNotNull("Job Mutex should be released after invoice was created.", jobWithMutex);
			jobWithMutex.Dispose();
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateGeneratePostInvoices_ValidationErrorForJobMutex()
		{
			var today = ZDateTime.Today;

			SetupCASSBilling(TestCASSBilling, setupAmount: false, setAdjustmentValues: false);

			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 68018M;
			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).VATDueAirline = 6801M;

			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 68018M;
			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).VATDueAirline = 6801M;

			var adjustmentLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			adjustmentLine.AirlinePrefix = "172";
			adjustmentLine.AWBSerialNumber = "67828073";
			adjustmentLine.AgentCode = "23470/068-510";
			adjustmentLine.DateAWBExecution = today.AddMonths(-3);
			adjustmentLine.DateOfArrival = today.AddMonths(-2);
			adjustmentLine.DateOfDelivery = today.AddMonths(-1);
			adjustmentLine.Origin = "LEJ";
			adjustmentLine.Destination = "MEX";
			adjustmentLine.Weight = 2150M;
			adjustmentLine.WeightUnit = "KG";
			adjustmentLine.CurrencyCode = "AUD";
			adjustmentLine.WeightChargePP = 101568M;
			adjustmentLine.VATDueAirline = 10156M;
			adjustmentLine.AWBSerialNumber = "12345678";
			TestCASSBilling.Lines[1].AddCostLine(adjustmentLine, TestCASSBilling.Lines[1].CASSCostCurrencyCode);

			SetupAssociatedBizos();
			Job1.Delete();

			Factory.Save();

			string lastJobCreationErrorMessage = null;
			TestCASSBilling.JobCreationError += (sender, e) =>
			{
				lastJobCreationErrorMessage = e.ErrorMessage;
			};

			string expectedMessage =
@"You have created the job SHIP1 on another form, but haven't saved it yet.
Please close or save other forms that use job SHIP1 to continue.";
			var newFactory = new BusinessObjectFactory();
			var jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();

			lastJobCreationErrorMessage = null;
			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();
			AssertEquals("Mutex error should be shown", expectedMessage, lastJobCreationErrorMessage);
			AssertEquals("APTransactions must have errors if mutex error is happened", true, TestCASSBilling.APTransactions.HasErrors());

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			transaction.ShowError = (message, caption) => { };

			lastJobCreationErrorMessage = null;
			jobWithMutex.Dispose();
			jobWithMutex.Delete();
			var completeInvoice = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNull("Mutex error should not be shown", lastJobCreationErrorMessage);
			AssertEquals("APTransactions must have errors as invoice was serialized with mutex error.", true, TestCASSBilling.APTransactions.HasErrors());
			completeInvoice.ReleaseAllMutexOnInvoice();
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			TestCASSBilling.PostAPTransactions();
			AssertNull("Mutex error should not be shown", lastJobCreationErrorMessage);
			AssertEquals("APTransactions must have errors as invoice was serialized with mutex error.", true, TestCASSBilling.APTransactions.HasErrors());
			jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();
			AssertNotNull("Job Mutex should be released if exception was raised.", jobWithMutex);
			jobWithMutex.Dispose();
		}

		[TestDate(2017, 09, 10)]
		public void TestPostInvoices_ReleaseAllMutexOnInvoice()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			SetupCASSBilling(TestCASSBilling);
			SetupAssociatedBizos();

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.RecordType = isAdjustment ? "DCO" : "AWM";
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = i == 0 ? "67828073" : "12345678";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "AUD";
						costLine.WeightChargePP = isAdjustment ? 10158M : 68018M;
						costLine.VATDueAirline = isAdjustment ? 1015M : 6801M;
						cassBillingLine.AddCostLine(costLine, "AUD");
					}
				}
			}
			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			TestCASSBilling.PostAPTransactions();
			AssertEquals("APTransactions must have errors as charge department (BRN) is not in the department list for FRT.", true, TestCASSBilling.APTransactions.HasErrors());

			var newFactory = new BusinessObjectFactory();
			var jobWithMutex = new Job.Loader(newFactory, Shipment1).TryCreateWithMutex();
			AssertNotNull("Job Mutex should be released even if invoice was not posted due to validation errors.", jobWithMutex);
			jobWithMutex.Dispose();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			var completeInvoice = TestCASSBilling.GenerateCompleteInvoice(transaction);
			Assert("Invoice must not be posted with errors.", !completeInvoice.IsInDatabase);
			completeInvoice.ReleaseAllMutexOnInvoice();

			IncludeCurrentDepartmentForChargeCodeFRT();
			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			TestCASSBilling.CreateInvoices();
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			TestCASSBilling.PostAPTransactions();
			AssertEquals("APTransactions must not have errors since mutex is released and charge's department is fixed.", false, TestCASSBilling.APTransactions.HasErrors());
			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			completeInvoice = TestCASSBilling.GenerateCompleteInvoice(transaction);
			Assert("Invoice must be posted.", completeInvoice.IsInDatabase);
		}

		public void TestCreateInvoiceGetLinesDepartmentFromApportionedCharge()
		{
			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);
			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, 1000, false);
			AssertNotNull(consolCost.ApportionmentCharges);
			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			GlbDepartment cEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA"));
			consolCost.ApportionmentCharges[0].JR_GE = cEA.PK;

			GlbDepartment cES = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			consolCost.ApportionmentCharges[1].JR_GE = cES.PK;
			Factory.Save();

			GlbDepartment fEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			AssertEquals("Job 1's department is FEA", fEA.PK, Job1.Department.PK);
			AssertEquals("Job 2's department is FEA", fEA.PK, Job2.Department.PK);

			GlbDepartment fIS = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
			JobCharge1.JR_GE = fIS.PK;
			JobCharge2.JR_GE = fIS.PK;
			Factory.Save();
			AssertEquals("Job charge 1's department is FIS", fIS.PK, JobCharge1.Department.PK);
			AssertEquals("Job charge 2's department is FIS", fIS.PK, JobCharge2.Department.PK);

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APInvoice);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			var sortedLines = transaction.Lines.OrderBy(x => x.Department.GE_Code);
			AssertEquals("Line 1's department should come from apportioned charge", cEA.PK, sortedLines.ElementAt(0).Department.PK);
			AssertEquals("Line 2's department should come from apportioned charge", cES.PK, sortedLines.ElementAt(1).Department.PK);
		}

		public void TestCreateInvoiceGetLinesDepartmentFromJobChargeWhenNoApportionedCharge()
		{
			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);
			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();
			Factory.Save();

			GlbDepartment fEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			AssertEquals("Job 1's department is FEA", fEA.PK, Job1.Department.PK);
			AssertEquals("Job 2's department is FEA", fEA.PK, Job2.Department.PK);

			GlbDepartment fIS = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
			JobCharge1.JR_GE = fIS.PK;

			GlbDepartment cES = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			JobCharge2.JR_GE = cES.PK;
			Factory.Save();
			AssertEquals("Job charge 1's department is FIS", fIS.PK, JobCharge1.Department.PK);
			AssertEquals("Job charge 2's department is CES", cES.PK, JobCharge2.Department.PK);

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();
			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			AssertNotNull(transaction as APInvoice);
			var sortedLines = transaction.Lines.OrderBy(x => x.Department.GE_Code);
			AssertEquals("Line 1's department should come from job charge", cES.PK, sortedLines.ElementAt(0).Department.PK);
			AssertEquals("Line 2's department should come from job charge", fIS.PK, sortedLines.ElementAt(1).Department.PK);
		}

		public void TestCreateInvoiceGetLinesDepartmentFromJobWhenNoApportionedChargeAndNoJobCharge()
		{
			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);
			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();
			Factory.Save();

			GlbDepartment fEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			AssertEquals("Job 1's department is FEA", fEA.PK, Job1.Department.PK);

			GlbDepartment cES = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			Job2.JH_GE = cES.PK;
			AssertEquals("Job 2's department is CES", cES.PK, Job2.Department.PK);

			AccChargeCode dOF = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DOF"));
			JobCharge1.JR_AC = dOF.PK;
			JobCharge2.JR_AC = dOF.PK;

			GlbDepartment fIS = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
			JobCharge1.JR_GE = fIS.PK;
			JobCharge2.JR_GE = fIS.PK;

			Factory.Save();
			AssertEquals("Job charge 1's department is FIS", fIS.PK, JobCharge1.Department.PK);
			AssertEquals("Job charge 2's department is FIS", fIS.PK, JobCharge2.Department.PK);

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();
			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APInvoice);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			var sortedLines = transaction.Lines.OrderBy(x => x.Department.GE_Code);
			AssertEquals("Line 1's department should come from job", cES.PK, sortedLines.ElementAt(0).Department.PK);
			AssertEquals("Line 2's department should come from job", fEA.PK, sortedLines.ElementAt(1).Department.PK);
		}

		public void TestCreateInvoiceGetLinesDepartmentFromUnRecognizedChargeWhenNoApportionedChargeAndNoJobCharge()
		{
			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);
			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();
			Factory.Save();

			GlbDepartment fEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			AssertEquals("Job 1's department is FEA", fEA.PK, Job1.Department.PK);

			GlbDepartment cES = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			Job2.JH_GE = cES.PK;
			AssertEquals("Job 2's department is CES", cES.PK, Job2.Department.PK);

			AccChargeCode dOF = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "DOF"));
			JobCharge1.JR_AC = dOF.PK;
			JobCharge2.JR_AC = dOF.PK;

			GlbDepartment fIS = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
			JobCharge1.JR_GE = fIS.PK;
			JobCharge2.JR_GE = fIS.PK;

			JobCharge1.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;
			JobCharge2.JR_OH_CostAccount = TestObjectCreator.ABIGAS.PK;

			JobCharge unrecognisedCharge1 = Factory.NewWithValidTestData<JobCharge>();
			unrecognisedCharge1.JR_AC = TestObjectCreator.FRT.PK;
			unrecognisedCharge1.JR_JH = Job1.PK;
			unrecognisedCharge1.JR_GB = Job1.JH_GB;
			GlbDepartment cIS = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CIS"));
			unrecognisedCharge1.JR_GE = cIS.PK;
			unrecognisedCharge1.JR_OSSellAmt = 15m;
			unrecognisedCharge1.JR_LocalSellAmt = 15m;
			unrecognisedCharge1.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			unrecognisedCharge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

			JobCharge unrecognisedCharge2 = Factory.NewWithValidTestData<JobCharge>();
			unrecognisedCharge2.JR_AC = TestObjectCreator.FRT.PK;
			unrecognisedCharge2.JR_JH = Job2.PK;
			unrecognisedCharge2.JR_GB = Job1.JH_GB;
			GlbDepartment cIA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CIA"));
			unrecognisedCharge2.JR_GE = cIA.PK;
			unrecognisedCharge2.JR_OSSellAmt = 15m;
			unrecognisedCharge2.JR_LocalSellAmt = 15m;
			unrecognisedCharge2.JR_OH_SellAccount = TestObjectCreator.ABIGAS.PK;
			unrecognisedCharge2.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;

			Factory.Save();
			AssertEquals("Job charge 1's department is FIS", fIS.PK, JobCharge1.Department.PK);
			AssertEquals("Job charge 2's department is FIS", fIS.PK, JobCharge2.Department.PK);

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();
			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APInvoice);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			var sortedLines = transaction.Lines.OrderBy(x => x.Department.GE_Code);
			AssertEquals("Line 1's department should come from unrecognized charge", cIA.PK, sortedLines.ElementAt(0).Department.PK);
			AssertEquals("Line 2's department should come from unrecognized charge", cIS.PK, sortedLines.ElementAt(1).Department.PK);
		}

		public void TestCreateInvoiceGetLinesDepartmentFromJobChargeIfApportionedChargeCreditorIsDifferentAndBringForwardAgainstCreditorIsTrue()
		{
			SetupApportionChargesWithDifferentCreditor();
			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APInvoice);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			var sortedLines = transaction.Lines.OrderBy(x => x.Department.GE_Code);
			GlbDepartment fIS = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
			AssertEquals("Line 1's department should come from job charge", fIS.PK, sortedLines.ElementAt(0).Department.PK);
			AssertEquals("Line 2's department should come from job charge", fIS.PK, sortedLines.ElementAt(1).Department.PK);
		}

		public void TestCreateInvoiceBringForwardAgainstCreditorForCASSChargeCodes()
		{
			var regValue = GetCASSChargeCodeRegistry(TestObjectCreator.FRT, TestObjectCreator.MRG100);
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, regValue);

			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);
			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();
			Factory.Save();

			var consolCost1 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, 1000, false);
			var consolCost2 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.MRG100, TestObjectCreator.Creditor1, 30, false);
			var consolCost3 = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.MRG60, null, 30, false);
			Factory.Save();

			var app = Consol.GetApportionments();
			AssertEquals("Consol has 3 costs", 3, app.CostsCollection.Count);

			TestCASSBilling.ForceRecalculateData();
			TestCASSBilling.CreateInvoices();
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			TestCASSBilling.PostAPTransactions();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			var transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APInvoice);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			Assert("Invoice must be posted.", transaction.IsInDatabase);

			var newFactory = new BusinessObjectFactory();
			var consolInNewFactory = newFactory.Load<ForwardingConsol>(Consol.PK);
			var costingBO = (BusinessObject)consolInNewFactory;
			costingBO.Factory.ClearCachedValue<ApportionmentListing>("ApportionmentListing|" + costingBO.PK.ToString());

			app = consolInNewFactory.GetApportionments();
			AssertEquals("Consol has 3 costs", 3, app.CostsCollection.Count);
			Assert("Cost for 'MRG100' should not be deleted", app.CostsCollection.Any(x => ((JobConsolCost)x).E6_AC_ChargeCode == TestObjectCreator.MRG100.PK));
			Assert("Cost for 'MRG60' should  not be deleted", app.CostsCollection.Any(x => ((JobConsolCost)x).E6_AC_ChargeCode == TestObjectCreator.MRG60.PK));
		}

		public void TestCreateInvoiceGetLinesDepartmentFromApportionedChargeIfApportionedChargeCreditorIsDifferentAndBringForwardAgainstCreditorIsFalse()
		{
			AccountingConfigurationRegistry.Instance.BringForwardAgainstCreditor.SetValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);

			SetupApportionChargesWithDifferentCreditor();
			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);
			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APInvoice);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			var sortedLines = transaction.Lines.OrderBy(x => x.Department.GE_Code);

			GlbDepartment cEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA"));
			GlbDepartment cES = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AssertEquals("Line 1's department should come from apportioned charge", cEA.PK, sortedLines.ElementAt(0).Department.PK);
			AssertEquals("Line 2's department should come from apportioned charge", cES.PK, sortedLines.ElementAt(1).Department.PK);
		}

		void SetupApportionChargesWithDifferentCreditor()
		{
			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);
			SetupAssociatedBizos();
			IncludeCurrentDepartmentForChargeCodeFRT();
			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.ABIGAS, 1000, false);
			AssertNotNull(consolCost.ApportionmentCharges);
			AssertEquals(2, consolCost.ApportionmentCharges.Count);
			GlbDepartment cEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA"));
			consolCost.ApportionmentCharges[0].JR_GE = cEA.PK;
			AssertEquals("apportion charge 1's creditor is ABIGAS", TestObjectCreator.ABIGAS.PK, consolCost.ApportionmentCharges[0].JR_OH_CostAccount);

			GlbDepartment cES = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			consolCost.ApportionmentCharges[1].JR_GE = cES.PK;
			AssertEquals("apportion charge 2's creditor is ABIGAS", TestObjectCreator.ABIGAS.PK, consolCost.ApportionmentCharges[1].JR_OH_CostAccount);
			Factory.Save();

			GlbDepartment fEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
			AssertEquals("Job 1's department is FEA", fEA.PK, Job1.Department.PK);
			AssertEquals("Job 2's department is FEA", fEA.PK, Job2.Department.PK);

			GlbDepartment fIS = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
			JobCharge1.JR_GE = fIS.PK;
			JobCharge2.JR_GE = fIS.PK;
			JobCharge1.JR_OH_CostAccount = TestObjectCreator.AALSHI.PK;
			Factory.Save();
			AssertEquals("Job charge 1's department is FIS", fIS.PK, JobCharge1.Department.PK);
			AssertEquals("Job charge 2's department is FIS", fIS.PK, JobCharge2.Department.PK);
			AssertEquals("Job charge 1's creditor is AALSHI", TestObjectCreator.AALSHI.PK, JobCharge1.JR_OH_CostAccount);
			AssertEquals("Job charge 2's creditor is empty", ZGuid.Empty, JobCharge2.JR_OH_CostAccount);
		}

		public void TestCreateGatewayInvoiceGetLinesDepartmentFromGatewayBillingCharge()
		{
			using (new TemporaryUserContext { DepartmentPK = GatewayDepartmentPK.ToGuid() }.Set())
			{
				SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, setupAmount: true);
				SetupAssociatedBizos();
				SetupGatewayOnConsol();
				IncludeCurrentDepartmentForChargeCodeFRT();
				Factory.Save();

				Assert(Consol.IsGateway());
				GatewaySellToCostSynchroniser.Synchronise(GatewayBillingJob);
				var appListing = Consol.GetApportionments(true);
				var consolCost = appListing.CostsCollection.Cast<JobConsolCost>().FirstOrDefault();

				AssertNotNull(consolCost);
				AssertNotNull(consolCost.ApportionmentCharges);
				AssertEquals(2, consolCost.ApportionmentCharges.Count);
				GlbDepartment cEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CEA"));
				consolCost.ApportionmentCharges[0].JR_GE = cEA.PK;

				GlbDepartment cES = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
				consolCost.ApportionmentCharges[1].JR_GE = cES.PK;
				Factory.Save();

				GlbDepartment fEA = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA"));
				AssertEquals("Job 1's department is FEA", fEA.PK, Job1.Department.PK);
				AssertEquals("Job 2's department is FEA", fEA.PK, Job2.Department.PK);

				GlbDepartment fIS = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FIS"));
				JobCharge1.JR_GE = fIS.PK;
				JobCharge2.JR_GE = fIS.PK;
				Factory.Save();
				AssertEquals("Job charge 1's department is FIS", fIS.PK, JobCharge1.Department.PK);
				AssertEquals("Job charge 2's department is FIS", fIS.PK, JobCharge2.Department.PK);

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();

				AssertEquals(1, TestCASSBilling.APTransactions.Count);
				InvoicingBase transaction = TestCASSBilling.APTransactions[0];
				AssertNotNull(transaction as APInvoice);
				transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
				AssertNotNull(transaction as APInvoice);
				Assert(transaction.Factory.HasContext(BusinessContext.CASS));
				AssertEquals("Line 1's department should come from gateway billing charge", GatewayDepartmentPK, transaction.Lines[0].Department.PK);
			}
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateInvoices_ResetLineJobDefaults()
		{
			var today = ZDateTime.Today;

			SetupCASSBilling(TestCASSBilling, setAdjustmentValues: false, setupAmount: false);
			SetupAssociatedBizos();

			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;
			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).VATDueAirline = 68.01M;

			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;
			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).VATDueAirline = 68.01M;

			var adjustmentLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			adjustmentLine.AirlinePrefix = "172";
			adjustmentLine.AWBSerialNumber = "67828073";
			adjustmentLine.AgentCode = "23470/068-510";
			adjustmentLine.DateAWBExecution = today.AddMonths(-3);
			adjustmentLine.DateOfArrival = today.AddMonths(-2);
			adjustmentLine.DateOfDelivery = today.AddMonths(-1);
			adjustmentLine.Origin = "LEJ";
			adjustmentLine.Destination = "MEX";
			adjustmentLine.Weight = 2150M;
			adjustmentLine.WeightUnit = "KG";
			adjustmentLine.CurrencyCode = "AUD";
			adjustmentLine.WeightChargePP = 1015.68M;
			adjustmentLine.VATDueAirline = 101.56M;
			adjustmentLine.AWBSerialNumber = "12345678";
			TestCASSBilling.Lines[1].AddCostLine(adjustmentLine, TestCASSBilling.Lines[1].CASSCostCurrencyCode);

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);

			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APInvoice);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			Assert(!transaction.IsValidationSuspended);

			AssertEquals(4, transaction.Lines.Count);
			AssertNull("There is not Apportionment Charge associated with the line.", transaction.Lines[0].ApportionmentChargeImportedFrom);
			AssertNull("There is not Apportionment Charge associated with the line.", transaction.Lines[1].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[2].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[3].ApportionmentChargeImportedFrom);

			TestCASSBilling.RunPreSaveValidation();
			AssertNoErrors(TestCASSBilling);
			TestCASSBilling.APTransactions.Factory.Save();

			AssertEquals(TestObjectCreator.GLHeader1.PK, transaction.Lines[0].AL_AG);
			AssertEquals(TestObjectCreator.GLHeader1.PK, transaction.Lines[1].AL_AG);
			AssertEquals("FRT", transaction.Lines[2].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[3].ChargeCode.AC_Code);
			AssertEquals(ZGuid.Empty, transaction.Lines[0].AL_JH);
			AssertEquals(ZGuid.Empty, transaction.Lines[1].AL_JH);
			if (Job1.PK == transaction.Lines[2].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[3].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[2].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[3].AL_JH);
			}
			AssertEquals(-748.19M, transaction.Lines[0].AL_OSAmount);
			AssertEquals(1117.24M, transaction.Lines[1].AL_OSAmount);
			AssertEquals(-374.09M, transaction.Lines[2].AL_OSAmount);
			AssertEquals(-374.10M, transaction.Lines[3].AL_OSAmount);
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateInvoices_WithAdjustedLine()
		{
			var today = ZDateTime.Today;

			SetupCASSBilling(TestCASSBilling, setAdjustmentValues: false, setupAmount: false);
			SetupAssociatedBizos();

			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;
			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).VATDueAirline = 68.01M;

			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).WeightChargePP = 680.18M;
			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).VATDueAirline = 68.01M;

			var adjustmentLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			adjustmentLine.AirlinePrefix = "172";
			adjustmentLine.AWBSerialNumber = "67828073";
			adjustmentLine.AgentCode = "23470/068-510";
			adjustmentLine.DateAWBExecution = today.AddMonths(-3);
			adjustmentLine.DateOfArrival = today.AddMonths(-2);
			adjustmentLine.DateOfDelivery = today.AddMonths(-1);
			adjustmentLine.Origin = "LEJ";
			adjustmentLine.Destination = "MEX";
			adjustmentLine.Weight = 2150M;
			adjustmentLine.WeightUnit = "KG";
			adjustmentLine.CurrencyCode = "AUD";
			adjustmentLine.WeightChargePP = 1015.68M;
			adjustmentLine.VATDueAirline = 101.56M;
			TestCASSBilling.Lines[1].AddCostLine(adjustmentLine, TestCASSBilling.Lines[1].CASSCostCurrencyCode);

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APInvoice);

			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APInvoice);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			Assert(!transaction.IsValidationSuspended);

			AssertEquals(6, transaction.Lines.Count);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[0].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[1].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[2].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[3].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[4].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[5].ApportionmentChargeImportedFrom);

			TestCASSBilling.RunPreSaveValidation();
			AssertNoErrors(TestCASSBilling);
			TestCASSBilling.APTransactions.Factory.Save();

			AssertEquals("FRT", transaction.Lines[0].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[1].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[2].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[3].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[4].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[5].ChargeCode.AC_Code);
			if (Job1.PK == transaction.Lines[0].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[1].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[0].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[1].AL_JH);
			}
			if (Job1.PK == transaction.Lines[2].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[3].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[2].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[3].AL_JH);
			}
			if (Job1.PK == transaction.Lines[4].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[5].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[4].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[5].AL_JH);
			}
			AssertEquals(-374.09M, transaction.Lines[0].AL_OSAmount);
			AssertEquals(-374.10M, transaction.Lines[1].AL_OSAmount);
			AssertEquals(-374.09M, transaction.Lines[2].AL_OSAmount);
			AssertEquals(-374.10M, transaction.Lines[3].AL_OSAmount);
			AssertEquals(558.62M, transaction.Lines[4].AL_OSAmount);
			AssertEquals(558.62M, transaction.Lines[5].AL_OSAmount);
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateInvoices_TypeIsAPInvoiceIfInvoiceSumPositive()
		{
			AssertCreateInvoiceTypeDependsOnSumOfInvoice(100, 90, 10, 0, 100, 90, 10, 0, typeof(APInvoice));
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateInvoices_TypeIsAPCreditNoteIfInvoiceSumZero()
		{
			AssertCreateInvoiceTypeDependsOnSumOfInvoice(100, 100, 0, 0, 100, 100, 0, 0, typeof(APCreditNote));
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateInvoices_TypeIsAPCreditNoteIfInvoiceSumNegative()
		{
			AssertCreateInvoiceTypeDependsOnSumOfInvoice(100, 110, -10, 0, 100, 110, -10, 0, typeof(APCreditNote));
		}

		void AssertCreateInvoiceTypeDependsOnSumOfInvoice(ZDecimal line1_CASSCostRowFileValue, ZDecimal line1_CASSCostAdjustedRowFileValue, ZDecimal line1_CASSCostTaxRowFileValue, ZDecimal line1_CASSCostTaxAdjustedRowFileValue,
														  ZDecimal line2_CASSCostRowFileValue, ZDecimal line2_CASSCostAdjustedRowFileValue, ZDecimal line2_CASSCostTaxRowFileValue, ZDecimal line2_CASSCostTaxAdjustedRowFileValue,
														  Type expectedInvoiceType)
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			SetupCASSBilling(TestCASSBilling, setupAmount: false, setAdjustmentValues: false);
			SetupAssociatedBizos();

			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).WeightChargePP = line1_CASSCostRowFileValue;
			(TestCASSBilling.Lines[0].AggregatedCostLine as CASSCostExportLine).VATDueAirline = line1_CASSCostTaxRowFileValue;

			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).WeightChargePP = line2_CASSCostRowFileValue;
			(TestCASSBilling.Lines[1].AggregatedCostLine as CASSCostExportLine).VATDueAirline = line2_CASSCostTaxRowFileValue;

			var adjustmentLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			adjustmentLine.AirlinePrefix = "172";
			adjustmentLine.AWBSerialNumber = "67828073";
			adjustmentLine.AgentCode = "23470/068-510";
			adjustmentLine.DateAWBExecution = threeMonthsAgo;
			adjustmentLine.DateOfArrival = twoMonthsAgo;
			adjustmentLine.DateOfDelivery = oneMonthAgo;
			adjustmentLine.Origin = "LEJ";
			adjustmentLine.Destination = "MEX";
			adjustmentLine.Weight = 2150M;
			adjustmentLine.WeightUnit = "KG";
			adjustmentLine.CurrencyCode = "AUD";
			adjustmentLine.WeightChargePP = line1_CASSCostAdjustedRowFileValue;
			adjustmentLine.VATDueAirline = line1_CASSCostTaxRowFileValue;
			adjustmentLine.AWBSerialNumber = "12345678";
			TestCASSBilling.Lines[0].AddCostLine(adjustmentLine, TestCASSBilling.Lines[0].CASSCostCurrencyCode);

			adjustmentLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			adjustmentLine.AirlinePrefix = "172";
			adjustmentLine.AWBSerialNumber = "67828073";
			adjustmentLine.AgentCode = "23470/068-510";
			adjustmentLine.DateAWBExecution = threeMonthsAgo;
			adjustmentLine.DateOfArrival = twoMonthsAgo;
			adjustmentLine.DateOfDelivery = oneMonthAgo;
			adjustmentLine.Origin = "LEJ";
			adjustmentLine.Destination = "MEX";
			adjustmentLine.Weight = 2150M;
			adjustmentLine.WeightUnit = "KG";
			adjustmentLine.CurrencyCode = "AUD";
			adjustmentLine.WeightChargePP = line2_CASSCostAdjustedRowFileValue;
			adjustmentLine.VATDueAirline = line1_CASSCostTaxRowFileValue;
			adjustmentLine.AWBSerialNumber = "12345678";
			TestCASSBilling.Lines[1].AddCostLine(adjustmentLine, TestCASSBilling.Lines[1].CASSCostCurrencyCode);

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertEquals(expectedInvoiceType, transaction.GetType());
		}

		[TestDate(2017, 09, 10)]
		public void TestCreateInvoices_CreditNote()
		{
			var today = ZDateTime.Today;

			SetupCASSBilling(TestCASSBilling, multiplier: -1, setAdjustmentValues: false);
			var adjustmentLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			adjustmentLine.AirlinePrefix = "172";
			adjustmentLine.AWBSerialNumber = "67828073";
			adjustmentLine.AgentCode = "23470068510";
			adjustmentLine.DateAWBExecution = today.AddMonths(-3);
			adjustmentLine.DateOfArrival = today.AddMonths(-2);
			adjustmentLine.DateOfDelivery = today.AddMonths(-1);
			adjustmentLine.Origin = "LEJ";
			adjustmentLine.Destination = "MEX";
			adjustmentLine.Weight = 2150M;
			adjustmentLine.WeightUnit = "KG";
			adjustmentLine.CurrencyCode = "AUD";
			adjustmentLine.WeightChargePP = 1015.68M;
			adjustmentLine.VATDueAirline = 101.56M;
			TestCASSBilling.Lines[0].AddCostLine(adjustmentLine, TestCASSBilling.Lines[0].CASSCostCurrencyCode);

			SetupAssociatedBizos();
			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals(1, TestCASSBilling.APTransactions.Count);
			InvoicingBase transaction = TestCASSBilling.APTransactions[0];
			AssertNotNull(transaction as APCreditNote);

			transaction = TestCASSBilling.GenerateCompleteInvoice(transaction);
			AssertNotNull(transaction as APCreditNote);
			Assert(transaction.Factory.HasContext(BusinessContext.CASS));
			Assert(!transaction.IsValidationSuspended);

			AssertEquals(6, transaction.Lines.Count);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[0].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[1].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[2].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[3].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[4].ApportionmentChargeImportedFrom);
			AssertNotNull("There is Apportionment Charge associated with the line.", transaction.Lines[5].ApportionmentChargeImportedFrom);

			TestCASSBilling.RunPreSaveValidation();
			AssertNoErrors(TestCASSBilling);
			TestCASSBilling.APTransactions.Factory.Save();

			AssertEquals("FRT", transaction.Lines[0].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[1].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[2].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[3].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[4].ChargeCode.AC_Code);
			AssertEquals("FRT", transaction.Lines[5].ChargeCode.AC_Code);
			if (Job1.PK == transaction.Lines[0].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[1].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[0].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[1].AL_JH);
			}
			if (Job1.PK == transaction.Lines[2].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[3].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[2].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[3].AL_JH);
			}
			if (Job1.PK == transaction.Lines[4].AL_JH)
			{
				AssertEquals(Job2.PK, transaction.Lines[5].AL_JH);
			}
			else
			{
				AssertEquals(Job2.PK, transaction.Lines[4].AL_JH);
				AssertEquals(Job1.PK, transaction.Lines[5].AL_JH);
			}
			AssertEquals(340.09M, transaction.Lines[0].AL_OSAmount);
			AssertEquals(340.09M, transaction.Lines[1].AL_OSAmount);
			AssertEquals(558.62M, transaction.Lines[2].AL_OSAmount);
			AssertEquals(558.62M, transaction.Lines[3].AL_OSAmount);
			AssertEquals(340.09M, transaction.Lines[4].AL_OSAmount);
			AssertEquals(340.09M, transaction.Lines[5].AL_OSAmount);
		}

		[TestDate(2017, 09, 10)]
		public void TestTotals()
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);
			var twoMonthsAgo = today.AddMonths(-2);
			var threeMonthsAgo = today.AddMonths(-3);

			var cassCostHeader = TestCASSBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < 2; i++)
			{
				var cassBillingLine = TestCASSBilling.Lines.AddNew();
				foreach (bool isAdjustment in new bool[] { false, true })
				{
					if (!(i == 0 && isAdjustment))
					{
						var costLine = new CASSCostExportLine(Factory, isAdjustment ? CASSCostLineType.Adjustment : CASSCostLineType.Billing);
						costLine.VATIndicator = "Y";
						costLine.AirlinePrefix = "172";
						costLine.AWBSerialNumber = "67828073";
						costLine.AgentCode = "23470/068-510";
						costLine.DateAWBExecution = threeMonthsAgo;
						costLine.DateOfArrival = twoMonthsAgo;
						costLine.DateOfDelivery = oneMonthAgo;
						costLine.Origin = "LEJ";
						costLine.Destination = "MEX";
						costLine.Weight = 2150M;
						costLine.WeightUnit = "KG";
						costLine.CurrencyCode = "AUD";
						costLine.WeightChargePP = isAdjustment ? 1015.68M : (i == 1 ? 900.00m : 680.18M);
						costLine.VATDueAirline = isAdjustment ? 101.56M : 68.01M;
						cassBillingLine.AddCostLine(costLine, "AUD");
					}
				}
			}

			SetupAssociatedBizos();
			Factory.Save();

			AssertEquals("Precondition:", 2, TestCASSBilling.Lines.Count);

			AccountingConfigurationRegistry.Instance.CASSCostImportAllowedDiscrepancy.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, 400);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);

			TestCASSBilling.ForceRecalculateData();

			AssertEquals(1, TestCASSBilling.Lines.Count);

			AssertEquals(0M, TestCASSBilling.TotalCASSCostAdjustedValue);
			AssertEquals(680.18M, TestCASSBilling.TotalCASSCostValue);
			AssertEquals(0M, TestCASSBilling.TotalCASSRejectedClaimValue);
			AssertEquals(-480.18M, TestCASSBilling.TotalCostDifferenceValue);
			AssertEquals(680.18M, TestCASSBilling.TotalNetCASSCostValue);
			AssertEquals(200M, TestCASSBilling.TotalSystemCostAccrualValue);

			AssertEquals(-1015.68M, TestCASSBilling.TotalHiddenCASSCostAdjustedValue);
			AssertEquals(900M, TestCASSBilling.TotalHiddenCASSCostValue);
			AssertEquals(0M, TestCASSBilling.TotalHiddenCASSRejectedClaimValue);
			AssertEquals(315.68M, TestCASSBilling.TotalHiddenCostDifferenceValue);
			AssertEquals(-115.68M, TestCASSBilling.TotalHiddenNetCASSCostValue);
			AssertEquals(200M, TestCASSBilling.TotalHiddenSystemCostAccrualValue);

			AssertEquals(-1015.68M, TestCASSBilling.TotalAllCASSCostAdjustedValue);
			AssertEquals(1580.18M, TestCASSBilling.TotalAllCASSCostValue);
			AssertEquals(0M, TestCASSBilling.TotalAllCASSRejectedClaimValue);
			AssertEquals(-164.5M, TestCASSBilling.TotalAllCostDifferenceValue);
			AssertEquals(564.5M, TestCASSBilling.TotalAllNetCASSCostValue);
			AssertEquals(400M, TestCASSBilling.TotalAllSystemCostAccrualValue);

			var costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Rejected);
			costLine1.VATIndicator = "Y";
			costLine1.AirlinePrefix = "172";
			costLine1.AWBSerialNumber = "67828073";
			costLine1.AgentCode = "23470/068-510";
			costLine1.DateAWBExecution = threeMonthsAgo;
			costLine1.DateOfArrival = twoMonthsAgo;
			costLine1.DateOfDelivery = oneMonthAgo;
			costLine1.Origin = "LEJ";
			costLine1.Destination = "MEX";
			costLine1.Weight = 2150M;
			costLine1.WeightUnit = "KG";
			costLine1.CurrencyCode = "AUD";
			costLine1.WeightChargePP = 0M;
			costLine1.VATDueAirline = 0M;
			TestCASSBilling.Lines[0].AddCostLine(costLine1, "AUD");

			TestCASSBilling.ForceRecalculateData();

			AssertEquals(0M, TestCASSBilling.TotalCASSCostAdjustedValue);
			AssertEquals(0M, TestCASSBilling.TotalCASSCostValue);
			AssertEquals(680.18M, TestCASSBilling.TotalCASSRejectedClaimValue);
			AssertEquals(-480.18M, TestCASSBilling.TotalCostDifferenceValue);
			AssertEquals(680.18M, TestCASSBilling.TotalNetCASSCostValue);
			AssertEquals(200M, TestCASSBilling.TotalSystemCostAccrualValue);

			AssertEquals(-1015.68M, TestCASSBilling.TotalHiddenCASSCostAdjustedValue);
			AssertEquals(900M, TestCASSBilling.TotalHiddenCASSCostValue);
			AssertEquals(0M, TestCASSBilling.TotalHiddenCASSRejectedClaimValue);
			AssertEquals(315.68M, TestCASSBilling.TotalHiddenCostDifferenceValue);
			AssertEquals(-115.68M, TestCASSBilling.TotalHiddenNetCASSCostValue);
			AssertEquals(200M, TestCASSBilling.TotalHiddenSystemCostAccrualValue);

			AssertEquals(-1015.68M, TestCASSBilling.TotalAllCASSCostAdjustedValue);
			AssertEquals(900M, TestCASSBilling.TotalAllCASSCostValue);
			AssertEquals(680.18M, TestCASSBilling.TotalAllCASSRejectedClaimValue);
			AssertEquals(-164.5M, TestCASSBilling.TotalAllCostDifferenceValue);
			AssertEquals(564.5M, TestCASSBilling.TotalAllNetCASSCostValue);
			AssertEquals(400M, TestCASSBilling.TotalAllSystemCostAccrualValue);

			costLine1 = new CASSCostExportLine(Factory, CASSCostLineType.Rejected);
			costLine1.VATIndicator = "Y";
			costLine1.AirlinePrefix = "172";
			costLine1.AWBSerialNumber = "67828073";
			costLine1.AgentCode = "23470/068-510";
			costLine1.DateAWBExecution = threeMonthsAgo;
			costLine1.DateOfArrival = twoMonthsAgo;
			costLine1.DateOfDelivery = oneMonthAgo;
			costLine1.Origin = "LEJ";
			costLine1.Destination = "MEX";
			costLine1.Weight = 2150M;
			costLine1.WeightUnit = "KG";
			costLine1.CurrencyCode = "AUD";
			costLine1.WeightChargePP = 0M;
			costLine1.VATDueAirline = 0M;
			TestCASSBilling.HiddenLines[0].AddCostLine(costLine1, TestCASSBilling.Lines[0].CASSCostCurrencyCode);

			TestCASSBilling.ForceRecalculateData();

			AssertEquals(0M, TestCASSBilling.TotalCASSCostAdjustedValue);
			AssertEquals(0M, TestCASSBilling.TotalCASSCostValue);
			AssertEquals(680.18M, TestCASSBilling.TotalCASSRejectedClaimValue);
			AssertEquals(-480.18M, TestCASSBilling.TotalCostDifferenceValue);
			AssertEquals(680.18M, TestCASSBilling.TotalNetCASSCostValue);
			AssertEquals(200M, TestCASSBilling.TotalSystemCostAccrualValue);

			AssertEquals(-1015.68M, TestCASSBilling.TotalHiddenCASSCostAdjustedValue);
			AssertEquals(0M, TestCASSBilling.TotalHiddenCASSCostValue);
			AssertEquals(900M, TestCASSBilling.TotalHiddenCASSRejectedClaimValue);
			AssertEquals(315.68M, TestCASSBilling.TotalHiddenCostDifferenceValue);
			AssertEquals(-115.68M, TestCASSBilling.TotalHiddenNetCASSCostValue);
			AssertEquals(200M, TestCASSBilling.TotalHiddenSystemCostAccrualValue);

			AssertEquals(-1015.68M, TestCASSBilling.TotalAllCASSCostAdjustedValue);
			AssertEquals(0M, TestCASSBilling.TotalAllCASSCostValue);
			AssertEquals(1580.18M, TestCASSBilling.TotalAllCASSRejectedClaimValue);
			AssertEquals(-164.5M, TestCASSBilling.TotalAllCostDifferenceValue);
			AssertEquals(564.5M, TestCASSBilling.TotalAllNetCASSCostValue);
			AssertEquals(400M, TestCASSBilling.TotalAllSystemCostAccrualValue);
		}

		public void TestCreateInvoices_NotCreateInvoicesWithoutLines()
		{
			SetupCASSBilling(TestCASSBilling, 0);
			SetupAssociatedBizos();
			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			TestCASSBilling.CreateInvoices();

			AssertEquals("Invoice without lines must not be created.", 0, TestCASSBilling.APTransactions.Count);
		}

		public void TestIsRejectedClaimLinesExpected()
		{
			AssertEquals("IsRejectedClaimLinesExpected", false, CASSBilling.IsRejectedClaimLinesExpected);

			GlbCompany.CurrentCompany.SetCountry(Constants.CountryCodes.UnitedStates);
			AssertEquals("IsRejectedClaimLinesExpected", true, CASSBilling.IsRejectedClaimLinesExpected);
		}

		[TestDate(2017, 10, 1)]
		public void TestPerformanceCASS()
		{
			//The bottleneck that slows down the import is on setting 'E6_AC_ChargeCode' and we should 
			//sure about 'GetSuspenderForConsolCostImporter' has been called on 'CreateInvoices' (the deptor
			//should not set on 'CreateInvoices' and deptor should set on 'PostAPTransactions'
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			SetupCASSBilling(TestCASSBilling);
			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			CASSBillingLine cassLine = TestCASSBilling.Lines[1];
			TestObjectCreator.SetupCASSBillingLine(cassLine);
			Consol = TestObjectCreator.CreateConsol(cassLine.LoadPortIATA, cassLine.DischargePortIATA, "C0001");
			Consol.JK_MasterBillNum = cassLine.MAWBNumber;
			Job1 = TestObjectCreator.CreateJob(TestObjectCreator.CreateShipment("SHIP1", cassLine.LoadPortIATA, cassLine.DischargePortIATA, Consol));
			var feaDepartment = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")).PK;
			Job1.JH_GE = feaDepartment;
			TestObjectCreator.CreateCharge(Job1, TestObjectCreator.FRT, "FRT", cassLine.CASSCostCurrency, 100M, TestObjectCreator.Agent2, cassLine.CASSCostCurrency, 100M, TestObjectCreator.AALSHI).JR_GE = feaDepartment;
			TestObjectCreator.CreateCharge(Job1, TestObjectCreator.FRT, "FRT", cassLine.CASSCostCurrency, 100M, TestObjectCreator.Agent2, cassLine.CASSCostCurrency, 100M, TestObjectCreator.Agent).JR_GE = feaDepartment;

			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(cassLine.AirlinePrefix), TestObjectCreator.AALSHI);
			Factory.Save();
			TestCASSBilling.ForceRecalculateData();

			AssertCASSGstRegistryAndCallCreateInvoices();
			AssertNull("The suspender is active and RelatedJobCharge should be null", TestCASSBilling.APTransactions[0].Lines[0].RelatedJobCharge);

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			Assert("The invoice should be posted", TestCASSBilling.PostAPTransactions());
			APInvoice[] expextedInvoices = newFactory.Load<APInvoice>(new ZQuery(AccTransactionHeaderSchema.AH_OH, TestObjectCreator.AALSHI.PK).AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK));
			AssertEquals("We should have only have one Invoice", 1, expextedInvoices.Length);
			Assert("We should have only have more than zero Lines on Invoice", expextedInvoices[0].Lines.Count > 0);
			AssertNotNull("The suspender is inactive and RelatedJobCharge should be created", expextedInvoices[0].Lines[0].RelatedJobCharge);
			AssertNotNull("The suspender is inactive so debtor should be set after posting", expextedInvoices[0].Lines[0].RelatedJobCharge.JR_OH_SellAccount);
		}

		public void TestOverseasCostTaxAmountNotRecalculatedForCreditNote()
		{
			var testAirLinePrefix = "172";
			var airLineOrg = TestObjectCreator.AALSHI;
			var airline = TestObjectCreator.CreateAirLine(testAirLinePrefix);
			airLineOrg.MiscServ.OM_RM_Airline = airline.PK;

			TestObjectCreator.CreateExchangeRate(RefCurrency.LoadFromCurrencyCode(Factory, "EUR"), 1);
			SetupCASSBilling(TestCASSBilling, 1);

			var cassBillingLine = TestCASSBilling.Lines[0];
			(cassBillingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 1000.00M;
			(cassBillingLine.AggregatedCostLine as CASSCostExportLine).VATDueAirline = 100.01M;

			var adjustmentCostLine = new CASSCostExportLine(Factory, CASSCostLineType.Adjustment);
			adjustmentCostLine.CurrencyCode = "AUD";
			adjustmentCostLine.WeightChargePP = 1200.00M;
			adjustmentCostLine.VATDueAirline = 120.00M;
			adjustmentCostLine.AirlinePrefix = testAirLinePrefix;
			cassBillingLine.AddCostLine(adjustmentCostLine, cassBillingLine.CASSCostCurrencyCode);

			SetupAssociatedBizos();

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();
			AssertCASSGstRegistryAndCallCreateInvoices();

			AssertEquals("AP Transactions created", 1, TestCASSBilling.APTransactions.Count);
			var apCreditNote = TestCASSBilling.APTransactions[0] as APCreditNote;
			AssertNotNull("AP Credit Note", apCreditNote);
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			AssertNoExceptionThrown("Should not get critical validation error due to overseas cost tax amount being recalculated to 100.00 (should stay at 100.01 as per CASS import)",
				() => TestCASSBilling.PostAPTransactions());
		}

		public void TestAPInvoiceDefaultExpectedTotalValue()
		{
			bool originalValue = AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.Value;
			try
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				Env.Security.AllowAPInvoiceChangeDefaultExpectedTotalValue.IsAllowed = false;
				SetupCASSBilling(TestCASSBilling, setAdjustmentValues: false);
				SetupAssociatedBizos();
				Factory.Save();

				TestCASSBilling.ForceRecalculateData();
				AssertCASSGstRegistryAndCallCreateInvoices();
				InvoicingBase transaction = TestCASSBilling.APTransactions[0];
				AssertNotNull("AP invoice", transaction as APInvoice);
				AssertEquals("Precondition: user does not have security right to modify the 'Expected Total' tick box on AP invoices.", false, Env.Security.AllowAPInvoiceChangeDefaultExpectedTotalValue.IsAllowed);
				AssertNoRowErrorContaining(transaction, @"You do not have sufficient security rights to modify the 'Expected Total' tick box.
Please contact your system administrator for right to modify this field.
The location of this security right is as follows: Accounts > Payables > Payables Transactions > New Transactions > Invoice > Allow Change Default Expected Total Value");
			}
			finally
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalValue);
			}
		}

		public void TestInvoiceNumberDuplicateInputsWithSameCreditorButDifferentCurrency()
		{
			String expectedInvoiceNumberDuplicateInCollectionError = "This invoice number is already used on another invoice in this batch. Invoice numbers must be unique by creditor and currency.";

			var org = TestObjectCreator.ABIGAS;
			org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
			org.OH_IsCreditor = true;
			Factory.Save();

			CASSBillingLine cassLine1 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine1);
			CASSBillingLine cassLine2 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine2, true, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828075", agentCode: "23470/068-510", weight: 2150M, currency: "USD");
			TestCASSBilling.Lines.Add(cassLine2);
			CASSBillingLine cassLine3 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine3, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828076", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine3);

			TestCASSBilling.Lines[0].InvoiceNumber = "CASS1";
			TestCASSBilling.Lines[1].InvoiceNumber = "CASS1";
			TestCASSBilling.Lines[2].InvoiceNumber = "CASS1";

			AssertEquals("There should be a critical error for invoice generation.", true, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);
			AssertHasError("This invoice number is already used on another invoice in this batch with the same creditor and currency.", TestCASSBilling.Lines[0].InvoiceNumberInfo, expectedInvoiceNumberDuplicateInCollectionError);
			AssertHasError("This invoice number is already used on another invoice in this batch with the same creditor and currency.", TestCASSBilling.Lines[1].InvoiceNumberInfo, expectedInvoiceNumberDuplicateInCollectionError);
			AssertHasError("This invoice number is already used on another invoice in this batch with the same creditor and currency.", TestCASSBilling.Lines[2].InvoiceNumberInfo, expectedInvoiceNumberDuplicateInCollectionError);

			TestCASSBilling.Lines[1].InvoiceNumber = "CASS2";

			AssertEquals("There should be no critical error for invoice generation.", false, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);
			AssertNoError("There should be no error in the property info.", TestCASSBilling.Lines[0].InvoiceNumberInfo, expectedInvoiceNumberDuplicateInCollectionError);
			AssertNoError("There should be no error in the property info.", TestCASSBilling.Lines[1].InvoiceNumberInfo, expectedInvoiceNumberDuplicateInCollectionError);
			AssertNoError("There should be no error in the property info.", TestCASSBilling.Lines[2].InvoiceNumberInfo, expectedInvoiceNumberDuplicateInCollectionError);
		}

		[TestDate(2017, 12, 5)]
		public void TestInvoiceNumberDuplicateInputsInDatabase_Standard()
		{
			AssertInvoiceNumberDuplicateInputsInDatabase(
				AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD,
				costHeaderDateOfBilling: ZDateTime.Today.AddMonths(AccountingUtils.DuplicateInvoiceNumberPeriodMonths));
		}

		[TestDate(2017, 12, 5)]
		public void TestInvoiceNumberDuplicateInputsInDatabase_Calendar()
		{
			AssertInvoiceNumberDuplicateInputsInDatabase(
				AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.CAL,
				costHeaderDateOfBilling: new ZDateTime(ZDateTime.Today.Year + 1, 1, 1));
		}

		void AssertInvoiceNumberDuplicateInputsInDatabase(string allowDuplicateInvoiceNumberRule, ZDateTime costHeaderDateOfBilling)
		{
			string expInvoiceNumberDuplicateMessage = CreateDuplicateNumberErrorMessage(allowDuplicateInvoiceNumberRule);
			string expInvoiceNumberDuplicateMessageNoPerm = CreateDuplicateNumberErrorMessageNoPermission(allowDuplicateInvoiceNumberRule);

			using (AccountingMasterFilesRegistry.Instance.AllowDuplicateInvoiceNumberDefaultingRule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, allowDuplicateInvoiceNumberRule))
			{
				var org = TestObjectCreator.ABIGAS;
				org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
				org.OH_IsCreditor = true;

				AccTransactionHeader aPInvoice = TestObjectCreator.InsertTransaction(TransactionTypes.Invoice);
				aPInvoice.AH_OH = org.PK;
				aPInvoice.AH_Ledger = LedgerTypes.AccountsPayable;
				aPInvoice.AH_TransactionNum = "CASS1";
				Factory.Save();

				CASSBillingLine cassLine1 = new CASSBillingLine(Factory);
				TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				TestCASSBilling.Lines.Add(cassLine1);
				CASSBillingLine cassLine2 = new CASSBillingLine(Factory);
				TestObjectCreator.SetupCASSCostComponent(cassLine2, false, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828075", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				TestCASSBilling.Lines.Add(cassLine2);
				CASSBillingLine cassLine3 = new CASSBillingLine(Factory);
				TestObjectCreator.SetupCASSCostComponent(cassLine3, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828076", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
				TestCASSBilling.Lines.Add(cassLine3);

				TestCASSBilling.Lines[0].InvoiceNumber = "CASS1";
				TestCASSBilling.Lines[1].InvoiceNumber = "CASS1";
				TestCASSBilling.Lines[2].InvoiceNumber = "CASS2";

				AssertEquals("There should be a critical error for invoice generation.", true, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);
				AssertHasError("The transaction number is already in use.", TestCASSBilling.Lines[0].InvoiceNumberInfo, expInvoiceNumberDuplicateMessage);
				AssertHasError("The transaction number is already in use.", TestCASSBilling.Lines[1].InvoiceNumberInfo, expInvoiceNumberDuplicateMessage);
				AssertNoError("There should be no error in the property info.", TestCASSBilling.Lines[2].InvoiceNumberInfo, expInvoiceNumberDuplicateMessage);

				TestCASSBilling.CostHeader.DateOfBilling = costHeaderDateOfBilling;
				AssertEquals("There should be no critical errors for invoice generation.", false, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);
				AssertNoErrors("The transaction number is used outside of period.", TestCASSBilling.Lines[0].InvoiceNumberInfo);
				AssertNoErrors("The transaction number is used outside of period.", TestCASSBilling.Lines[1].InvoiceNumberInfo);
				AssertNoErrors("There should be no error in the property info.", TestCASSBilling.Lines[2].InvoiceNumberInfo);

				Env.Security.NewPayablesDuplicateInvoiceNumber.IsAllowed = false;
				AssertEquals("There should be a critical error for invoice generation.", true, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);
				AssertHasError("The transaction number is already in use.", TestCASSBilling.Lines[0].InvoiceNumberInfo, expInvoiceNumberDuplicateMessageNoPerm);
				AssertHasError("The transaction number is already in use.", TestCASSBilling.Lines[1].InvoiceNumberInfo, expInvoiceNumberDuplicateMessageNoPerm);
				AssertNoError("There should be no error in the property info.", TestCASSBilling.Lines[2].InvoiceNumberInfo, expInvoiceNumberDuplicateMessageNoPerm);
			}

			ZString CreateDuplicateNumberErrorMessage(string ruleCode)
			{
				var messageDetails = ruleCode == AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD ? "less than 12 months apart" : "in the same calendar year";
				return ZString.Format("The transaction number is already in use. Last posted transaction’s invoice date is 05-Dec-17 which is {0}. This transaction number cannot be used. Please enter another one.", messageDetails);
			}

			ZString CreateDuplicateNumberErrorMessageNoPermission(string ruleCode)
			{
				var messageDetails = ruleCode == AccountingMasterFilesConstants.AllowDuplicateInvoiceNumberRule.STD ? "at least 12 months apart" : "in another calendar year";
				return ZString.Format("The transaction number is already in use. Last posted transaction’s invoice date is 05-Dec-17 which is {0}. You are not granted security right to Payables > Payables Transactions > New Transactions > Allow Duplicate AP Invoice Number. This transaction number cannot be used. Please enter another one.", messageDetails);
			}
		}

		public void TestCreateInvoices_WithNoInvoiceNumberInput()
		{
			var org = TestObjectCreator.ABIGAS;
			org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
			org.OH_IsCreditor = true;
			Factory.Save();

			CASSBillingLine cassLine1 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine1);
			CASSBillingLine cassLine2 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine2, true, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828075", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine2);
			CASSBillingLine cassLine3 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine3, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828076", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine3);

			AssertEquals("There should be no critical errors for invoice generation.", false, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);

			TestCASSBilling.CreateInvoices();

			AssertEquals("There should be 1 invoice generated.", 1, TestCASSBilling.APTransactions.Count);
			AssertStartsWith("Invoice number should be autogenerated.", "CASSAUD", TestCASSBilling.APTransactions[0].AH_TransactionNum);
		}

		public void TestCreateInvoices_WithInvoiceNumberInput()
		{
			var org = TestObjectCreator.ABIGAS;
			org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
			org.OH_IsCreditor = true;
			Factory.Save();

			CASSBillingLine cassLine1 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine1);
			CASSBillingLine cassLine2 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine2, true, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828075", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine2);
			CASSBillingLine cassLine3 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine3, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828076", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine3);

			String transactionNumber = "CASS1";

			TestCASSBilling.Lines[0].InvoiceNumber = transactionNumber;
			TestCASSBilling.Lines[1].InvoiceNumber = transactionNumber;
			TestCASSBilling.Lines[2].InvoiceNumber = transactionNumber;

			AssertEquals("There should be no critical errors for invoice generation.", false, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);

			TestCASSBilling.CreateInvoices();

			AssertEquals("There should be 1 invoice generated.", 1, TestCASSBilling.APTransactions.Count);
			AssertEquals("Invoice number should be the invoice number input.", transactionNumber, TestCASSBilling.APTransactions[0].AH_TransactionNum);
		}

		public void TestCreateInvoices_SomeWithInvoiceNumberInput()
		{
			var org = TestObjectCreator.ABIGAS;
			org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
			org.OH_IsCreditor = true;
			Factory.Save();

			CASSBillingLine cassLine1 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine1);
			CASSBillingLine cassLine2 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine2, true, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828075", agentCode: "23470/068-510", weight: 2150M, currency: "USD");
			TestCASSBilling.Lines.Add(cassLine2);
			CASSBillingLine cassLine3 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine3, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828076", agentCode: "23470/068-510", weight: 2150M, currency: "AUD");
			TestCASSBilling.Lines.Add(cassLine3);

			String transactionNumber = "CASS1";

			TestCASSBilling.Lines[1].InvoiceNumber = transactionNumber;

			AssertEquals(false, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);

			TestCASSBilling.CreateInvoices();

			AssertEquals("There should be 2 invoices generated.", 2, TestCASSBilling.APTransactions.Count);
			AssertStartsWith("Invoice number should be autogenerated.", "CASSAUD", TestCASSBilling.APTransactions[0].AH_TransactionNum);
			AssertEquals("Invoice number should be the invoice number input.", transactionNumber, TestCASSBilling.APTransactions[1].AH_TransactionNum);
		}

		[TestDate(2018, 09, 22)]
		public void TestCreateInvoices_WithInvoiceNumberContainingInvalidSuffix()
		{
			var org = TestObjectCreator.ABIGAS;
			org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
			org.OH_IsCreditor = true;
			Factory.Save();

			CASSBillingLine cassLine1 = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "USD");
			TestCASSBilling.Lines.Add(cassLine1);

			TestCASSBilling.Lines[0].InvoiceNumber = "CASSUSD180922.";
			AssertEquals(true, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);

			TestCASSBilling.Lines[0].InvoiceNumber = "CASSUSD180922/A";
			AssertEquals(false, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);

			TestCASSBilling.Lines[0].InvoiceNumber = "TESTMANUALINVNUMBER.";
			AssertEquals(false, TestCASSBilling.HasCriticalErrorsForInvoiceCreation);
		}

		public void TestCreateInvoices_WithExchangeRateValidation()
		{
			var org = TestObjectCreator.ABIGAS;
			org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
			org.OH_IsCreditor = true;
			Factory.Save();

			SetupAssociatedBizos(false);

			Shipment1.JS_UnitOfWeight = Constants.Weight.Kilograms;
			Shipment1.JS_UnitOfVolume = Constants.Volume.CubicMetres;
			Shipment1.JS_ActualWeight = 990;
			Shipment1.JS_ActualVolume = 1000;

			var cost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI);
			cost.E6_LocalCostAmount = 500;
			cost.E6_OSCostAmount = 250;
			cost.E6_ApportionmentMethod = AllocationMethod.Manual;

			Factory.Save();

			var invoiceDate = new CASSCostHeader
			{
				DateOfBilling = new ZDateTime(2024, 12, 22)
			};
			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, currencyCode: "EUR");
			TestCASSBilling.CostHeader.DateOfBilling = new ZDateTime(2024, 12, 22);

			Factory.Save();
			TestCASSBilling.CreateInvoices();
			AssertEquals("Should have no invoices due to missing exchange rate", 0, TestCASSBilling.APTransactions.Count);
			AssertEquals("BUY Exchange rate for invoice date 22-Dec-24 is missing", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestCreateAndPostInvoices_WithComplianceNumbering()
		{
			var currCompany = GlbCompany.CurrentCompany;
			using (currCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			{
				var org = TestObjectCreator.ABIGAS;
				org.MiscServ.OM_RM_Airline = TestObjectCreator.CreateAirLine("081").PK;
				org.OH_IsCreditor = true;
				org.CompanyData.SetAPTaxApplicableIgnoringRegistrySetting(true);

				const string subType = "APS";
				var sequence = Factory.NewWithValidTestData<AccComplianceSequence>();
				sequence.XD_SequenceClass = subType;
				sequence.XD_Code = "APS1";
				sequence.XD_StartNumber = 1;
				sequence.XD_EndNumber = 100;
				sequence.XD_NextNumber = 1;
				sequence.XD_MaximumNumberDigits = 9;
				sequence.XD_GC_Company = currCompany.PK;
				sequence.XD_GB_BranchOwner = GlbBranch.CurrentBranch.PK;

				TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

				Factory.Save();

				var testCassLines = TestCASSBilling.Lines;

				var cassLine1 = new CASSBillingLine(Factory);
				TestObjectCreator.SetupCASSCostComponent(cassLine1, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828074", agentCode: "23470/068-510", weight: 2150M, currency: "EUR");
				testCassLines.Add(cassLine1);
				var cassLine2 = new CASSBillingLine(Factory);
				TestObjectCreator.SetupCASSCostComponent(cassLine2, true, 101568M, 0m, 0m, 0m, 0m, 0m, adjustedVatDueAirlineAmount: 10156M, airlinePrefix: "081", awbSerialNumber: "67828075", agentCode: "23470/068-510", weight: 2150M, currency: "EUR");
				testCassLines.Add(cassLine2);
				var cassLine3 = new CASSBillingLine(Factory);
				TestObjectCreator.SetupCASSCostComponent(cassLine3, false, 68018M, 0m, 0m, 0m, 0m, 0m, vatDueAirlineAmount: 6801M, airlinePrefix: "081", awbSerialNumber: "67828076", agentCode: "23470/068-510", weight: 2150M, currency: "EUR");
				testCassLines.Add(cassLine3);

				const string transactionNumber = "CASS1";
				testCassLines[0].InvoiceNumber = transactionNumber;
				testCassLines[1].InvoiceNumber = transactionNumber;
				testCassLines[2].InvoiceNumber = transactionNumber;

				Assert(!TestCASSBilling.HasCriticalErrorsForInvoiceCreation);

				var countryCode = currCompany.Country.Code;

				var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
				var config = collection.AddNew();
				config.Country = countryCode;
				config.SubType = subType;
				config.LedgerType = LedgerTypes.AccountsPayable;
				config.InvoiceType = TransactionTypes.Invoice;
				config.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
				config.DisbursementRule = DisbursementRuleCodes.AllTransactions;
				config.OriginalRule = OriginalRuleCodes.AllTransactions;

				var masterRegistry = AccountingMasterFilesRegistry.Instance;
				var configRegistry = AccountingConfigurationRegistry.Instance;
				var guid_GC = currCompany.PK.ToGuid();

				using (masterRegistry.ComplianceNumberAllocationDate_AP.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccountingMasterFilesConstants.ComplianceNumberAllocationDateOptions.PostDate.Code))
				using (masterRegistry.ComplianceDocumentNumberAllocation_Payables.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingCodes.Post))
				using (masterRegistry.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, collection))
				using (configRegistry.CASSGLAccount.SetTemporaryValue(guid_GC, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid()))
				{
					var taxRate0 = TestObjectCreator.CreateTaxRate("NOT", "Not reportable", AccTaxRate.Types.NotReportable, 0, ZString.Empty, 0, 1, countryCode);
					var taxRate1 = TestObjectCreator.CreateTaxRate("NOT", "Not Reportable", AccTaxRate.Types.NotReportable, 10, AccTaxRate.ExtraTypes.StateGST, 0, 1, countryCode);

					var registryValue = new CASSFileImportDefaultTaxID();
					registryValue.ZeroRatedTaxID = taxRate0.PK;
					registryValue.StandardRatedTaxID = taxRate1.PK;
					configRegistry.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue);

					TestCASSBilling.CreateInvoices();
					var testCassAPTransactions = TestCASSBilling.APTransactions;
					AssertEquals(1, testCassAPTransactions.Count);
					var testAPTransaction = testCassAPTransactions[0];
					Assert(testAPTransaction.Factory.HasContext(BusinessContext.CASS));
					AssertCollectionNotContains(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage, testAPTransaction.Notifications);

					TestCASSBilling.PostAPTransactions();
					testCassAPTransactions = TestCASSBilling.APTransactions;
					AssertEquals(1, testCassAPTransactions.Count);
					testAPTransaction = testCassAPTransactions[0];
					Assert(testAPTransaction.Factory.HasContext(BusinessContext.CASS));
					AssertCollectionNotContains(ComplianceSequenceNumberAllocationErrorMessages.FailedToFindComplianceSequenceMessage, testAPTransaction.Notifications);

					var completeInvoice = TestCASSBilling.GenerateCompleteInvoice(testAPTransaction);
					AssertEquals(subType, completeInvoice.AH_ComplianceSubType);
				}
			}
		}

		void AssertDistributedCost(InvoicingBase invoice, AccChargeCode chargeCode, params Tuple<ZDecimal, ZDecimal>[] costAndTax)
		{
			var costs = invoice.ConsolCosting.ConsolCosts.Where(x => x.E6_AC_ChargeCode == chargeCode.PK).ToArray();
			AssertEquals("Number of Costs " + chargeCode.AC_Code, costAndTax.Length, costs.Length);
			costAndTax.ForEach(x => Assert("Cost " + x.Item1.ToString() + "Should Exist for " + chargeCode.AC_Code, costs.Any(c => c.E6_OSCostAmount == x.Item1)));
			costAndTax.ForEach(x => Assert("Tax " + x.Item2.ToString() + "Should Exist for " + chargeCode.AC_Code, costs.Any(c => c.E6_OSGSTAmount_Calc == x.Item2)));
		}

		public void TestCreateInvoices_TaxShouldBeCASSDefaultTaxID_WontbeOverriden()
		{
			IncludeCurrentDepartmentForChargeCodeFRT();
			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false);

			SetupAssociatedBizos();
			var consolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, null, 1200m, true, AllocationMethod.Shipment);
			Factory.Save();

			AssertEquals("Precondition: charge count", 2, consolCost.ApportionmentCharges.Count);
			var charge1 = consolCost.ApportionmentCharges.FindChargeForJob(Shipment1);
			var charge2 = consolCost.ApportionmentCharges.FindChargeForJob(Shipment2);
			AssertEquals("Precondition: charge1 JR_LocalCostAmt", 600m, charge1.JR_LocalCostAmt);
			AssertEquals("Precondition: charge2 JR_LocalCostAmt", 600m, charge2.JR_LocalCostAmt);

			var defaultTaxId = new CASSFileImportDefaultTaxID();
			defaultTaxId.StandardRatedTaxID = TestObjectCreator.GST1.PK;
			defaultTaxId.ZeroRatedTaxID = TestObjectCreator.FREECAPGST.PK;

			var registryValue = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			var registryValueEnumerable = registryValue.OfType<CodeDescriptionBool>();
			registryValueEnumerable.ForEach(x => x.Bool = true);

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
			using (AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultTaxId))
			using (AccountingConfigurationRegistry.Instance.CASSGLAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid()))
			{
				TestCASSBilling.ForceRecalculateData();
				TestCASSBilling.CreateInvoices();
				var testCassAPTransactions = TestCASSBilling.APTransactions;
				AssertEquals(1, testCassAPTransactions.Count);
				var testAPTransaction = testCassAPTransactions[0];
				Assert(testAPTransaction.Factory.HasContext(BusinessContext.CASS));

				var completeInvoice = TestCASSBilling.GenerateCompleteInvoice(testAPTransaction);
				Assert(!completeInvoice.IsValidationSuspended);
				AssertEquals(TestObjectCreator.FREECAPGST.PK, completeInvoice.Lines[0].AL_AT);
				AssertEquals(TestObjectCreator.FREECAPGST.PK, completeInvoice.Lines[1].AL_AT);
			}

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, registryValue))
			using (AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultTaxId))
			using (AccountingConfigurationRegistry.Instance.CASSGLAccount.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid()))
			using (AccountingMasterFilesRegistry.Instance.EnableBranchLevelTaxOverrideRuleConfigurations.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true))
			{
				TestCASSBilling.ForceRecalculateData();
				TestCASSBilling.CreateInvoices();
				var testCassAPTransactions = TestCASSBilling.APTransactions;
				AssertEquals(1, testCassAPTransactions.Count);
				var testAPTransaction = testCassAPTransactions[0];
				Assert(testAPTransaction.Factory.HasContext(BusinessContext.CASS));

				var completeInvoice = TestCASSBilling.GenerateCompleteInvoice(testAPTransaction);
				Assert(!completeInvoice.IsValidationSuspended);
				AssertEquals(TestObjectCreator.FREECAPGST.PK, completeInvoice.Lines[0].AL_AT);
				AssertEquals(TestObjectCreator.FREECAPGST.PK, completeInvoice.Lines[1].AL_AT);
			}
		}

		public void TestCreateInvoices_WhenConsoleCostTaxIDSimilarToCassDefaultTaxID_NonItaly_TaxMassageIsTaxIDDefaultMessage()
		{
			AssertNotEquals("Logic for Non-Italy", Constants.CountryCodes.Italy, GlbCompany.CurrentCompany.Country.Code);
			AssertTaxMassage_WhenConsoleCostTaxIDSimilarToCassDefaultTaxID(false, TestObjectCreator.TaxMsg1.PK);
		}

		public void TestCreateInvoices_WhenConsoleCostTaxIDSimilarToCassDefaultTaxID_InItaly_TaxMessageIsTaxOverrideMessage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Italy))
			using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Constants.CurrencyCodes.Italy))
			{
				AssertTaxMassage_WhenConsoleCostTaxIDSimilarToCassDefaultTaxID(false, TestObjectCreator.TaxMsg3.PK);
			}
		}

		public void TestCreateInvoices_WhenAccrualIsZero_CountryNotImportant_TaxMassageIsTaxIDDefaultMessage()
		{
			AccountingConfigurationRegistry.Instance.CASSChargeCodes.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory));
			AssertTaxMassage_WhenConsoleCostTaxIDSimilarToCassDefaultTaxID(true, TestObjectCreator.TaxMsg1.PK);
		}

		void AssertTaxMassage_WhenConsoleCostTaxIDSimilarToCassDefaultTaxID(bool isCaseForZeroSystemCostAccrualValue, ZGuid expectedTaxMsgPK)
		{
			TestObjectCreator.AALSHI.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			TestObjectCreator.CreateTaxOverride(TestObjectCreator.FRT, TestObjectCreator.GST1.PK, TestObjectCreator.TaxMsg4.PK);

			var defaultTaxId = CreateCassDefaultTaxId();

			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultTaxId);

			SetupCASSBilling(TestCASSBilling, 1, setAdjustmentValues: false, currencyCode: GlbCompany.CurrentCompany.LocalCurrency.Code);
			SetupAssociatedBizos(true, GlbCompany.CurrentCompany.LocalCurrency.Code);

			Assert(TestCASSBilling.Lines[0].IsGSTApplicable);
			AssertEquals(1, TestCASSBilling.Lines.Count);
			AssertEquals(0m, TestCASSBilling.Lines[0].CASSCostTaxValue);
			AssertEquals(0m, TestCASSBilling.Lines[0].CASSCostTaxAdjustedValue);

			var consolCost = TestObjectCreator.CreateConsolCost(Consol, TestObjectCreator.FRT, TestObjectCreator.AALSHI, 1200m, true, AllocationMethod.Shipment);
			consolCost.E6_AT_TaxRate = TestObjectCreator.FREEVAT.PK;
			consolCost.E6_A9_VATClass = TestObjectCreator.TaxMsg3.PK;

			Factory.Save();

			TestCASSBilling.ForceRecalculateData();

			if (isCaseForZeroSystemCostAccrualValue)
			{
				AssertEquals(0m, TestCASSBilling.Lines[0].SystemCostAccrualValue);
			}
			else
			{
				AssertNotEquals(0m, TestCASSBilling.Lines[0].SystemCostAccrualValue);
			}

			TestCASSBilling.CreateInvoices();
			var testCassAPTransactions = TestCASSBilling.APTransactions;
			AssertEquals(1, testCassAPTransactions.Count);
			var testAPTransaction = testCassAPTransactions[0];
			Assert(testAPTransaction.Factory.HasContext(BusinessContext.CASS));

			var completeInvoice = TestCASSBilling.GenerateCompleteInvoice(testAPTransaction);
			AssertNotNull(completeInvoice);

			foreach (APInvoiceLine invLine in completeInvoice.Lines)
			{
				AssertEquals(defaultTaxId.ZeroRatedTaxID, invLine.AL_AT);
				AssertEquals(expectedTaxMsgPK, invLine.AL_A9_VATClass);
			}

			CASSFileImportDefaultTaxID CreateCassDefaultTaxId()
			{
				TestObjectCreator.FREEVAT.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg1.PK;
				TestObjectCreator.GST1.AT_A9_DefaultVatClass = TestObjectCreator.TaxMsg2.PK;

				return new CASSFileImportDefaultTaxID()
				{
					ZeroRatedTaxID = TestObjectCreator.FREEVAT.PK,
					StandardRatedTaxID = TestObjectCreator.GST1.PK,
				};
			}
		}

		#region Implementation

		CASSChargeCodeCollection GetCASSChargeCodeRegistry(params AccChargeCode[] chargeCodes)
		{
			CASSChargeCodeCollection cASSMaps = null;
			if (chargeCodes != null)
			{
				cASSMaps = new CASSChargeCodeCollection(new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty), Factory);
				foreach (AccChargeCode chargeCode in chargeCodes)
				{
					AddCASSChargeCodeLine(cASSMaps, "ALL", "ALL", chargeCode.PK);
				}
			}
			return cASSMaps;
		}

		void AddExportCASSBillingLine(ZDecimal pWCAmount, ZDecimal pVCAmount, ZDecimal pCCAmount, ZDecimal cOAAmount, ZDecimal cOMAmount, ZDecimal dOIAmount, ZDecimal vatDueAirlineAmount, ZDecimal adjustedVatDueAirlineAmount, string currency = "AUD", string vATIndicator = "Y", decimal vatDueAgentAmount = 0M, decimal adjustedVatDueAgentAmount = 0M)
		{
			var cassBillingLine = TestCASSBilling.Lines.AddNew();
			TestObjectCreator.SetupCASSCostComponent(cassBillingLine, false, pWCAmount, pVCAmount, pCCAmount, cOAAmount, cOMAmount, dOIAmount, vatDueAirlineAmount: vatDueAirlineAmount, currency: currency, vatIndicator: vATIndicator, vatDueAgentAmount: vatDueAgentAmount);
			TestObjectCreator.SetupCASSCostComponent(cassBillingLine, true, pWCAmount * 0.05M, pVCAmount * 0.05M, pCCAmount * 0.05M, cOAAmount * 0.05M, cOMAmount * 0.05M, dOIAmount * 0.05M, adjustedVatDueAirlineAmount: adjustedVatDueAirlineAmount, currency: currency, vatIndicator: vATIndicator, adjustedVatDueAgentAmount: adjustedVatDueAgentAmount);
		}

		public static void AddCASSChargeCodeLine(CASSChargeCodeCollection cassChargeCodeCollection, ZString cASSType, ZString component, ZGuid chargeCodePK)
		{
			var cassChargeCode = cassChargeCodeCollection.AddNew();
			cassChargeCode.CurrentFallbackLevel = new FallbackLevel(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty);
			cassChargeCode.CASSType = cASSType;
			cassChargeCode.ChargeCodePK = chargeCodePK;
			cassChargeCode.CASSComponentCode = component;
		}

		string RunAllocationOfTransactionNumberScenario(bool createCreditNote)
		{
			AccountingConfigurationRegistry.Instance.CASSFileImportDefaultTaxID.Value.ZeroRatedTaxID = TestObjectCreator.GSTFREE1.PK;
			GlbCompany company = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, "SIN"));
			company.GC_IsGSTRegistered = true;

			SetupAssociatedBizos(false);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var testCassBilling = new CASSBilling(factory2);
			SetupCASSBilling(testCassBilling, 0);
			var billingLine = testCassBilling.Lines.AddNew();
			TestObjectCreator.SetupCASSBillingLine(billingLine, false, false);

			if (createCreditNote)
			{
				(billingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = -68000M;
				(billingLine.AggregatedCostLine as CASSCostExportLine).VATDueAirline = -6800M;
			}
			else
			{
				(billingLine.AggregatedCostLine as CASSCostExportLine).WeightChargePP = 68000M;
				(billingLine.AggregatedCostLine as CASSCostExportLine).VATDueAirline = 6800M;
			}

			testCassBilling.CreateInvoices();
			AccountingConfigurationRegistry.Instance.CASSImportAutoCreateClaims.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CASSAutoCreateClaim.NotCreated);
			testCassBilling.PostAPTransactions();

			AssertEquals("One transaction header created", 1, testCassBilling.APTransactions.Count);

			if (createCreditNote)
			{
				AssertEquals("Sanity test - Transaction should be a AP Credit Note", typeof(APCreditNote), testCassBilling.APTransactions[0].GetType());
			}
			else
			{
				AssertEquals("Sanity test - Transaction should be an AP Invoice", typeof(APInvoice), testCassBilling.APTransactions[0].GetType());
			}

			var cassInvoice = testCassBilling.APTransactions[0];

			return cassInvoice.InvoiceNumber;
		}

		void SetupCASSBilling(CASSBilling cassBilling, int numberOfLines = 2, int multiplier = 1, bool setupAmount = true, bool setAdjustmentValues = true, string currencyCode = "AUD")
		{
			var today = ZDateTime.Today;
			var oneMonthAgo = today.AddMonths(-1);

			var cassCostHeader = cassBilling.CostHeader;
			cassCostHeader.HOTFileName = "TestCASS.hot";
			cassCostHeader.DatePeriodStart = new ZDateTime(oneMonthAgo.Year, oneMonthAgo.Month, 1);
			cassCostHeader.DatePeriodEnd = new ZDateTime(today.Year, today.Month, 1).AddDays(-1);
			cassCostHeader.DateOfBilling = today;
			cassCostHeader.InitializeAsExportCASS();

			for (int i = 0; i < numberOfLines; i++)
			{
				var billingLine = cassBilling.Lines.AddNew();
				TestObjectCreator.SetupCASSBillingLine(billingLine, multiplier: multiplier, setAdjustmentValues: setAdjustmentValues, setupAmount: setupAmount, currencyCode: currencyCode);
			}
		}

		void SetupAssociatedBizos(bool withCharges = true, string currency = "AUD", bool setConsigneeDocAddressOrg = true)
		{
			var cassLine = new CASSBillingLine(Factory);
			TestObjectCreator.SetupCASSBillingLine(cassLine, setAdjustmentValues: false, currencyCode: currency);

			var jobChargeInfo = new List<(AccChargeCode, ZDecimal, ZDecimal)>();
			if (withCharges)
			{
				if (TestObjectCreator.FRT == null)
				{
					var fRTExisting = Factory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
					var factory2 = new BusinessObjectFactory();
					var fRT = factory2.New<AccChargeCode>();
					fRT.CopyPersistentValuesFrom(fRTExisting);
					fRT.AC_GC = GlbCompany.CurrentCompany.PK;
					factory2.Save();
					Env.Registry.FreightChargeCode = fRT.PK.ToGuid();
				}
				jobChargeInfo.Add((TestObjectCreator.FRT, 100M, 100M));
			}

			SetupAssociatedBizos(cassLine, jobChargeInfo, setConsigneeDocAddressOrg: setConsigneeDocAddressOrg);
		}

		void SetupAssociatedBizos(CASSBillingLine cassLine, List<(AccChargeCode chargeCode, ZDecimal costAmount, ZDecimal sellAmount)> jobChargeInfo, bool multipleShipment = true, bool setConsigneeDocAddressOrg = true)
		{
			string origin = RefUNLOCO.LoadFromIATA(Factory, cassLine.LoadPortIATA).RL_Code;
			string destination = RefUNLOCO.LoadFromIATA(Factory, cassLine.DischargePortIATA).RL_Code;

			var consolWithJobs = SetupConsolAndJobs("C0001", origin, destination, cassLine.MAWBNumber, cassLine.CASSCostCurrency, (x) => jobChargeInfo, multipleShipment ? new[] { "SHIP1", "SHIP2" } : new[] { "SHIP1" });

			Consol = consolWithJobs.consol;
			Shipment1 = consolWithJobs.shipmentsWihAssociatedJob[0].shipment;
			if (setConsigneeDocAddressOrg)
			{
				Shipment1.ConsigneeDocumentaryAddress.OrganisationPK = TestObjectCreator.AALSHI.PK;
			}
			Job1 = consolWithJobs.shipmentsWihAssociatedJob[0].invoicingJob;
			JobCharge1 = Job1.Charges[0];

			if (multipleShipment)
			{
				Shipment2 = consolWithJobs.shipmentsWihAssociatedJob[1].shipment;
				Job2 = consolWithJobs.shipmentsWihAssociatedJob[1].invoicingJob;
				JobCharge2 = Job2.Charges[0];
			}

			TestObjectCreator.SetExchangeRate(Job1, cassLine.CASSCostCurrency, 2M);

			TestObjectCreator.CreateTestPeriods(ZDateTime.Today);

			TestObjectCreator.GLHeader1.AG_AccountType = Constants.AccountType.BalanceSheetAccount;
			TestObjectCreator.GLHeader1.AG_Description = "Test Account";
			AccountingConfigurationRegistry.Instance.CASSGLAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, TestObjectCreator.GLHeader1.PK.ToGuid());
			TestObjectCreator.SetupCreditorAirline(TestObjectCreator.CreateAirLine(cassLine.AirlinePrefix), TestObjectCreator.AALSHI);
		}

		(ForwardingConsol consol, (ForwardingShipment shipment, Job invoicingJob)[] shipmentsWihAssociatedJob) SetupConsolAndJobs(ZString consolNo, ZString origin, ZString destination, ZString mawbNumber, RefCurrency costCurrency, Func<ForwardingShipment, List<(AccChargeCode chargeCode, ZDecimal costAmount, ZDecimal sellAmount)>> jobChargeInfoBuilder, params string[] shipmentNumber)
		{
			var shipmentsWihJobs = new List<(ForwardingShipment shipment, Job invoicingJob)>();

			var consol = TestObjectCreator.CreateConsol(origin, destination, consolNo);
			consol.JK_MasterBillNum = mawbNumber;
			foreach (var number in shipmentNumber)
			{
				var shipment = TestObjectCreator.CreateShipment(number, origin, destination, consol);
				var job = PrepareJob(shipment);
				shipmentsWihJobs.Add((shipment, job));
			}

			return (consol, shipmentsWihJobs.ToArray());

			Job PrepareJob(ForwardingShipment shipment)
			{
				var job = TestObjectCreator.CreateJob(shipment, false);

				var dept = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")).PK;
				job.JH_GE = dept;

				if (jobChargeInfoBuilder != null)
				{
					var jobChargeInfo = jobChargeInfoBuilder(shipment);
					if (jobChargeInfo != null)
					{
						foreach (var (chargeCode, costAmount, sellAmount) in jobChargeInfo)
						{
							var charge = TestObjectCreator.CreateCharge(job, chargeCode, chargeCode.AC_Desc, costCurrency, costAmount, null, costCurrency, sellAmount, null);
							charge.JR_GE = GlbDepartment.CurrentDepartment.PK;
							if (charge.Department.GE_Misc)
							{
								charge.Department.GE_Misc = false;
								charge.JR_GE = charge.JR_GE;
							}
						}
					}
				}

				return job;
			}
		}

		void SetupGatewayOnConsol(string agentStatus = AgentStatusList.Codes.GatewayAgentWithTariff)
		{
			Consol.JK_AgentType = Constants.AgentType.Agent;
			Consol.JK_RL_NKLoadPort = "AUBNE";
			Consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			Consol.JK_SendingForwarderHandlingType = agentStatus;

			var port = Consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			port.O5_SeaAirCarrierOrForwarderType = "GTW";
			port.O5_PortOrCountry = Consol.JK_RL_NKLoadPort;
			port.O5_OA_AgentOfficeAddress = Consol.JK_OA_SendingForwarderAddress;
			port.O5_AgentDirection = AgentDirectionList.Codes.Both;
			port.O5_AirAgentStatus = agentStatus;
			Consol.SendingForwarder.AppointedGatewayAgentPorts.Add(port);

			GatewayBillingJob = TestObjectCreator.CreateJob(Consol, false);
			GatewayBillingJob.JH_GE = GatewayDepartmentPK;

			var jobChargeToBecomeConsolGost = GatewayBillingJob.Charges.AddNew();
			jobChargeToBecomeConsolGost.JR_AC = TestObjectCreator.FRT.PK;
			jobChargeToBecomeConsolGost.JR_OH_SellAccount = Consol.SendingForwarderPK;
			jobChargeToBecomeConsolGost.JR_RX_NKSellCurrency = "AUD";
			jobChargeToBecomeConsolGost.JR_OSSellAmt = 500m;
			jobChargeToBecomeConsolGost.JR_JH_InternalJob = GatewayBillingJob.PK;
			jobChargeToBecomeConsolGost.JR_GB_InternalBranch = jobChargeToBecomeConsolGost.JR_GB;
			jobChargeToBecomeConsolGost.JR_GE_InternalDept = jobChargeToBecomeConsolGost.JR_GE;
		}

		ZGuid GatewayDepartmentPK
		{
			get
			{
				if (!gatewayDepartmentPK.IsValid)
				{
					gatewayDepartmentPK = Factory.LoadFromNaturalKey<GlbDepartment>(GlbDepartmentSchema.GE_Code, "GIS").PK;
				}

				return gatewayDepartmentPK;
			}
		}

		ZGuid gatewayDepartmentPK;

		void IncludeCurrentDepartmentForChargeCodeFRT()
		{
			if (TestObjectCreator.FRT != null)
			{
				TestObjectCreator.FRT.AC_DepartmentFilterList += (", " + GlbDepartment.CurrentDepartment.GE_Code);
			}
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;

		CASSBilling TestCASSBilling
		{
			get { return fTestCASSBilling ?? (fTestCASSBilling = new CASSBilling(Factory)); }
		}
		CASSBilling fTestCASSBilling;

		ForwardingConsol Consol;
		ForwardingShipment Shipment1;
		ForwardingShipment Shipment2;
		Job Job1;
		Job Job2;
		Job GatewayBillingJob;
		JobCharge JobCharge1;
		JobCharge JobCharge2;

		DummyTracer DummyTracer;

		protected override void SetUp()
		{
			DummyTracer = new DummyTracer();
			ObjectFactory.Substitute<ITracer>(DummyTracer);
		}

		#endregion
	}

	public class TotalCostCorrectorTest : TestCaseWithFactory
	{
		public void TestBasicScenario()
		{
			RunTestCase(
				new[] { new ZDecimal[] { 1000M, 100M }, new ZDecimal[] { 2000M, 200M }, new ZDecimal[] { 1000M, 100M } },
				new[] { new ZDecimal[] { 1000M, 100M }, new ZDecimal[] { 2000M, 200M }, new ZDecimal[] { 1000M, 100M } },
				4000M, 400M);

			RunTestCase(
				new[] { new ZDecimal[] { 0.14M, 0.01M }, new ZDecimal[] { 0.28M, 0.03M }, new ZDecimal[] { 0.57M, 0.05M } },
				new[] { new ZDecimal[] { 0.14M, 0.01M }, new ZDecimal[] { 0.28M, 0.03M }, new ZDecimal[] { 0.58M, 0.06M } },
				1.00M, 0.10M);
		}

		public void Test3EqualValues()
		{
			RunTestCase(
				new[] { new ZDecimal[] { 1M, 0.1M }, new ZDecimal[] { 1M, 0.1M }, new ZDecimal[] { 1M, 0.1M } },
				new[] { new ZDecimal[] { 1M, 0.1M }, new ZDecimal[] { 1M, 0.1M }, new ZDecimal[] { 1M, 0.1M } },
				3.00M, 0.30M);
			RunTestCase(
				new[] { new ZDecimal[] { -1M, -0.1M }, new ZDecimal[] { -1M, -0.1M }, new ZDecimal[] { -1M, -0.1M } },
				new[] { new ZDecimal[] { -1M, -0.1M }, new ZDecimal[] { -1M, -0.1M }, new ZDecimal[] { -1M, -0.1M } },
				-3.00M, -0.30M);
			RunTestCase(
				new[] { new ZDecimal[] { 0.33M, 0.03M }, new ZDecimal[] { 0.33M, 0.03M }, new ZDecimal[] { 0.33M, 0.03M } },
				new[] { new ZDecimal[] { 0.34M, 0.04M }, new ZDecimal[] { 0.33M, 0.03M }, new ZDecimal[] { 0.33M, 0.03M } },
				1.00M, 0.10M);
		}

		public void TestZeroedValuesGetRemoved()
		{
			RunTestCase(
				new[] { new ZDecimal[] { 5M, 0M }, new ZDecimal[] { 0M, 0M }, new ZDecimal[] { 0M, 0M } },
				new[] { new ZDecimal[] { 5M, 0M } },
				5M, 0M);

			RunTestCase(
				new[] { new ZDecimal[] { 0.01M, 0M }, new ZDecimal[] { 0.01M, 0M }, new ZDecimal[] { 0.01M, 0M } },
				new[] { new ZDecimal[] { 0.01M, 0M }, new ZDecimal[] { 0.01M, 0M } },
				0.02M, 0M);

			RunTestCase(
				new[] { new ZDecimal[] { -0.01M, 0M }, new ZDecimal[] { -0.01M, 0M }, new ZDecimal[] { -0.01M, 0M } },
				new[] { new ZDecimal[] { -0.01M, 0M }, new ZDecimal[] { -0.01M, 0M } },
				-0.02M, 0M);

			RunTestCase(
				new[] { new ZDecimal[] { 0.01M, 0.01M }, new ZDecimal[] { 0.01M, 0.01M }, new ZDecimal[] { 0.01M, 0.01M } },
				new[] { new ZDecimal[] { 0.01M, 0.01M }, new ZDecimal[] { 0.01M, 0.01M } },
				0.02M, 0.02M);

			RunTestCase(
				new[] { new ZDecimal[] { 0.01M, 0M }, new ZDecimal[] { 0.01M, 0.01M }, new ZDecimal[] { 0.01M, 0.01M } },
				new[] { new ZDecimal[] { 0.01M, 0.01M }, new ZDecimal[] { 0.01M, 0.01M } },
				0.02M, 0.02M);
		}

		public void ZeroOffItemsThatAreZeroInLocalCurrency()
		{
			RunTestCase(
				new[] { new ZDecimal[] { 5M, 0M, 1M }, new ZDecimal[] { 3M, 0M, 0M } },
				new[] { new ZDecimal[] { 8M, 0M } },
				8M, 0M);

			RunTestCase(
				new[] { new ZDecimal[] { 5M, 0M, 1M }, new ZDecimal[] { 3M, 0M, 1M } },
				new[] { new ZDecimal[] { 5M, 0M, 1M }, new ZDecimal[] { 3M, 0M, 1M } },
				8M, 0M);
		}

		#region Implementation

		int RunTestCaseNumber;

		protected override void SetUp()
		{
			base.SetUp();
			RunTestCaseNumber = 0;
		}

		public void RunTestCase(
			ZDecimal[][] currentCostValues,
			ZDecimal[][] expectedCostValues,
			ZDecimal originalTotalCostAmount,
			ZDecimal originalTotalTaxAmount
			)
		{
			var factory = new BusinessObjectFactory();
			var testObjectCreator = new TestObjectCreator(factory);
			var consol = testObjectCreator.CreateConsol("AUSYD", "AUMEL", (RunTestCaseNumber++).ToString());
			testObjectCreator.CreateShipment(RunTestCaseNumber.ToString(), consol);
			JobConsolCostCollection collection = new JobConsolCostCollection(factory, consol);

			var corrector = new CASSBilling.TotalCostCorrector<JobConsolCost>(
					cst => (ZPropertyInfo<ZDecimal>)cst.E6_OSCostAmountInfo,
					cst => (ZPropertyInfo<ZDecimal>)cst.E6_OSGSTAmount_CalcInfo,
					cst => (ZPropertyInfo<ZDecimal>)cst.E6_LocalCostAmountInfo,
					cst =>
					{
						collection.Remove(cst);
					});

			for (int i = 0; i < currentCostValues.Length; i++)
			{
				var cost = collection.TryAddNew();
				cost.E6_IsTaxAmountOverridden = true;
				AssertNotNull("Consol Cost cannot be added", cost);
				cost.E6_OSCostAmount = currentCostValues[i][0];
				cost.E6_OSGSTAmount_Calc = currentCostValues[i][1];
				if (currentCostValues[i].Length >= 3)
				{
					cost.E6_LocalCostAmount = currentCostValues[i][2];
				}
				corrector.MarkForAdjustment(cost);
			}

			corrector.AdjustValuesToSumCorrectlyAfterApportionment(originalTotalCostAmount, originalTotalTaxAmount);

			for (int i = 0; i < expectedCostValues.Length; i++)
			{
				JobConsolCost cost = collection[i];
				AssertEquals(String.Format("Cost value {0} is correct", i), expectedCostValues[i][0], cost.E6_OSCostAmount);
				AssertEquals(String.Format("Tax value {0} is correct", i), expectedCostValues[i][1], cost.E6_OSGSTAmount_Calc);
				if (currentCostValues[i].Length >= 3)
				{
					AssertEquals(String.Format("Local value {0} is correct", i), expectedCostValues[i][2], cost.E6_LocalCostAmount);
				}
			}
		}

		#endregion
	}
}
