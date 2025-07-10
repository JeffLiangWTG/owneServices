using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	partial class InboundCommunicationPartyConfigUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;
		private ZGuidFindBox branchGuidFindBox;
		private ZGuidFindBox departmentGuidFindBox;


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
            this.branchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.departmentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.inboundOAuthUserControl1 = new Enterprise.Messaging.GUI.InboundOAuthUserControl();
            this.authorizationTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.inboundBasicAuthenticationUserControl1 = new Enterprise.Messaging.GUI.InboundBasicAuthenticationUserControl();
            this.authorizationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.inboundActive = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.inboundDisableWarning = new Enterprise.ZArchitecture.ZLabel();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.branchGuidFindBox.SuspendLayout();
            this.departmentGuidFindBox.SuspendLayout();
            this.inboundOAuthUserControl1.SuspendLayout();
            this.authorizationTypeGroupBox.SuspendLayout();
            this.inboundBasicAuthenticationUserControl1.SuspendLayout();
            this.authorizationTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EDICommunicationParty);
            // 
            // branchGuidFindBox
            // 
            this.branchGuidFindBox.AllowDrop = true;
            this.branchGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.branchGuidFindBox, "InboundConfig.ECC_GB_Branch");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.ECC_GB_Branch)));
            this.branchGuidFindBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("726c3ac9-96a5-4885-af5f-4bb870eb659f", "Branch");
            this.branchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 25, true);
            this.branchGuidFindBox.Name = "branchGuidFindBox";
            this.branchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.branchGuidFindBox.ParentType = null;
            this.branchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 17, true);
            this.branchGuidFindBox.TabIndex = 11;
            // 
            // departmentGuidFindBox
            // 
            this.departmentGuidFindBox.AllowDrop = true;
            this.departmentGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.departmentGuidFindBox, "InboundConfig.ECC_GE_Department");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.ECC_GE_Department)));
            this.departmentGuidFindBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("2bef859d-ed93-404d-b0b0-2ff6075752c0", "Department");
            this.departmentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 46, true);
            this.departmentGuidFindBox.Name = "departmentGuidFindBox";
            this.departmentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.departmentGuidFindBox.ParentType = null;
            this.departmentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(177, 17, true);
            this.departmentGuidFindBox.TabIndex = 12;
            // 
            // inboundOAuthUserControl1
            // 
            this.inboundOAuthUserControl1.AllowDrop = true;
            this.inboundOAuthUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.inboundOAuthUserControl1, ".");
            this.inboundOAuthUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
            this.inboundOAuthUserControl1.Name = "inboundOAuthUserControl1";
            this.inboundOAuthUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 291, true);
            this.inboundOAuthUserControl1.TabIndex = 0;
            // 
            // modeGroupBox
            // 
            this.authorizationTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.authorizationTypeGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("ad2c4b4a-55f8-4fb1-8f45-440d7bbdb583", "Details");
            this.authorizationTypeGroupBox.Controls.Add(this.inboundBasicAuthenticationUserControl1);
            this.authorizationTypeGroupBox.Controls.Add(this.inboundOAuthUserControl1);
            this.authorizationTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 67, true);
            this.authorizationTypeGroupBox.Name = "authorizationTypeGroupBox";
            this.authorizationTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(659, 317, true);
            this.authorizationTypeGroupBox.TabIndex = 13;
            this.authorizationTypeGroupBox.TabStop = false;
            // 
            // inboundBasicAuthenticationUserControl1
            // 
            this.inboundBasicAuthenticationUserControl1.AllowDrop = true;
            this.inboundBasicAuthenticationUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.inboundBasicAuthenticationUserControl1, ".");
            this.inboundBasicAuthenticationUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
            this.inboundBasicAuthenticationUserControl1.Name = "inboundBasicAuthenticationUserControl1";
            this.inboundBasicAuthenticationUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(647, 291, true);
            this.inboundBasicAuthenticationUserControl1.TabIndex = 1;
            // 
            // modeDropEdit
            // 
            this.authorizationTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.authorizationTypeDropEdit, "InboundConfig.Auth.ECA_AuthorizationMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.Auth.ECA_AuthorizationMode)));
            this.authorizationTypeDropEdit.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDICommunicationPartyUserControl|3eb9ae6b-4920-4c9d-a8ba-32da992aaa77", "Authorization Type");
            this.authorizationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(130, 25, true);
            this.authorizationTypeDropEdit.Name = "authorizationTypeDropEdit";
            this.authorizationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
            this.authorizationTypeDropEdit.TabIndex = 8;
            // 
            // inboundActive
            // 
            this.inboundActive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.inboundActive, "InboundConfig.ECC_IsActive");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).InboundConfig.ECC_IsActive)));
            this.inboundActive.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("fcc16a31-6809-4c7c-912f-35acd174abff", "Active");
            this.inboundActive.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(452, 5, true);
            this.inboundActive.Name = "inboundActive";
            this.inboundActive.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 17, true);
            this.inboundActive.TabIndex = 9;
            this.inboundActive.Text = "Active";
            this.inboundActive.UseVisualStyleBackColor = true;
            this.inboundActive.CheckedChanged += new System.EventHandler(this.InboundActive_OnCheckedChanged);
            // 
            // inboundDisableWarning
            // 
            this.inboundDisableWarning.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.inboundDisableWarning.ForeColor = System.Drawing.Color.Red;
            this.inboundDisableWarning.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 0, true);
            this.inboundDisableWarning.Name = "inboundDisableWarning";
            this.inboundDisableWarning.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(205, 15, true);
            this.inboundDisableWarning.TabIndex = 14;
            this.inboundDisableWarning.Text = "The inbound config is currently disabled.";
            this.inboundDisableWarning.UseMnemonic = false;
            this.inboundDisableWarning.Visible = false;
            // 
            // InboundCommunicationPartyConfigUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.inboundDisableWarning);
            this.Controls.Add(this.inboundActive);
            this.Controls.Add(this.authorizationTypeGroupBox);
            this.Controls.Add(this.authorizationTypeDropEdit);
            this.Controls.Add(this.branchGuidFindBox);
            this.Controls.Add(this.departmentGuidFindBox);
            this.Name = "InboundCommunicationPartyConfigUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(665, 387, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.branchGuidFindBox.ResumeLayout(true);
            this.branchGuidFindBox.PerformLayout();
            this.departmentGuidFindBox.ResumeLayout(true);
            this.departmentGuidFindBox.PerformLayout();
            this.inboundOAuthUserControl1.ResumeLayout(true);
            this.inboundOAuthUserControl1.PerformLayout();
            this.authorizationTypeGroupBox.ResumeLayout(false);
            this.authorizationTypeGroupBox.PerformLayout();
            this.inboundBasicAuthenticationUserControl1.ResumeLayout(true);
            this.inboundBasicAuthenticationUserControl1.PerformLayout();
            this.authorizationTypeDropEdit.ResumeLayout(true);
            this.authorizationTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private InboundOAuthUserControl inboundOAuthUserControl1;
		private ZDropEdit authorizationTypeDropEdit;
		private ZGroupBox authorizationTypeGroupBox;
		private InboundBasicAuthenticationUserControl inboundBasicAuthenticationUserControl1;
		private ZCheckBox inboundActive;
		private ZLabel inboundDisableWarning;
	}
}
