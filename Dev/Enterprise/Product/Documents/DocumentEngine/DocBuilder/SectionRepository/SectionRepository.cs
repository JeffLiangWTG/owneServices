using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.ExcelTemplates;

namespace Enterprise.DocumentEngine.DocBuilder
{
	class SectionRepository : NonPersistentBusinessObject
	{
		public SectionRepository(ExcelTemplate excelTemplateContainingSuperSet)
		{
			if (excelTemplateContainingSuperSet == null)
			{
				throw new ArgumentNullException(nameof(excelTemplateContainingSuperSet));
			}
			ExcelTemplate = excelTemplateContainingSuperSet;
		}

		internal readonly ExcelTemplate ExcelTemplate;
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Worksheet Identification String used on System Section Repositories.")]
		internal const string DocumentWorkSheetName = "Document";

		internal bool IsTemplateCustomisable
		{
			get { return AllSections.Count > 2; }
		}

		public SectionRepositoryValidation Validation
		{
			get { return new SectionRepositoryValidation(this); }
		}

		public TemplateSectionCollection AllSections
		{
			get { return fAllSections ?? (fAllSections = GetAndLoadAllSectionsCollection()); }
		}
		TemplateSectionCollection fAllSections;
		TemplateSectionCollection GetAndLoadAllSectionsCollection()
		{
			TemplateSectionCollection result = new TemplateSectionCollection(ExcelTemplate);
			RegisterEditableChildObject(result);
			return result;
		}

		#region Exposed Config Section Properties
		#region TemplateName
		public ZString TemplateName
		{
			get { return ConfigReader.TemplateName; } //"NAME="
		}

		public ZPropertyInfo TemplateNameInfo
		{
			get { return GetZPropertyInfo(nameof(TemplateName)); }
		}
		#endregion

		#region Version
		public ZString Version
		{
			get { return ConfigReader.Version; } //"VERSION="
		}

		public ZPropertyInfo VersionInfo
		{
			get { return GetZPropertyInfo(nameof(Version)); }
		}
		#endregion

		#region PageStyle
		public ZString PageStyle
		{
			get { return ConfigReader.PageStyle; } //"PAGESTYLE="
		}

		public ZPropertyInfo PageStyleInfo
		{
			get { return GetZPropertyInfo(nameof(PageStyle)); }
		}
		#endregion

		#region EmailSubject
		public ZString EmailSubject
		{
			get { return ConfigReader.EmailSubject; } //"EMAILSUBJECT="
		}

		public ZPropertyInfo EmailSubjectInfo
		{
			get { return GetZPropertyInfo(nameof(EmailSubject)); }
		}
		#endregion

		#region DataContext
		public ZString DataContext
		{
			get { return ConfigReader.DataContext; } //"DATACONTEXT="
		}

		public ZPropertyInfo DataContextInfo
		{
			get { return GetZPropertyInfo(nameof(DataContext)); }
		}
		#endregion

		#region DataSource
		public ZString DataSource
		{
			get { return ConfigReader.DataSource; } //"DATA:"
		}

		public ZPropertyInfo DataSourceInfo
		{
			get { return GetZPropertyInfo(nameof(DataSource)); }
		}
		#endregion

		ConfigSectionReader ConfigReader
		{
			get { return fConfigReader ?? (fConfigReader = new ConfigSectionReader(ExcelTemplate)); }
		}
		ConfigSectionReader fConfigReader;

		class ConfigSectionReader
		{
			public ConfigSectionReader(ExcelTemplate excelTemplate)
			{
				using (var documentPack = new DocumentPack())
				using (var report = new Report(documentPack, excelTemplate))
				{
					report.Analyser = new ReportAnalyser(report);

					var configSectionEnd = GetConfigSectionEnd(report);
					var configArea = new ConfigArea(0, configSectionEnd, report, "");

					TemplateName = configArea.ReportName;
					Version = new ZDecimal(configArea.ReportVersion / 100).ToString(2);
					PageStyle = configArea.PageStyle.ToString();
					EmailSubject = configArea.EmailSubject;
					DataContext = configArea.DataContextValue.FullDataContext;

					var dataSources = new ZStringBuilder();
					foreach (var dataSource in configArea.DataSourceStrings)
					{
						dataSources.Append(dataSource);
					}

					DataSource = dataSources.ToStringWithNewLineBetweenAppends();
				}
			}

			public readonly ZString TemplateName;
			public readonly ZString Version;
			public readonly ZString PageStyle;
			public readonly ZString EmailSubject;
			public readonly ZString DataContext;
			public readonly ZString DataSource;

			int GetConfigSectionEnd(Report report)
			{
				var workSheet = report.WorkSheetCurrentlyBeingProcessed;
				var rowCount = workSheet.RowCount;

				for (var result = 1; result < rowCount; result++)
				{
					if (workSheet[result, 0].ToString().StartsWith("#"))
					{
						return result - 1;
					}
				}

				return -1;
			}
		}

		#endregion
	}
}
