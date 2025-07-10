using Enterprise.Customs.CA.Business;
using Enterprise.MasterFiles.Business;

public class ExportJobDocAddressValidation : JobDocAddressValidation
{
	public ExportJobDocAddressValidation(AutoJobDocAddress parent, JobDeclaration declaration)
		: base(parent)
	{
		this.declaration = declaration;
	}

	protected override void CheckE2_OA_Address()
	{
		base.CheckE2_OA_Address();
		if (declaration.IsG7ExportDeclaration && Parent.E2_AddressType == DocAddressTypes.Codes.SellingParty)
		{
			if (Parent.Address != null)
			{
				CAAddressValidator.ValidateMandatory(Parent, Parent.E2_OA_AddressInfo);
			}
		}
	}

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();
		switch (Parent.E2_AddressType)
		{
			case DocAddressTypes.Codes.CustomsWarehouseAddress:
			case DocAddressTypes.Codes.SellingParty:
				OrganisationValidation.ValidateNamesAndAddressesSpecial(Parent.OrganisationPKInfo, Parent.Organisation);
				break;
		}
	}

	readonly JobDeclaration declaration;
}
