
namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentDifferencesPreviousDocumentsGridUserControl
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
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.PreviousDocumentsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).BeginInit();
			this.PreviousDocumentsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EU.Business.Declaration.MultiLineAddInfos.PreviousDocument);
			// 
			// PreviousDocumentsGrid
			// 
			this.PreviousDocumentsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PreviousDocumentsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ItemNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Quantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_UnitOfQuantity2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Quantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_UnitOfQuantity)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_ReferenceNumber2)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EU.Business.Declaration.MultiLineAddInfos.PreviousDocument)(null)).CSI_Status)));
			this.PreviousDocumentsGrid.CaptionVisible = false;
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CSI_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_ReferenceNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.ColumnName = "CSI_ReferenceNumber2";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "CSI_LineNo";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CSI_Status";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PreviousDocumentsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PreviousDocumentsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PreviousDocumentsGrid.GridId = "2F019DBA-DD85-4A48-8A58-94800DA37E13";
			this.PreviousDocumentsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PreviousDocumentsGrid.LayoutKey = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PreviousDocumentsGrid.Name = "PreviousDocumentsGrid";
			this.PreviousDocumentsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 135, true);
			this.PreviousDocumentsGrid.TabIndex = 0;
			// 
			// HouseConsignmentDifferencesPreviousDocumentsGridUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PreviousDocumentsGrid);
			this.Name = "HouseConsignmentDifferencesPreviousDocumentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(703, 135, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PreviousDocumentsGrid)).EndInit();
			this.PreviousDocumentsGrid.ResumeLayout(false);
			this.PreviousDocumentsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid PreviousDocumentsGrid;
	}
}
