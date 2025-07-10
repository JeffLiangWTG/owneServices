using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;

namespace Enterprise.Client.Wow
{
	public abstract class InvoiceOrderLinkTestCase : EnterpriseBusinessObjectTestCase
	{
		protected WoolworthsJobDeclaration fDeclaration;
		protected WoolworthsJobComInvoiceHeader fInvoiceHeader;
		protected WoolworthsJobComInvoiceLine fInvoiceLine;
		protected WoolworthsCusContainer fCusContainer;
		protected WoolworthsOrder fOrder;
		protected OrderLine fOrderLine;
		protected WoolworthsOrderLineDelivery fDelivery;
		protected WoolworthsOrderLineDeliverContainer fDeliverContainer;
		protected override void SetUp()
		{
			base.SetUp();
			WowDataRegistry.Instance.EnableOrderNumberFountain = false;
			fDeclaration = (WoolworthsJobDeclaration)Factory.New(typeof(JobDeclaration));
			fDeclaration.JE_TransportMode = fDeclaration.TransportModeSeaCodeForTesting;
			fCusContainer = (WoolworthsCusContainer)fDeclaration.CusContainers.AddNew();
			fCusContainer.CO_ContainerNumber = "12345678";
			fInvoiceHeader = (WoolworthsJobComInvoiceHeader)fDeclaration.Invoices.AddNew();
			fInvoiceLine = (WoolworthsJobComInvoiceLine)fInvoiceHeader.JobComInvoiceLines.AddNew();
			fOrder = (WoolworthsOrder)Factory.New(typeof(Order));
			fOrder.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			fOrder.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			fOrderLine = fOrder.OrderLines.AddNew();
			fOrderLine.JO_Quantity = 10;
			fDelivery = (WoolworthsOrderLineDelivery)fOrderLine.Deliveries.AddNew();
			fDeliverContainer = (WoolworthsOrderLineDeliverContainer)fDelivery.Containers.AddNew();
			fDeliverContainer.J5_ContainerNum = "12345678";
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
