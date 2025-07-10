using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.InternationalLogistics;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.InternationalLogistics.Testing
{
	[TestedType(typeof(vw_CUS__JobDocAddress))]
	internal class vw_CUS__JobDocAddressTest : BiCreateScriptTest
	{
		/// <summary>
		/// Basic auto-generated test.
		/// Please replace it with more elaborated tests.
		/// </summary>

		[ExpectNoExceptions]
		public void TestRun()
		{
			var columns = GetColumns();
			TestColumnsAreAsExpected(ScriptToTest.Name, columns);

			PrepareTestData();
			Execute();

			var resultTable = SelectRows();
			AssertEquals("Rowcount", 1, resultTable.Rows.Count);

			CombineAssertions(() =>
			{
				AssertRowValues(resultTable, new Guid("B80FB034-05BD-4F0F-AD6E-767D9332DEF5"), "JS", "CRG", 0, 1, "RM 2602 PEAR PLAZA NO 9", "WEST XDIANQIAN ROAD", "CITY GUANGZHOU", "GUANGDONG", "029996", "China", 1, 1, 1, null, 1);
			});
		}

		void AssertRowValues(DataTable resultTable, Guid? parentID, string parentTableCode, string addressType, int? addressSequence, int? addressOverride, string address1, string address2, string city, string state, string postCode, string description, int? docAddressKey, int? organizationAddressKey, int? organizationKey, int? refUNLOCOKey, int? countryKey)
		{
			var selectqry = string.Format("ParentID {0} AND ParentTableCode {1} AND AddressType {2} AND AddressSequence {3} AND AddressOverride {4} AND Address1 {5} AND Address2 {6} AND City {7} AND State {8} AND PostCode {9} AND Description {10} AND DocAddressKey {11} AND OrganizationAddressKey {12} AND OrganizationKey {13} AND RefUNLOCOKey {14} AND CountryKey {15}",
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

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>();

			columns.Add("ParentID");
			columns.Add("ParentTableCode");
			columns.Add("AddressType");
			columns.Add("AddressSequence");
			columns.Add("AddressOverride");
			columns.Add("Address1");
			columns.Add("Address2");
			columns.Add("City");
			columns.Add("State");
			columns.Add("PostCode");
			columns.Add("Description");
			columns.Add("CountryKey");
			columns.Add("DocAddressKey");
			columns.Add("OrganizationAddressKey");
			columns.Add("OrganizationKey");
			columns.Add("RefUNLOCOKey");

			return columns;
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
					"SELECT * FROM [{0}].[InternationalLogistics].[vw_CUS__JobDocAddress]",
					ScriptDbName
			);

			return DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
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
