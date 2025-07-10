using NUnit.Framework;

namespace CargoWise.Services.Calendar.Testing
{
	sealed class ReminderRecipientCollectionTest : TestCase
	{
		public void TestAddWithBlankEmail()
		{
			ReminderRecipientCollection recipients = new ReminderRecipientCollection();

			recipients.Add("Some Name", "");
			AssertEquals("No recipients - as email was blank", 0, recipients.Count);

			recipients.Add("Some One Else", "test@example.com");
			AssertEquals("Recipient Added", 1, recipients.Count);

			recipients.Add("", "test2@example.com");
			AssertEquals("Recipient Added - name is irrelevant", 2, recipients.Count);
		}

		public void TestAddDuplicates()
		{
			ReminderRecipientCollection recipients = new ReminderRecipientCollection();

			recipients.Add("Mailing List", "aaa@email.com");
			recipients.Add("Assumingly an Individual", "aaa@email.com");
			AssertEquals("Duplicate adding of recipients with the same emails don't happen.", 1, recipients.Count);

			recipients = new ReminderRecipientCollection();
			recipients.Add("Jenny", "Jenny.Nguyen@cargowise.com");
			recipients.Add("Jenny", "Jenny.Nguyen@wisetechglobal.com");
			AssertEquals("Duplicate adding of recipients with the same names but different emails are allowed.", 2, recipients.Count);
		}
	}
}
