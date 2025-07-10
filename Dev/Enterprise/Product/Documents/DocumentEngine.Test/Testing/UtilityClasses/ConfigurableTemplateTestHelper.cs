using System.Collections.Generic;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ExcelTemplates;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Testing.UtilityClasses
{
	public static class ConfigurableTemplateTestHelper
	{
		public static void SetWorkSheets(StmTemplateBase template, params KeyValuePair<string, string>[] workSheetInfos)
		{
			var sheetContents = new List<KeyValuePair<string, string>>();

			foreach (var workSheetInfo in workSheetInfos)
			{
				sheetContents.Add(workSheetInfo);
			}

			using (var stream = new MemoryStream())
			{
				DocumentEngineTestHelper.GenerateTemplateStream(stream, sheetContents);
				template.SO_Template = stream.CopyToByteArray();
			}
		}

		public static StmMenuDocumentConfig CreateDocumentConfig(StmTemplateBase template)
		{
			var factory = template.Factory;
			var documentCommand = factory.New<DocumentCommand>();
			var pivot = factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;
			var config = pivot.DocConfigs.AddNew();
			return config;
		}

		#region Get DocBuilder Documents

		/// <summary>
		/// DocBuilder Documents and optionally the developer only customised documents.
		/// </summary>
		/// <param name="factory">BusinessObjectFactory to use.</param>
		/// <returns>DocBuilder documents to be checked in if run by DAT, otherwise also includes developer documents</returns>
		internal static List<StmMenuDocumentConfig> GetAllDocBuilderDocuments(BusinessObjectFactory factory)
		{
			List<StmMenuDocumentConfig> result = new List<StmMenuDocumentConfig>();
			result.AddRange(factory.Load<StmMenuDocumentConfig>(new ZQuery(StmMenuDocumentConfigSchema.S3_IsSystem, ZBool.True)));

			if (!TestingState.IsRunningOnDAT)
			{
				result.AddRange(factory.Load<StmMenuDocumentConfig>(new ZQuery(StmMenuDocumentConfigSchema.S3_IsSystem, ZBool.False)));
			}
			return result;
		}

		#endregion

		#region Get System Configurable Template Array (aka DocBuilder Templates)

		/// <summary>
		/// DocBuilder Templates including foreign languages and optionally the developer only customised template.
		/// </summary>
		/// <param name="factory">BusinessObjectFactory to use.</param>
		/// <returns>"System Document Elements", "System Document Elements [GRM]" plus other supported languages if run by DAT, otherwise it will also include "Customized Document Elements" for developers.</returns>
		internal static StmTemplateBase[] GetAllSystemConfigurableTemplates(BusinessObjectFactory factory)
		{
			return GetSystemConfigurableTemplatesCore(factory, true);
		}

		/// <summary>
		/// Only the main DocBuilder Template and optionally the developer only customised template.
		/// </summary>
		/// <param name="factory">BusinessObjectFactory to use.</param>
		/// <returns>"System Document Elements" English template only if run by DAT, otherwise it will also include "Customized Document Elements" for developers.</returns>
		internal static StmTemplateBase[] GetEnglishSystemConfigurableTemplates(BusinessObjectFactory factory)
		{
			return GetSystemConfigurableTemplatesCore(factory, false);
		}

		static StmTemplateBase[] GetSystemConfigurableTemplatesCore(BusinessObjectFactory factory, bool includeLanguages)
		{
			List<StmTemplateBase> result = new List<StmTemplateBase>();
			var query = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, true);
			if (includeLanguages)
			{
				query.AddToFilter(StmTemplateSchema.SO_Name, SQLComparisonOperator.StartsWith, SectionRepositoryTemplateNames.System);
			}
			else
			{
				query.AddToFilter(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System);
			}
			result.AddRange(factory.Load<StmTemplateBase>(query));

			if (!TestingState.IsRunningOnDAT)
			{
				query = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, false);
				query.AddToFilter(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.User);
				result.AddRange(factory.Load<StmTemplateBase>(query));
			}

			return result.ToArray();
		}

		#endregion

		internal static StmTemplateBase SetupSystemTemplateFromExcelTemplate(BusinessObjectFactory factory, ExcelTemplate excelTemplate)
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(factory, DocBuilderTemplateType.System);
			systemTemplate.SO_Template = excelTemplate.GetAsByteArray();
			return systemTemplate;
		}

		public static StmTemplateBase SetupSystemTemplateFromString(BusinessObjectFactory factory, string contents, DocBuilderTemplateType templateType = DocBuilderTemplateType.System)
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(factory, templateType);
			systemTemplate.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(contents);
			return systemTemplate;
		}

		public const string ConfigurableStripsForTesting =
