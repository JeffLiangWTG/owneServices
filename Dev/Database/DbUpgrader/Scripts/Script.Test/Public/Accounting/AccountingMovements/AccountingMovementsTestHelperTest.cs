using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Build.Database.Script.TestFramework;
using static NUnit.Framework.Assertion;

namespace Enterprise.Build.Database.Script.Public.Accounting.AccountingMovements.Testing
{
	public class AccountingMovementsTestHelper
	{
		public AccountingMovementsTestHelper(DbConnection connection)
		{
			this.connection = connection;
		}

		readonly DbConnection connection;

		TestDbHelper helper;
		TestDbHelper Helper => helper ?? (helper = new TestDbHelper(connection));

		public Guid BranchPK;
		public Guid BranchPK2;
		public Guid DepartmentPK;

		public void Setup()
		{
			Helper.InsertAccPeriod(2018, 06, TestDbHelper.DefaultCompanyPK);
			Helper.InsertAccPeriod(2018, 07, TestDbHelper.DefaultCompanyPK);
			Helper.InsertAccPeriod(2018, 08, TestDbHelper.DefaultCompanyPK);
			BranchPK = Helper.InsertBranch("ZZB", TestDbHelper.DefaultCompanyPK);
			BranchPK2 = Helper.InsertBranch("ZZC", TestDbHelper.DefaultCompanyPK);
			DepartmentPK = Helper.InsertDepartment("ZZD");

			SetupControlAccount();
			DeleteChargeCodes();
		}

		public DateTime PostDate => new DateTime(2018, 06, 20);

		public DateTime ReverseDate => new DateTime(2018, 07, 20);

		public DateTime CashDate => new DateTime(2018, 08, 20);

		#region Control Account

		public void SetupControlAccount(bool clearControlAccounts = false, bool clearCFXAccountSetup = false, bool clearJRJAccountSetup = false)
		{
			SetupAPControlAccount(clearControlAccounts);
			SetupARControlAccount(clearControlAccounts);
			SetupAccruedControlAccount(clearControlAccounts);
			SetupJobJournalControlAccount(clearControlAccounts || clearCFXAccountSetup, clearControlAccounts || clearJRJAccountSetup);
		}

		public Guid APControlAccountPK;
		public Guid GSTInputAccountPK;
		public Guid APSuspenseControlAccountPK;
		public Guid PendingGSTInputAccountPK;

		public void SetupAPControlAccount(bool clearControlAccounts = false)
		{
			if (clearControlAccounts)
			{
				DeleteControlAccount("GL_AP_CONTROL_ACCOUNT");
				DeleteControlAccount("GL_GST_INPUT_ACCOUNT");
				DeleteControlAccount("GL_AP_SUSPENSE_CONTROL_ACCOUNT");
				DeleteControlAccount("GL_PENDING_GST_INPUT_ACCOUNT");
			}
			else
			{
				APControlAccountPK = InsertControlAccount("1234.56.01", "GL_AP_CONTROL_ACCOUNT");
				GSTInputAccountPK = InsertControlAccount("1234.56.02", "GL_GST_INPUT_ACCOUNT");
				APSuspenseControlAccountPK = InsertControlAccount("1234.56.03", "GL_AP_SUSPENSE_CONTROL_ACCOUNT");
				PendingGSTInputAccountPK = InsertControlAccount("1234.56.04", "GL_PENDING_GST_INPUT_ACCOUNT");
			}
		}

		public Guid ARControlAccountPK;
		public Guid GSTOutputAccountPK;
		public Guid ARSuspenseControlAccountPK;
		public Guid PendingGSTOutputAccountPK;

		public void SetupARControlAccount(bool clearControlAccounts = false)
		{
			if (clearControlAccounts)
			{
				DeleteControlAccount("GL_AR_CONTROL_ACCOUNT");
				DeleteControlAccount("GL_GST_OUTPUT_ACCOUNT");
				DeleteControlAccount("GL_AR_SUSPENSE_CONTROL_ACCOUNT");
				DeleteControlAccount("GL_PENDING_GST_OUTPUT_ACCOUNT");
			}
			else
			{
				ARControlAccountPK = InsertControlAccount("1234.57.01", "GL_AR_CONTROL_ACCOUNT");
				GSTOutputAccountPK = InsertControlAccount("1234.57.02", "GL_GST_OUTPUT_ACCOUNT");
				ARSuspenseControlAccountPK = InsertControlAccount("1234.57.03", "GL_AR_SUSPENSE_CONTROL_ACCOUNT");
				PendingGSTOutputAccountPK = InsertControlAccount("1234.57.04", "GL_PENDING_GST_OUTPUT_ACCOUNT");
			}
		}

