using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI
{
	public sealed partial class CreateDeclarationForm
	{
		new void InitializeComponent()
		{
			this.CustomsOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DeclarationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CPCDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.DeclarantsRefTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomsDeadlineEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CustomsOfficeFindBox.SuspendLayout();
			this.DeclarationTypeDropEdit.SuspendLayout();
			this.CPCDropEdit.SuspendLayout();
			this.CustomsDeadlineEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 151, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CreateDeclarationBizObj);
			// 
			// CustomsOfficeFindBox
			// 
			this.CustomsOfficeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsOfficeFindBox, "CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CreateDeclarationBizObj)(null)).CustomsOffice)));
			this.CustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 37, true);
			this.CustomsOfficeFindBox.Name = "CustomsOfficeFindBox";
			this.CustomsOfficeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsOfficeFindBox.ParentType = null;
			this.CustomsOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.CustomsOfficeFindBox.TabIndex = 2;
			// 
			// DeclarationTypeDropEdit
			// 
			this.DeclarationTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DeclarationTypeDropEdit, "DeclarationType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CreateDeclarationBizObj)(null)).DeclarationType)));
			this.DeclarationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 61, true);
			this.DeclarationTypeDropEdit.Name = "DeclarationTypeDropEdit";
			this.DeclarationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.DeclarationTypeDropEdit.TabIndex = 3;
			// 
			// CPCDropEdit
			// 
			this.CPCDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCDropEdit, "CPC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CreateDeclarationBizObj)(null)).CPC)));
			this.CPCDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 88, true);
			this.CPCDropEdit.Name = "CPCDropEdit";
			this.CPCDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.CPCDropEdit.TabIndex = 4;
			// 
			// OkButton
			// 
			this.OkButton.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("34E9FD8A-2A22-4773-B489-A02C4408B951", "&OK");
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(230, 146, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.OkButton.TabIndex = 6;
			this.OkButton.ToolTipCaption = null;
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.okButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("11E93682-880E-40D7-8381-676E4E647278", "&Cancel");
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(283, 146, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 23, true);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
			// 
			// DeclarantsRefTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarantsRefTextBox, "DeclarantsReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CreateDeclarationBizObj)(null)).DeclarantsReference)));
			this.DeclarantsRefTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 11, true);
			this.DeclarantsRefTextBox.Name = "DeclarantsRefTextBox";
			this.DeclarantsRefTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.DeclarantsRefTextBox.TabIndex = 1;
			// 
			// CustomsDeadlineEdit
			// 
			this.CustomsDeadlineEdit.AllowDrop = true;
			this.CustomsDeadlineEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.CustomsDeadlineEdit, "CustomsDeadline");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.CreateDeclarationBizObj)(null)).CustomsDeadline)));
			this.CustomsDeadlineEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 115, true);
			this.CustomsDeadlineEdit.Name = "CustomsDeadlineEdit";
			this.CustomsDeadlineEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 15, true);
			this.cancelButton.TabIndex = 5;
			// 
			// CreateDeclarationForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("273A9B8A-8721-4B7B-AE49-F07EB5782C5C", "Create Declaration(s)");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 202, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.CustomsOfficeFindBox);
			this.Controls.Add(this.DeclarantsRefTextBox);
			this.Controls.Add(this.DeclarationTypeDropEdit);
			this.Controls.Add(this.CPCDropEdit);
			this.Controls.Add(this.CustomsDeadlineEdit);
			this.DataSourceType = typeof(Enterprise.Customs.DE.Business.CreateDeclarationBizObj);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Name = "CreateDeclarationForm";
			this.Controls.SetChildIndex(this.CPCDropEdit, 0);
			this.Controls.SetChildIndex(this.DeclarationTypeDropEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.DeclarantsRefTextBox, 0);
			this.Controls.SetChildIndex(this.CustomsOfficeFindBox, 0);
			this.Controls.SetChildIndex(this.CustomsDeadlineEdit, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CustomsOfficeFindBox.ResumeLayout(true);
			this.CustomsOfficeFindBox.PerformLayout();
			this.DeclarationTypeDropEdit.ResumeLayout(true);
			this.DeclarationTypeDropEdit.PerformLayout();
			this.CPCDropEdit.ResumeLayout(true);
			this.CPCDropEdit.PerformLayout();
			this.CustomsDeadlineEdit.ResumeLayout(true);
			this.CustomsDeadlineEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		internal ZCodeFindBox CustomsOfficeFindBox;
		internal ZDropEdit DeclarationTypeDropEdit;
		internal ZDropEdit CPCDropEdit;
		internal ZDateEdit CustomsDeadlineEdit;
		internal ZTextBox DeclarantsRefTextBox;
		internal ZButton OkButton;
		ZButton cancelButton;
	}
}
