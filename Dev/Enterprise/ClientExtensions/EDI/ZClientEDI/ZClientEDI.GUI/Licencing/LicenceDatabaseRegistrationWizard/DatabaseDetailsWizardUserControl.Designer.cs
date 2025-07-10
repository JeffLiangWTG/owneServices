namespace Enterprise.Client.EDI.Licencing.GUI
{
	partial class DatabaseDetailsWizardUserControl
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
			this.LicenceGrid = new Enterprise.ZArchitecture.ZGrid();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.LicenceGrid)).BeginInit();
			this.LicenceGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LicenceGrid
			// 
			this.LicenceGrid.AllowBeginDrag = false;
			this.LicenceGrid.AllowDragDropWithChanges = false;
			this.LicenceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.LicenceGrid, ".");
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("42F41DDF-DD2A-4278-8CA0-3A92930D3B5B", "Product");
			zTextBoxColumnStyleInfo1.ColumnName = "LD_Product";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("1912FBF5-6D5D-49FB-A09F-05312867CA91", "Licence Type");
			zTextBoxColumnStyleInfo2.ColumnName = "LD_LicenceType";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("0AFDE8FC-67DF-4BE5-8EDA-0B73731DD24A", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "LD_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zTextBoxColumnStyleInfo3.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("124CC844-3266-49BD-AD5A-C0D1AED58BCF", "Server Code");
			zTextBoxColumnStyleInfo3.ColumnName = "LD_ServerCode";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo4.Caption = "#";
			zTextBoxColumnStyleInfo4.ColumnName = "LD_DatabaseNumber";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo5.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("0ADE2169-1413-450C-B9B5-06E2ADE85D83", "Tenant ID");
			zTextBoxColumnStyleInfo5.ColumnName = "LD_TenantID";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo6.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("D3DE065B-F73B-4261-A534-E15F0BD700B0", "Master Org");
			zTextBoxColumnStyleInfo6.ColumnName = "WebAccessOrg+OH_Code";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo7.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("53A6A9BB-B23C-4B88-9F8F-BE397E07689C", "Model");
			zTextBoxColumnStyleInfo7.ColumnName = "BillingModel";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("D9E3A372-F6D5-48AC-A5F5-D1E0058DD5C3", "Hosted Location");
			zTextBoxColumnStyleInfo8.ColumnName = "LD_HostedLocation";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo9.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("C69FC0D9-4089-4B84-9471-5863D628DE6F", "Release Ring");
			zTextBoxColumnStyleInfo9.ColumnName = "LD_ReleaseRing";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo10.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("093d88c4-cc8c-411c-b042-709b5748ea10", "System ID");
			zTextBoxColumnStyleInfo10.ColumnName = "SystemID";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.LicenceGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.LicenceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.LicenceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.LicenceGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.LicenceGrid.TabIndex = 3;
			this.LicenceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LicenceGrid.ReadOnly = true;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase);
			// 
			// DatabaseDetailsWizardUserControl
			// 
			this.Name = "DatabaseDetailsWizardUserControl";
			this.Controls.Add(LicenceGrid);
			((System.ComponentModel.ISupportInitialize)(this.LicenceGrid)).EndInit();
			this.LicenceGrid.ResumeLayout(false);
			this.LicenceGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		public Enterprise.ZArchitecture.ZGrid LicenceGrid;

		#endregion
	}
}
