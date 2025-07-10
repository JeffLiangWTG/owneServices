using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	[DescriptionProperty("SectionName")]
	public class BMComponentSectionConfiguration : BMBoardSectionConfigurationWithOverridableSectionName<BMComponentSectionConfigurationValidation>
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
		public BMComponentSectionConfiguration(IBMBoardSection section)
			: base(section.Factory)
		{
			this.section = section as BMBoardSection;
		}

		public BMBoardSection Section
		{
			get { return section; }
		}

		BMBoardSection section;

		#region BusinessObject Overrides

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			FlowDirection = FlowDirectionList.Codes.Down;
			Subsections = 1;
			CellsPerSubsection = 1;
			ChannelBy = ChannelTypeList.Codes.NotChanneled;
			ChannelSecondaryBy = BMConstants.ChannelByTimeCode;
			TimeProgressionMode = TimeProgressionModeList.Codes.Age;
			TimeField = DefaultTimeFieldValue;
			ShowZones = true;
			CardType = CardTypeList.Codes.Task;

			CountdownTargetBorderColor = Color.Red.Name;
			CountdownTargetBorderStyle = new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, true).ToString();
			CountdownStartableBorderColor = Color.Blue.Name;
			CountdownStartableBorderStyle = new SizedButtonBorderStyle(VisualBoardButtonBorderStyle.Solid, true).ToString();

			EnableShowCurrentItemsFilterByDefault = false;
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (Section.IsInDatabase)
			{
				PrimaryChannelsViewModel.OnChannelsChanged(ChannelAxis.Primary);
				SecondaryChannelsViewModel.OnChannelsChanged(ChannelAxis.Secondary);
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			section.UpdateConfigurationForFilters();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);

			if (saveSucceeded)
			{
				if (applicableLayouts != null)
				{
					ApplicableLayouts.Reload();
				}
			}
		}

		public override void Delete()
		{
			section.DeleteFilters();
			AdditionalComponents.DeleteAll();
			CustomisedLayoutLinks.DeleteAll();
			PrimaryAxisChannels.DeleteAll();
			SecondaryAxisChannels.DeleteAll();

			base.Delete();
		}

		public override void CopyConfigurationPropertiesToNewSection(IBMBoardSection newSection)
		{
			var cloneSection = (BMBoardSection)newSection;
			var cloneConfig = cloneSection.SectionConfiguration;
			cloneConfig.section = cloneSection;
			cloneConfig.AdditionalComponents.DeleteAll();
			section.CopyFilters(cloneSection);

			foreach (var component in this.AdditionalComponents.ToArray())
			{
				var newComponent = Factory.New<BMBoardSectionAdditionalComponent>();
				newComponent.BSA_MS_Section = cloneConfig.PK;
				newComponent.BSA_FC_Component = component.BSA_FC_Component;
				cloneConfig.AdditionalComponents.Add(newComponent);
			}

			if (OverrideChannels)
			{
				foreach (var channel in PrimaryAxisChannels)
				{
					CloneChannel(channel, cloneSection);
				}
			}
			else
			{
				DefaultChannelsProvider.AddMissingDefaultChannels(cloneConfig, cloneConfig.PrimaryAxisChannels, cloneConfig.ChannelBy, cloneConfig.OverrideChannels);
			}

			if (OverrideSecondaryChannels)
			{
				foreach (var channel in SecondaryAxisChannels)
				{
					CloneChannel(channel, cloneSection);
				}
			}
			else
			{
				DefaultChannelsProvider.AddMissingDefaultChannels(cloneConfig, cloneConfig.SecondaryAxisChannels, cloneConfig.ChannelSecondaryBy, cloneConfig.OverrideSecondaryChannels);
			}
		}

		void CloneChannel(BMBoardSectionChannel channel, BMBoardSection cloneSection)
		{
			var clone = (BMBoardSectionChannel)channel.Clone(new BusinessObjectCloneArgs(new[] { BMBoardSectionChannelSchema.Constants.MSC_MS_Section }));

			clone.MSC_MS_Section = cloneSection.PK;
		}

		public override BMComponentSectionConfigurationValidation GetNewValidation()
		{
			return new BMComponentSectionConfigurationValidation(this);
		}

		protected override ZString HumanReadableNameCore => Res.GetString("3ac3179d-04bf-44b0-b35c-9f661ddd13d0", "Section Configuration");

		#endregion

		#region Component Defaults

		public void SetDefaultValues_ForBuffer()
		{
			CellsPerSubsection = 10;
			FadeBackgroundAtPercentage = 80;
			FlowDirection = FlowDirectionList.Codes.Up;
			PanelLayoutStyle = PanelLayoutTypeList.Codes.Staggered;
			ShowZones = true;
			TimeProgressionMode = TimeProgressionModeList.Codes.Age;
		}

		public void SetDefaultValues_ForBucket()
		{
			PanelLayoutStyle = PanelLayoutTypeList.Codes.Stacked;
			ShowZones = false;
		}

		#endregion

		#region XML Properties

		#region ReleaseGroup

		[XmlColumnProperty]
		[List("Lookups.SystemReleaseGroups")]
		[ResourceStringData("BMBoardSectionConfiguration.ReleaseGroupPK", Caption = "Release Group", FullDescription = "For bucket components, only tasks from workflows belonging to the selected Release Group will be displayed.")]
		public ZGuid ReleaseGroupPK
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(ReleaseGroupPKInfo); }
			set
			{
				SetXmlColumnPropertyValue(ReleaseGroupPKInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateReleaseGroupPK();
					Validation.ValidateShowWorkInReleaseGroupOnly();
				}

				if (value.IsValid)
				{
					var component = Section.Component;
					if (component != null && !component.IsBuffer)
					{
						ShowWorkInReleaseGroupOnly = true;
					}
				}
				else
				{
					ShowWorkInReleaseGroupOnly = false;
				}
			}
		}

		public ZPropertyInfo ReleaseGroupPKInfo
		{
			get { return GetZPropertyInfo(nameof(ReleaseGroupPK)); }
		}

		#endregion

		#region Subsections

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.Subsections", Caption = "Subsections", FullDescription = "The number of rows or columns in this section (depending on orientation). A value greater than 1 indicates this section is wrapped.")]
		public ZInt Subsections
		{
			get { return GetXmlColumnPropertyValue<ZInt>(SubsectionsInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(Subsections, value))
				{
					SetXmlColumnPropertyValue(SubsectionsInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateSubsections();
						Validation.ValidateLastCell();
					}
				}
			}
		}

		public ZPropertyInfo SubsectionsInfo
		{
			get { return GetZPropertyInfo(nameof(Subsections)); }
		}

		public bool IsWrapped
		{
			get { return Subsections > 1; }
		}

		#endregion

		#region FlowDirection

		[XmlColumnProperty]
		[List("Lookups.FlowDirectionList")]
		[ResourceStringData("BMBoardSectionConfiguration.FlowDirection", Caption = "Flow Direction")]
		[MaxLength(3)]
		public ZString FlowDirection
		{
			get { return GetXmlColumnPropertyValue<ZString>(FlowDirectionInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(FlowDirection, value))
				{
					SetXmlColumnPropertyValue(FlowDirectionInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateFlowDirection();
					}
					if (!Lookups.LastCellList.ContainsCode(LastCell))
					{
						LastCell = ZString.Empty;
					}
				}
			}
		}

		public ZPropertyInfo FlowDirectionInfo
		{
			get { return GetZPropertyInfo(nameof(FlowDirection)); }
		}

		#endregion

		#region LastCell

		[XmlColumnProperty]
		[List("Lookups.LastCellList")]
		[ResourceStringData("BMBoardSectionConfiguration.LastCell", Caption = "Last Cell")]
		[MaxLength(3)]
		public ZString LastCell
		{
			get { return GetXmlColumnPropertyValue<ZString>(LastCellInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(LastCell, value))
				{
					SetXmlColumnPropertyValue(LastCellInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateLastCell();
					}
				}
			}
		}

		public ZPropertyInfo LastCellInfo
		{
			get { return GetZPropertyInfo(nameof(LastCell)); }
		}

		protected bool LastCell_ReadOnly
		{
			get { return !IsWrapped; }
		}

		#endregion

		#region CellsPerSubsection

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		[ResourceStringData("BMBoardSectionConfiguration.CellsPerSubsection", Caption = "Cells Per Subsection", ShortCaption = "Subsection Cells", FullDescription = "The number of cells in each row or column subsection.")]
		public ZInt CellsPerSubsection
		{
			get { return GetXmlColumnPropertyValue<ZInt>(CellsPerSubsectionInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(CellsPerSubsection, value))
				{
					SetXmlColumnPropertyValue(CellsPerSubsectionInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateCellsPerSubsection();
						Validation.ValidateTimePerCell();
						Validation.ValidateChannelSecondaryBy();
						Validation.ValidateFadeBackgroundAtPercentage();
						ValidateAcceptabilityBands();
					}
				}
			}
		}

		public ZPropertyInfo CellsPerSubsectionInfo
		{
			get { return GetZPropertyInfo(nameof(CellsPerSubsection)); }
		}

		void ValidateAcceptabilityBands()
		{
			foreach (var band in AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>())
			{
				band.Validation.ValidateFiltersBySectionOverride();
			}
		}

		#endregion

		#region TimePerCell

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.TimePerCell", Caption = "Time Per Cell", ShortCaption = "Cell Time", FullDescription = "The length of time each cell represents.")]
		public ZDateTime TimePerCell
		{
			get { return IsBuffer ? GetBufferTimePerCell() : GetXmlColumnPropertyValue<ZDateTime>(TimePerCellInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(TimePerCell, value))
				{
					SetXmlColumnPropertyValue(TimePerCellInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateTimePerCell();
					}
				}
			}
		}

		public ZPropertyInfo TimePerCellInfo
		{
			get { return GetZPropertyInfo(nameof(TimePerCell)); }
		}

		protected bool TimePerCell_ReadOnly
		{
			get { return IsBuffer; }
		}

		ZDateTime GetBufferTimePerCell()
		{
			if (ShowChildComponentZones && Section.ApplicableComponents.Count() > 1)
			{
				return ComponentGridHelper.GetTimePerCellForBuffer(this, Section.ApplicableComponents.MaxBy(c => c.FC_BufferTimespanInMinutes));
			}

			if (Section.Component != null)
			{
				return ComponentGridHelper.GetTimePerCellForBuffer(this, Section.Component);
			}

			return ZDateTime.Empty;
		}

		public TimeSpan TimeSpanPerCell
		{
			get
			{
				var timePerCell = TimePerCell;
				return timePerCell.IsValid ? timePerCell - new ZDateTime(timePerCell.Year, 1, 1) : TimeSpan.Zero;
			}
		}

		#endregion

		#region ShowWorkInReleaseGroupOnly

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		[ResourceStringData("BMBoardSectionConfiguration.ShowWorkInReleaseGroupOnly", Caption = "Show Work In Release Group Only", FullDescription = "When ticked, only workflows with the release group set on this board section will be shown.")]
		public ZBool ShowWorkInReleaseGroupOnly
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowWorkInReleaseGroupOnlyInfo); }
			set
			{
				SetXmlColumnPropertyValue(ShowWorkInReleaseGroupOnlyInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateShowWorkInReleaseGroupOnly();
				}
			}
		}

		public ZPropertyInfo ShowWorkInReleaseGroupOnlyInfo
		{
			get { return GetZPropertyInfo(nameof(ShowWorkInReleaseGroupOnly)); }
		}

		#endregion

		#region TimeProgressionMode

		[XmlColumnProperty(DefaultValue = TimeProgressionModeList.Codes.Age)]
		[List("Lookups.TimeProgressionModeList")]
		[ResourceStringData("BMBoardSectionConfiguration.TimeProgressionMode", Caption = "Time Progression Mode")]
		[MaxLength(3)]
		public ZString TimeProgressionMode
		{
			get { return GetXmlColumnPropertyValue<ZString>(TimeProgressionModeInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(TimeProgressionMode, value))
				{
					SetXmlColumnPropertyValue(TimeProgressionModeInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateTimeProgressionMode();
						Validation.ValidateTimeField();
					}
				}
			}
		}

		public ZPropertyInfo TimeProgressionModeInfo
		{
			get { return GetZPropertyInfo(nameof(TimeProgressionMode)); }
		}

		#endregion

		#region TimeField

		const string DefaultTimeFieldValue = TimeProgressionFieldList.Codes.TransferTime;

		[XmlColumnProperty(DefaultValue = DefaultTimeFieldValue)]
		[List("Lookups.TimeFieldList")]
		[ResourceStringData("BMBoardSectionConfiguration.TimeField", Caption = "Date/Time for Progression", FullDescription = "The field to use when calculating age or countdown of tasks.")]
		[MaxLength(3)]
		public ZString TimeField
		{
			get { return GetXmlColumnPropertyValue<ZString>(TimeFieldInfo); }
			set
			{
				if (TimeField != value)
				{
					timeFieldProperty = null;
				}

				SetXmlColumnPropertyValue(TimeFieldInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateTimeField();
				}
			}
		}

		public ZPropertyInfo TimeFieldInfo
		{
			get { return GetZPropertyInfo(nameof(TimeField)); }
		}

		public ZDateTime GetTimeFieldValue(IProcessHeader processHeader)
		{
			Argument.NotNull(processHeader, "processHeader");
			var value = TimeFieldProperty != null ? (ZDateTime)TimeFieldProperty.GetValue(processHeader, null) : ZDateTime.Empty;
			return value.IsValid ? value : processHeader.FH_FH_ParentHeader.IsValid ? GetTimeFieldValue(processHeader.JobHeader) : ZDateTime.Empty;
		}

		PropertyInfo TimeFieldProperty
		{
			get { return timeFieldProperty ?? (timeFieldProperty = typeof(ProcessHeader).GetProperty(GetTimeFieldName(), BindingFlags.Public | BindingFlags.Instance)); }
		}
		PropertyInfo timeFieldProperty;

		string GetTimeFieldName()
		{
			switch (TimeField)
			{
				case TimeProgressionFieldList.Codes.AgreedDeliveryDate:
					return ProcessHeaderSchema.Constants.FH_AgreedDeliveryDate;
				case TimeProgressionFieldList.Codes.CreateTime:
					return ProcessHeaderSchema.Constants.FH_SystemCreateTimeUtc;
				case TimeProgressionFieldList.Codes.DoNotStartBeforeDate:
					return ProcessHeaderSchema.Constants.FH_DoNotStartBeforeDate;
				case TimeProgressionFieldList.Codes.TransferTime:
					return ProcessHeaderSchema.Constants.FH_ReleaseDateTime;
			}

			return string.Empty;
		}

		#endregion

		#region MaxOverdueSlots

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.MaxOverdueSlots", Caption = "Overdue Slots", FullDescription = "The number of slots with a negative time")]
		public ZInt MaxOverdueSlots
		{
			get { return GetXmlColumnPropertyValue<ZInt>(MaxOverdueSlotsInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(MaxOverdueSlots, value))
				{
					SetXmlColumnPropertyValue(MaxOverdueSlotsInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateMaxOverdueSlots();
					}
				}
			}
		}

		public ZPropertyInfo MaxOverdueSlotsInfo
		{
			get { return GetZPropertyInfo(nameof(MaxOverdueSlots)); }
		}

		#endregion

		#region IsReleaseScheduler

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.IsReleaseScheduler", Caption = "Release Scheduler", FullDescription = "When ticked, this board section will automatically show channels for constrained resources, allowing scheduling of their work. All workflows in components with a Release Gate into this buffer are shown for the relevant resources.")]
		public ZBool IsReleaseScheduler
		{
			get { return GetXmlColumnPropertyValue<ZBool>(IsReleaseSchedulerInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(IsReleaseScheduler, value))
				{
					SetXmlColumnPropertyValue(IsReleaseSchedulerInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateIsReleaseScheduler();
						Validation.ValidateSubsections();
						Validation.ValidateCellsPerSubsection();
						Validation.ValidateMaxOverdueSlots();
						Validation.ValidateFadeBackgroundAtPercentage();
						Validation.ValidateCardType();
						Validation.ValidateChannelBy();
						Validation.ValidateChannelSecondaryBy();
						Validation.ValidateOverrideChannels();
						Validation.ValidateOverrideSecondaryChannels();
						Validation.ValidateShowUnchanneled();
						Validation.ValidateShowSecondaryUnchanneled();
						Validation.ValidateShowZones();
						Validation.ValidateShowChildComponentZones();
						Validation.ValidatePanelLayoutStyle();
					}
				}
			}
		}

		public ZPropertyInfo IsReleaseSchedulerInfo
		{
			get { return GetZPropertyInfo(nameof(IsReleaseScheduler)); }
		}

		#endregion

		#region Show Zones

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		[ResourceStringData("BMBoardSectionConfiguration.ShowZones", Caption = "Show Zones", ShortCaption = "Show Zones", FullDescription = "Show Zones on a buffer section.")]
		public ZBool ShowZones
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowZonesInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(ShowZones, value))
				{
					SetXmlColumnPropertyValue(ShowZonesInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateCellsPerSubsection();
						Validation.ValidateTimePerCell();
						Validation.ValidateChannelSecondaryBy();
						Validation.ValidateShowZones();
						Validation.ValidateShowChildComponentZones();
					}
				}
			}
		}

		public ZPropertyInfo ShowZonesInfo
		{
			get { return GetZPropertyInfo(nameof(ShowZones)); }
		}

		#endregion

		#region Show Child Component Zones

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		[ResourceStringData("BMBoardSectionConfiguration.ShowChildComponentZones", Caption = "Show Child Component Zones", ShortCaption = "Show Child Component Zones", FullDescription = "Show child component zones, in addition to primary zones, on a buffer section.")]
		public ZBool ShowChildComponentZones
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowChildComponentZonesInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(ShowChildComponentZones, value))
				{
					SetXmlColumnPropertyValue(ShowChildComponentZonesInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateCellsPerSubsection();
						Validation.ValidateTimePerCell();
						Validation.ValidateChannelSecondaryBy();
						Validation.ValidateShowZones();
						Validation.ValidateShowChildComponentZones();
					}
				}
			}
		}

		public ZPropertyInfo ShowChildComponentZonesInfo
		{
			get { return GetZPropertyInfo(nameof(ShowChildComponentZones)); }
		}

		#endregion

		#region Panel Layout

		[XmlColumnProperty]
		[List("Lookups.PanelLayoutStyleList")]
		[MaxLength(3)]
		[ResourceStringData("BMBoardSectionConfiguration.PanelLayoutStyle", Caption = "Panel Layout", ShortCaption = "Layout", FullDescription = "The panel layout for this board section.")]
		public ZString PanelLayoutStyle
		{
			get { return GetXmlColumnPropertyValue<ZString>(PanelLayoutStyleInfo); }
			set
			{
				SetXmlColumnPropertyValue(PanelLayoutStyleInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidatePanelLayoutStyle();
					Validation.ValidateShowZones();
					Validation.ValidateShowChildComponentZones();
				}
			}
		}

		public ZPropertyInfo PanelLayoutStyleInfo
		{
			get { return GetZPropertyInfo(nameof(PanelLayoutStyle)); }
		}

		#endregion

		#region CardType

		[XmlColumnProperty(DefaultValue = CardTypeList.Codes.Task)]
		[List("Lookups.CardTypeList")]
		[ResourceStringData("BMBoardSectionConfiguration.CardType", Caption = "Card Type", FullDescription = "The type of card to be displayed.")]
		[MaxLength(3)]
		public ZString CardType
		{
			get { return GetXmlColumnPropertyValue<ZString>(CardTypeInfo); }
			set
			{
				SetXmlColumnPropertyValue(CardTypeInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateCardType();

					var links = CustomisedLayoutLinks.ToArray();
					BMControlCustomisationLink.AddBMControlCustomisationFetchHints(links, Factory);
					foreach (var link in links)
					{
						link.Validation.ValidateFML_FM_ControlCustomisation();
					}
				}
			}
		}

		public ZPropertyInfo CardTypeInfo
		{
			get { return GetZPropertyInfo(nameof(CardType)); }
		}

		public bool ShowWorkflowOrJobWorkflowCards
		{
			get { return CardType == CardTypeList.Codes.Workflow || ShowJobWorkflowCards; }
		}

		public bool ShowJobWorkflowCards
		{
			get { return CardType == CardTypeList.Codes.JobLevelWorkflow; }
		}

		#endregion

		#region FadeBackgroundAtPercentage

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.FadeBackgroundAtPercentage", Caption = "Background Fade Percentage", ShortCaption = "Fade %", FullDescription = "The percentage of work in a Buffer resource channel which should be underneath a fade to white of the background color.")]
		public ZInt FadeBackgroundAtPercentage
		{
			get { return GetXmlColumnPropertyValue<ZInt>(FadeBackgroundAtPercentageInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(FadeBackgroundAtPercentage, value))
				{
					SetXmlColumnPropertyValue(FadeBackgroundAtPercentageInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateFadeBackgroundAtPercentage();
					}
				}
			}
		}

		public ZPropertyInfo FadeBackgroundAtPercentageInfo
		{
			get { return GetZPropertyInfo(nameof(FadeBackgroundAtPercentage)); }
		}

		#endregion

		#region HideResourceTasksFromCapabilityChannels

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.HideResourceTasksFromCapabilityChannels", Caption = "Capability Channels Hide Resource Tasks", FullDescription = "When ticked, capability channels will not show tasks assigned to a resource.")]
		public ZBool HideResourceTasksFromCapabilityChannels
		{
			get { return GetXmlColumnPropertyValue<ZBool>(HideResourceTasksFromCapabilityChannelsInfo); }
			set { SetXmlColumnPropertyValue(HideResourceTasksFromCapabilityChannelsInfo, value); }
		}

		public ZPropertyInfo HideResourceTasksFromCapabilityChannelsInfo
		{
			get { return GetZPropertyInfo(nameof(HideResourceTasksFromCapabilityChannels)); }
		}

		#endregion

		#region HideCapabilityTasksFromResourceChannels

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.HideCapabilityTasksFromResourceChannels", Caption = "Resource Channels Hide Capability Tasks", FullDescription = "When ticked, resource channels will not show tasks with required capabilities and no resource assigned.")]
		public ZBool HideCapabilityTasksFromResourceChannels
		{
			get { return GetXmlColumnPropertyValue<ZBool>(HideCapabilityTasksFromResourceChannelsInfo); }
			set { SetXmlColumnPropertyValue(HideCapabilityTasksFromResourceChannelsInfo, value); }
		}

		public ZPropertyInfo HideCapabilityTasksFromResourceChannelsInfo
		{
			get { return GetZPropertyInfo(nameof(HideCapabilityTasksFromResourceChannels)); }
		}

		#endregion

		#region EnableShowCurrentItemsFilterByDefault

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.EnableShowCurrentItemsFilterByDefault", Caption = "Enable Show Startable Items Filter By Default", FullDescription = "When ticked, the 'Show Startable Items' filter will be enabled by default on this board section.")]
		public ZBool EnableShowCurrentItemsFilterByDefault
		{
			get { return GetXmlColumnPropertyValue<ZBool>(EnableShowCurrentItemsFilterByDefaultInfo); }
			set { SetXmlColumnPropertyValue(EnableShowCurrentItemsFilterByDefaultInfo, value); }
		}

		public ZPropertyInfo EnableShowCurrentItemsFilterByDefaultInfo
		{
			get { return GetZPropertyInfo(nameof(EnableShowCurrentItemsFilterByDefault)); }
		}

		#endregion

		#region LayoutConfigUpdated

		IDisposable FireLayoutConfigUpdatedOnDisposeIfValueChanged<T>(T oldValue, T newValue)
		{
			if (EqualityComparer<T>.Default.Equals(oldValue, newValue))
			{
				return DisposableAction.NoAction;
			}
			else
			{
				return new DisposableAction(() =>
				{
					if (Section != null)
					{
						Section.OnLayoutConfigUpdated();
					}
				});
			}
		}

		#endregion

		#region Colors

		#region BufferZone3Color

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSectionConfiguration.BufferZone3Color", Caption = "Buffer Zone 3 Color", ShortCaption = "Zone 3", FullDescription = "The Zone 3 background color of a buffer component on a Visual Board.")]
		[ReadOnlyMember(nameof(IsBucket))]
		[MaxLength(50)]
		public ZString BufferZone3Color
		{
			get { return GetXmlColumnPropertyValue<ZString>(BufferZone3ColorInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(BufferZone3Color, value))
				{
					SetXmlColumnPropertyValue(BufferZone3ColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateBufferZone3Color();
					}
				}
			}
		}

		public ZPropertyInfo BufferZone3ColorInfo
		{
			get { return GetZPropertyInfo(nameof(BufferZone3Color)); }
		}

		[BusinessObjectTestExclude] // We want to return null if no color is specified
		public Color? BufferZone3ColorValue
		{
			get { return string.IsNullOrEmpty(BufferZone3Color) ? null : new Color?(ColorList.ColorFromName(BufferZone3Color)); }
		}

		#endregion

		#region BufferZone2Color

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSectionConfiguration.BufferZone2Color", Caption = "Buffer Zone 2 Color", ShortCaption = "Zone 2", FullDescription = "The Zone 2 background color of a buffer component on a Visual Board.")]
		[ReadOnlyMember(nameof(IsBucket))]
		[MaxLength(50)]
		public ZString BufferZone2Color
		{
			get { return GetXmlColumnPropertyValue<ZString>(BufferZone2ColorInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(BufferZone2Color, value))
				{
					SetXmlColumnPropertyValue(BufferZone2ColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateBufferZone2Color();
					}
				}
			}
		}

		public ZPropertyInfo BufferZone2ColorInfo
		{
			get { return GetZPropertyInfo(nameof(BufferZone2Color)); }
		}

		[BusinessObjectTestExclude] // We want to return null if no color is specified
		public Color? BufferZone2ColorValue
		{
			get { return string.IsNullOrEmpty(BufferZone2Color) ? null : new Color?(ColorList.ColorFromName(BufferZone2Color)); }
		}

		#endregion

		#region BufferZone1Color

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSectionConfiguration.BufferZone1Color", Caption = "Buffer Zone 1 Color", ShortCaption = "Zone 1", FullDescription = "The Zone 1 background color of a buffer component on a Visual Board.")]
		[ReadOnlyMember(nameof(IsBucket))]
		[MaxLength(50)]
		public ZString BufferZone1Color
		{
			get { return GetXmlColumnPropertyValue<ZString>(BufferZone1ColorInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(BufferZone1Color, value))
				{
					SetXmlColumnPropertyValue(BufferZone1ColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateBufferZone1Color();
					}
				}
			}
		}

		public ZPropertyInfo BufferZone1ColorInfo
		{
			get { return GetZPropertyInfo(nameof(BufferZone1Color)); }
		}

		[BusinessObjectTestExclude] // We want to return null if no color is specified
		public Color? BufferZone1ColorValue
		{
			get { return string.IsNullOrEmpty(BufferZone1Color) ? null : new Color?(ColorList.ColorFromName(BufferZone1Color)); }
		}

		#endregion

		#region BufferZone0Color

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSectionConfiguration.BufferZone0Color", Caption = "Buffer Zone 0 Color", ShortCaption = "Zone 0", FullDescription = "The Zone 0 background color of a buffer component on a Visual Board.")]
		[ReadOnlyMember(nameof(IsBucket))]
		[MaxLength(50)]
		public ZString BufferZone0Color
		{
			get { return GetXmlColumnPropertyValue<ZString>(BufferZone0ColorInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(BufferZone0Color, value))
				{
					SetXmlColumnPropertyValue(BufferZone0ColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateBufferZone0Color();
					}
				}
			}
		}

		public ZPropertyInfo BufferZone0ColorInfo
		{
			get { return GetZPropertyInfo(nameof(BufferZone0Color)); }
		}

		[BusinessObjectTestExclude] // We want to return null if no color is specified
		public Color? BufferZone0ColorValue
		{
			get { return string.IsNullOrEmpty(BufferZone0Color) ? null : new Color?(ColorList.ColorFromName(BufferZone0Color)); }
		}

		#endregion

		#region OverdueBackgroundColor

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSectionConfiguration.OverdueBackgroundColor", Caption = "Overdue Background Color", FullDescription = "The background color of cells in the overdue part of a section.")]
		[MaxLength(50)]
		public ZString OverdueBackgroundColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(OverdueBackgroundColorInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(OverdueBackgroundColor, value))
				{
					SetXmlColumnPropertyValue(OverdueBackgroundColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateOverdueBackgroundColor();
					}
				}
			}
		}

		public ZPropertyInfo OverdueBackgroundColorInfo
		{
			get { return GetZPropertyInfo(nameof(OverdueBackgroundColor)); }
		}

		[BusinessObjectTestExclude] // We want to return null if no color is specified
		public Color? OverdueBackgroundColorValue
		{
			get { return string.IsNullOrEmpty(OverdueBackgroundColor) ? null : new Color?(ColorList.ColorFromName(OverdueBackgroundColor)); }
		}

		#endregion

		#region OverdueForegroundColor

		[XmlColumnProperty]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSectionConfiguration.OverdueForegroundColor", Caption = "Overdue Foreground Color", FullDescription = "The foreground color of cells in the overdue part of a section.")]
		[MaxLength(50)]
		public ZString OverdueForegroundColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(OverdueForegroundColorInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(OverdueForegroundColor, value))
				{
					SetXmlColumnPropertyValue(OverdueForegroundColorInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateOverdueForegroundColor();
					}
				}
			}
		}

		public ZPropertyInfo OverdueForegroundColorInfo
		{
			get { return GetZPropertyInfo(nameof(OverdueForegroundColor)); }
		}

		[BusinessObjectTestExclude] // We want to return null if no color is specified
		public Color? OverdueForegroundColorValue
		{
			get { return string.IsNullOrEmpty(OverdueForegroundColor) ? null : new Color?(ColorList.ColorFromName(OverdueForegroundColor)); }
		}

		#endregion

		#region CountdownTargetBorderColor

		[XmlColumnProperty(DefaultValue = "Red")]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSectionConfiguration.CountdownTargetBorderColor", Caption = "Countdown Target Task Border Color", ShortCaption = "Target Task Border Color", FullDescription = "The border color of tasks which are the target of an active countdown.")]
		[MaxLength(50)]
		public ZString CountdownTargetBorderColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(CountdownTargetBorderColorInfo); }
			set
			{
				SetXmlColumnPropertyValue(CountdownTargetBorderColorInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateCountdownTargetBorderColor();
				}
			}
		}

		public ZPropertyInfo CountdownTargetBorderColorInfo
		{
			get { return GetZPropertyInfo(nameof(CountdownTargetBorderColor)); }
		}

		[BusinessObjectTestExclude] // We want to return null when no color was specified
		public Color? CountdownTargetBorderColorValue
		{
			get { return !CountdownTargetBorderColor.IsEmpty ? ColorList.ColorFromName(CountdownTargetBorderColor) : null; }
		}

		#endregion

		#region CountdownTargetBorderStyle

		[XmlColumnProperty(DefaultValue = "Solid - large")]
		[List("BorderStyles")]
		[ResourceStringData("BMBoardSectionConfiguration.CountdownTargetBorderStyle", Caption = "Countdown Target Task Border Style", ShortCaption = "Target Task Border Style", FullDescription = "The border style of tasks which are the target of an active countdown.")]
		[MaxLength(20)]
		public ZString CountdownTargetBorderStyle
		{
			get { return GetXmlColumnPropertyValue<ZString>(CountdownTargetBorderStyleInfo); }
			set
			{
				SetXmlColumnPropertyValue(CountdownTargetBorderStyleInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateCountdownTargetBorderStyle();
				}
			}
		}

		public ZPropertyInfo CountdownTargetBorderStyleInfo
		{
			get { return GetZPropertyInfo(nameof(CountdownTargetBorderStyle)); }
		}

		public CodeDescriptionPairList BorderStyles
		{
			get { return Factory.GetCachedValue<BorderStyleList>(); }
		}

		public SizedButtonBorderStyle CountdownTargetBorderStyleValue
		{
			get { return !string.IsNullOrEmpty(CountdownTargetBorderStyle) ? SizedButtonBorderStyle.FromCode(CountdownTargetBorderStyle) : null; }
		}

		#endregion

		#region CountdownStartableBorderColor

		[XmlColumnProperty(DefaultValue = "Blue")]
		[List("Lookups.ColorList")]
		[ResourceStringData("BMBoardSectionConfiguration.CountdownStartableBorderColor", Caption = "Countdown Startable Task Border Color", ShortCaption = "Startable Task Border Color", FullDescription = "The border color of tasks which we believe can be started whilst the current user is the target of a countdown.")]
		[MaxLength(50)]
		public ZString CountdownStartableBorderColor
		{
			get { return GetXmlColumnPropertyValue<ZString>(CountdownStartableBorderColorInfo); }
			set
			{
				SetXmlColumnPropertyValue(CountdownStartableBorderColorInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateCountdownStartableBorderColor();
				}
			}
		}

		public ZPropertyInfo CountdownStartableBorderColorInfo
		{
			get { return GetZPropertyInfo(nameof(CountdownStartableBorderColor)); }
		}

		[BusinessObjectTestExclude] // We want to return null when no color was specified
		public Color? CountdownStartableBorderColorValue
		{
			get { return !CountdownStartableBorderColor.IsEmpty ? ColorList.ColorFromName(CountdownStartableBorderColor) : null; }
		}

		#endregion

		#region CountdownStartableBorderStyle

		[XmlColumnProperty(DefaultValue = "Solid - large")]
		[List("BorderStyles")]
		[ResourceStringData("BMBoardSectionConfiguration.CountdownStartableBorderStyle", Caption = "Countdown Startable Task Border Style", ShortCaption = "Startable Task Border Style", FullDescription = "The border style of tasks which we believe can be started whilst the current user is the target of a countdown.")]
		[MaxLength(20)]
		public ZString CountdownStartableBorderStyle
		{
			get { return GetXmlColumnPropertyValue<ZString>(CountdownStartableBorderStyleInfo); }
			set
			{
				SetXmlColumnPropertyValue(CountdownStartableBorderStyleInfo, value);

				if (!IsValidationSuspended)
				{
					Validation.ValidateCountdownStartableBorderStyle();
				}
			}
		}

		public ZPropertyInfo CountdownStartableBorderStyleInfo
		{
			get { return GetZPropertyInfo(nameof(CountdownStartableBorderStyle)); }
		}

		public SizedButtonBorderStyle CountdownStartableBorderStyleValue
		{
			get { return !string.IsNullOrEmpty(CountdownStartableBorderStyle) ? SizedButtonBorderStyle.FromCode(CountdownStartableBorderStyle) : null; }
		}

		#endregion

		#endregion

		#region Channel config

		#region ChannelBy

		[XmlColumnProperty(SerialiseDefaultValues = true, DefaultValue = "")]
		[List("Lookups.ChannelByList")]
		[ResourceStringData("BMBoardSectionConfiguration.ChannelBy", Caption = "Channel By", FullDescription = "Select which type of channels to display on this axis of this board section.")]
		[MaxLength(3)]
		public ZString ChannelBy
		{
			get { return GetXmlColumnPropertyValue<ZString>(ChannelByInfo); }
			set
			{
				var originalValue = ChannelBy;

				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(originalValue, value))
				{
					if (originalValue != value && !OverrideChannels && section != null)
					{
						DefaultChannelsProvider.AddMissingDefaultChannels(this, PrimaryAxisChannels, value, OverrideChannels);
					}

					SetXmlColumnPropertyValue(ChannelByInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateChannelBy();
						Validation.ValidateShowWorkInReleaseGroupOnly();
					}
				}
			}
		}

		public ZPropertyInfo ChannelByInfo
		{
			get { return GetZPropertyInfo(nameof(ChannelBy)); }
		}

		protected bool ChannelBy_ReadOnly
		{
			get { return OverrideChannels && !IsReleaseScheduler; }
		}

		#endregion

		#region OverrideChannels

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.OverrideChannels", Caption = "Override Channels", FullDescription = "Use this option to override the default channels and add or remove any channels of any type.")]
		public ZBool OverrideChannels
		{
			get { return GetXmlColumnPropertyValue<ZBool>(OverrideChannelsInfo); }
			set
			{
				var oldValue = OverrideChannels;

				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(oldValue, value))
				{
					SetXmlColumnPropertyValue(OverrideChannelsInfo, value);

					if (oldValue != value)
					{
						if (value)
						{
							ChannelBy = ZString.Empty;
						}
						else
						{
							ShowUnchanneled = false;
						}
					}

					if (!IsValidationSuspended)
					{
						Validation.ValidateOverrideChannels();
					}
				}
			}
		}

		public ZPropertyInfo OverrideChannelsInfo
		{
			get { return GetZPropertyInfo(nameof(OverrideChannels)); }
		}

		#endregion

		#region ShowUnchanneled

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.ShowUnchanneled", Caption = "Show Un-channeled", FullDescription = "Shows a channel for all valid tasks which do not belong in any channel.")]
		public ZBool ShowUnchanneled
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowUnchanneledInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(ShowUnchanneled, value))
				{
					if (ShowUnchanneled != value)
					{
						UpdateUnchanneledChannel(value, PrimaryAxisChannels, ChannelBy);
					}

					SetXmlColumnPropertyValue(ShowUnchanneledInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateShowUnchanneled();
					}
				}
			}
		}

		public ZPropertyInfo ShowUnchanneledInfo
		{
			get { return GetZPropertyInfo(nameof(ShowUnchanneled)); }
		}

		void UpdateUnchanneledChannel(bool showUnchanneled, BMBoardSectionChannelCollection channels, ZString channelBy)
		{
			var unchanneledChannel = channels.Cast<BMBoardSectionChannel>().FirstOrDefault(c => c.IsUnChanneled);
			if (unchanneledChannel != null && !showUnchanneled)
			{
				unchanneledChannel.Delete();
			}
			else if (unchanneledChannel == null && showUnchanneled)
			{
				unchanneledChannel = channels.AddNew();
				unchanneledChannel.IsUnChanneled = true;
				unchanneledChannel.MSC_ChannelType = channelBy;
			}
		}

		#endregion

		#region ChannelSecondaryBy

		[XmlColumnProperty(SerialiseDefaultValues = true, DefaultValue = "")]
		[List("Lookups.ChannelSecondaryByList")]
		[ResourceStringData("BMBoardSectionConfiguration.ChannelBy", Caption = "Channel By", FullDescription = "Select which type of channels to display on this axis of this board section.")]
		[MaxLength(3)]
		public ZString ChannelSecondaryBy
		{
			get { return GetXmlColumnPropertyValue<ZString>(ChannelSecondaryByInfo); }
			set
			{
				var originalValue = ChannelSecondaryBy;

				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(originalValue, value))
				{
					if (originalValue != value && !OverrideSecondaryChannels && section != null)
					{
						DefaultChannelsProvider.AddMissingDefaultChannels(this, SecondaryAxisChannels, value, OverrideSecondaryChannels);
					}

					SetXmlColumnPropertyValue(ChannelSecondaryByInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateChannelSecondaryBy();
						Validation.ValidateTimePerCell();
						Validation.ValidateShowWorkInReleaseGroupOnly();
					}
				}
			}
		}

		public ZPropertyInfo ChannelSecondaryByInfo
		{
			get { return GetZPropertyInfo(nameof(ChannelSecondaryBy)); }
		}

		protected bool ChannelSecondaryBy_ReadOnly
		{
			get { return OverrideSecondaryChannels && !IsReleaseScheduler; }
		}

		#endregion

		#region OverrideSecondaryChannels

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.OverrideChannels", Caption = "Override Channels", FullDescription = "Use this option to override the default channels and add or remove any channels of any type.")]
		public ZBool OverrideSecondaryChannels
		{
			get { return GetXmlColumnPropertyValue<ZBool>(OverrideSecondaryChannelsInfo); }
			set
			{
				var oldValue = OverrideSecondaryChannels;

				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(oldValue, value))
				{
					SetXmlColumnPropertyValue(OverrideSecondaryChannelsInfo, value);

					if (oldValue != value)
					{
						if (value)
						{
							ChannelSecondaryBy = ZString.Empty;
						}
						else
						{
							ShowSecondaryUnchanneled = false;
						}
					}
					if (!IsValidationSuspended)
					{
						Validation.ValidateOverrideSecondaryChannels();
						Validation.ValidateChannelSecondaryBy();
					}
				}
			}
		}

		public ZPropertyInfo OverrideSecondaryChannelsInfo
		{
			get { return GetZPropertyInfo(nameof(OverrideSecondaryChannels)); }
		}

		#endregion

		#region ShowSecondaryUnchanneled

		[XmlColumnProperty]
		[ResourceStringData("BMBoardSectionConfiguration.ShowUnchanneled", Caption = "Show Un-channeled", FullDescription = "Shows a channel for all valid tasks which do not belong in any channel.")]
		public ZBool ShowSecondaryUnchanneled
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowSecondaryUnchanneledInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(ShowSecondaryUnchanneled, value))
				{
					if (ShowSecondaryUnchanneled != value)
					{
						UpdateUnchanneledChannel(value, SecondaryAxisChannels, ChannelSecondaryBy);
					}

					SetXmlColumnPropertyValue(ShowSecondaryUnchanneledInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateShowSecondaryUnchanneled();
					}
				}
			}
		}

		public ZPropertyInfo ShowSecondaryUnchanneledInfo
		{
			get { return GetZPropertyInfo(nameof(ShowSecondaryUnchanneled)); }
		}

		#endregion

		#region SortPrimaryChannels

		[XmlColumnProperty(DefaultValue = true)]
		[ResourceStringData("BMBoardSectionConfiguration.SortPrimaryChannels", Caption = "Display Channels Alphabetically", FullDescription = "Display these channels on a Visual Board in alphabetical order according to their description.")]
		public ZBool SortPrimaryChannels
		{
			get { return GetXmlColumnPropertyValue<ZBool>(SortPrimaryChannelsInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(SortPrimaryChannels, value))
				{
					SetXmlColumnPropertyValue(SortPrimaryChannelsInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateSortPrimaryChannels();
					}
				}
			}
		}

		public ZPropertyInfo SortPrimaryChannelsInfo
		{
			get { return GetZPropertyInfo(nameof(SortPrimaryChannels)); }
		}

		#endregion

		#region SortSecondaryChannels

		[XmlColumnProperty(DefaultValue = true)]
		[ResourceStringData("BMBoardSectionConfiguration.SortSecondaryChannels", Caption = "Display Channels Alphabetically", FullDescription = "Display these channels on a Visual Board in alphabetical order according to their description.")]
		public ZBool SortSecondaryChannels
		{
			get { return GetXmlColumnPropertyValue<ZBool>(SortSecondaryChannelsInfo); }
			set
			{
				using (FireLayoutConfigUpdatedOnDisposeIfValueChanged(SortSecondaryChannels, value))
				{
					SetXmlColumnPropertyValue(SortSecondaryChannelsInfo, value);

					if (!IsValidationSuspended)
					{
						Validation.ValidateSortSecondaryChannels();
					}
				}
			}
		}

		public ZPropertyInfo SortSecondaryChannelsInfo
		{
			get { return GetZPropertyInfo(nameof(SortSecondaryChannels)); }
		}

		#endregion

		#endregion

		#region Section Filter Rules

		public StmModuleFilter WorkflowFilter => section.WorkflowFilter;
		public StmModuleFilter TaskFilter => section.TaskFilter;

		public ZQuery GetQuery(ModuleIdentifier moduleID, StmModuleFilter filter)
		{
			var filterBusinessObject = RelatedModuleFiltersHelper.GetNewFilterBusinessObject(moduleID);
			(filterBusinessObject as BusinessObject).SetContext(this.GetContexts<BufferManagementBusinessContext>().FirstOrDefault());
			return filterBusinessObject.GetFilter(filter);
		}

		#endregion

		#endregion

		#region New Properties

		public bool IsBuffer
		{
			get { return !IsReleaseScheduler && Section.Component != null && Section.Component.IsBuffer; }
		}

		public bool IsBucket
		{
			get { return Section.Component != null && Section.Component.IsBucket; }
		}

		public int CellsInZoneZero
		{
			get
			{
				var totalCells = CellsPerSubsection * Subsections;
				if (totalCells % BMConstants.NumberOfZonesIncludingZoneZero == 0)
				{
					return totalCells / BMConstants.NumberOfZonesIncludingZoneZero;
				}
				else if (totalCells % BMConstants.NumberOfZones == 0)
				{
					return BMConstants.NumberOfZones; // We don't want to end up with no cells in zone zero
				}
				else
				{
					return totalCells % BMConstants.NumberOfZones;
				}
			}
		}

		#region ApplicableReleaseGroupPK

		public ZGuid ApplicableReleaseGroupPK
		{
			get
			{
				return !ReleaseGroupPK.IsEmpty ? ReleaseGroupPK
					: !Section.MS_GG_ReleaseGroup.IsEmpty ? Section.MS_GG_ReleaseGroup
					: Section.Board != null && !Section.Board.MB_GG_ReleaseGroup.IsEmpty ? Section.Board.MB_GG_ReleaseGroup
					: ZGuid.Empty;
			}
		}

		#endregion

		#region Orientation

		BMBoardSectionOrientation? OrientationCore
		{
			get
			{
				switch (FlowDirection)
				{
					case FlowDirectionList.Codes.Left:
					case FlowDirectionList.Codes.Right:
						return BMBoardSectionOrientation.Horizontal;

					case FlowDirectionList.Codes.Up:
					case FlowDirectionList.Codes.Down:
						return BMBoardSectionOrientation.Vertical;

					default:
						return null;
				}
			}
		}

		public BMBoardSectionOrientation OrientationValue
		{
			get { return OrientationCore ?? BMBoardSectionOrientation.Vertical; }
		}

		[ResourceStringData("BMBoardSectionConfiguration.Orientation", Caption = "Orientation")]
		[ReadOnly(true)]
		public ZString Orientation
		{
			get { return OrientationCore != null ? OrientationCore.ToString() : string.Empty; }
		}

		#endregion

		#endregion

		#region Related Business Objects

		public GlbGroup ReleaseGroup
		{
			get { return Factory.Load<GlbGroup>(ApplicableReleaseGroupPK); }
		}

		[ChildEditable]
		public BMBoardSectionAdditionalComponentCollection AdditionalComponents
		{
			get
			{
				if (additionalComponents == null)
				{
					additionalComponents = new BMBoardSectionAdditionalComponentCollection(Section);
					RegisterEditableChildObject(additionalComponents);
				}

				return additionalComponents;
			}
		}

		BMBoardSectionAdditionalComponentCollection additionalComponents;

		[ChildEditable]
		public BMControlCustomisationLinkCollection CustomisedLayoutLinks
		{
			get
			{
				if (customisedLayoutLinks == null)
				{
					customisedLayoutLinks = new BMControlCustomisationLinkCollection(Section);
					RegisterEditableChildObject(customisedLayoutLinks);
				}

				return customisedLayoutLinks;
			}
		}

		BMControlCustomisationLinkCollection customisedLayoutLinks;

		public ApplicableCustomisedLayoutCollection ApplicableLayouts
		{
			get
			{
				if (applicableLayouts == null)
				{
					applicableLayouts = new ApplicableCustomisedLayoutCollection(this);
				}

				return applicableLayouts;
			}
		}

		ApplicableCustomisedLayoutCollection applicableLayouts;

		[ChildEditable]
		[XmlColumnProperty]
		public BoardSectionAcceptabilityBandCollection AcceptabilityBands
		{
			get
			{
				if (acceptabilityBands == null)
				{
					acceptabilityBands = new BoardSectionAcceptabilityBandCollection(Section);
					RegisterEditableChildObject(acceptabilityBands);
				}

				return acceptabilityBands;
			}
		}

		BoardSectionAcceptabilityBandCollection acceptabilityBands;

		public IEnumerable<BoardSectionAcceptabilityBand> HeadingAcceptabilityBands => AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Where(x => x.ShouldShowInHeading);

		public IEnumerable<BoardSectionAcceptabilityBand> TileAcceptabilityBands => AcceptabilityBands.Cast<BoardSectionAcceptabilityBand>().Where(x => x.ShouldShowAsTile);

		#region Channels

		public BMBoardSectionChannelsViewModel PrimaryChannelsViewModel
		{
			get { return primaryChannelsViewModel ?? (primaryChannelsViewModel = new BMBoardSectionChannelsViewModel(this, ChannelAxis.Primary)); }
		}
		BMBoardSectionChannelsViewModel primaryChannelsViewModel;

		public BMBoardSectionChannelsViewModel SecondaryChannelsViewModel
		{
			get { return secondaryChannelsViewModel ?? (secondaryChannelsViewModel = new BMBoardSectionChannelsViewModel(this, ChannelAxis.Secondary)); }
		}
		BMBoardSectionChannelsViewModel secondaryChannelsViewModel;

		[ChildEditable]
		public BMBoardSectionChannelCollection PrimaryAxisChannels
		{
			get
			{
				if (primaryAxisChannels == null)
				{
					primaryAxisChannels = new BMBoardSectionChannelCollection(Section, ChannelAxisCodeList.Codes.Primary);
					RegisterEditableChildObject(primaryAxisChannels);
				}

				return primaryAxisChannels;
			}
		}
		BMBoardSectionChannelCollection primaryAxisChannels;

		public IEnumerable<BMBoardSectionChannel> OrderedPrimaryAxisChannels => GetOrderedChannels(PrimaryAxisChannels, ChannelAxisCodeList.Codes.Primary);

		[ChildEditable]
		public BMBoardSectionChannelCollection SecondaryAxisChannels
		{
			get
			{
				if (secondaryAxisChannels == null)
				{
					secondaryAxisChannels = new BMBoardSectionChannelCollection(Section, ChannelAxisCodeList.Codes.Secondary);
					RegisterEditableChildObject(secondaryAxisChannels);
				}

				return secondaryAxisChannels;
			}
		}
		BMBoardSectionChannelCollection secondaryAxisChannels;

		public IEnumerable<BMBoardSectionChannel> OrderedSecondaryAxisChannels => GetOrderedChannels(SecondaryAxisChannels, ChannelAxisCodeList.Codes.Secondary);

		public IEnumerable<BMBoardSectionChannel> Channels => PrimaryAxisChannels.Union(SecondaryAxisChannels);

		IEnumerable<BMBoardSectionChannel> GetOrderedChannels(IEnumerable<BMBoardSectionChannel> channelsToSort, string channelAxisType)
		{
			var shouldBeSortedAlphabetically = channelAxisType == ChannelAxisCodeList.Codes.Primary ? SortPrimaryChannels : SortSecondaryChannels;

			return shouldBeSortedAlphabetically
				? channelsToSort.OrderBy(x => x.BizoDescription)
				: channelsToSort.OrderBy(x => x.MSC_Sequence);
		}

		#endregion

		public BMBoard Board
		{
			get { return Section.Board; }
		}

		#endregion

		#region Lookups

		public BMComponentSectionConfigurationLookups Lookups
		{
			get
			{
				if (fLookups == null || !IsLookupsCachedInBase)
				{
					fLookups = GetNewLookups();
				}

				return fLookups;
			}
		}

		protected BMComponentSectionConfigurationLookups GetNewLookups()
		{
			return new BMComponentSectionConfigurationLookups(this);
		}

		BMComponentSectionConfigurationLookups fLookups;

		#endregion

		#region Helper Functions

		public int GetPrimaryAxis(int row, int col)
		{
			return OrientationValue == BMBoardSectionOrientation.Vertical ? col : row;
		}

		public int GetSecondaryAxis(int row, int col)
		{
			return OrientationValue == BMBoardSectionOrientation.Vertical ? row : col;
		}

		public bool FlowsInSameDirectionAsAxis()
		{
			switch (FlowDirection)
			{
				case FlowDirectionList.Codes.Right:
				case FlowDirectionList.Codes.Down:
					return true;
				default:
					return false;
			}
		}

		public ConstraintStatus GetCCRStatus(int ccrLineSecondaryAxis, int cellSecondaryAxis)
		{
			if (FlowsInSameDirectionAsAxis())
			{
				// given constraint-line secondaryAxis = 1 then cell secondaryAxis = 1 is pre-constraint
				return cellSecondaryAxis < ccrLineSecondaryAxis
					? ConstraintStatus.PreConstraint
					: ConstraintStatus.PostConstraint;
			}
			else
			{
				// given constraint-line secondaryAxis = 13 then cell secondaryAxis = 13 is pre-constraint
				return cellSecondaryAxis > ccrLineSecondaryAxis
					? ConstraintStatus.PreConstraint
					: ConstraintStatus.PostConstraint;
			}
		}

		#endregion

		#region BMBoardSectionConfigurationBase Members

		public override ZString SectionNameOverride
		{
			get
			{
				return base.SectionNameOverride;
			}

			set
			{
				base.SectionNameOverride = value;
				if (!IsValidationSuspended)
				{
					Validation.ValidateSectionNameOverride();
				}
			}
		}

		public override ZString DefaultSectionName
		{
			get
			{
				var component = section.Component;

				if (component != null && section.SectionConfiguration.IsReleaseScheduler)
				{
					return Res.GetString("cd561323-e094-4f46-811c-cd004c7e639c", "Release Gate for {0}", component.FC_Name);
				}
				else
				{
					if (component != null)
					{
						var parentComponent = component.ParentComponent;
						return parentComponent != null
						? string.Format(CultureInfo.InvariantCulture, "{0}: {1}", parentComponent.FC_Name, component.FC_Name)
							: string.Join(", ", section.AllComponents.Select(c => c.FC_Name));
					}
					else
					{
						return string.Empty;
					}
				}
			}
		}

		#endregion
	}
}
