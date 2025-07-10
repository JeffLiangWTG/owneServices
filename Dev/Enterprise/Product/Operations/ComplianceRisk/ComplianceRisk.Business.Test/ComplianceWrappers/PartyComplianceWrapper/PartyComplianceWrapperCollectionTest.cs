using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(PartyComplianceWrapperCollection))]
	public class PartyComplianceWrapperCollectionTest : NonPersistentBusinessObjectCollectionTestCase<PartyComplianceWrapperCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, new PartyComplianceWrapperCollection().AllowNew);
		}

		protected override PartyComplianceWrapperCollection GetCollectionToTest() => new PartyComplianceWrapperCollection();

		protected override BusinessObject GetNewElementToAddToTheCollection() => new PartyComplianceWrapper(new ScreeningParty(Factory.NewWithValidTestData<OrgHeader>(), "", OrgHeader.New(Factory)));
	}
}
