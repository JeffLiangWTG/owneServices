using CargoWise.Common;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.AE.Manifest.Business;

sealed class TransportEquipmentInfoProvider : ITransportEquipmentInfoProvider
{
	public TransportEquipmentInfoProvider(AsycudaContainer container, AsycudaBill bill)
	{
		Container = Argument.NotNull(container, nameof(container));
		Bill = Argument.NotNull(bill, nameof(bill));
	}
	AsycudaContainer Container { get; }
	AsycudaBill Bill { get; }

	public ITransportEquipmentDetailsProvider ContainerDetails => containerDetails ??= new TransportEquipmentDetailsProvider(Container);
	ITransportEquipmentDetailsProvider containerDetails;

	public ITransportServiceRequirementsProvider ServiceRequirements => serviceRequirements ??= GetTransportServiceRequirements();
	ITransportServiceRequirementsProvider serviceRequirements;

	public decimal GoodsWeightInKgs => goodsWeightInKgs ??= Container.ACN_GoodsWeightInKilos;
	decimal? goodsWeightInKgs;

	public string SealNumber => sealNumber ??= Container.ACN_Seal1;
	string sealNumber;

	public ITemperatureDetailsProvider Temperature => temperature ??= GetTemperatureDetails();
	ITemperatureDetailsProvider temperature;

	TemperatureDetailsProvider GetTemperatureDetails()
	{
		return Container.ContainerType is { } containerType && containerType.RC_ContainerType == ContainerTypes.Refrigerated
			? new TemperatureDetailsProvider(Container)
			: null;
	}

	TransportServiceRequirementsProvider GetTransportServiceRequirements()
	{
		return Bill.ABL_SpecialCargoCode.IsEmpty && Bill.ABL_CargoType.IsEmpty
			? null
			: new TransportServiceRequirementsProvider(Bill);
	}
}
