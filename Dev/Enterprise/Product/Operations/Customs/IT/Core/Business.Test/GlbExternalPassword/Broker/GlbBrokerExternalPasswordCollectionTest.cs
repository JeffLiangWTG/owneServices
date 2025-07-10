using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Testing;

[TestedType(typeof(GlbBrokerExternalPasswordCollection))]
sealed class GlbBrokerExternalPasswordCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestCertificateInfoDefaultingOnItemAdded()
	{
		var validCertificateBinary = X509Certificate2TestHelper.ValidCertificate;
		var validCertificatePassword = X509Certificate2TestHelper.ValidPassword;
		var validCertificate = new X509Certificate2(validCertificateBinary, validCertificatePassword);
		var certificateExpiryDate = validCertificate.NotAfter;

		var passwordCollection = GetNewGlbExternalPasswordCollection();
		Assert("PRE-CONDITION: empty collection", !passwordCollection.Any());

		var externalPassword1 = passwordCollection.AddNew();
		CombineAssertions("PRE-CONDITION: certificate info are empty", () =>
		{
			AssertEquals(externalPassword1.GP_Certificate, ZBlob.Empty);
			AssertEquals(externalPassword1.CurrentDecryptedCertificatePassphrase, ZString.Empty);
			AssertEquals(externalPassword1.GP_ExpiryDate, ZDateTime.Empty);
		});

		externalPassword1.GP_Certificate = validCertificateBinary;
		externalPassword1.CurrentDecryptedCertificatePassphrase = validCertificatePassword;
		CombineAssertions("PRE-CONDITION 1: externalPassword1 certificate info are set", () =>
		{
			AssertEquals(externalPassword1.GP_Certificate, validCertificateBinary);
			AssertEquals(externalPassword1.CurrentDecryptedCertificatePassphrase, validCertificatePassword);
			AssertEquals(externalPassword1.GP_ExpiryDate, certificateExpiryDate);
		});

		var externalPassword2 = passwordCollection.AddNew();
		CombineAssertions("POST-CONDITION: externalPassword2 certificate info have been defaulted from externalPassword1", () =>
		{
			AssertEquals(externalPassword2.GP_Certificate, validCertificateBinary);
			AssertEquals(externalPassword2.CurrentDecryptedCertificatePassphrase, validCertificatePassword);
			AssertEquals(externalPassword2.GP_ExpiryDate, certificateExpiryDate);
		});
	}

	public void TestCopyCertificateInfoExceptionThrown()
	{
		var dummyBizo = Factory.New<DummyBusinessObject>();
		var collectionForTest = GetNewGlbExternalPasswordCollection();
		collectionForTest.AddNew();
		var externalPassword = collectionForTest.AddNew();

		AssertExceptionThrown<InvalidOperationException>("Attemp to change the value of a property that does not belong to GlbExternalPassword_IT", () => collectionForTest.CopyCertificateInfo(externalPassword, dummyBizo.Z0_DescriptionInfo));
		AssertNoExceptionThrown(() => collectionForTest.CopyCertificateInfo(externalPassword, externalPassword.GP_CertificateInfo));
	}

	GlbBrokerExternalPasswordCollection GetNewGlbExternalPasswordCollection()
	{
		var staff = Factory.New<GlbStaff>();
		return new GlbBrokerExternalPasswordCollection(staff);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return GetNewGlbExternalPasswordCollection();
	}
}
