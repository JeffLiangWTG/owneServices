using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.JP.Common;

namespace Enterprise.Customs.JP.Manifest.Business
{
	public class AsycudaBillValidationForRegularBill(ASYCUDA.Business.AsycudaBill parent) : ASYCUDA.Business.AsycudaBillValidationForRegularBill(parent)
	{
		AsycudaManifestHeader Header => Parent.Header;

		public new AsycudaBill Parent => (AsycudaBill)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateCustomsWeight();
			ValidateCustomsNetWeight();
			ValidateCustomsVolume();
			ValidateABL_Tariff();
			ValidateABL_CountryOfOrigin();
			ValidateFinalDestinationIATACode();
		}

		public void ValidateABL_CountryOfOrigin()
		{
			ValidateCalculatedProperty(Parent.ABL_CountryOfOriginInfo);
		}

		public void ValidateCustomsWeight()
		{
			ValidateCalculatedProperty(Parent.CustomsWeightInfo);
		}

		public void ValidateCustomsNetWeight()
		{
			ValidateCalculatedProperty(Parent.CustomsNetWeightInfo);
		}

		public void ValidateCustomsVolume()
		{
			ValidateCalculatedProperty(Parent.CustomsVolumeInfo);
		}

		public void ValidateABL_Tariff()
		{
			ValidateCalculatedProperty(Parent.ABL_TariffInfo);
		}

		public void ValidateFinalDestinationIATACode()
		{
			ValidateCalculatedProperty(Parent.FinalDestinationIATACodeInfo);
		}

		protected override ZBool NeedsToCheckABL_Consignee => !Parent.IsHDF && !Parent.IsHCH;

		protected override ZBool NeedsToCheckABL_ShipperPostcode => !Parent.IsHDF;

		protected override ZBool NeedsToShowABL_ManifestUQNotEnteredMessageError => !Parent.IsHCH;

		protected override ZBool NeedsToCheckABL_ManifestQty => !Parent.IsHCH || !Parent.ABL_ManifestUQ.IsEmpty;

		protected override ZBool NeedsToCheckABL_RL_NKFinalDestination => !Parent.IsHCH;

		protected override ZBool NeedsToCheckABL_GrossWeight => !Parent.ABL_GrossWeightUQ.IsEmpty;

		protected override ZBool NeedsToCheckABL_GrossWeightUQ => !Parent.ABL_GrossWeight.IsEmpty;

		protected override bool IsMandatoryForNetWeightUQ => false;

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();
			var parent = Parent;
			var billNumber = parent.ABL_BillNumber;
			var targetInfo = parent.ABL_BillNumberInfo;

