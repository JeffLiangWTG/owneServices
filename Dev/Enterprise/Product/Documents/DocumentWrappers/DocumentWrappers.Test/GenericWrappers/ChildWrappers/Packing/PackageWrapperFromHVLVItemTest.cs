using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.eTail.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperFromHVLVItem))]
	sealed class PackageWrapperFromHVLVItemTest : PackageWrapperTest
	{
		public void TestGetDescription()
		{
			const string description = "Nice Stuff!!!";
			AssertEquals(ZString.Empty, Wrapper.Description);

			Item.Consignment.HVC_GoodsDescription = description;
			AssertEquals(description, Wrapper.Description);
		}

		public void TestGetOrigin()
		{
			var header = Factory.New<HVLVBookingHeader>();
			var originDepot = Factory.New<OrgAddress>();
			originDepot.ClosestPort = "AUSYD";
			header.HVH_OA_OriginDepot = originDepot.PK;
			Item.Consignment.HVC_HVH_BookingHeader = header.PK;
			AssertEquals("AUSYD", Wrapper.Origin.UNLOCO);
		}

		public void TestGetPostcodeBarcodeNumber()
		{
			AssertEquals(ZString.Empty, Wrapper.PostcodeBarcodeNumber);
			Item.Consignment.HVC_RN_NKConsigneeCountryCode = "CN";
			Item.Consignment.HVC_ConsigneePostcode = "8888";
			AssertEquals("1568888", Wrapper.PostcodeBarcodeNumber);
		}

		public new void TestIsPackageIdValidSSCCBarCode()
		{
			Assert(!Wrapper.IsPackageIdValidSSCCBarCode);
			Item.HVI_CurrentBarcode = "00667676545577876556";
			Assert(Wrapper.IsPackageIdValidSSCCBarCode);
		}

		public void TestGetPackageBarcodeWithSSCCPrefix()
		{
			AssertEquals(ZString.Empty, Wrapper.PackageBarcodeWithSSCCPrefix);
			Item.HVI_CurrentBarcode = "123456789";
			AssertEquals("ÈÆ1Ã7Mcy9Ê", Wrapper.PackageBarcodeWithSSCCPrefix);
		}

		#region TestConsigneeReferenceValidSSCCBarCode

		public void TestConsigneeReferenceValidSSCCBarCode()
		{
			AssertEquals(false, Wrapper.IsPackageIdValidSSCCBarCode);

			var item = Factory.New<HVLVItem>();
			var wrapper = new PackageWrapperFromHVLVItem(item, Factory);
			item.HVI_CurrentBarcode = "123465454654654659";
			Assert("It's valid SSCC BarCode", wrapper.IsPackageIdValidSSCCBarCode);

			item.HVI_CurrentBarcode = "00123465454654654659";
			Assert("It's valid SSCC BarCode", wrapper.IsPackageIdValidSSCCBarCode);
		}

		#endregion

		#region TestPackageBarcodeWithSSCCPrefix

		public override void TestPackageBarcodeWithSSCCPrefix()
		{
			AssertEquals(ZString.Empty, Wrapper.PackageBarcodeWithSSCCPrefix);

			var item = Factory.New<HVLVItem>();
			var wrapper = new PackageWrapperFromHVLVItem(item, Factory);

			item.HVI_CurrentBarcode = "ab";
			AssertEquals("ÈÆab3Ê", wrapper.PackageBarcodeWithSSCCPrefix);

			item.HVI_CurrentBarcode = "0012";
			AssertEquals("ÉÆ¯,EÊ", wrapper.PackageBarcodeWithSSCCPrefix); // numbers should be compressed (using the optimisation flag)
		}

		#endregion

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (PackageWrapper)GetNewDocumentWrapper();
			CombineAssertions(() =>
			{
				AssertEquals("wrapperEmpty.Packages.Value", 1m, wrapperEmpty.Packages.Value);
				AssertEquals("wrapperEmpty.Packages.Unit.Code", "PKG", wrapperEmpty.Packages.Unit.Code);
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
				AssertEquals("wrapperEmpty.UNDGSubstance", 0, wrapperEmpty.UNDGSubstances.Count);
				AssertEquals("wrapperEmpty.Volume.Value", ZDecimal.Zero, wrapperEmpty.Volume.Value);
				AssertEquals("wrapperEmpty.Volume.Unit.Code", "M3", wrapperEmpty.Volume.Unit.Code);
				AssertEquals("wrapperEmpty.Weight.Value", ZDecimal.Zero, wrapperEmpty.Weight.Value);
				AssertEquals("wrapperEmpty.Weight.Unit.Code", "KG", wrapperEmpty.Weight.Unit.Code);
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
				AssertEquals("wrapperEmpty.IsTopLevelPackage", false, wrapperEmpty.IsTopLevelPackage);
				AssertEquals("wrapperEmpty.HasSingleProduct", false, wrapperEmpty.HasSingleProduct);
				AssertEquals("wrapperEmpty.HasPackedItem", false, wrapperEmpty.HasPackedItem);
				AssertNull("wrapperEmpty.PackedItem", wrapperEmpty.PackedItem);
				AssertEquals("wrapperEmpty.PostcodeBarcodeNumber", "", wrapperEmpty.PostcodeBarcodeNumber);
				AssertEquals("wrapperEmpty.PostcodeBarcodeNumber", "", wrapperEmpty.PostcodeBarcodeNumber);
				AssertEquals("wrapperEmpty.PostcodeBarcodeNumberWithPrefix", "", wrapperEmpty.PostcodeBarcodeNumberWithPrefix);
				AssertEquals("wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix", "", wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix);
				AssertEquals("wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText", "", wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText);
				AssertEquals("wrapperEmpty.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText", "", wrapperEmpty.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText);
				AssertEquals("wrapperEmpty.PostcodeBarcode", "", wrapperEmpty.PostcodeBarcode);
				AssertEquals("wrapperEmpty.OutterPackageSequence", ZShort.Zero, wrapperEmpty.OutterPackageSequence);
				AssertEquals("wrapperEmpty.OutterPackagesCount", ZShort.Zero, wrapperEmpty.OutterPackagesCount);
				AssertEquals("wrapperEmpty.PickLocation", ZString.Empty, wrapperEmpty.PickLocation);
				AssertEquals("wrapperEmpty.PickMethod", ZString.Empty, wrapperEmpty.PickMethod);
			});
		}

		public override void TestWrapperMappingFull()
		{
			Item.Consignment.HVC_GoodsDescription = "GoodsDescription";
			Item.HVI_ActualWeight = 300m;
			Item.Consignment.HVC_WeightUQ = "LB";
			Item.HVI_ActualVolume = 200m;
			Item.Consignment.HVC_VolumeUQ = "M3";
			Item.Consignment.HVC_ConsigneeAddress1 = "Address";
			Item.Consignment.HVC_ConsigneePostcode = "2213";
			Item.Consignment.HVC_RN_NKConsigneeCountryCode = "AU";

			var wrapperFull = new PackageWrapperFromHVLVItem(Item, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("wrapperFull.Packages.Value", 1m, wrapperFull.Packages.Value);
				AssertEquals("wrapperFull.Packages.Unit.Code", "PKG", wrapperFull.Packages.Unit.Code);
				AssertEquals("wrapperFull.OutturnPackages.Value", 0m, wrapperFull.OutturnedPackages.Value);
				AssertEquals("wrapperFull.OutturnPackages.Unit.Code", ZString.Empty, wrapperFull.OutturnedPackages.Unit.Code);
				AssertEquals("wrapperFull.PillagedPackages.Value", 0m, wrapperFull.PillagedPackages.Value);
				AssertEquals("wrapperFull.PillagedPackages.Unit.Code", ZString.Empty, wrapperFull.PillagedPackages.Unit.Code);
				AssertEquals("wrapperFull.DamagedPackages.Value", 0m, wrapperFull.DamagedPackages.Value);
				AssertEquals("wrapperFull.DamagedPackages.Unit.Code", ZString.Empty, wrapperFull.DamagedPackages.Unit.Code);
				AssertEquals("wrapperFull.CartonGroupAndSize", ZString.Empty, wrapperFull.CartonGroupAndSize);
				AssertEquals("wrapperFull.ContainerNo", "", wrapperFull.ContainerNo);
				AssertEquals("wrapperFull.ContainerJobID", "", wrapperFull.ContainerJobID);
				AssertEquals("wrapperFull.HouseBill", "", wrapperFull.HouseBill);
				AssertEquals("wrapperFull.MasterBill", "", wrapperFull.MasterBill);
				AssertEquals("wrapperFull.Volume.Value", 200m, wrapperFull.Volume.Value);
				AssertEquals("wrapperFull.Volume.Unit.Code", "M3", wrapperFull.Volume.Unit.Code);
				AssertEquals("wrapperFull.OutturnVolume.Value", 0m, wrapperFull.OutturnedVolume.Value);
				AssertEquals("wrapperFull.OutturnVolume.Unit.Code", ZString.Empty, wrapperFull.OutturnedVolume.Unit.Code);
				AssertEquals("wrapperFull.Weight.Value", 300m, wrapperFull.Weight.Value);
				AssertEquals("wrapperFull.Weight.Unit.Code", "LB", wrapperFull.Weight.Unit.Code);
				AssertEquals("wrapperFull.OutturnWeight.Value", 0m, wrapperFull.OutturnedWeight.Value);
				AssertEquals("wrapperFull.OutturnWeight.Unit.Code", ZString.Empty, wrapperFull.OutturnedWeight.Unit.Code);
				AssertEquals("wrapperFull.Description", "GoodsDescription", wrapperFull.Description);
				AssertEquals("wrapperFull.MarksAndNumbers", "", wrapperFull.MarksAndNumbers);
				AssertEquals("wrapperFull.Commodity.Description", "", wrapperFull.Commodity.Description);
				AssertEquals("wrapperFull.OutturnComment", "", wrapperFull.OutturnComment);
				AssertEquals("wrapperFull.Height", 0m, wrapperFull.Dimensions.Height);
				AssertEquals("wrapperFull.Length", 0m, wrapperFull.Dimensions.Length);
				AssertEquals("wrapperFull.Width", 0m, wrapperFull.Dimensions.Width);
				AssertEquals("wrapperFull.DimensionUnit", ZString.Empty, wrapperFull.Dimensions.Unit.Code);
				AssertEquals("wrapperFull.Parent.GoodsDescription", "GoodsDescription", wrapperFull.Parent.GoodsDescription);
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
				AssertEquals("wrapperFull.IsExclusive", false, wrapperFull.IsExclusive);
				AssertEquals("wrapperFull.IsExpiryUsed", false, wrapperFull.IsExpiryUsed);
				AssertEquals("wrapperFull.IsPackingDateUsed", false, wrapperFull.IsPackingDateUsed);
				AssertEquals("wrapperFull.IsPartAttrib1Used", false, wrapperFull.IsPartAttrib1Used);
				AssertEquals("wrapperFull.IsPartAttrib2Used", false, wrapperFull.IsPartAttrib2Used);
				AssertEquals("wrapperFull.IsPartAttrib3Used", false, wrapperFull.IsPartAttrib3Used);
				AssertEquals("wrapperFull.IsTrackedSerialUsed", false, wrapperFull.IsTrackedSerialUsed);
				AssertEquals("wrapperFull.DisplayOrder", "", wrapperFull.DisplayOrder);
				AssertEquals("wrapperFull.IsTopLevelPackage", false, wrapperFull.IsTopLevelPackage);
				AssertEquals("wrapperFull.HasSingleProduct", false, wrapperFull.HasSingleProduct);
				AssertEquals("wrapperFull.HasPackedItem", false, wrapperFull.HasPackedItem);
				AssertNull("wrapperFull.PackedItem", wrapperFull.PackedItem);
				AssertNull("wrapperFull.FreightPackLine", wrapperFull.FreightPackLine);
				AssertEquals("wrapperFull.PackedItem", "0362213", wrapperFull.PostcodeBarcodeNumber);
				AssertEquals("wrapperFull.PackedItem", "(421) 0362213", wrapperFull.PostcodeBarcodeNumberWithPrefix);
				AssertEquals("wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix", "(421) 0362213", wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix);
				AssertEquals("wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText", "ÈÆ(421)¯0362213[Ê", wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText);
				AssertEquals("wrapperFull.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText", "ÈÆ(421)¯0362213[Ê",
				wrapperFull.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText);
				AssertEquals("wrapperFull.PackedItem", "ÉÆJ*D6-CÊ", wrapperFull.PostcodeBarcode);
				AssertEquals("wrapperFull.OutterPackageSequence", ZShort.Zero, wrapperFull.OutterPackageSequence);
				AssertEquals("wrapperFull.OutterPackagesCount", ZShort.Zero, wrapperFull.OutterPackagesCount);
				AssertEquals("wrapperFull.PickLocation", ZString.Empty, wrapperFull.PickLocation);
				AssertEquals("wrapperFull.PickMethod", ZString.Empty, wrapperFull.PickMethod);
			});
		}

		[TestDate(2011, 11, 11)]
		public new void TestStarTrack_QRCodeText()
		{
			Item.Consignment.HVC_ConsigneeCity = "NANJING";
			Item.Consignment.HVC_ConsigneePostcode = "2100";

			var wrapperFull = new PackageWrapperFromHVLVItem(Item, Factory);
			var expectedQRCodeText = $"NANJING                       2100                                                                 20111111                                                                                                                                                                                     NN                                            ";
			AssertEquals(expectedQRCodeText, wrapperFull.StarTrack_QRCodeText);
		}

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageWrapperFromHVLVItem(Item, Factory);
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
Commodity : 
CommonCurrency : 
Container :  is null
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
Packages : 1 PKG
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

		HVLVItem Item => _item ?? (_item = Factory.New<HVLVConsignment>().Items.AddNew());
		HVLVItem _item;

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			Item.HVI_CurrentBarcode = "POODLE";
			Item.Consignment.HVC_VolumeUQ = "M3";
			Item.Consignment.HVC_ActualVolume = 200m;
			Item.Consignment.HVC_ActualWeight = 300m;
			Item.Consignment.HVC_WeightUQ = "LB";

			return new PackageWrapperFromHVLVItem(Item, Factory);
		}
	}
}
