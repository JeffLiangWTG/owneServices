
namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class AdditionalInfosUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.AdditionalInfosGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AdditionalInfosGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AddInfoTypeCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AddiInfoDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AdditionalInfosPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).BeginInit();
			this.AdditionalInfosGrid.SuspendLayout();
			this.AdditionalInfosGroupBox.SuspendLayout();
			this.AddInfoTypeCodeDropEdit.SuspendLayout();
			this.AdditionalInfosPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// AdditionalInfosGrid
			// 
			this.AdditionalInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalInfosGrid, "FilteredInvoiceLines.AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null))).SyncRoot)).AdditionalInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null))).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null))).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null))).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null))).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_NctsExportFromEC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null))).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Status)));
			this.AdditionalInfosGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(340);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("16bd01d5-fd4e-4ba2-a245-cd51692a54a3", "Export from other country");
			zDropEditColumnStyleInfo2.ColumnName = "CSI_RN_NKCountryCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(147);
			zCheckBoxColumnStyleInfo1.ColumnName = "CSI_NctsExportFromEC";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("c98cc126-eb4b-4bc7-9e71-85f385d707d8", "Issuer");
			zDropEditColumnStyleInfo3.ColumnName = "CSI_Status";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AdditionalInfosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AdditionalInfosGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AdditionalInfosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AdditionalInfosGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AdditionalInfosGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.AdditionalInfosGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosGrid.GridId = "97634e6a-c7d0-48d3-a892-5aa308d1826a";
			this.AdditionalInfosGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AdditionalInfosGrid.LayoutKey = "AdditionalInfosGrid";
			this.AdditionalInfosGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosGrid.Name = "AdditionalInfosGrid";
			this.AdditionalInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 154, true);
			this.AdditionalInfosGrid.TabIndex = 6;
			// 
			// AdditionalInfosGroupBox
			//
			this.AdditionalInfosGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1e63aff7-c014-4a39-a0c7-f855b2b678d5", "[44] Additional Infos");
			this.AdditionalInfosGroupBox.Controls.Add(this.AddInfoTypeCodeDropEdit);
			this.AdditionalInfosGroupBox.Controls.Add(this.AddiInfoDescriptionTextBox);
			this.AdditionalInfosGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosGroupBox.Name = "AdditionalInfosGroupBox";
			this.AdditionalInfosGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 120, true);
			this.AdditionalInfosGroupBox.TabIndex = 0;
			this.AdditionalInfosGroupBox.TabStop = false;
			// 
			// AddInfoTypeCodeDropEdit
			// 
			this.AddInfoTypeCodeDropEdit.AllowDrop = true;
			this.AddInfoTypeCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AddInfoTypeCodeDropEdit, "FilteredInvoiceLines.AdditionalInfos.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null))).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			this.AddInfoTypeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true);
			this.AddInfoTypeCodeDropEdit.Name = "AddInfoTypeCodeDropEdit";
			this.AddInfoTypeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 20, true);
			this.AddInfoTypeCodeDropEdit.TabIndex = 1;
			// 
			// AddiInfoDescriptionTextBox
			// 
			this.AddiInfoDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AddiInfoDescriptionTextBox, "FilteredInvoiceLines.AdditionalInfos.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos.AdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine)(((System.Collections.IList)((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null))).SyncRoot)).AdditionalInfos)).SyncRoot)).CSI_Description)));
			this.AddiInfoDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 38, true);
			this.AddiInfoDescriptionTextBox.Multiline = true;
			this.AddiInfoDescriptionTextBox.Name = "AddiInfoDescriptionTextBox";
			this.AddiInfoDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(375, 76, true);
			this.AddiInfoDescriptionTextBox.TabIndex = 3;
			// 
			// AdditionalInfosPanel
			// 
			this.AdditionalInfosPanel.Controls.Add(this.AdditionalInfosGroupBox);
			this.AdditionalInfosPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AdditionalInfosPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 154, true);
			this.AdditionalInfosPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 120, true);
			this.AdditionalInfosPanel.Name = "AdditionalInfosPanel";
			this.AdditionalInfosPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 120, true);
			this.AdditionalInfosPanel.TabIndex = 5;
			// 
			// AdditionalInfosUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalInfosGrid);
			this.Controls.Add(this.AdditionalInfosPanel);
			this.Name = "AdditionalInfosUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(457, 274, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AdditionalInfosGrid)).EndInit();
			this.AdditionalInfosGrid.ResumeLayout(false);
			this.AdditionalInfosGrid.PerformLayout();
			this.AdditionalInfosGroupBox.ResumeLayout(false);
			this.AdditionalInfosGroupBox.PerformLayout();
			this.AddInfoTypeCodeDropEdit.ResumeLayout(true);
			this.AddInfoTypeCodeDropEdit.PerformLayout();
			this.AdditionalInfosPanel.ResumeLayout(false);
			this.AdditionalInfosPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected Enterprise.ZArchitecture.GUI.ZGroupBox AdditionalInfosGroupBox;
		protected Enterprise.ZArchitecture.ZTextBox AddiInfoDescriptionTextBox;
		protected Enterprise.ZArchitecture.GUI.ZPanel AdditionalInfosPanel;
		protected Enterprise.ZArchitecture.ZGrid AdditionalInfosGrid;
		protected Enterprise.ZArchitecture.GUI.ZDropEdit AddInfoTypeCodeDropEdit;
	}
}
