using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.Business
{
	public class ModuleGridSectionPanelConfiguration : NonPersistentBusinessObject<ModuleGridSectionPanelConfigurationValidation>
	{
		public ModuleGridSectionPanelConfiguration(BMBoardSection section)
			: base(section.Factory)
		{
			Section = section;

			if (Section.Configuration is ModuleGridSectionConfiguration)
			{
				var panelConfigs = (Section.Configuration as ModuleGridSectionConfiguration).PanelConfigurations;
				if (panelConfigs != null && panelConfigs.Any())
				{
					using (SuspendSettingHasChanges())
					{
						Sequence = panelConfigs.Max(p => (p as ModuleGridSectionPanelConfiguration).Sequence) + 1;
					}
				}
			}
		}

		#region BusinessObject Overrides

		public override ModuleGridSectionPanelConfigurationValidation GetNewValidation()
		{
			return new ModuleGridSectionPanelConfigurationValidation(this);
		}

		#endregion

		#region XML Properties

		#region ModuleName

		[XmlColumnProperty]
		[List("Lookups.AllModules")]
		[ResourceStringData("ModuleGridSectionPanelConfiguration.ModuleName", Caption = "Module Name", FullDescription = "The name of the module grid to be shown in this section panel.")]
		public ZString ModuleName
		{
			get { return GetXmlColumnPropertyValue<ZString>(ModuleNameInfo); }
			set
			{
				FilterLayout = ZGuid.Empty;
				SetXmlColumnPropertyValue(ModuleNameInfo, value);
			}
		}

		public ZPropertyInfo ModuleNameInfo
		{
			get { return GetZPropertyInfo(nameof(ModuleName)); }
		}

		#endregion

		#region Sequence

		[XmlColumnProperty]
		[ResourceStringData("ModuleGridSectionPanelConfiguration.Sequence", Caption = "Sequence", FullDescription = "Specifies panel sequence number.")]
		public ZInt Sequence
		{
			get { return GetXmlColumnPropertyValue<ZInt>(SequenceInfo); }
			set
			{
				SetXmlColumnPropertyValue(SequenceInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSequence();
				}
			}
		}

		public ZPropertyInfo SequenceInfo
		{
			get { return GetZPropertyInfo(nameof(Sequence)); }
		}

		#endregion

		#region FilterLayout
		[XmlColumnProperty]
		[List("Lookups.ModuleFilters")]
		[ResourceStringData("ModuleGridSectionPanelConfiguration.FilterLayout", Caption = "Filter Layout", FullDescription = "Select the filter strip layout that will be used by default when displaying this board section panel.")]
		public ZGuid FilterLayout
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(FilterLayoutInfo); }
			set
			{
				SetXmlColumnPropertyValue(FilterLayoutInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateFilterLayout();
				}
			}
		}

		public ZPropertyInfo FilterLayoutInfo
		{
			get { return GetZPropertyInfo(nameof(FilterLayout)); }
		}

		#endregion

		#region SectionNameIsOverridden

		[XmlColumnProperty]
		[ResourceStringData("ModuleGridSectionPanelConfiguration.SectionNameIsOverridden", Caption = "Use Custom Panel Name", FullDescription = "Specifies whether to use the custom panel name.")]
		public ZBool SectionNameIsOverridden
		{
			get { return GetXmlColumnPropertyValue<ZBool>(SectionNameIsOverriddenInfo); }
			set { SetXmlColumnPropertyValue(SectionNameIsOverriddenInfo, value); }
		}

		public ZPropertyInfo SectionNameIsOverriddenInfo
		{
			get { return GetZPropertyInfo(nameof(SectionNameIsOverridden)); }
		}

		#endregion

		#region SectionNameOverride

		[XmlColumnProperty]
		[ReadOnlyMember(nameof(CustomPanelNameIsReadOnly))]
		[ResourceStringData("ModuleGridSectionPanelConfiguration.SectionNameOverride", Caption = "Custom Panel Name", FullDescription = "Specifies the custom panel name.")]
		public ZString SectionNameOverride
		{
			get { return GetXmlColumnPropertyValue<ZString>(SectionNameOverrideInfo); }
			set
			{
				SetXmlColumnPropertyValue(SectionNameOverrideInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateSectionNameOverride();
				}
			}
		}

		public ZPropertyInfo SectionNameOverrideInfo
		{
			get { return GetZPropertyInfo(nameof(SectionNameOverride)); }
		}

		protected bool CustomPanelNameIsReadOnly => !SectionNameIsOverridden;

		#endregion

		#region ShowFilters

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		[ResourceStringData("ModuleGridSectionPanelConfiguration.ShowFilters", Caption = "Show Filters", FullDescription = "Specifies whether filter strips are visible by default.")]
		public ZBool ShowFilters
		{
			get { return GetXmlColumnPropertyValue<ZBool>(ShowFiltersInfo); }
			set { SetXmlColumnPropertyValue(ShowFiltersInfo, value); }
		}

		public ZPropertyInfo ShowFiltersInfo
		{
			get { return GetZPropertyInfo(nameof(ShowFilters)); }
		}

		#endregion

		#region AllowFilterToggle

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		[ResourceStringData("ModuleGridSectionPanelConfiguration.AllowFilterToggle", Caption = "Allow Filter Toggle", FullDescription = "Specifies whether to allow filter toggle.")]
		public ZBool AllowFilterToggle
		{
			get { return GetXmlColumnPropertyValue<ZBool>(AllowFilterToggleInfo); }
			set { SetXmlColumnPropertyValue(AllowFilterToggleInfo, value); }
		}

		public ZPropertyInfo AllowFilterToggleInfo
		{
			get { return GetZPropertyInfo(nameof(AllowFilterToggle)); }
		}

		#endregion

		#region AllowOpenModule

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		[ResourceStringData("ModuleGridSectionPanelConfiguration.AllowOpenModule", Caption = "Allow Open Module", FullDescription = "Specifies whether to allow to open module in a new window.")]
		public ZBool AllowOpenModule
		{
			get { return GetXmlColumnPropertyValue<ZBool>(AllowOpenModuleInfo); }
			set { SetXmlColumnPropertyValue(AllowOpenModuleInfo, value); }
		}

		public ZPropertyInfo AllowOpenModuleInfo
		{
			get { return GetZPropertyInfo(nameof(AllowOpenModule)); }
		}

		#endregion

		#region AllowFilterEdit

		[XmlColumnProperty(SerialiseDefaultValues = true)]
		[ResourceStringData("ModuleGridSectionPanelConfiguration.AllowFilterEdit", Caption = "Allow Filter Edit", FullDescription = "Specifies whether to allow to edit filters.")]
		public ZBool AllowFilterEdit
		{
			get { return GetXmlColumnPropertyValue<ZBool>(AllowFilterEditInfo); }
			set { SetXmlColumnPropertyValue(AllowFilterEditInfo, value); }
		}

		public ZPropertyInfo AllowFilterEditInfo
		{
			get { return GetZPropertyInfo(nameof(AllowFilterEdit)); }
		}

		#endregion

		#endregion

		#region Related BusinessObjects

		public BMBoardSection Section { get; }

		#endregion

		#region Set Default Values

		protected override void SetDefaultValues()
		{
			using (SuspendSettingHasChanges())
			{
				base.SetDefaultValues();

				ModuleName = string.Empty;
				Sequence = 1;
				SectionNameIsOverridden = false;
				SectionNameOverride = string.Empty;
				FilterLayout = ZGuid.Empty;
				ShowFilters = true;
				AllowFilterToggle = true;
				AllowOpenModule = true;
				AllowFilterEdit = true;
			}
		}

		#endregion

		#region Lookups

		public ModuleGridSectionPanelConfigurationLookups Lookups
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

		protected ModuleGridSectionPanelConfigurationLookups GetNewLookups()
		{
			return new ModuleGridSectionPanelConfigurationLookups(this);
		}

		ModuleGridSectionPanelConfigurationLookups fLookups;

		#endregion

		public ZString PanelName
		{
			get
			{
				if (SectionNameIsOverridden)
				{
					return SectionNameOverride;
				}

				if (FilterLayout != null)
				{
					var filter = Factory.Load<StmModuleFilter>(FilterLayout);
					if (filter != null)
					{
						return filter.S9_FilterName;
					}
				}

				var moduleId = ModuleIDs.AllIncludingClientModules.FirstOrDefault(x => x.Name == ModuleName);
				if (moduleId != null)
				{
					return moduleId.Description;
				}

				return ModuleName;
			}
		}
	}
}
