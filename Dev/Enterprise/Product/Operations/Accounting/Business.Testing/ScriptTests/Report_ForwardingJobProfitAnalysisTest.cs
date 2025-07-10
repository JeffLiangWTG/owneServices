
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.Business.Testing.ScriptTests.BaseTests;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class Report_ForwardingJobProfitAnalysisTest : JobProfitFilterTest
	{
		#region Active Status filter test

		protected override string[] ActiveStatusFilterKeyColumnsForTest =>
			["JH_JobNum", "AL_LineType"];

		protected override string[] ActiveStatusFilterHeadersForTest =>
			["JH_JobNum", "AL_LineType", "AL_LineAmount", "JH_IsActive"];

		protected override object[][] ActiveStatusFilterActiveLinesForTest =>
		[
			["S0001", "ACR", -10m, true],
			["S0001", "WIP", 20m, true],
			["S0002", "ACR", -40m, true],
			["S0002", "WIP", 60m, true]
		];

		protected override object[][] ActiveStatusFilterInactiveLinesForTest =>
		[
			["S0003", "ACR", 0m, false],
			["S0003", "WIP", 0m, false],
			["S0003", "ACR", 0m, false],
			["S0003", "WIP", 0m, false]
		];

		#endregion

		protected override bool ShouldTestShowReverseFilter => false;

		[SnailTest]
		public void TestForwardingAndCustomsTransactionDetailReport_UseARSettlementGroup()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, TestObjectCreator.LocalClient, 0M, TestObjectCreator.Agent, 0M);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0m, TestObjectCreator.ABIGAS);
			Factory.Save();

			DataTable jobForwardProfitTable = RunScript();
			var records = jobForwardProfitTable.Select("JH_JOBNUM = 'S001001'");
			AssertEquals(1, records.Length);
			AssertEquals("Should use local client's code", TestObjectCreator.LocalClient.OH_Code, records[0]["JobLocalClientARSettlementGroupCode"]);
			AssertEquals("Should use overseas agent's code", TestObjectCreator.Agent.OH_Code, records[0]["JobOverseasAgentARSettlementGroupCode"]);

			TestObjectCreator.CreateARSettlementGroup(TestObjectCreator.LocalClient, TestObjectCreator.LocalClient2);
			TestObjectCreator.CreateARSettlementGroup(TestObjectCreator.Agent, TestObjectCreator.Agent2);
			Factory.Save();

			jobForwardProfitTable = RunScript();
			records = jobForwardProfitTable.Select("JH_JOBNUM = 'S001001'");
			AssertEquals(1, records.Length);
			AssertEquals("Should use local client's AR Settlement Group code", TestObjectCreator.LocalClient2.OH_Code, records[0]["JobLocalClientARSettlementGroupCode"]);
			AssertEquals("Should use overseas agent's AR Settlement Group code", TestObjectCreator.Agent2.OH_Code, records[0]["JobOverseasAgentARSettlementGroupCode"]);
		}

		[SnailTest]
		[TestDate(2011, 1, 24, 3, 4, 0)]
		public void TestForwardingAndCustomsTransactionDetailReport_NotIncludeReversedWIPACR()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, departmentCode: "FIP");
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 102m, 101m);
			Factory.Save();
			charge.Accrual.AL_PostDate = new ZDateTime(2011, 1, 21);
			charge.WIP.AL_PostDate = new ZDateTime(2011, 1, 22);
			charge.ReverseAccrual(new ZDateTime(2011, 1, 23));
			charge.ReverseWIP(new ZDateTime(2011, 1, 23));
			Factory.Save();

			var jobForwardProfitTable = RunScript(notIncludeReversedWIPACR: "N");
			AssertEquals(4, jobForwardProfitTable.Rows.Count);
			AssertEquals(1, jobForwardProfitTable.Select("AL_LineType = 'ACR' and AL_LineAmount = 0").Length);
			AssertEquals(1, jobForwardProfitTable.Select("AL_LineType = 'WIP' and AL_LineAmount = 0").Length);
			AssertEquals(1, jobForwardProfitTable.Select("AL_LineType = 'ACR' and AL_LineAmount = -102").Length);
			AssertEquals(1, jobForwardProfitTable.Select("AL_LineType = 'WIP' and AL_LineAmount = 101").Length);

			jobForwardProfitTable = RunScript(notIncludeReversedWIPACR: "Y");
			AssertEquals(2, jobForwardProfitTable.Rows.Count);
			AssertEquals(1, jobForwardProfitTable.Select("AL_LineType = 'ACR' and AL_LineAmount = -102").Length);
			AssertEquals(1, jobForwardProfitTable.Select("AL_LineType = 'WIP' and AL_LineAmount = 101").Length);
		}

		[SnailTest]
		public void TestForwardingJobProfitReportForJobDeclaration_CheckNoDuplicateRecordsWhenMultipleContainers()
		{
			var declaration = TestObjectCreator.CreateDeclaration();
			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);

			TestObjectCreator.CreateContainer(declaration, "QWER098765");
			TestObjectCreator.CreateContainer(declaration, "QRER009123");

			Factory.Save();

			DataTable jobForwardProfitTable = RunScript();

			AssertEquals("Report should return 2 records", 2, jobForwardProfitTable.Rows.Count);

			var acrualRecords = jobForwardProfitTable.Select("AL_LINETYPE = 'ACR'");
			AssertEquals("Report should contain 1 Acrual record", 1, acrualRecords.Length);
			AssertEquals("Report should show Acrual amount as -100.00", -100.00M, (decimal)acrualRecords[0]["ACRAMOUNT"]);

			var wipRecords = jobForwardProfitTable.Select("AL_LINETYPE = 'WIP'");
			AssertEquals("Report should contain 1 WIP record", 1, wipRecords.Length);
			AssertEquals("Report should show WIP amount as 100", 100.00M, (decimal)wipRecords[0]["WIPAMOUNT"]);
		}

		[SnailTest]
		public void TestForwardingJobProfitReportForJobDeclaration_CheckTwentyFootEquivalentUnitCount()
		{
			var declaration1 = TestObjectCreator.CreateDeclaration("B0000100");
			var job1 = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration1, false);

			TestObjectCreator.CreateCharge(job1, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);

			TestObjectCreator.CreateContainer(declaration1, "QWER098765", Core.Constants.ContainerModes.FCL, Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20FR").PK, "4419151");
			TestObjectCreator.CreateContainer(declaration1, "QRER009123", Core.Constants.ContainerModes.FCL, Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20RE").PK, "4419152");
			TestObjectCreator.CreateContainer(declaration1, "HUYR098761", Core.Constants.ContainerModes.FCL, Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "40FR").PK, "4419153");

			var declaration2 = TestObjectCreator.CreateDeclaration("B0000101");
			var job2 = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration2, false);

			TestObjectCreator.CreateCharge(job2, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);

			TestObjectCreator.CreateContainer(declaration2, "QWER098761", Core.Constants.ContainerModes.FCL, Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20FR").PK, "4419154");

			Factory.Save();

			DataTable jobForwardProfitTable = RunScript();

			AssertEquals("Report should return 4 records", 4, jobForwardProfitTable.Rows.Count);

			AssertEquals("Report should show Twenty Foot Equivalent unit count as 2", 2, jobForwardProfitTable.Select("JH_JOBNUM = 'B0000100'")[0]["TWENTYFOOTEQUIVALENTUNIT"]);

			AssertEquals("Report should show Twenty Foot Equivalent unit count as 1", 1, jobForwardProfitTable.Select("JH_JOBNUM = 'B0000101'")[0]["TWENTYFOOTEQUIVALENTUNIT"]);
		}

		[SnailTest]
		public void TestForwardingJobProfitReportForJobDeclaration_CheckHouseBill()
		{
			var declaration = TestObjectCreator.CreateDeclaration();
			declaration.JE_HouseBill = "123456";
			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);

			Factory.Save();

			DataTable jobForwardProfitTable = RunScript();

			AssertEquals("Report should return 2 records", 2, jobForwardProfitTable.Rows.Count);

			var recordsWithHouseBill = jobForwardProfitTable.Select("HouseBillNumber = '123456'");
			AssertEquals("Report should contain 2 records with House Bill Number", 2, recordsWithHouseBill.Length);
		}

		[SnailTest]
		public void TestForwardingJobProfitReport_DeclarationOwnerReference()
		{
			var declaration = TestObjectCreator.CreateDeclaration("B0000100");
			declaration.JE_OwnerRef = "654321";
			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateContainer(declaration, "QWER098765", Core.Constants.ContainerModes.FCL, Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20FR").PK, "4419151");

			var declarationOther = TestObjectCreator.CreateDeclaration("B0000101");
			declarationOther.JE_OwnerRef = "654321";
			job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declarationOther, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);
			TestObjectCreator.CreateContainer(declarationOther, "QWER098765", Core.Constants.ContainerModes.FCL, Factory.LoadFromNaturalKey(typeof(RefContainer), RefContainerSchema.RC_Code, "20FR").PK, "4419151");

			var company = Factory.New<GlbCompany>();
			var branch = Factory.New<GlbBranch>();
			branch.GB_GC = company.PK;
			declarationOther.JE_GB = branch.PK;
			declarationOther.JE_GC = company.PK;
			Factory.Save();

			var jobForwardProfitTable = RunScript();
			var records = jobForwardProfitTable.Select("JH_JOBNUM = 'B0000100'");
			AssertEquals(2, records.Length);
			AssertEquals("Source is declaration, get value from JE_OwnerRef", "654321", records[0]["DeclarationOwnerReference"]);
			AssertEquals("Source is declaration, get value from JE_OwnerRef", "654321", records[1]["DeclarationOwnerReference"]);

			var shipment = TestObjectCreator.CreateShipment("S001001");
			var declaration2 = TestObjectCreator.CreateDeclaration("B0000101");
			declaration2.JE_OwnerRef = "123123";
			declaration2.JE_JS = shipment.PK;
			declarationOther.JE_JS = shipment.PK;
			job = TestObjectCreator.CreateJob(shipment, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0m, TestObjectCreator.ABIGAS);
			Factory.Save();

			jobForwardProfitTable = RunScript();
			records = jobForwardProfitTable.Select("JH_JOBNUM = 'S001001'");
			AssertEquals(1, records.Length);
			AssertEquals("Source is shipment, get value from JE_OwnerRef", "123123", records[0]["DeclarationOwnerReference"]);

			var importer = TestObjectCreator.CreateOrgHeader("DMIMPTR", false, false);
			importer.OH_FullName = "Dummy importer";

			var isf = TestObjectCreator.CreateISFHeader("1", "ISF00001", importer);
			var addressProvider = isf as IDocAddresses;

			job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)isf, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			Factory.Save();

			jobForwardProfitTable = RunScript();

			records = jobForwardProfitTable.Select("JH_JOBNUM = 'ISF00001'");
			AssertEquals(1, records.Length);
			AssertEquals("Source is ISF, value is empty", ZString.Empty, records[0]["DeclarationOwnerReference"]);

			var gCNConsol = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", "C002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var gcnJob = TestObjectCreator.CreateJob(gCNConsol, false);
			TestObjectCreator.CreateCharge(gcnJob, TestObjectCreator.CC1, osCostAmt: 15M, creditor: TestObjectCreator.AALSHI, osSellAmt: 45M, debtor: TestObjectCreator.ABIGAS);
			Factory.Save();

			AssertEquals(true, gCNConsol.IsGatewayConsol);

			jobForwardProfitTable = RunScript();

			records = jobForwardProfitTable.Select("JK_UniqueConsignRef = 'C002'");
			AssertEquals(2, records.Length);
			AssertEquals("Source is Consol, value is empty", ZString.Empty, records[0]["DeclarationOwnerReference"]);
			AssertEquals("Source is Consol, value is empty", ZString.Empty, records[1]["DeclarationOwnerReference"]);
		}

		[SnailTest]
		public void TestForwardingAndCustomsTransactionDetailReport_ForShipmentWithoutConsigneeOrConsignor()
		{
			var shipment = TestObjectCreator.CreateShipment("S001001");
			var job = TestObjectCreator.CreateJob(shipment, false);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0m, TestObjectCreator.ABIGAS);

			Factory.Save();

			AssertNull("Consignee is null", shipment.Consignee);
			AssertNull("Consignor is null", shipment.Consignor);

			DataTable jobForwardProfitTable = RunScript();

			var records = jobForwardProfitTable.Select("JH_JOBNUM = 'S001001'");
			AssertEquals("Report should contain shipment even without consigee and consignor", 1, records.Length);
		}

		[SnailTest]
		public void TestForwardingAndCustomsTransactionDetailReport_NoDuplicateRecords_ForISF_WithBuyingAndSellingParty()
		{
			var importer = TestObjectCreator.CreateOrgHeader("DMIMPTR", false, false);
			importer.OH_FullName = "Dummy importer";

			var isf = TestObjectCreator.CreateISFHeader("1", "ISF00003", importer);
			var addressProvider = isf as IDocAddresses;

			var buyingAddress = addressProvider.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.BuyingParty);
			buyingAddress.E2_AddressOverride = true;
			buyingAddress.E2_CompanyName = "DummyBuyCompany";

			var sellingAddress1 = addressProvider.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.SellingParty);
			sellingAddress1.E2_AddressOverride = true;
			sellingAddress1.E2_CompanyName = "DummySellCompany1";

			var sellingAddress2 = addressProvider.DocAddresses.AddNew(MasterFiles.Integration.DocAddressType.SellingParty);
			sellingAddress2.E2_AddressOverride = true;
			sellingAddress2.E2_CompanyName = "DummySellCompany2";

			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)isf, false);
			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 0M, TestObjectCreator.ABIGAS);
			Factory.Save();

			DataTable jobForwardProfitTable = RunScript();

			var records = jobForwardProfitTable.Select("JH_JOBNUM = 'ISF00003'");
			AssertEquals("Report should contain only one record for job ISF00003", 1, records.Length);
			AssertEquals("The record is for -100 ACR", -100M, (decimal)records.Single(x => (decimal)x["ACRAMOUNT"] != 0)["ACRAMOUNT"]);

			TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 0M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 101M, TestObjectCreator.ABIGAS);
			Factory.Save();

			jobForwardProfitTable = RunScript();

			records = jobForwardProfitTable.Select("JH_JOBNUM = 'ISF00003'");
			AssertEquals("Report should contain two records for job ISF00003", 2, records.Length);
			AssertEquals("One record is for -100 ACR", -100M, (decimal)records.Single(x => (decimal)x["ACRAMOUNT"] != 0)["ACRAMOUNT"]);
			AssertEquals("The other record is for 101 WIP", 101M, (decimal)records.Single(x => (decimal)x["WIPAMOUNT"] != 0)["WIPAMOUNT"]);
		}

		[SnailTest]
		public void TestForwardingAndCustomsTransactionDetailReport_WithNewGCN()
		{
			var gCNConsol = TestObjectCreator.CreateGatewayConsol("AUSYD", "NZAKL", "C002", receivingGatewayCompany: GlbCompany.CurrentCompany);
			var gcnJob = TestObjectCreator.CreateJob(gCNConsol, false);
			TestObjectCreator.CreateCharge(gcnJob, TestObjectCreator.CC1, osCostAmt: 15M, creditor: TestObjectCreator.AALSHI, osSellAmt: 45M, debtor: TestObjectCreator.ABIGAS);
			Factory.Save();

			AssertEquals(true, gCNConsol.IsGatewayConsol);

			var jobForwardProfitTable = RunScript();
			jobForwardProfitTable.DefaultView.Sort = "JH_JobNum ASC, AL_LineType ASC";
			var sortedJobForwardProfitTable = jobForwardProfitTable.DefaultView.ToTable();

			AssertEquals(2, sortedJobForwardProfitTable.Rows.Count);

			var headers = new string[] { "AL_LineType", "AL_LineAmount", "JH_JobNum" };
			var lines = new List<object[]>
				{
					new object[] { "ACR", -15M, "C002" },
					new object[] { "WIP", 45M, "C002" },
				};

			AssertDataTableAllRowsByKeyColumns("", sortedJobForwardProfitTable, headers, lines.ToArray());
		}

		[SnailTest]
		public void TestForwardingAndCustomsTransactionDetailReport_ControllingCustomerAndAgent()
		{
			var infoChecker = new CAGAndCCBInfoScriptTest();
			infoChecker.VerifyControllingCustomerAndAgentInfo(RunScriptForCAGAndCCBInfoScriptTest);
		}

		[SnailTest]
		public void TestTaxExpenseRowsAreReturnedCorrectly()
		{
			var declaration = TestObjectCreator.CreateDeclaration();
			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);

			TestObjectCreator.CreateContainer(declaration, "QWER098765");
			TestObjectCreator.CreateContainer(declaration, "QRER009123");

			var revenueLine = TestObjectCreator.CreateRevenueLine(charge, TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD).PK);
			revenueLine.AL_PostDate = new ZDateTime(2011, 2, 15);
			revenueLine.AL_ReverseDate = new ZDate(2011, 2, 18);

			var costLine = TestObjectCreator.CreateCostLine(charge, TestObjectCreator.CreateInvoice(typeof(APInvoice)).PK);
			costLine.AL_PostDate = new ZDateTime(2011, 2, 15);
			costLine.AL_ReverseDate = new ZDate(2011, 2, 18);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxableTransactionLine = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 18), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord1.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(taxableTransactionLine);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 18), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord2.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(taxableTransactionLine);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			DataTable jobForwardProfitTable = RunScript();

			var resultsREV = jobForwardProfitTable.Select(AccTransactionLines.Schema.PK + $" = '{revenueLine.PK}'");
			AssertEquals(1, resultsREV.Length);
			AssertEquals(revenueLine.PK, resultsREV[0]["AL_PK"]);
			AssertEquals(100m, resultsREV[0]["AL_LineAmount"]);
			AssertEquals(0m, resultsREV[0]["WIPAmount"]);
			AssertEquals(0m, resultsREV[0]["CSTAmount"]);
			AssertEquals(0m, resultsREV[0]["ACRAmount"]);
			AssertEquals(100m, resultsREV[0]["REVAmount"]);
			AssertEquals(-13m, resultsREV[0]["TaxExpenseAmount"]);
			AssertEquals(new ZDate(2011, 2, 19), resultsREV[0]["TaxExpenseRealisationDate"]);
		}

		[SnailTest]
		public void TestTaxExpenseRowsWithDebtorOrgFiltering()
		{
			var declaration = TestObjectCreator.CreateDeclaration();
			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);

			TestObjectCreator.CreateContainer(declaration, "QWER098765");
			TestObjectCreator.CreateContainer(declaration, "QRER009123");

			var revenueLine = TestObjectCreator.CreateRevenueLine(charge, TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD, 1.2m, TestObjectCreator.Debtor).PK);
			revenueLine.AL_PostDate = new ZDateTime(2011, 2, 15);
			revenueLine.AL_ReverseDate = new ZDate(2011, 2, 18);

			var costLine = TestObjectCreator.CreateCostLine(charge, TestObjectCreator.CreateInvoice(typeof(APInvoice)).PK);
			costLine.AL_PostDate = new ZDateTime(2011, 2, 15);
			costLine.AL_ReverseDate = new ZDate(2011, 2, 18);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxableTransactionLine = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(revenueLine);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 18), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord1.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(taxableTransactionLine);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 18), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord2.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(taxableTransactionLine);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			DataTable jobForwardProfitTable = RunScript();
			var resultsREV = jobForwardProfitTable.Select(AccTransactionLines.Schema.PK + $" = '{revenueLine.PK}'");
			AssertEquals(1, resultsREV.Length);
			AssertEquals(revenueLine.PK, resultsREV[0]["AL_PK"]);
			AssertEquals(100m, resultsREV[0]["AL_LineAmount"]);
			AssertEquals(0m, resultsREV[0]["WIPAmount"]);
			AssertEquals(0m, resultsREV[0]["CSTAmount"]);
			AssertEquals(0m, resultsREV[0]["ACRAmount"]);
			AssertEquals(100m, resultsREV[0]["REVAmount"]);
			AssertEquals(-13m, resultsREV[0]["TaxExpenseAmount"]);
			AssertEquals(new ZDate(2011, 2, 19), resultsREV[0]["TaxExpenseRealisationDate"]);

			jobForwardProfitTable = RunScript($"WHERE DebtorOrg = '{TestObjectCreator.Debtor.PK}'");
			resultsREV = jobForwardProfitTable.Select(AccTransactionLines.Schema.PK + $" = '{revenueLine.PK}'");
			AssertEquals(1, resultsREV.Length);

			jobForwardProfitTable = RunScript($"WHERE DebtorOrg = '{TestObjectCreator.Debtor1.PK}'");
			resultsREV = jobForwardProfitTable.Select(AccTransactionLines.Schema.PK + $" = '{revenueLine.PK}'");
			AssertEquals(0, resultsREV.Length);
		}

		[SnailTest]
		public void TestTaxExpenseRowsWithCreditorOrgFiltering()
		{
			var declaration = TestObjectCreator.CreateDeclaration();
			var job = TestObjectCreator.CreateJob((IJobInvoicingPlugIn)declaration, false);

			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, string.Empty, TestObjectCreator.AUD, 100M, TestObjectCreator.AALSHI, TestObjectCreator.AUD, 100M, TestObjectCreator.ABIGAS);

			TestObjectCreator.CreateContainer(declaration, "QWER098765");
			TestObjectCreator.CreateContainer(declaration, "QRER009123");

			var revenueLine = TestObjectCreator.CreateRevenueLine(charge, TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD).PK);
			revenueLine.AL_PostDate = new ZDateTime(2011, 2, 15);
			revenueLine.AL_ReverseDate = new ZDate(2011, 2, 18);

			var costLine = TestObjectCreator.CreateCostLine(charge, TestObjectCreator.CreateInvoice(typeof(APInvoice), TestObjectCreator.USD, 1.2m, TestObjectCreator.Creditor1).PK);
			costLine.AL_PostDate = new ZDateTime(2011, 2, 15);
			costLine.AL_ReverseDate = new ZDate(2011, 2, 18);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var taxableTransactionLine = TaxFrameworkObjectFactory.GetInvoicingLineBaseTaxable(costLine);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = costLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 18), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord1.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(taxableTransactionLine);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = costLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 18), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord2.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(taxableTransactionLine);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			DataTable jobForwardProfitTable = RunScript();
			var resultsCST = jobForwardProfitTable.Select(AccTransactionLines.Schema.PK + $" = '{costLine.PK}'");
			AssertEquals(1, resultsCST.Length);
			AssertEquals(costLine.PK, resultsCST[0]["AL_PK"]);
			AssertEquals(-100m, resultsCST[0]["AL_LineAmount"]);
			AssertEquals(0m, resultsCST[0]["WIPAmount"]);
			AssertEquals(-100m, resultsCST[0]["CSTAmount"]);
			AssertEquals(0m, resultsCST[0]["ACRAmount"]);
			AssertEquals(0m, resultsCST[0]["REVAmount"]);
			AssertEquals(-13m, resultsCST[0]["TaxExpenseAmount"]);
			AssertEquals(new ZDate(2011, 2, 19), resultsCST[0]["TaxExpenseRealisationDate"]);

			jobForwardProfitTable = RunScript($"WHERE CreditorOrg = '{TestObjectCreator.Creditor1.PK}'");
			resultsCST = jobForwardProfitTable.Select(AccTransactionLines.Schema.PK + $" = '{costLine.PK}'");
			AssertEquals(1, resultsCST.Length);

			jobForwardProfitTable = RunScript($"WHERE CreditorOrg = '{TestObjectCreator.Creditor2.PK}'");
			resultsCST = jobForwardProfitTable.Select(AccTransactionLines.Schema.PK + $" = '{costLine.PK}'");
			AssertEquals(0, resultsCST.Length);
		}

		protected override DataTable RunScriptWithFilter(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			return RunScript(activeStatus: activeStatus);
		}

		DataTable RunScript(string whereClause = "", ZGuid? mNGOrgPK = null, string notIncludeReversedWIPACR = "", string activeStatus = "")
		{
			string sqlQuery = $@"
			SELECT *
			FROM Report_ForwardingJobProfitAnalysis
			(
				'AU' --@CurrentCountry
				, '{GlbCompany.CurrentCompany.PK.ToString()}' --@CompanyPK
				, '1900-01-01 00:00:00' --@TransactionFrom
				, '2079-06-06 23:59:29' --@TransactionTo
				, N'' --@JobType
				, N'' --@OutstandingWIP
				, N'' --@OutstandingACR
				, '{notIncludeReversedWIPACR}'
				, N'' --@ChargeCode
				, N'' --@ExcludeChargeCode
				, N'' --@ChargeGroup
				, NULL --@SalesGroup
				, NULL --@ExpenseGroup 
				, N'' --@TransactionBranch
				, N'' --@TransactionDepartment
				, N'' --@PostedOnly
				, '1900-01-01 00:00:00' --@RevRecogFrom
				, '2079-06-06 23:59:29' --@RevRecogTo
				, NULL --@ShipmentTransport
				, NULL --@DeclarationTransport
				, NULL --@ShipmentCont
				, NULL --@DeclarationCont
				, NULL --@ISFTransportMode 
				, NULL --@ISFShipmentType 
				, @MNGListValue
				, @MNGListIsEmptyValue
				, 'ALL' --@Gateway
				, '{activeStatus}' --@ActiveStatus
				)

			 {whereClause}
				";

			var command = Db.Connection.Command(sqlQuery);
			AddTVPAndIsEmptyParameters(command, "@MNGListValue", "dbo.TVP_uniqueidentifier", "@MNGListIsEmptyValue", mNGOrgPK.HasValue ? new Guid[] { mNGOrgPK.Value.ToGuid() } : Array.Empty<Guid>());
			return DataUtils.GetDataTableFromCommand(command);
		}

		protected DataTable RunScriptForCAGAndCCBInfoScriptTest(string whereClause = "", ZGuid? mNGOrgPK = null)
		{
			return RunScript(whereClause, mNGOrgPK, "");
		}

		TaxFrameworkTestObjectCreator TaxFrameworkTestObjectCreator
		{
			get
			{
				if (taxFrameworkTestObjectCreator == null)
				{
					taxFrameworkTestObjectCreator = new TaxFrameworkTestObjectCreator(Factory);
				}
				return taxFrameworkTestObjectCreator;
			}
		}
		TaxFrameworkTestObjectCreator taxFrameworkTestObjectCreator;
	}
}
