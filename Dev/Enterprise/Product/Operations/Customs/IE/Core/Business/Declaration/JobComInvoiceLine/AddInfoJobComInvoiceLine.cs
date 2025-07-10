using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
	{
		public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
		{
		}

		public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;
		public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;

		public JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent;

		protected override bool IsLookupsCachedInBase => false;

		protected override EUAddInfoLookups GetNewLookups()
		{
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsExport)
			{
				return new ExportAddInfoJobComInvoiceLineLookups(this);
			}
			else
			{
				return new AddInfoJobComInvoiceLineLookups(this);
			}
		}

		protected override EUAddInfoValidation GetNewValidation()
		{
			var invoiceLine = InvoiceLine;
			if (invoiceLine.IsExport)
			{
				return new ExportAddInfoJobComInvoiceLineValidation(this);
			}
			else if (invoiceLine.IsImport)
			{
				return new ImportAddInfoJobComInvoiceLineValidation(this);
			}
			else
			{
				return new AddInfoJobComInvoiceLineValidation(this);
			}
		}
	}
}
