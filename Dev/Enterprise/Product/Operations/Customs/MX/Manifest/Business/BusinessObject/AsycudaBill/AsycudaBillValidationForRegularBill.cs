using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.MX.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override ZBool NeedsToCheckABL_GrossWeightMatchSumOfPacks => true;

		protected override ZBool NeedsToCheckABL_VolumeMatchSumOfPacks => true;

		#region Consignee

		protected override void CheckABL_ConsigneeCity()
		{
			base.CheckABL_ConsigneeCity();
			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeCityInfo);
			}
		}

		protected override void CheckABL_ConsigneeName()
		{
			base.CheckABL_ConsigneeName();
			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeNameInfo);
			}
		}

		protected override void CheckABL_ConsigneePostcode()
		{
			base.CheckABL_ConsigneePostcode();
			if (Parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Mexico && Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneePostcodeInfo);
			}
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			if (Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeRegNoInfo);
			}
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			base.CheckABL_RN_NKConsigneeCountry();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_RN_NKConsigneeCountryInfo);
		}

		protected override void CheckABL_ConsigneeState()
		{
			base.CheckABL_ConsigneeState();
			if (Parent.IsSea && Parent.ConsigneeCountry != null && SEA309Helper.MustStateCodeBeSent(Parent.ConsigneeCountry.Code))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeStateInfo);
			}
		}

		#endregion

		#region NotifyParty

		protected override void CheckABL_NotifyPartyName()
		{
			base.CheckABL_NotifyPartyName();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyNameInfo);
		}

		protected override void CheckABL_NotifyPartyCity()
		{
			base.CheckABL_NotifyPartyCity();
			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyCityInfo);
			}
		}

		protected override void CheckABL_NotifyPartyPostcode()
		{
			base.CheckABL_NotifyPartyPostcode();
			if (Parent.ABL_RN_NKNotifyPartyCountry == Core.Constants.CountryCodes.Mexico && Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyPostcodeInfo);
			}
		}

		protected override void CheckABL_NotifyPartyStreet1()
		{
			base.CheckABL_NotifyPartyStreet1();
			if (Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyStreet1Info);
			}
		}

		protected override void CheckABL_RN_NKNotifyPartyCountry()
		{
			base.CheckABL_RN_NKNotifyPartyCountry();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_RN_NKNotifyPartyCountryInfo);
		}

		protected override void CheckABL_NotifyPartyState()
		{
			base.CheckABL_NotifyPartyState();
			if (Parent.IsSea && Parent.NotifyPartyCountry != null && SEA309Helper.MustStateCodeBeSent(Parent.NotifyPartyCountry.Code))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyStateInfo);
			}
		}

		#endregion

		#region Shipper

		protected override void CheckABL_ShipperCity()
		{
			base.CheckABL_ShipperCity();
			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperCityInfo);
			}
		}

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();
			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperNameInfo);
			}
		}

		protected override void CheckABL_ShipperPostcode()
		{
			base.CheckABL_ShipperPostcode();
			if (Parent.ABL_RN_NKShipperCountry == Core.Constants.CountryCodes.Mexico && Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperPostcodeInfo);
			}
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();
			if (Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperRegNoInfo);
			}
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			base.CheckABL_RN_NKShipperCountry();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_RN_NKShipperCountryInfo);
		}

		protected override void CheckABL_ShipperState()
		{
			base.CheckABL_ShipperState();
			if (Parent.IsSea && Parent.ShipperCountry != null && SEA309Helper.MustStateCodeBeSent(Parent.ShipperCountry.Code))
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperStateInfo);
			}
		}

		#endregion

		protected override void CheckABL_GoodsDescription()
		{
			base.CheckABL_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GoodsDescriptionInfo);
		}

		protected override void CheckABL_PrepaidCollect()
		{
			base.CheckABL_PrepaidCollect();

			if (Parent.IsAir)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_PrepaidCollectInfo);
			}
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
			base.CheckABL_RL_NKFinalDestination();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKFinalDestinationInfo);
		}

		protected override void CheckABL_Volume()
		{
			base.CheckABL_Volume();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_VolumeInfo);
		}

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();

			if (Parent.Packs.Count == 0)
			{
				Parent.ABL_BillNumberInfo.AddMessageError(ResString.GetMultilingualString("FD2AD262-D919-4628-8B5B-89E905FD91B5", "At least one Pack should be inserted"));
			}
		}
	}
}
