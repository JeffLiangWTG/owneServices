using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Filters.Testing
{
	[TestedType(typeof(PresentationCategoryFilter))]
	sealed class PresentationCategoryFilterTest : ModuleTextFilterTest
	{
		public void TestGetNewValidation()
		{
			AssertNotNull(Filter.Validation);
			AssertEquals("Validation type", typeof(PresentationCategoryFilterValidation), Filter.Validation.GetType());
		}

		protected override ModuleTextFilter GetNewModuleFilter()
		{
			return GetNewBusinessObject() as PresentationCategoryFilter;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var presentationCategoryList = new CodeDescriptionPairList(AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetValueWithoutFallback(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList());
			presentationCategoryList.AddRangeOverwriteIfExists(AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty).GetActiveCodeDescriptionPairList());

			return new PresentationCategoryFilter("Presentation Category", GetPresentationCategoryQuery, presentationCategoryList);

			ZQuery GetPresentationCategoryQuery(SQLComparisonOperator @operator, ZString value)
			{
				var operatorList = new List<SQLComparisonOperator>() { SQLComparisonOperator.Contains, SQLComparisonOperator.NotContains, SQLComparisonOperator.Equal, SQLComparisonOperator.NotEqual };
				var query = new ZDBOnlyQuery(typeof(AccGeneralLedgerData));
				var categorySubQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccGeneralLedgerDataSchema.GLD_AH_TransactionHeader);
				categorySubQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.General);

				if (string.IsNullOrEmpty(value) || !operatorList.Contains(@operator))
				{
					categorySubQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionCategory, @operator, value);
					query.AddSubQuery(categorySubQuery, JoinCondition.And);
					return query;
				}

				var glPresentationJournalCategoryCollection = AccountingConfigurationRegistry.Instance.GLPresentationJournalCategoriesList.Value.Cast<GLPresentationJournalCategory>();
				var childCategories = glPresentationJournalCategoryCollection.Where(x => x.Bool && x.Code.Contains(value)).Select(x => x.Code).ToList();
				if (@operator == SQLComparisonOperator.Equal || @operator == SQLComparisonOperator.NotEqual)
				{
					childCategories = childCategories.Where(x => x.Equals(value)).ToList();
				}

				if (childCategories.Count == 0)
				{
					categorySubQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionCategory, @operator, value);
					query.AddSubQuery(categorySubQuery, JoinCondition.And);
					return query;
				}
				var realOperator = @operator == SQLComparisonOperator.Contains || @operator == SQLComparisonOperator.Equal ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual;
				categorySubQuery.AddToFilter(JoinCondition.And, AccTransactionHeaderSchema.AH_TransactionCategory, realOperator, childCategories);
				query.AddSubQuery(categorySubQuery, JoinCondition.And);

				return query;
			}
		}

		protected override ZString ExpectedDescription
		{
			get { return "Presentation Category"; }
		}
	}
}
