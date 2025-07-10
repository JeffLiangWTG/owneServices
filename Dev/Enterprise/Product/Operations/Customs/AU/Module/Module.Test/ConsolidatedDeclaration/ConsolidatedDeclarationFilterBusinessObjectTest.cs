using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(ConsolidatedDeclarationFilterBusinessObject))]
	sealed class ConsolidatedDeclarationFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestMessageStatusFilter()
		{
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.MessageStatusText];
			var expectedMessageStatusList = new JobDeclarationFilterBusinessObject().Lookups.CMRMessageStatusList;
			AssertContainsExactElementsInExactOrder(expectedMessageStatusList, filter.List);
		}

		public void TestPaymentStatusFilterProperties()
		{
			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PaymentStatusText];
			var expectedList = new JobDeclarationFilterBusinessObject().Lookups.PaymentStatusList;
			AssertContainsExactElementsInExactOrder(expectedList, filter.List);
			AssertEquals("Category", FilterCategories.StatusAndFlags, filter.Category);
		}

		public void TestPaymentStatusFilter() => CombineAssertions(() =>
		{
			var consolidatedDec1 = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var entryHeader1 = (CusEntryHeader)consolidatedDec1.LeadDeclaration.CustomsEntryHeaders[0];
			entryHeader1.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Paid;

			var consolidatedDec2 = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var entryHeader2 = (CusEntryHeader)consolidatedDec2.LeadDeclaration.CustomsEntryHeaders[0];
			entryHeader2.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.Refunded;

			var consolidatedDec3 = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var entryHeader3 = (CusEntryHeader)consolidatedDec3.LeadDeclaration.CustomsEntryHeaders[0];
			entryHeader3.AddInfo.ZA_PaymentStatus_Hidden = CMREntryPaymentStatusList.Codes.PayPending;

			var consolidatedDec4 = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			Factory.Save();

			var filter = (ModuleTextFilter)filterBO[DeclarationFilterConstants.PaymentStatusText];
			filter.IsActive = true;

			AssertFilterQuery(DeclarationFilterConstants.PaymentStatus.Paid, true, true, false, false);
			AssertFilterQuery(DeclarationFilterConstants.PaymentStatus.NotPaid, false, false, true, true);

			void AssertFilterQuery(string filterValue, bool matchesConsolidatedDec1, bool matchesConsolidatedDec2, bool matchesConsolidatedDec3, bool matchesConsolidatedDec4)
			{
				filter.Property = filterValue;
				var filterQuery = filter.Query;
				AssertEquals("ConsolidatedDec1", matchesConsolidatedDec1, consolidatedDec1.MatchesFilter(filterQuery));
				AssertEquals("ConsolidatedDec2", matchesConsolidatedDec2, consolidatedDec2.MatchesFilter(filterQuery));
				AssertEquals("ConsolidatedDec3", matchesConsolidatedDec3, consolidatedDec3.MatchesFilter(filterQuery));
				AssertEquals("ConsolidatedDec4", matchesConsolidatedDec4, consolidatedDec4.MatchesFilter(filterQuery));
			}
		});

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ConsolidatedDeclarationFilterBusinessObject();

		protected override void SetUp()
		{
			base.SetUp();
			filterBO = GetNewFilterStripBusinessObject();
		}
		FilterStripBusinessObject filterBO;
	}
}
