using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.Registry.GUI
{
	partial class DocumentSigningServiceCredentialsConfigurationControl
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
			this.CredentialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.KeyIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccessKeyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClientIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ProviderCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CredentialsGroupBox.SuspendLayout();
			this.ProviderCodeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.DocumentSigningServiceCredentialsConfiguration);
			// 
			// CredentialsGroupBox
			// 
			this.CredentialsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.CredentialsGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DocumentSigningServiceCredentialsConfigurationControl|F186C0DE-8190-409C-A101-B34068AA6EE6", "Configuration Settings");
			this.CredentialsGroupBox.Controls.Add(this.ProviderCodeDropEdit);
			this.CredentialsGroupBox.Controls.Add(this.KeyIDTextBox);
			this.CredentialsGroupBox.Controls.Add(this.AccessKeyTextBox);
			this.CredentialsGroupBox.Controls.Add(this.ClientIDTextBox);
			this.CredentialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 4, true);
			this.CredentialsGroupBox.Name = "CredentialsGroupBox";
			this.CredentialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 136, true);
			this.CredentialsGroupBox.TabIndex = 3;
			this.CredentialsGroupBox.TabStop = false;
			// 
			// KeyIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.KeyIDTextBox, "KeyID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DocumentSigningServiceCredentialsConfiguration)(null)).KeyID)));
			this.KeyIDTextBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DocumentSigningServiceCredentialsConfigurationControl|316DFF0E-C66A-42A9-B79B-EB318781C890", "Key ID");
			this.KeyIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.KeyIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 95, true);
			this.KeyIDTextBox.Name = "KeyIDTextBox";
			this.KeyIDTextBox.PasswordChar = '*';
			this.KeyIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.KeyIDTextBox.TabIndex = 15;
			// 
			// AccessKeyTextBox
			// 
			this.BindingSource.SetBindingMember(this.AccessKeyTextBox, "AccessKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DocumentSigningServiceCredentialsConfiguration)(null)).AccessKey)));
			this.AccessKeyTextBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DocumentSigningServiceCredentialsConfigurationControl|413767B2-0CCD-4746-9E2A-42479CDC61C2", "Access Key");
			this.AccessKeyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AccessKeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 70, true);
			this.AccessKeyTextBox.Name = "AccessKeyTextBox";
			this.AccessKeyTextBox.PasswordChar = '*';
			this.AccessKeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.AccessKeyTextBox.TabIndex = 13;
			// 
			// ClientIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.ClientIDTextBox, "ClientID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DocumentSigningServiceCredentialsConfiguration)(null)).ClientID)));
			this.ClientIDTextBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("DocumentSigningServiceCredentialsConfigurationControl|78A7B5F9-1266-47F9-82F2-C7EA5837A35B", "Client ID");
			this.ClientIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.ClientIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 45, true);
			this.ClientIDTextBox.Name = "ClientIDTextBox";
			this.ClientIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ClientIDTextBox.TabIndex = 11;
			// 
			// ProviderCodeDropEdit
			// 
			this.ProviderCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProviderCodeDropEdit, "ProviderCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentEngineCore.Registry.DocumentSigningServiceCredentialsWithProviderConfiguration)(null)).ProviderCode)));
			this.ProviderCodeDropEdit.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("8ed5f3bb-041c-4e4d-964b-e3737df5abe0", "Provider");
			this.ProviderCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 19, true);
			this.ProviderCodeDropEdit.Name = "ProviderCodeDropEdit";
			this.ProviderCodeDropEdit.PreBoundMaxLength = 3;
			this.ProviderCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.ProviderCodeDropEdit.TabIndex = 9;
			// 
			// DocumentSigningServiceCredentialsConfigurationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CredentialsGroupBox);
			this.Name = "DocumentSigningServiceCredentialsConfigurationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(351, 140, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CredentialsGroupBox.ResumeLayout(false);
			this.CredentialsGroupBox.PerformLayout();
			this.ProviderCodeDropEdit.ResumeLayout(true);
			this.ProviderCodeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox CredentialsGroupBox;
		private Enterprise.ZArchitecture.ZTextBox ClientIDTextBox;
		private Enterprise.ZArchitecture.ZTextBox AccessKeyTextBox;
		private Enterprise.ZArchitecture.ZTextBox KeyIDTextBox;
		private ZArchitecture.GUI.ZDropEdit ProviderCodeDropEdit;
	}
}
