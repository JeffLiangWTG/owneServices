using System;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWise.Windows.UI.Testing;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentVisualizer.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.GUI.Testing
{
	sealed class MenuItemsExtensionsTest : TestCaseWithFactory
	{
		public void TestAddFormsMenuItems()
		{
			TestCaseHelper.ClearTable(StmMenuEDocsSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuDocumentConfigItemSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuDocumentConfigSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuMenuPivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuTemplatePivotSchema.Constants.TableName);
			TestCaseHelper.ClearTable(StmMenuItemSchema.Constants.TableName);

			var menuItem1 = Factory.New<StmMenuItem>();
			menuItem1.SU_MenuName = "Barbeque";
			menuItem1.SU_MenuType = Enterprise.Core.Constants.StmMenuItemTypes.Forms;
			menuItem1.SU_BusinessContext = nameof(BusinessContext.Test);

			var menuItem2 = Factory.New<StmMenuItem>();
			menuItem2.SU_MenuName = "Pierogi";
			menuItem2.SU_MenuType = Enterprise.Core.Constants.StmMenuItemTypes.Forms;
			menuItem2.SU_BusinessContext = nameof(BusinessContext.Test);
			menuItem2.SU_FilterList = "Z0_Description.IsNoneOrEmpty";

			var menuItem3 = Factory.New<StmMenuItem>();
			menuItem3.SU_MenuName = "Ducking";
			menuItem3.SU_MenuType = Enterprise.Core.Constants.StmMenuItemTypes.Forms;
			menuItem3.SU_BusinessContext = nameof(BusinessContext.Test);
			menuItem3.SU_FilterList = "Z0_Description == \"duck lover\"";

			var formsTemplate = Factory.New<StmTemplate>();

			CreatePivot(menuItem1, formsTemplate, "7-11");
			CreatePivot(menuItem2, formsTemplate, "7-11");
			CreatePivot(menuItem3, formsTemplate, "7-11");

			var bizObj = Factory.New<DummyWithUXmlSupport>();
			bizObj.Z0_Description = ZString.Empty;

			Factory.Save();

			var menuItem = new ZMenuItem((NoResString)"Test");

			var isCustomMenuItemApplicable = true;

			menuItem.AddFormsMenuItems(bizObj,
				DummyModuleIDs.Dummy,
				new IMenuItemInfo[]
				{
					new ParentMenuItemInfo
					{
						Name = "Level 1",
						SubMenus = new IMenuItemInfo[]
						{
							new ParentMenuItemInfo
							{
								Name = "Level 2",
								SubMenus = new[]
								{
									new SystemMenuItemInfo
									{
										ID = menuItem1.PK
									}
								}
							},
							new SystemMenuItemInfo
							{
								ID = menuItem2.PK
							}
						}
					},
					new SystemMenuItemInfo
					{
						ID = menuItem3.PK
					},
					new CustomMenuItemInfo
					{
						Name = "Custom form name",
						IsApplicable = _ => isCustomMenuItemApplicable
					}
				});

			menuItem.OnPopup(EventArgs.Empty);

			AssertMultilineASCIIEquals("menu items",
@"Test
   Level 1
      Level 2
         Barbeque
      Pierogi
   Custom form name",
			menuItem.GetVisibleMenuItemsCaptions());

			bizObj.Z0_Description = "duck lover";

			menuItem.OnPopup(EventArgs.Empty);

			AssertMultilineASCIIEquals("menu items",
@"Test
   Level 1
      Level 2
         Barbeque
   Ducking
   Custom form name",
			menuItem.GetVisibleMenuItemsCaptions());

			menuItem1.SU_FilterList = "false";
			isCustomMenuItemApplicable = false;

			Factory.Save();

			bizObj.Z0_Description = "xxx";

			menuItem.OnPopup(EventArgs.Empty);

			AssertMultilineASCIIEquals("menu items",
@"Test
   No Messages Available",
			menuItem.GetVisibleMenuItemsCaptions());
		}

		void CreatePivot(StmMenuItem menuItem, StmTemplate template, string dataStoreName)
		{
			var pivot = Factory.New<StmMenuTemplatePivotBase>();
			pivot.SI_SU = menuItem.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_DocumentTitle = menuItem.SU_MenuName;
			pivot.SI_DataStoreName = dataStoreName;
		}
	}
}
