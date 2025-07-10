using CargoWise.EntityFramework;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS
{
	public class CountryOfRoutingValidation : Customs.Business.CusCodeDataValidation
	{
		public CountryOfRoutingValidation(CountryOfRouting parent) : base(parent)
		{
		}

		new CountryOfRouting Parent => (CountryOfRouting)base.Parent;

		protected override void CheckCY_Data()
		{
			base.CheckCY_Data();

			var parent = Parent;
			var propertyInfo = parent.CY_DataInfo;
			var countryOfRouting = parent.CY_Data;
			var valueNotEnteredPrefix = string.Empty;

			if (parent.Parent is NctsHeader header
				&& header.MovementHeader is NctsDepartureMovementHeader departureMovementHeader
				&& header.Configuration is NctsConfiguration configuration
				&& configuration.ValidationRuleConfiguration is ValidationRuleConfiguration ruleConfiguration
				&& Parent.ValidationDecider is ICountryOfRoutingDeparturePhase5ValidationDecider departurePhase5ValidationDecider)
			{
				if (departurePhase5ValidationDecider.IsRuleC0586Active && !header.IsInPhase5TransitionPeriod && departureMovementHeader.IsSecurityTypeENTOrBTHOrEXI)
				{
					valueNotEnteredPrefix = ruleConfiguration.Messages.C0586RuleCode.GetRuleCodeMessagePrefix(true);
				}

				if (!countryOfRouting.IsEmpty)
				{
					CheckConditionC0030(header);
				}
			}

			MandatoryValidation.MessageErrorIfNotEntered(propertyInfo, messagePrefix: valueNotEnteredPrefix);

			ListValidation.MessageErrorIfInvalidCode(propertyInfo);

			void CheckConditionC0030(NctsHeader header)
			{
				if (departurePhase5ValidationDecider.IsRuleC0030Active
					&& !(departurePhase5ValidationDecider.IsRuleB1836Active && header.IsInPhase5TransitionPeriod)
					&& Parent.Factory.GetCountryCodesCTC().ContainsCode(countryOfRouting)
					&& !departureMovementHeader.HasTransitOffice())
				{
					propertyInfo.AddMessageError(NctsConstants.ValidationMessages.YouHaveNotEnteredACustomsOfficeOfTransitDeclared);
				}
			}
		}
	}
}
