using System;
using System.Collections.Generic;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public delegate void ConfigurationSavedOrDeletedEventHandler(ColumnConfigurationManager saved);

	public class ColumnConfigurationsManager : IColumnSettingXMLVersionUpgraderV1Parameters, IJsonSerializable
	{
		readonly CompanyDefaultConfigurationManager companyDefaultConfigurationManager;
		SavedScheduleConfigurationManager savedScheduleSetting;
		ReportColumnSettings configuration;
		ColumnConfigurationManager currentColumnConfigurationManager;
		ZGuid reportID;
		readonly bool scheduled;
		string saveToFilterField;
		bool isReportTitleChangeable;
		readonly DefaultTemplateConfigurationManager defaultTemplateConfigurationManager;

		public ColumnConfigurationsManager(ZGuid reportID, bool scheduled)
		{
			this.isReportTitleChangeable = false;
			this.companyDefaultConfigurationManager = new CompanyDefaultConfigurationManager(this);
			this.reportID = reportID;
			this.scheduled = scheduled;
			this.defaultTemplateConfigurationManager = new DefaultTemplateConfigurationManager(this);
		}

		public void AddLinkedField(FilterField linkedFilterField)
		{
			LinkedFilterFields.Add(linkedFilterField);
		}
		public readonly CollectionOfIFilter LinkedFilterFields = new CollectionOfIFilter();

		internal string headingsxml { get; private set; }

		#region Constructor For IJsonSerializable

		internal ColumnConfigurationsManager(ColumnConfigurationsManagerJsonData data)
		{
			headingsxml = data.Headings;
		}

		#endregion

		public void RefreshCurrentConfiguration(ReportColumnSettings configuration, ColumnConfigurationManager manager)
		{
			this.configuration = configuration;
			currentColumnConfigurationManager = manager;
			FireOnColumnConfigurationLoaded();
		}

		public bool ConfigurationExists(ColumnConfigurationManager manager)
		{
			bool result = false;
			foreach (ColumnConfigurationManager existingManager in this.ConfigurationManagersForAllSavedConfigurations)
			{
				if (existingManager.Equals(manager))
				{
					result = true;
					break;
				}
			}
			return result;
		}

		public CompanyDefaultConfigurationManager CompanyDefaultConfigurationManager
		{
			get { return companyDefaultConfigurationManager; }
		}

		public ReportColumnSettings CurrentConfiguration
		{
			get
			{
				if (configuration == null)
				{
					if (string.IsNullOrEmpty(headingsxml))
					{
						configuration = new ReportColumnSettings();
					}
					else
					{
						if (reportID.IsEmpty || !reportID.IsValid)
						{
							throw new DocumentEngineException("No report ID for deserialised scheduled report.");
						}
						ColumnSettingXMLVersionUpgraderCurrentVersionParameters parameters = new ColumnSettingXMLVersionUpgraderCurrentVersionParameters(this);
						configuration = ColumnHeadingListSerialiser.Deserialise(headingsxml, parameters);
					}
				}
				return configuration;
			}
		}

		public ColumnConfigurationManager CurrentColumnConfigurationManager
		{
			get
			{
				return currentColumnConfigurationManager;
			}
		}

		public bool IsEmpty
		{
			get { return CurrentConfiguration.Worksheets.IsEmpty; }
		}

		public ZGuid ReportID
		{
			get { return reportID; }
		}

		public bool Scheduled
		{
			get { return scheduled; }
		}

		public string SaveToFilterField
		{
			get { return saveToFilterField; }
			set { saveToFilterField = value; }
		}

		public string ColumnHeadingCellContent
		{
			get;
			set;
		}

		public bool IsReportTitleChangeable
		{
			get { return this.isReportTitleChangeable; }
			set { this.isReportTitleChangeable = value; }
		}

		public DefaultTemplateConfigurationManager DefaultTemplateConfigurationManager
		{
			get { return defaultTemplateConfigurationManager; }
		}

		public CollectionOfIFilter DefaultFilters { get; internal set; }

		public event EventHandler CurrentConfigurationRefreshed;
		public event ConfigurationSavedOrDeletedEventHandler ConfigurationSaved;
		public event ConfigurationSavedOrDeletedEventHandler ConfigurationDeleted;

		public void FireOnColumnConfigurationLoaded()
		{
			if (CurrentConfigurationRefreshed != null)
			{
				CurrentConfigurationRefreshed(this, EventArgs.Empty);
			}
		}

		public void FireOnColumnConfigurationSaved(ColumnConfigurationManager saved)
		{
			if (!ConfigurationManagersForAllSavedConfigurations.Contains(saved))
			{
				if (typeof(CombinedConfigurationManager).IsAssignableFrom(saved.GetType()))
				{
					CombinedConfigurationManagers.Add((CombinedConfigurationManager)saved);
					CombinedConfigurationManagers.Sort();
				}
			}
			if (ConfigurationSaved != null)
			{
				ConfigurationSaved(saved);
			}
		}

		public void FireOnColumnConfigurationDeleted(ColumnConfigurationManager deleted)
		{
			if (deleted.GetType().IsAssignableFrom(typeof(CombinedConfigurationManager)))
			{
				CombinedConfigurationManagers.Remove((CombinedConfigurationManager)deleted);
			}
			if (ConfigurationDeleted != null)
			{
				ConfigurationDeleted(deleted);
			}
		}

		public string GetHeadingText(string workSheetName, string displayLabel)
		{
			if (CurrentConfiguration.Worksheets.Contains(workSheetName))
			{
				foreach (ColumnHeading heading in CurrentConfiguration.Worksheets[workSheetName].ColumnHeadings)
				{
					if (string.Equals(heading.DisplayLabel, displayLabel, StringComparison.OrdinalIgnoreCase))
					{
						return heading.HeadingText;
					}
				}
			}

			return string.Empty;
		}

		public List<ColumnConfigurationManager> ConfigurationManagersForAllSavedConfigurations
		{
			get
			{
				if (CombinedConfigurationManagers == null)
				{
					GetConfigurationManagersForAllSavedConfigurations();
				}
				List<ColumnConfigurationManager> settings = new List<ColumnConfigurationManager>();
				if (savedScheduleSetting != null)
				{
					settings.Add(savedScheduleSetting);
				}
				settings.Add(this.defaultTemplateConfigurationManager);
				settings.Add(this.companyDefaultConfigurationManager);
				settings.AddRange(CombinedConfigurationManagers.ToArray());
				return settings;
			}
		}

		void GetConfigurationManagersForAllSavedConfigurations()
		{
			CombinedConfigurationManagers = new List<CombinedConfigurationManager>();
			List<ReportColumnSettingKeys> savedSettings = Globals.IsWeb ?
				DocumentsDataRegistry.Instance.ReportColumnSettings.GetSavedColumnSettingKeysForAllCompanies(this.reportID) :
				DocumentsDataRegistry.Instance.ReportColumnSettings.GetSavedColumnSettingKeysForCurrentCompany(this.reportID);

			LookupFilterFieldBase lookupField = LinkedFilterFields[SaveToFilterField] as LookupFilterFieldBase;
			if (lookupField != null)
			{
				var collection = lookupField.CollectionProvider.Collection;
				var factory = collection.Factory;
				string tableName = collection.TableName;
				foreach (ReportColumnSettingKeys savedSetting in savedSettings)
				{
					factory.AddFetchHint(tableName, savedSetting.LinkID);
				}
			}

			foreach (ReportColumnSettingKeys savedSetting in savedSettings)
			{
				LoadConfigFromKeys(savedSetting);
			}
			CombinedConfigurationManagers.Sort();
		}

		List<CombinedConfigurationManager> CombinedConfigurationManagers;

		public void SetCombinedConfigurationManagers(List<CombinedConfigurationManager> value)
		{
			CombinedConfigurationManagers = value;
		}

		public List<CombinedConfigurationManager> GetCombinedConfigurationManagers()
		{
			return CombinedConfigurationManagers;
		}

		internal CombinedConfigurationManager LoadConfigFromKeys(ReportColumnSettingKeys savedSetting)
		{
			CombinedConfigurationManager result = null;
			if (savedSetting.LinkID.IsEmpty &&
				(string.IsNullOrEmpty(savedSetting.CompanyCode) || savedSetting.CompanyCode.Equals(GlbCompany.CurrentCompany.GC_Code, StringComparison.OrdinalIgnoreCase)))
			{
				result = new CombinedConfigurationManager(this, savedSetting.Description);
			}
			else if (savedSetting.LinkID != companyDefaultConfigurationManager.LinkPK && !String.IsNullOrEmpty(SaveToFilterField))
			{
				LookupField lookupField = LinkedFilterFields[SaveToFilterField] as LookupField;
				if (lookupField != null)
				{
					ICodeDescription codeDescription = lookupField.GetCodeDescriptionForGUID(savedSetting.LinkID);
					if (codeDescription != null && codeDescription.PK != null)
					{
						result = Globals.IsWeb ?
							new CombinedConfigurationManager(this, savedSetting.LinkID, codeDescription.Code, codeDescription.Description, savedSetting.Description, savedSetting.CompanyCode) :
							new CombinedConfigurationManager(this, savedSetting.LinkID, codeDescription.Code, codeDescription.Description, savedSetting.Description);
					}
				}
			}
			if (result != null && (result.Company == null || result.Company.GC_IsActive))
			{
				CombinedConfigurationManagers.Add(result);
			}
			return result;
		}

		public LookupField LinkedLookupField
		{
			get { return LinkedFilterFields[SaveToFilterField] as LookupField; }
		}

		public void UpdateFromDeserialisedValue(ColumnConfigurationsManager deserialisedValue, Report report)
		{
			if (deserialisedValue != null)
			{
				deserialisedValue.reportID = reportID;
				ReportColumnSettings mergedHeadings = ColumnHeadingListSerialiser.MergeDeserialisedAndTemplateHeadings(deserialisedValue.CurrentConfiguration, DefaultTemplateConfigurationManager, report);
				savedScheduleSetting = new SavedScheduleConfigurationManager(this);
				savedScheduleSetting.AddHeadings(mergedHeadings);
				savedScheduleSetting.Load();
			}
		}

		public void SaveLastSavedSetting(Report report)
		{
			if (savedScheduleSetting != null)
			{
				savedScheduleSetting.SaveXml(report);
			}
		}

		#region IJsonSerializable Members

		string overridenHeadingsForSerialization;
		internal void OverrideHeadingsSerialization(string headingsxml)
		{
			overridenHeadingsForSerialization = headingsxml;
		}

		public object GetJsonData() =>
			new ColumnConfigurationsManagerJsonData
			{
				Headings = overridenHeadingsForSerialization ?? ColumnHeadingListSerialiser.Serialise(CurrentConfiguration)
			};

		#endregion

		#region IColumnSettingXMLVersionUpgraderV1Parameters Members

		ZGuid IColumnSettingXMLVersionUpgraderV1Parameters.ReportID
		{
			get { return this.reportID; }
		}

		#endregion
	}
}
