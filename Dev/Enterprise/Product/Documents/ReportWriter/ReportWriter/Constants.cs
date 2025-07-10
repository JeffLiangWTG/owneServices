namespace Enterprise.ReportWriter
{
	class Constants : DocumentEngine.Constants
	{
		public static class ConfigArea
		{
			public const string ExpandRow = "ExpandRow";
			public const string ApplyDocumentCurrency = "ApplyDocumentCurrency";

			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
			public static class ColumnHeading
			{
				public const string SavesTo = "SavesTo=";
				public const string Hidden = "Hidden";
				public const string ShowPerformanceWarning = "ShowPerformanceWarning";
				public const string HideIfDescriptionEmpty = "HideIfDescriptionEmpty";
			}
		}

		public static class DocumeentHeader
		{
			public const string CustomisedColumn = "<CustomisedColumn(";
		}

		public static class GroupByAreaParameters
		{
			public const string GroupTitle = "GroupTitle";
			public const string KeepInSamePage = "KeepInSamePage";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static class Area
		{
			public const string StartFromSecondPage = "#StartFromSecondPage";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public new class AreaIdentifierTags : DocumentEngine.Constants.AreaIdentifierTags
		{
			public const string DocumentHeader = "#DocumentHeader";
			public const string BackPage = "#BackPage";
			public const string DocumentFooter = "#DocumentFooter";
			public const string FirstPageFooter = "#FirstPageFooter";
			public const string GroupBy = "#GroupBy:";
			public const string LastPageFooter = "#LastPageFooter";
			public const string OnlyOnePageFooter = "#OnlyOnePageFooter";
			public const string PageFooter = "#PageFooter";
			public const string PageHeader = "#PageHeader";
			public const string SectionFooter = "#SectionFooter";
			public const string SectionHeader = "#SectionHeader";
			public const string SectionPageFooter = "#SectionPageFooter";
			public const string SectionPageHeader = "#SectionPageHeader";
			public const string SectionBody = "#SectionBody:Data=";
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		public static class DefaultTemplate
		{
			public const string ReportTemplateResources = "ReportWriter.ReportTemplate.xls";
			public const string TemplateName = "Test Report.xls";
		}
	}
}
