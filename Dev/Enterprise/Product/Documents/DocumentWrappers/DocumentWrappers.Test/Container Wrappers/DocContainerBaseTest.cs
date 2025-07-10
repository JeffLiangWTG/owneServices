using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.ContainerWrappers
{
	[TestedType(typeof(DocContainer))]
	class DocContainerBaseTest : DocFreightBaseContainerAbstractTest
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[] { DocContainer.New(FreightContainer, Factory) };
		}

		#region Overrides

		public void TestFrieghtContainerToString()
		{
			FreightContainer.JC_ContainerNum = "CONNUM";
			AssertEquals("ToString()", "CONNUM", ContainerWrapper.ToString());
		}

		#region Cartage Advice Fields

		public override void TestJourneyOnePickUpAddress()
		{
			AssertNull("JourneyOnePickUpAddress", ContainerWrapper.JourneyOnePickUpAddress);

			var consol = Factory.New<ForwardingConsol>();
			FreightContainer = consol.Containers.AddNew();

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consol.JK_OA_ContainerYardEmptyPickupAddress = address.PK;

			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			Factory.Save();
			AssertNotNull("JourneyOnePickUpAddress", ContainerWrapper.JourneyOnePickUpAddress);
			AssertEquals("JourneyOnePickUpAddress is of type DocDocAddress", typeof(DocDocAddress), ContainerWrapper.JourneyOnePickUpAddress.GetType());
			AssertEquals("JourneyOnePickUpAddress", address.OA_Code, ContainerWrapper.JourneyOnePickUpAddress.Code);

			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, consol.JK_OA_ContainerYardEmptyPickupAddress));
			consol.JK_OA_ArrivalCTOAddress = address.PK;
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			Factory.Save();
			AssertNotNull("JourneyOnePickUpAddress", ContainerWrapper.JourneyOnePickUpAddress);
			AssertEquals("JourneyOnePickUpAddress is of type DocDocAddress", typeof(DocDocAddress), ContainerWrapper.JourneyOnePickUpAddress.GetType());
			AssertEquals("JourneyOnePickUpAddress", address.OA_Code, ContainerWrapper.JourneyOnePickUpAddress.Code);

			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, consol.JK_OA_ArrivalCTOAddress));
			FreightContainer.JC_OA_DepartureContainerYardAddress = address.PK;
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			Factory.Save();
			AssertNotNull("JourneyOnePickUpAddress", ContainerWrapper.JourneyOnePickUpAddress);
			AssertEquals("JourneyOnePickUpAddress is of type DocDocAddress", typeof(DocDocAddress), ContainerWrapper.JourneyOnePickUpAddress.GetType());
			AssertEquals("JourneyOnePickUpAddress", address.OA_Code, ContainerWrapper.JourneyOnePickUpAddress.Code);

			address = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, FreightContainer.JC_OA_DepartureContainerYardAddress));
			//FreightContainer.JC_OA_ArrivalCTOAddress = Address.PK;
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			Factory.Save();
			AssertNotNull("JourneyOnePickUpAddress", ContainerWrapper.JourneyOnePickUpAddress);
			AssertEquals("JourneyOnePickUpAddress is of type DocDocAddress", typeof(DocDocAddress), ContainerWrapper.JourneyOnePickUpAddress.GetType());
			AssertEquals("JourneyOnePickUpAddress", address.OA_Code, ContainerWrapper.JourneyOnePickUpAddress.Code);
		}

		public override void TestJourneyOneDeliverToAddressForExport()
		{
			AssertNull("JourneyOneDeliverToAddressForExport", ContainerWrapper.JourneyOneDeliverToAddressForExport);

			AddContainerToConsolAndShipment();
			AssertNull("JourneyOneDeliverToAddressForExport", ContainerWrapper.JourneyOneDeliverToAddressForExport);

			OrgHeader consignor = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsignorPK = consignor.PK;
			AssertNotNull("JourneyOneDeliverToAddressForExport", ContainerWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyOneDeliverToAddressForExport", ContainerWrapper.Shipment.Consignor.DeliverAddress.PostalAddress, ContainerWrapper.JourneyOneDeliverToAddressForExport.PostalAddress);

			OrgAddress address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsignorPickupAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyOneDeliverToAddressForExport", ContainerWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyOneDeliverToAddressForExport", DocAddress.New(address, Factory).PostalAddress, ContainerWrapper.JourneyOneDeliverToAddressForExport.PostalAddress);

			OrgAddress confirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			FreightContainer.OriginConfirm.PickupFrom.E2_OA_Address = confirmAddress.PK;
			AssertNotNull("JourneyOneDeliverToAddressForExport", ContainerWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyOneDeliverToAddressForExport", DocAddress.New(confirmAddress, Factory).PostalAddress, ContainerWrapper.JourneyOneDeliverToAddressForExport.PostalAddress);

			OrgAddress cfsAddress = Factory.New<OrgAddress>();
			cfsAddress.OA_Address1 = "cfs Address";
			Consol.JK_OA_PackDepotAddress = cfsAddress.PK;
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertNotNull("JourneyOneDeliverToAddressForExport", ContainerWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyOneDeliverToAddressForExport", cfsAddress.OA_Address1, ContainerWrapper.JourneyOneDeliverToAddressForExport.Address1);
		}

		public override void TestJourneyOneDeliverToAddressForImport()
		{
			AssertNull("JourneyOneDeliverToAddressForImport", ContainerWrapper.JourneyOneDeliverToAddressForImport);

			AddContainerToConsolAndShipment();
			AssertNull("JourneyOneDeliverToAddressForImport", ContainerWrapper.JourneyOneDeliverToAddressForImport);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsigneePK = header.PK;
			AssertNotNull("JourneyOneDeliverToAddressForImport", ContainerWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyOneDeliverToAddressForImport", ContainerWrapper.Shipment.Consignee.DeliverAddress.PostalAddress, ContainerWrapper.JourneyOneDeliverToAddressForImport.PostalAddress);

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyOneDeliverToAddressForImport", ContainerWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyOneDeliverToAddressForImport", DocAddress.New(address, Factory).PostalAddress, ContainerWrapper.JourneyOneDeliverToAddressForImport.PostalAddress);

			OrgAddress confirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			FreightContainer.DestinationConfirm.DeliverTo.E2_OA_Address = confirmAddress.PK;
			AssertNotNull("JourneyOneDeliverToAddressForImport", ContainerWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyOneDeliverToAddressForImport", DocAddress.New(confirmAddress, Factory).PostalAddress, ContainerWrapper.JourneyOneDeliverToAddressForImport.PostalAddress);

			OrgAddress cfsAddress = Factory.New<OrgAddress>();
			cfsAddress.OA_Address1 = "cfs Address";
			Consol.JK_OA_UnpackDepotAddress = cfsAddress.PK;
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertNotNull("JourneyOneDeliverToAddressForImport", ContainerWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyOneDeliverToAddressForImport", cfsAddress.OA_Address1, ContainerWrapper.JourneyOneDeliverToAddressForImport.Address1);
		}

		public override void TestJourneyTwoPickUpAddressForExport()
		{
			AssertNull("JourneyTwoPickUpAddressForExport", ContainerWrapper.JourneyTwoPickUpAddressForExport);

			AddContainerToConsolAndShipment();
			AssertNull("JourneyTwoPickUpAddressForExport", ContainerWrapper.JourneyTwoPickUpAddressForExport);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsignorPK = header.PK;
			AssertNotNull("JourneyTwoPickUpAddressForExport", ContainerWrapper.JourneyTwoPickUpAddressForExport);
			AssertEquals("JourneyTwoPickUpAddressForExport", ContainerWrapper.Shipment.Consignor.PickUpAddress.PostalAddress, ContainerWrapper.JourneyTwoPickUpAddressForExport.PostalAddress);

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsignorPickupAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyTwoPickUpAddressForExport", ContainerWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyTwoPickUpAddressForExport", DocAddress.New(address, Factory).PostalAddress, ContainerWrapper.JourneyTwoPickUpAddressForExport.PostalAddress);

			OrgAddress confirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			FreightContainer.OriginConfirm.PickupFrom.E2_OA_Address = confirmAddress.PK;
			AssertNotNull("JourneyTwoPickUpAddressForExport", ContainerWrapper.JourneyOneDeliverToAddressForExport);
			AssertEquals("JourneyTwoPickUpAddressForExport", DocAddress.New(confirmAddress, Factory).PostalAddress, ContainerWrapper.JourneyTwoPickUpAddressForExport.PostalAddress);

			OrgAddress cfsAddress = Factory.New<OrgAddress>();
			cfsAddress.OA_Address1 = "cfs Address";
			Consol.JK_OA_PackDepotAddress = cfsAddress.PK;
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("JourneyTwoPickUpAddressForExport is of type DocDocAddress", typeof(DocDocAddress), ContainerWrapper.JourneyTwoPickUpAddressForExport.GetType());
			AssertEquals("JourneyTwoPickUpAddressForExport", cfsAddress.OA_Address1, ContainerWrapper.JourneyTwoPickUpAddressForExport.Address1);
		}

		public override void TestJourneyTwoPickUpAddressForImport()
		{
			AssertNull("JourneyTwoPickUpAddressForImport", ContainerWrapper.JourneyTwoPickUpAddressForImport);

			AddContainerToConsolAndShipment();
			AssertNull("JourneyTwoPickUpAddressForImport", ContainerWrapper.JourneyTwoPickUpAddressForImport);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			Shipment.ConsigneePK = header.PK;
			AssertNotNull("JourneyTwoPickUpAddressForImport", ContainerWrapper.JourneyTwoPickUpAddressForImport);
			AssertEquals("JourneyTwoPickUpAddressForImport", ContainerWrapper.Shipment.Consignee.PickUpAddress.PostalAddress, ContainerWrapper.JourneyTwoPickUpAddressForImport.PostalAddress);

			var address = Factory.LoadTop1<OrgAddress>(new ZQuery());
			Shipment.ConsigneeDeliveryAddress.E2_OA_Address = address.PK;
			AssertNotNull("JourneyTwoPickUpAddressForImport", ContainerWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyTwoPickUpAddressForImport", DocAddress.New(address, Factory).PostalAddress, ContainerWrapper.JourneyTwoPickUpAddressForImport.PostalAddress);

			OrgAddress confirmAddress = Factory.NewWithValidTestData<OrgAddress>();
			FreightContainer.DestinationConfirm.DeliverTo.E2_OA_Address = confirmAddress.PK;
			AssertNotNull("JourneyTwoPickUpAddressForImport", ContainerWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyTwoPickUpAddressForImport", DocAddress.New(confirmAddress, Factory).PostalAddress, ContainerWrapper.JourneyTwoPickUpAddressForImport.PostalAddress);

			OrgAddress cfsAddress = Factory.New<OrgAddress>();
			cfsAddress.OA_Address1 = "cfs Address";
			Consol.JK_OA_UnpackDepotAddress = cfsAddress.PK;
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertNotNull("JourneyTwoPickUpAddressForImport", ContainerWrapper.JourneyOneDeliverToAddressForImport);
			AssertEquals("JourneyTwoPickUpAddressForImport", cfsAddress.OA_Address1, ContainerWrapper.JourneyTwoPickUpAddressForImport.Address1);
		}

		public override void TestJourneyTwoDeliverToAddress()
		{
			AssertNull("JourneyTwoDeliverToAddress", ContainerWrapper.JourneyTwoDeliverToAddress);

			var consol = Factory.New<ForwardingConsol>();
			FreightContainer = consol.Containers.AddNew();

			var address1 = Factory.LoadTop1<OrgAddress>(new ZQuery());
			consol.JK_OA_DepartureCTOAddress = address1.PK;

			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			Factory.Save();
			AssertNotNull("JourneyTwoDeliverToAddress", ContainerWrapper.JourneyTwoDeliverToAddress);
			AssertEquals("JourneyTwoDeliverToAddress is of type DocDocAddress", typeof(DocDocAddress), ContainerWrapper.JourneyTwoDeliverToAddress.GetType());
			AssertEquals("JourneyTwoDeliverToAddress", address1.OA_Code, ContainerWrapper.JourneyTwoDeliverToAddress.Code);

			address1 = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, consol.JK_OA_ContainerYardEmptyPickupAddress));
			consol.JK_OA_ContainerYardEmptyReturnAddress = address1.PK;
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			Factory.Save();
			AssertNotNull("JourneyTwoDeliverToAddress", ContainerWrapper.JourneyTwoDeliverToAddress);
			AssertEquals("JourneyTwoDeliverToAddress is of type DocDocAddress", typeof(DocDocAddress), ContainerWrapper.JourneyTwoDeliverToAddress.GetType());
			AssertEquals("JourneyTwoDeliverToAddress", address1.OA_Code, ContainerWrapper.JourneyTwoDeliverToAddress.Code);
			Factory.Save();
			address1 = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, consol.JK_OA_ArrivalCTOAddress));
			FreightContainer.Consol.JK_OA_DepartureCTOAddress = address1.PK;
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			Factory.Save();
			AssertNotNull("JourneyTwoDeliverToAddress", ContainerWrapper.JourneyTwoDeliverToAddress);
			AssertEquals("JourneyTwoDeliverToAddress is of type DocDocAddress", typeof(DocDocAddress), ContainerWrapper.JourneyTwoDeliverToAddress.GetType());
			AssertEquals("JourneyTwoDeliverToAddress", address1.OA_Code, ContainerWrapper.JourneyTwoDeliverToAddress.Code);

			address1 = Factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.PK, SQLComparisonOperator.NotEqual, FreightContainer.JC_OA_DepartureContainerYardAddress));
			FreightContainer.JC_OA_ArrivalContainerYardAddress = address1.PK;
			ContainerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			Factory.Save();
			AssertNotNull("JourneyTwoDeliverToAddress", ContainerWrapper.JourneyTwoDeliverToAddress);
			AssertEquals("JourneyTwoDeliverToAddress is of type DocDocAddress", typeof(DocDocAddress), ContainerWrapper.JourneyTwoDeliverToAddress.GetType());
			AssertEquals("JourneyTwoDeliverToAddress", address1.OA_Code, ContainerWrapper.JourneyTwoDeliverToAddress.Code);

			QuotedBooking quickBooking = QuotedBooking.New(QuoteBookingType.QuickBooking, Factory);
			OrgAddress address = Factory.NewWithValidTestData<OrgAddress>();
			quickBooking.Booking.JS_OA_ExportReceivingDepot = address.PK;
			ForwardingContainer bookingContainer = quickBooking.QuotedBookingContainers.AddNew();
			DocContainer containerWrapper = DocContainer.New(bookingContainer, quickBooking.Booking, Factory);
			containerWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.DEP));
			AssertEquals("JourneyTwoDeliverToAddress", address.OA_Code, containerWrapper.JourneyTwoDeliverToAddress.Code);
		}

		public void TestEquipmentType()
		{
			AssertEquals("EquipmentType", ZString.Empty, ContainerWrapper.EquipmentType);

			AddContainerToConsolAndShipment();
			AssertEquals("EquipmentType", ContainerWrapper.Shipment.EquipmentType, ContainerWrapper.EquipmentType);
		}

		public void TestConsignee()
		{
			AssertNull("No Consignee", ContainerWrapper.Consignee);

			OrgHeader consignee = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignee.OH_FullName = "Consignee";
			AddContainerToConsolAndShipment();
			Shipment.ConsigneePK = consignee.PK;
			AssertEquals("Consignee should be Consignee", "Consignee", ContainerWrapper.Consignee.Name);
		}

		public void TestConsignor()
		{
			AssertNull("No Consignor", ContainerWrapper.Consignor);

			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignor.OH_FullName = "Consignor";
			AddContainerToConsolAndShipment();
			Shipment.ConsignorPK = consignor.PK;
			AssertEquals("Consignor should be Consignor", "Consignor", ContainerWrapper.Consignor.Name);

			ContainerWrapper.SetReportNameForTesting("CFS blah");
			OrgHeader cFSClient = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			cFSClient.OH_FullName = "CFSClient";
			FreightContainer.JC_OH_CFSClient = cFSClient.PK;
			AssertEquals("Consignor should be CFSClient", "CFSClient", ContainerWrapper.Consignor.Name);
		}

		public void TestConsignorForERA()
		{
			AssertNull("No Consignor", ContainerWrapper.ConsignorForERA);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			OrgHeader forwarder = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			forwarder.OH_FullName = "Forwarder";
			consol.SetDefaultSendingForwarderAddress(forwarder);

			consol.Containers.Add(FreightContainer);
			ForwardingShipment shipment = consol.Shipments.AddNew();
			OrgHeader consignor = Factory.NewWithValidTestData<OrgHeader>(TestBusinessObjectKind.MinimumRequiredToSave);
			consignor.OH_FullName = "Consignor";
			shipment.ConsignorPK = consignor.PK;

			var packLine = shipment.OuterPackLines.AddNew();
			packLine.SetContainer(consol, FreightContainer);

			ContainerWrapper = DocContainer.New(FreightContainer, shipment, Factory);
			AssertEquals("Consignor should be Consignor", "Consignor", ContainerWrapper.ConsignorForERA.Name);

			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("Consignor should be Consignor", "Consignor", ContainerWrapper.ConsignorForERA.Name);

			consol.JK_ConsolMode = Constants.ContainerModes.Groupage;
			AssertEquals("Consignor should be Forwarder", "Forwarder", ContainerWrapper.ConsignorForERA.Name);
		}

		public void TestDocContainerGoodsDescriptionForERA()
		{
			var expectedDefaultResult = "Freight All Kinds";
			AssertEquals("Container Goods desc for ERA", expectedDefaultResult, ContainerWrapper.GoodsDescriptionForERA);

			AddContainerToConsolAndShipment();
			Consol.AutomaticallyUpdatePackLineContainers = false;
			AssertEquals("PackLines count should be 1", 1, ContainerWrapper.PackLines.Count);
			AssertEquals("Shipments count should be 1", 1, ContainerWrapper.Shipments.Count);

			PackLine.JL_Description = "Pack Line Goods Description";
			Shipment.JS_GoodsDescription = "";
			AssertEquals("Only one pack line with the description", "Pack Line Goods Description", ContainerWrapper.GoodsDescriptionForERA);

			Shipment.JS_GoodsDescription = "Shipment Goods Description";
			AssertEquals("One pack line with description and one shipment with description", "Pack Line Goods Description", ContainerWrapper.GoodsDescriptionForERA);

			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.SetContainer(Consol, FreightContainer);
			packLine2.JL_Description = "Pack Line Goods Description";
			AssertEquals("PackLines count should be 2", 2, ContainerWrapper.PackLines.Count);
			AssertEquals("Multiple pack lines with the same descriptions", "Pack Line Goods Description", ContainerWrapper.GoodsDescriptionForERA);

			packLine2.JL_Description = "Pack Line Goods Description 2";
			Shipment.JS_GoodsDescription = "Shipment Goods Description";
			AssertEquals("Multiple pack lines with different descriptions and only one shipment", "Shipment Goods Description", ContainerWrapper.GoodsDescriptionForERA);

			packLine2.JL_Description = "";
			AssertEquals("Multiple pack lines, including one empty-description pack line, and only one shipment", "Shipment Goods Description", ContainerWrapper.GoodsDescriptionForERA);

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_GoodsDescription = "Shipment Goods Description";
			var packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.JL_Description = "Pack Line Goods Description 3";
			packLine3.SetContainer(Consol, FreightContainer);
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);
			AssertEquals("PackLines count should be 3", 3, ContainerWrapper.PackLines.Count);
			AssertEquals("Shipments count should be 2", 2, ContainerWrapper.Shipments.Count);
			AssertEquals("Multiple pack lines with different descriptions and multiple shipments with same descriptions", "Shipment Goods Description", ContainerWrapper.GoodsDescriptionForERA);

			PackLine.JL_Description = "Pack Line Goods Description";
			packLine2.JL_Description = "Pack Line Goods Description";
			packLine3.JL_Description = "";
			Shipment.JS_GoodsDescription = "Shipment Goods Description";
			shipment2.JS_GoodsDescription = "Shipment Goods Description";
			AssertEquals("Multiple pack lines including one empty-description pack line and multiple shipments with same descriptions", "Shipment Goods Description", ContainerWrapper.GoodsDescriptionForERA);

			PackLine.JL_Description = "Pack Line Goods Description";
			packLine2.JL_Description = "Pack Line Goods Description 2";
			packLine3.JL_Description = "Pack Line Goods Description 3";
			Shipment.JS_GoodsDescription = "Shipment Goods Description";
			shipment2.JS_GoodsDescription = "Shipment Goods Description 2";
			AssertEquals("Multiple pack lines with different descriptions and multiple shipments with different descriptions", expectedDefaultResult, ContainerWrapper.GoodsDescriptionForERA);

			PackLine.JL_Description = "Pack Line Goods Description";
			packLine2.JL_Description = "Pack Line Goods Description";
			packLine3.JL_Description = "";
			Shipment.JS_GoodsDescription = "Shipment Goods Description";
			shipment2.JS_GoodsDescription = "Shipment Goods Description 2";
			AssertEquals("Multiple pack lines with different descriptions, including one empty-description, and multiple shipments with different descriptions", expectedDefaultResult, ContainerWrapper.GoodsDescriptionForERA);

			PackLine.JL_Description = "";
			packLine2.JL_Description = "";
			packLine3.JL_Description = "";
			Shipment.JS_GoodsDescription = "Shipment Goods Description";
			shipment2.JS_GoodsDescription = "Shipment Goods Description 2";
			AssertEquals("Multiple pack lines with all empty descriptions and multiple shipments with different descriptions", expectedDefaultResult, ContainerWrapper.GoodsDescriptionForERA);

			PackLine.JL_Description = "";
			packLine2.JL_Description = "";
			packLine3.JL_Description = "";
			Shipment.JS_GoodsDescription = "";
			shipment2.JS_GoodsDescription = "";
			AssertEquals("Multiple pack lines with all empty descriptions and multiple shipments with all empty descritions", expectedDefaultResult, ContainerWrapper.GoodsDescriptionForERA);
		}

		#region IDocContainer Members

		#region ZString Fields

		public override void TestForwardingInstructionWeight()
		{
			AssertEquals("", ContainerWrapper.ForwardingInstructionWeight);

			AddContainerToConsolAndShipment();
			PackLine.JL_ActualWeight = 100.50M;
			AssertEquals("100.5", ContainerWrapper.ForwardingInstructionWeight);

			PackLine.JL_ActualWeight = 0M;
			AssertEquals("", ContainerWrapper.ForwardingInstructionWeight);
		}

		public override void TestForwardingInstructionVolume()
		{
			AssertEquals("", ContainerWrapper.ForwardingInstructionVolume);

			AddContainerToConsolAndShipment();
			PackLine.JL_ActualVolume = 3.50M;
			AssertEquals("3.5", ContainerWrapper.ForwardingInstructionVolume);

			PackLine.JL_ActualVolume = 0M;
			AssertEquals("", ContainerWrapper.ForwardingInstructionVolume);
		}

		public override void TestForwardingInstructionPackages()
		{
			AssertEquals("", ContainerWrapper.ForwardingInstructionPackages);

			AddContainerToConsolAndShipment();
			PackLine.JL_PackageCount = 100;
			AssertEquals("100", ContainerWrapper.ForwardingInstructionPackages);

			PackLine.JL_PackageCount = 0;
			AssertEquals("", ContainerWrapper.ForwardingInstructionPackages);
		}

		public override void TestDescriptionAndStatus()
		{
			AssertEquals("", ContainerWrapper.ForwardingInstructionPackages);
			AddContainerToConsolAndShipment();
			Shipment.JS_GoodsDescription = "Goods Description";
			FreightContainer.JC_ContainerMode = "FCL";
			FreightContainer.JC_IsEmptyContainer = ZBool.False;
			PackLine.JL_RH_NKCommodityCode = "";

			AssertEquals("Description and status ", "Goods Description/FCL", ContainerWrapper.DescriptionAndStatus);

			var commCode = Factory.New<RefCommodityCode>();
			commCode.RH_Code = "TEST";
			commCode.RH_Description = "TEST Description";
			PackLine.JL_RH_NKCommodityCode = commCode.RH_Code;
			AssertEquals("Description and status ", "Goods Description/TEST Description/FCL", ContainerWrapper.DescriptionAndStatus);

			FreightContainer.JC_IsEmptyContainer = ZBool.True;
			AssertEquals("Description and status ", "Goods Description/TEST Description/MT", ContainerWrapper.DescriptionAndStatus);
		}

		#endregion

		#region ZDecimal Fields

		public override void TestTotalAllocatedShipmentWeight()
		{
			AddContainerToConsolAndShipment();
			Consol.AutomaticallyUpdatePackLineContainers = false;
			PackLine.JL_ActualWeight = 40.23M;
			AssertEquals("TotalAllocatedShipmentWeight", 40.23M, ContainerWrapper.TotalAllocatedShipmentWeight);

			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 65.89M;
			packLine2.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalAllocatedShipmentWeight", 106.12M, ContainerWrapper.TotalAllocatedShipmentWeight);

			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualWeight = 12.48M;
			if (FreightContainer.PackLines.Contains(packLine3))
			{
				FreightContainer.PackLines.Remove(packLine3);
			}

			AssertEquals("TotalAllocatedShipmentWeight", 106.12M, ContainerWrapper.TotalAllocatedShipmentWeight);

			var shipment2 = Consol.Shipments.AddNew();
			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_ActualWeight = 134.4356M;
			packLine4.SetContainer(Consol, FreightContainer);

			//First Shipment
			AssertEquals("TotalAllocatedShipmentWeight", 106.12M, ContainerWrapper.TotalAllocatedShipmentWeight);

			//Second Shipment
			ContainerWrapper = DocContainer.New(FreightContainer, shipment2, Factory);
			AssertEquals("TotalAllocatedShipmentWeight", 134.436M, ContainerWrapper.TotalAllocatedShipmentWeight);

			FreightContainer.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals("TotalAllocatedShipmentWeight in Pounds", 296.381m, ContainerWrapper.TotalAllocatedShipmentWeight);
		}

		public override void TestTotalPackLineWeight()
		{
			AddContainerToConsolAndShipment();
			PackLine.JL_ActualWeight = 40.23M;
			AssertEquals("TotalPackLineWeight", 40.23M, ContainerWrapper.TotalPackLineWeight);

			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualWeight = 65.89M;
			packLine2.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalPackLineWeight", 106.12M, ContainerWrapper.TotalPackLineWeight);

			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualWeight = 12.48M;
			FreightContainer.PackLines.Remove(packLine3);

			AssertEquals("TotalPackLineWeight", 106.12M, ContainerWrapper.TotalPackLineWeight);

			var shipment2 = Consol.Shipments.AddNew();
			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_ActualWeight = 134.4356M;
			packLine4.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalPackLineWeight", 240.556M, ContainerWrapper.TotalPackLineWeight);

			shipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			AssertEquals("TotalPackLineWeight - sub shipment weight should be included", 240.556M, ContainerWrapper.TotalPackLineWeight);

			FreightContainer.JC_GrossWeightUQ = Core.Constants.Weight.Pounds;
			AssertEquals("TotalPackLineWeight - sub shipment weight should not be included in Pounds", 530.335m, ContainerWrapper.TotalPackLineWeight);
		}

		public override void TestTotalAllocatedShipmentVolume()
		{
			AddContainerToConsolAndShipment();
			PackLine.JL_ActualVolume = 40.23M;
			PackLine.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			Consol.AutomaticallyUpdatePackLineContainers = false;
			AssertEquals("TotalAllocatedShipmentVolume", 40.23M, ContainerWrapper.TotalAllocatedShipmentVolume);
			AssertEquals("TotalAllocatedShipmentVolumeUQ", "M3", ContainerWrapper.TotalAllocatedShipmentVolumeUQ);

			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualVolume = 65.89M;
			packLine2.JL_ActualVolumeUQ = Constants.Volume.CubicFeet;
			packLine2.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalAllocatedShipmentVolume", 42.096M, ContainerWrapper.TotalAllocatedShipmentVolume);
			AssertEquals("TotalAllocatedShipmentVolumeUQ", "M3", ContainerWrapper.TotalAllocatedShipmentVolumeUQ);

			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualVolume = 12.48M;
			packLine3.JL_ActualVolumeUQ = Constants.Volume.CubicDecimetres;
			if (FreightContainer.PackLines.Contains(packLine3))
			{
				FreightContainer.PackLines.Remove(packLine3);
			}

			AssertEquals("TotalAllocatedShipmentVolume", 42.096M, ContainerWrapper.TotalAllocatedShipmentVolume);
			AssertEquals("TotalAllocatedShipmentVolumeUQ", "M3", ContainerWrapper.TotalAllocatedShipmentVolumeUQ);

			var shipment2 = Consol.Shipments.AddNew();
			shipment2.JS_UnitOfVolume = Constants.Volume.CubicFeet;
			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_ActualVolume = 134.4356M;
			packLine4.JL_ActualVolumeUQ = Constants.Volume.CubicMetres;
			packLine4.SetContainer(Consol, FreightContainer);

			//First Shipment
			AssertEquals("TotalAllocatedShipmentVolume", 42.096M, ContainerWrapper.TotalAllocatedShipmentVolume);
			AssertEquals("TotalAllocatedShipmentVolumeUQ", "M3", ContainerWrapper.TotalAllocatedShipmentVolumeUQ);

			//Second Shipment
			ContainerWrapper = DocContainer.New(FreightContainer, shipment2, Factory);
			AssertEquals("TotalAllocatedShipmentVolume", 4747.563M, ContainerWrapper.TotalAllocatedShipmentVolume);
			AssertEquals("TotalAllocatedShipmentVolumeUQ", "CF", ContainerWrapper.TotalAllocatedShipmentVolumeUQ);
		}

		public override void TestTotalPackLineVolume()
		{
			AddContainerToConsolAndShipment();
			PackLine.JL_ActualVolume = 40.23M;
			AssertEquals("TotalPackLineVolume", 40.23M, ContainerWrapper.TotalPackLineVolume);

			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_ActualVolume = 65.89M;
			packLine2.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalPackLineVolume", 106.12M, ContainerWrapper.TotalPackLineVolume);

			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_ActualVolume = 12.48M;
			FreightContainer.PackLines.Remove(packLine3);

			AssertEquals("TotalPackLineVolume", 106.12M, ContainerWrapper.TotalPackLineVolume);

			var shipment2 = Consol.Shipments.AddNew();
			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_ActualVolume = 134.4356M;
			packLine4.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalPackLineVolume", 240.556M, ContainerWrapper.TotalPackLineVolume);

			shipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			AssertEquals("TotalPackLineVolume - sub shipment volume should be included", 240.556M, ContainerWrapper.TotalPackLineVolume);
		}

		#endregion

		#region ZInt Fields

		public override void TestTotalPackLinePackages()
		{
			AddContainerToConsolAndShipment();
			PackLine.JL_PackageCount = 40;
			AssertEquals("TotalPackLinePackages", 40, ContainerWrapper.TotalPackLinePackages);

			var packLine2 = Shipment.OuterPackLines.AddNew();
			packLine2.JL_PackageCount = 60;
			packLine2.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalPackLinePackages", 100, ContainerWrapper.TotalPackLinePackages);

			var packLine3 = Shipment.OuterPackLines.AddNew();
			packLine3.JL_PackageCount = 12;
			FreightContainer.PackLines.Remove(packLine3);

			AssertEquals("TotalPackLinePackages", 100, ContainerWrapper.TotalPackLinePackages);

			var shipment2 = Consol.Shipments.AddNew();
			var packLine4 = shipment2.OuterPackLines.AddNew();
			packLine4.JL_PackageCount = 130;
			packLine4.SetContainer(Consol, FreightContainer);

			AssertEquals("TotalPackLinePackages", 230, ContainerWrapper.TotalPackLinePackages);

			shipment2.JS_JS_ColoadMasterShipment = Shipment.PK;
			AssertEquals("TotalPackLinePackages - sub shipment packages should be included", 230, ContainerWrapper.TotalPackLinePackages);
		}

		#endregion

		#endregion

		#endregion

		#endregion

		#region TestTEU

		public void TestTEU()
		{
			AssertEquals("TEU", 0M, ContainerWrapper.TEU);

			FreightContainer.JC_RC = Factory.NewWithValidTestData<RefContainer>().PK;
			FreightContainer.RefContainer.RC_TEU = 1M;
			AssertEquals("TEU", 1M, ContainerWrapper.TEU);
		}

		#endregion

		#region Implementation

		ForwardingContainer FreightContainer;
		DocContainer ContainerWrapper;
		PackLine PackLine;
		ForwardingShipment Shipment;
		ForwardingConsol Consol;

		protected override void SetUp()
		{
			if (!GlbBranch.CurrentBranch.GB_OH_OrgProxy.IsValid)
			{
				GlbBranch.CurrentBranch.GB_OH_OrgProxy = Factory.LoadTop1(typeof(OrgHeader), new ZQuery(OrgHeaderSchema.OH_Code, "EDICUS")).PK;
			}

			FreightContainer = Factory.New<ForwardingContainer>();
			ContainerWrapper = DocContainer.New(FreightContainer, Factory);

			base.SetUp();
		}

		void AddContainerToConsolAndShipment()
		{
			Consol = Factory.New<ForwardingConsol>();
			Consol.JK_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;

			Transport transport = Consol.Transports[0];
			transport.JW_VoyageFlight = "51N";
			transport.JW_Vessel = (Factory.LoadTop1<RefVessel>(new ZQuery())).RV_Code;
			transport.JW_RL_NKLoadPort = "USLAX";
			transport.JW_RL_NKDiscPort = "AUSYD";
			transport.JW_ETD = ZDateTime.Today.AddMonths(-1);
			transport.JW_ETA = ZDateTime.Today.AddDays(1);

			Consol.Containers.Add(FreightContainer);
			Shipment = Consol.Shipments.AddNew();

			PackLine = Shipment.OuterPackLines.AddNew();
			PackLine.SetContainer(Consol, FreightContainer);
			ContainerWrapper = DocContainer.New(FreightContainer, Shipment, Factory);
		}

		#endregion
	}
}
