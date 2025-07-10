using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NL.Business.Declaration;

class DeclarationJobDocAddressValidation : JobDocAddressValidation
{
	readonly JobDeclaration declaration;

	public DeclarationJobDocAddressValidation(JobDocAddress addressToValidate, JobDeclaration declaration) : base(addressToValidate)
	{
		this.declaration = declaration;
	}

	OrgHeader OrgHeader => Parent.Organisation;

	protected override void CheckOrganisationPK()
	{
		base.CheckOrganisationPK();
		var info = Parent.OrganisationPKInfo;
		var addressType = Parent.E2_AddressType;

		if (addressType == DocAddressTypes.Codes.Carrier && declaration.IsExport)
		{
			if (declaration.ZG_TypeOfSecurity == ExportSecurityTypeList.Codes.EXS)
			{
				CheckOrganisationPK_CarrierEUBorderEoriDetailsAndTcuNumber(info);
			}
			if (declaration.ZG_TypeOfSecurity != ExportSecurityTypeList.Codes.NotUsed)
			{
				CheckOrganisationPK_CarrierEUBorderUC9011(info);
			}
		}
	}

	void CheckOrganisationPK_CarrierEUBorderEoriDetailsAndTcuNumber(ZPropertyInfo info)
	{
		if (OrgHeader == null)
		{
			info.AddMessageError(Res.GetString("868752B3-D7D7-499F-AF73-C5E73D871493", "[13 12 000 000] The carrier cannot be empty when it is a security declaration (EXS) ."));
		}
		else
		{
			var missingEoriNumber = EU.Business.Extensions.GetCustomsRegNoIgnoringCountry(OrgHeader, OrgCusCode.EuropeanUnionSharedCodeTypes.Eori).IsEmpty;
			var missingTCUReference = EU.Business.Extensions.GetCustomsRegNoIgnoringCountry(OrgHeader, OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU).IsEmpty;

			if (missingEoriNumber && missingTCUReference)
			{
				info.AddMessageError(Res.GetString("98022A04-70F6-480A-A300-6306DC7D3BAC", "EORI or TCU reference is required for Carrier EU Border."));
			}
		}
	}

	void CheckOrganisationPK_CarrierEUBorderUC9011(ZPropertyInfo info)
	{
		if (OrgHeader == null)
		{
			info.AddMessageError(Res.GetString("61475C26-1472-422A-B4D2-177F248CEEE0", "[C9011] If security is not 0 THEN carrier is required as organization"));
		}
	}
}
