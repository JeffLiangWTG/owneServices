using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsEuOfficeCodeValidation : EuOfficeCodeValidation
	{
		public NctsEuOfficeCodeValidation(NctsEuOfficeCode parent)
			: base(parent)
		{
		}

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();
			var parent = Parent;
			var header = parent.EffectiveHeader;
			if (header is null)
			{
				return;
			}

			if (header.IsDepartureMovement)
			{
				var propertyInfo = parent.CY_DataInfo;
				header.CheckDepartureMovementCustomsOffice(propertyInfo);

				var isOfficeDeparture = parent.IsOfficeDeparture;
				if (isOfficeDeparture || (ZString)parent.CY_CodeInfo.OriginalValue == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)
				{
					header.CheckConditionR0520(propertyInfo);

					if (isOfficeDeparture)
					{
						CheckCountryCodeSameAsCurrentCompany(header, propertyInfo);
						header.Validation.CheckDepartureCustomsOfficeAgainstConsignorAuthorisation(propertyInfo);
					}
				}
				new NctsEuOfficeCodeRuleG0034Validation(parent).ValidateRuleG0034();
			}
			else
			{
				header.DestinationTrader.Validation.ValidateOrganisationPK();
			}
		}

		void CheckCountryCodeSameAsCurrentCompany(NctsHeader nctsHeader, ZPropertyInfo info)
		{
			var departureOfficeCountryCode = Parent.CY_Data.Left(2);
			if (nctsHeader.Configuration.ValidationRuleConfiguration.IsCountryCodeRequiredToBeSameAsCurrentCompany
				&& departureOfficeCountryCode != Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(nctsHeader.CountryCode)
				&& nctsHeader.CountryCode != Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(departureOfficeCountryCode))
			{
				info.AddMessageError(NctsConstants.ValidationMessages.CountryCodeDepartureOfficeShouldBeSameOfNCTSCountryCode);
			}
		}

		protected override void CheckCY_Code()
		{
			base.CheckCY_Code();
			var parent = Parent;
			if (parent.IsOfficeDeparture || (ZString)parent.CY_CodeInfo.OriginalValue == OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture)
			{
				var header = parent.Header ?? parent.MovementHeader?.Header;
				header?.CheckConditionR0520(parent.CY_CodeInfo);
			}
		}

		protected string GetMaxCustomsOfficesOfTypeXExceededMessageError(string ruleName, int maxAmount, string codeType) => maxAmount == 1
			? Res.GetString("A6EE91A8-09A3-4D5D-91DC-6D35CA7883C4", "[{0}] The maximum number of 1 Customs Office with Purpose '{1}' has exceeded.", ruleName, codeType)
			: Res.GetString("B223E035-46C2-43F6-91FC-5D08D1461E2C", "[{0}] The maximum number of {1} Customs Offices with Purpose '{2}' has exceeded.", ruleName, maxAmount, codeType);

		protected override Dictionary<string, ValidationRule> GetCountryValidationRulesCore()
		{
			return new Dictionary<string, ValidationRule>
			{
				{ nameof(MustBeMemberOfEUOrCommonTransit), new MustBeMemberOfEUOrCommonTransit() },
				{ nameof(TIRCanOnlyGoToEU), new TIRCanOnlyGoToEU(Parent) },
				{ nameof(DestinationOfficeCannotBeInSanMarinoForT1Movements), new DestinationOfficeCannotBeInSanMarinoForT1Movements(Parent) },
			};
		}

		protected new NctsEuOfficeCode Parent => (NctsEuOfficeCode)base.Parent;
	}
}
