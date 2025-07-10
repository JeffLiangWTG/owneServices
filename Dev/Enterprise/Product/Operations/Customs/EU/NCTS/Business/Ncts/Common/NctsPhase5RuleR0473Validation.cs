using System.Collections.Immutable;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Customs.EU.Business.ModeOfTransportList.Codes;
using static Enterprise.Customs.EU.NCTS.Business.NctsConstants;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public sealed class NctsPhase5RuleR0473Validation
	{
		public NctsPhase5RuleR0473Validation(NctsDepartureMovementHeader movementHeader)
		{
			this.movementHeader = Argument.NotNull(movementHeader, nameof(movementHeader));
		}

		internal void CheckTransportAtDeparture(ZString transportType, ZPropertyInfo propertyInfo)
		{
			var transportMode = movementHeader.BM_InlandTransportMode;
			CheckTransportCharacterCasingForTransportMode(transportMode, transportType, propertyInfo);
		}

		public void CheckTransportAtBorder(ZString transportType, ZPropertyInfo propertyInfo)
		{
			var transportMode = movementHeader.BM_ExportTransportMode;
			CheckTransportCharacterCasingForTransportMode(transportMode, transportType, propertyInfo);
		}

		void CheckTransportCharacterCasingForTransportMode(ZString transportMode, ZString transportType, ZPropertyInfo propertyInfo)
		{
			var validationDecider = movementHeader.ValidationDecider as INctsDepartureMovementHeaderPhase5ValidationDecider;
			if (validationDecider is null)
			{
				return;
			}

			var isRuleR0473Active = validationDecider.IsRuleR0473Active && !movementHeader.IsInPhase5TransitionPeriod;
			if (isRuleR0473Active || validationDecider.IsRuleR0473_1Active)
			{
				var rulePrefix = isRuleR0473Active ? ValidationRuleMessagePrefixes.R0473 : ValidationRuleMessagePrefixes.R0473_1;

				if ((transportMode == _1_SeaTransport && transportType == NctsTransportTypeOfIdList.Codes._10)
					|| transportMode == _2_RailTransport
					|| transportMode == _3_RoadTransport
					|| transportMode == _4_AirTransport
					|| (transportMode == _7_FixedTransportInstallations && transportType.In(ruleR0473TypeOfTransportIdsForOwnPropulsionAndFixedTransportInstallations))
					|| (transportMode == _8_InlandWaterwayTransport && transportType == NctsTransportTypeOfIdList.Codes._80)
					|| (transportMode == _9_OwnPropulsion && transportType.In(ruleR0473TypeOfTransportIdsForOwnPropulsionAndFixedTransportInstallations)))
				{
					UniversalValidationHelper.CheckNoLowerCaseLetters(propertyInfo, rulePrefix);
				}
			}
		}
		#region Implementation

		readonly NctsDepartureMovementHeader movementHeader;

		readonly ImmutableArray<ZString> ruleR0473TypeOfTransportIdsForOwnPropulsionAndFixedTransportInstallations = new ZString[]
		{
			NctsTransportTypeOfIdList.Codes._10,
			NctsTransportTypeOfIdList.Codes._20,
			NctsTransportTypeOfIdList.Codes._21,
			NctsTransportTypeOfIdList.Codes._30,
			NctsTransportTypeOfIdList.Codes._31,
			NctsTransportTypeOfIdList.Codes._40,
			NctsTransportTypeOfIdList.Codes._41,
			NctsTransportTypeOfIdList.Codes._80,
			NctsTransportTypeOfIdList.Codes._99
		}.ToImmutableArray();

		#endregion
	}
}
