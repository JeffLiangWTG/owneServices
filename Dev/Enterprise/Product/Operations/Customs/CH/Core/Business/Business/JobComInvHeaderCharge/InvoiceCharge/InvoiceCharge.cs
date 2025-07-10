using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class InvoiceCharge : Customs.Business.BaseInvoiceCharge
{
	public InvoiceCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new InvoiceCharge Clone() => (InvoiceCharge)base.Clone();

	public new InvoiceChargeLookups Lookups => (InvoiceChargeLookups)base.Lookups;

	public new InvoiceChargeValidation Validation => (InvoiceChargeValidation)base.Validation;

	protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceChargeLookups(this);

	protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceChargeValidation(this);
}
