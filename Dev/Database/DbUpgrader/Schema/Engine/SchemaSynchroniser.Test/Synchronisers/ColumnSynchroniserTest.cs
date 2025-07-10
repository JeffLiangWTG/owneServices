using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.SqlServer;
using CargoWise.Data.Testing;
using CargoWise.Database.Abstractions;
using CargoWise.Database.ExtendedProperties;
using Enterprise.DbUpgrader.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Enterprise.DbUpgrader.Schema.Testing
{
	[UseSnapshotProtection]
	sealed class ColumnSynchroniserTest : TestCaseWithMockMainDbAndTemplateDbTransactional
	{
		#region TestDropAlterAndAddColumns

		public void TestDropAlterAndAddColumns()
		{
			AssertPreConditions();

			var testSynchroniser = new ColumnSynchroniserForTesting(TestConnection, mockMainDb, mockTemplateDb);
			RunActionOnMockMainDb(testSynchroniser.DropAlterAndAddColumns);
			var modifiedTables = testSynchroniser.DistinctModifiedTablesIgnoringCase.ToList();
			AssertColumnsSynchronised();
			AssertTablesToRebuildOnlineAfterAlterAndDropColumns();
			var expected = "dbo.AlterTableIssues, dbo.IdentityColumnAdded, dbo.IdentityColumnModified, dbo.TestCharToChar, dbo.TestCompleteOnlineTransformedColumnUpdates, dbo.TestComputedColumns, dbo.TestDateTimeNoRename, dbo.TestDateTimeToDateTimeOffset, dbo.TestDateToDateTimeOffset, dbo.TestIntToMoney, dbo.TestMoneyToDecimal, dbo.TestPreservedColumns, dbo.XmlTypeChanges";
			AssertEquals(expected, string.Join(", ", modifiedTables.Select(x => x.SchemaName + "." + x.TableName).OrderBy(y => y)));
		}

		/// <summary>
		/// PRE-CONDITION Assertions (Asserts schema before synchronisation)
		/// </summary>
		void AssertPreConditions()
		{
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0206_Uid_2_Int", "UNIQUEIDENTIFIER", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0220_Int_2_SmallInt", "INT", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0260_Char_2_Money", "CHAR", "YES", "5");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0298_Datetime_2_SmallDatetime", "DATETIME", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0515_Null_2_NotNull", "INT", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err4928_VarcharMax_2_NVarcharMax", "VARCHAR", "YES", "-1");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err8115_BigInt_2_Int", "BIGINT", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_ToBeDropped", "INT", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err8152_Char10_2_Char5", "CHAR", "YES", "10");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_ToBeDropped_WithStats", "INT", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err8169_Char1_2_Uid", "CHAR", "YES", "1");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Binary7_2_Binary3", "BINARY", "YES", "7");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Ntext_2_Nvarchar5", "NTEXT", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Char_2_Bit", "CHAR", "YES", "1");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Char_2_Varchar", "CHAR", "YES", "5");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Computed_2_NotComputed", "DATETIME2", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_NotComputed_2_Computed", "VARCHAR", "YES", "10");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_CASEDIFF", "INT", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_DateTimeOffset", "DateTimeOffset", "No", "4");
			AssertColumnDoesNotExistInMockMainDb("AlterTableIssues", "Col_Added");
			AssertColumnDoesNotExistInMockMainDb("AlterTableIssues", "Col_NewComputed1");
			AssertColumnDoesNotExistInMockMainDb("AlterTableIssues", "Col_NewComputed2");
			AssertComputedColumn("AlterTableIssues", "Col_Computed_2_NotComputed", "(SYSUTCDATETIME())", false);
			AssertComputedColumn("AlterTableIssues", "Col_NotComputed_2_Computed", null);
			AssertColumnNameInMockMainDbCaseSensitive("AlterTableIssues", "Col_CASEDIFF", expected: true);
			AssertColumnNameInMockMainDbCaseSensitive("AlterTableIssues", "Col_CaseDiff", expected: false);

			AssertColumnExistInMockMainDb("TestCharToChar", "TST_Col_1", "char", "NO", "10");
			AssertColumnExistInMockMainDb("TestCharToChar", "TST_Col_2", "char", "YES", "10");
			AssertColumnExistInMockMainDb("TestCharToChar", "TST_Col_3", "nvarchar", "YES", "-1");
			AssertColumnExistInMockMainDb("TestCharToChar", "TST_Col_4", "nchar", "YES", "10");

			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_VarcharMax_2_Xml", "VARCHAR", "YES", "-1");
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_Xml_Old", "XML", "YES", null);
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_XsdA_2_XsdB", "XML", "YES", null);
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_Untyped_2_XsdA", "XML", "NO", null);
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_XsdB_2_Untyped", "XML", "NO", null);
			AssertColumnDoesNotExistInMockMainDb("XmlTypeChanges", "Col_Xml_New");
			AssertColumnDoesNotExistInMockMainDb("XmlTypeChanges", "Col_XsdA_New");

			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_Xml_Old", null);
			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_XsdA_2_XsdB", "XsdA");
			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_Untyped_2_XsdA", null);
			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_XsdB_2_Untyped", "XsdB");

			AssertColumnDoesNotExistInMockMainDb("IdentityColumnAdded", "Col_Identity");
			AssertColumnExistInMockMainDb("IdentityColumnModified", "Col_IdentityChanged", "INT", "NO", null);
			AssertIdentityMockMainDbIdentityColumnSeedAndIncrement("IdentityColumnModified", "Col_IdentityChanged", 1, 1);
			AssertEquals("IdentityColumnModified row count", 2, GetRowCount("IdentityColumnModified"));
			AssertEquals("(Col_IdentityChanged = 1) row count", 1, GetRowCountWithGivenValue("IdentityColumnModified", "Col_IdentityChanged", "1"));
			AssertEquals("(Col_IdentityChanged = 2) row count", 1, GetRowCountWithGivenValue("IdentityColumnModified", "Col_IdentityChanged", "2"));

			// TestComputedColumns
			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_id", "INT", "YES", null);

			AssertColumnDoesNotExistInMockMainDb("TestComputedColumns", "CC_AddNew_Computed");
			AssertColumnDoesNotExistInMockMainDb("TestComputedColumns", "CC_AddNew_ComputedSource");

			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_DropOld_ComputedSource", "INT", "YES", null);
			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_DropOld_Computed", "INT", "YES", null);
			AssertComputedColumn("TestComputedColumns", "CC_DropOld_Computed", "(CHECKSUM([CC_DropOld_ComputedSource]))", false);

			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AlterSource_ComputedSource_1", "INT", "YES", null);
			AssertColumnDoesNotExistInMockMainDb("TestComputedColumns", "CC_AlterSource_ComputedSource_2");
			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AlterSource_Computed", "INT", "YES", null);
			AssertComputedColumn("TestComputedColumns", "CC_AlterSource_Computed", "(CHECKSUM([CC_id],[CC_AlterSource_ComputedSource_1]))", false);

			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AlterSourceType_ComputedSource", "CHAR", "NO", "1");
			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AlterSourceType_Computed", "INT", "YES", null);
			AssertComputedColumn("TestComputedColumns", "CC_AlterSourceType_Computed", "(CHECKSUM([CC_id],[CC_AlterSourceType_ComputedSource]))", false);

			// TestPreservedColumns
			AssertColumnExistInMockMainDb("TestPreservedColumns", "Col1", "INT", isNullable: "NO", length: null);
			AssertColumnExistInMockMainDb("TestPreservedColumns", "CW!!PreserveMe", "CHAR", isNullable: "NO", length: "3");
			AssertColumnExistInMockMainDb("TestPreservedColumns", "DropMe", "BIT", isNullable: "YES", length: null);
			AssertEquals("TestPreservedColumns row count", 2, GetRowCount("TestPreservedColumns"));
			AssertEquals("(CW!!PreserveMe = 'UNO') row count", 1, GetRowCountWithGivenValue("TestPreservedColumns", "[CW!!PreserveMe]", "'UNO'"));
			AssertEquals("(CW!!PreserveMe = 'DUE') row count", 1, GetRowCountWithGivenValue("TestPreservedColumns", "[CW!!PreserveMe]", "'DUE'"));

			// Data Pre-condition
			AssertEquals("AlterTableIssues row count", 2, GetRowCount("AlterTableIssues"));
			AssertEquals("(len(Col_Char_2_Varchar + '.') = 6) row count", 2,
				GetRowCountWithGivenValue("AlterTableIssues", "len(Col_Char_2_Varchar + '.')", "6"));
			AssertEquals("(len(Col_Char_2_Varchar + '.') = len(rtrim(Col_Char_2_Varchar) + '.')) row count", 0,
				GetRowCountWithGivenValue("AlterTableIssues", "len(Col_Char_2_Varchar + '.')", "len(rtrim(Col_Char_2_Varchar) + '.')"));

			AssertEquals("Pre-condition. TestCharToChar row count", 5, GetRowCount("TestCharToChar"));
			AssertEquals("Pre-condition. TestCharToChar TST_Col_1 empty count", 2, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_1", "''"));
			AssertEquals("Pre-condition. TestCharToChar TST_Col_1 value count", 3, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_1", "'12345'"));
			AssertEquals("Pre-condition. TestCharToChar TST_Col_2 null count", 2, GetRowCountWithNullValues("TestCharToChar", "TST_Col_2"));
			AssertEquals("Pre-condition. TestCharToChar TST_Col_2 value count", 3, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_2", "'O''Kiev'"));
			AssertEquals("Pre-condition. TestCharToChar TST_Col_3 null count", 4, GetRowCountWithNullValues("TestCharToChar", "TST_Col_3"));
			AssertEquals("Pre-condition. TestCharToChar TST_Col_3 value count", 1, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_3", "'12345'"));
			AssertEquals("Pre-condition. TestCharToChar TST_Col_4 null count", 4, GetRowCountWithNullValues("TestCharToChar", "TST_Col_4"));
			AssertEquals("Pre-condition. TestCharToChar TST_Col_4 value count", 1, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_4", "'12345'"));

			//INT->Money
			AssertColumnExistInMockMainDb("TestIntToMoney", "ColMax", "INT", "NO", null);
			AssertColumnExistInMockMainDb("TestIntToMoney", "ColMin", "INT", "NO", null);
			AssertColumnExistInMockMainDb("TestIntToMoney", "ColZero", "INT", "NO", null);
			AssertColumnExistInMockMainDb("TestIntToMoney", "Col1900", "INT", "NO", null);
			AssertEquals(1, GetRowCount("TestIntToMoney"));
			AssertEquals(1, GetRowCountWithGivenValue("TestIntToMoney", "ColMax", "2147483647"));
			AssertEquals(1, GetRowCountWithGivenValue("TestIntToMoney", "ColMin", "-2147483648"));
			AssertEquals(1, GetRowCountWithGivenValue("TestIntToMoney", "ColZero", "0"));
			AssertEquals(1, GetRowCountWithGivenValue("TestIntToMoney", "Col1900", "1900"));

			//Money->Decimal
			AssertColumnExistInMockMainDb("TestMoneyToDecimal", "ColMax", "MONEY", "NO", null);
			AssertColumnExistInMockMainDb("TestMoneyToDecimal", "ColMin", "MONEY", "NO", null);
			AssertColumnExistInMockMainDb("TestMoneyToDecimal", "ColZero", "MONEY", "NO", null);
			AssertColumnExistInMockMainDb("TestMoneyToDecimal", "Col6DP", "MONEY", "NO", null);
			AssertEquals(1, GetRowCount("TestMoneyToDecimal"));
			CombineAssertions(() =>
			{
				AssertEquals("ColMax", "99999999999999.9900", ((decimal)GetSingleRowValue("TestMoneyToDecimal", "ColMax")).ToString(CultureInfo.InvariantCulture));
				AssertEquals("ColMin", "-99999999999999.9900", ((decimal)GetSingleRowValue("TestMoneyToDecimal", "ColMin")).ToString(CultureInfo.InvariantCulture));
				AssertEquals("ColZero", 0m, (decimal)GetSingleRowValue("TestMoneyToDecimal", "ColZero"));
				AssertEquals("Col6DP", 1.1111m, (decimal)GetSingleRowValue("TestMoneyToDecimal", "Col6DP"));
			});

			//Date->DateTimeOffset
			AssertColumnExistInMockMainDb("TestDateToDateTimeOffset", "ColEffectiveDate", "DATE", "NO", null);
			AssertEquals(1, GetRowCount("TestDateToDateTimeOffset"));
			AssertEquals("TestDateToDateTimeOffset", "2021-10-25", ((DateTime)GetSingleRowValue("TestDateToDateTimeOffset", "ColEffectiveDate")).ToString("yyyy-MM-dd"));
		}

		void AssertColumnsSynchronised()
		{
			// Assert Schema
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0206_Uid_2_Int", "INT", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0220_Int_2_SmallInt", "SMALLINT", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0260_Char_2_Money", "MONEY", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0298_Datetime_2_SmallDatetime", "SMALLDATETIME", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err0515_Null_2_NotNull", "INT", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err4928_VarcharMax_2_NVarcharMax", "NVARCHAR", "YES", "-1");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err8115_BigInt_2_Int", "INT", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err8152_Char10_2_Char5", "CHAR", "YES", "5");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Err8169_Char1_2_Uid", "UNIQUEIDENTIFIER", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Added", "VARCHAR", "NO", "100");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Binary7_2_Binary3", "BINARY", "YES", "3");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Ntext_2_Nvarchar5", "NVARCHAR", "NO", "5");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Char_2_Bit", "BIT", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Char_2_Varchar", "VARCHAR", "YES", "5");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_Computed_2_NotComputed", "SMALLINT", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_NotComputed_2_Computed", "VARCHAR", "YES", "9");
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_NewComputed1", "INT", "NO", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_NewComputed2", "FLOAT", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_CaseDiff", "INT", "YES", null);
			AssertColumnExistInMockMainDb("AlterTableIssues", "Col_DateTimeOffset", "DateTimeOffset", "No", "4");
			AssertColumnDoesNotExistInMockMainDb("AlterTableIssues", "Col_ToBeDropped");
			AssertColumnDoesNotExistInMockMainDb("AlterTableIssues", "Col_ToBeDropped_WithStats");
			AssertComputedColumn("AlterTableIssues", "Col_Computed_2_NotComputed", null);
			AssertComputedColumn("AlterTableIssues", "Col_NotComputed_2_Computed", "(('**'+[Col_Char_2_Varchar])+'**')", false);
			AssertComputedColumn("AlterTableIssues", "Col_NewComputed1", "([Col_Err0220_Int_2_SmallInt]*(2))", true);
			AssertComputedColumn("AlterTableIssues", "Col_NewComputed2", "(rand())", false);
			AssertColumnNameInMockMainDbCaseSensitive("AlterTableIssues", "Col_CASEDIFF", expected: false);
			AssertColumnNameInMockMainDbCaseSensitive("AlterTableIssues", "Col_CaseDiff", expected: true);

			AssertColumnExistInMockMainDb("TestCharToChar", "TST_Col_1", "nchar", "YES", "10");
			AssertColumnExistInMockMainDb("TestCharToChar", "TST_Col_2", "varchar", "NO", "100");
			AssertColumnExistInMockMainDb("TestCharToChar", "TST_Col_3", "char", "YES", "3");
			AssertColumnExistInMockMainDb("TestCharToChar", "TST_Col_4", "nvarchar", "NO", "-1");

			// XmlTypeChanges table
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_VarcharMax_2_Xml", "XML", "YES", null);
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_Xml_New", "XML", "NO", null);
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_XsdA_2_XsdB", "XML", "YES", null);
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_Untyped_2_XsdA", "XML", "YES", null);
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_XsdB_2_Untyped", "XML", "NO", null);
			AssertColumnExistInMockMainDb("XmlTypeChanges", "Col_XsdA_New", "XML", "YES", null);
			AssertColumnDoesNotExistInMockMainDb("XmlTypeChanges", "Col_Xml_Old");
			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_VarcharMax_2_Xml", null);
			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_Xml_New", null);
			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_XsdA_2_XsdB", "XsdB");
			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_Untyped_2_XsdA", "XsdA");
			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_XsdB_2_Untyped", null);
			AssertColumnXmlSchemaInMockMainDb("XmlTypeChanges", "Col_XsdA_New", "XsdA");

			// Assert that the identity column changes
			AssertColumnExistInMockMainDb("IdentityColumnAdded", "Col_Identity", "INT", "NO", null);
			AssertIdentityMockMainDbIdentityColumnSeedAndIncrement("IdentityColumnAdded", "Col_Identity", 11, 3);
			AssertColumnExistInMockMainDb("IdentityColumnModified", "Col_IdentityChanged", "INT", "NO", null);
			AssertIdentityMockMainDbIdentityColumnSeedAndIncrement("IdentityColumnModified", "Col_IdentityChanged", 1, 5);
			AssertEquals("IdentityColumnModified row count", 2, GetRowCount("IdentityColumnModified"));
			AssertEquals("(Col_IdentityChanged = 1) row count", 1, GetRowCountWithGivenValue("IdentityColumnModified", "Col_IdentityChanged", "1"));
			AssertEquals("(Col_IdentityChanged = 6) row count", 1, GetRowCountWithGivenValue("IdentityColumnModified", "Col_IdentityChanged", "6"));

			// Assert TestComputedColumns
			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_id", "INT", "YES", null);

			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AddNew_ComputedSource", "INT", "YES", null);
			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AddNew_Computed", "INT", "YES", null);
			AssertComputedColumn("TestComputedColumns", "CC_AddNew_Computed", "(CHECKSUM([CC_AddNew_ComputedSource]))", false);

			AssertColumnDoesNotExistInMockMainDb("TestComputedColumns", "CC_DropOld_Computed");
			AssertColumnDoesNotExistInMockMainDb("TestComputedColumns", "CC_DropOld_ComputedSource");

			AssertColumnDoesNotExistInMockMainDb("TestComputedColumns", "CC_AlterSource_ComputedSource_1");
			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AlterSource_ComputedSource_2", "INT", "YES", null);
			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AlterSource_Computed", "INT", "YES", null);
			AssertComputedColumn("TestComputedColumns", "CC_AlterSource_Computed", "(CHECKSUM([CC_id],[CC_AlterSource_ComputedSource_2]))", false);

			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AlterSourceType_ComputedSource", "BIT", "NO", null);
			AssertColumnExistInMockMainDb("TestComputedColumns", "CC_AlterSourceType_Computed", "INT", "YES", null);
			AssertComputedColumn("TestComputedColumns", "CC_AlterSourceType_Computed", "(CHECKSUM([CC_id],[CC_AlterSourceType_ComputedSource]))", false);

			// TestPreservedColumns
			AssertColumnExistInMockMainDb("TestPreservedColumns", "Col1", "INT", isNullable: "NO", length: null);
			AssertColumnExistInMockMainDb("TestPreservedColumns", "CW!!PreserveMe", "CHAR", isNullable: "NO", length: "3");
			AssertColumnDoesNotExistInMockMainDb("TestPreservedColumns", "DropMe");
			AssertEquals("TestPreservedColumns row count", 2, GetRowCount("TestPreservedColumns"));
			AssertEquals("(CW!!PreserveMe = 'UNO') row count", 1, GetRowCountWithGivenValue("TestPreservedColumns", "[CW!!PreserveMe]", "'UNO'"));
			AssertEquals("(CW!!PreserveMe = 'DUE') row count", 1, GetRowCountWithGivenValue("TestPreservedColumns", "[CW!!PreserveMe]", "'DUE'"));

			// Assert Data
			AssertEquals("AlterTableIssues row count", 2, GetRowCount("AlterTableIssues"));
			AssertEquals("(Col_Err0206_Uid_2_Int is null) row count", 2, GetRowCountWithNullValues("AlterTableIssues", "Col_Err0206_Uid_2_Int"));
			AssertEquals("(Col_Err0220_Int_2_SmallInt = 20) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Err0220_Int_2_SmallInt", "20"));
			AssertEquals("(Col_Err0220_Int_2_SmallInt = 32767) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Err0220_Int_2_SmallInt", "32767"));
			AssertEquals("(Col_Err0260_Char_2_Money is null) row count", 2, GetRowCountWithNullValues("AlterTableIssues", "Col_Err0260_Char_2_Money"));
			AssertEquals("(Col_Err0298_Datetime_2_SmallDatetime is null) row count", 0, GetRowCountWithNullValues("AlterTableIssues", "Col_Err0298_Datetime_2_SmallDatetime"));
			AssertEquals("(Col_Err0515_Null_2_NotNull = 3) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Err0515_Null_2_NotNull", "3"));
			AssertEquals("(Col_Err0515_Null_2_NotNull = 50) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Err0515_Null_2_NotNull", "3"));
			AssertEquals("(Col_Err4928_VarcharMax_2_NVarcharMax is null) row count", 0, GetRowCountWithNullValues("AlterTableIssues", "Col_Err4928_VarcharMax_2_NVarcharMax"));
			AssertEquals("(Col_Err8115_BigInt_2_Int = 2147483647) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Err8115_BigInt_2_Int", "2147483647"));
			AssertEquals("(Col_Err8115_BigInt_2_Int = -2147483648) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Err8115_BigInt_2_Int", "-2147483648"));
			AssertEquals("(Col_Err8152_Char10_2_Char5 = '12345') row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Err8152_Char10_2_Char5", "'12345'"));
			AssertEquals("(Col_Err8152_Char10_2_Char5 = 'abcde') row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Err8152_Char10_2_Char5", "'abcde'"));
			AssertEquals("(Col_Err8169_Char1_2_Uid is null) row count", 2, GetRowCountWithNullValues("AlterTableIssues", "Col_Err8169_Char1_2_Uid"));
			AssertEquals("(Col_Added = 'AddedColumn') row count", 2, GetRowCountWithGivenValue("AlterTableIssues", "Col_Added", "'AddedColumn'"));
			AssertEquals("(Col_Int_2_SmallInt_Null = 30) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Int_2_SmallInt_Null", "30"));
			AssertEquals("(Col_Int_2_SmallInt_Null = 32767) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Int_2_SmallInt_Null", "32767"));
			AssertEquals("(Col_Datetime_2_SmallDatetime_Null = 1900-01-01) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Datetime_2_SmallDatetime_Null", "'1900-01-01'"));
			AssertEquals("(Col_Datetime_2_SmallDatetime_Null = 2079-01-01) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Datetime_2_SmallDatetime_Null", "'2079-01-01'"));
			AssertEquals("(Col_BigInt_2_Int_Null = -2147483648) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_BigInt_2_Int_Null", "-2147483648"));
			AssertEquals("(Col_BigInt_2_Int_Null = 2147483647) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_BigInt_2_Int_Null", "2147483647"));
			AssertEquals("(Col_Binary7_2_Binary3 = 0x123456) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Binary7_2_Binary3", "0x123456"));
			AssertEquals("(Col_Binary7_2_Binary3 = null) row count", 1, GetRowCountWithNullValues("AlterTableIssues", "Col_Binary7_2_Binary3"));
			AssertEquals("(Col_Ntext_2_Nvarchar5 = 'ABCDE') row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Ntext_2_Nvarchar5", "'ABCDE'"));
			AssertEquals("(Col_Ntext_2_Nvarchar5 = 'ABC') row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Ntext_2_Nvarchar5", "'ABC'"));
			AssertEquals("(Col_Char_2_Bit = 0) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Char_2_Bit", "0"));
			AssertEquals("(Col_Char_2_Bit = 1) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_Char_2_Bit", "1"));
			AssertEquals("(len(Col_Char_2_Varchar + '.') = 2) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "len(Col_Char_2_Varchar + '.')", "2"));
			AssertEquals("(len(Col_Char_2_Varchar + '.') = 3) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "len(Col_Char_2_Varchar + '.')", "3"));
			AssertEquals("(len(Col_Char_2_Varchar + '.') = len(rtrim(Col_Char_2_Varchar) + '.')) row count", 2,
				GetRowCountWithGivenValue("AlterTableIssues", "len(Col_Char_2_Varchar + '.')", "len(rtrim(Col_Char_2_Varchar) + '.')"));
			AssertEquals("(Col_Computed_2_NotComputed = 200) row count", 2, GetRowCountWithGivenValue("AlterTableIssues", "Col_Computed_2_NotComputed", "200"));
			AssertEquals("(Col_NotComputed_2_Computed = '**1**') row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_NotComputed_2_Computed", "'**1**'"));
			AssertEquals("(Col_NotComputed_2_Computed = '**22**') row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_NotComputed_2_Computed", "'**22**'"));
			AssertEquals("(Col_NewComputed1 = 40) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_NewComputed1", "40"));
			AssertEquals("(Col_NewComputed1 = 65534) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_NewComputed1", "65534"));
			AssertEquals("(Col_NewComputed2 is null) row count", 0, GetRowCountWithNullValues("AlterTableIssues", "Col_NewComputed2"));
			AssertEquals("(Col_CaseDiff = 99) row count", 2, GetRowCountWithGivenValue("AlterTableIssues", "Col_CaseDiff", "99"));
			AssertEquals("(Col_DateTimeOffset = 1800-01-01) row count", 1, GetRowCountWithGivenValue("AlterTableIssues", "Col_DateTimeOffset", "'1800-01-01'"));
			AssertEquals("XmlTypeChanges row count", 2, GetRowCount("XmlTypeChanges"));
			AssertEquals("(Col_VarcharMax_2_Xml is null) row count", 0, GetRowCountWithNullValues("XmlTypeChanges", "Col_VarcharMax_2_Xml"));
			AssertEquals("(Col_Xml_New  is null) row count", 0, GetRowCountWithNullValues("XmlTypeChanges", "Col_Xml_New"));
			AssertEquals("(Col_XsdA_2_XsdB is null) row count", 1, GetRowCountWithNullValues("XmlTypeChanges", "Col_XsdA_2_XsdB"));
			AssertEquals("(Col_XsdA_2_XsdB = '<e>2</e>') row count", 1,
				XmlSchemaSynchroniserForTesting.GetRowCountWithGivenXmlValue(TestConnection, mockMainDb, "XmlTypeChanges", "Col_XsdA_2_XsdB", "<e>2</e>"));
			AssertEquals("Col_Untyped_2_XsdA = '<e>11</e>'", 1,
				XmlSchemaSynchroniserForTesting.GetRowCountWithGivenXmlValue(TestConnection, mockMainDb, "XmlTypeChanges", "Col_Untyped_2_XsdA", "<e>11</e>"));
			AssertEquals("Col_Untyped_2_XsdA = '<e>22</e>'", 1,
				XmlSchemaSynchroniserForTesting.GetRowCountWithGivenXmlValue(TestConnection, mockMainDb, "XmlTypeChanges", "Col_Untyped_2_XsdA", "<e>22</e>"));
			AssertEquals("Col_XsdB_2_Untyped = '<e>111</e>'", 1,
				XmlSchemaSynchroniserForTesting.GetRowCountWithGivenXmlValue(TestConnection, mockMainDb, "XmlTypeChanges", "Col_XsdB_2_Untyped", "<e>111</e>"));
			AssertEquals("Col_XsdB_2_Untyped = '<e>BBB</e>'", 1,
				XmlSchemaSynchroniserForTesting.GetRowCountWithGivenXmlValue(TestConnection, mockMainDb, "XmlTypeChanges", "Col_XsdB_2_Untyped", "<e>BBB</e>"));
			AssertEquals("(Col_XsdA_New is null) row count", 2, GetRowCountWithNullValues("XmlTypeChanges", "Col_XsdA_New"));

			AssertEquals("Result. TestCharToChar row count", 5, GetRowCount("TestCharToChar"));
			AssertEquals("Result. TestCharToChar TST_Col_1 empty count", 2, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_1", "''"));
			AssertEquals("Result. TestCharToChar TST_Col_1 value count", 3, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_1", "'12345'"));
			AssertEquals("Result. TestCharToChar TST_Col_2 empty count", 2, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_2", "''"));
			AssertEquals("Result. TestCharToChar TST_Col_2 value count", 3, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_2", "'O''Kiev'"));
			AssertEquals("Result. TestCharToChar TST_Col_3 null count", 4, GetRowCountWithNullValues("TestCharToChar", "TST_Col_3"));
			AssertEquals("Result. TestCharToChar TST_Col_3 value count", 1, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_3", "'123'"));
			AssertEquals("Result. TestCharToChar TST_Col_4 empty count", 4, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_4", "''"));
			AssertEquals("Result. TestCharToChar TST_Col_4 value count", 1, GetRowCountWithGivenValue("TestCharToChar", "TST_Col_4", "'12345'"));

			//INT->Money
			AssertColumnExistInMockMainDb("TestIntToMoney", "ColMax", "MONEY", "NO", null);
			AssertColumnExistInMockMainDb("TestIntToMoney", "ColMin", "MONEY", "NO", null);
			AssertColumnExistInMockMainDb("TestIntToMoney", "ColZero", "MONEY", "NO", null);
			AssertColumnExistInMockMainDb("TestIntToMoney", "Col1900", "MONEY", "NO", null);
			AssertEquals(1, GetRowCount("TestIntToMoney"));
			AssertEquals(1, GetRowCountWithGivenValue("TestIntToMoney", "ColMax", "2147483647"));
			AssertEquals(1, GetRowCountWithGivenValue("TestIntToMoney", "ColMin", "-2147483648"));
			AssertEquals(1, GetRowCountWithGivenValue("TestIntToMoney", "ColZero", "0"));
			AssertEquals(1, GetRowCountWithGivenValue("TestIntToMoney", "Col1900", "1900"));

			//Money->Decimal
			AssertColumnExistInMockMainDb("TestMoneyToDecimal", "ColMax", "DECIMAL", "NO", null);
			AssertColumnExistInMockMainDb("TestMoneyToDecimal", "ColMin", "DECIMAL", "NO", null);
			AssertColumnExistInMockMainDb("TestMoneyToDecimal", "ColZero", "DECIMAL", "NO", null);
			AssertColumnExistInMockMainDb("TestMoneyToDecimal", "Col6DP", "DECIMAL", "NO", null);
			AssertEquals(1, GetRowCount("TestMoneyToDecimal"));
			CombineAssertions(() =>
			{
				AssertEquals("ColMax", "99999999999999.9900", ((decimal)GetSingleRowValue("TestMoneyToDecimal", "ColMax")).ToString(CultureInfo.InvariantCulture));
				AssertEquals("ColMin", "-99999999999999.9900", ((decimal)GetSingleRowValue("TestMoneyToDecimal", "ColMin")).ToString(CultureInfo.InvariantCulture));
				AssertEquals("ColZero", 0m, (decimal)GetSingleRowValue("TestMoneyToDecimal", "ColZero"));
				AssertEquals("Col6DP", 1.1111m, (decimal)GetSingleRowValue("TestMoneyToDecimal", "Col6DP"));
			});

			//Date->DateTimeOffset
			AssertColumnExistInMockMainDb("TestDateToDateTimeOffset", "ColEffectiveDate", "DATETIMEOFFSET", "NO", null);
			AssertEquals(1, GetRowCount("TestDateToDateTimeOffset"));
			AssertEquals("TestDateToDateTimeOffset", new DateTimeOffset(new DateTime(2021, 10, 25), new TimeSpan()), ((DateTimeOffset)GetSingleRowValue("TestDateToDateTimeOffset", "ColEffectiveDate")));

			AssertColumnExistInMockMainDb("TestDateTimeToDateTimeOffset", "ColSmallDateTime", "DATETIMEOFFSET", "NO", null);
			AssertColumnExistInMockMainDb("TestDateTimeToDateTimeOffset", "ColDateTime", "DATETIMEOFFSET", "NO", null);
			AssertColumnExistInMockMainDb("TestDateTimeToDateTimeOffset", "ColDateTime2", "DATETIMEOFFSET", "NO", null);
			AssertEquals(1, GetRowCount("TestDateTimeToDateTimeOffset"));
			AssertEquals("ColSmallDateTime", new DateTimeOffset(new DateTime(2021, 10, 25), TimeSpan.FromHours(1)), (DateTimeOffset)GetSingleRowValue("TestDateTimeToDateTimeOffset", "ColSmallDateTime"));
			AssertEquals("ColDateTime", new DateTimeOffset(new DateTime(2021, 10, 26), TimeSpan.FromHours(2)), (DateTimeOffset)GetSingleRowValue("TestDateTimeToDateTimeOffset", "ColDateTime"));
			AssertEquals("ColDateTime2", new DateTimeOffset(new DateTime(2021, 10, 27), TimeSpan.FromHours(3)), (DateTimeOffset)GetSingleRowValue("TestDateTimeToDateTimeOffset", "ColDateTime2"));

			AssertColumnExistInMockMainDb("TestDateTimeNoRename", "ColSmallDateTime_NoRename", "SMALLDATETIME", "NO", null);
			AssertColumnExistInMockMainDb("TestDateTimeNoRename", "ColDateTime_NoRename", "DATETIME", "NO", null);
			AssertColumnExistInMockMainDb("TestDateTimeNoRename", "ColDateTime2_NoRename", "DATETIME2", "NO", null);
			AssertEquals(1, GetRowCount("TestDateTimeNoRename"));
			AssertEquals("ColSmallDateTime", new DateTime(2021, 10, 25), (DateTime)GetSingleRowValue("TestDateTimeNoRename", "ColSmallDateTime_NoRename"));
			AssertEquals("ColDateTime", new DateTime(2021, 10, 26), (DateTime)GetSingleRowValue("TestDateTimeNoRename", "ColDateTime_NoRename"));
			AssertEquals("ColDateTime2", new DateTime(2021, 10, 27), (DateTime)GetSingleRowValue("TestDateTimeNoRename", "ColDateTime2_NoRename"));

			//Complete Online-Transformed Column Updates
			AssertColumnExistInMockMainDb("TestCompleteOnlineTransformedColumnUpdates", "Col1", "INT", "NO", null);
			AssertColumnExistInMockMainDb("TestCompleteOnlineTransformedColumnUpdates", "Col2", "VARCHAR", "YES", "3");
			AssertEquals(1, GetRowCount("TestCompleteOnlineTransformedColumnUpdates"));
			AssertEquals("Col1", 2, (int)GetSingleRowValue("TestCompleteOnlineTransformedColumnUpdates", "Col1"));
			AssertEquals("Col2", "DEF", (string)GetSingleRowValue("TestCompleteOnlineTransformedColumnUpdates", "Col2"));
		}

		void AssertTablesToRebuildOnlineAfterAlterAndDropColumns()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(mockMainDb))
			{
				var persister = GlobalServiceProvider.Instance.GetRequiredService<ITableRebuildPersisterFactory>().Get(TestConnection);
				if (TestConnection.ServerEdition == DbConnection.SqlServerEdition.EnterpriseDeveloper)
				{
					var expectedRebuildList = Array.Empty<string>();

					var list = new List<string>();
					foreach (var table in persister.GetTablesToRebuild())
					{
						list.Add(table.ToString());
						persister.MarkTableAsNotRequiringRebuild(table);
					}

					AssertContainsExactElementsInAnyOrder("TablesToRebuild", expectedRebuildList, list);
				}
				else
				{
					AssertEquals(0, persister.GetTablesToRebuild().Count());
				}
			}
		}

		#endregion

		#region Implementation

		protected override IAuxiliaryDbCreator GetMockMainDbCreator()
		{
			string[] createScripts = new string[]
			{
				ExtProperty.TableDefinition,

				createTestXmlSchemaScript,
				createTestMainDbObjectsScript,
				createTestMainDbUserStatsScript,
				createTestMainDbSchemaBoundView01,
				createTestMainDbSchemaBoundView02,
				createStmDataScript,
			};

			return new AuxiliaryDbCreatorForTesting(mockMainDb, createScripts);
		}

		protected override IAuxiliaryDbCreator GetMockTemplateDbCreator()
		{
			string[] createScripts = new string[] { createTestXmlSchemaScript, createTestTemplateDbObjectsScript };
			return new AuxiliaryDbCreatorForTesting(mockTemplateDb, createScripts);
		}

		void AssertColumnXmlSchemaInMockMainDb(string tableName, string columnName, string expectedXsd)
		{
			string columnXmlSchema = XmlSchemaSynchroniserForTesting.GetColumnXmlSchema(TestConnection, mockMainDb, tableName, columnName);
			AssertEquals(tableName + "." + columnName + " XML schema", expectedXsd, columnXmlSchema);
		}

		void AssertComputedColumn(string tableName, string columnName, string expectedComputedDefinition, bool expectedIsPersisted = false)
		{
			var sqlText = String.Format(@"
				SELECT count(*)
				FROM [{0}].sys.tables t
				INNER JOIN [{0}].sys.columns c ON t.object_id = c.object_id
				LEFT JOIN [{0}].sys.computed_columns cc ON c.object_id = cc.object_id AND c.column_id = cc.column_id
				WHERE t.name = '{1}'
				AND c.name = '{2}'
				AND {3}",
				mockMainDb, tableName, columnName,
				((expectedComputedDefinition == null) ?
					"c.is_computed = 0" :
					"cc.definition = '" + DataUtils.EscapeSingleQuotes(expectedComputedDefinition) + "' AND cc.is_persisted = " + (expectedIsPersisted ? "1" : "0")
				)
			);

			var columnExists = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText)) > 0;
			var computedDefinition = (expectedComputedDefinition == null)
					? "NOT Computed"
					: String.Format("Computed Definition: {0} / IsPersisted: {1}", expectedComputedDefinition, expectedIsPersisted.ToString());
			var assertMessage = String.Format("Column Exists? => {0}.{1} / {2}",
				tableName,         // 0
				columnName,        // 1
				computedDefinition // 2
				);
			AssertEquals(assertMessage, true, columnExists);
		}

		void AssertColumnNameInMockMainDbCaseSensitive(string tableName, string columnName, bool expected)
		{
			DbObjectCreatorTest.AssertColumnExistCaseSensitive(TestConnection, mockMainDb, Db.SqlDbOwnerSchema, tableName, columnName, expected);
		}

		int GetRowCount(string tableName)
		{
			string sqlText = String.Format("SELECT count(*) FROM [{0}]..[{1}]", mockMainDb, tableName);

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			return qtyRows;
		}

		int GetRowCountWithNullValues(string tableName, string columnName)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM [{0}]..[{1}]
				WHERE {2} is null",
				mockMainDb, tableName, columnName);

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			return qtyRows;
		}

		int GetRowCountWithGivenValue(string tableName, string columnName, string expectedValue)
		{
			string sqlText = String.Format(@"
				SELECT count(*) FROM [{0}]..[{1}]
				WHERE {2} = {3}",
				mockMainDb, tableName, columnName, expectedValue);

			int qtyRows = Convert.ToInt32(TestConnection.ExecuteScalar(sqlText));
			return qtyRows;
		}

		object GetSingleRowValue(string tableName, string columnName)
		{
			string sqlText = string.Format(
				@"SELECT top 1 {2} FROM [{0}]..[{1}]",
				mockMainDb, tableName, columnName);

			return TestConnection.ExecuteScalar(sqlText);
		}

		#region Scripts

		const string createTestXmlSchemaScript = @"
			CREATE XML SCHEMA COLLECTION XsdA AS '<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""e"" type=""xsd:byte"" /></xsd:schema>';
			CREATE XML SCHEMA COLLECTION XsdB AS '<xsd:schema xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><xsd:element name=""e"" type=""xsd:string"" /></xsd:schema>';";

		const string createTestMainDbObjectsScript = @"
			-- This table will have a number of changes to test alter table issues are handled
			CREATE TABLE AlterTableIssues
			( 
				Col_Err0206_Uid_2_Int UNIQUEIDENTIFIER NULL DEFAULT newid(),
				Col_Err0220_Int_2_SmallInt INT NULL,
				Col_Err0260_Char_2_Money CHAR(5) NULL,
				Col_Err0298_Datetime_2_SmallDatetime DATETIME NULL,
				Col_Err0515_Null_2_NotNull INT NULL,
				Col_Err4928_VarcharMax_2_NVarcharMax VARCHAR(MAX) NULL,
				Col_Err8115_BigInt_2_Int BIGINT NULL,
				Col_ToBeDropped INT NOT NULL DEFAULT 0,
				Col_Err8152_Char10_2_Char5 CHAR(10) NULL DEFAULT 'ABCDE',
				Col_ToBeDropped_WithStats INT NULL,
				Col_Err8169_Char1_2_Uid CHAR(1) NULL,
				Col_Int_2_SmallInt_Null INT NULL,
				Col_Datetime_2_SmallDatetime_Null DATETIME NULL,
				Col_BigInt_2_Int_Null BIGINT NULL,
				Col_Binary7_2_Binary3 BINARY(7) NULL,
				Col_Ntext_2_Nvarchar5 NTEXT NOT NULL,
				Col_Char_2_Bit CHAR(1) NULL DEFAULT 'Y',
				Col_Char_2_Varchar CHAR(5) NULL,
				Col_Computed_2_NotComputed AS (SYSUTCDATETIME()),
				Col_NotComputed_2_Computed VARCHAR(10) NULL,
				Col_CASEDIFF INT NULL,
                Col_DateTimeOffset datetimeoffset(4) Not NULL,
			)
			;
			ALTER TABLE AlterTableIssues
				ADD CONSTRAINT PK_AlterTableIssues PRIMARY KEY NONCLUSTERED (Col_ToBeDropped)
			;
			ALTER TABLE AlterTableIssues
				ADD CONSTRAINT CK_AlterTableIssues_Col_Err8152_Char10_2_Char5 CHECK (Col_Err8152_Char10_2_Char5 != '')
			;
			ALTER TABLE AlterTableIssues
				ADD CONSTRAINT CK_AlterTableIssues_Col_Err5074_MultiColConstraint CHECK (Col_ToBeDropped >= 0 AND Col_ToBeDropped_WithStats >= 0)
			;
			ALTER TABLE AlterTableIssues
				ADD CONSTRAINT CK_AlterTableIssues_Col_CASEDIFF CHECK (Col_CASEDIFF > 90)
			;
			CREATE NONCLUSTERED INDEX IX_AlterTableIssues_01 ON AlterTableIssues (Col_Err0260_Char_2_Money)
			;
			CREATE NONCLUSTERED INDEX IX_AlterTableIssues_02 ON AlterTableIssues (Col_Err0298_Datetime_2_SmallDatetime)
			;
			CREATE NONCLUSTERED INDEX IX_AlterTableIssues_03 ON AlterTableIssues (Col_Err0515_Null_2_NotNull)
			;

			-- Insert AlterTableIssues Data
			INSERT INTO AlterTableIssues VALUES
				(newid(), 20   , 'A3' , '1800-01-01', null, 'Text1', 9999999999 , 2, '1234567890', 0, 'A', 30   ,
				'1850-01-01', 9999999998 , 0x123456789ABCDE, N'ABCDEFG', 'Y' , '1' , 'Two'  , 99,'1800-01-01'),
				(newid(), 65535, '4.1', '2090-01-01', 3   , 'Text2', -2147483649, 1, 'abcdefghij', 0, 'B', 60000,
				'2085-01-01', -2147483650, null            , N'ABC'    , null, '22', 'Three', 99,'2090-01-01')
			;

			-- Table with an FK referencing a column which will be dropped
			CREATE TABLE EnsureDropChildFkConstraintsWorks
			(
				Col1 INT NULL,
			)
			;
			ALTER TABLE EnsureDropChildFkConstraintsWorks
				ADD CONSTRAINT FK_EnsureDropChildFkConstraintsWorks_TO_AlterTableIssues FOREIGN KEY (Col1)
				REFERENCES AlterTableIssues (Col_ToBeDropped)
			;

			-- This table will have a number of XML field changes
			CREATE TABLE XmlTypeChanges
			( 
				Col_Xml_Old XML NULL,
				Col_VarcharMax_2_Xml VARCHAR(MAX) NULL,
				Col_XsdA_2_XsdB XML(CONTENT XsdA) NULL,
				Col_Untyped_2_XsdA XML NOT NULL,
				Col_XsdB_2_Untyped XML(CONTENT XsdB) NOT NULL,
			)
			;

			-- Insert XmlTypeChanges Data
			INSERT INTO XmlTypeChanges VALUES ('old1', 'SingleElement'      , null      , '<e>11</e>', '<e>111</e>')
			INSERT INTO XmlTypeChanges VALUES ('old2', '<tag>whatever</tag>', '<e>2</e>', '<e>22</e>', '<e>BBB</e>')
			;

			-- This table will have a new IDENTITY column added
			CREATE TABLE IdentityColumnAdded
			( 
				Col_Char CHAR(1) NULL,
			)
			;

			-- This table will have its IDENTITY column modified
			CREATE TABLE IdentityColumnModified
			( 
				Col_IdentityChanged INT IDENTITY(1, 1) NOT NULL,
				Col_Char CHAR(1) NULL,
			)
			;

			-- Insert IdentityColumnModified Data
			INSERT INTO IdentityColumnModified DEFAULT VALUES
			INSERT INTO IdentityColumnModified DEFAULT VALUES
			;

			CREATE TABLE TestCharToChar
			(
				TST_PK               uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED,
				TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED,
				TST_Col_1            char(10)         NOT NULL,
				TST_Col_2            char(10)             NULL,
				TST_Col_3            nvarchar(max)        NULL,
				TST_Col_4            nchar(10)            NULL,
			)
			;
			INSERT TestCharToChar (TST_SystemCreateTimeUtc, TST_Col_1, TST_Col_2, TST_Col_3, TST_Col_4) VALUES
				(1, ''     , NULL     , NULL   , NULL   ),
				(2, ''     , NULL     , NULL   , NULL   ),
				(3, '12345', 'O''Kiev', NULL   , NULL   ),
				(4, '12345', 'O''Kiev', NULL   , NULL   ),
				(5, '12345', 'O''Kiev', '12345', '12345')
			;

			CREATE TABLE dbo.TestComputedColumns
			(
				CC_id int NULL,

				CC_DropOld_Computed AS (CHECKSUM(CC_DropOld_ComputedSource)),
				CC_DropOld_ComputedSource int NULL,

				CC_AlterSource_Computed AS (CHECKSUM(CC_id, CC_AlterSource_ComputedSource_1)),
				CC_AlterSource_ComputedSource_1 int NULL,

				CC_AlterSourceType_Computed AS (CHECKSUM(CC_id, CC_AlterSourceType_ComputedSource)),
				CC_AlterSourceType_ComputedSource char(1) NOT NULL DEFAULT 'Y',
			)
			;

			-- This table will have:
			--   * column [CW!!PreserveMe] preserved even though it's not in the new template
			--   * column [DropMe] dropped
			CREATE TABLE dbo.TestPreservedColumns
			(
				[Col1] int NOT NULL PRIMARY KEY NONCLUSTERED,
				[CW!!PreserveMe] char(3) NOT NULL,
				[DropMe] bit NULL,
			)
			;
			INSERT dbo.TestPreservedColumns ([Col1], [CW!!PreserveMe]) VALUES
				(1, 'UNO'),
				(2, 'DUE')
			;

			CREATE TABLE TestIntToMoney
			(
				ColMax INT NOT NULL,
				ColMin INT NOT NULL,
				ColZero INT NOT NULL,
				Col1900 INT NOT NULL
			)
			;
			INSERT TestIntToMoney (ColMax, ColMin, ColZero, Col1900) VALUES (2147483647, -2147483648, 0, 1900);

			CREATE TABLE TestMoneyToDecimal
			(
				ColMax MONEY NOT NULL,
				ColMin MONEY NOT NULL,
				ColZero MONEY NOT NULL,
				Col6DP MONEY NOT NULL
			)
			;
			INSERT TestMoneyToDecimal (ColMax, ColMin, ColZero, Col6DP) VALUES (99999999999999.99, -99999999999999.99, 0, 1.1111);

			CREATE TABLE TestDateToDateTimeOffset
			(
				ColEffectiveDate DATE NOT NULL
			)
			;
			INSERT TestDateToDateTimeOffset (ColEffectiveDate) VALUES ('2021-10-25');

			-- This table will have temp columns renamed, as the new template is for DateTimeOffset
			CREATE TABLE TestDateTimeToDateTimeOffset
			(
				ColSmallDateTime SMALLDATETIME NOT NULL,
				_DTO_ColSmallDateTime DATETIMEOFFSET NOT NULL CONSTRAINT TestSmallDateTime DEFAULT ('1994-01-01'),
				ColDateTime DATETIME NOT NULL,
				_DTO_ColDateTime DATETIMEOFFSET NOT NULL CONSTRAINT TestDateTime DEFAULT ('1994-01-01'),
				ColDateTime2 DATETIME2 NOT NULL,
				_DTO_ColDateTime2 DATETIMEOFFSET NOT NULL CONSTRAINT TestDateTime2 DEFAULT ('1994-01-01')
			)

			INSERT TestDateTimeToDateTimeOffset (ColSmallDateTime, _DTO_ColSmallDateTime, ColDateTime, _DTO_ColDateTime, ColDateTime2, _DTO_ColDateTime2)
			VALUES ('2021-10-25', '2021-10-25 00:00:00 +01:00', '2021-10-26', '2021-10-26 00:00:00 +02:00', '2021-10-27', '2021-10-27 00:00:00 +03:00');

			-- This table will have no columns renamed, as the new template is not for DateTimeOffset
			CREATE TABLE TestDateTimeNoRename
			(
				ColSomePK UNIQUEIDENTIFIER NOT NULL,
				ColSmallDateTime_NoRename SMALLDATETIME NOT NULL,
				_DTO_ColSmallDateTime_NoRename DATETIMEOFFSET NOT NULL CONSTRAINT TestSmallDateTime_NoRename DEFAULT ('1994-01-01'),
				ColDateTime_NoRename DATETIME NOT NULL,
				_DTO_ColDateTime_NoRename DATETIMEOFFSET NOT NULL CONSTRAINT TestDateTime_NoRename DEFAULT ('1994-01-01'),
				ColDateTime2_NoRename DATETIME2 NOT NULL,
				_DTO_ColDateTime2_NoRename DATETIMEOFFSET NOT NULL CONSTRAINT TestDateTime2_NoRename DEFAULT ('1994-01-01')
			)

			INSERT TestDateTimeNoRename (ColSomePK, ColSmallDateTime_NoRename, _DTO_ColSmallDateTime_NoRename, ColDateTime_NoRename, _DTO_ColDateTime_NoRename, ColDateTime2_NoRename, _DTO_ColDateTime2_NoRename)
			VALUES (NEWID(), '2021-10-25', '2021-10-25 00:00:00 +01:00', '2021-10-26', '2021-10-26 00:00:00 +02:00', '2021-10-27', '2021-10-27 00:00:00 +03:00');

			CREATE TABLE TestCompleteOnlineTransformedColumnUpdates
			(
				Col1 INT NOT NULL,
				_R_Col1 INT NOT NULL CONSTRAINT Test_R_Col1 DEFAULT (10),
				Col2 VARCHAR(3) NOT NULL,
				_R_Col2 VARCHAR(3) CONSTRAINT Test_R_Col2 DEFAULT ('XYZ'),
			)

			INSERT TestCompleteOnlineTransformedColumnUpdates (Col1, _R_Col1, Col2, _R_Col2)
			VALUES (1, 2, 'ABC', 'DEF');
			";

		const string createTestMainDbUserStatsScript = @"
			CREATE STATISTICS STAT_AlterTableIssues_01 ON AlterTableIssues (Col_ToBeDropped_WithStats)
			;
			";

		const string createTestMainDbSchemaBoundView01 = "CREATE VIEW vw_AlterTableIssues01 WITH SCHEMABINDING AS SELECT Col_ToBeDropped FROM dbo.AlterTableIssues;";
		const string createTestMainDbSchemaBoundView02 = "CREATE VIEW vw_AlterTableIssues02 WITH SCHEMABINDING AS SELECT Col_Char_2_Bit FROM dbo.AlterTableIssues;";

		const string createTestTemplateDbObjectsScript = @"
			-- This table had a number of changes to test alter table issues are handled
			CREATE TABLE AlterTableIssues
			( 
				Col_Added VARCHAR(100) NOT NULL DEFAULT 'AddedColumn',
				Col_Err0206_Uid_2_Int INT NULL DEFAULT 6,
				Col_Err0220_Int_2_SmallInt SMALLINT NOT NULL,
				Col_Err0260_Char_2_Money MONEY NULL,
				Col_Err0298_Datetime_2_SmallDatetime SMALLDATETIME NOT NULL,
				Col_Err0515_Null_2_NotNull INT NOT NULL DEFAULT 50,
				Col_Err4928_VarcharMax_2_NVarcharMax NVARCHAR(MAX) NULL,
				Col_Err8115_BigInt_2_Int INT NOT NULL,
				Col_Err8152_Char10_2_Char5 CHAR(5) NULL DEFAULT 'ABCDE',
				Col_Err8169_Char1_2_Uid UNIQUEIDENTIFIER NULL,
				Col_Int_2_SmallInt_Null SMALLINT NULL,
				Col_Datetime_2_SmallDatetime_Null SMALLDATETIME NULL,
				Col_BigInt_2_Int_Null INT NULL,
				Col_Binary7_2_Binary3 BINARY(3) NULL,
				Col_Ntext_2_Nvarchar5 NVARCHAR(5) NOT NULL,
				Col_Char_2_Bit BIT NOT NULL,
				Col_Char_2_Varchar VARCHAR(5) NULL,
				Col_Computed_2_NotComputed SMALLINT NOT NULL DEFAULT 200,
				Col_NotComputed_2_Computed AS ('**' + Col_Char_2_Varchar + '**'),
				Col_NewComputed1 AS (Col_Err0220_Int_2_SmallInt * 2) PERSISTED NOT NULL,
				Col_NewComputed2 AS (rand()),
				Col_CaseDiff INT NULL,
                Col_DateTimeOffset datetimeoffset(4) Not NULL,
			)
			;
			ALTER TABLE AlterTableIssues
				ADD CONSTRAINT CK_AlterTableIssues_Col_Err8152_Char10_2_Char5 CHECK (Col_Err8152_Char10_2_Char5 != '')
			;
			CREATE NONCLUSTERED INDEX IX_AlterTableIssues_01 ON AlterTableIssues (Col_Err0260_Char_2_Money)
			;
			CREATE NONCLUSTERED INDEX IX_AlterTableIssues_02 ON AlterTableIssues (Col_Err0298_Datetime_2_SmallDatetime)
			;
			CREATE NONCLUSTERED INDEX IX_AlterTableIssues_03 ON AlterTableIssues (Col_Err0515_Null_2_NotNull)
			;

			-- This table had a number of XML field changes
			CREATE TABLE XmlTypeChanges
			( 
				Col_Xml_New XML NOT NULL DEFAULT '',
				Col_VarcharMax_2_Xml XML NULL,
				Col_XsdA_2_XsdB XML(CONTENT XsdB) NULL,
				Col_Untyped_2_XsdA XML(CONTENT XsdA) NULL,
				Col_XsdB_2_Untyped XML NOT NULL,
				Col_XsdA_New XML(CONTENT XsdA) NULL,
			)
			;

			-- This table had a new IDENTITY column added
			CREATE TABLE IdentityColumnAdded
			( 
				Col_Identity INT IDENTITY(11, 3) NOT NULL,
				Col_Char CHAR(1) NULL,
			)
			;

			-- This table had its IDENTITY column modified
			CREATE TABLE IdentityColumnModified
			( 
				Col_IdentityChanged INT IDENTITY(1, 5) NOT NULL,
				Col_Char CHAR(1) NULL,
			)
			;

			CREATE TABLE TestCharToChar
			(
				TST_PK                  uniqueidentifier NOT NULL DEFAULT (NEWID()) PRIMARY KEY NONCLUSTERED,
				TST_SystemCreateTimeUtc int              NOT NULL UNIQUE CLUSTERED,
				TST_Col_1               nchar(10)            NULL,
				TST_Col_2               varchar(100)     NOT NULL CONSTRAINT DF_TST_Col_2 DEFAULT (''),
				TST_Col_3               char(3)              NULL,
				TST_Col_4               nvarchar(max)    NOT NULL CONSTRAINT DF_TST_Col_4 DEFAULT (''),
			)
			;

			CREATE TABLE TestComputedColumns
			(
				CC_id int NULL,

				CC_AddNew_Computed AS (CHECKSUM(CC_AddNew_ComputedSource)),
				CC_AddNew_ComputedSource int NULL,

				CC_AlterSource_Computed AS (CHECKSUM(CC_id, CC_AlterSource_ComputedSource_2)),
				CC_AlterSource_ComputedSource_2 int NULL,

				CC_AlterSourceType_Computed AS (CHECKSUM(CC_id, CC_AlterSourceType_ComputedSource)),
				CC_AlterSourceType_ComputedSource bit NOT NULL DEFAULT 1,
			)
			;

			-- This table had:
			--   * column [CW!!PreserveMe] preserved even though it's not here in the new template
			--   * column [DropMe] dropped
			CREATE TABLE dbo.TestPreservedColumns
			(
				[Col1] int NOT NULL PRIMARY KEY NONCLUSTERED,
			)
			;

			CREATE TABLE TestIntToMoney
			(
				ColMax MONEY NOT NULL,
				ColMin MONEY NOT NULL,
				ColZero MONEY NOT NULL,
				Col1900 MONEY NOT NULL
			)
			;

			CREATE TABLE TestMoneyToDecimal
			(
				ColMax decimal(18,4) NOT NULL,
				ColMin decimal(18,4) NOT NULL,
				ColZero decimal(5,2) NOT NULL,
				Col6DP decimal(18,6) NOT NULL
			);

			CREATE TABLE TestDateToDateTimeOffset
			(
				ColEffectiveDate DATETIMEOFFSET(0) NOT NULL
			);

			-- This table will have temp columns renamed, as the new template is for DateTimeOffset
			CREATE TABLE TestDateTimeToDateTimeOffset
			(
				ColSmallDateTime DATETIMEOFFSET NOT NULL,
				ColDateTime DATETIMEOFFSET NOT NULL,
				ColDateTime2 DATETIMEOFFSET NOT NULL
			)

			CREATE TABLE TestDateTimeNoRename
			(
				ColSomePK UNIQUEIDENTIFIER NOT NULL,
				ColSmallDateTime_NoRename SMALLDATETIME NOT NULL,
				ColDateTime_NoRename DATETIME NOT NULL,
				ColDateTime2_NoRename DATETIME2 NOT NULL
			)

			";

		const string createStmDataScript = @"
			CREATE TABLE dbo.StmData
			( 
					SD_PK UNIQUEIDENTIFIER NOT NULL DEFAULT newid(),
					SD_Name VARCHAR(300) NOT NULL,
					SD_Owner UNIQUEIDENTIFIER NULL,
					SD_DepartmentGuid UNIQUEIDENTIFIER NULL,
					SD_Type CHAR(3) NOT NULL,
					SD_IsLogged CHAR(1) NULL DEFAULT ('N'),
					SD_BinaryValue VARBINARY(max) NULL,
					SD_GuidValue UNIQUEIDENTIFIER NULL,
					SD_PreserveTestValue BIT NULL,
			)
			;
			ALTER TABLE dbo.StmData
				ADD CONSTRAINT PK_StmData PRIMARY KEY NONCLUSTERED (SD_PK)
			;";

		#endregion

		#endregion
	}
}
