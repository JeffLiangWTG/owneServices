using System.Data;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.MX.Manifest.Business.Testing
{
	[TestedType(typeof(GlbCompanyCredential))]
	public sealed class GlbCompanyCredentialTest : GlbExternalPasswordTest<GlbCompanyCredential>
	{
		protected override GlbCompanyCredential CreateNewGlbExternalPassword(BusinessObjectFactory factory)
		{
			return factory.New<GlbCompanyCredential>();
		}

		public void TestPasswordStatus()
		{
			GlbExternalPassword.GP_PasswordStatus = "VAL";
			AssertEquals("Valid", GlbExternalPassword.PasswordStatus);

			GlbExternalPassword.GP_PasswordStatus = "INV";
			AssertEquals("Invalid", GlbExternalPassword.PasswordStatus);
		}

		public void TestReadOnly()
		{
			AssertEquals("GP_PasswordStatus.ReadOnly", true, GlbExternalPassword.GP_PasswordStatusInfo.ReadOnly);
		}

		public void TestGP_UserIDMaxLength()
		{
			var credential = Factory.New<GlbCompanyCredential>();
			AssertEquals(13, credential.GP_UserIDInfo.MaxLength);
		}

		public void TestCurrentPasswordMaxLength() 
		{
			var credential = Factory.New<GlbCompanyCredential>();
			AssertEquals(128, credential.GP_CurrentPasswordInfo.MaxLength);
		}

		public void TestGetMessageAttrDictionary()
		{
			var credential = Business.GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(GlbCompany.CurrentCompany).GlbExternalPassword;
			credential.GP_UserID = "USERNAME";
			credential.GP_CurrentPassword = "PASSWORD";

			GlbCompany.CurrentCompany.Factory.Save();

			var dictionary = credential.GetMessageAttrDictionary();

			string value;
			dictionary.TryGetValue("httpclient.user", out value);
			AssertEquals("user", credential.GP_UserID, value);

			dictionary.TryGetValue("httpclient.password", out value);
			AssertEquals("password", credential.GP_CurrentPassword, value);
		}

		public void TestShouldSendCredential()
		{
			var credential = Factory.New<GlbCompanyCredentialForTest>();
			AssertEquals("ShouldSendCredential", false, credential.ShouldSendCredentialExposed());
		}
	}

	class GlbCompanyCredentialForTest : GlbCompanyCredential
	{
		public GlbCompanyCredentialForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public bool ShouldSendCredentialExposed() => ShouldSendCredential();
	}
}
