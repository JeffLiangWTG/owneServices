using System;
using CargoWise.Windows.UI;
using Enterprise.Client.AUS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.AUS.GUI
{
	public partial class ClientAUSOriginPreferenceMappingForm : ZForm
	{
		Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox SupplierFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox OriginFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel3;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox ImporterFindBox;
		Enterprise.ZArchitecture.ZLabel zLabel4;
		Enterprise.ZArchitecture.ZLabel zLabel5;
		Enterprise.ZArchitecture.ZLabel zLabel7;
		Enterprise.ZArchitecture.GUI.ZDropEdit PrefSchemeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit PrefRulesDropEdit;
		Enterprise.ZArchitecture.GUI.ZCodeFindBox PrefOriginFindBox;

		new void InitializeComponent()
		{
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.SupplierFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.ImporterFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.PrefOriginFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel7 = new Enterprise.ZArchitecture.ZLabel();
			this.PrefSchemeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PrefRulesDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 286, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 24, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(220);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(221);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping);
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 253, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.PostingButtonsUserControl.TabIndex = 30;
			// 
			// SupplierFindBox
			// 
			this.BindingSource.SetBindingMember(this.SupplierFindBox, "T7_OH_Supplier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).T7_OH_Supplier)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).SupplierList)));
			this.SupplierFindBox.BindToList = "SupplierList";
			this.SupplierFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 49, true);
			this.SupplierFindBox.Name = "SupplierFindBox";
			this.SupplierFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.SupplierFindBox.TabIndex = 18;
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 46, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.zLabel2.TabIndex = 19;
			this.zLabel2.Text = "Supplier";
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 16, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 20, true);
			this.zLabel1.TabIndex = 17;
			this.zLabel1.Text = "Importer";
			// 
			// ImporterFindBox
			// 
			this.BindingSource.SetBindingMember(this.ImporterFindBox, "T7_OH_Importer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).T7_OH_Importer)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).ImporterList)));
			this.ImporterFindBox.BindToList = "ImporterList";
			this.ImporterFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 16, true);
			this.ImporterFindBox.Name = "ImporterFindBox";
			this.ImporterFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ImporterFindBox.TabIndex = 16;
			// 
			// OriginFindBox
			// 
			this.BindingSource.SetBindingMember(this.OriginFindBox, "T7_RN_NKOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).T7_RN_NKOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).OriginList)));
			this.OriginFindBox.BindToList = "OriginList";
			this.OriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 82, true);
			this.OriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.OriginFindBox.Name = "OriginFindBox";
			this.OriginFindBox.PreBoundMaxLength = 2;
			this.OriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 21, true);
			this.OriginFindBox.TabIndex = 20;
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 76, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 24, true);
			this.zLabel3.TabIndex = 21;
			this.zLabel3.Text = "Origin";
			// 
			// zLabel4
			// 
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 110, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 24, true);
			this.zLabel4.TabIndex = 23;
			this.zLabel4.Text = "Preference Origin";
			// 
			// PrefOriginFindBox
			// 
			this.BindingSource.SetBindingMember(this.PrefOriginFindBox, "T7_RN_NKPreferenceOrigin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).T7_RN_NKPreferenceOrigin)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).OriginList)));
			this.PrefOriginFindBox.BindToList = "OriginList";
			this.PrefOriginFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 116, true);
			this.PrefOriginFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefCountry;
			this.PrefOriginFindBox.Name = "PrefOriginFindBox";
			this.PrefOriginFindBox.PreBoundMaxLength = 2;
			this.PrefOriginFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 21, true);
			this.PrefOriginFindBox.TabIndex = 22;
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 144, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 24, true);
			this.zLabel5.TabIndex = 24;
			this.zLabel5.Text = "Preference Scheme";
			// 
			// zLabel7
			// 
			this.zLabel7.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 178, true);
			this.zLabel7.Name = "zLabel7";
			this.zLabel7.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 24, true);
			this.zLabel7.TabIndex = 25;
			this.zLabel7.Text = "Preference Rules";
			// 
			// PrefSchemeDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PrefSchemeDropEdit, "T7_PreferenceSchemeType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).T7_PreferenceSchemeType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).Lookups.T7_PST_List)));
			this.PrefSchemeDropEdit.BindToList = "Lookups+T7_PST_List";
			this.PrefSchemeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 150, true);
			this.PrefSchemeDropEdit.Name = "PrefSchemeDropEdit";
			this.PrefSchemeDropEdit.PreBoundMaxLength = 4;
			this.PrefSchemeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PrefSchemeDropEdit.TabIndex = 23;
			// 
			// PrefRulesDropEdit
			// 
			this.BindingSource.SetBindingMember(this.PrefRulesDropEdit, "T7_PreferenceRuleType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).T7_PreferenceRuleType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping)(null)).Lookups.T7_PRT_List)));
			this.PrefRulesDropEdit.BindToList = "Lookups+T7_PRT_List";
			this.PrefRulesDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 183, true);
			this.PrefRulesDropEdit.Name = "PrefRulesDropEdit";
			this.PrefRulesDropEdit.PreBoundMaxLength = 4;
			this.PrefRulesDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.PrefRulesDropEdit.TabIndex = 24;
			// 
			// ClientAUSOriginPreferenceMappingForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(456, 310, true);
			this.Controls.Add(this.PrefRulesDropEdit);
			this.Controls.Add(this.PrefSchemeDropEdit);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.PrefOriginFindBox);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.OriginFindBox);
			this.Controls.Add(this.SupplierFindBox);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ImporterFindBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.zLabel7);
			this.DataSourceAssemblyName = "ZClientAUS";
			this.DataSourceType = typeof(Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping);
			this.DataSourceTypeName = "Enterprise.Client.AUS.Business.ClientAUSOriginPreferenceMapping";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(464, 344, true);
			this.Name = "ClientAUSOriginPreferenceMappingForm";
			this.Controls.SetChildIndex(this.zLabel7, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.ImporterFindBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.SupplierFindBox, 0);
			this.Controls.SetChildIndex(this.OriginFindBox, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.PrefOriginFindBox, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.zLabel5, 0);
			this.Controls.SetChildIndex(this.PrefSchemeDropEdit, 0);
			this.Controls.SetChildIndex(this.PrefRulesDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
