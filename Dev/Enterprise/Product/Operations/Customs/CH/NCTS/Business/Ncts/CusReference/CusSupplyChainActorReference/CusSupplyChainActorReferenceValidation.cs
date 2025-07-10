using CargoWise.EntityFramework;
using Enterprise.Customs.CH.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusSupplyChainActorReferenceValidation : EU.NCTS.Business.CusSupplyChainActorReferenceValidation
{
	public CusSupplyChainActorReferenceValidation(EU.NCTS.Business.CusSupplyChainActorReference parent) : base(parent)
	{
	}

	new CusSupplyChainActorReference Parent => (CusSupplyChainActorReference)base.Parent;

	protected override void CheckOwnerOrgPK() => TypeValidation.CheckValidGuid(Parent.OwnerOrgPKInfo);

	protected override void CheckCFR_Reference()
	{
		base.CheckCFR_Reference();

		PassarValidation.CheckNP70177(Parent.CFR_ReferenceInfo, Parent.CFR_Reference);
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
				if (Parent.Owner?.Header is OrgHeader ownerOrg && ownerOrg.GetCHCustomsRegNoList(OrgCusCode.CodeTypes.DataUniversalNumberingSystem).Length > 1)
				{
					targetInfo.AddError(Res.GetString("C5358EF7-A3FA-4B25-A8F4-CCFB51FD6313", "The selected Owner ({0}) contains more than one DUNS Registration Number", Parent.Owner?.Header?.OH_Code));
				}
				else
				{
					targetInfo.AddError(GetCustomsCodeMissingErrorForSelectedOwner());
				}
			}
		}
	}

	protected override string GetCustomsCodeMissingErrorForSelectedOwner() => $"The selected Owner ({Parent.Owner?.Header?.OH_Code}) does not contain a BP-ID or UID or DUNS Registration Number.";
}
