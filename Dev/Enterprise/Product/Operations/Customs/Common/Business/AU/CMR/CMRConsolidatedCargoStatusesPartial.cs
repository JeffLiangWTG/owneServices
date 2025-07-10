using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common.AU.CMR
{
	public partial class CMRConsolidatedCargoStatuses : ICustomsStatusReasonHelper
	{
		public static class Filter
		{
			public static class Codes
			{
				public const string NotClear = "NCL";
				public const string HeldOrConditional = "HOC";
				public const string ClearOrConditional = "COC";
			}

			public static class Descriptions
			{
				public static MultilingualString NotClear => ResString.GetMultilingualString("9BB3957E-6296-4C73-ACA9-4CF3A316391B", "Not Clear");
				public static MultilingualString HeldOrConditional => ResString.GetMultilingualString("19D3EF48-C4E3-4909-8A21-0E82CDD4F7DD", "Held or Conditional Clear");
				public static MultilingualString ClearOrConditional => ResString.GetMultilingualString("A05D6869-185B-43AA-BF47-805F60B8381A", "Clear or Conditional Clear");
			}
		}

		public static class ShortDescriptions
		{
			public static MultilingualString Acsseized => ResString.GetMultilingualString("4AAA23DA-5724-426E-BFD8-1B5E70DE96EB", "ACSSEIZED");
			public static MultilingualString Aqisseized => ResString.GetMultilingualString("0A39CACE-D037-4679-AB9D-27B1B1681CC6", "AQISSEIZED");
			public static MultilingualString Clear => ResString.GetMultilingualString("6A81BFA4-C1B4-49C9-BD93-89C14087167D", "CLEAR");
			public static MultilingualString Clearhrm => ResString.GetMultilingualString("CE2C4B07-B7D9-4B79-BD27-5B9CD464993F", "CLEARHRM");
			public static MultilingualString Condclear => ResString.GetMultilingualString("C77F2E3F-5EF5-498D-822A-3611208A5D74", "CONDCLEAR");
			public static MultilingualString Dclallowed => ResString.GetMultilingualString("5AB5DE21-E8BF-4442-9328-57788B70E122", "DCLALLOWED");
			public static MultilingualString Held => ResString.GetMultilingualString("809903B9-3CF2-4F2F-B612-CCECD46B33F3", "HELD");
			public static MultilingualString SeePackingDetails => ResString.GetMultilingualString("602C3715-D832-4DEF-BCDC-DA9FBFCAC24F", "SEE PACKING DETAILS");
			public static MultilingualString Sububmov => ResString.GetMultilingualString("EBEB3058-D541-413C-BA3C-692D3BACF91F", "SUBUBMOV");
			public static MultilingualString Tranship => ResString.GetMultilingualString("91C871D4-B67B-45E0-94B6-DFA205ABB7A9", "TRANSHIP");
			public static MultilingualString Transhphrm => ResString.GetMultilingualString("ED7CD15A-510C-46A6-A9C6-58411DA7B1B2", "TRANSHPHRM");
			public static MultilingualString Transit => ResString.GetMultilingualString("932BD2CE-CF94-4C5C-8A60-6CFC49B6B464", "TRANSIT");
			public static MultilingualString Withdrawn => ResString.GetMultilingualString("6872C36A-13C0-4E85-B713-77D2317D56EF", "WITHDRAWN");
		}

		public static ZString GetShortDescriptionFromCode(ZString statusCode)
		{
			switch (statusCode)
			{
				case Codes.AcsseizedCargoIsSeizedByCustoms:
					return ShortDescriptions.Acsseized;
				case Codes.AqisseizedCargoIsSeizedByQuarantine:
					return ShortDescriptions.Aqisseized;
				case Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased:
					return ShortDescriptions.Clear;
				case Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement:
					return ShortDescriptions.Clearhrm;
				case Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation:
					return ShortDescriptions.Condclear;
				case Codes.DclallowedCargoMayBeDeconsolidatedThisStatusValueOnlyAppliesToAirCargo:
					return ShortDescriptions.Dclallowed;
				case Codes.HeldCargoIsHeldUnderCustomsControl:
					return ShortDescriptions.Held;
				case Codes.SeePackingDetails:
					return ShortDescriptions.SeePackingDetails;
				case Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed:
					return ShortDescriptions.Sububmov;
				case Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus:
					return ShortDescriptions.Tranship;
				case Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement:
					return ShortDescriptions.Transhphrm;
				case Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction:
					return ShortDescriptions.Transit;
				case Codes.WithdrawnCargoReportHadBeenWithdrawn:
					return ShortDescriptions.Withdrawn;
				default:
					return "";
			}
		}

		public static bool IsCargoAbbreviatedStatusDescriptionRequiredFor(ZString cargoStatusCode)
		{
			return !NotRequiredList.Contains(cargoStatusCode);
		}
		static readonly ImmutableList<string> NotRequiredList = ImmutableList.Create(new string[] {
			Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased,
			Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement,
			Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation,
			Codes.WithdrawnCargoReportHadBeenWithdrawn,
			Codes.SeePackingDetails });

		public static bool IsCargoStatusClearFor(ZString cargoStatusCode)
		{
			return ClearList.Contains(cargoStatusCode);
		}

		static readonly ImmutableList<string> ClearList = ImmutableList.Create(new string[] {
			Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased,
			Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement,
			Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation });

		public static CodeDescriptionPairList AllClearStatus
		{
			get
			{
				CodeDescriptionPairList result = new CodeDescriptionPairList();
				result.AddPair(Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, Descriptions.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased);
				result.AddPair(Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement, Descriptions.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement);
				result.AddPair(Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation, Descriptions.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);
				result.AddPair(CMRBaseStatuses.Codes.OriginalAccepted, CMRBaseStatuses.Descriptions.OriginalAccepted);
				return result;
			}
		}

		public static CodeDescriptionPairList AllFilterStatuses
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Filter.Codes.NotClear, Filter.Descriptions.NotClear);
				result.AddPair(Filter.Codes.HeldOrConditional, Filter.Descriptions.HeldOrConditional);
				result.AddPair(Filter.Codes.ClearOrConditional, Filter.Descriptions.ClearOrConditional);
				result.AddRange(new CMRConsolidatedCargoStatuses());
				return result;
			}
		}

		public static CodeDescriptionPairList SeaFilterStatuses
		{
			get
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Filter.Codes.NotClear, Filter.Descriptions.NotClear);
				result.AddRange(new CMRConsolidatedCargoStatuses());
				return result;
			}
		}

		static readonly Dictionary<string, string> AUCustomsStatusReasonToGenericCustomsStatusCodeMapping = new Dictionary<string, string>()
		{
			{ CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased, CustomsStatusCodeList.Codes.Cleared },
			{ CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation, CustomsStatusCodeList.Codes.Cleared },
			{ CMRConsolidatedCargoStatuses.Codes.ClearhrmCargoIsClearButIsIdentifiedAsHighRiskMovement, CustomsStatusCodeList.Codes.Cleared },
			{ CMRConsolidatedCargoStatuses.Codes.DclallowedCargoMayBeDeconsolidatedThisStatusValueOnlyAppliesToAirCargo, CustomsStatusCodeList.Codes.Held },
			{ CMRConsolidatedCargoStatuses.Codes.SububmovCargoCannotBeReleasedIntoHomeConsumptionButUnderbondMovementIsAllowed, CustomsStatusCodeList.Codes.Held },
			{ CMRConsolidatedCargoStatuses.Codes.TranshipCargoIsForTranshipmentATranshipmentNumberWillBeGeneratedAndTransmittedWithStatus, CustomsStatusCodeList.Codes.Cleared },
			{ CMRConsolidatedCargoStatuses.Codes.TransitCargoIsTransitCargoThisValueWillOnlyBeViewableFromAnInteractiveFunction, CustomsStatusCodeList.Codes.Cleared },
			{ CMRConsolidatedCargoStatuses.Codes.TranshphrmTranshipmentCargoIsClearButIsIdentifiedAsHighRiskMovement, CustomsStatusCodeList.Codes.Cleared },
			{ CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl, CustomsStatusCodeList.Codes.Held },
			{ CMRConsolidatedCargoStatuses.Codes.AcsseizedCargoIsSeizedByCustoms, CustomsStatusCodeList.Codes.Held },
			{ CMRConsolidatedCargoStatuses.Codes.AqisseizedCargoIsSeizedByQuarantine, CustomsStatusCodeList.Codes.Held },
			{ CMRConsolidatedCargoStatuses.Codes.WithdrawnCargoReportHadBeenWithdrawn, CustomsStatusCodeList.Codes.Withdrawn },
		};

		public ZString ConvertCustomsStatusReasonCodeToGenericCustomsStatus(ZString reasonCode)
		{
			return AUCustomsStatusReasonToGenericCustomsStatusCodeMapping.TryGetValue(reasonCode, out var convertedCode) ? convertedCode : string.Empty;
		}

		public ZString GetCustomsStatusReasonDescription(ZString reasonCode)
		{
			var codeDescriptionPairList = new CMRConsolidatedCargoStatuses();
			return codeDescriptionPairList.GetDescriptionFromCode(reasonCode) ?? string.Empty;
		}
	}
}
