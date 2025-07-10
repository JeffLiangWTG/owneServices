using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class TransitPackageWrapperCollection : PackageWrapperCollection
	{
		public TransitPackageWrapperCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public TransitPackageWrapperCollection(IEnumerable<PkgPackage> packages, PackLevel packLevel, BusinessObjectFactory factory)
			: base(factory)
		{
			AddPackages(packages, packLevel, 0);
		}

		public TransitPackageWrapperCollection(IEnumerable<PkgPackage> packages, PackLevel packLevel, BusinessObjectFactory factory, Func<PkgPackage, IEnumerable<PkgPackage>> childrenPackagesSelector)
			: base(factory)
		{
			if (childrenPackagesSelector != null)
			{
				this.childrenPackagesSelector = childrenPackagesSelector;
			}
			AddPackages(packages, packLevel, 0);
		}

		void AddPackages(IEnumerable<PkgPackage> packages, PackLevel levelWanted, int currentLevel)
		{
			int order = 0;
			foreach (var package in packages)
			{
				Add(new PackageWrapperFromTransitPackage(package, Factory, order++, currentLevel));

				if (currentLevel != (int)levelWanted)
				{
					AddPackages(childrenPackagesSelector(package), levelWanted, currentLevel + 1);
				}
			}
		}

		readonly Func<PkgPackage, IEnumerable<PkgPackage>> childrenPackagesSelector = pkg => pkg.Packages;
	}
}
