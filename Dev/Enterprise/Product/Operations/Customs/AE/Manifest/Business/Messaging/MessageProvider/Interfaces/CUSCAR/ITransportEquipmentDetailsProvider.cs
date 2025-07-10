namespace Enterprise.Customs.AE.Manifest.Business;

public interface ITransportEquipmentDetailsProvider
{
	string EquipmentIdentifier { get; }

	string EquipmentType { get; }

	string EquipmentIndicator { get; }
}