		public Guid WIPControlAccountPK;
		public Guid AccrualControlAccountPK;

		public void SetupAccruedControlAccount(bool clearControlAccounts = false)
		{
			if (clearControlAccounts)
			{
				DeleteControlAccount("GL_ACCRUED_REVENUE_ACCOUNT");
				DeleteControlAccount("GL_ACCRUED_COST_ACCOUNT");
			}
			else
			{
				WIPControlAccountPK = InsertControlAccount("1234.58.01", "GL_ACCRUED_REVENUE_ACCOUNT");
				AccrualControlAccountPK = InsertControlAccount("1234.58.02", "GL_ACCRUED_COST_ACCOUNT");
			}
		}

		public Guid CFXAccountPK;
		public Guid JobRevenueJournalControlAccountPK;

		public void SetupJobJournalControlAccount(bool clearCFXAccountSetup = false, bool clearJRJAccountSetup = false)
		{
			if (clearCFXAccountSetup)
			{
				DeleteControlAccount("GL_CFX_ACCOUNT");
			}
			else
			{
				CFXAccountPK = InsertControlAccount("1234.59.01", "GL_CFX_ACCOUNT");
			}

			if (clearJRJAccountSetup)
			{
				DeleteControlAccount("GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT");
			}
			else
			{
				JobRevenueJournalControlAccountPK = InsertControlAccount("1234.59.02", "GL_JOB_REVENUE_JOURNAL_CONTROL_ACCOUNT", "GL_JOB_REV_JOURNAL_CONTROL_ACCOUNT");
			}
		}

		#endregion

		#region GL Account

		public Guid GLAccountPK1
		{
			get
			{
				if (glAccountPK1 == Guid.Empty)
				{
					glAccountPK1 = helper.InsertGLAccount("1234.55.01", "TestGLAccount 1");
				}
				return glAccountPK1;
			}
		}
		Guid glAccountPK1;

		public Guid GLAccountPK2
		{
			get
			{
				if (glAccountPK2 == Guid.Empty)
				{
					glAccountPK2 = helper.InsertGLAccount("1234.55.02", "TestGLAccount 2");
				}
				return glAccountPK2;
			}
		}
		Guid glAccountPK2;

		#endregion

		#region Charge Code

