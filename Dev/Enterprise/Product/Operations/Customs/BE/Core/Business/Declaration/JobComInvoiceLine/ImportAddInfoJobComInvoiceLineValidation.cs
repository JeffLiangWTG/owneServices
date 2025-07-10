using CargoWise.EntityFramework;

namespace Enterprise.Customs.BE.Business.Declaration;

public class ImportAddInfoJobComInvoiceLineValidation : AddInfoJobComInvoiceLineValidation
{
	public ImportAddInfoJobComInvoiceLineValidation(AddInfoJobComInvoiceLine parent) : base(parent)
	{
	}

	protected override void CheckZG_CountryOfDestination()
	{
		base.CheckZG_CountryOfDestination();
		ListValidation.MessageErrorIfInvalidCode(Parent.ZG_CountryOfDestinationInfo);
	}

	protected override void CheckZG_RegionOfDestination()
	{
		base.CheckZG_RegionOfDestination();
		ListValidation.MessageErrorIfInvalidCode(Parent.ZG_RegionOfDestinationInfo);
	}

	protected override void CheckZG_CountryOfSupply()
	{
		base.CheckZG_CountryOfSupply();

		if (Parent.ZG_CountryOfSupply.IsEmpty && IsPreferenceUsed() && (Parent.Parent.EntryInstruction?.IsImportDeclarationType() ?? false))
		{
			Parent.ZG_CountryOfSupplyInfo.AddMessageError(Res.GetString("94A16A2E-83B4-4C8F-8610-588D5760BE25", "Country of Origin (=Country of Supply) is required when Preference is {0} for Declaration Type {1}", Parent.Parent.JI_PrimaryPreference, Parent.Parent.EntryInstruction.CEI_Style));
		}

		bool IsPreferenceUsed()
		{
			var firstCharOfPrimaryReference = Parent.Parent.JI_PrimaryPreference.SubstringSafe(0, 1);
			return firstCharOfPrimaryReference == "2"
					|| firstCharOfPrimaryReference == "3"
					|| firstCharOfPrimaryReference == "4";
		}
	}
}
