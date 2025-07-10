using System.Linq;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Module.Testing
{
	[TestedType(typeof(IncidentEConversationParticipantsFilter<GlbGroup>))]
	class IncidentGroupParticipantsFilterTest : ModuleFilterTestCase<IncidentEConversationParticipantsFilter<GlbGroup>>
	{
		public void TestGroupParticipantsOfIncidentFilterQuery()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var incident1 = Factory.NewWithValidTestData<SupportIncident>();
			incident1.EConversation.Conversation.RelatedParties.AddNewParticipant(group);
			var incident2 = Factory.NewWithValidTestData<SupportIncident>();
			var incident3 = Factory.NewWithValidTestData<SupportIncident>();

			Factory.Save();

			var filter = Filter;
			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.AnyMatch;

			var result = Factory.Load<SupportIncident>(filter.Query);
			AssertEquals(1, result.Length);
			AssertEquals(result.FirstOrDefault().PK, incident1.PK);
		}

		public void TestAnyMatchWithFiltersSupportIncident()
		{
			CreateTestIncidents(out var incidentAlexBob, out var incidentAlex, out var incidentBob, out var incidentNoOne);
			Filter.SelectedFilters.AddTextFilterStrip("Code", "ALX");
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.AnyMatch, [incidentAlex, incidentAlexBob]);
		}

		public void TestAllMatchWithFiltersSupportIncident()
		{
			CreateTestIncidents(out var incidentAlexBob, out var incidentAlex, out var incidentBob, out var incidentNoOne);
			Filter.SelectedFilters.AddTextFilterStrip("Code", "ALX");
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.AllMatch, [incidentAlex, incidentNoOne]);
		}

		public void TestNoneMatchWithFiltersSupportIncident()
		{
			CreateTestIncidents(out var incidentAlexBob, out var incidentAlex, out var incidentBob, out var incidentNoOne);
			Filter.SelectedFilters.AddTextFilterStrip("Code", "ALX");
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.NoneMatch, [incidentBob, incidentNoOne]);
		}

		public void TestAnyMatchWithNoFiltersSupportIncident()
		{
			CreateTestIncidents(out var incidentAlexBob, out var incidentAlex, out var incidentBob, out var incidentNoOne);
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.AnyMatch, [incidentAlex, incidentAlexBob, incidentBob]);
		}

		public void TestAllMatchWithNoFiltersSupportIncident()
		{
			CreateTestIncidents(out var incidentAlexBob, out var incidentAlex, out var incidentBob, out var incidentNoOne);
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.AllMatch, [incidentAlexBob, incidentAlex, incidentBob, incidentNoOne]);
		}

		public void TestNoneMatchWithNoFiltersSupportIncident()
		{
			CreateTestIncidents(out var incidentAlexBob, out var incidentAlex, out var incidentBob, out var incidentNoOne);
			DoComparisonOperatorTest(ModuleTextFilter.ComparisonConstants.NoneMatch, [incidentNoOne]);
		}

		void CreateTestIncidents(out SupportIncident incidentAlexBob, out SupportIncident incidentAlex, out SupportIncident incidentBob, out SupportIncident incidentNoOne)
		{
			var alex = Factory.NewWithValidTestData<GlbGroup>();
			alex.GG_Code = "ALX";
			var bob = Factory.NewWithValidTestData<GlbGroup>();
			bob.GG_Code = "BOB";

			incidentAlexBob = Factory.NewWithValidTestData<SupportIncident>();
			incidentAlexBob.IM_Description = "Incident with Alex Bob";
			incidentAlexBob.EConversation.Conversation.RelatedParties.AddNewParticipant(alex);
			incidentAlexBob.EConversation.Conversation.RelatedParties.AddNewParticipant(bob);

			incidentAlex = Factory.NewWithValidTestData<SupportIncident>();
			incidentAlex.IM_Description = "Incident with Alex";
			incidentAlex.EConversation.Conversation.RelatedParties.AddNewParticipant(alex);

			incidentBob = Factory.NewWithValidTestData<SupportIncident>();
			incidentBob.IM_Description = "Incident with Bob";
			incidentBob.EConversation.Conversation.RelatedParties.AddNewParticipant(bob);

			incidentNoOne = Factory.NewWithValidTestData<SupportIncident>();
			incidentNoOne.IM_Description = "Incident with no participants";

			Factory.Save();
		}

		void DoComparisonOperatorTest(ZString comparisonOperator, SupportIncident[] expected)
		{
			Filter.ComparisonOperator = comparisonOperator;
			var result = Factory.Load<SupportIncident>(Filter.Query);
			AssertContainsExactElementsInAnyOrder(expected.Select(x => x.IM_Description), result.Select(x => x.IM_Description));
		}

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory ExpectedDefaultCategory => FilterCategories.Other;

		protected override IncidentEConversationParticipantsFilter<GlbGroup> GetNewModuleFilter()
		{
			return new IncidentEConversationParticipantsFilter<GlbGroup>("moo", ModuleIDs.GlbGroup, IncidentMainSchema.PK, new GlbGroupCollection(Factory), typeof(SupportIncident));
		}
	}
}
