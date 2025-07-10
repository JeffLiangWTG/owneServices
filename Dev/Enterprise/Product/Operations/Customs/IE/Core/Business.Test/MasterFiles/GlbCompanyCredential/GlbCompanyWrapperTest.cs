using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.IE.Business.Testing
{
	[TestedType(typeof(GlbCompanyWrapper))]
	public class GlbCompanyWrapperTest : Enterprise.MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
	{
		public void TestIsValidWrapper()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Ireland;

			var wrapper = GetWrapper(company);
			Assert("Should be true as the country code is IE.", wrapper.IsValidWrapper);
		}

		public void TestGetMessageSenderEORI()
		{
			var declaration = Factory.New<JobDeclaration>();
			var companyCredential = InterchangeProcessorTestHelper.CreateValidCredential(declaration.Company);
			var entry = declaration.CustomsEntryHeaders.AddNew();

			AssertEquals(companyCredential.GP_MailBoxID, GlbCompanyWrapper.GetMessageSenderEORI(entry));
		}
	}
}
