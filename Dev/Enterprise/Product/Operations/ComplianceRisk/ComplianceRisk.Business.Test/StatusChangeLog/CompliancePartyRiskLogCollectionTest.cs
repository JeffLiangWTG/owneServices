using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(CompliancePartyRiskLogCollection))]
	public class CompliancePartyRiskLogCollectionTest : NonPersistentBusinessObjectCollectionTestCase<CompliancePartyRiskLogCollection>
	{
		protected override CompliancePartyRiskLogCollection GetCollectionToTest()
		{
			return new CompliancePartyRiskLogCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new CompliancePartyRiskLog(new Party(), new ScreeningStatusesList(), null);
		}
	}
}
