namespace Enterprise.Customs.AE.Manifest.Business;

public interface ITransportServiceRequirementsProvider
{
	string ServiceRequirementCode { get; }

	string CargoType { get; }
}
