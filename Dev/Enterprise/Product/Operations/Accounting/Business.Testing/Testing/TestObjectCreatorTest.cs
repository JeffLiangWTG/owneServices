using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Testing
{
	public class TestObjectCreatorTest : TestCaseWithFactory
	{
		public void TestNonCurrentCompanyBranch()
		{
			TestObjectCreator testHelper = new TestObjectCreator(Factory);
			Assert(GlbCompany.CurrentCompany.PK != testHelper.NonCurrentCompanyBranch.GB_GC);
		}

		public void TestCreateAccountDescriptorWithGLHeader()
		{
			var testHelper = new TestObjectCreator(Factory);
			var glHeader = testHelper.CreateGLHeader();
			AssertNotNull(testHelper.CreateAccountDescriptor(null, "123", AccGLAccountDescriptor.ReportTypeCOA, "P&L", Core.SharedConstants.Languages.ChineseSimplified, "123", Constants.CountryCodes.China, Constants.DebitCredit.Credit).ParentGLHeader);
			AssertNull(testHelper.CreateAccountDescriptor(null, "123", "TST", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "123", Constants.CountryCodes.China, Constants.DebitCredit.Credit).ParentGLHeader);
			AssertEquals(glHeader.PK, testHelper.CreateAccountDescriptor(glHeader, "123", AccGLAccountDescriptor.ReportTypeCOA, "P&L", Core.SharedConstants.Languages.ChineseSimplified, "123", Constants.CountryCodes.China, Constants.DebitCredit.Credit).ParentGLHeader.PK);
			AssertNull(testHelper.CreateAccountDescriptor(glHeader, "123", "TST", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "123", Constants.CountryCodes.China, Constants.DebitCredit.Credit).ParentGLHeader);
		}

		public void TestCreateGLHeaderLoadBeforeCreate()
		{
			var factory1 = new BusinessObjectFactory();
			var creator1 = new TestObjectCreator(factory1, true);
			var glHeader1 = creator1.CreateGLHeader("XX01");
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var creator2 = new TestObjectCreator(factory2, true);
			var glHeader2 = creator2.CreateGLHeader("XX01");

			AssertEquals(glHeader2.PK, glHeader1.PK);
		}

		public void TestCreateAccGLHeaderLoadBeforeCreate()
		{
			var factory1 = new BusinessObjectFactory();
			var creator1 = new TestObjectCreator(factory1, true);
			var accGLHeader1 = creator1.CreateAccGLHeader("XX02", string.Empty, string.Empty, Core.Constants.AccountType.ProfitAndLossAccount, DebitCreditDataEntry.DR);
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var creator2 = new TestObjectCreator(factory2, true);
			var accGLHeader2 = creator2.CreateAccGLHeader("XX02", string.Empty, string.Empty, Core.Constants.AccountType.ProfitAndLossAccount, DebitCreditDataEntry.DR);

			AssertEquals(accGLHeader2.PK, accGLHeader1.PK);
		}

		public void TestCreateAUDCheckBookLoadBeforeCreate()
		{
			var factory1 = new BusinessObjectFactory();
			var creator1 = new TestObjectCreator(factory1, true);
			var chequeBook1 = creator1.AUDChequeBook;
			var bankAccount1 = creator1.AUDBankAccount;
			factory1.Save();

			var factory2 = new BusinessObjectFactory();
			var creator2 = new TestObjectCreator(factory2, true);
			var chequeBook2 = creator2.AUDChequeBook;
			var bankAccount2 = creator2.AUDBankAccount;

			AssertEquals(chequeBook2.PK, chequeBook1.PK);
			AssertEquals(bankAccount2.PK, bankAccount1.PK);
		}

		[TestDate(2010, 01, 20)]
		public void TestSubLedgerStatusWhenCreateTestPeriodsWithoutReopenPeriod()
		{
			TestSubLedgerWhenCreateTestPeriodsCore(false, false, true);
		}

		[TestDate(2010, 01, 20)]
		public void TestSubLedgerStatusWhenCreateTestPeriodsAndReopenPeriod()
		{
			TestSubLedgerWhenCreateTestPeriodsCore(true, false, false);
		}

		void TestSubLedgerWhenCreateTestPeriodsCore(bool isReopenPeriod, bool expectedIsSubLedgerClosedForNewPeriod, bool expectedIsSubLedgerClosedForLoadedPeriod)
		{
			var testObjectCreator = new TestObjectCreator(Factory);
			var calculator = new AccountingPeriodCalculator(Factory);

			var preConditionPeriod = new AccountingPeriodCalculator(Factory.CreateNewFactory()).GetPeriodManagementFromDate(ZDateTime.Today);
			AssertNull("PreCondition: period does not exist.", preConditionPeriod);

			testObjectCreator.CreateTestPeriods(ZDateTime.Today, null, isReopenPeriod);
			var period = calculator.GetPeriodManagementFromDate(ZDateTime.Today);
			AssertEquals("New period", expectedIsSubLedgerClosedForNewPeriod, period.AM_IsSubLedgerClosed);

			period.AM_IsSubLedgerClosed = true;
			Factory.Save();
			preConditionPeriod = new AccountingPeriodCalculator(Factory.CreateNewFactory()).GetPeriodManagementFromDate(ZDateTime.Today);
			AssertNotNull("PreCondition: period exist in DB.", preConditionPeriod);
			AssertEquals(true, preConditionPeriod.AM_IsSubLedgerClosed);

			testObjectCreator.CreateTestPeriods(ZDateTime.Today, null, isReopenPeriod);
			period = calculator.GetPeriodManagementFromDate(ZDateTime.Today);
			AssertEquals("Loaded period", expectedIsSubLedgerClosedForLoadedPeriod, period.AM_IsSubLedgerClosed);
		}
	}
}
