using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.JP.Manifest.GUI
{
	partial class JPManifestCountrySpecificUserControl
	{

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.InputReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MasterBillCustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MasterBillMessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsSubConsolidationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.IsCoLoadedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.MoveInDestinationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsAgentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.ViaLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CustomsAgentCredentialGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.ConsolidatorUserControl = new Enterprise.Customs.JP.Manifest.GUI.ConsolidatorUserControl();
			this.BookingNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PortOfDischargeUserControl = new Enterprise.Customs.JP.Manifest.GUI.PortOfDischargeUserControl();
			this.PortOfLoadingUserControl = new Enterprise.Customs.JP.Manifest.GUI.PortOfLoadingUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MoveInDestinationCodeFindBox.SuspendLayout();
			this.MasterBillCustomsStatusDropEdit.SuspendLayout();
			this.MasterBillMessageStatusDropEdit.SuspendLayout();
			this.CustomsAgentCodeFindBox.SuspendLayout();
			this.ViaLocationCodeFindBox.SuspendLayout();
			this.CustomsAgentCredentialGuidDropEdit.SuspendLayout();
			this.ConsolidatorUserControl.SuspendLayout();
			this.PortOfDischargeUserControl.SuspendLayout();
			this.PortOfLoadingUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader);
			// 
			// InputReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.InputReferenceTextBox, "AMA_InputReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).AMA_InputReference)));
			this.InputReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 3, true);
			this.InputReferenceTextBox.Name = "InputReferenceTextBox";
			this.InputReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.InputReferenceTextBox.TabIndex = 13;
			// 
			// MasterBillCustomsStatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.MasterBillCustomsStatusDropEdit, "MasterBillCustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).MasterBillCustomsStatus)));
			this.MasterBillCustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 3, true);
			this.MasterBillCustomsStatusDropEdit.Name = "MasterBillCustomsStatusDropEdit";
			this.MasterBillCustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MasterBillCustomsStatusDropEdit.PreBoundMaxLength = 3;
			// 
			// MasterBillMessageStatusDropEdit
			// 
			this.BindingSource.SetBindingMember(this.MasterBillMessageStatusDropEdit, "MasterBillMessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).MasterBillMessageStatus)));
			this.MasterBillMessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 3, true);
			this.MasterBillMessageStatusDropEdit.Name = "MasterBillMessageStatusDropEdit";
			this.MasterBillMessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.MasterBillMessageStatusDropEdit.PreBoundMaxLength = 3;
			// 
			// IsSubConsolidationCheckBox
			// 
			this.IsSubConsolidationCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsSubConsolidationCheckBox, "IsSubConsolidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).IsSubConsolidation)));
			this.IsSubConsolidationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSubConsolidationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 108, true);
			this.IsSubConsolidationCheckBox.Name = "IsSubConsolidationCheckBox";
			this.IsSubConsolidationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IsSubConsolidationCheckBox.TabIndex = 0;
			this.IsSubConsolidationCheckBox.UseVisualStyleBackColor = true;
			// 
			// IsCoLoadedCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsCoLoadedCheckBox, "IsCoLoaded");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).IsCoLoaded)));
			this.IsCoLoadedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsCoLoadedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 128, true);
			this.IsCoLoadedCheckBox.Name = "IsCoLoadedCheckBox";
			this.IsCoLoadedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.IsCoLoadedCheckBox.TabIndex = 9;
			// 
			// MoveInDestinationCodeFindBox
			// 
			this.MoveInDestinationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MoveInDestinationCodeFindBox, "AMA_RL_NKPortOfFinalDeparture");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).AMA_RL_NKPortOfFinalDeparture)));
			this.MoveInDestinationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 105, true);
			this.MoveInDestinationCodeFindBox.Name = "MoveInDestinationCodeFindBox";
			this.MoveInDestinationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.MoveInDestinationCodeFindBox.ParentType = null;
			this.MoveInDestinationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.MoveInDestinationCodeFindBox.TabIndex = 15;
			// 
			// CustomsAgentCodeFindBox
			// 
			this.CustomsAgentCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsAgentCodeFindBox, "AMA_GS_NKCustomsAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).AMA_GS_NKCustomsAgent)));
			this.CustomsAgentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 53, true);
			this.CustomsAgentCodeFindBox.Name = "CustomsAgentCodeFindBox";
			this.CustomsAgentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsAgentCodeFindBox.ParentType = null;
			this.CustomsAgentCodeFindBox.PreBoundMaxLength = 5;
			this.CustomsAgentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.CustomsAgentCodeFindBox.TabIndex = 11;
			// 
			// ViaLocationCodeFindBox
			// 
			this.ViaLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ViaLocationCodeFindBox, "ViaLocationCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).ViaLocationCode)));
			this.ViaLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 79, true);
			this.ViaLocationCodeFindBox.Name = "ViaLocationCodeFindBox";
			this.ViaLocationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ViaLocationCodeFindBox.ParentType = null;
			this.ViaLocationCodeFindBox.PreBoundMaxLength = 5;
			this.ViaLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.ViaLocationCodeFindBox.TabIndex = 15;
			// 
			// CustomsAgentCredentialGuidDropEdit
			// 
			this.CustomsAgentCredentialGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsAgentCredentialGuidDropEdit, "AMA_CustomsAgentCredentialPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).AMA_CustomsAgentCredentialPK)));
			this.CustomsAgentCredentialGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 29, true);
			this.CustomsAgentCredentialGuidDropEdit.Name = "CustomsAgentCredentialGuidDropEdit";
			this.CustomsAgentCredentialGuidDropEdit.PreBoundMaxLength = 5;
			this.CustomsAgentCredentialGuidDropEdit.ShowDescriptionBox = false;
			this.CustomsAgentCredentialGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.CustomsAgentCredentialGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(73, 20, true);
			this.CustomsAgentCredentialGuidDropEdit.TabIndex = 12;
			// 
			// ConsolidatorUserControl
			// 
			this.ConsolidatorUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ConsolidatorUserControl, ".");
			this.ConsolidatorUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(197, 262, true);
			this.ConsolidatorUserControl.Name = "ConsolidatorUserControl";
			this.ConsolidatorUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 70, true);
			this.ConsolidatorUserControl.TabIndex = 10;
			// 
			// BookingNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.BookingNumberTextBox, "AMA_BookingNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Manifest.Business.AsycudaManifestHeader)(null)).AMA_BookingNumber)));
			this.BookingNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 79, true);
			this.BookingNumberTextBox.Name = "BookingNumberTextBox";
			this.BookingNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.BookingNumberTextBox.TabIndex = 14;
			// 
			// PortOfDischargeUserControl
			// 
			this.PortOfDischargeUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfDischargeUserControl, ".");
			this.PortOfDischargeUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.PortOfDischargeUserControl.Name = "PortOfDischargeUserControl";
			this.PortOfDischargeUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.PortOfDischargeUserControl.CaptionRenderingEnabled = true;
			this.PortOfDischargeUserControl.TabIndex = 16;
			// 
			// PortOfLoadingUserControl
			// 
			this.PortOfLoadingUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PortOfLoadingUserControl, ".");
			this.PortOfLoadingUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 29, true);
			this.PortOfLoadingUserControl.Name = "PortOfLoadingUserControl";
			this.PortOfLoadingUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.PortOfLoadingUserControl.CaptionRenderingEnabled = true;
			this.PortOfLoadingUserControl.TabIndex = 17;
			// 
			// JPManifestCountrySpecificUserControl
			// 
			this.Controls.Add(this.PortOfLoadingUserControl);
			this.Controls.Add(this.PortOfDischargeUserControl);
			this.Controls.Add(this.BookingNumberTextBox);
			this.Controls.Add(this.IsSubConsolidationCheckBox);
			this.Controls.Add(this.IsCoLoadedCheckBox);
			this.Controls.Add(this.ConsolidatorUserControl);
			this.Controls.Add(this.CustomsAgentCodeFindBox);
			this.Controls.Add(this.ViaLocationCodeFindBox);
			this.Controls.Add(this.CustomsAgentCredentialGuidDropEdit);
			this.Controls.Add(this.InputReferenceTextBox);
			this.Controls.Add(this.MasterBillCustomsStatusDropEdit);
			this.Controls.Add(this.MasterBillMessageStatusDropEdit);
			this.Controls.Add(this.MoveInDestinationCodeFindBox);
			this.Name = "JPManifestCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(485, 155, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MoveInDestinationCodeFindBox.ResumeLayout(true);
			this.MoveInDestinationCodeFindBox.PerformLayout();
			this.CustomsAgentCodeFindBox.ResumeLayout(true);
			this.CustomsAgentCodeFindBox.PerformLayout();
			this.ViaLocationCodeFindBox.ResumeLayout(true);
			this.ViaLocationCodeFindBox.PerformLayout();
			this.MasterBillCustomsStatusDropEdit.ResumeLayout(true);
			this.MasterBillCustomsStatusDropEdit.PerformLayout();
			this.MasterBillMessageStatusDropEdit.ResumeLayout(true);
			this.MasterBillMessageStatusDropEdit.PerformLayout();
			this.CustomsAgentCredentialGuidDropEdit.ResumeLayout(true);
			this.CustomsAgentCredentialGuidDropEdit.PerformLayout();
			this.ConsolidatorUserControl.ResumeLayout(true);
			this.ConsolidatorUserControl.PerformLayout();
			this.PortOfDischargeUserControl.ResumeLayout(true);
			this.PortOfDischargeUserControl.PerformLayout();
			this.PortOfLoadingUserControl.ResumeLayout(true);
			this.PortOfLoadingUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		ZArchitecture.ZTextBox InputReferenceTextBox;
		ZArchitecture.GUI.ZDropEdit MasterBillCustomsStatusDropEdit;
		ZArchitecture.GUI.ZDropEdit MasterBillMessageStatusDropEdit;
		ZArchitecture.GUI.ZCheckBox IsSubConsolidationCheckBox;
		ZArchitecture.GUI.ZCheckBox IsCoLoadedCheckBox;
		ConsolidatorUserControl ConsolidatorUserControl;
		ZArchitecture.GUI.ZCodeFindBox CustomsAgentCodeFindBox;
		ZArchitecture.GUI.ZCodeFindBox ViaLocationCodeFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidDropEdit CustomsAgentCredentialGuidDropEdit;
		ZArchitecture.ZTextBox BookingNumberTextBox;
		ZArchitecture.GUI.ZCodeFindBox MoveInDestinationCodeFindBox;
		PortOfDischargeUserControl PortOfDischargeUserControl;
		PortOfLoadingUserControl PortOfLoadingUserControl;
	}
}
