using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.MasterFiles.Business.Documents;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
  public abstract class DocumentContentTest : TestCaseWithFactory
	{
		[SnailTest]
		public abstract void TestDocumentContent();

		protected void AssertContents(BusinessObject parent, string documentTitle, string expectedContent, IDocDataObjectParameters docDataObjectParameters = null)
		{
			AssertNotNull(nameof(parent), parent);
			AssertNotNullOrEmpty(nameof(documentTitle), documentTitle);

			var documentSupportable = parent as IDocumentSupportable;

			AssertNotNull("bizObj has to be IDocumentSupportable", documentSupportable?.DocumentSupporter);

			var query = new ZDBOnlyQuery(typeof(VisualizerMenuTemplatePivot));
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_DocumentTitle, documentTitle);

			var pivots = parent.Factory.Load<VisualizerMenuTemplatePivot>(query);

			Assert($"There's no document with title '{documentTitle}'", pivots.Length > 0);
			Assert($"There's more than one document with title '{documentTitle}'", pivots.Length == 1);

			AssertContents(parent, pivots.Single(), expectedContent, docDataObjectParameters);
		}

		protected void AssertContents(BusinessObject parent, ZGuid menuItemPK, string documentTitle, byte index, string expectedContent, IDocDataObjectParameters docDataObjectParameters = null)
		{
			AssertNotNull(nameof(parent), parent);
			AssertNotNullOrEmpty(nameof(documentTitle), documentTitle);

			var documentSupportable = parent as IDocumentSupportable;

			AssertNotNull("bizObj has to be IDocumentSupportable", documentSupportable?.DocumentSupporter);

			var query = new ZDBOnlyQuery(typeof(VisualizerMenuTemplatePivot));
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_DocumentTitle, documentTitle);
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_Index, index);
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_SU, menuItemPK);

			var pivot = parent.Factory.Load<VisualizerMenuTemplatePivot>(query).Single();

			AssertContents(parent, pivot, expectedContent, docDataObjectParameters);
		}

		protected void AssertContents(BusinessObject parent, ZGuid pivotPK, string expectedContent)
		{
			AssertNotNull(nameof(parent), parent);

			var documentSupportable = parent as IDocumentSupportable;

			AssertNotNull("bizObj has to be IDocumentSupportable", documentSupportable?.DocumentSupporter);

			var query = new ZDBOnlyQuery(typeof(VisualizerMenuTemplatePivot));
			query.AddToFilter(StmMenuTemplatePivotSchema.PK, pivotPK);

			var pivot = parent.Factory.LoadTop1<VisualizerMenuTemplatePivot>(query);

			AssertNotNull($"VisualizerMenuTemplatePivot not found with PK: {pivotPK}", pivot);
			AssertContents(parent, pivot, expectedContent);
		}

		protected abstract void AssertContents(BusinessObject parent, VisualizerMenuTemplatePivot pivot, string expectedContent, IDocDataObjectParameters docDataObjectParameters = null);

		protected IEnumerable<IMacroLibrary> GetLibraries(BusinessObject parent, string dataContext)
		{
			yield return new MetaDataLibrary();
			yield return new TableTextGeneratorLibrary();
			yield return new StandardLibrary();
			yield return new DocumentLibrary();
			yield return new DataLibrary(parent.Factory);
			yield return new MasterFilesLibrary(parent.Factory);

			var supporter = parent.GetSupporter();
			var libraries = supporter?.GetLibraries(dataContext);

			if (libraries != null)
			{
				foreach (var library in libraries)
				{
					yield return library;
				}
			}
		}

		protected IEnumerable<IVariable> GetVariables(IServiceContainer services, IDocumentDescriptor descriptor, IStandardTemplate template, IStmALogParent logParent)
		{
			yield return new Variable(VariableNames.Environment,
				new Enterprise.MasterFiles.Business.Macros.Environment());

			yield return new Variable(VariableNames.Document,
					new DocumentVisualizer.Business.Document(descriptor, logParent));

			yield return new Variable(VariableNames.Console,
					new ConsoleForTest());

			yield return new Variable(VariableNames.UI,
				new UserInterface(services));

			yield return new Variable(VariableNames.Commands,
				new MacroMap(new Dictionary<string, object>()));

			yield return new Variable(VariableNames.Resources,
				new DocumentVisualizer.Business.Resources(services.Resolve<IResourceAccessor>()));
		}

		sealed protected class ConsoleForTest : IConsole
		{
			void IConsole.Log(object obj)
			{
				throw new InvalidOperationException("We should not use @console.Log in the production.");
			}
		}

		protected string ConvertToString(IWorksheet worksheet)
		{
			var builder = new StringBuilder();

			foreach (var row in worksheet.Rows)
			{
				foreach (var column in worksheet.Columns)
				{
					var cell = worksheet.GetCell(row.Number, column.Number);

					if (cell.TopRow != row.Number
						|| cell.LeftColumn != column.Number)
					{
						continue;
					}

					var text = Convert.ToString(cell.Value);

					if (!string.IsNullOrWhiteSpace(text))
					{
						builder.AppendLine($"[{row.Number},{column.Number}] {text}");
					}
				}
			}

			return builder.ToString().TrimEnd('\r', '\n');
		}
	}
}
