using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
{
	public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
	{
	}

	new AsycudaBill Parent => (AsycudaBill)base.Parent;

	public override void ValidateAll()
	{
		base.ValidateAll();
		ValidateNegotiable();
		ValidatePayer();
		ValidateABL_SplitBillNumber();
	}

	protected override void CheckABL_SpecialCargoCode()
	{
		base.CheckABL_SpecialCargoCode();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_SpecialCargoCodeInfo);
	}

	protected override void CheckABL_OA_Shipper()
	{
		base.CheckABL_OA_Shipper();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_OA_ShipperInfo);
		ValidationHelper.CheckOrgContactInfo(Parent.Shipper, Parent.ABL_OA_ShipperInfo);
	}

	protected override void CheckABL_CargoType()
	{
		base.CheckABL_CargoType();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_CargoTypeInfo);
	}

	protected override void CheckABL_FreightValue()
	{
		base.CheckABL_FreightValue();
		MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_FreightValueInfo);
	}

	protected override void CheckABL_OA_Consignee()
	{
		base.CheckABL_OA_Consignee();
		ValidationHelper.CheckOrgContactInfo(Parent.Consignee, Parent.ABL_OA_ConsigneeInfo);

		if (Parent.Consignee != null && Parent.ABL_RL_NKFinalDestination.Substring(0, 2) == Constants.CountryCodes.UnitedArabEmirates)
		{
			var hasConsigneePartyIdentifiers = Parent.Consignee.Header?.CustomsCodes.Cast<OrgCusCode>()
				.Any(x => x.OK_RN_NKCodeCountry == Constants.CountryCodes.UnitedArabEmirates && consigneePartyIdentifiers.Contains(x.OK_CodeType)) ?? false;
			if (!hasConsigneePartyIdentifiers)
			{
				Parent.ABL_OA_ConsigneeInfo.AddMessageError(Res.GetString("37ec06e4-39f0-4de6-baa8-33a1ae772a2c", "Consignee Requires a 'Government Tax File Code (GTX)' or 'Id Number (IDO)' or 'Passport Number (PAS)' or 'Government Corporation Code (GCR)' to be set in the Organization > Details > Config > Registration Numbers / Codes."));
			}
		}
	}

	readonly ZString[] consigneePartyIdentifiers =
	[
		OrgCusCode.CodeTypes.PassportID,
		OrgCusCode.UnitedArabEmiratesCodeTypes.IDNumber,
		OrgCusCode.CodeTypes.TaxFileCode,
		OrgCusCode.CodeTypes.CorporationCode
	];

	protected override void CheckABL_OA_NotifyParty()
	{
		base.CheckABL_OA_NotifyParty();
		ValidationHelper.CheckOrgContactInfo(Parent.NotifyParty, Parent.ABL_OA_NotifyPartyInfo);
	}

	protected override void CheckABL_OA_ContainerAgent()
	{
		base.CheckABL_OA_ContainerAgent();
		var parent = Parent;

		ValidationHelper.CheckOrgContactInfo(Parent.ContainerAgent, parent.ABL_OA_ContainerAgentInfo);
		MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_OA_ContainerAgentInfo);
	}

	protected override void CheckABL_OA_DeliveryAgent()
	{
		base.CheckABL_OA_DeliveryAgent();
		ValidationHelper.CheckOrgContactInfo(Parent.DeliveryAgent, Parent.ABL_OA_DeliveryAgentInfo);
	}

	#region Validate Negotiable

	public void ValidateNegotiable()
	{
		ValidateCalculatedProperty(Parent.NegotiableInfo);
	}

	internal void CheckNegotiable()
	{
		if(Parent.Negotiable.IsEmpty)
		{
			Parent.NegotiableInfo.AddMessageError(Res.GetString("7dbf8185-72c3-448a-bd56-b35aed46885c", "Please indicate whether the bill is Negotiable or Non-Negotiable"));
		}
		ListValidation.MessageErrorIfInvalidCode(Parent.NegotiableInfo);
	}

	#endregion

	#region Validate Payer

	public void ValidatePayer()
	{
		ValidateCalculatedProperty(Parent.PayerInfo);
	}

	internal void CheckPayer()
	{
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(Parent.PayerInfo,
			Parent.NegotiableInfo, (ZString)NegotiableList.Codes.Yes,
			Res.GetString("0bbe7525-633a-49ef-9bf0-bfff4dbb622e", "Payer is required for Non-Negotiable bills"));
	}

	#endregion

	public void ValidateABL_SplitBillNumber()
	{
		ValidateCalculatedProperty(Parent.ABL_SplitBillNumberInfo);
	}

	internal void CheckABL_SplitBillNumber()
	{
		MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(Parent.ABL_SplitBillNumberInfo,
			Parent.ABL_SplitBillInfo, ZBool.True,
			Res.GetString("CE876F00-F104-42BA-BCB3-F2801F7C05F6", "Split Bill Number must be populated for a Split Bill of Lading."));
	}

	protected override void CheckABL_OA_Forwarder()
	{
		base.CheckABL_OA_Forwarder();
		if (Parent.ABL_BolType == Core.Constants.ShipmentTypes.CoLoadMaster)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_OA_ForwarderInfo);
			ValidationHelper.CheckOrgContactInfo(Parent.Forwarder, Parent.ABL_OA_ForwarderInfo);
			ValidationHelper.CheckAnyMPCICode(Parent.Forwarder, Parent.ABL_OA_ForwarderInfo);
		}
	}
}
