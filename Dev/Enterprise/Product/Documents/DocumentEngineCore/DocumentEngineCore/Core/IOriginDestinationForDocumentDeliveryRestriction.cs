namespace Enterprise.DocumentEngineCore
{
	public interface IOriginDestinationForDocumentDeliveryRestriction
	{
		string OriginCountryCode { get; }
		string DestinationCountryCode { get; }
	}
}
