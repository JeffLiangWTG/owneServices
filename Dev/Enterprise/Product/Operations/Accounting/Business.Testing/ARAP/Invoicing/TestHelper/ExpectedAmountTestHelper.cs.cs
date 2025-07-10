using System;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	class ExpectedAmountTestHelper : Assertion
	{
		public ExpectedAmountTestHelper(Func<InvoicingBase> invoiceCreator)
		{
			InvoiceCreator = invoiceCreator;
		}

		Func<InvoicingBase> InvoiceCreator { get; }
		public void AssertEnableValidationOfValidateExpectedInvoiceTotal()
		{
			AssertCore(true);
			AssertCore(false);

			void AssertCore(bool defaultExpectedTotalValue)
			{
				AccountingConfigurationRegistry.Instance.DefaultExpectedTotalValue.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultExpectedTotalValue);

				var invoicingBase = InvoiceCreator();

				invoicingBase.SetIsSetFromDraftInvoice_ForTestOnly(true);
				AssertEquals("PreCondition, IsSetFromDraftInvoice is enabled by somehow.", true, invoicingBase.GetIsSetFromDraftInvoice_ForTestOnly());
				invoicingBase.EnableValidationOfValidateExpectedInvoiceTotal();
				AssertEquals("IsValidationOfValidateExpectedInvoiceTotalEnabled", false, invoicingBase.IsValidationOfValidateExpectedInvoiceTotalEnabled);
				AssertEquals("ValidateExpectedInvoiceTotal", false, invoicingBase.ValidateExpectedInvoiceTotal);

				invoicingBase.SetIsSetFromDraftInvoice_ForTestOnly(false);
				AssertEquals("PreCondition, IsSetFromDraftInvoice is disabled as default.", false, invoicingBase.GetIsSetFromDraftInvoice_ForTestOnly());
				invoicingBase.EnableValidationOfValidateExpectedInvoiceTotal();
				AssertEquals("IsValidationOfValidateExpectedInvoiceTotalEnabled", true, invoicingBase.IsValidationOfValidateExpectedInvoiceTotalEnabled);
				AssertEquals("ValidateExpectedInvoiceTotal", defaultExpectedTotalValue, invoicingBase.ValidateExpectedInvoiceTotal);
			}
		}

		public void TestExpectedInvoiceTotalAdjustmentSuspender()
		{
			var invoicingBase = InvoiceCreator();
			AssertEquals("PreCondition, ExpectedInvoiceTaxTotal", 0m, invoicingBase.ExpectedInvoiceTaxTotal);
			AssertEquals("PreCondition, ExpectedInvoiceExclTaxTotal", 0m, invoicingBase.ExpectedInvoiceExclTaxTotal);
			AssertEquals("PreCondition, ExpectedInvoiceTotal", 0m, invoicingBase.ExpectedInvoiceTotal);

			using (invoicingBase.ExpectedInvoiceTotalAdjustmentSuspender.GetSuspender())
			{
				invoicingBase.ExpectedInvoiceTaxTotal = 100m;
				invoicingBase.ExpectedInvoiceExclTaxTotal = 200m;
				invoicingBase.ExpectedInvoiceTotal = 300m;
			}
			AssertEquals("ExpectedInvoiceTaxTotal", 100m, invoicingBase.ExpectedInvoiceTaxTotal);
			AssertEquals("ExpectedInvoiceExclTaxTotal", 200m, invoicingBase.ExpectedInvoiceExclTaxTotal);
			AssertEquals("ExpectedInvoiceTotal", 300m, invoicingBase.ExpectedInvoiceTotal);
		}

		public void AssertSetExpectedOSAmountFromAccDraftInvoice()
		{
			var invoicingBase = InvoiceCreator();
			var draftInvoice = invoicingBase.Factory.NewWithValidTestData<AccDraftInvoiceHeader>();

			invoicingBase.SetIsSetFromDraftInvoice_ForTestOnly(false);
			draftInvoice.AIH_ExpectedOSTaxAmount = 100m;
			draftInvoice.AIH_ExpectedOSExTaxAmount = 101m;
			draftInvoice.AIH_ExpectedOSTotalAmount = 102m;
			AssertEquals("PreCondition, IsSetFromDraftInvoice", false, invoicingBase.GetIsSetFromDraftInvoice_ForTestOnly());
			invoicingBase.SetExpectedOSAmountFromAccDraftInvoice(draftInvoice);
			AssertEquals("IsSetFromDraftInvoice", true, invoicingBase.GetIsSetFromDraftInvoice_ForTestOnly());
			AssertEquals("ExpectedInvoiceTaxTotal", 100m, invoicingBase.ExpectedInvoiceTaxTotal);
			AssertEquals("ExpectedInvoiceExclTaxTotal", 101m, invoicingBase.ExpectedInvoiceExclTaxTotal);
			AssertEquals("ExpectedInvoiceTotal", 102m, invoicingBase.ExpectedInvoiceTotal);
			AssertEquals("IsValidationOfValidateExpectedInvoiceTotalEnabled", true, invoicingBase.IsValidationOfValidateExpectedInvoiceTotalEnabled);
			AssertEquals("ValidateExpectedInvoiceTotal", true, invoicingBase.ValidateExpectedInvoiceTotal);

			draftInvoice.AIH_ExpectedOSTaxAmount = 200m;
			draftInvoice.AIH_ExpectedOSExTaxAmount = 201m;
			draftInvoice.AIH_ExpectedOSTotalAmount = 202m;
			AssertEquals("PreCondition, IsSetFromDraftInvoice", true, invoicingBase.GetIsSetFromDraftInvoice_ForTestOnly());
			invoicingBase.SetExpectedOSAmountFromAccDraftInvoice(draftInvoice);
			AssertEquals("IsSetFromDraftInvoice", true, invoicingBase.GetIsSetFromDraftInvoice_ForTestOnly());
			AssertEquals("ExpectedInvoiceTaxTotal", 200m, invoicingBase.ExpectedInvoiceTaxTotal);
			AssertEquals("ExpectedInvoiceExclTaxTotal", 201m, invoicingBase.ExpectedInvoiceExclTaxTotal);
			AssertEquals("ExpectedInvoiceTotal", 202m, invoicingBase.ExpectedInvoiceTotal);
			AssertEquals("IsValidationOfValidateExpectedInvoiceTotalEnabled", true, invoicingBase.IsValidationOfValidateExpectedInvoiceTotalEnabled);
			AssertEquals("ValidateExpectedInvoiceTotal", true, invoicingBase.ValidateExpectedInvoiceTotal);
		}

		public void AssertUpdateExpectedAmountFromOSAmount(bool shouldAssignExpectedAmountFromOSAmount, bool shouldExpectTaxTotal, bool isInvoiceSupportingTaxAmount)
		{
			var invoicingBase = InvoiceCreator();
			if (invoicingBase.Header == null)
			{
				invoicingBase.AH_OH = invoicingBase.Factory.NewWithValidTestData<OrgHeader>().PK;
			}
			AssertNotNull("PreCondition, invoicingBase.Header", invoicingBase.Header);

			invoicingBase.Header.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable.Code;
			AssertEquals("PreCondition, IsAPTaxApplicable", false, invoicingBase.Header.CompanyData.IsAPTaxApplicable);
			AssertUpdateExpectedAmountFromOSAmountCore(invoicingBase, shouldAssignExpectedAmountFromOSAmount, false, isInvoiceSupportingTaxAmount);

			invoicingBase.Header.CompanyData.OB_APVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default.Code;
			AssertEquals("PreCondition, IsAPTaxApplicable", true, invoicingBase.Header.CompanyData.IsAPTaxApplicable);
			AssertUpdateExpectedAmountFromOSAmountCore(invoicingBase, shouldAssignExpectedAmountFromOSAmount, shouldExpectTaxTotal, isInvoiceSupportingTaxAmount);
		}

		void AssertUpdateExpectedAmountFromOSAmountCore(InvoicingBase invoicingBase, bool shouldAssignExpectedAmountFromOSAmount, bool shouldExpectTaxTotal, bool isInvoiceSupportingTaxAmount)
		{
			if (!shouldAssignExpectedAmountFromOSAmount)
			{
				AssertEquals("The shouldExpectTaxTotal need to be false when shouldAssignExpectedAmountFromOSAmount is false.", false, shouldExpectTaxTotal);
			}

			AssertCore(true);
			AssertCore(false);

			void AssertCore(bool isExpectedTotalValue)
			{
				invoicingBase.AH_OSExTaxAmount = 500m;
				invoicingBase.AH_OSTaxAmount = 350m;
				invoicingBase.SetValidateExpectedInvoiceTotal_ForTestOnly(isExpectedTotalValue);
				AssertEquals("PreCondition, IsInDatabase", false, invoicingBase.IsInDatabase);

				invoicingBase.SetIsSetFromDraftInvoice_ForTestOnly(true);
				invoicingBase.UpdateExpectedAmountFromOSAmount();
				CombineAssertions("Do not assign value when IsSetFromDraftInvoice == true.", () =>
				{
					AssertEquals("ExpectedInvoiceTaxTotal", 0m, invoicingBase.ExpectedInvoiceTaxTotal);
					AssertEquals("ExpectedInvoiceExclTaxTotal", 0m, invoicingBase.ExpectedInvoiceExclTaxTotal);
					AssertEquals("ExpectedInvoiceTotal", 0m, invoicingBase.ExpectedInvoiceTotal);
				});

				invoicingBase.SetIsSetFromDraftInvoice_ForTestOnly(false);
				invoicingBase.UpdateExpectedAmountFromOSAmount();
				if (isExpectedTotalValue && shouldAssignExpectedAmountFromOSAmount)
				{
					CombineAssertions("Assign value when IsSetFromDraftInvoice == false.", () =>
					{
						if (isInvoiceSupportingTaxAmount)
						{
							AssertEquals("ExpectedInvoiceTotal", 850m, invoicingBase.ExpectedInvoiceTotal);
						}
						else
						{
							AssertEquals("ExpectedInvoiceTotal", 500m, invoicingBase.ExpectedInvoiceTotal);
						}

						if (shouldExpectTaxTotal)
						{
							if (isInvoiceSupportingTaxAmount)
							{
								AssertEquals("ExpectedInvoiceTaxTotal", 350m, invoicingBase.ExpectedInvoiceTaxTotal);
							}
							else
							{
								AssertEquals("ExpectedInvoiceTaxTotal", 0m, invoicingBase.ExpectedInvoiceTaxTotal);
							}
							AssertEquals("ExpectedInvoiceExclTaxTotal", 500m, invoicingBase.ExpectedInvoiceExclTaxTotal);
						}
						else
						{
							AssertEquals("ExpectedInvoiceTaxTotal", 0m, invoicingBase.ExpectedInvoiceTaxTotal);
							AssertEquals("ExpectedInvoiceExclTaxTotal", 0m, invoicingBase.ExpectedInvoiceExclTaxTotal);
						}
					});
				}
				else
				{
					CombineAssertions($"Transaction Type {invoicingBase.AH_TransactionType}, should not support Expected Invoice Amount.", () =>
					{
						AssertEquals("ExpectedInvoiceTotal", 0m, invoicingBase.ExpectedInvoiceTotal);
						AssertEquals("ExpectedInvoiceTaxTotal", 0m, invoicingBase.ExpectedInvoiceTaxTotal);
						AssertEquals("ExpectedInvoiceExclTaxTotal", 0m, invoicingBase.ExpectedInvoiceExclTaxTotal);
					});
				}
			}
		}
	}
}