using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.ES.GUI.Test.PlugIn.SupportingDocuments.ImportSupportingDocumentsFields
{
	sealed class ImportSupportingDocumentReferenceNumberUserControlTest : TestCaseWithFactory
	{
		public void TestReferenceNumberTextBox()
		{
			var textBox = userControl.ReferenceNumberTextBox;
			CombineAssertions(() =>
			{
				AssertEquals("ReferenceNumberTextBox: CharacterCasing", CharacterCasing.Normal, textBox.CharacterCasing);
				AssertEquals("BindingMember", nameof(SupportingDocument.CSI_ReferenceNumber), textBox.GetBindingMember());
			});
		}

		public void TestReferenceNumberCodeFindBox()
		{
			var codeFindBox = userControl.ReferenceNumberCodeFindBox;
			CombineAssertions(() =>
			{
				AssertEquals("ReferenceNumberCodeFindBox: CharacterCasing", CharacterCasing.Upper, codeFindBox.CodeBox.CharacterCasing);
				AssertEquals("BindingMember", nameof(SupportingDocument.CSI_ReferenceNumber), codeFindBox.GetBindingMember());
			});
		}

		public void TestDuplicateBindingThrowsException()
		{
			var declaration = Factory.New<JobDeclaration>();
			var invoiceLine = declaration
				.Invoices.AddNew()
				.InvoiceLines.AddNew();

			using (var form = new ZForm())
			{
				form.Controls.Add(userControl);

				userControl.SetDataBinding(declaration, "FilteredInvoiceLines.SupportingDocuments");
				AssertNoExceptionThrown(() => userControl.SetDataBinding(declaration, "FilteredInvoiceLines.SupportingDocuments"));

				form.Show();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			userControl = new ImportSupportingDocumentReferenceNumberUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			userControl.Dispose();
		}

		ImportSupportingDocumentReferenceNumberUserControl userControl;
	}
}
