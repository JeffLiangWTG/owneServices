using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	sealed class G3MessageProcessorsProviderTest : TestCaseWithFactory
	{
		public void TestGetProcessors()
		{
			var logger = new LoggingInformation();
			var result = new G3MessageProcessorsProvider().GetProcessors(logger)
				.Select(processor => processor.GetType());

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(G3DeclarationResponseMessageProcessor),
					typeof(G3RevokeResponseMessageProcessor),
				},
				result);
		}
	}
}
