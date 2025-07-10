namespace Enterprise.Customs.CA.Services
{
	public interface IAIRSOGDValidationQueriedLine : IAIRSValidationQueriedLine
	{
		string RequirementId { get; }
		string RequirementVersion { get; }
		string DestinationProvince { get; }
	}
}