@"{A}-[#config]
{A}-[Name=TemplateWithGenericSections]
{A}-[#ConfigurableSection:BOD, Section Body 1]
{B}-[#SectionBody]
{B}-[This is Section Body 1.]
{A}-[#ConfigurableSection:BOD, Section Body 2]
{B}-[#SectionBody]
{B}-[This is Section Body 2.]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#ConfigurableSection:GEN, Generic Section 3]
{B}-[This is Generic Section 3.]
{A}-[#ConfigurableSection:GEN, Generic Section 4]
{B}-[This is Generic Section 4.]
{A}-[#ConfigurableSection:GEN, Generic Section 5]
{B}-[This is Generic Section 5.]
{A}-[#ConfigurableSection:GEN, Generic Section 6]
{B}-[This is Generic Section 6.]
{A}-[#ConfigurableSection:PFT, Page Footer 1]
{B}-[#PageFooter]
{B}-[This is Page Footer 1.]
{A}-[#ConfigurableSection:PFT, Page Footer 2]
{B}-[#PageFooter]
{B}-[This is Page Footer 2.]
{A}-[#EndOfReport]";

		internal const string NonConfigurableStripsForTesting =
@"{A}-[#config]
{A}-[Name=TemplateWithGenericSections]
{B}-[#SectionBody]
{B}-[This is Section Body 1.]
{B}-[#SectionBody]
{B}-[This is Section Body 2.]
{B}-[#PageFooter]
{B}-[This is Page Footer 1.]
{B}-[#PageFooter]
{B}-[This is Page Footer 2.]
{A}-[#EndOfReport]";

		public static StmTemplateBase SetupSystemTemplateForTesting(BusinessObjectFactory factory)
		{
			StmTemplateBase result = SetupSystemTemplateFromString(factory, ConfigurableStripsForTesting);
			return result;
		}

		public static StmTemplateBase CreateNonConfigurableTemplate(BusinessObjectFactory factory)
		{
			StmTemplateBase result = DocumentEngineTestHelper.CreateTemplateFromString(factory, "TemplateWithGenericSections", NonConfigurableStripsForTesting);
			return result;
		}

		public static StmMenuDocumentConfigItem AddFromTemplateSection(StmMenuDocumentConfig config, ZString sectionType, ZString sectionName)
		{
			var result = AddFromTemplateSection(config, sectionName);
			result.S4_SectionType = sectionType;
			return result;
		}

		public static StmMenuDocumentConfigItem AddFromTemplateSection(StmMenuDocumentConfig config, ZString sectionName)
		{
			StmMenuDocumentConfigItem result = null;

			if (config != null && config.TemplateSections != null)
			{
				var templateSection = config.TemplateSections.Find(sectionName);
				result = config.ConfigItems.AddFromTemplateSection(templateSection);
			}

			return result;
		}

		public static string GetTemplateSectionContents(StmTemplateBase template, ZString sectionName)
		{
			var result = string.Empty;
			var templateSection = template.TemplateSections.Find(sectionName);

			if (templateSection != null)
			{
				using (var excelInterface = new ExcelInterface())
				{
					using (var stream = template.GetExcelTemplate().GetAsTemplateStream())
					{
						excelInterface.LoadExcelFile(stream);
					}

					var workSheet = excelInterface.WorkSheets[0];

					result = workSheet.ToString(templateSection.StartingRowNumber - 1, templateSection.LastRowNumber - 1);
				}
			}

			return result;
		}

		public static string GetConfigItemsString(IEnumerable<StmMenuDocumentConfigItem> configItems)
		{
			ZStringBuilder builder = new ZStringBuilder();

			foreach (var configItem in configItems)
			{
				builder.Append(string.Format("{0}: {1}, {2} [{3}]", configItem.S4_PrintOrder, configItem.S4_SectionType, configItem.S4_SectionItemName, configItem.S4_FilterList));
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}

		public static string GetConfigItemsString(StmMenuDocumentConfigItemCollection configItems)
		{
			return GetConfigItemsString(configItems.ToArray<StmMenuDocumentConfigItem>());
		}

		public static ExcelWorkSheet GetExcelWorkSheetFromSection(StmTemplateBase template, ZString sectionName)
		{
			var templateSection = template.TemplateSections.Find(sectionName);

			if (templateSection != null)
			{
				using (var excelInterface = new ExcelInterface())
				{
					using (var stream = template.GetExcelTemplate().GetAsTemplateStream())
					{
						excelInterface.LoadExcelFile(stream);
					}

					excelInterface.ActiveWorksheet = 0;
					var sourceWorkSheet = excelInterface.WorkSheets[0];

					excelInterface.Xls.AddSheet();
					excelInterface.Xls.ActiveSheet = excelInterface.Xls.SheetCount;
					var emptyWorkSheet = new ExcelWorkSheet(excelInterface, excelInterface.Xls.SheetName);
					emptyWorkSheet.CopyAndInsertRows(sourceWorkSheet, templateSection.StartingRowNumber - 1, templateSection.RowCount + 1, 0);

					return emptyWorkSheet;
				}
			}

			return null;
		}

		public static float GetRowHeightInPoints(ExcelWorkSheet excelWorkSheet, int row)
		{
			return excelWorkSheet.GetRowHeight(row) / 20;
		}
	}
}
