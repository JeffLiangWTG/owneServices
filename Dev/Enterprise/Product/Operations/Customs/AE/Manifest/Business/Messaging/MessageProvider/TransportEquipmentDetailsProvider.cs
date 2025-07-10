using CargoWise.Common;
using Enterprise.Customs.ASYCUDA.Business;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class TransportEquipmentDetailsProvider : ITransportEquipmentDetailsProvider
{
	public TransportEquipmentDetailsProvider(AsycudaContainer container)
	{
		Container = Argument.NotNull(container, nameof(container));
	}
	
	AsycudaContainer Container { get; }

	public string EquipmentIdentifier => equipmentIndentifier ??= Container.ACN_ContainerNumber;
	string equipmentIndentifier;

	public string EquipmentType => equipmentType ??= Container.ContainerType?.RC_Code ?? CargoWise.Types.ZString.Empty;
	string equipmentType;

	public string EquipmentIndicator => equipmentIndicator ??= GetEquipmentIndicator();
	string equipmentIndicator;

	public string GetEquipmentIndicator()
	{
		return (string)Container.ACN_EmptyFullIndicator switch
		{
			EmptyFullIndicatorList.Codes.EmptyContainer => "4",
			EmptyFullIndicatorList.Codes.FullContainerLoad => "5",
			EmptyFullIndicatorList.Codes.LessThanFullContainerLoad => "7",
			_ => null
		};
	}
}
