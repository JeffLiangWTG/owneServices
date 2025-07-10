using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(SadDocumentSupporterConfiguratorForm))]
sealed class SadDocumentSupporterConfiguratorFormTest : ZFormBasherTest
{
	protected override Form GetFormToBashCore() => new SadDocumentSupporterConfiguratorForm(JobDeclarationSadDocumentSupporter);

	public void TestShowErrorsDialog()
	{
		using (var form = new SadDocumentSupporterConfiguratorForm(JobDeclarationSadDocumentSupporter))
		{
			bool formClosedFlag = false;
			form.FormClosed += (sender, e) => formClosedFlag = true;
			form.Show();
			var acceptButton = form.FindSingle<ZButton>("acceptButton");

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			acceptButton.PerformClick();
			AssertEquals("Error message", "There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("DialogResult None", DialogResult.None, form.DialogResult);
			Assert("Form not closed", !formClosedFlag);

			Declaration.CustomsEntryHeaders.AddNew().CH_BGMReference = "X";
			JobDeclarationSadDocumentSupporter.BGMReferenceToPrint = "X";
			JobDeclarationSadDocumentSupporter.LayoutStyle = "6";
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			acceptButton.PerformClick();
			AssertNull("No errors", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("DialogResult OK", DialogResult.OK, form.DialogResult);
			Assert("Form closed", formClosedFlag);
		}
	}

	public void TestShowErrorsDialog_WhenBGMReferenceReadonly()
	{
		AssertBGMReferenceToPrintDropEditReadOnly(expectedReadOnly: true);
		AssertBGMReferenceToPrintDropEditReadOnly(expectedReadOnly: false);

		void AssertBGMReferenceToPrintDropEditReadOnly(bool expectedReadOnly)
		{
			using (var form = new SadDocumentSupporterConfiguratorForm(JobDeclarationSadDocumentSupporter, bgmReferenceReadOnly: expectedReadOnly))
			{
				form.Show();

				var bGMReferenceToPrintDropEdit = form.FindSingle<ZDropEdit>("bGMReferenceToPrintDropEdit");
				AssertEquals("bGMReferenceToPrintDropEdit Read Only", expectedReadOnly, bGMReferenceToPrintDropEdit.ReadOnly);
			}
		}
	}

	JobDeclarationSadDocumentSupporter JobDeclarationSadDocumentSupporter => Declaration.DocumentSupporter.SadDocumentSupporter;

	JobDeclaration Declaration
	{
		get
		{
			if (declaration == null)
			{
				declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
			}
			return declaration;
		}
	}
	JobDeclaration declaration;
}
