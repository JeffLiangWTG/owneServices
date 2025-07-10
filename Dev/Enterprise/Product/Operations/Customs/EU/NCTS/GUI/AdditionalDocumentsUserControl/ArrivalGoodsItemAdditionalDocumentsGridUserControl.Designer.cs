namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class ArrivalGoodsItemAdditionalDocumentsGridUserControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo lineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo statusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.AdditionalDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsGrid)).BeginInit();
			this.AdditionalDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.INctsAdditionalInfoCollection<Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo>);
			// 
			// AdditionalDocumentsGrid
			// 
			this.AdditionalDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalDocumentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(null)).CSI_Description)));
			this.AdditionalDocumentsGrid.CaptionVisible = false;
			lineNoCalcEdit.BindToDecimalPlaces = null;
			lineNoCalcEdit.ColumnName = "CSI_LineNo";
			lineNoCalcEdit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			statusDropEdit.ColumnName = "CSI_Status";
			statusDropEdit.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.ColumnName = "CSI_SubType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.AdditionalDocumentsGrid.ColumnStyles.Add(lineNoCalcEdit);
			this.AdditionalDocumentsGrid.ColumnStyles.Add(statusDropEdit);
			this.AdditionalDocumentsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.AdditionalDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AdditionalDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalDocumentsGrid.GridId = "194e9e62-ec30-46c6-aba3-c3f90fd567b3";
			this.AdditionalDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalDocumentsGrid.LayoutKey = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalDocumentsGrid.Name = "AdditionalDocumentsGrid";
			this.AdditionalDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 135, true);
			this.AdditionalDocumentsGrid.TabIndex = 0;
			// 
			// GoodsItemAdditionalDocumentsGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalDocumentsGrid);
			this.Name = "GoodsItemAdditionalDocumentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(620, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalDocumentsGrid)).EndInit();
			this.AdditionalDocumentsGrid.ResumeLayout(false);
			this.AdditionalDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		protected internal ZArchitecture.ZGrid AdditionalDocumentsGrid;
	}
}
