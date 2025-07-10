using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.ServiceTasks.JobCostingReport;

namespace Enterprise.Accounting.ServiceTasks.Testing
{
	class JCDTableAndPartitionObjectTest : TestCaseWithFactory
	{
		public void TestGetTableAndPartitionInfo()
		{
			var items = JCDTableAndPartitionObject.GetTableAndPartitionInfo();

			AssertEquals("Number of DB Object", 4, items.Length);

			//RptDtUnprocessedAccTransactionLines
			AssertEquals("RptDtUnprocessedAccTransactionLines:Name", "RptDtUnprocessedAccTransactionLines", items[0].Name);
			AssertEquals("RptDtUnprocessedAccTransactionLines:Sequence", 1, items[0].Sequence);
			AssertEquals("RptDtUnprocessedAccTransactionLines:IsTable", true, items[0].IsTable);
			AssertEquals("RptDtUnprocessedAccTransactionLines:IsTemporary", true, items[0].IsTemporary);
			AssertEquals("RptDtUnprocessedAccTransactionLines:Table Name matches with Name property", true, items[0].CreateSQLText.StartsWith(string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0}", items[0].Name)));

			//PS_AccountingPeriodCompany
			AssertEquals("PS_AccountingPeriodCompany:Name", "PS_AccountingPeriodCompany", items[1].Name);
			AssertEquals("PS_AccountingPeriodCompany:Sequence", 2, items[1].Sequence);
			AssertEquals("PS_AccountingPeriodCompany:IsTable", false, items[1].IsTable);
			AssertEquals("PS_AccountingPeriodCompany:IsTemporary", false, items[1].IsTemporary);
			AssertEquals("PS_AccountingPeriodCompany:Partition Name matches with Name property", true, items[1].CreateSQLText.Contains(string.Format(CultureInfo.InvariantCulture, "CREATE PARTITION SCHEME {0}", items[1].Name)));

			//RptDtUnprocessedReversedAL
			AssertEquals("RptDtUnprocessedReversedAL:Name", "RptDtUnprocessedReversedAL", items[2].Name);
			AssertEquals("RptDtUnprocessedReversedAL:Sequence", 3, items[2].Sequence);
			AssertEquals("RptDtUnprocessedReversedAL:IsTable", true, items[2].IsTable);
			AssertEquals("RptDtUnprocessedReversedAL:IsTemporary", true, items[2].IsTemporary);
			AssertEquals("RptDtUnprocessedReversedAL:Table Name matches with Name property", true, items[2].CreateSQLText.StartsWith(string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0}", items[2].Name)));

			//RptDtJobCostingData
			AssertEquals("RptDtJobCostingData:Name", "RptDtJobCostingData", items[3].Name);
			AssertEquals("RptDtJobCostingData:Sequence", 4, items[3].Sequence);
			AssertEquals("RptDtJobCostingData:IsTable", true, items[3].IsTable);
			AssertEquals("RptDtJobCostingData:IsTemporary", false, items[3].IsTemporary);
			AssertEquals("RptDtJobCostingData:Table Name matches with Name property", true, items[3].CreateSQLText.Equals(string.Format(CultureInfo.InvariantCulture, "EXEC CreateJobCostingDataTableAndCCI @isStagingTable = 0", items[3].Name)));
		}

		public void TestAllTableNameStartsWithRptDt()
		{
			var tables = JCDTableAndPartitionObject.GetTableAndPartitionInfo().Where(t => t.IsTable).ToList();

			AssertEquals("Number of DB Object", 3, tables.Count);

			tables.ForEach((t) =>
			{
				if (t.Name == "RptDtJobCostingData")
				{
					AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0}:Table Name matches with Name property", t.Name), true, t.CreateSQLText.Equals("EXEC CreateJobCostingDataTableAndCCI @isStagingTable = 0"));
				}
				else
				{
					AssertEquals(string.Format(CultureInfo.InvariantCulture, "{0}:Table Name matches with Name property", t.Name), true, t.CreateSQLText.StartsWith(string.Format(CultureInfo.InvariantCulture, "CREATE TABLE {0}", t.Name)));
				}

				AssertEquals(t.Name, true, t.Name.StartsWith("RptDt"));
			});
		}
	}
}
