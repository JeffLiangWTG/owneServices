using Enterprise.Client.EDI.IdentityCertificate.Business;

namespace Enterprise.Client.EDI.IdentityCertificate
{
	partial class EdiIdentityCertificateFilterControl
	{
		System.ComponentModel.IContainer components = null;

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo validDate = new ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo expiryDate = new ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
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
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowBeginDrag = false;
			this.grid.AllowDragDropWithChanges = false;
			this.BindingSource.SetBindingMember(this.grid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((EdiIdentityCertificate)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).ICE_CertificateThumbprint)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((EdiIdentityCertificate)(null)).ICE_CertificateValidDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((EdiIdentityCertificate)(null)).ICE_CertificateExpiryDate)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).ICE_CertificateIssuedBy)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).ICE_CertificateIssuedTo)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).ICE_ProcessingStatus)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((EdiIdentityCertificate)(null)).ICE_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((EdiIdentityCertificate)(null)).ICE_IsCertificateRevoked)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((EdiIdentityCertificate)(null)).Application.IDA_IsRollback)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).Application.IDA_ApplicationName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).LicenseDatabase.EnterpriseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).LicenseDatabase.EnterpriseCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).LicenseDatabase.CompanyName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).LicenseDatabase.CompanyCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).LicenseDatabase.LD_Product)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).LicenseDatabase.LD_LicenceType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).Application.IDA_ClientID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((EdiIdentityCertificate)(null)).LicenseDatabase.LD_ServerCode)));

			zTextBoxColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("BAB6A61C-A01E-4476-9C1B-2BCAF92A9E5A", "Thumbprint");
			zTextBoxColumnStyleInfo1.ColumnName = "ICE_CertificateThumbprint";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			validDate.CaptionResourceString = ZClientEDI.Res.GetData("0FB94913-26FD-400E-AFC2-F6C3861EA248", "Valid Date");
			validDate.ColumnName = "ICE_CertificateValidDate";
			validDate.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			expiryDate.CaptionResourceString = ZClientEDI.Res.GetData("B509D791-5490-434F-8513-8B833DDFAFA9", "Expiry Date");
			expiryDate.ColumnName = "ICE_CertificateExpiryDate";
			expiryDate.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("746DD6B7-5E0A-46B8-A9A5-390D8C219259", "Issued By");
			zTextBoxColumnStyleInfo2.ColumnName = "ICE_CertificateIssuedBy";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("C2B15B0F-6DF9-4E01-8657-2A4F3B10AEB4", "Issued To");
			zTextBoxColumnStyleInfo3.ColumnName = "ICE_CertificateIssuedTo";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo11.CaptionResourceString = ZClientEDI.Res.GetData("9B5DBCC6-2A50-42FD-846F-60181E7693A2", "Processing Status");
			zTextBoxColumnStyleInfo11.ColumnName = "ICE_ProcessingStatus";
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo12.CaptionResourceString = ZClientEDI.Res.GetData("5A8A85CF-C9CA-478F-A9A7-8FA42233B860", "Application Name");
			zTextBoxColumnStyleInfo12.ColumnName = "Application+IDA_ApplicationName";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo13.CaptionResourceString = ZClientEDI.Res.GetData("052C3545-EAEF-4E0C-9DA5-D78573AA018F", "AWS Issuing CA");
			zTextBoxColumnStyleInfo13.ColumnName = "ICE_CARoot";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("0443161A-E404-4F03-A99D-33592BDE7E81", "Is Active");
			zCheckBoxColumnStyleInfo2.ColumnName = "ICE_IsActive";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = ZClientEDI.Res.GetData("A67F8AE9-2BDF-44B3-8D4D-6C53454BF596", "Is Revoked");
			zCheckBoxColumnStyleInfo3.ColumnName = "ICE_IsCertificateRevoked";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("743DBDD5-A847-43A5-BC54-E89F122012F2", "License Is Active");
			zCheckBoxColumnStyleInfo4.ColumnName = "LicenseDatabase+LD_IsActive";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("CDA39CED-96E1-4162-BCF0-D1E59EEBF7CD", "Is Rolled Back");
			zCheckBoxColumnStyleInfo5.ColumnName = "Application+IDA_IsRollback";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("08D17FF9-0961-478E-B190-3580E9EA0273", "Enterprise ID");
			zTextBoxColumnStyleInfo4.ColumnName = "LicenseDatabase+EnterpriseID";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("4DEBB9F6-A6EF-46EC-8026-FBAC33C7D002", "Enterprise Code");
			zTextBoxColumnStyleInfo5.ColumnName = "LicenseDatabase+EnterpriseCode";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("120DDFFB-9A4D-4058-A41B-13354CACA571", "Company Name");
			zTextBoxColumnStyleInfo6.ColumnName = "LicenseDatabase+CompanyName";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo7.CaptionResourceString = ZClientEDI.Res.GetData("029CACF9-2CEA-46BB-8654-58F4A170BE39", "Product Code");
			zTextBoxColumnStyleInfo7.ColumnName = "LicenseDatabase+LD_Product";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo8.CaptionResourceString = ZClientEDI.Res.GetData("76404EF1-4B7D-42E3-A71E-D8EE1F6FDF58", "License Type");
			zTextBoxColumnStyleInfo8.ColumnName = "LicenseDatabase+LD_LicenceType";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo9.CaptionResourceString = ZClientEDI.Res.GetData("2FD0B47C-7C0C-4326-9FA5-C65B03AA38E7", "Company Code");
			zTextBoxColumnStyleInfo9.ColumnName = "LicenseDatabase+CompanyCode";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.CaptionResourceString = ZClientEDI.Res.GetData("576F03E1-C4F5-4892-9ABE-F7C2BE26200C", "Client ID");
			zTextBoxColumnStyleInfo10.ColumnName = "Application+IDA_ClientID";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo15.CaptionResourceString = ZClientEDI.Res.GetData("157ea2e9-eb02-4b8a-952b-db1ef34d523a", "Server Code");
			zTextBoxColumnStyleInfo15.ColumnName = "LicenseDatabase+LD_ServerCode";
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo16.CaptionResourceString = ZClientEDI.Res.GetData("93F47184-BB23-4AD9-8214-7080A9207D6E", "Sequence Number");
			zTextBoxColumnStyleInfo16.ColumnName = "ICE_SequenceNumber";
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(validDate);
			this.grid.ColumnStyles.Add(expiryDate);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 413, true);
			this.grid.TabIndex = 10;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(EdiIdentityCertificate);
			// 
			// EdiTrustedSystemFilterControl
			// 
			this.Name = "EdiIdentityCertificateFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(731, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
