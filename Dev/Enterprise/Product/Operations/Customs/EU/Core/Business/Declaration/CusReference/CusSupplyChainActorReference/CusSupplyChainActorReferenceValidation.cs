using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Declaration;

public class CusSupplyChainActorReferenceValidation : CommonCusReferenceValidation
{
	public CusSupplyChainActorReferenceValidation(CusSupplyChainActorReference parent)
		: base(parent)
	{
	}

	protected override void CheckOwnerOrgPK()
	{
		base.CheckOwnerOrgPK();
		if (!HasAEORegNumber)
		{
			Parent.OwnerOrgPKInfo.AddMessageError(Res.GetString("6002AD1B-D597-44EA-B942-ABFD7D8BC7EA", "Organization is missing a Registration Number of type 'AEO'."));
		}
	}

	bool HasAEORegNumber => Parent.CFR_OA_Owner.IsValid && Parent.Owner?.Header is OrgHeader ownerOrg && !GetAEORegNumber(ownerOrg).IsEmpty;

	protected virtual ZString GetAEORegNumber(OrgHeader ownerOrg) => ownerOrg.CustomsCodes.GetCustomsRegNo(OrgCusCode.EuropeanUnionSharedCodeTypes.AuthorisedEconomicOperator);

	protected override void CheckCFR_Reference()
	{
		base.CheckCFR_Reference();

		var parent = Parent;
		if (parent.CFR_Reference.Length > 17)
		{
			parent.CFR_ReferenceInfo.AddMessageError(Res.GetString("C4AF1326-AB05-44AB-8D4B-8F9DCBAA977B", "The value entered must not exceed 17 characters"));
		}
	}

	protected override void CheckCFR_ReferenceIsNotEmpty()
	{
		var parent = Parent;
		var targetInfo = parent.CFR_ReferenceInfo;
		if (parent.CFR_Reference.IsEmpty)
		{
			if (!targetInfo.ReadOnly)
			{
				base.CheckCFR_ReferenceIsNotEmpty();
			}
			else
			{
				targetInfo.AddError(GetCustomsCodeMissingErrorForSelectedOwner());
			}
		}
	}

	protected virtual string GetCustomsCodeMissingErrorForSelectedOwner() => Res.GetString("D1597349-9AC4-469B-AB85-813CEF6A86FE",
					"The selected Owner ({0}) does not contain an {1} or {2} Customs Code",
					Parent.Owner?.Header?.OH_Code,
					OrgCusCode.EuropeanUnionSharedCodeTypes.Eori,
					OrgCusCode.EuropeanUnionFriendsThirdCountry.TCU);
}
