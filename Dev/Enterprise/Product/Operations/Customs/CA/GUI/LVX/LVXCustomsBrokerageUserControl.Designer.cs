namespace Enterprise.Customs.CA.GUI
{
	partial class LVXCustomsBrokerageUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

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
			this.MainTabControl.Controls.SetChildIndex(this.MessagesTabPage, 0);
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
			this.DeclarationTabPage.AutoScrollMinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1195, 631, true);
			this.DeclarationTabPage.Text = "Courier LVS Declaration";
			// 
			// LVXCustomsBrokerageUserControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "LVXCustomsBrokerageUserControl";
			this.MainTabControl.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion
	}
}
