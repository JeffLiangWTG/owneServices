using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.Module.OperationalActions
{
	partial class DeclarationUpdatePreviousDocumentsApplicatorControl
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
			this.DocumentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ClassDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.LineNoCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.OverrideExistingDocumentCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocumentCodeDropEdit.SuspendLayout();
			this.ClassDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator);
			// 
			// DocumentCodeDropEdit
			// 
			this.DocumentCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocumentCodeDropEdit, "DocumentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator)(null)).DocumentCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator)(null)).DocumentCodeList)));
			this.DocumentCodeDropEdit.BindToList = "DocumentCodeList";
			this.DocumentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 16, true);
			this.DocumentCodeDropEdit.Name = "DocumentCodeDropEdit";
			this.DocumentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.DocumentCodeDropEdit.TabIndex = 0;
			// 
			// ReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReferenceTextBox, "ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator)(null)).ReferenceNumber)));
			this.ReferenceTextBox.CaptionResourceString = null;
			this.ReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 43, true);
			this.ReferenceTextBox.Name = "ReferenceTextBox";
			this.ReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.ReferenceTextBox.TabIndex = 1;
			// 
			// ClassDropEdit
			// 
			this.ClassDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ClassDropEdit, "Class");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator)(null)).Class)));
			this.ClassDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 70, true);
			this.ClassDropEdit.Name = "ClassDropEdit";
			this.ClassDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(300, 20, true);
			this.ClassDropEdit.TabIndex = 2;
			// 
			// LineNoCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.LineNoCalcEdit, "LineNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator)(null)).LineNo)));
			this.LineNoCalcEdit.CaptionResourceString = null;
			this.LineNoCalcEdit.DecimalPlaces = 0;
			this.LineNoCalcEdit.Decimals = 0;
			this.LineNoCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 97, true);
			this.LineNoCalcEdit.Name = "LineNoCalcEdit";
			this.LineNoCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
			this.LineNoCalcEdit.TabIndex = 3;
			this.LineNoCalcEdit.Text = "0";
			this.LineNoCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// OverrideExistingDocumentCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideExistingDocumentCheckBox, "OverrideExisitingDocument");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.OperationalActions.DeclarationUpdatePreviousDocumentsApplicator)(null)).OverrideExisitingDocument)));
			this.OverrideExistingDocumentCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OverrideExistingDocumentCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 123, true);
			this.OverrideExistingDocumentCheckBox.Name = "OverrideExistingDocumentCheckBox";
			this.OverrideExistingDocumentCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(247, 24, true);
			this.OverrideExistingDocumentCheckBox.TabIndex = 4;
			// 
			// DeclarationUpdatePreviousDocumentsApplicatorControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.OverrideExistingDocumentCheckBox);
			this.Controls.Add(this.DocumentCodeDropEdit);
			this.Controls.Add(this.ReferenceTextBox);
			this.Controls.Add(this.ClassDropEdit);
			this.Controls.Add(this.LineNoCalcEdit);
			this.Name = "DeclarationUpdatePreviousDocumentsApplicatorControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 282, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocumentCodeDropEdit.ResumeLayout(true);
			this.DocumentCodeDropEdit.PerformLayout();
			this.ClassDropEdit.ResumeLayout(true);
			this.ClassDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZDropEdit DocumentCodeDropEdit;
		ZArchitecture.ZTextBox ReferenceTextBox;
		ZDropEdit ClassDropEdit;
		ZArchitecture.ZCalcEdit LineNoCalcEdit;

		#endregion

		private ZCheckBox OverrideExistingDocumentCheckBox;
	}
}
