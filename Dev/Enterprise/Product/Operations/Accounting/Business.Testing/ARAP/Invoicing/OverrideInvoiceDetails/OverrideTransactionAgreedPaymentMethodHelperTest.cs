using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(OverrideTransactionAgreedPaymentMethodHelper))]
	public class OverrideTransactionAgreedPaymentMethodHelperTest : OverrideInvoiceDetailsHelperTest
	{
		protected override void AssertBusinessContext(InvoicingBase invoice)
		{
			Assert("Invoice is in correct context", invoice.HasContext(BusinessContext.OverrideTransactionAgreedPaymentMethod));
		}

		protected override void AssertWritableColumns(InvoicingBase invoice)
		{
			var ledger = invoice.AH_Ledger;

			bool isPaymentEnabled = Env.Security.PayablesModifyAgreedPaymentMethodForPosted.IsAllowed && ledger == LedgerTypes.AccountsPayable
							|| Env.Security.ReceivablesModifyAgreedPaymentMethodForPosted.IsAllowed && ledger == LedgerTypes.AccountsReceivable;
			AssertEquals("invoice.AH_AgreedPaymentMethodOverrideInfo.ReadOnly", !isPaymentEnabled, invoice.AH_AgreedPaymentMethodOverrideInfo.ReadOnly);

			bool isDueDateEnabled = Env.Security.PayablesModifyDueDateForPosted.IsAllowed && ledger == LedgerTypes.AccountsPayable
							|| Env.Security.ReceivablesModifyDueDateForPosted.IsAllowed && ledger == LedgerTypes.AccountsReceivable;
			AssertEquals("invoice.AH_DueDateInfo.ReadOnly", !isDueDateEnabled, invoice.AH_DueDateInfo.ReadOnly);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return InvoicePKs != null ? new OverrideTransactionAgreedPaymentMethodHelper(Factory, InvoicePKs) : new OverrideTransactionAgreedPaymentMethodHelper(Factory, Factory.NewWithValidTestData<ARInvoice>().PK);
		}
	}
}
