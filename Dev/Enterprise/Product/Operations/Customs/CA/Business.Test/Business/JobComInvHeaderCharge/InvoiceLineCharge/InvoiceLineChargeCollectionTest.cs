using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.CA.Business.Testing
{
	using NUnit.Framework;

	[TestedType(typeof(InvoiceLineCharge))]
	sealed class InvoiceLineChargeCollectionTest : Customs.Business.Testing.BaseInvoiceLineChargeCollectionTest<InvoiceLineCharge>
	{
		public override void TestHasValidCharges()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CA_RX_DeclaredCurr = ZGuid.Empty;
			JobComInvoiceHeader invoice = declaration.Invoices.AddNew();
			JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();

			AssertEquals("Has no valid charges", false, invoiceLine.Charges.HasAnElementWithValidCharges());

			InvoiceLineCharge appLineCharge = invoiceLine.Charges.AddNew();
			AssertEquals("Has no valid charges", false, invoiceLine.Charges.HasAnElementWithValidCharges());

			appLineCharge.J7_Amount = 100m;
			AssertEquals("Has no valid charges", false, invoiceLine.Charges.HasAnElementWithValidCharges());

			appLineCharge.J7_RX_NKCurrency = JobDeclaration.LocalCurrencyConstantCode;
			AssertEquals("Has valid charges", true, invoiceLine.Charges.HasAnElementWithValidCharges());
		}

		protected override JobComInvChargeCollection<InvoiceLineCharge> GetCollectionToTest()
		{
			return new JobComInvChargeCollection<InvoiceLineCharge>(InvoiceLine);
		}

		new JobComInvoiceLine InvoiceLine
		{
			get
			{
				if (fInvoiceLine == null)
				{
					fInvoiceLine = Invoice.JobComInvoiceLines.AddNew();
				}
				return fInvoiceLine;
			}
		}
		JobComInvoiceLine fInvoiceLine;

		JobComInvoiceHeader Invoice
		{
			get
			{
				if (fInvoice == null)
				{
					fInvoice = Declaration.Invoices.AddNew();
				}
				return fInvoice;
			}
		}
		JobComInvoiceHeader fInvoice;

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
