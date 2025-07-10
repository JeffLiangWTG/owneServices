using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperForLoadManifest : PackageWrapperFromPkgPackage
	{
		public PackageWrapperForLoadManifest(PkgPackage packageBO, BusinessObjectFactory factory)
			: base(packageBO, System.Array.Empty<PkgPackageItemDivotsWrapper>(), factory, 0, 0)
		{
		}

		#region GetPackedItemCount

		/// <summary>
		/// Unfortunate naming of 'PackedItemCount', this is actually the Packed Qty of PackableItems in the Package.
		/// </summary>
		protected override ZInt GetPackedItemCount()
		{
			return (ZInt)GetPackedQuantity(PackageBO);
		}

		decimal GetPackedQuantity(PkgPackage package)
		{
			var result = 0m;
			if (package.KP_KJ_ParentPackageJob.IsValid
				&& package.KP_KPH_PackageHeader.IsValid)
			{
				foreach (var inner in package.Packages)
				{
					result += GetPackedQuantity(inner);
				}
			}
			return result + package.PackedItems.Typed.Sum(p => p.PackedQty);
		}

		#endregion
	}
}
