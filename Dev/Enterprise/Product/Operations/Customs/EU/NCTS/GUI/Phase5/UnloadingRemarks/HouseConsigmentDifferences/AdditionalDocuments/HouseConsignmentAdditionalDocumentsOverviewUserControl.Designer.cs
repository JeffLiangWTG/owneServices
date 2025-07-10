using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class HouseConsignmentAdditionalDocumentsOverviewUserControl
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
			SequenceNumberTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			UnloadedStateDropEditColumn = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			KindDropEditColumn = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			DocTypeCodeFindBoxColumn = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			ReferenceNumberTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			TextTextBoxColumn = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();

			this.HouseConsignmentAdditionalDocumentsOverviewGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentAdditionalDocumentsOverviewGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocumentCollection<NctsBillAdditionalDocument>);
			// 
			// HouseConsignmentDifferencesGrid
			// 
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.HouseConsignmentAdditionalDocumentsOverviewGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsBill)(null)).AdditionalDocuments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_LineNo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_Status)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_SubType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsBillAdditionalDocument)(null)).CSI_Description)));
			
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.CaptionVisible = false;
			//SequenceNumberTextBoxColumn.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("C2B25D9F-898E-4630-A3E7-4AE2F56A8AD7", "Sequence No.");
			SequenceNumberTextBoxColumn.ColumnName = "CSI_LineNo";
			SequenceNumberTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			SequenceNumberTextBoxColumn.IsReadOnly = true;
			//UnloadedStateDropEditColumn.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("0B6DB19C-80A9-417F-92AC-0EB8EADB3441", "Unloaded State");
			UnloadedStateDropEditColumn.ColumnName = "CSI_Status";
			UnloadedStateDropEditColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			//KindDropEditColumn.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("1DBB4241-1A72-47E8-9BCC-152DB016E047", "Kind");
			KindDropEditColumn.ColumnName = "CSI_SubType";
			KindDropEditColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			//DocTypeCodeFindBoxColumn.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("15B316CD-3935-4B9D-B186-FFCA9FBB9072", "Doc. Type");
			DocTypeCodeFindBoxColumn.ColumnName = "CSI_Code";
			DocTypeCodeFindBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			//ReferenceNumberTextBoxColumn.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("B506CAAC-5E3C-4837-B9FE-FC3ED0AC74D1", "Reference Number");
			ReferenceNumberTextBoxColumn.ColumnName = "CSI_ReferenceNumber";
			ReferenceNumberTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			//TextTextBoxColumn.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("931C58AE-6825-4918-8A55-04BB6305E5C7", "Text");
			TextTextBoxColumn.ColumnName = "CSI_Description";
			TextTextBoxColumn.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(480);
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.ColumnStyles.Add(SequenceNumberTextBoxColumn);
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.ColumnStyles.Add(UnloadedStateDropEditColumn);
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.ColumnStyles.Add(KindDropEditColumn);
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.ColumnStyles.Add(DocTypeCodeFindBoxColumn);
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.ColumnStyles.Add(ReferenceNumberTextBoxColumn);
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.ColumnStyles.Add(TextTextBoxColumn);
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.GridId = "710D1797-7472-4AC5-8241-235767A04DFE";
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.LayoutKey = "HouseConsignmentAdditionalDocumentsOverviewGrid";
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.Name = "HouseConsignmentAdditionalDocumentsOverviewGrid";
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
			this.HouseConsignmentAdditionalDocumentsOverviewGrid.TabIndex = 0;
			// 
			// HouseConsignmentDifferencesOverviewUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.HouseConsignmentAdditionalDocumentsOverviewGrid);
			this.Name = "HouseConsignmentDifferencesOverviewUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1700, 266, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.HouseConsignmentAdditionalDocumentsOverviewGrid)).EndInit();
			this.ResumeLayout(false);
		}

		internal ZArchitecture.ZGrid HouseConsignmentAdditionalDocumentsOverviewGrid;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo SequenceNumberTextBoxColumn;
		internal Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo UnloadedStateDropEditColumn;
		internal Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo KindDropEditColumn;
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo DocTypeCodeFindBoxColumn;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo ReferenceNumberTextBoxColumn;
		internal Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo TextTextBoxColumn;
		#endregion
	}
}
