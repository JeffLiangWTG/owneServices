using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CH.NCTS.Business;

public class CusSupplyChainActorReference : EU.NCTS.Business.CusSupplyChainActorReference
{
	public CusSupplyChainActorReference(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override CusReferenceValidation GetNewValidation() => new CusSupplyChainActorReferenceValidation(this);

	[ResourceStringData("CH.NCTS.CusSupplyChainActorReference|CFR_Reference", Caption = "Identification (BP-ID/UID/DUNS)", MediumCaption = "Identification", ShortCaption = "ID")]
	public override ZString CFR_Reference { get => base.CFR_Reference; set => base.CFR_Reference = value; }

	protected override ZString DataGroupingCode => Core.Constants.CountryCodes.Switzerland;
}
