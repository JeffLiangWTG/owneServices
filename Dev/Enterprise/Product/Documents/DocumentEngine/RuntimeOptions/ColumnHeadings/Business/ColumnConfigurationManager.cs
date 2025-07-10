using System;
using System.IO;
using System.Text;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class ColumnConfigurationManager : IComparable
	{
		public ColumnConfigurationManager(ColumnConfigurationsManager headingManager)
		{
			HeadingManager = headingManager;
		}

		public override bool Equals(object obj)
		{
			bool result = false;
			ColumnConfigurationManager manager = obj as ColumnConfigurationManager;
			if (manager != null)
			{
				result = Description.EqualsIgnoringCase(manager.Description) && LinkPK == manager.LinkPK && ReportID == manager.ReportID;
			}
			return result;
		}

		public override int GetHashCode()
		{
			return Description.GetHashCode() ^ LinkPK.GetHashCode() ^ ReportID.GetHashCode();
		}

		public bool CanSaveAndDelete
		{
			get { return CanSaveAndDeleteCore; }
		}

		protected virtual bool CanSaveAndDeleteCore
		{
			get { return true; }
		}

		public ColumnConfigurationsManager HeadingManager { get; private set; }

		public void Delete()
		{
			DeleteCore();
			HeadingManager.FireOnColumnConfigurationDeleted(this);
		}

		protected internal void Deserialise(string xml)
		{
			Deserialise(xml, null, null, null, null, null);
		}

		internal bool RefreshColumnSettingsIfLanguageChangedBack(ZString language, bool forceRefresh = false)
		{
			if (lastDeserialisedHeadings != null && (forceRefresh || lastDeserialisedHeadings.SelectedLanguage == language))
			{
				var mergedHeadings = ColumnHeadingListSerialiser.MergeDeserialisedAndTemplateHeadings(lastDeserialisedHeadings, HeadingManager.DefaultTemplateConfigurationManager, null); //TODO: Does it matter that we don't have access to Report here?
				RefreshHeadingManager(mergedHeadings);
				return true;
			}

			return false;
		}

		ReportColumnSettings lastDeserialisedHeadings;

		protected void Deserialise(string xml, CollectionOfIFilter filters, GroupByCollection groupByCollection, SortOrderCollection sortOrderCollection, Report.OrientationManagement orientationManager, DocumentPack documentPack)
		{
			lastDeserialisedHeadings = DeserialiseHeadings(xml);
			if (documentPack != null)
			{
				documentPack.Language = lastDeserialisedHeadings.SelectedLanguage;
			}

			if (filters != null)
			{
				CollectionOfIFilter deserialisedFilters = DeserialiseFilters(lastDeserialisedHeadings);

				if (deserialisedFilters != null)
				{
					UpdateFilterValues(deserialisedFilters, filters);
				}
			}

			if (groupByCollection != null)
			{
				ZString selectedGroupByName = lastDeserialisedHeadings.SelectedGroupByName;
				groupByCollection.SelectedGroupBy = String.IsNullOrEmpty(selectedGroupByName) ? groupByCollection.DefaultGroupBy : groupByCollection[selectedGroupByName];
			}

			if (sortOrderCollection != null)
			{
				ZString selectedSortOrderName = lastDeserialisedHeadings.SelectedSortOrderName;
				sortOrderCollection.SelectedOrder = String.IsNullOrEmpty(selectedSortOrderName) ? sortOrderCollection.DefaultOrder : sortOrderCollection[selectedSortOrderName];
			}

			if (orientationManager != null)
			{
				ZString deserialisedOrientation = lastDeserialisedHeadings.SelectedOrientation;
				if (!deserialisedOrientation.IsEmpty)
				{
					orientationManager.Value = deserialisedOrientation;
				}
			}

			RefreshColumnSettingsIfLanguageChangedBack(string.Empty, true);
		}

		protected string CreateXml(CollectionOfIFilter filters, string selectedGroupByName, string selectedSortOrderName, string selectedOrientation, string selectedLanguage)
		{
			string result;

			if (filters != null)
			{
				var jsonResult = JsonConverterHelper.Serialize(filters);
				HeadingManager.CurrentConfiguration.Filters = Encoding.UTF8.GetBytes(jsonResult);
			}

			HeadingManager.CurrentConfiguration.SelectedGroupByName = selectedGroupByName;
			HeadingManager.CurrentConfiguration.SelectedSortOrderName = selectedSortOrderName;
			HeadingManager.CurrentConfiguration.SelectedOrientation = selectedOrientation;
			HeadingManager.CurrentConfiguration.SelectedLanguage = selectedLanguage;

			result = ColumnHeadingListSerialiser.Serialise(HeadingManager.CurrentConfiguration);
			HeadingManager.CurrentConfiguration.Filters = null;

			return result;
		}

		public void Load()
		{
			Load(null, null, null, null, null);
		}

		public void Load(Report report)
		{
			if (report != null)
			{
				Load(report.ColumnHeadingManager.LinkedFilterFields, report.GroupByCollection, report.SortOrderCollection, report.OrientationManager, report.Parent);

				report.RefreshBindingIncludingChildren();
				if (typeof(CombinedConfigurationManager).IsAssignableFrom(GetType()) && LinkPK.IsValid)
				{
					report.ColumnHeadingManager.LinkedLookupField.Value = LinkPK.ToGuid();
				}
			}
		}

		public void Load(CollectionOfIFilter filters, GroupByCollection groupByCollection, SortOrderCollection sortOrderCollection, Report.OrientationManagement orientationManager, DocumentPack documentPack)
		{
			if (filters != null)
			{
				filters.ClearValues(HeadingManager.LinkedLookupField);
			}
			string xml = GetXml();
			if (String.IsNullOrEmpty(xml))
			{
				if (documentPack != null)
				{
					documentPack.Language = DataRegistry.Instance.EnglishSpelling;
				}
				if (filters != null && HeadingManager.DefaultFilters != null)
				{
					UpdateFilterValues(HeadingManager.DefaultFilters, filters);
				}
			}
			else
			{
				Deserialise(xml, filters, groupByCollection, sortOrderCollection, orientationManager, documentPack);
			}

			LoadCore();
		}

		public void Save()
		{
			Save(null, null, null, null, null);
		}

		public void Save(CollectionOfIFilter filters, string selectedGroupByName, string selectedSortOrderName, string selectedOrientation, string selectedLanguage)
		{
			SaveCore(filters, selectedGroupByName, selectedSortOrderName, selectedOrientation, selectedLanguage);
			HeadingManager.FireOnColumnConfigurationSaved(this);
		}

		protected void RefreshHeadingManager(ReportColumnSettings settings)
		{
			HeadingManager.RefreshCurrentConfiguration(settings, this);
		}

		protected abstract void DeleteCore();
		protected virtual void LoadCore() { }
		protected abstract void SaveCore(CollectionOfIFilter filters, string selectedGroupByName, string selectedSortOrderName, string orientation, string selectedLanguage);
		protected abstract string GetXml();
		public abstract override string ToString();

		ReportColumnSettings DeserialiseHeadings(string xml)
		{
			ColumnSettingXMLVersionUpgraderCurrentVersionParameters parameters = new ColumnSettingXMLVersionUpgraderCurrentVersionParameters(HeadingManager);
			return ColumnHeadingListSerialiser.Deserialise(xml, parameters);
		}

		public static CollectionOfIFilter DeserialiseFilters_JsonFormat(ReportColumnSettings deserialisedHeadings)
		{
			if (deserialisedHeadings.Filters != null)
			{
				using (var stream = new MemoryStream(deserialisedHeadings.Filters))
				using (var reader = new StreamReader(stream))
				{
					var json = reader.ReadToEnd();
					return JsonConverterHelper.Deserialize<CollectionOfIFilter>(json);
				}
			}
			return null;
		}

		public static CollectionOfIFilter DeserialiseFilters(ReportColumnSettings deserialisedHeadings)
		{
			return DeserialiseFilters_JsonFormat(deserialisedHeadings);
		}

		protected void UpdateFilterValues(CollectionOfIFilter source, CollectionOfIFilter target)
		{
			foreach (IFilter filter in target)
			{
				IFilter sourceFilterValue = source[((FilterField)filter).DisplayName];
				if (sourceFilterValue != null)
				{
					filter.SafeCopyValuesFrom(sourceFilterValue);
				}
			}
		}

		#region IComparable Members

		int IComparable.CompareTo(object obj)
		{
			return ToString().CompareTo(obj.ToString());
		}

		#endregion

		#region SettingKeys
		public abstract ZString Description { get; }
		public abstract ZGuid LinkPK { get; }
		public abstract ZString UniqueDescription { get; }
		public abstract ZString LinkCode { get; }
		public ZGuid ReportID
		{
			get
			{
				return HeadingManager.ReportID;
			}
		}

		public bool Scheduled
		{
			get
			{
				return HeadingManager.Scheduled;
			}
		}

		#endregion
	}
}
