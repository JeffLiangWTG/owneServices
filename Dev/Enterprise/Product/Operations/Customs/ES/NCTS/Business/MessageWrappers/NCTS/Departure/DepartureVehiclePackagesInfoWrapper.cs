using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class DepartureVehiclePackagesInfoWrapper : IVehiclePackagesInfoCommon
	{
		public DepartureVehiclePackagesInfoWrapper(NctsDepartureCargoDesc goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}

		readonly NctsDepartureCargoDesc goodsItem;

		public IReadOnlyCollection<IVehicleCommon> Packages
		{
			get
			{
				if (packages == null)
				{
					packages = goodsItem.Packages
						.Cast<NctsPackage>()
						.Where(x => !x.B5_PackageID.IsEmpty || !x.B5_Brand.IsEmpty || !x.B5_Model.IsEmpty)
						.Select(pack => new VehicleCommonWrapper(pack.B5_PackageID, pack.B5_Brand, pack.B5_Model))
						.ToList().AsReadOnly();
				}
				return packages;
			}
		}
		IReadOnlyCollection<IVehicleCommon> packages;
	}
}
