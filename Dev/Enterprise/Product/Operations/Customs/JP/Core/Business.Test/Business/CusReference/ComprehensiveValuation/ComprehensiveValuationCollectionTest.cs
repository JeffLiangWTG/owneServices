using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.JP.Business.Testing
{
	[TestedType(typeof(ComprehensiveValuationCollection))]
	sealed class ComprehensiveValuationCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestMaxCount()
		{
			const int maxRowCount = 3;
			AssertEquals(maxRowCount, Collection.MaxCount);

			for (var i = 0; i < maxRowCount - 1; i++)
			{
				Collection.AddNew();
			}
			Assert($"Currently, collection has {Collection.Count} elements", Collection.AllowNew);
			Collection.AddNew();
			Assert($"Currently, collection has {Collection.Count} elements", !Collection.AllowNew);
		}

		public void TestParentID()
		{
			var reference = Collection.AddNew() as ComprehensiveValuation;
			AssertEquals("CFR_ParentID", invoiceHeader.PK, reference.CFR_ParentID);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			invoiceHeader = declaration.Invoices.AddNew();
			return new ComprehensiveValuationCollection(invoiceHeader);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ComprehensiveValuation>();
		}

		JobComInvoiceHeader invoiceHeader;
		JobDeclaration declaration;
	}
}
