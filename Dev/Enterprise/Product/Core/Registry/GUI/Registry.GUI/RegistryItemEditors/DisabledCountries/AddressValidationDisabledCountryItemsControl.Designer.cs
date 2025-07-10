namespace Enterprise.Registry.GUI
{
	partial class AddressValidationDisabledCountryItemsControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.AddressValidationDisabledCountryItemsGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressValidationDisabledCountryItemsGrid)).BeginInit();
			this.AddressValidationDisabledCountryItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.AddressValidationDisabledCountryItemCollection);
			// 
			// AddressValidationDisabledCountryItemsGrid
			// 
			this.AddressValidationDisabledCountryItemsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.AddressValidationDisabledCountryItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).CountryPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).CountryCollection)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).CountryName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForOverrideAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForOrgAddress)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForAdminPanel)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForPerson)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForApplicant)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForStaff)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForCompany)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForBranch)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForSalesInquiry)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForHVLVConsignment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.AddressValidationDisabledCountryItem)(null)).DisabledForSupplierBookingLine)));
			this.AddressValidationDisabledCountryItemsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "CountryCollection";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("455c0c85-83aa-4536-a7ad-d477c77416f6", "Country/Region");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "CountryPK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("5f2a6e9c-d792-44f3-977f-24289e357cdf", "Country/Region Name");
			zTextBoxColumnStyleInfo1.ColumnName = "CountryName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("02145a85-b265-4838-88d9-18d48fdba962", "Override Address");
			zCheckBoxColumnStyleInfo1.ColumnName = "DisabledForOverrideAddress";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("81803048-42a9-4771-a285-949a00542fc1", "Organization Address");
			zCheckBoxColumnStyleInfo2.ColumnName = "DisabledForOrgAddress";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("0618348a-0a8e-4b94-adc9-d3ba4f190a31", "Admin Panel");
			zCheckBoxColumnStyleInfo3.ColumnName = "DisabledForAdminPanel";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("9d0362b6-142b-4dad-89ba-f98b896cd4fd", "Person");
			zCheckBoxColumnStyleInfo4.ColumnName = "DisabledForPerson";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("077891fe-69db-45ab-b8b2-4548b00024a3", "Job Applicant");
			zCheckBoxColumnStyleInfo5.ColumnName = "DisabledForApplicant";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6646c375-cd51-4fb3-89f3-d9f42016a1f1", "Staff");
			zCheckBoxColumnStyleInfo6.ColumnName = "DisabledForStaff";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("a89359c8-1f0e-4f33-a354-61082323eccb", "Company");
			zCheckBoxColumnStyleInfo7.ColumnName = "DisabledForCompany";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("53ee94ea-78fa-4820-aa07-a639df64511e", "Branch");
			zCheckBoxColumnStyleInfo8.ColumnName = "DisabledForBranch";
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d2aba398-a9a4-4821-a60b-93ed6ea4d120", "Sales Inquiry");
			zCheckBoxColumnStyleInfo9.ColumnName = "DisabledForSalesInquiry";
			zCheckBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo10.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("4d009128-5360-499e-91af-a6cde249a8ae", "HVLV Consignment");
			zCheckBoxColumnStyleInfo10.ColumnName = "DisabledForHVLVConsignment";
			zCheckBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo11.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("acfdbb91-3979-476b-8ca7-57a06defc6b2", "Supplier Booking Line");
			zCheckBoxColumnStyleInfo11.ColumnName = "DisabledForSupplierBookingLine";
			zCheckBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);
			this.AddressValidationDisabledCountryItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo11);
			this.AddressValidationDisabledCountryItemsGrid.CopySelectedRowsAllowed = false;
			this.AddressValidationDisabledCountryItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AddressValidationDisabledCountryItemsGrid.GridId = "de55eeeb-a8bf-4de5-8a2f-3de7de245a1f";
			this.AddressValidationDisabledCountryItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AddressValidationDisabledCountryItemsGrid.LayoutKey = "RatesPriorityGrid";
			this.AddressValidationDisabledCountryItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AddressValidationDisabledCountryItemsGrid.Name = "AddressValidationDisabledCountryItemsGrid";
			this.AddressValidationDisabledCountryItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 320, true);
			this.AddressValidationDisabledCountryItemsGrid.TabIndex = 4;
			// 
			// AddressValidationDisabledCountryItemsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AddressValidationDisabledCountryItemsGrid);
			this.Name = "AddressValidationDisabledCountryItemsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(467, 320, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.AddressValidationDisabledCountryItemsGrid)).EndInit();
			this.AddressValidationDisabledCountryItemsGrid.ResumeLayout(false);
			this.AddressValidationDisabledCountryItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.ZGrid AddressValidationDisabledCountryItemsGrid;
	}
}
