using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ManifestBase;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.H7.Business
{
	public class AsycudaPackPackedItemLink : AutoAsycudaPackPackedItemLink
	{
		public AsycudaPackPackedItemLink(AsycudaPackedItem packedItem)
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
				Factory.InvalidateCachedProperties();
			}
		}
		AsycudaPack fPackage;

		[ResourceStringData("AsycudaPackPackedItemLink.PackageNumber", Caption = "Package Number", MediumCaption = "Package No.", ShortCaption = "Package No.", FullDescription = "Package number of linked pack.")]
		public override ZString PackageNumber => Factory.GetValue(ref packageNumber, GetPackageNumber);

		CachedProperty<ZString> packageNumber;

		[ResourceStringData("AsycudaPackPackedItemLink.PackQty", Caption = "Quantity", MediumCaption = "Qty.", ShortCaption = "Qty.", FullDescription = "Item quantity within linked pack.")]
		public override ZInt PackQty => Factory.GetValue(ref packQty, GetPackQty);

		CachedProperty<ZInt> packQty;

		[BusinessObjectTestExclude]
		[ResourceStringData("AsycudaPackPackedItemLink.IsLinked", Caption = "Is Linked?", MediumCaption = "Is Linked?", ShortCaption = "Is Linked?", FullDescription = "Indicates whether the item is a stand-alone item or linked to a pack.")]
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
					PackedItem.AsycudaPackPackedItemLinks.Cast<AsycudaPackPackedItemLink>().ForEach(x => x.Validation.ValidateAll());
				}
			}
		}

		public AsycudaPackPackedItemPivot Pivot => Package != null ? PackedItem.PackagesPivot.GetRelatedPivot(Package) : null;

		ZString GetPackageNumber()
		{
			var package = Package;
			if (package is null)
			{
				return ZString.Empty;
			}

			var packageUQ = package.APA_PackUQ;
			var marksAndNumbers = package.APA_MarksAndNumbers;

			if (marksAndNumbers.IsEmpty)
			{
				return packageUQ;
			}

			var marked = Res.GetString("573131F8-3680-4528-9ECA-0C663144B5DB", "marked");
			var packageNumber = packageUQ.IsEmpty ? marked : $"{packageUQ} {marked}";

			return $"{packageNumber} {marksAndNumbers}";
		}

		ZInt GetPackQty()
		{
			var packQty = ZInt.Zero;

			if (Pivot != null && !PackageLinkedInPreviousLine)
			{
				packQty = Pivot.Pack.APA_PackQty;
			}

			return packQty;
		}

		bool PackageLinkedInPreviousLine => PackedItem.Bill.PackedItems.Any(line => line.API_LineNo < PackedItem.API_LineNo && line.AsycudaPackPackedItemLinks.Any(link => link.IsLinked && link.Package == Package));
	}
}
