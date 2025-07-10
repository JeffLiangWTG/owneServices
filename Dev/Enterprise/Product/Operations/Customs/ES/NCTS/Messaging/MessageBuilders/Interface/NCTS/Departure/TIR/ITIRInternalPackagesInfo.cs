using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders
{
	public interface ITIRInternalPackagesInfo : IInternalPackagesInfoCommon
	{
		ZBool IsVehiclePackage { get; }
	}
}
