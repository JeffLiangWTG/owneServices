using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.AccountingMasterFilesTaxFrameworkConstants;

namespace Enterprise.Accounting.Business.Testing.ScriptTests
{
	class csfn_AllJobProfitDetailCoreWithTaxExpenseTest : csfn_AllJobProfitDetailCoreTest
	{
		#region Active Status filter test

		protected override bool ShouldTestActiveStatusFilter => true;

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

		public void TestWithTaxExpense()
		{
			var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1);
			charge.JR_LocalCostAmt = 50M;
			charge.JR_LocalSellAmt = 100M;
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

			var results = RunScript("NULL", "NULL");
			var resultsREV = results.Select(AccTransactionLines.Schema.PK + $" = '{revenueLine.PK}'");
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

		public void TestWithoutTaxExpense()
		{
			var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1);
			charge.JR_LocalCostAmt = 50M;
			charge.JR_LocalSellAmt = 100M;
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
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 18), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord1.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(taxableTransactionLine);
			pivot1.ATP_IsTaxExpense = false;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 18), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord2.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(taxableTransactionLine);
			pivot2.ATP_IsTaxExpense = false;

			Factory.Save();

			var results = RunScript("NULL", "NULL");
			var resultsREV = results.Select(AccTransactionLines.Schema.PK + $" = '{revenueLine.PK}'");
			AssertEquals(1, resultsREV.Length);
			AssertEquals(revenueLine.PK, resultsREV[0]["AL_PK"]);
			AssertEquals(100m, resultsREV[0]["AL_LineAmount"]);
			AssertEquals(0m, resultsREV[0]["WIPAmount"]);
			AssertEquals(0m, resultsREV[0]["CSTAmount"]);
			AssertEquals(0m, resultsREV[0]["ACRAmount"]);
			AssertEquals(100m, resultsREV[0]["REVAmount"]);
			AssertEquals(0m, resultsREV[0]["TaxExpenseAmount"]);
			AssertEquals(System.DBNull.Value, resultsREV[0]["TaxExpenseRealisationDate"]);
		}

		public void TestTaxExpenseDateGreaterThanTransactionToDate()
		{
			var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1);
			charge.JR_LocalCostAmt = 50M;
			charge.JR_LocalSellAmt = 100M;
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

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(revenueLine.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 19), RealisationDate = new ZDate(2011, 2, 21), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord1.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 19), RealisationDate = new ZDate(2011, 2, 21), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord2.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var results = RunScript("NULL", "NULL");
			var resultsREV = results.Select(AccTransactionLines.Schema.PK + $" = '{revenueLine.PK}'");
			AssertEquals(1, resultsREV.Length);
			AssertEquals(revenueLine.PK, resultsREV[0]["AL_PK"]);
			AssertEquals(100m, resultsREV[0]["AL_LineAmount"]);
			AssertEquals(0m, resultsREV[0]["WIPAmount"]);
			AssertEquals(0m, resultsREV[0]["CSTAmount"]);
			AssertEquals(0m, resultsREV[0]["ACRAmount"]);
			AssertEquals(100m, resultsREV[0]["REVAmount"]);
			AssertEquals(0m, resultsREV[0]["TaxExpenseAmount"]);
			AssertEquals(System.DBNull.Value, resultsREV[0]["TaxExpenseRealisationDate"]);
		}

		public void TestTaxExpenseDateLessThanTransactionFromDate()
		{
			var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1);
			charge.JR_LocalCostAmt = 50M;
			charge.JR_LocalSellAmt = 100M;
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

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(revenueLine.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 19), RealisationDate = new ZDate(2011, 1, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord1.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 19), RealisationDate = new ZDate(2011, 1, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord2.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var results = RunScript("NULL", "NULL");
			var resultsREV = results.Select(AccTransactionLines.Schema.PK + $" = '{revenueLine.PK}'");
			AssertEquals(1, resultsREV.Length);
			AssertEquals(revenueLine.PK, resultsREV[0]["AL_PK"]);
			AssertEquals(100m, resultsREV[0]["AL_LineAmount"]);
			AssertEquals(0m, resultsREV[0]["WIPAmount"]);
			AssertEquals(0m, resultsREV[0]["CSTAmount"]);
			AssertEquals(0m, resultsREV[0]["ACRAmount"]);
			AssertEquals(100m, resultsREV[0]["REVAmount"]);
			AssertEquals(0m, resultsREV[0]["TaxExpenseAmount"]);
			AssertEquals(System.DBNull.Value, resultsREV[0]["TaxExpenseRealisationDate"]);
		}

		public void TestPostDateGreaterThanTransactionToDate()
		{
			var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1);
			charge.JR_LocalCostAmt = 50M;
			charge.JR_LocalSellAmt = 100M;
			var revenueLine = TestObjectCreator.CreateRevenueLine(charge, TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD).PK);
			revenueLine.AL_PostDate = new ZDateTime(2011, 2, 21);
			revenueLine.AL_ReverseDate = new ZDate(2011, 2, 21);

			var costLine = TestObjectCreator.CreateCostLine(charge, TestObjectCreator.CreateInvoice(typeof(APInvoice)).PK);
			costLine.AL_PostDate = new ZDateTime(2011, 2, 21);
			costLine.AL_ReverseDate = new ZDate(2011, 2, 21);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(revenueLine.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 19), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord1.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 19), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord2.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var results = RunScript("NULL", "NULL");
			var resultsREV = results.Select($"TaxExpenseAmount = -13.0000");
			AssertEquals(1, resultsREV.Length);
			AssertEquals(revenueLine.PK, resultsREV[0]["AL_PK"]);
			AssertEquals(0m, resultsREV[0]["AL_LineAmount"]);
			AssertEquals(0m, resultsREV[0]["WIPAmount"]);
			AssertEquals(0m, resultsREV[0]["CSTAmount"]);
			AssertEquals(0m, resultsREV[0]["ACRAmount"]);
			AssertEquals(0m, resultsREV[0]["REVAmount"]);
			AssertEquals(-13m, resultsREV[0]["TaxExpenseAmount"]);
			AssertEquals(new ZDate(2011, 2, 19), resultsREV[0]["TaxExpenseRealisationDate"]);
		}

		public void TestPostDateLessThanTransactionFromDate()
		{
			var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1);
			charge.JR_LocalCostAmt = 50M;
			charge.JR_LocalSellAmt = 100M;
			var revenueLine = TestObjectCreator.CreateRevenueLine(charge, TestObjectCreator.CreateInvoice(typeof(ARInvoice), TestObjectCreator.USD).PK);
			revenueLine.AL_PostDate = new ZDateTime(2011, 1, 19);
			revenueLine.AL_ReverseDate = new ZDateTime(2011, 1, 19);

			var costLine = TestObjectCreator.CreateCostLine(charge, TestObjectCreator.CreateInvoice(typeof(APInvoice)).PK);
			costLine.AL_PostDate = new ZDateTime(2011, 1, 19);
			costLine.AL_ReverseDate = new ZDateTime(2011, 1, 19);

			var taxSystem = TaxFrameworkTestObjectCreator.CreateTaxSystem("AUSPR", taxSuperType: TaxSuperTypeList.StandardPaymentRetention.Code);
			taxSystem.Name = "PBW - COMPANY LVL - NAT AUTH - OFT NEG";

			var taxAuthority = TaxFrameworkTestObjectCreator.CreateTaxAuthority("N01AU");
			taxAuthority.Name = "AU NATIONAL - ATO";

			var taxConfig = TaxFrameworkTestObjectCreator.CreateTaxConfiguration(GlbCompany.CurrentCompany, taxAuthority, taxSystem, TaxConfigurationLedgers.AccountsPayable.Code);

			var lineMock = new Mock<ITaxableTransactionLine>();
			lineMock.Setup(l => l.PK).Returns(revenueLine.PK);
			lineMock.Setup(l => l.BaseOSAmount).Returns(300m);
			var taxRecord1 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 19), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -10M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord1.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot1 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot1.ATP_ATT = taxRecord1.PK;
			pivot1.LinkLine(lineMock.Object);
			pivot1.ATP_IsTaxExpense = true;
			var taxRecord2 = TaxFrameworkTestObjectCreator.CreateTaxTransaction(new TaxFrameworkTestObjectCreator.CreateTaxTransactionParameters { TransactionHeaderPK = revenueLine.TransactionHeader.PK, TaxSystem = taxSystem, TaxConfiguration = taxConfig, TaxBasis = TaxBasisList.PostingOnMatching.Code, BranchPK = TestObjectCreator.NonCurrentBranch.PK, DepartmentPK = TestObjectCreator.NonCurrentDepartment.PK, PostDate = new ZDate(2011, 2, 19), RealisationDate = new ZDate(2011, 2, 19), OsTaxBaseAmount = 300M, OsTaxAmount = -45M, LocalTaxBaseAmount = 300M, LocalTaxAmount = -3M, TaxExpenseAccountPK = TestObjectCreator.GLHeader1.PK, DoesNotCreateGLMovemetsOnSaving = true });
			TaxFrameworkTestObjectCreator.CreateAccTaxGLMovement(taxTransactionPK: taxRecord2.PK, amount: 4M, date: ZDateTime.Today.Date, type: TaxGLMovementTypeList.Normal.Code);
			var pivot2 = Factory.New<AccTaxRecordTransactionLinePivot>();
			pivot2.ATP_ATT = taxRecord2.PK;
			pivot2.LinkLine(lineMock.Object);
			pivot2.ATP_IsTaxExpense = true;

			Factory.Save();

			var results = RunScript("NULL", "NULL");
			var resultsREV = results.Select($"TaxExpenseAmount = -13.0000");
			AssertEquals(1, resultsREV.Length);
			AssertEquals(revenueLine.PK, resultsREV[0]["AL_PK"]);
			AssertEquals(0m, resultsREV[0]["AL_LineAmount"]);
			AssertEquals(0m, resultsREV[0]["WIPAmount"]);
			AssertEquals(0m, resultsREV[0]["CSTAmount"]);
			AssertEquals(0m, resultsREV[0]["ACRAmount"]);
			AssertEquals(0m, resultsREV[0]["REVAmount"]);
			AssertEquals(-13m, resultsREV[0]["TaxExpenseAmount"]);
			AssertEquals(new ZDate(2011, 2, 19), resultsREV[0]["TaxExpenseRealisationDate"]);
		}

		[TestDate(2011, 1, 24, 3, 4, 0)]
		public void TestReversedWIPandACR()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, departmentCode: "FIP");
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 102m, 101m);
			Factory.Save();
			charge.Accrual.AL_PostDate = new ZDateTime(2011, 1, 21);
			charge.WIP.AL_PostDate = new ZDateTime(2011, 1, 22);
			charge.ReverseAccrual(new ZDateTime(2011, 1, 23));
			charge.ReverseWIP(new ZDateTime(2011, 1, 23));
			Factory.Save();

			var result = RunScript("NULL", "NULL");
			AssertEquals(4, result.Rows.Count);
			AssertEquals(1, result.Select("AL_LineType = 'ACR' and AL_LineAmount = 0 and JH_JobNum = 'S00001000'").Length);
			AssertEquals(1, result.Select("AL_LineType = 'WIP' and AL_LineAmount = 0 and JH_JobNum = 'S00001000'").Length);
			AssertEquals(1, result.Select("AL_LineType = 'ACR' and AL_LineAmount = -102 and JH_JobNum = 'S00001000'").Length);
			AssertEquals(1, result.Select("AL_LineType = 'WIP' and AL_LineAmount = 101 and JH_JobNum = 'S00001000'").Length);
		}

		[TestDate(2011, 1, 24, 3, 4, 0)]
		public void TestNotIncludeReversedWIPACR()
		{
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code, departmentCode: "FIP");
			var charge = TestObjectCreator.CreateCharge(job, TestObjectCreator.CC1, 102m, 101m);
			Factory.Save();
			charge.Accrual.AL_PostDate = new ZDateTime(2011, 1, 21);
			charge.WIP.AL_PostDate = new ZDateTime(2011, 1, 22);
			charge.ReverseAccrual(new ZDateTime(2011, 1, 23));
			charge.ReverseWIP(new ZDateTime(2011, 1, 23));
			Factory.Save();

			var result = RunScriptWithNotIncludeReversedWIPACR("N");
			AssertEquals(4, result.Rows.Count);
			AssertEquals(1, result.Select("AL_LineType = 'ACR' and AL_LineAmount = 0 and JH_JobNum = 'S00001000'").Length);
			AssertEquals(1, result.Select("AL_LineType = 'WIP' and AL_LineAmount = 0 and JH_JobNum = 'S00001000'").Length);
			AssertEquals(1, result.Select("AL_LineType = 'ACR' and AL_LineAmount = -102 and JH_JobNum = 'S00001000'").Length);
			AssertEquals(1, result.Select("AL_LineType = 'WIP' and AL_LineAmount = 101 and JH_JobNum = 'S00001000'").Length);

			result = RunScriptWithNotIncludeReversedWIPACR("Y");
			AssertEquals(2, result.Rows.Count);
			AssertEquals(1, result.Select("AL_LineType = 'ACR' and AL_LineAmount = -102 and JH_JobNum = 'S00001000'").Length);
			AssertEquals(1, result.Select("AL_LineType = 'WIP' and AL_LineAmount = 101 and JH_JobNum = 'S00001000'").Length);
		}

		protected override DataTable RunScript(string chargeGroup)
		{
			string sql = $@"SELECT * FROM csfn_AllJobProfitDetailCoreWithTaxExpense('{GlbCompany.CurrentCompany.PK.ToString()}','1900-01-01 00:00:00','2079-06-06 23:59:29','','','','',''
							           ,NULL,'{chargeGroup}',NULL,NULL,'','',NULL,'',NULL,'')";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		protected override DataTable RunScript(string postedOnly, string unPostedOnly)
		{
			string sql = $@"SELECT *
							FROM csfn_AllJobProfitDetailCoreWithTaxExpense('{GlbCompany.CurrentCompany.PK.ToString()}','2011-01-20 00:00:00','2011-02-20 23:59:29','','','','',''
							,NULL,NULL,NULL,NULL,'','',{postedOnly},'',{unPostedOnly},'')";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		DataTable RunScriptWithNotIncludeReversedWIPACR(string notIncludeReversedWIPACR)
		{
			string sql = $@"SELECT * FROM csfn_AllJobProfitDetailCoreWithTaxExpense('{GlbCompany.CurrentCompany.PK.ToString()}','1900-01-01 00:00:00','2079-06-06 23:59:29','','','',
							'{notIncludeReversedWIPACR}','',NULL,NULL,NULL,NULL,'','',NULL,'',NULL,'')";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
		}

		protected override DataTable RunScriptWithFilter(string activeStatus = "", string notIncludeReversedWIPACR = "")
		{
			string sql = $@"SELECT * FROM csfn_AllJobProfitDetailCoreWithTaxExpense('{GlbCompany.CurrentCompany.PK.ToString()}','1900-01-01 00:00:00','2079-06-06 23:59:29','','','','',''
							           ,NULL,NULL,NULL,NULL,'','',NULL,'',NULL,'{activeStatus}')";
			return DataUtils.GetDataTableFromQuery(Db.Connection, sql);
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


