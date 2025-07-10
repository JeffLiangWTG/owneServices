using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperFromCustomsPackage))]
	sealed class PackageWrapperFromCustomsPackageTest : PackageWrapperTest
	{
		public override void TestWrapperMappingsEmpty()
		{
			PackageWrapper wrapperEmpty = (PackageWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Packages.Value", ZDecimal.Zero, wrapperEmpty.Packages.Value);
			AssertEquals("wrapperEmpty.Packages.Unit.Code", ZString.Empty, wrapperEmpty.Packages.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnPackages.Value", ZDecimal.Zero, wrapperEmpty.OutturnedPackages.Value);
			AssertEquals("wrapperEmpty.OutturnPackages.Unit.Code", ZString.Empty, wrapperEmpty.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.PillagedPackages.Value", ZDecimal.Zero, wrapperEmpty.PillagedPackages.Value);
			AssertEquals("wrapperEmpty.PillagedPackages.Unit.Code", ZString.Empty, wrapperEmpty.PillagedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.DamagedPackages.Value", ZDecimal.Zero, wrapperEmpty.DamagedPackages.Value);
			AssertEquals("wrapperEmpty.DamagedPackages.Unit.Code", ZString.Empty, wrapperEmpty.DamagedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.CartonGroupAndSize", ZString.Empty, wrapperEmpty.CartonGroupAndSize);
			AssertEquals("wrapperEmpty.ContainerNo", ZString.Empty, wrapperEmpty.ContainerNo);
			AssertEquals("wrapperEmpty.ContainerJobID", ZString.Empty, wrapperEmpty.ContainerJobID);
			AssertEquals("wrapperEmpty.HouseBill", ZString.Empty, wrapperEmpty.HouseBill);
			AssertEquals("wrapperEmpty.MasterBill", ZString.Empty, wrapperEmpty.MasterBill);
			AssertEquals("wrapperEmpty.UNDGSubstances", 0, wrapperEmpty.UNDGSubstances.Count);
			AssertEquals("wrapperEmpty.Volume.Value", ZDecimal.Zero, wrapperEmpty.Volume.Value);
			AssertEquals("wrapperEmpty.Volume.Unit.Code", ZString.Empty, wrapperEmpty.Volume.Unit.Code);
			AssertEquals("wrapperEmpty.Weight.Value", ZDecimal.Zero, wrapperEmpty.Weight.Value);
			AssertEquals("wrapperEmpty.Weight.Unit.Code", ZString.Empty, wrapperEmpty.Weight.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnWeight.Value", ZDecimal.Zero, wrapperEmpty.OutturnedWeight.Value);
			AssertEquals("wrapperEmpty.OutturnWeight.Unit.Code", ZString.Empty, wrapperEmpty.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperEmpty.Description", ZString.Empty, wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.MarksAndNumbers", ZString.Empty, wrapperEmpty.MarksAndNumbers);
			AssertEquals("wrapperEmpty.Commodity.Code", ZString.Empty, wrapperEmpty.Commodity.Code);
			AssertEquals("wrapperEmpty.Height", ZDecimal.Zero, wrapperEmpty.Dimensions.Height);
			AssertEquals("wrapperEmpty.Width", ZDecimal.Zero, wrapperEmpty.Dimensions.Width);
			AssertEquals("wrapperEmpty.Length", ZDecimal.Zero, wrapperEmpty.Dimensions.Length);
			AssertEquals("wrapperEmpty.Dimension.Unit.Code", ZString.Empty, wrapperEmpty.Dimensions.Unit.Code);
			AssertEquals("wrapperEmpty.RefNumber", ZString.Empty, wrapperEmpty.RefNumber);
			AssertEquals("wrapperEmpty.ExportRefNumber", ZString.Empty, wrapperEmpty.ExportRefNumber);
			AssertEquals("wrapperEmpty.ImportRefNumber", ZString.Empty, wrapperEmpty.ImportRefNumber);
			AssertEquals("wrapperEmpty.PackageBarcode", ZString.Empty, wrapperEmpty.PackageBarcode);
			AssertEquals("wrapperEmpty.PackageBarcodeWithOptimisedEncoding", ZString.Empty, wrapperEmpty.PackageBarcodeWithOptimisedEncoding);
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
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "HOUSECAT";
			declaration.JE_MasterBill = "MASTERCAT";
			Bill houseBill = declaration.PrimaryHouseBill;
			AssertNotNull("Precondition: declaration.PrimaryBill should not be null", houseBill.CU_HouseBill);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000026";

			BasePackingGroup packingGroup = declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			packingGroup.CR_CO_Container = container.PK;

			BasePackage package = declaration.Packages.AddNew();
			package.CW_CR_HouseContainer = packingGroup.PK;
			package.CW_PackQty = 23;
			package.CW_PackType = "PK";

			PackageWrapper wrapperFull = new PackageWrapperFromCustomsPackage(package, Factory);
			AssertEquals("wrapperFull.Packages.Value", 23m, wrapperFull.Packages.Value);
			AssertEquals("wrapperFull.Packages.Unit.Code", "PK", wrapperFull.Packages.Unit.Code);
			AssertEquals("wrapperFull.OutturnedPackages.Value", ZDecimal.Zero, wrapperFull.OutturnedPackages.Value);
			AssertEquals("wrapperFull.OutturnedPackages.Unit.Code", ZString.Empty, wrapperFull.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperFull.PillagedPackages.Value", ZDecimal.Zero, wrapperFull.PillagedPackages.Value);
			AssertEquals("wrapperFull.PillagedPackages.Unit.Code", ZString.Empty, wrapperFull.PillagedPackages.Unit.Code);
			AssertEquals("wrapperFull.DamagedPackages.Value", ZDecimal.Zero, wrapperFull.DamagedPackages.Value);
			AssertEquals("wrapperFull.DamagedPackages.Unit.Code", ZString.Empty, wrapperFull.DamagedPackages.Unit.Code);
			AssertEquals("wrapperFull.CartonGroupAndSize", ZString.Empty, wrapperFull.CartonGroupAndSize);
			AssertEquals("wrapperFull.Volume.Value", ZDecimal.Zero, wrapperFull.Volume.Value);
			AssertEquals("wrapperFull.Volume.Unit.Code", ZString.Empty, wrapperFull.Volume.Unit.Code);
			AssertEquals("wrapperFull.OutturnedVolume.Value", ZDecimal.Zero, wrapperFull.OutturnedVolume.Value);
			AssertEquals("wrapperFull.OutturnedVolume.Unit.Code", ZString.Empty, wrapperFull.OutturnedVolume.Unit.Code);
			AssertEquals("wrapperFull.Weight.Value", ZDecimal.Zero, wrapperFull.Weight.Value);
			AssertEquals("wrapperFull.Weight.Unit.Code", ZString.Empty, wrapperFull.Weight.Unit.Code);
			AssertEquals("wrapperFull.OutturnedWeight.Value", ZDecimal.Zero, wrapperFull.OutturnedWeight.Value);
			AssertEquals("wrapperFull.OutturnedWeight.Unit.Code", ZString.Empty, wrapperFull.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperFull.MarksAndNumbers", ZString.Empty, wrapperFull.MarksAndNumbers);
			AssertEquals("wrapperFull.ContainerNo", "OOCL0000026", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerJobID", "OOCL0000026", wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.HouseBill", "HOUSECAT", wrapperFull.HouseBill);
			AssertEquals("wrapperFull.MasterBill", "MASTERCAT", wrapperFull.MasterBill);
			AssertEquals("wrapperFull.UNDGSubstances", 0, wrapperFull.UNDGSubstances.Count);
			AssertEquals("wrapperFull.Description", ZString.Empty, wrapperFull.Description);
			AssertEquals("wrapperFull.Commodity.Code", ZString.Empty, wrapperFull.Commodity.Code);
			AssertEquals("wrapperFull.OutturnComment", ZString.Empty, wrapperFull.OutturnComment);
			AssertEquals("wrapperFull.Height", ZDecimal.Zero, wrapperFull.Dimensions.Height);
			AssertEquals("wrapperFull.Width", ZDecimal.Zero, wrapperFull.Dimensions.Width);
			AssertEquals("wrapperFull.Length", ZDecimal.Zero, wrapperFull.Dimensions.Length);
			AssertEquals("wrapperFull.Dimension.Unit.Code", ZString.Empty, wrapperFull.Dimensions.Unit.Code);
			AssertEquals("wrapperFull.Parent", "HOUSECAT", wrapperFull.Parent.HouseBill);
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
			AssertEquals("wrapperFull.PackedItemCount", 0, wrapperFull.PackedItemCount);
			AssertEquals("wrapperFull.IsExpiryUsed", false, wrapperFull.IsExpiryUsed);
			AssertEquals("wrapperFull.IsExclusive", false, wrapperFull.IsExclusive);
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
			AssertEquals("wrapperFull.PackageSequence", ZShort.Zero, wrapperFull.OutterPackageSequence);
			AssertEquals("wrapperFull.OutterPackagesCount", ZShort.Zero, wrapperFull.OutterPackagesCount);
			AssertEquals("wrapperFull.PickLocation", ZString.Empty, wrapperFull.PickLocation);
			AssertEquals("wrapperFull.PickMethod", ZString.Empty, wrapperFull.PickMethod);
			AssertEquals("wrapperFull.Container.ContainerNo", "OOCL0000026", wrapperFull.Container.ContainerNo);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Commodity : 
CommonCurrency : 
Container : OOCL0000026
DamagedPackages : 
DamagedReason : 
Dimensions : 
FumigatedPackages : 
HandlingUnit :  is null
HeatTreatedPackages : 
ISPMPalletPackages : 
MostRecentAudit :  is null
NonStackablePackages : 
Origin : 
OutturnedPackages : 
OutturnedVolume : 
OutturnedWeight : 
PackageOrderReference : 
Packages : 23 PK
PackageState :  is null
PackedItem :  is null
Parent : 
PillagedPackages : 
Registry : (No Default Field Value Available on Registry)
TopLevelHandlingUnit :  is null
TopLoadOnlyPackages : 
UOMType : 
Volume : 
Weight :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_HouseBill = "HOUSECAT";
			declaration.JE_MasterBill = "MASTERCAT";
			Bill houseBill = declaration.PrimaryHouseBill;
			AssertNotNull("Precondition: declaration.PrimaryBill should not be null", houseBill.CU_HouseBill);

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "OOCL0000026";

			BasePackingGroup packingGroup = declaration.PackingGroups.AddNew();
			packingGroup.CR_CU_HouseBill = houseBill.PK;
			packingGroup.CR_CO_Container = container.PK;

			BasePackage package = declaration.Packages.AddNew();
			package.CW_CR_HouseContainer = packingGroup.PK;
			package.CW_PackQty = 23;
			package.CW_PackType = "PK";

			return new PackageWrapperFromCustomsPackage(package, Factory);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageWrapperFromCustomsPackage(null, Factory);
		}
	}
}
