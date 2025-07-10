namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment
{
	partial class ApportionmentControl
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.apportionmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.apportionmentDetailsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.apportionmentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.apportionmentDetailsGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice);
			// 
			// apportionmentGroupBox
			// 
			this.apportionmentGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|7a05ba22-b870-480f-b334-09d15893af35", "Charge Line Apportionment Details");
			this.apportionmentGroupBox.Controls.Add(this.apportionmentDetailsGrid);
			this.apportionmentGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.apportionmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.apportionmentGroupBox.Name = "apportionmentGroupBox";
			this.apportionmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 255, true);
			this.apportionmentGroupBox.TabIndex = 0;
			this.apportionmentGroupBox.TabStop = false;
			// 
			// apportionmentDetailsGrid
			// 
			this.apportionmentDetailsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.apportionmentDetailsGrid, "Lines.Apportionments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).Branch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).Department)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).ApportionmentFactor)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).CompanyPostToGLAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).ForeignApportionedAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).ForeignGST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).ExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).LocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).LocalGST)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).CompanyLocalExchangeRate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).CompanyLocalCurrency)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).CompanyLocalAmount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).IntercompanyGLAccount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).AccountingPeriod)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).TaxBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionment)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoiceLine)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.IntercompanyCostsApportionmentInvoice)(null)).Lines)).SyncRoot)).Apportionments)).SyncRoot)).TaxBranchName)));
			this.apportionmentDetailsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|0b64b9b2-2f28-450c-8829-3dba20e9f9fb", "Company");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Company";
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|0f681d57-d291-4776-b8f0-e99ee2e73ade", "Branch");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "Branch";
			zGuidFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|f3578288-4102-4f93-8cf0-452290ee6f2b", "Department");
			zGuidFindBoxColumnStyleInfo3.ColumnName = "Department";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|8211912f-6304-4d4f-904e-c199101eba3b", "Apportionment Factor");
			zCalcEditColumnStyleInfo1.ColumnName = "ApportionmentFactor";
			zCalcEditColumnStyleInfo1.Decimals = 3;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|2442fe36-7b0f-4d72-9375-c81ac404e8cc", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "Description";
			zGuidFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|f97d4dc8-5c22-4d79-9797-9fefa3df6435", "Company Post To GL Account");
			zGuidFindBoxColumnStyleInfo4.ColumnName = "CompanyPostToGLAccount";
			zGuidFindBoxColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|204b3d44-11c8-4811-b57e-bdcbc6a00f93", "Foreign Amount");
			zCalcEditColumnStyleInfo2.ColumnName = "ForeignApportionedAmount";
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|48b465ba-4519-4521-9578-a29a48013e39", "Foreign GST");
			zCalcEditColumnStyleInfo3.ColumnName = "ForeignGST";
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|70d67380-14fc-49d3-82fa-c89175e8e71e", "Exchange Rate");
			zCalcEditColumnStyleInfo4.ColumnName = "ExchangeRate";
			zCalcEditColumnStyleInfo4.Decimals = 6;
			zCalcEditColumnStyleInfo4.IsReadOnly = true;
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|f53c5208-510b-4e01-9f25-fa5de120abcf", "Local Amount");
			zCalcEditColumnStyleInfo5.ColumnName = "LocalAmount";
			zCalcEditColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|db840de0-4b63-46e0-bed8-595af908c038", "Local GST");
			zCalcEditColumnStyleInfo6.ColumnName = "LocalGST";
			zCalcEditColumnStyleInfo6.IsReadOnly = true;
			zCalcEditColumnStyleInfo7.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo7.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|794591dc-22a0-4b86-8945-6dbb84b00681", "Company Local Exchange Rate");
			zCalcEditColumnStyleInfo7.ColumnName = "CompanyLocalExchangeRate";
			zCalcEditColumnStyleInfo7.Decimals = 6;
			zCalcEditColumnStyleInfo7.IsReadOnly = true;
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|f6bee90d-92d0-48a9-a509-1389399d46e3", "Company Local Currency");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "CompanyLocalCurrency";
			zCodeFindBoxColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo8.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo8.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|b02cc0d8-9fa3-4d0c-8198-47ebf7402fa0", "Company Local Amount");
			zCalcEditColumnStyleInfo8.ColumnName = "CompanyLocalAmount";
			zCalcEditColumnStyleInfo8.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|2cd9f9cd-db08-4da3-b390-f9cc4ae844c4", "Intercompany GL Account");
			zGuidFindBoxColumnStyleInfo5.ColumnName = "IntercompanyGLAccount";
			zGuidFindBoxColumnStyleInfo5.IsReadOnly = true;
			zCalcEditColumnStyleInfo9.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo9.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|387b6c30-c3a4-4145-87a5-135b0c56759e", "Accounting Period");
			zCalcEditColumnStyleInfo9.ColumnName = "AccountingPeriod";
			zCalcEditColumnStyleInfo9.Decimals = 0;
			zCalcEditColumnStyleInfo9.IsReadOnly = true;
			zCalcEditColumnStyleInfo9.ShowGroupSeparators = false;
			zGuidFindBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|F8D7EFD8-2B23-4847-A302-5C59AD9C32D7", "Tax Branch");
			zGuidFindBoxColumnStyleInfo6.ColumnName = "TaxBranch";
			zGuidFindBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentControl|968F4804-34E8-4B02-B35E-932C8229D636", "Tax Branch Name");
			zTextBoxColumnStyleInfo2.ColumnName = "TaxBranchName";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo4);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo7);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo8);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo5);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo9);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo6);
			this.apportionmentDetailsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.apportionmentDetailsGrid.GridId = "42d2ca2a-944a-485d-b882-d0f759f0f765";
			this.apportionmentDetailsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.apportionmentDetailsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.apportionmentDetailsGrid.LayoutKey = "zGrid1";
			this.apportionmentDetailsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.apportionmentDetailsGrid.Name = "apportionmentDetailsGrid";
			this.apportionmentDetailsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(737, 236, true);
			this.apportionmentDetailsGrid.TabIndex = 0;
			// 
			// ApportionmentControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.apportionmentGroupBox);
			this.Name = "ApportionmentControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(743, 255, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.apportionmentGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.apportionmentDetailsGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZGroupBox apportionmentGroupBox;
		public Enterprise.ZArchitecture.ZGrid apportionmentDetailsGrid;
	}
}
