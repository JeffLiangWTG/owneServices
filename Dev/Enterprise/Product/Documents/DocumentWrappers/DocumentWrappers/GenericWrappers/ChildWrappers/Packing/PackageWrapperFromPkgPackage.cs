using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperFromPkgPackage : PackageWrapperFromPkgPackageHeader
	{
		public PackageWrapperFromPkgPackage(PkgPackage packageBO, BusinessObjectFactory factory)
			: this(packageBO, Array.Empty<PkgPackageItemDivotsWrapper>(), factory, 0, 0)
		{
		}

		public PackageWrapperFromPkgPackage(PkgPackage packageBO, BusinessObjectFactory factory, ZInt displayOrder, ZInt indent)
			: this(packageBO, Array.Empty<PkgPackageItemDivotsWrapper>(), factory, displayOrder, indent)
		{
		}

		public PackageWrapperFromPkgPackage(PkgPackage packageBO, PkgPackageItemDivotsWrapper packedItem, BusinessObjectFactory factory, ZInt displayOrder, ZInt indent)
			: this(packageBO, new[] { packedItem }, factory, displayOrder, indent)
		{
		}

		public PackageWrapperFromPkgPackage(PkgPackage packageBO, PkgPackageItemDivotsWrapper[] packedItemsOverride, BusinessObjectFactory factory, ZInt displayOrder, ZInt indent)
			: base(packageBO, factory)
		{
			PackageBO = packageBO ?? factory.GetNull<PkgPackage>();
			PackedItemsInternal = packedItemsOverride ?? Array.Empty<PkgPackageItemDivotsWrapper>();
			this.indent = indent;
			this.displayOrder = displayOrder;

			PackageProductAttributes = new Lazy<PackageProductAttributesInfo>(() => CheckPackedItemsAttributeUsage());
		}

		protected readonly PkgPackage PackageBO;
		readonly PkgPackageItemDivotsWrapper[] PackedItemsInternal;
		readonly ZInt indent;
		readonly ZInt displayOrder;
		readonly Lazy<PackageProductAttributesInfo> PackageProductAttributes;

		#region Wrappers

		protected override PackedItemWrapper GetPackedItem()
		{
			var wrapper = HasPackedItem ? PackedItems[0] : null;
			var packedItems = wrapper != null ? wrapper.PackedItems.ToArray() : Array.Empty<PkgPackageItemDivotsWrapper>();
			return PackedItemWrapper.New((IPackableItemParent)wrapper?.WrappedObject, Factory, packedItems);
		}

		protected override ContainerWrapper GetContainer()
		{
			var packageToUse = PackageBO.IsContainer ? PackageBO : null;
			return new ContainerWrapperFromPkgPackage(packageToUse, Factory);
		}

		protected override FreightWrapper GetParent()
		{
			var wrappers = FreightWrapper.New(PackageBO.PackageJob, Factory);
			var wrapper = wrappers.Length > 0 ? wrappers[0] : null;
			var iDocTypeCode = wrapper as IDocTypeCode;
			if (iDocTypeCode != null)
			{
				iDocTypeCode.DocTypeCode = "PPL";
			}

			var iPackageOverride = wrapper as IPackageOverrider;
			if (iPackageOverride != null)
			{
				iPackageOverride.SetPackageOverride(PackageBO, PackedItemsInternal);
			}

			return wrapper;
		}

		protected override LocationWrapper GetOrigin()
		{
			return new LocationWrapper("", Factory);
		}

		#region GetMostRecentAudit

		protected override PackageAuditWrapper GetMostRecentAudit()
		{
			var audit = PackageBO?.GetMostRecentAudit();
			return audit != null ? new PackageAuditWrapper(audit, audit.Factory) : null;
		}

		#endregion

		#region PackageOrderReference

		protected override PackageOrderReferenceWrapper GetPackageOrderReference()
		{
			var orderReference = PackageBO?.PackageOrderReference;
			return orderReference != null ? new PackageOrderReferenceWrapper(orderReference, orderReference.Factory) : null;
		}

		#endregion

		#endregion

		#region Properties

		#region Packages - Damaged, Outturned, Pillaged

		#region GetPackages

		protected override PackQTYWrapper GetPackages()
		{
			return new PackQTYWrapper(PackageBO.KP_PackageQty, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetOutturnedPackages

		protected override PackQTYWrapper GetOutturnedPackages()
		{
			return new PackQTYWrapper(0, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetPillagedPackages

		protected override PackQTYWrapper GetPillagedPackages()
		{
			return new PackQTYWrapper(PackageBO.KP_IsPillaged ? PackageBO.KP_PackageQty : ZInt.Zero, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetFumigatedPackages

		protected override PackQTYWrapper GetFumigatedPackages()
		{
			return new PackQTYWrapper(PackageBO.KP_IsFumigated ? PackageBO.KP_PackageQty : ZInt.Zero, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetNonStackablePackages

		protected override PackQTYWrapper GetNonStackablePackages()
		{
			return new PackQTYWrapper(PackageBO.KP_IsNonStackable ? PackageBO.KP_PackageQty : ZInt.Zero, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetTopLoadOnlyPackages

		protected override PackQTYWrapper GetTopLoadOnlyPackages()
		{
			return new PackQTYWrapper(PackageBO.KP_IsTopLoadOnly ? PackageBO.KP_PackageQty : ZInt.Zero, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetHeatTreatedPackages

		protected override PackQTYWrapper GetHeatTreatedPackages()
		{
			return new PackQTYWrapper(PackageBO.KP_IsHeatTreated ? PackageBO.KP_PackageQty : ZInt.Zero, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetISPMPalletPackages

		protected override PackQTYWrapper GetISPMPalletPackages()
		{
			return new PackQTYWrapper(PackageBO.KP_IsISPMPallet ? PackageBO.KP_PackageQty : ZInt.Zero, PackageBO.KP_F3_NKPackType, PackageBO.Lookups.PackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#region GetDamagedPackages

		protected override PackQTYWrapper GetDamagedPackages()
		{
			return new PackQTYWrapper(PackageBO.KP_IsDamaged ? PackageBO.KP_PackageQty : ZInt.Zero, PackageBO.KP_F3_NKPackType, BindToLists.GetCachedLists(Factory).OuterPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		#endregion

		#endregion

		#region Volume / Weight / Dimensions

		#region GetWeight

		protected override WeightWrapper GetWeight()
		{
			return new WeightWrapper(PackageBO.KP_Weight, PackageBO.KP_WeightUQ, 2, PackageBO.Lookups.WeightUQs, Factory);
		}

		#endregion

		#region GetOutturnedWeight

		protected override WeightWrapper GetOutturnedWeight()
		{
			return new WeightWrapper(0, PackageBO.KP_WeightUQ, WeightWrapper.StandardDecimalPlaces, PackageBO.Lookups.WeightUQs, Factory);
		}

		#endregion

		#region GetVolume

		protected override VolumeWrapper GetVolume()
		{
			return new VolumeWrapper(PackageBO.KP_Volume, PackageBO.KP_VolumeUQ, PackageBO.Lookups.VolumeUQs, Factory);
		}

		#endregion

		#region GetOutturnedVolume

		protected override VolumeWrapper GetOutturnedVolume()
		{
			return new VolumeWrapper(0, PackageBO.KP_VolumeUQ, PackageBO.Lookups.VolumeUQs, Factory);
		}

		#endregion

		#region GetDimensions

		protected override DimensionsWrapper GetDimensions()
		{
			return new DimensionsWrapper(PackageBO.KP_Length, PackageBO.KP_Width, PackageBO.KP_Height, PackageBO.KP_DimensionUQ, DimensionDecimalPlaces, PackageBO.Lookups.DimensionUQs, Factory);
		}
		const int DimensionDecimalPlaces = 3;

		#endregion

		#endregion

		#region GetCartonGroupAndSize

		protected override ZString GetCartonGroupAndSize()
		{
			return PackageBO.CartonGroupAndSize;
		}

		#endregion

		#region GetCommodity

		protected override CodeAndDescriptionWrapper GetCommodity()
		{
			return CodeAndDescriptionWrapper.Empty;
		}

		#endregion

		#region GetDamagedReason

		protected override CodeAndDescriptionWrapper GetDamagedReason()
		{
			var damagedReasonCodeAndDescriptionList = PackingRegistry.Instance.DamagedReasons.Value.GetCodeDescriptionPairList();
			var damagedReason = PackageBO.KP_DamagedReason;
			if (!damagedReason.IsEmpty && damagedReasonCodeAndDescriptionList != null && damagedReasonCodeAndDescriptionList.ContainsCode(damagedReason))
			{
				return new CodeAndDescriptionWrapper(PackageBO.KP_DamagedReason, damagedReasonCodeAndDescriptionList.GetDescriptionFromCode(damagedReason), Factory);
			}

			return CodeAndDescriptionWrapper.Empty;
		}

		#endregion

		#region GetContainerNo / JobId

		protected override ZString GetContainerNo()
		{
			return GetPkgPackageContainerNo();
		}

		protected override ZString GetContainerJobID()
		{
			return GetPkgPackageContainerNo();
		}

		ZString GetPkgPackageContainerNo()
		{
			return PackageBO.IsContainer
				? PackageBO.KP_PackageID
				: PackageBO.OuterPackage.IsContainer
					? PackageBO.OuterPackage.KP_PackageID
					: ZString.Empty;
		}

		#endregion

		#region Custom Attributes

		protected override ZString GetCustomAttribute1()
		{
			return "";
		}

		protected override ZString GetCustomAttribute2()
		{
			return "";
		}

		protected override ZString GetCustomAttribute3()
		{
			return "";
		}

		protected override ZString GetCustomAttribute4()
		{
			return "";
		}

		protected override ZDateTime GetCustomDate1()
		{
			return ZDateTime.Empty;
		}

		protected override ZDateTime GetCustomDate2()
		{
			return ZDateTime.Empty;
		}

		protected override ZDecimal GetCustomDecimal1()
		{
			return 0m;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return 0m;
		}

		protected override ZBool GetCustomFlag1()
		{
			return ZBool.False;
		}

		protected override ZBool GetCustomFlag2()
		{
			return ZBool.False;
		}

		#endregion

		#region GetDescription

		protected override ZString GetDescription()
		{
			return PackageBO.KP_GoodsDescription;
		}

		#endregion

		#region GetDisplayOrder

		protected override ZString GetDisplayOrder()
		{
			return displayOrder.ToString("000");
		}

		#endregion

		#region GetFreightPackLine

		protected override PackLine GetFreightPackLine()
		{
			return null;
		}

		#endregion

		#region GetHarmonizedCode

		protected override ZString GetHarmonizedCode()
		{
			return PackageBO.KP_HSCode;
		}

		#endregion

		#region GetHouseBill

		protected override ZString GetHouseBill()
		{
			return "";
		}

		#endregion

		#region GetIndent

		protected override ZString GetIndent()
		{
			return new string(' ', indent * 6);
		}

		#endregion

		#region GetInners

		protected override ZInt GetInners()
		{
			return PackageBO.IsNull ? 0 : PackageBO.Packages.Sum(p => p.KP_PackageQty);
		}

		#endregion

		#region GetItemNumber

		protected override ZShort GetItemNumber()
		{
			return 0;
		}

		#endregion

		#region GetInnersDetail

		protected override ZString GetInnersDetail()
		{
			var result = new ZStringBuilder();
			var validInners = PackageBO.IsNull ? Array.Empty<PkgPackage>() : PackageBO.Packages.Where(p => !p.KP_PackageID.IsEmpty).ToArray();
			result.Append(string.Join(", ", validInners.Take(5).Select(p => string.Format("{0}({1})", p.KP_PackageID, p.KP_F3_NKPackType))));
			if (validInners.Length > 5)
			{
				result.Append(Res.GetString("da70a968-e4b4-42d2-a5ee-939a282ed59f", ",\r\nMore Packages exist, please refer to Manifest"));
			}

			return result.ToString();
		}

		#endregion

		#region GetLinePrice

		protected override ZDecimal GetLinePrice()
		{
			return string.IsNullOrEmpty(CommonCurrency?.Code)
				? 0m
				: Utilities.Round(GetPackedItems().Cast<PackedItemWrapper>().Sum(i => i.LinePrice), 2);
		}

		#endregion

		#region GetCommonCurrency

		protected override CurrencyWrapper GetCommonCurrency()
		{
			var packedItemCurrencies = GetPackedItems().Cast<PackedItemWrapper>().Select(p => p.CommonCurrency);
			var currencyCodes = packedItemCurrencies.Select(c => c.Code).Distinct();
			return currencyCodes.Count() == 1 ? packedItemCurrencies.First() : new CurrencyWrapper(null, Factory);
		}

		#endregion

		#region GetMarksAndNumbers

		protected override ZString GetMarksAndNumbers()
		{
			return PackageBO.KP_MarksAndNumbers;
		}

		#endregion

		#region GetMasterBill

		protected override ZString GetMasterBill()
		{
			return "";
		}

		#endregion

		#region GetPickLocation

		protected override ZString GetPickLocation()
		{
			return GetPickLinePropertyContent(pl => pl.InventoryLineForAvailableInventory.LocationString);
		}

		#endregion

		#region GetPickMethod

		protected override ZString GetPickMethod()
		{
			return GetPickLinePropertyContent(pl => pl.InventoryLineForAvailableInventory.Location.WLV_PickMethod);
		}

		#endregion

		#region GetPickGroup

		protected override ZString GetPickGroup()
		{
			var result = base.GetPickGroup();
			var orderLine = new PackagePackingHelper(PackageBO).GetPickLineFromPackage()?.DocketLine as WhsOrderLine;
			if (orderLine != null)
			{
				result = orderLine.WE_PickGroup.IsEmpty ? (ZString)Res.GetString("EBC6E36A-D064-4606-9F20-6413172E3DFB", "None") : orderLine.PickGroupDescription;
			}
			return result;
		}

		#endregion

		#region GetAreaName

		protected override ZString GetAreaName()
		{
			var result = new PackagePackingHelper(PackageBO).GetPickLineFromPackage()?.InventoryLineForAvailableInventory?.LocationAreaName;
			return result ?? base.GetAreaName();
		}

		#endregion

		#region GetPickLinePropertyContent

		ZString GetPickLinePropertyContent(Func<WhsPickLine, ZString> getPickLineProperty)
		{
			var result = ZString.Empty;

			var propertyFromPickLines = new PackagePackingHelper(PackageBO).GetPickLinesFromPackage().Select(getPickLineProperty).Distinct().Take(2).ToArray();

			if (propertyFromPickLines.Length > 1)
			{
				result = Res.GetString("A339F205-6813-44D2-A5C0-5E6BC62FD774", "MULTIPLE");
			}
			else if (propertyFromPickLines.Length == 1)
			{
				result = propertyFromPickLines.First();
			}

			return result;
		}

		#endregion

		#region GetNMFC

		protected override ZString GetNMFC()
		{
			var commodity = PackageBO.CommodityCode;
			var nmfc = commodity != null ? commodity.NMFC : null;
			return nmfc != null ? nmfc.FN_Code : ZString.Empty;
		}

		#endregion

		#region GetOutturnComment

		protected override ZString GetOutturnComment()
		{
			return "";
		}

		#endregion

		#region GetPackageTemperatures

		protected override ZString GetPackageTemperatures()
		{
			var result = ZString.Empty;
			if (PackageBO.IsContainer)
			{
				var container = Container;
				if (container != null && container.ControlledAtmosphere)
				{
					result = Res.GetString("3e170624-4dff-474f-a186-74610529f2da", "Set Point Temp.: {0}°{1}",
						container.SetPointTemperature.Value, container.SetPointTemperature.Unit.Code);
				}
			}
			else if (PackageBO.KP_RequiresTemperatureControl)
			{
				result = Res.GetString("bfb02ec7-fd7e-4e62-a1f4-e5b5ae8e11d7", "Min/Max Temp.: {0}°{2} - {1}°{2}",
					PackageBO.KP_RequiredTemperatureMinimum, PackageBO.KP_RequiredTemperatureMaximum, PackageBO.KP_RequiredTemperatureUnit);
			}

			return result;
		}

		#endregion

		#region GetPackedItemCount

		/// <summary>
		/// Unfortunate naming of 'PackedItemCount', this is actually the Packed Qty of PackableItems in the Package.
		/// </summary>
		protected override ZInt GetPackedItemCount()
		{
			return (ZInt)PackageBO.PackedItems.Typed.Sum(p => p.PackedQty);
		}

		#endregion

		#region GetPackingOrder

		protected override ZInt GetPackingOrder()
		{
			return 0; // Not required
		}

		#endregion

		#region GetOutterPackageSequence

		protected override ZShort GetOutterPackageSequence()
		{
			return PackageBO?.KP_Sequence ?? ZShort.Zero;
		}

		#endregion

		#region GetOutterPackagesCount

		protected override ZShort GetOutterPackagesCount()
		{
			var result = ZShort.Zero;
			if (PackageBO?.PackageJob != null && PackageBO?.PackageJob.ParentJob != null)
			{
				switch (PackageBO.PackageJob.ParentJob.PackageSequenceType)
				{
					case PackageSequenceType.OuterWithLooseID:
						{
							result = (ZShort)(Math.Min(PackageBO.PackageJob.Packages.Count(p => !p.KP_KPH_PackageHeader.IsEmpty) + PackageBO.PackageJob.LoosePackageIDs.Count, short.MaxValue));
							break;
						}
					default:
						{
							result = (ZShort)PackageBO.PackageJob.GetAllPackagesOnJob().Count(p => p.KP_KP_ParentPackage.IsEmpty);
							break;
						}
				}
			}
			return result;
		}

		#endregion

		#region GetRefNumber

		protected override ZString GetRefNumber()
		{
			return PackageBO.KP_PackageID;
		}

		#endregion

		#region GetExportRefNumber

		protected override ZString GetExportRefNumber()
		{
			return ZString.Empty;
		}

		#endregion

		#region GetSeals

		protected override ZString GetSeals()
		{
			ZString result = new();

			var container = Container;
			if (container != null)
			{
				var builder = new ZStringBuilder();
				builder.AppendIfNotEmpty(container.SealNo);
				builder.AppendIfNotEmpty(container.SealNo2);
				builder.AppendIfNotEmpty(container.SealNo3);
				result = builder.ToStringWithNewLineBetweenAppends();
			}

			return result;
		}

		#endregion

		#region GetUOMType

		protected override CodeAndDescriptionWrapper GetUOMType()
		{
			switch (PackageBO?.PackType?.F3_UOMType)
			{
				case UOMPackTypesList.Codes.Pallet:
					return new CodeAndDescriptionWrapper(UOMPackTypesList.Codes.Pallet, UOMPackTypesList.Descriptions.Pallet, Factory);
				case UOMPackTypesList.Codes.Case:
					return new CodeAndDescriptionWrapper(UOMPackTypesList.Codes.Case, UOMPackTypesList.Descriptions.Case, Factory);
				case UOMPackTypesList.Codes.SplitCase:
					return new CodeAndDescriptionWrapper(UOMPackTypesList.Codes.SplitCase, UOMPackTypesList.Descriptions.SplitCase, Factory);
				default:
					return CodeAndDescriptionWrapper.Empty;
			}
		}

		#endregion

		#endregion

		#region Collections

		protected override CustomsEntryWrapperCollection GetCustomsEntries()
		{
			return new CustomsEntryWrapperCollection(PackageBO, Factory);
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			return new UNDGSubstanceWrapperCollection(PackageBO, Factory);
		}

		protected override PackProductWrapperCollection GetProducts()
		{
			return PackProductWrapperCollection.Empty;
		}

		protected override PackedItemWrapperCollection GetPackedItems()
		{
			var packedItems = PackedItemsInternal.Length > 0 ? PackedItemsInternal : PackageBO.PackedItems.Typed.ToArray();
			return new PackedItemWrapperCollection(packedItems, Factory);
		}

		#endregion

		#region Flags

		#region GetHasPackedItem

		protected override ZBool GetHasPackedItem()
		{
			return PackedItems.Count > 0;
		}

		#endregion

		#region GetHasSingleProduct

		/// <summary>
		/// This checks if the Current Package and all its Inners are the same product
		/// </summary>
		protected override ZBool GetHasSingleProduct()
			=> PackageProductAttributes.Value.IsSameProductUsedOnAllPackages;

		protected override ZBool GetHasSingleAttribute1()
			=> PackageProductAttributes.Value.IsSameAttribute1UsedOnAllPackages;

		protected override ZBool GetHasSingleAttribute2()
			=> PackageProductAttributes.Value.IsSameAttribute2UsedOnAllPackages;

		protected override ZBool GetHasSingleAttribute3()
			=> PackageProductAttributes.Value.IsSameAttribute3UsedOnAllPackages;

		protected override ZBool GetHasSingleExpiryDate()
			=> PackageProductAttributes.Value.IsSameExpiryDateUsedOnAllPackages;

		protected override ZBool GetHasSinglePackingDate()
			=> PackageProductAttributes.Value.IsSamePackingDateUsedOnAllPackages;

		PackageProductAttributesInfo CheckPackedItemsAttributeUsage()
		{
			var packagesToCheck = new Queue<PackageWrapperFromPkgPackage>([this]);
			var result = new PackageProductAttributesInfo();

			string productToMatch = null;
			string attribute1ToMatch = null;
			string attribute2ToMatch = null;
			string attribute3ToMatch = null;
			string expiryDateToMatch = null;
			string packingDateToMatch = null;

			var shouldShortCircuit = false;

			while (packagesToCheck.Count > 0)
			{
				var packageToCheckWrapper = packagesToCheck.Dequeue();
				foreach (PackedItemWrapper packedItem in packageToCheckWrapper.PackedItems)
				{
					shouldShortCircuit = UpdatePackageProductAttributesInfo(packedItem);

					if (shouldShortCircuit)
					{
						break;
					}
				}

				if (shouldShortCircuit)
				{
					break;
				}

				if (!packageToCheckWrapper.PackageBO.IsNull)
				{
					foreach (var package in packageToCheckWrapper.PackageBO.Packages)
					{
						var innerWrapper = new PackageWrapperFromPkgPackage(package, package.PackedItems.Typed.ToArray(), packageToCheckWrapper.Factory, 0, 0);
						packagesToCheck.Enqueue(innerWrapper);
					}
				}
			}

			return result;

			bool UpdatePackageProductAttributesInfo(PackedItemWrapper packedItem)
			{
				result.IsAttribute1Used = result.IsAttribute1Used || packedItem.IsPartAttrib1Used;
				result.IsAttribute2Used = result.IsAttribute2Used || packedItem.IsPartAttrib2Used;
				result.IsAttribute3Used = result.IsAttribute3Used || packedItem.IsPartAttrib3Used;
				result.IsExpiryDateUsed = result.IsExpiryDateUsed || packedItem.IsExpiryUsed;
				result.IsPackingDateUsed = result.IsPackingDateUsed || packedItem.IsPackingDateUsed;
				result.IsTrackedSerialUsed = result.IsTrackedSerialUsed || packedItem.IsTrackedSerialUsed;

				if (productToMatch == null)
				{
					productToMatch = packedItem.Code;
					result.IsSameProductUsedOnAllPackages = true;
				}
				else if (result.IsSameProductUsedOnAllPackages)
				{
					result.IsSameProductUsedOnAllPackages &= productToMatch == packedItem.Code;

					if (!result.IsSameProductUsedOnAllPackages)
					{
						result.IsSameAttribute1UsedOnAllPackages = false;
						result.IsSameAttribute2UsedOnAllPackages = false;
						result.IsSameAttribute3UsedOnAllPackages = false;
						result.IsSameExpiryDateUsedOnAllPackages = false;
						result.IsSamePackingDateUsedOnAllPackages = false;
					}
				}

				attribute1ToMatch = InitialiseOrVerifySamePartAttributeUsed(
					attribute1ToMatch,
					packedItem.PartAttribute1,
					result.IsAttribute1Used,
					result.IsSameAttribute1UsedOnAllPackages,
					(isAttribute1Used) => result.IsSameAttribute1UsedOnAllPackages = isAttribute1Used);

				attribute2ToMatch = InitialiseOrVerifySamePartAttributeUsed(
					attribute2ToMatch,
					packedItem.PartAttribute2,
					result.IsAttribute2Used,
					result.IsSameAttribute2UsedOnAllPackages,
					(isAttribute2Used) => result.IsSameAttribute2UsedOnAllPackages = isAttribute2Used);

				attribute3ToMatch = InitialiseOrVerifySamePartAttributeUsed(
					attribute3ToMatch,
					packedItem.PartAttribute3,
					result.IsAttribute3Used,
					result.IsSameAttribute3UsedOnAllPackages,
					(isAttribute3Used) => result.IsSameAttribute3UsedOnAllPackages = isAttribute3Used);

				expiryDateToMatch = InitialiseOrVerifySamePartAttributeUsed(
					expiryDateToMatch,
					packedItem.Expiry,
					result.IsExpiryDateUsed,
					result.IsSameExpiryDateUsedOnAllPackages,
					(isExpiryUsed) => result.IsSameExpiryDateUsedOnAllPackages = isExpiryUsed);

				packingDateToMatch = InitialiseOrVerifySamePartAttributeUsed(
					packingDateToMatch,
					packedItem.PackingDate,
					result.IsPackingDateUsed,
					result.IsSamePackingDateUsedOnAllPackages,
					(isPackingDateUsed) => result.IsSamePackingDateUsedOnAllPackages = isPackingDateUsed);

				return CanShortcircuitQueue();

				string InitialiseOrVerifySamePartAttributeUsed(
					string attributeValueToMatch,
					string attributeValue,
					ZBool isAttribUsedOnPackage,
					ZBool isSameAttribUsedOnPackages,
					Action<ZBool> setAttribIsUsedOnAllPackages)
				{
					var revisedAttributeToMatch = attributeValueToMatch;

					if (attributeValueToMatch == null && isAttribUsedOnPackage)
					{
						revisedAttributeToMatch = attributeValue;
						setAttribIsUsedOnAllPackages(true);
					}
					else if (isSameAttribUsedOnPackages)
					{
						setAttribIsUsedOnAllPackages(attributeValueToMatch.Equals(attributeValue, StringComparison.OrdinalIgnoreCase));
					}

					return revisedAttributeToMatch;
				}
			}

			bool CanShortcircuitQueue()
				=> AreAllAttributesUsed() && !ContinueCheckingForSameAttributes();

			bool AreAllAttributesUsed()
				=> result.IsAttribute1Used
				&& result.IsAttribute2Used
				&& result.IsAttribute3Used
				&& result.IsPackingDateUsed
				&& result.IsExpiryDateUsed;

			bool ContinueCheckingForSameAttributes()
				=> result.IsSameProductUsedOnAllPackages
					|| result.IsSameAttribute1UsedOnAllPackages
					|| result.IsSameAttribute2UsedOnAllPackages
					|| result.IsSameAttribute3UsedOnAllPackages
					|| result.IsSameExpiryDateUsedOnAllPackages
					|| result.IsSamePackingDateUsedOnAllPackages;
		}

		#endregion

		#region GetIsAttributeUsed

		protected override ZBool GetIsPartAttrib1Used()
			=> PackageProductAttributes.Value.IsAttribute1Used;

		protected override ZBool GetIsPartAttrib2Used()
			=> PackageProductAttributes.Value.IsAttribute2Used;

		protected override ZBool GetIsPartAttrib3Used()
			=> PackageProductAttributes.Value.IsAttribute3Used;

		protected override ZBool GetIsTrackedSerialUsed()
			=> PackageProductAttributes.Value.IsTrackedSerialUsed;

		#endregion

		#region GetIsExclusive

		/// <summary>
		/// This checks *only* the Current Package to see if there is only 1 product.
		/// </summary>
		protected override ZBool GetIsExclusive()
		{
			var distinctItems = PackageBO.PackedItemDivots.Select(d => d.PackedItem?.Key).Distinct().ToArray();
			return distinctItems.Length == 1 && distinctItems.Single() != null;
		}

		#endregion

		#region GetIsExpiryUsed

		protected override ZBool GetIsExpiryUsed()
			=> PackageProductAttributes.Value.IsExpiryDateUsed;

		#endregion

		#region GetIsPackageIdValidSSCCBarCode

		protected override ZBool GetIsPackageIdValidSSCCBarCode()
		{
			// tested in base class.
			return PackageBO.IsPackageIdValidSSCCBarCode;
		}

		#endregion

		#region GetIsPackingDateUsed

		protected override ZBool GetIsPackingDateUsed()
			=> PackageProductAttributes.Value.IsPackingDateUsed;

		#endregion

		#region GetIsTopLevelNonContainerisedPackage

		protected override ZBool GetIsTopLevelNonContainerisedPackage()
		{
			var packageJob = PackageBO.PackageJob;
			return packageJob != null && packageJob.GetAllNonContainerOutersAndFirstLevelPackagesOnContainers().Contains(PackageBO);
		}

		#endregion

		#region GetIsTopLevelPackage

		protected override ZBool GetIsTopLevelPackage()
		{
			return indent == 0;
		}

		#endregion

		#region PackageState

		protected override PackageStateWrapper GetPackageState()
		{
			return new PackageStateWrapper(this, Factory);
		}

		#endregion

		#endregion

		#region GetPackageBookedDetail

		public PkgPackageBookedDetail GetPackageBookedDetail()
		{
			return PackageBO.BookedDimensions;
		}

		#endregion
	}
}
