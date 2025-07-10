using System;
using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public sealed class KoreaSouthAmendStatusCodeValidationProviderTest : TestCaseWithFactory
	{
		public void TestValidateAmendStatusCodeForInvoice()
		{
			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			var auBranch = TestObjectCreator.CreateBranchWithCompany("AU");
			Factory.Save();

			ValidateAmendStatusCodeForInvoiceCore(auBranch, isRegistryOn: true, AssertNoErrorAndWarning, AssertNoErrorAndWarning, AssertNoErrorAndWarning);
			ValidateAmendStatusCodeForInvoiceCore(auBranch, isRegistryOn: false, AssertNoErrorAndWarning, AssertNoErrorAndWarning, AssertNoErrorAndWarning);
			ValidateAmendStatusCodeForInvoiceCore(krBranch, isRegistryOn: false, AssertNoErrorAndWarning, AssertInvalidAmendCode, AssertNoErrorAndWarning);
			ValidateAmendStatusCodeForInvoiceCore(krBranch, isRegistryOn: true, AssertEmptyAmendCode, AssertInvalidAmendCode, AssertAmendCodeNotSuitableForAmendmentInvoice);
		}

		void ValidateAmendStatusCodeForInvoiceCore(GlbBranch branch,
			bool isRegistryOn,
			Action<InvoicingBase> assertEmptyAmendCode,
			Action<InvoicingBase> assertInvalidAmendCode,
			Action<InvoicingBase> assertAmendCodeNotSuitable)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isRegistryOn);

				var originalARInvoice = Factory.NewWithValidTestData<ARInvoice>();
				IAmending original = originalARInvoice;
				var invoicingBase = original.GenerateAmendingTransaction(originalARInvoice.AH_TransactionType) as InvoicingBase;
				var line = invoicingBase.Lines.AddNew();

				var taxRate = TestObjectCreator.CreateTaxRate(code: "EXT", description: "desc", rateNum: 1, type: AccTaxRate.Types.Exempt);
				line.AL_AT = taxRate.PK;

				var invoiceBaseValidation = invoicingBase.Validation as InvoiceBaseValidation;

				invoiceBaseValidation.ValidateAH_Calc_AmendStatusCode();
				assertEmptyAmendCode(invoicingBase);

				invoicingBase.AH_Calc_AmendStatusCode = "22";
				assertInvalidAmendCode(invoicingBase);

				invoicingBase.AH_Calc_AmendStatusCode = "04";
				assertAmendCodeNotSuitable(invoicingBase);

				invoicingBase.AH_Calc_AmendStatusCode = "01";
				AssertNoErrorAndWarning(invoicingBase);
			}
		}

		public void TestValidateAmendStatusCodeForInvoiceReversal()
		{
			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			var auBranch = TestObjectCreator.CreateBranchWithCompany("AU");
			Factory.Save();

			ValidateAmendStatusCodeForInvoiceReversalCore(auBranch, isRegistryOn: true, AssertNoErrorAndWarning, AssertNoErrorAndWarning, AssertNoErrorAndWarning);
			ValidateAmendStatusCodeForInvoiceReversalCore(auBranch, isRegistryOn: false, AssertNoErrorAndWarning, AssertNoErrorAndWarning, AssertNoErrorAndWarning);
			ValidateAmendStatusCodeForInvoiceReversalCore(krBranch, isRegistryOn: false, AssertNoErrorAndWarning, AssertInvalidAmendCode, AssertNoErrorAndWarning);
			ValidateAmendStatusCodeForInvoiceReversalCore(krBranch, isRegistryOn: true, AssertEmptyAmendCode, AssertInvalidAmendCode, AssertAmendCodeNotSuitableForInvoiceReversal);
		}

		void ValidateAmendStatusCodeForInvoiceReversalCore(GlbBranch branch,
			bool isRegistryOn,
			Action<InvoicingBase> assertEmptyAmendCode,
			Action<InvoicingBase> assertInvalidAmendCode,
			Action<InvoicingBase> assertAmendCodeNotSuitable)
		{
			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isRegistryOn);

				var originalAR = Factory.NewWithValidTestData<ARInvoice>();
				var invoicingBase = TestObjectCreator.ReverseTransaction(originalAR, out _) as InvoicingBase;
				var line = invoicingBase.Lines.AddNew();

				var taxRate = TestObjectCreator.CreateTaxRate(code: "EXT", description: "desc", rateNum: 1, type: AccTaxRate.Types.Exempt);
				line.AL_AT = taxRate.PK;

				var invoicingBaseReversalValidation = invoicingBase.Validation as InvoicingBaseReversalValidation;

				invoicingBaseReversalValidation.ValidateAH_Calc_AmendStatusCode();
				assertEmptyAmendCode(invoicingBase);

				invoicingBase.AH_Calc_AmendStatusCode = "22";
				assertInvalidAmendCode(invoicingBase);

				invoicingBase.AH_Calc_AmendStatusCode = "01";
				assertAmendCodeNotSuitable(invoicingBase);

				invoicingBase.AH_Calc_AmendStatusCode = "04";
				AssertNoErrorAndWarning(invoicingBase);
			}
		}

		public void TestValidateAmendStatusCodeExcludingTaxRateType()
		{
			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			Factory.Save();

			var exlTaxRate = new List<(string code, string taxType)> { ("EXL", AccTaxRate.Types.ExcludedFromTheTaxBase) };
			var notTaxRate = new List<(string code, string taxType)> { ("NOT", AccTaxRate.Types.NotReportable) };
			var extTaxRate = new List<(string code, string taxType)> { ("EXT", AccTaxRate.Types.Exempt) };
			var multipleTaxRates = new List<(string code, string taxType)>
			{
				("EXL", AccTaxRate.Types.ExcludedFromTheTaxBase),
				("NOT", AccTaxRate.Types.NotReportable),
				("EXT", AccTaxRate.Types.Exempt)
			};

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, krBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				ValidateAmendStatusCodeExcludingTaxRateTypeCore(exlTaxRate, AssertNoErrorAndWarning);
				ValidateAmendStatusCodeExcludingTaxRateTypeCore(notTaxRate, AssertNoErrorAndWarning);
				ValidateAmendStatusCodeExcludingTaxRateTypeCore(extTaxRate, AssertEmptyAmendCode);
				ValidateAmendStatusCodeExcludingTaxRateTypeCore(multipleTaxRates, AssertEmptyAmendCode);
			}
		}

		void ValidateAmendStatusCodeExcludingTaxRateTypeCore(List<(string code, string taxType)> taxRateInfos, Action<InvoicingBase> assertAmendCode)
		{
			var aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
			var invoicingBase = ((IAmending)aRInvoice).GenerateAmendingTransaction(aRInvoice.AH_TransactionType) as InvoicingBase;
			var line = invoicingBase.Lines.AddNew();

			foreach (var taxRateInfo in taxRateInfos)
			{
				var taxRate = TestObjectCreator.CreateTaxRate(taxRateInfo.code, "desc", 1, type: taxRateInfo.taxType);
				line.AL_AT = taxRate.PK;
			}

			var invoiceBaseValidation = invoicingBase.Validation as InvoiceBaseValidation;
			invoiceBaseValidation.ValidateAH_Calc_AmendStatusCode();

			assertAmendCode(invoicingBase);
		}

		public void TestAllowedAmendmentStatusCodesWhenAmendWithInvoice()
		{
			var statusCodesFromConstants = EInvoicingKoreaSouthConstants.AllowedAmendmentStatusCodesWhenAmendWithInvoice;
			var statusCodesForTest = new string[] { "01", "02", "05" };

			AssertContainsExactElementsInAnyOrder(statusCodesForTest, statusCodesFromConstants);
		}

		public void TestSuggestedAmendmentStatusCodesWhenReverseInvoice()
		{
			var statusCodesFromConstants = EInvoicingKoreaSouthConstants.SuggestedAmendmentStatusCodesWhenReverseInvoice;
			var statusCodesForTest = new string[] { "03", "04", "06" };

			AssertContainsExactElementsInAnyOrder(statusCodesForTest, statusCodesFromConstants);
		}

		public void TestShouldNotValidateAmendStatusCodeWhenTaxRateIsNull()
		{
			var krBranch = TestObjectCreator.CreateBranchWithCompany("KR");
			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, krBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true);

				var aRInvoice = Factory.NewWithValidTestData<ARInvoice>();
				var invoicingBase = ((IAmending)aRInvoice).GenerateAmendingTransaction(aRInvoice.AH_TransactionType) as InvoicingBase;
				var line1 = invoicingBase.Lines.AddNew();
				var line2 = invoicingBase.Lines.AddNew();

				AssertNull(line1.TaxRate);
				AssertNull(line2.TaxRate);

				var invoiceBaseValidation = invoicingBase.Validation as InvoiceBaseValidation;
				invoiceBaseValidation.ValidateAH_Calc_AmendStatusCode();

				AssertNoErrorAndWarning(invoicingBase);
			}
		}

		void AssertNoErrorAndWarning(InvoicingBase invoicingBase)
		{
			AssertNoErrors(invoicingBase.AH_Calc_AmendStatusCodeInfo);
			AssertNoWarnings(invoicingBase.AH_Calc_AmendStatusCodeInfo);
		}

		void AssertEmptyAmendCode(InvoicingBase invoicingBase) => AssertHasError(invoicingBase.AH_Calc_AmendStatusCodeInfo, "An amendment status code is required for the amending transaction. Please select an amendment status code.");

		void AssertInvalidAmendCode(InvoicingBase invoicingBase) => AssertHasError(invoicingBase.AH_Calc_AmendStatusCodeInfo, "Enter a valid Amend Status Code.");

		void AssertAmendCodeNotSuitableForAmendmentInvoice(InvoicingBase invoicingBase) => AssertHasError(invoicingBase.AH_Calc_AmendStatusCodeInfo, "For amendment invoice, only amend status code 01, 02 or 05 can be used.");

		void AssertAmendCodeNotSuitableForInvoiceReversal(InvoicingBase invoicingBase) => AssertHasWarning(invoicingBase.AH_Calc_AmendStatusCodeInfo, "For invoice reversal, amend status code should be 03, 04 or 06.");

		TestObjectCreator TestObjectCreator => testObjectCreator ??= new TestObjectCreator(Factory);

		TestObjectCreator testObjectCreator;
	}
}
