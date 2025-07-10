using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.EConversation.Testing
{
	sealed class SubscriberAutoCompleteHelperTest : TestCaseWithFactory
	{
		public void TestGetWithPartialResult()
		{
			var provider = Factory.NewWithValidTestData<DummyConversationProvider>();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Xzyllio Zamarin";

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "XZYS Uncommon Name";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_ContactName = "Xzysiso";
			Factory.Save();

			provider.AddRelatedParticipant(contact);

			Factory.Save();

			var helper = new SubscriberAutocompleteHelper(Factory, provider);

			var list = helper.GetList("Xzy");
			Assert("Should find our items above", list.Count >= 3);
			AssertEquals("All items should be of type SubscriberWrapper", list.Count, list.OfType<SubscriberWrapper>().Count());

			var contents = list.Cast<SubscriberWrapper>().Select(wrapper => wrapper.Parent).ToList();
			AssertCollectionContains("Should contain staff", staff, contents);
			AssertCollectionContains("Should contain group", group, contents);
			AssertCollectionContains("Should contain contact", contact, contents);
		}

		public void TestGetListDoesntExceedItemLimit()
		{
			var provider = Factory.NewWithValidTestData<DummyConversationProvider>();

			for (var i = 0; i < 15; i++)
			{
				Factory.NewWithValidTestData<GlbStaff>().GS_Code = "G" + i;
			}

			for (var i = 0; i < 15; i++)
			{
				Factory.NewWithValidTestData<GlbGroup>().GG_Code = "G" + i;
			}

			for (var i = 0; i < 15; i++)
			{
				var contact = Factory.NewWithValidTestData<OrgContact>();
				contact.OC_ContactName = "O" + i;

				provider.AddRelatedParticipant(contact);
			}

			Factory.Save();

			var helper = new SubscriberAutocompleteHelper(Factory, provider);
			AssertEquals("Should not attempt to load everything - should limit to 30", 30, helper.GetList(string.Empty).Count);
		}

		public void TestListDoesRecieveRelationWhenPresent()
		{
			var provider = Factory.NewWithValidTestData<DummyConversationProvider>();
			var contact = Factory.NewWithValidTestData<OrgContact>();
			provider.AddRelatedParticipant(contact, "Da Boss");

			Factory.Save();

			var helper = new SubscriberAutocompleteHelper(Factory, provider);

			var wrapper = helper.GetList("").Cast<SubscriberWrapper>().First(w => w.Parent == contact);

			AssertEquals("The participants relation should be passed onto the subscriber", "Da Boss", wrapper.Relation);
		}

		public void TestGetListHonorsAdditionalParticipants()
		{
			var provider = Factory.NewWithValidTestData<DummyConversationProvider>();
			var addedContact = Factory.NewWithValidTestData<OrgContact>();
			addedContact.OC_ContactName = "ImAdded";

			var notAddedContact = Factory.NewWithValidTestData<OrgContact>();
			notAddedContact.OC_ContactName = "ImNot";

			provider.AddRelatedParticipant(addedContact);

			Factory.Save();

			var helper = new SubscriberAutocompleteHelper(Factory, provider);
			var contacts = GetFilteredList<OrgContact>(helper, string.Empty);

			AssertEquals("It should only contain the contacts relevant to the job", addedContact, contacts.Single());
		}

		public void TestGetListSkipsNullAdditionalParticipants()
		{
			var provider = Factory.NewWithValidTestData<DummyConversationProvider>();
			provider.AddRelatedParticipant(null);
			Factory.Save();

			var helper = new SubscriberAutocompleteHelper(Factory, provider);
			AssertNoExceptionThrown(() => GetFilteredList<OrgContact>(helper, string.Empty));
		}

		public void TestGetListFiltersByPartialResult()
		{
			var provider = Factory.NewWithValidTestData<DummyConversationProvider>();
			var john = Factory.NewWithValidTestData<OrgContact>();
			john.OC_ContactName = "John";

			var mojo = Factory.NewWithValidTestData<OrgContact>();
			mojo.OC_ContactName = "Mojo";

			provider.AddRelatedParticipant(john);
			provider.AddRelatedParticipant(mojo);

			Factory.Save();

			var helper = new SubscriberAutocompleteHelper(Factory, provider);

			var contacts = GetFilteredList<OrgContact>(helper, string.Empty);
			AssertArrayEqualsByElements("For string.Empty they should all match", new[] { john, mojo }, contacts);

			contacts = GetFilteredList<OrgContact>(helper, "Jo");
			AssertArrayEqualsByElements("'Jo' matches both Josh and Mojo", new[] { john, mojo }, contacts);

			contacts = GetFilteredList<OrgContact>(helper, "Joh");
			AssertArrayEqualsByElements("Joh only matches John", new[] { john }, contacts);
		}

		public void TestGetListIncludesCurrentlySubscribedRelatedParties()
		{
			var provider = Factory.NewWithValidTestData<DummyConversationProvider>();

			var related = Factory.NewWithValidTestData<OrgContact>();
			provider.AddRelatedParticipant(related);

			var notRelated = Factory.NewWithValidTestData<OrgContact>();

			Factory.Save();

			provider.eConversation.Participants.AddNewParticipant(related);
			provider.eConversation.Participants.AddNewParticipant(notRelated);

			var helper = new SubscriberAutocompleteHelper(Factory, provider);
			var contacts = GetFilteredList<OrgContact>(helper, string.Empty);

			AssertContainsExactElementsInAnyOrder(new[] { related, notRelated }, contacts);
		}

		public void TestGetListDoesntIncludeInactiveMembers()
		{
			var activeOrg = Factory.NewWithValidTestData<OrgHeader>();
			activeOrg.OH_FullName = "XYZ_ORG_1";
			var inactiveOrg = Factory.NewWithValidTestData<OrgHeader>();
			inactiveOrg.OH_FullName = "XYZ_ORG_2";
			inactiveOrg.OH_IsActive = false;

			var activeContact = Factory.NewWithValidTestData<OrgContact>();
			activeContact.OC_ContactName = "XYZ_OC_1";
			var inactiveContact = Factory.NewWithValidTestData<OrgContact>();
			inactiveContact.OC_ContactName = "XYZ_OC_2";
			inactiveContact.OC_IsActive = false;

			var activeGroup = Factory.NewWithValidTestData<GlbGroup>();
			activeGroup.GG_Desc = "XYZ_GG_1";
			var inactiveGroup = Factory.NewWithValidTestData<GlbGroup>();
			inactiveGroup.GG_Desc = "XYZ_GG_2";
			inactiveGroup.GG_IsActive = false;

			var activeStaff = Factory.NewWithValidTestData<GlbStaff>();
			activeStaff.GS_FullName = "XYZ_GS_1";
			var inactiveStaff = Factory.NewWithValidTestData<GlbStaff>();
			inactiveStaff.GS_FullName = "XYZ_GS_2";
			inactiveStaff.GS_IsActive = false;

			var provider = Factory.NewWithValidTestData<DummyConversationProvider>();
			provider.AddRelatedParticipant(activeOrg);
			provider.AddRelatedParticipant(inactiveOrg);
			provider.AddRelatedParticipant(activeContact);
			provider.AddRelatedParticipant(inactiveContact);

			Factory.Save();

			var helper = new SubscriberAutocompleteHelper(Factory, provider);
			var list = GetFilteredList<IConversationParticipant>(helper, "XYZ");

			var activeParticipants = new IConversationParticipant[] { activeOrg, activeContact, activeGroup, activeStaff };
			var inactiveParticipants = new IConversationParticipant[] { inactiveOrg, inactiveContact, inactiveGroup, inactiveStaff };

			CombineAssertions(() =>
			{
				foreach (var participant in activeParticipants)
				{
					AssertCollectionContains($"Active {participant.GetType().Name} should be included", participant, list);
				}

				foreach (var participant in inactiveParticipants)
				{
					AssertCollectionNotContains($"Inactive {participant.GetType().Name} should NOT be included", participant, list);
				}
			});
		}

		public void TestSubscriberWrapperCodeIsConstructedProperly()
		{
			var user1 = Factory.NewWithValidTestData<OrgHeader>();
			user1.OH_Code = "TS1";
			user1.OH_FullName = "Test1";

			var user2 = Factory.NewWithValidTestData<OrgContact>();
			user2.OC_ContactName = "Test2";

			var user3 = Factory.NewWithValidTestData<GlbGroup>();
			user3.GG_Code = "TS3";
			user3.GG_Desc = "Test3";

			var user4 = Factory.NewWithValidTestData<GlbStaff>();
			user4.GS_Code = "TS4";
			user4.GS_FullName = "Test4";

			var otherParticipants = new IConversationParticipant[] { user1, user3, user4 };
			var contactParticipants = new IConversationParticipant[] { user2 };

			CombineAssertions(() =>
			{
				foreach (var participant in otherParticipants)
				{
					var wrapper = new SubscriberWrapper(participant);
					AssertEquals($"The displayed text for {participant.Name} should show Name and Code", participant.Name + " (" + participant.Code + ")", wrapper.Code);
				}

				foreach (var participant in contactParticipants)
				{
					var wrapper = new SubscriberWrapper(participant);
					AssertEquals($"The displayed text for {participant.Name} should show Name and Code", participant.Name, wrapper.Code);
				}
			});
		}

		T[] GetFilteredList<T>(SubscriberAutocompleteHelper helper, string partial)
		{
			return helper.GetList(partial)
				.Cast<SubscriberWrapper>()
				.Select(wrapper => wrapper.Parent)
				.OfType<T>()
				.ToArray();
		}
	}
}
