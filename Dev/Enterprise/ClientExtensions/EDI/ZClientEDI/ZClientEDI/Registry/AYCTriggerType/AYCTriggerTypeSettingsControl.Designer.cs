
namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class AYCTriggerTypeSettingsControl
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
			this.TextBoxPrimaryChargeCode = new Enterprise.ZArchitecture.ZTextBox();
			this.TextBoxSecondaryChargeCode = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.AYCTriggerTypeSettings);
			// 
			// TextBoxPrimaryChargeCode
			//
			this.BindingSource.SetBindingMember(this.TextBoxPrimaryChargeCode, "PrimaryChargeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.AYCTriggerTypeSettings)(null)).PrimaryChargeCode)));
			this.TextBoxPrimaryChargeCode.CaptionResourceString = ZClientEDI.Res.GetData("b9430576-529c-4fc3-a702-b510eba3198a", "Primary Charge Code");
			this.TextBoxPrimaryChargeCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 20, true);
			this.TextBoxPrimaryChargeCode.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.TextBoxPrimaryChargeCode.Name = "TextBoxPrimaryChargeCode";
			this.TextBoxPrimaryChargeCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 20, true);
			this.TextBoxPrimaryChargeCode.TabIndex = 0;
			// 
			// TextBoxSecondaryChargeCode
			// 
			this.BindingSource.SetBindingMember(this.TextBoxSecondaryChargeCode, "SecondaryChargeCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Registry.Business.AYCTriggerTypeSettings)(null)).SecondaryChargeCode)));
			this.TextBoxSecondaryChargeCode.CaptionResourceString = ZClientEDI.Res.GetData("6d58aac7-483d-491c-a140-f580acd951ef", "Secondary Charge Code");
			this.TextBoxSecondaryChargeCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(150, 49, true);
			this.TextBoxSecondaryChargeCode.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.TextBoxSecondaryChargeCode.Name = "TextBoxSecondaryChargeCode";
			this.TextBoxSecondaryChargeCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(84, 20, true);
			this.TextBoxSecondaryChargeCode.TabIndex = 1;
			// 
			// AYCTriggerTypeSettingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.TextBoxSecondaryChargeCode);
			this.Controls.Add(this.TextBoxPrimaryChargeCode);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.Name = "AYCTriggerTypeSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox TextBoxPrimaryChargeCode;
		private ZArchitecture.ZTextBox TextBoxSecondaryChargeCode;
	}
}
