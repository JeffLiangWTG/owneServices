using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class CusGoodsLocationAddressValidation : EU.Business.CusGoodsLocationAddressValidation
	{
		public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent)
			: base(parent)
		{
		}

		new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

		CusGoodsLocation GoodsLocation => Parent.GoodsLocation;

		protected override void CheckE2_GovRegNum()
		{
			base.CheckE2_GovRegNum();
			var parent = Parent;
			if (parent.E2_GovRegNum.IsEmpty && GoodsLocation is CusGoodsLocation goodsLocation)
			{
				var qualifier = goodsLocation.CGL_Qualifier;
				if (qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber || qualifier == CusGoodsLocationQualifierList.Codes.EoriNumber)
				{
					if (IsRuleC0394Active)
					{
						var messageError = CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(parent.E2_GovRegNumInfo.HumanReadableName, qualifier, RuleC0394Code);
						parent.E2_GovRegNumInfo.AddMessageError(messageError);
					}
				}
			}

			CheckAuthorizationNumberRuleNR0022();
		}

		protected override void CheckE2_Address1AndE2_Address2()
		{
			base.CheckE2_Address1AndE2_Address2();
			var parent = Parent;
			if (parent.E2_Address1AndE2_Address2.IsEmpty
				&& GoodsLocation is CusGoodsLocation goodsLocation
				&& IsRuleC0394Active
				&& goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address)
			{
				var messageError = CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(parent.E2_Address1AndE2_Address2Info.HumanReadableName, goodsLocation.CGL_Qualifier, RuleC0394Code);
				parent.E2_Address1AndE2_Address2Info.AddMessageError(messageError);
			}
			CheckAddress1AndAddress2RuleE1104_1();
		}

		protected override void CheckE2_City()
		{
			base.CheckE2_City();
			var parent = Parent;
			if (parent.E2_City.IsEmpty
				&& GoodsLocation is CusGoodsLocation goodsLocation
				&& IsRuleC0394Active
				&& goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address)
			{
				var messageError = CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(parent.E2_CityInfo.HumanReadableName, goodsLocation.CGL_Qualifier, RuleC0394Code);
				parent.E2_CityInfo.AddMessageError(messageError);
			}
		}

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();
			var parent = Parent;
			if (parent.E2_RN_NKCountryCode.IsEmpty
				&& GoodsLocation is CusGoodsLocation goodsLocation
				&& IsRuleC0394Active)
			{
				var qualifier = goodsLocation.CGL_Qualifier;
				if (qualifier == CusGoodsLocationQualifierList.Codes.Address || qualifier == CusGoodsLocationQualifierList.Codes.PostcodeAddress)
				{
					var messageError = CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(parent.E2_RN_NKCountryCodeInfo.HumanReadableName, goodsLocation.CGL_Qualifier, RuleC0394Code);
					parent.E2_RN_NKCountryCodeInfo.AddMessageError(messageError);
				}
			}
		}

		protected override void CheckE2_Postcode()
		{
			base.CheckE2_Postcode();
			var parent = Parent;
			var postcodeMaxLength = NctsConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.PostCode;

			if (GoodsLocation is CusGoodsLocation goodsLocation)
			{
				var postcode = parent.E2_Postcode;
				var qualifier = goodsLocation.CGL_Qualifier;
				var postcodeInfo = parent.E2_PostcodeInfo;

				if (postcode.IsEmpty && goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.PostcodeAddress && IsRuleC0394Active)
				{
					postcodeInfo.AddMessageError(CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(postcodeInfo.HumanReadableName, qualifier, RuleC0394Code));
				}
				if (qualifier == CusGoodsLocationQualifierList.Codes.Address)
				{
					var isInPhase5TransitionPeriod = IsInPhase5TransitionPeriod;
					if (postcode.Length > postcodeMaxLength && IsPhase5Departure && isInPhase5TransitionPeriod)
					{
						if (IsRuleE1102Active)
						{
							postcodeInfo.AddMessageError(Res.GetString("cd4a2dad-de51-4016-bd87-dd7359f0dacf", "[E1102] Postcode should be less than or equal to {0} Char.", postcodeMaxLength));
						}
						if (IsRuleE1102_1Active)
						{
							postcodeInfo.AddWarning(Res.GetString("76BBC585-C582-4D3B-8194-185FE2DD25C7", "Postcode is longer than 9 characters, it will be truncated in the message"));
						}
					}

					if ((Parent.Country?.RN_PostcodeValidationRule ?? ZString.Empty) == CountryAddressValidationRuleList.Codes.MustBeEntered && IsRuleC0505Active)
					{
						MandatoryValidation.MessageErrorIfNotEntered(postcodeInfo, messagePrefix: NctsConstants.ValidationRuleMessagePrefixes.C0505);
					}
				}
			}
		}

		protected override void CheckE2_Latitude()
		{
			var parent = Parent;
			if (parent.E2_Latitude.IsEmpty
				&& GoodsLocation is CusGoodsLocation goodsLocation
				&& IsRuleC0394Active
				&& goodsLocation.CGL_Qualifier.EqualsIgnoringCase(CusGoodsLocationQualifierList.Codes.GnssCoordinates))
			{
				var messageError = CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(Parent.E2_LatitudeInfo.HumanReadableName, GoodsLocation.CGL_Qualifier, RuleC0394Code);
				parent.E2_LatitudeInfo.AddMessageError(messageError);
			}
		}

		protected override void CheckE2_Longitude()
		{
			var parent = Parent;
			if (parent.E2_Longitude.IsEmpty
				&& GoodsLocation is CusGoodsLocation goodsLocation
				&& IsRuleC0394Active
				&& goodsLocation.CGL_Qualifier.EqualsIgnoringCase(CusGoodsLocationQualifierList.Codes.GnssCoordinates))
			{
				var messageError = CusGoodsLocationValidationHelper.YouHaveNotEnteredMessageWithQualifier(Parent.E2_LongitudeInfo.HumanReadableName, GoodsLocation.CGL_Qualifier, RuleC0394Code);
				parent.E2_LongitudeInfo.AddMessageError(messageError);
			}
		}

		protected virtual string RuleC0394Code => ValidationRuleCodeConstants.C0394;

		protected override bool ApplyC0065Rule => GoodsLocation.Header?.Configuration.ValidationRuleConfiguration.IsRuleC0065Active ?? false;

		void CheckAddress1AndAddress2RuleE1104_1()
		{
			var parent = Parent;
			var goodsLocation = GoodsLocation;

			var addressMaxLength = IsInPhase5TransitionPeriod
				? NctsConstants.CustomsFieldMaxLength.TransitionPeriod.Trader.Address
				: NctsConstants.CustomsFieldMaxLength.Trader.Address;

			if ((goodsLocation?.Header?.Configuration.ValidationRuleConfiguration.IsRuleE1104_1Active ?? false)
				&& IsPhase5Departure
				&& parent.E2_Address1AndE2_Address2.Length > addressMaxLength
				&& goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.Address)
			{
				var addressInfo = parent.E2_Address1AndE2_Address2Info;
				addressInfo.AddWarning(GetFieldExceedsCustomsMaxLengthExceesWillBeTruncateCaption(addressInfo, addressMaxLength, ValidationRuleCodeConstants.E1104_1.GetRuleCodeMessagePrefix(true)));
			}
		}

		string GetFieldExceedsCustomsMaxLengthExceesWillBeTruncateCaption(ZPropertyInfo propertyInfo, int customsMaxLength, string ruleCode)
			=> GetFieldExceedsCustomsMaxLengthExceesWillBeTruncateCaption(propertyInfo?.HumanReadableName ?? ZString.Empty, customsMaxLength, ruleCode);

		string GetFieldExceedsCustomsMaxLengthExceesWillBeTruncateCaption(string fieldName, int customsMaxLength, string ruleCode)
			=> Res.GetString("F551E9BF-B1A2-4EC4-95D0-22946DB21A50", "{0}{1} exceeds the maximum allowed length in the declaration message ({2} characters). Excess characters will be truncated.", ruleCode, fieldName, customsMaxLength);

		void CheckAuthorizationNumberRuleNR0022()
		{
			var goodsLocation = GoodsLocation;
			if (goodsLocation is null)
			{
				return;
			}

			var parent = Parent;
			if (goodsLocation.CGL_Qualifier == CusGoodsLocationQualifierList.Codes.AuthorizationNumber
				&& IsPhase5Departure
				&& IsRuleNR0022Active)
			{
				var authorizationLength = parent.AuthorisationNumber.Length;
				var placeCodeLength = goodsLocation.CGL_AdditionalIdentifier.Length;
				const int authorisationNumberAndPlaceCodeMaxLength = 34;

				if ((authorizationLength + placeCodeLength) > authorisationNumberAndPlaceCodeMaxLength)
				{
					var messageError = Res.GetString("E574BDD4-8548-443B-B727-F69DD2381BA1", "{0} The sum of [(Authorization Number) + (Place Code) must be less than or equal to {1} char.", ValidationRuleCodeConstants.NR0022.GetRuleCodeMessagePrefix(), authorisationNumberAndPlaceCodeMaxLength);
					parent.E2_GovRegNumInfo.AddMessageError(messageError);
				}
			}
		}

		bool IsRuleC0394Active => ValidationDecider is IArrivalPhase5CusGoodsLocationValidationDecider { IsRuleC0394Active: true } || ValidationDecider is IDeparturePhase5CusGoodsLocationValidationDecider { IsRuleC0394Active: true };

		bool IsRuleE1102Active => GoodsLocation.Header?.Configuration.ValidationRuleConfiguration.IsRuleE1102Active ?? true;

		bool IsRuleC0505Active => GoodsLocation.Header?.Configuration.ValidationRuleConfiguration.IsRuleC0505Active ?? false;

		bool IsRuleE1102_1Active => GoodsLocation.Header?.Configuration.ValidationRuleConfiguration.IsRuleE1102_1Active ?? false;

		bool IsRuleNR0022Active => GoodsLocation.Header?.Configuration.ValidationRuleConfiguration.IsRuleNR0022Active ?? false;

		bool IsPhase5Departure => GoodsLocation.Header?.IsPhase5Departure ?? false;

		bool IsInPhase5TransitionPeriod => GoodsLocation.Header?.IsInPhase5TransitionPeriod ?? false;

		public ICusGoodsLocationValidationDecider ValidationDecider => Parent.Factory.GetValue(ref validationDeciderCached, GetValidationDecider);
		CachedProperty<ICusGoodsLocationValidationDecider> validationDeciderCached;

		ICusGoodsLocationValidationDecider GetValidationDecider()
		{
			return Parent.GoodsLocation.Parent is ICusGoodsLocationProviderWithValidationDecider provider ? provider.GoodsLocationValidationDecider : null;
		}
	}
}
