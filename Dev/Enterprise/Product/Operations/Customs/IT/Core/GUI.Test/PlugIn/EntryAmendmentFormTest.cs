using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.GUI.PlugIn;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(EntryAmendmentForm))]
sealed class EntryAmendmentFormTest : ZFormBasherTest
{
	public void TestFormProperties()
	{
		using (var form = new EntryAmendmentForm(handler))
		{
			CombineAssertions(() =>
			{
				AssertEquals("FormHeading", "Entry Amendment", form.FormHeading);

				AssertEquals("FormBorderStyle", FormBorderStyle.FixedDialog, form.FormBorderStyle);
			});
		}
	}

	public void TestMovementReferenceNumberTextBox()
	{
		using (var form = new EntryAmendmentForm(handler))
		{
			AssertEquals(nameof(EntryAmendmentHandler.MovementReferenceNumber), form.movementReferenceNumberTextBox.BindTo);
		}
	}

	public void TestTotalEntryLinesField()
	{
		using (var form = new EntryAmendmentForm(new EntryAmendmentHandlerForTest(Factory, true)))
		{
			form.Show();
			Assert("TotalEntryLinesCalcEdit should be visible", form.totalEntryLinesCalcEdit.Visible);
			AssertEquals(nameof(EntryAmendmentHandler.TotalEntryLines), form.totalEntryLinesCalcEdit.BindTo);
		}

		using (var form = new EntryAmendmentForm(new EntryAmendmentHandlerForTest(Factory, false)))
		{
			form.Show();
			Assert("TotalEntryLinesCalcEdit should be invisible", !form.totalEntryLinesCalcEdit.Visible);
		}
	}

	public void TestOkButton_Caption()
	{
		using (var form = new EntryAmendmentForm(handler))
		{
			AssertEquals("OK", form.okButton.CaptionResourceString.Caption);
		}
	}

	public void TestOkButton_ClickWhenBusinessEntityHasErrors()
	{
		using (var form = new EntryAmendmentForm(handler))
		{
			form.Show();

			form.okButton.PerformClick();
			CombineAssertions(() =>
			{
				AssertEquals("Visible", true, form.Visible);
				AssertEquals("DialogResult", DialogResult.None, form.DialogResult);
				AssertEquals("Last message prompted to the user", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}
	}

	public void TestOkButton_ClickWhenBusinessEntityDoesNotHaveErrors()
	{
		using (var form = new EntryAmendmentForm(handler))
		{
			form.Show();
			handler.MovementReferenceNumber = "24ITQ0B08AAB1911J3";
			handler.TotalEntryLines = 12345;

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.okButton.PerformClick();

				AssertEquals("User presses OK", "Are you sure you want to set this Entry as AMENDING?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Visible", false, form.Visible);
				AssertEquals("DialogResult", DialogResult.OK, form.DialogResult);
			});
		}
	}

	public void TestOkButton_ClickWhenBusinessEntityDoesNotHaveErrors_ConfirmationCancel()
	{
		using (var form = new EntryAmendmentForm(handler))
		{
			form.Show();
			handler.MovementReferenceNumber = "24ITQ0B08AAB1911J3";
			handler.TotalEntryLines = 12345;

			CombineAssertions(() =>
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.okButton.PerformClick();

				AssertEquals("User presses Cancel", "Are you sure you want to set this Entry as AMENDING?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Visible", true, form.Visible);
				AssertEquals("DialogResult", DialogResult.None, form.DialogResult);
			});
		}
	}

	public void TestAbortButton_Caption()
	{
		using (var form = new EntryAmendmentForm(handler))
		{
			AssertEquals("Cancel", form.abortButton.CaptionResourceString.Caption);
		}
	}

	public void TestAbortButton_Click()
	{
		using (var form = new EntryAmendmentForm(handler))
		{
			form.Show();
			handler.MovementReferenceNumber = "24ITQ0B08AAB1911J3";
			handler.TotalEntryLines = 12345;

			form.CancelButton.PerformClick();
			CombineAssertions(() =>
			{
				AssertEquals("Visible", false, form.Visible);
				AssertEquals("DialogResult", DialogResult.Cancel, form.DialogResult);
			});
		}
	}

	protected override void SetUp()
	{
		base.SetUp();

		handler = new EntryAmendmentHandler(Factory);
	}
	EntryAmendmentHandler handler;

	protected override Form GetFormToBashCore() => new EntryAmendmentForm(handler);

	internal class EntryAmendmentHandlerForTest : EntryAmendmentHandler
	{
		public EntryAmendmentHandlerForTest(BusinessObjectFactory factory, ZBool showTotalEntryLines) : base(factory)
		{
			this.showTotalEntryLines = showTotalEntryLines;
		}

		public override ZBool ShowTotalEntryLines => this.showTotalEntryLines;

		readonly ZBool showTotalEntryLines;
	}
}
