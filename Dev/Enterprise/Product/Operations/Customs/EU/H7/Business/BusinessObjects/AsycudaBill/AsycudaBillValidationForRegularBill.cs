using System.Collections.Generic;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaBillValidationForRegularBill : ASYCUDA.Business.AsycudaBillValidation
	{
		public AsycudaBillValidationForRegularBill(ASYCUDA.Business.AsycudaBill parent)
			: base(parent)
		{
		}

		protected new AsycudaBill Parent => (AsycudaBill)base.Parent;

		protected virtual IMultilingualString InvalidCodeErrorMessage => null;

		public void ValidateGoodsLocationDescription()
		{
			ValidateCalculatedProperty(Parent.GoodsLocationDescriptionInfo);
		}

		public void ValidateAddititionalProcedureCodeAsString()
		{
			ValidateCalculatedProperty(Parent.AdditionalProcedureCodesAsStringInfo);
		}

		protected virtual void CheckGoodsLocationDescription()
		{
			CusGoodsLocationValidationHelper.ValidateInnerGoodsLocation(Parent);
		}

		protected override void CheckABL_RN_NKShipperCountry()
		{
			base.CheckABL_RN_NKShipperCountry();
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_RN_NKShipperCountryInfo);
		}

		protected override void CheckABL_ConsigneeRegNo()
		{
			base.CheckABL_ConsigneeRegNo();
			var consigneeRegNo = Parent.ABL_ConsigneeRegNo;

			if (!consigneeRegNo.IsEmpty && Parent.ABL_ConsigneeRegNoType == OrgCusCode.EuropeanUnionSharedCodeTypes.Eori)
			{
				var euMemberStates = GetEUMemberStates();
				var euMemberStateSubstring = consigneeRegNo.SubstringSafe(0, 2);
				if (!euMemberStates.Contains(euMemberStateSubstring))
				{
					Parent.ABL_ConsigneeRegNoInfo.AddMessageError(Parent.ValidationConfiguration.ValidationMessage.GetBR3181RuleEUMemberStateMessage());
				}
			}
		}

		protected override void CheckABL_SellerRegNo()
		{
			base.CheckABL_SellerRegNo();

			if (NeedCheckSellerRegNoEnteredAndValid)
			{
				if (Parent.ABL_SellerRegNo.IsEmpty)
				{
					var unenteredMsg = Res.GetString("8a017c5d-cc8f-46ed-836d-b83f181deb8d", "You have not entered a Seller IOSS number.");
					Parent.ABL_SellerRegNoInfo.AddMessageError(unenteredMsg);
				}
				else if (Parent.ABL_OA_Seller.IsEmpty || Parent.ABL_OA_SellerInfo.HasMessageErrors())
				{
					if (!Regex.IsMatch(Parent.ABL_SellerRegNo, $@"^IM[0-9]{{10}}$"))
					{
						Parent.ABL_SellerRegNoInfo.AddMessageError(Res.GetString("2f15259f-6e44-45aa-ac84-8e6f8241d411", "The format should be 'IMxxxyyyyyyz'. Where 'xxx' is the IOSS three letter numeric code, 'yyyyyy' is the 6-digit number allocated by the member country, and 'z' is the check digit."));
					}
				}
			}
		}

		protected virtual bool NeedCheckSellerRegNoEnteredAndValid => Parent.ABL_Procedure == EUH7AdditionalProcedureCodeList.Codes.C07F48;

		protected ICollection<ZString> GetEUMemberStates()
		{
			var refCusCodeList = RefCusCodeListTypes.GetCachedList(Parent.Factory, Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_CL010, ZDate.Today);
			return refCusCodeList.GetAllCodesZString();
		}

		protected ICollection<ZString> GetEUImporterCountries()
		{
			// We are reusing the list for Ireland as it is the same for other EU countries
			var refCusCodeList = RefCusCodeListTypes.GetCachedList(Parent.Factory, CountryCodes.Ireland, EU.Business.UniversalReferenceConstants.RefCusCodeListType.Code.Code_AI008, ZDate.Today);
			return refCusCodeList.GetAllCodesZString();
		}

		protected override void CheckABL_ConsigneeName()
		{
			base.CheckABL_ConsigneeName();
			ValidateABL_ConsigneeRegNo();
			var consigneeRegNoIsInvalid = Parent.ABL_ConsigneeRegNo.IsEmpty || Parent.ABL_ConsigneeRegNoInfo.HasMessageErrors();
			if (consigneeRegNoIsInvalid && Parent.ABL_ConsigneeName.IsEmpty)
			{
				Parent.ABL_ConsigneeNameInfo.AddMessageError(ValidationErrorMessageForABL_ConsigneeName);
			}
		}

		protected virtual string ValidationErrorMessageForABL_ConsigneeName => Res.GetString("82e9e2da-69dc-4e2d-b16e-2870425d3874", "Please enter an Importer Name or declare a valid Identification No.");

		protected override void CheckABL_ConsigneeStreet1()
		{
			base.CheckABL_ConsigneeStreet1();
			ValidateConsigneeStreet(Parent.ABL_ConsigneeStreet1Info);
			ValidateABL_ConsigneeStreet2();
		}

		protected override void CheckABL_ConsigneeStreet2()
		{
			base.CheckABL_ConsigneeStreet2();
			ValidateConsigneeStreet(Parent.ABL_ConsigneeStreet2Info);
			ValidateABL_ConsigneeStreet1();
		}

		void ValidateConsigneeStreet(ZPropertyInfo streetPropertyInfo)
		{
			var consigneeStreet1 = Parent.ABL_ConsigneeStreet1;
			var consigneeStreet2 = Parent.ABL_ConsigneeStreet2;

			ValidateABL_ConsigneeRegNo();
			var consigneeRegNoIsInvalid = Parent.ABL_ConsigneeRegNo.IsEmpty || Parent.ABL_ConsigneeRegNoInfo.HasMessageErrors();
			if (consigneeRegNoIsInvalid && consigneeStreet1.IsEmpty && consigneeStreet2.IsEmpty)
			{
				streetPropertyInfo.AddMessageError(ValidationErrorMessageForABL_ConsigneeStreet);
			}

			if (consigneeStreet1.Length + consigneeStreet2.Length > 70)
			{
				streetPropertyInfo.AddMessageError(StreetCharacterExceededErrorMessage);
			}
		}

		protected virtual string ValidationErrorMessageForABL_ConsigneeStreet => Res.GetString("719942f3-4997-4850-b5a8-23c422dc5615", "Please enter an Importer Street Address or declare a valid Identification No.");

		protected override void CheckABL_ConsigneePostcode()
		{
			base.CheckABL_ConsigneePostcode();

			ValidateABL_ConsigneeRegNo();
			var consigneeRegNoIsInvalid = Parent.ABL_ConsigneeRegNo.IsEmpty || Parent.ABL_ConsigneeRegNoInfo.HasMessageErrors();
			if (consigneeRegNoIsInvalid && Parent.ABL_ConsigneePostcode.IsEmpty)
			{
				Parent.ABL_ConsigneePostcodeInfo.AddMessageError(ValidationErrorMessageForABL_ConsigneePostcode);
			}
		}

		protected virtual string ValidationErrorMessageForABL_ConsigneePostcode => Res.GetString("ff406af3-29e8-4569-82e8-822760a04158", "Please enter an Importer Postcode or declare a valid Identification No.");

		protected override void CheckABL_ConsigneeCity()
		{
			base.CheckABL_ConsigneeCity();

			ValidateABL_ConsigneeRegNo();
			var consigneeRegNoIsInvalid = Parent.ABL_ConsigneeRegNo.IsEmpty || Parent.ABL_ConsigneeRegNoInfo.HasMessageErrors();
			if (consigneeRegNoIsInvalid && Parent.ABL_ConsigneeCity.IsEmpty)
			{
				Parent.ABL_ConsigneeCityInfo.AddMessageError(ValidationErrorMessageForABL_ConsigneeCity);
			}
		}

		protected virtual string ValidationErrorMessageForABL_ConsigneeCity => Res.GetString("c21f7669-5a2b-4ede-8a41-9dca24674cca", "Please enter an Importer City or declare a valid Identification No.");

		protected override void CheckABL_RN_NKConsigneeCountry()
		{
			base.CheckABL_RN_NKConsigneeCountry();

			ValidateABL_ConsigneeRegNo();
			var consigneeRegNoIsInvalid = Parent.ABL_ConsigneeRegNo.IsEmpty || Parent.ABL_ConsigneeRegNoInfo.HasMessageErrors();
			if (consigneeRegNoIsInvalid && Parent.ABL_RN_NKConsigneeCountry.IsEmpty)
			{
				Parent.ABL_RN_NKConsigneeCountryInfo.AddMessageError(ValidationErrorMessageForABL_RN_NKConsigneeCountry);
				return;
			}

			var euImporterCountries = GetEUImporterCountries();
			if (!Parent.ABL_RN_NKConsigneeCountry.IsEmpty && !euImporterCountries.Contains(Parent.ABL_RN_NKConsigneeCountry))
			{
				Parent.ABL_RN_NKConsigneeCountryInfo.AddMessageError(ValidationErrorMessageForABL_RN_NKConsigneeCountryIfNotInEU);
			}
		}

		protected virtual string ValidationErrorMessageForABL_RN_NKConsigneeCountry => Res.GetString("afdbcc55-3121-4cfb-b918-8613aba540fd", "Please enter an Importer Country/Region or declare a valid Identification No.");
		protected virtual string ValidationErrorMessageForABL_RN_NKConsigneeCountryIfNotInEU => Res.GetString("86b3eecd-a16c-4fbe-a78b-658e2c755247", "Please enter a valid Importer Country/Region.");

		protected override void CheckABL_ShipperStreet1()
		{
			base.CheckABL_ShipperStreet1();
			ValidateShipperStreet(Parent.ABL_ShipperStreet1Info);
			ValidateABL_ShipperStreet2();
		}

		protected override void CheckABL_ShipperStreet2()
		{
			base.CheckABL_ShipperStreet2();
			ValidateShipperStreet(Parent.ABL_ShipperStreet2Info);
			ValidateABL_ShipperStreet1();
		}

		void ValidateShipperStreet(ZPropertyInfo streetPropertyInfo)
		{
			var shipperStreet1 = Parent.ABL_ShipperStreet1;
			var shipperStreet2 = Parent.ABL_ShipperStreet2;

			if (shipperStreet1.Length + shipperStreet2.Length > 70)
			{
				streetPropertyInfo.AddMessageError(StreetCharacterExceededErrorMessage);
			}
		}

		protected override void CheckABL_TransportValueIsValidMoney()
		{
			CostValueCheck(Parent.ABL_TransportValueInfo);
		}

		protected override void CheckABL_InsuranceValueIsValidMoney()
		{
			CostValueCheck(Parent.ABL_InsuranceValueInfo);
		}

		static void CostValueCheck(ZPropertyInfo info)
		{
			MandatoryValidation.CheckNotNegative(info);
			IZType value = info.Value;
			if (value is ZDecimal costValue && costValue > 999999999999.99m)
			{
				info.AddMessageError(Res.GetString("6d1a140f-e2ab-48f7-8473-ed707d0d6e11", "The number {0} is too large, the maximum allowed for {1} is 999,999,999,999.99.", costValue.ToString("n2"), info.HumanReadableName));
			}
		}
		protected override void CheckABL_TransportValue()
		{
			base.CheckABL_TransportValue();

			if (Parent.ABL_InsuranceValue.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.ABL_TransportValueInfo, TransportAndInsuranceValueDescription);
			}
		}

		protected override void CheckABL_InsuranceValue()
		{
			base.CheckABL_InsuranceValue();

			if (Parent.ABL_TransportValue.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.ABL_InsuranceValueInfo, TransportAndInsuranceValueDescription);
			}
		}

		internal static string TransportAndInsuranceValueDescription => Res.GetString("c46dc2cc-6b5b-4105-9116-da152122c484", "Transport Value and/or Insurance Value");

		protected override void CheckABL_RX_NKTransportValueCurrency()
		{
			base.CheckABL_RX_NKTransportValueCurrency();

			if (Parent.ABL_TransportValue > 0 && Parent.ABL_RX_NKTransportValueCurrency.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.ABL_RX_NKTransportValueCurrencyInfo);
			}

			if (InvalidCodeErrorMessage == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ABL_RX_NKTransportValueCurrencyInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(InvalidCodeErrorMessage, Parent.ABL_RX_NKTransportValueCurrencyInfo);
			}
		}

		protected override void CheckABL_RX_NKInsuranceValueCurrency()
		{
			base.CheckABL_RX_NKInsuranceValueCurrency();

			if (Parent.ABL_InsuranceValue > 0 && Parent.ABL_RX_NKInsuranceValueCurrency.IsEmpty)
			{
				MandatoryValidation.WarnIfNotEntered(Parent.ABL_RX_NKInsuranceValueCurrencyInfo);
			}

			if (InvalidCodeErrorMessage == null)
			{
				ListValidation.ErrorIfInvalidCode(Parent.ABL_RX_NKInsuranceValueCurrencyInfo);
			}
			else
			{
				ListValidation.ErrorIfInvalidCode(InvalidCodeErrorMessage, Parent.ABL_RX_NKInsuranceValueCurrencyInfo);
			}
		}

		protected override void CheckABL_GrossWeight()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.ABL_GrossWeightInfo, Res.GetString("258967d9-b418-4cbf-a9a8-0f6d974e1c95", "Gross Mass"));
		}

		string StreetCharacterExceededErrorMessage => Res.GetString("87133478-b1b1-4e9b-8431-de87ffe1daf3", "The total characters for Street 1 and Street 2 cannot exceed 70.");

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateAtLeastOneItem();
		}

		void ValidateAtLeastOneItem()
		{
			var message = Res.GetString("c9fa49a5-c601-4bd3-acae-9b361a02e52a", "You have not entered an Item. At least one Item per Bill is required.");
			Parent.RemoveRowMessageError(message);

			if (Parent.PackedItems.Count == 0)
			{
				Parent.AddRowMessageError(message);
			}
		}
	}
}
