using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(PartyComplianceWrapper))]
	public class PartyComplianceWrapperTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var screeningParty = new ScreeningParty(header, "Organization Test Only", header);
			return new PartyComplianceWrapper(screeningParty);
		}
	}
}
