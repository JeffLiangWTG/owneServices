using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.IdentityRedirectUrl.Business;
using Enterprise.Client.EDI.IdentityTenant.Business;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using File = System.IO.File;

namespace Enterprise.Client.EDI.IdentityApplication.GUI.Testing
{
	[TestedType(typeof(EdiIdentityApplicationForm))]
	public class EdiIdentityApplicationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var ediIdentityApplication = Factory.New<EdiIdentityApplication>();
			return new EdiIdentityApplicationForm(ediIdentityApplication);
		}

		public void TestTenantIDReadOnly()
		{
			var app = Factory.NewWithValidTestData<EdiIdentityApplication>();
			var tenant = Factory.NewWithValidTestData<EdiIdentityTenant>();
			tenant.IDT_TenantId = ZGuid.BrettsGuid.ToString();
			app.IDA_IDT = tenant.PK;
			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();
				Assert(form.TenantIdTextBoxForTest.ReadOnly);
				AssertEquals(ZGuid.BrettsGuid.ToString(), form.TenantIdTextBoxForTest.Text);
			}
		}

		public void TestCheckApplicationName()
		{
			var collection = CreateAWSPrivateCACollection();
			var database = Factory.NewWithValidTestData<LicenceDatabase>();
			var database2 = Factory.NewWithValidTestData<LicenceDatabase>();
			var app2 = Factory.New<EdiIdentityApplication>();
			app2.IDA_LD = database2.PK;
			app2.IDA_ApplicationName = "QuinceTest01";
			app2.IDA_Product = "CW1";
			app2.IDA_ApplicationType = "TST";
			Factory.Save();
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_LD = database.PK;
			app.IDA_Product = "CW1";
			app.IDA_ApplicationType = "TST";
			UnitTestUserNotification.Instance.ClearMessages();
			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();
				var cert = app.Certificates.AddNew();
				cert.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
				cert.ICE_CertificateSigningRequest = Csr;
				form.FireSaveButton();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(app.IDA_ApplicationNameInfo.HasError("Please enter an Application Name."));
				UnitTestUserNotification.Instance.ClearMessages();
				app.IDA_ApplicationName = "QuinceTest01";
				form.FireSaveButton();
				AssertEquals("The value of EdiIdentityApplication|IDA_ApplicationName must be unique on EdiIdentityApplication. The duplicate value(s) are: (QuinceTest01).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCheckApplicationType()
		{
			var collection = CreateAWSPrivateCACollection();
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_Product = "CW1";
			app.IDA_ApplicationName = "Test";
			app.IDA_ApplicationType = "XXX";
			UnitTestUserNotification.Instance.ClearMessages();
			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();
				var cert = app.Certificates.AddNew();
				cert.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
				cert.ICE_CertificateSigningRequest = Csr;
				form.FireSaveButton();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(app.IDA_ApplicationTypeInfo.HasError("Enter a valid selection."));
				UnitTestUserNotification.Instance.ClearMessages();
				app.IDA_ApplicationType = "TST";
				form.FireSaveButton();
				AssertNoErrors(app.ApplicationTypeInfo);
			}
		}

		public void TestCheckProduct()
		{
			var collection = CreateAWSPrivateCACollection();
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_ApplicationType = "TST";
			app.IDA_ApplicationName = "Test";
			app.IDA_Product = "XXX";
			UnitTestUserNotification.Instance.ClearMessages();
			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();
				var cert = app.Certificates.AddNew();
				cert.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
				cert.ICE_CertificateSigningRequest = Csr;
				form.FireSaveButton();
				AssertEquals("There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(app.IDA_ProductInfo.HasError("Enter a valid selection."));
				UnitTestUserNotification.Instance.ClearMessages();
				app.IDA_Product = "WTA";
				form.FireSaveButton();
				AssertNoErrors(app.ProductInfo);
			}
		}

		public void TestMakeInactiveMenuItemIsNull()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Make Inactive");
				AssertNotNull(menuItem);
			}

			app1.IDA_IsActive = false;
			using (var form2 = new EdiIdentityApplicationForm(app1))
			{
				form2.Show();
				var menuItem = GetActionsMenuItem(form2, "Make Active");
				AssertNotNull(menuItem);
			}
		}

		public void TestMakeInactiveAndActive()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Make Inactive");
				AssertNotNull(menuItem);
				menuItem.PerformClick();
				AssertEquals("app1.IDA_IsActive", false, app1.IDA_IsActive);
				AssertEquals("app1.IDA_IsRollback", false, app1.IDA_IsRollback);
			}

			app1.IDA_IsRollback = true;
			Factory.Save();

			using (var form2 = new EdiIdentityApplicationForm(app1))
			{
				form2.Show();
				var menuItem = GetActionsMenuItem(form2, "Make Active");
				AssertNotNull(menuItem);
				menuItem.PerformClick();
				AssertEquals("app1.IDA_IsActive", true, app1.IDA_IsActive);
				AssertEquals("app1.IDA_IsRollback", false, app1.IDA_IsRollback);
			}
		}

		public void TestGenerateCertificateEnable()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Generate Certificate (Upload CSR)");
				AssertNotNull(menuItem);
			}

			app1.IDA_LD = ZGuid.BrettsGuid;
			using (var form2 = new EdiIdentityApplicationForm(app1))
			{
				form2.Show();
				var menuItem = GetActionsMenuItem(form2, "Generate Certificate (Upload CSR)");
				AssertNull(menuItem);
			}

			var app2 = Factory.New<EdiIdentityApplication>();
			app2.Certificates.AddNew();
			using (var form = new EdiIdentityApplicationForm(app2))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Generate Certificate (Upload CSR)");
				AssertNull(menuItem);
				menuItem = GetActionsMenuItem(form, "Renew Certificate (Upload CSR)");
				AssertNotNull(menuItem);
			}
		}

		public void TestGenerateCertificateEnableWithSecurityCheckpoint()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Generate Certificate (Upload CSR)");
				AssertNotNull(menuItem);
			}

			EDISecurityCheckpoints.EdiIdentityCertificateEditCertificate.IsAllowed = false;
			using (var form2 = new EdiIdentityApplicationForm(app1))
			{
				form2.Show();
				var menuItem = GetActionsMenuItem(form2, "Generate Certificate (Upload CSR)");
				AssertNull(menuItem);
			}
		}

		public void TestGenerateCertificate()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ApplicationName = "QuinceTest01";
			app1.IDA_Product = "WTA";
			app1.IDA_ApplicationType = "TST";

			var collection = CreateAWSPrivateCACollection();

			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new EdiIdentityApplicationForm(app1))
			using (var file = TempFile.NewWithExtension("csr"))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var menuItem = GetActionsMenuItem(form, "Generate Certificate (Upload CSR)");
				File.WriteAllText(file.Filename, Csr);
				menuItem.PerformClick();
				var certificate = app1.Certificates[0];
				certificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
				form.FireSaveButton();
				AssertEquals("Should have no error message.", null, UnitTestUserNotification.Instance.LastMessage.Text);
				certificate.ReloadSafe();
				AssertEquals(Csr, certificate.ICE_CertificateSigningRequest.ToString());
			}
		}

		public void TestGenerateCertificate_OnlyAddCertificateOnceOneTime()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ApplicationName = "QuinceTest01";
			app1.Certificates.AddNew();
			var collection = CreateAWSPrivateCACollection();

			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new EdiIdentityApplicationForm(app1))
			using (var file = TempFile.NewWithExtension("csr"))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var menuItem = GetActionsMenuItem(form, "Renew Certificate (Upload CSR)");
				File.WriteAllText(file.Filename, Csr);
				menuItem.PerformClick();
				AssertEquals("Should have error message.", "Only one certificate can be added at a time.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGenerateCertificateWithDuplicatedCsr()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			var certificate1 = app1.Certificates.AddNew();
			certificate1.ICE_CertificateSigningRequest = Csr;
			Factory.Save();

			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_ApplicationName = "QuinceTest01";

			var collection = CreateAWSPrivateCACollection();

			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new EdiIdentityApplicationForm(app))
			using (var file = TempFile.NewWithExtension("csr"))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var menuItem = GetActionsMenuItem(form, "Generate Certificate (Upload CSR)");
				File.WriteAllText(file.Filename, Csr);
				menuItem.PerformClick();
				AssertEquals("Should have error message.", "The certificate is already in use, please add a new certificate.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveFormWithoutCa()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ApplicationName = "QuinceTest01";

			var collection = CreateAWSPrivateCACollection();

			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new EdiIdentityApplicationForm(app1))
			using (var file = TempFile.NewWithExtension("csr"))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				var menuItem = GetActionsMenuItem(form, "Generate Certificate (Upload CSR)");
				File.WriteAllText(file.Filename, Csr);
				menuItem.PerformClick();
				var certificate = app1.Certificates[0];
				form.FireSaveButton();
				AssertEquals("Should have error message.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertHasError(certificate.ICE_CARootInfo, "Please enter an AWS Issuing CA.");
			}
		}

		public void TestGridColumnName()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ApplicationName = "QuinceTest01";

			var expectedCertificateListOfColumns = new[]
			{
				$"{EdiIdentityCertificate.Schema.ICE_CARoot} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{EdiIdentityCertificate.Schema.ICE_CertificateSigningRequest} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{EdiIdentityCertificate.Schema.ICE_CertificateThumbprint} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{EdiIdentityCertificate.Schema.ICE_CertificateIssuedBy} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{EdiIdentityCertificate.Schema.ICE_CertificateIssuedTo} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{EdiIdentityCertificate.Schema.ICE_IsActive} (ZCheckBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{EdiIdentityCertificate.Schema.ICE_IsCertificateRevoked} (ZCheckBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{EdiIdentityCertificate.Schema.ICE_ProcessingStatus} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{EdiIdentityCertificate.Schema.ICE_SystemCreateTimeUtc} (ZDateEditColumnStyleInfo) IsVisible:True IsUnavailable:False"
			};

			var expectedRedirectUrlListOfColumns = new[]
			{
				$"{AutoEdiIdentityRedirectUrl.Schema.IAR_ApplicationName} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AutoEdiIdentityRedirectUrl.Schema.IAR_RedirectType} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AutoEdiIdentityRedirectUrl.Schema.IAR_RedirectUrl} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
			};

			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				var grid1 = form.GetField("CertificatesGrid") as ZGrid;
				var realListOfColumns1 = grid1.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible} IsUnavailable:{x.IsUnavailable}").ToArray();
				AssertArrayEqualsByElements(expectedCertificateListOfColumns, realListOfColumns1);
				var grid2 = form.GetField("RedirectUrlGrid") as ZGrid;
				var realListOfColumns2 = grid2.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible} IsUnavailable:{x.IsUnavailable}").ToArray();
				AssertArrayEqualsByElements(expectedRedirectUrlListOfColumns, realListOfColumns2);
			}
		}

		public void TestDatabaseGridColumnName()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ApplicationName = "QuinceTest01";
			app1.IDA_LD = licenceDatabase.PK;
			Factory.Save();

			var expectedCertificateListOfColumns = new[]
			{
				$"{LicenceDatabase.Schema.LD_IsActive} (ZCheckBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{LicenceDatabase.Schema.LD_Product} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{LicenceDatabase.Schema.LD_DatabaseNumber} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{LicenceDatabase.Schema.LD_HostedLocation} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{LicenceDatabase.Schema.LD_LicenceType} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{LicenceDatabase.Schema.LD_ReleaseRing} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{LicenceDatabase.Schema.LD_ServerCode} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{"BillingModel"} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{"WebAccessOrg+OH_Code"} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{"EnterpriseCode"} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{"EnterpriseID"} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
			};

			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				var grid1 = form.GetField("DatabasesGrid") as ZGrid;
				var realListOfColumns1 = grid1.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible} IsUnavailable:{x.IsUnavailable}").ToArray();
				AssertArrayEqualsByElements(expectedCertificateListOfColumns, realListOfColumns1);
			}
		}

		public void TestDatabaseGrid_ShowLicenceTab()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var app = Factory.NewWithValidTestData<EdiIdentityApplication>();
			app.IDA_LD = licenceDatabase.PK;
			Factory.Save();

			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();
				var licenseTabPage = FindLicenceDatabaseTabPage(form);
				var mainTabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTabControl;
				AssertNotNull(mainTabControl);
				mainTabControl.SelectedTab = licenseTabPage;

				var databaseGrid = licenseTabPage.Controls.Find("DatabasesGrid", true).FirstOrDefault() as ZGrid;
				AssertNotNull(databaseGrid);
				Assert(databaseGrid.ReadOnly);
				AssertEquals(1, databaseGrid.ListManager.List.Count);

				var database = (LicenceDatabase)databaseGrid.ListManager.List[0];
				AssertEquals(licenceDatabase.PK, database.PK);
			}
		}

		public void TestDatabaseGrid_HideLicenceTab()
		{
			var app = Factory.NewWithValidTestData<EdiIdentityApplication>();
			Factory.Save();

			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();
				var licenseTabPage = FindLicenceDatabaseTabPage(form);
				AssertNull(licenseTabPage);
			}
		}

		ZTabPage FindLicenceDatabaseTabPage(EdiIdentityApplicationForm form) => form.Controls.Find("LicenceDatabaseTabPage", true).FirstOrDefault() as ZTabPage;

		public void TestSaveFormWithoutCertificate()
		{
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_ApplicationName = "QuinceTest01";
			UnitTestUserNotification.Instance.ClearMessages();
			using var form = new EdiIdentityApplicationForm(app);
			form.Show();
			form.FireSaveButton();

			CombineAssertions(() =>
			{
				AssertEquals("the application should be saved into db", true, app.IsInDatabase);
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestGridActionMenuHasRevokeCertItem()
		{
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_ApplicationName = "QuinceTest01";
			var cert2 = app.Certificates.AddNew();
			cert2.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			cert2.ICE_CertificateSigningRequest = Csr2;
			Factory.Save();
			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();
				var certsGrid = (ZGrid)form.Controls.Find("zGrid1", true).Single();
				var certsMenuItemList = certsGrid.ContextMenu.MenuItems.ToList<ZMenuItem>();
				var menuItem = certsMenuItemList.Find(x => x.Text == "Revoke");
				AssertNotNull(menuItem);
			}

			EDISecurityCheckpoints.EdiIdentityCertificateEditCertificate.IsAllowed = false;
			using (var form2 = new EdiIdentityApplicationForm(app))
			{
				form2.Show();
				var certsGrid = (ZGrid)form2.Controls.Find("zGrid1", true).Single();
				var certsMenuItemList = certsGrid.ContextMenu.MenuItems.ToList<ZMenuItem>();
				var menuItem = certsMenuItemList.Find(x => x.Text == "Revoke");
				AssertNull(menuItem);
			}
		}

		public void TestSaveFormWithRedirectUrl()
		{
			var tenant = Factory.New<EdiIdentityTenant>();
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ApplicationName = "QuinceTest01";
			app1.IDA_Product = "WTA";
			app1.IDA_ApplicationType = "TST";
			app1.IDA_IDT = tenant.PK;
			var certificate = app1.Certificates.AddNew();
			certificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			certificate.ICE_CertificateSigningRequest = Csr;
			Factory.Save();

			var collection = CreateAWSPrivateCACollection();

			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				var redirectUrl = app1.RedirectUrls.AddNew();
				redirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.SinglePage;
				redirectUrl.IAR_ApplicationName = "UrlTest001";
				redirectUrl.IAR_RedirectUrl = "https://www.example.com";
				form.FireSaveButton();

				AssertEquals("Should have no error message.", null, UnitTestUserNotification.Instance.LastMessage.Text);
				var app2 = new BusinessObjectFactory().Load<EdiIdentityApplication>(app1.PK);
				CombineAssertions(() =>
				{
					AssertEquals(app2.IDA_ApplicationName, "QuinceTest01");
					AssertNotNull(app2.RedirectUrls[0]);
					AssertEquals(app2.RedirectUrls[0].IAR_RedirectType, EdiIdentityRedirectType.Codes.SinglePage);
					AssertEquals(app2.RedirectUrls[0].IAR_ApplicationName, "UrlTest001");
					AssertEquals(app2.RedirectUrls[0].IAR_RedirectUrl, "https://www.example.com");
				});
			}
		}

		public void TestSaveFormWithRedirectUrlErrors()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ApplicationName = "QuinceTest01";
			var certificate = app1.Certificates.AddNew();
			certificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			certificate.ICE_CertificateSigningRequest = Csr;
			Factory.Save();

			var collection = CreateAWSPrivateCACollection();

			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				var redirectUrl = app1.RedirectUrls.AddNew();
				redirectUrl.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
				redirectUrl.IAR_RedirectUrl = "https://www.example.com";
				form.FireSaveButton();

				AssertEquals("Should have error message.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertHasError(redirectUrl.IAR_ApplicationNameInfo, "Please enter a value.");

				UnitTestUserNotification.Instance.ClearMessages();
				redirectUrl.IAR_ApplicationName = "Test001";
				redirectUrl.IAR_RedirectUrl = string.Empty;
				form.FireSaveButton();
				AssertEquals("Should have error message.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertHasError(redirectUrl.IAR_RedirectUrlInfo, "Please enter a value.");

				UnitTestUserNotification.Instance.ClearMessages();
				redirectUrl.IAR_RedirectUrl = "www.example.com";
				form.FireSaveButton();
				AssertEquals("Should have error message.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertHasError(redirectUrl.IAR_RedirectUrlInfo, "Please enter the valid URL.");

				UnitTestUserNotification.Instance.ClearMessages();
				redirectUrl.IAR_RedirectType = string.Empty;
				redirectUrl.IAR_RedirectUrl = "http://www.example.com";
				form.FireSaveButton();
				AssertEquals("Should have error message.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertHasError(redirectUrl.IAR_RedirectTypeInfo, "Please enter a value.");

				UnitTestUserNotification.Instance.ClearMessages();
				redirectUrl.IAR_RedirectType = "Tst";
				form.FireSaveButton();
				AssertEquals("Should have error message.", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertHasError(redirectUrl.IAR_RedirectTypeInfo, "Enter a valid selection.");
			}
		}

		public void TestRevokeCertificate()
		{
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_ApplicationName = "QuinceTest01";
			var cert1 = app.Certificates.AddNew();
			cert1.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.CAN;
			cert1.ICE_CertificateSigningRequest = Csr;
			var cert2 = app.Certificates.AddNew();
			cert2.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			cert2.ICE_CertificateSigningRequest = Csr2;
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessages();
			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();
				AssertEquals(2, app.Certificates.Count);
				form.RevokeCertificate();
				AssertEquals("Should have error message.", "Please select at least one certificate to revoke.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessages();
				form.CertificateGridForTest.SelectAllElements();
				form.RevokeCertificate();
				AssertStartsWith("Should have error message.", "All selected records must be in the completed state", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			var app2 = Factory.New<EdiIdentityApplication>();
			app2.IDA_ApplicationName = "QuinceTest02";
			var cert3 = app2.Certificates.AddNew();
			cert3.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			cert3.ICE_CertificateSigningRequest = Csr3;
			var cert4 = app2.Certificates.AddNew();
			cert4.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			cert4.ICE_CertificateSigningRequest = Csr4;
			Factory.Save();
			using (var form = new EdiIdentityApplicationForm(app2))
			{
				form.Show();
				AssertEquals(2, app2.Certificates.Count);
				UnitTestUserNotification.Instance.ClearMessages();
				form.CertificateGridForTest.SelectAllElements();
				form.RevokeCertificate();
				app2.ReloadSafe();
				AssertStartsWith("Should have confirm message.", "You are about to revoke 2 certificates and remove the corresponding certificate in application from our Azure AD B2C server", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(app2.IDA_ApplicationName, "QuinceTest02");
				AssertEquals(app2.Certificates.Count, 2);
				Assert(app2.Certificates.All(x => x.ICE_ProcessingStatus == EdiIdentityCertificateProcessingStatus.Codes.CAN));
			}
		}

		public void TestRollbackApplication()
		{
			var app = Factory.New<EdiIdentityApplication>();
			app.IDA_ApplicationName = "QuinceTest01";
			var cert1 = app.Certificates.AddNew();
			cert1.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			cert1.ICE_CertificateSigningRequest = Csr;
			var cert2 = app.Certificates.AddNew();
			cert2.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
			cert2.ICE_CertificateSigningRequest = Csr2;
			var url1 = app.RedirectUrls.AddNew();
			url1.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			url1.IAR_RedirectUrl = "https://www.example1.com";
			var url2 = app.RedirectUrls.AddNew();
			url2.IAR_RedirectType = EdiIdentityRedirectType.Codes.Web;
			url2.IAR_RedirectUrl = "https://www.example2.com";
			Factory.Save();
			UnitTestUserNotification.Instance.ClearMessages();
			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();
				AssertEquals(2, app.Certificates.Count);
				AssertEquals(2, app.RedirectUrls.Count);
				UnitTestUserNotification.Instance.ClearMessages();
				form.RollbackApplication();
				AssertStartsWith("Should have message.", "You are about to roll-back this application and remove the corresponding certificates and redirect URL in application from our Azure AD B2C server.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(app.IDA_ApplicationName, "QuinceTest01");
				Assert(app.IDA_IsRollback);
			}
		}

		public void TestRollbackApplicationEnable()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ClientID = ZGuid.BrettsGuid.ToString();
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				var menuItem = GetActionsMenuItem(form, "Rollback Application");
				AssertNull(menuItem);
			}

			Factory.Save();
			using (var form2 = new EdiIdentityApplicationForm(app1))
			{
				form2.Show();
				var menuItem = GetActionsMenuItem(form2, "Rollback Application");
				AssertNotNull(menuItem);
			}
		}

		public void TestRedirectUrlGridReadOnlyWhenLicenceIsNotEmpty()
		{
			var licenceDatabase = Factory.NewWithValidTestData<LicenceDatabase>();
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ClientID = ZGuid.BrettsGuid.ToString();
			app1.IDA_LD = licenceDatabase.PK;
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				Assert(form.RedirectUrlGridForTest.ReadOnly);
			}
		}

		public void TestRedirectUrlGridNotReadOnlyWhenLicenceIsEmpty()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ClientID = ZGuid.BrettsGuid.ToString();
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				Assert(!form.RedirectUrlGridForTest.ReadOnly);
			}
		}

		public void TestAllUIReadOnlyWhenApplicationIsInActive()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ApplicationName = "Test";
			app1.IDA_IsActive = false;
			Factory.Save();
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				Assert(form.ApplicationNameTextboxForTest.ReadOnly);
				Assert(form.ClientIdTextBoxForTest.ReadOnly);
				Assert(form.ApplicationTypeDropBoxForTest.ReadOnly);
				Assert(form.ProductDropBoxForTest.ReadOnly);
				Assert(!form.IsActiveCheckBoxForTest.Enabled);
				Assert(!form.IsRollbackCheckBoxForTest.Enabled);
				Assert(form.CertificateGridForTest.ReadOnly);
				Assert(form.RedirectUrlGridForTest.ReadOnly);
			}
		}

		public void TestAllUINotReadOnlyWhenApplicationIsActive()
		{
			var app1 = Factory.New<EdiIdentityApplication>();
			app1.IDA_ApplicationName = "Test";
			Factory.Save();
			using (var form = new EdiIdentityApplicationForm(app1))
			{
				form.Show();
				Assert(form.ApplicationNameTextboxForTest.ReadOnly);
				Assert(form.ClientIdTextBoxForTest.ReadOnly);
				Assert(!form.ApplicationTypeDropBoxForTest.ReadOnly);
				Assert(!form.ProductDropBoxForTest.ReadOnly);
				Assert(!form.IsActiveCheckBoxForTest.Enabled);
				Assert(!form.IsRollbackCheckBoxForTest.Enabled);
				Assert(!form.CertificateGridForTest.ReadOnly);
				Assert(!form.RedirectUrlGridForTest.ReadOnly);
			}
		}

		public void TestPermissionGridIsVisibleOnAzureApplicationForm_S2STApplication()
		{
			var app = Factory.NewWithValidTestData<EdiIdentityApplication>();
			using (var form = new EdiIdentityApplicationForm(app))
			{
				form.Show();

				var permissionGridColumns = form.PermissionGridForTest.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.CaptionResourceString.Caption).ToArray();
				CombineAssertions(() =>
				{
					Assert(nameof(form.PermissionGroupBoxForTest), form.PermissionGroupBoxForTest.Visible);
					Assert(nameof(form.PermissionGridForTest), form.PermissionGridForTest.Visible);
					AssertContainsExactElementsInExactOrder(new[] { "Scope" }, permissionGridColumns);
				});
			}
		}

		MenuItem GetActionsMenuItem(Form form, string text)
		{
			MenuItem actionsMenu = GetActionsMenu(form);
			foreach (MenuItem actionItem in actionsMenu.MenuItems)
			{
				if (actionItem.Text == text && actionItem.Visible)
				{
					return actionItem;
				}
			}

			return null;
		}

		MenuItem GetActionsMenu(Form form)
		{
			foreach (MenuItem item in form.Menu.MenuItems)
			{
				if (item.Text == "Actio&ns")
				{
					return item;
				}
			}

			return null;
		}

		public AWSPrivateCACollection CreateAWSPrivateCACollection()
		{
			var awsPrivateCACollection = new AWSPrivateCACollection();
			var awsPrivateCa = awsPrivateCACollection.AddNew();
			awsPrivateCa.AccessKey = "TestAccessKey";
			awsPrivateCa.SecretKey = "TestSecretKey";
			awsPrivateCa.IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			awsPrivateCa.Arn = "pc:ca:arn";
			awsPrivateCa.IsEnabled = ZBool.True;
			return awsPrivateCACollection;
		}

		public SystemProductCollection CreateSystemProductCollection()
		{
			var collection = new SystemProductCollection();
			var product = collection.AddNew();
			product.Code = "WTA";
			product.Description = "WTA Product";
			return collection;
		}

		protected override void SetUp()
		{
			EDIDataRegistry.Instance.AWSPrivateCAListManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateAWSPrivateCACollection());
			EDIDataRegistry.Instance.SystemProductMappings.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateSystemProductCollection());
			base.SetUp();
		}

		const string Csr = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGFDSLFKSDFLDSJFLKDSJFLAoMCGpheXd0Z0NBMRww
