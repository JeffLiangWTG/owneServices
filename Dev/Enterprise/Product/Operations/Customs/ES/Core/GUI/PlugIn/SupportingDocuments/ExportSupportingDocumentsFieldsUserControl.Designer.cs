namespace Enterprise.Customs.ES.GUI
{
	public partial class ExportSupportingDocumentsFieldsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SupportingDocumentsFieldsDynamicLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SupportingDocumentsGroupBox.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// SupportingDocumentsGroupBox
			// 
			this.SupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.ES.GUI.Res.GetData("BF424BD6-0591-4CB2-BE9A-91208D2578BA", "[44] Supporting Documents");
			this.SupportingDocumentsGroupBox.Controls.Add(this.SupportingDocumentsFieldsDynamicLayoutPanel);
			this.SupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SupportingDocumentsGroupBox.Name = "SupportingDocumentsGroupBox";
			this.SupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 185, true);
			this.SupportingDocumentsGroupBox.TabIndex = 1;
			this.SupportingDocumentsGroupBox.TabStop = false;
			// 
			// SupportingDocumentsFieldsDynamicLayoutPanel
			//
			this.SupportingDocumentsFieldsDynamicLayoutPanel.AllowDrop = true;
			this.SupportingDocumentsFieldsDynamicLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SupportingDocumentsFieldsDynamicLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.SupportingDocumentsFieldsDynamicLayoutPanel.Name = "SupportingDocumentsFieldsDynamicLayoutPanel";
			this.SupportingDocumentsFieldsDynamicLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(601, 166, true);
			this.SupportingDocumentsFieldsDynamicLayoutPanel.TabIndex = 0;
			// 
			// ExportSupportingDocumentsFieldsUserControl
			// 
			this.Controls.Add(this.SupportingDocumentsGroupBox);
			this.Name = "ExportSupportingDocumentsFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 185, true);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.Controls.SetChildIndex(this.SupportingDocumentsGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SupportingDocumentsGroupBox.ResumeLayout(false);
			this.SupportingDocumentsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox SupportingDocumentsGroupBox;
		internal Enterprise.ZArchitecture.GUI.DynamicLayoutPanel SupportingDocumentsFieldsDynamicLayoutPanel;
	}
}
