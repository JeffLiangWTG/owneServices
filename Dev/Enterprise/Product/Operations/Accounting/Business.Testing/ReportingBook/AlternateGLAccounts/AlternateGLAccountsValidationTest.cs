using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Business.Testing
{
	public class AlternateGLAccountsValidationTest : TestCaseWithFactory
	{
		public void TestValidateChartPK()
		{
			var newAccAlternateGLAccounts = new AlternateGLAccounts(Factory);
			newAccAlternateGLAccounts.Validation.ValidateChartPK();
			Assert(newAccAlternateGLAccounts.ChartPKInfo.HasError("Please enter a value."));

			newAccAlternateGLAccounts.ChartPK = Guid.NewGuid();
			newAccAlternateGLAccounts.Validation.ValidateChartPK();
			Assert(newAccAlternateGLAccounts.ChartPKInfo.HasError("Enter a valid selection."));

			newAccAlternateGLAccounts.ReadOnly = true;
			newAccAlternateGLAccounts.Validation.ValidateChartPK();
			AssertEquals(newAccAlternateGLAccounts.ChartPKInfo.HasErrors(), false);

			newAccAlternateGLAccounts.ReadOnly = false;
			newAccAlternateGLAccounts.ChartPK = Chart.PK;
			newAccAlternateGLAccounts.Validation.ValidateChartPK();
			AssertEquals(newAccAlternateGLAccounts.ChartPKInfo.HasErrors(), false);
		}

		public void TestValidateAccountType()
		{
			var newAccAlternateGLAccounts = new AlternateGLAccounts(Factory);
			newAccAlternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			newAccAlternateGLAccounts.ChartPK = Chart.PK;
			var alternateGLAccountWithAttributeSet = newAccAlternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
			newAccAlternateGLAccounts.FirstAlternateGLAccountWithAttributeSet.AlternateGLAccountNum = "1A22";
			newAccAlternateGLAccounts.Validation.ValidateAccountType();
			Assert(newAccAlternateGLAccounts.AccountTypeInfo.HasError("Please enter an Account Type."));

			newAccAlternateGLAccounts.AccountType = "XXX";
			newAccAlternateGLAccounts.Validation.ValidateAccountType();
			Assert(newAccAlternateGLAccounts.AccountTypeInfo.HasError("Enter a valid Account Type."));

			newAccAlternateGLAccounts.ReadOnly = true;
			newAccAlternateGLAccounts.Validation.ValidateAccountType();
			AssertEquals(newAccAlternateGLAccounts.AccountTypeInfo.HasErrors(), false);

			newAccAlternateGLAccounts.ReadOnly = false;
			newAccAlternateGLAccounts.AccountType = "GRP";
			newAccAlternateGLAccounts.Validation.ValidateAccountType();
			AssertEquals(newAccAlternateGLAccounts.AccountTypeInfo.HasErrors(), false);
		}

		public void TestValidateParentGLAccountPK()
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var alternateGLAccount1 = testObjectCreator.CreateAccAlternateGlAccount(Chart.PK, "10.00.1100", "BSH");
			var alternateGLAccount2 = testObjectCreator.CreateAccAlternateGlAccount(Chart.PK, "20.00.1100", "BSH");
			Factory.Save();

			var alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			var alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = alternateGLAccount1.AGA_AccountNum;
			alternateGLAccountWithAttributeSet.Attributes.RemoveAndDeleteAll();
			var attribute = alternateGLAccountWithAttributeSet.Attributes.AddNew();
			attribute.AAA_AAC_AlternateChart = Chart.PK;
			attribute.AAA_AGA_AlternateGLAccount = alternateGLAccount1.PK;
			alternateGLAccounts.Validation.ValidateParentGLAccountPK();
			Assert(alternateGLAccounts.ParentGLAccountPKInfo.HasError("Please enter a value."));

			alternateGLAccounts.ParentGLAccountPK = Guid.NewGuid();
			alternateGLAccounts.Validation.ValidateParentGLAccountPK();
			Assert(alternateGLAccounts.ParentGLAccountPKInfo.HasError("Enter a valid selection."));

			alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			GLHeader.AG_AccountType = "P&L";
			alternateGLAccounts.AccountType = "BSH";
			alternateGLAccounts.Validation.ValidateParentGLAccountPK();
			Assert(alternateGLAccounts.ParentGLAccountPKInfo.HasError("Parent GL Accounts should match up with the Account Type."));

			alternateGLAccounts.AccountType = "P&L";
			alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			attribute.AAA_AG_GLHeader = GLHeader.PK;
			Factory.Save();

			alternateGLAccounts = new AlternateGLAccounts(Factory);
			alternateGLAccounts.AccountType = "P&L";
			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.ParentGLAccountPK = GLHeader.PK;
			alternateGLAccounts.Validation.ValidateParentGLAccountPK();
			Assert(alternateGLAccounts.ParentGLAccountPKInfo.HasError("The selected Parent Account has been referenced to Alternate Account 10.00.1100 of Chart MGT, please enter another one."));

			alternateGLAccounts.ChartPK = Chart2.PK;
			alternateGLAccounts.Validation.ValidateParentGLAccountPK();
			AssertEquals(false, alternateGLAccounts.ParentGLAccountPKInfo.HasErrors());

			alternateGLAccounts.ChartPK = Chart.PK;
			alternateGLAccounts.ParentGLAccountPK = GLHeader2.PK;
			alternateGLAccounts.AlternateGLAccountsWithAttributeSet.RemoveAndDeleteAll();
			alternateGLAccountWithAttributeSet = alternateGLAccounts.AlternateGLAccountsWithAttributeSet.AddNew();
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = alternateGLAccount2.AGA_AccountNum;
			alternateGLAccountWithAttributeSet.AlternateGLAccount.AGA_Description = "TST";
			Factory.Save();

			var gLHeader3 = Factory.NewWithValidTestData<AccGLHeader>();
			gLHeader3.AG_AccountNum = "30.00.1010";
			gLHeader3.AG_DebitCredit = "DR";
			gLHeader3.AG_AccountType = "P&L";
			var dissection = gLHeader3.AlternateGLAccountDissections.AddNew();
			dissection.ADC_AAC_AlternateChart = Chart.PK;
			dissection.ADC_Attribute = "OCG";
			dissection.ADC_SeparateNumbering = true;
			alternateGLAccounts.ParentGLAccountPK = gLHeader3.PK;
			alternateGLAccounts.Validation.ValidateParentGLAccountPK();
			Assert(alternateGLAccounts.ParentGLAccountPKInfo.HasError("You cannot change the Parent Account to '30.00.1010' as it has dissection configuration with separate numbering. Please change the Parent Account back to '20.00.1010'."));

			dissection.ADC_SeparateNumbering = false;
			alternateGLAccounts.Validation.ValidateParentGLAccountPK();
			AssertEquals(alternateGLAccounts.ParentGLAccountPKInfo.HasErrors(), false);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			var creator = new TestObjectCreator(Factory);
			Chart = creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Chart2 = creator.CreateAlternateChart("MG2", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			creator.CreateAccAlternateChartFormat(Chart, 1, "9", "2", ".");
			creator.CreateAccAlternateChartFormat(Chart, 2, "X99", "2", ".");

			GLHeader = creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			GLHeader2 = creator.CreateAccGLHeader("20.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			Factory.Save();
		}

		#endregion

		AccAlternateChart Chart;
		AccAlternateChart Chart2;
		AccGLHeader GLHeader;
		AccGLHeader GLHeader2;
	}
}
