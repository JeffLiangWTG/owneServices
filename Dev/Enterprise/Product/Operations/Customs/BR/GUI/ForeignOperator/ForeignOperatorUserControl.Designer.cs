namespace Enterprise.Customs.BR.GUI
{
	partial class ForeignOperatorUserControl
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
			this.ForeignOperatorGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.MessageStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AuthorityVersionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AuthorityIdentifierTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ForeignOperatorDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EmailTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.InternalCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.TinTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ForeignOperatorNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ForeignOperatorCountryTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ForeignOperatorGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.OwnerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ForeignOperatorGroupBox.SuspendLayout();
			this.CustomsStatusDropEdit.SuspendLayout();
			this.MessageStatusDropEdit.SuspendLayout();
			this.ForeignOperatorDetailsGroupBox.SuspendLayout();
			this.ForeignOperatorGuidFindBox.SuspendLayout();
			this.OwnerGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Business.CusBRForeignOperator);
			// 
			// ForeignOperatorGroupBox
			//
			this.ForeignOperatorGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("135331DA-422A-4B34-8CBF-76E5877ADE6B", "Foreign Operator");
			this.ForeignOperatorGroupBox.Controls.Add(this.CustomsStatusDropEdit);
			this.ForeignOperatorGroupBox.Controls.Add(this.MessageStatusDropEdit);
			this.ForeignOperatorGroupBox.Controls.Add(this.AuthorityVersionTextBox);
			this.ForeignOperatorGroupBox.Controls.Add(this.AuthorityIdentifierTextBox);
			this.ForeignOperatorGroupBox.Controls.Add(this.ForeignOperatorDetailsGroupBox);
			this.ForeignOperatorGroupBox.Controls.Add(this.ForeignOperatorGuidFindBox);
			this.ForeignOperatorGroupBox.Controls.Add(this.OwnerGuidFindBox);
			this.ForeignOperatorGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ForeignOperatorGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ForeignOperatorGroupBox.Name = "ForeignOperatorGroupBox";
			this.ForeignOperatorGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 306, true);
			this.ForeignOperatorGroupBox.TabIndex = 0;
			this.ForeignOperatorGroupBox.TabStop = false;
			// 
			// CustomsStatusDropEdit
			// 
			this.CustomsStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsStatusDropEdit, "BFR_CustomsStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).BFR_CustomsStatus)));
			this.CustomsStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 79, true);
			this.CustomsStatusDropEdit.Name = "CustomsStatusDropEdit";
			this.CustomsStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 15, true);
			this.CustomsStatusDropEdit.TabIndex = 8;
			// 
			// MessageStatusDropEdit
			// 
			this.MessageStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessageStatusDropEdit, "BFR_MessageStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).BFR_MessageStatus)));
			this.MessageStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 49, true);
			this.MessageStatusDropEdit.Name = "MessageStatusDropEdit";
			this.MessageStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 15, true);
			this.MessageStatusDropEdit.TabIndex = 1;
			// 
			// AuthorityVersionTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorityVersionTextBox, "BFR_AuthorityVersion");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).BFR_AuthorityVersion)));
			this.AuthorityVersionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(259, 259, true);
			this.AuthorityVersionTextBox.Name = "AuthorityVersionTextBox";
			this.AuthorityVersionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.AuthorityVersionTextBox.TabIndex = 7;
			// 
			// AuthorityIdentifierTextBox
			// 
			this.BindingSource.SetBindingMember(this.AuthorityIdentifierTextBox, "BFR_AuthorityIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).BFR_AuthorityIdentifier)));
			this.AuthorityIdentifierTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 259, true);
			this.AuthorityIdentifierTextBox.Name = "AuthorityIdentifierTextBox";
			this.AuthorityIdentifierTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 15, true);
			this.AuthorityIdentifierTextBox.TabIndex = 6;
			// 
			// ForeignOperatorDetailsGroupBox
			//
			this.ForeignOperatorDetailsGroupBox.CaptionResourceString = Enterprise.Customs.BR.GUI.Res.GetData("99A25618-61EF-403B-96F1-B51B0FB2553E", "Foreign Operator Details");
			this.ForeignOperatorDetailsGroupBox.Controls.Add(this.EmailTextBox);
			this.ForeignOperatorDetailsGroupBox.Controls.Add(this.InternalCodeTextBox);
			this.ForeignOperatorDetailsGroupBox.Controls.Add(this.TinTextBox);
			this.ForeignOperatorDetailsGroupBox.Controls.Add(this.ForeignOperatorNameTextBox);
			this.ForeignOperatorDetailsGroupBox.Controls.Add(this.ForeignOperatorCountryTextBox);
			this.ForeignOperatorDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 136, true);
			this.ForeignOperatorDetailsGroupBox.Name = "ForeignOperatorDetailsGroupBox";
			this.ForeignOperatorDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(622, 106, true);
			this.ForeignOperatorDetailsGroupBox.TabIndex = 4;
			this.ForeignOperatorDetailsGroupBox.TabStop = false;
			// 
			// EmailTextBox
			// 
			this.BindingSource.SetBindingMember(this.EmailTextBox, "ForeignOperatorEmail");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).ForeignOperatorEmail)));
			this.EmailTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 79, true);
			this.EmailTextBox.Name = "EmailTextBox";
			this.EmailTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 15, true);
			this.EmailTextBox.TabIndex = 8;
			// 
			// InternalCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.InternalCodeTextBox, "ForeignOperatorInternalCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).ForeignOperatorInternalCode)));
			this.InternalCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 49, true);
			this.InternalCodeTextBox.Name = "InternalCodeTextBox";
			this.InternalCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 15, true);
			this.InternalCodeTextBox.TabIndex = 7;
			// 
			// TinTextBox
			// 
			this.BindingSource.SetBindingMember(this.TinTextBox, "ForeignOperatorTIN");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((string)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).ForeignOperatorTIN)));
			this.TinTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(435, 20, true);
			this.TinTextBox.Name = "TinTextBox";
			this.TinTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 15, true);
			this.TinTextBox.TabIndex = 6;
			// 
			// ForeignOperatorNameTextBox
			// 
			this.BindingSource.SetBindingMember(this.ForeignOperatorNameTextBox, "ForeignOperatorName");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).ForeignOperatorName)));
			this.ForeignOperatorNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 20, true);
			this.ForeignOperatorNameTextBox.Name = "ForeignOperatorNameTextBox";
			this.ForeignOperatorNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 15, true);
			this.ForeignOperatorNameTextBox.TabIndex = 4;
			// 
			// ForeignOperatorCountryTextBox
			// 
			this.BindingSource.SetBindingMember(this.ForeignOperatorCountryTextBox, "ForeignOperatorCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).ForeignOperatorCountry)));
			this.ForeignOperatorCountryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 49, true);
			this.ForeignOperatorCountryTextBox.Name = "ForeignOperatorCountryTextBox";
			this.ForeignOperatorCountryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(211, 15, true);
			this.ForeignOperatorCountryTextBox.TabIndex = 5;
			// 
			// ForeignOperatorGuidFindBox
			// 
			this.ForeignOperatorGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ForeignOperatorGuidFindBox, "BFR_OH_ForeignOperator");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).BFR_OH_ForeignOperator)));
			this.ForeignOperatorGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 107, true);
			this.ForeignOperatorGuidFindBox.Name = "ForeignOperatorGuidFindBox";
			this.ForeignOperatorGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.ForeignOperatorGuidFindBox.ParentType = null;
			this.ForeignOperatorGuidFindBox.ShowDescriptionBox = false;
			this.ForeignOperatorGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 15, true);
			this.ForeignOperatorGuidFindBox.TabIndex = 3;
			// 
			// OwnerGuidFindBox
			// 
			this.OwnerGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OwnerGuidFindBox, "BFR_OH_Owner");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.BR.Business.CusBRForeignOperator)(null)).BFR_OH_Owner)));
			this.OwnerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 20, true);
			this.OwnerGuidFindBox.Name = "OwnerGuidFindBox";
			this.OwnerGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.OwnerGuidFindBox.ParentType = null;
			this.OwnerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 15, true);
			this.OwnerGuidFindBox.TabIndex = 0;
			// 
			// ForeignOperatorUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ForeignOperatorGroupBox);
			this.Name = "ForeignOperatorUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(632, 306, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ForeignOperatorGroupBox.ResumeLayout(false);
			this.ForeignOperatorGroupBox.PerformLayout();
			this.CustomsStatusDropEdit.ResumeLayout(true);
			this.CustomsStatusDropEdit.PerformLayout();
			this.MessageStatusDropEdit.ResumeLayout(true);
			this.MessageStatusDropEdit.PerformLayout();
			this.ForeignOperatorDetailsGroupBox.ResumeLayout(false);
			this.ForeignOperatorDetailsGroupBox.PerformLayout();
			this.ForeignOperatorGuidFindBox.ResumeLayout(true);
			this.ForeignOperatorGuidFindBox.PerformLayout();
			this.OwnerGuidFindBox.ResumeLayout(true);
			this.OwnerGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox ForeignOperatorGroupBox;
		internal ZArchitecture.GUI.ZGroupBox ForeignOperatorDetailsGroupBox;
		internal ZArchitecture.GUI.ZGuidFindBox OwnerGuidFindBox;
		internal ZArchitecture.ZTextBox ForeignOperatorNameTextBox;
		internal ZArchitecture.ZTextBox ForeignOperatorCountryTextBox;
		internal ZArchitecture.ZTextBox AuthorityVersionTextBox;
		internal ZArchitecture.ZTextBox AuthorityIdentifierTextBox;
		internal ZArchitecture.GUI.ZDropEdit MessageStatusDropEdit;
		internal ZArchitecture.GUI.ZGuidFindBox ForeignOperatorGuidFindBox;
		internal ZArchitecture.GUI.ZDropEdit CustomsStatusDropEdit;
		internal ZArchitecture.ZTextBox TinTextBox;
		internal ZArchitecture.ZTextBox InternalCodeTextBox;
		internal ZArchitecture.ZTextBox EmailTextBox;
	}
}
