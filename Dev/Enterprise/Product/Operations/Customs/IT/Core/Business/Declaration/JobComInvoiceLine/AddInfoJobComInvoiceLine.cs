using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class AddInfoJobComInvoiceLine : EU.Business.Declaration.AddInfoJobComInvoiceLine
{
	public AddInfoJobComInvoiceLine(ZPropertyInfo addInfoProperty)
		: base(addInfoProperty)
	{
	}

	[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.PortTaxRateList))]
	public override ZString ZG_PortTaxRate
	{
		get => base.ZG_PortTaxRate;
		set => base.ZG_PortTaxRate = value;
	}

	[List(nameof(Lookups) + "." + nameof(AddInfoJobComInvoiceLineLookups.SteelTypeList))]
	public override ZString ZG_SteelType
	{
		get => base.ZG_SteelType;
		set => base.ZG_SteelType = value;
	}

	[ResourceStringData("2DEAE314-5236-4371-A533-82A23AD3BA76", Caption = "Country of Destination")]
	public override ZString ZG_CountryOfDestination { get => base.ZG_CountryOfDestination; set => base.ZG_CountryOfDestination = value; }

	public JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)Parent;

	public new AddInfoJobComInvoiceLineValidation Validation => (AddInfoJobComInvoiceLineValidation)base.Validation;

	protected override EUAddInfoValidation GetNewValidation() => new AddInfoJobComInvoiceLineValidation(this);

	public new AddInfoJobComInvoiceLineLookups Lookups => (AddInfoJobComInvoiceLineLookups)base.Lookups;

	protected override EUAddInfoLookups GetNewLookups() => new AddInfoJobComInvoiceLineLookups(this);
}
