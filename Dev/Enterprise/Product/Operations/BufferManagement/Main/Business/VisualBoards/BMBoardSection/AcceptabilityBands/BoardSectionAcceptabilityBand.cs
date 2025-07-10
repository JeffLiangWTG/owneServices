using System;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.BufferManagement.Business
{
	public class BoardSectionAcceptabilityBand : NonPersistentBusinessObject<BoardSectionAcceptabilityBandValidation>, IAcceptabilityBandOverride
	{
		public BoardSectionAcceptabilityBand(BMBoardSection boardSection)
			: base(boardSection.Factory)
		{
			BoardSection = boardSection;
		}

		public BMBoardSection BoardSection { get; }

		#region BusinessObject Overrides

		public override BoardSectionAcceptabilityBandValidation GetNewValidation()
		{
			return new BoardSectionAcceptabilityBandValidation(this);
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FiltersByReleaseGroupOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			FiltersBySectionOverride = BoardSectionAcceptabilityBandLookups.DefaultOptionCode;
			ShowOn = AcceptabilityBandShowOnOptions.Codes.Tile;
			MaximumItems = 20;
		}

		protected override ZString HumanReadableNameCore => Res.GetString("b4d65bc2-e512-41c8-bd9c-2e5511f3d6a1", "Acceptability Band");

		#endregion

		#region Properties

		#region AcceptabilityBandPK

		[XmlColumnProperty]
		[List("Lookups.AvailableAcceptabilityBands")]
		[RelatedBusinessObject("AcceptabilityBand")]
		[ResourceStringData("BoardSectionAcceptabilityBand.AcceptabilityBandPK", Caption = "Acceptability Band")]
		public ZGuid AcceptabilityBandPK
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(AcceptabilityBandPKInfo); }
			set
			{
				SetXmlColumnPropertyValue(AcceptabilityBandPKInfo, value);
				var band = AcceptabilityBand;

				if (band != null)
				{
					DisplayName = band.BAB_Name;
					UpdateDefaultBoundaryOverrideValues(band);
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateAcceptabilityBandPK();
					Validation.ValidateAllBoundaryValues();
				}
			}
		}

		public ZPropertyInfo AcceptabilityBandPKInfo
		{
			get { return GetZPropertyInfo(nameof(AcceptabilityBandPK)); }
		}

		#endregion

		#region DisplaySequence

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.DisplaySequence", Caption = "Sequence", ShortCaption = "Seq.")]
		public ZShort DisplaySequence
		{
			get { return GetXmlColumnPropertyValue<ZShort>(DisplaySequenceInfo); }
			set
			{
				SetXmlColumnPropertyValue(DisplaySequenceInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateDisplaySequence();
				}
			}
		}

		public ZPropertyInfo DisplaySequenceInfo
		{
			get { return GetZPropertyInfo(nameof(DisplaySequence)); }
		}

		#endregion

		#region DisplayName

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.DisplayName", Caption = "Display Name", ShortCaption = "Name")]
		public ZString DisplayName
		{
			get { return GetXmlColumnPropertyValue<ZString>(DisplayNameInfo); }
			set { SetXmlColumnPropertyValue(DisplayNameInfo, value); }
		}

		public ZPropertyInfo DisplayNameInfo
		{
			get { return GetZPropertyInfo(nameof(DisplayName)); }
		}

		#endregion

		#region DisplayUnits

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.DisplayUnits", Caption = "Display Units", ShortCaption = "Units", FullDescription = "The type of units which will be appended to the value when displayed on board section tiles.")]
		public ZString DisplayUnits
		{
			get { return GetXmlColumnPropertyValue<ZString>(DisplayUnitsInfo); }
			set { SetXmlColumnPropertyValue(DisplayUnitsInfo, value); }
		}

		public ZPropertyInfo DisplayUnitsInfo
		{
			get { return GetZPropertyInfo(nameof(DisplayUnits)); }
		}

		#endregion

		#region Visualization Option Overrides

		#region FiltersByReleaseGroupOverride

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.FiltersByReleaseGroupOverride", Caption = "Filter By Release Group", ShortCaption = "Release Group", FullDescription = "Only consider workflows that have a release group matching the release group of this board section.")]
		[List("Lookups.FiltersByReleaseGroupOverrideOptions")]
		public ZString FiltersByReleaseGroupOverride
		{
			get { return GetXmlColumnPropertyValue<ZString>(FiltersByReleaseGroupOverrideInfo); }
			set
			{
				SetXmlColumnPropertyValue(FiltersByReleaseGroupOverrideInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAcceptabilityBandPK();
				}
			}
		}

		public ZPropertyInfo FiltersByReleaseGroupOverrideInfo => GetZPropertyInfo(nameof(FiltersByReleaseGroupOverride));

		internal AcceptabilityBandVisualizationOption FiltersByReleaseGroup => GetVisualizationOption(FiltersByReleaseGroupOverride);

		public bool IsFilteringByReleaseGroup => IsFiltering(FiltersByReleaseGroup, band => band.BAB_FiltersByReleaseGroup);

		#endregion

		#region FiltersBySectionOverride

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.FiltersBySectionOverride", Caption = "Filter By Board Section", FullDescription = "Only consider workflows that match the workflow filters and component for this board section.")]
		[List("Lookups.FiltersBySectionOverrideOptions")]
		public ZString FiltersBySectionOverride
		{
			get { return GetXmlColumnPropertyValue<ZString>(FiltersBySectionOverrideInfo); }
			set
			{
				SetXmlColumnPropertyValue(FiltersBySectionOverrideInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateFiltersBySectionOverride();
					Validation.ValidateAcceptabilityBandPK();
				}
			}
		}

		public ZPropertyInfo FiltersBySectionOverrideInfo => GetZPropertyInfo(nameof(FiltersBySectionOverride));

		internal AcceptabilityBandVisualizationOption FiltersBySection => GetVisualizationOption(FiltersBySectionOverride);

		static AcceptabilityBandVisualizationOption GetVisualizationOption(string code)
		{
			switch (code)
			{
				case BoardSectionAcceptabilityBandLookups.YesOptionCode:
					return AcceptabilityBandVisualizationOption.Yes;
				case BoardSectionAcceptabilityBandLookups.NoOptionCode:
					return AcceptabilityBandVisualizationOption.No;
			}

			return AcceptabilityBandVisualizationOption.Default;
		}

		public bool IsFilteringBySection => IsFiltering(FiltersBySection, band => band.BAB_FiltersBySection);

		#endregion

		bool IsFiltering(AcceptabilityBandVisualizationOption visualizationOption, Func<BMComponentAcceptabilityBand, ZBool> bandFilteringSettingGetter)
		{
			switch (visualizationOption)
			{
				case AcceptabilityBandVisualizationOption.Yes:
					return true;
				case AcceptabilityBandVisualizationOption.No:
					return false;
			}

			BMComponentAcceptabilityBand band = null;

			try
			{
				band = AcceptabilityBand;
			}
			catch (NullReferenceException ex)
			{
				ErrorReporter.ReportOnce($"Could not load accessibility band", ex);
			}
			return band != null && bandFilteringSettingGetter(band);
		}

		[XmlColumnProperty]
		[ReadOnlyMember(nameof(MaximumItems_ReadOnly))]
		[ResourceStringData("BoardSectionAcceptabilityBand.MaximumItems", Caption = "Maximum Items to Display", FullDescription = "The maximum number of acceptability bands or section heading acceptability items to display when the acceptability band's query includes an Additional Aggregator column. Items will be displayed in the order generated by the SQL query.")]
		public ZInt MaximumItems
		{
			get => GetXmlColumnPropertyValue<ZInt>(MaximumItemsInfo);
			set
			{
				SetXmlColumnPropertyValue(MaximumItemsInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateMaximumItems();
				}
			}
		}

		public ZPropertyInfo MaximumItemsInfo => GetZPropertyInfo(nameof(MaximumItems));

		bool MaximumItems_ReadOnly
		{
			get
			{
				var band = AcceptabilityBand;

				return band == null || band.IsSqlDisabled || band.BAB_SqlText.IsEmpty || !BMComponentAcceptabilityBand.IsAdditionalAggregatorColumnPresent(band.BAB_SqlText.ToString());
			}
		}

		#endregion

		#region Boundary Values Overrides

		#region AreBoundaryValuesOverridden

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.AreBoundaryValuesOverridden", Caption = "Override Boundary Values", FullDescription = "Enable to specify overridden boundary values without changing the values configured on the Acceptability Band form.")]
		public ZBool AreBoundaryValuesOverridden
		{
			get { return GetXmlColumnPropertyValue<ZBool>(AreBoundaryValuesOverriddenInfo); }
			set
			{
				SetXmlColumnPropertyValue(AreBoundaryValuesOverriddenInfo, value);
				UpdateDefaultBoundaryOverrideValues(AcceptabilityBand);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAllBoundaryValues();
				}
			}
		}

		public ZPropertyInfo AreBoundaryValuesOverriddenInfo => GetZPropertyInfo(nameof(AreBoundaryValuesOverridden));

		#endregion

		#region CautionMinOverride

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.CautionMinOverride", Caption = "Minimum Caution Value", ShortCaption = "Caution Min", FullDescription = "Overrides the minimum value for this Acceptability Band to have a status of Caution.")]
		public ZInt CautionMinOverride
		{
			get { return GetXmlColumnPropertyValue<ZInt>(CautionMinOverrideInfo); }
			set
			{
				SetXmlColumnPropertyValue(CautionMinOverrideInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAllBoundaryValues();
				}
			}
		}

		public ZPropertyInfo CautionMinOverrideInfo => GetZPropertyInfo(nameof(CautionMinOverride));

		[ReadOnlyMember(nameof(AreBoundaryValueOverridePropertiesReadOnly))]
		[ResourceStringData("BoardSectionAcceptabilityBand.CautionMinOverride", Caption = "Minimum Caution Value", ShortCaption = "Caution Min", FullDescription = "Overrides the minimum value for this Acceptability Band to have a status of Caution.")]
		public ZInt CautionMinEffectiveValue
		{
			get => AreBoundaryValuesOverridden ? CautionMinOverride : AcceptabilityBand?.BAB_CautionLowerBound ?? 0;
			set
			{
				if (AreBoundaryValuesOverridden)
				{
					CautionMinOverride = value;
				}
			}
		}

		public ZWrappedPropertyInfo CautionMinEffectiveValueInfo => GetWrappedZPropertyInfo(nameof(CautionMinEffectiveValue), _ => CautionMinOverrideInfo);

		#endregion

		#region GoodMinOverride

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.GoodMinOverride", Caption = "Minimum Good Value", ShortCaption = "Good Min", FullDescription = "Overrides the minimum value for this Acceptability Band to have a status of Good.")]
		public ZInt GoodMinOverride
		{
			get { return GetXmlColumnPropertyValue<ZInt>(GoodMinOverrideInfo); }
			set
			{
				SetXmlColumnPropertyValue(GoodMinOverrideInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAllBoundaryValues();
				}
			}
		}

		public ZPropertyInfo GoodMinOverrideInfo => GetZPropertyInfo(nameof(GoodMinOverride));

		[ReadOnlyMember(nameof(AreBoundaryValueOverridePropertiesReadOnly))]
		[ResourceStringData("BoardSectionAcceptabilityBand.GoodMinOverride", Caption = "Minimum Good Value", ShortCaption = "Good Min", FullDescription = "Overrides the minimum value for this Acceptability Band to have a status of Good.")]
		public ZInt GoodMinEffectiveValue
		{
			get => AreBoundaryValuesOverridden ? GoodMinOverride : AcceptabilityBand?.BAB_GoodLowerBound ?? 0;
			set
			{
				if (AreBoundaryValuesOverridden)
				{
					GoodMinOverride = value;
				}
			}
		}

		public ZWrappedPropertyInfo GoodMinEffectiveValueInfo => GetWrappedZPropertyInfo(nameof(GoodMinEffectiveValue), _ => GoodMinOverrideInfo);

		#endregion

		#region ExcellentMinOverride

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.ExcellentMinOverride", Caption = "Minimum Excellent Value", ShortCaption = "Excellent Min", FullDescription = "Overrides the minimum value for this Acceptability Band to have a status of Excellent.")]
		public ZInt ExcellentMinOverride
		{
			get { return GetXmlColumnPropertyValue<ZInt>(ExcellentMinOverrideInfo); }
			set
			{
				SetXmlColumnPropertyValue(ExcellentMinOverrideInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAllBoundaryValues();
				}
			}
		}

		public ZPropertyInfo ExcellentMinOverrideInfo => GetZPropertyInfo(nameof(ExcellentMinOverride));

		[ReadOnlyMember(nameof(AreBoundaryValueOverridePropertiesReadOnly))]
		[ResourceStringData("BoardSectionAcceptabilityBand.ExcellentMinOverride", Caption = "Minimum Excellent Value", ShortCaption = "Excellent Min", FullDescription = "Overrides the minimum value for this Acceptability Band to have a status of Excellent.")]
		public ZInt ExcellentMinEffectiveValue
		{
			get => AreBoundaryValuesOverridden ? ExcellentMinOverride : AcceptabilityBand?.BAB_ExcellentLowerBound ?? 0;
			set
			{
				if (AreBoundaryValuesOverridden)
				{
					ExcellentMinOverride = value;
				}
			}
		}

		public ZWrappedPropertyInfo ExcellentMinEffectiveValueInfo => GetWrappedZPropertyInfo(nameof(ExcellentMinEffectiveValue), _ => ExcellentMinOverrideInfo);

		#endregion

		#region ExcellentMaxOverride

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.ExcellentMaxOverride", Caption = "Maximum Excellent Value", ShortCaption = "Excellent Max", FullDescription = "Overrides the maximum value for this Acceptability Band to have a status of Excellent.")]
		public ZInt ExcellentMaxOverride
		{
			get { return GetXmlColumnPropertyValue<ZInt>(ExcellentMaxOverrideInfo); }
			set
			{
				SetXmlColumnPropertyValue(ExcellentMaxOverrideInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAllBoundaryValues();
				}
			}
		}

		public ZPropertyInfo ExcellentMaxOverrideInfo => GetZPropertyInfo(nameof(ExcellentMaxOverride));

		[ReadOnlyMember(nameof(AreBoundaryValueOverridePropertiesReadOnly))]
		[ResourceStringData("BoardSectionAcceptabilityBand.ExcellentMaxOverride", Caption = "Maximum Excellent Value", ShortCaption = "Excellent Max", FullDescription = "Overrides the maximum value for this Acceptability Band to have a status of Excellent.")]
		public ZInt ExcellentMaxEffectiveValue
		{
			get => AreBoundaryValuesOverridden ? ExcellentMaxOverride : AcceptabilityBand?.BAB_ExcellentUpperBound ?? 0;
			set
			{
				if (AreBoundaryValuesOverridden)
				{
					ExcellentMaxOverride = value;
				}
			}
		}

		public ZWrappedPropertyInfo ExcellentMaxEffectiveValueInfo => GetWrappedZPropertyInfo(nameof(ExcellentMaxEffectiveValue), _ => ExcellentMaxOverrideInfo);

		#endregion

		#region GoodMaxOverride

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.GoodMaxOverride", Caption = "Maximum Good Value", ShortCaption = "Good Max", FullDescription = "Overrides the maximum value for this Acceptability Band to have a status of Good.")]
		public ZInt GoodMaxOverride
		{
			get { return GetXmlColumnPropertyValue<ZInt>(GoodMaxOverrideInfo); }
			set
			{
				SetXmlColumnPropertyValue(GoodMaxOverrideInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAllBoundaryValues();
				}
			}
		}

		public ZPropertyInfo GoodMaxOverrideInfo => GetZPropertyInfo(nameof(GoodMaxOverride));

		[ReadOnlyMember(nameof(AreBoundaryValueOverridePropertiesReadOnly))]
		[ResourceStringData("BoardSectionAcceptabilityBand.GoodMaxOverride", Caption = "Maximum Good Value", ShortCaption = "Good Max", FullDescription = "Overrides the maximum value for this Acceptability Band to have a status of Good.")]
		public ZInt GoodMaxEffectiveValue
		{
			get => AreBoundaryValuesOverridden ? GoodMaxOverride : AcceptabilityBand?.BAB_GoodUpperBound ?? 0;
			set
			{
				if (AreBoundaryValuesOverridden)
				{
					GoodMaxOverride = value;
				}
			}
		}

		public ZWrappedPropertyInfo GoodMaxEffectiveValueInfo => GetWrappedZPropertyInfo(nameof(GoodMaxEffectiveValue), _ => GoodMaxOverrideInfo);

		#endregion

		#region CautionMaxOverride

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.CautionMaxOverride", Caption = "Maximum Caution Value", ShortCaption = "Caution Max", FullDescription = "Overrides the maximum value for this Acceptability Band to have a status of Caution.")]
		public ZInt CautionMaxOverride
		{
			get { return GetXmlColumnPropertyValue<ZInt>(CautionMaxOverrideInfo); }
			set
			{
				SetXmlColumnPropertyValue(CautionMaxOverrideInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateAllBoundaryValues();
				}
			}
		}

		public ZPropertyInfo CautionMaxOverrideInfo => GetZPropertyInfo(nameof(CautionMaxOverride));

		[ReadOnlyMember(nameof(AreBoundaryValueOverridePropertiesReadOnly))]
		[ResourceStringData("BoardSectionAcceptabilityBand.CautionMaxOverride", Caption = "Maximum Caution Value", ShortCaption = "Caution Max", FullDescription = "Overrides the maximum value for this Acceptability Band to have a status of Caution.")]
		public ZInt CautionMaxEffectiveValue
		{
			get => AreBoundaryValuesOverridden ? CautionMaxOverride : AcceptabilityBand?.BAB_CautionUpperBound ?? 0;
			set
			{
				if (AreBoundaryValuesOverridden)
				{
					CautionMaxOverride = value;
				}
			}
		}

		public ZWrappedPropertyInfo CautionMaxEffectiveValueInfo => GetWrappedZPropertyInfo(nameof(CautionMaxEffectiveValue), _ => CautionMaxOverrideInfo);

		#endregion

		bool AreBoundaryValueOverridePropertiesReadOnly => !AreBoundaryValuesOverridden;

		void UpdateDefaultBoundaryOverrideValues(BMComponentAcceptabilityBand band)
		{
			if (band != null)
			{
				using (GetValidationSuspender())
				{
					CautionMinOverride = band.BAB_CautionLowerBound;
					GoodMinOverride = band.BAB_GoodLowerBound;
					ExcellentMinOverride = band.BAB_ExcellentLowerBound;
					ExcellentMaxOverride = band.BAB_ExcellentUpperBound;
					GoodMaxOverride = band.BAB_GoodUpperBound;
					CautionMaxOverride = band.BAB_CautionUpperBound;
				}
			}
		}

		public AcceptabilityBandBoundaryValues BoundaryValues => new AcceptabilityBandBoundaryValues(CautionMinEffectiveValue, GoodMinEffectiveValue, ExcellentMinEffectiveValue, ExcellentMaxEffectiveValue, GoodMaxEffectiveValue, CautionMaxEffectiveValue);

		#endregion

		#region Show On

		[XmlColumnProperty]
		[ResourceStringData("BoardSectionAcceptabilityBand.ShowOn", Caption = "Show On", FullDescription = "Set which part of the board section to show this Acceptability Band.")]
		[List("Lookups.ShowOnOptions")]
		public ZString ShowOn
		{
			get { return GetXmlColumnPropertyValue<ZString>(ShowOnInfo); }
			set
			{
				SetXmlColumnPropertyValue(ShowOnInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateShowOn();
				}
			}
		}

		public ZPropertyInfo ShowOnInfo => GetZPropertyInfo(nameof(ShowOn));

		public AcceptabilityBandShowOnOption SelectedShowOnOption
		{
			get
			{
				return GetShowOnOption(ShowOn);
			}
			set
			{
				switch (value)
				{
					case AcceptabilityBandShowOnOption.Heading:
						ShowOn = AcceptabilityBandShowOnOptions.Codes.Heading;
						break;
					case AcceptabilityBandShowOnOption.Both:
						ShowOn = AcceptabilityBandShowOnOptions.Codes.Both;
						break;
					default:
						ShowOn = AcceptabilityBandShowOnOptions.Codes.Tile;
						break;
				}
			}
		}

		static AcceptabilityBandShowOnOption GetShowOnOption(string code)
		{
			if (string.Equals(code, AcceptabilityBandShowOnOptions.Codes.Heading, StringComparison.OrdinalIgnoreCase))
			{
				return AcceptabilityBandShowOnOption.Heading;
			}
			if (string.Equals(code, AcceptabilityBandShowOnOptions.Codes.Both, StringComparison.OrdinalIgnoreCase))
			{
				return AcceptabilityBandShowOnOption.Both;
			}

			return AcceptabilityBandShowOnOption.Tile;
		}

		public bool ShouldShowAsTile
		{
			get
			{
				var selectedOption = SelectedShowOnOption;
				return selectedOption == AcceptabilityBandShowOnOption.Tile || selectedOption == AcceptabilityBandShowOnOption.Both;
			}
		}

		public bool ShouldShowInHeading
		{
			get
			{
				var selectedOption = SelectedShowOnOption;
				return selectedOption == AcceptabilityBandShowOnOption.Heading || selectedOption == AcceptabilityBandShowOnOption.Both;
			}
		}

		#endregion

		#endregion

		#region Related Business Objects

		public BMComponentAcceptabilityBand AcceptabilityBand
		{
			get
			{
				if (acceptabilityBand == null)
				{
					acceptabilityBand = Factory.Load<BMComponentAcceptabilityBand>(AcceptabilityBandPK);
				}
				return acceptabilityBand;
			}
		}
		BMComponentAcceptabilityBand acceptabilityBand;

		internal void AddBMComponentAcceptabilityBandFetchHint()
		{
			Factory.AddFetchHint(BMComponentAcceptabilityBand.Schema.TableName, AcceptabilityBandPK);
		}

		#endregion

		#region Lookups

		public BoardSectionAcceptabilityBandLookups Lookups => lookups ?? (lookups = new BoardSectionAcceptabilityBandLookups(this));
		BoardSectionAcceptabilityBandLookups lookups;

		#endregion
	}
}
