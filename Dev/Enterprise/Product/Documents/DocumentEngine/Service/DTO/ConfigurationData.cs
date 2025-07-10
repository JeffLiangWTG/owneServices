using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Enterprise.DocumentEngine
{
	public class ConfigurationData
	{
		public Guid ReportId { get; set; }
		public string Description { get; set; }
		public string UniqueDescription { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public ConfigurationType Type { get; set; }

		public bool CanSaveAndDelete { get; set; }
		public List<WorkSheetData> WorkSheets { get; } = new List<WorkSheetData>();
		public ReportFilterData FilterData { get; } = new ReportFilterData();
		public Guid LinkPk { get; set; }
		public string GroupBy { get; set; }
		public string SortOrder { get; set; }
		public string Orientation { get; set; }
		public string PrintLanguage { get; set; }
		public bool IsReportTitleChangeable { get; set; }
		public string BindTextInGui { get; set; }
	}

	public class SelectedValueConfigurationData
	{
		public Guid ReportId { get; set; }
		public string UniqueDescription { get; set; }
		public Guid LinkPk { get; set; }
		public List<SelectedValueWorkSheetData> WorkSheets { get; } = new List<SelectedValueWorkSheetData>();
		public ReportFilterData FilterData { get; } = new ReportFilterData();
		public string GroupBy { get; set; }
		public string SortOrder { get; set; }
		public string Orientation { get; set; }
		public string PrintLanguage { get; set; }
	}

	public enum ConfigurationType
	{
		Default, CompanyDefault, Combined
	}

	public class WorkSheetData
	{
		public string Name { get; set; }
		public string NameLocalized { get; set; }
		public string Title { get; set; }
		public List<ColumnHeadingData> ColumnHeadings { get; } = new List<ColumnHeadingData>();
	}

	public class ColumnHeadingData
	{
		public string Description { get; set; }
		public string DisplayLabel { get; set; }
		public int OriginalColumnNumber { get; set; }
		public string HeadingText { get; set; }
		public string TagName { get; set; }
		public bool Hidden { get; set; }
		public int WidthInPixels { get; set; }
		public int CurrentPosition { get; set; }
		public bool HideIfDescriptionEmpty { get; set; }
		public string BindTextInGui { get; set; }
		public string HeadingTextXMLFormat { get; set; }
		public string TagNameXMLFormat { get; set; }
		public bool ShowPerformanceWarning { get; set; }
	}

	public class SelectedValueWorkSheetData
	{
		public string Name { get; set; }
		public string Title { get; set; }
		public List<SelectedValueColumnHeadingData> ColumnHeadings { get; } = new List<SelectedValueColumnHeadingData>();
	}

	public class SelectedValueColumnHeadingData
	{
		public string DisplayLabel { get; set; }
		public string HeadingText { get; set; }
		public string TagName { get; set; }
		public bool Hidden { get; set; }
		public int WidthInPixels { get; set; }
		public int CurrentPosition { get; set; }
	}
}
