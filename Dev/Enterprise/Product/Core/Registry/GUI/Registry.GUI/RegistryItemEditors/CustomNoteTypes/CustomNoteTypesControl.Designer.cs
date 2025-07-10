using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	partial class CustomNoteTypesControl : RegistryBusinessObjectTemplateZUserControl
	{
		ZModuleCountryTreeView ModuleAndCountryTreeView;
		CargoWise.Windows.UI.KSplitContainer splitContainer1;
		ZGrid zGrid1;

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.ModuleAndCountryTreeView = new Enterprise.Registry.GUI.ZModuleCountryTreeView();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.zGrid1 = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CustomNoteTypes);
			// 
			// ModuleAndCountryTreeView
			// 
			this.ModuleAndCountryTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ModuleAndCountryTreeView.HideSelection = false;
			this.ModuleAndCountryTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ModuleAndCountryTreeView.Name = "ModuleAndCountryTreeView";
			this.ModuleAndCountryTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 235, true);
			this.ModuleAndCountryTreeView.TabIndex = 0;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.ModuleAndCountryTreeView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.zGrid1);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 364, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(235);
			this.splitContainer1.TabIndex = 1;
			// 
			// zGrid1
			// 
			this.zGrid1.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.zGrid1, "SelectedModuleCollectionForBinding.CustomNoteTypesList");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteModuleAndCountry)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteTypes)(null)).SelectedModuleCollectionForBinding)).SyncRoot)).CustomNoteTypesList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CustomNoteTypeItem)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteModuleAndCountry)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteTypes)(null)).SelectedModuleCollectionForBinding)).SyncRoot)).CustomNoteTypesList)).SyncRoot)).NoteName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CustomNoteTypeItem)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteModuleAndCountry)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteTypes)(null)).SelectedModuleCollectionForBinding)).SyncRoot)).CustomNoteTypesList)).SyncRoot)).DefaultVisibility)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteTypeItem)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteModuleAndCountry)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteTypes)(null)).SelectedModuleCollectionForBinding)).SyncRoot)).CustomNoteTypesList)).SyncRoot)).VisibilityList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CustomNoteTypeItem)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteModuleAndCountry)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteTypes)(null)).SelectedModuleCollectionForBinding)).SyncRoot)).CustomNoteTypesList)).SyncRoot)).IsTextOnly)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CustomNoteTypeItem)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteModuleAndCountry)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteTypes)(null)).SelectedModuleCollectionForBinding)).SyncRoot)).CustomNoteTypesList)).SyncRoot)).IsAppendingNote)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CustomNoteTypeItem)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteModuleAndCountry)(((System.Collections.IList)(((Enterprise.Registry.Business.CustomNoteTypes)(null)).SelectedModuleCollectionForBinding)).SyncRoot)).CustomNoteTypesList)).SyncRoot)).IsReadOnlyAfterAdd)));
			this.zGrid1.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CustomNoteTypesControl|e73bef3e-5a51-4395-ae5f-26a0d0308d96", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "NoteName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo1.BindToList = "VisibilityList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CustomNoteTypesControl|5a1e69a1-2b67-4c46-a6eb-d79aa79b612c", "Visibility");
			zDropEditColumnStyleInfo1.ColumnName = "DefaultVisibility";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CustomNoteTypesControl|1411cc53-d40b-45e0-a2e7-97bc6d02e05c", "Text Only");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsTextOnly";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CustomNoteTypesControl|8059772f-6c51-4d0a-8ef6-fc9c58a76e45", "Is Appending Note");
			zCheckBoxColumnStyleInfo2.ColumnName = "IsAppendingNote";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(105);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("CustomNoteTypesControl|e433daed-37af-440d-8473-c7467341477b", "Non-Editable");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsReadOnlyAfterAdd";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.zGrid1.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.zGrid1.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.zGrid1.GridId = "52f3091c-3390-43d2-a56a-560529acb2a9";
			this.zGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.zGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.zGrid1.LayoutKey = "zGrid1";
			this.zGrid1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zGrid1.Name = "zGrid1";
			this.zGrid1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 125, true);
			this.zGrid1.TabIndex = 0;
			// 
			// CustomNoteTypesControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.splitContainer1);
			this.Name = "CustomNoteTypesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(613, 364, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.zGrid1)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
