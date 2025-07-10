using System.Windows.Forms;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.Customs.AU.Declaration.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.GUI.Testing
{
	[TestedType(typeof(MessageSelectionForm))]
	sealed class MessageSelectionFormTest : ZFormBasherTest
	{
		public void TestIsOKClicked()
		{
			using (MessageSelectionForm form = new MessageSelectionForm(collection))
			{
				AssertEquals("PreCondition:There is something to send a message for", true, collection.GetMessageAttacheesToSendMessagesFor().Length > 0);
				form.Show();
				form.oKBoundButton.PerformClick();
				AssertEquals("OK button is clicked", true, form.IsOKToSend);
			}

			using (MessageSelectionForm form = new MessageSelectionForm(collection))
			{
				form.Show();
				form.cancelBoundButton.PerformClick();
				AssertEquals("Cancel button is clicked", false, form.IsOKToSend);
			}
		}

		public void TestWarnIfThereIsNothingSelected()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (MessageSelectionForm form = new MessageSelectionForm(collection))
			{
				foreach (MessageAttacheeSelection selection in collection)
				{
					selection.ShouldSendNow = false;
				}

				AssertEquals("PreCondition:There is nothing selected", 0, collection.GetMessageAttacheesToSendMessagesFor().Length);
				form.Show();
				form.oKBoundButton.PerformClick();
				AssertEquals("OK button is clicked, but there is nothing selected", false, form.IsOKToSend);
				AssertEquals("Users are warned", "There are no entries selected to send a message.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			SetUp();
			return new MessageSelectionForm(collection);
		}

		DummyMessageAttacheeHolder dummyHolder;
		DummyMessageAttachee dummyAttachee;
		MessageAttacheeSelectionCollection collection;
		protected override void SetUp()
		{
			base.SetUp();
			dummyHolder = new DummyMessageAttacheeHolder(Factory);
			dummyAttachee = new DummyMessageAttachee(Factory);
			dummyAttachee.IsValidToSendForAmendExposed = true;
			dummyHolder.MessageAttacheesExposed = new IMessageAttachee[] { dummyAttachee };
			collection = new MessageAttacheeSelectionCollection(dummyHolder, MessageAttacheeMessageType.Amend);
		}
	}
}
