using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(EmailRecipientSelection))]
	public class EmailRecipientSelectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestValidateRecipientName()
		{
			EmailRecipientSelection selector = new EmailRecipientSelection(Factory);

			selector.RecipientName = "AAAA";
			AssertNoErrors(selector.RecipientNameInfo);

			selector.RecipientName = "";
			AssertHasErrors(selector.RecipientNameInfo);
		}

		public void TestValidateRecipientEmail()
		{
			EmailRecipientSelection selector = new EmailRecipientSelection(Factory);

			selector.RecipientEmail = "";
			AssertHasErrors(selector.RecipientEmailInfo);

			selector.RecipientEmail = "test@example.com";
			AssertNoErrors(selector.RecipientEmailInfo);

			selector.RecipientEmail = "hello";
			AssertHasErrors(selector.RecipientEmailInfo);
		}

		public void TestValidateSelectedRecipient()
		{
			EmailRecipientSelection selector = new EmailRecipientSelection(Factory);
			selector.Recipients.AddPair("Zubin", "zubin@example.com");

			selector.SelectedRecipientName = "Zubin";
			AssertNoErrors(selector.SelectedRecipientNameInfo);

			selector.SelectedRecipientName = "AAAAA";
			AssertHasErrors(selector.SelectedRecipientNameInfo);

			selector.SelectedRecipientName = "";
			AssertNoErrors(selector.SelectedRecipientNameInfo);
		}

		public void TestRecipientDetails()
		{
			EmailRecipientSelection selector = new EmailRecipientSelection(Factory);
			selector.Recipients.AddPair("Zubin", "zubin@example.com");
			Assert("No name", selector.RecipientName.IsEmpty);
			Assert("No Email", selector.RecipientEmail.IsEmpty);

			selector.SelectedRecipientName = "Zubin";
			AssertEquals("Correct name", "Zubin", selector.RecipientName);
			AssertEquals("Correct Email", "zubin@example.com", selector.RecipientEmail);

			selector.RecipientName = "John";
			selector.RecipientEmail = "john@example.com";
			AssertEquals("Correct name", "John", selector.RecipientName);
			AssertEquals("Correct Email", "john@example.com", selector.RecipientEmail);
		}

		public void TestAddNewRecipientIfRequired()
		{
			EmailRecipientSelection selector = new EmailRecipientSelection(Factory);
			selector.RecipientName = "John";
			selector.RecipientEmail = "john@example.com";
			AssertEquals(false, selector.Recipients.ContainsCode("John"));

			selector.AddNewRecipientIfRequired();
			AssertEquals(true, selector.Recipients.ContainsCode("John"));
			AssertEquals("john@example.com", selector.Recipients.GetDescriptionFromCode("John"));

			EmailRecipientSelection newSelector = new EmailRecipientSelection(Factory);
			AssertEquals("Master list in registry updated", true, newSelector.Recipients.ContainsCode("John"));
			AssertEquals("Master list in registry updated", "john@example.com", newSelector.Recipients.GetDescriptionFromCode("John"));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new EmailRecipientSelection(Factory);
		}
	}
}
