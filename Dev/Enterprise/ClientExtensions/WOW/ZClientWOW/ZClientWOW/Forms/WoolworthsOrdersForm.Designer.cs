using System.ComponentModel;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.Wow
{
	public partial class WoolworthsOrdersForm : OrdersForm
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OrdersUserControl
			// 
			this.OrdersUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1000, 560, true);
			// 
			// WoolworthsOrdersForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1008, 666, true);
			this.Name = "WoolworthsOrdersForm";
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		private System.ComponentModel.Container components = null;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}
	}
}
