using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Database.TestFramework.ObjectModel;
using CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.EDW.Functions.Accounting;
using Enterprise.Build.Database.Script.Public.BusinessIntelligence.EDW.Functions.Accounting.Testing;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.BusinessIntelligence.EDW.Functions.Accounting
{
	[TestedType(typeof(ChinaAssetsProvision))]
	class ChinaAssetsProvisionTest : BiCreateScriptTest
	{
		protected override string ScriptDbName => Db.EdwDatabaseName;

		[ExpectNoExceptions]
		public void TestTransactionDescPaymentReferenceNumberMaxLength()
		{
			SetUpDataForTestTransactionDescPaymentReferenceNumberMaxLength();
			CallStoredProcedure();
		}

		void SetUpDataForTestTransactionDescPaymentReferenceNumberMaxLength()
		{
			var helper = new AccountingFunctionTestingHelper(TestConnection, ScriptDbName);
			var prepareDataHelper = new PrepareDataHelper(TestConnection, ScriptDbName);
			prepareDataHelper.InsertCompanyBranchAndDepartment();
			helper.InsertPeriodForInputYear(2015);
			var date = new DateTime(2015, 04, 24, 14, 04, 00);
			var dateStr = date.ToString("yyyy-MM-dd HH:mm:ss");
			helper.InsertStmDataDate("JournalEntriesLastProcessedDate", date, 1L);

			var transactionHeader =
				new AccTransactionHeader("AR", "INV", branchPK, departmentPK, companyPK)
				{
					AH_TransactionNum = "00001000",
					AH_InvoiceDate = date,
					AH_InvoiceAmount = 6000m,
					AH_OSTotal = 6000m,
					AH_OutstandingAmount = 6000m
				};

			helper.InsertTransactionHeader(transactionHeader, StringLen35, "AUD", dateStr, dateStr, Guid.Parse("0DAAB61B-255E-4AD7-AFC5-4E7B03C3BDA1"));
			var lineKey = helper.InsertAccGLTransactionLine("CST", companyPK, dateStr, dateStr, 3000M);
			helper.InsertBASCashBasisVAT(1L, lineKey, dateStr, 100m);
		}

		DataTable CallStoredProcedure()
		{
			var sqlText = @"EXEC [{0}].[dbo].ChinaAssetsProvision
							@CompanyPK = '{1}',
							@EndPeriod = 201505,
							@Branch = NULL";
			var result = DataUtils.GetDataTableFromQuery(TestConnection, string.Format(sqlText, ScriptDbName, companyPK));
			return result;
		}

		readonly string StringLen35 = new string('b', 35);
		readonly Guid companyPK = Guid.Parse("878D7ACA-FFC3-49FC-9710-969CA0C0F2AC");
		readonly Guid branchPK = Guid.Parse("27A55065-AC88-4EC3-8BED-E575E79172CB");
		readonly Guid departmentPK = Guid.Parse("86BB1C22-0865-4685-996E-D56CBD136491");
	}
}
