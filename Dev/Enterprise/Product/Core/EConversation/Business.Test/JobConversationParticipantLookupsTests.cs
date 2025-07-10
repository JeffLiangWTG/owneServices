using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.EConversation.Testing.BusinessObjects
{
	sealed class JobConversationParticipantLookupsTests : TestCaseWithFactory
	{
		public void TestRelatedItemsLookups_Staff()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ASS";

			AssertReturnsCorrectCollection<GlbStaffCollection>("staff", staff, convo => convo.Staff);
		}

		public void TestRelatedItemsLookups_Group()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "ASS";

			AssertReturnsCorrectCollection<GlbGroupCollection>("group", group, convo => convo.Groups);
		}

		public void TestRelatedItemsLookups_Contact()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "ASS";

			AssertReturnsCorrectCollection<OrgContactCollection>("Contact", contact, convo => convo.RelatedParties);
		}

		void AssertReturnsCorrectCollection<T>(string name, IConversationParticipant participantToAdd, Func<JobConversation, JobConversationParticipantCollection> getCollectionToAddTo) where T : class, IBusinessObjectCollection
		{
			var parent = Factory.NewWithValidTestData<DummyConversationProvider>();
			Factory.Save();

			var participant = getCollectionToAddTo(parent.eConversation).AddNew();
			participant.ParentKey = participantToAdd.Code;

			AssertEquals($"PRE: Because the participant was created by the {name} list, it should recognise that it's recieved a {name} code", participantToAdd, participant.Parent);

			Assert($"When we have a {name} participant, the lookups should return a {name} collection", participant.Lookups.AvailableParentsList is T);
		}
	}
}
