using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration
{
	public class InvoiceLineCompleteCollection : EU.Business.Declaration.InvoiceLineCompleteCollection
	{
		public InvoiceLineCompleteCollection(JobDeclaration jobDeclaration)
			: base(jobDeclaration)
		{
		}

		public new JobComInvoiceLine this[int index] => (JobComInvoiceLine)Elements[index];

		public new JobComInvoiceLine AddNew() => (JobComInvoiceLine)base.AddNew();

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			JobComInvoiceLine newLine = (JobComInvoiceLine)child;
			var invoiceHeader = newLine.InvoiceHeader;
			if (invoiceHeader != null && invoiceHeader.ZG_ValuationMethod != ZString.Empty)
			{
				newLine.JI_ValuationCode = invoiceHeader.ZG_ValuationMethod;
			}

			newLine.ZG_CountryOfDestination = ZString.Empty;
		}
	}
}
