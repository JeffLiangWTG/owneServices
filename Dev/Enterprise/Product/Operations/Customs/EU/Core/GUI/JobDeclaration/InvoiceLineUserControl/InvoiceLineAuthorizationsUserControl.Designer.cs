namespace Enterprise.Customs.EU.GUI
{
	partial class InvoiceLineAuthorisationsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

			this.BindingSource.SetBindingMember(this.AuthorisationsGrid, "FilteredInvoiceLines.CusAuthorizationUsages");
			this.AuthorisationsGrid.Dock = System.Windows.Forms.DockStyle.None;
			this.AuthorisationsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(900, 320, true);
		}

		#endregion
	}
}
