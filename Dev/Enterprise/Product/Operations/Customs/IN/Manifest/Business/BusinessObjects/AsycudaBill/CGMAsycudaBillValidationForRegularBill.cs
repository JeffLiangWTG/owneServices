using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.IN.Manifest.Business;

sealed class CGMAsycudaBillValidationForRegularBill : AsycudaBillValidationForRegularBill
{
	public CGMAsycudaBillValidationForRegularBill(CGMAsycudaBill parent)
		: base(parent)
	{
	}

	new CGMAsycudaBill Parent => (CGMAsycudaBill)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		using (((ISingleElementListInternal)Parent).SuspendListChanged())
		{
			ValidateBondNumber();
		}
	}

	protected override void CheckABL_RL_NKOrigin()
	{
		if (!Parent.IsSea)
		{
			base.CheckABL_RL_NKOrigin();
		}
	}

	protected override void CheckABL_CargoStatus()
	{
		base.CheckABL_CargoStatus();
		if (Parent.IsSea)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_CargoStatusInfo);
		}
	}

	protected override void CheckABL_SpecialCargoCode()
	{
		base.CheckABL_SpecialCargoCode();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_SpecialCargoCodeInfo);
	}

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

	protected override void CheckABL_OA_Buyer()
	{
		base.CheckABL_OA_Buyer();
		var bill = Parent;
		if (bill.IsSea && bill.ABL_BuyerName.IsEmpty)
		{
			MandatoryValidation.MessageErrorIfNotEntered(bill.ABL_OA_BuyerInfo);
		}
	}

	protected override void CheckABL_GoodsDescription()
	{
		base.CheckABL_GoodsDescription();
		var goodsDescriptionInfo = Parent.ABL_GoodsDescriptionInfo;
		AsycudaBillValidationHelper.CheckGoodsDescription(goodsDescriptionInfo);
	}

	protected override void CheckABL_BillNumber()
	{
		base.CheckABL_BillNumber();
		var bill = Parent;
		var billNumber = bill.ABL_BillNumber;
		if (bill.IsSea && !billNumber.IsLettersAndNumbersOnlyOrEmpty)
		{
			bill.ABL_BillNumberInfo.AddMessageError(Res.GetString("AD888D50-7687-4C04-8739-5663EEDA4347", "You have entered invalid HBL/Bill Number."));
		}
	}

	protected override void CheckAtLeastOnePackWhereRelevant()
	{
		var parent = Parent;
		if (parent.IsSea && parent.IsContainerModeCorCP() && parent.Header.Containers.Count > 0 && parent.Packs.Cast<AsycudaPack>().All(x => x.ContainerPK.IsEmpty))
		{
			parent.ABL_BillNumberInfo.AddMessageError(Res.GetString("F3000D7C-D43E-49F4-BD79-A4E636F0F655", "You have not entered the associated Container Information under Packs."));
		}
		else
		{
			base.CheckAtLeastOnePackWhereRelevant();
		}
	}

	protected override void CheckABL_ContainerMode()
	{
		base.CheckABL_ContainerMode();
		var parent = Parent;
		if (parent.IsSea)
		{
			MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_ContainerModeInfo);
			if (parent.IsContainerModeCorCP() && parent.Header.Containers.Count == 0)
			{
				parent.ABL_ContainerModeInfo.AddMessageError(Res.GetString("98104428-1A63-4D95-A1A9-6905BCA40BFE", "You have not entered a Container Number."));
			}
		}
	}

	protected override void CheckABL_MarksAndNumbers()
	{
		base.CheckABL_MarksAndNumbers();
		var bill = Parent;
		if (bill.IsSea)
		{
			MandatoryValidation.MessageErrorIfNotEntered(bill.ABL_MarksAndNumbersInfo);
		}
	}

	protected override void CheckABL_BillStatus()
	{
		base.CheckABL_BillStatus();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_BillStatusInfo);
	}

	protected override void CheckABL_OH_BondHolder()
	{
		base.CheckABL_OH_BondHolder();
		var bill = Parent;
		if (bill.IsSea && bill.IsContainerModeCorCP())
		{
			MandatoryValidation.MessageErrorIfNotEntered(bill.ABL_OH_BondHolderInfo);
			if (bill.ABL_OH_BondHolder.IsValid && bill.MLOCode.IsEmpty)
			{
				bill.ABL_OH_BondHolderInfo.AddMessageError(Res.GetString("088BA330-BC84-4BD2-832F-6DF287DE9505", "CCC/MLO - Carrier Customs Code/Main Line Operator Code is missing for selected Bond Holder."));
			}
		}
	}

	public void ValidateBondNumber()
	{
		ValidateCalculatedProperty(Parent.BondNumberInfo);
	}

	[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Used by test via reflection")]
	void CheckBondNumber()
	{
		var bill = Parent;
		if (bill.IsSea && bill.IsContainerModeCorCP())
		{
			MandatoryValidation.MessageErrorIfNotEntered(bill.BondNumberInfo);
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

	protected override void CheckABL_InlandTransportMode()
	{
		base.CheckABL_InlandTransportMode();
		var bill = Parent;
		if (bill.IsSea && bill.IsTranshipment)
		{
			MandatoryValidation.MessageErrorIfNotEntered(bill.ABL_InlandTransportModeInfo, Res.GetString("EC7A0DC5-F928-4E9D-A4F8-CAF7E2AA1C4F", "Transshipment Mode Of Transport (M.O.T)"));
		}
	}

	protected override void CheckABL_OH_LocalTransportCarrier()
	{
		base.CheckABL_OH_LocalTransportCarrier();
		var bill = Parent;
		if (bill.IsSea && bill.IsTranshipment)
		{
			if (bill.ABL_OH_LocalTransportCarrier.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(bill.ABL_OH_LocalTransportCarrierInfo, Res.GetString("75BC4AC6-D222-4E5D-B182-866C33080AF3", "Transshipment Carrier"));
			}
			else if (bill.CarrierCode.IsEmpty)
			{
				bill.ABL_OH_LocalTransportCarrierInfo.AddMessageError(Res.GetString("AD0C1E41-875A-4A3A-8915-FB8857FB4129", "CCC - Carrier Customs Code is missing for selected Carrier."));
			}
		}
	}

	protected override void CheckABL_CustomsFinalDestinationPort()
	{
		base.CheckABL_CustomsFinalDestinationPort();
		var parent = Parent;
		if (parent.IsSea)
		{
			var targetInfo = parent.ABL_CustomsFinalDestinationPortInfo;
			if (parent.FinalDestinationIsCustomsHouse)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo, Res.GetString("23EDFE5F-28EE-400F-A99E-E64881D9D9B3", "Final Destination [Customs House]"));
				ListValidation.MessageErrorIfInvalidCode(targetInfo, ResString.GetMultilingualString("98B5801E-2651-462D-8554-8357A52947BF", "The Customs House Code you have selected is not in the list."));
			}
			else if (parent.FinalDestinationIsCFS && parent.ABL_CustomsFinalDestinationPort.Length < 10)
			{
				targetInfo.AddMessageError(Res.GetString("0677DB67-C129-4561-AF18-1726EA6B8BA3", "You have entered invalid CFS Code."));
			}
		}
	}

	protected override ZBool NeedsToShowABL_ManifestUQNotMappedMessageError => false;
}
