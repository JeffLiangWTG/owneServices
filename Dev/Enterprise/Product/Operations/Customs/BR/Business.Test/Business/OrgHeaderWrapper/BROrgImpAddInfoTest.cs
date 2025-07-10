using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.BR.Business.Testing
{
	[TestedType(typeof(BROrgImpAddInfo))]
	public class BROrgImpAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		public void TestBrokerCertificate()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_WorkPhone = "0412345678";
			staff.GS_FullName = "USERDOWNLOADCATALOG";
			staff.GS_Code = "TST";

			var password = BRGlbStaffWrapper.Get(staff).CCTPassword;
			password.GP_Certificate = X509Certificate2TestHelper.ValidCertificate;
			password.CurrentDecryptedCertificatePassphrase = X509Certificate2TestHelper.ValidPassword;
			password.GP_ExpiryDate = ZDateTime.Today.AddDays(10);
			password.GP_PasswordStatus = BRPasswordStatusList.Codes.Valid;

			var addInfo = new BROrgImpAddInfo(Factory);
			addInfo.ZO_BrokerCode = "TST";

			AssertEquals(staff, addInfo.Broker);
			AssertEquals(password, addInfo.BrokerCertificate);
		}
	}
}
