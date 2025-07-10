using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.EU.Business.Declaration.Testing
{
	[TestedType(typeof(InvoiceLineChargeCollection<InvoiceLineCharge>))]
	class InvoiceLineChargeCollectionTest : SubsetBusinessObjectCollectionTestCase<InvoiceLineChargeCollection<InvoiceLineCharge>, InvoiceLineCharge>
	{
		public void TestDistributeByForExports()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_MessageType = MessageTypeList.Codes.Export;
			var invLine = dec.Invoices.AddNew().InvoiceLines.AddNew();
			var charge = invLine.Charges.AddNew();
			AssertEquals(ChargeDistributeByList.Codes.Weight, charge.J7_DistributeBy);
		}

		protected override InvoiceLineChargeCollection<InvoiceLineCharge> GetCollectionToTest()
		{
			return new InvoiceLineChargeCollection<InvoiceLineCharge>(InvoiceLine);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(InvoiceLineCharge));
		}

		JobComInvoiceLine InvoiceLine => fInvoiceLine ?? (fInvoiceLine = Invoice.JobComInvoiceLines.AddNew());
		JobComInvoiceLine fInvoiceLine;

		JobComInvoiceHeader Invoice => fInvoice ?? (fInvoice = Declaration.Invoices.AddNew());
		JobComInvoiceHeader fInvoice;

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = MessageTypeList.Codes.Import;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
