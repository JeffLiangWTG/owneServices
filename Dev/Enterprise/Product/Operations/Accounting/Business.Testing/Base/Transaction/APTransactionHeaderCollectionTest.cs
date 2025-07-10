using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Transaction.Testing
{
	[TestedType(typeof(APTransactionHeaderCollection))]
	public class APTransactionHeaderCollectionTest : TransactionHeaderCollectionTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new APTransactionHeaderCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New(typeof(APInvoice));
		}

		public void TestRelationshipFilter()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			GlbBranch anotherBranch = Factory.NewWithValidTestData<GlbBranch>();

			APInvoice aPInv = Factory.NewWithValidTestData<APInvoice>();
			ARInvoice aRInv = Factory.NewWithValidTestData<ARInvoice>();
			APInvoice aPInv_AnotherBranch = Factory.NewWithValidTestData<APInvoice>();
			aPInv_AnotherBranch.AH_GB = anotherBranch.PK;

			Collection.Load();
			AssertEquals("Collection should contain 1 element", 1, Collection.Count);
			Assert("The element should be APInv", Collection.Contains(aPInv));
		}
	}
}
