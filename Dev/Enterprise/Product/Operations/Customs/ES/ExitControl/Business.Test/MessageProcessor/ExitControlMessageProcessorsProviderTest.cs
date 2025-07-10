using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.BatchProcessor;

namespace Enterprise.Customs.ES.ExitControl.Business.Testing
{
	sealed class ExitControlMessageProcessorsProviderTest : TestCaseWithFactory
	{
		public void TestDefaultMessageProcessors()
		{
			var logger = new LoggingInformation();
			var result = new ExitControlMessageProcessorsProvider().GetProcessors(logger)
				.Select(processor => processor.GetType());

			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					typeof(EALInboxNotificationExitNonConformityAESResponseMessageProcessor),
					typeof(EALInboxNotificationExitClearanceAESResponseMessageProcessor),
					typeof(EALAESResponseMessageProcessor),
				},
				result);
		}
	}
}
