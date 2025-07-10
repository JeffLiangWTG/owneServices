using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.DocumentWrappers.GenericWrappers.Warehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.TransportBookings.Business.Testing;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromWhsBO))]
	sealed class FreightWrapperFromWhsBOTest : FreightWrapperTest
	{
		#region Properties

		#region TestGetWarehouseJob

		public void TestGetWarehouseJob()
		{
			var receive = Factory.New<WhsReceive>();
			var adjustment = Factory.New<WhsAdjustment>();
			var order = Factory.New<WhsOrder>();
			var workOrder = Factory.New<WhsWorkOrder>();
			var pick = Factory.New<WhsPick>();
			var transfer = Factory.New<WhsTransfer>();
			var stocktake = Factory.New<WhsStocktake>();

			pick.Orders.Add(order);

			AssertWarehouseJobWrapper(adjustment, "WAC", typeof(WarehouseAdjustmentConfirmationWrapper), typeof(WarehouseAdjustmentConfirmationWrapper));

			AssertWarehouseJobWrapper(receive, "WPU", typeof(WarehouseReceiveWrapper), typeof(WarehouseReceiveWrapper));
			AssertWarehouseJobWrapper(receive, "WDL", typeof(WarehouseReceiveWrapper), typeof(WarehouseReceiveWrapper));
			AssertWarehouseJobWrapper(receive, "WIC", typeof(WarehouseReceiveWrapper), typeof(WarehouseReceiveWrapper));
			AssertWarehouseJobWrapper(receive, "MYD", typeof(WarehouseReceiveWrapper), typeof(WarehouseReceiveWrapper));

			AssertWarehouseJobWrapper(adjustment, "WIC", typeof(WarehouseAdjustmentConfirmationWrapper), typeof(WarehouseAdjustmentConfirmationWrapper));

			AssertWarehouseJobWrapper(transfer, "WTR", typeof(WarehouseTransferWrapper), typeof(WarehouseTransferWrapper));
			AssertWarehouseJobWrapper(transfer, "WIC", typeof(WarehouseTransferWrapper), typeof(WarehouseTransferWrapper));

			AssertWarehouseJobWrapper(stocktake, "WSS", typeof(WarehouseStocktakeWrapper), typeof(WarehouseStocktakeWrapper));
			AssertWarehouseJobWrapper(stocktake, "WSV", typeof(WarehouseStocktakeWrapper), typeof(WarehouseStocktakeWrapper));

			AssertWarehouseJobWrapper(order, "WPA", typeof(WarehousePackingSlipWrapper), typeof(WarehouseOrderWrapper));
			AssertWarehouseJobWrapper(order, "INV", typeof(WarehouseOrderWrapper), typeof(WarehouseOrderWrapper));
			AssertWarehouseJobWrapper(order, "PMP", typeof(WarehouseOrderWrapperForManifest), typeof(WarehouseOrderWrapper));
			AssertWarehouseJobWrapper(order, "PMR", typeof(WarehouseOrderWrapperForManifest), typeof(WarehouseOrderWrapper));
			AssertWarehouseJobWrapper(order, "WCA", typeof(WarehouseCartageAdviceWrapper), typeof(WarehouseOrderWrapper));
			AssertWarehouseJobWrapper(order, "PPL", typeof(WarehouseOrderWrapper), typeof(WarehouseOrderWrapper));

			AssertWarehouseJobWrapper(workOrder, "WPA", typeof(WarehousePackingSlipWrapper), typeof(WarehouseWorkOrderWrapper));
			AssertWarehouseJobWrapper(workOrder, "INV", typeof(WarehouseWorkOrderWrapper), typeof(WarehouseWorkOrderWrapper));
			AssertWarehouseJobWrapper(workOrder, "WWO", typeof(WarehouseWorkOrderWrapper), typeof(WarehouseWorkOrderWrapper));

			AssertWarehouseJobWrapper(pick, "PMP", typeof(WarehouseOrderWrapperForManifest), typeof(WarehousePickingSlipWrapper));
			AssertWarehouseJobWrapper(pick, "PMR", typeof(WarehouseOrderWrapperForManifest), typeof(WarehousePickingSlipWrapper));
			AssertWarehouseJobWrapper(pick, "WPS", typeof(WarehousePickingSlipWrapper), typeof(WarehousePickingSlipWrapper));
			AssertWarehouseJobWrapper(pick, "WNP", typeof(WarehousePickNonPickedItemsWrapper), typeof(WarehousePickingSlipWrapper));
			AssertWarehouseJobWrapper(pick, "WPO", typeof(WarehousePickOrderSummaryWrapper), typeof(WarehousePickingSlipWrapper));
			AssertWarehouseJobWrapper(pick, "WSI", typeof(WarehousePickShortfallItemsWrapper), typeof(WarehousePickingSlipWrapper));
			AssertWarehouseJobWrapper(pick, "PPL", typeof(WarehouseOrderWrapper), typeof(WarehousePickingSlipWrapper));
		}

		void AssertWarehouseJobWrapper(BusinessObject bizO, ZString validDocumentWrapper, Type expectedDocumentWrapperType, Type expectedDocumentWrapperTypeWithoutDocType)
		{
			var freightWrapper = FreightWrapperFromWhsBO.New(bizO, Factory)[0];
			AssertEquals(expectedDocumentWrapperTypeWithoutDocType, freightWrapper.WarehouseJob.GetType());

			var freightWrapperWithDocType = FreightWrapperFromWhsBO.New(bizO, Factory)[0];
			((IDocTypeCode)freightWrapperWithDocType).DocTypeCode = validDocumentWrapper;
			AssertEquals(expectedDocumentWrapperType, freightWrapperWithDocType.WarehouseJob.GetType());
		}

		#endregion

		#region TestFullHandlingInstructions

		public void TestFullHandlingInstructions()
		{
			var order = Factory.New<WhsOrder>();
			order.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with no care");
			var wrapper = new FreightWrapperFromWhsBO(order, Factory);
			AssertEquals("Handle with no care", wrapper.FullHandlingInstructions);
		}

		#endregion

		#region TestGetShippersReference

		public void TestGetShippersReference()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			((IDocTypeCode)wrapper).DocTypeCode = "PPL";
			AssertEquals("Since there is no external reference for the order, therfore shippers reference will be an empty string.", ZString.Empty, wrapper.ShippersReference);

			order.WD_ExternalReference = "OrderRef1";
			AssertEquals("WD_ExternalReference should be returned for ShipperReference.", order.WD_ExternalReference, wrapper.ShippersReference);
		}

		#endregion

		#region TestShipmentInnerPacksQty

		public void TestShipmentInnerPacksQty()
		{
			var order = Factory.New<WhsOrder>();
			order.WD_UnitsSent = 3;
			order.WD_PackagesSent = 2;
			order.WD_F3_NKTotalPackType = "BAG";
			order.WD_PalletsSent = 1;

			// PackagesSent
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			var result = wrapper.ShipmentInnerPacksQty;
			AssertEquals("Should return PackagesSent quantity", new ZDecimal(2), result.Value);
			AssertEquals("Should return PackagesSent unit", "BAG", result.Unit.Code);

			// UnitsSent
			order.WD_PalletsSent = 0;
			wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			result = wrapper.ShipmentInnerPacksQty;
			AssertEquals("Should return UnitsSent quantity", new ZDecimal(3), result.Value);
			AssertEquals("Should return UnitsSent unit", Constants.PkgUnit.Package, result.Unit.Code);

			// Empty
			order.WD_PackagesSent = 0;
			wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			result = wrapper.ShipmentInnerPacksQty;
			AssertEquals("Should return 0 quantity", new ZDecimal(0), result.Value);
		}

		#endregion

		#region TestGetShipmentType

		public void TestGetShipmentType()
		{
			var warehouseAddress = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			warehouseAddress.OA_RL_NKRelatedPortCode = "AUSYD";

			var warehouse = Factory.New<WhsWarehouse>();
			warehouse.WW_OA_WarehouseAddress = warehouseAddress.PK;

			var consignee = Factory.NewWithValidTestData<OrgHeader>().MainAddress;
			consignee.OA_RL_NKRelatedPortCode = "AUMEL";

			var order = Factory.New<WhsOrder>();
			order.WD_WW_Whs = warehouse.PK;
			order.ConsigneeDocAddress.E2_OA_Address = consignee.PK;

			var wrapper = new FreightWrapperFromWhsBO(order, Factory);
			((IDocTypeCode)wrapper).DocTypeCode = "PPL";
			AssertEquals("DOM", wrapper.ShipmentType.Code);
		}

		#endregion

		#region TestGetConsignor

		public void TestGetConsignor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrder(data.Org1, data.Whs1);

			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];

			AssertEquals(data.Org1.OH_Code, wrapper.Consignor.CompanyCode);
		}

		#endregion

		#region TestMasterBill

		public void TestMasterBill()
		{
			var order = Factory.New<WhsOrder>();
			var masterBillRef = order.References.AddNew();
			masterBillRef.WX_Reference = "Master Bill 123456";
			masterBillRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.MasterBill;
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];

			AssertEquals(wrapper.WarehouseJob.MasterBill, wrapper.MasterBill);
			AssertEquals("Master Bill 123456", wrapper.MasterBill);
		}

		#endregion

		#region TestMasterBillHeading

		public void TestMasterBillHeading()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertEquals(wrapper.WarehouseJob.MasterBillHeading, wrapper.MasterBillHeading);
			AssertEquals(WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetDescriptionFromCode(WarehouseAdditionalReferenceTypes.Codes.MasterBill), wrapper.MasterBillHeading);
		}

		#endregion

		#region TestHouseBill

		public void TestHouseBill()
		{
			var order = Factory.New<WhsOrder>();
			var houseBillRef = order.References.AddNew();
			houseBillRef.WX_Reference = "House Bill 123456";
			houseBillRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.HouseBill;
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];

			AssertEquals(wrapper.WarehouseJob.HouseBill, wrapper.HouseBill);
			AssertEquals("House Bill 123456", wrapper.HouseBill);
		}

		#endregion

		#region TestHouseBillHeading

		public void TestHouseBillHeading()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertEquals(wrapper.WarehouseJob.HouseBillHeading, wrapper.HouseBillHeading);
			AssertEquals(WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetDescriptionFromCode(WarehouseAdditionalReferenceTypes.Codes.HouseBill), wrapper.HouseBillHeading);
		}

		#endregion

		#region TestOtherReferences

		public void TestOtherReferences()
		{
			var order = Factory.New<WhsOrder>();

			var houseBillRef = order.References.AddNew();
			houseBillRef.WX_Reference = "House Bill 123456";
			houseBillRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.HouseBill;
			var canRef = order.References.AddNew();
			canRef.WX_Reference = "CAN 67890";
			canRef.WX_RefType = "CAN";

			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertEquals(wrapper.WarehouseJob.OtherReferences, wrapper.OtherReferences);
			ZString expected = WarehouseDataRegistry.Instance.AdditionalReferenceType.Value.GetDescriptionFromCode("CAN") + ": CAN 67890";
			AssertEquals(expected, wrapper.OtherReferences);
		}

		#endregion

		#region TestTransportZone

		public void TestTransportZone()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var transportHelper = new TransportBookingTestHelper(Factory);
			var transportCo = Helper.CreateClient("ABCDEFG");

			var consignee = transportHelper.CreateOrganisation("CONSIGNEE");
			var consigneeDocAddress = transportHelper.AddAddressToOrganisation(consignee, "Address1", OrgAddressType.Office);
			consigneeDocAddress.OA_RN_NKCountryCode = "AU";
			consigneeDocAddress.City = "TEST";
			consigneeDocAddress.OA_PostCode = "1200";

			var prov = transportHelper.CreateZoneRateProvider("AU", transportCo);
			var zone = transportHelper.CreateZoneWithPostCodes("V0", prov, "1000", "1500");

			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "O1");
			order.TransportCoPK = transportCo.PK;

			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];

			AssertEquals("TransportZone should not empty", "V0", wrapper.TransportZone);
		}

		#endregion

		#region TestCarrierAccount

		public void TestCarrierAccount()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var client = data.Org1;
			var warehouse = data.Whs1;

			var orgCarrierAccount = Factory.New<OrgCarrierAccount>();
			orgCarrierAccount.OAN_OH_Carrier = client.PK;
			orgCarrierAccount.OAN_AccountNumber = "1234567";

			var accountAssociation = Factory.New<OrgWhsClientAccountAssociation>();
			accountAssociation.OWC_WW_Warehouse = warehouse.PK;
			accountAssociation.OWC_OH_Client = client.PK;
			accountAssociation.OWC_OAN_CarrierAccount = orgCarrierAccount.PK;

			var order = Helper.CreateWhsOrder(client, warehouse);
			order.TransportCoPK = client.PK;
			Factory.Save();

			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertEquals("1234567", wrapper.CarrierAccount.AccountNumber);
		}

		#endregion

		#region TestIsAuthorisedToLeave

		public void TestIsAuthorisedToLeave()
		{
			var wrapper = new FreightWrapperFromWhsBO(null, Factory);
			AssertEquals("When BO is null should return false", false, wrapper.IsAuthorisedToLeave);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Factory.Save();

			AssertEquals("Precondition:", false, order.WD_IsAuthorisedToLeave);

			var wrapper2 = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertEquals("Order.IsAuthorisedToLeave is false", false, wrapper2.IsAuthorisedToLeave);

			order.WD_IsAuthorisedToLeave = true;
			AssertEquals("Order.IsAuthorisedToLeave is true", true, wrapper2.IsAuthorisedToLeave);
		}

		#endregion

		#region TestIsHazardous

		public void TestHazardous()
		{
			var wrapper1 = new FreightWrapperFromWhsBO(null, Factory);
			AssertEquals("When null, should return false.", false, wrapper1.Hazardous);

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			Factory.Save();

			var wrapper2 = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertEquals("Order is not hazardous.", false, wrapper2.Hazardous);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			data.Part1.UNDGs.AddNew();
			AssertEquals("Order is hazardous.", true, wrapper2.Hazardous);
		}

		#endregion

		#endregion

		#region Collections

		#region TestPackages

		public void TestPackages()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertEquals(wrapper.WarehouseJob.Packages, wrapper.Packages);
		}

		#endregion

		#region TestGetUNDGs

		public void TestGetUNDGs()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var helper = new WhsTestHelperFunctions(Factory);
			var order = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 1);

			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertEquals(0, wrapper.UNDGs.Count);

			var undg1 = data.Part1.UNDGs.AddNew();
			var undg2 = data.Part1.UNDGs.AddNew();

			wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertContainsExactElementsInAnyOrder(new[] { undg1, undg2 }, wrapper.UNDGs.Cast<UNDGSubstanceWrapper>().Select(w => w.WrappedObject));
		}

		#endregion

		#region TestCustomsEntries

		public void TestCustomsEntries()
		{
			var order = Factory.New<WhsOrder>();

			var houseBillRef = order.References.AddNew();
			houseBillRef.WX_Reference = "House Bill 123456";
			houseBillRef.WX_RefType = WarehouseAdditionalReferenceTypes.Codes.HouseBill;
			var canRef = order.References.AddNew();
			canRef.WX_Reference = "CAN 67890";
			canRef.WX_RefType = "CAN";

			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			AssertEquals(2, wrapper.CustomsEntries.Count);
			AssertContainsExactElementsInAnyOrder(new[] { houseBillRef, canRef }, wrapper.CustomsEntries.Cast<GenericWrapper>().Select(w => w.WrappedObject));
		}

		#endregion

		#endregion

		#region FreightWrapper

		#region TestGetShipmentInnerPacksQty_NullRef

		public void TestGetShipmentInnerPacksQty_NullRef()
		{
			var order = Factory.New<WhsStocktake>();
			order.Notes.AddNew(false, PredefinedNoteTypes.Instance.HandlingInstructions.Description, "Handle with no care");
			var wrapper = new FreightWrapperFromWhsBO(order, Factory);
			AssertEquals("wrapperEmpty.Unit.Code", ZString.Empty, wrapper.ShipmentInnerPacksQty.Unit.Code);
			AssertEquals("wrapperEmpty.Value", ZDecimal.Zero, wrapper.ShipmentInnerPacksQty.Value);
			AssertEquals("wrapperEmpty.ValueAndUnitCode", "0", wrapper.ShipmentInnerPacksQty.ValueAndUnitCode);
			AssertEquals("wrapperEmpty.ValueAndUnitCodeBlankIfZero", ZString.Empty, wrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero);
		}

		#endregion

		#endregion

		#region IPackingParentWrapper Members

		#region TestIPackingParentWrapper_GetPickupAddress

		public void TestIPackingParentWrapper_GetPickupAddress()
		{
			var client = Factory.NewWithValidTestData<OrgHeader>();
			var order = Factory.New<WhsOrder>();
			order.WD_OH_Client = client.PK;
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			((IDocTypeCode)wrapper).DocTypeCode = "PPL";
			var packingWrapper = (IPackingParentWrapper)wrapper;
			var testAddress = new AddressWrapper(client.MainAddress, ContactType.Consignor, Factory).AddressAsASingleLine;
			AssertEquals("Precondition", false, testAddress.IsEmpty);
			AssertEquals("Although package is null address should come from warehouse order.", testAddress, packingWrapper.GetPickupAddress(null).AddressAsASingleLine);

			var package = CreatePackageForOrder(order);

			AssertEquals("Although package is null its client address should come from warehouse order.", testAddress, packingWrapper.GetPickupAddress(null).AddressAsASingleLine);
			AssertEquals("Regardless of whether there is a package or not address should come from warehouse order.", testAddress, packingWrapper.GetPickupAddress(package).AddressAsASingleLine);
		}

		#endregion

		#region TestIPackingParentWrapper_GetDeliveryAddress

		public void TestIPackingParentWrapper_GetDeliveryAddress()
		{
			var order = Factory.New<WhsOrder>();
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			((IDocTypeCode)wrapper).DocTypeCode = "PPL";
			var packingWrapper = (IPackingParentWrapper)wrapper;

			AssertEquals("Since there is no pickupand delivery address Get delivery address returns empty string.", ZString.Empty, packingWrapper.GetDeliveryAddress(null).AddressAsASingleLine);

			var package = CreatePackageForOrder(order);
			AssertEquals("Since there is no pickupand delivery address Get delivery address returns empty string.", ZString.Empty, packingWrapper.GetDeliveryAddress(null).AddressAsASingleLine);
			AssertEquals("Since there is no pickupand delivery address Get delivery address returns empty string.", ZString.Empty, packingWrapper.GetDeliveryAddress(package).AddressAsASingleLine);

			var pickUpAndDeliveryAddress = consignee.Addresses.AddNew();
			pickUpAndDeliveryAddress.AddAddressType(OrgAddressType.PickupAndDelivery);
			pickUpAndDeliveryAddress.OA_Address1 = "Address1";
			pickUpAndDeliveryAddress.OA_Address2 = "Address2";
			pickUpAndDeliveryAddress.OA_RN_NKCountryCode = "AU";
			order.ConsigneeDocAddress.E2_OA_Address = pickUpAndDeliveryAddress.PK;

			wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			packingWrapper = (IPackingParentWrapper)wrapper;
			AssertEquals("Although there package is passed as null, it should return consignee's pickup and delivery address as delivery address.", "ADDRESS1 ADDRESS2 AUSTRALIA", packingWrapper.GetDeliveryAddress(null).AddressAsASingleLine);
			AssertEquals("Consignee's pikcup and delivery address is returned as delivery address.", "ADDRESS1 ADDRESS2 AUSTRALIA", packingWrapper.GetDeliveryAddress(package).AddressAsASingleLine);
		}

		#endregion

		#region TestIPackingParentWrapper_GetDeliveryRequiredBy

		public void TestIPackingParentWrapper_GetDeliveryRequiredBy()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			((IDocTypeCode)wrapper).DocTypeCode = "PPL";
			var packingWrapper = (IPackingParentWrapper)wrapper;
			AssertEquals("Since there is no delivery required date specified, it should return an empty datetime.", ZDateTime.Empty, packingWrapper.GetDeliveryRequiredBy(null));

			var package = CreatePackageForOrder(order);
			AssertEquals("Since there is no delivery required date specified, it should return an empty datetime.", ZDateTime.Empty, packingWrapper.GetDeliveryRequiredBy(null));
			AssertEquals("Since there is no delivery required date specified, it should return an empty datetime.", ZDateTime.Empty, packingWrapper.GetDeliveryRequiredBy(package));

			order.WD_RequiredDate = ZDateTimeOffset.Now.AddDays(1);
			AssertEquals("Although there is no package passed in, it should still return required date.", order.WD_RequiredDate.ToZDateTime(), packingWrapper.GetDeliveryRequiredBy(null));
			AssertEquals("Delivery required date specified in the order is returned as delivery required by date.", order.WD_RequiredDate.ToZDateTime(), packingWrapper.GetDeliveryRequiredBy(package));
		}

		#endregion

		#region TestIPackingParentWrapper_GetTransportReference

		public void TestIPackingParentWrapper_GetTransportReference()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			var packingWrapper = (IPackingParentWrapper)wrapper;
			AssertEquals("Since there is no transport reference specified, it should return an empty string.", ZString.Empty, packingWrapper.GetTransportReference(null));

			var package = CreatePackageForOrder(order);
			AssertEquals("Since there is no transport reference specified, it should return an empty string.", ZString.Empty, packingWrapper.GetTransportReference(null));
			AssertEquals("Since there is no transport reference specified, it should return an empty string.", ZString.Empty, packingWrapper.GetTransportReference(package));

			order.WD_TransportReference = "TRANS123";
			AssertEquals("Although there is no package passed in, it should still return transport reference.", order.WD_TransportReference, packingWrapper.GetTransportReference(null));
			AssertEquals("Transport reference specified in the order is returned as transport reference.", order.WD_TransportReference, packingWrapper.GetTransportReference(package));
		}

		#endregion

		#region TestIPackingParentWrapper_GetTransportCompany

		public void TestIPackingParentWrapper_GetTransportCompany()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			var transportCompany = Factory.NewWithValidTestData<OrgHeader>();
			((IDocTypeCode)wrapper).DocTypeCode = "PPL";
			var packingWrapper = (IPackingParentWrapper)wrapper;
			AssertEquals("Since there is no transport company specified, it should be null.", null, packingWrapper.GetTransportCompany(null).Organisation);

			var package = CreatePackageForOrder(order);
			wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			((IDocTypeCode)wrapper).DocTypeCode = "PPL";
			AssertEquals("Since there is no transport company specified, it should be null.", null, packingWrapper.GetTransportCompany(null).Organisation);
			AssertEquals("Since there is no transport company specified, it should be null.", null, packingWrapper.GetTransportCompany(package).Organisation);

			order.TransportCoDocAddress.E2_OA_Address = transportCompany.MainAddress.PK;
			wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			((IDocTypeCode)wrapper).DocTypeCode = "PPL";
			packingWrapper = (IPackingParentWrapper)wrapper;
			AssertEquals("Although there is no package passed in, it should still return transport company.", transportCompany, packingWrapper.GetTransportCompany(null).Organisation);
			AssertEquals("it should return transport company since we specified it.", transportCompany, packingWrapper.GetTransportCompany(package).Organisation);
		}

		#endregion

		#region TestIPackingParentWrapper_GetCustomerReference

		public void TestIPackingParentWrapper_GetCustomerReference()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			var packingWrapper = (IPackingParentWrapper)wrapper;
			AssertEquals("Since there is no customer reference specified, it should return an empty string.", ZString.Empty, packingWrapper.GetCustomerReference(null));

			var package = CreatePackageForOrder(order);
			AssertEquals("Since there is no customer reference specified, it should return an empty string.", ZString.Empty, packingWrapper.GetCustomerReference(null));
			AssertEquals("Since there is no customer reference specified, it should return an empty string.", ZString.Empty, packingWrapper.GetCustomerReference(package));

			order.WD_CustomerReference = "TRANS123";
			AssertEquals("Although there is no package passed in, it should still return customer reference from the order.", "TRANS123", packingWrapper.GetCustomerReference(null));
			AssertEquals("Customer reference should be returned.", "TRANS123", packingWrapper.GetCustomerReference(package));
		}

		#endregion

		#region TestIPackingParentWrapper_GetOwnerReference

		public void TestIPackingParentWrapper_GetOwnerReference()
		{
			var order = Factory.New<WhsOrder>();
			var wrapper = FreightWrapperFromWhsBO.New(order, Factory)[0];
			var packingWrapper = (IPackingParentWrapper)wrapper;
			AssertEquals("Since there is no receiver reference specified, it should return an empty string.", ZString.Empty, packingWrapper.GetOwnerReference(null));

			var package = CreatePackageForOrder(order);
			AssertEquals("Since there is no owner reference specified, it should return an empty string.", ZString.Empty, packingWrapper.GetOwnerReference(null));
			AssertEquals("Since there is no owner reference specified, it should return an empty string.", ZString.Empty, packingWrapper.GetOwnerReference(package));

			order.WD_ExternalReference = "TRANS123";
			AssertEquals("Although there is no package passed in, it should still return order reference from the order.", "TRANS123", packingWrapper.GetOwnerReference(null));
			AssertEquals("Customer reference should be returned.", "TRANS123", packingWrapper.GetOwnerReference(package));
		}

		#endregion

		#region TestIPackingParentWrapper_GetCarrierServiceLevel

		public void TestIPackingParentWrapper_GetCarrierServiceLevel()
		{
			var order = Factory.New<WhsOrder>();
			var packingWrapper1 = GetNewPackingParentWrapper(order);
			AssertEquals("Since there is no carrier service level specified, it should return empty.", ZString.Empty, packingWrapper1.GetCarrierServiceLevel(null).Code);

			var package = CreatePackageForOrder(order);
			var packingWrapper2 = GetNewPackingParentWrapper(order);
			AssertEquals("Since there is no carrier service level specified, it should return empty.", ZString.Empty, packingWrapper2.GetCarrierServiceLevel(null).Code);
			var packingWrapper3 = GetNewPackingParentWrapper(order);
			AssertEquals("Since there is no carrier service level specified, it should return empty.", ZString.Empty, packingWrapper3.GetCarrierServiceLevel(package).Code);

			order.WD_PL_NKCarrierServiceLevel = "STD";
			var packingWrapper4 = GetNewPackingParentWrapper(order);
			AssertEquals("Although there is no package passed in, it should still return carrier service level from the order.", "STD", packingWrapper4.GetCarrierServiceLevel(null).Code);
			var packingWrapper5 = GetNewPackingParentWrapper(order);
			AssertEquals("Carrier service level should be returned.", "STD", packingWrapper5.GetCarrierServiceLevel(package).Code);
		}

		IPackingParentWrapper GetNewPackingParentWrapper(WhsOrder order)
		{
			return (IPackingParentWrapper)FreightWrapperFromWhsBO.New(order, Factory)[0];
		}

		#endregion

		#endregion

		#region IWhsDocumentInventory Members

		public void TestIWhsDocumentInventory_SetInventory()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R3");
			var line1 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, null, "123");
			var line2 = Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 0m, null, "789");
			Factory.Save();

			var wrapper = FreightWrapperFromWhsBO.New(receive, Factory)[0];
			((IDocTypeCode)wrapper).DocTypeCode = "WPU";
			AssertEquals(2, wrapper.WarehouseJob.JobLines.Count);

			wrapper = FreightWrapperFromWhsBO.New(receive, Factory)[0];
			((IDocTypeCode)wrapper).DocTypeCode = "WPU";
			var iWrapper = (IWhsDocumentInventory)wrapper;
			var docInventory = new WhsDocumentInventory(line2);
			docInventory.LabelsToPrint = 1;
			iWrapper.SetInventory(docInventory);

			AssertEquals(1, wrapper.WarehouseJob.JobLines.Count);
			AssertEquals(line2.InDocketLine, wrapper.WarehouseJob.JobLines[0].WrappedObject);
		}

		#endregion

		#region Implementation

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "HouseBillHeading", "House Bill" },
					{ "IsDomestic", ZBool.True.ToString() },
					{ "JobNumberHeading", "Order Number" },
					{ "MasterBillHeading", "Master Bill" },
					{ "SecondaryHeading", "ORDER Details" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
ShipmentType : DOM - Domestic
SupplierBuyerLink : (No Default Field Value Available on SupplierBuyerLink)
WarehouseJob : (No Default Field Value Available on WarehouseJob)";
			}
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return Factory.New<WhsOrder>();
		}

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			var whsOrder = (WhsOrder)GetNewBusinessObjectToWrap();
			return new FreightWrapperFromWhsBO(whsOrder, Factory);
		}

		public override void TestOrgWrappersReturnTypesOnEmptyWrapper()
		{
			Assert(true);
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		PkgPackage CreatePackageForOrder(WhsOrder order)
		{
			var packageJob = Factory.New<PkgPackageJob>();
			packageJob.KJ_ParentID = order.PK;
			packageJob.KJ_ParentTableCode = order.TablePrefix;
			var package = packageJob.Packages.AddNew("PLT", 10);
			return package;
		}

		WhsTestHelperFunctions Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctions(Factory)); }
		}
		WhsTestHelperFunctions helper;
		#endregion
	}
}
