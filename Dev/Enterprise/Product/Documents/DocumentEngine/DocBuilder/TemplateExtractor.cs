using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.ZArchitecture.Schema;
using FlexCel.Core;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.DocBuilder
{
	class TemplateExtractor
	{
		internal TemplateExtractor(SectionRepository baseSectionRepository)
		{
			this.baseSectionRepository = baseSectionRepository;
			this.overridingSectionRepositories = new List<SectionRepository>();
		}

		readonly SectionRepository baseSectionRepository;
		readonly List<SectionRepository> overridingSectionRepositories;

		internal void AddOverridingSectionRepositoryIfTemplateExists(ZString templateName, BusinessObjectFactory factory)
		{
			var query = new ZQuery(StmTemplateSchema.SO_Name, templateName);
			query.AddToFilter(StmTemplateSchema.SO_IsUserConfigurable, true);
			var template = factory.LoadTop1<StmTemplateBase>(query);
			if (template != null)
			{
				var sectionRepository = new SectionRepository(template.GetExcelTemplate());
				if (template.IsDocBuilderStyle)
				{
					overridingSectionRepositories.Add(sectionRepository);
				}
			}
		}

		internal void AddOverridingSectionRepositoryIfTemplateExists(ZString templateName, ZString language, BusinessObjectFactory factory)
		{
			if (!language.IsEmpty && !language.Equals(Enterprise.Core.Constants.Languages.English))
			{
				AddOverridingSectionRepositoryIfTemplateExists(ZString.Format("{0} [{1}]", templateName, language), factory);
			}
		}

		List<SectionRepository> GetAllSectionRepositories()
		{
			var result = new List<SectionRepository>(overridingSectionRepositories);
			result.Add(baseSectionRepository);
			return result;
		}

		internal ExcelInterface GenerateExcelFile(IDocumentConfig documentConfig, FilterEvaluator filterEvaluator, IList<TemplateGenerationError> errors)
		{
			var sourceWorkSheets = new Dictionary<SectionRepository, ExcelWorkSheet>();
			ExcelWorkSheet destinationWorkSheet = null;
			var containsCustomisedSections = false;
			try
			{
				var allSectionRepositories = GetAllSectionRepositories();

				foreach (var sectionRepository in allSectionRepositories)
				{
					var workSheet = GetWorkSheet(sectionRepository, errors);
					if (workSheet != null)
					{
						sourceWorkSheets.Add(sectionRepository, workSheet);
					}
				}

				//Copying sections from one workbook to another one is *very* slow, so we reuse the workbook containing most of the sections we want.
				//Although creating an empty workbook and adding just the sections we want to it is more straight forward, it has a huge (bad!) performance impact.
				var destinationSectionRep = GetBestDestinationSectionRepository(documentConfig, filterEvaluator);
				destinationWorkSheet = sourceWorkSheets[destinationSectionRep];
				var fileFormat = destinationWorkSheet.ParentExcelInterface.Xls.FileFormatWhenOpened;

				var stripManipulator = new StripManipulator(destinationWorkSheet);

				var configSection = baseSectionRepository.AllSections.GetConfigSection();
				stripManipulator.CopyAndInsertRows(sourceWorkSheets[baseSectionRepository], configSection.StartingRowNumber - 1, configSection.RowCount + 1, null);

				if (documentConfig != null)
				{
					documentConfig.ConfigItems.SortByPrintOrder();

					foreach (IDocumentConfigItem documentConfigItem in documentConfig.ConfigItems)
					{
						if (documentConfigItem.EvaluatedValue ?? filterEvaluator.MatchesFilter(documentConfigItem.FilterList))
						{
							var sectionName = documentConfigItem.SectionName;

							bool templateSectionFound = false;

							foreach (var sectionRepository in allSectionRepositories)
							{
								var templateSection = sectionRepository.AllSections.Find(sectionName);

								if (templateSection != null && !templateSection.IsControlSection)
								{
									if (sectionRepository != baseSectionRepository)
									{
										containsCustomisedSections = true;
									}

									var sourceWorkSheet = sourceWorkSheets[sectionRepository];
									stripManipulator.InsertStrip(documentConfigItem, sourceWorkSheet, templateSection);

									if (fileFormat != TFileFormats.Xlsx && sourceWorkSheet.ParentExcelInterface.Xls.FileFormatWhenOpened == TFileFormats.Xlsx)
									{
										fileFormat = TFileFormats.Xlsx;
									}

									templateSectionFound = true;
									break;
								}
							}

							if (!templateSectionFound)
							{
								string message = Res.GetString("4b3a0e88-197c-460a-9aec-461ca8d3670e", "Section [{0}] could not be found and has been ignored.", sectionName);
								errors.Add(new TemplateGenerationError(message, ReportProcessingErrorSeverity.WarningWithoutErrorReport));
							}
						}
					}
				}

				var endOfReportSection = baseSectionRepository.AllSections.GetEndOfReportSection();
				stripManipulator.CopyAndInsertRows(sourceWorkSheets[baseSectionRepository], endOfReportSection.StartingRowNumber - 1, endOfReportSection.RowCount + 1, null);
				stripManipulator.RemoveUnnecessaryRows();

				if (containsCustomisedSections)
				{
					var customizedRowNumber = destinationSectionRep.AllSections.GetConfigSection().StartingRowNumber;
					destinationWorkSheet.InsertRows(customizedRowNumber, 1);
					destinationWorkSheet[customizedRowNumber, 0] = Constants.ConfigAreaParameters.ContainsCustomisedSections + true;
				}

				if (documentConfig != null)
				{
					var setter = new TemplatePageLayoutSetter();
					setter.SetPageLayout(destinationWorkSheet, documentConfig.PageStyle);
				}

				var excelInterface = GetNewExcelInterfaceAndCopySheets(destinationWorkSheet.ParentExcelInterface.Xls);
				excelInterface.ContainsCustomisedSections = containsCustomisedSections;
				excelInterface.Xls.DefaultFileFormat = fileFormat;

				if (Res.IsRightToLeft(Res.CurrentLanguage))
				{
					new RightToLeftTransformer(excelInterface.WorkSheets[0]).RightToLeft();
				}

				return excelInterface;
			}
			finally
			{
				foreach (var sourceWorkSheet in sourceWorkSheets.Values)
				{
					if (sourceWorkSheet != destinationWorkSheet)
					{
						sourceWorkSheet.Dispose();
						sourceWorkSheet.ParentExcelInterface.Dispose();
					}
				}
			}
		}

		// This method will return a new ExcelInterface and copy sheets from the source XlsFile.
		// This will get rid of all unused styles from the original file
		ExcelInterface GetNewExcelInterfaceAndCopySheets(XlsFile sourceWorkbook)
		{
			var excelInterface = new ExcelInterface(new XlsFile(), false);
			excelInterface.NewExcelFile(1); //Create a new workbook. It needs at least 1 (blank) sheet.
			excelInterface.Xls.InsertAndCopySheets(1, 1, 1, sourceWorkbook); //Add the generated template as the first sheet in the target workbook
			excelInterface.Xls.ActiveSheet = 2; // Select the blank sheet ...
			excelInterface.Xls.DeleteSheet(1); // ... and delete it
			excelInterface.UpdateWorkSheets();
			return excelInterface;
		}

#if DEBUG
		internal
#endif
		SectionRepository GetBestDestinationSectionRepository(IDocumentConfig documentConfig, FilterEvaluator filterEvaluator)
		{
			var sectionNamesToFind = new List<ZString>();

			if (documentConfig != null)
			{
				foreach (IDocumentConfigItem documentConfigItem in documentConfig.ConfigItems)
				{
					if (filterEvaluator.MatchesFilter(documentConfigItem.FilterList))
					{
						sectionNamesToFind.Add(documentConfigItem.SectionName);
					}
				}
			}

			SectionRepository result = null;
			var bestScore = -1;

			foreach (var sectionRepository in GetAllSectionRepositories())
			{
				var score = 0;

				foreach (var sectionNameToFind in sectionNamesToFind.ToArray())
				{
					var templateSection = sectionRepository.AllSections.Find(sectionNameToFind);
					if (templateSection != null)
					{
						sectionNamesToFind.Remove(sectionNameToFind);
						score += templateSection.RowCount;
					}
				}

				if (score > bestScore)
				{
					bestScore = score;
					result = sectionRepository;
				}
			}

			return result;
		}

		ExcelWorkSheet GetWorkSheet(SectionRepository sectionRepository, IList<TemplateGenerationError> errors)
		{
			ExcelWorkSheet result = null;
			var excelInterface = new ExcelInterface(new XlsFile(), false);

			try
			{
				using (var sourceStream = sectionRepository.ExcelTemplate.GetAsTemplateStream())
				{
					excelInterface.LoadExcelFile(sourceStream);
					excelInterface.ActiveWorksheet = 0;
					result = excelInterface.WorkSheets[0];
				}
			}
			catch (ExcelInterfaceException ex)
			{
				errors.Add(new TemplateGenerationError(ex.Message, ReportProcessingErrorSeverity.Fatal));
			}
			finally
			{
				if (result == null)
				{
					excelInterface.Dispose();
				}
			}

			return result;
		}
	}
}
