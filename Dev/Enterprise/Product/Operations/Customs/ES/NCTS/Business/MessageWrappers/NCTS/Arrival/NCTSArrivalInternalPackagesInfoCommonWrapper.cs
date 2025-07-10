using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NctsArrivalInternalPackagesInfoCommonWrapper : IInternalPackagesInfoCommon
	{
		public NctsArrivalInternalPackagesInfoCommonWrapper(NctsArrivalAndUnloadingCargoDesc goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		readonly NctsArrivalAndUnloadingCargoDesc goodsItem;

		public IReadOnlyCollection<IInternalPackageIdentificationCommon> Packages
		{
			get
			{
				if (packages == null)
				{
					packages = goodsItem.Packages
						.Cast<EU.NCTS.Business.NctsPackage>()
						.Select(pack => new InternalPackageIdentificationCommonWrapper(pack.B5_MarksAndNumbers, pack.B5_UnitType, pack.B5_UnitCount))
						.ToList().AsReadOnly();
				}
				return packages;
			}
		}
		IReadOnlyCollection<InternalPackageIdentificationCommonWrapper> packages;
	}
}
