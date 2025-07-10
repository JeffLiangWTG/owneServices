
namespace Enterprise.Customs.DE.GUI
{
	partial class EntryInstructionTopDetailsUserControl
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
			this.AuthorisationNumberDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.StyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
			this.AdditionalInfoTextBox = new Enterprise.Customs.GUI.LongTextControl();
			this.SubStyleDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CPCDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PartyConstellationDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DateForDutyDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ExitDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AuthorisationNumberDropEdit.SuspendLayout();
			this.StyleDropEdit.SuspendLayout();
			this.AdditionalInfoTextBox.SuspendLayout();
			this.SubStyleDropEdit.SuspendLayout();
			this.CPCDropEdit.SuspendLayout();
			this.PartyConstellationDropEdit.SuspendLayout();
			this.DateForDutyDateEdit.SuspendLayout();
			this.ExitDateDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction);
			// 
			// AuthorisationNumberDropEdit
			// 
			this.AuthorisationNumberDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AuthorisationNumberDropEdit, "CEI_AuthorisationNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).CEI_AuthorisationNumber)));
			this.AuthorisationNumberDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 94, true);
			this.AuthorisationNumberDropEdit.Name = "AuthorisationNumberDropEdit";
			this.AuthorisationNumberDropEdit.ShouldResizeByMaxLength = true;
			this.AuthorisationNumberDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.AuthorisationNumberDropEdit.TabIndex = 12;
			// 
			// StyleDropEdit
			// 
			this.StyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.StyleDropEdit, "CEI_Style");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).CEI_Style)));
			this.StyleDropEdit.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("c51fea20-02b8-42d6-bb5f-8dfde634f849", "Type (Procedure)");
			this.StyleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.StyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 9, true);
			this.StyleDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 7, 2, 2, true);
			this.StyleDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 0, true);
			this.StyleDropEdit.Name = "StyleDropEdit";
			this.StyleDropEdit.ShouldResizeByMaxLength = true;
			this.StyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.StyleDropEdit.TabIndex = 7;
			// 
			// AdditionalInfoTextBox
			// 
			this.AdditionalInfoTextBox.AllowDrop = true;
			this.AdditionalInfoTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AdditionalInfoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 9, true);
			this.AdditionalInfoTextBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 7, 2, 2, true);
			this.AdditionalInfoTextBox.Name = "AdditionalInfoTextBox";
			this.AdditionalInfoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.AdditionalInfoTextBox.TabIndex = 10;
			// 
			// SubStyleDropEdit
			// 
			this.SubStyleDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SubStyleDropEdit, "CEI_SubStyle");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).CEI_SubStyle)));
			this.SubStyleDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.SubStyleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 29, true);
			this.SubStyleDropEdit.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 0, true);
			this.SubStyleDropEdit.Name = "SubStyleDropEdit";
			this.SubStyleDropEdit.ShouldResizeByMaxLength = true;
			this.SubStyleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.SubStyleDropEdit.TabIndex = 8;
			// 
			// CPCDropEdit
			// 
			this.CPCDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CPCDropEdit, "CEI_Procedure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).CEI_Procedure)));
			this.CPCDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 29, true);
			this.CPCDropEdit.Name = "CPCDropEdit";
			this.CPCDropEdit.ShouldResizeByMaxLength = true;
			this.CPCDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(195, 20, true);
			this.CPCDropEdit.TabIndex = 11;
			// 
			// PartyConstellationDropEdit
			// 
			this.PartyConstellationDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PartyConstellationDropEdit, "ZG_PartyConstellation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).ZG_PartyConstellation)));
			this.PartyConstellationDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.PartyConstellationDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(794, 9, true);
			this.PartyConstellationDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, 7, 2, 2, true);
			this.PartyConstellationDropEdit.Name = "PartyConstellationDropEdit";
			this.PartyConstellationDropEdit.ShouldResizeByMaxLength = true;
			this.PartyConstellationDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(229, 20, true);
			this.PartyConstellationDropEdit.TabIndex = 13;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "CEI_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).CEI_Description)));
			this.DescriptionTextBox.CaptionResourceString = null;
			this.DescriptionTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(70, 51, true);
			this.DescriptionTextBox.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 0, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.DescriptionTextBox.TabIndex = 9;
			// 
			// DateForDutyDateEdit
			// 
			this.DateForDutyDateEdit.AllowDrop = true;
			this.DateForDutyDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateForDutyDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateForDutyDateEdit, "CEI_DateForDuty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).CEI_DateForDuty)));
			this.DateForDutyDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 51, true);
			this.DateForDutyDateEdit.Name = "DateForDutyDateEdit";
			this.DateForDutyDateEdit.TabIndex = 14;
			// 
			// ExitDateDateEdit
			// 
			this.ExitDateDateEdit.AllowDrop = true;
			this.ExitDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ExitDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ExitDateDateEdit, "ZG_ExitDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.DE.Business.Declaration.CusEntryInstruction)(null)).ZG_ExitDate)));
			this.ExitDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(496, 73, true);
			this.ExitDateDateEdit.Name = "ExitDateDateEdit";
			this.ExitDateDateEdit.TabIndex = 15;
			// 
			// EntryInstructionTopDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.ExitDateDateEdit);
			this.Controls.Add(this.DateForDutyDateEdit);
			this.Controls.Add(this.AuthorisationNumberDropEdit);
			this.Controls.Add(this.StyleDropEdit);
			this.Controls.Add(this.AdditionalInfoTextBox);
			this.Controls.Add(this.SubStyleDropEdit);
			this.Controls.Add(this.CPCDropEdit);
			this.Controls.Add(this.PartyConstellationDropEdit);
			this.Controls.Add(this.DescriptionTextBox);
			this.Name = "EntryInstructionTopDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1025, 120, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AuthorisationNumberDropEdit.ResumeLayout(true);
			this.AuthorisationNumberDropEdit.PerformLayout();
			this.StyleDropEdit.ResumeLayout(true);
			this.StyleDropEdit.PerformLayout();
			this.AdditionalInfoTextBox.ResumeLayout(true);
			this.AdditionalInfoTextBox.PerformLayout();
			this.SubStyleDropEdit.ResumeLayout(true);
			this.SubStyleDropEdit.PerformLayout();
			this.CPCDropEdit.ResumeLayout(true);
			this.CPCDropEdit.PerformLayout();
			this.PartyConstellationDropEdit.ResumeLayout(true);
			this.PartyConstellationDropEdit.PerformLayout();
			this.DateForDutyDateEdit.ResumeLayout(true);
			this.DateForDutyDateEdit.PerformLayout();
			this.ExitDateDateEdit.ResumeLayout(true);
			this.ExitDateDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZDropEdit AuthorisationNumberDropEdit;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth StyleDropEdit;
		internal Customs.GUI.LongTextControl AdditionalInfoTextBox;
		internal ZArchitecture.GUI.ZDropEdit SubStyleDropEdit;
		internal ZArchitecture.GUI.ZDropEdit CPCDropEdit;
		internal ZArchitecture.GUI.ZDropEdit PartyConstellationDropEdit;
		internal ZArchitecture.ZTextBox DescriptionTextBox;
		internal ZArchitecture.GUI.ZDateEdit DateForDutyDateEdit;
		internal ZArchitecture.GUI.ZDateEdit ExitDateDateEdit;
	}
}
