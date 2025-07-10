
namespace Enterprise.Customs.BR.GUI
{
	partial class OrganisationConsigneePlugInUserControl
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.bankCodeZTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.sbsNoZTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.accountNoZTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.BROrgImpAddInfo);
            // 
            // bankCodeZTextBox
            // 
            this.BindingSource.SetBindingMember(this.bankCodeZTextBox, "ZO_BankCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.BROrgImpAddInfo)(null)).ZO_BankCode)));
            this.bankCodeZTextBox.CaptionResourceString = null;
            this.bankCodeZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 23, true);
            this.bankCodeZTextBox.Name = "bankCodeZTextBox";
            this.bankCodeZTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.bankCodeZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
            this.bankCodeZTextBox.TabIndex = 0;
            // 
            // sbsNoZTextBox
            // 
            this.BindingSource.SetBindingMember(this.sbsNoZTextBox, "ZO_BSBNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.BROrgImpAddInfo)(null)).ZO_BSBNumber)));
            this.sbsNoZTextBox.CaptionResourceString = null;
            this.sbsNoZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 47, true);
            this.sbsNoZTextBox.Name = "sbsNoZTextBox";
            this.sbsNoZTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.sbsNoZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
            this.sbsNoZTextBox.TabIndex = 1;
            // 
            // accountNoZTextBox
            // 
            this.BindingSource.SetBindingMember(this.accountNoZTextBox, "ZO_AccountNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.BROrgImpAddInfo)(null)).ZO_AccountNumber)));
            this.accountNoZTextBox.CaptionResourceString = null;
            this.accountNoZTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 71, true);
            this.accountNoZTextBox.Name = "accountNoZTextBox";
            this.accountNoZTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.accountNoZTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 18, true);
            this.accountNoZTextBox.TabIndex = 2;
            // 
            // OrganisationConsigneePlugInUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.accountNoZTextBox);
            this.Controls.Add(this.sbsNoZTextBox);
            this.Controls.Add(this.bankCodeZTextBox);
            this.Name = "OrganisationConsigneePlugInUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(581, 277, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox bankCodeZTextBox;
		private ZArchitecture.ZTextBox sbsNoZTextBox;
		private ZArchitecture.ZTextBox accountNoZTextBox;
	}
}
