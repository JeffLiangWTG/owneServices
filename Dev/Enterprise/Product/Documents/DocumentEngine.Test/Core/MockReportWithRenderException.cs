using System;
using System.IO;
using CargoWise.IO;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Moq;

namespace Enterprise.DocumentEngine.Testing
{
	public class MockReportWithRenderException : MockReport
	{
		public MockReportWithRenderException()
			: base(TestTemplate)
		{
			var renderer = new Mock<IReportRenderer>();
			var renderException = new SQLExecutionException("ARandomTableName", "SomeCommand", new Exception());
			renderer.SetupSequence(m => m.Render()).Throws(renderException);

			this.IncludedInPrint = true;
			this.Renderer = renderer.Object;
			((IReportForUnitTesting)this).GenerateRegardlessOfAnyErrors = true;
		}

		readonly static Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		static ExcelTemplateForUnitTesting testTemplate;
		static ExcelTemplateForUnitTesting TestTemplate
		{
			get
			{
				if (testTemplate == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.EmptyAndValidTemplate.xls", "EmptyAndValidTemplate.xls");
					testTemplate = new ExcelTemplateForUnitTesting("EmptyAndValidTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return testTemplate;
			}
		}

		protected override void Dispose(bool isDisposing)
		{
			base.Dispose(isDisposing);
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
			testTemplate = null;
		}
	}
}
