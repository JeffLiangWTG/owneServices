using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using HouseBillDocumentBuilder = Enterprise.DocumentVisualizer.Business.HouseBillDocumentBuilder;

namespace Enterprise.DocumentVisualizer.Testing
{
  public abstract class BillOfLadingContentTest : DocumentContentTest
	{
		protected void AssertContents(BusinessObject parent, ZGuid menuItemPK, string documentTitle, byte index, string expectedContent, string templateName, ZGuid templatePK = default)
		{
			AssertNotNull(nameof(parent), parent);
			AssertNotNullOrEmpty(nameof(documentTitle), documentTitle);

			var documentSupportable = parent as IDocumentSupportable;

			AssertNotNull("bizObj has to be IDocumentSupportable", documentSupportable?.DocumentSupporter);

			var query = new ZDBOnlyQuery(typeof(VisualizerMenuTemplatePivot));
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_DocumentTitle, documentTitle);
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_Index, index);
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_SU, menuItemPK);
			if (templatePK != ZGuid.Empty)
			{
				query.AddToFilter(StmMenuTemplatePivotSchema.SI_SO, templatePK);
			}

			var pivot = parent.Factory.Load<VisualizerMenuTemplatePivot>(query).Single();

			Assert("valid menu-item", parent.IsApplicable(pivot.SI_MenuTemplateFilter));
			AssertEquals("the correct template name", templateName, pivot.Template.SO_Name);

			AssertContents(parent, pivot, expectedContent);
		}

		protected override void AssertContents(BusinessObject parent, VisualizerMenuTemplatePivot pivot, string expectedContent, IDocDataObjectParameters docDataObjectParameters = null)
		{
			var templateBizObj = pivot.Template;

			var documentData = parent.LoadOrCreateDocumentData(pivot.SI_DataStoreName);

			AssertNotNull("document data has been created", documentData);

			docDataObjectParameters = new DocDataObjectParameters(pivot.SI_DocumentTitle, pivot.SI_DataStoreName);
			var tryCreateDynamicData = parent.TryCreate(templateBizObj.SO_DataContext, docDataObjectParameters);

			if (tryCreateDynamicData.IsFaulted)
			{
				Fail(tryCreateDynamicData.Message);
			}

			var macroEvalContext = GetLibraries(parent, pivot.Template.SO_DataContext).CreateContext();

			var worksheet = templateBizObj
				.GetFlexCelWorksheet();

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();
			var logger = new DummyLogger(null);

			IServiceContainer services = new ServiceContainer();
			services.Register<IEventBroker>(() => new EventBroker());
			services.Register<IConsole>(() => new ConsoleForTest());

			var documentScope = new MacroScope(tryCreateDynamicData.Value);

			Either<string, Core.IDocument> rest;
			string templateName = "";

			var supporter = parent.GetSupporter();
			var dataObjectRes = supporter.GetDocDataObject(parent, Constants.HouseBillTemplateDataContext, docDataObjectParameters);

			IHouseBillTemplate template = new HouseBillTemplate(worksheet);
			templateName = templateBizObj.SO_Name;

			var descriptor = new HouseBillDocumentDescriptor(
				parent,
				documentPivot,
				documentData,
				template,
				new ServiceContainer(),
				System.Array.Empty<ICommand>());

			foreach (var variable in GetVariables(services, descriptor, null, (IStmALogParent)parent))
			{
				documentScope.SetVariable(variable.Name, variable.Value);
			}

			var parameters = new HouseBillDocumentBuilder.Parameters
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

			var documentBuilder = new HouseBillDocumentBuilder(parameters);

			rest = documentBuilder.Build();

			if (rest.IsLeft)
			{
				Fail(rest.Left);
			}

			var errors = rest.Right.ToFormatString(includeErrors: true, includeMessageErrors: false, includeWarnings: false);

			AssertMultilineASCIIEquals($"Template: '{templateName}' Pivot: '{pivot.SI_DocumentTitle}' should have no errors after creation",
				string.Empty,
				errors);

			AssertMultilineASCIIEquals("expected document content",
				expectedContent, ConvertToString(rest.Right));

			errors = rest.Right.ToFormatString(includeErrors: true, includeMessageErrors: false, includeWarnings: false);

			AssertMultilineASCIIEquals($"Template: '{templateName}' Pivot: '{pivot.SI_DocumentTitle}' should have no errors after getting the cell contents",
				string.Empty,
				errors);
		}

		protected IHouseBillTemplate LoadTemplate(ZQuery query)
		{
			var worksheet = Factory.Load<VisualizerTemplate>(query)
				.Single()
				.GetFlexCelWorksheet();

			return new HouseBillTemplate(worksheet);
		}
	}
}
