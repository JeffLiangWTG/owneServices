using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.APAutomation.Testing
{
	public class ConsolCostRelatedAPReconciliationLineProviderTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			objectCreator = new TestObjectCreator(Factory);

			//Setup AP Settlement Group with Creditor1 & Creditor2
			objectCreator.Creditor1.APSettlementGroupPK = objectCreator.Creditor1.PK;
			objectCreator.Creditor2.APSettlementGroupPK = objectCreator.Creditor1.PK;
		}
		TestObjectCreator objectCreator;

		[SuspendCriticalValidation]
		public void TestGetLines_ConsolCosts()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var costMatchingCreditor = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: objectCreator.Creditor1);
			var costSettlementGroupCreditor = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: objectCreator.Creditor2);
			var costOtherCreditor = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: objectCreator.Creditor3);
			var costEmptyCreditor = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: null);

			foreach (var transactionType in new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote })
			{
				var draft = objectCreator.CreateDraftWithConsol(transactionType, "001", objectCreator.Creditor1.PK, 800M, 0M, objectCreator.AUD.Code, consol);
				var provider = new ConsolCostRelatedAPReconciliationLineProvider(consol.PK, draft) as IAPReconciliationLineProvider;

				CombineAssertions(() =>
				{
					AssertConsolCostsFound(provider, APReconciliationAccrualFilterTypes.MatchingCreditor, new[] { costMatchingCreditor });
					AssertConsolCostsFound(provider, APReconciliationAccrualFilterTypes.EmptyCreditor, new[] { costEmptyCreditor });
					AssertConsolCostsFound(provider, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, new[] { costSettlementGroupCreditor });
					AssertConsolCostsFound(provider, APReconciliationAccrualFilterTypes.AllOtherCreditors, new[] { costSettlementGroupCreditor, costOtherCreditor });
				});
			}

			void AssertConsolCostsFound(IAPReconciliationLineProvider provider, APReconciliationAccrualFilterTypes filterType, JobConsolCost[] expectedConsolCosts)
			{
				var lineIdentifiers = provider.GetLines(filterType).Select(l => l.LineIdentifier);
				var expectedConsolCostPks = expectedConsolCosts.Select(c => c.PK);
				AssertContainsExactElementsInAnyOrder($"Filter:{filterType}", expectedConsolCostPks, lineIdentifiers);
			}
		}

		[SuspendCriticalValidation]
		public void TestGetLines_ConsolCharges()
		{
			var gatewayConsol = objectCreator.CreateGatewayConsol(receivingGatewayCompany: GlbCompany.CurrentCompany);
			var gatewayBillingJob = objectCreator.CreateJob(gatewayConsol);
			Factory.Save();

			var chargeMatchingCreditor = objectCreator.CreateCharge(gatewayBillingJob, objectCreator.CC1, "charge 1", costCurrency: objectCreator.AUD, osCostAmt: 400M, creditor: objectCreator.Creditor1);
			var chargeSettlementGroupCreditor = objectCreator.CreateCharge(gatewayBillingJob, objectCreator.CC1, "charge 2", costCurrency: objectCreator.AUD, osCostAmt: 400M, creditor: objectCreator.Creditor2);
			var chargeOtherCreditor = objectCreator.CreateCharge(gatewayBillingJob, objectCreator.CC1, "charge 3", costCurrency: objectCreator.AUD, osCostAmt: 400M, creditor: objectCreator.Creditor3);
			var chargeEmptyCreditor = objectCreator.CreateCharge(gatewayBillingJob, objectCreator.CC1, "charge 4", costCurrency: objectCreator.AUD, osCostAmt: 400M, creditor: null);
			Factory.Save();

			foreach (var transactionType in new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote })
			{
				var draft = objectCreator.CreateDraftWithConsol(transactionType, "001", objectCreator.Creditor1.PK, 800M, 0M, objectCreator.AUD.Code, gatewayConsol);
				var provider = new ConsolCostRelatedAPReconciliationLineProvider(gatewayConsol.PK, draft) as IAPReconciliationLineProvider;

				CombineAssertions(() =>
				{
					AssertJobChargesFound(provider, APReconciliationAccrualFilterTypes.MatchingCreditor, new[] { chargeMatchingCreditor });
					AssertJobChargesFound(provider, APReconciliationAccrualFilterTypes.EmptyCreditor, new[] { chargeEmptyCreditor });
					AssertJobChargesFound(provider, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, new[] { chargeSettlementGroupCreditor });
					AssertJobChargesFound(provider, APReconciliationAccrualFilterTypes.AllOtherCreditors, new[] { chargeSettlementGroupCreditor, chargeOtherCreditor });
				});
			}
		}

		[SuspendCriticalValidation]
		public void TestGetLines_RelatedJobCharges()
		{
			var forwardingConsol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = objectCreator.CreateShipment("S0001", forwardingConsol);
			var shipmentWithNoJob = objectCreator.CreateShipment("S0002", forwardingConsol);
			var job = objectCreator.CreateJob(shipment);
			Factory.Save();

			var chargeMatchingCreditor = objectCreator.CreateCharge(job, objectCreator.CC1, "charge 1", costCurrency: objectCreator.AUD, osCostAmt: 400M, creditor: objectCreator.Creditor1);
			var chargeSettlementGroupCreditor = objectCreator.CreateCharge(job, objectCreator.CC1, "charge 2", costCurrency: objectCreator.AUD, osCostAmt: 400M, creditor: objectCreator.Creditor2);
			var chargeOtherCreditor = objectCreator.CreateCharge(job, objectCreator.CC1, "charge 3", costCurrency: objectCreator.AUD, osCostAmt: 400M, creditor: objectCreator.Creditor3);
			var chargeEmptyCreditor = objectCreator.CreateCharge(job, objectCreator.CC1, "charge 4", costCurrency: objectCreator.AUD, osCostAmt: 400M, creditor: null);
			Factory.Save();

			foreach (var transactionType in new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote })
			{
				var draft = objectCreator.CreateDraftWithConsol(transactionType, "001", objectCreator.Creditor1.PK, 800M, 0M, objectCreator.AUD.Code, forwardingConsol);
				var provider = new ConsolCostRelatedAPReconciliationLineProvider(forwardingConsol.PK, draft) as IAPReconciliationLineProvider;

				CombineAssertions(() =>
				{
					AssertJobChargesFound(provider, APReconciliationAccrualFilterTypes.MatchingCreditor, new[] { chargeMatchingCreditor });
					AssertJobChargesFound(provider, APReconciliationAccrualFilterTypes.EmptyCreditor, new[] { chargeEmptyCreditor });
					AssertJobChargesFound(provider, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, new[] { chargeSettlementGroupCreditor });
					AssertJobChargesFound(provider, APReconciliationAccrualFilterTypes.AllOtherCreditors, new[] { chargeSettlementGroupCreditor, chargeOtherCreditor });
				});
			}
		}

		[SuspendCriticalValidation]
		public void TestGetLines_NonZeroCosts()
		{
			CombineAssertions(() =>
			{
				AssertShouldIncludeNonZeroCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.MatchingCreditor, objectCreator.Creditor1);
				AssertShouldIncludeNonZeroCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.EmptyCreditor, null);
				AssertShouldIncludeNonZeroCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, objectCreator.Creditor2);
				AssertShouldIncludeNonZeroCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.AllOtherCreditors, objectCreator.Creditor3);
				AssertShouldIncludeNonZeroCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.MatchingCreditor, objectCreator.Creditor1);
				AssertShouldIncludeNonZeroCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.EmptyCreditor, null);
				AssertShouldIncludeNonZeroCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, objectCreator.Creditor2);
				AssertShouldIncludeNonZeroCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.AllOtherCreditors, objectCreator.Creditor3);
			});

			void AssertShouldIncludeNonZeroCosts(string transactionType, APReconciliationAccrualFilterTypes filterType, OrgHeader creditor)
			{
				using (AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var consol = Factory.NewWithValidTestData<ForwardingConsol>();
					var positive = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: creditor);
					var negative = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, -200M, creditor: creditor);
					var zero = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 0M, creditor: creditor);

					var draft = objectCreator.CreateDraftWithConsol(transactionType, "001", objectCreator.Creditor1.PK, 800M, 0M, objectCreator.AUD.Code, consol);
					var provider = new ConsolCostRelatedAPReconciliationLineProvider(consol.PK, draft) as IAPReconciliationLineProvider;

					var lines = provider.GetLines(filterType).ToArray();
					AssertContainsExactElementsInAnyOrder($"TransactionType:{transactionType}|Filter:{filterType}",
						new[] { positive.PK, negative.PK },
						lines.Select(l => l.LineIdentifier));
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestGetLines_ExcludeNegativeCosts()
		{
			CombineAssertions(() =>
			{
				AssertShouldExcludeNegativeCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.MatchingCreditor, objectCreator.Creditor1);
				AssertShouldExcludeNegativeCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.EmptyCreditor, null);
				AssertShouldExcludeNegativeCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, objectCreator.Creditor2);
				AssertShouldExcludeNegativeCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.AllOtherCreditors, objectCreator.Creditor3);
				AssertShouldExcludeNegativeCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.MatchingCreditor, objectCreator.Creditor1);
				AssertShouldExcludeNegativeCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.EmptyCreditor, null);
				AssertShouldExcludeNegativeCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, objectCreator.Creditor2);
				AssertShouldExcludeNegativeCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.AllOtherCreditors, objectCreator.Creditor3);
			});

			void AssertShouldExcludeNegativeCosts(string transactionType, APReconciliationAccrualFilterTypes filterType, OrgHeader creditor)
			{
				using (AccountingConfigurationRegistry.Instance.EnableNegativeAccrualBehaviors.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var consol = Factory.NewWithValidTestData<ForwardingConsol>();
					var positive = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: creditor);
					var negative = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, -200M, creditor: creditor);

					var draft = objectCreator.CreateDraftWithConsol(transactionType, "001", objectCreator.Creditor1.PK, 800M, 0M, objectCreator.AUD.Code, consol);
					var provider = new ConsolCostRelatedAPReconciliationLineProvider(consol.PK, draft) as IAPReconciliationLineProvider;

					var lines = provider.GetLines(filterType).ToArray();

					if (transactionType == TransactionTypes.Invoice)
					{
						AssertEquals($"{transactionType}| When negative accrual behaviour is disabled, reconciliation line should be created for charges with a positive amount.", 1, lines.Length);
						AssertEquals(positive.PK, lines[0].LineIdentifier);
					}
					else if (transactionType == TransactionTypes.CreditNote)
					{
						AssertEquals($"{transactionType}| When negative accrual behaviour is disabled, no reconciliation line should be returned.", 0, lines.Length);
					}
				}
			}
		}

		[SuspendCriticalValidation]
		public void TestGetLines_UnPostedCosts()
		{
			CombineAssertions(() =>
			{
				AssertShouldIncludeUnPostedCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.MatchingCreditor, objectCreator.Creditor1);
				AssertShouldIncludeUnPostedCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.EmptyCreditor, null);
				AssertShouldIncludeUnPostedCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, objectCreator.Creditor2);
				AssertShouldIncludeUnPostedCosts(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.AllOtherCreditors, objectCreator.Creditor3);
				AssertShouldIncludeUnPostedCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.MatchingCreditor, objectCreator.Creditor1);
				AssertShouldIncludeUnPostedCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.EmptyCreditor, null);
				AssertShouldIncludeUnPostedCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, objectCreator.Creditor2);
				AssertShouldIncludeUnPostedCosts(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.AllOtherCreditors, objectCreator.Creditor3);
			});

			void AssertShouldIncludeUnPostedCosts(string transactionType, APReconciliationAccrualFilterTypes filterType, OrgHeader creditor)
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var consolCost = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: creditor);
				var msgPrefix = $"TransactionType:{transactionType}|Filter:{filterType}";

				var draft = objectCreator.CreateDraftWithConsol(transactionType, "001", objectCreator.Creditor1.PK, 800M, 0M, objectCreator.AUD.Code, consol);
				var provider = new ConsolCostRelatedAPReconciliationLineProvider(consol.PK, draft) as IAPReconciliationLineProvider;

				var lines = provider.GetLines(filterType).ToArray();
				AssertEquals(msgPrefix, 1, lines.Length);
				AssertEquals(msgPrefix, consolCost.PK, lines.First().LineIdentifier);

				var invoice = objectCreator.CreateAPInvoice<APInvoice>(TestObjectCreator.GetRandomString(5), objectCreator.AUD, 1, 400M, 0, 0, 400M, 0, 0);
				invoice.AH_OH = objectCreator.Creditor1.PK;
				consolCost.E6_AH_APInvoice = invoice.PK;
				Assert($"{msgPrefix} - Precondition: consol cost is posted", consolCost.IsPosted);

				lines = provider.GetLines(filterType).ToArray();
				AssertEquals($"{msgPrefix} - posted consol costs should be excluded", 0, lines.Length);
			}
		}

		[SuspendCriticalValidation]
		public void TestGetLines_ConsolCostsCurrency()
		{
			CombineAssertions(() =>
			{
				AssertShouldIncludeCostsInCorrectCurrency(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.MatchingCreditor, objectCreator.Creditor1);
				AssertShouldIncludeCostsInCorrectCurrency(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.EmptyCreditor, null);
				AssertShouldIncludeCostsInCorrectCurrency(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, objectCreator.Creditor2);
				AssertShouldIncludeCostsInCorrectCurrency(TransactionTypes.Invoice, APReconciliationAccrualFilterTypes.AllOtherCreditors, objectCreator.Creditor3);
				AssertShouldIncludeCostsInCorrectCurrency(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.MatchingCreditor, objectCreator.Creditor1);
				AssertShouldIncludeCostsInCorrectCurrency(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.EmptyCreditor, null);
				AssertShouldIncludeCostsInCorrectCurrency(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.SettlementGroupCreditors, objectCreator.Creditor2);
				AssertShouldIncludeCostsInCorrectCurrency(TransactionTypes.CreditNote, APReconciliationAccrualFilterTypes.AllOtherCreditors, objectCreator.Creditor3);
			});

			void AssertShouldIncludeCostsInCorrectCurrency(string transactionType, APReconciliationAccrualFilterTypes filterType, OrgHeader creditor)
			{
				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var consolCostLocalCurrency = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: creditor);
				var consolCostForeignCurrency = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.EUR, 1M, 400M, creditor: creditor);
				var msgPrefix = $"TransactionType:{transactionType}|Filter:{filterType}";

				var draft = objectCreator.CreateDraftWithConsol(transactionType, "001", objectCreator.Creditor1.PK, 800M, 0M, objectCreator.AUD.Code, consol);
				Assert($"{msgPrefix} - Precondition: AUD is local currency", draft.IsInLocalCurrency);
				var provider = new ConsolCostRelatedAPReconciliationLineProvider(consol.PK, draft) as IAPReconciliationLineProvider;

				var lines = provider.GetLines(filterType).ToArray();
				AssertContainsExactElementsInAnyOrder(msgPrefix,
					new[] { consolCostLocalCurrency.PK, consolCostForeignCurrency.PK },
					lines.Select(l => l.LineIdentifier));

				draft.AIH_RX_NKTransactionCurrency = objectCreator.EUR.Code;

				lines = provider.GetLines(filterType).ToArray();
				AssertContainsExactElementsInAnyOrder(msgPrefix,
					new[] { consolCostForeignCurrency.PK },
					lines.Select(l => l.LineIdentifier));
			}
		}

		[SuspendCriticalValidation]
		public void TestGetLines_SettlementGroupCreditors()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			AssertEquals("Precondition", objectCreator.Creditor1.APSettlementGroupPK, objectCreator.Creditor2.APSettlementGroupPK);
			var consolCost = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: objectCreator.Creditor2);

			foreach (var transactionType in new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote })
			{
				var msgPrefix = $"TransactionType:{transactionType}";

				var lines = GetAccrualsForCreditor(transactionType, objectCreator.Creditor3);
				AssertEquals($"{msgPrefix} - the creditor has no AP settlement group", 0, lines.Length);

				lines = GetAccrualsForCreditor(transactionType, objectCreator.Creditor2);
				AssertEquals($"{msgPrefix} - no accruals for the other creditors in the group", 0, lines.Length);

				lines = GetAccrualsForCreditor(transactionType, objectCreator.Creditor1);
				AssertEquals(msgPrefix, 1, lines.Length);
				AssertEquals(msgPrefix, consolCost.PK, lines.Single().LineIdentifier);
			}

			APReconciliationLine[] GetAccrualsForCreditor(string transactionType, OrgHeader creditor)
			{
				var draft = objectCreator.CreateDraftWithConsol(transactionType, "001", creditor.PK, 800M, 0M, objectCreator.AUD.Code, consol);
				var provider = new ConsolCostRelatedAPReconciliationLineProvider(consol.PK, draft) as IAPReconciliationLineProvider;
				return provider.GetLines(APReconciliationAccrualFilterTypes.SettlementGroupCreditors).ToArray();
			}
		}

		public void TestGetLines_CreditorInvoicedExchangeRate()
		{
			var consol = TestObjectCreator.CreateConsol(consolNum: "C00001001");
			var shipment1 = TestObjectCreator.CreateShipment("S00001003", consol);
			var shipment2 = testObjectCreator.CreateShipment("S00001004", consol);
			TestObjectCreator.CreateJob(shipment1);
			TestObjectCreator.CreateJob(shipment2);

			Factory.Save();

			var consolCost = TestObjectCreator.CreateConsolCost(consol, TestObjectCreator.CC3, TestObjectCreator.USD, 2m, 100m, TestObjectCreator.AALSHI);

			Factory.Save();

			var draftInvoice = TestObjectCreator.CreateDraftInvoice("00001001", "R00001001", TestObjectCreator.AALSHI.PK, 100m, 0m, TestObjectCreator.AUD.Code);

			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.USD, 5m);
			TestObjectCreator.AddDraftInvocieExchangeRate(draftInvoice, TestObjectCreator.GBP, 10m);

			var jobCluster = TestObjectCreator.AddClusterToDraftTransaction(draftInvoice, 0m);
			TestObjectCreator.AddJobToTheCluster(jobCluster, consol);

			Factory.Save();

			var provider = new ConsolCostRelatedAPReconciliationLineProvider(consol.PK, draftInvoice) as IAPReconciliationLineProvider;
			var rLineCollection = provider.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor);
			var rLine = rLineCollection.First();

			AssertEquals(1, rLineCollection.Count());
			AssertEquals(5m, rLine.ExchangeRate);
			AssertEquals(20m, rLine.LocalExTaxAmount);
			AssertEquals(100m, rLine.OSExTaxAmount);
		}

		[SuspendCriticalValidation]
		public void TestGetLines_ConvertsCostToReconciliationLine()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var cost = objectCreator.CreateConsolCost(consol, objectCreator.FRT, objectCreator.AUD, 1M, 400M, creditor: objectCreator.Creditor1);
			var draft = objectCreator.CreateDraftWithConsol(TransactionTypes.Invoice, "001", objectCreator.Creditor1.PK, 800M, 0M, objectCreator.AUD.Code, consol);
			var provider = new ConsolCostRelatedAPReconciliationLineProvider(consol.PK, draft) as IAPReconciliationLineProvider;
			var lines = provider.GetLines(APReconciliationAccrualFilterTypes.MatchingCreditor).ToList();
			AssertEquals(1, lines.Count);

			var line = lines.Single();
			AssertEquals(nameof(line.LineIdentifier), cost.PK, line.LineIdentifier);
			AssertEquals(nameof(line.Description), FormattableString.Invariant($"{cost.Consol?.JK_UniqueConsignRef}-{cost.ChargeCode.AC_Code}-{cost.E6_Description}"), line.Description);
			AssertEquals(nameof(line.LocalCurrency), "AUD", line.LocalCurrency);
			AssertEquals(nameof(line.OSCurrency), "AUD", line.OSCurrency);
			AssertEquals(nameof(line.LineType), APReconciliationLineTypes.ConsolCost, line.LineType);
			AssertEquals(nameof(line.ExchangeRate), cost.E6_ExchangeRate, line.ExchangeRate);
			AssertEquals(nameof(line.CreditorPk), objectCreator.Creditor1.PK, line.CreditorPk);
			AssertEquals(nameof(line.LocalExTaxAmount), 400M, line.LocalExTaxAmount);
			AssertEquals(nameof(line.OSExTaxAmount), 400M, line.OSExTaxAmount);
			AssertEquals(nameof(line.ParentId), cost.E6_ParentID, line.ParentId);
		}

		void AssertJobChargesFound(IAPReconciliationLineProvider provider, APReconciliationAccrualFilterTypes filterType, IEnumerable<JobCharge> expectedJobCharges)
		{
			var lineIdentifiers = provider.GetLines(filterType).Select(l => l.LineIdentifier);
			var expectedJobChargePks = expectedJobCharges.Select(c => c.PK);
			AssertContainsExactElementsInAnyOrder($"Filter:{filterType}", expectedJobChargePks, lineIdentifiers);
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);
		TestObjectCreator testObjectCreator;
	}
}
