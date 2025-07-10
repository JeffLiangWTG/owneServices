using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.BR.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override ZBool NeedsToCheckABL_GrossWeightMatchSumOfPacks => true;

		protected override ZBool NeedsToCheckABL_VolumeMatchSumOfPacks => true;

		protected override INotificationType ABL_ManifestQtyMatchSumOfPacksNotificationType => CargoWise.EntityFramework.NotificationType.MessageError;

		public override void ValidateAll()
		{
			base.ValidateAll();

			ValidateDocumentType();
			ValidateFRTMode();
			ValidateCustomsOwnNumber();
		}

		public void ValidateDocumentType()
		{
			ValidateCalculatedProperty(Parent.DocumentTypeInfo);
		}

		protected void CheckDocumentType()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DocumentTypeInfo);
		}

		protected override void CheckABL_GoodsLocation()
		{
			base.CheckABL_GoodsLocation();

			if (Parent.IsMercante)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GoodsLocationInfo);
			}
		}

		public void ValidateFRTMode()
		{
			ValidateCalculatedProperty(Parent.FRTModeInfo);
		}

		protected void CheckFRTMode()
		{
			if (Parent.IsMercante && !Parent.FRTMode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.FRTModeInfo);

				if (Parent.Header.AMA_ContainerMode == Core.Constants.ContainerModes.Containerised)
				{
					Parent.FRTModeInfo.AddMessageError(Res.GetString("9D8B7385-AF83-4487-BD4D-FB592F3CB61B", "Leave blank if Container Mode is Containerized."));
				}
				if (Parent.BL_Service)
				{
					Parent.FRTModeInfo.AddMessageError(Res.GetString("7F5346E6-7561-44EF-8644-1609CB9949B0", "Leave blank if BL Service is selected."));
				}
			}
		}

		protected override void CheckABL_RN_NKSellerCountry()
		{
			base.CheckABL_RN_NKSellerCountry();

			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RN_NKSellerCountryInfo);

			if (Parent.IsMercante)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RN_NKSellerCountryInfo);
			}
		}

		#region Consignee

		protected override void CheckABL_ConsigneeRegNoType()
		{
			base.CheckABL_ConsigneeRegNoType();

			if (Parent.IsMercante && Parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Brazil && !Parent.ConsigneeRegNoTypes().Contains(Parent.ABL_ConsigneeRegNoType))
			{
				Parent.ABL_ConsigneeRegNoTypeInfo.AddMessageError(regNoTypesValidationForMercante);
			}
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();

			if (Parent.IsMercante && Parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Brazil)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeRegNoInfo);
			}
		}

		#endregion

		#region Notify Party

		protected override void CheckABL_NotifyPartyRegNoType()
		{
			base.CheckABL_NotifyPartyRegNoType();

			if (Parent.IsMercante && Parent.ABL_RN_NKNotifyPartyCountry == Core.Constants.CountryCodes.Brazil && !Parent.NotifyPartyRegNoTypes().Contains(Parent.ABL_NotifyPartyRegNoType))
			{
				Parent.ABL_NotifyPartyRegNoTypeInfo.AddMessageError(regNoTypesValidationForMercante);
			}
		}

		protected override void CheckABL_NotifyPartyRegNo()
		{
			base.CheckABL_NotifyPartyRegNo();

			if (Parent.IsMercante && Parent.ABL_RN_NKNotifyPartyCountry == Core.Constants.CountryCodes.Brazil)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyRegNoInfo);
			}
		}

		#endregion

		readonly ZString regNoTypesValidationForMercante = Res.GetString("D5B6C249-3076-46B5-9084-93F3C2963A67", "CJN or CPF type should be selected.");

		public void ValidateCustomsOwnNumber()
		{
			ValidateCalculatedProperty(Parent.CustomsOwnNumberInfo);
		}

		protected void CheckCustomsOwnNumber()
		{
			if (Parent.IsMercante && Parent.BL_Service)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.CustomsOwnNumberInfo);
			}
		}
	}
}
