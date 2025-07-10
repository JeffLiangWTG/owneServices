using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Billing.Integration;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	public class ContainerCustomsDeclGeneratorTest : TestCaseWithFactory
	{
		#region TestExecute

		[TestDate(2010, 12, 12, 12, 12, 0)]
		public void TestExecute()
		{
			WoolworthsOrder order = Factory.New<WoolworthsOrder>();
			WoolworthsOrderLine orderLine = (WoolworthsOrderLine)order.OrderLines.AddNew();
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			OrderLineDeliverContainer container = delivery.Containers.AddNew();
			SetupOrderTestData(Factory, Buyer, Supplier, order, orderLine, delivery, container);
			container.J5_Voyage = "testvoy";
			Factory.Save();
			fGenerator.Execute(Buffer, false);
			AssertEquals("Should execute without problems", false, Buffer.HasErrors);
			JobDeclaration decl = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_VoyageFlightNo, "testvoy"));
			AssertNotNull("Exists declaration", decl);
			AssertDeclarationPopulatedWithTestDataCorrectly(decl, order, orderLine, delivery, container);
		}

		#region SetupOrderTestData
		void SetupOrderTestData(BusinessObjectFactory factory, OrgHeader buyer, OrgHeader supplier, WoolworthsOrder order, WoolworthsOrderLine orderLine, OrderLineDelivery delivery, OrderLineDeliverContainer container)
		{
			RefVessel arrivalVessel = factory.New<RefVessel>();
			arrivalVessel.RV_Code = "arvvessel";
			OrgAddress deliverPoint = buyer.Addresses.AddNew();
			deliverPoint.OA_Code = "addr";
			deliverPoint.OA_Address1 = "addr";
			// Declaration
			order.JD_ContainerMode = Enterprise.Core.Constants.ContainerModes.LCL;
			container.J5_ETA = ZDateTime.Now.AddDays(1);
			order.UpdateEvent(Events.Departure, ZDateTimeOffset.Now.AddDays(1));
			CommonShipment shipment = factory.New<CommonShipment>();
			shipment.JS_UniqueConsignRef = "S1010";
			order.JD_JS = shipment.PK;
			container.J5_MasterBill = "masterb";
			OrgHeader sendingAgent = factory.New<OrgHeader>();
			sendingAgent.OH_Code = "sndg";
			sendingAgent.MainAddress.OA_Address1 = "sndg";
			order.JD_OH_SendingAgent = sendingAgent.PK;
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			delivery.J4_RL_NKDestinationPort = "dsprt";
			delivery.J4_OA_NKDeliveryPoint = deliverPoint.OA_Code;
			container.J5_MasterBill = "MasterBill";
			container.J5_RV_NKArrivalVessel = arrivalVessel.RV_Code;
			order.JD_TransportMode = Enterprise.Core.Constants.TransportModes.Sea;
			container.J5_Weight = 5;
			container.J5_Volume = 6;
			container.J5_QuantityInvoiced = 7;
			container.J5_PackCount = 7;
			orderLine.JO_OuterPacks = 2;
			OrgSupplierBuyerLink link = factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = buyer.PK;
			link.OL_OH_Supplier = supplier.PK;
			link.OL_ValuationBasis = EDIFICEValuationBasis.Line_Codes.RelatedTransaction;
			// Customs Container
			container.J5_ContainerNum = "contnum";
			container.J5_RC_NKContainerType = "20GP";
			orderLine.JO_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Unit;
			container.J5_ContainerSeal = "contseal";
			container.J5_Weight = 7;
			// Invoice
			order.JD_IncoTerm = "CIP";
			order.JD_RX_NKOrderCurrency = "HKD";
			container.J5_Volume = 2;
			container.J5_VolumeUQ = Enterprise.Core.Constants.Volume.CubicFeet;
			container.J5_Weight = 3;
			container.J5_WeightUQ = Enterprise.Core.Constants.Weight.Pounds;
			container.J5_PackCount = 11;
			// Invoice Line
			container.J5_PackCount = 30;
			container.J5_F3_NKPackType = Enterprise.Core.Constants.PkgUnit.Unit;
			orderLine.JO_ItemPrice = 0.2m;
			orderLine.JO_Quantity = 60;
			orderLine.JO_LinePrice = 12;
			order.JD_OrderNumber = "ordnum";
			orderLine.JO_Partno = "partx";
			container.J5_Volume = 8;
			container.J5_VolumeUQ = "x";
			container.J5_Weight = 9;
			container.J5_WeightUQ = "y";
			container.J5_CustomAttribute1 = "USSPL";
			// Part for the order
			AUOrgSupplierPart product = factory.New<AUOrgSupplierPart>();
			product.OP_PartNum = "partx";
			Classification @class = product.ClassificationsForBinding.AddNew();
			@class.CC_LookupCode = "TestLookup";
			@class.CC_TariffNum = "2203.00.69 20";
			@class.CC_ClassificationType = "IMP";
			product.PivotsForBinding[0].CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTI;
			product.RelatedOrganisations.AddOrganisationIfNotExist(buyer.PK, OrgPartRelation.RelationshipTypes.Owner);
			product.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, OrgPartRelation.RelationshipTypes.Supplier);
		}

		#endregion
		#region AssertDeclarationPopulatedWithTestDataCorrectly
		void AssertDeclarationPopulatedWithTestDataCorrectly(JobDeclaration decl, WoolworthsOrder order, WoolworthsOrderLine orderLine, OrderLineDelivery delivery, OrderLineDeliverContainer container)
		{
			// Declaration
			AssertEquals(1, (int)decl.JE_ContainerCount);
			AssertEquals(Core.Constants.ContainerModes.FCL, decl.JE_ContainerMode);
			AssertEquals(container.J5_ETA, decl.JE_DateOfArrival);
			AssertEquals(container.J5_ETA, decl.JE_DateOfFirstArrival);
			AssertEquals(container.J5_ETA, decl.JE_DateAtFinalDestination);
			AssertEquals(container.J5_ETD, decl.JE_ExportDate);
			AssertEquals(container.J5_ETD, decl.JE_DateAtOrigin);
			AssertEquals(container.J5_MasterBill, decl.JE_MasterBill);
			AssertEquals(Customs.Business.JobMessageTypeList.Codes.Import, decl.JE_MessageType);
			AssertEquals(delivery.J4_OA_NKDeliveryPoint, decl.ImporterDeliveryAddress.Address.OA_Code);
			AssertEquals(order.JD_OH_SendingAgent, decl.JE_OH_Forwarder);
			AssertEquals(order.BuyerPK, decl.JE_OH_Importer);
			AssertEquals(order.SupplierPK, decl.JE_OH_Supplier);
			AssertEquals(delivery.J4_RL_NKDestinationPort, decl.JE_RL_NKFinalDestination);
			AssertEquals(container.J5_RL_NKLoadPort, decl.JE_RL_NKPortOfLoading);
			AssertEquals(container.J5_RV_NKArrivalVessel, decl.JE_VesselName);
			AssertEquals(order.JD_TransportMode, decl.JE_TransportMode);
			AssertEquals(EDIFICEValuationBasis.Line_Codes.RelatedTransaction, decl.AddInfo.ZA_ValuationBasis_Hidden);
			AssertEquals(container.J5_Weight, decl.JE_TotalWeight);
			AssertEquals(container.J5_Volume, decl.JE_TotalVolume);
			AssertEquals((decimal)container.J5_PackCount, (decimal)(int)decl.JE_TotalNoOfPacks);
			AssertEquals("Bill to Party", order.Buyer.MainAddress.PK, decl.Job.JH_OA_LocalChargesAddr);
			// Customs Container
			AssertEquals(Enterprise.Core.Constants.ContainerModes.FCL, decl.CusContainers[0].CO_FCL_LCL_AIR);
			AssertEquals(container.J5_ContainerNum.ToUpper(), decl.CusContainers[0].CO_ContainerNumber);
			AssertEquals(container.J5_ContainerSeal.ToUpper(), decl.CusContainers[0].CO_Seal.ToUpper());
			AssertEquals(container.J5_Weight, decl.CusContainers[0].CO_Weight);
			AssertEquals(container.J5_RC_NKContainerType, decl.CusContainers[0].Container.RC_Code);
			// Invoice
			AssertEquals("C&I", decl.Invoices[0].JZ_IncoTerm);
			AssertEquals(12m, (decimal)decl.Invoices[0].JZ_InvoiceAmount);
			AssertEquals(order.Supplier.OH_Code.ToUpper(), decl.Invoices[0].JZ_InvoiceNumber.ToUpper());
			AssertEquals(order.BuyerPK, decl.Invoices[0].JZ_OH_Buyer);
			AssertEquals(order.SupplierPK, decl.Invoices[0].JZ_OH_Supplier);
			AssertEquals(order.JD_RX_NKOrderCurrency, decl.Invoices[0].JZ_RX_NKInvoice_Currency);
			AssertEquals(container.J5_Volume, decl.Invoices[0].JZ_Volume);
			AssertEquals(container.J5_VolumeUQ, decl.Invoices[0].JZ_VolumeUQ);
			AssertEquals(container.J5_Weight, decl.Invoices[0].JZ_Weight);
			AssertEquals(container.J5_WeightUQ, decl.Invoices[0].JZ_WeightUQ);
			AssertEquals(container.J5_PackCount, decl.Invoices[0].JZ_Nature10PackCount);
			// Invoice Line
			AssertEquals((decimal)container.J5_PackCount * container.OrderLineDelivery.OrderLine.JO_OuterPacks, (decimal)decl.InvoiceLines[0].JI_InvoiceQuantity);
			AssertEquals(12m, (decimal)decl.InvoiceLines[0].JI_LinePrice);
			AssertEquals(Enterprise.Core.Constants.PkgUnit.Unit, decl.InvoiceLines[0].JI_InvoiceUQ);
			AssertEquals(order.JD_OrderNumber, decl.InvoiceLines[0].JI_OrderNumber);
			AssertEquals(orderLine.JO_Partno, decl.InvoiceLines[0].JI_PartNo);
			Assert(decl.InvoiceLines[0].JI_CC != ZGuid.Empty);
			Assert(decl.InvoiceLines[0].JI_Tariff != ZString.Empty);
			AssertEquals(container.J5_Volume, decl.InvoiceLines[0].JI_Volume);
			AssertEquals(container.J5_VolumeUQ, decl.InvoiceLines[0].JI_VolumeUQ);
			AssertEquals(container.J5_Weight, decl.InvoiceLines[0].JI_Weight);
			AssertEquals(container.J5_WeightUQ, decl.InvoiceLines[0].JI_WeightUQ);
			AssertEquals(container.J5_CustomAttribute1.Substring(0, 2), decl.InvoiceLines[0].JI_CountryOfOrigin);
			AssertEquals(container.J5_CustomAttribute1.Substring(0, 2), decl.InvoiceLines[0].AddInfo.ZA_ORG);
			AssertEquals(container.J5_ContainerNum, (decl.InvoiceLines[0] as WoolworthsJobComInvoiceLine).ContainerNumber);
		}

		#endregion
		#endregion
		#region TestExecuteWithMultipleOrdersSameContainer
		public void TestDickSmithBranch()
		{
			AssertEquals("Dick Smith Branch code", "DSE", ContainerCustomsDeclGenerator.DickSmithBranch);
		}

		public void TestExecuteWithMultipleOrdersSameContainer()
		{
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.MainAddress.OA_Address1 = "addr1";
			supplier.OH_Code = "sup";
			OrderLineDeliverContainer container1 = CreateOrderAndContainer(Factory, supplier.PK, "buy1", "contnum", "voyage", "arvvessel", "part01", Enterprise.Core.Constants.TransportModes.Sea, "MasterBill", 1);
			WoolworthsOrder order1 = (WoolworthsOrder)container1.Order;
			OrderLineDeliverContainer container2 = CreateOrderAndContainer(Factory, supplier.PK, "buy2", "contnum", "voyage", "arvvessel", "part02", Enterprise.Core.Constants.TransportModes.Sea, "MasterBill", 2);
			WoolworthsOrder order2 = (WoolworthsOrder)container2.Order;
			Factory.Save();
			// create the declarations
			fGenerator.Execute(Buffer, false);
			AssertEquals("Should execute without problems", false, Buffer.HasErrors);
			JobDeclaration[] decls = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), new ZQuery(JobDeclarationSchema.JE_VoyageFlightNo, SQLComparisonOperator.Equal, "voyage"));
			AssertEquals("Even though there are 2 containers, they are actually the same container so only make 1 declaration", 1, decls.Length);
			JobDeclaration decl = decls[0];
			AssertEquals("Should be 1 declaration as there is only 1 supplier", 1, decl.Invoices.Count);
			AssertEquals("Should be 2 invoice lines, one for each order line", 2, decl.Invoices[0].JobComInvoiceLines.Count);
			AssertEquals("Woolworths only have 1 declaration per container", 1, (int)decl.JE_ContainerCount);
			Assert((decl.Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part01" && decl.Invoices[0].JobComInvoiceLines[1].JI_PartNo == "part02") || (decl.Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part02" && decl.Invoices[0].JobComInvoiceLines[1].JI_PartNo == "part01"));
		}

		#endregion
		#region TestMultipleSuppliersSameContainer
		public void TestMultipleSuppliersSameContainer()
		{
			OrderLineDeliverContainer container1 = CreateOrderAndContainer(Factory, "buy1", "sup1", "contnum", "voyage", "arvvessel", "part01", Enterprise.Core.Constants.TransportModes.Sea, 1);
			WoolworthsOrder order1 = (WoolworthsOrder)container1.Order;
			OrderLineDeliverContainer container2 = CreateOrderAndContainer(Factory, "buy2", "sup2", "contnum", "voyage", "arvvessel", "part02", Enterprise.Core.Constants.TransportModes.Sea, 2);
			WoolworthsOrder order2 = (WoolworthsOrder)container2.Order;
			Factory.Save();
			// create the declarations
			fGenerator.Execute(Buffer, false);
			AssertEquals("Should execute without problems", false, Buffer.HasErrors);
			JobDeclaration[] decls = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), new ZQuery(JobDeclarationSchema.JE_VoyageFlightNo, SQLComparisonOperator.Equal, "voyage"));
			AssertEquals("Even though there are 2 containers, they are actually the same container so only make 1 declaration", 1, decls.Length);
			JobDeclaration decl = decls[0];
			AssertEquals("Should be 2 invoices, one for each supplier", 2, decl.Invoices.Count);
			Assert((decl.Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part01" && decl.Invoices[1].JobComInvoiceLines[0].JI_PartNo == "part02") || (decl.Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part02" && decl.Invoices[1].JobComInvoiceLines[0].JI_PartNo == "part01"));
			Assert(((decl.Invoices[0].JobComInvoiceLines[0].JI_CustomDecimal1 == 1m && decl.Invoices[1].JobComInvoiceLines[0].JI_CustomDecimal1 == 2m) || (decl.Invoices[0].JobComInvoiceLines[0].JI_CustomDecimal1 == 2m && decl.Invoices[1].JobComInvoiceLines[0].JI_CustomDecimal1 == 1m)));
		}

		#endregion
		public void TestSameSupplierMultipleContainers()
		{
			OrderLineDeliverContainer container1 = CreateOrderAndContainer(Factory, Supplier.PK, "buy1", "contnum1", "voyage", "arvvessel1", "part01", Enterprise.Core.Constants.TransportModes.Sea, "MasterBill", 1);
			WoolworthsOrder order1 = (WoolworthsOrder)container1.Order;
			OrderLineDeliverContainer container2 = CreateOrderAndContainer(Factory, Supplier.PK, "buy2", "contnum2", "voyage", "arvvessel2", "part02", Enterprise.Core.Constants.TransportModes.Sea, "MasterBill", 2);
			WoolworthsOrder order2 = (WoolworthsOrder)container2.Order;
			Factory.Save();
			fGenerator.Execute(Buffer, false);
			AssertEquals("Should execute without problems", false, Buffer.HasErrors);
			JobDeclaration[] decls = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), new ZQuery(JobDeclarationSchema.JE_VoyageFlightNo, SQLComparisonOperator.Equal, "voyage"));
			AssertEquals("2 containers, 2 declarations", 2, decls.Length);
			AssertEquals("Should be 1 invoice on each declaration", 1, decls[0].Invoices.Count);
			AssertEquals("Should be 1 invoice on each declaration", 1, decls[1].Invoices.Count);
			Assert((decls[0].Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part01" && decls[1].Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part02") || (decls[0].Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part02" && decls[1].Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part01"));
		}

		public void TestOnlyCreateUncreatedDeclarations()
		{
			OrderLineDeliverContainer container1 = CreateOrderAndContainer(Factory, "bu1", "su1", "contnum1", "voyage", "arvvessel", "part", Enterprise.Core.Constants.TransportModes.Sea, 1);
			WoolworthsOrder order1 = (WoolworthsOrder)container1.Order;
			Factory.Save();
			fGenerator.Execute(Buffer, false);
			AssertEquals("No errors should ensue", false, Buffer.HasErrors);
			JobDeclaration[] decls = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), new ZQuery(JobDeclarationSchema.JE_VesselName, SQLComparisonOperator.Equal, "arvvessel"));
			AssertEquals("Should exist 1 declaration", 1, decls.Length);
			AssertEquals("Should exist correct declaration", "CONTNUM1", decls[0].CusContainers[0].CO_ContainerNumber);
			OrderLineDeliverContainer container2 = CreateOrderAndContainer(Factory, "bu2", "su2", "contnum2", "voyage", "arvvessel", "part", Enterprise.Core.Constants.TransportModes.Sea, 2);
			WoolworthsOrder order2 = (WoolworthsOrder)container2.Order;
			// make sure the filter below doesn't pick up this declaration
			decls[0].JE_VesselName = "splat";
			Factory.Save();
			Buffer = new NotificationBuffer(null);
			fGenerator.Execute(Buffer, false);
			AssertEquals("No errors should ensue", false, Buffer.HasErrors);
			decls = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), new ZQuery(JobDeclarationSchema.JE_VesselName, SQLComparisonOperator.Equal, "arvvessel"));
			AssertEquals("Should exist only the declaration for the most recent container", 1, decls.Length);
			AssertEquals("Should exist correct declaration", "CONTNUM2", decls[0].CusContainers[0].CO_ContainerNumber);
		}

		public void TestContainersUniqueNotIncludingVesselIfAir()
		{
			OrderLineDeliverContainer container1 = CreateOrderAndContainer(Factory, "buy1", "sup1", "contnum", "voyage", "arvvessel1", "part01", Enterprise.Core.Constants.TransportModes.Air, 1);
			WoolworthsOrder order1 = (WoolworthsOrder)container1.Order;
			OrderLineDeliverContainer container2 = CreateOrderAndContainer(Factory, "buy2", "sup2", "contnum", "voyage", "arvvessel2", "part02", Enterprise.Core.Constants.TransportModes.Air, 2);
			WoolworthsOrder order2 = (WoolworthsOrder)container2.Order;
			Factory.Save();
			fGenerator.Execute(Buffer, false);
			AssertEquals("Should execute without problems", false, Buffer.HasErrors);
			JobDeclaration[] decls = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), new ZQuery(JobDeclarationSchema.JE_VoyageFlightNo, SQLComparisonOperator.Equal, "voyage"));
			AssertEquals("Even though there are 2 containers with different vessels, they are actually the same container (transport mode is air) so only make 1 declaration", 1, decls.Length);
			JobDeclaration decl = decls[0];
			AssertEquals("Should be 2 invoices, one for each supplier", 2, decl.Invoices.Count);
			Assert((decl.Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part01" && decl.Invoices[1].JobComInvoiceLines[0].JI_PartNo == "part02") || (decl.Invoices[0].JobComInvoiceLines[0].JI_PartNo == "part02" && decl.Invoices[1].JobComInvoiceLines[0].JI_PartNo == "part01"));
		}

		public void TestGetCountryOfOriginFromContainer()
		{
			OrgHeader matchingOrg = Factory.New<OrgHeader>();
			OrgPatternMatchOverride matchOverride = Factory.New<OrgPatternMatchOverride>();
			matchOverride.OO_OH = matchingOrg.PK;
			matchOverride.OO_Relationship = Enterprise.Core.Constants.OrgPatternMatchOverrideRelationships.Country;
			matchOverride.OO_LocalCode = "CN";
			matchOverride.OO_ForeignCode = "HK";
			Order order = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			OrderLineDeliverContainer container = delivery.Containers.AddNew();
			container.J5_CustomAttribute1 = "HKXXX";
			ZString mappedCountryCode = new TestContainerCustomsDeclGenerator().GetCountryOfOriginFromContainer(container, matchingOrg.PK);
			AssertEquals("CN", mappedCountryCode);
		}

		public void TestDontCreateDeclarationWhenContainerNumberEmpty()
		{
			OrderLineDeliverContainer container1 = CreateOrderAndContainer(Factory, "", "voyage", "arvvessel1", "part01", Enterprise.Core.Constants.TransportModes.Air);
			WoolworthsOrder order1 = (WoolworthsOrder)container1.Order;
			Factory.Save();
			fGenerator.Execute(Buffer, false);
			AssertEquals("Should execute without problems", false, Buffer.HasErrors);
			JobDeclaration[] decls = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), new ZQuery(JobDeclarationSchema.JE_VoyageFlightNo, SQLComparisonOperator.Equal, "voyage"));
			AssertEquals("Should not create a declaration as the container number was empty", 0, decls.Length);
		}

		public void TestDontChangeToExportWhenSupplierInAU()
		{
			OrderLineDeliverContainer container1 = CreateOrderAndContainer(Factory, "container#", "voyage", "arvvessel1", "part01", Enterprise.Core.Constants.TransportModes.Air);
			WoolworthsOrder order1 = (WoolworthsOrder)container1.Order;
			OrgHeader supplier = Factory.New<OrgHeader>();
			supplier.OH_Code = "ausup";
			supplier.MainAddress.OA_Address1 = "ausupaddr";
			supplier.OH_RL_NKClosestPort = "AUMEL";
			order1.SupplierPK = supplier.PK;
			Factory.Save();
			fGenerator.Execute(Buffer, false);
			AssertEquals("Should execute without problems", false, Buffer.HasErrors);
			JobDeclaration[] decls = (JobDeclaration[])Factory.Load(typeof(JobDeclaration), new ZQuery(JobDeclarationSchema.JE_VoyageFlightNo, SQLComparisonOperator.Equal, "voyage"));
			AssertEquals("Should be 1 declaration created", 1, decls.Length);
			AssertEquals("Should be import even though supplier in AU", Customs.Business.JobMessageTypeList.Codes.Import, decls[0].JE_MessageType);
		}

		public void TestDeclarationsCreatedForAddedOrUpdatedOrderContainers_AndRecreatedIfPreviouslyCreatedDeclarationDeactivated()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ordernum";
			order.SupplierPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.BuyerPK = Buyer.PK;
			OrderLine orderLine = order.OrderLines.AddNew();
			OrderLineDelivery orderDelivery = orderLine.Deliveries.AddNew();
			orderDelivery.J4_RL_NKDestinationPort = "AUSYD";
			OrderLineDeliverContainer orderContainer = orderDelivery.Containers.AddNew();
			orderContainer.J5_ContainerNum = "tstcontainer";
			orderContainer.J5_RV_NKArrivalVessel = "vessel";
			orderContainer.J5_Voyage = "voyage";
			orderContainer.J5_MasterBill = "masterbill";
			Factory.Save();
			ZQuery containerFilter = new ZQuery(CusContainerSchema.CO_ContainerNumber, orderContainer.J5_ContainerNum)
			{ IgnoreActiveFilter = true };
			new TestContainerCustomsDeclGenerator().Execute(Buffer, false);
			CusContainer[] cusContainersAfterFirstGenerate = (CusContainer[])Factory.Load(typeof(CusContainer), containerFilter);
			AssertEquals("Container should be created from the order container", 1, cusContainersAfterFirstGenerate.Length);
			new TestContainerCustomsDeclGenerator().Execute(Buffer, false);
			CusContainer[] cusContainersAfterSecondGenerate = (CusContainer[])Factory.Load(typeof(CusContainer), containerFilter);
			AssertEquals("Container should not be re-imported", 1, cusContainersAfterSecondGenerate.Length);
			cusContainersAfterSecondGenerate[0].Declaration.JE_IsCancelled = true;
			orderContainer.J5_CustomAttribute1 = "x";
			Factory.Save();
			new TestContainerCustomsDeclGenerator().Execute(Buffer, false);
			CusContainer[] cusContainersAfterInitialDeclarationDeactivated = (CusContainer[])Factory.Load(typeof(CusContainer), containerFilter);
			AssertEquals("2 containers should be created, the deactivated one and this one", 2, cusContainersAfterInitialDeclarationDeactivated.Length);
		}

		public void TestPackingInfoPopulatedIfETAAfterCMRDate()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "OrderNum";
			order.SupplierPK = Factory.NewWithValidTestData(typeof(OrgHeader)).PK;
			order.BuyerPK = Buyer.PK;
			OrderLine orderLine = order.OrderLines.AddNew();
			OrderLineDelivery orderDelivery = orderLine.Deliveries.AddNew();
			orderDelivery.J4_RL_NKDestinationPort = "AUSYD";
			OrderLineDeliverContainer orderContainer = orderDelivery.Containers.AddNew();
			orderContainer.J5_MasterBill = "MasterBill";
			orderContainer.J5_ContainerNum = "TestContNum";
			orderContainer.J5_RV_NKArrivalVessel = "Vessel";
			orderContainer.J5_Voyage = "Voyage";
			orderContainer.J5_ETA = new ZDateTime(2005, 10, 12);
			orderContainer.J5_PackCount = 10;
			Factory.Save();
			new TestContainerCustomsDeclGenerator().Execute(new NotificationBuffer(), false);
			ZQuery containerFilter = new ZQuery(CusContainerSchema.CO_ContainerNumber, orderContainer.J5_ContainerNum);
			CusContainer cusContainer = Factory.LoadTop1<CusContainer>(containerFilter);
			Bill masterBill = (Bill)cusContainer.Declaration.Bills.FindByBillNumberAndType("MASTERBILL", Customs.Business.BillTypeList.Codes.MasterBill);
			AssertNotNull("Correct Master Bill", masterBill);
			AssertEquals("Correct CusContainer", cusContainer.PK, cusContainer.Declaration.PackingGroups[0].CR_CO_Container);
			AssertEquals("Correct No. of Packages", 10, ((PackingGroup)cusContainer.Declaration.PackingGroups[0]).TotalNumberOfPackages);
		}

		#region TestEndToEndDeclMerge
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestEndToEndDeclMerge()
		{
			using (StreamReader orders = new WowTestUtil().GetStreamReaderForTestFile("ContainerCustomsDeclGeneration\\Testing\\TestOrder.csv"))
			using (StreamReader containers = new WowTestUtil().GetStreamReaderForTestFile("ContainerCustomsDeclGeneration\\Testing\\TestContainers.csv"))
			{
				SetupRefDataForEndToEndDeclMerge(Factory); // PUT THIS BACK!
				TestWowDataImporter importer = new TestWowDataImporter(Factory);
				NotificationBuffer buffer = new NotificationBuffer(null);
				Factory.Save();
				importer.ImportDataToFactory(orders, "", buffer, SourceInfo.EmptySourceInfo, out ITransactionParticipant[] transactionActionsUnused);
				AssertEquals("No errors should ensue for order import: \n\n" + buffer.AsString, false, buffer.HasErrors);
				importer.ImportDataToFactory(containers, "", buffer, SourceInfo.EmptySourceInfo, out transactionActionsUnused);
				AssertEquals("No errors should ensue for container import: \n\n" + buffer.AsString, false, buffer.HasErrors);
				WoolworthsOrder order = Factory.LoadTop1<WoolworthsOrder>(new ZQuery(JobOrderHeaderSchema.JD_OrderNumber, SQLComparisonOperator.Equal, "P04946"));
				SetupOrderDetailsNotImportedFromMI(Factory, order);
				Factory.Save();
				WowDataRegistry.Instance.ContainerCustomsDeclLastCreated = ZDateTime.Now.AddSeconds(-5);
				new TestContainerCustomsDeclGenerator().Execute(buffer, false);
				AssertEquals("No errors should ensue for decl generator: \n\n" + buffer.AsString, false, buffer.HasErrors);
				AssertNotNull("No declaration was created", Factory.LoadTop1(typeof(JobDeclaration), new ZQuery()));
				JobDeclaration declaration = Factory.LoadTop1<JobDeclaration>(new ZQuery(JobDeclarationSchema.JE_VoyageFlightNo, SQLComparisonOperator.Equal, "voyg"));
				AssertNotNull("Declaration not created or voyage not set", declaration);
				SetupDeclarationDetailsNotAutoCopied(Factory, declaration);
				declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
				bool success = declaration.DoMerge();
				AssertEquals("Merge should succeed", true, success);
			}
		}

		void SetupRefDataForEndToEndDeclMerge(BusinessObjectFactory factory)
		{
			Supplier.OH_FullName = "W.GREAT WORTH LTD";
			Supplier.MiscServ.OM_RN_NKEXDefaultCntryOfOrigin = factory.LoadTop1<RefCountry>(new ZQuery()).RN_Code;
			Supplier.SetLocalCustomsCode(OrgCusCode.CodeTypes.SupplierCode, "1234567V");
			AssertEquals("Customs registration no. set correctly for test", "1234567V", Supplier.LocalCustomsSupplierCode);
			Buyer.OH_FullName = "LI & FUNG (TRADING) LTD.";
			OrgSupplierBuyerLink link = factory.New<OrgSupplierBuyerLink>();
			link.OL_OH_Buyer = Buyer.PK;
			link.OL_OH_Supplier = Supplier.PK;
			link.OL_ValuationBasis = EDIFICEValuationBasis.Line_Codes.RelatedTransaction;
			WoolworthsProduct product1 = factory.New<WoolworthsProduct>();
			OrgPartRelation newRelation1 = product1.RelatedOrganisations.AddNew();
			newRelation1.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			newRelation1.OU_OH = Buyer.PK;
			product1.OP_PartNum = "part01";
			product1.OP_Desc = "part01_desc";
			product1.OP_StockKeepingUnit = Enterprise.Core.Constants.PkgUnit.Unit;
			AUOrgSupplierPart product2 = factory.New<AUOrgSupplierPart>();
			OrgPartRelation newRelation2 = product2.RelatedOrganisations.AddNew();
			newRelation2.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			newRelation2.OU_OH = Buyer.PK;
			product2.OP_PartNum = "part02";
			product2.OP_Desc = "part02_desc";
			product2.OP_StockKeepingUnit = Enterprise.Core.Constants.PkgUnit.Unit;
			Classification newClass1 = product1.ClassificationsForBinding.AddNew();
			Classification newClass2 = product2.ClassificationsForBinding.AddNew();
			newClass1.CC_ClassificationType = Classification.ClassificationType.IMP;
			newClass2.CC_ClassificationType = Classification.ClassificationType.IMP;
			newClass1.CC_LookupCode = "lcode1";
			newClass2.CC_LookupCode = "lcode2";
			newClass1.CC_Description = "desc1";
			newClass2.CC_Description = "desc2";
			newClass1.CC_TariffNum = "4602.90.00 12";
			newClass2.CC_TariffNum = "4602.90.00 12";
			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
			pivot1.CI_CC = newClass1.PK;
			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
			pivot2.CI_CC = newClass2.PK;
			OrgAddress addr1 = factory.New<OrgAddress>();
			addr1.OA_Code = "1904";
			addr1.OA_OH = Buyer.PK;
			addr1.OA_RL_NKRelatedPortCode = "USAAA";
			addr1.OA_Address1 = "xyz1xxxx";
			OrgAddress addr2 = factory.New<OrgAddress>();
			addr2.OA_Code = "2899";
			addr2.OA_OH = Buyer.PK;
			addr2.OA_RL_NKRelatedPortCode = "USAAA";
			addr2.OA_Address1 = "xyz2xxxx";
			RefVessel vessel = factory.New<RefVessel>();
			vessel.RV_Code = "OOCL MELBOURNE";
			vessel.RV_LloydsNumber = "lloyd";
			RefExchangeRate customsRate = factory.New<RefExchangeRate>();
			customsRate.RE_RX_NKExCurrency = "USD";
			customsRate.RE_SellRate = 0.5m;
			customsRate.RE_StartDate = new ZDateTime(2003, 12, 1);
			customsRate.RE_ExpiryDate = new ZDateTime(2003, 12, 29);
			customsRate.RE_ExRateType = Enterprise.Core.Constants.ExchangeRateTypes.Code.CustomsRate;
		}

		// These fields are not set up automatically by the MI import process and must be entered manually by the user.
		void SetupOrderDetailsNotImportedFromMI(BusinessObjectFactory factory, WoolworthsOrder order)
		{
			order.UpdateEvent(Events.Departure, new ZDateTimeOffset(2003, 12, 11));
			order.JD_RX_NKOrderCurrency = "USD";
		}

		void SetupDeclarationDetailsNotAutoCopied(BusinessObjectFactory factory, JobDeclaration decl)
		{
			foreach (JobComInvoiceLine line in decl.InvoiceLines)
			{
				line.AddInfo.ZA_PRF = "X";
			}

			// TODO: these will be automatically populated soon..
			decl.JE_RL_NKPortOfFirstArrival = "USAAA";
			decl.JE_DateOfFirstArrival = new ZDateTime(2003, 12, 10);
		}

		#endregion
		#region Implementation
		protected override void SetUp()
		{
			base.SetUp();
			Buffer = new NotificationBuffer(null);
			fGenerator = new TestContainerCustomsDeclGenerator();
			WowDataRegistry.Instance.DeclarationImporter = Buyer.PK.ToGuid();
			string clientXmlFilePath = BaseSourcePath + @"Enterprise\ClientExtensions\WOW\Documents\WOWDocuments.xml";
			var task = new DbUpgrader.Data.ClientDocumentsUpgradeTask(clientXmlFilePath);
			task.Run();
		}

		TestContainerCustomsDeclGenerator fGenerator;
		NotificationBuffer Buffer;
		#region Buyer
		OrgHeader Buyer
		{
			get
			{
				if (fBuyer == null)
				{
					fBuyer = Factory.New<OrgHeader>();
					fBuyer.MainAddress.OA_Address1 = "addr1";
					fBuyer.OH_Code = "buyer";
					fBuyer.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
					fBuyer.MiscServ.OM_RX_NKEXDefCurrency = "HKD";
				}

				return fBuyer;
			}
		}

		OrgHeader fBuyer;
		#endregion
		#region Supplier
		OrgHeader Supplier
		{
			get
			{
				if (fSupplier == null)
				{
					fSupplier = Factory.New<OrgHeader>();
					fSupplier.MainAddress.OA_Address1 = "addr1";
					fSupplier.OH_Code = "supplier";
					fSupplier.MiscServ.OM_RX_NKEXDefCurrency = "HKD";
				}

				return fSupplier;
			}
		}

		OrgHeader fSupplier;
		#endregion
		OrderLineDeliverContainer CreateOrderAndContainer(BusinessObjectFactory factory, ZString containerNum, ZString voyage, ZString arrivalVessel, ZString orderLinePartno, ZString transportMode)
		{
			return CreateOrderAndContainer(factory, "buy", "sup", containerNum, voyage, arrivalVessel, orderLinePartno, transportMode, 1);
		}

		OrderLineDeliverContainer CreateOrderAndContainer(BusinessObjectFactory factory, ZString buyerCode, ZString supplierCode, ZString containerNum, ZString voyage, ZString arrivalVessel, ZString orderLinePartno, ZString transportMode, int lineNo)
		{
			OrgHeader supplier = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, SQLComparisonOperator.Equal, supplierCode));
			if (supplier == null)
			{
				supplier = factory.New<OrgHeader>();
				supplier.OH_Code = supplierCode;
				supplier.MainAddress.OA_Address1 = "addy";
			}

			return CreateOrderAndContainer(factory, supplier.PK, buyerCode, containerNum, voyage, arrivalVessel, orderLinePartno, transportMode, "MasterBill", lineNo);
		}

		OrderLineDeliverContainer CreateOrderAndContainer(BusinessObjectFactory factory, ZGuid supplierPK, ZString buyerCode, ZString containerNum, ZString voyage, ZString arrivalVessel, ZString orderLinePartno, ZString transportMode, ZString masterBill, int lineNo)
		{
			OrgHeader buyer = factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, buyerCode));
			if (buyer == null)
			{
				buyer = factory.New<OrgHeader>();
				buyer.OH_Code = buyerCode;
				buyer.MainAddress.OA_Address1 = "addy";
				buyer.CompanyData.OB_GB_ControllingBranch = GlbBranch.CurrentBranch.PK;
			}

			Order order = factory.New<WoolworthsOrder>();
			OrderLine line = order.OrderLines.AddNew();
			OrderLineDelivery delivery = line.Deliveries.AddNew();
			delivery.J4_RL_NKDestinationPort = "AUSYD";
			OrderLineDeliverContainer container = delivery.Containers.AddNew();
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplierPK;
			order.JD_TransportMode = transportMode;
			order.JD_OrderNumber = "myord";
			line.JO_Partno = orderLinePartno;
			line.JO_LineNo = lineNo;
			container.J5_ContainerNum = containerNum;
			container.J5_Voyage = voyage;
			container.J5_RV_NKArrivalVessel = arrivalVessel;
			container.J5_MasterBill = masterBill;
			return container;
		}

		#region Test Classes
		class TestWowDataImporter : WowDataImporter
		{
			public TestWowDataImporter(BusinessObjectFactory factory) : base(new SingleBusinessObjectFactoryProvider(factory))
			{
			}

			public new bool ImportDataToFactory(TextReader data, string attachmentFileName, INotifications notify, ISourceInfo sourceInfo, out ITransactionParticipant[] transactionActions)
			{
				return base.ImportDataToFactory(data, attachmentFileName, notify, sourceInfo, out transactionActions);
			}
		}

		class TestContainerCustomsDeclGenerator : ContainerCustomsDeclGenerator
		{
			public new ZString GetCountryOfOriginFromContainer(OrderLineDeliverContainer container, ZGuid mappingOrgPK)
			{
				return base.GetCountryOfOriginFromContainer(container, mappingOrgPK);
			}

			public new ZQuery GetFilterForOrdersWhoseContainersModifiedOrAddedAfter(ZDateTime afterDate)
			{
				return base.GetFilterForOrdersWhoseContainersModifiedOrAddedAfter(afterDate);
			}
		}
		#endregion
		#endregion
	}
}
