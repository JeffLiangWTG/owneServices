using System;
using System.IO;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmMenuItemBaseCollection))]
	sealed class StmMenuItemBaseCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestPrivateDocumentVisible()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			Factory.Save();

			var menu1 = Factory.New<StmMenuItemBase>();
			var menu2 = Factory.New<StmMenuItemBase>();
			var menu3 = Factory.New<StmMenuItemBase>();

			menu1.SU_BusinessContext = "a";
			menu2.SU_BusinessContext = "a";
			menu3.SU_BusinessContext = "a";

			menu1.SU_GS_NKStaffCode = ZString.Empty;
			menu2.SU_GS_NKStaffCode = "TT";
			menu3.SU_GS_NKStaffCode = "xxx";

			var menus = new StmMenuItemBaseCollection(Factory);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentCompanyPK))
			{
				menus.Load(StmMenuItemBaseCollection.GetApplicableMenusFilterForTesting("a", true));
				AssertEquals("Count", 2, menus.Count);
			}

			using (Env.SetTemporaryUserContext(new UserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				menus.Load(StmMenuItemBaseCollection.GetApplicableMenusFilterForTesting("a", true));
				AssertEquals("Count", 3, menus.Count);
			}

			staff.GS_IsController = true;
			Factory.Save();
			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentCompanyPK))
			{
				var staffMenus = new StmMenuItemBaseCollection(Factory);
				staffMenus.Load(StmMenuItemBaseCollection.GetApplicableMenusFilterForTesting("a", true));
				AssertEquals("Count", 3, staffMenus.Count);
			}
		}

		public void TestPrivateDocumentVisibleForUserWithAccessUnpublishedCustomizedDocumentsAndReportsSecurityCheckPoint()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			Factory.Save();

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "TT2";
			Factory.Save();

			var menu1 = Factory.New<StmMenuItemBase>();
			var menu2 = Factory.New<StmMenuItemBase>();
			var menu3 = Factory.New<StmMenuItemBase>();

			menu1.SU_BusinessContext = "a";
			menu2.SU_BusinessContext = "a";
			menu3.SU_BusinessContext = "a";

			menu1.SU_GS_NKStaffCode = ZString.Empty;
			menu2.SU_GS_NKStaffCode = "TT";
			menu3.SU_GS_NKStaffCode = "xxx";

			var menus = new StmMenuItemBaseCollection(Factory);

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentCompanyPK))
			{
				Assert(!Env.Security.AccessUnpublishedCustomizedDocumentsAndReports.IsAllowed);
				menus.Load(StmMenuItemBaseCollection.GetApplicableMenusFilterForTesting("a", true));
				AssertEquals("Count", 2, menus.Count);
			}

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentCompanyPK))
			{
				Env.Security.AccessUnpublishedCustomizedDocumentsAndReports.IsAllowed = true;
				menus.Load(StmMenuItemBaseCollection.GetApplicableMenusFilterForTesting("a", true));
				AssertEquals("Count", 3, menus.Count);
			}
		}

		public void TestEditingModeGetsPassedOn()
		{
			StmMenuItemBase sysMenu = Factory.New<StmMenuItemBase>();
			sysMenu.SU_MenuName = "SysMenu";
			sysMenu.SU_IsSystemDefined = true;

			StmMenuItemBase userMenu = Factory.New<StmMenuItemBase>();
			userMenu.SU_MenuName = "UserMenu";
			userMenu.SU_IsSystemDefined = false;

			StmMenuItemBase clientMenu = Factory.New<StmMenuItemBase>();
			clientMenu.SU_MenuName = "ClientMenu";
			clientMenu.SU_IsClientSpecific = true;
			clientMenu.SU_IsSystemDefined = true;

			Collection.Load();
			CheckCollectionEditingMode(MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, sysMenu, userMenu, clientMenu);

			Collection.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			CheckCollectionEditingMode(MenuEditingMode.AllowEditingOfClientSpecificOnly, sysMenu, userMenu, clientMenu);
		}

		void CheckCollectionEditingMode(MenuEditingMode expectedMode, StmMenuItemBase sysMenu, StmMenuItemBase userMenu, StmMenuItemBase clientMenu)
		{
			AssertEquals("Collection.EditingMode", expectedMode, Collection.EditingMode);
			AssertEquals("sysMenu.EditingMode", expectedMode, sysMenu.EditingMode);
			AssertEquals("userMenu.EditingMode", expectedMode, userMenu.EditingMode);
			AssertEquals("clientMenu.EditingMode", expectedMode, clientMenu.EditingMode);
		}

		public void TestEditingMode()
		{
			StmTemplateBase sysTemplate = Factory.New<StmTemplateBase>();
			sysTemplate.SO_Name = "SysTemplate";
			sysTemplate.SO_IsSystemDefined = true;
			sysTemplate.SO_DataContext = nameof(Core.Constants.DataContext.None);

			StmTemplateBase userTemplate = Factory.New<StmTemplateBase>();
			userTemplate.SO_Name = "UserTemplate";
			userTemplate.SO_IsSystemDefined = false;
			userTemplate.SO_DataContext = nameof(Core.Constants.DataContext.None);

			StmTemplateBase clientTemplate = Factory.New<StmTemplateBase>();
			clientTemplate.SO_Name = "ClientTemplate";
			clientTemplate.SO_IsSystemDefined = true;
			clientTemplate.SO_IsClientSpecific = true;
			clientTemplate.SO_DataContext = nameof(Core.Constants.DataContext.None);

			StmMenuItemBase sysMenu = Factory.New<StmMenuItemBase>();
			sysMenu.SU_MenuName = "SysMenu";
			sysMenu.SU_IsSystemDefined = true;

			StmMenuItemBase userMenu = Factory.New<StmMenuItemBase>();
			userMenu.SU_MenuName = "UserMenu";
			userMenu.SU_IsSystemDefined = false;

			StmMenuItemBase clientMenu = Factory.New<StmMenuItemBase>();
			clientMenu.SU_MenuName = "ClientMenu";
			clientMenu.SU_IsClientSpecific = true;
			clientMenu.SU_IsSystemDefined = true;

			StmMenuTemplatePivot sysPivot = Factory.New<StmMenuTemplatePivot>();
			sysPivot.SI_DocumentTitle = "SysPivot";
			sysPivot.SI_IsClientSpecific = false;
			sysPivot.SI_IsSystemDefined = true;
			sysPivot.SI_SO = sysTemplate.PK;
			sysPivot.SI_SU = sysMenu.PK;

			StmMenuTemplatePivot userPivot = Factory.New<StmMenuTemplatePivot>();
			userPivot.SI_DocumentTitle = "UserPivot";
			userPivot.SI_IsClientSpecific = false;
			userPivot.SI_IsSystemDefined = false;
			userPivot.SI_SO = userTemplate.PK;
			userPivot.SI_SU = userMenu.PK;

			StmMenuTemplatePivot clientPivot = Factory.New<StmMenuTemplatePivot>();
			clientPivot.SI_DocumentTitle = "ClientPivot";
			clientPivot.SI_IsClientSpecific = true;
			clientPivot.SI_IsSystemDefined = true;
			clientPivot.SI_SO = clientTemplate.PK;
			clientPivot.SI_SU = clientMenu.PK;

			StmMenuItemBaseCollection collection = new StmMenuItemBaseCollection(Factory);
			collection.Load();

			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, collection.EditingMode);
			AssertEquals("SysMenu ReadOnly", true, collection.FindByPK(sysMenu.PK).SU_MenuNameInfo.ReadOnly);
			AssertEquals("UserMenu ReadOnly", false, collection.FindByPK(userMenu.PK).SU_MenuNameInfo.ReadOnly);
			AssertEquals("ClientMenu ReadOnly", true, collection.FindByPK(clientMenu.PK).SU_MenuNameInfo.ReadOnly);

			AssertEquals("SysMenu CanDelete", false, (collection.FindByPK(sysMenu.PK)).CanDelete);
			AssertEquals("UserMenu CanDelete", true, (collection.FindByPK(userMenu.PK)).CanDelete);
			AssertEquals("ClientMenu CanDelete", false, (collection.FindByPK(clientMenu.PK)).CanDelete);

			AssertEquals("SysMenu IsSystemDefined Column ReadOnly", true, collection.FindByPK(sysMenu.PK).SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("UserMenu IsSystemDefined Column ReadOnly", true, collection.FindByPK(userMenu.PK).SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("ClientMenu IsSystemDefined Column ReadOnly", true, collection.FindByPK(clientMenu.PK).SU_IsSystemDefinedInfo.ReadOnly);

			AssertEquals("SysMenu IsClientSpecific Column ReadOnly", true, collection.FindByPK(sysMenu.PK).SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("UserMenu IsClientSpecific Column ReadOnly", true, collection.FindByPK(userMenu.PK).SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("ClientMenu IsClientSpecific Column ReadOnly", true, collection.FindByPK(clientMenu.PK).SU_IsClientSpecificInfo.ReadOnly);

			AssertEquals("Sys Pivots ReadOnly", false, collection.FindByPK(sysMenu.PK).Documents.ReadOnly);
			AssertEquals("User Pivots ReadOnly", false, collection.FindByPK(userMenu.PK).Documents.ReadOnly);
			AssertEquals("Client Pivots ReadOnly", false, collection.FindByPK(clientMenu.PK).Documents.ReadOnly);

			collection.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			CheckForAllowEditingOfSystemDefinedOnly(collection, sysMenu, sysTemplate, userMenu, userTemplate, clientMenu, clientTemplate);

			collection.AddNew();
			CheckForAllowEditingOfSystemDefinedOnly(collection, sysMenu, sysTemplate, userMenu, userTemplate, clientMenu, clientTemplate);

			collection.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			CheckForAllowEditingOfIsClientSpecificOnly(collection, sysMenu, sysTemplate, userMenu, userTemplate, clientMenu, clientTemplate);
		}

		public void TestHasChanges()
		{
			StmTemplateBase sysTemplate = Factory.New<StmTemplateBase>();
			sysTemplate.SO_Name = "SysTemplate";
			sysTemplate.SO_IsSystemDefined = true;
			sysTemplate.SO_DataContext = nameof(Core.Constants.DataContext.None);

			StmMenuItemBase sysMenu = Factory.New<StmMenuItemBase>();
			sysMenu.SU_MenuName = "SysMenu";
			sysMenu.SU_IsSystemDefined = true;

			StmMenuTemplatePivot sysPivot = Factory.New<StmMenuTemplatePivot>();
			sysPivot.SI_DocumentTitle = "SysPivot";
			sysPivot.SI_SO = sysTemplate.PK;
			sysPivot.SI_SU = sysMenu.PK;

			Factory.Save();

			StmMenuItemBaseCollection collection = new StmMenuItemBaseCollection(Factory);
			collection.Load();

			AssertEquals("HasChanges", false, collection.HasChanges);
			collection[0].Documents[0].Delete();
			AssertEquals("HasChanges", true, collection.HasChanges);
		}

		public void TestIsManagedByDataRefresh()
		{
			StmMenuItemBaseCollection collection = new StmMenuItemBaseCollection(Factory);
			AssertEquals("IsManagedForDataRefresh", true, collection.IsManagedForDataRefresh);

			StmMenuItemBaseCollection collection2 = new StmMenuItemBaseCollection(Factory, new ZQuery());
			AssertEquals("IsManagedForDataRefresh", true, collection2.IsManagedForDataRefresh);
		}

		public void TestLoadNewTemplateFromFile()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var udfWithDefaultsPath = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with defaults.xls", "UDF with defaults.xls");
				var udfWithDefaultsBytes = resourceRetriever.GetBytes("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with defaults.xls");
				var template = Collection.LoadNewTemplateFromFile(udfWithDefaultsPath);
				AssertEquals("SO_DataContext", nameof(Core.Constants.DataContext.None), template.SO_DataContext);
				AssertEquals("SO_Name", "New Template", template.SO_Name);
				AssertEquals("SO_Template", udfWithDefaultsBytes, template.SO_Template);
				AssertType("Should be StmTemplateBase", typeof(StmTemplateBase), template);
			}
		}

		public void TestLoadNewTemplateThatIsEditedByAnotherApplication()
		{
			var tempFileName = Temp.GetTempFileName();
			try
			{
				using (var stream = File.Open(tempFileName, FileMode.Open))
				{
					StmTemplateBase template =
						Collection.LoadNewTemplateFromFile(tempFileName);
				}
			}
			catch (Exception e)
			{
				AssertContains("Please make sure the file is not currently being edited by another application and try again.", e.Message);
			}
			finally
			{
				Factory.Save();
				var loadedBOs = Factory.Load<StmTemplateBase>(new ZQuery());
				AssertEquals("There should be no template generation", 0, loadedBOs.Length);
				File.Delete(tempFileName);
			}
		}

		public void TestGetApplicableMenusFilter()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			Factory.Save();
			StmMenuItemBase menu1 = Factory.New<StmMenuItemBase>();
			StmMenuItemBase menu2 = Factory.New<StmMenuItemBase>();
			StmMenuItemBase menu3 = Factory.New<StmMenuItemBase>();
			StmMenuItemBase menu4 = Factory.New<StmMenuItemBase>();

			menu1.SU_BusinessContext = "a";
			menu2.SU_BusinessContext = "a";
			menu3.SU_BusinessContext = "a";
			menu4.SU_BusinessContext = "b";

			menu1.SU_MenuIndex = 4;
			menu2.SU_MenuIndex = 3;
			menu3.SU_MenuIndex = 2;
			menu4.SU_MenuIndex = 1;

			menu1.SU_MenuPath = "x";
			menu2.SU_MenuPath = "x";
			menu3.SU_MenuPath = "y";
			menu4.SU_MenuPath = "y";

			using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentCompanyPK))
			{
				menu1.SU_GS_NKStaffCode = ZString.Empty;
				menu2.SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
				menu3.SU_GS_NKStaffCode = "xxx";
				menu4.SU_GS_NKStaffCode = ZString.Empty;

				StmMenuItemBaseCollection menus = new StmMenuItemBaseCollection(Factory);

				menus.Load(StmMenuItemBaseCollection.GetApplicableMenusFilterForTesting("a", true));
				AssertEquals("Count", 2, menus.Count);
				AssertEquals("[0]", menu2, menus[0]);
				AssertEquals("[1]", menu1, menus[1]);

				menus.Load(StmMenuItemBaseCollection.GetApplicableMenusFilterForTesting("a", false));
				AssertEquals("Count", 1, menus.Count);
				AssertEquals("[0]", menu1, menus[0]);

				menus.Load(StmMenuItemBaseCollection.GetApplicableMenusFilterForTesting("b", false));
				AssertEquals("Count", 1, menus.Count);
				AssertEquals("[0]", menu4, menus[0]);
			}
		}

		public void TestCreateItemsForVirtualBusinessContexts()
		{
			var realStmMenuItem = Factory.New<StmMenuItemBase>();
			realStmMenuItem.SU_MenuName = "some menu name";
			realStmMenuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			realStmMenuItem.SU_DocumentDirection = nameof(DocumentDirection.DEP);
			realStmMenuItem.SU_FilterList = "BUY=" + GlbStaff.CurrentUser.PK;
			realStmMenuItem.SU_MenuPath = "Import";
			realStmMenuItem.SU_IsSystemDefined = true;
			realStmMenuItem.SU_IsClientSpecific = true;

			StmMenuItemBaseCollection collection = new StmMenuItemBaseCollection(Factory);
			collection.Load(new[] { BusinessContext.Shipment });

			AssertEquals("Collection contains Shipment business context.", collection[0].SU_BusinessContext, nameof(BusinessContext.Shipment));

			collection.Load(new[] { BusinessContext.Shipment, BusinessContext.SubShipment });

			AssertEquals("Collection contains Shipment business context.", collection[0].SU_BusinessContext, nameof(BusinessContext.Shipment));
			AssertEquals("Collection contains SubShipment business context.", collection[1].SU_BusinessContext, nameof(BusinessContext.SubShipment));
			AssertEquals(collection[0].SU_BusinessContext, nameof(BusinessContext.Shipment));
			AssertEquals(collection[1].SU_BusinessContext, nameof(BusinessContext.SubShipment));
			AssertEquals(collection[1].SU_MenuName, realStmMenuItem.SU_MenuName);
			AssertEquals(collection[1].SU_DocumentDirection, realStmMenuItem.SU_DocumentDirection);
			AssertEquals(collection[1].SU_FilterList, realStmMenuItem.SU_FilterList);
			AssertEquals(collection[1].SU_MenuPath, realStmMenuItem.SU_MenuPath);
			AssertEquals(collection[1].SU_IsSystemDefined, realStmMenuItem.SU_IsSystemDefined);
			AssertEquals(collection[1].SU_IsClientSpecific, realStmMenuItem.SU_IsClientSpecific);
		}

		public void TestCreateItemsForVirtualBusinessContexts_WithExcludedTemplatePKs()
		{
			var menuItem = Factory.New<StmMenuItemBase>();
			menuItem.SU_MenuName = "some menu name";
			menuItem.SU_BusinessContext = nameof(BusinessContext.Shipment);
			menuItem.SU_DocumentDirection = nameof(DocumentDirection.DEP);
			menuItem.SU_FilterList = "BUY=" + GlbStaff.CurrentUser.PK;
			menuItem.SU_MenuPath = "Import";
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_IsClientSpecific = true;

			var template = Factory.New<StmTemplate>();
			template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			template.SO_IsSystemDefined = true;
			template.SO_Name = "System Shipment Template";

			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = menuItem.PK;
			pivot.SI_DocumentTitle = "Pub System Ship Doc1 Pivot";

			var collection = new StmMenuItemBaseCollection(Factory);
			collection.Load(new[] { BusinessContext.SubShipment }, new[] { template.PK });

			AssertEquals("Template had been excluded", 0, collection.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
		}

		new StmMenuItemBaseCollection Collection
		{
			get { return (StmMenuItemBaseCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmMenuItemBaseCollection(Factory);
		}

		void CheckForAllowEditingOfSystemDefinedOnly(StmMenuItemBaseCollection collection, StmMenuItemBase sysMenu, StmTemplateBase sysTemplate, StmMenuItemBase userMenu, StmTemplateBase userTemplate, StmMenuItemBase clientMenu, StmTemplateBase clientTemplate)
		{
			AssertEquals("SysMenu ReadOnly", false, collection.FindByPK(sysMenu.PK).SU_MenuNameInfo.ReadOnly);
			AssertEquals("UserMenu ReadOnly", false, collection.FindByPK(userMenu.PK).SU_MenuNameInfo.ReadOnly);
			AssertEquals("ClientMenu ReadOnly", true, collection.FindByPK(clientMenu.PK).SU_MenuNameInfo.ReadOnly);

			AssertEquals("SysMenu CanDelete", true, collection.FindByPK(sysMenu.PK).CanDelete);
			AssertEquals("UserMenu CanDelete", true, collection.FindByPK(userMenu.PK).CanDelete);
			AssertEquals("ClientMenu CanDelete", false, collection.FindByPK(clientMenu.PK).CanDelete);

			AssertEquals("SysMenu IsSystemDefined Column ReadOnly", false, collection.FindByPK(sysMenu.PK).SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("UserMenu IsSystemDefined Column ReadOnly", false, collection.FindByPK(userMenu.PK).SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("ClientMenu IsSystemDefined Column ReadOnly", true, collection.FindByPK(clientMenu.PK).SU_IsSystemDefinedInfo.ReadOnly);

			AssertEquals("SysMenu IsClientSpecific Column ReadOnly", true, collection.FindByPK(sysMenu.PK).SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("UserMenu IsClientSpecific Column ReadOnly", true, collection.FindByPK(userMenu.PK).SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("ClientMenu IsClientSpecific Column ReadOnly", true, collection.FindByPK(clientMenu.PK).SU_IsClientSpecificInfo.ReadOnly);

			AssertEquals("Sys Pivots ReadOnly", false, collection.FindByPK(sysMenu.PK).Documents[0].ReadOnly);
			AssertEquals("User Pivots ReadOnly", false, collection.FindByPK(userMenu.PK).Documents[0].ReadOnly);
			AssertMultilineASCIIEquals("Client Pivots ReadOnly"
				, "SI_IsPasswordProtected\r\nSI_IsPasswordProtectedForOpening\r\nSI_PrintByDefault"
				, string.Join("\r\n", StmMenuTemplatePivotBaseTest.GetNonReadOnlyPropertyNames(collection.FindByPK(clientMenu.PK).Documents[0])));

			AssertEquals("Sys Pivots Collection ReadOnly", false, collection.FindByPK(sysMenu.PK).Documents.ReadOnly);
			AssertEquals("User Pivots Collection ReadOnly", false, collection.FindByPK(userMenu.PK).Documents.ReadOnly);
			AssertEquals("Client Pivots Collection ReadOnly", false, collection.FindByPK(clientMenu.PK).Documents.ReadOnly);
		}

		void CheckForAllowEditingOfIsClientSpecificOnly(StmMenuItemBaseCollection collection, StmMenuItemBase sysMenu, StmTemplateBase sysTemplate, StmMenuItemBase userMenu, StmTemplateBase userTemplate, StmMenuItemBase clientMenu, StmTemplateBase clientTemplate)
		{
			AssertEquals("SysMenu ReadOnly", true, collection.FindByPK(sysMenu.PK).SU_MenuNameInfo.ReadOnly);
			AssertEquals("UserMenu ReadOnly", false, collection.FindByPK(userMenu.PK).SU_MenuNameInfo.ReadOnly);
			AssertEquals("ClientMenu ReadOnly", false, collection.FindByPK(clientMenu.PK).SU_MenuNameInfo.ReadOnly);

			AssertEquals("SysMenu CanDelete", false, collection.FindByPK(sysMenu.PK).CanDelete);
			AssertEquals("UserMenu CanDelete", true, collection.FindByPK(userMenu.PK).CanDelete);
			AssertEquals("ClientMenu CanDelete", true, collection.FindByPK(clientMenu.PK).CanDelete);

			AssertEquals("SysMenu IsSystemDefined Column ReadOnly", true, collection.FindByPK(sysMenu.PK).SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("UserMenu IsSystemDefined Column ReadOnly", true, collection.FindByPK(userMenu.PK).SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("ClientMenu IsSystemDefined Column ReadOnly", true, collection.FindByPK(clientMenu.PK).SU_IsSystemDefinedInfo.ReadOnly);

			AssertEquals("SysMenu IsClientSpecific Column ReadOnly", true, collection.FindByPK(sysMenu.PK).SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("UserMenu IsClientSpecific Column ReadOnly", false, collection.FindByPK(userMenu.PK).SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("ClientMenu IsClientSpecific Column ReadOnly", false, collection.FindByPK(clientMenu.PK).SU_IsClientSpecificInfo.ReadOnly);

			AssertMultilineASCIIEquals("Sys Pivots ReadOnly"
				, "SI_IsPasswordProtected\r\nSI_IsPasswordProtectedForOpening\r\nSI_PrintByDefault"
				, string.Join("\r\n", StmMenuTemplatePivotBaseTest.GetNonReadOnlyPropertyNames(collection.FindByPK(sysMenu.PK).Documents[0])));
			AssertEquals("User Pivots ReadOnly", false, collection.FindByPK(userMenu.PK).Documents[0].ReadOnly);
			AssertEquals("Client Pivots ReadOnly", false, collection.FindByPK(clientMenu.PK).Documents[0].ReadOnly);

			AssertEquals("Sys Pivots Collection ReadOnly", false, collection.FindByPK(sysMenu.PK).Documents.ReadOnly);
			AssertEquals("User Pivots Collection ReadOnly", false, collection.FindByPK(userMenu.PK).Documents.ReadOnly);
			AssertEquals("Client Pivots Collection ReadOnly", false, collection.FindByPK(clientMenu.PK).Documents.ReadOnly);
		}

		#endregion
	}
}
