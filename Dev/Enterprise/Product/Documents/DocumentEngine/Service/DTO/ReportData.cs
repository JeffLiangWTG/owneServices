using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Enterprise.DocumentEngine
{
	public class ReportData
	{
		public Guid Id { get; set; }
		public string ReportName { get; set; }
		public GroupByData GroupBys { get; set; }
		public IReadOnlyCollection<string> SortOrderCollection { get; set; }
		public IReadOnlyCollection<string> OptionalTemplateCollection { get; set; }
		public LanguageData Language { get; } = new LanguageData();
		public OrientationData Orientation { get; } = new OrientationData();
		public ReportFilterData FilterData { get; } = new ReportFilterData();
		public LookupFilter LinkedLookupFilter { get; set; }
		public List<WorkSheetData> WorkSheets { get; } = new List<WorkSheetData>();
	}

	public class SelectedValueReportData
	{
		public Guid Id { get; set; }
		public SelectedValueGroupByData GroupBy { get; set; }
		public string SortOrder { get; set; }
		public List<string> OptionalTemplates { get; } = new List<string>();
		public string PrintLanguage { get; set; }
		public string Orientation { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public FileType FileType { get; set; }

		public ReportFilterData FilterData { get; } = new ReportFilterData();
		public List<SelectedValueWorkSheetData> WorkSheets { get; } = new List<SelectedValueWorkSheetData>();
	}

	public class OrientationData
	{
		public string Orientation { get; set; }
		public bool HiddenInGui { get; set; }
		public List<CodeDescription> OrientationList { get; } = new List<CodeDescription>();
	}

	public class LanguageData
	{
		public string PrintLanguage { get; set; }
		public bool HiddenInGui { get; set; }
		public List<CodeDescription> LanguageList { get; } = new List<CodeDescription>();
	}

	public class GroupByData
	{
		public List<string> GroupByCollection { get; } = new List<string>();
		public bool BreakPage { get; set; }
	}

	public class SelectedValueGroupByData
	{
		public string GroupBy { get; set; }
		public bool BreakPage { get; set; }
	}

	public class ReportSummaryData
	{
		public Guid Id { get; set; }
		public string ReportName { get; set; }
		public string ReportDescription { get; set; }
		public bool IsSystemDefined { get; set; }
		public bool IsClientSpecific { get; set; }
		public bool IsPublished { get; set; }
	}

	public class CodeDescription
	{
		public Guid Pk { get; set; }
		public string Code { get; set; }
		public string Description { get; set; }
	}

	public enum FileType
	{
		XLS, XLSX, PDF
	}
}


