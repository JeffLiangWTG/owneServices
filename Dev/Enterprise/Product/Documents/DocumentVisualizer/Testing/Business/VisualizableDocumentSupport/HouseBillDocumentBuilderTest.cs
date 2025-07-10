using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.DocumentVisualizer.Presentation;
using Enterprise.DocumentVisualizer.Testing.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Forwarding = Enterprise.Integration.Forwarding;
using HouseBillDocumentBuilder = Enterprise.DocumentVisualizer.Business.HouseBillDocumentBuilder;

namespace Enterprise.DocumentVisualizer.Testing
{
	sealed class HouseBillDocumentBuilderTest : TestCaseWithFactory
	{
		string CustomBillOfLadingTemplateWithNREFilePath => Path.Combine(TestCase.BaseSourcePath, @"Enterprise\Product\Documents\DocumentVisualizer\Testing\Business\VisualizableDocumentSupport\CustomBillOfLadingTemplateWithNRE.xls");

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportBillOfLadingTemplateThrowingAnException_CustomTemplate() =>
			AssertReportBillOfLadingTemplateThrowingAnException(isSystemTemplate: false);

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReportBillOfLadingTemplateThrowingAnException_SystemTemplate() =>
			AssertReportBillOfLadingTemplateThrowingAnException(isSystemTemplate: true);

		void AssertReportBillOfLadingTemplateThrowingAnException(bool isSystemTemplate)
		{
			var shipment = Factory.New<Forwarding.IForwardingShipment>();

			var bizObj = (BusinessObject)shipment;
			bizObj[JobShipmentSchema.Constants.JS_TransportMode] = "SEA";
			bizObj[JobShipmentSchema.Constants.JS_RL_NKLoadPort] = "AUSYD";
			bizObj[JobShipmentSchema.Constants.JS_RL_NKDischargePort] = "SGSIN";

			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Custom Bill Of Lading";
			menuItem.SU_IsSystemDefined = isSystemTemplate;

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = StmTemplateBase.GetTemplateBlobFromFile(CustomBillOfLadingTemplateWithNREFilePath);

			var stmPivot = Factory.New<VisualizerMenuTemplatePivot>();
			stmPivot.SI_DocumentTitle = "ORIGINAL";
			stmPivot.SI_DataStoreName = "7-11";
			stmPivot.SI_SO = template.PK;
			stmPivot.SI_SU = menuItem.PK;

			Factory.Save();

			var securityHelper = new DocumentSecurityService(menuItem, DummyModuleIDs.Dummy);

			var services = new ServiceContainer();
			services.Register<IDocumentSecurityService>(securityHelper);
			services.Register<IPageViewBuildService>(() => new PageViewBuildService());
			services.Register<IEditorBuildService>(() => new DynamicContentEditorBuildService());
			services.Register<IDocumentDeliveryService>(() => new DocumentDeliveryService());
			services.Register<IUserNotificationService>(() => new UserNotificationService());
			services.Register<IDocumentToolsService>(() => new DocumentToolsService());
			services.Register<IEventBroker>(new EventBroker());
			services.Register<IConsoleService>(new ConsoleService());
			services.Register<IResourceProvider>(new DummyResourcesProvider());
			services.Register<INotificationViewBuildService>(new DummyNotificationViewBuildService());

			var houseBillTemplate = (IHouseBillTemplate)template
				.GetFlexCelWorksheet()
				.CreateTemplate(isSystemDefined: isSystemTemplate)
				.Right;

			var documentData = Factory.New<VisualizerDocumentData>();
			var commands = Array.Empty<ICommand>();
			var pivot = DocumentPivot
				.Create(new[] { stmPivot })
				.Single();

			var descriptor = new HouseBillDocumentDescriptor((BusinessObject)shipment, pivot, documentData, houseBillTemplate, services, commands);

			var logs = new List<string>();
			void LogHandler(LogMessageType type, string message) => logs.Add(message);

			var context = new IMacroLibrary[]
			{
				new StandardLibrary(),
				new DataLibrary(),
				new HouseBillMacroLibraryForTest()
			}.CreateContext();

			var vessel = new Vessel { Name = "Exception Thrower", VoyageFlightNumber = "123" };

			var billOfLading = new BillOfLading
			{
				ShipmentNumber = "S0001",
				Consols = new BillOfLadingConsolCollection
				{
					Departure = new BillOfLadingConsol
					{
						Number = "C0001",
						Vessel = vessel
					}
				}
			};

			vessel.NameProvider = () => ((object)null).ToString();

			var parameters = new HouseBillDocumentBuilder.Parameters
			{
				Template = houseBillTemplate,
				Services = services,
				Scope = new MacroScope(billOfLading.MakeDynamic()),
				Descriptor = descriptor,
				DocumentData = documentData,
				Commands = commands,
				MacroEvaluationContext = context,
				Logger = new DummyLogger(LogHandler)
			};

			var builder = new HouseBillDocumentBuilder(parameters);
			var res = builder.Build();

			Assert("building document failed", res.IsLeft);
			AssertContains("exception message has been reported", "Object reference not set to an instance of an object.", res.Left);
			AssertContains("macro causing exception has been reported", "CustomField(\"PreCarriage\", \"<Consols.Departure.Vessel.Name> /<Consols.Departure.Vessel.VoyageFlightNumber>\")", res.Left);

			if (!isSystemTemplate)
			{
				AssertEquals("we reported macro with NRE issue", "BillOfLadingTemplateBuildIssue", ErrorReporter.LastKeyReported);
				AssertContains("error report contains stack trace of the method throwing NRE", "Enterprise.DocumentVisualizer.Testing.HouseBillDocumentBuilderTest.Vessel.get_Name()", ErrorReporter.LastMessageReported);
				ErrorReporter.Clear();
			}
		}

