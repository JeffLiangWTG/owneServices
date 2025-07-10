using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.RuntimeOptions.Testing
{
	[TestedType(typeof(RuntimeOptionsForm))]
	sealed class RuntimeOptionsFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => RuntimeOptionsFormTestCase.GetNewForm(LoadReport());

		protected override bool ShouldIgnoreMissingBindingMember(Control control) => control.Name == "LanguageZDropEdit" || base.ShouldIgnoreMissingBindingMember(control);

		Report LoadReport()
		{
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
			var template = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
			formReport = new Report(new DocumentPack(Factory.LoadTop1<StmMenuItem>(new ZQuery())), template);
			formReport.PrepareForRender();
			return formReport;
		}

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

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(DocumentEngine.Testing.PrintTaskTest).Assembly));
	}
}
