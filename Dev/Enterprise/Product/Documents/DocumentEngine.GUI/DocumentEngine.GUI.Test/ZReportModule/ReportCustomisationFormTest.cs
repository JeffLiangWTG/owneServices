using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.IO;
using Enterprise.DocumentEngine.GUI.DocumentMenu.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	[TestedType(typeof(ReportCustomisationForm))]
	sealed class ReportCustomisationFormTest : MenuCustomisationFormAbstractTest
	{
		[RequiresSTA]
		public void TestMacroColumnStyleInFilter()
		{
			using (var form = (ReportCustomisationForm)GetFormToBash())
			{
				form.Show();

				var macroFilterList = (ZMacrosFindBoxColumnStyleInfo)form.MenusGrid.GetColumnStyle("SU_FilterList");
				AssertEquals(typeof(ZMacrosFindBoxColumnStyleInfo), macroFilterList.GetType());
				AssertEquals(true, macroFilterList.IsUsedForExpressions);
			}
		}

		public override void TestEmailSubjectTextBox_CharacterLimitIs512()
		{
			using (var form = (ReportCustomisationForm)GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuDetailsTab;
				var emailSubjectTextBox = form.PivotAndChildMenuTabControl.Controls.Find("emailSubjectTextBox", true).FirstOrDefault() as ZTextBox;

				AssertNull(emailSubjectTextBox);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var customisation = new ReportMenuCustomisation(Factory, "RepRefFilesReports");
			return new ReportCustomisationForm(customisation);
		}

		protected override string FilePathForLoadTemplate
			=> resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with defaults.xls");

		protected override string FilePathForLoadTemplateXlsx
			=> resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with defaults.xlsx");

		protected override string OriginalDataContext
		{
			get { return nameof(Core.Constants.DataContext.None); }
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly));
	}
}