GgYDVQQLDBNJZGVudGl0eUFuZFNlY3VyaXR5MQ8wDQYDVQQIDAZTeWRuZXkxDDAK
BgNVBAMMA0NBMTEMMAoGA1UEBwwDV1RHMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAinpVLEBYZdwzwC7OAOy9WT7bBqlFQU0bFWONu2WGXBblXJw1j3+e
I+GerBPO6ZwbNXSo+vr0q6dDLlD5pJsP3Rld6UwB4gquJyJfAGIuA8SQcJrYzkl6
xq3f8gL3q65f8HS/UiP+exO5VocCGjQhhvjPbsSZjF4FIWsOMb9E7pvNQ+MbJ+qs
MEIK3HO2LtgwiQdNZeUxZR4TJN7Vv3iimWG3QSAl1Q7dz4iT/dD4zId4xKK+yXPk
dczgfZQN5LnQ9Nt1/Qi97Xj8Wbu5VOSpeTb8eDP1Jezhq64Xsm7W27U6SYemF2gi
Ef5HLxt4A04XFiPbgnbm7xHArm86TnQ3AQIDAQABoCIwIAYJKoZIhvcNAQkOMRMw
ETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAlzcyPQj5P1iRe
HkTInm8x7lKnfbsuphlyg+HD2j2AGWtKwCR532piCTA0E2zUKqh+E0GO68eH6ou2
YbIzGk/MTUVJ2xZkYUuCNhXEdChfj0xou5Z7QFo+kZ5QUd+fQ08Jp/lZGMZrFGmc
AfwU+hn6ySqY0lFf4mSVY7hUUjPBIkO6YREhvEHuy/ShIjmbjNXCntjkd5zW8gV4
GUE/1cXy10RU86fZj6arGToDL2W7bbtfo6fu+EhNi66sawn5ipIDJ1ab91zn7iOt
yQnZCmN3mvB9tS1PH8KWEYJoYHdXojaC88IuC4xdSb8X4d9vuMtwidQEz7MpL9Li
NBmVuj0B
-----END CERTIFICATE REQUEST-----";

		const string Csr2 = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGFDSLFKSDFLDSJFLKDSJFLAoMCGpheXd0Z0NBMRww
