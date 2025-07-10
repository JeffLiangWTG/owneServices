using System.Windows.Forms;
using Enterprise.Client.EDI.DeviceManagement.Business;
using Enterprise.Client.EDI.Telematics.Tca;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.DeviceManagement.GUI
{
	partial class ClientTelRimRegistrationChangeRegistrationForm
	{
		protected ZButton cancelButton;
		protected ZButton okButton;
		protected ClientDeviceHeader clientDeviceHeader;
		protected ClientTelRimRegistrationSchemeDropEdit enrolmentSchemeDropEdit;
		protected ClientTelRimRegistrationStateCodeDropEdit vehicleRegistrationStateDropEdit;
		protected IRimEnrolmentRequestProcessor rimEnrolmentRequestProcessor;
		protected ZDateTimeOffsetEdit installationDateTimeOffsetEdit;
		protected ZDateTimeOffsetEdit commencementDateTimeOffsetEdit;
		protected ZDateTimeOffsetEdit approvalDateTimeOffsetEdit;
		protected ZArchitecture.ZTextBox vehicleRegistrationTextBox;
		protected ZArchitecture.ZTextBox VehicleIdentificationNumberTextBox;
		protected ZArchitecture.ZTextBox deviceLocationTextBox;

		new void InitializeComponent()
		{
			this.enrolmentSchemeDropEdit = new Enterprise.Client.EDI.DeviceManagement.GUI.ClientTelRimRegistrationSchemeDropEdit();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.vehicleRegistrationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.VehicleIdentificationNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.installationDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.commencementDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.approvalDateTimeOffsetEdit = new Enterprise.ZArchitecture.GUI.ZDateTimeOffsetEdit();
			this.deviceLocationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.vehicleRegistrationStateDropEdit = new Enterprise.Client.EDI.DeviceManagement.GUI.ClientTelRimRegistrationStateCodeDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.enrolmentSchemeDropEdit.SuspendLayout();
			this.installationDateTimeOffsetEdit.SuspendLayout();
			this.commencementDateTimeOffsetEdit.SuspendLayout();
			this.approvalDateTimeOffsetEdit.SuspendLayout();
			this.vehicleRegistrationStateDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 275, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader);
			// 
			// enrolmentSchemeDropEdit
			// 
			this.enrolmentSchemeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.enrolmentSchemeDropEdit, "RimRegistrations.TRR_EnrolmentScheme");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientTelRimRegistration)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).RimRegistrations)).SyncRoot)).TRR_EnrolmentScheme)));
			this.enrolmentSchemeDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("79cfaecb-a30e-457b-987f-31ff0285c684", "Scheme", "RIM Scheme", "Remote Infrastructure Management Scheme", "");
			this.enrolmentSchemeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 12, true);
			this.enrolmentSchemeDropEdit.Name = "enrolmentSchemeDropEdit";
			this.enrolmentSchemeDropEdit.ShouldResizeByMaxLength = true;
			this.enrolmentSchemeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.enrolmentSchemeDropEdit.TabIndex = 1;
			// 
			// clientTelRimRegistrationStateDropEdit
			// 
			this.vehicleRegistrationStateDropEdit.AllowDrop = true;
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientTelRimRegistration)(((System.Collections.IList)(((Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader)(null)).RimRegistrations)).SyncRoot)).TRR_EnrolmentScheme)));
			this.vehicleRegistrationStateDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("66d9cde3-6948-4b9f-ba39-a5851f847f0b", "State Code", "Vehicle State Code", "Vehicle Registration State Code", "");
			this.vehicleRegistrationStateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 38, true);
			this.vehicleRegistrationStateDropEdit.Name = "clientTelRimRegistrationStateDropEdit";
			this.vehicleRegistrationStateDropEdit.ShouldResizeByMaxLength = true;
			this.vehicleRegistrationStateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.vehicleRegistrationStateDropEdit.TabIndex = 2;
			// 
			// okButton
			// 
			this.okButton.IsCaptionOverridden = true;
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 222, true);
			this.okButton.Name = "okButton";
			this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.okButton.TabIndex = 9;
			this.okButton.Text = "Ok";
			this.okButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.DialogResult = DialogResult.OK;
			// 
			// cancelButton
			// 
			this.cancelButton.IsCaptionOverridden = true;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(455, 222, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelButton.TabIndex = 10;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.okButton.DialogResult = DialogResult.Cancel;
			// 
			// vehicleRegistrationTextBox
			// 
			this.vehicleRegistrationTextBox.CaptionResourceString = ZClientEDI.Res.GetData("6cf2e6fe-f3e3-49a4-b90a-06625be3171b", "Vehicle Rn.", "Vehicle Registration", "");
			this.vehicleRegistrationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 65, true);
			this.vehicleRegistrationTextBox.Name = "vehicleRegistrationTextBox";
			this.vehicleRegistrationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.vehicleRegistrationTextBox.TabIndex = 3;
			// 
			// VehicleIdentificationNumberTextBox
			// 
			this.VehicleIdentificationNumberTextBox.CaptionResourceString = ZClientEDI.Res.GetData("1fb1e56c-eb9c-44d8-8a0a-5b5fcacd9b90", "VIN", "Vehicle Identification Number", "");
			this.VehicleIdentificationNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(298, 91, true);
			this.VehicleIdentificationNumberTextBox.Name = "VehicleIdentificationNumberTextBox";
			this.VehicleIdentificationNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
			this.VehicleIdentificationNumberTextBox.TabIndex = 4;
			// 
			// installationDateTimeOffsetEdit
			// 
			this.installationDateTimeOffsetEdit.AllowDrop = true;
			this.installationDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.installationDateTimeOffsetEdit.AutoCompleteYear = true;
			this.installationDateTimeOffsetEdit.CaptionResourceString = ZClientEDI.Res.GetData("bd413e18-0eb9-4081-b247-71b96ff057cf", "Installation Date Time");
			this.installationDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.installationDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(414, 144, true);
			this.installationDateTimeOffsetEdit.Name = "installationDateTimeOffsetEdit";
			this.installationDateTimeOffsetEdit.TabIndex = 6;
			// 
			// commencementDateTimeOffsetEdit
			// 
			this.commencementDateTimeOffsetEdit.AllowDrop = true;
			this.commencementDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.commencementDateTimeOffsetEdit.AutoCompleteYear = true;
			this.commencementDateTimeOffsetEdit.CaptionResourceString = ZClientEDI.Res.GetData("358a6c71-c6b4-43c0-bde9-00efbabdf88f", "Commencement Date Time");
			this.commencementDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.commencementDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(414, 170, true);
			this.commencementDateTimeOffsetEdit.Name = "commencementDateTimeOffsetEdit";
			this.commencementDateTimeOffsetEdit.TabIndex = 7;
			// 
			// approvalDateTimeOffsetEdit
			// 
			this.approvalDateTimeOffsetEdit.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
			this.approvalDateTimeOffsetEdit.AllowDrop = true;
			this.approvalDateTimeOffsetEdit.AutoCompleteMonthThreshold = 1;
			this.approvalDateTimeOffsetEdit.AutoCompleteYear = true;
			this.approvalDateTimeOffsetEdit.CaptionResourceString = ZClientEDI.Res.GetData("0d466c27-c8eb-40db-b5bf-5db48c3b26e7", "Approval Date Time");
			this.approvalDateTimeOffsetEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.approvalDateTimeOffsetEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 196, true);
			this.approvalDateTimeOffsetEdit.Name = "approvalDateTimeOffsetEdit";
			this.approvalDateTimeOffsetEdit.TabIndex = 8;
			// 
			// deviceLocationTextBox
			// 
			this.deviceLocationTextBox.CaptionResourceString = ZClientEDI.Res.GetData("948e15f9-5d29-4045-bc9f-2011784beb0c", "Device Location");
			this.deviceLocationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(299, 118, true);
			this.deviceLocationTextBox.Name = "deviceLocationTextBox";
			this.deviceLocationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 20, true);
			this.deviceLocationTextBox.TabIndex = 5;
			// 
			// ClientTelRimRegistrationChangeRegistrationForm
			//
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 299, true);
			this.Controls.Add(this.vehicleRegistrationStateDropEdit);
			this.Controls.Add(this.deviceLocationTextBox);
			this.Controls.Add(this.approvalDateTimeOffsetEdit);
			this.Controls.Add(this.commencementDateTimeOffsetEdit);
			this.Controls.Add(this.installationDateTimeOffsetEdit);
			this.Controls.Add(this.VehicleIdentificationNumberTextBox);
			this.Controls.Add(this.vehicleRegistrationTextBox);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.enrolmentSchemeDropEdit);
			this.DataSourceAssemblyName = "ZClientEDI";
			this.DataSourceType = typeof(Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader);
			this.DataSourceTypeName = "Enterprise.Client.EDI.DeviceManagement.Business.ClientDeviceHeader";
			this.Name = "ClientTelRimRegistrationChangeRegistrationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.enrolmentSchemeDropEdit, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.vehicleRegistrationTextBox, 0);
			this.Controls.SetChildIndex(this.VehicleIdentificationNumberTextBox, 0);
			this.Controls.SetChildIndex(this.installationDateTimeOffsetEdit, 0);
			this.Controls.SetChildIndex(this.commencementDateTimeOffsetEdit, 0);
			this.Controls.SetChildIndex(this.approvalDateTimeOffsetEdit, 0);
			this.Controls.SetChildIndex(this.deviceLocationTextBox, 0);
			this.Controls.SetChildIndex(this.vehicleRegistrationStateDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.enrolmentSchemeDropEdit.ResumeLayout(true);
			this.enrolmentSchemeDropEdit.PerformLayout();
			this.installationDateTimeOffsetEdit.ResumeLayout(true);
			this.installationDateTimeOffsetEdit.PerformLayout();
			this.commencementDateTimeOffsetEdit.ResumeLayout(true);
			this.commencementDateTimeOffsetEdit.PerformLayout();
			this.approvalDateTimeOffsetEdit.ResumeLayout(true);
			this.approvalDateTimeOffsetEdit.PerformLayout();
			this.vehicleRegistrationStateDropEdit.ResumeLayout(true);
			this.vehicleRegistrationStateDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
