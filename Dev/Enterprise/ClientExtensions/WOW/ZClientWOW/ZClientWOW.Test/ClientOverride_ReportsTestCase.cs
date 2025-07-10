using System.Data;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EntityFramework.Testing.DataAccess;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using OrgSupplierPart = Enterprise.Customs.Business.OrgSupplierPart;

namespace Enterprise.Client.Wow.Testing
{
	public class ClientOverride_ReportsTestCase : TestCaseWithFactory
	{
		[TestDate(1900, 1, 1)]
		public void TestOutstandingOrders()
		{
			#region SetUp
			OrgHeader buyer = Factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_FullName = "Buyer";
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Supplier";
			RefCurrency currency = Factory.LoadTop1<RefCurrency>(new ZQuery());
			RefUNLOCO portOfLoading = Factory.LoadTop1<RefUNLOCO>(new ZQuery());
			OrgSupplierPart part = Factory.New<OrgSupplierPart>();
			part.RelatedOrganisations.AddOrganisationIfNotExist(buyer.PK, "OWN", false);
			part.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, "SUP", false);
			part.OP_VendorPackQty = 2;
			part.OP_PartNum = "Part Number";
			part.OP_Department = "Part Department";
			part.OP_Division = "Part Division";
			// row 1
			Order order1 = Factory.New<Order>();
			order1.BuyerPK = buyer.PK;
			order1.SupplierPK = supplier.PK;
			order1.JD_OrderDate = new ZDateTime(1900, 1, 1);
			order1.JD_BookingConfDate = new ZDateTime(1900, 1, 3);
			order1.JD_OrderNumber = "ORD1";
			order1.JD_FirstBuyerContact = "Joe";
			order1.JD_SecondBuyerContact = "Jin";
			order1.JD_RL_NKPortOfLoading = portOfLoading.RL_Code;
			order1.JD_RX_NKOrderCurrency = currency.RX_Code;
			order1.JD_IsCancelled = false;
			OrderLine orderLine1 = order1.OrderLines.AddNew();
			orderLine1.JO_Partno = "Part Number";
			orderLine1.JO_CustomDate1 = new ZDateTime(1900, 1, 1);
			orderLine1.JO_Description = "Description";
			orderLine1.JO_ItemPrice = new ZDecimal(1.23);
			orderLine1.JO_Quantity = 30;
			OrderLineDelivery delivery1 = orderLine1.Deliveries.AddNew();
			delivery1.J4_CustomDecimal5 = 30;
			delivery1.J4_Allocated = 20;
			delivery1.J4_OA_NKDeliveryPoint = "Delivery Point";
			delivery1.J4_CustomAttribute1 = "CustomAttr1";
			// row 2
			Order order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_OrderDate = new ZDateTime(1900, 1, 1);
			order.JD_OrderNumber = "ORD2";
			order.JD_FirstBuyerContact = "Joe";
			order.JD_SecondBuyerContact = "Jin";
			order.JD_RL_NKPortOfLoading = portOfLoading.RL_Code;
			order.JD_RX_NKOrderCurrency = currency.RX_Code;
			order.JD_IsCancelled = false;
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_Partno = "Part Number";
			orderLine.JO_CustomDate1 = new ZDateTime(1900, 1, 1);
			orderLine.JO_Description = "Description";
			orderLine.JO_ItemPrice = new ZDecimal(1.23);
			orderLine.JO_Quantity = 30;
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			delivery.J4_CustomDecimal5 = 30;
			delivery.J4_Allocated = 20;
			delivery.J4_OA_NKDeliveryPoint = "Delivery Point";
			delivery.J4_CustomAttribute1 = "CustomAttr1";
			// row 3
			Order order3 = Factory.New<Order>();
			order3.BuyerPK = buyer.PK;
			order3.SupplierPK = supplier.PK;
			order3.JD_OrderDate = new ZDateTime(1900, 1, 1);
			order3.JD_BookingConfDate = new ZDateTime(1900, 1, 3);
			order3.JD_OrderNumber = "ORD3";
			order3.JD_FirstBuyerContact = "Joe";
			order3.JD_SecondBuyerContact = "Jin";
			order3.JD_RL_NKPortOfLoading = portOfLoading.RL_Code;
			order3.JD_RX_NKOrderCurrency = currency.RX_Code;
			order3.JD_IsCancelled = false;
			orderLine = order3.OrderLines.AddNew();
			orderLine.JO_Partno = "Part Number";
			orderLine.JO_CustomDate1 = new ZDateTime(1900, 1, 1);
			orderLine.JO_Description = "Description";
			orderLine.JO_ItemPrice = new ZDecimal(1.23);
			orderLine.JO_Quantity = 30;
			delivery = orderLine.Deliveries.AddNew();
			delivery.J4_CustomDecimal5 = 30;
			delivery.J4_Allocated = 20;
			delivery.J4_OA_NKDeliveryPoint = "Delivery Point";
			delivery.J4_CustomAttribute1 = "CustomAttr1";
			//Factory.Save();
			// row 4
			order = Factory.New<Order>();
			order.BuyerPK = buyer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_OrderDate = new ZDateTime(1900, 1, 1);
			order.JD_OrderNumber = "ORD4";
			order.JD_FirstBuyerContact = "Joe";
			order.JD_SecondBuyerContact = "Jin";
			order.JD_RL_NKPortOfLoading = portOfLoading.RL_Code;
			order.JD_RX_NKOrderCurrency = currency.RX_Code;
			order.JD_IsCancelled = false;
			orderLine = order.OrderLines.AddNew();
			orderLine.JO_Partno = "Part Number";
			orderLine.JO_CustomDate1 = new ZDateTime(1900, 1, 1);
			orderLine.JO_Description = "Description";
			orderLine.JO_ItemPrice = new ZDecimal(1.23);
			orderLine.JO_Quantity = 30;
			delivery = orderLine.Deliveries.AddNew();
			delivery.J4_CustomDecimal5 = 30;
			delivery.J4_Allocated = 30;
			delivery.J4_OA_NKDeliveryPoint = "Delivery Point";
			delivery.J4_CustomAttribute1 = "CustomAttr1";
			Factory.Save();
			#endregion
			OrderCollection orders = new OrderCollection(Factory);
			AssertEquals("Total orders", 4, orders.Count);
			DataTable table = Utilities.GetDataTableFromQuery(Db.Connection, SelectOutstandingOrders(ZString.Empty));
			AssertEquals("Total good orders", 3, table.Rows.Count);
			order3.JD_IsCancelled = true;
			Factory.Save();
			table = Utilities.GetDataTableFromQuery(Db.Connection, SelectOutstandingOrders(ZString.Empty));
			AssertEquals("Total good orders", 2, table.Rows.Count);
			table = Utilities.GetDataTableFromQuery(Db.Connection, SelectOutstandingOrders("NOTCONFIRMED"));
			AssertEquals("Rows returned", 1, table.Rows.Count);
			table = Utilities.GetDataTableFromQuery(Db.Connection, SelectOutstandingOrders("CONFIRMED"));
			AssertEquals("Rows returned", 1, table.Rows.Count);
			DataRow row = table.Rows[0];
			AssertEquals("Order date", order1.JD_OrderDate, new ZDateTime(row["OrderDate"]));
			AssertEquals("Order number", order1.JD_OrderNumber, row["OrderNumber"]);
			AssertEquals("Confirm Date", order1.JD_BookingConfDate, new ZDateTime(row["ConfirmDate"]));
			AssertEquals("Supplier Name", supplier.OH_FullName, row["SupplierFullName"]);
			AssertEquals("Buyer Name", buyer.OH_FullName, row["BuyerFullName"]);
			AssertEquals("Buyer first contact", order1.JD_FirstBuyerContact, row["BuyerContact"]);
			AssertEquals("Buyer second contact", order1.JD_SecondBuyerContact, row["RebuyerContact"]);
			AssertEquals("Port of loading", order1.JD_RL_NKPortOfLoading, row["PortOfLoading"]);
			AssertEquals("Port of loading name", portOfLoading.RL_PortName, row["PortOfLoadingName"]);
			AssertEquals("Ship date", orderLine1.JO_CustomDate1, new ZDateTime(row["ShipDate"]));
			AssertEquals("Orderline number", orderLine1.JO_Partno, row["Partno"]);
			AssertEquals("Orderline description", orderLine1.JO_Description, row["Description"]);
			AssertEquals("Part department", part.OP_Department, row["Department"]);
			AssertEquals("Part division", part.OP_Division, row["Division"]);
			AssertEquals("Currency", currency.RX_Code, row["Currency"]);
			AssertEquals("Orderline item price", orderLine1.JO_ItemPrice, new ZDecimal(row["LineItemPrice"]));
			AssertEquals("Quantity ordered by port", delivery1.J4_CustomDecimal5, new ZDecimal(row["QuantityOrderedByPort"]));
			AssertEquals("Quantity delivered by port", delivery1.J4_Allocated, new ZDecimal(row["QuantityDeliveredByPort"]));
			AssertEquals("Delivery point", delivery1.J4_OA_NKDeliveryPoint, row["DeliveryPoint"]);
			AssertEquals("POM number", delivery1.J4_CustomAttribute1, row["POMNum"]);
		}

