using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.Freight;
using Enterprise.DocumentWrappersCore.Testing;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Tracking;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ForwardingShipment = Enterprise.Freight.Forwarding.Business.ForwardingShipment;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;

namespace Enterprise.DocumentWrappers.Orders.Testing
{
	[TestedType(typeof(DocOrder))]
	sealed class DocOrderTest : DocumentWrapperTestCase
	{
		public override DocumentWrapper[] GetDocumentWrappers()
		{
			return new DocumentWrapper[]
			{
				DocOrder.New(fOrder, Factory),
				DocOrder.New(GetOrderWithShipment, Factory)
			};
		}

		#region Recommended Agents - Routing Order

		public void TestRecommendedAgents()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			OrgHeader sendingAgentOverride = OrgHeader.New(newFactory);
			sendingAgentOverride.OH_Code = "TESTZUB";
			sendingAgentOverride.OH_FullName = "Some Agent";
			sendingAgentOverride.OH_RL_NKClosestPort = "AUSYD";

			OrgHeader supplier = OrgHeader.New(newFactory);
			supplier.OH_Code = "TESTSUP1";
			supplier.OH_FullName = "Test Supplier in Ahmedabad";
			supplier.OH_RL_NKClosestPort = "INAMD";

			OrgHeader buyer = OrgHeader.New(newFactory);
			buyer.OH_Code = "TESTBUY1";
			buyer.OH_FullName = "Test Buyer";
			buyer.OH_RL_NKClosestPort = "AUSYD";

			Order order = newFactory.New<Order>();
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.SupplierPK = supplier.PK;
			order.BuyerPK = buyer.PK;
			order.JD_RL_NKPortOfLoading = "INABG";

			newFactory.Save();

			DocOrganisationCollection collection;

			DocOrder docOrder = DocOrder.New(order, Factory);

			collection = docOrder.RecommendedAgents;
			AssertEquals("No recommended agents", 0, collection.Count);

			OrgHeader agentAMD = OrgHeader.New(newFactory);
			agentAMD.OH_Code = "FORW1";
			agentAMD.OH_FullName = "Forwarder 1";
			agentAMD.OH_RL_NKClosestPort = "INAMD";
			agentAMD.OH_IsForwarder = true;
			OrgAppointedAgentPorts agentPortAMD = agentAMD.AppointedAgentPorts.AddNew();
			agentPortAMD.O5_PortOrCountry = "INAMD";
			agentPortAMD.O5_SeaAgentStatus = "PUB";
			agentPortAMD.O5_OA_AgentOfficeAddress = agentAMD.MainAddress.PK;

			newFactory.Save();

			collection = docOrder.RecommendedAgents;
			AssertEquals("Recommended agent exists from Supplier", 1, collection.Count);
			AssertEquals("Should be Agent for INAMD", agentAMD.OH_Code, collection[0].Code);

			OrgHeader agentABG = OrgHeader.New(newFactory);
			agentABG.OH_Code = "FORW1";
			agentABG.OH_FullName = "Forwarder 1";
			agentABG.OH_RL_NKClosestPort = "INABG";
			agentABG.OH_IsForwarder = true;
			OrgAppointedAgentPorts agentPortABG = agentABG.AppointedAgentPorts.AddNew();
			agentPortABG.O5_PortOrCountry = "INABG";
			agentPortABG.O5_SeaAgentStatus = "PUB";
			agentPortABG.O5_OA_AgentOfficeAddress = agentABG.MainAddress.PK;

			newFactory.Save();

			collection = docOrder.RecommendedAgents;
			AssertEquals("Recommended agent exists from Load Port", 1, collection.Count);
			AssertEquals("Should be Agent for INABG", agentABG.OH_Code, collection[0].Code);

			order.JD_OH_SendingAgent = sendingAgentOverride.PK;

			newFactory.Save();

			docOrder = DocOrder.New(order, Factory);
			collection = docOrder.RecommendedAgents;
			AssertEquals("Recommended agent exists from Sending Agent", 1, collection.Count);
			AssertEquals("Should be Sending Agent", sendingAgentOverride.OH_Code, collection[0].Code);

			order.JD_TransportMode = Core.Constants.TransportModes.Air;

			newFactory.Save();

			docOrder = DocOrder.New(order, Factory);
			collection = docOrder.RecommendedAgents;
			AssertEquals("Recommended agent exists from Sending Agent", 1, collection.Count);
			AssertEquals("Should be Sending Agent", sendingAgentOverride.OH_Code, collection[0].Code);
		}

		#endregion

		#region Overrides

		public void TestToString()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			ZString orderNumber = new ZString("OrderNum");
			orderBisObj.JD_OrderNumber = orderNumber;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ToString()", orderNumber, orderWrapper.ToString());

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			shipment.JS_UniqueConsignRef = "ShipNum";
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ToString()", shipmentWrapper.ShipmentNumber, orderWrapper.ToString());

			orderBisObj.JD_OrderNumberSplit = 2;
			AssertEquals("ShipNum", orderWrapper.JobNumberAndSplit);
			AssertEquals("OrderNum-2", orderWrapper.SecondJobNumber);
			AssertEquals("SPLIT:", orderWrapper.SecondJobNumberHeading);

