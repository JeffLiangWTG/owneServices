namespace Enterprise.Customs.EU.GUI
{
	partial class ShipmentTypeUserControl
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
			this.CTStatusIDDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SpecificCircumstanceDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.EntryStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.SecurityDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsHighValueOvrdCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.BorderTransportMeansDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.IsSecurityDeclarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CTStatusIDDropEdit.SuspendLayout();
			this.SpecificCircumstanceDropEdit.SuspendLayout();
			this.EntryStyleDropEdit.SuspendLayout();
			this.SecurityDropEdit.SuspendLayout();
			this.BorderTransportMeansDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// CTStatusIDDropEdit
			// 
			this.CTStatusIDDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CTStatusIDDropEdit, "ZG_CTStatusID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_CTStatusID)));
			this.CTStatusIDDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 43, true);
			this.CTStatusIDDropEdit.Name = "CTStatusIDDropEdit";
			this.CTStatusIDDropEdit.PreBoundMaxLength = 3;
			this.CTStatusIDDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.CTStatusIDDropEdit.TabIndex = 19;
			// 
			// SpecificCircumstanceDropEdit
			// 
			this.SpecificCircumstanceDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SpecificCircumstanceDropEdit, "ZG_SpecificCircumstanceIndicator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_SpecificCircumstanceIndicator)));
			this.SpecificCircumstanceDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 69, true);
			this.SpecificCircumstanceDropEdit.Name = "SpecificCircumstanceDropEdit";
			this.SpecificCircumstanceDropEdit.PreBoundMaxLength = 3;
			this.SpecificCircumstanceDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.SpecificCircumstanceDropEdit.TabIndex = 20;
			// 
			// EntryStyleDropEdit
			// 
			this.EntryStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryStyleDropEdit, "JE_EntryStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).JE_EntryStyle)));
			this.EntryStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 17, true);
			this.EntryStyleDropEdit.Name = "EntryStyleDropEdit";
			this.EntryStyleDropEdit.PreBoundMaxLength = 3;
			this.EntryStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.EntryStyleDropEdit.TabIndex = 18;
			// 
			// IsHighValueOvrdCheckBox
			// 
			this.IsHighValueOvrdCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsHighValueOvrdCheckBox, "ZG_IsHighValueOvrd");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_IsHighValueOvrd)));
			this.IsHighValueOvrdCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 95, true);
			this.IsHighValueOvrdCheckBox.Name = "IsHighValueOvrdCheckBox";
			this.IsHighValueOvrdCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(59, 17, true);
			this.IsHighValueOvrdCheckBox.TabIndex = 21;
			this.IsHighValueOvrdCheckBox.UseVisualStyleBackColor = true;
			// 
			// BorderTransportMeansDropEdit
			// 
			this.BorderTransportMeansDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BorderTransportMeansDropEdit, "ZG_BorderTransportMeans");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_BorderTransportMeans)));
			this.BorderTransportMeansDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 115, true);
			this.BorderTransportMeansDropEdit.Name = "BorderTransportMeansDropEdit";
			this.BorderTransportMeansDropEdit.PreBoundMaxLength = 2;
			this.BorderTransportMeansDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.BorderTransportMeansDropEdit.TabIndex = 22;
			// 
			// IsSecurityDeclarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsSecurityDeclarationCheckBox, "ZG_IsSecurityDeclaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_IsSecurityDeclaration)));
			this.IsSecurityDeclarationCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.IsSecurityDeclarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 139, true);
			this.IsSecurityDeclarationCheckBox.Name = "IsSecurityDeclarationCheckBox";
			this.IsSecurityDeclarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 24, true);
			this.IsSecurityDeclarationCheckBox.TabIndex = 23;
			this.IsSecurityDeclarationCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// SecurityDropEdit
			// 
			this.SecurityDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SecurityDropEdit, "ZG_TypeOfSecurity");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobDeclaration)(null)).ZG_TypeOfSecurity)));
			this.SecurityDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 167, true);
			this.SecurityDropEdit.Name = "SecurityDropEdit";
			this.SecurityDropEdit.PreBoundMaxLength = 3;
			this.SecurityDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 20, true);
			this.SecurityDropEdit.TabIndex = 24;
			// 
			// ShipmentTypeUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BorderTransportMeansDropEdit);
			this.Controls.Add(this.IsHighValueOvrdCheckBox);
			this.Controls.Add(this.EntryStyleDropEdit);
			this.Controls.Add(this.SpecificCircumstanceDropEdit);
			this.Controls.Add(this.CTStatusIDDropEdit);
			this.Controls.Add(this.IsSecurityDeclarationCheckBox);
			this.Controls.Add(this.SecurityDropEdit);
			this.Name = "ShipmentTypeUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 200, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CTStatusIDDropEdit.ResumeLayout(true);
			this.CTStatusIDDropEdit.PerformLayout();
			this.SpecificCircumstanceDropEdit.ResumeLayout(true);
			this.SpecificCircumstanceDropEdit.PerformLayout();
			this.EntryStyleDropEdit.ResumeLayout(true);
			this.EntryStyleDropEdit.PerformLayout();
			this.BorderTransportMeansDropEdit.ResumeLayout(true);
			this.BorderTransportMeansDropEdit.PerformLayout();
			this.SecurityDropEdit.ResumeLayout(true);
			this.SecurityDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit CTStatusIDDropEdit;
		internal ZArchitecture.GUI.ZDropEdit SpecificCircumstanceDropEdit;
		internal ZArchitecture.GUI.ZDropEdit EntryStyleDropEdit;
		internal ZArchitecture.GUI.ZCheckBox IsHighValueOvrdCheckBox;
		internal ZArchitecture.GUI.ZDropEdit BorderTransportMeansDropEdit;
		internal ZArchitecture.GUI.ZCheckBox IsSecurityDeclarationCheckBox;
		internal ZArchitecture.GUI.ZDropEdit SecurityDropEdit;
	}
}
