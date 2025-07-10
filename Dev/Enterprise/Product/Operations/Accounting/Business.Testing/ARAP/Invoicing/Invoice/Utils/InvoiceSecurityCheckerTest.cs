using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.Testing
{
	[TestedType(typeof(InvoiceSecurityChecker))]
	public class InvoiceSecurityCheckerTest : TestCaseWithFactory
	{
		public void TestDependencyInjection()
		{
			AssertNotNull(InvoiceSecurityChecker);
			AssertType<InvoiceSecurityChecker>(InvoiceSecurityChecker);
		}

		public void TestGetBasicSecuritySettings_APINV()
		{
			var result = InvoiceSecurityChecker.GetBasicSecuritySettings(
				LedgerTypes.AccountsPayable, TransactionTypes.Invoice
			);
			AssertEquals("New", Env.Security.NewPayablesInvoice, result.New);
			AssertEquals("Delete", Env.Security.ReversePayablesInvoice, result.Delete);
			AssertEquals("Edit", Env.Security.ViewPayablesTransaction, result.Edit);
			AssertEquals("View", Env.Security.ViewPayablesTransaction, result.View);
		}

		public void TestGetBasicSecuritySettings_APCRD()
		{
			var result = InvoiceSecurityChecker.GetBasicSecuritySettings(
				LedgerTypes.AccountsPayable, TransactionTypes.CreditNote
			);
			AssertEquals("New", Env.Security.NewPayablesCreditNote, result.New);
			AssertEquals("Delete", Env.Security.ReversePayablesCreditNote, result.Delete);
			AssertEquals("Edit", Env.Security.ViewPayablesTransaction, result.Edit);
			AssertEquals("View", Env.Security.ViewPayablesTransaction, result.View);
		}

		IInvoiceSecurityChecker InvoiceSecurityChecker => invoiceSecurityChecker ??= ObjectFactory.Get<IInvoiceSecurityChecker>();
		IInvoiceSecurityChecker invoiceSecurityChecker;
	}
}
