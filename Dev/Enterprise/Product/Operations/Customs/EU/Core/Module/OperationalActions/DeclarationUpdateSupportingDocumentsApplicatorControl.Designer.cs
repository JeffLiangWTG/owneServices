using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module.OperationalActions
{
	partial class DeclarationUpdateSupportingDocumentsApplicatorControl
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
            this.DocumentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.DateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.OverrideExistingDocumentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.DocumentCodeFindBox.SuspendLayout();
            this.DateEdit.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdateSupportingDocumentsApplicator);
            // 
            // DocumentCodeFindBox
            // 
            this.DocumentCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DocumentCodeFindBox, "DocumentCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdateSupportingDocumentsApplicator)(null)).DocumentCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdateSupportingDocumentsApplicator)(null)).DocumentCodeList)));
            this.DocumentCodeFindBox.BindToList = "DocumentCodeList";
            this.DocumentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
            this.DocumentCodeFindBox.Name = "DocumentCodeFindBox";
            this.DocumentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.DocumentCodeFindBox.ParentType = null;
            this.DocumentCodeFindBox.ShowDescriptionBox = false;
            this.DocumentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
            this.DocumentCodeFindBox.TabIndex = 0;
            // 
            // ReferenceTextBox
            // 
            this.BindingSource.SetBindingMember(this.ReferenceTextBox, "ReferenceNumber");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdateSupportingDocumentsApplicator)(null)).ReferenceNumber)));
            this.ReferenceTextBox.CaptionResourceString = null;
            this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 43, true);
            this.ReferenceTextBox.Name = "ReferenceTextBox";
            this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
            this.ReferenceTextBox.TabIndex = 1;
            // 
            // DateEdit
            // 
            this.DateEdit.AllowDrop = true;
            this.DateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.DateEdit, "Date");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdateSupportingDocumentsApplicator)(null)).Date)));
            this.DateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 70, true);
            this.DateEdit.Name = "DateEdit";
            this.DateEdit.TabIndex = 2;
            // 
            // OverrideExistingDocumentCheckBox
            // 
            this.BindingSource.SetBindingMember(this.OverrideExistingDocumentCheckBox, "OverrideExisitingDocument");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdateSupportingDocumentsApplicator)(null)).OverrideExisitingDocument)));
            this.OverrideExistingDocumentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.OverrideExistingDocumentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 98, true);
            this.OverrideExistingDocumentCheckBox.Name = "OverrideExistingDocumentCheckBox";
            this.OverrideExistingDocumentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 24, true);
            this.OverrideExistingDocumentCheckBox.TabIndex = 3;
            // 
            // DeclarationUpdateSupportingDocumentsApplicatorControl
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.DocumentCodeFindBox);
            this.Controls.Add(this.ReferenceTextBox);
            this.Controls.Add(this.DateEdit);
            this.Controls.Add(this.OverrideExistingDocumentCheckBox);
            this.Name = "DeclarationUpdateSupportingDocumentsApplicatorControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 282, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.DocumentCodeFindBox.ResumeLayout(true);
            this.DocumentCodeFindBox.PerformLayout();
            this.DateEdit.ResumeLayout(true);
            this.DateEdit.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		ZCodeFindBox DocumentCodeFindBox;
		ZArchitecture.ZTextBox ReferenceTextBox;
		ZDateEdit DateEdit;
		ZCheckBox OverrideExistingDocumentCheckBox;
		#endregion
	}
}
