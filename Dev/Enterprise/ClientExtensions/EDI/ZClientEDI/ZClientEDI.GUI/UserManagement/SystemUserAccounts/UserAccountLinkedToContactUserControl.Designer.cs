namespace Enterprise.Client.EDI.UserManagement.GUI
{
	partial class UserAccountLinkedToContactUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.UserAccountGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo0 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.UserAccountGrid)).BeginInit();
			this.UserAccountGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LicenceGrid
			// 
			this.UserAccountGrid.AllowBeginDrag = false;
			this.UserAccountGrid.AllowDragDropWithChanges = false;
			this.UserAccountGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.UserAccountGrid, ".");
			zTextBoxColumnStyleInfo0.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c9bb2fef-184f-48e4-bc6f-e6e997a803f4", "Master Org");
			zTextBoxColumnStyleInfo0.ColumnName = "ContactOrganisation+OH_Code";
			zTextBoxColumnStyleInfo0.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("da39793d-ae40-44c3-ae56-04376294aeb5", "Product");
			zTextBoxColumnStyleInfo1.ColumnName = "Database+LD_Product";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2c50af28-2ea7-4065-8175-ca06166cb463", "Server Code");
			zTextBoxColumnStyleInfo2.ColumnName = "Database+LD_ServerCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo3.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2b1f53e1-4fc3-4912-9705-8c58021f66c5", "Tenant ID");
			zTextBoxColumnStyleInfo3.ColumnName = "Database+LD_TenantID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo4.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("28c89a92-baad-475c-8d9e-ff280a2fff05", "System User ID");
			zTextBoxColumnStyleInfo4.ColumnName = "EUA_UserID";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("a84179af-0d76-43e5-b187-0aa83ebd278a", "User ID Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "EUA_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2b3a4fe1-7906-4d46-9349-ee7f4e482c11", "User FullName");
			zTextBoxColumnStyleInfo6.ColumnName = "EUA_FullName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("667c3ea0-ae1d-49a8-af56-c27ee652f699", "User Email");
			zTextBoxColumnStyleInfo7.ColumnName = "EUA_Email";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo8.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5981a403-2876-4224-afbc-83f4b8292350", "Account Verification");
			zTextBoxColumnStyleInfo8.ColumnName = "AccountVerificationStatus";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3c62a276-f94d-4d9a-a04d-b04113c4b7d1", "Email Verification Required");
			zCheckBoxColumnStyleInfo2.ColumnName = "EUA_IsEmailVerificationRequired";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			zTextBoxColumnStyleInfo10.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c07780c0-511f-4abd-b5e2-9f0845d1e9a7", "Enterprise ID");
			zTextBoxColumnStyleInfo10.ColumnName = "DatabaseLicenceEnterprise+LE_EnterpriseID";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("ec569328-38d6-48b4-b328-65675cc67a08", "Enterprise Code");
			zTextBoxColumnStyleInfo11.ColumnName = "DatabaseLicenceEnterprise+LE_EnterpriseCode";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo12.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("dc2eb494-53e4-4c2d-b7af-4452ddc1f39c", "Master Organisation Name");
			zTextBoxColumnStyleInfo12.ColumnName = "ContactOrganisation+OH_FullName";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo13.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b1b40c9e-48ad-4444-a2db-75bdd6b8010f", "Password Set?");
			zTextBoxColumnStyleInfo13.ColumnName = "EDIWebAccessContact+PasswordIsSet";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("a1a9233e-5c7d-4052-a9ba-368abe717e7e", "Personal Recovery Email");
			zTextBoxColumnStyleInfo14.ColumnName = "EDIWebAccessContact+PersonalRecoveryEmail";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo15.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3a9b5d38-417c-47d3-bf87-5029ec9a1ada", "Status Code");
			zTextBoxColumnStyleInfo15.ColumnName = "EUA_ContactRelationshipStatus";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo0);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.UserAccountGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.UserAccountGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.UserAccountGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.UserAccountGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.UserAccountGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.UserAccountGrid.TabIndex = 3;
			this.UserAccountGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UserAccountGrid.ReadOnly = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(UserManagement.Business.EdiCustomerUserAccount);
			// 
			// UserAccountLinkedToContactUserControl
			// 
			this.Name = "UserAccountLinkedToContactUserControl";
			this.Controls.Add(UserAccountGrid);
			((System.ComponentModel.ISupportInitialize)(this.UserAccountGrid)).EndInit();
			this.UserAccountGrid.ResumeLayout(false);
			this.UserAccountGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		protected Enterprise.ZArchitecture.ZGrid UserAccountGrid;

		#endregion
	}
}
