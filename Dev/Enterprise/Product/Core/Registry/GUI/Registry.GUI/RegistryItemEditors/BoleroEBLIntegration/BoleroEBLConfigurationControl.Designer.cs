using Enterprise.ZArchitecture;

namespace Enterprise.Registry.GUI
{
	partial class BoleroEBLConfigurationControl : RegistryBusinessObjectTemplateZUserControl
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
			this.OptionGroupBox = new CargoWise.Windows.UI.KGroupBox();
			this.NoRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.YesRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.GalileoProdTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GalileoProdLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GalileoTestTitleLabel = new Enterprise.ZArchitecture.ZLabel();
			this.GalileoTestLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UrlTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GalileoAudienceBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UrlTestTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GalileoTestAudienceBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.BoleroEBLConfiguration);
			// 
			// OptionGroupBox
			// 
			this.OptionGroupBox.Controls.Add(this.NoRadioButton);
			this.OptionGroupBox.Controls.Add(this.YesRadioButton);
			this.OptionGroupBox.Font = new System.Drawing.Font("Tahoma", 8F, System.Drawing.FontStyle.Bold);
			this.OptionGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionGroupBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 3, 3, true);
			this.OptionGroupBox.Name = "OptionGroupBox";
			this.OptionGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 53, true);
			this.OptionGroupBox.TabIndex = 1;
			this.OptionGroupBox.TabStop = false;
			// 
			// NoRadioButton
			// 
			this.NoRadioButton.AutoCheck = false;
			this.NoRadioButton.AutoSize = true;
			this.NoRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.NoRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("632B6072-1E1B-453B-8284-991F4A3CB55D", "No");
			this.NoRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(81, 13, true);
			this.NoRadioButton.Name = "NoRadioButton";
			this.NoRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(37, 15, true);
			this.NoRadioButton.TabIndex = 2;
			this.NoRadioButton.TabStop = true;
			this.NoRadioButton.UseVisualStyleBackColor = false;
			// 
			// YesRadioButton
			// 
			this.YesRadioButton.AutoCheck = false;
			this.YesRadioButton.AutoSize = true;
			this.YesRadioButton.BackColor = System.Drawing.Color.Transparent;
			this.BindingSource.SetBindingMember(this.YesRadioButton, "EnableEBLIntegration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.BoleroEBLConfiguration)(null)).EnableEBLIntegration)));
			this.YesRadioButton.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ECDD2028-F46C-4823-9CA0-3C40891413FC", "Yes");
			this.YesRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 13, true);
			this.YesRadioButton.Name = "YesRadioButton";
			this.YesRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 15, true);
			this.YesRadioButton.TabIndex = 1;
			this.YesRadioButton.TabStop = true;
			this.YesRadioButton.UseVisualStyleBackColor = false;
			// 
			// GalileoProdTitleLabel
			// 
			this.GalileoProdTitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)(((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)
			| Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.GalileoProdTitleLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("86E041C2-F583-4C6A-89F5-1036ACF4B697", "Galileo Production endpoint and Audience");
			this.GalileoProdTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 70, true);
			this.GalileoProdTitleLabel.Name = "GalileoProdTitleLabel";
			this.GalileoProdTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 15, true);
			this.GalileoProdTitleLabel.TabIndex = 2;
			this.GalileoProdTitleLabel.UseMnemonic = false;
			// 
			// GalileoProdLabel
			// 
			this.GalileoProdLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.GalileoProdLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("28761132-961A-4383-8F7A-1D8E39285C17", "To perform single sign-on from the CargoWise production client environment to the Galileo production server.");
			this.GalileoProdLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 90, true);
			this.GalileoProdLabel.Name = "GalileoProdLabel";
			this.GalileoProdLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 30, true);
			this.GalileoProdLabel.TabIndex = 2;
			this.GalileoProdLabel.UseMnemonic = false;
			// 
			// UrlTextBox
			// 
			this.BindingSource.SetBindingMember(this.UrlTextBox, "GalileoEndPointUrl");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BoleroEBLConfiguration)(null)).GalileoEndPointUrl)));
			this.UrlTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UrlTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("E5F9B082-77E1-40E5-B84B-4F747B8C7651", "endpoint URL");
			this.UrlTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 130, true);
			this.UrlTextBox.Name = "UrlTextBox";
			this.UrlTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.UrlTextBox.TabIndex = 2;
			// 
			// GalileoAudienceBox
			// 
			this.BindingSource.SetBindingMember(this.GalileoAudienceBox, "GalileoAudience");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BoleroEBLConfiguration)(null)).GalileoAudience)));
			this.GalileoAudienceBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8F2F575F-4AFD-441C-8316-3770C781380B", "audience");
			this.GalileoAudienceBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GalileoAudienceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 153, true);
			this.GalileoAudienceBox.Name = "GalileoAudienceBox";
			this.GalileoAudienceBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GalileoAudienceBox.TabIndex = 2;
			// 
			// GalileoTestTitleLabel
			//             
			this.GalileoTestTitleLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)(((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)
			| Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.GalileoTestTitleLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("64303682-A4B4-46E9-AA1A-25C28AE9B5C5", "Galileo Test endpoint and Audience");
			this.GalileoTestTitleLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 200, true);
			this.GalileoTestTitleLabel.Name = "GalileoTestTitleLabel";
			this.GalileoTestTitleLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 15, true);
			this.GalileoTestTitleLabel.TabIndex = 2;
			this.GalileoTestTitleLabel.UseMnemonic = false;
			// 
			// GalileoTestLabel
			//             
			this.GalileoTestLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.GalileoTestLabel.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("78c8c1b0-a8c5-4af8-8e49-df61d5e404fa", "To perform single sign-on from the CargoWise test client environment to the Galileo test server.");
			this.GalileoTestLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(9, 220, true);
			this.GalileoTestLabel.Name = "GalileoTestLabel";
			this.GalileoTestLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 30, true);
			this.GalileoTestLabel.TabIndex = 2;
			this.GalileoTestLabel.UseMnemonic = false;
			// 
			// UrlTestTextBox
			// 
			this.BindingSource.SetBindingMember(this.UrlTestTextBox, "GalileoTestEndPointUrl");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BoleroEBLConfiguration)(null)).GalileoTestEndPointUrl)));
			this.UrlTestTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("E5F9B082-77E1-40E5-B84B-4F747B8C7165", "endpoint URL");
			this.UrlTestTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UrlTestTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 265, true);
			this.UrlTestTextBox.Name = "UrlTestTextBox";
			this.UrlTestTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.UrlTestTextBox.TabIndex = 2;
			// 
			// GalileoTestAudienceBox
			// 
			this.BindingSource.SetBindingMember(this.GalileoTestAudienceBox, "GalileoTestAudience");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.BoleroEBLConfiguration)(null)).GalileoTestAudience)));
			this.GalileoTestAudienceBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8F2F575F-4AFD-441C-8316-3770C7813B80", "audience");
			this.GalileoTestAudienceBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GalileoTestAudienceBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 289, true);
			this.GalileoTestAudienceBox.Name = "GalileoTestAudienceBox";
			this.GalileoTestAudienceBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.GalileoTestAudienceBox.TabIndex = 2;
			// 
			// BoleroEBLConfigurationControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.OptionGroupBox);
			this.Controls.Add(this.UrlTextBox);
			this.Controls.Add(this.GalileoAudienceBox);
			this.Controls.Add(this.UrlTestTextBox);
			this.Controls.Add(this.GalileoTestAudienceBox);
			this.Controls.Add(this.GalileoProdTitleLabel);
			this.Controls.Add(this.GalileoProdLabel);
			this.Controls.Add(this.GalileoTestTitleLabel);
			this.Controls.Add(this.GalileoTestLabel);
			this.Name = "BoleroEBLConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 310, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionGroupBox.ResumeLayout(false);
			this.OptionGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private CargoWise.Windows.UI.KGroupBox OptionGroupBox;
		private Enterprise.ZArchitecture.GUI.ZRadioButton NoRadioButton;
		private Enterprise.ZArchitecture.GUI.ZRadioButton YesRadioButton;
		private ZArchitecture.ZTextBox UrlTextBox;
		private ZArchitecture.ZTextBox GalileoAudienceBox;
		private ZArchitecture.ZTextBox UrlTestTextBox;
		private ZArchitecture.ZTextBox GalileoTestAudienceBox;
		private ZArchitecture.ZLabel GalileoProdTitleLabel;
		private ZArchitecture.ZLabel GalileoProdLabel;
		private ZArchitecture.ZLabel GalileoTestTitleLabel;
		private ZArchitecture.ZLabel GalileoTestLabel;
	}
}
