using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Freight.Business;
using Enterprise.Packing.Business;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	public class PackageWrapperFromPkgPackageHeader : PackageWrapper
	{
		protected PackageWrapperFromPkgPackageHeader(PkgPackage package, BusinessObjectFactory factory)
			: base(package, factory)
		{
			PackageJobBO = (package == null) ? factory.GetNull<PkgPackageJob>() : package.PackageJob;
			PackageHeaderBO = (package == null) ? factory.GetNull<PkgPackageHeader>() : package.GetPackageHeader();
			Package = package ?? factory.GetNull<PkgPackage>();
		}

		public PackageWrapperFromPkgPackageHeader(PkgPackageJob packageJob, PkgPackageHeader packageHeaderBO, BusinessObjectFactory factory)
			: base(packageHeaderBO ?? (BusinessObject)packageHeaderBO, factory)
		{
			PackageJobBO = packageJob;
			PackageHeaderBO = packageHeaderBO ?? factory.GetNull<PkgPackageHeader>();
			Package = (packageHeaderBO == null || packageHeaderBO.CurrentPackageJob == null)
				? factory.GetNull<PkgPackage>()
				: packageHeaderBO.CurrentPackageJob.Packages.FirstOrDefault(p => p.KP_PackageID == packageHeaderBO.KPH_PackageID);
		}

		readonly PkgPackageJob PackageJobBO;
		readonly PkgPackageHeader PackageHeaderBO;
		readonly PkgPackage Package;

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

		protected override ContainerWrapper GetContainer()
		{
			return null;
		}

		protected override ZString GetContainerNo()
		{
			return ZString.Empty;
		}

		protected override ZString GetContainerJobID()
		{
			return ZString.Empty;
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

		protected override PackQTYWrapper GetDamagedPackages()
		{
			return PackQTYWrapper.Empty;
		}

		protected override ZString GetDescription()
		{
			return ZString.Empty;
		}

		protected override DimensionsWrapper GetDimensions()
		{
			return DimensionsWrapper.Empty;
		}

		protected override ZString GetDisplayOrder()
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

		protected override PackLine GetFreightPackLine()
		{
			return null;
		}

		protected override ZString GetHarmonizedCode()
		{
			return ZString.Empty;
		}

		protected override HarmonisedCodeWrapperCollection GetHarmonizedCodes()
		{
			return new HarmonisedCodeWrapperCollection(Factory);
		}

		protected override ZBool GetHasPackedItem()
		{
			return ZBool.False;
		}

		protected override ZBool GetHasSingleProduct()
		{
			return ZBool.False;
		}

		protected override ZString GetHouseBill()
		{
			return ZString.Empty;
		}

		protected override ZString GetIndent()
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

		protected override ZBool GetIsExclusive()
		{
			return ZBool.False;
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

		protected override ZBool GetIsTopLevelNonContainerisedPackage()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsTopLevelPackage()
		{
			return ZBool.False;
		}

		protected override ZBool GetIsOuterPackage()
		{
			return false;
		}

		protected override ZShort GetItemNumber()
		{
			return ZShort.Zero;
		}

		protected override ZDecimal GetLinePrice()
		{
			return ZDecimal.Zero;
		}

		protected override ZString GetMarksAndNumbers()
		{
			return ZString.Empty;
		}

		protected override ZString GetMasterBill()
		{
			return ZString.Empty;
		}

		protected override LocationWrapper GetOrigin()
		{
			return new LocationWrapper("", Factory);
		}

		protected override ZShort GetOutterPackagesCount()
		{
			var result = ZShort.Zero;
			if (PackageJobBO?.ParentJob?.PackageSequenceType == PackageSequenceType.OuterWithLooseID)
			{
				result = (ZShort)(Math.Min(PackageJobBO.Packages.Count(p => !p.KP_KPH_PackageHeader.IsEmpty) + PackageJobBO.LoosePackageIDs.Count, short.MaxValue));
			}
			return result;
		}

		protected override ZShort GetOutterPackageSequence()
		{
			var result = ZShort.Zero;
			if (PackageJobBO?.ParentJob?.PackageSequenceType == PackageSequenceType.OuterWithLooseID)
			{
				if (Package != null)
				{
					result = Package.KP_Sequence;
				}
				else if (PackageHeaderBO != null)
				{
					var pivot = PackageJobBO.LoosePackagePivots.FirstOrDefault(p => p.KPJ_KPH_PackageHeader == PackageHeaderBO.PK);
					if (pivot != null)
					{
						result = pivot.KPJ_Sequence;
					}
				}
			}
			return result;
		}

		protected override ZShort GetInnerPackagesCount()
		{
			return ZShort.Zero;
		}

		protected override ZString GetOutturnComment()
		{
			return ZString.Empty;
		}

		protected override ZString GetStarTrack_QRCodeText()
		{
			return new StarTrackQRCodeTextProvider(this, Parent, this.Package).StarTrackQRCodeText();
		}

		protected override PackQTYWrapper GetOutturnedPackages()
		{
			return PackQTYWrapper.Empty;
		}

		protected override VolumeWrapper GetOutturnedVolume()
		{
			return VolumeWrapper.Empty;
		}

		protected override WeightWrapper GetOutturnedWeight()
		{
			return WeightWrapper.Empty;
		}

		protected override PackQTYWrapper GetPackages()
		{
			return PackQTYWrapper.Empty;
		}

		protected override PackageStateWrapper GetPackageState()
		{
			return null;
		}

		protected override PackedItemWrapper GetPackedItem()
		{
			return null;
		}

		protected override ZInt GetPackedItemCount()
		{
			return ZInt.Zero;
		}

		protected override PackedItemWrapperCollection GetPackedItems()
		{
			return new PackedItemWrapperCollection(Factory);
		}

		protected override ZInt GetPackingOrder()
		{
			return ZInt.Zero;
		}

		protected override FreightWrapper GetParent()
		{
			var wrappers = FreightWrapper.New(PackageJobBO, Factory);
			var wrapper = wrappers.Length > 0 ? wrappers[0] : null;
			var iDocTypeCode = wrapper as IDocTypeCode;
			if (iDocTypeCode != null)
			{
				iDocTypeCode.DocTypeCode = "PPL";
			}

			return wrapper;
		}

		protected override PackQTYWrapper GetPillagedPackages()
		{
			return PackQTYWrapper.Empty;
		}

		#region PostcodeBarcodeNumber

		protected override ZString GetPostcodeBarcodeNumber()
		{
			var parent = Parent;
			return (parent != null && parent.IsDomestic ? ZString.Empty : ConsigneeCountryISONumericCode) + ConsigneePostCode;
		}

		#endregion

		protected override PackProductWrapperCollection GetProducts()
		{
			return PackProductWrapperCollection.Empty;
		}

		#region GetRefNumber

		protected override ZString GetRefNumber()
		{
			return PackageHeaderBO.IsNull ? ZString.Empty : PackageHeaderBO.KPH_PackageID;
		}

		#endregion

		protected override ZBool GetIsPackageIdValidSSCCBarCode()
		{
			return ZBool.False;
		}

		protected override UNDGSubstanceWrapperCollection GetUNDGSubstances()
		{
			return new UNDGSubstanceWrapperCollection(Factory);
		}

		protected override VolumeWrapper GetVolume()
		{
			return VolumeWrapper.Empty;
		}

		protected override WeightWrapper GetWeight()
		{
			return WeightWrapper.Empty;
		}

		protected override PackageAuditWrapper GetMostRecentAudit()
		{
			return null;
		}
	}
}
