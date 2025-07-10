
namespace Enterprise.Customs.MX.Manifest.GUI
{
	partial class MXWsVucemRegistryItemUserControl
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
            this.WSAirModeURLTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.WSAirModeUsernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.WSAirModePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.WSSeaModeURLTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.WSSeaModeUsernameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.WSSeaModePasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Manifest.Business.MXWsVucem);
            // 
            // WSAirModeURLTextBox
            // 
            this.BindingSource.SetBindingMember(this.WSAirModeURLTextBox, "AirModeWSResponse");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.MXWsVucem)(null)).AirModeWSResponse)));
            this.WSAirModeURLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 3, true);
            this.WSAirModeURLTextBox.Name = "WSAirModeURLTextBox";
            this.WSAirModeURLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
            this.WSAirModeURLTextBox.TabIndex = 1;
            // 
            // WSAirModeUsernameTextBox
            // 
            this.BindingSource.SetBindingMember(this.WSAirModeUsernameTextBox, "AirModeWSUsername");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.MXWsVucem)(null)).AirModeWSUsername)));
            this.WSAirModeUsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 29, true);
            this.WSAirModeUsernameTextBox.Name = "WSAirModeUsernameTextBox";
            this.WSAirModeUsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
            this.WSAirModeUsernameTextBox.TabIndex = 2;
			// 
			// WSAirModePasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.WSAirModePasswordTextBox, "AirModeWSPassword");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.MXWsVucem)(null)).AirModeWSPassword)));
            this.WSAirModePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 55, true);
            this.WSAirModePasswordTextBox.Name = "WSAirModePasswordTextBox";
            this.WSAirModePasswordTextBox.PasswordChar = '*';
            this.WSAirModePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
            this.WSAirModePasswordTextBox.TabIndex = 3;
            // 
            // WSSeaModeURLTextBox
            // 
            this.BindingSource.SetBindingMember(this.WSSeaModeURLTextBox, "SeaModeWSResponse");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.MXWsVucem)(null)).SeaModeWSResponse)));
            this.WSSeaModeURLTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 81, true);
            this.WSSeaModeURLTextBox.Name = "WSSeaModeURLTextBox";
            this.WSSeaModeURLTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
            this.WSSeaModeURLTextBox.TabIndex = 4;
			this.WSSeaModeURLTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// WSSeaModeUsernameTextBox
			// 
			this.BindingSource.SetBindingMember(this.WSSeaModeUsernameTextBox, "SeaModeWSUsername");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.MXWsVucem)(null)).SeaModeWSUsername)));
            this.WSSeaModeUsernameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 107, true);
            this.WSSeaModeUsernameTextBox.Name = "WSSeaModeUsernameTextBox";
            this.WSSeaModeUsernameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
            this.WSSeaModeUsernameTextBox.TabIndex = 5;
			this.WSSeaModeUsernameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// WSSeaModePasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.WSSeaModePasswordTextBox, "SeaModeWSPassword");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.MXWsVucem)(null)).SeaModeWSPassword)));
            this.WSSeaModePasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(152, 133, true);
            this.WSSeaModePasswordTextBox.Name = "WSSeaModePasswordTextBox";
            this.WSSeaModePasswordTextBox.PasswordChar = '*';
            this.WSSeaModePasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 17, true);
            this.WSSeaModePasswordTextBox.TabIndex = 6;
			this.WSSeaModePasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// MXWsVucemRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.WSSeaModeURLTextBox);
            this.Controls.Add(this.WSSeaModeUsernameTextBox);
            this.Controls.Add(this.WSSeaModePasswordTextBox);
            this.Controls.Add(this.WSAirModeURLTextBox);
            this.Controls.Add(this.WSAirModeUsernameTextBox);
            this.Controls.Add(this.WSAirModePasswordTextBox);
            this.Name = "MXWsVucemRegistryItemUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 160, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox WSAirModeURLTextBox;
		private ZArchitecture.ZTextBox WSAirModeUsernameTextBox;
		private ZArchitecture.ZTextBox WSAirModePasswordTextBox;
		private ZArchitecture.ZTextBox WSSeaModeURLTextBox;
		private ZArchitecture.ZTextBox WSSeaModeUsernameTextBox;
		private ZArchitecture.ZTextBox WSSeaModePasswordTextBox;
	}
}
