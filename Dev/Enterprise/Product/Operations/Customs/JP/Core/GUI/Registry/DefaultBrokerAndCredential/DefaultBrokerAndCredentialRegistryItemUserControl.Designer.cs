namespace Enterprise.Customs.JP.GUI
{
	partial class DefaultBrokerAndCredentialRegistryItemUserControl
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
			this.DefaultBrokerCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.DefaultCredentialSEADropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefaultCredentialAIRDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ForwarderManifestSEADropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ForwarderManifestAIRDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefaultCredentialsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DefaultBrokerCodeFindBox.SuspendLayout();
			this.DefaultCredentialSEADropEdit.SuspendLayout();
			this.DefaultCredentialAIRDropEdit.SuspendLayout();
			this.ForwarderManifestSEADropEdit.SuspendLayout();
			this.ForwarderManifestAIRDropEdit.SuspendLayout();
			this.DefaultCredentialsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Common.DefaultBrokerAndCredential);
			// 
			// DefaultBrokerCodeFindBox
			// 
			this.DefaultBrokerCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultBrokerCodeFindBox, "DefaultBrokerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.DefaultBrokerAndCredential)(null)).DefaultBrokerCode)));
			this.DefaultBrokerCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(103, 3, true);
			this.DefaultBrokerCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.DefaultBrokerCodeFindBox.Name = "DefaultBrokerCodeFindBox";
			this.DefaultBrokerCodeFindBox.PreBoundMaxLength = 3;
			this.DefaultBrokerCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(220, 20, true);
			this.DefaultBrokerCodeFindBox.TabIndex = 0;
			// 
			// DefaultCredentialSEADropEdit
			// 
			this.DefaultCredentialSEADropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultCredentialSEADropEdit, "DefaultCredentialSEA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.DefaultBrokerAndCredential)(null)).DefaultCredentialSEA)));
			this.DefaultCredentialSEADropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 29, true);
			this.DefaultCredentialSEADropEdit.Name = "DefaultCredentialSEADropEdit";
			this.DefaultCredentialSEADropEdit.ShouldResizeByMaxLength = true;
			this.DefaultCredentialSEADropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DefaultCredentialSEADropEdit.TabIndex = 1;
			// 
			// DefaultCredentialAIRDropEdit
			// 
			this.DefaultCredentialAIRDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefaultCredentialAIRDropEdit, "DefaultCredentialAIR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.DefaultBrokerAndCredential)(null)).DefaultCredentialAIR)));
			this.DefaultCredentialAIRDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 55, true);
			this.DefaultCredentialAIRDropEdit.Name = "DefaultCredentialAIRDropEdit";
			this.DefaultCredentialAIRDropEdit.ShouldResizeByMaxLength = true;
			this.DefaultCredentialAIRDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.DefaultCredentialAIRDropEdit.TabIndex = 2;
			// 
			// ForwarderManifestSEADropEdit
			// 
			this.ForwarderManifestSEADropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForwarderManifestSEADropEdit, "ForwarderManifestSEA");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.DefaultBrokerAndCredential)(null)).ForwarderManifestSEA)));
			this.ForwarderManifestSEADropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 81, true);
			this.ForwarderManifestSEADropEdit.Name = "ForwarderManifestSEADropEdit";
			this.ForwarderManifestSEADropEdit.ShouldResizeByMaxLength = true;
			this.ForwarderManifestSEADropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ForwarderManifestSEADropEdit.TabIndex = 3;
			// 
			// ForwarderManifestAIRDropEdit
			// 
			this.ForwarderManifestAIRDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForwarderManifestAIRDropEdit, "ForwarderManifestAIR");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Common.DefaultBrokerAndCredential)(null)).ForwarderManifestAIR)));
			this.ForwarderManifestAIRDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 107, true);
			this.ForwarderManifestAIRDropEdit.Name = "ForwarderManifestAIRDropEdit";
			this.ForwarderManifestAIRDropEdit.ShouldResizeByMaxLength = true;
			this.ForwarderManifestAIRDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.ForwarderManifestAIRDropEdit.TabIndex = 4;
			// 
			// DefaultCredentialsGroupBox
			// 
			this.DefaultCredentialsGroupBox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("C0446EC5-3DE8-440E-B27F-AF27FFC30BA2", "Default Credentials");
			this.DefaultCredentialsGroupBox.Controls.Add(this.DefaultCredentialAIRDropEdit);
			this.DefaultCredentialsGroupBox.Controls.Add(this.DefaultCredentialSEADropEdit);
			this.DefaultCredentialsGroupBox.Controls.Add(this.ForwarderManifestSEADropEdit);
			this.DefaultCredentialsGroupBox.Controls.Add(this.ForwarderManifestAIRDropEdit);
			this.DefaultCredentialsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(25, 40, true);
			this.DefaultCredentialsGroupBox.Name = "DefaultCredentialsGroupBox";
			this.DefaultCredentialsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(270, 150, true);
			this.DefaultCredentialsGroupBox.TabIndex = 7;
			// 
			// CusBrokerStaffRegistryItemUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DefaultBrokerCodeFindBox);
			this.Controls.Add(this.DefaultCredentialsGroupBox);
			this.Name = "DefaultBrokerAndCredentialRegistryItemUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(360, 450, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DefaultBrokerCodeFindBox.ResumeLayout(true);
			this.DefaultBrokerCodeFindBox.PerformLayout();
			this.DefaultCredentialSEADropEdit.ResumeLayout(true);
			this.DefaultCredentialSEADropEdit.PerformLayout();
			this.DefaultCredentialAIRDropEdit.ResumeLayout(true);
			this.DefaultCredentialAIRDropEdit.PerformLayout();
			this.ForwarderManifestSEADropEdit.ResumeLayout(true);
			this.ForwarderManifestSEADropEdit.PerformLayout();
			this.ForwarderManifestAIRDropEdit.ResumeLayout(true);
			this.ForwarderManifestAIRDropEdit.PerformLayout();
			this.DefaultCredentialsGroupBox.ResumeLayout(false);
			this.DefaultCredentialsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		ZArchitecture.GUI.ZGroupBox DefaultCredentialsGroupBox;
		private ZArchitecture.GUI.ZCodeFindBox DefaultBrokerCodeFindBox;
		private ZArchitecture.GUI.ZDropEdit DefaultCredentialSEADropEdit;
		private ZArchitecture.GUI.ZDropEdit DefaultCredentialAIRDropEdit;
		private ZArchitecture.GUI.ZDropEdit ForwarderManifestSEADropEdit;
		private ZArchitecture.GUI.ZDropEdit ForwarderManifestAIRDropEdit;
	}
}
