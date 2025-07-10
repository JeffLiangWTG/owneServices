using CargoWise.Types;
using Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureInternalPackagesInfoWrapper : NctsDepartureInternalPackagesInfoCommonWrapper, IDepartureInternalPackagesInfo
	{
		public DepartureInternalPackagesInfoWrapper(NctsDepartureCargoDesc goodsItem) : base(goodsItem)
		{
		}

		public ZBool IsVehiclePackage => goodsItem.IsVehicles;
		protected override bool OnlyOneVehiclePackage => true;
	}
}
