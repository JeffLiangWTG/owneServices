using CargoWise.Common;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class TransportServiceRequirementsProvider : ITransportServiceRequirementsProvider
{
	public TransportServiceRequirementsProvider(AsycudaBill bill)
	{
		Bill = Argument.NotNull(bill, nameof(bill));
	}
	AsycudaBill Bill { get; }

	public string ServiceRequirementCode => serviceRequirementCode ??= Bill.ABL_SpecialCargoCode;
	string serviceRequirementCode;

	public string CargoType => cargoType ??= Bill.ABL_CargoType;
	string cargoType;
}
