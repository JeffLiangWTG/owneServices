using System;
using System.Collections;
using System.ComponentModel;
using CargoWise.Windows.UI;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Orders.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.Wow
{
	public partial class WoolworthsOrdersUserControl : OrdersUserControl
	{
		protected Enterprise.ZArchitecture.GUI.ZDropEdit JD_PaymentTypeBoundDropEdit;

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.JD_PaymentTypeBoundDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OrderDetailsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrderLinesBoundButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.OrderSplitsButtonGrid.InnerGrid)).BeginInit();
			this.RightTabControl.SuspendLayout();
			this.OrdersTab.SuspendLayout();
			this.BottomTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ConsolsBoundButtonGrid.InnerGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PlannedContainersGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OrderDetailsGroupBox
			// 
			this.OrderDetailsGroupBox.Controls.Add(this.JD_PaymentTypeBoundDropEdit);
			this.OrderDetailsGroupBox.Controls.SetChildIndex(this.JD_RXBoundCurrency, 0);
			this.OrderDetailsGroupBox.Controls.SetChildIndex(this.zCodeFindBox1, 0);
			this.OrderDetailsGroupBox.Controls.SetChildIndex(this.IncoTermsExplainButton, 0);
			this.OrderDetailsGroupBox.Controls.SetChildIndex(this.JD_PaymentTypeBoundDropEdit, 0);
			this.OrderDetailsGroupBox.Controls.SetChildIndex(this.JD_RSLabel, 0);
			this.OrderDetailsGroupBox.Controls.SetChildIndex(this.JD_RSBoundFindBox, 0);
			// 
			// OrderLinesBoundButtonGrid
			// 
			// 
			// 
			// 
			this.OrderLinesBoundButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrderLinesBoundButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OrderLinesBoundButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrderLinesBoundButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrderLinesBoundButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrderLinesBoundButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.OrderLinesBoundButtonGrid.InnerGrid.Name = "Grid";
			this.OrderLinesBoundButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(976, 171, true);
			this.OrderLinesBoundButtonGrid.InnerGrid.TabIndex = 0;
			// 
			// OrderSplitsButtonGrid
			// 
			// 
			// 
			// 
			this.OrderSplitsButtonGrid.InnerGrid.AllowNavigation = false;
			this.OrderSplitsButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.OrderSplitsButtonGrid.InnerGrid.CaptionVisible = false;
			this.OrderSplitsButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrderSplitsButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.OrderSplitsButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.OrderSplitsButtonGrid.InnerGrid.Name = "Grid";
			this.OrderSplitsButtonGrid.InnerGrid.ReadOnly = true;
			this.OrderSplitsButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(358, 287, true);
			this.OrderSplitsButtonGrid.InnerGrid.TabIndex = 0;
			// 
			// ConsolsBoundButtonGrid
			// 
			// 
			// 
			// 
			this.ConsolsBoundButtonGrid.InnerGrid.AllowNavigation = false;
			this.ConsolsBoundButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.ConsolsBoundButtonGrid.InnerGrid.CaptionVisible = false;
			this.ConsolsBoundButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ConsolsBoundButtonGrid.InnerGrid.LayoutKey = "Grid";
			this.ConsolsBoundButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
			this.ConsolsBoundButtonGrid.InnerGrid.Name = "Grid";
			this.ConsolsBoundButtonGrid.InnerGrid.ReadOnly = true;
			this.ConsolsBoundButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(685, 144, true);
			this.ConsolsBoundButtonGrid.InnerGrid.TabIndex = 0;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.Wow.WoolworthsOrder);
			// 
			// JD_PaymentTypeBoundDropEdit
			// 
			this.BindingSource.SetBindingMember(this.JD_PaymentTypeBoundDropEdit, "JD_PaymentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.Wow.WoolworthsOrder)(null)).JD_PaymentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.Wow.WoolworthsOrder)(null)).JD_PaymentType_List)));
			this.JD_PaymentTypeBoundDropEdit.BindToList = "JD_PaymentType_List";
			this.JD_PaymentTypeBoundDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 172, true);
			this.JD_PaymentTypeBoundDropEdit.Name = "JD_PaymentTypeBoundDropEdit";
			this.JD_PaymentTypeBoundDropEdit.ShowDescriptionBox = false;
			this.JD_PaymentTypeBoundDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.JD_PaymentTypeBoundDropEdit.TabIndex = 70;
			// 
			// WoolworthsOrdersUserControl
			// 
			this.Name = "WoolworthsOrdersUserControl";
			this.OrderDetailsGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.OrderLinesBoundButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.OrderSplitsButtonGrid.InnerGrid)).EndInit();
			this.RightTabControl.ResumeLayout(false);
			this.OrdersTab.ResumeLayout(false);
			this.BottomTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.ConsolsBoundButtonGrid.InnerGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PlannedContainersGrid)).EndInit();
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
