namespace Enterprise.Customs.EU.GUI
{
	partial class VATDetailControl
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
			this.VATDetailRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.VATDetailLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.GuidedDecisionMakingVAT);
			// 
			// VATDetailRadioButton
			// 
			this.VATDetailRadioButton.AutoCheck = false;
			this.VATDetailRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.VATDetailRadioButton, "IsTicked");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.GuidedDecisionMakingVAT)(null)).IsTicked)));
			this.VATDetailRadioButton.Dock = System.Windows.Forms.DockStyle.Left;
			this.VATDetailRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.VATDetailRadioButton.Name = "VATDetailRadioButton";
			this.VATDetailRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 74, true);
			this.VATDetailRadioButton.TabIndex = 0;
			this.VATDetailRadioButton.TabStop = true;
			this.VATDetailRadioButton.UseVisualStyleBackColor = true;
			// 
			// VATDetailLabel
			// 
			this.VATDetailLabel.AutoSize = true;
			this.VATDetailLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.VATDetailLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 4, true);
			this.VATDetailLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 3, 2, 3, true);
			this.VATDetailLabel.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 0, true);
			this.VATDetailLabel.Name = "VATDetailLabel";
			this.VATDetailLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.VATDetailLabel.TabIndex = 1;
			// 
			// VATDetailControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.AutoSize = true;
			this.Controls.Add(this.VATDetailLabel);
			this.Controls.Add(this.VATDetailRadioButton);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(927, 0, true);
			this.Name = "VATDetailControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(794, 74, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZRadioButton VATDetailRadioButton;
		private ZArchitecture.ZLabel VATDetailLabel;
	}
}
