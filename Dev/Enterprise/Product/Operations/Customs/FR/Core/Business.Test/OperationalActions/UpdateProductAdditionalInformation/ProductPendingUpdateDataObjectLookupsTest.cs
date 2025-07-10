using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.OperationalActions.Testing
{
	[TestedType(typeof(ProductPendingUpdateDataObjectLookups))]
	public class ProductPendingUpdateDataObjectLookupsTest : TestCaseWithFactory
	{
		public void TestProducts()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			AssertType(typeof(OrgSupplierPartCollection), dataObject.Lookups.Products);
		}

		public void TestCustomsTypes()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			AssertEquals("EXP, IMP", dataObject.Lookups.CustomsTypes.CodesAsString);
		}

		public void TestRelatedOrgs()
		{
			var dataObject = new ProductPendingUpdateDataObject(Factory);
			var filterBusinessObjectDefaults = dataObject.Lookups.RelatedOrgs.FilterBusinessObjectDefaults;
			AssertEquals(true, filterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property2"));
			AssertEquals(true, filterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:Property3"));
			AssertEquals(true, filterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:OrJoinCondition"));
			AssertEquals(true, filterBusinessObjectDefaults.ContainsDefaultFor("Organisation Types:AndJoinCondition"));
		}
	}
}
