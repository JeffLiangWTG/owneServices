using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.GeneralLedger.GLBudget.Testing
{
	[TestedType(typeof(GLBudgetLineDependentCollection))]
	public class GLBudgetLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new GLBudgetLineDependentCollection(Factory.New<GLBudget>(), Factory);
		}

		public void TestRecalculateIndividualWithNoPercentage()
		{
			GLBudget parent = Factory.New<GLBudget>();
			parent.AU_AllocationType = AllocationTypeList.Codes.individual;

			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();

			AssertEquals(4, parent.BudgetLines.Count);

			parent.AU_AllocationValue = 120m;
			parent.AU_AllocationIncrement = 0m;

			parent.BudgetLines.RecalculateAmount();

			AssertEquals(120m, parent.BudgetLines[0].AD_Amount);
			AssertEquals(120m, parent.BudgetLines[1].AD_Amount);
			AssertEquals(120m, parent.BudgetLines[2].AD_Amount);
			AssertEquals(120m, parent.BudgetLines[3].AD_Amount);
		}

		public void TestRecalculateIndividualWithPercentageSet()
		{
			GLBudget parent = Factory.New<GLBudget>();
			parent.AU_AllocationType = AllocationTypeList.Codes.individual;

			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();

			AssertEquals(4, parent.BudgetLines.Count);

			parent.AU_AllocationValue = 120m;
			parent.AU_AllocationIncrement = 12m;

			parent.BudgetLines.RecalculateAmount();

			AssertEquals(120m, parent.BudgetLines[0].AD_Amount);
			AssertEquals(134.4m, parent.BudgetLines[1].AD_Amount);
			AssertEquals(150.528m, parent.BudgetLines[2].AD_Amount);
			AssertEquals(168.5914m, parent.BudgetLines[3].AD_Amount);
		}

		public void TestRecalculatePeriodWithPercentageNotSet()
		{
			GLBudget parent = Factory.New<GLBudget>();
			parent.AU_AllocationType = AllocationTypeList.Codes.byPeriod;

			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();

			AssertEquals(4, parent.BudgetLines.Count);

			parent.AU_AllocationValue = 120m;
			parent.AU_AllocationIncrement = 0m;

			parent.BudgetLines.RecalculateAmount();

			AssertEquals(30m, parent.BudgetLines[0].AD_Amount);
			AssertEquals(30m, parent.BudgetLines[1].AD_Amount);
			AssertEquals(30m, parent.BudgetLines[2].AD_Amount);
			AssertEquals(30m, parent.BudgetLines[3].AD_Amount);
		}

		public void TestRecalculatePeriodWithPercentageSet()
		{
			GLBudget parent = Factory.New<GLBudget>();
			parent.AU_AllocationType = AllocationTypeList.Codes.byPeriod;

			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();

			AssertEquals(12, parent.BudgetLines.Count);

			parent.AU_AllocationValue = 120m;
			parent.AU_AllocationIncrement = 12m;

			parent.BudgetLines.RecalculateAmount();

			AssertEquals(4.97m, parent.BudgetLines[0].AD_Amount);
			AssertEquals(5.57m, parent.BudgetLines[1].AD_Amount);
			AssertEquals(6.24m, parent.BudgetLines[2].AD_Amount);
			AssertEquals(6.99m, parent.BudgetLines[3].AD_Amount);
			AssertEquals(7.82m, parent.BudgetLines[4].AD_Amount);
			AssertEquals(8.76m, parent.BudgetLines[5].AD_Amount);
			AssertEquals(9.81m, parent.BudgetLines[6].AD_Amount);
			AssertEquals(10.99m, parent.BudgetLines[7].AD_Amount);
			AssertEquals(12.31m, parent.BudgetLines[8].AD_Amount);
			AssertEquals(13.79m, parent.BudgetLines[9].AD_Amount);
			AssertEquals(15.44m, parent.BudgetLines[10].AD_Amount);
			AssertEquals(17.31m, parent.BudgetLines[11].AD_Amount);
		}

		public void TestPadTheLastPeriod_ByPeriod()
		{
			AccGLHeader gLAccount1 = TestObjectCreator.CreateGLHeader();
			gLAccount1.AG_DebitCredit = DebitCreditTypeList.Codes.debit;

			GLBudget parent = Factory.New<GLBudget>();
			parent.AU_AllocationType = AllocationTypeList.Codes.byPeriod;

			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();

			parent.AU_AllocationValue = 120m;
			parent.AU_AllocationIncrement = 12m;

			parent.BudgetLines.RecalculateAmount();

			AssertEquals(35.56m, parent.BudgetLines[0].AD_Amount);
			AssertEquals(39.83m, parent.BudgetLines[1].AD_Amount);
			AssertEquals(44.61m, parent.BudgetLines[2].AD_Amount);

			AssertEquals(35.56m, parent.BudgetLines[0].UnsignedAmount);
			AssertEquals(39.83m, parent.BudgetLines[1].UnsignedAmount);
			AssertEquals(44.61m, parent.BudgetLines[2].UnsignedAmount);

			AssertEquals(DebitCreditTypeList.Codes.debit, parent.BudgetLines[0].DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.debit, parent.BudgetLines[1].DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.debit, parent.BudgetLines[2].DebitCreditSign);

			parent.AU_AllocationValue = -120m;

			AssertEquals(-35.56m, parent.BudgetLines[0].AD_Amount);
			AssertEquals(-39.83m, parent.BudgetLines[1].AD_Amount);
			AssertEquals(-44.61m, parent.BudgetLines[2].AD_Amount);

			AssertEquals(35.56m, parent.BudgetLines[0].UnsignedAmount);
			AssertEquals(39.83m, parent.BudgetLines[1].UnsignedAmount);
			AssertEquals(44.61m, parent.BudgetLines[2].UnsignedAmount);

			AssertEquals(DebitCreditTypeList.Codes.credit, parent.BudgetLines[0].DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.credit, parent.BudgetLines[1].DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.credit, parent.BudgetLines[2].DebitCreditSign);
		}

		public void TestNegativeAllocationAmount_ForIndividual()
		{
			AccGLHeader gLAccount1 = TestObjectCreator.CreateGLHeader();
			gLAccount1.AG_DebitCredit = DebitCreditTypeList.Codes.debit;

			GLBudget parent = Factory.New<GLBudget>();
			parent.AU_AllocationType = AllocationTypeList.Codes.individual;

			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();
			parent.BudgetLines.AddNew();

			parent.AU_AllocationValue = 120m;
			parent.AU_AllocationIncrement = 12m;

			parent.BudgetLines.RecalculateAmount();

			AssertEquals(120.00m, parent.BudgetLines[0].AD_Amount);
			AssertEquals(134.4m, parent.BudgetLines[1].AD_Amount);
			AssertEquals(150.5280m, parent.BudgetLines[2].AD_Amount);

			AssertEquals(120.00m, parent.BudgetLines[0].UnsignedAmount);
			AssertEquals(134.4m, parent.BudgetLines[1].UnsignedAmount);
			AssertEquals(150.5280m, parent.BudgetLines[2].UnsignedAmount);

			AssertEquals(DebitCreditTypeList.Codes.debit, parent.BudgetLines[0].DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.debit, parent.BudgetLines[1].DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.debit, parent.BudgetLines[2].DebitCreditSign);

			parent.AU_AllocationValue = -120m;

			AssertEquals(-120.00m, parent.BudgetLines[0].AD_Amount);
			AssertEquals(-134.4m, parent.BudgetLines[1].AD_Amount);
			AssertEquals(-150.5280m, parent.BudgetLines[2].AD_Amount);

			AssertEquals(120.00m, parent.BudgetLines[0].UnsignedAmount);
			AssertEquals(134.4m, parent.BudgetLines[1].UnsignedAmount);
			AssertEquals(150.5280m, parent.BudgetLines[2].UnsignedAmount);

			AssertEquals(DebitCreditTypeList.Codes.credit, parent.BudgetLines[0].DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.credit, parent.BudgetLines[1].DebitCreditSign);
			AssertEquals(DebitCreditTypeList.Codes.credit, parent.BudgetLines[2].DebitCreditSign);
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
