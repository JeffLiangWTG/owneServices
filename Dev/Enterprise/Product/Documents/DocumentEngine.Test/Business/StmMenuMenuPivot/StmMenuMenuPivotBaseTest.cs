using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.DocumentEngine.Business.Testing
{
	sealed class StmMenuMenuPivotBaseTest : TestCaseWithFactory
	{
		public void TestChildMenuTypeDescription()
		{
			var testMenuDescription = Factory.New<StmMenuMenuPivotBase>();
			var testStmMenuItem = Factory.New<StmMenuItem>();
			testMenuDescription.SF_SU_Outward = testStmMenuItem.PK;

			testMenuDescription.Outward.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			AssertEquals(testMenuDescription.ChildMenuTypeDescription, "Normal Document");
			testMenuDescription.Outward.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
			AssertEquals(testMenuDescription.ChildMenuTypeDescription, "Normal Document");
			testMenuDescription.Outward.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;
			AssertEquals(testMenuDescription.ChildMenuTypeDescription, "Visualizer Form");
		}

		public void TestStmMenuMenuPivotBase()
		{
			var pivot = Factory.New<StmMenuMenuPivotBase>();
			Assert("SF_SU_Inward IsEmpty", pivot.SF_SU_Inward.IsEmpty);
			Assert("SF_SU_Outward IsEmpty", pivot.SF_SU_Outward.IsEmpty);

			var parentMenu = Factory.New<StmMenuItemBase>();
			parentMenu.SU_MenuName = "Parent";
			parentMenu.SU_MenuPath = "fake/path/parent";
			parentMenu.SU_Hint = "Parent hint";

			var childMenu = Factory.New<StmMenuItemBase>();
			childMenu.SU_MenuName = "Child";
			childMenu.SU_MenuPath = "fake/path/child";
			childMenu.SU_Hint = "Child hint";
			childMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);

			pivot.SF_SU_Inward = parentMenu.PK;
			pivot.SF_SU_Outward = childMenu.PK;
			pivot.SF_OverriddenBusinessContext = ZString.Empty;

			AssertEquals("ChildName", "Child", pivot.SF_Calc_ChildName);
			AssertEquals("ChildMenuPath", "fake/path/child", pivot.SF_Calc_ChildMenuPath);
			AssertEquals("ChildHint", "Child hint", pivot.SF_Calc_ChildHint);
			AssertEquals("ChildBusinessContext", "Shipment", pivot.SF_Calc_ChildBusinessContext);
			AssertEquals("ChildMenuType", "DOC", pivot.SF_Calc_ChildMenuType);

			pivot.SF_OverriddenBusinessContext = nameof(BusinessContext.SubShipment);
			AssertEquals("ChildName", "Child", pivot.SF_Calc_ChildName);
			AssertEquals("ChildMenuPath", "fake/path/child", pivot.SF_Calc_ChildMenuPath);
			AssertEquals("ChildHint", "Child hint", pivot.SF_Calc_ChildHint);
			AssertEquals("ChildBusinessContext", "SubShipment", pivot.SF_Calc_ChildBusinessContext);
			AssertEquals("ChildMenuType", "DOC", pivot.SF_Calc_ChildMenuType);
		}

		public void TestCheckSF_Calc_ChildBusinessContextInfoIsNotNull()
		{
			var pivot = Factory.New<StmMenuMenuPivotBase>();

			var parentMenu = Factory.New<StmMenuItemBase>();
			parentMenu.SU_MenuName = "Parent";

			var childMenu = Factory.New<StmMenuItemBase>();
			childMenu.SU_MenuName = "Child";
			childMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);

			pivot.SF_SU_Inward = parentMenu.PK;
			pivot.SF_SU_Outward = childMenu.PK;

			Assert("Outward is Null. Main Purpose of Test failed.", pivot.Outward != null);
			Assert("SU_BusinessContextInfo is Null", pivot.SF_Calc_ChildBusinessContextInfo != null);
		}

		public void TestEditingMode()
		{
			var pivot = Factory.New<StmMenuMenuPivotBase>();
			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, pivot.EditingMode);

			// User Defined
			pivot.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, pivot.ReadOnly);
			AssertEquals("CanDelete", true, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", false, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", false, pivot.SF_IsClientSpecificInfo.ReadOnly);

			pivot.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", false, pivot.ReadOnly);
			AssertEquals("CanDelete", true, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", true, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", false, pivot.SF_IsClientSpecificInfo.ReadOnly);

			pivot.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", false, pivot.ReadOnly);
			AssertEquals("CanDelete", true, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", false, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", true, pivot.SF_IsClientSpecificInfo.ReadOnly);

			pivot.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", false, pivot.ReadOnly);
			AssertEquals("CanDelete", true, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", true, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", true, pivot.SF_IsClientSpecificInfo.ReadOnly);

			// Client specific
			pivot.SF_IsClientSpecific = true;
			pivot.SF_IsSystemDefined = true;

			pivot.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, pivot.ReadOnly);
			AssertEquals("CanDelete", true, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", false, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", false, pivot.SF_IsClientSpecificInfo.ReadOnly);

			pivot.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", false, pivot.ReadOnly);
			AssertEquals("CanDelete", true, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", true, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", false, pivot.SF_IsClientSpecificInfo.ReadOnly);

			pivot.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", true, pivot.ReadOnly);
			AssertEquals("CanDelete", false, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", true, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", true, pivot.SF_IsClientSpecificInfo.ReadOnly);

			pivot.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", true, pivot.ReadOnly);
			AssertEquals("CanDelete", false, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", true, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", true, pivot.SF_IsClientSpecificInfo.ReadOnly);

			// System Defined
			pivot.SF_IsClientSpecific = false;
			pivot.SF_IsSystemDefined = true;

			pivot.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, pivot.ReadOnly);
			AssertEquals("CanDelete", true, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", false, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", false, pivot.SF_IsClientSpecificInfo.ReadOnly);

			pivot.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", true, pivot.ReadOnly);
			AssertEquals("CanDelete", false, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", true, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", true, pivot.SF_IsClientSpecificInfo.ReadOnly);

			pivot.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", false, pivot.ReadOnly);
			AssertEquals("CanDelete", true, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", false, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", true, pivot.SF_IsClientSpecificInfo.ReadOnly);

			pivot.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", true, pivot.ReadOnly);
			AssertEquals("CanDelete", false, pivot.CanDelete);
			AssertEquals("SF_IsSystemDefinedInfo.ReadOnly", true, pivot.SF_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SF_IsClientSpecificInfo.ReadOnly", true, pivot.SF_IsClientSpecificInfo.ReadOnly);
		}

		public void TestSetIsClientSpecific()
		{
			var pivot = Factory.New<StmMenuMenuPivotBase>();

			pivot.SF_IsSystemDefined = true;
			AssertEquals("SF_IsSystemDefined", true, pivot.SF_IsSystemDefined);
			AssertEquals("SF_IsClientSpecific", false, pivot.SF_IsClientSpecific);

			pivot.SF_IsSystemDefined = false;
			AssertEquals("SF_IsSystemDefined", false, pivot.SF_IsSystemDefined);
			AssertEquals("SF_IsClientSpecific", false, pivot.SF_IsClientSpecific);

			pivot.SF_IsClientSpecific = true;
			AssertEquals("SF_IsSystemDefined", true, pivot.SF_IsSystemDefined);
			AssertEquals("SF_IsClientSpecific", true, pivot.SF_IsClientSpecific);

			pivot.SF_IsClientSpecific = false;
			AssertEquals("SF_IsSystemDefined", false, pivot.SF_IsSystemDefined);
			AssertEquals("SF_IsClientSpecific", false, pivot.SF_IsClientSpecific);

			pivot.SF_IsClientSpecific = true;
			AssertEquals("SF_IsSystemDefined", true, pivot.SF_IsSystemDefined);
			AssertEquals("SF_IsClientSpecific", true, pivot.SF_IsClientSpecific);

			pivot.SF_IsSystemDefined = false;
			AssertEquals("SF_IsSystemDefined", false, pivot.SF_IsSystemDefined);
			AssertEquals("SF_IsClientSpecific", false, pivot.SF_IsClientSpecific);

			pivot.SF_IsSystemDefined = true;
			AssertEquals("SF_IsSystemDefined", true, pivot.SF_IsSystemDefined);
			AssertEquals("SF_IsClientSpecific", false, pivot.SF_IsClientSpecific);

			pivot.SF_IsClientSpecific = true;
			AssertEquals("SF_IsSystemDefined", true, pivot.SF_IsSystemDefined);
			AssertEquals("SF_IsClientSpecific", true, pivot.SF_IsClientSpecific);
		}

		public void TestOutwardReturnsStmMenuItemBase()
		{
			var pivot = Factory.New<StmMenuMenuPivotBase>();
			AssertNull("Can't access property Outward using the interface", ((IStmMenuMenuPivotBase)pivot).Outward);

			var outward = Factory.New<StmMenuItem>();
			outward.SU_MenuName = "Wibbly wobbly timey wimey stuff";
			pivot.SF_SU_Outward = outward.PK;

			CombineAssertions(() =>
			{
				AssertType<StmMenuItemBase>(
					"When using IStmMenuMenuPivotBase, can't provide a StmMenuItemBase, which is necessary for Documents property to return a full list of documents",
					((IStmMenuMenuPivotBase)pivot).Outward
				);
				AssertType<StmMenuItemBase>(
					"Can't provide a StmMenuItemBase, which is necessary for Documents property to return a full list of documents",
					pivot.Outward
				);
			});
		}
	}
}
