namespace Enterprise.Registry.GUI
{
	partial class CreditReportsRegistryControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();

			this.CreditReportRegistryGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.CreditReportRegistryGrid)).BeginInit();
			this.CreditReportRegistryGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Registry.Business.CreditReportItemCollection);
			// 
			// CreditReportRegistryGrid
			// 
			this.CreditReportRegistryGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.CreditReportRegistryGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Registry.Business.CreditReportItem)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CreditReportItem)(null)).CountryCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Registry.Business.CreditReportItem)(null)).Country)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CreditReportItem)(null)).CountryEnabledForCompany)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CreditReportItem)(null)).CountryEnabledForOrganisation)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CreditReportItem)(null)).ComprehensiveReportEnabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CreditReportItem)(null)).FailureRiskEnabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CreditReportItem)(null)).LatePaymentRiskEnabled)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Registry.Business.CreditReportItem)(null)).CommercialBureauEnquiryEnabled)));
			this.CreditReportRegistryGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("eaa75cb1-3078-455b-a059-659c93157a72", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "CountryCode";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("cc9def2f-cfd5-4f3a-b57d-1b9c7924bbb6", "Country/Region");
			zTextBoxColumnStyleInfo2.ColumnName = "Country";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(128);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("8fb9da9e-158b-4d8e-9b37-ae9ce12619c2", "Enable Company Reports");
			zCheckBoxColumnStyleInfo1.ColumnName = "CountryEnabledForCompany";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("57b3252c-4d4b-4f3f-bf5e-ead33c68b0ac", "Enable Organization Reports");
			zCheckBoxColumnStyleInfo2.ColumnName = "CountryEnabledForOrganisation";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("d767b883-973a-425a-af5c-cf2de1691b04", "Comprehensive Report");
			zCheckBoxColumnStyleInfo3.ColumnName = "ComprehensiveReportEnabled";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("09197833-5245-4616-a8f5-4309f0901ff8", "Failure Risk");
			zCheckBoxColumnStyleInfo4.ColumnName = "FailureRiskEnabled";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("6ca000c0-a660-44ba-88a9-0afe2cc949a8", "Late Payment Risk");
			zCheckBoxColumnStyleInfo5.ColumnName = "LatePaymentRiskEnabled";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(104);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Registry.GUI.Res.GetData("519bdb87-788e-47e0-ade8-dddf9431c8ab", "Commercial Bureau Enquiry");
			zCheckBoxColumnStyleInfo6.ColumnName = "CommercialBureauEnquiryEnabled";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
			this.CreditReportRegistryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.CreditReportRegistryGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.CreditReportRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.CreditReportRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.CreditReportRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.CreditReportRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.CreditReportRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.CreditReportRegistryGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.CreditReportRegistryGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CreditReportRegistryGrid.GridId = "128dfb7a-52e3-49bd-91ed-e5c2ab997a76";
			this.CreditReportRegistryGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.CreditReportRegistryGrid.LayoutKey = "CreditReportRegistryGrid";
			this.CreditReportRegistryGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CreditReportRegistryGrid.Name = "CreditReportRegistryGrid";
			this.CreditReportRegistryGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 400, true);
			this.CreditReportRegistryGrid.TabIndex = 0;
			// 
			// CreditReportsRegistryControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CreditReportRegistryGrid);
			this.Name = "CreditReportsRegistryControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(594, 400, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.CreditReportRegistryGrid)).EndInit();
			this.CreditReportRegistryGrid.ResumeLayout(false);
			this.CreditReportRegistryGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZGrid CreditReportRegistryGrid;

		#endregion
	}
}
