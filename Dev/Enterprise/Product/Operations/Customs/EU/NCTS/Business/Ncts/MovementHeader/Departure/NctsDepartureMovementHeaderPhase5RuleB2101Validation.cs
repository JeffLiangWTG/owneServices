using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	sealed class NctsDepartureMovementHeaderPhase5RuleB2101Validation
	{
		public NctsDepartureMovementHeaderPhase5RuleB2101Validation(NctsDepartureMovementHeader departureMovementHeader)
		{
			this.departureMovementHeader = Argument.NotNull(departureMovementHeader, nameof(departureMovementHeader));
		}

		internal void ValidateBM_ActiveBorderIdentificationType()
		{
			if (IsRuleB2101Applicable
				&& departureMovementHeader.BM_ActiveBorderIdentificationType.IsEmpty)
			{
				departureMovementHeader.BM_ActiveBorderIdentificationTypeInfo.AddMessageError(ValidationRuleConfiguration.Messages.B2101TypeOfIdMessage);
			}
		}

		internal void ValidateBM_TOLCarrierID()
		{
			if (IsRuleB2101Applicable
				&& departureMovementHeader.BM_TOLCarrierID.IsEmpty)
			{
				departureMovementHeader.BM_TOLCarrierIDInfo.AddMessageError(ValidationRuleConfiguration.Messages.B2101TransportIdMessage);
			}
		}

		internal void ValidateBM_RN_NKTOLCarrierNationality()
		{
			if (IsRuleB2101Applicable
				&& departureMovementHeader.BM_RN_NKTOLCarrierNationality.IsEmpty)
			{
				departureMovementHeader.BM_RN_NKTOLCarrierNationalityInfo.AddMessageError(ValidationRuleConfiguration.Messages.B2101NationalityMessage);
			}
		}

		internal void ValidateBM_CustomsOfficeAtBorder()
		{
			if (IsRuleB2101Applicable
				&& departureMovementHeader.BM_CustomsOfficeAtBorder.IsEmpty)
			{
				departureMovementHeader.BM_CustomsOfficeAtBorderInfo.AddMessageError(ValidationRuleConfiguration.Messages.B2101CustomsOfficeMessage);
			}
		}

		internal void ValidateBM_TransportAtDepartureType()
		{
			if (IsB2101ActiveOverN0002
				&& (!departureMovementHeader.BM_TransportAtDeparture.IsEmpty || !departureMovementHeader.BM_RN_NKTransportAtDepartureCountry.IsEmpty))
			{
				MandatoryValidation.MessageErrorIfNotEntered(departureMovementHeader.BM_TransportAtDepartureTypeInfo, messagePrefix: RulePrefix);
			}
		}

		internal void ValidateBM_TransportAtDepartureTrailer1RegNo()
		{
			if (IsB2101ActiveOverN0002
				&& departureMovementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport
				&& !departureMovementHeader.Trailer1NationalityAtDeparture.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(departureMovementHeader.BM_TransportAtDepartureTrailer1RegNoInfo, messagePrefix: RulePrefix);
			}
		}

		internal void ValidateBM_TransportAtDepartureTrailer2RegNo()
		{
			if (IsB2101ActiveOverN0002
				&& departureMovementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport
				&& !departureMovementHeader.Trailer2NationalityAtDeparture.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(departureMovementHeader.BM_TransportAtDepartureTrailer2RegNoInfo, messagePrefix: RulePrefix);
			}
		}

		internal void ValidateBM_TransportAtDeparture()
		{
			if (IsRuleB2101ApplicableForBM_TransportAtDeparture())
			{
				MandatoryValidation.MessageErrorIfNotEntered(departureMovementHeader.BM_TransportAtDepartureInfo, messagePrefix: RulePrefix);
			}
		}

		internal void ValidateBM_RN_NKTransportAtDepartureCountry()
		{
			if (IsRuleB2101ApplicableForBM_RN_NKTransportAtDepartureCountry())
			{
				MandatoryValidation.MessageErrorIfNotEntered(departureMovementHeader.BM_RN_NKTransportAtDepartureCountryInfo, messagePrefix: RulePrefix);
			}
		}

		internal void ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality()
		{
			if (IsB2101ActiveOverN0002
				&& departureMovementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport
				&& !departureMovementHeader.Trailer1IDAtDeparture.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(departureMovementHeader.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo, messagePrefix: RulePrefix);
			}
		}

		internal void ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality()
		{
			if (IsB2101ActiveOverN0002
				&& departureMovementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport
				&& !departureMovementHeader.Trailer2IDAtDeparture.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(departureMovementHeader.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo, messagePrefix: RulePrefix);
			}
		}

		bool IsRuleB2101ApplicableForBM_TransportAtDeparture()
		{
			var parent = departureMovementHeader;

			if (!IsB2101ActiveOverN0002
				|| (departureMovementHeader.BM_TransportAtDepartureType.IsEmpty && departureMovementHeader.BM_RN_NKTransportAtDepartureCountry.IsEmpty))
			{
				return false;
			}

			return parent.BM_InlandTransportMode.ToString() switch
			{
				ModeOfTransportList.Codes._1_SeaTransport or
				ModeOfTransportList.Codes._2_RailTransport or
				ModeOfTransportList.Codes._3_RoadTransport or
				ModeOfTransportList.Codes._4_AirTransport or
				ModeOfTransportList.Codes._7_FixedTransportInstallations or
				ModeOfTransportList.Codes._8_InlandWaterwayTransport or
				ModeOfTransportList.Codes._9_OwnPropulsion => true,
				_ => false
			};
		}

		bool IsRuleB2101ApplicableForBM_RN_NKTransportAtDepartureCountry()
		{
			var parent = departureMovementHeader;

			if (!IsB2101ActiveOverN0002
				|| (departureMovementHeader.BM_TransportAtDeparture.IsEmpty && departureMovementHeader.BM_TransportAtDepartureType.IsEmpty))
			{
				return false;
			}

			return parent.BM_InlandTransportMode.ToString() switch
			{
				ModeOfTransportList.Codes._1_SeaTransport or
				ModeOfTransportList.Codes._2_RailTransport or
				ModeOfTransportList.Codes._3_RoadTransport or
				ModeOfTransportList.Codes._4_AirTransport or
				ModeOfTransportList.Codes._7_FixedTransportInstallations or
				ModeOfTransportList.Codes._8_InlandWaterwayTransport or
				ModeOfTransportList.Codes._9_OwnPropulsion => true,
				_ => false
			};
		}

		readonly NctsDepartureMovementHeader departureMovementHeader;

		ValidationRuleConfiguration ValidationRuleConfiguration => departureMovementHeader.Header?.Configuration.ValidationRuleConfiguration;

		bool IsRuleB2101Applicable
			=> departureMovementHeader.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider departureValidationDecider
			&& departureValidationDecider.IsRuleB2101Active
			&& !departureMovementHeader.IsInPhase5TransitionPeriod;

		bool IsB2101ActiveOverN0002 => IsRuleB2101Applicable && !NctsDepartureMovementHeaderOrBillPhase5RuleN0002Validation.IsRuleN0002Applicable(departureMovementHeader);

		string RulePrefix => ValidationRuleConfiguration.Messages.B2101RuleCode.GetRuleCodeMessagePrefix(true);
	}
}
