using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_CUS__JobDeclarationExtended))]
	internal class vw_CUS__JobDeclarationExtendedTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
			HashSet<string> columns = GetColumns();
			TestColumnsAreAsExpected(ScriptToTest.Name, columns);

			PrepareTestData();
			Execute();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, "Test 1:", new Guid("E34C3B3A-8634-4B08-BEC2-658AB0B7F567"), 1, "ORG1", 1, new Guid("1AAC679E-6C8E-4D0B-A601-6FBA3FA54855"), "ORG1", 1, new Guid("1AAC679E-6C8E-4D0B-A601-6FBA3FA54855"), "ORG2", 2, new Guid("4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29"));
				AssertRowValues(resultTable, "Test 2:", new Guid("5C722829-ADA9-4F02-BB8A-D20A70086CFE"), 2, "ORG3", 3, new Guid("32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C"), "ORG3", 3, new Guid("32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C"), "ORG4", 4, new Guid("415AF163-DD9F-4C05-9E28-002EFC35E760"));
			});
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>();

			columns.Add("JobDeclarationID");
			columns.Add("JobDeclarationKey");
			columns.Add("ReceivingForwarderCode");
			columns.Add("ReceivingForwarderKey");
			columns.Add("ReceivingForwarderPK");
			columns.Add("SendingForwarderCode");
			columns.Add("SendingForwarderKey");
			columns.Add("SendingForwarderPK");
			columns.Add("ShippingLineCode");
			columns.Add("ShippingLineKey");
			columns.Add("ShippingLinePK");

			return columns;
		}
		void AssertRowValues(DataTable resultTable, string testName, Guid? jobDeclarationID, int? jobDeclarationKey, string receivingForwarderCode, int? receivingForwarderKey, Guid? receivingForwarderPK, string sendingForwarderCode, int? sendingForwarderKey, Guid? sendingForwarderPK, string shippingLineCode, int? shippingLineKey, Guid? shippingLinePK)
		{
			var selectqry = string.Format("JobDeclarationID {0} AND JobDeclarationKey {1} AND ReceivingForwarderCode {2} AND ReceivingForwarderKey {3} AND ReceivingForwarderPK {4} AND SendingForwarderCode {5} AND SendingForwarderKey {6} AND SendingForwarderPK {7} AND ShippingLineCode {8} AND ShippingLineKey {9} AND ShippingLinePK {10}",
				jobDeclarationID == null ? "IS NULL" : "= '" + jobDeclarationID + "'",
				jobDeclarationKey == null ? "IS NULL" : "= " + jobDeclarationKey,
				receivingForwarderCode == null ? "IS NULL" : "='" + receivingForwarderCode + "'",
				receivingForwarderKey == null ? "IS NULL" : "= " + receivingForwarderKey,
				receivingForwarderPK == null ? "IS NULL" : "= '" + receivingForwarderPK + "'",
				sendingForwarderCode == null ? "IS NULL" : "='" + sendingForwarderCode + "'",
				sendingForwarderKey == null ? "IS NULL" : "= " + sendingForwarderKey,
				sendingForwarderPK == null ? "IS NULL" : "= '" + sendingForwarderPK + "'",
				shippingLineCode == null ? "IS NULL" : "='" + shippingLineCode + "'",
				shippingLineKey == null ? "IS NULL" : "= " + shippingLineKey,
				shippingLinePK == null ? "IS NULL" : "= '" + shippingLinePK + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals(testName, 1, rows.Length);
		}
		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_CUS__JobDeclarationExtended]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Customs].[BAS__Declaration]
				([DeclarationKey], [DeclarationID],[ForwarderKey], [ShippingLineKey])
				VALUES
					(1, 'E34C3B3A-8634-4B08-BEC2-658AB0B7F567', 1, 2),
					(2, '5C722829-ADA9-4F02-BB8A-D20A70086CFE', 3, 4)

				INSERT [{0}].[Organization].[BAS__Organization]
				([OrganizationKey], [OrganizationID], [FullName], [Code])
				VALUES
					(1, '1AAC679E-6C8E-4D0B-A601-6FBA3FA54855', 'Test Org 1', 'ORG1'),
					(2, '4F9B9C96-BEC4-4F4C-96A3-0043A3AEAF29', 'Test Org 2', 'ORG2'),
					(3, '32EC689D-09BE-40C1-A4B9-C6A37E2ECB6C', 'Test Org 3', 'ORG3'),
					(4, '415AF163-DD9F-4C05-9E28-002EFC35E760', 'Test Org 4', 'ORG4')

				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT * FROM [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}
	}
}
