using CargoWise.EntityFramework;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using NUnit.Framework;

	[TestedType(typeof(GLAccountBalancesAndMovements))]
	public class GLAccountBalancesAndMovementsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T205", GLAccountBalancesAndMovements.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new GLAccountBalancesAndMovements();
		}
	}

	[TestedType(typeof(GLAccountBalancesAndMovementsCollection))]
	public class GLAccountBalancesAndMovementsCollectionTest : NonPersistentBusinessObjectCollectionTestCase<GLAccountBalancesAndMovementsCollection>
	{
		[TestDate(2018, 01, 01)]
		public void TestAddElementsAndClassProperties()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.China))
				using (GlbCompany.CurrentCompany.TemporarilySetCurrency(Core.Constants.CurrencyCodes.Australia))
				{
					AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
					testHelper.PostPeriodsForEntireYear(2008, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
					testHelper.PostPeriodsForEntireYear(2009, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
					testHelper.PostPeriodsForEntireYear(2010, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
					TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
					AccGLHeader glHeader0PNL = testObjectCreator.CreateAccGLHeader("1100.90.00", "TS", "Test PNL 0", "P&L", Constants.DebitCredit.Debit);
					AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.93.95", "TS", "Test PNL 1", "P&L", Constants.DebitCredit.Debit);
					AccGLHeader glHeader2PNL = testObjectCreator.CreateAccGLHeader("1100.93.96", "TS", "Test PNL 2", "P&L", Constants.DebitCredit.Debit);
					var longDescription = "This is a loooooooooooooooooooooonoooog Description.";
					testObjectCreator.CreateAccountDescriptor(glHeader0PNL, "6100.000", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccountDescriptor(glHeader1PNL, "6100.095", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccountDescriptor(glHeader2PNL, "6100.096", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, longDescription, "CN", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccGLAggregate(1001m, 201012, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(1001m, 201012, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					AccGLHeader glHeader3PNL = testObjectCreator.CreateAccGLHeader("1200.03.97", "TS", "Test PNL 3", "P&L", Constants.DebitCredit.Debit);
					AccGLHeader glHeader4PNL = testObjectCreator.CreateAccGLHeader("1200.03.98", "TS", "Test PNL 4", "P&L", Constants.DebitCredit.Credit);
					testObjectCreator.CreateAccountDescriptor(glHeader3PNL, "6100.097", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccountDescriptor(glHeader4PNL, "5300.098", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
					testObjectCreator.CreateAccGLAggregate(1002m, 201012, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(-1012m, 201012, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(1101m, 201001, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(-1201m, 201001, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(101m, 200911, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(201m, 200911, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(301m, 200911, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(-201m, 200911, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					AccGLHeader glHeader0 = testObjectCreator.CreateAccGLHeader("5300.90.00", "AS", "Test BSH 0", "BSH", Constants.DebitCredit.Debit);
					AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.93.95", "AS", "Test BSH 1", "BSH", Constants.DebitCredit.Debit);
					AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.93.96", "AS", "Test BSH 2", "BSH", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccountDescriptor(glHeader0, "5300.000", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccountDescriptor(glHeader1, "5300.095", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccountDescriptor(glHeader2, "5300.096", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccGLAggregate(1003m, 201012, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(1003m, 201012, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					AccGLHeader glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.97", "AS", "Test BSH 3", "BSH", Constants.DebitCredit.Debit);
					AccGLHeader glHeader4 = testObjectCreator.CreateAccGLHeader("5300.03.98", "AS", "Test BSH 4", "BSH", Constants.DebitCredit.Credit);
					testObjectCreator.CreateAccountDescriptor(glHeader3, "5300.097", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccountDescriptor(glHeader4, "5300.098", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
					testObjectCreator.CreateAccGLAggregate(1004m, 201012, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(-1104m, 201012, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(1100m, 201001, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(-1200m, 201001, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(100m, 200912, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(200m, 200912, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(300m, 200912, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					testObjectCreator.CreateAccGLAggregate(-350m, 200912, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
					var list = new GLPresentationJournalCategoryCollection();
					var category = list.AddNew();
					category.Code = "ABC";
					category.Description = (NoResString)"ABC Desc";
					category.Bool = true; // Active
					category.Bool2 = false; // Elimination
					category.Bool3 = true; // Elimination
					AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, list);
					AccGLHeader glHeader5 = testObjectCreator.CreateAccGLHeader("5300.05.97", "AS", "Test BSH 5", "BSH", Constants.DebitCredit.Debit);
					AccGLHeader glHeader6 = testObjectCreator.CreateAccGLHeader("5300.05.98", "AS", "Test BSH 6", "BSH", Constants.DebitCredit.Credit);
					testObjectCreator.CreateAccountDescriptor(glHeader5, "5300.197", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit);
					testObjectCreator.CreateAccountDescriptor(glHeader6, "5300.198", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
					testObjectCreator.CreateAccGLAggregate(300m, 201011, glHeader5.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "ABC");
					testObjectCreator.CreateAccGLAggregate(-350, 201011, glHeader6.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "ABC");
					Factory.Save();
					GLAccountBalancesAndMovementsCollection collection = new GLAccountBalancesAndMovementsCollection(Factory, 201012);
					AssertEquals(11, collection.Count);
					GLAccountBalancesAndMovements glAccountBalancesAndMovements = collection[1];
					AssertEquals(glAccountBalancesAndMovements.FinancialYear, 2010);
					AssertEquals(glAccountBalancesAndMovements.Period, 201012);
					AssertEquals(glAccountBalancesAndMovements.GLAccountNumber, "5300.095");
					AssertEquals(glAccountBalancesAndMovements.CurrencyCode, "CNY");
					AssertEquals(glAccountBalancesAndMovements.OpenQuantity, 0m);
					AssertEquals(glAccountBalancesAndMovements.OpenBalanceCurrency, 100m);
					AssertEquals(glAccountBalancesAndMovements.OpenBalanceLocalCurrency, 100m);
					AssertEquals(glAccountBalancesAndMovements.DebitQuantity, 0m);
					AssertEquals(glAccountBalancesAndMovements.DebitCurrencyAmount, 1003m);
					AssertEquals(glAccountBalancesAndMovements.DebitAmountLocalCurrency, 1003m);
					AssertEquals(glAccountBalancesAndMovements.CreditQuantity, 0m);
					AssertEquals(glAccountBalancesAndMovements.CreditCurrencyAmount, 0m);
					AssertEquals(glAccountBalancesAndMovements.CreditAmountLocalCurrency, 0m);
					AssertEquals(glAccountBalancesAndMovements.EndQuantity, 0m);
					AssertEquals(glAccountBalancesAndMovements.EndBalanceCurrency, 1103m);
					AssertEquals(glAccountBalancesAndMovements.EndBalanceLocalCurrency, 1103m);
					glAccountBalancesAndMovements = collection[7];
					AssertEquals(glAccountBalancesAndMovements.FinancialYear, 2010);
					AssertEquals(glAccountBalancesAndMovements.Period, 201012);
					AssertEquals(glAccountBalancesAndMovements.GLAccountNumber, "5300.198");
					glAccountBalancesAndMovements = collection[10];
					AssertEquals(glAccountBalancesAndMovements.FinancialYear, 2010);
					AssertEquals(glAccountBalancesAndMovements.Period, 201012);
					AssertEquals(glAccountBalancesAndMovements.GLAccountNumber, "6100.097");
					AssertEquals(glAccountBalancesAndMovements.CurrencyCode, "CNY");
					AssertEquals(glAccountBalancesAndMovements.OpenQuantity, 0m);
					AssertEquals(glAccountBalancesAndMovements.OpenBalanceCurrency, 1101m);
					AssertEquals(glAccountBalancesAndMovements.OpenBalanceLocalCurrency, 1101m);
					AssertEquals(glAccountBalancesAndMovements.DebitQuantity, 0m);
					AssertEquals(glAccountBalancesAndMovements.DebitCurrencyAmount, 1002m);
					AssertEquals(glAccountBalancesAndMovements.DebitAmountLocalCurrency, 1002m);
					AssertEquals(glAccountBalancesAndMovements.CreditQuantity, 0m);
					AssertEquals(glAccountBalancesAndMovements.CreditCurrencyAmount, 0m);
					AssertEquals(glAccountBalancesAndMovements.CreditAmountLocalCurrency, 0m);
					AssertEquals(glAccountBalancesAndMovements.EndQuantity, 0m);
					AssertEquals(glAccountBalancesAndMovements.EndBalanceCurrency, 2103m);
					AssertEquals(glAccountBalancesAndMovements.EndBalanceLocalCurrency, 2103m);
				}
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new GLAccountBalancesAndMovements();
		}

		protected override GLAccountBalancesAndMovementsCollection GetCollectionToTest()
		{
			return new GLAccountBalancesAndMovementsCollection(Factory, 0);
		}
	}
}
