using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget.Testing
{
	[TestedType(typeof(GLBudgetLine))]
	public class GLBudgetLineTest : EnterpriseBusinessObjectTestCase
	{
		public void TestZDecimalsHaveCorrectDecimalPlacesGLBudgetLine()
		{
			GLBudgetLine line = Factory.New<GLBudgetLine>();
			AccGLHeader gLHeader = TestObjectCreator.CreateGLHeader();

			GLBudget budget = Factory.New<GLBudget>();
			budget.AU_AG = gLHeader.PK;
			budget.AU_GB = GlbBranch.CurrentBranch.PK;
			budget.AU_GE = GlbDepartment.CurrentDepartment.PK;

			line.AD_AU = budget.PK;
			var localList = new List<string>
				{
					nameof(line.LastYearActual),
					nameof(line.LastYearBudget),
					nameof(line.LastYearBudget_SignedAmount),
					nameof(line.UnsignedAmount),
					nameof(line.AD_Amount)
				};

			var percentList = new List<string>
				{
					nameof(line.AD_Percent)
				};

			var tester = new DecimalPlacesAttributeTester(line);
			tester.CheckLocalCurrency(localList, nameof(line.LocalDecimals));
			tester.CheckConstant(percentList, nameof(line.PercentageDecimals), Core.Constants.DecimalPlaces.DefaultNumberOfDecimalsForPercentages);
		}

		public void TestGetLastYearPeriod()
		{
			ZShort thisYear = 2006;
			ZInt currentPeriod = 200607;

			GLBudgetLine line = Factory.New<GLBudgetLine>();
			ZInt lastYearPeriod = line.GetLastYearPeriod_ForTestOnly(currentPeriod);

			AssertEquals(200507, lastYearPeriod);
		}

		[TestDate(2006, 12, 25)]
		public void TestLastYearActual()
		{
			ZInt testPeriod = 200605;
			AccountingPeriodTestHelper periodTestHelper = new AccountingPeriodTestHelper(Factory);

			AccPeriodManagement period1 = Factory.New<AccPeriodManagement>();
			AccPeriodManagement period2 = Factory.New<AccPeriodManagement>();
			AccPeriodManagement period3 = Factory.New<AccPeriodManagement>();
			AccPeriodManagement period4 = Factory.New<AccPeriodManagement>();
			AccPeriodManagement period5 = Factory.New<AccPeriodManagement>();

			AccGLHeader gLHeader1 = TestObjectCreator.CreateGLHeader();
			AccGLHeader gLHeader2 = TestObjectCreator.CreateGLHeader();

			period1.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period1.AM_Year = 2005;
			period1.AM_Period = 200505;
			period1.AM_StartDate = new ZDateTime(2005, 05, 01);
			period1.AM_EndDate = new ZDateTime(2005, 05, 30);

			period2.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period2.AM_Year = 2005;
			period2.AM_Period = 200506;
			period2.AM_StartDate = new ZDateTime(2005, 06, 01);
			period2.AM_EndDate = new ZDateTime(2005, 06, 30);

			period3.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period3.AM_Year = 2005;
			period3.AM_Period = 200507;
			period3.AM_StartDate = new ZDateTime(2005, 07, 01);
			period3.AM_EndDate = new ZDateTime(2005, 07, 30);

			period4.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period4.AM_Year = 2006;
			period4.AM_Period = 200605;
			period4.AM_StartDate = new ZDateTime(2006, 05, 01);
			period4.AM_EndDate = new ZDateTime(2006, 05, 30);

			period5.AM_GC_Company = GlbCompany.CurrentCompany.PK;
			period5.AM_Year = 2006;
			period5.AM_Period = 200606;
			period5.AM_StartDate = new ZDateTime(2006, 06, 01);
			period5.AM_EndDate = new ZDateTime(2006, 06, 30);

			AccGLAggregate aggregate1 = Factory.New<AccGLAggregate>();
			aggregate1.AA_AG = gLHeader1.PK;
			aggregate1.AA_GE = GlbDepartment.CurrentDepartment.PK;
			aggregate1.AA_GB = GlbBranch.CurrentBranch.PK;
			aggregate1.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregate1.AA_Period = 200505;
			aggregate1.AA_Amount = 120m;

			AccGLAggregate aggregate2 = Factory.New<AccGLAggregate>();
			aggregate2.AA_AG = gLHeader1.PK;
			aggregate2.AA_GE = GlbDepartment.CurrentDepartment.PK;
			aggregate2.AA_GB = GlbBranch.CurrentBranch.PK;
			aggregate2.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregate2.AA_Period = 200505;
			aggregate2.AA_Amount = 156m;

			AccGLAggregate aggregate3 = Factory.New<AccGLAggregate>();
			aggregate3.AA_AG = gLHeader1.PK;
			aggregate3.AA_GE = GlbDepartment.CurrentDepartment.PK;
			aggregate3.AA_GB = GlbBranch.CurrentBranch.PK;
			aggregate3.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregate3.AA_Period = 200605;
			aggregate3.AA_Amount = -356m;

			AccGLAggregate aggregate4 = Factory.New<AccGLAggregate>();
			aggregate4.AA_AG = gLHeader2.PK;
			aggregate4.AA_GE = GlbDepartment.CurrentDepartment.PK;
			aggregate4.AA_GB = GlbBranch.CurrentBranch.PK;
			aggregate4.AA_GC = GlbCompany.CurrentCompany.PK;
			aggregate4.AA_Period = 200605;
			aggregate4.AA_Amount = 402m;

			Factory.Save();

			GLBudget budget = Factory.New<GLBudget>();
			budget.AU_AG = gLHeader1.PK;
			budget.AU_GB = GlbBranch.CurrentBranch.PK;
			budget.AU_GE = GlbDepartment.CurrentDepartment.PK;

			GLBudgetLine lineToTest = budget.BudgetLines.AddNew();
			lineToTest.AD_Period = 200605;
			AssertEquals(276m, lineToTest.LastYearActual);
			AssertEquals(276m, lineToTest.LastYearActualWithoutDebitCredit_ForTestOnly);

			AssertEquals(DebitCreditTypeList.Codes.debit, lineToTest.LastYearActualDebitCredit);

			budget.AU_Year = 2007;
			GLBudgetLine creditLineToTest = budget.BudgetLines.AddNew();
			creditLineToTest.AD_Period = 200705;
			AssertEquals(356m, creditLineToTest.LastYearActual);

			AssertEquals(DebitCreditTypeList.Codes.credit, creditLineToTest.LastYearActualDebitCredit);
			AssertEquals(-356m, creditLineToTest.LastYearActualWithoutDebitCredit_ForTestOnly);
		}

		public void TestLastYearActualDebitCreditForZeroAmount()
		{
			AccGLHeader gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			gLHeader.AG_DebitCredit = DebitCreditTypeList.Codes.credit;

			GLBudget budget = Factory.New<GLBudget>();
			budget.AU_AG = gLHeader.PK;
			budget.AU_GB = GlbBranch.CurrentBranch.PK;
			budget.AU_GE = GlbDepartment.CurrentDepartment.PK;

			GLBudgetLine creditLineToTest = budget.BudgetLines.AddNew();

			creditLineToTest.AD_Period = 200705;
			AssertEquals(0m, creditLineToTest.LastYearActual);
			AssertEquals(DebitCreditTypeList.Codes.credit, creditLineToTest.LastYearActualDebitCredit);
			AssertEquals(0m, creditLineToTest.LastYearActualWithoutDebitCredit_ForTestOnly);

			gLHeader.AG_DebitCredit = DebitCreditTypeList.Codes.debit;

			GLBudgetLine debitLineToTest = budget.BudgetLines.AddNew();

			debitLineToTest.AD_Period = 200705;
			AssertEquals(0m, debitLineToTest.LastYearActual);
			AssertEquals(DebitCreditTypeList.Codes.debit, debitLineToTest.LastYearActualDebitCredit);
			AssertEquals(0m, debitLineToTest.LastYearActualWithoutDebitCredit_ForTestOnly);
		}

		[TestDate(2006, 12, 25)]
		public void TestGetLastYearBudget()
		{
			AccGLHeader gLHeader1 = TestObjectCreator.CreateGLHeader();
			AccGLHeader gLHeader2 = TestObjectCreator.CreateGLHeader();

			GLBudget lastYearBudget = Factory.NewWithValidTestData<GLBudget>();
			lastYearBudget.AU_AG = gLHeader1.PK;
			lastYearBudget.AU_Year = 2005;

			GLBudget lastYearBudget2 = Factory.NewWithValidTestData<GLBudget>();
			lastYearBudget2.AU_AG = gLHeader2.PK;
			lastYearBudget2.AU_Year = 2005;

			GLBudgetLine lastYearLine = lastYearBudget.BudgetLines.AddNew();
			lastYearLine.AD_Period = 200505;
			lastYearLine.AD_Amount = 154m;

			GLBudgetLine lastYearLine2 = lastYearBudget.BudgetLines.AddNew();
			lastYearLine2.AD_Period = 200506;
			lastYearLine2.AD_Amount = 73m;

			GLBudgetLine lastYearLine3 = lastYearBudget2.BudgetLines.AddNew();
			lastYearLine3.AD_Period = 200505;
			lastYearLine3.AD_Amount = 85m;

			Factory.Save();

			GLBudget thisYearBudget = Factory.New<GLBudget>();
			thisYearBudget.AU_AG = lastYearBudget.AU_AG;
			thisYearBudget.AU_GB = lastYearBudget.AU_GB;
			thisYearBudget.AU_GE = lastYearBudget.AU_GE;

			GLBudgetLine thisYearLine = thisYearBudget.BudgetLines.AddNew();
			thisYearLine.AD_Period = 200605;
			AssertNotNull(thisYearLine.LastYearBudgetLine_ForTestOnly);
			AssertEquals(154m, thisYearLine.LastYearBudgetLine_ForTestOnly.AD_Amount);
		}

		public void TestLastYearBudgetZero()
		{
			AccGLHeader gLHeader1 = TestObjectCreator.CreateGLHeader();
			AccGLHeader gLHeader2 = TestObjectCreator.CreateGLHeader();
			gLHeader1.AG_DebitCredit = DebitCreditTypeList.Codes.debit;

			GLBudget thisYearBudget = Factory.NewWithValidTestData<GLBudget>();
			thisYearBudget.AU_AG = gLHeader1.PK;
			thisYearBudget.AU_Year = 2006;

			GLBudgetLine line = thisYearBudget.BudgetLines.AddNew();
			line.AD_Period = 200607;

			AssertEquals(0m, line.LastYearBudget);
			AssertEquals(DebitCreditTypeList.Codes.debit, line.LastYearBudgetDebitCredit);

			gLHeader1.AG_DebitCredit = DebitCreditTypeList.Codes.credit;

			GLBudgetLine line2 = thisYearBudget.BudgetLines.AddNew();
			line2.AD_Period = 200607;
			AssertEquals(0m, line2.LastYearBudget);
			AssertEquals(DebitCreditTypeList.Codes.credit, line2.LastYearBudgetDebitCredit);
		}

		public void TestReadOnlyForPercentInfo()
		{
			GLBudget thisYearBudget = Factory.NewWithValidTestData<GLBudget>();
			GLBudgetLine line = thisYearBudget.BudgetLines.AddNew();

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.individual;
			Assert(line.AD_PercentInfo.ReadOnly);

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.byPeriod;
			Assert(line.AD_PercentInfo.ReadOnly);

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.none;
			Assert(line.AD_PercentInfo.ReadOnly);

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.percentage;
			Assert(!line.AD_PercentInfo.ReadOnly);
		}

		public void TestReadOnlyForUnsignedAmountInfo()
		{
			GLBudget thisYearBudget = Factory.NewWithValidTestData<GLBudget>();
			GLBudgetLine line = thisYearBudget.BudgetLines.AddNew();

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.individual;
			Assert(!line.UnsignedAmountInfo.ReadOnly);

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.byPeriod;
			Assert(!line.UnsignedAmountInfo.ReadOnly);

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.none;
			Assert(!line.UnsignedAmountInfo.ReadOnly);

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.percentage;
			Assert(line.UnsignedAmountInfo.ReadOnly);
		}

		public void TestReadOnlyForDebitCreditInfo()
		{
			GLBudget thisYearBudget = Factory.NewWithValidTestData<GLBudget>();
			GLBudgetLine line = thisYearBudget.BudgetLines.AddNew();

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.individual;
			Assert(!line.DebitCreditSignInfo.ReadOnly);

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.byPeriod;
			Assert(!line.DebitCreditSignInfo.ReadOnly);

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.none;
			Assert(!line.DebitCreditSignInfo.ReadOnly);

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.percentage;
			Assert(line.DebitCreditSignInfo.ReadOnly);
		}

		public void TestSetADAmountIfPercentageSet()
		{
			GLBudget thisYearBudget = Factory.NewWithValidTestData<GLBudget>();
			GLBudgetLine line = thisYearBudget.BudgetLines.AddNew();

			thisYearBudget.AU_AllocationType = AllocationTypeList.Codes.percentage;
			thisYearBudget.AU_AllocationValue = 120m;

			line.DebitCreditSign = DebitCreditTypeList.Codes.credit;
			line.AD_Percent = 12m;
			AssertEquals(14.4m, line.UnsignedAmount);
			AssertEquals(-14.4m, line.AD_Amount);
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
	}
}
