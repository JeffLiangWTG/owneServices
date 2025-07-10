using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder.Styling;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using FlexCel.XlsAdapter;

namespace Enterprise.DocumentEngine.DocBuilder
{
	public class TemplateGenerator
	{
		public TemplateGenerator(StmMenuTemplatePivot menuTemplatePivot, OrgHeader client, ZString language)
			: this(new DocumentConfigPicker(menuTemplatePivot, TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations).GetDocumentConfig(client, GlbCompany.CurrentCompany),
				menuTemplatePivot.Template, language, !TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations)
		{
			MenuTemplatePivot = menuTemplatePivot;
		}

		public TemplateGenerator(IDocumentConfig documentConfig, StmMenuTemplatePivot menuTemplatePivot, ZString language)
			: this(documentConfig, menuTemplatePivot.Template, language, !TemplateGenerator.IgnoreUserCustomizedTemplatesAndConfigurations)
		{
			MenuTemplatePivot = menuTemplatePivot;
		}

		public TemplateGenerator(IDocumentConfig documentConfig, StmTemplate template, ZString language, bool isCustomizedSectionsIncluded = true)
		{
			Argument.NotNull(template, "template");
			this.template = template;
			this.isCustomizedSectionsIncluded = isCustomizedSectionsIncluded;
			this.documentConfig = documentConfig;
			this.language = language;
		}

		readonly IDocumentConfig documentConfig;
		readonly StmTemplate template;
		readonly ZString language;
		readonly List<TemplateGenerationError> errors = new List<TemplateGenerationError>();
		readonly bool isCustomizedSectionsIncluded;
		readonly StmMenuTemplatePivot MenuTemplatePivot;

		[ThreadStatic]
		static bool ignoreUserCustomizedTemplatesAndConfigurations;
		public static bool IgnoreUserCustomizedTemplatesAndConfigurations
		{
			get { return ignoreUserCustomizedTemplatesAndConfigurations; }
			set { ignoreUserCustomizedTemplatesAndConfigurations = value; }
		}

		internal TemplateGenerationError[] Errors
		{
			get { return errors.ToArray(); }
		}

		internal bool IsCustomizedSectionsIncluded
		{
			get { return isCustomizedSectionsIncluded; }
		}

		internal bool IsDocumentConfigValid
		{
			get { return documentConfig != null && !documentConfig.IsTemplate || !template.IsDocBuilderStyle; }
		}

		internal bool IsDocumentConfigTemplate
		{
			get { return documentConfig != null && documentConfig.IsTemplate; }
		}

		internal string DocumentTitle => documentConfig?.DocumentTitle;

		public DataContextValue DataContext
		{
			get
			{
				DataContextValue result = null;
				if (documentConfig != null)
				{
					var overrideDataContext = documentConfig.OverrideDataContext;
					if (DataContextValue.IsValidFullDataContext(overrideDataContext))
					{
						result = new DataContextValue(overrideDataContext);
					}
				}

				return result ?? new DataContextValue(template.SO_DataContext);
			}
		}

		public ExcelTemplate Generate(FilterEvaluator filterEvaluator)
		{
			errors.Clear();
			ExcelTemplate result = new ExcelTemplateReadFromStmTemplateTable(template);

			if (template.IsDocBuilderStyle)
			{
				try
				{
					result = GetDocBuilderTemplate(result, filterEvaluator);
				}
				catch (FlexCelXlsAdapterException ex)
				{
					throw new TemplateGeneratingException(ex.Message, ex);
				}
			}

			return result;
		}

		ExcelTemplate GetDocBuilderTemplate(ExcelTemplate sourceTemplate, FilterEvaluator filterEvaluator)
		{
			var sectionFiltersWithMacrosEvaluated = SectionFiltersEvaluator.GetEvaluatedSectionFilters(documentConfig, filterEvaluator);

			bool isReadOnlyCacheKey = !isCustomizedSectionsIncluded;
			var templateKey = new TemplateKey(template, MenuTemplatePivot, sectionFiltersWithMacrosEvaluated, language, isReadOnlyCacheKey, isCustomizedSectionsIncluded);

			var docBuilderFile = TemplateCache.Instance.Get(templateKey, () => GenerateDocBuilderFile(sourceTemplate, filterEvaluator));
			try
			{
				return GenerateAndStyleTemplateFromExcelFile(docBuilderFile, sourceTemplate);
			}
			finally
			{
				if (!docBuilderFile.IsCached)
				{
					docBuilderFile.Dispose();
				}
			}
		}

		ExcelInterface GenerateDocBuilderFile(ExcelTemplate sourceTemplate, FilterEvaluator filterEvaluator)
		{
			var sectionRepository = new SectionRepository(sourceTemplate);
			var extractor = GetExtractorForRepository(sectionRepository);

			using (Res.TemporarilySwitchLanguage(language))
			{
				return extractor.GenerateExcelFile(documentConfig, filterEvaluator ?? new FilterEvaluator(), errors);
			}
		}

		TemplateExtractor GetExtractorForRepository(SectionRepository repository)
		{
			var extractor = new TemplateExtractor(repository);

			if (template.SO_Name.EqualsIgnoringCase(SectionRepositoryTemplateNames.System))
			{
				if (isCustomizedSectionsIncluded)
				{
					extractor.AddOverridingSectionRepositoryIfTemplateExists(SectionRepositoryTemplateNames.User, language, template.Factory);
					extractor.AddOverridingSectionRepositoryIfTemplateExists(SectionRepositoryTemplateNames.User, template.Factory);
				}
				extractor.AddOverridingSectionRepositoryIfTemplateExists(SectionRepositoryTemplateNames.System, language, template.Factory);
			}

			return extractor;
		}

		protected ExcelTemplate GenerateAndStyleTemplateFromExcelFile(ExcelInterface docBuilderFile, ExcelTemplate sourceTemplate)
		{
			lock (docBuilderFile)
			{
				var areaMerger = new AreaMerger();
				areaMerger.Merge(docBuilderFile);

				var stylizer = new TemplateStylizer(docBuilderFile, DocumentsDataRegistry.Instance.DocBuilderTheme.Value.SelectedTheme);
				stylizer.Stylize();

				const int initialSizeEstimatedForDocBuilderAsByteArray = 16384;

				using (var stream = new MemoryStream(initialSizeEstimatedForDocBuilderAsByteArray))
				{
					docBuilderFile.SaveToStream(stream);

					return new ExcelTemplateReadFromByteArray(sourceTemplate.TemplateName, string.Empty, stream.CopyToByteArray())
					{
						ContainsCustomisedSections = docBuilderFile.ContainsCustomisedSections
					};
				}
			}
		}
	}
}
