using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.Wow.Testing
{
	public class WoolworthsJobComInvoiceLineValidationTest : TestCaseWithFactory
	{
		public void TestValidateInvoiceMatchesOrderDelivery_LinePrice()
		{
			fOrderLine.JO_LinePrice = 2;
			fInvoiceLine.JI_LinePrice = 4;
			AssertEquals("Has warning because the line price is not consistent", true, fInvoiceLine.JI_LinePriceInfo.HasWarnings());
			fOrderLine.JO_LinePrice = 3;
			fInvoiceLine.JI_LinePrice = 3;
			AssertEquals("No warning because the line price is now consistent", false, fInvoiceLine.JI_LinePriceInfo.HasWarnings());
		}

		public void TestValidateInvoiceMatchesOrderDelivery_InvoiceUQ()
		{
			fOrderLine.JO_F3_NKPackType = "TN";
			fInvoiceLine.JI_InvoiceUQ = "UNX";
			AssertEquals("Has warning because the UnitOfQuantity is not consistent", true, fInvoiceLine.JI_InvoiceUQInfo.HasWarnings());
			fOrderLine.JO_F3_NKPackType = "UNT";
			fInvoiceLine.JI_InvoiceUQ = "UNT";
			AssertEquals("No warning because the UnitOfQuantity is now consistent", false, fInvoiceLine.JI_InvoiceUQInfo.HasWarnings());
		}

		public void TestValidateInvoiceMatchesOrderDelivery_UnitPrice()
		{
			var validation = (JobComInvoiceLineValidation)fInvoiceLine.Validation;
			fInvoiceLine.JI_InvoiceUQ = "UNT";
			fOrderLine.JO_ItemPrice = 2;
			validation.ValidateUnitPrice();
			AssertEquals("Has warning because the UnitPrice is not consistent", true, fInvoiceLine.UnitPriceInfo.HasWarnings());
			fInvoiceLine.JI_LinePrice = 6;
			fInvoiceLine.JI_InvoiceQuantity = 3;
			AssertEquals("Unit price for test", 2m, fInvoiceLine.UnitPrice);
			validation.ValidateUnitPrice();
			AssertEquals("No warning because the UnitPrice is now consistent", false, fInvoiceLine.UnitPriceInfo.HasWarnings());
		}

		public void TestValidateInvoiceQuantityDividesVendorPack()
		{
			fInvoiceLine.JI_InvoiceQuantity = 10;
			fDeliverContainer.J5_QuantityInStore = 10;
			fOrderLine.JO_InnerPacks = 3;
			fInvoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertEquals("Has warning because the invoice quantity is not consistent", true, fInvoiceLine.JI_InvoiceQuantityInfo.HasWarnings());
			fOrderLine.JO_InnerPacks = 2;
			fInvoiceLine.Validation.ValidateJI_InvoiceQuantity();
			AssertEquals("No warning because the invoice quantity is now consistent", false, fInvoiceLine.JI_InvoiceQuantityInfo.HasWarnings());
		}

		protected WoolworthsJobDeclaration fDeclaration;
		protected WoolworthsJobComInvoiceHeader fInvoiceHeader;
		protected WoolworthsJobComInvoiceLine fInvoiceLine;
		protected CusContainer fCusContainer;
		protected WoolworthsOrder fOrder;
		protected OrderLine fOrderLine;
		protected WoolworthsOrderLineDelivery fDelivery;
		protected WoolworthsOrderLineDeliverContainer fDeliverContainer;
		protected override void SetUp()
		{
			base.SetUp();
			WowDataRegistry.Instance.EnableOrderNumberFountain = false;
			fDeclaration = (WoolworthsJobDeclaration)Factory.New(typeof(JobDeclaration));
			fCusContainer = fDeclaration.CusContainers.AddNew();
			fCusContainer.CO_ContainerNumber = "12345678";
			fInvoiceHeader = (WoolworthsJobComInvoiceHeader)fDeclaration.Invoices.AddNew();
			fInvoiceLine = (WoolworthsJobComInvoiceLine)fInvoiceHeader.JobComInvoiceLines.AddNew();
			fOrder = (WoolworthsOrder)Factory.New(typeof(Order));
			fOrder.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			fOrder.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			fOrderLine = fOrder.OrderLines.AddNew();
			fDelivery = (WoolworthsOrderLineDelivery)fOrderLine.Deliveries.AddNew();
			fDeliverContainer = (WoolworthsOrderLineDeliverContainer)fDelivery.Containers.AddNew();
			fDeliverContainer.J5_ContainerNum = "12345678";
			fOrderLine.JO_Quantity = 10;
			fOrderLine.JO_QtyInvoiced = 5;
			fDelivery.J4_Allocated = 10;
			fInvoiceLine.JI_PartNo = "tstprt";
			fOrderLine.JO_Partno = "tstprt";
			fInvoiceLine.JI_OrderNumber = "ordernum";
			fInvoiceLine.JI_CustomDecimal1 = new ZDecimal(fOrderLine.JO_LineNo);
			fOrder.JD_OrderNumber = "ordernum";
			fDelivery.J4_RL_NKDestinationPort = "AUMEL";
			fDeclaration.JE_RL_NKFinalDestination = "AUMEL";
		}
	}
}
