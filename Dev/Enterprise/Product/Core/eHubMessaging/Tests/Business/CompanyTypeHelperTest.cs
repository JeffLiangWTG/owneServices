using CargoWise.Application;
using Enterprise.eHubMessaging.Business;
using Enterprise.Integration.Licensing;
using NUnit.Framework;

namespace Enterprise.eHubMessaging.Tests.Business
{
	class CompanyTypeHelperTest : TestCase
	{
		public void TestIsClientEnterpriseCode()
		{
			var registrationKey = ObjectFactory.Get<IProductRegistration>().KeyForTest;
			registrationKey.EnterpriseCodeForTest = "";
			Assert(!CompanyTypeHelper.IsClientEnterpriseCode());

			registrationKey.EnterpriseCodeForTest = "HYE";
			Assert(!CompanyTypeHelper.IsClientEnterpriseCode());

			registrationKey.EnterpriseCodeForTest = "EDI";
			Assert(!CompanyTypeHelper.IsClientEnterpriseCode());

			registrationKey.EnterpriseCodeForTest = "EHW";
			Assert(!CompanyTypeHelper.IsClientEnterpriseCode());

			registrationKey.EnterpriseCodeForTest = "TST";
			Assert(CompanyTypeHelper.IsClientEnterpriseCode());
		}
	}
}

