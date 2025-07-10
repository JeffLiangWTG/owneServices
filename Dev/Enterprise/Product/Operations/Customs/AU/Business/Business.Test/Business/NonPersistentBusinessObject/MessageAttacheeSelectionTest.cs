using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	[TestedType(typeof(MessageAttacheeSelection))]
	sealed class MessageAttacheeSelectionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShouldSendNow()
		{
			DummyMessageAttachee messageAttachee = new DummyMessageAttachee(Factory);
			MessageAttacheeSelection selection = new MessageAttacheeSelection(messageAttachee);
			AssertEquals("Default values", true, selection.ShouldSendNow);

			selection.ShouldSendNow = false;
			AssertEquals("Default values", false, selection.ShouldSendNow);
		}

		public void TestUserFriendlyCode()
		{
			DummyMessageAttachee messageAttachee = new DummyMessageAttachee(Factory);
			messageAttachee.UserFriendlyCodeExposed = "TEST";

			MessageAttacheeSelection selection = new MessageAttacheeSelection(messageAttachee);
			AssertEquals("UserFriendlyCode", "TEST", selection.UserFriendlyCode);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MessageAttacheeSelection(new DummyMessageAttachee(Factory));
		}
	}
}
