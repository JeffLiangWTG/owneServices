using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Messaging.GUI
{
	partial class EDICommunicationPartyUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		private Enterprise.ZArchitecture.GUI.ZGroupBox groupBox;
		private ZArchitecture.ZTextBox nameTextBox;
		private Enterprise.ZArchitecture.GUI.ZTabControl tabControl;
		private ZArchitecture.ZTextBox summaryTextBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit connectionTypeDropEdit;
		private ZCheckBox activeClient;

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
            this.components = new System.ComponentModel.Container();
            this.tabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
            this.groupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.technicalContactGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.activeClient = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.summaryTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.nameTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.connectionTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.StaffFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.groupBox.SuspendLayout();
            this.technicalContactGuidFindBox.SuspendLayout();
            this.connectionTypeDropEdit.SuspendLayout();
            this.StaffFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.EDICommunicationParty);
            // 
            // tabControl
            // 
            this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 123, true);
            this.tabControl.Name = "tabControl";
            this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(597, 250, true);
            this.tabControl.TabIndex = 9;
            // 
            // groupBox
            // 
            this.groupBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("52de7e15-6992-4f49-93d5-5c233bc77ed7", "Config");
            this.groupBox.Controls.Add(this.technicalContactGuidFindBox);
            this.groupBox.Controls.Add(this.activeClient);
            this.groupBox.Controls.Add(this.summaryTextBox);
            this.groupBox.Controls.Add(this.tabControl);
            this.groupBox.Controls.Add(this.nameTextBox);
            this.groupBox.Controls.Add(this.connectionTypeDropEdit);
            this.groupBox.Controls.Add(this.StaffFindBox);
            this.groupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.groupBox.Name = "groupBox";
            this.groupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 378, true);
            this.groupBox.TabIndex = 7;
            this.groupBox.TabStop = false;
            // 
            // technicalContactGuidFindBox
            // 
            this.technicalContactGuidFindBox.AllowDrop = true;
            this.technicalContactGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.technicalContactGuidFindBox, "ECP_OC_TechnicalContact");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).ECP_OC_TechnicalContact)));
            this.technicalContactGuidFindBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("d5b7b153-e886-4377-a5ec-165b6099cda7", "Technical Contact");
            this.technicalContactGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 71, true);
			this.technicalContactGuidFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.OrgContacts;
			this.technicalContactGuidFindBox.Name = "technicalContactGuidFindBox";
			this.technicalContactGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.technicalContactGuidFindBox.ParentType = null;
            this.technicalContactGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 21, true);
            this.technicalContactGuidFindBox.TabIndex = 6;
            // 
            // activeClient
            // 
            this.activeClient.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.activeClient, "ECP_IsActive");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).ECP_IsActive)));
            this.activeClient.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("007d316d-d889-463a-acf8-910fb304152e", "Enabled");
            this.activeClient.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 45, true);
            this.activeClient.Name = "activeClient";
            this.activeClient.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(86, 16, true);
            this.activeClient.TabIndex = 5;
            this.activeClient.Text = "Enabled";
            this.activeClient.UseVisualStyleBackColor = false;
            this.activeClient.CheckedChanged += new System.EventHandler(this.ActiveCheckBox_OnCheckedChanged);
            // 
            // summaryTextBox
            // 
            this.BindingSource.SetBindingMember(this.summaryTextBox, "ECP_Summary");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).ECP_Summary)));
            this.summaryTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("002980eb-a4c9-46ad-a100-eb13364306e5", "Summary");
            this.summaryTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.summaryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 71, true);
            this.summaryTextBox.Name = "summaryTextBox";
            this.summaryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 21, true);
            this.summaryTextBox.TabIndex = 4;
            // 
            // nameTextBox
            // 
            this.BindingSource.SetBindingMember(this.nameTextBox, "ECP_Name");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).ECP_Name)));
            this.nameTextBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("fed629c3-32d2-4515-8456-8d405cc93f6e", "EDI Client Name");
            this.nameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.nameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 45, true);
            this.nameTextBox.Name = "nameTextBox";
            this.nameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 21, true);
            this.nameTextBox.TabIndex = 3;
            this.nameTextBox.Tag = "";
            // 
            // connectionTypeDropEdit
            // 
            this.connectionTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.connectionTypeDropEdit, "ECP_ApplicationCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).ECP_ApplicationCode)));
            this.connectionTypeDropEdit.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDICommunicationPartyUserControl|ad0fdec0-4472-48bb-b341-a985ddca71a1", "Connection Type");
            this.connectionTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 19, true);
            this.connectionTypeDropEdit.Name = "connectionTypeDropEdit";
            this.connectionTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 21, true);
            this.connectionTypeDropEdit.TabIndex = 1;
            // 
            // StaffFindBox
            // 
            this.StaffFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StaffFindBox, "ECP_GS_SecurityProxy");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.EDICommunicationParty)(null)).ECP_GS_SecurityProxy)));
            this.StaffFindBox.CaptionResourceString = Enterprise.Messaging.GUI.Res.GetData("EDICommunicationPartyUserControl|e78c4aac-e6c2-4540-a5c6-87780e3b4b2b", "Staff Proxy");
            this.StaffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 96, true);
            this.StaffFindBox.Name = "StaffFindBox";
            this.StaffFindBox.ParentType = null;
            this.StaffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 21, true);
            this.StaffFindBox.TabIndex = 8;
            // 
            // EDICommunicationPartyUserControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.Controls.Add(this.groupBox);
            this.Name = "EDICommunicationPartyUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(607, 378, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.groupBox.ResumeLayout(false);
            this.groupBox.PerformLayout();
            this.technicalContactGuidFindBox.ResumeLayout(true);
            this.technicalContactGuidFindBox.PerformLayout();
            this.connectionTypeDropEdit.ResumeLayout(true);
            this.connectionTypeDropEdit.PerformLayout();
            this.StaffFindBox.ResumeLayout(true);
            this.StaffFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGuidFindBox technicalContactGuidFindBox;
		private ZGuidFindBox StaffFindBox;
	}
}
