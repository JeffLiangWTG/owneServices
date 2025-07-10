using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.CH.Business;

public class GroupInvoiceCharge : Customs.Business.BaseGroupInvoiceCharge
{
	public GroupInvoiceCharge(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new GroupInvoiceCharge Clone() => (GroupInvoiceCharge)base.Clone();

	public new GroupInvoiceChargeLookups Lookups => (GroupInvoiceChargeLookups)base.Lookups;

	public new GroupInvoiceChargeValidation Validation => (GroupInvoiceChargeValidation)base.Validation;

	protected override Common.JobComInvHeaderChargeLookups GetNewLookups() => new GroupInvoiceChargeLookups(this);

	protected override Common.JobComInvHeaderChargeValidation GetNewValidation() => new GroupInvoiceChargeValidation(this);
}
