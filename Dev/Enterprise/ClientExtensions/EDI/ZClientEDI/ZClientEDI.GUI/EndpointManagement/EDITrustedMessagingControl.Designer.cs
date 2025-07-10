namespace Enterprise.Client.EDI.EndpointManagement.GUI
{
	partial class EDITrustedMessagingControl
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
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SystemIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TrustedSystemFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.HasSecretKeyCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ProductBox = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainPanel.SuspendLayout();
			this.TrustedSystemFindBox.SuspendLayout();
			this.ProductBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase);
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.SystemIDTextBox);
			this.MainPanel.Controls.Add(this.TrustedSystemFindBox);
			this.MainPanel.Controls.Add(this.HasSecretKeyCheckBox);
			this.MainPanel.Controls.Add(this.ProductBox);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 170, true);
			this.MainPanel.TabIndex = 1;
			// 
			// SystemIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.SystemIDTextBox, "TrustedSystem+ETS_SystemID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).TrustedSystem.ETS_SystemID)));
			this.SystemIDTextBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("364f98f1-4e5f-4951-8744-e6da162b4c7d", "System ID");
			this.SystemIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SystemIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 56, true);
			this.SystemIDTextBox.Name = "SystemIDTextBox";
			this.SystemIDTextBox.ReadOnly = true;
			this.SystemIDTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.SystemIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.SystemIDTextBox.TabIndex = 2;
			// 
			// TrustedSystemFindBox
			// 
			this.TrustedSystemFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TrustedSystemFindBox, "LD_ETS_TrustedSystem");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ETS_TrustedSystem)));
			this.TrustedSystemFindBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9e2cf36d-96c2-4853-b7f7-50b74cf0682b", "Trusted System");
			this.TrustedSystemFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 16, true);
			this.TrustedSystemFindBox.Name = "TrustedSystemFindBox";
			this.TrustedSystemFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.TrustedSystemFindBox.ParentType = null;
			this.TrustedSystemFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(349, 20, true);
			this.TrustedSystemFindBox.TabIndex = 1;
			// 
			// HasSecretKeyCheckBox
			// 
			this.HasSecretKeyCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.HasSecretKeyCheckBox, "TrustedSystem+HasSecretKey");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).TrustedSystem.HasSecretKey)));
			this.HasSecretKeyCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.HasSecretKeyCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.HasSecretKeyCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 98, true);
			this.HasSecretKeyCheckBox.Name = "HasSecretKeyCheckBox";
			this.HasSecretKeyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.HasSecretKeyCheckBox.TabIndex = 4;
			this.HasSecretKeyCheckBox.Text = "Has Secret Key";
			this.HasSecretKeyCheckBox.UseVisualStyleBackColor = true;
			// 
			// ProductBox
			// 
			this.ProductBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ProductBox, "TrustedSystem+ETS_Product");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).TrustedSystem.ETS_Product)));
			this.ProductBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("81887dcf-a9cb-4684-90cc-2915068f9ef8", "Product");
			this.ProductBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(106, 96, true);
			this.ProductBox.Name = "ProductBox";
			this.ProductBox.PreBoundMaxLength = 2;
			this.ProductBox.ShouldResizeByMaxLength = true;
			this.ProductBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(243, 20, true);
			this.ProductBox.ReadOnly = true;
			this.ProductBox.TabIndex = 3;
			// 
			// EDITrustedMessagingControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "EDITrustedMessagingControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(505, 170, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.TrustedSystemFindBox.ResumeLayout(true);
			this.TrustedSystemFindBox.PerformLayout();
			this.ProductBox.ResumeLayout(true);
			this.ProductBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZCheckBox HasSecretKeyCheckBox;
		private ZArchitecture.GUI.ZDropEdit ProductBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox TrustedSystemFindBox;
		private ZArchitecture.ZTextBox SystemIDTextBox;
	}
}
