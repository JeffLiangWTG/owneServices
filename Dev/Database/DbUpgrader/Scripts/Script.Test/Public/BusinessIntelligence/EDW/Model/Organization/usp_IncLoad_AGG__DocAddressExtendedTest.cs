using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Organization;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Organization.Testing
{
	[TestedType(typeof(usp_IncLoad_AGG__DocAddressExtended))]
	class usp_IncLoad_AGG__DocAddressExtendedTest : BiCreateScriptTest
	{
		[ExpectNoExceptions]
		public void TestRun()
		{
			PrepareTestData();
			Execute();
			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, "Test 1:", daAddressOverride: false, 0, "CON", "Test Co. 1", 1, 1, new Guid("F49A01E2-163D-4E1A-82CC-4E178B8A7C65"), "PAR", 1, "ORG1ACON", "Test Org. 1", 1);
			});
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__DocAddress]
					([AddressOverride], [AddressSequence], [AddressType], [CompanyName], [DocAddressKey], [OrganizationAddressKey], [ParentID], [ParentTableCode], [OrganizationAddressID], [DocAddressID])
				VALUES
					(0, 0, 'CON', 'Test Co. 1', 1, 1, 'F49A01E2-163D-4E1A-82CC-4E178B8A7C65', 'PAR', '7A3BB44E-E13D-430B-AAB9-A90B3E419D74', '81F67AEA-75AE-486E-947F-00092F4FC750');

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]
					([OrganizationAddressKey], [OrganizationAddressID], [Organization])
				VALUES
					(1, '7A3BB44E-E13D-430B-AAB9-A90B3E419D74', '6DCC457E-BE7A-40FF-B1C5-81BA611BA050');

				INSERT [{0}].[Organization].[BAS__Organization]
					([Code], [FullName], [OrganizationKey], [OrganizationID])
				VALUES
					('ORG1ACON', 'Test Org. 1', 1, '6DCC457E-BE7A-40FF-B1C5-81BA611BA050');

				INSERT INTO [{0}].[biadmin].[TransformedRow] (SchemaName, TableName, KeyValue)
				SELECT 'InternationalLogistics', 'BAS__DocAddress', DocAddressKey
				FROM [{0}].[InternationalLogistics].[BAS__DocAddress];
				", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void AssertRowValues(DataTable resultTable, string testName, bool? daAddressOverride, byte? daAddressSequence, string daAddressType, string daCompanyName, int? daKey, int? daOrganizationAddressKey, Guid? daParentID, string daParentTableCode, int? orgAddressKey, string orgCode, string orgFullName, int? orgKey)
		{
			var selectQuery = string.Format("DocAddressAddressOverride {0} AND DocAddressAddressSequence {1} AND DocAddressAddressType {2} AND DocAddressCompanyName {3} AND DocAddressKey {4} AND DocAddressOrganizationAddressKey {5} AND DocAddressParentID {6} AND DocAddressParentTableCode {7} AND OrganizationAddressKey {8} AND OrganizationCode {9} AND OrganizationFullName {10} AND OrganizationKey {11}",
				daAddressOverride == null ? "IS NULL" : "= " + daAddressOverride,
				daAddressSequence == null ? "IS NULL" : "= " + daAddressSequence,
				daAddressType == null ? "IS NULL" : "= '" + daAddressType + "'",
				daCompanyName == null ? "IS NULL" : "= '" + daCompanyName + "'",
				daKey == null ? "IS NULL" : "= " + daKey,
				daOrganizationAddressKey == null ? "IS NULL" : "= " + daOrganizationAddressKey,
				daParentID == null ? "IS NULL" : "= '" + daParentID + "'",
				daParentTableCode == null ? "IS NULL" : "= '" + daParentTableCode + "'",
				orgAddressKey == null ? "IS NULL" : "= " + orgAddressKey,
				orgCode == null ? "IS NULL" : "= '" + orgCode + "'",
				orgFullName == null ? "IS NULL" : "= '" + orgFullName + "'",
				orgKey == null ? "IS NULL" : "= " + orgKey
				);

			var rows = resultTable.Select(selectQuery);

			AssertEquals(testName, 1, rows.Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		void Execute()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
				"EXEC [{0}].[{1}].[{2}]",
				ScriptDbName,
				ScriptToTest.SchemaName,
				ScriptToTest.Name
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[Organization].[AGG__DocAddressExtended]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}
	}
}
