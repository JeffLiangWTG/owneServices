using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(RuntimeOptionsForm))]
	sealed class RuntimeOptionsUserControlsBasherTest : ZFormBasherTest
	{
		public void TestTemplateContainsAllControls()
		{
			using (var report = LoadReport())
			{
				var allControls = Enum.GetValues(typeof(FilterFieldSuggestedUserControlType))
					.Cast<FilterFieldSuggestedUserControlType>()
					.Except(FilterFieldSuggestedUserControlType.ZMultiLineTextFieldUserControl) // This is used by the 'memo' field, which is a User Defined Field and not a Filter used by this form.
					.Except(FilterFieldSuggestedUserControlType.ZCheckBox) // Used for optional templates and doesn't appear in FilterControls list
					.Except(FilterFieldSuggestedUserControlType.None);

				var reportControls = report.FilterCollection.OfType<FilterField>().Select(f => f.SuggestedUserControlType);
				var missingControls = allControls.Except(reportControls).Select((value, i) => i + ": " + value);

				Assert("Please add a filter that uses the following controls to AllFilterControls.xlsx to allow them to be tested by the basher: " + string.Join(System.Environment.NewLine, missingControls), !missingControls.Any());
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = RuntimeOptionsFormTestCase.GetNewForm(LoadReport());
			MissingResourceStringChecker.ExcludeFromTest(form.Controls.Find("DropEditReportOrientation", true).Single());
			return form;
		}

		protected override bool ShouldTestFormIsFullyTranslatable => false;

		protected override bool ShouldIgnoreMissingBindingMember(Control control) => !(control is RuntimeOptionUserControl);

		Report formReport;
		protected override void TearDown()
		{
			formReport?.Dispose();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
			base.TearDown();
		}

		Report LoadReport()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.AllFilterControls.xls", "AllFilterControls.xls");
			var template = new ExcelTemplateForUnitTesting("AllFilterControls.xls", Path.GetFullPath(tempFileName));
			formReport = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template);
			formReport.PrepareForRender();
			return formReport;
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly));
	}
}
