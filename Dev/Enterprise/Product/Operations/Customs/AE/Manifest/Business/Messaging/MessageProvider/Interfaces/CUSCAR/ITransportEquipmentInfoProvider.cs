namespace Enterprise.Customs.AE.Manifest.Business;

public interface ITransportEquipmentInfoProvider
{
	ITransportEquipmentDetailsProvider ContainerDetails { get; }

	ITransportServiceRequirementsProvider ServiceRequirements { get; }

	decimal GoodsWeightInKgs { get; }

	string SealNumber { get; }

	ITemperatureDetailsProvider Temperature { get; }
}
