using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.IN.Business;

public class ExportDeclarationJobDocAddressValidation : JobDocAddressValidation
{
	public ExportDeclarationJobDocAddressValidation(AutoJobDocAddress parent) : base(parent)
	{
	}

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();

		if (Parent.E2_AddressType == AutoDocAddressTypes.Codes.Transhipper)
		{
			if (Parent.Organisation is { } orgHeader
				&& orgHeader.CustomsCodes.GetCustomsRegNo(IndiaOrgCusCodeInfo.OrgCusCodes.PAN, Core.Constants.CountryCodes.India).IsEmpty)
			{
				Parent.OrganisationPKInfo.AddWarning(Res.GetString("40000277-6878-4410-A839-3873B77BBC62", "PAN No. is missing for selected organisation."));
			}
		}
	}
}
