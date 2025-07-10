using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	public class BusinessObjectRetrieverTest : TestCaseWithFactory
	{
		public void TestRetrieveBranch()
		{
			GlbBranch branch1InThisCompany;
			GlbBranch branch2InThisCompany;
			GlbBranch branch3InDifferentCompany;

			branch1InThisCompany = Factory.NewWithValidTestData<GlbBranch>();
			branch1InThisCompany.GB_Code = "BR1";
			branch1InThisCompany.GB_GC = GlbCompany.CurrentCompany.PK;

			branch2InThisCompany = Factory.NewWithValidTestData<GlbBranch>();
			branch2InThisCompany.GB_Code = "BR2";
			branch2InThisCompany.GB_GC = GlbCompany.CurrentCompany.PK;

			branch3InDifferentCompany = Factory.NewWithValidTestData<GlbBranch>();
			branch3InDifferentCompany.GB_Code = "BR3";
			branch3InDifferentCompany.GB_GC = new ZGuid();

			GlbBranch retreivedBranch1 = BusinessObjectRetriever.GetBranchFromBranchCode(Factory, "BR1");
			GlbBranch retreivedBranch2 = BusinessObjectRetriever.GetBranchFromBranchCode(Factory, "BR2");
			GlbBranch retreivedBranch3 = BusinessObjectRetriever.GetBranchFromBranchCode(Factory, "BR3");

			AssertNotNull("Retrieved Branch 1 should not be null", retreivedBranch1);
			AssertEquals("Retrieved Branch 1 Code", branch1InThisCompany.GB_Code, retreivedBranch1.GB_Code);

			AssertNotNull("Retrieved Branch 2 should not be null", retreivedBranch2);
			AssertEquals("Retrieved Branch 2 Code", branch2InThisCompany.GB_Code, retreivedBranch2.GB_Code);

			AssertNull("Retrieved Branch 3 should be null (its in another company)", retreivedBranch3);
		}

		public void TestRetrieveDepartment()
		{
			GlbDepartment department1;
			GlbDepartment department2;

			department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "BR1";

			department2 = Factory.NewWithValidTestData<GlbDepartment>();
			department2.GE_Code = "BR2";

			GlbDepartment retreivedDepartment1 = BusinessObjectRetriever.GetDepartmentFromDepartmentCode(Factory, "BR1");
			GlbDepartment retreivedDepartment2 = BusinessObjectRetriever.GetDepartmentFromDepartmentCode(Factory, "BR2");

			AssertNotNull("Retrieved Department 1 should not be null", retreivedDepartment1);
			AssertEquals("Retrieved Department 1 Code", department1.GE_Code, retreivedDepartment1.GE_Code);

			AssertNotNull("Retrieved Department 2 should not be null", retreivedDepartment2);
			AssertEquals("Retrieved Department 2 Code", department2.GE_Code, retreivedDepartment2.GE_Code);
		}

		public void TestRetrieveGLAccount()
		{
			AccGLHeader header1;
			AccGLHeader header2;
			AccGLHeader header3;

			header1 = Factory.NewWithValidTestData<AccGLHeader>();
			header1.AG_AccountNum = "1234.56.78";
			header1.AG_AccountType = Core.Constants.AccountType.ProfitAndLossAccount;

			header2 = Factory.NewWithValidTestData<AccGLHeader>();
			header2.AG_AccountNum = "8765.43.21";
			header2.AG_AccountType = Core.Constants.AccountType.Total;

			header3 = Factory.NewWithValidTestData<AccGLHeader>();
			header3.AG_AccountNum = "1111.11.11";
			header3.AG_AccountType = Core.Constants.AccountType.BalanceSheetAccount;
			header3.AG_DisallowDirectPosting = true;

			AccGLHeader header1Retrived = BusinessObjectRetriever.GetGLHeaderFromGLAccountNumber(Factory, header1.AG_AccountNum, false);
			AccGLHeader header2Retrived = BusinessObjectRetriever.GetGLHeaderFromGLAccountNumber(Factory, header2.AG_AccountNum, false);
			AccGLHeader header3Retrived = BusinessObjectRetriever.GetGLHeaderFromGLAccountNumber(Factory, header2.AG_AccountNum, false);

			AssertNotNull("Retrieved Header 1", header1Retrived);
			AssertNotNull("Retrieved Header 2", header2Retrived);
			AssertNotNull("Retrieved Header 3", header3Retrived);

			header1Retrived = BusinessObjectRetriever.GetGLHeaderFromGLAccountNumber(Factory, header1.AG_AccountNum, true);
			header2Retrived = BusinessObjectRetriever.GetGLHeaderFromGLAccountNumber(Factory, header2.AG_AccountNum, true);
			header3Retrived = BusinessObjectRetriever.GetGLHeaderFromGLAccountNumber(Factory, header2.AG_AccountNum, true);

			AssertNotNull("Retrieved Header 1", header1Retrived);
			AssertNull("Retrieved Header 2 (Invalid Type)", header2Retrived);
			AssertNull("Retrieved Header 3 (Disallow Direct Posting)", header3Retrived);
		}

		public void TestRetrieveBankAccount()
		{
			AccBankAccount bank1InThisCompany;
			AccBankAccount bank2InThisCompany;
			AccBankAccount bank3InDifferentCompany;

			bank1InThisCompany = Factory.NewWithValidTestData<AccBankAccount>();
			bank1InThisCompany.AB_Code = "BA1";
			bank1InThisCompany.AB_GC = GlbCompany.CurrentCompany.PK;

			bank2InThisCompany = Factory.NewWithValidTestData<AccBankAccount>();
			bank2InThisCompany.AB_Code = "BA2";
			bank2InThisCompany.AB_GC = GlbCompany.CurrentCompany.PK;

			bank3InDifferentCompany = Factory.NewWithValidTestData<AccBankAccount>();
			bank3InDifferentCompany.AB_Code = "BA3";
			bank3InDifferentCompany.AB_GC = new ZGuid();

			AccBankAccount retreivedBank1 = BusinessObjectRetriever.GetBankFromBankCode(Factory, "BA1");
			AccBankAccount retreivedBank2 = BusinessObjectRetriever.GetBankFromBankCode(Factory, "BA2");
			AccBankAccount retreivedBank3 = BusinessObjectRetriever.GetBankFromBankCode(Factory, "BA3");

			AssertNotNull("Retrieved Bank 1 should not be null", retreivedBank1);
			AssertEquals("Retrieved Bank 1 Code", bank1InThisCompany.AB_Code, retreivedBank1.AB_Code);

			AssertNotNull("Retrieved Bank 2 should not be null", retreivedBank2);
			AssertEquals("Retrieved Bank 2 Code", bank2InThisCompany.AB_Code, retreivedBank2.AB_Code);

			AssertNull("Retrieved Bank 3 should be null (its in another company)", retreivedBank3);
		}

		public void TestRetrieveChequeBookAccount()
		{
			AccChequeBook chequeBook1InThisCompany;
			AccChequeBook chequeBook2InThisCompany;
			AccChequeBook chequeBook3InDifferentCompany;

			chequeBook1InThisCompany = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook1InThisCompany.AK_Code = "BA1";
			chequeBook1InThisCompany.AK_GB = GlbBranch.CurrentBranch.PK;

			chequeBook2InThisCompany = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook2InThisCompany.AK_Code = "BA2";
			chequeBook2InThisCompany.AK_GB = GlbBranch.CurrentBranch.PK;

			chequeBook3InDifferentCompany = Factory.NewWithValidTestData<AccChequeBook>();
			chequeBook3InDifferentCompany.AK_Code = "BA3";
			chequeBook3InDifferentCompany.AK_GB = new ZGuid();

			AccChequeBook retreivedChequeBook1 = BusinessObjectRetriever.GetChequeBookFromChequeBookCode(Factory, "BA1");
			AccChequeBook retreivedChequeBook2 = BusinessObjectRetriever.GetChequeBookFromChequeBookCode(Factory, "BA2");
			AccChequeBook retreivedChequeBook3 = BusinessObjectRetriever.GetChequeBookFromChequeBookCode(Factory, "BA3");

			AssertNotNull("Retrieved ChequeBook 1 should not be null", retreivedChequeBook1);
			AssertEquals("Retrieved ChequeBook 1 Code", chequeBook1InThisCompany.AK_Code, retreivedChequeBook1.AK_Code);

			AssertNotNull("Retrieved ChequeBook 2 should not be null", retreivedChequeBook2);
			AssertEquals("Retrieved ChequeBook 2 Code", chequeBook2InThisCompany.AK_Code, retreivedChequeBook2.AK_Code);

			AssertNull("Retrieved ChequeBook 3 should be null (its in another company)", retreivedChequeBook3);
		}

		public void TestRetrievePeriod()
		{
			ZDateTime postDate = new ZDateTime(2005, 01, 01, 10, 30, 0);
			new AccountingPeriodTestHelper().PostPeriodsForEntireYear(postDate.Year, GlbCompany.CurrentCompany.PK);
			AssertEquals("GL Period for date: " + postDate.ToString(), 200507, BusinessObjectRetriever.GetGLPeriodFromDate(Factory, postDate));

			ZDateTime dateWithNoPeriod = postDate.AddYears(3);
			AssertEquals("GL Period for date: " + dateWithNoPeriod.ToString(), 0, BusinessObjectRetriever.GetGLPeriodFromDate(Factory, dateWithNoPeriod));
		}

		public void TestRetrieveOrgHeaderByCode()
		{
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader1.OH_Code = "ORG1";
			orgHeader1.OH_IsActive = true;

			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader2.OH_Code = "ORG2";
			orgHeader2.OH_IsActive = false;

			var retrievedOrgHeader1 = BusinessObjectRetriever.GetOrgHeaderByCode(Factory, "ORG1");
			var retrievedOrgHeader2 = BusinessObjectRetriever.GetOrgHeaderByCode(Factory, "ORG2");

			AssertNotNull(retrievedOrgHeader1);
			AssertNull(retrievedOrgHeader2);
		}

		public void TestRetrieveGlbStaffByCode()
		{
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";
			staff1.GS_IsActive = true;

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "ST2";
			staff2.GS_IsActive = false;

			var retrievedStaff1 = BusinessObjectRetriever.GetGlbStaffByCode(Factory, "ST1");
			var retrievedStaff2 = BusinessObjectRetriever.GetGlbStaffByCode(Factory, "ST2");

			AssertNotNull(retrievedStaff1);
			AssertNull(retrievedStaff2);
		}

		public void TestRetrieveGlbGroupByCode()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "GR1";
			group1.GG_IsActive = true;

			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "GR2";
			group2.GG_IsActive = false;

			var retrievedGroup1 = BusinessObjectRetriever.GetGlbGroupByCode(Factory, "GR1");
			var retrievedGroup2 = BusinessObjectRetriever.GetGlbGroupByCode(Factory, "GR2");

			AssertNotNull(retrievedGroup1);
			AssertNull(retrievedGroup2);
		}

		public void TestRetrieveAccGroups()
		{
			var grp = Factory.NewWithValidTestData<AccGroups>();
			grp.AR_Code = "Sales";

			var retrievedGrp = BusinessObjectRetriever.GetAccGroupsByCode(Factory, "Sales");

			AssertNotNull(retrievedGrp);
		}
	}
}
