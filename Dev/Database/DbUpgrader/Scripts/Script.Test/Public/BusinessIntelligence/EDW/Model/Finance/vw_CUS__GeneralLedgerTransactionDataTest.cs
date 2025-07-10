using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Model.Finance;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Model.Finance.Testing
{
	[TestedType(typeof(vw_CUS__GeneralLedgerTransactionData))]
	internal class vw_CUS__GeneralLedgerTransactionDataTest : BiCreateScriptTest
	{
		public void TestColumns()
		{
			var columns = GetColumns();
			var result = SelectRows();
			foreach (var column in columns)
			{
				Assert($"{column} should be contained", result.Columns.Contains(column));
			}
			AssertEquals(columns.Count, result.Columns.Count);
		}

		public void TestViewData()
		{
			var result = SelectRows("vw_CUS__GeneralLedgerTransactionData");
			AssertInitialData(result);
		}

		public void TestIniLoad()
		{
			var result = SelectRows();
			AssertInitialData(result);
		}

		void AssertInitialData(DataTable result)
		{
			AssertEquals("Rowcount", 7, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("DueDate = '2023-08-10' and GLAmountLocalCredit = 100 and GLAmountLocalDebit = 0 and GLAmountOSCredit = 100 and GLAmountOSDebit = 0 and LedgerCode = 'AR' and LineDescription = 'LineDesc1' and TransactionDate = '2023-08-10' and TransactionNum = '012345' and TransactionType = 'INV' and JournalEntriesNumber = '100000'").Length);
				AssertEquals(1, result.Select("DueDate = '2023-08-10' and GLAmountLocalCredit = 200 and GLAmountLocalDebit = 0 and GLAmountOSCredit = 200 and GLAmountOSDebit = 0 and LedgerCode = 'AR' and LineDescription = 'LineDesc2' and TransactionDate = '2023-08-10' and TransactionNum = '012345' and TransactionType = 'INV' and JournalEntriesNumber = '100001'").Length);
				AssertEquals(1, result.Select("DueDate = '2023-08-08' and GLAmountLocalCredit = 300 and GLAmountLocalDebit = 0 and GLAmountOSCredit = 300 and GLAmountOSDebit = 0 and LedgerCode = 'AP' and LineDescription = 'DUU AP FIN Invoice 01234567' and TransactionDate = '2023-08-09' and TransactionNum = '0123456' and TransactionType = 'CRD' and TransactionLineID IS NULL and TransactionHeaderID = '61E7F2A2-979E-482B-9648-E09FDF0B0D12' and JournalEntriesNumber = '100002'").Length);
				AssertEquals(1, result.Select("DueDate = '2023-08-13' and GLAmountLocalCredit = 400 and GLAmountLocalDebit = 0 and GLAmountOSCredit = 400 and GLAmountOSDebit = 0 and LedgerCode = 'AP' and LineDescription = 'LineDesc3' and TransactionDate = '2023-08-13' and TransactionNum = '012345678' and TransactionType = 'PAY' and TransactionLineID = '83055969-C125-439E-B709-F91F60CEA6BD' and TransactionHeaderID = '576A6CBA-500E-4DC6-8BEB-ABFD3C925CE7' and JournalEntriesNumber = '100003'").Length);
				var attributeIsNotEmptyRow = result.Select("GLAmountLocalCredit = 500 and GLAmountLocalDebit = 0 and GLAmountOSCredit = 500 and GLAmountOSDebit = 0 and LedgerCode = 'JC' and LineDescription = 'LineDesc4' and TransactionDate = '2023-08-12' and TransactionNum = '' and TransactionType = 'ACR' and JournalEntriesNumber = '100004'");
				AssertEquals(1, attributeIsNotEmptyRow.Length);
				Assert(attributeIsNotEmptyRow[0].IsNull("DueDate"));
				AssertEquals(1, result.Select("DueDate = '2023-08-12' and GLAmountLocalCredit = 600 and GLAmountLocalDebit = 0 and GLAmountOSCredit = 600 and GLAmountOSDebit = 0 and LedgerCode = 'CB' and LineDescription = 'LineDesc5' and TransactionDate = '2023-08-13' and TransactionNum = '236232' and TransactionType = 'DRC' and JournalEntriesNumber = '100005'").Length);
				AssertEquals(1, result.Select("DueDate = '2023-08-14' and GLAmountLocalCredit = 600 and GLAmountLocalDebit = 0 and GLAmountOSCredit = 600 and GLAmountOSDebit = 0 and LedgerCode = 'CB' and LineDescription = 'LineDesc6' and TransactionDate = '2023-08-14' and TransactionNum = '236232' and TransactionType = 'DRC' and JournalEntriesNumber = '100006'").Length);
			});
		}

		[ExpectNoExceptions]
		public void TestIncLoadForChargeCodeTable()
		{
			var result = SelectRows();
			CombineAssertions(() =>
			{
				AssertEquals(3, result.Select("ChargeCodeKey = 1 AND GoodsServiceType = 'SRV'").Length);
				AssertEquals(3, result.Select("ChargeCodeKey = 2 AND GoodsServiceType = 'SRV'").Length);
				AssertEquals(0, result.Select("ChargeCodeKey = 3 AND GoodsServiceType = 'SRV'").Length);
			});

			PrepareChargeCodeTestDataForIncLoad();
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 8, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(3, result.Select("ChargeCodeKey = 1 AND GoodsServiceType = 'SRV'").Length);
				AssertEquals(3, result.Select("ChargeCodeKey = 2 AND GoodsServiceType = 'GDS'").Length);
				AssertEquals(1, result.Select("ChargeCodeKey = 3 AND GoodsServiceType = 'GDS'").Length);
			});
			var transformedRows = SelectTransformedRows();
			AssertEquals(7, transformedRows.Select("KeyValue IN (5, 6, 7, 8, 9, 10, 11)").Length);
		}

		[ExpectNoExceptions]
		public void TestIncLoadForCompanyTable()
		{
			var result = SelectRows();
			CombineAssertions(() =>
			{
				AssertEquals(7, result.Select("CompanyKey = 1 AND CountryCode = 'AU' AND LocalCurrency = 'AUD'").Length);
				AssertEquals(0, result.Select("CompanyKey = 2 AND CountryCode = 'CN' AND LocalCurrency = 'CNY'").Length);
			});

			PrepareCompanyTestDataForIncLoad();
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 8, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(7, result.Select("CompanyKey = 1 AND CountryCode = 'US' AND LocalCurrency = 'USD'").Length);
				AssertEquals(1, result.Select("CompanyKey = 2 AND CountryCode = 'CN' AND LocalCurrency = 'CNY'").Length);
			});
			var transformedRows = SelectTransformedRows();
			AssertEquals(15, transformedRows.Select("KeyValue IN (1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15)").Length);
		}

		[ExpectNoExceptions]
		public void TestIncLoadForGLAccountTable()
		{
			var result = SelectRows();
			CombineAssertions(() =>
			{
				AssertEquals(6, result.Select("GLAccountKey = 1 AND GLAccountNum = '6556.45.13' AND GLAccountName = 'TestA'").Length);
				AssertEquals(1, result.Select("GLAccountKey = 2 AND GLAccountNum = '3532.24.32' AND GLAccountName = 'TestB'").Length);
				AssertEquals(0, result.Select("GLAccountKey = 3 AND GLAccountNum = '4455.45.54' AND GLAccountName = 'TestC'").Length);
			});

			PrepareGLAccountTestDataForIncLoad();
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 9, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(6, result.Select("GLAccountKey = 1 AND GLAccountNum = '6556.45.13' AND GLAccountName = 'Update Desc'").Length);
				AssertEquals(2, result.Select("GLAccountKey = 2 AND GLAccountNum = '3532.24.32' AND GLAccountName = 'TestB'").Length);
				AssertEquals(1, result.Select("GLAccountKey = 3 AND GLAccountNum = '4455.45.54' AND GLAccountName = 'TestC'").Length);
			});
			var transformedRows = SelectTransformedRows();
			AssertEquals(14, transformedRows.Select("KeyValue IN (1, 2, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14)").Length);
		}

		[ExpectNoExceptions]
		public void TestIncLoadForBASGeneralLedgerDataTable()
		{
			var result = SelectRows();
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("GLAmountLocalCredit = 100 AND GLAmountLocalDebit = 0").Length);
				AssertEquals(0, result.Select("GLAmountLocalCredit = 111 AND GLAmountLocalDebit = 0").Length);
			});

			PrepareBASGeneralLedgerDataTestDataForIncLoad();
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 8, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(0, result.Select("GLAmountLocalCredit = 100 AND GLAmountLocalDebit = 0").Length);
				AssertEquals(1, result.Select("GLAmountLocalCredit = 111 AND GLAmountLocalDebit = 0").Length);
				AssertEquals(1, result.Select("GLAmountLocalCredit = 222 AND GLAmountLocalDebit = 0").Length);
				AssertEquals(1, result.Select("TransactionHeaderID = '2EAD1186-B082-40F0-81A4-4C7D4D5BC18C' AND TransactionLineID = '61E7F2A2-979E-482B-9648-E09FDF0B0D12'").Length);
			});
			var transformedRows = SelectTransformedRows();
			AssertEquals(3, transformedRows.Select("KeyValue IN (1, 8)").Length);
		}

		[ExpectNoExceptions]
		public void TestIncLoadForTransactionHeaderTable()
		{
			var result = SelectRows();
			CombineAssertions(() =>
			{
				AssertEquals(2, result.Select("InvoiceAmount = 200").Length);
				AssertEquals(0, result.Select("InvoiceAmount = 400").Length);
				AssertEquals(0, result.Select("InvoiceAmount = 222").Length);
			});

			PrepareTransactionHeaderTestDataForIncLoad();
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 8, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(0, result.Select("InvoiceAmount = 200").Length);
				AssertEquals(1, result.Select("InvoiceAmount = 400").Length);
				AssertEquals(2, result.Select("InvoiceAmount = 222").Length);
			});
			var transformedRows = SelectTransformedRows();
			AssertEquals(5, transformedRows.Select("KeyValue IN (1, 2, 8, 9, 10)").Length);
		}

		[ExpectNoExceptions]
		public void TestIncLoadForTransactionLineTable()
		{
			var result = SelectRows();
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("LineAmount = 100").Length);
				AssertEquals(0, result.Select("LineAmount = 222").Length);
			});

			PrepareTransactionLineTestDataForIncLoad();
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 8, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(0, result.Select("LineAmount = 100").Length);
				AssertEquals(1, result.Select("LineAmount = 400").Length);
				AssertEquals(1, result.Select("LineAmount = 222").Length);
			});
			var transformedRows = SelectTransformedRows();
			AssertEquals(3, transformedRows.Select("KeyValue IN (1, 8, 9)").Length);
		}

		[ExpectNoExceptions]
		public void TestIncLoadForTaxGLMovementInfo()
		{
			var result = SelectRows();
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("TaxGLMovementKey = 2").Length);
			});

			PrepareTaxGLMovementInfo();
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();
			AssertEquals("Rowcount", 8, result.Rows.Count);
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("TaxGLMovementKey = 2 AND TransactionDate = '2023-08-09 00:00:00'").Length);
				AssertEquals(1, result.Select("TaxGLMovementKey = 3 AND HeaderDescription = '1' AND LocalCurrency = 'CNY' AND SubUnitRatio = 100 ").Length);
			});
			var transformedRows = SelectTransformedRows();
			AssertEquals(3, transformedRows.Select("KeyValue IN (3, 8)").Length);
		}

		[ExpectNoExceptions]
		public void TestIncLoadForJournalEntriesNumber()
		{
			var result = SelectRows();
			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("JournalEntriesNumber = '100000'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100001'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100002'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100003'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100004'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100006'").Length);
				AssertEquals(0, result.Select("JournalEntriesNumber = '200000'").Length);
			});

			PrepareTestDataForIncLoad();
			ExecuteCusTableLoad("IncrementalLoadQuery");
			result = SelectRows();

			CombineAssertions(() =>
			{
				AssertEquals(1, result.Select("JournalEntriesNumber = '100000'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100001'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100002'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100003'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100004'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '100006'").Length);
				AssertEquals(1, result.Select("JournalEntriesNumber = '200000'").Length);
			});
		}

		void PrepareTaxGLMovementInfo()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO [{0}].[Organization].[BAS__Company] (CompanyKey, CompanyID, CountryCode, LocalCurrency) VALUES (2, NEWID(), 'CN', 'CNY')

				INSERT [{0}].[Finance].[BAS__GeneralLedgerData]
					([AccGLTransactionLineKey], [BranchKey], [CompanyKey], [DepartmentKey],
					[GeneralLedgerDataID], [GeneralLedgerDataKey],
					[GLAccountKey], [Currency], [ExchangeRate], [GLAccountType],
					[LocalCreditAmount], [LocalDebitAmount], [OSCreditAmount], [OSDebitAmount], [PostDate], [PostPeriod],
					[SystemCreateTimeUtc], [SystemCreateUser], [SystemLastEditTimeUtc], [SystemLastEditUser],
					[Type], [GLTransactionHeaderKey], [TaxBranchKey], [TaxGLMovementKey])
					VALUES
						(null, 3, 2, 8, newid(), 8, 2, 'AUD', 1, 'MCA', 300.00, 0, 300.00, 0, '2023-08-12', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'TGM', null, 1, 3)

				INSERT [{0}].[Finance].[CUS__TaxGLMovementInfo]
					([TaxGLMovementKey],[DueDateTime],[LedgerCode], [TaxHeaderDescription], [InvoiceDateTime], [GLTransactionHeaderID], [TransactionTypeCode], [TaxLineDescription], [GLTransactionHeaderKey], [TransactionNo])
					VALUES
					(3, '2023-08-10 00:00:00', 'AP', '1', '2023-08-10 00:00:00', '381D8F36-6405-4797-BABB-4CB4DCBB01CE', 'INV', 'DUU AP FIN Invoice 012345', 8, '012345')

				UPDATE [{0}].[Finance].[BAS__TaxTransaction] SET TaxSystemCode = 'DAU' WHERE TaxTransactionKey = 1
				UPDATE [{0}].[Finance].[BAS__TaxGLMovement] SET Amount = 100 WHERE TaxGLMovementKey = 2
				UPDATE [{0}].[Finance].[BAS__GLTransactionHeader] SET LocalAmount = 100 WHERE GLTransactionHeaderKey = 2
				UPDATE [{0}].[Finance].[BAS__GLTransactionHeader] SET LocalAmount = 100 WHERE GLTransactionHeaderKey = 3

				INSERT INTO[{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, PkValue)
				VALUES
					('Finance', 'CUS__TaxGLMovementInfo', 1, NULL),
					('Finance', 'CUS__TaxGLMovementInfo', 2, NEWID()),
					('Finance', 'CUS__TaxGLMovementInfo', 3, NULL),
					('Finance', 'BAS__GeneralLedgerData', 8, NULL)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void PrepareTransactionLineTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Finance].[BAS__AccGLTransactionLine] SET LineAmount = 222 WHERE AccGLTransactionLineKey = 1

				INSERT INTO[{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, PkValue)
				VALUES ('Finance', 'BAS__AccGLTransactionLine', 1, NULL), ('Finance', 'BAS__AccGLTransactionLine', 1, NEWID()), ('Finance', 'BAS__AccGLTransactionLine', 8, NULL)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			PrepareTestDataForIncLoad();
		}

		void PrepareTransactionHeaderTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Finance].[BAS__GLTransactionHeader] SET LocalAmount = 222 WHERE GLTransactionHeaderKey = 1

				INSERT INTO[{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, PkValue)
				VALUES ('Finance', 'BAS__GLTransactionHeader', 1, NULL), ('Finance', 'BAS__GLTransactionHeader', 1, NEWID()), ('Finance', 'BAS__GLTransactionHeader', 8, NULL)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			PrepareTestDataForIncLoad();
		}

		void PrepareBASGeneralLedgerDataTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Finance].[BAS__GeneralLedgerData] SET LocalCreditAmount = 222, LocalDebitAmount = 0 WHERE GeneralLedgerDataKey = 1

				INSERT INTO[{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, PkValue)
				VALUES 
					('Finance', 'BAS__GeneralLedgerData', 1, NULL), 
					('Finance', 'BAS__GeneralLedgerData', 1, NEWID()), 
					('Finance', 'BAS__GeneralLedgerData', 8, NULL)
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			PrepareTestDataForIncLoad();
		}

		void PrepareGLAccountTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT [{0}].[Finance].[BAS__GeneralLedgerData]
				([AccGLTransactionLineKey], [BranchKey], [CompanyKey], [DepartmentKey],
				[GeneralLedgerDataID], [GeneralLedgerDataKey],
				[GLAccountKey], [Currency],[ExchangeRate],[GLAccountType],
				[LocalCreditAmount],  [LocalDebitAmount], [OSCreditAmount], [OSDebitAmount], [PostDate], [PostPeriod],
				[SystemCreateTimeUtc], [SystemCreateUser], [SystemLastEditTimeUtc], [SystemLastEditUser],
				[Type], [GLTransactionHeaderKey], [TaxBranchKey], [TaxGLMovementKey])
				VALUES
					(8, 1, 2, 1, newid(), 9, 3, 'CNY', 1, 'ARC', 111.00, 0, 111.00, 0, '2023-08-10', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'PST', 8, NULL, NULL)

				INSERT[{0}].[Finance].[BAS__GLAccount]
					([GLAccountKey], [GLAccountID], [AccountNo], [Description])
					VALUES
						(3, newID(), '4455.45.54', 'TestC')

				UPDATE [{0}].[Finance].[BAS__GLAccount] SET Description = 'Update Desc' WHERE GLAccountKey = 1

				INSERT INTO[{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, PkValue, RefValue3)
				VALUES ('Finance', 'BAS__GLAccount', 1, NULL, ''), 
					('Finance', 'BAS__GLAccount', 1, NEWID(), 'Update Desc1'),
					('Finance', 'BAS__GLAccount', 3, NULL, ''),
					('Finance', 'BAS__GeneralLedgerData', 9, NULL, '')
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			PrepareTestDataForIncLoad();
		}

		void PrepareCompanyTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				UPDATE [{0}].[Organization].[BAS__Company] SET CountryCode = 'US', LocalCurrency = 'USD' WHERE CompanyKey = 1
				INSERT INTO [{0}].[Finance].[BAS__Currency] (CurrencyKey, CurrencyID, CurrencyCode, SubUnitRatio) VALUES (10, NEWID(), 'USD', 100)

				INSERT INTO[{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, PkValue, RefValue2)
				VALUES ('Organization', 'BAS__Company', 1, NULL, 'AUD'), ('Organization', 'BAS__Company', 1, NEWID(), 'AUD'), ('Organization', 'BAS__Company', 2, NULL, '')
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			PrepareTestDataForIncLoad();
		}

		void PrepareChargeCodeTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO [{0}].[Finance].[BAS__ChargeCode] (ChargeCodeKey, ChargeCodeID, GoodsServiceType) VALUES
					(3, NEWID(), 'GDS')

				UPDATE [{0}].[Finance].[BAS__ChargeCode] SET GoodsServiceType = 'GDS' WHERE ChargeCodeKey = 2

				INSERT INTO[{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, PkValue, RefValue4)
				VALUES ('Finance', 'BAS__ChargeCode', 2, NULL, ''),
						('Finance', 'BAS__ChargeCode', 2, NEWID(), ''),
						('Finance', 'BAS__ChargeCode', 3, NULL, '')
			", ScriptDbName);
			TestConnection.ExecuteNonQuery(sqlText);
			PrepareTestDataForIncLoad();
		}

		void PrepareTestDataForIncLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"
				INSERT INTO [{0}].[Organization].[BAS__Company] (CompanyKey, CompanyID, CountryCode, LocalCurrency) VALUES (2, NEWID(), 'CN', 'CNY')

				INSERT [{0}].[Finance].[BAS__GeneralLedgerData]
					([AccGLTransactionLineKey], [BranchKey], [CompanyKey], [DepartmentKey],
					[GeneralLedgerDataID], [GeneralLedgerDataKey],
					[GLAccountKey], [Currency],[ExchangeRate],[GLAccountType],
					[LocalCreditAmount],  [LocalDebitAmount], [OSCreditAmount], [OSDebitAmount], [PostDate], [PostPeriod],
					[SystemCreateTimeUtc], [SystemCreateUser], [SystemLastEditTimeUtc], [SystemLastEditUser],
					[Type], [GLTransactionHeaderKey], [TaxBranchKey], [TaxGLMovementKey], [JournalEntriesNumber])
					VALUES
						(8, 1, 2, 1, newid(), 8, 2, 'CNY', 1, 'ARC', 111.00, 0, 111.00, 0, '2023-08-10', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'PST', 8, NULL, NULL, '200000')

				INSERT INTO[{0}].[biadmin].[TransformedRow]
					(SchemaName, TableName, KeyValue, PkValue)
				VALUES ('Finance', 'BAS__GeneralLedgerData', 8, NULL)

				INSERT [{0}].[Finance].[BAS__GLTransactionHeader]
					([GLTransactionHeaderKey], [GLTransactionHeaderID], [CompanyKey],  [LocalAmount], [TransactionType], [TransactionTypeCode], [LedgerCode], [PostDateTime],  [OrganizationHeaderKey], [CreateDateTimeUtc], [ComplianceSubType], [TransactionNo], [TransactionReference], [BranchKey], [InvoiceDateTime], [DueDateTime], [Description], [TransactionCategory], [TransactionBelongsToGroup], [IsCancelled])
					VALUES
						(8, '2EAD1186-B082-40F0-81A4-4C7D4D5BC18C', 1, 400.00, 'Invoice', 'INV', 'AR', '2023-08-10', 1 ,'2023-08-10', '', '012345', 'ABC124', 1, '2023-08-10', '2023-08-10', '1', 'FIN', NULL, 0)

				INSERT [{0}].[Finance].[BAS__AccGLTransactionLine]
					([AccGLTransactionLineKey], [AccGLTransactionLineID], [GLTransactionHeaderKey], [OrganizationKey], [LineAmount], [LineType],  [GLAccountKey],[CreateDateUtc], [PostDateTime], [BranchKey], [DepartmentKey],[ChargeCodeKey], [Description],[TaxExtraRateNumerator],[TaxRateKey])
					VALUES
						(8, '61E7F2A2-979E-482B-9648-E09FDF0B0D12', 8, null, 400.00, 'REV',  1, '2023-08-10', '2023-08-10', 1, 1, 3, 'LineDesc1', 1, 1)
			", ScriptDbName
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		void PrepareTestDataForIniLoad()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture, @"

				DELETE FROM [{0}].Finance.BAS__GeneralLedgerData
				DELETE FROM [{0}].Organization.BAS__Company
				DELETE FROM [{0}].Finance.BAS__GLTransactionHeader
				DELETE FROM [{0}].Finance.BAS__AccGLTransactionLine
				DELETE FROM [{0}].Finance.BAS__TaxGLMovement
				DELETE FROM [{0}].Finance.BAS__TaxTransaction
				DELETE FROM [{0}].Finance.BAS__ChargeCode
				DELETE FROM [{0}].Finance.BAS__Currency
				DELETE FROM [{0}].Finance.BAS__GLAccount
				DELETE FROM [{0}].Organization.BAS__Organization

				INSERT INTO [{0}].[Organization].[BAS__Company] (CompanyID, CompanyKey, CountryCode, LocalCurrency) VALUES
					(newID(), 1, 'AU', 'AUD')

				INSERT INTO [{0}].[Finance].[BAS__ChargeCode] (ChargeCodeKey, ChargeCodeID, GoodsServiceType) VALUES (1, NEWID(), 'SRV'), (2, NEWID(), 'SRV')

				INSERT INTO [{0}].[Finance].[BAS__Currency] (CurrencyKey, CurrencyID, CurrencyCode, SubUnitRatio) VALUES (1, NEWID(), 'CNY', 100), (2, NEWID(), 'AUD', 200)

				INSERT [{0}].[Finance].[BAS__GLAccount]
					([GLAccountKey], [GLAccountID], [AccountNo], [Description])
					VALUES
						(1, newID(), '6556.45.13', 'TestA'),
						(2, newID(), '3532.24.32', 'TestB')

				INSERT INTO [{0}].[Organization].[BAS__Organization] (OrganizationKey, OrganizationID, Code) VALUES (1, '{1}', 'POROFFFXT')

				INSERT [{0}].[Finance].[BAS__GeneralLedgerData]
					([AccGLTransactionLineKey], [BranchKey], [CompanyKey], [DepartmentKey],
					[GeneralLedgerDataID], [GeneralLedgerDataKey],
					[GLAccountKey], [Currency],[ExchangeRate],[GLAccountType],
					[LocalCreditAmount],  [LocalDebitAmount], [OSCreditAmount], [OSDebitAmount], [PostDate], [PostPeriod],
					[SystemCreateTimeUtc], [SystemCreateUser], [SystemLastEditTimeUtc], [SystemLastEditUser],
					[Type], [GLTransactionHeaderKey], [TaxBranchKey], [TaxGLMovementKey], [JournalEntriesNumber])
					VALUES
						(1, 1, 1, 1, newid(), 1, 1, 'AUD', 1, 'ARC', 100.00, 0, 100.00, 0, '2023-08-10', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'PST', 1, NULL, NULL, '100000'),
						(2, 2, 1, 2, newid(), 2, 1, 'AUD', 1, 'ARC', 200.00, 0, 200.00, 0, '2023-08-10', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'PST', 1, NULL, NULL, '100001'),
						(null, 3, 1, 3, newid(), 3, 2, 'AUD', 1, 'MCA', 300.00, 0, 300.00, 0, '2023-08-12', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'TGM', null, 1, 2, '100002'),
						(3, 4, 1, 4, NEWID(), 4, 1, 'AUD', 1, 'TBA', 400, 0, 400, 0, '2023-08-12', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'CBV', 4, NULL, NULL, '100003'),
						(4, 4, 1, 4, NEWID(), 5, 1, 'AUD', 1, 'TLG', 500, 0, 500, 0, '2023-08-12', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'CBV', NULL, NULL, NULL, '100004'),
						(5, 4, 1, 4, NEWID(), 6, 1, 'AUD', 1, 'TBA', 600, 0, 600, 0, '2023-08-12', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'CBV', 5, NULL, NULL, '100005'),
						(6, 4, 1, 4, NEWID(), 7, 1, 'AUD', 1, 'TBA', 600, 0, 600, 0, '2023-08-12', 202308, '2023-08-21', '~BP', '2023-08-21', '~BP', 'CBV', 6, NULL, NULL, '100006')

				INSERT [{0}].[Finance].[BAS__GLTransactionHeader]
					([GLTransactionHeaderKey], [GLTransactionHeaderID], [CompanyKey],  [LocalAmount], [TransactionType], [TransactionTypeCode], [LedgerCode], [PostDateTime],  [OrganizationHeaderKey], [CreateDateTimeUtc], [ComplianceSubType], [TransactionNo], [TransactionReference], [BranchKey], [InvoiceDateTime], [DueDateTime], [Description], [TransactionCategory], [TransactionBelongsToGroup], [IsCancelled])
					VALUES
						(1, newid(), 1, 200.00, 'Invoice', 'INV', 'AR', '2023-08-10', 1 ,'2023-08-10', '', '012345', 'ABC124', 1, '2023-08-10', '2023-08-10', '1', 'FIN', NULL, 0),
						(2, '61E7F2A2-979E-482B-9648-E09FDF0B0D12', 1, 300.00, 'Credit Note', 'CRD', 'AP', '2023-08-13', 1 ,'2023-08-21', '', '0123456', 'ABC1245', 1, '2023-08-09', '2023-08-08', 'Desc1', 'FIN', NULL, 0),
						(3, '83055969-C125-439E-B709-F91F60CEA6BD', 1, 300.00, 'Invoice', 'INV', 'AP', '2023-08-12', 1 ,'2023-08-21', '', '01234567', 'ABC12456', 1, '2023-08-11', '2023-08-09', 'Desc2', 'FIN', NULL, 0),
						(4, '576A6CBA-500E-4DC6-8BEB-ABFD3C925CE7', 1, 300.00, 'Payment', 'PAY', 'AP', '2023-08-12', 1 ,'2023-08-21', '', '012345678', 'ABC124567', 1, '2023-08-13', '2023-08-13', 'Desc3', 'FIN', NULL, 0),
						(5, newid(), 1, 600.00, 'Direct Receipt', 'DRC', 'CB', '2023-08-12', 1 ,'2023-08-21', '', '236232', '', 1, '2023-08-13', '2023-08-12', 'Desc3', '', NULL, 0),
						(6, newid(), 1, 600.00, 'Direct Receipt', 'DRC', 'CB', '2023-08-12', 1 ,'2023-08-21', '', '236232', '', 1, '2023-08-14', '2023-08-14', 'Desc3', '', NEWID(), 1)

				INSERT [{0}].[Finance].[BAS__AccGLTransactionLine]
					([AccGLTransactionLineKey], [AccGLTransactionLineID], [GLTransactionHeaderKey], [OrganizationKey], [LineAmount], [LineType],  [GLAccountKey],[CreateDateUtc], [PostDateTime], [BranchKey], [DepartmentKey],[ChargeCodeKey], [Description],[TaxExtraRateNumerator],[TaxRateKey])
					VALUES
						(1, newid(), 1, null, 100.00, 'REV',  1, '2023-08-10', '2023-08-10', 1, 1, 1, 'LineDesc1', 1, 1),
						(2, newid(), 1, null, 200.00, 'REV',  1, '2023-08-10', '2023-08-12', 2, 1, 1, 'LineDesc2', 0, 1),
						(3, '83055969-C125-439E-B709-F91F60CEA6BD', 4, null, 200.00, 'CST',  1, '2023-08-10', '2023-08-12', 3, 1, 1, 'LineDesc3', 0, null),
						(4, newid(), NULL, 1, 500.00, 'ACR',  1, '2023-08-10', '2023-08-12', 4, 1, 2, 'LineDesc4', 0, null),
						(5, newid(), 5, null, 600.00, 'DRC',  1, '2023-08-10', '2023-08-12', 4, 1, 2, 'LineDesc5', 0, null),
						(6, newid(), 6, null, 600.00, 'DRC',  1, '2023-08-10', '2023-08-12', 4, 1, 2, 'LineDesc6', 0, null)

				INSERT [{0}].[Finance].[CUS__TaxGLMovementInfo]
					([TaxGLMovementKey],[DueDateTime],[LedgerCode], [TaxHeaderDescription], [InvoiceDateTime], [GLTransactionHeaderID], [TransactionTypeCode], [TaxLineDescription], [GLTransactionHeaderKey], [TransactionNo])
					VALUES
						(2, '2023-08-08 00:00:00', 'AP', 'Desc1', '2023-08-09 00:00:00', '61E7F2A2-979E-482B-9648-E09FDF0B0D12', 'CRD', 'DUU AP FIN Invoice 01234567', 2, '0123456')
				", ScriptDbName, OrganizationID
			);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override string ScriptDbName
		{
			get { return Db.EdwDatabaseName; }
		}

		HashSet<string> GetColumns()
		{
			var columns = new HashSet<string>
			{
				"BankAccountKey",
				"BranchKey",
				"CashBasisVATKey",
				"ChargeCodeKey",
				"ChequeOrReference",
				"CompanyID",
				"CompanyKey",
				"ComplianceNumber",
				"ComplianceSubType",
				"CountryCode",
				"Currency",
				"CurrencyKey",
				"DueDate",
				"DepartmentKey",
				"ExchangeRate",
				"GeneralLedgerDataID",
				"GeneralLedgerDataKey",
				"GeneralLedgerTransactionDataKey",
				"GLAccountID",
				"GLAccountKey",
				"GLAccountName",
				"GLAccountNum",
				"GLAccountType",
				"GLAmountLocalBalance",
				"GLAmountLocalCredit",
				"GLAmountLocalDebit",
				"GLAmountOSBalance",
				"GLAmountOSCredit",
				"GLAmountOSDebit",
				"GLType",
				"GoodsServiceType",
				"HeaderDescription",
				"HeaderJobKey",
				"HeaderTaxAmount",
				"InputGSTVATRecoverable",
				"InternalReference",
				"InvoiceAddressOverrideKey",
				"InvoiceAmount",
				"InvoiceDate",
				"JobInvoiceNumber",
				"JournalEntriesNumber",
				"LedgerCode",
				"LineAmount",
				"LineDescription",
				"LineGLAccountKey",
				"LineJobKey",
				"LineTaxAmount",
				"LocalCurrency",
				"OrganizationKey",
				"PostDate",
				"PostPeriod",
				"SubUnitRatio",
				"TaxBranchKey",
				"TaxClassID",
				"TaxExtraRateDenominator",
				"TaxExtraRateNumerator",
				"TaxGLMovementKey",
				"TaxRateDenominator",
				"TaxRateID",
				"TaxRateNumerator",
				"TransactionCategory",
				"TransactionDate",
				"TransactionHeaderID",
				"TransactionHeaderKey",
				"TransactionHeaderType",
				"TransactionLineID",
				"TransactionLineKey",
				"TransactionLineType",
				"TransactionNum",
				"TransactionType"
			};

			return columns;
		}

		void ExecuteCusTableLoad(string sqlName)
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				"SELECT {0} FROM [{1}].[biAdmin].[CustomTableConfiguration] WHERE  [ModelSchemaName] = 'Finance' AND [ModelTableName] = 'CUS__GeneralLedgerTransactionData'",
				sqlName, ScriptDbName
			);
			var resultTable = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			var record = resultTable.Select().Single();
			var incLoadSQLText = record.ItemArray[0].ToString();
			var sqlTextDL = "USE " + ScriptDbName + " " + incLoadSQLText;
			TestConnection.ExecuteNonQuery(sqlTextDL);
		}

		DataTable SelectRows(string tableViewName = "CUS__GeneralLedgerTransactionData")
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[{ScriptToTest.SchemaName}].[{tableViewName}]");
		}

		DataTable SelectTransformedRows()
		{
			return DataUtils.GetDataTableFromQuery(TestConnection, $"SELECT * FROM [{ScriptDbName}].[biadmin].TransformedRow where Tablename ='CUS__GeneralLedgerTransactionData'");
		}

		protected override void SetUp()
		{
			base.SetUp();
			OrganizationID = Guid.NewGuid();
			PrepareTestDataForIniLoad();
			ExecuteCusTableLoad("InitialLoadQuery");
		}

		Guid OrganizationID;
	}
}
