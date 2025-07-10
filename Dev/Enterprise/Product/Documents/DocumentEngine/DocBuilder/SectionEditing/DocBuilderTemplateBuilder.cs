using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngine.DocBuilder.SectionEditing
{
	public class DocBuilderTemplateBuilder
	{
		public DocBuilderTemplateBuilder(StmTemplateBase template)
		{
			Argument.NotNull(template, "template");
			this.template = template;
		}

		readonly StmTemplateBase template;

		public TemplateSection CopySectionAndInsertAtEnd(StmTemplateBase sourceTemplate, ZString sectionName)
		{
			int startingRowNumber;
			var endOfReportSection = template.TemplateSections.Find(ConfigurableSectionTypeList.Descriptions.EndOfReport);
			if (endOfReportSection != null)
			{
				startingRowNumber = endOfReportSection.StartingRowNumber - 1;
			}
			else
			{
				var finalSection = template.TemplateSections.Cast<TemplateSection>().OrderByDescending(s => s.LastRowNumber).FirstOrDefault();
				startingRowNumber = finalSection != null ? finalSection.LastRowNumber : 0;
			}

			return CopySectionAndInsertAt(sourceTemplate, sectionName, startingRowNumber);
		}

		public TemplateSection CopySectionAndInsertAt(StmTemplateBase sourceTemplate, ZString sectionName, int row)
		{
			Argument.NotNull(sourceTemplate, "sourceTemplate");
			TemplateSection result = null;
			var sourceSection = sourceTemplate.TemplateSections.Find(sectionName);
			if (sourceSection != null)
			{
				using (var sourceExcelInterface = new ExcelInterface(sourceTemplate.SO_Template))
				{
					var sourceWorkSheet = sourceExcelInterface.WorkSheets.Find(SectionRepository.DocumentWorkSheetName);

					using (var excelInterface = new ExcelInterface(template.SO_Template))
					{
						var workSheet = excelInterface.WorkSheets.Find(SectionRepository.DocumentWorkSheetName);
						if (workSheet != null)
						{
							workSheet.CopyAndInsertRows(
								sourceWorkSheet,
								sourceSection.StartingRowNumber - 1,
								sourceSection.RowCount + 1,
								row);

							using (var stream = new MemoryStream())
							{
								try
								{
									excelInterface.SaveToStream(stream);
								}
								catch (ExcelLimitationForThisFileFormatException ex)
								{
									if (Globals.CanShowDialogs)
									{
										var answer = ExcelLimitationsHelper.Messages.ShowTooManyForExcel2003WithFormatSwitchQuestion(ex);
										if (answer == ZDialogResult.Yes)
										{
											stream.SetLength(0);
											stream.Position = 0;
											excelInterface.SaveToStream(stream, AttachmentTypeList.Codes.Xlsx);
										}
										else
										{
											throw;
										}
									}
									else
									{
										throw;
									}
								}

								template.SO_Template = stream.CopyToByteArray();
							}
						}
						else
						{
							throw new MissingWorksheetException(string.Format("Could not find worksheet named [{0}] in template [{1}].", SectionRepository.DocumentWorkSheetName, template.SO_Name), SectionRepository.DocumentWorkSheetName, template.SO_Name);
						}
						result = template.TemplateSections.Find(sectionName);
					}
				}
			}

			return result;
		}

		public void RemoveSection(ZString sectionName)
		{
			var section = template.TemplateSections.Find(sectionName);
			if (section != null)
			{
				using (var excelInterface = new ExcelInterface(template.SO_Template))
				{
					var workSheet = excelInterface.WorkSheets.Find(SectionRepository.DocumentWorkSheetName);

					workSheet.RemoveRows(section.StartingRowNumber - 1, section.LastRowNumber);

					using (var stream = new MemoryStream())
					{
						excelInterface.SaveToStream(stream);
						template.SO_Template = stream.CopyToByteArray();
					}
				}
			}
		}
	}
}
