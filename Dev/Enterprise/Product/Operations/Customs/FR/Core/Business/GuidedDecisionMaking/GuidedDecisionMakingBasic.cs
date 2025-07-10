using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.FR.Business.GDM
{
	public class GuidedDecisionMakingBasic : EU.Business.GuidedDecisionMakingBasic
	{
		public GuidedDecisionMakingBasic(IGuidedDecisionMakingSource source, BusinessObjectFactory factory) : base(source, factory)
		{
			RegionOrTerritoryOfDestination = source.RegionOrTerritoryOfDestination;
		}

		public static new class Schema
		{
			public const string RegionOrTerritoryOfDestination = "RegionOrTerritoryOfDestination";
			public const int RegionOrTerritoryOfDestinationaxLength = 5;
		}

		protected override ITariffAdditionalCodeSelectionCriteria GetNewTariffAdditionalCodeSelectionCriteria()
		{
			var direction = IsImport ? UniversalReferenceConstants.RefCusTariffAdditionalCodeCategories.SIP : UniversalReferenceConstants.RefCusTariffAdditionalCodeCategories.SEP;
			return new TariffAdditionalCodeSelectionCriteria(direction, EffectiveDate, EffectiveTradeGroupCountry, DataGrouping);
		}

		[List(nameof(Lookups) + "." + nameof(GuidedDecisionMakingBasicLookups.RegionOrTerritoryOfDestinationList))]
		[ResourceStringData("cf0f2325-eea3-42be-898f-b791f6bce0f5", Caption = "Region")]
		[MaxLength(Schema.RegionOrTerritoryOfDestinationaxLength)]
		public ZString RegionOrTerritoryOfDestination
		{
			get => fRegionOrTerritoryOfDestination;
			set
			{
				CheckMaximumLength(RegionOrTerritoryOfDestinationInfo, value);
				SetNonPersistentPropertyValue(RegionOrTerritoryOfDestinationInfo, ref fRegionOrTerritoryOfDestination, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateRegionOrTerritoryOfDestination();
				}
				RegionOrTerritoryOfDestinationInfo.RefreshBinding();
			}
		}
		ZString fRegionOrTerritoryOfDestination;

		public ZPropertyInfo RegionOrTerritoryOfDestinationInfo => GetZPropertyInfo(nameof(RegionOrTerritoryOfDestination));

		public override EU.Business.GuidedDecisionMakingBasicLookups GetNewLookups() => new GuidedDecisionMakingBasicLookups(this);

		public new GuidedDecisionMakingBasicLookups Lookups => (GuidedDecisionMakingBasicLookups)base.Lookups;

		protected override EU.Business.GuidedDecisionMakingBasicValidation GetNewValidation() => new GuidedDecisionMakingBasicValidation(this);

		public new GuidedDecisionMakingBasicValidation Validation => (GuidedDecisionMakingBasicValidation)base.Validation;

		protected override IValueSetStrategy GetNewValueSetStrategy() => new GuidedDecisionMakingBasicValueSetStrategy(this);

		protected override IZZRateSelectionCriteria GetNewRateSelectionCriteria() => new SpecificRateSelectionCriteria(CountryOfOrigin, DataGrouping, Preference, QuotaOrderNumber, SelectedSupplementaryCodes.ToHashSet(), EffectiveDate, ZString.Empty, ZString.Empty, GetSecondaryTradeGroup(), IsExport ? RateDirection.Export : RateDirection.Import);

		protected override IZZRateSelectionCriteria GetNewRateSelectionCriteriaWithoutAdditionalCodes() => new SpecificRateSelectionCriteria(EffectiveTradeGroupCountry, DataGrouping, Preference, QuotaOrderNumber, new HashSet<ZString>(), EffectiveDate, ZString.Empty, ZString.Empty, GetSecondaryTradeGroup(), IsExport ? RateDirection.Export : RateDirection.Import);

		protected override IZZConditionSelectionCriteria GetNewConditionSelectionCriteria()
		{
			var direction = IsImport ? ConditionChecker.ConditionDirection.Import : IsExport ? ConditionChecker.ConditionDirection.Export : ConditionChecker.ConditionDirection.Either;
			return new ZZConditionSelectionCriteria(EffectiveDate, EffectiveTradeGroupCountry, Preference, SelectedSupplementaryCodes.ToHashSet(), QuotaOrderNumber, DataGrouping, direction, ZString.Empty, ZString.Empty, GetSecondaryTradeGroup());
		}

		protected override IZZConditionSelectionCriteria GetNewConditionSelectionCriteriaWithoutAdditionalCodes()
		{
			var direction = IsImport ? ConditionChecker.ConditionDirection.Import : IsExport ? ConditionChecker.ConditionDirection.Export : ConditionChecker.ConditionDirection.Either;
			return new ZZConditionSelectionCriteria(EffectiveDate, EffectiveTradeGroupCountry, Preference, new HashSet<ZString>(), QuotaOrderNumber, DataGrouping, direction, ZString.Empty, ZString.Empty, GetSecondaryTradeGroup());
		}

		protected override EU.Business.GuidedDecisionMakingVATCollection GetNewGuidedDecisionMakingVATCollectionCore() => new GuidedDecisionMakingVATCollection(this);

		internal HashSet<ZString> GetSecondaryTradeGroup() => !RegionOrTerritoryOfDestination.IsEmpty ? FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined(RegionOrTerritoryOfDestination).ToHashSet() : new HashSet<ZString>();

		protected override void TransferToTargetCore(EU.Business.IGuidedDecisionMakingTarget target)
		{
			base.TransferToTargetCore(target);
			((IGuidedDecisionMakingTarget)target).RegionOrTerritoryOfDestination = RegionOrTerritoryOfDestination;
		}

		public override ZString PreferredLanguage => Core.SharedConstants.Languages.French;
	}
}
