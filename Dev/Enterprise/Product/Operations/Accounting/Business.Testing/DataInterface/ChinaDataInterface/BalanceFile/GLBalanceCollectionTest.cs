using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.DataInterface.ChinaDataInterface
{
	public class GLBalanceCollectionTest : TestCaseWithFactory
	{
		public void TestLoad()
		{
			GLBalanceCollection balanceCollection = new GLBalanceCollection(Factory);
			balanceCollection.LoadCollection(200403);
			AssertEquals("Collection Count should be 4", 4, balanceCollection.Count);
			GLBalance balance = FindGLBalance(GLAccountNum1, balanceCollection);
			AssertEquals(-120m, balance.BalanceAmount);
			balance = FindGLBalance(GLAccountNum2, balanceCollection);
			AssertEquals(-280m, balance.BalanceAmount);
			balance = FindGLBalance(GLAccountNum3, balanceCollection);
			AssertEquals(-40m, balance.BalanceAmount);
			balance = FindGLBalance(GLAccountNum4, balanceCollection);
			AssertEquals(0m, balance.BalanceAmount);
		}

		[ExpectNoExceptions()]
		public void TestGetFirstLocalAccountDescription()
		{
			GLBalanceCollection balanceCollection = new GLBalanceCollection(Factory);
			balanceCollection.GetFirstLocalAccountDescription();
		}

		[ExpectNoExceptions()]
		public void TestGetFirstLastAccountDescription()
		{
			GLBalanceCollection balanceCollection = new GLBalanceCollection(Factory);
			balanceCollection.GetLastLocalAccountDescription();
		}

		public void TestPLAppropriationAccount()
		{
			var mockBalanceCollection = new Mock<GLBalanceCollection>(Factory) { CallBase = true };
			mockBalanceCollection.Protected().Setup<ZGuid>("GLAccountSecondReportStartsFrom").Returns(GLLocalPK2.PK);
			AssertEquals("Expected PL Appropriation No.", GLAccountNum2, mockBalanceCollection.Object.PLAppropriationAccount);
		}

		public void TestGetAccountRanges()
		{
			GLLocalPK2.AJ_Language = Core.Constants.Languages.ChineseSimplified;
			GLLocalPK2.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			GLLocalPK2.AJ_LocalAccountNumber = GLAccountNum2;
			GLLocalPK2.AJ_ReportCategory = "HDR";
			GLLocalPK3.AJ_Language = Core.Constants.Languages.ChineseSimplified;
			GLLocalPK3.AJ_RN_NKCountryOfCompliance = Constants.CountryCodes.China;
			GLLocalPK3.AJ_LocalAccountNumber = GLAccountNum3;
			GLLocalPK3.AJ_ReportCategory = "HDR";
			GLLocalPK2.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			GLLocalPK3.AJ_ReportType = AccGLAccountDescriptor.ReportTypeCOA;
			Factory.Save();
			ReportOrderCollection reportOrders = AccountingConfigurationRegistry.Instance.ReportOrder.Value;
			ReportOrder reportOrder = reportOrders.FindByLanguageAndCountryCode(Core.Constants.Languages.ChineseSimplified, Constants.CountryCodes.China);
			if (reportOrder == null)
			{
				reportOrders.RemoveAndDeleteAll();
				reportOrder = reportOrders.AddNew();
				reportOrder.Language = Core.Constants.Languages.ChineseSimplified;
				reportOrder.CountryCode = Constants.CountryCodes.China;
			}

			reportOrder.GLAccountSecondReportStartsFrom = GLLocalPK3.PK;
			reportOrder.AccountsOrderBeginsWith = nameof(AccountOrderType.BalanceSheet);
			AccountingConfigurationRegistry.Instance.ReportOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportOrders);
			GLBalanceCollection balanceCollection = new GLBalanceCollection(Factory);
			balanceCollection.GetAccountRanges();
			AssertEquals("BSHStartAccount", GLAccountNum1, balanceCollection.BSHStartAccount);
			AssertEquals("BSHEndAccount", GLAccountNum2, balanceCollection.BSHEndAccount);
			AssertEquals("PLStartAccount", GLAccountNum3, balanceCollection.PLStartAccount);
			AssertEquals("PLEndAccount", GLAccountNum4, balanceCollection.PLEndAccount);
			reportOrder.AccountsOrderBeginsWith = nameof(AccountOrderType.ProfitAndLoss);
			AccountingConfigurationRegistry.Instance.ReportOrder.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, reportOrders);
			balanceCollection = new GLBalanceCollection(Factory);
			balanceCollection.GetAccountRanges();
			AssertEquals("BSHStartAccount", GLAccountNum3, balanceCollection.BSHStartAccount);
			AssertEquals("BSHEndAccount", GLAccountNum4, balanceCollection.BSHEndAccount);
			AssertEquals("PLStartAccount", GLAccountNum1, balanceCollection.PLStartAccount);
			AssertEquals("PLEndAccount", GLAccountNum2, balanceCollection.PLEndAccount);
		}

		AccGLHeader[] GLHeader;
		string GLAccountNum1, GLAccountNum2, GLAccountNum3, GLAccountNum4;
		AccGLAccountDescriptor GLLocalPK2, GLLocalPK3;
		readonly ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
		protected override void SetUp()
		{
			base.SetUp();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			SetupAggregate();
		}

		protected override void TearDown()
		{
			base.TearDown();
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		void SetupAggregate()
		{
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.SetupSinglePeriod(200401, new ZDateTime(2004, 1, 1), new ZDateTime(2004, 1, 31));
			testHelper.SetupSinglePeriod(200402, new ZDateTime(2004, 2, 1), new ZDateTime(2004, 2, 28));
			testHelper.SetupSinglePeriod(200403, new ZDateTime(2004, 3, 1), new ZDateTime(2004, 3, 31));
			testHelper.SetupSinglePeriod(200301, new ZDateTime(2003, 1, 1), new ZDateTime(2003, 1, 31));
			testHelper.SetupSinglePeriod(200302, new ZDateTime(2003, 2, 1), new ZDateTime(2003, 2, 28));
			testHelper.SetupSinglePeriod(200303, new ZDateTime(2003, 3, 1), new ZDateTime(2003, 3, 31));
			ZQuery filter = new ZQuery();
			filter.MaximumRows = 4;
			filter.OrderBy = AccGLHeaderSchema.Constants.AG_AccountNum;
			filter.AddToFilter(AccGLHeaderSchema.AG_AccountType, Constants.AccountType.ProfitAndLossAccount);
			GLHeader = Factory.Load(typeof(AccGLHeader), filter) as AccGLHeader[];
			GLAccountNum1 = "Chinese1";
			GLAccountNum2 = "Chinese2";
			GLAccountNum3 = "Chinese3";
			GLAccountNum4 = "Chinese4";
			TestUtils.AddGLHeaderDescriptor(Factory, GLHeader[0].PK, GLAccountNum1, GLAccountNum1, GLHeader[0].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);
			GLLocalPK2 = TestUtils.AddGLHeaderDescriptor(Factory, GLHeader[1].PK, GLAccountNum2, GLAccountNum2, GLHeader[1].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);
			GLLocalPK3 = TestUtils.AddGLHeaderDescriptor(Factory, GLHeader[2].PK, GLAccountNum3, GLAccountNum3, GLHeader[2].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);
			TestUtils.AddGLHeaderDescriptor(Factory, GLHeader[3].PK, GLAccountNum4, GLAccountNum4, GLHeader[3].AG_DebitCredit, Constants.AccountType.ProfitAndLossAccount);
			InsertAccGLAggregate(GLHeader[0], 120, 200301);
			InsertAccGLAggregate(GLHeader[1], 50, 200301);
			InsertAccGLAggregate(GLHeader[2], 10, 200301);
			InsertAccGLAggregate(GLHeader[1], 80, 200302);
			InsertAccGLAggregate(GLHeader[1], 150, 200301);
			InsertAccGLAggregate(GLHeader[2], 30, 200302);
			InsertAccGLAggregate(GLHeader[2], 30, 200202);
			Factory.Save();
		}

		AccGLAggregate InsertAccGLAggregate(AccGLHeader gLHeader, decimal amount, int period)
		{
			AccGLAggregate aggregateRow = Factory.New(typeof(AccGLAggregate)) as AccGLAggregate;
			aggregateRow.AA_AG = gLHeader.PK;
			aggregateRow.AA_GB = GlbBranch.CurrentBranch.PK;
			aggregateRow.AA_GE = GlbDepartment.CurrentDepartment.PK;
			aggregateRow.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregateRow.AA_Period = period;
			aggregateRow.AA_Amount = amount;
			return aggregateRow;
		}

		GLBalance FindGLBalance(string accountNum, GLBalanceCollection balanceCollection)
		{
			foreach (GLBalance balance in balanceCollection)
			{
				if (balance.GLAccountNumber == accountNum)
				{
					return balance;
				}
			}

			return null;
		}
	}
}
