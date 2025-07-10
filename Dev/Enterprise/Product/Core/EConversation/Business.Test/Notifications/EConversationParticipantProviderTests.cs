using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.EConversation.Testing
{
	sealed class EConversationParticipantProviderTests : TestCaseWithFactory
	{
		public void TestGetParticipants_ShouldConsiderAdditionalParticipants()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProviderWithAdditionalParticipants);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_EmailAddress = "MrsSullivan@Testimeow.com";
			staff4.GS_EmailAddress = "MrsSullivan'sCats@Testimeow.com";

			var bizo = Factory.NewWithValidTestData<DummyConversationProviderWithAdditionalParticipants>();
			bizo.AdditionalParticipants.Add(staff1);
			bizo.AdditionalParticipants.Add(staff3);
			bizo.AdditionalParticipants.Add(staff4);

			Factory.Save();

			// WI00213370
			AssertEquals("Email address should keep same after Facotry Save", "MrsSullivan'sCats@Testimeow.com", staff4.GS_EmailAddress);
			AssertEquals("Should contain 3 participants", 3, bizo.AdditionalParticipants.Count);
			AssertEquals("Last Participant should be Staff4", "MrsSullivan'sCats@Testimeow.com", bizo.AdditionalParticipants.Last().Email);

			var conversation = bizo.eConversation;
			conversation.Participants.AddNewParticipant(staff1);
			conversation.Participants.AddNewParticipant(staff2);

			var message = conversation.AddMessageFromCurrentUser("Sup", false);

			var participantsForNotifications = EConversationParticipantProvider.GetParticipantsForEConversationMessageNotifications(conversation, false, null, message.Sender);

			AssertEquals(2, participantsForNotifications.Count);
			AssertContainsExactElementsInAnyOrder("Should contain unique staff with non-empty email addresses", new[] { staff1, staff4 }, participantsForNotifications);
		}

		public void TestGetParticipants_ContactsAdditionalParticipants()
		{
			DummyBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyConversationProviderWithAdditionalParticipants);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			var staff4 = Factory.NewWithValidTestData<GlbStaff>();

			staff1.GS_EmailAddress = "MrsSullivan@Testimeow.com";
			staff4.GS_EmailAddress = "MrsSullivan'sCats@Testimeow.com";

			var org = Factory.New<IOrgHeader>();
			org.OH_Code = "~oo";

			var contact = Factory.New<IOrgContact>();
			contact.OC_ContactName = "peabody";
			contact.OC_Email = "lol@test.com";
			contact.OC_OH = org.PK;

			var contactConvo = contact as IConversationParticipant;

			var bizo = Factory.NewWithValidTestData<DummyConversationProviderWithAdditionalParticipants>();
			bizo.AdditionalParticipants.Add(staff1);
			bizo.AdditionalParticipants.Add(staff3);
			bizo.AdditionalParticipants.Add(staff4);
			bizo.AdditionalParticipants.Add(contactConvo);

			Factory.Save();

			var conversation = bizo.eConversation;
			conversation.Participants.AddNewParticipant(staff1);
			conversation.Participants.AddNewParticipant(staff2);

			var message = conversation.AddMessageFromCurrentUser("Sup", false);

			var participantsForNotifications = EConversationParticipantProvider.GetParticipantsForEConversationMessageNotifications(conversation, false, null, message.Sender);

			AssertEquals(3, participantsForNotifications.Count);
			AssertContainsExactElementsInAnyOrder("Should contain unique staff with non-empty email addresses", new[] { staff1, staff4, contactConvo }, participantsForNotifications);

			participantsForNotifications = EConversationParticipantProvider.GetParticipantsForEConversationMessageNotifications(conversation, true, null, message.Sender);

			AssertEquals(2, participantsForNotifications.Count);
			AssertContainsExactElementsInAnyOrder("Should contain unique staff with non-empty email addresses", new[] { staff1, staff4 }, participantsForNotifications);
		}
	}
}
