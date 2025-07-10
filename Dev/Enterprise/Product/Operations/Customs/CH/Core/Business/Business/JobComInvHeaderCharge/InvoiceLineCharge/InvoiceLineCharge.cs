using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class InvoiceLineCharge : Customs.Business.BaseInvoiceLineCharge
{
	public InvoiceLineCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new InvoiceLineCharge Clone() => (InvoiceLineCharge)base.Clone();

	public new InvoiceLineChargeValidation Validation => (InvoiceLineChargeValidation)base.Validation;

	public new InvoiceLineChargeLookups Lookups => (InvoiceLineChargeLookups)base.Lookups;

	protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceLineChargeValidation(this);

	protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceLineChargeLookups(this);
}
