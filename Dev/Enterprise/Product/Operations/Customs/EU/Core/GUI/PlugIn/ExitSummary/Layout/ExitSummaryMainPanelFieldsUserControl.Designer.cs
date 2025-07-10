namespace Enterprise.Customs.EU.GUI.PlugIn
{
	partial class ExitSummaryMainPanelFieldsUserControl
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
			this.ReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderCustomsOfficeCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.HeaderArrivalNotificationDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.HeaderArrivalNotificationPlaceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.HeaderExitDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.HeaderTransportIdTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AgentOrgAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.HeaderCarrierOrgAddressControl = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.HeaderCustomsOfficeCodeFindBox.SuspendLayout();
			this.HeaderArrivalNotificationDateDateEdit.SuspendLayout();
			this.HeaderExitDateDateEdit.SuspendLayout();
			this.AgentOrgAddressControl.SuspendLayout();
			this.HeaderCarrierOrgAddressControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.CusExitControlHeader);
			// 
			// ReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceNumberTextBox, "CEH_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_ReferenceNumber)));
			this.ReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("93fbea06-3e63-4bec-a3bf-406e8cf8eae5", "Reference Number");
			this.ReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 13, true);
			this.ReferenceNumberTextBox.Name = "ReferenceNumberTextBox";
			this.ReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.ReferenceNumberTextBox.TabIndex = 0;
			// 
			// HeaderCustomsOfficeCodeFindBox
			// 
			this.HeaderCustomsOfficeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderCustomsOfficeCodeFindBox, "CEH_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_CustomsOffice)));
			this.HeaderCustomsOfficeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 39, true);
			this.HeaderCustomsOfficeCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Universal.ZZRefCusCodeList;
			this.HeaderCustomsOfficeCodeFindBox.Name = "HeaderCustomsOfficeCodeFindBox";
			this.HeaderCustomsOfficeCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.HeaderCustomsOfficeCodeFindBox.ParentType = null;
			this.HeaderCustomsOfficeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.HeaderCustomsOfficeCodeFindBox.TabIndex = 4;
			// 
			// HeaderArrivalNotificationDateDateEdit
			// 
			this.HeaderArrivalNotificationDateDateEdit.AllowDrop = true;
			this.HeaderArrivalNotificationDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.HeaderArrivalNotificationDateDateEdit, "CEH_ArrivalNotificationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_ArrivalNotificationDate)));
			this.HeaderArrivalNotificationDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 65, true);
			this.HeaderArrivalNotificationDateDateEdit.Name = "HeaderArrivalNotificationDateDateEdit";
			this.HeaderArrivalNotificationDateDateEdit.TabIndex = 5;
			// 
			// HeaderArrivalNotificationPlaceTextBox
			// 
			this.BindingSource.SetBindingMember(this.HeaderArrivalNotificationPlaceTextBox, "CEH_ArrivalNotificationPlace");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_ArrivalNotificationPlace)));
			this.HeaderArrivalNotificationPlaceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 91, true);
			this.HeaderArrivalNotificationPlaceTextBox.Name = "HeaderArrivalNotificationPlaceTextBox";
			this.HeaderArrivalNotificationPlaceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.HeaderArrivalNotificationPlaceTextBox.TabIndex = 6;
			// 
			// HeaderExitDateDateEdit
			// 
			this.HeaderExitDateDateEdit.AllowDrop = true;
			this.HeaderExitDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.HeaderExitDateDateEdit, "CEH_ExitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_ExitDate)));
			this.HeaderExitDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 117, true);
			this.HeaderExitDateDateEdit.Name = "HeaderExitDateDateEdit";
			this.HeaderExitDateDateEdit.TabIndex = 7;
			// 
			// HeaderTransportIdTextBox
			// 
			this.BindingSource.SetBindingMember(this.HeaderTransportIdTextBox, "CEH_TransportID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_TransportID)));
			this.HeaderTransportIdTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 143, true);
			this.HeaderTransportIdTextBox.Name = "HeaderTransportIdTextBox";
			this.HeaderTransportIdTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 20, true);
			this.HeaderTransportIdTextBox.TabIndex = 8;
			// 
			// AgentOrgAddressControl
			// 
			this.AgentOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AgentOrgAddressControl, "CEH_OA_Agent_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_OA_Agent_ZAddress)));
			this.AgentOrgAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("957ef1c2-b860-4fcd-b7f2-52feb55c22e5", "Agent");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AgentOrgAddressControl, false);
			this.AgentOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(492, 13, true);
			this.AgentOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AgentOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AgentOrgAddressControl.Name = "AgentOrgAddressControl";
			this.AgentOrgAddressControl.PopupCaption = "";
			this.AgentOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.AgentOrgAddressControl.TabIndex = 9;
			// 
			// HeaderCarrierOrgAddressControl
			// 
			this.HeaderCarrierOrgAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HeaderCarrierOrgAddressControl, "CEH_OA_Carrier_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.EU.Business.CusExitControlHeader)(null)).CEH_OA_Carrier_ZAddress)));
			this.HeaderCarrierOrgAddressControl.CaptionResourceString = Enterprise.Customs.EU.GUI.Res.GetData("10976774-0F77-4150-8EA4-46FFF89356F5", "Carrier");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.HeaderCarrierOrgAddressControl, false);
			this.HeaderCarrierOrgAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(759, 13, true);
			this.HeaderCarrierOrgAddressControl.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.HeaderCarrierOrgAddressControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.HeaderCarrierOrgAddressControl.Name = "HeaderCarrierOrgAddressControl";
			this.HeaderCarrierOrgAddressControl.PopupCaption = "";
			this.HeaderCarrierOrgAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.HeaderCarrierOrgAddressControl.TabIndex = 10;
			// 
			// ExitSummaryMainPanelFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ReferenceNumberTextBox);
			this.Controls.Add(this.HeaderCustomsOfficeCodeFindBox);
			this.Controls.Add(this.HeaderArrivalNotificationDateDateEdit);
			this.Controls.Add(this.HeaderArrivalNotificationPlaceTextBox);
			this.Controls.Add(this.HeaderExitDateDateEdit);
			this.Controls.Add(this.HeaderTransportIdTextBox);
			this.Controls.Add(this.AgentOrgAddressControl);
			this.Controls.Add(this.HeaderCarrierOrgAddressControl);
			this.Name = "ExitSummaryMainPanelFieldsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1101, 222, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.HeaderCustomsOfficeCodeFindBox.ResumeLayout(true);
			this.HeaderCustomsOfficeCodeFindBox.PerformLayout();
			this.HeaderArrivalNotificationDateDateEdit.ResumeLayout(true);
			this.HeaderArrivalNotificationDateDateEdit.PerformLayout();
			this.HeaderExitDateDateEdit.ResumeLayout(true);
			this.HeaderExitDateDateEdit.PerformLayout();
			this.AgentOrgAddressControl.ResumeLayout(true);
			this.AgentOrgAddressControl.PerformLayout();
			this.HeaderCarrierOrgAddressControl.ResumeLayout(true);
			this.HeaderCarrierOrgAddressControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox ReferenceNumberTextBox;
		internal ZArchitecture.GUI.ZCodeFindBox HeaderCustomsOfficeCodeFindBox;
		internal ZArchitecture.GUI.ZDateEdit HeaderArrivalNotificationDateDateEdit;
		internal ZArchitecture.ZTextBox HeaderArrivalNotificationPlaceTextBox;
		internal ZArchitecture.GUI.ZDateEdit HeaderExitDateDateEdit;
		internal ZArchitecture.ZTextBox HeaderTransportIdTextBox;
		internal MasterFiles.GUI.ZOrgAddressControl AgentOrgAddressControl;
		internal MasterFiles.GUI.ZOrgAddressControl HeaderCarrierOrgAddressControl;
	}
}
