using CargoWise.EntityFramework;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing
{
	public class StaffWithCertificateTestHelper
	{
		public StaffWithCertificateTestHelper(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		public CertificateProviderTestClass Certificate
		{
			get
			{
				if (certificate == null)
				{
					certificate = new CertificateProviderTestClass
					{
						BrokerCode = "AZ",
						CertificateName = "CertName",
						CertificateThumbPrint = "CertThumbPrint",
						CertificateBytes = BuilderHelperTest.GetCertificateBytes(),
						DecryptedCertificatePassphrase = BuilderHelperTest.CertificatePassword,
						CertificateID = "123456Z"
					};
				}
				return certificate;
			}
		}
		CertificateProviderTestClass certificate;

		public GlbStaff Staff
		{
			get
			{
				if (staff == null)
				{
					var certificate = Certificate;
					staff = factory.New<GlbStaff>();
					staff.GS_Code = certificate.BrokerCode;
					staff.GS_LoginName = "ahtest";
					staff.StaffPlainTextPassword = "1234";
					var wrapper = GlbStaffWrapper.Get(staff);
					var cert = wrapper.ESBPasswordCollection.AddNew();
					cert.GP_Name = certificate.CertificateName;
					cert.GP_Certificate = certificate.CertificateBytes;
					cert.CurrentDecryptedCertificatePassphrase = certificate.DecryptedCertificatePassphrase;
					cert.GP_UserID = certificate.CertificateThumbPrint;
					certificate.CertificatePK = cert.PK;
				}
				return staff;
			}
		}
		GlbStaff staff;
	}
}
