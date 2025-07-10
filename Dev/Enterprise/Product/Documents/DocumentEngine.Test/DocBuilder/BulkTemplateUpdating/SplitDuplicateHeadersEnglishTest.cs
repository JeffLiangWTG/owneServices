using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.Testing
{
	[TestedType(typeof(SplitDuplicateHeadersEnglish))]
	sealed class SplitDuplicateHeadersEnglishTest : UpdateTemplateCommandTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SplitDuplicateHeadersEnglish(Factory);
		}
	}
}
