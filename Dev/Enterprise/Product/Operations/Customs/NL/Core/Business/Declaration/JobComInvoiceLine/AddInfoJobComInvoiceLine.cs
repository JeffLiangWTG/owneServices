using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
{
	public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty) : base(addInfoProperty)
	{
	}

	public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;

	public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceLineLookups(this);

	protected override EUAddInfoValidation GetNewValidation() => new AddInfoJobComInvoiceLineValidation(this);

	public new JobComInvoiceLine Parent => (JobComInvoiceLine)base.Parent;

	[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.CountryOfDestinationList))]
	public override ZString ZG_CountryOfDestination { get => base.ZG_CountryOfDestination; set => base.ZG_CountryOfDestination = value; }
}