GgYDVQQLDBNJZGVudGl0eUFuZFNlY3VyaXR5MQ8wDQYDVQQIDAZTeWRuZXkxDDAK
BgNVBAMMA0NBMTEMMAoGA1UEBwwDV1RHMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAinpVLEBYZdwzwC7OAOy9WT7bBqlFQU0bFWONu2WGXBblXJw1j3+e
I+GerBPO6ZwbNXSo+vr0q6dDLlD5pJsP3Rld6UwB4gquJyJfAGIuA8SQcJrYzkl6
xq3f8gL3q65f8HS/UiP+exO5VocCGjQhhvjPbsSZjF4FIWsOMb9E7pvNQ+MbJ+qs
MEIK3HO2LtgwiQdNZeUxZR4TJN7Vv3iimWG3QSAl1Q7dz4iT/dD4zId4xKK+yXPk
dczgfZQN5LnQ9Nt1/Qi97Xj8Wbu5VOSpeTb8eDP1Jezhq64Xsm7W27U6SYemF2gi
Ef5HLxt4A04XFiPbgnbm7xHArm86TnQ3AQIDAQABoCIwIAYJKoZIhvcNAQkOMRMw
ETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAlzcyPQj5P1iRe
HkTInm8x7lKnfbsuphlyg+HD2j2AGWtKwCR532piCTA0E2zUKqh+E0GO68eH6ou2
YbIzGk/MTUVJ2xZkYUuCNhXEdChfj0xou5Z7QFo+kZ5QUd+fQ08Jp/lZGMZrFGmc
AfwU+hn6ySqY0lFf4mSVY7hUUjPBIkO6YREhvEHuy/ShIjmbjNXCntjkd5zW8gV4
GUE/1cXy10RU86fZj6arGToDL2W7bbtfo6fu+EhNi66sawn5ipIDJ1ab91zn7iOt
yQnZCmN3mvB9tS1PH8KWEYJoYHdXojaC88IuC4xdSb8X4d9vuMtwidQEz7MpL9Li
NBmVuj0C
-----END CERTIFICATE REQUEST-----";

		const string Csr3 = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGFDSLFKSDFLDSJFLKDSJFLAoMCGpheXd0Z0NBMRww
