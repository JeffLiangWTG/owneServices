namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5UnloadingDifferencesPreviousDocumentsGridUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.UnloadingDifferencesPreviousDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.UnloadingDifferencesPreviousDocumentsGrid)).BeginInit();
			this.UnloadingDifferencesPreviousDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocumentCollection<Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument>);
			// 
			// UnloadingDifferencesPreviousDocumentsGrid
			// 
			this.UnloadingDifferencesPreviousDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UnloadingDifferencesPreviousDocumentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument)(null)).CSI_ItemNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument)(null)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument)(null)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument)(null)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.CommonPreviousDocument)(null)).CSI_ReferenceNumber2)));
			this.UnloadingDifferencesPreviousDocumentsGrid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "CSI_ItemNumber";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Status";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(95);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_Code";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo4.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			this.UnloadingDifferencesPreviousDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.UnloadingDifferencesPreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UnloadingDifferencesPreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.UnloadingDifferencesPreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.UnloadingDifferencesPreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.UnloadingDifferencesPreviousDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnloadingDifferencesPreviousDocumentsGrid.GridId = "136bd171-b544-49ae-9afe-ae2b2af43dfe";
			this.UnloadingDifferencesPreviousDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnloadingDifferencesPreviousDocumentsGrid.LayoutKey = "zGrid1";
			this.UnloadingDifferencesPreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnloadingDifferencesPreviousDocumentsGrid.Name = "UnloadingDifferencesPreviousDocumentsGrid";
			this.UnloadingDifferencesPreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 56, true);
			this.UnloadingDifferencesPreviousDocumentsGrid.TabIndex = 0;
			// 
			// Phase5UnloadingDifferencesPreviousDocumentsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.UnloadingDifferencesPreviousDocumentsGrid);
			this.Name = "Phase5UnloadingDifferencesPreviousDocumentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 56, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.UnloadingDifferencesPreviousDocumentsGrid)).EndInit();
			this.UnloadingDifferencesPreviousDocumentsGrid.ResumeLayout(false);
			this.UnloadingDifferencesPreviousDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid UnloadingDifferencesPreviousDocumentsGrid;
	}
}
