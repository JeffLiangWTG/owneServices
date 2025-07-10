using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class InvoiceApportionCharge : EU.Business.Declaration.InvoiceApportionCharge, Integration.Customs.IE.IInvoiceApportionCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => IsImport ? new ImportInvoiceApportionChargeValidation(this) : base.GetNewValidation();

		protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => IsImport ? new ImportInvoiceApportionChargeLookups(this) : base.GetNewLookups();
		bool IsImport => Invoice?.IsImport ?? false;
	}
}
