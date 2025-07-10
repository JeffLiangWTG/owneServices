namespace Enterprise.Accounting.GUI.DataExportBatch
{
	partial class DataExportBatchUserControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.DataExportBatchGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.DataExportBatchGrid)).BeginInit();
			this.DataExportBatchGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.DataExportBatch.IDataExportBatchSource);
			// 
			// DataExportBatchGrid
			// 
			this.DataExportBatchGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.DataExportBatchGrid, "DataExportBatchCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.DataExportBatch.IDataExportBatchSource)(null)).DataExportBatchCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenExportBatchSequence)(((System.Collections.IList)(((Enterprise.Accounting.Business.DataExportBatch.IDataExportBatchSource)(null)).DataExportBatchCollection)).SyncRoot)).Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenExportBatchSequence)(((System.Collections.IList)(((Enterprise.Accounting.Business.DataExportBatch.IDataExportBatchSource)(null)).DataExportBatchCollection)).SyncRoot)).TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenExportBatchSequence)(((System.Collections.IList)(((Enterprise.Accounting.Business.DataExportBatch.IDataExportBatchSource)(null)).DataExportBatchCollection)).SyncRoot)).SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenExportBatchSequence)(((System.Collections.IList)(((Enterprise.Accounting.Business.DataExportBatch.IDataExportBatchSource)(null)).DataExportBatchCollection)).SyncRoot)).SubTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.GenExportBatchSequence)(((System.Collections.IList)(((Enterprise.Accounting.Business.DataExportBatch.IDataExportBatchSource)(null)).DataExportBatchCollection)).SyncRoot)).BatchNumber)));
			this.DataExportBatchGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f6ff09f5-776a-4dce-84e6-8c9214fb6e17", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "Type";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("70f696f7-eb9e-4919-90ee-4735590ac2ac", "Type Description");
			zTextBoxColumnStyleInfo2.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a57e1057-83c8-4f2c-8abd-c678ed55411c", "Sub Type");
			zTextBoxColumnStyleInfo3.ColumnName = "SubType";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("2940a22c-c1b3-470d-bc8a-97c3e28f9dc7", "Sub Type Description");
			zTextBoxColumnStyleInfo4.ColumnName = "SubTypeDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("80bfd921-e580-4ad1-9028-c9f88a492421", "Batch Number");
			zCalcEditColumnStyleInfo1.ColumnName = "BatchNumber";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.DataExportBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.DataExportBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.DataExportBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.DataExportBatchGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.DataExportBatchGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.DataExportBatchGrid.CopySelectedRowsAllowed = true;
			this.DataExportBatchGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DataExportBatchGrid.GridId = "54835967-146f-46b8-b2cb-334303495c09";
			this.DataExportBatchGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.DataExportBatchGrid.LayoutKey = "DataExportBatchGrid";
			this.DataExportBatchGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DataExportBatchGrid.Name = "DataExportBatchGrid";
			this.DataExportBatchGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.DataExportBatchGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(643, 545, true);
			this.DataExportBatchGrid.TabIndex = 0;
			// 
			// DataExportBatchUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DataExportBatchGrid);
			this.Name = "DataExportBatchUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(643, 545, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.DataExportBatchGrid)).EndInit();
			this.DataExportBatchGrid.ResumeLayout(false);
			this.DataExportBatchGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid DataExportBatchGrid;
	}
}
