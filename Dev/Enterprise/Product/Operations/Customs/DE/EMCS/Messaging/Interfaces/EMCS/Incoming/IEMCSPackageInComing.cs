using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Types;

namespace Enterprise.Customs.DE.EMCS.Messaging
{
	public interface IEMCSPackageInComing : IInboundProvider
	{
		ZString KindOfPackages { get; }
		ZLong NumberOfPackages { get; }
		ZString SealNumber { get; }
		ZString SealInformation { get; }
		ZString ShippingMarks { get; }
		ZBool IsNumberOfPackagesProvided { get; }
	}
}
