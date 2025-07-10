namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	partial class ReopenPeriodKeyGeneratorForm
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
		
		protected new void InitializeComponent()
		{
			this.UserStaffCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PeriodTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.KeyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GenerateButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EnterpriseLicenceCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientCompanyDropEdit = new Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit();
			this.DatabaseDropEdit = new Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ClientCompanyDropEdit.SuspendLayout();
			this.DatabaseDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 272, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.ReopenPeriodKeyBusinessObject);
			// 
			// UserStaffCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.UserStaffCodeTextBox, "UserStaffCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.ReopenPeriodKeyBusinessObject)(null)).UserStaffCode)));
			this.UserStaffCodeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ReopenPeriodKeyGeneratorForm|98c07e9b-c57a-4b36-9152-4fae191a6b25", "User Staff Code");
			this.UserStaffCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 97, true);
			this.UserStaffCodeTextBox.Name = "UserStaffCodeTextBox";
			this.UserStaffCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.UserStaffCodeTextBox.TabIndex = 4;
			// 
			// PeriodTextBox
			// 
			this.BindingSource.SetBindingMember(this.PeriodTextBox, "Period");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.ReopenPeriodKeyBusinessObject)(null)).Period)));
			this.PeriodTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ReopenPeriodKeyGeneratorForm|b26aebbf-d1d5-4383-9f6d-9081a8040200", "Period");
			this.PeriodTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 123, true);
			this.PeriodTextBox.Name = "PeriodTextBox";
			this.PeriodTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.PeriodTextBox.TabIndex = 5;
			// 
			// KeyTextBox
			// 
			this.BindingSource.SetBindingMember(this.KeyTextBox, "KeyOutput");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.ReopenPeriodKeyBusinessObject)(null)).KeyOutput)));
			this.KeyTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ReopenPeriodKeyGeneratorForm|d974c1fe-499c-481a-978e-b9ad2a4a62c8", "Key Output");
			this.KeyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.KeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 147, true);
			this.KeyTextBox.Multiline = true;
			this.KeyTextBox.Name = "KeyTextBox";
			this.KeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 100, true);
			this.KeyTextBox.TabIndex = 6;
			// 
			// GenerateButton
			// 
			this.GenerateButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ReopenPeriodKeyGeneratorForm|bb3109c3-5832-460d-893a-e025fd989dbc", "Generate Key");
			this.GenerateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 17, true);
			this.GenerateButton.Name = "GenerateButton";
			this.GenerateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(99, 23, true);
			this.GenerateButton.TabIndex = 7;
			this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
			// 
			// EnterpriseLicenceCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.EnterpriseLicenceCodeTextBox, "EnterpriseLicenceCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.ReopenPeriodKeyBusinessObject)(null)).EnterpriseLicenceCode)));
			this.EnterpriseLicenceCodeTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ReopenPeriodKeyGeneratorForm|f7c55244-c5a6-494d-adef-d6a91828abd5", "Enterprise License Code");
			this.EnterpriseLicenceCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 20, true);
			this.EnterpriseLicenceCodeTextBox.Name = "EnterpriseLicenceCodeTextBox";
			this.EnterpriseLicenceCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.EnterpriseLicenceCodeTextBox.TabIndex = 0;
			this.EnterpriseLicenceCodeTextBox.TabStop = false;
			// 
			// ClientCompanyDropEdit
			// 
			this.ClientCompanyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClientCompanyDropEdit, "ClientCompanyCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.ReopenPeriodKeyBusinessObject)(null)).ClientCompanyCode)));
			this.ClientCompanyDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("18e671f4-8500-40f4-b6b6-ebbb543cece3", "DB Company");
			this.ClientCompanyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 73, true);
			this.ClientCompanyDropEdit.Name = "ClientCompanyDropEdit";
			this.ClientCompanyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(374, 18, true);
			this.ClientCompanyDropEdit.TabIndex = 3;
			// 
			// DatabaseDropEdit
			// 
			this.DatabaseDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DatabaseDropEdit, "DatabaseServerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.ReopenPeriodKeyBusinessObject)(null)).DatabaseServerCode)));
			this.DatabaseDropEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b524dd40-ae9f-491d-b690-44b0d19293d9", "Database");
			this.DatabaseDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 47, true);
			this.DatabaseDropEdit.Name = "DatabaseDropEdit";
			this.DatabaseDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(373, 18, true);
			this.DatabaseDropEdit.TabIndex = 2;
			// 
			// ReopenPeriodKeyGeneratorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ReopenPeriodKeyGeneratorForm|b2ee9ca3-b422-44c9-bc56-dfbdbaedc59b", "Reopen Period Key Generator");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 296, true);
			this.Controls.Add(this.ClientCompanyDropEdit);
			this.Controls.Add(this.DatabaseDropEdit);
			this.Controls.Add(this.KeyTextBox);
			this.Controls.Add(this.PeriodTextBox);
			this.Controls.Add(this.EnterpriseLicenceCodeTextBox);
			this.Controls.Add(this.UserStaffCodeTextBox);
			this.Controls.Add(this.GenerateButton);
			this.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.ReopenPeriodKeyBusinessObject);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 333, true);
			this.Name = "ReopenPeriodKeyGeneratorForm";
			this.Text = "ReopenPeriodKeyGeneratorForm";
			this.Controls.SetChildIndex(this.GenerateButton, 0);
			this.Controls.SetChildIndex(this.UserStaffCodeTextBox, 0);
			this.Controls.SetChildIndex(this.EnterpriseLicenceCodeTextBox, 0);
			this.Controls.SetChildIndex(this.PeriodTextBox, 0);
			this.Controls.SetChildIndex(this.KeyTextBox, 0);
			this.Controls.SetChildIndex(this.DatabaseDropEdit, 0);
			this.Controls.SetChildIndex(this.ClientCompanyDropEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ClientCompanyDropEdit.ResumeLayout(true);
			this.ClientCompanyDropEdit.PerformLayout();
			this.DatabaseDropEdit.ResumeLayout(true);
			this.DatabaseDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZTextBox UserStaffCodeTextBox;
		private Enterprise.ZArchitecture.ZTextBox PeriodTextBox;
		private Enterprise.ZArchitecture.ZTextBox KeyTextBox;
		private Enterprise.ZArchitecture.GUI.ZButton GenerateButton;
		private Enterprise.ZArchitecture.ZTextBox EnterpriseLicenceCodeTextBox;
		private Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit ClientCompanyDropEdit;
		private Enterprise.Client.EDI.Billing.GUI.CategoryDropEdit DatabaseDropEdit;
	}
}
