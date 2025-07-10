using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants.Customs.Universal.RefCusCodeList;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public partial class AsycudaBillValidationForRegularBill : AsycudaBillValidation
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDiscountValue();
			ValidateDiscountValueCurrency();
			ValidateOtherChargesValue();
			ValidateOtherChargesValueCurrency();
		}

		protected override void CheckABL_BillNumber()
		{
			base.CheckABL_BillNumber();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BillNumberInfo);
			var header = Parent.Header;

			if (header != null)
			{
				var duplicateBills = from AsycudaBill b in header.Bills
									 where b.PK != Parent.PK && b.ABL_BillNumber == Parent.ABL_BillNumber
									 select new { b.PK };

				if (duplicateBills.Any())
				{
					Parent.ABL_BillNumberInfo.AddNotification(NotificationTypeForDuplicateBillNumber, "Bill number must be unique");
				}

				if (!header.IsAir)
				{
					CheckAtLeastOnePackWhereRelevant();  // Called from CheckBillNumber() because we're validating for the *lack* of packs so when we need to show a message it needs to live somewhere.
				}
			}
		}

		protected virtual INotificationType NotificationTypeForDuplicateBillNumber => CargoWise.EntityFramework.NotificationType.Warning;

		protected virtual void CheckAtLeastOnePackWhereRelevant()
		{
			if (Parent.Packs.Count == 0 && Parent.Header.Containers.Count > 0)
			{
				Parent.ABL_BillNumberInfo.AddWarning("Containers exist on this manifest.  If this bill is packed into a container please enter at least one pack line.");
			}
		}

		#region DiscountValue

		public void ValidateDiscountValue()
		{
			ValidateCalculatedProperty(Parent.DiscountValueInfo);
		}

		protected virtual void CheckDiscountValue()
		{
			ValidateDiscountValueCurrency();
		}

		#endregion

		#region DiscountValueCurrency

		public void ValidateDiscountValueCurrency()
		{
			ValidateCalculatedProperty(Parent.DiscountValueCurrencyInfo);
		}

		protected virtual void CheckDiscountValueCurrency()
		{
			ValidationHelper.CheckCurrency(Parent.DiscountValueCurrencyInfo, Parent.DiscountValue);
		}

		#endregion

		#region OtherChangesValue

		public void ValidateOtherChargesValue()
		{
			ValidateCalculatedProperty(Parent.OtherChargesValueInfo);
		}

		protected virtual void CheckOtherChargesValue()
		{
			ValidateOtherChargesValueCurrency();
		}

		#endregion

		#region OtherChangesValueCurrency

		public void ValidateOtherChargesValueCurrency()
		{
			ValidateCalculatedProperty(Parent.OtherChargesValueCurrencyInfo);
		}

		protected virtual void CheckOtherChargesValueCurrency()
		{
			ValidationHelper.CheckCurrency(Parent.OtherChargesValueCurrencyInfo, Parent.OtherChargesValue);
		}

		#endregion

		protected override void CheckABL_GoodsDescription()
		{
			base.CheckABL_GoodsDescription();
			ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_GoodsDescriptionInfo, ManifestValidationRuleCodes.GoodsDescription);
		}

		protected override void CheckABL_GrossWeight()
		{
			base.CheckABL_GrossWeight();
			if (NeedsToCheckABL_GrossWeightMatchSumOfPacks)
			{
				if (Parent.Packs.Count > 0 && !Parent.ABL_GrossWeight.IsEmpty)
				{
					var totalPackedGrossWeight = Parent.TotalPacksGrossWeight;
					if (totalPackedGrossWeight != Parent.ABL_GrossWeight)
					{
						Parent.ABL_GrossWeightInfo.AddMessageError(ResString.GetMultilingualString("F08AE44E-B5E6-4A97-A301-FCAE796B198F", "Weight of packages ({0}) is not equal to manifest Gross Weight ({1}) in ({2})", totalPackedGrossWeight, Parent.ABL_GrossWeight, Parent.ABL_GrossWeightUQ));
					}
				}
			}
		}

		protected override ZBool NeedsToCheckABL_GrossWeight => true;

		protected virtual ZBool NeedsToCheckABL_GrossWeightMatchSumOfPacks => false;

		protected override ZBool NeedsToCheckABL_GrossWeightUQ => true;

		protected override void CheckABL_NetWeightUQ()
		{
			base.CheckABL_NetWeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_NetWeightUQInfo);
		}

		protected virtual ZBool NeedsToCheckABL_VolumeMatchSumOfPacks => false;

		protected override void CheckABL_Volume()
		{
			base.CheckABL_Volume();
			ValidateABL_VolumeUQ();
			MandatoryValidation.CheckNotNegative(Parent.ABL_VolumeInfo);

			if (NeedsToCheckABL_VolumeMatchSumOfPacks)
			{
				if (Parent.Packs.Count > 0 && !Parent.ABL_Volume.IsEmpty)
				{
					var totalPackedVolume = Parent.TotalPacksVolume;
					if (totalPackedVolume != Parent.ABL_Volume)
					{
						Parent.ABL_VolumeInfo.AddMessageError(ResString.GetMultilingualString("32045D0A-C4E4-4AF1-8A41-ED880D07F1B0", "Volume of packages ({0}) is not equal to manifest Volume ({1}) in ({2})", totalPackedVolume, Parent.ABL_Volume, Parent.ABL_VolumeUQ));
					}
				}
			}
		}

		protected override void CheckABL_MarksAndNumbers()
		{
			base.CheckABL_MarksAndNumbers();
			ZZValidationHeaderHelper?.CheckIsMandatoryFor(Parent.ABL_MarksAndNumbersInfo, ManifestValidationRuleCodes.MarksAndNumbers);
		}

		protected override void CheckABL_VolumeUQ()
		{
			base.CheckABL_VolumeUQ();
			if (!Parent.ABL_Volume.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ABL_VolumeUQInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_VolumeUQInfo);
			}
		}

		protected override void CheckABL_RL_NKFinalDestination()
		{
			base.CheckABL_RL_NKFinalDestination();
			if (Parent.ABL_RL_NKFinalDestination.IsEmpty && NeedsToCheckABL_RL_NKFinalDestination)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_RL_NKFinalDestinationInfo, ManifestValidationRuleCodes.FinalDestination);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RL_NKFinalDestinationInfo);
				if (Parent.IsAir && (Parent.FinalDestination?.RL_IATA ?? ZString.Empty).IsEmpty)
				{
					ZZValidationHeaderHelper.CheckIsMandatoryForValidationRuleWhenTheRelatedValueIsEmpty(Parent.ABL_RL_NKFinalDestinationInfo, ManifestValidationRuleCodes.IATAFinalDestination);
				}
			}
		}

		protected virtual ZBool NeedsToCheckABL_RL_NKFinalDestination => true;

		protected override void CheckABL_RL_NKOrigin()
		{
			base.CheckABL_RL_NKOrigin();
			CheckMandatoryABL_RL_NKOrigin();
			if (!Parent.ABL_RL_NKOrigin.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RL_NKOriginInfo);
				if (Parent.IsAir && (Parent.Origin?.RL_IATA ?? ZString.Empty).IsEmpty)
				{
					ZZValidationHeaderHelper.CheckIsMandatoryForValidationRuleWhenTheRelatedValueIsEmpty(Parent.ABL_RL_NKOriginInfo, ManifestValidationRuleCodes.IATAPortOfOrigin);
				}
			}
		}

		protected virtual void CheckMandatoryABL_RL_NKOrigin()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RL_NKOriginInfo);
		}

		protected override void CheckABL_OA_Consignee()
		{
			base.CheckABL_OA_Consignee();
			ValidateConsignee();
		}

		void ValidateConsignee()
		{
			if (!Parent.ABL_OA_ConsigneeInfo.HasErrors())
			{
				OrgAddress consignee = null;
				if (Parent.ABL_OA_Consignee.IsEmpty)
				{
					CheckMandatoryABL_OA_Consignee();
				}
				else
				{
					consignee = Parent.Consignee;
				}
				var haveMandatoryCusCodes = ZZValidationHeaderHelper.HasMandatoryCusCode(ManifestValidationRuleCodes.Consignee, consignee?.Header);
				if (!haveMandatoryCusCodes.IsEmpty)
				{
					Parent.ABL_OA_ConsigneeInfo.AddMessageError(haveMandatoryCusCodes);
				}
			}
		}

		protected virtual void CheckMandatoryABL_OA_Consignee()
		{
			if (Parent.ABL_ConsigneeName.IsEmpty)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_OA_ConsigneeInfo, ManifestValidationRuleCodes.Consignee);
			}
		}

		protected override void CheckABL_OA_Shipper()
		{
			base.CheckABL_OA_Shipper();
			if (!Parent.ABL_OA_ShipperInfo.HasErrors())
			{
				OrgAddress shipper = null;
				if (Parent.ABL_OA_Shipper.IsEmpty)
				{
					if (Parent.ABL_ShipperName.IsEmpty)
					{
						ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_OA_ShipperInfo, ManifestValidationRuleCodes.Consignor);
					}
				}
				else
				{
					shipper = Parent.Shipper;
				}
				var haveMandatoryCusCodes = ZZValidationHeaderHelper.HasMandatoryCusCode(ManifestValidationRuleCodes.Consignor, shipper?.Header);
				if (!haveMandatoryCusCodes.IsEmpty)
				{
					Parent.ABL_OA_ShipperInfo.AddMessageError(haveMandatoryCusCodes);
				}
			}
		}

		protected override void CheckABL_OA_NotifyParty()
		{
			base.CheckABL_OA_NotifyParty();
			if (!Parent.ABL_OA_NotifyPartyInfo.HasErrors())
			{
				OrgAddress notifyParty = null;
				if (Parent.ABL_OA_NotifyParty.IsEmpty)
				{
					if (Parent.ABL_NotifyPartyName.IsEmpty)
					{
						ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_OA_NotifyPartyInfo, ManifestValidationRuleCodes.Notify);
					}
				}
				else
				{
					notifyParty = Parent.NotifyParty;
				}
				var haveMandatoryCusCodes = ZZValidationHeaderHelper.HasMandatoryCusCode(ManifestValidationRuleCodes.Notify, notifyParty?.Header);
				if (!haveMandatoryCusCodes.IsEmpty)
				{
					Parent.ABL_OA_NotifyPartyInfo.AddMessageError(haveMandatoryCusCodes);
				}
			}
		}

		protected override void CheckABL_ManifestQty()
		{
			base.CheckABL_ManifestQty();
			if (NeedsToCheckABL_ManifestQty)
			{
				CheckABL_ManifestQtyMatchSumOfPacks();
			}
		}

		protected override ZBool NeedsToCheckABL_ManifestQty => true;

		protected virtual INotificationType ABL_ManifestQtyMatchSumOfPacksNotificationType => CargoWise.EntityFramework.NotificationType.Warning;

		protected virtual void CheckABL_ManifestQtyMatchSumOfPacks()
		{
			if (Parent.Packs.Count > 0)
			{
				var totalPackedPieces = (from AsycudaPack p in Parent.Packs select (int)p.APA_PackQty).Sum();
				if (totalPackedPieces != Parent.ABL_ManifestQty)
				{
					Parent.ABL_ManifestQtyInfo.AddNotification(ABL_ManifestQtyMatchSumOfPacksNotificationType, ZString.Format("Sum of packages' package counts ({0}) is not equal to manifest quantity", totalPackedPieces));
				}
			}
		}

		protected override void CheckABL_ManifestUQ()
		{
			base.CheckABL_ManifestUQ();
			if (NeedsToCheckABL_ManifestUQ)
			{
				if (Parent.ABL_ManifestUQ.IsEmpty)
				{
					if (NeedsToShowABL_ManifestUQNotEnteredMessageError)
					{
						Parent.ABL_ManifestUQInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(Parent.ABL_ManifestUQInfo.HumanReadableName));
					}
				}
				else
				{
					if (NeedsToShowABL_ManifestUQNotMappedMessageError)
					{
						var countryCode = Parent.CountryCode;
						if (!countryCode.IsEmpty && countryCode.IsSupportedCountries(Parent.Factory))
						{
							ShowMessageErrorIfUQUnknownForCountry(countryCode, Parent.ABL_ManifestUQInfo);
						}
					}
				}
			}
		}

		protected override ZBool NeedsToCheckABL_ManifestUQ => true;

		protected virtual ZBool NeedsToShowABL_ManifestUQNotEnteredMessageError => true;

		protected virtual ZBool NeedsToShowABL_ManifestUQNotMappedMessageError => true;

		void ShowMessageErrorIfUQUnknownForCountry(ZString countryCode, ZPropertyInfo zPropertyInfo)
		{
			var commericalUq = (ZString)zPropertyInfo.Value;
			AsycudaUniversalReference.CusRefPackLoaderHelper.MessageErrorIfNeeded(countryCode, commericalUq, zPropertyInfo, Parent.Factory);
		}

		protected override void CheckABL_RX_NKFreightValueCurrency()
		{
			base.CheckABL_RX_NKFreightValueCurrency();
			ValidationHelper.CheckCurrency(Parent.ABL_RX_NKFreightValueCurrencyInfo, Parent.ABL_FreightValue);
		}

		protected override void CheckABL_RX_NKInsuranceValueCurrency()
		{
			base.CheckABL_RX_NKInsuranceValueCurrency();
			ValidationHelper.CheckCurrency(Parent.ABL_RX_NKInsuranceValueCurrencyInfo, Parent.ABL_InsuranceValue);
		}

		protected override void CheckABL_RX_NKTransportValueCurrency()
		{
			base.CheckABL_RX_NKTransportValueCurrency();
			ValidationHelper.CheckCurrency(Parent.ABL_RX_NKTransportValueCurrencyInfo, Parent.ABL_TransportValue);
		}

		protected override void CheckABL_CustomsValue()
		{
			base.CheckABL_CustomsValue();
			ValidateABL_RX_NKCustomsValueCurrency();
		}

		protected override void CheckABL_RX_NKCustomsValueCurrency()
		{
			base.CheckABL_RX_NKCustomsValueCurrency();
			ValidationHelper.CheckCurrency(Parent.ABL_RX_NKCustomsValueCurrencyInfo, Parent.ABL_CustomsValue);
		}

		protected override void CheckABL_FreightValue()
		{
			base.CheckABL_FreightValue();
			ValidateABL_RX_NKFreightValueCurrency();
		}

		protected override void CheckABL_TransportValue()
		{
			base.CheckABL_TransportValue();
			ValidateABL_RX_NKTransportValueCurrency();
		}

		protected override void CheckABL_InsuranceValue()
		{
			base.CheckABL_InsuranceValue();
			ValidateABL_RX_NKInsuranceValueCurrency();
		}

		protected override void CheckABL_BolType()
		{
			base.CheckABL_BolType();
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_BolTypeInfo);
			ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_BolTypeInfo, ManifestValidationRuleCodes.BillType);
		}

		#region Shipper

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();
			if (!Parent.ShipperUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ShipperNameInfo, ManifestValidationRuleCodes.Consignor);
			}
		}

		protected override void CheckABL_ShipperStreet1()
		{
			base.CheckABL_ShipperStreet1();
			if (!Parent.ShipperUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ShipperStreet1Info, ManifestValidationRuleCodes.Consignor);
			}
		}

		protected override void CheckABL_ShipperCity()
		{
			base.CheckABL_ShipperCity();
			if (!Parent.ShipperUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ShipperCityInfo, ManifestValidationRuleCodes.Consignor);
			}
		}

		protected override void CheckABL_ShipperState()
		{
			base.CheckABL_ShipperState();
			if (!Parent.ShipperUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ShipperStateInfo, ManifestValidationRuleCodes.ConsignorState);
			}
		}

		protected virtual ZBool NeedsToCheckABL_ShipperPostcode => true;

		protected override void CheckABL_ShipperPostcode()
		{
			base.CheckABL_ShipperPostcode();
			if (NeedsToCheckABL_ShipperPostcode && !Parent.ShipperUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ShipperPostcodeInfo, ManifestValidationRuleCodes.Consignor);
			}
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			base.CheckABL_RN_NKShipperCountry();
			if (!Parent.ShipperUseRealOrg)
			{
				ZZValidationHeaderHelper?.CheckIsMandatoryFor(Parent.ABL_RN_NKShipperCountryInfo, ManifestValidationRuleCodes.Consignor);
			}
		}

		#endregion

		#region NotifyParty

		protected override void CheckABL_NotifyPartyName()
		{
			base.CheckABL_NotifyPartyName();
			if (!Parent.NotifyPartyUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_NotifyPartyNameInfo, ManifestValidationRuleCodes.Notify);
			}
		}

		protected override void CheckABL_NotifyPartyStreet1()
		{
			base.CheckABL_NotifyPartyStreet1();
			if (!Parent.NotifyPartyUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_NotifyPartyStreet1Info, ManifestValidationRuleCodes.Notify);
			}
		}

		protected override void CheckABL_NotifyPartyCity()
		{
			base.CheckABL_NotifyPartyCity();
			if (!Parent.NotifyPartyUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_NotifyPartyCityInfo, ManifestValidationRuleCodes.Notify);
			}
		}

		protected override void CheckABL_NotifyPartyState()
		{
			base.CheckABL_NotifyPartyState();
			if (!Parent.NotifyPartyUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_NotifyPartyStateInfo, ManifestValidationRuleCodes.NotifyState);
			}
		}

		protected override void CheckABL_NotifyPartyPostcode()
		{
			base.CheckABL_NotifyPartyPostcode();
			if (!Parent.NotifyPartyUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_NotifyPartyPostcodeInfo, ManifestValidationRuleCodes.Notify);
			}
		}

		protected override void CheckABL_RN_NKNotifyPartyCountry()
		{
			base.CheckABL_RN_NKNotifyPartyCountry();
			if (!Parent.NotifyPartyUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_RN_NKNotifyPartyCountryInfo, ManifestValidationRuleCodes.Notify);
			}
		}

		protected override void CheckABL_NotifyPartyPhone()
		{
			base.CheckABL_NotifyPartyPhone();
			if (!Parent.NotifyPartyUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_NotifyPartyPhoneInfo, ManifestValidationRuleCodes.NotifyPhone);
			}
		}

		#endregion

		#region Consignee

		protected override void CheckABL_ConsigneeName()
		{
			base.CheckABL_ConsigneeName();
			if (!Parent.ConsigneeUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ConsigneeNameInfo, ManifestValidationRuleCodes.Consignee);
			}
		}

		protected virtual ZBool NeedsToCheckABL_Consignee => true;

		protected override void CheckABL_ConsigneeStreet1()
		{
			base.CheckABL_ConsigneeStreet1();
			if (NeedsToCheckABL_Consignee && !Parent.ConsigneeUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ConsigneeStreet1Info, ManifestValidationRuleCodes.Consignee);
			}
		}

		protected override void CheckABL_ConsigneeCity()
		{
			base.CheckABL_ConsigneeCity();
			if (NeedsToCheckABL_Consignee && !Parent.ConsigneeUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ConsigneeCityInfo, ManifestValidationRuleCodes.Consignee);
			}
		}

		protected override void CheckABL_ConsigneeState()
		{
			base.CheckABL_ConsigneeState();
			if (!Parent.ConsigneeUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ConsigneeStateInfo, ManifestValidationRuleCodes.ConsigneeState);
			}
		}

		protected virtual ZBool NeedsToCheckABL_ConsigneePostcode => true;

		protected override void CheckABL_ConsigneePostcode()
		{
			base.CheckABL_ConsigneePostcode();
			if (NeedsToCheckABL_Consignee && NeedsToCheckABL_ConsigneePostcode && !Parent.ConsigneeUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ConsigneePostcodeInfo, ManifestValidationRuleCodes.Consignee);
			}
		}

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			base.CheckABL_RN_NKConsigneeCountry();
			if (NeedsToCheckABL_Consignee && !Parent.ConsigneeUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_RN_NKConsigneeCountryInfo, ManifestValidationRuleCodes.Consignee);
			}
		}

		protected override void CheckABL_ConsigneePhone()
		{
			base.CheckABL_ConsigneePhone();
			if (!Parent.ConsigneeUseRealOrg)
			{
				ZZValidationHeaderHelper.CheckIsMandatoryFor(Parent.ABL_ConsigneePhoneInfo, ManifestValidationRuleCodes.ConsigneePhone);
			}
		}

		#endregion
	}
}
