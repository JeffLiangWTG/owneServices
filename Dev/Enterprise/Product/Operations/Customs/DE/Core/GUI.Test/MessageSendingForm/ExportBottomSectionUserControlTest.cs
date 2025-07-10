using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.DE.Business;
using Enterprise.Customs.DE.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.DE.GUI.Testing
{
	sealed class ExportBottomSectionUserControlTest : TestCaseWithFactory
	{
		public void TestControls()
		{
			CombineAssertions(() =>
			{
				using (var control = new ExportBottomSectionUserControl())
				{
					AssertNotNull("SecurityDropEdit", control.FindSingleOrDefault<ZDropEdit>("SecurityDropEdit"));

					var topPanel = control.TopPanel;
					var additionalWarningUserControl = control.AdditionalWarningUserControl;
					AssertType<MessageSendingFormBottomSectionUserControl>("AdditionalWarningUserControlType", additionalWarningUserControl);
					Assert("Contains AdditionalWarningUserControl", topPanel.Contains(additionalWarningUserControl));
					Assert("Contains AnnotationAndExitInfoPanel", topPanel.Contains(control.AnnotationAndExitInfoPanel));
				}
			});
		}

		public void TestControls_AlternativeEvidenceGroupBox()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.AddNew();

			var parent = new ExportDeclarationMessageSendingActionParent(declaration);
			using (var form = new ExportMessageSendingForm(parent))
			using (var control = new ExportBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var alternativeEvidenceGroupBox = control.FindSingle<ZGroupBox>(c => c.Name == "AlternativeEvidenceGroupBox");
					var alternativeEvidenceGrid = alternativeEvidenceGroupBox.FindSingle<ZGrid>(c => c.Name == "AlternativeEvidenceGrid");

					var columnNames = alternativeEvidenceGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					AssertSequencesEqual("Columns", new[] { nameof(AlternativeEvidence.EvidenceType), nameof(AlternativeEvidence.DocType), nameof(AlternativeEvidence.Reference) }, columnNames);

					AssertEquals("Type", 40, alternativeEvidenceGrid.GetColumnStyle(nameof(AlternativeEvidence.EvidenceType)).Width);
					AssertEquals("DocType", 80, alternativeEvidenceGrid.GetColumnStyle(nameof(AlternativeEvidence.DocType)).Width);
					AssertEquals("Reference", 120, alternativeEvidenceGrid.GetColumnStyle(nameof(AlternativeEvidence.Reference)).Width);
				});
			}
		}

		public void TestEntryTypeChangeResultsInAnnotationGroupBoxCaption()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();

			var parent = new ExportDeclarationMessageSendingActionParent(declaration);
			using (var form = new ExportMessageSendingForm(parent))
			{
				form.Show();
				var annotationGroupBox = form.Controls.Find("AnnotationGroupBox", true)[0];

				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];
				grid.ListManager.Position = 0;
				Application.DoEvents();

				var action = (ExportEntryMessageSendingAction)grid.ListManager.Current;
				action.EntryType = ExportEntryTypeList.Codes.CancellationRequest;
				AssertEquals("Reason for Cancellation", annotationGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);

				grid.ListManager.Position = 1;
				Application.DoEvents();
				var action2 = (ExportEntryMessageSendingAction)grid.ListManager.Current;
				action2.EntryType = ExportEntryTypeList.Codes.ExportAmendment;
				AssertEquals("Annotation", annotationGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);

				grid.ListManager.Position = 0;
				Application.DoEvents();
				AssertEquals("Reason for Cancellation", annotationGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);

				grid.ListManager.Position = 1;
				Application.DoEvents();
				AssertEquals("Annotation", annotationGroupBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestEntryTypeChangeResultsInControlVisibilitySetting()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();

			var parent = new ExportDeclarationMessageSendingActionParent(declaration);
			using (var form = new ExportMessageSendingForm(parent))
			{
				form.Show();
				var notSubmitConsigneeCheckBox = form.Controls.Find("NotSubmitConsigneeCheckBox", true)[0];
				var movementReferenceNumberTextBox = form.Controls.Find("MovementReferenceNumberTextBox", true)[0];

				var grid = (ZGrid)form.Controls.Find("MessageSendingObjectsGrid", true)[0];

				CombineAssertions(() =>
				{
					grid.ListManager.Position = 0;
					Application.DoEvents();
					var action = (ExportEntryMessageSendingAction)grid.ListManager.Current;
					action.EntryType = ExportEntryTypeList.Codes.SupplementaryExportDeclaration;
					AssertEquals($"EntryType={action.EntryType}: {nameof(notSubmitConsigneeCheckBox)}.Visible", true, notSubmitConsigneeCheckBox.Visible);
					AssertEquals($"EntryType={action.EntryType}: {nameof(movementReferenceNumberTextBox)}.Visible", true, movementReferenceNumberTextBox.Visible);

					grid.ListManager.Position = 1;
					Application.DoEvents();
					var action2 = (ExportEntryMessageSendingAction)grid.ListManager.Current;
					action2.EntryType = ExportEntryTypeList.Codes.ExportAmendment;
					AssertEquals($"EntryType={action2.EntryType}: {nameof(notSubmitConsigneeCheckBox)}.Visible", false, notSubmitConsigneeCheckBox.Visible);
					AssertEquals($"EntryType={action2.EntryType}: {nameof(movementReferenceNumberTextBox)}.Visible", false, movementReferenceNumberTextBox.Visible);

					grid.ListManager.Position = 0;
					Application.DoEvents();
					AssertEquals($"EntryType={action.EntryType} (after row selection): {nameof(notSubmitConsigneeCheckBox)}.Visible", true, notSubmitConsigneeCheckBox.Visible);
					AssertEquals($"EntryType={action.EntryType} (after row selection): {nameof(movementReferenceNumberTextBox)}.Visible", true, movementReferenceNumberTextBox.Visible);

					grid.ListManager.Position = 1;
					Application.DoEvents();
					AssertEquals($"EntryType={action2.EntryType} (after row selection): {nameof(notSubmitConsigneeCheckBox)}.Visible", false, notSubmitConsigneeCheckBox.Visible);
					AssertEquals($"EntryType={action2.EntryType} (after row selection): {nameof(movementReferenceNumberTextBox)}.Visible", false, movementReferenceNumberTextBox.Visible);
				});
			}
		}

		public void TestEntryLinesGridVisibility()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.AddNew();

			var parent = new ExportDeclarationMessageSendingActionParent(declaration);
			using (var form = new ExportMessageSendingForm(parent))
			using (var control = new ExportBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var messageSendingObjectsGrid = form.FindSingle<ZGrid>("MessageSendingObjectsGrid");
				var entryLinesGroupBox = control.FindSingle<ZGroupBox>("EntryLinesGroupBox");

				CombineAssertions(() =>
				{
					var action = (ExportEntryMessageSendingAction)messageSendingObjectsGrid.ListManager.Current;
					AssertEquals("entryLinesGroupBox is hidden by default", false, entryLinesGroupBox.Visible);

					action.EntryType = ExportEntryTypeList.Codes.ExportAmendment;
					AssertEquals($"EntryType={action.EntryType}", true, entryLinesGroupBox.Visible);

					action.EntryType = ExportEntryTypeList.Codes.ExportDeclaration;
					AssertEquals($"EntryType={action.EntryType}", false, entryLinesGroupBox.Visible);

					action.EntryType = ExportEntryTypeList.Codes.SupplementaryExportDeclaration;
					AssertEquals($"EntryType={action.EntryType}", true, entryLinesGroupBox.Visible);
				});
			}
		}

		public void TestEntryLinesGridColumns()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.CustomsEntryHeaders.AddNew();

			var parent = new ExportDeclarationMessageSendingActionParent(declaration);
			using (var form = new ExportMessageSendingForm(parent))
			using (var control = new ExportBottomSectionUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				CombineAssertions(() =>
				{
					var entryLinesGroupBox = control.FindSingle<ZGroupBox>("EntryLinesGroupBox");
					var entryLinesGrid = entryLinesGroupBox.FindSingle<ZGrid>("EntryLinesGrid");

					var columnNames = entryLinesGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => x.ColumnName).ToArray();
					AssertSequencesEqual("Columns", new[] { nameof(ExportEntryLine.ShouldSend), nameof(ExportEntryLine.LineNumber), nameof(ExportEntryLine.Tariff), nameof(ExportEntryLine.Description) }, columnNames);
					AssertEquals("ShouldSend Width", 52, entryLinesGrid.GetColumnStyle(nameof(ExportEntryLine.ShouldSend)).Width);
					AssertEquals("LineNumber Width", 95, entryLinesGrid.GetColumnStyle(nameof(ExportEntryLine.LineNumber)).Width);
					AssertEquals("Tariff Width", 80, entryLinesGrid.GetColumnStyle(nameof(ExportEntryLine.Tariff)).Width);
					AssertEquals("Description Width", 150, entryLinesGrid.GetColumnStyle(nameof(ExportEntryLine.Description)).Width);
				});
			}
		}
	}
}