GgYDVQQLDBNJZGVudGl0eUFuZFNlY3VyaXR5MQ8wDQYDVQQIDAZTeWRuZXkxDDAK
BgNVBAMMA0NBMTEMMAoGA1UEBwwDV1RHMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAinpVLEBYZdwzwC7OAOy9WT7bBqlFQU0bFWONu2WGXBblXJw1j3+e
I+GerBPO6ZwbNXSo+vr0q6dDLlD5pJsP3Rld6UwB4gquJyJfAGIuA8SQcJrYzkl6
xq3f8gL3q65f8HS/UiP+exO5VocCGjQhhvjPbsSZjF4FIWsOMb9E7pvNQ+MbJ+qs
MEIK3HO2LtgwiQdNZeUxZR4TJN7Vv3iimWG3QSAl1Q7dz4iT/dD4zId4xKK+yXPk
dczgfZQN5LnQ9Nt1/Qi97Xj8Wbu5VOSpeTb8eDP1Jezhq64Xsm7W27U6SYemF2gi
Ef5HLxt4A04XFiPbgnbm7xHArm86TnQ3AQIDAQABoCIwIAYJKoZIhvcNAQkOMRMw
ETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAlzcyPQj5P1iRe
HkTInm8x7lKnfbsuphlyg+HD2j2AGWtKwCR532piCTA0E2zUKqh+E0GO68eH6ou2
YbIzGk/MTUVJ2xZkYUuCNhXEdChfj0xou5Z7QFo+kZ5QUd+fQ08Jp/lZGMZrFGmc
AfwU+hn6ySqY0lFf4mSVY7hUUjPBIkO6YREhvEHuy/ShIjmbjNXCntjkd5zW8gV4
GUE/1cXy10RU86fZj6arGToDL2W7bbtfo6fu+EhNi66sawn5ipIDJ1ab91zn7iOt
yQnZCmN3mvB9tS1PH8KWEYJoYHdXojaC88IuC4xdSb8X4d9vuMtwidQEz7MpL9Li
NBmVuj0D
-----END CERTIFICATE REQUEST-----";

		const string Csr4 = @"-----BEGIN CERTIFICATE REQUEST-----
