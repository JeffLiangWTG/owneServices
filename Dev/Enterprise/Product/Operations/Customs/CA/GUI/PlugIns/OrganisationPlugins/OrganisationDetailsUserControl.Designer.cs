namespace Enterprise.Customs.CA.GUI
{
	partial class OrganisationDetailsUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SafeFoodLicenseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SafeFoodLicensesGrid = new Enterprise.ZArchitecture.ZGrid();
			this.CFIAFeePaymentMethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.AccountSecurityPasswordTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AccountSecurityNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DetailsGroupBox.SuspendLayout();
			this.SafeFoodLicenseGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.SafeFoodLicensesGrid)).BeginInit();
			this.SafeFoodLicensesGrid.SuspendLayout();
			this.CFIAFeePaymentMethodDropEdit.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.OrgImpAddInfo);
			//
			// DetailsGroupBox
			//
			this.DetailsGroupBox.Controls.Add(this.SafeFoodLicenseGroupBox);
			this.DetailsGroupBox.Controls.Add(this.CFIAFeePaymentMethodDropEdit);
			this.DetailsGroupBox.Controls.Add(this.AccountSecurityPasswordTextBox);
			this.DetailsGroupBox.Controls.Add(this.AccountSecurityNumberTextBox);
			this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DetailsGroupBox.Name = "DetailsGroupBox";
			this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 423, true);
			this.DetailsGroupBox.TabIndex = 0;
			this.DetailsGroupBox.TabStop = false;
			//
			// SafeFoodLicenseGroupBox
			//
			this.SafeFoodLicenseGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("B90E9AC6-97B9-4D19-8378-A1942C42E3FF", "CFIA Safe Food Licenses");
			this.SafeFoodLicenseGroupBox.Controls.Add(this.SafeFoodLicensesGrid);
			this.SafeFoodLicenseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(33, 61, true);
			this.SafeFoodLicenseGroupBox.Name = "SafeFoodLicenseGroupBox";
			this.SafeFoodLicenseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 189, true);
			this.SafeFoodLicenseGroupBox.TabIndex = 6;
			this.SafeFoodLicenseGroupBox.TabStop = false;
			//
			// SafeFoodLicensesGrid
			//
			this.SafeFoodLicensesGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.SafeFoodLicensesGrid, "SafeFoodLicenses");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).SafeFoodLicenses)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.SafeFoodLicense)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).SafeFoodLicenses)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.SafeFoodLicense)(((System.Collections.IList)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).SafeFoodLicenses)).SyncRoot)).CY_Data)));
			this.SafeFoodLicensesGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "CY_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233);
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "CY_Data";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(233);
			this.SafeFoodLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.SafeFoodLicensesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.SafeFoodLicensesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SafeFoodLicensesGrid.GridId = "3fae4b5a-8095-4a7f-8d73-d841f85936de";
			this.SafeFoodLicensesGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.SafeFoodLicensesGrid.LayoutKey = "SafeFoodLicensesGrid";
			this.SafeFoodLicensesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.SafeFoodLicensesGrid.Name = "SafeFoodLicensesGrid";
			this.SafeFoodLicensesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(507, 172, true);
			this.SafeFoodLicensesGrid.TabIndex = 0;
			//
			// CFIAFeePaymentMethodDropEdit
			//
			this.CFIAFeePaymentMethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CFIAFeePaymentMethodDropEdit, "ZO_CFIAFeePaymentMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_CFIAFeePaymentMethod)));
			this.CFIAFeePaymentMethodDropEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("06bd8fd4-2d71-4d3c-a877-46217644f549", "CFIA Payment Method", "CFIA Fee Payment Method");
			this.CFIAFeePaymentMethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 40, true);
			this.CFIAFeePaymentMethodDropEdit.Name = "CFIAFeePaymentMethodDropEdit";
			this.CFIAFeePaymentMethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 17, true);
			this.CFIAFeePaymentMethodDropEdit.TabIndex = 5;
			//
			// AccountSecurityPasswordTextBox
			//
			this.BindingSource.SetBindingMember(this.AccountSecurityPasswordTextBox, "ZO_AccountSecirityPassword");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_AccountSecirityPassword)));
			this.AccountSecurityPasswordTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("dd393688-ff14-4839-92a7-476d4bcdd976", "Password", "Account Security Password", "");
			this.AccountSecurityPasswordTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(445, 21, true);
			this.AccountSecurityPasswordTextBox.Name = "AccountSecurityPasswordTextBox";
			this.AccountSecurityPasswordTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.AccountSecurityPasswordTextBox.TabIndex = 4;
			//
			// AccountSecurityNumberTextBox
			//
			this.BindingSource.SetBindingMember(this.AccountSecurityNumberTextBox, "ZO_AccountSecurityNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CA.Business.OrgImpAddInfo)(null)).ZO_AccountSecurityNumber)));
			this.AccountSecurityNumberTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("1aa001af-c29c-45f5-9f10-74de90b6f34c", "Account Security Number");
			this.AccountSecurityNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(249, 19, true);
			this.AccountSecurityNumberTextBox.Name = "AccountSecurityNumberTextBox";
			this.AccountSecurityNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.AccountSecurityNumberTextBox.TabIndex = 3;
			//
			// OrganisationDetailsUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DetailsGroupBox);
			this.Name = "OrganisationDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(760, 423, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DetailsGroupBox.ResumeLayout(false);
			this.DetailsGroupBox.PerformLayout();
			this.SafeFoodLicenseGroupBox.ResumeLayout(false);
			this.SafeFoodLicenseGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.SafeFoodLicensesGrid)).EndInit();
			this.SafeFoodLicensesGrid.ResumeLayout(false);
			this.SafeFoodLicensesGrid.PerformLayout();
			this.CFIAFeePaymentMethodDropEdit.ResumeLayout(true);
			this.CFIAFeePaymentMethodDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		private ZArchitecture.ZTextBox AccountSecurityPasswordTextBox;
		private ZArchitecture.ZTextBox AccountSecurityNumberTextBox;
		private ZArchitecture.GUI.ZDropEdit CFIAFeePaymentMethodDropEdit;
		private ZArchitecture.GUI.ZGroupBox SafeFoodLicenseGroupBox;
		private ZArchitecture.ZGrid SafeFoodLicensesGrid;
	}
}
