using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceCommodityRiskLogCollection))]
	public class ComplianceCommodityRiskLogCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ComplianceCommodityRiskLogCollection>
	{
		protected override ComplianceCommodityRiskLogCollection GetCollectionToTest()
		{
			return new ComplianceCommodityRiskLogCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ComplianceCommodityRiskLog(new Commodity());
		}
	}
}
