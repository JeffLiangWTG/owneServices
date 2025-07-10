using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.ZArchitecture.Core;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.Areas
{
	sealed class ConfigArea : Area
	{
		public ConfigArea(int start, int end, Report report, string data)
			: base(start, end, report, data)
		{
			InitializeExposedConfigParameters();
			if (start <= end)
			{
				for (int i = start; i <= end; i++)
				{
					object cellContent = report.WorkSheetCurrentlyBeingProcessed[i, 0];
					using (report.ErrorManager.EvaluatingCell(new CellReference(report.WorkSheetCurrentlyBeingProcessed.SheetName, i, 0)))
					using (report.ErrorManager.EvaluatingOuterContent(cellContent))
					{
						ProcessConfigParameters(cellContent.ToString(), i);
					}
				}
				if (!ParentReport.ColumnHeadingsProcessedByReportAnalyser)
				{
					ProcessColumnHeadings();
				}
			}
			report.Template.ContainsCustomisedSections = ContainsCustomisedSections;
			Validate();
		}

		public ConfigArea(Report report, string data, int[] configRows)
			: base(0, 0, report, data)
		{
			InitializeExposedConfigParameters();
			if (configRows.Length > 0)
			{
				foreach (var row in configRows)
				{
					object cellContent = report.WorkSheetCurrentlyBeingProcessed[row, 0];
					using (report.ErrorManager.EvaluatingCell(new CellReference(report.WorkSheetCurrentlyBeingProcessed.SheetName, row, 0)))
					using (report.ErrorManager.EvaluatingOuterContent(cellContent))
					{
						ProcessConfigParameters(cellContent.ToString(), row);
					}
				}
				if (!ParentReport.ColumnHeadingsProcessedByReportAnalyser)
				{
					ProcessColumnHeadings();
				}
			}
			report.Template.ContainsCustomisedSections = ContainsCustomisedSections;
			Validate();
		}

		ConfigArea() { }

		void Validate()
		{
			if (ExpandRowsForAutoHeight && PageStyle != PageStyles.Continuous)
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("727991e3-582e-41a6-b533-833494cef2f1", "Expand rows can only be used with 'CONTINUOUS' Page style."),
																			new CellReference(ParentReport.WorkSheetCurrentlyBeingProcessed.SheetName, "A1"), ReportProcessingErrorSeverity.Error));
			}

			if (ParentReport.Style == Report.Styles.Document && !string.IsNullOrEmpty(ForcedLanguage) && !TranslateLegacyDocument)
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("1F479BA5-5251-4230-B232-F30EB63CB334", "{0} only works when used together with {1}.", "ForcedLanguage", "TranslateLegacyDocument"),
					new CellReference(ParentReport.WorkSheetCurrentlyBeingProcessed.SheetName, string.Empty), ReportProcessingErrorSeverity.Warning));
				ForcedLanguage = string.Empty;
			}
		}

		public override Area Clone(int position)
		{
			throw new CloneAreaException("Cannot clone a #Config Area.");
		}

		public override bool CanCloseAPage
		{
			get { return true; }
		}

		public override List<Area> Parents
		{
			get { return new List<Area>(); }
		}

		public override ValueProviderDocumenter GetDocumentation()
		{
			var summary = ResString.GetMultilingualString("975361e0-ab7a-49d5-9b58-3B898fd484e8",
				@"The first tab page on every template *MUST* start with a {0} Area. You use the {0} Area to specify parameters that affect the layout and data content of the report or document you're trying to produce.

The {0} Area contains a list of parameters with one parameter and optional value on each line in the first column of the tab page. Some parameter types also take note of values entered in following columns on the same row, but most are contained only in the first column with data separated by an '=' or a ':'.

Reports must have a '{1}' and at least one '{2}' parameter, Documents must have a '{1}' and '{3}'. All other parameters are optional. Please note that keywords are always case insensitive, and this Area is removed from the final output generated from the template.",
				"#Config", "Name", "Data", "DataContext");

			var supportedParameters1 = ResString.GetMultilingualString("1a2d2666-e8d8-46a2-b7bf-52279a44566f",
				@"Supported Parameters:

{0}={{Report Name}} - Used to specify the name of the report. E.g: {0}=My Report

{1}={{Page Style}} - Allowed Page Style options are: {2}, {3}, {4}, {5}, {6}, {7}, {8}. E.g: {9}

If Page Style is set to {8}, the following two parameters are available to set the printout page size:
{10}={{Page Height}} - Used to set the printout page height in millimeters. E.g: {10}=900
{11}={{Page Width}} - Used to set the printout page width millimeters. E.g: {11}=500

If Page Style is set to {4} and {11} is set, the parameter below is available to make the page height variable:
{12}={{AutoPageHeight}} - Usually used by roll paper printers. If it is set, the printout page height will be set to the height of its content. E.g: {12}",
				"Name", "PageStyle", "Portrait", "Landscape", "Continuous", "LetterPortrait", "LetterLandscape", "LetterPortraitMatrix", "Custom", "PageStyle=Portrait", "PageHeight", "PageWidth", "AutoPageHeight");

			var supportedParameters2 = ResString.GetMultilingualString("a4f0037a-1883-408b-bbbb-de35acb413f9",
				@"{0}=nn.nn - Version.Revision number for your template to help you keep track of modifications. Does not affect the way a document is produced at this point. E.g: {0}=1.00

{1}={{Email Subject Macro}} - The string used to generate the Email Subject when the Report or Document is output directly to email. Can also include macros. E.g: {1}=My Favorite Data as of <Now> for <JobNumber>

{2}:{{DataSourceName}}={{SQL Statement}}[{3}] - Used on Reports only to specify all data sources used. You can specify one or many data sources which are referenced by name in other Areas in the template. To have multiple data sources, include multiple parameter lines starting with '{2}:'. Can optionally include '{3}' after the select statement to prevent a where clause and order being built for the statement automatically.

If {4} is set to {5}, commented-out lines (starting with '--') are not recommended to be used here.

{6}:{{DataSourceName}}={{SQL Statement}}[{3}] - Used on Reports to specify all data sources used which all come from Odyssey or EDW（Enterprise Data Warehouse）database.

The data source is defined with {6} will draw report data from EDW database when a report is listed as a report in the ‘List of Supported Reports using EDW as a data source’ system registry. Otherwise it will draw data from Odyssey database.

You can specify one or many data sources which are referenced by name in other Areas in the template.

To have multiple data sources, include multiple parameter lines starting with '{6}:'.

Can optionally include '{3}' after the select statement to prevent a where clause and order being built for the statement automatically.

If {4} is set to {5}, commented-out lines (starting with '--') are not recommended to be used here.

{7}:{{DataSourceName}}={{SQL Statement}}[{3}] - Used on Reports only to specify all data sources used which all come from EDW database. You can specify one or many data sources which are referenced by name in other Areas in the template.

To have multiple data sources, include multiple parameter lines starting with '{7}'.

Can optionally include '{3}' after the select statement to prevent a where clause and order being built for the statement automatically.

If {4} is set to {5}, commented-out lines (starting with '--') are not recommended to be used here.",
				"Version", "EmailSubject", "Data", ":NoWhereClause", "DataSourceName", "ReportData", "EDWData", "EDWOnlyData");

			var supportedParameters3 = ResString.GetMultilingualString("66f7d40b-043e-4951-8116-ecf7fe9b4875",
				@"{0}={{Data Context Name}} - Used on Documents only to specify the desired type for the top level data source. When a document is produced, the Document Engine passes the data context specified into the Document Supporter for the current top level business object which returns either one or many data sources. The type of data source you get back is usually based on the Data Source alone, but may take into account other factors as well. The range of data contexts supported varies between the Document Supporters for each type of top level business object.

{1} - Switches Auto Height row expansion to expand existing rows rather than inserting additional rows to handle overflowing data in fields. Only has an effect if you specify the '{2}' Page Style on a Report.

{3}={{Form Length In 360ths Of An Inch}} - Allows you to specify the trailing form feed size as a whole number measured in 360ths of an inch.

{4}={{Evaluable Expression}} - If the expression specified evaluates to 'true', will remove the worksheet from the final output. E.g: {4}=""Black""==""White"" would not hide, {4}=""White""==""White"" would hide.

{5} - Any boolean expressions entered on the same row will cause the column the expression is entered in to be hidden if the expression evaluates to 'true'. The Document Engine uses the JavaScript engine to evaluate these expressions, so most types of logical comparisons will work. E.g: 1==1, 2!=1, {6}

{7}= these are query hints that override the default behavior of the SQL Query optimizer. Options include recompile, fast etc. The only the options need to be specified. '{8}' will turn into '{9}'",
				"DataContext", "AutoHeightMode=ExpandRow", "Continuous", "TrailingFormFeed", "HideSheetIf", "HideColumnIf",
				"\"<Contact.Name>\"==\"Fred\"", "SQLQUERYHINTS", "SQLQUERYHINTS=recompile, fast 10", "option (recompile, fast 10)");

			var supportedParameters4 = ResString.GetMultilingualString("0e99937e-f6e6-4d4d-a217-9b9acddd1804",
				@"{0} - Will ignore '{1}' query hint (which might be set as default in registry).

{2} - Will disable the option to export report as CSV format. Use this option when there are multiple levels of data in the report, which is not supported by CSV.

{3}={{Evaluable Expression}} - If the expression specified evaluates to 'true', this report will always evaluate the value of UDF (User Defined Field) from its default value specified in template, it will not use the overridden value Doc Data tab page on the job. E.g: {4}

{5} - Used on Reports Only, use this option when the report doesn't need to be translated

{6} - Used on Reports Only, specifies the row Option Column Headings are entered on. Any Column Heading parameter entered on the same row will cause the column the expression is entered in to be seen by the Document Engine as an Optional Column.

{7} - Use this option to remove the first page if there's no data in it when using {8} Area with {9} parameter

{10} - Used on legacy documents only, use this option when the legacy document needs to be translated

{11} - Used on legacy documents only, use this option when the legacy document needs to be translated by a specific language, it only works when used together with {10}, please note, both {10} and {11} only work with system defined templates

{12} - Use this option to hide the #Config rows in the template when the document or report is generated in Excel format. The #Config rows are removed by default, and if there are a lot of rows to remove, this can lead to longer generation times. Please note that when using this option that there can be a dramatic increase in the total number of rows in the Excel file. So if your report is nearing the limits of the maximum allowed rows in an Excel file, then do not use this option.",
				"SQLQUERYHINTSIGNORERECOMPILE", "recompile", "DisableCSVExport", "DisableFixedValueCacheExpression", "DisableFixedValueCacheExpression=\"<IsManufacturerBillOfLading>\"==\"Y\"",
				"DisableTranslate", "ColumnHeadings", "RemoveFirstPageIfNoData", "#SectionHeader", "PageBreak", "TranslateLegacyDocument", "ForcedLanguage", "HideRowsInsteadOfRemove");

			var columnHeadingExplanation = ResString.GetMultilingualString("5902b23b-fa45-4e0d-9471-83b7dc78e4ac",
				@"A single Column Heading parameter follows the format '{0}' where:
	 - {1} is a column key for storing settings etc
	 - {2} is a meaningful description of the column that the users use in the report configuration form to select which columns to show and what order they appear.
	 - {3} specifies the default heading for the column that prints on the report. The user can optionally override this in the Report Configuration. If no {3} is defined, {1} is used instead.
	 - {4} is used to indicate that the column is hidden unless the user decides to include it. 
	 - {5} is used to indicate that the column has potential performance problems and the user is advised not to include it. 
	 - {6} is used to indicate that the column should be hidden from the report if the Description of that column is blank. E.g: {7}",
				"DisplayLabel=\"{DisplayLabel}\"[,Description=\"{Description}\"][,HeadingText=\"{HeadingText}\"][,Hidden][,ShowPerformanceWarning][,HideIfDescriptionEmpty]",
				"DisplayLabel", "Description", "HeadingText", "Hidden", "ShowPerformanceWarning", "HideIfDescriptionEmpty", "DisplayLabel=\"Total Earnings\",Hidden");

			var completeExample = ResString.GetMultilingualString("abdee3fa-0ce5-4bdb-b4a8-463540aaf432",
				@"{0}={{Currency Code}} - Used to apply special currency format on selected columns. '{1}' entered on the same row, will cause the the column where expression is entered in to have its decimal value in the selected currency format. 
'Currency Code' may be a 3-letter code of currency or any macros that will return currency code (e.g. <{2}> etc.). Formatting will be applied only on body {3} and {4} Areas of a document.

A completed Template will have a {5} Area looking something like this:

{6}",
				"DocumentCurrency", "ApplyDocumentCurrency", "CompanyCurrencyCode", "SectionBody", "GroupBy", "#Config",
				@"#Config  
Name=Test Template  
PageStyle=Portrait  
Data:Notes=select * from NotesTable
#DocumentHeader
...
#EndOfReport");

			return new ValueProviderDocumenter("#Config",
				MultilingualString.Join(System.Environment.NewLine + System.Environment.NewLine, new[] { summary, supportedParameters1, supportedParameters2, supportedParameters3, supportedParameters4, columnHeadingExplanation, completeExample }));
		}

		#region Exposed Config Parameters
		public ZString GetRawConfigAreaParameters()
		{
			var result = new ZStringBuilder();
			for (int i = fStart; i <= fEnd; i++)
			{
				result.AppendIfNotEmpty(ParentReport.XlInterface.WorkSheets[0][i, 0].ToString());
			}
			return result.ToStringWithNewLineBetweenAppends();
		}

		void InitializeExposedConfigParameters()
		{
			ReportName = "";
			EmailSubject = "";
			DataSourceStrings = new List<string>();
			DataContextValue = DataContextValue.None;
			HideColumnExpressions = new Dictionary<int, string>();
			PageStyle = PageStyles.Portrait;
			SlowServers = Array.Empty<string>();
			DocumentCurrencyPattern = "";
			ColumnsWithCurrencySetup = new List<int>();
			SuppressDraftWatermark = false;
			ForceWebPublish = false;
			SqlTimeout = -1;
			SqlQueryHints = "";
			SqlQueryHintsIgnoreRecompile = false;
			DisableXLSXExport = false;
			DisableCSVExport = false;
			DisableFixedValueCacheExpression = "";
			DisableTranslate = false;
			RemoveFirstPageIfNoData = false;
			TranslateLegacyDocument = false;
			HideRowsInsteadOfRemove = false;
			ForcedLanguage = "";
		}

		internal CellReference SavesToFieldCellReference { get; private set; }

		public string ReportName { get; private set; }

		public bool ForceWebPublish { get; private set; }

		internal int ReportVersion { get; private set; }

		public bool ExpandRowsForAutoHeight { get; private set; }

		public string EmailSubject { get; private set; }

		public string ForcedLanguage { get; private set; }

		public List<string> DataSourceStrings { get; private set; }

		public DataContextValue DataContextValue { get; private set; }

		public Dictionary<int, string> HideColumnExpressions { get; private set; }

		public PageStyles PageStyle { get; private set; }

		public bool HasPageStyleSignature { get; private set; }

		public int CustomPageWidthInMillimeters { get; private set; }

		public int CustomPageHeightInMillimeters { get; private set; }

		public bool AutoPageHeight { get; private set; }

		public bool ContainsCustomisedSections { get; private set; }

		public uint TrailingFormFeedLengthIn360thsOfAnInch { get; private set; }

		public string[] SlowServers { get; private set; }

		public string DocumentCurrencyPattern { get; private set; }

		public List<int> ColumnsWithCurrencySetup { get; private set; }

		public string SheetNameOverride { get; private set; }

		public string HideSheetIfExpression { get; private set; }

		public bool SuppressDraftWatermark { get; private set; }

		public int SqlTimeout { get; private set; }

		public string SqlQueryHints { get; private set; }

		public bool SqlQueryHintsIgnoreRecompile { get; private set; }

		public bool DisableXLSXExport { get; private set; }

		public bool DisableCSVExport { get; private set; }

		public string DisableFixedValueCacheExpression { get; private set; }

		public bool DisableTranslate { get; private set; }

		public bool RemoveFirstPageIfNoData { get; private set; }

		public bool TranslateLegacyDocument { get; private set; }

		public bool HideRowsInsteadOfRemove { get; private set; }

		#endregion

		#region ProcessConfigParameters

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void ProcessConfigParameters(string lineText, int rowNumber)
		{
			if (lineText.StartsWith(Constants.ConfigAreaParameters.TemplateNameSignature, StringComparison.OrdinalIgnoreCase))
			{
				ReportName = lineText.Substring(Constants.ConfigAreaParameters.TemplateNameSignature.Length);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.VersionSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessVersion(lineText, rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.AutoHeightModeSignature, StringComparison.OrdinalIgnoreCase))
			{
				ExpandRowsForAutoHeight = (lineText.Substring(Constants.ConfigAreaParameters.AutoHeightModeSignature.Length).ToLower().Trim() == "expandrow");
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.ContainsCustomisedSections, StringComparison.OrdinalIgnoreCase))
			{
				ContainsCustomisedSections = (lineText.Substring(Constants.ConfigAreaParameters.ContainsCustomisedSections.Length).ToLower().Trim() == "true");
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.EmailSubjectSignature, StringComparison.OrdinalIgnoreCase))
			{
				EmailSubject = lineText.Substring(Constants.ConfigAreaParameters.EmailSubjectSignature.Length);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.DataSourceSignature, StringComparison.OrdinalIgnoreCase) ||
					lineText.StartsWith(Constants.ConfigAreaParameters.EDWDataSourceSignature, StringComparison.OrdinalIgnoreCase) ||
					lineText.StartsWith(Constants.ConfigAreaParameters.EDWOnlyDataSourceSignature, StringComparison.OrdinalIgnoreCase))
			{
				DataSourceStrings.Add(lineText);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.DataContextSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessDataContext(lineText, rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.HideColumnIfSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessHideColumns(lineText, rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.PageStyleSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessPageStyle(lineText, rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.ForcedLanguageSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessForcedLanguageSignature(lineText, rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.PageWidthSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessPageWidth(lineText, rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.PageHeightSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessPageHeight(lineText, rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.TrailingFormFeedSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessTrailingFormFeed(lineText, rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.SlowServers, StringComparison.OrdinalIgnoreCase))
			{
				ValidateSlowServers(lineText.Substring(Constants.ConfigAreaParameters.SlowServers.Length).ToLower().Trim().Split(','), rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.DocumentCurrency, StringComparison.OrdinalIgnoreCase))
			{
				ProcessDocumentCurrency(lineText, rowNumber);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.SheetNameOverride, StringComparison.OrdinalIgnoreCase))
			{
				SheetNameOverride = lineText.Substring(Constants.ConfigAreaParameters.SheetNameOverride.Length);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.HideSheetIfSignature, StringComparison.OrdinalIgnoreCase))
			{
				HideSheetIfExpression = lineText.Substring(Constants.ConfigAreaParameters.HideSheetIfSignature.Length);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.DisableFixedValueCacheSignature, StringComparison.OrdinalIgnoreCase))
			{
				DisableFixedValueCacheExpression = lineText.Substring(Constants.ConfigAreaParameters.DisableFixedValueCacheSignature.Length);
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.SqlTimeoutSignature, StringComparison.OrdinalIgnoreCase))
			{
				var items = lineText.Split('=');
				if (items.Length == 2)
				{
					int sqlTimeout;
					if (int.TryParse(items[1], out sqlTimeout))
					{
						if (sqlTimeout >= MinSqlTimeout && sqlTimeout <= MaxSqlTimeout)
						{
							SqlTimeout = sqlTimeout;
						}
						else
						{
							ParentReport.ErrorManager.Add(new ReportProcessingError(
								Res.GetString("180f9a8c-8949-4cf4-8431-f5cb54d180d9", "SQL Timeout [{0}] must be set between [{1}] and [{2}].",
								sqlTimeout, MinSqlTimeout, MaxSqlTimeout), ReportProcessingErrorSeverity.Warning));
						}
					}
				}
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.SqlQueryHints, StringComparison.OrdinalIgnoreCase))
			{
				SqlQueryHints = lineText.Substring(Constants.ConfigAreaParameters.SqlQueryHints.Length);
			}
			else
			{
				ProcessConfigFlags(lineText);
			}
		}

		AvailableDocBuilderLanguageList StandardLanguages => standardLanguages ?? (standardLanguages = new AvailableDocBuilderLanguageList(null));

		AvailableDocBuilderLanguageList standardLanguages;

		void ProcessConfigFlags(string lineText)
		{
			if (lineText.StartsWith(Constants.ConfigAreaParameters.ForceWebPublishSignature, StringComparison.OrdinalIgnoreCase))
			{
				ForceWebPublish = true;
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.SuppressDraftWatermark, StringComparison.OrdinalIgnoreCase))
			{
				SuppressDraftWatermark = true;
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.DisableXLSXExport, StringComparison.OrdinalIgnoreCase))
			{
				DisableXLSXExport = true;
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.DisableCSVExport, StringComparison.OrdinalIgnoreCase))
			{
				DisableCSVExport = true;
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.SqlQueryHintsIgnoreRecompile, StringComparison.OrdinalIgnoreCase))
			{
				SqlQueryHintsIgnoreRecompile = true;
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.DisableTranslate, StringComparison.OrdinalIgnoreCase))
			{
				DisableTranslate = true;
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.RemoveFirstPageIfNoData, StringComparison.OrdinalIgnoreCase))
			{
				RemoveFirstPageIfNoData = true;
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.TranslateLegacyDocument, StringComparison.OrdinalIgnoreCase))
			{
				TranslateLegacyDocument = true;
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.AutoPageHeightSignature, StringComparison.OrdinalIgnoreCase))
			{
				AutoPageHeight = true;
			}
			else if (lineText.StartsWith(Constants.ConfigAreaParameters.HideRowsInsteadOfRemove, StringComparison.OrdinalIgnoreCase))
			{
				HideRowsInsteadOfRemove = true;
			}
		}

		const int MinSqlTimeout = 30;
		const int MaxSqlTimeout = 21600;

		CellReference GetColumnABasedCellReference(int rowNumber) => new CellReference(ParentReport.WorkSheetCurrentlyBeingProcessed.SheetName, "A" + (rowNumber + 1).ToString(CultureInfo.InvariantCulture));

		void ValidateSlowServers(string[] slowServers, int rowNumber)
		{
			for (int count = 0; count < slowServers.Length; count++)
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(slowServers[count] + " " + Res.GetString("27104c63-3529-42e1-ad3d-90457ab47fa7", "is not a recognized slow server"),
					GetColumnABasedCellReference(rowNumber),
					ReportProcessingErrorSeverity.Warning));
			}
		}

		void ProcessVersion(string line, int rowNumber)
		{
			var versionAsText = line.Substring(Constants.ConfigAreaParameters.VersionSignature.Length);
			double reportVersion;

			using (Culture.SetTemporarily(Culture.Default))
			{
				if (!double.TryParse(versionAsText, out reportVersion) || reportVersion < 0 || reportVersion > int.MaxValue / 100.00)
				{
					reportVersion = 1.00;
					ParentReport.ErrorManager.Add(new ReportProcessingError(
						Res.GetString("77623b2f-861b-44a9-a81d-d935059d86bc", "Parameter '{0}' must be a positive decimal (between 0.00 and 21474836.47) with format as {1} but its value is '{2}'", "Version", "nn.nn", versionAsText),
						GetColumnABasedCellReference(rowNumber),
						ReportProcessingErrorSeverity.Warning));
				}
			}

			ReportVersion = (int)(reportVersion * 100);
		}

		void ProcessDataContext(string line, int rowNumber)
		{
			string dataContextStr = line.Substring(Constants.ConfigAreaParameters.DataContextSignature.Length);
			try
			{
				DataContextValue = new DataContextValue(dataContextStr);
			}
			catch (ArgumentException ex)
			{
				var message = Res.GetString("57a798d5-57fd-4444-8352-ac5c33f0e852", "Undefined DataContext {0}", dataContextStr);
				ParentReport.ErrorManager.Add(new ReportProcessingError(message, GetColumnABasedCellReference(rowNumber), ReportProcessingErrorSeverity.Error, ex));
			}
		}

		#region ProcessColumnHeadings

		class Reference
		{
			public Reference(string sheetName, ColumnHeading columnHeading, int rowNumber, int colNumber)
			{
				this.SheetName = sheetName;
				this.ColumnHeading = columnHeading;
				this.RowNumber = rowNumber;
				this.ColumnNumber = colNumber;
			}

			public ColumnHeading ColumnHeading;
			public List<int> References = new List<int>();
			public string SheetName;
			public int RowNumber;
			public int ColumnNumber;
		}

		int GetEndOfConfigArea(ExcelWorkSheet worksheet)
		{
			int startOfFirstAreaAfterConfig;
			for (startOfFirstAreaAfterConfig = 1; startOfFirstAreaAfterConfig < ParentReport.XlInterface.MaxRowCountSupportedByCurrentExcelFile; startOfFirstAreaAfterConfig++)
			{
				if (worksheet[startOfFirstAreaAfterConfig, 0].ToString().StartsWith("#"))
				{
					break;
				}
			}
			return startOfFirstAreaAfterConfig - 1;
		}

