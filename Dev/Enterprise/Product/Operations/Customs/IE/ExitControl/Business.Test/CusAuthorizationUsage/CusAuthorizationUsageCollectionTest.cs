using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.IE.ExitControl.Business.Testing
{
	[TestedType(typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignment>))]
	class CusAuthorizationUsageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestGetEnumerator()
		{
			var authorizationUsageCollection = (CusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignment>)(EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignment>)GetCollectionToTest();
			AssertNotNull(authorizationUsageCollection.GetEnumerator());
		}

		public void TestSetDefaultsForNewChild()
		{
			var consignment = Factory.New<CusExitConsignment>();
			var authorizationUsage = consignment.CusAuthorizationUsages.AddNew();
			AssertEquals(CusExitConsignmentSchema.Constants.Prefix, authorizationUsage.AGC_ParentTableCode);

			var item = consignment.CusExitConsignmentItems.AddNew();
			authorizationUsage = item.CusAuthorizationUsages.AddNew();
			AssertEquals(CusExitConsignmentItemSchema.Constants.Prefix, authorizationUsage.AGC_ParentTableCode);
		}

		protected override Type GetExpectedCollectionType() => typeof(CusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignment>);

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var testCusExitConsignment = Factory.New<CusExitConsignment>();
			return (BusinessObjectCollection)testCusExitConsignment.CusAuthorizationUsages;
		}
	}
}
