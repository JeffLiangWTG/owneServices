using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class JobChargeRelatedAPReconciliationLineProviderTest : TestCaseWithFactory
	{
		public void TestGetLines_MatchingCreditor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var shipment = testObjectCreator.CreateShipment("S0001");
			var job = testObjectCreator.CreateJob(shipment);
			Factory.Save();

			var invoice = testObjectCreator.CreateAPInvoice<APInvoice>("1", testObjectCreator.LocalCurrency, 1, 400M, 0, 0, 400M, 0, 0);
			invoice.AH_OH = testObjectCreator.Creditor1.PK;
			var apLine = testObjectCreator.CreateAPInvoiceLine(invoice, job, testObjectCreator.CC1, testObjectCreator.LocalCurrency, 1, "", 400M);
			var chargePosted = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 02", costCurrency: testObjectCreator.AUD, osCostAmt: 400M, creditor: testObjectCreator.Creditor1);
			chargePosted.JR_AL_APLine = apLine.PK;
			Assert("Precondition: charge 02 cost is posted", chargePosted.IsCostPosted);

			var consol = testObjectCreator.CreateConsol("AUSYD", "NZAKL", "C001001");
			var consolCost = consol.GetApportionments().CostsCollection.TryAddNew();
			consolCost.E6_OH_Creditor = testObjectCreator.Creditor1.PK;
			consolCost.E6_AC_ChargeCode = testObjectCreator.CC1.PK;
			consolCost.E6_OSCostAmount = 400m;
			var chargeApportioned = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 03", costCurrency: testObjectCreator.AUD, osCostAmt: 400M, creditor: testObjectCreator.Creditor1);
			chargeApportioned.JR_E6 = consolCost.PK;
			Assert("Precondition: charge 03 is apportioned", chargeApportioned.IsApportioned);

			Factory.Save();

			var chargeCodeInCurrentCompany = testObjectCreator.CC1;
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, testObjectCreator.NonCurrentCompanyBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var chargeInDiffCompany = testObjectCreator.CreateCharge(job, chargeCodeInCurrentCompany, "charge 04", costCurrency: testObjectCreator.AUD, osCostAmt: 400M, creditor: testObjectCreator.Creditor1);
				chargeInDiffCompany.JR_GC = testObjectCreator.NonCurrentCompany.PK;
				chargeInDiffCompany.JR_GB = testObjectCreator.NonCurrentCompanyBranch.PK;
				Factory.Save();
			}

			var chargeForDiffCreditor = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 05", costCurrency: testObjectCreator.AUD, osCostAmt: 400M, creditor: testObjectCreator.Creditor2);

			var chargeWithZeroCost = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 06", costCurrency: testObjectCreator.AUD, osCostAmt: 0M, creditor: testObjectCreator.Creditor1);

			var chargeInAUD = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 01", costCurrency: testObjectCreator.AUD, osCostAmt: 400M, creditor: testObjectCreator.Creditor1);

			var chargeInEUR = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 07", costCurrency: testObjectCreator.EUR, osCostAmt: 400M, creditor: testObjectCreator.Creditor1);

			Factory.Save();

			var draftInvoiceAUD = testObjectCreator.CreateDraftWithJob(TransactionTypes.Invoice, "001", chargeInAUD.JR_OH_CostAccount, 1200M, 0M, testObjectCreator.AUD.Code, shipment);
			Assert("Precondition: AUD is local currency", draftInvoiceAUD.Company.GC_RX_NKLocalCurrency == testObjectCreator.AUD.Code);
			AssertGetLines(provider: new JobChargeRelatedAPReconciliationLineProvider(job.JH_ParentID, draftInvoiceAUD),
				expectedJobCharges: new List<JobCharge> { chargeInAUD, chargeInEUR });

			var draftInvoiceEUR = testObjectCreator.CreateDraftWithJob(TransactionTypes.Invoice, "002", chargeInAUD.JR_OH_CostAccount, 1200M, 0M, testObjectCreator.EUR.Code, shipment);
			AssertGetLines(provider: new JobChargeRelatedAPReconciliationLineProvider(job.JH_ParentID, draftInvoiceEUR),
				expectedJobCharges: new List<JobCharge> { chargeInEUR });

			void AssertGetLines(IAPReconciliationLineProvider provider, List<JobCharge> expectedJobCharges)
			{
				var linesByIdentifier = provider.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor).ToDictionary(l => l.LineIdentifier);

				AssertEquals(expectedJobCharges.Count, linesByIdentifier.Count);
				expectedJobCharges.ForEach(charge => AssertReconciliationLine(linesByIdentifier[charge.PK], charge));
			}
		}

		public void TestGetLines_AccrualAmountFilter()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testcase = 0;
			foreach (var transactionType in new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote })
			{
				foreach (var negativeAccrualBehaviourEnabled in new[] { true, false })
				{
					testcase++;
					using (AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, negativeAccrualBehaviourEnabled))
					{
						var shipment = testObjectCreator.CreateShipment("S000" + testcase.ToString());
						var job = testObjectCreator.CreateJob(shipment);
						Factory.Save();

						var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 01", costCurrency: testObjectCreator.AUD, osCostAmt: 400M, creditor: testObjectCreator.Creditor1);
						var charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "charge 02", costCurrency: testObjectCreator.AUD, osCostAmt: -200M, creditor: testObjectCreator.Creditor1);

						Factory.Save();

						var draftTransaction = testObjectCreator.CreateDraftWithJob(transactionType, testcase.ToString(), testObjectCreator.Creditor1.PK, 600M, 0M, "AUD", shipment);

						var provider = new JobChargeRelatedAPReconciliationLineProvider(shipment.PK, draftTransaction) as IAPReconciliationLineProvider;
						var reconciliationLines = provider.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor).OrderByDescending(l => l.LocalExTaxAmount).ToArray();

						var msgPrefix = FormattableString.Invariant($"{transactionType}-{negativeAccrualBehaviourEnabled}");
						if (negativeAccrualBehaviourEnabled)
						{
							//All Non-Zero amount accruals will be loaded.
							AssertEquals(msgPrefix, 2, reconciliationLines.Length);
							AssertReconciliationLine(reconciliationLines[0], charge1, msgPrefix);
							AssertReconciliationLine(reconciliationLines[1], charge2, msgPrefix);
						}
						else if(transactionType == TransactionTypes.Invoice)
						{
							AssertEquals($"{transactionType}| When negative accrual behaviour is disabled, reconciliation line should be created for charges with a positive amount.", 1, reconciliationLines.Length);
							AssertReconciliationLine(reconciliationLines[0], charge1, msgPrefix);
						}
						else if (transactionType == TransactionTypes.CreditNote)
						{
							AssertEquals($"{transactionType}| When negative accrual behaviour is disabled, no reconciliation line should be returned.", 0, reconciliationLines.Length);
						}
					}
				}
			}
		}

		public void TestGetLines_EmptyCreditor()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var testcase = 0;
			foreach (var transactionType in new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote })
			{
				foreach (var negativeAccrualBehaviourEnabled in new[] { true, false })
				{
					testcase++;
					using (AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, negativeAccrualBehaviourEnabled))
					{
						var shipment = testObjectCreator.CreateShipment("S000" + testcase.ToString());
						var job = testObjectCreator.CreateJob(shipment);
						Factory.Save();

						var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 01", costCurrency: testObjectCreator.AUD, osCostAmt: 400M, creditor: null);
						var charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "charge 02", costCurrency: testObjectCreator.AUD, osCostAmt: -200M, creditor: null);

						Factory.Save();

						var draftTransaction = testObjectCreator.CreateDraftWithJob(transactionType, testcase.ToString(), testObjectCreator.Creditor1.PK, 600M, 0M, "AUD", shipment);

						var provider = new JobChargeRelatedAPReconciliationLineProvider(shipment.PK, draftTransaction) as IAPReconciliationLineProvider;
						var reconciliationLines = provider.GetLines(APReconciliationAccrualFilterTypes.EmptyCreditor).OrderByDescending(l => l.LocalExTaxAmount).ToArray();

						var msgPrefix = FormattableString.Invariant($"{transactionType}-{negativeAccrualBehaviourEnabled}");
						if (negativeAccrualBehaviourEnabled)
						{
							//All Non-Zero amount accruals will be loaded.
							AssertEquals(msgPrefix, 2, reconciliationLines.Length);
							AssertReconciliationLine(reconciliationLines[0], charge1, msgPrefix);
							AssertReconciliationLine(reconciliationLines[1], charge2, msgPrefix);
						}
						else if (transactionType == TransactionTypes.Invoice)
						{
							AssertEquals($"{transactionType}| When negative accrual behaviour is disabled, reconciliation line should be created for charges with a positive amount.", 1, reconciliationLines.Length);
							AssertReconciliationLine(reconciliationLines[0], charge1, msgPrefix);
						}
						else if (transactionType == TransactionTypes.CreditNote)
						{
							AssertEquals($"{transactionType}| When negative accrual behaviour is disabled, no reconciliation line should be returned.", 0, reconciliationLines.Length);
						}
					}
				}
			}
		}

		public void TestGetLines_SettlementGroupCreditors()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			//Add Creditor 1 & 2 to the same AP settlement group
			testObjectCreator.Creditor1.APSettlementGroupPK = testObjectCreator.Creditor1.PK;
			testObjectCreator.Creditor2.APSettlementGroupPK = testObjectCreator.Creditor1.PK;

			var shipment = testObjectCreator.CreateShipment("S0001");
			var job = testObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 01", costCurrency: testObjectCreator.AUD, osCostAmt: 100M, creditor: testObjectCreator.Creditor1);
			var charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "charge 02", costCurrency: testObjectCreator.AUD, osCostAmt: 200M, creditor: testObjectCreator.Creditor2);
			var charge3 = testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "charge 03", costCurrency: testObjectCreator.AUD, osCostAmt: 300M, creditor: testObjectCreator.Creditor3);

			Factory.Save();

			foreach (var transactionType in new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote })
			{
				var draftTransaction = testObjectCreator.CreateDraftWithJob(transactionType, "001", testObjectCreator.Creditor1.PK, 200M, 0M, "AUD", shipment);

				var provider = new JobChargeRelatedAPReconciliationLineProvider(shipment.PK, draftTransaction) as IAPReconciliationLineProvider;
				var reconciliationLines = provider.GetLines(APReconciliationAccrualFilterTypes.SettlementGroupCreditors).ToArray();

				var msgPrefix = draftTransaction.AIH_TransactionType;
				AssertEquals(msgPrefix, 1, reconciliationLines.Length);
				AssertReconciliationLine(reconciliationLines[0], charge2, msgPrefix);
			}
		}

		public void TestGetLines_AllOtherCreditors()
		{
			var testObjectCreator = new TestObjectCreator(Factory);

			var shipment = testObjectCreator.CreateShipment("S0001");
			var job = testObjectCreator.CreateJob(shipment);
			Factory.Save();

			var charge1 = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "charge 01", costCurrency: testObjectCreator.AUD, osCostAmt: 100M, creditor: testObjectCreator.Creditor1);
			var charge2 = testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "charge 02", costCurrency: testObjectCreator.AUD, osCostAmt: 200M, creditor: testObjectCreator.Creditor2);
			var charge3 = testObjectCreator.CreateCharge(job, testObjectCreator.CC2, "charge 03", costCurrency: testObjectCreator.AUD, osCostAmt: 300M, creditor: testObjectCreator.Creditor3);

			Factory.Save();

			foreach (var transactionType in new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote })
			{
				var draftTransaction = testObjectCreator.CreateDraftWithJob(transactionType, "001", testObjectCreator.Creditor1.PK, 200M, 0M, "AUD", shipment);

				var provider = new JobChargeRelatedAPReconciliationLineProvider(shipment.PK, draftTransaction) as IAPReconciliationLineProvider;
				var reconciliationLines = provider.GetLines(APReconciliationAccrualFilterTypes.AllOtherCreditors)
					.OrderBy(l => l.LocalExTaxAmount).ToArray();

				var msgPrefix = draftTransaction.AIH_TransactionType;
				AssertEquals(msgPrefix, 2, reconciliationLines.Length);
				AssertReconciliationLine(reconciliationLines[0], charge2, msgPrefix);
				AssertReconciliationLine(reconciliationLines[1], charge3, msgPrefix);
			}
		}

		public void TestGetLines_CreditorInvoicedExchangeRate()
		{
			var shipment = TestObjectCreator.CreateShipment("S00001001");
			var job = TestObjectCreator.CreateJob(shipment);

			Factory.Save();

			var charge = testObjectCreator.CreateCharge(job, TestObjectCreator.CC1, "Charge 1", TestObjectCreator.USD, 100m, TestObjectCreator.AALSHI);
			charge.JR_OSCostExRate = 2m;

			Factory.Save();

			var draftInvoice = TestObjectCreator.CreateDraftInvoice("00001001", "R00001001", TestObjectCreator.AALSHI.PK, 100m, 0m, TestObjectCreator.AUD.Code);

			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.USD, 5m);
			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.GBP, 10m);

			var jobCluster = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice, 0m);
			TestObjectCreator.AddJobToTheCluster(jobCluster, shipment);

			Factory.Save();

			var provider = new JobChargeRelatedAPReconciliationLineProvider(shipment.PK, draftInvoice) as IAPReconciliationLineProvider;
			var rLineCollection = provider.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor);
			var rLine = rLineCollection.First();

			AssertEquals(1, rLineCollection.Count());
			AssertEquals(5m, rLine.ExchangeRate);
			AssertEquals(20m, rLine.LocalExTaxAmount);
			AssertEquals(100m, rLine.OSExTaxAmount);
		}

		void AssertReconciliationLine(APReconciliationLine line, JobCharge charge, string message = "")
		{
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.LineIdentifier)}"), charge.PK, line.LineIdentifier);
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.Description)}"), FormattableString.Invariant($"{charge.Job?.JH_JobNum}-{charge.ChargeCode.AC_Code}-{charge.JR_Desc}"), line.Description);
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.LocalCurrency)}"), charge.Company.LocalCurrency.Code, line.LocalCurrency);
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.OSCurrency)}"), charge.JR_RX_NKCostCurrency, line.OSCurrency);
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.LineType)}"), APReconciliationLineTypes.JobCharge, line.LineType);
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.ExchangeRate)}"), charge.JR_OSCostExRate, line.ExchangeRate);
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.CreditorPk)}"), charge.JR_OH_CostAccount, line.CreditorPk);
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.LocalExTaxAmount)}"), charge.JR_LocalCostAmt, line.LocalExTaxAmount);
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.OSExTaxAmount)}"), charge.JR_OSCostAmt, line.OSExTaxAmount);
			AssertEquals(FormattableString.Invariant($"{message}-{nameof(line.ParentId)}"), charge.Job.JH_ParentID, line.ParentId);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
