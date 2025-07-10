namespace Enterprise.Customs.CN.GUI
{
	partial class CusSupportingDocumentsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CusSupportingDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CusSupportingDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CusSupportingDocumentsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusSupportingDocumentsGrid)).BeginInit();
			this.CusSupportingDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CusEntryInstruction);
			// 
			// CusSupportingDocumentsGroupBox
			// 
			this.CusSupportingDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("F690FA70-6A83-4F25-A0CF-6BF52F4B2D9B", "Supporting Documents");
			this.CusSupportingDocumentsGroupBox.Controls.Add(this.CusSupportingDocumentsGrid);
			this.CusSupportingDocumentsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusSupportingDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CusSupportingDocumentsGroupBox.Name = "CusSupportingDocumentsGroupBox";
			this.CusSupportingDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 403, true);
			this.CusSupportingDocumentsGroupBox.TabIndex = 0;
			this.CusSupportingDocumentsGroupBox.TabStop = false;
			// 
			// CusSupportingDocumentsGrid
			// 
			this.CusSupportingDocumentsGrid.AllowDrop = true;
			this.CusSupportingDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CusSupportingDocumentsGrid, "FilteredCusSupportingDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.CusSupportingDocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "DocumentType";
			zDropEditColumnStyleInfo1.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			this.CusSupportingDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CusSupportingDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CusSupportingDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CusSupportingDocumentsGrid.GridId = "01625b43-a29e-40d0-a41b-653c6e12fea3";
			this.CusSupportingDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CusSupportingDocumentsGrid.LayoutKey = "CusSupportingDocumentsGrid";
			this.CusSupportingDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CusSupportingDocumentsGrid.Name = "CusSupportingDocumentsGrid";
			this.CusSupportingDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(405, 384, true);
			this.CusSupportingDocumentsGrid.TabIndex = 1;
			// 
			// CusSupportingDocumentsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.CusSupportingDocumentsGroupBox);
			this.Name = "CusSupportingDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(411, 403, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CusSupportingDocumentsGroupBox.ResumeLayout(false);
			this.CusSupportingDocumentsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CusSupportingDocumentsGrid)).EndInit();
			this.CusSupportingDocumentsGrid.ResumeLayout(false);
			this.CusSupportingDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid CusSupportingDocumentsGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox CusSupportingDocumentsGroupBox;
	}
}
