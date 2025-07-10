using System;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.MacroValueProviders;
using Enterprise.DocumentEngine.ValueProviders.Attributes;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.DocumentEngine.Testing
{
	sealed class AcceptanceTest : TestCaseWithFactory
	{
		public void TestMultiLineAddressIsWrapped()
		{
			AssertDocBuilderSections(MultiLineAddressIsWrapped);
		}

		delegate bool CellTester(Report report, ExcelWorkSheet workSheet, int row, int column);

		void AssertDocBuilderSections(CellTester cellTester)
		{
			var groupedErrors = new GroupedErrorList();

			using (var documentPack = new DocumentPack())
			{
				var declaration = Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>() as BusinessObject;
				var documentWrappers = DocumentWrapperFactory.GenerateGenericWrappers(Core.Constants.DataContext.GenericFreightJob, declaration);
				var dataProviders = new DataProviderList(documentWrappers);

				var customTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized);
				if (customTemplate != null)
				{
					using (var report = new Report(documentPack, customTemplate.GetExcelTemplate(), dataProviders, "Testing DocBuilder Sections", null, DocumentEngineCore.DocumentSupport.DocumentDirection.ANY, false))
					{
						var workSheet = report.WorkSheetCurrentlyBeingProcessed;
						var formatter = new CellFormatterExposingFormatting(workSheet.ParentExcelInterface.Xls);

						foreach (TemplateSection section in customTemplate.TemplateSections)
						{
							for (var row = section.StartingRowNumber; row < section.LastRowNumber; row++)
							{
								for (var column = 0; column < 72; column++)
								{
									if (!cellTester(report, workSheet, row, column))
									{
										groupedErrors.Add("(Customized) " + section.SectionName, string.Format(
@"[{0}]
{1}

",
											CellReference.GetCellRef(row, column),
											formatter.Format(workSheet, row, column)));
									}
								}
							}
						}
					}
				}

				var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
				using (var report = new Report(documentPack, systemTemplate.GetExcelTemplate(), dataProviders, "Testing DocBuilder Sections", null, DocumentEngineCore.DocumentSupport.DocumentDirection.ANY, false))
				{
					var workSheet = report.WorkSheetCurrentlyBeingProcessed;
					var formatter = new CellFormatterExposingFormatting(workSheet.ParentExcelInterface.Xls);

					foreach (TemplateSection section in systemTemplate.TemplateSections)
					{
						if (customTemplate == null || !customTemplate.TemplateSections.Contains(section.SectionName))
						{
							for (var row = section.StartingRowNumber; row < section.LastRowNumber; row++)
							{
								for (var column = 0; column < 72; column++)
								{
									if (!cellTester(report, workSheet, row, column))
									{
										groupedErrors.Add(section.SectionName, string.Format(
@"[{0}]
{1}

",
											CellReference.GetCellRef(row, column),
											formatter.Format(workSheet, row, column)));
									}
								}
							}
						}
					}
				}
			}

			groupedErrors.Assert("Multi-line addresses need to be wrapped with <ShrinkToFit>, <ExpandToFit> or <AutoHeight> macros.");
		}

		bool MultiLineAddressIsWrapped(Report report, ExcelWorkSheet workSheet, int row, int column)
		{
			if (column < 72)
			{
				var containsMultiLineAddress = false;
				var text = workSheet[row, column].ToString();

				while (RegexProvider.InnermostMacrosRegex.IsMatch(text))
				{
					foreach (Match match in RegexProvider.InnermostMacrosRegex.Matches(text))
					{
						var matchValue = match.Value;
						var valueProvider = report.MacroTranslator.GetValueProvider(Passes.FirstPass, matchValue);

						if (valueProvider is DBOrBOValueProvider)
						{
							var propertyIdentifier = matchValue.TrimStart('<').TrimEnd('>');

							var businessObjectReflector = new BusinessObjectReflector();
							var type = report.BODocDataProvider.GetType();

							var methodInfoChainLinks = businessObjectReflector.GetMethodInfoChain(type, report.BODocDataProvider, propertyIdentifier);
							if (methodInfoChainLinks != null && methodInfoChainLinks.Length > 0)
							{
								var methodInfo = methodInfoChainLinks[methodInfoChainLinks.Length - 1].MethodInfo;

								if (methodInfo.Name.Equals("ToString", StringComparison.InvariantCultureIgnoreCase) && methodInfoChainLinks.Length > 1)
								{
									methodInfo = methodInfoChainLinks[methodInfoChainLinks.Length - 2].MethodInfo;
								}

								var customAttributes = methodInfo.GetCustomAttributes(typeof(MultiLineAddressAttribute), true);
								if (customAttributes != null && customAttributes.Length > 0)
								{
									containsMultiLineAddress = true;
								}

								customAttributes = methodInfo.ReturnType.GetCustomAttributes(typeof(MultiLineAddressAttribute), true);
								if (customAttributes != null && customAttributes.Length > 0)
								{
									containsMultiLineAddress = true;
								}

								var propertyName = propertyIdentifier;

								var indexOfLastDot = propertyIdentifier.LastIndexOf('.');
								if (indexOfLastDot > 0)
								{
									propertyName = propertyName.Remove(0, indexOfLastDot);
								}
							}
						}
					}

					text = RegexProvider.InnermostMacrosRegex.Replace(text, string.Empty);
				}

				if (containsMultiLineAddress)
				{
					var input = workSheet[row, column].ToString();
					return
						AutoHeight.RegexToFindMacroAnyWhereInString.IsMatch(input) ||
						ExpandToFit.RegexToFindMacroAnyWhereInString.IsMatch(input) ||
						ShrinkToFit.RegexToFindMacroAnyWhereInString.IsMatch(input) ||
						ShrinkToFitForBillOfLading.RegexToFindMacroAnyWhereInString.IsMatch(input);
				}
			}

			return true;
		}
	}
}
