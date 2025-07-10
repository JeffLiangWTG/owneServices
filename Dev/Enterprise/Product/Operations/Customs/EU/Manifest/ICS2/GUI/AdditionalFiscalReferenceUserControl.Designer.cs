using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.Manifest.ICS2.GUI
{
	partial class AdditionalFiscalReferenceUserControl
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
			this.AdditionalFiscalReferenceGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalFiscalReferenceGrid)).BeginInit();
			this.AdditionalFiscalReferenceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.ICS2.Business.AsycudaBill);
			// 
			// AdditionalFiscalReferenceGrid
			// 
			this.AdditionalFiscalReferenceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalFiscalReferenceGrid, "AdditionalFiscalReferences");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.AdditionalFiscalReferenceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "CFR_Reference";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDropEditColumnStyleInfo1.ColumnName = "CFR_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.AdditionalFiscalReferenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalFiscalReferenceGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalFiscalReferenceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalFiscalReferenceGrid.GridId = "2C71B410-D041-4EE6-9787-287F00EFC54E";
			this.AdditionalFiscalReferenceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalFiscalReferenceGrid.LayoutKey = "AdditionalFiscalReferenceGrid";
			this.AdditionalFiscalReferenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalFiscalReferenceGrid.Name = "AdditionalFiscalReferenceGrid";
			this.AdditionalFiscalReferenceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 343, true);
			this.AdditionalFiscalReferenceGrid.TabIndex = 0;
			// 
			// AdditionalFiscalReferenceUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalFiscalReferenceGrid);
			this.Name = "AdditionalFiscalReferenceUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 343, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalFiscalReferenceGrid)).EndInit();
			this.AdditionalFiscalReferenceGrid.ResumeLayout(false);
			this.AdditionalFiscalReferenceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		ZGrid AdditionalFiscalReferenceGrid;

		#endregion
	}
}
