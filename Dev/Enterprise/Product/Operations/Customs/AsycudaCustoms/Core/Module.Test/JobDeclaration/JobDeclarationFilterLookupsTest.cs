using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.AsycudaCustoms.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AsycudaCustoms.Module.Testing
{
	class JobDeclarationFilterLookupsTest : TestCaseWithFactory
	{
		public void TestGuaranteeHeaders()
		{
			var validGuarantee = CreateCusGuaranteeHeader(GlbCompany.CurrentCompany.GC_RN_NKCountryCode, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			var invalidGuarantee = CreateCusGuaranteeHeader(Core.Constants.CountryCodes.China, ZDate.Today.AddDays(-1), ZDate.Today.AddDays(1));
			AssertEquals("GuaranteeHeaders of correct type", typeof(CusGuaranteeHeaderCollection), AsycudaFilterLookups.GuaranteeHeaders.GetType());
			Assert(AsycudaFilterLookups.GuaranteeHeaders.Contains(validGuarantee));
			Assert(!AsycudaFilterLookups.GuaranteeHeaders.Contains(invalidGuarantee));
		}

		CusGuaranteeHeader CreateCusGuaranteeHeader(string countryCode, ZDate start, ZDate? end)
		{
			var result = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			result.CPH_RN_NKCountryCode = countryCode;
			result.CPH_ApplicationCode = CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			result.CPH_StartDate = start;
			if (end.HasValue)
			{
				result.CPH_EndDate = end.Value;
			}

			return result;
		}

		public void TestGuaranteeStatusList()
		{
			AssertEquals("GuaranteeStatusList of correct type", typeof(GuaranteeStatusList), AsycudaFilterLookups.GuaranteeStatusList.GetType());
		}

		public void TestGuaranteeActivityList()
		{
			AssertEquals("GuaranteeActivityList of correct type", typeof(GuaranteeActivityCodeList), AsycudaFilterLookups.GuaranteeActivityList.GetType());
		}

		JobDeclarationFilterLookups AsycudaFilterLookups => new JobDeclarationFilterLookups(new JobDeclarationFilterBusinessObject());
	}
}
