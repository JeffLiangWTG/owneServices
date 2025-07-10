using Enterprise.Customs.EU.TemporaryStorage.Business;

namespace Enterprise.Customs.EU.TemporaryStorage.GUI
{
	partial class TempStoragePremisesDetailsUserControl
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
			this.CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AuthorizationNumberCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AuthorizationOwnerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.PremisesAddressAddressControl = new Enterprise.ZArchitecture.GUI.ZAddressControl();
			this.CustomsLocationCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TypeDropEdit.SuspendLayout();
			this.AuthorizationNumberCodeFindBox.SuspendLayout();
			this.AuthorizationOwnerGuidFindBox.SuspendLayout();
			this.PremisesAddressAddressControl.SuspendLayout();
			this.CustomsLocationCodeFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(CusTempStorageRegPremises);
			// 
			// CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.CodeTextBox, "SRP_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusTempStorageRegPremises)(null)).SRP_Code)));
			this.CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 17, true);
			this.CodeTextBox.Name = "CodeTextBox";
			this.CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.CodeTextBox.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "SRP_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusTempStorageRegPremises)(null)).SRP_Description)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 43, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(33, 20, true);
			this.DescriptionTextBox.TabIndex = 2;
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "SRP_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((CusTempStorageRegPremises)(null)).SRP_Type)));
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 69, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.TypeDropEdit.TabIndex = 3;
			// 
			// AuthorizationNumberCodeFindBox
			// 
			this.AuthorizationNumberCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationNumberCodeFindBox, "AuthorizationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusTempStorageRegPremises)(null)).AuthorizationNumber)));
			this.AuthorizationNumberCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 95, true);
			this.AuthorizationNumberCodeFindBox.Name = "AuthorizationNumberCodeFindBox";
			this.AuthorizationNumberCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AuthorizationNumberCodeFindBox.ParentType = null;
			this.AuthorizationNumberCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.AuthorizationNumberCodeFindBox.TabIndex = 4;
			// 
			// AuthorizationOwnerGuidFindBox
			// 
			this.AuthorizationOwnerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorizationOwnerGuidFindBox, "AuthorizationOwner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CusTempStorageRegPremises)(null)).AuthorizationOwner)));
			this.AuthorizationOwnerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 121, true);
			this.AuthorizationOwnerGuidFindBox.Name = "AuthorizationOwnerGuidFindBox";
			this.AuthorizationOwnerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.AuthorizationOwnerGuidFindBox.ParentType = null;
			this.AuthorizationOwnerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(149, 20, true);
			this.AuthorizationOwnerGuidFindBox.TabIndex = 5;
			// 
			// PremisesAddressAddressControl
			// 
			this.PremisesAddressAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PremisesAddressAddressControl, "SRP_OA_PremisesAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((CusTempStorageRegPremises)(null)).SRP_OA_PremisesAddress)));
			this.PremisesAddressAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 147, true);
			this.PremisesAddressAddressControl.Name = "PremisesAddressAddressControl";
			this.PremisesAddressAddressControl.PopupCaption = "";
			this.PremisesAddressAddressControl.ShowAddress = false;
			this.PremisesAddressAddressControl.ShowOrganisationName = true;
			this.PremisesAddressAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 20, true);
			this.PremisesAddressAddressControl.TabIndex = 6;
			// 
			// CustomsLocationCodeFindBox
			// 
			this.CustomsLocationCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsLocationCodeFindBox, "SRP_CustomsLocation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((CusTempStorageRegPremises)(null)).SRP_CustomsLocation)));
			this.CustomsLocationCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 173, true);
			this.CustomsLocationCodeFindBox.Name = "CustomsLocationCodeFindBox";
			this.CustomsLocationCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CustomsLocationCodeFindBox.ParentType = null;
			this.CustomsLocationCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.CustomsLocationCodeFindBox.TabIndex = 7;
			// 
			// TempStoragePremisesDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CodeTextBox);
			this.Controls.Add(this.DescriptionTextBox);
			this.Controls.Add(this.TypeDropEdit);
			this.Controls.Add(this.AuthorizationNumberCodeFindBox);
			this.Controls.Add(this.AuthorizationOwnerGuidFindBox);
			this.Controls.Add(this.PremisesAddressAddressControl);
			this.Controls.Add(this.CustomsLocationCodeFindBox);
			this.Name = "TempStoragePremisesDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 460, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.AuthorizationNumberCodeFindBox.ResumeLayout(true);
			this.AuthorizationNumberCodeFindBox.PerformLayout();
			this.AuthorizationOwnerGuidFindBox.ResumeLayout(true);
			this.AuthorizationOwnerGuidFindBox.PerformLayout();
			this.PremisesAddressAddressControl.ResumeLayout(true);
			this.PremisesAddressAddressControl.PerformLayout();
			this.CustomsLocationCodeFindBox.ResumeLayout(true);
			this.CustomsLocationCodeFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZTextBox CodeTextBox;
		internal ZArchitecture.ZTextBox DescriptionTextBox;
		internal ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		internal ZArchitecture.GUI.ZCodeFindBox AuthorizationNumberCodeFindBox;
		internal ZArchitecture.GUI.ZGuidFindBox AuthorizationOwnerGuidFindBox;
		internal ZArchitecture.GUI.ZAddressControl PremisesAddressAddressControl;
		internal ZArchitecture.GUI.ZCodeFindBox CustomsLocationCodeFindBox;
	}
}
