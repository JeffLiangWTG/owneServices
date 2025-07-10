using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Business.Testing
{
	[TestedType(typeof(GlbILStaffExternalPassword))]
	public class GlbILStaffExternalPasswordTest : GlbExternalPasswordTest<GlbILStaffExternalPassword>
	{
		public void TestSetDefaultValues()
		{
			var password = Factory.New<GlbILStaffExternalPassword>();
			AssertEquals("Password Type should be ILS", "ILS", password.GP_PasswordType);

			AssertNotEquals(ZGuid.Empty, password.GP_GC);
		}

		public void TestCaptions()
		{
			AssertEquals("GP_CertificateAuthority caption", "Certificate Authority", DataBoundResourceStrings.GetDataForProperty(GlbExternalPassword.GP_CertificateAuthorityInfo).Caption);
			AssertEquals("GP_UserID caption", "Certificate ID", DataBoundResourceStrings.GetDataForProperty(GlbExternalPassword.GP_UserIDInfo).Caption);
			AssertEquals("CurrentDecryptedPassword caption", "Pin Code", DataBoundResourceStrings.GetDataForProperty(GlbExternalPassword.CurrentDecryptedPasswordInfo).Caption);
		}

		public void TestLookups()
		{
			AssertType<GlbILStaffExternalPasswordLookups>(GlbExternalPassword.Lookups);
		}
	}
}
