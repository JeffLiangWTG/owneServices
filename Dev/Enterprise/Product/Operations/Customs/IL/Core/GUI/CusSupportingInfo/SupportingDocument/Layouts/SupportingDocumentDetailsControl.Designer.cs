using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.IL.GUI
{
	partial class SupportingDocumentDetailsControl
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
			this.typeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.referenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.statusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.additionalDescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.eDocGuidDropEditGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.customsDocIDTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.typeDropEdit.SuspendLayout();
			this.eDocGuidDropEditGuidDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.IL.Business.SupportingDocument);
			// 
			// typeDropEdit
			// 
			this.typeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.typeDropEdit, "CSI_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).CSI_Code)));
			this.typeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 27, true);
			this.typeDropEdit.Name = "typeDropEdit";
			this.typeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.typeDropEdit.TabIndex = 0;
			// 
			// referenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.referenceNumberTextBox, "CSI_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).CSI_ReferenceNumber)));
			this.referenceNumberTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.referenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 62, true);
			this.referenceNumberTextBox.Name = "referenceNumberTextBox";
			this.referenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.referenceNumberTextBox.TabIndex = 1;
			// 
			// statusTextBox
			// 
			this.BindingSource.SetBindingMember(this.statusTextBox, "CSI_Status");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).CSI_Status)));
			this.statusTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.statusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(635, 27, true);
			this.statusTextBox.Name = "statusTextBox";
			this.statusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.statusTextBox.TabIndex = 3;
			// 
			// additionalDescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.additionalDescriptionTextBox, "CSI_AdditionalDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).CSI_AdditionalDescription)));
			this.additionalDescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.additionalDescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(635, 62, true);
			this.additionalDescriptionTextBox.Name = "additionalDescriptionTextBox";
			this.additionalDescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.additionalDescriptionTextBox.TabIndex = 4;
			// 
			// eDocGuidDropEditGuidDropEdit
			// 
			this.eDocGuidDropEditGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.eDocGuidDropEditGuidDropEdit, "EDoc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).EDoc)));
			this.eDocGuidDropEditGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 102, true);
			this.eDocGuidDropEditGuidDropEdit.Name = "eDocGuidDropEditGuidDropEdit";
			this.eDocGuidDropEditGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.eDocGuidDropEditGuidDropEdit.TabIndex = 6;
			// 
			// customsDocIDTextBox
			// 
			this.BindingSource.SetBindingMember(this.customsDocIDTextBox, "CSI_ReferenceNumber2");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.IL.Business.SupportingDocument)(null)).CSI_ReferenceNumber2)));
			this.customsDocIDTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.customsDocIDTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(177, 62, true);
			this.customsDocIDTextBox.Name = "customsDocIDTextBox";
			this.customsDocIDTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.customsDocIDTextBox.TabIndex = 7;
			// 
			// SupportingDocumentDetailsControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.eDocGuidDropEditGuidDropEdit);
			this.Controls.Add(this.additionalDescriptionTextBox);
			this.Controls.Add(this.statusTextBox);
			this.Controls.Add(this.referenceNumberTextBox);
			this.Controls.Add(this.typeDropEdit);
			this.Controls.Add(this.customsDocIDTextBox);
			this.Name = "SupportingDocumentDetailsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1118, 421, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.typeDropEdit.ResumeLayout(true);
			this.typeDropEdit.PerformLayout();
			this.eDocGuidDropEditGuidDropEdit.ResumeLayout(true);
			this.eDocGuidDropEditGuidDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZDropEdit typeDropEdit;
		internal ZArchitecture.ZTextBox referenceNumberTextBox;
		internal ZArchitecture.ZTextBox statusTextBox;
		internal ZArchitecture.ZTextBox additionalDescriptionTextBox;
		internal ZGuidDropEdit eDocGuidDropEditGuidDropEdit;
		internal ZArchitecture.ZTextBox customsDocIDTextBox;
	}
}
