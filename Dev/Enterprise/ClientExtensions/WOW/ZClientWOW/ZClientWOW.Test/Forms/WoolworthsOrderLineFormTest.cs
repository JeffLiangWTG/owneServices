using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.Wow.Testing
{
	[TestedType(typeof(WoolworthsOrderLineForm))]
	public class WoolworthsOrderLineFormTest : ZFormBasherTest
	{
		[ExpectNoExceptions]
		public void TestLoadForm()
		{
			Order order = Factory.New<Order>();
			order.JD_OrderNumber = "ordernum";
			order.BuyerPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			order.SupplierPK = Factory.LoadTop1(typeof(OrgHeader), new ZQuery()).PK;
			OrderLine orderLine = order.OrderLines.AddNew();
			Factory.Save();
			ZController controller = ZControllerFactory.Create(ControllerIDs.OrderLine);
			controller.ShowEditForm(orderLine);
			Application.DoEvents();
			using (controller.LastShownForm)
			{
				AssertEquals("Correct form type created", typeof(WoolworthsOrderLineForm), controller.LastShownForm.GetType());
			}
		}

		public void TestEventDateColumnsAdded()
		{
			Order order = Factory.New<Order>();
			WoolworthsOrderLine orderLine = (WoolworthsOrderLine)order.OrderLines.AddNew();
			using (TestWoolworthsOrderLineForm form = new TestWoolworthsOrderLineForm(orderLine))
			{
				form.Show();
				Application.DoEvents();
				AssertColumnCreatedAndVisible(form.DeliveryContainersBoundGrid, WoolworthsOrderLineDeliverContainer.Schema.LastContainerFromWarfToDepot);
				AssertColumnCreatedAndVisible(form.DeliveryContainersBoundGrid, WoolworthsOrderLineDeliverContainer.Schema.LastContainerUnpack);
				AssertColumnCreatedAndVisible(form.DeliveryContainersBoundGrid, WoolworthsOrderLineDeliverContainer.Schema.LastDeliveredToWarehouse);
				AssertColumnCreatedAndVisible(form.DeliveryContainersBoundGrid, WoolworthsOrderLineDeliverContainer.Schema.LastDeliveredToWarehouseReference);
			}
		}

		public void TestPackCountBalloonTip()
		{
			Order order = Factory.New<Order>();
			WoolworthsOrderLine orderLine = (WoolworthsOrderLine)order.OrderLines.AddNew();
			using (TestWoolworthsOrderLineForm form = new TestWoolworthsOrderLineForm(orderLine))
			{
				form.Show();
				Application.DoEvents();
				ZCalcEditColumnStyleInfo column = (ZCalcEditColumnStyleInfo)form.DeliveryContainersBoundGrid.GetColumnStyle(OrderLineDeliverContainer.Schema.J5_PackCount);
				AssertEquals("The number of outer packages in the container for the current product / port split.", column.ToolTip);
			}
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			Order order = Factory.New<Order>();
			WoolworthsOrderLine orderLine = (WoolworthsOrderLine)order.OrderLines.AddNew();
			orderLine.HasChanges = false;
			return new WoolworthsOrderLineForm(orderLine)
			{ ControllerID = ControllerIDs.OrderLine };
		}

		protected void AssertColumnCreatedAndVisible(ZGrid grid, string columnName)
		{
			ZGridColumn column = grid.Columns[columnName];
			AssertNotNull("Column exists for " + columnName, column);
			AssertEquals("Column visible for " + columnName, true, column.IsVisible);
			AssertNotNull("Column has column style for " + columnName, column.ColumnStyle);
		}

		public class TestWoolworthsOrderLineForm : WoolworthsOrderLineForm
		{
			public TestWoolworthsOrderLineForm(WoolworthsOrderLine bO) : base(bO)
			{
			}

			public new ZGrid DeliveryContainersBoundGrid
			{
				get
				{
					return base.DeliveryContainersBoundGrid;
				}
			}
		}
		#endregion
	}
}
