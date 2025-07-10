using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.DocumentWrappers.GenericWrappers.Base.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromOrder))]
	sealed class FreightWrapperFromOrderTest : FreightWrapperTest
	{
		public void TestAlternativeBranding()
		{
			var orgBuyer = Factory.NewWithValidTestData<OrgHeader>();

			using (MemoryStream stream = new MemoryStream())
			using (Bitmap image = new Bitmap(22, 11))
			{
				image.Save(stream, ImageFormat.Bmp);
				stream.Flush();
				orgBuyer.MiscServ.ClientDocumentLogo = stream.ToArray();
			}

			var order = Factory.New<Order>();
			order.BuyerPK = orgBuyer.PK;

			var wrapper = new FreightWrapperFromOrder(order, Factory);

			AssertNotNull("wrapper.AlternativeBranding", wrapper.CompanyLogo);
			AssertEquals("wrapper.AlternativeBranding.Height", 11, wrapper.CompanyLogo.Height);
			AssertEquals("wrapper.AlternativeBranding.Width", 22, wrapper.CompanyLogo.Width);
		}

		public void TestGetFactoryEx()
		{
			var order = Factory.New<Order>();
			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("FactoryEx", ZDateTime.Empty, wrapper.FactoryEx);

			var now = new ZDateTimeOffset(2015, 09, 30);

			order.UpdateEvent(Events.ExWorks, now);
			order.UpdateEventEstimate(Events.ExWorks, ZDateTimeOffset.Empty);

			AssertEquals("FactoryEx should get from actual date at first", now.ToZDateTime(), wrapper.FactoryEx);

			order.UpdateEventEstimate(Events.ExWorks, now.AddDays(1));
			order.UpdateEvent(Events.ExWorks, ZDateTimeOffset.Empty);

			AssertEquals("FactoryEx should get from estimate date when actual date is empty", now.AddDays(1).ToZDateTime(), wrapper.FactoryEx);
		}

		public void TestConsignorAndConsignee()
		{
			var order = Factory.New<Order>();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SUPPLIER FROM ORDER";
			order.SupplierPK = supplier.PK;
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "BUYER FROM ORDER";
			order.BuyerPK = buyer.PK;

			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Consignor should be the order's supplier when no shipment or declaration is attached", "SUPPLIER FROM ORDER", wrapper.Consignor.CompanyName);
			AssertEquals("Consignee should be the order's buyer when no shipment or declaration is attached", "BUYER FROM ORDER", wrapper.Consignee.CompanyName);

			var declaration = Factory.New<BaseJobDeclaration>();
			order.JD_JE = declaration.PK;

			var declarationSupplier = Factory.New<OrgHeader>();
			declarationSupplier.OH_FullName = "SUPPLIER FROM DECLARATION";
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			var declarationImporter = Factory.New<OrgHeader>();
			declarationImporter.OH_FullName = "IMPORTER FROM DECLARATION";
			declaration.JE_OH_Importer = declarationImporter.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Consignor should be the declaration's supplier when no shipment is attached", "SUPPLIER FROM DECLARATION", wrapper.Consignor.CompanyName);
			AssertEquals("Consignee should be the declaration's importer when no shipment is attached", "IMPORTER FROM DECLARATION", wrapper.Consignee.CompanyName);

			var shipment = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment.PK;

			var shipmentConsignor = Factory.New<OrgHeader>();
			shipmentConsignor.OH_FullName = "CONSIGNOR FROM SHIPMENT";
			shipment.ConsignorPK = shipmentConsignor.PK;
			var shipmentConsignee = Factory.New<OrgHeader>();
			shipmentConsignee.OH_FullName = "CONSIGNEE FROM SHIPMENT";
			shipment.ConsigneePK = shipmentConsignee.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Consignor should be the shipment's consignor if a shipment is attached", "CONSIGNOR FROM SHIPMENT", wrapper.Consignor.CompanyName);
			AssertEquals("Consignee should be the shipment's consignee if a shipment is attached", "CONSIGNEE FROM SHIPMENT", wrapper.Consignee.CompanyName);
		}

		public void TestConsignorAddress()
		{
			var supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "SUPPLIER FROM ORDER";
			supplier.MainAddress.OA_Address1 = "123 test address";
			supplier.MainAddress.OA_RN_NKCountryCode = "AU";

			var address2 = supplier.Addresses.AddNew();
			address2.OA_Address1 = "456 test address 2nd";
			address2.OA_RN_NKCountryCode = "AU";
			address2.AddAddressType(OrgAddressType.Office);

			var order = Factory.New<Order>();
			order.SupplierPK = supplier.PK;
			order.JD_OA_SupplierAddress = address2.PK;

			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("456 TEST ADDRESS 2ND\nAUSTRALIA", wrapper.Consignor.MainAddress.Address);
		}

		public void TestConsigneeAddress()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "BUYER FROM ORDER";
			buyer.MainAddress.OA_Address1 = "123 test address";
			buyer.MainAddress.OA_RN_NKCountryCode = "AU";

			var address2 = buyer.Addresses.AddNew();
			address2.OA_Address1 = "456 test address 2nd";
			address2.OA_RN_NKCountryCode = "AU";
			address2.AddAddressType(OrgAddressType.Office);

			var order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.JD_OA_BuyerAddress = address2.PK;

			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("456 TEST ADDRESS 2ND\nAUSTRALIA", wrapper.Consignee.MainAddress.Address);
		}

		public void TestSecondaryHeadingAndNumber()
		{
			Order order = Factory.New<Order>();
			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("SecondaryHeading", ZString.Empty, wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber", ZString.Empty, wrapper.SecondaryNumber);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_DeclarationReference = "B00001029";
			order.JD_JE = declaration.PK;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("SecondaryHeading", "Declaration", wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber", "B00001029", wrapper.SecondaryNumber);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00001004";
			order.JD_JS = shipment.PK;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("SecondaryHeading", "Shipment", wrapper.SecondaryHeading);
			AssertEquals("SecondaryNumber", "S00001004", wrapper.SecondaryNumber);
		}

		public void TestShipmentOuterPacksQty()
		{
			Order order = Factory.New<Order>();
			order.JD_JS = ZGuid.Empty;

			OrderLine line1 = order.OrderLines.AddNew();
			line1.JO_OuterPacks = 20;

			OrderLine line2 = order.OrderLines.AddNew();
			line2.JO_OuterPacks = 40;

			order.JD_Packs = 30;
			order.JD_F3_NKPackType = Constants.PkgUnit.Drum;

			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Packages", "30 DRM", wrapper.ShipmentOuterPacksQty.ValueAndUnitCode);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			order.JD_JE = declaration.PK;
			declaration.JE_TotalNoOfPacks = 501;
			declaration.JE_TotalNoOfPacksPackType = Constants.PkgUnit.Reel;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Packages", "501 REL", wrapper.ShipmentOuterPacksQty.ValueAndUnitCode);

			order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OuterPacks = 120;
			shipment.JS_F3_NKPackType = Constants.PkgUnit.Pallet;
			order.JD_JS = shipment.PK;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Packages", "120 PLT", wrapper.ShipmentOuterPacksQty.ValueAndUnitCode);
		}

		public void TestWeight()
		{
			Order order = Factory.New<Order>();
			order.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			order.JD_JS = ZGuid.Empty;
			order.JD_ActualWeight = 28m;
			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Weight", "28.000 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			order.JD_JE = declaration.PK;
			declaration.JE_TotalWeight = 30m;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Weight", "30.000 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment.PK;
			shipment.JS_ActualWeight = 8.000m;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Weight", "8.000 KG", wrapper.Weight.ValueAndUnitCodeBlankIfZero);
		}

		public void TestGoodsAvailableAt()
		{
			var order = Factory.New<Order>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			order.SupplierPK = supplier.PK;
			order.GoodsAvailableAtAddress.E2_OA_Address = supplier.Addresses[0].PK;
			order.JD_JS = ZGuid.Empty;
			order.JD_JE = ZGuid.Empty;

			var orgAddress = order.GoodsAvailableAtAddress.Address;
			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("OrderWrapper should have a JobDocAddress", order.GoodsAvailableAtAddress, wrapper.GoodsAvailableAt.WrappedObject);

			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			order.JD_JE = declaration.PK;
			declaration.SupplierPickupAddress.E2_OA_Address = orgAddress.PK;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("JobDocAddress should come from Declaration", declaration.SupplierPickupAddress, wrapper.GoodsAvailableAt.WrappedObject);

			var shipment = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment.PK;
			shipment.ConsignorPickupAddress.E2_OA_Address = orgAddress.PK;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("JobDocAddress should come from Shipment", shipment.ConsignorPickupAddress, wrapper.GoodsAvailableAt.WrappedObject);
		}

		public void TestPackages()
		{
			Order order = Factory.New<Order>();
			order.JD_JS = ZGuid.Empty;
			order.JD_JE = ZGuid.Empty;
			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Packages", 0, wrapper.Packages.Count);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			order.JD_JE = declaration.PK;
			var customsPack = declaration.Packages.AddNew();
			customsPack.CW_PackType = "PCK";
			customsPack.CW_PackQty = 21;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Packages", 1, wrapper.Packages.Count);
			AssertEquals("Quantity", 21m, wrapper.Packages[0].Packages.Value);
			AssertEquals("Unit", "PCK", wrapper.Packages[0].Packages.Unit.Code);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment.PK;
			var shipmentPack = shipment.OuterPackLines.AddNew();
			shipmentPack.JL_PackageCount = 20;
			shipmentPack.JL_F3_NKPackType = "PCK";
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Packages", 1, wrapper.Packages.Count);
			AssertEquals("Quantity", 20m, wrapper.Packages[0].Packages.Value);
			AssertEquals("Unit", "PCK", wrapper.Packages[0].Packages.Unit.Code);
		}

		public void TestExportReceivingDepotAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Order order = Factory.New<Order>();
			order.JD_JS = shipment.PK;
			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingDepotAddress.Address);

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO DEPARTUREDEPOTADDRESS 55";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTUREDEPOTADDRESS 55\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);

			OrgAddress shipmentExportReceivingDepot = Factory.New<OrgAddress>();
			shipmentExportReceivingDepot.OA_Address1 = "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66";
			shipmentExportReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentExportReceivingDepot.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Should return AddressWrapper with ShipmentBO.ExportReceivingDepot", "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66\nAUSTRALIA", wrapper.ExportReceivingDepotAddress.Address);
		}

		public void TestMasterBillNum()
		{
			Order order = Factory.New<Order>();

			order.JD_JS = ZGuid.Empty;
			order.JD_JE = ZGuid.Empty;
			order.JD_MasterWaybill = "M1111";
			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("MasterWayBill", "M1111", wrapper.MasterBill);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			order.JD_JE = declaration.PK;
			declaration.JE_MasterBill = "MD111";
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Order Wrapper should get masterbill from Declaration", "MD111", wrapper.MasterBill);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			order.JD_JS = shipment.PK;
			consol.JK_MasterBillNum = "MC111";
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Order Wrapper should get masterbill from Consol", "MC111", wrapper.MasterBill);
		}

		public void TestGetGoodsDescription()
		{
			Order order = Factory.New<Order>();

			order.JD_JS = ZGuid.Empty;
			order.JD_JE = ZGuid.Empty;
			order.JD_OrderGoodsDescription = "GOODS";
			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("GoodsDescription", "GOODS", wrapper.GoodsDescription);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			order.JD_JS = shipment.PK;
			shipment.JS_GoodsDescription = "SHIPMENT OF GOODS";
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Order Wrapper should get goods description from Shipments", "SHIPMENT OF GOODS", wrapper.GoodsDescription);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			order.JD_JE = declaration.PK;
			declaration.JE_GoodsDescription = "SHIPMENT OF GOODS DECLARATION";
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Order Wrapper should get goods description from Declaration", "SHIPMENT OF GOODS DECLARATION", wrapper.GoodsDescription);
		}

		public void TestExportReceivingCTOAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Order order = Factory.New<Order>();
			order.JD_JS = shipment.PK;
			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivingCTOAddress.Address);

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivingCTOAddress.Address);
		}

		public void TestExportReceivalAddress()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment = consol.Shipments.AddNew();
			Order order = Factory.New<Order>();
			order.JD_JS = shipment.PK;
			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Should return AddressWrapper with Null JobDocAddress", ZString.Empty, wrapper.ExportReceivalAddress.Address);

			OrgAddress consolPackDepotAddress = Factory.New<OrgAddress>();
			consolPackDepotAddress.OA_Address1 = "CONSOLBO PACKDEPOTADDRESS 88";
			consolPackDepotAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_PackDepotAddress = consolPackDepotAddress.PK;

			OrgAddress consolDepartureCTOAddress = Factory.New<OrgAddress>();
			consolDepartureCTOAddress.OA_Address1 = "CONSOLBO DEPARTURECTOADDRESS 77";
			consolDepartureCTOAddress.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_DepartureCTOAddress = consolDepartureCTOAddress.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.PackDepotAddress", "CONSOLBO PACKDEPOTADDRESS 88\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			order.JD_ContainerMode = Constants.ContainerModes.FCL;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Should return AddressWrapper with ConsolBO.DepartureCTOAddress", "CONSOLBO DEPARTURECTOADDRESS 77\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);

			OrgAddress shipmentExportReceivingDepot = Factory.New<OrgAddress>();
			shipmentExportReceivingDepot.OA_Address1 = "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66";
			shipmentExportReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentExportReceivingDepot.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Should return AddressWrapper with ShipmentBO.ExportReceivingDepot", "SHIPMENTBO EXPORTRECEIVINGDEPOTADDRESS 66\nAUSTRALIA", wrapper.ExportReceivalAddress.Address);
		}

		public void TestGetPickUpCFSAddressGetsOverridenValues()
		{
			var order = Factory.New<Order>();
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			order.JD_JE = declaration.PK;

			var shipmentExportReceivingDepot = Factory.New<OrgAddress>();
			shipmentExportReceivingDepot.OA_PostCode = "1234";
			shipmentExportReceivingDepot.OA_Address1 = "exportReceivingDepot";
			shipmentExportReceivingDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ExportReceivingDepot = shipmentExportReceivingDepot.PK;

			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Expected depotDocAddress to be empty", declaration.DepotDocAddress.IsEmpty, true);
			AssertEquals("Expected pickupCFSAddress to be same as shipmentExportReceivingDepot address", "1234", wrapper.PickupCFSAddress.PostCode);
			AssertEquals("Expected CountryCode to be same as shipmentExportReceivingDepot Code", "AU", wrapper.PickupCFSAddress.Country.Code);

			var declarationDepotDocAddress = declaration.DepotDocAddress;
			declarationDepotDocAddress.E2_AddressOverride = true;
			declarationDepotDocAddress.E2_Postcode = "5678";
			declarationDepotDocAddress.E2_RN_NKCountryCode = "HK";

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Expected depotDocAddress to be not empty", declaration.DepotDocAddress.IsEmpty, false);
			AssertEquals("Expected pickupCFS Address to be overridden", "5678", wrapper.PickupCFSAddress.PostCode);
			AssertEquals("Expected CountryCode to be overriden", "HK", wrapper.PickupCFSAddress.Country.Code);
		}

		public void TestGetUnpackCFSAddressGetsOverridenValues()
		{
			var order = Factory.New<Order>();
			var declaration = Factory.New<BaseJobDeclaration>();
			var shipment = Factory.New<ForwardingShipment>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_JS = shipment.PK;
			order.JD_JE = declaration.PK;

			var shipmentImportReleaseDepot = Factory.New<OrgAddress>();
			shipmentImportReleaseDepot.OA_PostCode = "1234";
			shipmentImportReleaseDepot.OA_RN_NKCountryCode = "AU";
			shipment.JS_OA_ImportReleaseDepot = shipmentImportReleaseDepot.PK;

			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Expected depotDocAddress to be empty", declaration.DepotDocAddress.IsEmpty, true);
			AssertEquals("Expected unpackCFSAddress to be same as shipmentImportReleaseDepot address", "1234", wrapper.UnpackCFSAddress.PostCode);
			AssertEquals("Expected CountryCode to be same as shipmentImportReleaseDepot Code", "AU", wrapper.UnpackCFSAddress.Country.Code);

			var declarationDepotDocAddress = declaration.DepotDocAddress;
			declarationDepotDocAddress.E2_AddressOverride = true;
			declarationDepotDocAddress.E2_Postcode = "5678";
			declarationDepotDocAddress.E2_RN_NKCountryCode = "HK";

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Expected depotDocAddress to be not empty", declaration.DepotDocAddress.IsEmpty, false);
			AssertEquals("Expected unpackCFSAddress to be overridden", "5678", wrapper.UnpackCFSAddress.PostCode);
			AssertEquals("Expected CountryCode to be overridden", "HK", wrapper.UnpackCFSAddress.Country.Code);
		}

		public override void TestContainerLayoutStyle()
		{
			Assert(true); // containers on an Order Header are a different type of object and do not have a container layout style
		}

		public void TestBillHeadings()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			Order order1 = Factory.New<Order>();
			Order order2 = Factory.New<Order>();
			Order order3 = Factory.New<Order>();

			FreightWrapper wrapper1 = FreightWrapper.New(order1, Factory)[0];
			FreightWrapper wrapper2 = FreightWrapper.New(order2, Factory)[0];
			FreightWrapper wrapper3 = FreightWrapper.New(order3, Factory)[0];

			AssertEquals("wrapper1.HouseBillHeading", "House Bill", wrapper1.HouseBillHeading);
			AssertEquals("wrapper2.HouseBillHeading", "House Bill", wrapper2.HouseBillHeading);
			AssertEquals("wrapper3.HouseBillHeading", "House Bill", wrapper3.HouseBillHeading);

			AssertEquals("wrapper1.MasterBillHeading", "Master Bill", wrapper1.MasterBillHeading);
			AssertEquals("wrapper2.MasterBillHeading", "Master Bill", wrapper2.MasterBillHeading);
			AssertEquals("wrapper3.MasterBillHeading", "Master Bill", wrapper3.MasterBillHeading);

			order1.JD_JS = shipment.PK;
			order2.JD_JS = shipment.PK;
			order3.JD_JS = shipment.PK;

			order1.JD_TransportMode = Core.Constants.TransportModes.Air;
			order2.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order3.JD_TransportMode = Core.Constants.TransportModes.Rail;

			wrapper1 = FreightWrapper.New(order1, Factory)[0];
			wrapper2 = FreightWrapper.New(order2, Factory)[0];
			wrapper3 = FreightWrapper.New(order3, Factory)[0];

			AssertEquals("wrapper1.HouseBillHeading", "HAWB", wrapper1.HouseBillHeading);
			AssertEquals("wrapper2.HouseBillHeading", "House Bill Of Lading", wrapper2.HouseBillHeading);
			AssertEquals("wrapper3.HouseBillHeading", "House Bill", wrapper3.HouseBillHeading);

			AssertEquals("wrapper1.MasterBillHeading", "MAWB", wrapper1.MasterBillHeading);
			AssertEquals("wrapper2.MasterBillHeading", "Ocean Bill Of Lading", wrapper2.MasterBillHeading);
			AssertEquals("wrapper3.MasterBillHeading", "Master Bill", wrapper3.MasterBillHeading);
		}

		public void TestOrderTransportMode()
		{
			Order order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			FreightWrapper wrapper = FreightWrapper.New(order, Factory)[0];

			AssertEquals("wrapper.OrderTransportMode", "SEA", wrapper.OrderTransportMode.Code);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = Core.Constants.TransportModes.Road;
			order.JD_JE = declaration.PK;
			wrapper = FreightWrapper.New(order, Factory)[0];

			AssertEquals("wrapper.OrderTransportMode", "ROA", wrapper.OrderTransportMode.Code);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Rail;
			order.JD_JS = shipment.PK;
			wrapper = FreightWrapper.New(order, Factory)[0];

			AssertEquals("wrapper.OrderTransportMode", "RAI", wrapper.OrderTransportMode.Code);
		}

		public override void TestInsuranceRelatedAddressFields()
		{
			Order order = Factory.New<Order>();
			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			declaration.JE_JS = shipment.PK;
			order.JD_JE = declaration.PK;

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY SHIPMENT";

			JobDocAddress shipmentAssuredParty = shipment.AssuredPartyDocAddress;
			shipmentAssuredParty.E2_AddressOverride = true;
			shipmentAssuredParty.E2_CompanyName = "ASSUREDPARTY SHIPMENT";

			JobDocAddress shipmentClaimsPayableBy = shipment.ClaimsPayableByDocAddress;
			shipmentClaimsPayableBy.E2_AddressOverride = true;
			shipmentClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY SHIPMENT";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY SHIPMENT";

			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.InsuredBy.CompanyName", "INSUREDBY SHIPMENT", wrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", "ASSUREDPARTY SHIPMENT", wrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", "CLAIMSPAYABLEBY SHIPMENT", wrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", "SURVEYREPORTPARTY SHIPMENT", wrapper.SurveyReportParty.CompanyName);

			JobDocAddress declarationInsuredBy = declaration.InsuredByDocAddress;
			declarationInsuredBy.E2_AddressOverride = true;
			declarationInsuredBy.E2_CompanyName = "INSUREDBY DECLARATION";

			JobDocAddress declarationAssuredParty = declaration.AssuredPartyDocAddress;
			declarationAssuredParty.E2_AddressOverride = true;
			declarationAssuredParty.E2_CompanyName = "ASSUREDPARTY DECLARATION";

			JobDocAddress declarationClaimsPayableBy = declaration.ClaimsPayableByDocAddress;
			declarationClaimsPayableBy.E2_AddressOverride = true;
			declarationClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY DECLARATION";

			JobDocAddress declarationSurveyReportParty = declaration.SurveyReportPartyDocAddress;
			declarationSurveyReportParty.E2_AddressOverride = true;
			declarationSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY DECLARATION";

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.InsuredBy.CompanyName", "INSUREDBY DECLARATION", wrapper.InsuredBy.CompanyName);
			AssertEquals("wrapper.AssuredParty.CompanyName", "ASSUREDPARTY DECLARATION", wrapper.AssuredParty.CompanyName);
			AssertEquals("wrapper.ClaimsPayableBy.CompanyName", "CLAIMSPAYABLEBY DECLARATION", wrapper.ClaimsPayableBy.CompanyName);
			AssertEquals("wrapper.SurveyReportParty.CompanyName", "SURVEYREPORTPARTY DECLARATION", wrapper.SurveyReportParty.CompanyName);
		}

		public override void TestBuyer()
		{
			Order order = Factory.New<Order>();
			FreightWrapperFromOrder wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", ZString.Empty, wrapper.Buyer.CompanyNameAndAddress);

			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "All I need for the test";
			buyer.MainAddress.OA_Address1 = "ADDRESS 1 STUFF";
			buyer.MainAddress.OA_PostCode = "DARWIN";
			buyer.MainAddress.OA_RN_NKCountryCode = "AU";

			order.BuyerPK = buyer.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.Buyer.CompanyNameAndAddress", "ALL I NEED FOR THE TEST\nADDRESS 1 STUFF\nDARWIN\nAUSTRALIA", wrapper.Buyer.CompanyNameAndAddress);
		}

		public override void TestRecommendedAgent()
		{
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_FullName = "SENDING AGENT COMPANY";
			sendingAgent.OH_RL_NKClosestPort = "AUSYD";

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "SUPPLIER COMPANY";
			supplier.OH_RL_NKClosestPort = "INAMD";

			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "BUYER COMPANY";
			buyer.OH_RL_NKClosestPort = "AUSYD";

			var portOfLoadingAgent = Factory.NewWithValidTestData<OrgHeader>();
			portOfLoadingAgent.OH_FullName = "PORT OF LOADING AGENT COMPANY";
			portOfLoadingAgent.OH_RL_NKClosestPort = "AUMEL";
			portOfLoadingAgent.OH_IsForwarder = true;

			var portOfLoadingAgentPortMEL = portOfLoadingAgent.AppointedAgentPorts.AddNew();
			portOfLoadingAgentPortMEL.O5_PortOrCountry = "AUMEL";
			portOfLoadingAgentPortMEL.O5_SeaAgentStatus = "PUB";
			portOfLoadingAgentPortMEL.O5_OA_AgentOfficeAddress = portOfLoadingAgent.MainAddress.PK;

			var supplierClosestPortAgent = Factory.NewWithValidTestData<OrgHeader>();
			supplierClosestPortAgent.OH_FullName = "PORT OF LOADING AGENT COMPANY";
			supplierClosestPortAgent.OH_RL_NKClosestPort = "AUMEL";
			supplierClosestPortAgent.OH_IsForwarder = true;

			var supplierClosestPortAgentPortAMD = supplierClosestPortAgent.AppointedAgentPorts.AddNew();
			supplierClosestPortAgentPortAMD.O5_PortOrCountry = "INAMD";
			supplierClosestPortAgentPortAMD.O5_SeaAgentStatus = "PUB";
			supplierClosestPortAgentPortAMD.O5_OA_AgentOfficeAddress = supplierClosestPortAgent.MainAddress.PK;

			Factory.Save();

			var order = Factory.New<Order>();
			order.JD_TransportMode = "SEA";

			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.RecommendedAgent.CompanyName", ZString.Empty, wrapper.RecommendedAgent.CompanyName);
			AssertEquals("wrapper.RecommendedAgent.ContactName", ContactType.Sales.DefaultName, wrapper.RecommendedAgent.ContactName);
			AssertEquals("wrapper.RecommendedAgent.ContactPhone", "", wrapper.RecommendedAgent.ContactPhone);
			AssertEquals("wrapper.RecommendedAgent.ContactFax", "", wrapper.RecommendedAgent.ContactFax);

			order.SupplierPK = supplier.PK;
			order.JD_RL_NKPortOfLoading = "";
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.RecommendedAgent.CompanyName", supplierClosestPortAgent.OH_FullName, wrapper.RecommendedAgent.CompanyName);
			AssertEquals("wrapper.RecommendedAgent.ContactName", ContactType.Sales.DefaultName, wrapper.RecommendedAgent.ContactName);
			AssertEquals("wrapper.RecommendedAgent.ContactPhone", supplierClosestPortAgent.MainAddress.OA_Phone_Formatted, wrapper.RecommendedAgent.ContactPhone);
			AssertEquals("wrapper.RecommendedAgent.ContactFax", supplierClosestPortAgent.MainAddress.OA_Fax_Formatted, wrapper.RecommendedAgent.ContactFax);

			order.JD_RL_NKPortOfLoading = "AUMEL";
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.RecommendedAgent.CompanyName", portOfLoadingAgent.OH_FullName, wrapper.RecommendedAgent.CompanyName);
			AssertEquals("wrapper.RecommendedAgent.ContactName", ContactType.Sales.DefaultName, wrapper.RecommendedAgent.ContactName);
			AssertEquals("wrapper.RecommendedAgent.ContactPhone", portOfLoadingAgent.MainAddress.OA_Phone_Formatted, wrapper.RecommendedAgent.ContactPhone);
			AssertEquals("wrapper.RecommendedAgent.ContactFax", portOfLoadingAgent.MainAddress.OA_Fax_Formatted, wrapper.RecommendedAgent.ContactFax);

			order.JD_OH_SendingAgent = sendingAgent.PK;
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.RecommendedAgent.CompanyName", sendingAgent.OH_FullName, wrapper.RecommendedAgent.CompanyName);
			AssertEquals("wrapper.RecommendedAgent.ContactName", ContactType.Sales.DefaultName, wrapper.RecommendedAgent.ContactName);
			AssertEquals("wrapper.RecommendedAgent.ContactPhone", sendingAgent.MainAddress.OA_Phone_Formatted, wrapper.RecommendedAgent.ContactPhone);
			AssertEquals("wrapper.RecommendedAgent.ContactFax", sendingAgent.MainAddress.OA_Fax_Formatted, wrapper.RecommendedAgent.ContactFax);

			var consolSendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			consolSendingAgent.OH_FullName = "CONSOL SENDING AGENT COMPANY";

			var shipment = Factory.New<ForwardingShipment>();
			var consol = shipment.Consols.AddNew();
			consol.JK_OA_SendingForwarderAddress = consolSendingAgent.MainAddress.PK;
			order.JD_JS = shipment.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.RecommendedAgent.CompanyName", consolSendingAgent.OH_FullName, wrapper.RecommendedAgent.CompanyName);
			AssertEquals("wrapper.RecommendedAgent.ContactName", ContactType.Sales.DefaultName, wrapper.RecommendedAgent.ContactName);
			AssertEquals("wrapper.RecommendedAgent.ContactPhone", consolSendingAgent.MainAddress.OA_Phone_Formatted, wrapper.RecommendedAgent.ContactPhone);
			AssertEquals("wrapper.RecommendedAgent.ContactFax", consolSendingAgent.MainAddress.OA_Fax_Formatted, wrapper.RecommendedAgent.ContactFax);
		}

		public void TestRecommendedAgent_HasSalesContact()
		{
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_FullName = "SENDING AGENT COMPANY";
			sendingAgent.OH_RL_NKClosestPort = "AUSYD";
			var address = sendingAgent.Addresses[0];
			address.OA_Phone = "+27 11 012 8700";
			address.OA_Fax = "+27 11 012 8701";

			var saleContact = sendingAgent.Contacts.AddNew();
			saleContact.OC_ContactName = "Sale Name";
			saleContact.OC_Phone = "+27 11 111 111";
			saleContact.OC_Fax = "+27 11 111 112";
			var contactDoc = saleContact.Documents.AddNew();
			contactDoc.OD_DocumentGroup = ContactType.Sales.Code;
			contactDoc.OD_DefaultContact = true;

			Factory.Save();

			var order = Factory.New<Order>();
			order.JD_TransportMode = "SEA";
			order.JD_OH_SendingAgent = sendingAgent.PK;

			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("wrapper.RecommendedAgent.ContactName", saleContact.OC_ContactName, wrapper.RecommendedAgent.ContactName);
			AssertEquals("wrapper.RecommendedAgent.ContactPhone", saleContact.OC_Phone, wrapper.RecommendedAgent.ContactPhone);
			AssertEquals("wrapper.RecommendedAgent.ContactFax", saleContact.OC_Fax, wrapper.RecommendedAgent.ContactFax);
		}

		public void TestRecommendedAgent_HasNoSalesContact()
		{
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			sendingAgent.OH_FullName = "SENDING AGENT COMPANY";
			sendingAgent.OH_RL_NKClosestPort = "AUSYD";
			var address = sendingAgent.Addresses[0];
			address.OA_Phone = "+27 11 012 8700";
			address.OA_Fax = "+27 11 012 8701";

			Factory.Save();

			var order = Factory.New<Order>();
			order.JD_TransportMode = "SEA";
			order.JD_OH_SendingAgent = sendingAgent.PK;

			var contactNamePlaceholder = "The Sales Manager";
			var wrapper = new FreightWrapperFromOrder(order, Factory);

			AssertEquals("sendingAgent.Contacts.Count()", 0, sendingAgent.Contacts.Count);
			AssertEquals("wrapper.RecommendedAgent.ContactName", contactNamePlaceholder, wrapper.RecommendedAgent.ContactName);
			AssertEquals("wrapper.RecommendedAgent.ContactPhone", address.OA_Phone, wrapper.RecommendedAgent.ContactPhone);
			AssertEquals("wrapper.RecommendedAgent.ContactFax", address.OA_Fax, wrapper.RecommendedAgent.ContactFax);
		}

		public override void TestWrapperNotes()
		{
			Order order = Factory.New<Order>();
			order.Notes.AddNew(false, PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description, "SOME DANGEROUS GOODS TEST HANDLING INFORMATION FOR ORDER");
			order.Notes.AddNew(false, PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description, "NOT SURE WHAT A CERTIFICATE OF ORIGIN NOTE SHOULD LOOK LIKE SO HERE IS A TEST");
			order.Notes.AddNew(false, PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description, "I DONT THINK THAT THE ORDER HAS A PRE ALERT ARRIVAL NOTICE SO THIS TEST IS PRETTY USELESS BUT SHOULD PASS");

			FreightWrapperFromOrder noteWrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("noteWrapper.DangerousGoodsAdditionalHandlingInformation", "SOME DANGEROUS GOODS TEST HANDLING INFORMATION FOR ORDER", noteWrapper.Notes[PredefinedNoteTypes.Instance.DangerousGoodsAdditionalHandlingInformation.Description].Text);
			AssertEquals("noteWrapper.CertificateOfOriginNotes", "NOT SURE WHAT A CERTIFICATE OF ORIGIN NOTE SHOULD LOOK LIKE SO HERE IS A TEST", noteWrapper.Notes[PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description].Text);
			AssertEquals("noteWrapper.PreAlertArrivalNoticeRemarks", "I DONT THINK THAT THE ORDER HAS A PRE ALERT ARRIVAL NOTICE SO THIS TEST IS PRETTY USELESS BUT SHOULD PASS", noteWrapper.Notes[PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description].Text);
		}

		public void TestWrapperMappingFullFromOrder()
		{
			#region Setup

			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "TEST CARRIER FOR ORDER";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "TEST SUPPLIER FOR ORDER";
			supplier.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "TEST BUYER FOR ORDER";
			buyer.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_FullName = "RECEIVING AGENT FOR ORDER";
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_FullName = "SENDING AGENT FOR ORDER";
			sendingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "CONSIGNEE FOR ORDER";
			consignee.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "TEST NOTIFY PARTY FOR ORDER";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress deliveryAddress = buyer.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "THE FIRST PART OF THE DELIVERY ADDRESS";
			deliveryAddress.OA_Address2 = "THE SECOND PART OF THE DELIVERY ADDRESS";
			deliveryAddress.OA_City = "WHO CARES";
			deliveryAddress.OA_PostCode = "*#&$(*@#";
			deliveryAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress pickupAddress = supplier.Addresses.AddNew();
			pickupAddress.OA_Address1 = "THE FIRST PART OF THE PICKUP ADDRESS";
			pickupAddress.OA_Address2 = "THE SECOND PART OF THE PICKUP ADDRESS";
			pickupAddress.OA_City = "SCOTTLAND";
			pickupAddress.OA_PostCode = "666";
			pickupAddress.OA_RN_NKCountryCode = "AU";

			Order order = Factory.New<Order>();
			order.JD_OrderStatus = "SHP";
			order.JD_ContainerMode = "FCL";
			order.JD_TransportMode = "SEA";
			order.JD_RS_NKServiceLevel_NI = "D2D";
			order.JD_IncoTerm = "FOB";
			order.JD_AdditionalTerms = "Requires cookies and cream";
			order.JD_OrderNumber = "00324324";
			order.JD_OrderNumberSplit = 3;
			order.JD_MasterWaybill = "MBL1237897";
			order.JD_OH_Carrier = carrier.PK;
			order.SupplierPK = supplier.PK;
			order.BuyerPK = buyer.PK;
			order.NotifyPartyDocAddress.OrganisationPK = notifyParty.PK;
			order.GoodsDeliveredToAddress.E2_OA_Address = deliveryAddress.PK;
			order.GoodsAvailableAtAddress.E2_OA_Address = pickupAddress.PK;
			order.JD_OH_SendingAgent = sendingAgent.PK;
			order.JD_RL_NKPortOfLoading = "AUSYD";
			order.UpdateEventEstimate(Events.Departure, new ZDateTimeOffset(2007, 4, 23));
			order.UpdateEvent(Events.Departure, new ZDateTimeOffset(2007, 4, 25));
			order.JD_RL_NKPortOfDischarge = "NZAKL";
			order.UpdateEventEstimate(Events.Arrival, new ZDateTimeOffset(2007, 5, 10));
			order.UpdateEvent(Events.Arrival, new ZDateTimeOffset(2007, 5, 13));
			order.JD_RL_NKGoodsAvailableAt = "AUMEL";
			order.JD_RL_NKGoodsDeliveredTo = "NZQQQ";
			order.JD_OH_ReceivingAgent = receivingAgent.PK;
			order.JD_Packs = 12;
			order.JD_F3_NKPackType = "BAG";
			order.JD_ActualWeight = 16.78m;
			order.JD_UnitOfWeight = "M3";
			order.JD_ActualVolume = 123.458m;
			order.JD_UnitOfVolume = "OT";
			order.JD_RX_NKOrderCurrency = "AUD";
			order.JD_OrderGoodsDescription = "THESE ARE FAKE EXPLODING GOODS";
			order.JD_Waybill = "HBL32432";
			order.UpdateEvent(Events.GateIn, new ZDateTimeOffset(2006, 12, 12));
			order.JD_BookingConfRef = "BOOK358923";
			order.UpdateEventEstimate(Events.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2008, 3, 28));
			order.UpdateEvent(Events.DeliveryCartageCompleteFinalised, new ZDateTimeOffset(2008, 4, 3));
			order.UpdateEventEstimate(Events.DeliveryCartageAdvised, new ZDateTimeOffset(2007, 5, 23));
			order.UpdateEvent(Events.DeliveryCartageAdvised, new ZDateTimeOffset(2007, 5, 25));
			order.UpdateEvent(Events.ExWorks, new ZDateTimeOffset(2011, 1, 4));
			order.JD_CustomAttrib1 = "CUST ATTRIB 1";
			order.JD_CustomAttrib2 = "CUST ATTRIB 2";
			order.JD_CustomDate1 = new ZDateTime(2004, 8, 12);
			order.JD_CustomDate2 = new ZDateTime(2004, 8, 25);
			order.JD_CustomDecimal1 = 12.3m;
			order.JD_CustomDecimal2 = 29.34m;
			order.JD_CustomFlag1 = true;
			order.JD_CustomFlag2 = true;
			order.JD_OrderDate = new ZDateTime(2011, 5, 20);
			order.JD_DeliveryRequiredBy = new ZDateTime(2011, 9, 24);
			order.JD_ExWorksRequiredBy = new ZDateTime(2011, 12, 25);

			order.JD_RV_NKDepartureVessel = "DEPARTURE VESSEL NAME";
			order.JD_DepartureVoyage = "S987";
			order.JD_Milestone_E_DEP = new ZDateTime(2008, 6, 25);
			order.JD_E_ARV_1stIntermediate = new ZDateTime(2008, 7, 1);
			order.JD_RV_NKIntermediateVessel = "INTERMEDIATE VESSEL NAME";
			order.JD_IntermediateVoyage = "N34";
			order.JD_E_DEP_2 = new ZDateTime(2008, 7, 8);
			order.JD_E_ARV_2ndIntermediate = new ZDateTime(2008, 7, 14);
			order.JD_RV_NKArrivalVessel = "ARRIVAL VESSEL NAME";
			order.JD_ArrivalVoyage = "E98";
			order.JD_E_DEP_3 = new ZDateTime(2008, 7, 23);
			order.JD_Milestone_E_ARV = new ZDateTime(2008, 7, 28);

			OrderLine orderline = order.OrderLines.AddNew();
			orderline.JO_LinePrice = 123.45m;

			order.PlannedContainers.AddNew();

			order.RequiredDocuments.AddNew();

			#endregion

			FreightWrapperFromOrder fullWrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("fullWrapper.ShipmentStatus.Description", "Shipped", fullWrapper.ShipmentStatus.Description);
			AssertEquals("fullWrapper.ShipmentContainerMode.Description", "Full Container Load", fullWrapper.ShipmentContainerMode.Description);
			AssertEquals("fullWrapper.ShipmentTransportMode.Description", "Sea Freight", fullWrapper.ShipmentTransportMode.Description);
			AssertEquals("fullWrapper.OrderTransportMode.Description", "Sea Freight", fullWrapper.OrderTransportMode.Description);
			AssertEquals("fullWrapper.ServiceLevel.Description", "Door to Door", fullWrapper.ServiceLevel.Description);
			AssertEquals("fullWrapper.IncoTerm.Description", "Free On Board", fullWrapper.IncoTerm.Description);
			AssertEquals("fullWrapper.IncoTerm.PaymentType", "CCX - Collect", fullWrapper.IncoTerm.PaymentType.CodeAndDescription);
			AssertEquals("fullWrapper.AdditionalTerms", "Requires cookies and cream", fullWrapper.AdditionalTerms);
			AssertEquals("fullWrapper.ExportAgentsReference", "00324324", fullWrapper.ExportAgentsReference);
			AssertEquals("fullWrapper.ImportAgentsReference", "00324324", fullWrapper.ImportAgentsReference);
			AssertEquals("fullWrapper.LocalForwarderReference", "00324324", fullWrapper.LocalForwarderReference);
			AssertEquals("fullWrapper.OrderNumbersWithOwnersReference", "00324324", fullWrapper.OrderNumbersWithOwnersReference);
			AssertEquals("fullWrapper.MasterBill", "MBL1237897", fullWrapper.MasterBill);
			AssertEquals("fullWrapper.MasterBillHeading", "Master Bill", fullWrapper.MasterBillHeading);
			AssertEquals("fullWrapper.HouseBillHeading", "House Bill", fullWrapper.HouseBillHeading);
			AssertEquals("fullWrapper.Carrier.CompanyName", "TEST CARRIER FOR ORDER", fullWrapper.Carrier.CompanyName);
			AssertEquals("fullWrapper.Consignor.CompanyName", "TEST SUPPLIER FOR ORDER", fullWrapper.Consignor.CompanyName);
			AssertEquals("fullWrapper.Consignee.CompanyName", "TEST BUYER FOR ORDER", fullWrapper.Consignee.CompanyName);
			AssertEquals("fullWrapper.Buyer.CompanyName", "TEST BUYER FOR ORDER", fullWrapper.Buyer.CompanyName);
			AssertEquals("fullWrapper.DeliveryAgent.CompanyName", "RECEIVING AGENT FOR ORDER", fullWrapper.DeliveryAgent.CompanyName);
			AssertEquals("fullWrapper.DeliveryAddress.Address", "THE FIRST PART OF THE DELIVERY ADDRESS\nTHE SECOND PART OF THE DELIVERY ADDRESS\nWHO CARES *#&$(*@#\nAUSTRALIA", fullWrapper.DeliveryAddress.Address);
			AssertEquals("fullWrapper.PickupAddress.Address", "THE FIRST PART OF THE PICKUP ADDRESS\nTHE SECOND PART OF THE PICKUP ADDRESS\nSCOTTLAND 666\nAUSTRALIA", fullWrapper.PickupAddress.Address);
			AssertEquals("fullWrapper.NotifyPartyDocumentAddress.CompanyName", "TEST NOTIFY PARTY FOR ORDER", fullWrapper.NotifyParty.CompanyName);
			AssertEquals("fullWrapper.BookingParty.CompanyName", ZString.Empty, fullWrapper.BookingParty.CompanyName);
			AssertEquals("fullWrapper.PickupAgent.CompanyName", "SENDING AGENT FOR ORDER", fullWrapper.PickupAgent.CompanyName);
			AssertEquals("fullWrapper.ShipmentRoutes[0].Origin.UNLOCO", "AUSYD", fullWrapper.ShipmentRoutes[0].Origin.UNLOCO);
			AssertEquals("fullWrapper.ShipmentRoutes[0].Destination.UNLOCO", ZString.Empty, fullWrapper.ShipmentRoutes[0].Destination.UNLOCO);
			AssertEquals("fullWrapper.ShipmentRoutes[0].Transport.VesselName", "DEPARTURE VESSEL NAME", fullWrapper.ShipmentRoutes[0].Transport.VesselName);
			AssertEquals("fullWrapper.ShipmentRoutes[0].Transport.VoyageNo", "S987", fullWrapper.ShipmentRoutes[0].Transport.VoyageNo);
			AssertEquals("fullWrapper.ShipmentRoutes[0].EstimatedDeparture", new ZDateTime(2008, 6, 25), fullWrapper.ShipmentRoutes[0].EstimatedDeparture);
			AssertEquals("fullWrapper.ShipmentRoutes[0].EstimatedArrival", new ZDateTime(2008, 7, 1), fullWrapper.ShipmentRoutes[0].EstimatedArrival);
			AssertEquals("fullWrapper.ShipmentRoutes[1].Origin.UNLOCO", ZString.Empty, fullWrapper.ShipmentRoutes[1].Origin.UNLOCO);
			AssertEquals("fullWrapper.ShipmentRoutes[1].Destination.UNLOCO", ZString.Empty, fullWrapper.ShipmentRoutes[1].Destination.UNLOCO);
			AssertEquals("fullWrapper.ShipmentRoutes[1].Transport.VesselName", "INTERMEDIATE VESSEL NAME", fullWrapper.ShipmentRoutes[1].Transport.VesselName);
			AssertEquals("fullWrapper.ShipmentRoutes[1].Transport.VoyageNo", "N34", fullWrapper.ShipmentRoutes[1].Transport.VoyageNo);
			AssertEquals("fullWrapper.ShipmentRoutes[1].EstimatedDeparture", new ZDateTime(2008, 7, 8), fullWrapper.ShipmentRoutes[1].EstimatedDeparture);
			AssertEquals("fullWrapper.ShipmentRoutes[1].EstimatedArrival", new ZDateTime(2008, 7, 14), fullWrapper.ShipmentRoutes[1].EstimatedArrival);
			AssertEquals("fullWrapper.ShipmentRoutes[2].Origin.UNLOCO", ZString.Empty, fullWrapper.ShipmentRoutes[2].Origin.UNLOCO);
			AssertEquals("fullWrapper.ShipmentRoutes[2].Destination.UNLOCO", "NZAKL", fullWrapper.ShipmentRoutes[2].Destination.UNLOCO);
			AssertEquals("fullWrapper.ShipmentRoutes[2].Transport.VesselName", "ARRIVAL VESSEL NAME", fullWrapper.ShipmentRoutes[2].Transport.VesselName);
			AssertEquals("fullWrapper.ShipmentRoutes[2].Transport.VoyageNo", "E98", fullWrapper.ShipmentRoutes[2].Transport.VoyageNo);
			AssertEquals("fullWrapper.ShipmentRoutes[2].EstimatedDeparture", new ZDateTime(2008, 7, 23), fullWrapper.ShipmentRoutes[2].EstimatedDeparture);
			AssertEquals("fullWrapper.ShipmentRoutes[2].EstimatedArrival", new ZDateTime(2008, 7, 28), fullWrapper.ShipmentRoutes[2].EstimatedArrival);
			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "AUMEL", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "NZQQQ", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero", "12 BAG", fullWrapper.ShipmentOuterPacksQty.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.Weight.ValueAndUnitCodeBlankIfZero", "16.780 M3", fullWrapper.Weight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.Volume.ValueAndUnitCodeBlankIfZero", "123.458 OT", fullWrapper.Volume.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.GoodsValue.AmountAndCurrencyCode", "123.45 AUD", fullWrapper.GoodsValue.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.GoodsDescription", "THESE ARE FAKE EXPLODING GOODS", fullWrapper.GoodsDescription);
			AssertEquals("fullWrapper.HouseBill", "HBL32432", fullWrapper.HouseBill);
			AssertEquals("fullWrapper.ActualReceive", new ZDateTime(2006, 12, 12), fullWrapper.ActualReceive);
			AssertEquals("fullWrapper.ShippersReference", "BOOK358923", fullWrapper.ShippersReference);
			AssertEquals("fullWrapper.JobNumberHeading", "Order", fullWrapper.JobNumberHeading);
			AssertEquals("fullWrapper.JobNumber", "00324324-3", fullWrapper.JobNumber);
			AssertEquals("fullWrapper.DeliveryFrom", new ZDateTime(2008, 3, 28), fullWrapper.DeliveryFrom);
			AssertEquals("fullWrapper.DeliveryGoodsDelivered", new ZDateTime(2008, 4, 3), fullWrapper.DeliveryGoodsDelivered);
			AssertEquals("fullWrapper.PickupCartageAdvised", new ZDateTime(2007, 5, 23), fullWrapper.PickupCartageAdvised);
			AssertEquals("fullWrapper.PickupGoodsPickedup", new ZDateTime(2007, 5, 25), fullWrapper.PickupGoodsPickedup);
			AssertEquals("fullWrapper.CustomAttribute1", "CUST ATTRIB 1", fullWrapper.CustomAttribute1);
			AssertEquals("fullWrapper.CustomAttribute2", "CUST ATTRIB 2", fullWrapper.CustomAttribute2);
			AssertEquals("fullWrapper.CustomDate1", new ZDateTime(2004, 8, 12), fullWrapper.CustomDate1);
			AssertEquals("fullWrapper.CustomDate2", new ZDateTime(2004, 8, 25), fullWrapper.CustomDate2);
			AssertEquals("fullWrapper.CustomDecimal1", 12.3m, fullWrapper.CustomDecimal1);
			AssertEquals("fullWrapper.CustomDecimal2", 29.34m, fullWrapper.CustomDecimal2);
			AssertEquals("fullWrapper.CustomFlag1", true, fullWrapper.CustomFlag1);
			AssertEquals("fullWrapper.CustomFlag2", true, fullWrapper.CustomFlag2);
			AssertEquals("fullWrapper.Containers.Count", 1, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.ExportReceivingDepotAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingDepotAddress.Address);
			AssertEquals("fullWrapper.ExportReceivingCTOAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivingCTOAddress.Address);
			AssertEquals("fullWrapper.ExportReceivalAddress", AddressWrapper.Empty(Factory).Address, fullWrapper.ExportReceivalAddress.Address);
			AssertEquals("fullWrapper.RequiredDocuments.Count", 1, fullWrapper.RequiredDocuments.Count);
			AssertEquals("fullWrapper.Services.Count", 0, fullWrapper.Services.Count);
			AssertEquals("fullWrapper.PickupDeliveryConfirmations.Count", 0, fullWrapper.PickupDeliveryConfirmations.Count);
			AssertEquals("fullWrapper.PickupLocation", "AUMEL", fullWrapper.PickupLocation.UNLOCO);
			AssertEquals("fullWrapper.DeliveryLocation", "NZQQQ", fullWrapper.DeliveryLocation.UNLOCO);
			AssertEquals("fullWrapper.FreightPayableAt", ZString.Empty, fullWrapper.FreightPayableAt.UNLOCO);
			AssertEquals("fullWrapper.MainShipToParty.CompanyName", ZString.Empty, fullWrapper.MainShipToParty.CompanyName);
			AssertEquals("fullWrapper.SellingParty.CompanyName", ZString.Empty, fullWrapper.SellingParty.CompanyName);
			AssertEquals("fullWrapper.Consolidator.CompanyName", ZString.Empty, fullWrapper.Consolidator.CompanyName);
			AssertEquals("fullWrapper.StuffingLocation.CompanyName", ZString.Empty, fullWrapper.StuffingLocation.CompanyName);
			AssertEquals("fullWrapper.OrderDate", new ZDateTime(2011, 5, 20), fullWrapper.OrderDate);
			AssertEquals("fullWrapper.FactoryEx", new ZDateTime(2011, 1, 4), fullWrapper.FactoryEx);
			AssertEquals("fullWrapper.DeliveryRequiredBy", new ZDateTime(2011, 9, 24), fullWrapper.DeliveryRequiredBy);
			AssertEquals("fullWrapper.ExWorksRequiredBy", new ZDateTime(2011, 12, 25), fullWrapper.ExWorksRequiredBy);
		}

		#region GetReceivingForwarder

		[SetOrgAllowMixedCase(true)]
		public void TestGetReceivingForwarder_DeclarationNotExists_ReturnConsolReceivingForwarder()
		{
			var order = CreateOrder(consolReceivingForwarderName: "McLaren", declarationForwarderName: string.Empty);

			var wrapperToTest = new FreightWrapperFromOrder(order, Factory);

			AssertEquals("Consol's receiving forwarder is used as receiving forwarder", "McLaren", wrapperToTest.ReceivingForwarder.CompanyName);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGetReceivingForwarder_DeclarationIsExport_ReturnConsolReceivingForwarder()
		{
			var order = CreateOrder(consolReceivingForwarderName: "McLaren", isExportDeclaration: true);

			var wrapperToTest = new FreightWrapperFromOrder(order, Factory);

			AssertEquals("Consol's receiving forwarder is used as receiving forwarder", "McLaren", wrapperToTest.ReceivingForwarder.CompanyName);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGetReceivingForwarder_DeclarationIsImport_ReturnDeclarationForwarder()
		{
			var order = CreateOrder(declarationForwarderName: "McLaren", isExportDeclaration: false);

			var wrapperToTest = new FreightWrapperFromOrder(order, Factory);

			AssertEquals("Declaration's forwarder is used as receiving forwarder", "McLaren", wrapperToTest.ReceivingForwarder.CompanyName);
		}

		#endregion

		#region GetSendingForwarder

		[SetOrgAllowMixedCase(true)]
		public void TestGetSendingForwarder_DeclarationNotExists_ReturnConsolSendingForwarder()
		{
			var order = CreateOrder(consolSendingForwarderName: "McLaren", declarationForwarderName: string.Empty);

			var wrapperToTest = new FreightWrapperFromOrder(order, Factory);

			AssertEquals("Consol's sending forwarder is used as sending forwarder", "McLaren", wrapperToTest.SendingForwarder.CompanyName);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGetSendingForwarder_DeclarationIsImport_ReturnConsolSendingForwarder()
		{
			var order = CreateOrder(consolSendingForwarderName: "McLaren", isExportDeclaration: false);

			var wrapperToTest = new FreightWrapperFromOrder(order, Factory);

			AssertEquals("Consol's sending forwarder is used as sending forwarder", "McLaren", wrapperToTest.SendingForwarder.CompanyName);
		}

		[SetOrgAllowMixedCase(true)]
		public void TestGetSendingForwarder_DeclarationIsExport_ReturnDeclarationForwarder()
		{
			var order = CreateOrder(declarationForwarderName: "McLaren", isExportDeclaration: true);

			var wrapperToTest = new FreightWrapperFromOrder(order, Factory);

			AssertEquals("Declaration's forwarder is used as sending forwarder", "McLaren", wrapperToTest.SendingForwarder.CompanyName);
		}

		#endregion

		public void TestNotifyParty()
		{
			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "NOTIFY PARTY ORDER";

			var declaration = Factory.New<BaseJobDeclaration>();
			var declarationNotify = declaration.NotifyPartyDocumentaryAddress;
			declarationNotify.E2_AddressOverride = true;
			declarationNotify.E2_CompanyName = "NOTIFY PARTY DECLARATION";

			var shipment = Factory.New<ForwardingShipment>();
			var shipmentNotify = shipment.NotifyPartyDocumentaryAddress;
			shipmentNotify.E2_AddressOverride = true;
			shipmentNotify.E2_CompanyName = "NOTIFY PARTY SHIPMENT";

			var order = Factory.New<Order>();
			declaration.JE_JS = shipment.PK;
			order.JD_JE = declaration.PK;
			order.JD_JS = shipment.PK;
			order.NotifyPartyDocAddress.OrganisationPK = notifyParty.PK;
			var wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Order's NotifyParty is used", "NOTIFY PARTY ORDER", wrapper.NotifyParty.CompanyName);

			order.NotifyPartyDocAddress.Delete();
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Declaration's NotifyParty is used", "NOTIFY PARTY DECLARATION", wrapper.NotifyParty.CompanyName);

			declaration.NotifyPartyDocumentaryAddress.Delete();
			wrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("Shipment's NotifyParty is used", "NOTIFY PARTY SHIPMENT", wrapper.NotifyParty.CompanyName);
		}

		[TestDate(2007, 1, 1)]
		public void TestFallbackToDeclaration()
		{
			OrgHeader forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = "TEST FORWARDER";
			forwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader exportBroker = Factory.New<OrgHeader>();
			exportBroker.OH_FullName = "EXPORT BROKER PROXY";
			exportBroker.MainAddress.OA_RN_NKCountryCode = "AU";

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			JobDocAddress declarationConsignee = declaration.ImporterDocumentaryAddress;
			declarationConsignee.E2_AddressOverride = true;
			declarationConsignee.E2_CompanyName = "CONSIGNEE DECLARATION";

			JobDocAddress declarationNotify = declaration.NotifyPartyDocumentaryAddress;
			declarationNotify.E2_AddressOverride = true;
			declarationNotify.E2_CompanyName = "NOTIFY DECLARATION";

			OrgAddress depotAddress = Factory.New<OrgAddress>();
			depotAddress.OA_Address1 = "TEST 1 ADDRESS";
			depotAddress.OA_RN_NKCountryCode = "AU";

			declaration.JE_ContainerMode = Constants.ContainerModes.FCL;
			declaration.JE_OH_Forwarder = forwarder.PK;
			declaration.Branch.GB_OH_OrgProxy = exportBroker.PK;
			declaration.JE_MarksAndNumbers = "SOME MARKS AND NUMBERS FROM THE DECLARATION";
			declaration.JE_OwnerRef = "DECLARATION OWNER REFERENCE";
			declaration.HouseBillIssuedDate = new ZDateTime(2008, 8, 8);
			declaration.JE_DeliveryOrPickupLabourCharge = 18.00m;
			declaration.JE_DeliveryOrPickupLabourTime = ZDateTime.Now.AddHours(34).AddMinutes(30);
			declaration.JE_PickupOrDeliveryTruckWaitCharge = 42.98m;
			declaration.JE_PickupOrDeliveryTruckWaitTime = ZDateTime.Now.AddHours(-34).AddMinutes(-30);
			declaration.DepotDocAddress.E2_OA_Address = depotAddress.PK;
			declaration.JE_RL_NKOrigin = "AUSYD";
			declaration.JE_RL_NKFinalDestination = "AUZZZ";
			declaration.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 7;
			declaration.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2012, 3, 19);
			declaration.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2012, 3, 20);
			declaration.JE_TotalVolume = 1234.4m;
			declaration.JE_TotalVolumeUnit = "OT";
			declaration.JE_HouseBill = "HD1111";
			declaration.JE_OH_ShippingLine = GetOrgHeader("SHIPPINGLINE").PK;

			BaseCusContainer container = declaration.CusContainers.AddNew();
			container.CO_ContainerNumber = "CL0000026";
			container.CO_FCL_LCL_AIR = Constants.ContainerModes.Containerised;

			declaration.Invoices.AddNew();
			declaration.InvoiceLines.AddNew();
			declaration.CustomsEntryHeaders.AddNew().EntryNumber = "E1";

			Order order = Factory.New<Order>();
			order.JD_JE = declaration.PK;

			FreightWrapperFromOrder fullWrapper = new FreightWrapperFromOrder(order, Factory);
			AssertEquals("fullWrapper.LocalForwarder.CompanyName", "TEST FORWARDER", fullWrapper.LocalForwarder.CompanyName);
			AssertEquals("fullWrapper.ExportAgent.CompanyName", "TEST FORWARDER", fullWrapper.ExportAgent.CompanyName);
			AssertEquals("fullWrapper.ExportBroker.CompanyName", "EXPORT BROKER PROXY", fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.NotifyParty.CompanyName", "NOTIFY DECLARATION", fullWrapper.NotifyParty.CompanyName);
			AssertEquals("fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero", "7 Days", fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.MarksAndNumbers", "SOME MARKS AND NUMBERS FROM THE DECLARATION", fullWrapper.MarksAndNumbers);
			AssertEquals("fullWrapper.OwnerReference", "DECLARATION OWNER REFERENCE", fullWrapper.OwnerReference);
			AssertEquals("fullWrapper.HouseBillIssue", new ZDateTime(2008, 8, 8), fullWrapper.HouseBillIssue);
			AssertEquals("fullWrapper.HBLContainerMode", Constants.ContainerModes.FCL, fullWrapper.HBLContainerMode);
			AssertEquals("fullWrapper.PickupLabourCharge", 18.00m, fullWrapper.PickupLabourCharge);
			AssertEquals("fullWrapper.PickupLabourTime", "34:30", fullWrapper.PickupLabourTime);
			AssertEquals("fullWrapper.PickupTruckWaitCharge", 42.98m, fullWrapper.PickupTruckWaitCharge);
			AssertEquals("fullWrapper.PickupTruckWaitTime", "-34:30", fullWrapper.PickupTruckWaitTime);
			AssertEquals("fullWrapper.PickupCFSAddress.Address", "TEST 1 ADDRESS\nAUSTRALIA", fullWrapper.PickupCFSAddress.Address);
			AssertEquals("fullWrapper.CommercialInvoices.Count", 1, fullWrapper.CommercialInvoices.Count);
			AssertEquals("fullWrapper.CommercialInvoiceLines.Count", 1, fullWrapper.CommercialInvoiceLines.Count);
			AssertEquals("fullWrapper.CustomsEntries.Count", 1, fullWrapper.CustomsEntries.Count);
			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "AUSYD", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "AUZZZ", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.Volume.ValueAndUnitCodeBlankIfZero", "1234.400 OT", fullWrapper.Volume.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.HouseBill", "HD1111", fullWrapper.HouseBill);
			AssertEquals("fullWrapper.DeliveryGoodsDelivered", new ZDateTime(2012, 3, 19), fullWrapper.DeliveryGoodsDelivered);
			AssertEquals("fullWrapper.PickupGoodsPickedup", new ZDateTime(2012, 3, 20), fullWrapper.PickupGoodsPickedup);
			AssertEquals("fullWrapper.Containers.Count", 1, fullWrapper.Containers.Count);
			AssertEquals("fullWrapper.Carrier.CompanyName", "SHIPPINGLINE", fullWrapper.Carrier.CompanyName);
		}

		[TestDate(2007, 1, 1)]
		public void TestFallbackToShipment()
		{
			var shipmentConsignee = Factory.New<OrgHeader>();
			shipmentConsignee.OH_FullName = "SHIPMENT CONSIGNEE";
			shipmentConsignee.MainAddress.OA_RN_NKCountryCode = "AU";

			var exportBroker = Factory.New<OrgHeader>();
			exportBroker.OH_FullName = "SHIPMENT EXPORT BROKER";
			exportBroker.MainAddress.OA_RN_NKCountryCode = "AU";

			var importBroker = Factory.New<OrgHeader>();
			importBroker.OH_FullName = "SHIPMENT IMPORT BROKER";
			importBroker.MainAddress.OA_RN_NKCountryCode = "AU";

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_FullName = "SHIPMENT NOTIFY PARTY";
			notifyParty.MainAddress.OA_RN_NKCountryCode = "AU";

			var consol = Factory.New<ForwardingConsol>();
			var carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "TEST CARRIER FOR SHIPMENT";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";
			var carrierAddr2 = carrier.Addresses.AddNew();
			carrierAddr2.OA_Address1 = "TEST CARRIER ADDRESS";
			carrierAddr2.OA_RN_NKCountryCode = "AU";
			consol.JK_OA_ShippingLineAddress = carrierAddr2.PK;

			var pickupdepotAddress = Factory.New<OrgAddress>();
			pickupdepotAddress.OA_Address1 = "PICKUP DEPOT ADDRESS";
			pickupdepotAddress.OA_RN_NKCountryCode = "AU";

			var unpackdepotAddress = Factory.New<OrgAddress>();
			unpackdepotAddress.OA_Address1 = "UNPACK DEPOT ADDRESS";
			unpackdepotAddress.OA_RN_NKCountryCode = "AU";

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = shipmentConsignee.PK;
			shipment.JS_OH_ExportBroker = exportBroker.PK;
			shipment.JS_OH_ImportBroker = importBroker.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			shipment.JS_TotalPackageCount = 23;
			shipment.JS_F3_NKTotalCountPackType = "BOX";
			shipment.DocsAndCartage.JP_LCLAirStorageDaysOrHours = 45;
			shipment.JS_OuterPacks = 23;
			shipment.JS_ActualVolume = 8.00m;
			shipment.JS_UnitOfVolume = "OT";
			shipment.JS_ActualChargeable = 23.45m;
			shipment.JS_GoodsValue = 109.87m;
			shipment.JS_RX_NKGoodsValueCurr = "AUD";
			shipment.JS_UnitFreightRate = 123.45m;
			shipment.JS_RX_NKFrtRateCurrency = "HKD";
			shipment.JS_MarksAndNumbers = "HOW MANY QUOLLS DIED TO MAKE THAT WIG";
			shipment.JS_HouseBillIssueDate = new ZDateTime(2007, 7, 7);
			shipment.JS_ShippedOnBoardDate = new ZDateTime(2008, 12, 15);
			shipment.JS_NoOriginalBills = 15;
			shipment.JS_NoCopyBills = 13;
			shipment.JS_InterimReceipt = "INTRMRCPT 123";
			shipment.JS_WarehouseLocation = "THE DUFF";
			shipment.JS_OA_ExportReceivingDepot = pickupdepotAddress.PK;
			shipment.JS_OA_ImportReleaseDepot = unpackdepotAddress.PK;
			shipment.JS_ReleaseType = "NXS";
			shipment.JS_RL_NKOrigin = "USLKJ";
			shipment.JS_RL_NKDestination = "AZZZ";
			shipment.JS_HouseBill = "HS1111";

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CL0000026";

			shipment.DocsAndCartage.JP_PickupLabourCharge = 14.50m;
			shipment.DocsAndCartage.JP_PickupLabourTime = ZDateTime.Now.AddHours(89);
			shipment.DocsAndCartage.JP_PickupTruckWaitCharge = 36.23m;
			shipment.DocsAndCartage.JP_PickupTruckWaitTime = ZDateTime.Now.AddHours(-48).AddMinutes(-23);
			shipment.DocsAndCartage.JP_DeliveryCartageCompleted = new ZDateTime(2012, 3, 19);
			shipment.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2012, 3, 20);

			var order = Factory.New<Order>();
			order.JD_JS = shipment.PK;

			var fullWrapper = new FreightWrapperFromOrder(order, Factory);

			AssertEquals("fullWrapper.ExportBroker.CompanyName", "SHIPMENT EXPORT BROKER", fullWrapper.ExportBroker.CompanyName);
			AssertEquals("fullWrapper.ImportBroker.CompanyName", "SHIPMENT IMPORT BROKER", fullWrapper.ImportBroker.CompanyName);
			AssertEquals("fullWrapper.NotifyParty.CompanyName", "SHIPMENT NOTIFY PARTY", fullWrapper.NotifyParty.CompanyName);
			AssertEquals("fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero", "23 BOX", fullWrapper.ShipmentInnerPacksQty.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero", "45 Days", fullWrapper.StorageTime.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero", "23.450 M3", fullWrapper.ChargeableWeight.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.GoodsValue.AmountAndCurrencyCode", "109.87 AUD", fullWrapper.GoodsValue.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.FreightRate.AmountAndCurrencyCode", "123.45 HKD", fullWrapper.FreightRate.AmountAndCurrencyCode);
			AssertEquals("fullWrapper.MarksAndNumbers", "HOW MANY QUOLLS DIED TO MAKE THAT WIG", fullWrapper.MarksAndNumbers);
			AssertEquals("fullWrapper.HouseBillIssue", new ZDateTime(2007, 7, 7), fullWrapper.HouseBillIssue);
			AssertEquals("fullWrapper.HBLContainerMode", Constants.ContainerModes.LCL, fullWrapper.HBLContainerMode);
			AssertEquals("fullWrapper.ShippedOnBoardDate", new ZDateTime(2008, 12, 15), fullWrapper.ShippedOnBoardDate);
			AssertEquals("fullWrapper.NoOriginalBills", 3, fullWrapper.NoOriginalBills);
			AssertEquals("fullWrapper.NoCopyBills", 3, fullWrapper.NoCopyBills);
			AssertEquals("fullWrapper.ReleaseType.Code", "NXS", fullWrapper.ReleaseType.Code);
			AssertEquals("fullWrapper.PickupInterimReceipt", "INTRMRCPT 123", fullWrapper.PickupInterimReceipt);
			AssertEquals("fullWrapper.WarehouseLocation", "THE DUFF", fullWrapper.WarehouseLocation);
			AssertEquals("fullWrapper.PickupLabourCharge", 14.50m, fullWrapper.PickupLabourCharge);
			AssertEquals("fullWrapper.PickupLabourTime", "89:00", fullWrapper.PickupLabourTime);
			AssertEquals("fullWrapper.PickupTruckWaitCharge", 36.23m, fullWrapper.PickupTruckWaitCharge);
			AssertEquals("fullWrapper.PickupTruckWaitTime", "-48:23", fullWrapper.PickupTruckWaitTime);
			AssertEquals("fullWrapper.PickupCFSAddress.CompanyName", "", fullWrapper.PickupCFSAddress.CompanyName);
			AssertEquals("fullWrapper.PickupCFSAddress.Address", "PICKUP DEPOT ADDRESS\nAUSTRALIA", fullWrapper.PickupCFSAddress.Address);
			AssertEquals("fullWrapper.UnpackCFSAddress.Address", "UNPACK DEPOT ADDRESS\nAUSTRALIA", fullWrapper.UnpackCFSAddress.Address);
			AssertEquals("fullWrapper.Carrier.CompanyName", "TEST CARRIER FOR SHIPMENT", fullWrapper.Carrier.CompanyName);
			AssertEquals("fullWrapper.Origin.Location.UNLOCO", "USLKJ", fullWrapper.Origin.Location.UNLOCO);
			AssertEquals("fullWrapper.Destination.Location.UNLOCO", "AZZZ", fullWrapper.Destination.Location.UNLOCO);
			AssertEquals("fullWrapper.Volume.ValueAndUnitCodeBlankIfZero", "8.000 OT", fullWrapper.Volume.ValueAndUnitCodeBlankIfZero);
			AssertEquals("fullWrapper.HouseBill", "HS1111", fullWrapper.HouseBill);
			AssertEquals("fullWrapper.DeliveryGoodsDelivered", new ZDateTime(2012, 3, 19), fullWrapper.DeliveryGoodsDelivered);
			AssertEquals("fullWrapper.PickupGoodsPickedup", new ZDateTime(2012, 3, 20), fullWrapper.PickupGoodsPickedup);
			AssertEquals("fullWrapper.Containers.Count", 1, fullWrapper.Containers.Count);
		}

		public void TestWeightVolumeDecimalPlaces()
		{
			var collection = new DefaultNumberOfDecimalsCollection(Module.Freight);
			var defaultNumberOfDecimals_SeaWeight = collection.AddNew();
			defaultNumberOfDecimals_SeaWeight.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaWeight.UnitOfMeasure = Core.Constants.Weight.Kilograms;
			defaultNumberOfDecimals_SeaWeight.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaWeight.RoundingMode = RoundingModes.Up;
			var defaultNumberOfDecimals_SeaVolume = collection.AddNew();
			defaultNumberOfDecimals_SeaVolume.TransportMode = Core.Constants.TransportModes.Sea;
			defaultNumberOfDecimals_SeaVolume.UnitOfMeasure = Core.Constants.Volume.CubicMetres;
			defaultNumberOfDecimals_SeaVolume.NumberOfDecimals = 2;
			defaultNumberOfDecimals_SeaVolume.RoundingMode = RoundingModes.Down;

			FreightConfigurationRegistry.Instance.DefaultNumberOfDecimalPlaces.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, collection);

			var order = Factory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.JD_UnitOfWeight = Core.Constants.Weight.Kilograms;
			order.JD_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			order.JD_ActualWeight = 16.741m;
			order.JD_ActualVolume = 123.458m;

			var wrapper = new FreightWrapperFromOrder(order, Factory);

			AssertEquals(16.75m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("16.75 KG", wrapper.Weight.ValueAndUnitCode);

			AssertEquals(123.45m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
			AssertEquals("123.45 M3", wrapper.Volume.ValueAndUnitCode);

			AssertEquals(0m, wrapper.ChargeableWeight.Value);
			AssertEquals("M3", wrapper.ChargeableWeight.Unit.Code);
			AssertEquals("0.000 M3", wrapper.ChargeableWeight.ValueAndUnitCode);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;
			shipment.JS_UnitOfWeight = Core.Constants.Weight.Kilograms;
			shipment.JS_UnitOfVolume = Core.Constants.Volume.CubicMetres;
			shipment.JS_ActualWeight = 157.261m;
			shipment.JS_ActualVolume = 23.567m;

			order.JD_JS = shipment.PK;

			wrapper = new FreightWrapperFromOrder(order, Factory);

			AssertEquals(157.27m, wrapper.Weight.Value);
			AssertEquals("KG", wrapper.Weight.Unit.Code);
			AssertEquals("157.27 KG", wrapper.Weight.ValueAndUnitCode);

			AssertEquals(23.56m, wrapper.Volume.Value);
			AssertEquals("M3", wrapper.Volume.Unit.Code);
			AssertEquals("23.56 M3", wrapper.Volume.ValueAndUnitCode);

			AssertEquals(23.56m, wrapper.ChargeableWeight.Value);
			AssertEquals("M3", wrapper.ChargeableWeight.Unit.Code);
			AssertEquals("23.56 M3", wrapper.ChargeableWeight.ValueAndUnitCode);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "BUYER";

			var result = Factory.New<Order>();
			result.BuyerPK = buyer.PK;

			return result;
		}

		protected override Base.GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			OrgHeader forwarder = Factory.New<OrgHeader>();
			forwarder.OH_FullName = "TEST FORWARDER";
			forwarder.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader cTO = Factory.New<OrgHeader>();
			cTO.OH_FullName = "I ABSOLUTELY HATE THIS CRAP";
			cTO.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader carrier = Factory.New<OrgHeader>();
			carrier.OH_FullName = "TEST CARRIER FOR ORDER";
			carrier.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_FullName = "TEST SUPPLIER FOR ORDER";
			supplier.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader buyer = Factory.New<OrgHeader>();
			buyer.OH_FullName = "TEST BUYER FOR ORDER";
			buyer.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_FullName = "RECEIVING AGENT FOR ORDER";
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_FullName = "SENDING AGENT FOR ORDER";
			sendingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress deliveryAddress = buyer.Addresses.AddNew();
			deliveryAddress.OA_Address1 = "THE FIRST PART OF THE DELIVERY ADDRESS";
			deliveryAddress.OA_Address2 = "THE SECOND PART OF THE DELIVERY ADDRESS";
			deliveryAddress.OA_City = "WHO CARES";
			deliveryAddress.OA_PostCode = "*#&$(*@#";
			deliveryAddress.OA_RN_NKCountryCode = "AU";

			OrgAddress pickupAddress = supplier.Addresses.AddNew();
			pickupAddress.OA_Address1 = "THE FIRST PART OF THE PICKUP ADDRESS";
			pickupAddress.OA_Address2 = "THE SECOND PART OF THE PICKUP ADDRESS";
			pickupAddress.OA_City = "SCOTTLAND";
			pickupAddress.OA_PostCode = "666";
			pickupAddress.OA_RN_NKCountryCode = "AU";

			OrgHeader shipmentConsignee = Factory.New<OrgHeader>();
			shipmentConsignee.OH_FullName = "SHIPMENT CONSIGNEE";
			shipmentConsignee.MainAddress.OA_RN_NKCountryCode = "AU";

			Order order = Factory.New<Order>();
			order.JD_OrderStatus = "SHP";
			order.JD_ContainerMode = "FCL";
			order.JD_TransportMode = "SEA";
			order.JD_RS_NKServiceLevel_NI = "D2D";
			order.JD_IncoTerm = "FOB";
			order.JD_OH_Carrier = carrier.PK;
			order.SupplierPK = supplier.PK;
			order.BuyerPK = buyer.PK;
			order.GoodsDeliveredToAddress.E2_OA_Address = deliveryAddress.PK;
			order.GoodsAvailableAtAddress.E2_OA_Address = pickupAddress.PK;
			order.JD_OH_SendingAgent = sendingAgent.PK;
			order.JD_Packs = 12;
			order.JD_F3_NKPackType = "BAG";
			order.JD_ActualWeight = 16.78m;
			order.JD_UnitOfWeight = "M3";
			order.JD_ActualVolume = 123.458m;
			order.JD_UnitOfVolume = "OT";
			order.JD_OrderDate = new ZDateTime(2017, 9, 12);

			order.JD_RV_NKDepartureVessel = "DEPARTURE VESSEL NAME";
			order.JD_DepartureVoyage = "S987";
			order.JD_RV_NKIntermediateVessel = "INTERMEDIATE VESSEL NAME";
			order.JD_IntermediateVoyage = "N34";
			order.JD_RV_NKArrivalVessel = "ARRIVAL VESSEL NAME";
			order.JD_ArrivalVoyage = "E98";

			OrderLine orderline = order.OrderLines.AddNew();
			orderline.JO_LinePrice = 123.45m;

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_OH_Forwarder = forwarder.PK;

			JobDocAddress declarationNotify = declaration.NotifyPartyDocumentaryAddress;
			declarationNotify.E2_AddressOverride = true;
			declarationNotify.E2_CompanyName = "NOTIFY DECLARATION";

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_ConsolMode = Constants.ContainerModes.BuyersConsol;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_OA_ArrivalCTOAddress = cTO.Addresses[0].PK;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualChargeable = 23.45m;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = shipmentConsignee.PK;
			shipment.JS_UnitFreightRate = 123.45m;
			shipment.JS_RX_NKFrtRateCurrency = "HKD";

			declaration.JE_JS = shipment.PK;
			order.JD_JE = declaration.PK;
			declaration.JE_RL_NKFinalDestination = "AUZZZ";
			declaration.JE_RL_NKOrigin = "AUMEL";
			declaration.JE_TotalVolume = 123.458m;
			declaration.JE_TotalVolumeUnit = "OT";
			declaration.JE_TotalWeight = 16.78m;
			declaration.JE_TotalWeightUnit = "M3";
			declaration.JE_OH_ShippingLine = GetOrgHeader("SHIPPINGLINE").PK;
			declaration.JE_TotalNoOfPacks = 500;
			declaration.JE_TotalNoOfPacksPackType = "BAG";

			JobDocAddress shipmentInsuredBy = shipment.InsuredByDocAddress;
			shipmentInsuredBy.E2_AddressOverride = true;
			shipmentInsuredBy.E2_CompanyName = "INSUREDBY SHIPMENT";

			JobDocAddress shipmentAssuredParty = shipment.AssuredPartyDocAddress;
			shipmentAssuredParty.E2_AddressOverride = true;
			shipmentAssuredParty.E2_CompanyName = "ASSUREDPARTY SHIPMENT";

			JobDocAddress shipmentClaimsPayableBy = shipment.ClaimsPayableByDocAddress;
			shipmentClaimsPayableBy.E2_AddressOverride = true;
			shipmentClaimsPayableBy.E2_CompanyName = "CLAIMSPAYABLEBY SHIPMENT";

			JobDocAddress shipmentSurveyReportParty = shipment.SurveyReportPartyDocAddress;
			shipmentSurveyReportParty.E2_AddressOverride = true;
			shipmentSurveyReportParty.E2_CompanyName = "SURVEYREPORTPARTY SHIPMENT";

			order.JD_RL_NKGoodsAvailableAt = "AUMEL";
			order.JD_RL_NKGoodsDeliveredTo = "NZQQQ";
			order.JD_OH_ReceivingAgent = receivingAgent.PK;

			return new FreightWrapperFromOrder(order, Factory);
		}

		protected override Dictionary<string, string> OverriddenValuesOfIZTypeProperties
		{
			get
			{
				return new Dictionary<string, string>
				{
					{ "HouseBillHeading", "House Bill Of Lading" },
					{ "JobNumberHeading", "Order" },
					{ "MasterBillHeading", "Ocean Bill Of Lading" },
					{ "NoCopyBills", "3" },
					{ "NoOriginalBills", "3" },
					{ "OrderDate", "12-Sep-17 00:00:00" },
					{ "SecondaryHeading", "Shipment" }
				};
			}
		}

		protected override ZString OverriddenExpectedDefaultFormatting
		{
			get
			{
				return @"
AssuredParty : ASSUREDPARTY SHIPMENT\nERITREA
Buyer : TEST BUYER FOR ORDER\nAUSTRALIA
Carrier : SHIPPINGLINE\nAUSTRALIA
CarrierAccount :  is null
CarrierServiceLevel : 
ChargeableWeight : 23.450 M3
ClaimsPayableBy : CLAIMSPAYABLEBY SHIPMENT\nERITREA
Consignee : SHIPMENT CONSIGNEE\nAUSTRALIA
ConsolContainerMode : FCL - Full Container Load
ConsolTransportMode : SEA - Sea Freight
ConsolType : AGT - Agent
CTOArrival : I ABSOLUTELY HATE THIS CRAP\nAUSTRALIA
DeliveryAddress : TEST BUYER FOR ORDER\nTHE FIRST PART OF THE DELIVERY ADDRESS\nTHE SECOND PART OF THE DELIVERY ADDRESS\nWHO CARES *#&$(*@#\nAUSTRALIA
DeliveryAgent : RECEIVING AGENT FOR ORDER\nAUSTRALIA
DeliveryLocation : AUZZZ
Destination : AUZZZ
ExportAgent : TEST FORWARDER\nAUSTRALIA
ExportBroker : EDI CUSTOMS BROKERS\n10 HUTCHESON STREET\nALBION QLD\n4010\nAUSTRALIA
FreightRate : 123.45 HKD
GoodsAvailableAt : TEST SUPPLIER FOR ORDER\nTHE FIRST PART OF THE PICKUP ADDRESS\nTHE SECOND PART OF THE PICKUP ADDRESS\nSCOTTLAND 666\nAUSTRALIA
GoodsValue : 123.45 ERN
ImportArrivalCTOAddress : I ABSOLUTELY HATE THIS CRAP\nAUSTRALIA
IncoTerm : FOB - Free On Board
InsuredBy : INSUREDBY SHIPMENT\nERITREA
LocalForwarder : TEST FORWARDER\nAUSTRALIA
NotifyParty : NOTIFY DECLARATION\nERITREA
OrderTransportMode : SEA - Sea Freight
Origin : AUMEL - Melbourne
PickupAddress : TEST SUPPLIER FOR ORDER\nTHE FIRST PART OF THE PICKUP ADDRESS\nTHE SECOND PART OF THE PICKUP ADDRESS\nSCOTTLAND 666\nAUSTRALIA
PickupAgent : SENDING AGENT FOR ORDER\nAUSTRALIA
PickupCFSAddress : 
PickupLocation : AUMEL - Melbourne
PortOfFirstArrival : 
Principal : 
QueryClaim :  is null
Rating :  is null
ReceivingForwarder : 
RecommendedAgent : 
Registry : (No Default Field Value Available on Registry)
ReleaseType : 
RunSheet :  is null
SalesRep :  is null
SellingParty : 
SendingForwarder : TEST FORWARDER\nAUSTRALIA
ServiceLevel : D2D - Door to Door
ShipmentContainerMode : FCL - Full Container Load
ShipmentOuterPacksQty : 500 BAG
ShipmentStatus : SHP - Shipped
ShipmentTransportMode : SEA - Sea Freight
SurveyReportParty : SURVEYREPORTPARTY SHIPMENT\nERITREA
Volume : 123.458 OT
Weight : 16.780 M3";
			}
		}

		protected override FreightWrapper GetNewDocumentWrapperWithCarrier()
		{
			((Order)WrappedBO).JD_OH_Carrier = Factory.LoadTop1<OrgHeader>(new ZQuery()).PK;
			return new FreightWrapperFromOrder((Order)WrappedBO, Factory);
		}

		Order CreateOrder(
			string consolReceivingForwarderName = "Lewis Hamilton",
			string consolSendingForwarderName = "Jenson Button",
			string declarationForwarderName = "Ron Dennis",
			bool isExportDeclaration = true)
		{
			var consol = Factory.New<ForwardingConsol>();

			if (!string.IsNullOrEmpty(consolReceivingForwarderName))
			{
				consol.SetDefaultReceivingForwarderAddress(GetOrgHeader(consolReceivingForwarderName));
			}

			if (!string.IsNullOrEmpty(consolSendingForwarderName))
			{
				consol.SetDefaultSendingForwarderAddress(GetOrgHeader(consolSendingForwarderName));
			}

			var shipment = consol.Shipments.AddNew();

			var order = Factory.New<Order>();
			order.JD_JS = shipment.PK;

			if (!string.IsNullOrEmpty(declarationForwarderName))
			{
				var forwarder = Factory.New<OrgHeader>();
				forwarder.OH_FullName = declarationForwarderName;

				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				declaration.JE_OH_Forwarder = forwarder.PK;
				declaration.JE_MessageType = isExportDeclaration ? JobMessageTypeList.Codes.Export : JobMessageTypeList.Codes.Import;

				order.JD_JE = declaration.PK;
			}

			return order;
		}
	}
}
