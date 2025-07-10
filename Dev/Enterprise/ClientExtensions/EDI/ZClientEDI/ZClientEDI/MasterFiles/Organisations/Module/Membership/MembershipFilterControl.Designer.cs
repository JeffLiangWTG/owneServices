namespace Enterprise.Client.EDI.MasterFiles.Module
{
	partial class MembershipFilterControl
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
			this.membershipTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.membershipOrganisationFindBox = new Enterprise.MasterFiles.GUI.ZOrganisationFindBox();
			this.validFromFrom = new ZArchitecture.GUI.ZDateEdit();
			this.validFromTo = new ZArchitecture.GUI.ZDateEdit();
			this.validToFrom = new ZArchitecture.GUI.ZDateEdit();
			this.validToTo = new ZArchitecture.GUI.ZDateEdit();
			this.agreementVersionFrom = new ZArchitecture.GUI.ZDateEdit();
			this.agreementVersionTo = new ZArchitecture.GUI.ZDateEdit();
			this.labelAgreementVersion = new ZArchitecture.ZLabel();
			this.labelFrom = new ZArchitecture.ZLabel();
			this.labelTo = new ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.membershipTypeDropEdit.SuspendLayout();
			this.membershipOrganisationFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(MembershipFilter);
			// 
			// MembershipTypeDropEdit
			// 
			this.membershipTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.membershipTypeDropEdit, "MembershipType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((MembershipFilter)(null)).MembershipType)));
			this.membershipTypeDropEdit.CaptionResourceString = ZClientEDI.Res.GetData("MembershipFilterControl|51299135-f63e-47bb-b18f-1bf521f8ec09", "Type");
			this.membershipTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 2, true);
			this.membershipTypeDropEdit.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.membershipTypeDropEdit.Name = "MembershipTypeDropEdit";
			this.membershipTypeDropEdit.PreBoundMaxLength = 2;
			this.membershipTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 15, true);
			this.membershipTypeDropEdit.CodeBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(30, 15, true);
			this.membershipTypeDropEdit.TabIndex = 1;
			// 
			// membershipOrganisationFindBox
			// 
			this.membershipOrganisationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.membershipOrganisationFindBox, "Organisation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Module.MembershipFilter)(null)).MembershipType)));
			this.membershipOrganisationFindBox.CaptionResourceString = ZClientEDI.Res.GetData("membershipOrganisationFindBox|334FF190-6ADF-4454-9A35-B3F221302484", "Organization");
			this.membershipOrganisationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 25, true);
			this.membershipOrganisationFindBox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.membershipOrganisationFindBox.Name = "membershipOrganisationFindBox";
			this.membershipOrganisationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 15, true);
			this.membershipOrganisationFindBox.TabIndex = 2;
			//
			// labelFrom
			//
			this.labelFrom.AutoSize = true;
			this.labelFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(160, 50, true);
			this.labelFrom.Text = "Valid From:";
			// 
			// validFromFrom
			// 
			this.validFromFrom.AllowDrop = true;
			this.validFromFrom.AutoCompleteMonthThreshold = 1;
			this.validFromFrom.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.validFromFrom, "ValidFromFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_EndDate)));
			this.validFromFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 47, true);
			this.validFromFrom.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.validFromFrom.Name = "ValidFromFrom";
			this.validFromFrom.TabIndex = 3;
			this.validFromFrom.CaptionResourceString = ZClientEDI.Res.GetData("9737319F-1454-4E7E-B51D-0F283123620C", "From");
			// 
			// validFromTo
			// 
			this.validFromTo.AllowDrop = true;
			this.validFromTo.AutoCompleteMonthThreshold = 1;
			this.validFromTo.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.validFromTo, "ValidFromTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_EndDate)));
			this.validFromTo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 47, true);
			this.validFromTo.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.validFromTo.Name = "ValidFromTo";
			this.validFromTo.TabIndex = 4;
			this.validFromTo.CaptionResourceString = ZClientEDI.Res.GetData("8B3D19B4-4EAA-4C1E-99EB-1F133D637CA5", "To");
			//
			// labelTo
			//
			this.labelTo.AutoSize = true;
			this.labelTo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(172, 72, true);
			this.labelTo.Text = "Valid To:";
			// 
			// validToFrom
			// 
			this.validToFrom.AllowDrop = true;
			this.validToFrom.AutoCompleteMonthThreshold = 1;
			this.validToFrom.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.validToFrom, "ValidToFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_EndDate)));
			this.validToFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 69, true);
			this.validToFrom.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.validToFrom.Name = "ValidToFrom";
			this.validToFrom.TabIndex = 5;
			this.validToFrom.CaptionResourceString = ZClientEDI.Res.GetData("EC2C6FCE-F2D1-4820-8B79-6C60E37903F9", "From");
			// 
			// validToTo
			// 
			this.validToTo.AllowDrop = true;
			this.validToTo.AutoCompleteMonthThreshold = 1;
			this.validToTo.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.validToTo, "ValidToTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			//CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.AccCommissionRule)(null)).ACM_EndDate)));
			this.validToTo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 69, true);
			this.validToTo.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.validToTo.Name = "ValidToTo";
			this.validToTo.TabIndex = 5;
			this.validToTo.CaptionResourceString = ZClientEDI.Res.GetData("C911D6AE-C4F5-42AB-8D42-23E34D792829", "To");
			//
			// LabelAgreementVersion
			//
			this.labelAgreementVersion.AutoSize = true;
			this.labelAgreementVersion.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 94, true);
			this.labelAgreementVersion.Text = "Agreement Version:";
			//
			// AgreementVersionFrom
			//
			this.agreementVersionFrom.AllowDrop = true;
			this.agreementVersionFrom.AutoCompleteMonthThreshold = 1;
			this.agreementVersionFrom.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.agreementVersionFrom, "AgreementVersionFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((MembershipFilter)(null)).AgreementVersionFrom)));
			this.agreementVersionFrom.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(260, 91, true);
			this.agreementVersionFrom.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.agreementVersionFrom.Name = "AgreementVersionFrom";
			this.agreementVersionFrom.TabIndex = 5;
			this.agreementVersionFrom.CaptionResourceString = ZClientEDI.Res.GetData("87B27081-E40D-4C06-8622-F2F7D7AFAE9E", "From");
			//
			// AgreementVersionTo
			//
			this.agreementVersionTo.AllowDrop = true;
			this.agreementVersionTo.AutoCompleteMonthThreshold = 1;
			this.agreementVersionTo.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.agreementVersionTo, "AgreementVersionTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((MembershipFilter)(null)).AgreementVersionTo)));
			this.agreementVersionTo.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(400, 91, true);
			this.agreementVersionTo.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(2, true);
			this.agreementVersionTo.Name = "AgreementVersionTo";
			this.agreementVersionTo.TabIndex = 5;
			this.agreementVersionTo.CaptionResourceString = ZClientEDI.Res.GetData("1DEB98E8-F804-4095-9FF1-EA68316193B9", "To");
			// 
			// MembershipFilterControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.membershipTypeDropEdit);
			this.Controls.Add(this.membershipOrganisationFindBox);
			this.Controls.Add(this.labelFrom);
			this.Controls.Add(this.labelTo);
			this.Controls.Add(this.labelAgreementVersion);
			this.Controls.Add(this.validFromFrom);
			this.Controls.Add(this.validFromTo);
			this.Controls.Add(this.validToFrom);
			this.Controls.Add(this.validToTo);
			this.Controls.Add(this.agreementVersionFrom);
			this.Controls.Add(this.agreementVersionTo);
			this.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, 3, 3, 3, true);
			this.Name = "MembershipFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1067, 110, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.membershipTypeDropEdit.ResumeLayout(true);
			this.membershipTypeDropEdit.PerformLayout();
			this.membershipOrganisationFindBox.ResumeLayout(true);
			this.membershipOrganisationFindBox.PerformLayout();
			this.validFromFrom.ResumeLayout(true);
			this.validFromFrom.PerformLayout();
			this.validFromTo.ResumeLayout(true);
			this.validFromTo.PerformLayout();
			this.validToFrom.ResumeLayout(true);
			this.validToFrom.PerformLayout();
			this.validToTo.ResumeLayout(true);
			this.validToTo.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDropEdit membershipTypeDropEdit;
		private Enterprise.MasterFiles.GUI.ZOrganisationFindBox membershipOrganisationFindBox;
		private ZArchitecture.GUI.ZDateEdit validFromFrom;
		private ZArchitecture.GUI.ZDateEdit validFromTo;
		private ZArchitecture.GUI.ZDateEdit validToFrom;
		private ZArchitecture.GUI.ZDateEdit validToTo;
		private ZArchitecture.GUI.ZDateEdit agreementVersionFrom;
		private ZArchitecture.GUI.ZDateEdit agreementVersionTo;
		private ZArchitecture.ZLabel labelFrom;
		private ZArchitecture.ZLabel labelTo;
		private ZArchitecture.ZLabel labelAgreementVersion;
	}
}
