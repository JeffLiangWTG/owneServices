using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_JobProfitSummaryByConsolTest : ScriptTest
	{
		public void TestChargeCodeFilter()
		{
			DataTable jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);

			CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m);

			CreateConsolShipmentAndJob("C002", "AUSYD", "USNYC",
										TestObjectCreator.CC1, TestObjectCreator.CC3,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										200m, 300m, 400m, 500m);

			TestObjectCreator.Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: (CC1 + CC2) for C001", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC2) for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: (CC1 + CC3) for C002", 800, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC3) for C002", -600, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			//Including
			jobForwardProfitTable = RunScript(TestObjectCreator.CC1.PK.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC1 for C001", 200, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 for C001", -100, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: CC1 for C002", 300, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1  for C002", -200, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(TestObjectCreator.CC2.PK.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("Consol containing the Charge", "C001", Convert.ToString(jobForwardProfitTable.Rows[0]["JK_UniqueConsignRef"]));
			AssertEquals("WIP Amount: CC2 for C001", 300, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC2 for C001", -300, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));

			jobForwardProfitTable = RunScript(TestObjectCreator.CC3.PK.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("Consol containing the Charge", "C002", Convert.ToString(jobForwardProfitTable.Rows[0]["JK_UniqueConsignRef"]));
			AssertEquals("WIP Amount: CC3 for C002", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC3 for C002", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));

			jobForwardProfitTable = RunScript(TestObjectCreator.CC4.PK.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 0, jobForwardProfitTable.Rows.Count);

			//Excluding
			jobForwardProfitTable = RunScript(string.Empty, TestObjectCreator.CC1.PK.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC2 for C001", 300, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC2 for C001", -300, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: CC3 for C002", 500, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: CC3 for C002", -400, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, TestObjectCreator.CC2.PK.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC1 for C001", 200, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 for C001", -100, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: CC1 + CC3 for C002", 800, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 + CC3 for C002", -600, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, TestObjectCreator.CC3.PK.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC1 + CC2 for C001", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 + CC2 for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: CC1 for C002", 300, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 for C002", -200, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, TestObjectCreator.CC4.PK.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
		}

		public void TestChargeGroupFilter()
		{
			DataTable jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);

			TestObjectCreator.CC2.AC_ChargeGroup = "FRT";

			CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m);

			CreateConsolShipmentAndJob("C002", "AUSYD", "USNYC",
										TestObjectCreator.CC1, TestObjectCreator.CC3,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										200m, 300m, 400m, 500m);

			TestObjectCreator.Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: (CC1 + CC2) for C001", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC2) for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: (CC1 + CC3) for C002", 800, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC3) for C002", -600, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, TestObjectCreator.CC2.AC_ChargeGroup, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC2 for C001", 300, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC2 for C001", -300, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
		}

		public void TestSalesGroupFilter()
		{
			DataTable jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			AccGroups salesGroup1 = TestObjectCreator.Factory.New<AccGroups>();
			salesGroup1.AR_Code = "SAL";
			salesGroup1.AR_Desc = "Sales Group 1";
			salesGroup1.Factory.Save();
			TestObjectCreator.CC1.AC_AR_SalesGroup = salesGroup1.PK;

			CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m);

			CreateConsolShipmentAndJob("C002", "AUSYD", "USNYC",
										TestObjectCreator.CC1, TestObjectCreator.CC3,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										200m, 300m, 400m, 500m);

			TestObjectCreator.Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: (CC1 + CC2) for C001", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC2) for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: (CC1 + CC3) for C002", 800, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC3) for C002", -600, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, TestObjectCreator.CC1.AC_AR_SalesGroup.ToString(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC1 for C001", 200, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 for C001", -100, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: CC1 for C002", 300, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 for C002", -200, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));
		}

		public void TestExpenseGroupFilter()
		{
			DataTable jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			AccGroups expenseGroup1 = TestObjectCreator.Factory.New<AccGroups>();
			expenseGroup1.AR_Code = "EXP";
			expenseGroup1.AR_Desc = "Expense Group 1";
			expenseGroup1.Factory.Save();
			TestObjectCreator.CC1.AC_AR_ExpenseGroup = expenseGroup1.PK;

			CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m);

			CreateConsolShipmentAndJob("C002", "AUSYD", "USNYC",
										TestObjectCreator.CC1, TestObjectCreator.CC3,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										200m, 300m, 400m, 500m);

			TestObjectCreator.Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: (CC1 + CC2) for C001", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC2) for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: (CC1 + CC3) for C002", 800, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC3) for C002", -600, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, TestObjectCreator.CC1.AC_AR_ExpenseGroup.ToString(), string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC1 for C001", 200, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 for C001", -100, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: CC1 for C002", 300, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 for C002", -200, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));
		}

		public void TestBranchFilter()
		{
			DataTable jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			AccGroups expenseGroup1 = TestObjectCreator.Factory.New<AccGroups>();
			expenseGroup1.AR_Code = "EXP";
			expenseGroup1.AR_Desc = "Expense Group 1";
			expenseGroup1.Factory.Save();
			TestObjectCreator.CC1.AC_AR_ExpenseGroup = expenseGroup1.PK;

			CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m);

			CreateConsolShipmentAndJob("C002", "AUSYD", "USNYC",
										TestObjectCreator.CC1, TestObjectCreator.CC3,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, GlbBranch.CurrentBranch.PK, GlbBranch.CurrentBranch.PK,
										200m, 300m, 400m, 500m);

			TestObjectCreator.Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: (CC1 + CC2) for C001", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC2) for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: (CC1 + CC3) for C002", 800, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC3) for C002", -600, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, branch2.PK.ToString(), string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC1 for C001", 200, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 for C001", -100, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
		}

		public void TestDepartmentFilter()
		{
			DataTable jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m);

			CreateConsolShipmentAndJob("C002", "AUSYD", "USNYC",
										TestObjectCreator.CC1, TestObjectCreator.CC3,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FISDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										200m, 300m, 400m, 500m);

			TestObjectCreator.Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: (CC1 + CC2) for C001", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC2) for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: (CC1 + CC3) for C002", 800, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC3) for C002", -600, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, TestObjectCreator.FESDepartment.PK.ToString(), string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC1 for C001", 300, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: CC1 for C001", -300, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
		}

		public void TestDebotrFilter()
		{
			DataTable jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m);

			CreateConsolShipmentAndJob("C002", "AUSYD", "USNYC",
										TestObjectCreator.CC1, TestObjectCreator.CC3,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.AALSHI,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FISDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										200m, 300m, 400m, 500m);

			TestObjectCreator.Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: (CC1 + CC2) for C001", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC2) for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: (CC1 + CC3) for C002", 800, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC3) for C002", -600, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, TestObjectCreator.ABIGAS.PK.ToString(), string.Empty);
			AssertEquals("Number of Rows after Data Setup", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: CC2 for C001", 300, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
		}

		public void TestCreditorFilter()
		{
			DataTable jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m);

			CreateConsolShipmentAndJob("C002", "AUSYD", "USNYC",
										TestObjectCreator.CC1, TestObjectCreator.CC3,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor3, TestObjectCreator.Debtor, TestObjectCreator.AALSHI,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FISDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										200m, 300m, 400m, 500m);

			TestObjectCreator.Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("WIP Amount: (CC1 + CC2) for C001", 500, Convert.ToInt32(jobForwardProfitTable.Rows[0]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC2) for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
			AssertEquals("WIP Amount: (CC1 + CC3) for C002", 800, Convert.ToInt32(jobForwardProfitTable.Rows[1]["WIPAmount"]));
			AssertEquals("ACR Amount: (CC1 + CC3) for C002", -600, Convert.ToInt32(jobForwardProfitTable.Rows[1]["ACRAmount"]));

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, TestObjectCreator.Creditor3.PK.ToString());
			AssertEquals("Number of Rows after Data Setup", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("ACR Amount: CC2 for C001", -400, Convert.ToInt32(jobForwardProfitTable.Rows[0]["ACRAmount"]));
		}

		public void TestShipmentMastersAndHouseColumns()
		{
			DataTable jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m,
										shipment1Type: "HLS", shipment2Type: "ASM");

			CreateConsolShipmentAndJob("C002", "AUSYD", "USNYC",
										TestObjectCreator.CC1, TestObjectCreator.CC3,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor3, TestObjectCreator.Debtor, TestObjectCreator.AALSHI,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FISDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										200m, 300m, 400m, 500m,
										shipment1Type: "BCN", shipment2Type: "STD");

			TestObjectCreator.Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows after Data Setup", 2, jobForwardProfitTable.Rows.Count);
			AssertEquals("Shipment for C001", 1, Convert.ToInt32(jobForwardProfitTable.Rows[0]["Shipments"]));
			AssertEquals("Masters for C001", 1, Convert.ToInt32(jobForwardProfitTable.Rows[0]["Masters"]));
			AssertEquals("House for C001", 0, Convert.ToInt32(jobForwardProfitTable.Rows[0]["House"]));
			AssertEquals("Shipment for C002", 1, Convert.ToInt32(jobForwardProfitTable.Rows[1]["Shipments"]));
			AssertEquals("Masters for C002", 1, Convert.ToInt32(jobForwardProfitTable.Rows[1]["Masters"]));
			AssertEquals("House for C002", 0, Convert.ToInt32(jobForwardProfitTable.Rows[1]["House"]));
		}

		public void TestNotIncludeDSBCharge()
		{
			var jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
			AssertEquals("Number of Rows before Data Setup", 0, jobForwardProfitTable.Rows.Count);

			var consol = TestObjectCreator.CreateConsol("AUSYD", "AUMEL", "C001");
			var shipment = TestObjectCreator.CreateShipment("S001001", "AUSYD", "AUMEL", consol);
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);

			var invoice = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "001", TestObjectCreator.AUD, 1.0m, 100.00m, 0m, 100.00m, 0m);
			var line1 = invoice.Lines[0];
			line1.AL_AC = TestObjectCreator.CC1.PK;
			line1.AL_JH = job.PK;
			var charge1 = TestObjectCreator.CreateCharge(line1);

			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.AUD, 1M, 50M);
			line2.AL_AC = TestObjectCreator.DSBChargeCode.PK;
			line2.AL_JH = job.PK;
			var charge2 = TestObjectCreator.CreateCharge(line2);

			Factory.Save();

			charge1.JR_LocalSellAmt = 0M;
			charge2.JR_LocalSellAmt = 0M;
			Factory.Save();

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true);
			AssertEquals("Number of Rows after Data Setup", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("Should include DSB charge", -150M, jobForwardProfitTable.Rows[0]["AL_LineAmount"]);
			AssertEquals("Should not include DSB charge", -150M, jobForwardProfitTable.Rows[0]["AL_LineAmountAfterChargeExclusion"]);

			jobForwardProfitTable = RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, false);
			AssertEquals("Number of Rows after Data Setup", 1, jobForwardProfitTable.Rows.Count);
			AssertEquals("Should not include DSB charge", -150M, jobForwardProfitTable.Rows[0]["AL_LineAmount"]);
			AssertEquals("Should not include DSB charge", -100M, jobForwardProfitTable.Rows[0]["AL_LineAmountAfterChargeExclusion"]);
		}

		ZGuid[] CreateConsolShipmentAndJob(string consolNumber, string origin, string destination,
										AccChargeCode chargeCode1, AccChargeCode chargeCode2,
										OrgHeader creditor1, OrgHeader creditor2, OrgHeader debtor1, OrgHeader debtor2,
										ZGuid department1PK, ZGuid department2PK, ZGuid branch1PK, ZGuid branch2PK,
										decimal charge1CostAmount, decimal charge1SellAmount, decimal charge2CostAmount, decimal charge2SellAmount,
										string shipment1Type = "STD", string shipment2Type = "STD")
		{
			var consol1 = TestObjectCreator.CreateConsol(origin, destination, consolNumber);

			var shipment1 = TestObjectCreator.CreateShipment(consolNumber + "S001", origin, destination, consol1);
			shipment1.JS_ShipmentType = shipment1Type;

			var shipment2 = TestObjectCreator.CreateShipment(consolNumber + "S002", origin, destination, consol1);
			shipment2.JS_ShipmentType = shipment2Type;

			var job1 = TestObjectCreator.CreateJob(shipment1);
			var job2 = TestObjectCreator.CreateJob(shipment2);
			var charge1 = TestObjectCreator.CreateCharge(job1, chargeCode1, string.Empty, TestObjectCreator.AUD, charge1CostAmount, creditor1, TestObjectCreator.AUD, charge1SellAmount, debtor1);
			charge1.JR_GE = department1PK;
			charge1.JR_GB = branch1PK;
			var charge2 = TestObjectCreator.CreateCharge(job2, chargeCode2, string.Empty, TestObjectCreator.AUD, charge2CostAmount, creditor2, TestObjectCreator.AUD, charge2SellAmount, debtor2);
			charge2.JR_GE = department2PK;
			charge2.JR_GB = branch2PK;

			return new ZGuid[] { shipment1.PK, shipment2.PK };
		}

		public void TestJobProfitSummaryByConsol_ControllingCustomerAndAgent()
		{
			var branch2 = TestObjectCreator.CreateBranch("BR1", GlbCompany.CurrentCompany);
			var shipmentPKs = CreateConsolShipmentAndJob("C001", "AUSYD", "AUMEL",
										TestObjectCreator.CC1, TestObjectCreator.CC2,
										TestObjectCreator.Creditor1, TestObjectCreator.Creditor2, TestObjectCreator.Debtor, TestObjectCreator.ABIGAS,
										TestObjectCreator.FIADepartment.PK, TestObjectCreator.FESDepartment.PK, branch2.PK, GlbBranch.CurrentBranch.PK,
										100m, 200m, 300m, 300m,
										shipment1Type: "HLS", shipment2Type: "ASM");
			TestObjectCreator.Factory.Save();

			var runScript = new Func<string, ZGuid?, DataTable>((sql, pK) =>
			{
				return RunScript(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, true, whereClause: sql, mNGOrgPK: pK);
			});

			var infoChecker = new CAGAndCCBInfoScriptTest();
			infoChecker.VerifyControllingCustomerAndAgentInfo(runScript, shipmentPKs[0], shipmentPKs[1]);
		}

		DataTable RunScript(string chargeCodePK, string excludeChargeCodePK, string chargeGroup, string salesGroupPK, string expenseGroupPK, string transactionBranchPK, string transactionDepartmentPK, string transactionDebtorPK, string @transactionCreditorPK, bool includeDSBCharge = true, string whereClause = "", ZGuid? mNGOrgPK = null)
		{
			string sqlQuery = string.Format(@"
				SELECT  REVAmount,
						WIPAmount,
						CSTAmount, 
						ACRAmount,
						AL_LineAmount,
						AL_LineAmountAfterChargeExclusion,
						JK_UniqueConsignRef,
						JK_MasterBillNum,
						JK_AgentType,
						ShippingLineCode,
						ShippingLinePK,
						JK_ConsolMode,
						JK_TransportMode,
						CreditorCode,
						CreditorPK,
						ReceivingForwarderCode,
						ReceivingForwarderPK,
						SendingForwarderCode,
						SendingForwarderPK,	
						DischargePort,
						DischargePortETA,
						LoadPort,
						LoadPortETD,
						LoadCountryCode,
						DischargeCountryCode,
						ActualWeight,
						UnitOfWeight,
						ActualVolume,
						UnitOfVolume,
						ActualChargeable,
						ChargeableUnit,
						Shipments,
						Masters,
						House,		
						FW_ConsolFirstLoad,
						FW_ConsolLastDischarge,  
						FW_ConsolETD,  
						FW_ConsolATD,  
						FW_ConsolETA,  
						FW_ConsolATA,
						FW_FirstLoadCountry,
						FW_LastDischargeCountry,
						CCBOrgName,						
						CAGOrgName,
						MNGName
				FROM	Report_JobProfitSummaryByConsol
				(
						N'{0}'								--@CurrentCountry
						, '{1}'								--@CompanyPK
						, '1900-01-01 00:00:00'				--@TransactionFrom
						, '2079-06-06 23:59:29'				--@TransactionTo
						, N''								--@OutstandingWIP
						, N''								--@OutstandingACR
						, N'{2}'							--@ChargeCode ''
						, N'{3}'							--@ExcludeChargeCode ''
						, {4}								--@ChargeGroup	NULL
						, {5}								--@SalesGroup	NULL
						, {6}								--@ExpenseGroup	NULL
						, N'{7}'							--@TransactionBranch ''
						, N'{8}'							--@TransactionDepartment ''
						, N'{9}'							--@TransactionDebtor ''
						, N'{10}'							--@TransactionCreditor ''
						, ''								--@PostedOnly
						, NULL								--@JobStatus
						, '1900-01-01 00:00:00'				--@JobOpenedFrom
						, '2079-06-06 23:59:29'				--@JobOpenedTo
						, '1900-01-01 00:00:00'				--@JobClosedFrom
						, '2079-06-06 23:59:29'				--@JobClosedTo
						, ''								--@JobNumber
						, {11}								--@IncludeDSBCharge
						, @MNGListValue
						, @MNGListIsEmptyValue
						, 'ALL'								--@Gateway
				)
				{12}
				ORDER BY JK_UniqueConsignRef 
				"
				, "AU"
				, GlbCompany.CurrentCompany.PK.ToString()
				, chargeCodePK
				, excludeChargeCodePK
				, string.IsNullOrEmpty(chargeGroup) ? "NULL" : "'" + chargeGroup + "'"
				, string.IsNullOrEmpty(salesGroupPK) ? "NULL" : "'" + salesGroupPK + "'"
				, string.IsNullOrEmpty(expenseGroupPK) ? "NULL" : "'" + expenseGroupPK + "'"
				, transactionBranchPK
				, transactionDepartmentPK
				, transactionDebtorPK
				, transactionCreditorPK
				, includeDSBCharge ? 1 : 0
				, whereClause
			 );

			var command = Db.Connection.Command(sqlQuery);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", mNGOrgPK.HasValue ? new Guid[] { mNGOrgPK.Value.ToGuid() } : Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}
	}
}
