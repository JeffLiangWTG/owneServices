using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class SupplementaryDeclarantUserControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.supplementaryDeclarantGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.supplementaryDeclarantGrid)).BeginInit();
			this.supplementaryDeclarantGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill);
			// 
			// supplementaryDeclarantGrid
			// 
			this.supplementaryDeclarantGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.supplementaryDeclarantGrid, "SupplementaryDeclarants");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).SupplementaryDeclarants)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.SupplementaryDeclarant)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).SupplementaryDeclarants)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.ICS2.Business.SupplementaryDeclarant)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill)(null)).SupplementaryDeclarants)).SyncRoot)).CY_Code)));
			this.supplementaryDeclarantGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.ColumnName = "CY_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.supplementaryDeclarantGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.supplementaryDeclarantGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.supplementaryDeclarantGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.supplementaryDeclarantGrid.GridId = "e2738366-bc1d-4163-95e6-2955cbfb0019";
			this.supplementaryDeclarantGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.supplementaryDeclarantGrid.LayoutKey = "SupplementaryDeclarantGrid";
			this.supplementaryDeclarantGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.supplementaryDeclarantGrid.Name = "supplementaryDeclarantGrid";
			this.supplementaryDeclarantGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 343, true);
			this.supplementaryDeclarantGrid.TabIndex = 0;
			// 
			// SupplementaryDeclarantUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.supplementaryDeclarantGrid);
			this.Name = "SupplementaryDeclarantUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 343, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.supplementaryDeclarantGrid)).EndInit();
			this.supplementaryDeclarantGrid.ResumeLayout(false);
			this.supplementaryDeclarantGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		ZGrid supplementaryDeclarantGrid;

		#endregion
	}
}
