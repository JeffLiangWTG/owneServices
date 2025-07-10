using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceLineCharge
		: EU.Business.Declaration.InvoiceLineCharge
		, Integration.Customs.GB.IInvoiceLineApportionCharge
	{
		public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString CDSChargeCode => this.GetCDSChargeCode();

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		public new InvoiceLineChargeValidation Validation => (InvoiceLineChargeValidation)base.Validation;

		public new InvoiceLineChargeLookups Lookups => (InvoiceLineChargeLookups)base.Lookups;

		protected override Customs.Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineChargeValidation(this);

		protected override Customs.Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineChargeLookups(this);
	}
}
