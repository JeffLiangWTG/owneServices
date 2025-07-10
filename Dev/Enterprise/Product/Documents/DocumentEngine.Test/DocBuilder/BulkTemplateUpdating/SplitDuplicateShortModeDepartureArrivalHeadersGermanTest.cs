using CargoWise.EntityFramework;
using NUnit.Framework;
using static Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.DocBuilderTemplateUpdater;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.Testing
{
	[TestedType(typeof(SplitDuplicateShortModeDepartureArrivalHeadersGerman))]
	sealed class SplitDuplicateShortModeDepartureArrivalHeadersGermanTest : UpdateTemplateCommandTest
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new SplitDuplicateShortModeDepartureArrivalHeadersGerman(Factory);
		}
	}
}
