using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.Testing
{
	[TestedType(typeof(SplitDuplicateShortHeadersEnglish))]
	sealed class SplitDuplicateShortHeadersEnglishTest : UpdateTemplateCommandTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SplitDuplicateShortHeadersEnglish(Factory);
		}
	}
}
