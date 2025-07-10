#if DEBUG
using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating
{
	class ConfigurableTemplateManipulator
	{
		readonly StmTemplateBase template;

		internal ConfigurableTemplateManipulator(StmTemplateBase template)
		{
			this.template = template;
		}

		internal void RenameSection(ZString sectionName, ZString renamedSectionName)
		{
			if (sectionName.Trim().EqualsIgnoringCase(renamedSectionName.Trim()))
			{ return; }

			var templateSections = template.TemplateSections;

			if (templateSections.Find(renamedSectionName) != null)
			{
				throw new InvalidOperationException(string.Format("Can not rename section [{0}] to [{1}] because section name already exists.", sectionName, renamedSectionName));
			}

			var templateSection = template.TemplateSections.Find(sectionName);
			if (templateSection != null)
			{
				using (var excelFile = GetNewExcelFileWithTemplateLoaded())
				{
					var workSheet = excelFile.WorkSheets[0];
					var rowIndex = templateSection.StartingRowNumber - 1;
					workSheet[rowIndex, 0] = workSheet[rowIndex, 0].ToString().Replace(templateSection.SectionName, renamedSectionName);
					template.InvalidateTemplateSections();

					SaveExcelFileToTemplate(excelFile);
				}

				template.Factory.Save();
			}
			else
			{
				throw new InvalidOperationException(string.Format("Can not find section [{0}] to rename.", sectionName));
			}
		}

		internal void RenameSectionAndConfigItems(ZString sectionName, ZString renamedSectionName)
		{
			RenameSection(sectionName, renamedSectionName);
			RenameConfigItems(sectionName, renamedSectionName);
		}

		internal void RenameConfigItems(ZString sectionName, ZString renamedSectionName)
		{
			if (sectionName.Trim().EqualsIgnoringCase(renamedSectionName.Trim()))
			{ return; }

			foreach (var configItem in GetConfigItems(sectionName))
			{
				configItem.S4_SectionItemName = renamedSectionName;
			}

			template.Factory.Save();
		}

		internal void ChangeSectionType(string sectionName, string sectionType)
		{
			var templateSections = template.TemplateSections;
			var templateSection = template.TemplateSections.Find(sectionName);

			if (templateSection != null)
			{
				using (var excelFile = GetNewExcelFileWithTemplateLoaded())
				{
					var workSheet = excelFile.WorkSheets[0];
					var rowIndex = templateSection.StartingRowNumber - 1;
					workSheet[rowIndex, 0] = "#ConfigurableSection:" + sectionType + ", " + templateSection.SectionName;
					template.InvalidateTemplateSections();

					SaveExcelFileToTemplate(excelFile);

					template.Factory.Save();
				}
			}
		}

		internal void ChangeConfigItemType(string sectionName, string sectionType)
		{
			foreach (var configItem in GetConfigItems(sectionName))
			{
				configItem.S4_SectionType = sectionType;
			}

			template.Factory.Save();
		}

		internal void ChangeSectionAndConfigType(string sectionName, string sectionType, ZString configSectionType)
		{
			ChangeSectionType(sectionName, sectionType);
			ChangeConfigItemType(sectionName, configSectionType);
		}

		internal void RemoveSection(ZString sectionName)
		{
			var templateSections = template.TemplateSections;
			var templateSection = template.TemplateSections.Find(sectionName);

			if (templateSection != null)
			{
				using (var excelFile = GetNewExcelFileWithTemplateLoaded())
				{
					var workSheet = excelFile.WorkSheets[0];
					workSheet.RemoveRows(templateSection.StartingRowNumber - 1, templateSection.StartingRowNumber + templateSection.RowCount);
					template.InvalidateTemplateSections();

					SaveExcelFileToTemplate(excelFile);
				}

				foreach (var configItem in GetConfigItems(templateSection.SectionName))
				{
					configItem.Delete();
				}

				template.Factory.Save();
			}
		}

		internal void SplitSection(string sectionToSplit, int splitAtRowIndex, string splitSectionName)
		{
			SplitSection(sectionToSplit, splitAtRowIndex, sectionToSplit, splitSectionName);
		}

		internal void SplitSection(string sectionToSplit, int splitAtRowIndex, string splitSectionName1, string splitSectionName2)
		{
			var templateSections = template.TemplateSections;

			if (sectionToSplit != splitSectionName1 && templateSections.Find(splitSectionName1) != null)
			{
				throw new InvalidOperationException(string.Format("The section [{0}] already exists.", splitSectionName1));
			}

			if (sectionToSplit != splitSectionName2 && templateSections.Find(splitSectionName2) != null)
			{
				throw new InvalidOperationException(string.Format("The section [{0}] already exists.", splitSectionName2));
			}

			var templateSection = template.TemplateSections.Find(sectionToSplit);

			if (templateSection != null)
			{
				using (var excelFile = GetNewExcelFileWithTemplateLoaded())
				{
					var addedCellContent = string.Empty;
					var workSheet = excelFile.WorkSheets[0];
					var rowIndexToInsert = templateSection.StartingRowNumber + splitAtRowIndex;
					workSheet.CopyAndInsertRows(workSheet, templateSection.StartingRowNumber - 1, 1, rowIndexToInsert);
					addedCellContent = workSheet[rowIndexToInsert, 0].ToString().Replace(templateSection.SectionName, splitSectionName2);
					workSheet[rowIndexToInsert, 0] = addedCellContent;
					template.InvalidateTemplateSections();

					SaveExcelFileToTemplate(excelFile);
				}

				RenameSection(sectionToSplit, splitSectionName1);

				template.Factory.Save();
			}
		}

		internal void SplitSectionAndConfigItems(string sectionToSplit, int splitAtRowIndex, string splitSectionName)
		{
			SplitSectionAndConfigItems(sectionToSplit, splitAtRowIndex, sectionToSplit, splitSectionName);
		}

		internal void SplitSectionAndConfigItems(string sectionToSplit, int splitAtRowIndex, string splitSectionName1, string splitSectionName2)
		{
			SplitSection(sectionToSplit, splitAtRowIndex, splitSectionName1, splitSectionName2);

			RenameConfigItems(sectionToSplit, splitSectionName1);

			foreach (var configItem in GetConfigItems(splitSectionName1))
			{
				var configItems = configItem.ParentConfig.ConfigItems;

				foreach (StmMenuDocumentConfigItem configItemToOffset in configItems)
				{
					if (configItemToOffset.S4_PrintOrder > configItem.S4_PrintOrder)
					{
						configItemToOffset.S4_PrintOrder++;
					}
				}

				var addedConfigItem = configItems.AddNew();
				addedConfigItem.S4_PrintOrder = configItem.S4_PrintOrder + 1;
				addedConfigItem.S4_SectionItemName = splitSectionName2;
				addedConfigItem.S4_SectionType = configItem.S4_SectionType;

				configItems.RebuildPrintOrdersBySectionTypes();
			}

			template.Factory.Save();
		}

		internal void SplitReplaceAndRemove(string sectionName, int splitAtRowIndex, string splitSectionName, string replacementSectionName)
		{
			SplitSectionAndConfigItems(
				sectionName,
				splitAtRowIndex,
				splitSectionName);

			ChangeConfigItemToUseAnotherSectionAndRemoveOldSection(
				sectionName,
				replacementSectionName);
		}

		public void RemoveConditionalStatementsAndPutInFilter(string sectionName, ZString filterList)
		{
			var templateSections = template.TemplateSections;
			var templateSection = template.TemplateSections.Find(sectionName);

			if (templateSection != null)
			{
				using (var excelInterface = GetNewExcelFileWithTemplateLoaded())
				{
					var workSheet = excelInterface.WorkSheets[0];

					for (var rowIndex = templateSection.LastRowNumber - 1; rowIndex >= templateSection.StartingRowNumber; rowIndex--)
					{
						var cellContent = workSheet[rowIndex, 0].ToString();
						if (IsConditionalCellContent(cellContent))
						{
							workSheet.RemoveRow(rowIndex);
						}
					}

					template.InvalidateTemplateSections();

					SaveExcelFileToTemplate(excelInterface);

					ChangeFilterList(sectionName, filterList);

					template.Factory.Save();
				}
			}
		}

		public void ChangeFilterList(string sectionName, ZString filterList)
		{
			var templateSections = template.TemplateSections;
			var templateSection = template.TemplateSections.Find(sectionName);

			if (templateSection != null)
			{
				foreach (var configItem in GetConfigItems(templateSection.SectionName))
				{
					configItem.S4_FilterList = filterList;
				}

				template.Factory.Save();
			}
		}

		internal void ChangeConfigItemToUseAnotherSection(string sectionNameToChange, string sectionName)
		{
			var templateSections = template.TemplateSections;

			if (templateSections.Find(sectionName) == null)
			{
				throw new InvalidOperationException(string.Format("Section [{0}] could not be found.", sectionName));
			}

			var templateSection = templateSections.Find(sectionNameToChange);

			if (templateSection != null)
			{
				foreach (var configItem in GetConfigItems(templateSection.SectionName))
				{
					configItem.S4_SectionItemName = sectionName;
				}

				template.Factory.Save();
			}
			else
			{
				throw new InvalidOperationException(string.Format("Section [{0}] could not be found.", sectionNameToChange));
			}
		}

		internal void ChangeConfigItemToUseAnotherSectionAndRemoveOldSection(string sectionNameToChange, string sectionName)
		{
			if (template.TemplateSections.Find(sectionNameToChange) != null && template.TemplateSections.Find(sectionName) != null)
			{
				ChangeConfigItemToUseAnotherSection(sectionNameToChange, sectionName);
				RemoveSection(sectionNameToChange);
			}
		}

		public bool StripAreaTagIfOnlyTagAndIsFirstRow(string sectionName, string areaTagToStrip)
		{
			var result = false;
			var templateSections = template.TemplateSections;
			var templateSection = template.TemplateSections.Find(sectionName);

			if (templateSection != null)
			{
				using (var excelInterface = GetNewExcelFileWithTemplateLoaded())
				{
					var workSheet = excelInterface.WorkSheets[0];

					result = workSheet[templateSection.StartingRowNumber, 0].ToString().Equals(areaTagToStrip, System.StringComparison.InvariantCultureIgnoreCase);

					if (result)
					{
						for (var rowIndex = templateSection.StartingRowNumber + 1; rowIndex < templateSection.LastRowNumber; rowIndex++)
						{
							var cellContent = workSheet[rowIndex, 0].ToString();
							if (cellContent.StartsWith("#") && !IsConditionalCellContent(cellContent))
							{
								result = false;
								break;
							}
						}
					}

					if (result)
					{
						workSheet.RemoveRow(templateSection.StartingRowNumber);
					}

					template.InvalidateTemplateSections();

					SaveExcelFileToTemplate(excelInterface);
				}
			}

			return result;
		}

		public void RemoveAllTags(string sectionName)
		{
			RemoveSectionRows(sectionName, cellContent => cellContent.StartsWith("#"));
		}

		public void RemoveAllSectionAreaIdentifiers(string sectionName)
		{
			RemoveSectionRows(sectionName, cellContent => cellContent.StartsWith("#") && !IsConditionalCellContent(cellContent));
		}

		void RemoveSectionRows(string sectionName, Predicate<String> cellContentPredicate)
		{
			var templateSections = template.TemplateSections;
			var templateSection = template.TemplateSections.Find(sectionName);

			if (templateSection != null)
			{
				using (var excelInterface = GetNewExcelFileWithTemplateLoaded())
				{
					var workSheet = excelInterface.WorkSheets[0];

					for (var rowIndex = templateSection.LastRowNumber - 1; rowIndex >= templateSection.StartingRowNumber; rowIndex--)
					{
						var cellContent = workSheet[rowIndex, 0].ToString();
						if (cellContentPredicate(cellContent))
						{
							workSheet.RemoveRow(rowIndex);
						}
					}

					template.InvalidateTemplateSections();

					SaveExcelFileToTemplate(excelInterface);
				}
			}
		}

		internal bool IsTemplateSectionContainsNoAreaTags(string sectionName)
		{
			var result = true;
			var templateSections = template.TemplateSections;
			var templateSection = template.TemplateSections.Find(sectionName);

			if (templateSection != null)
			{
				using (var excelInterface = GetNewExcelFileWithTemplateLoaded())
				{
					var workSheet = excelInterface.WorkSheets[0];

					for (var rowIndex = templateSection.StartingRowNumber; rowIndex < templateSection.LastRowNumber; rowIndex++)
					{
						var cellContent = workSheet[rowIndex, 0].ToString();
						if (cellContent.Contains("#") && !IsConditionalCellContent(cellContent))
						{
							result = false;
							break;
						}
					}
				}
			}

			return result;
		}

		ExcelInterface GetNewExcelFileWithTemplateLoaded()
		{
			var result = new ExcelInterface();

			using (var stream = new MemoryStream(template.SO_Template))
			{
				result.LoadExcelFile(stream);
			}

			return result;
		}

		void SaveExcelFileToTemplate(ExcelInterface excelFile)
		{
			using (var stream = new MemoryStream())
			{
				excelFile.SaveToStream(stream);
				template.SO_Template = stream.CopyToByteArray();
			}
		}

		StmMenuDocumentConfigItem[] GetConfigItems(ZString sectionName)
		{
			var templateSubQuery = new ZDBOnlySubQuery(typeof(StmTemplate), StmTemplateSchema.PK);
			templateSubQuery.AddToFilter(StmTemplateSchema.PK, template.PK);

			var pivotSubQuery = new ZDBOnlySubQuery(typeof(StmMenuTemplatePivot), StmMenuTemplatePivotSchema.PK);
			pivotSubQuery.AddSubQuery(StmMenuTemplatePivotSchema.SI_SO, templateSubQuery, JoinCondition.And);

			var configSubQuery = new ZDBOnlySubQuery(typeof(StmMenuDocumentConfig), StmMenuDocumentConfigSchema.PK);
			configSubQuery.AddSubQuery(StmMenuDocumentConfigSchema.S3_SI, pivotSubQuery, JoinCondition.And);

			var query = new ZDBOnlyQuery(typeof(StmMenuDocumentConfigItem));
			query.AddToFilter(StmMenuDocumentConfigItemSchema.S4_SectionItemName, sectionName);
			query.AddSubQuery(StmMenuDocumentConfigItemSchema.S4_S3, configSubQuery, JoinCondition.And);

			return template.Factory.Load<StmMenuDocumentConfigItem>(query);
		}

		bool IsConditionalCellContent(string cellContent)
		{
			return
				cellContent.StartsWith("#if", StringComparison.InvariantCultureIgnoreCase) ||
				cellContent.StartsWith("#else", StringComparison.InvariantCultureIgnoreCase) ||
				cellContent.StartsWith("#endif", StringComparison.InvariantCultureIgnoreCase);
		}
	}
}
#endif
