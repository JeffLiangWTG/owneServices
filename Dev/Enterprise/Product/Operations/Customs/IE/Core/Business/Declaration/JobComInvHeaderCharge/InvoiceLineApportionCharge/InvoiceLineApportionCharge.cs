using System.Data;
using CargoWise.EntityFramework;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class InvoiceLineApportionCharge : EU.Business.Declaration.InvoiceLineApportionCharge, Integration.Customs.IE.IInvoiceLineApportionCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => (InvoiceLine?.IsImport ?? false) ? new ImportInvoiceLineApportionChargeLookups(this) : base.GetNewLookups();
	}
}