			if (parent.IsNVC)
			{
				if (billNumber.Length < 5)
				{
					targetInfo.AddMessageError(Res.GetString("EE03295E-CCED-4C7D-A72E-975D9DBE0F40", "[NVC01] House B/L must have five or more digits."));
				}
				if (billNumber.Contains(','))
				{
					targetInfo.AddMessageError(Res.GetString("E7645902-CA3E-457D-8933-F58B025E9D3E", "[NVC01] House B/L cannot contain the comma (\",\") symbol."));
				}
			}
			else if (parent.IsHCH || parent.IsHDF)
			{
				if (billNumber.Length > 16)
				{
					targetInfo.AddWarning(Res.GetString("3A2026CF-D977-4EBB-A21C-F34794BEAC34",
						"The entered {0} number exceeds the maximum allowable length set by the customs. Only the first 16 characters will be sent to the customs.",
						targetInfo.HumanReadableName));
				}
			}
		}

		protected void CheckABL_CountryOfOrigin()
		{
			if (Parent.IsNVC)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_CountryOfOriginInfo);
			}
		}

		protected override void CheckABL_OA_Consignee()
		{
			if (NeedsToCheckABL_Consignee)
			{
				base.CheckABL_OA_Consignee();
			}
		}

		protected override void CheckABL_RL_NKOrigin()
		{
		}

		protected override void CheckABL_GrossWeight()
		{
			base.CheckABL_GrossWeight();

			if (Parent.ABL_GrossWeightUQ.Equals(Core.Constants.Weight.Pounds) && Parent.ABL_GrossWeight.ToStringTrimZeros().Length > 8)
			{
				Parent.ABL_GrossWeightInfo.AddMessageError(Res.GetString("16F86ED6-D063-4584-A63F-68112EFBCCDA", "The 'Weight' entered surpasses the maximum allowed by customs. Only 8 characters, including the decimal point, can be transmitted."));
			}
		}

		protected void CheckCustomsWeight() => CheckCustomsWeightOrVolumeMaxValue(Parent.CustomsWeightInfo);

		protected void CheckCustomsNetWeight() => CheckCustomsWeightOrVolumeMaxValue(Parent.CustomsNetWeightInfo);

		protected void CheckCustomsVolume() => CheckCustomsWeightOrVolumeMaxValue(Parent.CustomsVolumeInfo);

		void CheckCustomsWeightOrVolumeMaxValue(ZPropertyInfo info)
		{
			if ((ZDecimal)info.Value  >= 1000000m)
			{
				info.AddMessageError(Res.GetString("28262717-BA74-4C45-8C70-0D58A7D1E984", "Value exceeds the upper limit."));
			}
		}

		protected void CheckABL_Tariff()
		{
			var parent = Parent;
			if (parent.IsNVC)
			{
				var targetInfo = parent.ABL_TariffInfo;
				var tariffLength = parent.TariffFormatter.Format(parent.ABL_Tariff).Length;

				if (tariffLength > 0)
				{
					if (tariffLength < 4)
					{
						targetInfo.AddMessageError(Res.GetString("4BDED0F0-1FE5-4C0B-AD06-B66A7324E4B2", "Statistical/HS Code must be at least 4 characters long."));
					}
					else if (parent.UniversalTariff == null)
					{
						parent.ABL_TariffInfo.AddMessageError(ListValidation.InvalidCodeMessage.ToString());
					}
				}
			}
		}

		protected override void CheckABL_ShipperStreet1()
		{
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ShipperStreet1Info);
			if (!Parent.IsHDF)
			{
				base.CheckABL_ShipperStreet1();
				if (Parent.IsHCH)
				{
					ValidateHCH01AddressLengthLimit(Parent.ShipperAddress, Parent.ABL_ShipperStreet1Info);
				}
			}
		}

		protected override void CheckABL_GoodsLocation()
		{
			if (!Parent.IsHDF)
			{
				base.CheckABL_GoodsLocation();
				var parent = Parent;
				var targetInfo = parent.ABL_GoodsLocationInfo;
				ListValidation.MessageErrorIfInvalidCode(targetInfo);
				if (parent.IsNVC && parent.IsTemporaryLandingTransportation)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}
			}
		}

		protected override void CheckABL_GoodsDescription()
		{
			base.CheckABL_GoodsDescription();
			Helper.CheckABL_GoodsDescription();
		}

		protected override void CheckABL_RL_NKPortOfDischarge()
		{
			if (Parent.IsNVC)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RL_NKPortOfDischargeInfo);
			}
		}

		protected override void CheckABL_ConsigneeStreet1()
		{
			base.CheckABL_ConsigneeStreet1();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ConsigneeStreet1Info);
			ValidateHCH01AddressLengthLimit(Parent.ConsigneeAddress, Parent.ABL_ConsigneeStreet1Info);
		}

		protected override void CheckABL_ShipperState()
		{
			base.CheckABL_ShipperState();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ShipperStateInfo);
		}

		protected override void CheckABL_ConsigneeState()
		{
			base.CheckABL_ConsigneeState();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ConsigneeStateInfo);
		}

		protected override void CheckABL_NotifyPartyState()
		{
			base.CheckABL_NotifyPartyState();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_NotifyPartyStateInfo);
		}

		protected override void CheckABL_ManifestQty()
		{
			base.CheckABL_ManifestQty();
			Helper.CheckABL_ManifestQty();
		}

		protected override void CheckABL_ManifestQtyMatchSumOfPacks()
		{ }

		protected override void CheckABL_ManifestUQ()
		{
			base.CheckABL_ManifestUQ();
			if (Parent.IsHCH && Parent.ABL_ManifestQty > 0)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_ManifestUQInfo);
			}
		}

		protected override void CheckABL_NetWeight()
		{
			var parnet = Parent;
			if (parnet.IsNVC)
			{
				Helper.CheckABL_NetWeight();
				if (!parnet.ABL_NetWeightUQ.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(parnet.ABL_NetWeightInfo);
				}
			}
		}

		protected override void CheckABL_ShipperStreet2()
		{
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ShipperStreet2Info);
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ShipperStreet2();
			}
		}

		protected override void CheckABL_ShipperCity()
		{
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ShipperCityInfo);
			if (!Parent.IsHDF)
			{
				base.CheckABL_ShipperCity();
				Helper.CheckABL_ShipperCity();
			}
		}

		protected override void CheckABL_ShipperPostcode()
		{
			base.CheckABL_ShipperPostcode();
			if (NeedsToCheckABL_ShipperPostcode)
			{
				Helper.CheckABL_ShipperPostcode();
			}
		}

		protected override void CheckABL_ShipperPhone()
		{
			if (!Parent.IsHDF)
			{
				Helper.CheckABL_ShipperPhone();
			}
		}
		protected override void CheckABL_ConsigneeStreet2()
		{
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ConsigneeStreet2Info);
			if (NeedsToCheckABL_Consignee)
			{
				Helper.CheckABL_ConsigneeStreet2();
			}
		}
		protected override void CheckABL_ConsigneeCity()
		{
			base.CheckABL_ConsigneeCity();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ConsigneeCityInfo);
			if (NeedsToCheckABL_Consignee)
			{
				Helper.CheckABL_ConsigneeCity();
			}
		}
		protected override void CheckABL_ConsigneePostcode()
		{
			base.CheckABL_ConsigneePostcode();
			if (NeedsToCheckABL_Consignee)
			{
				Helper.CheckABL_ConsigneePostcode();
			}
		}
		protected override void CheckABL_ConsigneePhone()
		{
			if (NeedsToCheckABL_Consignee)
			{
				base.CheckABL_ConsigneePhone();
				Helper.CheckABL_ConsigneePhone();
			}
		}

		protected override void CheckABL_NotifyPartyStreet2()
		{
			base.CheckABL_NotifyPartyStreet2();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_NotifyPartyStreet2Info);
			if (Parent.IsNVC)
			{
				Helper.CheckABL_NotifyPartyStreet2();
			}
		}

		protected override void CheckABL_NotifyPartyCity()
		{
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_NotifyPartyCityInfo);
			if (Parent.IsNVC)
			{
				base.CheckABL_NotifyPartyCity();
				Helper.CheckABL_NotifyPartyCity();
			}
		}

		protected override void CheckABL_NotifyPartyPostcode()
		{
			if (Parent.IsNVC)
			{
				base.CheckABL_NotifyPartyPostcode();
				Helper.CheckABL_NotifyPartyPostcode();
			}
		}

		protected override void CheckABL_NotifyPartyPhone()
		{
			if (Parent.IsNVC)
			{
				base.CheckABL_NotifyPartyPhone();
				Helper.CheckABL_NotifyPartyPhone();
			}
		}

		void ValidateHCH01AddressLengthLimit(string address, ZPropertyInfo targetInfo)
		{
			var parent = Parent;
			var shouldApplyHCH01AddressLengthLimit = Core.Constants.CountryCodes.Japan.Equals(Header?.AMA_RN_NKCountry) && parent.IsAir && parent.IsImport;
			if (shouldApplyHCH01AddressLengthLimit && address.Length > 105)
			{
				var truncatedAddress = address.Substring(0, 105);
				targetInfo.AddWarning(
						Res.GetString("65234BD5-9717-4BF2-B3F6-925231766AD0",
										"Only the initial 105 bytes will be sent to customs as the address. The designated value sent to customs is \"{0}\". If this isn't desired, please clear the 'Party' field to override the values.", truncatedAddress));
			}
		}

		protected override void CheckABL_ShipperName()
		{
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ShipperNameInfo);
			if (!Parent.IsHDF)
			{
				CheckNameLength(Parent.ABL_ShipperNameInfo);
			}
		}

		protected override void CheckABL_ConsigneeName()
		{
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_ConsigneeNameInfo);
			if (NeedsToCheckABL_Consignee)
			{
				CheckNameLength(Parent.ABL_ConsigneeNameInfo);
			}
		}

		protected override void CheckABL_NotifyPartyName()
		{
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_NotifyPartyNameInfo);
			if (Parent.IsNVC)
			{
				CheckNameLength(Parent.ABL_NotifyPartyNameInfo);
			}
		}

		protected override void CheckABL_NotifyPartyStreet1()
		{
			base.CheckABL_NotifyPartyStreet1();
			EnglishCharactersValidation.MessageErrorIfNotWesternEuropean(Parent.ABL_NotifyPartyStreet1Info);
		}

		void CheckNameLength(ZPropertyInfo infoToBeChecked)
		{
			Helper.ValidateByteLength(infoToBeChecked, 70);
		}

		protected override void CheckABL_ConsigneeRegNoType()
		{
			base.CheckABL_ConsigneeRegNoType();
			var parent = Parent;
			ValidatePartyRegNoType(parent.ABL_ConsigneeRegNoType, parent.ABL_ConsigneeRegNo, parent.ABL_ConsigneeRegNoTypeInfo, parent.ShouldShowConsigneeRegNoType,
				CustomsRegNumTypeValidation.GetListByImportAndPartyType(parent.IsImport, isShipper: false, isConsignee: true));
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			var parent = Parent;
			ValidatePartyRegNo(parent.ABL_ConsigneeRegNo, parent.ABL_ConsigneeRegNoType, parent.ABL_ConsigneeRegNoInfo, parent.ShouldShowConsigneeRegNoType,
				CustomsRegNumTypeValidation.GetListByImportAndPartyType(parent.IsImport, isShipper: false, isConsignee: true));
		}

		protected override void CheckABL_ShipperRegNoType()
		{
			base.CheckABL_ShipperRegNoType();
			var parent = Parent;
			ValidatePartyRegNoType(parent.ABL_ShipperRegNoType, parent.ABL_ShipperRegNo, parent.ABL_ShipperRegNoTypeInfo, parent.ShouldShowShipperRegNoType, CustomsRegNumTypeValidation.NvcShipperList);
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();
			var parent = Parent;
			ValidatePartyRegNo(parent.ABL_ShipperRegNo, parent.ABL_ShipperRegNoType, parent.ABL_ShipperRegNoInfo, parent.ShouldShowShipperRegNoType, CustomsRegNumTypeValidation.NvcShipperList);
		}

		protected override void CheckABL_NotifyPartyRegNoType()
		{
			base.CheckABL_NotifyPartyRegNoType();
			var parent = Parent;
			ValidatePartyRegNoType(parent.ABL_NotifyPartyRegNoType, parent.ABL_NotifyPartyRegNo, parent.ABL_NotifyPartyRegNoTypeInfo, parent.ShouldShowNotifyPartyRegNoType,
				CustomsRegNumTypeValidation.GetListByImportAndPartyType(parent.IsImport, false, false));
		}

		protected override void CheckABL_NotifyPartyRegNo()
		{
			base.CheckABL_NotifyPartyRegNo();
			var parent = Parent;
			ValidatePartyRegNo(parent.ABL_NotifyPartyRegNo, parent.ABL_NotifyPartyRegNoType, parent.ABL_NotifyPartyRegNoInfo, parent.ShouldShowNotifyPartyRegNoType,
				CustomsRegNumTypeValidation.GetListByImportAndPartyType(parent.IsImport, isShipper: false, isConsignee: false));
		}

		protected void ValidatePartyRegNo(ZString regNo, ZString regType, ZPropertyInfo targetInfo, ZBool shouldShowParty, IEnumerable<ZString> regNumTypeCodeList)
		{
			if (shouldShowParty)
			{
				if (regNo.IsEmpty && !regType.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
				}

				if (!regNo.IsEmpty && regNumTypeCodeList.Contains(regType))
				{
					CustomsRegistrationNumberValidation.ValidateCustomsCode(NotificationType.MessageError, regType, regNo, targetInfo);
				}
			}
		}

		protected void ValidatePartyRegNoType(ZString regNoType, ZString regNo, ZPropertyInfo targetInfo, ZBool shouldShowParty, IEnumerable<ZString> regNumTypeCodeList)
		{
			if (!regNumTypeCodeList.Contains(regNoType) && !regNoType.IsEmpty)
			{
				CustomsRegNumTypeValidation.ValidateRegNumTypeIsValid(NotificationType.MessageError, regNoType, regNumTypeCodeList, targetInfo);
			}

			if (shouldShowParty && !regNo.IsEmpty && regNoType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(targetInfo);
			}
		}

		protected override void CheckABL_NetWeightUQ()
		{
			if (Parent.IsNVC)
			{
				base.CheckABL_NetWeightUQ();
				if (Parent.ABL_NetWeight > 0)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_NetWeightUQInfo);
				}
			}
		}

		protected override void CheckABL_ShipmentType()
		{
		}

		protected override void CheckABL_MarksAndNumbers()
		{
			if (Parent.IsNVC)
			{
				base.CheckABL_MarksAndNumbers();
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_MarksAndNumbersInfo);
			}
		}

		protected override void CheckABL_OA_Shipper()
		{
			if (!Parent.IsHDF)
			{
				base.CheckABL_OA_Shipper();
			}
		}

		protected override void CheckABL_OA_NotifyParty()
		{
			if (Parent.IsNVC)
			{
				base.CheckABL_OA_NotifyParty();
			}
		}

		protected void CheckFinalDestinationIATACode()
		{
			if (Parent.IsAir)
			{
				ValidationHelper.CheckIATACode(Parent.Factory, Parent.FinalDestinationIATACodeInfo, Parent.ABL_RL_NKFinalDestinationInfo.HumanReadableName);
			}
		}

		protected override void CheckABL_RX_NKFreightValueCurrency()
		{
			if (Parent.IsNVC)
			{
				base.CheckABL_RX_NKFreightValueCurrency();
			}
		}

		protected override void CheckABL_RX_NKTransportValueCurrency()
		{
			if (Parent.IsNVC)
			{
				base.CheckABL_RX_NKTransportValueCurrency();
			}
		}

		protected override void CheckABL_RX_NKInsuranceValueCurrency()
		{
		}

		protected override void CheckDiscountValueCurrency()
		{
		}

		protected override void CheckOtherChargesValueCurrency()
		{
		}

		protected override void CheckABL_RX_NKCustomsValueCurrency()
		{
		}

		protected override void CheckABL_VolumeUQ()
		{
			if (Parent.IsNVC)
			{
				base.CheckABL_VolumeUQ();
			}
		}

		protected override ZBool NeedsToShowABL_ManifestUQNotMappedMessageError => false;

		AsycudaBillValidationHelper Helper => helper ??= new AsycudaBillValidationHelper(Parent);

		AsycudaBillValidationHelper helper;
	}
}
