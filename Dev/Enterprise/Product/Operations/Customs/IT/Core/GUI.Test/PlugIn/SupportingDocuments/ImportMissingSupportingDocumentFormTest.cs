using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Declaration.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.IT.GUI.Testing;

[TestedType(typeof(ImportMissingSupportingDocumentForm))]
sealed class ImportMissingSupportingDocumentFormTest : ZFormBasherTest
{
	public void TestInitializeMissingSupportingDocumentsLayout()
	{
		using (var importMissingSupportingDocumentForm = new ImportMissingSupportingDocumentForm(MissingSupportingDocumentParent))
		{
			importMissingSupportingDocumentForm.Show();
			var titleLabel = importMissingSupportingDocumentForm.Controls.Find("titleLabel", true).Single() as ZLabel;
			var mainPanel = importMissingSupportingDocumentForm.Controls.Find("mainPanel", true).Single() as ZPanel;

			AssertEquals("Title label text", "Select the documents", titleLabel.Text);
			AssertGreaterThan("More than one groupbox is expected", mainPanel.Controls.OfType<ZGroupBox>().Count(), 0);
		}

		InvoiceLine.JI_Tariff = "";
		var emptyMissingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(InvoiceLine);
		using (var importMissingSupportingDocumentForm = new ImportMissingSupportingDocumentForm(emptyMissingSupportingDocumentParent))
		{
			importMissingSupportingDocumentForm.Show();
			var controls = importMissingSupportingDocumentForm.Controls;
			var titleLabel = controls.Find("titleLabel", true).Single() as ZLabel;
			var mainPanel = controls.Find("mainPanel", true).Single() as ZPanel;
			var importButton = controls.Find("importButton", true).Single() as ZButton;
			var setIntoAllMergableInvoiceLinesCheckBox = controls.Find("setIntoAllMergableInvoiceLinesCheckBox", true).Single() as ZCheckBox;

			AssertEquals("Title label text", "No missing supporting documents to select", titleLabel.Text);
			AssertEquals("No groupbox expected", 0, mainPanel.Controls.OfType<ZGroupBox>().Count());
			AssertEquals("Import button Enabled", false, importButton.Enabled);
			AssertEquals("Set Into All Mergable Invoice Lines CheckBox Visible", false, setIntoAllMergableInvoiceLinesCheckBox.Visible);
		}
	}

	protected override Form GetFormToBashCore() => new ImportMissingSupportingDocumentForm(MissingSupportingDocumentParent);

	MissingSupportingDocumentParent MissingSupportingDocumentParent
	{
		get
		{
			if (missingSupportingDocumentParent == null)
			{
				MissingSupportingDocumentParentTest.SetupRefCusConditionValueForMissingSupportingDocumentImportTest(Factory);
				Factory.Save();

				missingSupportingDocumentParent = MissingSupportingDocumentParent.LoadNew(InvoiceLine);
			}
			return missingSupportingDocumentParent;
		}
	}
	MissingSupportingDocumentParent missingSupportingDocumentParent;

	JobComInvoiceLine InvoiceLine
	{
		get
		{
			if (invoiceLine == null)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = "IMP";
				invoiceLine = declaration.Invoices.AddNew().InvoiceLines.AddNew();

				invoiceLine.JI_Tariff = "800900";
			}
			return invoiceLine;
		}
	}
	JobComInvoiceLine invoiceLine;
}
