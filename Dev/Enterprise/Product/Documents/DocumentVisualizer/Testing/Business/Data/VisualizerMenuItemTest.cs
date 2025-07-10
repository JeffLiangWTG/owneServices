using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine;
using Enterprise.DocumentVisualizer.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(VisualizerMenuItem))]
	sealed class VisualizerMenuItemTest : EnterpriseBusinessObjectTestCase
	{
		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		[StressTest]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		public void TestDefaultValues()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();

			AssertEquals(StmMenuItemSchema.SU_MenuType.Name, Enterprise.Core.Constants.StmMenuItemTypes.Forms, menuItem.SU_MenuType);
			AssertEquals(StmMenuItemSchema.SU_PreventAutoDelivery.Name, Enterprise.Core.Constants.StmMenuItemTypes.Forms, menuItem.SU_MenuType);
		}

		public void TestMenuTypeList()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			AssertEquals(Constants.StmMenuItemTypes.Forms, menuItem.MenuTypeList.CodesAsString);
		}

		public void TestIncludedDocumentsList()
		{
			var menuItem = Factory.NewWithValidTestData<VisualizerMenuItem>();

			var pivot = Factory.NewWithValidTestData<VisualizerMenuTemplatePivot>();
			pivot.SI_DocumentTitle = "Test";
			pivot.SI_SU = menuItem.PK;

			Factory.Save();

			var list = menuItem.IncludedDocumentsList;
			Assert(list.ContainsCode(pivot.SI_DocumentTitle));
		}

		public void TestDocuments()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			var document = menuItem.Documents.AddNew();

			AssertNotNull("document", document);
			AssertEquals("document type", typeof(VisualizerMenuTemplatePivot), document.GetType());
		}

		public void TestMenuReadOnlyWhenAnyTemplateCheckedOut()
		{
			var oldIsDeveloperValue = GlbStaff.CurrentUser.GS_IsDeveloper;
			var oldIsSystemAccountValue = GlbStaff.CurrentUser.GS_IsSystemAccount;

			try
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = true;
				GlbStaff.CurrentUser.GS_IsSystemAccount = true;

				var template = Factory.New<VisualizerTemplate>();
				template.SO_Name = "Blah Blah";

				var menuItem = Factory.New<VisualizerMenuItem>();
				menuItem.SU_MenuName = "Bill Of Lading";

				var pivot = Factory.New<VisualizerMenuTemplatePivot>();
				pivot.SI_SO = template.PK;
				pivot.SI_SU = menuItem.PK;
				pivot.SI_DocumentTitle = "ORIGINAL";

				template.IsCheckedOutByMe = false;
				Assert(menuItem.SU_DeliveryRestrictionMacroInfo.ReadOnly);

				template.IsCheckedOutByMe = true;
				Assert(!menuItem.SU_DeliveryRestrictionMacroInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsSystemAccount = oldIsSystemAccountValue;
				GlbStaff.CurrentUser.GS_IsDeveloper = oldIsDeveloperValue;
			}
		}

		public void TestValidation()
		{
			var menuItem = Factory.New<VisualizerMenuItem>();
			AssertEquals(typeof(VisualizerMenuItemValidation), menuItem.Validation.GetType());
		}

		public void TestIsApplicable()
		{
			var airConsol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>() as BusinessObject;
			airConsol[JobConsolSchema.Constants.JK_TransportMode] = "AIR";

			var seaConsol = Factory.New<Enterprise.Integration.Forwarding.IForwardingConsol>() as BusinessObject;
			seaConsol[JobConsolSchema.Constants.JK_TransportMode] = "SEA";

			var nonSupportedBizObj = Factory.New<DummyBusinessObject>();

			var menuItem = Factory.New<VisualizerMenuItem>();
			Assert(airConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(seaConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(!nonSupportedBizObj.IsApplicable(menuItem.SU_FilterList));

			menuItem.SU_FilterList = "JK_TransportMode == \"AIR\"";
			Assert(airConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(!seaConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(!((BusinessObject)null).IsApplicable(menuItem.SU_FilterList));
			Assert(!nonSupportedBizObj.IsApplicable(menuItem.SU_FilterList));

			menuItem.SU_FilterList = "\"<JK_TransportMode>\" == \"AIR\"";
			Assert(airConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(!seaConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(!nonSupportedBizObj.IsApplicable(menuItem.SU_FilterList));

			menuItem.SU_FilterList = "JK_TransportMode == \"SEA\"";
			Assert(!airConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(seaConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(!nonSupportedBizObj.IsApplicable(menuItem.SU_FilterList));

			menuItem.SU_FilterList = "I have no idea what I'm doing";
			Assert(!airConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(!nonSupportedBizObj.IsApplicable(menuItem.SU_FilterList));

			menuItem.SU_FilterList = "1 == 1";
			Assert(airConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(!nonSupportedBizObj.IsApplicable(menuItem.SU_FilterList));

			menuItem.SU_FilterList = "1 + 1";
			Assert(!airConsol.IsApplicable(menuItem.SU_FilterList));
			Assert(!nonSupportedBizObj.IsApplicable(menuItem.SU_FilterList));
		}

		#region ControllerCanEditIsCreditControlled

		public void TestControllerCanEditIsCreditControlled_UDF_NotAllowEditingOfSystemOrClientMenus_Controller_SystemDefined_Developer()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, isDeveloper: true, isController: true, systemDefined: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowAll_Controller_SystemDefined_Developer()
		{
			AssertControllerCanEditIsCreditControlled(isDeveloper: true, isController: true, systemDefined: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowEditingOfClientSpecificOnly_Controller_SystemDefined_Developer()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.AllowEditingOfClientSpecificOnly, isDeveloper: true, isController: true, systemDefined: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowEditingOfSystemDefinedOnly_Controller_SystemDefined_Developer()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.AllowEditingOfSystemDefinedOnly, isDeveloper: true, isController: true, systemDefined: true);
		}

		public void TestControllerCanEditIsCreditControlled_NON_AllowEditingOfSystemDefinedOnly_Controller_SystemDefined_Developer()
		{
			AssertControllerCanEditIsCreditControlled(DeliveryRestrictionType.NON, MenuEditingMode.AllowEditingOfSystemDefinedOnly, isDeveloper: true, isController: true, systemDefined: true);
		}

		public void TestControllerCanEditIsCreditControlled_NON_NotAllowEditingOfSystemOrClientMenus_ClientSpecific_Developer()
		{
			AssertControllerCanEditIsCreditControlled(DeliveryRestrictionType.NON, MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, isDeveloper: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_NotAllowEditingOfSystemOrClientMenus_ClientSpecific_Developer()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, isDeveloper: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowAll_ClientSpecific_Developer()
		{
			AssertControllerCanEditIsCreditControlled(isDeveloper: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowEditingOfClientSpecificOnly_ClientSpecific_Developer()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.AllowEditingOfClientSpecificOnly, isDeveloper: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowEditingOfSystemDefinedOnly_ClientSpecific_Developer()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.AllowEditingOfSystemDefinedOnly, isDeveloper: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_NotAllowEditingOfSystemOrClientMenus_ClientSpecific_AllowOnCreditHold_Developer()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, isDeveloper: true, allowOnCreditHold: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_NotAllowEditingOfSystemOrClientMenus_Controller_SystemDefined_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, isController: true, systemDefined: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowAll_Controller_SystemDefined_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(isController: true, systemDefined: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowEditingOfClientSpecificOnly_Controller_SystemDefined_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.AllowEditingOfClientSpecificOnly, isController: true, systemDefined: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowEditingOfSystemDefinedOnly_Controller_SystemDefined_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.AllowEditingOfSystemDefinedOnly, isController: true, systemDefined: true);
		}

		public void TestControllerCanEditIsCreditControlled_NON_AllowEditingOfSystemDefinedOnly_Controller_SystemDefined_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(DeliveryRestrictionType.NON, MenuEditingMode.AllowEditingOfSystemDefinedOnly, isController: true, systemDefined: true, restrictionMacroReadOnly: true, restrictionDescReadOnly: true);
		}

		public void TestControllerCanEditIsCreditControlled_NON_NotAllowEditingOfSystemOrClientMenus_ClientSpecific_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(DeliveryRestrictionType.NON, MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, restrictionTypeReadOnly: true, restrictionMacroReadOnly: true, restrictionDescReadOnly: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_NotAllowEditingOfSystemOrClientMenus_ClientSpecific_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, restrictionTypeReadOnly: true, restrictionMacroReadOnly: true, restrictionDescReadOnly: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowAll_ClientSpecific_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled();
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowEditingOfClientSpecificOnly_ClientSpecific_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.AllowEditingOfClientSpecificOnly);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_AllowEditingOfSystemDefinedOnly_ClientSpecific_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.AllowEditingOfSystemDefinedOnly, restrictionTypeReadOnly: true, restrictionMacroReadOnly: true, restrictionDescReadOnly: true);
		}

		public void TestControllerCanEditIsCreditControlled_UDF_NotAllowEditingOfSystemOrClientMenus_ClientSpecific_AllowOnCreditHold_NonDeveloper()
		{
			AssertControllerCanEditIsCreditControlled(editMode: MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, allowOnCreditHold: true);
		}

		void AssertControllerCanEditIsCreditControlled(
			DeliveryRestrictionType restrictionType = DeliveryRestrictionType.UDF,
			MenuEditingMode editMode = MenuEditingMode.AllowAll,
			bool isDeveloper = false, bool isController = false, bool allowOnCreditHold = false, bool systemDefined = false,
			bool restrictionTypeReadOnly = false, bool restrictionMacroReadOnly = false, bool restrictionDescReadOnly = false, bool menuIndexReadOnly = false)
		{
			var oldIsControllerValue = GlbStaff.CurrentUser.GS_IsController;
			var oldIsCreditHolderValue = Env.Security.ReceivablesOnCreditHoldController.IsAllowed;
			var oldIsDeveloperValue = GlbStaff.CurrentUser.GS_IsDeveloper;
			var oldIsSystemAccountValue = GlbStaff.CurrentUser.GS_IsSystemAccount;

			try
			{
				GlbStaff.CurrentUser.GS_IsDeveloper = isDeveloper;
				GlbStaff.CurrentUser.GS_IsSystemAccount = true;
				GlbStaff.CurrentUser.GS_IsController = isController;
				Env.Security.ReceivablesOnCreditHoldController.IsAllowed = allowOnCreditHold;

				var template = Factory.New<VisualizerTemplate>();
				template.SO_Name = "Blah Blah";
				template.IsCheckedOutByMe = true;

				var pubSysShipmentMenu = Factory.New<VisualizerMenuItem>();

				var pivot = Factory.New<VisualizerMenuTemplatePivot>();
				pivot.SI_SO = template.PK;
				pivot.SI_SU = pubSysShipmentMenu.PK;
				pivot.SI_DocumentTitle = "ORIGINAL";

				pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
				pubSysShipmentMenu.SU_IsSystemDefined = systemDefined;
				pubSysShipmentMenu.SU_IsClientSpecific = !systemDefined;
				pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
				pubSysShipmentMenu.EditingMode = editMode;
				pubSysShipmentMenu.SU_DeliveryRestrictionMacro = "<Macro>";
				pubSysShipmentMenu.SU_DeliveryRestrictionType = restrictionType.ToString();

				AssertEquals("SU_DeliveryRestrictionTypeInfo.ReadOnly", restrictionTypeReadOnly, pubSysShipmentMenu.SU_DeliveryRestrictionTypeInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionMacroInfo.ReadOnly", restrictionMacroReadOnly, pubSysShipmentMenu.SU_DeliveryRestrictionMacroInfo.ReadOnly);
				AssertEquals("SU_DeliveryRestrictionDescriptionInfo.ReadOnly", restrictionDescReadOnly, pubSysShipmentMenu.SU_DeliveryRestrictionDescriptionInfo.ReadOnly);
				AssertEquals("SU_MenuIndexInfo.ReadOnly", menuIndexReadOnly, pubSysShipmentMenu.SU_MenuIndexInfo.ReadOnly);
				AssertEquals("SU_EmailSubjectLineInfo.ReadOnly", false, pubSysShipmentMenu.SU_EmailSubjectLineInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldIsControllerValue;
				GlbStaff.CurrentUser.GS_IsDeveloper = oldIsDeveloperValue;
				GlbStaff.CurrentUser.GS_IsSystemAccount = oldIsSystemAccountValue;
				Env.Security.ReceivablesOnCreditHoldController.IsAllowed = oldIsCreditHolderValue;
			}
		}

		#endregion
	}
}
