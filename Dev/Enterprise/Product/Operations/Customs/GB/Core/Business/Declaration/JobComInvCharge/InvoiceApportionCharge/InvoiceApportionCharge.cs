using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceApportionCharge
		: EU.Business.Declaration.InvoiceApportionCharge
		, Integration.Customs.GB.IInvoiceApportionCharge
	{
		public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString CDSChargeCode => this.GetCDSChargeCode();

		public new JobComInvoiceHeader Invoice => (JobComInvoiceHeader)base.Invoice;

		public new InvoiceApportionChargeValidation Validation => (InvoiceApportionChargeValidation)base.Validation;

		public new InvoiceApportionChargeLookups Lookups => (InvoiceApportionChargeLookups)base.Lookups;

		protected override JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceApportionChargeLookups(this);

		protected override JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceApportionChargeValidation(this);
	}
}
