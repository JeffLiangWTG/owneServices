using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityCertificate.Module.Testing
{
	[TestedType(typeof(EdiIdentityCertificateModule))]
	public class EdiIdentityCertificateModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return Modules.ClientModuleRegistration.EdiIdentityCertificate;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			Factory.Save();

			var item = collection.AddNew() as EdiIdentityCertificate;
			item.ICE_IDA = application.PK;
			item.ICE_CertificateValidDate = ZDateTime.UtcNow;
			item.ICE_CertificateExpiryDate = ZDateTime.UtcNow.AddYears(1);
			collection.Factory.Save();
			base.AddTestObjects(collection);
		}

		public void TestAllowNew()
		{
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(false, module.AllowNew);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(false, module.AllowDelete);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(false, module.AllowEdit);
			}
		}

		public void TestAllowView()
		{
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(false, module.AllowView);
			}
		}

		public void TestActionMenuHasCancelCertItem()
		{
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Revoke Certificate");
				AssertNotEquals(null, menuItem);
			}

			EDISecurityCheckpoints.EdiIdentityCertificateEditCertificate.IsAllowed = false;
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Revoke Certificate");
				AssertEquals(null, menuItem);
			}
		}

		public void TestActionMenuHasResetQueItem()
		{
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Reset Status To QUE");
				AssertNotEquals(null, menuItem);
			}

			EDISecurityCheckpoints.EdiIdentityCertificateEditCertificate.IsAllowed = false;
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Reset Status To QUE");
				AssertEquals(null, menuItem);
			}
		}

		public void TestNotSelectOneHasError()
		{
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Revoke Certificate");
				menuItem.PerformClick();
				AssertEquals("Please select Certificate to revoke.", ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestHasNoComRecordHasError()
		{
			var cert = CreateTestRecords("PRC", false, false, "csr");
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm(cert))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Revoke Certificate");
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true).First() as ZDisplayGrid;
				grid.Select(0);
				menuItem.PerformClick();
				AssertEquals("All selected records must be in the completed state.", ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#region CreateTestRecords

		EdiIdentityCertificate CreateTestRecords(ZString processingStatus, ZBool isCertificateRevoked, ZBool isB2CApplicationRollback, ZString csr)
		{
			var cert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			var certData = new X509Certificate2(Encoding.UTF8.GetBytes(certificate));
			cert.ICE_CertificateData = certData.RawData;
			cert.ICE_CertificateThumbprint = "";
			cert.ICE_CertificateValidDate = ZDateTime.Now.AddDays(-3);
			cert.ICE_CertificateExpiryDate = ZDateTime.Now.AddDays(3);
			cert.ICE_CertificateIssuedTo = "IssuedTo";
			cert.ICE_CertificateIssuedBy = "IssuedBy";
			cert.ICE_ProcessingStatus = processingStatus;
			cert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			cert.ICE_IsActive = true;
			cert.ICE_IsCertificateRevoked = isCertificateRevoked;
			cert.Application.IDA_IsRollback = isB2CApplicationRollback;
			cert.ICE_CertificateSigningRequest = csr;
			Factory.Save();
			return cert;
		}

		#endregion

		public void TestCancelCertificateOperation()
		{
			var cert = CreateTestRecords("COM", false, false, "csr");
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm(cert))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Revoke Certificate");
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true).First() as ZDisplayGrid;
				grid.Select(0);
				menuItem.PerformClick();
				AssertEquals(false, cert.ICE_IsCertificateRevoked);
				AssertEquals(false, cert.Application.IDA_IsRollback);
				AssertEquals(EdiIdentityCertificateProcessingStatus.Codes.CAN, cert.ICE_ProcessingStatus);
			}
		}

		#region basic RowData

		const string certificate = @"-----BEGIN CERTIFICATE-----
