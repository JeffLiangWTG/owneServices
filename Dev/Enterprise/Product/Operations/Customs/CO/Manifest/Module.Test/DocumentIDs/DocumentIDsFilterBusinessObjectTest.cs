using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CO.Manifest.Module.Testing
{
	[TestedType(typeof(DocumentIDsFilterBusinessObject))]
	class DocumentIDsFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestIsUsedFilter()
		{
			var filterObj = new DocumentIDsFilterBusinessObject();
			var filter = (ModuleFlagsFilter)filterObj[DocumentIDsFilterBusinessObject.FilterConstants.IsUsed];
			filter.IsActive = true;
			filter.Property0 = ZBool.True;

			var isUsed1 = Factory.NewWithValidTestData<CusTransactionNumber>();
			isUsed1.TN_IsUsed = ZBool.True;

			var isUsed2 = Factory.NewWithValidTestData<CusTransactionNumber>();
			isUsed2.TN_IsUsed = ZBool.False;

			CombineAssertions("Asserted to Match the Filters for Is Used", () =>
			{
				Assert(isUsed1.MatchesFilter(filterObj.Filter));
				Assert(!isUsed2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestTransactionNumberFilter()
		{
			var filterObj = new DocumentIDsFilterBusinessObject();
			var filter = (ModuleTextFilter)filterObj[DocumentIDsFilterBusinessObject.FilterConstants.TransactionReference];
			filter.IsActive = true;
			filter.Property = "11667803932049";

			var transactionNumber1 = Factory.NewWithValidTestData<CusTransactionNumber>();
			transactionNumber1.TN_TransactionReference = "11667803932049";

			var transactionNumber2 = Factory.NewWithValidTestData<CusTransactionNumber>();
			transactionNumber2.TN_TransactionReference = "11667803932050";

			CombineAssertions("Asserted to Match the Filters for Transaction Number", () =>
			{
				Assert(transactionNumber1.MatchesFilter(filterObj.Filter));
				Assert(!transactionNumber2.MatchesFilter(filterObj.Filter));
			});
		}

		public void TestFilters()
		{
			var filter = new DocumentIDsFilterBusinessObject();
			AssertNotNull(filter[DocumentIDsFilterBusinessObject.FilterConstants.TransactionReference]);
			AssertNotNull(filter[DocumentIDsFilterBusinessObject.FilterConstants.IsUsed]);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new DocumentIDsFilterBusinessObject();
	}
}
