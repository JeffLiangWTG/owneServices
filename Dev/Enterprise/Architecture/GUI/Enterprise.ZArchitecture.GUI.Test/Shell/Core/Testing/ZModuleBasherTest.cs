using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.EntityFramework.Testing.DataAccess;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.DataMapping;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Res = Enterprise.ZArchitecture.GUI.Res;

namespace Enterprise.ZArchitecture.Modules.Testing
{
	[TestsSubclassesOf(typeof(ZFilterModule), typeof(TestExcludeZFilterGridModulesAllHaveModuleBashersAttribute), ExcludeClientDlls = true)]
	public abstract class ZModuleBasherTest : ZEmbeddedModuleBasherTest
	{
		#region Null Business Objects Loaded in Correct Thread

		public void TestShowViewForm_WhenLoadingInCorrectThreadReturnsNull_ShouldShowErrorAndNotThrowException()
		{
			using (var module = GetModule())
			{
				if (ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View || !module.AllowView)
				{
					Assert(true);
				}
				else
				{
					AssertShowForm_WhenLoadingInCorrectThreadReturnsNull_ShouldShowErrorAndNotThrowException(module, bizo => module.ShowViewForm(bizo));
				}
			}
		}

		public void TestShowEditForm_WhenLoadingInCorrectThreadReturnsNull_ShouldShowErrorAndNotThrowException()
		{
			using (var module = GetModule())
			{
				if (ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Edit || !module.AllowEdit)
				{
					Assert(true);
				}
				else
				{
					AssertShowForm_WhenLoadingInCorrectThreadReturnsNull_ShouldShowErrorAndNotThrowException(module, bizo => module.ShowEditForm(bizo));
				}
			}
		}

		public void TestShowDeleteForm_WhenLoadingInCorrectThreadReturnsNull_ShouldShowErrorAndNotThrowException()
		{
			using (var module = GetModule())
			{
				if (ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete || !module.AllowDelete)
				{
					Assert(true);
				}
				else
				{
					AssertShowForm_WhenLoadingInCorrectThreadReturnsNull_ShouldShowErrorAndNotThrowException(module, bizo => module.ShowDeleteForm(bizo));
				}
			}
		}

