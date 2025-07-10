using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ExcelTemplates;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class ReportTemplateTranslationHelper : DocumentTemplateTranslationHelper
	{
		public override List<string> GetAvaliableWorkSheetNames(ExcelInterface excelInterface)
		{
			return excelInterface.WorkSheets.Select(sheet => sheet.SheetName).ToList();
		}

		public DataWithDocumentMacroUsage<ResourceStringData>[] GetUniqueLabels(BusinessObjectFactory factory, Dictionary<string, byte[]> templatesWithPath)
		{
			var results = new Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>>();
			foreach (var templatePath in templatesWithPath.Keys)
			{
				MergeDictionary(results, GetUniqueLabelsInDictionary(factory, templatePath, templatesWithPath[templatePath]));
			}

			return results.Values.ToArray();
		}

		public readonly ConcurrentBag<string> SheetNames = new ConcurrentBag<string>();

		public readonly ConcurrentBag<string> ReportTitles = new ConcurrentBag<string>();

		public override string KeyPrefix
		{
			get
			{
				return DocBuilderResourceStrings.ReportLabelKeyPrefix;
			}
		}

		protected override string SubRootDirectory
		{
			get
			{
				return @"\Reports\";
			}
		}

		public override string AnalyseGroupName(ExcelWorkSheet workSheet, ExcelCell cell, int row)
		{
			return workSheet.SheetName;
		}

		protected override string AnalyzeResourceStringKey(string templateFileName, string groupName, string keyPrefix, string translatableText)
		{
			return DocBuilderResourceStrings.GetKey(templateFileName, keyPrefix, translatableText);
		}

		public override DataWithDocumentMacroUsage<ResourceStringData> GenerateDataWithDocumentMacroUsage(ResourceStringData data)
		{
			return new DataWithReportFieldUsage<ResourceStringData>(data);
		}

		public override DocumentMacroUsage GenerateDocumentMacroUsage(string macro, string fileName, string templateName)
		{
			return new ReportFieldUsage(macro, fileName + "+" + templateName);
		}

		public override int GetSheetContentRowStart(ExcelWorkSheet workSheet, int rowEnd)
		{
			var hasConfigArea = IsContentSheet(workSheet);
			var result = rowEnd;
			if (hasConfigArea)
			{
				var sheetName = workSheet.SheetName;

				for (var row = 0; row <= rowEnd; row++)
				{
					var areaId = workSheet[row, 0].ToString();
					if (areaId.StartsWith("#", StringComparison.OrdinalIgnoreCase) && !areaId.StartsWith(Constants.AreaIdentifierTags.Config, StringComparison.OrdinalIgnoreCase))
					{
						result = row;
						break;
					}

					if (areaId.StartsWith(Constants.ConfigAreaParameters.Title, StringComparison.OrdinalIgnoreCase))
					{
						var reportTitle = areaId.Substring(Constants.ConfigAreaParameters.Title.Length);
						ReportTitles.Add(reportTitle);
					}

					if (areaId.StartsWith(Constants.ConfigAreaParameters.SheetNameOverride, StringComparison.OrdinalIgnoreCase))
					{
						sheetName = areaId.Substring(Constants.ConfigAreaParameters.SheetNameOverride.Length);
					}
				}

				SheetNames.Add(sheetName);
			}

			return result;
		}

		bool IsContentSheet(ExcelWorkSheet workSheet)
		{
			return workSheet[0, 0].ToString().StartsWith(Constants.AreaIdentifierTags.Config, StringComparison.OrdinalIgnoreCase);
		}

		public override Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>> GetUniquelabelsForNonContentSheet(BusinessObjectFactory factory, string templateFilePath, byte[] template, out bool needTranslate)
		{
			Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>> results = null;

			var excelTemplate = new ExcelTemplateReadFromByteArray("TemplateFromStream", "", template);
			using (var pack = new DocumentPack())
			using (var report = new Report(pack, excelTemplate, Guid.Empty, Enterprise.Core.Constants.DataContext.None))
			{
				report.IsInTaskBuild = true;
				report.Analyser = report.GetNewReportAnalyser();
				report.Analyser.AnalyseReportInBuildTask();

				needTranslate = !report.Analyser.Config.DisableTranslate;
				if (needTranslate)
				{
					results = new Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>>();
					var columnSetting = report.ColumnHeadingManager.DefaultTemplateConfigurationManager.GetCopyOfHeadings();
					foreach (Worksheet worksheet in columnSetting.Worksheets)
					{
						foreach (ColumnHeading columnHeading in worksheet.ColumnHeadings)
						{
							AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, worksheet.Name, columnHeading.Description);
							AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, worksheet.Name, columnHeading.HeadingText);
						}
					}

					foreach (var field in report.FilterCollection)
					{
						var filterField = field as FilterField;
						if (filterField != null)
						{
							AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.FilterSheet.SheetName, filterField.DisplayName);
							AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.FilterSheet.SheetName, filterField.GroupName);
							AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.FilterSheet.SheetName, filterField.GroupDescription);
						}

						var descriptionListField = field as IDescriptionPairListSupportField;
						if (descriptionListField != null)
						{
							foreach (var description in descriptionListField.DescriptionList)
							{
								AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.FilterSheet.SheetName, description);
							}
						}

						if (filterField is MultipleSelectionLookup multipleSelectionLookup)
						{
							foreach (var column in multipleSelectionLookup.Columns.Where(column => !string.IsNullOrEmpty(column.ColumnName)))
							{
								AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.FilterSheet.SheetName, column.Caption);
							}
						}
					}

					foreach (RuntimeOptions.SortOrder sortField in report.SortOrderCollection)
					{
						AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.SortSheet.SheetName, sortField.DisplayName);
					}

					foreach (GroupBy groupbyField in report.GroupByCollection)
					{
						AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.GroupBySheet.SheetName, groupbyField.DisplayName);
					}

					foreach (var constantField in report.SheetDefinedConstants)
					{
						AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.ConstantsSheet.SheetName, constantField.Value.ToString());
					}

					foreach (FilterField userField in report.UserDefinedFieldList)
					{
						AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.UDFSheet.SheetName, userField.DisplayName);
					}

					foreach (OptionalTemplateSheet sheet in report.OptionalTemplateSheetCollection)
					{
						AddReportFieldsResourceStringIfNotEmptyAndExists(results, templateFilePath, report.OptionalTemplatesSheet.SheetName, sheet.Name);
					}
				}
			}

			return results;
		}

		void AddReportFieldsResourceStringIfNotEmptyAndExists(Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>> target, string templateFilePath, string sourceSheetName, string sourceFieldName)
		{
			if (string.IsNullOrEmpty(sourceFieldName))
			{
				return;
			}
			var templateFileName = GetFileNameByPath(templateFilePath);
			var key = AnalyzeResourceStringKey(templateFileName, sourceSheetName, KeyPrefix, sourceFieldName);
			if (!target.ContainsKey(key))
			{
				target.Add(key, GenerateDataWithDocumentMacroUsage(GenerateResourceStringData(key, sourceFieldName, templateFileName, templateFilePath)));
			}
			target[key].DocBuilderUsages.Add(new ReportFieldUsage(sourceFieldName, templateFileName + "." + sourceSheetName));
		}
		public override IEnumerable<Report> ForEachReport(BusinessObjectFactory factory)
		{
			yield break;
		}
	}
}
