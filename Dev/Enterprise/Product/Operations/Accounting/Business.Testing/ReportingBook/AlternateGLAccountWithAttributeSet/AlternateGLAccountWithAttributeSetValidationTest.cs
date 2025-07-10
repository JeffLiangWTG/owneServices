using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.MasterFiles.Business.AccAlternateChartLookups;

namespace Enterprise.Accounting.Business.Testing
{
	public class AlternateGLAccountWithAttributeSetValidationTest : TestCaseWithFactory
	{
		public void TestValidateAlternateGLAccountNum()
		{
			var alternateGLAccountWithAttributeSetDetails = new AlternateGLAccountWithAttributeSetDetails(GLHeader.PK, Chart.PK, 0, "BSH", "CSH", "KG");
			var alternateGLAccountWithAttributeSet = new AlternateGLAccountWithAttributeSet(alternateGLAccountWithAttributeSetDetails, Factory);
			alternateGLAccountWithAttributeSet.AlternateGLAccount = Factory.New<AccAlternateGLAccount>();

			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccountNum, string.Empty);
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasError("Please enter an Alternate Account.\r\nThe length of Account Number is invalid. Please enter the number with 4 characters."));

			alternateGLAccountWithAttributeSet.Chart.AAC_IsFixedLength = true;
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "12";
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasError("The length of Account Number is invalid. Please enter the number with 4 characters."));

			alternateGLAccountWithAttributeSet.Chart.AAC_IsFixedLength = false;
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasError("The length of Account Number is invalid. Please enter the number with 1, or 4 characters."));

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "1212";
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasError("The Account Number does not matched up with the Account Format '9X99' which is set in Chart MGT."));

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "1A12";
			var newAttribute = alternateGLAccountWithAttributeSet.Attributes.AddNew();
			newAttribute.AAA_AAC_AlternateChart = Chart.PK;
			newAttribute.AAA_AG_GLHeader = GLHeader.PK;
			newAttribute.AAA_AGA_AlternateGLAccount = alternateGLAccountWithAttributeSet.AlternateGLAccount.PK;
			newAttribute.AAA_Sequence = 10;
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasError("Cannot be the same Account Number for different combinations."));

			newAttribute.Delete();

			alternateGLAccountWithAttributeSet.AccountType = "GRP";
			alternateGLAccountWithAttributeSet.ParentGLAccountPK = ZGuid.Empty;
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "1A22";
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasError("The Account type RUP/GRP must be the first GL Account of a GL Account Tier and have child GL Accounts."));

			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "1";
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasError("The Account type RUP/GRP must be the first GL Account of a GL Account Tier and have child GL Accounts."));

			alternateGLAccountWithAttributeSet.ReadOnly = true;
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasErrors(), false);

			alternateGLAccountWithAttributeSet.ReadOnly = false;
			var newFactory = new BusinessObjectFactory();
			var alternateGLAccount = new TestObjectCreator(newFactory).CreateAccAlternateGlAccount(Chart.PK, "1A22", Core.Constants.AccountType.BalanceSheetAccount);

			newFactory.Save();

			alternateGLAccountWithAttributeSet.AccountType = "P&L";
			alternateGLAccountWithAttributeSet.AlternateGLAccountNum = "1A22";
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			Assert(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasError("The specified Alternate Account's Number '1A22' already existed in the Alternate Chart 'MGT' with a different Account Type 'BSH'.\r\nPlease enter a different value."));

			alternateGLAccountWithAttributeSet.AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			alternateGLAccountWithAttributeSet.Validation.ValidateAlternateGLAccountNum();
			AssertEquals(alternateGLAccountWithAttributeSet.AlternateGLAccountNumInfo.HasErrors(), false);
		}

		public void TestAGA_AccountTypeSetReadonly()
		{
			var alternateGLAccountWithAttributeSetDetails = new AlternateGLAccountWithAttributeSetDetails(GLHeader.PK, Chart.PK, 0, "BSH", "CSH", "KG");
			var alternateGLAccountWithAttributeSet = new AlternateGLAccountWithAttributeSet(alternateGLAccountWithAttributeSetDetails, Factory);
			alternateGLAccountWithAttributeSet.AlternateGLAccount = Creator.CreateAccAlternateGlAccount(Chart.PK, "1111", Core.Constants.AccountType.BalanceSheetAccount, totalLevel: 2, percentNum: Guid.NewGuid(), consolidate: Guid.NewGuid(), alternateNum: Guid.NewGuid());

			AssertEquals(false, alternateGLAccountWithAttributeSet.PercentNumInfo.ReadOnly);
			AssertEquals(false, alternateGLAccountWithAttributeSet.ConsolidationNumInfo.ReadOnly);
			AssertEquals(false, alternateGLAccountWithAttributeSet.AlternateNumInfo.ReadOnly);
			Assert(alternateGLAccountWithAttributeSet.PercentNum.IsValid);
			Assert(alternateGLAccountWithAttributeSet.ConsolidationNum.IsValid);
			Assert(alternateGLAccountWithAttributeSet.AlternateNum.IsValid);

			alternateGLAccountWithAttributeSet.AccountType = "HDR";
			AssertEquals(true, alternateGLAccountWithAttributeSet.PercentNumInfo.ReadOnly);
			AssertEquals(true, alternateGLAccountWithAttributeSet.ConsolidationNumInfo.ReadOnly);
			AssertEquals(true, alternateGLAccountWithAttributeSet.AlternateNumInfo.ReadOnly);
			AssertEquals(false, alternateGLAccountWithAttributeSet.PercentNum.IsValid);
			AssertEquals(false, alternateGLAccountWithAttributeSet.ConsolidationNum.IsValid);
			AssertEquals(false, alternateGLAccountWithAttributeSet.AlternateNum.IsValid);

			alternateGLAccountWithAttributeSet.HeaderDependsOnTotal = Guid.NewGuid();
			AssertEquals(false, alternateGLAccountWithAttributeSet.HeaderDependsOnTotalInfo.ReadOnly);
			Assert(alternateGLAccountWithAttributeSet.HeaderDependsOnTotal.IsValid);

			alternateGLAccountWithAttributeSet.AccountType = "TTL";
			alternateGLAccountWithAttributeSet.TotalLevel = 2;
			AssertEquals(true, alternateGLAccountWithAttributeSet.HeaderDependsOnTotalInfo.ReadOnly);
			AssertEquals(false, alternateGLAccountWithAttributeSet.HeaderDependsOnTotal.IsValid);
			AssertEquals(false, alternateGLAccountWithAttributeSet.TotalLevelInfo.ReadOnly);
			AssertEquals(alternateGLAccountWithAttributeSet.TotalLevel, 2);

			alternateGLAccountWithAttributeSet.AccountType = "NTE";
			AssertEquals(true, alternateGLAccountWithAttributeSet.TotalLevelInfo.ReadOnly);
			AssertEquals(alternateGLAccountWithAttributeSet.TotalLevel, 0);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Creator = new TestObjectCreator(Factory);

			Chart = Creator.CreateAlternateChart("MGT", "Management Reporting", true, false, BalanceSheetStyleCode.ELA);
			Creator.CreateAccAlternateChartFormat(Chart, 1, "9", "2", ".");
			Creator.CreateAccAlternateChartFormat(Chart, 2, "X99", "2", ".");

			GLHeader = Creator.CreateAccGLHeader("10.00.1010", AccGLHeader.Constants.SectionTypes.Codes.Overheads, "desc", Core.Constants.AccountType.BalanceSheetAccount, Core.Constants.DebitCredit.Debit);
			Factory.Save();
		}

		#endregion

		AccAlternateChart Chart;
		AccGLHeader GLHeader;
		TestObjectCreator Creator;
	}
}
