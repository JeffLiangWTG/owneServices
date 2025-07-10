using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceRiskPartyWrapperCollection))]
	public class ComplianceRiskPartyWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComplianceRiskPartyWrapperCollection>
	{
		public void TestAllowNew()
		{
			var result = new ComplianceRiskPartyWrapperCollection();
			AssertEquals(false, result.AllowNew);
		}

		protected override ComplianceRiskPartyWrapperCollection GetCollectionToTest()
		{
			return new ComplianceRiskPartyWrapperCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var org = Factory.New<OrgHeader>();
			return new ComplianceRiskPartyWrapper(new ScreeningParty(org, "", org));
		}
	}
}
