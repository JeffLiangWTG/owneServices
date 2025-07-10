namespace Enterprise.Customs.EU.GUI
{
	partial class CustomsBrokerageUserControl
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
				MainTabControl.SelectedIndexChanged -= MainTabControl_SelectedIndexChanged;
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.MainTabControl.SuspendLayout();
			this.WorkflowTabPage.SuspendLayout();
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
			this.MainTabControl.Controls.SetChildIndex(this.DeliveryTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.PickupTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.PackingTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.ContainerTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.EntryInstructionDetailsTabPage, 0);
			this.MainTabControl.Controls.SetChildIndex(this.DeclarationTabPage, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// CustomsBrokerageUserControl
			// 
			this.Name = "CustomsBrokerageUserControl";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			this.WorkflowTabPage.ResumeLayout(false);
			this.WorkflowTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
