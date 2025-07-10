using Enterprise.Customs.FR.NCTS.Messaging;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.FR.GUI.NCTS
{
	partial class MessageSendingFormBottomSectionUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.ActualConsigneeDocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.ActualOfficeOfDestinationFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.JustificationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.QueryInformationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TC11DeliveryDate = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ActualConsigneeLabel = new Enterprise.ZArchitecture.ZLabel();
			this.RequesterIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequesterRoleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.QueryPeriodFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.QueryPeriodToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.QueryIdentifierDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TC11DeliveryDate.SuspendLayout();
			this.ActualOfficeOfDestinationFindBox.SuspendLayout();
			this.ActualConsigneeDocAddressControl.SuspendLayout();
			this.RequesterRoleDropEdit.SuspendLayout();
			this.QueryPeriodFromDateEdit.SuspendLayout();
			this.QueryPeriodToDateEdit.SuspendLayout();
			this.QueryIdentifierDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(TP5MessageSendingObjectParent);
			// 
			// JustificationTextBox
			// 
			this.BindingSource.SetBindingMember(this.JustificationTextBox, "SendingObjectsCollection.Justification");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).Justification)));
			this.JustificationTextBox.CaptionResourceString = null;
			this.LabelCaptionRenderProvider.SetLabelTop(this.JustificationTextBox, 0);
			this.JustificationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 22, true);
			this.JustificationTextBox.Multiline = true;
			this.JustificationTextBox.Name = "JustificationTextBox";
			this.JustificationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 215, true);
			this.JustificationTextBox.TabIndex = 0;
			// 
			// TC11DeliveryDateEdit
			// 
			this.TC11DeliveryDate.AllowDrop = true;
			this.TC11DeliveryDate.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.TC11DeliveryDate, "SendingObjectsCollection.TC11DeliveryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).TC11DeliveryDate)));
			this.TC11DeliveryDate.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 22, true);
			this.TC11DeliveryDate.Name = "TC11DeliveryDateEdit";
			this.TC11DeliveryDate.TabIndex = 1;
			// 
			// QueryInformationTextBox
			// 
			this.BindingSource.SetBindingMember(this.QueryInformationTextBox, "SendingObjectsCollection.QueryInformation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).QueryInformation)));
			this.LabelCaptionRenderProvider.SetLabelTop(this.QueryInformationTextBox, 0);
			this.QueryInformationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 40, true);
			this.QueryInformationTextBox.Multiline = true;
			this.QueryInformationTextBox.Name = "QueryInformationTextBox";
			this.QueryInformationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 215, true);
			this.QueryInformationTextBox.TabIndex = 2;
			// 
			// ActualOfficeOfDestinationFindBox
			// 
			this.ActualOfficeOfDestinationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActualOfficeOfDestinationFindBox, "SendingObjectsCollection.ActualOfficeOfDestination");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ActualOfficeOfDestination)));
			this.ActualOfficeOfDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(840, 22, true);
			this.ActualOfficeOfDestinationFindBox.Name = "ActualOfficeOfDestinationFindBox";
			this.ActualOfficeOfDestinationFindBox.ParentType = null;
			this.ActualOfficeOfDestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 15, true);
			this.ActualOfficeOfDestinationFindBox.TabIndex = 3;
			// 
			// ActualConsigneeLabel
			// 
			this.ActualConsigneeLabel.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("b6f381dc-414b-4f9e-a314-a75fc3d632b8", "Actual Consignee");
			this.ActualConsigneeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(750, 52, true);
			this.ActualConsigneeLabel.Name = "ActualConsigneeLabel";
			this.ActualConsigneeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			// 
			// ActualConsigneeDocAddressControl
			// 
			this.ActualConsigneeDocAddressControl.AddressValidationProcessCmdKey = null;
			this.ActualConsigneeDocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ActualConsigneeDocAddressControl, "SendingObjectsCollection.ActualConsignee");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).ActualConsignee)));
			this.ActualConsigneeDocAddressControl.BindToOrganisations = "SendingObjectsCollection.Lookups.Consignees";
			this.ActualConsigneeDocAddressControl.DisplayMode = Enterprise.MasterFiles.GUI.ZDocAddressControlDisplayMode.CompactWithOverride;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ActualConsigneeDocAddressControl, false);
			this.ActualConsigneeDocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(843, 40, true);
			this.ActualConsigneeDocAddressControl.Name = "ActualConsigneeDocAddressControl";
			this.ActualConsigneeDocAddressControl.ReadOnly = false;
			this.ActualConsigneeDocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.ActualConsigneeDocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 130, true);
			this.ActualConsigneeDocAddressControl.TabIndex = 4;
			this.ActualConsigneeDocAddressControl.ValidationJustForced = false;
			// 
			// QueryIdentifierDropEdit
			//
			this.BindingSource.SetBindingMember(this.QueryIdentifierDropEdit, "SendingObjectsCollection.QueryIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).QueryIdentifier)));
			this.QueryIdentifierDropEdit.AllowDrop = true;
			this.QueryIdentifierDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("22d271ab-6e22-4fb5-a5a8-d3437549569a", "Query Identifier");
			this.QueryIdentifierDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 10, true);
			this.QueryIdentifierDropEdit.Name = "QueryIdentifierDropEdit";
			this.QueryIdentifierDropEdit.ShowDescriptionBox = true;
			this.QueryIdentifierDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 20, true);
			this.QueryIdentifierDropEdit.TabIndex = 5;
			// 
			// QueryPeriodFromDateEdit
			//
			this.BindingSource.SetBindingMember(this.QueryPeriodFromDateEdit, "SendingObjectsCollection.PeriodFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PeriodFrom)));
			this.QueryPeriodFromDateEdit.AllowDrop = true;
			this.QueryPeriodFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.QueryPeriodFromDateEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("e4fbe043-6892-4f25-830b-e98142296a00", "Period From");
			this.QueryPeriodFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(563, 10, true);
			this.QueryPeriodFromDateEdit.Name = "QueryPeriodFromDateEdit";
			this.QueryPeriodFromDateEdit.TabIndex = 6;
			// 
			// QueryPeriodToDateEdit
			//
			this.BindingSource.SetBindingMember(this.QueryPeriodToDateEdit, "SendingObjectsCollection.PeriodTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).PeriodTo)));
			this.QueryPeriodToDateEdit.AllowDrop = true;
			this.QueryPeriodToDateEdit.AutoCompleteMonthThreshold = 1;
			this.QueryPeriodToDateEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("7558ab96-aca9-4819-94c5-eb2970bf8e2a", "To");
			this.QueryPeriodToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(713, 10, true);
			this.QueryPeriodToDateEdit.Name = "QueryPeriodToDateEdit";
			this.QueryPeriodToDateEdit.TabIndex = 7;
			// 
			// RequesterIDTextBox
			//
			this.BindingSource.SetBindingMember(this.RequesterIDTextBox, "SendingObjectsCollection.RequesterId");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RequesterId)));
			this.RequesterIDTextBox.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("05840318-9bd4-4ccd-92d1-9c6a26294196", "Requester");
			this.RequesterIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(137, 40, true);
			this.RequesterIDTextBox.Name = "RequesterIDTextBox";
			this.RequesterIDTextBox.ReadOnly = true;
			this.RequesterIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.RequesterIDTextBox.TabIndex = 8;
			// 
			// RequesterRoleDropEdit
			//
			this.BindingSource.SetBindingMember(this.RequesterRoleDropEdit, "SendingObjectsCollection.RequesterRole");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((TP5MessageSendingObject)(((System.Collections.IList)(((TP5MessageSendingObjectParent)(null)).SendingObjectsCollection)).SyncRoot)).RequesterRole)));
			this.RequesterRoleDropEdit.AllowDrop = true;
			this.RequesterRoleDropEdit.CaptionResourceString = Enterprise.Customs.FR.GUI.Res.GetData("7f0312b2-4098-4f54-a5c9-b5326041db8e", "Role");
			this.RequesterRoleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(527, 40, true);
			this.RequesterRoleDropEdit.Name = "RequesterRoleDropEdit";
			this.RequesterRoleDropEdit.ShowDescriptionBox = true;
			this.RequesterRoleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 20, true);
			this.RequesterRoleDropEdit.TabIndex = 9;
			// 
			// MessageSendingFormBottomSectionUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.QueryIdentifierDropEdit);
			this.Controls.Add(this.QueryPeriodToDateEdit);
			this.Controls.Add(this.QueryPeriodFromDateEdit);
			this.Controls.Add(this.RequesterRoleDropEdit);
			this.Controls.Add(this.RequesterIDTextBox);
			this.Controls.Add(this.ActualConsigneeDocAddressControl);
			this.Controls.Add(this.ActualOfficeOfDestinationFindBox);
			this.Controls.Add(this.JustificationTextBox);
			this.Controls.Add(this.QueryInformationTextBox);
			this.Controls.Add(this.TC11DeliveryDate);
			this.Controls.Add(this.ActualConsigneeLabel);
			this.Name = "MessageSendingFormBottomSectionUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(707, 233, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ActualConsigneeDocAddressControl.ResumeLayout(true);
			this.ActualConsigneeDocAddressControl.PerformLayout();
			this.ActualOfficeOfDestinationFindBox.ResumeLayout(true);
			this.ActualOfficeOfDestinationFindBox.PerformLayout();
			this.TC11DeliveryDate.ResumeLayout(true);
			this.TC11DeliveryDate.PerformLayout();
			this.RequesterRoleDropEdit.ResumeLayout(true);
			this.RequesterRoleDropEdit.PerformLayout();
			this.QueryPeriodFromDateEdit.ResumeLayout(true);
			this.QueryPeriodFromDateEdit.PerformLayout();
			this.QueryPeriodToDateEdit.ResumeLayout(true);
			this.QueryPeriodToDateEdit.PerformLayout();
			this.QueryIdentifierDropEdit.ResumeLayout(true);
			this.QueryIdentifierDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal ZArchitecture.ZTextBox JustificationTextBox;
		internal ZArchitecture.GUI.ZDateEdit TC11DeliveryDate;
		internal ZArchitecture.ZTextBox QueryInformationTextBox;
		internal ZCodeFindBox ActualOfficeOfDestinationFindBox;
		internal ZDocAddressControl ActualConsigneeDocAddressControl;
		internal ZLabel ActualConsigneeLabel;
		internal ZTextBox RequesterIDTextBox;
		internal ZDropEdit RequesterRoleDropEdit;
		internal ZDateEdit QueryPeriodFromDateEdit;
		internal ZDateEdit QueryPeriodToDateEdit;
		internal ZDropEdit QueryIdentifierDropEdit;
	}
}
