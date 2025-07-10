using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using FlexCel.Core;

namespace Enterprise.DocumentEngine
{
	class ReportAnalyser
	{
		public ReportAnalyser(Report report)
		{
			Argument.NotNull(report, "report");

			this.report = report;
		}

		readonly Report report;

		bool HasAnalyserErrors;

		public void AnalyseReportInBuildTask()
		{
			ReadConfigAreaInTaskBuild();
			ReadConstantsFromConstantsTab();
			ReadSortOrdersFromSortOrdersTab();
			ReadGroupBysFromGroupByTab();
			ReadFiltersFromFiltersTab(false);
			ReadOptionalTemplatesFromOptionalTemplatesTab(false);
			ReadUserDefinedFieldsFromFieldsTab();
		}

		public void Analyse()
		{
			ReadConfigArea();
			AddDataSourcesFromConfigArea();
			ReadConstantsFromConstantsTab();
			ProcessDisableFixedValueCache();

			if (!report.SortOrderLoadedByReportAnalyser)
			{
				ReadSortOrdersFromSortOrdersTab();
			}

			if (!report.GroupBysLoadedByReportAnalyser)
			{
				ReadGroupBysFromGroupByTab();
			}

			if (!report.FilterLoadedByReportAnalyser)
			{
				report.ColumnHeadingManager.LinkedFilterFields.Clear();
				ReadFiltersFromFiltersTab();

				string saveToFilterField = report.ColumnHeadingManager.SaveToFilterField;
				if (!string.IsNullOrEmpty(saveToFilterField) && !(report.FilterCollection[saveToFilterField] is LookupField))
				{
					using (report.ErrorManager.EvaluatingOuterContent(report.ColumnHeadingManager.ColumnHeadingCellContent))
					{
						report.ErrorManager.Add(new ReportProcessingError(
							Res.GetString("bae54d59-a095-4af5-846e-a1dde788784c", "Filter [{0}] is not a table based filter field. [{1}] must point to a table based filter field. You will not be able to save configurations against this filter value.", saveToFilterField, "SavesTo")
							, Config.SavesToFieldCellReference
							, ReportProcessingErrorSeverity.Warning));
					}
				}
			}
			ReadUserDefinedFieldsFromFieldsTab();

			if (!report.OptionalTemplatesLoadedByReportAnalyser)
			{
				ReadOptionalTemplatesFromOptionalTemplatesTab();
			}

			report.SynchroniseReportDataWithDeserialisedReport();

			if (report.ColumnHeadingManager.LinkedFilterFields.Count == 0)
			{
				foreach (var filter in report.FilterCollection)
				{
					var filterField = filter as FilterField;
					if (filterField != null)
					{
						report.ColumnHeadingManager.AddLinkedField(filterField);
					}
				}
			}

			HideConditionalRowsExcludedByEvaluatedHashIfs();
			ProcessAddressAreas();

			if (!HasAnalyserErrors)
			{
				ReadAllAreasExceptConfig();
				PushAreasIntoTypedPropertiesAndSectionsList();
				ValidateAreasWithinSections();
			}

			if (report.LinkedLookupField != null)
			{
				report.LinkedLookupField.IsLinkedToContactUser = true;
			}

			if (ReadCharts())
			{
				report.HasChart = true;
			}

			report.IsAnalyzed = true;
		}

		bool ReadCharts()
		{
			var xlsFile = report.XlInterface.Xls;
			var oldActiveSheet = xlsFile.ActiveSheet;

			try
			{
				for (var sheetIndex = 1; sheetIndex <= xlsFile.SheetCount; sheetIndex++)
				{
					xlsFile.ActiveSheet = sheetIndex;
					for (var chartIndex = 1; chartIndex <= xlsFile.ObjectCount; chartIndex++)
					{
						var props = xlsFile.GetObjectProperties(chartIndex, true);
						if (ProcessChart(xlsFile, chartIndex, props))
						{
							return true;
						}
					}

					if (xlsFile.SheetType == TSheetType.Chart)
					{
						if (xlsFile.GetChart(1, null) != null)
						{
							return true;
						}
					}
				}
				return false;
			}
			finally
			{
				xlsFile.ActiveSheet = oldActiveSheet;
			}
		}

		bool ProcessChart(ExcelFile xls, int iChart, TShapeProperties props)
		{
			if (props.ObjectType == TObjectType.Chart && xls.GetChart(iChart, props.ObjectPath) != null)
			{
				return true;
			}

			for (var i = 1; i <= props.ChildrenCount; i++)
			{
				var childProp = props.Children(i);
				if (ProcessChart(xls, i, childProp))
				{
					return true;
				}
			}

			return false;
		}

		public string[] GetExcludedAttachmentTypes()
		{
			var attachmentTypes = new HashSet<string>();
			if (report != null)
			{
				if (report.HasChart)
				{
					var templateFormat = report.XlInterface?.GetExtensionForExcelFromFile();
					if (templateFormat == AttachmentTypeList.Codes.Xls)
					{
						attachmentTypes.Add(AttachmentTypeList.Codes.Xlsx);
					}
					if (templateFormat == AttachmentTypeList.Codes.Xlsx)
					{
						attachmentTypes.Add(AttachmentTypeList.Codes.Xls);
					}
				}

				if (report.Analyser is { Config.DisableXLSXExport: true })
				{
					attachmentTypes.Add(AttachmentTypeList.Codes.Xlsx);
				}

				if (report.Analyser is { Config.DisableCSVExport: true })
				{
					attachmentTypes.Add(AttachmentTypeList.Codes.Csv);
				}
			}
			return attachmentTypes.ToArray();
		}

