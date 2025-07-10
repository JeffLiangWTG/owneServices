using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Business;
using StandardDocumentBuilder = Enterprise.DocumentVisualizer.Business.StandardDocumentBuilder;

namespace Enterprise.DocumentVisualizer.Testing
{
	public abstract class StandardDocumentContentTest : DocumentContentTest
	{
		protected override void AssertContents(BusinessObject parent, VisualizerMenuTemplatePivot pivot, string expectedContent, IDocDataObjectParameters docDataObjectParameters = null)
		{
			var templateBizObj = pivot.Template;

			var documentData = parent.LoadOrCreateDocumentData(pivot.SI_DataStoreName);

			AssertNotNull("document data has been created", documentData);

			docDataObjectParameters = docDataObjectParameters ?? new DocDataObjectParameters(pivot.SI_DocumentTitle, pivot.SI_DataStoreName);
			var tryCreateDynamicData = parent.TryCreate(templateBizObj.SO_DataContext, docDataObjectParameters);

			if (tryCreateDynamicData.IsFaulted)
			{
				Fail(tryCreateDynamicData.Message);
			}

			var macroEvalContext = GetLibraries(parent, pivot.Template.SO_DataContext).CreateContext();

			var worksheet = templateBizObj
				.GetFlexCelWorksheet();

			IStandardTemplate template = new StandardTemplate(worksheet);

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			var descriptor = new StandardDocumentDescriptor(documentPivot,
				documentData,
				template,
				new MacroScope(documentData),
				macroEvalContext);

			IServiceContainer services = new ServiceContainer();
			services.Register<IEventBroker>(() => new EventBroker());
			services.Register<IConsole>(() => new ConsoleForTest());

			var logger = new DummyLogger(null);

			var documentScope = new MacroScope(tryCreateDynamicData.Value);

			foreach (var variable in GetVariables(services, descriptor, template, (IStmALogParent)parent))
			{
				documentScope.SetVariable(variable.Name, variable.Value);
			}

			var parameters = new StandardDocumentBuilder.Parameters
			{
				Template = template,
				Descriptor = descriptor,
				Services = services,
				Logger = logger,
				DocumentData = documentData,
				Scope = documentScope,
				MacroEvaluationContext = macroEvalContext,
				Commands = System.Array.Empty<ICommand>()
			};

			var documentBuilder = new StandardDocumentBuilder(parameters);

			var rest = documentBuilder.Build();

			if (rest.IsLeft)
			{
				Fail(rest.Left);
			}

			var errors = rest.Right.ToFormatString(includeErrors: true, includeMessageErrors: false, includeWarnings: false);

			AssertMultilineASCIIEquals($"Template: '{template.Name}' Pivot: '{pivot.SI_DocumentTitle}' should have no errors after creation",
				string.Empty,
				errors);

			var actualContent = ConvertToString(rest.Right);

			AssertMultilineASCIIEquals("expected document content",
				expectedContent, actualContent);

			errors = rest.Right.ToFormatString(includeErrors: true, includeMessageErrors: false, includeWarnings: false);

			AssertMultilineASCIIEquals($"Template: '{template.Name}' Pivot: '{pivot.SI_DocumentTitle}' should have no errors after getting the cell contents",
				string.Empty,
				errors);
		}

		protected IStandardTemplate LoadTemplate(ZQuery query)
		{
			var worksheet = Factory.Load<VisualizerTemplate>(query)
				.Single()
				.GetFlexCelWorksheet();

			return new StandardTemplate(worksheet);
		}

		protected IDocumentDescriptor CreateDocumentDescriptor(BusinessObject parent, VisualizerMenuTemplatePivot pivot)
		{
			var templateBizObj = pivot.Template;
			var documentData = parent.LoadOrCreateDocumentData(pivot.SI_DataStoreName);

			AssertNotNull("document data has been created", documentData);

			var macroEvalContext = GetLibraries(parent, pivot.Template.SO_DataContext).CreateContext();
			var worksheet = templateBizObj
				.GetFlexCelWorksheet();
			IStandardTemplate template = new StandardTemplate(worksheet);
			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			return new StandardDocumentDescriptor(documentPivot,
				documentData,
				template,
				new MacroScope(documentData),
				macroEvalContext);
		}
	}
}
