using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.Customs.BE.Business;
using Enterprise.Customs.BE.Business.Declaration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI.Testing;

class ExportBottomSectionUserControlTest : TestCaseWithFactory
{
	public void TestControls()
	{
		using (var control = new ExportBottomSectionUserControl())
		{
			AssertNotNull("SecurityDropEdit", control.FindSingleOrDefault<ZDropEdit>("SecurityDropEdit"));
		}
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
				AssertSequencesEqual("Columns", new[] { nameof(AlternativeEvidence.DocType), nameof(AlternativeEvidence.Reference) }, columnNames);

				AssertEquals("DocType", 80, alternativeEvidenceGrid.GetColumnStyle(nameof(AlternativeEvidence.DocType)).Width);
				AssertEquals("Reference", 120, alternativeEvidenceGrid.GetColumnStyle(nameof(AlternativeEvidence.Reference)).Width);
			});
		}
	}

	public void TestControls_JustificationTextBox()
	{
		using var control = new ExportBottomSectionUserControl();
		var justificationTextBox = control.FindSingle<ZTextBox>(c => c.Name == "JustificationTextBox");
		CombineAssertions(() =>
		{
			AssertType<ZTextBox>("Control Type", justificationTextBox);
			AssertEquals("BindTo", "SendingObjectsCollection.Justification", justificationTextBox.BindTo);
		});
	}

	public void TestLayout_ByEntryType()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
		declaration.CustomsEntryHeaders.AddNew();

		var parent = new ExportDeclarationMessageSendingActionParent(declaration);

		using var form = new ExportMessageSendingForm(parent);
		using var control = new ExportBottomSectionUserControl();

		form.Controls.Add(control);
		form.Show();

		var justificationTextBox = control.FindSingle<ZTextBox>(c => c.Name == "JustificationTextBox");
		var alternativeEvidenceGroupBox = control.FindSingle<ZGroupBox>(c => c.Name == "AlternativeEvidenceGroupBox");
		var messageSendingAction = parent.SendingObjectsCollection[0];
		messageSendingAction.TypeOfEntry = Enterprise.Customs.BE.Business.BEExportEntryTypeList.Codes.ExportDeclaration;

		CombineAssertions(() =>
		{
			AssertEquals($"Entry type: {messageSendingAction.TypeOfEntry} JustificationTextBox not visible", false, justificationTextBox.Visible);
			AssertEquals($"Entry type: {messageSendingAction.TypeOfEntry} Alternative evidence visible", true, alternativeEvidenceGroupBox.Visible);

			messageSendingAction.TypeOfEntry = Enterprise.Customs.BE.Business.BEExportEntryTypeList.Codes.CancellationRequest;
			AssertEquals($"Entry type: {messageSendingAction.TypeOfEntry} JustificationTextBox visible", true, justificationTextBox.Visible);
			AssertEquals($"Entry type: {messageSendingAction.TypeOfEntry} Alternative evidence not visible", false, alternativeEvidenceGroupBox.Visible);
		});
	}
}
