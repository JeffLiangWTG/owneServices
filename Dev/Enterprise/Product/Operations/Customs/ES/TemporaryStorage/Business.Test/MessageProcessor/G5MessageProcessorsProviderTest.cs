using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.Testing;

sealed class G5MessageProcessorsProviderTest : TestCaseWithFactory
{
	public void TestDefaultMessageProcessors()
	{
		var logger = new LoggingInformation();
		var result = new G5MessageProcessorsProvider().GetProcessors(logger)
			.Select(processor => processor.GetType());

		AssertContainsExactElementsInAnyOrder(
			new[]
			{
				typeof(ExpeditionG5ResponseMessageProcessor),
				typeof(ExpAmendmentG5ResponseMessageProcessor),
				typeof(ReceptionG5ResponseMessageProcessor),
				typeof(ExpCancelG5ResponseMessageProcessor),
				typeof(G5ClearanceEmailResponseMessageProcessor),
			},
			result);
	}
}