			orderBisObj.JD_JS = ZGuid.Empty;
			AssertEquals("OrderNum-2", orderWrapper.JobNumberAndSplit);
			AssertEquals("OrderNum-2", orderWrapper.SecondJobNumber);
		}

		#endregion

		#region Collections

		public void TestArrivalContainers()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.PlannedContainers.AddNew();
			orderBisObj.PlannedContainers.AddNew();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ArrivalContainers", 2, orderWrapper.Containers.Count);
			AssertEquals("ArrivalContainers is of type DocOrderContainer", typeof(DocOrderContainer), orderWrapper.Containers[0].GetType());
			AssertEquals("ArrivalContainers is of type DocOrderContainer", typeof(DocOrderContainer), orderWrapper.Containers[1].GetType());

			var shipment = Factory.New<ForwardingShipment>();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			ForwardingConsol arrivalConsol = shipment.Consols.AddNew();
			CommonContainer arrivalContainer1 = arrivalConsol.Containers.AddNew();
			arrivalContainer1.JC_ContainerNum = "ARRIVAL1";
			packLine1.SetContainer(arrivalConsol, arrivalContainer1);

			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			shipmentWrapper.SetDocumentDirectionForTesting(nameof(DocumentDirection.ARV));
			AssertEquals("ArrivalContainers", 1, orderWrapper.Containers.Count);
			AssertEquals("ArrivalContainers is of type DocOrderContainer", typeof(DocContainer), orderWrapper.Containers[0].GetType());
		}

		public void TestSimpleContainers()
		{
			GlbCompany.CurrentCompany.SetCountry("AU");

			var orderBisObj = Factory.New<Order>();
			OrderContainer ordCont1 = orderBisObj.PlannedContainers.AddNew();
			ordCont1.J1_ContainerNumber = "111111";
			OrderContainer ordCont2 = orderBisObj.PlannedContainers.AddNew();
			ordCont2.J1_ContainerNumber = "222222";
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("SimpleContainers", 2, orderWrapper.Containers.Count);
			AssertEquals("SimpleContainers contains OrdCont1", "111111", orderWrapper.SimpleContainers[0].ContainerNumber);
			AssertEquals("SimpleContainers contains OrdCont2", "222222", orderWrapper.SimpleContainers[1].ContainerNumber);

			BaseJobDeclaration declaration = Factory.New<BaseJobDeclaration>();
			BaseCusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "DEC12345";
			orderBisObj.JD_JE = declaration.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("SimpleContainers", 1, orderWrapper.SimpleContainers.Count);
			AssertEquals("SimpleContainer contains Declarations container", "DEC12345", orderWrapper.SimpleContainers[0].ContainerNumber);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			PackLine packLine1 = shipment.OuterPackLines.AddNew();
			ForwardingConsol consol = shipment.Consols.AddNew();
			CommonContainer container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CON1234";
			packLine1.SetContainer(consol, container2);
			orderBisObj.JD_JS = shipment.PK;
			AssertEquals("SimpleContainers", 1, orderWrapper.SimpleContainers.Count);
			AssertEquals("SimpleContainers contains Container2", "CON1234", orderWrapper.SimpleContainers[0].ContainerNumber);
		}

		public void TestOrderLines()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("No order lines", 0, orderWrapper.OrderLines.Count);

			orderBisObj.OrderLines.AddNew();
			AssertEquals("One order lines", 1, orderWrapper.OrderLines.Count);
			AssertEquals("Is type of DocOrderLine", typeof(DocOrderLine), orderWrapper.OrderLines[0].GetType());

			orderBisObj.OrderLines.AddNew();
			AssertEquals("Two order lines", 2, orderWrapper.OrderLines.Count);
			AssertEquals("Is type of DocOrderLine", typeof(DocOrderLine), orderWrapper.OrderLines[0].GetType());
			AssertEquals("Is type of DocOrderLine", typeof(DocOrderLine), orderWrapper.OrderLines[1].GetType());
		}

		public void TestAllDeliverContainers_DeliveriesSortedPerOrderLine()
		{
			var order = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();

			OrderLineDelivery delivery1 = orderLine.Deliveries.AddNew();
			delivery1.J4_OA_NKDeliveryPoint = "2222";
			OrderLineDelivery delivery2 = orderLine.Deliveries.AddNew();
			delivery2.J4_OA_NKDeliveryPoint = "3333";
			OrderLineDelivery delivery3 = orderLine.Deliveries.AddNew();
			delivery3.J4_OA_NKDeliveryPoint = "1111";

			DocOrder doc = DocOrder.New(order, Factory);
			AssertEquals("1111", doc.AllDeliverContainers[0].Delivery.DeliverPointCode);
			AssertEquals("2222", doc.AllDeliverContainers[1].Delivery.DeliverPointCode);
			AssertEquals("3333", doc.AllDeliverContainers[2].Delivery.DeliverPointCode);
		}

		public void TestOrderLinesWithUndefinedProduct()
		{
			var order = Factory.New<Order>();
			var orderWrapper = DocOrder.New(order, Factory);

			AssertEquals("Pre-Condition: No Order Lines", 0, orderWrapper.OrderLinesWithUndefinedProduct.Count);

			var orderLine1 = order.OrderLines.AddNew();

			AssertEquals("The Order Line has no part number, so no undefined product", 0, orderWrapper.OrderLinesWithUndefinedProduct.Count);

			orderLine1.JO_Partno = "RTX 3080 GPU";

			AssertEquals("The Order Line does not have a defined product", 1, orderWrapper.OrderLinesWithUndefinedProduct.Count);

			var product = OrgSupplierPart.New(Factory);
			product.OP_PartNum = "GTX 1070 GPU";

			AssertEquals("The Order Line part number and product part number do not match", 1, orderWrapper.OrderLinesWithUndefinedProduct.Count);

			product.OP_PartNum = "RTX 3080 GPU";

			AssertEquals("There is now a matching product", 0, orderWrapper.OrderLinesWithUndefinedProduct.Count);
		}

		#endregion

		#region ZString Fields

		public void TestAllClientVisibleNotes()
		{
			var buyer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			fOrder.BuyerPK = buyer.PK;
			fOrder.SupplierPK = supplier.PK;

			var orderNote = fOrder.Notes.AddNew();
			var nonVisibleOrderNote = fOrder.Notes.AddNew();
			orderNote.ST_Description = "xxx";
			orderNote.ST_NoteType = nameof(StmNoteVisibility.PUB);
			orderNote.ST_NoteDataAsText = "order";

			var buyerNote = buyer.Notes.AddNew();
			var nonVisibleBuyerNote = buyer.Notes.AddNew();
			buyerNote.ST_Description = "xxx";
			buyerNote.ST_NoteType = nameof(StmNoteVisibility.PUB);
			buyerNote.ST_NoteDataAsText = "buyer";

			var supplierNote = supplier.Notes.AddNew();
			var nonVisibleSupplierNote = supplier.Notes.AddNew();
			supplierNote.ST_Description = "xxx";
			supplierNote.ST_NoteType = nameof(StmNoteVisibility.PUB);
			supplierNote.ST_NoteDataAsText = "supplier";

			var docOrder = DocOrder.New(fOrder, Factory);
			var allNoteText = docOrder.AllClientVisibleNotes;
			Assert("Should contain the order note", allNoteText.IndexOf(orderNote.ST_NoteDataAsText) != -1);
			Assert("Should contain the buyer note", allNoteText.IndexOf(buyerNote.ST_NoteDataAsText) != -1);
			Assert("Should contain the supplier note", allNoteText.IndexOf(supplierNote.ST_NoteDataAsText) != -1);
		}

		public void TestOrderManagementUpdateNote()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			var order = newFactory.New<Order>();
			order.BuyerPK = GlbCompany.CurrentCompany.OrgProxy.PK;
			StmNote note1 = order.Notes.AddNew();
			note1.ST_NoteDataAsText = "Bob";
			note1.ST_NoteText = "This is the text of not a note";
			newFactory.Save();
			DocOrder docOrder = DocOrder.New(order, newFactory);
			AssertEquals("Should be empty note", "", docOrder.MostRecentOrderManagementUpdateNote);

			Thread.Sleep(10); // Ensure that sequential notes do not have the same date

			StmNote note2 = order.Notes.AddNew();
			note2.ST_Description = PredefinedNoteTypes.Instance.OrderManagementUpdate.Description;
			note2.ST_NoteDataAsText = "First Order Note";
			newFactory.Save();
			AssertEquals("Should be 'First Order Note'", note2.ST_NoteText, docOrder.MostRecentOrderManagementUpdateNote);

			Thread.Sleep(10); // Ensure that sequential notes do not have the same date

			StmNote note3 = order.Notes.AddNew();
			note3.ST_Description = PredefinedNoteTypes.Instance.OrderManagementUpdate.Description;
			note3.ST_NoteDataAsText = "Second More recent Order Note";
			newFactory.Save();
			Assert("PreCondition: Second Note should be more recent", note3.ST_CreatedDateUtc > note2.ST_CreatedDateUtc);
			AssertEquals("Should be second note'", note3.ST_NoteText, docOrder.MostRecentOrderManagementUpdateNote);

			Thread.Sleep(10); // Ensure that sequential notes do not have the same date

			StmNote note4 = order.Notes.AddNew();
			note4.ST_Description = PredefinedNoteTypes.Instance.OrderManagementUpdate.Description;
			note4.ST_NoteDataAsText = "Third More recent Order Note";
			newFactory.Save();
			AssertEquals("Should be third note'", note4.ST_NoteText, docOrder.MostRecentOrderManagementUpdateNote);
		}

		public void TestClientVisibleJobNotes()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);

			var marksAndNumberNote1 = orderBisObj.Notes.AddNew();
			marksAndNumberNote1.ST_Description = PredefinedNoteTypes.Instance.ClientVisibleJobNotes.Description;
			marksAndNumberNote1.ST_ParentID = orderBisObj.PK;
			marksAndNumberNote1.ST_Table = orderBisObj.TableName;
			marksAndNumberNote1.ST_NoteDataAsText = "Client Visible Job Notes";

			AssertEquals("Notes is not empty", "Client Visible Job Notes", orderWrapper.ClientVisibleJobNotes);
		}

		public void TestOrderAndSplitNumber()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_OrderNumber = "OrderNum";
			orderBisObj.JD_OrderNumberSplit = 1;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("OrderAndSplitNumber", "OrderNum-1", orderWrapper.OrderAndSplitNumber);
		}

		public void TestAdditionalTerms()
		{
			var orderBisObj = Factory.New<Order>();
			ZString additionalTerms = new ZString("AdditionalTerms");
			orderBisObj.JD_AdditionalTerms = additionalTerms;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("AdditionalTerms", additionalTerms, orderWrapper.AdditionalTerms);
		}

		public void TestArrivalVoyage()
		{
			var orderBisObj = Factory.New<Order>();
			ZString arrivalVoyage = new ZString("Arrival");
			orderBisObj.JD_ArrivalVoyage = arrivalVoyage;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ArrivalVoyage", arrivalVoyage, orderWrapper.ArrivalVoyage);

			CommonConsol consol = OrderWithShip.Shipment.Consols[0];
			consol.JK_RL_NKDischargePort = GlbCompany.CurrentCompany.OrgProxy.UNLOCO.RL_Code;

			Transport transport = consol.Transports[0];
			transport.JW_VoyageFlight = arrivalVoyage;
			OrderWithShip.Shipment.JS_RL_NKDestination = GlbCompany.CurrentCompany.OrgProxy.UNLOCO.RL_Code;
			AssertEquals("Should get voyage from shipment consol", arrivalVoyage, OrderWrapperWithShip.ArrivalVoyage);

			OrderWithDec.Declaration[JobDeclarationSchema.JE_VoyageFlightNo.Name] = arrivalVoyage;
			AssertEquals("Should get voyage from Declaration", arrivalVoyage, OrderWrapperWithDec.ArrivalVoyage);
		}

		public void TestBookingConfRef()
		{
			var orderBisObj = Factory.New<Order>();
			ZString bookingConfRef = new ZString("BookingConfRef");
			orderBisObj.JD_BookingConfRef = bookingConfRef;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("BookingConfRef", bookingConfRef, orderWrapper.BookingConfRef);
		}

		public void TestContainerMode()
		{
			var orderBisObj = Factory.New<Order>();
			ZString containerMode = new ZString("CCC");
			orderBisObj.JD_ContainerMode = containerMode;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ContainerMode", containerMode, orderWrapper.ContainerMode);

			OrderWithShip.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			OrderWithShip.Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			AssertEquals("Should get Container mode from Shipment", OrderWithShip.Shipment.JS_PackingMode, OrderWrapperWithShip.ContainerMode);

			OrderWithDec.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			OrderWithDec.Declaration[JobDeclarationSchema.JE_ContainerMode.Name] = Core.Constants.ContainerModes.LCL;
			AssertEquals("Should get container mode from declaration", OrderWithDec.Declaration[JobDeclarationSchema.JE_ContainerMode.Name], OrderWrapperWithDec.ContainerMode);
		}

		public void TestCustomAttrib1()
		{
			var orderBisObj = Factory.New<Order>();
			ZString customAttrib1 = new ZString("CustomAttrib1");
			orderBisObj.JD_CustomAttrib1 = customAttrib1;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomAttrib1", customAttrib1, orderWrapper.CustomAttrib1);
		}

		public void TestCustomAttrib2()
		{
			var orderBisObj = Factory.New<Order>();
			ZString customAttrib2 = new ZString("CustomAttrib2");
			orderBisObj.JD_CustomAttrib2 = customAttrib2;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomAttrib2", customAttrib2, orderWrapper.CustomAttrib2);
		}

		public void TestCustomAttrib3()
		{
			var orderBisObj = Factory.New<Order>();
			ZString customAttrib3 = new ZString("CustomAttrib3");
			orderBisObj.JD_CustomAttrib3 = customAttrib3;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomAttrib3", customAttrib3, orderWrapper.CustomAttrib3);
		}

		public void TestCustomAttrib4()
		{
			var orderBisObj = Factory.New<Order>();
			ZString customAttrib4 = new ZString("CustomAttrib4");
			orderBisObj.JD_CustomAttrib4 = customAttrib4;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomAttrib4", customAttrib4, orderWrapper.CustomAttrib4);
		}

		public void TestCustomAttrib5()
		{
			var orderBisObj = Factory.New<Order>();
			ZString customAttrib5 = new ZString("CustomAttrib5");
			orderBisObj.JD_CustomAttrib5 = customAttrib5;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomAttrib5", customAttrib5, orderWrapper.CustomAttrib5);
		}

		public void TestFirstBuyerContact()
		{
			var orderBisObj = Factory.New<Order>();
			ZString firstBuyerContact = new ZString("FirstBuyerContact");
			orderBisObj.JD_FirstBuyerContact = firstBuyerContact;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("FirstBuyerContact", firstBuyerContact, orderWrapper.FirstBuyerContact);
		}

		public void TestSecondBuyerContact()
		{
			var orderBisObj = Factory.New<Order>();
			ZString secondBuyerContact = new ZString("SecondBuyerContact");
			orderBisObj.JD_SecondBuyerContact = secondBuyerContact;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("SecondBuyerContact", secondBuyerContact, orderWrapper.SecondBuyerContact);
		}

		public void TestDepartureVoyage()
		{
			var orderBisObj = Factory.New<Order>();
			ZString departureVoyage = new ZString("Departure");
			orderBisObj.JD_DepartureVoyage = departureVoyage;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("DepartureVoyage", departureVoyage, orderWrapper.DepartureVoyage);
		}

		public void TestIncoTerm()
		{
			var orderBisObj = Factory.New<Order>();
			ZString incoTerm = new ZString("III");
			orderBisObj.JD_IncoTerm = incoTerm;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("IncoTerm", incoTerm, orderWrapper.IncoTerm);

			OrderWithShip.JD_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			OrderWithShip.Shipment.JS_INCO = Core.Constants.IncoTerms.CarriagePaidTo;
			AssertEquals("Order Wrapper should get Inco term from Shipment", Core.Constants.IncoTerms.CarriagePaidTo, OrderWrapperWithShip.IncoTerm);

			OrderWithDec.JD_IncoTerm = Core.Constants.IncoTerms.CostAndFreight;
			OrderWithDec.Declaration[BaseJobDeclaration.Schema.JE_ShipmentIncoTerm] = Core.Constants.IncoTerms.CarriagePaidTo;
			AssertEquals("Order Wrapper should get Inco term from Dec", Core.Constants.IncoTerms.CarriagePaidTo, OrderWrapperWithDec.IncoTerm);
		}

		public void TestIntermediateVoyage()
		{
			var orderBisObj = Factory.New<Order>();
			ZString intermediateVoyage = new ZString("Voyage");
			orderBisObj.JD_IntermediateVoyage = intermediateVoyage;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("IntermediateVoyage", intermediateVoyage, orderWrapper.IntermediateVoyage);
		}

		public void TestInvoiceNumber()
		{
			var orderBisObj = Factory.New<Order>();
			ZString invoiceNumber = new ZString("InvoiceNum");
			orderBisObj.JD_InvoiceNumber = invoiceNumber;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("InvoiceNumber", invoiceNumber, orderWrapper.InvoiceNumber);
		}

		public void TestOrderStatus()
		{
			var orderBisObj = Factory.New<Order>();
			ZString orderStatus = new ZString("OOO");
			orderBisObj.JD_OrderStatus = orderStatus;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("OrderStatus", orderStatus, orderWrapper.OrderStatus);
		}

		public void TestServiceLevelCode()
		{
			var orderBisObj = Factory.New<Order>();
			ZString serviceLevelCode = new ZString("CCC");
			orderBisObj.JD_RS_NKServiceLevel_NI = serviceLevelCode;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ServiceLevelCode", serviceLevelCode, orderWrapper.ServiceLevelCode);

			OrderWithShip.Shipment.JS_RS_NKServiceLevel = serviceLevelCode;
			AssertEquals("Order Wrapper should get service level from Shipment", serviceLevelCode, OrderWrapperWithShip.ServiceLevelCode);
		}

		public void TestServiceLevelDescription()
		{
			var orderBisObj = Factory.New<Order>();

			var serviceLevel = Factory.LoadTop1<RefServiceLevel>(new ZQuery());
			orderBisObj.JD_RS_NKServiceLevel_NI = serviceLevel.RS_Code;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ServiceLevelDescription", serviceLevel.RS_Description, orderWrapper.ServiceLevelDescription);

			orderBisObj.JD_RS_NKServiceLevel_NI = ZString.Empty;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ServiceLevelDescription", ZString.Empty, orderWrapper.ServiceLevelDescription);

			OrderWithShip.Shipment.JS_RS_NKServiceLevel = serviceLevel.RS_Code;
			AssertEquals("Order wrapper should get description based of Code on Shipment", serviceLevel.RS_Description, OrderWrapperWithShip.ServiceLevelDescription);

			OrderWithShip.Shipment.JS_RS_NKServiceLevel = "LZP";
			AssertEquals("Order wrapper should not explode on invalid shipment code", ZString.Empty, OrderWrapperWithShip.ServiceLevelDescription);
		}

		public void TestShipmentContacts()
		{
			AssertEquals("Should be empty string without shipment", ZString.Empty, OrderWrapper.ShipmentConsigneeContact);
			AssertEquals("Should be empty string without shipment", ZString.Empty, OrderWrapper.ShipmentConsignorContact);

			AssertEquals("Should be empty string with shipment without contact set", ZString.Empty, OrderWrapperWithShip.ShipmentConsigneeContact);
			AssertEquals("Should be empty string with shipment without contact set", ZString.Empty, OrderWrapperWithShip.ShipmentConsigneeContact);

			OrderWithShip.Shipment.ConsignorDocumentaryAddress.E2_Contact = "Bob";
			OrderWithShip.Shipment.ConsigneeDocumentaryAddress.E2_Contact = "Geoff";

			AssertEquals("Should be from Shipment Consignee Contact", OrderWithShip.Shipment.ConsigneeDocumentaryAddress.E2_Contact, OrderWrapperWithShip.ShipmentConsigneeContact);
			AssertEquals("Should be from Shipment Consignor Contact", OrderWithShip.Shipment.ConsignorDocumentaryAddress.E2_Contact, OrderWrapperWithShip.ShipmentConsignorContact);
		}

		public void TestConsigneeContactsWithOverride()
		{
			AssertNull("Should be empty string without shipment", OrderWrapper.ConsigneeContact);
			AssertNull("Should be empty string with shipment without contact set", OrderWrapperWithShip.ConsigneeContact);

			OrderWithShip.Shipment.ConsigneeDocumentaryAddress.E2_Contact = "Bob";
			AssertEquals("Should be from Shipment Consignee Contact", OrderWithShip.Shipment.ConsigneeDocumentaryAddress.E2_Contact, OrderWrapperWithShip.ConsigneeContact.Name);
		}

		public void TestConsignorContactsWithOverride()
		{
			AssertNull("Should be empty string without shipment", OrderWrapper.ConsignorContact);
			AssertNull("Should be empty string with shipment without contact set", OrderWrapperWithShip.ConsignorContact);

			OrderWithShip.Shipment.ConsignorDocumentaryAddress.E2_Contact = "Bob";
			AssertEquals("Should be from Shipment Consignor Contact", OrderWithShip.Shipment.ConsignorDocumentaryAddress.E2_Contact, OrderWrapperWithShip.ConsignorContact.Name);
		}

		public void TestArrivalVessel()
		{
			var orderBisObj = Factory.New<Order>();

			ZString arrivalVessel = new ZString("ArrivalVessel");
			orderBisObj.JD_RV_NKArrivalVessel = arrivalVessel;

			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ArrivalVessel", arrivalVessel, orderWrapper.ArrivalVessel);

			CommonConsol consol = OrderWithShip.Shipment.Consols[0];
			consol.JK_RL_NKDischargePort = GlbCompany.CurrentCompany.OrgProxy.UNLOCO.RL_Code;

			Transport transport = consol.Transports[0];
			transport.JW_Vessel = arrivalVessel;
			OrderWithShip.Shipment.JS_RL_NKDestination = GlbCompany.CurrentCompany.OrgProxy.UNLOCO.RL_Code;
			OrderWithDec.Declaration[JobDeclarationSchema.JE_VesselName.Name] = arrivalVessel;
			AssertEquals("Should get vessel from Shipment Consol", arrivalVessel, OrderWrapperWithShip.ArrivalVessel);
			AssertEquals("Should get vessel from Declaration", arrivalVessel, OrderWrapperWithDec.ArrivalVessel);
		}

		public void TestSailing()
		{
			var order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;

			DocOrder orderWrapper = DocOrder.New(order1, Factory);
			AssertArrivalFieldsEmptyOnOrder(orderWrapper);

			order1.JD_RV_NKArrivalVessel = "ArrivalVessel";
			order1.JD_ArrivalVoyage = "312";
			order1.UpdateEventEstimate(Events.Arrival, ZDateTimeOffset.Now.AddDays(2));
			order1.JD_E_DEP_3 = ZDateTime.Now;
			AssertEquals("Should be Arrival Vessel", order1.JD_RV_NKArrivalVessel, orderWrapper.ArrivalVessel);
			AssertEquals("Should have Arrival Voyage", order1.JD_ArrivalVoyage, orderWrapper.ArrivalVoyage);
			AssertEquals("Should be Arrival ETA", order1.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime(), orderWrapper.EstimatedARV);
			AssertEquals("Should be Arrival ETD", order1.JD_E_DEP_3, orderWrapper.ArrivalETD);

			FreightHelper helper = new FreightHelper();
			ForwardingShipment ship = (ForwardingShipment)helper.GetImportShipment(typeof(ForwardingShipment), Factory);
			order1.JD_JS = ship.PK;
			Factory.Save();
			AssertArrivalFieldsEmptyOnOrder(orderWrapper);

			var consol1 = Factory.New<ForwardingConsol>();
			ship.Consols.Add(consol1);
			Factory.Save();
			AssertArrivalFieldsEmptyOnOrder(orderWrapper);

			consol1.JK_RL_NKDischargePort = ship.JS_RL_NKDestination;
			Transport transport1 = consol1.Transports[0];
			transport1.JW_Vessel = "VesselOfArrival";
			transport1.JW_VoyageFlight = "1";
			transport1.JW_ETD = ZDateTime.SmallDateTimeNow;
			transport1.JW_ETA = ZDateTime.SmallDateTimeNow.AddDays(2);
			Factory.Save();
			AssertEquals("Should have Consol Vessel", consol1.JK_JX_JV_NKVessel, orderWrapper.ArrivalVessel);
			AssertEquals("Should be Consol Voyage", consol1.JK_JX_JV_VoyageFlight, orderWrapper.ArrivalVoyage);
			AssertEquals("Should be Consol ETD", consol1.JK_JX_JA_E_DEP, orderWrapper.ArrivalETD);
			AssertEquals("Should be Consol ETA", consol1.JK_JX_JB_E_ARV, orderWrapper.EstimatedARV);

			var consol2 = Factory.New<ForwardingConsol>();
			ship.Consols.Add(consol2);

			consol2.JK_RL_NKDischargePort = ship.JS_RL_NKDestination;
			consol1.JK_RL_NKDischargePort = ship.JS_RL_NKOrigin;
			consol1.JK_RL_NKLoadPort = ship.JS_RL_NKOrigin;
			Factory.Save();
			AssertEquals("PreRequisite: Consol2 should be Arrival Consol", consol2, ship.ArrivalConsol);
			AssertEquals("PreRequisite: Consol1 should be Departure Consol", consol1, ship.DepartureConsol);

			Transport transport2 = consol2.Transports[0];
			transport2.JW_Vessel = "VesselOfArrival";
			transport2.JW_VoyageFlight = "2342";
			transport2.JW_ETD = ZDateTime.SmallDateTimeNow.AddDays(5);
			transport2.JW_ETA = ZDateTime.SmallDateTimeNow.AddDays(6);
			Factory.Save();
			AssertEquals("Should be Consol2 Vessel", consol2.JK_JX_JV_NKVessel, orderWrapper.ArrivalVessel);
			AssertEquals("Should be Consol2 Voyage", consol2.JK_JX_JV_VoyageFlight, orderWrapper.ArrivalVoyage);
			AssertEquals("Should be Consol2 ETD", consol2.JK_JX_JA_E_DEP, orderWrapper.ArrivalETD);
			AssertEquals("Should be Consol2 ETA", consol2.JK_JX_JB_E_ARV, orderWrapper.EstimatedARV);
			AssertEquals("Should be Consol1 Vessel", consol1.JK_JX_JV_NKVessel, orderWrapper.DepartureVessel);
			AssertEquals("Should be Consol1 Voyage", consol1.JK_JX_JV_VoyageFlight, orderWrapper.DepartureVoyage);
			AssertEquals("Should be Consol1 ETD", consol1.JK_JX_JA_E_DEP, orderWrapper.EstimatedDEP);
			AssertEquals("Should be Consol1 ETA", consol1.JK_JX_JB_E_ARV, orderWrapper.DepartureETA);

			var consol3 = Factory.New<ForwardingConsol>();
			ship.Consols.Add(consol3);

			Transport transport3 = consol3.Transports[0];
			transport3.JW_Vessel = "IntermediateVessel";
			transport3.JW_VoyageFlight = "3343";
			transport3.JW_ETD = ZDateTime.SmallDateTimeNow.AddDays(3);
			transport3.JW_ETA = ZDateTime.SmallDateTimeNow.AddDays(4);
			Factory.Save();
			AssertEquals("Should be Consol3 Vessel", consol3.JK_JX_JV_NKVessel, orderWrapper.IntermediateVessel);
			AssertEquals("Should be Consol3 Voyage", consol3.JK_JX_JV_VoyageFlight, orderWrapper.IntermediateVoyage);
			AssertEquals("Should be Consol3 ETD", consol3.JK_JX_JA_E_DEP, orderWrapper.IntermediateETD);
			AssertEquals("Should be Consol3 ETA", consol3.JK_JX_JB_E_ARV, orderWrapper.IntermediateETA);

			var consol4 = Factory.New<ForwardingConsol>();
			ship.Consols.Add(consol4);

			Transport transport4 = consol4.Transports[0];
			transport4.JW_Vessel = "GoobVessel";
			transport4.JW_VoyageFlight = "4444";
			transport4.JW_ETA = ZDateTime.SmallDateTimeNow.AddDays(7);
			transport4.JW_ETD = ZDateTime.SmallDateTimeNow.AddDays(8);
			Factory.Save();
			AssertEquals("Shouldn't be Empty", false, orderWrapper.IntermediateVessel.IsEmpty);
			AssertEquals("Shouldn't be Empty", false, orderWrapper.IntermediateVoyage.IsEmpty);
			AssertEquals("Shouldn't be Empty", false, orderWrapper.IntermediateETA.IsEmpty);
			AssertEquals("Shouldn't be Empty", false, orderWrapper.IntermediateETD.IsEmpty);

			var ship2 = Factory.New<ForwardingShipment>();
			ship2.JS_RL_NKOrigin = ship.JS_RL_NKDestination;
			var foreignPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, ship.JS_RL_NKDestination.Substring(0, 2)));
			ship2.JS_RL_NKDestination = foreignPort.RL_Code;
			order1.JD_JS = ship2.PK;
			var exportConsol = Factory.New<ForwardingConsol>();

			exportConsol.JK_RL_NKDischargePort = ship2.Destination.RL_Code;
			exportConsol.JK_RL_NKLoadPort = ship2.Origin.RL_Code;
			ship2.Consols.Add(exportConsol);

			Factory.Save();

			Transport transport = exportConsol.Transports[0];
			transport.JW_Vessel = "ExpoVessel";
			transport.JW_VoyageFlight = "4245";
			transport.JW_ETA = ZDateTime.Now.AddDays(6);
			transport.JW_ETD = ZDateTime.Now.AddDays(7);
			Factory.Save();
			AssertEquals("Should be ExportConsol Vessel", exportConsol.JK_JX_JV_NKVessel, orderWrapper.DepartureVessel);
			AssertEquals("Should be ExportConsol Voyage", exportConsol.JK_JX_JV_VoyageFlight, orderWrapper.DepartureVoyage);
			AssertEquals("Should be ExportConsol ETD", exportConsol.JK_JX_JA_E_DEP.ToSmallDateTimeFloor(), orderWrapper.EstimatedDEP.ToSmallDateTimeFloor());
			AssertEquals("Should be ExportConsol ETA", exportConsol.JK_JX_JB_E_ARV, orderWrapper.DepartureETA);

			var dec = Factory.New<BaseJobDeclaration>();
			dec.JE_MessageType = "EXP";
			Assert("PreRequisite: Dec needs to be Export", dec.IsExport);
			dec.JE_VesselName = "A Vessel";
			dec.JE_VoyageFlightNo = "3243";
			dec.JE_DateAtOrigin = ZDateTime.SmallDateTimeNow;
			dec.JE_DateAtFinalDestination = ZDateTime.SmallDateTimeNow.AddDays(3);
			order1.JD_JS = ZGuid.Empty;
			order1.JD_JE = dec.PK;
			Factory.Save();
			AssertEquals("Should be Dec Vessel", dec.JE_VesselName, orderWrapper.DepartureVessel);
			AssertEquals("Should be Dec Voyage", dec.JE_VoyageFlightNo, orderWrapper.DepartureVoyage);
			AssertEquals("Should be Dec ETD", dec.JE_DateAtOrigin, orderWrapper.EstimatedDEP);
			AssertEquals("Should be Dec ETA", dec.JE_DateAtFinalDestination, orderWrapper.DepartureETA);

			dec.JE_MessageType = "IMP";
			Factory.Save();
			Assert("PreRequisite: Dec needs to be Import", dec.IsImport);
			AssertEquals("Should be Dec Vessel", dec.JE_VesselName, orderWrapper.ArrivalVessel);
			AssertEquals("Should be Dec Voyage", dec.JE_VoyageFlightNo, orderWrapper.ArrivalVoyage);
			AssertEquals("Should be Dec ETD", dec.JE_DateAtOrigin, orderWrapper.ArrivalETD);
			AssertEquals("Should be Dec ETA", dec.JE_DateAtFinalDestination, orderWrapper.EstimatedARV);
		}

		public void AssertArrivalFieldsEmptyOnOrder(DocOrder orderWrapper)
		{
			AssertEquals("Vessel Should be Empty", ZString.Empty, orderWrapper.ArrivalVessel);
			AssertEquals("Voyage Should be Empty", ZString.Empty, orderWrapper.ArrivalVoyage);
		}

		public class FreightHelper : Enterprise.Freight.Business.Testing.BaseFreightTest
		{
		}

		public void TestDepartureVessel()
		{
			var orderBisObj = Factory.New<Order>();
			ZString departureVessel = new ZString("DepartureVessel");
			orderBisObj.JD_RV_NKDepartureVessel = departureVessel;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("DepartureVessel", departureVessel, orderWrapper.DepartureVessel);
		}

		public void TestIntermediateVessel()
		{
			var orderBisObj = Factory.New<Order>();
			ZString intermediateVessel = new ZString("IntermediateVessel");
			orderBisObj.JD_RV_NKIntermediateVessel = intermediateVessel;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("IntermediateVessel", intermediateVessel, orderWrapper.IntermediateVessel);
		}

		public void TestTransportMode()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_TransportMode = Core.Constants.TransportModes.Air;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("TransportMode", Core.Constants.TransportModes.Air, orderWrapper.TransportMode);
			AssertEquals("TransportModeDescription", "Air Freight", orderWrapper.TransportModeDescription);

			OrderWithShip.JD_TransportMode = Core.Constants.TransportModes.Sea;
			OrderWithShip.Shipment.JS_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Order Wrapper should get transport mode from shipment", Core.Constants.TransportModes.Air, OrderWrapperWithShip.TransportMode);
			AssertEquals("Order Wrapper should get transport mode from shipment", "Air Freight", OrderWrapperWithShip.TransportModeDescription);

			OrderWithDec.JD_TransportMode = Core.Constants.TransportModes.Sea;
			OrderWithDec.Declaration[BaseJobDeclaration.Schema.JE_TransportMode] = Core.Constants.TransportModes.Air;
			AssertEquals("Order Wrapper should get transport mode from Dec", Core.Constants.TransportModes.Air, OrderWrapperWithDec.TransportMode);
			AssertEquals("Order Wrapper should get transport mode from Dec", "Air Freight", OrderWrapperWithShip.TransportModeDescription);
		}

		public void TestPortOfLoadingName()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			orderBisObj.JD_RL_NKPortOfLoading = uNLOCO.Code;
			AssertEquals("PortOfLoadingName", uNLOCO.RL_PortName, orderWrapper.PortOfLoadingName);

			orderBisObj.JD_RL_NKPortOfLoading = ZString.Empty;
			AssertEquals("PortOfLoadingName", ZString.Empty, orderWrapper.PortOfLoadingName);
		}

		public void TestJobNumberHeading()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("JobNumberHeader", "ORDER", orderWrapper.JobNumberHeading);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("JobNumberHeader", shipmentWrapper.JobNumberHeading, orderWrapper.JobNumberHeading);
		}

		public void TestJobNumber()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			ZString orderNumber = new ZString("OrderNum");
			orderBisObj.JD_OrderNumber = orderNumber;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ShipmentNumber", orderNumber, orderWrapper.JobNumber);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			shipment.JS_UniqueConsignRef = "ShipNum";
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ShipmentNumber", shipmentWrapper.ShipmentNumber, orderWrapper.JobNumber);
		}

		public void TestUltimateNotification()
		{
			AssertEquals("UltimateNotification", ZString.Empty, OrderWrapper.UltimateNotification);

			ForwardingShipment shipment = AddShipmentToOrderAndGetShipment;
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("UltimateNotification", shipmentWrapper.UltimateNotification, OrderWrapper.UltimateNotification);
		}

		public void TestStorageStartsDate()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ImportContainerStorageCommenceDate", "To Be Advised", orderWrapper.StorageStartsDate);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ImportContainerStorageCommenceDate", shipmentWrapper.StorageCommenceDate.ToShortDateString(), orderWrapper.StorageStartsDate);
		}

		public void TestReleaseType()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ReleaseType", "To Be Advised", orderWrapper.ReleaseType);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ReleaseType", shipmentWrapper.ReleaseType, orderWrapper.ReleaseType);
		}

		public void TestOrderNumbers()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_OrderNumber = "OrderNumber";
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("OrderNumbers", "OrderNumber", orderWrapper.OrderNumbers);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocForwardingShipment shipmentWrapper = DocForwardingShipment.New(shipment, Factory);
			AssertEquals("OrderNumbers", shipmentWrapper.OrderNumbers, orderWrapper.OrderNumbers);
		}

		public void TestGoodsDescription()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_OrderGoodsDescription = "GoodsDescription";
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("GoodsDescription", "GoodsDescription", orderWrapper.GoodsDescription);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			shipment.JS_GoodsDescription = "Description";
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("GoodsDescription", shipmentWrapper.GoodsDescription, orderWrapper.GoodsDescription);

			ZString description = "Goods Desctiption";
			OrderWithDec.Declaration[BaseJobDeclaration.Schema.JE_GoodsDescription] = description;
			AssertEquals("Order Wrapper should get Goods Description from Dec", description, OrderWrapperWithDec.GoodsDescription);
		}

		public void TestCommodityCode()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CommodityCode", "To Be Advised", orderWrapper.CommodityCode);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("CommodityCode", shipmentWrapper.Commodity.Code, orderWrapper.CommodityCode);
		}

		public void TestCommodityDescription()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CommodityDescription", ZString.Empty, orderWrapper.CommodityDescription);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("CommodityDescription", shipmentWrapper.Commodity.Description, orderWrapper.CommodityDescription);
		}

		public void TestWeight()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_ActualWeight = 28;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("Weight", "28", orderWrapper.Weight);

			orderBisObj.JD_ActualWeight = new ZDecimal(12.23);
			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			orderBisObj.JD_JS = ZGuid.Empty;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("Weight", "12.23", orderWrapper.Weight);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Weight", shipmentWrapper.Weight, orderWrapper.Weight);
		}

		public void TestWeightUnit()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_UnitOfWeight = "GM";
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("WeightUnit", "GM", orderWrapper.WeightUnit);

			orderBisObj.JD_UnitOfWeight = "KG";
			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			orderBisObj.JD_JS = ZGuid.Empty;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("WeightUnit", "KG", orderWrapper.WeightUnit);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("WeightUnit", shipmentWrapper.WeightUnit, orderWrapper.WeightUnit);
		}

		public void TestVolume()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_ActualVolume = 27;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("Volume", "27", orderWrapper.Volume);

			orderBisObj.JD_ActualVolume = new ZDecimal(12.23);
			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			orderBisObj.JD_JS = ZGuid.Empty;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("Volume", "12.23", orderWrapper.Volume);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Volume", shipmentWrapper.Volume, orderWrapper.Volume);
		}

		public void TestVolumeUnit()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_UnitOfVolume = "ML";
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("VolumeUnit", "ML", orderWrapper.VolumeUnit);

			orderBisObj.JD_UnitOfVolume = "M3";
			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			orderBisObj.JD_JS = ZGuid.Empty;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("VolumeUnit", "M3", orderWrapper.VolumeUnit);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("VolumeUnit", shipmentWrapper.VolumeUnit, orderWrapper.VolumeUnit);
		}

		public void TestChargeable()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("Chargeable", "To Be Advised", orderWrapper.Chargeable);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Chargeable", shipmentWrapper.Chargeable, orderWrapper.Chargeable);
		}

		public void TestChargeableUnit()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ChargeableUnit", ZString.Empty, orderWrapper.ChargeableUnit);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ChargeableUnit", shipmentWrapper.ChargeableUnit, orderWrapper.ChargeableUnit);
		}

		public void TestHeadingTransportMode()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_TransportMode = Core.Constants.TransportModes.Air;

			ZString result = orderBisObj.JD_TransportMode_List.GetDescriptionFromCode(orderBisObj.JD_TransportMode);
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HeadingTransportMode", result, orderWrapper.HeadingTransportMode);

			orderBisObj.JD_TransportMode = Core.Constants.TransportModes.Sea;
			orderBisObj.JD_ContainerMode = ZString.Empty;

			result = orderBisObj.JD_TransportMode_List.GetDescriptionFromCode(orderBisObj.JD_TransportMode);
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HeadingTransportMode", result, orderWrapper.HeadingTransportMode);

			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			result = orderBisObj.JD_ContainerMode + " " + orderBisObj.JD_TransportMode_List.GetDescriptionFromCode(orderBisObj.JD_TransportMode);
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HeadingTransportMode", result, orderWrapper.HeadingTransportMode);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("HeadingTransportMode", shipmentWrapper.HeadingTransportMode, orderWrapper.HeadingTransportMode);
		}

		public void TestFormattedOriginLoco()
		{
			var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			ZString formattedPort = port.RL_Code + " = " + port.Description + ", " + port.Country.Description;
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_RL_NKPortOfLoading = port.RL_Code;
			orderBisObj.JD_RL_NKGoodsAvailableAt = port.RL_Code;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("Origin", formattedPort, orderWrapper.Origin);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = port.RL_Code;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Origin", shipmentWrapper.Origin, orderWrapper.Origin);

			OrderWithDec.Declaration[BaseJobDeclaration.Schema.JE_RL_NKOrigin] = port.RL_Code;
			AssertEquals("Order Wrapper should get Origin from Dec", formattedPort, OrderWrapperWithDec.Origin);
		}

		public void TestCollectedFromETDString()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.UpdateEventEstimate(Events.Departure, new ZDateTimeOffset(2004, 04, 04));
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ETDString", orderBisObj.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime().ToShortDateString(), orderWrapper.CollectedFromETDString);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_E_DEP = new ZDateTime(2004, 05, 05);
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ETDString", shipmentWrapper.ETDString, orderWrapper.CollectedFromETDString);
		}

		public void TestHouseBill()
		{
			ZString houseBill = "HouseBill";

			fOrder.JD_JS = ZGuid.Empty;
			fOrder.JD_JE = ZGuid.Empty;
			fOrder.JD_Waybill = ZString.Empty;
			AssertEquals("HouseBill", "To Be Advised", OrderWrapper.HouseBill);

			fOrder.JD_Waybill = houseBill;
			AssertEquals("HouseBill", houseBill, OrderWrapper.HouseBill);

			OrderWithShip.Shipment.JS_HouseBill = houseBill;
			AssertEquals("Order Wrapper should get value from Ship", houseBill.ToUpper(), OrderWrapperWithShip.HouseBill);

			OrderWithDec.Declaration[BaseJobDeclaration.Schema.JE_HouseBill] = houseBill;
			AssertEquals("Order Wrapper should get value from Dec", houseBill, OrderWrapperWithDec.HouseBill);
		}

		public void TestDestination()
		{
			var port = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			ZString formattedPort = port.RL_Code + " = " + port.Description + ", " + port.Country.Description;
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.JD_RL_NKGoodsDeliveredTo = port.RL_Code;

			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("Destination", formattedPort, orderWrapper.Destination);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKDestination = port.RL_Code;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Destination", shipmentWrapper.Destination, orderWrapper.Destination);

			OrderWithDec.Declaration[BaseJobDeclaration.Schema.JE_RL_NKFinalDestination] = port.RL_Code;
			AssertEquals("Destination should come from Dec", formattedPort, OrderWrapperWithDec.Destination);
		}

		public void TestDischargeETAString()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.UpdateEventEstimate(Events.Arrival, new ZDateTimeOffset(2004, 04, 04));
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ETAString", orderBisObj.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime().ToShortDateString(), orderWrapper.DeliveredToETAString);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_E_ARV = new ZDateTime(2004, 05, 05);
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ETAString", shipmentWrapper.ETAString, orderWrapper.DeliveredToETAString);
		}

		public void TestHouseBillHeading()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HouseBillHeading", "HOUSE BILL", orderWrapper.HouseBillHeading);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("HouseBillHeading", shipmentWrapper.HouseBillHeading, orderWrapper.HouseBillHeading);
		}

		public void TestHouseBillAndIssueHeading()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HouseBillHeading", "HOUSE BILL", orderWrapper.HouseBillAndIssueHeading);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_HouseBillIssueDate = ZDateTime.Today;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("HouseBillHeading with issue date", shipmentWrapper.HouseBillHeading + " / ISSUE", orderWrapper.HouseBillAndIssueHeading);
		}

		public void TestPackages()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			orderBisObj.JD_JS = ZGuid.Empty;

			OrderLine line1 = orderBisObj.OrderLines.AddNew();
			line1.JO_OuterPacks = 20;

			OrderLine line2 = orderBisObj.OrderLines.AddNew();
			line2.JO_OuterPacks = 40;

			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("Packages", "60", orderWrapper.Packages);

			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.LCL;
			orderBisObj.JD_Packs = 10;
			AssertEquals("Should be 10 PKG", "10 PLT", orderWrapper.Packages);

			orderBisObj.JD_ContainerMode = Core.Constants.ContainerModes.FCL;
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OuterPacks = 120;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Packages", shipmentWrapper.Packages, orderWrapper.Packages);
		}

		public void TestHazCat()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HazCat", ZString.Empty, orderWrapper.HazCat);

			var shipment = Factory.New<ForwardingShipment>();
			var line = (PackLine)shipment.OuterPackLines.AddNew();
			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.General;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("HazCat", "", orderWrapper.HazCat);

			line.JL_RH_NKCommodityCode = Core.Constants.CargoTypes.Hazardous;
			var hazCat = UNDGSubstanceLoader.LoadSubstances(Factory, "2478", "c", "IMO").First();
			line.UNDGs.AddNew().DI_DG = hazCat.PK;
			AssertEquals("HazCat", "HAZ - UN2478, ISOCYANATES, FLAMMABLE, TOXIC, N.O.S., class 3 (6.1), PG III", orderWrapper.HazCat);

			line.UNDGs[0].DI_MPMarinePollutant = "T";
			AssertEquals("HazCat", "HAZ - UN2478, ISOCYANATES, FLAMMABLE, TOXIC, N.O.S., class 3 (6.1), PG III, MARINE POLLUTANT", orderWrapper.HazCat);
		}

		public void TestSealNumberHeading()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("SealNumberHeading", "COUNT", orderWrapper.SealNumberHeading);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("SealNumberHeading", shipmentWrapper.SealNumberHeading, orderWrapper.SealNumberHeading);
		}

		public void TestSealHeading()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("SealHeading", "COUNT", orderWrapper.SealHeading);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("SealHeading", shipmentWrapper.SealHeading, orderWrapper.SealHeading);
		}

		public void TestContainerWeightHeading()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ContainerWeightHeading", ZString.Empty, orderWrapper.ContainerWeightHeading);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ContainerWeightHeading", shipmentWrapper.ContainerWeightHeading, orderWrapper.ContainerWeightHeading);
		}

		public void TestContainerVolumeHeading()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ContainerVolumeHeading", ZString.Empty, orderWrapper.ContainerVolumeHeading);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ContainerVolumeHeading", shipmentWrapper.ContainerVolumeHeading, orderWrapper.ContainerVolumeHeading);
		}

		public void TestContainerPackageHeading()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ContainerPackageHeading", ZString.Empty, orderWrapper.ContainerPackageHeading);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ContainerPackageHeading", shipmentWrapper.ContainerPackageHeading, orderWrapper.ContainerPackageHeading);
		}

		public void TestNotifyPartyPostalAddress()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("NotifyPartyPostalAddress", "NOTIFY CONSIGNEE", orderWrapper.NotifyPartyPostalAddress);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.NotifyPartyDocumentaryAddress.ContactPK = Factory.LoadTop1(typeof(OrgContact), new ZQuery()).PK;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("NotifyPartyPostalAddress", shipmentWrapper.NotifyPartyPostalAddress, orderWrapper.NotifyPartyPostalAddress);
		}

		public void TestBrokerNameFromShipment()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ImportBrokerName", "To Be Advised", orderWrapper.BrokerName);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DocOrganisation orgWrapper = DocOrganisation.New(header, Factory);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OH_ImportBroker = header.PK;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ImportBrokerName", shipmentWrapper.ImportBrokerName, orderWrapper.BrokerName);
		}

		public void TestBrokerNameFromBuyersSupplierLink_TransportAndContainerMode()
		{
			AssertTestBrokerNameFromBuyersSupplierLink_TransportAndContainerMode(Core.Constants.TransportModes.Sea, Core.Constants.ContainerModes.FCL);
			AssertTestBrokerNameFromBuyersSupplierLink_TransportAndContainerMode(Core.Constants.TransportModes.Air, Core.Constants.ContainerModes.Loose);
			AssertTestBrokerNameFromBuyersSupplierLink_TransportAndContainerMode(Core.Constants.TransportModes.Road, Core.Constants.ContainerModes.FTL);
			AssertTestBrokerNameFromBuyersSupplierLink_TransportAndContainerMode(Core.Constants.TransportModes.Rail, Core.Constants.ContainerModes.FCL);
			AssertTestBrokerNameFromBuyersSupplierLink_TransportAndContainerMode(Core.Constants.TransportModes.Courier, Core.Constants.ContainerModes.OnBoardCourier);
		}

		void AssertTestBrokerNameFromBuyersSupplierLink_TransportAndContainerMode(ZString transportMode, ZString containerMode)
		{
			Order orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ImportBrokerName", "To Be Advised", orderWrapper.BrokerName);

			var importBroker1 = CreateOrgHeader(transportMode + "BROKER");
			var importBroker2 = CreateOrgHeader(transportMode + containerMode + "xBROKER");
			var importBrokerALL = CreateOrgHeader("ALLIMPBKR");

			OrgHeader supplier = CreateOrgHeader("SUPPLIER");
			OrgHeader buyer = CreateOrgHeader("BUYER");
			buyer.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			buyer.SetRelatedParty(importBroker1, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, transportMode, ZString.Empty);
			buyer.SetRelatedParty(importBroker2, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, transportMode, containerMode);
			buyer.SetRelatedParty(importBrokerALL, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty);

			orderBisObj.BuyerPK = buyer.PK;
			orderBisObj.SupplierPK = supplier.PK;
			orderBisObj.JD_TransportMode = transportMode;
			orderBisObj.JD_ContainerMode = containerMode;

			AssertEquals(importBroker2.OH_FullName, orderWrapper.BrokerName);
		}

		public void TestBrokerNameFromBuyersSupplierLink()
		{
			Order orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ImportBrokerName", "To Be Advised", orderWrapper.BrokerName);

			OrgHeader buyer = CreateOrgHeader("BUYER");
			buyer.OH_RL_NKClosestPort = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
			OrgHeader supplier = CreateOrgHeader("SUPPLIER");
			OrgHeader airImportBroker = CreateOrgHeader("AIRIMPBKR");
			OrgHeader seaImportBroker = CreateOrgHeader("SEAIMPBKR");
			OrgHeader melImportBroker = CreateOrgHeader("MELIMPBKR");
			OrgHeader railImportBroker = CreateOrgHeader("RAIIMPBKR");
			OrgHeader roadImportBroker = CreateOrgHeader("ROAIMPBKR");

			orderBisObj.BuyerPK = buyer.PK;
			orderBisObj.SupplierPK = supplier.PK;

			AssertEquals("To Be Advised", orderWrapper.BrokerName);

			orderBisObj.JD_TransportMode = "SEA";
			buyer.SetRelatedParty(airImportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Air, ZString.Empty);
			buyer.SetRelatedParty(seaImportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Sea, ZString.Empty);
			buyer.SetRelatedParty(railImportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Rail, ZString.Empty);
			buyer.SetRelatedParty(roadImportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.Road, ZString.Empty);
			buyer.SetRelatedParty(melImportBroker, RelatedPartyTypeList.Codes.CustomsAgentBroker, RelatedPartyDirectionList.Codes.Delivery, Core.Constants.TransportModes.All, ZString.Empty, "AUMEL");

			AssertEquals("SEAIMPBKR", orderWrapper.BrokerName);

			orderBisObj.JD_TransportMode = "AIR";
			AssertEquals("AIRIMPBKR", orderWrapper.BrokerName);
			orderBisObj.JD_TransportMode = "RAI";
			AssertEquals("RAIIMPBKR", orderWrapper.BrokerName);
			orderBisObj.JD_TransportMode = "ROA";
			AssertEquals("ROAIMPBKR", orderWrapper.BrokerName);

			orderBisObj.JD_RL_NKPortOfDischarge = "AUMEL";
			AssertEquals("MELIMPBKR", orderWrapper.BrokerName);

			orderBisObj.JD_TransportMode = "SEA";
			orderBisObj.JD_ContainerMode = "FCL";

			OrgHeader supplier1 = CreateOrgHeader("SUPPLIER1");
			OrgHeader supplier2 = CreateOrgHeader("SUPPLIER2");
			OrgHeader supplier3 = CreateOrgHeader("SUPPLIER3");
			OrgHeader supplier4 = CreateOrgHeader("SUPPLIER4");

			OrgHeader iMPBroker1 = CreateOrgHeader("IMPBroker1");
			OrgHeader iMPBroker2 = CreateOrgHeader("IMPBroker2");
			OrgHeader iMPBroker3 = CreateOrgHeader("IMPBroker3");
			OrgHeader iMPBroker4 = CreateOrgHeader("IMPBroker4");
			OrgHeader iMPBroker5 = CreateOrgHeader("IMPBroker5");
			OrgHeader iMPBroker6 = CreateOrgHeader("IMPBroker6");

			OrgSupplierBuyerLink link1 = CreateOrgSupplierBuyerLink(buyer, supplier1.PK, "SEA", "LCL", iMPBroker1.PK);
			OrgSupplierBuyerLink link2 = CreateOrgSupplierBuyerLink(buyer, supplier.PK, "AIR", "LCL", iMPBroker2.PK);
			OrgSupplierBuyerLink link3 = CreateOrgSupplierBuyerLink(buyer, supplier3.PK, "ALL", "", iMPBroker3.PK);
			OrgSupplierBuyerLink link4 = CreateOrgSupplierBuyerLink(buyer, supplier4.PK, "SEA", "LCL", iMPBroker4.PK);
			SetupTrnMode(link2.OrgSupBuyLinkTrnModes.AddNew(), "SEA", "FCL", iMPBroker5.PK);
			SetupTrnMode(link2.OrgSupBuyLinkTrnModes.AddNew(), "SEA", "LCL", iMPBroker6.PK);

			AssertEquals("IMPBroker5", orderWrapper.BrokerName);

			orderBisObj.JD_TransportMode = "AIR";
			AssertEquals("IMPBroker2", orderWrapper.BrokerName);

			orderBisObj.JD_TransportMode = "RAI";
			link2.OrgSupBuyLinkTrnModes[1].PF_TransportMode = "ALL";
			link2.OrgSupBuyLinkTrnModes[1].PF_ContainerMode = "";
			AssertEquals("IMPBroker5", orderWrapper.BrokerName);

			orderBisObj.JD_TransportMode = "SEA";
			orderBisObj.JD_ContainerMode = "LCL";
			AssertEquals("IMPBroker6", orderWrapper.BrokerName);

			buyer.SupplierLinks.RemoveAll();
			link1 = CreateOrgSupplierBuyerLink(buyer, supplier.PK, "SEA", "", iMPBroker1.PK);
			link2 = CreateOrgSupplierBuyerLink(buyer, supplier.PK, "AIR", "LCL", iMPBroker2.PK);
			link3 = CreateOrgSupplierBuyerLink(buyer, supplier.PK, "SEA", "GRP", iMPBroker1.PK);
			AssertEquals("IMPBroker1", orderWrapper.BrokerName);
		}

		public void TestPreAlertArrivalNoticeRemarks()
		{
			var note = fOrder.Notes.AddNew();
			note.ST_Description = PredefinedNoteTypes.Instance.PrealertArrivalNoticeRemarks.Description;
			note.ST_Table = fOrder.TableName;
			note.ST_ParentID = fOrder.PK;
			note.ST_NoteDataAsText = "Pre Alert and Arrival Notice Remarks\nThis PreAlert goes to CNE\n";

			var otherNotes = fOrder.Notes.AddNew();
			otherNotes.ST_Table = fOrder.TableName;
			otherNotes.ST_ParentID = fOrder.PK;
			otherNotes.ST_Description = PredefinedNoteTypes.Instance.CertificateOfOriginNote.Description;
			otherNotes.ST_NoteDataAsText = "This is other notes and should not be included.";
			AssertEquals("Wrong Outturn notes", "Pre Alert and Arrival Notice Remarks\nThis PreAlert goes to CNE", OrderWrapper.PreAlertArrivalNoticeRemarks);
		}

		public void TestPacksType()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_F3_NKPackType = "TTT";
			DocOrder wrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("PacksType", "TTT", wrapper.PacksType);

			orderBisObj.JD_F3_NKPackType = ZString.Empty;
			AssertEquals("Should default to PKG", Core.Constants.PkgUnit.Package, wrapper.PacksType);
		}

		public void TestUnitOfVolume()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_UnitOfVolume = "TT";
			DocOrder wrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("UnitOfVolume", "TT", wrapper.UnitOfVolume);

			orderBisObj.JD_UnitOfVolume = ZString.Empty;
			AssertEquals("Should Default to M3", Core.Constants.Volume.CubicMetres, wrapper.UnitOfVolume);
		}

		public void TestUnitOfWeight()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_UnitOfWeight = "TT";
			DocOrder wrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("UnitOfWeight", "TT", wrapper.UnitOfWeight);

			orderBisObj.JD_UnitOfWeight = ZString.Empty;
			AssertEquals("Should Default to KG", Core.Constants.Weight.Kilograms, wrapper.UnitOfWeight);
		}

		public void TestNotAlllocatedWeightVolumePackages()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("NotAllocatedWeight", "", orderWrapper.NotAllocatedWeight);
			AssertEquals("NotAllocatedVolume", "", orderWrapper.NotAllocatedVolume);
			AssertEquals("NotAllocatedPackages", "", orderWrapper.NotAllocatedPackages);

			var shipment = Factory.New<ForwardingShipment>();
			//Shipment.JS_OH_ImportBroker = Header.PK;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("NotAllocatedWeight", shipmentWrapper.NotAllocatedWeight, orderWrapper.NotAllocatedWeight);
			AssertEquals("NotAllocatedVolume", shipmentWrapper.NotAllocatedVolume, orderWrapper.NotAllocatedVolume);
			AssertEquals("NotAllocatedPackages", shipmentWrapper.NotAllocatedPackages, orderWrapper.NotAllocatedPackages);
		}

		public void TestSpecialInstructionNote()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			var specialInstructionNote1 = orderBisObj.Notes.AddNew();
			specialInstructionNote1.ST_Description = PredefinedNoteTypes.Instance.SpecialInstructions.Description;
			specialInstructionNote1.ST_ParentID = orderBisObj.PK;
			specialInstructionNote1.ST_Table = orderBisObj.TableName;
			specialInstructionNote1.ST_NoteDataAsText = "Special Instruction Note Line One\nLine Two";
			AssertEquals("Special Instruction Note Line One\nLine Two", orderWrapper.SpecialInstructionNote);

			specialInstructionNote1.ST_Description = "";
			specialInstructionNote1.ST_NoteDataAsText = "Special Instruction Note Line One\nLine Two";
			AssertEquals("", orderWrapper.SpecialInstructionNote);
		}

		public void TestShipmentBrokerageNumber()
		{
			var order = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(order, Factory);
			AssertEquals("Should be blank", ZString.Empty, orderWrapper.ShipmentBrokerageNumber);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIP1234";
			order.JD_JS = shipment.PK;
			AssertEquals("Shipment number should be returned", "SHIP1234", orderWrapper.ShipmentBrokerageNumber);

			order.JD_JS = ZGuid.Empty;
			var declaration = Factory.New<BaseJobDeclaration>();
			order.JD_JE = declaration.PK;
			declaration.JE_DeclarationReference = "DEC5678";
			AssertEquals("Declaration number should be returned", "DEC5678", orderWrapper.ShipmentBrokerageNumber);
		}

		public void TestShipmentBrokerageNumberHeading()
		{
			var order = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(order, Factory);
			AssertEquals("Should be blank", ZString.Empty, orderWrapper.ShipmentBrokerageNumberHeading);

			var shipment = Factory.New<ForwardingShipment>();
			order.JD_JS = shipment.PK;
			AssertEquals("Shipment heading should be returned", "SHIPMENT NO:", orderWrapper.ShipmentBrokerageNumberHeading);

			order.JD_JS = ZGuid.Empty;
			var declaration = Factory.New<BaseJobDeclaration>();
			order.JD_JE = declaration.PK;
			AssertEquals("Declaration heading should be returned", "DECLARATION NO:", orderWrapper.ShipmentBrokerageNumberHeading);
		}

		public void TestAgentNotes()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			var agentNotes = orderBisObj.Notes.AddNew();
			agentNotes.ST_Description = PredefinedNoteTypes.Instance.AgentNotes.Description;
			agentNotes.ST_ParentID = orderBisObj.PK;
			agentNotes.ST_Table = orderBisObj.TableName;
			agentNotes.ST_NoteDataAsText = "Agent Notes Stuff\nLine Two";
			AssertEquals("Should return Agent Notes", "Agent Notes Stuff\nLine Two", orderWrapper.AgentNotes);

			agentNotes.ST_Description = "";
			agentNotes.ST_NoteDataAsText = "Agent Notes Stuff\nLine Two";
			AssertEquals("Agent Notes should be blank", ZString.Empty, orderWrapper.AgentNotes);
		}

		#endregion

		#region ZDateTime Fields

		public void TestActualARV()
		{
			var actualARV = new ZDateTime(2016, 04, 04);

			var arrivalMilestone = OrderWithShip.WorkflowItems.Milestones[Events.Arrival];
			if (arrivalMilestone == null)
			{
				arrivalMilestone = OrderWithShip.WorkflowItems.Milestones.AddNew();
				((ITriggerConditions)arrivalMilestone).TriggerEventCode = Events.ArrivalCode;
			}
			arrivalMilestone.SetMilestoneActualDateForTest(actualARV);
			AssertEquals("Should get Actual Arrival Date from Order's milestone", actualARV, OrderWrapperWithShip.ActualARV);
		}

		public void TestActualDEP()
		{
			var actualDEP = new ZDateTime(2016, 04, 04);

			var departMilestone = OrderWithShip.WorkflowItems.Milestones[Events.Departure];
			if (departMilestone == null)
			{
				departMilestone = OrderWithShip.WorkflowItems.Milestones.AddNew();
				((ITriggerConditions)departMilestone).TriggerEventCode = Events.DepartureCode;
			}
			departMilestone.SetMilestoneActualDateForTest(actualDEP);
			AssertEquals("Should get Actual Departure date from Order's milestone", actualDEP, OrderWrapperWithShip.ActualDEP);
		}

		public void TestActualCCC()
		{
			var actualCCC = new ZDateTime(2004, 04, 04);
			EnsureMilestoneExist(OrderWithShip, Events.CustomsCommenced);
			EnsureMilestoneExist(OrderWithDec, Events.CustomsCommenced);
			Factory.Save();

			OrderWithShip.Shipment.Logs.AddNew(Events.CustomsCommenced, actualCCC.ToOffset());
			Factory.Save();
			AssertEquals("Should come from Shipment event", actualCCC, OrderWrapperWithShip.ActualCCC);

			OrderWithDec.Declaration.GetLogs().AddNew(Events.CustomsCommenced, actualCCC.ToOffset());
			Factory.Save();
			AssertEquals("Should come from Declaration event", actualCCC, OrderWrapperWithDec.ActualCCC);

			var orderBisObj = Factory.New<Order>();
			orderBisObj.UpdateEvent(Events.CustomsCommenced, actualCCC.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualCCC", actualCCC, orderWrapper.ActualCCC);
		}

		public void TestActualCLR()
		{
			var actualCLR = new ZDateTime(2004, 04, 04);
			EnsureMilestoneExist(OrderWithShip, Events.CustomsCleared);
			EnsureMilestoneExist(OrderWithDec, Events.CustomsCleared);
			Factory.Save();

			OrderWithShip.Shipment.Logs.AddNew(Events.CustomsCleared, actualCLR.ToOffset());
			Factory.Save();
			AssertEquals("Should come from Shipment event", actualCLR, OrderWrapperWithShip.ActualCLR);

			OrderWithDec.Declaration.GetLogs().AddNew(Events.CustomsCleared, actualCLR.ToOffset());
			Factory.Save();
			AssertEquals("Should come from Declaration event", actualCLR, OrderWrapperWithDec.ActualCLR);

			var orderBisObj = Factory.New<Order>();
			orderBisObj.UpdateEvent(Events.CustomsCleared, actualCLR.ToOffset());
			var orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualCLR", actualCLR, orderWrapper.ActualCLR);
		}

		public void TestActualEXW()
		{
			var orderBisObj = Factory.New<Order>();
			var actualEXW = new ZDateTime(2004, 04, 04);
			orderBisObj.UpdateEvent(Events.ExWorks, actualEXW.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualEXW", actualEXW, orderWrapper.ActualEXW);
		}

		public void TestActualIST()
		{
			var actualIST = new ZDateTime(2004, 04, 04);
			EnsureMilestoneExist(OrderWithShip, Events.DeliveryCartageCompleteFinalised);
			EnsureMilestoneExist(OrderWithDec, Events.DeliveryCartageCompleteFinalised);
			Factory.Save();

			OrderWithShip.Shipment.DocsAndCartage.JP_DeliveryCartageCompleted = actualIST;
			Factory.Save();
			AssertEquals("Should get date from Shipment", actualIST, OrderWrapperWithShip.ActualIST);

			((BaseJobDeclaration)OrderWithDec.Declaration).DocsAndCartage.JP_DeliveryCartageCompleted = actualIST;
			Factory.Save();
			AssertEquals("Should get date from Declaration", actualIST, OrderWrapperWithDec.ActualIST);

			var orderBisObj = Factory.New<Order>();
			orderBisObj.UpdateEvent(Events.DeliveryCartageCompleteFinalised, actualIST.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualIST", actualIST, orderWrapper.ActualIST);
		}

		public void TestActualPUP()
		{
			var actualPUP = new ZDateTime(2004, 04, 04);
			EnsureMilestoneExist(OrderWithShip, Events.DeliveryCartageAdvised);
			EnsureMilestoneExist(OrderWithDec, Events.DeliveryCartageAdvised);
			Factory.Save();

			OrderWithShip.Shipment.DocsAndCartage.JP_DeliveryCartageAdvised = actualPUP;
			OrderWithShip.Shipment.JS_PackingMode = Core.Constants.ContainerModes.LCL;
			Factory.Save();
			AssertEquals("Should get Date from Shipment", actualPUP, OrderWrapperWithShip.ActualPUP);

			((BaseJobDeclaration)OrderWithDec.Declaration).DocsAndCartage.JP_DeliveryCartageAdvised = actualPUP;
			Factory.Save();
			AssertEquals("Should get Date from Declaration", actualPUP, OrderWrapperWithDec.ActualPUP);

			var orderBisObj = Factory.New<Order>();
			orderBisObj.UpdateEvent(Events.DeliveryCartageAdvised, actualPUP.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualPUP", actualPUP, orderWrapper.ActualPUP);
		}

		public void TestActualRCV()
		{
			var orderBisObj = Factory.New<Order>();
			var actualRCV = new ZDateTime(2004, 04, 04);
			orderBisObj.UpdateEvent(Events.GateIn, actualRCV.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualRCV", actualRCV, orderWrapper.ActualRCV);
		}

		public void TestActualUNP()
		{
			var actualUNP = new ZDateTime(2004, 04, 04);
			EnsureMilestoneExist(OrderWithShip, Events.CargoAvailable);
			EnsureMilestoneExist(OrderWithDec, Events.CargoAvailable);
			Factory.Save();

			OrderWithShip.Shipment.DocsAndCartage.JP_LCLAvailable = actualUNP;
			Factory.Save();
			AssertEquals("Should get date from Shipment", actualUNP, OrderWrapperWithShip.ActualUNP);
			AssertEquals("Should be blank with Dec attached", ZDateTime.Empty, OrderWrapperWithDec.ActualUNP);

			var orderBisObj = Factory.New<Order>();
			orderBisObj.UpdateEvent(Events.CargoAvailable, actualUNP.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualUNP", actualUNP, orderWrapper.ActualUNP);
		}

		public void TestActualUS1()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime actualUS1 = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_ActualUserDate1 = actualUS1;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualUS1", actualUS1, orderWrapper.ActualUS1);
		}

		public void TestActualUS2()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime actualUS2 = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_ActualUserDate2 = actualUS2;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualUS2", actualUS2, orderWrapper.ActualUS2);
		}

		public void TestBookingConfDate()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime bookingConfDate = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_BookingConfDate = bookingConfDate;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("BookingConfDate", bookingConfDate, orderWrapper.BookingConfDate);
		}

		public void TestCustomDate1()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime customDate1 = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_CustomDate1 = customDate1;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomDate1", customDate1, orderWrapper.CustomDate1);
		}

		public void TestCustomDate2()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime customDate2 = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_CustomDate2 = customDate2;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomDate2", customDate2, orderWrapper.CustomDate2);
		}

		public void TestEstimatedCCC()
		{
			var orderBisObj = Factory.New<Order>();
			var estimatedCCC = new ZDateTime(2004, 04, 04);
			orderBisObj.UpdateEventEstimate(Events.CustomsCommenced, estimatedCCC.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedCCC", estimatedCCC, orderWrapper.EstimatedCCC);
		}

		public void TestEstimatedCLR()
		{
			var orderBisObj = Factory.New<Order>();
			var estimatedCLR = new ZDateTime(2004, 04, 04);
			orderBisObj.UpdateEventEstimate(Events.CustomsCleared, estimatedCLR.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedCLR", estimatedCLR, orderWrapper.EstimatedCLR);
		}

		public void TestEstimatedDEP()
		{
			ZDateTime estimatedDEP = new ZDateTime(2004, 04, 04);
			OrderWithShip.Shipment.JS_RL_NKOrigin = GlbCompany.CurrentCompany.OrgProxy.UNLOCO.RL_Code;
			Factory.Save();

			Transport transport1 = OrderWithShip.Shipment.Consols[0].Transports[0];
			transport1.JW_ETD = estimatedDEP;
			Factory.Save();
			AssertEquals("Should get date from Shipment Consol", estimatedDEP, OrderWrapperWithShip.EstimatedDEP);

			var transport = ((IRoutingSupport)OrderWithDec.Declaration).Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETD = estimatedDEP;
			Factory.Save();
			AssertEquals("Should get date from declaration", estimatedDEP, OrderWrapperWithDec.EstimatedDEP);

			var orderBisObj = Factory.New<Order>();
			orderBisObj.UpdateEventEstimate(Events.Departure, estimatedDEP.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedDEP", estimatedDEP, orderWrapper.EstimatedDEP);
		}

		public void TestEstimatedDE2()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime estimatedDE2 = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_E_DEP_2 = estimatedDE2;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedDE2", estimatedDE2, orderWrapper.IntermediateETD);
		}

		public void TestEstimatedDE3()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime estimatedDE3 = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_E_DEP_3 = estimatedDE3;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedDE3", estimatedDE3, orderWrapper.ArrivalETD);
		}

		public void TestEstimatedEXW()
		{
			var orderBisObj = Factory.New<Order>();
			var estimatedEXW = new ZDateTime(2004, 04, 04);
			orderBisObj.UpdateEventEstimate(Events.ExWorks, estimatedEXW.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedEXW", estimatedEXW, orderWrapper.EstimatedEXW);
		}

		public void TestEstimatedIST()
		{
			var orderBisObj = Factory.New<Order>();
			var estimatedIST = new ZDateTime(2004, 04, 04);
			orderBisObj.UpdateEventEstimate(Events.DeliveryCartageCompleteFinalised, estimatedIST.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedIST", estimatedIST, orderWrapper.EstimatedIST);
		}

		public void TestEstimatedPUP()
		{
			var orderBisObj = Factory.New<Order>();
			var estimatedPUP = new ZDateTime(2004, 04, 04);
			orderBisObj.UpdateEventEstimate(Events.DeliveryCartageAdvised, estimatedPUP.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedPUP", estimatedPUP, orderWrapper.EstimatedPUP);
		}

		public void TestEstimatedRCV()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime estimatedRCV = new ZDateTime(2004, 04, 04);
			orderBisObj.UpdateEventEstimate(Events.GateIn, estimatedRCV.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedRCV", estimatedRCV, orderWrapper.EstimatedRCV);
		}

		public void TestEstimatedUS1()
		{
			var orderBisObj = Factory.New<Order>();
			var estimatedUS1 = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_EstimateUserDate1 = estimatedUS1;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedUS1", estimatedUS1, orderWrapper.EstimatedUS1);
		}

		public void TestEstimatedUS2()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime estimatedUS2 = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_EstimateUserDate2 = estimatedUS2;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedUS2", estimatedUS2, orderWrapper.EstimatedUS2);
		}

		public void TestDepartureVesselCutoffDate()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime departureVesselCutoffDate = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_DepartureVesselCutoffDate = departureVesselCutoffDate;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("DepartureVesselCutoffDate", departureVesselCutoffDate, orderWrapper.DepartureVesselCutoffDate);
		}

		public void TestEstimatedARV()
		{
			var estimatedARV = new ZDateTime(2004, 04, 04);
			OrderWithShip.Shipment.JS_RL_NKDestination = GlbCompany.CurrentCompany.OrgProxy.UNLOCO.RL_Code;
			Factory.Save();

			Transport transport1 = OrderWithShip.Shipment.Consols[0].Transports[0];
			transport1.JW_ETA = estimatedARV.AddDays(1);
			transport1.JW_RL_NKDiscPort = GlbCompany.CurrentCompany.OrgProxy.UNLOCO.RL_Code;
			Factory.Save();
			AssertEquals("Should get value from Shipment Consol", estimatedARV.AddDays(1), OrderWrapperWithShip.EstimatedARV);

			var transport = ((IRoutingSupport)OrderWithDec.Declaration).Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_ETA = estimatedARV.AddDays(2);

			Factory.Save();
			AssertEquals("Should get value from Declaration", estimatedARV.AddDays(2), OrderWrapperWithDec.EstimatedARV);

			var orderBisObj = Factory.New<Order>();
			orderBisObj.UpdateEventEstimate(Events.Arrival, estimatedARV.ToOffset());
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedARV", estimatedARV, orderWrapper.EstimatedARV);
		}

		public void TestEstimatedAR1stIntermediate()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime estimatedAR1stIntermediate = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_E_ARV_1stIntermediate = estimatedAR1stIntermediate;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedAR1stIntermediate", estimatedAR1stIntermediate, orderWrapper.DepartureETA);
		}

		public void TestEstimatedAR2ndIntermediate()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime estimatedAR2ndIntermediate = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_E_ARV_2ndIntermediate = estimatedAR2ndIntermediate;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedAR2ndIntermediate", estimatedAR2ndIntermediate, orderWrapper.IntermediateETA);
		}

		public void TestFollowUpDate()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime followUpDate = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_FollowUpDate = followUpDate;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("FollowUpDate", followUpDate, orderWrapper.FollowUpDate);
		}

		public void TestInvoiceDate()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime invoiceDate = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_InvoiceDate = invoiceDate;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("InvoiceDate", invoiceDate, orderWrapper.InvoiceDate);
		}

		public void TestOrderDate()
		{
			var orderBisObj = Factory.New<Order>();
			ZDateTime orderDate = new ZDateTime(2004, 04, 04);
			orderBisObj.JD_OrderDate = orderDate;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("OrderDate", orderDate, orderWrapper.OrderDate);
		}

		public void TestReqInStore()
		{
			fOrder.JD_DeliveryRequiredBy = ZDateTime.Now;
			AssertEquals("Should have Req In Store", fOrder.JD_DeliveryRequiredBy, OrderWrapper.ReqInStore);
		}

		public void TestReqExWorks()
		{
			fOrder.JD_ExWorksRequiredBy = ZDateTime.Now;
			AssertEquals("Should have Req Ex Works", fOrder.JD_ExWorksRequiredBy, OrderWrapper.ReqExWorks);
		}

		public void TestAvailableDate()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			orderBisObj.UpdateEventEstimate(Events.CargoAvailable, new ZDateTimeOffset(2004, 04, 04));
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("AvailableDateHeading", "AVAILABLE DATE:", orderWrapper.AvailableDateHeading);
			AssertEquals("AvailableDate", orderBisObj.GetMilestoneEstimatedDate(Events.CargoAvailable).ToZDateTime(), orderWrapper.AvailableDate);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("AvailableDate", shipmentWrapper.AvailableDate, orderWrapper.AvailableDate);
		}

		public void TestHouseBillIssueDate()
		{
			Order orderBisObj = Factory.New<Order>();
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HouseBillIssueDate should be emtpy", ZDateTime.Empty, OrderWrapper.HouseBillIssueDate);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ZDateTime today = ZDateTime.Today;
			shipment.JS_HouseBillIssueDate = today;
			orderBisObj.JD_JS = shipment.PK;
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HouseBillIssueDate should be Today", today, OrderWrapper.HouseBillIssueDate);
		}

		public void TestHouseBillAndIssueDate()
		{
			Order orderBisObj = Factory.New<Order>();
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HouseBillIssueDate should be emtpy", "To Be Advised", OrderWrapper.HouseBillAndIssueDate);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ZDateTime today = ZDateTime.Today;
			shipment.JS_HouseBill = "12345";
			shipment.JS_HouseBillIssueDate = today;
			orderBisObj.JD_JS = shipment.PK;
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("HouseBillIssueDate should be Today", "12345 / " + today.ToShortDateString(), OrderWrapper.HouseBillAndIssueDate);
		}

		public void TestMasterBillIssueDate()
		{
			Order orderBisObj = Factory.New<Order>();
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("MasterBillIssueDate should be emtpy", ZDateTime.Empty, OrderWrapper.MasterBillIssueDate);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			ZDateTime today = ZDateTime.Today;
			consol.JK_MasterBillIssueDate = today;
			orderBisObj.JD_JS = shipment.PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("MasterBillIssueDate should be emtpy -only show for air", ZDateTime.Empty, OrderWrapper.MasterBillIssueDate);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("MasterBillIssueDate should be Today", today, OrderWrapper.MasterBillIssueDate);
		}

		public void TestMasterBillAndIssueDate()
		{
			Order orderBisObj = Factory.New<Order>();
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("MasterBillIssueDate should be emtpy", "To Be Advised", OrderWrapper.MasterBillAndIssueDate);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			ForwardingConsol consol = shipment.Consols.AddNew();
			ZDateTime today = ZDateTime.Today;
			consol.JK_MasterBillIssueDate = today;
			consol.JK_MasterBillNum = "1234";
			orderBisObj.JD_JS = shipment.PK;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("MasterBillIssueDate should be emtpy -only show for air", "1234", OrderWrapper.MasterBillAndIssueDate);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_MasterBillNum = "1234";
			OrderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("MasterBillIssueDate should be Today", "1234 / " + today.ToShortDateString(), OrderWrapper.MasterBillAndIssueDate);
		}

		#endregion

		#region ZBool Fields

		public void TestArchive()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_IsCancelled = false;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("!IsCancelled", !orderWrapper.IsCancelled);

			orderBisObj.JD_IsCancelled = true;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("IsCancelled", orderWrapper.IsCancelled);
		}

		public void TestCustomFlag1()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_CustomFlag1 = false;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("!CustomFlag1", !orderWrapper.CustomFlag1);

			orderBisObj.JD_CustomFlag1 = true;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("CustomFlag1", orderWrapper.CustomFlag1);
		}

		public void TestCustomFlag2()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_CustomFlag2 = false;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("!CustomFlag2", !orderWrapper.CustomFlag2);

			orderBisObj.JD_CustomFlag2 = true;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("CustomFlag2", orderWrapper.CustomFlag2);
		}

		public void TestCustomFlag3()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_CustomFlag3 = false;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("!CustomFlag3", !orderWrapper.CustomFlag3);

			orderBisObj.JD_CustomFlag3 = true;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("CustomFlag3", orderWrapper.CustomFlag3);
		}

		public void TestCustomFlag4()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_CustomFlag4 = false;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("!CustomFlag4", !orderWrapper.CustomFlag4);

			orderBisObj.JD_CustomFlag4 = true;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("CustomFlag4", orderWrapper.CustomFlag4);
		}

		public void TestCustomFlag5()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_CustomFlag5 = false;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("!CustomFlag5", !orderWrapper.CustomFlag5);

			orderBisObj.JD_CustomFlag5 = true;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			Assert("CustomFlag5", orderWrapper.CustomFlag5);
		}

		#endregion

		#region ZDecimal Fields

		public void TestCustomDecimal1()
		{
			var orderBisObj = Factory.New<Order>();
			ZDecimal customDecimal1 = new ZDecimal(4.89);
			orderBisObj.JD_CustomDecimal1 = customDecimal1;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomDecimal1", customDecimal1, orderWrapper.CustomDecimal1);
		}

		public void TestCustomDecimal2()
		{
			var orderBisObj = Factory.New<Order>();
			ZDecimal customDecimal2 = new ZDecimal(4.89);
			orderBisObj.JD_CustomDecimal2 = customDecimal2;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomDecimal2", customDecimal2, orderWrapper.CustomDecimal2);
		}

		public void TestCustomDecimal3()
		{
			var orderBisObj = Factory.New<Order>();
			ZDecimal customDecimal3 = new ZDecimal(4.89);
			orderBisObj.JD_CustomDecimal3 = customDecimal3;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomDecimal3", customDecimal3, orderWrapper.CustomDecimal3);
		}

		public void TestCustomDecimal4()
		{
			var orderBisObj = Factory.New<Order>();
			ZDecimal customDecimal4 = new ZDecimal(4.89);
			orderBisObj.JD_CustomDecimal4 = customDecimal4;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomDecimal4", customDecimal4, orderWrapper.CustomDecimal4);
		}

		public void TestCustomDecimal5()
		{
			var orderBisObj = Factory.New<Order>();
			ZDecimal customDecimal5 = new ZDecimal(4.89);
			orderBisObj.JD_CustomDecimal5 = customDecimal5;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("CustomDecimal5", customDecimal5, orderWrapper.CustomDecimal5);
		}

		public void TestEstimatedExchangeRate()
		{
			var orderBisObj = Factory.New<Order>();
			ZDecimal estimatedExchangeRate = new ZDecimal(4.89);
			orderBisObj.JD_EstimatedExchangeRate = estimatedExchangeRate;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("EstimatedExchangeRate", estimatedExchangeRate, orderWrapper.EstimatedExchangeRate);
		}

		public void TestActualWeight()
		{
			var orderBisObj = Factory.New<Order>();
			ZDecimal actualWeight = new ZDecimal(4.89);
			orderBisObj.JD_ActualWeight = actualWeight;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualWeight", actualWeight, orderWrapper.OrderWeight);
		}

		public void TestActualVolume()
		{
			var orderBisObj = Factory.New<Order>();
			ZDecimal actualVolume = new ZDecimal(4.89);
			orderBisObj.JD_ActualVolume = actualVolume;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("ActualVolume", actualVolume, orderWrapper.OrderVolume);
		}

		#endregion

		#region ZInt Fields

		public void TestPacks()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_Packs = 1;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("Packs", 1, orderWrapper.OrderPacks);
		}

		public void TestOrderShipmentConsolsCount()
		{
			AssertEquals("Should have 0 with no shipment on Order", 0, OrderWrapper.OrderShipmentConsolsCount);
			AssertEquals("Should have 0 with shipment with no consol", 0, OrderWrapperWithShipWithoutConsol.OrderShipmentConsolsCount);
			AssertEquals("Should have 1 with Shipment with 1 consol", 1, OrderWrapperWithShip.OrderShipmentConsolsCount);
		}

		#endregion

		#region Wrapper Fields

		public void TestShipment()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("Shipment", orderWrapper.Shipment);

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			AssertNotNull("Shipment", orderWrapper.Shipment);
			AssertEquals("Shipment is of type DocShipment", typeof(DocForwardingShipment), orderWrapper.Shipment.GetType());
		}

		public void TestReceivingAgent()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("ReceivingAgent", orderWrapper.ReceivingAgent);

			var org = Factory.New<OrgHeader>();
			orderBisObj.JD_OH_ReceivingAgent = org.PK;
			AssertNotNull("ReceivingAgent", orderWrapper.ReceivingAgent);
			AssertEquals("ReceivingAgent is of type DocOrganisation", typeof(DocOrganisation), orderWrapper.ReceivingAgent.GetType());

			OrderWithShip.Shipment.Consols[0].SetDefaultReceivingForwarderAddress(org);
			DocBaseWrapper intermediateWrapper = (DocBaseWrapper)OrderWrapperWithShip.ReceivingAgent.WrappedObject;
			AssertEquals("Receiving Agent should come from Consol if attached", org.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);

			OrderWithShipWithoutConsol.JD_OH_ReceivingAgent = org.PK;
			intermediateWrapper = (DocBaseWrapper)OrderWrapperWithShipWithoutConsol.ReceivingAgent.WrappedObject;
			AssertEquals("Should come from the order if a shipment without a consol is attached", org.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);
		}

		public void TestSendingAgent()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("SendingAgent", orderWrapper.SendingAgent);

			var org = Factory.New<OrgHeader>();
			orderBisObj.JD_OH_SendingAgent = org.PK;
			AssertNotNull("SendingAgent", orderWrapper.SendingAgent);
			AssertEquals("SendingAgent is of type DocOrganisation", typeof(DocOrganisation), orderWrapper.SendingAgent.GetType());

			OrderWithShip.Shipment.Consols[0].SetDefaultSendingForwarderAddress(org);
			DocBaseWrapper intermediateWrapper = (DocBaseWrapper)OrderWrapperWithShip.SendingAgent.WrappedObject;
			AssertEquals("Sending Agent should come from Consol if attached", org.PK, ((BusinessObject)intermediateWrapper.WrappedObject).PK);
		}

		public void TestGoodsDeliveredTo()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("GoodsDeliveredTo", orderWrapper.GoodsDeliveredTo);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			orderBisObj.JD_RL_NKGoodsDeliveredTo = uNLOCO.Code;
			AssertNotNull("GoodsDeliveredTo", orderWrapper.GoodsDeliveredTo);
			AssertEquals("GoodsDeliveredTo is of type DocUNLOCO", typeof(DocUNLOCO), orderWrapper.GoodsDeliveredTo.GetType());

			OrderWithShip.Shipment.JS_RL_NKDestination = uNLOCO.RL_Code;
			AssertEquals("Should get Origin from Shipment", uNLOCO, OrderWrapperWithShip.GoodsDeliveredTo.WrappedObject);

			((BaseJobDeclaration)OrderWithDec.Declaration).JE_RL_NKFinalDestination = uNLOCO.RL_Code;
			AssertEquals("Should get Destination from Dec", uNLOCO, OrderWrapperWithDec.GoodsDeliveredTo.WrappedObject);
		}

		public void TestGoodsDeliveredToOrderAndGoodsAvailableAtOrder()
		{
			var buyer = Factory.New<OrgHeader>();
			buyer.OH_Code = "BUYER";
			buyer.OH_FullName = "Buyer Org Header";

			var deliveryAddress = buyer.Addresses.AddNew();
			deliveryAddress.OA_Code = "DLV: Delivery Address";
			deliveryAddress.OA_Address1 = "Delivery Address";
			deliveryAddress.OA_OH = buyer.PK;

			var supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "SUPLI";
			supplier.OH_FullName = "Supplier Org Header";

			var pickupAddress = supplier.Addresses.AddNew();
			pickupAddress.OA_Code = "PUP: Pickup Address";
			pickupAddress.OA_Address1 = "Pickup Address";
			pickupAddress.OA_OH = supplier.PK;

			var orderBisObj = Factory.New<Order>();

			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals(AutoDocAddressTypes.Codes.GoodsDeliveredTo, orderWrapper.GoodsDeliveredToOrder.Code);
			AssertEquals(string.Empty, orderWrapper.GoodsDeliveredToOrder.Address1);
			AssertEquals(AutoDocAddressTypes.Codes.GoodsAvailableAt, orderWrapper.GoodsAvailableAtOrder.Code);
			AssertEquals(string.Empty, orderWrapper.GoodsAvailableAtOrder.Address1);

			orderBisObj.BuyerPK = buyer.PK;
			orderBisObj.SupplierPK = supplier.PK;

			orderBisObj.GoodsDeliveredToAddress.E2_OA_Address = deliveryAddress.PK;
			orderBisObj.GoodsAvailableAtAddress.E2_OA_Address = pickupAddress.PK;
			AssertEquals(deliveryAddress.OA_Code, orderWrapper.GoodsDeliveredToOrder.Code);
			AssertEquals(deliveryAddress.Address1, orderWrapper.GoodsDeliveredToOrder.Address1);
			AssertEquals(pickupAddress.OA_Code, orderWrapper.GoodsAvailableAtOrder.Code);
			AssertEquals(pickupAddress.Address1, orderWrapper.GoodsAvailableAtOrder.Address1);
		}

		public void TestGoodsReceivedAt()
		{
			var orderBisObj = Factory.New<Order>();
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("GoodsReceivedAt", orderWrapper.GoodsReceivedAt);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			orderBisObj.JD_RL_NKGoodsAvailableAt = uNLOCO.Code;
			AssertNotNull("GoodsReceivedAt", orderWrapper.GoodsReceivedAt);
			AssertEquals("GoodsReceivedAt is of type DocUNLOCO", typeof(DocUNLOCO), orderWrapper.GoodsReceivedAt.GetType());

			OrderWithShip.Shipment.JS_RL_NKOrigin = uNLOCO.RL_Code;
			AssertEquals("Should get Origin from Shipment", uNLOCO, OrderWrapperWithShip.GoodsReceivedAt.WrappedObject);

			((BaseJobDeclaration)OrderWithDec.Declaration).JE_RL_NKOrigin = uNLOCO.RL_Code;
			AssertEquals("Should get Origin from Declaration", uNLOCO, OrderWrapperWithDec.GoodsReceivedAt.WrappedObject);
		}

		public void TestCurrency()
		{
			Order orderBisObj = Factory.New<Order>();
			orderBisObj.JD_RX_NKOrderCurrency = ZString.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("Currency", orderWrapper.Currency);

			RefCurrency currency = Factory.New<RefCurrency>();
			currency.RX_Code = Enterprise.Core.Constants.CurrencyCodes.Australia;
			orderBisObj.JD_RX_NKOrderCurrency = currency.RX_Code;

			AssertNotNull("Currency", orderWrapper.Currency);
			AssertEquals("Currency is of type DocUNLOCO", typeof(DocCurrency), orderWrapper.Currency.GetType());
		}

		public void TestConsignor()
		{
			var testOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("Consignor", orderWrapper.Consignor);

			orderBisObj.SupplierPK = testOrg.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNotNull("Consignor", orderWrapper.Consignor);
			AssertEquals("Consignor is of type DocOrganisation", typeof(DocOrganisation), orderWrapper.Consignor.GetType());

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Consignor", shipmentWrapper.Consignor, orderWrapper.Consignor);

			shipment.ConsignorPK = testOrg.PK;
			shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertNotNull("Consignor", orderWrapper.Consignor);
			AssertEquals("Consignor is of type DocOrganisation", typeof(DocOrganisation), orderWrapper.Consignor.GetType());

			((BaseJobDeclaration)OrderWithDec.Declaration).JE_OH_Supplier = testOrg.PK;
			AssertNotNull("Order Consignee with Declaration Attatched Should not be null", OrderWrapperWithDec.Consignor);
			AssertEquals("Order Consignee should be Declarations Importer", testOrg.OH_FullName, OrderWrapperWithDec.Consignor.Name);
		}

		public void TestConsignee()
		{
			var testOrg = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("Consignee", orderWrapper.Consignee);

			orderBisObj.BuyerPK = testOrg.PK;
			AssertNotNull("Consignee", orderWrapper.Consignee);
			AssertEquals("Consignee is of type DocOrganisation", typeof(DocOrganisation), orderWrapper.Consignee.GetType());

			var shipment = Factory.New<ForwardingShipment>();
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("Consignee", shipmentWrapper.Consignee, orderWrapper.Consignee);

			shipment.ConsigneePK = testOrg.PK;
			shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertNotNull("Consignee", orderWrapper.Consignee);
			AssertEquals("Consignee is of type DocOrganisation", typeof(DocOrganisation), orderWrapper.Consignee.GetType());

			((BaseJobDeclaration)OrderWithDec.Declaration).JE_OH_Importer = testOrg.PK;
			AssertNotNull("Order Consignee with Declaration Attatched Should not be null", OrderWrapperWithDec.Consignee);
			AssertEquals("Order Consignee should be Declarations Importer", testOrg.OH_FullName, OrderWrapperWithDec.Consignee.Name);
		}

		public void TestNotifyParty()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("NotifyParty", orderWrapper.NotifyParty);

			var contact = Factory.LoadTop1<OrgContact>(new ZQuery());
			DocContacts contactWrapper = DocContacts.New(contact, Factory);

			var shipment = Factory.New<ForwardingShipment>();
			((ISupportDataImporting)shipment).IsImportingData = true;
			shipment.NotifyPartyDocumentaryAddress.ContactPK = contact.PK;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("NotifyParty", contactWrapper.Name, orderWrapper.NotifyParty.Name);
		}

		public void TestImportBroker()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_JS = ZGuid.Empty;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertNull("ImportBroker", orderWrapper.ImportBroker);

			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			DocOrganisation orgWrapper = DocOrganisation.New(header, Factory);

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_OH_ImportBroker = header.PK;
			orderBisObj.JD_JS = shipment.PK;
			orderWrapper = DocOrder.New(orderBisObj, Factory);
			DocShipment shipmentWrapper = DocShipment.New(shipment, Factory);
			AssertEquals("ImportBroker", orgWrapper.Name, orderWrapper.ImportBroker.Name);
		}

		public void TestDocumentLogo()
		{
			((BaseJobDeclaration)OrderWithDec.Declaration).ImporterDocumentaryAddress.E2_AddressOverride = true;
			SystemDataRegistry.Instance.CompanyLogo.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, new Bitmap(1, 1));
			AssertImageEquals("Should fallback to registry value rather than throw exception", new Bitmap(1, 1), DocOrder.New(OrderWithDec, Factory).DocumentLogo);
		}

		#endregion

		#region ZByte Fields

		public void TestOrderNumberSplit()
		{
			var orderBisObj = Factory.New<Order>();
			orderBisObj.JD_OrderNumberSplit = 1;
			DocOrder orderWrapper = DocOrder.New(orderBisObj, Factory);
			AssertEquals("OrderNumberSplit", "1", orderWrapper.OrderNumberSplit.ToString());
		}

		#endregion

		#region IDocConsol

		#region ZString Fields

		public void TestTransportInfo()
		{
			fOrder.JD_JS = ZGuid.Empty;
			AssertEquals("ConsolTransportInfo", ZString.Empty, OrderWrapper.TransportInfo);

			fOrder.JD_TransportMode = Core.Constants.TransportModes.Air;
			fOrder.JD_DepartureVoyage = "Dep";
			AssertEquals("ConsolTransportInfo", "Dep", OrderWrapper.TransportInfo);

			fOrder.JD_ArrivalVoyage = ZString.Empty;
			fOrder.JD_IntermediateVoyage = "Inter";
			AssertEquals("ConsolTransportInfo", "Inter", OrderWrapper.TransportInfo);

			fOrder.JD_ArrivalVoyage = "Arrive";
			AssertEquals("ConsolTransportInfo", "Arrive", OrderWrapper.TransportInfo);

			fOrder.JD_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("ConsolTransportInfo", "Arrive", OrderWrapper.TransportInfo);

			var depVessel = Factory.LoadTop1<RefVessel>(new ZQuery());
			fOrder.JD_RV_NKDepartureVessel = depVessel.RV_Code;
			AssertEquals("ConsolTransportInfo", depVessel.RV_Code + " / " + "Arrive", OrderWrapper.TransportInfo);

			fOrder.JD_ArrivalVoyage = ZString.Empty;
			var interVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, SQLComparisonOperator.NotEqual, depVessel.RV_Code));
			fOrder.JD_RV_NKIntermediateVessel = interVessel.RV_Code;
			AssertEquals("ConsolTransportInfo", interVessel.RV_Code + " / " + "Inter", OrderWrapper.TransportInfo);

			fOrder.JD_ArrivalVoyage = "Arrive";
			var arriveVessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, SQLComparisonOperator.NotEqual, interVessel.RV_Code));
			fOrder.JD_RV_NKIntermediateVessel = arriveVessel.RV_Code;
			AssertEquals("ConsolTransportInfo", arriveVessel.RV_Code + " / " + "Arrive", OrderWrapper.TransportInfo);
		}

		public void TestTransportHeading()
		{
			fOrder.JD_JS = ZGuid.Empty;
			fOrder.JD_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("ConsolTransportInfo", ZString.Empty, OrderWrapper.TransportHeading);

			fOrder.JD_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("ConsolTransportInfo", "JOURNEY NAME / JOURNEY NUMBER", OrderWrapper.TransportHeading);

			fOrder.JD_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ConsolTransportInfo", "FLIGHT", OrderWrapper.TransportHeading);

			fOrder.JD_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("ConsolTransportInfo", "VESSEL / VOYAGE", OrderWrapper.TransportHeading);
		}

		public void TestMasterBillHeading()
		{
			AssertEquals("MasterBillHeading", "MASTER BILL NUMBER", OrderWrapper.MasterBillHeading);
		}

		public void TestMasterBillAndIssueHeading()
		{
			AssertEquals("MasterBillHeading", "MASTER BILL NUMBER", OrderWrapper.MasterBillAndIssueHeading);

			ForwardingConsol consol = OrderWithShip.Shipment.Consols[0];
			consol.JK_MasterBillIssueDate = ZDateTime.Today;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("MasterBillHeading - issue only shows if air", "MASTER BILL NUMBER", OrderWrapper.MasterBillAndIssueHeading);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("Order Wrapper should get masterbill from Consol", "MASTER BILL NUMBER / ISSUE", OrderWrapperWithShip.MasterBillAndIssueHeading);
		}

		public void TestMasterBillNum()
		{
			ZString masterWayBill = "MasterWay";

			fOrder.JD_JS = ZGuid.Empty;
			fOrder.JD_JE = ZGuid.Empty;
			fOrder.JD_MasterWaybill = ZString.Empty;
			AssertEquals("MasterWayBill", "To Be Advised", OrderWrapper.MasterBillNum);

			fOrder.JD_MasterWaybill = masterWayBill;
			AssertEquals("Masterbill Num", masterWayBill, OrderWrapper.MasterBillNum);

			OrderWithShip.Shipment.Consols[0].JK_MasterBillNum = masterWayBill;
			AssertEquals("Order Wrapper should get masterbill from Consol", masterWayBill, OrderWrapperWithShip.MasterBillNum);

			OrderWithDec.Declaration[BaseJobDeclaration.Schema.JE_MasterBill] = masterWayBill;
			AssertEquals("Order Wrapper should get masterbill from Dec", masterWayBill, OrderWrapperWithDec.MasterBillNum);
		}

		#endregion

		#region Wrappers

		public void TestGoodsAvailableAt()
		{
			AssertNotNull("GoodsAvailableAt", OrderWrapper.GoodsAvailableAt);
			fOrder.SupplierPK = (Factory.LoadTop1<OrgHeader>(new ZQuery())).PK;
			fOrder.JD_JS = ZGuid.Empty;
			fOrder.JD_JE = ZGuid.Empty;

			var pickupAddress = Factory.New<OrgAddress>();
			pickupAddress.OA_Address1 = "Pickup Address";
			fOrder.GoodsAvailableAtAddress.E2_OA_Address = pickupAddress.PK;
			AssertEquals("OrderWrapper should be pickupAddress", "Pickup Address", OrderWrapper.GoodsAvailableAt.Address1);
			AssertEquals("GoodsAvailableAtAddress should be used.", fOrder.GoodsAvailableAtAddress, OrderWrapper.GoodsAvailableAt.WrappedObject);

			var address1 = Factory.New<OrgAddress>();
			address1.OA_Address1 = "Address";
			OrderWithShip.Shipment.ConsignorPickupAddress.E2_OA_Address = address1.PK;
			AssertEquals("Address should come from Shipment", address1.OA_Address1, OrderWrapperWithShip.GoodsAvailableAt.Address1);
			AssertEquals("ConsignorPickupAddress should be used.", OrderWithShip.Shipment.ConsignorPickupAddress, OrderWrapperWithShip.GoodsAvailableAt.WrappedObject);

			var supplierPickupAddress = ((BaseJobDeclaration)OrderWithDec.Declaration).SupplierPickupAddress;
			supplierPickupAddress.E2_OA_Address = address1.PK;
			AssertEquals("Address should come from Declaration", address1.OA_Address1, OrderWrapperWithDec.GoodsAvailableAt.Address1);
			AssertEquals("Declaration SupplierPickupAddress should be used.", supplierPickupAddress, OrderWrapperWithDec.GoodsAvailableAt.WrappedObject);
		}

		public void TestPortOfDischarge()
		{
			var uNLOCO1 = Factory.New<RefUNLOCO>();
			uNLOCO1.RL_Code = "USEWO";

			var uNLOCO2 = Factory.New<RefUNLOCO>();
			uNLOCO2.RL_Code = "AUBBB";

			fOrder.JD_RL_NKPortOfDischarge = ZString.Empty;
			AssertNull("PortOfDischarge", OrderWrapper.PortOfDischarge);

			fOrder.JD_RL_NKGoodsDeliveredTo = uNLOCO1.RL_Code;
			fOrder.JD_RL_NKPortOfDischarge = uNLOCO2.RL_Code;

			AssertNotNull("PortOfDischarge", OrderWrapper.PortOfDischarge);
			AssertEquals("PortOfDischarge is of type DocUNLOCO", typeof(DocUNLOCO), OrderWrapper.PortOfDischarge.GetType());

			CommonConsol consol = OrderWithShip.Shipment.Consols[0];

			consol.JK_RL_NKDischargePort = uNLOCO2.RL_Code;
			AssertEquals("Should get Port of Discharge from Consol", uNLOCO2.RL_Code, OrderWrapperWithShip.PortOfDischarge.Code);

			OrderWithDec.Declaration[BaseJobDeclaration.Schema.JE_RL_NKPortOfArrival] = uNLOCO2.RL_Code;
			AssertEquals("Should get Port of Discharge from Dec", uNLOCO2.RL_Code, OrderWrapperWithDec.PortOfDischarge.Code);

			AssertEquals("Port of Discharge As String", uNLOCO2.RL_Code + " = " + uNLOCO2.Description + ", " + uNLOCO2.Country.Description, OrderWrapperWithDec.PortOfDischargeAsString);
		}

		public void TestPortOfLoading()
		{
			var uNLOCO1 = Factory.New<RefUNLOCO>();
			uNLOCO1.RL_Code = "USEWO";

			var uNLOCO2 = Factory.New<RefUNLOCO>();
			uNLOCO2.RL_Code = "AUBBB";

			fOrder.JD_RL_NKPortOfLoading = ZString.Empty;
			AssertNull("PortOfLoading", OrderWrapper.PortOfLoading);

			fOrder.JD_RL_NKGoodsAvailableAt = uNLOCO1.RL_Code;
			fOrder.JD_RL_NKPortOfLoading = uNLOCO2.RL_Code;

			AssertNotNull("PortOfLoading", OrderWrapper.PortOfLoading);
			AssertEquals("PortOfLoading is of type DocUNLOCO", typeof(DocUNLOCO), OrderWrapper.PortOfLoading.GetType());

			CommonConsol consol = OrderWithShip.Shipment.Consols[0];
			consol.JK_RL_NKLoadPort = uNLOCO2.RL_Code;
			AssertEquals("Should get Port from Consol", uNLOCO2.RL_Code, OrderWrapperWithShip.PortOfLoading.Code);

			OrderWithDec.Declaration[BaseJobDeclaration.Schema.JE_RL_NKPortOfLoading] = uNLOCO2.RL_Code;
			AssertEquals("Should get Port from Dec", uNLOCO2.RL_Code, OrderWrapperWithDec.PortOfLoading.Code);
		}

		public void TestShippingLine()
		{
			AssertNull("ShippingLine", OrderWrapper.ShippingLine);
			var line = Factory.LoadTop1<OrgHeader>(new ZQuery());
			fOrder.JD_OH_Carrier = line.PK;
			AssertNotNull("ShippingLine", OrderWrapper.ShippingLine);
			AssertEquals("ShippingLine is of type DocOrganisation", typeof(DocOrganisation), OrderWrapper.ShippingLine.GetType());

			OrderWithShip.Shipment.Consols[0].SetDefaultShippingLineAddress(line);
			DocBaseWrapper intermediateWrapper = (DocBaseWrapper)OrderWrapperWithShip.ShippingLine.WrappedObject;
			AssertEquals("Shipping Line should come from Orders Shipment", line.OH_FullName, ((OrgHeader)intermediateWrapper.WrappedObject).OH_FullName);
			AssertEquals("Shipping Line should be blank if attached to a dec", null, OrderWrapperWithDec.ShippingLine);
		}

		#endregion

		#endregion

		#region IPreAlert

		public void TestCompleteRouting()
		{
			ZDateTime now = ZDateTime.Now;

			fOrder.JD_TransportMode = Core.Constants.TransportModes.Air;
			fOrder.JD_JS = ZGuid.Empty;
			fOrder.JD_JE = ZGuid.Empty;
			fOrder.JD_RL_NKPortOfLoading = "AUBNE";
			fOrder.JD_RL_NKPortOfDischarge = "SGSIN";
			fOrder.JD_DepartureVoyage = "QF1234";

			AssertEquals("order should have a single leg", 1, OrderWrapper.CompleteRouting.Count);
			AssertEquals("should be the correct leg type", OrderSource.Leg.SingleLeg, (OrderSource.Leg)(int)OrderWrapper.CompleteRouting[0].LegOrder);

			fOrder.JD_ArrivalVoyage = "QF2345";

			AssertEquals("order should now have 2 legs", 2, OrderWrapper.CompleteRouting.Count);
			AssertEquals("first leg should be of the correct type", OrderSource.Leg.Departure, (OrderSource.Leg)(int)OrderWrapper.CompleteRouting[0].LegOrder);
			AssertEquals("second leg should be of the correct type", OrderSource.Leg.Arrival, (OrderSource.Leg)(int)OrderWrapper.CompleteRouting[1].LegOrder);

			fOrder.JD_IntermediateVoyage = "QF3456";

			AssertEquals("order should now have 3 legs", 3, OrderWrapper.CompleteRouting.Count);
			AssertEquals("first leg should be of the correct type", OrderSource.Leg.Departure, (OrderSource.Leg)(int)OrderWrapper.CompleteRouting[0].LegOrder);
			AssertEquals("second leg should be of the correct type", OrderSource.Leg.Intermediate, (OrderSource.Leg)(int)OrderWrapper.CompleteRouting[1].LegOrder);
			AssertEquals("third leg should be of the correct type", OrderSource.Leg.Arrival, (OrderSource.Leg)(int)OrderWrapper.CompleteRouting[2].LegOrder);

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs._CustomsTemplate_.IJobDeclaration>();
			fOrder.JD_JE = declaration.PK;

			AssertEquals("declaration takes precidence over order", 1, OrderWrapper.CompleteRouting.Count);
			AssertEquals("should be the correct leg type", typeof(DeclarationSource), OrderWrapper.CompleteRouting[0].WrappedObject.GetType());

			CommonShipment shipment = Factory.New<CommonShipment>();
			shipment.Transports.AddNew();
			shipment.Transports.AddNew();
			fOrder.JD_JS = shipment.PK;

			AssertEquals("shipment takes precidence over order and declaration", 2, OrderWrapper.CompleteRouting.Count);
			AssertEquals("should be the correct leg type", typeof(TransportSource), OrderWrapper.CompleteRouting[0].WrappedObject.GetType());
			AssertEquals("should be the correct leg type", typeof(TransportSource), OrderWrapper.CompleteRouting[1].WrappedObject.GetType());
		}

		public void TestPortDisplayMode()
		{
			AssertEquals("LoadDischargeCollectDeliver", OrderWrapper.PortDisplayMode);
		}

		public void TestShowChargesOnArrivalNotice()
		{
			AssertEquals(false, OrderWrapper.ShowChargesOnArrivalNotice);
		}

		public void TestShowExchangeRatesOnArrivalNotice()
		{
			AssertEquals(false, OrderWrapper.ShowExchangeRatesOnArrivalNotice);
		}

		public void TestPreAlertReference()
		{
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			AssertEquals("ORDER NUMBERS / REFERENCE", OrderWrapper.PreAlertReferenceHeading);

			AssertEquals("", OrderWrapper.PreAlertReference);

			fOrder.JD_OrderNumber = "order";
			AssertEquals("order", OrderWrapper.PreAlertReference);

			Enterprise.Customs.AU.Declaration.Business.JobDeclaration dec1 = Factory.New<Enterprise.Customs.AU.Declaration.Business.JobDeclaration>();
			dec1.JE_GB = GlbBranch.CurrentBranch.PK;
			dec1.JE_OwnerRef = "OWNERS REFERENCE";

			fOrder.JD_JE = dec1.PK;

			AssertEquals("order OWNERS REFERENCE", OrderWrapper.PreAlertReference);

			dec1.JE_OwnerRef = "order";
			AssertEquals("order", OrderWrapper.PreAlertReference);
		}

		public void TestOriginLoco()
		{
			RefUNLOCO originPort = GlbBranch.CurrentBranch.HomePort;
			var loadingPort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, originPort.RL_Code));
			fOrder.JD_RL_NKGoodsAvailableAt = originPort.RL_Code;
			fOrder.JD_RL_NKPortOfLoading = loadingPort.RL_Code;
			fOrder.JD_JS = ZGuid.Empty;
			fOrder.JD_JE = ZGuid.Empty;

			AssertEquals("Should come from Origin on Order", originPort.RL_Code, OrderWrapper.OriginLoco.Code);
		}

		public void TestDestinationLoco()
		{
			RefUNLOCO destinationPort = GlbBranch.CurrentBranch.HomePort;
			var dischargePort = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.NotEqual, destinationPort.RL_Code));
			fOrder.JD_RL_NKGoodsDeliveredTo = destinationPort.RL_Code;
			fOrder.JD_RL_NKPortOfDischarge = dischargePort.RL_Code;
			fOrder.JD_JS = ZGuid.Empty;
			fOrder.JD_JE = ZGuid.Empty;

			AssertEquals("Should come from destination on Order", destinationPort.RL_Code, OrderWrapper.DestinationLoco.Code);
		}

		#endregion

		#region IRequestForMissingDocuments

		public void TestContainerNumbers()
		{
			var order = Factory.NewWithValidTestData<Order>();
			OrderContainer container1 = order.PlannedContainers.AddNew();
			container1.J1_ContainerNumber = "Container1";
			OrderContainer container2 = order.PlannedContainers.AddNew();
			container2.J1_ContainerNumber = "Container2";
			OrderContainer container3 = order.PlannedContainers.AddNew();
			container3.J1_ContainerNumber = "Container3";

			OrderWrapper = DocOrder.New(order, Factory);
			AssertEquals("Container1, Container2, Container3", OrderWrapper.ContainerNumbers);
		}

		public void TestETAString()
		{
			Order order = Factory.NewWithValidTestData<Order>(TestBusinessObjectKind.MinimumRequiredToSave);
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.UpdateEventEstimate(Events.Arrival, new ZDateTimeOffset(2005, 12, 23));

			OrderWrapper = DocOrder.New(order, Factory);
			AssertEquals(order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime().ToShortDateString(), OrderWrapper.ETAString);

			order.JD_TransportMode = Core.Constants.TransportModes.Air;

			OrderWrapper = DocOrder.New(order, Factory);
			AssertEquals(order.GetMilestoneEstimatedDate(Events.Arrival).ToZDateTime().ToLongTimeString(), OrderWrapper.ETAString);
		}

		public void TestETDString()
		{
			Order order = Factory.NewWithValidTestData<Order>(TestBusinessObjectKind.MinimumRequiredToSave);
			order.JD_TransportMode = Core.Constants.TransportModes.Sea;
			order.UpdateEventEstimate(Events.Departure, new ZDateTimeOffset(2005, 12, 23));

			OrderWrapper = DocOrder.New(order, Factory);
			AssertEquals(order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime().ToShortDateString(), OrderWrapper.ETDString);

			order.JD_TransportMode = Core.Constants.TransportModes.Air;

			OrderWrapper = DocOrder.New(order, Factory);
			AssertEquals(order.GetMilestoneEstimatedDate(Events.Departure).ToZDateTime().ToLongTimeString(), OrderWrapper.ETDString);
		}

		public void TestOwnerRefAndOrderRefHeading()
		{
			AssertEquals("ORDER NUMBERS / REFERENCE", OrderWrapper.OwnerRefAndOrderRefHeading);
		}

		public void TestConsigneeOrg()
		{
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var order = Factory.New<Order>();
			order.BuyerPK = header.PK;
			DocOrder orderWrapper = DocOrder.New(order, Factory);
			AssertEquals("Should return Consignee", orderWrapper.Consignee.Code, orderWrapper.ConsigneeOrg.Code);
		}

		public void TestConsignorOrg()
		{
			var order = Factory.New<Order>();
			var header = Factory.LoadTop1<OrgHeader>(new ZQuery());
			order.SupplierPK = header.PK;
			DocOrder orderWrapper = DocOrder.New(order, Factory);
			AssertEquals("Should return Consignor", orderWrapper.Consignor.Code, orderWrapper.ConsignorOrg.Code);
		}

		public void TestConsigneeOrgHeading()
		{
			AssertEquals("CONSIGNEE", OrderWrapper.ConsigneeOrgHeading);
		}

		public void ConsignorOrgHeading()
		{
			AssertEquals("CONSIGNOR", OrderWrapper.ConsignorOrgHeading);
		}

		#endregion

		#region ITrackingBusinessObject

		public void TestTrackingBusinessContext()
		{
			AssertEquals(TrackingConstants.BusinessContext.Order, OrderWrapper.TrackingBusinessContext);
		}

		public void TestTrackingBusinessObjectPK()
		{
			AssertEquals(fOrder.PK, OrderWrapper.TrackingBusinessObjectPK);
		}

		#endregion

		#region Implementation

		Order fOrder;
		Order OrderWithShipWithoutConsol;
		Order OrderWithShip;
		Order OrderWithDec;
		CommonShipment ShipWithOrder;
		BaseJobDeclaration DecWithShip;
		DocOrder OrderWrapperWithShipWithoutConsol;
		DocOrder OrderWrapperWithShip;
		DocOrder OrderWrapperWithDec;
		DocOrder OrderWrapper;
		OrgHeader buyer;

		protected override string TestingCountry
		{
			get { return null; }
		}

		protected override void SetUp()
		{
			StoredCountry = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.SetCountry(Enterprise.Core.Constants.CountryCodes.Australia);

			fOrder = Factory.New<Order>();
			OrderWithShip = Factory.New<Order>();
			ShipWithOrder = Factory.New<ForwardingShipment>();

			OrderWithShipWithoutConsol = Factory.New<Order>();
			ForwardingShipment ship = Factory.New<ForwardingShipment>();
			OrderWithShipWithoutConsol.JD_JS = ship.PK;
			OrderWrapperWithShipWithoutConsol = DocOrder.New(OrderWithShipWithoutConsol, Factory);

			OrderWithDec = Factory.New<Order>();
			DecWithShip = Factory.New<BaseJobDeclaration>();

			OrderWrapper = DocOrder.New(fOrder, Factory);
			fOrder.JD_JS = ZGuid.Empty;
			ForwardingConsol shipConsol = (ForwardingConsol)ShipWithOrder.Consols.AddNew();

			OrderWithShip.JD_JS = ShipWithOrder.PK;
			OrderWithShip.JD_JE = ZGuid.Empty;
			OrderWithDec.JD_JE = DecWithShip.PK;
			OrderWithDec.JD_JS = ZGuid.Empty;
			OrderWrapperWithShip = DocOrder.New(OrderWithShip, Factory);
			OrderWrapperWithDec = DocOrder.New(OrderWithDec, Factory);

			OrgHeader shippingLine = Factory.NewWithValidTestData<OrgHeader>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_Name = "Vessel";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_OH_Line = shippingLine.PK;

			var voyOrigin = voyage.Origins.AddNew();
			voyOrigin.JA_RL_NKPortOfLoading = CurrentCountryUNLOCO();
			voyOrigin.JA_E_DEP = new ZDateTime(2004, 02, 01);
			voyOrigin.JA_A_DEP = new ZDateTime(2004, 02, 02);

			var voyDestination = voyage.Destinations.AddNew();
			voyDestination.JB_RL_NKPortOfDischarge = NotCurrentCountryUNLOCO();
			voyDestination.JB_E_ARV = new ZDateTime(2004, 02, 03);
			voyDestination.JB_A_ARV = new ZDateTime(2004, 02, 04);

			JobSailing schedule = voyage.Sailings[0];
			schedule.Origin.JA_ReceivalCommences = new ZDateTime(2004, 02, 05);
			schedule.JX_DepotReceivalCommences = new ZDateTime(2004, 02, 06);
			schedule.Origin.JA_CutOff = new ZDateTime(2004, 02, 07);
			schedule.JX_DepotCutOff = new ZDateTime(2004, 02, 08);
			schedule.Origin.JA_DocumentaryCutoff = new ZDateTime(2004, 02, 08);
			schedule.Destination.JB_AvailabilityDate = new ZDateTime(2004, 02, 09);
			schedule.JX_DepotAvailabilityDate = new ZDateTime(2004, 02, 10);
			schedule.JX_ReservedMasterBill = "BookingRef";

			Transport transport = shipConsol.Transports[0];
			transport.JW_JX = schedule.PK;
			((BaseJobDeclaration)OrderWithDec.Declaration).JE_MessageType = "IMP";

			buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "BUY1";
			OrderWithShip.BuyerPK = buyer.PK;
			OrderWithShipWithoutConsol.BuyerPK = buyer.PK;
			OrderWithDec.BuyerPK = buyer.PK;
			fOrder.BuyerPK = buyer.PK;

			base.SetUp();
		}
		new string StoredCountry;

		protected override void TearDown()
		{
			GlbCompany.CurrentCompany.SetCountry(StoredCountry);
			base.TearDown();
		}

		void EnsureMilestoneExist(IWorkflowProvider job, Event evnt, string eventContext = "", bool respondToCascadedEvents = true)
		{
			var task = job.WorkflowItems.Milestones[evnt];
			if (task == null)
			{
				task = job.WorkflowItems.Milestones.AddNew();
				task.TriggerConditions.TriggerEventCode = evnt.Code;
			}

			task.P9_CascadedEventsContext = eventContext;
			task.P9_RespondToCascadedEvents = respondToCascadedEvents;
		}

		Order GetOrderWithShipment
		{
			get
			{
				var shipment = Factory.New<ForwardingShipment>();
				fOrder.JD_JS = shipment.PK;
				return fOrder;
			}
		}

		ForwardingShipment AddShipmentToOrderAndGetShipment
		{
			get
			{
				var shipment = Factory.New<ForwardingShipment>();
				fOrder.JD_JS = shipment.PK;
				OrderWrapper = DocOrder.New(fOrder, Factory);
				return shipment;
			}
		}

		OrgHeader CreateOrgHeader(ZString name)
		{
			var newOrg = Factory.NewWithValidTestData<OrgHeader>();
			newOrg.OH_FullName = name;
			return newOrg;
		}

		OrgSupplierBuyerLink CreateOrgSupplierBuyerLink(OrgHeader parent, ZGuid jE_OH_Supplier, ZString transportMode, ZString containerMode, ZGuid importBrokerPK)
		{
			OrgSupplierBuyerLink link = parent.SupplierLinks.AddNew();
			link.OL_OH_Supplier = jE_OH_Supplier;
			SetupTrnMode(link.OrgSupBuyLinkTrnModes[0], transportMode, containerMode, importBrokerPK);
			return link;
		}

		void SetupTrnMode(OrgSupBuyLinkTrnMode trnMode, ZString transportMode, ZString containerMode, ZGuid importBrokerPK)
		{
			trnMode.PF_TransportMode = transportMode;
			trnMode.PF_ContainerMode = containerMode;
			trnMode.PF_OH_ImportCustomsAgent = importBrokerPK;
		}

		ZString CurrentCountryUNLOCO()
		{
			return GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		}

		ZString NotCurrentCountryUNLOCO()
		{
			ZString result = "";
			ZQuery notLocalFilter = new ZQuery(RefUNLOCOSchema.RL_Code, SQLComparisonOperator.DoesNotStartWith, GlbBranch.CurrentBranch.GB_RL_NKHomePort);
			result = (Factory.LoadTop1<RefUNLOCO>(notLocalFilter)).RL_Code;
			return result;
		}
		#endregion
	}
}
