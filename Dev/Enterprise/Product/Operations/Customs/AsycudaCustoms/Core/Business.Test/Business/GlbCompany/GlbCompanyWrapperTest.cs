using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AsycudaCustoms.Business.Testing
{
	[TestedType(typeof(GlbCompanyWrapper))]
	class GlbCompanyWrapperTest : MasterFiles.Business.Testing.GlbCompanyWrapperTest<GlbCompanyWrapper>
	{
		public void TestConfiguration()
		{
			AssertEquals("Pre-Condition:", 0, Factory.GetDatabaseCount(typeof(ZZRefCusConfiguration)));
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;
			var wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(company);
			var configuration = wrapper.Configuration;
			AssertNotNull(configuration);
			AssertType<ZZRefCusConfiguration>(configuration);
			Factory.Save();
			AssertEquals("Post-Condition:", 1, Factory.GetDatabaseCount(typeof(ZZRefCusConfiguration)));
			var newFactory = new BusinessObjectFactory();
			var companyInNewFactory = newFactory.Load<GlbCompany>(company.PK);
			wrapper = GlbCompanyWrapper.GetWrapper<GlbCompanyWrapper>(companyInNewFactory);
			AssertEquals("Pre-Condition:", 1, Factory.GetDatabaseCount(typeof(ZZRefCusConfiguration)));
			configuration = wrapper.Configuration;
			AssertNotNull(configuration);
			AssertType<ZZRefCusConfiguration>(configuration);
			AssertEquals("Post-Condition:", 1, Factory.GetDatabaseCount(typeof(ZZRefCusConfiguration)));
		}

		public void TestIsValidWrapper()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda, Core.Constants.CountryCodes.Namibia, Core.Constants.CountryCodes.Namibia, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			Factory.Save();
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Namibia;
			company.Branches.AddNew().FillWithValidTestData();
			var wrapper1 = GlbCompanyWrapper.GetWrapper<MasterFiles.Business.GlbCompanyWrapper>(company);
			AssertType<GlbCompanyWrapper>(wrapper1);
			var currentCompany = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			MasterFiles.Business.GlbCompanyWrapper wrapper2 = null;
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Namibia))
			{
				wrapper2 = GlbCompanyWrapper.GetWrapper<MasterFiles.Business.GlbCompanyWrapper>(currentCompany);
				AssertType<GlbCompanyWrapper>(wrapper2);
				AssertEquals("Not logged in company.", false, wrapper1.IsValidWrapper);
				AssertEquals("Logged in company is AsycudaCustoms country.", true, wrapper2.IsValidWrapper);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				AssertEquals("Not logged in company.", false, wrapper1.IsValidWrapper);
				AssertEquals("Country code is not an AsycudaCustoms country.", false, wrapper2.IsValidWrapper);
			}
		}
	}
}
