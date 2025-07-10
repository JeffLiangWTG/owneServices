using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Packing.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transit.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(PackageWrapperFromPkgPackageHeader))]
	sealed class PackageWrapperFromPkgPackageHeaderTest : PackageWrapperTest
	{
		#region TestWrapperMappingFull

		public override void TestWrapperMappingFull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAllAttributeType(data.Org1, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part2, true, useSerialNumber: false);

			data.Whs1.WarehouseAddress.OA_RL_NKRelatedPortCode = "AU2CO";

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", helper.Notify);
			var inventory1 = helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 20m);
			var inventory2 = helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 20m, ZDate.Today.AddDays(10), ZDate.Today.AddDays(-10), "PA1", "PA2", "PA3", "");
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised.", true, receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 5m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 11m);

			var consignee = helper.CreateClient("CNE", "CNE SYDNEY");
			consignee.MainAddress.FillWithValidTestData();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consignee.MainAddress.OA_PostCode = "2213";
			order.ConsigneeDocAddress.E2_OA_Address = consignee.MainAddress.PK;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX", "123");
			var packageHeader = package.GetPackageHeader();
			packageHeader.CurrentPackageJob = packageJob;

			Factory.Save();

			var wrapperFull = new PackageWrapperFromPkgPackageHeader(packageJob, packageHeader, Factory);
			AssertEquals("wrapperFull.Packages.Value", 0m, wrapperFull.Packages.Value);
			AssertEquals("wrapperFull.Packages.Unit.Code", "", wrapperFull.Packages.Unit.Code);
			AssertEquals("wrapperFull.OutturnPackages.Value", 0m, wrapperFull.OutturnedPackages.Value);
			AssertEquals("wrapperFull.OutturnPackages.Unit.Code", "", wrapperFull.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperFull.PillagedPackages.Value", 0m, wrapperFull.PillagedPackages.Value);
			AssertEquals("wrapperFull.PillagedPackages.Unit.Code", "", wrapperFull.PillagedPackages.Unit.Code);
			AssertEquals("wrapperFull.DamagedPackages.Value", 0m, wrapperFull.DamagedPackages.Value);
			AssertEquals("wrapperFull.DamagedPackages.Unit.Code", "", wrapperFull.DamagedPackages.Unit.Code);
			AssertEquals("wrapperFull.FumigatedPackages.Value", 0m, wrapperFull.FumigatedPackages.Value);
			AssertEquals("wrapperFull.FumigatedPackages.Unit.Code", "", wrapperFull.FumigatedPackages.Unit.Code);
			AssertEquals("wrapperFull.NonStackablePackages.Value", 0m, wrapperFull.NonStackablePackages.Value);
			AssertEquals("wrapperFull.NonStackablePackages.Unit.Code", "", wrapperFull.NonStackablePackages.Unit.Code);
			AssertEquals("wrapperFull.TopLoadOnlyPackages.Value", 0m, wrapperFull.TopLoadOnlyPackages.Value);
			AssertEquals("wrapperFull.TopLoadOnlyPackages.Unit.Code", "", wrapperFull.TopLoadOnlyPackages.Unit.Code);
			AssertEquals("wrapperFull.HeatTreatedPackages.Value", 0m, wrapperFull.HeatTreatedPackages.Value);
			AssertEquals("wrapperFull.HeatTreatedPackages.Unit.Code", "", wrapperFull.HeatTreatedPackages.Unit.Code);
			AssertEquals("wrapperFull.ISPMPalletPackages.Value", 0m, wrapperFull.ISPMPalletPackages.Value);
			AssertEquals("wrapperFull.ISPMPalletPackages.Unit.Code", "", wrapperFull.ISPMPalletPackages.Unit.Code);
			AssertEquals("wrapperFull.CartonGroupAndSize", ZString.Empty, wrapperFull.CartonGroupAndSize);
			AssertEquals("wrapperFull.ContainerNo", "", wrapperFull.ContainerNo);
			AssertEquals("wrapperFull.ContainerJobID", "", wrapperFull.ContainerJobID);
			AssertEquals("wrapperFull.HouseBill", "", wrapperFull.HouseBill);
			AssertEquals("wrapperFull.MasterBill", "", wrapperFull.MasterBill);
			AssertEquals("wrapperFull.PickLocation", "", wrapperFull.PickLocation);
			AssertEquals("wrapperFull.PickMethod", "", wrapperFull.PickMethod);
			AssertEquals("wrapperFull.UNDGSubstance", 0, wrapperFull.UNDGSubstances.Count);
			AssertEquals("wrapperFull.Volume.Value", 0m, wrapperFull.Volume.Value);
			AssertEquals("wrapperFull.Weight.Value", 0m, wrapperFull.Weight.Value);
			AssertEquals("wrapperFull.OutturnWeight.Value", 0m, wrapperFull.OutturnedWeight.Value);
			AssertEquals("wrapperFull.OutturnWeight.Unit.Code", "", wrapperFull.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperFull.Description", "", wrapperFull.Description);
			AssertEquals("wrapperFull.MarksAndNumbers", "", wrapperFull.MarksAndNumbers);
			AssertEquals("wrapperFull.Commodity.Code", "", wrapperFull.Commodity.Code);
			AssertEquals("wrapperFull.Height", 0m, wrapperFull.Dimensions.Height);
			AssertEquals("wrapperFull.Width", 0m, wrapperFull.Dimensions.Width);
			AssertEquals("wrapperFull.Length", 0m, wrapperFull.Dimensions.Length);
			AssertEquals("wrapperFull.Dimension.Unit.Code", "", wrapperFull.Dimensions.Unit.Code);
			AssertEquals("wrapperFull.RefNumber", "123", wrapperFull.RefNumber);
			AssertEquals("wrapperFull.ExportRefNumber", ZString.Empty, wrapperFull.ExportRefNumber);
			AssertEquals("wrapperFull.ImportRefNumber", ZString.Empty, wrapperFull.ImportRefNumber);
			AssertEquals("wrapperFull.BarcodeText", "", wrapperFull.BarcodeText);
			AssertEquals("wrapperFull.Parent", "W00000002", wrapperFull.Parent.ToString());
			AssertEquals("wrapperFull.ItemNumber", (ZShort)0, wrapperFull.ItmNumber);
			AssertEquals("wrapperFull.HarmonizedCode", "", wrapperFull.HarmonizedCode);
			AssertEquals("wrapperFull.Origin", "", wrapperFull.Origin.UNLOCO);
			AssertEquals("wrapperFull.Indent", "", wrapperFull.Indent);
			AssertEquals("wrapperFull.InnersDetail", "", wrapperFull.InnersDetail);
			AssertEquals("wrapperFull.IsExclusive", false, wrapperFull.IsExclusive);
			AssertEquals("wrapperFull.IsExpiryUsed", false, wrapperFull.IsExpiryUsed);
			AssertEquals("wrapperFull.IsPackingDateUsed", false, wrapperFull.IsPackingDateUsed);
			AssertEquals("wrapperFull.IsPartAttrib1Used", false, wrapperFull.IsPartAttrib1Used);
			AssertEquals("wrapperFull.IsPartAttrib2Used", false, wrapperFull.IsPartAttrib2Used);
			AssertEquals("wrapperFull.IsPartAttrib3Used", false, wrapperFull.IsPartAttrib3Used);
			AssertEquals("wrapperFull.IsTrackedSerialUsed", false, wrapperFull.IsTrackedSerialUsed);
			AssertEquals("wrapperFull.DisplayOrder", "", wrapperFull.DisplayOrder);
			AssertEquals("wrapperFull.Products", PackProductWrapperCollection.Empty, wrapperFull.Products);
			AssertEquals("wrapperFull.RefNumber", "123", wrapperFull.RefNumber);
			AssertEquals("wrapperFull.BarcodeText", "È123(Ê", wrapperFull.PackageBarcode);
			AssertEquals("wrapperFull.PackageBarcodeWithOptimisedEncoding", "È123(Ê", wrapperFull.PackageBarcodeWithOptimisedEncoding);
			AssertEquals("wrapperFull.HasSingleProduct", false, wrapperFull.HasSingleProduct);
			AssertEquals("wrapperFull.HasPackedItem", false, wrapperFull.HasPackedItem);
			AssertNull("wrapperFull.PackedItem", wrapperFull.PackedItem);
			AssertEquals("wrapperFull.PackedItem", "2213", wrapperFull.PostcodeBarcodeNumber);
			AssertEquals("wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix", "(421) 0362213", wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix);
			AssertEquals("wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText", "ÈÆ(421)¯0362213[Ê", wrapperFull.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText);
			AssertEquals("wrapperFull.PostcodeBarcodeNumberWithPrefix", "(420) 2213", wrapperFull.PostcodeBarcodeNumberWithPrefix);
			AssertEquals("wrapperFull.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText", "ÈÆ(421)¯0362213(90)#1gÊ",
				wrapperFull.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText);
			AssertEquals("wrapperFull.PackedItem", "ÈÆ4Ã46-zÊ", wrapperFull.PostcodeBarcode);
			AssertEquals("wrapperFull.IsTopLevelNonContainerisedPackage", false, wrapperFull.IsTopLevelNonContainerisedPackage);
			AssertNull("wrapperFull.FreightPackLine", wrapperFull.FreightPackLine);
			AssertEquals("wrapperFull.NMFC", "", wrapperFull.NMFC);
		}

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (PackageWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Packages.Value", 0m, wrapperEmpty.Packages.Value);
			AssertEquals("wrapperEmpty.Packages.Unit.Code", "", wrapperEmpty.Packages.Unit.Code);
			AssertEquals("wrapperEmpty.OutturnPackages.Value", 0m, wrapperEmpty.OutturnedPackages.Value);
			AssertEquals("wrapperEmpty.OutturnPackages.Unit.Code", "", wrapperEmpty.OutturnedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.PillagedPackages.Value", 0m, wrapperEmpty.PillagedPackages.Value);
			AssertEquals("wrapperEmpty.PillagedPackages.Unit.Code", "", wrapperEmpty.PillagedPackages.Unit.Code);
			AssertEquals("wrapperEmpty.DamagedPackages.Value", 0m, wrapperEmpty.DamagedPackages.Value);
			AssertEquals("wrapperEmpty.DamagedPackages.Unit.Code", "", wrapperEmpty.DamagedPackages.Unit.Code);
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
			AssertEquals("wrapperEmpty.CartonGroupAndSize", ZString.Empty, wrapperEmpty.CartonGroupAndSize);
			AssertEquals("wrapperEmpty.ContainerNo", "", wrapperEmpty.ContainerNo);
			AssertEquals("wrapperEmpty.ContainerJobID", "", wrapperEmpty.ContainerJobID);
			AssertEquals("wrapperEmpty.HouseBill", "", wrapperEmpty.HouseBill);
			AssertEquals("wrapperEmpty.PickLocation", "", wrapperEmpty.PickLocation);
			AssertEquals("wrapperEmpty.PickMethod", "", wrapperEmpty.PickMethod);
			AssertEquals("wrapperEmpty.MasterBill", "", wrapperEmpty.MasterBill);
			AssertEquals("wrapperEmpty.UNDGSubstance", 0, wrapperEmpty.UNDGSubstances.Count);
			AssertEquals("wrapperEmpty.Volume.Value", 0m, wrapperEmpty.Volume.Value);
			AssertEquals("wrapperEmpty.Weight.Value", 0m, wrapperEmpty.Weight.Value);
			AssertEquals("wrapperEmpty.OutturnWeight.Value", 0m, wrapperEmpty.OutturnedWeight.Value);
			AssertEquals("wrapperEmpty.OutturnWeight.Unit.Code", "", wrapperEmpty.OutturnedWeight.Unit.Code);
			AssertEquals("wrapperEmpty.Description", "", wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.MarksAndNumbers", "", wrapperEmpty.MarksAndNumbers);
			AssertEquals("wrapperEmpty.Commodity.Code", "", wrapperEmpty.Commodity.Code);
			AssertEquals("wrapperEmpty.Height", 0m, wrapperEmpty.Dimensions.Height);
			AssertEquals("wrapperEmpty.Width", 0m, wrapperEmpty.Dimensions.Width);
			AssertEquals("wrapperEmpty.Length", 0m, wrapperEmpty.Dimensions.Length);
			AssertEquals("wrapperEmpty.Dimension.Unit.Code", "", wrapperEmpty.Dimensions.Unit.Code);
			AssertEquals("wrapperEmpty.RefNumber", "", wrapperEmpty.RefNumber);
			AssertEquals("wrapperEmpty.ExportRefNumber", ZString.Empty, wrapperEmpty.ExportRefNumber);
			AssertEquals("wrapperEmpty.ImportRefNumber", ZString.Empty, wrapperEmpty.ImportRefNumber);
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
			AssertEquals("wrapperEmpty.DisplayOrder", "", wrapperEmpty.DisplayOrder);
			AssertEquals("wrapperEmpty.HasMixedProducts", false, wrapperEmpty.HasSingleProduct);
			AssertEquals("wrapperEmpty.HasPackedItem", false, wrapperEmpty.HasPackedItem);
			AssertNull("wrapperEmpty.PackedItem", wrapperEmpty.PackedItem);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumber", "", wrapperEmpty.PostcodeBarcodeNumber);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumberWithPrefix", "", wrapperEmpty.PostcodeBarcodeNumberWithPrefix);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix", "", wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefix);
			AssertEquals("wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText", "", wrapperEmpty.PostcodeBarcodeNumberAndISONumericCountryCodeWithPrefixBarcodeText);
			AssertEquals("wrapperEmpty.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText", "", wrapperEmpty.PostCodeISONumericCountryCodeAndConsigneeAddressShortCodeBarcodeText);
			AssertEquals("wrapperEmpty.PostcodeBarcode", "", wrapperEmpty.PostcodeBarcode);
			AssertEquals("wrapperEmpty.IsTopLevelNonContainerisedPackage", false, wrapperEmpty.IsTopLevelNonContainerisedPackage);
			AssertEquals("wrapperEmpty.PackageTemperatures", ZString.Empty, wrapperEmpty.PackageTemperatures);
			AssertEquals("wrapperEmpty.Seals", ZString.Empty, wrapperEmpty.Seals);
			AssertEquals("wrapperEmpty.NMFC", ZString.Empty, wrapperEmpty.NMFC);
			AssertEquals("wrapperEmpty.StarTrack_QRCodeText", ZString.Empty.PadRight(334), wrapperEmpty.StarTrack_QRCodeText);
		}

		#endregion

		#region TestStarTrack_QRCodeText

		public new void TestStarTrack_QRCodeText()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var consignee = helper.CreateClient("CNE", "CNE SYDNEY");
			consignee.MainAddress.FillWithValidTestData();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consignee.MainAddress.OA_PostCode = "2000";                   // Consignee Postcode
			consignee.MainAddress.OA_City = "Sydney";                   // Consignee City
			order.ConsigneeDocAddress.E2_OA_Address = consignee.MainAddress.PK;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX", "123");
			var packageHeader = package.GetPackageHeader();
			packageHeader.CurrentPackageJob = packageJob;

			Factory.Save();

			var wrapperFull = new PackageWrapperFromPkgPackageHeader(packageJob, packageHeader, Factory);
			var expectedQRCodeText = $"SYDNEY                        2000            123                                    1   0    0    {ZDateTime.Today.ToString("yyyyMMdd")}CNE SYDNEY                                                                      BOX    #1                                                                                            NN                                            ";
			AssertEquals(expectedQRCodeText, wrapperFull.StarTrack_QRCodeText);
		}

		public void TestStarTrack_QRCodeText_UsingProtectedConstructor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 20m);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, "O1", data.Part1, 10m);
			var consignee = helper.CreateClient("CNE", "CNE SYDNEY");
			consignee.MainAddress.FillWithValidTestData();
			consignee.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			consignee.MainAddress.OA_PostCode = "2000";                   // Consignee Postcode
			consignee.MainAddress.OA_City = "Sydney";                   // Consignee City
			order.ConsigneeDocAddress.E2_OA_Address = consignee.MainAddress.PK;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX", "123");

			Factory.Save();

			var wrapperFull = new PackageWrapperFromPkgPackageHeaderForTest(package, Factory);
			var expectedQRCodeText = $"SYDNEY                        2000            123                                    1   0    0    {ZDateTime.Today.ToString("yyyyMMdd")}CNE SYDNEY                                                                      BOX    #1                                                                                            NN                                            ";
			AssertEquals(expectedQRCodeText, wrapperFull.StarTrack_QRCodeText);
		}

		public void TestStarTrack_QRCodeText_Length()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);

			var receive = helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", helper.Notify);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 5m);

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var package = packageJob.Packages.AddNew("BOX", "123");
			var packageHeader = package.GetPackageHeader();
			packageHeader.CurrentPackageJob = packageJob;

			Factory.Save();

			var wrapperFull = new PackageWrapperFromPkgPackageHeader(packageJob, packageHeader, Factory);
			AssertEquals(334, wrapperFull.StarTrack_QRCodeText.Length);
		}

		#endregion

		#region TestOutterSequence

		public void TestOutterSequence_FromPackage()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var job = PkgPackageJob.LoadOrCreatePackageJob(rcn);
			job.Packages.AddNew("BOX", "P1");
			var package2 = job.Packages.AddNew("BOX", "P2");
			Factory.Save();

			var packageHeader = package2.GetPackageHeader();
			packageHeader.CurrentPackageJob = job;
			var wrapper = new PackageWrapperFromPkgPackageHeader(job, packageHeader, Factory);
			AssertEquals((ZShort)2, wrapper.OutterPackageSequence);
		}

		public void TestOutterSequence_FromPivot()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var job = PkgPackageJob.LoadOrCreatePackageJob(rcn);
			var packageHeader1 = job.LoosePackageIDs.AddNew();
			packageHeader1.KPH_PackageID = "P1";
			var packageHeader2 = job.LoosePackageIDs.AddNew();
			packageHeader2.KPH_PackageID = "P2";
			Factory.Save();

			packageHeader2.CurrentPackageJob = job;
			var wrapper = new PackageWrapperFromPkgPackageHeader(job, packageHeader2, Factory);
			AssertEquals((ZShort)2, wrapper.OutterPackageSequence);
		}

		#endregion

		#region TestOutterCount

		public void TestOutterCount()
		{
			var helper = new WhsTransitTestHelper(Factory);
			var warehouse = helper.CreateTRWWarehouse();
			var rcn = helper.CreateReceiveConsignment("RCN1", warehouse.PK);
			var job = PkgPackageJob.LoadOrCreatePackageJob(rcn);
			var package = job.Packages.AddNew("BOX", "P1");
			job.Packages.AddNew("BOX", "P2");
			job.Packages.AddNew("BOX", "P3");
			Factory.Save();

			var packageHeader = package.GetPackageHeader();
			packageHeader.CurrentPackageJob = job;
			var wrapper = new PackageWrapperFromPkgPackageHeader(job, packageHeader, Factory);
			AssertEquals((ZShort)3, wrapper.OutterPackagesCount);

			var loosePackageHeader = job.LoosePackageIDs.AddNew();
			loosePackageHeader.KPH_PackageID = "LP1";
			Factory.Save();

			var wrapper2 = new PackageWrapperFromPkgPackageHeader(job, packageHeader, Factory);
			AssertEquals((ZShort)4, wrapper2.OutterPackagesCount);

			var packageWithoutHeader = job.Packages.AddNew();
			Factory.Save();

			var wrapper3 = new PackageWrapperFromPkgPackageHeader(job, packageHeader, Factory);
			AssertEquals((ZShort)4, wrapper3.OutterPackagesCount);
		}

		#endregion

		#region Implementation

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return new PackageWrapperFromPkgPackageHeader(null, null, Factory);
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
Packages : 
PackageState :  is null
PackedItem :  is null
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
			var packageJob = Factory.New<PkgPackageJob>();
			var package = packageJob.Packages.AddNew("BOX", "ABC");
			var header = package.GetPackageHeader();
			header.CurrentPackageJob = package.PackageJob;
			return new PackageWrapperFromPkgPackageHeader(null, header, Factory);
		}

		#endregion

		class PackageWrapperFromPkgPackageHeaderForTest : PackageWrapperFromPkgPackageHeader
		{
			public PackageWrapperFromPkgPackageHeaderForTest(PkgPackage package, BusinessObjectFactory factory) : base(package, factory)
			{ }
		}
	}
}
