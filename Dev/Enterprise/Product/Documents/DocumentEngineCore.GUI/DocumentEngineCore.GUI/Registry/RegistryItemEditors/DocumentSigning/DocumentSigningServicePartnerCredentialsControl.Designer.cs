using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngineCore.Registry.GUI
{
	partial class DocumentSigningServicePartnerCredentialsControl
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
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PartnerAccessKeyTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PartnerIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngineCore.Registry.DocumentSigningServicePartnerCredentials);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.CaptionResourceString = Enterprise.DocumentEngineCore.GUI.Res.GetData("2204bcec-babe-4c3f-8717-2eb6f1912c05", "Partner Credentials");
			this.MainGroupBox.Controls.Add(this.PartnerAccessKeyTextBox);
			this.MainGroupBox.Controls.Add(this.PartnerIDTextBox);
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 86, true);
			this.MainGroupBox.TabIndex = 3;
			this.MainGroupBox.TabStop = false;
			// 
			// PartnerAccessKeyTextBox
			// 
			this.PartnerAccessKeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PartnerAccessKeyTextBox, "PartnerAccessKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DocumentSigningServicePartnerCredentials)(null)).PartnerAccessKey)));
			this.PartnerAccessKeyTextBox.CaptionResourceString = null;
			this.PartnerAccessKeyTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PartnerAccessKeyTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 49, true);
			this.PartnerAccessKeyTextBox.Name = "PartnerAccessKeyTextBox";
			this.PartnerAccessKeyTextBox.PasswordChar = '*';
			this.PartnerAccessKeyTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 20, true);
			this.PartnerAccessKeyTextBox.TabIndex = 2;
			// 
			// PartnerIDTextBox
			// 
			this.PartnerIDTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PartnerIDTextBox, "PartnerID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentEngineCore.Registry.DocumentSigningServicePartnerCredentials)(null)).PartnerID)));
			this.PartnerIDTextBox.CaptionResourceString = null;
			this.PartnerIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PartnerIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 23, true);
			this.PartnerIDTextBox.Name = "PartnerIDTextBox";
			this.PartnerIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(203, 20, true);
			this.PartnerIDTextBox.TabIndex = 1;
			// 
			// DocumentSigningServicePartnerCredentialsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "DocumentSigningServicePartnerCredentialsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(343, 86, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private Enterprise.ZArchitecture.GUI.ZGroupBox MainGroupBox;
		private ZArchitecture.ZTextBox PartnerAccessKeyTextBox;
		private ZArchitecture.ZTextBox PartnerIDTextBox;
	}
}
