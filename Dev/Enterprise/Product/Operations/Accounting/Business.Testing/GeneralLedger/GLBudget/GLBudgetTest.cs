using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget.Testing
{
	[TestedType(typeof(GLBudget))]
	public class GLBudgetTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			AccGLHeader gLAccount = factory.LoadTop1(typeof(AccGLHeader), new ZQuery()) as AccGLHeader;
			GLBudget objectToReturn = factory.NewWithValidTestData(typeof(GLBudget)) as GLBudget;
			objectToReturn.AU_AG = gLAccount.PK;
			return objectToReturn;
		}

		public void TestZDecimalsHaveCorrectDecimalPlacesGLBudget()
		{
			AccGLHeader gLAccount = TestObjectCreator.CreateGLHeader();
			gLAccount.AG_AccountType = AccountTypesList.Codes.ProfitLoss;
			GLBudget testBudget = Factory.New<GLBudget>();
			testBudget.AU_AG = gLAccount.PK;

			var localList = new List<string>
				{
					nameof(testBudget.AU_AllocationValue),
					nameof(testBudget.AU_Opening),
					nameof(testBudget.AU_Closing),
					nameof(testBudget.OpeningBalance),
					nameof(testBudget.TotalAmount)
				};

			var percentList = new List<string>
				{
					nameof(testBudget.TotalPercentage),
					nameof(testBudget.AU_AllocationIncrement)
				};

			var tester = new DecimalPlacesAttributeTester(testBudget, testBudget.Branch.Company);
			tester.CheckLocalCurrency(localList, nameof(testBudget.Decimals));
			tester.CheckConstant(percentList, nameof(testBudget.PercentageDecimals), Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
		}

		public void TestAU_OpeningInfo()
		{
			AccGLHeader gLAccount = TestObjectCreator.CreateGLHeader();
			gLAccount.AG_AccountType = AccountTypesList.Codes.ProfitLoss;
			GLBudget testBudget = Factory.New<GLBudget>();
			Assert(testBudget.AU_OpeningInfo.ReadOnly);

			testBudget.AU_AG = gLAccount.PK;
			Assert(testBudget.AU_OpeningInfo.ReadOnly);

			gLAccount.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			Assert(!testBudget.AU_OpeningInfo.ReadOnly);
		}

		public void TestDefaultValues()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			AssertEquals(GlbBranch.CurrentBranch.PK, testBudget.AU_GB);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, testBudget.AU_GE);
			AssertEquals(GlbDepartment.CurrentDepartment.PK, testBudget.AU_GE);
			AssertEquals(ZDateTime.Now.Year, testBudget.AU_Year);
			AssertEquals(AllocationTypeList.Codes.none, testBudget.AU_AllocationType);
		}

		public void TestDefaultBudgetLines()
		{
			AccPeriodManagement period1 = Factory.New<AccPeriodManagement>();
			period1.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period1.AM_StartDate = ZDateTime.Now.AddDays(-1);
			period1.AM_EndDate = ZDateTime.Now.AddDays(1);
			period1.AM_Year = (ZShort)ZDateTime.Now.Year;
			period1.AM_Period = (ZDateTime.Now.Year * 100) + 1;

			AccPeriodManagement period2 = Factory.New<AccPeriodManagement>();
			period2.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period2.AM_StartDate = ZDateTime.Now.AddDays(2);
			period2.AM_EndDate = ZDateTime.Now.AddDays(3);
			period2.AM_Year = (ZShort)ZDateTime.Now.Year;
			period2.AM_Period = (ZDateTime.Now.Year * 100) + 2;

			Factory.Save();

			GLBudget testBudget = Factory.New<GLBudget>();
			AssertEquals(2, testBudget.BudgetLines.Count);
		}

		public void TestNewValidation()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			Assert(testBudget.Validation is GLBudgetValidation);
		}

		public void TestBudgetLineNotNull()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			AssertNotNull(testBudget.BudgetLines);
		}

		public void TestAllocationIncrementRecalculates()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			GLBudgetLine line1 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line2 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line3 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line4 = testBudget.BudgetLines.AddNew();

			AssertEquals(0m, line1.AD_Amount);
			AssertEquals(0m, line2.AD_Amount);
			AssertEquals(0m, line3.AD_Amount);
			AssertEquals(0m, line4.AD_Amount);

			testBudget.AU_AllocationType = AllocationTypeList.Codes.individual;
			testBudget.AU_AllocationValue = 100;
			testBudget.AU_AllocationIncrement = 10;

			AssertEquals(100m, line1.AD_Amount);
			AssertEquals(110m, line2.AD_Amount);
			AssertEquals(121m, line3.AD_Amount);
			AssertEquals(133.1m, line4.AD_Amount);
		}

		public void TestAllocationTypePermission()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			testBudget.AU_AllocationType = AllocationTypeList.Codes.none;
			Assert(testBudget.AU_AllocationValueInfo.ReadOnly);

			testBudget.AU_AllocationType = AllocationTypeList.Codes.byPeriod;
			Assert(!testBudget.AU_AllocationValueInfo.ReadOnly);

			testBudget.AU_AllocationType = AllocationTypeList.Codes.individual;
			Assert(!testBudget.AU_AllocationValueInfo.ReadOnly);

			testBudget.AU_AllocationType = AllocationTypeList.Codes.percentage;
			Assert(!testBudget.AU_AllocationValueInfo.ReadOnly);
		}

		public void TestAllocationValuePermission()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			testBudget.AU_AllocationType = AllocationTypeList.Codes.none;
			Assert(testBudget.AU_AllocationIncrementInfo.ReadOnly);

			testBudget.AU_AllocationType = AllocationTypeList.Codes.byPeriod;
			Assert(!testBudget.AU_AllocationIncrementInfo.ReadOnly);

			testBudget.AU_AllocationType = AllocationTypeList.Codes.individual;
			Assert(!testBudget.AU_AllocationIncrementInfo.ReadOnly);

			testBudget.AU_AllocationType = AllocationTypeList.Codes.percentage;
			Assert(testBudget.AU_AllocationIncrementInfo.ReadOnly);
		}

		public void TestAllocationValueAndIncrementForNon()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			testBudget.AU_AllocationType = AllocationTypeList.Codes.byPeriod;
			testBudget.AU_AllocationValue = 100m;
			testBudget.AU_AllocationIncrement = 10m;

			testBudget.AU_AllocationType = AllocationTypeList.Codes.none;

			AssertEquals(0m, testBudget.AU_AllocationValue);
			AssertEquals(0m, testBudget.AU_AllocationIncrement);
		}

		public void TestAllocationValueAndIncrementForPeriod()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			testBudget.AU_AllocationType = AllocationTypeList.Codes.none;
			testBudget.AU_AllocationValue = 100m;
			testBudget.AU_AllocationIncrement = 10m;

			testBudget.AU_AllocationType = AllocationTypeList.Codes.byPeriod;

			AssertEquals(100m, testBudget.AU_AllocationValue);
			AssertEquals(10m, testBudget.AU_AllocationIncrement);
		}

		public void TestAllocationValueAndIncrementForIndividual()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			testBudget.AU_AllocationType = AllocationTypeList.Codes.none;
			testBudget.AU_AllocationValue = 100m;
			testBudget.AU_AllocationIncrement = 10m;

			testBudget.AU_AllocationType = AllocationTypeList.Codes.individual;

			AssertEquals(100m, testBudget.AU_AllocationValue);
			AssertEquals(10m, testBudget.AU_AllocationIncrement);
		}

		public void TestAllocationValueAndIncrementForPercentage()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			testBudget.AU_AllocationType = AllocationTypeList.Codes.none;
			testBudget.AU_AllocationValue = 100m;
			testBudget.AU_AllocationIncrement = 10m;

			testBudget.AU_AllocationType = AllocationTypeList.Codes.percentage;

			AssertEquals(100m, testBudget.AU_AllocationValue);
			AssertEquals(0m, testBudget.AU_AllocationIncrement);
		}

		public void TestSetAllocationTypeRecalculates()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			GLBudgetLine line1 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line2 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line3 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line4 = testBudget.BudgetLines.AddNew();

			testBudget.AU_AllocationType = AllocationTypeList.Codes.individual;
			testBudget.AU_AllocationValue = 100;
			testBudget.AU_AllocationIncrement = 0;

			AssertEquals(100m, line1.AD_Amount);
			AssertEquals(100m, line2.AD_Amount);
			AssertEquals(100m, line3.AD_Amount);
			AssertEquals(100m, line4.AD_Amount);

			testBudget.AU_AllocationType = AllocationTypeList.Codes.byPeriod;

			AssertEquals(25m, line1.AD_Amount);
			AssertEquals(25m, line2.AD_Amount);
			AssertEquals(25m, line3.AD_Amount);
			AssertEquals(25m, line4.AD_Amount);

			testBudget.AU_AllocationType = AllocationTypeList.Codes.percentage;

			AssertEquals(0m, line1.AD_Amount);
			AssertEquals(0m, line2.AD_Amount);
			AssertEquals(0m, line3.AD_Amount);
			AssertEquals(0m, line4.AD_Amount);
		}

		public void TestAllocationValueRecalulates()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			GLBudgetLine line1 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line2 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line3 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line4 = testBudget.BudgetLines.AddNew();

			testBudget.AU_AllocationType = AllocationTypeList.Codes.individual;
			testBudget.AU_AllocationValue = 100;
			testBudget.AU_AllocationIncrement = 0;

			AssertEquals(100m, line1.AD_Amount);
			AssertEquals(100m, line2.AD_Amount);
			AssertEquals(100m, line3.AD_Amount);
			AssertEquals(100m, line4.AD_Amount);

			testBudget.AU_AllocationValue = 200;

			AssertEquals(200m, line1.AD_Amount);
			AssertEquals(200m, line2.AD_Amount);
			AssertEquals(200m, line3.AD_Amount);
			AssertEquals(200m, line4.AD_Amount);

			testBudget.AU_AllocationValue = 0;

			AssertEquals(0m, line1.AD_Amount);
			AssertEquals(0m, line2.AD_Amount);
			AssertEquals(0m, line3.AD_Amount);
			AssertEquals(0m, line4.AD_Amount);
		}

		public void TestAllocationValueRecalulatesWithPercentage()
		{
			GLBudget testBudget = Factory.New<GLBudget>();

			GLBudgetLine line1 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line2 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line3 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line4 = testBudget.BudgetLines.AddNew();

			testBudget.AU_AllocationType = AllocationTypeList.Codes.individual;
			testBudget.AU_AllocationValue = 100;
			testBudget.AU_AllocationIncrement = 10;

			AssertEquals(100m, line1.AD_Amount);
			AssertEquals(110m, line2.AD_Amount);
			AssertEquals(121m, line3.AD_Amount);
			AssertEquals(133.1m, line4.AD_Amount);

			testBudget.AU_AllocationValue = 200;

			AssertEquals(200m, line1.AD_Amount);
			AssertEquals(220m, line2.AD_Amount);
			AssertEquals(242m, line3.AD_Amount);
			AssertEquals(266.2m, line4.AD_Amount);
		}

		public void TestDebitCredit()
		{
			AccGLHeader debitGLHeader = TestObjectCreator.CreateGLHeader();
			debitGLHeader.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			debitGLHeader.AG_DebitCredit = DebitCreditTypeList.Codes.debit;

			AccGLHeader creditGLHeader = TestObjectCreator.CreateGLHeader();
			creditGLHeader.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			creditGLHeader.AG_DebitCredit = DebitCreditTypeList.Codes.credit;

			Factory.Save();

			GLBudget testBudget = Factory.New<GLBudget>();

			GLBudgetLine line1 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line2 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line3 = testBudget.BudgetLines.AddNew();
			GLBudgetLine line4 = testBudget.BudgetLines.AddNew();

			testBudget.AU_AG = debitGLHeader.PK;

			AssertEquals(DebitCreditTypeList.Codes.debit, line1.DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.debit, line2.DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.debit, line3.DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.debit, line4.DebitCreditSign);

			testBudget.AU_AG = creditGLHeader.PK;

			AssertEquals(DebitCreditTypeList.Codes.credit, line1.DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.credit, line2.DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.credit, line3.DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.credit, line4.DebitCreditSign);
		}

		public void TestAllocationTypePermissionforAU_AG()
		{
			AccGLHeader debitGLHeader = TestObjectCreator.CreateGLHeader();
			debitGLHeader.AG_AccountType = AccountTypesList.Codes.BalanceSheet;
			debitGLHeader.AG_DebitCredit = DebitCreditTypeList.Codes.debit;

			Factory.Save();
			GLBudget testBudget = Factory.New<GLBudget>();

			testBudget.AU_AG = debitGLHeader.PK;
			Assert(!testBudget.AU_AllocationTypeInfo.ReadOnly);
			Assert(testBudget.AU_AllocationIncrementInfo.ReadOnly);
			Assert(testBudget.AU_AllocationValueInfo.ReadOnly);

			testBudget.AU_AG = ZGuid.Empty;
			Assert(!testBudget.AU_AllocationTypeInfo.ReadOnly);
			Assert(testBudget.AU_AllocationIncrementInfo.ReadOnly);
			Assert(testBudget.AU_AllocationValueInfo.ReadOnly);
		}

		public void TestReadonlyWhenInDatabase()
		{
			GLBudget budget = Factory.NewWithValidTestData<GLBudget>();

			Assert(!budget.AU_YearInfo.ReadOnly);
			Assert(!budget.AU_AGInfo.ReadOnly);
			Assert(!budget.AU_GBInfo.ReadOnly);
			Assert(!budget.AU_GEInfo.ReadOnly);

			Factory.Save();

			Assert(budget.AU_YearInfo.ReadOnly);
			Assert(budget.AU_AGInfo.ReadOnly);
			Assert(budget.AU_GBInfo.ReadOnly);
			Assert(budget.AU_GEInfo.ReadOnly);
		}

		public void TestYearLoadsDifferentPeriods()
		{
			ZDateTime start200601 = new ZDateTime(2006, 1, 1);
			ZDateTime end200601 = new ZDateTime(2006, 1, 31);
			ZDateTime start200602 = new ZDateTime(2006, 2, 1);
			ZDateTime end200602 = new ZDateTime(2006, 2, 28);

			ZDateTime start200701 = new ZDateTime(2007, 1, 1);
			ZDateTime end200701 = new ZDateTime(2007, 1, 31);
			ZDateTime start200702 = new ZDateTime(2007, 2, 1);
			ZDateTime end200702 = new ZDateTime(2007, 2, 28);
			ZDateTime start200703 = new ZDateTime(2007, 3, 1);
			ZDateTime end200703 = new ZDateTime(2007, 3, 31);

			PeriodTestHelper.SetupSinglePeriod(200601, start200601, end200601);
			PeriodTestHelper.SetupSinglePeriod(200602, start200602, end200602);

			PeriodTestHelper.SetupSinglePeriod(200701, start200701, end200701);
			PeriodTestHelper.SetupSinglePeriod(200702, start200702, end200702);
			PeriodTestHelper.SetupSinglePeriod(200703, start200703, end200703);

			GLBudget budget = Factory.New<GLBudget>();

			budget.AU_Year = 2006;
			AssertEquals(2, budget.BudgetLines.Count);

			budget.AU_Year = 2007;
			AssertEquals(3, budget.BudgetLines.Count);
		}

		public void TestYearLoadLastYearDetail()
		{
			AccGLHeader gLAccount = TestObjectCreator.CreateGLHeader();

			ZDateTime start200601 = new ZDateTime(2006, 1, 1);
			ZDateTime end200601 = new ZDateTime(2006, 1, 31);
			ZDateTime start200602 = new ZDateTime(2006, 2, 1);
			ZDateTime end200602 = new ZDateTime(2006, 2, 28);

			ZDateTime start200701 = new ZDateTime(2007, 1, 1);
			ZDateTime end200701 = new ZDateTime(2007, 1, 31);
			ZDateTime start200702 = new ZDateTime(2007, 2, 1);
			ZDateTime end200702 = new ZDateTime(2007, 2, 28);
			ZDateTime start200703 = new ZDateTime(2007, 3, 1);
			ZDateTime end200703 = new ZDateTime(2007, 3, 31);

			PeriodTestHelper.SetupSinglePeriod(200601, start200601, end200601);
			PeriodTestHelper.SetupSinglePeriod(200602, start200602, end200602);

			PeriodTestHelper.SetupSinglePeriod(200701, start200701, end200701);
			PeriodTestHelper.SetupSinglePeriod(200702, start200702, end200702);
			PeriodTestHelper.SetupSinglePeriod(200703, start200703, end200703);

			GLBudget budget = Factory.New<GLBudget>();

			budget.AU_Year = 2006;
			AssertEquals(2, budget.BudgetLines.Count);
			AssertNotNull(budget.PreviousCollection);
			AssertEquals(0, budget.PreviousCollection.Count);

			budget.AU_GB = GlbBranch.CurrentBranch.PK;
			budget.AU_GE = GlbDepartment.CurrentDepartment.PK;
			budget.AU_AG = gLAccount.PK;

			Factory.Save();

			GLBudget newBudget = Factory.New<GLBudget>();

			newBudget.AU_Year = 2007;
			newBudget.AU_GB = GlbBranch.CurrentBranch.PK;
			newBudget.AU_GE = GlbDepartment.CurrentDepartment.PK;
			newBudget.AU_AG = gLAccount.PK;

			AssertEquals(3, newBudget.BudgetLines.Count);
			AssertNotNull(newBudget.PreviousCollection);
			AssertEquals(2, newBudget.PreviousCollection.Count);
		}

		public void TestNoStmAlog()
		{
			var budget = Factory.NewWithValidTestData<GLBudget>();
			budget.AU_Year = 2022;
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, budget.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				budget.AU_Year = 2023;
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				budget.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		#region Implementation

		protected decimal GetSumOfBudgetLines(GLBudget budget)
		{
			decimal sum = 0m;

			foreach (GLBudgetLine line in budget.BudgetLines)
			{
				sum += line.AD_Amount;
			}

			return sum;
		}

		protected void AssertDebitCreditColumnEquals(ZString expected, GLBudget budget)
		{
			Assert("So this is not empty", true);
		}

		AccountingPeriodTestHelper fAccountingPeriodTestHelper;
		AccountingPeriodTestHelper PeriodTestHelper
		{
			get
			{
				if (fAccountingPeriodTestHelper == null)
				{
					fAccountingPeriodTestHelper = new AccountingPeriodTestHelper(Factory);
				}
				return fAccountingPeriodTestHelper;
			}
		}

		TestObjectCreator fTestObjectCreator;
		TestObjectCreator TestObjectCreator
		{
			get
			{
				if (fTestObjectCreator == null)
				{
					fTestObjectCreator = new TestObjectCreator(Factory);
				}
				return fTestObjectCreator;
			}
		}

		#endregion
	}
}
