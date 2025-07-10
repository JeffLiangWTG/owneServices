using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.ExcelTemplates;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Testing
{
	public class ExcelTemplateDateRangeFieldValueTypeTest : TestCaseWithFactory
	{
		static readonly Regex pathRegex = new Regex(@"Enterprise.+");

		public void TestSystemDefinedTemplate()
		{
			var baseline = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
			using (var reader = new StreamReader(resourceRetriever.Value.GetStream("DateRangeFieldBaseline.txt")))
			{
				while (reader.ReadLine() is { } line)
				{
					baseline.Add(line.Replace("\xa0", " "));
				}
			}

			var additionalQuery = new ZQuery(StmTemplateSchema.SO_IsSystemDefined, true);
			additionalQuery.AddToFilter(StmTemplateSchema.SO_IsClientSpecific, false);
			additionalQuery.AddToFilter(StmTemplateSchema.SO_DataContext, SQLComparisonOperator.Equal, nameof(Core.Constants.DataContext.None));
			var templates = new StmTemplateBaseCollection(Factory);
			templates.Load(additionalQuery);

			var testResults = new List<TemplateTestResult>();
			foreach (var template in templates.OfType<StmTemplateBase>())
			{
				var fullPath = template.ExcelTemplateFullPath.Replace('\\', '/');
				var path = pathRegex.Match(fullPath).Value;

				using var pack = new DocumentPack();
				using var rpt = new Report(pack, new ExcelTemplateReadFromStmTemplateTable(template));
				var problematicFunctions = ExcelTemplateCheckHelper.CheckDateRangeFilterInSQLQuery(rpt);

				testResults.Add(new TemplateTestResult(path, problematicFunctions.Count > 0));
			}

			var problematicTemplates = testResults.Where(x => x.HasErrors && !baseline.Contains(x.Path)).Select(x => x.Path).ToArray();
			var fixedTemplatesNotRemovedFromBaseline = testResults.Where(x => !x.HasErrors && baseline.Contains(x.Path)).Select(x => x.Path).ToArray();

			Assert($@"You tried using some un-expected functions of {nameof(DateRangeField)} or {nameof(DateField)} in Report SQL Sources, this function might return an empty string instead of a DB Null value for SQL usage, this doesn't make sense, please try using FromDateForSQLParameter or ToDateForSQLParameter (or add your new custom function like them), to make sure it returns a DB Null value when it's empty.
Template Paths: \r\n{string.Join("\r\n", problematicTemplates)}", problematicTemplates.Length == 0);
			Assert($"If the template has been fixed, please remove it from the baseline file.\r\n {string.Join("\r\n", fixedTemplatesNotRemovedFromBaseline)}", fixedTemplatesNotRemovedFromBaseline.Length == 0);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new (() => new EmbeddedResourceRetriever());

		class TemplateTestResult
		{
			public string Path { get; }
			public bool HasErrors { get; }

			public TemplateTestResult(string path, bool hasErrors)
			{
				Path = path;
				HasErrors = hasErrors;
			}
		}
	}
}
