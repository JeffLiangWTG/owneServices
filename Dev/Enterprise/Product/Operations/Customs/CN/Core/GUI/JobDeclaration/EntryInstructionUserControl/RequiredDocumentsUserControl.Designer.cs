namespace Enterprise.Customs.CN.GUI
{
	partial class RequiredDocumentsUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.CIQRequiredDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CIQRequiredDocumentsGrid)).BeginInit();
			this.CIQRequiredDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.CusEntryInstruction);
			// 
			// CIQRequiredDocumentsGrid
			// 
			this.CIQRequiredDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CIQRequiredDocumentsGrid, "CIQRequiredDocuments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CIQRequiredDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CIQRequiredDocument)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CIQRequiredDocuments)).SyncRoot)).XC_DocumentType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CIQRequiredDocument)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CIQRequiredDocuments)).SyncRoot)).XC_DocumentName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CIQRequiredDocument)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CIQRequiredDocuments)).SyncRoot)).XC_NumberOfOriginals)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.CN.Business.CIQRequiredDocument)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.CusEntryInstruction)(null)).CIQRequiredDocuments)).SyncRoot)).XC_NumberOfCopies)));
			this.CIQRequiredDocumentsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "XC_DocumentType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo1.ColumnName = "XC_DocumentName";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "XC_NumberOfOriginals";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "XC_NumberOfCopies";
			zCalcEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.CIQRequiredDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.CIQRequiredDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CIQRequiredDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.CIQRequiredDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.CIQRequiredDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CIQRequiredDocumentsGrid.GridId = "77039464-87ef-4d3d-859f-b315b326aef8";
			this.CIQRequiredDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CIQRequiredDocumentsGrid.LayoutKey = "CIQRequiredDocumentsGrid";
			this.CIQRequiredDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CIQRequiredDocumentsGrid.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.CIQRequiredDocumentsGrid.Name = "CIQRequiredDocumentsGrid";
			this.CIQRequiredDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 217, true);
			this.CIQRequiredDocumentsGrid.TabIndex = 0;
			// 
			// RequiredDocumentsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CIQRequiredDocumentsGrid);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(1, true);
			this.Name = "RequiredDocumentsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(395, 217, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CIQRequiredDocumentsGrid)).EndInit();
			this.CIQRequiredDocumentsGrid.ResumeLayout(false);
			this.CIQRequiredDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZGrid CIQRequiredDocumentsGrid;
	}
}
