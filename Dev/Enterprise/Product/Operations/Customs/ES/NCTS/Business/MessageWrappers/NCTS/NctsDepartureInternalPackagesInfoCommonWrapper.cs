using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.NCTS.Business.MessageWrappers
{
	public class NctsDepartureInternalPackagesInfoCommonWrapper : IInternalPackagesInfoCommon
	{
		public NctsDepartureInternalPackagesInfoCommonWrapper(NctsDepartureCargoDesc goodsItem)
		{
			this.goodsItem = Argument.NotNull(goodsItem, nameof(goodsItem));
		}
		protected readonly NctsDepartureCargoDesc goodsItem;

		public IReadOnlyCollection<IInternalPackageIdentificationCommon> Packages => packages ?? (packages = GetInternalPackageList(goodsItem));
		IReadOnlyCollection<InternalPackageIdentificationCommonWrapper> packages;

		protected virtual bool OnlyOneVehiclePackage => false;

		IReadOnlyCollection<InternalPackageIdentificationCommonWrapper> GetInternalPackageList(NctsDepartureCargoDesc goodsItem)
		{
			var packages = new List<InternalPackageIdentificationCommonWrapper>();

			if (goodsItem.IsVehicles)
			{
				if (OnlyOneVehiclePackage)
				{
					packages.Add(new InternalPackageIdentificationCommonWrapper(VehiclePackageMark, RefCusCodeList.PackageType.Frame, goodsItem.Packages.Count));
				}
				else
				{
					packages.AddRange(goodsItem.Packages.Cast<NctsPackage>().Select(pack => new InternalPackageIdentificationCommonWrapper(pack.B5_PackageID, RefCusCodeList.PackageType.Frame, goodsItem.Packages.Count)));
				}
			}
			else
			{
				packages.AddRange(goodsItem.Packages.Cast<NctsPackage>().Select(pack => new InternalPackageIdentificationCommonWrapper(pack.B5_MarksAndNumbers, pack.B5_UnitType, pack.B5_UnitCount)));
			}
			return packages.AsReadOnly();
		}

		const string VehiclePackageMark = "BASTIDORES";
	}
}
