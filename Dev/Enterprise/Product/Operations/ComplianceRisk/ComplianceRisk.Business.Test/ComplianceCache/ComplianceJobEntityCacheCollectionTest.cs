using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.ComplianceRisk.Business.Test
{
	[TestedType(typeof(ComplianceJobEntityCacheCollection))]
	public class ComplianceJobEntityCacheCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var (complianceRisk, _) = Factory.CreateNewComplianceRiskWithShipment();
			return new ComplianceJobEntityCacheCollection(complianceRisk);
		}

		public override void TestAddNew()
		{
			base.TestAddNew();

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var (complianceRisk, _) = Factory.CreateNewComplianceRiskWithShipment();
			var cache = Factory.CreateNewComplianceJobEntityCache(complianceRisk, orgHeader);
			var collection = GetCollectionToTest();
			collection.Add(cache);

			AssertEquals("Collection count 1", 1, collection.Count);
			AssertEquals("ComplianceJobEntityCache 1", 1, collection.OfType<ComplianceJobEntityCache>().Count());
		}
	}
}
