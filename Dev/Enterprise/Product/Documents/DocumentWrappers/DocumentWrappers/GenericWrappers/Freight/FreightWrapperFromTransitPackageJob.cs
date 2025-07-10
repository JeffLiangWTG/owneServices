using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class FreightWrapperFromTransitPackageJob : FreightWrapper
	{
		protected FreightWrapperFromTransitPackageJob(BusinessObject businessObjectToWrap, BusinessObjectFactory factory) : base(businessObjectToWrap, factory)
		{
		}

		protected PackageWrapperCollection CreatePackageWrapperCollection(BusinessObjectFactory factory, PkgPackage[] packages, PkgPackageHeader[] packageHeaders, ZShort? outPackageCount = null)
		{
			var headerItmes = packageHeaders == null ? null : packageHeaders.ToDictionary(p => p, p => (ZGuid?)null);
			return CreatePackageWrapperCollection(factory, packages, headerItmes, outPackageCount);
		}

		protected PackageWrapperCollection CreatePackageWrapperCollection(BusinessObjectFactory factory, PkgPackage[] packages, Dictionary<PkgPackageHeader, ZGuid?> packageHeaderWithPackagePK, ZShort? outPackageCount = null)
		{
			var overriddenPackages = new PackageWrapperCollection(factory);
			if (packages != null)
			{
				foreach (var package in packages)
				{
					var packageWrapper = new PackageWrapperFromTransitPackage(package, factory);
					if (outPackageCount != null)
					{
						packageWrapper.SetOutterPackagesCount(outPackageCount.Value);
					}
					overriddenPackages.Add(packageWrapper);
				}
			}

			if (packageHeaderWithPackagePK != null)
			{
				foreach (var headerItem in packageHeaderWithPackagePK)
				{
					var header = headerItem.Key;
					var packageHeaderWrapper = new PackageWrapperFromTransitPackageHeader(header.CurrentPackageJob, header, factory);

					if (headerItem.Value.HasValue)
					{
						packageHeaderWrapper.SetPKGPackage(headerItem.Value.Value);
					}

					if (outPackageCount != null)
					{
						packageHeaderWrapper.SetOutterPackagesCount(outPackageCount.Value);
					}

					overriddenPackages.Add(packageHeaderWrapper);
				}
			}
			return overriddenPackages;
		}
	}
}
