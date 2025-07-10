using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public abstract class PackedItemWrapper : GenericWrapper
	{
		const int PackedQtyDecimalPlaces = 2;

		protected PackedItemWrapper(IPackableItemParent packableItemParent, PkgPackageItemDivotsWrapper[] packedItems, BusinessObjectFactory factory)
			: base((BusinessObject)packableItemParent, factory)
		{
			PackedItems = Argument.NotNull(packedItems, nameof(packedItems));
			EnsurePackedItemsAreInTheSamePackage(packableItemParent, packedItems);
		}

		static void EnsurePackedItemsAreInTheSamePackage(IPackableItemParent packableItemParent, PkgPackageItemDivotsWrapper[] packedItems)
		{
			var firstPackedItem = packedItems.FirstOrDefault();
			var packableItemParentToCheck = packableItemParent ?? firstPackedItem?.PackableItemParent;
			if (firstPackedItem != null && packedItems.Any(p => p.PackagePK != firstPackedItem.PackagePK || p.PackableItemParent != packableItemParentToCheck))
			{
				throw new InvalidOperationException("Should only pass in Packed Items for the same Package and same PackableItemParent.");
			}
		}

		public static PackedItemWrapper New(IPackableItemParent packableItemParent, BusinessObjectFactory factory, params PkgPackageItemDivotsWrapper[] packedItems)
		{
			var releaseLine = packableItemParent as WhsReleaseLine;

			return (releaseLine != null)
				? new PackedItemWrapperFromWhsReleaseLine(releaseLine, packedItems, factory)
				: new PackedItemWrapperEmpty(packedItems, factory);
		}

		#region Related Business Objects

		#region CustomFields

		public CustomFieldWrapperCollection CustomFields
		{
			get { return customFields ?? (customFields = GetCustomFields()); }
		}

		CustomFieldWrapperCollection GetCustomFields()
		{
			var collection = new CustomFieldWrapperCollection(Factory);

			var customLabelsProvider = GetCustomLabelsProvider();
			if (customLabelsProvider != null)
			{
				var configOrg = customLabelsProvider.CustomLabelsProvider.ConfigOrgProvider.ConfigOrg;
				if (configOrg != null)
				{
					var customFields = customLabelsProvider.CustomLabelsProvider.GetCustomFields(configOrg, Factory).OfType<CustomLabelInfo>().Where(c => c.IsEnabled);
					foreach (var customField in customFields)
					{
						collection.Add(new CustomFieldWrapper(customLabelsProvider.BizOWithCustomFields, customField, Factory));
					}
				}
			}

			return collection;
		}

		protected virtual CustomLabelsProviderAndBizO GetCustomLabelsProvider()
		{
			return null;
		}

		CustomFieldWrapperCollection customFields;

		#endregion

		#region PackableItemParent

		protected IPackableItemParent PackableItemParent => (IPackableItemParent)WrappedObject;

		#endregion

		#region PackedItems

		public IEnumerable<PkgPackageItemDivotsWrapper> PackedItems { get; }

		#endregion

		#region Product

		public GenericWrapper Product
		{
			get { return GetProduct(); }
		}

		protected virtual GenericWrapper GetProduct()
		{
			return new ProductWrapper(null, Factory);
		}

		#endregion

		#region OrgPartRelation

		public OrgPartRelation OrgPartRelation
		{
			get { return GetOrgPartRelation(); }
		}

		protected virtual OrgPartRelation GetOrgPartRelation()
		{
			return null;
		}

		#endregion

		#endregion

		#region Properties

		#region Code

		public ZString Code
		{
			get { return GetCode(); }
		}

		protected virtual ZString GetCode()
		{
			return PackableItemParent?.Code ?? ZString.Empty;
		}

		public ZString LocalCode
		{
			get { return GetLocalCode(); }
		}

		protected virtual ZString GetLocalCode()
		{
			return ZString.Empty;
		}

		public ZString LocalCodeWithFallback
		{
			get { return GetLocalCodeWithFallback(); }
		}

		protected virtual ZString GetLocalCodeWithFallback()
		{
			return ZString.Empty;
		}

		#endregion

		#region Description

		public ZString Description
		{
			get { return GetDescription(); }
		}

		protected virtual ZString GetDescription()
		{
			return PackableItemParent?.Description ?? ZString.Empty;
		}

		public ZString LocalDescription
		{
			get { return GetLocalDescription(); }
		}

		protected virtual ZString GetLocalDescription()
		{
			return ZString.Empty;
		}

		public ZString LocalDescriptionWithFallback
		{
			get { return GetLocalDescriptionWithFallback(); }
		}

		protected virtual ZString GetLocalDescriptionWithFallback()
		{
			return ZString.Empty;
		}

		public ZString DescriptionSupplement
		{
			get { return PackableItemParent?.DescriptionSupplement ?? ZString.Empty; }
		}

		#endregion

		#region Quantity

		public ValueAndUnitWrapper PackedQty
		{
			get
			{
				var packedQty = PackedItems.Sum(d => d.PackedQty);
				return new ValueAndUnitWrapper(packedQty, TotalQtyUQ, PackedQtyDecimalPlaces, PackedItemTypes, Factory);
			}
		}

		public ValueAndUnitWrapper TotalQty
		{
			get
			{
				var value = PackableItemParent?.TotalQty ?? ZDecimal.Zero;
				return new ValueAndUnitWrapper(value, TotalQtyUQ, PackedQtyDecimalPlaces, PackedItemTypes, Factory);
			}
		}

		ZString TotalQtyUQ
		{
			get { return PackableItemParent?.TotalQtyUQ ?? ZString.Empty; }
		}

		CodeDescriptionPairList PackedItemTypes
		{
			get { return packedItemTypes ?? (packedItemTypes = new RefPackTypeCollection(Factory).GetAsCodeDescriptionPairWithStandardUnits()); }
		}

		CodeDescriptionPairList packedItemTypes;

		#endregion

		#region Part Attributes

		#region PartAttribute1

		public ZBool IsPartAttrib1Used
		{
			get { return GetIsPartAttrib1Used(); }
		}

		protected virtual ZBool GetIsPartAttrib1Used()
		{
			return ZBool.False;
		}

		public ZString PartAttrib1Name
		{
			get { return GetPartAttrib1Name(); }
		}

		protected virtual ZString GetPartAttrib1Name()
		{
			return Res.GetString("61027a06-6f4a-46c1-8f2c-33185a2048b8", "Part Attrib. 1");
		}

		public ZString PartAttribute1
		{
			get { return GetPartAttribute1(); }
		}

		protected virtual ZString GetPartAttribute1()
		{
			return GetCustomProperty(PartAttrib1Name);
		}

		#endregion

		#region PartAttribute2

		public ZBool IsPartAttrib2Used
		{
			get { return GetIsPartAttrib2Used(); }
		}

		protected virtual ZBool GetIsPartAttrib2Used()
		{
			return ZBool.False;
		}

		public ZString PartAttrib2Name
		{
			get { return GetPartAttrib2Name(); }
		}

		protected virtual ZString GetPartAttrib2Name()
		{
			return Res.GetString("88612d92-4169-4945-96a7-13d26dea5fad", "Part Attrib. 2");
		}

		public ZString PartAttribute2
		{
			get { return GetPartAttribute2(); }
		}

		protected virtual ZString GetPartAttribute2()
		{
			return GetCustomProperty(PartAttrib2Name);
		}

		#endregion

		#region PartAttribute3

		public ZBool IsPartAttrib3Used
		{
			get { return GetIsPartAttrib3Used(); }
		}

		protected virtual ZBool GetIsPartAttrib3Used()
		{
			return ZBool.False;
		}

		public ZString PartAttrib3Name
		{
			get { return GetPartAttrib3Name(); }
		}

		protected virtual ZString GetPartAttrib3Name()
		{
			return Res.GetString("38d34541-c042-4db1-ba67-847780570410", "Part Attrib. 3");
		}

		public ZString PartAttribute3
		{
			get { return GetPartAttribute3(); }
		}

		protected virtual ZString GetPartAttribute3()
		{
			return GetCustomProperty(PartAttrib3Name);
		}

		#endregion

		#region TrackedSerialNumber

		public ZBool IsTrackedSerialUsed
		{
			get { return GetIsTrackedSerialUsed(); }
		}

		protected virtual ZBool GetIsTrackedSerialUsed()
		{
			return ZBool.False;
		}

		public ZString TrackedSerialNumberName
		{
			get { return GetTrackedSerialNumberName(); }
		}

		protected virtual ZString GetTrackedSerialNumberName()
		{
			return Res.GetString("730c4d88-937c-4ead-b03a-570616b1d93d", "Tracked Serial Number");
		}

		public ZString TrackedSerialNumber
		{
			get { return GetTrackedSerialNumber(); }
		}

		protected virtual ZString GetTrackedSerialNumber()
		{
			return GetCustomProperty(Core.Constants.PartAttributes.SerialNumberPropertyName);
		}

		#endregion

		#region PackingDate

		public ZBool IsPackingDateUsed
		{
			get { return GetIsPackingDateUsed(); }
		}

		protected virtual ZBool GetIsPackingDateUsed()
		{
			return ZBool.False;
		}

		public ZString PackingDate
		{
			get { return GetPackingDate(); }
		}

		protected virtual ZString GetPackingDate()
		{
			return GetCustomProperty((NoResString)"Packing");
		}

		#endregion

		#endregion

		#region Expiry

		public ZBool IsExpiryUsed
		{
			get { return GetIsExpiryUsed(); }
		}

		protected virtual ZBool GetIsExpiryUsed()
		{
			return ZBool.False;
		}

		public ZString ExpiryLabel
		{
			get { return GetExpiryLabel(); }
		}

		protected virtual ZString GetExpiryLabel()
		{
			return Res.GetString("920a881f-80ec-464e-a904-da1709cb25f3", "EXPIRY DATE");
		}

		public ZString Expiry
		{
			get { return GetExpiry(); }
		}

		protected virtual ZString GetExpiry()
		{
			return GetCustomProperty((NoResString)"Expiry");
		}

		protected ZString ProductLabelDateFormat
		{
			get { return PackingRegistry.Instance.ProductLabelDateFormat.Value; }
		}

		#endregion

		#region Batch

		public ZString BatchLabel
		{
			get { return GetBatchLabel(); }
		}

		protected virtual ZString GetBatchLabel()
		{
			return Res.GetString("be06ed52-5431-4f4f-9654-60c2edb574b1", "BATCH/LOT");
		}

		public ZString Batch
		{
			get { return GetBatch(); }
		}

		protected virtual ZString GetBatch()
		{
			return "";
		}

		#endregion

		#region ProductCodeStockUnitBarcodeNumber

		public ZString ProductCodeStockUnitBarcodeNumber
		{
			get { return GetProductCodeStockUnitBarcodeNumber(); }
		}

		protected virtual ZString GetProductCodeStockUnitBarcodeNumber()
		{
			return "";
		}

		#endregion

		#region ProductBarcodeWithPrefixes

		public ZString ProductBarcodeWithPrefixes
		{
			get { return GetProductBarcodeWithPrefixes(); }
		}

		protected virtual ZString GetProductBarcodeWithPrefixes()
		{
			return "";
		}

		#endregion

		#region ProductBarcode

		public ZString ProductBarcode
		{
			get { return GetProductBarcode(); }
		}

		protected virtual ZString GetProductBarcode()
		{
			return "";
		}

		#endregion

		#region PackageInfo

		public ZString PackageInfo
		{
			get
			{
				ZString result = "";
				var package = PackedItems.FirstOrDefault()?.ParentPackage;
				if (package != null)
				{
					result = GetPackageDescription(package);
					result += GetRecursiveParentPackages(package, "");
				}
				return result;
			}
		}

		ZString GetRecursiveParentPackages(PkgPackage package, string indent)
		{
			ZString result = "";
			var parentPackage = package.ParentPackage;
			if (parentPackage != null)
			{
				indent += new string(' ', 6);
				result += System.Environment.NewLine + indent + GetPackageDescription(parentPackage);
				result += GetRecursiveParentPackages(parentPackage, indent);
			}
			return result;
		}

		ZString GetPackageDescription(PkgPackage package)
		{
			var result = string.Format((NoResString)"{0}x {1}", package.KP_PackageQty, package.KP_F3_NKPackType); // String is empty or contains only symbols.
			if (!package.KP_PackageID.IsEmpty)
			{
				result += " - " + package.KP_PackageID;
			}
			return result;
		}

		protected ZString GetCustomProperty(ZString identifier)
		{
			var result = "";

			var additionalProperties = PackableItemParent?.AdditionalProperties;
			if (additionalProperties != null)
			{
				var customProperty = additionalProperties.CustomProperties.FirstOrDefault(c => c.Identifier == identifier);
				result = (customProperty != null) ? (ZString)customProperty.GetValue((BusinessObject)PackableItemParent) : ZString.Empty;
			}

			return result;
		}

		#endregion

		#region LinePrice

		public ZDecimal LinePrice => GetLinePrice();

		protected virtual ZDecimal GetLinePrice()
		{
			return 0m;
		}

		#endregion

		#region CommonCurrency

		public CurrencyWrapper CommonCurrency => GetCommonCurrency();

		protected virtual CurrencyWrapper GetCommonCurrency()
		{
			return new CurrencyWrapper(null, Factory);
		}

		#endregion

		#endregion
	}
}
