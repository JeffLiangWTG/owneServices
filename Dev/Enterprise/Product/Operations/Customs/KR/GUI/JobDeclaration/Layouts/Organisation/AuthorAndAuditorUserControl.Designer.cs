
namespace Enterprise.Customs.KR.GUI
{
	partial class AuthorAndAuditorUserControl
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
			this.AuthorGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.AuthorNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorJobTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuditorPhoneTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuditorJobTitleTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuditorNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuditorGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AuthorGuidDropEdit.SuspendLayout();
			this.AuditorGuidDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// AuthorGuidDropEdit
			// 
			this.AuthorGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorGuidDropEdit, "VDAuthor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).VDAuthor)));
			this.AuthorGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 43, true);
			this.AuthorGuidDropEdit.Name = "AuthorGuidDropEdit";
			this.AuthorGuidDropEdit.ShowDescriptionBox = false;
			this.AuthorGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AuthorGuidDropEdit.TabIndex = 0;
			// 
			// AuthorNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorNameTextBox, "JE_AuthorName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_AuthorName)));
			this.AuthorNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AuthorNameTextBox, false);
			this.AuthorNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 43, true);
			this.AuthorNameTextBox.Name = "AuthorNameTextBox";
			this.AuthorNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.AuthorNameTextBox.TabIndex = 1;
			// 
			// AuthorJobTitleTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorJobTitleTextBox, "JE_AuthorJobTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_AuthorJobTitle)));
			this.AuthorJobTitleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AuthorJobTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 95, true);
			this.AuthorJobTitleTextBox.Name = "AuthorJobTitleTextBox";
			this.AuthorJobTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.AuthorJobTitleTextBox.TabIndex = 2;
			// 
			// AuthorPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorPhoneTextBox, "JE_AuthorPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_AuthorPhone)));
			this.AuthorPhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AuthorPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 69, true);
			this.AuthorPhoneTextBox.Name = "AuthorPhoneTextBox";
			this.AuthorPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.AuthorPhoneTextBox.TabIndex = 3;
			// 
			// AuditorPhoneTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuditorPhoneTextBox, "JE_AuditorPhone");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_AuditorPhone)));
			this.AuditorPhoneTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AuditorPhoneTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 183, true);
			this.AuditorPhoneTextBox.Name = "AuditorPhoneTextBox";
			this.AuditorPhoneTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.AuditorPhoneTextBox.TabIndex = 7;
			// 
			// AuditorJobTitleTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuditorJobTitleTextBox, "JE_AuditorJobTitle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_AuditorJobTitle)));
			this.AuditorJobTitleTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AuditorJobTitleTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 209, true);
			this.AuditorJobTitleTextBox.Name = "AuditorJobTitleTextBox";
			this.AuditorJobTitleTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(337, 20, true);
			this.AuditorJobTitleTextBox.TabIndex = 6;
			// 
			// AuditorNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuditorNameTextBox, "JE_AuditorName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).JE_AuditorName)));
			this.AuditorNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AuditorNameTextBox, false);
			this.AuditorNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(138, 157, true);
			this.AuditorNameTextBox.Name = "AuditorNameTextBox";
			this.AuditorNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(227, 20, true);
			this.AuditorNameTextBox.TabIndex = 5;
			// 
			// AuditorGuidDropEdit
			// 
			this.AuditorGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuditorGuidDropEdit, "VDAuditor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.KR.Business.JobDeclaration)(null)).VDAuditor)));
			this.AuditorGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(28, 157, true);
			this.AuditorGuidDropEdit.Name = "AuditorGuidDropEdit";
			this.AuditorGuidDropEdit.ShowDescriptionBox = false;
			this.AuditorGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.AuditorGuidDropEdit.TabIndex = 4;
			// 
			// AuthorAndAuditorUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.AuditorPhoneTextBox);
			this.Controls.Add(this.AuditorJobTitleTextBox);
			this.Controls.Add(this.AuditorNameTextBox);
			this.Controls.Add(this.AuditorGuidDropEdit);
			this.Controls.Add(this.AuthorPhoneTextBox);
			this.Controls.Add(this.AuthorJobTitleTextBox);
			this.Controls.Add(this.AuthorNameTextBox);
			this.Controls.Add(this.AuthorGuidDropEdit);
			this.Name = "AuthorAndAuditorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(458, 286, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AuthorGuidDropEdit.ResumeLayout(true);
			this.AuthorGuidDropEdit.PerformLayout();
			this.AuditorGuidDropEdit.ResumeLayout(true);
			this.AuditorGuidDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		internal ZArchitecture.ZTextBox AuthorNameTextBox;
		internal ZArchitecture.ZTextBox AuthorJobTitleTextBox;
		internal ZArchitecture.ZTextBox AuthorPhoneTextBox;
		internal ZArchitecture.GUI.ZGuidDropEdit AuthorGuidDropEdit;
		internal ZArchitecture.ZTextBox AuditorPhoneTextBox;
		internal ZArchitecture.ZTextBox AuditorJobTitleTextBox;
		internal ZArchitecture.ZTextBox AuditorNameTextBox;
		internal ZArchitecture.GUI.ZGuidDropEdit AuditorGuidDropEdit;
	}
}
