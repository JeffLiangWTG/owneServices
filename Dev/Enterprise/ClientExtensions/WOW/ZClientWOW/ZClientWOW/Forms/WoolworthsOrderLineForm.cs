using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.Wow
{
	public partial class WoolworthsOrderLineForm : OrderLineForm
	{
		public WoolworthsOrderLineForm(WoolworthsOrderLine orderLine) : base(orderLine)
		{
			InitializeComponent();
			allControlsCreated = true;
			SetDataBinding(orderLine, "");

			if (orderLine.Order != null)
			{
				((WoolworthsOrder)orderLine.Order).InvoiceOrderMismatching += new CancelEventHandler(OnOrderInvoiceMismatching);
			}
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);
			if (DataSource != null && !initializedColumns && allControlsCreated)
			{
				ZDateEditColumnStyleInfo column;
				column = new ZDateEditColumnStyleInfo(WoolworthsOrderLineDeliverContainer.Schema.LastContainerFromWarfToDepot, 100);
				column.Caption = "Wharf To Depot";
				TypeDescriptor.AddAttributes(column, new SuppressFormsLocalizedTestAttribute());
				this.DeliveryContainersBoundGrid.ColumnStyles.Add(column);
				this.DeliveryContainersBoundGrid.Columns.Add(column);
				column = new ZDateEditColumnStyleInfo(WoolworthsOrderLineDeliverContainer.Schema.LastContainerUnpack, 100);
				column.Caption = "Container Unpack";

				TypeDescriptor.AddAttributes(column, new SuppressFormsLocalizedTestAttribute());

				this.DeliveryContainersBoundGrid.ColumnStyles.Add(column);
				this.DeliveryContainersBoundGrid.Columns.Add(column);
				column = new ZDateEditColumnStyleInfo(WoolworthsOrderLineDeliverContainer.Schema.LastDeliveredToWarehouse, 100);
				column.Caption = "Delivered To Warehouse";

				TypeDescriptor.AddAttributes(column, new SuppressFormsLocalizedTestAttribute());

				this.DeliveryContainersBoundGrid.ColumnStyles.Add(column);
				this.DeliveryContainersBoundGrid.Columns.Add(column);
				ZTextBoxColumnStyleInfo textColumn = new ZTextBoxColumnStyleInfo(WoolworthsOrderLineDeliverContainer.Schema.LastDeliveredToWarehouseReference, 100);
				textColumn.Caption = "Delivery Method";

				TypeDescriptor.AddAttributes(textColumn, new SuppressFormsLocalizedTestAttribute());

				this.DeliveryContainersBoundGrid.ColumnStyles.Add(textColumn);
				this.DeliveryContainersBoundGrid.Columns.Add(textColumn);

				ZCalcEditColumnStyleInfo containerPackCountColumn = (ZCalcEditColumnStyleInfo)DeliveryContainersBoundGrid.GetColumnStyle(OrderLineDeliverContainer.Schema.J5_PackCount);
				containerPackCountColumn.ToolTip = "The number of outer packages in the container for the current product / port split.";

				this.DeliveryContainersBoundGrid.RefreshTableStyles();
				initializedColumns = true;
			}
		}
		bool initializedColumns;
		readonly bool allControlsCreated;

		#region Implementation

		protected void OnOrderInvoiceMismatching(object sender, CancelEventArgs e)
		{
			DialogResult result = Globals.Message.Show("You cannot modify this field as it will break the link between the order line delivery and the invoice line.",
				"",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			//			if (Result == DialogResult.No)
			//			{
			e.Cancel = true;
			//			}
		}

		#endregion
	}
}
