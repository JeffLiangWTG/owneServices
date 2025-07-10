using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Data;
using Enterprise.Billing.Integration;
using Enterprise.Integration.Billing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Testing
{
	static class BillingTestHelper
	{
		public static IEnumerable<BillingTransaction> GetBillingTransactionsFromDatabase(DbConnection testConnection)
		{
			var billingTransactions = new List<BillingTransaction>();

			using (var cmd = testConnection.Command("SELECT SUD_Data FROM dbo.StmUsageData WHERE SUD_Category <> 'USG'"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					billingTransactions.Add(BillingManager.DecryptTransaction(reader[0].ToString(), BillingManager.CurrentSchemaVersion));
				}
			}

			return billingTransactions;
		}

		public static IEnumerable<UsageTransaction> GetUsageTransactionsFromDatabase(DbConnection testConnection)
		{
			var usageTransactions = new List<UsageTransaction>();

			using (var cmd = testConnection.Command("SELECT SUD_Data FROM dbo.StmUsageData WHERE SUD_Category = 'USG'"))
			using (var reader = cmd.ExecuteReader())
			{
				while (reader.Read())
				{
					usageTransactions.Add(BillingManager.DecryptUsageTransaction(reader[0].ToString()));
				}
			}

			return usageTransactions;
		}

		public static DateTime StlCollectorHighWaterMarkRegistry
		{
			get
			{
				return SystemDataRegistry.Instance.StlCollectorHighWaterMark.Value;
			}
			set
			{
				SystemDataRegistry.Instance.StlCollectorHighWaterMark.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
			}
		}

		public static int AssertDetailRangeTransactions(IEnumerable<BillingTransaction> billingTransactions, IDateTimeRange collectionRange)
		{
			var detailRangeTransactions = billingTransactions.Where(t => t.PriceItemCode == "TRN");

			var detailRangeTransaction = detailRangeTransactions.Single();
			AssertTransactionFields(detailRangeTransaction);
			Assertion.AssertEquals("PriceItemCode", "TRN", detailRangeTransaction.PriceItemCode);
			Assertion.AssertEquals("ServiceOccuredUTC", collectionRange.StartDateTimeInclusive, detailRangeTransaction.ServiceOccuredUTC);
			Assertion.AssertEquals("Reference1", "DAILY#1", detailRangeTransaction.Reference1);
			Assertion.AssertEquals("Reference2", "DAILY#2", detailRangeTransaction.Reference2);
			Assertion.AssertNull("Reference3", detailRangeTransaction.Reference3);
			Assertion.AssertNull("Reference4", detailRangeTransaction.Reference4);
			Assertion.AssertEquals("Reference5", "448AC110-9B18-497F-8F6E-CA80AC82E098", detailRangeTransaction.Reference5);
			Assertion.AssertEquals("BillableCount", 1, detailRangeTransaction.BillableCount);

			return detailRangeTransactions.Count();
		}

		public static int AssertMonthlyAllowHistoricalRangeTransactions(IEnumerable<BillingTransaction> billingTransactions, IDateTimeRange collectionRange)
		{
			var monthlyTransactions = billingTransactions.Where(t => t.PriceItemCode == "MON");

			var monthlyTransaction = monthlyTransactions.Single();
			AssertTransactionFields(monthlyTransaction);
			Assertion.AssertEquals("PriceItemCode", "MON", monthlyTransaction.PriceItemCode);
			Assertion.AssertEquals("ServiceOccuredUTC", collectionRange.MonthlyRangeStartInclusive, monthlyTransaction.ServiceOccuredUTC);
			Assertion.AssertEquals("Reference1", "MONTHLYHISTORICAL#1", monthlyTransaction.Reference1);
			Assertion.AssertEquals("Reference2", "MONTHLYHISTORICAL#2", monthlyTransaction.Reference2);
			Assertion.AssertEquals("Reference3", "MONTHLYHISTORICAL#3", monthlyTransaction.Reference3);
			Assertion.AssertNull("Reference4", monthlyTransaction.Reference4);
			Assertion.AssertEquals("Reference5", "C93C8E21-5E1B-4265-9616-82D0383E99EA", monthlyTransaction.Reference5);
			Assertion.AssertEquals("BillableCount", 20, monthlyTransaction.BillableCount);

			return monthlyTransactions.Count();
		}

		public static int AssertMonthlyCurrentOnlyRangeTransactions(IEnumerable<BillingTransaction> billingTransactions, IDateTimeRange collectionRange)
		{
			var monthlyTransactions = billingTransactions.Where(t => t.PriceItemCode == "MCO");

			var monthlyTransaction = monthlyTransactions.Single();
			AssertTransactionFields(monthlyTransaction);
			Assertion.AssertEquals("PriceItemCode", "MCO", monthlyTransaction.PriceItemCode);
			Assertion.AssertEquals("ServiceOccuredUTC", collectionRange.MonthlyRangeStartInclusive, monthlyTransaction.ServiceOccuredUTC);
			Assertion.AssertEquals("Reference1", "MONTHLYCURRENT#1", monthlyTransaction.Reference1);
			Assertion.AssertEquals("Reference2", "MONTHLYCURRENT#2", monthlyTransaction.Reference2);
			Assertion.AssertEquals("Reference3", "MONTHLYCURRENT#3", monthlyTransaction.Reference3);
			Assertion.AssertEquals("Reference4", "MONTHLYCURRENT#4", monthlyTransaction.Reference4);
			Assertion.AssertEquals("Reference5", "171D86DF-065D-4232-8F9D-246FF3612EF5", monthlyTransaction.Reference5);
			Assertion.AssertEquals("BillableCount", 25, monthlyTransaction.BillableCount);

			return monthlyTransactions.Count();
		}

		public static int AssertDailyTransactions(IEnumerable<BillingTransaction> billingTransactions, IDateTimeRange collectionRange)
		{
			var transactions = billingTransactions.Where(t => t.PriceItemCode == "DAY");

			var transaction = transactions.Single();
			AssertTransactionFields(transaction);
			Assertion.AssertEquals("PriceItemCode", "DAY", transaction.PriceItemCode);
			Assertion.AssertEquals("ServiceOccuredUTC", collectionRange.DailyRangeStartInclusive.Value, transaction.ServiceOccuredUTC);
			Assertion.AssertEquals("Reference1", "DAILY#1", transaction.Reference1);
			Assertion.AssertEquals("Reference2", "DAILY#2", transaction.Reference2);
			Assertion.AssertEquals("Reference3", "DAILY#3", transaction.Reference3);
			Assertion.AssertEquals("Reference4", "DAILY#4", transaction.Reference4);
			Assertion.AssertEquals("Reference5", "44F618EA-6D0C-47F2-8C3C-57BB635F6015", transaction.Reference5);
			Assertion.AssertEquals("BillableCount", 5, transaction.BillableCount);

			return transactions.Count();
		}

		public static int AssertStlMilestoneTransactions(IEnumerable<BillingTransaction> billingTransactions, IDateTimeRange collectionRange, IEnumerable<IStlScriptWithConfig> scripts)
		{
			var stlMilestoneTransactions = billingTransactions.Where(t => t.PriceItemCode == "STL");
			var mandatoryScripts = scripts.Where(s => s.Script.IsMandatoryForMilestones);
			var stlMilestoneTransaction = stlMilestoneTransactions.Single();
			AssertTransactionCategory(stlMilestoneTransaction);
			Assertion.AssertEquals("PriceItemCode", "STL", stlMilestoneTransaction.PriceItemCode);
			Assertion.AssertEquals("ServiceOccuredUTC", collectionRange.StlMilestoneTimestamp.Value, stlMilestoneTransaction.ServiceOccuredUTC);
			Assertion.AssertEquals("BillableCount", 1, stlMilestoneTransaction.BillableCount);
			Assertion.AssertEquals("Reference1", SqlFormatInfo.ToSqlDateString(collectionRange.StlMilestoneTimestamp.Value), stlMilestoneTransaction.Reference1);
			Assertion.AssertEquals("Reference2", (collectionRange is AusydMonthRange) ? "MONTH" : "DAY", stlMilestoneTransaction.Reference2);
			Assertion.AssertNull("Reference3", stlMilestoneTransaction.Reference3);
			Assertion.AssertNull("Reference4", stlMilestoneTransaction.Reference4);
			Assertion.AssertNull("Reference5", stlMilestoneTransaction.Reference5);
			Assertion.AssertNull("Branch", stlMilestoneTransaction.Branch);
			Assertion.AssertNull("ClientStaffCode", stlMilestoneTransaction.ClientStaffCode);
			Assertion.AssertEquals("AdditionalRefs", GetExpectedAdditionalRefsForStlMilestone(mandatoryScripts), stlMilestoneTransaction.AdditionalRefs);

			return stlMilestoneTransactions.Count();
		}

		public static void AssertUsageTransactions(IEnumerable<UsageTransaction> usageTransactions, int expectedTimebasedCount, int expectedLiveConfigCount)
		{
			var timeBasedUsageTransactionCount = AssertSnapshotTimeBasedTransactions(usageTransactions);
			Assertion.AssertEquals("Snapshot Timebased - transaction count", expectedTimebasedCount, timeBasedUsageTransactionCount);
			var liveConfigurationUsageTransactionCount = AssertSnapshotLiveConfigurationTransactions(usageTransactions);
			Assertion.AssertEquals("Snapshot live configuration - transaction count", expectedLiveConfigCount, liveConfigurationUsageTransactionCount);
		}

		public static int AssertSnapshotTimeBasedTransactions(IEnumerable<UsageTransaction> usageTransactions)
		{
			return usageTransactions.Count(t => t.BillableCount == 57);
		}

		public static int AssertSnapshotLiveConfigurationTransactions(IEnumerable<UsageTransaction> usageTransactions)
		{
			return usageTransactions.Count(t => t.BillableCount == 17);
		}

		public static string GetExpectedAdditionalRefsForStlMilestone(IEnumerable<IStlScriptWithConfig> mandatoryScripts)
		{
			var builder = new StringBuilder("{\n  \"MandatoryItemsCount\": ");
			builder.Append(mandatoryScripts.Count().ToString());
			builder.Append(",\n  \"CargoWiseVersion\": \"");
			builder.Append(ReleaseInfo.Instance.VersionNumber.ToString());
			builder.Append("\",\n  \"BilledPriceItemCodes\": [");
			var mandatoryScriptArray = mandatoryScripts.ToArray();
			for (int i = 0; i < mandatoryScriptArray.Length; i++)
			{
				builder.Append($"\n    \"{mandatoryScriptArray[i].Script.Code}\"");
				if (i < mandatoryScriptArray.Length - 1)
				{
					builder.Append(",");
				}
			}
			builder.Append("\n  ]\n}");

			return builder.ToString();
		}

		static void AssertTransactionCategory(BillingTransaction transaction)
		{
			Assertion.AssertEquals("ReportingSource", BillingManager.ReportingSource, transaction.ReportingSource);
			Assertion.AssertEquals("Category", "STL", transaction.Category);
		}

		static void AssertTransactionFields(BillingTransaction transaction)
		{
			AssertTransactionCategory(transaction);
			Assertion.AssertEquals("Branch", "~BR", transaction.Branch);
			Assertion.AssertEquals("ClientStaffCode", "USR", transaction.ClientStaffCode);
		}
	}
}
