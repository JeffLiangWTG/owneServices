using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperFromOneOffContainer : PackageWrapper
	{
		public PackageWrapperFromOneOffContainer(RateOneOffPackLine pack, BusinessObjectFactory factory)
			: base(pack, factory)
		{
			this.pack = pack;
		}

		protected override ZString GetDescription()
		{
			return "";
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			return new UNDGSubstanceWrapperCollection(Factory);
		}

		protected override PackQTYWrapper GetPackages()
		{
			return new PackQTYWrapper(pack.TPL_PackLineCount, pack.TPL_F3_NKPackType, pack.Lookups.RefPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetOutturnedPackages()
		{
			return new PackQTYWrapper(0, pack.TPL_F3_NKPackType, pack.Lookups.RefPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetPillagedPackages()
		{
			return new PackQTYWrapper(0, pack.TPL_F3_NKPackType, pack.Lookups.RefPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		protected override PackQTYWrapper GetDamagedPackages()
		{
			return new PackQTYWrapper(0, pack.TPL_F3_NKPackType, pack.Lookups.RefPackTypes.GetAsCodeDescriptionPair(), Factory);
		}

		protected override WeightWrapper GetWeight()
		{
			return new WeightWrapper(pack.TPL_Weight, pack.TPL_WeightUQ, RateOneOffPackLineSchema.TPL_Weight.Scale, pack.Lookups.WeightUnits, Factory);
		}

		protected override WeightWrapper GetOutturnedWeight()
		{
			return new WeightWrapper(0, pack.TPL_WeightUQ, RateOneOffPackLineSchema.TPL_Weight.Scale, pack.Lookups.WeightUnits, Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			return new VolumeWrapper(pack.TPL_Volume, pack.TPL_VolumeUQ, pack.Lookups.VolumeUnits, Factory);
		}

		protected override VolumeWrapper GetOutturnedVolume()
		{
			return new VolumeWrapper(0, pack.TPL_VolumeUQ, pack.Lookups.VolumeUnits, Factory);
		}

		protected override DimensionsWrapper GetDimensions()
		{
			return new DimensionsWrapper(pack.TPL_Length, pack.TPL_Width, pack.TPL_Height, pack.TPL_DimensionUQ, 3, pack.Lookups.DimensionUnits, Factory);
		}

		protected override ZInt GetPackingOrder()
		{
			return 0;
		}

		protected override ZString GetCartonGroupAndSize()
		{
			return ZString.Empty;
		}

		protected override ContainerWrapper GetContainer()
		{
			return null;
		}

		protected override PackLine GetFreightPackLine()
		{
			return null;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return "";
		}

		protected override ZString GetContainerNo()
		{
			return "";
		}

		protected override ZString GetContainerJobID()
		{
			return "";
		}

		protected override ZString GetHouseBill()
		{
			return "";
		}

		protected override ZString GetMasterBill()
		{
			return "";
		}

		protected override ZString GetOutturnComment()
		{
			return "";
		}

		protected override CodeAndDescriptionWrapper GetCommodity()
		{
			RateOneOffShipment parent = pack.Parent;

			if (parent != null && !parent.TT_RH_NKCommodity.IsEmpty)
			{
				var commodityCode = parent.TT_RH_NKCommodity;
				var refCode = Factory.LoadTop1<RefCommodityCode>(new ZQuery(RefCommodityCodeSchema.RH_Code, commodityCode));

				if (refCode != null)
				{
					var codePair = new CodeDescriptionPairList();
					codePair.AddPair(commodityCode, refCode.RH_DescriptionMultilingual);

					return new CodeAndDescriptionWrapper(commodityCode, codePair, Factory);
				}
			}

			return CodeAndDescriptionWrapper.Empty;
		}

		protected override CodeAndDescriptionWrapper GetDamagedReason()
		{
			return CodeAndDescriptionWrapper.Empty;
		}

		protected override ZString GetRefNumber()
		{
			return ZString.Empty;
		}

		protected override ZString GetExportRefNumber()
		{
			return ZString.Empty;
		}

		protected override ZString GetImportRefNumber()
		{
			return ZString.Empty;
		}

		protected override ZString GetPackageReferenceHeaderText() => ZString.Empty;

		protected override FreightWrapper GetParent()
		{
			RateOneOffShipment oneOff = pack.Parent;

			if (oneOff == null)
			{
				return null;
			}

			Quote quote = oneOff.ParentQuote;

			if (quote == null)
			{
				return null;
			}

			return new FreightWrapperFromOneOffQuote(quote, Factory);
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

		protected override LocationWrapper GetOrigin()
		{
			return new LocationWrapper(ZString.Empty, Factory);
		}

		protected override PackProductWrapperCollection GetProducts()
		{
			return PackProductWrapperCollection.Empty;
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
			return "";
		}

		protected override ZString GetDisplayOrder()
		{
			return "";
		}

		protected override ZBool GetIsTopLevelPackage()
		{
			return true;
		}

		protected override ZBool GetIsTopLevelNonContainerisedPackage()
		{
			return true;
		}

		protected override ZBool GetIsOuterPackage()
		{
			return false;
		}

		protected override ZBool GetIsExclusive()
		{
			return false;
		}

		protected override ZBool GetHasSingleProduct()
		{
			return ZBool.False;
		}

		protected override ZBool GetHasPackedItem()
		{
			return PackedItem != null;
		}

		protected override PackedItemWrapper GetPackedItem()
		{
			return null;
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

		protected override ZString GetPostcodeBarcodeNumber()
		{
			return ZString.Empty;
		}

		readonly RateOneOffPackLine pack;

		protected override ZInt GetInners()
		{
			return ZInt.Zero;
		}

		protected override ZString GetInnersDetail()
		{
			return ZString.Empty;
		}

		protected override ZString GetStarTrack_QRCodeText()
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

		protected override PackageAuditWrapper GetMostRecentAudit()
		{
			return null;
		}

		#region PackageState

		protected override PackageStateWrapper GetPackageState()
		{
			return null;
		}

		#endregion
	}
}
