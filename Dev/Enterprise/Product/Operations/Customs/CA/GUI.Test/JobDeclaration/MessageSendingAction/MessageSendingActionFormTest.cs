using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Customs.CA.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Moq;
using Moq.Protected;
using NUnit.Framework;

namespace Enterprise.Customs.CA.GUI.Testing
{
	[TestedType(typeof(MessageSendingActionForm))]
	sealed class MessageSendingActionFormTest : ZFormBasherTest
	{
		public void TestRemoveColumns()
		{
			using (MessageSendingActionForm form = CreateNewForm(MessageSendingMessageType.Original))
			{
				form.Show();
				AssertNull("CA_SaveWithoutSending should be hidden", form.EntriesGrid.Columns[CAMessageSendingAction.Schema.CA_SaveWithoutSending]);
				AssertNull("CA_SaveWithoutSendingReasonText should be hidden", form.EntriesGrid.Columns[CAMessageSendingAction.Schema.CA_SaveWithoutSendingReasonText]);
			}

			using (MessageSendingActionForm form = CreateNewForm(MessageSendingMessageType.Amendment))
			{
				form.AddColumnsForAmendmentDetection();
				form.Show();
				AssertNotNull("CA_SaveWithoutSending should be visible for Amendment", form.EntriesGrid.Columns[CAMessageSendingAction.Schema.CA_SaveWithoutSending]);
				AssertNotNull("CA_SaveWithoutSendingReasonText should be visible for Amendment", form.EntriesGrid.Columns[CAMessageSendingAction.Schema.CA_SaveWithoutSendingReasonText]);
			}
		}

		public void TestClickSendButton()
		{
			using (MessageSendingActionForm form = CreateNewForm(MessageSendingMessageType.Original))
			{
				form.Show();
				form.actions[0].CA_SendMessage = false;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.OKButton.PerformClick();
				AssertEquals(MessageSendingActionForm.YouHaveNotSelectedAnythingToSendMessagesFor, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsCancelled", true, form.actions.IsCancelled);

				form.actions[0].CA_SendMessage = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.OKButton.PerformClick();
				AssertNotEquals(MessageSendingActionForm.YouHaveNotSelectedAnythingToSendMessagesFor, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsCancelled", false, form.actions.IsCancelled);
			}
		}

		public void TestClickSendButtonWarnsAboutMessageErrors()
		{
			CAMessageSendingActionCollection collection = new CAMessageSendingActionCollection(Declaration, MessageSendingMessageType.Original);
			collection.RemoveAndDeleteAll();
			var mock = new Mock<CAMessageSendingAction>(new object[] { Entry, MessageType.DataLoadingModule, collection });
			mock.CallBase = true;
			CAMessageSendingAction action = mock.Object;
			mock.Protected().Setup("RunPreSaveValidationCore").Callback(() =>
			{
				action.AddRowMessageError("Test");
			});
			collection.Add(action);
			using (MessageSendingActionForm form = new MessageSendingActionForm(collection))
			{
				form.Show();
				action.CA_SendMessage = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				form.OKButton.PerformClick();
				AssertEquals("Message error is warned", MessageSendingActionForm.ThereIsANotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsCancelled", true, form.actions.IsCancelled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.OKButton.PerformClick();
				AssertEquals("Message error is warned", MessageSendingActionForm.ThereIsANotification, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("IsCancelled", false, form.actions.IsCancelled);
			}
			mock.VerifyAll();
		}

		public void TestClickCancelButton()
		{
			using (MessageSendingActionForm form = CreateNewForm(MessageSendingMessageType.Original))
			{
				form.Show();
				form.actions[0].CA_SendMessage = true;

				form.CancelButton.PerformClick();
				AssertEquals("IsCancelled", true, form.actions.IsCancelled);
			}
		}

		protected override Form GetFormToBashCore() => new MessageSendingActionForm(new CAMessageSendingActionCollection(Declaration, MessageSendingMessageType.Original));

		protected override IEnumerable<Form> FormsToBash
		{
			get
			{
				yield return GetFormToBash();
			}
		}

		MessageSendingActionForm CreateNewForm(MessageSendingMessageType messageType)
		{
			CusEntryHeader entryAccessed = Entry;
			CAMessageSendingActionCollection collection = new CAMessageSendingActionCollection(Declaration, messageType);
			return new MessageSendingActionForm(collection);
		}

		CusEntryHeader Entry
		{
			get
			{
				if (entry == null)
				{
					entry = Declaration.CustomsEntryHeaders.AddNew();
					entry.CH_MessageType = MessageTypeList.Codes.DataLoadingModule;
					JobComInvoiceHeader invoice = Declaration.Invoices.AddNew();
					JobComInvoiceLine invoiceLine = invoice.JobComInvoiceLines.AddNew();
					CusEntryLine entryLine = entry.MergedLines.AddNew();
					invoiceLine.JI_CL = entryLine.PK;
				}
				return entry;
			}
		}
		CusEntryHeader entry;

		JobDeclaration Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = Factory.New<JobDeclaration>();
					fDeclaration.JE_MessageType = JobMessageTypeList.Codes.Export;
				}
				return fDeclaration;
			}
		}
		JobDeclaration fDeclaration;
	}
}