		ZString SelectOutstandingOrders(ZString confirmDateCondition)
		{
			return "SELECT * FROM Client_WOW_OutstandingOrdersReport('" + confirmDateCondition + "') where OrderDate between '1/1/1900' and '2/1/1900'";
		}

		[TestDate(2006, 9, 15)]
		public void TestClientReportOrderCurrencyRequirement()
		{
			//RunClientDbCreateScripts();
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Bob The Builder";
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "Sloppy Joe";
			OrgSupplierPart product = Factory.NewWithValidTestData<OrgSupplierPart>();
			product.OP_VendorPackQty = 50;
			product.OP_Department = "Jenny Craig";
			product.OP_PartNum = "Body Fat Calculator";
			Factory.Save();
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);
			RefCurrency uSD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.UnitedStates);
			foreach (ZDateTime date in new object[] { ZDateTime.Today.AddDays(-20), ZDateTime.Today, ZDateTime.Today.AddDays(20) })
			{
				foreach (RefCurrency currency in new object[] { aUD, uSD })
				{
					CreateNewOrder(supplier, importer, product, date, currency);
				}
			}

			Factory.Save();
			string selectString = "select * from ClientReportOrderCurrencyRequirement('PAD','1-AUG-2006','31-AUG-2006')";
			DataTable table = Utilities.GetDataTableFromQuery(Db.Connection, selectString);
			AssertEquals("Expected 2 rows for 'PAD','1-AUG-2006','31-AUG-2006'", 2, table.Rows.Count);
			selectString = "select * from ClientReportOrderCurrencyRequirement('PAD','1-SEP-2006','30-SEP-2006')";
			table = Utilities.GetDataTableFromQuery(Db.Connection, selectString);
			AssertEquals("Expected 2 rows for '1-SEP-2006','31-SEP-2006'", 2, table.Rows.Count);
			selectString = "select * from ClientReportOrderCurrencyRequirement('WHD','1-SEP-2006','30-SEP-2006')";
			table = Utilities.GetDataTableFromQuery(Db.Connection, selectString);
			AssertEquals("Expected 4 rows for 'WHD','1-SEP-2006','31-SEP-2006'", 4, table.Rows.Count);
			selectString = "select * from ClientReportOrderCurrencyRequirement('WHD','1-OCT-2006','31-OCT-2006')";
			table = Utilities.GetDataTableFromQuery(Db.Connection, selectString);
			AssertEquals("Expected 2 rows for 'WHD','1-OCT-2006','31-OCT-2006'", 2, table.Rows.Count);
		}

		void CreateNewOrder(OrgHeader supplier, OrgHeader importer, OrgSupplierPart product, ZDateTime date, RefCurrency currency)
		{
			Order order = Factory.New<Order>();
			order.BuyerPK = importer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_ArrivalVoyage = "ArvVoyage";
			order.JD_RX_NKOrderCurrency = currency.RX_Code;
			order.JD_IncoTerm = "FOB";
			order.JD_OrderNumber = currency.RX_Code + date.ToString("YYYYMMDD");
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_OuterPacks = 30m;
			orderLine.JO_F3_NKPackType = Core.Constants.PkgUnit.Piece;
			orderLine.JO_ItemPrice = 8m;
			orderLine.JO_Partno = product.OP_PartNum;
			orderLine.JO_Quantity = 100.0m;
			orderLine.JO_QtyReceived = 50.0m;
			orderLine.JO_CustomDate2 = date;
			orderLine.JO_CustomDate3 = date.AddDays(10);
		}

		public void TestTotalQuantityInCartons()
		{
			Order order = Factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "1234";
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_Quantity = 4;
			orderLine.JO_OuterPacks = 2;
			Factory.Save();
			DbCommand command = Db.Connection.Command("select TotalQuantityInCartons from Client_WOW_Report_ClientOverseasConsolidatorOrderReport('','','','',NULL,'','') where OrderNumber='1234'");
			decimal totalQuantityInCartons = (decimal)command.ExecuteScalar();
			AssertEquals("TotalQuantityInCartons = 4/2 = 2; should be the total of JO_Quantity/JO_OuterPacks unless JO_OuterPacks=0", 2m, totalQuantityInCartons);
		}

		public void TestOrderContainersInYard()
		{
			#region SetUp
			Classification @class = Factory.New<Classification>();
			@class.CC_LookupCode = "12345";
			@class.CC_ClassificationType = BaseCusClassification.ClassificationType.Both;
			@class.CC_Description = "Classification Description";
			OrgHeader importerFromJobDec = Factory.NewWithValidTestData<OrgHeader>();
			importerFromJobDec.OH_FullName = "Silly Pig";
			OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_FullName = "Bob The Builder";
			OrgHeader importer = Factory.NewWithValidTestData<OrgHeader>();
			importer.OH_FullName = "Sloppy Joe";
			AUOrgSupplierPart product = Factory.NewWithValidTestData<AUOrgSupplierPart>();
			product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, "OWN");
			product.RelatedOrganisations.AddOrganisationIfNotExist(supplier.PK, "SUP");
			product.OP_VendorPackQty = 50;
			product.OP_Department = "Jenny Craig";
			product.OP_PartNum = "Body Fat Calculator";
			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = Customs.Business.ClassificationTypeList.Codes.HTB;
			pivot.CI_CC = @class.PK;
			Factory.Save();
			//Declaration
			JobDeclaration declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			declaration.JE_OH_Importer = importerFromJobDec.PK;
			declaration.JE_DateAtFinalDestination = ZDateTime.Now.AddDays(2);
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			CusContainer container1 = declaration.CusContainers.AddNew();
			container1.CO_ContainerNumber = "TNLU12121212";
			CommonContainer jobContainer = Factory.NewWithValidTestData<CommonContainer>();
			jobContainer.JC_FCLWharfGateOut = new ZDateTime(2006, 12, 10);
			container1.CO_JC = jobContainer.PK;
			RefContainer containerType = Factory.LoadTop1<RefContainer>(new ZQuery());
			container1.CO_RC = containerType.PK;
			JobComInvoiceHeader invoiceHeader = declaration.Invoices.AddNew();
			invoiceHeader.JZ_GroupInvoice = false;
			invoiceHeader.JZ_OH_Supplier = supplier.PK;
			invoiceHeader.JZ_OH_Buyer = importer.PK;
			RefCurrency aUD = RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Australia);
			invoiceHeader.JZ_RX_NKInvoice_Currency = aUD.RX_Code;
			Factory.Save();
			Order order = Factory.New<Order>();
			order.JD_JE = declaration.PK;
			order.BuyerPK = importer.PK;
			order.SupplierPK = supplier.PK;
			order.JD_ArrivalVoyage = "ArvVoyage";
			order.JD_RX_NKOrderCurrency = aUD.RX_Code;
			order.JD_IncoTerm = "FOB";
			order.JD_OrderNumber = "1234";
			OrderLine orderLine = order.OrderLines.AddNew();
			orderLine.JO_OuterPacks = 30m;
			orderLine.JO_F3_NKPackType = Core.Constants.PkgUnit.Piece;
			orderLine.JO_ItemPrice = 8m;
			orderLine.JO_Partno = product.OP_PartNum;
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			delivery.J4_OA_NKDeliveryPoint = "1234";
			delivery.J4_JO = orderLine.PK;
			delivery.J4_Allocated = 30;
			delivery.J4_RL_NKDestinationPort = "AUSYD";
			delivery.J4_CustomAttribute1 = "1000";
			OrderLineDeliverContainer container2 = delivery.Containers.AddNew();
			container2.J5_ContainerNum = container1.CO_ContainerNumber;
			container2.J5_PackCount = 13;
			container2.J5_QuantityInStore = 17;
			container2.J5_RV_NKArrivalVessel = order.JD_ArrivalVoyage;
			JobComInvoiceLine invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product.OP_PartNum;
			invoiceLine.JI_OP = product.PK;
			invoiceLine.JI_InvoiceQuantity = 30;
			invoiceLine.JI_LinePrice = 1500m;
			invoiceLine.JI_Description = "Eat more, exercise less, die early";
			invoiceLine.JI_OrderNumber = order.JD_OrderNumber;
			invoiceLine.JI_CustomAttrib4 = container2.J5_ContainerNum;
			invoiceLine.JI_CustomDecimal1 = new ZDecimal(orderLine.JO_LineNo);
			Factory.Save();
			#endregion
			DataTable table = Utilities.GetDataTableFromQuery(Db.Connection, "SELECT * FROM vw_Report_ClientOrderContainersInYard");
			AssertEquals("Rows returned", 1, table.Rows.Count);
			DataRow row = table.Rows[0];
			AssertEquals("Declaration PK", declaration.PK, row["DeclarationPK"]);
			AssertEquals("CusContainer PK", container1.PK, row["CusContainerPK"]);
			AssertEquals("Declaration Reference", declaration.JE_DeclarationReference, row["DeclarationReference"]);
			AssertEquals("Container Number", container1.CO_ContainerNumber, row["ContainerNum"]);
			AssertEquals("Container Type", containerType.RC_Code, new ZString(row["ContainerType"]).Trim());
			AssertEquals("Arrival Vessel", declaration.JE_VesselName, row["ArrivalVessel"]);
			AssertEquals("Arrival Voyage", declaration.JE_VoyageFlightNo, row["ArrivalVoyage"]);
			AssertEquals("Part Number", invoiceLine.JI_PartNo, row["Partno"]);
			AssertEquals("Description", invoiceLine.JI_Description, row["Description"]);
			AssertEquals("Buyer PK", importer.PK, row["BuyerPK"]);
			AssertEquals("Buyer Code", importer.OH_Code, row["BuyerCode"]);
			AssertEquals("Buyer FullName", importer.OH_FullName, row["BuyerFullName"]);
			AssertEquals("Delivery Address", delivery.J4_OA_NKDeliveryPoint, row["DeliveryAddress"]);
			AssertEquals("Container In Yard Date", jobContainer.JC_FCLWharfGateOut, row["ContainerInYardDate"]);
			AssertEquals("Total Packs In Cartons", (int)container2.J5_PackCount, row["TotalPacksInCartons"]);
			AssertEquals("Vendor Pack Qty", product.OP_VendorPackQty, new ZDecimal(row["VendorPackQty"]));
			AssertEquals("Invoice Currency Code", aUD.RX_Code, row["InvoiceCurrencyCode"]);
			AssertEquals("Invoice Total Line Price", invoiceLine.JI_LinePrice, new ZDecimal(row["InvoiceLinePrice"]));
			AssertEquals("Invoice Item Price", invoiceLine.JI_LinePrice / invoiceLine.JI_InvoiceQuantity, new ZDecimal(row["InvoiceItemPrice"]));
			AssertEquals("POM Number", delivery.J4_CustomAttribute1, row["POMNum"]);
			AssertEquals("Order Number", invoiceLine.JI_OrderNumber, row["OrderNumber"]);
		}

		#region Client-specific Indexes
		public void TestIndex_JobComInvoiceLine_JI_OrderNumber_JI_PartNo()
		{
			AssertContainsIndex(JobComInvoiceLineSchema.Constants.TableName, JobComInvoiceLineSchema.JI_OrderNumber.Name, JobComInvoiceLineSchema.JI_PartNo.Name);
		}

		void AssertContainsIndex(string tableName, params string[] indexColumns)
		{
			DbIndexReader indexReader = new DbIndexReader(tableName);
			AssertEquals("Expected to find the index", true, indexReader.IsIndexed(indexColumns));
		}
		#endregion
	}
}
