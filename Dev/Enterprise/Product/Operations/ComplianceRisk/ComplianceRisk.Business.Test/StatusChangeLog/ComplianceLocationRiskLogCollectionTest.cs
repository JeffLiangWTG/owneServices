using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceLocationRiskLogCollection))]
	public class ComplianceLocationRiskLogCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComplianceLocationRiskLogCollection>
	{
		protected override ComplianceLocationRiskLogCollection GetCollectionToTest()
		{
			return new ComplianceLocationRiskLogCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceLocationRiskLog(new Country(), null);
		}
	}
}
