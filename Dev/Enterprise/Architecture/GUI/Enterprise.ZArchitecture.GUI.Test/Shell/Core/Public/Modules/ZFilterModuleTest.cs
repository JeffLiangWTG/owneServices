using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Shell.Core.Public.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Modules.ZFilterModule;
using ErrorReporter = CargoWise.Common.ErrorReporter;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	internal abstract class ZFilterModuleTest : TestCaseWithDummy
	{
		#region Dispose

		public class DisposeGridCollectionThrows : DummyFilterGridModule
		{
			public void ForceCreateSearchManager()
				=> _ = SearchManager;

			protected override FilteredGridLoader CreateSearchManager()
				=> new ThrowingSearchManager(FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

			class ThrowingSearchManager : FilteredGridLoader
			{
				public ThrowingSearchManager(FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
					: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
				{
				}

				protected override void Dispose(bool isDisposing)
				{
					base.Dispose(isDisposing);
					throw new InvalidOperationException("DisposeGridCollection threw");
				}
			}
		}

		public void TestDisposeHandlesExceptionIn_DisposeGridCollection()
		{
			var filterModule = new DisposeGridCollectionThrows();
			filterModule.ForceCreateSearchManager();

			AssertNoExceptionThrown(() => filterModule.Dispose());
			AssertEquals("DisposeGridCollection threw", ErrorReporter.LastMessageReported);
			Assert("Filter module wasn't disposed", filterModule.IsDisposed);
			ErrorReporter.Clear();
		}

		public class DisposeFilterBusinessObjectThrows : ZFilterModuleForTest
		{
			protected override void DisposeFilterBusinessObject()
			{
				throw new InvalidOperationException("DisposeFilterBusinessObject threw");
			}
		}

		public void TestDisposeHandlesExceptionIn_DisposeFilterBusinessObject()
		{
			var filterModule = new DisposeFilterBusinessObjectThrows();
			AssertNoExceptionThrown(() => filterModule.Dispose());
			AssertEquals("DisposeFilterBusinessObject threw", ErrorReporter.LastMessageReported);
			Assert("Filter module wasn't disposed", filterModule.IsDisposed);
			ErrorReporter.Clear();
		}

		public void TestDisposeHandlesExceptionIn_DisposeFilterBusinessObject_ZCannotSaveException()
		{
			var filterModule = new ZFilterModuleForTest();
			var filterStripBizO = filterModule.FilterBusinessObject;
			var strip = filterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = "Test";
			var savedFilter = filterStripBizO.SaveLayout("savedFilter");
			filterStripBizO.LoadLayout(savedFilter);

			AssertNoExceptionThrown(filterModule.Dispose);
			AssertEquals(string.Empty, ErrorReporter.LastMessageReported);
			Assert("Filter module was disposed", filterModule.IsDisposed);
		}

		#endregion

		public void TestExportShouldValidateCustomSqlFilter()
		{
			using (var module = (DummyFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterControl = (DummyFilterControl)module.EmbeddedControl;

				var sqlFilter = (ModuleSQLFilter)module.FilterBusinessObject["Custom SQL Filter"];
				sqlFilter.IsActive = true;
				sqlFilter.Property1 = "invalid sql filter string";

				var export = module.ExportMenuItemsForTest["Export All Columns To Excel"];
				AssertNotNull(export);
				AssertNoExceptionThrown(() => { export.Invoke(null, null); });
				Assert(module.FilterBusinessObject.HasErrors);

				UnitTestUserNotification.Instance.ClearMessages();

				var export1 = module.ExportMenuItemsForTest["Export Visible Columns To Excel"];
				AssertNotNull(export1);
				AssertNoExceptionThrown(() => { export1.Invoke(null, null); });
				Assert(module.FilterBusinessObject.HasErrors);
			}
		}

		public void TestNewAndEdit_OnModuleFindBox_ShouldPrepopulate()
		{
			var orgHeader = Factory.New<IOrgHeader>();
			orgHeader.OH_Code = "ORG1";
			orgHeader.OH_FullName = "Org Header 1";
			Factory.Save();

			var controller = (IOrganisationController)ZControllerFactory.Create(ControllerIDs.Organisation);
			using (var form = (ZForm)controller.ShowForm((BusinessObject)orgHeader, OrganisationTabPages.Details, FormAction.Edit))
			{
				var securityGroupFindBox = form.FindSingleOrDefault<ZGuidFindBox>(c => c.Name == "OrgSecurityGroupFindBox");
				KeySender.PostKeyDown(securityGroupFindBox.CodeBox, Keys.F3);
				Application.DoEvents();
				using (var newSecurityForm = (ZForm)ZFormModaliser.LastFormShownForTest)
				{
					AssertEquals("WHEN user hit F3 on empty findbox THEN should show New Form", "New Group", newSecurityForm.Text);

					var newGroupCode = newSecurityForm.FindSingleOrDefault<ZTextBox>(c => c.Name == "GG_CodeBoundTextBox");
					newGroupCode.Focus();
					newGroupCode.Text = "SECURITY1";
					Application.DoEvents();

					var newGroupDescription = newSecurityForm.FindSingleOrDefault<ZTextBox>(c => c.Name == "GG_DescBoundTextBox");
					newGroupDescription.Focus();
					newGroupDescription.Text = "SECURITY1 Description";
					Application.DoEvents();

					((BusinessObject)newSecurityForm.BusinessEntity).Factory.Save();

					AssertNull("GIVEN no error when inserting new security record", UnitTestUserNotification.Instance.LastMessage.Text);

					newSecurityForm.Close();
				}

				UnitTestUserNotification.Instance.ClearMessages();

				Application.DoEvents();
				AssertEquals("WHEN creating new bizObj THEN findbox should be set to it", "SECURITY1", securityGroupFindBox.CodeBox.Text);

				KeySender.PostKeyDown(securityGroupFindBox.CodeBox, Keys.F3);
				Application.DoEvents();

				using (var editSecurityForm = (ZForm)ZFormModaliser.LastFormShownForTest)
				{
					AssertEquals("WHEN user hit F3 on existing findbox THEN should show Edit Form", "Edit Group", editSecurityForm.Text);

					var editGroupCode = editSecurityForm.FindSingleOrDefault<ZTextBox>(c => c.Name == "GG_CodeBoundTextBox");
					editGroupCode.Focus();
					editGroupCode.Text = "SECURITY2";
					Application.DoEvents();

					var editGroupDescription = editSecurityForm.FindSingleOrDefault<ZTextBox>(c => c.Name == "GG_DescBoundTextBox");
					editGroupDescription.Focus();
					editGroupDescription.Text = "SECURITY2 Description";
					Application.DoEvents();

					((BusinessObject)editSecurityForm.BusinessEntity).Factory.Save();
					editSecurityForm.Close();
				}

				Application.DoEvents();
				AssertEquals("WHEN editing existing bizObj THEN findbox should be set to it", "SECURITY2", securityGroupFindBox.CodeBox.Text);
			}
		}

		public void TestShowDeleteFormPopulatesControllerParentModule()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;

				using (var form = module.ShowDeleteForm(dummy))
				{
					AssertEquals(module, controller.ParentModule);
				}
			}
		}

		public void TestShowEditFormPopulatesControllerParentModule()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;

				using (var form = module.ShowEditForm(dummy))
				{
					AssertEquals(module, controller.ParentModule);
				}
			}
		}

		public void TestShowDeleteFormShowsIncorrectTypeErrorMessage()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var expectedError = "The selected object is no longer valid. Please refresh the grid and try again.";
			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;
				module.HasTypeError_ForTest = true;
				using (var form = module.ShowDeleteForm(dummy))
				{
					AssertEquals("Should not shown error.", true, UnitTestUserNotification.Instance.LastMessage.Contains(expectedError));
				}

				UnitTestUserNotification.Instance.ClearMessages();
				module.HasTypeError_ForTest = false;
				using (var form = module.ShowDeleteForm(dummy))
				{
					AssertEquals("Should not shown error.", false, UnitTestUserNotification.Instance.LastMessage.Contains(expectedError));
				}
			}
		}

		public void TestShowDeleteMultipleShowsIncorrectTypeErrorMessage()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var expectedError = "The selected object is no longer valid. Please refresh the grid and try again.";
			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;
				module.HasTypeError_ForTest = true;
				module.DeleteMultiple(new[] { dummy1, dummy2 });
				AssertEquals("Should not shown error.", true, UnitTestUserNotification.Instance.LastMessage.Contains(expectedError));

				UnitTestUserNotification.Instance.ClearMessages();
				module.HasTypeError_ForTest = false;
				module.DeleteMultiple(new[] { dummy1, dummy2 });
				AssertEquals("Should not shown error.", false, UnitTestUserNotification.Instance.LastMessage.Contains(expectedError));
			}
		}

		public void TestShowEditFormShowsIncorrectTypeErrorMessage()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var expectedError = "The selected object is no longer valid. Please refresh the grid and try again.";
			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;
				module.HasTypeError_ForTest = true;
				using (var form = module.ShowEditForm(dummy))
				{
					AssertEquals("Should not shown error.", true, UnitTestUserNotification.Instance.LastMessage.Contains(expectedError));
				}

				UnitTestUserNotification.Instance.ClearMessages();
				module.HasTypeError_ForTest = false;
				using (var form = module.ShowEditForm(dummy))
				{
					AssertEquals("Should not shown error.", false, UnitTestUserNotification.Instance.LastMessage.Contains(expectedError));
				}
			}
		}

		public void TestShowViewFormShowsIncorrectTypeErrorMessage()
		{
			var dummy = Factory.NewWithValidTestData<DummyBusinessObject>();
			Factory.Save();

			var expectedError = "The selected object is no longer valid. Please refresh the grid and try again.";
			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;
				module.HasTypeError_ForTest = true;
				using (var form = module.ShowViewForm(dummy))
				{
					AssertEquals("Should not shown error.", true, UnitTestUserNotification.Instance.LastMessage.Contains(expectedError));
				}

				UnitTestUserNotification.Instance.ClearMessages();
				module.HasTypeError_ForTest = false;
				using (var form = module.ShowViewForm(dummy))
				{
					AssertEquals("Should not shown error.", false, UnitTestUserNotification.Instance.LastMessage.Contains(expectedError));
				}
			}
		}

		public void TestShowNewFormPopulatesControllerParentModule()
		{
			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;

				using (var form = module.ShowNewForm())
				{
					AssertEquals(module, controller.ParentModule);
				}
			}
		}

		public void TestShowViewFormPopulatesControllerParentModule()
		{
			var dummy = Factory.New<DummyBusinessObject>();

			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;

				using (var form = module.ShowViewForm(dummy))
				{
					AssertEquals(module, controller.ParentModule);
				}
			}
		}

		public void TestHasMenuItems()
		{
			using (var testModule = new ZFilterModuleForTest())
			{
				Assert(!testModule.HasImportMenuItems);
				testModule.ImportMenuItems_Exposed.Add((NoResString)"zzz", new EventHandler(delegate
				{ }));
				Assert(testModule.HasImportMenuItems);
			}
		}

		public void TestHasCustomSqlAsDefault()
		{
			using (var testModule = new ZFilterModuleForTest())
			{
				Assert(testModule.FilterBusinessObject.HasCustomSqlFilter);
			}
		}

		public void TestOverrideLicenseCheckpointIsPassedToController()
		{
			using (var testModule = CreateDummyFilterModule())
			{
				(testModule as ZFilterModule).LicenceCheckPointOverride = EnvProxy.Instance.Licence.DocManager;
				using (var form = (Form)testModule.ShowNewForm())
				{
					AssertEquals(EnvProxy.Instance.Licence.DocManager, testModule.LastController.LicenceCheckpointForModifyOverride);
					AssertEquals(EnvProxy.Instance.Licence.DocManager, testModule.LastController.LicenceCheckpointForViewOverride);
				}
			}
		}

		public void TestShowPopup()
		{
			using (var testModule = new ZFilterModuleForTest())
			using (var form = (Form)testModule.ShowPopup())
			{
				Application.DoEvents();

				AssertNotNull(form);
				AssertEquals("Test Module", form.Text);
				AssertEquals(true, form.Visible);
				AssertEquals(typeof(EmbeddedModulePopupWithNoButtonPanelAndNoModality), form.GetType());
			}
		}

		public void TestAddImportDataMenuItems()
		{
			using (var testModule = new ZFilterModuleForTest())
			{
				try
				{
					testModule.AddImportDataMenuItems();

					AssertEquals("Import with license", testModule.ImportMenuItems_Exposed[0].Text);
					AssertNotNull(testModule.ImportMenuItems_Exposed[0].OnClick);
					AssertEquals(typeof(ImportSecurityChecker), testModule.ImportMenuItems_Exposed[0].OnClick.Target.GetType());

					AssertEquals("Import without license", testModule.ImportMenuItems_Exposed[1].Text);
					AssertNull(testModule.ImportMenuItems_Exposed[1].OnClick);
				}
				finally
				{
					testModule.ImportMenuItems_Exposed.Clear();
				}
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestAddInterfaceConnectorImportMenuItem()
		{
			AssertAddInterfaceConnectorImportMenuItem(true, true, true, false);
			AssertAddInterfaceConnectorImportMenuItem(false, false, false, false);
			AssertAddInterfaceConnectorImportMenuItem(false, true, true, true);
			AssertAddInterfaceConnectorImportMenuItem(true, false, true, false);
		}

		public void AssertAddInterfaceConnectorImportMenuItem(bool temporarilyAllowed, bool enableInterfaceConnector, bool expectedMenuItemAdded, bool expectedAddedMenuItemHasChecker)
		{
			using (var testModule = new ZFilterModuleForTest())
			{
				try
				{
					if (enableInterfaceConnector)
					{
						var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
						eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
					}

					testModule.AddInterfaceConnectorImportMenuItem(temporarilyAllowed);

					if (expectedMenuItemAdded)
					{
						AssertEquals("Import menu", testModule.ImportMenuItems_Exposed[0].Text);

						if (expectedAddedMenuItemHasChecker)
						{
							AssertNotNull(testModule.ImportMenuItems_Exposed[0].OnClick);
							AssertEquals(typeof(ImportSecurityChecker), testModule.ImportMenuItems_Exposed[0].OnClick.Target.GetType());
						}
						else
						{
							AssertNull(testModule.ImportMenuItems_Exposed[0].OnClick);
						}
					}
					else
					{
						AssertEquals("No menu items should be added without valid licence", 0, testModule.ImportMenuItems_Exposed.Count);
					}
				}
				finally
				{
					testModule.ImportMenuItems_Exposed.Clear();
					var empty = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
					eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, empty);
				}
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestAddInterfaceConnectorExportMenuItem()
		{
			AssertAddInterfaceConnectorExportMenuItem(true, true, true);
			AssertAddInterfaceConnectorExportMenuItem(true, false, false);
			AssertAddInterfaceConnectorExportMenuItem(false, true, true);
			AssertAddInterfaceConnectorExportMenuItem(false, false, false);
		}

		public void AssertAddInterfaceConnectorExportMenuItem(bool includeAccessAllowedCheck, bool enableInterfaceConnector, bool expectedMenuItemAdded)
		{
			using (var testModule = new ZFilterModuleForTest())
			{
				try
				{
					if (enableInterfaceConnector)
					{
						var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
						eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
					}

					testModule.AddInterfaceConnectorExportMenuItem(includeAccessAllowedCheck);

					if (expectedMenuItemAdded)
					{
						AssertEquals("Export To &XML", testModule.ExportMenuItems_Exposed[2].Text);
					}
				}
				finally
				{
					testModule.ExportMenuItems_Exposed.Clear();
					var empty = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
					eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, empty);
				}
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestAddImportFromCSVDataMenuItems()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Today.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var testModule = new ZFilterModuleForTest())
			{
				try
				{
					testModule.AddInterfaceConnectorCSVImportMenuItem();

					AssertEquals("Import From &CSV", testModule.ImportMenuItems_Exposed[0].Text);
					AssertNotNull(testModule.ImportMenuItems_Exposed[0].OnClick);
					AssertEquals(typeof(ImportSecurityChecker), testModule.ImportMenuItems_Exposed[0].OnClick.Target.GetType());
				}
				finally
				{
					testModule.ImportMenuItems_Exposed.Clear();
				}
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var testModule = new ZFilterModuleForTest())
			{
				try
				{
					testModule.AddInterfaceConnectorCSVImportMenuItem();
					// BG: For now does not matter what InterfaceConnector registry is set to.
					AssertEquals(typeof(ImportSecurityChecker), testModule.ImportMenuItems_Exposed[0].OnClick.Target.GetType());
				}
				finally
				{
					testModule.ImportMenuItems_Exposed.Clear();
				}
			}
		}

		public void TestExportToExcelDoesNotUseNoResultQuery()
		{
			using (var testModule = new ZFilterModuleForTest())
			{
				testModule.GridCollection.CompleteFilter.IsNoResultQuery = true;

				Assert("ExportQuery should not return NoResultQuery even if module.GridCollection.CompleteFilter.IsNoResultQuery = true (should change it back to false).",
					!testModule.GetExportQuery().IsNoResultQuery);
			}
		}

		public void TestExportToExcelWhenFilterBizObjHasErrors()
		{
			using (var testModule = CreateDummyFilterModule())
			{
				testModule.FilterBusinessObject.AddRowError("Error");
				testModule.ExportToExcel();

				Assert("Should have shown error.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Error text", "There are errors. Please correct these before exporting to excel.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions()]
		public void TestItShows()
		{
			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Text = "An exciting module test!";
				form.Show();
			}
		}

		public void TestDoNotCheckOrSaveChanges()
		{
			using (var testModule = CreateDummyFilterModule() as ZFilterModule)
			{
				Assert("DoNotCheckOrSaveChanges should not be set by default", !testModule.DoNotCheckOrSaveChanges);
				testModule.DoNotCheckOrSaveChanges = true;
				Assert(testModule.DoNotCheckOrSaveChanges);
			}
		}

		public void TestPassingEnablePreviousNextButtonsToController()
		{
			using (var testModule = CreateDummyFilterModule())
			using (var findBox = new ZGuidFindBox())
			{
				testModule.ModuleDecisionProvider = new DummyModuleDecisionProvider(findBox);
				testModule.ShowNewForm().Dispose();
				Assert("Passed correctly", !testModule.LastController.EnablePreviousNextSupport);
				Assert("Passed correctly", !((ZForm)testModule.LastController.LastShownForm).AutoAddPreviousNextButtons);
			}
		}

		public void TestUpdatesModuleResultsPKListOnSearch_WhenGridCollectionIsBusinessCollection()
		{
			TestCaseHelper.ClearTable(Dummy.TableName);

			var factory = new BusinessObjectFactory();
			var dummy1 = factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "YEA";
			var dummy2 = factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "NAA";
			factory.Save();

			using var form = new ZChildForm();
			using var testModule = CreateDummyFilterModule();
			form.Controls.Add(testModule.EmbeddedControl);
			form.Text = "Hey you!!!";
			form.Show();

			testModule.FilterBusinessObject.ResetToDefaultValues();
			((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();

			AssertEquals("Searched OK", 2, testModule.GridList.Count);

			var pks = ZModuleResults.Instance.GetPKCollectionForModule(ModuleID);
			AssertEquals("Module results pk list updated", 2, pks.Count);
			AssertEquals("Module results pk list updated", ((BusinessObject)testModule.GridList[0]).PK, pks[0].PK);
			AssertEquals("Module results pk list updated", ((BusinessObject)testModule.GridList[1]).PK, pks[1].PK);
		}

		public void TestUpdatesModuleResultsPKListOnSearch_WhenGridCollectionIsActiveBusinessCollection()
		{
			TestCaseHelper.ClearTable(Dummy.TableName);

			var factory = new BusinessObjectFactory();
			var dummy1 = factory.New<DummyBusinessObject>();
			dummy1.Z0_Bool = true;
			var dummy2 = factory.New<DummyBusinessObject>();
			dummy2.Z0_Bool = false;
			var dummy3 = factory.New<DummyBusinessObject>();
			dummy3.Z0_Bool = true;

			factory.Save();

			using var testForm = new ZChildForm();
			using var testModule = new DummyFilterGridModuleWithActiveBOC();

			testModule.RelationshipFilter = new ZQuery(DummyBizoSchema.Z0_Bool, true);
			testForm.Controls.Add(testModule.EmbeddedControl);
			testForm.Text = "PKList should be updated for ActiveBusinessCollection";
			testForm.Show();

			testModule.FilterBusinessObject.ResetToDefaultValues();
			testModule.GridCollection.ApplySort(dummy1.Z0_BoolInfo.PropertyDescriptor, ListSortDirection.Ascending);
			((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();

			AssertEquals("Searched successfully with 2 records in the collection", 2, testModule.GridCollection.Count);

			var pkCollection = ZModuleResults.Instance.GetPKCollectionForModule(ModuleID);

			AssertEquals("PK collection for module results should be updated containing 2 elements", 2, pkCollection.Count);
			AssertEquals("First PK in pk collection for module should be same as the one in grid collection", ((BusinessObject)testModule.GridCollection[0]).PK, pkCollection[0].PK);
			AssertEquals("Second PK in pk collection for module should be same as the one in grid collection", ((BusinessObject)testModule.GridCollection[1]).PK, pkCollection[1].PK);
		}

		public void TestUpdatesModuleResultsPKListOnSort()
		{
			TestCaseHelper.ClearTable(Dummy.TableName);

			var factory = new BusinessObjectFactory();
			var dummy1 = factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "YEA";
			var dummy2 = factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "NAA";
			factory.Save();

			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Text = "What are you smiling about????";
				form.Show();

				testModule.FilterBusinessObject.ResetToDefaultValues();
				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();

				AssertEquals("Searched OK", 2, testModule.GridList.Count);

				testModule.GridList.ApplySort(dummy1.Z0_CodeInfo.PropertyDescriptor, ListSortDirection.Ascending);
				var pks = ZModuleResults.Instance.GetPKCollectionForModule(ModuleID);
				AssertEquals("Module results pk list updated", ((BusinessObject)((IList)testModule.GridList)[0]).PK, pks[0].PK);
				AssertEquals("Module results pk list updated", ((BusinessObject)((IList)testModule.GridList)[1]).PK, pks[1].PK);

				testModule.GridList.ApplySort(dummy1.Z0_CodeInfo.PropertyDescriptor, ListSortDirection.Descending);
				pks = ZModuleResults.Instance.GetPKCollectionForModule(ModuleID);
				AssertEquals("Module results pk list updated", ((BusinessObject)((IList)testModule.GridList)[0]).PK, pks[0].PK);
				AssertEquals("Module results pk list updated", ((BusinessObject)((IList)testModule.GridList)[1]).PK, pks[1].PK);
			}
		}

		public void TestFactoryIsRestoredForCollectionAfterSearch()
		{
			BusinessObjectFactory initialFactory = null;
			var testModule = CreateDummyFilterModule();
			try
			{
				initialFactory = testModule.Collection.Factory;
				((IFilterModuleInternalsForTesting)testModule).PerformSearch();
				Assert("Factory should change", initialFactory != testModule.Collection.Factory);
			}
			finally
			{
				testModule.Dispose();
			}
			AssertEquals("Original factory should be back", initialFactory, testModule.Collection.Factory);
		}

		public void TestPerformSearchThrowException()
		{
			BusinessObjectFactory initialFactory = null;
			var testModule = CreateDummyFilterModule();
			initialFactory = testModule.Collection.Factory;
			initialFactory = testModule.Collection.Factory;
			((ZFilterModule)testModule).sqlExceptionOccured = true;
			try
			{
				AssertNoExceptionThrown(() => { ((IFilterModuleInternalsForTesting)testModule).PerformSearch(); });
			}
			finally
			{
				testModule.Dispose();
			}
		}

		public void TestHoldsReferenceToResultListAfterSearch()
		{
			WeakReference weakRef;

			var testModule = CreateDummyFilterModule();
			try
			{
				weakRef = GetPKCollectionForModuleWeakReference(ModuleID);
				GC.Collect();
				Assert("Should get garbage collected as has not yet searched", !weakRef.IsAlive);

				((IFilterModuleInternalsForTesting)testModule).PerformSearch();
				weakRef = GetPKCollectionForModuleWeakReference(ModuleID);
				GC.Collect();
				Assert("Should not get garbage collected as has searched", weakRef.IsAlive);
			}
			finally
			{
				testModule.Dispose();
				testModule = null;
			}

			GC.Collect();

			Assert("Should be garbage collected", !weakRef.IsAlive);
		}

		static WeakReference GetPKCollectionForModuleWeakReference(ModuleIdentifier moduleId)
		{
			return new WeakReference(ZModuleResults.Instance.GetPKCollectionForModule(moduleId));
		}

		public void TestSetInitialCodeForSearch()
		{
			TestCaseHelper.ClearTable(Dummy.TableName);

			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "YEA";
			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "NAA";
			Factory.Save();

			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				testModule.SetInitialCodeForSearch("YE");
				form.Controls.Add(testModule.EmbeddedControl);
				form.Text = "Yet another exciting module test!";
				form.Show();

				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();

				AssertEquals("Only found YEA", 1, testModule.GridList.Count);
				AssertEquals("Only found YEA", dummy1.PK, ((BusinessObject)((IList)testModule.GridList)[0]).PK);
			}
		}

		public void TestNoAfterDisposeWoesCausedByDataRefresh()
		{
			TestCaseHelper.ClearTable(Dummy.TableName);
			using (var form = new ZChildForm())
			{
				IDummyFilterModule testModule;
				IList gridList;
				using (testModule = CreateDummyFilterModule())
				{
					form.Controls.Add(testModule.EmbeddedControl);
					form.Show();

					((IDummyFilterControl)testModule.EmbeddedControl).ResetFilter();
					((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();

					gridList = ((IDummyFilterControl)testModule.EmbeddedControl).GridList;
					AssertEquals("No results", 0, gridList.Count);
				}

				var otherFactory = new BusinessObjectFactory();
				var otherFactoryDummy = otherFactory.New<DummyBusinessObject>();
				otherFactory.Save();
				AssertEquals("Should not be updated by datarefresh as it is disposed", 0, gridList.Count);

				otherFactoryDummy.Delete();
				otherFactory.Save();
				AssertEquals("Should not be updated by datarefresh as it is disposed", 0, gridList.Count);
			}
		}

		public void TestSearchWithFilterCombine()
		{
			DeleteAllDummies();
			Factory.Save();

			const int filterGreaterThanNumber = 42;
			const string filterByString = "TEST";

			SetActiveFilter(new ZQuery(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThan, filterGreaterThanNumber));
			try
			{
				using (var testModule = CreateDummyFilterModule())
				{
					var findBoxMock = new Mock<IFindBox>();
					testModule.ModuleDecisionProvider = new TestPopupModuleDecisionProvider(findBoxMock.Object);
					using (var popup = new EmbeddedModulePopup((ZFilterModule)testModule))
					{
						findBoxMock.SetupGet(o => o.PopupForm).Returns(popup);
						var allowClose = false;

						popup.Text = "Module Test";
						popup.Show();
						popup.Closing += (sender, e) => e.Cancel = !allowClose;

						try
						{
							((IDummyFilterControl)testModule.EmbeddedControl).ResetFilter();
							((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
							AssertEquals("No search results found", 0, ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count);

							var anotherFactory = new BusinessObjectFactory();

							var dummy1 = anotherFactory.New<DummyBusinessObject>();
							dummy1.Z0_Description = "";
							dummy1.Z0_Number = filterGreaterThanNumber - 1;

							var dummy2 = anotherFactory.New<DummyBusinessObject>();
							dummy2.Z0_Description = "";
							dummy2.Z0_Number = filterGreaterThanNumber - 1;

							anotherFactory.Save();

							((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();

							AssertEquals("List Count", 2, ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count);
							AssertRowValidity(dummy1.PK, false, ((IDummyFilterControl)testModule.EmbeddedControl), popup);
							AssertRowValidity(dummy2.PK, false, ((IDummyFilterControl)testModule.EmbeddedControl), popup);

							dummy1.Z0_Number = filterGreaterThanNumber + 1;
							dummy1.Z0_Description = filterByString;
							dummy2.Z0_Number = filterGreaterThanNumber - 1;
							dummy2.Z0_Description = filterByString;

							var dummy3 = anotherFactory.New<DummyBusinessObject>();
							dummy3.Z0_Number = filterGreaterThanNumber + 1;
							dummy3.Z0_Description = "";

							anotherFactory.Save();

							var txtFilter = (ModuleTextFilter)testModule.FilterBusinessObject[DummyBizoSchema.Z0_Description.Name];
							txtFilter.IsActive = true;
							txtFilter.Property = filterByString;
							((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();

							AssertEquals("List Count", 2, ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count);
							AssertRowValidity(dummy1.PK, true, ((IDummyFilterControl)testModule.EmbeddedControl), popup);
							AssertRowValidity(dummy2.PK, false, ((IDummyFilterControl)testModule.EmbeddedControl), popup);
						}
						finally
						{
							allowClose = true;
						}
					}
				}
			}
			finally
			{
				SetActiveFilter(null);
			}
		}

		void AssertRowValidity(ZGuid pk, bool expectedValid, IDummyFilterControl dummyFilterControl, EmbeddedModulePopup popup)
		{
			var bizObj = dummyFilterControl.GridList.FindByPK(pk);
			AssertNotNull("BizObj is contained by collection", bizObj);

			var index = dummyFilterControl.GridList.IndexOf(bizObj);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			dummyFilterControl.SelectRow(index);
			popup.ExposedOKButtonForTesting.PerformClick();

			AssertEquals("Selection Validity", !expectedValid, UnitTestUserNotification.Instance.LastMessage.WasError);

			((BusinessObjectCollection)dummyFilterControl.GridList).SetOverrideNotificationTypeWhenAdditionalFilterNotMet(CargoWise.ComponentModel.NotificationType.Warning);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			popup.ExposedOKButtonForTesting.PerformClick();
			AssertEquals("Selection Validity", false, UnitTestUserNotification.Instance.LastMessage.WasError);

			((BusinessObjectCollection)dummyFilterControl.GridList).SetOverrideNotificationTypeWhenAdditionalFilterNotMet(null);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			popup.ExposedOKButtonForTesting.PerformClick();
			AssertEquals("Selection Validity", !expectedValid, UnitTestUserNotification.Instance.LastMessage.WasError);
			dummyFilterControl.UnselectRow(index);

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
		}

		#region TestCheckValidSelectionForFindBox

		public void TestCheckValidSelectionForFindBox_DBHits()
		{
			var dummyA = Factory.New<DummyBusinessObject>();
			var dummyB = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
			};
			using (AssertDbHitsForAllFactories("", expectedDbHits, useOnlyNewFactories: true, tablesToIgnore: new string[] { "RefDatabase_RefCusTradeGroup", "RefDatabase_RefDataGrouping" }))
			using (var module = new ZFilterModuleForTest())
			{
				AssertEquals(true, module.CheckValidSelectionForFindBox(new[] { dummyA, dummyB }));
			}
		}

		public void TestCheckValidSelectionForFindBox_AdditionalFilter_invalid()
		{
			// Because dummyA does not have Z0_Number == 4, it will not match the additional filter added by ZFilterModuleForTest_AdditionalFilter.
			// This results in CheckValidSelectionForFindBox returning false.
			var dummyA = Factory.New<DummyBusinessObject>();
			var dummyB = Factory.New<DummyBusinessObject>();
			dummyB.Z0_Number = 4;
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
			};
			using (AssertDbHitsForAllFactories("", expectedDbHits, useOnlyNewFactories: true, tablesToIgnore: new string[] { "RefDatabase_RefCusTradeGroup", "RefDatabase_RefDataGrouping" }))
			using (var module = new ZFilterModuleForTest_AdditionalFilter())
			{
				AssertEquals(false, module.CheckValidSelectionForFindBox(new[] { dummyA, dummyB }));
			}
		}

		public void TestCheckValidSelectionForFindBox_AdditionalFilter_valid()
		{
			// Because dummyA and dummyB both have Z0_Number == 4, they will match the additional filter added by ZFilterModuleForTest_AdditionalFilter.
			// This results in CheckValidSelectionForFindBox returning true.
			var dummyA = Factory.New<DummyBusinessObject>();
			dummyA.Z0_Number = 4;
			var dummyB = Factory.New<DummyBusinessObject>();
			dummyB.Z0_Number = 4;
			Factory.Save();

			var expectedDbHits = new Dictionary<string, int>()
			{
				{ DummyBizoSchema.Constants.TableName, 1 },
			};
			using (AssertDbHitsForAllFactories("", expectedDbHits, useOnlyNewFactories: true, tablesToIgnore: new string[] { "RefDatabase_RefCusTradeGroup", "RefDatabase_RefDataGrouping" }))
			using (var module = new ZFilterModuleForTest_AdditionalFilter())
			{
				AssertEquals(true, module.CheckValidSelectionForFindBox(new[] { dummyA, dummyB }));
			}
		}

		public void TestCheckValidSelectionForFindBox_NoExceptionThrown()
		{
			using (var module = new ZFilterModuleForTest_AdditionalFilter())
			{
				AssertNoExceptionThrown(() => module.CheckValidSelectionForFindBox(new BusinessObject[] { null }));
			}
		}

		class ZFilterModuleForTest_AdditionalFilter : ZFilterModuleForTest
		{
			protected override IBusinessObjectCollection GetNewGridCollection()
			{
				var filter = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
				filter.AdditionalFilter = new ZQuery(DummyBizoSchema.Z0_Number, 4);
				return filter;
			}
		}

		public void TestCheckValidSelectionForFindBox_BizoFromDifferentFactoryValidation()
		{
			DeleteAllDummies();

			var differentFactory = new BusinessObjectFactory();
			var bizoFromDifferentFactory = differentFactory.New<DummyBusinessObject>();
			bizoFromDifferentFactory.Z0_Code = "B";
			differentFactory.Save();

			using (var module = CreateDummyFilterModule())
			{
				var collection = new DummyBizoCollectionForExtraNotificationTesting(Factory);

				var moduleDecisionProvider = new Mock<IModuleDecisionProvider>();
				moduleDecisionProvider.Setup(x => x.ShouldIgnoreAdditionalFilter).Returns(true);
				moduleDecisionProvider.Setup(x => x.List).Returns(collection);

				module.ModuleDecisionProvider = moduleDecisionProvider.Object;

				AssertEquals("Testing bizos from different factories", true, bizoFromDifferentFactory.Factory._Instance != module.Factory._Instance);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("A bizo from a different bizo factory should undergo extra validation when selected", false, ((ZFilterModule)module).CheckValidSelectionForFindBox(new BusinessObjectFactory(), bizoFromDifferentFactory));
				AssertEquals("Error Chpok! Extra notification...", UnitTestUserNotification.Instance.LastMessage.ToString());

				var bizoFromSameFactory = module.Factory.New<DummyBusinessObject>();
				bizoFromSameFactory.Z0_Code = "B";
				module.Factory.Save();

				AssertEquals("Testing bizos from the same factory", true, bizoFromSameFactory.Factory._Instance == module.Factory._Instance);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("A bizo from the same bizo factory should not undergo extra validation when selected", true, ((ZFilterModule)module).CheckValidSelectionForFindBox(new BusinessObjectFactory(), bizoFromSameFactory));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		#endregion

		#region ZFilterModuleAlwaysCheckExtraNotificationAttribute

		public void TestZFilterModuleAlwaysCheckExtraNotificationAttributeTriggeringExtraNotificationCheck()
		{
			DeleteAllDummies();

			var testBusinessObject = Factory.New<DummyBusinessObject>();
			testBusinessObject.Z0_Code = "A";
			Factory.Save();

			using (var module = CreateDummyFilterModule())
			{
				var collectionWhichDoesNotCheckExtraNotification = new DummyClassWithoutAttribute(Factory);

				var moduleDecisionProvider = new Mock<IModuleDecisionProvider>();
				moduleDecisionProvider.Setup(x => x.ShouldIgnoreAdditionalFilter).Returns(true);
				moduleDecisionProvider.Setup(x => x.List).Returns(collectionWhichDoesNotCheckExtraNotification);

				module.ModuleDecisionProvider = moduleDecisionProvider.Object;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Should return true as extra notification check not occuring", true, ((ZFilterModule)module).CheckValidSelectionForFindBox(new BusinessObjectFactory(), testBusinessObject));
				AssertEquals("There should not be any error messages as the notification is not being checked", true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			using (var module = CreateDummyFilterModule())
			{
				var collectionWhichDoesCheckExtraNotification = new DummyClassWithAttribute(Factory);

				var newModuleDecisionProvider = new Mock<IModuleDecisionProvider>();
				newModuleDecisionProvider.Setup(x => x.ShouldIgnoreAdditionalFilter).Returns(true);
				newModuleDecisionProvider.Setup(x => x.List).Returns(collectionWhichDoesCheckExtraNotification);

				module.ModuleDecisionProvider = newModuleDecisionProvider.Object;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals("Should return false as extra notification check is run", false, ((ZFilterModule)module).CheckValidSelectionForFindBox(new BusinessObjectFactory(), testBusinessObject));
				AssertEquals("Error message should be present as the extra notification is being added", "Error Always Check For Me!", UnitTestUserNotification.Instance.LastMessage.ToString());
			}
		}

		[ZFilterModuleAlwaysCheckExtraNotification]
		class DummyClassWithAttribute : DummyBusinessObjectCollection, IFilterModuleExtraNotificationProvider
		{
			public DummyClassWithAttribute(BusinessObjectFactory factory) : base(factory)
			{
			}

			INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
			{
				var dummy = businessObject as DummyBusinessObject;
				if (dummy.Z0_Code == "A")
				{
					return new Notification(CargoWise.ComponentModel.NotificationType.Error, "Always Check For Me!");
				}

				return null;
			}
		}

		class DummyClassWithoutAttribute : DummyBusinessObjectCollection, IFilterModuleExtraNotificationProvider
		{
			public DummyClassWithoutAttribute(BusinessObjectFactory factory) : base(factory)
			{
			}

			INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
			{
				var dummy = businessObject as DummyBusinessObject;
				if (dummy.Z0_Code == "A")
				{
					return new Notification(CargoWise.ComponentModel.NotificationType.Error, "Always Check For Me!");
				}

				return null;
			}
		}

		#endregion

		#region IFilterModuleExtraNotificationProvider

		public void TestIFilterModuleExtraNotificationProvider()
		{
			DeleteAllDummies();

			var dummyA = Factory.New<DummyBusinessObject>();
			dummyA.Z0_Code = "A";

			var dummyB = Factory.New<DummyBusinessObject>();
			dummyB.Z0_Code = "B";

			var dummyC = Factory.New<DummyBusinessObject>();
			dummyC.Z0_Code = "C";

			Factory.Save();

			using (var form = new ZChildForm())
			using (var module = CreateDummyFilterModule())
			{
				var collection = new DummyBizoCollectionForExtraNotificationTesting(Factory);
				collection.SetOverrideNotificationWhenAdditionalFilterNotMet("Additional filter not met");

				var moduleDecisionProvider = new Mock<IModuleDecisionProvider>();
				moduleDecisionProvider.Setup(x => x.ShouldIgnoreAdditionalFilter).Returns(true);
				moduleDecisionProvider.Setup(x => x.List).Returns(collection);

				module.ModuleDecisionProvider = moduleDecisionProvider.Object;
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IFilterModuleInternalsForTesting)module).PerformSearch();
				AssertEquals(3, module.GridList.Count);

				dummyA = (DummyBusinessObject)module.GridList.FindByPK(dummyA.PK);
				dummyB = (DummyBusinessObject)module.GridList.FindByPK(dummyB.PK);
				dummyC = (DummyBusinessObject)module.GridList.FindByPK(dummyC.PK);

				AssertHasRowError(dummyA, "Additional filter not met");
				AssertHasRowError(dummyB, "Chpok! Extra notification...");
				AssertEquals(false, dummyC.HasRowErrors);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals(false, ((ZFilterModule)module).CheckValidSelectionForFindBox(new BusinessObjectFactory(), dummyA));
				AssertEquals("Error Additional filter not met", UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals(false, ((ZFilterModule)module).CheckValidSelectionForFindBox(new BusinessObjectFactory(), dummyB));
				AssertEquals("Error Chpok! Extra notification...", UnitTestUserNotification.Instance.LastMessage.ToString());

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals(true, ((ZFilterModule)module).CheckValidSelectionForFindBox(new BusinessObjectFactory(), dummyC));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		class DummyBizoCollectionForExtraNotificationTesting : DummyBusinessObjectCollection, IFilterModuleExtraNotificationProvider
		{
			public DummyBizoCollectionForExtraNotificationTesting(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override ZQuery CreateAdditionalFilter()
			{
				return new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.NotContains, "A");
			}

			INotification IFilterModuleExtraNotificationProvider.GetExtraNotification(BusinessObject businessObject)
			{
				var dummy = businessObject as DummyBusinessObject;
				if (dummy.Z0_Code == "A")
				{
					throw new Exception("Should not be called for objects that are already not matched by the additional filter");
				}
				else if (dummy.Z0_Code == "B")
				{
					return new Notification(CargoWise.ComponentModel.NotificationType.Error, "Chpok! Extra notification...");
				}

				return null;
			}
		}

		#endregion

		public void TestSearch()
		{
			DeleteAllDummies();
			Factory.Save();

			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Text = "Module Test";
				form.Show();

				((IDummyFilterControl)testModule.EmbeddedControl).ResetFilter();
				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				AssertEquals("No search results found", 0, ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count);

				var anotherFactory = new BusinessObjectFactory();
				var dummy1 = anotherFactory.New<DummyBusinessObject>();
				var dummy2 = anotherFactory.New<DummyBusinessObject>();
				dummy1.Z0_Description = "";
				dummy2.Z0_Description = "";
				anotherFactory.Save();

				var prevFactory = ((IFilterModuleInternalsForTesting)testModule).GridCollection.Factory;
				var cachedBusinessObjectCount = ((IBusinessObjectFactoryInternals)prevFactory).NumberOfBusinessObjects;

				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				AssertEquals("search results found", 2, ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count);
				AssertEquals("Previous Factory should not be disposed. CachedBusinessObjectCount should not be changed.", cachedBusinessObjectCount, ((IBusinessObjectFactoryInternals)prevFactory).NumberOfBusinessObjects);

				dummy1.Z0_Description = "BLAH";
				dummy2.Z0_Description = "far";
				anotherFactory.Save();

				var txtFilter = (ModuleTextFilter)testModule.FilterBusinessObject[DummyBizoSchema.Z0_Description.Name];
				txtFilter.IsActive = true;
				txtFilter.Property = "BLAH";
				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				AssertEquals("search result found", 1, ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count);

				var gridFactoryBeforeDispose = ((IFilterModuleInternalsForTesting)testModule).GridCollection.Factory;
				var cachedCountBeforeDispose = ((IBusinessObjectFactoryInternals)gridFactoryBeforeDispose).NumberOfBusinessObjects;
				testModule.Dispose();
				AssertEquals("GridFactory should not be disposed when Module is disposed. CachedBusinessObjectCount should not be changed.", cachedCountBeforeDispose, ((IBusinessObjectFactoryInternals)gridFactoryBeforeDispose).NumberOfBusinessObjects);
			}
		}

		public void TestSearchWithCollectionView()
		{
			DeleteAllDummies();
			Factory.Save();

			using (var form = new ZChildForm())
			using (var testModule = new ZFilterModuleWithCollectionViewForTest())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Text = "Module Test";
				form.Show();

				((IDummyFilterControl)testModule.EmbeddedControl).ResetFilter();
				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				AssertEquals("No search results found", 0, testModule.GridCollection.Count);

				var dummy1 = Factory.New<DummyBusinessObject>();
				var dummy2 = Factory.New<DummyBusinessObject>();
				dummy1.Z0_Description = "BLAH";
				dummy2.Z0_Description = "BLAH";
				Factory.Save();

				var filter = new ModuleTextFilter("Description Filter", DummyBizoSchema.Z0_Description);
				filter.IsActive = true;
				filter.Property = "BLAH";

				testModule.FilterBusinessObject.ModuleFilters.AddFilter(filter);
				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				AssertEquals("search result should found 2 record", 2, testModule.GridCollection.Count);

				var anotherFactory = new BusinessObjectFactory();
				anotherFactory.RefreshEnabled = false;
				var dummy1InAnotherFactory = anotherFactory.Load<DummyBusinessObject>(dummy1.PK);
				dummy1InAnotherFactory.Z0_Description = "ABC";
				anotherFactory.Save();

				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				AssertEquals("search result should not found 1 record now", 1, testModule.GridCollection.Count);
			}
		}

		public void TestLoadedMaximumRows()
		{
			DeleteAllDummies();
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.New(typeof(DummyBusinessObject));
			factory2.New(typeof(DummyBusinessObject));
			factory2.New(typeof(DummyBusinessObject));
			factory2.New(typeof(DummyBusinessObject));
			factory2.Save();

			using (var form = new ZChildForm())
			using (var module = CreateDummyFilterModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IDummyFilterControl)module.EmbeddedControl).ResetFilter();
				((IDummyFilterControl)module.EmbeddedControl).FindButton.PerformButtonClick();
				AssertEquals("4 records found", 4, ((IDummyFilterControl)module.EmbeddedControl).GridList.Count);
			}

			using (var form = new ZChildForm())
			using (var module = CreateDummyFilterModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.ModifyMaxRowsToLoad(2);
				((IDummyFilterControl)module.EmbeddedControl).ResetFilter();
				((IDummyFilterControl)module.EmbeddedControl).FindButton.PerformButtonClick();
				var gridCollectionFactory = ((IFilterModuleInternalsForTesting)module).GridCollection.Factory;
				AssertEquals("MaximumRowsLoaded error message", "This search returns more than the maximum number of records to display.\r\nThe number of search records to display can be defined in the registry up to a maximum value of 15,000 records.\r\n\r\nSee: Registry > Physical Server > Display Grid > Max No. of Records to Show\r\n\r\nThe current value is set to 2.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("No business object should be constructed.", 0, ((IBusinessObjectFactoryInternals)gridCollectionFactory).NumberOfBusinessObjects);
			}

			using (var form = new ZChildForm())
			using (var module = CreateDummyFilterModule())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				((IDummyFilterControl)module.EmbeddedControl).ResetFilter();

				module.LoadingCollection += (factory) => { factory.LoadTop1<DummyBusinessObject>(new ZQuery()); };
				module.LoadedCollection += (factory) => { factory.LoadTop1<DummyBusinessObject>(new ZQuery()); };

				((IDummyFilterControl)module.EmbeddedControl).FindButton.PerformButtonClick();

				AssertEquals("4 records found", 4, ((IDummyFilterControl)module.EmbeddedControl).GridList.Count);
			}
		}

		[StressTest]
		public void TestFilterWithMaximumRows()
		{
			var rowsToShow = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;

			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var factory = new BusinessObjectFactory();
				for (var i = 0; i < rowsToShow; i++)
				{
					var dummyBusinessObject = factory.New<DummyBusinessObject>();
					dummyBusinessObject.Z0_Description = "";
				}
				factory.Save();

				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				Assert("Maximum number of rows is shown as expected", ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count == rowsToShow);

				const int numMoreRowsToShow = 10;
				for (var i = 0; i < numMoreRowsToShow; i++)
				{
					var dummyBusinessObject = factory.New<DummyBusinessObject>();
					dummyBusinessObject.Z0_Description = "";
				}
				factory.Save();

				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				Assert("The number of rows shown does not exceed the maximum", ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count <= rowsToShow);
			}
		}

		[StressTest]
		public void TestRunFilterWithExceedMaximumRowsOnActiveBusinessObjectCollection()
		{
			var factory = new BusinessObjectFactory();
			var rowsToShow = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
			for (var i = 0; i < rowsToShow + 1; i++)
			{
				var dummyBusinessObject = factory.New<DummyBusinessObject>();
				dummyBusinessObject.Z0_Description = "";
			}

			factory.Save();
			var originalValue = EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult;
			try
			{
				EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = true;
				using (var filter = new ZFilterModuleWithActiveCollectionForTest())
				{
					filter.PerformSearch();
					AssertNullOrEmpty("Should not report the developer notification 'Modifications have been disabled on this filter. No further changes should be made. Check the origin of this object (AdditionalFilter / RelationshipFilter etc)'",
						ErrorReporter.LastMessageReported);
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = originalValue;
			}
		}

		[StressTest]
		public void TestFilterWithTooManyRecordsShowsTooManyRecordsText()
		{
			var factory = new BusinessObjectFactory();
			var maxRows = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
			for (var i = 0; i < maxRows + 1; i++)
			{
				var dummyBusinessObject = factory.New<DummyBusinessObject>();
				dummyBusinessObject.Z0_Description = "";
			}

			factory.Save();
			var originalValue = EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult;
			try
			{
				EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = false;
				using (var module = new ZFilterModuleForTest())
				{
					var filterBizO = module.FilterBusinessObject;
					var query = string.Format("{0} IN (SELECT {0} FROM {1})",
						DummyBizoSchema.Constants.PK,
						DummyBizoSchema.Constants.TableName);

					var filter = new ModuleSQLFilter($"Custom SQL ({query})", filterBizO.QueryObjectType)
					{
						Property1 = query,
						Category = FilterCategories.Other,
						IsActive = true
					};

					filterBizO.ModuleFilters.AddFilter(filter);

					var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;
					filterControl.Find();

					AssertContains("Found\r\n  too many\r\n   records", filterControl.GetToolStripRecordsFoundLabelText());
				}
			}
			finally
			{
				EnvProxy.Instance.Registry.ShowExactRowCountOnExcessResult = originalValue;
			}
		}

		public void TestFilterWithExactMaximumRowsDoesntShowTooManyRecordsText()
		{
			var factory = new BusinessObjectFactory();
			var rowsToShow = SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.Value;
			for (var i = 0; i < rowsToShow; i++)
			{
				var dummyBusinessObject = factory.New<DummyBusinessObject>();
				dummyBusinessObject.Z0_Description = "";
			}

			factory.Save();
			using (var module = new ZFilterModuleForTest())
			{
				var filterBizO = module.FilterBusinessObject;
				var query = string.Format("{0} IN (SELECT TOP {1} {0} FROM {2})",
					DummyBizoSchema.Constants.PK,
					rowsToShow,
					DummyBizoSchema.Constants.TableName);

				var filter = new ModuleSQLFilter($"Custom SQL ({query})", filterBizO.QueryObjectType)
				{
					Property1 = query,
					Category = FilterCategories.Other,
					IsActive = true
				};

				filterBizO.ModuleFilters.AddFilter(filter);

				var filterControl = (ZFilterStripCommonControl)module.EmbeddedControl;
				filterControl.Find();

				AssertNotContains("Found\r\n  too many\r\n   records", filterControl.GetToolStripRecordsFoundLabelText());
			}
		}

		public void TestEnterIsNotProcessed()
		{
			DeleteAllDummies();
			Factory.Save();

			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Text = "Module Test";
				form.Show();

				((IDummyFilterControl)testModule.EmbeddedControl).ResetFilter();
				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				AssertEquals("No search results found", 0, ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count);

				var anotherFactory = new BusinessObjectFactory();
				var dummy1 = anotherFactory.New<DummyBusinessObject>();
				var dummy2 = anotherFactory.New<DummyBusinessObject>();
				dummy1.Z0_Description = "";
				dummy2.Z0_Description = "";
				anotherFactory.Save();

				var prevFactory = ((IFilterModuleInternalsForTesting)testModule).GridCollection.Factory;
				var cachedBusinessObjectCount = ((IBusinessObjectFactoryInternals)prevFactory).NumberOfBusinessObjects;

				((IDummyFilterControl)testModule.EmbeddedControl).FindButton.PerformButtonClick();
				AssertEquals("search results found", 2, ((IDummyFilterControl)testModule.EmbeddedControl).GridList.Count);
				AssertEquals("Previous Factory should not be disposed. CachedBusinessObjectCount should not be changed.", cachedBusinessObjectCount, ((IBusinessObjectFactoryInternals)prevFactory).NumberOfBusinessObjects);

				((IDummyFilterControl)testModule.EmbeddedControl).SelectRow(0);
				((IDummyFilterControl)testModule.EmbeddedControl).SelectRow(1);

				ZGrid grid = (testModule.EmbeddedControl as DummyFilterControl).FilteredGrid;
				KeySender.SendKeyDown(grid, grid.Handle, Keys.Enter);
				AssertEquals(2, (testModule.ModuleDecisionProvider as IDefaultActionCounter).DefaultActionsCount);
			}
		}

		public void TestEnterIsNotProcessed_ShowErrorMessage()
		{
			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Text = "Module Test";
				form.Show();

				var grid = (testModule.EmbeddedControl as DummyFilterControl).FilteredGrid;
				KeySender.SendKeyDown(grid, grid.Handle, Keys.Enter);

				AssertEquals("Please select a record in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestColumnsAreSetReadOnly()
		{
			DeleteAllDummies();
			Factory.Save();

			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				form.Controls.Add(testModule.EmbeddedControl);
				form.Text = "Module Test";
				form.Show();

				ZGrid grid = (testModule.EmbeddedControl as DummyFilterControl).FilteredGrid;
				AssertEquals(true, grid.ReadOnly);

				foreach (var columnStyle in grid.ColumnStyles.OfType<ZGridColumnInfo>())
				{
					AssertEquals(true, columnStyle.IsReadOnly);
				}
				grid.ExposeAllColumns();
				foreach (var column in grid.Columns)
				{
					AssertEquals(true, column.ColumnStyle.ReadOnly);
					//AssertEquals(true, ((ZGridColumnStyle)column.ColumnStyle).TextBox.ReadOnly); //For some reason this fails on DAT but not when I run it locally.
				}
			}
		}

		public void TestSetLimitedColumnsIfNoSecurityRight()
		{
			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				testModule.FilterBusinessObject.ParentModule.LimitedColumns = new ZLimitedColumnsProvider(DummyBizoSchema.Z0_Code, DummyBizoSchema.Z0_Description);
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var grid = (testModule.EmbeddedControl as DummyFilterControl).FilteredGrid;
				AssertEquals("Z0_Code", grid.LimitedColumns.CodeColumnName);
				AssertEquals("Z0_Description", grid.LimitedColumns.DescriptionColumnName);
			}
		}

		public void TestSetLimitedColumnsIfNoSecurityRight_InCaseOf_NoValidColumnSchemaForProvidedCodeProperty()
		{
			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				testModule.FilterBusinessObject.ParentModule.LimitedColumns = new ZLimitedColumnsProvider(typeof(DummyBizo));
				form.Controls.Add(testModule.EmbeddedControl);
				form.Show();

				var grid = (testModule.EmbeddedControl as DummyFilterControl).FilteredGrid;
				AssertEquals("Code", grid.LimitedColumns.CodeColumnName);
				AssertEquals("Description", grid.LimitedColumns.DescriptionColumnName);
			}
		}

		public void TestModuleDecisionProviderProperty()
		{
			using (var testModule = CreateDummyFilterModule())
			{
				AssertEquals("Unexpected decision provider type", testModule.ExpectedModuleDecisionProviderType, testModule.ModuleDecisionProvider.GetType());
			}
		}

		public virtual void TestGetModuleDecisionProviderForFindBox()
		{
			using (var testModule = CreateDummyFilterModule())
			{
				AssertEquals(typeof(DirectToFormModuleDecisionProvider), testModule.GetModuleDecisionProviderForFindBox().GetType());
			}
		}

		public virtual void TestGetModuleDecisionProviderForFindBoxPopup()
		{
			using (var testModule = CreateDummyFilterModule())
			{
				AssertEquals(typeof(PopupModuleDecisionProvider), testModule.GetModuleDecisionProviderForFindBoxPopup().GetType());
			}
		}

		public void TestGetModuleDecisionProviderForFindBoxPopup_AllowModuleMultiSelect()
		{
			using (var testModule = new ZFilterModuleForTest())
			{
				var findBox = new DummyFindBox();
				AssertEquals(typeof(PopupModuleDecisionProvider), testModule.GetModuleDecisionProviderForFindBoxPopup(findBox).GetType());

				findBox = new DummyFindBoxWithMultipleSelect();
				AssertEquals(typeof(PopupModuleDecisionProviderWithMultipleSelect), testModule.GetModuleDecisionProviderForFindBoxPopup(findBox).GetType());
			}
		}

		public void TestGlowReportingModuleDataQueryProvider()
		{
			using (var testModule = new ZFilterModuleForTest())
			{
				AssertEquals("ZFilterModule should be assinable to IGlowReportingModuleDataQueryProvider.", true, typeof(IGlowReportingModuleDataQueryProvider).IsAssignableFrom(testModule.GetType()));
				AssertNoExceptionThrown(() =>
				{
					((IGlowReportingModuleDataQueryProvider)testModule).BuildQuery();
				});
			}
		}

		public void TestDelegateDeleteToControllerIfAllowMultiDelete()
		{
			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;

				module.Selected = Array.Empty<BusinessObject>();
				module.ClickDelete();
				Assert(!controller.BatchDeleteWasCalled);

				module.Selected = new BusinessObject[] { Factory.New<DummyBusinessObject>(), Factory.New<DummyBusinessObject>() };
				module.ClickDelete();
				Assert(controller.BatchDeleteWasCalled);
				Assert(controller.BusinessObjectsToDelete == module.Selected);
			}
		}

		public void TestDeleteClick_ShowsDeleteFormForTemplateRecordIfApplicable()
		{
			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;

				var templateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
				var templateRecord = Factory.New<DummyTemplateRecord>();
				templateRecordProvider.TemplateRecord = templateRecord;

				Factory.Save();

				module.Selected = new BusinessObject[] { templateRecordProvider };
				module.CurrentBusinessObject = templateRecordProvider;
				module.ClickDelete();

				AssertEquals(
					"Should be cancelling the template record, not the provider",
					templateRecord.PK,
					(controller.LastShownForm.BusinessEntityForPersistingForm as BusinessObject).PK
				);

				controller.LastShownForm.Dispose();
				templateRecordProvider.TemplateRecord = null;
				module.ClickDelete();

				AssertEquals(
					"Since there is no template record, it should use the providing entity",
					templateRecordProvider.PK,
					(controller.LastShownForm.BusinessEntityForPersistingForm as BusinessObject).PK
				);

				controller.LastShownForm.Dispose();
			}
		}

		#region Mock

		class MockFilterModule : ZFilterModuleForTest
		{
			public BusinessObject CurrentBusinessObject;
			public BusinessObject[] Selected;
			public ZController Controller;

			protected override BusinessObject[] SelectedBusinessObjects
			{
				get { return Selected; }
			}

			protected override BusinessObject CurrentBusinessObjectInGrid => CurrentBusinessObject ?? base.CurrentBusinessObjectInGrid;

			protected override bool HasTypeErrorForSelectedBusinessObjects(BusinessObject selectedBusinessObject)
			{
				return HasTypeError_ForTest;  //Module which needs this check can override it
			}

			protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return Controller;
			}

			public void ClickDelete()
			{
				HandleDeleteClick(null, null);
			}

			public bool HasTypeError_ForTest;
		}

		class MockController : DummyController
		{
			public bool BatchDeleteWasCalled;
			public BusinessObject[] BusinessObjectsToDelete;

			protected override void DeleteMultipleCore(BusinessObject[] selectedBusinessObjects)
			{
				BatchDeleteWasCalled = true;
				BusinessObjectsToDelete = selectedBusinessObjects;
			}
		}

		#endregion

		public void TestHandleNewClick()
		{
			using (var form = new ZChildForm())
			using (var testModule = (CreateDummyFilterModule() as DummyFilterGridModule))
			{
				SetAllowNew(true);
				try
				{
					var newMenuItem = testModule.GetNewStandardMenuItemsExposed().FindByText("&New");

					newMenuItem.MenuItems[0].DefaultItem = true;
					newMenuItem.PerformClick();
					Assert("Default menu item performed", testModule.NewMenuClickPerformDefaultSubMenu);

					newMenuItem.MenuItems[0].DefaultItem = false;
					newMenuItem.PerformClick();
					Assert("Default menu item not performed", testModule.NewMenuClickPerformDefaultSubMenu);
				}
				finally
				{
					ResetAllowNewToDefault();
				}
			}
		}

		public void TestHandleTemplateCopyClick_CallsShowTemplateCopyFormWithThreadSafeBusinessObject()
		{
			using (var testFilterModule = new ZFilterModuleForTestHandleTemplateCopyClick())
			{
				var dummy = new DummyBusinessObjectCollection(Factory);
				dummy.AddNew();
				testFilterModule.DummySelectedBusinessObjects = dummy;

				testFilterModule.OverrideCurrentThreadBusinessObjectLoader_ForTest(new MockCurrentThreadBusinessObjectLoader(Factory));

				AssertNotNull(testFilterModule.CurrentBusinessObjectInGrid);

				testFilterModule.ShowFormsFromMainThread = true;
				testFilterModule.HandleTemplateCopyClick(null, null);

				var lastController = testFilterModule.LastController as ZControllerForTestHandleTemplateCopyClick;

				AssertNotNull("Last controller should be of type ZControllerForTestHandleTemplateCopyClick.", lastController);

				AssertType<MockThreadSafeBusinessObject>("ShowTemplateCopyForm should be called with a thread safe business object.", testFilterModule.SelectedBusinessObject);
				AssertType<MockThreadSafeBusinessObject>("ShowTemplateCopyForm should be called with a thread safe business object.", lastController.InMemorySourceEntity);
			}
		}

		public void TestHandleTemplateCopyClick_NotAllowedIfTemplateRecord()
		{
			using (var testFilterModule = new ZFilterModuleForTestHandleTemplateCopyClick())
			{
				var dummyCollection = new DummyBusinessObjectCollection(Factory);
				var templateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
				var templateRecord = Factory.New<DummyTemplateRecord>();
				templateRecordProvider.TemplateRecord = templateRecord;
				dummyCollection.Add(templateRecordProvider);

				testFilterModule.DummySelectedBusinessObjects = dummyCollection;
				testFilterModule.ShowFormsFromMainThread = true;
				testFilterModule.HandleTemplateCopyClick(null, null);

				AssertEquals(
					"The Copy function cannot be used to copy template records. Instead, use the Universal Copy function to create new records from templates.",
					UnitTestUserNotification.Instance.LastMessage.Text
				);
			}
		}

		public void TestHandleCopyAndReverseClick_NotAllowedIfTemplateRecord()
		{
			using (var testFilterModule = new ZFilterModuleForTestHandleTemplateCopyClick())
			{
				var dummyCollection = new DummyBusinessObjectCollection(Factory);
				var templateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
				var templateRecord = Factory.New<DummyTemplateRecord>();
				templateRecordProvider.TemplateRecord = templateRecord;
				dummyCollection.Add(templateRecordProvider);

				testFilterModule.DummySelectedBusinessObjects = dummyCollection;
				testFilterModule.ShowFormsFromMainThread = true;
				testFilterModule.HandleCopyAndReverseClick(null, null);

				AssertEquals(
					"The Copy function cannot be used to copy template records. Instead, use the Universal Copy function to create new records from templates.",
					UnitTestUserNotification.Instance.LastMessage.Text
				);
			}
		}

		public class ZFilterModuleForTestHandleTemplateCopyClick : ZFilterModuleForTest
		{
			public new void HandleTemplateCopyClick(object sender, EventArgs e) => base.HandleTemplateCopyClick(sender, e);
			public new void HandleCopyAndReverseClick(object sender, EventArgs e) => base.HandleCopyAndReverseClick(sender, e);

			public DummyBusinessObjectCollection DummySelectedBusinessObjects
			{
				get; set;
			}

			public new BusinessObject CurrentBusinessObjectInGrid
			{
				get
				{
					return base.CurrentBusinessObjectInGrid;
				}
			}

			public BusinessObject SelectedBusinessObject { get; private set; }

			protected override Control GetNewEmbeddedControl()
			{
				return (Control)GetNewFilterControlForGrid();
			}

			protected override void OnSetupAndGetGrid(ZDisplayGrid grid)
			{
				grid.SetDataBinding(DummySelectedBusinessObjects, "");
				base.OnSetupAndGetGrid(grid);
			}

			protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
			{
				return new ZControllerForTestHandleTemplateCopyClick();
			}

			protected override IZForm ShowTemplateCopyForm(BusinessObject selectedBusinessObject)
			{
				this.SelectedBusinessObject = selectedBusinessObject;
				return base.ShowTemplateCopyForm(selectedBusinessObject);
			}
		}

		class MockCurrentThreadBusinessObjectLoader : CurrentThreadBusinessObjectLoader
		{
			readonly BusinessObjectFactory Factory;

			public MockCurrentThreadBusinessObjectLoader(BusinessObjectFactory f)
			{
				this.Factory = f;
			}

			protected override BusinessObject GetOnCurrentThreadCore(BusinessObject maybeUnsafeBizo, string sourceFilePath)
			{
				return Factory.New<MockThreadSafeBusinessObject>();
			}
		}

		class MockThreadSafeBusinessObject : DummyBusinessObject
		{
			public MockThreadSafeBusinessObject(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}
		}

		public class ZControllerForTestHandleTemplateCopyClick : ZController
		{
			public BusinessObject InMemorySourceEntity { get; private set; }

			public override IZForm ShowTemplateCopyForm(BusinessObject inMemorySourceEntity)
			{
				InMemorySourceEntity = inMemorySourceEntity;

				return null;
			}

			#region virtual
			public override ControllerID ID => throw new NotImplementedException();

			public override ModuleIdentifier ModuleID => throw new NotImplementedException();

			public override Type TypeOfTopLevelBusinessObject => throw new NotImplementedException();

			public override bool MakeUrlsOnlyOpenableForCurrentCompany => throw new NotImplementedException();

			protected override SecurityCheckpoint CheckPointForView => throw new NotImplementedException();

			protected override SecurityCheckpoint CheckPointForNew => throw new NotImplementedException();

			protected override SecurityCheckpoint CheckPointForEdit => throw new NotImplementedException();

			protected override SecurityCheckpoint CheckPointForDelete => throw new NotImplementedException();

			protected override IZForm GetForm(IBusiness businessEntity)
			{
				throw new NotImplementedException();
			}
			#endregion
		}

		public void TestAllowNew()
		{
			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				SetAllowNew(false);
				try
				{
					AssertNotNull(testModule.GetContextMenuItemByText("&View"));
					AssertNull(testModule.GetContextMenuItemByText("&New"));
				}
				finally
				{
					ResetAllowNewToDefault();
				}
			}
		}

		public void TestAllowEdit()
		{
			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				testModule.SetAllowEdit(false);
				AssertNotNull(testModule.GetContextMenuItemByText("&View"));
				AssertNull(testModule.GetContextMenuItemByText("&Edit"));
			}
		}

		public void TestAllowView()
		{
			using (var form = new ZChildForm())
			using (var testModule = CreateDummyFilterModule())
			{
				testModule.SetAllowEdit(false);
				testModule.SetAllowView(false);
				AssertNoExceptionThrown(delegate
				{ testModule.ModuleDecisionProvider.HandleFindBoxOKButton(new BusinessObject[] { Factory.New<DummyBusinessObject>() }); });
			}
		}

		public void TestAllowDelete()
		{
			using (var testModule = CreateDummyFilterModule())
			{
				testModule.SetAllowDelete(false);
				AssertNotNull(testModule.GetContextMenuItemByText("&View"));
				AssertNull(testModule.GetContextMenuItemByText("&Delete"));

				var module = ((DummyFilterGridModule)testModule);
				module.SelectedBusinessObjectsOverride = new BusinessObject[] { Factory.New<DummyBusinessObject>() };
				AssertNoExceptionThrown(() => module.HandleDeleteClick_Exposed(null, EventArgs.Empty));
			}
		}

		public void TestDeleteWhileHasActionsIsFalse()
		{
			try
			{
				DummyFilterGridModule.HasActionsExposed = false;
				using (var testModule = CreateDummyFilterModule())
				{
					var module = ((DummyFilterGridModule)testModule);
					module.SelectedBusinessObjectsOverride = new BusinessObject[] { Factory.New<DummyBusinessObject>() };
					AssertNoExceptionThrown(() => module.HandleDeleteClick_Exposed(null, EventArgs.Empty));
				}
			}
			finally
			{
				DummyFilterGridModule.HasActionsExposed = true;
			}
		}

		public void TestAddingDomainValidationGroupToController()
		{
			using (var testModule = CreateDummyFilterModule())
			{
				testModule.ShowNewForm().Dispose();
				Assert("Pre-condition", !testModule.LastController.Factory.Validation.HasDomainValidation);

				testModule.Factory.Validation.MainGroup.RegisterValidationType<DummyBusinessObject, DummyBizoValidation>();
				testModule.ShowNewForm().Dispose();
				Assert("Should have domain validation now", testModule.LastController.Factory.HasDomainValidation);
			}
		}

		[ExpectNoExceptions]
		public void TestAddingDomainValidationGroupToController_NullFactory()
		{
			using (var testModule = CreateDummyFilterModule())
			using (var findBox = new ZGuidFindBox())
			{
				findBox.List = new DummyBusinessObjectCollection(null);
				testModule.ModuleDecisionProvider = new DummyModuleDecisionProvider(findBox);
				AssertNull("Sanity check", testModule.ModuleDecisionProvider.List.Factory);

				testModule.ShowNewForm().Dispose();
			}
		}

		[ExpectNoExceptions]
		public void TestFilterModuleStrategy()
		{
			var strategy = new FilterModuleStrategyForTest();
			var strategyList = new List<FilterModuleStrategy>();
			strategyList.Add(strategy);

			using (ObjectFactory.Substitute("FilterModuleStrategies", strategyList))
			using (var form = new ZChildForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				form.Controls.Add(module.EmbeddedControl);

				form.Show();
				var filterControl = (DummyFilterControl)module.EmbeddedControl;
				AssertEquals(filterControl, strategy.Control);
				AssertNotNull("Should contain a filter called 'Z0_Description'", filterControl.FilterBusinessObject.ModuleFilters["Z0_Description"]);
				AssertNotNull("Should contain a filter called 'Z0_Description'" + FilterModuleStrategy.UniqueSuffix,
					filterControl.FilterBusinessObject.ModuleFilters["Z0_Description" + FilterModuleStrategy.UniqueSuffix]);
				AssertEquals(typeof(DummyBusinessObject), strategy.BizObjType);
				AssertEquals(filterControl.FilterBusinessObject.ModuleFilters, strategy.Filters);
			}
		}

		[ExpectNoExceptions]
		public void TestShowEditFormWithDummyNonPersistentBusinessObject()
		{
			var dummy = new DummyNonPersistent();

			using (var module = new MockFilterModule())
			{
				var controller = new MockController();
				module.Controller = controller;

				using (var form = module.ShowEditForm(dummy))
				{
				}
			}
		}

		[ExpectNoExceptions]
		public void TestTypeOfTopLevelBusinessObject_NoController()
		{
			using (var module = new DummyFilterModuleWithNoController())
			{
				_ = module.TypeOfTopLevelBusinessObject;
			}
		}

		public void TestCheckValidSelectionForFindBox_WhenComesFromRecentItems_ShouldFilteredByReadonlyAndAlwaysFilters()
		{
			var dummyA = Factory.New<DummyBusinessObject>();
			dummyA.Z0_Code = "codeA";
			dummyA.Z0_Description = "descriptionDummyA";
			dummyA.Z0_Bool = true;
			dummyA.Z0_Number = 1;
			var dummyB = Factory.New<DummyBusinessObject>();
			dummyB.Z0_Code = "codeB";
			dummyB.Z0_Description = "descriptionDummyB";
			dummyB.Z0_Bool = false;
			dummyB.Z0_Number = 2;
			var dummyC = Factory.New<DummyBusinessObject>();
			dummyC.Z0_Code = "codeC";
			dummyC.Z0_Description = "descriptionDummyC";
			dummyC.Z0_Bool = false;
			dummyC.Z0_Number = 3;
			var dummyD = Factory.New<DummyBusinessObject>();
			dummyD.Z0_Code = "codeD";
			dummyD.Z0_Description = "descriptionDummyD";
			dummyD.Z0_Bool = true;
			dummyD.Z0_Number = 4;
			Factory.Save();

			using (var testModule = new DummyFilterGridModuleWithABOCForRecentItemsTest())
			{
				var filterBizo = testModule.FilterBusinessObject;

				var filter1 = (ModuleTextFilter)filterBizo.ModuleFilters["Z0_Description"];
				filter1.IsActive = true;
				filter1.ReadOnly = true;
				filter1.Visibility = FilterVisibility.AlwaysVisible;
				filter1.Property = "description";
				filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

				var filter2 = (ModuleTextFilter)filterBizo.ModuleFilters["Z0_Code"];
				filter2.IsActive = true;
				filter2.Property = "codeB";
				filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

				var filter3 = (ModuleNumberRangeFilter)filterBizo.ModuleFilters["Z0_Number"];
				filter3.IsActive = true;
				filter3.Property1 = 2;
				filter3.Property2 = 2;

				testModule.PerformSearch();

				using (testModule.SetRecentItemsSelection())
				{
					CombineAssertions(() =>
					{
						AssertEquals("dummyA is opened in recent items, it should be checked by readonly & always visible filters, default relationship filter", true, testModule.CheckValidSelectionForFindBox(new[] { dummyA }));
						AssertEquals("dummyB is opened in recent items, it should be checked by readonly & always visible filters, default relationship filter", false, testModule.CheckValidSelectionForFindBox(new[] { dummyB }));
						AssertEquals("dummyC is opened in recent items, it should be checked by readonly & always visible filters, default relationship filter", false, testModule.CheckValidSelectionForFindBox(new[] { dummyC }));
						AssertEquals("dummyD is opened in recent items, it should be checked by readonly & always visible filters, default relationship filter", true, testModule.CheckValidSelectionForFindBox(new[] { dummyD }));
					});
				}
			}

			using (var testModule = new DummyFilterGridModuleWithBOCForRecentItemsTest())
			{
				var filterBizo = testModule.FilterBusinessObject;

				var filter1 = (ModuleTextFilter)filterBizo.ModuleFilters["Z0_Description"];
				filter1.IsActive = true;
				filter1.ReadOnly = true;
				filter1.Visibility = FilterVisibility.AlwaysVisible;
				filter1.Property = "description";
				filter1.ComparisonOperator = ModuleTextFilter.ComparisonConstants.StartsWith;

				var filter2 = (ModuleTextFilter)filterBizo.ModuleFilters["Z0_Code"];
				filter2.IsActive = true;
				filter2.Property = "codeB";
				filter2.ComparisonOperator = ModuleTextFilter.ComparisonConstants.Exact;

				var filter3 = (ModuleNumberRangeFilter)filterBizo.ModuleFilters["Z0_Number"];
				filter3.IsActive = true;
				filter3.Property1 = 3;
				filter3.Property2 = 3;

				testModule.PerformSearch();

				using (testModule.SetRecentItemsSelection())
				{
					CombineAssertions(() =>
					{
						AssertEquals("dummyA is opened in recent items, it should be checked by readonly & always visible filters, default relationship filter, default additional filter", false, testModule.CheckValidSelectionForFindBox(new[] { dummyA }));
						AssertEquals("dummyB is opened in recent items, it should be checked by readonly & always visible filters, default relationship filter, default additional filter", false, testModule.CheckValidSelectionForFindBox(new[] { dummyB }));
						AssertEquals("dummyC is opened in recent items, it should be checked by readonly & always visible filters, default relationship filter, default additional filter", false, testModule.CheckValidSelectionForFindBox(new[] { dummyC }));
						AssertEquals("dummyD is opened in recent items, it should be checked by readonly & always visible filters, default relationship filter, default additional filter", true, testModule.CheckValidSelectionForFindBox(new[] { dummyD }));
					});
				}
			}
		}

		public void TestCheckValidSelectionForFindBox_WhenComesFromRecentItemsAndGridCollectionIsActiveBusinessObjectCollection()
		{
			using (var testModule = new ZFilterModuleForTest())
			using (testModule.SetRecentItemsSelection())
			{
				AssertNoExceptionThrown(() => testModule.CheckValidSelectionForFindBox(new[] { Dummy }));
			}
		}

		class DummyFilterGridModuleWithBOCForRecentItemsTest : DummyFilterGridModule
		{
			protected override IBusinessObjectCollection GetNewGridCollection() => new BOC(Factory);

			public class BOC : DummyBusinessObjectCollection
			{
				public BOC(BusinessObjectFactory factory) : base(factory)
				{
				}

				protected override ZQuery CreateAdditionalFilter()
				{
					var query = base.CreateAdditionalFilter();
					query.AddToFilter(DummyBizoSchema.Z0_Number, SQLComparisonOperator.GreaterThanOrEqualTo, 3);

					return query;
				}

				protected override ZQuery CreateRelationshipFilter()
				{
					var query = base.CreateRelationshipFilter();
					query.AddToFilter(DummyBizoSchema.Z0_Bool, true);

					return query;
				}
			}
		}

		class DummyFilterGridModuleWithABOCForRecentItemsTest : DummyFilterGridModule
		{
			protected override IBusinessObjectCollection GetNewGridCollection() => new ABOC(Factory);

			public class ABOC : ActiveBusinessObjectCollection<DummyBusinessObject>
			{
				public ABOC(BusinessObjectFactory factory) : base(factory)
				{
				}

				protected override ZQuery CreateRelationshipFilter()
				{
					var query = base.CreateRelationshipFilter();
					query.AddToFilter(DummyBizoSchema.Z0_Bool, true);

					return query;
				}
			}
		}

		class DummyNonPersistent : NonPersistentBusinessObject { }

		#region TestLoadSortedCollectionWithCustomProperty

		public void TestLoadSortedCollectionWithCustomProperty()
		{
			var anotherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var dummy1 = anotherFactory.New<DummyBusinessObject>();
			var dummy2 = anotherFactory.New<DummyBusinessObject>();
			dummy1.Z0_Description = string.Empty;
			dummy2.Z0_Description = string.Empty;
			anotherFactory.Save();

			using (var form = new ZChildForm())
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var columnInfo = new ZTextBoxColumnStyleInfo("Abc Efg", 80) { IsVisible = true };
				((IOverridablePropertyDescriptor)columnInfo).PropertyDescriptor = new MockCustomPropertDescriptor("Abc Efg", typeof(string));
				module.Grid.ColumnStyles.Add(columnInfo);
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch();
				module.GridCollection.ApplySort(((IOverridablePropertyDescriptor)columnInfo).PropertyDescriptor, ListSortDirection.Ascending);

				AssertNoExceptionThrown(() => module.PerformSearch());

				AssertEquals("Abc Efg", module.GridCollection.SortInformation.PropertyName);
			}
		}

		class MockCustomPropertDescriptor : ZCustomPropertyDescriptor
		{
			public MockCustomPropertDescriptor(string identifier, Type propertyType)
				: base(identifier, propertyType)
			{
			}

			protected override object GetValueCore(object component)
			{
				return string.Empty;
			}

			protected override void SetValueCore(object component, object value)
			{
			}
		}

		#endregion

		#region Implementation

		protected virtual IDummyFilterModule CreateDummyFilterModule()
		{
			return (IDummyFilterModule)ZModuleFactory.Instance.Create(ModuleID);
		}

		protected abstract ModuleIdentifier ModuleID { get; }
		protected abstract void SetActiveFilter(ZQuery query);
		protected abstract void SetAllowNew(bool value);
		protected abstract void ResetAllowNewToDefault();

		protected override void TearDown()
		{
			base.TearDown();
			SetActiveFilter(new ZQuery());
		}

		#endregion
	}

	#region Test classes

	class DummyModuleDecisionProvider : ModuleDecisionProvider
	{
		public DummyModuleDecisionProvider(ZGuidFindBox findBox)
			: base(findBox, new PopupControllerLink(findBox))
		{
		}

		public override bool EnablePreviousNextSupport
		{
			get { return false; }
		}

		public override bool ShouldLoadFilterBizObj
		{
			get { return shouldLoadFilterBizObj; }
		}

		public void SetShouldLoadFilterBizObj(bool value)
		{
			shouldLoadFilterBizObj = value;
		}	

		public bool ShouldAllowExcelImport
		{
			get { return shouldAllowExcelImport; }
			set { shouldAllowExcelImport = value; }
		}

		public override bool AllowExcelExport
		{
			get { return shouldAllowExcelImport; }
		}

		bool shouldAllowExcelImport;
		bool shouldLoadFilterBizObj;
	}

	class TestPopupModuleDecisionProvider : PopupModuleDecisionProvider
	{
		public TestPopupModuleDecisionProvider(IFindBox findBox)
			: base(findBox)
		{
		}

		public override IBusinessObjectCollection List
		{
			get { return null; }
		}
	}

	class ZFilterModuleWithCollectionViewForTest : ZFilterModuleForTest
	{
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var originalCollection = new DummyBusinessObjectCollection(Factory);
			return new DummyViewCollection(originalCollection).CollectionToFilter;
		}
	}

	class ActiveDummyCollection : ActiveBusinessObjectCollection<DummyBusinessObject>
	{
		public ActiveDummyCollection(BusinessObjectFactory factory) : base(factory)
		{
		}
	}

	class ZFilterModuleWithActiveCollectionForTest : ZFilterModuleForTest
	{
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			var collection = new ActiveDummyCollection(Factory);
			return collection;
		}
	}

	class DummyFilterModuleWithNoController : ZFilterModuleForTest
	{
		protected override Type TypeOfTopLevelBusinessObjectCore => typeof(BusinessObject);
	}

	class ZFilterModuleForTest : ZFilterGridModule
	{
		public ZQuery GetExportQuery()
		{
			return ExportQuery;
		}

		#region Abstract Members Implementation Dummies

		protected override Licensing.LicenceCheckpoint LicenceCheckPointCore
		{
			get { throw new NotImplementedException(); }
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return EnvProxy.Instance.Security.None as SecurityCheckpoint; }
		}

		public override ModuleIdentifier ID
		{
			get { return DummyModuleIDs.Dummy; }
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new DummyFilterStripBusinessObjectOverride();
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new DummyFilterControl(GetNewGridCollection(), (FilterStripBusinessObject)GetNewFilterBusinessObject());
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory);
		}

		protected internal override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			throw new NotImplementedException();
		}

		public class DummyFilterStripBusinessObjectOverride : DummyFilterStripBusinessObject
		{
			public DummyFilterStripBusinessObjectOverride()
				: base()
			{
			}

			public class Schema
			{
				public const string TableName = "DummyFilterStripBusinessObjectOverride";
			}

			protected override void SetLastUsedLayoutValue(StmData lastUsedFilterStmData)
			{
				throw new ZCannotSaveException(string.Empty, string.Empty);
			}
		}

		#endregion

		public void AddImportDataMenuItems()
		{
			AddImportDataMenuItem("with license", null);
			AddImportDataMenuItem("without license", null, false);
		}

		public void AddInterfaceConnectorImportMenuItem(bool includeAccessAllowedCheck)
		{
			AddInterfaceConnectorImportMenuItem("menu", null, includeAccessAllowedCheck);
		}

		public void AddInterfaceConnectorExportMenuItem(bool includeAccessAllowedCheck)
		{
			AddInterfaceConnectorExportMenuItem("To &XML", null, includeAccessAllowedCheck);
		}

		internal void AddInterfaceConnectorCSVImportMenuItem()
		{
			AddInterfaceConnectorCSVImportMenuItem(CommonDataTransferCaptions.FromCsvMenuText, delegate
			{ });
		}

		protected override void ExportVisibleIntoAndOpenExcel()
		{
			throw new NotImplementedException();
		}

		public FilterModuleMenuItemDescriptorCollection ImportMenuItems_Exposed
		{
			get
			{
				return base.ImportMenuItems;
			}
		}

		public FilterModuleMenuItemDescriptorCollection ExportMenuItems_Exposed
		{
			get
			{
				return base.ExportMenuItems;
			}
		}

		public bool HasInterfaceConnector
		{
			get { return eHubMessagingRegistry.Instance.HasInterfaceConnector; }
		}
	}

	class FilterModuleStrategyForTest : FilterModuleStrategy
	{
		public IFilterControl Control { get; set; }

		public ModuleFilterCollection Filters
		{
			get { return AllFiltersForTest; }
		}

		public Type BizObjType
		{
			get { return BizoType; }
		}

		public override void RunOnFilterControlInitialisation(IFilterControl control, IBusinessObjectCollection gridCollection)
		{
			Control = control;
		}

		protected override IEnumerable<ModuleFilter> FiltersToAdd
		{
			get
			{
				yield return new ModuleTextFilter("Z0_Description", DummyBizoSchema.Z0_VarCharMax);
			}
		}
	}

	class DummyViewCollection : BusinessObjectCollectionView<DummyBusinessObject>
	{
		public DummyViewCollection(BusinessObjectCollection collectionToFilter)
			: base(collectionToFilter)
		{
		}

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			return true;
		}

		public override void Load()
		{
			collectionToFilter.Load();
		}
	}

	#endregion

	[UseSnapshotProtection]
	class ZFilterModuleQueryHintFallbackTest : TestCase
	{
		public void TestRunLightQuery()
		{
			using (var filter = new ZFilterForQueryHintFallback("SELECT COUNT(*) FROM dbo.StmNums"))
			{
				filter.PerformSearch();
				AssertEquals("Read from first attempt", 1, filter.LoadCount);

				var message = UnitTestUserNotification.Instance.LastMessage;
				Assert("Warning message", message.WasNone);
			}
		}

#if !WINZOR
		public void TestRunWithFail_8003_TooManyParameters()
		{
			var query = new ZQuery();
			query.TableIndexHints.Add(new TableIndexHint("IDX"));

			using (var form = new ZForm())
			using (var filter = new ZFilterForQueryHintFallbackWrongException(query))
			{
				filter.PerformSearch(8003, "Weeeeeeeeee");
				AssertEquals("The incoming request has too many parameters. The server supports a maximum of 2100 parameters. Reduce the number of parameters and resend the request.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
#endif

#if !WINZOR
		public void TestRunWithFail_8618_MinimumRowSizeExceeds()
		{
			var query = new ZQuery();
			query.TableIndexHints.Add(new TableIndexHint("IDX"));

			using (var form = new ZForm())
			using (var filter = new ZFilterForQueryHintFallbackWrongException(query))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				filter.PerformSearch(8618, "Weeeeeeeeee", true);

				AssertEquals(@"The query processor could not produce a query plan because a worktable is required, and its minimum row size exceeds the maximum allowable of 8060 bytes.
A typical reason why a worktable is required is a GROUP BY or ORDER BY clause in the query.
If the query has a GROUP BY or ORDER BY clause, consider reducing the number and/or size of the fields in the clause.
Consider using prefix (LEFT()) or hash (CHECKSUM()) of fields for grouping or prefix for ordering.
Note however that this will change the behavior of the query.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}
#endif

		public void TestRunWithHintsFail_8622_RejectRetry()
			=> AssertRunWithHintsFail_RejectRetry(8622, "Query processor could not produce a query plan because of the hints defined in this query. Resubmit the query without specifying any hints and without using SET FORCEPLAN.");

		public void TestRunWithHintsFail_8618_RejectRetry()
			=> AssertRunWithHintsFail_RejectRetry(8618, "Query processor could not produce query plan because a worktable is required, and its minimum size exceeds the maximum allowable.");

		void AssertRunWithHintsFail_RejectRetry(int errorNumber, string errorMessage)
		{
			var query = new ZQuery();
			query.TableIndexHints.Add(new TableIndexHint("IDX"));

			using (var form = new ZForm())
			using (var filter = new ZFilterForQueryHintFallbackWrongException(query))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				filter.SetFormsModalTo(form);

				filter.PerformSearch(errorNumber, errorMessage);

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length);

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				AssertEquals(@"The search criteria you have provided has created a query that may take a long time to run.
It is better to provide more precise criteria that may result in a smaller number of results.
Alternately it may also be better to provide simpler search criteria to decrease the work being performed.
Consider using starts with or exact criteria, or restricting the amount of data by using smaller date ranges.

Technical information : SQL Server was unable to generate an execution plan for this query in conjunction with the recommended SQL query hints indicated by the search criteria. The query can be retried without the recommended SQL query hints, but this could result in unexpected performance implications.

Are you sure you want to run it?", message.Text);
				AssertEquals("ParentComponent should be parent form", form, message.ParentComponent);

				filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.Is<ZString>(s => s.Contains("Query execution has been canceled to prevent unexpected performance degradation.", StringComparison.OrdinalIgnoreCase)), 0, true), Times.Once);
			}
		}

		public void TestRunWithHintsFail_8622_AcceptRetry()
			=> AssertRunWithHintsFail_AcceptRetry(8622, "Query processor could not produce a query plan because of the hints defined in this query. Resubmit the query without specifying any hints and without using SET FORCEPLAN.");

		public void TestRunWithHintsFail_8618_AcceptRetry()
			=> AssertRunWithHintsFail_AcceptRetry(8618, "Query processor could not produce query plan because a worktable is required, and its minimum size exceeds the maximum allowable.");

		void AssertRunWithHintsFail_AcceptRetry(int errorNumber, string errorMessage)
		{
			var query = new ZQuery();
			query.TableIndexHints.Add(new TableIndexHint("IDX"));

			using (var filter = new ZFilterForQueryHintFallbackWrongException(query))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				filter.PerformSearch(errorNumber, errorMessage);

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length);

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				AssertEquals(@"The search criteria you have provided has created a query that may take a long time to run.
It is better to provide more precise criteria that may result in a smaller number of results.
Alternately it may also be better to provide simpler search criteria to decrease the work being performed.
Consider using starts with or exact criteria, or restricting the amount of data by using smaller date ranges.

Technical information : SQL Server was unable to generate an execution plan for this query in conjunction with the recommended SQL query hints indicated by the search criteria. The query can be retried without the recommended SQL query hints, but this could result in unexpected performance implications.

Are you sure you want to run it?", message.Text);

				filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.Is<ZString>(s => s.Contains("There are no records that match your search.", StringComparison.OrdinalIgnoreCase)), 0, true), Times.Exactly(2));
			}
		}

		public void TestRunWithHintsFail_8622_SecurityNotAllowed()
			=> AssertRunWithHintsFail_SecurityNotAllowed(8622, "Query processor could not produce a query plan because of the hints defined in this query. Resubmit the query without specifying any hints and without using SET FORCEPLAN.");

		public void TestRunWithHintsFail_8618_SecurityNotAllowed()
			=> AssertRunWithHintsFail_SecurityNotAllowed(8618, "Query processor could not produce query plan because a worktable is required, and its minimum size exceeds the maximum allowable.");

		void AssertRunWithHintsFail_SecurityNotAllowed(int errorNumber, string errorMessage)
		{
			Env.Security.RunExtremelyLongFindQuery.IsAllowed = false;

			var query = new ZQuery();
			query.TableIndexHints.Add(new TableIndexHint("IDX"));

			using (var filter = new ZFilterForQueryHintFallbackWrongException(query))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				filter.PerformSearch(errorNumber, errorMessage);

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be one message", 1, messages.Length);
				Assert("Warning message", UnitTestUserNotification.Instance.LastMessage.WasNone);

				filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.Is<ZString>(s => s.Contains(@"The search criteria you have provided has created a query that may take a long time to run.
It is better to provide more precise criteria that may result in a smaller number of results.
Alternately it may also be better to provide simpler search criteria to decrease the work being performed.
Consider using starts with or exact criteria, or restricting the amount of data by using smaller date ranges. To run this query you are required to have 'Specialized Rights -> Allow to Run Query with Unlimited Estimated Cost on Find Screens' security right.

Technical information : SQL Server was unable to generate an execution plan for this query in conjunction with the recommended SQL query hints indicated by the search criteria.")), 0, true), Times.Once);
			}
		}

		class ZFilterForQueryHintFallbackWrongException : ZFilterModuleWithCollectionViewWithFilterControlForTest
		{
			readonly ZQuery query;

			public ZFilterForQueryHintFallbackWrongException(ZQuery query)
			{
				this.query = query;
			}

			protected override ZQuery GetDisplayResultsQuery() => query;

			public void PerformSearch(int errorNumber, string errorMessage, bool throwSqlExceptionAgain = false)
			{
				this.errorNumber = errorNumber;
				this.errorMessage = errorMessage;
				this.throwSqlExceptionAgain = throwSqlExceptionAgain;
				base.PerformSearch();
			}
			int errorNumber;
			string errorMessage;
			bool throwSqlExceptionAgain;

			protected override FilteredGridLoader CreateSearchManager()
				=> new GridLoader(this, FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

			class GridLoader : FilteredGridLoader
			{
				readonly ZFilterForQueryHintFallbackWrongException parent;

				public GridLoader(ZFilterForQueryHintFallbackWrongException parent, FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
					: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
				{
					this.parent = parent;
				}

				protected override BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
				{
					if (query.TableIndexHints.Any())
					{
						throw NewSqlException(parent.errorNumber, parent.errorMessage);
					}
					else if (parent.throwSqlExceptionAgain)
					{
						throw NewSqlException(parent.errorNumber, parent.errorMessage);
					}
					return base.LoadCollectionCore(factory, type, query);
				}

				static T Construct<T>(params object[] p)
				{
					var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
					return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
				}

				static SqlException NewSqlException(int number, string errorMessage = "error message", string proc = "proc", string serverName = "server name")
				{
					var collection = Construct<SqlErrorCollection>();
					var error = Construct<SqlError>(number, (byte)2, (byte)3, serverName, errorMessage, proc, 100);

					typeof(SqlErrorCollection)
						.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
						.Invoke(collection, new object[] { error });

					return typeof(SqlException)
						.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
							null,
							CallingConventions.ExplicitThis,
							new[] { typeof(SqlErrorCollection), typeof(string) },
							Array.Empty<ParameterModifier>())
						.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
				}
			}
		}

		class ZFilterForQueryHintFallback : ZFilterModuleWithCollectionViewWithFilterControlForTest
		{
			public int LoadCount { get; private set; }
			readonly string query;

			public ZFilterForQueryHintFallback(string query)
			{
				this.query = query;
			}

			protected override FilteredGridLoader CreateSearchManager()
				=> new GridLoader(this, FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

			class GridLoader : FilteredGridLoader
			{
				readonly ZFilterForQueryHintFallback parent;

				public GridLoader(ZFilterForQueryHintFallback parent, FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
					: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
				{
					this.parent = parent;
				}

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory")]
				protected override BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
				{
					using (var reader = ((IDbConnected)factory).Connection.Command(parent.query).ExecuteReader())
					{
						parent.LoadCount++;
						reader.Read();
					}
					return Array.Empty<BusinessObject>();
				}
			}
		}

		class ZFilterModuleWithCollectionViewWithFilterControlForTest : ZFilterModuleWithCollectionViewForTest
		{
			public Mock<IFilterControl> FilterControlMock { get; } = new Mock<IFilterControl>();

			protected override ResultCountMessage GetNewResultCountMessage()
			{
				return new ResultCountMessage(FilterControlMock.Object, MaxRowsToLoad, MaxRecommendedRowsToLoad);
			}
		}
	}

	[UseSnapshotProtection]
	class ZFilterModuleQueryCostTest : TestCase
	{
		public void TestRunLightQuery()
		{
			using (var filter = new ZFilterForCostedQuery("SELECT COUNT(*) FROM dbo.StmNums"))
			{
				filter.PerformSearch();
				AssertEquals("Read from first attempt", 1, filter.LoadCount);

				var message = UnitTestUserNotification.Instance.LastMessage;
				Assert("Warning message", message.WasNone);
			}
		}

		public void TestRunMediumQueryRejectedRun()
		{
			// Arrange
			SystemDataRegistry.Instance.FindWindowQueryGovernorCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FindWindowQueryCosts { AllowedCost = 1, MaximalCost = 3600 });
			using (var filter = new ZFilterForCostedQuery(MediumQuery))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				// Act
				filter.PerformSearch();

				// Assert
				AssertEquals("Rejected after first run", 1, filter.LoadCount);

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length); // 1st is expected and 2nd is {None}

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				Assert("Message text", message.Text.StartsWith(ExpectedLongQueryMessageWithSecurityAllowed) && message.Text.EndsWith("Are you sure you want to run it?"));
				filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.IsAny<ZString>(), It.IsAny<int>(), It.IsAny<bool>()), Times.AtLeastOnce);
				filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.Is<ZString>(s => s.Contains("Query execution has been canceled due to high estimated cost", StringComparison.OrdinalIgnoreCase)), 0, true), Times.Once);
			}
		}

		public void TestRunMediumQueryAcceptedRun()
		{
			SystemDataRegistry.Instance.FindWindowQueryGovernorCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FindWindowQueryCosts { AllowedCost = 1, MaximalCost = 3600 });
			using (var filter = new ZFilterForCostedQuery("SELECT a.SNS_Number FROM dbo.StmNumberSequence a CROSS JOIN dbo.StmNumberSequence b WHERE a.SNS_Number < 2"))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				filter.PerformSearch();

				AssertEquals("Executed two times", 2, filter.LoadCount);

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length); // 1st is expected and 2nd is {None}

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				Assert("Message text", message.Text.StartsWith(ExpectedLongQueryMessageWithSecurityAllowed) && message.Text.EndsWith("Are you sure you want to run it?"));
			}
		}

		public void TestRunMediumQueryRejectedRunWithSecurityAllowed()
		{
			// Arrange
			Env.Security.RunExtremelyLongFindQuery.IsAllowed = true;
			SystemDataRegistry.Instance.FindWindowQueryGovernorCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FindWindowQueryCosts { AllowedCost = 1, MaximalCost = 3600 });
			using (var filter = new ZFilterForCostedQuery(MediumQuery))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				// Act
				filter.PerformSearch();

				// Assert
				AssertEquals("Rejected after first run", 1, filter.LoadCount);

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length); // 1st is expected and 2nd is {None}

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				AssertStartsWith("Message text", ExpectedLongQueryMessageWithSecurityAllowed, message.Text);
				filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.IsAny<ZString>(), It.IsAny<int>(), It.IsAny<bool>()), Times.AtLeastOnce);
				filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.Is<ZString>(s => s.Contains("Query execution has been canceled due to high estimated cost", StringComparison.OrdinalIgnoreCase)), 0, true), Times.Once);
			}
		}

		public void TestRunMediumQueryAcceptedRunWithSecurityAllowed()
		{
			Env.Security.RunExtremelyLongFindQuery.IsAllowed = true;
			SystemDataRegistry.Instance.FindWindowQueryGovernorCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FindWindowQueryCosts { AllowedCost = 1, MaximalCost = 3600 });
			using (var filter = new ZFilterForCostedQuery("SELECT a.SNS_Number FROM dbo.StmNumberSequence a CROSS JOIN dbo.StmNumberSequence b WHERE a.SNS_Number < 2"))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				filter.PerformSearch();

				AssertEquals("Executed two times", 2, filter.LoadCount);

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length); // 1st is expected and 2nd is {None}

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				AssertStartsWith("Message text", ExpectedLongQueryMessageWithSecurityAllowed, message.Text);
			}
		}

		public void TestRunHeavyQueryOnActiveBusinessObjectCollection()
		{
			var factory = new BusinessObjectFactory();
			factory.New<DummyBusinessObject>();
			factory.Save();

			Env.Security.RunExtremelyLongFindQuery.IsAllowed = true;
			SystemDataRegistry.Instance.FindWindowQueryGovernorCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FindWindowQueryCosts { AllowedCost = 1, MaximalCost = 3600 });

			using (var filter = new ZFilterModuleWithActiveCollectionForTest())
			{
				var result = new ModuleSQLFilter("moo", typeof(DummyBusinessObject));
				result.Property1 = "LEN(Z0_Description) < (SELECT count(*) FROM dbo.GlbStaff CROSS JOIN dbo.StmNumberSequence)";
				result.IsActive = true;

				filter.FilterBusinessObject.ModuleFilters.AddCustomFilter(result);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				using (PersistentFactoryCacheManager.Instance.TrackFactoriesCreatedOnCurrentThread())
				{
					filter.PerformSearch();
					var newFactory = PersistentFactoryCacheManager.Instance.GetFactoriesCreatedInTrackedRegion().SingleOrDefault(f => f.NameForDebugging == "Module : Dummy (new factory for query with high estimated cost)");
					AssertNull("We should not proceed to perfroming search and creating a new factory as the user answered 'no'", newFactory);
					AssertEquals("Should be no search results yet", 0, filter.GridCollection.Count);
					AssertEquals("We should not proceed to perfroming search and updateing grid collection due to the 'no'", 0, filter.GridCollection.Factory.DatabaseLoadCount);
				}

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				filter.PerformSearch();
				// db hits should have happened from PerformSearch
				AssertEquals(1, filter.GridCollection.Factory.DatabaseLoadCount);
				// so this line doesn't make any other db hit...
				AssertEquals(1, filter.GridCollection.Count);
				AssertEquals("Db should have already been loaded, but GridCollection.Count forced the db hit here", 1, filter.GridCollection.Factory.DatabaseLoadCount);
			}
		}

		public void TestRunHeavyQuery()
		{
			// Arrange
			using (var filter = new ZFilterForCostedQuery("SELECT * FROM dbo.StmNumberSequence A CROSS JOIN dbo.StmNumberSequence B"))
			{
				// Act
				filter.PerformSearch();

				// Assert
				AssertEquals("Fail in first run", 1, filter.LoadCount);
				Assert("Warning message", UnitTestUserNotification.Instance.LastMessage.WasNone);
				filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.IsAny<ZString>(), It.IsAny<int>(), It.IsAny<bool>()), Times.AtLeastOnce);
				filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.Is<ZString>(s => s.Contains("To run this query you are required to have 'Specialized Rights -> Allow to Run Query with Unlimited Estimated Cost on Find Screens' security right.")), 0, true), Times.Once);
			}
		}

		public void TestRunRunHeavyRejectedRunWithSecurityAllowed()
		{
			Env.Security.RunExtremelyLongFindQuery.IsAllowed = true;
			using (var filter = new ZFilterForCostedQuery("SELECT SG_SN, SG_Value FROM dbo.StmNumberCache CROSS JOIN dbo.StmNumberSequence"))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				filter.PerformSearch();

				AssertEquals("Rejected after first run", 1, filter.LoadCount);

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length); // 1st is expected and 2nd is {None}

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				AssertStartsWith("Message text", ExpectedLongQueryMessageWithSecurityAllowed, message.Text);
			}
		}

		[SnailTest]
		public void TestRunRunHeavyAcceptedRunWithSecurityAllowed()
		{
			Env.Security.RunExtremelyLongFindQuery.IsAllowed = true;
			using (var filter = new ZFilterForCostedQuery("SELECT a.SNS_Number FROM dbo.StmNumberSequence a CROSS JOIN dbo.StmNumberSequence b WHERE a.SNS_Number < 2"))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				filter.PerformSearch();

				AssertEquals("Executed two times", 2, filter.LoadCount);

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length); // 1st is expected and 2nd is {None}

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				AssertStartsWith("Message text", ExpectedLongQueryMessageWithSecurityAllowed, message.Text);
			}
		}

		public void TestRunQueryWithWrongExceptionMessage()
		{
			CombineAssertions(() =>
			{
				Test(null);
				Test("");
				Test("error message");
				Test("error (message)");
				Test("error (message");
				Test("error )message");
				Test(")error( message");
			});

			void Test(string errorMessage)
			{
				// Arrange
				using (var filter = new ZFilterForCostedQueryWrongException())
				{
					// Act
					filter.PerformSearch(errorMessage);

					// Assert
					Assert("Warning message", UnitTestUserNotification.Instance.LastMessage.WasNone);
					filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage(It.IsAny<ZString>(), It.IsAny<int>(), It.IsAny<bool>()), Times.AtLeastOnce);
					filter.FilterControlMock.Verify(control => control.UpdateNumberLoadedMessage("The estimated cost of your query can't be retrieved from SQL Server. You may not run it.", 0, true), Times.Once);
				}
			}
		}

		public void TestRunQueryWithWrongExceptionMessageWithSecurityAllowed()
		{
			Env.Security.RunExtremelyLongFindQuery.IsAllowed = true;
			using (var filter = new ZFilterForCostedQueryWrongException())
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				filter.PerformSearch(")error( message");

				var messages = UnitTestUserNotification.Instance.PreviousMessages;
				AssertEquals("Should be two messages", 2, messages.Length); // 1st is expected and 2nd is {None}

				var message = messages[0];
				Assert("Warning message", message.WasQuestion);
				AssertMultilineASCIIEquals("Message text", ExpectedNoEstimatedValueFromServerMessageWithSecurityAllowed, message.Text);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			SystemDataRegistry.Instance.FindWindowQueryGovernorCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new FindWindowQueryCosts { AllowedCost = 1, MaximalCost = 100 });
			Env.Security.RunExtremelyLongFindQuery.IsAllowed = false;
		}

		const string ExpectedNoEstimatedValueFromServerMessageWithSecurityAllowed = @"The estimated cost of your query can't be retrieved from SQL Server. 
