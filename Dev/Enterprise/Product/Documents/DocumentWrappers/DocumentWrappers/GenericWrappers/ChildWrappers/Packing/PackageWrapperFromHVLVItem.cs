using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Barcode.Business;
using Enterprise.eTail.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperFromHVLVItem : PackageWrapper
	{
		public PackageWrapperFromHVLVItem(HVLVItem item, BusinessObjectFactory factory)
			: base(item, factory)
		{
			Argument.NotNull(factory, "factory");
			ItemBO = item ?? Factory.GetNull<HVLVItem>();
			ConsignmentBO = item?.Consignment ?? Factory.GetNull<HVLVConsignment>();
		}

		readonly HVLVItem ItemBO;
		readonly HVLVConsignment ConsignmentBO;

		protected override PackQTYWrapper GetPackages()
		{
			return new PackQTYWrapper(1, Core.Constants.PkgUnit.Package, BindToLists.GetCachedLists(Factory).OuterPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetOutturnedPackages()
		{
			return PackQTYWrapper.Empty;
		}

		protected override PackQTYWrapper GetPillagedPackages()
		{
			return PackQTYWrapper.Empty;
		}

		protected override PackQTYWrapper GetDamagedPackages()
		{
			return PackQTYWrapper.Empty;
		}

		protected override ZString GetCartonGroupAndSize()
		{
			return ZString.Empty;
		}

		protected override CodeAndDescriptionWrapper GetCommodity()
		{
			return CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetDamagedReason()
		{
			return CodeAndDescriptionWrapper.Empty;
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			return new UNDGSubstanceWrapperCollection(Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			var volume = ItemBO.HVI_ActualVolume <= 0 ? ItemBO.HVI_ManifestedVolume : ItemBO.HVI_ActualVolume;
			return new VolumeWrapper(volume, ConsignmentBO.HVC_VolumeUQ,
				HVLVItemSchema.HVI_ActualVolume.Scale, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume), Factory);
		}

		protected override VolumeWrapper GetOutturnedVolume()
		{
			return VolumeWrapper.Empty;
		}

		protected override WeightWrapper GetWeight()
		{
			var weight = ItemBO.HVI_ActualWeight <= 0 ? ItemBO.HVI_ManifestedWeight : ItemBO.HVI_ActualWeight;
			return new WeightWrapper(weight, ConsignmentBO.HVC_WeightUQ,
				HVLVItemSchema.HVI_ActualWeight.Scale, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight), Factory);
		}

		protected override WeightWrapper GetOutturnedWeight()
		{
			return WeightWrapper.Empty;
		}

		protected override DimensionsWrapper GetDimensions()
		{
			return DimensionsWrapper.Empty;
		}

		protected override ContainerWrapper GetContainer()
		{
			return null;
		}

		protected override ZInt GetPackingOrder()
		{
			return ZInt.Zero;
		}

		protected override ZString GetDescription()
		{
			return ConsignmentBO.HVC_GoodsDescription;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return ZString.Empty;
		}

		protected override ZString GetContainerNo()
		{
			return ZString.Empty;
		}

		protected override ZString GetContainerJobID()
		{
			return ZString.Empty;
		}

		protected override ZString GetHouseBill()
		{
			return ZString.Empty;
		}

		protected override ZString GetMasterBill()
		{
			return ZString.Empty;
		}

		protected override ZString GetOutturnComment()
		{
			return ZString.Empty;
		}

		protected override FreightWrapper GetParent()
		{
			FreightWrapper[] parents = FreightWrapper.New(ConsignmentBO, ItemBO, Factory);
			return (parents.Length > 0) ? parents[0] : null;
		}

		protected override ZDecimal GetLinePrice()
		{
			return ZDecimal.Zero;
		}

		protected override ZShort GetItemNumber()
		{
			return ZShort.Zero;
		}

		protected override ZString GetHarmonizedCode()
		{
			return ZString.Empty;
		}

		protected override HarmonisedCodeWrapperCollection GetHarmonizedCodes()
		{
			return new HarmonisedCodeWrapperCollection(Factory);
		}

		protected override PackLine GetFreightPackLine()
		{
			return null;
		}

		protected override LocationWrapper GetOrigin()
		{
			var unloco = ConsignmentBO.BookingHeader?.OriginDepot?.ClosestPort ?? ZString.Empty;

			return new LocationWrapper(unloco, Factory);
		}

		protected override PackProductWrapperCollection GetProducts()
		{
			return PackProductWrapperCollection.Empty;
		}

		protected override PackedItemWrapperCollection GetPackedItems()
		{
			return new PackedItemWrapperCollection(Factory);
		}

		protected override ZShort GetOutterPackageSequence()
		{
			return ZShort.Zero;
		}

		protected override ZShort GetOutterPackagesCount()
		{
			return ZShort.Zero;
		}

		protected override ZShort GetInnerPackagesCount()
		{
			return ZShort.Zero;
		}

		protected override ZString GetRefNumber()
		{
			return ItemBO.HVI_CurrentBarcode;
		}

		protected override ZString GetExportRefNumber()
		{
			return ZString.Empty;
		}

		protected override ZString GetImportRefNumber()
		{
			return ZString.Empty;
		}

		protected override ZString GetPackageReferenceHeaderText()
		{
			return ZString.Empty;
		}

		protected override ZString GetPostcodeBarcodeNumber()
		{
			return ConsigneeCountryISONumericCode + ConsigneePostCode;
		}

		protected override ZString GetConsigneeCountryISONumericCodeCore()
		{
			var consigneeCountry = ConsignmentBO.ConsigneeCountryCode;
			return consigneeCountry?.RN_IsoNumericUNM49Code ?? ZString.Empty;
		}

		protected override ZString GetConsigneePostCodeCore()
		{
			return ConsignmentBO.HVC_ConsigneePostcode;
		}

		protected override ZString GetCustomAttribute1()
		{
			return ZString.Empty;
		}

		protected override ZString GetCustomAttribute2()
		{
			return ZString.Empty;
		}

		protected override ZString GetCustomAttribute3()
		{
			return ZString.Empty;
		}

		protected override ZString GetCustomAttribute4()
		{
			return ZString.Empty;
		}

		protected override ZString GetStarTrack_QRCodeText()
		{
			return new StarTrackQRCodeTextProvider(this, Parent, null).StarTrackQRCodeText();
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
			return ZDecimal.Zero;
		}

		protected override ZDecimal GetCustomDecimal2()
		{
			return ZDecimal.Zero;
		}

		protected override ZBool GetCustomFlag1()
		{
			return ZBool.False;
		}

		protected override ZBool GetCustomFlag2()
		{
			return ZBool.False;
		}

		protected override ZString GetIndent()
		{
			return ZString.Empty;
		}

		protected override ZString GetDisplayOrder()
		{
			return ZString.Empty;
		}

		protected override ZBool GetIsTopLevelPackage()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsTopLevelNonContainerisedPackage()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsOuterPackage()
		{
			return false;
		}

		protected override ZBool GetIsExclusive()
		{
			return ZBool.False;
		}

		protected override ZBool GetHasSingleProduct()
		{
			return ZBool.False;
		}

		protected override ZBool GetHasPackedItem()
		{
			return ZBool.False;
		}

		protected override PackedItemWrapper GetPackedItem()
		{
			return null;
		}

		protected override ZInt GetInners()
		{
			return ZInt.Zero;
		}

		protected override ZString GetInnersDetail()
		{
			return ZString.Empty;
		}

		protected override ZInt GetPackedItemCount()
		{
			return ZInt.Zero;
		}

		protected override ZBool GetIsExpiryUsed()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsPackingDateUsed()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsPartAttrib1Used()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsPartAttrib2Used()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsPartAttrib3Used()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsTrackedSerialUsed()
		{
			return ZBool.False;
		}

		#region PackageState

		protected override PackageStateWrapper GetPackageState()
		{
			return null;
		}

		#endregion

		#region GetIsPackageIdValidSSCCBarCode

		protected override ZBool GetIsPackageIdValidSSCCBarCode()
		{
			return ItemBO.IsPackageIdValidSSCCBarCode;
		}

		#endregion

		#region PackageBarcodeWithSSCCPrefix

		protected override ZString GetPackageBarcodeWithSSCCPrefixCore()
		{
			const bool useOptimizedEncoding = true;
			const bool useGS1BarCode = true;

			return new TextBarcode(RefNumber, useOptimizedEncoding, useGS1BarCode).TextAs128sFontString;
		}

		#endregion

		protected override PackageAuditWrapper GetMostRecentAudit()
		{
			return null;
		}
	}
}
