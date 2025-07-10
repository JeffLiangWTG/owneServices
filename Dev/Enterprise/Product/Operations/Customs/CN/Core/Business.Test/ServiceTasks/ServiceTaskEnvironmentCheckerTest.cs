using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.CN.Business.Testing
{
	[TestedType(typeof(ServiceTaskEnvironmentChecker))]
	public class ServiceTaskEnvironmentCheckerTest : TestCaseWithFactory
	{
		public void TestCheckCNSWClientSetting()
		{
			TestCheckCNSWClientSetting(Factory, ServiceTaskEnvironmentChecker.CheckCNSWClientSetting);
		}

		public static void TestCheckCNSWClientSetting(BusinessObjectFactory factory, Func<string> checkCNSWClientSettingFunc)
		{
			const string registryHasNotBeenConfigured = "The registry setting 'Customs -> Country or Region Specific -> China -> Single Window Client Application Settings' has not been configured.";

			ServiceTaskEnvironmentChecker.Reset();
			AssertEquals(registryHasNotBeenConfigured, checkCNSWClientSettingFunc());

			var company = factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			factory.Save();

			using (CNSWClientSettingCheckerTest.TemporarilySetCNSWClientSetting(factory, company.PK.ToGuid(), Guid.Empty))
			{
				ServiceTaskEnvironmentChecker.Reset();
				AssertEquals(registryHasNotBeenConfigured, checkCNSWClientSettingFunc());

				var branch = factory.NewWithValidTestData<GlbBranch>();
				branch.GB_GC = company.PK;
				factory.Save();

				ServiceTaskEnvironmentChecker.Reset();
				AssertEquals("", checkCNSWClientSettingFunc());
			}
		}
	}
}
