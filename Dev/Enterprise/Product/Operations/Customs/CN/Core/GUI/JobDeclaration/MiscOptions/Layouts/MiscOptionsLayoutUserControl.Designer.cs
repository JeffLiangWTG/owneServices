namespace Enterprise.Customs.CN.GUI
{
	partial class MiscOptionsLayoutUserControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MoreMergeOptionsSeparatorUserControl = new Enterprise.ZArchitecture.GUI.SeparatorUserControl();
			this.MergeOptionsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MoreMergeOptionsSeparatorUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MergeOptionsGrid)).BeginInit();
			this.MergeOptionsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.JobDeclaration);
			// 
			// MoreMergeOptionsSeparatorUserControl
			// 
			this.MoreMergeOptionsSeparatorUserControl.AllowDrop = true;
			this.MoreMergeOptionsSeparatorUserControl.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("c38890d2-da9c-48fb-a9c2-097592a4b20c", "More Merge Options");
			this.MoreMergeOptionsSeparatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 169, true);
			this.MoreMergeOptionsSeparatorUserControl.Name = "MoreMergeOptionsSeparatorUserControl";
			this.MoreMergeOptionsSeparatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 15, true);
			this.MoreMergeOptionsSeparatorUserControl.TabIndex = 0;
			// 
			// MergeOptionsGrid
			// 
			this.MergeOptionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.MergeOptionsGrid, "MergingRuleOptions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).MergingRuleOptions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.CN.Business.CodeDescriptionOption)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).MergingRuleOptions)).SyncRoot)).Selected)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.CodeDescriptionOption)(((System.Collections.IList)(((Enterprise.Customs.CN.Business.JobDeclaration)(null)).MergingRuleOptions)).SyncRoot)).Description)));
			this.MergeOptionsGrid.CaptionVisible = false;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("EF9E1A3E-2053-4FC8-9669-97425AFF7A64", "Selected");
			zCheckBoxColumnStyleInfo1.ColumnName = "Selected";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("79D98368-B61E-4534-9D83-3ED61B5F6494", "Merge By");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			this.MergeOptionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.MergeOptionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.MergeOptionsGrid.GridId = "17d485bc-eff7-4784-990e-4d6575c348b5";
			this.MergeOptionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.MergeOptionsGrid.LayoutKey = "MergeOptionsGrid";
			this.MergeOptionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
			this.MergeOptionsGrid.Name = "MergeOptionsGrid";
			this.MergeOptionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(469, 134, true);
			this.MergeOptionsGrid.TabIndex = 1;
			// 
			// MiscOptionsLayoutUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MoreMergeOptionsSeparatorUserControl);
			this.Controls.Add(this.MergeOptionsGrid);
			this.Name = "MiscOptionsLayoutUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(502, 300, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MoreMergeOptionsSeparatorUserControl.ResumeLayout(true);
			this.MoreMergeOptionsSeparatorUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MergeOptionsGrid)).EndInit();
			this.MergeOptionsGrid.ResumeLayout(false);
			this.MergeOptionsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.SeparatorUserControl MoreMergeOptionsSeparatorUserControl;
		internal Enterprise.ZArchitecture.ZGrid MergeOptionsGrid;
	}
}
