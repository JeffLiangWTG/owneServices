using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperFromTransitPackageHeader : PackageWrapperFromPkgPackageHeader
	{
		protected PackageWrapperFromTransitPackageHeader(PkgPackage package, BusinessObjectFactory factory)
			: base(package, factory)
		{
		}

		public PackageWrapperFromTransitPackageHeader(PkgPackageJob packageJob, PkgPackageHeader packageHeaderBO, BusinessObjectFactory factory)
			: base(packageJob, packageHeaderBO, factory)
		{
		}

		#region GetOutterPackagesCount

		protected override ZShort GetOutterPackagesCount()
		{
			return outterPackagesCount ?? base.GetOutterPackagesCount();
		}

		ZShort? outterPackagesCount;

		internal void SetOutterPackagesCount(ZShort count)
		{
			this.outterPackagesCount = count;
		}

		#endregion

		#region GetPackageState

		protected override PackageStateWrapper GetPackageState()
		{
			return new PackageStateWrapper(PackageStateBO, Factory);
		}

		WhsItemPackageState PackageStateBO { get { return packageState ?? (packageState = Factory.GetNull<WhsItemPackageState>()); } }

		WhsItemPackageState packageState;

		#endregion

		#region GetHandlingUnit

		protected override PackageWrapper GetHandlingUnit()
		{
			PkgPackage pkgHandlingUnit = Factory.GetNull<PkgPackage>();
			if (package != null)
			{
				var divotQuery = new ZQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, package.PK);
				divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);
				var divot = Factory.LoadTop1<PkgPackageHandlingUnitDivot>(divotQuery);
				if (divot != null)
				{
					var huQuery = new ZQuery(PkgPackageSchema.PK, divot.KPD_KP_HandlingUnit);
					pkgHandlingUnit = Factory.LoadTop1<PkgPackage>(huQuery);
				}
			}
			return new PackageWrapperFromTransitPackage(pkgHandlingUnit, Factory);
		}

		#endregion

		#region GetParent

		protected override FreightWrapper GetParent()
		{
			if (package != null)
			{
				var wrappers = FreightWrapper.New(package.PackageJob, Factory);
				var wrapper = wrappers.Length > 0 ? wrappers[0] : null;
				var iDocTypeCode = wrapper as IDocTypeCode;
				if (iDocTypeCode != null)
				{
					iDocTypeCode.DocTypeCode = "PPL";
				}

				return wrapper;
			}
			return base.GetParent();
		}

		#endregion

		#region GetTopLevelHandlingUnit

		protected override PackageWrapper GetTopLevelHandlingUnit()
		{
			var topLevelHandlingUnit = Factory.GetNull<PkgPackage>();
			if (package != null && !package.KP_KP_TopHandlingUnitPackage.IsEmpty)
			{
				var query = new ZQuery(PkgPackageSchema.PK, package.KP_KP_TopHandlingUnitPackage);
				topLevelHandlingUnit = Factory.LoadTop1<PkgPackage>(query);
			}
			return new PackageWrapperFromTransitPackage(topLevelHandlingUnit, Factory);
		}

		#endregion

		#region Package

		public void SetPKGPackage(ZGuid pkgPackagePK)
		{
			this.packageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, pkgPackagePK));
			this.package = Factory.Load<PkgPackage>(pkgPackagePK);
		}

		PkgPackage package;

		#endregion
	}
}