Please review your filters and see if you can specify more accurately what you are looking for.
Are you sure you want to run it?";
		const string ExpectedLongQueryMessageWithSecurityAllowed = "The search criteria you have provided";

		const string MediumQuery = @"
SELECT SNS_Number FROM dbo.StmNumberSequence
union all
SELECT SNS_Number FROM dbo.StmNumberSequence
union all
SELECT SNS_Number FROM dbo.StmNumberSequence
union all
SELECT SNS_Number FROM dbo.StmNumberSequence
";

		class ZFilterForCostedQueryWrongException : ZFilterModuleWithCollectionViewWithFilterControlForTest
		{
			public void PerformSearch(string errorMessage)
			{
				exceptionErrorMessage = errorMessage;
				base.PerformSearch();
			}
			string exceptionErrorMessage;

			protected override FilteredGridLoader CreateSearchManager()
				=> new GridLoader(this, FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

			class GridLoader : FilteredGridLoader
			{
				readonly ZFilterForCostedQueryWrongException parent;

				public GridLoader(ZFilterForCostedQueryWrongException parent, FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
					: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
				{
					this.parent = parent;
				}

				protected override BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
				{
					throw NewSqlException(8649, parent.exceptionErrorMessage);
				}

				static T Construct<T>(params object[] p)
				{
					var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
					return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
				}

				static SqlException NewSqlException(int number, string errorMessage = "error message", string proc = "proc", string serverName = "server name")
				{
					var collection = Construct<SqlErrorCollection>();
					var error = Construct<SqlError>(number, (byte)2, (byte)3, serverName, errorMessage, proc, 100);

					typeof(SqlErrorCollection)
						.GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
						.Invoke(collection, new object[] { error });

					return typeof(SqlException)
						.GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
							null,
							CallingConventions.ExplicitThis,
							new[] { typeof(SqlErrorCollection), typeof(string) },
							Array.Empty<ParameterModifier>())
						.Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
				}
			}
		}

		class ZFilterForCostedQuery : ZFilterModuleWithCollectionViewWithFilterControlForTest
		{
			public int LoadCount { get; private set; }
			readonly string query;

			public ZFilterForCostedQuery(string query)
			{
				this.query = query;
			}

			protected override FilteredGridLoader CreateSearchManager()
				=> new GridLoader(this, FilterBusinessObject, ResultCountMessage, ModuleDecisionProvider, ID, GetNewFactory, GridCollection.TypeOfElements);

			class GridLoader : FilteredGridLoader
			{
				readonly ZFilterForCostedQuery parent;

				public GridLoader(ZFilterForCostedQuery parent, FilterStripBusinessObject filterBusinessObject, ResultCountMessage handler, IModuleDecisionProvider provider, ModuleIdentifier moduleId, Func<BusinessObjectFactory> createFactory, Type typeOfElements)
					: base(filterBusinessObject, handler, provider, moduleId, createFactory, typeOfElements)
				{
					this.parent = parent;
				}

				[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1107:UseBusinessObjectFactory")]
				protected override BusinessObject[] LoadCollectionCore(BusinessObjectFactory factory, Type type, ZQuery query)
				{
					using (var reader = ((IDbConnected)factory).Connection.Command(parent.query).ExecuteReader())
					{
						parent.LoadCount++;
						reader.Read();
					}
					return Array.Empty<BusinessObject>();
				}
			}
		}

		class ZFilterModuleWithCollectionViewWithFilterControlForTest : ZFilterModuleWithCollectionViewForTest
		{
			public Mock<IFilterControl> FilterControlMock { get; } = new Mock<IFilterControl>();

			protected override ResultCountMessage GetNewResultCountMessage()
			{
				return new ResultCountMessage(FilterControlMock.Object, MaxRowsToLoad, MaxRecommendedRowsToLoad);
			}
		}

		[TestedType(typeof(EmbeddedModulePopupWithNoButtonPanelAndNoModality))]
		public class EmbeddedModulePopupWithNoButtonPanelAndNoModalityBasherTest : GUI.Internal.Testing.EmbeddModulePopupBasherTest
		{
			protected override Form GetFormToBashCore() => new EmbeddedModulePopupWithNoButtonPanelAndNoModalityTest.EmbeddedModulePopupWithNoButtonPanelAndNoModalityForTest();
		}
	}
}
