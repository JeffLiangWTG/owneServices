using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperFromCartagePackage))]
	sealed class PackageWrapperFromCartagePackageTest : PackageWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			PackageWrapper wrapperEmpty = (PackageWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Packages.Value", ZDecimal.Zero, wrapperEmpty.Packages.Value);
			AssertEquals("wrapperEmpty.Packages.Unit.Code", Core.Constants.PkgUnit.Pallet, wrapperEmpty.Packages.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnPackages.Value", ZDecimal.Zero, wrapperEmpty.OutturnedPackages.Value);
			AssertEquals("wrapperEmpty.OutturnPackages.Unit.Code", Core.Constants.PkgUnit.Pallet, wrapperEmpty.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.PillagedPackages.Value", ZDecimal.Zero, wrapperEmpty.PillagedPackages.Value);
			AssertEquals("wrapperEmpty.PillagedPackages.Unit.Code", Core.Constants.PkgUnit.Pallet, wrapperEmpty.PillagedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.DamagedPackages.Value", ZDecimal.Zero, wrapperEmpty.DamagedPackages.Value);
			AssertEquals("wrapperEmpty.DamagedPackages.Unit.Code", Core.Constants.PkgUnit.Pallet, wrapperEmpty.DamagedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.CartonGroupAndSize", ZString.Empty, wrapperEmpty.CartonGroupAndSize);
			AssertEquals("wrapperEmpty.ContainerNo", ZString.Empty, wrapperEmpty.ContainerNo);
			AssertEquals("wrapperEmpty.ContainerJobID", ZString.Empty, wrapperEmpty.ContainerJobID);
			AssertEquals("wrapperEmpty.HouseBill", ZString.Empty, wrapperEmpty.HouseBill);
			AssertEquals("wrapperEmpty.MasterBill", ZString.Empty, wrapperEmpty.MasterBill);
			AssertEquals("wrapperEmpty.UNDGSubstance", 0, wrapperEmpty.UNDGSubstances.Count);
			AssertEquals("wrapperEmpty.Volume.Value", ZDecimal.Zero, wrapperEmpty.Volume.Value);
			AssertEquals("wrapperEmpty.Volume.Unit.Code", "M3", wrapperEmpty.Volume.Unit.Code);
			AssertEquals("wrapperEmpty.Weight.Value", ZDecimal.Zero, wrapperEmpty.Weight.Value);
			AssertEquals("wrapperEmpty.Weight.Unit.Code", "KG", wrapperEmpty.Weight.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnWeight.Value", ZDecimal.Zero, wrapperEmpty.OutturnedWeight.Value);
			AssertEquals("wrapperEmpty.OutturnWeight.Unit.Code", "KG", wrapperEmpty.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperEmpty.Description", ZString.Empty, wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.MarksAndNumbers", ZString.Empty, wrapperEmpty.MarksAndNumbers);
			AssertEquals("wrapperEmpty.Commodity.Code", ZString.Empty, wrapperEmpty.Commodity.Code);
			AssertEquals("wrapperEmpty.Height", ZDecimal.Zero, wrapperEmpty.Dimensions.Height);
			AssertEquals("wrapperEmpty.Width", ZDecimal.Zero, wrapperEmpty.Dimensions.Width);
			AssertEquals("wrapperEmpty.Length", ZDecimal.Zero, wrapperEmpty.Dimensions.Length);
			AssertEquals("wrapperEmpty.Dimension.Unit.Code", "M", wrapperEmpty.Dimensions.Unit.Code);
			AssertEquals("wrapperEmpty.RefNumber", ZString.Empty, wrapperEmpty.RefNumber);
			AssertEquals("wrapperEmpty.ExportRefNumber", ZString.Empty, wrapperEmpty.ExportRefNumber);
			AssertEquals("wrapperEmpty.ImportRefNumber", ZString.Empty, wrapperEmpty.ImportRefNumber);
			AssertEquals("wrapperEmpty.PackageBarcode", ZString.Empty, wrapperEmpty.PackageBarcode);
			AssertEquals("wrapperEmpty.PackageBarcodeWithOptimisedEncoding", ZString.Empty, wrapperEmpty.PackageBarcodeWithOptimisedEncoding);
			AssertEquals("wrapperEmpty.Parent", null, wrapperEmpty.Parent);
			AssertEquals("wrapperEmpty.Products", PackProductWrapperCollection.Empty, wrapperEmpty.Products);
			AssertEquals("wrapperEmpty.Indent", "", wrapperEmpty.Indent);
			AssertEquals("wrapperEmpty.Inners", 0, wrapperEmpty.Inners);
			AssertEquals("wrapperEmpty.InnersDetail", "", wrapperEmpty.InnersDetail);
			AssertEquals("wrapperEmpty.PackedItemCount", 0, wrapperEmpty.PackedItemCount);
			AssertEquals("wrapperEmpty.IsExclusive", false, wrapperEmpty.IsExclusive);
			AssertEquals("wrapperEmpty.IsExpiryUsed", false, wrapperEmpty.IsExpiryUsed);
			AssertEquals("wrapperEmpty.IsPackingDateUsed", false, wrapperEmpty.IsPackingDateUsed);
			AssertEquals("wrapperEmpty.IsPartAttrib1Used", false, wrapperEmpty.IsPartAttrib1Used);
			AssertEquals("wrapperEmpty.IsPartAttrib2Used", false, wrapperEmpty.IsPartAttrib2Used);
			AssertEquals("wrapperEmpty.IsPartAttrib3Used", false, wrapperEmpty.IsPartAttrib3Used);
			AssertEquals("wrapperEmpty.IsTrackedSerialUsed", false, wrapperEmpty.IsTrackedSerialUsed);
			AssertEquals("wrapperEmpty.DisplayOrder", "", wrapperEmpty.DisplayOrder);
			AssertEquals("wrapperEmpty.IsTopLevelPackage", true, wrapperEmpty.IsTopLevelPackage);
			AssertEquals("wrapperEmpty.HasSingleProduct", false, wrapperEmpty.HasSingleProduct);
			AssertEquals("wrapperEmpty.HasPackedItem", false, wrapperEmpty.HasPackedItem);
			AssertNull("wrapperEmpty.PackedItem", wrapperEmpty.PackedItem);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumber", "", wrapperEmpty.PostcodeBarcodeNumber);
			AssertEquals("wrapperEmpty.OutterPackageSequence", ZShort.Zero, wrapperEmpty.OutterPackageSequence);
			AssertEquals("wrapperEmpty.OutterPackagesCount", ZShort.Zero, wrapperEmpty.OutterPackagesCount);
			AssertEquals("wrapperEmpty.PickLocation", ZString.Empty, wrapperEmpty.PickLocation);
			AssertEquals("wrapperEmpty.PickMethod", ZString.Empty, wrapperEmpty.PickMethod);
		}

		public override void TestWrapperMappingFull()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			cartage.JJ_GoodsDescription = "GoodsDescription";
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();

			move1.EW_BookedPackCount = 23;
			move1.EW_F3_NKPackType = "PKG";

			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "123";
			move1.UNDGs.AddNew().DI_DG = substance.PK;

			move1.EW_BookedHeight = 1.11m;
			move1.EW_BookedLength = 2.22m;
			move1.EW_BookedWidth = 3.33m;
			move1.EW_DimUnit = "M";
			move1.EW_VolumeUQ = "M3";
			move1.EW_BookedVolume = 200m;
			move1.EW_BookedWeight = 300m;
			move1.EW_WeightUQ = "LB";

			PackageWrapper wrapperFull = new PackageWrapperFromCartagePackage(move1, Factory);
			AssertEquals("wrapperFull.Packages.Value", 23m, wrapperFull.Packages.Value);
			AssertEquals("wrapperFull.Packages.Unit.Code", "PKG", wrapperFull.Packages.Unit.Code);
			AssertEquals("wrapperFull.OutturnPackages.Value", 23m, wrapperFull.OutturnedPackages.Value);
			AssertEquals("wrapperFull.OutturnPackages.Unit.Code", "PKG", wrapperFull.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperFull.PillagedPackages.Value", 0m, wrapperFull.PillagedPackages.Value);
			AssertEquals("wrapperFull.PillagedPackages.Unit.Code", "PKG", wrapperFull.PillagedPackages.Unit.Code);
			AssertEquals("wrapperFull.DamagedPackages.Value", 0m, wrapperFull.DamagedPackages.Value);
			AssertEquals("wrapperFull.DamagedPackages.Unit.Code", "PKG", wrapperFull.DamagedPackages.Unit.Code);
			AssertEquals("wrapperFull.CartonGroupAndSize", ZString.Empty, wrapperFull.CartonGroupAndSize);
			AssertEquals("wrapperFull.ContainerNo", "", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerJobID", "", wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.HouseBill", "", wrapperFull.HouseBill);
			AssertEquals("wrapperFull.MasterBill", "", wrapperFull.MasterBill);
			AssertEquals("wrapperFull.UNDGSubstances[0].UNNumber", "123", wrapperFull.UNDGSubstances[0].UNNumber);
			AssertEquals("wrapperFull.Volume.Value", 200m, wrapperFull.Volume.Value);
			AssertEquals("wrapperFull.Volume.Unit.Code", "M3", wrapperFull.Volume.Unit.Code);
			AssertEquals("wrapperFull.OutturnVolume.Value", 200m, wrapperFull.OutturnedVolume.Value);
			AssertEquals("wrapperFull.OutturnVolume.Unit.Code", "M3", wrapperFull.OutturnedVolume.Unit.Code);
			AssertEquals("wrapperFull.Weight.Value", 300.0m, wrapperFull.Weight.Value);
			AssertEquals("wrapperFull.Weight.Unit.Code", "LB", wrapperFull.Weight.Unit.Code);
			AssertEquals("wrapperFull.OutturnWeight.Value", 300.0m, wrapperFull.OutturnedWeight.Value);
			AssertEquals("wrapperFull.OutturnWeight.Unit.Code", "LB", wrapperFull.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperFull.Description", "", wrapperFull.Description);
			AssertEquals("wrapperFull.MarksAndNumbers", "", wrapperFull.MarksAndNumbers);
			AssertEquals("wrapperFull.Commodity.Description", "", wrapperFull.Commodity.Description);
			AssertEquals("wrapperFull.OutturnComment", "", wrapperFull.OutturnComment);
			AssertEquals("wrapperFull.Height", 1.11m, wrapperFull.Dimensions.Height);
			AssertEquals("wrapperFull.Length", 2.22m, wrapperFull.Dimensions.Length);
			AssertEquals("wrapperFull.Width", 3.33m, wrapperFull.Dimensions.Width);
			AssertEquals("wrapperFull.DimensionUnit", "M", wrapperFull.Dimensions.Unit.Code);
			AssertEquals("wrapperFull.Parent", "GoodsDescription", wrapperFull.Parent.GoodsDescription);
			AssertEquals("wrapperFull.LinePrice", ZDecimal.Zero, wrapperFull.LinePrice);
			AssertEquals("wrapperFull.ItemNumber", ZShort.Zero, wrapperFull.ItmNumber);
			AssertEquals("wrapperFull.HarmonizedCode", ZString.Empty, wrapperFull.HarmonizedCode);
			AssertEquals("wrapperFull.Origin", ZString.Empty, wrapperFull.Origin.UNLOCO);
			AssertEquals("wrapperFull.CustomText1", ZString.Empty, wrapperFull.CustomAttribute1);
			AssertEquals("wrapperFull.CustomText2", ZString.Empty, wrapperFull.CustomAttribute2);
			AssertEquals("wrapperFull.CustomText3", ZString.Empty, wrapperFull.CustomAttribute3);
			AssertEquals("wrapperFull.CustomText4", ZString.Empty, wrapperFull.CustomAttribute4);
			AssertEquals("wrapperFull.CustomDate1", ZDateTime.Empty, wrapperFull.CustomDate1);
			AssertEquals("wrapperFull.CustomDate2", ZDateTime.Empty, wrapperFull.CustomDate2);
			AssertEquals("wrapperFull.CustomDecimal1", ZDecimal.Zero, wrapperFull.CustomDecimal1);
			AssertEquals("wrapperFull.CustomDecimal2", ZDecimal.Zero, wrapperFull.CustomDecimal2);
			AssertEquals("wrapperFull.CustomFlag1", ZBool.False, wrapperFull.CustomFlag1);
			AssertEquals("wrapperFull.CustomFlag2", ZBool.False, wrapperFull.CustomFlag2);
			AssertEquals("wrapperFull.Products", PackProductWrapperCollection.Empty, wrapperFull.Products);
			AssertEquals("wrapperFull.Indent", "", wrapperFull.Indent);
			AssertEquals("wrapperFull.Inners", 0, wrapperFull.Inners);
			AssertEquals("wrapperFull.InnersDetail", "", wrapperFull.InnersDetail);
			AssertEquals("wrapperFull.OutterPackageSequence", ZShort.Zero, wrapperFull.OutterPackageSequence);
			AssertEquals("wrapperFull.OutterPackagesCount", ZShort.Zero, wrapperFull.OutterPackagesCount);
			AssertEquals("wrapperFull.PackedItemCount", 0, wrapperFull.PackedItemCount);
			AssertEquals("wrapperFull.IsExclusive", false, wrapperFull.IsExclusive);
			AssertEquals("wrapperFull.IsExpiryUsed", false, wrapperFull.IsExpiryUsed);
			AssertEquals("wrapperFull.IsPackingDateUsed", false, wrapperFull.IsPackingDateUsed);
			AssertEquals("wrapperFull.IsPartAttrib1Used", false, wrapperFull.IsPartAttrib1Used);
			AssertEquals("wrapperFull.IsPartAttrib2Used", false, wrapperFull.IsPartAttrib2Used);
			AssertEquals("wrapperFull.IsPartAttrib3Used", false, wrapperFull.IsPartAttrib3Used);
			AssertEquals("wrapperFull.IsTrackedSerialUsed", false, wrapperFull.IsTrackedSerialUsed);
			AssertEquals("wrapperFull.DisplayOrder", "", wrapperFull.DisplayOrder);
			AssertEquals("wrapperFull.IsTopLevelPackage", true, wrapperFull.IsTopLevelPackage);
			AssertEquals("wrapperFull.HasSingleProduct", false, wrapperFull.HasSingleProduct);
			AssertEquals("wrapperFull.HasPackedItem", false, wrapperFull.HasPackedItem);
			AssertNull("wrapperFull.PackedItem", wrapperFull.PackedItem);
			AssertNull("wrapperFull.FreightPackLine", wrapperFull.FreightPackLine);
			AssertEquals("wrapperFull.PickLocation", ZString.Empty, wrapperFull.PickLocation);
			AssertEquals("wrapperFull.PickMethod", ZString.Empty, wrapperFull.PickMethod);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageWrapperFromCartagePackage(null, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Commodity : 
CommonCurrency : 
Container : 
DamagedPackages : 
DamagedReason : 
Dimensions : 2.22 x 3.33 x 1.11 M
FumigatedPackages : 
HandlingUnit :  is null
HeatTreatedPackages : 
ISPMPalletPackages : 
MostRecentAudit :  is null
NonStackablePackages : 
Origin : 
OutturnedPackages : 23 PKG
OutturnedVolume : 200.000 M3
OutturnedWeight : 300.0 LB
PackageOrderReference : 
Packages : 23 PKG
PackageState :  is null
PackedItem :  is null
Parent : 
PillagedPackages : 
Registry : (No Default Field Value Available on Registry)
TopLevelHandlingUnit :  is null
TopLoadOnlyPackages : 
UOMType : 
Volume : 200.000 M3
Weight : 300.0 LB
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			CommonCartage cartage = Factory.New<CommonCartage>();
			CommonBookedCtgMove move1 = cartage.LooseBookedMoves.AddNew();

			move1.EW_BookedPackCount = 23;
			move1.EW_F3_NKPackType = "PKG";

			UNDGSubstance substance = Factory.New<UNDGSubstance>();
			substance.DG_UNNO = "123";
			move1.UNDGs.AddNew().DI_DG = substance.PK;

			move1.EW_BookedHeight = 1.11m;
			move1.EW_BookedLength = 2.22m;
			move1.EW_BookedWidth = 3.33m;
			move1.EW_DimUnit = "M";
			move1.EW_VolumeUQ = "M3";
			move1.EW_BookedVolume = 200m;
			move1.EW_BookedWeight = 300m;
			move1.EW_WeightUQ = "LB";

			return new PackageWrapperFromCartagePackage(move1, Factory);
		}
	}
}
