namespace Enterprise.Customs.CA.Services
{
	public interface IAIRSIIDValidationQueriedLine : IAIRSValidationQueriedLine
	{
		string DeliveryPartyProvince { get; }
	}
}
