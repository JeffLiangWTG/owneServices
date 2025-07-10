using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.GUI.SDF;
using Enterprise.DocumentEngine.GUI.Testing;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using static Enterprise.DocumentEngine.GUI.DocumentUDFPlugIn;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.DocDataPlugIn.Testing
{
	sealed class DocDataPlugInTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestShouldHideTopLevelMenuWithTab()
		{
			BusinessObject dummy = new DummyNonPersistentBusinessObjectWithRow(Factory);

			using (ZFormForTesting form = new ZFormForTesting(dummy))
			{
				form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
				DocDataPlugIn docDataPlugin = (DocDataPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);
				AssertEquals("ShouldHideTopLevelMenuWithTab should be false", false, docDataPlugin.ShouldHideTopLevelMenuWithTab);
			}
		}

		public void TestDocDataPluginDoesNotShowForNonPersistentBusinessObjects()
		{
			BusinessObject dummy = new DummyNonPersistentBusinessObjectWithRow(Factory);

			using (ZFormForTesting form = new ZFormForTesting(dummy))
			{
				form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
				DocDataPlugIn docDataPlugin = (DocDataPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);
				AssertNull("Should be no Doc Data tab for non-persistent business objects", docDataPlugin.UserControl);
			}

			dummy = Factory.New<DummyConsolBusinessObject>();

			using (ZFormForTesting form = new ZFormForTesting(dummy))
			{
				form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
				DocDataPlugIn docDataPlugin = (DocDataPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);
				AssertNotNull("Doc Data tab should be here for persistent business objects", docDataPlugin.UserControl);
			}
		}

		#region IDocumentEvents Tests

		public void TestNotifyDocumentPrinted()
		{
			var docEvents = new DocumentEventsTestClass();
			var documentEventsForMenu = new DocumentEventsForMenu(docEvents);
			docEvents.DocumentPrinted = false;
			documentEventsForMenu.NotifyDocumentPrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, Factory.New<StmMenuItem>()));
			Assert(docEvents.DocumentPrinted);

			//test to see that there is no exception if the event is not initialised
			docEvents = new DocumentEventsTestClass(false);
			documentEventsForMenu = new DocumentEventsForMenu(docEvents);
			documentEventsForMenu.NotifyDocumentPrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, Factory.New<StmMenuItem>()));
			Assert(true);
		}

		public void TestNotifyDocumentPrePreviewed()
		{
			var docEvents = new DocumentEventsTestClass();
			var documentEventsForMenu = new DocumentEventsForMenu(docEvents);

			docEvents.DocumentPrePreviewed = false;
			documentEventsForMenu.NotifyDocumentPrePreviewed(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, Factory.New<StmMenuItem>()));
			Assert(docEvents.DocumentPrePreviewed);

			//test to see that there is no exception if the event is not initialised
			docEvents = new DocumentEventsTestClass(false);
			documentEventsForMenu = new DocumentEventsForMenu(docEvents);
			documentEventsForMenu.NotifyDocumentPrePreviewed(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, Factory.New<StmMenuItem>()));
			Assert(true);
		}

		public void TestNotifyDocumentPrePrinted()
		{
			var docEvents = new DocumentEventsTestClass();
			var documentEventsForMenu = new DocumentEventsForMenu(docEvents);
			docEvents.DocumentPrePrinted = false;
			documentEventsForMenu.NotifyDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, Factory.New<StmMenuItem>()));
			Assert(docEvents.DocumentPrePrinted);

			//test to see that there is no exception if the event is not initialised
			docEvents = new DocumentEventsTestClass(false);
			documentEventsForMenu = new DocumentEventsForMenu(docEvents);
			documentEventsForMenu.NotifyDocumentPrePrinted(new DocumentPrintedEventArgs(DeliveryInstructionDestination.Print, Factory.New<StmMenuItem>()));
			Assert(true);
		}

		public void TestNotifyDocumentPrintRequested()
		{
			var docEvents = new DocumentEventsTestClass();
			var documentEventsForMenu = new DocumentEventsForMenu(docEvents);
			docEvents.CancelDocumentPrintRequested = true;
			documentEventsForMenu.NotifyDocumentPrintRequested(Factory.New<StmMenuItem>());
			Assert(documentEventsForMenu.CancelPrintRequest);

			docEvents.CancelDocumentPrintRequested = false;
			documentEventsForMenu.NotifyDocumentPrintRequested(Factory.New<StmMenuItem>());
			Assert(!documentEventsForMenu.CancelPrintRequest);

			docEvents.CancelDocumentPrintRequested = true;
			documentEventsForMenu.NotifyDocumentPrintRequested(Factory.New<StmMenuItem>());
			Assert(documentEventsForMenu.CancelPrintRequest);

			//test to see that there is no exception if the event is not initialised
			docEvents = new DocumentEventsTestClass(false);
			documentEventsForMenu = new DocumentEventsForMenu(docEvents);
			documentEventsForMenu.NotifyDocumentPrintRequested(Factory.New<StmMenuItem>());
			Assert(true);
		}

		#region DocumentEventsTestClass

		sealed class DocumentEventsTestClass : NonPersistentBusinessObject, IDocumentSupportable
		{
			public DocumentEventsTestClass()
				: this(true)
			{
			}

			public DocumentEventsTestClass(bool initialiseEvents)
			{
				this.InitialiseEvents = initialiseEvents;
			}

			public bool DocumentPrinted;
			void DocumentEventSource_DocumentPrinted(object sender, DocumentPrintedEventArgs e)
			{
				DocumentPrinted = true;
			}

			public bool DocumentPrePreviewed;
			void DocumentEventSource_DocumentPrePreviewed(object sender, DocumentPrintedEventArgs e)
			{
				DocumentPrePreviewed = true;
			}

			public bool DocumentPrePrinted;
			void DocumentEventSource_DocumentPrePrinted(object sender, DocumentPrintedEventArgs e)
			{
				DocumentPrePrinted = true;
			}

			public bool CancelDocumentPrintRequested;
			void DocumentEventSource_DocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				e.Cancel = CancelDocumentPrintRequested;
			}

			public DocumentSupporter DocumentSupporter
			{
				get { return new DocumentEventsTestClassDocumentSupporter(this); }
			}

			readonly bool InitialiseEvents;

			sealed class DocumentEventsTestClassDocumentSupporter : DocumentSupporter
			{
				public DocumentEventsTestClassDocumentSupporter(DocumentEventsTestClass documentEventsTestClass)
					: base(documentEventsTestClass)
				{
				}

				DocumentEventsTestClass DocumentEventsTestClass
				{
					get { return (DocumentEventsTestClass)BusinessObject; }
				}

				public override BusinessContext BusinessContext
				{
					get { return BusinessContext.Consol; }
				}

				protected override Enterprise.Core.Constants.DataContext[] GetSupportedDataContexts()
				{
					return null;
				}

				public override ISecurityCheckpoint CustomisationSecurityCheckpoint
				{
					get { return null; }
				}

				protected override DocumentWrapper[] GetDocumentWrappersInternal(Enterprise.Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
				{
					return null;
				}

				protected override void InitialiseCore(IDocumentEvents documentEventSource)
				{
					if (DocumentEventsTestClass.InitialiseEvents)
					{
						documentEventSource.DocumentPrinted += new DocumentPrintedEventHandler(DocumentEventsTestClass.DocumentEventSource_DocumentPrinted);
						documentEventSource.DocumentPrePreviewed += new DocumentPrintedEventHandler(DocumentEventsTestClass.DocumentEventSource_DocumentPrePreviewed);
						documentEventSource.DocumentPrePrinted += new DocumentPrintedEventHandler(DocumentEventsTestClass.DocumentEventSource_DocumentPrePrinted);
						documentEventSource.DocumentPrintRequested += new DocumentCancelEventHandler(DocumentEventsTestClass.DocumentEventSource_DocumentPrintRequested);
					}

					base.InitialiseCore(documentEventSource);
				}
			}
		}

		#endregion

		#endregion

		public void TestOnMenuShown()
		{
			DummyConsolBusinessObject dummyConsol = Factory.New<DummyConsolBusinessObject>();
			dummyConsol.Z0_Code = "DC1";
			dummyConsol.Factory.Save();

			using (ZFormForTesting form = new ZFormForTesting(dummyConsol))
			{
				form.PlugIns.Add(ControllerIDs.DocDataPlugIn);

				DocDataPlugIn docDataPlugin = (DocDataPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);

				AssertEquals("dummyConsol.HasChanges", false, dummyConsol.HasChanges);
				docDataPlugin.OnMenuShown();
				AssertNotContains("plugin.DocsMenu.MenuItems", "Please save your record before running documents.", GetMenuItemsToString(docDataPlugin.DocsMenu.MenuItems));

				dummyConsol.Z0_Code = "DC2";

				AssertEquals("dummyConsol.HasChanges", true, dummyConsol.HasChanges);
				docDataPlugin.OnMenuShown();
				AssertMultilineASCIIEquals("plugin.DocsMenu.MenuItems", "Please save your record before running documents.", GetMenuItemsToString(docDataPlugin.DocsMenu.MenuItems));

				form.Save();

				AssertEquals("dummyConsol.HasChanges", false, dummyConsol.HasChanges);
				docDataPlugin.OnMenuShown();
				AssertNotContains("plugin.DocsMenu.MenuItems", "Please save your record before running documents.", GetMenuItemsToString(docDataPlugin.DocsMenu.MenuItems));
			}
		}

		public void TestOnMenuShown_ShowCustomizeMenuItems_TabPageIsNull()
		{
			var dummyObject = new DummyNonPersistentBusinessObjectWithDocument(Factory);

			Factory.Save();

			using var form = new ZFormForTesting(dummyObject);
			form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
			form.PlugIns.Add(ControllerIDs.DocumentVisualizer);

			var docDataPlugin = (DocDataPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);

			docDataPlugin.OnMenuShown();

			var menuString = GetMenuItemsToString(docDataPlugin.DocsMenu.MenuItems);

			AssertContains("Customize (Documents) menu item exists", "Customize (Documents)", menuString);
			AssertContains("Customize (Forms) menu item exists", "Customize (Forms)", menuString);
		}

		public void TestOnMenuShown_ShowCustomizeMenuItems_TabPageFindFormIsNull()
		{
			var dummyShipment = Factory.New<DummyShipmentBusinessObject>();

			Factory.Save();

			using var form = new ZFormForTesting(dummyShipment);
			form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
			form.PlugIns.Add(ControllerIDs.DocumentVisualizer);

			var docDataPlugin = (DocDataPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);

			docDataPlugin.OnMenuShown();

			var menuString = GetMenuItemsToString(docDataPlugin.DocsMenu.MenuItems);

			AssertContains("Customize (Documents) menu item exists", "Customize (Documents)", menuString);
			AssertContains("Customize (Forms) menu item exists", "Customize (Forms)", menuString);
		}

		[RequiresSTA]
		public void TestOnMenuShown_ShowCustomizeMenuItems_TabPageFindFormIsNotNull()
		{
			var dummyConsol = Factory.New<DummyConsolBusinessObject>();

			Factory.Save();

			using var form = new DocumentUDFPlugInTest.TestForm(dummyConsol);
			form.MyTabControl.TabPages.Add(new ZTabPage());
			form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
			form.PlugIns.Add(ControllerIDs.DocumentVisualizer);
			form.Show();

			var docDataPlugin = (DocDataPlugIn)form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn);

			docDataPlugin.OnMenuShown();

			var menuString = GetMenuItemsToString(docDataPlugin.DocsMenu.MenuItems);

			AssertContains("Customize (Documents) menu item exists", "Customize (Documents)", menuString);
			AssertContains("Customize (Forms) menu item exists", "Customize (Forms)", menuString);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[RequiresSTA]
		public void TestIsUserReadOnlyAndHintLabel()
		{
			var dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";

			var sysConsolTemplate = Factory.New<StmTemplateBase>();
			sysConsolTemplate.SO_DataContext = nameof(Core.Constants.DataContext.Consol);
			sysConsolTemplate.SO_IsSystemDefined = true;
			sysConsolTemplate.SO_Name = "System Consol Template1";
			var exlTemplate1 = new ExcelTemplateForUnitTesting("UDF with tabs.xls", TestFilesSubFolder.ReportTestFiles);
			sysConsolTemplate.SO_Template = exlTemplate1.GetAsByteArray();

			var pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Consol Document";
			pubSysConsolMenu.SU_MenuIndex = 1;
			pubSysConsolMenu.SU_MenuPath = "";
			pubSysConsolMenu.SU_MenuShortcut = "CtrlF2";

			var pubSysConsolPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot.SI_SO = sysConsolTemplate.PK;
			pubSysConsolPivot.SI_SU = pubSysConsolMenu.PK;
			pubSysConsolPivot.SI_DocumentTitle = "Pub System Consol Document";

			Factory.Save();

			using (var testForm = new DocumentUDFPlugInTest.TestForm(dummyConsol1))
			{
				AssertHintLabel(testForm, false);

				var factory2 = new BusinessObjectFactory();
				var dummyConsol2 = factory2.Load<DummyConsolBusinessObject>(dummyConsol1.PK);
				using (var testForm2 = new DocumentUDFPlugInTest.TestForm(dummyConsol2))
				{
					AssertHintLabel(testForm2, true);
				}
			}

			void AssertHintLabel(DocumentUDFPlugInTest.TestForm testForm, bool expectedVisibility)
			{
				testForm.MinimumSize = new System.Drawing.Size(800, 185);
				testForm.PlugIns.Add(ControllerIDs.DocDataPlugIn);
				testForm.Show();
				UserIdleWorker.Flush();

				var testControl = (DocDataUserControl)testForm.PlugIns.Instances[0].UserControl;
				var docDataMainTabControl = testControl.Controls[0] as ZTabControl;
				var sdfPlugin = docDataMainTabControl.PlugIns.GetPlugIn(ControllerIDs.DocumentSDFPlugIn) as DocumentSDFPlugIn;
				var sDFHintLabel = ((DocumentSDFControl)sdfPlugin.UserControl).Controls[0] as ZLabel;
				AssertEquals("HintLabel", sDFHintLabel.Name);
				AssertEquals(expectedVisibility, sDFHintLabel.Visible);

				var udfPlugin = docDataMainTabControl.PlugIns.GetPlugIn(ControllerIDs.DocumentUDFPlugIn) as DocumentUDFPlugIn;
				udfPlugin.SelectTabPage();
				var mainTabControl = (ZTemplateTabControl)udfPlugin.UserControl.Controls[0];

				var alphaTabPage = mainTabControl.TabPages.OfType<UDFTabPage>().FirstOrDefault(t => t.Text == "Alpha");
				var alphaTabPageHintLabel = alphaTabPage.Controls[0] as ZLabel;
				mainTabControl.SelectTab(alphaTabPage);
				AssertEquals("HintLabel", alphaTabPageHintLabel.Name);
				AssertEquals(expectedVisibility, alphaTabPageHintLabel.Visible);

				var betaTabPage = mainTabControl.TabPages.OfType<UDFTabPage>().FirstOrDefault(t => t.Text == "Beta");
				var betaTabPageHintLabel = betaTabPage.Controls[0] as ZLabel;
				mainTabControl.SelectTab(betaTabPage);
				AssertEquals("HintLabel", betaTabPageHintLabel.Name);
				AssertEquals(expectedVisibility, betaTabPageHintLabel.Visible);
			}
		}

		public void TestIsUserReadOnlyWorkForCreatePlugInGUIAndSetReadOnlyChild()
		{
			DummyShipmentBusinessObject dummyShipment = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment.Z0_Code = "DS110";
			dummyShipment.Factory.Save();

			using (ZFormForTesting form = new ZFormForTesting(dummyShipment))
			{
				form.PlugIns.Add(ControllerIDs.DocDataPlugIn);
				using (TestDocDataPlugIn docDataPlugin = new TestDocDataPlugIn(dummyShipment))
				{
					docDataPlugin.OnMenuShown();
					AssertEquals(false, docDataPlugin.ShouldBeReadOnly);
					AssertEquals(true, docDataPlugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_Exposed());
					using (ZFormForTesting formNew = new ZFormForTesting(dummyShipment))
					{
						formNew.PlugIns.Add(ControllerIDs.DocDataPlugIn);
						using (TestDocDataPlugIn docDataPluginNew = new TestDocDataPlugIn(dummyShipment))
						{
							docDataPlugin.OnMenuShown();
							AssertEquals(true, docDataPluginNew.ShouldBeReadOnly);
							AssertEquals(true, docDataPluginNew.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_Exposed());
						}
					}
				}
			}
		}

		public void TestOnMenuShownWithLockedDocumentNote()
		{
			DummyShipmentBusinessObject dummyShipment = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment.Z0_Code = "DS1";
			dummyShipment.Factory.Save();

			using (ZFormForTesting form = new ZFormForTesting(dummyShipment))
			{
				form.PlugIns.Add(ControllerIDs.DocDataPlugIn);

				using (TestDocDataPlugIn docDataPlugin = new TestDocDataPlugIn(dummyShipment))
				{
					docDataPlugin.OnMenuShown();
					AssertNotNull(docDataPlugin.Note);
					AssertNotNull(docDataPlugin.NoteWithoutMutex);
					AssertNoteUDF(docDataPlugin.NoteWithoutMutex, dummyShipment);

					using (ZFormForTesting formNew = new ZFormForTesting(dummyShipment))
					{
						formNew.PlugIns.Add(ControllerIDs.DocDataPlugIn);
						using (TestDocDataPlugIn docDataPluginNew = new TestDocDataPlugIn(dummyShipment))
						{
							docDataPluginNew.OnMenuShown();
							AssertNull(docDataPluginNew.Note);
							AssertNotNull(docDataPluginNew.NoteWithoutMutex);
							AssertNoteUDF(docDataPlugin.NoteWithoutMutex, dummyShipment);
						}
					}
				}
			}
		}

		void AssertNoteUDF(DocumentNote actualNote, IStmNoteParent inputBusinessObject)
		{
			var expectedNote = DocumentNote.LoadNote(inputBusinessObject);

			AssertNotNull(expectedNote);
			AssertNotNull(actualNote.UserDefinedFieldList);
			AssertNotNull(expectedNote.UserDefinedFieldList);

			var expectedUDF = expectedNote.UserDefinedFieldList.Select(o => o.DisplayName);
			var actualUDF = actualNote.UserDefinedFieldList.Select(o => o.DisplayName);

			AssertContainsExactElementsInAnyOrder("User Control Provider List should have the same elements.", expectedUDF, actualUDF);
		}

		[RequiresSTA]
		public void TestPlugIn()
		{
			TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);

			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";

			AssertEquals("BusinessContext", BusinessContext.Consol, dummyConsol1.DocumentSupporter.BusinessContext);

			StmSystemDefinedFieldCollection sDFCollection = new StmSystemDefinedFieldCollection(Factory);
			StmSystemDefinedField textField1 = sDFCollection.AddNew();
			textField1.S1_BusinessContext = nameof(BusinessContext.Consol);
			textField1.S1_Name = "TextField1";
			textField1.S1_Type = StmSystemDefinedFieldLookups.TypeCodes.Text;
			textField1.S1_Hint = "TextField1 Hint";

			Factory.Save();

			using (DocumentUDFPlugInTest.TestForm testForm = new DocumentUDFPlugInTest.TestForm(dummyConsol1))
			{
				testForm.MinimumSize = new System.Drawing.Size(800, 185);
				testForm.PlugIns.Add(ControllerIDs.DocDataPlugIn);
				testForm.Show();

				using (DocDataPlugIn docDataPlugin = new DocDataPlugIn(dummyConsol1))
				{
					docDataPlugin.OnUserControlShown();

					DocumentNote note = DocumentNote.LoadNote(dummyConsol1);
					AssertEquals("Note.HasChanges", false, note.HasChanges);
				}
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestDropDownWhenObjectHasChanges()
		{
			var bizO = Factory.New<DummyConsolBusinessObject>();
			bizO.Z0_Code = "DC1";
			using (var form = new DocumentUDFPlugInTest.TestForm(bizO))
			{
				form.Show();
				using (var plugin = new DocDataPlugIn(bizO))
				{
					plugin.OnMenuShown();
				}
			}
		}

		[RequiresSTA]
		public void TestUpdateSDFsAndUDFsOnFactorySaving_SetWhenTabShown()
		{
			DummyConsolBusinessObject consol = Factory.New<DummyConsolBusinessObject>();
			using (DocumentUDFPlugInTest.TestForm testForm = new DocumentUDFPlugInTest.TestForm(consol))
			{
				testForm.MyTabControl.TabPages.Add(new Enterprise.ZArchitecture.GUI.ZTabPage());
				testForm.PlugIns.Add(ControllerIDs.DocDataPlugIn);
				testForm.Show();
				using (TestDocDataPlugIn docDataPlugin = new TestDocDataPlugIn(consol))
				{
					AssertEquals("UpdateSDFsAndUDFsOnFactorySaving before tab shown", false, docDataPlugin.Note.UpdateSDFsAndUDFsOnFactorySaving);
					docDataPlugin.OnUserControlShown();
					DocumentNote note = DocumentNote.LoadNote(consol);
					AssertEquals("UpdateSDFsAndUDFsOnFactorySaving after tab shown", true, docDataPlugin.Note.UpdateSDFsAndUDFsOnFactorySaving);
				}
			}
		}

		public void TestPluginDropDownMenu_WhenNoteCannotBeLoaded_ShouldStillShowDropdown()
		{
			var menuCustomisation = DocumentMenuCustomisation.New(Factory.New<DummyConsolBusinessObject>(), null);

			using (var plugin = new TestDocDataPlugIn(menuCustomisation))
			{
				AssertNull(plugin.Note);

				AssertEquals(false, plugin.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_Exposed());
				AssertEquals(true, plugin.ShouldPluginDropdownMenuBeCreated_Exposed());
			}
		}

		#region Test Classes

		class TestDocDataPlugIn : DocDataPlugIn
		{
			public TestDocDataPlugIn(IBusiness hostBusinessEntity)
				: base(hostBusinessEntity)
			{
			}

			public new DocumentNote Note
			{
				get { return base.Note; }
			}

			public new DocumentNote NoteWithoutMutex
			{
				get { return base.NoteWithoutMutex; }
			}

			internal bool ShouldPluginDropdownMenuBeCreated_Exposed()
			{
				return base.ShouldPluginDropdownMenuBeCreated();
			}

			internal bool QueryUserShouldPlugInGUIAndBusinessEntityBeCreated_Exposed()
			{
				return base.QueryUserShouldPlugInGUIAndBusinessEntityBeCreated();
			}
		}

		internal class ZFormForTesting : ZDummyForm
		{
			internal ZFormForTesting(IBusiness dataSource)
				: base(dataSource)
			{
			}

			internal void Save()
			{
				base.SaveInternal();
			}
		}

		internal class DummyNonPersistentBusinessObjectWithDocument : NonPersistentBusinessObject, IDocumentSupportable
		{
			public DummyNonPersistentBusinessObjectWithDocument(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public DocumentSupporter DocumentSupporter
			{
				get { return new DummyDocumentSupporter(this); }
			}
		}

		internal class DummyDocumentSupporter : DocumentSupporter
		{
			public DummyDocumentSupporter(BusinessObject parentBusinessObject) : base(parentBusinessObject)
			{
			}

			public override BusinessContext BusinessContext => new BusinessContext();
			public override ISecurityCheckpoint CustomisationSecurityCheckpoint => null;
			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun) => null;
			protected override Core.Constants.DataContext[] GetSupportedDataContexts() => null;
		}

		#endregion

		string GetMenuItemsToString(System.Windows.Forms.Menu.MenuItemCollection menuItems)
		{
			ZStringBuilder builder = new ZStringBuilder();

			foreach (MenuItem menuItem in menuItems)
			{
				builder.Append(menuItem.Text);
			}

			return builder.ToStringWithNewLineBetweenAppends();
		}
	}
}
