using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.Testing
{
	[TestedType(typeof(SplitDuplicateShortModeDepartureArrivalHeadersEnglish))]
	sealed class SplitDuplicateShortModeDepartureArrivalHeadersEnglishTest : UpdateTemplateCommandTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SplitDuplicateShortModeDepartureArrivalHeadersEnglish(Factory);
		}
	}
}
