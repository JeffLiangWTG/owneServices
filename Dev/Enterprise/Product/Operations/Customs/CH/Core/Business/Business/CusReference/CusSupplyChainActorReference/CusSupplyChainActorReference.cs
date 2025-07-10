using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.Business;

public class CusSupplyChainActorReference : CommonCusReference
{
	public CusSupplyChainActorReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new CusSupplyChainActorReferenceLookups Lookups => (CusSupplyChainActorReferenceLookups)base.Lookups;

	protected override CusReferenceLookups GetNewLookups() => new CusSupplyChainActorReferenceLookups(this);

	public new CusSupplyChainActorReferenceValidation Validation => (CusSupplyChainActorReferenceValidation)base.Validation;

	protected override CusReferenceValidation GetNewValidation() => new CusSupplyChainActorReferenceValidation(this);

	public new CusEntryInstruction Parent => base.Parent as CusEntryInstruction;

	[ResourceStringData("CH.CusSupplyChainActorReference|CFR_Reference", Caption = "Identification (BP-ID/UID/DUNS)", MediumCaption = "Identification", ShortCaption = "ID")]
	public override ZString CFR_Reference { get => base.CFR_Reference; set => base.CFR_Reference = value; }

	public override ZGuid OwnerOrgPK
	{
		get => base.OwnerOrgPK;
		set
		{
			var oldValue = OwnerOrgPK;
			base.OwnerOrgPK = value;

			if (!IsCopying && oldValue != OwnerOrgPK)
			{
				AutopopulateReference();
			}
		}
	}

	protected ZBool OwnerOrgPK_ReadOnly => !CFR_Reference.IsEmpty && CFR_OA_Owner.IsEmpty;

	void AutopopulateReference()
	{
		CFR_Reference = Owner?.Header?.GetIdentificationNumberForCH() ?? ZString.Empty;
	}

	protected override ZString HumanReadableNameCore => Res.GetString("DF8A5796-CBFF-4F3F-9794-6B6E80779EA8", "Supply Chain Actor Reference");
}
