using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing.ScriptTests.DbCreateScriptTests
{
	[UseSnapshotProtection(new[] { DatabaseType.Main })]
	class ChinaAssetsProvisionTest : ScriptTest
	{
		[ExpectNoExceptions]
		public void TestTransactionDescPaymentReferenceNumberMaxLength()
		{
			SetUpDataForTestTransactionDescPaymentReferenceNumberMaxLength();
			CallStoredProcedure();
		}

		void SetUpDataForTestTransactionDescPaymentReferenceNumberMaxLength()
		{
			TestObjectCreator.CreateTestPeriodsForEntireYear(2015);
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionHeader (AH_PK, AH_Ledger, AH_TransactionType, AH_TransactionNum, AH_TransactionCount, AH_Desc, AH_InvoiceDate, AH_DueDate, AH_InvoiceAmount, AH_OSTotal, AH_RX_NKTransactionCurrency, AH_ExchangeRate, AH_PostDate, AH_TransactionCategory, AH_OutstandingAmount, AH_PostToGL, AH_InvoiceTerm,AH_NumberOfSupportingDocuments, AH_OH, AH_OA_InvoiceAddressOverride,AH_GB, AH_GC, AH_GE, AH_ChequeOrReference)
VALUES('591E6B52-95DF-42F8-8972-53FFF59C123F', 'AR', 'INV', '00001000', 1, 'AR INVOICE', '2015-04-24 14:04:00', '2015-04-24 14:04:00', 6000, 6000, 'AUD', 1, '2015-04-24 14:04:00', 'FIN', 6000, 'Y', 'COD', 1, '0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1', '37A65812-36DB-4D6C-9AB7-71EE9BF599F2', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '86BB1C22-0865-4685-996E-D56CBD136491', '" + StringLen35 + @"')");
			TestConnection.ExecuteNonQuery(@"
INSERT INTO dbo.AccTransactionLines (AL_PK,AL_LineType,AL_Sequence,AL_Desc,AL_LineAmount, AL_GSTVATBasis, AL_OSAmount,AL_RX_NKTransactionCurrency,AL_ExchangeRate,AL_InputGSTVATRecoverable, AL_PostDate, AL_PostToGL, AL_ReverseDate,AL_ReverseToGL, AL_RevRecognitionType, AL_GE, AL_GB, AL_GC, AL_AG, AL_AH) 
VALUES('3E50F5C5-49B2-45D3-AA4C-099FAE6833CD', 'CST', 1, '" + StringLen1024 + @"', 3000, 'C', 3000, 'AUD', 1.000000000, 0, '2015-04-24 14:04:00', 'N', '2015-04-24 14:04:00', 'Y', 'IMM', '86BB1C22-0865-4685-996E-D56CBD136491', '27A55065-AC88-4EC3-8BED-E575E79172CB', '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC', '907B7A0F-3700-4C4C-B313-971C8B0AE6FC', '591E6B52-95DF-42F8-8972-53FFF59C123F')");
			TestConnection.ExecuteNonQuery(@"
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
				YC_SystemCreateUser)
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
				'ABC');
				");

			AccountingMasterFilesRegistry.Instance.JournalEntriesLastProcessedDate.SetValue(new Guid("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC"), Guid.Empty, Guid.Empty, new DateTime(2015, 04, 01));
		}

		DataTable CallStoredProcedure()
		{
			var sqlText = @"EXEC [{0}].[dbo].ChinaAssetsProvision
							@CompanyPK = '878D7ACA-FFC3-49FC-9710-969CA0C0F2AC',
							@EndPeriod = 201505,
							@Branch = NULL";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, ScriptDbName));
			return result;
		}

		readonly string StringLen35 = new string('b', 35);
		readonly string StringLen1024 = new string('a', 1024);

		protected string ScriptDbName
		{
			get { return Db.DatabaseName; }
		}
	}
}
