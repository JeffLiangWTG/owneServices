using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing.ARAP.Invoicing
{
	public class TaxRateOverrideCalculatorTest : TestCaseWithFactory
	{
		public void TestInterCompanySumARAmount()
		{
			var isRegistryEnabled = true;
			var transactionContext = TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			var transactionRule = TaxOverrideDefaultingRule.Codes.SumARAmount;
			Factory.SetContext(BusinessContext.InterCompanyInvoiceImport);
			var calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(false, calculator.IsUseDefaultTaxLogic);
			AssertEquals(true, calculator.IsUseSumARAmount);
			AssertEquals(true, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNotNull(calculator.CachedTaxRateOverride);

			isRegistryEnabled = false;
			calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(true, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNull(calculator.CachedTaxRateOverride);
		}

		public void TestInterCompanyCopyARAmount()
		{
			var isRegistryEnabled = true;
			var transactionContext = TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			var transactionRule = TaxOverrideDefaultingRule.Codes.CopyARAmount;
			Factory.SetContext(BusinessContext.InterCompanyInvoiceImport);
			var calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(true, calculator.IsUseCopyARAmount);
			AssertEquals(false, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNotNull(calculator.CachedTaxRateOverride);

			isRegistryEnabled = false;
			calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(true, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNull(calculator.CachedTaxRateOverride);
		}

		public void TestInterCompanyWithTransactionContextINTButNotApplicable()
		{
			var isRegistryEnabled = true;
			var transactionContext = TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport;
			var transactionRule = TaxOverrideDefaultingRule.Codes.NotApplicable;
			Factory.SetContext(BusinessContext.InterCompanyInvoiceImport);
			var calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(true, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNotNull(calculator.CachedTaxRateOverride);

			isRegistryEnabled = false;
			calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(true, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNull(calculator.CachedTaxRateOverride);
		}

		public void TestInterCompanyWithTransactionContextAll()
		{
			var isRegistryEnabled = true;
			var transactionContext = TaxOverrideTransactionContext.Codes.All;
			var transactionRule = TaxOverrideDefaultingRule.Codes.NotApplicable;
			Factory.SetContext(BusinessContext.InterCompanyInvoiceImport);
			var calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(false, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(true, calculator.IsUseTaxRateOverride);
			AssertEquals(true, calculator.IsUseTransactionContextAll);
			AssertNotNull(calculator.CachedTaxRateOverride);

			isRegistryEnabled = false;
			calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(true, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNull(calculator.CachedTaxRateOverride);
		}

		public void TestInterCompanyWithTransactionContextStandard()
		{
			var isRegistryEnabled = true;
			var transactionContext = TaxOverrideTransactionContext.Codes.Standard;
			var transactionRule = TaxOverrideDefaultingRule.Codes.NotApplicable;
			Factory.SetContext(BusinessContext.InterCompanyInvoiceImport);
			var calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(true, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNotNull(calculator.CachedTaxRateOverride);

			isRegistryEnabled = false;
			calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(true, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNull(calculator.CachedTaxRateOverride);
		}

		public void TestSameCompanyStandard()
		{
			var isRegistryEnabled = true;
			var transactionContext = TaxOverrideTransactionContext.Codes.Standard;
			var transactionRule = TaxOverrideDefaultingRule.Codes.NotApplicable;
			var calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(true, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNotNull(calculator.CachedTaxRateOverride);

			isRegistryEnabled = false;
			calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(true, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(false, calculator.IsUseTransactionContextAll);
			AssertNotNull(calculator.CachedTaxRateOverride);
		}

		public void TestSameCompanyWithTransactionContextAll()
		{
			var isRegistryEnabled = true;
			var transactionContext = TaxOverrideTransactionContext.Codes.All;
			var transactionRule = TaxOverrideDefaultingRule.Codes.NotApplicable;
			var calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(false, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(true, calculator.IsUseTransactionContextAll);
			AssertNotNull(calculator.CachedTaxRateOverride);

			isRegistryEnabled = false;
			calculator = CreateCalculator(TestObjectCreator.GSTFREE1, isRegistryEnabled, transactionContext, transactionRule);

			AssertEquals(false, calculator.IsUseCopyARAmount);
			AssertEquals(false, calculator.IsUseDefaultTaxLogic);
			AssertEquals(false, calculator.IsUseSumARAmount);
			AssertEquals(false, calculator.IsUseTaxRateOverride);
			AssertEquals(true, calculator.IsUseTransactionContextAll);
			AssertNotNull(calculator.CachedTaxRateOverride);
		}

		public void TestGetIsUseTaxOverrideForInterCompanyInvoiceImport()
		{
			Factory.RemoveContext(BusinessContext.InterCompanyInvoiceImport);
			AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TaxRateOverrideCalculator.GetIsUseTaxOverrideForInterCompanyInvoiceImport(Factory));

			AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals(false, TaxRateOverrideCalculator.GetIsUseTaxOverrideForInterCompanyInvoiceImport(Factory));

			Factory.SetContext(BusinessContext.InterCompanyInvoiceImport);
			AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false);
			AssertEquals(false, TaxRateOverrideCalculator.GetIsUseTaxOverrideForInterCompanyInvoiceImport(Factory));

			AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);
			AssertEquals("Only enable when is intercompany import and registry is YES", true, TaxRateOverrideCalculator.GetIsUseTaxOverrideForInterCompanyInvoiceImport(Factory));
		}

		TaxRateOverrideCalculator CreateCalculator(AccTaxRate taxOverrideTaxRate, bool isRegistryEnabled = true, string transactionContext = TaxOverrideTransactionContext.Codes.All, string transactionRule = TaxOverrideDefaultingRule.Codes.NotApplicable)
		{
			AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.SetValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, isRegistryEnabled);

			var chargeCode = TestObjectCreator.CreateChargeCode("CC1", "Charge Code 1 for receiving company", ChargeType.Margin, 100, TestObjectCreator.GST2, TestObjectCreator.WHTFREE1);
			Factory.Save();

			var taxMessage = TestObjectCreator.CreateTaxMsg("Test", "Test", "Test", "Test");
			var taxRateOverride = TestObjectCreator.CreateTaxOverride(chargeCode, taxOverrideTaxRate.PK, taxMessage.PK, transactionContext: transactionContext, defaultingRule: transactionRule);
			Factory.Save();

			return new TaxRateOverrideCalculator(Factory, () => taxRateOverride);
		}

		protected override void SetUp()
		{
			base.SetUp();
			TestObjectCreator = new TestObjectCreator(Factory);
		}

		TestObjectCreator TestObjectCreator;
	}
}
