using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public abstract class TypeSafeJobComInvoiceHeader : AutoAEJobComInvoiceHeader
{
	protected TypeSafeJobComInvoiceHeader(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobComInvoiceHeaderLookups Lookups
	{
		get { return (JobComInvoiceHeaderLookups)base.Lookups; }
	}

	public new JobComInvoiceHeaderValidation Validation
	{
		get { return (JobComInvoiceHeaderValidation)base.Validation; }
	}

	public new JobComInvoiceLineViewCollection JobComInvoiceLines
	{
		get { return (JobComInvoiceLineViewCollection)base.JobComInvoiceLines; }
	}

	public new JobDeclaration JobDeclaration
	{
		get { return (JobDeclaration)base.JobDeclaration; }
	}

	#region Implementation

	#region Overrides

	protected override Customs.Business.JobComInvoiceHeaderLookups GetNewLookups()
	{
		return new JobComInvoiceHeaderLookups(this as JobComInvoiceHeader);
	}

	protected override Customs.Business.JobComInvoiceHeaderValidation GetNewValidation()
	{
		return new JobComInvoiceHeaderValidation(this as JobComInvoiceHeader);
	}

	protected override Customs.Business.BaseJobComInvoiceLineViewCollection CreateNewJobComInvoiceLineCollection()
	{
		if (JobDeclaration != null)
		{
			return new JobComInvoiceLineViewCollection(this as JobComInvoiceHeader, JobDeclaration.InvoiceLines);
		}
		return null;
	}

	#endregion

	#endregion
}
