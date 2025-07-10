using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperFromCustomsPackage : PackageWrapper
	{
		public PackageWrapperFromCustomsPackage(BasePackage packageBO, BusinessObjectFactory factory)
			: base(packageBO, factory)
		{
			PackageBO = packageBO ?? factory.GetNull<BasePackage>();
		}
		readonly BasePackage PackageBO;

		protected override ZString GetDescription()
		{
			return "";
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			UNDGSubstanceWrapperCollection result = new UNDGSubstanceWrapperCollection(Factory);
			foreach (UNDGDataItem dg in PackageBO.UNDGs)
			{
				if (dg.Substance != null || !dg.DI_IMOClass.IsEmpty)
				{
					UNDGSubstanceWrapper wrapper = new UNDGSubstanceWrapper(dg, Factory);
					wrapper.ContainingPackage = this;
					result.Add(wrapper);
				}
			}
			return result;
		}

		protected override PackQTYWrapper GetPackages()
		{
			return new PackQTYWrapper(PackageBO.CW_PackQty, PackageBO.CW_PackType, PackageBO.PackTypeList, Factory);
		}

		protected override ZString GetCartonGroupAndSize()
		{
			return ZString.Empty;
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

		protected override VolumeWrapper GetVolume()
		{
			return VolumeWrapper.Empty;
		}

		protected override VolumeWrapper GetOutturnedVolume()
		{
			return VolumeWrapper.Empty;
		}

		protected override WeightWrapper GetWeight()
		{
			return WeightWrapper.Empty;
		}

		protected override WeightWrapper GetOutturnedWeight()
		{
			return WeightWrapper.Empty;
		}

		protected override DimensionsWrapper GetDimensions()
		{
			return DimensionsWrapper.Empty;
		}

		protected override ZInt GetPackingOrder()
		{
			return 0; // Not required
		}

		protected override PackLine GetFreightPackLine()
		{
			return null;
		}

		protected override ContainerWrapper GetContainer()
		{
			return new ContainerWrapperFromCustoms(PackageBO.PackingGroup?.Container, Factory);
		}

		protected override ZString GetMarksAndNumbers()
		{
			return PackageBO.CW_MarksAndNos;
		}

		protected override ZString GetContainerNo()
		{
			return GetBasePackageContainerNo();
		}

		protected override ZString GetContainerJobID()
		{
			return GetBasePackageContainerNo();
		}

		ZString GetBasePackageContainerNo()
		{
			return PackageBO.CW_ContainerNoOrEquipmentNo;
		}

		protected override ZString GetHouseBill()
		{
			return PackageBO.Bill != null && PackageBO.Bill.IsHouseBill ? PackageBO.Bill.CU_BillNum : ZString.Empty;
		}

		protected override ZString GetMasterBill()
		{
			return PackageBO.Bill != null ? PackageBO.Bill.CU_MasterBill : ZString.Empty;
		}

		protected override ZString GetOutturnComment()
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
			FreightWrapper[] parents = FreightWrapper.New(PackageBO.Declaration, Factory);
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

		#region PackageState

		protected override PackageStateWrapper GetPackageState()
		{
			return null;
		}

		protected override PackageAuditWrapper GetMostRecentAudit()
		{
			return null;
		}

		#endregion
	}
}
