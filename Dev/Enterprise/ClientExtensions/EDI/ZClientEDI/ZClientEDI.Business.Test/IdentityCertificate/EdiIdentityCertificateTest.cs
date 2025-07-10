using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IdentityCertificate.Business.Testing
{
	[TestedType(typeof(EdiIdentityCertificate))]
	public class EdiIdentityCertificateTest : EnterpriseBusinessObjectTestCase
	{
		#region TestCARootHasDefault

		public void TestCARootHasDefault()
		{
			var certificate = Factory.New<EdiIdentityCertificate>();
			AssertEquals("", certificate.ICE_CARoot);
		}

		#endregion

		#region TestSystemFiledsHaveValue

		public void TestSystemFieldsHaveValue()
		{
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			Factory.Save();
			var ca = certificate as IAuditDetails;
			AssertNotNullOrEmpty(ca.SystemCreateTimeUtc.ToString());
			AssertNotNullOrEmpty(ca.SystemCreateUser);
			AssertNotNullOrEmpty(ca.SystemLastEditTimeUtc.ToString());
			AssertNotNullOrEmpty(ca.SystemLastEditUser);
		}

		public void TestDuplicateCertificateSigningRequest()
		{
			var cert1 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			var cert2 = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			cert1.ICE_CertificateSigningRequest = Guid.NewGuid().ToString();
			cert2.ICE_CertificateSigningRequest = Guid.NewGuid().ToString();
			AssertNoExceptionThrown(() => { Factory.Save(); });

			cert2.ICE_CertificateSigningRequest = cert1.ICE_CertificateSigningRequest;
			AssertExceptionThrown<ZSaveException>(() => { Factory.Save(); });

			cert2.ICE_IsActive = false;
			AssertNoExceptionThrown(() => { Factory.Save(); });

			cert2.ICE_IsActive = true;
			cert2.ICE_CertificateData = ZBlob.FromAscii("01234567");
			AssertNoExceptionThrown(() => { Factory.Save(); });
		}

		#endregion

		public void TestCARootListsCodeAndDescription()
		{
			var caList = new AWSPrivateCACollection();
			caList.Add(new AWSPrivateCA() { IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust, Arn = "arn:aws:acm-pca:ap-southeast-2:079973481859:123123123", IsEnabled = true, AccessKey = "Test01", SecretKey = "Test02" });
			caList.Add(new AWSPrivateCA() { IssuingCA = CARootCodeDescriptionList.Codes.Adaptor, Arn = "arn:aws:acm-pca:ap-southeast-2:079973481859:456456456", IsEnabled = false, AccessKey = "Test01", SecretKey = "Test02" });
			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, caList))
			{
				var certificate = Factory.New<EdiIdentityCertificate>();
				var caRootList = certificate.CARootLists;
				AssertEquals(1, caRootList.Count);
				var codes = caRootList.OfType<CodeDescriptionPair>().Select(pair => pair.Code);
				AssertCollectionContains(CARootCodeDescriptionList.Codes.SystemToSystemTrust, codes);

				var descs = caRootList.OfType<CodeDescriptionPair>().Select(pair => pair.Description);
				AssertCollectionContains("arn:aws:acm-pca:ap-southeast-2:079973481859:123123123", descs);
				AssertCollectionNotContains("arn:aws:acm-pca:ap-southeast-2:079973481859:456456456", descs);
			}
		}

		public void TestArn()
		{
			var caList = new AWSPrivateCACollection();
			caList.Add(new AWSPrivateCA() { IssuingCA = CARootCodeDescriptionList.Codes.SystemToSystemTrust, Arn = "arn:aws:acm-pca:ap-southeast-2:079973481859:123123123", IsEnabled = true, AccessKey = "Test01", SecretKey = "Test02" });
			caList.Add(new AWSPrivateCA() { IssuingCA = CARootCodeDescriptionList.Codes.Adaptor, Arn = "arn:aws:acm-pca:ap-southeast-2:079973481859:456456456", IsEnabled = false, AccessKey = "Test01", SecretKey = "Test02" });
			using (EDIDataRegistry.Instance.AWSPrivateCAListManager.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, caList))
			{
				var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();

				AssertEquals("", certificate.Arn);

				certificate.ICE_CARoot = CARootCodeDescriptionList.Codes.SystemToSystemTrust;
				AssertEquals("arn:aws:acm-pca:ap-southeast-2:079973481859:123123123", certificate.Arn);

				certificate.ICE_CARoot = "Arn4";
				AssertEquals("", certificate.Arn);
			}
		}

		public void TestSequenceNumber()
		{
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.OnSaving();
			AssertNotEquals(ZLong.Zero, certificate.ICE_SequenceNumber);
		}

		public void TestSequenceNumberChanged()
		{
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.OnSaving();
			var sequenceNumber = certificate.ICE_SequenceNumber;
			certificate.OnSaving();
			AssertNotEquals(sequenceNumber, certificate.ICE_SequenceNumber);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.NewWithValidTestData<EdiIdentityCertificate>();
		}

		public void TestCaReadOnly()
		{
			var certificate = Factory.NewWithValidTestData<EdiIdentityCertificate>();
			certificate.ICE_CertificateThumbprint =
				new X509Certificate2(Encoding.UTF8.GetBytes(Certificate)).Thumbprint;
			Factory.Save();
			certificate.ReloadSafe();
			Assert(certificate.ICE_CARoot_ReadOnly);
		}

		const string Certificate = @"-----BEGIN CERTIFICATE-----
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
	}
}
