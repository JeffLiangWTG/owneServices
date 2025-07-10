using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.AR.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected override ZBool NeedsToCheckABL_GrossWeightMatchSumOfPacks => true;

		protected override ZBool NeedsToCheckABL_VolumeMatchSumOfPacks => true;

		protected override void CheckABL_MarksAndNumbers()
		{
			base.CheckABL_MarksAndNumbers();

			if (Parent.IsSea)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_MarksAndNumbersInfo);
			}
		}

		#region  Shipper

		protected override void CheckABL_ShipperRegNoType()
		{
			base.CheckABL_ShipperRegNoType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ShipperRegNoTypeInfo);
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperRegNoInfo);
		}

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();

			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperNameInfo);
			}
		}

		protected override void CheckABL_ShipperCity()
		{
			base.CheckABL_ShipperCity();

			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ShipperCityInfo);
			}
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			base.CheckABL_RN_NKShipperCountry();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_RN_NKShipperCountryInfo);
		}

		#endregion

		#region Consignee

		protected override void CheckABL_ConsigneeRegNoType()
		{
			base.CheckABL_ConsigneeRegNoType();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ConsigneeRegNoTypeInfo);
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeRegNoInfo);
		}

		protected override void CheckABL_ConsigneeName()
		{
			base.CheckABL_ConsigneeName();

			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeNameInfo);
			}
		}

		protected override void CheckABL_ConsigneeCity()
		{
			base.CheckABL_ConsigneeCity();

			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ConsigneeCityInfo);
			}
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			base.CheckABL_RN_NKConsigneeCountry();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_RN_NKConsigneeCountryInfo);
		}

		#endregion

		#region Notify Party

		protected override void CheckABL_NotifyPartyName()
		{
			base.CheckABL_NotifyPartyName();

			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyNameInfo);
			}
		}

		protected override void CheckABL_NotifyPartyCity()
		{
			base.CheckABL_NotifyPartyCity();

			if (Parent.IsAir)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NotifyPartyCityInfo);
			}
		}

		protected override void CheckABL_RN_NKNotifyPartyCountry()
		{
			base.CheckABL_RN_NKNotifyPartyCountry();

			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_RN_NKNotifyPartyCountryInfo);
		}

		#endregion

		protected override void CheckABL_GoodsDescription()
		{
			base.CheckABL_GoodsDescription();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GoodsDescriptionInfo);
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
			base.CheckABL_RL_NKFinalDestination();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKFinalDestinationInfo);
		}

		protected override void CheckABL_PrepaidCollect()
		{
			base.CheckABL_PrepaidCollect();

			if (Parent.IsAir)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_PrepaidCollectInfo);
			}
		}

		protected override void CheckABL_Volume()
		{
			base.CheckABL_Volume();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_VolumeInfo);
		}

		protected override void CheckABL_VolumeUQ()
		{
			base.CheckABL_VolumeUQ();
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_VolumeUQInfo);
		}

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();

			if (Parent.Packs.Count == 0)
			{
				Parent.ABL_BillNumberInfo.AddMessageError(ResString.GetMultilingualString("FFC3265B-1817-4496-9717-F00AAC522C23", "At least one Pack should be inserted"));
			}
		}
	}
}
