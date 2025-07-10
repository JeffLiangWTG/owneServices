using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.Testing
{
	[TestedType(typeof(SplitDuplicateServiceDocumentTitlesEnglish))]
	sealed class SplitDuplicateServiceDocumentTitlesEnglishTest : UpdateTemplateCommandTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SplitDuplicateServiceDocumentTitlesEnglish(Factory);
		}
	}
}
