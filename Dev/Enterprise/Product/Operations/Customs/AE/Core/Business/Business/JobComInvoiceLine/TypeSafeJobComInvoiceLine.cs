using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public abstract class TypeSafeJobComInvoiceLine : AutoAEJobComInvoiceLine
{
	protected TypeSafeJobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobComInvoiceLineLookups Lookups
	{
		get { return base.Lookups as JobComInvoiceLineLookups; }
	}

	protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
	{
		return new JobComInvoiceLineValidation(this as JobComInvoiceLine);
	}
}
