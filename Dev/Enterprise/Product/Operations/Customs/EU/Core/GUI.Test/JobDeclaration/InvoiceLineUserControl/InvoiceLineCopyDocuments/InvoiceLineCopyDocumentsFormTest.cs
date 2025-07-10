using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.EU.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.EU.GUI.Testing
{
	[TestedType(typeof(InvoiceLineCopyDocumentsForm))]
	sealed class InvoiceLineCopyDocumentsFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new InvoiceLineCopyDocumentsForm(header);
		}

		public void TestFormHeading()
		{
			AssertEquals("Copy Documents of Invoice Line", form.FormHeading);
		}

		public void TestMinimumSize()
		{
			AssertEquals(CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1200, 725), form.MinimumSize);
		}

		public void TestSelectAllButton()
		{
			form.Show();

			form.SelectAllButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Select All", form.SelectAllButton.CaptionResourceString.Caption);
				AssertEquals("DialogResult", DialogResult.None, form.SelectAllButton.DialogResult);
				AssertEquals("Form is not closed after clicking button", expected: true, form.Visible);
				AssertEquals("Lines selected", 1, header.Lines.Cast<CopyDocumentsSelectionLine>().Count(e => e.IsSelected));
			});
		}

		[RequiresSTA]
		public void TestCancelButton()
		{
			form.Show();

			form.CloseButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "Cancel", form.CloseButton.CaptionResourceString.Caption);
				AssertEquals("DialogResult", DialogResult.Cancel, form.CloseButton.DialogResult);
				AssertEquals("Form is closed after clicking button", expected: false, form.Visible);
			});
		}

		public void TestOkButton()
		{
			header.SelectAll();
			form.Show();

			form.OkButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Caption", "OK", form.OkButton.CaptionResourceString.Caption);
				AssertEquals("DialogResult", DialogResult.OK, form.OkButton.DialogResult);
				AssertEquals("Form is closed after clicking button", expected: false, form.Visible);
				AssertEquals(1, otherInvoiceLine.SupportingDocuments.Count);
			});
		}

		[RequiresSTA]
		public void TestOkButtonError()
		{
			header.SelectAll();
			form.Show();
			header.InvoiceNumber = string.Empty;

			form.OkButton.PerformClick();

			CombineAssertions(() =>
			{
				AssertEquals("Form is not closed after clicking button when errors", expected: true, form.Visible);
				AssertEquals("The form has errors. Please fix them before continuing.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestCopyDocumentsLinesGridGroupBox()
		{
			CombineAssertions(() =>
			{
				AssertEquals(expected: true, form.CopyDocumentsLinesGridGroupBox.Controls.Contains(form.CopyDocumentsLinesGridControl));
				AssertEquals(DockStyle.Fill, form.CopyDocumentsLinesGridGroupBox.Dock);
				AssertEquals("Copy documents:", form.CopyDocumentsLinesGridGroupBox.CaptionResourceString.Caption);
			});
		}

		public void TestCopyDocumentsLinesGridControl()
		{
			CombineAssertions(() =>
			{
				AssertEquals(DockStyle.Fill, form.CopyDocumentsLinesGridControl.Dock);
				AssertEquals("Lines", form.CopyDocumentsLinesGridControl.BindTo);
			});
		}

		public void TestSplitContainer()
		{
			CombineAssertions(() =>
			{
				AssertEquals(900, form.SplitContainer.SplitterDistance);
				AssertEquals("Splitter is fixed", expected: true, form.SplitContainer.IsSplitterFixed);
				AssertEquals(FixedPanel.Panel2, form.SplitContainer.FixedPanel);
				AssertEquals(Orientation.Vertical, form.SplitContainer.Orientation);
				AssertEquals("Panel2 contains DynamicLayoutPanel", expected: true, form.SplitContainer.Panel2.Controls.Contains(form.DynamicLayoutPanel));
				AssertEquals("Panel1 contains Grid", expected: true, form.SplitContainer.Panel1.Controls.Contains(form.CopyDocumentsLinesGridGroupBox));
			});
		}

		public void TestDynamicLayoutPanel()
		{
			form.Show();

			CombineAssertions(() =>
			{
				DynamicLayoutPanelTest.AssertControlsOrder(form.DynamicLayoutPanel, nameof(InvoiceLineCopyDocumentControlBag.Instance.InvoiceNumberDropEditGroupBox));
				AssertEquals(DockStyle.Fill, form.DynamicLayoutPanel.Dock);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			invoiceLine.SupportingDocuments.AddNew();
			otherInvoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();
			header = new CopyDocumentsSelectionHeader(invoiceLine);
			form = new InvoiceLineCopyDocumentsForm(header);
		}

		protected override void TearDown()
		{
			base.TearDown();
			form.Dispose();
		}

		JobComInvoiceLine invoiceLine;
		JobComInvoiceLine otherInvoiceLine;
		JobDeclaration declaration;
		CopyDocumentsSelectionHeader header;
		InvoiceLineCopyDocumentsForm form;
	}
}
