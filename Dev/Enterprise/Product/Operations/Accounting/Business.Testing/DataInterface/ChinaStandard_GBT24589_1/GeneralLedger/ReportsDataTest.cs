using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.DataInterface.ChinaStandard_GBT24589_1.Testing
{
	using System;
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Core;
	using NUnit.Framework;

	[TestedType(typeof(ReportsData))]
	public class ReportsDataTest : NonPersistentBusinessObjectTestCase
	{
		public void TestClassProperties()
		{
			AssertEquals("T209", ReportsData.LocID);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ReportsData();
		}
	}

	[TestedType(typeof(ReportsDataCollection))]
	public class ReportsDataCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ReportsDataCollection>
	{
		[TestDate(2018, 07, 14)]
		public void TestAddElementsAndClassProperties()
		{
			ZString countryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Constants.CountryCodes.China;
			ComplianceReportTypeCollection list = new ComplianceReportTypeCollection(Constants.CountryCodes.China);
			AccountingMasterFilesRegistry.Instance.ComplianceReportsSetupsCN.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, list);
			AccountingPeriodTestHelper testHelper = new AccountingPeriodTestHelper(Factory);
			testHelper.PostPeriodsForEntireYear(2009, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			testHelper.PostPeriodsForEntireYear(2010, GlbCompany.CurrentCompany.PK, AccountingPeriodTestHelper.CalendarType.CalendarYear);
			TestObjectCreator testObjectCreator = new TestObjectCreator(Factory);
			AccGLHeader glHeader1PNL = testObjectCreator.CreateAccGLHeader("1100.03.95", "TS", "Test PNL 1", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2PNL = testObjectCreator.CreateAccGLHeader("1100.03.96", "TS", "Test PNL 2", "P&L", Constants.DebitCredit.Debit);
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader1PNL, "61000.95", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader1PNL, "P&L", "D11");
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader2PNL, "61000.96", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader2PNL, "P&L", "D11");
			AccGLHeader glHeader3PNL = testObjectCreator.CreateAccGLHeader("1200.03.97", "TS", "Test PNL 3", "P&L", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4PNL = testObjectCreator.CreateAccGLHeader("1200.03.98", "TS", "Test PNL 4", "P&L", Constants.DebitCredit.Credit);
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader3PNL, "63000.97", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader3PNL, "P&L", "D12");
			AccGLAccountDescriptor testAccGLAccountDescriptor4PNL = testObjectCreator.CreateAccountDescriptor(glHeader4PNL, "53000.98", "COA", "P&L", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
			testObjectCreator.CreateGLDescriptorPivot(testAccGLAccountDescriptor4PNL, glHeader4PNL, "P&L", "D13");
			testObjectCreator.CreateAccGLAggregate(1101m, 201001, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1101m, 201001, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(101m, 201001, glHeader1PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(201m, 201001, glHeader2PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(301m, 200911, glHeader3PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-301m, 200911, glHeader4PNL.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			AccGLHeader glHeader1 = testObjectCreator.CreateAccGLHeader("5300.03.95", "AS", "Test BSH 1", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader2 = testObjectCreator.CreateAccGLHeader("5300.03.96", "AS", "Test BSH 2", "BSH", Constants.DebitCredit.Debit);
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader1, "53000.95", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader1, "BSH", "D01");
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader2, "53000.96", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader2, "BSH", "D01");
			AccGLHeader glHeader3 = testObjectCreator.CreateAccGLHeader("5300.03.97", "AS", "Test BSH 3", "BSH", Constants.DebitCredit.Debit);
			AccGLHeader glHeader4 = testObjectCreator.CreateAccGLHeader("5300.03.98", "AS", "Test BSH 4", "BSH", Constants.DebitCredit.Credit);
			testObjectCreator.CreateGLDescriptorPivot(testObjectCreator.CreateAccountDescriptor(glHeader3, "53000.97", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Debit), glHeader3, "BSH", "D02");
			AccGLAccountDescriptor testAccGLAccountDescriptor4 = testObjectCreator.CreateAccountDescriptor(glHeader4, "53000.98", "COA", "BSH", Core.SharedConstants.Languages.ChineseSimplified, "", "CN", Constants.DebitCredit.Credit);
			testObjectCreator.CreateGLDescriptorPivot(testAccGLAccountDescriptor4, glHeader4, "BSH", "D03");
			testObjectCreator.CreateAccGLAggregate(1100m, 201001, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-1100m, 201001, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(100m, 201001, glHeader1.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(200m, 201001, glHeader2.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(300m, 200912, glHeader3.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			testObjectCreator.CreateAccGLAggregate(-300m, 200912, glHeader4.PK, GlbBranch.CurrentBranch.PK, GlbCompany.CurrentCompany.PK, GlbDepartment.CurrentDepartment.PK, "");
			Factory.Save();
			ReportsDataCollection collection = new ReportsDataCollection(Factory, 201001);
			AssertEquals(292, collection.Count);
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = countryCode;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ReportsData();
		}

		protected override ReportsDataCollection GetCollectionToTest()
		{
			return new ReportsDataCollection(Factory, 0);
		}
	}
}
