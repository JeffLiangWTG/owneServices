using CargoWise.Common;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsDepartureMovementHeaderPhase5RuleC0710Validation
	{
		public NctsDepartureMovementHeaderPhase5RuleC0710Validation(NctsDepartureMovementHeader departureMovementHeader)
		{
			this.departureMovementHeader = Argument.NotNull(departureMovementHeader, nameof(departureMovementHeader));
		}

		public void Validate()
		{
			var header = departureMovementHeader.Header;
			var goodsLocationDescriptionInfo = departureMovementHeader.GoodsLocationDescriptionInfo;

			if (departureMovementHeader.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider { IsRuleC0710Active: true }
				&& !departureMovementHeader.IsInPhase5TransitionPeriod // TODO: implement rule B1804
				&& departureMovementHeader.GoodsLocationDescription.IsEmpty
				&& departureMovementHeader.BM_AdditionalDeclarationType != NctsTypeOfAdditionalDeclarationList.Codes.D
				&& !IsOfficeCountryConsideredInEuForSafetyAndSecurity())
			{
				goodsLocationDescriptionInfo.AddMessageError(YouHaveNotEnteredGoodsLocationMessage);
			}
		}

		bool IsOfficeCountryConsideredInEuForSafetyAndSecurity()
		{
			return departureMovementHeader.DepartureCustomsOffice is NctsEuOfficeCode officeCode
				&& officeCode.IsOfficeCountryConsideredInEuForSafetyAndSecurity;
		}

		string YouHaveNotEnteredGoodsLocationMessage => Res.GetString("23D4167D-3931-41A1-8147-9C6CC4713BB3", "[C0710] You have not entered a Location of Goods");

		readonly NctsDepartureMovementHeader departureMovementHeader;
	}
}
