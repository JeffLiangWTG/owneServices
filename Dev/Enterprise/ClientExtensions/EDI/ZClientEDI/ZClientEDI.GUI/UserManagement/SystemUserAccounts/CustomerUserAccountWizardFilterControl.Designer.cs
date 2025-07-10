namespace Enterprise.Client.EDI.UserManagement.GUI
{
	partial class CustomerUserAccountWizardFilterControl
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
			SearchManager?.Dispose();
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			this.FilteredGrid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			this.FilteredGrid.AllowBeginDrag = false;
			this.FilteredGrid.AllowDragDropWithChanges = false;
			this.FilteredGrid.AllowNavigation = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("986a34ff-5419-47a7-b7fe-10ea3c4b81ee", "Product");
			zTextBoxColumnStyleInfo1.ColumnName = "Database+LD_Product";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("5d51e152-47f2-48d4-a34b-5536b65d8d7a", "Server Code");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "Database+LD_ServerCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo3.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8e3018fe-c842-4dd1-a6e3-0a9dbbe09ac6", "Tenant ID");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Database+LD_TenantID";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("f6f10e0c-e107-45bc-aed5-6d1e81add720", "System User ID");
			zTextBoxColumnStyleInfo4.ColumnName = "EUA_UserID";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("d3300b5d-a383-43d3-961d-7b7f7e16e0c0", "User ID Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "EUA_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("50524015-95b8-4533-983a-73e28a5b2f02", "User FullName");
			zTextBoxColumnStyleInfo6.ColumnName = "EUA_FullName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo7.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("dc9e1206-bdad-45af-88c1-30548a612743", "User Email");
			zTextBoxColumnStyleInfo7.ColumnName = "EUA_Email";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			zTextBoxColumnStyleInfo8.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7892ca6a-9ba7-4089-a9dd-660851db60c0", "Account Verification Status");
			zTextBoxColumnStyleInfo8.ColumnName = "AccountVerificationStatus";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b357f5ae-55ef-4172-bb9f-c6d5eb402ae1", "Pending Email Verification");
			zCheckBoxColumnStyleInfo2.ColumnName = "EUA_IsEmailVerificationRequired";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo10.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e6f4b1f5-39cb-440a-85a0-ebae33196f34", "Enterprise ID");
			zTextBoxColumnStyleInfo10.ColumnName = "DatabaseLicenceEnterprise+LE_EnterpriseID";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo11.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2d02eedb-b5a8-4f4f-b47f-e1e2fb799bad", "Enterprise Code");
			zTextBoxColumnStyleInfo11.ColumnName = "DatabaseLicenceEnterprise+LE_EnterpriseCode";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo12.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("8b16b4bd-e13c-4817-b385-f81c1ace275c", "Organisation");
			zTextBoxColumnStyleInfo12.ColumnName = "EDIWebAccessContact+OrganisationCode";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo13.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2ce7cde5-3305-455c-ada5-a19c27210160", "Organisation Name");
			zTextBoxColumnStyleInfo13.ColumnName = "EDIWebAccessContact+WorkingAddressCompanyName";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo14.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("444f4153-d659-457a-84a8-0762b118dd01", "Contact");
			zTextBoxColumnStyleInfo14.ColumnName = "EDIWebAccessContact+OC_ContactName";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo15.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("3137ce55-e2a9-448b-889b-0ccd5cbfec33", "Status Code");
			zTextBoxColumnStyleInfo15.ColumnName = "EUA_ContactRelationshipStatus";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("dc9e1206-bdad-45af-88c1-30548a612743", "Contact Email");
			zTextBoxColumnStyleInfo16.ColumnName = "EDIWebAccessContact+OC_Email";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(170);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.FilteredGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 171, true);
			this.FilteredGrid.TabIndex = 3;
			this.FilteredGrid.ReadOnly = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.UserManagement.Business.EdiCustomerUserAccount);
			// 
			// CustomerUserAccountWizardFilterControl
			// 
			this.Name = "CustomerUserAccountWizardFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			this.FilteredGrid.ResumeLayout(false);
			this.FilteredGrid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