MIID6DCCAtCgAwIBAgIQROOIIrCQ7LRi0hKk4rLEdDANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwMjI0MDIzMTU5WhcNMjQwMjI0MDMzMTU5
WjBxMQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lk
ZW50aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5K
RzExEDAOBgNVBAcMB1dURyBOSkcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQCVJt0UDpMaAOMxUiBtstzgTVtBC541t5+mGCS8wOmOdMCspwU1jkC6w0VB
sh0ZwFkGJyu51aEOkqES2oTR/G7/ISj7iZO2Hx4C7dlShm31gDlp2sgNkWls+Acg
HsAvu1GpHEXQjMv1gtPZr5uC3ys9uY5zm5XyirCno4+AEzntZ9Bxvupu1cmuS9Z7
xFdFXuBq4pmb7s6vGp3bMMabEimvlRkg1EiaIJLlLVxbb4til3jmh+wkf6o3R2KS
V65+f4F6XZvA5vsB7rUv7HDvuSNNGVdtFSQ9RxnzKOSyemypSM+CPQyDNcWJZSjd
51o58wEEv7jQrvu5NmeggXMiFDR3AgMBAAGjfDB6MAkGA1UdEwQCMAAwHwYDVR0j
BBgwFoAUxRkU5BydYIvZdbTEsI6oOFnvAuIwHQYDVR0OBBYEFMUZFOQcnWCL2XW0
xLCOqDhZ7wLiMA4GA1UdDwEB/wQEAwIFoDAdBgNVHSUEFjAUBggrBgEFBQcDAQYI
KwYBBQUHAwIwDQYJKoZIhvcNAQELBQADggEBAFd9ujf8VlE2UxAqjTmaAddX/FKU
NHSWILGSjOZm6Lb2nz0267Al8G71NiLdwDAPEH7sBwKUIXZBKoLJU9pBxSshxkf0
lX+4bKUvHkiDf/GDg9X1RqX7sVgglPaR3FmQNxEvbs6lD+rWar6ZtHOX2Kaqrb/7
szao5Hnoo7U8CL4Lm0ZDZ+nxP9gwq3W83KDLsGHHNld0i9zl55Lzt1vCoUsqJlsJ
XPyDaHxT4m7lrI+fv8HAC2KclNkEf9xYTqDfwHpbWKQtV54gYG568D+kyptHDsgv
7qiJtBmvKf+O4g7p9NPjcQdfoHq2IrKzZO9kOANlcVE0UCXBk4Msi4VguJ0=
-----END CERTIFICATE-----";

		public static ZGuid objectId = ZGuid.NewZGuid();

		#endregion

		public void TestActionMenuHasResetToQUE()
		{
			var cert = CreateTestRecordsForQue("FAL", false, false, "csr");
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm(cert))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Reset Status To QUE");
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true).First() as ZDisplayGrid;
				grid.Select(0);
				menuItem.PerformClick();
				AssertNotEquals(null, menuItem);
			}
		}

		public void TestActionMenuResetToQUEWhenNoJobSelected()
		{
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Reset Status To QUE");
				menuItem.PerformClick();
				AssertEquals("Please select one or more Jobs to reset the Status.", ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActionMenuResetToQUEWhenJobSelected()
		{
			var cert = CreateTestRecordsForQue("FAL", false, false, "csr");
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm(cert))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Reset Status To QUE");
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true).First() as ZDisplayGrid;
				grid.Select(0);
				menuItem.PerformClick();
				AssertEquals("Reset Status to QUE for 1 Job(s).", ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActionMenuResetToQUEWhenWrongJobSelected()
		{
			var cert = CreateTestRecordsForQue("QUE", false, false, "csr");
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm(cert))
			{
				var menuItem = module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Reset Status To QUE");
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true).First() as ZDisplayGrid;
				grid.Select(0);
				menuItem.PerformClick();
				AssertEquals("Please Select Certificates with FAL Status Only.", ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		EdiIdentityCertificate CreateTestRecordsForQue(ZString processingStatus, ZBool isCertificateRevoked, ZBool isB2CApplicationRollback, ZString csr)
		{
			var cert = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			var certData = new X509Certificate2(Encoding.UTF8.GetBytes(certificate2));
			cert.ICE_CertificateData = certData.RawData;
			cert.ICE_CertificateThumbprint = "";
			cert.ICE_CertificateValidDate = ZDateTime.Now.AddDays(-3);
			cert.ICE_CertificateExpiryDate = ZDateTime.Now.AddDays(3);
			cert.ICE_CertificateIssuedTo = "IssuedTo";
			cert.ICE_CertificateIssuedBy = "IssuedBy";
			cert.ICE_ProcessingStatus = processingStatus;
			cert.Application.IDA_ClientID = ZGuid.NewZGuid().ToString();
			cert.ICE_IsActive = true;
			cert.ICE_IsCertificateRevoked = isCertificateRevoked;
			cert.Application.IDA_IsRollback = isB2CApplicationRollback;
			cert.ICE_CertificateSigningRequest = csr;
			Factory.Save();
			return cert;
		}

		const string certificate2 = @"-----BEGIN CERTIFICATE-----
MIID6DCCAtCgAwIBAgIQROOIIrCQ7LRi0hKk4rLEdDANBgkqhkiG9w0BAQsFADBx
MQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lkZW50
aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5KRzEx
EDAOBgNVBAcMB1dURyBOSkcwHhcNMjMwMjI0MDIzMTU5WhcNMjQwMjI0MDMzMTU5
WjBxMQswCQYDVQQGEwJDTjERMA8GA1UECgwIamF5d3RnQ0ExHDAaBgNVBAsME0lk
ZW50aXR5QW5kU2VjdXJpdHkxEDAOBgNVBAgMB05hbmppbmcxDTALBgNVBAMMBE5K
RzExEDAOBgNVBAcMB1dURyBOSkcwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEK
AoIBAQCVJt0UDpMaAOMxUiBtstzgTVtBC541t5+mGCS8wOmOdMCspwU1jkC6w0VB
sh0ZwFkGJyu51aEOkqES2oTR/G7/ISj7iZO2Hx4C7dlShm31gDlp2sgNkWls+Acg
HsAvu1GpHEXQjMv1gtPZr5uC3ys9uY5zm5XyirCno4+AEzntZ9Bxvupu1cmuS9Z7
xFdFXuBq4pmb7s6vGp3bMMabEimvlRkg1EiaIJLlLVxbb4til3jmh+wkf6o3R2KS
V65+f4F6XZvA5vsB7rUv7HDvuSNNGVdtFSQ9RxnzKOSyemypSM+CPQyDNcWJZSjd
51o58wEEv7jQrvu5NmeggXMiFDR3AgMBAAGjfDB6MAkGA1UdEwQCMAAwHwYDVR0j
BBgwFoAUxRkU5BydYIvZdbTEsI6oOFnvAuIwHQYDVR0OBBYEFMUZFOQcnWCL2XW0
xLCOqDhZ7wLiMA4GA1UdDwEB/wQEAwIFoDAdBgNVHSUEFjAUBggrBgEFBQcDAQYI
KwYBBQUHAwIwDQYJKoZIhvcNAQELBQADggEBAFd9ujf8VlE2UxAqjTmaAddX/FKU
NHSWILGSjOZm6Lb2nz0267Al8G71NiLdwDAPEH7sBwKUIXZBKoLJU9pBxSshxkf0
lX+4bKUvHkiDf/GDg9X1RqX7sVgglPaR3FmQNxEvbs6lD+rWar6ZtHOX2Kaqrb/7
szao5Hnoo7U8CL4Lm0ZDZ+nxP9gwq3W83KDLsGHHNld0i9zl55Lzt1vCoUsqJlsJ
XPyDaHxT4m7lrI+fv8HAC2KclNkEf9xYTqDfwHpbWKQtV54gYG568D+kyptHDsgv
7qiJtBmvKf+O4g7p9NPjcQdfoHq2IrKzZO9kOANlcVE0UCXBk4Msi4VguJ0=
-----END CERTIFICATE-----";

		public void TestActionMenuItemsHasDownloadCertificateMenu()
		{
			EDISecurityCheckpoints.EdiIdentityCertificateDownloadCertificate.IsAllowed = true;
			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertNotNull(module.FormActionMenu);
				AssertNotNull(module.FormActionMenu.FindByText("&Actions"));
				AssertNotNull(module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Download Certificate"));
			}
		}

		public void TestNotSelectOneCertificateToDownload()
		{
			EDISecurityCheckpoints.EdiIdentityCertificateDownloadCertificate.IsAllowed = true;

			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Download Certificate").PerformClick();
				var message = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Please select one certificate to download.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCannotDownloadCertificateIfProceesingStatusIsNotCom()
		{
			EDISecurityCheckpoints.EdiIdentityCertificateDownloadCertificate.IsAllowed = true;

			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_ProcessingStatus = "QUE";
			Factory.Save();

			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm(certificate))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true).First() as ZDisplayGrid;
				grid.Select(0);

				module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Download Certificate").PerformClick();
				AssertEquals("Certificates are only available to download when the certificate status is set to 'COM' (Complete). Please try again by selecting certificates that have status set as 'COM' only.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCannotDownloadCertificateIfDataIsNull()
		{
			EDISecurityCheckpoints.EdiIdentityCertificateDownloadCertificate.IsAllowed = true;

			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_ProcessingStatus = "COM";
			Factory.Save();

			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm(certificate))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true).First() as ZDisplayGrid;
				grid.Select(0);

				module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Download Certificate").PerformClick();
				AssertEquals("This certificate data is null.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCannotDownloadCertificateIfSelectedMoreThanOne()
		{
			EDISecurityCheckpoints.EdiIdentityCertificateDownloadCertificate.IsAllowed = true;

			var certificate1 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate1.ICE_ProcessingStatus = "COM";
			Factory.Save();

			var certificate2 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate2.ICE_ProcessingStatus = "COM";
			Factory.Save();

			var collection = new EdiIdentityCertificateCollection(Factory);

			using (var module = (EdiIdentityCertificateModule)ZModuleFactory.Instance.Create(GetModuleID()))
			using (var form = new ZForm(collection))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true).First() as ZDisplayGrid;
				grid.SelectAllElements();
				module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Download Certificate").PerformClick();
				AssertEquals("Please select only one certificate to download.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCanDownloadCertificateIfProcessingStatusIsCom()
		{
			EDISecurityCheckpoints.EdiIdentityCertificateDownloadCertificate.IsAllowed = true;

			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_ProcessingStatus = "COM";
			var certificateData = new byte[] { 1, 2, 3 };
			certificate.ICE_CertificateData = certificateData;
			Factory.Save();

			using (var module = new MockFunctionDownloadCertificateModule())
			using (var form = new ZForm(certificate))
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();
				module.PerformSearch_ForTest();
				var grid = module.EmbeddedControl.Controls.Find("FilteredGrid", true).First() as ZDisplayGrid;
				grid.Select(0);

				module.FormActionMenu.FindByText("&Actions").MenuItems.FindByText("Download Certificate").PerformClick();
				AssertEquals("Download certificate successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		class MockFunctionDownloadCertificateModule : EdiIdentityCertificateModule
		{
			protected override bool DownloadCertificate(ZBlob certificateData)
			{
				return true;
			}
		}
	}
}
