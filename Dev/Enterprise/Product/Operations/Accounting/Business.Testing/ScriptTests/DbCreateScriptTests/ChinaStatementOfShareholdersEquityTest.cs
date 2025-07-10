using System;
using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests
{
	class ChinaStatementOfShareholdersEquityTest : ScriptTest
	{
		[ExpectNoExceptions]
		public void TestTransactionDescPaymentReferenceNumber()
		{
			SetUpDataForTestTransactionDescPaymentReferenceNumber();
			var result = CallStoredProcedure();
			AssertEquals("Should be 1 rows in report", 1, result.Rows.Count);
			AssertEquals("Should be 119 columns in result", 119, result.Columns.Count);
			AssertEquals("B03_Credit should be 3000.0000", 3000m, result.Rows[0]["B03_Credit"]);
		}

		void SetUpDataForTestTransactionDescPaymentReferenceNumber()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			var accGlHeader = Factory.LoadTop1<AccGLHeader>(new ZQuery(AccGLHeaderSchema.AG_AccountNum, "3510.00.00"));
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'INV', '00001000', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491')");
			TestConnection.ExecuteNonQuery($@"
INSERT INTO dbo.AccTransactionLines (AL_PK, AL_LineType, AL_Sequence, AL_LineAmount, AL_GSTVATBasis, AL_OSAmount, AL_RX_NKTransactionCurrency, AL_ExchangeRate, AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE, AL_GB, AL_GC, AL_AG, AL_AH) 
VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833CD', 'CST', 1,  3000, 'C', 3000, 'AUD', 1.000000000, 0, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '{accGlHeader.PK}', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			var accGlAccountDescriptor = TestObjectCreator.CreateAccountDescriptor(accGlHeader, "1000.00.00", "SSE", "B03", "ZH-CN",
				"", "CN", Constants.DebitCredit.Debit);
			TestObjectCreator.CreateGLDescriptorPivotLight(accGlAccountDescriptor, accGlHeader);
			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"), Guid.Empty, Guid.Empty, new DateTime(2014, 12, 1));
			Factory.Save();
		}

		DataTable CallStoredProcedure()
		{
			var sqlText = $@"EXEC [{ScriptDbName}].[dbo].ChinaStatementOfShareholdersEquity
							@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
							@EndPeriod = 201505,
							@Branch = NULL";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, sqlText);
			return result;
		}

		protected string ScriptDbName
		{
			get { return Db.DatabaseName; }
		}
	}
}
