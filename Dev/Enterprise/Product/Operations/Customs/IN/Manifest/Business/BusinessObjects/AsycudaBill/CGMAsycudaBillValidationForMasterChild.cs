using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.IN.Manifest.Business;

sealed class CGMAsycudaBillValidationForMasterChild : ASYCUDA.Business.AsycudaBillValidationForMasterChild
{
	public CGMAsycudaBillValidationForMasterChild(CGMAsycudaBill parent) : base(parent)
	{
	}

	public new CGMAsycudaBill Parent => (CGMAsycudaBill)base.Parent;

	protected override void CheckABL_CustomsDischargePort()
	{
		base.CheckABL_CustomsDischargePort();
		if (Parent.IsSea)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_CustomsDischargePortInfo);
		}
	}

	protected override void CheckABL_BillIssueDate()
	{
		base.CheckABL_BillIssueDate();
		if (Parent.IsSea)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillIssueDateInfo);
		}
	}

	protected override void CheckMandatoryABL_E_ARV()
	{
	}

	protected override void CheckABL_SpecialCargoCode()
	{
		base.CheckABL_SpecialCargoCode();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_SpecialCargoCodeInfo);
	}

	protected override void CheckABL_RL_NKOrigin()
	{
		base.CheckABL_RL_NKOrigin();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKOriginInfo);
	}

	protected override void CheckABL_RL_NKFinalDestination()
	{
		base.CheckABL_RL_NKFinalDestination();
		if (Parent.IsAir)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKFinalDestinationInfo);
		}
	}

	protected override void CheckABL_RL_NKPortOfLoading()
	{
		if (!Parent.IsSea)
		{
			base.CheckABL_RL_NKPortOfLoading();
		}
	}

	protected override void CheckABL_RL_NKPortOfDischarge()
	{
		if (!Parent.IsSea)
		{
			base.CheckABL_RL_NKPortOfDischarge();
		}
	}

	protected override bool ShouldCheckHasAsycudaCountry => false;

	protected override void CheckABL_CarrierReference()
	{
		if (Parent.IsSea)
		{
			base.CheckABL_CarrierReference();
			var carrierReferenceInfo = Parent.ABL_CarrierReferenceInfo;
			MandatoryValidation.MessageErrorIfNotEntered(carrierReferenceInfo);
			ValidationHelper.CheckWithinRange((ZPropertyInfoString)carrierReferenceInfo, 1, 9999);
		}
	}

	protected override void CheckABL_GoodsDescription()
	{
		base.CheckABL_GoodsDescription();
		if (Parent.IsAir)
		{
			AsycudaBillValidationHelper.CheckGoodsDescription(Parent.ABL_GoodsDescriptionInfo);
		}
	}

	protected override void CheckABL_GrossWeightIsValidZDecimal()
	{
		TypeValidation.CheckValidDecimal(Parent.ABL_GrossWeightInfo, CGMAsycudaManifestHeader.Schema.GrossWeightPrecision, CGMAsycudaManifestHeader.Schema.GrossWeightDecimal);
	}

	protected override ZBool NeedsToCheckABL_GrossWeight => Parent.IsAir;

	protected override ZBool NeedsToCheckABL_GrossWeightUQ => Parent.IsAir;

	protected override ZBool NeedsToCheckABL_ManifestQty => Parent.IsAir;

	protected override ZBool NeedsToCheckABL_ManifestUQ => Parent.IsAir;

	protected override bool IsPortOfDischargesRequired => false;

	protected override bool IsPortOfLoadingRequired => false;
}
