using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrderLineDelivery))]
	public class WoolworthsOrderLineDeliveryTest : InvoiceOrderLinkTestCase
	{
		public void TestHasJobComInvoiceLineLink_WhenDeclarationInactive()
		{
			TestWoolworthsOrderLineDelivery orderLineDelivery = Factory.Load<TestWoolworthsOrderLineDelivery>(fDelivery.PK);
			AssertEquals("Should have a link to the invoice line initially", true, orderLineDelivery.HasJobComInvoiceLineLink);
			fDeclaration.JE_IsCancelled = true;
			AssertEquals("Should not a link now that the declaration has been made inactive", false, orderLineDelivery.HasJobComInvoiceLineLink);
		}

		public void TestHasJobComInvoiceLineLink_NonAUDeclaration()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.NewZealand))
			{
				var order = Factory.New<WoolworthsOrder>();
				order.JD_OrderNumber = "TestOrder";

				var orderLine = order.OrderLines.AddNew();
				orderLine.JO_JD = order.PK;
				orderLine.JO_Partno = "PENCIL";
				orderLine.JO_LineNo = 1;
				orderLine.JO_Quantity = 10;

				var invoiceNZ = Factory.New<Customs.Business.BaseJobComInvoiceHeader>();
				var invoiceLineNZ = invoiceNZ.InvoiceLines.AddNew();
				invoiceLineNZ.JI_OrderNumber = "TestOrder";
				invoiceLineNZ.JI_PartNo = "PENCIL";
				invoiceLineNZ.JI_CustomDecimal1 = 1;

				var decNZ = Factory.New<Customs.Business.BaseJobDeclaration>();
				var cusContainer = decNZ.CusContainers.AddNew();
				cusContainer.CO_ContainerNumber = "12345678";
				var orderLineDelivery = orderLine.Deliveries.AddNew();
				orderLineDelivery.J4_Allocated = 10;
				var orderLineDeliveryCont = orderLineDelivery.Containers.AddNew();
				orderLineDeliveryCont.J5_ContainerNum = "12345678";
				orderLineDelivery.J4_RL_NKDestinationPort = "NZAKL";
				decNZ.JE_RL_NKFinalDestination = "NZAKL";
				invoiceNZ.JZ_JE = decNZ.PK;

				TestWoolworthsOrderLineDelivery wowOrderLineDelivery = Factory.Load<TestWoolworthsOrderLineDelivery>(orderLineDelivery.PK);

				Assert("HasJobComInvoiceLineLink should check for AU declaration only", !wowOrderLineDelivery.HasJobComInvoiceLineLink);
			}
		}

		public void TestPOMNumUpperCase()
		{
			ICustomLabelsProvider provider = OrderLineDelivery.NewCustomLabelsProvider(fOrder);
			CustomLabelInfoList customLabels = provider.GetCustomFields(Factory.New<OrgHeader>(), Factory);
			CustomLabelInfoBase customAttribute1Label = customLabels.GetFieldByPropertyName(OrderLineDelivery.Schema.J4_CustomAttribute1);
			AssertEquals("CustomAttribute1 is UpperCase", true, (customAttribute1Label.Styles & CustomLabelStyles.UpperCase) != 0);
		}

		[ExpectNoExceptions]
		public void TestClone_DoesntThrowExceptions()
		{
			WoolworthsOrder order = Factory.New<WoolworthsOrder>();
			OrderLine orderLine = order.OrderLines.AddNew();
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			foreach (ZPropertyInfo property in delivery.ZPropertyInfoHash)
			{
				if (property.Value is INumericZType && property.HasSetter)
				{
					property.Value = new ZDecimal(2m);
				}
			}

			WoolworthsOrder clonedOrder = (WoolworthsOrder)order.Clone();
		}

		#region Test Classes
		class TestWoolworthsOrderLineDelivery : WoolworthsOrderLineDelivery
		{
			public TestWoolworthsOrderLineDelivery(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new bool HasJobComInvoiceLineLink
			{
				get
				{
					return base.HasJobComInvoiceLineLink;
				}
			}
		}

		#endregion
		#region Implementation
		protected override BusinessObject GetNewBusinessObject()
		{
			Order order = Factory.New<Order>();
			OrderLine orderLine = order.OrderLines.AddNew();
			OrderLineDelivery delivery = orderLine.Deliveries.AddNew();
			return delivery;
		}
		#endregion
	}
}
