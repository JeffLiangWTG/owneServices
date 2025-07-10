using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.ExcelTemplates;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class TemplateSectionCollection : NonPersistentBusinessObjectCollection<TemplateSection>
	{
		public TemplateSectionCollection(ExcelTemplate template)
			: this(template, false)
		{
		}

		public TemplateSectionCollection(ExcelTemplate template, bool configurableSectionsOnly)
		{
			if (template != null)
			{
				this.templateName = template.TemplateName;
				this.language = SectionRepositoryTemplateNames.GetLanguageCodeFromTemplateName(templateName);
				try
				{
					using (var templateStream = template.GetAsTemplateStream())
					using (var excelInterface = ExcelInterface.GetLoadedExcelInterface(templateStream))
					{
						if (excelInterface.WorkSheets.Count > 0)
						{
							if (configurableSectionsOnly)
							{
								LoadConfigurableSectionsOnly(excelInterface.WorkSheets[0]);
							}
							else
							{
								Load(excelInterface.WorkSheets[0]);
							}
						}
					}
				}
				catch (ExcelInterfaceException ex)
				{
					if (templateName != Core.Constants.SectionRepositoryTemplateNames.System && (ex.Type == ExcelInterfaceExceptionType.CouldNotOpenFile || ex.Type == ExcelInterfaceExceptionType.CouldNotOpenStream))
					{
						throw new ExcelInterfaceException(ExcelInterfaceExceptionType.CouldNotOpenCustomizedDocumentElements, Res.GetString("14d4300f-e062-40fe-b55b-dfffca0c6f9f", "Your {0} template seems to be corrupted. Please delete it or reload it from a backup.", templateName));
					}
					throw;
				}
			}
		}

		readonly ZString templateName;
		readonly string language;

		void Load(ExcelWorkSheet workSheet)
		{
			string lastSectionParameter = null;
			int lastSectionStart = 1;
			int lastSectionRowSize = -1;
			for (int rowIndex = 0; rowIndex < workSheet.RowCount; rowIndex++)
			{
				lastSectionRowSize++;
				var cellContent = workSheet[rowIndex, 0].ToString().Trim();
				if (cellContent.StartsWith(Constants.AreaIdentifierTags.ConfigurableSection, StringComparison.OrdinalIgnoreCase))
				{
					var parameters = cellContent.Split(new char[] { ':' }, 2);
					if (parameters.Length >= 2)
					{
						Add(new TemplateSection(lastSectionParameter ?? ConfigurableSectionTypeList.Codes.ConfigSection, lastSectionStart, rowIndex - lastSectionStart, templateName, language: language));
						lastSectionRowSize = -1;
						lastSectionParameter = parameters[1];
						lastSectionStart = rowIndex + 1;
					}
				}
				else if (cellContent.Equals(Constants.AreaIdentifierTags.EndOfReport, StringComparison.OrdinalIgnoreCase))
				{
					Add(new TemplateSection(lastSectionParameter ?? ConfigurableSectionTypeList.Codes.ConfigSection, lastSectionStart, rowIndex - lastSectionStart, language: language));
					lastSectionRowSize = -1;
					lastSectionParameter = ConfigurableSectionTypeList.Codes.EndOfReport;
					lastSectionStart = rowIndex + 1;
					break;
				}
			}
			if (lastSectionParameter == null)
			{
				int rowOfFirstSection = this.Count > 0 ? (int)this[0].StartingRowNumber : lastSectionRowSize;
				Add(new TemplateSection(ConfigurableSectionTypeList.Codes.ConfigSection, lastSectionStart, rowOfFirstSection, language: language));
			}
			else
			{
				Add(new TemplateSection(lastSectionParameter, lastSectionStart, lastSectionRowSize + 1, language: language));
			}
		}

		void LoadConfigurableSectionsOnly(ExcelWorkSheet workSheet)
		{
			for (int rowIndex = 0; rowIndex < workSheet.RowCount; rowIndex++)
			{
				var cellContent = workSheet[rowIndex, 0].ToString().Trim();
				if (cellContent.StartsWith(Constants.AreaIdentifierTags.ConfigurableSection, StringComparison.OrdinalIgnoreCase))
				{
					var parameters = cellContent.Split(new char[] { ':' }, 2);
					if (parameters.Length >= 2)
					{
						Add(new TemplateSection(parameters[1], 0, 0, templateName, cellContent, language));
					}
				}
			}
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException("AddNew(Type BizOType) is not allowed as you need a TypeCode and a SectionName to instantiate a new TemplateSection.");
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("AddNew() is not allowed as you need a TypeCode and a SectionName to instantiate a new TemplateSection.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		internal void Move(TemplateSection templateSectionToMove, int destinationRow)
		{
			int sourceRow = templateSectionToMove.StartingRowNumber;
			int adjustment = (destinationRow < sourceRow ? 1 : -1) * templateSectionToMove.RowCount;

			foreach (TemplateSection templateSection in this)
			{
				if (templateSection == templateSectionToMove)
				{
					templateSection.MoveStartingRowTo(destinationRow);
				}
				else if (RowNumberIsBetween(templateSection.StartingRowNumber, sourceRow, destinationRow))
				{
					templateSection.MoveStartingRowTo(templateSection.StartingRowNumber + adjustment);
				}
			}
		}

		bool RowNumberIsBetween(int rowNumber, int boundary1, int boundary2)
		{
			return (rowNumber >= boundary1 && rowNumber <= boundary2)
				|| (rowNumber >= boundary2 && rowNumber <= boundary1);
		}

		internal bool Contains(string sectionName)
		{
			return Find(sectionName) != null;
		}

		public TemplateSection Find(string sectionID)
		{
			TemplateSection result = null;
			foreach (TemplateSection templateSection in this)
			{
				if (templateSection.SectionName.EqualsIgnoringCase(sectionID))
				{
					result = templateSection;
					break;
				}
			}

			return result;
		}

		internal TemplateSection GetConfigSection()
		{
			for (int index = 0; index < Count; index++)
			{
				TemplateSection templateSection = this[index];
				if (templateSection.TypeCode == ConfigurableSectionTypeList.Codes.ConfigSection)
				{
					return templateSection;
				}
			}
			throw new TemplateDefinitionException("You cannot have a Template without a '#Config' Section.", new CellReference("First Sheet", ""));
		}

		internal TemplateSection GetEndOfReportSection()
		{
			for (int index = (Count - 1); index >= 0; index--)
			{
				TemplateSection templateSection = this[index];
				if (templateSection.TypeCode == ConfigurableSectionTypeList.Codes.EndOfReport)
				{
					return templateSection;
				}
			}
			throw new TemplateDefinitionException("You cannot have a Template without a '#EndOfReport' Section.", new CellReference("First Sheet", ""));
		}

		internal class CacheManager
		{
			internal static CacheManager Get(BusinessObjectFactory sourceFactory, BusinessObjectFactory temporaryFactory)
			{
				return temporaryFactory.GetCachedValue<CacheManager>("TemplateSectionCollectionCacheManager", delegate
				{
					return Get(sourceFactory);
				});
			}

			internal static CacheManager Get(BusinessObjectFactory sourceFactory)
			{
				return sourceFactory.GetCachedValue<CacheManager>("TemplateSectionCollectionCacheManager", delegate
				{
					return new CacheManager(sourceFactory);
				});
			}

			CacheManager(BusinessObjectFactory factory)
			{
				Argument.NotNull(factory, "BusinessObjectFactory factory");
				this.factory = factory;
			}
			readonly BusinessObjectFactory factory;

			internal TemplateSectionCollection SystemTemplateSections
			{
				get { return systemTemplateSections ?? (systemTemplateSections = GetSystemTemplateSections()); }
			}
			TemplateSectionCollection systemTemplateSections;

			TemplateSectionCollection GetSystemTemplateSections()
			{
				var result = new TemplateSectionCollection(null);
				var systemTemplateQuery = new ZQuery(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System);
				var systemTemplate = factory.LoadTop1<StmTemplateBase>(systemTemplateQuery);
				if (systemTemplate != null)
				{
					result.AddRange(systemTemplate.TemplateSections);
				}
				return result;
			}

			internal TemplateSectionCollection GetCollection()
			{
				return templateSections ?? (templateSections = GetNewCollection());
			}
			TemplateSectionCollection templateSections;

			TemplateSectionCollection GetNewCollection()
			{
				var result = new TemplateSectionCollection(null);

				if (SystemTemplateSections.Count > 0)
				{
					var userTemplateQuery = new ZQuery(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.User);
					var userTemplate = factory.LoadTop1<StmTemplateBase>(userTemplateQuery);
					if (userTemplate != null)
					{
						result.AddRange(userTemplate.TemplateSections);
					}

					foreach (TemplateSection systemSection in SystemTemplateSections)
					{
						if (!result.Contains(systemSection.SectionName))
						{
							result.Add(systemSection);
						}
					}
				}

				return result;
			}

			internal TemplateSectionCollection GetConfigurableOnlyCollection()
			{
				return templateConfigurableOnlySections ?? (templateConfigurableOnlySections = GetNewConfigurableOnlyCollection());
			}
			TemplateSectionCollection templateConfigurableOnlySections;

			TemplateSectionCollection GetNewConfigurableOnlyCollection()
			{
				var result = new TemplateSectionCollection(null);

				var systemTemplateQuery = new ZQuery(StmTemplateSchema.SO_Name, SectionRepositoryTemplateNames.System);
				var systemTemplate = factory.LoadTop1<StmTemplateBase>(systemTemplateQuery);
				if (systemTemplate != null)
				{
					var userTemplatesQuery = new ZQuery(StmTemplateSchema.SO_Name, SQLComparisonOperator.StartsWith, SectionRepositoryTemplateNames.User);
					var userTemplates = factory.Load<StmTemplateBase>(userTemplatesQuery);
					foreach (var template in userTemplates)
					{
						result.AddRange(template.TemplateConfigurableOnlySections);
					}

					result.AddRange(systemTemplate.TemplateConfigurableOnlySections);
				}

				return result;
			}

			internal void InvalidateTemplateSectionsCache()
			{
				templateSections = null;
			}
		}
	}
}
