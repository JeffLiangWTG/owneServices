using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class DocBuilderTemplateTranslationHelper : DocumentTemplateTranslationHelper
	{
		#region Override

		public override List<string> GetAvaliableWorkSheetNames(ExcelInterface excelInterface)
		{
			return new List<string>() { SectionRepository.DocumentWorkSheetName };
		}

		public override string AnalyseGroupName(ExcelWorkSheet workSheet, ExcelCell cell, int row)
		{
			TemplateSection templateSection = null;
			if (!cell.IsEmpty && cell.ValueSourceText.StartsWith(Constants.AreaIdentifierTags.ConfigurableSection, StringComparison.OrdinalIgnoreCase))
			{
				templateSection = new TemplateSection(cell.ValueSourceText.Substring(21), row, -1);
				return templateSection == null ? "" : templateSection.SectionName.ToString();
			}
			return GroupName;
		}

		protected override string AnalyzeResourceStringKey(string templateFileName, string groupName, string keyPrefix, string translatableText)
		{
			if (translatableText.ContainsLetters())
			{
				groupName = string.Empty;
			}
			return DocBuilderResourceStrings.GetKey(groupName, keyPrefix, translatableText);
		}

		protected override ResourceStringData GenerateResourceStringData(string key, string captionOrFullDescription, string contextClassName, string contextFile)
		{
			return new ResourceStringData(key, captionOrFullDescription);
		}

		public override string KeyPrefix
		{
			get
			{
				return DocBuilderResourceStrings.DocLabelKeyPrefix;
			}
		}

		protected override string SubRootDirectory
		{
			get
			{
				return @"\Documents\";
			}
		}

		public override int GetSheetContentRowStart(ExcelWorkSheet workSheet, int rowEnd)
		{
			var result = 0;

			for (var row = 0; row <= rowEnd; row++)
			{
				var areaId = workSheet[row, 0].ToString();
				if (areaId.StartsWith(Constants.AreaIdentifierTags.ConfigurableSection, StringComparison.OrdinalIgnoreCase))
				{
					result = row;
					break;
				}
			}

			return result;
		}

		public override DataWithDocumentMacroUsage<ResourceStringData> GenerateDataWithDocumentMacroUsage(ResourceStringData data)
		{
			return new DataWithDocBuilderUsage<ResourceStringData>(data);
		}

		public override DocumentMacroUsage GenerateDocumentMacroUsage(string macro, string fileName, string templateName)
		{
			return new DocBuilderUsage(macro, templateName);
		}

		#endregion

		#region Labels

		public DataWithDocumentMacroUsage<ResourceStringData>[] GetUniqueLabels(BusinessObjectFactory factory)
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(factory, DocBuilderTemplateType.System);
			return GetUniqueLabels(factory, systemTemplate.SO_ExcelTemplatePath, systemTemplate.SO_Template);
		}

		#endregion

		public override IEnumerable<Report> ForEachReport(BusinessObjectFactory factory)
		{
			var declaration = factory.New(ObjectFactory.GetType(typeof(Enterprise.Integration.Customs.IBaseJobDeclaration)));
			var genericFreightJobWrapper = DocumentWrapperFactory.GenerateGenericWrappers(Enterprise.Core.Constants.DataContext.GenericFreightJob, declaration)[0];

			var systemDocumentElementsTemplates = GetEnglishSystemConfigurableTemplates(factory);
			foreach (StmTemplateBase template in systemDocumentElementsTemplates)
			{
				yield return CreateReport(genericFreightJobWrapper, template);
			}
		}

		public static ResourceStringData[] GetReportNames(string keyPrefix, string[] reportNames)
		{
			var results = new Dictionary<string, ResourceStringData>(StringComparer.OrdinalIgnoreCase);

			foreach (var reportName in reportNames)
			{
				var cellAnalyzer = new ExcelCellAnalyzer(reportName);
				var translatable = cellAnalyzer.GetTextToBeTranslated();
				if (translatable.Length > 0)
				{
					foreach (var item in translatable)
					{
						string key = keyPrefix + item.TranslatableText;
						if (!results.ContainsKey(key))
						{
							results.Add(key, new ResourceStringData(key, ReportNameShort.GetShortReportName(item.TranslatableText), "", item.TranslatableText, ""));
						}
					}
				}
			}

			return results.Values.ToArray();
		}

		Report CreateReport(DocumentWrapper genericFreightJobWrapper, StmTemplateBase systemDocumentElementTemplate)
		{
			ExcelTemplateReadFromStmTemplateTable excelTemplate = new ExcelTemplateReadFromStmTemplateTable(systemDocumentElementTemplate);
			return new Report(new DocumentPack(new BusinessObjectFactory().New<StmMenuItem>()), excelTemplate, genericFreightJobWrapper, (NoResString)"Test ReportName", ContactType.Consignor, null, DocumentDirection.ANY, false); // Development constants will only be English
		}

		StmTemplateBase[] GetEnglishSystemConfigurableTemplates(BusinessObjectFactory factory)
		{
			return GetSystemConfigurableTemplatesCore(factory, false);
		}

		StmTemplateBase[] GetSystemConfigurableTemplatesCore(BusinessObjectFactory factory, bool includeLanguages)
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

			return result.ToArray();
		}
	}
}
