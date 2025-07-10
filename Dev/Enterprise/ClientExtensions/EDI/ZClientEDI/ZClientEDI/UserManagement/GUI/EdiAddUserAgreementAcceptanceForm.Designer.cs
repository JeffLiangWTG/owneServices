using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.UserManagement.GUI
{
	partial class EdiAddUserAgreementAcceptanceForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.typeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.variantDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.databaseGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.acceptanceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.emailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.jobTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.fullNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.enterpriseGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.addAcceptanceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.typeDropEdit.SuspendLayout();
			this.variantDropEdit.SuspendLayout();
			this.databaseGuidFindBox.SuspendLayout();
			this.zGroupBox1.SuspendLayout();
			this.acceptanceDateEdit.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.enterpriseGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 286, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog);
			// 
			// typeDropEdit
			// 
			this.typeDropEdit.AllowDrop = true;
			this.typeDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.typeDropEdit, "UserAgreement.ERA_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(null)).UserAgreement.ERA_Type)));
			this.typeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 15, true);
			this.typeDropEdit.Name = "typeDropEdit";
			this.typeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 21, true);
			this.typeDropEdit.TabIndex = 1;
			// 
			// variantDropEdit
			// 
			this.variantDropEdit.AllowDrop = true;
			this.variantDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.variantDropEdit, "UserAgreement.ERA_VariantCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(null)).UserAgreement.ERA_VariantCode)));
			this.variantDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 39, true);
			this.variantDropEdit.Name = "variantDropEdit";
			this.variantDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 21, true);
			this.variantDropEdit.TabIndex = 2;
			// 
			// databaseGuidFindBox
			// 
			this.databaseGuidFindBox.AllowDrop = true;
			this.databaseGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.databaseGuidFindBox, "EUL_LD");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(null)).EUL_LD)));
			this.databaseGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 89, true);
			this.databaseGuidFindBox.Name = "databaseGuidFindBox";
			this.databaseGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.databaseGuidFindBox.ParentType = null;
			this.databaseGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 21, true);
			this.databaseGuidFindBox.TabIndex = 4;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zGroupBox1.Controls.Add(this.acceptanceDateEdit);
			this.zGroupBox1.Controls.Add(this.emailTextBox);
			this.zGroupBox1.Controls.Add(this.jobTitleTextBox);
			this.zGroupBox1.Controls.Add(this.fullNameTextBox);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 121, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(454, 127, true);
			this.zGroupBox1.TabIndex = 5;
			this.zGroupBox1.TabStop = false;
			this.zGroupBox1.Text = "zGroupBox1";
			// 
			// acceptanceDateEdit
			// 
			this.acceptanceDateEdit.AllowDrop = true;
			this.acceptanceDateEdit.Anchor = System.Windows.Forms.AnchorStyles.Right;
			this.acceptanceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.acceptanceDateEdit, "EUL_AcceptanceTimeUtc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(null)).EUL_AcceptanceTimeUtc)));
			this.acceptanceDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.acceptanceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 96, true);
			this.acceptanceDateEdit.Name = "acceptanceDateEdit";
			this.acceptanceDateEdit.TabIndex = 3;
			// 
			// emailTextBox
			// 
			this.emailTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.emailTextBox, "EUL_AcceptedByEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(null)).EUL_AcceptedByEmail)));
			this.emailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 69, true);
			this.emailTextBox.Name = "emailTextBox";
			this.emailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 21, true);
			this.emailTextBox.TabIndex = 2;
			this.emailTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// jobTitleTextBox
			// 
			this.jobTitleTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.jobTitleTextBox, "EUL_AcceptedByJobTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(null)).EUL_AcceptedByJobTitle)));
			this.jobTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 45, true);
			this.jobTitleTextBox.Name = "jobTitleTextBox";
			this.jobTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 21, true);
			this.jobTitleTextBox.TabIndex = 1;
			this.jobTitleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// fullNameTextBox
			// 
			this.fullNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.fullNameTextBox, "EUL_AcceptedByName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(null)).EUL_AcceptedByName)));
			this.fullNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 20, true);
			this.fullNameTextBox.Name = "fullNameTextBox";
			this.fullNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(350, 21, true);
			this.fullNameTextBox.TabIndex = 0;
			this.fullNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.enterpriseGuidFindBox);
			this.zPanel1.Controls.Add(this.typeDropEdit);
			this.zPanel1.Controls.Add(this.variantDropEdit);
			this.zPanel1.Controls.Add(this.databaseGuidFindBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 117, true);
			this.zPanel1.TabIndex = 6;
			// 
			// enterpriseGuidFindBox
			// 
			this.enterpriseGuidFindBox.AllowDrop = true;
			this.enterpriseGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.enterpriseGuidFindBox, "EUL_LE");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(null)).EUL_LE)));
			this.enterpriseGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(104, 65, true);
			this.enterpriseGuidFindBox.Name = "enterpriseGuidFindBox";
			this.enterpriseGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.enterpriseGuidFindBox.ParentType = null;
			this.enterpriseGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(352, 21, true);
			this.enterpriseGuidFindBox.TabIndex = 3;
			// 
			// addAcceptanceButton
			// 
			this.addAcceptanceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addAcceptanceButton.IsCaptionOverridden = true;
			this.addAcceptanceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(337, 252, true);
			this.addAcceptanceButton.Name = "addAcceptanceButton";
			this.addAcceptanceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 28, true);
			this.addAcceptanceButton.TabIndex = 8;
			this.addAcceptanceButton.Text = "Add Acceptance";
			this.addAcceptanceButton.ToolTipCaption = null;
			this.addAcceptanceButton.UseVisualStyleBackColor = true;
			this.addAcceptanceButton.Click += new System.EventHandler(this.AddAcceptanceButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.IsCaptionOverridden = true;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 251, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(125, 28, true);
			this.cancelButton.TabIndex = 7;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// EdiAddUserAgreementAcceptanceForm
			//
			SetCaptions();
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(470, 310, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.addAcceptanceButton);
			this.Controls.Add(this.zPanel1);
			this.Controls.Add(this.zGroupBox1);
			this.DataSourceType = typeof(Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MinimizeBox = false;
			this.Name = "EdiAddUserAgreementAcceptanceForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zGroupBox1, 0);
			this.Controls.SetChildIndex(this.zPanel1, 0);
			this.Controls.SetChildIndex(this.addAcceptanceButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.typeDropEdit.ResumeLayout(true);
			this.typeDropEdit.PerformLayout();
			this.variantDropEdit.ResumeLayout(true);
			this.variantDropEdit.PerformLayout();
			this.databaseGuidFindBox.ResumeLayout(true);
			this.databaseGuidFindBox.PerformLayout();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.acceptanceDateEdit.ResumeLayout(true);
			this.acceptanceDateEdit.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.zPanel1.PerformLayout();
			this.enterpriseGuidFindBox.ResumeLayout(true);
			this.enterpriseGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		void SetCaptions()
		{
			typeDropEdit.CaptionResourceString = Res.GetData("4c693d40-088a-4f8a-a41e-74b40a86ad21", "Type");
			variantDropEdit.CaptionResourceString = Res.GetData("041277b3-0aec-42ee-86df-1c1703922d21", "Variant");
			enterpriseGuidFindBox.CaptionResourceString = Res.GetData("9979a657-a994-4ddc-b606-2802c4506de1", "Enterprise");
			databaseGuidFindBox.CaptionResourceString = Res.GetData("41f0a442-bfb8-4912-a526-6e4f116c766a", "Single Database");

			typeDropEdit.ReadOnly = true;
			variantDropEdit.ReadOnly = true;
			enterpriseGuidFindBox.ReadOnly = true;
			
			zGroupBox1.Text = ResString.GetMultilingualString("6fb293f0-5ea3-4fc7-a325-89d1d5447aae", "Authorized Officer Details");
			fullNameTextBox.CaptionResourceString = Res.GetData("cbe14acc-b852-42cf-89a8-1de89dc04d32", "Full Name");
			jobTitleTextBox.CaptionResourceString = Res.GetData("945cecf4-4acc-4c88-97d3-6fd50410b6f6", "Job Title");
			emailTextBox.CaptionResourceString = Res.GetData("15882dce-4b56-40ad-b431-757721f421f7", "Email");
			acceptanceDateEdit.CaptionResourceString = Res.GetData("4280b652-bed5-4b83-9201-2bf3028b06cb", "Acceptance Date (UTC)");

			cancelButton.CaptionResourceString = Res.GetData("49835105-048b-462e-bd0f-253c7991553f", "Cancel");
			addAcceptanceButton.CaptionResourceString = Res.GetData("287460ce-3689-4e16-8769-eb1fe7f47169", "Add Acceptance");
		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit typeDropEdit;
		private ZArchitecture.GUI.ZDropEdit variantDropEdit;
		private ZArchitecture.GUI.ZGuidFindBox databaseGuidFindBox;
		private ZArchitecture.GUI.ZGroupBox zGroupBox1;
		private ZArchitecture.GUI.ZPanel zPanel1;
		private ZArchitecture.ZTextBox emailTextBox;
		private ZArchitecture.ZTextBox jobTitleTextBox;
		private ZArchitecture.ZTextBox fullNameTextBox;
		private ZArchitecture.GUI.ZButton addAcceptanceButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.GUI.ZDateTimeOffsetEdit acceptanceDateEdit;
		private ZArchitecture.GUI.ZGuidFindBox enterpriseGuidFindBox;
	}
}