#if DEBUG
		public int GetEndOfConfigArea_ExposedForTest(ExcelWorkSheet worksheet)
		{
			return GetEndOfConfigArea(worksheet);
		}
#endif

		void ProcessColumnHeadings()
		{
			if (!ParentReport.ColumnHeadingsProcessedByReportAnalyser)
			{
				ParentReport.ColumnHeadingMacros = new List<string>();

				const string savesToKeyword = "savesto=";
				float widthMultiplier = (float)ExcelMetrics.ColMult(ParentReport.XlInterface.Xls);
				bool reportDefinesOptionalColumns = false;

				for (int x = 0; x < ParentReport.XlInterface.WorkSheets.Count; x++)
				{
					int row = 0;
					ExcelWorkSheet workSheet = ParentReport.XlInterface.WorkSheets[x];

					int endOfConfigArea = GetEndOfConfigArea(workSheet);

					bool sheetDefinesOptionalColumns = false;
					for (int i = 1; i <= endOfConfigArea; i++)
					{
						string configCellValue = workSheet[i, 0].ToString();
						using (ParentReport.ErrorManager.EvaluatingCell(new CellReference(workSheet.SheetName, i, 0)))
						{
							if (configCellValue.StartsWith(Constants.ConfigAreaParameters.Title, StringComparison.OrdinalIgnoreCase))
							{
								if (!ParentReport.ColumnHeadingManager.IsReportTitleChangeable)
								{
									ParentReport.ColumnHeadingManager.IsReportTitleChangeable = true;
								}

								string title = configCellValue.Substring(Constants.ConfigAreaParameters.Title.Length);
								if (!string.IsNullOrEmpty(title))
								{
									var localizedTitle = DocBuilderResourceStrings.GetLocalizedReportTitle(title);
									ParentReport.ColumnHeadingManager.DefaultTemplateConfigurationManager.SetTitle(workSheet.SheetName, localizedTitle, SheetNameOverride);
								}
							}
							else if (configCellValue.StartsWith(Constants.ConfigAreaParameters.ColumnHeadingsSignature, StringComparison.OrdinalIgnoreCase))
							{
								sheetDefinesOptionalColumns = true;
								reportDefinesOptionalColumns = true;
								row = i;
								if (SavesToFieldCellReference.IsEmpty)
								{
									string columnHeadingCellContent = configCellValue.Trim();
									int savesToKeywordPosition = columnHeadingCellContent.IndexOf(savesToKeyword, StringComparison.OrdinalIgnoreCase);

									if (savesToKeywordPosition > -1)
									{
										ParentReport.ColumnHeadingManager.ColumnHeadingCellContent = columnHeadingCellContent;
										ParentReport.ColumnHeadingManager.SaveToFilterField = columnHeadingCellContent.Substring(savesToKeywordPosition + savesToKeyword.Length);
										SavesToFieldCellReference = new CellReference(ParentReport.WorkSheetCurrentlyBeingProcessed.SheetName, row, 0);
									}
								}
							}
						}
					}

					if (Report.IsTemplateSheet(workSheet.SheetName) && sheetDefinesOptionalColumns)
					{
						int displayOrder = 0;
						bool hasHitColumnHeading = false;
						bool hasHitBlankAfterColumnHeading = false;
						var unresolvedReferences = new List<Reference>();
						var columnHeadings = new ColumnHeadingCollection();

						for (int col = 1; col < workSheet.ColumnCount; col++)
						{
							string cellValue = workSheet[row, col].ToString().Trim();
							using (ParentReport.ErrorManager.EvaluatingCell(new CellReference(workSheet.SheetName, row, col)))
							{
								bool doesntHaveColumnHeading = string.IsNullOrEmpty(cellValue);
								if (doesntHaveColumnHeading && hasHitColumnHeading)
								{
									hasHitBlankAfterColumnHeading = true;
								}
								else if (!doesntHaveColumnHeading)
								{
									hasHitColumnHeading = true;
								}

								if (!doesntHaveColumnHeading && hasHitBlankAfterColumnHeading)
								{
									ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("60a2eb38-58f1-4e33-b31c-0de0171558df", "All column headings should group together, no empty column headings allowed in between."), new CellReference(workSheet.SheetName, row, col), ReportProcessingErrorSeverity.Error));
									break;
								}

								if (RegexProvider.ColumnHeadingDisplayLabelRegex.IsMatch(cellValue))
								{
									var displayLabelMatch = RegexProvider.ColumnHeadingDisplayLabelRegex.Match(cellValue);
									var displayLabel = ParentReport.GetTranslatedColumnHeading(displayLabelMatch.Groups[1].Value);
									if (!string.IsNullOrEmpty(displayLabel) && columnHeadings.Contains(displayLabel))
									{
										ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("D169680A-7DD8-4533-9CD8-9728DED31E87", "Duplicated column heading display label: {0}", displayLabel), new CellReference(workSheet.SheetName, row, col), ReportProcessingErrorSeverity.Error));
									}
									else
									{
										var columnHeading = GetColumnHeading(workSheet, row, col, displayOrder, displayLabel, cellValue, widthMultiplier, unresolvedReferences);
										columnHeadings.Add(columnHeading);
										ParentReport.ColumnHeadingManager.DefaultTemplateConfigurationManager.AddHeading(workSheet.SheetName, columnHeading, SheetNameOverride);
										if (!columnHeading.Hidden)
										{
											displayOrder++;
										}
									}
								}
								else if (doesntHaveColumnHeading)
								{
									foreach (int i in GetReferencedColumns(workSheet, row + 1, col))
									{
										string columnName = GetDisplayLabelFromOriginalColumnNumber(columnHeadings, i);
										if (columnHeadings.Contains(columnName))
										{
											columnHeadings[columnName].ReferencedBy.Add(DummyColumnHeading);
										}
									}
								}
								else
								{
									ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("1e7c2672-cf1b-42ba-8320-8e94263aebaa", "Column heading must have a display label keyword."), new CellReference(workSheet.SheetName, row, col), ReportProcessingErrorSeverity.Error));
								}
							}
						}
						ResolveReferences(columnHeadings, unresolvedReferences);
						DetectCirularReferences(columnHeadings, workSheet.SheetName);
					}
				}
				if (reportDefinesOptionalColumns)
				{
					ParentReport.ColumnHeadingManager.DefaultTemplateConfigurationManager.Load();
				}
				ParentReport.ColumnHeadingsProcessedByReportAnalyser = true;
			}
		}

		ColumnHeading DummyColumnHeading
		{
			get { return dummyColumnHeading ?? (dummyColumnHeading = new ColumnHeading()); }
		}
		ColumnHeading dummyColumnHeading;

		void ResolveReferences(ColumnHeadingCollection columnHeadings, List<Reference> unresolvedReferences)
		{
			foreach (Reference reference in unresolvedReferences)
			{
				foreach (int columnOriginalNumber in reference.References)
				{
					string columnName = GetDisplayLabelFromOriginalColumnNumber(columnHeadings, columnOriginalNumber);
					if (columnHeadings.Contains(columnName))
					{
						columnHeadings[columnName].ReferencedBy.Add(reference.ColumnHeading);
					}
				}
			}
		}

		void DetectCirularReferences(ColumnHeadingCollection columnHeadings, string sheetName)
		{
			foreach (ColumnHeading columnHeading in columnHeadings)
			{
				if (HasCirularReferences(columnHeading.ReferencedBy, columnHeading, new List<ColumnHeading>()))
				{
					ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("8f6f2bac-2aed-4dde-bc7f-51ba0cacff32", "Column [{0}] is a part of circular reference.", columnHeading.DisplayLabel), new CellReference(sheetName, ""), ReportProcessingErrorSeverity.Error));
				}
			}
		}

		bool HasCirularReferences(ColumnHeadingCollection columnHeadings, ColumnHeading startColumnHeading, List<ColumnHeading> visited)
		{
			foreach (ColumnHeading columnHeading in columnHeadings)
			{
				if (columnHeading == startColumnHeading)
				{
					return true;
				}
				else
				{
					if (!visited.Contains(columnHeading))
					{
						visited.Add(columnHeading);
						return HasCirularReferences(columnHeading.ReferencedBy, startColumnHeading, visited);
					}
				}
			}
			return false;
		}

		ColumnHeading GetColumnHeading(ExcelWorkSheet worksheet, int row, int column, int displayOrder, string displayLabel, string cellValue, float widthMultiplier, List<Reference> unresolvedReferences)
		{
			var displayLabelMatch = RegexProvider.ColumnHeadingDisplayLabelRegex.Match(cellValue);
			cellValue = cellValue.Replace(displayLabelMatch.Groups[0].Value, "");
			var result = new ColumnHeading(displayLabel);
			var templateFileName = string.Empty;

			if (ParentReport.StTemplate != null && ParentReport.StTemplate.SO_IsSystemDefined)
			{
				templateFileName = Path.GetFileNameWithoutExtension(ParentReport.StTemplate.SO_ExcelTemplatePath);
			}

			result.HideIfDescriptionEmpty = cellValue.ToUpperInvariant().Contains("HIDEIFDESCRIPTIONEMPTY");

			if (RegexProvider.ColumnHeadingDescriptionRegex.IsMatch(cellValue))
			{
				var descriptionMatch = RegexProvider.ColumnHeadingDescriptionRegex.Match(cellValue);
				var translatedDescriptionText = DocBuilderResourceStrings.GetReportString(templateFileName, descriptionMatch.Groups[1].Value.Trim());
				result.Description = ParentReport.GetTranslatedColumnHeading(translatedDescriptionText);
				cellValue = cellValue.Replace(descriptionMatch.Groups[0].Value, "");
			}
			else if (!result.HideIfDescriptionEmpty)
			{
				result.Description = DocBuilderResourceStrings.GetReportString(templateFileName, displayLabel);
			}

			if (RegexProvider.ColumnHeadingHeadingTextRegex.IsMatch(cellValue))
			{
				var headingTextMatch = RegexProvider.ColumnHeadingHeadingTextRegex.Match(cellValue);
				var englishHeadingText = headingTextMatch.Groups[1].Value.Trim();
				var translatedHeadingText = DocBuilderResourceStrings.GetReportString(templateFileName, englishHeadingText);
				result.HeadingText = ParentReport.GetTranslatedColumnHeading(translatedHeadingText);
				result.EnglishHeadingText = englishHeadingText;
				cellValue = cellValue.Replace(headingTextMatch.Groups[0].Value, "");
			}
			else
			{
				result.HeadingText = displayLabel;
				result.EnglishHeadingText = displayLabel;
			}

			if (RegexProvider.ColumnHeadingTagNameTextRegex.IsMatch(cellValue))
			{
				var tagNameTextMatch = RegexProvider.ColumnHeadingTagNameTextRegex.Match(cellValue);
				result.TagName = ParentReport.GetTranslatedColumnHeading(tagNameTextMatch.Groups[1].Value);
				cellValue = cellValue.Replace(tagNameTextMatch.Groups[0].Value, "");
			}

			var reference = new Reference(worksheet.SheetName, result, row, column);

			AddCellReferencesToList(reference, GetReferencedColumns(worksheet, row + 1, column));

			if (reference.References.Count > 0)
			{
				unresolvedReferences.Add(reference);
			}

			result.OriginalColumnNumber = column;
			result.CurrentPosition = displayOrder;
			result.Hidden = cellValue.ToUpperInvariant().Contains("HIDDEN");
			int columnWidth = worksheet.GetColWidth(column);
			result.WidthInPixels = (int)(columnWidth / widthMultiplier);
			result.ShowPerformanceWarning = cellValue.ToUpperInvariant().Contains("SHOWPERFORMANCEWARNING");

			return result;
		}

		int[] GetReferencedColumns(ExcelWorkSheet workSheet, int startRow, int inputColumnIndex)
		{
			var result = new List<int>();

			for (int i = startRow; i < workSheet.RowCount; i++)
			{
				if (workSheet[i, inputColumnIndex] is TFormula)
				{
					MatchCollection referencesMatch = columnReferenceRegex.Matches(((TFormula)workSheet[i, inputColumnIndex]).Text);
					foreach (Match refMatch in referencesMatch)
					{
						if (!string.IsNullOrEmpty(refMatch.Groups["SingleReference"].Value))
						{
							int columnIndex = GetColumnIndexFromExcelName(refMatch.Groups["SingleReference"].Value);
							if (columnIndex != inputColumnIndex && !result.Contains(columnIndex))
							{
								result.Add(columnIndex);
							}
						}
						if (!string.IsNullOrEmpty(refMatch.Groups["RangeReference"].Value))
						{
							int startRangeColumnIndex = GetColumnIndexFromExcelName(refMatch.Groups["StartCell"].Value);
							int endRangeColumnIndex = GetColumnIndexFromExcelName(refMatch.Groups["EndCell"].Value);
							for (int j = startRangeColumnIndex; j <= endRangeColumnIndex; j++)
							{
								if (j != inputColumnIndex && !result.Contains(j))
								{
									result.Add(j);
								}
							}
						}
					}
				}
			}
			return result.ToArray();
		}

		void AddCellReferencesToList(Reference cellReferences, int[] references)
		{
			foreach (int reference in references)
			{
				AddCellReferenceToList(cellReferences, reference);
			}
		}

		void AddCellReferenceToList(Reference cellReferences, int reference)
		{
			if (cellReferences.ColumnNumber != reference && !cellReferences.References.Contains(reference))
			{
				cellReferences.References.Add(reference);
			}
		}

		int GetColumnIndexFromExcelName(string columnName)
		{
			int a = Convert.ToInt32('A');
			int columnIndex = 0;
			for (int i = 0; i < columnName.Length; i++)
			{
				columnIndex = columnIndex * 26 + (Convert.ToInt32(columnName[i]) - a + 1);
			}
			return columnIndex - 1;
		}

		string GetDisplayLabelFromOriginalColumnNumber(ColumnHeadingCollection columnsHeadings, int columnIndex)
		{
			string result = null;
			foreach (ColumnHeading heading in columnsHeadings)
			{
				if (heading.OriginalColumnNumber == columnIndex)
				{
					result = heading.DisplayLabel;
					break;
				}
			}
			return result;
		}

		static readonly Regex columnReferenceRegex = new Regex(@"(?<SingleReference>(?<=[^:]?)\b\$?[A-Z]{1,2})(?:\$?[0-9]{1,3}\b)(?![\:])|(?<RangeReference>(?<StartCell>\b\$?[A-Z]{1,2})(?:\$?[0-9]{1,3}\b\:)(?<EndCell>\b\$?[A-Z]{1,2})(?:\$?[0-9]{1,3}\b))", RegexOptions.Compiled);

		#endregion

		void ProcessHideColumns(string line, int rowNumber)
		{
			for (int col = 1; col < ParentReport.WorkSheetCurrentlyBeingProcessed.ColumnCount; col++)
			{
				if (!string.IsNullOrWhiteSpace(ParentReport.WorkSheetCurrentlyBeingProcessed[rowNumber, col].ToString()) && !HideColumnExpressions.ContainsKey(col))
				{
					HideColumnExpressions.Add(col, ParentReport.WorkSheetCurrentlyBeingProcessed[rowNumber, col].ToString().Trim());
				}
			}
		}

		void ProcessPageStyle(string line, int rowNumber)
		{
			try
			{
				PageStyle = (PageStyles)Enum.Parse(typeof(PageStyles), line.Substring(Constants.ConfigAreaParameters.PageStyleSignature.Length), true);
				HasPageStyleSignature = true;
			}
			catch (ArgumentException ex)
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("c22b2333-b919-4fcf-a9ca-86f712f709ee", "Page style can only be one of: {0}.", GetAvailablePageStyles()),
					GetColumnABasedCellReference(rowNumber),
					ReportProcessingErrorSeverity.Error, ex));
			}
		}

		void ProcessForcedLanguageSignature(string lineText, int rowNumber)
		{
			var languageCode = lineText.Substring(Constants.ConfigAreaParameters.ForcedLanguageSignature.Length);
			if (!languageCode.IsNullOrEmpty())
			{
				if (RegexProvider.IsSingleMacro(languageCode))
				{
					languageCode = ParentReport.TranslateMacros(lineText.Substring(Constants.ConfigAreaParameters.ForcedLanguageSignature.Length));
				}

				if (!StandardLanguages.ContainsCode(languageCode))
				{
					ParentReport.ErrorManager.Add(
						new ReportProcessingError(
							Res.GetString("5199CBBB-E973-42B0-949F-F50A8D9ED85F", "Forced language '{0}' is not valid, it has been changed to default empty.", languageCode),
							GetColumnABasedCellReference(rowNumber),
							ReportProcessingErrorSeverity.Warning));
				}
				else
				{
					ForcedLanguage = languageCode.ToUpperInvariant();
				}
			}
		}

		void ProcessPageWidth(string line, int rowNumber)
		{
			if (!int.TryParse(line.Substring(Constants.ConfigAreaParameters.PageWidthSignature.Length), out var pageWidth))
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("c8eb8fa2-0e39-4e76-b291-19220b351286", "Page width should be a valid number."),
					GetColumnABasedCellReference(rowNumber),
					ReportProcessingErrorSeverity.Error));
			}
			else if (pageWidth <= 0)
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("00c591e9-ca46-419c-a3a9-9337cf2d37e4", "Page width should be larger than zero."),
					GetColumnABasedCellReference(rowNumber),
					ReportProcessingErrorSeverity.Error));
			}
			else
			{
				CustomPageWidthInMillimeters = pageWidth;
			}
		}

		void ProcessPageHeight(string line, int rowNumber)
		{
			if (!int.TryParse(line.Substring(Constants.ConfigAreaParameters.PageHeightSignature.Length), out int pageHeight))
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("2841d5d7-22fb-4e87-9a00-8fce40ab56ca", "Page height should be a valid number."),
					GetColumnABasedCellReference(rowNumber),
					ReportProcessingErrorSeverity.Error));
			}
			else if (pageHeight <= 0)
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("c5135322-bb38-40c7-a609-f4806d3ecf9a", "Page height should be larger than zero."),
					GetColumnABasedCellReference(rowNumber),
					ReportProcessingErrorSeverity.Error));
			}
			else
			{
				CustomPageHeightInMillimeters = pageHeight;
			}
		}

		string GetAvailablePageStyles()
		{
			return "'" + string.Join("', '", Enum.GetNames(typeof(PageStyles))) + "'";
		}

		void ProcessTrailingFormFeed(string line, int rowNumber)
		{
			string pageSizeText = line.Substring(Constants.ConfigAreaParameters.TrailingFormFeedSignature.Length);
			try
			{
				TrailingFormFeedLengthIn360thsOfAnInch = uint.Parse(pageSizeText);
			}
			catch (FormatException ex)
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("354cdfb6-6f71-4ea7-8bd4-4f56d3e0b260", "Trailing form feed size must be specified as a whole number, measured in 360ths of an inch"),
					GetColumnABasedCellReference(rowNumber),
					ReportProcessingErrorSeverity.Error, ex));
			}
			catch (OverflowException ex)
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("c0d3e766-e449-4122-982b-2529fb5dd9f8", "Trailing form feed size must be between {0} and {1}", uint.MinValue, uint.MaxValue),
					GetColumnABasedCellReference(rowNumber),
					ReportProcessingErrorSeverity.Error, ex));
			}
		}

		Currency CurrencyProvider;

		#region ProcessDocumentCurrency

		void ProcessDocumentCurrency(string line, int rowNumber)
		{
			string documentCurrency = line.Substring(Constants.ConfigAreaParameters.DocumentCurrency.Length);

			if (IsSingleMacro(documentCurrency))
			{
				documentCurrency = GetValueForSingleMacro(documentCurrency).ToString();
			}

			if (CurrencyProvider == null)
			{
				CurrencyProvider = new Currency();
			}
			DocumentCurrencyPattern = CurrencyProvider.GetFormatPatternForCurrency(documentCurrency);

			if (!string.IsNullOrEmpty(DocumentCurrencyPattern))
			{
				for (int col = 1; col < ParentReport.WorkSheetCurrentlyBeingProcessed.ColumnCount; col++)
				{
					if (ParentReport.WorkSheetCurrentlyBeingProcessed[rowNumber, col].ToString().Trim().ToLower() == Constants.ConfigAreaParameters.DocumentCurrencyApplyExpression.ToLower())
					{
						ColumnsWithCurrencySetup.Add(col);
					}
				}
			}
			else
			{
				ParentReport.ErrorManager.Add(new ReportProcessingError(Res.GetString("f01ca249-71af-47a9-9f29-33b21ae1e4df", "Currency code not found : {0}", documentCurrency)
					, new CellReference(ParentReport.WorkSheetCurrentlyBeingProcessed.SheetName, "A" + (rowNumber + 1).ToString())
					, ReportProcessingErrorSeverity.Error));
			}
		}

		Dictionary<int, int> ColumnCurrentPositionsIndexedByOriginalColumnPositions
		{
			get
			{
				if (columnCurrentPositionsIndexedByOriginalColumnPositions == null)
				{
					columnCurrentPositionsIndexedByOriginalColumnPositions = new Dictionary<int, int>();
					if (CurrentColumnHeadings != null)
					{
						int startingColumn = CurrentColumnHeadings[0].OriginalColumnNumber;
						foreach (ColumnHeading column in CurrentColumnHeadings)
						{
							columnCurrentPositionsIndexedByOriginalColumnPositions.Add(column.OriginalColumnNumber, startingColumn + column.CurrentPosition);
						}
					}
				}
				return columnCurrentPositionsIndexedByOriginalColumnPositions;
			}
		}
		Dictionary<int, int> columnCurrentPositionsIndexedByOriginalColumnPositions;

		ColumnHeadingCollection CurrentColumnHeadings
		{
			get
			{
				if (currentColumnHeadings == null && ParentReport.ColumnHeadingManager.CurrentConfiguration.Worksheets.Count > 0)
				{
					currentColumnHeadings = ParentReport.ColumnHeadingManager.CurrentConfiguration.Worksheets[ParentReport.WorkSheetCurrentlyBeingProcessed.SheetName].ColumnHeadings;
				}
				return currentColumnHeadings;
			}
		}
		ColumnHeadingCollection currentColumnHeadings;

#if DEBUG

		public ColumnHeadingCollection CurrentColumnHeadingsExposedForTest
		{
			get
			{
				return CurrentColumnHeadings;
			}
		}

		public Dictionary<int, int> ColumnCurrentPositionsIndexedByOriginalColumnPositionsExposedForTest
		{
			get
			{
				return ColumnCurrentPositionsIndexedByOriginalColumnPositions;
			}
		}

		public
#endif
 bool IsSingleMacro(string value)
		{
			return ParentReport.IsSingleMacroCache.GetOrAdd(value, key => RegexProvider.IsSingleMacro(value.Trim()));
		}

		object GetValueForSingleMacro(string macro)
		{
			object result = "";
			try
			{
				if (ParentReport.Renderer != null)
				{
					result = ParentReport.MacroTranslator.GetValue(macro, ParentReport.Renderer.CurrentPass);
				}
			}
			catch (FieldNotFoundException exception)
			{
				exception.AddAsWarningToReport(ParentReport);
			}
			return result;
		}

		#endregion

		#endregion
	}
}
