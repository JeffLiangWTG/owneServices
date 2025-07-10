using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.KR.Business.Testing
{
	[TestedType(typeof(GlbCompanyWrapper))]
	sealed class GlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
	{
		public void TestIsValidWrapper()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;

			var wrapper = GetWrapper(company);
			Assert("Should be true as the country code is KR.", wrapper.IsValidWrapper);
		}

		public void TestCertificateForUnipass()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			company.GC_Code = "KRC";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRB";
			branch.GB_IsActive = true;
			var password1 = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password1.GP_PasswordType = "TVB";
			password1.GP_UserID = "1";
			password1.GP_GC = company.PK;
			var password2 = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password2.GP_PasswordType = "KRB";
			password2.GP_UserID = "2";
			password2.GP_GC = company.PK;
			var password3 = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password3.GP_PasswordType = "KRB";
			password3.GP_UserID = "3";
			password3.GP_GC = GlbCompany.CurrentCompany.PK;
			Factory.Save();

			var factory = new BusinessObjectFactory();
			company = factory.Load<GlbCompany>(company.PK);
			password2 = factory.Load<GlbCompanyCredential>(password2.PK);
			var wrapper = GetWrapper(company);
			AssertEquals(password2, wrapper.CertificateForUnipass);
		}

		[TestDate(2022, 08, 30)]
		public void TestIsValidForMessaging()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.KoreaSouth;
			company.GC_Code = "KRC";
			var branch = company.Branches.AddNew();
			branch.GB_Code = "KRB";
			branch.GB_IsActive = true;
			KRCustomsRegistry.Instance.UNIPASSDeclarantID.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, "6N002");
			Factory.Save();

			var factory1 = new BusinessObjectFactory();
			company = factory1.Load<GlbCompany>(company.PK);
			var wrapper1 = GetWrapper(company);
			AssertEquals(false, wrapper1.IsValidForMessaging);

			var password3 = Factory.NewWithValidTestData<GlbCompanyCredential>();
			password3.GP_PasswordType = "KRB";
			password3.GP_UserID = "3";
			password3.GP_GC = company.PK;
			password3.GP_Certificate = new byte[] { 48, 130, 8 };
			password3.GP_IssueDate = new ZDateTime(2022, 1, 1);
			password3.GP_ExpiryDate = new ZDateTime(2022, 12, 31);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			company = factory2.Load<GlbCompany>(company.PK);
			var wrapper2 = GetWrapper(company);
			AssertEquals(true, wrapper2.IsValidForMessaging);
		}
	}
}