MIIC0jCCAboCAQAwazELMAkGFDSLFKSDFLDSJFLKDSJFLAoMCGpheXd0Z0NBMRww
GgYDVQQLDBNJZGVudGl0eUFuZFNlY3VyaXR5MQ8wDQYDVQQIDAZTeWRuZXkxDDAK
BgNVBAMMA0NBMTEMMAoGA1UEBwwDV1RHMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8A
MIIBCgKCAQEAinpVLEBYZdwzwC7OAOy9WT7bBqlFQU0bFWONu2WGXBblXJw1j3+e
I+GerBPO6ZwbNXSo+vr0q6dDLlD5pJsP3Rld6UwB4gquJyJfAGIuA8SQcJrYzkl6
xq3f8gL3q65f8HS/UiP+exO5VocCGjQhhvjPbsSZjF4FIWsOMb9E7pvNQ+MbJ+qs
MEIK3HO2LtgwiQdNZeUxZR4TJN7Vv3iimWG3QSAl1Q7dz4iT/dD4zId4xKK+yXPk
dczgfZQN5LnQ9Nt1/Qi97Xj8Wbu5VOSpeTb8eDP1Jezhq64Xsm7W27U6SYemF2gi
Ef5HLxt4A04XFiPbgnbm7xHArm86TnQ3AQIDAQABoCIwIAYJKoZIhvcNAQkOMRMw
ETAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAlzcyPQj5P1iRe
HkTInm8x7lKnfbsuphlyg+HD2j2AGWtKwCR532piCTA0E2zUKqh+E0GO68eH6ou2
YbIzGk/MTUVJ2xZkYUuCNhXEdChfj0xou5Z7QFo+kZ5QUd+fQ08Jp/lZGMZrFGmc
AfwU+hn6ySqY0lFf4mSVY7hUUjPBIkO6YREhvEHuy/ShIjmbjNXCntjkd5zW8gV4
GUE/1cXy10RU86fZj6arGToDL2W7bbtfo6fu+EhNi66sawn5ipIDJ1ab91zn7iOt
yQnZCmN3mvB9tS1PH8KWEYJoYHdXojaC88IuC4xdSb8X4d9vuMtwidQEz7MpL9Li
NBmVuj0E
-----END CERTIFICATE REQUEST-----";
	}
}
