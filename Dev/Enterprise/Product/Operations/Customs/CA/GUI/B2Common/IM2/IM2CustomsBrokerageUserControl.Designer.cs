using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.CA.GUI
{
	partial class IM2CustomsBrokerageUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.SetChildIndex(this.EventTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.MiscOptionsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoiceLinesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoicesTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.InvoiceGroupingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.PackingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ContainerTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.DeclarationTabPage, 0);
			// 
			// DeclarationTabPage
			// 
			this.DeclarationTabPage.AutoScroll = true;
			this.DeclarationTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1195, 610, true);
			this.DeclarationTabPage.Text = "B2";
			// 
			// IM2CustomsBrokerageUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "IM2CustomsBrokerageUserControl";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}