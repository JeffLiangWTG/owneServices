using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AlternateGLAccounts))]
	public class AlternateGLAccountsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestParentGLAccountPK()
		{
			var newAccAlternateGLAccounts = (AlternateGLAccounts)GetNewBusinessObject();
			var alternateGLAccountWithAttributeSet = newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = AlternateGLAccount;
			newAccAlternateGLAccounts.ChartPK = Chart.PK;
			newAccAlternateGLAccounts.ParentGLAccountPK = GLHeader.PK;

			AssertEquals(newAccAlternateGLAccounts.ParentGLAccountPK, GLHeader.PK);
			AssertEquals(newAccAlternateGLAccounts.CashFlowCategory, GLHeader.AG_CashFlowType);
			AssertEquals(newAccAlternateGLAccounts.Unit, GLHeader.AG_StatisticalUnits);
			AssertEquals(newAccAlternateGLAccounts.ParentGLAccountPK, GLHeader.PK);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_DebitCredit, GLHeader.AG_DebitCredit);
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_ReportSection, GLHeader.AG_Column);
			AssertEquals(alternateGLAccountWithAttributeSet.ParentGLAccountPK, GLHeader.PK);
		}

		public void TestSetAccountType()
		{
			var newAccAlternateGLAccounts = (AlternateGLAccounts)GetNewBusinessObject();

			newAccAlternateGLAccounts.ChartPK = Chart.PK;
			newAccAlternateGLAccounts.AccountType = "BSH";
			newAccAlternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			AssertEquals(newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.AccountType, "BSH");
			Assert(newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.AccountType == "BSH"));

			newAccAlternateGLAccounts.AccountType = "TTL";
			AssertEquals(newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.AccountType, "TTL");
			Assert(newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.AccountType == "TTL"));
			AssertEquals(newAccAlternateGLAccounts.ParentGLAccountPK, ZGuid.Empty);
		}

		public void TestSetCashFlowCategory()
		{
			var newAccAlternateGLAccounts = (AlternateGLAccounts)GetNewBusinessObject();

			newAccAlternateGLAccounts.ChartPK = Chart.PK;
			newAccAlternateGLAccounts.CashFlowCategory = "XXX";
			newAccAlternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			AssertEquals(newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.CashFlowCategory, "XXX");
			Assert(newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.CashFlowCategory == "XXX"));

			newAccAlternateGLAccounts.CashFlowCategory = "YYY";
			AssertEquals(newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.CashFlowCategory, "YYY");
			Assert(newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().All(x => x.CashFlowCategory == "YYY"));
		}

		public void TestHasChange()
		{
			var newAccAlternateGLAccounts = (AlternateGLAccounts)GetNewBusinessObject();
			var alternateGLAccountWithAttributeSet = newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = AlternateGLAccount;
			newAccAlternateGLAccounts.ChartPK = Chart.PK;
			newAccAlternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			Factory.Save();

			AssertEquals(false, newAccAlternateGLAccounts.HasChanges);
			newAccAlternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
			AssertEquals(true, newAccAlternateGLAccounts.HasChanges);
		}

		public void TestIsInDatabase()
		{
			var newAccAlternateGLAccounts = (AlternateGLAccounts)GetNewBusinessObject();
			var alternateGLAccountWithAttributeSet = newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = AlternateGLAccount;
			newAccAlternateGLAccounts.ChartPK = Chart.PK;
			newAccAlternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			Factory.Save();

			AssertEquals(true, newAccAlternateGLAccounts.IsInDatabase);

			newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();

			AssertEquals(true, newAccAlternateGLAccounts.IsInDatabase);
		}

		public void TestNoAuditLogsWithAttibute()
		{
			var alternateGLAccounts = (AlternateGLAccounts)GetNewBusinessObject();
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.AccountType = Constants.AccountType.BalanceSheetAccount;
			alternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = "10.00.9999";
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_DebitCredit = Constants.DebitCredit.Debit;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_Description = "10.00.9999";

			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_Description = "TEST LOG";
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestChartPKAndAccountType_ReadOnly()
		{
			var newAccAlternateGLAccounts = (AlternateGLAccounts)GetNewBusinessObject();
			newAccAlternateGLAccounts.ChartPK = Chart.PK;
			newAccAlternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			var alternateGLAccountWithAttributeSet = newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.Cast<AlternateGLAccountWithAttributeSet>().FirstOrDefault();
			var altrenateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "20.00.1100", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1);
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = altrenateGLAccount.AGA_AccountNum;
			altrenateGLAccount.AGA_AccountType = Core.Constants.AccountType.BalanceSheetAccount;

			AssertEquals(false, newAccAlternateGLAccounts.IsInDatabase);
			AssertEquals(false, newAccAlternateGLAccounts.ChartPKInfo.ReadOnly);
			AssertEquals(false, newAccAlternateGLAccounts.AccountTypeInfo.ReadOnly);

			Factory.Save();

			AssertEquals(true, newAccAlternateGLAccounts.IsInDatabase);
			AssertEquals(true, newAccAlternateGLAccounts.ChartPKInfo.ReadOnly);
			AssertEquals(true, newAccAlternateGLAccounts.AccountTypeInfo.ReadOnly);
		}

		public void TestResetAlternateGLAccountsWithAttributeSet_NotDeleteAlternateGLAccountInDB()
		{
			var chart = Creator.CreateAlternateChart("ABC");

			var dissection = Creator.GLHeader1.AlternateGLAccountDissections.AddNew();
			dissection.ADC_AAC_AlternateChart = chart.PK;
			dissection.ADC_Attribute = "OCG";
			dissection.ADC_SeparateNumbering = true;
			Factory.Save();

			var account = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			var attribute = Creator.CreateAccAlternateGlAccountAttribute(account, Creator.GLHeader1.PK);
			Factory.Save();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = chart.PK;
			alternateGLAccounts.ParentGLAccountPK = Creator.GLHeader1.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();

			var alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = account;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountWithAttributeSet;

			alternateGLAccounts.ResetAlternateGLAccountsWithAttributeSet(alternateGLAccounts.ChartPK, alternateGLAccounts.ParentGLAccountPK);
			Assert(account.IsInDatabase);
			AssertEquals(false, account.IsDeleted);

			var accountNotSaved = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1010", "P&L", "CR", 1, "AS", 2);
			var attributeNotSaved = Creator.CreateAccAlternateGlAccountAttribute(accountNotSaved, Creator.GLHeader1.PK, 2);

			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = accountNotSaved;
			alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet = alternateGLAccountWithAttributeSet;

			alternateGLAccounts.ResetAlternateGLAccountsWithAttributeSet(alternateGLAccounts.ChartPK, alternateGLAccounts.ParentGLAccountPK);

			AssertEquals(false, accountNotSaved.IsInDatabase);
			Assert(accountNotSaved.IsDeleted);
		}

		public void TestFirstAlternateGLAccountWithAttributeSetAfterResetAlternateGLAccountsWithAttributeSet()
		{
			var chart = Creator.CreateAlternateChart("ABC");
			Factory.Save();
			var account = Creator.CreateAccAlternateGlAccount(chart.PK, "10.00.1000", "BSH", "DR", 1, "OV", 1);
			Creator.CreateAccAlternateGlAccountAttribute(account, Creator.GLHeader1.PK);
			Factory.Save();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = chart.PK;
			alternateGLAccounts.ParentGLAccountPK = Creator.GLHeader1.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			alternateGLAccounts.ResetAlternateGLAccountsWithAttributeSet(chart.PK, Creator.GLHeader1.PK);
			AssertEquals(1, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count);
			AssertEquals(account.PK, alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.PK);

			alternateGLAccounts.ParentGLAccountPK = Creator.GLHeader2.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			alternateGLAccounts.ResetAlternateGLAccountsWithAttributeSet(chart.PK, Creator.GLHeader2.PK);
			AssertEquals(1, alternateGLAccounts.AlternateGLAccountsWithAttributeSet.Count);
			AssertEquals("", alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_AccountNum);
			AssertEquals(Creator.GLHeader2.AG_Column, alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_ReportSection);
			AssertEquals(Creator.GLHeader2.AG_DebitCredit, alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_DebitCredit);
			Assert(!alternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccount.IsInDatabase);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AlternateGLAccounts(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			Chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Chart2 = Creator.CreateAlternateChart("MG2", "Management Reporting", true, true, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(Chart, 1, "X", "2", ".");
			Creator.CreateAccAlternateChartFormat(Chart2, 1, "X", "2", ".");
			Factory.Save();

			GLHeader = Creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Debit);
			GLHeader.AG_CashFlowType = "XXX";
			GLHeader.AG_StatisticalUnits = "KG";
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, true);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader, Chart2.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, true);

			GLHeader2 = Creator.CreateAccGLHeader("20.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Debit);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.ORG, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFE, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.LFO, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.TIC, false);
			Creator.CreateAccAlternateGLAccountDissection(GLHeader2, Chart.PK, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.SPR, false);

			AlternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "10.00.1100", Constants.AccountType.BalanceSheetAccount, Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1);
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, GLHeader.PK.ToGuid());
		}

		#endregion

		AccAlternateChart Chart;
		AccAlternateChart Chart2;
		AccGLHeader GLHeader;
		AccGLHeader GLHeader2;
		AccAlternateGLAccount AlternateGLAccount;
		TestObjectCreator Creator;
	}
}
