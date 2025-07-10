using System;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	public class OverrideInvoiceRemittanceTypeValidationTest : OverrideInvoiceDetailValidationTest
	{
		public void TestCheckAH_InvoicePaymentReferenceCode()
		{
			var configurationCollection = InvoiceRemittanceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection);

			var header = Factory.New<ARInvoice>();
			header.AH_GC = GlbCompany.CurrentCompany.PK;
			var org = TestObjectCreator.CreateOrgHeader("TST", true, true, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			header.AH_OH = org.PK;

			header.SetContext(BusinessContext.OverrideInvoiceRemittanceType);
			var testValidation = (OverrideInvoiceRemittanceTypeValidation)header.Validation;
			header.AH_InvoicePaymentReferenceCode = "";
			AssertHasErrors("Please enter a value.", header.AH_InvoicePaymentReferenceCodeInfo);

			header.AH_InvoicePaymentReferenceCode = "XXX";
			AssertHasErrors("Enter a valid selection.", header.AH_InvoicePaymentReferenceCodeInfo);

			header.AH_InvoicePaymentReferenceCode = "AAA";
			AssertNoErrors(header.AH_InvoicePaymentReferenceCodeInfo);
		}

		protected override Type InvoiceType
		{
			get
			{
				return typeof(ARInvoice);
			}
		}

		protected override OverrideInvoiceDetailValidation GetValidation(TransactionHeader parent)
		{
			return parent.Validation as OverrideInvoiceRemittanceTypeValidation;
		}

		protected override void SetBusinessContext(InvoicingBase invoice)
		{
			invoice.SetContext(BusinessContext.OverrideInvoiceRemittanceType);
		}
	}
}
