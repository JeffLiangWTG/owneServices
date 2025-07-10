using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CA.Business.Testing
{
	[TestedType(typeof(CusStatementLineGroupFinancialDetailCollection))]
	sealed class CusStatementLineGroupFinancialDetailCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestUpdateFinancialDetailFor()
		{
			var group = Factory.New<CusStatementLineGroup>();
			var collection = group.FinancialDetailCollection;

			var financialDetail = collection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalCredits, 0m);
			AssertNull("Should be null as the amount is zero.", financialDetail);

			financialDetail = collection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalCredits, 10m);
			AssertEquals("Should add a new element.", PostingJournalTypeList.Codes.TotalCredits, financialDetail.B11_Type);
			AssertEquals("Should add a new element.", 10m, financialDetail.B11_Amount);

			var result = collection.UpdateFinancialDetailFor(PostingJournalTypeList.Codes.TotalCredits, 0m);
			AssertNull("Should be null as the amount is zero.", result);
			Assert("Should be deleted as the amount is zero.", financialDetail.IsDeleted);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var group = Factory.New<CusStatementLineGroup>();
			return group.FinancialDetailCollection;
		}
	}
}
