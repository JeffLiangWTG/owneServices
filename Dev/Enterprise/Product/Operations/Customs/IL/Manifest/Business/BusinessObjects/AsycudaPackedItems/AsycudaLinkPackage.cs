using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.IL.Manifest.Business
{
	public class AsycudaLinkPackage : AutoAsycudaLinkPackage
	{
		public new class Schema : AutoAsycudaLinkPackage.Schema
		{
			public const string ContainerPK = "ContainerPK";
		}

		public AsycudaLinkPackage(AsycudaPackedItem packedItem)
			: base(packedItem.Factory)
		{
			PackedItem = packedItem;
		}

		public AsycudaPackedItem PackedItem { get; }

		public AsycudaPack Package
		{
			get
			{
				return fPackage != null && !fPackage.IsDeleted
					? fPackage
					: null;
			}
			set
			{
				fPackage = value;

				PackQtyInfo.RefreshBinding();
			}
		}
		AsycudaPack fPackage;

		public override ZString PackageNumber => Package?.APA_MarksAndNumbers ?? ZString.Empty;

		public override ZInt PackQty => Package?.APA_PackQty ?? ZInt.Zero;

		public override ZBool IsLinked
		{
			get => Pivot != null;
			set
			{
				var oldValue = IsLinked;
				PackedItem.ToggleLinkageWithPackage(Package, value);
				IsLinkedInfo.RefreshBinding(oldValue);
				if (!IsValidationSuspended)
				{
					Validation.ValidateIsLinked();
					PackedItem.AsycudaLinkPackages.Cast<AsycudaLinkPackage>().ForEach(x => x.Validation.ValidateAll());
				}
			}
		}

		public AsycudaPackPackedItemPivot Pivot => Package != null ? PackedItem.PackagesPivot.GetRelatedPivot(Package) : null;

		[List(nameof(Lookups) + "." + nameof(AsycudaLinkPackageLookups.Containers))]
		[ResourceStringData("Enterprise.Customs.IL.Manifest.Business.AsycudaLinkPackage|ContainerPK", Caption = "Container Number")]
		public ZGuid ContainerPK => Package?.ContainerPK ?? ZGuid.Empty;

		public AsycudaLinkPackageLookups Lookups => new AsycudaLinkPackageLookups(parent: this);
	}
}
