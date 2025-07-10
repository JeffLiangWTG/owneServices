using CargoWise.Common;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsDepartureMovementHeaderPhase5RuleB1893_1Validation
	{
		public NctsDepartureMovementHeaderPhase5RuleB1893_1Validation(NctsDepartureMovementHeader departureMovementHeader)
		{
			this.departureMovementHeader = Argument.NotNull(departureMovementHeader, nameof(departureMovementHeader));
		}

		internal void ValidatePlaceOfLoading()
		{
			if (IsRuleB1893_1ApplicableAndTypeOfSecurityIsNON()
				&& !departureMovementHeader.BM_PlaceOfLoading.IsEmpty)
			{
				departureMovementHeader.BM_PlaceOfLoadingInfo.AddMessageError(errorMessage);
			}
		}

		internal void ValidatePortOfPresentationCode()
		{
			if (IsRuleB1893_1ApplicableAndTypeOfSecurityIsNON()
				&& !departureMovementHeader.BM_PortOfPresentationCode.IsEmpty)
			{
				departureMovementHeader.BM_PortOfPresentationCodeInfo.AddMessageError(errorMessage);
			}
		}

		internal bool IsRuleB1893_1ApplicableAndTypeOfSecurityIsNON()
		{
			return (ValidationDecider?.IsRuleB1893_1Active ?? false)
				&& IsInPhase5TransitionPeriod
				&& departureMovementHeader.BM_TypeOfSecurity == NctsTypeOfSecurityList.Codes.NON;
		}

		readonly NctsDepartureMovementHeader departureMovementHeader;

		bool IsInPhase5TransitionPeriod => departureMovementHeader.IsInPhase5TransitionPeriod;

		INctsDepartureMovementHeaderPhase5ValidationDecider ValidationDecider => (INctsDepartureMovementHeaderPhase5ValidationDecider)departureMovementHeader.ValidationDecider;

		static string errorMessage => Res.GetString("6B16C033-D41C-4CDC-B9F8-0972D153A82B", "[B1893-1] Place of Loading must be empty if Security field = NON.");
	}
}
