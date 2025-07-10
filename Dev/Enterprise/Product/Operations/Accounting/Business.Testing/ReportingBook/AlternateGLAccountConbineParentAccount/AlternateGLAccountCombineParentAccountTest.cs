using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Business.Testing
{
	[TestedType(typeof(AlternateGLAccountCombineParentAccount))]
	public class AlternateGLAccountCombineParentAccountTest : NonPersistentBusinessObjectTestCase
	{
		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new AlternateGLAccountCombineParentAccount(Factory);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);
			Chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(Chart, 1, "X", "2", ".");

			GLHeader1 = Creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Credit);
			Factory.Save();
			AlternateGLAccount2 = Creator.CreateAccAlternateGlAccount(Chart.PK, "10.00.1100", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0);
			AlternateGLAccount3 = Creator.CreateAccAlternateGlAccount(Chart.PK, "10.00.1110", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 0, AccGLHeader.Constants.SectionTypes.Codes.TradingStatement, 0);

			Factory.Save();

			AlternateGLAccount1 = Creator.CreateAccAlternateGlAccount(Chart.PK, "10.00.1000", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit, 1, AccGLHeader.Constants.SectionTypes.Codes.Overheads, 1, alternateNum: AlternateGLAccount2.PK, percentNum: AlternateGLAccount3.PK, consolidate: AlternateGLAccount2.PK, totalReference: AlternateGLAccount3.PK);
			Creator.CreateAccAlternateGlAccountAttribute(AlternateGLAccount1, GLHeader1.PK, 1, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");
			Creator.CreateAccAlternateGlAccountAttribute(AlternateGLAccount1, GLHeader1.PK, 2, AccountingMasterFilesConstants.AlternateGLAccountAttributeCode.OCG, "OCG");

			Factory.Save();
		}

		#endregion

		public void TestAttributes()
		{
			var glAccountCombineParentAccount = (AlternateGLAccountCombineParentAccount)GetNewBusinessObject();
			glAccountCombineParentAccount.AlternateGLAccount = AlternateGLAccount1;
			glAccountCombineParentAccount.GLHeaderPK = GLHeader1.PK;

			AssertEquals("MGT", glAccountCombineParentAccount.ChartCode);
			Assert(!glAccountCombineParentAccount.IsGlobal);
			AssertEquals("10.00.1010", glAccountCombineParentAccount.ParentAccount);
			AssertEquals("10.00.1100", glAccountCombineParentAccount.AlternateNum);
			AssertEquals("10.00.1110", glAccountCombineParentAccount.PercentNum);
			AssertEquals("10.00.1100", glAccountCombineParentAccount.ConsolidationNum);
			AssertEquals("10.00.1110", glAccountCombineParentAccount.HeaderDependsOnTotal);
			AssertEquals(AlternateGLAccount1.AGA_SystemCreateTimeUtc.ToLocalBranchTime(), glAccountCombineParentAccount.CreateTime);
			AssertEquals(AlternateGLAccount1.AGA_SystemLastEditTimeUtc.ToLocalBranchTime(), glAccountCombineParentAccount.LastEditTime);
			AssertEquals(2, glAccountCombineParentAccount.AlternateGLAccountAttributes.Count);
		}

		public void TestCodeAndDescriptionProperty()
		{
			var codeProperty = CodePropertyAttribute.CodePropertyNameFromType(GetExpectedBusinessObjectType());
			var descriptionProperty = DescriptionPropertyAttribute.DescriptionPropertyNameFromType(GetExpectedBusinessObjectType());
			AssertEquals(nameof(AlternateGLAccountCombineParentAccount.AlternateAccountNum), codeProperty);
			AssertEquals(nameof(AlternateGLAccountCombineParentAccount.AlternateAccountName), descriptionProperty);

			var account = (AlternateGLAccountCombineParentAccount)GetNewBusinessObject();
			account.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();
			account.AlternateGLAccount.AGA_AccountNum = "998877";
			account.AlternateGLAccount.AGA_Description = "Test Property";
			AssertEquals("CodeProperty", "998877", account.AlternateAccountNum);
			AssertEquals("DescriptionProperty", "Test Property", account.AlternateAccountName);
		}

		AccAlternateChart Chart;
		protected AccAlternateGLAccount AlternateGLAccount1;
		protected AccAlternateGLAccount AlternateGLAccount2;
		protected AccAlternateGLAccount AlternateGLAccount3;
		protected AccGLHeader GLHeader1;
		TestObjectCreator Creator;
	}
}
