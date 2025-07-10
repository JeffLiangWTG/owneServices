using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceCharge
		: EU.Business.Declaration.InvoiceCharge
		, Integration.Customs.GB.IInvoiceCharge
	{
		public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		public ZString CDSChargeCode => this.GetCDSChargeCode();

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		public new InvoiceChargeLookups Lookups => (InvoiceChargeLookups)base.Lookups;

		public new InvoiceChargeValidation Validation => (InvoiceChargeValidation)base.Validation;

		protected override Customs.Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceChargeLookups(this);

		protected override Customs.Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceChargeValidation(this);
	}
}
