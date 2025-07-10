using System.Linq;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.Customs.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.CH.GUI.Testing;

[TestedType(typeof(EComplaintMessageSendingForm))]
internal class EComplaintMessageSendingFormTest : MessageSendingObjectFormTest
{
	protected override Form GetFormToBashCore()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		Factory.Save();
		return new EComplaintMessageSendingForm(new EComplaintMessageSendingObject(entryHeader));
	}

	public void TestFormHeading()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		entryHeader.MovementReferenceNumberSetter("MRN1234");

		using (var form = new EComplaintMessageSendingForm(new EComplaintMessageSendingObject(entryHeader)))
		{
			AssertEquals("Send eCom - Entry: MRN1234", form.FormHeading);
		}
	}

	public void TestControls()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		using (var form = new EComplaintMessageSendingForm(new EComplaintMessageSendingObject(entryHeader)))
		{
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("CorrectionReasonDropEdit", form.CorrectionReasonDropEdit);
				AssertType<ZCheckBox>("AttachedDeclarationCheckBox", form.AttachedDeclarationCheckBox);
				AssertType<ZDropEdit>("LocationDropEdit", form.LocationDropEdit);
				AssertType<ZGuidDropEdit>("EntryLineDropEdit", form.EntryLineDropEdit);
				AssertType<ZDropEdit>("FieldNameDropEdit", form.FieldNameDropEdit);
				AssertType<ZTextBox>("RemarkTextBox", form.RemarkTextBox);
				AssertEquals("RemarkTextBox.Multiline", true, form.RemarkTextBox.Multiline);
			});
		}
	}

	public void TestGridColums()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		using (var form = new EComplaintMessageSendingFormForTesting(new EComplaintMessageSendingObject(entryHeader)))
		{
			CombineAssertions(() =>
			{
				var columnStyles = form.MessageSendingObjectsGridExposed.ColumnStyles.Cast<ZGridColumnInfo>().ToArray();
				UserControlTestHelper.AssertColumnStyles(columnStyles, EComplaintMessageSendingObjectLine.Schema.Location, 0, typeof(ZDropEditColumnStyleInfo));
				UserControlTestHelper.AssertColumnStyles(columnStyles, EComplaintMessageSendingObjectLine.Schema.EntryLinePK, 1, typeof(ZGuidDropEditColumnStyleInfo));
				UserControlTestHelper.AssertColumnStyles(columnStyles, EComplaintMessageSendingObjectLine.Schema.FieldName, 2, typeof(ZDropEditColumnStyleInfo));
				UserControlTestHelper.AssertColumnStyles(columnStyles, EComplaintMessageSendingObjectLine.Schema.Remark, 3, typeof(ZTextBoxColumnStyleInfo));
			});
		}
	}

	public void TestSendButtonShowErrors()
	{
		RefCusCodeTestHelper.CreateCorrectionReasonTypeList(Factory);
		RefCusCodeTestHelper.CreateEComplaintFieldNames(Factory);

		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();

		using (var form = new EComplaintMessageSendingFormForTesting(new EComplaintMessageSendingObject(entryHeader)))
		{
			form.Show();
			var messageSendingObject = (EComplaintMessageSendingObject)form.BusinessEntity;
			messageSendingObject.CorrectionReason = RefCusCodeTestHelper.ValidCorrectionReasonTypeCode;
			var line = messageSendingObject.SendingObjectLines.AddNew();
			line.Location = EComplaintLocationList.Codes.Header;
			CombineAssertions(() =>
			{
				line.FieldName = RefCusCodeTestHelper.InvalidEComplaintField;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.SendButton.PerformClick();
				AssertEquals("With validation errors: WasError", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Show validation errors", "Please fix these errors before sending any messages:\r\n\r\nField Name: Enter a valid Field Name.", UnitTestUserNotification.Instance.LastMessage.Text);

				line.FieldName = RefCusCodeTestHelper.ValidEComplaintHeaderField;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddOKAnswer();
				form.SendButton.PerformClick();
				AssertEquals("With no validation errors: WasError", false, UnitTestUserNotification.Instance.LastMessage.WasError);
			});
		}
	}

	class EComplaintMessageSendingFormForTesting : EComplaintMessageSendingForm
	{
		internal EComplaintMessageSendingFormForTesting(EComplaintMessageSendingObject cusEntryHeaderWrapper) : base(cusEntryHeaderWrapper)
		{
		}

		internal ZGrid MessageSendingObjectsGridExposed => LinesGrid;
	}
}
