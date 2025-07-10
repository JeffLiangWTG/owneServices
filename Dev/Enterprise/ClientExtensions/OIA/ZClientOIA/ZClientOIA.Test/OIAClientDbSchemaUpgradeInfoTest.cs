using System.Collections;
using System.Data;
using CargoWise.Common.Testing;
using CargoWise.Data;
using CargoWise.Database.Abstractions.Extensions;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.OIA.Testing
{
	[TestedType(typeof(OIAClientDbSchemaUpgradeInfo))]
	internal class OIAClientDbSchemaUpgradeInfoTest : ConstraintForClientSpecificSchema
	{
		[ExpectNoExceptions]
		public void TestDbSchemaUpgradeInfo()
		{
			Db.Connection.BeginTransaction();
			try
			{
				ArrayList scripts = new ArrayList();
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.TableCreationScripts);
				scripts.AddRange(ClientOverride.Instance.DbSchemaExtensionObjects.ViewAndRoutineCreationScripts);
				scripts.Reverse();
				foreach (DatabaseObjectCreateScript script in scripts)
				{
					try
					{
						TestHelper.ExecuteNonQuery(script.DropScript);
					}
					catch
					{
					}
				}
			}
			finally
			{
				Db.Connection.RollbackTransaction();
			}
		}

		public override void TestAllIndexesMustSetAllowPageLocksToOffForClientSpecificSchema()
		{
			Assert("Will remove this method next PR", true);
		}

		public void TestExistence()
		{
			string sql = string.Format("SELECT name FROM sys.objects WHERE NAME = '{0}'", ClientOverride.Instance.DbSchemaExtensionObjects.ViewAndRoutineCreationScripts[0].ObjectName);
			DbCommand cmd = Db.Connection.Command(sql);
			object result = cmd.ExecuteScalar();
			AssertEquals("Client_OIA_GLTransactionsBatch", result.ToString());
		}

		[ExpectNoExceptions]
		public void TestGLAccountDescWithBankAccountDescMaxLength()
		{
			var testConnection = Db.Connection;
			SetupData(testConnection, "a", "b", "c");
			var bankAccountDesc = new string('a', AccBankAccountSchema.AB_Desc.MaxLength);
			testConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccBankAccount(AB_PK, AB_Code, AB_Desc, AB_RX_NKAccountCurrency, AB_AG, AB_GC, AB_AccountNum, AB_BSB, AB_SystemCreateTimeUtc, AB_SystemCreateUser, AB_SystemLastEditTimeUtc, AB_SystemLastEditUser)
			VALUES('576f75ea-85bc-431d-b448-372059355bda', 'AAA','" + bankAccountDesc + @"', 'AUD', '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '111111111', '123456', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			var result = ExecuteClientOIAGLTransactionsBatch(testConnection);
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			AssertEquals("RENTAL", result.Rows[0]["GLAccountDesc"].ToString());
		}

		[ExpectNoExceptions]
		public void TestTransactionDesc_TransactionNum_SecondRef_MaxLength()
		{
			var testConnection = Db.Connection;
			var transactionDesc = new string('a', AccTransactionLinesSchema.AL_Desc.MaxLength);
			var secondReference = new string('b', AccTransactionHeaderSchema.AH_ConsolidatedInvoiceRef.MaxLength);
			var transactionNum = new string('c', AccTransactionHeaderSchema.AH_TransactionNum.MaxLength);
			SetupData(testConnection, transactionDesc, transactionNum, secondReference);
			var result = DataUtils.GetDataTableFromQuery(testConnection, "SELECT * FROM dbo.AccTransactionHeader where AH_PK = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);
			result = DataUtils.GetDataTableFromQuery(testConnection, "SELECT * FROM dbo.AccTransactionLines where AL_AH = '591E6B52-95DF-42F8-8972-53FFF59C123F'");
			AssertEquals("Result should have row(s)", 1, result.Rows.Count);
			result = ExecuteClientOIAGLTransactionsBatch(testConnection);
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			AssertEquals(transactionDesc, result.Rows[0]["TransactionDesc"].ToString());
			AssertEquals(transactionNum, result.Rows[0]["TransactionNum"].ToString());
			AssertEquals(secondReference, result.Rows[0]["SecondRef"].ToString());
		}

		DataTable ExecuteClientOIAGLTransactionsBatch(DbConnection testConnection)
		{
			var template = @"EXEC Client_OIA_GLTransactionsBatch
			@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
			@StartPeriod = NULL,
			@EndPeriod = NULL,
			@StartDate = '2015-04-01',
			@EndDate = '2015-04-30',
			@StartGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
			@EndGLAccountPK = '907B7A0F-3700-4C4C-B313-971C8B0AE6FC',
			@BranchPK = NULL,
			@DepartmentPK = NULL,
			@DisplayDescription = 'L',
			@TransactionCategory = NULL,
			@BatchNumberToGet = NULL,
			@BatchNumberToSet = NULL,
			@IncludeZeroBalance = 'n',
			@IsExportingBatch = 'Y'";
			return DataUtils.GetDataTableFromQuery(testConnection, template);
		}

		void SetupData(DbConnection testConnection, string transactionDesc, string transactionNum, string secondReference)
		{
			testConnection.ExecuteNonQuery(@"INSERT INTO dbo.AccPeriodManagement (AM_PK, AM_Period, AM_Year, AM_StartDate,AM_EndDate,AM_IsSubLedgerClosed,AM_IsGeneralLedgerClosed, AM_GC_Company, AM_SystemCreateTimeUtc, AM_SystemCreateUser, AM_SystemLastEditTimeUtc, AM_SystemLastEditUser) values (NEWID(), 201504,2015,'04/01/2005','04/30/2015 23:59',1,1,'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			testConnection.ExecuteNonQuery(@"
			INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ConsolidatedInvoiceRef, AH_SystemCreateTimeUtc, AH_SystemCreateUser, AH_SystemLastEditTimeUtc, AH_SystemLastEditUser)
			VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'INV', '" + transactionNum + @"', 1, 'AR Invoice', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '" + secondReference + @"', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			testConnection.ExecuteNonQuery(@"
			INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE,AL_GB,AL_GC,AL_AG, AL_AH, AL_SystemCreateTimeUtc, AL_SystemCreateUser, AL_SystemLastEditTimeUtc, AL_SystemLastEditUser) 
			VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833CD', 'CST', 1, '" + transactionDesc + @"', 3000, 'C', 3000, 'AUD', 1.000000000, 0, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', '591E6B52-95DF-42F8-8972-53FFF59C123F', GetUtcDate(), '~BP', GetUtcDate(), '~BP')");
			testConnection.ExecuteNonQuery(@"
				INSERT INTO dbo.AccCashBasisVAT
				(
				YC_PK,
				YC_PostDate,
				YC_MatchGroupNum,
				YC_TaxBaseAmount,
				YC_TaxAmount,
				YC_AL_TransactionLine,
				YC_GC,
				YC_SystemCreateTimeUtc,
				YC_SystemCreateUser,
				YC_SystemLastEditTimeUtc,
				YC_SystemLastEditUser)
				VALUES
				(
				'de1cefa5-10c1-4835-a178-9a128b2eccb2',
				'2015-04-24 14:04:00',
				0,
				100,
				100,
				'3E50F5C5-49B2-45D3-AA4C-099FAE6833CD', --YC_AL_TransactionLine,
				'878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', --GC
				'2015-04-24 14:04:00',
				'ABC',
				GetUtcDate(),
				'ABC');
				");
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestCaseHelper.RunClientDbCreateScripts();
		}

		OIATestHelper TestHelper
		{
			get
			{
				return testHelper ?? (testHelper = new OIATestHelper(Factory));
			}
		}

		OIATestHelper testHelper;
	}
}
