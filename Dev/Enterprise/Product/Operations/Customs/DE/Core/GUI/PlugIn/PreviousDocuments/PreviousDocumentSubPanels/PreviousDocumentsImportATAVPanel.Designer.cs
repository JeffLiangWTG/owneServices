using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.DE.GUI.PlugIn
{
	partial class PreviousDocumentsImportATAVPanel
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
			this.AuthorizationNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MonitoringCustomsOfficeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.SimplifiedGrantAuthorizationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AuthorizationNumberDropEdit.SuspendLayout();
			this.MonitoringCustomsOfficeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction);
			// 
			// AuthorizationNumberDropEdit
			// 
			this.AuthorizationNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationNumberDropEdit, "PreviousDocumentMaster.AuthorizationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).PreviousDocumentMaster.AuthorizationNumber)));
			this.AuthorizationNumberDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("fe65399c-d963-478c-99c3-9ac8f8a50b67", "Authorization Number");
			this.AuthorizationNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(133, 29, true);
			this.AuthorizationNumberDropEdit.Name = "AuthorizationNumberDropEdit";
			this.AuthorizationNumberDropEdit.ShouldResizeByMaxLength = true;
			this.AuthorizationNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
			this.AuthorizationNumberDropEdit.TabIndex = 1;
			// 
			// MonitoringCustomsOfficeFindBox
			// 
			this.MonitoringCustomsOfficeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MonitoringCustomsOfficeFindBox, "PreviousDocumentMaster.CSI_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).PreviousDocumentMaster.CSI_CustomsOffice)));
			this.MonitoringCustomsOfficeFindBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ccb54e38-b8ce-4faf-b668-24441f158f79", "Monitoring Customs Office");
			this.MonitoringCustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(508, 29, true);
			this.MonitoringCustomsOfficeFindBox.Name = "MonitoringCustomsOfficeFindBox";
			this.MonitoringCustomsOfficeFindBox.ShouldResize = true;
			this.MonitoringCustomsOfficeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(274, 20, true);
			this.MonitoringCustomsOfficeFindBox.TabIndex = 2;
			// 
			// SimplifiedGrantAuthorizationCheckBox
			// 
			this.SimplifiedGrantAuthorizationCheckBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SimplifiedGrantAuthorizationCheckBox, "PreviousDocumentMaster.SimplifiedGrantAuthorizationFlag");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).PreviousDocumentMaster.SimplifiedGrantAuthorizationFlag)));
			this.SimplifiedGrantAuthorizationCheckBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("dd0dfa8c-4ce7-41b8-9260-b4eefa447b97", "Simplified Grant Authorization?");
			this.SimplifiedGrantAuthorizationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.SimplifiedGrantAuthorizationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SimplifiedGrantAuthorizationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 3, true);
			this.SimplifiedGrantAuthorizationCheckBox.Name = "SimplifiedGrantAuthorizationCheckBox";
			this.SimplifiedGrantAuthorizationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(173, 20, true);
			this.SimplifiedGrantAuthorizationCheckBox.TabIndex = 3;
			// 
			// PreviousDocumentsImportATAVPanel
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SimplifiedGrantAuthorizationCheckBox);
			this.Controls.Add(this.AuthorizationNumberDropEdit);
			this.Controls.Add(this.MonitoringCustomsOfficeFindBox);
			this.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 43, true);
			this.Name = "PreviousDocumentsImportATAVPanel";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(813, 52, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AuthorizationNumberDropEdit.ResumeLayout(true);
			this.AuthorizationNumberDropEdit.PerformLayout();
			this.MonitoringCustomsOfficeFindBox.ResumeLayout(true);
			this.MonitoringCustomsOfficeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZArchitecture.GUI.ZDropEdit AuthorizationNumberDropEdit;
		public ZArchitecture.GUI.ZCodeFindBox MonitoringCustomsOfficeFindBox;
		public ZArchitecture.GUI.ZCheckBox SimplifiedGrantAuthorizationCheckBox;
	}
}
