using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ReportWriter
{
	public class ReportBizObj : AutoReportBizObj, IReportBizObjProvider
	{
		public ReportBizObj(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		[List("PageStyleList")]
		public override ZString PageStyle
		{
			get { return base.PageStyle; }
			set { base.PageStyle = value; }
		}

		public ICodeDescriptionPairList PageStyleList
		{
			get
			{
				return Factory.GetCachedValue<ICodeDescriptionPairList>("ReportWriterPageStyleList", () =>
				{
					var result = new CodeDescriptionPairList();
					foreach (var name in Enum.GetNames(typeof(PageStyles)))
					{
						result.AddPair(name, name);
					}
					return result;
				});
			}
		}

		#region DataSourceName

		public override ZString DataSourceName
		{
			get { return base.DataSourceName; }
			set
			{
				var oldValue = DataSourceName;
				base.DataSourceName = value;
				if (!IsCopying && oldValue != DataSourceName)
				{
					SetMainDataSourceName();
				}
			}
		}

		protected override int DataSourceName_MaxLength
		{
			get { return DataSource.Schema.NameMaxLength; }
		}

		void SetMainDataSourceName()
		{
			if (!IsSettingMainDataSourceSuspended)
			{
				LoadOrCreateMainDataSource();
				if (mainDataSource != null)
				{
					mainDataSource.Name = DataSourceName;
				}
			}
		}

		#endregion

		#region DataSourceSQL

		public override ZString DataSourceSQL
		{
			get
			{
				return base.DataSourceSQL;
			}
			set
			{
				var oldValue = DataSourceSQL;
				base.DataSourceSQL = value;
				if (!IsCopying && oldValue != DataSourceSQL)
				{
					SetMainDataSourceSQLIfNeeded();
				}
			}
		}

		void SetMainDataSourceSQLIfNeeded()
		{
			if (!IsSettingMainDataSourceSuspended)
			{
				LoadOrCreateMainDataSource();
				if (mainDataSource != null)
				{
					mainDataSource.SQL = DataSourceSQL;
				}
			}
		}

		protected override int DataSourceSQL_MaxLength
		{
			get { return DataSource.Schema.SQLMaxLength; }
		}

		#endregion
		public void PopulateFromDataSource()
		{
			LoadOrCreateMainDataSource();
			if (mainDataSource != null)
			{
				var dataSourceName = DataSourceName;
				ColumnHeadings.RemoveAndDeleteAll();
				var sectionBodyArea = Areas.Cast<Area>().FirstOrDefault(x => x.Type == AreaTypeList.Codes.SectionBody && x.DataSource == dataSourceName);
				if (sectionBodyArea == null)
				{
					sectionBodyArea = Areas.AddNew();
					sectionBodyArea.Type = AreaTypeList.Codes.SectionBody;
					sectionBodyArea.DataSource = dataSourceName;
				}
				else
				{
					sectionBodyArea.Rows.RemoveAndDeleteAll();
				}
				var sectionBodyRow = sectionBodyArea.Rows.AddNew();
				sectionBodyRow.Order = 1;
				foreach (var column in mainDataSource.GetFunctionColumns())
				{
					var columnNumber = column.Ordinal + 1;
					var columnName = column.ColumnName;
					var columnHeading = ColumnHeadings.AddNew();
					columnHeading.ColumnNumber = columnNumber;
					columnHeading.DisplayLabel = columnName;
					columnHeading.HeadingText = columnName;
					var sectionBodyRowColumn = sectionBodyRow.Columns.AddNew();
					sectionBodyRowColumn.ColumnNumber = columnNumber;
					sectionBodyRowColumn.Expression = string.Format(Culture.Invariant, "<{0}.{1}>", dataSourceName, columnName);
				}
			}
		}

		void LoadOrCreateMainDataSource()
		{
			if (mainDataSource == null)
			{
				mainDataSource = DataSources.Cast<DataSource>().FirstOrDefault(x => x.IsMain);
				if (mainDataSource == null)
				{
					var dataSouraceName = DataSourceName;
					if (!dataSouraceName.IsEmpty)
					{
						using (SuspendSettingMainDataSource())
						{
							mainDataSource = DataSources.Cast<DataSource>().FirstOrDefault(x => x.Name == dataSouraceName);
							if (mainDataSource == null)
							{
								mainDataSource = DataSources.AddNew();
								mainDataSource.Name = dataSouraceName;
								mainDataSource.SQL = DataSourceSQL;
							}
							mainDataSource.IsMain = true;
						}
					}
				}
			}
		}

		internal DataSource MainDataSource
		{
			get { return mainDataSource; }
			set
			{
				if (mainDataSource != value)
				{
					using (SuspendSettingMainDataSource())
					{
						mainDataSource = value;
						DataSourceName = mainDataSource?.Name ?? ZString.Empty;
						DataSourceSQL = mainDataSource?.SQL ?? ZString.Empty;
					}
				}
			}
		}
		DataSource mainDataSource;

		public IDisposable SuspendSettingMainDataSource()
		{
			return new SettingMainDataSourceSuspender(this);
		}

		public bool IsSettingMainDataSourceSuspended
		{
			get { return settingMainDataSourceIndex > 0; }
		}
		byte settingMainDataSourceIndex;

		class SettingMainDataSourceSuspender : IDisposable
		{
			public SettingMainDataSourceSuspender(ReportBizObj bizObj)
			{
				this.bizObj = bizObj;
				bizObj.settingMainDataSourceIndex++;
			}

			readonly ReportBizObj bizObj;

			void IDisposable.Dispose()
			{
				bizObj.settingMainDataSourceIndex--;
			}
		}

		public void RefreshRelatedDatasIfNeeded()
		{
			if (needRefreshRelatedDatas)
			{
				needRefreshRelatedDatas = false;
				foreach (var area in Areas.Cast<Area>())
				{
					foreach (var row in area.Rows.Cast<RowData>())
					{
						foreach (var column in row.Columns.Cast<ColumnData>())
						{
							column.DisplayLabelInfo.RefreshBinding();
						}
					}
				}
			}
		}

		internal void MarkAsNeedingRefreshRelatedDatas()
		{
			needRefreshRelatedDatas = true;
		}
		bool needRefreshRelatedDatas;

		public ColumnHeadingCollection ColumnHeadings
		{
			get
			{
				if (columnHeadings == null)
				{
					columnHeadings = new ColumnHeadingCollection(this);
					RegisterEditableChildObject(columnHeadings);
				}
				return columnHeadings;
			}
		}
		ColumnHeadingCollection columnHeadings;

		public ICodeDescriptionPairList ColumnHeadingsList
		{
			get
			{
				if (columnHeadingsList == null)
				{
					columnHeadingsList = new CodeDescriptionPairList();
					foreach (var columnHeading in ColumnHeadings.Cast<ColumnHeading>())
					{
						columnHeadingsList.Add(columnHeading);
					}
					ColumnHeadings.CountChanged += ColumnHeadings_CountChanged;
				}
				return columnHeadingsList;
			}
		}
		CodeDescriptionPairList columnHeadingsList;

		void ColumnHeadings_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			if (e.ItemAdded)
			{
				if (!ColumnHeadingsList.Contains(e.BizObject))
				{
					ColumnHeadingsList.Add(e.BizObject);
				}
			}
			else if (e.ItemRemoved)
			{
				ColumnHeadingsList.Remove(e.BizObject);
			}
		}

		public ColumnDataCollection HideColumns
		{
			get
			{
				if (hideColumns == null)
				{
					hideColumns = new ColumnDataCollection(this);
					RegisterEditableChildObject(hideColumns);
				}
				return hideColumns;
			}
		}
		ColumnDataCollection hideColumns;

		public ColumnDataCollection ApplyDocumentCurrencies
		{
			get
			{
				if (applyDocumentCurrencies == null)
				{
					applyDocumentCurrencies = new ColumnDataCollection(this);
					RegisterEditableChildObject(applyDocumentCurrencies);
				}
				return applyDocumentCurrencies;
			}
		}
		ColumnDataCollection applyDocumentCurrencies;

		public DataSourceCollection DataSources
		{
			get
			{
				if (dataSources == null)
				{
					dataSources = new DataSourceCollection(this);
					RegisterEditableChildObject(dataSources);
				}
				return dataSources;
			}
		}
		DataSourceCollection dataSources;

		public DocumentHeaderRowCollection DocumentHeaderRows
		{
			get
			{
				if (documentHeaderRows == null)
				{
					documentHeaderRows = new DocumentHeaderRowCollection(this);
					RegisterEditableChildObject(documentHeaderRows);
				}
				return documentHeaderRows;
			}
		}
		DocumentHeaderRowCollection documentHeaderRows;

		public AreaCollection Areas
		{
			get
			{
				if (areas == null)
				{
					areas = new AreaCollection(this);
					RegisterEditableChildObject(areas);
				}
				return areas;
			}
		}

		AreaCollection areas;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		public void PopulateFrom(ExcelWorkSheet worksheet)
		{
			if (worksheet != null)
			{
				var lastRowOfTemplate = worksheet.GetLastRowOfTemplate();
				if (lastRowOfTemplate > 0 && worksheet[0, 0].ToString().Trim().Equals(Constants.AreaIdentifierTags.Config, StringComparison.OrdinalIgnoreCase))
				{
					WorkSheetName = worksheet.SheetName;
					int rowIndex;
					int previousAreaStart = 1;
					string previousAreaIdentifier = Constants.AreaIdentifierTags.Config;

					for (rowIndex = 1; rowIndex <= lastRowOfTemplate; rowIndex++)
					{
						string areaIdentifier = worksheet[rowIndex, 0].ToString().Trim();
						if (areaIdentifier.StartsWith("#", StringComparison.OrdinalIgnoreCase))
						{
							if (previousAreaStart != 0)
							{
								PopulateArea(previousAreaStart, rowIndex - 1, worksheet, previousAreaIdentifier);
							}
							previousAreaIdentifier = areaIdentifier;
							previousAreaStart = rowIndex;
						}
					}

					if (previousAreaStart != 0)
					{
						PopulateArea(previousAreaStart, rowIndex - 1, worksheet, previousAreaIdentifier);
					}
				}
			}
		}

		void PopulateArea(int startRow, int endRow, ExcelWorkSheet worksheet, string areaIdentifier)
		{
			if (areaIdentifier.Equals(Constants.AreaIdentifierTags.Config))
			{
				PopulateConfigAreaDetail(startRow, endRow, worksheet);
			}
			else if (areaIdentifier.Equals(Constants.AreaIdentifierTags.DocumentHeader, StringComparison.OrdinalIgnoreCase))
			{
				PopulateDocumentHeaderDetail(startRow, endRow, worksheet);
			}
			else if (areaIdentifier.StartsWith(Constants.AreaIdentifierTags.SectionBody, StringComparison.OrdinalIgnoreCase))
			{
				PopulateSectionBodyDetail(startRow, endRow, worksheet);
			}
		}

		void PopulateSectionBodyDetail(int startRow, int endRow, ExcelWorkSheet worksheet)
		{
			var sectionBodyArea = Areas.AddNew();
			sectionBodyArea.Type = AreaTypeList.Codes.SectionBody;
			foreach (var parameter in AreaHelper.SplitParametersForSectionBody(worksheet[startRow, 0].ToString().Trim()).Select(x => x.Trim()))
			{
				if (parameter.StartsWith("DATA=", StringComparison.OrdinalIgnoreCase))
				{
					sectionBodyArea.DataSource = parameter.Substring(5);
					MarkDataSourceAsMain(sectionBodyArea.DataSource);
				}
				else if (parameter.StartsWith(Constants.SectionBodyAreaParameters.MaximumNumberOfRowsToShow, StringComparison.OrdinalIgnoreCase))
				{
					sectionBodyArea.MaximumNumberOfRowsToShow = ParseNumberOfRows(parameter);
				}
				else if (parameter.StartsWith(Constants.SectionBodyAreaParameters.RowCountAMultipleof, StringComparison.OrdinalIgnoreCase))
				{
					sectionBodyArea.RowCountAMultipleOf = ParseNumberOfRows(parameter);
				}
				else if (parameter.StartsWith(Constants.SectionBodyAreaParameters.NumberOfRowsToShow, StringComparison.OrdinalIgnoreCase))
				{
					sectionBodyArea.NumberOfRowsToShow = ParseNumberOfRows(parameter);
				}
				else if (parameter.StartsWith(Constants.SectionBodyAreaParameters.StartingRowToShow, StringComparison.OrdinalIgnoreCase))
				{
					sectionBodyArea.StartingRowToShow = ParseNumberOfRows(parameter);
				}
				else if (parameter.Equals(Constants.CommonAreaParameters.Sticky, StringComparison.OrdinalIgnoreCase))
				{
					sectionBodyArea.Sticky = true;
				}
			}
			for (int rowNumber = startRow + 1; rowNumber <= endRow; rowNumber++)
			{
				var cellValue = worksheet[rowNumber, 0].ToString().Trim();
				PopulateSectionBodyDetail(sectionBodyArea, worksheet, rowNumber, cellValue);
			}
		}

		void MarkDataSourceAsMain(ZString dataSourceName)
		{
			if (!hasSetDataSourceMain && !dataSourceName.IsEmpty)
			{
				var dataSource = DataSources.Cast<DataSource>().FirstOrDefault(x => x.Name == dataSourceName);
				if (dataSource != null)
				{
					hasSetDataSourceMain = true;
					dataSource.IsMain = true;
				}
			}
		}
		bool hasSetDataSourceMain;

		void PopulateSectionBodyDetail(Area sectionBodyArea, ExcelWorkSheet worksheet, int rowNumber, string cellData)
		{
			var row = sectionBodyArea.Rows.AddNew();
			row.Order = DocumentHeaderRows.Count;
			row.Expression = cellData;
			row.Height = worksheet.GetRowHeight(rowNumber);
			var columnCount = worksheet.ColumnCount;
			for (int i = 1; i < columnCount; i++)
			{
				var cellValue = worksheet[rowNumber, i].ToString();
				if (!string.IsNullOrEmpty(cellValue))
				{
					var column = row.Columns.AddNew();
					column.ColumnNumber = i;
					column.Expression = cellValue;
				}
			}
		}

		int ParseNumberOfRows(string parameter)
		{
			int numberOfRows;
			return int.TryParse(parameter.Substring(parameter.IndexOf("=", StringComparison.OrdinalIgnoreCase) + 1), out numberOfRows) ? numberOfRows : 0;
		}

		void PopulateDocumentHeaderDetail(int startRow, int endRow, ExcelWorkSheet worksheet)
		{
			for (int rowNumber = startRow + 1; rowNumber <= endRow; rowNumber++)
			{
				var cellValue = worksheet[rowNumber, 0].ToString().Trim();
				PopulateDocumentHeaderDetail(worksheet, rowNumber, cellValue);
			}
		}

		void PopulateDocumentHeaderDetail(ExcelWorkSheet worksheet, int rowNumber, string cellData)
		{
			var documentHeaderRow = DocumentHeaderRows.AddNew();
			documentHeaderRow.Order = DocumentHeaderRows.Count;
			documentHeaderRow.Expression = cellData;
			documentHeaderRow.Height = worksheet.GetRowHeight(rowNumber);

			var columnCount = worksheet.ColumnCount;
			for (int i = 1; i < columnCount; i++)
			{
				var cellValue = worksheet[rowNumber, i].ToString();
				if (!string.IsNullOrEmpty(cellValue))
				{
					if (RegexProvider.CustomisedColumnRegex.IsMatch(cellValue))
					{
						documentHeaderRow.Columns.RemoveAndDeleteAll();
						documentHeaderRow.IsCustomisedColumnRow = true;
						var cellFormat = worksheet.GetCell(rowNumber, i).Format;
						documentHeaderRow.BackgroundColorInArgb = cellFormat.BackgroundColor.ToArgb();
						// TODO: Store additional cell information
						break;
					}
					var column = documentHeaderRow.Columns.AddNew();
					column.ColumnNumber = i;
					column.Expression = cellValue;
				}
			}
		}

		void PopulateConfigAreaDetail(int startRow, int endRow, ExcelWorkSheet worksheet)
		{
			for (int rowNumber = startRow; rowNumber <= endRow; rowNumber++)
			{
				var cellValue = worksheet[rowNumber, 0].ToString().Trim();
				PopulateConfigAreaDetail(worksheet, rowNumber, cellValue);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Hard-coded constant")]
		void PopulateConfigAreaDetail(ExcelWorkSheet worksheet, int rowNumber, string cellValue)
		{
			if (cellValue.StartsWith(Constants.ConfigAreaParameters.TemplateNameSignature, StringComparison.OrdinalIgnoreCase))
			{
				Name = cellValue.Substring(Constants.ConfigAreaParameters.TemplateNameSignature.Length);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.VersionSignature, StringComparison.OrdinalIgnoreCase))
			{
				Version = ZDecimal.ParseSafe(cellValue.Substring(Constants.ConfigAreaParameters.VersionSignature.Length), ZDecimal.Zero).Round(2);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.AutoHeightModeSignature, StringComparison.OrdinalIgnoreCase))
			{
				IsExpandRow = (cellValue.Substring(Constants.ConfigAreaParameters.AutoHeightModeSignature.Length).ToLower(Culture.Invariant).Trim() == "expandrow");
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.EmailSubjectSignature, StringComparison.OrdinalIgnoreCase))
			{
				EmailSubject = cellValue.Substring(Constants.ConfigAreaParameters.EmailSubjectSignature.Length);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.DataSourceSignature, StringComparison.OrdinalIgnoreCase))
			{
				var dataSource = cellValue.ParseDataSource();
				if (dataSource != null)
				{
					DataSources.AddOrUpdate(dataSource.TableName, dataSource.SelectStatement);
				}
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.DataContextSignature, StringComparison.OrdinalIgnoreCase))
			{
				DataContext = cellValue.Substring(Constants.ConfigAreaParameters.DataContextSignature.Length);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.HideColumnIfSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessHideColumns(worksheet, rowNumber);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.ColumnHeadingsSignature, StringComparison.OrdinalIgnoreCase))
			{
				ProcessColumnHeadings(worksheet, cellValue, rowNumber);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.PageStyleSignature, StringComparison.OrdinalIgnoreCase))
			{
				PageStyles pageStyles;
				if (Enum.TryParse(cellValue.Substring(Constants.ConfigAreaParameters.PageStyleSignature.Length), true, out pageStyles))
				{
					PageStyle = pageStyles.ToString();
				}
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.TrailingFormFeedSignature, StringComparison.OrdinalIgnoreCase))
			{
				TrailingFormFeed = ZInt.ParseSafe(cellValue.Substring(Constants.ConfigAreaParameters.TrailingFormFeedSignature.Length), ZInt.Zero);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.SlowServers, StringComparison.OrdinalIgnoreCase))
			{
				SlowServers = cellValue.Substring(Constants.ConfigAreaParameters.SlowServers.Length);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.DocumentCurrency, StringComparison.OrdinalIgnoreCase))
			{
				ProcessDocumentCurrency(worksheet, cellValue, rowNumber);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.SheetNameOverride, StringComparison.OrdinalIgnoreCase))
			{
				SheetNameOverride = cellValue.Substring(Constants.ConfigAreaParameters.SheetNameOverride.Length);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.HideSheetIfSignature, StringComparison.OrdinalIgnoreCase))
			{
				HideSheetIf = cellValue.Substring(Constants.ConfigAreaParameters.HideSheetIfSignature.Length);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.SqlTimeoutSignature, StringComparison.OrdinalIgnoreCase))
			{
				SqlTimeout = ZInt.ParseSafe(cellValue.Substring(Constants.ConfigAreaParameters.SqlTimeoutSignature.Length), ZInt.Zero);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.SqlQueryHints, StringComparison.OrdinalIgnoreCase))
			{
				SqlQueryHints = cellValue.Substring(Constants.ConfigAreaParameters.SqlQueryHints.Length);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.Title, StringComparison.OrdinalIgnoreCase))
			{
				Title = cellValue.Substring(Constants.ConfigAreaParameters.Title.Length);
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.ForceWebPublishSignature, StringComparison.OrdinalIgnoreCase))
			{
				ForceWebPublish = true;
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.SuppressDraftWatermark, StringComparison.OrdinalIgnoreCase))
			{
				SuppressDraftWatermark = true;
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.DisableXLSXExport, StringComparison.OrdinalIgnoreCase))
			{
				DisableXLSXExport = true;
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.DisableCSVExport, StringComparison.OrdinalIgnoreCase))
			{
				DisableCSVExport = true;
			}
			else if (cellValue.StartsWith(Constants.ConfigAreaParameters.SqlQueryHintsIgnoreRecompile, StringComparison.OrdinalIgnoreCase))
			{
				SqlQueryHintsIgnoreRecompile = true;
			}
		}

		void ProcessDocumentCurrency(ExcelWorkSheet worksheet, string cellData, int rowNumber)
		{
			DocumentCurrency = cellData.Substring(Constants.ConfigAreaParameters.DocumentCurrency.Length);
			for (int col = 1; col < worksheet.ColumnCount; col++)
			{
				var cellValue = worksheet[rowNumber, col].ToString();
				if (cellValue.Equals(Constants.ConfigArea.ApplyDocumentCurrency, StringComparison.OrdinalIgnoreCase))
				{
					var column = ApplyDocumentCurrencies.AddNew();
					column.ColumnNumber = col;
				}
			}
		}

		void ProcessHideColumns(ExcelWorkSheet worksheet, int rowNumber)
		{
			for (int col = 1; col < worksheet.ColumnCount; col++)
			{
				var cellValue = worksheet[rowNumber, col].ToString();
				if (!string.IsNullOrEmpty(cellValue))
				{
					var hideColumn = HideColumns.AddNew();
					hideColumn.ColumnNumber = col;
					hideColumn.Expression = cellValue;
				}
			}
		}

		void ProcessColumnHeadings(ExcelWorkSheet worksheet, string columnHeadingCellContent, int rowNumber)
		{
			var columnCount = worksheet.ColumnCount;
			var savesToKeywordPosition = columnHeadingCellContent.IndexOf(Constants.ConfigArea.ColumnHeading.SavesTo, StringComparison.OrdinalIgnoreCase);
			if (savesToKeywordPosition > -1)
			{
				ColumnHeadingSavesTo = columnHeadingCellContent.Substring(savesToKeywordPosition + Constants.ConfigArea.ColumnHeading.SavesTo.Length);
			}

			for (int i = 1; i < columnCount; i++)
			{
				var cellValue = worksheet[rowNumber, i].ToString();
				if (!string.IsNullOrEmpty(cellValue))
				{
					var columnHeading = ColumnHeadings.AddNew();
					columnHeading.ColumnNumber = i;
					columnHeading.PopulateFromExcel(cellValue, worksheet.GetColWidth(i));
				}
			}
		}

		ReportBizObj IReportBizObjProvider.GetReportBizObj()
		{
			return this;
		}
	}
}
