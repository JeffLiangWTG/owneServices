using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class TIRInternalPackagesInfoWrapper : NctsDepartureInternalPackagesInfoCommonWrapper, ITIRInternalPackagesInfo
	{
		public TIRInternalPackagesInfoWrapper(NctsDepartureCargoDesc goodsItem) : base(goodsItem)
		{
		}

		public ZBool IsVehiclePackage => goodsItem.IsVehicles;
	}
}
