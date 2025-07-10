using CargoWise.EntityFramework;
using ManifestValidationRuleCodes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeList.ManifestValidationRuleCodes;

namespace Enterprise.Customs.ASYCUDA.Business
{
	public partial class AsycudaBillValidationForRegularBill
	{
		protected override void CheckCustomsEntryNumberType()
		{
			base.CheckCustomsEntryNumberType();
			var cusEntryNumber = Parent.CusEntryNumber;
			if (cusEntryNumber != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.CustomsEntryNumberTypeInfo);
				cusEntryNumber.Validation.ValidateCE_EntryType();
				Parent.CustomsEntryNumberTypeInfo.AddAllNotificationsFrom(cusEntryNumber.CE_EntryTypeInfo);
			}
		}

		protected override void CheckCustomsEntryNumber()
		{
			base.CheckCustomsEntryNumber();
			var cusEntryNumber = Parent.CusEntryNumber;
			if (cusEntryNumber != null)
			{
				cusEntryNumber.Validation.ValidateCE_EntryNum();
				Parent.CustomsEntryNumberTypeInfo.AddAllNotificationsFrom(cusEntryNumber.CE_EntryNumInfo);
			}
		}

		protected override void CheckRegistrationDate()
		{
			var registrationEntryNumber = Parent.RegistrationEntryNumber;
			if (registrationEntryNumber != null)
			{
				registrationEntryNumber.Validation.ValidateCE_IssueDate();
				Parent.RegistrationDateInfo.AddAllNotificationsFrom(registrationEntryNumber.CE_IssueDateInfo);
			}
		}

		protected override void CheckABL_ShipmentType()
		{
			base.CheckABL_ShipmentType();

			ZZValidationHeaderHelper?.CheckIsMandatoryForOneCountryWhenTransportModeMatches(Parent.ABL_ShipmentTypeInfo, Parent.CountryCode, ManifestValidationRuleCodes.ShipmentType, Parent.ABL_Calc_AMA_TransportMode);
			ListValidation.MessageErrorIfInvalidCode(Parent.ABL_ShipmentTypeInfo);
		}

		protected override void CheckABL_BillIssuer()
		{
			base.CheckABL_BillIssuer();

			if (Parent.ABL_BillIssuer.IsEmpty)
			{
				var helper = ZZValidationHeaderHelper;
				if (helper != null)
				{
					var header = Parent.Header;
					var manifestType = header.AMA_ManifestType;
					helper.CheckIsMandatoryWhenAttributeMatches(Parent.ABL_BillIssuerInfo,
						ManifestValidationRuleCodes.BillIssuer,
						ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, manifestType);

					helper.CheckIsMandatoryWhenAttributeMatches(Parent.ABL_BillIssuerInfo,
						ManifestValidationRuleCodes.BillIssuer,
						ManifestValidationRuleCodes.MandatoryForManifestType, manifestType);
				}
			}
			else
			{
				// List validation?  Only if we have some data for that country
				if (Parent.ZZCarrierLoader.AnyRecordsForCountry(Parent.CountryCode))
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.ABL_BillIssuerInfo);
				}
			}
		}

		protected override void CheckABL_GoodsLocation()
		{
			base.CheckABL_GoodsLocation();
			if (Parent.ABL_GoodsLocation.IsEmpty)
			{
				ZZValidationHeaderHelper?.CheckIsMandatoryFor(
					Parent.ABL_GoodsLocationInfo,
					new[]
					{
						new ZZDatabaseValidationHelper.ManifestValidationRule(ManifestValidationRuleCodes.GoodsLocation, ManifestValidationRuleCodes.MANDATORYFORNATURE, header => header.AMA_NatureInfo),
						new ZZDatabaseValidationHelper.ManifestValidationRule(ManifestValidationRuleCodes.GoodsLocation, ManifestValidationRuleCodes.MANDATORYFORMESSAGETYPE, header => header.AMA_ManifestTypeInfo)
					});
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ABL_GoodsLocationInfo);
			}
		}
	}
}
