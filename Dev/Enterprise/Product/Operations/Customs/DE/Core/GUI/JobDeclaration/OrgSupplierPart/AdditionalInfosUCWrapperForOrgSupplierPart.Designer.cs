using Enterprise.Customs.EU.GUI.PlugIn;

namespace Enterprise.Customs.DE.GUI
{
	partial class AdditionalInfosUCWrapperForOrgSupplierPart
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ExportAdditionalInfosUserControl = new Enterprise.Customs.EU.GUI.PlugIn.AdditionalInfosUserControlWithGrid();
			this.ImportAdditionalInfosUserControl = new Enterprise.Customs.EU.GUI.PlugIn.AdditionalInfosUserControl();
			this.AdditionalInfosGroupBox.SuspendLayout();
			this.AdditionalInfosPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).BeginInit();
			this.AdditionalInfosGrid.SuspendLayout();
			this.AddInfoTypeCodeDropEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ExportAdditionalInfosUserControl.SuspendLayout();
			this.ImportAdditionalInfosUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// ExportAdditionalInfosUserControl
			// 
			this.ExportAdditionalInfosUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ExportAdditionalInfosUserControl, ".");
			this.ExportAdditionalInfosUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ExportAdditionalInfosUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ExportAdditionalInfosUserControl.Name = "ExportAdditionalInfosUserControl";
			this.ExportAdditionalInfosUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 274, true);
			this.ExportAdditionalInfosUserControl.TabIndex = 7;
			// 
			// ImportAdditionalInfosUserControl
			// 
			this.ImportAdditionalInfosUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportAdditionalInfosUserControl, ".");
			this.ImportAdditionalInfosUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ImportAdditionalInfosUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ImportAdditionalInfosUserControl.Name = "ImportAdditionalInfosUserControl";
			this.ImportAdditionalInfosUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 274, true);
			this.ImportAdditionalInfosUserControl.TabIndex = 8;
			// 
			// AdditionalInfosUCWrapperForOrgSupplierPart
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ExportAdditionalInfosUserControl);
			this.Controls.Add(this.ImportAdditionalInfosUserControl);
			this.Name = "AdditionalInfosUCWrapperForOrgSupplierPart";
			this.Controls.SetChildIndex(this.ImportAdditionalInfosUserControl, 0);
			this.Controls.SetChildIndex(this.ExportAdditionalInfosUserControl, 0);
			this.Controls.SetChildIndex(this.AdditionalInfosPanel, 0);
			this.Controls.SetChildIndex(this.AdditionalInfosGrid, 0);
			this.Controls.SetChildIndex(this.RequiresMergeLabel, 0);
			this.AdditionalInfosGroupBox.ResumeLayout(false);
			this.AdditionalInfosGroupBox.PerformLayout();
			this.AdditionalInfosPanel.ResumeLayout(false);
			this.AdditionalInfosPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).EndInit();
			this.AdditionalInfosGrid.ResumeLayout(false);
			this.AdditionalInfosGrid.PerformLayout();
			this.AddInfoTypeCodeDropEdit.ResumeLayout(true);
			this.AddInfoTypeCodeDropEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ExportAdditionalInfosUserControl.ResumeLayout(true);
			this.ExportAdditionalInfosUserControl.PerformLayout();
			this.ImportAdditionalInfosUserControl.ResumeLayout(true);
			this.ImportAdditionalInfosUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal AdditionalInfosUserControlWithGrid ExportAdditionalInfosUserControl;
		internal EU.GUI.PlugIn.AdditionalInfosUserControl ImportAdditionalInfosUserControl;
	}
}
