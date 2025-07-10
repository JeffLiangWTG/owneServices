using System;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Accounting;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Accounting
{
	[TestedType(typeof(Report_ProfitandLossMovementsbyAccount))]
	class Report_ProfitandLossMovementsbyAccountTest : Report_ProfitandLossMovementsbyAccountBaseTest
	{
		protected override string ScriptDbName => Db.DatabaseName;

		public override void CreateAccGLHeader(Guid pk, string accountNum, string description, string accountType, string statisticalUnits, string debitCredit, bool controlAccount, int totalLevel, string column, long gLAccoutKey = 1)
		{
			var sql = @"
INSERT INTO AccGLHeader (AG_PK, AG_AccountNum, AG_Description, AG_AccountType, AG_StatisticalUnits, AG_DebitCredit, AG_ControlAccount, AG_TotalLevel, AG_Column)
VALUES (@pk, @accountNum, @description, @accountType, @statisticalUnits, @debitCredit, @controlAccount, @totalLevel, @column)
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@pk", SqlDbType.UniqueIdentifier, pk);
				command.AddParameter("@accountNum", SqlDbType.VarChar, accountNum);
				command.AddParameter("@description", SqlDbType.VarChar, description);
				command.AddParameter("@accountType", SqlDbType.VarChar, accountType);
				command.AddParameter("@statisticalUnits", SqlDbType.VarChar, statisticalUnits);
				command.AddParameter("@debitCredit", SqlDbType.VarChar, debitCredit);
				command.AddParameter("@controlAccount", SqlDbType.Bit, controlAccount);
				command.AddParameter("@totalLevel", SqlDbType.Int, totalLevel);
				command.AddParameter("@column", SqlDbType.VarChar, column);
				command.ExecuteNonQuery();
			}
		}

		public override void CreateAccountGLAggregate(decimal amount, int period, TestParameter account, TestParameter branch, TestParameter company, TestParameter department, string transactionCategory = "")
		{
			var sql = @"
INSERT INTO AccGLAggregate (AA_PK, AA_Amount, AA_Period, AA_AG, AA_GB, AA_GC, AA_GE, AA_TransactionCategory)
VALUES (newid(), @amount, @period, @accountPK,@branchPK, @companyPK, @departmentPK, @transactionCategory);
";
			using (var command = TestConnection.Command(sql))
			{
				command.AddParameter("@amount", SqlDbType.Money, amount);
				command.AddParameter("@period", SqlDbType.Int, period);
				command.AddParameter("@accountPK", SqlDbType.UniqueIdentifier, account.PK);
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branch.PK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, company.PK);
				command.AddParameter("@departmentPK", SqlDbType.UniqueIdentifier, department.PK);
				command.AddParameter("@transactionCategory", SqlDbType.VarChar, transactionCategory);

				command.ExecuteNonQuery();
			}
		}

		public override Guid CreateCompany(TestParameter companyParameter, string countryCode, string currencyCode)
		{
			return TestDataCreator.CreateCompany(companyParameter.Code, countryCode, currencyCode);
		}

		public override Guid CreateBranch(TestParameter companyParameter, TestParameter branchParameter, TestParameter homePortParameter)
		{
			return TestDataCreator.CreateBranch(companyParameter.PK, branchParameter.Code, homePortParameter.Code);
		}

		public override Guid CreateDepartment(TestParameter departmentParameter)
		{
			return TestDataCreator.CreateDepartment(departmentParameter.Code);
		}
	}
}