		void ProcessAddressAreas()
		{
			var addressPositionProvider = new AddressPositionProvider();
			string companyCountryAddressPosition = addressPositionProvider.GetAddressPosition();
			var addressPosition = companyCountryAddressPosition;

			var workSheet = report.WorkSheetCurrentlyBeingProcessed;

			for (int row = 0; row < LastRowOfTemplate; row++)
			{
				var areaId = workSheet[row, 0].ToString();

				using (report.ErrorManager.EvaluatingCell(new CellReference(workSheet.SheetName, row, 0)))
				{
					var isLeftHandAddressId = areaId.StartsWith(Constants.AddressTags.LeftHandAddress, StringComparison.OrdinalIgnoreCase);
					if (isLeftHandAddressId)
					{
						using (report.ErrorManager.EvaluatingOuterContent(areaId))
						{
							var leftHandAddressRow = row;
							var rightHandAddressRow = FindNextAreaIdRow(Constants.AddressTags.RightHandAddress, leftHandAddressRow);
							if (rightHandAddressRow != -1)
							{
								var endAddressRow = FindNextAreaIdRow(Constants.AddressTags.EndAddress, rightHandAddressRow);
								if (endAddressRow != -1)
								{
									RemoveRows(workSheet, endAddressRow, endAddressRow + 1);
									RemoveRows(workSheet, rightHandAddressRow, rightHandAddressRow + 1);
									RemoveRows(workSheet, leftHandAddressRow, leftHandAddressRow + 1);

									rightHandAddressRow -= 1;
									endAddressRow -= 2;

									var leftHandAddressEndRow = rightHandAddressRow;
									var rightHandAddressEndRow = endAddressRow;

									switch (addressPosition)
									{
										case AddressPositionList.Codes.Left:
											RemoveRows(workSheet, rightHandAddressRow, rightHandAddressEndRow);
											break;

										case AddressPositionList.Codes.Right:
											RemoveRows(workSheet, leftHandAddressRow, leftHandAddressEndRow);
											break;
									}
								}
								else
								{
									AddError(GetProcessAddressAreaErrorMessage(Constants.AddressTags.LeftHandAddress, row, Constants.AddressTags.EndAddress), row);
								}
							}
							else
							{
								AddError(GetProcessAddressAreaErrorMessage(Constants.AddressTags.LeftHandAddress, row, Constants.AddressTags.RightHandAddress), row);
							}
						}
					}
					else if (areaId.StartsWith(Constants.AddressTags.RightHandAddress, StringComparison.OrdinalIgnoreCase))
					{
						AddError(GetProcessAddressAreaErrorMessage(Constants.AddressTags.RightHandAddress, row, Constants.AddressTags.LeftHandAddress), row);
					}
					else if (areaId.StartsWith(Constants.AddressTags.EndAddress, StringComparison.OrdinalIgnoreCase))
					{
						AddError(GetProcessAddressAreaErrorMessage(Constants.AddressTags.EndAddress, row, Constants.AddressTags.LeftHandAddress), row);
					}
				}

				if (HasAnalyserErrors)
				{
					break;
				}
			}
		}

		/// <summary>
		/// We want to call this method whenever a report has expanded or removed rows to update the DataRowIndexWithRowRanges property in all SectionBodyArea
		/// </summary>
		internal void UpdateSectionBodyAreaRowRangesInReport(int rowChangeIndex, int delta)
		{
			Areas.OfType<SectionBodyArea>().ForEach(body => body.ShiftForAllChildForeachAreas(rowChangeIndex, delta));

			foreach (var bodyArea in Areas.OfType<SectionBodyArea>().Where(b => !b.IsClonedForPageBreaking))
			{
				bodyArea.UpdateRowRangesWithIndexIncludeAllChildren(rowChangeIndex, delta);
			}
		}

		string GetProcessAddressAreaErrorMessage(string addressAreaId, int row, string expectedAreaId)
		{
			return Res.GetString("3d10a4c3-1de4-4748-bc5b-71b78bff0ec4", "{0} in row number {1} does not have {2}.", addressAreaId, row, expectedAreaId);
		}

		int FindNextAreaIdRow(string areaId, int startRow)
		{
			var result = -1;

			for (var row = startRow; row < LastRowOfTemplate; row++)
			{
				var areaIdAtRow = report.WorkSheetCurrentlyBeingProcessed[row, 0].ToString();
				if (areaIdAtRow.StartsWith(areaId, StringComparison.OrdinalIgnoreCase))
				{
					result = row;
					break;
				}
			}

			return result;
		}

		const int LandscapeFullPageSize = 11750;
		const int LandscapeMarginMultiplier = 1305;
		const int PortraitFullPageSize = 16950;
		const int PortraitMarginMultiplier = 1450;
		const int LetterPortraitFullPageSize = 16280;
		const int CustomsLetterPortraitFullPageSize = 15500;
		const int LetterLandscapeFullPageSize = 13300;
		const int LetterPortraitMatrixFullPageSize = 14570;
		const int LabelFullPageSize = 8698;
		const int PageHeightMultiplier = 57; // PortraitFullPageSize / 297mm

		public int PageHeight
		{
			get
			{
				switch (Config.PageStyle)
				{
					case PageStyles.Portrait:
						return PortraitFullPageSize - PortraitVerticalMargin;

					case PageStyles.Landscape:
						return LandscapeFullPageSize - LandscapeVerticalMargin;

					case PageStyles.LetterPortrait:
						return LetterPortraitFullPageSize - PortraitVerticalMargin;

					case PageStyles.CustomsLetterPortrait:
						return CustomsLetterPortraitFullPageSize - PortraitVerticalMargin;

					case PageStyles.LetterLandscape:
						return LetterLandscapeFullPageSize - LandscapeVerticalMargin;

					case PageStyles.LetterPortraitMatrix:
						return LetterPortraitMatrixFullPageSize - PortraitVerticalMargin;

					case PageStyles.Label:
						return LabelFullPageSize;

					case PageStyles.Continuous:
						return int.MaxValue;

					case PageStyles.Custom:
						return Config.CustomPageHeightInMillimeters > 0 ? Config.CustomPageHeightInMillimeters * PageHeightMultiplier : int.MaxValue;
				}

				throw new DocumentEngineException($"Page style can not have any value other than {string.Join(",", Enum.GetNames(typeof(PageStyles)))}.");
			}
		}

		int LandscapeVerticalMargin
		{
			get { return GetVerticalMargin(LandscapeMarginMultiplier); }
		}

		int PortraitVerticalMargin
		{
			get { return GetVerticalMargin(PortraitMarginMultiplier); }
		}

		int GetVerticalMargin(int marginMultiplier)
		{
			return (int)(report.WorkSheetCurrentlyBeingProcessed.GetTopMargin() * marginMultiplier) + (int)(report.WorkSheetCurrentlyBeingProcessed.GetBottomMargin() * marginMultiplier);
		}

		internal List<(DataSourceTypes DataSourceType, string TableNameAndSelectStatement)> ReportSQLSources
		{
			get { return reportSQLSources ?? (reportSQLSources = new List<(DataSourceTypes, string)>()); }
		}
		List<(DataSourceTypes, string)> reportSQLSources;

		public List<Section> Sections
		{
			get { return sections ?? (sections = new List<Section>()); }
		}
		List<Section> sections;

		public StringCollection DataSourceParameters
		{
			get { return dataSourceParameters ?? (dataSourceParameters = new StringCollection()); }
		}
		StringCollection dataSourceParameters;

