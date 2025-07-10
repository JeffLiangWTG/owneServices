using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.BuildTools.Testing;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.DbUpgrader.Data.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.ExcelTemplates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	abstract class ZDocumentMenuTest<T, U> : TestCaseWithFactory
			where T : IZDocumentMenuItem, IDisposable, new()
			where U : class
	{
		public void TestResetMenuItemsAreShowingWhenMenuMakerMenuItemsChanged()
		{
			using (var form = new ZForm(DocBusinessObject))
			using (var documentMenuItem = new T())
			{
				documentMenuItem.Setup(DocBusinessObject, null, null, null);
				documentMenuItem.LoadMenus(form);
				AssertNull("Pre-condition: documentMenuItem.MenuItems.FindByText(\"My Document\")", FindByText(documentMenuItem, "My Document"));

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_MenuName = "My Document";
				documentCommand.SU_IsSystemDefined = ZBool.True;
				documentCommand.SU_IsPublished = ZBool.True;
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
				Factory.Save();

				HelperForTesting(documentMenuItem).RaiseMenusMakerMenuItemsChangedForTesting();
				Assert(HelperForTesting(documentMenuItem).IsGetApplicalbeDocumentsCommandsCalled);
				AssertNotNull("documentMenuItem.MenuItems.FindByText(\"My Document\")", FindByText(documentMenuItem, "My Document"));
			}
		}

		[ExpectNoExceptions]
		public void TestDocumentPackWithInvalidFilterOnTemplatePivot()
		{
			using (var form = new ZForm(DocBusinessObject))
			using (var documentMenuItem = new T())
			{
				documentMenuItem.Setup(DocBusinessObject, null, null, null);
				documentMenuItem.LoadMenus(form);
				AssertNull("Pre-condition: documentMenuItem.MenuItems.FindByText(\"My Document\")", FindByText(documentMenuItem, "My Document"));

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_MenuName = "My Document";
				documentCommand.SU_IsSystemDefined = ZBool.True;
				documentCommand.SU_IsPublished = ZBool.True;
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);

				var template = Factory.New<StmTemplateBase>();
				template.SO_DataContext = nameof(Core.Constants.DataContext.Shipment);
				template.SO_Name = "Test Template";

				var pivot = Factory.New<StmMenuTemplatePivotBase>();
				pivot.SI_SO = template.PK;
				pivot.SI_SU = documentCommand.PK;
				pivot.SI_DocumentTitle = "TestDocForFilter";
				pivot.SI_MenuTemplateFilter = "\"<TransportMode>\"";
				Factory.Save();

				HelperForTesting(documentMenuItem).RaiseMenusMakerMenuItemsChangedForTesting();
				U menuItem = FindByText(documentMenuItem, "My Document");
				PerformClick(menuItem);
				var expectedMessage = string.Format(
@"The template filter or child document filter below is invalid. Please fix this filter in order to run this document:
Invalid filter expression:
Document name: {0}
Document path: {1}
Template name: {2}
Template title: {3}
Template filter: {4}", documentCommand.SU_MenuName, documentCommand.SU_MenuPath, pivot.SO_Name, pivot.DocumentTitle, pivot.SI_MenuTemplateFilter);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestDocumentPackWithInvalidFilterOnMenuItemPivot()
		{
			using (var form = new ZForm(DocBusinessObject))
			using (var documentMenuItem = new T())
			{
				documentMenuItem.Setup(DocBusinessObject, null, null, null);
				documentMenuItem.LoadMenus(form);
				AssertNull("Pre-condition: documentMenuItem.MenuItems.FindByText(\"My Document\")", FindByText(documentMenuItem, "My Document"));

				var documentCommand = Factory.New<DocumentCommand>();
				documentCommand.SU_MenuName = "My Document";
				documentCommand.SU_IsSystemDefined = ZBool.True;
				documentCommand.SU_IsPublished = ZBool.True;
				documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);

				var documentCommand2 = Factory.New<DocumentCommand>();
				documentCommand2.SU_MenuName = "My Document 2";
				documentCommand2.SU_IsSystemDefined = ZBool.True;
				documentCommand2.SU_IsPublished = ZBool.True;
				documentCommand2.SU_BusinessContext = nameof(BusinessContext.Shipment);

				var pivot = Factory.New<StmMenuMenuPivotBase>();
				pivot.SF_SU_Inward = documentCommand.PK;
				pivot.SF_SU_Outward = documentCommand2.PK;
				pivot.SF_Filter = "\"<TransportMode>\"";
				Factory.Save();

				HelperForTesting(documentMenuItem).RaiseMenusMakerMenuItemsChangedForTesting();
				U menuItem = FindByText(documentMenuItem, "My Document");
				PerformClick(menuItem);
				var expectedMessage = string.Format(
@"The template filter or child document filter below is invalid. Please fix this filter in order to run this document:
Invalid filter expression:
Document name: {0}
Document path: {1}
Child document name: {2}
Child document filter: {3}", documentCommand.SU_MenuName, documentCommand.SU_MenuPath, documentCommand2.SU_MenuName, pivot.SF_Filter);
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestMenuItemsAreOnlyReBuiltWhenRequired()
		{
			var dummyConsol = Factory.New<DummyConsolBusinessObject>();
			dummyConsol.Z0_Code = "DC1";
			dummyConsol.Factory.Save();

			using (var form = new ZForm(dummyConsol))
			{
				using (var documentMenuItem = new T())
				{
					documentMenuItem.Setup(dummyConsol, null, null, null);

					AssertEquals("Pre-condition: documentMenuItem.IsMenusLoaded", false, HelperForTesting(documentMenuItem).IsRebuildMenuItemsCalled);

					documentMenuItem.LoadMenus(form);
					AssertEquals("MenuItems should be rebuilt as it has not been built yet.", true, HelperForTesting(documentMenuItem).IsRebuildMenuItemsCalled);
					ResetIsRebuildMenuItemsCalled(documentMenuItem);

					documentMenuItem.LoadMenus(form);
					AssertEquals("MenuItems should not be rebuilt as nothing has changed.", false, HelperForTesting(documentMenuItem).IsRebuildMenuItemsCalled);
					ResetIsRebuildMenuItemsCalled(documentMenuItem);

					dummyConsol.Z0_Code = "DC2";

					documentMenuItem.LoadMenus(form);
					AssertEquals("MenuItems should not be rebuilt as parent has changed but not saved.", false, HelperForTesting(documentMenuItem).IsRebuildMenuItemsCalled);
					ResetIsRebuildMenuItemsCalled(documentMenuItem);

					dummyConsol.Factory.Save();

					documentMenuItem.LoadMenus(form);
					AssertEquals("MenuItems should be rebuilt as parent has just been saved.", true, HelperForTesting(documentMenuItem).IsRebuildMenuItemsCalled);
					ResetIsRebuildMenuItemsCalled(documentMenuItem);

					documentMenuItem.LoadMenus(form);
					AssertEquals("MenuItems should not be rebuilt as nothing has changed.", false, HelperForTesting(documentMenuItem).IsRebuildMenuItemsCalled);
				}
			}
		}

		[GuiTest]
		public void TestDisposeOnCustomizeFormCalledAfterDisposeOnMenuItemDoesNotTriggerException()
		{
			using (var form = new ZForm(DocBusinessObject))
			using (var documentMenuItem = new T())
			{
				documentMenuItem.Setup(DocBusinessObject, null, null, null);
				documentMenuItem.LoadMenus(form);

				var customiseMenuItem = FindByText(documentMenuItem, "Customize");
				AssertNotNull("Precondition: 'Customize' MenuItem should exist", customiseMenuItem);

				PerformClick(customiseMenuItem);
			}
		}

		[ExpectException(typeof(InvalidOperationException))]
		[RequiresSTA]
		public void TestCallingLoadBeforeSetup()
		{
			using (var testForm = new ZForm(DocBusinessObject))
			using (var documentMenuItem = new T())
			{
				documentMenuItem.LoadMenus(testForm);
				documentMenuItem.Setup(DocBusinessObject, null, null, null);
			}
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestSetupThrowsExceptionOnNullParentBizObj()
		{
			using (var testForm = new ZForm(DocBusinessObject))
			using (var documentMenuItem = new T())
			{
				documentMenuItem.Setup(null, null, null, null);
			}
		}

		public void TestDocumentEventsForMenuIsNotNullIfWeHaveAParentBusinessObject()
		{
			using (var testForm = new ZForm(DocBusinessObject))
			using (var documentMenuItem = new T())
			{
				documentMenuItem.Setup(DocBusinessObject, null, null, null);
				AssertEquals("DocumentEventsForMenu should not have been created yet as we passed in null", null, HelperForTesting(documentMenuItem).DocumentEventsForMenuForTesting);
				documentMenuItem.LoadMenus(testForm);

				AssertEquals("Should have some menu items", true, documentMenuItem.Items.Count > 0);
				AssertNotEquals("DocumentEventsForMenu should have been created", null, HelperForTesting(documentMenuItem).DocumentEventsForMenuForTesting);
			}
		}

		public void TestDuplicatedSDFandUDFFieldsDontExplode()
		{
			using (var menuItem = new T())
			{
				var userDefinedFieldList = new UserControlProviderList();

				var userDefinedField1 = new TextField(Factory);
				userDefinedField1.DisplayName = "Field 1";
				userDefinedFieldList.Add(userDefinedField1);

				var userDefinedField3 = new TextField(Factory);
				userDefinedField3.DisplayName = "Field 3";
				userDefinedFieldList.Add(userDefinedField3);

				var systemDefinedFieldList = new UserControlProviderList();

				var systemDefinedField1 = new TextField(Factory);
				systemDefinedField1.DisplayName = "Field 1";
				systemDefinedFieldList.Add(systemDefinedField1);

				var systemDefinedField2 = new TextField(Factory);
				systemDefinedField2.DisplayName = "Field 2";
				systemDefinedFieldList.Add(systemDefinedField2);

				menuItem.Setup(DocBusinessObject, null, userDefinedFieldList, systemDefinedFieldList);

				var documentCommand = Factory.New<DocumentCommand>();
				AssertNoExceptionThrown(() => HelperForTesting(menuItem).GetDocumentPrintSetForTesting(documentCommand));
			}
		}

		public void TestGetModuleIDFromZFilterGrid()
		{
			using (var testForm = new TestForm())
			{
				ZFilterGrid grid = new ZFilterGrid();
				testForm.Controls.Add(grid);
				grid.SetModuleId(ModuleId.Organisation.GetModuleID());

				ZMenuItem menuItemForTestingModuleID = new ZMenuItem();
				menuItemForTestingModuleID.Click += (object sender, EventArgs e) =>
				{
					var helper = new ZDocumentsMenuItemMenuHelper(menuItemForTestingModuleID);
					DocumentRunner runner = helper.GetNewDocumentRunner(sender);
					AssertEquals(ModuleId.Organisation.GetModuleID(), runner.moduleIDForSecurity);
					grid.ContextMenu.Dispose();
				};
				grid.ContextMenu.MenuItems.Add(menuItemForTestingModuleID);

				grid.Visible = true;
				testForm.Show();

				grid.ContextMenu.Popup += (_, x_) =>
				{
					menuItemForTestingModuleID.PerformClick();
				};
				grid.ContextMenu.Show(grid, grid.Location);
			}
		}

		[GuiTest]
		public void TestLoadingDocumentMenusOnClick()
		{
			#region Templates

			var sysShipmentTemplate = Factory.New<StmTemplateBase>();
			sysShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";

			var userShipmentTemplate = Factory.New<StmTemplateBase>();
			userShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			userShipmentTemplate.SO_IsSystemDefined = false;
			userShipmentTemplate.SO_Name = "User Shipment Template";

			var sysConsolTemplate = Factory.New<StmTemplateBase>();
			sysConsolTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Consol);
			sysConsolTemplate.SO_IsSystemDefined = true;
			sysConsolTemplate.SO_Name = "System Consol Template";

			#endregion

			#region Published System Shipment

			var pubSysShipmentMenu = Factory.New<DocumentCommand>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			pubSysShipmentMenu.SU_MenuIndex = 0;
			pubSysShipmentMenu.SU_MenuPath = "";
			pubSysShipmentMenu.SU_MenuShortcut = "F1";

			var pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Pub System Shipment Document";

			var legacyPubSysShipmentMenu = Factory.New<DocumentCommand>();
			legacyPubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			legacyPubSysShipmentMenu.SU_IsPublished = true;
			legacyPubSysShipmentMenu.SU_IsSystemDefined = true;
			legacyPubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			legacyPubSysShipmentMenu.SU_MenuIndex = 0;
			legacyPubSysShipmentMenu.SU_MenuPath = "Legacy Documents/";
			legacyPubSysShipmentMenu.SU_MenuShortcut = "F1";

			#endregion

			#region UnPublished System Shipment

			var unPubSysShipmentMenu = Factory.New<DocumentCommand>();
			unPubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			unPubSysShipmentMenu.SU_IsSystemDefined = true;
			unPubSysShipmentMenu.SU_IsPublished = false;
			unPubSysShipmentMenu.SU_MenuName = "UnPub System Shipment Document";
			unPubSysShipmentMenu.SU_MenuIndex = 1;
			unPubSysShipmentMenu.SU_MenuPath = "";
			unPubSysShipmentMenu.SU_MenuShortcut = "F1";

			var unPubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			unPubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			unPubSysShipmentPivot1.SI_SU = unPubSysShipmentMenu.PK;
			unPubSysShipmentPivot1.SI_DocumentTitle = "UnPub System Shipment Document";

			#endregion

			#region Published System Consol

			var pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Pub System Consol Document";
			pubSysConsolMenu.SU_MenuIndex = 2;
			pubSysConsolMenu.SU_MenuPath = "";
			pubSysConsolMenu.SU_MenuShortcut = "F2";

			var pubSysConsolPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot.SI_SO = sysConsolTemplate.PK;
			pubSysConsolPivot.SI_SU = pubSysConsolMenu.PK;
			pubSysConsolPivot.SI_DocumentTitle = "Pub System Consol Document";

			#endregion

			#region Published UserDefined Shipment

			var pubUserShipmentMenu = Factory.New<DocumentCommand>();
			pubUserShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubUserShipmentMenu.SU_IsPublished = true;
			pubUserShipmentMenu.SU_IsSystemDefined = false;
			pubUserShipmentMenu.SU_MenuName = "Pub User Shipment Document";
			pubUserShipmentMenu.SU_MenuIndex = 3;
			pubUserShipmentMenu.SU_MenuPath = "";
			pubUserShipmentMenu.SU_MenuShortcut = "F3";

			var pubUserShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubUserShipmentPivot1.SI_SO = userShipmentTemplate.PK;
			pubUserShipmentPivot1.SI_SU = pubUserShipmentMenu.PK;
			pubUserShipmentPivot1.SI_DocumentTitle = "Pub User Shipment Document";

			#endregion

			#region Private UserDefined Shipment for current user

			var privateUserShipmentMenu = Factory.New<DocumentCommand>();
			privateUserShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			privateUserShipmentMenu.SU_IsPublished = false;
			privateUserShipmentMenu.SU_IsSystemDefined = false;
			privateUserShipmentMenu.SU_MenuName = "Private User Shipment Document";
			privateUserShipmentMenu.SU_MenuIndex = 4;
			privateUserShipmentMenu.SU_MenuPath = "";
			privateUserShipmentMenu.SU_MenuShortcut = "F4";
			privateUserShipmentMenu.SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;

			var privateUserShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			privateUserShipmentPivot1.SI_SO = userShipmentTemplate.PK;
			privateUserShipmentPivot1.SI_SU = privateUserShipmentMenu.PK;
			privateUserShipmentPivot1.SI_DocumentTitle = "Private User Shipment Document";

			#endregion

			#region Private UserDefined Shipment for some other user

			var privateOtherUserShipmentMenu = Factory.New<DocumentCommand>();
			privateOtherUserShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			privateOtherUserShipmentMenu.SU_IsPublished = false;
			privateOtherUserShipmentMenu.SU_IsSystemDefined = false;
			privateOtherUserShipmentMenu.SU_MenuName = "Private Other User Shipment Document";
			privateOtherUserShipmentMenu.SU_MenuPath = "";
			privateOtherUserShipmentMenu.SU_MenuIndex = 5;
			privateOtherUserShipmentMenu.SU_MenuShortcut = "F5";
			privateOtherUserShipmentMenu.SU_GS_NKStaffCode = "PM";

			var privateUserShipmentPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			privateUserShipmentPivot2.SI_SO = userShipmentTemplate.PK;
			privateUserShipmentPivot2.SI_SU = privateOtherUserShipmentMenu.PK;
			privateUserShipmentPivot2.SI_DocumentTitle = "Private Other User Shipment Document";

			#endregion

			#region	Published System Specialised Shipment that should be shown in Special Folder

			var pubSysSpecialisedShipmentMenu = Factory.New<DocumentCommand>();
			pubSysSpecialisedShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysSpecialisedShipmentMenu.SU_IsPublished = true;
			pubSysSpecialisedShipmentMenu.SU_IsSystemDefined = true;
			pubSysSpecialisedShipmentMenu.SU_MenuName = "Pub Sys Specialised Shipment Document";
			pubSysSpecialisedShipmentMenu.SU_MenuIndex = 1;
			pubSysSpecialisedShipmentMenu.SU_MenuPath = "Special";
			pubSysSpecialisedShipmentMenu.SU_MenuShortcut = "F6";
			pubSysSpecialisedShipmentMenu.SU_FilterList = "BUY=" + GlbStaff.CurrentUser.PK.ToString(); //any PK will do for testing.

			var pubSpecialisedShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSpecialisedShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSpecialisedShipmentPivot1.SI_SU = pubSysSpecialisedShipmentMenu.PK;
			pubSpecialisedShipmentPivot1.SI_DocumentTitle = "Pub Sys Specialised Shipment Document";

			#endregion

			#region Published System Specialised Shipment that should NOT be shown

			var pubSysOtherSpecialisedShipmentMenu = Factory.New<DocumentCommand>();
			pubSysOtherSpecialisedShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysOtherSpecialisedShipmentMenu.SU_IsPublished = true;
			pubSysOtherSpecialisedShipmentMenu.SU_IsSystemDefined = true;
			pubSysOtherSpecialisedShipmentMenu.SU_MenuName = "Pub Sys Other Special Shipment Document";
			pubSysOtherSpecialisedShipmentMenu.SU_MenuIndex = 2;
			pubSysOtherSpecialisedShipmentMenu.SU_MenuPath = "Special";
			pubSysOtherSpecialisedShipmentMenu.SU_MenuShortcut = "F7";
			pubSysOtherSpecialisedShipmentMenu.SU_FilterList = "BUY=" + Guid.NewGuid().ToString(); //any PK will do for testing.

			var pubSysOtherSpecialisedShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysOtherSpecialisedShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysOtherSpecialisedShipmentPivot1.SI_SU = pubSysOtherSpecialisedShipmentMenu.PK;
			pubSysOtherSpecialisedShipmentPivot1.SI_DocumentTitle = "Pub Sys Other Special Shipment Document";

			#endregion

			#region Private User Specialised Shipment that should be shown in Special\Private Folder

			var privateUserSpecialisedShipmentMenu = Factory.New<DocumentCommand>();
			privateUserSpecialisedShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			privateUserSpecialisedShipmentMenu.SU_IsPublished = false;
			privateUserSpecialisedShipmentMenu.SU_IsSystemDefined = false;
			privateUserSpecialisedShipmentMenu.SU_MenuName = "Pri User Special Shipment Document";
			privateUserSpecialisedShipmentMenu.SU_MenuIndex = 1;
			privateUserSpecialisedShipmentMenu.SU_MenuPath = "Special/Private";
			privateUserSpecialisedShipmentMenu.SU_MenuShortcut = "F8";

			privateUserSpecialisedShipmentMenu.SU_FilterList = "BUY=" + GlbStaff.CurrentUser.PK.ToString(); //any PK will do for testing.
			privateUserSpecialisedShipmentMenu.SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;

			var privateUserSpecialisedShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			privateUserSpecialisedShipmentPivot1.SI_SO = userShipmentTemplate.PK;
			privateUserSpecialisedShipmentPivot1.SI_SU = privateUserSpecialisedShipmentMenu.PK;
			privateUserSpecialisedShipmentPivot1.SI_DocumentTitle = "Pr User Special Shipment Document";

			#endregion

			#region Two Published System Shipment - One Under Import Folder and One Under Brokerage\Import

			var importPubSysShipmentMenu = Factory.New<DocumentCommand>();
			importPubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			importPubSysShipmentMenu.SU_IsPublished = true;
			importPubSysShipmentMenu.SU_IsSystemDefined = true;
			importPubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			importPubSysShipmentMenu.SU_MenuIndex = 0;
			importPubSysShipmentMenu.SU_MenuPath = "Import";
			importPubSysShipmentMenu.SU_MenuShortcut = "F1";

			var importPubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			importPubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			importPubSysShipmentPivot1.SI_SU = importPubSysShipmentMenu.PK;
			importPubSysShipmentPivot1.SI_DocumentTitle = "Pub System Shipment Document";

			var brokerageImportPubSysShipmentMenu = Factory.New<DocumentCommand>();
			brokerageImportPubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			brokerageImportPubSysShipmentMenu.SU_IsPublished = true;
			brokerageImportPubSysShipmentMenu.SU_IsSystemDefined = true;
			brokerageImportPubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			brokerageImportPubSysShipmentMenu.SU_MenuIndex = 0;
			brokerageImportPubSysShipmentMenu.SU_MenuPath = "Brokerage/Import";
			brokerageImportPubSysShipmentMenu.SU_MenuShortcut = "F1";

			var brokerageImportPubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			brokerageImportPubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			brokerageImportPubSysShipmentPivot1.SI_SU = brokerageImportPubSysShipmentMenu.PK;
			brokerageImportPubSysShipmentPivot1.SI_DocumentTitle = "Pub System Shipment Document";

			var legacyBrokerageImportPubSysShipmentMenu = Factory.New<DocumentCommand>();
			legacyBrokerageImportPubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			legacyBrokerageImportPubSysShipmentMenu.SU_IsPublished = true;
			legacyBrokerageImportPubSysShipmentMenu.SU_IsSystemDefined = true;
			legacyBrokerageImportPubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			legacyBrokerageImportPubSysShipmentMenu.SU_MenuIndex = 0;
			legacyBrokerageImportPubSysShipmentMenu.SU_MenuPath = "Legacy Documents/Brokerage/Import";
			legacyBrokerageImportPubSysShipmentMenu.SU_MenuShortcut = "F1";

			#endregion

			Factory.Save();

			var factory2 = new BusinessObjectFactory();

			var docDummy = factory2.New<DocDummyBusinessObject>();
			docDummy.Z0_Code = "Tst";
			docDummy.Z0_Description = "Desc";

			factory2.Save();

			using (var testForm = new TestForm(docDummy))
			{
				var docMenuItem = new T();
				docMenuItem.Setup(docDummy, docDummy, null, null);
				AddToMainMenu(testForm, docMenuItem);
				testForm.Show();

				AssertEquals("No documents should be loaded to menu yet", 0, docMenuItem.Items.Count);
				docMenuItem.LoadMenus(testForm);

				AssertMultilineASCIIEquals("GetMenuItemDetails",
	@"Text:[&Documents]
    Text:[Pub System Shipment Document]  Shortcut:[F1]
    Text:[Pub User Shipment Document <Customized>]  Shortcut:[F3]
    Text:[Private User Shipment Document <Private>]  Shortcut:[F4]
    Text:[Private Other User Shipment Document <Private>]  Shortcut:[F5]
    Text:[Brokerage]
        Text:[Import]
            Text:[Pub System Shipment Document]  Shortcut:[F1]
    Text:[Import]
        Text:[Pub System Shipment Document]  Shortcut:[F1]
    Text:[Special]
        Text:[Pub Sys Specialised Shipment Document]  Shortcut:[F6]
        Text:[Private]
            Text:[Pri User Special Shipment Document <Private>]  Shortcut:[F8]
    Text:[-]
    Text:[Un-published Documents]
        Text:[UnPub System Shipment Document]  Shortcut:[F1]  [Disabled]
    Text:[-]
    Text:[Legacy Documents]
        Text:[Pub System Shipment Document]  Shortcut:[F1]
        Text:[Brokerage]
            Text:[Import]
                Text:[Pub System Shipment Document]  Shortcut:[F1]
    Text:[-]
    Text:[Customize]
    Text:[-]
    Text:[Mark All Documents Editable]
    Text:[Mark Documents Editable For Client]
    Text:[Save Documents Config Changes]  [Disabled]
    Text:[Undo All Changes]  [Disabled]
    Text:[Regenerate all clients Documents.xml]
    Text:[System Defined Fields]",
					GetMenuItemDetails(docMenuItem as U, menuItem => GetText(menuItem) != "Mark Documents Editable For Client").Trim());
			}
		}

		[GuiTest]
		public void TestTranslatedParentMenuItemsAreNotDuplicated()
		{
			using (var mockGermanData = Res.GetLanguageInstance(Core.SharedConstants.Languages.German).UseMockData())
			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.German))
			{
				var item = Factory.New<DocumentCommand>();

				string resKey = item.SU_MenuPathInfo.CustomizableDataResourceStrings.GetMultilingualString(item, "Import").ResourceKey;
				mockGermanData.Put(resKey, new ResourceStringData(resKey, "Tropmi"));

				item.SU_BusinessContext = nameof(BusinessContext.Shipment);
				item.SU_IsPublished = true;
				item.SU_IsSystemDefined = true;
				item.SU_MenuName = "First Document";
				item.SU_MenuIndex = 0;
				item.SU_MenuPath = "Import";

				item = Factory.New<DocumentCommand>();
				item.SU_BusinessContext = nameof(BusinessContext.Shipment);
				item.SU_IsPublished = true;
				item.SU_IsSystemDefined = true;
				item.SU_MenuName = "Second Document";
				item.SU_MenuIndex = 1;
				item.SU_MenuPath = "Import";

				Factory.Save();

				var factory2 = new BusinessObjectFactory();

				var docDummy = factory2.New<DocDummyBusinessObject>();
				docDummy.Z0_Code = "Tst";
				docDummy.Z0_Description = "Desc";

				factory2.Save();

				using (var testForm = new TestForm(docDummy))
				{
					var docMenuItem = new T();
					docMenuItem.Setup(docDummy, docDummy, null, null);
					AddToMainMenu(testForm, docMenuItem);
					testForm.Show();

					AssertEquals("No documents should be loaded to menu yet", 0, docMenuItem.Items.Count);
					docMenuItem.LoadMenus(testForm);

					AssertMultilineASCIIEquals("GetMenuItemDetails",
		@"Text:[&Documents]
    Text:[Tropmi]
        Text:[First Document]
        Text:[Second Document]
    Text:[-]
    Text:[Customize]
    Text:[-]
    Text:[Mark All Documents Editable]
    Text:[Mark Documents Editable For Client]
    Text:[Save Documents Config Changes]  [Disabled]
    Text:[Undo All Changes]  [Disabled]
    Text:[Regenerate all clients Documents.xml]
    Text:[System Defined Fields]",
						GetMenuItemDetails(docMenuItem as U, menuItem => GetText(menuItem) != "Mark Documents Editable For Client").Trim());
				}
			}
		}

		#region GetMenuItemDetails

		string GetMenuItemDetails(U menuItem, Predicate<U> showChildrenPredicate)
		{
			return GetMenuItemDetails(menuItem, showChildrenPredicate, 0);
		}

		string GetMenuItemDetails(U menuItem, Predicate<U> showChildrenPredicate, int level)
		{
			var result = new StringBuilder();
			var shortcut = GetShortCut(menuItem);

			for (var index = 0; index < level; index++)
			{
				result.Append("    ");
			}

			result.AppendLine(string.Format("Text:[{0}]{1}{2}",
				GetText(menuItem),
				shortcut != "None" ? string.Format("  Shortcut:[{0}]", shortcut) : string.Empty,
				GetEnabled(menuItem) ? string.Empty : "  [Disabled]"));

			if (showChildrenPredicate == null || showChildrenPredicate(menuItem))
			{
				foreach (var childMenuItem in GetItems(menuItem))
				{
					var wMenuItem = GetMenuItem(childMenuItem);
					if (wMenuItem != null)
					{
						result.Append(GetMenuItemDetails(wMenuItem, showChildrenPredicate, level + 1));
					}
					else
					{
						for (var index = 0; index < level + 1; index++)
						{
							result.Append("    ");
						}

						result.AppendLine("Text:[-]");
					}
				}
			}

			return result.ToString();
		}

		protected virtual U GetMenuItem(object childMenuItem)
		{
			return childMenuItem as U;
		}

		#endregion

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestTooManyRowsExceptionIsHandled()
		{
			var customizedTemplate = Factory.New<StmTemplateBase>();
			customizedTemplate.SO_DataContext = "GenericFreightJob";
			customizedTemplate.SO_Name = "Customized Document Elements";
			var customizedExcelTemplate = new ExcelTemplateForUnitTesting("Customized Document Elements With Too Many Rows in One Section.xlsx", TestFilesSubFolder.DocumentTestFiles);
			customizedTemplate.SO_Template = customizedExcelTemplate.GetAsByteArray();

			var systemTemplate = Factory.New<StmTemplateBase>();
			systemTemplate.SO_DataContext = "GenericFreightJob";
			systemTemplate.SO_Name = "System Document Elements";
			var systemExcelTemplate = new ExcelTemplateForUnitTesting("Architecture/content/DocumentEngine/TestFiles/TemplateGeneratorSystem.xls", TestFilesSubFolder.DocumentTestFiles);
			systemTemplate.SO_Template = systemExcelTemplate.GetAsByteArray();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
			documentCommand.SU_IsPublished = true;
			documentCommand.SU_MenuName = "Booking Summary";

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = systemTemplate.PK;
			document.SI_SU = documentCommand.PK;

			var docConfig = document.DocConfigs.AddNew();
			docConfig.ConfigItems.AddFromTemplateSection(systemTemplate.TemplateSections.Find("Recipient with Consignee + Terms"));
			docConfig.ConfigItems.AddFromTemplateSection(customizedTemplate.TemplateSections.Find("Master Bill + Client/Booking Reference + Carrier"));

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var docDummy = factory2.New<DocDummyBusinessObject>();
			docDummy.Z0_Code = "Tst";
			docDummy.Z0_Description = "Desc";

			factory2.Save();

			using (var testForm = new TestForm(docDummy))
			{
				((IZForm)testForm).ControllerID = ControllerIDs.QuotedBookings;
				using (var docMenuItem = new T())
				{
					docMenuItem.Setup(docDummy, docDummy, null, null);
					AddToMainMenu(testForm, docMenuItem);
					testForm.Show();

					docMenuItem.LoadMenus(testForm);
					AssertEquals("Booking Summary Menu", "Booking Summary <Customized>", GetText((U)docMenuItem.Items[0]));
					AssertNoExceptionThrown(() => PerformClick((U)docMenuItem.Items[0]));

					AssertEquals(@"Error generating the template when delivering the following document menu item:
	Menu Path: 
	Menu Name: Booking Summary
	System Defined: N
	Parent: DummyBizo

The following error occurred:
Too many rows (1048577): The maximum number of rows supported by this file format is 1048576.
Please check your Customized Document Elements template.", UnitTestUserNotification.Instance.LastMessage.Text);

					UnitTestUserNotification.Instance.ClearMessages();
				}
			}
		}

		[GuiTest]
		public void TestRunningPublicSystemSingleDocument()
		{
			var pubSysShipmentMenu = Factory.New<DocumentCommand>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "Booking Summary";
			pubSysShipmentMenu.SU_MenuPath = "";
			pubSysShipmentMenu.SU_MenuShortcut = nameof(Shortcut.F1);

			var pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot1.SI_SO = ShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Test Booking Summary";

			var pubSysShipmentPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot2.SI_SO = ShipmentTemplate.PK;
			pubSysShipmentPivot2.SI_SU = pubSysShipmentMenu.PK;
			pubSysShipmentPivot2.SI_DocumentTitle = "Test Booking Summary";

			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var docDummy = factory2.New<DocDummyBusinessObject>();
			docDummy.Z0_Code = "Tst";
			docDummy.Z0_Description = "Desc";

			factory2.Save();

			using (var testForm = new TestForm(docDummy))
			{
				((IZForm)testForm).ControllerID = ControllerIDs.QuotedBookings;
				using (var docMenuItem = new T())
				{
					docMenuItem.Setup(docDummy, docDummy, null, null);
					AddToMainMenu(testForm, docMenuItem);
					testForm.Show();

					docMenuItem.LoadMenus(testForm);
					AssertEquals("Booking Summary Menu", "Booking Summary", GetText((U)docMenuItem.Items[0]));

					AssertEquals("Pre-condition: LastRunDocumentPackInfos.Count", 0, HelperForTesting(docMenuItem).LastRunReportInfos.Count);
					PerformClick((U)docMenuItem.Items[0]);

					AssertEquals("LastRunDocumentPackInfos.Count", 2, HelperForTesting(docMenuItem).LastRunReportInfos.Count);

					AssertEquals("ReportName1", pubSysShipmentPivot1.SI_DocumentTitle, HelperForTesting(docMenuItem).LastRunReportInfos[0].PivotTitle);
					AssertEquals("TemplateName1", ShipmentTemplate.SO_Name, HelperForTesting(docMenuItem).LastRunReportInfos[0].TemplateName);

					AssertEquals("ReportName2", pubSysShipmentPivot2.SI_DocumentTitle, HelperForTesting(docMenuItem).LastRunReportInfos[1].PivotTitle);
					AssertEquals("TemplateName2", ShipmentTemplate.SO_Name, HelperForTesting(docMenuItem).LastRunReportInfos[1].TemplateName);
				}
			}
		}

		[GuiTest]
		public void TestMenusLoadsAWarningMessageBusinessObjectIsInEdit()
		{
			DocBusinessObject.HasChanges = true;
			using (var testForm = new TestForm(DocBusinessObject))
			{
				DocumentMenuItem.Setup(DocBusinessObject, DocBusinessObject, null, null);
				AddToMainMenu(testForm, DocumentMenuItem);
				testForm.Show();

				DocumentMenuItem.LoadMenus(testForm);
				AssertEquals("Menu Count", 1, DocumentMenuItem.Items.Count);
				AssertEquals("Warning Message", "Please save your record before running documents.", GetText((U)DocumentMenuItem.Items[0]));
			}
		}

		public void TestOnMenuClickDoesNotSwallowException()
		{
			var command = Factory.New<DocumentCommand>();
			var menuItem = new T();
			HelperForTesting(menuItem).ThrowExceptionInGetDocumentPrintSetForTesting = () => throw new DocumentEngineExceptionForTesting("ExceptionInGetDocumentPrintSetForTesting");
			var taggedMenuItem = HelperForTesting(menuItem).CreateMenuForTesting(command);

			AssertExceptionThrown(typeof(DocumentEngineException), () => PerformClick(taggedMenuItem));
		}

		[GuiTest]
		public void TestZCannotSaveExceptionHandling()
		{
			var message = "Error occurred";
			AssertSaveExceptionHandling(() => throw new ZCannotSaveException(message, "BLAH"), message);
		}

		[GuiTest]
		public void TestZSaveExceptionHandling()
		{
			var message = "Error occurred";
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();
			var row = ((INeedRow)dummy).Row;
			var innerEx = new ZDataException(new Exception(message), row, Db.Connection);
			innerEx.SetFriendlyMessageForTest(message);
			AssertSaveExceptionHandling(() => throw new ZSaveException(innerEx, factory), message);
		}

		void AssertSaveExceptionHandling(Action throwException, string expectedMessage)
		{
			var command = Factory.New<DocumentCommand>();
			var menuItem = new T();
			HelperForTesting(menuItem).ThrowExceptionInGetDocumentPrintSetForTesting = throwException;
			var taggedMenuItem = HelperForTesting(menuItem).CreateMenuForTesting(command);
			var form = this.FormWithLoadedDocumentMenuItem;
			AddToMainMenu(form, menuItem);
			DocumentMenuItem.Items.Add(taggedMenuItem);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			Factory.Save();
			PerformClick(taggedMenuItem);
			AssertEquals("Error Message", expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		[GuiTest]
		public void TestGetDataStateBeforeRun_CreditControlGUIWrapperIsInitialised()
		{
			ShipmentDocumentCommand.SU_MenuName = "Menu Item That Causes Error";
			ShipmentPivot.Factory.Save();
			var formWithLoadedDocumentMenuItem = this.FormWithLoadedDocumentMenuItem;
			var creditControlledGUIManager = new TestDocumentDeliveryRestrictionGUIManager();

			using (ObjectFactory.Substitute<IDocumentDeliveryRestrictionGUIManager>(creditControlledGUIManager))
			{
				AssertEquals("Precondition: InitialiseWasCalled", false, creditControlledGUIManager.InitialiseWasCalled);
				AssertEquals("Precondition: DisposeWasCalled", false, creditControlledGUIManager.DisposeWasCalled);
				PerformClick((U)DocumentMenuItem.Items[0]);
				AssertEquals("CreditControlGUIManager was initialised", true, creditControlledGUIManager.InitialiseWasCalled);
				AssertEquals("CreditControlGUIManager was disposed", true, creditControlledGUIManager.DisposeWasCalled);
			}
		}

		[GuiTest]
		public void TestDocumentSecurityOnClick()
		{
			ShipmentPivot.Factory.Save();
			var formWithLoadedDocumentMenuItem = this.FormWithLoadedDocumentMenuItem;

			//with Controller ID = null
			PerformClick((U)DocumentMenuItem.Items[0]);
			Assert("No error message should have been displayed. Current error message: " + UnitTestUserNotification.Instance.LastMessage.Text, UnitTestUserNotification.Instance.LastMessage.WasNone);
			Assert("One document should have been run", HelperForTesting(DocumentMenuItem).LastRunReportInfos.Count > 0);

			((IZForm)formWithLoadedDocumentMenuItem).ControllerID = ControllerIDs.JobAirSailing;
			var controller = ZControllerFactory.Create(((IZForm)formWithLoadedDocumentMenuItem).ControllerID);

			//with Controller ID = true

			var checkpoint = Env.Security.FindOrCreateDocumentCheckpoint(ShipmentDocumentCommand.PK.ToGuid(), ShipmentDocumentCommand.SU_MenuNameMultilingual, controller.ModuleID, Env.Security.CustomsDeclarationEnquiry);
			checkpoint.IsAllowed = false;

			PerformClick((U)DocumentMenuItem.Items[0]);
			AssertEquals("An error message should have been displayed", checkpoint.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("Error message should display path to document - and hence should contain several arrows ( '->' ) indicating the path", 4, checkpoint.ErrorMessageForNotAllowed.ToString().Split(new char[] { '>' }).Length);
			AssertEquals("No documents should have been run", 0, HelperForTesting(DocumentMenuItem).LastRunReportInfos.Count);

			checkpoint.IsAllowed = true;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			PerformClick((U)DocumentMenuItem.Items[0]);
			Assert("No error message should have been displayed", UnitTestUserNotification.Instance.LastMessage.WasNone);
			Assert("A document should have been run", HelperForTesting(DocumentMenuItem).LastRunReportInfos.Count > 0);
		}

		[GuiTest]
		public void TestQuestionsAreDisplayed()
		{
			ShipmentPivot.Factory.Save();
			using (var form = FormWithLoadedDocumentMenuItemWithQuestions)
			{
				form.ControllerID = ControllerIDs.GlbPortDeliveryTime;    // this module has no security

				PerformClick((U)DocumentMenuItem.Items[0]);
				AssertEquals("Are cuckoo squeakers awesome?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestDocumentSecurityOnClick_NoModuleSecurity()
		{
			ShipmentPivot.Factory.Save();
			((IZForm)FormWithLoadedDocumentMenuItem).ControllerID = ControllerIDs.GlbPortDeliveryTime;    // this module has no security

			PerformClick((U)DocumentMenuItem.Items[0]);
			Assert("No error message should have been displayed", UnitTestUserNotification.Instance.LastMessage.WasNone);
			Assert("A document should have been run as the module had no security", HelperForTesting(DocumentMenuItem).LastRunReportInfos.Count > 0);
		}

		[ExpectNoExceptions]
		public void TestDocumentSecurityOnClick_ControllerWithNoModuleID()
		{
			ShipmentPivot.Factory.Save();
			var controllerIDWithoutModuleID = ControllerIDs.DocAddresses;
			((IZForm)FormWithLoadedDocumentMenuItem).ControllerID = controllerIDWithoutModuleID;

			var controller = ZControllerFactory.Create(controllerIDWithoutModuleID);
			AssertEquals("The controller should have no ModuleID for the test", null, controller.ModuleID);
			PerformClick((U)DocumentMenuItem.Items[0]);
		}

		[GuiTest]
		public void TestDocumentMenuExceptionsAreCaught()
		{
			ShipmentDocumentCommand.SU_MenuDataContext = "I Will Not Exist";
			ShipmentDocumentCommand.SU_MenuName = "Something is wrong with me";
			ShipmentPivot.Factory.Save();
			var formWithLoadedDocumentMenuItem = this.FormWithLoadedDocumentMenuItem;

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			PerformClick((U)DocumentMenuItem.Items[0]);
			AssertEquals("Error Message", "An error occurred processing the Document Menu command:\r\nThe Email Subject Data Context of \"I Will Not Exist\" for document \"Something is wrong with me\" is not valid. Please check the Delivery Options of this document", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestDocumentSupporterQuestions()
		{
			var command = Factory.New<DocumentCommand>();
			command.SU_BusinessContext = nameof(BusinessContext.Customs);
			command.Parent = Factory.New<DocDummyBusinessObjectWithQuestions>();
			var printSet = new DocumentPrintSet(command, null);
			var questions = printSet.GenerateQuestionsToAskUsersBeforeRunningDocument();
			AssertEquals("GenerateQuestionsToAskUsersBeforeRunningDocument().Count", 1, questions.Count);
			AssertEquals("GenerateQuestionsToAskUsersBeforeRunningDocument()[0].Title", "Cuckoo Squeakers are great!", questions[0].Title);
		}

		public void TestMenuItemsShortcut()
		{
			var menuItem1 = Factory.New<DocumentCommand>();
			menuItem1.SU_MenuName = "My menuItem1";
			menuItem1.SU_MenuShortcut = "";
			var menuItem2 = Factory.New<DocumentCommand>();
			menuItem2.SU_MenuName = "My menuItem2";
			menuItem2.SU_MenuShortcut = "CtrlShiftN";
			var menuItem3 = Factory.New<DocumentCommand>();
			menuItem3.SU_MenuName = "My menuItem3";
			menuItem3.SU_MenuShortcut = "F1";
			var menuItem4 = Factory.New<DocumentCommand>();
			menuItem4.SU_MenuName = "My menuItem4";
			menuItem4.SU_MenuShortcut = "NotAShortCut";

			var documentToolStrip = new ZDocumentToolStripMenuItem();
			var toolStripMenu1 = documentToolStrip.HelperForTesting.CreateMenuForTesting(menuItem1);
			AssertEquals("MenuItem1 ShortcutKeys should be None", ((ZToolStripMenuItem)toolStripMenu1).ShortcutKeys, Keys.None);
			var toolStripMenu2 = documentToolStrip.HelperForTesting.CreateMenuForTesting(menuItem2);
			AssertEquals("MenuItem2 ShortcutKeys should be Control, Shift and N", ((ZToolStripMenuItem)toolStripMenu2).ShortcutKeys, Keys.Control | Keys.Shift | Keys.N);
			var toolStripMenu3 = documentToolStrip.HelperForTesting.CreateMenuForTesting(menuItem3);
			AssertEquals("MenuItem3 ShortcutKeys should be F1", ((ZToolStripMenuItem)toolStripMenu3).ShortcutKeys, Keys.F1);
			var toolStripMenu4 = documentToolStrip.HelperForTesting.CreateMenuForTesting(menuItem4);
			AssertEquals("MenuItem4 ShortcutKeys should be None", ((ZToolStripMenuItem)toolStripMenu4).ShortcutKeys, Keys.None);
		}

		[GuiTest]
		public void TestLoadFormsMenuItems()
		{
			var menu1 = Factory.New<DocumentCommand>();
			menu1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			menu1.SU_IsPublished = true;
			menu1.SU_IsSystemDefined = true;
			menu1.SU_MenuName = "Booking Summary";
			menu1.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;

			var menu2 = Factory.New<DocumentCommand>();
			menu2.SU_BusinessContext = nameof(BusinessContext.Shipment);
			menu2.SU_IsPublished = true;
			menu2.SU_IsSystemDefined = true;
			menu2.SU_MenuName = "Shipping Summary";
			menu2.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;

			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>() as BusinessObject;

			Factory.Save();

			using (var testForm = new TestForm(shipment))
			{
				((IZForm)testForm).ControllerID = ControllerIDs.QuotedBookings;
				using (var docMenuItem = new T())
				{
					docMenuItem.Setup(shipment as IDocumentSupportable, shipment as IDocumentEventsForMenu, null, null);
					AddToMainMenu(testForm, docMenuItem);
					testForm.Show();

					docMenuItem.LoadMenus(testForm);
					AssertEquals("Documents are loaded", "Booking Summary", GetText((U)docMenuItem.Items[0]));
					AssertEquals("Forms are loaded", "Shipping Summary", GetText((U)docMenuItem.Items[1]));
				}
			}
		}

		[GuiTest]
		[RequiresSTA]
		public void TestRun_ExtraDialogShown()
		{
			var command = Factory.New<DocumentCommand>();
			var menuItem = new T();

			var deliveryForm = new DocDeliveryForm(new DeliveryInstructions(), Env.Security.None);
			var helper = HelperForTesting(menuItem);
			helper.ThrowExceptionInGetDocumentPrintSetForTesting =
				() =>
				{
					ZFormModaliser.ShowDialogAndDispose(new ZForm());
					ZFormModaliser.ShowDialogAndDispose(deliveryForm);
				};
			var taggedMenuItem = helper.CreateMenuForTesting(command);

			Assert("Default Flag should be false", !helper.HasExtraFormsForTesting);
			PerformClick(taggedMenuItem);
			Assert("After Form Shown, Flag should be true", helper.HasExtraFormsForTesting);
			AssertEquals(deliveryForm.BackgroundDeliveryCheckBox.Visible, false);
		}

		#region Implementation

		protected abstract U FindByText(T documentMenuItem, string text);
		protected abstract void PerformClick(U documentMenuItem);
		protected abstract void AddToMainMenu(ZForm form, T docMenuItem);
		protected abstract IList GetMainMenuCollection(ZForm form);
		protected abstract string GetText(U menuItem);
		protected abstract bool GetEnabled(U menuItem);
		protected abstract string GetShortCut(U menuItem);
		protected abstract IList GetItems(U menuItem);
		protected abstract ZDocumentsMenuItemHelper<U> HelperForTesting(T menuItem);
		protected abstract void ResetIsRebuildMenuItemsCalled(T menuItem);

		class TestForm : ZForm
		{
			public TestForm()
			{
				MainMenuStrip = new MenuStrip();
				Controls.Add(MainMenuStrip);
			}

			public TestForm(BusinessObject bizObj)
				: base(bizObj)
			{
				MainMenuStrip = new MenuStrip();
				Controls.Add(MainMenuStrip);
			}
		}

		#region DummyBusinessObjects

		public class DocDummyBusinessObjectWithQuestions : DocDummyBusinessObject, IDocumentSupportable
		{
			public DocDummyBusinessObjectWithQuestions(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override DocumentSupporter DocumentSupporterCore
			{
				get { return new DocDummyBusinessObjectDocumentSupporterWithQuestionsGenerated(this); }
			}
		}

		public class DocDummyBusinessObject : DummyBusinessObject, IDocumentSupportable, IDocumentEventsForMenu, IStmNoteParent
		{
			public DocDummyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public DocumentSupporter DocumentSupporter
			{
				get { return DocumentSupporterCore; }
			}

			protected virtual DocumentSupporter DocumentSupporterCore
			{
				get { return new DocDummyBusinessObjectDocumentSupporter(this); }
			}

			#region IDocumentEvents Members

			public event DocumentPrintedEventHandler DocumentPrinted;
			public void NotifyDocumentPrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrinted != null)
				{
				}
			}

			public event DocumentPrintedEventHandler DocumentPrePreviewed;
			public void NotifyDocumentPrePreviewed(DocumentPrintedEventArgs e)
			{
				if (DocumentPrePreviewed != null)
				{
				}
			}

			public event DocumentPrintedEventHandler DocumentPrePrinted;
			public void NotifyDocumentPrePrinted(DocumentPrintedEventArgs e)
			{
				if (DocumentPrePrinted != null)
				{
				}
			}

			public event DocumentCancelEventHandler DocumentPrintRequested;

			public void NotifyDocumentPrintRequested(IStmMenuItem menuItem)
			{
				if (DocumentPrintRequested != null)
				{
				}
			}

			public bool CancelPrintRequest
			{
				get { return false; }
			}

			#endregion

			#region IStmNoteParent Members

			BusinessObject[] IStmNoteParent.BusinessObjectsWithRelatedNotes
			{
				get { return Array.Empty<BusinessObject>(); }
			}

			GetValueDelegate<NoteTypeCollection> IStmNoteParent.CustomNoteTypesDelegate
			{
				get { return null; }
				set { }
			}

			StmNoteContexts IStmNoteParent.NoteContextsForRelatedNotes
			{
				get { return new StmNoteContexts(); }
			}

			NoteTypeCollection IStmNoteParent.NoteTypes
			{
				get { return new NoteTypeCollection(); }
			}

			Notes IStmNoteParent.Notes
			{
				get { return new Notes(this); }
			}

			BusinessObjectFactory IStmNoteParent.NotesFactory
			{
				get { return Factory; }
			}

			ZGuid IStmNoteParent.NotesParentPK
			{
				get { return PK; }
			}

			string IStmNoteParent.NotesParentTableName
			{
				get { return string.Empty; }
			}

			bool IStmNoteParent.SupportsNotes
			{
				get { return false; }
			}

			#endregion
		}

		#endregion

		#region DummyBusinessObjectDocumentSupporters

		class DocDummyBusinessObjectDocumentSupporterWithQuestionsGenerated : DocDummyBusinessObjectDocumentSupporter
		{
			public DocDummyBusinessObjectDocumentSupporterWithQuestionsGenerated(DocDummyBusinessObject docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
			}

			protected override List<DocumentSupporterQuestion> GenerateQuestionsToAskUsersBeforeRunningDocumentCore(IStmMenuItem commandAboutToBeRun)
			{
				var result = base.GenerateQuestionsToAskUsersBeforeRunningDocumentCore(commandAboutToBeRun);
				var question = new DocumentSupporterQuestion("Cuckoo Squeakers are great!", "Are cuckoo squeakers awesome?", QuestionType.Warning);
				result.Add(question);
				return result;
			}
		}

		class DocDummyBusinessObjectDocumentSupporter : DocumentSupporter
		{
			public DocDummyBusinessObjectDocumentSupporter(DocDummyBusinessObject docDummyBusinessObject)
				: base(docDummyBusinessObject)
			{
			}

			public override DocumentSupporterDataState GetDataStateBeforeRun(IStmMenuItem commandAboutToBeRun)
			{
				if (commandAboutToBeRun.SU_MenuName == "Menu Item That Causes Error")
				{
					return new DocumentSupporterDataState(false, "Error occurred");
				}
				else
				{
					return null;
				}
			}

			public override BusinessContext BusinessContext
			{
				get
				{
					return BusinessContext.Shipment;
				}
			}

			public override string GetFilterValue(DocumentFilters filterName)
			{
				string value = null;

				switch (filterName)
				{
					case DocumentFilters.BUY:
						value = GlbStaff.CurrentUser.PK.ToString();
						break;

					default:
						value = base.GetFilterValue(filterName);
						break;
				}

				return value;
			}

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				return new DocumentWrapper[] { new DummyDocumentWrapper(BusinessObject, Factory) };
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return new Core.Constants.DataContext[] { Core.Constants.DataContext.Shipment };
			}

			public override ISecurityCheckpoint CustomisationSecurityCheckpoint { get { return Env.Security.None; } }

			public override IDocumentDeliveryContact GetContactOrganisation(ZString menuName, IContactType contact, DocumentDirection direction)
			{
				if (contact == ContactType.Consignee)
				{
					return new OrgHeaderContact(Factory.Load<OrgHeader>(new Guid("47F51331-B556-4BD2-977C-F99132751BA9")), null);
				}
				else
				{
					return null;
				}
			}
		}

		#endregion

		class TestDocumentDeliveryRestrictionGUIManager : IDocumentDeliveryRestrictionGUIManager
		{
			public void SetDescription(string description)
			{
			}

			public void SetCaption(string caption)
			{
			}

			public void Initialise(ICreditControlledBusinessObject creditControlledBusinessObject)
			{
				InitialiseWasCalled = true;
			}

			public void Dispose()
			{
				DisposeWasCalled = true;
			}

			public bool InitialiseWasCalled, DisposeWasCalled;

			public bool IsAuthenticationDeclined
			{
				get { return true; }
			}
		}

		#region FormWithLoadedDocumentMenuItemWithQuestions

		ZForm FormWithLoadedDocumentMenuItemWithQuestions
		{
			get
			{
				if (fFormWithLoadedDocumentMenuItemWithQuestions == null)
				{
					fFormWithLoadedDocumentMenuItemWithQuestions = new TestForm(DocBusinessObjectWithQuestions);
					DocumentMenuItem.Setup(DocBusinessObjectWithQuestions, DocBusinessObjectWithQuestions, null, null);
					AddToMainMenu(fFormWithLoadedDocumentMenuItemWithQuestions, DocumentMenuItem);

					fFormWithLoadedDocumentMenuItemWithQuestions.Show();
					DocumentMenuItem.LoadMenus(fFormWithLoadedDocumentMenuItemWithQuestions);
					AssertCollectionContains("Menu Items should contain the Dummy Menu Item", DocumentMenuItem, GetMainMenuCollection(FormWithLoadedDocumentMenuItemWithQuestions));
				}
				return fFormWithLoadedDocumentMenuItemWithQuestions;
			}
		}
		ZForm fFormWithLoadedDocumentMenuItemWithQuestions;

		#endregion

		#region FormWithLoadedDocumentMenuItem

		ZForm FormWithLoadedDocumentMenuItem
		{
			get
			{
				if (fFormWithLoadedDocumentMenuItem == null)
				{
					fFormWithLoadedDocumentMenuItem = new TestForm(DocBusinessObject);
					DocumentMenuItem.Setup(DocBusinessObject, DocBusinessObject, null, null);
					AddToMainMenu(fFormWithLoadedDocumentMenuItem, DocumentMenuItem);

					fFormWithLoadedDocumentMenuItem.Show();
					DocumentMenuItem.LoadMenus(fFormWithLoadedDocumentMenuItem);
					AssertCollectionContains("Menu Items should contain the Dummy Menu Item", DocumentMenuItem, GetMainMenuCollection(FormWithLoadedDocumentMenuItem));
				}
				return fFormWithLoadedDocumentMenuItem;
			}
		}
		ZForm fFormWithLoadedDocumentMenuItem;

		#endregion

		#region DocumentMenuItem

		T DocumentMenuItem
		{
			get
			{
				if (fDocumentMenuItem == null)
				{
					fDocumentMenuItem = new T();
				}
				return fDocumentMenuItem;
			}
		}
		T fDocumentMenuItem;

		#endregion

		#region ShipmentDocumentCommand

		DocumentCommand ShipmentDocumentCommand
		{
			get
			{
				if (fShipmentDocumentCommand == null)
				{
					fShipmentDocumentCommand = Factory.New<DocumentCommand>();
					fShipmentDocumentCommand.SU_BusinessContext = nameof(BusinessContext.Shipment);
					fShipmentDocumentCommand.SU_IsSystemDefined = true;
					fShipmentDocumentCommand.SU_IsPublished = true;
					fShipmentDocumentCommand.SU_MenuName = "Menu_Name";
					fShipmentDocumentCommand.SU_MenuIndex = 1;
					fShipmentDocumentCommand.SU_MenuPath = "";
					fShipmentDocumentCommand.SU_MenuShortcut = "F1";
				}
				return fShipmentDocumentCommand;
			}
		}
		DocumentCommand fShipmentDocumentCommand;

		#endregion

		#region ShipmentTemplate

		StmTemplateBase ShipmentTemplate
		{
			get
			{
				if (fShipmentSystemTemplate == null)
				{
					fShipmentSystemTemplate = Factory.New<StmTemplateBase>();
					fShipmentSystemTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
					fShipmentSystemTemplate.SO_IsSystemDefined = true;

					var bookingSummaryTemplate = new ExcelTemplateReadFromExcelTemplatesSolution(ExcelTemplateReadFromExcelTemplatesSolution.TemplateNames.BookingSummary);
					fShipmentSystemTemplate.SO_Template = new ZBlob(bookingSummaryTemplate.GetAsByteArray());
					fShipmentSystemTemplate.SO_Name = bookingSummaryTemplate.TemplateName + "1";
				}
				return fShipmentSystemTemplate;
			}
		}
		StmTemplateBase fShipmentSystemTemplate;

		#endregion

		#region ShipmentPivot

		StmMenuTemplatePivotBase ShipmentPivot
		{
			get
			{
				if (fShipmentPivot == null)
				{
					fShipmentPivot = Factory.New<StmMenuTemplatePivotBase>();
					fShipmentPivot.SI_SO = ShipmentTemplate.PK;
					fShipmentPivot.SI_SU = ShipmentDocumentCommand.PK;
					fShipmentPivot.SI_DocumentTitle = "Test Document";
				}
				return fShipmentPivot;
			}
		}
		StmMenuTemplatePivotBase fShipmentPivot;

		#endregion

		#region DocBusinessObjectWithQuestions

		DocDummyBusinessObjectWithQuestions DocBusinessObjectWithQuestions
		{
			get
			{
				if (fDocBusinessObjectWithQuestions == null)
				{
					var newFactory = new BusinessObjectFactory();
					fDocBusinessObjectWithQuestions = newFactory.New<DocDummyBusinessObjectWithQuestions>();
					fDocBusinessObjectWithQuestions.Z0_Code = "Tst";
					fDocBusinessObjectWithQuestions.Z0_Description = "Desc";
					newFactory.Save();
				}
				return fDocBusinessObjectWithQuestions;
			}
		}
		DocDummyBusinessObjectWithQuestions fDocBusinessObjectWithQuestions;

		#endregion

		#region DocBusinessObject

		protected DocDummyBusinessObject DocBusinessObject
		{
			get
			{
				if (fDocBusinessObject == null)
				{
					var newFactory = new BusinessObjectFactory();
					fDocBusinessObject = newFactory.New<DocDummyBusinessObject>();
					fDocBusinessObject.Z0_Code = "Tst";
					fDocBusinessObject.Z0_Description = "Desc";
					newFactory.Save();
				}
				return fDocBusinessObject;
			}
		}
		DocDummyBusinessObject fDocBusinessObject;

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
			DocumentCustomisationMenuItemMenusMaker.ShouldAddDebugOnlyMenuItemsForTesting = true;
			DocumentCustomisationToolStripMenusMaker.ShouldAddDebugOnlyMenuItemsForTesting = true;
			DocumentTablesCleaner.Clean();
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
			DocumentCustomisationMenuItemMenusMaker.ShouldAddDebugOnlyMenuItemsForTesting = false;
			DocumentCustomisationToolStripMenusMaker.ShouldAddDebugOnlyMenuItemsForTesting = false;

			if (fDocumentMenuItem != null)
			{
				fDocumentMenuItem.Dispose();
			}
			if (fFormWithLoadedDocumentMenuItem != null)
			{
				fFormWithLoadedDocumentMenuItem.Dispose();
			}
		}

		#endregion
	}
}
