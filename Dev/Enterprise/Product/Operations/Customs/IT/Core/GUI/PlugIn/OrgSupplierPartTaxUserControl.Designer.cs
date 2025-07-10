namespace Enterprise.Customs.IT.GUI
{
	partial class OrgSupplierPartTaxUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			components = new System.ComponentModel.Container();
			this.ZG_PortTaxRatesDropEdit = new ZArchitecture.GUI.ZDropEdit();
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			// 
			// TaxGroupBox
			// 
			this.TaxGroupBox.Controls.Add(this.ZG_PortTaxRatesDropEdit);
			// 
			// ZG_PortTaxRatesDropEdit
			// 
			this.ZG_PortTaxRatesDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ZG_PortTaxRatesDropEdit, "FilteredInvoiceLines.Taxes.Data.G4_PortTaxRate");
			this.ZG_PortTaxRatesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(140, 71, true);
			this.ZG_PortTaxRatesDropEdit.Name = "ZG_PortTaxRatesDropEdit";
			this.ZG_PortTaxRatesDropEdit.PreBoundMaxLength = 2;
			this.ZG_PortTaxRatesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(206, 20, true);
			this.ZG_PortTaxRatesDropEdit.TabIndex = 6;
		}
		private ZArchitecture.GUI.ZDropEdit ZG_PortTaxRatesDropEdit;

		#endregion
	}
}
