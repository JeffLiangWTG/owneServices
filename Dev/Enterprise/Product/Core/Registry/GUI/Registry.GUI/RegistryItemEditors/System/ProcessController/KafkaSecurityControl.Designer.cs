
using Enterprise.Registry.Business.eServices.HealthCheckSettings;

namespace Enterprise.Registry.GUI
{
	partial class KafkaSecurityControl
	{
		void InitializeComponent()
		{
			this.protocolDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.UserNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SslCaLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.protocolDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.KafkaSecurity);
			// 
			// protocolDropEdit
			// 
			this.protocolDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.protocolDropEdit, "SecurityProtocol");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Registry.Business.KafkaSecurity)(null)).SecurityProtocol)));
			this.protocolDropEdit.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("B40EA0D6-46ED-431D-A029-001D0671E95E", "Protocol");
			this.protocolDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 12, true);
			this.protocolDropEdit.Name = "protocolDropEdit";
			this.protocolDropEdit.PreBoundMaxLength = 4;
			this.protocolDropEdit.ShouldResizeByMaxLength = true;
			this.protocolDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 18, true);
			this.protocolDropEdit.TabIndex = 0;
			// 
			// PasswordTextBox
			// 
			this.BindingSource.SetBindingMember(this.PasswordTextBox, "SaslPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.KafkaSecurity)(null)).SaslPassword)));
			this.PasswordTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AE46A468-4616-4C7A-825F-A481F5B1FF58", "SASL Password");
			this.PasswordTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 90, true);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.PasswordChar = '*';
			this.PasswordTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.PasswordTextBox.TabIndex = 11;
			// 
			// UserNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.UserNameTextBox, "SaslUsername");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.KafkaSecurity)(null)).SaslUsername)));
			this.UserNameTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("AEEC8E82-3430-4B69-AB4A-480EF33D2D26", "SASL Username");
			this.UserNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 54, true);
			this.UserNameTextBox.Name = "UserNameTextBox";
			this.UserNameTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.UserNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.UserNameTextBox.TabIndex = 9;
			// 
			// SslCaLocationTextBox
			// 
			this.BindingSource.SetBindingMember(this.SslCaLocationTextBox, "SslCaLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.KafkaSecurity)(null)).SslCaLocation)));
			this.SslCaLocationTextBox.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("ea80e2ec-ad97-43ca-862b-98a8b026373b", "SSL certificate Location");
			this.SslCaLocationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SslCaLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 125, true);
			this.SslCaLocationTextBox.Name = "SslCaLocationTextBox";
			this.SslCaLocationTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.SslCaLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 18, true);
			this.SslCaLocationTextBox.TabIndex = 12;
			this.SslCaLocationTextBox.Visible = false;
			// 
			// KafkaSecurityControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SslCaLocationTextBox);
			this.Controls.Add(this.UserNameTextBox);
			this.Controls.Add(this.PasswordTextBox);
			this.Controls.Add(this.protocolDropEdit);
			this.Name = "KafkaSecurityControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 173, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.protocolDropEdit.ResumeLayout(true);
			this.protocolDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private Enterprise.ZArchitecture.GUI.ZDropEdit protocolDropEdit;
		private Enterprise.ZArchitecture.ZTextBox UserNameTextBox;
		private Enterprise.ZArchitecture.ZTextBox PasswordTextBox;
		private ZArchitecture.ZTextBox SslCaLocationTextBox;
	}
}
