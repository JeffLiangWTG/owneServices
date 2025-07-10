using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.GB.Business.Declaration
{
	public class InvoiceLineApportionCharge
		: EU.Business.Declaration.InvoiceLineApportionCharge
		, Integration.Customs.GB.IInvoiceLineApportionCharge
	{
		public InvoiceLineApportionCharge(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString CDSChargeCode => this.GetCDSChargeCode();

		public new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

		public new InvoiceLineApportionChargeValidation Validation => (InvoiceLineApportionChargeValidation)base.Validation;

		public new InvoiceLineApportionChargeLookups Lookups => (InvoiceLineApportionChargeLookups)base.Lookups;

		protected override Customs.Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineApportionChargeValidation(this);

		protected override Customs.Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineApportionChargeLookups(this);
	}
}
