using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Documents;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using StandardDocumentBuilder = Enterprise.DocumentVisualizer.Business.StandardDocumentBuilder;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class SystemTemplatesTest : TestCaseWithFactory
	{
		public void TestDataStoreNameDoesntConflictWithDocumentTitlesUsedForUXMLMatching()
		{
			var query = new ZDBOnlyQuery(typeof(StmMenuTemplatePivot));
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_IsSystemDefined, true);
			query.AddToFilter(StmMenuTemplatePivotSchema.SI_DataStoreName, SQLComparisonOperator.Equal, DocumentTitlesUsedForUXMLMatching);

			var menuSubQuery = new ZDBOnlySubQuery(typeof(StmMenuItem), StmMenuTemplatePivotSchema.SI_SU);
			menuSubQuery.AddToFilter(StmMenuItemSchema.SU_MenuType, Enterprise.Core.Constants.StmMenuItemTypes.Forms);

			query.AddSubQuery(menuSubQuery, JoinCondition.And);

			AssertEquals(0, Factory.GetDatabaseCount(typeof(StmMenuTemplatePivot), query));
		}

		[SnailTest]
		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestSystemTemplatesHaveNoErrors()
		{
			var query = new ZQuery(StmTemplateSchema.SO_TemplateType, StmTemplateTypes.Codes.Form);
			query.AddToFilter(StmTemplateSchema.SO_DataContext, DataContext.UXML);
			query.AddToFilter(StmTemplateSchema.SO_IsSystemDefined, true);
			query.AddToFilter(StmTemplateSchema.SO_Name, SQLComparisonOperator.NotEqual, TemplatesToIgnore);

			var templates = Factory.Load<VisualizerTemplate>(query);

			var pivotQuery = new ZQuery();
			pivotQuery.AddToFilter(StmMenuTemplatePivotSchema.SI_SO, templates.Select(t => t.PK));

			var pivots = Factory.Load<VisualizerMenuTemplatePivot>(pivotQuery);

			IDataObject dataObject = null;

			var text = File.ReadAllText(TestFiles.UniversalXmlFilePath);

			using (var reader = new StringReader(text))
			{
				dataObject = reader.Parse<Shipment>();
			}

			AssertNotNull("UXml", dataObject);

			var pivotsByTemplate = pivots
				.GroupBy(p => p.SI_SO);

			CombineAssertions(() =>
			{
				foreach (var group in pivotsByTemplate)
				{
					var pivot = group.First();
					var parent = CreateBusinessObject(pivot.MenuItem);

					AssertTemplate(parent, pivot, dataObject);
				}
			});
		}

		#region Implementation

		void AssertTemplate(BusinessObject parent, VisualizerMenuTemplatePivot pivot, IDataObject dataObject)
		{
			var templateBizObj = pivot.Template;

			var documentData = parent.LoadOrCreateDocumentData(pivot.SI_DataStoreName);

			var dynamicData = dataObject.MakeDynamic(new UXmlMetaDataProvider(new UXmlLinkManager()));

			var macroEvalContext = GetLibraries(parent, pivot.Template.SO_DataContext).CreateContext();

			var documentPivot = DocumentPivot.Create(new[] { pivot }).Single();

			var worksheet = templateBizObj
				.GetFlexCelWorksheet();

			IStandardTemplate template = new StandardTemplate(worksheet);

			var descriptor = new StandardDocumentDescriptor(documentPivot,
				documentData,
				template,
				new MacroScope(documentData),
				macroEvalContext);

			IServiceContainer services = new ServiceContainer();
			services.Register<IEventBroker>(() => new EventBroker());
			services.Register<IConsole>(() => new ConsoleForTest());

			var logger = new DummyLogger(null);

			var documentScope = new MacroScope(dynamicData);

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
				Commands = Array.Empty<ICommand>()
			};

			var documentBuilder = new StandardDocumentBuilder(parameters);

			var res = documentBuilder.Build();

			if (res.IsLeft)
			{
				Fail(res.Left);
			}

			var errors = res.Right.ToFormatString(includeErrors: true, includeMessageErrors: false, includeWarnings: false);

			AssertMultilineASCIIEquals($"Template: '{template.Name}' Pivot: '{pivot.SI_DocumentTitle}' should have no errors",
				string.Empty,
				errors);
		}

		IEnumerable<IMacroLibrary> GetLibraries(BusinessObject parent, string dataContext)
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

		IEnumerable<IVariable> GetVariables(IServiceContainer services, IDocumentDescriptor descriptor, IStandardTemplate template, IStmALogParent logParent)
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

		sealed class ConsoleForTest : IConsole
		{
			void IConsole.Log(object obj)
			{
				throw new InvalidOperationException("We should not use @console.Log in the production.");
			}
		}

		IEnumerable<string> TemplatesToIgnore
		{
			get
			{
				yield break;
			}
		}

		BusinessObject CreateBusinessObject(StmMenuItem menuItem)
		{
			switch (menuItem.SU_BusinessContext)
			{
				case "Consol":
					return (BusinessObject)Factory.New<Forwarding.IForwardingConsol>();

				case "Shipment":
					return (BusinessObject)Factory.New<Forwarding.IForwardingShipment>();

				case "Customs":
					return (BusinessObject)Factory.New<Customs.AU.IJobDeclaration>();

				default:
					throw new NotImplementedException($"Create business object for '{menuItem.SU_BusinessContext}' SU_BusinessContext");
			}
		}

		IEnumerable<string> DocumentTitlesUsedForUXMLMatching
		{
			get
			{
				yield return "Booking Request";
				yield return "Shipping Instruction";
				yield return "Verified Gross Container Weight";
				yield return "Shipper's Declaration For Dangerous Goods";
				yield return "Export Notification";
				yield return "Import Notification";
				yield return "ATF6A";
				yield return "Form of Undertaking for Inter-RA AWB Handling";
				yield return "Declaration of Export Consignment - Bulk";
				yield return "Regulated Agent Aviation Security Declaration";
				yield return "Declaration of Export Consignment - Pre-packed Unit";
				yield return "Pro Form Invoice";
				yield return "Pro Forma Invoice";
			}
		}

		#endregion
	}
}
