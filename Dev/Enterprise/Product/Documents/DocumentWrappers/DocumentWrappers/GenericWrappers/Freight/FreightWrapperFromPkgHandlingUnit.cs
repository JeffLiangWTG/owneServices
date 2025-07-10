using System;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class FreightWrapperFromPkgHandlingUnit : FreightWrapper, IPackageOverrider
	{
		protected FreightWrapperFromPkgHandlingUnit(BusinessObject businessObjectToWrap, BusinessObjectFactory factory) : base(businessObjectToWrap, factory)
		{
		}
		public FreightWrapperFromPkgHandlingUnit(PkgHandlingUnit handlingUnit, BusinessObjectFactory factory)
			: base(handlingUnit, factory)
		{
			HandlingUnit = handlingUnit ?? Factory.GetNull<PkgHandlingUnit>();
		}

		readonly PkgHandlingUnit HandlingUnit;

		#region GetPackages

		protected override PackageWrapperCollection GetPackages()
		{
			return overriddenPackages ?? (overriddenPackages = new PackageWrapperCollection(PackageJob, Factory));
		}
		PackageWrapperCollection overriddenPackages;

		#endregion

		#region PackageJob

		PkgPackageJob PackageJob
		{
			get { return packageJob ?? (packageJob = PkgPackageJob.LoadOrCreatePackageJobWithNoChanges(HandlingUnit)); }
		}
		PkgPackageJob packageJob;

		#endregion

		#region IPackageOverrider

		void IPackageOverrider.SetPackageOverride(PkgPackage package, PkgPackageItemDivotsWrapper[] packedItems, int documentNumber, int documentTotal, int uomTypeNumber, int uomTypeTotal)
		{
			overriddenPackages = new PackageWrapperCollection(Factory);
			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			overriddenPackages.Add(packageWrapper);
		}

		void IPackageOverrider.SetPackageCollectionOverride(PkgPackage[] packages, PkgPackageHeader[] packageHeaders)
		{
			if (packages != null && packages.Length > 0)
			{
				throw new ArgumentException("Not Support this Parameter", nameof(packages));
			}

			if (packageHeaders != null && packageHeaders.Length > 0)
			{
				if (packageHeaders.Length > 1)
				{
					throw new ArgumentException("Support only one Package Headerthis", nameof(packageHeaders));
				}
				overriddenPackages = new PackageWrapperCollection(Factory);
				var packageWrapper = new PackageWrapperFromPkgPackageHeader(packageHeaders[0].CurrentPackageJob, packageHeaders[0], Factory);
				overriddenPackages.Add(packageWrapper);
			}
		}

		#endregion
	}
}
