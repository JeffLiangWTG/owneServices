namespace Enterprise.Customs.JP.GUI
{
	partial class StaffCredentialsUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.CredentialsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.PasswordGroupbox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PasswordGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PasswordGroupbox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.PasswordGrid)).BeginInit();
			this.PasswordGrid.SuspendLayout();
			this.CredentialsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.JP.Common.GlbStaffWrapper);
			// 
			// PasswordGroupbox
			// 
			this.PasswordGroupbox.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("StaffCredentialsUserControl|85AD864F-3390-41C6-B883-BCBA75EDB27F", "Passwords Management");
			this.PasswordGroupbox.Controls.Add(this.PasswordGrid);
			this.PasswordGroupbox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PasswordGroupbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PasswordGroupbox.Name = "PasswordGroupbox";
			this.PasswordGroupbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(400, 300, true);
			this.PasswordGroupbox.TabIndex = 1;
			this.PasswordGroupbox.TabStop = false;
			this.PasswordGroupbox.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PasswordGroupbox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			// 
			// PasswordGrid
			// 
			this.PasswordGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.PasswordGrid, "PasswordCollection");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.JP.Common.GlbStaffWrapper)(null)).PasswordCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.GlbExternalPasswordCUS)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_PasswordType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.GlbExternalPasswordCUS)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_Transport)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.GlbExternalPasswordCUS)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_MailBoxID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.GlbExternalPasswordCUS)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).GP_UserID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.JP.Common.GlbExternalPasswordCUS)(((System.Collections.IList)(((Enterprise.Customs.JP.Common.GlbStaffWrapper)(null)).PasswordCollection)).SyncRoot)).CurrentDecryptedPassword)));
			this.PasswordGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("StaffCredentialsUserControl|906ED280-3675-499A-90F7-C1C20CAD0FA2", "Type", "Password Type");
			zDropEditColumnStyleInfo1.ColumnName = "GP_PasswordType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zDropEditColumnStyleInfo1.IsReadOnly = true;

			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("StaffCredentialsUserControl|EB8A8B7A-C4D8-474E-84CC-E7833326E90E", "System");
			zDropEditColumnStyleInfo2.ColumnName = "GP_Transport";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zDropEditColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;

			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("StaffCredentialsUserControl|3210F478-C8EA-4FC5-A1C2-9EF81C65C1B4", "Code", "User Code");
			zTextBoxColumnStyleInfo1.ColumnName = "GP_MailBoxID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;

			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("StaffCredentialsUserControl|85274F90-5368-4DF6-A9EF-A7D9440D1080", "ID", "User ID");
			zTextBoxColumnStyleInfo2.ColumnName = "GP_UserID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;

			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.JP.GUI.Res.GetData("StaffCredentialsUserControl|BABC4D87-3986-4FB4-9C69-06595E37229D", "Password", "User Password");
			zTextBoxColumnStyleInfo3.ColumnName = "CurrentDecryptedPassword";
			zTextBoxColumnStyleInfo3.PasswordChar = '*';
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.PasswordGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PasswordGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PasswordGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PasswordGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PasswordGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);

			this.PasswordGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PasswordGrid.GridId = "753A5F58-97FB-4C5C-99CD-D95AC0D4CCD4";
			this.PasswordGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PasswordGrid.LayoutKey = "PasswordGrid";
			this.PasswordGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.PasswordGrid.Name = "PasswordGrid";
			this.PasswordGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(812, 378, true);
			this.PasswordGrid.TabIndex = 1;
			// 
			// CredentialsPanel
			// 
			this.CredentialsPanel.Controls.Add(this.PasswordGroupbox);
			this.CredentialsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CredentialsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CredentialsPanel.Name = "CredentialsPanel";
			this.CredentialsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 400, true);
			this.CredentialsPanel.TabIndex = 5;
			// 
			// GlbStaffForm_TWCredentialsUserControl
			//
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CredentialsPanel);
			this.Name = "StaffCredentialsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 458, true);
			this.Controls.SetChildIndex(this.CredentialsHintLabel, 0);
			this.Controls.SetChildIndex(this.CredentialsPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PasswordGroupbox.ResumeLayout(false);
			this.PasswordGroupbox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.PasswordGrid)).EndInit();
			this.PasswordGrid.ResumeLayout(false);
			this.PasswordGrid.PerformLayout();
			this.CredentialsPanel.ResumeLayout(false);
			this.CredentialsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		Enterprise.ZArchitecture.GUI.ZPanel CredentialsPanel;
		Enterprise.ZArchitecture.ZGrid PasswordGrid;
		Enterprise.ZArchitecture.GUI.ZGroupBox PasswordGroupbox;
	}
}
