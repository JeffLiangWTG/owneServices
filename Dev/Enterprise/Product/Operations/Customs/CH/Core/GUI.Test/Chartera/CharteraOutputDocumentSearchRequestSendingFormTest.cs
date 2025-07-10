using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(CharteraOutputDocumentSearchRequestSendingForm))]

class CharteraOutputDocumentSearchRequestSendingFormTest : MessageSendingObjectFormTest
{
	public void TestControls() => CombineAssertions(() =>
	{
		using (var form = CreateForm())
		{
			AssertType<ZDateEdit>("CreationTimeFromDateEdit", form.CreationTimeFromDateEdit);
			AssertEquals("CreationTimeFromDateEdit.DateTimeFormat", ZDateTimePickerFormat.Long, form.CreationTimeFromDateEdit.DateTimeFormat);
			AssertEquals("CreationTimeFromDateEdit.BindTo", nameof(CharteraOutputDocumentSearchSendingObject.CreationTimeFrom), form.CreationTimeFromDateEdit.BindTo);

			AssertType<ZDateEdit>("CreationTimeToDateEdit", form.CreationTimeToDateEdit);
			AssertEquals("CreationTimeToDateEdit.DateTimeFormat", ZDateTimePickerFormat.Long, form.CreationTimeToDateEdit.DateTimeFormat);
			AssertEquals("CreationTimeToDateEdit.BindTo", nameof(CharteraOutputDocumentSearchSendingObject.CreationTimeTo), form.CreationTimeToDateEdit.BindTo);

			AssertType<ZCheckBox>("HistoryQueryCheckBox", form.HistoryQueryCheckBox);
			AssertEquals("HistoryQueryCheckBox.BindTo", nameof(CharteraOutputDocumentSearchSendingObject.IsHistoricalQuery), form.HistoryQueryCheckBox.BindTo);
		}
	});

	public void TestFormHeading()
	{
		using (var form = CreateForm())
		{
			AssertEquals("Chartera Output Documents Search Request", form.FormHeading);
		}
	}

	public void TestSendButtonShowErrors() => CombineAssertions(() =>
	{
		using (var form = CreateForm())
		{
			form.Show();
			var messageSendingObject = (CharteraOutputDocumentSearchSendingObject)form.BusinessEntity;

			messageSendingObject.CreationTimeFrom = ZDateTime.Empty;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			form.SendButton.PerformClick();
			AssertEquals("With validation errors: WasError", true, UnitTestUserNotification.Instance.LastMessage.WasError);
			AssertEquals("Show validation errors", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);

			messageSendingObject.CreationTimeFrom = ZDateTime.Now;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddOKAnswer();
			form.SendButton.PerformClick();
			AssertEquals("With no validation errors: WasError", false, UnitTestUserNotification.Instance.LastMessage.WasError);
		}
	});

	protected override Form GetFormToBashCore() => CreateForm();

	CharteraOutputDocumentSearchRequestSendingForm CreateForm()
	{
		var sendingObject = new CharteraOutputDocumentSearchSendingObject(Factory);
		return new CharteraOutputDocumentSearchRequestSendingForm(sendingObject);
	}
}
