using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ASYCUDA.GUI
{
	partial class HeaderManagementUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.WR_CountryCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ManifestHeaderMainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WR_ManifestTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zButtonDeleteManifest = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zDropEditManifestToDelete = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.CreateHeaderButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ManifestsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ManifestsGrid = new Enterprise.Customs.ASYCUDA.GUI.ManifestModuleButtonGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.WR_CountryCodeDropEdit.SuspendLayout();
			this.ManifestHeaderMainPanel.SuspendLayout();
			this.WR_ManifestTypeDropEdit.SuspendLayout();
			this.zDropEditManifestToDelete.SuspendLayout();
			this.ManifestsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.ManifestsGrid.InnerGrid)).BeginInit();
			this.ManifestsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.ManifestHeadersWrapper);
			// 
			// WR_CountryCodeDropEdit
			// 
			this.WR_CountryCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WR_CountryCodeDropEdit, "WR_CountryCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.ManifestHeadersWrapper)(null)).WR_CountryCode)));
			this.WR_CountryCodeDropEdit.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("d368e231-1094-4277-b31f-4bba3f60b9f0", "New Manifest");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.WR_CountryCodeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.WR_CountryCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 17, true);
			this.WR_CountryCodeDropEdit.Name = "WR_CountryCodeDropEdit";
			this.WR_CountryCodeDropEdit.PreBoundMaxLength = 2;
			this.WR_CountryCodeDropEdit.ShowDescriptionBox = false;
			this.WR_CountryCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.WR_CountryCodeDropEdit.TabIndex = 0;
			// 
			// ManifestHeaderMainPanel
			// 
			this.ManifestHeaderMainPanel.Controls.Add(this.WR_ManifestTypeDropEdit);
			this.ManifestHeaderMainPanel.Controls.Add(this.zButtonDeleteManifest);
			this.ManifestHeaderMainPanel.Controls.Add(this.zDropEditManifestToDelete);
			this.ManifestHeaderMainPanel.Controls.Add(this.CreateHeaderButton);
			this.ManifestHeaderMainPanel.Controls.Add(this.WR_CountryCodeDropEdit);
			this.ManifestHeaderMainPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.ManifestHeaderMainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ManifestHeaderMainPanel.Name = "ManifestHeaderMainPanel";
			this.ManifestHeaderMainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 85, true);
			this.ManifestHeaderMainPanel.TabIndex = 0;
			// 
			// WR_ManifestTypeDropEdit
			// 
			this.WR_ManifestTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WR_ManifestTypeDropEdit, "WR_ManifestType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.ManifestHeadersWrapper)(null)).WR_ManifestType)));
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.WR_ManifestTypeDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.WR_ManifestTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(171, 17, true);
			this.WR_ManifestTypeDropEdit.Name = "WR_ManifestTypeDropEdit";
			this.WR_ManifestTypeDropEdit.PreBoundMaxLength = 6;
			this.WR_ManifestTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
			this.WR_ManifestTypeDropEdit.TabIndex = 1;
			// 
			// zButtonDeleteManifest
			// 
			this.zButtonDeleteManifest.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("528d2d27-0260-4474-bde5-e6cd0dbda6a2", "Delete");
			this.zButtonDeleteManifest.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 49, true);
			this.zButtonDeleteManifest.Name = "zButtonDeleteManifest";
			this.zButtonDeleteManifest.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 22, true);
			this.zButtonDeleteManifest.TabIndex = 4;
			this.zButtonDeleteManifest.ToolTipCaption = null;
			this.zButtonDeleteManifest.UseVisualStyleBackColor = true;
			this.zButtonDeleteManifest.Click += new System.EventHandler(this.DeleteManifestButton_Click);
			// 
			// zDropEditManifestToDelete
			// 
			this.zDropEditManifestToDelete.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zDropEditManifestToDelete, "WR_KeywordCombination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ASYCUDA.Business.ManifestHeadersWrapper)(null)).WR_KeywordCombination)));
			this.zDropEditManifestToDelete.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("1bc7c50c-503f-45a4-a3a8-a3ee200365df", "Delete existing Manifest");
			this.zDropEditManifestToDelete.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 50, true);
			this.zDropEditManifestToDelete.Name = "zDropEditManifestToDelete";
			this.zDropEditManifestToDelete.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(251, 20, true);
			this.zDropEditManifestToDelete.TabIndex = 3;
			// 
			// CreateHeaderButton
			// 
			this.CreateHeaderButton.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("492db835-0e42-40e3-940d-e3d24f305c34", "Create");
			this.CreateHeaderButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(391, 16, true);
			this.CreateHeaderButton.Name = "CreateHeaderButton";
			this.CreateHeaderButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 22, true);
			this.CreateHeaderButton.TabIndex = 2;
			this.CreateHeaderButton.ToolTipCaption = null;
			this.CreateHeaderButton.UseVisualStyleBackColor = true;
			this.CreateHeaderButton.Click += new System.EventHandler(this.CreateHeaderButton_Click);
			// 
			// ManifestsGroupBox
			// 
			this.ManifestsGroupBox.CaptionResourceString = Enterprise.Customs.ASYCUDA.Gui.Res.GetData("D61785A1-8EF7-4DDF-A09B-65572DF39407", "Manifests");
			this.ManifestsGroupBox.Controls.Add(this.ManifestsGrid);
			this.ManifestsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.ManifestsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 50, true);
			this.ManifestsGroupBox.Name = "ManifestsGroupBox";
			this.ManifestsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 300, true);
			this.ManifestsGroupBox.TabIndex = 1;
			this.ManifestsGroupBox.TabStop = false;
			// 
			// ManifestsGrid
			// 
			this.ManifestsGrid.AllowDrop = true;
			this.ManifestsGrid.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ManifestsGrid, "Headers");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.ASYCUDA.Business.ManifestHeadersWrapper)(null)).Headers)));
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "AMA_JobReference";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CountryName";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "AMA_ManifestType";
			zTextBoxColumnStyleInfo3.IsMandatory = true;
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo4.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo4.ColumnName = "AMA_Nature";
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo5.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo5.ColumnName = "AMA_MasterBill";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo6.ColumnName = "AMA_RL_NKPortOfLoading";
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo7.ColumnName = "AMA_E_DEP";
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo8.ColumnName = "AMA_RL_NKPortOfDischarge";
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo9.ColumnName = "AMA_E_ARV";
			zTextBoxColumnStyleInfo9.IsReadOnly = true;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo10.ColumnName = "MasterBill+ABL_BillStatus";
			zTextBoxColumnStyleInfo10.IsReadOnly = true;
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo11.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo11.ColumnName = "AMA_MessageStatus";
			zTextBoxColumnStyleInfo11.IsReadOnly = true;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo12.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo12.ColumnName = "RegistrationStatusCodeAndDescription";
			zTextBoxColumnStyleInfo12.IsReadOnly = true;
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.ManifestsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.ManifestsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestsGrid.GridId = "9A09F432-6024-4838-9B3D-AED75B70B19B";
			// 
			// 
			// 
			this.ManifestsGrid.InnerGrid.AllowNavigation = false;
			this.ManifestsGrid.InnerGrid.CaptionVisible = false;
			this.ManifestsGrid.InnerGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ManifestsGrid.InnerGrid.GridId = "9A09F432-6024-4838-9B3D-AED75B70B19B";
			this.ManifestsGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ManifestsGrid.InnerGrid.LayoutKey = "ManifestsBoundGrid";
			this.ManifestsGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.ManifestsGrid.InnerGrid.Name = "Grid";
			this.ManifestsGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(479, 182, true);
			this.ManifestsGrid.InnerGrid.TabIndex = 0;
			this.ManifestsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.ManifestsGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.ASYCUDA.Manifest;
			this.ManifestsGrid.Name = "ManifestsGrid";
			this.ManifestsGrid.ReadOnly = false;
			this.ManifestsGrid.ShowAttachButton = false;
			this.ManifestsGrid.ShowDetachButton = false;
			this.ManifestsGrid.ShowEditButton = false;
			this.ManifestsGrid.ShowNewButton = false;
			this.ManifestsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 220, true);
			this.ManifestsGrid.TabIndex = 0;
			// 
			// HeaderManagementUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ManifestsGroupBox);
			this.Controls.Add(this.ManifestHeaderMainPanel);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, true);
			this.Name = "HeaderManagementUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(491, 330, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.WR_CountryCodeDropEdit.ResumeLayout(true);
			this.WR_CountryCodeDropEdit.PerformLayout();
			this.ManifestHeaderMainPanel.ResumeLayout(false);
			this.ManifestHeaderMainPanel.PerformLayout();
			this.WR_ManifestTypeDropEdit.ResumeLayout(true);
			this.WR_ManifestTypeDropEdit.PerformLayout();
			this.zDropEditManifestToDelete.ResumeLayout(true);
			this.zDropEditManifestToDelete.PerformLayout();
			this.ManifestsGroupBox.ResumeLayout(false);
			this.ManifestsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.ManifestsGrid.InnerGrid)).EndInit();
			this.ManifestsGrid.ResumeLayout(true);
			this.ManifestsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZDropEdit WR_CountryCodeDropEdit;
		ZPanel ManifestHeaderMainPanel;
		ZButton CreateHeaderButton;
		ZButton zButtonDeleteManifest;
		ZDropEditWithFixedWidth zDropEditManifestToDelete;
		ZDropEdit WR_ManifestTypeDropEdit;
		ZGroupBox ManifestsGroupBox;
		ManifestModuleButtonGrid ManifestsGrid;
	}
}
