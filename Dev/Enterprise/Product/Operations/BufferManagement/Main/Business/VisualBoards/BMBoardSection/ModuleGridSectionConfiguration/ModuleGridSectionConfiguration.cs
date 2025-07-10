using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.BufferManagement.Business
{
	[CodeProperty("StmModuleFilterSchema.S9_ModuleID")]
	[DescriptionProperty("SectionName")]
	public class ModuleGridSectionConfiguration : BMBoardSectionConfigurationWithOverridableSectionName<ModuleGridSectionConfigurationValidation>
	{
		public ModuleGridSectionConfiguration(IBMBoardSection section)
			: base(section.Factory)
		{
			Section = section as BMBoardSection;
		}

		#region BusinessObject Overrides

		public override ModuleGridSectionConfigurationValidation GetNewValidation()
		{
			return new ModuleGridSectionConfigurationValidation(this);
		}

		#endregion

		#region XML Properties

		#region PanelsConfigurations

		[ChildEditable]
		[XmlColumnProperty]
		public ModuleGridSectionPanelConfigurationCollection PanelConfigurations
		{
			get
			{
				if (panelConfigurations == null)
				{
					panelConfigurations = new ModuleGridSectionPanelConfigurationCollection(Section);
					RegisterEditableChildObject(panelConfigurations);
				}
				return panelConfigurations;
			}
		}

		ModuleGridSectionPanelConfigurationCollection panelConfigurations;

		#endregion

		#endregion

		#region Related BusinessObjects

		public BMBoard Board
		{
			get { return Section.Board; }
		}

		public BMBoardSection Section { get; }

		#endregion

		#region Lookups

		public ModuleGridSectionConfigurationLookups Lookups
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

		protected ModuleGridSectionConfigurationLookups GetNewLookups()
		{
			return new ModuleGridSectionConfigurationLookups(this);
		}

		ModuleGridSectionConfigurationLookups fLookups;

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
			get { return string.Empty; }
		}

		public override void CopyConfigurationPropertiesToNewSection(IBMBoardSection boardSection)
		{
		}

		#endregion

		[List("Lookups.AllPanels")]
		public ZString PanelID
		{
			get { return new ZString(currentPanelID); }
			set
			{
				using (SuspendSettingHasChanges())
				{
					SetNonPersistentPropertyValue(PanelIDInfo, ref currentPanelID, value);
				}
			}
		}

		public ZPropertyInfo PanelIDInfo
		{
			get { return GetZPropertyInfo(nameof(PanelID)); }
		}

		ZString currentPanelID;
	}
}
