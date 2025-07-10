using CargoWise.Definitions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentMenuCustomisationDocumentSupporter))]
	sealed class DocumentMenuCustomisationDocumentSupporterTest : MenuCustomisationDocumentSupporterTest
	{
		protected override MenuCustomisationDocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					var menuCustomisationBO = DocumentMenuCustomisation.New(null, null, Factory);
					fDocumentSupporter = new DocumentMenuCustomisationDocumentSupporter(menuCustomisationBO);
				}

				return fDocumentSupporter;
			}
		}
		MenuCustomisationDocumentSupporter fDocumentSupporter;

		protected override BusinessContext SupportedBusinessContext
		{
			get
			{
				return BusinessContext.DocumentCustomiser;
			}
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var customization = DocumentMenuCustomisation.New(null, null, Factory);
			return customization;
		}
	}
}
