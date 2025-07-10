using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GB.GUI.JobDeclarationForms.Testing
{
	internal class JobDeclarationInvoiceHeadersTest : TestCaseWithFactory
	{
		public void TestJobDeclarationExportInvoiceHeadersCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ApplicationCode = "CHF";
			declaration.JE_MessageType = "EXP";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = form.FindSingle<LazyLoadedTabControl>("MainTabControl");
				var page = control.FindSingle<ZTabPage>(c => c.Name.Contains("InvoicesTabPage"));
				control.SelectedTab = page;
				AssertExportCaptions(declaration, page, JobDeclaration.MultipleKeyChief, "CHIEF");
			}

			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "EXP";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = form.FindSingle<LazyLoadedTabControl>("MainTabControl");
				var page = control.FindSingle<ZTabPage>(c => c.Name.Contains("InvoicesTabPage"));
				control.SelectedTab = page;
				AssertExportCaptions(declaration, page, JobDeclaration.MultipleKeyCdsExport, "CDS Export");
			}
		}

		void AssertExportCaptions(JobDeclaration declaration, ZTabPage page, string keyToCheck, string codeString)
		{
			AssertSequencesEqual($"Pre-requisite: declaration should be selecting captions for {codeString}", new[] { keyToCheck }, declaration.MultipleKeysToUse);
			CombineAssertions($"Captions for {codeString}", () =>
			{
				AssertCorrectCaption<ZDropEdit>("ZG_TransportChargesMethodOfPayment", keyToCheck, page, "TransportChargesMethodOfPaymentDropEdit");
				AssertCorrectCaption<ConvertToLocalCurrencyControl>("JZ_InvoiceAmount", keyToCheck, page, "JZ_InvoiceAmountBoundCurrencyControl");
				AssertCorrectCaption<ZDropEdit>("JZ_ValuationCode", keyToCheck, page, "JZ_ValuationCodeDropEdit");
				AssertCorrectCaption<ZDropEdit>("JZ_IncoTerm", keyToCheck, page, "JZ_IncoTermBoundDropDownEdit");
				AssertCorrectCaption<ZTextBox>("JZ_IncoTermPlace", keyToCheck, page, "JZ_IncoTermPlaceTextBox");
			});
		}

		public void TestJobDeclarationImportInvoiceHeadersCaptions()
		{
			var declaration = Factory.New<JobDeclaration>();

			declaration.JE_ApplicationCode = "CHF";
			declaration.JE_MessageType = "IMP";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = form.FindSingle<LazyLoadedTabControl>("MainTabControl");
				var page = control.FindSingle<ZTabPage>(c => c.Name.Contains("InvoicesTabPage"));
				control.SelectedTab = page;
				AssertImportCaptions(declaration, page, JobDeclaration.MultipleKeyChief, "CHIEF");
			}

			declaration.JE_ApplicationCode = "CDS";
			declaration.JE_MessageType = "IMP";
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var control = form.FindSingle<LazyLoadedTabControl>("MainTabControl");
				var page = control.FindSingle<ZTabPage>(c => c.Name.Contains("InvoicesTabPage"));
				control.SelectedTab = page;
				AssertImportCaptions(declaration, page, JobDeclaration.MultipleKeyCdsImport, "CDS Import");
			}
		}

		void AssertImportCaptions(JobDeclaration declaration, ZTabPage page, string keyToCheck, string codeString)
		{
			AssertSequencesEqual($"Pre-requisite: declaration should be selecting captions for {codeString}", new[] { keyToCheck }, declaration.MultipleKeysToUse);
			CombineAssertions($"Captions for {codeString}", () =>
			{
				AssertCorrectCaption<ConvertToLocalCurrencyControl>("JZ_InvoiceAmount", keyToCheck, page, "JZ_InvoiceAmountBoundCurrencyControl");
				AssertCorrectCaption<ZDropEdit>("JZ_ValuationCode", keyToCheck, page, "JZ_ValuationCodeDropEdit");
				AssertCorrectCaption<ZDropEdit>("JZ_IncoTerm", keyToCheck, page, "JZ_IncoTermBoundDropDownEdit");
				AssertCorrectCaption<ZTextBox>("JZ_IncoTermPlace", keyToCheck, page, "JZ_IncoTermPlaceTextBox");
			});
		}

		void AssertCorrectCaption<TControl>(string fieldName, string multipleKey, ZTabPage page, string controlName) where TControl : Control
		{
			AssertEquals(DataBoundResourceStrings.GetDataForProperty(typeof(JobComInvoiceHeader), fieldName, new[] { multipleKey }).Caption, page.FindSingle<TControl>(controlName).GetExtension<ILabelCaptionRenderer>().Caption);
		}
	}
}
