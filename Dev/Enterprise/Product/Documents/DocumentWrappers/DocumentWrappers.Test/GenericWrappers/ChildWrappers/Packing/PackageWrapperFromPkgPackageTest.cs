using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportBookings.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transit.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperFromPkgPackage))]
	internal class PackageWrapperFromPkgPackageTest : PackageWrapperTest
	{
		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (PackageWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.MostRecentAudit", null, wrapperEmpty.MostRecentAudit);
			AssertEquals("wrapperEmpty.Packages.Value", 1m, wrapperEmpty.Packages.Value);
			AssertEquals("wrapperEmpty.Packages.Unit.Code", "", wrapperEmpty.Packages.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnPackages.Value", 0m, wrapperEmpty.OutturnedPackages.Value);
			AssertEquals("wrapperEmpty.OutturnPackages.Unit.Code", "", wrapperEmpty.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.PillagedPackages.Value", 0m, wrapperEmpty.PillagedPackages.Value);
			AssertEquals("wrapperEmpty.PillagedPackages.Unit.Code", "", wrapperEmpty.PillagedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.FumigatedPackages.Value", 0m, wrapperEmpty.FumigatedPackages.Value);
			AssertEquals("wrapperEmpty.FumigatedPackages.Unit.Code", "", wrapperEmpty.FumigatedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.NonStackablePackages.Value", 0m, wrapperEmpty.NonStackablePackages.Value);
			AssertEquals("wrapperEmpty.NonStackablePackages.Unit.Code", "", wrapperEmpty.NonStackablePackages.Unit.Code);
			AssertEquals("wrapperEmpty.TopLoadOnlyPackages.Value", 0m, wrapperEmpty.TopLoadOnlyPackages.Value);
			AssertEquals("wrapperEmpty.TopLoadOnlyPackages.Unit.Code", "", wrapperEmpty.TopLoadOnlyPackages.Unit.Code);
			AssertEquals("wrapperEmpty.HeatTreatedPackages.Value", 0m, wrapperEmpty.HeatTreatedPackages.Value);
			AssertEquals("wrapperEmpty.HeatTreatedPackages.Unit.Code", "", wrapperEmpty.HeatTreatedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.ISPMPalletPackages.Value", 0m, wrapperEmpty.ISPMPalletPackages.Value);
			AssertEquals("wrapperEmpty.ISPMPalletPackages.Unit.Code", "", wrapperEmpty.ISPMPalletPackages.Unit.Code);
			AssertEquals("wrapperEmpty.DamagedPackages.Value", 0m, wrapperEmpty.DamagedPackages.Value);
			AssertEquals("wrapperEmpty.DamagedPackages.Unit.Code", "", wrapperEmpty.DamagedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.CartonGroupAndSize", ZString.Empty, wrapperEmpty.CartonGroupAndSize);
			AssertEquals("wrapperEmpty.ContainerNo", "", wrapperEmpty.ContainerNo);
			AssertEquals("wrapperEmpty.ContainerJobID", "", wrapperEmpty.ContainerJobID);
			AssertEquals("wrapperEmpty.HouseBill", "", wrapperEmpty.HouseBill);
			AssertEquals("wrapperEmpty.MasterBill", "", wrapperEmpty.MasterBill);
			AssertEquals("wrapperEmpty.UNDGSubstance", 0, wrapperEmpty.UNDGSubstances.Count);
			AssertEquals("wrapperEmpty.Volume.Value", 0m, wrapperEmpty.Volume.Value);
			AssertEquals("wrapperEmpty.Volume.Unit.Code", "M3", wrapperEmpty.Volume.Unit.Code);
			AssertEquals("wrapperEmpty.Weight.Value", 0m, wrapperEmpty.Weight.Value);
			AssertEquals("wrapperEmpty.Weight.Unit.Code", "KG", wrapperEmpty.Weight.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnWeight.Value", 0m, wrapperEmpty.OutturnedWeight.Value);
			AssertEquals("wrapperEmpty.OutturnWeight.Unit.Code", "KG", wrapperEmpty.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperEmpty.Description", "", wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.MarksAndNumbers", "", wrapperEmpty.MarksAndNumbers);
			AssertEquals("wrapperEmpty.Commodity.Code", "", wrapperEmpty.Commodity.Code);
			AssertEquals("wrapperEmpty.DamagedReason.Code", "", wrapperEmpty.DamagedReason.Code);
			AssertEquals("wrapperEmpty.Height", 0m, wrapperEmpty.Dimensions.Height);
			AssertEquals("wrapperEmpty.Width", 0m, wrapperEmpty.Dimensions.Width);
			AssertEquals("wrapperEmpty.Length", 0m, wrapperEmpty.Dimensions.Length);
			AssertEquals("wrapperEmpty.Dimension.Unit.Code", "M", wrapperEmpty.Dimensions.Unit.Code);
			AssertEquals("wrapperEmpty.RefNumber", "", wrapperEmpty.RefNumber);
			AssertEquals("wrapperEmpty.ExportRefNumber", ZString.Empty, wrapperEmpty.ExportRefNumber);
			AssertEquals("wrapperEmpty.BarcodeText", "", wrapperEmpty.BarcodeText);
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
			AssertEquals("wrapperEmpty.DisplayOrder", "000", wrapperEmpty.DisplayOrder);
			AssertEquals("wrapperEmpty.HasMixedProducts", false, wrapperEmpty.HasSingleProduct);
			AssertEquals("wrapperEmpty.HasPackedItem", false, wrapperEmpty.HasPackedItem);
			AssertEquals("wrapperEmpty.PackedItem", "", wrapperEmpty.PackedItem.Description);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumber", "", wrapperEmpty.PostcodeBarcodeNumber);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumberWithPrefix", "", wrapperEmpty.PostcodeBarcodeNumberWithPrefix);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix", "", wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix", "", wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText);
			AssertEquals("wrapperEmpty.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText", "", wrapperEmpty.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText);
			AssertEquals("wrapperEmpty.PostcodeBarcode", "", wrapperEmpty.PostcodeBarcode);
			AssertEquals("wrapperEmpty.IsTopLevelNonContainerisedPackage", false, wrapperEmpty.IsTopLevelNonContainerisedPackage);
			AssertEquals("wrapperEmpty.PackageTemperatures", ZString.Empty, wrapperEmpty.PackageTemperatures);
			AssertEquals("wrapperEmpty.Seals", ZString.Empty, wrapperEmpty.Seals);
			AssertEquals("wrapperEmpty.NMFC", ZString.Empty, wrapperEmpty.NMFC);
		}

		#endregion

		#region TestWrapperMappingFull

		[TestDate(2014, 2, 6)]
		public override void TestWrapperMappingFull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part2, true);

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AU2CO";

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", helper.Notify);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, ZDate.Today.AddDays(10), ZDate.Today.AddDays(-10), "PA1", "PA2", "PA3", "");
			var inventory3 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 1m, ZDate.Today.AddDays(11), ZDate.Today.AddDays(-10), "PA1", "PA2", "PA4", "");
			inventory2.WI_SerialNumber = "SN1";
			inventory3.WI_SerialNumber = "SN2";

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised.", true, receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 1m);
			var orderLine3 = helper.CreateWhsOrderLine(order, data.Part2, 1m);

			var consignee = helper.CreateClient("CNE", "CNE SYDNEY");
			consignee.MainAddress.FillWithValidTestData();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consignee.MainAddress.OA_PostCode = "2213";
			order.ConsigneeDocAddress.E2_OA_Address = consignee.MainAddress.PK;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, "PKG1", 1, Constants.PkgUnit.Box, "B1");
			package1.KP_PackageQty = 1; //"A Package ID is for a single Package. Either change the Package Qty to 1 or remove the Package ID."
			package1.KP_F3_NKPackType = Constants.PkgUnit.Box;
			package1.KP_DimensionUQ = "M";
			package1.KP_Height = 1.2344m;
			package1.KP_Length = 2.3454m;
			package1.KP_PackageID = "PACKAGE123";
			package1.KP_VolumeUQ = "M3";
			package1.KP_Weight = 5m;
			package1.KP_WeightUQ = "T";
			package1.KP_Width = 6.7891m;
			package1.KP_IsDamaged = true;
			package1.KP_IsPillaged = true;
			package1.KP_IsFumigated = true;
			package1.KP_IsNonStackable = true;
			package1.KP_IsTopLoadOnly = true;
			package1.KP_IsHeatTreated = true;
			package1.KP_IsISPMPallet = true;
			package1.KP_MarksAndNumbers = "MARK123";
			package1.KP_TransportRef = "TRANSPORT REF";
			package1.KP_HSCode = "HARMONIZED CODE";
			package1.KP_GoodsDescription = "GOODS123";
			package1.KP_PackageID = "123";
			package1.KP_Volume = 12m;
			package1.KP_RequiredTemperatureMinimum = -4;
			package1.KP_RequiredTemperatureMaximum = 2;
			package1.KP_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			package1.KP_RequiresTemperatureControl = true;
			package1.CartonGroupAndSize = "CARTONS - BIG";

			var package2 = PackingHelper.CreatePackage(packageJob, "PKG2", 1, Constants.PkgUnit.Box, "B2");

			var commodity = Factory.New<RefCommodityCode>();
			commodity.RH_Code = "OMFG";
			package1.KP_RH_NKCommodityCode = commodity.RH_Code;

			var nMFC = Factory.New<RefNMFC>();
			nMFC.FN_Class = "12345";
			nMFC.FN_ItemNo = "123456";
			commodity.RH_FN_NKNMFC = nMFC.FN_Code;

			var undg = package1.UNDGs.AddNew();
			undg.DI_DG = UNDGSubstanceLoader.LoadSubstances(Factory, "2478", "a", "IMO").First().PK;

			var innerPackage1 = PackingHelper.CreatePackage(package1, 1, Constants.PkgUnit.Pail, "P1");
			var innerPackage2 = PackingHelper.CreatePackage(package1, 1, Constants.PkgUnit.Pail, "P2");
			var innerPackage3 = PackingHelper.CreatePackage(package1, 1, Constants.PkgUnit.Pail, "P3");
			var innerPackage4 = PackingHelper.CreatePackage(package1, 1, Constants.PkgUnit.Pail, "P4");
			var innerPackage5 = PackingHelper.CreatePackage(package1, 1, Constants.PkgUnit.Pail, "P5");
			var innerPackage6 = PackingHelper.CreatePackage(package1, 1, Constants.PkgUnit.Pail, "P6");
			Factory.Save();

			var pick = helper.CreatePickNew(order);
			order.WD_DocketID = "W00000002";
			AssertEquals("Precondition - Pick or stock allocation failed.", 1, orderLine2.ReleaseLines.Count);

			var packedItem = package1.Pack_ForTesting(orderLine2.ReleaseLines[0], 1m);
			var wrapperFull = new PackageWrapperFromPkgPackage(package1, packedItem, Factory, 2, 3);
			AssertEquals("wrapperFull.Packages.Value", 1m, wrapperFull.Packages.Value); // set as 1
			AssertEquals("wrapperFull.Packages.Unit.Code", Constants.PkgUnit.Box, wrapperFull.Packages.Unit.Code);
			AssertEquals("wrapperFull.OutturnPackages.Value", 0m, wrapperFull.OutturnedPackages.Value);
			AssertEquals("wrapperFull.OutturnPackages.Unit.Code", Constants.PkgUnit.Box, wrapperFull.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperFull.PillagedPackages.Value", 1m, wrapperFull.PillagedPackages.Value);
			AssertEquals("wrapperFull.PillagedPackages.Unit.Code", Constants.PkgUnit.Box, wrapperFull.PillagedPackages.Unit.Code);
			AssertEquals("wrapperFull.DamagedPackages.Value", 1m, wrapperFull.DamagedPackages.Value);
			AssertEquals("wrapperFull.DamagedPackages.Unit.Code", Constants.PkgUnit.Box, wrapperFull.DamagedPackages.Unit.Code);
			AssertEquals("wrapperFull.FumigatedPackages.Value", 1m, wrapperFull.FumigatedPackages.Value);
			AssertEquals("wrapperFull.FumigatedPackages.Unit.Code", Constants.PkgUnit.Box, wrapperFull.FumigatedPackages.Unit.Code);
			AssertEquals("wrapperFull.NonStackablePackages.Value", 1m, wrapperFull.NonStackablePackages.Value);
			AssertEquals("wrapperFull.NonStackablePackages.Unit.Code", Constants.PkgUnit.Box, wrapperFull.NonStackablePackages.Unit.Code);
			AssertEquals("wrapperFull.TopLoadOnlyPackages.Value", 1m, wrapperFull.TopLoadOnlyPackages.Value);
			AssertEquals("wrapperFull.TopLoadOnlyPackages.Unit.Code", Constants.PkgUnit.Box, wrapperFull.TopLoadOnlyPackages.Unit.Code);
			AssertEquals("wrapperFull.HeatTreatedPackages.Value", 1m, wrapperFull.HeatTreatedPackages.Value);
			AssertEquals("wrapperFull.HeatTreatedPackages.Unit.Code", Constants.PkgUnit.Box, wrapperFull.HeatTreatedPackages.Unit.Code);
			AssertEquals("wrapperFull.ISPMPalletPackages.Value", 1m, wrapperFull.ISPMPalletPackages.Value);
			AssertEquals("wrapperFull.ISPMPalletPackages.Unit.Code", Constants.PkgUnit.Box, wrapperFull.ISPMPalletPackages.Unit.Code);
			AssertEquals("wrapperFull.CartonGroupAndSize", "CARTONS - BIG", wrapperFull.CartonGroupAndSize);
			AssertEquals("wrapperFull.ContainerNo", "", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerJobID", "", wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.HouseBill", "", wrapperFull.HouseBill);
			AssertEquals("wrapperFull.MasterBill", "", wrapperFull.MasterBill);
			AssertEquals("wrapperFull.UNDGSubstances.Count", 1, wrapperFull.UNDGSubstances.Count);
			AssertEquals("wrapperFull.UNDGSubstances.UNNumbers", "2478", wrapperFull.UNDGSubstances.UNNumbers);
			AssertEquals("wrapperFull.Volume.Value", 12m, wrapperFull.Volume.Value);
			AssertEquals("wrapperFull.Volume.Unit.Code", "M3", wrapperFull.Volume.Unit.Code);
			AssertEquals("wrapperFull.OutturnVolume.Value", 0m, wrapperFull.OutturnedVolume.Value);
			AssertEquals("wrapperFull.OutturnVolume.Unit.Code", "M3", wrapperFull.OutturnedVolume.Unit.Code);
			AssertEquals("wrapperFull.Weight.Value", 5.00m, wrapperFull.Weight.Value);
			AssertEquals("wrapperFull.Weight.Unit.Code", "T", wrapperFull.Weight.Unit.Code);
			AssertEquals("wrapperFull.OutturnWeight.Value", 0m, wrapperFull.OutturnedWeight.Value);
			AssertEquals("wrapperFull.OutturnWeight.Unit.Code", "T", wrapperFull.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperFull.Description", "GOODS123", wrapperFull.Description);
			AssertEquals("wrapperFull.MarksAndNumbers", "MARK123", wrapperFull.MarksAndNumbers);
			AssertEquals("wrapperFull.Commodity.Code", "", wrapperFull.Commodity.Code);
			AssertEquals("wrapperFull.DamagedReason.Code", "", wrapperFull.DamagedReason.Code);
			AssertEquals("wrapperFull.OutturnComment", "", wrapperFull.OutturnComment);
			AssertEquals("wrapperFull.Height", 1.234m, wrapperFull.Dimensions.Height);
			AssertEquals("wrapperFull.Length", 2.345m, wrapperFull.Dimensions.Length);
			AssertEquals("wrapperFull.Width", 6.789m, wrapperFull.Dimensions.Width);
			AssertEquals("wrapperFull.DimensionUnit", "M", wrapperFull.Dimensions.Unit.Code);
			AssertEquals("wrapperFull.Parent", "W00000002", wrapperFull.Parent.ToString());
			AssertEquals("wrapperFull.LinePrice", 0m, wrapperFull.LinePrice);
			AssertEquals("wrapperFull.ItemNumber", (ZShort)0, wrapperFull.ItmNumber);
			AssertEquals("wrapperFull.HarmonizedCode", "HARMONIZED CODE", wrapperFull.HarmonizedCode);
			AssertEquals("wrapperFull.Origin", "", wrapperFull.Origin.UNLOCO);
			AssertEquals("wrapperFull.Indent", new string(' ', 18), wrapperFull.Indent);
			AssertEquals("wrapperFull.Inners", 6, wrapperFull.Inners);
			AssertEquals("wrapperFull.InnersDetail", @"P1(PAI), P2(PAI), P3(PAI), P4(PAI), P5(PAI),
More Packages exist, please refer to Manifest", wrapperFull.InnersDetail);
			AssertEquals("wrapperFull.PackedItemCount", 1, wrapperFull.PackedItemCount);
			AssertEquals("wrapperFull.IsExclusive", true, wrapperFull.IsExclusive);
			AssertEquals("wrapperFull.IsExpiryUsed", true, wrapperFull.IsExpiryUsed);
			AssertEquals("wrapperFull.IsPackingDateUsed", true, wrapperFull.IsPackingDateUsed);
			AssertEquals("wrapperFull.IsPartAttrib1Used", true, wrapperFull.IsPartAttrib1Used);
			AssertEquals("wrapperFull.IsPartAttrib2Used", true, wrapperFull.IsPartAttrib2Used);
			AssertEquals("wrapperFull.IsPartAttrib3Used", true, wrapperFull.IsPartAttrib3Used);
			AssertEquals("wrapperFull.IsTrackedSerialUsed", true, wrapperFull.IsTrackedSerialUsed);
			AssertEquals("wrapperFull.DisplayOrder", "002", wrapperFull.DisplayOrder);
			AssertEquals("wrapperFull.Products", PackProductWrapperCollection.Empty, wrapperFull.Products);
			AssertEquals("wrapperFull.RefNumber", "123", wrapperFull.RefNumber);
			AssertEquals("wrapperFull.BarcodeText", "È123(Ê", wrapperFull.PackageBarcode);
			AssertEquals("wrapperFull.PackageBarcodeWithOptimisedEncoding", "È123(Ê", wrapperFull.PackageBarcodeWithOptimisedEncoding);
			AssertEquals("wrapperFull.HasSingleProduct", true, wrapperFull.HasSingleProduct);
			AssertEquals("wrapperFull.HasPackedItem", true, wrapperFull.HasPackedItem);
			AssertEquals("wrapperFull.PackedItem", "P2", wrapperFull.PackedItem.Description);
			AssertEquals("wrapperFull.PackedItem", "2213", wrapperFull.PostcodeBarcodeNumber);
			AssertEquals("wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix", "(421) 0362213", wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix);
			AssertEquals("wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText", "ÈÆ(421)¯0362213[Ê", wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText);
			AssertEquals("wrapperFull.PostcodeBarcodeNumberWithPrefix", "(420) 2213", wrapperFull.PostcodeBarcodeNumberWithPrefix);
			AssertEquals("wrapperFull.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText", "ÈÆ(421)¯0362213(90)#1gÊ",
				wrapperFull.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText);
			AssertEquals("wrapperFull.PackedItem", "ÈÆ4Ã46-zÊ", wrapperFull.PostcodeBarcode);
			AssertEquals("wrapperFull.IsTopLevelNonContainerisedPackage", true, wrapperFull.IsTopLevelNonContainerisedPackage);
			AssertNull("wrapperFull.FreightPackLine", wrapperFull.FreightPackLine);
			AssertEquals("wrapperFull.NMFC", "123456|12345", wrapperFull.NMFC);
			AssertEquals("wrapperFull.PackageSequence", (ZShort)1, wrapperFull.OutterPackageSequence);
			AssertEquals("wrapperFull.OutterPackagesCount", (ZShort)2, wrapperFull.OutterPackagesCount);

			var wrapper2 = new PackageWrapperFromPkgPackage(package2, packedItem, Factory, 2, 3);
			AssertEquals("wrapperFull.PackageSequence", (ZShort)2, wrapper2.OutterPackageSequence);
			AssertEquals("wrapperFull.OutterPackagesCount", (ZShort)2, wrapper2.OutterPackagesCount);

			_ = orderLine1.ReleaseLines.Count; // need to poke the collection to rebuild, the indexer won't rebuild
			var packedItemWithoutAttributes = innerPackage1.Pack_ForTesting(orderLine1.ReleaseLines[0], 3m);
			var wrapperWithoutAttributes = new PackageWrapperFromPkgPackage(innerPackage1, packedItemWithoutAttributes, Factory, 2, 3);
			AssertEquals("wrapperFull.IsExpiryUsed", false, wrapperWithoutAttributes.IsExpiryUsed);
			AssertEquals("wrapperFull.IsPackingDateUsed", false, wrapperWithoutAttributes.IsPackingDateUsed);
			AssertEquals("wrapperFull.IsPartAttrib1Used", false, wrapperWithoutAttributes.IsPartAttrib1Used);
			AssertEquals("wrapperFull.IsPartAttrib2Used", false, wrapperWithoutAttributes.IsPartAttrib2Used);
			AssertEquals("wrapperFull.IsPartAttrib3Used", false, wrapperWithoutAttributes.IsPartAttrib3Used);
			AssertEquals("wrapperFull.IsTrackedSerialUsed", false, wrapperWithoutAttributes.IsTrackedSerialUsed);

			consignee.MainAddress.OA_RL_NKRelatedPortCode = "NZAKL";
			innerPackage1.Delete();
			innerPackage2.Delete();

			_ = orderLine3.ReleaseLines.Count; // need to poke the collection to rebuild, the indexer won't rebuild
			innerPackage3.Pack(orderLine3.ReleaseLines[0], 1m);
			wrapperFull = new PackageWrapperFromPkgPackage(package1, packedItem, Factory, 2, 3);
			AssertEquals("wrapperFull.PostcodeBarcodeNumberWithPrefix", "(421) 5542213", wrapperFull.PostcodeBarcodeNumberWithPrefix);
			AssertEquals("wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix", "(421) 5542213", wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix);
			AssertEquals("wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText", "ÈÆ(421)¯5542213ÁÊ", wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText);
			AssertEquals("wrapperFull.PostcodeBarcode", "ÉÆJ/V6-3Ê", wrapperFull.PostcodeBarcode);
			AssertEquals("wrapperFull.Inners", 4, wrapperFull.Inners);
			AssertEquals("wrapperFull.InnersDetail", @"P3(PAI), P4(PAI), P5(PAI), P6(PAI)", wrapperFull.InnersDetail);
			AssertEquals("wrapperFull.PackedItemCount", 1, wrapperFull.PackedItemCount);
			AssertEquals("wrapperFull.IsExclusive", true, wrapperFull.IsExclusive);
			AssertEquals("wrapperFull.HasSingleProduct", true, wrapperFull.HasSingleProduct);

			innerPackage3.Delete();
			innerPackage4.Pack(orderLine1.ReleaseLines[0], 4m);
			wrapperFull = new PackageWrapperFromPkgPackage(package1, packedItem, Factory, 2, 3);
			AssertEquals("wrapperFull.IsExclusive", true, wrapperFull.IsExclusive);
			AssertEquals("wrapperFull.HasSingleProduct", false, wrapperFull.HasSingleProduct);
			AssertEquals("wrapperFull.IsExpiryUsed", true, wrapperFull.IsExpiryUsed);
			AssertEquals("wrapperFull.IsPackingDateUsed", true, wrapperFull.IsPackingDateUsed);
			AssertEquals("wrapperFull.IsPartAttrib1Used", true, wrapperFull.IsPartAttrib1Used);
			AssertEquals("wrapperFull.IsPartAttrib2Used", true, wrapperFull.IsPartAttrib2Used);
			AssertEquals("wrapperFull.IsPartAttrib3Used", true, wrapperFull.IsPartAttrib3Used);
			AssertEquals("wrapperFull.IsTrackedSerialUsed", true, wrapperFull.IsTrackedSerialUsed);

			innerPackage4.Delete();
			package1.Pack(orderLine1.ReleaseLines[0], 4.1m);
			wrapperFull = new PackageWrapperFromPkgPackage(package1, packedItem, Factory, 2, 3);
			AssertEquals("wrapperFull.IsExclusive", false, wrapperFull.IsExclusive);
			AssertEquals("wrapperFull.HasSingleProduct", true, wrapperFull.HasSingleProduct);
			AssertEquals("wrapperFull.PackedItemCount", 5, wrapperFull.PackedItemCount);
		}

		public void TestContainerPackage()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part2, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part2, 11m);
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			var packedItem = package.Pack_ForTesting(order.Lines[0].ReleaseLines[0], 5m);
			var wrapperFull = new PackageWrapperFromPkgPackage(package, packedItem, Factory, 2, 3);

			package.Container.K0_Seal1 = "Seal 1";
			AssertEquals("wrapperFull.Seals", "Seal 1", wrapperFull.Seals);
			package.Container.K0_Seal2 = "Seal 2";
			AssertEquals("wrapperFull.Seals", "Seal 1\r\nSeal 2", wrapperFull.Seals);
			package.Container.K0_Seal3 = "Seal 3";
			AssertEquals("wrapperFull.Seals", "Seal 1\r\nSeal 2\r\nSeal 3", wrapperFull.Seals);
			package.Container.K0_Seal2 = "";
			AssertEquals("wrapperFull.Seals", "Seal 1\r\nSeal 3", wrapperFull.Seals);
			package.Container.K0_Seal1 = "";
			AssertEquals("wrapperFull.Seals", "Seal 3", wrapperFull.Seals);
			package.Container.K0_Seal2 = "Seal 2";
			package.Container.K0_Seal3 = "";
			AssertEquals("wrapperFull.Seals", "Seal 2", wrapperFull.Seals);

			package.Container.K0_SetPointTemp = 15;
			package.Container.K0_SetPointTempUnit = Constants.Temperature.Centigrade;
			package.Container.K0_IsControlledAtmosphere = true;
			AssertEquals("wrapperFull.ContainerTemperature", "Set Point Temp.: 15°C", wrapperFull.PackageTemperatures);
		}

		#endregion

		#region TestHasPackedItem

		public void TestHasPackedItem()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			package.Pack(orderLine.ReleaseLines[0], 5m);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("HasPackedItem should be true if Package contains a PackableItem.", true, wrapper.HasPackedItem);
			AssertEquals("PackedItem should should return the first PackableItem on the Package.", orderLine.ReleaseLines[0], wrapper.PackedItem.WrappedObject);
		}

		#endregion

		#region TestIsTopLevelPackage

		public void TestIsTopLevelPackage()
		{
			var package = Factory.New<PkgPackage>();
			var wrapper1 = new PackageWrapperFromPkgPackage(package, Factory, 0, 0);
			var wrapper2 = new PackageWrapperFromPkgPackage(package, Factory, 0, 5);
			AssertEquals(true, wrapper1.IsTopLevelPackage);
			AssertEquals(false, wrapper2.IsTopLevelPackage);
		}

		#endregion

		#region TestIsTopLevelNonContainerisedPackage

		public void TestIsTopLevelNonContainerisedPackage()
		{
			var packageJob1 = Factory.New<PkgPackageJob>();
			var outerPack = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");
			var innerPack = PackingHelper.CreatePackage(outerPack, 1, Constants.PkgUnit.Box, "B2");
			var container = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Container);
			var outerInContainerPack = PackingHelper.CreatePackage(container, 1, Constants.PkgUnit.Box);

			var outerPackWrapper = new PackageWrapperFromPkgPackage(outerPack, Factory);
			var innerPackWrapper = new PackageWrapperFromPkgPackage(innerPack, Factory);
			var containerWrapper = new PackageWrapperFromPkgPackage(container, Factory);
			var outerInContainerPackWrapper = new PackageWrapperFromPkgPackage(outerInContainerPack, Factory);

			AssertEquals(true, outerPackWrapper.IsTopLevelNonContainerisedPackage);
			AssertEquals(false, innerPackWrapper.IsTopLevelNonContainerisedPackage);
			AssertEquals(false, containerWrapper.IsTopLevelNonContainerisedPackage);
			AssertEquals(true, outerInContainerPackWrapper.IsTopLevelNonContainerisedPackage);
		}

		#endregion

		#region TestPackedItems

		public void TestPackedItems()
		{
			TestPackedItemsCore();
		}

		protected virtual void TestPackedItemsCore()
		{
			#region SetUpData
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var part3 = helper.CreateProduct(data.Org1, "part3");
			var part4 = helper.CreateProduct(data.Org1, "part4");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, part3, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, part4, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var order1Line1 = helper.CreateWhsOrderLine(order1, data.Part1, 5);
			var order1Line2 = helper.CreateWhsOrderLine(order1, data.Part2, 5);
			var order1Line3 = helper.CreateWhsOrderLine(order1, part3, 5);
			var order1Line4 = helper.CreateWhsOrderLine(order1, part4, 5);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			// Package Job 1
			//    1x Box B1
			//    1x Box B2
			//       Part1
			//		 1x Box B4
			//          Part1
			//          Part2
			//          Part3
			//          Part4
			//    1x Box B3
			//       Part2
			//       Part3
			//       Part4

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var order1OuterPackWith0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");

			var order1OuterPackWith1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B2");
			var order1OuterPackedItem1 = order1OuterPackWith1.Pack(order1Line1.ReleaseLines[0], 1m).Single();
			var order1OuterPackedItem2 = order1OuterPackWith1.Pack(order1Line1.ReleaseLines[0], 1m).Single();

			var order1OuterPackWith3 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B3");
			order1OuterPackWith3.Pack(order1Line2.ReleaseLines[0], 1);
			order1OuterPackWith3.Pack(order1Line3.ReleaseLines[0], 1);
			order1OuterPackWith3.Pack(order1Line4.ReleaseLines[0], 1);

			var order1InnerPackWith4 = PackingHelper.CreatePackage(order1OuterPackWith1, 1, Constants.PkgUnit.Box, "B4");
			var order1InnerPackedItem1 = order1InnerPackWith4.Pack_ForTesting(order1Line1.ReleaseLines[0], 1);
			var order1InnerPackedItem2 = order1InnerPackWith4.Pack_ForTesting(order1Line1.ReleaseLines[0], 1);
			var order1InnerPackedItem3 = order1InnerPackWith4.Pack_ForTesting(order1Line2.ReleaseLines[0], 1);
			var order1InnerPackedItem4 = order1InnerPackWith4.Pack_ForTesting(order1Line3.ReleaseLines[0], 1);
			var order1InnerPackedItem5 = order1InnerPackWith4.Pack_ForTesting(order1Line4.ReleaseLines[0], 1);

			var order1InnerPackWith1 = PackingHelper.CreatePackage(order1OuterPackWith3, 1, Constants.PkgUnit.Box, "B5");
			order1InnerPackWith1.Pack(order1Line2.ReleaseLines[0], 1);
			order1InnerPackWith1.Pack(order1Line2.ReleaseLines[0], 1);

			// pack an empty inner to test 'HasSingleProduct'
			PackingHelper.CreatePackage(order1InnerPackWith1, 1, Constants.PkgUnit.Box, "B6");

			#endregion

			var wrapper1 = new PackageWrapperFromPkgPackage(order1InnerPackWith4, new[] { order1InnerPackedItem3, order1InnerPackedItem4 }, Factory, 2, 3);
			AssertEquals(nameof(wrapper1.HasPackedItem), true, wrapper1.HasPackedItem);
			AssertEquals(nameof(wrapper1.PackedItem.Description), data.Part2.OP_Desc, wrapper1.PackedItem.Description);
			AssertEquals(nameof(wrapper1.PackedItems.Count), 2, wrapper1.PackedItems.Count);
			AssertEquals("IsExclusive, really has 4, so not exclusive.", false, wrapper1.IsExclusive);
			AssertEquals("Has 4 Products, so HasSingleProduct should be false.", false, wrapper1.HasSingleProduct);
			AssertEquals("There are 5 units Packed.", 5, wrapper1.PackedItemCount);

			var wrapper2 = new PackageWrapperFromPkgPackage(order1InnerPackWith4, new[] { order1InnerPackedItem4 }, Factory, 2, 3);
			AssertEquals("Only 1 Packed Item Passed in, so PackedItems Collection should contain only 1 Wrapper.", 1, wrapper2.PackedItems.Count);
			AssertEquals("IsExclusive, really has 4, so not exclusive.", false, wrapper2.IsExclusive);
			AssertEquals("Has 4 matching Products, so HasSingleProduct should be true.", true, wrapper2.HasSingleProduct);
			AssertEquals("There are 5 units Packed.", 5, wrapper2.PackedItemCount);

			var wrapper3 = new PackageWrapperFromPkgPackage(order1OuterPackWith1, new[] { order1OuterPackedItem1, order1OuterPackedItem2 }, Factory, 2, 3);
			AssertEquals("Even though 2 Packed Items are Passed in, they are for both the same PackableItemParent so there should be only 1 wrapper.", 1, wrapper3.PackedItems.Count);
			AssertEquals("IsExclusive, really has 1, so is exclusive.", true, wrapper3.IsExclusive);
			AssertEquals("Package and Inners are mixed products so HasSingleProduct should be false.", false, wrapper3.HasSingleProduct);
			AssertEquals("There are 2 units Packed.", 2, wrapper3.PackedItemCount);

			var wrapper4 = new PackageWrapperFromPkgPackage(order1InnerPackWith1, Factory, 2, 3);
			AssertEquals(nameof(wrapper4.HasPackedItem), true, wrapper4.HasPackedItem);
			AssertEquals(nameof(wrapper4.PackedItem.Description), data.Part2.OP_Desc, wrapper4.PackedItem.Description);
			AssertEquals("When no Packed Items are passed in, all Packed Items on the Package should be considered. There should be 1 wrapper.", 1, wrapper4.PackedItems.Count);
			AssertEquals("IsExclusive, really has 1, so is exclusive.", true, wrapper4.IsExclusive);
			AssertEquals("Has 1 Product on itself and has no inners with Packed Items, so HasSingleProduct should be true.", true, wrapper4.HasSingleProduct);
			AssertEquals("There are 2 units Packed.", 2, wrapper4.PackedItemCount);

			var wrapper5 = new PackageWrapperFromPkgPackage(order1InnerPackWith4, Factory, 2, 3);
			AssertEquals(nameof(wrapper5.HasPackedItem), true, wrapper5.HasPackedItem);
			AssertCollectionContains(data.Part1.OP_Desc, wrapper5.PackedItems.Select(p => ((PackedItemWrapper)p).Description).ToList());
			AssertEquals("When no Packed Items are passed in, all Packed Items on the Package should be considered. There should be 4 wrappers.", 4, wrapper5.PackedItems.Count);
			AssertEquals("IsExclusive, really has 4, so not exclusive.", false, wrapper5.IsExclusive);
			AssertEquals("Has 4 Products, so HasSingleProduct should be false.", false, wrapper5.HasSingleProduct);
			AssertEquals("There are 5 units Packed.", 5, wrapper5.PackedItemCount);
		}

		#endregion

		#region LinePrice

		public void TestLinePrice_SomePackedItems()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 15m);
			orderLine1.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine1.WE_UnitPriceAfterDiscount = 75.32m;

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 29m);
			orderLine2.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine2.WE_UnitPriceAfterDiscount = 39.26m;
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			package.Pack(orderLine1.ReleaseLines[0], 8m);
			package.Pack(orderLine2.ReleaseLines[0], 17m);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("LinePrice has the correct value.", 1269.98m, wrapper.LinePrice);
		}

		public void TestLinePrice_SomePackedItems_SomeWithNoLinePrice()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 15m);

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 29m);
			orderLine2.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine2.WE_UnitPriceAfterDiscount = 39.26m;
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			package.Pack(orderLine1.ReleaseLines[0], 8m);
			package.Pack(orderLine2.ReleaseLines[0], 17m);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("LinePrice has the correct value.", 667.42m, wrapper.LinePrice);
		}

		public void TestLinePrice_SomePackedItems_DifferentCurrencies()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 15m);
			orderLine1.WE_RX_NKUnitPriceCurrency = "AUD";
			orderLine1.WE_UnitPriceAfterDiscount = 75.32m;

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 29m);
			orderLine2.WE_RX_NKUnitPriceCurrency = "USD";
			orderLine2.WE_UnitPriceAfterDiscount = 39.26m;
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			package.Pack(orderLine1.ReleaseLines[0], 8m);
			package.Pack(orderLine2.ReleaseLines[0], 17m);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("LinePrice has the correct value.", 0m, wrapper.LinePrice);
		}

		#endregion

		#region CommonCurrency

		protected override void CommonCurrencyTestCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 15m);
			orderLine1.WE_RX_NKUnitPriceCurrency = "USD";

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 29m);
			orderLine2.WE_RX_NKUnitPriceCurrency = "USD";
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			package.Pack(orderLine1.ReleaseLines[0], 8m);
			package.Pack(orderLine2.ReleaseLines[0], 17m);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("Common Currency has the correct value.", "USD", wrapper.CommonCurrency.Code);
		}

		public void TestCommonCurrency_OnlyOneLineWithCurrency()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 15m);

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 29m);
			orderLine2.WE_RX_NKUnitPriceCurrency = "USD";
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			package.Pack(orderLine1.ReleaseLines[0], 8m);
			package.Pack(orderLine2.ReleaseLines[0], 17m);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("Common Currency has the correct value.", "USD", wrapper.CommonCurrency.Code);
		}

		public void TestCommonCurrency_MultipleCurrencies()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 100m);
			helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part2, 100m);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 15m);
			orderLine1.WE_RX_NKUnitPriceCurrency = "AUD";

			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 29m);
			orderLine2.WE_RX_NKUnitPriceCurrency = "USD";
			helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			package.Pack(orderLine1.ReleaseLines[0], 8m);
			package.Pack(orderLine2.ReleaseLines[0], 17m);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("Common Currency has the correct value.", string.Empty, wrapper.CommonCurrency.Code);
		}

		#endregion

		#region TestGetPickMethodAndLocation

		public void TestGetPickMethodAndLocation_PickLinesWithOutRCA()
		{
			TestGetPickMethodAndLocation(setRCAOnPickLine: false, setRCAOnBothPickLines: false);
		}

		public void TestGetPickMethodAndLocation_OnePickLinesWithRCA()
		{
			TestGetPickMethodAndLocation(setRCAOnPickLine: true, setRCAOnBothPickLines: false);
		}

		public void TestGetPickMethodAndLocation_PickLineaWithRCA()
		{
			TestGetPickMethodAndLocation(setRCAOnPickLine: true, setRCAOnBothPickLines: true);
		}

		void TestGetPickMethodAndLocation(bool setRCAOnPickLine, bool setRCAOnBothPickLines)
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, false);

			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, true, true);
			helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, true, true);

			Factory.Save();

			var location1 = data.Whs1.FindLocation("A-1-1");
			var location2 = data.Whs1.FindLocation("A-1-2");
			location1.WLV_PickMethod = "ABC";
			location2.WLV_PickMethod = "XYZ";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine1 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location1, "123");
			var receiveLine2 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location2, "456");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location1, "789");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part2, 1m, location1, "abc");

			receive.FinaliseDocketWithoutUserConfirmation();
			Assert("Receive is finalised.", receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 2);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 2);

			Factory.Save();
			var pick = helper.CreatePickNew(order);

			if (setRCAOnPickLine)
			{
				if (setRCAOnBothPickLines)
				{
					var releaseLine1 = orderLine1.ReleaseLines[0];
					releaseLine1.PartAttribute1 = "AAA";
					releaseLine1.Quantity = 2;
					var releaseLine2 = orderLine2.ReleaseLines[0];
					releaseLine2.PartAttribute1 = "AAA";
					releaseLine2.Quantity = 2;
				}
				else
				{
					var releaseLine1 = orderLine1.ReleaseLines[0];
					releaseLine1.PartAttribute1 = "";
					releaseLine1.Quantity = 1;
					var releaseLine2 = orderLine1.ReleaseLines.AddNew();
					releaseLine2.PartAttribute1 = "AAA";
					releaseLine2.Quantity = 1;

					var releaseLine3 = orderLine2.ReleaseLines[0];
					releaseLine3.PartAttribute1 = "";
					releaseLine3.Quantity = 1;
					var releaseLine4 = orderLine2.ReleaseLines.AddNew();
					releaseLine4.PartAttribute1 = "AAA";
					releaseLine4.Quantity = 1;
				}
			}

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package1 = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			var packageWithDifferentPickMethods = package1.Pack(orderLine1.ReleaseLines[0], 2m).Single();

			var package2 = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B2");
			var packageWithSamePickMethods = package2.Pack(orderLine2.ReleaseLines[0], 2m).Single();

			PackageWrapperFromPkgPackage wrapper1;
			PackageWrapperFromPkgPackage wrapper2;

			if (setRCAOnPickLine && !setRCAOnBothPickLines)
			{
				AssertEquals("Precondition: 2 ReleaseLines on orderLine1.", 2, orderLine1.ReleaseLines.Count);
				var anotherPackageWrapper1 = package1.Pack(orderLine1.ReleaseLines[1], 1m).Single();
				wrapper1 = new PackageWrapperFromPkgPackage(package1, new[] { packageWithDifferentPickMethods, anotherPackageWrapper1 }, Factory, 1, 1);
			}
			else
			{
				wrapper1 = new PackageWrapperFromPkgPackage(package1, new[] { packageWithDifferentPickMethods }, Factory, 1, 1);
			}
			AssertEquals("Package with different pick method should display 'MULTIPLE'.", "MULTIPLE", wrapper1.PickMethod);
			AssertEquals("Package with different pick location should display 'MULTIPLE'.", "MULTIPLE", wrapper1.PickLocation);

			if (setRCAOnPickLine && !setRCAOnBothPickLines)
			{
				AssertEquals("Precondition: 2 ReleaseLines on orderLine2.", 2, orderLine2.ReleaseLines.Count);
				var anotherPackageWrapper2 = package2.Pack(orderLine2.ReleaseLines[1], 1m).Single();
				wrapper2 = new PackageWrapperFromPkgPackage(package2, new[] { packageWithSamePickMethods, anotherPackageWrapper2 }, Factory, 1, 1);
			}
			else
			{
				wrapper2 = new PackageWrapperFromPkgPackage(package2, new[] { packageWithSamePickMethods }, Factory, 1, 1);
			}
			AssertEquals("Package with same pick method should display the location.", "ABC", wrapper2.PickMethod);
			AssertEquals("Package with same pick location should display the location.", "A-1-1", wrapper2.PickLocation);
		}

		public void TestGetPickMethodAndLocation_InTransit()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory, 2, 2);
			Factory.Save();

			var location = data.Whs1.FindLocation("A-1-1");
			location.WLV_PickMethod = "ABC";

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine3 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, "789");
			var receiveLine4 = Helper.CreateWhsReceiveLine(receive, data.Part1, 1m, location, "123");

			receive.FinaliseDocketWithoutUserConfirmation();
			Assert("Receive is finalised.", receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 2m);

			Factory.Save();
			var pick = helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			var packageWithSamePickMethods = package.Pack(orderLine.ReleaseLines[0], 2m).Single();

			var pickLine1 = orderLine.PickLines.Single(pl => pl.InventoryLine.WE_PalletID == "789");
			var pickLine2 = orderLine.PickLines.Single(pl => pl.InventoryLine.WE_PalletID == "123");
			Helper.PickAndMakeInTransitTransfer(pickLine1, ZDateTimeOffset.Now);
			Helper.PickAndMakeInTransitTransfer(pickLine2, ZDateTimeOffset.Now);

			var wrapper = new PackageWrapperFromPkgPackage(package, new[] { packageWithSamePickMethods }, Factory, 1, 1);
			AssertEquals("Package with same pick method should display the pick method.", "ABC", wrapper.PickMethod);
			AssertEquals("Package with same pick location should display the location.", "A-1-1", wrapper.PickLocation);
		}

		#endregion

		#region TestGetParent

		public void TestGetParent()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 2m);
			var pick = helper.CreatePickNew(order);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			package.KP_PackageID = "TI1";

			var package2 = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			package2.KP_PackageID = "TI1";

			var packageJobWrapper = new FreightWrapperFromPkgPackageJob(packageJob, Factory);
			AssertEquals("Packages should include all (2) packages for this package job. We have no overridden the package.", 2, packageJobWrapper.Packages.Count);

			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("There is only one package in this package job.", 1, wrapper.Parent.Packages.Count);
			AssertEquals("Parent package should be the package specified.", package, wrapper.Parent.Packages[0].WrappedObject);
		}

		#endregion

		#region TestGetPackageState

		public void TestGetPackageState()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var pkgPackage = packageJob.Packages.AddNew();
			var packageWrapper = new PackageWrapperFromPkgPackage(pkgPackage, Factory);

			AssertEquals(typeof(PackageStateWrapper), packageWrapper.PackageState.GetType());
		}

		#endregion

		#region TestGetPackageBookedDetail

		public void TestGetPackageBookedDetail()
		{
			var package = Factory.New<PkgPackage>();
			package.BookedDimensions.KPB_Volume = 1;
			package.BookedDimensions.KPB_VolumeUQ = "M3";

			var packageWrapper = new PackageWrapperFromPkgPackage(package, Factory);
			var packageBookedDetail = packageWrapper.GetPackageBookedDetail();

			AssertEquals(typeof(PkgPackageBookedDetail), packageBookedDetail.GetType());
			AssertEquals(1m, packageBookedDetail.KPB_Volume);
			AssertEquals("M3", packageBookedDetail.KPB_VolumeUQ);
		}

		#endregion

		#region TestGetMostRecentAudit

		public void TestGetMostRecentAudit_SingleAudit()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "Order", Data.Part1, 50m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var audit = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);

			AssertNotNull("Most recent audit must be set.", wrapper.MostRecentAudit);
			AssertEquals("Most recent audit must be returned.", audit, wrapper.MostRecentAudit.WrappedObject);
		}

		public void TestGetMostRecentAudit_MultipleAudits()
		{
			var order = Helper.CreateWhsOrderWithOrderLine(Data.Org1, Data.Whs1, "Order", Data.Part1, 50m);
			var package = PackingHelper.CreatePackage(order, order.WD_ExternalReference, 1, "PLT");
			var znow = ZDateTimeOffset.Now;
			var audit1 = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m, znow.AddHours(-2));
			var audit2 = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 48m, znow.AddHours(-1));
			var audit3 = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 51m, znow);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);

			AssertNotNull("Most recent audit must be set.", wrapper.MostRecentAudit);
			AssertEquals("Most recent audit must be returned.", audit3, wrapper.MostRecentAudit.WrappedObject);
		}

		public void TestGetMostRecentAudit_NotAnOrderType()
		{
			var docket = Factory.New<DtbBookingConsolidation>();
			var package = PackingHelper.CreatePackage(docket, "PKG1", 1, "PLT");
			var znow = ZDateTimeOffset.Now;
			var audit1 = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 50m, 49m, znow.AddHours(-2));
			var audit2 = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 30m, 29m, znow.AddHours(-1));
			var audit3 = Helper.CreateWhsPackageAuditWithLineFailure(package, Data.Part1, 20m, 19m, znow);
			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertNull("Most recent audit must be null since the docket is not an order.", wrapper.MostRecentAudit);
		}

		#endregion

		#region TestGetHasSingleProduct

		public void TestGetHasSingleProduct_SingleProduct()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 5);
			var orderLine2 = helper.CreateWhsOrderLine(order1, data.Part1, 5);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var outerPack0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B0");

			var outerPack1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");
			var outerPackedItem1 = outerPack1.Pack(orderLine1.ReleaseLines[0], 5m).Single();

			var innerPack1 = PackingHelper.CreatePackage(outerPack1, 1, Constants.PkgUnit.Box, "B2");
			var innerPackedItem1 = innerPack1.Pack_ForTesting(orderLine2.ReleaseLines[0], 5m);

			// Package Job
			//	1x Box B0
			//	1x Box B1
			//		Part1
			//		1x Box B2
			//			Part1

			PackingHelper.CreatePackage(innerPack1, 1, Constants.PkgUnit.Box, "B6");

			var wrapper1 = new PackageWrapperFromPkgPackage(outerPack0, Factory, 2, 3);
			AssertEquals(nameof(wrapper1.HasSingleProduct), false, wrapper1.HasSingleProduct);

			var wrapper2 = new PackageWrapperFromPkgPackage(outerPack1, new[] { outerPackedItem1, innerPackedItem1 }, Factory, 2, 3);
			AssertEquals(nameof(wrapper2.HasSingleProduct), true, wrapper2.HasSingleProduct);

			var wrapper3 = new PackageWrapperFromPkgPackage(innerPack1, new[] { innerPackedItem1 }, Factory, 2, 3);
			AssertEquals(nameof(wrapper3.HasSingleProduct), true, wrapper3.HasSingleProduct);
		}

		public void TestGetHasSingleProduct_MultipleProducts()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 5);
			var orderLine2 = helper.CreateWhsOrderLine(order1, data.Part2, 5);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var outerPack0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B0");

			var outerPack1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");
			var outerPackedItem1 = outerPack1.Pack(orderLine1.ReleaseLines[0], 5m).Single();

			var innerPack1 = PackingHelper.CreatePackage(outerPack1, 1, Constants.PkgUnit.Box, "B2");
			var innerPackedItem1 = innerPack1.Pack_ForTesting(orderLine2.ReleaseLines[0], 5m);

			// Package Job
			//	1x Box B0
			//	1x Box B1
			//		Part1
			//		1x Box B2
			//			Part2

			PackingHelper.CreatePackage(innerPack1, 1, Constants.PkgUnit.Box, "B6");

			var wrapper1 = new PackageWrapperFromPkgPackage(outerPack0, Factory, 2, 3);
			AssertEquals(nameof(wrapper1.HasSingleProduct), false, wrapper1.HasSingleProduct);

			var wrapper2 = new PackageWrapperFromPkgPackage(outerPack1, new[] { outerPackedItem1, innerPackedItem1 }, Factory, 2, 3);
			AssertEquals(nameof(wrapper2.HasSingleProduct), false, wrapper2.HasSingleProduct);

			var wrapper3 = new PackageWrapperFromPkgPackage(innerPack1, new[] { innerPackedItem1 }, Factory, 2, 3);
			AssertEquals(nameof(wrapper3.HasSingleProduct), true, wrapper3.HasSingleProduct);
		}

		public void TestGetHasSingleProduct_SingleProduct_DifferentAttributes()
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var today = ZDate.Today;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			var receiveLine1 = helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine1.WE_PartAttrib1 = "A1";
			receiveLine1.WE_PartAttrib2 = "A1";
			receiveLine1.WE_PartAttrib3 = "A1";
			receiveLine1.WE_SerialNumber = "A1";
			receiveLine1.WE_ExpiryDate = today.AddMonths(1);
			receiveLine1.WE_PackingDate = today.AddMonths(-1);

			var receiveLine2 = helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			receiveLine2.WE_PartAttrib1 = "A2";
			receiveLine2.WE_PartAttrib2 = "A2";
			receiveLine2.WE_PartAttrib3 = "A2";
			receiveLine2.WE_SerialNumber = "A2";
			receiveLine2.WE_ExpiryDate = today.AddMonths(2);
			receiveLine2.WE_PackingDate = today.AddMonths(-2);

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 1);
			var orderLine2 = helper.CreateWhsOrderLine(order1, data.Part1, 1);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var outerPack0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B0");

			var outerPack1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");
			var outerPackedItem1 = outerPack1.Pack(orderLine1.ReleaseLines[0], 1m).Single();

			var innerPack1 = PackingHelper.CreatePackage(outerPack1, 1, Constants.PkgUnit.Box, "B2");
			var innerPackedItem1 = innerPack1.Pack_ForTesting(orderLine2.ReleaseLines[0], 1m);

			// Package Job
			//	1x Box B0
			//	1x Box B1
			//		Part1
			//		1x Box B2
			//			Part1

			PackingHelper.CreatePackage(innerPack1, 1, Constants.PkgUnit.Box, "B6");

			var wrapper1 = new PackageWrapperFromPkgPackage(outerPack0, Factory, 2, 3);
			AssertEquals(nameof(wrapper1.HasSingleProduct), false, wrapper1.HasSingleProduct);

			var wrapper2 = new PackageWrapperFromPkgPackage(outerPack1, new[] { outerPackedItem1, innerPackedItem1 }, Factory, 2, 3);
			AssertEquals(nameof(wrapper2.HasSingleProduct), true, wrapper2.HasSingleProduct);

			var wrapper3 = new PackageWrapperFromPkgPackage(outerPack1, Factory, 2, 3);
			AssertEquals(nameof(wrapper3.HasSingleProduct), true, wrapper3.HasSingleProduct);

			var wrapper4 = new PackageWrapperFromPkgPackage(innerPack1, new[] { innerPackedItem1 }, Factory, 2, 3);
			AssertEquals(nameof(wrapper4.HasSingleProduct), true, wrapper4.HasSingleProduct);
		}

		#endregion

		#region TestGetHasSingleAttribute1

		public void TestGetHasSingleAttribute1_SingleProduct_SingleAttribute()
			=> TestGetHasSingleAttribute1Core(isSingleProductTest: true, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute1_SingleProduct_MultipleAttributes()
			=> TestGetHasSingleAttribute1Core(isSingleProductTest: true, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute1_MultipleProducts_SingleAttribute()
			=> TestGetHasSingleAttribute1Core(isSingleProductTest: false, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute1_MultipleProducts_MultipleAttributes()
			=> TestGetHasSingleAttribute1Core(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute1_AttributeNotEnabled()
			=> TestGetHasSingleAttribute1Core(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: false);

		void TestGetHasSingleAttribute1Core(bool isSingleProductTest, bool isSingleAttributeTest, bool isAttributeEnabledForTest)
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.One, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.One, isAttributeEnabledForTest);

			var today = ZDate.Today;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			var receiveLine1 = helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLine2 = helper.CreateWhsReceiveLine(receive, isSingleProductTest ? data.Part1 : data.Part2, 1m);

			if (isAttributeEnabledForTest)
			{
				receiveLine1.WE_PartAttrib1 = "A1";
				receiveLine2.WE_PartAttrib1 = isSingleAttributeTest ? "A1" : "A2";
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 1);
			var orderLine2 = helper.CreateWhsOrderLine(order1, isSingleProductTest ? data.Part1 : data.Part2, 1);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var outerPack0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B0");

			var outerPack1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");
			var outerPackedItem1 = outerPack1.Pack(orderLine1.ReleaseLines[0], 1m).Single();

			var innerPack1 = PackingHelper.CreatePackage(outerPack1, 1, Constants.PkgUnit.Box, "B2");
			var innerPackedItem1 = innerPack1.Pack_ForTesting(orderLine2.ReleaseLines[0], 1m);

			// Package Job
			//	1x Box B0
			//	1x Box B1
			//		ReleaseLine1
			//		1x Box B2
			//			ReleaseLine2

			PackingHelper.CreatePackage(innerPack1, 1, Constants.PkgUnit.Box, "B6");

			var wrapper1 = new PackageWrapperFromPkgPackage(outerPack0, Factory, 2, 3);
			AssertEquals("Empty Package will never have Single Attribute.", false, wrapper1.HasSingleAttribute1);

			var wrapper2 = new PackageWrapperFromPkgPackage(outerPack1, new[] { outerPackedItem1, innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper2.HasSingleAttribute1);

			var wrapper3 = new PackageWrapperFromPkgPackage(outerPack1, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper3.HasSingleAttribute1);

			var wrapper4 = new PackageWrapperFromPkgPackage(innerPack1, new[] { innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Single Package only should be considered.", isAttributeEnabledForTest, wrapper4.HasSingleAttribute1);
		}

		#endregion

		#region TestGetHasSingleAttribute2

		public void TestGetHasSingleAttribute2_SingleProduct_SingleAttribute()
			=> TestGetHasSingleAttribute2Core(isSingleProductTest: true, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute2_SingleProduct_MultipleAttributes()
			=> TestGetHasSingleAttribute2Core(isSingleProductTest: true, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute2_MultipleProducts_SingleAttribute()
			=> TestGetHasSingleAttribute2Core(isSingleProductTest: false, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute2_MultipleProducts_MultipleAttributes()
			=> TestGetHasSingleAttribute2Core(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute2_AttributeNotEnabled()
			=> TestGetHasSingleAttribute2Core(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: false);

		void TestGetHasSingleAttribute2Core(bool isSingleProductTest, bool isSingleAttributeTest, bool isAttributeEnabledForTest)
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Two, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Two, isAttributeEnabledForTest);

			var today = ZDate.Today;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			var receiveLine1 = helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLine2 = helper.CreateWhsReceiveLine(receive, isSingleProductTest ? data.Part1 : data.Part2, 1m);

			if (isAttributeEnabledForTest)
			{
				receiveLine1.WE_PartAttrib2 = "A1";
				receiveLine2.WE_PartAttrib2 = isSingleAttributeTest ? "A1" : "A2";
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 1);
			var orderLine2 = helper.CreateWhsOrderLine(order1, isSingleProductTest ? data.Part1 : data.Part2, 1);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var outerPack0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B0");

			var outerPack1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");
			var outerPackedItem1 = outerPack1.Pack(orderLine1.ReleaseLines[0], 1m).Single();

			var innerPack1 = PackingHelper.CreatePackage(outerPack1, 1, Constants.PkgUnit.Box, "B2");
			var innerPackedItem1 = innerPack1.Pack_ForTesting(orderLine2.ReleaseLines[0], 1m);

			// Package Job
			//	1x Box B0
			//	1x Box B1
			//		ReleaseLine1
			//		1x Box B2
			//			ReleaseLine2

			PackingHelper.CreatePackage(innerPack1, 1, Constants.PkgUnit.Box, "B6");

			var wrapper1 = new PackageWrapperFromPkgPackage(outerPack0, Factory, 2, 3);
			AssertEquals("Empty Package will never have Single Attribute.", false, wrapper1.HasSingleAttribute2);

			var wrapper2 = new PackageWrapperFromPkgPackage(outerPack1, new[] { outerPackedItem1, innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper2.HasSingleAttribute2);

			var wrapper3 = new PackageWrapperFromPkgPackage(outerPack1, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper3.HasSingleAttribute2);

			var wrapper4 = new PackageWrapperFromPkgPackage(innerPack1, new[] { innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Single Package only should be considered.", isAttributeEnabledForTest, wrapper4.HasSingleAttribute2);
		}

		#endregion

		#region TestGetHasSingleAttribute3

		public void TestGetHasSingleAttribute3_SingleProduct_SingleAttribute()
			=> TestGetHasSingleAttribute3Core(isSingleProductTest: true, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute3_SingleProduct_MultipleAttributes()
			=> TestGetHasSingleAttribute3Core(isSingleProductTest: true, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute3_MultipleProducts_SingleAttribute()
			=> TestGetHasSingleAttribute3Core(isSingleProductTest: false, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute3_MultipleProducts_MultipleAttributes()
			=> TestGetHasSingleAttribute3Core(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSingleAttribute3_AttributeNotEnabled()
			=> TestGetHasSingleAttribute3Core(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: false);

		void TestGetHasSingleAttribute3Core(bool isSingleProductTest, bool isSingleAttributeTest, bool isAttributeEnabledForTest)
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.Three, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.Three, isAttributeEnabledForTest);

			var today = ZDate.Today;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			var receiveLine1 = helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLine2 = helper.CreateWhsReceiveLine(receive, isSingleProductTest ? data.Part1 : data.Part2, 1m);

			if (isAttributeEnabledForTest)
			{
				receiveLine1.WE_PartAttrib3 = "A1";
				receiveLine2.WE_PartAttrib3 = isSingleAttributeTest ? "A1" : "A2";
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 1);
			var orderLine2 = helper.CreateWhsOrderLine(order1, isSingleProductTest ? data.Part1 : data.Part2, 1);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var outerPack0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B0");

			var outerPack1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");
			var outerPackedItem1 = outerPack1.Pack(orderLine1.ReleaseLines[0], 1m).Single();

			var innerPack1 = PackingHelper.CreatePackage(outerPack1, 1, Constants.PkgUnit.Box, "B2");
			var innerPackedItem1 = innerPack1.Pack_ForTesting(orderLine2.ReleaseLines[0], 1m);

			// Package Job
			//	1x Box B0
			//	1x Box B1
			//		ReleaseLine1
			//		1x Box B2
			//			ReleaseLine2

			PackingHelper.CreatePackage(innerPack1, 1, Constants.PkgUnit.Box, "B6");

			var wrapper1 = new PackageWrapperFromPkgPackage(outerPack0, Factory, 2, 3);
			AssertEquals("Empty Package will never have Single Attribute.", false, wrapper1.HasSingleAttribute3);

			var wrapper2 = new PackageWrapperFromPkgPackage(outerPack1, new[] { outerPackedItem1, innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper2.HasSingleAttribute3);

			var wrapper3 = new PackageWrapperFromPkgPackage(outerPack1, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper3.HasSingleAttribute3);

			var wrapper4 = new PackageWrapperFromPkgPackage(innerPack1, new[] { innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Single Package only should be considered.", isAttributeEnabledForTest, wrapper4.HasSingleAttribute3);
		}

		#endregion

		#region TestGetHasSingleExpiryDate

		public void TestGetHasSingleExpiryDate_SingleProduct_SingleAttribute()
			=> TestGetHasSingleExpiryDateCore(isSingleProductTest: true, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSingleExpiryDate_SingleProduct_MultipleAttributes()
			=> TestGetHasSingleExpiryDateCore(isSingleProductTest: true, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSingleExpiryDate_MultipleProducts_SingleAttribute()
			=> TestGetHasSingleExpiryDateCore(isSingleProductTest: false, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSingleExpiryDate_MultipleProducts_MultipleAttributes()
			=> TestGetHasSingleExpiryDateCore(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSingleExpiryDate_AttributeNotEnabled()
			=> TestGetHasSingleExpiryDateCore(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: false);

		void TestGetHasSingleExpiryDateCore(bool isSingleProductTest, bool isSingleAttributeTest, bool isAttributeEnabledForTest)
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.ExpiryDate, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.ExpiryDate, isAttributeEnabledForTest);

			var today = ZDate.Today;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			var receiveLine1 = helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLine2 = helper.CreateWhsReceiveLine(receive, isSingleProductTest ? data.Part1 : data.Part2, 1m);

			if (isAttributeEnabledForTest)
			{
				receiveLine1.WE_ExpiryDate = today.AddDays(7);
				receiveLine2.WE_ExpiryDate = isSingleAttributeTest ? today.AddDays(7) : today.AddDays(14);
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 1);
			var orderLine2 = helper.CreateWhsOrderLine(order1, isSingleProductTest ? data.Part1 : data.Part2, 1);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var outerPack0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B0");

			var outerPack1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");
			var outerPackedItem1 = outerPack1.Pack(orderLine1.ReleaseLines[0], 1m).Single();

			var innerPack1 = PackingHelper.CreatePackage(outerPack1, 1, Constants.PkgUnit.Box, "B2");
			var innerPackedItem1 = innerPack1.Pack_ForTesting(orderLine2.ReleaseLines[0], 1m);

			// Package Job
			//	1x Box B0
			//	1x Box B1
			//		ReleaseLine1
			//		1x Box B2
			//			ReleaseLine2

			PackingHelper.CreatePackage(innerPack1, 1, Constants.PkgUnit.Box, "B6");

			var wrapper1 = new PackageWrapperFromPkgPackage(outerPack0, Factory, 2, 3);
			AssertEquals("Empty Package will never have Single Attribute.", false, wrapper1.HasSingleExpiryDate);

			var wrapper2 = new PackageWrapperFromPkgPackage(outerPack1, new[] { outerPackedItem1, innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper2.HasSingleExpiryDate);

			var wrapper3 = new PackageWrapperFromPkgPackage(outerPack1, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper3.HasSingleExpiryDate);

			var wrapper4 = new PackageWrapperFromPkgPackage(innerPack1, new[] { innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Single Package only should be considered.", isAttributeEnabledForTest, wrapper4.HasSingleExpiryDate);
		}

		#endregion

		#region TestGetHasSinglePackingDate

		public void TestGetHasSinglePackingDate_SingleProduct_SingleAttribute()
			=> TestGetHasSinglePackingDateCore(isSingleProductTest: true, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSinglePackingDate_SingleProduct_MultipleAttributes()
			=> TestGetHasSinglePackingDateCore(isSingleProductTest: true, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSinglePackingDate_MultipleProducts_SingleAttribute()
			=> TestGetHasSinglePackingDateCore(isSingleProductTest: false, isSingleAttributeTest: true, isAttributeEnabledForTest: true);

		public void TestGetHasSinglePackingDate_MultipleProducts_MultipleAttributes()
			=> TestGetHasSinglePackingDateCore(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: true);

		public void TestGetHasSinglePackingDate_AttributeNotEnabled()
			=> TestGetHasSinglePackingDateCore(isSingleProductTest: false, isSingleAttributeTest: false, isAttributeEnabledForTest: false);

		void TestGetHasSinglePackingDateCore(bool isSingleProductTest, bool isSingleAttributeTest, bool isAttributeEnabledForTest)
		{
			var helper = new WhsTestHelperFunctions(Factory);
			var data = new TestDataSimpleEnvironment(Factory);

			helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part1, AttributeNumber.PackingDate, isAttributeEnabledForTest);
			helper.SetProductAttributeUse(data.Org1, data.Part2, AttributeNumber.PackingDate, isAttributeEnabledForTest);

			var today = ZDate.Today;
			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "r1", new TestNotificationBuffer());
			var receiveLine1 = helper.CreateWhsReceiveLine(receive, data.Part1, 1m);
			var receiveLine2 = helper.CreateWhsReceiveLine(receive, isSingleProductTest ? data.Part1 : data.Part2, 1m);

			if (isAttributeEnabledForTest)
			{
				receiveLine1.WE_PackingDate = today.AddDays(-7);
				receiveLine2.WE_PackingDate = isSingleAttributeTest ? today.AddDays(-7) : today.AddDays(-14);
			}

			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();

			AssertEquals("Check receive is finalised", true, receive.IsFinalised);
			var order1 = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1");
			var orderLine1 = helper.CreateWhsOrderLine(order1, data.Part1, 1);
			var orderLine2 = helper.CreateWhsOrderLine(order1, isSingleProductTest ? data.Part1 : data.Part2, 1);
			Factory.Save();

			var pick = helper.CreatePickNew(order1);

			var packageJob1 = PkgPackageJob.LoadOrCreatePackageJob(order1);
			var outerPack0 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B0");

			var outerPack1 = PackingHelper.CreatePackage(packageJob1, 1, Constants.PkgUnit.Box, "B1");
			var outerPackedItem1 = outerPack1.Pack(orderLine1.ReleaseLines[0], 1m).Single();

			var innerPack1 = PackingHelper.CreatePackage(outerPack1, 1, Constants.PkgUnit.Box, "B2");
			var innerPackedItem1 = innerPack1.Pack_ForTesting(orderLine2.ReleaseLines[0], 1m);

			// Package Job
			//	1x Box B0
			//	1x Box B1
			//		ReleaseLine1
			//		1x Box B2
			//			ReleaseLine2

			PackingHelper.CreatePackage(innerPack1, 1, Constants.PkgUnit.Box, "B6");

			var wrapper1 = new PackageWrapperFromPkgPackage(outerPack0, Factory, 2, 3);
			AssertEquals("Empty Package will never have Single Attribute.", false, wrapper1.HasSinglePackingDate);

			var wrapper2 = new PackageWrapperFromPkgPackage(outerPack1, new[] { outerPackedItem1, innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper2.HasSinglePackingDate);

			var wrapper3 = new PackageWrapperFromPkgPackage(outerPack1, Factory, 2, 3);
			AssertEquals("Package including inners should be considered.", isSingleProductTest && isSingleAttributeTest, wrapper3.HasSinglePackingDate);

			var wrapper4 = new PackageWrapperFromPkgPackage(innerPack1, new[] { innerPackedItem1 }, Factory, 2, 3);
			AssertEquals("Single Package only should be considered.", isAttributeEnabledForTest, wrapper4.HasSinglePackingDate);
		}

		#endregion

		#region TestStarTrack_QRCodeText

		public new void TestStarTrack_QRCodeText()
		{
			AssertEquals(ZString.Empty.PadRight(334), Wrapper.StarTrack_QRCodeText);
		}

		#endregion

		#region TestContainerNo

		public void TestContainerNo()
		{
			var packageJob = Factory.New<PkgPackageJob>();
			var containerPackage = packageJob.Packages.AddNew(Constants.PkgUnit.Container);
			containerPackage.KP_PackageID = "TestContainer";
			containerPackage.KP_PackageQty = 1;
			var innerContainerPackage = containerPackage.Packages.AddNew(Constants.PkgUnit.Box);
			var innerInnerContainerPackage = innerContainerPackage.Packages.AddNew(Constants.PkgUnit.Bag);

			var nonContainerPackage = packageJob.Packages.AddNew(Constants.PkgUnit.Pallet);
			var innerNonContainerPackage = nonContainerPackage.Packages.AddNew(Constants.PkgUnit.Unit);

			var containerPackageWrapper = new PackageWrapperFromPkgPackage(containerPackage, Factory);
			AssertEquals("Container package wrapper.", "TestContainer", containerPackageWrapper.ContainerNo);

			var innerContainerPackageWrapper = new PackageWrapperFromPkgPackage(innerContainerPackage, Factory);
			AssertEquals("Inner container package wrapper.", "TestContainer", innerContainerPackageWrapper.ContainerNo);

			var innerInnerContainerPackageWrapper = new PackageWrapperFromPkgPackage(innerInnerContainerPackage, Factory);
			AssertEquals("Inner inner container package wrapper.", "TestContainer", innerInnerContainerPackageWrapper.ContainerNo);

			var nonContainerPackageWrapper = new PackageWrapperFromPkgPackage(nonContainerPackage, Factory);
			AssertEquals("Noncontainer package wrapper.", ZString.Empty, nonContainerPackageWrapper.ContainerNo);

			var innerNonContainerPackageWrapper = new PackageWrapperFromPkgPackage(innerNonContainerPackage, Factory);
			AssertEquals("Inner noncontainer package wrapper.", ZString.Empty, innerNonContainerPackageWrapper.ContainerNo);
		}

		#endregion

		#region TestAreaName

		public void TestGetAreaName()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA1.PickingArea.WA_Name = "TestAreaNameZZZ";
			var area2 = Helper.CreateArea(data.Whs1, "SomeOtherArea");
			locationA2.WLV_WA_PickingArea = area2.PK;

			Factory.Save(); // have to save before changing WLV_PickPathSequence
			locationA1.WLV_PickPathSequence = 2;
			locationA2.WLV_PickPathSequence = 1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locationA2, "");
			Factory.Save();

			AssertEquals("Precondition: pick path sequence is set correctly", 2, locationA1.WLV_PickPathSequence);
			AssertEquals("Precondition: pick path sequence is set correctly", 1, locationA2.WLV_PickPathSequence);

			var packageJob = Factory.New<PkgPackageJob>();
			var package = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var wrapper = new PackageWrapperFromPkgPackage(package, package.Factory);

			AssertEquals("If no picklines attached to the package - area name should be empty.", "", wrapper.AreaName);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineLocationA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.LocationString == "A-1");
			var pickLineLocationA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.LocationString == "A-2");
			pickLineLocationA2.WZ_ReleaseCapturedPartAttrib1 = "AAA";
			package = Helper.CreatePackage("KEG", "123", order.PackageJob.Packages);
			PackingHelper.CreatePackageDivot(package, pickLineLocationA1); // divot to pickline
			PackingHelper.CreatePackageDivot(package, pickLineLocationA2); // divot to RCA
			wrapper = new PackageWrapperFromPkgPackage(package, package.Factory);

			AssertEquals("Area name must be taken from a picklines linked to location with the smallest PickPathSequence.", "SomeOtherArea", wrapper.AreaName);
		}

		public void TestGetAreaName_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");
			locationA1.PickingArea.WA_Name = "TestAreaNameZZZ";

			var area2 = Helper.CreateArea(data.Whs1, "SomeOtherArea");
			locationA2.WLV_WA_PickingArea = area2.PK;

			Factory.Save(); // have to save before changing WLV_PickPathSequence
			locationA1.WLV_PickPathSequence = 2;
			locationA2.WLV_PickPathSequence = 1;

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locationA1, "");
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locationA2, "");
			Factory.Save();

			AssertEquals("Precondition: pick path sequence is set correctly", 2, locationA1.WLV_PickPathSequence);
			AssertEquals("Precondition: pick path sequence is set correctly", 1, locationA2.WLV_PickPathSequence);

			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
			var pick = Helper.CreatePickNew(order);
			var pickLineLocationA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.LocationString == "A-1");
			var pickLineLocationA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.LocationString == "A-2");

			var package = Helper.CreatePackage("KEG", "123", order.PackageJob.Packages);
			PackingHelper.CreatePackageDivot(package, pickLineLocationA1);
			PackingHelper.CreatePackageDivot(package, pickLineLocationA2);

			Helper.PickAndMakeInTransitTransfer(pickLineLocationA1, ZDateTimeOffset.Now);
			Helper.PickAndMakeInTransitTransfer(pickLineLocationA2, ZDateTimeOffset.Now);

			var wrapper = new PackageWrapperFromPkgPackage(package, package.Factory);
			AssertEquals("Area name must be taken from a picklines linked to location with the smallest PickPathSequence.", "SomeOtherArea", wrapper.AreaName);
		}

		#endregion

		#region TestPickGroup

		public void TestGetPickGroup()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			var pickGroup2 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Group 1";
			pickGroup2.Description = (NoResString)"Group 2";
			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				Factory.Save(); // have to save before changing WLV_PickPathSequence
				locationA1.WLV_PickPathSequence = 2;
				locationA2.WLV_PickPathSequence = 1;

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locationA1, "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locationA2, "");
				Factory.Save();

				AssertEquals("Precondition: pick path sequence is set correctly", 2, locationA1.WLV_PickPathSequence);
				AssertEquals("Precondition: pick path sequence is set correctly", 1, locationA2.WLV_PickPathSequence);

				var packageJob = Factory.New<PkgPackageJob>();
				var package = Helper.CreatePackage("KEG", "123", packageJob.Packages);
				var wrapper = new PackageWrapperFromPkgPackage(package, package.Factory);

				AssertEquals("If no picklines attached to the package - pick group should be empty.", "", wrapper.AreaName);

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
				var pick = Helper.CreatePickNew(order);
				var pickLineLocationA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.LocationString == "A-1");
				var pickLineLocationA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.LocationString == "A-2");
				pickLineLocationA1.DocketLine.WE_PickGroup = new ZShort(pickGroup1.Code);
				pickLineLocationA2.DocketLine.WE_PickGroup = new ZShort(pickGroup2.Code);
				package = Helper.CreatePackage("KEG", "123", order.PackageJob.Packages);
				PackingHelper.CreatePackageDivot(package, pickLineLocationA1); // divot to pickline
				wrapper = new PackageWrapperFromPkgPackage(package, package.Factory);

				AssertEquals("Pick group must be taken from a picklines linked to location with the smallest PickPathSequence.", "Group 2", wrapper.PickGroup);
			}
		}

		public void TestGetPickGroup_InTransit()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var locationA1 = data.Whs1.FindLocation("A-1");
			var locationA2 = data.Whs1.FindLocation("A-2");

			var collection = new PickGroupCollection();
			var pickGroup1 = collection.AddNew();
			var pickGroup2 = collection.AddNew();
			pickGroup1.Description = (NoResString)"Group 1";
			pickGroup2.Description = (NoResString)"Group 2";
			using (WarehouseDataRegistry.Instance.PickGroups.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			{
				Factory.Save(); // have to save before changing WLV_PickPathSequence
				locationA1.WLV_PickPathSequence = 2;
				locationA2.WLV_PickPathSequence = 1;

				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 1, locationA1, "");
				Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R2", data.Part1, 1, locationA2, "");
				Factory.Save();

				AssertEquals("Precondition: pick path sequence is set correctly", 2, locationA1.WLV_PickPathSequence);
				AssertEquals("Precondition: pick path sequence is set correctly", 1, locationA2.WLV_PickPathSequence);

				var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 2);
				var pick = Helper.CreatePickNew(order);
				var pickLineLocationA1 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.LocationString == "A-1");
				var pickLineLocationA2 = pick.GetAllPickLines().Single(pl => pl.InventoryLine.LocationString == "A-2");
				pickLineLocationA1.DocketLine.WE_PickGroup = new ZShort(pickGroup1.Code);
				pickLineLocationA2.DocketLine.WE_PickGroup = new ZShort(pickGroup2.Code);

				var package = Helper.CreatePackage("KEG", "123", order.PackageJob.Packages);
				PackingHelper.CreatePackageDivot(package, pickLineLocationA1);
				PackingHelper.CreatePackageDivot(package, pickLineLocationA2);

				Helper.PickAndMakeInTransitTransfer(pickLineLocationA1, ZDateTimeOffset.Now);
				Helper.PickAndMakeInTransitTransfer(pickLineLocationA2, ZDateTimeOffset.Now);

				var wrapper = new PackageWrapperFromPkgPackage(package, package.Factory);
				AssertEquals("Pick group must be taken from a pickline linked to location with the smallest PickPathSequence.", "Group 2", wrapper.PickGroup);
			}
		}

		#endregion

		#region TestDamagedReason

		public void TestDamagedReason_DamageReasonIsEmpty()
		{
			var package = Factory.New<PkgPackage>();
			AssertEquals("Precondition:", "", package.KP_DamagedReason);

			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("", wrapper.DamagedReason.Code);
			AssertEquals("", wrapper.DamagedReason.Description);
		}

		public void TestDamagedReason_DamageReasonIsInvalid()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_DamagedReason = "XXX";

			var damagedReasonCodeAndDescriptionList = PackingRegistry.Instance.DamagedReasons.Value.GetCodeDescriptionPairList();
			AssertEquals("Precondition: Damaged Reason is not in list", false, damagedReasonCodeAndDescriptionList.ContainsCode(package.KP_DamagedReason));

			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("", wrapper.DamagedReason.Code);
			AssertEquals("", wrapper.DamagedReason.Description);
		}

		public void TestDamagedReason()
		{
			var package = Factory.New<PkgPackage>();
			package.KP_DamagedReason = "DAM";

			var damagedReasonCodeAndDescriptionList = new SystemDefinableCodeDescriptionBoolCollection()
			{
				{ new SystemDefinableCodeDescriptionBool { Code = "BRO", Description = (NoResString)"Broken" } },
				{ new SystemDefinableCodeDescriptionBool { Code = "DAM", Description = (NoResString)"Damage" } },
			};
			PackingRegistry.Instance.DamagedReasons.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, damagedReasonCodeAndDescriptionList);

			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals("DAM", wrapper.DamagedReason.Code);
			AssertEquals("Damage", wrapper.DamagedReason.Description);
		}

		#endregion

		#region TestUOMType

		public void TestUOMType_Empty()
		{
			TestUOMType_Core("BOX", "", "");
		}

		public void TestUOMType_Pallet()
		{
			TestUOMType_Core("PLT", "PLT", "Pallet");
		}

		public void TestUOMType_Case()
		{
			TestUOMType_Core("CTN", "CAS", "Case");
		}

		public void TestUOMType_SplitCase()
		{
			TestUOMType_Core("CAS", "SPC", "Split-Case");
		}

		void TestUOMType_Core(string packType, string uomCode, string uomDescription)
		{
			SetupUOMPackType();
			var package = Factory.New<PkgPackage>();
			package.KP_F3_NKPackType = packType;
			AssertEquals("Precondition:", uomCode, package.PackType.F3_UOMType);

			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertEquals(uomCode, wrapper.UOMType.Code);
			AssertEquals(uomDescription, wrapper.UOMType.Description);
		}

		void SetupUOMPackType()
		{
			var pallet = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, "PLT"));
			pallet.F3_UOMType = UOMPackTypesList.Codes.Pallet;
			var carton = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, "CTN"));
			carton.F3_UOMType = UOMPackTypesList.Codes.Case;
			var caseType = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, "CAS"));
			caseType.F3_UOMType = UOMPackTypesList.Codes.SplitCase;
			var box = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, "BOX"));
			box.F3_UOMType = "";
		}

		#endregion

		#region TestPackageOrderReference

		public void TestPackageOrderReference()
		{
			var package = Factory.New<PkgPackage>();
			var orderReference = Factory.New<PkgPackageOrderReference>();
			orderReference.KPO_KP_Package = package.PK;

			var wrapper = new PackageWrapperFromPkgPackage(package, Factory);
			AssertNotNull(wrapper.PackageOrderReference);
		}

		#endregion

		#region TestCustomsEntries

		protected override void TestCustomsEntriesCore()
		{
			var helper = new WhsTransitTestHelper(Factory);

			var package = Factory.New<PkgPackage>();
			var cusEntry1 = helper.CreateAdditionalReference(package, "REF1", "IOT");
			var packageWrapper = new PackageWrapperFromTransitPackage(package, Factory);

			AssertNotNull(packageWrapper.CustomsEntries);
			AssertEquals(1, packageWrapper.CustomsEntries.Count);

			var actualCusEntry = (CusEntryNumber)((CustomsEntryWrapper)packageWrapper.CustomsEntries.Single()).WrappedObject;

			AssertEquals(cusEntry1.PK, actualCusEntry.PK);
			AssertEquals(cusEntry1.CE_EntryType, actualCusEntry.CE_EntryType);
			AssertEquals(cusEntry1.CE_EntryNum, actualCusEntry.CE_EntryNum);
			AssertEquals(cusEntry1.CE_ParentID, actualCusEntry.CE_ParentID);
			AssertEquals(cusEntry1.CE_ParentTable, actualCusEntry.CE_ParentTable);
			AssertEquals(cusEntry1.CE_Category, actualCusEntry.CE_Category);
			AssertEquals(cusEntry1.CE_EntryStatus, actualCusEntry.CE_EntryStatus);
			AssertEquals(cusEntry1.CE_RN_NKCountryCode, actualCusEntry.CE_RN_NKCountryCode);
		}

		#endregion

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageWrapperFromPkgPackage(null, Factory);
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
PackageOrderReference :  is null
Packages : 1 PLT
PackageState : (No Default Field Value Available on PackageState)
PackedItem : (No Default Field Value Available on PackedItemEmpty)
Parent :  is null
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
			var pallet = Factory.New<PkgPackage>();
			pallet.KP_PackageQty = 1;
			pallet.KP_F3_NKPackType = Constants.PkgUnit.Pallet;

			return new PackageWrapperFromPkgPackage(pallet, Factory);
		}

		#endregion

		#region TestHelpers

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		TestDataSimpleEnvironment Data
		{
			get { return data ?? (data = new TestDataSimpleEnvironment(Factory, 2, 2)); }
		}
		TestDataSimpleEnvironment data;

		#endregion
	}
}
