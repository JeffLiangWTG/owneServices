using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentGroupEConversationParticipantsFilter<OrgContact>))]
	public class ContactParticipantsFilterTest : ModuleFilterTestCase<IncidentGroupEConversationParticipantsFilter<OrgContact>>
	{
		public void TestContactParticipantsOfIncidentManagementGroupFilterQuery()
		{
			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			var group1 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			group1.EConversation.Conversation.RelatedParties.AddNewParticipant(orgContact);
			var group2 = Factory.NewWithValidTestData<IncidentManagementGroup>();
			var group3 = Factory.NewWithValidTestData<IncidentManagementGroup>();

			Factory.Save();

			BusinessObjectType = typeof(IncidentManagementGroup);
			var filter = Filter;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var result = Factory.Load<IncidentManagementGroup>(filter.Query);
			AssertEquals(1, result.Length);
			AssertEquals(result.FirstOrDefault().PK, group1.PK);
		}

		public void TestAnyMatchWithNoFilters()
		{
			CreateTestIncidentGroups(out var groupAlexBob, out var groupAlex, out var groupBob, out var groupNoOne);
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.AnyMatch, [groupAlex, groupAlexBob, groupBob]);
		}

		public void TestAllMatchWithNoFilters()
		{
			CreateTestIncidentGroups(out var groupAlexBob, out var groupAlex, out var groupBob, out var groupNoOne);
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.AllMatch, [groupAlex, groupAlexBob, groupBob, groupNoOne]);
		}

		public void TestNoneMatchWithNoFilters()
		{
			CreateTestIncidentGroups(out var groupAlexBob, out var groupAlex, out var groupBob, out var groupNoOne);
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.NoneMatch, [groupNoOne]);
		}

		public void TestAnyMatchWithFilters()
		{
			CreateTestIncidentGroups(out var groupAlexBob, out var groupAlex, out var groupBob, out var groupNoOne);
			Filter.SelectedFilters.AddTextFilterStrip("Name", "Alex");
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.AnyMatch, [groupAlex, groupAlexBob]);
		}

		public void TestAllMatchWithFilters()
		{
			CreateTestIncidentGroups(out var groupAlexBob, out var groupAlex, out var groupBob, out var groupNoOne);
			Filter.SelectedFilters.AddTextFilterStrip("Name", "Alex");
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.AllMatch, [groupAlex, groupNoOne]);
		}

		public void TestNoneMatchWithFilters()
		{
			CreateTestIncidentGroups(out var groupAlexBob, out var groupAlex, out var groupBob, out var groupNoOne);
			Filter.SelectedFilters.AddTextFilterStrip("Name", "Alex");
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.NoneMatch, [groupNoOne, groupBob]);
		}

		void CreateTestIncidentGroups(out IncidentManagementGroup groupAlexBob, out IncidentManagementGroup groupAlex, out IncidentManagementGroup groupBob, out IncidentManagementGroup groupNoOne)
		{
			var alex = Factory.NewWithValidTestData<OrgContact>();
			alex.OC_ContactName = "Alex Contact";
			var bob = Factory.NewWithValidTestData<OrgContact>();
			bob.OC_ContactName = "Bob Contact";

			groupAlexBob = Factory.NewWithValidTestData<IncidentManagementGroup>();
			groupAlexBob.ING_Description = "Group with alex and bob";
			groupAlexBob.EConversation.Conversation.RelatedParties.AddNewParticipant(alex);
			groupAlexBob.EConversation.Conversation.RelatedParties.AddNewParticipant(bob);

			groupAlex = Factory.NewWithValidTestData<IncidentManagementGroup>();
			groupAlex.ING_Description = "Group with alex";
			groupAlex.EConversation.Conversation.RelatedParties.AddNewParticipant(alex);

			groupBob = Factory.NewWithValidTestData<IncidentManagementGroup>();
			groupBob.ING_Description = "Group with bob";
			groupBob.EConversation.Conversation.RelatedParties.AddNewParticipant(bob);

			groupNoOne = Factory.NewWithValidTestData<IncidentManagementGroup>();
			groupNoOne.ING_Description = "Group with no participants";
			// The ING conversation is lazily initialised, so we force initialise it like this
			_ = groupNoOne.EConversation.Conversation;

			Factory.Save();
		}

		void DoComparisonOperatorTest(ZString comparisonOperator, IncidentManagementGroup[] expected)
		{
			Filter.ComparisonOperator = comparisonOperator;
			var result = Factory.Load<IncidentManagementGroup>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(expected.Select(x => x.ING_Description), result.Select(x => x.ING_Description));
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		Type businessObjectType;

		protected Type BusinessObjectType
		{
			get => businessObjectType ?? typeof(IncidentManagementGroup);
			set => businessObjectType = value;
		}

		protected SchemaGuidColumn GetPrimaryKeyColumn() => BusinessObjectFactory.GetTableSchemaFromType(BusinessObjectType).PK;

		protected override IncidentGroupEConversationParticipantsFilter<OrgContact> GetNewModuleFilter()
		{
			return new IncidentGroupEConversationParticipantsFilter<OrgContact>("moo", ModuleIDs.OrgContacts, GetPrimaryKeyColumn(), new OrgContactCollection(Factory), BusinessObjectType);
		}
	}
}