		public Guid ChargeCodePK
		{
			get
			{
				if (chargeCodePK == Guid.Empty)
				{
					chargeCodePK = Helper.InsertChargeCode(TestDbHelper.DefaultCompanyPK, "CC1");
					var glAccount1 = Helper.InsertGLAccount("1111.01.01", "Revenue Account");
					var glAccount2 = Helper.InsertGLAccount("1111.01.02", "WIP Account");
					var glAccount3 = Helper.InsertGLAccount("1111.01.03", "Cost Account");
					var glAccount4 = Helper.InsertGLAccount("1111.01.04", "Accrual Account");

					connection.ExecuteNonQuery($@"
UPDATE dbo.AccChargeCode SET AC_AG_RevenueAccount = '{glAccount1}', AC_AG_WIPAccount = '{glAccount2}',
AC_AG_CostAccount = '{glAccount3}', AC_AG_AccrualAccount = '{glAccount4}',
AC_SystemLastEditTimeUtc = GETUTCDATE(), AC_SystemLastEditUser = 'TST'
WHERE AC_PK = '{ChargeCodePK}'
");
				}
				return chargeCodePK;
			}
		}
		Guid chargeCodePK;

		#endregion

		public void AssertJournalAmount(DataTable table, decimal journalAmount, Guid? accountPK = null, DateTime? glPostdate = null, Guid? branch = null)
		{
			AssertJournalAmount(table, "JournalAmount", journalAmount, accountPK, glPostdate, branch);
		}

		public void AssertOSJournalAmount(DataTable table, decimal osJournalAmount, Guid? accountPK = null)
		{
			AssertJournalAmount(table, "OSJournalAmount", osJournalAmount, accountPK);
		}

		void AssertJournalAmount(DataTable table, string amountColumn, decimal amount, Guid? accountPK = null, DateTime? glPostdate = null, Guid? branch = null)
		{
			var journals = table.AsEnumerable();

			if (glPostdate.HasValue)
			{
				journals = journals.Where(x => (DateTime)x["GLPostDate"] == glPostdate.Value);
			}
			if (accountPK.HasValue)
			{
				journals = journals.Where(x => (Guid)x["AG_PK"] == accountPK.Value);
			}
			if (branch.HasValue)
			{
				journals = journals.Where(x => (Guid)x["GL_GB"] == branch.Value);
			}

			AssertEquals("journal amount", amount, journals.Sum(x => (decimal)x[amountColumn]));
		}

		public void AssertContainJournal(DataTable table, decimal journalAmount, Guid accountPK)
		{
			Assert("journal exists", table.AsEnumerable().Any(x => (Guid)x["AG_PK"] == accountPK && (decimal)x["JournalAmount"] == journalAmount));
		}

		public void AssertContainOSJournal(DataTable table, decimal journalAmount, Guid accountPK)
		{
			Assert("journal exists", table.AsEnumerable().Any(x => (Guid)x["AG_PK"] == accountPK && (decimal)x["OSJournalAmount"] == journalAmount));
		}

		public void AssertAggregationAmount(DataTable journalTable, bool isEmptyAggregation = false)
		{
			try
			{
				connection.ExecuteNonQuery($"EXEC TakeUpSubledgers '{TestDbHelper.DefaultCompanyPK}', 'TST'");

				var aggregateTable = DataUtils.GetDataTableFromQuery(Db.Connection, "SELECT * FROM dbo.AccGLAggregate");

				if (isEmptyAggregation)
				{
					Assert("AccGLAggregate has no rows.", aggregateTable.Rows.Count == 0);
					return;
				}

				Assert("AccGLAggregate has rows.", aggregateTable.Rows.Count > 0);

				var groupedJournal = journalTable.AsEnumerable().
							GroupBy(r => new
							{
								Period = (int)r["AM_Period"],
								GLAccount = (Guid)r["AG_PK"],
								Branch = (Guid)r["GL_GB"],
								Department = (Guid)r["GL_GE"]
							})
							.Select(g => new
							{
								Amount = g.Sum(r => (decimal)r["JournalAmount"]),
								g.Key.Period,
								g.Key.GLAccount,
								g.Key.Branch,
								g.Key.Department
							});

				var groupedAggregation = aggregateTable.AsEnumerable().
							GroupBy(r => new
							{
								Period = (int)r["AA_Period"],
								GLAccount = (Guid)r["AA_AG"],
								Branch = (Guid)r["AA_GB"],
								Department = (Guid)r["AA_GE"]
							})
							.Select(g => new
							{
								Amount = g.Sum(r => (decimal)r["AA_Amount"]),
								g.Key.Period,
								g.Key.GLAccount,
								g.Key.Branch,
								g.Key.Department
							});

				AssertEquals("aggregation count should equal to journal count", groupedAggregation.Count(), groupedJournal.Count());
				foreach (var aggregation in groupedAggregation)
				{
					var journal = groupedJournal.FirstOrDefault(x => x.Period == aggregation.Period &&
																	x.GLAccount == aggregation.GLAccount &&
																	x.Branch == aggregation.Branch &&
																	x.Department == aggregation.Department);
					AssertNotNull(journal);
					AssertEquals("aggregation amount", aggregation.Amount, journal.Amount);
				}
			}
			catch (SqlException ex)
			{
				AssertNullOrEmpty("SqlException", ex.Message);
			}
		}

		public Guid InsertControlAccount(string accountNumber, string accountName, string accountDescription = null)
		{
			var accountPK = Helper.InsertGLAccount(accountNumber, accountDescription ?? accountName);
			connection.ExecuteNonQuery($@"
DELETE FROM dbo.StmData WHERE SD_Name IN ('{accountName}');
INSERT INTO dbo.StmData (SD_PK, SD_Name, SD_Owner, SD_DepartmentGuid, SD_Type, SD_BinaryValue, SD_GuidValue)
VALUES
(NEWID(), '{accountName}', NULL, NULL, 'GID', null, '{accountPK}')
");

			return accountPK;
		}

		public void DeleteControlAccount(string accountName)
		{
			connection.Command($@"
DELETE FROM dbo.StmData WHERE SD_Name IN ('{accountName}');
").ExecuteNonQuery();
		}

		public void EnableGSTCashBasis(Guid companyPK)
		{
			connection.ExecuteNonQuery($@"UPDATE dbo.GlbCompany SET GC_IsGSTCashBasis = 1, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetUtcDate() WHERE GC_PK = '{companyPK}';");
		}

		public void SetCompanyReciprocal(Guid companyPK, bool isReciprocal)
		{
			connection.ExecuteNonQuery($@"UPDATE dbo.GlbCompany SET GC_IsReciprocal = {(isReciprocal ? 1 : 0)}, GC_SystemLastEditUser = 'E', GC_SystemLastEditTimeUtc = GetUtcDate() WHERE GC_PK = '{companyPK}';");
		}

		void DeleteChargeCodes()
		{
			connection.ExecuteNonQuery("DELETE FROM dbo.AccChargeCode;");
		}
	}
}

