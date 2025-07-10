using static Enterprise.ZArchitecture.GUI.ZDropEdit;

namespace Enterprise.Customs.ES.Manifest.H7.GUI
{
	partial class ESH7ManifestFieldsUserControl
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
			this.CusAgentCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.CertificateDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.TrainingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.LocationOfGoodsUserControl = new EU.GUI.LocationOfGoodsUserControl();
			this.TransportDocumentReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TransportDocumentTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.G3MRNToRevokeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryLineNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CusAgentCodeFindBox.SuspendLayout();
			this.CertificateDropEdit.SuspendLayout();
			this.TrainingCheckBox.SuspendLayout();
			this.TransportDocumentReferenceTextBox.SuspendLayout();
			this.TransportDocumentTypeDropEdit.SuspendLayout();
			this.G3MRNToRevokeDropEdit.SuspendLayout();
			this.EntryLineNumberTextBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader);
			// 
			// CusAgentCodeFindBox
			// 
			this.CusAgentCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CusAgentCodeFindBox, "AMA_GS_NKCustomsAgent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader)(null)).AMA_GS_NKCustomsAgent)));
			this.CusAgentCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(671, 266, true);
			this.CusAgentCodeFindBox.Name = "CusAgentCodeFindBox";
			this.CusAgentCodeFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.CusAgentCodeFindBox.ParentType = null;
			this.CusAgentCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(201, 18, true);
			this.CusAgentCodeFindBox.TabIndex = 1;
			// 
			// CertificateDropEdit
			// 
			this.CertificateDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CertificateDropEdit, "AMA_CustomsProfile");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader)(null)).AMA_CustomsProfile)));
			this.CertificateDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1074, 39, true);
			this.CertificateDropEdit.Name = "CertificateDropEdit";
			this.CertificateDropEdit.PreBoundMaxLength = 27;
			this.CertificateDropEdit.ShouldResizeByMaxLength = true;
			this.CertificateDropEdit.ShowDescriptionBox = false;
			this.CertificateDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.CertificateDropEdit.TabIndex = 2;
			// 
			// TrainingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.TrainingCheckBox, "TrainingEntry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader)(null)).TrainingEntry)));
			this.TrainingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.TrainingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.TrainingCheckBox.Name = "TrainingCheckBox";
			this.TrainingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.TrainingCheckBox.TabIndex = 3;
			this.TrainingCheckBox.UseVisualStyleBackColor = true;
			//
			//LocationOfGoodsUserControl
			//
			this.LocationOfGoodsUserControl.AllowDrop = true;
			this.LocationOfGoodsUserControl.Name = "LocationOfGoodsUserControl";
			// 
			// TransportDocumentTypeDropEdit
			// 
			this.TransportDocumentTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDocumentTypeDropEdit, "TransportDocumentType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader)(null)).TransportDocumentType)));
			this.TransportDocumentTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1074, 39, true);
			this.TransportDocumentTypeDropEdit.Name = "TransportDocumentTypeDropEdit";
			this.TransportDocumentTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(182, 20, true);
			this.TransportDocumentTypeDropEdit.TabIndex = 5;
			// 
			// TransportDocumentReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.TransportDocumentReferenceTextBox, "TransportDocumentReference");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader)(null)).TransportDocumentReference)));
			this.TransportDocumentReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.TransportDocumentReferenceTextBox.Name = "TransportDocumentReferenceTextBox";
			this.TransportDocumentReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.TransportDocumentReferenceTextBox.TabIndex = 4;
			// 
			// G3MRNToRevokeTextBox
			//
			this.BindingSource.SetBindingMember(this.G3MRNToRevokeDropEdit, "G3MRNToRevoke");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader)(null)).G3MRNToRevoke)));
			this.G3MRNToRevokeDropEdit.AllowDrop = true;
			this.G3MRNToRevokeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.G3MRNToRevokeDropEdit.Name = "G3MRNToRevokeDropEdit";
			this.G3MRNToRevokeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.G3MRNToRevokeDropEdit.ShowDescriptionBox = false;
			this.G3MRNToRevokeDropEdit.ShowInDropDown = ShowInDropDownList.OnlyShowCode;
			this.G3MRNToRevokeDropEdit.TabIndex = 6;
			// 
			// EntryLineNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.EntryLineNumberTextBox, "EntryLineNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Manifest.H7.Business.AsycudaManifestHeader)(null)).EntryLineNumber)));
			this.EntryLineNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(78, 86, true);
			this.EntryLineNumberTextBox.Name = "EntryLineNumberTextBox";
			this.EntryLineNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 19, true);
			this.EntryLineNumberTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.NumericOnly_KeyPress);
			this.EntryLineNumberTextBox.TabIndex = 7;
			//
			// ESH7ManifestFieldsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "ESH7ManifestFieldsUserControl";
			this.Controls.Add(this.CusAgentCodeFindBox);
			this.Controls.Add(this.CertificateDropEdit);
			this.Controls.Add(this.TrainingCheckBox);
			this.Controls.Add(this.LocationOfGoodsUserControl);
			this.Controls.Add(this.TransportDocumentReferenceTextBox);
			this.Controls.Add(this.TransportDocumentTypeDropEdit);
			this.Controls.Add(this.G3MRNToRevokeDropEdit);
			this.Controls.Add(this.EntryLineNumberTextBox);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CusAgentCodeFindBox.ResumeLayout(true);
			this.CusAgentCodeFindBox.PerformLayout();
			this.CertificateDropEdit.ResumeLayout(true);
			this.CertificateDropEdit.PerformLayout();
			this.TrainingCheckBox.ResumeLayout(true);
			this.TrainingCheckBox.PerformLayout();
			this.TransportDocumentReferenceTextBox.ResumeLayout(true);
			this.TransportDocumentReferenceTextBox.PerformLayout();
			this.TransportDocumentTypeDropEdit.ResumeLayout(true);
			this.TransportDocumentTypeDropEdit.PerformLayout();
			this.G3MRNToRevokeDropEdit.ResumeLayout(true);
			this.G3MRNToRevokeDropEdit.PerformLayout();
			this.EntryLineNumberTextBox.ResumeLayout(true);
			this.EntryLineNumberTextBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		internal ZArchitecture.GUI.ZCodeFindBox CusAgentCodeFindBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit CertificateDropEdit;
		internal ZArchitecture.GUI.ZCheckBox TrainingCheckBox;
		internal EU.GUI.LocationOfGoodsUserControl LocationOfGoodsUserControl;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit TransportDocumentTypeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox TransportDocumentReferenceTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDropEdit G3MRNToRevokeDropEdit;
		internal Enterprise.ZArchitecture.ZTextBox EntryLineNumberTextBox;
	}
}
