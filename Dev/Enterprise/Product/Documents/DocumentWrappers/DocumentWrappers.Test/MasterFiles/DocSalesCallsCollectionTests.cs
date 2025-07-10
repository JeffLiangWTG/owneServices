using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.MasterFiles.Testing
{
	[TestedType(typeof(DocSalesCallsCollection))]
	sealed class DocSalesCallsCollectionTests : NonPersistentBusinessObjectCollectionTestCase<DocSalesCallsCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var orgSalesCall = Factory.New<OrgSalesCall>();
			return DocSalesCall.New(orgSalesCall, Factory);
		}

		protected override DocSalesCallsCollection GetCollectionToTest()
		{
			return new DocSalesCallsCollection(Factory);
		}

		public void TestCreateDocSalesCallsCollection()
		{
			OrgHeader orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			OrgSalesCall orgSalesCall1 = orgHeader.SalesCalls.AddNew();
			OrgSalesCall orgSalesCall2 = orgHeader.SalesCalls.AddNew();

			orgSalesCall1.ShouldIncludeOnSalesCallDocument = true;
			orgSalesCall2.ShouldIncludeOnSalesCallDocument = true;

			orgHeader.SalesCalls.ShouldCheckIncludeOnSalesCallDocument = true;
			DocSalesCallsCollection dscc = new DocSalesCallsCollection(Factory, orgHeader.SalesCalls);
			Assert("Must be 2 DocSalesCalls in collection", dscc.Count == 2);

			orgSalesCall2.ShouldIncludeOnSalesCallDocument = false;
			dscc = new DocSalesCallsCollection(Factory, orgHeader.SalesCalls);
			Assert("Must be 2 DocSalesCalls in collection", dscc.Count == 1);

			orgHeader.SalesCalls.ShouldCheckIncludeOnSalesCallDocument = false;
			dscc = new DocSalesCallsCollection(Factory, orgHeader.SalesCalls);
			Assert("Must be 2 DocSalesCalls in collection", dscc.Count == 2);
		}
	}
}
