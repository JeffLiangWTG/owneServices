using System;
using System.IO;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalCopy.Business;
using Enterprise.UniversalCopy.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using DummyTemplateRecordProvider = Enterprise.ZArchitecture.Modules.Testing.DummyTemplateRecordProvider;

namespace Enterprise.UniversalCopy.GUI.Testing
{
	sealed class ModuleUniversalCopyManagerTest : TestCaseWithFactory
	{
		public void TestUniversalCopyMenuItems()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var copyManager = new ModuleUniversalCopyManager(module))
			{
				Assert(copyManager.AllowsUniversalCopy);
				var menuItems = module.FormActionMenu;
				module.SetupAndGetGrid();
				AssertEquals(1, menuItems.FindByText("&New").MenuItems.Count);
			}

			CreateUniversalCopyTemplate(ConfigurationSourceCodes.NominatedRecord, Guid.Empty);
			CreateUniversalCopyTemplate(ConfigurationSourceCodes.FilteredRecord, Guid.Empty);
			Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var copyManager = new ModuleUniversalCopyManager(module))
			{
				Assert(copyManager.AllowsUniversalCopy);
				var menuItems = module.FormActionMenu;
				module.SetupAndGetGrid();

				var newMenuItem = (ZMenuItem)menuItems.FindByText("&New");
				AssertEquals(3, newMenuItem.MenuItems.Count);
				AssertEquals(2, newMenuItem.MenuItems.FindByText("Universal Copy").MenuItems.Count);
			}
		}

		public void TestAddMenuItems()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var copyManager = new ModuleUniversalCopyManager(module))
			{
				Assert(copyManager.AllowsUniversalCopy);
				var menuItems = module.FormActionMenu;
				module.SetupAndGetGrid();
				AssertEquals(1, menuItems.FindByText("&New").MenuItems.Count);

				CreateUniversalCopyTemplate(ConfigurationSourceCodes.NominatedRecord, Guid.Empty);
				CreateUniversalCopyTemplate(ConfigurationSourceCodes.FilteredRecord, Guid.Empty);
				Factory.Save();

				var newMenuItem = (ZMenuItem)menuItems.FindByText("&New");
				newMenuItem.ShowPopupMenu();
				AssertEquals(2, newMenuItem.MenuItems.Count);
				AssertEquals(2, newMenuItem.MenuItems.FindByText("Universal Copy").MenuItems.Count);

				newMenuItem.ShowPopupMenu();
				AssertEquals(2, newMenuItem.MenuItems.Count);
				AssertEquals(2, newMenuItem.MenuItems.FindByText("Universal Copy").MenuItems.Count);
			}
		}

		public void TestMenuItemClickEvent()
		{
			using var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy);
			using var copyManager = new ModuleUniversalCopyManagerForTest(module);
			AssertEquals(true, copyManager.AllowsUniversalCopy);

			var menuItems = module.FormActionMenu;
			var dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "FM";
			CreateUniversalCopyTemplate(ConfigurationSourceCodes.NominatedRecord, dummy.PK.ToGuid(), "Z0_Code");
			CreateUniversalCopyTemplate(ConfigurationSourceCodes.FilteredRecord, Guid.Empty);

			Factory.Save();

			copyManager.AddMenuItems(module.NewMenuItem, (_, _) => { });

			var universalCopyButton = menuItems.FindByText("&New").MenuItems.FindByText("Universal Copy");
			AssertEquals(2, universalCopyButton.MenuItems.Count);

			// FilteredRecord
			universalCopyButton.MenuItems[0].PerformClick();
			AssertNotEquals((copyManager.NewElementForTest as DummyBusinessObject).Z0_Code, dummy.Z0_Code);

			// NominatedRecord
			universalCopyButton.MenuItems[1].PerformClick();
			AssertEquals((copyManager.NewElementForTest as DummyBusinessObject).Z0_Code, dummy.Z0_Code);
		}

		public void TestMenuItemClickEvent_InactiveTemplateShowsError()
		{
			using (var module = new DummyFilterGridModuleWithTemplateRecordProvider())
			using (var copyManager = new ModuleUniversalCopyManagerForTest(module))
			{
				Assert(copyManager.AllowsUniversalCopy);
				var menuItems = module.FormActionMenu;

				var dummyTemplateRecordProvider = Factory.New<DummyTemplateRecordProvider>();
				dummyTemplateRecordProvider.Z0_Code = "zzz";

				var dummyTemplateRecord = Factory.New<DummyTemplateRecord>();
				dummyTemplateRecord.IsCancelled = true;
				dummyTemplateRecordProvider.TemplateRecord = dummyTemplateRecord;

				CreateUniversalCopyTemplate(ConfigurationSourceCodes.NominatedRecord, dummyTemplateRecordProvider.PK.ToGuid(), "Z0_Code");

				Factory.Save();

				copyManager.AddMenuItems(module.NewMenuItem, (sendr, e) => { });

				var universalCopyButton = menuItems.FindByText("&New").MenuItems.FindByText("Universal Copy");
				AssertEquals(1, universalCopyButton.MenuItems.Count);

				universalCopyButton.MenuItems[0].PerformClick();

				AssertNull("Should not create a new element", copyManager.NewElementForTest);

				CombineAssertions("Should display an error message", () =>
				{
					Assert(UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals(
						"This template is inactive. Please re-activate the nominated record to use this functionality.",
						UnitTestUserNotification.Instance.LastMessage.Text
					);
				});
			}
		}

		public void TestLoadNominatedRecord()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_ModuleID = nameof(ModuleId.Dummy);
			templateRecord.STR_Data = "XYZ";
			Factory.Save();

			var ucTemplate = CreateUniversalCopyTemplate(ConfigurationSourceCodes.NominatedRecord, templateRecord.PK.ToGuid(), "Z0_Code");
			Factory.Save();

			using (var module = new DummyTemplateModule())
			using (var copyManager = new ModuleUniversalCopyManagerForTest(module))
			{
				var dummy = (DummyBusinessObject)copyManager.LoadNominatedRecord(ucTemplate);
				AssertEquals("XYZ", dummy.Z0_Description);
			}
		}

		public void TestLoadNominatedRecord_ModuleWithDifferentInstanceType()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			Factory.Save();

			var ucTemplate = CreateUniversalCopyTemplate(ConfigurationSourceCodes.NominatedRecord, dummy.PK.ToGuid());
			Factory.Save();
			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithInstance2())
			using (var copyManager = new ModuleUniversalCopyManagerForTest(module))
			{
				var loadedDummy = copyManager.LoadNominatedRecord(ucTemplate);
				AssertType(typeof(DummyBusinessObject), loadedDummy);
			}
		}

		UniversalCopyTemplate CreateUniversalCopyTemplate(string configurationSource, Guid nominatedRecordPk, string copyPropertyName = "")
		{
			var newTemplate = Factory.NewWithValidTestData<UniversalCopyTemplate>();
			newTemplate.S9_ModuleID = DummyModuleIDs.Dummy + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;
			newTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			newTemplate.S9_IsPublished = true;
			var copyTemplateTree = new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true))
			{
				ConfigurationName = configurationSource,
				ConfigurationSource = configurationSource,
				NominatedRecordPk = nominatedRecordPk
			};

			if (!string.IsNullOrEmpty(copyPropertyName))
			{
				var propertyNode = (PropertyCopyTemplateNode)((EntityCopyTemplateNode)copyTemplateTree.InnerNode).Nodes.Find(node => node is PropertyCopyTemplateNode && node.Name == copyPropertyName);
				propertyNode.CopyMethod = CopyMethod.Copy;
			}

			var copyTemplateTreeBizo = new CopyTemplateTreeBizo(copyTemplateTree, newTemplate);
			copyTemplateTreeBizo.EntityFilter = new EntityFilter();
			newTemplate.CopyTemplateTree = copyTemplateTreeBizo;

			using (var stream = new MemoryStream())
			{
				copyTemplateTreeBizo.CopyTemplateNode.GetCompactCopy().Serialize(stream);
				newTemplate.S9_FilterData = stream.ToArray();
			}

			return newTemplate;
		}

		public void TestElementTypeIsOverridden()
		{
			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithInstance())
			using (var copyManager = new ModuleUniversalCopyManagerForTest(module))
			{
				AssertEquals(typeof(UniversalCopyDummy2), copyManager.ElementType);
			}
		}

		public void TestCopyMenuClicked_GetSourceElement()
		{
			using (var module = new GridUniversalCopyManagerTest.DummyModuleWithInstance())
			using (var copyManager = new ModuleUniversalCopyManagerForTest(module))
			{
				var dummy1 = Factory.New<UniversalCopyDummy1>();
				var dummy2 = copyManager.CopyMenuClicked_GetSourceElement_Exposed(dummy1);

				Assert(dummy2 is UniversalCopyDummy2);
			}
		}

		#region Test classes

		class ModuleUniversalCopyManagerForTest : ModuleUniversalCopyManager
		{
			public BusinessObject NewElementForTest
			{
				get { return newElementForTest; }
				private set { newElementForTest = value; }
			}
			BusinessObject newElementForTest;

			public ModuleUniversalCopyManagerForTest(ZFilterGridModule gridModule)
				: base(gridModule)
			{
			}

			protected override void CopyMenuClicked_OnNewElement(BusinessObject newElement)
			{
				newElementForTest = newElement;
			}

			public BusinessObject CopyMenuClicked_GetSourceElement_Exposed(BusinessObject selectedElement)
			{
				return CopyMenuClicked_GetSourceElement(selectedElement);
			}
		}

		class DummyTemplateModule : GridUniversalCopyManagerTest.DummyModuleWithUniversalCopy
		{
			protected override bool SupportTemplateRecords => true;

			protected override BusinessObject LoadFromTemplateRecordPkCore(BusinessObjectFactory localFactory, ZGuid templateRecordPk)
			{
				var templateRecord = localFactory.Load<StmTemplateRecord>(templateRecordPk);
				if (templateRecord != null)
				{
					var dummy = localFactory.New<UniversalCopyManagerTest.DummyTemplateRecordProvider>();
					((ITemplateRecordProvider)dummy).LoadFromTemplateRecord(templateRecord);
					return dummy;
				}
				return null;
			}

			protected override IBusinessObjectCollection GetNewGridCollection()
			{
				return new UniversalCopyManagerTest.DummyTemplateRecordProviderCollection(Factory);
			}
		}

		public class DummyFilterGridModuleWithTemplateRecordProvider : DummyFilterGridModule, IZFilterGridModule
		{
			protected override IBusinessObjectCollection GetNewGridCollection() => new ActiveBusinessObjectCollection<DummyTemplateRecordProvider>(Factory);

			bool IZFilterGridModule.AllowTemplateRecords => true;
		}

		#endregion
	}
}
