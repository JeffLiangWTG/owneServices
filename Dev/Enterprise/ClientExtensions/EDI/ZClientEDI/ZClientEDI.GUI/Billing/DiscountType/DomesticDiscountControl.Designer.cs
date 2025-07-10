namespace Enterprise.Client.EDI.Billing.GUI
{
	partial class DomesticDiscountControl
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
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.grid = new Enterprise.ZArchitecture.ZGrid();
			this.multiEntityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.expiryMonthCountBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.maxForeignCompanyCountBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.maxForeignUserCountBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.foreignUserPerecentBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.percentCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.requiresDevelopingCountryCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.singleEntityGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.multiEntityGroupBox.SuspendLayout();
			this.singleEntityGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Billing.Business.DomesticDiscount);
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.grid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscount)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscountLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscount)(null)).Lines)).SyncRoot)).UserCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscountLine)(((System.Collections.IList)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscount)(null)).Lines)).SyncRoot)).Percent)));
			this.grid.CaptionVisible = false;
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.Caption = ">= Users";
			zCalcEditColumnStyleInfo1.ColumnName = "UserCount";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.Caption = "%";
			zCalcEditColumnStyleInfo2.ColumnName = "Percent";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.CopySelectedRowsAllowed = true;
			this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grid.GridId = "675bd5b6-7108-4b00-90ef-fcb1018bc2f4";
			this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grid.LayoutKey = "grid";
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.grid.Name = "grid";
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(308, 59, true);
			this.grid.TabIndex = 1;
			// 
			// multiEntityGroupBox
			// 
			this.multiEntityGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.multiEntityGroupBox.Controls.Add(this.expiryMonthCountBox);
			this.multiEntityGroupBox.Controls.Add(this.maxForeignCompanyCountBox);
			this.multiEntityGroupBox.Controls.Add(this.maxForeignUserCountBox);
			this.multiEntityGroupBox.Controls.Add(this.foreignUserPerecentBox);
			this.multiEntityGroupBox.Controls.Add(this.percentCalcEdit);
			this.multiEntityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 102, true);
			this.multiEntityGroupBox.Name = "multiEntityGroupBox";
			this.multiEntityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 134, true);
			this.multiEntityGroupBox.TabIndex = 2;
			this.multiEntityGroupBox.TabStop = false;
			this.multiEntityGroupBox.Text = "Multiple Entity";
			// 
			// expiryMonthCountBox
			// 
			this.BindingSource.SetBindingMember(this.expiryMonthCountBox, "ExpiryMonthCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscount)(null)).ExpiryMonthCount)));
			this.expiryMonthCountBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9d31c5f7-66ae-4287-85cd-dcbc06e53910", "Months Continues After Any Max. Exceeded");
			this.expiryMonthCountBox.DecimalPlaces = 0;
			this.expiryMonthCountBox.Decimals = 0;
			this.expiryMonthCountBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 103, true);
			this.expiryMonthCountBox.Name = "expiryMonthCountBox";
			this.expiryMonthCountBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.expiryMonthCountBox.TabIndex = 4;
			this.expiryMonthCountBox.Text = "0";
			this.expiryMonthCountBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// maxForeignCompanyCountBox
			// 
			this.BindingSource.SetBindingMember(this.maxForeignCompanyCountBox, "MaxForeignCompanyCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscount)(null)).MaxForeignCompanyCount)));
			this.maxForeignCompanyCountBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("e8d051d9-b8f0-4bb9-9e13-2b320c78fd38", "Max. Foreign Companies");
			this.maxForeignCompanyCountBox.DecimalPlaces = 0;
			this.maxForeignCompanyCountBox.Decimals = 0;
			this.maxForeignCompanyCountBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 82, true);
			this.maxForeignCompanyCountBox.Name = "maxForeignCompanyCountBox";
			this.maxForeignCompanyCountBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.maxForeignCompanyCountBox.TabIndex = 3;
			this.maxForeignCompanyCountBox.Text = "0";
			this.maxForeignCompanyCountBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// maxForeignUserCountBox
			// 
			this.BindingSource.SetBindingMember(this.maxForeignUserCountBox, "MaxForeignUserCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscount)(null)).MaxForeignUserCount)));
			this.maxForeignUserCountBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2bd53273-d939-4fc1-8496-26330547a7a1", "Max. Foreign Users");
			this.maxForeignUserCountBox.DecimalPlaces = 0;
			this.maxForeignUserCountBox.Decimals = 0;
			this.maxForeignUserCountBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 60, true);
			this.maxForeignUserCountBox.Name = "maxForeignUserCountBox";
			this.maxForeignUserCountBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.maxForeignUserCountBox.TabIndex = 2;
			this.maxForeignUserCountBox.Text = "0";
			this.maxForeignUserCountBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// foreignUserPerecentBox
			// 
			this.BindingSource.SetBindingMember(this.foreignUserPerecentBox, "MaxForeignUserPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscount)(null)).MaxForeignUserPercent)));
			this.foreignUserPerecentBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("7d9fabd3-98a2-4efb-b094-1c130af92db2", "Max. Foreign User %");
			this.foreignUserPerecentBox.DecimalPlaces = 2;
			this.foreignUserPerecentBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 39, true);
			this.foreignUserPerecentBox.Name = "foreignUserPerecentBox";
			this.foreignUserPerecentBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.foreignUserPerecentBox.TabIndex = 1;
			this.foreignUserPerecentBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// percentCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.percentCalcEdit, "MultiEntityPercent");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscount)(null)).MultiEntityPercent)));
			this.percentCalcEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("04d93285-1cfc-4fb9-b01e-31e2f76983e1", "Discount %");
			this.percentCalcEdit.DecimalPlaces = 2;
			this.percentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(239, 18, true);
			this.percentCalcEdit.Name = "percentCalcEdit";
			this.percentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(67, 17, true);
			this.percentCalcEdit.TabIndex = 0;
			this.percentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// requiresDevelopingCountryCheckBox
			// 
			this.BindingSource.SetBindingMember(this.requiresDevelopingCountryCheckBox, "RequiresDevelopingCountry");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.Billing.Business.DomesticDiscount)(null)).RequiresDevelopingCountry)));
			this.requiresDevelopingCountryCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.requiresDevelopingCountryCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 0, true);
			this.requiresDevelopingCountryCheckBox.Name = "requiresDevelopingCountryCheckBox";
			this.requiresDevelopingCountryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 16, true);
			this.requiresDevelopingCountryCheckBox.TabIndex = 0;
			this.requiresDevelopingCountryCheckBox.Text = "Requires Developing Country";
			// 
			// singleEntityGroupBox
			// 
			this.singleEntityGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
			this.singleEntityGroupBox.Controls.Add(this.grid);
			this.singleEntityGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 23, true);
			this.singleEntityGroupBox.Name = "singleEntityGroupBox";
			this.singleEntityGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(312, 75, true);
			this.singleEntityGroupBox.TabIndex = 3;
			this.singleEntityGroupBox.TabStop = false;
			this.singleEntityGroupBox.Text = "Single Entity";
			// 
			// DomesticDiscountControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.singleEntityGroupBox);
			this.Controls.Add(this.requiresDevelopingCountryCheckBox);
			this.Controls.Add(this.multiEntityGroupBox);
			this.Name = "DomesticDiscountControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(316, 238, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.multiEntityGroupBox.ResumeLayout(false);
			this.multiEntityGroupBox.PerformLayout();
			this.singleEntityGroupBox.ResumeLayout(false);
			this.singleEntityGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZGrid grid;
		private ZArchitecture.GUI.ZGroupBox multiEntityGroupBox;
		private ZArchitecture.ZCalcEdit percentCalcEdit;
		private ZArchitecture.ZCalcEdit foreignUserPerecentBox;
		private ZArchitecture.ZCalcEdit maxForeignCompanyCountBox;
		private ZArchitecture.ZCalcEdit maxForeignUserCountBox;
		private ZArchitecture.ZCalcEdit expiryMonthCountBox;
		private ZArchitecture.GUI.ZCheckBox requiresDevelopingCountryCheckBox;
		private ZArchitecture.GUI.ZGroupBox singleEntityGroupBox;
	}
}
