using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	abstract class PackedItemWrapperTest : GenericWrapperTest
	{
		#region TestConstructor

		public void TestConstructor()
		{
			using (SetupDummyTypes())
			{
				var data = new TestDataForPacking(Factory);
				data.CreatePackingData();

				var package1 = data.PackageJob.Packages.AddNew();
				var package2 = data.PackageJob.Packages.AddNew();
				var dummyItem1 = data.DummyPackableItemOnLine1;
				var dummyItem2 = data.DummyLine1.AddNewPackableItem();

				var divot1 = package1.PackedItemDivots.AddNew();
				divot1.KI_ParentID = dummyItem1.PK;
				divot1.KI_ParentTableCode = dummyItem1.TablePrefix;
				divot1.KI_PackedQty = 1m;

				var divot2 = package2.PackedItemDivots.AddNew();
				divot2.KI_ParentID = dummyItem2.PK;
				divot2.KI_ParentTableCode = dummyItem2.TablePrefix;
				divot2.KI_PackedQty = 2m;

				var packedItem1 = PkgPackageItemDivotsWrapper.New(divot1, data.DummyLine1);
				var packedItem2 = PkgPackageItemDivotsWrapper.New(divot2, data.DummyLine1);
				AssertExceptionThrown(typeof(InvalidOperationException), "Should only pass in Packed Items for the same Package and same PackableItemParent.",
					() => PackedItemWrapper.New(data.DummyLine1, Factory, packedItem1, packedItem2));

				var dummyItem3 = data.DummyLine2.PackableItems.Single();
				var divot3 = package1.PackedItemDivots.AddNew();
				divot3.KI_ParentID = dummyItem2.PK;
				divot3.KI_ParentTableCode = dummyItem2.TablePrefix;
				divot3.KI_PackedQty = 4m;

				var packedItem3 = PkgPackageItemDivotsWrapper.New(divot3, data.DummyLine2);
				AssertExceptionThrown(typeof(InvalidOperationException), "Should only pass in Packed Items for the same Package and same PackableItemParent.",
					() => PackedItemWrapper.New(data.DummyLine1, Factory, packedItem1, packedItem3));
			}
		}

		#endregion

		#region TestWrapperMappingsEmpty

		public override void TestWrapperMappingsEmpty()
		{
			var wrapperEmpty = (PackedItemWrapper)GetNewDocumentWrapper();
			AssertEquals("wrapperEmpty.Code", "", wrapperEmpty.Code);
			AssertEquals("wrapperEmpty.LocalCode", "", wrapperEmpty.LocalCode);
			AssertEquals("wrapperEmpty.LocalCodeWithFallback", "", wrapperEmpty.LocalCodeWithFallback);
			AssertEquals("wrapperEmpty.Description", "", wrapperEmpty.Description);
			AssertEquals("wrapperEmpty.LocalDescription", "", wrapperEmpty.LocalDescription);
			AssertEquals("wrapperEmpty.LocalDescriptionWithFallback", "", wrapperEmpty.LocalDescriptionWithFallback);
			AssertEquals("wrapperEmpty.DescriptionSupplement", "", wrapperEmpty.DescriptionSupplement);
			AssertEquals("wrapperEmpty.PackedQty", 0m, wrapperEmpty.PackedQty.Value);
			AssertTotalQtyAndExpiryLabel(wrapperEmpty);
			AssertNull("wrapperEmpty.Product", wrapperEmpty.Product.WrappedObject);
			AssertEquals("wrapperEmpty.IsExpiryUsed", false, wrapperEmpty.IsExpiryUsed);
			AssertEquals("wrapperEmpty.IsPackingDateUsed", false, wrapperEmpty.IsPackingDateUsed);
			AssertEquals("wrapperEmpty.IsPartAttrib1Used", false, wrapperEmpty.IsPartAttrib1Used);
			AssertEquals("wrapperEmpty.IsPartAttrib2Used", false, wrapperEmpty.IsPartAttrib2Used);
			AssertEquals("wrapperEmpty.IsPartAttrib3Used", false, wrapperEmpty.IsPartAttrib3Used);
			AssertEquals("wrapperEmpty.IsTrackedSerialUsed", false, wrapperEmpty.IsTrackedSerialUsed);
			AssertEquals("wrapperEmpty.PartAttrib1Name", "Part Attrib. 1", wrapperEmpty.PartAttrib1Name);
			AssertEquals("wrapperEmpty.PartAttrib2Name", "Part Attrib. 2", wrapperEmpty.PartAttrib2Name);
			AssertEquals("wrapperEmpty.PartAttrib3Name", "Part Attrib. 3", wrapperEmpty.PartAttrib3Name);
			AssertEquals("wrapperEmpty.TrackedSerialNumberName", "Tracked Serial Number", wrapperEmpty.TrackedSerialNumberName);
			AssertEquals("wrapperEmpty.PartAttribute1", "", wrapperEmpty.PartAttribute1);
			AssertEquals("wrapperEmpty.PartAttribute2", "", wrapperEmpty.PartAttribute2);
			AssertEquals("wrapperEmpty.PartAttribute3", "", wrapperEmpty.PartAttribute3);
			AssertEquals("wrapperEmpty.TrackedSerialNumber", "", wrapperEmpty.TrackedSerialNumber);
			AssertEquals("wrapperEmpty.Expiry", "", wrapperEmpty.Expiry);
			AssertEquals("wrapperEmpty.PackingDate", "", wrapperEmpty.PackingDate);
			AssertEquals("wrapperEmpty.BatchLabel", "BATCH/LOT", wrapperEmpty.BatchLabel);
			AssertEquals("wrapperEmpty.Batch", "", wrapperEmpty.Batch);
			AssertEquals("wrapperEmpty.ProductBarcode", "", wrapperEmpty.ProductBarcode);
			AssertEquals("wrapperEmpty.ProductBarcodeWithPrefixes", "", wrapperEmpty.ProductBarcodeWithPrefixes);
			AssertEquals("wrapperEmpty.ProductCodeStockUnitBarcodeNumber", "", wrapperEmpty.ProductCodeStockUnitBarcodeNumber);
			AssertEquals("wrapperEmpty.CustomFields.Count", 0, wrapperEmpty.CustomFields.Count);
		}

		protected virtual void AssertTotalQtyAndExpiryLabel(PackedItemWrapper wrapperEmpty)
		{
			AssertEquals("wrapperEmpty.TotalQtyUQ", "", wrapperEmpty.TotalQty.Unit.Code);
			AssertEquals("wrapperEmpty.TotalQtyUQDescription", "", wrapperEmpty.TotalQty.Unit.Description);
			AssertEquals("wrapperEmpty.TotalQtyUQWithDescription", "", wrapperEmpty.TotalQty.Unit.CodeAndDescription);
			AssertEquals("wrapperEmpty.ExpiryLabel", "EXPIRY DATE", wrapperEmpty.ExpiryLabel);
		}

		#endregion

		#region TestWrapperMappingFull

		[TestDate(2011, 1, 1)]
		public virtual void TestWrapperMappingFull()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.One, true, "Color");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Two, true, "Serial#");
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Three, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.Serial, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.ExpiryDate, true);
			helper.SetClientAttributeType(data.Org1, AttributeNumber.PackingDate, true);
			helper.SetProductAllAttributeUse(data.Org1, data.Part1, true);

			var receiveDocket = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
			var receiveLine = helper.CreateWhsReceiveInventoryLine(receiveDocket, data.Part1, 1m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "BN:00234", "");
			receiveLine.InDocketLine.WE_SerialNumber = "SN:Here Now";
			receiveDocket.AllocateLocationsWithMock();
			receiveDocket.FinaliseDocketWithoutUserConfirmation();
			AssertEquals("Precondition - ensure Receive is finalised.", true, receiveDocket.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 1m, new ZDate(2011, 9, 3), new ZDate(2011, 7, 13), "RED", "SN:0001", "BN:00234", "", "");
			orderLine.WE_SerialNumber = "SN:Here Now";
			helper.CreatePickNew(order);

			IPackableItem pickLine = orderLine.PickLines[0];
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.KI_ParentID = pickLine.PK;
			packageItemDivot.KI_ParentTableCode = pickLine.TablePrefix;
			packageItemDivot.KI_PackedQty = 3m;

			var packedItem = PkgPackageItemDivotsWrapper.New(packageItemDivot, orderLine.ReleaseLines[0]);
			var wrapperFull = PackedItemWrapper.New(orderLine.ReleaseLines[0], Factory, packedItem);
			AssertEquals(nameof(wrapperFull.Code), "P1", wrapperFull.Code);
			AssertEquals(nameof(wrapperFull.LocalCode), "", wrapperFull.LocalCode);
			AssertEquals(nameof(wrapperFull.Description), "P1", wrapperFull.Description);
			AssertEquals(nameof(wrapperFull.LocalDescription), "", wrapperFull.LocalDescription);
			AssertEquals(nameof(wrapperFull.DescriptionSupplement), "Color: RED, Serial#: SN:0001, Part Attrib. 3: BN:00234, Serial Number: SN:Here Now, EXP: 03-Sep-11, PKD: 13-Jul-11", wrapperFull.DescriptionSupplement);
			AssertEquals(nameof(wrapperFull.PackedQty.Value), 3m, wrapperFull.PackedQty.Value);
			AssertEquals(nameof(wrapperFull.PackedQty.ValueAndUnitCode), "3.00 UNT", wrapperFull.PackedQty.ValueAndUnitCode);
			AssertEquals(nameof(wrapperFull.TotalQty.Unit.Code), "UNT", wrapperFull.TotalQty.Unit.Code);
			AssertEquals(nameof(wrapperFull.TotalQty.Unit.Description), "Unit", wrapperFull.TotalQty.Unit.Description);
			AssertEquals(nameof(wrapperFull.TotalQty.Unit.CodeAndDescription), "UNT - Unit", wrapperFull.TotalQty.Unit.CodeAndDescription);
			AssertEquals(nameof(wrapperFull.Product), data.Part1, wrapperFull.Product.WrappedObject);
			AssertEquals(nameof(wrapperFull.IsExpiryUsed), true, wrapperFull.IsExpiryUsed);
			AssertEquals(nameof(wrapperFull.IsPackingDateUsed), true, wrapperFull.IsPackingDateUsed);
			AssertEquals(nameof(wrapperFull.IsPartAttrib1Used), true, wrapperFull.IsPartAttrib1Used);
			AssertEquals(nameof(wrapperFull.IsPartAttrib2Used), true, wrapperFull.IsPartAttrib2Used);
			AssertEquals(nameof(wrapperFull.IsPartAttrib3Used), true, wrapperFull.IsPartAttrib3Used);
			AssertEquals(nameof(wrapperFull.IsTrackedSerialUsed), true, wrapperFull.IsTrackedSerialUsed);
			AssertEquals(nameof(wrapperFull.PartAttrib1Name), "Color", wrapperFull.PartAttrib1Name);
			AssertEquals(nameof(wrapperFull.PartAttrib2Name), "Serial#", wrapperFull.PartAttrib2Name);
			AssertEquals(nameof(wrapperFull.PartAttrib3Name), "Part Attrib. 3", wrapperFull.PartAttrib3Name);
			AssertEquals(nameof(wrapperFull.TrackedSerialNumberName), "Tracked Serial Number", wrapperFull.TrackedSerialNumberName);
			AssertEquals(nameof(wrapperFull.PartAttribute1), "RED", wrapperFull.PartAttribute1);
			AssertEquals(nameof(wrapperFull.PartAttribute2), "SN:0001", wrapperFull.PartAttribute2);
			AssertEquals(nameof(wrapperFull.PartAttribute3), "BN:00234", wrapperFull.PartAttribute3);
			AssertEquals(nameof(wrapperFull.TrackedSerialNumber), "SN:Here Now", wrapperFull.TrackedSerialNumber);
			AssertEquals(nameof(wrapperFull.ExpiryLabel), "EXPIRY DATE (ddmmyyyy)", wrapperFull.ExpiryLabel);
			AssertEquals(nameof(wrapperFull.Expiry), "03.09.2011", wrapperFull.Expiry);
			AssertEquals(nameof(wrapperFull.PackingDate), "13.07.2011", wrapperFull.PackingDate);
			AssertEquals(nameof(wrapperFull.BatchLabel), "BATCH/LOT", wrapperFull.BatchLabel);
			AssertEquals(nameof(wrapperFull.Batch), "", wrapperFull.Batch);
			AssertEquals(nameof(wrapperFull.ProductBarcode), "ÈÆ02PÃ-iÆ1+)#!Ê", wrapperFull.ProductBarcode);
			AssertEquals(nameof(wrapperFull.ProductBarcodeWithPrefixes), "(02)P1(37)3(17)110903", wrapperFull.ProductBarcodeWithPrefixes);
			AssertEquals(nameof(wrapperFull.ProductCodeStockUnitBarcodeNumber), "P1", wrapperFull.ProductCodeStockUnitBarcodeNumber);
		}

		#endregion

		#region TestPackageInfo

		public void TestPackageInfo()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			helper.CreateWhsReceiveInventoryLine(receive, data.Part2, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine1 = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			var orderLine2 = helper.CreateWhsOrderLine(order, data.Part2, 10m);
			Factory.Save();

			var pick = helper.CreatePickNew(order);

			// So we have
			// PackageJob
			//     1x Container
			//         2x Part1 (PackableItem)
			//         4x Crate
			//             3x Part1 (PackableItem)
			//             7x Part2 (PackableItem)
			//     1x Pallet
			//         1x Part2 (PackableItem)
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);

			// Level 1
			var container = PackingHelper.CreatePackage(packageJob, "PACK0001", 1, Constants.PkgUnit.Container);
			var pallet = PackingHelper.CreatePackage(packageJob, "PACK0002", 1, Constants.PkgUnit.Pallet);

			// Level 2
			var packedItem1 = container.Pack_ForTesting(orderLine1.ReleaseLines[0], 2);
			var crate = PackingHelper.CreatePackage(container, 4, Constants.PkgUnit.Crate);
			var packedItem2 = pallet.Pack_ForTesting(orderLine2.ReleaseLines[0], 1);

			// Level 3
			var packedItem3 = crate.Pack_ForTesting(orderLine1.ReleaseLines[0], 3);
			var packedItem4 = crate.Pack_ForTesting(orderLine2.ReleaseLines[0], 7);

			var packedItem1Wrapper = PackedItemWrapper.New(orderLine1.ReleaseLines[0], Factory, packedItem1);
			AssertEquals("1x CNT - PACK0001", packedItem1Wrapper.PackageInfo);

			var packedItem2Wrapper = PackedItemWrapper.New(orderLine2.ReleaseLines[0], Factory, packedItem2);
			AssertEquals("1x PLT - PACK0002", packedItem2Wrapper.PackageInfo);

			var packedItem3Wrapper = PackedItemWrapper.New(orderLine1.ReleaseLines[0], Factory, packedItem3);
			AssertEquals(
@"4x CRT
      1x CNT - PACK0001", packedItem3Wrapper.PackageInfo);

			var packedItem4Wrapper = PackedItemWrapper.New(orderLine2.ReleaseLines[0], Factory, packedItem4);
			AssertEquals(
@"4x CRT
      1x CNT - PACK0001", packedItem4Wrapper.PackageInfo);
		}

		#endregion

		#region TestPackedQty

		public void TestPackedQty()
		{
			using (SetupDummyTypes())
			{
				var data = new TestDataForPacking(Factory);
				data.CreatePackingData();

				var package = data.PackageJob.Packages.AddNew();
				var dummyItem1 = data.DummyPackableItemOnLine1;
				var dummyItem2 = data.DummyLine1.AddNewPackableItem();

				var divot1 = package.PackedItemDivots.AddNew();
				divot1.KI_ParentID = dummyItem1.PK;
				divot1.KI_ParentTableCode = dummyItem1.TablePrefix;
				divot1.KI_PackedQty = 1m;

				var divot2 = package.PackedItemDivots.AddNew();
				divot2.KI_ParentID = dummyItem2.PK;
				divot2.KI_ParentTableCode = dummyItem2.TablePrefix;
				divot2.KI_PackedQty = 2m;

				var packedItem1 = PkgPackageItemDivotsWrapper.New(divot1, data.DummyLine1);
				var packedItem2 = PkgPackageItemDivotsWrapper.New(divot2, data.DummyLine1);
				var wrapper = PackedItemWrapper.New(data.DummyLine1, Factory, packedItem1, packedItem2);
				AssertEquals(nameof(wrapper.PackedQty.Value), 3m, wrapper.PackedQty.Value);
			}
		}

		public void TestPackedQty_DoesNotRoundDivotPackedQtyValue()
		{
			using (SetupDummyTypes())
			{
				var data = new TestDataForPacking(Factory);
				data.CreatePackingData();

				var package = data.PackageJob.Packages.AddNew();
				var dummyItem1 = data.DummyPackableItemOnLine1;
				var dummyItem2 = data.DummyLine1.AddNewPackableItem();
				var dummyItem3 = data.DummyLine1.AddNewPackableItem();

				var divot1 = package.PackedItemDivots.AddNew();
				divot1.KI_ParentID = dummyItem1.PK;
				divot1.KI_ParentTableCode = dummyItem1.TablePrefix;
				divot1.KI_PackedQty = 1.4m;

				var divot2 = package.PackedItemDivots.AddNew();
				divot2.KI_ParentID = dummyItem2.PK;
				divot2.KI_ParentTableCode = dummyItem2.TablePrefix;
				divot2.KI_PackedQty = 2.4m;

				var divot3 = package.PackedItemDivots.AddNew();
				divot3.KI_ParentID = dummyItem3.PK;
				divot3.KI_ParentTableCode = dummyItem3.TablePrefix;
				divot3.KI_PackedQty = 3.4m;

				var packedItem1 = PkgPackageItemDivotsWrapper.New(divot1, data.DummyLine1);
				var packedItem2 = PkgPackageItemDivotsWrapper.New(divot2, data.DummyLine1);
				var packedItem3 = PkgPackageItemDivotsWrapper.New(divot3, data.DummyLine1);
				var wrapper = PackedItemWrapper.New(data.DummyLine1, Factory, packedItem1, packedItem2, packedItem3);
				AssertEquals(nameof(wrapper.PackedQty.Value), 7.2m, wrapper.PackedQty.Value);
			}
		}

		public void TestPackedQty_RoundsPackedQtyToCorrectDecimalPlaces()
		{
			using (SetupDummyTypes())
			{
				var data = new TestDataForPacking(Factory);
				data.CreatePackingData();

				var package = data.PackageJob.Packages.AddNew();
				var dummyItem1 = data.DummyPackableItemOnLine1;
				var dummyItem2 = data.DummyLine1.AddNewPackableItem();

				var divot1 = package.PackedItemDivots.AddNew();
				divot1.KI_ParentID = dummyItem1.PK;
				divot1.KI_ParentTableCode = dummyItem1.TablePrefix;
				divot1.KI_PackedQty = 1.554m;

				var divot2 = package.PackedItemDivots.AddNew();
				divot2.KI_ParentID = dummyItem2.PK;
				divot2.KI_ParentTableCode = dummyItem2.TablePrefix;
				divot2.KI_PackedQty = 2.54m;

				var packedItem1 = PkgPackageItemDivotsWrapper.New(divot1, data.DummyLine1);
				var packedItem2 = PkgPackageItemDivotsWrapper.New(divot2, data.DummyLine1);
				var wrapper = PackedItemWrapper.New(data.DummyLine1, Factory, packedItem1, packedItem2);
				AssertEquals("PackedQty is rounded off to 2 decimal places.", 4.09m, wrapper.PackedQty.Value);
			}
		}

		#endregion

		#region TestGetLinePrice

		public void TestGetLinePrice()
		{
			var packedItem1Wrapper = PackedItemWrapper.New(null, Factory);
			AssertEquals("Line Price is zero for empty wrapper.", 0m, packedItem1Wrapper.LinePrice);
		}

		public void TestGetLinePrice_WrapperWithPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			helper.CreatePickNew(order);

			IPackableItem pickLine = orderLine.PickLines[0];
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.KI_ParentID = pickLine.PK;
			packageItemDivot.KI_ParentTableCode = pickLine.TablePrefix;
			packageItemDivot.KI_PackedQty = 3m;

			var packedItem = PkgPackageItemDivotsWrapper.New(packageItemDivot, orderLine.ReleaseLines[0]);
			var packedItemWrapper = PackedItemWrapper.New(orderLine.ReleaseLines[0], Factory, packedItem);
			AssertEquals("Line Price is zero for wrapper with package.", 0m, packedItemWrapper.LinePrice);
		}

		#endregion

		#region TestGetCommonCurrency

		public void TestGetCommonCurrency()
		{
			var packedItem1Wrapper = PackedItemWrapper.New(null, Factory);
			AssertEquals("Common Currency is empty for empty wrapper.", string.Empty, packedItem1Wrapper.CommonCurrency.Code);
		}

		public void TestGetCommonCurrency_WrapperWithPackage()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", new TestNotificationBuffer());
			helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			receive.FinaliseDocket();
			AssertEquals("Precondition - ensure receive is finalised", true, receive.IsFinalised);
			Factory.Save();

			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);
			var orderLine = helper.CreateWhsOrderLine(order, data.Part1, 10m);
			orderLine.WE_RX_NKUnitPriceCurrency = "AUD";
			helper.CreatePickNew(order);

			IPackableItem pickLine = orderLine.PickLines[0];
			var packageItemDivot = Factory.New<PkgPackageItemDivot>();
			packageItemDivot.KI_ParentID = pickLine.PK;
			packageItemDivot.KI_ParentTableCode = pickLine.TablePrefix;
			packageItemDivot.KI_PackedQty = 3m;

			var packedItem = PkgPackageItemDivotsWrapper.New(packageItemDivot, orderLine.ReleaseLines[0]);
			var packedItemWrapper = PackedItemWrapper.New(orderLine.ReleaseLines[0], Factory, packedItem);
			AssertEquals("Common Currency is empty for non_implemented wrapper.", 0m, packedItemWrapper.LinePrice);
		}

		#endregion

		#region Implementations

		IDisposable SetupDummyTypes()
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithPacking);
			DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyPackableItemParent);

			return new DisposableAction(() =>
			{
				DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = null;
				DummyDependantBusinessObject.TypeDecider.TypeForLoadOverride = null;
			});
		}

		protected override ZString ExpectedDefaultFormatting
		{
			get
			{
				return @"
CommonCurrency : 
PackedQty : 
Product : (No Default Field Value Available on Product)
Registry : (No Default Field Value Available on Registry)
TotalQty :
";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return PackedItemWrapper.New(null, Factory);
		}

		protected override string ExpectedFieldMap
		{
			get
			{
				return string.Format(@"
{0}
======================================================================
Name                                    Type
----------------------------------------------------------------------
CommonCurrency                          Currency
Product                                 Generic
PackedQty                               ValueAndUnit
TotalQty                                ValueAndUnit
Batch                                   String
BatchLabel                              String
Code                                    String
Description                             String
DescriptionSupplement                   String
Expiry                                  String
ExpiryLabel                             String
IsExpiryUsed                            Bool
IsPackingDateUsed                       Bool
IsPartAttrib1Used                       Bool
IsPartAttrib2Used                       Bool
IsPartAttrib3Used                       Bool
IsTrackedSerialUsed                     Bool
LinePrice                               Decimal
LocalCode                               String
LocalCodeWithFallback                   String
LocalDescription                        String
LocalDescriptionWithFallback            String
PackageInfo                             String
PackingDate                             String
PartAttrib1Name                         String
PartAttrib2Name                         String
PartAttrib3Name                         String
PartAttribute1                          String
PartAttribute2                          String
PartAttribute3                          String
ProductBarcode                          String
ProductBarcodeWithPrefixes              String
ProductCodeStockUnitBarcodeNumber       String
TrackedSerialNumber                     String
TrackedSerialNumberName                 String

CustomFields                            CustomField Collection
", ExpectedWrapperNameInFieldMap);
			}
		}

		protected virtual string ExpectedWrapperNameInFieldMap => "PackedItemEmpty";

		protected override DocBaseWrapper GetNewDocumentWrapper()
		{
			return PackedItemWrapper.New(null, Factory);
		}

		#region PackingHelper

		protected PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#endregion
	}
}
