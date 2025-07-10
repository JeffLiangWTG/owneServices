using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.CommissionManagement.Business.Test
{
	[TestedType(typeof(EdiCommissionHeaderAdditionalInfo))]
	public class EdiCommissionHeaderAdditionalInfoTest : EnterpriseBusinessObjectTestCase
	{
		public void TestEnterpriseDatabaseClientCompanyString()
		{
			var licEnterprise = Factory.New<LicenceEnterprise>();
			licEnterprise.LE_EnterpriseCode = "PRD";
			var licDatabase = licEnterprise.Databases.AddNew();
			licDatabase.LD_LE = licEnterprise.PK;
			licDatabase.LD_ServerCode = "AAA";
			var clientCompany = Factory.New<ClientCompany>();
			clientCompany.LCC_LD = licDatabase.PK;
			clientCompany.LCC_Code = "SYD";

			var additionalInfo = Factory.New<EdiCommissionHeaderAdditionalInfo>();
			additionalInfo.ECH_LD = licDatabase.PK;
			AssertEquals("PRD-AAA", additionalInfo.EnterpriseDatabaseClientCompanyString);

			additionalInfo.ECH_LCC = clientCompany.PK;
			AssertEquals("PRD-AAA-SYD", additionalInfo.EnterpriseDatabaseClientCompanyString);

			clientCompany.LCC_RN_NKCountryCode = "AU";
			AssertEquals("PRD-AAA-SYD (AU)", additionalInfo.EnterpriseDatabaseClientCompanyString);
		}
	}
}
