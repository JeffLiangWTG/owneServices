using System;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(usp_IniLoad_AGG__Consignee))]
	internal class usp_IniLoad_AGG__ConsigneeTest : BiCreateScriptTest
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
				AssertRowValues(resultTable, "ESABSHA", 1, "ESAB CO.", 1, 2, 2, new Guid("47AFF155-FE3C-4921-9766-FB1A5976A47B"));
			});
		}

		void AssertRowValues(DataTable resultTable, string code, int? docaddresskey, string fullname, int? organizationaddresskey, int? organizationaddressorganizationkey, int? organizationkey, Guid? parentID)
		{
			var selectqry = string.Format("Code {0} AND DocAddressKey {1} AND FullName {2} AND OrganizationAddressKey {3} AND OrganizationAddressOrganizationKey {4} AND OrganizationKey {5} AND ParentID {6}",
				code == null ? "IS NULL" : "= '" + code + "'",
				docaddresskey == null ? "IS NULL" : "= " + docaddresskey,
				fullname == null ? "IS NULL" : "= '" + fullname + "'",
				organizationaddresskey == null ? "IS NULL" : "= '" + organizationaddresskey + "'",
				organizationaddressorganizationkey == null ? "IS NULL" : "= '" + organizationaddressorganizationkey + "'",
				organizationkey == null ? "IS NULL" : "= '" + organizationkey + "'",
				parentID == null ? "IS NULL" : "= '" + parentID + "'"
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		DataTable SelectRows()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[AGG__Consignee]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		void PrepareTestData()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__DocAddress]
					([AddressOverride], [AddressSequence], [AddressType], [CompanyName], [DocAddressKey], [OrganizationAddressKey], [ParentID], [ParentTableCode], [OrganizationAddressID], [DocAddressID])
				VALUES
					(0,	0, 'CED', 'Test organization', 1, 1, '47AFF155-FE3C-4921-9766-FB1A5976A47B', 'JS', 'BCF529F1-A996-4196-961D-2B0729BB39EB', '2F397815-1B9B-403C-99B1-451FF647044C');

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]
					([OrganizationAddressKey], [OrganizationAddressID], [Organization], [OrganizationKey])
				VALUES
					(1, '7A3BB44E-E13D-430B-AAB9-A90B3E419D74', '6DCC457E-BE7A-40FF-B1C5-81BA611BA050', 2);

				INSERT [{0}].[Organization].[BAS__Organization]
					([Code], [FullName], [OrganizationKey], [OrganizationID])
				VALUES
					('ESABSHA', 'ESAB CO.', 2, '6DCC457E-BE7A-40FF-B1C5-81BA611BA050');
				",
					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
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
	}
}
