using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.Licencing.Business;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class UsageTest : TestCaseWithFactory
	{
		public void TestClientCompanyPk()
		{
			var clientUsage = Factory.New<ClientChargeableUsage>();
			var clientPremium = Factory.New<ClientPremiumService>();
			var licenceDatabase = Factory.New<LicenceDatabase>();
			clientPremium.CPS_LD = licenceDatabase.PK;

			var clientCompany1 = Factory.New<ClientCompany>();
			var clientCompany2 = Factory.New<ClientCompany>();

			clientUsage.U1_LCC = clientCompany1.PK;
			clientPremium.CPS_LCC = clientCompany2.PK;

			var usage = new Usage(clientUsage, clientPremium, ZDateTime.Now, clientCompany2);
			AssertEquals(clientCompany2.PK, usage.ClientCompanyPk);
		}
	}

	public class UsageSetTest : TestCaseWithFactory
	{
		public void TestGoldenTaxDesc()
		{
			var usage = Factory.NewWithValidTestData<ClientChargeableUsage>();
			usage.U1_Code = BillingConstants.Category.GoldenTax;
			usage.U1_SubCode = "GTS";
			usage.U1_Reference1 = "#001";
			var usageSet = new UsageSet(new[] { usage }, System.Array.Empty<ClientPremiumService>(), System.Array.Empty<ClientPremiumService>(), ZDateTime.Today, null, null);
			AssertEquals("Reg. Code: #001", usageSet.Usages.Single().AdditionalDescription);
		}
	}
}
