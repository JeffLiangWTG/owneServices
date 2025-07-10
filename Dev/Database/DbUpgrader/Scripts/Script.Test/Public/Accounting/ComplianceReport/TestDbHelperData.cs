using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using Enterprise.Build.Database.Script.TestFramework;

namespace Enterprise.Build.Database.Script.Testing.Public.Accounting.ComplianceReport
{
	public class TestDbHelperData
	{
		public TestDbHelperData(TestDbHelper helper)
		{
			this.helper = helper;
		}

		public TestDbHelperData SetupData()
		{
			dbHelperDataPKs = SetupHelperData();
			return this;
		}

		DbHelperDataPKs SetupHelperData()
		{
			var helperDataPKs = new DbHelperDataPKs();
			helperDataPKs.ReportPK = helper.InsertComplianceReport("U11", helper.ToDate("2021-01-01"), helper.ToDate("2021-12-31"));

			helperDataPKs.DefaultCompanyPK = TestDbHelper.DefaultCompanyPK;
			helperDataPKs.BranchPK = helper.InsertBranch("ZZB", helperDataPKs.DefaultCompanyPK);
			helperDataPKs.DepartmentPK = helper.InsertDepartment("ZZD");

			helperDataPKs.Creditor1PK = helper.InsertOrgHeader("CRD1", "Creditor1");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = helperDataPKs.DefaultCompanyPK, OB_OH = helperDataPKs.Creditor1PK, OB_IsCreditor = 1 });
			helperDataPKs.Debitor1PK = helper.InsertOrgHeader("DEB1", "Debitor1");
			helper.Insert("OrgCompanyData", new { OB_PK = Guid.NewGuid(), OB_GC = helperDataPKs.DefaultCompanyPK, OB_OH = helperDataPKs.Debitor1PK, OB_IsCreditor = 0 });

			helperDataPKs.UstvaAccount = helper.InsertGLAccount("1234.00.10", "USTVA Account");
			helperDataPKs.Ust111Account = helper.InsertGLAccount("1234.00.20", "UST111 Account");
			helperDataPKs.GlAccount = helper.InsertGLAccount("1234.56.78", "Bank Account 1", "BSH");
			helperDataPKs.BankAccount = helper.InsertBankAccount("BANK1", helperDataPKs.GlAccount);
			helperDataPKs.GlPnLAccount = helper.InsertGLAccount("1400.00.10", "BL P&L Account 1");

			return helperDataPKs;
		}

		public DataTable CreateTVP(TestDbHelper helper)
		{
			var taxMsgId1 = helper.InsertInvMsg("IM1");        // MST    19% for AR			-> 81_na_na
			var taxMsgId2 = helper.InsertInvMsg("IM2");        // LOWMST  7% for AR			-> 86_na_na
			var taxMsgId3 = helper.InsertInvMsg("IM3");        // MSTREV  0% for AR			-> 49_na_na
			var taxMsgId4 = helper.InsertInvMsg("IM4");        // MST	19% for AP			-> na_na_66
			var taxMsgId5 = helper.InsertInvMsg("IM5");        // NOTREPORT for AR and AP	-> na_na_na
			var taxMsgId6 = helper.InsertInvMsg("IM6");        // LOWMST  7% for AP			-> na_na_66
			var taxMsgId7 = helper.InsertInvMsg("IM7");        // MSTREV  0% for AP			-> 46_47_67

			// @TVP
			var tvpList = new List<Tuple<string, Guid>>() {
				new Tuple<string, Guid>("81_NA_NA", taxMsgId1),
				new Tuple<string, Guid>("86_NA_NA", taxMsgId2),
				new Tuple<string, Guid>("49_NA_NA", taxMsgId3),
				new Tuple<string, Guid>("NA_NA_66", taxMsgId4),
				new Tuple<string, Guid>("NA_NA_66", taxMsgId6),
				new Tuple<string, Guid>("46_47_67", taxMsgId7)
			};

			var tvp = new DataTable();
			tvp.Locale = CultureInfo.InvariantCulture;
			tvp.Columns.Add("Code", typeof(string)); // Part of SQL code
			tvp.Columns.Add("Guid", typeof(Guid)); // Part of SQL code
			DataRow row;
			foreach (Tuple<string, Guid> t in tvpList)
			{
				row = tvp.NewRow();
				row["Code"] = t.Item1;
				row["Guid"] = t.Item2;
				tvp.Rows.Add(row);
			}

			return tvp;
		}

		public Guid CreateACashbookQueueEntry(string type, string transNum, string postDateStr, string currency, string reportCode, int offSet, string reportSubCode, decimal amount)
		{
			var postDate = helper.ToDate(postDateStr);
			var cb = helper.InsertTransactionHeader("CB", type, transNum, amount * -1, postDate, bankAccountPK: dbHelperDataPKs.BankAccount, companyPK: dbHelperDataPKs.DefaultCompanyPK, currency: currency, branchPK: dbHelperDataPKs.BranchPK);
			var cbL1 = helper.InsertTransactionLine(cb, null, null, dbHelperDataPKs.UstvaAccount, dbHelperDataPKs.BranchPK, null, null, amount, type, postDate: postDate, null, 0m, transactionCurrency: currency, companyPK: dbHelperDataPKs.DefaultCompanyPK);
			postDate = postDate.AddMonths(offSet);
			reportSubCode = reportSubCode + " - " + postDate.Year.ToString("D4") + postDate.Month.ToString("D2");
			helper.InsertComplianceReportQueue(reportCode, cbL1, "AL", postDate, dbHelperDataPKs.DefaultCompanyPK, dbHelperDataPKs.BranchPK, reportSubCode);
			return cbL1;
		}

		public struct DbHelperDataPKs
		{
			public Guid ReportPK;
			public Guid DefaultCompanyPK;
			public Guid BranchPK;
			public Guid DepartmentPK;
			public Guid Creditor1PK;
			public Guid Debitor1PK;
			public Guid UstvaAccount;
			public Guid Ust111Account;
			public Guid GlAccount;
			public Guid BankAccount;
			public Guid GlPnLAccount;
		}

		public DbHelperDataPKs dbHelperDataPKs;
		readonly TestDbHelper helper;
	}
}
