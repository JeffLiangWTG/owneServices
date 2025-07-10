using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.IL.Manifest.Business;
using Enterprise.Customs.Universal.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.IL.Manifest.GUI.Testing
{
	[TestedType(typeof(SupportingDocumentsSelectionDialog))]
	sealed class SupportingDocumentsSelectionDialogTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestCaption()
		{
			using var form = GetFormToBash();
			AssertEquals("Caption should be as expected", "Send Supporting Documents", form.Text);
		}

		public void TestSupportingDocumentsGrid()
		{
			using var form = GetFormToBash();

			form.Show();

			var grid = form.FindSingle<ZGrid>("ItemsGrid");
			AssertEquals("Should be NoRemovePossible.", RemoveAction.NoRemovePossible, grid.RemoveAction);

			var expectedColumns = new[]
			{
					nameof(SupportingDocumentWrapper.IsSelected),
					nameof(SupportingDocumentWrapper.Type),
					nameof(SupportingDocumentWrapper.Description),
					nameof(SupportingDocumentWrapper.Status),
					nameof(SupportingDocumentWrapper.CustomsNotes),
					nameof(SupportingDocumentWrapper.EDoc),
				};

			var actualColumns = grid.Columns.Select(c => c.ColumnName).ToArray();
			AssertArrayEqualsByElements(expectedColumns, actualColumns);
		}

		public void TestSelectAndDeselectAllButtons()
		{
			using var dialog = (SupportingDocumentsSelectionDialog)GetFormToBash();

			var manifestHeaderWrapper = (AsycudaManifestHeaderWrapper)dialog.DataSource;

			var items = manifestHeaderWrapper.SupportingDocuments.Cast<SupportingDocumentWrapper>();
			AssertEquals("Precondition", 2, items.Count());

			Assert("Precondition", items.All(c => !c.IsSelected));

			dialog.Show();

			var selectAllButton = dialog.FindSingle<ZButton>("SelectAllButton");
			selectAllButton.PerformClick();

			Assert("Should select all items", items.All(c => c.IsSelected));

			var deselectAllButton = dialog.FindSingle<ZButton>("DeselectAllButton");
			deselectAllButton.PerformClick();

			Assert("Should deselect all items", items.All(c => !c.IsSelected));
		}

		public void TestRunPreSendValidation_WhenNoSupportingDocumentSelected()
		{
			using var dialog = (SupportingDocumentsSelectionDialog)GetFormToBash();
			var manifestHeaderWrapper = (AsycudaManifestHeaderWrapper)dialog.DataSource;

			dialog.Show();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var sendButton = dialog.FindSingle<ZButton>("SendButton");
			sendButton.PerformClick();

			var lastMessageText = UnitTestUserNotification.Instance.LastMessage.Text;

			CombineAssertions(() =>
			{
				AssertContains("There are errors - can't save.", lastMessageText);
				AssertNotContains("A meta data value is mandatory for this type of supporting document.", lastMessageText);
			});
		}

		public void TestRunPreSendValidation_WhenSelectNotValidSupportingDocument()
		{
			using var dialog = (SupportingDocumentsSelectionDialog)GetFormToBash();
			var manifestHeaderWrapper = (AsycudaManifestHeaderWrapper)dialog.DataSource;
			var supportingDocumentWrapper = manifestHeaderWrapper.SupportingDocuments.Cast<SupportingDocumentWrapper>().First();
			supportingDocumentWrapper.IsSelected = true;
			var supportingDocument = supportingDocumentWrapper.SupportingDocument;
			var docManagerInfo = header.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Hello world"), "Invoice1.pdf", "CIV");
			supportingDocument.EDoc = eDoc1.UniqueKey;

			dialog.Show();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var sendButton = dialog.FindSingle<ZButton>("SendButton");
			sendButton.PerformClick();

			var lastMessageText = UnitTestUserNotification.Instance.LastMessage.Text;

			CombineAssertions(() =>
			{
				AssertContains("A meta data value is mandatory for this type of supporting document.", lastMessageText);
			});
		}

		public void TestRunPreSendValidation_WhenSelectValidSupportingDocument()
		{
			using var dialog = (SupportingDocumentsSelectionDialog)GetFormToBash();
			var manifestHeaderWrapper = (AsycudaManifestHeaderWrapper)dialog.DataSource;
			var supportingDocumentWrapper = manifestHeaderWrapper.SupportingDocuments.Cast<SupportingDocumentWrapper>().First();
			var supportingDocument = supportingDocumentWrapper.SupportingDocument;
			var supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems.Cast<SupportingDocumentMetaData>().First();
			supportingDocumentMetaData.CY_Data = "Val1";
			supportingDocumentWrapper.IsSelected = true;

			dialog.Show();

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			var sendButton = dialog.FindSingle<ZButton>("SendButton");
			sendButton.PerformClick();

			var lastMessageText = UnitTestUserNotification.Instance.LastMessage.Text;

			CombineAssertions(() =>
			{
				AssertNotContains("A meta data value is mandatory for this type of supporting document.", lastMessageText);
			});
		}

		public void TestSendButtonShowErrorsDialogWhenEDocIsMissing()
		{
			using var dialog = (SupportingDocumentsSelectionDialog)GetFormToBash();
			var manifestHeaderWrapper = (AsycudaManifestHeaderWrapper)dialog.DataSource;
			var supportingDocumentWrapper = manifestHeaderWrapper.SupportingDocuments.Cast<SupportingDocumentWrapper>().First();
			var supportingDocument = supportingDocumentWrapper.SupportingDocument;
			var supportingDocumentMetaData = supportingDocument.SupportingDocumentMetadataItems.Cast<SupportingDocumentMetaData>().First();
			supportingDocumentMetaData.CY_Data = "Val1";
			supportingDocumentWrapper.IsSelected = true;
			var docManagerInfo = header.DocManagerInfo;
			var eDoc1 = docManagerInfo.AddFileOrDocument(Encoding.UTF8.GetBytes("Hello world"), "Invoice1.pdf", "CIV");

			dialog.Show();
			dialog.SendButton.PerformClick();
			AssertEquals(true, supportingDocumentWrapper.HasErrors());
			AssertEquals(DialogResult.None, dialog.DialogResult);
			supportingDocument.EDoc = eDoc1.UniqueKey;
			dialog.SendButton.PerformClick();
			AssertEquals(DialogResult.OK, dialog.DialogResult);
		}

		public void TestMinimumSize()
		{
			using var form = GetFormToBash();
			AssertEquals(new Size(900, 427), form.MinimumSize);
		}

		protected override Form GetFormToBashCore()
		{
			var factory = Factory;
			var helper = new UniversalReferenceTestDataHelper(factory);
			var codeTypeILDOC = helper.CreateCusCodeType("ILDOC", "IL Document Types");
			var code380 = helper.CreateCusCodeList(Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType, "380", ZDateTime.BrettsBirthday, ZDateTime.Today);

			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("14", "Importer VAT", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("18", "Exporter Name", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);
			helper.CreateNewOrGetExistingRefCusCodeListAttributeName("MetaDatasMandatory", "Metadatas Mandatory", codeTypeILDOC.ZZK_CodeType, Core.Constants.CountryCodes.Israel, codeTypeILDOC.ZZK_CodeType);

			helper.CreateCusCodeListAttribute(code380.PK, "14", "", false);
			helper.CreateCusCodeListAttribute(code380.PK, "18", "", false);
			helper.CreateCusCodeListAttribute(code380.PK, "MetaDatasMandatory", "18", false);
			factory.Save();

			header = factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();

			var supportingDocument1 = bill.SupportingDocuments.AddNew();
			supportingDocument1.CSI_Code = "380";
			supportingDocument1.SupportingDocumentMetadataItems.RemoveAndDeleteAll();
			var supportingDocument1MetaData = supportingDocument1.SupportingDocumentMetadataItems.AddNew();
			supportingDocument1MetaData.CY_Code = "18";

			var supportingDocument2 = bill.SupportingDocuments.AddNew();
			supportingDocument2.CSI_Code = "380";
			supportingDocument2.SupportingDocumentMetadataItems.RemoveAndDeleteAll();
			var supportingDocument2MetaData = supportingDocument2.SupportingDocumentMetadataItems.AddNew();
			supportingDocument2MetaData.CY_Code = "18";

			var wrapper = new AsycudaManifestHeaderWrapper(header);
			return new SupportingDocumentsSelectionDialog(wrapper);
		}

		AsycudaManifestHeader header;
	}
}
