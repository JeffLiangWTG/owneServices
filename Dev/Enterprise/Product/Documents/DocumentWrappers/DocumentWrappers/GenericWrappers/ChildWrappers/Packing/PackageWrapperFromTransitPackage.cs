using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transit.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperFromTransitPackage : PackageWrapperFromPkgPackage
	{
		public PackageWrapperFromTransitPackage(PkgPackage packageBO, BusinessObjectFactory factory)
			: this(packageBO, factory, 0, 0)
		{
		}

		public PackageWrapperFromTransitPackage(PkgPackage packageBO, BusinessObjectFactory factory, ZInt displayOrder, ZInt indent)
			: base(packageBO, factory, displayOrder, indent)
		{
			if (packageBO == null || packageBO.IsNull)
			{
				PackageStateBO = factory.GetNull<WhsItemPackageState>();
			}
			else
			{
				var query = new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, PackageBO.PK);
				var relatedPackageState = Factory.Load<WhsItemPackageState>(query).FirstOrDefault();
				if (relatedPackageState != null)
				{
					PackageStateBO = relatedPackageState;
				}
			}
		}

		readonly WhsItemPackageState PackageStateBO;

		#region GetOutturnedPackages

		protected override PackQTYWrapper GetOutturnedPackages()
		{
			if (PackageStateBO != null && !PackageStateBO.WPS_WRH_TransitReceiveHeader.IsEmpty)
			{
				return new PackQTYWrapper(1, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
			}

			return new PackQTYWrapper(0, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

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

		#region GetInnerPackagesCount

		protected override ZShort GetInnerPackagesCount()
		{
			if (PackageBO != null && PackageStateBO != null && PackageBO.KP_KP_TopHandlingUnitPackage.IsEmpty && PackageStateBO.WPS_UnitType != PackageStateUnitType.Codes.HandlingUnit)
			{
				var innerPackagesCount = ZShort.Zero;
				var query = new ZQuery(PkgPackageSchema.KP_KP_ParentPackage, PackageBO.PK);
				var packages = Factory.Load<PkgPackage>(query).ToList();
				foreach (var package in packages)
				{
					innerPackagesCount += (ZShort)package.KP_PackageQty;
				}

				return innerPackagesCount;
			}
			return ZShort.Zero;
		}

		#endregion

		#region GetOutturnedWeight

		protected override WeightWrapper GetOutturnedWeight()
		{
			if (PackageStateBO != null && !PackageStateBO.WPS_WRH_TransitReceiveHeader.IsEmpty)
			{
				return new WeightWrapper(PackageBO.KP_Weight, PackageBO.KP_WeightUQ, WeightWrapper.StandardDecimalPlaces, PackageBO.Lookups.WeightUQs, Factory);
			}

			return new WeightWrapper(0, PackageBO.KP_WeightUQ, WeightWrapper.StandardDecimalPlaces, PackageBO.Lookups.WeightUQs, Factory);
		}

		#endregion

		#region GetOutturnedVolume

		protected override VolumeWrapper GetOutturnedVolume()
		{
			if (PackageStateBO != null && !PackageStateBO.WPS_WRH_TransitReceiveHeader.IsEmpty)
			{
				return new VolumeWrapper(PackageBO.KP_Volume, PackageBO.KP_VolumeUQ, PackageBO.Lookups.VolumeUQs, Factory);
			}

			return new VolumeWrapper(0, PackageBO.KP_VolumeUQ, PackageBO.Lookups.VolumeUQs, Factory);
		}

		#endregion

		#region GetHandlingUnit

		protected override PackageWrapper GetHandlingUnit()
		{
			PkgPackage pkgHandlingUnit = Factory.GetNull<PkgPackage>();
			if (PackageBO != null)
			{
				var divotQuery = new ZQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, PackageBO.PK);
				divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);
				var divot = Factory.Load<PkgPackageHandlingUnitDivot>(divotQuery).FirstOrDefault();
				if (divot != null)
				{
					var huQuery = new ZQuery(PkgPackageSchema.PK, divot.KPD_KP_HandlingUnit);
					pkgHandlingUnit = Factory.Load<PkgPackage>(huQuery).FirstOrDefault();
				}
			}
			return new PackageWrapperFromTransitPackage(pkgHandlingUnit, Factory);
		}

		#endregion

		#region GetTopLevelHandlingUnit

		protected override PackageWrapper GetTopLevelHandlingUnit()
		{
			PkgPackage pkgTopLevelHandlingUnit = Factory.GetNull<PkgPackage>();
			if (PackageBO != null && !PackageBO.KP_KP_TopHandlingUnitPackage.IsEmpty)
			{
				var query = new ZQuery(PkgPackageSchema.PK, PackageBO.KP_KP_TopHandlingUnitPackage);
				pkgTopLevelHandlingUnit = Factory.Load<PkgPackage>(query).FirstOrDefault();
			}
			return new PackageWrapperFromTransitPackage(pkgTopLevelHandlingUnit, Factory);
		}

		#endregion

		#region PackedPackages

		protected override PackageWrapperCollection GetPackedPackages()
		{
			if (PackageBO != null && PackageStateBO != null)
			{
				if (PackageStateBO.WPS_IsHandlingUnit)
				{
					return new TransitPackageWrapperCollection(PackageBO.HandlingUnitPackedPackages, PackageWrapperCollection.PackLevel.All, Factory, pkg => pkg.HandlingUnitPackedPackages.Union(pkg.Packages));
				}
				else
				{
					return new TransitPackageWrapperCollection(PackageBO.Packages, PackageWrapperCollection.PackLevel.All, Factory, pkg => pkg.Packages);
				}
			}
			return base.GetPackedPackages();
		}

		#endregion

		#region FirstLevelPackedPackages

		protected override PackageWrapperCollection GetFirstLevelPackedPackages()
		{
			if (PackageBO != null && PackageStateBO != null)
			{
				if (PackageStateBO.WPS_IsHandlingUnit)
				{
					return new TransitPackageWrapperCollection(PackageBO.HandlingUnitPackedPackages, PackageWrapperCollection.PackLevel.First, Factory, pkg => pkg.HandlingUnitPackedPackages.Union(pkg.Packages));
				}
				else
				{
					return new TransitPackageWrapperCollection(PackageBO.Packages, PackageWrapperCollection.PackLevel.First, Factory, pkg => pkg.Packages);
				}
			}
			return base.GetFirstLevelPackedPackages();
		}

		#endregion

		#region IsOuterPackage

		protected override ZBool GetIsOuterPackage()
		{
			if (PackageBO != null && PackageStateBO != null)
			{
				return PackageStateBO.WPS_UnitType != PackageStateUnitType.Codes.HandlingUnit && PackageBO.KP_KP_ParentPackage.IsEmpty ? ZBool.True : ZBool.False;
			}
			return ZBool.False;
		}

		#endregion
	}
}
