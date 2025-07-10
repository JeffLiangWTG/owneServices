using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class WarehouseOrderWrapperForManifest : WarehouseOrderWrapper, ILoadingSupport
	{
		public WarehouseOrderWrapperForManifest(WhsOrder order, BusinessObjectFactory factoryToWrap)
			: base(order, factoryToWrap)
		{
		}

		#region Properties

		#region ILoadingSupport

		ZGuid ILoadingSupport.LoadPK
		{
			get => LoadPK;
			set => LoadPK = value;
		}

		ZGuid LoadPK;

		ZString ILoadingSupport.CommonLoadVolumeUQ
		{
			get => CommonLoadVolumeUQ;
			set => CommonLoadVolumeUQ = value;
		}

		ZString CommonLoadVolumeUQ;

		ZString ILoadingSupport.CommonLoadWeightUQ
		{
			get => CommonLoadWeightUQ;
			set => CommonLoadWeightUQ = value;
		}

		ZString CommonLoadWeightUQ;

		#endregion

		#region TotalLoadedPackages

		protected override LabelValuePairWrapper TotalLoadedPackagesCore
			=> new LabelValuePairWrapper(
				Res.GetString("7472b1a4-541e-46e3-9548-7f27d746f27f", "Loaded Packages:"),
				LoadedPackages.Count,
				Factory);

		#endregion

		#region TotalLoadedUnits

		protected override LabelValuePairWrapper TotalLoadedUnitsCore
			=> new LabelValuePairWrapper(
				Res.GetString("4eb79e22-8dd3-4839-8a77-f35d4b690abe", "Loaded Units:"),
				LoadedPackages?.Cast<PackageWrapper>().Sum(d => d.PackedItemCount),
				Factory);

		#endregion

		#region TotalLoadedWeightCore

		protected override WeightWrapper TotalLoadedWeightCore
		{
			get
			{
				var result = WeightWrapper.Empty;
				if (Constants.Weight.ContainsCode(CommonLoadWeightUQ))
				{
					var groupedPackages = LoadedPkgPackages?
					.GroupBy(p => p.KP_WeightUQ)
					.Select(gp => (WeightUQ: gp.Key, WeightTotal: gp.Sum(p => p.KP_Weight)));

					result =
						groupedPackages.Any()
							? new WeightWrapper(
									groupedPackages.Sum(p => Constants.Weight.Convert(p.WeightTotal, p.WeightUQ, CommonLoadWeightUQ)),
									CommonLoadWeightUQ.ToString(),
									2,
									Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight),
									Factory
								)
							: WeightWrapper.Empty;
				}
				return result;
			}
		}

		#endregion

		#region TotalLoadedVolume

		protected override VolumeWrapper TotalLoadedVolumeCore
		{
			get
			{
				var result = VolumeWrapper.Empty;
				if (Constants.Volume.ContainsCode(CommonLoadVolumeUQ))
				{
					var groupedPackages = LoadedPkgPackages?
					.GroupBy(p => p.KP_VolumeUQ)
					.Select(gp => (VolumeUQ: gp.Key, VolumeTotal: gp.Sum(p => p.KP_Volume)));

					result =
						groupedPackages.Any()
							? new VolumeWrapper(
									groupedPackages.Sum(p => Constants.Volume.Convert(p.VolumeTotal, p.VolumeUQ, CommonLoadVolumeUQ)),
									CommonLoadVolumeUQ.ToString(),
									Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume),
									Factory
								)
							: VolumeWrapper.Empty;
				}
				return result;
			}
		}

		PkgPackageCollection LoadedPkgPackages
		{
			get
			{
				if (loadedPkgPackages == null)
				{
					var packages = Order.PackageJob?.Packages;
					if (packages != null)
					{
						packages.AdditionalFilter = LoadedPackagesQuery();
						loadedPkgPackages = packages;
					}
				}

				return loadedPkgPackages;
			}
		}
		PkgPackageCollection loadedPkgPackages;

		ZQuery LoadedPackagesQuery()
		{
			var pivotQuery = new ZDBOnlySubQuery(typeof(WhsLoadPkgPackagePivot), WhsLoadPkgPackagePivotSchema.WLP_KP_Package);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_WLO_Load, LoadPK);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_LoadedTime, SQLComparisonOperator.NotEqual, null);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);

			var query = new ZDBOnlyQuery(typeof(PkgPackage));
			query.AddSubQuery(pivotQuery, JoinCondition.And);

			return query;
		}

		#endregion

		#endregion

		#region Packages

		protected override PackageWrapperCollection GetPackages()
			=> new PackageWrapperCollection(GetPackageJob(isLoaded: false), Factory);

		protected override PackageWrapperCollection GetLoadedPackages()
			=> new PackageWrapperCollectionForLoadManifest(GetPackageJob(isLoaded: true), Factory);

		PkgPackageJob GetPackageJob(bool isLoaded)
		{
			if (Order?.PackageJob is var packageJob && packageJob != null)
			{
				packageJob.Packages.AdditionalFilter = isLoaded ? LoadedPackagesQuery() : new ZQuery();
			}

			return packageJob;
		}

		#endregion
	}
}
