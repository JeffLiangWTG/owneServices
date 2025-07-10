using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Model.InternationalLogistics
{
	[TestedType(typeof(vw_CUS__JobDocAddress))]
	internal class usp_IniLoad_CUS__JobDocAddressTest : BiCreateScriptTest
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
				AssertRowValues(resultTable, 1, new Guid("B80FB034-05BD-4F0F-AD6E-767D9332DEF5"), "JS", "CRG", 0, 1, "RM 2602 PEAR PLAZA NO 9", "WEST XDIANQIAN ROAD", "CITY GUANGZHOU", "GUANGDONG", "029996", "China", 1, 1, 1, null, 1);
			});
		}

		void AssertRowValues(DataTable resultTable, int? jobDocAddressKey, Guid? parentID, string parentTableCode, string addressType, int? addressSequence, int? addressOverride, string address1, string address2, string city, string state, string postCode, string description, int? docAddressKey, int? organizationAddressKey, int? organizationKey, int? refUNLOCOKey, int? countryKey)
		{
			var selectqry = string.Format("JobDocAddressKey {0} AND ParentID {1} AND ParentTableCode {2} AND AddressType {3} AND AddressSequence {4} AND AddressOverride {5} AND Address1 {6} AND Address2 {7} AND City {8} AND State {9} AND PostCode {10} AND Description {11} AND DocAddressKey {12} AND OrganizationAddressKey {13} AND OrganizationKey {14} AND RefUNLOCOKey {15} AND CountryKey {16}",
				jobDocAddressKey == null ? "IS NULL" : "= " + jobDocAddressKey,
				parentID == null ? "IS NULL" : "= '" + parentID + "'",
				parentTableCode == null ? "IS NULL" : "= '" + parentTableCode + "'",
				addressType == null ? "IS NULL" : "= '" + addressType + "'",
				addressSequence == null ? "IS NULL" : "= " + addressSequence,
				addressOverride == null ? "IS NULL" : "= " + addressOverride,
				address1 == null ? "IS NULL" : "= '" + address1 + "'",
				address2 == null ? "IS NULL" : "= '" + address2 + "'",
				city == null ? "IS NULL" : "= '" + city + "'",
				state == null ? "IS NULL" : "= '" + state + "'",
				postCode == null ? "IS NULL" : "= '" + postCode + "'",
				description == null ? "IS NULL" : "= '" + description + "'",
				docAddressKey == null ? "IS NULL" : "= " + docAddressKey,
				organizationAddressKey == null ? "IS NULL" : "= " + organizationAddressKey,
				organizationKey == null ? "IS NULL" : "= " + organizationKey,
				refUNLOCOKey == null ? "IS NULL" : "= " + refUNLOCOKey,
				countryKey == null ? "IS NULL" : "= " + countryKey
				);

			var rows = resultTable.Select(selectqry);

			AssertEquals("Rowcount should be 1", 1, rows.Length);
		}

		void PrepareTestData()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[InternationalLogistics].[BAS__DocAddress]
					([DocAddressKey], [DocAddressID], [ParentID], [ParentTableCode], [AddressType], [AddressSequence], [AddressOverride], [Address1], [Address2], [City], [State], [PostCode], [NKCountryCode] , [OrganizationAddressKey])
					VALUES
						(1,  '96A2CEAD-0308-4966-9DA2-F72C54A397D5',	'B80FB034-05BD-4F0F-AD6E-767D9332DEF5',	'JS', 'CRG',	0,	1,	'RM 2602 PEAR PLAZA NO 9',	'WEST XDIANQIAN ROAD',	'CITY GUANGZHOU',	'GUANGDONG',	'029996', 'CN',	1),
						(2,	 newid()							   ,    '79E2ED2A-917D-414B-9859-6882F3C902B1',	'JS', 'CRG',	0,	1,	'2ND FLOORNO 45 1 INGLIYUAN',	'BINHU DISTRICT WUXI',	'CITY GUANGZHOU',	'GUANGDONG',	'029996', 'CN',	2);

				INSERT [{0}].[Organization].[BAS__OrganizationAddress]
					([OrganizationAddressKey], [OrganizationAddressID], [Address1], [Address2], [City], [State], [PostCode], [Organization], [OrganizationKey])
					VALUES
						(1, '96A2CEAD-0308-4966-9DA2-F72C54A397D5', 'RM 2602 PEAR PLAZA NO 9',	'WEST XDIANQIAN ROAD',	'CITY GUANGZHOU',	'GUANGDONG',	'029996', '57EB1112-B012-4686-AA73-C0123B8F9A00', 1);

				INSERT [{0}].[Organization].[BAS__Organization]
					([OrganizationID], [OrganizationKey], [ClosestPort])
					VALUES
						('57EB1112-B012-4686-AA73-C0123B8F9A00', 1, 'CNSHA');

				INSERT [{0}].[Organization].[BAS__RefUNLOCO]
					([Code], [RefUNLOCOID], [RefUNLOCOKey], [CountryCode])
					VALUES
						('CNSHA', newid(), 1, 'CN');

				INSERT INTO [{0}].[Geography].[BAS__Country]
					([CountryKey], [CountryID], [Code], [Description])
					VALUES
						(1, newid(), 'CN', 'China');",

					ScriptDbName
			);

			TestConnection.ExecuteNonQuery(sqlText);
		}

		DataTable SelectRows()
		{
			string sqlText = string.Format(CultureInfo.InvariantCulture,
					"SELECT * FROM [{0}].[InternationalLogistics].[CUS__JobDocAddress]",
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
					"SELECT [InitialLoadQuery] FROM [{0}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'InternationalLogistics' AND [ModelTableName] = 'CUS__JobDocAddress'",
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
