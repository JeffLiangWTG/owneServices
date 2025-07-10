using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.KR.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(DocumentGeneratingActionForm))]
	sealed class DocumentGeneratingActionFormTest : ZFormBasherTest
	{
		public void TestClickOKButton()
		{
			var actionCollection = new DocumentGeneratingActionCollection(declaration);
			using (var form = new DocumentGeneratingActionForm(actionCollection))
			{
				form.Show();
				var okButton = form.FindSingle<ZButton>("OKButton");
				okButton.PerformClick();
				AssertEquals("Please select at lease one entry to be delivered.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				actionCollection[0].ToBeDelivered = true;
				okButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				actionCollection[1].ToBeDelivered = true;
				okButton.PerformClick();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestClickCancelButton()
		{
			var actionCollection = new DocumentGeneratingActionCollection(declaration);
			using (var form = new DocumentGeneratingActionForm(actionCollection))
			{
				form.Show();
				AssertEquals("Pre-condition", DialogResult.None, form.DialogResult);

				var cancelButton = form.FindSingle<ZButton>("CancelButton");
				cancelButton.PerformClick();
				AssertEquals(DialogResult.Cancel, form.DialogResult);
			}
		}
		void AssertColumnStyles(string menuName, bool isStatusColumnRemoved, bool isStatementColumnRemoved, bool isIncludingCurrentDifferenceColumnRemoved)
		{
			var actionCollection = new DocumentGeneratingActionCollection(declaration);

			IStmMenuItem menuItem = Factory.New<IStmMenuItem>();
			menuItem.SU_MenuName = menuName;
			actionCollection.InitialiseFor(menuItem);

			using (var form = new DocumentGeneratingActionForm(actionCollection))
			{
				form.Show();

				var entriesGrid = form.FindSingle<ZGrid>("EntriesGrid");
				AssertEquals(isStatusColumnRemoved, entriesGrid.GetColumnStyle(nameof(DocumentGeneratingAction.StatusDescription)).IsUnavailable);

				AssertEquals(isStatementColumnRemoved, entriesGrid.GetColumnStyle(nameof(DocumentGeneratingAction.PaymentInvoiceNumber)).IsUnavailable);
				AssertEquals(isStatementColumnRemoved, entriesGrid.GetColumnStyle(nameof(DocumentGeneratingAction.ReceivedDate)).IsUnavailable);
				AssertEquals(isIncludingCurrentDifferenceColumnRemoved, entriesGrid.GetColumnStyle(nameof(DocumentGeneratingAction.IncludingCurrentDifference)).IsUnavailable);
			}
		}

		public void TestAssertColumnStyles()
		{
			AssertColumnStyles(JobDeclarationDocumentSupporter.MenuNames.NoticeOfTaxAdjustment, true, true, true);
			AssertColumnStyles(JobDeclarationDocumentSupporter.MenuNames.CorrectionNoticeOfCountryOfOrigin, true, true, true);
			AssertColumnStyles(JobDeclarationDocumentSupporter.MenuNames.NoticeOfFinalizedRefund, true, true, true);
			AssertColumnStyles(JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate, false, true, true);
			AssertColumnStyles(JobDeclarationDocumentSupporter.MenuNames.ExportDeclarationCertificate_English, false, true, true);
			AssertColumnStyles(JobDeclarationDocumentSupporter.MenuNames.NoticeOfAmendmentOrSupplementaryActions, true, true, true);
			AssertColumnStyles(JobDeclarationDocumentSupporter.MenuNames.ImportTaxInvoiceForIndividualDeclaredCase, true, true, true);
			AssertColumnStyles(JobDeclarationDocumentSupporter.MenuNames.InvoiceofCustomsDisbursementCharges, true, false, true);
			AssertColumnStyles(JobDeclarationDocumentSupporter.MenuNames.AmendmentOfExportDeclaration, false, true, false);
		}

		protected override Form GetFormToBashCore() => new DocumentGeneratingActionForm(new DocumentGeneratingActionCollection(declaration));

		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			var entry1 = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber1 = entry1.EntryNumbers.AddNew();
			entryNumber1.CE_EntryType = "EXP";
			entryNumber1.CE_EntryNum = "A111";
			var entry2 = declaration.CustomsEntryHeaders.AddNew();
			var entryNumber2 = entry2.EntryNumbers.AddNew();
			entryNumber2.CE_EntryType = "EXP";
			entryNumber2.CE_EntryNum = "A222";
			Factory.Save();
		}
		JobDeclaration declaration;
	}
}
