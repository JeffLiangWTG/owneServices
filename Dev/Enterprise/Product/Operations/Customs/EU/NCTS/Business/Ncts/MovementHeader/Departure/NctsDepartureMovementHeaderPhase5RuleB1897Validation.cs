using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.EU.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsDepartureMovementHeaderPhase5RuleB1897Validation
	{
		public NctsDepartureMovementHeaderPhase5RuleB1897Validation(NctsDepartureMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
			isRuleB1897ActiveAndInPhase5TransitionPeriod = this.movementHeader.ValidationDecider is INctsDepartureMovementHeaderPhase5ValidationDecider departureValidationDecider
				&& departureValidationDecider.IsRuleB1897Active
				&& movementHeader.IsInPhase5TransitionPeriod;
		}

		internal void ValidateBM_RN_NKTransportAtDepartureCountry()
		{
			if (!movementHeader.BM_TransportAtDeparture.IsEmpty)
			{
				if (isRuleB1897ActiveAndInPhase5TransitionPeriod)
				{
					if (movementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport
						&& !movementHeader.BM_RN_NKTransportAtDepartureCountry.IsEmpty)
					{
						AddNationalityNotRequiredMessageError(movementHeader.BM_RN_NKTransportAtDepartureCountryInfo, Res.GetString("074f5091-7a27-4d11-8bb2-3c7fd082bb62", "Train Number"));
					}
					else if (movementHeader.BM_RN_NKTransportAtDepartureCountry.IsEmpty
						&& NctsHeaderNotContainsContainerWithCNTRecord()
						&& transportModeWithNCTRecordForNationalityDictionary.ContainsKey(movementHeader.BM_InlandTransportMode))
					{
						AddYouHaveNotEnteredNationalityMessageError(movementHeader.BM_RN_NKTransportAtDepartureCountryInfo, transportModeWithNCTRecordForNationalityDictionary[movementHeader.BM_InlandTransportMode]);
					}
				}
			}
		}

		internal void ValidateBM_RN_NKTransportAtDepartureTrailer1Nationality()
		{
			if (!movementHeader.BM_TransportAtDepartureTrailer1RegNo.IsEmpty)
			{
				if (isRuleB1897ActiveAndInPhase5TransitionPeriod)
				{
					if (movementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._2_RailTransport
						&& !movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality.IsEmpty)
					{
						AddNationalityNotRequiredMessageError(movementHeader.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo, Res.GetString("1b7c5842-f0ae-4298-80d6-5d797e7636cb", "Wagon"));
					}
					else if (movementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport
						&& movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality.IsEmpty
						&& NctsHeaderNotContainsContainerWithCNTRecord())
					{
						AddYouHaveNotEnteredNationalityMessageError(movementHeader.BM_RN_NKTransportAtDepartureTrailer1NationalityInfo, Res.GetString("47b09b95-2e81-483d-a3a5-2e1d136ba8ca", "Trailer 1 ID"));
					}
				}
			}
		}

		internal void ValidateBM_RN_NKTransportAtDepartureTrailer2Nationality()
		{
			if (!movementHeader.BM_TransportAtDepartureTrailer2RegNo.IsEmpty)
			{
				if (isRuleB1897ActiveAndInPhase5TransitionPeriod)
				{
					if (movementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality.IsEmpty
						&& movementHeader.BM_InlandTransportMode == ModeOfTransportList.Codes._3_RoadTransport
						&& NctsHeaderNotContainsContainerWithCNTRecord())
					{
						AddYouHaveNotEnteredNationalityMessageError(movementHeader.BM_RN_NKTransportAtDepartureTrailer2NationalityInfo, Res.GetString("c5d1e5b5-3082-494e-b200-c697077c4afb", "Trailer 2 ID"));
					}
				}
			}
		}

		bool NctsHeaderNotContainsContainerWithCNTRecord()
		{
			var containers = movementHeader.Header.DepartureHeaderContainers;
			return containers.IsNullOrEmpty() || !containers.Any(x => x.BC_Mode.EqualsIgnoringCase(Core.Constants.ContainerModes.Containerised));
		}

		void AddNationalityNotRequiredMessageError(ZPropertyInfo propertyInfo, string transportIdName)
		{
			propertyInfo.AddMessageError(
				Res.GetString(
					"E4F40066-0D46-418E-A237-2F364070E10B",
					"{0} Nationality for {1} is not required.",
					ValidationRuleCodeConstants.B1897.GetRuleCodeMessagePrefix(),
					transportIdName));
		}

		void AddYouHaveNotEnteredNationalityMessageError(ZPropertyInfo propertyInfo, string transportIdName)
		{
			propertyInfo.AddMessageError(
				Res.GetString(
					"F27C5960-9AD3-4C69-BB1E-BF294479AA4D",
					"{0} You have not entered Nationality for {1}.",
					ValidationRuleCodeConstants.B1897.GetRuleCodeMessagePrefix(),
					transportIdName));
		}

		readonly NctsDepartureMovementHeader movementHeader;
		readonly bool isRuleB1897ActiveAndInPhase5TransitionPeriod;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Constant strings")]
		readonly Dictionary<string, string> transportModeWithNCTRecordForNationalityDictionary = new Dictionary<string, string> {
			{ ModeOfTransportList.Codes._1_SeaTransport, "Vessel" },
			{ ModeOfTransportList.Codes._3_RoadTransport, "Transport ID" },
			{ ModeOfTransportList.Codes._4_AirTransport, "Flight Number" },
			{ ModeOfTransportList.Codes._7_FixedTransportInstallations, "Transport ID" },
			{ ModeOfTransportList.Codes._8_InlandWaterwayTransport, "Vessel" },
			{ ModeOfTransportList.Codes._9_OwnPropulsion, "Transport ID" },
		};
	}
}
