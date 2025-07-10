using CargoWise.Definitions;
using Enterprise.DocumentEngine.DocumentMenu.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(ReportMenuCustomisationDocumentSupporter))]
	sealed class ReportMenuCustomisationDocumentSupporterTest : MenuCustomisationDocumentSupporterTest
	{
		protected override MenuCustomisationDocumentSupporter DocumentSupporter
		{
			get
			{
				if (fDocumentSupporter == null)
				{
					var menuCustomisationBO = DocumentMenuCustomisation.New(null, null, Factory);
					fDocumentSupporter = new ReportMenuCustomisationDocumentSupporter(menuCustomisationBO);
				}

				return fDocumentSupporter;
			}
		}
		MenuCustomisationDocumentSupporter fDocumentSupporter;

		protected override BusinessContext SupportedBusinessContext
		{
			get
			{
				return BusinessContext.DocReportCustomiser;
			}
		}

		protected override IDocumentSupportable GetDocumentSupportableBusinessObject()
		{
			var customisation = new ReportMenuCustomisation(Factory, "RepRefFilesReports");
			return customisation;
		}
	}
}
