using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Security.Provider.Testing
{
	abstract class StmMenuItemSecurityInfoProviderTest<T> : TestCaseWithFactory
			where T : StmMenuItemSecurityInfoProvider
	{
		public void TestGetDocumentCheckPoint()
		{
			var security = new SecurityCore(new GlbSecurityCollection(Factory), Factory.New<GlbStaff>(), Guid.Empty, Guid.Empty, Guid.Empty);
			using (var shipmentModule = ObjectFactory.Get<IModuleFactory>().Create(ModuleIDs.JobShipment))
			{
				var provider = new ModuleSecurityInfoProvider(new RootSecurityInfoProvider(security), shipmentModule);

				var item1 = Factory.NewWithValidTestData<StmMenuItem>();
				item1.SU_BusinessContext = "Shipment";
				item1.SU_IsPublished = true;
				item1.SU_MenuName = "ThisIsAMenuName";
				item1.SU_MenuPath = "AndThatIsAMenuPath";
				item1.SU_FilterList = "Filter1";
				item1.SU_DocumentDirection = nameof(DocumentDirection.DEP);
				item1.SU_Hint = "Description for item1";
				item1.SU_AddressCategory = OrgAddressCategory.Codes.Office;
				Factory.Save();

				var menuItems = new StmMenuItemsForSecurity(new StmMenuItem[] { item1 });
				var checkPoint = StmMenuItemSecurityInfoProvider.GetDocumentCheckPoint(provider, shipmentModule, menuItems, 0, MenuItemCheckpointHelper);

				AssertEquals(MenuItemCheckpointHelper.ModuleIDPrefix + shipmentModule.ModuleID.ToString(), checkPoint.Code);
				AssertEquals(menuItems.GetValue(0).SU_MenuPath.Replace("/", " -- ") + " -- " + menuItems.GetValue(0).SU_MenuName, checkPoint.DisplayText);

				var item2 = Factory.NewWithValidTestData<StmMenuItem>();
				item2.SU_BusinessContext = "Shipment";
				item2.SU_IsPublished = true;
				item2.SU_MenuName = "ThisIsAMenuName";
				item2.SU_MenuPath = "AndThatIsAMenuPath";
				item2.SU_FilterList = "Filter2";
				item2.SU_DocumentDirection = nameof(DocumentDirection.ARV);
				item2.SU_Hint = "Description for item2";
				item2.SU_AddressCategory = OrgAddressCategory.Codes.Receivables;
				Factory.Save();

				menuItems = new StmMenuItemsForSecurity(new StmMenuItem[] { item1, item2 });
				checkPoint = StmMenuItemSecurityInfoProvider.GetDocumentCheckPoint(provider, shipmentModule, menuItems, 1, MenuItemCheckpointHelper);

				AssertEquals(MenuItemCheckpointHelper.ModuleIDPrefix + shipmentModule.ModuleID.ToString(), checkPoint.Code);
				AssertEquals(menuItems.GetValue(1).SU_MenuPath.Replace("/", " -- ") + " -- " + menuItems.GetValue(0).SU_MenuName + " (" + menuItems.GetValue(1).SU_Hint + ")", checkPoint.DisplayText);
			}
		}

		protected abstract StmMenuItemCheckpointHelper MenuItemCheckpointHelper { get; }
	}
}
