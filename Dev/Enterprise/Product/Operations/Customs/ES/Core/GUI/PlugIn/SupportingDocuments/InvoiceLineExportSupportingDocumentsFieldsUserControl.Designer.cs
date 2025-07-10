namespace Enterprise.Customs.ES.GUI
{
	public partial class InvoiceLineExportSupportingDocumentsFieldsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InvoiceLineSupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.InvoiceLineSupportingDocumentsGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// InvoiceLineSupportingDocumentsGroupBox
			// 
			this.InvoiceLineSupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("BF424BD6-0591-4CB2-BE9A-91208D2578BA", "[44] Supporting Documents");
			this.InvoiceLineSupportingDocumentsGroupBox.Controls.Add(this.InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel);
			this.InvoiceLineSupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineSupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.InvoiceLineSupportingDocumentsGroupBox.Name = "InvoiceLineSupportingDocumentsGroupBox";
			this.InvoiceLineSupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 185, true);
			this.InvoiceLineSupportingDocumentsGroupBox.TabIndex = 1;
			this.InvoiceLineSupportingDocumentsGroupBox.TabStop = false;
			// 
			// InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel
			//
			this.InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel.AllowDrop = true;
			this.InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel.Name = "InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel";
			this.InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 166, true);
			this.InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel.TabIndex = 0;
			// 
			// InvoiceLineExportSupportingDocumentsFieldsUserControl
			// 
			this.Controls.Add(this.InvoiceLineSupportingDocumentsGroupBox);
			this.Name = "InvoiceLineExportSupportingDocumentsFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 185, true);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.Controls.SetChildIndex(this.InvoiceLineSupportingDocumentsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.InvoiceLineSupportingDocumentsGroupBox.ResumeLayout(false);
			this.InvoiceLineSupportingDocumentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox InvoiceLineSupportingDocumentsGroupBox;
		internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel InvoiceLineSupportingDocumentsFieldsDynamicLayoutPanel;
	}
}
