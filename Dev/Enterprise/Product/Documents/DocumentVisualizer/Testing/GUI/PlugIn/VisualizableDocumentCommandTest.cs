using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Macros;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.FlexCelIntegration;
using Enterprise.DocumentVisualizer.GUI;
using Enterprise.DocumentVisualizer.Integration;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.DocumentVisualizer.Testing.GUI
{
	public class VisualizableDocumentCommandTest : TestCaseWithFactory
	{
		public void TestGetVisualizableDocumentCommand_RelatedForm()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);
			menuItem.SU_MenuType = Enterprise.Core.Constants.StmMenuItemTypes.Forms;

			menuItem.Documents.AddNew();

			var dummy = Factory.New<DummyWithUXmlSupport>();
			var command = dummy.GetVisualizableDocumentCommand(menuItem, null);

			AssertNotNull("created command for related form", command);
			Assert("command is applicable", command.IsApplicable);
		}

		public void TestGetVisualizableDocumentCommand_RelatedDocument()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);
			menuItem.SU_MenuType = Enterprise.Core.Constants.StmMenuItemTypes.Documents;

			menuItem.Documents.AddNew();

			var dummy = Factory.New<DummyWithUXmlSupport>();
			var command = dummy.GetVisualizableDocumentCommand(menuItem, null);

			AssertNotNull("created command for non-form", command);
			Assert("command is not applicable", !command.IsApplicable);
		}

		public void TestIsApplicable()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);
			menuItem.SU_FilterList = "Z0_Code == \"AAA\"";

			var template = Factory.New<VisualizerTemplate>();

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var dummy = Factory.New<DummyWithUXmlSupport>();
			dummy.Z0_Code = "AAA";

			var command = dummy.GetVisualizableDocumentCommand(menuItem, null);

			AssertNotNull("prerequisite: command", command);

			AssertEquals("applicable, matches the filter", true, command.IsApplicable);

			dummy.Z0_Code = "BBB";

			AssertEquals("not applicable, doesn't match the filter", false, command.IsApplicable);
		}

		public void TestExecute_Unsaved()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);
			menuItem.SU_FilterList = "Z0_Code == \"AAA\"";

			var template = Factory.New<VisualizerTemplate>();

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var dummy = Factory.New<DummyWithUXmlSupport>();
			dummy.Z0_Code = "AAA";

			var command = dummy.GetVisualizableDocumentCommand(menuItem, null);

			command.Execute();

			AssertEquals("message shown", "Please save before opening 'Test Doc' form.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_HasChanges()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);

			var template = Factory.New<VisualizerTemplate>();

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var dummy = Factory.New<DummyWithUXmlSupport>();
			dummy.Z0_Code = "AAA";

			Factory.Save();

			dummy.Z0_Code = "BBB";

			var command = dummy.GetVisualizableDocumentCommand(menuItem, null);

			command.Execute();

			AssertEquals("message shown", "Please save before opening 'Test Doc' form.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_ChildrenHaveChanges()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);

			var template = Factory.New<VisualizerTemplate>();

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var dummy = Factory.New<DummyWithUXmlSupport>();
			dummy.Z0_Code = "AAA";

			var related = Factory.New<DummyBusinessObject>();
			dummy.Z0_Guid = related.PK;

			dummy.RegisterEditableChildObject(related);

			Factory.Save();

			related.Z0_Code = "BBB";

			var command = dummy.GetVisualizableDocumentCommand(menuItem, null);

			command.Execute();

			AssertEquals("message shown", "Please save before opening 'Test Doc' form.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_ShowForm()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_FilterList = "JK_TransportMode == \"SEA\"";

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = TemplateXlsBlob;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "SEA";

			var command = ((BusinessObject)consol).GetVisualizableDocumentCommand(menuItem, null);

			Factory.Save();

			command.Execute();

			AssertType("visualizer form has been shown", typeof(DocumentVisualizerForm), ZFormModaliser.LastFormShownDialogForTest);
		}

		public void TestExecute_ShowForm_BadFactory()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_FilterList = "JK_TransportMode == \"SEA\"";

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = TemplateXlsBlob;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "SEA";

			var command = ((BusinessObject)consol).GetVisualizableDocumentCommand(menuItem, null);

			Factory.Save();
			using (PersistentFactoryCacheManager.Instance.TrackAllCreatedFactories_ForTest())
			{
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(f =>
				{
					var form = (DocumentVisualizerForm)f;
					var factory = PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Single(fac => fac.NameForDebugging == "VisualizableDocumentCommandFactory");
					factory.SubscribeForDispose(new DisposableAction(() => { }));
				});
				command.Execute();

				AssertType("visualizer form has been shown", typeof(DocumentVisualizerForm), ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#region Execute ShowForm with CNH DeliveryRestriction

		public void TestExecute_ShowForm_DeliveryRestriction_CNH_DoNotOverride_WithCreditOnHold()
		{
			var dummyConsol = Factory.New<DummyConsolBusinessObjectForCreditOnHoldTest>();
			dummyConsol.Z0_Code = "DC1";
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true);
			dummyConsol.OrganisationsForCreditChecksForTest = new[] { orgHeader };

			var command = SetupVisualizableDocumentCommandForCNH(dummyConsol);

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

			command.Execute();

			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertMultilineASCIIEquals(@"Delivery of this form is restricted because:
       The Consignee, Consignor or Local Client for Billing
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this form?
", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Printing Document", UnitTestUserNotification.Instance.LastMessage.Caption);
		}

		public void TestExecute_ShowForm_DeliveryRestriction_CNH_Override_WithCreditOnHold()
		{
			var dummyConsol = Factory.New<DummyConsolBusinessObjectForCreditOnHoldTest>();
			dummyConsol.Z0_Code = "DC1";
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, true);
			dummyConsol.OrganisationsForCreditChecksForTest = new[] { orgHeader };

			var command = SetupVisualizableDocumentCommandForCNH(dummyConsol);

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			command.Execute();

			AssertNotNull("A form has been shown", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("DocumentLoginForm has been shown", "Enterprise.ZArchitecture.GUI.DocumentLoginForm", ZFormModaliser.LastFormShownDialogForTest.GetType().ToString());
			AssertMultilineASCIIEquals(@"Delivery of this form is restricted because:
       The Consignee, Consignor or Local Client for Billing
	  a) Have at least one or more outstanding transactions that have fulfilled the restriction
	      set in the Credit Controlled Documents Check Registry, OR
	  b) Is at or above their Credit Limit, OR
	  c) Has been put on Credit Hold.

Do you wish to override it and deliver this form?
", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Printing Document", UnitTestUserNotification.Instance.LastMessage.Caption);
		}

		public void TestExecute_ShowForm_DeliveryRestriction_CNH_WithoutCreditOnHold()
		{
			var dummyConsol = Factory.New<DummyConsolBusinessObjectForCreditOnHoldTest>();
			dummyConsol.Z0_Code = "DC1";
			var orgHeader = CreditControlledBizo.CreateOrganization(Factory);
			CreditControlledBizo.SetOrganizationCreditOnHold(orgHeader, false);
			dummyConsol.OrganisationsForCreditChecksForTest = new[] { orgHeader };

			var command = SetupVisualizableDocumentCommandForCNH(dummyConsol);

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			command.Execute();

			AssertType("visualizer form has been shown", typeof(DocumentVisualizerForm), ZFormModaliser.LastFormShownDialogForTest);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		IVisualizableDocumentCommand SetupVisualizableDocumentCommandForCNH(DummyConsolBusinessObject dummyConsol)
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_FilterList = "Z0_Code == \"DC1\"";
			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.CNH);
			menuItem.SU_DeliveryRestrictionDescription = ZString.Empty;
			menuItem.SU_DeliveryRestrictionMacro = ZString.Empty;

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = TemplateXlsBlob;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var command = dummyConsol.GetVisualizableDocumentCommand(menuItem, null);

			Factory.Save();

			return command;
		}

		[VisualizableDocumentsSupportable(typeof(DummyConsolBusinessObjectForCreditOnHoldTestSupporter))]
		sealed class DummyConsolBusinessObjectForCreditOnHoldTest : DummyConsolBusinessObject
		{
			public DummyConsolBusinessObjectForCreditOnHoldTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		sealed class DummyConsolBusinessObjectForCreditOnHoldTestSupporter : IVisualizableDocumentSupporter
		{
			public ISecurityCheckpoint CustomizeFormCheckpoint => null;
			public Either<string, object> GetAdditionalData(object parent, IStmMenuItem menuItem) => (object)null;

			public IEnumerable<IDocument> GetAdditionalDocuments(IDocument document, IMessageInstructions messageInstructions)
			{
				yield break;
			}

			public object GetBusinessObjectInAnotherFactory(BusinessObjectFactory factory, object bizObj) => null;

			public IEnumerable<ICommand> GetCustomCommands(string dataContext)
			{
				yield break;
			}

			public Either<string, object> GetDocDataObject(object parent, string dataContext, IDocDataObjectParameters parameters) => (object)null;
			public object GetEventParent(IXmlEventValueObject universalEvent) => null;
			public IEnumerable<IMacroLibrary> GetLibraries(string dataContext) => null;
			public IMessageEventsProcessor GetMessageEventsProcessor(IDocument document) => null;
			public IMessageLogCreator GetMessageLogCreator(IDocument document) => null;
			public IMessagingExtensions GetMessagingExtensions(IDocument document, IMessageInstructions messageInstructions) => null;
			public Either<string, ITopLevelDataObject> GetUniversalXmlDataObject(IDataObjectWriterStrategy strategy, IDocument document, MessageType messageType = MessageType.Unspecified) => new Either<string, ITopLevelDataObject>((ITopLevelDataObject)null);
			public string GetMessageBroker() => string.Empty;
			public bool ShouldUseDraftWatermark(IDocument document) => false;
		}

		#endregion

		public void TestExecute_ShowForm_DeliveryRestriction_UDF()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);
			menuItem.SU_FilterList = "JK_TransportMode == \"SEA\"";
			menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.UDF);
			menuItem.SU_DeliveryRestrictionDescription = "JK_RL_NKLoadPort should be AUSYD";
			menuItem.SU_DeliveryRestrictionMacro = "JK_RL_NKLoadPort ==\"AUSYD\"";

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Template = TemplateXlsBlob;

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUBNE";

			var command = ((BusinessObject)consol).GetVisualizableDocumentCommand(menuItem, null);

			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			command.Execute();
			AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("User defined delivery restriction condition is not met. Condition description: JK_RL_NKLoadPort should be AUSYD", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Unable To Run This Form", UnitTestUserNotification.Instance.LastMessage.Caption);

			consol.JK_RL_NKLoadPort = "AUSYD";
			Factory.Save();

			ZFormModaliser.LastFormShownDialogForTest = null;
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			command.Execute();
			AssertType("visualizer form has been shown", typeof(DocumentVisualizerForm), ZFormModaliser.LastFormShownDialogForTest);
			Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
		}

		public void TestExecute_ShowForm_FirstDocumentWithLogsMatchingFilter() =>
			AssertExecute_ShowForm(
				templates: new[]
				{
					CreateTempTemplate("AAA", true),
					CreateTempTemplate("BBB", true)
				},
				filters: new[]
				{
					showFormMatchingFilter,
					showFormNotMatchingFilter
				},
				expectedTabs: new[]
				{
					"AAA",
					"Events"
				});

		public void TestExecute_ShowForm_SecondDocumentWithLogsMatchingFilter() =>
			AssertExecute_ShowForm(
				templates: new[]
				{
					CreateTempTemplate("AAA", true),
					CreateTempTemplate("BBB", true)
				},
				filters: new[]
				{
					showFormNotMatchingFilter,
					showFormMatchingFilter
				},
				expectedTabs: new[]
				{
					"BBB",
					"Events"
				});

		public void TestExecute_ShowForm_SecondDocumentWithoutLogsMatchingFilter() =>
			AssertExecute_ShowForm(
				templates: new[]
				{
					CreateTempTemplate("AAA", false),
					CreateTempTemplate("BBB", false)
				},
				filters: new[]
				{
					showFormNotMatchingFilter,
					showFormMatchingFilter
				},
				expectedTabs: new[]
				{
					"BBB"
				});

		public void TestExecute_ShowForm_TwoDocumentsWithoutLogsMatchingFilter() =>
			AssertExecute_ShowForm(
				templates: new[]
				{
					CreateTempTemplate("AAA", false),
					CreateTempTemplate("BBB", false)
				},
				filters: new[]
				{
					showFormMatchingFilter,
					showFormMatchingFilter
				},
				expectedTabs: new[]
				{
					"AAA",
					"BBB"
				});

		public void TestExecute_ShowForm_TwoDocumentsWithLogsMatchingFilter() =>
			AssertExecute_ShowForm(
				templates: new[]
				{
					CreateTempTemplate("AAA", true),
					CreateTempTemplate("BBB", true)
				},
				filters: new[]
				{
					showFormMatchingFilter,
					showFormMatchingFilter
				},
				expectedTabs: new[]
				{
					"AAA",
					"BBB",
					"Events"
				});

		const string showFormMatchingFilter = "JK_TransportMode == \"SEA\"";
		const string showFormNotMatchingFilter = "JK_TransportMode == \"AIR\"";

		void AssertExecute_ShowForm(VisualizerTemplate[] templates, string[] filters, string[] expectedTabs)
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);

			for (int i = 0; i < templates.Length; i++)
			{
				var template = templates[i];
				var filter = filters[i];

				var pivot = Factory.New<StmMenuTemplatePivotBase>();
				pivot.SI_SU = menuItem.PK;
				pivot.SI_SO = template.PK;
				pivot.SI_DocumentTitle = "Test 01";
				pivot.SI_DataStoreName = "TEST";
				pivot.SI_MenuTemplateFilter = filter;
			}

			var consol = Factory.New<Forwarding.IForwardingConsol>();
			consol.JK_TransportMode = "SEA";

			var command = ((BusinessObject)consol).GetVisualizableDocumentCommand(menuItem, null);

			Factory.Save();

			void AssertShownForm(DocumentVisualizerForm form)
			{
				AssertEquals("visualizer form has been shown for the correct document", "Test Doc", form.Text);
				var tabControl = form.FindAll<ZTabControl>().FirstOrDefault();
				AssertNotNull("visualizer form has ZTabControl", tabControl);

				AssertContainsExactElementsInAnyOrder("tabs",
					tabControl.AllTabPages.Select(t => t.Text),
					expectedTabs);
			}

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(form =>
			{
				if (form is DocumentVisualizerForm docForm)
				{
					AssertShownForm(docForm);
				}
				else
				{
					Fail("expected to show visualizer form");
				}
			});

			command.Execute();
		}

		VisualizerTemplate CreateTempTemplate(string templateName, bool showEvents)
		{
			var configSection = showEvents
				? $"#Config:Name=\"{templateName}\":DataContext=\"UXML\""
				: $"#Config:Name=\"{templateName}\":DataContext=\"UXML\":ShowEvents=false";

			var content = string.Format($@"{configSection}
#End
#Body
#End");

			var template = Factory.New<VisualizerTemplate>();
			template.SO_Name = templateName;

			var worksheet = DummyWorksheet.Parse(content);
			var builder = new XlsFileBuilder(worksheet);
			var xls = builder.Build();

			using (var templateStream = new MemoryStream())
			{
				xls.Save(templateStream);
				template.SO_Template = templateStream.ToArray();
			}

			return template;
		}

		public void TestExecute_NotApplicable_NoDocumentLinked()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);
			menuItem.SU_FilterList = "Z0_Code == \"AAA\"";

			var template = Factory.New<VisualizerTemplate>();

			var dummy = Factory.New<DummyWithUXmlSupport>();
			dummy.Z0_Code = "AAA";

			var command = dummy.GetVisualizableDocumentCommand(menuItem, null);

			Factory.Save();
			command.Execute();

			AssertEquals("message shown", "'Test Doc' form cannot be shown because there is no document linked.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_NotApplicable_ExecutionContextNotMatched()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);
			menuItem.SU_FilterList = "Z0_Code == \"AAA\"";

			var template = Factory.New<VisualizerTemplate>();

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = "abracadabra";
			pivot.SI_DataStoreName = "7-11";

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_Code = "BBB";

			var command = dummy.GetVisualizableDocumentCommand(menuItem, null);

			Factory.Save();
			command.Execute();

			AssertEquals("message shown", "'Test Doc' form cannot be shown because the filter doesn't match current execution context.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_NotApplicable_FilteredOut()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Test);

			var template = Factory.New<VisualizerTemplate>();

			var pivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pivot1.SI_SU = menuItem.PK;
			pivot1.SI_SO = template.PK;
			pivot1.SI_DocumentTitle = "AAA";
			pivot1.SI_DataStoreName = "7-11";
			pivot1.SI_MenuTemplateFilter = "Z0_Code == \"AAA\"";

			var pivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pivot2.SI_SU = menuItem.PK;
			pivot2.SI_SO = template.PK;
			pivot2.SI_DocumentTitle = "BBB";
			pivot2.SI_DataStoreName = "7-11";
			pivot2.SI_MenuTemplateFilter = "Z0_Code == \"BBB\"";

			var dummy = Factory.New<DummyWithUXmlSupport>();
			dummy.Z0_Code = "CCC";

			var command = dummy.GetVisualizableDocumentCommand(menuItem, null);

			Factory.Save();

			command.Execute();

			AssertEquals("message shown", "None of the documents matches the filtering criteria.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestExecute_NotApplicable_AmbiguousFilter()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			menuItem.SU_MenuName = "Test Doc";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Consol);

			var template = Factory.New<VisualizerTemplate>();

			var pivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pivot1.SI_SU = menuItem.PK;
			pivot1.SI_SO = template.PK;
			pivot1.SI_DocumentTitle = "AAA";
			pivot1.SI_DataStoreName = "7-11";
			pivot1.SI_MenuTemplateFilter = "JK_TransportMode == \"AAA\"";

			var pivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pivot2.SI_SU = menuItem.PK;
			pivot2.SI_SO = template.PK;
			pivot2.SI_DocumentTitle = "BBB";
			pivot2.SI_DataStoreName = "7-11";
			pivot2.SI_MenuTemplateFilter = "JK_TransportMode == \"AAA\"";

			var dummy = Factory.New<Forwarding.IForwardingConsol>();
			dummy.JK_TransportMode = "SEA";

			var command = ((BusinessObject)dummy).GetVisualizableDocumentCommand(menuItem, null);

			Factory.Save();

			command.Execute();

			AssertEquals("message shown",
				"None of the documents matches the filtering criteria.",
				UnitTestUserNotification.Instance.LastMessage.Text);
		}

		protected override void TearDown()
		{
			base.TearDown();
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever());

		ZBlob templateXlsBlob;
		ZBlob TemplateXlsBlob
		{
			get
			{
				if (templateXlsBlob == null)
				{
					var tempFilePath = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentVisualizer.Testing.Core_Legacy.Template.TestTemplate.xls", "TestTemplate.xls");
					templateXlsBlob = StmTemplateBase.GetTemplateBlobFromFile(tempFilePath);
				}
				return templateXlsBlob;
			}
		}
	}
}