		public List<Area> Areas
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return areas ?? (areas = new List<Area>()); }
		}
		List<Area> areas;

		#region Exposed Individual Areas

		public ConfigArea Config
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return areas.Count > 0 ? (ConfigArea)Areas[0] : null; }
		}

		public DocumentHeaderArea DocumentHeader
		{
			get { return fDocumentHeader; }
		}
		DocumentHeaderArea fDocumentHeader;

		public PageHeaderArea PageHeader
		{
			get { return fPageHeader; }
		}
		PageHeaderArea fPageHeader;

		public FooterArea LastPageFooter
		{
			get { return fLastPageFooter; }
		}
		FooterArea fLastPageFooter;

		public FooterArea OnlyOnePageFooter
		{
			get { return fOnlyOnePageFooter; }
		}
		FooterArea fOnlyOnePageFooter;

		public PageFooterArea PageFooter
		{
			get { return fPageFooter; }
		}
		PageFooterArea fPageFooter;

		public FirstPageFooterArea FirstPageFooter
		{
			get { return fFirstPageFooter; }
		}
		FirstPageFooterArea fFirstPageFooter;

		public DocumentFooterArea DocumentFooter
		{
			get { return fDocumentFooter; }
		}
		DocumentFooterArea fDocumentFooter;

		public BackPageArea BackPage
		{
			get { return fBackPage; }
		}
		BackPageArea fBackPage;

		#endregion

		public ValidatorPack ValidatorPack
		{
			get { return fValidatorPack ?? (fValidatorPack = new ValidatorPack()); }
		}
		ValidatorPack fValidatorPack;

		#region Conditional Area

		internal bool ShouldHideConditionalColumn(int columnIndex)
		{
			string expressionWithMacrosInIt = Config.HideColumnExpressions[columnIndex];
			string expression;
			ConditionalColumnErrorVetter vetter = new ConditionalColumnErrorVetter(report);
			using (var suspender = report.ErrorManager.GetErrorCheckingSuspender(vetter.ShouldHideError))
			{
				expression = GetExpressionWithMacrosReplaced(expressionWithMacrosInIt);
				if (suspender.HidError)
				{
					AddNonExistantOptionalColumn(columnIndex);
					return true;
				}
			}
			return ExpressionEvaluator.Evaluate(expression, RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value);
		}

		class ConditionalColumnErrorVetter
		{
			public ConditionalColumnErrorVetter(Report report)
			{
				this.report = report;
			}

			public bool ShouldHideError(IReportProcessingError error)
			{
				if (FieldWithError == null
					&& (error.Severity == ReportProcessingErrorSeverity.Warning || error.Severity == ReportProcessingErrorSeverity.WarningWithoutErrorReport)
					&& error.InnerMacro.ContainsNumber())
				{
					// If the error said that a field could not be found on the datasource, add it to the list of "Non Existant Optional Columns" and return true.
					FieldWithError = error.InnerMacro;
					return true;
				}

				// Send the diagnostic information back about "Column 'Type6' is not found in Table 'HeaderData'" issue
				if (!report.ContainsAnyCustomisation)
				{
					var message = new StringBuilder();
					message.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"FieldWithError = {0}, error.Severity = {1}, error.InnerMacro = {2}\r\n", FieldWithError ?? string.Empty, error.Severity, error.InnerMacro ?? string.Empty);
					message.AppendFormat(CultureInfo.InvariantCulture, (NoResString)"error.CellName = {0}, error.Message = {1}, error.TemplatePath = {2}, error.SheetName = {3}, error.OuterContent = {4}, error.Occurrences = {5}\r\nerror.StackTrace = {6}\r\n",
						error.CellName ?? string.Empty,
						error.Message ?? string.Empty,
						error.TemplatePath ?? string.Empty,
						error.SheetName ?? string.Empty,
						error.OuterContent ?? string.Empty,
						error.Occurrences,
						error.Exception?.StackTrace ?? string.Empty);
					ErrorReporter.ReportOnce("DocumentEngine_ReportAnalyser_ConditionalColumnErrorVetter_ShouldHideError", message.ToString());
				}

				return false;
			}

			public string FieldWithError { get; private set; }
			readonly Report report;
		}

		void AddNonExistantOptionalColumn(int nonExistantOptionalColumnIndex)
		{
			if (!NonExistantOptionalColumns.Contains(nonExistantOptionalColumnIndex))
			{
				NonExistantOptionalColumns.Add(nonExistantOptionalColumnIndex);
			}
		}

		internal bool NonExistantOptionalColumnsContains(int columnIndex)
		{
			return NonExistantOptionalColumns.Contains(columnIndex);
		}

		internal List<int> NonExistantOptionalColumns
		{
			get { return fNonExistantOptionalColumns ?? (fNonExistantOptionalColumns = new List<int>()); }
		}
		List<int> fNonExistantOptionalColumns;

#if DEBUG
		virtual
