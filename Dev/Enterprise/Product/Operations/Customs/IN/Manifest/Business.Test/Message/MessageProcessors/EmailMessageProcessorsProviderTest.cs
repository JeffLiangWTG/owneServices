using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IN.Manifest.Business.Testing;

sealed class EmailMessageProcessorsProviderTest : TestCaseWithFactory
{
	public void TestGetMessageProcessor()
	{
		var messageProcessors = new EmailMessageProcessorsProvider().GetMessageProcessors();
		AssertContainsExactElementsInExactOrder(
		[
			typeof(AirCgmCHCMI01MessageProcessor),
			typeof(AirCgmCHCMI02MessageProcessor),
			typeof(SeaCgmCHCMI21AMessageProcessor),
		], messageProcessors.Select(x => x.GetType()));
	}
}