		void AssertShowForm_WhenLoadingInCorrectThreadReturnsNull_ShouldShowErrorAndNotThrowException(ZFilterModule module, Action<BusinessObject> showFormAction)
		{
			var bizo = GetNewBusinessObjectForLoadingInCorrectThreadTests();

			module.OverrideCurrentThreadBusinessObjectLoader_ForTest(new CurrentThreadBusinessObjectLoaderThatAlwaysReturnsNull());

			AssertNoExceptionThrown("Trying to show a form for a deleted object should not throw exceptions. SAD!", () => showFormAction(bizo));
			AssertEquals("Unable to display the selected record. It may have been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		class CurrentThreadBusinessObjectLoaderThatAlwaysReturnsNull : CurrentThreadBusinessObjectLoader
		{
			protected override BusinessObject GetOnCurrentThreadCore(BusinessObject maybeUnsafeBizo, string sourceFilePath)
			{
				return null;
			}
		}

		protected virtual BusinessObject GetNewBusinessObjectForLoadingInCorrectThreadTests()
		{
			return Factory.NewWithValidTestData<DummyBusinessObject>();
		}

		protected virtual bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Edit => false;
		protected virtual bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_View => false;
		protected virtual bool ShouldExcludeFromShowFormForBizoOnCorrectThreadTest_Delete => false;

		#endregion

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestModuleWithNonPersistentBusinessObject_ExportIntoAndOpenExcel()
		{
			try
			{
				using (var module = GetModule(GetModuleID()))
				{
					if (module.HasExportMenuItems && typeof(NonPersistentBusinessObject).IsAssignableFrom(module.GetElementType()) && BusinessObjectFactory.HasTableName(module.GetElementType()))
					{
						module.DisplayGrid.ExportIntoAndOpenExcel();
					}
				}
			}
			catch (ModuleGuiNotSupportedException)
			{
				Assert("Module does not support GUI, nothing needs to be tested", true);
			}
		}

		[RequiresSTA]
		public virtual void TestActionsDataTransferHasSameElementsInTopMenuBarAndContextMenu()
		{
			try
			{
				using (var module = GetModule(GetModuleID()))
				{
					if (!module.IsExcludedFromUtcTestsEgNoGuiNeededOrNoFilterBusinessStripNeeded)
					{
						var grid = (ZDisplayGrid)module.DisplayGrid; // will call ContextMenu first, just like OpenZEmbeddedModule()

						var actionsToolBarButton = module.ToolBarButtons?.Where(x => x.Text == ZFilterGridModule.ActionsMenuItemText).FirstOrDefault();
						var dataTransferTopMenuBarMenuItem = actionsToolBarButton?.DropDownMenu.MenuItems.FindByText("Data Transfer");

						var actionsContextMenuButton = grid.ContextMenu.MenuItems.FindByText(ZFilterGridModule.ActionsMenuItemText);
						var dataTransferContextMenuItem = actionsContextMenuButton?.MenuItems.FindByText("Data Transfer");

						if (dataTransferTopMenuBarMenuItem != null || dataTransferContextMenuItem != null)
						{
							AssertNotNull("Actions Menu should have a Data Transfer option", dataTransferTopMenuBarMenuItem);
							AssertNotNull("Grid Context Menu should have a Data Transfer option", dataTransferContextMenuItem);

							AssertEquals("'Actions / Data Transfer' should have the same numbers of items in top menu bar and right-click menu.", dataTransferContextMenuItem.MenuItems.Count, dataTransferTopMenuBarMenuItem.MenuItems.Count);
							for (var i = 0; i < dataTransferTopMenuBarMenuItem.MenuItems.Count; i++)
							{
								AssertEquals(string.Format("Items in position {0} should have the same text", i), dataTransferContextMenuItem.MenuItems[i].Text, dataTransferTopMenuBarMenuItem.MenuItems[i].Text);
							}
						}
						else
						{
							Assert("If neither module has an 'Actions / Data Transfer' menu, nothing needs to be tested", true);
						}
					}
					else
					{
						Assert("Module is excluded from GUI testing", true);
					}
				}
			}
			catch (ModuleGuiNotSupportedException)
			{
				Assert("Module does not support GUI, nothing needs to be tested", true);
			}
		}

		[RequiresSTA]
		public void TestMenuCopyHyperlinksToClipboard()
		{
			var moduleId = GetModuleID();
			using (var module = GetModule(moduleId))
			{
				if (!module.HasActions)
				{
					Assert("GIVEN module has no actions THEN AllowCopyFilterGridHyperlinkToClipboard is never created i.e. BMComponentModule", true);
					return;
				}

				try
				{
					if (module.AllowCopyFilterGridHyperlinkToClipboard)
					{
						var isCopyHyperlinksToClipboardMenuExist = false;
						Menu.MenuItemCollection menuItems = null;

						if (module.EmbeddedControl is IFilterControl filterControl)
						{
							menuItems = filterControl.FilteredGrid.ContextMenu.MenuItems;
							isCopyHyperlinksToClipboardMenuExist = menuItems.FindByText(CopyFilterGridHyperlinkToClipboardMenuItem.CopyHyperlinksToClipboardText) != null;
						}

						if (!isCopyHyperlinksToClipboardMenuExist)
						{
							isCopyHyperlinksToClipboardMenuExist = module.ActionsMenuItem.MenuItems.FindByText(CopyFilterGridHyperlinkToClipboardMenuItem.CopyHyperlinksToClipboardText) != null;
						}

						Assert("GIVEN AllowCopyFilterGridHyperlinkToClipboard = True THEN menu should have 'Copy Hyperlinks to Clipboard'", isCopyHyperlinksToClipboardMenuExist);

						var bizo = GetBusinessObjectForHyperlinking(module);
						if (bizo != null)
						{
							// Remove these lines to see failures for WI00201912
							var itemsInWI00201912 = new[] { "AccApportionmentTemplate", "AccCollectionBatch", "AccPayableOrderHeader", "AccPaymentApproval", "AccPaymentApproval", "vw_OrgCollectionCall", "AccGlobalChargeCodeMap", "AccGlobalChargeCodeMap", "AccGLBudget", "NettingSystemPeriod", "AccComplianceDocumentHeader" };

							if (itemsInWI00201912.Contains(bizo.TableName))
							{
								AssertEquals("Looks like you're doing Work Item WI00201912, please delete this condition & array when you're done so this test can check all bizos safely", bizo.TableName, bizo.HumanReadableName);
							}
							else
							{
								AssertNotEquals($"Please override the HumanReadableName of {bizo.GetType().FullName} or one of its parent classes. This is used to generate hyperlinks for your module. \r\nIf you don't want hyperlinking, set AllowCopyFilterGridHyperlinkToClipboard to false in the module.", bizo.TableName, bizo.HumanReadableName);
							}
						}
					}
					else
					{
						AssertNull(
							"GIVEN AllowCopyFilterGridHyperlinkToClipboard = False THEN menu should not have 'Copy Hyperlinks to Clipboard'",
							module.FormActionMenu.FindByText(CopyFilterGridHyperlinkToClipboardMenuItem.CopyHyperlinksToClipboardText));
					}
				}
				catch (Exception ex) when (ex is ModuleGuiNotSupportedException || ex.InnerException is ModuleGuiNotSupportedException)
				{
					Assert("GIVEN module does not support GUI", true);
				}
			}
		}

		protected virtual BusinessObject GetBusinessObjectForHyperlinking(ZFilterGridModule module)
		{
			try
			{
				var bizoType = module.GetNewController(null)?.TypeOfTopLevelBusinessObject;

				if (bizoType != null && !bizoType.IsAbstract && !bizoType.IsSubclassOf(typeof(NonPersistentBusinessObject)))
				{
					return Factory.NewWithValidTestData(bizoType);
				}
			}
			catch // Controller might not exist, TypeOfTopLevelBizo could be not supported, factory might not be able to instantiate.
			{
			}

			return null;
		}

		#region TestControllersDefinedForAllCountriesModuleDefinedOn and associated overrides

		[SnailTest()]
		public virtual void TestControllersDefinedForAllCountriesModuleDefinedOn()
		{
			var countriesQuery = new ZQuery();
			if (!CountryCode.IsNullOrEmpty())
			{
				countriesQuery.AddToFilter(RefCountrySchema.RN_Code, CountryCode);
			}
			var countries = Factory.Load<IRefCountry>(countriesQuery);

			var staffPk = Env.CurrentUserPK;
			var departmentPk = Env.CurrentDepartmentPK;

			var businessObjectsToGetControllersFor = GetBusinessObjectsToGetControllersFor();

			for (var i = 0; i < countries.Length; ++i)
			{
				var country = countries[i];

				var factory = new BusinessObjectFactory() { RefreshEnabled = false };
				var branch = factory.Load<IGlbBranch>(Env.CurrentBranchPK);
				var branchBizo = (BusinessObject)branch;
				var companyBizo = (BusinessObject)branch.Company;

				using (branchBizo.GetValidationSuspender())
				using (companyBizo.GetValidationSuspender())
				{
					branch.SetCountryIncludingCompanyWithoutCreatingAccountingData(country.RN_Code);
					factory.Save();

					using (Env.SetTemporaryUserContext(new UserContext(staffPk, branch.PK.ToGuid(), departmentPk, null, true, factory)))
					using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
					{
						AssertModuleControllersWorkInCurrentCountry(module, country.RN_Code, businessObjectsToGetControllersFor);
					}
				}
			}
		}

		public void TestTableShouldHaveOnlyOneSystemColumn()
		{
			Type gridElementType = null;
			try
			{
				using (var module = ZFilterModule.GetZFilterModule(GetModuleID()))
				{
					gridElementType = module.GridCollection.TypeOfElements;
					var tableName = BusinessObjectFactory.GetTableNameFromType(gridElementType);
					if (!string.IsNullOrEmpty(tableName))
					{
						var tableSchema = EnterpriseSchema.GetTableSchema(tableName);
						if (tableSchema != null)
						{
							var isSystemColumnCount = 0;
							foreach (var column in tableSchema.All)
							{
								var columnNameWithOutPrefix = column.Name.Substring(column.Name.IndexOf("_", StringComparison.Ordinal) + 1);
								if (BizObjectForTest.SystemKindList.Contains(columnNameWithOutPrefix))
								{
									isSystemColumnCount++;
								}
							}
							AssertEquals("Table should have only one system column", true, isSystemColumnCount <= 1);
						}
					}
				}
			}
			catch (ModuleGuiNotSupportedException)
			{
				// Acceptable failure
			}
			catch (ZException ex)
			{
				if (!ex.Message.EndsWith(" does not have a Schema.TableName.") && (gridElementType != null && gridElementType.Name != "ILocation"))
				{
					throw;
				}
			}

			Assert("If module isn't used for gui, nothing needs to be tested", true);
		}

		public void TestIsSystemDefinedFilter()
		{
			Type gridElementType = null;
			try
			{
				using (var module = ZFilterModule.GetZFilterModule(GetModuleID()))
				{
					gridElementType = module.GridCollection.TypeOfElements;
					var tableName = BusinessObjectFactory.GetTableNameFromType(gridElementType);
					var filterStripBusinessObject = module.FilterBusinessObject;
					if (!string.IsNullOrEmpty(tableName))
					{
						var tableSchema = EnterpriseSchema.GetTableSchema(tableName);
						if (tableSchema != null)
						{
							var shouldGenerateIsSystemDefinedFilter = false;
							foreach (var column in tableSchema.All)
							{
								var columnNameWithOutPrefix = column.Name.Substring(column.Name.IndexOf("_", StringComparison.Ordinal) + 1);
								if (BizObjectForTest.SystemKindList.Contains(columnNameWithOutPrefix))
								{
									AssertEquals(column.Name, filterStripBusinessObject.SystemDefinedStatusFilterColumn.Name);
									shouldGenerateIsSystemDefinedFilter = true;
									break;
								}
							}
							if (shouldGenerateIsSystemDefinedFilter)
							{
								AssertIsSystemDefinedFilter(filterStripBusinessObject);
							}
						}
					}
				}
			}
			catch (ModuleGuiNotSupportedException)
			{
				// Acceptable failure
			}
			catch (ZException ex)
			{
				if (!ex.Message.EndsWith(" does not have a Schema.TableName.") && (gridElementType != null && gridElementType.Name != "ILocation"))
				{
					throw;
				}
			}

			Assert("If module isn't used for gui or table doesn't have a system column, nothing needs to be tested", true);
		}

		void AssertIsSystemDefinedFilter(FilterStripBusinessObject filterStripBusinessObject)
		{
			var systemFilter = (ModuleTextFilter)filterStripBusinessObject["Is System Defined"];
			var list = (CodeDescriptionPairList)systemFilter.List;

			AssertNotNull("Should have a filter called 'Is System Defined'", systemFilter);
			AssertEquals("The category should be Status and Flags", FilterCategories.StatusAndFlags, systemFilter.Category);
			AssertEquals("The property should be correct", GetIsSystemDefinedDefaultProperty(),
				systemFilter.DefaultProperty.ToString());
			AssertEquals("The description should be Is System Defined",
				(NoResString)"Is System Defined",
				systemFilter.MultilingualDescription);
			AssertEquals("The number of value should be 3", 3, list.Count);
			Assert("The code list should contaion 'All'", list.ContainsCode("All"));
			Assert("The code list should contaion 'System'", list.ContainsCode("System"));
			Assert("The code list should contaion 'Not System'", list.ContainsCode("Not System"));
		}

		public void TestOtherSystemFilterNotExist()
		{
			try
			{
				using (var module = ZFilterModule.GetZFilterModule(GetModuleID()))
				{
					var filterStrip = module.FilterBusinessObject;
					if (filterStrip != null)
					{
						CombineAssertions(delegate
						{
							foreach (var filter in filterStrip.ModuleFilters)
							{
								if (filter.Category == FilterCategories.StatusAndFlags)
								{
									if (filter.Description == "System" || filter.Description == "Is System" ||
											filter.Description == "Is System Generated" || filter.Description == "IsSystem" ||
											filter.Description == "System Status")
									{
										AssertEquals(
											$"The '{filter.Description}' filter has been automatically created and is called 'Is System Defined', you should not create again",
											"Is System Defined", filter.Description);
									}
								}
							}
						});
					}
				}
			}
			catch (ModuleGuiNotSupportedException)
			{
				// Acceptable failure
			}

			Assert("If module isn't used for gui or other SystemFilters do not exist, nothing need to be tested", true);
		}

		public void TestAllIsSystemDefinedFilterShouldBeCreatedByCorrectColumn()
		{
			try
			{
				using (var module = ZFilterModule.GetZFilterModule(GetModuleID()))
				{
					var filterStrip = module.FilterBusinessObject;
					if (filterStrip != null)
					{
						CombineAssertions(delegate
						{
							foreach (var filter in filterStrip.ModuleFilters)
							{
								if (filter.Category == FilterCategories.StatusAndFlags)
								{
									if (filter.Description == "Is System Defined")
									{
										var column = filter.FilterColumn;
										if (!string.IsNullOrEmpty(column?.Name))
										{
											var availableColumnsName = BizObjectForTest.SystemKindList;
											var filterColumnName = column.Name.Split('_')[1];
											AssertCollectionContains(
												@"You should not create 'Is System Defined' filter manually, the system will create it automatically add you should add the schemaColumn into the avilable columns",
												filterColumnName, availableColumnsName);
										}
									}
								}
							}
						});
					}
				}
			}
			catch (ModuleGuiNotSupportedException)
			{
				// Acceptable failure
			}

			Assert("If module isn't used for gui or IsSystemDefinedFilter do not exist, nothing needs to be tested", true);
		}

		FilterStripBusinessObjectForTest BizObjectForTest => filterStripBusinessObject ?? (filterStripBusinessObject = new FilterStripBusinessObjectForTest());

		FilterStripBusinessObjectForTest filterStripBusinessObject;

		#region FilterStripBusinessObjectForTest

		public class FilterStripBusinessObjectForTest : FilterStripBusinessObject
		{
			protected override ModuleFilterCollection GetModuleFiltersCore()
			{
				throw new NotImplementedException();
			}
		}

		#endregion

		protected void AssertModuleControllersWorkInCurrentCountry(ZModule module, string nextCountryCode, BusinessObject[] businessObjectsToGetControllersFor)
		{
			var filterModule = module as ZFilterModule;
			if (filterModule != null)
			{
				if (HasController())
				{
					if (HasDefaultController())
					{
						var controller = filterModule.GetNewController(null);
						AssertNotNull("Module " + GetModuleID() + " works for country " + nextCountryCode + " but its controller does not", controller);
					}
					foreach (var bO in businessObjectsToGetControllersFor)
					{
						var controller = filterModule.GetNewController(bO);
						AssertNotNull("Module " + GetModuleID() + " works for country " + nextCountryCode + " but its controller does not for business object type " + bO.GetType().FullName, controller);
					}
				}
				else
				{
					AssertNull("Module " + GetModuleID() + " it currently not set to have a controller but one was found", filterModule.GetNewController(null));
				}
			}
		}

		/// <summary>
		/// Get an array of BusinessObjects that should be passed into GetNewController for the test.
		/// </summary>
		protected virtual BusinessObject[] GetBusinessObjectsToGetControllersFor()
		{
			return Array.Empty<BusinessObject>();
		}

		/// <summary>
		/// Get whether null can be passed into GetNewController().
		/// </summary>
		protected virtual bool HasDefaultController()
		{
			return true;
		}

		/// <summary>
		/// Gets whether the Module has a ZForm controller associated with it.
		/// </summary>
		protected virtual bool HasController()
		{
			return true;
		}

		#endregion

		[SnailTest()]
		[ExpectNoExceptions()]
		[RequiresSTA]
		public virtual void TestModuleShowsAndCanSearch()
		{
			CheckForMemoryLeaks(new TestMethod(CheckModuleShowsAndCanSearch));
		}

		public void TestOperationalActionCanExploreAllCollections()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleID))
			{
				if (module.GetType().GetInterface("IOperationalActionSupportable") != null)
				{
					var bizOType = module.GridCollection.TypeOfElements;
					AssertCurrentTypeIsExplorable(bizOType, "", 1);
				}
			}
			Assert(true); //If we find no collection properties or we don't implement operational actions, nothing to test
		}

