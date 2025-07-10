using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.IdentityApplication.Business;
using Enterprise.Client.EDI.IdentityCertificate.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityApplication.GUI.Testing
{
	[TestedType(typeof(EdiIdentityCustomerApplicationForm))]
	public class EdiIdentityCustomerApplicationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IsCustomerApplication = true;
			Factory.Save();
			return new EdiIdentityCustomerApplicationForm(application);
		}

		public void TestDoesNotHaveEDocsTab()
		{
			using (var form = (EdiIdentityCustomerApplicationForm)GetFormToBash())
			{
				form.Show();
				var eDocsPlugin = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				AssertNull(eDocsPlugin);
			}
		}

		public void TestDoesNotHaveNoteTab()
		{
			using (var form = (EdiIdentityCustomerApplicationForm)GetFormToBash())
			{
				form.Show();
				AssertEquals("Should not have notes tab", 0, form.Controls.Find("NotesTabPage", true).Length);
			}
		}

		public void TestSecurityCheckForCertificateMenuItems()
		{
			EDISecurityCheckpoints.EdiIdentityCertificateEditCertificate.IsAllowed = true;

			using (var form = (EdiIdentityCustomerApplicationForm)GetFormToBash())
			{
				form.Show();
				AssertNotNull(form.GenerateCertificateMenuItem);
				AssertNotNull(form.RevokeCertificateMenuItem);
			}

			EDISecurityCheckpoints.EdiIdentityCertificateEditCertificate.IsAllowed = false;

			using (var form = (EdiIdentityCustomerApplicationForm)GetFormToBash())
			{
				form.Show();
				AssertNull(form.GenerateCertificateMenuItem);
				AssertNull(form.RevokeCertificateMenuItem);
			}
		}

		public void TestGenerateCertificate()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "TestCustomerApp";
			application.IsCustomerApplication = true;

			using (var form = new EdiIdentityCustomerApplicationForm(application))
			using (var file = TempFile.NewWithExtension("csr"))
			{
				form.Show();
				AssertEquals(0, form.Application.Certificates.Count);
				AssertNotNull(form.GenerateCertificateMenuItem);
				AssertNull(form.RenewCertificateMenuItem);

				File.WriteAllText(file.Filename, Csr);
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.GenerateCertificateMenuItem.PerformClick();

				AssertEquals(1, form.Application.Certificates.Count);
				var certificate = form.Application.Certificates[0];
				AssertEquals(Csr, certificate.ICE_CertificateSigningRequest.ToString());
			}
		}

		public void TestRenewCertificate()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "TestCustomerApp";
			application.IsCustomerApplication = true;
			var existingCertificate = application.Certificates.AddNew();
			existingCertificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			existingCertificate.ICE_CertificateSigningRequest = Csr;
			Factory.Save();
			using (var form = new EdiIdentityCustomerApplicationForm(application))
			using (var file = TempFile.NewWithExtension("csr"))
			{
				form.Show();
				AssertEquals(1, form.Application.Certificates.Count);
				AssertNull(form.GenerateCertificateMenuItem);
				AssertNotNull(form.RenewCertificateMenuItem);

				File.WriteAllText(file.Filename, Csr2);
				ZFormModaliser.FileNameToSelectInShowCommonDialog = file.Filename;
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				form.RenewCertificateMenuItem.PerformClick();

				AssertEquals(2, form.Application.Certificates.Count);
				var certificate = form.Application.Certificates.First(cert => string.IsNullOrEmpty(cert.ICE_CARoot));
				AssertEquals(Csr2, certificate.ICE_CertificateSigningRequest.ToString());
			}
		}

		public void TestRollbackApplication()
		{
			var application = Factory.NewWithValidTestData<EdiIdentityApplication>();
			application.IDA_ApplicationName = "TestCustomerApp";
			application.IsCustomerApplication = true;
			var existingCertificate = application.Certificates.AddNew();
			existingCertificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
			existingCertificate.ICE_CertificateSigningRequest = Csr;
			Factory.Save();
			using (var form = new EdiIdentityCustomerApplicationForm(application))
			{
				Factory.Save();
				Assert(!form.Application.IDA_IsRollback);

				form.Show();
				AssertNotNull(form.RollbackApplicationMenuItem);
				form.RollbackApplicationMenuItem.PerformClick();

				Assert(form.Application.IDA_IsRollback);
			}
		}

		public void TestRevokeCertificate()
		{
			using (var form = (EdiIdentityCustomerApplicationForm)GetFormToBash())
			{
				var certificate = form.Application.Certificates.AddNew();
				certificate.ICE_ProcessingStatus = EdiIdentityCertificateProcessingStatus.Codes.COM;
				certificate.ICE_CertificateSigningRequest = Csr;
				Factory.Save();

				form.Show();
				AssertEquals(1, form.Application.Certificates.Count);
				form.CertificatesGridForTest.SelectAllElements();
				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.OK);
				form.RevokeCertificateMenuItem.PerformClick();
				AssertStartsWith("Should have confirm message.", "You are about to revoke 1 certificates.\r\nAre you sure you want to proceed?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(EdiIdentityCertificateProcessingStatus.Codes.CAN, form.Application.Certificates[0].ICE_ProcessingStatus);
			}
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

		protected override void SetUp()
		{
			EDIDataRegistry.Instance.AWSPrivateCAListManager.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, CreateAWSPrivateCACollection());
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
	}
}
