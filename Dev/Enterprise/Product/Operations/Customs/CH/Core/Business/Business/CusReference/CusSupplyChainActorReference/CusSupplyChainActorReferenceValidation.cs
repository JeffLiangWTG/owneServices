using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.CH.Business;

public class CusSupplyChainActorReferenceValidation : CommonCusReferenceValidation
{
	public CusSupplyChainActorReferenceValidation(CusSupplyChainActorReference parent) : base(parent)
	{
	}

	protected new CusSupplyChainActorReference Parent => (CusSupplyChainActorReference)base.Parent;

	PlausiValidation PlausiValidation => plausiValidation ??= PlausiValidation.New(Parent.Parent.JobDeclaration);
	PlausiValidation plausiValidation;

	protected override void CheckCFR_Code()
	{
		base.CheckCFR_Code();
		PlausiValidation.CheckNS30003_NotAllowed(Parent.CFR_CodeInfo, Parent.Parent, humanReadbleNameProvider: () => Parent.HumanReadableName);
	}

	protected override void CheckCFR_Reference()
	{
		base.CheckCFR_Reference();
		PassarValidation.CheckNP70177(Parent.CFR_ReferenceInfo, Parent.CFR_Reference);
	}

	protected override void CheckCFR_ReferenceIsNotEmpty()
	{
		base.CheckCFR_ReferenceIsNotEmpty();

		var parent = Parent;
		var targetInfo = parent.CFR_ReferenceInfo;
		if (parent.CFR_Reference.IsEmpty)
		{
			if (targetInfo.ReadOnly)
			{
				targetInfo.AddError(GetCustomsCodeMissingErrorForSelectedOwner());
			}
		}
		else
		{
			if (targetInfo.ReadOnly)
			{
				var noBID = Parent.Owner?.Header is OrgHeader ownerOrg && ownerOrg.GetCHCustomsRegNoList(OrgCusCode.SwissCodeTypes.BID).Length == 0;
				var multipleDUNs = Parent.Owner?.Header is OrgHeader ownerOrgDUN && ownerOrgDUN.GetCHCustomsRegNoList(OrgCusCode.CodeTypes.DataUniversalNumberingSystem).Length > 1;

				if (noBID && multipleDUNs)
				{
					targetInfo.AddError(GetCustomsCodeMoreThanOneErrorForSelectedOwner());
				}
			}
		}
	}

	protected string GetCustomsCodeMoreThanOneErrorForSelectedOwner() => Res.GetString("87FA2B7F-87EA-4D7B-A7CD-987D325B052B", "The selected Owner ({0}) contains more than one DUNS Registration Number", ownerOrgCode);
	protected string GetCustomsCodeMissingErrorForSelectedOwner() => Res.GetString("76536F94-1219-4D6C-A6A4-EE1721A00288", "The selected Owner ({0}) does not contain a BP-ID or UID or DUNS Registration Number.", ownerOrgCode);

	ZString ownerOrgCode => Parent.Owner?.Header?.OH_Code ?? ZString.Empty;
}