		void AssertCurrentTypeIsExplorable(Type type, string parentProperty, int depth)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			foreach (var info in properties.Where(x => ActionFieldFollowAttribute.ShouldFollow(x)))
			{
				var reflectType = ActionFieldFollowAttribute.GetReturnType(info);

				if (reflectType != null)
				{
					if (typeof(IBusinessObjectCollection).IsAssignableFrom(reflectType))
					{
						if (reflectType.IsInterface)
						{
							var indexer = reflectType.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly, null, null, new[] { typeof(int) }, null);
							Assert(reflectType.FullName + " - Indexer should always exist because IBusinessObjectCollection implements IList", indexer != null);
							Assert($"{reflectType.FullName} - Collection interface type {reflectType.Name}'s Indexer returns {indexer?.PropertyType.Name}, which is a subclass of IBusiness (type: {type.Name}, parentProperty: {parentProperty}, info: {info.Name})",
								typeof(IBusiness).IsAssignableFrom(indexer?.PropertyType)
							);
						}

						if (depth < 2)
						{
							if (info.CanRead && ActionFieldFollowAttribute.ShouldFollow(info)) //copied from ReflectionHelper.cs Classify
							{
								AssertCurrentTypeIsExplorable(reflectType, info.Name, depth + 1);
							}
						}
					}
					else if (typeof(IBusiness).IsAssignableFrom(reflectType))
					{
						if (depth < 2)
						{
							if (info.CanRead && ActionFieldFollowAttribute.ShouldFollow(info))
							{
								AssertCurrentTypeIsExplorable(reflectType, info.Name, depth + 1);
							}
						}
					}
				}
			}
		}

		public virtual void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				try
				{
					var gridElementType = module.GridCollection.TypeOfElements;
					if (!gridElementType.IsSubclassOf(typeof(NonPersistentBusinessObject)) &&
							module.ModuleDecisionProvider.AllowExcelExport)
					{
						var objectName = BusinessObjectFactory.GetTableNameFromType(gridElementType);
						SchemaGuidColumn pKColumn = ObjectFactory.Get<IApplicationSchemaResolver>().GetPkColumn(objectName);
						var isIndexed = new DbIndexReader(objectName).IsIndexed(pKColumn);
						if (isIndexed)
						{
							Assert("Table is indexed OK", true);
						}
						else
						{
							var isObjectReallyAViewComprisingAnySynonyms = IsObjectReallyAViewComprisingAtLeastOneSynonymSoIsNotIndexableBecauseItsNotSchemaBindable(objectName);
							AssertEquals("Table " + objectName + " requires an index on the PK otherwise exporting to Excel will suffer performance problems. It is not a view on a synonym so it not excluded from this requirement.",
											true, isObjectReallyAViewComprisingAnySynonyms);
						}
					}
					else
					{
						Assert("If excel export isn't supported on this module, no problem", true);
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
					Assert("If the module isn't used for gui, no excel export will occur", true);
				}
			}
		}

		bool IsObjectReallyAViewComprisingAtLeastOneSynonymSoIsNotIndexableBecauseItsNotSchemaBindable(string tableName)
		{
			var sql = string.Format(@"
											SELECT  count(*)
											FROM
												  sys.views v
												  INNER JOIN sys.sql_expression_dependencies dep ON dep.referencing_id = v.object_id
												  INNER JOIN sys.objects rfedobj ON rfedobj.object_id = dep.referenced_id
											WHERE  v.name = '{0}' and rfedobj.type_desc = 'SYNONYM' ", tableName);
			return (int)Db.Connection.ExecuteScalar(sql) > 0;
		}

		protected virtual ZFilterModule GetModule()
		{
			return (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID());
		}

		protected void CheckModuleShowsAndCanSearch()
		{
			using (var module = GetModule())
			{
				ZFilterModule.ReturnSingleRow = true;
				try
				{
					try
					{
						BashModule(module);
					}
					catch (ModuleGuiNotSupportedException)
					{
						// Acceptable failure
					}
				}
				finally
				{
					ZFilterModule.ReturnSingleRow = false;
				}
			}
		}

		void TryReallyHardToAddTestObjects(IBusinessObjectCollection collection, IFilterModuleInternalsForTesting module)
		{
			AddTestObjects(collection);

			if (collection.Count == 0)
			{
				if (module.FilterBusinessObject != null)
				{
					// Try and fake it
					module.FilterBusinessObject.ResetToDefaultValues();
					module.PerformSearch();
				}

				if (collection.Count == 0)
				{
					ForciblyAddTestObjects(collection);
				}
			}
		}

		/// <summary>
		/// Override this method if FillWithValidTestData does not populate your objects so they show in the module grid
		/// </summary>
		/// <param name="collection"></param>
		protected virtual void AddTestObjects(IBusinessObjectCollection collection)
		{
		}

		void ForciblyAddTestObjects(IBusinessObjectCollection collection)
		{
			var legacyCollection = collection as BusinessObjectCollection;
			var activeCollection = collection as IActiveBusinessObjectCollection;

			BusinessObject bizO;
			if (collection is INonPersistentBusinessObjectCollection nonPersistentCollection)
			{
				bizO = ((ILegacyBusinessObjectCollectionInternals)nonPersistentCollection).CreateNewBusinessObject();
				bizO.FillWithValidTestData();
			}
			else
			{
				bizO = collection.Factory.NewWithValidTestData(collection.TypeOfElements, TestBusinessObjectKind.MinimumRequiredToSave);
			}

			if (legacyCollection != null)
			{
				collection.Add(bizO);
			}

			if (activeCollection != null && !activeCollection.Contains(bizO))
			{
				activeCollection.Relationship = new AdhocCollectionRelationship(activeCollection.TypeOfElements);
				activeCollection.Add(bizO);
			}
		}

		class ZFormWithIMainForm : ZForm, IMainForm
		{
			#region IMainForm Members

			public ZString CaptionSuffix
			{
				get { return ""; }
				set { }
			}

			public void UpdateToolBarDeleteButton(ZEmbeddedModule embeddedModule) { }

			public INamedModule CurrentModule { get { return null; } }

			public string CurrentModuleLicenceCheckPointName { get { return string.Empty; } }

			#endregion
		}

		Control GetFilterDescriptionDropEdit(Control control)
		{
			foreach (Control innerControl in control.Controls)
			{
				Control result = null;
				if (innerControl.Name == "FilterDescriptionDropEdit")
				{
					result = innerControl;
				}
				else
				{
					result = GetFilterDescriptionDropEdit(innerControl);
				}
				if (result != null)
				{
					return result;
				}
			}
			return null;
		}

		void BashDropEdit(ZDropEdit filterDescriptionDropEdit)
		{
			if (filterDescriptionDropEdit == null)
			{
				throw new Exception("Could not find filterDescriptionDropEdit");
			}
			foreach (CodeDescriptionPair pair in filterDescriptionDropEdit.List)
			{
				SwitchToFilter(filterDescriptionDropEdit, pair.Description);
			}

			SwitchToFilter(filterDescriptionDropEdit, string.Empty); // Clear off that last filter
		}

		static void SwitchToFilter(ZDropEdit filterDescriptionDropEdit, string description)
		{
			filterDescriptionDropEdit.Focus();
			Application.DoEvents();
			filterDescriptionDropEdit.Text = description;
			filterDescriptionDropEdit.CommitBoundValue();
			TestKeyStrokeHelper.SendKeyToControl(filterDescriptionDropEdit, Keys.Tab);
			Application.DoEvents();
		}

		protected virtual void BashModule(ZFilterModule module1)
		{
			AssertNotNull("Module not found - are you running with the right country?", module1);

			using (ZForm form = new ZFormWithIMainForm())
			{
				module1.EmbeddedControl.Dock = DockStyle.Fill;

				form.Size = new System.Drawing.Size(1366, 768);
				form.Controls.Add(module1.EmbeddedControl);
				form.Show();

				BashDropEdit((ZDropEdit)GetFilterDescriptionDropEdit(module1.EmbeddedControl));

				CheckBindedBusinessObjectCollectionThrowsNoExceptionForCodeFromDescription(module1);

				var collection = ((IFilterModuleInternalsForTesting)module1).GridCollection;
				var module = (IFilterModuleInternalsForTesting)module1;

				TryReallyHardToAddTestObjects(collection, module);
				Assert("GridCollection does not have any objects.\r\nOverride AddTestObjects in your module test to add valid objects", collection.Count > 0);

				var filterGridModule = module as IFilterGridModuleInternalsForTesting;
				if (filterGridModule != null)
				{
					filterGridModule.Grid.ExposeAllColumns();
					Assert("No rows were found in the grid", filterGridModule.Grid.VisibleRowCount >= 0);

					var horizontalScrollPosition = 0;
					filterGridModule.Grid.HorizontalScrollToOffset(0);
					while (horizontalScrollPosition < filterGridModule.Grid.HorizontalScrollBarMaximum)
					{
						var offset = Math.Min(filterGridModule.Grid.HorizontalScrollBarMaximum - horizontalScrollPosition, filterGridModule.Grid.ClientRectangle.Width);
						horizontalScrollPosition += offset;
						filterGridModule.Grid.HorizontalScrollToOffset(horizontalScrollPosition);
						System.Windows.Forms.Application.DoEvents();
					}
				}

				var basher = new ZFormBasher(form);
				try
				{
					basher.BashForm();
				}
				finally
				{
					AddFailures(basher.GetFailureMessages());
				}
			}
		}

		[StressTest]
		[RequiresSTA]
		public virtual void TestAllGridColumnsCanBeExportedToExcel()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()) as ZFilterModule)
			{
				try
				{
					if (module.ModuleDecisionProvider.AllowExcelExport && module.FilterBusinessObject != null)
					{
						var filterStripControl = module.EmbeddedControl as IFilterControl;
						ZGrid grid = filterStripControl.FilteredGrid;

						foreach (ZGridColumnInfo info in grid.ColumnStyles)
						{
							info.IsVisible = true;
						}

						((IFilterStripCommonControlInternalsForTesting)filterStripControl).Bind();

						grid.RefreshTableStyles();

						foreach (var column in grid.Columns)
						{
							if (!(column.ColumnStyle.PropertyDescriptor.PropertyType.IsInterface && column.ColumnStyle.PropertyDescriptor.PropertyType == typeof(IZType)))
							{
								var zInterface = column.ColumnStyle.PropertyDescriptor.PropertyType.GetInterface(nameof(IZType));
								AssertNotNull(string.Format("The property '{0}' of data source bound to ZGrid should implement IZType interface for exporting to excel", column.ColumnName), zInterface);
							}
						}
					}
					else
					{
						Assert("If excel export isn't supported on this module, no problem", true);
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
					Assert("If the module isn't used for gui, no excel export will occur", true);
				}
			}
		}

		public void CheckBindedBusinessObjectCollectionThrowsNoExceptionForCodeFromDescription(ZFilterModule module)
		{
			if (module.HasImportMenuItems)
			{
				var importCollectionInfoProvider = module as IImportCollectionInfoProvider;
				if (importCollectionInfoProvider != null)
				{
					BusinessObject bo;
					if (importCollectionInfoProvider.ImportCollectionInfo.Collection is IActiveBusinessObjectCollection)
					{
						bo = ((IBindingList)importCollectionInfoProvider.ImportCollectionInfo.Collection).AddNew() as BusinessObject;
					}
					else
					{
						bo = importCollectionInfoProvider.ImportCollectionInfo.Collection.AddNew();
					}

					foreach (var propertyInfo in importCollectionInfoProvider.ImportCollectionInfo.Properties)
					{
						var provider = propertyInfo.GetFindBoxListProvider(bo) as IFindBoxListProviderDescriptionEx;
						if (provider != null)
						{
							AssertNoExceptionThrown(delegate
							{ provider.CodeFromDescription("description"); });
						}
					}
					//TODO: iterate test collection and child collections
				}
			}
		}

		public void TestRunImportDataWizard_DoesNotUseActiveHeaderCollectionWithoutAdHocRelationship()
		{
			// See WI00220544
			// Unfortunately this code is very poorly designed for testability, reflection was most appropriate way to broadly test it for a defect fix where refactoring is not practical.
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var importInfoProvider = module as IImportCollectionInfoProvider;
				var filterGridModule = module as ZFilterGridModule;
				if (importInfoProvider != null && (filterGridModule?.HasImportMenuItems ?? false))
				{
					var processor = filterGridModule.GetType().GetMethod("GetDataImportWizardProcessor", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(filterGridModule, new[] { importInfoProvider.ImportCollectionInfo });
					var headerCollection = processor != null ? processor.GetType().GetField("headerCollection", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(processor) as IBusinessObjectCollection : null;

					if (headerCollection != null)
					{
						if (headerCollection is IActiveBusinessObjectCollection activeHeaderCollection)
						{
							AssertType<AdhocCollectionRelationship>("Should use adhoc collection relationship for an ActiveBusinessObjectCollection.", activeHeaderCollection.Relationship);
						}
					}
				}
			}

			Assert(true); // Will fail if necessary above, should pass otherwise
		}

		public void TestTranslatableTextFiltersAreOnTranslatableDataFields()
		{
			using (Res.TemporarilySwitchLanguage(Enterprise.Core.SharedConstants.Languages.ChineseSimplified))
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()) as ZFilterModule)
			{
				try
				{
					if (module.FilterBusinessObject != null)
					{
						var elementType = module.GridCollection.TypeOfElements;
						foreach (var filter in module.FilterBusinessObject.ModuleFilters)
						{
							if (filter is ModuleTextFilter && filter.FilterColumn != null)
							{
								var property = elementType.GetProperty(filter.FilterColumn.Name);
								if (property != null)
								{
									var attribute = Attribute.GetCustomAttribute(property, typeof(TranslatableDataFieldAttribute), true) ?? Attribute.GetCustomAttribute(property, typeof(LinkedTranslatableDataFieldAttribute), true);
									if (attribute != null && !filter.Description.EndsWith("_Local"))
									{
										var translatableFilter = module.FilterBusinessObject.ModuleFilters[filter.Description + "_Local"];
										AssertType("There should be a translatable text filter for " + filter.FilterColumn.Name, typeof(ModuleTranslatableTextFilter), translatableFilter);
									}
								}
							}
						}
					}
				}
				catch (ModuleGuiNotSupportedException)
				{ }
			}
			Assert(true);
		}

		public void TestModulesWhichSupportWorkflowShouldSpecifyWorkflowTypes()
		{
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				if (module != null && module is ZFilterGridModule filterModule)
				{
					try
					{
						var elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(filterModule.GridCollection.GetType());
						var implementsIWorkflowProvider = typeof(IWorkflowProviderCore).IsAssignableFrom(elementType) || elementType.IsAssignableFrom((typeof(IWorkflowProviderCore)));

						if (filterModule.SupportsWorkflow && implementsIWorkflowProvider)
						{
							var bizo = CreateValidBOForTestSpecifyWorkflowType(elementType);
							var bizoWorkflowType = ((IWorkflowProviderCore)bizo).WorkflowType;
							AssertNotNull("Please override WorkflowTypes and specify which WorkflowDescriptor codes are supported by this module. If none are supported, then return string.Empty.", filterModule.WorkflowType);
							AssertEquals(bizoWorkflowType, filterModule.WorkflowType);
						}
					}
					catch (ModuleGuiNotSupportedException)
					{
					}
					catch (NoConcreteTypeException)
					{
					}
					catch (ZException)
					{
					}
					catch (ApplicationException)
					{
					}
					catch (NotSupportedException)
					{
					}
					catch (SqlException)
					{
					}
				}
			}
			Assert(true);
		}

		protected virtual BusinessObject CreateValidBOForTestSpecifyWorkflowType(Type elementType)
		{
			return Factory.NewWithValidTestData(elementType);
		}

		public void TestControllerUsedInModuleHasModuleID()
		{
			if (HasController())
			{
				using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
				{
					if (module is ZFilterGridModule)
					{
						var gridFilterModule = module as ZFilterGridModule;
						if (HasDefaultController())
						{
							var showRecentItems = gridFilterModule.ShowRecentItems;
							if (!showRecentItems && gridFilterModule.ShowRecentItemsCoreNotOverriden)
							{
								var controller = gridFilterModule.GetNewController(null);
								AssertNotNull(string.Format("Controller for module {0} should not be null", module.ID.Name), controller);
								AssertNotNull(string.Format("ModuleID is null for Controller {0}", controller.ID.Name), controller.ModuleID);
							}
							else
							{
								Assert("Module has overriden ShowRecentItemsCore method", true);
							}
						}
						else
						{
							AssertNull(string.Format("Module {0} should not have a default controller", module.ID.Name), (module as ZFilterModule).GetNewController(null));
						}
					}
					else
					{
						Assert("Module is not a filter grid", true);
					}
				}
			}
			else
			{
				Assert(string.Format("Module {0} doesn't have a controller", this.ModuleID.Name), true);
			}
		}

		[StressTest] // Some modules load a lot of bizos by default
		[RequiresSTA]
		public void TestDateFilters_ShouldBeUtcIfThePropertyIsUtc()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var costs = new FindWindowQueryCosts { AllowedCost = 0, MaximalCost = 0 };
			costs.AllowedCostInfo.ClearAllNotifications();
			costs.MaximalCostInfo.ClearAllNotifications();
			SystemDataRegistry.Instance.FindWindowQueryGovernorCosts.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, costs);

			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				if (module.IsExcludedFromUtcTestsEgNoGuiNeededOrNoFilterBusinessStripNeeded)
				{
					Assert("Not every module needs a FilterBusinessObject or a GUI, yo.", true);
				}
				else
				{
					var filterBizo = (FilterStripBusinessObject)((IFilterModuleInternalsForTesting)module).FilterBusinessObject;
					var moduleInternals = module as IFilterGridModuleInternalsForTesting;
					var collection = moduleInternals != null ? ((IFilterGridModuleInternalsForTesting)module).GridCollection : null;

					if (collection != null && collection.Count == 0)
					{
						TryReallyHardToAddTestObjects(collection, moduleInternals);
					}

					var bizo = collection != null ? collection.OfType<BusinessObject>().FirstOrDefault() : null;
					var dateFilters = filterBizo.ModuleFilters.OfType<ModuleDateFilter>().ToArray();

					CombineAssertions(string.Format("Filters defined in {0} which are based on UTC columns should have ConvertFromLocalToUTC set to true, otherwise filter options using the current time like 'In the past', 'In the future', and 'Hour Offset Range' won't work consistently.", filterBizo.GetType().Name), () =>
					{
						foreach (var filter in dateFilters)
						{
							if (filter.FilterColumn != null)
							{
								var message = new Lazy<string>(() => string.Format("Description: {0}, Column: {1}", filter.Description, filter.FilterColumn.Name));

								if (filter.FilterColumn.Name.Contains("UTC", StringComparison.InvariantCultureIgnoreCase) &&
									!filter.Description.Contains("UTC", StringComparison.InvariantCultureIgnoreCase))
								{
									EnsureFilterIsUtc(message.Value, filter);
								}
								else if (bizo != null)
								{
									try
									{
										bizo[filter.FilterColumn] = ZDateTime.UtcToday;
									}
									catch (ArgumentException)
									{
										// Module is probably a view that doesn't have a property for the same name as the schema column...
										continue;
									}

									var value = bizo[filter.FilterColumn];

									if (value is ZDateTime) // Might be ZDate, which doesn't have a Kind property.
									{
										if (((ZDateTime)value).Kind == DateTimeKind.Utc &&
											!filter.Description.Contains("UTC", StringComparison.InvariantCultureIgnoreCase))
										{
											EnsureFilterIsUtc(message.Value, filter);
										}
									}
								}
							}
						}
					});

					Assert("Don't use date/time filters? That's fine by me, I suppose.", true);
				}
			}
		}

		[RequiresSTA]
		public void TestFiltersMatchOperator_ShouldGenerateValidQueries()
		{
			SystemDataRegistry.Instance.MaxNumberOfRecordsToShowInDisplayGrids.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 1); // To ensure module.PerformSearch results are low, lest a bazillion business objects get created.
			using (var module = ZModuleFactory.Instance.Create(GetModuleID()))
			{
				if (module != null && module is ZFilterModule filterModule)
				{
					FilterStripBusinessObject filterBizo = null;

					try
					{
						filterBizo = filterModule.FilterBusinessObject;
					}
					catch (ModuleGuiNotSupportedException)
					{
					}

					if (filterBizo != null)
					{
						var filters = filterBizo.ModuleFilters.OfType<IModuleFilterWithSelectedFilters>().Where(x => x.AllowedComparisonOperators.Contains(ModuleTextFilter.ComparisonConstants.FiltersMatch));

						CombineAssertions(() =>
						{
							foreach (var filter in filters)
							{
								var failureInfo = string.Format(CultureInfo.InvariantCulture, "Filter: [{0}]. Schema Column: [{1}]. FilterBusinessObject type: [{2}]", filter.Description, (filter.FilterColumn?.Name ?? "None Specified"), filterBizo.GetType().Name);

								filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
								ZQuery query = null;
								var moduleSpecifiedFilter = filter as ModuleGuidModuleSpecifiedFilter;

								if (moduleSpecifiedFilter != null)
								{
									moduleSpecifiedFilter.SetModuleId_ForTest(ModuleIDs.JobShipment); // Otherwise the query will be empty.
									AssertEquals("The text entered should have selected a valid module option. You may need to override SetModuleId_ForTestCore on your filter. SAD!", ModuleIDs.JobShipment, moduleSpecifiedFilter.ModuleId);
								}

								var failureMessage = string.Format(CultureInfo.InvariantCulture, "When generating the query for this filter, the 'filters match' option has caused an exception for the {0}.", failureInfo);
								AssertNoExceptionThrown(failureMessage, () => query = filter.Query);

								var queryText = query.LiteralTextADOFormatted;
								AssertEquals(string.Format(CultureInfo.InvariantCulture, "The query should not be empty, and yet... {0}.{1}{2}", failureInfo, System.Environment.NewLine, queryText), false, query.IsEmpty);
								AssertContains("The query should contain the keyword IN because there should always be a subquery, even with no selected filters. And yet..." + queryText, "IN", queryText);

								filter.IsActive = true;

								AssertNoExceptionThrown("Running this query threw an exception. Please fix it up, somehow, or disable filters match for this filter (by setting SupportsFiltersMatchComparisonOperator to false) if you really can't get it to work. " + failureInfo, () =>
								{
									using (Db.Connection.TrackExecutedCommands())
									{
										try
										{
											filterModule.PerformSearch();
										}
										catch (ModuleGuiNotSupportedException)
										{
											var fullFilter = filterBizo.Filter;
											fullFilter.AddFilterAndZSQLParameterCollection("1=2", new ZSqlParameterCollection());

											try
											{
												Factory.Load(filterBizo.QueryObjectType, fullFilter);
											}
											catch (Exception)
											{
												Fail("Executed Commands: " + System.Environment.NewLine + string.Join(System.Environment.NewLine, Db.Connection.ExecutedCommands));
												throw;
											}
										}
										catch (Exception)
										{
											Fail("Executed Commands: " + System.Environment.NewLine + string.Join(System.Environment.NewLine, Db.Connection.ExecutedCommands));
											throw;
										}
									}
								});

								using (var subModule = ZFilterModule.GetZFilterModule(filter.ModuleId))
								{
									var subModuleAdditionalQuery = subModule.GridCollection.CompleteFilter;

									if (!subModuleAdditionalQuery.IsEmpty)
									{
										var subModuleQueryText = subModuleAdditionalQuery.LiteralTextADO;
										var selectedFiltersQueryText = filter.GetSubFilterQueryIncludingCollectionFilters().LiteralTextADO;
										AssertContains("The filter's query should contain any additional query parts from the module, and yet... " + failureInfo, subModuleQueryText, selectedFiltersQueryText);

										var fullFilterQueryText = filter.Query.LiteralTextADO;
										AssertContains("The filter's query should contain any additional query parts from the module, and yet... " + failureInfo, subModuleQueryText, fullFilterQueryText);
									}
								}

								filter.IsActive = false;
								AssertEquals(failureInfo + "Query: " + filterModule.ExportQuery.LiteralTextADOFormatted, string.Empty, ErrorReporter.LastMessageReported);
								ErrorReporter.Clear();
							}
						});
					}
				}
			}

			Assert(true);
		}

		protected override void AssertUserDefinedFilters_ShouldGenerateValidQueries()
		{
			var moduleId = GetModuleID();

			using (var module = ZFilterModule.GetZFilterModule(moduleId))
			{
				PrepareModuleForUserDefinedFilterTest(module);
				var filterBizo = module.FilterBusinessObject;

				if (filterBizo == null || !filterBizo.SupportsUserDefinedFilters)
				{
					Assert(true);
					return;
				}

				SaveUserDefinedFilter(filterBizo);
			}

			using (var module = ZFilterModule.GetZFilterModule(moduleId))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddFilterStrip<ModuleUserDefinedFilter>("[USR]User-Defined");
				ZQuery query = null;

				AssertNoExceptionThrown("Generating and running the query should not cause exceptions, and yet...", () =>
				{
					query = filterBizo.Filter;
					Factory.Load(filterBizo.QueryObjectType, query);
				});

				AssertEquals(query.LiteralTextSqlFormatted, true, query.LiteralTextSqlFormatted.Contains("1=2"));
			}
		}

		protected virtual void SaveUserDefinedFilter(FilterStripBusinessObject filterBizo)
		{
			GetFilterForUserDefinedFilterTest(filterBizo);
			FilterStripsTestHelper.SaveFilterLayout(filterBizo, "User-Defined", true, false, isUserDefinedFilter: true);
		}

		protected virtual void PrepareModuleForUserDefinedFilterTest(ZFilterModule module)
		{
		}

		protected virtual Func<FilterStripBusinessObject, ModuleFilter> GetFilterForUserDefinedFilterTest => filterBizo =>
		{
			var filter = (ModuleSQLFilter)filterBizo.FilterStrips.AddNew("Custom SQL Filter").CurrentModuleFilter;
			filter.Property1 = "1=2";

			return filter;
		};

		#region Filters Auto-Added from FilterStripsHelpers

		void RunFiltersAutoAddedFromHelpersTest(IFilterStripsHelper helper, string methodName, params string[] filtersThatMustBePresent)
		{
			var moduleId = GetModuleID();

			using (var module = GetModule(moduleId))
			{
				try
				{
					var filterBusinessObject = module.FilterBusinessObject;

					if (filterBusinessObject != null)
					{
						var elementType = BusinessObjectCollection.GetElementTypeFromCollectionType(module.GridCollection.GetType());
						helper.Initialise(elementType, filterBusinessObject.Factory);

						if (helper.CanAddFilters() && !filterBusinessObject.IsFilterStripsHelperExcluded(helper.GetType()))
						{
							foreach (var filter in filtersThatMustBePresent)
							{
								if (filterBusinessObject.GetShouldUseHelperFilter)
								{
									AssertNotNull(filter + " filter should have been automatically added by the helper, and yet...", filterBusinessObject[filter]);
								}
								else
								{
									AssertNull(filterBusinessObject[filter]);
								}
							}

							CustomiseFilterForFilterStripsHelperTests(filterBusinessObject);
							if (filterBusinessObject.GetShouldUseHelperFilter)
							{
								var testCase = ObjectFactory.Get<AutomaticFilterTest>(helper.GetAutomaticFilterTestCaseName_ForObjectFactory());
								testCase.SetUpForHelperFiltersWorkTests(elementType);
								testCase.AssertHelperFiltersWork(methodName, filterBusinessObject, elementType, GetNewBusinessObjectForHelperFilterTests);
							}
						}
					}
				}
				catch (TargetInvocationException ex)
				{
					if (ex.InnerException == null || ex.InnerException.GetType() != typeof(ZException) || !ex.InnerException.Message.Contains("does not have a Schema.PK"))
					{
						var message = string.Format(autoFilterTestFailureMessageFormat, methodName, moduleId.ID, helper.GetType().Name);
						throw new AssertionFailedError(message, ex);
					}
				}
				catch (ModuleGuiNotSupportedException)
				{
				}
			}
			Assert(true);
		}

		const string autoFilterTestFailureMessageFormat = "The {0} test for module [{1}] to test filters automatically added by a {2} failed to pass. If special setup is needed for new business objects to match filters, please override ZModuleBasherTest.GetNewBusinessObjectForHelperFilterTests. If your module requires special filters to be added, you may need to create your own subclass of {2} and include it when overriding GetCustomFilterStripsHelpersCore on your FilterStripBusinessObject.";

		IFilterStripsHelper WorkflowFilterStripsHelper
		{
			get { return workflowFilterStripsHelper ?? (workflowFilterStripsHelper = (IFilterStripsHelper)ObjectFactory.Get("IWorkflowFilterStripsHelper")); }
		}
		IFilterStripsHelper workflowFilterStripsHelper;

		IFilterStripsHelper BMFilterStripsHelper
		{
			get { return bMFilterStripsHelper ?? (bMFilterStripsHelper = (IFilterStripsHelper)ObjectFactory.Get("IBMFilterStripsHelper")); }
		}
		IFilterStripsHelper bMFilterStripsHelper;

		static ZFilterGridModule GetModule(ModuleIdentifier moduleId)
		{
			return ZModuleFactory.Instance.Create(moduleId) as ZFilterGridModule;
		}

		protected virtual BusinessObject GetNewBusinessObjectForHelperFilterTests(BusinessObjectFactory factory, Type businessObjectType)
		{
			return factory.NewWithValidTestData(businessObjectType);
		}

		protected virtual void CustomiseFilterForFilterStripsHelperTests(FilterStripBusinessObject filterBusinessObject)
		{
		}

		public virtual void TestAutoAddedMilestoneDateFilter()
		{
			RunFiltersAutoAddedFromHelpersTest(WorkflowFilterStripsHelper, "AssertMilestoneDate", "Milestone Date");
		}

		public virtual void TestAutoAddedTaskStatusFilter()
		{
			RunFiltersAutoAddedFromHelpersTest(WorkflowFilterStripsHelper, "AssertTaskStatusFilter", "Milestone Date");
		}

		public void TestAutoAddedBufferManagementComponentFilter()
		{
			RunFiltersAutoAddedFromHelpersTest(BMFilterStripsHelper, "AssertBufferManagementComponent", "Milestone Date", "Buffer Management Component");
		}

		public void TestAutoAddedTagFilters()
		{
			RunFiltersAutoAddedFromHelpersTest(BMFilterStripsHelper, "AssertTagFilters", "Milestone Date", "Buffer Management Component");
		}

		public virtual void TestTasksFilter()
		{
			RunFiltersAutoAddedFromHelpersTest(WorkflowFilterStripsHelper, "AssertTasksFilter", "Tasks");
		}

		public virtual void TestExceptionsFilter()
		{
			RunFiltersAutoAddedFromHelpersTest(WorkflowFilterStripsHelper, "AssertExceptionsFilter", "Exceptions");
		}

		public virtual void TestMilestonesFilter()
		{
			RunFiltersAutoAddedFromHelpersTest(WorkflowFilterStripsHelper, "AssertMilestonesFilter", "Milestones");
		}

		public virtual void TestTriggersFilter()
		{
			RunFiltersAutoAddedFromHelpersTest(WorkflowFilterStripsHelper, "AssertTriggersFilter", "Triggers");
		}

		public void TestWorkflowsFilter()
		{
			RunFiltersAutoAddedFromHelpersTest(BMFilterStripsHelper, "AssertWorkflowsFilter", "Workflows for Job");
		}

		public void TestWorkflowCustomFieldsFilters_EnabledByDefault()
		{
			RunFiltersAutoAddedFromHelpersTest(WorkflowFilterStripsHelper, "AssertCustomFields");
		}

		#endregion

		void EnsureFilterIsUtc(string message, ModuleDateFilter filter)
		{
			if (!filter.ConvertFromLocalToUTC && !filter.UserEntersUtcValue)
			{
				if (!IsCurrentTestExcludedFromUtcFilterChecks)
				{
					AssertEquals(message, true, filter.ConvertFromLocalToUTC);
				}
			}
		}

		bool IsCurrentTestExcludedFromUtcFilterChecks
		{
			get
			{
				return SpecialExceptionWithDateFilters_ConvertFromLocalToUTC_Is_False.Any(a => CurrentTestName.Contains(a));
			}
		}

		static string[] SpecialExceptionWithDateFilters_ConvertFromLocalToUTC_Is_False
		{
			get
			{
				return new[]
				{
										"Enterprise.Freight.Agency.Module.Testing.ContainerMoveModuleBasherTest",
								};
			}
		}

		#region Assertion Methods

		protected void AssertImportByDataWizardRequiresImportToSystemPrivilege(SecurityCheckpoint moduleSecurityCheckPoint, SecurityCheckpoint securityCheckpointNeededForAddingObjects)
		{
			var staff = Factory.NewWithValidTestData(ObjectFactory.GetType<IGlbStaff>());
			Factory.Save();

			var importToSystemSecurityCheckPoint = Env.Security.FindOrCreateImportCheckPoint(moduleSecurityCheckPoint);

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var module = GetModule())
			{
				AssertEquals("Precondition", false, importToSystemSecurityCheckPoint.IsAllowed);
				AssertEquals("Precondition", false, securityCheckpointNeededForAddingObjects.IsAllowed);
				AssertErrorMessageWhenImportingByDataWizard($@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{importToSystemSecurityCheckPoint.DisplayTextPathToSecurityRight}", module);
			}

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var module = GetModule())
			{
				importToSystemSecurityCheckPoint.IsAllowed = true;
				AssertEquals("Precondition", false, securityCheckpointNeededForAddingObjects.IsAllowed);
				AssertErrorMessageWhenImportingByDataWizard($@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{securityCheckpointNeededForAddingObjects.DisplayTextPathToSecurityRight}", module);
			}

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var module = GetModule())
			{
				importToSystemSecurityCheckPoint.IsAllowed = true;
				securityCheckpointNeededForAddingObjects.IsAllowed = true;
				AssertErrorMessageWhenImportingByDataWizard(null, module);
			}
		}

		void AssertErrorMessageWhenImportingByDataWizard(string expectedMessage, ZFilterModule module)
		{
			var formActions = module.FormActionMenu; // will load menu items
			var importMenuItems = ((IFilterModuleInternalsForTesting)module).ImportMenuItems;
			var importByDataWizardMenuItem = importMenuItems.SingleOrDefault(x => x.Text == "&Import By Data Wizard");
			AssertNotNull(importByDataWizardMenuItem);

			importByDataWizardMenuItem.OnClick(null, EventArgs.Empty);

			using (ZApplication.GetOpenForms().OfType<MultistepDataImportWizardForm>().SingleOrDefault())
			{
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
			UnitTestUserNotification.Instance.ClearMessages();
		}

		#endregion
	}

	[TestedType(typeof(DummyModule))]
	[DoNotAddToTestTree]
	[TestExcludeZFilterGridModulesAllHaveModuleBashers]
	public sealed class ZModuleBasher : ZModuleBasherTest
	{
		public ZModuleBasher(ModuleIdentifier moduleID)
		{
			this.moduleID = moduleID;
		}

		readonly ModuleIdentifier moduleID;

		protected override ModuleIdentifier GetModuleID()
		{
			return moduleID;
		}

		public void TestWithoutMemoryChecking()
		{
			CheckModuleShowsAndCanSearch();
		}
	}

	[TestedType(typeof(DummyFilterGridModule))]
	internal class TestZModuleBasherTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return DummyModuleIDs.Dummy;
		}

		public override void TestAllGridColumnsCanBeExportedToExcel()
		{
			Assert(true); //Only run this test in derived classes.
		}

		protected override BusinessObject GetBusinessObjectForHyperlinking(ZFilterGridModule module)
		{
			var dummy = (DummyBusinessObject)base.GetBusinessObjectForHyperlinking(module);
			dummy.HumanReadableNameForTest = "Literally anything";
			return dummy;
		}
	}
}
