using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Manifest.H7.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Manifest.H7.GUI.Testing
{
	[TestedType(typeof(ESSendH7ToCustomsApplicator))]
	sealed class ESSendH7ToCustomsApplicatorTest : OperationalActionMethodApplicatorTest
	{
		public void TestSubmitMessagesSuccessfully()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Spain))
			{
				var applicater = new ESSendH7ToCustomsApplicator();
				var logger = new DummyOperationalActionSectionLog();
				var broker = Factory.NewWithValidTestData<GlbStaff>();
				var wrapper = ES.Business.GlbStaffWrapper.Get(broker);

				var cert = wrapper.ESBPasswordCollection.AddNew();
				cert.GP_Name = "TESTCERT1";
				cert.GP_MailBoxID = "Test1";
				cert.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
				cert.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
				var auth = cert.Authorisations.AddNew();
				auth.GEA_GS_AuthorisedStaff = broker.PK;

				var header1 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill1 = header1.Bills.AddNew();
				header1.AMA_JobReference = "ES1234";
				header1.AMA_GS_NKCustomsAgent = broker.GS_Code;
				header1.AMA_CustomsProfile = "TESTCERT1";

				var header2 = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				var bill2 = header2.Bills.AddNew();
				header2.AMA_JobReference = "ES2345";
				header2.AMA_CustomsProfile = "TESTCERT1";
				header2.AMA_GS_NKCustomsAgent = broker.GS_Code;
				applicater.Apply(logger, new AsycudaManifestHeader[] { header1, header2 });

				CombineAssertions(() =>
				{
					Assert(logger.messages.Contains("SUCCESS: [HL ES1234] : Submit Succeeded."));
					Assert(logger.messages.Contains("SUCCESS: [HL ES2345] : Submit Succeeded."));
				});
			}
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new ESSendH7ToCustomsApplicator();
		}
	}
}
