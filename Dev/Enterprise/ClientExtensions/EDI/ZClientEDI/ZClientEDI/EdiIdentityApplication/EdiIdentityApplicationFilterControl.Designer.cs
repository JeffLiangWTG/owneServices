using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.EDI.IdentityApplication
{
	partial class EdiIdentityApplicationFilterControl
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();

			this.grid.AllowBeginDrag = false;
			this.grid.AllowDragDropWithChanges = false;
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Business.EdiIdentityApplication)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).IDA_ApplicationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).IDA_ApplicationModule)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).IDA_ClientID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.EdiIdentityApplication)(null)).IDA_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.EdiIdentityApplication)(null)).IDA_IsRollback)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).IDA_RedirectUrlStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Business.EdiIdentityApplication)(null)).IsLicenceActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).LicenceDatabase.EnterpriseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).LicenceDatabase.EnterpriseCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).LicenceDatabase.CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).LicenceDatabase.CompanyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).LicenceProductCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).LicenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).LicenceDatabase.LD_ServerCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).Tenant.IDT_Name)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Business.EdiIdentityApplication)(null)).Tenant.IDT_TenantId)));
			//grid column
			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("72DB8BD0-A1AE-456E-BFD2-2D420263E065", "Application Name");
			zTextBoxColumnStyleInfo1.ColumnName = "IDA_ApplicationName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("33ABA0FC-90D7-444E-981C-17AD1377FB2B", "Client ID");
			zTextBoxColumnStyleInfo2.ColumnName = "IDA_ClientID";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("9E63138D-9BB3-46CB-8938-67BF8A53788C", "Is Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "IDA_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("35E13D73-240B-4E15-98F8-3EA55353E576", "Is Roll-back");
			zCheckBoxColumnStyleInfo2.ColumnName = "IDA_IsRollback";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("4E299794-A1F8-4631-ACF6-8207FBD0A3E7", "Redirect URL Status");
			zTextBoxColumnStyleInfo3.ColumnName = "IDA_RedirectUrlStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("C9150AB6-2C0E-493A-9A1C-FA442B7A6496", "License Is Active");
			zCheckBoxColumnStyleInfo3.ColumnName = "IsLicenceActive";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("C6A6ACCF-AEB8-4066-9138-4DD0E6E3E0D0", "Enterprise ID");
			zTextBoxColumnStyleInfo4.ColumnName = "LicenceDatabase+EnterpriseID";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("7085890F-F363-4468-A470-9E07090F3FFE", "Enterprise Code");
			zTextBoxColumnStyleInfo5.ColumnName = "LicenceDatabase+EnterpriseCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("DA5D9D30-579D-4C92-BDBB-69798B370CDC", "Company Name");
			zTextBoxColumnStyleInfo6.ColumnName = "LicenceDatabase+CompanyName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.CaptionResourceString = ZClientEDI.Res.GetData("90E561D1-D5C1-4DC3-A4B4-FDEF5B3AF545", "Company Code");
			zTextBoxColumnStyleInfo7.ColumnName = "LicenceDatabase+CompanyCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.CaptionResourceString = ZClientEDI.Res.GetData("01A96FC4-7A36-497F-8E79-83275D07B8E5", "Product Code");
			zTextBoxColumnStyleInfo8.ColumnName = "LicenceProductCode";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.CaptionResourceString = ZClientEDI.Res.GetData("E5D20AC6-4811-4B9A-A022-D194B1A54D02", "License Type");
			zTextBoxColumnStyleInfo9.ColumnName = "LicenceType";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.CaptionResourceString = ZClientEDI.Res.GetData("64E0BB72-9CD7-4E48-8B93-4443CFB61A36", "Server Code");
			zTextBoxColumnStyleInfo10.ColumnName = "LicenceDatabase+LD_ServerCode";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo11.CaptionResourceString = ZClientEDI.Res.GetData("D359F030-C464-4DC1-A03B-696BABBBD25A", "Application Module");
			zTextBoxColumnStyleInfo11.ColumnName = "IDA_ApplicationModule";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.CaptionResourceString = ZClientEDI.Res.GetData("EFACFA20-8AD6-4C67-9333-1F170DA67D13", "Tenant Name");
			zTextBoxColumnStyleInfo12.ColumnName = "Tenant+IDT_Name";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo13.ColumnName = "TenantID";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 413, true);
			this.grid.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EdiIdentityApplication);
			// 
			// EdiIdentityApplicationFilterControl
			// 
			this.Name = "EdiIdentityApplicationFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
