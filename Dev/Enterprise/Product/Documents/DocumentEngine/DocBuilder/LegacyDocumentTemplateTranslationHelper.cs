using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.FlexCelInterface;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class LegacyDocumentTemplateTranslationHelper : DocumentTemplateTranslationHelper
	{
		public readonly ConcurrentBag<string> SheetNames = new ConcurrentBag<string>();

		public override string KeyPrefix => DocBuilderResourceStrings.LegacyDocLabelKeyPrefix;

		protected override string SubRootDirectory => @"\Documents\";

		public override List<string> GetAvaliableWorkSheetNames(ExcelInterface excelInterface)
		{
			return excelInterface.WorkSheets.Select(sheet => sheet.SheetName).ToList();
		}

		public DataWithDocumentMacroUsage<ResourceStringData>[] GetUniqueLabels(BusinessObjectFactory factory, Dictionary<string, byte[]> templatesWithPath)
		{
			var results = new Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>>();
			foreach (var templatePath in templatesWithPath.Keys)
			{
				MergeDictionary<string, DataWithDocumentMacroUsage<ResourceStringData>>(results, GetUniqueLabelsInDictionary(factory, templatePath, templatesWithPath[templatePath]));
			}

			return results.Values.ToArray();
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
			var result = rowEnd;

			var sheetName = workSheet.SheetName;

			for (var row = 0; row <= rowEnd; row++)
			{
				var areaId = workSheet[row, 0].ToString();
				if (areaId.StartsWith("#", StringComparison.OrdinalIgnoreCase) && !areaId.StartsWith(Constants.AreaIdentifierTags.Config, StringComparison.OrdinalIgnoreCase))
				{
					result = row;
					break;
				}

				if (areaId.StartsWith(Constants.ConfigAreaParameters.SheetNameOverride, StringComparison.OrdinalIgnoreCase))
				{
					sheetName = areaId.Substring(Constants.ConfigAreaParameters.SheetNameOverride.Length);
				}
			}

			SheetNames.Add(sheetName);

			return result;
		}

		public override Dictionary<string, DataWithDocumentMacroUsage<ResourceStringData>> GetUniquelabelsForNonContentSheet(BusinessObjectFactory factory, string templateFilePath, byte[] template, out bool needTranslate)
		{
			needTranslate = NeedTranslate(templateFilePath, template);
			return null;
		}

		readonly Dictionary<string, bool> templatesNeedTranslate = new Dictionary<string, bool>();

		public virtual bool NeedTranslate(string excelTemplateName, byte[] template)
		{
			if (templatesNeedTranslate.ContainsKey(excelTemplateName))
			{
				return templatesNeedTranslate[excelTemplateName];
			}

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template);
				var avaliableWorkSheetNames = GetAvaliableWorkSheetNames(excelInterface);
				foreach (var workSheetName in avaliableWorkSheetNames)
				{
					var workSheet = excelInterface.WorkSheets.Find(workSheetName);
					var rowEnd = workSheet.RowCount - 1;
					for (var row = 0; row <= rowEnd; row++)
					{
						var areaId = workSheet[row, 0].ToString();

						if (areaId.StartsWith("#", StringComparison.OrdinalIgnoreCase) && !areaId.StartsWith(Constants.AreaIdentifierTags.Config, StringComparison.OrdinalIgnoreCase))
						{
							break;
						}

						if (areaId.StartsWith(Constants.ConfigAreaParameters.TranslateLegacyDocument, StringComparison.OrdinalIgnoreCase))
						{
							templatesNeedTranslate.Add(excelTemplateName, true);
							return true;
						}
					}
				}
			}

			templatesNeedTranslate.Add(excelTemplateName, false);
			return false;
		}

		public override IEnumerable<Report> ForEachReport(BusinessObjectFactory factory)
		{
			yield break;
		}
	}
}
