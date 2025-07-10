namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentGoodItemsSupportingDocumentsGridUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			SequenceNumberTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			UnloadedStateDropEditColumn = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			DocTypeCodeFindBoxColumn = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			ReferenceNumberTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			ComplementInfoTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocumentCollection<Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument>);
			// 
			// HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid
			// 
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsArrivalCargoDesc)(null)).SupportingDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(null)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(null)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(null)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(null)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsSupportingDocument)(null)).CSI_ReferenceNumber2)));

			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.CaptionVisible = false;
			SequenceNumberTextBoxColumn.ColumnName = "CSI_LineNo";
			SequenceNumberTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			SequenceNumberTextBoxColumn.IsReadOnly = true;
			UnloadedStateDropEditColumn.ColumnName = "CSI_Status";
			UnloadedStateDropEditColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			DocTypeCodeFindBoxColumn.ColumnName = "CSI_Code";
			DocTypeCodeFindBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			ReferenceNumberTextBoxColumn.ColumnName = "CSI_ReferenceNumber";
			ReferenceNumberTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			ComplementInfoTextBoxColumn.ColumnName = "CSI_ReferenceNumber2";
			ComplementInfoTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(480);
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.ColumnStyles.Add(SequenceNumberTextBoxColumn);
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.ColumnStyles.Add(UnloadedStateDropEditColumn);
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.ColumnStyles.Add(DocTypeCodeFindBoxColumn);
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.ColumnStyles.Add(ReferenceNumberTextBoxColumn);
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.ColumnStyles.Add(ComplementInfoTextBoxColumn);
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.GridId = "9A0ABCE7-BE07-424D-A9F7-86CB0C6DAA6A";
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.LayoutKey = "HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid";
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.Name = "HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid";
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
			this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid.TabIndex = 0;
			// 
			// HouseConsignmentGoodItemsSupportingDocumentsGridUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid);
			this.Name = "HouseConsignmentGoodItemsSupportingDocumentsGridUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid)).EndInit();
			this.ResumeLayout(false);
		}

		internal ZArchitecture.ZGrid HouseConsignmentGoodItemsSupportingDocumentsOverviewGrid;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo SequenceNumberTextBoxColumn;
		internal Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo UnloadedStateDropEditColumn;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo DocTypeCodeFindBoxColumn;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo ReferenceNumberTextBoxColumn;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo ComplementInfoTextBoxColumn;
		#endregion
	}
}
