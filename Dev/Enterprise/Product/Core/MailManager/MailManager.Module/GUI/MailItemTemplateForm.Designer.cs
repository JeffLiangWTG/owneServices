using ExternalMailManager = MailManager.Module;

namespace Enterprise.MailManager.GUI
{
	partial class MailItemTemplateForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TemplateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.BodyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.BodyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LanguageDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CompanyFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.BranchFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DepartmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TemplateDropEdit.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			this.BodyGroupBox.SuspendLayout();
			this.LanguageDropEdit.SuspendLayout();
			this.CompanyFindBox.SuspendLayout();
			this.BranchFindBox.SuspendLayout();
			this.DepartmentFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 347, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MailManager.Business.MailItemTemplate);
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "MIT_Name");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItemTemplate)(null)).MIT_Name)));
			this.CodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CodeTextBox.EnableValidStateColor = false;
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 12, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.CodeTextBox.TabIndex = 1;
			// 
			// TemplateDropEdit
			// 
			this.TemplateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TemplateDropEdit, "MIT_Category");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MailManager.Business.MailItemTemplate)(null)).MIT_Category)));
			this.TemplateDropEdit.CaptionResourceString = ExternalMailManager.Res.GetData("2207b932-cbf4-439d-8366-ddb6504fc86c", "Category");
			this.TemplateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 38, true);
			this.TemplateDropEdit.Name = "TemplateDropEdit";
			this.TemplateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(228, 20, true);
			this.TemplateDropEdit.TabIndex = 2;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 316, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.TabIndex = 4;
			// 
			// BodyGroupBox
			// 
			this.BodyGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
			| System.Windows.Forms.AnchorStyles.Left) 
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BodyGroupBox.CaptionResourceString = ExternalMailManager.Res.GetData("d1071bf4-90f1-4c9f-965c-737e8addba16", "Body");
			this.BodyGroupBox.Controls.Add(this.BodyTextBox);
			this.BodyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 90, true);
			this.BodyGroupBox.Name = "BodyGroupBox";
			this.BodyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(688, 220, true);
			this.BodyGroupBox.TabIndex = 3;
			this.BodyGroupBox.TabStop = false;
			// 
			// BodyTextBox
			// 
			this.BindingSource.SetBindingMember(this.BodyTextBox, "MIT_Body");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MailManager.Business.MailItemTemplate)(null)).MIT_Body)));
			this.BodyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.BodyTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BodyTextBox.EnableValidStateColor = false;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.BodyTextBox, false);
			this.BodyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.BodyTextBox.Multiline = true;
			this.BodyTextBox.Name = "BodyTextBox";
			this.BodyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(682, 201, true);
			this.BodyTextBox.TabIndex = 4;
			// 
			// LanguageDropEdit
			// 
			this.LanguageDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LanguageDropEdit, "MIT_Language");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MailManager.Business.MailItemTemplate)(null)).MIT_Language)));
			this.LanguageDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 64, true);
			this.LanguageDropEdit.Name = "LanguageDropEdit";
			this.LanguageDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.LanguageDropEdit.TabIndex = 5;
			// 
			// CompanyFindBox
			// 
			this.CompanyFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CompanyFindBox, "MIT_GC_Company");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MailManager.Business.MailItemTemplate)(null)).MIT_GC_Company)));
			this.CompanyFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 12, true);
			this.CompanyFindBox.Name = "CompanyFindBox";
			this.CompanyFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.CompanyFindBox.TabIndex = 23;
			// 
			// BranchFindBox
			// 
			this.BranchFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchFindBox, "MIT_GB_Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MailManager.Business.MailItemTemplate)(null)).MIT_GB_Branch)));
			this.BranchFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 38, true);
			this.BranchFindBox.Name = "BranchFindBox";
			this.BranchFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.BranchFindBox.TabIndex = 24;
			// 
			// DepartmentFindBox
			// 
			this.DepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DepartmentFindBox, "MIT_GE_Department");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MailManager.Business.MailItemTemplate)(null)).MIT_GE_Department)));
			this.DepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(404, 64, true);
			this.DepartmentFindBox.Name = "DepartmentFindBox";
			this.DepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(293, 20, true);
			this.DepartmentFindBox.TabIndex = 25;
			// 
			// MailItemTemplateForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = ExternalMailManager.Res.GetData("d4284e12-ac01-4440-9828-ff35d7089fbd", "Template");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(712, 371, true);
			this.Controls.Add(this.DepartmentFindBox);
			this.Controls.Add(this.BranchFindBox);
			this.Controls.Add(this.CompanyFindBox);
			this.Controls.Add(this.LanguageDropEdit);
			this.Controls.Add(this.BodyGroupBox);
			this.Controls.Add(this.PostingButtonsUserControl);
			this.Controls.Add(this.TemplateDropEdit);
			this.Controls.Add(this.CodeTextBox);
			this.DataSourceType = typeof(Enterprise.MailManager.Business.MailItemTemplate);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 410, true);
			this.Name = "MailItemTemplateForm";
			this.Controls.SetChildIndex(this.CodeTextBox, 0);
			this.Controls.SetChildIndex(this.TemplateDropEdit, 0);
			this.Controls.SetChildIndex(this.PostingButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.BodyGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LanguageDropEdit, 0);
			this.Controls.SetChildIndex(this.CompanyFindBox, 0);
			this.Controls.SetChildIndex(this.BranchFindBox, 0);
			this.Controls.SetChildIndex(this.DepartmentFindBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TemplateDropEdit.ResumeLayout(true);
			this.TemplateDropEdit.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.BodyGroupBox.ResumeLayout(false);
			this.BodyGroupBox.PerformLayout();
			this.LanguageDropEdit.ResumeLayout(true);
			this.LanguageDropEdit.PerformLayout();
			this.CompanyFindBox.ResumeLayout(true);
			this.CompanyFindBox.PerformLayout();
			this.BranchFindBox.ResumeLayout(true);
			this.BranchFindBox.PerformLayout();
			this.DepartmentFindBox.ResumeLayout(true);
			this.DepartmentFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox CodeTextBox;
		private ZArchitecture.GUI.ZDropEdit TemplateDropEdit;
		private Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private ZArchitecture.GUI.ZGroupBox BodyGroupBox;
		private ZArchitecture.ZTextBox BodyTextBox;
		private ZArchitecture.GUI.ZDropEdit LanguageDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox CompanyFindBox;
		private ZArchitecture.GUI.ZGuidFindBox BranchFindBox;
		private ZArchitecture.GUI.ZGuidFindBox DepartmentFindBox;
	}
}
