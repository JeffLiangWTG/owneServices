using System.Drawing;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	partial class OutboundCommunicationPartyConfigUserControl
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
            this.authorizationTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.outboundNoAuthenticationUserControl1 = new Enterprise.Messaging.GUI.OutboundNoAuthenticationUserControl();
            this.outboundOAuthUserControl1 = new Enterprise.Messaging.GUI.OutboundOAuthUserControl();
            this.outboundBasicAuthenticationUserControl1 = new Enterprise.Messaging.GUI.OutboundBasicAuthenticationUserControl();
            this.authorizationTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.EndpointTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.outboundDisableWarning = new Enterprise.ZArchitecture.ZLabel();
            this.outboundActive = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.authorizationTypeGroupBox.SuspendLayout();
            this.outboundOAuthUserControl1.SuspendLayout();
            this.outboundBasicAuthenticationUserControl1.SuspendLayout();
            this.authorizationTypeDropEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EDICommunicationParty);
            // 
            // modeGroupBox
            // 
            this.authorizationTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.authorizationTypeGroupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDICommunicationPartyUserControl|ed8f49a6-f1aa-40cf-853c-7614d24db82f", "Details");
			this.authorizationTypeGroupBox.Controls.Add(this.outboundNoAuthenticationUserControl1);
            this.authorizationTypeGroupBox.Controls.Add(this.outboundOAuthUserControl1);
            this.authorizationTypeGroupBox.Controls.Add(this.outboundBasicAuthenticationUserControl1);
            this.authorizationTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 67, true);
            this.authorizationTypeGroupBox.Name = "authorizationTypeGroupBox";
            this.authorizationTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 305, true);
            this.authorizationTypeGroupBox.TabIndex = 15;
            this.authorizationTypeGroupBox.TabStop = false;
            // 
            // outboundNoAuthenticationUserControl1
            // 
            this.outboundNoAuthenticationUserControl1.AllowDrop = true;
            this.outboundNoAuthenticationUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.outboundNoAuthenticationUserControl1, ".");
            this.outboundNoAuthenticationUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 11, true);
            this.outboundNoAuthenticationUserControl1.Name = "outboundNoAuthenticationUserControl1";
            this.outboundNoAuthenticationUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 287, true);
            this.outboundNoAuthenticationUserControl1.TabIndex = 1;
            // 
            // outboundOAuthUserControl1
            // 
            this.outboundOAuthUserControl1.AllowDrop = true;
            this.outboundOAuthUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.outboundOAuthUserControl1, ".");
            this.outboundOAuthUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
            this.outboundOAuthUserControl1.Name = "outboundOAuthUserControl1";
            this.outboundOAuthUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 280, true);
            this.outboundOAuthUserControl1.TabIndex = 0;
            // 
            // outboundBasicAuthenticationUserControl1
            // 
            this.outboundBasicAuthenticationUserControl1.AllowDrop = true;
            this.outboundBasicAuthenticationUserControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.outboundBasicAuthenticationUserControl1, ".");
            this.outboundBasicAuthenticationUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 19, true);
            this.outboundBasicAuthenticationUserControl1.Name = "outboundBasicAuthenticationUserControl1";
            this.outboundBasicAuthenticationUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(637, 280, true);
            this.outboundBasicAuthenticationUserControl1.TabIndex = 2;
            // 
            // modeDropEdit
            // 
            this.authorizationTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.authorizationTypeDropEdit, "OutboundConfig.Auth.ECA_AuthorizationMode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.Auth.ECA_AuthorizationMode)));
            this.authorizationTypeDropEdit.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDICommunicationPartyUserControl|564d20cf-e946-4e87-8200-f60ac0e2c23a", "Authorization Type");
            this.authorizationTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(113, 24, true);
            this.authorizationTypeDropEdit.Name = "authorizationTypeDropEdit";
            this.authorizationTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 17, true);
            this.authorizationTypeDropEdit.TabIndex = 16;
            // 
            // EndpointTextBox
            // 
            this.EndpointTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.EndpointTextBox, "OutboundConfig.ECC_Endpoint");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.ECC_Endpoint)));
            this.EndpointTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("ff296ffb-f18a-46dc-a224-27cf9e6c0a6c", "Endpoint");
            this.EndpointTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.EndpointTextBox.Cursor = System.Windows.Forms.Cursors.Default;
            this.EndpointTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(65, 45, true);
            this.EndpointTextBox.Name = "EndpointTextBox";
            this.EndpointTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(571, 17, true);
            this.EndpointTextBox.TabIndex = 17;
            this.EndpointTextBox.Tag = "";
            // 
            // outboundDisableWarning
            // 
            this.outboundDisableWarning.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.outboundDisableWarning.ForeColor = System.Drawing.Color.Red;
            this.outboundDisableWarning.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(185, 2, true);
            this.outboundDisableWarning.Name = "outboundDisableWarning";
            this.outboundDisableWarning.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(225, 15, true);
            this.outboundDisableWarning.TabIndex = 18;
            this.outboundDisableWarning.Text = "The outbound config is currently disabled.";
            this.outboundDisableWarning.UseMnemonic = false;
            this.outboundDisableWarning.Visible = false;
            // 
            // outboundActive
            // 
            this.outboundActive.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.outboundActive, "OutboundConfig.ECC_IsActive");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).OutboundConfig.ECC_IsActive)));
            this.outboundActive.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("bd1ab9cc-1a1d-4242-aa38-3c573e919cdc", "Active");
            this.outboundActive.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(442, 5, true);
            this.outboundActive.Name = "outboundActive";
            this.outboundActive.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(70, 20, true);
            this.outboundActive.TabIndex = 19;
            this.outboundActive.Text = "Active";
            this.outboundActive.UseVisualStyleBackColor = true;
            this.outboundActive.CheckedChanged += new System.EventHandler(this.OutboundActive_OnCheckedChanged);
            // 
            // OutboundCommunicationPartyConfigUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.outboundActive);
            this.Controls.Add(this.outboundDisableWarning);
            this.Controls.Add(this.EndpointTextBox);
            this.Controls.Add(this.authorizationTypeDropEdit);
            this.Controls.Add(this.authorizationTypeGroupBox);
            this.Name = "OutboundCommunicationPartyConfigUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(655, 375, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.authorizationTypeGroupBox.ResumeLayout(false);
            this.authorizationTypeGroupBox.PerformLayout();
            this.outboundOAuthUserControl1.ResumeLayout(true);
            this.outboundOAuthUserControl1.PerformLayout();
            this.outboundBasicAuthenticationUserControl1.ResumeLayout(true);
            this.outboundBasicAuthenticationUserControl1.PerformLayout();
            this.authorizationTypeDropEdit.ResumeLayout(true);
            this.authorizationTypeDropEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private OutboundOAuthUserControl outboundOAuthUserControl1;
		private OutboundBasicAuthenticationUserControl outboundBasicAuthenticationUserControl1;
		private OutboundNoAuthenticationUserControl outboundNoAuthenticationUserControl1;
		private ZGroupBox authorizationTypeGroupBox;
		private ZDropEdit authorizationTypeDropEdit;
		private ZTextBox EndpointTextBox;
		private ZLabel outboundDisableWarning;
		private ZCheckBox outboundActive;
	}
}
