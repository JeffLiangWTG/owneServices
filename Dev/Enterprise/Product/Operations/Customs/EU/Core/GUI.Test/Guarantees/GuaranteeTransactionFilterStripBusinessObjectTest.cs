using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(GuaranteeTransactionFilterStripBusinessObject))]
	class GuaranteeTransactionFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestFilters()
		{
			var filter = new GuaranteeTransactionFilterStripBusinessObject();
			AssertNotNull(filter["Type"]);
			AssertNotNull(filter["Reference"]);
		}

		public void TestTypeFilter()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();

			var transaction1 = AddNewGuaranteeLineTransaction(guaranteeHeader, "ref1", 1, "OBL");
			var transaction2 = AddNewGuaranteeLineTransaction(guaranteeHeader, "ref2", 2, "OBL");
			var transaction3 = AddNewGuaranteeLineTransaction(guaranteeHeader, "ref3", 3, "OBA");

			var transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.TypeGuarantee, "OBL", guaranteeHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Total Transaction match the filter with OBL", 2, transactions.Count);
				AssertEquals("Transaction 1 is in the filter type OBL", true, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is in the filter type OBL", true, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter type OBL", false, transactions.Contains(transaction3));

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.TypeGuarantee, "OBA", guaranteeHeader);

				AssertEquals("Total Transaction match the filter with OBA", 1, transactions.Count);
				AssertEquals("Transaction 1 is not in the filter type OBA", false, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter type OBA", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is in the filter type OBA", true, transactions.Contains(transaction3));

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.TypeGuarantee, "CCC", guaranteeHeader);

				AssertEquals("Total Transaction match the filter with CCC", 0, transactions.Count);
				AssertEquals("Transaction 1 is not in the filter type CCC", false, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter type CCC", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter type CCC", false, transactions.Contains(transaction3));
			});
		}

		public void TestReferenceFilter()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();

			var transaction1 = AddNewGuaranteeLineTransaction(guaranteeHeader, "Reference1", 10, "OBL");
			var transaction2 = AddNewGuaranteeLineTransaction(guaranteeHeader, "Reference1", 10, "OBL");
			var transaction3 = AddNewGuaranteeLineTransaction(guaranteeHeader, "Reference2", 10, "OBL");

			var transactions = new CusGuaranteeLineTransactionCollection(guaranteeHeader);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total CusGuarantee Transactions", 3, transactions.Count);

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.ReferenceGuarantee, "Reference1", guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 2, transactions.Count);
				AssertEquals("Transaction 1 is in the filter cause its Reference is Reference1", true, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is in the filter cause its Reference is Reference1", true, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter cause its Reference is not Reference1", false, transactions.Contains(transaction3));

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.ReferenceGuarantee, "Reference2", guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 1, transactions.Count);
				AssertEquals("Transaction 1 is not in the filter cause its Reference is not Reference2", false, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter cause its Reference is not Reference2", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is in the filter cause its Reference is Reference2", true, transactions.Contains(transaction3));

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.ReferenceGuarantee, "Reference3", guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 0, transactions.Count);
				AssertEquals("Transaction 1 is not in the filter cause its Reference is not Reference3", false, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter cause its Reference is not Reference3", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter cause its Reference is not Reference3", false, transactions.Contains(transaction3));
			});
		}

		public void TestValueFilter()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();

			var transaction1 = AddNewGuaranteeLineTransaction(guaranteeHeader, "Reference", 10, "OBL");
			var transaction2 = AddNewGuaranteeLineTransaction(guaranteeHeader, "Reference", 20, "OBL");
			var transaction3 = AddNewGuaranteeLineTransaction(guaranteeHeader, "Reference", 30, "OBL");

			var transactions = new CusGuaranteeLineTransactionCollection(guaranteeHeader);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total CusGuarantee Transactions", 3, transactions.Count);

				transactions = LoadCollectionAmountFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.ValueGuarantee, 0, 20, guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 2, transactions.Count);
				AssertEquals("Transaction 1 is in the filter cause its Value is between 0 and 20", true, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is in the filter cause its Value is between 0 and 20", true, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter cause its Value is not between 0 and 20", false, transactions.Contains(transaction3));

				transactions = LoadCollectionAmountFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.ValueGuarantee, 0, 10, guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 1, transactions.Count);
				AssertEquals("Transaction 1 is in the filter cause its Value is between 0 and 10", true, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter cause its Value is not between 0 and 10", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter cause its Value is not between 0 and 10", false, transactions.Contains(transaction3));

				transactions = LoadCollectionAmountFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.ValueGuarantee, 0, 5, guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 0, transactions.Count);
				AssertEquals("Transaction 1 is not in the filter cause its Value is not between 0 and 5", false, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter cause its Value is not between 0 and 5", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter cause its Value is not between 0 and 5", false, transactions.Contains(transaction3));
			});
		}

		public void TestCommentFilter()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();

			var transaction1 = AddNewGuaranteeLineTransaction(guaranteeHeader, "Reference", 10, "OBL", "Comment1");
			var transaction2 = AddNewGuaranteeLineTransaction(guaranteeHeader, "Reference", 10, "OBL", "Comment1");
			var transaction3 = AddNewGuaranteeLineTransaction(guaranteeHeader, "Reference", 10, "OBL", "Comment2");

			var transactions = new CusGuaranteeLineTransactionCollection(guaranteeHeader);

			CombineAssertions(() =>
			{
				AssertEquals("[PRECONDITION] Total CusGuarantee Transactions", 3, transactions.Count);

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.CommentGuarantee, "Comment1", guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 2, transactions.Count);
				AssertEquals("Transaction 1 is in the filter cause its Comment is Comment1", true, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is in the filter cause its Comment is Comment1", true, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter cause its Comment is not Comment1", false, transactions.Contains(transaction3));

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.CommentGuarantee, "Comment2", guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 1, transactions.Count);
				AssertEquals("Transaction 1 is not in the filter cause its Comment is not Comment2", false, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter cause its Comment is not Comment2", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is in the filter cause its Comment is Comment2", true, transactions.Contains(transaction3));

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.CommentGuarantee, "Comment3", guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 0, transactions.Count);
				AssertEquals("Transaction 1 is not in the filter cause its Comment is not Comment3", false, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter cause its Comment is not Comment3", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter cause its Comment is not Comment3", false, transactions.Contains(transaction3));
			});
		}

		public void TestStatusFilter()
		{
			var guaranteeHeader = Factory.NewWithValidTestData<BaseCusGuaranteeHeader>();

			var transaction1 = AddNewGuaranteeLineTransaction(guaranteeHeader, "ref1", 1, "OBL", status: "CON");
			var transaction2 = AddNewGuaranteeLineTransaction(guaranteeHeader, "ref2", 2, "OBL", status: "CON");
			var transaction3 = AddNewGuaranteeLineTransaction(guaranteeHeader, "ref3", 3, "OBA", status: "PND");

			var transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.StatusGuarantee, "CON", guaranteeHeader);

			CombineAssertions(() =>
			{
				AssertEquals("Total Transaction match the filter", 2, transactions.Count);
				AssertEquals("Transaction 1 is in the filter cause its Status is CON", true, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is in the filter cause its Status is CON", true, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter cause its Status is not CON", false, transactions.Contains(transaction3));

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.StatusGuarantee, "PND", guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 1, transactions.Count);
				AssertEquals("Transaction 1 is not in the filter cause its Status is not PND", false, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter cause its Status is not PND", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is in the filter cause its Status is PND", true, transactions.Contains(transaction3));

				transactions = LoadCollectionTextFilter(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.StatusGuarantee, "DEL", guaranteeHeader);

				AssertEquals("Total Transaction match the filter", 0, transactions.Count);
				AssertEquals("Transaction 1 is not in the filter cause its Status is not DEL", false, transactions.Contains(transaction1));
				AssertEquals("Transaction 2 is not in the filter cause its Status is not DEL", false, transactions.Contains(transaction2));
				AssertEquals("Transaction 3 is not in the filter cause its Status is not DEL", false, transactions.Contains(transaction3));
			});
		}

		public void TestTypeFilterHasSomeComparisonConstansRemoved()
		{
			AssertFilterHasSomeComparisonOptionsRemoved(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.TypeGuarantee, typeAndStatusComparisonConstansRemoved);
		}

		public void TestStatusFilterHasSomeComparisonConstansRemoved()
		{
			AssertFilterHasSomeComparisonOptionsRemoved(GuaranteeTransactionFilterStripBusinessObject.EUFilterConstants.StatusGuarantee, typeAndStatusComparisonConstansRemoved);
		}

		readonly string[] typeAndStatusComparisonConstansRemoved = { ModuleTextFilter.ComparisonConstants.StartsWith, ModuleTextFilter.ComparisonConstants.NotStartsWith,
				ModuleTextFilter.ComparisonConstants.Contains, ModuleTextFilter.ComparisonConstants.NotContain };

		void AssertFilterHasSomeComparisonOptionsRemoved(ZString filterName, params string[] removedComparisonOptions)
		{
			var filterComparisonOperatorList = ((ModuleTextFilter)filterBusinessObject[filterName]).ComparisonOperator_List;

			foreach (string removedOption in removedComparisonOptions)
			{
				AssertEquals($"Does {filterName} have {removedOption} filter option?", false, filterComparisonOperatorList.ContainsCode(removedOption));
			}
		}

		CusGuaranteeLineTransactionCollection LoadCollectionTextFilter(ZString filterField, ZString filterValue, BaseCusGuaranteeHeader guaranteeHeader, string comparisonOperator = ModuleFilterWithListAndComparisonOperators<ZString>.ComparisonConstants.Exact)
		{
			var filter = (ModuleTextFilter)filterBusinessObject[filterField];
			filter.IsActive = true;
			filter.Property = filterValue;
			filter.ComparisonOperator = comparisonOperator;
			var transactions = new CusGuaranteeLineTransactionCollection(guaranteeHeader, filterBusinessObject.Filter);
			return transactions;
		}

		CusGuaranteeLineTransactionCollection LoadCollectionAmountFilter(ZString filterField, ZDecimal filterValueRange1, ZDecimal filterValueRange2, BaseCusGuaranteeHeader guaranteeHeader)
		{
			var filter = (ModuleNumberRangeFilter)filterBusinessObject[filterField];
			filter.IsActive = true;
			filter.Decimals = 2;
			filter.BetweenDefaultProperty1 = filterValueRange1;
			filter.BetweenDefaultProperty2 = filterValueRange2;
			var transactions = new CusGuaranteeLineTransactionCollection(guaranteeHeader, filterBusinessObject.Filter);
			return transactions;
		}

		BaseCusGuaranteeLineTransaction AddNewGuaranteeLineTransaction(BaseCusGuaranteeHeader guaranteeHeader, ZString reference, ZDecimal tranValue, ZString type, string comment = "", string status = "")
		{
			var transaction = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction.CPL_Reference = reference;
			transaction.CPL_TranValue = tranValue;
			transaction.CPL_TransactionType = type;
			transaction.CPL_TransactionStatus = PermitTransactionStatusList.Codes.Pending;
			transaction.CPL_Comment = comment;
			transaction.CPL_TransactionStatus = status;
			return transaction;
		}

		protected override void SetUp()
		{
			base.SetUp();
			filterBusinessObject = new GuaranteeTransactionFilterStripBusinessObject();
		}
		GuaranteeTransactionFilterStripBusinessObject filterBusinessObject;

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => filterBusinessObject;
	}
}
