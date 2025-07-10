using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.GUI
{
	partial class EntryInstructionBasicDetailsControl
	{
		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.LocationOfGoodsUserControl = new Enterprise.Customs.EU.GUI.LocationOfGoodsUserControl();
			this.ToWarehouseUserControl = new Enterprise.Customs.EU.GUI.ToWarehouseUserControl();
			this.ToWarehouseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ToWarehouseTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ToWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWarehouseUserControl = new Enterprise.Customs.EU.GUI.FromWarehouseUserControl();
			this.FromWarehouseLabel = new Enterprise.ZArchitecture.ZLabel();
			this.FromWarehouseTypeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.FromWarehouseCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NewOwnerOrganisationControl = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.AcceptanceDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RequestedDocumentsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RequestedDocumentsUserControl = new Enterprise.Customs.EU.GUI.DeclarationRequestedDocumentsUserControl();
			this.ToWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.FromWarehouseAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LocationOfGoodsUserControl.SuspendLayout();
			this.ToWarehouseUserControl.SuspendLayout();
			this.FromWarehouseUserControl.SuspendLayout();
			this.NewOwnerOrganisationControl.SuspendLayout();
			this.AcceptanceDateEdit.SuspendLayout();
			this.RequestedDocumentsGroupBox.SuspendLayout();
			this.RequestedDocumentsUserControl.SuspendLayout();
			this.ToWarehouseAddressControl.SuspendLayout();
			this.FromWarehouseAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction);
			// 
			// LocationOfGoodsUserControl
			// 
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.LocationOfGoodsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.ICusGoodsLocationProvider)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)))));
			this.LocationOfGoodsUserControl.CusGoodsLocationProviderType = null;
			this.LocationOfGoodsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 93, true);
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			this.LocationOfGoodsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 21, true);
			this.LocationOfGoodsUserControl.TabIndex = 2;
			// 
			// ToWarehouseUserControl
			// 
			this.ToWarehouseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseUserControl, ".");
			this.ToWarehouseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 48, true);
			this.ToWarehouseUserControl.Name = "ToWarehouseUserControl";
			this.ToWarehouseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 22, true);
			this.ToWarehouseUserControl.TabIndex = 2;
			// 
			// ToWarehouseLabel
			// 
			this.ToWarehouseLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("90D0C1C0-422C-4350-BA52-8D1EA67ABFF0", "To Warehouse");
			this.ToWarehouseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.ToWarehouseLabel.IsFontBold = true;
			this.ToWarehouseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(846, 402, true);
			this.ToWarehouseLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.ToWarehouseLabel.Name = "ToWarehouseLabel";
			this.ToWarehouseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.ToWarehouseLabel.TabIndex = 5;
			this.ToWarehouseLabel.UseMnemonic = false;
			// 
			// ToWarehouseTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWarehouseTypeTextBox, "ToWarehouseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).ToWarehouseType)));
			this.ToWarehouseTypeTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1fc1ab9a-2102-43c4-92a0-f987af7aec80", "Warehouse Type");
			this.ToWarehouseTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 13, true);
			this.ToWarehouseTypeTextBox.Name = "ToWarehouseTypeTextBox";
			this.ToWarehouseTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 15, true);
			this.ToWarehouseTypeTextBox.TabIndex = 1;
			// 
			// ToWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.ToWarehouseCodeTextBox, "ToWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).ToWarehouseCode)));
			this.ToWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9c3e67d9-45eb-43fa-8466-441a2a514cf9", "Warehouse ID");
			this.ToWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 48, true);
			this.ToWarehouseCodeTextBox.Name = "ToWarehouseCodeTextBox";
			this.ToWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.ToWarehouseCodeTextBox.TabIndex = 2;
			// 
			// FromWarehouseUserControl
			// 
			this.FromWarehouseUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseUserControl, ".");
			this.FromWarehouseUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 68, true);
			this.FromWarehouseUserControl.Name = "FromWarehouseUserControl";
			this.FromWarehouseUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 22, true);
			this.FromWarehouseUserControl.TabIndex = 7;
			// 
			// FromWarehouseLabel
			// 
			this.FromWarehouseLabel.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("3818CCDE-0D47-4118-AA3B-CDE0E6D3A455", "From Warehouse");
			this.FromWarehouseLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.FromWarehouseLabel.IsFontBold = true;
			this.FromWarehouseLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(10, 438, true);
			this.FromWarehouseLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 0, 0, 10, true);
			this.FromWarehouseLabel.Name = "FromWarehouseLabel";
			this.FromWarehouseLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 20, true);
			this.FromWarehouseLabel.TabIndex = 6;
			this.FromWarehouseLabel.UseMnemonic = false;
			// 
			// FromWarehouseTypeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseTypeTextBox, "FromWarehouseType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).FromWarehouseType)));
			this.FromWarehouseTypeTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1fc1ab9a-2102-43c4-92a0-f987af7aec80", "Warehouse Type");
			this.FromWarehouseTypeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 30, true);
			this.FromWarehouseTypeTextBox.Name = "FromWarehouseTypeTextBox";
			this.FromWarehouseTypeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 15, true);
			this.FromWarehouseTypeTextBox.TabIndex = 1;
			// 
			// FromWarehouseCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.FromWarehouseCodeTextBox, "FromWarehouseCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).FromWarehouseCode)));
			this.FromWarehouseCodeTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("9c3e67d9-45eb-43fa-8466-441a2a514cf9", "Warehouse ID");
			this.FromWarehouseCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 83, true);
			this.FromWarehouseCodeTextBox.Name = "FromWarehouseCodeTextBox";
			this.FromWarehouseCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 15, true);
			this.FromWarehouseCodeTextBox.TabIndex = 2;
			// 
			// NewOwnerOrganisationControl
			// 
			this.NewOwnerOrganisationControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NewOwnerOrganisationControl, "CEI_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).CEI_OH_Owner)));
			this.NewOwnerOrganisationControl.BindToOrganisations = "Lookups+Organisations";
			this.NewOwnerOrganisationControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("1331cd2f-2d8a-4795-bc9b-7543e3577f9c", "Owner");
			this.NewOwnerOrganisationControl.Captions = new string[] {
        "Owner"};
			this.NewOwnerOrganisationControl.Details = Enterprise.MasterFiles.GUI.OrganisationDetails.None;
			this.NewOwnerOrganisationControl.IsCaptionOverridden = false;
			this.NewOwnerOrganisationControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 120, true);
			this.NewOwnerOrganisationControl.Name = "NewOwnerOrganisationControl";
			this.NewOwnerOrganisationControl.OrgAddressFormatter = null;
			this.NewOwnerOrganisationControl.PopupCaption = "";
			this.NewOwnerOrganisationControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 34, true);
			this.NewOwnerOrganisationControl.TabIndex = 3;
			this.NewOwnerOrganisationControl.Visible = false;
			// 
			// AcceptanceDateEdit
			// 
			this.AcceptanceDateEdit.AllowDrop = true;
			this.AcceptanceDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.AcceptanceDateEdit, "CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).CEI_DateForDuty)));
			this.AcceptanceDateEdit.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("de56bfdf-c376-49db-a359-08581c3fe986", "Acceptance Date");
			this.AcceptanceDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.AcceptanceDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(706, 66, true);
			this.AcceptanceDateEdit.Name = "AcceptanceDateEdit";
			this.AcceptanceDateEdit.TabIndex = 4;
			this.AcceptanceDateEdit.Visible = false;
			// 
			// RequestedDocumentsGroupBox
			// 
			this.RequestedDocumentsGroupBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("c6143724-2555-4cd4-8fd9-7aa2c0fc9d65", "Documents Requested");
			this.RequestedDocumentsGroupBox.Controls.Add(this.RequestedDocumentsUserControl);
			this.RequestedDocumentsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 181, true);
			this.RequestedDocumentsGroupBox.Name = "RequestedDocumentsGroupBox";
			this.RequestedDocumentsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1177, 383, true);
			this.RequestedDocumentsGroupBox.TabIndex = 2;
			this.RequestedDocumentsGroupBox.TabStop = false;
			// 
			// RequestedDocumentsUserControl
			// 
			this.RequestedDocumentsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RequestedDocumentsUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Customs.EU.Business.IRequestedDocumentsProvider)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)))));
			this.RequestedDocumentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RequestedDocumentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 14, true);
			this.RequestedDocumentsUserControl.Name = "RequestedDocumentsUserControl";
			this.RequestedDocumentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1174, 368, true);
			this.RequestedDocumentsUserControl.TabIndex = 0;
			// 
			// ToWarehouseAddressControl
			// 
			this.ToWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ToWarehouseAddressControl, "CEI_OA_Warehouse2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).CEI_OA_Warehouse2)));
			this.ToWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToWarehouseAddressControl, false);
			this.ToWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 13, true);
			this.ToWarehouseAddressControl.Name = "ToWarehouseAddressControl";
			this.ToWarehouseAddressControl.PopupCaption = "";
			this.ToWarehouseAddressControl.ShowAddress = false;
			this.ToWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 15, true);
			this.ToWarehouseAddressControl.TabIndex = 1;
			// 
			// FromWarehouseAddressControl
			// 
			this.FromWarehouseAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FromWarehouseAddressControl, "CEI_OA_Warehouse");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.EU.Business.Declaration.CusEntryInstruction)(null)).CEI_OA_Warehouse)));
			this.FromWarehouseAddressControl.BindToOrgList = "Lookups.BondedWarehouseCollection";
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.FromWarehouseAddressControl, false);
			this.FromWarehouseAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(56, 30, true);
			this.FromWarehouseAddressControl.Name = "FromWarehouseAddressControl";
			this.FromWarehouseAddressControl.PopupCaption = "";
			this.FromWarehouseAddressControl.ShowAddress = false;
			this.FromWarehouseAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(552, 15, true);
			this.FromWarehouseAddressControl.TabIndex = 8;
			// 
			// EntryInstructionBasicDetailsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.FromWarehouseAddressControl);
			this.Controls.Add(this.LocationOfGoodsUserControl);
			this.Controls.Add(this.ToWarehouseUserControl);
			this.Controls.Add(this.ToWarehouseTypeTextBox);
			this.Controls.Add(this.ToWarehouseCodeTextBox);
			this.Controls.Add(this.ToWarehouseLabel);
			this.Controls.Add(this.FromWarehouseLabel);
			this.Controls.Add(this.FromWarehouseUserControl);
			this.Controls.Add(this.FromWarehouseTypeTextBox);
			this.Controls.Add(this.FromWarehouseCodeTextBox);
			this.Controls.Add(this.NewOwnerOrganisationControl);
			this.Controls.Add(this.AcceptanceDateEdit);
			this.Controls.Add(this.RequestedDocumentsGroupBox);
			this.Controls.Add(this.ToWarehouseAddressControl);
			this.Name = "EntryInstructionBasicDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1181, 558, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LocationOfGoodsUserControl.ResumeLayout(true);
			this.LocationOfGoodsUserControl.PerformLayout();
			this.ToWarehouseUserControl.ResumeLayout(true);
			this.ToWarehouseUserControl.PerformLayout();
			this.FromWarehouseUserControl.ResumeLayout(true);
			this.FromWarehouseUserControl.PerformLayout();
			this.NewOwnerOrganisationControl.ResumeLayout(true);
			this.NewOwnerOrganisationControl.PerformLayout();
			this.AcceptanceDateEdit.ResumeLayout(true);
			this.AcceptanceDateEdit.PerformLayout();
			this.RequestedDocumentsGroupBox.ResumeLayout(false);
			this.RequestedDocumentsGroupBox.PerformLayout();
			this.RequestedDocumentsUserControl.ResumeLayout(true);
			this.RequestedDocumentsUserControl.PerformLayout();
			this.ToWarehouseAddressControl.ResumeLayout(true);
			this.ToWarehouseAddressControl.PerformLayout();
			this.FromWarehouseAddressControl.ResumeLayout(true);
			this.FromWarehouseAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ToWarehouseUserControl ToWarehouseUserControl;
		protected internal ZArchitecture.ZTextBox ToWarehouseTypeTextBox;
		protected internal ZArchitecture.ZTextBox ToWarehouseCodeTextBox;
		protected internal ZArchitecture.ZTextBox FromWarehouseTypeTextBox;
		protected internal ZArchitecture.ZTextBox FromWarehouseCodeTextBox;
		internal ZLabel ToWarehouseLabel;
		internal ZLabel FromWarehouseLabel;
		internal FromWarehouseUserControl FromWarehouseUserControl;
		internal LocationOfGoodsUserControl LocationOfGoodsUserControl;
		internal MasterFiles.GUI.ZOrganisationControl NewOwnerOrganisationControl;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit AcceptanceDateEdit;
		protected internal ZArchitecture.GUI.ZGroupBox RequestedDocumentsGroupBox;
		internal DeclarationRequestedDocumentsUserControl RequestedDocumentsUserControl;
		internal ZArchitecture.GUI.ZAddressControl ToWarehouseAddressControl;
		internal ZArchitecture.GUI.ZAddressControl FromWarehouseAddressControl;
	}
}