#endif
 internal bool EvaluateExpressionForConditionalAreas(string expressionWithMacrosInIt)
		{
			string expression = GetExpressionWithMacrosReplaced(expressionWithMacrosInIt);
			return ExpressionEvaluator.Evaluate(expression, RawDataRegistry.Instance.UseJSEngineForDocumentMacroEvaluation.Value);
		}

		string GetExpressionWithMacrosReplaced(string expression)
		{
			using (report.ErrorManager.EvaluatingOuterContent(expression))
			{
				do
				{
					string expressionBeforeReplace = expression;
					expression = RegexProvider.InnermostMacrosRegex.Replace(expression, new MatchEvaluator(ReplaceSingleMacrosForConditionalAreas));
					if (expressionBeforeReplace == expression || expressionBeforeReplace == expression.Replace("\\", ""))
					{
						break;
					}
				}
				while (RegexProvider.InnermostMacrosRegex.IsMatch(expression));
			}
			return expression;
		}
		#endregion

		internal void ReadUserDefinedFieldsFromFieldsTab()
		{
			UserDefinedFieldCollectionBuilder userDefinedFieldCollectionBuilder = new UserDefinedFieldCollectionBuilder(report.UDFSheet, report.Analyser.ValidatorPack, report.ReplaceSingleMacroNotInTemplateBody);
			userDefinedFieldCollectionBuilder.Build();
			report.UserDefinedFieldList = userDefinedFieldCollectionBuilder.UserDefinedFields;
			report.ErrorManager.AddRange(userDefinedFieldCollectionBuilder.Errors);
		}

		static readonly Regex DataSection = new Regex("^(Data:|EDWData:|EDWOnlyData:)([^=]+)=(.+)", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline | RegexOptions.Compiled);

		string ReplaceSingleMacrosForConditionalAreas(Match match)
		{
			string macroToReplace = match.Groups[0].Value;
			using (report.ErrorManager.EvaluatingInnerMacro(macroToReplace))
			{
				if (IsProcessingDisableFixedValueCacheFlag)
				{
					// Use a seperate macro translator in order to avoid registering UDF which will be triggered in getter of report.MacroTranslator
					return report.MacroTranslatorForProcessingDisableFixedValueCacheFlag.GetValue(macroToReplace, Passes.FirstPass).ToString().EscapeQuotes();
				}

				return report.MacroTranslator.GetValue(macroToReplace, Passes.FirstPass).ToString().EscapeQuotes();
			}
		}

		bool IsProcessingDisableFixedValueCacheFlag { get; set; }

		internal int LastRowOfTemplate
		{
			get
			{
				if (fLastRowOfTemplate == 0)
				{
					for (int i = 0; i < report.XlInterface.MaxRowCountSupportedByCurrentExcelFile; i++)
					{
						if (report.WorkSheetCurrentlyBeingProcessed[i, 0].ToString().Equals(Constants.AreaIdentifierTags.EndOfReport, StringComparison.OrdinalIgnoreCase))
						{
							fLastRowOfTemplate = i;
							break;
						}
					}
				}

				if (fLastRowOfTemplate == 0)
				{
					AddError(Res.GetString("7d7d7a0f-c956-42d3-80aa-f13b385db28a", "Template must have a {0} area identifier marking its end.", "#EndOfReport"), 0);
				}

				return fLastRowOfTemplate;
			}
		}
		int fLastRowOfTemplate;

		int FindRelatedElse(int start, int end)
		{
			int depth = 0;
			string cellValue;
			int row;
			bool found = false;
			for (row = start + 1; row < end; row++)
			{
				cellValue = report.WorkSheetCurrentlyBeingProcessed[row, 0].ToString();
				if (cellValue.StartsWith(Constants.ConditionalTags.If, StringComparison.OrdinalIgnoreCase))
				{
					depth++;
				}
				else
				{
					if (cellValue.StartsWith(Constants.ConditionalTags.EndIf, StringComparison.OrdinalIgnoreCase))
					{
						depth--;
					}
					else
					{
						if (cellValue.StartsWith(Constants.ConditionalTags.Else, StringComparison.OrdinalIgnoreCase) && depth == 0)
						{
							found = true;
							break;
						}
					}
				}
			}
			return found ? row : -1;
		}

		int FindRelatedEndIf(int start, int end)
		{
			int result = -1;
			int depth = 0;

			for (var row = start + 1; row < end; row++)
			{
				string cellValue = report.WorkSheetCurrentlyBeingProcessed[row, 0].ToString();
				if (cellValue.StartsWith(Constants.ConditionalTags.If, StringComparison.OrdinalIgnoreCase))
				{
					depth++;
				}
				else
				{
					if (cellValue.StartsWith(Constants.ConditionalTags.EndIf, StringComparison.OrdinalIgnoreCase))
					{
						if (depth == 0)
						{
							result = row;
							break;
						}
						else
						{
							depth--;
						}
					}
				}
			}

			return result;
		}

		void ProcessBlock(int startRowIndex, int endRowIndex, string expression)
		{
			int elseRowIndex = FindRelatedElse(startRowIndex, endRowIndex);
			var workSheet = report.WorkSheetCurrentlyBeingProcessed;

			var result = false;

			try
			{
				result = EvaluateExpressionForConditionalAreas(expression);
			}
			catch (ExpressionEvaluationException exception)
			{
				AddError(exception.Message, startRowIndex + rowsRemoved);
			}
			catch (FieldNotFoundException exception)
			{
				AddError(exception.Message, startRowIndex + rowsRemoved);
			}

			if (result)
			{
				RemoveRows(workSheet, elseRowIndex != -1 ? elseRowIndex : endRowIndex, endRowIndex + 1);
				RemoveRows(workSheet, startRowIndex, startRowIndex + 1);
			}
			else
			{
				RemoveRows(workSheet, endRowIndex, endRowIndex + 1);
				RemoveRows(workSheet, startRowIndex, elseRowIndex == -1 ? endRowIndex : elseRowIndex + 1);
			}
		}

		void RemoveRows(ExcelWorkSheet workSheet, int startRow, int endRow)
		{
			workSheet.RemoveRows(startRow, endRow);
			rowsRemoved += endRow - startRow;
		}

		int rowsRemoved;

		void HideConditionalRowsExcludedByEvaluatedHashIfs()
		{
			for (int rowIndex = 0; rowIndex < LastRowOfTemplate; rowIndex++)
			{
				var cellContentAsString = report.WorkSheetCurrentlyBeingProcessed[rowIndex, 0].ToString();

				using (report.ErrorManager.EvaluatingCell(new CellReference(report.WorkSheetCurrentlyBeingProcessed.SheetName, rowIndex, 0)))
				{
					if (cellContentAsString.StartsWith(Constants.ConditionalTags.If, StringComparison.OrdinalIgnoreCase))
					{
						using (report.ErrorManager.EvaluatingOuterContent(cellContentAsString))
						{
							var expression = cellContentAsString.Substring(3);
							var start = rowIndex;
							var end = FindRelatedEndIf(start, LastRowOfTemplate);
							if (end == -1)
							{
								AddError(Res.GetString("35e822af-13a6-42a8-8f27-86590fdb837d", "#IF in row number {0} does not have #ENDIF", rowIndex), rowIndex);
								return;
							}

							ProcessBlock(start, end, expression);
							rowIndex--;
						}
					}
					else if (cellContentAsString.StartsWith(Constants.ConditionalTags.Else, StringComparison.OrdinalIgnoreCase))
					{
						AddError(Res.GetString("9480cd77-9a08-4ad4-ba2c-5cf85fbff91f", "#ELSE in row number {0} does not have #IF", rowIndex + rowsRemoved), rowIndex + rowsRemoved);
					}
					else if (cellContentAsString.StartsWith(Constants.ConditionalTags.EndIf, StringComparison.OrdinalIgnoreCase))
					{
						AddError(Res.GetString("2b48d299-c97f-4b87-9da8-e1658abcc6ec", "#ENDIF in row number {0} does not have #IF", rowIndex + rowsRemoved), rowIndex + rowsRemoved);
					}
				}
			}
		}

		void ProcessDisableFixedValueCache()
		{
			if (Config != null)
			{
				IsProcessingDisableFixedValueCacheFlag = true;

				try
				{
					string disableFixedValueCacheExpression = Config.DisableFixedValueCacheExpression;
					if (!string.IsNullOrEmpty(disableFixedValueCacheExpression))
					{
						if (disableFixedValueCacheExpression == "Y" || EvaluateExpressionForConditionalAreas(disableFixedValueCacheExpression))
						{
							report.DisableFixedValueCache = true;
						}
					}
				}
				finally
				{
					IsProcessingDisableFixedValueCacheFlag = false;
				}
			}
		}

		ReportRunningType GetReportRunningType(Report report)
		{
			if (report.MenuItem?.PK == ReportRunningConstants.DocumentReferenceMenuItemPk)
			{
				return ReportRunningType.DocumentReferenceGuide;
			}

			if (report.MenuItem?.PK == ReportRunningConstants.ReportReferenceMenuItemPk)
			{
				return ReportRunningType.ReportReferenceGuide;
			}

			if (report.Style == Report.Styles.Document)
			{
				return ReportRunningType.Document;
			}

			if (report.ScheduleTask != null)
			{
				if (report.ScheduleTask.S5_IsPrivate)
				{
					return ReportRunningType.OneOffScheduledReport;
				}

				return ReportRunningType.NormalScheduledReport;
			}

			return ReportRunningType.Report;
		}

		void ReadFiltersFromFiltersTab(bool isInRuntime = true)
		{
			var templateFileName = Path.GetFileNameWithoutExtension(report.StTemplate?.SO_ExcelTemplatePath);
			var reportRunningType = GetReportRunningType(report);
			var filterCollectionBuilder = new FilterCollectionBuilder(DataSourceParameters, ValidatorPack, new StringTreeBuilder(report.FilterSheet).GetTree(), report.ReplaceSingleMacroNotInTemplateBody, report.ColumnHeadingManager, templateFileName, reportRunningType);
			filterCollectionBuilder.Build(report.Style == Report.Styles.Report, isInRuntime);
			report.FilterCollection = filterCollectionBuilder.IFilterCollection;
			report.FilterLoadedByReportAnalyser = true;

			if (report.StTemplate != null && report.StTemplate.SO_IsSystemDefined && report.Style == Report.Styles.Report)
			{
				foreach (FilterField filter in report.FilterCollection)
				{
					if (!string.IsNullOrEmpty(filter.Language))
					{
						using (Res.TemporarilySwitchLanguage(filter.Language))
						{
							filter.DisplayNameLocalizedData = DocBuilderResourceStrings.GetReportStringData(templateFileName, filter.DisplayName);
						}
					}
					else
					{
						filter.DisplayNameLocalizedData = DocBuilderResourceStrings.GetReportStringData(templateFileName, filter.DisplayName);
					}

					filter.GroupName = DocBuilderResourceStrings.GetReportString(templateFileName, filter.GroupName);
					filter.GroupDescription = DocBuilderResourceStrings.GetReportString(templateFileName, filter.GroupDescription);

					if (filter is MultipleSelectionLookup multipleSelectionLookup)
					{
						foreach (var column in multipleSelectionLookup.Columns.Where(column => !string.IsNullOrEmpty(column.ColumnName)))
						{
							column.CaptionLocalizedData = DocBuilderResourceStrings.GetReportStringData(templateFileName, column.Caption);
						}
					}
				}

				CodeDescriptionPairList translatedGroups = new CodeDescriptionPairList();
				foreach (CodeDescriptionPair sourceGroup in report.FilterCollection.FilterGroups)
				{
					translatedGroups.AddPair(DocBuilderResourceStrings.GetReportString(templateFileName, sourceGroup.Code),
						DocBuilderResourceStrings.GetReportString(templateFileName, sourceGroup.Description));
				}
				report.FilterCollection.FilterGroups = translatedGroups;
			}

			if (isInRuntime)
			{
				var json = JsonConverterHelper.Serialize(filterCollectionBuilder.IFilterCollection);
				report.ColumnHeadingManager.DefaultFilters = JsonConverterHelper.Deserialize<CollectionOfIFilter>(json);
			}

			report.ErrorManager.AddRange(filterCollectionBuilder.Errors);
		}

		void ReadConstantsFromConstantsTab()
		{
			if (report.ConstantsSheet != null)
			{
				StringTreeBuilder constantsTree = new StringTreeBuilder(report.ConstantsSheet);
				StringTreeNode root = constantsTree.GetTree();
				if (root.ChildExists((NoResString)"Constants"))
				{
					foreach (StringTreeNode constantNode in root.FindChild((NoResString)"Constants").Children)
					{
						AddNodeToConstantsIfNotAlreadyAdded(constantNode);
					}
				}
				else
				{
					AddError(Res.GetString("6f513a64-2159-40c0-a7df-df6370a51128", "Constants tab page should start with 'Constants'."), report.ConstantsSheet.SheetName, 0);
				}
			}

			report.AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.ReportName, report.Name);
			report.AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.DocumentDirection, report.Direction.ToString());
			report.AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.DeliveryMode, report.DeliveryMode);

			if (report.Parent != null && report.Parent.StmMenuCommand != null)
			{
				report.AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.MenuTitle, report.Parent.StmMenuCommand.SU_MenuName);
				report.AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.MenuItemPK, report.Parent.StmMenuCommand.PK.ToString());
			}

			if (report.TypeOfContact != null)
			{
				report.AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, report.TypeOfContact.Code);
			}
			else if (IsCoverSheet)
			{
				if (report.Parent != null && report.Parent.StmMenuCommand != null)
				{
					report.AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.ContactType, report.Parent.StmMenuCommand.SU_ContactType);
				}
			}

			ZGuid brandedOrganisationPK = (report.Parent != null && report.Parent.BrandedOrganisation != null ? report.Parent.BrandedOrganisation.PK : ZGuid.Empty);
			report.AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.BrandedOrganisationPK, brandedOrganisationPK.ToString());

			ZGuid contactOrganisationPK = (report.Parent != null && report.Parent.Organisation != null ? report.Parent.Organisation.PK : ZGuid.Empty);
			report.AddTemplateDefinedConstantIfNotAlreadyAdded(DocumentEngineIntegration.Constants.TemplateDefined.ContactOrganisationPK, contactOrganisationPK.ToString());

			report.SetTemplateConstantsOnBODocDataProvider();
		}

		bool IsCoverSheet
		{
			get { return report.BODocDataProvider is DeliveryInstructions; }
		}

		protected void AddNodeToConstantsIfNotAlreadyAdded(StringTreeNode constantNode)
		{
			int intValue;
			if (int.TryParse(constantNode.Child().Value, out intValue))
			{
				report.AddTemplateDefinedConstantIfNotAlreadyAdded(constantNode.Value, intValue);
				report.AddSheetDefinedConstantIfNotAlreadyAdded(constantNode.Value, intValue);
			}
			else
			{
				report.AddTemplateDefinedConstantIfNotAlreadyAdded(constantNode.Value, constantNode.Child().Value);
				report.AddSheetDefinedConstantIfNotAlreadyAdded(constantNode.Value, constantNode.Child().Value);
			}
		}

		void ReadSortOrdersFromSortOrdersTab()
		{
			SortOrderCollectionBuilder sortOrderCollectionBuilder = new SortOrderCollectionBuilder(report.SortSheet);
			sortOrderCollectionBuilder.Build();
			report.SortOrderCollection = sortOrderCollectionBuilder.SortOrderCollection;
			if (report.StTemplate != null && report.StTemplate.SO_IsSystemDefined && report.Style == Report.Styles.Report)
			{
				var templateFileName = Path.GetFileNameWithoutExtension(report.StTemplate.SO_ExcelTemplatePath);
				foreach (RuntimeOptions.SortOrder sort in report.SortOrderCollection)
				{
					sort.DisplayNameLocalizedData = DocBuilderResourceStrings.GetReportStringData(templateFileName, sort.DisplayName);
				}
			}
			report.SortOrderLoadedByReportAnalyser = true;
			report.ErrorManager.AddRange(sortOrderCollectionBuilder.Errors);
		}

		void ReadGroupBysFromGroupByTab()
		{
			GroupByCollectionBuilder groupByCollectionBuilder = new GroupByCollectionBuilder(report.GroupBySheet);
			groupByCollectionBuilder.Build();
			report.GroupByCollection = groupByCollectionBuilder.GroupByCollection;
			if (report.StTemplate != null && report.StTemplate.SO_IsSystemDefined && report.Style == Report.Styles.Report)
			{
				var templateFileName = Path.GetFileNameWithoutExtension(report.StTemplate.SO_ExcelTemplatePath);
				foreach (GroupBy groupby in report.GroupByCollection)
				{
					groupby.DisplayNameLocalizedData = DocBuilderResourceStrings.GetReportStringData(templateFileName, groupby.DisplayName);
				}
			}
			report.GroupBysLoadedByReportAnalyser = true;
			report.ErrorManager.AddRange(groupByCollectionBuilder.Errors);
		}

		void ReadOptionalTemplatesFromOptionalTemplatesTab(bool isInRuntime = true)
		{
			OptionalTemplateSheetCollectionBuilder optionalTemplateSheetCollectionBuilder = new OptionalTemplateSheetCollectionBuilder(report.OptionalTemplatesSheet, report.TemplateSheets, ValidatorPack, isInRuntime);
			optionalTemplateSheetCollectionBuilder.Build();
			report.OptionalTemplateSheetCollection = optionalTemplateSheetCollectionBuilder.OptionalTemplateSheets;
			if (report.StTemplate != null && report.StTemplate.SO_IsSystemDefined && report.Style == Report.Styles.Report)
			{
				var templateFileName = Path.GetFileNameWithoutExtension(report.StTemplate.SO_ExcelTemplatePath);
				foreach (OptionalTemplateSheet sheet in report.OptionalTemplateSheetCollection)
				{
					sheet.DisplayNameLocalizedData = DocBuilderResourceStrings.GetReportStringData(templateFileName, sheet.DisplayName);
				}
			}
			AddMandatoryTemplatesToValidator();
			report.OptionalTemplatesLoadedByReportAnalyser = true;
			report.ErrorManager.AddRange(optionalTemplateSheetCollectionBuilder.Errors);
		}

		void AddMandatoryTemplatesToValidator()
		{
			AtLeastOneFilterNotEmptyValidator validator = ValidatorPack.GetAtLeastOneFilterNotEmptyValidatorForGroup(report.OptionalTemplateSheetCollection.ValidatorGroupName);
			foreach (string sheet in report.TemplateSheets)
			{
				if (!report.OptionalTemplateSheetCollection.Contains(sheet))
				{
					OptionalTemplateSheet optionalSheet = new OptionalTemplateSheet(report.Factory, sheet);
					optionalSheet.Selected = true;
					validator.AddFilterField(optionalSheet);
					optionalSheet.Validators.Add(validator);
				}
			}
		}

		void ReadConfigArea()
		{
			int startOfFirstAreaAfterConfig = 0;
			try
			{
				if (!report.WorkSheetCurrentlyBeingProcessed[0, 0].ToString().Trim().Equals(Constants.AreaIdentifierTags.Config, StringComparison.OrdinalIgnoreCase))
				{
					AddError(
						Res.GetString(
							"3972836c-3be4-4830-9479-262099bc117d",
							"Template must have a #Config area at the top of the first tab, but it is: \"{0}\"",
							report.WorkSheetCurrentlyBeingProcessed),
						0);
				}
				for (startOfFirstAreaAfterConfig = 1; startOfFirstAreaAfterConfig < report.XlInterface.MaxRowCountSupportedByCurrentExcelFile; startOfFirstAreaAfterConfig++)
				{
					if (IsAreaStart(report.WorkSheetCurrentlyBeingProcessed[startOfFirstAreaAfterConfig, 0].ToString()))
					{
						break;
					}
				}
			}
			finally // If you don't add in a Config Area, even if it's blank with nothing in it, other bits blow up before the error gets reported.
			{
				Areas.Add(AreaFactory.InstantiateArea(0, startOfFirstAreaAfterConfig - 1, report, Constants.AreaIdentifierTags.Config));
			}
		}

		void ReadConfigAreaInTaskBuild()
		{
			List<int> configRows = new List<int>();
			try
			{
				if (!report.WorkSheetCurrentlyBeingProcessed[0, 0].ToString().Trim().Equals(Constants.AreaIdentifierTags.Config, StringComparison.OrdinalIgnoreCase))
				{
					AddError(
						Res.GetString(
							"3972836c-3be4-4830-9479-262099bc117d",
							"Template must have a #Config area at the top of the first tab, but it is: \"{0}\"",
							report.WorkSheetCurrentlyBeingProcessed),
						0);
				}
				for (int configRow = 0; configRow < report.XlInterface.MaxRowCountSupportedByCurrentExcelFile; configRow++)
				{
					if (reportConfigsToAnalyzeInTaskBuild.Any(config => report.WorkSheetCurrentlyBeingProcessed[configRow, 0].ToString().StartsWith(config, StringComparison.OrdinalIgnoreCase)))
					{
						configRows.Add(configRow);
					}
				}
			}
			finally // If you don't add in a Config Area, even if it's blank with nothing in it, other bits blow up before the error gets reported.
			{
				Areas.Add(new ConfigArea(report, Constants.AreaIdentifierTags.Config, configRows.ToArray()));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule")]
		static readonly string[] reportConfigsToAnalyzeInTaskBuild = new string[]
		{
			Constants.ConfigAreaParameters.ColumnHeadingsSignature,
			Constants.ConfigAreaParameters.TemplateNameSignature,
			Constants.ConfigAreaParameters.DisableTranslate
		};

		internal string ReadSpecificParameterValueFromConfigArea(ExcelWorkSheet workSheet, string parameter)
		{
			var result = string.Empty;

			if (workSheet[0, 0].ToString().Trim().Equals(Constants.AreaIdentifierTags.Config, StringComparison.OrdinalIgnoreCase))
			{
				for (int startOfFirstAreaAfterConfig = 1; startOfFirstAreaAfterConfig < report.XlInterface.MaxRowCountSupportedByCurrentExcelFile; startOfFirstAreaAfterConfig++)
				{
					if (IsAreaStart(workSheet[startOfFirstAreaAfterConfig, 0].ToString()))
					{
						break;
					}

					var cellContent = workSheet[startOfFirstAreaAfterConfig, 0].ToString();
					if (cellContent.StartsWith(parameter, StringComparison.OrdinalIgnoreCase))
					{
						result = parameter.Contains("=") ? cellContent.Substring(parameter.Length) : parameter;
						break;
					}
				}
			}

			return result;
		}

		void AddDataSourcesFromConfigArea()
		{
			foreach (string dataSourceString in Config.DataSourceStrings)
			{
				AddDataSource(dataSourceString);
			}
		}

		protected void AddDataSource(string cellValue)
		{
			Match match = DataSection.Match(cellValue);
			string dataName = match.Groups[1].Value;
			string tableName = match.Groups[2].Value;
			string dataSource = match.Groups[3].Value;

			ReportSQLSource newSQLSource = new ReportSQLSource(tableName, dataSource);

			foreach (Match param in Regex.Matches(dataSource, "<([^<]*?)>"))
			{
				string name = param.Groups[1].Value;

				if (name.Equals("MAINID", StringComparison.OrdinalIgnoreCase))
				{
					AddError(Res.GetString("255c1a03-677e-4862-b5d2-4379675bf5fe", "This is an old style template, please replace '{0}'.", "MainID"), 0);
				}

				if (!DataSourceParameters.Contains(name.ToUpper(Culture.Invariant)))
				{
					DataSourceParameters.Add(name.ToUpper(Culture.Invariant));
				}
			}

			var dataSourceType = DataSourceTypes.NormalData;
			if (dataName.ToUpper() == Constants.ConfigAreaParameters.EDWDataSourceSignature)
			{
				dataSourceType = DataSourceTypes.EdwData;
			}
			else if (dataName.ToUpper() == Constants.ConfigAreaParameters.EDWOnlyDataSourceSignature)
			{
				dataSourceType = DataSourceTypes.EdwData;
				report.IsEdwDataSource = true;
			}

			ReportSQLSources.Add((dataSourceType, newSQLSource.TableName + ":" + newSQLSource.SelectStatement));
		}

		void ReadAllAreasExceptConfig()
		{
			int rowIndex;
			int previousAreaStart = 0;
			string previousAreaIdentifier = "";

			for (rowIndex = Config.End + 1; rowIndex <= LastRowOfTemplate; rowIndex++)
			{
				string areaIdentifier = report.WorkSheetCurrentlyBeingProcessed[rowIndex, 0].ToString();
				using (report.ErrorManager.EvaluatingCell(new CellReference(report.WorkSheetCurrentlyBeingProcessed.SheetName, rowIndex, 0)))
				{
					if (IsAreaStart(areaIdentifier) && !SectionForeachArea.IsSectionForeachAreaBeginStart(areaIdentifier) && !SectionForeachArea.IsSectionForeachAreaEndStart(areaIdentifier))
					{
						if (previousAreaStart != 0)
						{
							using (report.ErrorManager.EvaluatingOuterContent(previousAreaIdentifier))
							{
								Area newArea = AreaFactory.InstantiateArea(previousAreaStart, rowIndex - 1, report, previousAreaIdentifier);
								Areas.Add(newArea);

								if (newArea is EndOfReportArea)
								{
									previousAreaStart = 0;
									break;
								}
							}
						}
						previousAreaIdentifier = areaIdentifier;
						previousAreaStart = rowIndex;
					}
				}
			}

			if (previousAreaStart != 0)
			{
				Areas.Add(AreaFactory.InstantiateArea(previousAreaStart, rowIndex - 1, report, previousAreaIdentifier));
			}
		}

		protected virtual bool IsAreaStart(string cellContents)
		{
			return cellContents.StartsWith("#");
		}

		void ValidateAreasWithinSections()
		{
			AreaListManager areaTypeManager = new AreaListManager();
			foreach (Section section in Sections)
			{
				Area lastArea = null;
				bool sectionBodyFound = false;

				foreach (Area currentArea in section.SectionBodyAndGroupByAreas)
				{
					if (currentArea is SectionBodyArea)
					{
						sectionBodyFound = true;
					}
					if (lastArea != null)
					{
						if (!areaTypeManager.IsAreaInCorrectOrder(lastArea, currentArea))
						{
							AddOutOfOrderError(lastArea, currentArea);
						}
					}
					lastArea = currentArea;
				}

				if (!sectionBodyFound)
				{
					string workSheet = string.Concat("\r\nWorkSheetCurrentlyBeingProcessed:\r\n", report.WorkSheetCurrentlyBeingProcessed.ToString());
					AddError(Res.GetString("30cc85fe-4f6f-4ce1-adf2-de1886047f69", "Every Section in a template must contain a {0}.{1}", "#SectionBody", workSheet), section.StartingRowOfSection);
				}
			}
		}

		void PushAreasIntoTypedPropertiesAndSectionsList()
		{
			AreaListManager areaTypeManager = new AreaListManager();
			Area lastArea = null;
			Area lastSectionArea = null;
			foreach (Area currentArea in Areas)
			{
				if (lastArea != null)
				{
					if (currentArea is ConfigArea)
					{
						AddDuplicateError(currentArea);
					}
					else if (areaTypeManager.IsSectionArea(currentArea))
					{
						if (Sections.Count == 0 || !areaTypeManager.IsAreaInCorrectOrder(lastSectionArea, currentArea))
						{
							Sections.Add(new Section());
						}
						if (currentArea is SectionHeaderArea)
						{
							Sections[Sections.Count - 1].SectionHeader = (SectionHeaderArea)currentArea;
						}
						else if (currentArea is SectionPageHeaderArea)
						{
							Sections[Sections.Count - 1].SectionPageHeader = currentArea;
						}
						else if (currentArea is SectionPageFooterArea)
						{
							Sections[Sections.Count - 1].SectionPageFooter = currentArea;
						}
						else if (currentArea is SectionFooterArea)
						{
							Sections[Sections.Count - 1].SectionFooter = (SectionFooterArea)currentArea;
						}
						else
						{
							if (currentArea is SectionBodyArea)
							{
								Sections[Sections.Count - 1].TableName = ((SectionBodyArea)currentArea).TableName;
							}

							Sections[Sections.Count - 1].SectionBodyAndGroupByAreas.Add(currentArea);
							currentArea.OwnerSection = Sections[Sections.Count - 1];
						}

						lastSectionArea = currentArea;
					}
					else
					{
						if (!areaTypeManager.IsAreaInCorrectOrder(lastArea, currentArea))
						{
							AddOutOfOrderError(lastArea, currentArea);
						}

						if (currentArea is DocumentHeaderArea)
						{
							if (fDocumentHeader != null)
							{
								AddDuplicateError(currentArea);
							}

							fDocumentHeader = (DocumentHeaderArea)currentArea;
						}
						else if (currentArea is PageHeaderArea)
						{
							if (fPageHeader != null)
							{
								AddDuplicateError(currentArea);
							}

							fPageHeader = (PageHeaderArea)currentArea;
						}
						else if (currentArea is OnlyOnePageFooterArea)
						{
							if (fOnlyOnePageFooter != null)
							{
								AddDuplicateError(currentArea);
							}

							fOnlyOnePageFooter = (OnlyOnePageFooterArea)currentArea;
						}
						else if (currentArea is LastPageFooterArea)
						{
							if (fLastPageFooter != null)
							{
								AddDuplicateError(currentArea);
							}

							fLastPageFooter = (LastPageFooterArea)currentArea;
						}
						else if (currentArea is DocumentFooterArea)
						{
							if (fDocumentFooter != null)
							{
								AddDuplicateError(currentArea);
							}

							fDocumentFooter = (DocumentFooterArea)currentArea;
						}
						else if (currentArea is BackPageArea)
						{
							if (fBackPage != null)
							{
								AddDuplicateError(currentArea);
							}

							fBackPage = (BackPageArea)currentArea;
						}
						else if (currentArea is FirstPageFooterArea)
						{
							if (fFirstPageFooter != null)
							{
								AddDuplicateError(currentArea);
							}

							fFirstPageFooter = (FirstPageFooterArea)currentArea;
							fFirstPageFooter.ShouldDelete = true;
						}
						else if (currentArea is PageFooterArea)
						{
							if (fPageFooter != null)
							{
								AddDuplicateError(currentArea);
							}

							fPageFooter = (PageFooterArea)currentArea;
							fPageFooter.ShouldDelete = true;
						}
					}
				}
				lastArea = currentArea;
			}

			foreach (Section section in Sections)
			{
				if (section.SectionBodyAndGroupByAreas.Count == 0)
				{
					break;
				}

				for (int i = 1; i < section.SectionBodyAndGroupByAreas.Count; i++)
				{
					if (section.SectionBodyAndGroupByAreas[i] is GroupByArea)
					{
						((GroupByArea)section.SectionBodyAndGroupByAreas[i]).SectionBody = section.SectionBody;
					}
				}

				for (int i = 1; i < section.SectionBodyAndGroupByAreas.Count; i++)
				{
					section.SectionBodyAndGroupByAreas[i].FormulaRelatedAreas.Add(GetLastDataAreaForSectionCountingBackwardsFrom(section, i - 1));
				}

				if (section.SectionFooter != null)
				{
					section.SectionFooter.FormulaRelatedAreas.Add(GetLastDataAreaForSectionCountingBackwardsFrom(section, section.SectionBodyAndGroupByAreas.Count - 1));
				}

				if (DocumentFooter != null)
				{
					if (section.SectionFooter != null)
					{
						DocumentFooter.FormulaRelatedAreas.Add(section.SectionFooter);
					}
					else
					{
						DocumentFooter.FormulaRelatedAreas.Add(GetLastDataAreaForSectionCountingBackwardsFrom(section, section.SectionBodyAndGroupByAreas.Count - 1));
					}
				}
			}
		}

		Area GetLastDataAreaForSectionCountingBackwardsFrom(Section section, int indexOfLastAreaToStartWith)
		{
			for (int i = indexOfLastAreaToStartWith; i >= 0; i--)
			{
				Area area = section.SectionBodyAndGroupByAreas[i];
				GroupByArea asGroupByArea = area as GroupByArea;
				if (asGroupByArea == null || asGroupByArea.GroupByPosition != GroupByArea.Position.Top)
				{
					return area;
				}
			}
			AddError(Res.GetString("cb55ead9-8f79-4b57-b2ca-9e713c13732c", "You must have at least one Data Area in each Section."), section.StartingRowOfSection);
			return null;
		}

		void AddOutOfOrderError(Area lastArea, Area currentArea)
		{
			if (GlbStaff.CurrentUser.WantsToSeeDocumentTemplateErrors)
			{
				AddError(Res.GetString("4e85632f-7e74-495e-91a9-c2bd8a7ab108", "{0} area must come before a {1} area.", currentArea.Identifier, lastArea.Identifier), currentArea);
			}
		}

		void AddDuplicateError(Area currentArea)
		{
			AddError(Res.GetString("e0ac1432-00a7-4b97-8614-74dddc6a92d2", "Template can only have one {0} area.", currentArea.Identifier), currentArea);
		}

		void AddError(string errorMessage, Area currentArea)
		{
			AddError(errorMessage, currentArea.StartingRow);
		}

		void AddError(string errorMessage, int rowIndex)
		{
			AddError(errorMessage, report.WorkSheetCurrentlyBeingProcessed.SheetName, rowIndex);
		}

		void AddError(string errorMessage, string workSheetName, int rowIndex)
		{
			HasAnalyserErrors = true;
			report.ErrorManager.Add(new ReportProcessingError(errorMessage, new CellReference(workSheetName, "A" + (rowIndex + 1).ToString()), ReportProcessingErrorSeverity.Error));
		}

		public void Reset()
		{
			ReportSQLSources.Clear();
			Sections.Clear();
			Areas.Clear();
			NonExistantOptionalColumns.Clear();

			fDocumentHeader = null;
			fPageHeader = null;
			fPageFooter = null;
			fDocumentFooter = null;
			fBackPage = null;
			fLastPageFooter = null;
			fOnlyOnePageFooter = null;
			fFirstPageFooter = null;
		}
	}
}
