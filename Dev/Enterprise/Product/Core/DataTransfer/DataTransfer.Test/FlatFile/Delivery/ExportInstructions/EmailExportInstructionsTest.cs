using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	[TestedType(typeof(EmailExportInstructions))]
	sealed class EmailExportInstructionsTest : NonPersistentBusinessObjectTestCase
	{
		public void TestAddEmailRecipient()
		{
			EmailExportInstructions emailInstructions = new EmailExportInstructions();
			AssertEquals("No recipients by default", 0, emailInstructions.Recipients.Count);

			emailInstructions.AddRecipient("test@test.com");
			AssertEquals("one recipient", 1, emailInstructions.Recipients.Count);

			emailInstructions.AddRecipient("test@test.com");
			AssertEquals("still one recipient - duplicates not added", 1, emailInstructions.Recipients.Count);

			emailInstructions.AddRecipient("testing@testing.com");
			AssertEquals("two recipients", 2, emailInstructions.Recipients.Count);
		}

		public void TestEmailRecipients()
		{
			EmailExportInstructions emailInstructions = new EmailExportInstructions();
			AssertNotNull("Email recipients can't be null on new object", emailInstructions.Recipients);
			AssertEquals("No recipients by default", 0, emailInstructions.Recipients.Count);
		}

		public void TestUserEnteredRecipients()
		{
			EmailExportInstructions emailInstructions = new EmailExportInstructions();
			AssertEquals("User entered recipients empty by default", true, emailInstructions.UserEnteredRecipients.IsEmpty);

			emailInstructions.RunPreSaveValidation();
			AssertEquals("Empty UserEnteredRecipients - should have error", true, emailInstructions.UserEnteredRecipientsInfo.HasErrors());

			emailInstructions.UserEnteredRecipients = "testing";
			AssertEquals("should have same value assigned", "testing", emailInstructions.UserEnteredRecipients);
			AssertEquals("Invalid UserEnteredRecipients - should have error", true, emailInstructions.UserEnteredRecipientsInfo.HasErrors());

			emailInstructions.UserEnteredRecipients = "test@test.test";
			AssertEquals("Valid UserEnteredRecipients - should not have error", false, emailInstructions.UserEnteredRecipientsInfo.HasErrors());

			emailInstructions.UserEnteredRecipients = "test@test.test; test2";
			AssertEquals("Second email is invalid in UserEnteredRecipients - should have error", true, emailInstructions.UserEnteredRecipientsInfo.HasErrors());

			emailInstructions.UserEnteredRecipients = "test@test.test; test2@test.test";
			AssertEquals("Valid UserEnteredRecipients - should not have error", false, emailInstructions.UserEnteredRecipientsInfo.HasErrors());
		}

		public void TestSplitUserEnteredRecipientsIntoSeparateAddresses()
		{
			EmailExportInstructions emailInstructions = new EmailExportInstructions();
			AssertEquals("pre: no recipients", 0, emailInstructions.Recipients.Count);
			emailInstructions.UserEnteredRecipients = "test@test.com; testing@testing.com; abc@test.com";

			emailInstructions.SplitUserEnteredRecipientsIntoSeparateAddresses();
			AssertEquals("Should now have three recipients", 3, emailInstructions.Recipients.Count);

			emailInstructions.Recipients.Clear();
			emailInstructions.UserEnteredRecipients = "test@test.com";
			emailInstructions.SplitUserEnteredRecipientsIntoSeparateAddresses();
			AssertEquals("Should now have one recipient", 1, emailInstructions.Recipients.Count);
		}

		public void TestSplitUserEnteredRecipientsIntoSeparateAddressesWithEmptyString()
		{
			EmailExportInstructions emailInstructions = new EmailExportInstructions();
			AssertEquals("pre: no recipients", 0, emailInstructions.Recipients.Count);
			emailInstructions.UserEnteredRecipients = ZString.Empty;
			emailInstructions.SplitUserEnteredRecipientsIntoSeparateAddresses();
			AssertEquals("Should have no recipients", 0, emailInstructions.Recipients.Count);
		}

		public void TestSubject()
		{
			EmailExportInstructions instructions = new EmailExportInstructions();
			AssertEquals("Subject empty by default", ZString.Empty, instructions.Subject);

			string subject = "This is a subject line";
			instructions.Subject = subject;
			AssertEquals("Subject line set", subject, instructions.Subject);
		}

		public void TestBody()
		{
			EmailExportInstructions instructions = new EmailExportInstructions();
			AssertEquals("Body empty by default", ZString.Empty, instructions.Body);

			string body = "This is a body. " + System.Environment.NewLine
				+ "There are multiple lines in this string." + System.Environment.NewLine
				+ "Just like a normal email body might have.";

			instructions.Body = body;
			AssertEquals("Body set", body, instructions.Body);
		}
	}
}
