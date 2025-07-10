using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Business;

public class OrgCusCodeValidation : MasterFiles.Business.OrgCusCodeValidation, Integration.Customs.AE.IOrgCusCodeValidation
{
	public OrgCusCodeValidation(AutoOrgCusCode parent) : base(parent)
	{
	}

	protected override void CheckOK_CustomsRegNo()
	{
		base.CheckOK_CustomsRegNo();

		switch (Parent.OK_CodeType.ToUpperInvariant())
		{
			case OrgCusCode.UnitedArabEmiratesCodeTypes.AEO when !Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{7}$"):
				Parent.OK_CustomsRegNoInfo.AddWarning(Res.GetString("52143ABB-AE27-466A-B0FD-A93C8D6D9791", "AE AEO number should consist of 7 digits."));
				break;
			case OrgCusCode.UnitedArabEmiratesCodeTypes.CBLSNumber when !Regex.IsMatch(Parent.OK_CustomsRegNo, "^[a-zA-Z0-9]{1,7}$"):
				Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("F3875DE8-4B76-47BC-861B-B490A7067AD5", "CBLS Number can only be a maximum of 7 characters."));
				break;
			case OrgCusCode.UnitedArabEmiratesCodeTypes.MPCINumber when !Regex.IsMatch(Parent.OK_CustomsRegNo, "^[a-zA-Z0-9]{1,7}$"):
				Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("3D2D761C-99CC-4A79-8ED6-8790531FBB6C", "MPCI Party Id can only be a maximum of 7 characters."));
				break;
			case OrgCusCode.CodeTypes.CarrierCode:
				if (Parent.OK_CustomsRegNo.Length != 3)
				{
					Parent.OK_CustomsRegNoInfo.AddError(Res.GetString("0A21F77F-DAF2-41A8-B3A9-BFCBA1203365", "CCC – Customs Carrier Code must be the Carrier's 3-character SMDG code."));
				}
				break;
			case OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber when !Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{15}$"):
				Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("A3219384-1026-4254-B435-53E95356F5A8", "ID Number must be 15 digits."));
				break;
			case OrgCusCode.CodeTypes.TaxFileCode when !Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{15}$"):
				Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("23A23707-EEA9-4A94-8B20-8A907AD78B1C", "GTX Number must be 15 digits."));
				break;
			case OrgCusCode.CodeTypes.CorporationCode when !Regex.IsMatch(Parent.OK_CustomsRegNo, "^[0-9]{15}$"):
				Parent.OK_CustomsRegNoInfo.AddMessageError(Res.GetString("03C4E98E-405D-4A4B-9F29-0AE94E44142F", "GCR Number must be 15 digits."));
				break;
		}
	}

	protected override void CheckOK_CodeType()
	{
		base.CheckOK_CodeType();
		CheckMutualExclusivePartyIdentifier();
	}

	void CheckMutualExclusivePartyIdentifier()
	{
		if (mutualExclusivePartyIdentifiers.Contains(Parent.OK_CodeType))
		{
			var orgHeader = Parent.Header;
			if (orgHeader != null
				&& orgHeader.CustomsCodes.Cast<OrgCusCode>().Any(x => x.OK_RN_NKCodeCountry == Constants.CountryCodes.UnitedArabEmirates
															&& x.OK_CodeType != Parent.OK_CodeType
															&& mutualExclusivePartyIdentifiers.Contains(x.OK_CodeType)))
			{
				Parent.OK_CodeTypeInfo.AddError(Res.GetString("B93AA47A-4808-4196-9E80-6BB778558765", "Only one of the following Types are allowed: 'PAS – Passport', 'IDO - ID Number', 'GTX – Government Tax File Code' or 'GCR – Government Corporation Code'."));
			}
		}
	}

	readonly ZString[] mutualExclusivePartyIdentifiers =
	[
		OrgCusCode.CodeTypes.PassportID,
		OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber,
		OrgCusCode.CodeTypes.TaxFileCode,
		OrgCusCode.CodeTypes.CorporationCode
	];
}
