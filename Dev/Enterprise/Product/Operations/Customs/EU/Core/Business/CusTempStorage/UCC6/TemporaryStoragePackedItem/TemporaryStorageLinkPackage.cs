using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ManifestBase;

namespace Enterprise.Customs.EU.Business.CusTempStorage
{
	public class TemporaryStorageLinkPackage : AutoTemporaryStorageLinkPackage
	{
		public TemporaryStorageLinkPackage(TemporaryStoragePackedItem packedItem)
			: base(packedItem.Factory)
		{
			PackedItem = packedItem;
		}

		public TemporaryStoragePackedItem PackedItem { get; }

		public TemporaryStoragePack Package
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
		TemporaryStoragePack fPackage;

		[ResourceStringData("TemporaryStorageLinkPackage.PackageNumber", Caption = "Package Number")]
		public override ZString PackageNumber => GetPackageNumber();

		ZString GetPackageNumber()
		{
			var package = Package;
			var result = string.Empty;

			if (package != null)
			{
				var container = package.Container;
				var maybeContainerised = container == null || container.ACN_ContainerNumber.IsEmpty ? string.Empty : Res.GetString("D292A98D-37BB-46F4-9D75-2CABBDAC6ECD", " in {0}", container.ACN_ContainerNumber);
				var maybeMarked = package.APA_MarksAndNumbers.IsEmpty ? string.Empty : Res.GetString("248B1C7E-1E2B-481A-BF4B-71B38B9B2A0D", " marked {0}", package.APA_MarksAndNumbers);
				result = Res.GetString("2EC07801-A7E2-48E1-8ECF-D8F626A3E87A", "{0}{1}{2}", package.APA_PackUQ, maybeContainerised, maybeMarked);
			}

			return result;
		}

		[ResourceStringData("TemporaryStorageLinkPackage.PackQty", Caption = "Pack Quantity", ShortCaption = "Pack Qty")]
		public override ZInt PackQty => GetPackQty();

		ZInt GetPackQty()
		{
			var result = ZInt.Zero;
			if (Pivot != null && PackageNotLinkedInPreviousLine())
			{
				result = Pivot.Pack.APA_PackQty;
			}

			return result;
		}

		ZBool PackageNotLinkedInPreviousLine() => !PackedItem.Bill.PackedItems.Cast<TemporaryStoragePackedItem>().Any(line => line.API_LineNo < PackedItem.API_LineNo && line.TemporaryStorageLinkPackages.Any(link => link.IsLinked && link.Package == Package));
		[BusinessObjectTestExclude]
		[ResourceStringData("TemporaryStorageLinkPackage.IsLinked", Caption = "Is Linked")]
		[ReadOnlyMember(nameof(IsLinked_ReadOnly))]
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
					PackedItem.TemporaryStorageLinkPackages.Cast<TemporaryStorageLinkPackage>().ForEach(x => x.Validation.ValidateAll());
				}
			}
		}

		protected ZBool IsLinked_ReadOnly => PackedItem?.Bill?.Header?.IsNoEditAllowedCustomsStatus() ?? false;

		public AsycudaPackPackedItemPivot Pivot => Package != null ? PackedItem.PackagesPivot.GetRelatedPivot(Package) : null;
	}
}
