using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	sealed class UniversalCopyManagerTest : TestCaseWithFactory
	{
		public void TestUniversalCopyListOverrideAttribute()
		{
			var consolType = ObjectFactory.GetType(typeof(Integration.Forwarding.IForwardingConsol));
			AssertUniversalCopyListOverrideAttribute(consolType, ModuleIDs.JobConsol, "JK_OA_SendingForwarderAddress", typeof(OrgAddressCollection));
			AssertUniversalCopyListOverrideAttribute(consolType, ModuleIDs.JobConsol, "JK_OA_ReceivingForwarderAddress", typeof(OrgAddressCollection));
		}

		[ExpectNoExceptions]
		public void TestUniversalCopyListOverrideAttributeWithBusinessObjectCollection()
		{
			using (var copyManager = new UniversalCopyManagerForTest(typeof(ProcessTaskNotification), (ModuleIdentifier)null))
			{
				copyManager.GetComponentTypeFromPath(typeof(ProcessTaskNotification), new[] { "Lookups", "PrintQueues" }, false);
			}
		}

		void AssertUniversalCopyListOverrideAttribute(Type bizOType, ModuleIdentifier moduleId, string propertyName, Type expectedType)
		{
			using (var copyManager = new UniversalCopyManagerForTest(bizOType, moduleId))
			{
				var type = copyManager.GetComponentPropertyListTypeFromPath(new List<string>(), propertyName);
				AssertEquals(expectedType, type);
			}
		}

		public void TestAllowUniversalCopy()
		{
			using (var form = new ZForm())
			{
				using (var copyManager = new UniversalCopyManagerForTest(typeof(DummyA), DummyModuleIDs.Dummy))
				{
					Assert("Do not allow as DummyA has no GLOW interface with description.", !copyManager.AllowsUniversalCopy);
				}

				using (var copyManager = new UniversalCopyManagerForTest(typeof(DummyB), DummyModuleIDs.Dummy))
				{
					Assert("Allow as DummyB has GLOW interface with description.", copyManager.AllowsUniversalCopy);
				}

				using (var copyManager = new UniversalCopyManagerForTest(typeof(DummyC), DummyModuleIDs.Dummy))
				{
					Assert("Allow as DummyC has UniversalCopyWithExtendedEntitiesAttribute applied.", copyManager.AllowsUniversalCopy);
				}
			}
		}

		public void TestImportTemplateMenuClicked_UniversalCopyWithExtendedEntitiesAttribute()
		{
			using (var copyManager = new UniversalCopyManagerForTest(typeof(DummyC), DummyModuleIDs.Dummy))
			using (var menu = new ContextMenu())
			{
				Assert("AllowsUniversalCopy as DummyC has UniversalCopyWithExtendedEntitiesAttribute applied.", copyManager.AllowsUniversalCopy);

				((IUniversalCopyManager)copyManager).AddMenuItems(menu, includeEditMenuItems: true, includeCopySchedulesItem: false, lazyPopulate: false);

				var newFromMenuItem = menu.MenuItems.FindByText("New Copy Template From", false);
				AssertNotNull("Should have 'New Copy Template From' menu.", newFromMenuItem);
				var importFromFileMenuItem = newFromMenuItem.MenuItems.FindByText("Import from File", false);
				AssertNotNull("Should have 'Import from File' menu.", importFromFileMenuItem);

				var openFormCount = Application.OpenForms.Count;

				importFromFileMenuItem.PerformClick();

				AssertEquals("Should have opened a form", openFormCount + 1, Application.OpenForms.Count);
				var dummyForm = Application.OpenForms[Application.OpenForms.Count - 1];
				AssertEquals("The UniversalCopyTemplateForm should display.", typeof(UniversalCopyTemplateForm), dummyForm.GetType());

				dummyForm.Dispose();
			}
		}

		#region Universal Copy Menu Items

		public void TestUniversalCopyMenuItems_TemplatesAreGroupedByTemplateNameWithoutHotKey()
		{
			CreateUCTemplate("P&ath1\\P&ath2\\&T1", isActive: true);
			CreateUCTemplate("P&ath1\\Pa&th2\\&T2", isActive: true);
			CreateUCTemplate("Pa&th1\\P&ath2\\&T3", isActive: true);
			CreateUCTemplate("Pa&th1\\Pa&th2\\&T4", isActive: true);
			Factory.Save();

			using var copyManager = new UniversalCopyManagerForTest(typeof(DummyB), DummyModuleIDs.Dummy2);
			using var menu = new ContextMenu();

			((IUniversalCopyManager)copyManager).AddMenuItems(menu, includeEditMenuItems: true, includeCopySchedulesItem: false, lazyPopulate: false);

			AssertEquals("There should be 5 menu items", 5, menu.MenuItems.Count);
			AssertEquals("P&ath1", menu.MenuItems[0].Text);

			var path1MenuItems = menu.MenuItems[0].MenuItems;
			AssertEquals(1, path1MenuItems.Count);
			AssertEquals("P&ath2", path1MenuItems[0].Text);

			var path2MenuItems = path1MenuItems[0].MenuItems;
			AssertEquals(4, path2MenuItems.Count);
			AssertEquals("&T1", path2MenuItems[0].Text);
			AssertEquals("&T2", path2MenuItems[1].Text);
			AssertEquals("&T3", path2MenuItems[2].Text);
			AssertEquals("&T4", path2MenuItems[3].Text);
		}

		public void TestUniversalCopyMenuItems_WhenNameIsBeginningPartOfPath()
		{
			CreateUCTemplate("&T1\\&T2", isActive: true);
			CreateUCTemplate("&T1", isActive: true);
			CreateUCTemplate("T&1\\T&2\\&T3", isActive: true);
			Factory.Save();

			using var copyManager = new UniversalCopyManagerForTest(typeof(DummyB), DummyModuleIDs.Dummy2);
			using var menu = new ContextMenu();

			((IUniversalCopyManager)copyManager).AddMenuItems(menu, includeEditMenuItems: true, includeCopySchedulesItem: false, lazyPopulate: false);

			AssertEquals("There should be 6 menu items", 6, menu.MenuItems.Count);
			AssertEquals("&T1", menu.MenuItems[0].Text);
			AssertNotNull("First one is template naming1 '&T1'", menu.MenuItems[0].Tag);
			AssertEquals("&T1", menu.MenuItems[1].Text);

			var path1MenuItems = menu.MenuItems[1].MenuItems;
			AssertEquals(2, path1MenuItems.Count);
			AssertEquals("&T2", path1MenuItems[0].Text);
			AssertNotNull("First one should template naming '&T1\\&T2'", path1MenuItems[0].Tag);
			AssertEquals("T&2", path1MenuItems[1].Text);

			var path2MenuItems = path1MenuItems[1].MenuItems;
			AssertEquals(1, path2MenuItems.Count);
			AssertEquals("&T3", path2MenuItems[0].Text);
		}

		public void TestUniversalCopyMenuItems_MenuItemsAreSorted()
		{
			CreateUCTemplate("Alic&e2\\&F2\\&T3", true);
			CreateUCTemplate("Ali&ce1\\&F3\\T&6", true);
			CreateUCTemplate("Bo&b\\&F3\\T&5", true);
			CreateUCTemplate("Al&ice2\\F&2\\T&2", true);
			CreateUCTemplate("ali&ce1\\F&2\\T&4", true);
			CreateUCTemplate("Alic&e\\F&2\\&T1", true);
			Factory.Save();

			using var copyManager = new UniversalCopyManagerForTest(typeof(DummyB), DummyModuleIDs.Dummy2);
			using var menu = new ContextMenu();

			((IUniversalCopyManager)copyManager).AddMenuItems(menu, includeEditMenuItems: true, includeCopySchedulesItem: false, lazyPopulate: false);

			AssertEquals("There should be 8 menu items", 8, menu.MenuItems.Count);
			AssertEquals("Alic&e", menu.MenuItems[0].Text);
			AssertEquals("ali&ce1", menu.MenuItems[1].Text);
			AssertEquals("Al&ice2", menu.MenuItems[2].Text);
			AssertEquals("Bo&b", menu.MenuItems[3].Text);

			var menuItemsUnderAlice1 = menu.MenuItems[1].MenuItems;
			AssertEquals(2, menuItemsUnderAlice1.Count);
			AssertEquals("F&2", menuItemsUnderAlice1[0].Text);
			AssertEquals("&F3", menuItemsUnderAlice1[1].Text);

			var menuItemsUnderAlice2 = menu.MenuItems[2].MenuItems;
			AssertEquals(1, menuItemsUnderAlice2.Count);
			AssertEquals("F&2", menuItemsUnderAlice2[0].Text);

			var menuItemsUnderAlice2F2 = menuItemsUnderAlice2[0].MenuItems;
			AssertEquals(2, menuItemsUnderAlice2F2.Count);
			AssertEquals("T&2", menuItemsUnderAlice2F2[0].Text);
			AssertEquals("&T3", menuItemsUnderAlice2F2[1].Text);
		}

		public void TestPrepareMenuWithActiveAndInactiveTemplates()
		{
			CreateUCTemplate("Template 1", false);
			CreateUCTemplate("Template 2", true);
			Factory.Save();

			using (var copyManager = new UniversalCopyManagerForTest(typeof(DummyB), DummyModuleIDs.Dummy2))
			using (var menu = new ContextMenu())
			{
				((IUniversalCopyManager)copyManager).AddMenuItems(menu, includeEditMenuItems: true, includeCopySchedulesItem: false, lazyPopulate: false);
				var ucMenuItem = menu;

				AssertNull("Should not find copy menu for inactive template.", ucMenuItem.MenuItems.FindByText("Template 1", false));
				AssertNotNull("Should find copy menu for active template.", ucMenuItem.MenuItems.FindByText("Template 2", false));

				var editMenuItem = ucMenuItem.MenuItems.FindByText("Edit Copy Template", false);
				AssertNotNull("Should have edit menu for all templates.", editMenuItem.MenuItems.FindByText("Template 1", false));
				AssertNotNull("Should have edit menu for all templates.", editMenuItem.MenuItems.FindByText("Template 2", false));

				var newFromMenuItem = ucMenuItem.MenuItems.FindByText("New Copy Template From", false);
				AssertNotNull("Should have 'New From' menu for all templates.", newFromMenuItem.MenuItems.FindByText("Template 1", false));
				AssertNotNull("Should have 'New From' menu for all templates.", newFromMenuItem.MenuItems.FindByText("Template 2", false));
			}
		}

		public void TestIncludeCopySchedulesItem()
		{
			CreateUCTemplate("Some Template", true);
			Factory.Save();

			using (var copyManager = new UniversalCopyManagerForTest(typeof(DummyB), DummyModuleIDs.Dummy2))
			using (var ucMenuItem = new ContextMenu())
			{
				((IUniversalCopyManager)copyManager).AddMenuItems(ucMenuItem, includeEditMenuItems: true, includeCopySchedulesItem: true, lazyPopulate: false);
				AssertNotNull("Should find copy schedules menu.", ucMenuItem.MenuItems.FindByText("Copy Schedules", false));
			}

			using (var copyManager = new UniversalCopyManagerForTest(typeof(DummyB), DummyModuleIDs.Dummy2))
			using (var ucMenuItem = new ContextMenu())
			{
				((IUniversalCopyManager)copyManager).AddMenuItems(ucMenuItem, includeEditMenuItems: true, includeCopySchedulesItem: false, lazyPopulate: false);
				AssertNull("Should not find copy schedules menu.", ucMenuItem.MenuItems.FindByText("Copy Schedules", false));
			}
		}

		public void TestGetStoredCopyConfiguration()
		{
			CreateUCTemplate("Alic&e2\\&F2\\&T3", true);
			CreateUCTemplate("ali&ce1\\F&2\\T&4", true);
			CreateUCTemplate("Bo&b\\&F1\\T&5", true);
			CreateUCTemplate("Al&ice2\\F&2\\T&2", true);
			CreateUCTemplate("Alic&e\\F&2\\&T1", true);

			Factory.Save();

			using var copyManager = new UniversalCopyManagerForTest(typeof(DummyB), DummyModuleIDs.Dummy2);
			using var ucMenuItem = new ContextMenu();
			var templates = copyManager.GetStoredCopyConfigurationExposed().ToList();

			AssertEquals(5, templates.Count);
			AssertEquals("Alic&e\\F&2\\&T1", templates[0].CopyTemplateTree.ConfigurationName);
			AssertEquals("ali&ce1\\F&2\\T&4", templates[1].CopyTemplateTree.ConfigurationName);
			AssertEquals("Al&ice2\\F&2\\T&2", templates[2].CopyTemplateTree.ConfigurationName);
			AssertEquals("Alic&e2\\&F2\\&T3", templates[3].CopyTemplateTree.ConfigurationName);
			AssertEquals("Bo&b\\&F1\\T&5", templates[4].CopyTemplateTree.ConfigurationName);
		}

		#endregion

		public void TestUCCollectionFilter()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.JobShipment + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;
			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "Shipment" }, template);
			AssertEquals("Shipments", template.ModuleDescription);

			var copyManager = new UniversalCopyManagerForTest(ObjectFactory.GetType<Integration.Freight.ICommonShipment>(), ModuleIDs.JobShipment);
			using (var form = new UniversalCopyTemplateForm(template, copyManager))
			{
				form.Show();
				var bizo = (FilterStripBusinessObject)template.CopyTemplateTree.FilterStripBizo;
				bool bShipmentNo = false;

				if (bizo.ModuleFilters["Shipment #"] != null)
				{
					bShipmentNo = true;
				}
				Assert("[Shipment #] should be in Collection Filter", bShipmentNo);
			}
		}

		public void TestFiltersWithAssociatedList()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.JobShipment + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;

			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "Note" }, template);
			template.IsActive = true;
			var entityCopyNode = new EntityCopyTemplateNode(typeof(StmNote));
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_NoteType.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_NoteType.DotNetType.Name });
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_Description.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_Description.DotNetType.Name });
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_GC_RelatedCompany.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_GC_RelatedCompany.DotNetType.Name });
			template.CopyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;
			template.PrepareForSave();

			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				var copyManager = new UniversalCopyManagerForTest(typeof(StmNote), module);
				using (var form = new UniversalCopyTemplateForm(template, copyManager))
				{
					form.Show();
					var bizoFilter = (SchemaFilterStripBusinessObject)template.CopyTemplateTree.FilterStripBizo;

					var noteTypeFilter = bizoFilter.ModuleFilters["ST_NoteType"] as ModuleTextFilter;
					AssertNotNull("ST_NoteType filter should be a ModuleTextFilter", noteTypeFilter);
					AssertNotNull("ST_NoteType filter should have an associated list", noteTypeFilter.List);

					var descriptionFilter = bizoFilter.ModuleFilters["ST_Description"] as ModuleTextFilter;
					AssertNotNull("ST_Description filter should be a ModuleTextFilter", descriptionFilter);
					AssertNull("ST_Description filter shouldn't have an associated list", descriptionFilter.List);

					var relatedCompanyFilter = bizoFilter.ModuleFilters["ST_GC_RelatedCompany"];
					AssertNull("ST_GC_RelatedCompany filter shouldn't be in the filter list", relatedCompanyFilter);
					AssertNull("ST_GC_RelatedCompany shouldn't have an associated list once lists should only be loaded for String filters", bizoFilter.ColumnNamesToInclude[StmNoteSchema.ST_GC_RelatedCompany.Name]);
				}
			}
		}

		public void TestFiltersWithAssociatedList_AbstractClass()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.JobShipment + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;

			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "Note" }, template);
			template.IsActive = true;
			var entityCopyNode = new EntityCopyTemplateNode(typeof(StmNoteForTest));
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_NoteType.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_NoteType.DotNetType.Name });
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_Description.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_Description.DotNetType.Name });
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_GC_RelatedCompany.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_GC_RelatedCompany.DotNetType.Name });
			template.CopyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;
			template.PrepareForSave();

			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				var copyManager = new UniversalCopyManagerForTest(typeof(StmNoteForTest), module);
				using (var form = new UniversalCopyTemplateForm(template, copyManager))
				{
					AssertNoExceptionThrown("Shouldn't have 'Abstract business object type was type decided' exception", () => form.Show());
				}
			}
		}

		public void TestFiltersWithAssociatedList_NoExceptionForEmptyList()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.JobShipment + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;

			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "Note" }, template);
			template.IsActive = true;
			var entityCopyNode = new EntityCopyTemplateNode(typeof(StmNote));
			template.CopyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;
			template.PrepareForSave();

			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				var copyManager = new UniversalCopyManagerForTest(typeof(StmNote), module);
				using (var form = new UniversalCopyTemplateForm(template, copyManager))
				{
					form.Show();
					var bizoFilter = (SchemaFilterStripBusinessObject)template.CopyTemplateTree.FilterStripBizo;

					Assert(true); //made it this far with no exceptions!

					/*var noteTypeFilter = bizoFilter.ModuleFilters["ST_NoteType"] as ModuleTextFilter;
					AssertNotNull("ST_NoteType filter should be a ModuleTextFilter", noteTypeFilter);
					AssertNotNull("ST_NoteType filter should have an associated list", noteTypeFilter.List);

					var descriptionFilter = bizoFilter.ModuleFilters["ST_Description"] as ModuleTextFilter;
					AssertNotNull("ST_Description filter should be a ModuleTextFilter", descriptionFilter);
					AssertNull("ST_Description filter shouldn't have an associated list", descriptionFilter.List);

					var relatedCompanyFilter = bizoFilter.ModuleFilters["ST_GC_RelatedCompany"];
					AssertNull("ST_GC_RelatedCompany filter shouldn't be in the filter list", relatedCompanyFilter);
					AssertNull("ST_GC_RelatedCompany shouldn't have an associated list once lists should only be loaded for String filters", bizoFilter.ColumnNamesToInclude[StmNoteSchema.ST_GC_RelatedCompany.Name]);*/
				}
			}
		}

		public void TestGetDefaultFactoryForCopyManager()
		{
			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				var copyManager = new UniversalCopyManagerForTest(typeof(DummyBusinessObject), module);

				var factory1 = copyManager.GetDefaultFactoryForCopyManagerExposed();
				AssertEquals("Should return factory from controller", GridUniversalCopyManagerTest.DummyController.FactoryNameForDebugging, factory1.NameForDebugging);

				var factory2 = copyManager.GetDefaultFactoryForCopyManagerExposed();
				AssertNotEquals("Should return new factory every time", factory1._Instance, factory2._Instance);
			}
		}

		public void TestImportIntoAnotherFactory()
		{
			var factory1 = new BusinessObjectFactory { RefreshEnabled = false };
			var factory2 = new BusinessObjectFactory { RefreshEnabled = false };

			var templateRecord = factory1.New<StmTemplateRecord>();
			templateRecord.STR_Data = "XYZ";
			var dummy = factory1.New<DummyTemplateRecordProvider>();
			((ITemplateRecordProvider)dummy).LoadFromTemplateRecord(templateRecord);
			AssertEquals("XYZ", dummy.Z0_Description);

			var copyManager = new UniversalCopyManagerForTest(typeof(DummyTemplateRecordProvider), DummyModuleIDs.Dummy);

			var otherDummy = (DummyTemplateRecordProvider)copyManager.ImportIntoAnotherFactoryExposed(dummy, typeof(DummyTemplateRecordProvider), factory2);
			var otherTemplateRecordProvider = (ITemplateRecordProvider)otherDummy;
			Assert(otherTemplateRecordProvider.IsTemplateRecord);

			AssertNotEquals("Data record is recreated so should have different PK", dummy.PK, otherDummy.PK);
			AssertNotEquals("Data records should be in different factories", dummy.Factory._Instance, otherDummy.Factory._Instance);
			AssertEquals("XYZ", otherDummy.Z0_Description);

			AssertEquals("Template record should be imported and have same PK", dummy.TemplateRecord.PK, otherDummy.TemplateRecord.PK);
			AssertNotEquals("Template records should be in different factories", dummy.TemplateRecord.Factory._Instance, otherDummy.TemplateRecord.Factory._Instance);
			AssertEquals(dummy.TemplateRecord.STR_Data, otherDummy.TemplateRecord.STR_Data);
		}

		public void TestUCSchemaFilterStripBusinessObject()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.JobShipment + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;

			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "Note" }, template);
			template.IsActive = true;
			var entityCopyNode = new EntityCopyTemplateNode(typeof(StmNote));
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_NoteType.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_NoteType.DotNetType.Name });
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_Description.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_Description.DotNetType.Name });
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_GC_RelatedCompany.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_GC_RelatedCompany.DotNetType.Name });
			template.CopyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;
			template.PrepareForSave();

			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				var copyManager = new UniversalCopyManagerForTest(typeof(StmNote), module);
				using (var form = new UniversalCopyTemplateForm(template, copyManager))
				{
					form.Show();

					Assert("FilterStripBizo should be UCSchemaFilterStripBusinessObject", template.CopyTemplateTree.FilterStripBizo is UCSchemaFilterStripBusinessObject);
					var bizoFilter = (UCSchemaFilterStripBusinessObject)template.CopyTemplateTree.FilterStripBizo;

					Assert("ModuleTextFilter with associated list should be UCModuleTextFilter", bizoFilter.ModuleFilters["ST_NoteType"] is UCModuleTextFilter);
					var noteTypeFilter = bizoFilter.ModuleFilters["ST_NoteType"] as UCModuleTextFilter;
					AssertNotNull("ST_NoteType filter should be a ModuleTextFilter", noteTypeFilter);
					AssertNotNull("ST_NoteType filter should have an associated list", noteTypeFilter.List);
				}
			}
		}

		public void TestFilterDictionaryDuplicatedKey()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.JobShipment + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;

			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "Note" }, template);
			template.IsActive = true;
			var entityCopyNode = new EntityCopyTemplateNode(typeof(StmNote));
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_NoteType.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_NoteType.DotNetType.Name });
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_Description.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_Description.DotNetType.Name });
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_GC_RelatedCompany.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_GC_RelatedCompany.DotNetType.Name });
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = StmNoteSchema.ST_NoteType.Name, CopyMethod = CopyMethod.Copy, PropertyType = StmNoteSchema.ST_NoteType.DotNetType.Name });
			template.CopyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;
			template.PrepareForSave();

			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				var copyManager = new UniversalCopyManagerForTest(typeof(StmNote), module);
				using (var form = new UniversalCopyTemplateForm(template, copyManager))
				{
					AssertNoExceptionThrown(() => form.Show());
					var expected = new StringBuilder();
					expected.AppendLine("An item with the same key has already been added.");
					expected.AppendLine("Entity: StmNote");
					expected.AppendLine("Property: ST_NoteType");
					expected.AppendLine("ST_NoteType | ST_Description | ST_GC_RelatedCompany | ST_NoteType");
					AssertEquals(expected.ToString(), ErrorReporter.LastMessageReported);
					ErrorReporter.Clear();
				}
			}
		}

		public void TestValidateUniversalCopyPreconditions_PreconditionsMet()
		{
			var copyTemplateTree = new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyObjectWithUniversalCopyValidationStrategy), true));
			var bizo = Factory.New<DummyObjectWithUniversalCopyValidationStrategy>();
			var selectedBizos = new[] { bizo };

			Factory.Save();

			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				bizo.SetPreconditionsAsValid = true;
				var copyManager = new UniversalCopyManagerForTest(typeof(DummyObjectWithUniversalCopyValidationStrategy), module);
				copyManager.CopyMenuClicked(copyTemplateTree, selectedBizos);

				AssertEquals("No validation error messages thrown", null, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestValidateUniversalCopyPreconditions_PreconditionsNotMet()
		{
			var copyTemplateTree = new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyObjectWithUniversalCopyValidationStrategy), true));
			var bizo = Factory.New<DummyObjectWithUniversalCopyValidationStrategy>();
			var selectedBizos = new[] { bizo };

			Factory.Save();

			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				bizo.SetPreconditionsAsValid = false;
				var copyManager = new UniversalCopyManagerForTest(typeof(DummyObjectWithUniversalCopyValidationStrategy), module);
				copyManager.CopyMenuClicked(copyTemplateTree, selectedBizos);

				AssertEquals("Test fails with a validation error", "Preconditions not met", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetComponentPropertyListModuleIdFromPath()
		{
			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				var copyManager = new UniversalCopyManagerForTest(typeof(OrgHeader), module);
				var propertyListModuleId = copyManager.GetComponentPropertyListModuleIdFromPath(new List<string>(), "SourceOfLeadPK", out var collectionType);
				AssertEquals(ModuleIDs.Organisation, propertyListModuleId);
				AssertEquals(typeof(OrganisationsFindBoxCollection), collectionType);
			}
		}

		public void TestGetComponentPropertyListModuleIdFromPathForActualDataPropertyType()
		{
			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy())
			{
				var elementType = ObjectFactory.GetType<Integration.Forwarding.IOrder>();
				var copyManager = new UniversalCopyManagerForTest(elementType, module);

				var propertyListModuleId = copyManager.GetComponentPropertyListModuleIdFromPath(new List<string>(), "JD_JE", out var collectionType);
				var expectedCollectionType = ObjectFactory.GetType<Integration.Customs.IBaseJobDeclarationCollection>();

				AssertEquals(ModuleIDs.Customs.JobDeclaration, propertyListModuleId);
				AssertEquals(expectedCollectionType, collectionType);
			}
		}

		public void TestUnknownBusinessObjectTypeDoesntThrowException()
		{
			var copyManager = new UniversalCopyManagerForTest(typeof(StmLoginFailureLog), null, false);
			//no reason why i'm using StmLoginFailureLog specifically, it's just the first BiO I could find that gives the failure i'm looking for

			var template = Factory.New<UniversalCopyTemplate>();

			template.S9_ModuleID = DummyModuleIDs.Dummy2 + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = "Name" }, template);
			template.S9_FilterName = "Name";
			template.IsActive = true;

			var entityCopyNode = new EntityCopyTemplateNode();
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Number, CopyMethod = CopyMethod.Copy });

			template.CopyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;

			template.PrepareForSave();

			AssertNoExceptionThrown(() => copyManager.GetFilter(template.CopyTemplateTree.CopyTemplateNode, new string[] { "Name" }));
		}

		#region Scheduled copy

		public void TestScheduledCopyMenuItems()
		{
			var dummy1 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var dummy2 = Factory.NewWithValidTestData<DummyBusinessObject>();
			var parent = Factory.NewWithValidTestData<DummyBusinessObject>();
			parent.Collection.Add(dummy1);
			parent.Collection.Add(dummy2);
			Factory.Save();

			using (var form = new ZTestForm(parent))
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Show();
				form.Grid.Select(0);

				var ucMenuItem = form.Grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);
				ucMenuItem.PerformSelect();

				var schedulesMenuItem = ucMenuItem.MenuItems.FindByText("Copy Schedules");
				AssertNotNull(schedulesMenuItem);
				AssertEquals("Seperator before Schedules menu item", "-", schedulesMenuItem.Parent.MenuItems[schedulesMenuItem.Index - 1].Text);
				AssertEquals("Schedules menu item is the last menu item", schedulesMenuItem.Parent.MenuItems.Count - 1, schedulesMenuItem.Index);
				schedulesMenuItem.PerformSelect();

				AssertEquals(2, schedulesMenuItem.MenuItems.Count);
				var createScheduleMenuItem = schedulesMenuItem.MenuItems[0];
				AssertEquals("Create Copy Schedule", createScheduleMenuItem.Text);
				AssertEquals("-", schedulesMenuItem.MenuItems[1].Text);
				AssertEquals(false, schedulesMenuItem.MenuItems[1].Visible);

				createScheduleMenuItem.PerformClick();
				AssertType(typeof(UniversalCopyScheduleForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertType(typeof(StmUniversalCopyScheduleTask), ZFormModaliser.LastIBusinessShownOnDialogForTest);
				var scheduleTask = ZFormModaliser.LastIBusinessShownOnDialogForTest as StmUniversalCopyScheduleTask;
				AssertNotNull(scheduleTask);
				AssertEquals(dummy1.PK, scheduleTask.Parent.CopyObject.PK);
				scheduleTask.S5_ScheduleDescription = "Copy Schedule Test";
				var copyTemplate = scheduleTask.Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = scheduleTask.Parent.GridContext;
				copyTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				copyTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true)), copyTemplate);
				copyTemplate.PrepareForSave();
				scheduleTask.Parent.SUC_S9_CopyTemplate = copyTemplate.PK;
				scheduleTask.Factory.Save();

				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
				ucMenuItem.PerformSelect();
				schedulesMenuItem = ucMenuItem.MenuItems.FindByText("Copy Schedules");
				AssertNotNull(schedulesMenuItem);
				AssertEquals("Seperator before Schedules menu item", "-", schedulesMenuItem.Parent.MenuItems[schedulesMenuItem.Index - 1].Text);
				AssertEquals("Schedules menu item is the last menu item", schedulesMenuItem.Parent.MenuItems.Count - 1, schedulesMenuItem.Index);
				schedulesMenuItem.PerformSelect();

				AssertEquals(3, schedulesMenuItem.MenuItems.Count);
				AssertEquals("Create Copy Schedule", schedulesMenuItem.MenuItems[0].Text);
				AssertEquals("-", schedulesMenuItem.MenuItems[1].Text);
				AssertEquals(true, schedulesMenuItem.MenuItems[1].Visible);
				AssertEquals("Edit: Copy Schedule Test", schedulesMenuItem.MenuItems[2].Text);

				schedulesMenuItem.MenuItems[2].PerformClick();
				AssertType(typeof(UniversalCopyScheduleForm), ZFormModaliser.LastFormShownDialogForTest);
				var editScheduleTask = ZFormModaliser.LastIBusinessShownOnDialogForTest as StmUniversalCopyScheduleTask;
				AssertEquals(scheduleTask.PK, editScheduleTask.PK);

				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
				form.Grid.ResetSelection();
				form.Grid.Select(1);
				ucMenuItem.PerformSelect();

				schedulesMenuItem = ucMenuItem.MenuItems.FindByText("Copy Schedules");
				AssertNotNull(schedulesMenuItem);
				AssertEquals("Seperator before Schedules menu item", "-", schedulesMenuItem.Parent.MenuItems[schedulesMenuItem.Index - 1].Text);
				AssertEquals("Schedules menu item is the last menu item", schedulesMenuItem.Parent.MenuItems.Count - 1, schedulesMenuItem.Index);
				schedulesMenuItem.PerformSelect();

				AssertEquals(2, schedulesMenuItem.MenuItems.Count);
				createScheduleMenuItem = schedulesMenuItem.MenuItems[0];
				AssertEquals("Create Copy Schedule", createScheduleMenuItem.Text);
				AssertEquals("-", schedulesMenuItem.MenuItems[1].Text);
				AssertEquals(false, schedulesMenuItem.MenuItems[1].Visible);

				createScheduleMenuItem.PerformClick();
				AssertType(typeof(UniversalCopyScheduleForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertType(typeof(StmUniversalCopyScheduleTask), ZFormModaliser.LastIBusinessShownOnDialogForTest);
				scheduleTask = ZFormModaliser.LastIBusinessShownOnDialogForTest as StmUniversalCopyScheduleTask;
				AssertNotNull(scheduleTask);
				AssertEquals(dummy2.PK, scheduleTask.Parent.CopyObject.PK);
				scheduleTask.S5_ScheduleDescription = "Different Copy Schedule";
				scheduleTask.Parent.SUC_S9_CopyTemplate = copyTemplate.PK;
				scheduleTask.Factory.Save();

				ucMenuItem.PerformSelect();
				schedulesMenuItem = ucMenuItem.MenuItems.FindByText("Copy Schedules");
				AssertNotNull(schedulesMenuItem);
				AssertEquals("Seperator before Schedules menu item", "-", schedulesMenuItem.Parent.MenuItems[schedulesMenuItem.Index - 1].Text);
				AssertEquals("Schedules menu item is the last menu item", schedulesMenuItem.Parent.MenuItems.Count - 1, schedulesMenuItem.Index);
				schedulesMenuItem.PerformSelect();

				AssertEquals(3, schedulesMenuItem.MenuItems.Count);
				AssertEquals("Create Copy Schedule", schedulesMenuItem.MenuItems[0].Text);
				AssertEquals("-", schedulesMenuItem.MenuItems[1].Text);
				AssertEquals(true, schedulesMenuItem.MenuItems[1].Visible);
				AssertEquals("Edit: Different Copy Schedule", schedulesMenuItem.MenuItems[2].Text);

				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
				createScheduleMenuItem.PerformClick();
				AssertType(typeof(UniversalCopyScheduleForm), ZFormModaliser.LastFormShownDialogForTest);
				AssertType(typeof(StmUniversalCopyScheduleTask), ZFormModaliser.LastIBusinessShownOnDialogForTest);
				scheduleTask = ZFormModaliser.LastIBusinessShownOnDialogForTest as StmUniversalCopyScheduleTask;
				AssertNotNull(scheduleTask);
				AssertEquals(dummy2.PK, scheduleTask.Parent.CopyObject.PK);
				scheduleTask.S5_ScheduleDescription = "Third Copy Schedule";
				scheduleTask.Parent.SUC_S9_CopyTemplate = copyTemplate.PK;
				scheduleTask.Factory.Save();

				ucMenuItem.PerformSelect();
				schedulesMenuItem = ucMenuItem.MenuItems.FindByText("Copy Schedules");
				AssertNotNull(schedulesMenuItem);
				AssertEquals("Seperator before Schedules menu item", "-", schedulesMenuItem.Parent.MenuItems[schedulesMenuItem.Index - 1].Text);
				AssertEquals("Schedules menu item is the last menu item", schedulesMenuItem.Parent.MenuItems.Count - 1, schedulesMenuItem.Index);
				schedulesMenuItem.PerformSelect();
				AssertEquals(4, schedulesMenuItem.MenuItems.Count);
				AssertEquals("Create Copy Schedule", schedulesMenuItem.MenuItems[0].Text);
				AssertEquals("-", schedulesMenuItem.MenuItems[1].Text);
				AssertEquals(true, schedulesMenuItem.MenuItems[1].Visible);
				AssertEquals("Edit: Different Copy Schedule", schedulesMenuItem.MenuItems[2].Text);
				AssertEquals("Edit: Third Copy Schedule", schedulesMenuItem.MenuItems[3].Text);
			}
		}

		public void TestEditScheduleWithBothTypesOfMenuItems_ShouldShowEditScheduleForm()
		{
			var copyTask = Factory.NewWithValidTestData<StmUniversalCopyScheduleTask>();
			Factory.Save();

			using (var manager = new UniversalCopyManagerForTest(typeof(DummyA), DummyModuleIDs.Dummy))
			{
				var menuItem = new ZMenuItem("Edit: Test", manager.EditScheduleMenuClicked) { Tag = copyTask.PK };
				menuItem.PerformClick();
				AssertEquals("The UniversalCopyScheduleForm was not displayed when MenuItem clicked.", typeof(UniversalCopyScheduleForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				ZFormModaliser.LastFormShownForTest = null;

				var toolStripMenuItem = MenuItemToToolStripItemConverter.ConvertMenuItemToToolStripItem(menuItem);
				AssertEquals("Failed to copy PK into the Tag when converting to ZToolStripMenuItem", copyTask.PK, toolStripMenuItem.Tag);

				toolStripMenuItem.PerformClick();
				AssertEquals("The UniversalCopyScheduleForm was not displayed when ZMenuItem converted to ZToolStripMenuItem.", typeof(UniversalCopyScheduleForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			}
		}

		public void TestScheduledCopyMenuItemOnUnsupportedType()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithUnsupportedTablePrefix>();
			var parent = Factory.NewWithValidTestData<DummyBusinessObject>();
			parent.Collection.Add(dummy);
			Factory.Save();

			using (var form = new ZTestForm(parent))
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Show();
				form.Grid.Select(0);

				var ucMenuItem = form.Grid.ContextMenu.MenuItems.FindByText("Universal Copy", true);
				ucMenuItem.PerformSelect();
				var schedulesMenuItem = ucMenuItem.MenuItems.FindByText("Copy Schedules");
				AssertNotNull(schedulesMenuItem);
				schedulesMenuItem.PerformSelect();
				var createScheduleMenuItem = schedulesMenuItem.MenuItems[0];
				createScheduleMenuItem.PerformClick();

				AssertEquals("Copy schedule cannot be created: Error - SUC_CopyObjectTableCode: Scheduled Universal Copy is not currently supported on the selected entity type.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetPropertyType()
		{
			var dummy = Factory.New<UniversalCopyDummy4>();
			using (var manager = new UniversalCopyManagerForTest(typeof(UniversalCopyDummy4), DummyModuleIDs.Dummy))
			{
				var type = manager.GetPropertyType(typeof(UniversalCopyDummy4), "PropertyInDefinition");
				AssertEquals("DummyChildBusinessObjectCollectionForTest", type.Name);
			}
		}

		public void TestShouldRemoveIgnoreElementWhenCreateScheduledCopyMenuItem()
		{
			using (var manager = new UniversalCopyManagerForTest(typeof(DummyUniversalCopyBusinessObject), DummyModuleIDs.Dummy))
			{
				var dummy = Factory.New<DummyUniversalCopyBusinessObject>();
				manager.CopyObject = dummy;

				var menuItem = new ZMenuItem("Create Copy Schedule", manager.CreateScheduleMenuClicked) { };
				menuItem.PerformClick();

				var scheduleTask = ZFormModaliser.LastIBusinessShownOnDialogForTest as StmUniversalCopyScheduleTask;
				scheduleTask.S5_ScheduleDescription = "Copy Schedule Test";

				var copyTemplate = scheduleTask.Factory.New<UniversalCopyTemplate>();
				copyTemplate.S9_ModuleID = scheduleTask.Parent.GridContext;
				copyTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
				copyTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyUniversalCopyBusinessObject), true)), copyTemplate);
				copyTemplate.PrepareForSave();

				var nodes = ((EntityCopyTemplateNode)copyTemplate.CopyTemplateTree.CopyTemplateNode.InnerNode).Nodes;
				AssertCollectionContains("Should contains Z0_Long", "Z0_Long", nodes.Select(x => x.Name));

				scheduleTask.Parent.SUC_S9_CopyTemplate = copyTemplate.PK;
				scheduleTask.Factory.Save();

				nodes = ((EntityCopyTemplateNode)scheduleTask.Parent.Template.CopyTemplateTree.CopyTemplateNode.InnerNode).Nodes;
				AssertCollectionNotContains("Should not contains Z0_Long", "Z0_Long", nodes.Select(x => x.Name));
			}
		}

		#endregion

		#region TestUniversalCopyShouldShowErrorMessageIfCopyResultObjectIsNull

		public void TestUniversalCopyShouldShowErrorMessageIfCopyResultObjectIsNull()
		{
			var source = Factory.New<DummyBusinessObject>();

			var copyTemplate = new CopyTemplateTree(typeof(DummyBusinessObject));
			var propertyNode = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplate.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == "Z0_Code");
			propertyNode.CopyMethod = CopyMethod.Default;
			propertyNode.Value = "blabla";

			using (var copyManager = new UniversalCopyManagerForTest(typeof(DummyBusinessObject), DummyModuleIDs.Dummy, true))
			{
				AssertNoExceptionThrown(() => copyManager.CopyMenuClicked(copyTemplate, new[] { source }));
			}
		}

		#endregion

		#region Implementations

		void CreateUCTemplate(string name, bool isActive)
		{
			var template = Factory.New<UniversalCopyTemplate>();

			template.S9_ModuleID = DummyModuleIDs.Dummy2 + StmModuleFilter.ModuleIdSuffix.UniversalCopyTemplate;
			template.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree { ConfigurationName = name }, template);
			template.S9_FilterName = name;
			template.IsActive = isActive;

			var entityCopyNode = new EntityCopyTemplateNode();
			entityCopyNode.Nodes.Add(new PropertyCopyTemplateNode { Name = DummyBizoSchema.Constants.Z0_Number, CopyMethod = CopyMethod.Copy });

			template.CopyTemplateTree.CopyTemplateNode.InnerNode = entityCopyNode;

			template.PrepareForSave();
		}

		#endregion

		#region Test classes

		[UniversalCopyWithExtendedEntities]
		[UniversalCopyIgnoreElement("Z0_Long")]
		class DummyUniversalCopyBusinessObject : DummyBusinessObject
		{
			public DummyUniversalCopyBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }
		}

		public class DummyTemplateRecordProvider : DummyBusinessObject, ITemplateRecordProvider
		{
			public DummyTemplateRecordProvider(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			void ITemplateRecordProvider.SaveToTemplateRecord()
			{
				TemplateRecord.STR_Data = Z0_Description;
			}

			void ITemplateRecordProvider.LoadFromTemplateRecord(ITemplateRecord templateRecord)
			{
				TemplateRecord = (StmTemplateRecord)templateRecord;
				Z0_Description = TemplateRecord.STR_Data;
				((ITemplateRecordProvider)this).IsTemplateRecord = true;
			}

			bool ITemplateRecordProvider.IsTemplateRecord { get; set; }

			ITemplateRecord ITemplateRecordProvider.TemplateRecord
			{
				get => TemplateRecord;
				set => TemplateRecord = (StmTemplateRecord)value;
			}

			public StmTemplateRecord TemplateRecord { get; private set; }

			BusinessObject ITemplateRecordProvider.InstantiateFromTemplateRecord(BusinessObjectFactory factory, Type elementType, ITemplateRecord templateRecord)
			{
				var otherElement = factory.New(elementType);
				var otherTemplateRecordProvider = (ITemplateRecordProvider)otherElement;
				otherTemplateRecordProvider.LoadFromTemplateRecord(templateRecord);
				return otherElement;
			}
		}

		public class DummyTemplateRecordProviderCollection : BusinessObjectCollection<DummyTemplateRecordProvider>
		{
			public DummyTemplateRecordProviderCollection(BusinessObjectFactory factory) : base(factory) { }
		}

		internal class UniversalCopyManagerForTest : UniversalCopyManager
		{
			public UniversalCopyManagerForTest(Type elementType, ZModule module)
				: base(elementType, module)
			{
			}

			public UniversalCopyManagerForTest(Type elementType, ModuleIdentifier moduleId)
				: base(elementType, moduleId)
			{
			}

			public UniversalCopyManagerForTest(Type elementType, ModuleIdentifier moduleId, bool needNullSourceElement)
				: base(elementType, moduleId)
			{
				NeedNullSourceElement = needNullSourceElement;
			}

			protected override CopyTemplateTree GetCopyTemplateTreeFromImport()
			{
				var copyTemplateTree = new CopyTemplateTree(typeof(DummyBusinessObject));
				return copyTemplateTree;
			}

			protected override bool CopyMenuClicked_TryGetCopyTargets(out IEnumerable<BusinessObject> copyTargets)
			{
				throw new NotImplementedException();
			}

			protected override void CopyMenuClicked_OnNewElement(BusinessObject newElement)
			{
				return;
			}

			protected override bool SchedulesMenuSelect_TryGetScheduleTarget(out BusinessObject scheduleTarget)
			{
				scheduleTarget = null;
				return false;
			}

			protected override bool CreateScheduleMenuClicked_TryGetScheduleTarget(out BusinessObject scheduleTarget)
			{
				scheduleTarget = CopyObject;
				return true;
			}

			public BusinessObject CopyObject { get; set; }

			protected override BusinessObject CopyMenuClicked_GetSourceElement(BusinessObject selectedElement)
			{
				return NeedNullSourceElement ? null : base.CopyMenuClicked_GetSourceElement(selectedElement);
			}

			public bool NeedNullSourceElement { get; }

			internal BusinessObjectFactory GetDefaultFactoryForCopyManagerExposed()
			{
				return GetDefaultFactoryForCopyManager();
			}

			internal BusinessObject ImportIntoAnotherFactoryExposed(BusinessObject selectedElement, Type elementType, BusinessObjectFactory otherFactory)
			{
				return ImportIntoAnotherFactory(selectedElement, elementType, otherFactory);
			}

			internal IEnumerable<UniversalCopyTemplate> GetStoredCopyConfigurationExposed()
			{
				return GetStoredCopyConfiguration();
			}
		}

		internal class DummyA { }
		[GlowInterfaceReference("IDummyBizo")]
		internal class DummyB { }
		[UniversalCopyWithExtendedEntities]
		internal class DummyC { }

		class DummyWithUnsupportedTablePrefix : DummyBusinessObject
		{
			public DummyWithUnsupportedTablePrefix(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{ }

			public override string TablePrefix
			{
				get { return "XXX"; }
			}
		}

		internal class DummyWhichSelectivelySupportsUniversalCopy : DummyBusinessObject, IUniversalCopySelectivelySupportable
		{
			public DummyWhichSelectivelySupportsUniversalCopy(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public string ReasonForNotSupportingUniversalCopy { get; set; }

			public bool SupportsUniversalCopy { get; set; }
		}

		#endregion
	}

	[UniversalCopyInstanceType(InstanceType = typeof(UniversalCopyDummy2))]
	internal class UniversalCopyDummy1 : DummyBusinessObject
	{
		public UniversalCopyDummy1(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}

	[UniversalCopyAssociateElement("PropertyInDefinition", "PropertyInClass")]
	abstract class UniversalCopyDummyAbstract : DummyBusinessObject
	{
		protected UniversalCopyDummyAbstract(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public DummyChildBusinessObjectCollection PropertyInClass { get; }
	}

	class UniversalCopyDummy4 : UniversalCopyDummyAbstract
	{
		public UniversalCopyDummy4(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new DummyChildBusinessObjectCollectionForTest PropertyInClass { get; }
	}

	class DummyChildBusinessObjectCollectionForTest : DummyChildBusinessObjectCollection
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Baseline")]
		DummyChildBusinessObjectCollectionForTest(BusinessObjectFactory factory) : base(factory)
		{ }
	}

	[UniversalCopyInstanceType(GetSourceMethod = "GetSourceForUniversalCopy")]
	internal class UniversalCopyDummy : DummyBusinessObject
	{
		public UniversalCopyDummy(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected BusinessObject GetSourceForUniversalCopy()
		{
			var result = CreateNewFactory().ImportFromAnotherFactory(this, typeof(UniversalCopyDummy)) as UniversalCopyDummy;
			result.Z0_Description = "This is customized for Universal Copy.";
			return result;
		}
	}

	abstract class StmNoteForTest : StmNote
	{
		public StmNoteForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}
	}

	class DummyObjectWithUniversalCopyValidationStrategy : DummyBusinessObject, IUniversalCopyValidationStrategy
	{
		public DummyObjectWithUniversalCopyValidationStrategy(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

		public bool SetPreconditionsAsValid { get; set; }

		public string ValidateUniversalCopyPreconditions(CopyTemplateTree configurationTree)
		{
			return SetPreconditionsAsValid ? null : "Preconditions not met";
		}
	}
}
