using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.Netting.Testing
{
	[TestedType(typeof(NettingOrganisation))]
	class NettingOrganisationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestNettingTypes()
		{
			var nettingOrg = Factory.New<NettingOrganisation>();

			nettingOrg.NSO_NettingType = "HOM";
			Assert(nettingOrg.IsHomeNettingType);

			nettingOrg.NSO_NettingType = "FUL";
			Assert(nettingOrg.IsFullNettingType);

			nettingOrg.NSO_NettingType = "CUR";
			Assert(nettingOrg.IsCurrencyNettingType);

			nettingOrg.NSO_NettingType = "GRS";
			Assert(nettingOrg.IsGrossNettingType);
		}
	}
}
