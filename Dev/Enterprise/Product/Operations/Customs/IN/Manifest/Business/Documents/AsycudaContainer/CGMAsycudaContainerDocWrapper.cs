using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.IN.Manifest.Business;

sealed class CGMAsycudaContainerDocWrapper : DocumentWrapper
{
	public CGMAsycudaContainerDocWrapper(CGMAsycudaPack pack)
	{
		Argument.NotNull(pack, nameof(pack));
		Container = Argument.NotNull(pack.Container, nameof(pack.Container));
		Bill = Argument.NotNull(pack.Bill, nameof(pack.Bill));
	}

	CGMAsycudaContainer Container { get; }

	CGMAsycudaBill Bill { get; }

	public ZString LineNumber => Container.Header?.AMA_CarrierReference ?? ZString.Empty;

	public ZInt SubLineNumber => Bill.ABL_SequenceNumber;

	public ZString ContainerNumber => Container.ACN_ContainerNumber;

	public ZString ContainerSealNumber => Container.ACN_Seal1;

	public ZString ContainerAgentCode => Container.ContainerAgentPAN;

	public ZString ContainerStatus => Container.ACN_EmptyFullIndicator;

	public ZInt TotalPackages => Container.ACN_NumberOfPackages;

	public ZDecimal ContainerWeight => Container.ACN_GoodsWeight;

	public ZString ISOCode => "TBA";

	public ZString SOCFlag => Container.ACN_IsShipperOwned ? Yes : No;

	const string Yes = "Yes, Its Shippers own container";
	const string No = "No";
}
