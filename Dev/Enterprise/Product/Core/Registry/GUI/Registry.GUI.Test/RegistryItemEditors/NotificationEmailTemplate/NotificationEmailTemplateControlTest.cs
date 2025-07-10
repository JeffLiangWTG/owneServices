using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;
using static Enterprise.Integration.DocumentWrappers;

namespace Enterprise.Registry.GUI
{
	[TestedType(typeof(NotificationEmailTemplateRegistryControl))]
	sealed class NotificationEmailTemplateControlTest : Enterprise.Registry.GUI.Testing.RegistryBusinessObjectTemplateZUserControlTest
	{
		public void TestInsertDocumentField()
		{
			NotificationEmailTemplate emailTemplate = new NotificationEmailTemplate(typeof(DummyDocSource), "test subject", "test  body");
			using (ZForm form = new ZForm(emailTemplate))
			using (NotificationEmailTemplateControl control = new NotificationEmailTemplateControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(emailTemplate, null);

				AssertEquals("test subject", control.EmailSubjectTextBox.Text);
				AssertEquals("test  body", control.EmailBodyTextBox.Text);
				AssertEquals(2, control.DocumentFieldsGrid.List.Count);

				control.EmailBodyTextBox.Select(5, 0);
				control.EmailTemplateTextBox_Leave(control.EmailBodyTextBox, EventArgs.Empty);

				AssertEquals("Cursor pos before losing focus", 5, control.cursorPosBeforeLosingFocus);
				string documentFieldToInsert = Core.Constants.DocumentEngine.EmailParsing.StartTag + "SomeProperty1" + Core.Constants.DocumentEngine.EmailParsing.EndTag;
				control.DocumentFieldsGrid.Select(0);
				control.DocumentFieldsGrid_DoubleClick(control.DocumentFieldsGrid, EventArgs.Empty);
				AssertEquals("test " + documentFieldToInsert + " body", control.EmailBodyTextBox.Text);

				control.EmailSubjectTextBox.Select(0, 0);
				control.EmailTemplateTextBox_Leave(control.EmailSubjectTextBox, EventArgs.Empty);
				AssertEquals("Cursor pos before losing focus", 0, control.cursorPosBeforeLosingFocus);
				documentFieldToInsert = Core.Constants.DocumentEngine.EmailParsing.StartTag + "SomeProperty2" + Core.Constants.DocumentEngine.EmailParsing.EndTag;
				control.DocumentFieldsGrid.ListManager.Position = 1;
				control.DocumentFieldsGrid_DoubleClick(control.DocumentFieldsGrid, EventArgs.Empty);
				AssertEquals(documentFieldToInsert + "test subject", control.EmailSubjectTextBox.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestInsertDocumentField_AfterTrailingCharacter()
		{
			NotificationEmailTemplate emailTemplate = new NotificationEmailTemplate(typeof(DummyDocSource));
			emailTemplate.EmailSubject = "test subject ";
			emailTemplate.EmailBody = "test body ";
			using (ZForm form = new ZForm(emailTemplate))
			using (NotificationEmailTemplateControl control = new NotificationEmailTemplateControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(emailTemplate, null);

				AssertEquals("test subject ", control.EmailSubjectTextBox.Text);
				AssertEquals("test body ", control.EmailBodyTextBox.Text);
				AssertEquals(2, control.DocumentFieldsGrid.List.Count);

				control.EmailBodyTextBox.Select(control.EmailBodyTextBox.TextLength, 0);
				control.EmailTemplateTextBox_Leave(control.EmailBodyTextBox, EventArgs.Empty);

				AssertEquals("Cursor pos before losing focus", control.EmailBodyTextBox.TextLength, control.cursorPosBeforeLosingFocus);
				string documentFieldToInsert = Core.Constants.DocumentEngine.EmailParsing.StartTag + "SomeProperty1" + Core.Constants.DocumentEngine.EmailParsing.EndTag;
				control.DocumentFieldsGrid.Select(0);
				control.DocumentFieldsGrid_DoubleClick(control.DocumentFieldsGrid, EventArgs.Empty);
				AssertEquals("test body " + documentFieldToInsert, control.EmailBodyTextBox.Text);

				control.EmailSubjectTextBox.Select(0, 0);
				control.EmailTemplateTextBox_Leave(control.EmailSubjectTextBox, EventArgs.Empty);
				AssertEquals("Cursor pos before losing focus", 0, control.cursorPosBeforeLosingFocus);
				documentFieldToInsert = Core.Constants.DocumentEngine.EmailParsing.StartTag + "SomeProperty2" + Core.Constants.DocumentEngine.EmailParsing.EndTag;
				control.DocumentFieldsGrid.ListManager.Position = 1;
				control.DocumentFieldsGrid_DoubleClick(control.DocumentFieldsGrid, EventArgs.Empty);
				AssertEquals(documentFieldToInsert + "test subject ", control.EmailSubjectTextBox.Text);
			}
		}

		[RequiresSTA]
		public void TestEmailBodyTextBoxVisible()
		{
			NotificationEmailTemplate emailTemplate = new NotificationEmailTemplate(typeof(IDocSalesCall), "test subject ", "test body ", true);
			using (ZForm form = new ZForm(emailTemplate))
			using (NotificationEmailTemplateControl control = new NotificationEmailTemplateControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(emailTemplate, null);
				AssertEquals("test visible ", !control.EmailBodyTextBox.Visible, emailTemplate.ShouldHideEmailBody);
			}
		}

		protected override IBusiness GetNewBusinessEntity()
		{
			return new NotificationEmailTemplate(typeof(DummyDocSource), "");
		}

		[RequiresSTA]
		public void TestPreviewButtonVisibility_Visible()
		{
			var emailTemplate = new NotificationEmailTemplate(typeof(IDocSalesCall), "test subject ", "test body ");
			using (var form = new ZForm(emailTemplate))
			using (var control = new NotificationEmailTemplateControl())
			{
				form.Controls.Add(control);
				control.emailGenerator = new Mock<IEmailGenerator>().Object;

				form.Show();
				control.SetDataBinding(emailTemplate, null);

				AssertEquals("PreviewButton must be visible when Generator is set", true, control.PreviewButton.Visible);
			}
		}

		public void TestPreviewButtonVisibility_Invisible()
		{
			var emailTemplate = new NotificationEmailTemplate(typeof(IDocSalesCall), "test subject ", "test body ");
			using (var form = new ZForm(emailTemplate))
			using (var control = new NotificationEmailTemplateControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(emailTemplate, null);

				AssertEquals("PreviewButton must be invisible when Generator is not set", false, control.PreviewButton.Visible);
			}
		}

		public void TestGenerateEmail_Exception()
		{
			var emailTemplate = new NotificationEmailTemplate(typeof(IDocSalesCall), "test subject ", "test body ");
			using (var form = new ZForm(emailTemplate))
			using (var control = new NotificationEmailTemplateControl())
			{
				form.Controls.Add(control);
				form.Show();
				control.SetDataBinding(emailTemplate, null);

				control.CurrentDataItem.DocSourceType = new Mock<IEmailGenerator>().Object as Type;

				_ = control.EmailGenerator;

				AssertEquals("Unable to get email generator", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		#region DummyDocSource

		class DummyDocSource
		{
			[DocumentField("Some Property 1")]
			public ZString SomeProperty1
			{
				get { return "123"; }
			}

			[DocumentField("Some Property 2")]
			public ZString SomeProperty2
			{
				get { return "234"; }
			}
		}

		#endregion
	}
}
