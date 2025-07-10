using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.CL.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent) : base(parent)
		{
		}
		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		#region Goods Location

		protected override void CheckABL_OA_GoodsLocation()
		{
			base.CheckABL_OA_GoodsLocation();
			var parent = Parent;

			var header = parent.Header;
			if (header != null && header.AMA_Nature == ShipmentTypeList.Codes.Import23)
			{
				if (parent.ABL_OA_GoodsLocation.IsEmpty)
				{
					parent.ABL_OA_GoodsLocationInfo.AddMessageError(ResString.GetMultilingualString("6F2B93A6-B01D-4A01-B6DF-7D8B6FA7FC2D", "A Warehouse is required"));
				}
				else
				{
					var goodsLocation = parent.GoodsLocation?.Header;
					if (goodsLocation != null)
					{
						var regNumberRUT = goodsLocation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(header.AMA_RN_NKCountry, new ZString[] { ChileOrgCusCodeInfo.OrgCusCodes.RUT });

						if (header.IsSea)
						{
							var regNumberCCP = goodsLocation.CustomsCodes.GetCustomsRegNoMatchingCountryAndCodes(header.AMA_RN_NKCountry, new ZString[] { OrgCusCode.CodeTypes.ControlledPremisesID });

							if (regNumberRUT.IsEmpty || regNumberCCP.IsEmpty)
							{
								parent.ABL_OA_GoodsLocationInfo.AddMessageError(ResString.GetMultilingualString("111687C4-D25B-40BB-B285-F0970F116199", "The Warehouse should have RUT and CCP assigned numbers."));
							}
						}
						else if (header.IsAir)
						{
							if (regNumberRUT.IsEmpty)
							{
								parent.ABL_OA_GoodsLocationInfo.AddMessageError(ResString.GetMultilingualString("CECE8F4C-C1DD-451C-A85B-A32921779BE4", "The Warehouse should have RUT assigned number."));
							}
						}
					}
				}
			}
		}

		#endregion

		#region Shipper

		protected override void CheckABL_ShipperRegNoType()
		{
			base.CheckABL_ShipperRegNoType();
			var parent = Parent;

			if (parent.ABL_RN_NKShipperCountry == Core.Constants.CountryCodes.Chile)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(parent.ABL_ShipperRegNoTypeInfo);
			}
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();
			var parent = Parent;

			if (parent.ABL_RN_NKShipperCountry == Core.Constants.CountryCodes.Chile || parent.ShipperRegNoTypes().Contains(parent.ABL_ShipperRegNoType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_ShipperRegNoInfo);
			}
		}

		#endregion

		#region Consignee

		protected override void CheckABL_ConsigneeRegNoType()
		{
			base.CheckABL_ConsigneeRegNoType();
			var parent = Parent;

			if (parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Chile)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_ConsigneeRegNoTypeInfo);
			}
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			var parent = Parent;

			if (parent.ABL_RN_NKConsigneeCountry == Core.Constants.CountryCodes.Chile || parent.ConsigneeRegNoTypes().Contains(parent.ABL_ConsigneeRegNoType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_ConsigneeRegNoInfo);
			}
		}

		#endregion

		#region Notify Party

		protected override void CheckABL_NotifyPartyRegNoType()
		{
			base.CheckABL_NotifyPartyRegNoType();
			var parent = Parent;

			if (parent.ABL_RN_NKNotifyPartyCountry == Core.Constants.CountryCodes.Chile)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_NotifyPartyRegNoTypeInfo);
			}
		}

		protected override void CheckABL_NotifyPartyRegNo()
		{
			base.CheckABL_NotifyPartyRegNo();
			var parent = Parent;

			if (parent.ABL_RN_NKNotifyPartyCountry == Core.Constants.CountryCodes.Chile || parent.NotifyPartyRegNoTypes().Contains(parent.ABL_NotifyPartyRegNoType))
			{
				MandatoryValidation.MessageErrorIfNotEntered(parent.ABL_NotifyPartyRegNoInfo);
			}
		}

		#endregion

		protected override INotificationType NotificationTypeForDuplicateBillNumber => CargoWise.EntityFramework.NotificationType.Error;
	}
}
