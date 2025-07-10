using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.IE.Business.Declaration
{
	public sealed class InvoiceLineCharge : EU.Business.Declaration.InvoiceLineCharge, Integration.Customs.IE.IInvoiceLineCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => InvoiceLine.IsImport ? new ImportInvoiceLineChargeLookups(this) : base.GetNewLookups();

		protected override bool ShouldResetDefaultIsIncludedInAmount(ZString incoTerm, ICustomsChargeCode charge) => true;
	}
}
