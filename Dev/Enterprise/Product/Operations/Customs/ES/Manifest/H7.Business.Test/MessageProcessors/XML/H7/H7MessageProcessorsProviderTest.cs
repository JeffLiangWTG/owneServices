using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class H7MessageProcessorsProviderTest : TestCaseWithFactory
	{
		public void TestGetProcessors()
		{
			var logger = new LoggingInformation();
			var result = new H7MessageProcessorsProvider().GetProcessors(logger)
				.Select(processor => processor.GetType());

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(DeclarationH7ResponseMessageProcessor),
					typeof(AnnexH7ResponseMessageProcessor),
					typeof(ReexportH7ResponseMessageProcessor),
					typeof(CancellationH7ResponseMessageProcessor),
					typeof(QueryH7ResponseMessageProcessor),
				},
				result);
		}
	}
}
