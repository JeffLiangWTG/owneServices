
namespace Enterprise.Customs.EU.NCTS.GUI
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc);
			// 
			// AdditionalInfosGrid
			// 
			this.AdditionalInfosGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AdditionalInfosGrid, "AdditionalInfos");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).AdditionalInfos)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).AdditionalInfos)).SyncRoot)).CSI_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).AdditionalInfos)).SyncRoot)).CSI_RN_NKCountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).AdditionalInfos)).SyncRoot)).CSI_NctsExportFromEC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).AdditionalInfos)).SyncRoot)).CSI_Status)));
			this.AdditionalInfosGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.ColumnName = "CSI_Code";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CSI_Description";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(610);
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("963DB499-B883-4762-8A38-C750ADB63FFB", "Export from other country");
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo2.ColumnName = "CSI_RN_NKCountryCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(149);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("D683DB29-850D-4F70-BD85-5457585EA0FB", "Export from EC");
			zCheckBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			zCheckBoxColumnStyleInfo1.ColumnName = "CSI_NctsExportFromEC";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(97);
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("9D7DB3B5-A394-4800-9218-558D7350356E", "Issuer");
			zDropEditColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo3.ColumnName = "CSI_Status";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(53);
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
			this.AdditionalInfosGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 154, true);
			this.AdditionalInfosGrid.TabIndex = 6;
			// 
			// AdditionalInfosGroupBox
			// 
			this.AdditionalInfosGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("112126A8-7B8F-409C-9FD8-89697C82CFA2", "[44] Additional Infos");
			this.AdditionalInfosGroupBox.Controls.Add(this.AddInfoTypeCodeDropEdit);
			this.AdditionalInfosGroupBox.Controls.Add(this.AddiInfoDescriptionTextBox);
			this.AdditionalInfosGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AdditionalInfosGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalInfosGroupBox.Name = "AdditionalInfosGroupBox";
			this.AdditionalInfosGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 120, true);
			this.AdditionalInfosGroupBox.TabIndex = 0;
			this.AdditionalInfosGroupBox.TabStop = false;
			// 
			// AddInfoTypeCodeDropEdit
			// 
			this.AddInfoTypeCodeDropEdit.AllowDrop = true;
			this.AddInfoTypeCodeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AddInfoTypeCodeDropEdit, "AdditionalInfos.CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).AdditionalInfos)).SyncRoot)).CSI_Code)));
			this.AddInfoTypeCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true);
			this.AddInfoTypeCodeDropEdit.Name = "AddInfoTypeCodeDropEdit";
			this.AddInfoTypeCodeDropEdit.ShouldResizeByMaxLength = true;
			this.AddInfoTypeCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 20, true);
			this.AddInfoTypeCodeDropEdit.TabIndex = 1;
			// 
			// AddiInfoDescriptionTextBox
			// 
			this.AddiInfoDescriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.AddiInfoDescriptionTextBox, "AdditionalInfos.CSI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.NctsAdditionalInfo)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.NctsCommonCargoDesc)(null)).AdditionalInfos)).SyncRoot)).CSI_Description)));
			this.AddiInfoDescriptionTextBox.CaptionResourceString = null;
			this.AddiInfoDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 38, true);
			this.AddiInfoDescriptionTextBox.Multiline = true;
			this.AddiInfoDescriptionTextBox.Name = "AddiInfoDescriptionTextBox";
			this.AddiInfoDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 76, true);
			this.AddiInfoDescriptionTextBox.TabIndex = 3;
			// 
			// AdditionalInfosPanel
			// 
			this.AdditionalInfosPanel.Controls.Add(this.AdditionalInfosGroupBox);
			this.AdditionalInfosPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.AdditionalInfosPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 154, true);
			this.AdditionalInfosPanel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 120, true);
			this.AdditionalInfosPanel.Name = "AdditionalInfosPanel";
			this.AdditionalInfosPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 120, true);
			this.AdditionalInfosPanel.TabIndex = 5;
			// 
			// AdditionalInfosUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalInfosGrid);
			this.Controls.Add(this.AdditionalInfosPanel);
			this.Name = "AdditionalInfosUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 274, true);
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
