namespace Enterprise.Customs.IT.NCTS.GUI
{
	partial class NctsMiscOptionsUserControl
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
			this.NodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AuthorizationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SubscriberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.UseElectronicFolderCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.DeferralGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PaymentPartyDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DefermentAccountNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.WarehouseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.WarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.WarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ParticipantDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MiscGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NodeDropEdit.SuspendLayout();
			this.SubscriberDropEdit.SuspendLayout();
			this.DeferralGroupBox.SuspendLayout();
			this.PaymentPartyDropEdit.SuspendLayout();
			this.DefermentAccountNumberDropEdit.SuspendLayout();
			this.WarehouseGroupBox.SuspendLayout();
			this.WarehouseAddressControl.SuspendLayout();
			this.ParticipantDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MiscGroupBox
			// 
			this.MiscGroupBox.Controls.Add(this.ParticipantDropEdit);
			this.MiscGroupBox.Controls.Add(this.SubscriberDropEdit);
			this.MiscGroupBox.Controls.Add(this.NodeDropEdit);
			this.MiscGroupBox.Controls.Add(this.UseElectronicFolderCheckBox);
			this.MiscGroupBox.Controls.Add(this.AuthorizationDropEdit);
			this.MiscGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 169, true);
			this.MiscGroupBox.Controls.SetChildIndex(this.UseElectronicFolderCheckBox, 0);
			this.MiscGroupBox.Controls.SetChildIndex(this.NodeDropEdit, 0);
			this.MiscGroupBox.Controls.SetChildIndex(this.SubscriberDropEdit, 0);
			this.MiscGroupBox.Controls.SetChildIndex(this.ParticipantDropEdit, 0);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IT.NCTS.Business.NctsHeader);
			// 
			// NodeDropEdit
			// 
			this.NodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NodeDropEdit, "BH_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).BH_CustomsProfile)));
			this.NodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 44, true);
			this.NodeDropEdit.Name = "NodeDropEdit";
			this.NodeDropEdit.PreBoundMaxLength = 3;
			this.NodeDropEdit.ShouldResizeByMaxLength = true;
			this.NodeDropEdit.ShowDescriptionBox = false;
			this.NodeDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.ShowCodeAndDescription;
			this.NodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(53, 20, true);
			this.NodeDropEdit.TabIndex = 1;
			// 
			// AuthorizationDropEdit
			// 
			this.AuthorizationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationDropEdit, "Authorization");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).Authorization)));
			this.AuthorizationDropEdit.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("NctsMiscOptionsUserControl|ED7F1E14-7C76-49D3-89E5-E330044E34CC", "Authorization");
			this.AuthorizationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 116, true);
			this.AuthorizationDropEdit.Name = "AuthorizationDropEdit";
			this.AuthorizationDropEdit.PreBoundMaxLength = 7;
			this.AuthorizationDropEdit.ShouldResizeByMaxLength = true;
			this.AuthorizationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.AuthorizationDropEdit.TabIndex = 4;
			// 
			// SubscriberDropEdit
			// 
			this.SubscriberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubscriberDropEdit, "Subscriber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).Subscriber)));
			this.SubscriberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 69, true);
			this.SubscriberDropEdit.Name = "SubscriberDropEdit";
			this.SubscriberDropEdit.PreBoundMaxLength = 3;
			this.SubscriberDropEdit.ShouldResizeByMaxLength = true;
			this.SubscriberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.SubscriberDropEdit.TabIndex = 2;
			// 
			// UseElectronicFolderCheckBox
			// 
			this.BindingSource.SetBindingMember(this.UseElectronicFolderCheckBox, "MovementHeader.UseElectronicFolder");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.UseElectronicFolder)));
			this.UseElectronicFolderCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UseElectronicFolderCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 91, true);
			this.UseElectronicFolderCheckBox.Name = "UseElectronicFolderCheckBox";
			this.UseElectronicFolderCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(148, 24, true);
			this.UseElectronicFolderCheckBox.TabIndex = 3;
			// 
			// DeferralGroupBox
			// 
			this.DeferralGroupBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("50d9cab8-912e-4782-ba9c-3b9660b8b81b", "Deferral");
			this.DeferralGroupBox.Controls.Add(this.PaymentPartyDropEdit);
			this.DeferralGroupBox.Controls.Add(this.DefermentAccountNumberDropEdit);
			this.DeferralGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 170, true);
			this.DeferralGroupBox.Name = "DeferralGroupBox";
			this.DeferralGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 77, true);
			this.DeferralGroupBox.TabIndex = 1;
			this.DeferralGroupBox.TabStop = false;
			// 
			// PaymentPartyDropEdit
			// 
			this.PaymentPartyDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentPartyDropEdit, "MovementHeader.PaymentParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.PaymentParty)));
			this.PaymentPartyDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 19, true);
			this.PaymentPartyDropEdit.Name = "PaymentPartyDropEdit";
			this.PaymentPartyDropEdit.PreBoundMaxLength = 1;
			this.PaymentPartyDropEdit.ShouldResizeByMaxLength = true;
			this.PaymentPartyDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(269, 20, true);
			this.PaymentPartyDropEdit.TabIndex = 0;
			// 
			// DefermentAccountNumberDropEdit
			// 
			this.DefermentAccountNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DefermentAccountNumberDropEdit, "MovementHeader.DefermentAccountNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.DefermentAccountNumber)));
			this.DefermentAccountNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(110, 45, true);
			this.DefermentAccountNumberDropEdit.Name = "DefermentAccountNumberDropEdit";
			this.DefermentAccountNumberDropEdit.PreBoundMaxLength = 16;
			this.DefermentAccountNumberDropEdit.ShouldResizeByMaxLength = true;
			this.DefermentAccountNumberDropEdit.ShowDescriptionBox = false;
			this.DefermentAccountNumberDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.DefermentAccountNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(133, 20, true);
			this.DefermentAccountNumberDropEdit.TabIndex = 1;
			// 
			// WarehouseGroupBox
			// 
			this.WarehouseGroupBox.CaptionResourceString = Enterprise.Customs.IT.NCTS.GUI.Res.GetData("fe39dccf-ec12-489e-a1b5-0f703aff2838", "[49] Customs Warehouse");
			this.WarehouseGroupBox.Controls.Add(this.WarehouseAddressControl);
			this.WarehouseGroupBox.Controls.Add(this.WarehouseCodeTextBox);
			this.WarehouseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 250, true);
			this.WarehouseGroupBox.Name = "WarehouseGroupBox";
			this.WarehouseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 46, true);
			this.WarehouseGroupBox.TabIndex = 2;
			this.WarehouseGroupBox.TabStop = false;
			// 
			// WarehouseAddressControl
			// 
			this.WarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WarehouseAddressControl, "MovementHeader.BM_OA_WarehouseAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.BM_OA_WarehouseAddress)));
			this.WarehouseAddressControl.BindToOrgList = "MovementHeader.ITLookups.BondedWarehouseCollection";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WarehouseAddressControl, false);
			this.WarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 20, true);
			this.WarehouseAddressControl.Name = "WarehouseAddressControl";
			this.WarehouseAddressControl.PopupCaption = "";
			this.WarehouseAddressControl.ReadOnly = false;
			this.WarehouseAddressControl.ShowAddress = false;
			this.WarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(291, 20, true);
			this.WarehouseAddressControl.TabIndex = 0;
			// 
			// WarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.WarehouseCodeTextBox, "MovementHeader.WarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.WarehouseCode)));
			this.WarehouseCodeTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.WarehouseCodeTextBox, false);
			this.WarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(300, 19, true);
			this.WarehouseCodeTextBox.Name = "WarehouseCodeTextBox";
			this.WarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(79, 20, true);
			this.WarehouseCodeTextBox.TabIndex = 1;
			// 
			// ParticipantDropEdit
			// 
			this.ParticipantDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ParticipantDropEdit, "MovementHeader.ParticipantType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IT.NCTS.Business.NctsHeader)(null)).MovementHeader.ParticipantType)));
			this.ParticipantDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 141, true);
			this.ParticipantDropEdit.Name = "ParticipantDropEdit";
			this.ParticipantDropEdit.PreBoundMaxLength = 3;
			this.ParticipantDropEdit.ShouldResizeByMaxLength = true;
			this.ParticipantDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(283, 20, true);
			this.ParticipantDropEdit.TabIndex = 6;
			// 
			// NctsMiscOptionsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.WarehouseGroupBox);
			this.Controls.Add(this.DeferralGroupBox);
			this.Name = "NctsMiscOptionsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(705, 385, true);
			this.Controls.SetChildIndex(this.DeferralGroupBox, 0);
			this.Controls.SetChildIndex(this.WarehouseGroupBox, 0);
			this.Controls.SetChildIndex(this.MiscGroupBox, 0);
			this.MiscGroupBox.ResumeLayout(false);
			this.MiscGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NodeDropEdit.ResumeLayout(true);
			this.NodeDropEdit.PerformLayout();
			this.AuthorizationDropEdit.ResumeLayout(true);
			this.AuthorizationDropEdit.PerformLayout();
			this.SubscriberDropEdit.ResumeLayout(true);
			this.SubscriberDropEdit.PerformLayout();
			this.DeferralGroupBox.ResumeLayout(false);
			this.DeferralGroupBox.PerformLayout();
			this.PaymentPartyDropEdit.ResumeLayout(true);
			this.PaymentPartyDropEdit.PerformLayout();
			this.DefermentAccountNumberDropEdit.ResumeLayout(true);
			this.DefermentAccountNumberDropEdit.PerformLayout();
			this.WarehouseGroupBox.ResumeLayout(false);
			this.WarehouseGroupBox.PerformLayout();
			this.WarehouseAddressControl.ResumeLayout(true);
			this.WarehouseAddressControl.PerformLayout();
			this.ParticipantDropEdit.ResumeLayout(true);
			this.ParticipantDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZDropEdit NodeDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit SubscriberDropEdit;
		Enterprise.ZArchitecture.GUI.ZCheckBox UseElectronicFolderCheckBox;
		Enterprise.ZArchitecture.GUI.ZGroupBox DeferralGroupBox;
		Enterprise.ZArchitecture.GUI.ZDropEdit PaymentPartyDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit DefermentAccountNumberDropEdit;
		ZArchitecture.GUI.ZGroupBox WarehouseGroupBox;
		ZArchitecture.GUI.ZAddressControl WarehouseAddressControl;
		ZArchitecture.ZTextBox WarehouseCodeTextBox;
		ZArchitecture.GUI.ZDropEdit AuthorizationDropEdit;
		private ZArchitecture.GUI.ZDropEdit ParticipantDropEdit;
	}
}
