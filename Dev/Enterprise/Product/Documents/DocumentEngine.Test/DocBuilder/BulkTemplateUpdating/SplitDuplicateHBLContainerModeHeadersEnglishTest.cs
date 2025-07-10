using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.Testing
{
	[TestedType(typeof(SplitDuplicateHBLContainerModeHeadersEnglish))]
	sealed class SplitDuplicateHBLContainerModeHeadersEnglishTest : UpdateTemplateCommandTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SplitDuplicateHBLContainerModeHeadersEnglish(Factory);
		}
	}
}
