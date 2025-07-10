namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class GuaranteeAccessCodesSendingForm
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
		new void InitializeComponent()
		{
			this.GuaranteeReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OfficeOfGuaranteeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CurrentCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewAccessCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MasterCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ButtonsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.messageSendingObjectsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).BeginInit();
			this.MessageSendingObjectsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OfficeOfGuaranteeCodeFindBox.SuspendLayout();
			this.MainGroupBox.SuspendLayout();
			this.ButtonsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// SendButton
			// 
			this.SendButton.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("C2912FE5-2FE5-4D14-827A-731D62D0EB5D", "Confirm and Send");
			this.SendButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 4, true);
			this.SendButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(117, 23, true);
			this.SendButton.TabIndex = 7;
			// 
			// CancelButton2
			// 
			this.CancelButton2.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("0F4FF6CE-8AE0-4CB2-90C4-CCECC8C578B4", "Cancel");
			this.CancelButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(618, 4, true);
			this.CancelButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(87, 21, true);
			this.CancelButton2.TabIndex = 8;
			// 
			// messageSendingObjectsGroupBox
			// 
			this.messageSendingObjectsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.messageSendingObjectsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(891, 242, true);
			this.messageSendingObjectsGroupBox.TabIndex = 0;
			this.messageSendingObjectsGroupBox.Visible = false;
			// 
			// MessageSendingObjectsGrid
			// 
			this.MessageSendingObjectsGrid.GridId = "fbdd025d-18bb-4336-a7c9-cc14cf920ecf";
			this.MessageSendingObjectsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(885, 223, true);
			this.MessageSendingObjectsGrid.TabIndex = 0;
			this.MessageSendingObjectsGrid.Visible = false;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 158, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 23, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObjectParent);
			// 
			// GuaranteeReferenceNumberTextBox
			// 
			this.GuaranteeReferenceNumberTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GuaranteeReferenceNumberTextBox, "SendingObjectsCollection.GuaranteeReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).GuaranteeReferenceNumber)));
			this.GuaranteeReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("dd5df5e7-015e-4d78-9ae0-d73b6e675f51", "GRN");
			this.GuaranteeReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 19, true);
			this.GuaranteeReferenceNumberTextBox.Name = "GuaranteeReferenceNumberTextBox";
			this.GuaranteeReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.GuaranteeReferenceNumberTextBox.TabIndex = 1;
			// 
			// OfficeOfGuaranteeCodeFindBox
			// 
			this.OfficeOfGuaranteeCodeFindBox.AllowDrop = true;
			this.OfficeOfGuaranteeCodeFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OfficeOfGuaranteeCodeFindBox, "SendingObjectsCollection.OfficeOfGuarantee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).OfficeOfGuarantee)));
			this.OfficeOfGuaranteeCodeFindBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("ee8243f5-bc22-467e-8ba9-9c4e644e29c8", "Office of Guarantee");
			this.OfficeOfGuaranteeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 19, true);
			this.OfficeOfGuaranteeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.OfficeOfGuaranteeCodeFindBox.Name = "OfficeOfGuaranteeCodeFindBox";
			this.OfficeOfGuaranteeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OfficeOfGuaranteeCodeFindBox.ParentType = null;
			this.OfficeOfGuaranteeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.OfficeOfGuaranteeCodeFindBox.TabIndex = 2;
			// 
			// CurrentCodeTextBox
			// 
			this.CurrentCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrentCodeTextBox, "SendingObjectsCollection.CurrentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).CurrentCode)));
			this.CurrentCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("b2c05c3f-025a-4ed8-89c0-6f466aad643e", "Current Code");
			this.CurrentCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CurrentCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 43, true);
			this.CurrentCodeTextBox.Name = "CurrentCodeTextBox";
			this.CurrentCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.CurrentCodeTextBox.TabIndex = 3;
			// 
			// NewAccessCodeTextBox
			// 
			this.NewAccessCodeTextBox.AllowDrop = true;
			this.NewAccessCodeTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.NewAccessCodeTextBox, "SendingObjectsCollection.NewAccessCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).NewAccessCode)));
			this.NewAccessCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("bbe6b305-39f4-4c45-9b65-860c7415486d", "New Access Code");
			this.NewAccessCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.NewAccessCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(459, 43, true);
			this.NewAccessCodeTextBox.Name = "NewAccessCodeTextBox";
			this.NewAccessCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(238, 20, true);
			this.NewAccessCodeTextBox.TabIndex = 4;
			// 
			// MasterCodeTextBox
			// 
			this.MasterCodeTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MasterCodeTextBox, "SendingObjectsCollection.MasterCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObject)(((System.Collections.IList)(((Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).MasterCode)));
			this.MasterCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("3755ef8a-81b1-4859-818a-839f648bbcb3", "Master Code");
			this.MasterCodeTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MasterCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 69, true);
			this.MasterCodeTextBox.Name = "MasterCodeTextBox";
			this.MasterCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 20, true);
			this.MasterCodeTextBox.TabIndex = 5;
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.Controls.Add(this.ButtonsPanel);
			this.MainGroupBox.Controls.Add(this.GuaranteeReferenceNumberTextBox);
			this.MainGroupBox.Controls.Add(this.OfficeOfGuaranteeCodeFindBox);
			this.MainGroupBox.Controls.Add(this.CurrentCodeTextBox);
			this.MainGroupBox.Controls.Add(this.NewAccessCodeTextBox);
			this.MainGroupBox.Controls.Add(this.MasterCodeTextBox);
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.MainGroupBox, false);
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 158, true);
			this.MainGroupBox.TabIndex = 11;
			this.MainGroupBox.TabStop = false;
			// 
			// ButtonsPanel
			// 
			this.ButtonsPanel.Controls.Add(this.CancelButton2);
			this.ButtonsPanel.Controls.Add(this.SendButton);
			this.ButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.ButtonsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 126, true);
			this.ButtonsPanel.Name = "ButtonsPanel";
			this.ButtonsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 29, true);
			this.ButtonsPanel.TabIndex = 6;
			// 
			// GuaranteeAccessCodesSendingForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(722, 181, true);
			this.Controls.Add(this.MainGroupBox);
			this.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.GuaranteeAccessCodesSendingObjectParent);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 220, true);
			this.Name = "GuaranteeAccessCodesSendingForm";
			this.Text = "Guarantee Access Code";
			this.Controls.SetChildIndex(this.messageSendingObjectsGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.MainGroupBox, 0);
			this.messageSendingObjectsGroupBox.ResumeLayout(false);
			this.messageSendingObjectsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageSendingObjectsGrid)).EndInit();
			this.MessageSendingObjectsGrid.ResumeLayout(false);
			this.MessageSendingObjectsGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OfficeOfGuaranteeCodeFindBox.ResumeLayout(true);
			this.OfficeOfGuaranteeCodeFindBox.PerformLayout();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			this.ButtonsPanel.ResumeLayout(false);
			this.ButtonsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		protected ZArchitecture.GUI.ZGroupBox MainGroupBox;
		protected Enterprise.ZArchitecture.ZTextBox GuaranteeReferenceNumberTextBox;
		protected Enterprise.ZArchitecture.GUI.ZCodeFindBox OfficeOfGuaranteeCodeFindBox;
		protected Enterprise.ZArchitecture.ZTextBox MasterCodeTextBox;
		protected Enterprise.ZArchitecture.ZTextBox CurrentCodeTextBox;
		protected Enterprise.ZArchitecture.ZTextBox NewAccessCodeTextBox;
		protected ZArchitecture.GUI.ZPanel ButtonsPanel;
	}
}
