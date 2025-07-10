using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.BE.Business.Declaration;

public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
{
	public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
		: base(addInfoProperty)
	{
	}

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceLineLookups(this);

	[MaxLength(1)]
	public override ZString ZG_RegionOfDestination
	{
		get => base.ZG_RegionOfDestination;
		set => base.ZG_RegionOfDestination = value;
	}

	[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CountryOfDestinationList))]
	public override ZString ZG_CountryOfDestination { get => base.ZG_CountryOfDestination; set => base.ZG_CountryOfDestination = value; }

	public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;

	public JobComInvoiceLine InvoiceLine => Parent;

	protected override EUAddInfoValidation GetNewValidation()
	{
		var invoiceLine = InvoiceLine;
		return invoiceLine.IsImport
				? new ImportAddInfoJobComInvoiceLineValidation(this)
				: new AddInfoJobComInvoiceLineValidation(this);
	}
}
