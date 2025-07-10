namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class ClientPremiumServicesControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo zMultiLineTextBoxColumnInfo1 = new Enterprise.ZArchitecture.GUI.ZMultiLineTextBoxColumnInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo zGuidDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidDropEditColumnStyleInfo();
			this.zLabel9 = new Enterprise.ZArchitecture.ZLabel();
			this.ServerCode = new Enterprise.ZArchitecture.ZTextBox();
			this.PremiumServicesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.zLabel8 = new Enterprise.ZArchitecture.ZLabel();
			this.ProductionServiceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.zGuidFindBox1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PremiumServicesGrid)).BeginInit();
			this.PremiumServicesGrid.SuspendLayout();
			this.zGuidFindBox1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase);
			// 
			// zLabel9
			// 
			this.zLabel9.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 5, true);
			this.zLabel9.Name = "zLabel9";
			this.zLabel9.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.zLabel9.TabIndex = 22;
			this.zLabel9.Text = "Database Code:";
			this.zLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ServerCode
			// 
			this.BindingSource.SetBindingMember(this.ServerCode, "LD_ServerCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_ServerCode)));
			this.ServerCode.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 4, true);
			this.ServerCode.Name = "ServerCode";
			this.ServerCode.ReadOnly = true;
			this.ServerCode.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.ServerCode.TabIndex = 21;
			// 
			// PremiumServicesGrid
			// 
			this.PremiumServicesGrid.AllowNavigation = false;
			this.PremiumServicesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.PremiumServicesGrid, "PremiumServices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).CPS_PriceHeaderCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).CPS_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).CPS_Units)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).CPS_StartDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).CPS_EndDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).CPS_ClientRef)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).CPS_DisplayOrder)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).CPS_Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).CPS_LCC)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).Lookups.UsageOwners)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).UsageOwnerOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.ClientPremiumService)(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).PremiumServices)).SyncRoot)).Lookups.UsageOwnersOrganisations)));
			this.PremiumServicesGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.Caption = "Global Price List";
			zDropEditColumnStyleInfo1.ColumnName = "CPS_PriceHeaderCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo2.Caption = "Type";
			zDropEditColumnStyleInfo2.ColumnName = "CPS_Type";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = "#Units";
			zCalcEditColumnStyleInfo1.ColumnName = "CPS_Units";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zDateEditColumnStyleInfo1.Caption = "Start Date";
			zDateEditColumnStyleInfo1.ColumnName = "CPS_StartDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "End Date";
			zDateEditColumnStyleInfo2.ColumnName = "CPS_EndDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zMultiLineTextBoxColumnInfo1.Caption = "Client Ref";
			zMultiLineTextBoxColumnInfo1.ColumnName = "CPS_ClientRef";
			zMultiLineTextBoxColumnInfo1.MinimumEditControlWidth = 300;
			zMultiLineTextBoxColumnInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "Display Order";
			zCalcEditColumnStyleInfo2.ColumnName = "CPS_DisplayOrder";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.Caption = "Comment";
			zTextBoxColumnStyleInfo2.ColumnName = "CPS_Comment";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zGuidDropEditColumnStyleInfo1.BindToList = "Lookups+UsageOwners";
			zGuidDropEditColumnStyleInfo1.Caption = "Usage Owner";
			zGuidDropEditColumnStyleInfo1.ColumnName = "CPS_LCC";
			zGuidDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidDropEditColumnStyleInfo2.BindToList = "Lookups+UsageOwnersOrganisations";
			zGuidDropEditColumnStyleInfo2.Caption = "Organisation";
			zGuidDropEditColumnStyleInfo2.ColumnName = "UsageOwnerOrgPK";
			zGuidDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.PremiumServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.PremiumServicesGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.PremiumServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.PremiumServicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.PremiumServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.PremiumServicesGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.PremiumServicesGrid.ColumnStyles.Add(zMultiLineTextBoxColumnInfo1);
			this.PremiumServicesGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.PremiumServicesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.PremiumServicesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo1);
			this.PremiumServicesGrid.ColumnStyles.Add(zGuidDropEditColumnStyleInfo2);
			this.PremiumServicesGrid.GridId = "8e920e7d-1db8-4468-884c-a50711659ccd";
			this.PremiumServicesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.PremiumServicesGrid.LayoutKey = "PremiumServicesGrid";
			this.PremiumServicesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 53, true);
			this.PremiumServicesGrid.Name = "PremiumServicesGrid";
			this.PremiumServicesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(592, 308, true);
			this.PremiumServicesGrid.TabIndex = 20;
			// 
			// zLabel8
			// 
			this.zLabel8.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(288, 5, true);
			this.zLabel8.Name = "zLabel8";
			this.zLabel8.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 17, true);
			this.zLabel8.TabIndex = 19;
			this.zLabel8.Text = "Premium services apply to all companies sharing the database";
			// 
			// ProductionServiceLabel
			// 
			this.ProductionServiceLabel.ForeColor = System.Drawing.Color.Red;
			this.ProductionServiceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(143, 5, true);
			this.ProductionServiceLabel.Name = "ProductionServiceLabel";
			this.ProductionServiceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 17, true);
			this.ProductionServiceLabel.TabIndex = 23;
			this.ProductionServiceLabel.Text = "Not a production database";
			// 
			// zGuidFindBox1
			// 
			this.zGuidFindBox1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zGuidFindBox1, "LD_OH_BillingParty");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceDatabase)(null)).LD_OH_BillingParty)));
			this.zGuidFindBox1.IsPrimaryKeyFromCodeRequired = false;
			this.zGuidFindBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 28, true);
			this.zGuidFindBox1.Name = "zGuidFindBox1";
			this.zGuidFindBox1.ShouldResize = true;
			this.zGuidFindBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(292, 20, true);
			this.zGuidFindBox1.TabIndex = 24;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 29, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			this.zLabel1.TabIndex = 25;
			this.zLabel1.Text = "DB Usage Owner:";
			this.zLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// ClientPremiumServicesControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zGuidFindBox1);
			this.Controls.Add(this.ProductionServiceLabel);
			this.Controls.Add(this.zLabel9);
			this.Controls.Add(this.ServerCode);
			this.Controls.Add(this.PremiumServicesGrid);
			this.Controls.Add(this.zLabel8);
			this.Name = "ClientPremiumServicesControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(595, 364, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PremiumServicesGrid)).EndInit();
			this.PremiumServicesGrid.ResumeLayout(false);
			this.PremiumServicesGrid.PerformLayout();
			this.zGuidFindBox1.ResumeLayout(true);
			this.zGuidFindBox1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel zLabel9;
		private ZArchitecture.ZTextBox ServerCode;
		private ZArchitecture.ZGrid PremiumServicesGrid;
		private ZArchitecture.ZLabel zLabel8;
		private ZArchitecture.ZLabel ProductionServiceLabel;
		private ZArchitecture.GUI.ZGuidFindBox zGuidFindBox1;
		private ZArchitecture.ZLabel zLabel1;
	}
}
