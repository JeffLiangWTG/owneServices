using System;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Customs.ES.Manifest.H7.Business.Testing
{
	public class G3MessageWrapperTest : DataProviderTestCase<G3MessageWrapper>
	{
		public void TestConstructor()
		{
#if NET
			AssertExceptionThrown("Throws Exception if certificate is null", typeof(ArgumentNullException),
				"Value cannot be null. (Parameter 'certificate')", () => new G3MessageWrapper(null, Factory));
			AssertExceptionThrown("Throws Exception if Factory is null", typeof(ArgumentNullException),
				"Value cannot be null. (Parameter 'factory')", () => new G3MessageWrapper(certificate, null));
#else
			AssertExceptionThrown("Throws Exception if certificate is null", typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: certificate", () => new G3MessageWrapper(null, Factory));
			AssertExceptionThrown("Throws Exception if Factory is null", typeof(ArgumentNullException),
				"Value cannot be null.\r\nParameter name: factory", () => new G3MessageWrapper(certificate, null));
#endif
		}

		public void TestSender()
		{
			AssertEquals(certPassword.GP_MailBoxID, wrapper.Sender);
		}

		protected override void SetUp()
		{
			base.SetUp();

			var broker = Factory.NewWithValidTestData<GlbStaff>();
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_GS_NKCustomsAgent = broker.GS_Code;

			var glbWRapper = ES.Business.GlbStaffWrapper.Get(broker);
			certPassword = glbWRapper.ESBPasswordCollection.AddNew();
			certPassword.GP_Name = "TESTCERT1";
			certPassword.GP_MailBoxID = "Test1";
			certPassword.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			certPassword.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			var auth = certPassword.Authorisations.AddNew();
			auth.GEA_GS_AuthorisedStaff = broker.PK;

			certificate = new CertificateObject(header.CustomsAgent, certPassword.GP_Name, ZString.Empty);
			wrapper = new G3MessageWrapper(certificate, Factory);
		}

		protected override G3MessageWrapper GetProvider()
		{
			return wrapper;
		}

		G3MessageWrapper wrapper;
		ES.Business.GlbExternalPassword certPassword;
		ICertificateProvider certificate;
	}
}
