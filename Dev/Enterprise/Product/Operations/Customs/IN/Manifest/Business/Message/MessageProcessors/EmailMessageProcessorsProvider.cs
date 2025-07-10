using Enterprise.Customs.IN.Business;

namespace Enterprise.Customs.IN.Manifest.Business;

public class EmailMessageProcessorsProvider : IEmailMessageProcessorsProvider
{
	public IEmailMessageProcessor[] GetMessageProcessors() =>
	[
		new AirCgmCHCMI01MessageProcessor(),
		new AirCgmCHCMI02MessageProcessor(),
		new SeaCgmCHCMI21AMessageProcessor(),
	];
}
