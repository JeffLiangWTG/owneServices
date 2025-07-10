using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideInvoiceReferenceHelper))]
	public class OverrideInvoiceReferenceHelperTest : OverrideInvoiceDetailsHelperTest
	{
		protected override void AssertBusinessContext(InvoicingBase invoice)
		{
			Assert("Invoice is in correct context", invoice.HasContext(BusinessContext.OverrideInvoiceReference));
		}

		protected override void AssertWritableColumns(InvoicingBase invoice)
		{
			var security = new Security.SecurityCore(null, GlbStaff.GetCurrentUser(Factory), Env.CurrentBranchPK, GlbDepartment.CurrentDepartment.PK.ToGuid(), Env.CurrentCompanyPK);

			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				invoice = GetNewInvoiceBaseOnSecurity(security, true, true);
				AssertEquals("invoice.InvoiceRemittanceReferenceInfo.ReadOnly", false, invoice.InvoiceRemittanceReferenceInfo.ReadOnly);
				AssertEquals("invoice.AH_TransactionNumInfo.ReadOnly", false, invoice.AH_TransactionNumInfo.ReadOnly);
				AssertEquals("invoice.AH_InvoiceDateInfo.ReadOnly", false, invoice.AH_InvoiceDateInfo.ReadOnly);
				AssertEquals("invoice.AH_ChequeOrReferenceInfo.ReadOnly", false, invoice.AH_ChequeOrReferenceInfo.ReadOnly);
			}

			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				invoice = GetNewInvoiceBaseOnSecurity(security, false, true);
				AssertEquals("invoice.InvoiceRemittanceReferenceInfo.ReadOnly", true, invoice.InvoiceRemittanceReferenceInfo.ReadOnly);
				AssertEquals("invoice.AH_TransactionNumInfo.ReadOnly", false, invoice.AH_TransactionNumInfo.ReadOnly);
				AssertEquals("invoice.AH_InvoiceDateInfo.ReadOnly", false, invoice.AH_InvoiceDateInfo.ReadOnly);
				AssertEquals("invoice.AH_ChequeOrReferenceInfo.ReadOnly", false, invoice.AH_ChequeOrReferenceInfo.ReadOnly);
			}

			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				invoice = GetNewInvoiceBaseOnSecurity(security, true, false);
				AssertEquals("invoice.InvoiceRemittanceReferenceInfo.ReadOnly", false, invoice.InvoiceRemittanceReferenceInfo.ReadOnly);
				AssertEquals("invoice.AH_TransactionNumInfo.ReadOnly", true, invoice.AH_TransactionNumInfo.ReadOnly);
				AssertEquals("invoice.AH_InvoiceDateInfo.ReadOnly", true, invoice.AH_InvoiceDateInfo.ReadOnly);
				AssertEquals("invoice.AH_ChequeOrReferenceInfo.ReadOnly", true, invoice.AH_ChequeOrReferenceInfo.ReadOnly);
			}

			using (Env.SetTemporarySecurityInstanceForTest(security))
			{
				invoice = GetNewInvoiceBaseOnSecurity(security, false, false);
				AssertEquals("invoice.InvoiceRemittanceReferenceInfo.ReadOnly", true, invoice.InvoiceRemittanceReferenceInfo.ReadOnly);
				AssertEquals("invoice.AH_TransactionNumInfo.ReadOnly", true, invoice.AH_TransactionNumInfo.ReadOnly);
				AssertEquals("invoice.AH_InvoiceDateInfo.ReadOnly", true, invoice.AH_InvoiceDateInfo.ReadOnly);
				AssertEquals("invoice.AH_ChequeOrReferenceInfo.ReadOnly", true, invoice.AH_ChequeOrReferenceInfo.ReadOnly);
			}
		}

		protected InvoicingBase GetNewInvoiceBaseOnSecurity(Security.SecurityCore security, bool invoiceRemittanceReferenceSecurity, bool invoiceDateNumOrSupplierCostRefSecurity)
		{
			InvoicingBase invoice = Factory.NewWithValidTestData<APInvoice>();
			InvoicePKs = new ZGuid[] { invoice.PK };
			security.PayablesModifyInvoiceRemittanceReference.IsAllowed = invoiceRemittanceReferenceSecurity;
			security.PayablesModifyInvoiceDateNumOrSupplierCostRef.IsAllowed = invoiceDateNumOrSupplierCostRefSecurity;
			OverrideInvoiceDetailsHelper testObject = (OverrideInvoiceDetailsHelper)GetNewBusinessObject();
			AssertEquals("InvoiceCollection.Count", 1, testObject.WrappedObjects.Count);
			return invoice;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoicePKs != null ? new OverrideInvoiceReferenceHelper(Factory, InvoicePKs) : new OverrideInvoiceReferenceHelper(Factory, Factory.NewWithValidTestData<APInvoice>().PK);
		}
	}
}
