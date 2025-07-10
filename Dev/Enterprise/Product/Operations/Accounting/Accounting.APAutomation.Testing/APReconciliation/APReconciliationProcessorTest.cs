using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Moq;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class APReconciliationProcessorTest : TestCaseWithFactory
	{
		public void TestDependencyInjection()
		{
			AssertNotNull(APReconciliationProcessor);
			AssertType<APReconciliationProcessor>(APReconciliationProcessor);
		}

		public void TestReconcile_2NHC_1HC()
		{
			var rLine1 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid() };
			var rLine2 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid() };
			var mockNHCluster1 = new Mock<IAPReconciliationCluster>();
			mockNHCluster1.Setup(c => c.TryToReconcile(It.IsAny<IAccrualSummator>())).Returns(new APReconciliationProcessingResult() { Result = APReconciliationResultTypes.Success, ReconciliableAccruals = new[] { rLine1 } });

			var mockNHCluster2 = new Mock<IAPReconciliationCluster>();
			mockNHCluster2.Setup(c => c.TryToReconcile(It.IsAny<IAccrualSummator>())).Returns(new APReconciliationProcessingResult() { Result = APReconciliationResultTypes.MergeWithHeaderCluster });

			var mockHCluster = new Mock<ISupportMergingAPReconciliationCluster>();
			mockHCluster.SetupGet(c => c.IsHeaderCluster).Returns(true);
			mockHCluster.Setup(c => c.MergeWithAnotherCluster(It.IsAny<IAPReconciliationCluster>()));
			mockHCluster.Setup(c => c.TryToReconcile(It.IsAny<IAccrualSummator>())).Returns(new APReconciliationProcessingResult() { Result = APReconciliationResultTypes.Success, ReconciliableAccruals = new[] { rLine2 } });

			var mockClusterBuilder = new Mock<IAPReconciliationClusterBuilder>();
			mockClusterBuilder.Setup(cb => cb.Build(DummyValidDraftInvoice)).Returns(new[] { mockNHCluster1.Object, mockNHCluster2.Object, mockHCluster.Object });
			ObjectFactory.Substitute(mockClusterBuilder.Object);

			var reconDetails = APReconciliationProcessor.Reconcile(DummyValidDraftInvoice);

			mockHCluster.Verify(c => c.MergeWithAnotherCluster(It.IsAny<IAPReconciliationCluster>()), Times.Once);
			AssertEquals(APReconciliationResultTypes.Success, reconDetails.Result);
			AssertContainsExactElementsInAnyOrder(new[] { rLine1, rLine2 }, reconDetails.ReconciliableAccruals);
		}

		public void TestReconcile_CreditorInvoicedExchangeRate_WithClusterAmount()
		{
			PrepareTestData_CreditorInvoicedExchangeRate();
			jobCluster1.AIC_Amount = 60m;
			jobCluster2.AIC_Amount = 40m;

			var result = APReconciliationProcessor.Reconcile(draftInvoice);

			AssertEquals(APReconciliationResultTypes.Success, result.Result);
			AssertEquals(2, result.ReconciliableAccruals.Count());
			AssertReconciliation_CreditorInvoicedExchangeRate(result, charge3.PK, TestObjectCreator.GBP.Code, 10m, 60m);
			AssertReconciliation_CreditorInvoicedExchangeRate(result, consolCost1.PK, TestObjectCreator.USD.Code, 5m, 40m);
		}

		public void TestReconcile_CreditorInvoicedExchangeRate_ZeroClusterAmount_MultipleMatches()
		{
			PrepareTestData_CreditorInvoicedExchangeRate();
			AssertEquals("Precondition", 0m, jobCluster1.AIC_Amount);
			AssertEquals(0m, jobCluster2.AIC_Amount);

			var result = APReconciliationProcessor.Reconcile(draftInvoice);

			AssertEquals(APReconciliationResultTypes.Failed, result.Result);
			AssertEquals("[HEADER_CLUSTER]: Multiple matching accruals found", result.FailureReason);
		}

		public void TestReconcile_CreditorInvoicedExchangeRate_ZeroClusterAmount_MatchChargeAndConsolCost()
		{
			PrepareTestData_CreditorInvoicedExchangeRate();
			charge1.JR_OSCostAmt = 2000m;
			charge2.JR_OSCostAmt = 2000m;
			Factory.Save();
			AssertEquals("Precondition", 0m, jobCluster1.AIC_Amount);
			AssertEquals(0m, jobCluster2.AIC_Amount);

			var result = APReconciliationProcessor.Reconcile(draftInvoice);

			AssertEquals(APReconciliationResultTypes.Success, result.Result);
			AssertEquals(2, result.ReconciliableAccruals.Count());
			AssertReconciliation_CreditorInvoicedExchangeRate(result, charge3.PK, TestObjectCreator.GBP.Code, 10m, 60m);
			AssertReconciliation_CreditorInvoicedExchangeRate(result, consolCost1.PK, TestObjectCreator.USD.Code, 5m, 40m);
		}

		public void TestReconcile_2NHC()
		{
			var rLine1 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid() };
			var mockNHCluster1 = new Mock<IAPReconciliationCluster>();
			mockNHCluster1.Setup(c => c.TryToReconcile(It.IsAny<IAccrualSummator>())).Returns(new APReconciliationProcessingResult() { Result = APReconciliationResultTypes.Success, ReconciliableAccruals = new[] { rLine1 } });

			var mockNHCluster2 = new Mock<IAPReconciliationCluster>();
			mockNHCluster2.Setup(c => c.TryToReconcile(It.IsAny<IAccrualSummator>())).Returns(new APReconciliationProcessingResult() { Result = APReconciliationResultTypes.MergeWithHeaderCluster });

			var mockClusterBuilder = new Mock<IAPReconciliationClusterBuilder>();
			mockClusterBuilder.Setup(cb => cb.Build(DummyValidDraftInvoice)).Returns(new[] { mockNHCluster1.Object, mockNHCluster2.Object });
			ObjectFactory.Substitute(mockClusterBuilder.Object);

			var reconDetails = APReconciliationProcessor.Reconcile(DummyValidDraftInvoice);

			AssertEquals(APReconciliationResultTypes.Failed, reconDetails.Result);
		}

		public void TestReconcile_HC_ACombinationFoundInTheHC()
		{
			var rLine1 = new APReconciliationLine() { LineIdentifier = ZGuid.NewZGuid() };

			var mockHCluster = new Mock<ISupportMergingAPReconciliationCluster>();
			mockHCluster.SetupGet(c => c.IsHeaderCluster).Returns(true);
			mockHCluster.Setup(c => c.MergeWithAnotherCluster(It.IsAny<IAPReconciliationCluster>()));
			mockHCluster.Setup(c => c.TryToReconcile(It.IsAny<IAccrualSummator>())).Returns(new APReconciliationProcessingResult() { Result = APReconciliationResultTypes.Success, ReconciliableAccruals = new[] { rLine1 } });

			var mockClusterBuilder = new Mock<IAPReconciliationClusterBuilder>();
			mockClusterBuilder.Setup(cb => cb.Build(DummyValidDraftInvoice)).Returns(new[] { mockHCluster.Object });
			ObjectFactory.Substitute(mockClusterBuilder.Object);

			var reconDetails = APReconciliationProcessor.Reconcile(DummyValidDraftInvoice);

			AssertEquals(APReconciliationResultTypes.Success, reconDetails.Result);
			AssertContainsExactElementsInAnyOrder(new[] { rLine1 }, reconDetails.ReconciliableAccruals);
		}

		public void TestReconcile_HC_NoCombinationFoundInTheHC()
		{
			var mockHCluster = new Mock<ISupportMergingAPReconciliationCluster>();
			mockHCluster.SetupGet(c => c.IsHeaderCluster).Returns(true);
			mockHCluster.Setup(c => c.MergeWithAnotherCluster(It.IsAny<IAPReconciliationCluster>()));
			mockHCluster.Setup(c => c.TryToReconcile(It.IsAny<IAccrualSummator>())).Returns(new APReconciliationProcessingResult() { Result = APReconciliationResultTypes.Failed });

			var mockClusterBuilder = new Mock<IAPReconciliationClusterBuilder>();
			mockClusterBuilder.Setup(cb => cb.Build(DummyValidDraftInvoice)).Returns(new[] { mockHCluster.Object });
			ObjectFactory.Substitute(mockClusterBuilder.Object);

			var reconDetails = APReconciliationProcessor.Reconcile(DummyValidDraftInvoice);

			AssertEquals(APReconciliationResultTypes.Failed, reconDetails.Result);
		}

		public void TestReconcile_CreditNote_NegativeBehaviourDisabled()
		{
			var draftInvoice = DummyValidDraftInvoice;
			draftInvoice.AIH_GC_Company = GlbCompany.CurrentCompany.PK;
			draftInvoice.AIH_GB_Branch = GlbBranch.CurrentBranch.PK;
			draftInvoice.AIH_TransactionType = TransactionTypes.CreditNote;
			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetTemporaryValue(draftInvoice.AIH_GC_Company.ToGuid(), Guid.Empty, Guid.Empty, false);

			var reconDetails = APReconciliationProcessor.Reconcile(draftInvoice);
			AssertEquals(APReconciliationResultTypes.Failed, reconDetails.Result);
			AssertEquals(AccDraftInvoiceProcessingErrors.Keys.DisabledNegativeAccrualBehaviour, reconDetails.FailureReasonCode);
			AssertEquals(ExpectedReconciliationAbortedDueToDisabledNegativeAccrualBehaviourErrorMessage(draftInvoice), reconDetails.FailureReason);
			AssertEquals(AccDraftInvoiceProcessingErrors.Context.AutoAPReconciliation, reconDetails.FailureReasonAsLogableError.ErrorContext);
			AssertNull(reconDetails.ReconciliableAccruals);

			string ExpectedReconciliationAbortedDueToDisabledNegativeAccrualBehaviourErrorMessage (AccDraftInvoiceHeader draftTransaction) => FormattableString.Invariant($"Reconciliation for {draftTransaction.AIH_TransactionNumber} is aborted as {AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.HumanReadableRegistryPath()} registry is disabled");
		}

		public void TestReconcile_CreditNote_NegativeBehaviourEnabled()
		{
			var objectCreator = new TestObjectCreator(Factory);

			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			//Build consols
			var forwardingConsol = objectCreator.CreateConsol(consolNum: "C0001");
			var shipment = objectCreator.CreateShipment("S0001", forwardingConsol);
			var cost1 = objectCreator.CreateConsolCost(forwardingConsol, objectCreator.FRT, objectCreator.AUD, 1M, -100M, objectCreator.Creditor1);
			var cost2 = objectCreator.CreateConsolCost(forwardingConsol, objectCreator.CC1, objectCreator.AUD, 1M, -200M, objectCreator.Creditor1);

			//Build Shipments
			var shipment1 = objectCreator.CreateShipment("S10001");
			var job = objectCreator.CreateJob(shipment1);
			var charge1 = objectCreator.CreateCharge(job, objectCreator.CC1, "charge 01", costCurrency: objectCreator.AUD, osCostAmt: -300M, creditor: objectCreator.Creditor1);
			var charge2 = objectCreator.CreateCharge(job, objectCreator.CC2, "charge 02", costCurrency: objectCreator.AUD, osCostAmt: -1000M, creditor: objectCreator.Creditor1);
			var charge3 = objectCreator.CreateCharge(job, objectCreator.CC2, "charge 03", costCurrency: objectCreator.AUD, osCostAmt: 400M, creditor: objectCreator.Creditor1);
			Factory.Save();

			//Build clusters with a mix of consol and jobs
			var draftInvoice = objectCreator.CreateDraftCreditNote("ADI 0001", "ADI-IREF-001", objectCreator.Creditor1.PK, 1200, 0, "AUD");
			var cluster1 = objectCreator.AddClusterToDraftTransaction(draftInvoice, 900);
			_ = objectCreator.AddJobToTheCluster(cluster1, shipment1);

			var cluster2 = objectCreator.AddClusterToDraftTransaction(draftInvoice, 300);
			_ = objectCreator.AddConsolToTheCluster(cluster2, forwardingConsol);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedDraftInvoice = newFactory.Load<AccDraftInvoiceHeader>(draftInvoice.PK);

			//Try to reconcile
			var reconProcessor = ObjectFactory.Get<IAPReconciliationProcessor>();
			var reconResult = reconProcessor.Reconcile(reloadedDraftInvoice);
			AssertEquals(APReconciliationResultTypes.Success, reconResult.Result);

			var sortedLines = reconResult.ReconciliableAccruals.OrderByDescending(rl => rl.LocalExTaxAmount).ToArray();
			AssertReconciliationJobChargeLine(sortedLines[0], charge3);
			AssertReconciliationConsolCostLine(sortedLines[1], cost1);
			AssertReconciliationConsolCostLine(sortedLines[2], cost2);
			AssertReconciliationJobChargeLine(sortedLines[3], charge1);
			AssertReconciliationJobChargeLine(sortedLines[4], charge2);
		}

		public void TestReconcile_Invoice_NegativeBehaviourEnabled()
		{
			var objectCreator = new TestObjectCreator(Factory);

			AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, true);

			//Build consols
			var forwardingConsol = objectCreator.CreateConsol(consolNum: "C0001");
			var shipment = objectCreator.CreateShipment("S0001", forwardingConsol);
			var cost1 = objectCreator.CreateConsolCost(forwardingConsol, objectCreator.FRT, objectCreator.AUD, 1M, -100M, objectCreator.Creditor1);
			var cost2 = objectCreator.CreateConsolCost(forwardingConsol, objectCreator.CC1, objectCreator.AUD, 1M, 200M, objectCreator.Creditor1);

			//Build Shipments
			var shipment1 = objectCreator.CreateShipment("S10001");
			var job = objectCreator.CreateJob(shipment1);
			var charge1 = objectCreator.CreateCharge(job, objectCreator.CC1, "charge 01", costCurrency: objectCreator.AUD, osCostAmt: -300M, creditor: objectCreator.Creditor1);
			var charge2 = objectCreator.CreateCharge(job, objectCreator.CC2, "charge 02", costCurrency: objectCreator.AUD, osCostAmt: 500M, creditor: objectCreator.Creditor1);

			Factory.Save();

			//Build clusters with a mix of consol and jobs
			var draftInvoice = objectCreator.CreateDraftInvoice("ADI 0001", "ADI-IREF-001", objectCreator.Creditor1.PK, 300, 0, "AUD");
			var cluster1 = objectCreator.AddClusterToDraftTransaction(draftInvoice, 200);
			_ = objectCreator.AddJobToTheCluster(cluster1, shipment1);

			var cluster2 = objectCreator.AddClusterToDraftTransaction(draftInvoice, 100);
			_ = objectCreator.AddConsolToTheCluster(cluster2, forwardingConsol);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var reloadedDraftInvoice = newFactory.Load<AccDraftInvoiceHeader>(draftInvoice.PK);

			//Try to reconcile
			var reconProcessor = ObjectFactory.Get<IAPReconciliationProcessor>();
			var reconResult = reconProcessor.Reconcile(reloadedDraftInvoice);
			AssertEquals(APReconciliationResultTypes.Success, reconResult.Result);

			var sortedLines = reconResult.ReconciliableAccruals.OrderByDescending(rl => rl.LocalExTaxAmount).ToArray();
			AssertReconciliationJobChargeLine(sortedLines[0], charge2);
			AssertReconciliationConsolCostLine(sortedLines[1], cost2);
			AssertReconciliationConsolCostLine(sortedLines[2], cost1);
			AssertReconciliationJobChargeLine(sortedLines[3], charge1);
		}

		public void TestValidate()
		{
			var draftInvoice = Factory.New<AccDraftInvoiceHeader>();

			CombineAssertions("PreConditions", () => {
				Assert("Empty Creditor", draftInvoice.AIH_OH_Creditor.IsEmpty);
				AssertEquals("Empty Expected Amount", 0M, draftInvoice.AIH_ExpectedOSTotalAmount);
				AssertEquals("Empty Job", 0, draftInvoice.JobClusters.Count);
			});

			var result = APReconciliationProcessor.Reconcile(draftInvoice);

			AssertEquals("Should get failed reconciliation result when having validation error."
				, APReconciliationResultTypes.Failed
				, result.Result);
			AssertEquals("Validation error message"
				, @"Creditor not provided
Transaction Amount not provided
No jobs found"
				, result.FailureReason);
		}

		#region Creditor Invoiced Exchange Rate

		void PrepareTestData_CreditorInvoicedExchangeRate()
		{
			var shipment1 = TestObjectCreator.CreateShipment("S00001001");
			var shipment2 = TestObjectCreator.CreateShipment("S00001002");
			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);

			var consol = TestObjectCreator.CreateConsol(consolNum: "C00001001");
			var shipment3 = TestObjectCreator.CreateShipment("S00001003", consol);
			var shipment4 = testObjectCreator.CreateShipment("S00001004", consol);
			TestObjectCreator.CreateJob(shipment3);
			TestObjectCreator.CreateJob(shipment4);

			Factory.Save();

			charge1 = testObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.USD, 100m, TestObjectCreator.Creditor1);
			charge1.JR_OSCostExRate = 2m;
			charge2 = testObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, "Charge 2", TestObjectCreator.GBP, 100m, TestObjectCreator.Creditor1);
			charge2.JR_OSCostExRate = 3m;
			charge3 = testObjectCreator.CreateCharge(job2, TestObjectCreator.CC2, "Charge 3", TestObjectCreator.GBP, 600m, TestObjectCreator.Creditor1);
			charge3.JR_OSCostExRate = 3m;

			consolCost1 = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC3, TestObjectCreator.USD, 2m, 200m, TestObjectCreator.Creditor1);
			TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC3, TestObjectCreator.GBP, 3m, 200m, TestObjectCreator.Creditor1);

			Factory.Save();

			draftInvoice = TestObjectCreator.CreateDraftInvoice("ADI 0001", "ADIR 0001", TestObjectCreator.Creditor1.PK, 100m, 0m, TestObjectCreator.AUD.Code);

			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.USD, 5m);
			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.GBP, 10m);

			jobCluster1 = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice, 0m);
			TestObjectCreator.AddJobToTheCluster(jobCluster1, shipment1);
			TestObjectCreator.AddJobToTheCluster(jobCluster1, shipment2);

			jobCluster2 = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice, 0m);
			TestObjectCreator.AddJobToTheCluster(jobCluster2, consol);

			Factory.Save();
		}

		void AssertReconciliation_CreditorInvoicedExchangeRate(APReconciliationProcessingResult result, ZGuid accrualParentPK, string osCurrencyCode, decimal exchangeRate, decimal localExTaxAmount)
		{
			AssertNotNull(result.ReconciliableAccruals.Single(x => x.LineIdentifier == accrualParentPK && x.OSCurrency == osCurrencyCode && x.ExchangeRate == exchangeRate && x.LocalExTaxAmount == localExTaxAmount));
		}

		AccDraftInvoiceHeader draftInvoice;
		JobCharge charge1, charge2, charge3;
		JobConsolCost consolCost1;
		AccDraftInvoiceJobCluster jobCluster1, jobCluster2;

		#endregion

		void AssertReconciliationConsolCostLine(APReconciliationLine line, JobConsolCost cost)
		{
			AssertEquals(nameof(line.LineIdentifier), cost.PK, line.LineIdentifier);
			AssertEquals(nameof(line.Description), FormattableString.Invariant($"{cost.Consol?.JK_UniqueConsignRef}-{cost.ChargeCode.AC_Code}-{cost.E6_Description}"), line.Description);
			AssertEquals(nameof(line.LocalCurrency), cost.Company.LocalCurrency.RX_Code, line.LocalCurrency);
			AssertEquals(nameof(line.OSCurrency), cost.Currency.RX_Code, line.OSCurrency);
			AssertEquals(nameof(line.LineType), APReconciliationLineTypes.ConsolCost, line.LineType);
			AssertEquals(nameof(line.ExchangeRate), cost.E6_ExchangeRate, line.ExchangeRate);
			AssertEquals(nameof(line.CreditorPk), cost.E6_OH_Creditor, line.CreditorPk);
			AssertEquals(nameof(line.LocalExTaxAmount), cost.E6_LocalCostAmount, line.LocalExTaxAmount);
			AssertEquals(nameof(line.OSExTaxAmount), cost.E6_OSCostAmount, line.OSExTaxAmount);
		}

		void AssertReconciliationJobChargeLine(APReconciliationLine line, JobCharge charge)
		{
			AssertEquals(nameof(line.LineIdentifier), charge.PK, line.LineIdentifier);
			AssertEquals(nameof(line.Description), FormattableString.Invariant($"{charge.Job?.JH_JobNum}-{charge.ChargeCode.AC_Code}-{charge.JR_Desc}"), line.Description);
			AssertEquals(nameof(line.LocalCurrency), charge.Company.LocalCurrency.Code, line.LocalCurrency);
			AssertEquals(nameof(line.OSCurrency), charge.JR_RX_NKCostCurrency, line.OSCurrency);
			AssertEquals(nameof(line.LineType), APReconciliationLineTypes.JobCharge, line.LineType);
			AssertEquals(nameof(line.ExchangeRate), charge.JR_OSCostExRate, line.ExchangeRate);
			AssertEquals(nameof(line.CreditorPk), charge.JR_OH_CostAccount, line.CreditorPk);
			AssertEquals(nameof(line.LocalExTaxAmount), charge.JR_LocalCostAmt, line.LocalExTaxAmount);
			AssertEquals(nameof(line.OSExTaxAmount), charge.JR_OSCostAmt, line.OSExTaxAmount);
		}

		AccDraftInvoiceHeader DummyValidDraftInvoice => dummyValidDraftInvoice ??= CreateValidDraftInvoice();
		AccDraftInvoiceHeader dummyValidDraftInvoice;

		AccDraftInvoiceHeader CreateValidDraftInvoice()
		{
			var draftInvoice = Factory.New<AccDraftInvoiceHeader>();
			draftInvoice.AIH_OH_Creditor = TestObjectCreator.Creditor1.PK;
			draftInvoice.AIH_ExpectedOSTotalAmount = 1M;
			draftInvoice.JobClusters.Add(Factory.New<AccDraftInvoiceJobCluster>());

			return draftInvoice;
		}

		IAPReconciliationProcessor APReconciliationProcessor => aPReconciliationProcessor ??= ObjectFactory.Get<IAPReconciliationProcessor>();
		IAPReconciliationProcessor aPReconciliationProcessor;

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