		#region Document Data Source

		sealed class BillOfLading : DocDataObject
		{
			public ZString ShipmentNumber
			{
				get => shipmentNumber;
				set
				{
					if (SetNonPersistentPropertyValue(ShipmentNumberInfo, ref shipmentNumber, value))
					{
					}
				}
			}

			ZString shipmentNumber;

			public ZPropertyInfo ShipmentNumberInfo => GetZPropertyInfo(nameof(ShipmentNumber));

			public BillOfLadingConsolCollection Consols
			{
				get => consols;
				set => consols = SetChild(consols, value);
			}
			BillOfLadingConsolCollection consols;
		}

		sealed class Vessel : DocDataObject
		{
			public Func<ZString> NameProvider;

			#region Name

			public ZString Name
			{
				get => NameProvider?.Invoke() ?? name;
				set
				{
					if (SetNonPersistentPropertyValue(NameInfo, ref name, value))
					{
					}
				}
			}

			ZString name;

			public ZPropertyInfo NameInfo => GetZPropertyInfo(nameof(Name));

			#endregion

			#region VoyageFlightNumber

			public ZString VoyageFlightNumber
			{
				get => voyageFlightNumber;
				set
				{
					if (SetNonPersistentPropertyValue(VoyageFlightNumberInfo, ref voyageFlightNumber, value))
					{
					}
				}
			}

			ZString voyageFlightNumber;

			public ZPropertyInfo VoyageFlightNumberInfo => GetZPropertyInfo(nameof(VoyageFlightNumber));

			#endregion
		}

		sealed class BillOfLadingConsolCollection : DocDataObject
		{
			public BillOfLadingConsol Departure
			{
				get => departure;
				set => departure = SetChild(departure, value);
			}

			BillOfLadingConsol departure;
		}

		sealed class BillOfLadingConsol : DocDataObject
		{
			#region Number

			public ZString Number
			{
				get => number;
				set
				{
					if (SetNonPersistentPropertyValue(NumberInfo, ref number, value))
					{
					}
				}
			}

			ZString number;

			public ZPropertyInfo NumberInfo => GetZPropertyInfo(nameof(Number));

			#endregion

			#region Vessel

			public Vessel Vessel
			{
				get => vessel;
				set => vessel = SetChild(vessel, value);
			}

			Vessel vessel;

			#endregion
		}

		#endregion

		#region HouseBillMacroLibraryForTest

		sealed class HouseBillMacroLibraryForTest : MacroLibrary
		{
			protected override IEnumerable<IHandler> MacroHandlers
			{
				get
				{
					yield return new Handler<Action>(
						"MacroThrowingNRE",
						"Macro throwing NRE.",
						() => MacroThrowingNRE());
				}
			}

			void MacroThrowingNRE() => throw new NullReferenceException("NRE from macro");
		}

		#endregion
	}
}
