using System.Linq;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Languages = Enterprise.Core.Constants.Languages;

namespace Enterprise.DocumentEngine.DocumentMenu
{
	public class DocumentConfigDocumentPackLoader
	{
		public DocumentConfigDocumentPackLoader(DocumentPack documentPack)
		{
			this.documentPack = documentPack;
		}

		readonly DocumentPack documentPack;

		public void Load(IDocumentConfig documentConfig, IDocumentSupportable documentSupportable)
		{
			var factory = documentSupportable.DocumentSupporter.Factory;
			var template = StmTemplateBase.GetDocBuilderTemplate(factory, DocBuilderTemplateType.System);
			var templateGenerator = new TemplateGenerator(documentConfig, template, Languages.English);
			var documentCommand = documentPack.StmMenuCommand.Factory.Load<DocumentCommand>(documentPack.StmMenuCommand.PK);
			var boDocDataProviders = documentSupportable.DocumentSupporter.GetBODocDataProviders(templateGenerator.DataContext, documentCommand);

			if (boDocDataProviders?.FirstOrDefault() != null)
			{
				var dataProviders = new DataProviderList(boDocDataProviders);
				var topLevelDataProvider = BODocDataProvider.Get(documentSupportable.DocumentSupporter.BusinessObject);

				if (!dataProviders.AllDataProviders.Contains(topLevelDataProvider))
				{
					dataProviders.Add(topLevelDataProvider);
				}

				if (!string.IsNullOrEmpty(documentConfig.DocumentType))
				{
					foreach (var docDataProvider in dataProviders.AllDataProviders)
					{
						IDocTypeCode docDataTypeCode = docDataProvider as IDocTypeCode;
						if (docDataTypeCode != null)
						{
							docDataTypeCode.DocTypeCode = documentConfig.DocumentType;
						}
					}
				}

				var filterEvaluator = new FilterEvaluator(boDocDataProviders);
				var excelTemplate = templateGenerator.Generate(filterEvaluator);
				var report = new Report(documentPack, excelTemplate, dataProviders, "", null, documentCommand.DocumentDirection, false);
				report.Name = report.TranslateMacros(documentConfig.DocumentTitle);
				documentPack.Add(report);
			}
		}
	}
}
