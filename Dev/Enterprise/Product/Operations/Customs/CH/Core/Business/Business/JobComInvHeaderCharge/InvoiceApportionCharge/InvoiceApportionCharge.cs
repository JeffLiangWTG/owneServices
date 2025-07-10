using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class InvoiceApportionCharge : Customs.Business.BaseApportionedCharge
{
	public InvoiceApportionCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new InvoiceApportionCharge Clone() => (InvoiceApportionCharge)base.Clone();

	public new InvoiceApportionChargeValidation Validation => (InvoiceApportionChargeValidation)base.Validation;

	public new InvoiceApportionChargeLookups Lookups => (InvoiceApportionChargeLookups)base.Lookups;

	protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new InvoiceApportionChargeValidation(this);

	protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new InvoiceApportionChargeLookups(this);
}
