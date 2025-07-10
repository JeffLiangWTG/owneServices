using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business.Extensions;
using Enterprise.Customs.Universal.Helper;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidationForRegularBill
	{
		public AsycudaBillValidationForRegularBill(AsycudaBill parent)
			: base(parent)
		{
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			CheckF50HasAPack();
			CheckF14HasSupplementaryDeclarant();
			CheckF14F15HasPack();
			CheckF14F15SpecificCirumstandIndicatorItinerary();
			ValidateReceptacleId();
			ValidateTransportDocumentType();
			ValidateShipperPersonType();
		}

		void CheckF50HasAPack()
		{
			var message = Res.GetString("a82b418b-04f7-4118-bb9d-b707d6592df3", "You have not entered any Pack Details.");
			Parent.RemoveRowMessageError(message);

			if (Parent.Packs.Count == 0 && Parent.Header.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F50))
			{
				Parent.AddRowMessageError(message);
			}
		}

		void CheckF14HasSupplementaryDeclarant()
		{
			var message = Res.GetString("1980D998-86BA-4BA6-9A03-23BDB6959C2F", "You have not entered a Supplementary Declarant.");
			Parent.RemoveRowMessageError(message);

			if (Parent.SupplementaryDeclarants.Count == 0 && Parent.Header.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F14)
				&& Parent.Factory.IsMemberOfEU(Parent.ABL_RL_NKFinalDestination.SubstringSafe(0, 2)))
			{
				Parent.AddRowMessageError(message);
			}
		}

		void CheckF14F15HasPack()
		{
			var message = Res.GetString("49C3CF54-AB70-4AA2-A6F5-9055C395B93F", "At least one Pack is required per Bill.");
			Parent.RemoveRowMessageError(message);

			if ((Parent.Header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F14
				|| Parent.Header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F15)
				&& Parent.Packs.Count == 0)
			{
				Parent.AddRowMessageError(message);
			}
		}

		void CheckF14F15SpecificCirumstandIndicatorItinerary()
		{
			var message = Res.GetString("4639FE76-B8AF-46C8-A089-05550A48FE9B", "Itinerary must at least contain Origin and Final Destination.");
			Parent.RemoveRowMessageError(message);

			if ((Parent.Header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F14
				|| Parent.Header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F15)
				&& !OriginAndFinalDestinationHasItinerary())
			{
				Parent.AddRowMessageError(message);
			}
		}

		bool OriginAndFinalDestinationHasItinerary()
		{
			var itineraryCountryCodes = Parent.Header.Itinerary.Cast<RouteEntry>().Select(i => i.CY_Data).Distinct();
			var originCountryCode = Parent.Origin?.Country?.Code ?? ZString.Empty;
			var destinationCountryCode = Parent.FinalDestination?.Country?.Code ?? ZString.Empty;

			return !originCountryCode.IsEmpty
				&& !destinationCountryCode.IsEmpty
				&& itineraryCountryCodes.Any(i => i == originCountryCode)
				&& itineraryCountryCodes.Any(i => i == destinationCountryCode);
		}

		protected override void CheckABL_SequenceNumber()
		{
			base.CheckABL_SequenceNumber();
			MandatoryValidation.CheckNotZero(Parent.ABL_SequenceNumberInfo);
		}

		protected override void CheckABL_ManifestUQ()
		{
			base.CheckABL_ManifestUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_ManifestUQInfo);
		}

		protected override void CheckABL_ShipmentType()
		{
			base.CheckABL_ShipmentType();

			var parent = Parent;
			if (parent.ABL_ShipmentType != ShipmentTypeList.Codes.Import23)
			{
				parent.ABL_ShipmentTypeInfo.AddMessageError(ImportManifestMessage);
			}
		}

		protected override void CheckABL_ShipperPhone()
		{
			var parent = Parent;
			base.CheckABL_ShipperPhone();
			if (!parent.ShipperUseRealOrg)
			{
				ValidationHelper.CheckValidPhoneNumber(parent.ABL_ShipperPhone, parent.ABL_ShipperPhoneInfo, Res.GetString("a24b53f6-3005-4144-84c5-5e3eef8912a6", "Shipper"));
			}
		}

		protected override void CheckABL_OA_Consignee()
		{
			if (Parent.Header.SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F44)
			{
				base.CheckABL_OA_Consignee();
			}
		}

		protected override void CheckABL_ConsigneePhone()
		{
			var parent = Parent;
			base.CheckABL_ConsigneePhone();
			if (!parent.ConsigneeUseRealOrg)
			{
				ValidationHelper.CheckValidPhoneNumber(Parent.ABL_ConsigneePhone, Parent.ABL_ConsigneePhoneInfo, Res.GetString("cf49cd50-e080-4c63-a38a-a8cc90a5c561", "Consignee"));
			}
		}

		protected override void CheckABL_NotifyPartyPhone()
		{
			var parent = Parent;
			base.CheckABL_NotifyPartyPhone();
			if (!parent.NotifyPartyUseRealOrg)
			{
				ValidationHelper.CheckValidPhoneNumber(Parent.ABL_NotifyPartyPhone, Parent.ABL_NotifyPartyPhoneInfo, Res.GetString("5f9f2203-69bd-432d-b1b5-edd78659b9e8", "Notify Party"));
			}
		}

		protected override void CheckABL_PrepaidCollect()
		{
			base.CheckABL_PrepaidCollect();
			var targetInfo = Parent.ABL_PrepaidCollectInfo;

			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValues(targetInfo,
				Parent.Header.SpecificCircumstanceIndicatorInfo,
				new IZType[]
				{
					(ZString)EUICS2SpecificCircumstanceList.Codes.F22, (ZString)EUICS2SpecificCircumstanceList.Codes.F26, (ZString)EUICS2SpecificCircumstanceList.Codes.F50,
					(ZString)EUICS2SpecificCircumstanceList.Codes.F14, (ZString)EUICS2SpecificCircumstanceList.Codes.F15
				},
				Res.GetString("8C543F91-AB32-471A-B9B1-2663BB6C86EE", "You have not entered a Method of Payment"));

			ListValidation.MessageErrorIfInvalidCode(targetInfo, Parent.Lookups.PrepaidCollectList);
		}

		protected override void CheckABL_FreightValue()
		{
			base.CheckABL_FreightValue();
			var propertyInfo = Parent.ABL_FreightValueInfo;

			if (propertyInfo.Value.IsEmpty && Parent.Header.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F43)
			&& Parent.AdditionalInfos.Cast<AdditionalInfo>().Any(x => x.CSI_Code == EUICS2AdditionalInfoTypes.Codes.CL701_10900))
			{
				propertyInfo.AddMessageError(Res.GetString("F45666A8-ED08-4A00-AEDC-510678D8B2A3", "You have not entered Postal Charges."));
			}
		}

		protected override void CheckABL_RX_NKFreightValueCurrency()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RX_NKFreightValueCurrencyInfo, Parent.Lookups.FreightValueCurrencies, ResString.GetMultilingualString("981C7EB8-8922-4844-A536-DD6D5829D0CA", "Enter a valid Currency."));

			var amount = Parent.ABL_FreightValue;
			if (!amount.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RX_NKFreightValueCurrencyInfo);
			}
		}

		public void ValidateReceptacleId()
		{
			ValidateCalculatedProperty(Parent.ReceptacleIdInfo);
		}

		protected void CheckReceptacleId()
		{
			var parent = Parent;
			var manifestHeader = parent.Header;
			var specificCircumstanceIndicator = manifestHeader.SpecificCircumstanceIndicator;

			var message = Res.GetString("B27BADCF-F24E-439E-B1A9-1FCB5B15DAF4", "You have not entered a Receptacle Identification Number.");

			if (manifestHeader.IsForwarderManifest && parent.ReceptacleId.IsEmpty && specificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F44)
			{
				parent.ReceptacleIdInfo.AddMessageError(message);
			}
		}

		public void ValidateTransportDocumentType()
		{
			ValidateCalculatedProperty(Parent.TransportDocumentTypeInfo);
		}

		protected void CheckTransportDocumentType()
		{
			var parent = Parent;
			var manifestHeader = parent.Header;
			MandatoryValidation.AddMessageErrorIfNotEnteredAndOtherPropertyHasValue(parent.TransportDocumentTypeInfo, manifestHeader.SpecificCircumstanceIndicatorInfo, (ZString)EUICS2SpecificCircumstanceList.Codes.F44);
			ValidationHelper.CheckTransportDocumentType(parent);
		}

		protected override void CheckABL_ManifestQty()
		{
			var parent = Parent;
			var manifestHeader = parent.Header;
			var skipCheckManifestQty =
				manifestHeader.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F14 ||
				manifestHeader.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F15 ||
				manifestHeader.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F16 ||
				manifestHeader.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F17 ||
				manifestHeader.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F44;
			if (skipCheckManifestQty)
			{
				return;
			}
			base.CheckABL_ManifestQty();
		}

		bool ShouldSkipValidationForEnsWithF16OrF17 =>
			Parent.Header is AsycudaManifestHeader header &&
			header.AMA_ManifestType == EUICS2ManifestTypes.Codes.ENS &&
			header.SpecificCircumstanceIndicator.ToString() is EUICS2SpecificCircumstanceList.Codes.F16 or EUICS2SpecificCircumstanceList.Codes.F17;

		protected override ZBool NeedsToCheckABL_RL_NKFinalDestination
		{
			get
			{
				if (ShouldSkipValidationForEnsWithF16OrF17 || Parent.Header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F44)
				{
					return false;
				}

				return true;
			}
		}

		protected override void CheckMandatoryABL_RL_NKOrigin()
		{
			if (ShouldSkipValidationForEnsWithF16OrF17 || Parent.Header.SpecificCircumstanceIndicator == EUICS2SpecificCircumstanceList.Codes.F44)
			{
				return;
			}
			base.CheckMandatoryABL_RL_NKOrigin();
		}

		bool BuyerSellerMandatory
		{
			get
			{
				var parent = Parent;

				return parent.Header.SpecificCircumstanceIndicator.ToString() switch
				{
					EUICS2SpecificCircumstanceList.Codes.F15 or EUICS2SpecificCircumstanceList.Codes.F16 or EUICS2SpecificCircumstanceList.Codes.F50 or EUICS2SpecificCircumstanceList.Codes.F51 when parent.FinalDestination is { } finalDestination => finalDestination.IsInEU,
					EUICS2SpecificCircumstanceList.Codes.F17 => true,
					_ => false,
				};
			}
		}

		protected override ZBool NeedsToCheckABL_ManifestUQ => false;

		protected override ZBool NeedsToCheckABL_GrossWeight => Parent.Header.SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F44;
		protected override ZBool NeedsToCheckABL_GrossWeightUQ => Parent.Header.SpecificCircumstanceIndicator != EUICS2SpecificCircumstanceList.Codes.F44;

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		string ImportManifestMessage => Res.GetString("c2b906df-c2d3-4064-a56a-834cecb66e07", "ICS2 manifests are Import manifests. Please select Import from the list.");

		#region Shipper

		public void ValidateShipperPersonType()
		{
			ValidateCalculatedProperty(Parent.ShipperPersonTypeInfo);
		}

		protected void CheckShipperPersonType()
		{
			ValidateShipperRequired(Parent.ShipperPersonTypeInfo);
		}

		protected override void CheckABL_OA_Shipper()
		{
			base.CheckABL_OA_Shipper();

			var parent = Parent;
			if (parent.ABL_ShipperName.IsEmpty)
			{
				ValidateShipperRequired(parent.ABL_OA_ShipperInfo);
			}
		}

		protected override void CheckABL_ShipperPostcode()
		{
			var parent = Parent;

			var shipperCountry = parent.ABL_RN_NKShipperCountry;
			if (!shipperCountry.IsEmpty && !parent.Lookups.CodeList733.ContainsCode(shipperCountry))
			{
				ValidateShipperRequired(parent.ABL_ShipperPostcodeInfo);
			}
		}

		protected override void CheckABL_ShipperName()
		{
			base.CheckABL_ShipperName();

			ValidateShipperRequired(Parent.ABL_ShipperNameInfo);
		}

		protected override void CheckABL_ShipperCity()
		{
			base.CheckABL_ShipperCity();

			ValidateShipperRequired(Parent.ABL_ShipperCityInfo);
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			base.CheckABL_RN_NKShipperCountry();

			ValidateShipperRequired(Parent.ABL_RN_NKShipperCountryInfo);
			if (Parent.Header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RN_NKShipperCountryInfo);
			}
		}

		void ValidateShipperRequired(ZPropertyInfo propertyInfo)
		{
			if (Parent.Header is AsycudaManifestHeader header
				&& ((header.AMA_ManifestType == EUICS2ManifestTypes.Codes.ENS && header.SpecificCircumstanceIndicator.ToString() is EUICS2SpecificCircumstanceList.Codes.F14 or EUICS2SpecificCircumstanceList.Codes.F15 or EUICS2SpecificCircumstanceList.Codes.F40)
					|| header.IsInlandWaterwayOrSeaCarrierManifestWithF11OrF12Indicator))
			{
				ValidationHelper.AddShipperRequiredMessageErrorIfEmpty(propertyInfo);
			}
		}

		#endregion

		#region Buyer

		string Buyer => Res.GetString("C344CFB0-DA67-49FB-8E0D-330EC84595FB", "Buyer");

		protected override void CheckABL_OA_Buyer()
		{
			base.CheckABL_OA_Buyer();
			if (Parent.ABL_OA_Buyer.IsEmpty && Parent.ABL_BuyerName.IsEmpty && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_OA_BuyerInfo, Buyer);
			}
		}

		protected override void CheckABL_BuyerName()
		{
			base.CheckABL_BuyerName();
			if (!Parent.BuyerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BuyerNameInfo, Buyer);
			}
		}

		protected override void CheckABL_BuyerStreet1()
		{
			base.CheckABL_BuyerStreet1();
			if (!Parent.BuyerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BuyerStreet1Info, Buyer);
			}
		}

		protected override void CheckABL_BuyerCity()
		{
			base.CheckABL_BuyerCity();
			if (!Parent.BuyerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BuyerCityInfo, Buyer);
			}
		}

		protected override void CheckABL_BuyerState()
		{
			base.CheckABL_BuyerState();
			if (!Parent.BuyerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BuyerStateInfo, Buyer);
			}
		}

		protected override void CheckABL_BuyerPostcode()
		{
			base.CheckABL_BuyerPostcode();
			if (!Parent.BuyerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_BuyerPostcodeInfo, Buyer);
			}
		}

		protected override void CheckABL_RN_NKBuyerCountry()
		{
			base.CheckABL_RN_NKBuyerCountry();
			if (!Parent.BuyerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RN_NKBuyerCountryInfo, Buyer);
			}
		}

		#endregion

		#region Seller

		string Seller => Res.GetString("36028D2F-2544-4180-A47D-0BE1B129B438", "Seller");

		protected override void CheckABL_OA_Seller()
		{
			base.CheckABL_OA_Seller();
			if (Parent.ABL_OA_Seller.IsEmpty && Parent.ABL_SellerName.IsEmpty && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_OA_SellerInfo, Seller);
			}
		}

		protected override void CheckABL_SellerName()
		{
			base.CheckABL_SellerName();
			if (!Parent.SellerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_SellerNameInfo, Seller);
			}
		}

		protected override void CheckABL_SellerStreet1()
		{
			base.CheckABL_SellerStreet1();
			if (!Parent.SellerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_SellerStreet1Info, Seller);
			}
		}

		protected override void CheckABL_SellerCity()
		{
			base.CheckABL_SellerCity();
			if (!Parent.SellerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_SellerCityInfo, Seller);
			}
		}

		protected override void CheckABL_SellerState()
		{
			base.CheckABL_SellerState();
			if (!Parent.SellerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_SellerStateInfo, Seller);
			}
		}

		protected override void CheckABL_SellerPostcode()
		{
			base.CheckABL_SellerPostcode();
			if (!Parent.SellerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_SellerPostcodeInfo, Seller);
			}
		}

		protected override void CheckABL_RN_NKSellerCountry()
		{
			base.CheckABL_RN_NKSellerCountry();
			if (!Parent.SellerUseRealOrg && BuyerSellerMandatory)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_RN_NKSellerCountryInfo, Seller);
			}
		}

		#endregion

		#region Consignee

		protected override ZBool NeedsToCheckABL_Consignee
		{
			get
			{
				if ((IsCircumstanceF16F17OrF14F15WithAdditionalInfo() && IsConsigneeDetailsEmpty()) || IsF44Manifest())
				{
					return false;
				}

				return true;
			}
		}

		protected override void CheckMandatoryABL_OA_Consignee()
		{
			if (!IsCircumstanceF16F17OrF14F15WithAdditionalInfo() && !IsF44Manifest())
			{
				base.CheckMandatoryABL_OA_Consignee();
			}
		}

		protected override void CheckABL_ConsigneeName()
		{
			if (NeedsToCheckABL_Consignee)
			{
				base.CheckABL_ConsigneeName();
			}
		}

		bool IsF44Manifest() => Parent.Header.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F44);

		bool IsCircumstanceF16F17OrF14F15WithAdditionalInfo()
		{
			var parent = Parent;
			var header = parent.Header;

			if (header.AMA_ManifestType != EUICS2ManifestTypes.Codes.ENS)
			{
				return false;
			}

			var isF16F17 = header.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F16)
				|| header.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F17);

			var isF14F15 = header.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F14)
			|| header.SpecificCircumstanceIndicator.EqualsIgnoringCase(EUICS2SpecificCircumstanceList.Codes.F15);

			return isF16F17 || (isF14F15 && parent.HasAdditionalInfoWithCode10600);
		}

		bool IsConsigneeDetailsEmpty()
		{
			var parent = Parent;
			return parent.ConsigneeABLAddress.AreEmpty() && parent.ABL_ConsigneeRegNo.IsEmpty && parent.ConsigneePersonType.IsEmpty;
		}

		#endregion

		protected override void CheckABL_BuyerRegNo()
		{
			base.CheckABL_BuyerRegNo();
			ErrorOnMultipleReNo(Parent.ABL_BuyerRegNoInfo, Parent.Buyer);
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			ErrorOnMultipleReNo(Parent.ABL_ConsigneeRegNoInfo, Parent.Consignee);
		}

		protected override void CheckABL_NotifyPartyRegNo()
		{
			base.CheckABL_NotifyPartyRegNo();
			ErrorOnMultipleReNo(Parent.ABL_NotifyPartyRegNoInfo, Parent.NotifyParty);
		}

		protected override void CheckABL_SellerRegNo()
		{
			base.CheckABL_SellerRegNo();
			ErrorOnMultipleReNo(Parent.ABL_SellerRegNoInfo, Parent.Seller);
		}

		protected override void CheckABL_ShipperRegNo()
		{
			base.CheckABL_ShipperRegNo();
			ErrorOnMultipleReNo(Parent.ABL_ShipperRegNoInfo, Parent.Shipper);
		}

		void ErrorOnMultipleReNo(ZPropertyInfo propertyInfo, OrgAddress org)
		{
			if ((ZString)propertyInfo.Value == Parent.RegNumberValueOnMultiple && org?.Header is not null)
			{
				propertyInfo.AddError(Res.GetString("2ce572aa-d0e0-46e0-8c4e-89d57dd46088", "Multiple EU/XI EORI found for {0}, which is not supported. Update {0} to have only one EORI. Then clear and set party again.", org.Header.OH_Code));
			}
		}
	}
}
