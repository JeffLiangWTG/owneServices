namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class AdditionalInfoUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AdditionalInfoGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfoGrid)).BeginInit();
			this.AdditionalInfoGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// AdditionalInfoGrid
			// 
			this.AdditionalInfoGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalInfoGrid, "AdditionalInfos");
			this.AdditionalInfoGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "AdditionalInfoDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			this.AdditionalInfoGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalInfoGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalInfoGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfoGrid.GridId = "ba04f1c3-a234-48c4-9b9c-e343490ce69d";
			this.AdditionalInfoGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalInfoGrid.LayoutKey = "AdditionalInfoGrid";
			this.AdditionalInfoGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfoGrid.Name = "AdditionalInfoGrid";
			this.AdditionalInfoGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 150, true);
			this.AdditionalInfoGrid.TabIndex = 0;
			// 
			// AdditionalInfoUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalInfoGrid);
			this.Name = "AdditionalInfoUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 150, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfoGrid)).EndInit();
			this.AdditionalInfoGrid.ResumeLayout(false);
			this.AdditionalInfoGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid AdditionalInfoGrid;
	}
}
