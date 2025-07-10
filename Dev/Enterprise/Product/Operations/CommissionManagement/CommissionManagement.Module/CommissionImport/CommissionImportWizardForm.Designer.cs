namespace Enterprise.CommissionManagement.Module
{
	partial class CommissionImportWizardForm
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
			this.OptionsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ImportTypeHintTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ImportTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MainTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionsTabPage.SuspendLayout();
			this.ImportTypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Controls.Add(this.OptionsTabPage);
			this.MainTabControl.Controls.SetChildIndex(this.OptionsTabPage, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.CommissionManagement.Business.CommissionImportWizard);
			// 
			// OptionsTabPage
			// 
			this.OptionsTabPage.CaptionResourceString = Enterprise.CommissionManagement.Module.Res.GetData("DataImportWizardForm|b42933ca-39e4-4b2d-a44a-9ac9ff095556", "Options");
			this.OptionsTabPage.Controls.Add(this.ImportTypeHintTextBox);
			this.OptionsTabPage.Controls.Add(this.ImportTypeDropEdit);
			this.OptionsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 17, true);
			this.OptionsTabPage.Name = "OptionsTabPage";
			this.OptionsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.OptionsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(623, 207, true);
			this.OptionsTabPage.TabIndex = 1;
			// 
			// ImportTypeHintTextBox
			// 
			this.BindingSource.SetBindingMember(this.ImportTypeHintTextBox, "ImportTypeHint");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.CommissionManagement.Business.CommissionImportWizard)(null)).ImportTypeHint)));
			this.ImportTypeHintTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ImportTypeHintTextBox, false);
			this.ImportTypeHintTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 34, true);
			this.ImportTypeHintTextBox.Multiline = true;
			this.ImportTypeHintTextBox.Name = "ImportTypeHintTextBox";
			this.ImportTypeHintTextBox.ReadOnly = true;
			this.ImportTypeHintTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.ImportTypeHintTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(450, 55, true);
			this.ImportTypeHintTextBox.TabIndex = 1;
			// 
			// ImportTypeDropEdit
			// 
			this.ImportTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ImportTypeDropEdit, "ImportType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.CommissionManagement.Business.CommissionImportWizard)(null)).ImportType)));
			this.ImportTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 14, true);
			this.ImportTypeDropEdit.Name = "ImportTypeDropEdit";
			this.ImportTypeDropEdit.PreBoundMaxLength = 4;
			this.ImportTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(340, 17, true);
			this.ImportTypeDropEdit.TabIndex = 0;
			// 
			// CommissionImportWizardForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionResourceString = Enterprise.CommissionManagement.Module.Res.GetData("6cee6aa6-5ce2-4f10-8e77-08d206dbe369", "Commission Import Wizard");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(829, 306, true);
			this.DataSourceType = typeof(Enterprise.CommissionManagement.Business.CommissionImportWizard);
			this.Name = "CommissionImportWizardForm";
			this.MainTabControl.ResumeLayout(false);
			this.MainTabControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionsTabPage.ResumeLayout(false);
			this.OptionsTabPage.PerformLayout();
			this.ImportTypeDropEdit.ResumeLayout(true);
			this.ImportTypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZTabPage OptionsTabPage;
		private ZArchitecture.GUI.ZDropEdit ImportTypeDropEdit;
		private ZArchitecture.ZTextBox ImportTypeHintTextBox;
	}
}