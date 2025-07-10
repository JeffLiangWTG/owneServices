namespace Enterprise.Integration
{
	public interface IMessagingContext
	{
		IEDICommunicationPartyConfig CurrentInboundConfig { get; set; }
		void ResetCurrentInboundConfig();
	}
}
