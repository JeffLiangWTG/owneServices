using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.TransportCommon.Shared;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromPkgPackageJob))]
	sealed class FreightWrapperFromPkgPackageJobTest : FreightWrapperTest
	{
		#region TestPackages

		public void TestPackages()
		{
			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "456", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package5 = Helper.CreatePackage("KEG", "345", packageJob.Packages);
			packageJob.Selected.UpdateSelectedPackages(new[] { package1, package4 });

			var wrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			AssertEquals("wrapper.Packages", 5, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages",
				new PkgPackage[] { package1, package2, package3, package4, package5 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		public void TestSetPackageOverride()
		{
			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "456", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "789", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var package5 = Helper.CreatePackage("KEG", "345", packageJob.Packages);
			packageJob.Selected.UpdateSelectedPackages(new[] { package1, package4 });

			var wrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			var iPackageOverrider = (IPackageOverrider)wrapper;
			iPackageOverrider.SetPackageOverride(package3);
			AssertEquals("wrapper.Packages", 1, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages",
				new PkgPackage[] { package3 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		public void TestPackagesAreSorted()
		{
			var package1 = Helper.CreatePackage("KEG", "KEG - 4", packageJob.Packages);
			var package2 = Helper.CreatePackage("KEG", "KEG - 2", package1.Packages);
			var package3 = Helper.CreatePackage("KEG", "KEG - 1", package2.Packages);
			var package4 = Helper.CreatePackage("KEG", "KEG - 3", packageJob.Packages);
			packageJob.Selected.UpdateSelectedPackages(new[] { package1, package4 });

			CombineAssertions("Packages should be sorted", delegate
			{
				var packageJobWrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
				var packages = packageJobWrapper.Packages;
				AssertEquals("packages[0].RefNumber", "KEG - 1", packages[0].RefNumber);
				AssertEquals("packages[1].RefNumber", "KEG - 2", packages[1].RefNumber);
				AssertEquals("packages[2].RefNumber", "KEG - 3", packages[2].RefNumber);
				AssertEquals("packages[3].RefNumber", "KEG - 4", packages[3].RefNumber);
			});
		}

		#endregion

		#region  TestSetPackageCollectionOverride

		public void TestSetPackageCollectionOverride_PkgPackage()
		{
			var package1 = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			packageJob.Selected.UpdateSelectedPackages(new[] { package1 });

			var wrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			var iPackageOverrider = (IPackageOverrider)wrapper;
			AssertExceptionThrown<ArgumentException>("Should not support passed package", "The parameter packages is not supported, try call SetPackageOverride method", () => iPackageOverrider.SetPackageCollectionOverride(new[] { package1 }));
		}

		public void TestSetPackageCollectionOverride_PkgPackageHeader()
		{
			var loosePackageHeader1 = packageJob.LoosePackageIDs.AddNew();
			loosePackageHeader1.KPH_PackageID = "XXX";
			var loosePackageHeader2 = packageJob.LoosePackageIDs.AddNew();
			loosePackageHeader2.KPH_PackageID = "YYY";

			loosePackageHeader1.CurrentPackageJob = packageJob;
			loosePackageHeader2.CurrentPackageJob = packageJob;

			var wrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			var iPackageOverrider = (IPackageOverrider)wrapper;
			iPackageOverrider.SetPackageCollectionOverride(packageHeaders: new[] { loosePackageHeader1, loosePackageHeader2 });
			AssertEquals("wrapper.Packages", 1, wrapper.Packages.Count);
			AssertContainsExactElementsInAnyOrder("wrapper.Packages", new[] { loosePackageHeader1 }, wrapper.Packages.Cast<PackageWrapper>().Select(item => item.WrappedObject));
		}

		#endregion

		#region TestShipmentOuterPacksQty

		public void TestShipmentOuterPacksQty()
		{
			var outerPack = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Box, "B1");
			var innerPack = PackingHelper.CreatePackage(outerPack, 1, Constants.PkgUnit.Box, "B2");
			var container = PackingHelper.CreatePackage(packageJob, 1, Constants.PkgUnit.Container);
			var outerInContainerPack = PackingHelper.CreatePackage(container, 10, Constants.PkgUnit.Box);

			var wrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			AssertEquals("11 BOX", wrapper.ShipmentOuterPacksQty.ValueAndUnitCode);

			var newOuter = PackingHelper.CreatePackage(container, 3, Constants.PkgUnit.Bottle);
			wrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			AssertEquals("14 PKG", wrapper.ShipmentOuterPacksQty.ValueAndUnitCode);
		}

		#endregion

		#region TestWarehousePackageJob

		public void TestWarehousePackageJob()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_TransportReference = "Transport Reference1";
			order.WD_RequiredDate = ZDateTimeOffset.Now.AddDays(2);
			var goodsHandlingInstructionsText = "This text need to print on label";
			order.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, goodsHandlingInstructionsText);

			var warehouseClient = Factory.NewWithValidTestData<OrgHeader>();
			warehouseClient.OH_FullName = "Testing Organisation";
			warehouseClient.MainAddress.OA_City = "Sydney";
			warehouseClient.MainAddress.OA_PostCode = "2000";
			order.WD_OH_Client = warehouseClient.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Testing Consignee";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			var pickUpAndDeliveryAddress = consignee.Addresses.AddNew();
			pickUpAndDeliveryAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			pickUpAndDeliveryAddress.OA_Address1 = "Address1";
			pickUpAndDeliveryAddress.OA_Address2 = "Address2";
			pickUpAndDeliveryAddress.OA_RN_NKCountryCode = "AU";
			order.ConsigneeDocAddress.E2_OA_Address = pickUpAndDeliveryAddress.PK;

			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(order);
			var packageJobWrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			((IDocTypeCode)packageJobWrapper).DocTypeCode = "PPL";

			AssertEquals("#1 SYDNEY 2000", packageJobWrapper.PickupAddress.AddressAsASingleLine);
			AssertEquals("ADDRESS1 ADDRESS2 AUSTRALIA", packageJobWrapper.DeliveryAddress.AddressAsASingleLine);
			AssertEquals("Transport reference should be WD_TransportReference from WhsOrder.", order.WD_TransportReference, packageJobWrapper.TransportReference);
			AssertEquals("Delivery required date should be WD_RequiredDate from WhsOrder.", order.WD_RequiredDate.ToZDateTime(), packageJobWrapper.DeliveryRequiredBy);
			AssertEquals("Goods Handling Instructions should have value from WhsOrder.", goodsHandlingInstructionsText, packageJobWrapper.GoodsHandlingInstructions);
		}

		#endregion

		#region TestPropertiesProxiedFromConsolidatedBooking

		public void TestPropertiesProxiedFromConsolidatedBooking()
		{
			var now = ZDateTime.Now;

			var orgProxy = GlbCompany.CurrentCompany.OrgProxy;
			var orgPickup = TransportBookingHelper.CreateOrganisation("pick-up");
			var orgDelivery = TransportBookingHelper.CreateOrganisation("Delivery");
			var orgDeliveryAgent = TransportBookingHelper.CreateOrganisation("DevAgent");

			var shipment = Factory.New<ForwardingShipment>();
			var shipmentBookingConsolidation = TransportBookingHelper.CreateConsolidation(shipment);

			var booking = TransportBookingHelper.CreateBooking(shipmentBookingConsolidation, "ABC", "DESC123", Constants.CartageDirection.Destination, "TRANS123");

			var pickUpInstruction = TransportBookingHelper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, LocalCartageJobOrgTypeList.Codes.CNR, orgPickup.MainAddress);
			var deliveryInstruction = TransportBookingHelper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, LocalCartageJobOrgTypeList.Codes.CNE, orgDelivery.MainAddress);

			var deliveryInstructionConfirmation = deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			deliveryInstructionConfirmation.KK_Estimated = now.AddDays(1);
			deliveryInstructionConfirmation.KK_RequiredTo = now.AddDays(1);

			packageJob.KJ_ParentID = shipmentBookingConsolidation.PK;
			packageJob.KJ_ParentTableCode = shipmentBookingConsolidation.TablePrefix;
			var package = packageJob.Packages.AddNew("PLT", 10);

			var packageJobWrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			((IDocTypeCode)packageJobWrapper).DocTypeCode = "PPL";
			((IPackageOverrider)packageJobWrapper).SetPackageOverride(package);

			AssertEquals("Since there are no divots delivery address should be null.", null, packageJobWrapper.DeliveryAddress);
			AssertEquals("Since there are no divots pickup address should be null.", null, packageJobWrapper.PickupAddress);
			AssertEquals("Since there are no divots TransportReference should be empty.", ZString.Empty, packageJobWrapper.TransportReference);
			AssertEquals("Since there are no divots DeliveryRequiredBy should be empty.", ZDateTime.Empty, packageJobWrapper.DeliveryRequiredBy);
			AssertEquals("Since there are no divots Customer reference should be empty.", ZString.Empty, packageJobWrapper.CustomerReference);
			AssertEquals("Since there are no divots carrier service level should be null.", null, packageJobWrapper.CarrierServiceLevel);
			AssertEquals("Since there there is no shippers reference specified it should be empty.", ZString.Empty, packageJobWrapper.ShippersReference);
			AssertEquals("Since there are no divots DeliveryAgent should be empty.", null, packageJobWrapper.DeliveryAgent);

			var pickupDivot = pickUpInstruction.PackageDivots.AddNew();
			pickupDivot.KD_KP_Package = package.PK;

			var deliveryDivot = deliveryInstruction.PackageDivots.AddNew();
			deliveryDivot.KD_KP_Package = package.PK;

			packageJobWrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			((IDocTypeCode)packageJobWrapper).DocTypeCode = "PPL";
			((IPackageOverrider)packageJobWrapper).SetPackageOverride(package);

			shipment.JS_BookingReference = "Booking reference 1";
			booking.KM_TransportReference = "12345";
			booking.KM_PL_NKCarrierServiceLevel = "STD";

			string orderItemString = "Order3, Order4";
			shipment.DocsAndCartage.JP_OrderItemsAsString = orderItemString;

			AssertEquals("Delivery address should be from the delivery instructions for the package job.", deliveryInstruction.Address, packageJobWrapper.DeliveryAddress.WrappedObject); // WarppedObject should be a JobDocAddress
			AssertEquals("Delivery address should be from the pickup instructions for the package job.", pickUpInstruction.Address, packageJobWrapper.PickupAddress.WrappedObject);
			AssertEquals("Transport reference should be from the corresponding booking.", "12345", packageJobWrapper.TransportReference);
			AssertEquals("Carrier service level should be from the corresponding booking.", "STD", packageJobWrapper.CarrierServiceLevel.Code);
			AssertEquals("Delivery required date should be from the delivery instructions since divots are not available.", deliveryInstructionConfirmation.KK_RequiredTo.Date, packageJobWrapper.DeliveryRequiredBy);
			AssertEquals("Customer reference from OrderItemsAsString property.", orderItemString, packageJobWrapper.CustomerReference);

			AssertEquals("Booking reference should be from shipment.", shipment.JS_BookingReference, packageJobWrapper.ShippersReference);

			package.KP_TransportRef = "TRANS_PACKAGE1234";
			AssertEquals("Transport reference should be from the corresponding booking.", package.KP_TransportRef, packageJobWrapper.TransportReference);

			booking.Address.E2_OA_Address = orgDeliveryAgent.MainAddress.PK;
			AssertEquals("Delivery agent should be retrieved from the booking.", orgDeliveryAgent, packageJobWrapper.DeliveryAgent.Organisation);

			shipment.DocsAndCartage.JP_OrderItemsAsString = ZString.Empty;
			var shipmentOrder1 = shipment.DocsAndCartage.OrderItems.AddNew();
			var shipmentOrder2 = shipment.DocsAndCartage.OrderItems.AddNew();
			shipmentOrder1.JT_OrderReference = "Order1";
			shipmentOrder2.JT_OrderReference = "Order2";

			packageJobWrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			((IDocTypeCode)packageJobWrapper).DocTypeCode = "PPL";
			((IPackageOverrider)packageJobWrapper).SetPackageOverride(package);

			AssertEquals("all order references should be returned as comma separated values.", string.Join(", ", shipmentOrder1.JT_OrderReference, shipmentOrder2.JT_OrderReference), packageJobWrapper.CustomerReference);

			var divotConfirmation = deliveryDivot.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Delivery required date should be from the delivery instructions since divot does not contain requiredto date.", deliveryInstructionConfirmation.KK_RequiredTo.Date, packageJobWrapper.DeliveryRequiredBy);

			var deliveryInstructionConfirmation2 = deliveryInstruction.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			deliveryInstructionConfirmation2.KK_Estimated = now.AddDays(2);
			deliveryInstructionConfirmation2.KK_RequiredTo = now.AddDays(2);
			AssertEquals("Since delivery instructions have two delivery confirmations with requiredto date and there are no delivery divots, return empty date time.", ZDateTime.Empty, packageJobWrapper.DeliveryRequiredBy);

			divotConfirmation.KK_Estimated = now.AddDays(3);
			divotConfirmation.KK_RequiredTo = now.AddDays(3);
			AssertEquals("Delivery required date should be from delivery divot.", divotConfirmation.KK_RequiredTo.Date, packageJobWrapper.DeliveryRequiredBy);

			var divotConfirmation2 = deliveryDivot.Confirmations.AddNew(ConfirmationTypes.Codes.Delivery);
			AssertEquals("Delivery required date should be from the delivery divot since it's having a one divot with delivery requiredto date.", divotConfirmation.KK_RequiredTo.Date, packageJobWrapper.DeliveryRequiredBy);

			divotConfirmation2.KK_Estimated = now.AddDays(4);
			divotConfirmation2.KK_RequiredTo = now.AddDays(4);
			AssertEquals("Since delivery divots have two delivery confirmations delivery required by date should be empty.", ZDateTime.Empty, packageJobWrapper.DeliveryRequiredBy);
		}

		#endregion

		#region TestPackageIndexAndCount

		public void TestPackageIndexAndCount()
		{
			var package = Helper.CreatePackage("KEG", "123", packageJob.Packages);
			var wrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			var iPackageOverrider = (IPackageOverrider)wrapper;
			iPackageOverrider.SetPackageOverride(package, null, 2, 3);
			AssertEquals("wrapper.DocumentNumber is incorrect", 2, wrapper.DocumentNumber);
			AssertEquals("wrapper.DocumentTotal is incorrect", 3, wrapper.DocumentTotal);
		}

		#endregion

		#region TestPackageUOMTypeAndNumberAndTotal

		public void TestPackageUOMTypeAndNumberAndTotal()
		{
			var package = Helper.CreatePackage("PLT", "PLT01", packageJob.Packages);
			var wrapper = new FreightWrapperFromPkgPackageJob(packageJob, packageJob.Factory);
			var iPackageOverrider = (IPackageOverrider)wrapper;
			iPackageOverrider.SetPackageOverride(package, uomTypeNumber: 3, uomTypeTotal: 4);
			AssertEquals("wrapper.UOMTypeNumber is incorrect", 3, wrapper.UOMTypeNumber);
			AssertEquals("wrapper.UOMTypeTotal is incorrect", 4, wrapper.UOMTypeTotal);
		}

		#region TestUOMTypeAndNumberAndTotalInWrappers

		public void TestUOMTypeAndNumberAndTotalInWrappers()
		{
			SetupUOMPackType();
			var data = new TestDataForPacking(Factory);
			data.CreatePackingData();
			var outerPackge1 = PackingHelper.CreatePackage(data.PackageJob, "O1", 1, Constants.PkgUnit.Pallet);
			var innerPackage1_outer1 = PackingHelper.CreatePackage(outerPackge1, 1, Constants.PkgUnit.Carton, "P1");
			var innerPackage2_outer1 = PackingHelper.CreatePackage(outerPackge1, 1, Constants.PkgUnit.Carton, "P2");
			var outerPackge2 = PackingHelper.CreatePackage(data.PackageJob, "O2", 1, Constants.PkgUnit.Pallet);
			var innerPackage1_outer2 = PackingHelper.CreatePackage(outerPackge2, 1, Constants.PkgUnit.Bottle, "P3");
			var outerPackge3 = PackingHelper.CreatePackage(data.PackageJob, "O3", 1, Constants.PkgUnit.Box);
			var innerPackage1_outer3 = PackingHelper.CreatePackage(outerPackge3, 1, Constants.PkgUnit.Carton, "P4");

			Factory.Save();

			AssertEquals("Precondition: outerPackge1 should be a UOM Type.", "PLT", outerPackge1.PackType.F3_UOMType);
			AssertEquals("Precondition: outerPackge2 should be a UOM Type.", "PLT", outerPackge2.PackType.F3_UOMType);
			AssertEquals("Precondition: outerPackge3 should not be a UOM Type.", "", outerPackge3.PackType.F3_UOMType);
			AssertEquals("Precondition: innerPackage1_outer1 should be a UOM Type.", "CAS", innerPackage1_outer1.PackType.F3_UOMType);
			AssertEquals("Precondition: innerPackage2_outer1 should be a UOM Type.", "CAS", innerPackage2_outer1.PackType.F3_UOMType);
			AssertEquals("Precondition: innerPackage2_outer2 should not be a UOM Type.", "", innerPackage1_outer2.PackType.F3_UOMType);
			AssertEquals("Precondition: innerPackage1_outer3 should be a UOM Type.", "CAS", innerPackage1_outer3.PackType.F3_UOMType);

			var wrappers = data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabel, Factory.New<StmMenuItem>());
			AssertEquals(3, wrappers.Length);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[0], 1, 2);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[1], 2, 2);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[2], 0, 0);

			wrappers = data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
			AssertEquals(7, wrappers.Length);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[0], 1, 2);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[1], 1, 3);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[2], 2, 3);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[3], 2, 2);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[4], 0, 0);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[5], 0, 0);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[6], 3, 3);
		}

		public void TestUOMTypeAndNumberAndTotalInWrappers_3Levels()
		{
			SetupUOMPackType();
			var data = new TestDataForPacking(Factory);
			data.CreatePackingData();
			var p1 = PackingHelper.CreatePackage(data.PackageJob, "P1", 1, Constants.PkgUnit.Pallet);
			var p2 = PackingHelper.CreatePackage(p1, 1, Constants.PkgUnit.Carton, "P2");
			var p3 = PackingHelper.CreatePackage(p1, 1, Constants.PkgUnit.Carton, "P3");
			var p4 = PackingHelper.CreatePackage(p3, 1, Constants.PkgUnit.Case, "P4");
			var p5 = PackingHelper.CreatePackage(p3, 1, Constants.PkgUnit.Case, "P5");
			var p6 = PackingHelper.CreatePackage(p3, 1, Constants.PkgUnit.Case, "P6");

			Factory.Save();

			AssertEquals("Precondition: p1 should be a UOM Type.", UOMPackTypesList.Codes.Pallet, p1.PackType.F3_UOMType);
			AssertEquals("Precondition: p2 should be a UOM Type.", UOMPackTypesList.Codes.Case, p2.PackType.F3_UOMType);
			AssertEquals("Precondition: p3 should be a UOM Type.", UOMPackTypesList.Codes.Case, p3.PackType.F3_UOMType);
			AssertEquals("Precondition: p4 should be a UOM Type.", UOMPackTypesList.Codes.SplitCase, p4.PackType.F3_UOMType);
			AssertEquals("Precondition: p5 should be a UOM Type.", UOMPackTypesList.Codes.SplitCase, p5.PackType.F3_UOMType);
			AssertEquals("Precondition: p6 should be a UOM Type.", UOMPackTypesList.Codes.SplitCase, p6.PackType.F3_UOMType);

			var wrappers = data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
			AssertEquals(6, wrappers.Length);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[0], 1, 1);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[1], 1, 2);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[2], 2, 2);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[3], 1, 3);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[4], 2, 3);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[5], 3, 3);
		}

		public void TestUOMTypeAndNumberAndTotalInWrappers_DifferentPackTypeWithSameUOMType()
		{
			var anotherPallet = Factory.NewWithValidTestData<RefPackType>();
			anotherPallet.F3_Code = "T99";
			anotherPallet.F3_UOMType = UOMPackTypesList.Codes.Pallet;
			SetupUOMPackType();
			var data = new TestDataForPacking(Factory);
			data.CreatePackingData();
			var p1 = PackingHelper.CreatePackage(data.PackageJob, "P1", 1, Constants.PkgUnit.Pallet);
			var p2 = PackingHelper.CreatePackage(data.PackageJob, "P2", 1, Constants.PkgUnit.Pallet);
			var p3 = PackingHelper.CreatePackage(data.PackageJob, "P3", 1, anotherPallet.F3_Code);

			Factory.Save();

			AssertEquals("Precondition: p1 should be a UOM Type.", UOMPackTypesList.Codes.Pallet, p1.PackType.F3_UOMType);
			AssertEquals("Precondition: p2 should be a UOM Type.", UOMPackTypesList.Codes.Pallet, p2.PackType.F3_UOMType);
			AssertEquals("Precondition: p3 should be a UOM Type.", UOMPackTypesList.Codes.Pallet, p3.PackType.F3_UOMType);

			var wrappers = data.PackageJobDocumentSupporter.GetDocumentWrappers(Constants.DataContext.GenericRetailersLabelAll, Factory.New<StmMenuItem>());
			AssertEquals(3, wrappers.Length);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[0], 1, 3);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[1], 2, 3);
			AssertUOMNumberAndTotal((FreightWrapperFromPkgPackageJob)wrappers[2], 3, 3);
		}

		void SetupUOMPackType()
		{
			var pallet = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, "PLT"));
			pallet.F3_UOMType = UOMPackTypesList.Codes.Pallet;
			var carton = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, "CTN"));
			carton.F3_UOMType = UOMPackTypesList.Codes.Case;
			var caseType = Factory.LoadTop1<RefPackType>(new ZQuery(RefPackTypeSchema.F3_Code, "CAS"));
			caseType.F3_UOMType = UOMPackTypesList.Codes.SplitCase;
		}

		void AssertUOMNumberAndTotal(FreightWrapperFromPkgPackageJob wrapper, ZInt uomTypeNumber, ZInt uomTypeTotal)
		{
			AssertEquals("UOM Type Number is incorrect", uomTypeNumber, wrapper.UOMTypeNumber);
			AssertEquals("UOM Type Total is incorrect", uomTypeTotal, wrapper.UOMTypeTotal);
		}

		#endregion

		#endregion

		#region Implementation

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
ParentJob :  is null";
			}
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromPkgPackageJob(packageJob, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<PkgPackageJob>();
		}

		#region Helper

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;

		#endregion

		#region PackingHelper

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;

		#endregion

		#region TransportBookingHelper

		TransportBookingTestHelper TransportBookingHelper
		{
			get { return transportBookingHelper ?? (transportBookingHelper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper transportBookingHelper;

		#endregion

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			packageJob = Factory.New<PkgPackageJob>();
		}

		PkgPackageJob packageJob;

		#endregion
	}
}
