using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusSupplyChainActorReference : EU.Business.Declaration.CusSupplyChainActorReference
	{
		public CusSupplyChainActorReference(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ResourceStringData("d8d483a8-3262-41ab-822d-d521ac13e935", Caption = "Identification (TCUI/EORI)", MediumCaption = "Identification", ShortCaption = "ID")]
		public override ZString CFR_Reference
		{
			get => base.CFR_Reference;
			set => base.CFR_Reference = value;
		}

		public NctsHeader NctsHeader => ParentAsNctsHeader ?? ParentAsBill?.Header ?? ParentAsGoodsItem?.Header ?? ParentAsDepartureMovementHeader?.Header;

		NctsHeader ParentAsNctsHeader => Parent as NctsHeader;

		NctsBill ParentAsBill => Parent as NctsBill;

		NctsDepartureMovementHeader ParentAsDepartureMovementHeader => Parent as NctsDepartureMovementHeader;

		NctsCommonCargoDesc ParentAsGoodsItem => Parent as NctsCommonCargoDesc;

		public ICusSupplyChainActorReferenceValidationDecider ValidationDecider => CachedValueHelper.GetValue(ref validationDeciderCached, () => NctsHeader?.Configuration.CusSupplyChainActorReferenceConfiguration.GetValidationDecider());
		CachedValue<ICusSupplyChainActorReferenceValidationDecider> validationDeciderCached;

		protected override CusReferenceValidation GetNewValidation() => new CusSupplyChainActorReferenceValidation(this);
	}
}
