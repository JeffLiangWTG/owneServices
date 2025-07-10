using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.EConversation.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.EConversation.Testing
{
	sealed class JobConversationParticipantValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCanSeeDuplicates()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var conversation = Factory.NewWithValidTestData<JobConversation>();

			var p1 = conversation.Participants.AddNewParticipant(staff);
			p1.Validation.ValidateAll();

			AssertNoWarning(p1.ParentKeyInfo, "This participant has already been added");

			var p2 = conversation.Participants.AddNewParticipant(staff);
			p2.Validation.ValidateAll();

			AssertHasWarning(p2.ParentKeyInfo, "This participant has already been added");
		}
		public void TestDuplicateParticipantMerger()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var p1 = conversation.Participants.AddNewParticipant(staff);
			p1.Validation.ValidateAll();
			Factory.Save();
			var p2 = conversation.Participants.AddNewParticipant(staff);
			p2.Validation.ValidateAll();
			var p3 = conversation.Participants.AddNewParticipant(staff2);
			p3.Validation.ValidateAll();
			AssertNoExceptionThrown("Should not throw any exception", () =>
			{
				try
				{
					Factory.Save();
				}
				catch (ZSaveException ex)
				{
						ZExceptionReporting.HandleSaveException(ex);
				}
			});
		}

		public void TestIgnoreCheckingDuplicateEmailIfInDatabase()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "test@test.com";

			var conversation = Factory.NewWithValidTestData<JobConversation>();

			var p1 = conversation.Participants.AddNewParticipant("test@test.com");
			var p2 = conversation.Participants.AddNewParticipant(contact);
			p1.Validation.ValidateAll();
			AssertHasError(p1.EmailAddressInfo, "This participant has already been added");

			contact.OC_Email = "test1@test.com";
			Factory.Save();

			contact.OC_Email = "test@test.com";
			Factory.Save();

			p1.Validation.ValidateAll();
			AssertNoError(p1.EmailAddressInfo, "This participant has already been added");
		}

		public void TestParentIsInactive()
		{
			const string message = "This participant is inactive";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var participant = conversation.Participants.AddNewParticipant(staff);

			staff.GS_IsActive = true;
			participant.Validation.ValidateAll();
			AssertNoWarning(participant.ParentKeyInfo, message);

			staff.GS_IsActive = false;
			participant.Validation.ValidateAll();
			AssertHasWarning(participant.ParentKeyInfo, message);
		}

		public void TestRequiresParentKey()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var conversation = Factory.NewWithValidTestData<JobConversation>();

			var p1 = conversation.Staff.AddNew();
			p1.ParentKey = null;

			AssertHasError(p1.ParentKeyInfo, ListValidation.InvalidCodeMessage.ToString());

			p1.ParentKey = staff.GS_Code;
			AssertNoError(p1.ParentKeyInfo, ListValidation.InvalidCodeMessage.ToString());
		}

		public void TestRelatedPartyDoesntHaveAnEmail()
		{
			const string warningMessage = "This Participant cannot be added as they do not have an associated email address";
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = string.Empty;

			var conversation = Factory.NewWithValidTestData<JobConversation>();
			var participant = conversation.Participants.AddNewParticipant(contact);

			participant.Validation.ValidateAll();

			AssertHasError(participant.ParentKeyInfo, warningMessage);

			contact.OC_Email = "blah@blah.com";

			participant.Validation.ValidateAll();

			AssertNoError(participant.ParentKeyInfo, warningMessage);

			Factory.Save();
			contact.OC_Email = string.Empty;
			Factory.Save();

			participant.Validation.ValidateAll();
			AssertNoError("no error if participant is in database", participant.ParentKeyInfo, warningMessage);
		}
	}
}
