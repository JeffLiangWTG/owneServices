using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Utility.Testing;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests.CashFlow
{
	[UseSnapshotProtection(new[] { DatabaseType.Main })]
	class CashFlowActivityTypesInRegistryTest : ScriptTest
	{
		string ScriptDbName => Db.DatabaseName;

		public void TestCashFlowActivityConfigurationDefaultValue()
		{
			var dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Code, CashFlowDescription, ActivityType, ActivityDescription FROM [{ScriptDbName}].[dbo].CashFlowActivityTypesInRegistry()");
			AssertEquals("Result should have rows", 23, dataTable.Rows.Count);

			var result = dataTable.Rows.Cast<DataRow>().Select(row => Tuple.Create(row["Code"].ToString(), row["CashFlowDescription"].ToString(), row["ActivityType"].ToString(), row["ActivityDescription"].ToString())).ToArray();
			AssertArrayEqualsByElements(new[]
			{
				Tuple.Create("XXX", "Undefined", "X", "Undefined"),
				Tuple.Create("NON", "Non Cash", "N", "Non Cash"),
				Tuple.Create("CSH", "Cash or Cash Equivalent", "C", "Cash or Cash Equivalent"),
				Tuple.Create("EXX", "Effects of Exchange Rate Change", "E", "Effects of Exchange Rate Change"),
				Tuple.Create("O01", "Receipts From Customers", "O", "Operating Activities"),
				Tuple.Create("O02", "Receipts From Other Operating Activities", "O", "Operating Activities"),
				Tuple.Create("O03", "Payments to Suppliers", "O", "Operating Activities"),
				Tuple.Create("O04", "Payments to Employees", "O", "Operating Activities"),
				Tuple.Create("O05", "Payment of Taxes", "O", "Operating Activities"),
				Tuple.Create("O06", "Payments for Other Operating Activities", "O", "Operating Activities"),
				Tuple.Create("I01", "Dividends Received", "I", "Investing Activities"),
				Tuple.Create("I02", "Proceeds from Disposal of Non-Current Assets", "I", "Investing Activities"),
				Tuple.Create("I03", "Purchases of Non-Current Assets", "I", "Investing Activities"),
				Tuple.Create("I04", "Proceeds from Disposal of Financial Assets", "I", "Investing Activities"),
				Tuple.Create("I05", "Acquisitions of Financial Assets", "I", "Investing Activities"),
				Tuple.Create("F01", "Interests Paid", "F", "Financing Activities"),
				Tuple.Create("F02", "Dividends Paid", "F", "Financing Activities"),
				Tuple.Create("F03", "Proceeds from Bank Borrowings", "F", "Financing Activities"),
				Tuple.Create("F04", "Repayments of Bank Borrowings", "F", "Financing Activities"),
				Tuple.Create("F05", "Proceeds from Issuance of New Shares", "F", "Financing Activities"),
				Tuple.Create("F06", "Loans from Intercompany", "F", "Financing Activities"),
				Tuple.Create("F07", "Repayments of Intercompany Loans", "F", "Financing Activities"),
				Tuple.Create("F08", "Repayments of Finance Leases", "F", "Financing Activities"),
			}, result);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Code, CashFlowDescription, ActivityType, ActivityDescription FROM [{ScriptDbName}].[dbo].CashFlowActivityTypesInRegistry() where Code = 'M01'");
			AssertEquals("Result should NOT contain override value M01", 0, dataTable.Rows.Count);
			dataTable = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Code, CashFlowDescription, ActivityType, ActivityDescription FROM [{ScriptDbName}].[dbo].CashFlowActivityTypesInRegistry() where Code = 'M02'");
			AssertEquals("Result should NOT contain override value M02", 0, dataTable.Rows.Count);
		}

		public void TestCashFlowActivityConfigurationOverrideValue()
		{
			PrepareData();

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Code, CashFlowDescription, ActivityType, ActivityDescription FROM [{ScriptDbName}].[dbo].CashFlowActivityTypesInRegistry()");
			AssertEquals("Result should have rows", 25, result.Rows.Count);
			foreach (var code in defaultCodeList)
			{
				var defaultCodeInResult = result.AsEnumerable().Where(x => x["Code"].Equals(code));
				AssertEquals(string.Format("Result should NOT contain code {0}", code), 1, defaultCodeInResult.Count());
			}
			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Code, CashFlowDescription, ActivityType, ActivityDescription FROM [{ScriptDbName}].[dbo].CashFlowActivityTypesInRegistry() where Code = 'M01'");
			AssertEquals("Result should NOT contain override value M01", 1, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Code, CashFlowDescription, ActivityType, ActivityDescription FROM [{ScriptDbName}].[dbo].CashFlowActivityTypesInRegistry() where Code = 'M02'");
			AssertEquals("Result should NOT contain override value M02", 1, result.Rows.Count);
		}

		public void TestCashFlowDescription_ActivityDescription_NotEndUpWithWhitespace()
		{
			PrepareData();

			var result = DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT Code, CashFlowDescription, ActivityType, ActivityDescription FROM [{ScriptDbName}].[dbo].CashFlowActivityTypesInRegistry()");

			var selectedCashFlowDescriptionList = result.Rows.OfType<DataRow>().OrderBy(x => x["CashFlowDescription"].ToString()).Select(x => x["CashFlowDescription"].ToString()).Distinct().ToArray();
			var selectedActivityDescriptionList = result.Rows.OfType<DataRow>().OrderBy(x => x["ActivityDescription"].ToString()).Select(x => x["ActivityDescription"].ToString()).Distinct().ToArray();

			AssertArrayEqualsByElements(defaultCashFlowDescriptionList, selectedCashFlowDescriptionList);
			AssertArrayEqualsByElements(defaultActivityDescriptionList, selectedActivityDescriptionList);
		}

		void PrepareData()
		{
			TestConnection.ExecuteNonQuery(insertRegistryValues);
		}

		readonly string[] defaultCodeList = { "XXX", "NON", "CSH", "EXX", "O01", "O02", "O03", "O04", "O05", "O06", "I01", "I02", "I03", "I04", "I05", "F01", "F02", "F03", "F04", "F05", "F06", "F07", "F08" };

		readonly string[] defaultCashFlowDescriptionList = { "Acquisitions of Financial Assets",
"Cash or Cash Equivalent",
"Dividends Paid",
"Dividends Received",
"Effects of Exchange Rate Change",
"Interests Paid",
"Loans from Intercompany",
"my test code 1",
"my test code 2",
"Non Cash",
"Payment of Taxes",
"Payments for Other Operating Activities",
"Payments to Employees",
"Payments to Suppliers",
"Proceeds from Bank Borrowings",
"Proceeds from Disposal of Financial Assets",
"Proceeds from Disposal of Non-Current Assets",
"Proceeds from Issuance of New Shares",
"Purchases of Non-Current Assets",
"Receipts From Customers",
"Receipts From Other Operating Activities",
"Repayments of Bank Borrowings",
"Repayments of Finance Leases",
"Repayments of Intercompany Loans",
"Undefined" };

		readonly string[] defaultActivityDescriptionList = { "Cash or Cash Equivalent",
"Effects of Exchange Rate Change",
"Financing Activities",
"Investing Activities",
"Non Cash",
"Operating Activities",
"Undefined" };

		readonly string insertRegistryValues = @"DECLARE @registryRawValue nvarchar(MAX)
Set @registryRawValue = '<ArrayOfCashFlowActivityConfiguration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""><CashFlowActivityConfiguration><Code>CSH</Code><Description>Cash or Cash Equivalent</Description><ActivityType>C</ActivityType><ActivityDescription>Cash or Cash Equivalent</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>EXX</Code><Description>Effects of Exchange Rate Change</Description><ActivityType>E</ActivityType><ActivityDescription>Effects of Exchange Rate Change</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>F01</Code><Description>Interests Paid</Description><ActivityType>F</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>F02</Code><Description>Dividends Paid</Description><ActivityType>F</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>F03</Code><Description>Proceeds from Bank Borrowings</Description><ActivityType>F</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>F04</Code><Description>Repayments of Bank Borrowings</Description><ActivityType>F</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>F05</Code><Description>Proceeds from Issuance of New Shares</Description><ActivityType>F</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>F06</Code><Description>Loans from Intercompany</Description><ActivityType>F</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>F07</Code><Description>Repayments of Intercompany Loans</Description><ActivityType>F</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>F08</Code><Description>Repayments of Finance Leases</Description><ActivityType>F</ActivityType><ActivityDescription>Financing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>I01</Code><Description>Dividends Received</Description><ActivityType>I</ActivityType><ActivityDescription>Investing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>I02</Code><Description>Proceeds from Disposal of Non-Current Assets</Description><ActivityType>I</ActivityType><ActivityDescription>Investing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>I03</Code><Description>Purchases of Non-Current Assets</Description><ActivityType>I</ActivityType><ActivityDescription>Investing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>I04</Code><Description>Proceeds from Disposal of Financial Assets</Description><ActivityType>I</ActivityType><ActivityDescription>Investing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>I05</Code><Description>Acquisitions of Financial Assets</Description><ActivityType>I</ActivityType><ActivityDescription>Investing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>M01</Code><Description>my test code 1</Description><ActivityType>O</ActivityType><ActivityDescription>Operating Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>M02</Code><Description>my test code 2</Description><ActivityType>I</ActivityType><ActivityDescription>Investing Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>NON</Code><Description>Non Cash</Description><ActivityType>N</ActivityType><ActivityDescription>Non Cash</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>O01</Code><Description>Receipts From Customers</Description><ActivityType>O</ActivityType><ActivityDescription>Operating Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>O02</Code><Description>Receipts From Other Operating Activities</Description><ActivityType>O</ActivityType><ActivityDescription>Operating Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>O03</Code><Description>Payments to Suppliers</Description><ActivityType>O</ActivityType><ActivityDescription>Operating Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>O04</Code><Description>Payments to Employees</Description><ActivityType>O</ActivityType><ActivityDescription>Operating Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>O05</Code><Description>Payment of Taxes</Description><ActivityType>O</ActivityType><ActivityDescription>Operating Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>O06</Code><Description>Payments for Other Operating Activities</Description><ActivityType>O</ActivityType><ActivityDescription>Operating Activities</ActivityDescription></CashFlowActivityConfiguration><CashFlowActivityConfiguration><Code>XXX</Code><Description>Undefined</Description><ActivityType>X</ActivityType><ActivityDescription>Undefined</ActivityDescription></CashFlowActivityConfiguration></ArrayOfCashFlowActivityConfiguration>'
INSERT INTO dbo.StmData
(SD_PK, SD_Name, SD_Type, SD_BinaryValue, SD_IsLogged, SD_IsCancelled)
Values
(newID(), 'CashFlowActivityConfiguration', 'BIN', CONVERT(varbinary(MAX), @registryRawValue), 1, 0)";
	}
}
