using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.Security.Provider;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Security.Testing
{
	abstract class StmMenuItemsSecurityInfoProviderTest<T> : TestCaseWithFactory
			where T : StmMenuItemsSecurityInfoProvider
	{
		public void TestAllMenuItemsLoaded()
		{
			var vector = new SecurityVector();
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			var moduleTreeloader = ObjectFactory.Get<IModuleTreeLoader>();
			moduleTreeloader.Initialise(ModuleTree.Tree, security);
			moduleTreeloader.LoadModules();

			vector.Initialise(security);
			foreach (ModuleCategory category in ModuleTree.Tree.Categories.Values)
			{
				foreach (ModuleSection section in category.Sections.Values)
				{
					foreach (INamedModule module in section.Modules.Values)
					{
						using (var newModule = ObjectFactory.Get<IModuleFactory>().Create(module.ModuleID))
						{
							if (newModule.BusinessContexts != null)
							{
								var filter = GetNewMenuItemQuery();
								filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, newModule.BusinessContexts);
								filter.AddToFilter(StmMenuItemSchema.SU_IsPublished, true);
								var menuItems = new BusinessObjectFactory().Load<StmMenuItem>(filter);

								foreach (StmMenuItem item in menuItems)
								{
									SecurityCheckpoint checkpoint = GetMenuItemCheckpoint(security, item.PK.ToGuid(), item.SU_MenuName, module.ModuleID, module.SecurityCheckpoint);
									ISecurityInfo securityInfo = vector.FirstOrDefault(info => info.Checkpoint == checkpoint);
									AssertNotNull("Node '" + item.SU_MenuName + "' in Module '" + module.ModuleID.ToString() + "' and Business Context '" + item.SU_BusinessContext + "' should exist.", securityInfo);
									AssertEquals(CheckpointHelper.ModuleIDPrefix + module.ModuleID.ToString(), securityInfo.Checkpoint.Code);
									AssertEquals(item.PK, securityInfo.Checkpoint.ItemGuid);

									AssertSecurityInfo(securityInfo, item, module.ModuleID.ToString());
								}
							}
						}
					}
				}
			}
		}

		protected abstract void AssertSecurityInfo(ISecurityInfo securityInfo, StmMenuItem item, string moduleID);

		public void TestMenuItems_WithSameNameAndMenuPath_ShowsDescription()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			using (var shipmentModule = ObjectFactory.Get<IModuleFactory>().Create(ModuleIDs.JobShipment))
			{
				var provider = new ModuleSecurityInfoProvider(new RootSecurityInfoProvider(security), shipmentModule);

				var item1 = GetNewMenuItem();
				item1.SU_BusinessContext = "Shipment";
				item1.SU_IsPublished = true;
				item1.SU_MenuName = "ThisIsAMenuName";
				item1.SU_MenuPath = "AndThatIsAMenuPath";
				item1.SU_FilterList = "Filter1";
				item1.SU_DocumentDirection = nameof(DocumentDirection.DEP);
				item1.SU_Hint = "Description for item1";
				item1.SU_AddressCategory = OrgAddressCategory.Codes.Office;

				var item2 = GetNewMenuItem();
				item2.SU_BusinessContext = "Shipment";
				item2.SU_IsPublished = true;
				item2.SU_MenuName = "ThisIsAMenuName";
				item2.SU_MenuPath = "AndThatIsAMenuPath";
				item2.SU_FilterList = "Filter2";
				item2.SU_DocumentDirection = nameof(DocumentDirection.ARV);
				item2.SU_Hint = "Description for item2";
				item2.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

				var item3 = GetNewMenuItem();
				item3.SU_BusinessContext = "Shipment";
				item3.SU_IsPublished = true;
				item3.SU_MenuName = "ThisIsAMenuName";
				item3.SU_MenuPath = "AndThatIsAMenuPath";
				item3.SU_FilterList = "Filter3";
				item3.SU_DocumentDirection = nameof(DocumentDirection.ARV);
				item3.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;

				Factory.Save();

				var filter = GetNewMenuItemQuery();
				filter.AddToFilter(StmMenuItemSchema.SU_BusinessContext, "Shipment");
				filter.AddToFilter(StmMenuItemSchema.SU_IsPublished, true);
				filter.AddToFilter(StmMenuItemSchema.SU_MenuName, "ThisIsAMenuName");
				filter.AddToFilter(StmMenuItemSchema.SU_MenuPath, "AndThatIsAMenuPath");
				filter.OrderBy = StmMenuItemSchema.Constants.SU_FilterList;
				var menuItems = new StmMenuItemsForSecurity(new BusinessObjectFactory().Load<StmMenuItem>(filter));
				AssertEquals(3, menuItems.Count);

				var stmMenuItemsProvider = (T)provider.GetChildren().First(info => info.Name == CheckpointHelper.DisplayText);
				var menuItemProviders = stmMenuItemsProvider.GetChildren().ToArray();

				AssertEquals("Only distinct elements left", 3, menuItems.Distinct().Count());

				SecurityCheckpoint checkpoint1 = GetMenuItemCheckpoint(security, menuItems.GetValue(0).PK.ToGuid(), menuItems.GetValue(0).SU_MenuName, ModuleIDs.JobShipment, null);
				SecurityCheckpoint checkpoint2 = GetMenuItemCheckpoint(security, menuItems.GetValue(1).PK.ToGuid(), menuItems.GetValue(1).SU_MenuName, ModuleIDs.JobShipment, null);
				SecurityCheckpoint checkpoint3 = GetMenuItemCheckpoint(security, menuItems.GetValue(2).PK.ToGuid(), menuItems.GetValue(2).SU_MenuName, ModuleIDs.JobShipment, null);

				AssertEquals(menuItems.GetValue(0).SU_MenuPath.Replace("/", " -- ") + " -- " + menuItems.GetValue(0).SU_MenuName + " (" + menuItems.GetValue(0).SU_Hint + ")", checkpoint1.DisplayText);
				AssertEquals(menuItems.GetValue(1).SU_MenuPath.Replace("/", " -- ") + " -- " + menuItems.GetValue(1).SU_MenuName + " (" + menuItems.GetValue(1).SU_Hint + ")", checkpoint2.DisplayText);
				AssertEquals(menuItems.GetValue(2).SU_MenuPath.Replace("/", " -- ") + " -- " + menuItems.GetValue(2).SU_MenuName + " (" + CheckpointHelper.FallbackHint + ")", checkpoint3.DisplayText);
			}
		}

		protected virtual StmMenuItem GetNewMenuItem()
		{
			return Factory.New<StmMenuItem>();
		}

		protected abstract ZQuery GetNewMenuItemQuery();
		protected abstract SecurityCheckpoint GetMenuItemCheckpoint(SecurityCore security, Guid guid, string menuName, ModuleIdentifier moduleId, ISecurityCheckpoint parent);
		protected abstract StmMenuItemCheckpointHelper CheckpointHelper { get; }
	}
}
