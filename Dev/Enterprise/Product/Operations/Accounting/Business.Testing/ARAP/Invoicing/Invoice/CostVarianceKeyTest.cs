using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing.CostVarianceApprovalExtension;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;
using CostVarianceKey = Enterprise.Accounting.Business.ARAP.Invoicing.CostVarianceApprovalHelper.CostVarianceKey;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class CostVarianceKeyTest : TestCaseWithFactory
	{
		public void TestCostVarianceKeyPK()
		{
			var listOption = new CostVarianceApproval().VarianceComparisonOptionsList.GetAllCodes();
			foreach (var option in listOption)
			{
				Factory.ClearCachedCostVarianceApproval(GlbCompany.CurrentCompany);
				SetCostVarianceKeyOption(option);
				var invoiceLineWithoutImportedCharge = new CostVarianceKey(GetAPInvoiceLine(false));
				var invoiceLineWithImportedCharge = new CostVarianceKey(GetAPInvoiceLine(true));
				var splitChargeWithoutImportedConsolCost = new CostVarianceKey(GetApportionSplitCharge(false));
				var splitChargeWithImportedConsolCost = new CostVarianceKey(GetApportionSplitCharge(true));
				switch (option)
				{
					case Core.Constants.CostVarianceComparisonOption.Job:
						AssertCostVarianceKeyPK(invoiceLineWithoutImportedCharge, false, true, true, false, false);
						AssertCostVarianceKeyPK(invoiceLineWithImportedCharge, false, true, true, false, false);
						AssertCostVarianceKeyPK(splitChargeWithoutImportedConsolCost, false, true, true, false, false);
						AssertCostVarianceKeyPK(splitChargeWithImportedConsolCost, false, true, true, false, false);
						break;
					case Core.Constants.CostVarianceComparisonOption.JobAndChargeCode:
						AssertCostVarianceKeyPK(invoiceLineWithoutImportedCharge, false, true, true, true, false);
						AssertCostVarianceKeyPK(invoiceLineWithImportedCharge, false, true, true, true, false);
						AssertCostVarianceKeyPK(splitChargeWithoutImportedConsolCost, false, true, true, true, false);
						AssertCostVarianceKeyPK(splitChargeWithImportedConsolCost, false, true, true, true, false);
						break;
					case Core.Constants.CostVarianceComparisonOption.JobAndCreditor:
						AssertCostVarianceKeyPK(invoiceLineWithoutImportedCharge, false, true, true, false, true);
						AssertCostVarianceKeyPK(invoiceLineWithImportedCharge, false, true, true, false, true);
						AssertCostVarianceKeyPK(splitChargeWithoutImportedConsolCost, false, true, true, false, true);
						AssertCostVarianceKeyPK(splitChargeWithImportedConsolCost, false, true, true, false, true);
						break;
					case Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJob:
						AssertCostVarianceKeyPK(invoiceLineWithoutImportedCharge, false, true, true, false, false);
						AssertCostVarianceKeyPK(invoiceLineWithImportedCharge, true, false, false, false, false);
						AssertCostVarianceKeyPK(splitChargeWithoutImportedConsolCost, false, true, true, false, false);
						AssertCostVarianceKeyPK(splitChargeWithImportedConsolCost, true, false, false, false, false);
						break;
					case Core.Constants.CostVarianceComparisonOption.ImportedChargeOrJobChargeCode:
						AssertCostVarianceKeyPK(invoiceLineWithoutImportedCharge, false, true, true, true, false);
						AssertCostVarianceKeyPK(invoiceLineWithImportedCharge, true, false, false, false, false);
						AssertCostVarianceKeyPK(splitChargeWithoutImportedConsolCost, false, true, true, true, false);
						AssertCostVarianceKeyPK(splitChargeWithImportedConsolCost, true, false, false, false, false);
						break;
					case Core.Constants.CostVarianceComparisonOption.ImportedChargeOrCreditor:
						AssertCostVarianceKeyPK(invoiceLineWithoutImportedCharge, false, true, true, false, true);
						AssertCostVarianceKeyPK(invoiceLineWithImportedCharge, true, false, false, false, false);
						AssertCostVarianceKeyPK(splitChargeWithoutImportedConsolCost, false, true, true, false, true);
						AssertCostVarianceKeyPK(splitChargeWithImportedConsolCost, true, false, false, false, false);
						break;
					default:
						break;
				}
			}
		}

		void AssertCostVarianceKeyPK(CostVarianceKey key, bool isChargePKValid, bool isBranchPKValid, bool isDepartmentPKValid, bool isChargeCodeValid, bool isCreditorPKValid)
		{
			AssertEquals("ChargePK", isChargePKValid, key.ChargePK.IsValid);
			AssertEquals("BranchPK", isBranchPKValid, key.BranchPK.IsValid);
			AssertEquals("DepartmentPK", isDepartmentPKValid, key.DepartmentPK.IsValid);
			AssertEquals("ChargeCode", isChargeCodeValid, key.ChargeCodePK.IsValid);
			AssertEquals("CreditorPK", isCreditorPKValid, key.CreditorPK.IsValid);
		}

		APInvoiceLine GetAPInvoiceLine(bool hasOriginalJobCharge)
		{
			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP100001", TestObjectCreator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, TestObjectCreator.Creditor1);
			var apInvoiceLine = TestObjectCreator.CreateAPInvoiceLine(apInvoice, TestObjectCreator.Job1, TestObjectCreator.CC1, null, 0M, null, 0M);
			var charge = Factory.NewWithValidTestData<JobCharge>();

			if (hasOriginalJobCharge)
			{
				apInvoiceLine.OriginalJobCharge = charge;
			}

			return apInvoiceLine;
		}

		ApportionSplitCharge GetApportionSplitCharge(bool isImportedFromConsolCost)
		{
			var consolCost = Factory.NewWithValidTestData<JobConsolCost>();
			var apportionCharge = consolCost.ApportionmentCharges.AddNew();
			apportionCharge.JR_JH = TestObjectCreator.Job1.PK;
			apportionCharge.JR_GB = GlbBranch.CurrentBranch.PK;
			apportionCharge.JR_GE = GlbDepartment.CurrentDepartment.PK;
			apportionCharge.JR_AC = TestObjectCreator.CC1.PK;
			apportionCharge.JR_OH_CostAccount = TestObjectCreator.Creditor1.PK;

			if (isImportedFromConsolCost)
			{
				apportionCharge.RelatedApportionChargeFromDB = Factory.NewWithValidTestData<JobCharge>();
			}

			return apportionCharge;
		}

		public void TestIsValid()
		{
			var apInvoiceLine = TestObjectCreator.CreateAPInvoiceLine(null, TestObjectCreator.Job1, TestObjectCreator.CC1, null, 0M, null, 0M);
			var key = new CostVarianceKey(apInvoiceLine);
			Assert(key.IsValid);

			apInvoiceLine.AL_GB = ZGuid.Empty;
			key = new CostVarianceKey(apInvoiceLine);
			Assert(!key.IsValid);

			var apportionCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			key = new CostVarianceKey(apportionCharge);
			Assert(key.IsValid);

			apportionCharge.JR_GB = ZGuid.Empty;
			key = new CostVarianceKey(apportionCharge);
			Assert(!key.IsValid);
		}

		public void TestEquals()
		{
			var apInvoiceLine = Factory.NewWithValidTestData<APInvoiceLine>();
			var key1 = new CostVarianceKey(apInvoiceLine);

			Assert(key1 != null);
			Assert(!key1.Equals(null));

			var apportionCharge = Factory.NewWithValidTestData<ApportionSplitCharge>();
			var key2 = new CostVarianceKey(apportionCharge);

			Assert(key1 != key2);
			Assert(!key1.Equals(key2));

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("AP100001", TestObjectCreator.AUD, 1.0m, 250m, 25m, 0m, 250m, 25m, 0m, TestObjectCreator.Creditor1);
			var apInvoiceLine2 = TestObjectCreator.CreateAPInvoiceLine(apInvoice, TestObjectCreator.Job1, TestObjectCreator.CC1, null, 0M, null, 0M);
			apInvoiceLine2.AL_JH = apInvoiceLine.AL_JH;
			apInvoiceLine2.AL_GB = apInvoiceLine.AL_GB;
			apInvoiceLine2.AL_GE = apInvoiceLine.AL_GE;
			apInvoiceLine2.AL_AC = apInvoiceLine.AL_AC;
			var key3 = new CostVarianceKey(apInvoiceLine2);

			Assert(key1 == key3);
			Assert(key1.Equals(key3));
		}

		public void TestLoadLineUnpostedCostsFromDBForCJR()
		{
			AssertLoadLineUnpostedCostsFromDBForImportedCharge(CostVarianceComparisonOption.ImportedChargeOrCreditor);
		}

		public void TestLoadLineUnpostedCostsFromDBForCJB()
		{
			AssertLoadLineUnpostedCostsFromDBForImportedCharge(CostVarianceComparisonOption.ImportedChargeOrJob);
		}

		public void TestLoadLineUnpostedCostsFromDBForCCH()
		{
			AssertLoadLineUnpostedCostsFromDBForImportedCharge(CostVarianceComparisonOption.ImportedChargeOrJobChargeCode);
		}

		void AssertLoadLineUnpostedCostsFromDBForImportedCharge(string comparisonOption)
		{
			SetCostVarianceKeyOption(comparisonOption);

			var charge1 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, 100m, 100m);
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var apInvoice = TestObjectCreator.CreateAPInvoice<APInvoice>("inv001", TestObjectCreator.AUD, 1m, 150m, 0m, 0m, 150m, 0m, 0m, TestObjectCreator.Creditor1);

			var invoiceLine1 = TestObjectCreator.CreateAPInvoiceLine(apInvoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1m, null, 100m);
			invoiceLine1.AL_GB = GlbBranch.CurrentBranch.PK;
			invoiceLine1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			invoiceLine1.OriginalJobCharge = charge1;

			var invoiceLine2 = TestObjectCreator.CreateAPInvoiceLine(apInvoice, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, null, 50m);
			invoiceLine2.AL_GB = GlbBranch.CurrentBranch.PK;
			invoiceLine2.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var keysToGetFromDB = new HashSet<CostVarianceKey>();
			keysToGetFromDB.Add(new CostVarianceKey(invoiceLine1));
			keysToGetFromDB.Add(new CostVarianceKey(invoiceLine2));

			var unpostedCosts = CostVarianceKey.LoadLineUnpostedCostsFromDB(keysToGetFromDB, TestObjectCreator.Creditor1.PK);
			AssertEquals(1, unpostedCosts.Count);
			Assert(unpostedCosts.Keys.Any(x => x.ChargePK == charge1.PK));
		}

		public void TestLoadLineUnpostedCostsFromDB()
		{
			SetCostVarianceKeyOption(CostVarianceComparisonOption.ImportedChargeOrJob);

			var charge1 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, 100M, 100M);
			charge1.JR_GB = GlbBranch.CurrentBranch.PK;
			charge1.JR_GE = GlbDepartment.CurrentDepartment.PK;

			var charge2 = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC2, 100M, 100M);
			charge2.JR_GB = GlbBranch.CurrentBranch.PK;
			charge2.JR_GE = GlbDepartment.CurrentDepartment.PK;

			Factory.Save();

			var invoiceLine1 = TestObjectCreator.CreateAPInvoiceLine(null, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, null, 100M);
			invoiceLine1.AL_GB = GlbBranch.CurrentBranch.PK;
			invoiceLine1.AL_GE = GlbDepartment.CurrentDepartment.PK;
			invoiceLine1.OriginalJobCharge = charge1;

			var invoiceLine2 = TestObjectCreator.CreateAPInvoiceLine(null, TestObjectCreator.Job1, TestObjectCreator.CC2, TestObjectCreator.AUD, 1M, null, 100M);
			invoiceLine2.AL_GB = GlbBranch.CurrentBranch.PK;
			invoiceLine2.AL_GE = GlbDepartment.CurrentDepartment.PK;
			invoiceLine2.OriginalJobCharge = charge2;

			var invoiceLine3 = TestObjectCreator.CreateAPInvoiceLine(null, TestObjectCreator.Job1, TestObjectCreator.CC3, TestObjectCreator.AUD, 1M, null, 100M);
			invoiceLine3.AL_GB = GlbBranch.CurrentBranch.PK;
			invoiceLine3.AL_GE = GlbDepartment.CurrentDepartment.PK;

			var keysToGetFromDB = new HashSet<CostVarianceKey>();
			keysToGetFromDB.Add(new CostVarianceKey(invoiceLine1));

			var unPostedCost = CostVarianceKey.LoadLineUnpostedCostsFromDB(keysToGetFromDB, ZGuid.Empty);
			AssertEquals(1, unPostedCost.Count);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { charge1.PK }, unPostedCost.Keys.Select(x => x.ChargePK));

			keysToGetFromDB.Add(new CostVarianceKey(invoiceLine2));
			unPostedCost = CostVarianceKey.LoadLineUnpostedCostsFromDB(keysToGetFromDB, ZGuid.Empty);
			AssertEquals(2, unPostedCost.Count);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { charge1.PK, charge2.PK }, unPostedCost.Keys.Select(x => x.ChargePK));

			keysToGetFromDB.Add(new CostVarianceKey(invoiceLine3));
			unPostedCost = CostVarianceKey.LoadLineUnpostedCostsFromDB(keysToGetFromDB, ZGuid.Empty);
			AssertEquals(2, unPostedCost.Count);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { charge1.PK, charge2.PK }, unPostedCost.Keys.Select(x => x.ChargePK));
		}

		public void TestLoadLineUnpostedCostsFromDBWithDifferentCreditor()
		{
			SetCostVarianceKeyOption(CostVarianceComparisonOption.ImportedChargeOrJobChargeCode);

			var charge = TestObjectCreator.CreateCharge(TestObjectCreator.Job1, TestObjectCreator.CC1, osCostAmt: 100M, creditor: TestObjectCreator.AALSHI);

			Factory.Save();

			var invoiceLine1 = TestObjectCreator.CreateAPInvoiceLine(null, TestObjectCreator.Job1, TestObjectCreator.CC1, TestObjectCreator.AUD, 1M, null, 100M);
			invoiceLine1.OriginalJobCharge = charge;

			var keysToGetFromDB = new HashSet<CostVarianceKey>();
			keysToGetFromDB.Add(new CostVarianceKey(invoiceLine1));

			var unPostedCost = CostVarianceKey.LoadLineUnpostedCostsFromDB(keysToGetFromDB, TestObjectCreator.AALSHI.PK);
			AssertEquals(1, unPostedCost.Count);
			AssertContainsExactElementsInAnyOrder(new List<ZGuid>() { charge.PK }, unPostedCost.Keys.Select(x => x.ChargePK));

			unPostedCost = CostVarianceKey.LoadLineUnpostedCostsFromDB(keysToGetFromDB, TestObjectCreator.ABIGAS.PK);
			AssertEquals(0, unPostedCost.Count);
		}

		void SetCostVarianceKeyOption(string option)
		{
			CostVarianceApproval valuesForTest = new CostVarianceApproval();
			valuesForTest.VarianceCalculationStyle = Enterprise.Core.Constants.CostVarianceCalculationStyle.LocalExTaxAmount;
			valuesForTest.VarianceComparisonOption = option;
			AccountingConfigurationRegistry.Instance.CostVarianceApproval.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, valuesForTest);
		}

		TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator fTestObjectCreator;
	}
}
