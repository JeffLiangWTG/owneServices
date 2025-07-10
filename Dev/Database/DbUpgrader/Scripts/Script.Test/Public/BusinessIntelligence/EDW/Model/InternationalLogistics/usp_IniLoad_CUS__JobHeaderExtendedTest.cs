using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_CUS__JobHeaderExtended))]
	internal class usp_IniLoad_CUS__JobHeaderExtendedTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 2, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, 1, new Guid("6F1755E3-D182-4844-9399-E8585E8D3BE8"), 1, 1, "Job1", "SYD", "AU1", "FCL", "Test1", "Test2", "Org1", "Test Orgnization 1");
				AssertRowValues(resultTable, 2, new Guid("6F1755E3-D182-4844-9399-E8585E8D3BE8"), 2, 2, "Job2", "MEL", "TW1", "AIR", "Test2", "Test1", "Org1", "Test Orgnization 1");
			});
		}

		void AssertRowValues(DataTable resultTable, int? jobHeaderKey, Guid? clientPK, int? branchKey, int? deptKey, string jobNum, string jobBranchCode, string jobDeptCode, string decContainerMode, string salesRepName, string operatorName, string localClient, string jobOverseasAgentName)
		{
			var selectqry = string.Format("JobHeaderKey {0} AND ClientPK {1} AND BranchKey {2} AND DepartmentKey {3} AND JobNum {4} AND JobBranchCode {5} AND JobDeptCode {6} AND DeclarationContainerMode {7} AND SalesRepName {8} AND OperatorName {9} AND LocalClient {10} AND JobOverseasAgentName {11}",
				jobHeaderKey == null ? "IS NULL" : "= " + jobHeaderKey,
				clientPK == null ? "IS NULL" : "= '" + clientPK + "'",
				branchKey == null ? "IS NULL" : "= " + branchKey,
				deptKey == null ? "IS NULL" : "= " + deptKey,
				jobNum == null ? "IS NULL" : "= '" + jobNum + "'",
				jobBranchCode == null ? "IS NULL" : "= '" + jobBranchCode + "'",
				jobDeptCode == null ? "IS NULL" : "= '" + jobDeptCode + "'",
				decContainerMode == null ? "IS NULL" : "= '" + decContainerMode + "'",
				salesRepName == null ? "IS NULL" : "= '" + salesRepName + "'",
				operatorName == null ? "IS NULL" : "= '" + operatorName + "'",
				localClient == null ? "IS NULL" : "= '" + localClient + "'",
				jobOverseasAgentName == null ? "IS NULL" : "= '" + jobOverseasAgentName + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__JobHeader]
					([JobHeaderKey], [JobHeaderID], [JobNo], [BranchKey], [DepartmentKey], [ShipmentKey], [DeclarationKey], [LocalAgentAddressKey], [ConsolidationKey], [RepresentativeSalesKey], [RepresentativeOperatorKey], [AddressKey])
					VALUES
						(1, newid(), 'Job1', 1, 1, 1, 1, 1, 1, 1, 2, 1),
						(2, newid(), 'Job2', 2, 2, 2, 2, 1, 1, 2, 1, 1);

				INSERT [{0}].[Organization].[BAS__Branch]
					([BranchKey], [BranchID], [BranchCode])
					VALUES
						(1, newid(), 'SYD'),
						(2, newid(), 'MEL');

				INSERT [{0}].[Organization].[BAS__Department]
					([DepartmentKey], [DepartmentID], [Code])
					VALUES
						(1, newid(), 'AU1'),
						(2, newid(), 'TW1');

				INSERT [{0}].[Organization].[BAS__Staff]
					([StaffKey], [StaffID], [FullName])
					VALUES
						(1, newid(), 'Test1'),
						(2, newid(), 'Test2');

				INSERT INTO [{0}].[Customs].[BAS__Declaration]
					([DeclarationKey], [DeclarationID], [ContainerMode], [TransportMode], [Origin])
					VALUES
						(1, newid(), 'FCL', 'SEA', 'AUSYD'),
						(2, newid(), 'AIR', 'AIR', 'AUMBE');

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]
					([OrganizationAddressKey], [OrganizationAddressID], [Organization], [OrganizationKey])
					VALUES (1, newid(), '6F1755E3-D182-4844-9399-E8585E8D3BE8', 1);

				INSERT [{0}].[Organization].[BAS__Organization]
					([OrganizationKey], [OrganizationID], [Code], [FullName])
					VALUES (1, '6F1755E3-D182-4844-9399-E8585E8D3BE8', 'Org1', 'Test Orgnization 1');

				INSERT [{0}].[InternationalLogistics].[BAS__Consolidation]
					([ConsolidationKey], [ConsolidationID], [IsCFS], [IsForwarding], [ReceivingForwarderHandlingType],[SendingForwarderHandlingType])
					VALUES
						(1, newid(), 0, 1, 'GTT', 'GTA'),
						(2, newid(), 1, 1, 'GTX', 'GTZ');",

					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[CUS__JobHeaderExtended]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		string GetIniLoadSQLText()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT [InitialLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'CUS__JobHeaderExtended'",
					ScriptDbName
			);

			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var iniLoadSQLText = record.ItemArray[0].ToString();
			return iniLoadSQLText;
		}

		void Execute()
		{
			var iniLoadSQLText = GetIniLoadSQLText();
			var sqlText1 = "USE " + ScriptDbName + " " + iniLoadSQLText;
			var sqlText2 = "USE " + Db.DatabaseName;

			TestConnection.ExecuteNonQuery(sqlText1);
			TestConnection.ExecuteNonQuery(sqlText2);
		}
	}
}
