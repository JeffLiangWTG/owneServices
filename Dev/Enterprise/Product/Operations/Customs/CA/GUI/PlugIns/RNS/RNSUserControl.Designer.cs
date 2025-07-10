using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI.PlugIns
{
	partial class RNSUserControl
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
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.LatestNoticeProcessingDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.LatestNoticeStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ArrivalCertificationDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReleaseDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EntryDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ArrivalCertificationStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ReleaseStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.LoginUserTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MessagesUserControl = new Enterprise.Customs.CA.GUI.CAMessagesUserControl();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.LatestNoticeProcessingDateEdit.SuspendLayout();
			this.ArrivalCertificationDateEdit.SuspendLayout();
			this.ReleaseDateEdit.SuspendLayout();
			this.EntryDateEdit.SuspendLayout();
			this.MessagesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.RNSMessagingBO);
			// 
			// DetailsGroupBox
			// 
			this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("RNSUserControl|2dd236c9-8645-437c-b19f-4720f0ffab77", "Release Details");
			this.DetailsGroupBox.Controls.Add(this.LatestNoticeProcessingDateEdit);
			this.DetailsGroupBox.Controls.Add(this.LatestNoticeStatusTextBox);
			this.DetailsGroupBox.Controls.Add(this.ArrivalCertificationDateEdit);
			this.DetailsGroupBox.Controls.Add(this.ReleaseDateEdit);
			this.DetailsGroupBox.Controls.Add(this.EntryDateEdit);
			this.DetailsGroupBox.Controls.Add(this.ArrivalCertificationStatusTextBox);
			this.DetailsGroupBox.Controls.Add(this.ReleaseStatusTextBox);
			this.DetailsGroupBox.Controls.Add(this.LoginUserTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 102, true);
			this.DetailsGroupBox.TabIndex = 1;
			this.DetailsGroupBox.TabStop = false;
			// 
			// LatestNoticeProcessingDateEdit
			// 
			this.LatestNoticeProcessingDateEdit.AllowDrop = true;
			this.LatestNoticeProcessingDateEdit.AutoCompleteMonthThreshold = 1;
			this.LatestNoticeProcessingDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.LatestNoticeProcessingDateEdit, "LatestNoticeMessage.RNSProcessingDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.RNSMessagingBO)(null)).LatestNoticeMessage.RNSProcessingDate)));
			this.LatestNoticeProcessingDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("c45aa878-e929-4518-8f4a-b43179563a10", "Processing Date");
			this.LatestNoticeProcessingDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.LatestNoticeProcessingDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(606, 71, true);
			this.LatestNoticeProcessingDateEdit.Name = "LatestNoticeProcessingDateEdit";
			this.LatestNoticeProcessingDateEdit.TabIndex = 6;
			// 
			// LatestNoticeStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.LatestNoticeStatusTextBox, "LatestNoticeMessage.StatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSMessagingBO)(null)).LatestNoticeMessage.StatusDescription)));
			this.LatestNoticeStatusTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("9e7c779a-dc2f-4088-86ed-7504642fb9c3", "Latest Notice Status");
			this.LatestNoticeStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 71, true);
			this.LatestNoticeStatusTextBox.Name = "LatestNoticeStatusTextBox";
			this.LatestNoticeStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 20, true);
			this.LatestNoticeStatusTextBox.TabIndex = 5;
			// 
			// ArrivalCertificationDateEdit
			// 
			this.ArrivalCertificationDateEdit.AllowDrop = true;
			this.ArrivalCertificationDateEdit.AutoCompleteMonthThreshold = 1;
			this.ArrivalCertificationDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ArrivalCertificationDateEdit, "ArrivalCertificationDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.RNSMessagingBO)(null)).ArrivalCertificationDate)));
			this.ArrivalCertificationDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(606, 45, true);
			this.ArrivalCertificationDateEdit.Name = "ArrivalCertificationDateEdit";
			this.ArrivalCertificationDateEdit.TabIndex = 3;
			// 
			// ReleaseDateEdit
			// 
			this.ReleaseDateEdit.AllowDrop = true;
			this.ReleaseDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReleaseDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReleaseDateEdit, "RN_ReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.RNSMessagingBO)(null)).RN_ReleaseDate)));
			this.ReleaseDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(606, 19, true);
			this.ReleaseDateEdit.Name = "ReleaseDateEdit";
			this.ReleaseDateEdit.TabIndex = 1;
			// 
			// EntryDateEdit
			// 
			this.EntryDateEdit.AllowDrop = true;
			this.EntryDateEdit.AutoCompleteMonthThreshold = 1;
			this.EntryDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EntryDateEdit, "RN_EntryDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.RNSMessagingBO)(null)).RN_EntryDate)));
			this.EntryDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(904, 19, true);
			this.EntryDateEdit.Name = "EntryDateEdit";
			this.EntryDateEdit.TabIndex = 3;
			// 
			// ArrivalCertificationStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.ArrivalCertificationStatusTextBox, "ArrivalCertificationStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSMessagingBO)(null)).ArrivalCertificationStatus)));
			this.ArrivalCertificationStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 45, true);
			this.ArrivalCertificationStatusTextBox.Name = "ArrivalCertificationStatusTextBox";
			this.ArrivalCertificationStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 20, true);
			this.ArrivalCertificationStatusTextBox.TabIndex = 4;
			// 
			// ReleaseStatusTextBox
			// 
			this.BindingSource.SetBindingMember(this.ReleaseStatusTextBox, "RN_ReleaseStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSMessagingBO)(null)).RN_ReleaseStatus)));
			this.ReleaseStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(151, 19, true);
			this.ReleaseStatusTextBox.Name = "ReleaseStatusTextBox";
			this.ReleaseStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(318, 20, true);
			this.ReleaseStatusTextBox.TabIndex = 0;
			// 
			// LoginUserTextBox
			// 
			this.BindingSource.SetBindingMember(this.LoginUserTextBox, "RN_LoginUser");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.RNSMessagingBO)(null)).RN_LoginUser)));
			this.LoginUserTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(755, 19, true);
			this.LoginUserTextBox.Name = "LoginUserTextBox";
			this.LoginUserTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.LoginUserTextBox.TabIndex = 2;
			// 
			// MessagesUserControl
			// 
			this.MessagesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MessagesUserControl, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Messaging.Business.IEDIMessageCollectionProvider)(((Enterprise.Customs.CA.Business.RNSMessagingBO)(null)))));
			this.MessagesUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MessagesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 102, true);
			this.MessagesUserControl.Name = "MessagesUserControl";
			this.MessagesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 462, true);
			this.MessagesUserControl.TabIndex = 0;
			// 
			// RNSUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MessagesUserControl);
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "RNSUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1019, 564, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.LatestNoticeProcessingDateEdit.ResumeLayout(true);
			this.LatestNoticeProcessingDateEdit.PerformLayout();
			this.ArrivalCertificationDateEdit.ResumeLayout(true);
			this.ArrivalCertificationDateEdit.PerformLayout();
			this.ReleaseDateEdit.ResumeLayout(true);
			this.ReleaseDateEdit.PerformLayout();
			this.EntryDateEdit.ResumeLayout(true);
			this.EntryDateEdit.PerformLayout();
			this.MessagesUserControl.ResumeLayout(true);
			this.MessagesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CAMessagesUserControl MessagesUserControl;
		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.ZTextBox ReleaseStatusTextBox;
		private ZArchitecture.GUI.ZDateEdit ReleaseDateEdit;
		private ZArchitecture.ZTextBox LoginUserTextBox;
		private ZArchitecture.GUI.ZDateEdit EntryDateEdit;
		private ZArchitecture.GUI.ZDateEdit ArrivalCertificationDateEdit;
		private ZArchitecture.ZTextBox ArrivalCertificationStatusTextBox;
		private ZArchitecture.ZTextBox LatestNoticeStatusTextBox;
		private ZDateEdit LatestNoticeProcessingDateEdit;
	}
}
