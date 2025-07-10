using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmMenuItemBase))]
	sealed class StmMenuItemBaseTest : EnterpriseBusinessObjectTestCase
	{
		public void TestControllerCanEditIsPublished()
		{
			ZBool oldIsControllerValue = GlbStaff.CurrentUser.GS_IsController;

			try
			{
				GlbStaff.CurrentUser.GS_IsController = true;

				StmMenuItemBase pubSysShipmentMenu = Factory.New<StmMenuItemBase>();
				pubSysShipmentMenu.SU_BusinessContext = "ShipmentReport";
				pubSysShipmentMenu.SU_IsPublished = true;
				pubSysShipmentMenu.SU_IsSystemDefined = true;
				pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Report";

				pubSysShipmentMenu.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsPublishedInfo.ReadOnly);

				pubSysShipmentMenu.EditingMode = MenuEditingMode.AllowAll;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsPublishedInfo.ReadOnly);

				pubSysShipmentMenu.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsPublishedInfo.ReadOnly);

				pubSysShipmentMenu.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
				AssertEquals("SU_IsModifiableInfo.ReadOnly", ZBool.False, pubSysShipmentMenu.SU_IsModifiableInfo.ReadOnly);

				GlbStaff.CurrentUser.GS_IsController = false;

				StmMenuItemBase pubSysShipmentMenu2 = Factory.New<StmMenuItemBase>();
				pubSysShipmentMenu2.SU_BusinessContext = "CustomsReport";
				pubSysShipmentMenu2.SU_IsPublished = true;
				pubSysShipmentMenu2.SU_IsSystemDefined = true;
				pubSysShipmentMenu2.SU_MenuName = "Pub System Customs Report";

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_IsPublishedInfo.ReadOnly);

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.AllowAll;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_IsPublishedInfo.ReadOnly);

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.True, pubSysShipmentMenu2.SU_IsPublishedInfo.ReadOnly);

				pubSysShipmentMenu2.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
				AssertEquals("IsPublishedInfo.ReadOnly", ZBool.False, pubSysShipmentMenu2.SU_IsPublishedInfo.ReadOnly);
			}
			finally
			{
				GlbStaff.CurrentUser.GS_IsController = oldIsControllerValue;
			}
		}

		public void TestStmMenuItemBase()
		{
			StmMenuItemBase item = Factory.New<StmMenuItemBase>();

			AssertEquals("MenuShortcut", "None", item.SU_MenuShortcut);
			AssertEquals("SU_GS_NKStaffCode", GlbStaff.CurrentUser.GS_Code, item.SU_GS_NKStaffCode);
			AssertEquals("SU_IsPublished", false, item.SU_IsPublished);

			item.SU_IsPublished = true;
			Assert("SU_GS_NKStaffCode IsEmpty", item.SU_GS_NKStaffCode.IsEmpty);

			item.SU_IsPublished = false;
			AssertEquals("SU_IsPublished", false, item.SU_IsPublished);
		}

		public void TestSU_GS_NKStaffCode_IsRefreshedProperly()
		{
			var item = Factory.New<StmMenuItemBase>();

			AssertEquals("SU_IsPublished", false, item.SU_IsPublished);
			AssertEquals("SU_IsSystemDefined", false, item.SU_IsSystemDefined);
			AssertEquals("SU_GS_NKStaffCode", GlbStaff.CurrentUser.GS_Code, item.SU_GS_NKStaffCode);

			item.SU_IsSystemDefined = true;
			Assert("SU_GS_NKStaffCode IsEmpty", item.SU_GS_NKStaffCode.IsEmpty);

			item.SU_IsPublished = true;
			Assert("SU_GS_NKStaffCode IsEmpty", item.SU_GS_NKStaffCode.IsEmpty);

			item.SU_IsPublished = false;
			Assert("SU_GS_NKStaffCode IsEmpty", item.SU_GS_NKStaffCode.IsEmpty);

			item.SU_IsSystemDefined = false;
			AssertEquals("SU_GS_NKStaffCode", GlbStaff.CurrentUser.GS_Code, item.SU_GS_NKStaffCode);
		}

		public void TestSU_ContactTypeSetsPreventAutoDelivery()
		{
			StmMenuItemBase item = Factory.New<StmMenuItemBase>();
			item.SU_ContactType = "NCT";
			Assert("Error expected if SU_PreventAutoDeliver is not Checked (true)", item.SU_PreventAutoDelivery);
		}

		public void TestSU_ContactTypeAffectsPreventAutoDeliveryReadOnly()
		{
			StmMenuItemBase item = Factory.New<StmMenuItemBase>();
			item.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			item.SU_IsSystemDefined = true;
			item.SU_IsPublished = true;

			item.SU_ContactType = "NCT";
			AssertEquals(true, item.SU_PreventAutoDeliveryInfo.ReadOnly);
			item.SU_ContactType = "CNE";
			AssertEquals(false, item.SU_PreventAutoDeliveryInfo.ReadOnly);
		}

		public void TestSU_Calc_IsWebSupportable()
		{
			StmMenuItemBase item = Factory.New<StmMenuItemBase>();
			AssertEquals("Should be False by default", false, item.SU_Calc_IsWebSupportable);

			item.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			AssertEquals("Should be False", false, item.SU_Calc_IsWebSupportable);

			item.SU_MenuType = Core.Constants.StmMenuItemTypes.OperationalActions;
			AssertEquals("Should be False", false, item.SU_Calc_IsWebSupportable);

			item.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;
			AssertEquals("Should be True", true, item.SU_Calc_IsWebSupportable);
		}

		public void TestSU_IsZippedDocPack()
		{
			var item = Factory.New<StmMenuItemBase>();
			Assert("Should be False by default", !item.SU_IsZippedDocPack);

			item.SU_IsDocPack = true;
			item.SU_IsZippedDocPack = true;
			Assert(item.SU_IsZippedDocPack);
		}

		public void TestSU_IsZippedDocPack_ReadOnly()
		{
			var item = Factory.New<StmMenuItemBase>();
			item.SU_IsDocPack = false;
			Assert(item.SU_IsZippedDocPackInfo.ReadOnly);
			item.SU_IsDocPack = true;
			Assert(!item.SU_IsZippedDocPackInfo.ReadOnly);
		}

		public void TestSU_IsDocPack_Refreshes_SU_IsZippedDocPack()
		{
			var item = Factory.New<StmMenuItemBase>();
			item.SU_IsDocPack = true;
			item.SU_IsZippedDocPack = true;
			Assert(item.SU_IsZippedDocPack);

			item.SU_IsDocPack = false;
			Assert("Setting SU_IsDocPack to false should set SU_IsZippedDocPack to false", !item.SU_IsZippedDocPack);

			item.SU_IsDocPack = true;
			Assert("Setting SU_IsDocPack to true should not refresh SU_IsZippedDocPack", !item.SU_IsZippedDocPack);
		}

		public void TestDeletingMenuWillDeleteAssociatedPivots()
		{
			StmTemplate sysShipmentTemplate = Factory.New<StmTemplate>();
			sysShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";

			StmMenuItemBase pubSysShipmentMenu1 = Factory.New<StmMenuItemBase>();
			pubSysShipmentMenu1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu1.SU_IsPublished = true;
			pubSysShipmentMenu1.SU_IsSystemDefined = true;
			pubSysShipmentMenu1.SU_MenuName = "Pub System Shipment Document1";

			StmMenuTemplatePivotBase pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu1.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Pub System Ship Doc1 Pivot";

			RefDocType docType = Factory.LoadTop1<RefDocType>(new ZQuery());
			StmMenuEDocs eDocMenu = Factory.New<StmMenuEDocs>();
			eDocMenu.SX_SU = pubSysShipmentMenu1.PK;
			eDocMenu.SX_RT_DocType = docType.PK;

			pubSysShipmentMenu1.Delete();
			AssertEquals("Pivot.IsDeleted", true, pubSysShipmentPivot1.IsDeleted);
			AssertEquals("Template.IsDeleted", false, sysShipmentTemplate.IsDeleted);
			Assert("eDocsMenu should be deleted", eDocMenu.IsDeleted);
		}

		public void TestDocumentDirection()
		{
			StmMenuItemBase menu = Factory.New<StmMenuItemBase>();
			menu.RunPreSaveValidation();

			AssertEquals("DocumentDirection", nameof(DocumentDirection.ANY), menu.SU_DocumentDirection);
			AssertEquals("HasErrors", false, menu.SU_DocumentDirectionInfo.HasErrors());

			menu.SU_DocumentDirection = "";
			menu.RunPreSaveValidation();

			AssertEquals("HasErrors", true, menu.SU_DocumentDirectionInfo.HasErrors());
			AssertEquals("Error Message", "You need to select a document direction from the list.", menu.SU_DocumentDirectionInfo.GetErrors().GetFirstMessage());

			menu.SU_DocumentDirection = "abc";
			menu.RunPreSaveValidation();

			AssertEquals("HasErrors", true, menu.SU_DocumentDirectionInfo.HasErrors());
			AssertEquals("Error Message", "You have entered an invalid code for document direction. Please select one from the list.", menu.SU_DocumentDirectionInfo.GetErrors().GetFirstMessage());

			menu.SU_DocumentDirection = menu.DocumentDirectionList[0].Code;
			AssertEquals("HasErrors", false, menu.SU_DocumentDirectionInfo.HasErrors());
		}

		public void TestSignBy()
		{
			StmMenuItemBase menu = Factory.New<StmMenuItemBase>();
			menu.RunPreSaveValidation();
			Factory.Save();

			AssertEquals("SignBy", DocumentsSignBy.NON, menu.SU_SignBy);
			AssertEquals("HasErrors", false, menu.SU_SignByInfo.HasErrors());

			menu.SU_SignBy = "";
			menu.RunPreSaveValidation();

			AssertEquals("HasErrors", true, menu.SU_SignByInfo.HasErrors());
			AssertEquals("Error Message", "Please enter a Sign By.", menu.SU_SignByInfo.GetErrors().GetFirstMessage());

			menu.SU_SignBy = "abc";
			menu.RunPreSaveValidation();

			AssertEquals("HasErrors", true, menu.SU_SignByInfo.HasErrors());
			AssertEquals("Error Message", "Enter a valid selection.", menu.SU_SignByInfo.GetErrors().GetFirstMessage());

			menu.SU_SignBy = menu.SignByList[0].Code;
			AssertEquals("HasErrors", false, menu.SU_SignByInfo.HasErrors());

			using (DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				menu.SU_SignBy = DocumentsSignBy.DOS;
				AssertEquals("HasErrors", false, menu.SU_SignByInfo.HasErrors());
				Factory.Save();
			}

			using (DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				menu.SU_SignBy = DocumentsSignBy.DOS;
				AssertEquals("HasErrors", false, menu.SU_SignByInfo.HasErrors());
			}

			using (DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				menu.SU_SignBy = DocumentsSignBy.NON;
				Factory.Save();
				menu.SU_SignBy = DocumentsSignBy.DOS;
				AssertEquals("HasErrors", true, menu.SU_SignByInfo.HasErrors());
			}
		}

		public void TestSignBy_ModifyDOSWhenServiceNotEnabled()
		{
			var menu = Factory.New<StmMenuItemBase>();
			menu.RunPreSaveValidation();
			using (DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				menu.SU_SignBy = DocumentsSignBy.DOS;
				Factory.Save();
			}

			using (DocumentsDataRegistry.Instance.EnableDocumentSigningService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				menu.SU_SignBy = DocumentsSignBy.NON;
				menu.RunPreSaveValidation();

				AssertEquals("HasErrors", true, menu.SU_SignByInfo.HasErrors());
				AssertEquals("Error Message", "Modifying the DOS option while logged into this Company is not supported.", menu.SU_SignByInfo.GetErrors().GetFirstMessage());

				menu.SU_SignBy = DocumentsSignBy.PFX;
				menu.RunPreSaveValidation();

				AssertEquals("HasErrors", true, menu.SU_SignByInfo.HasErrors());
				AssertEquals("Error Message", "Modifying the DOS option while logged into this Company is not supported.", menu.SU_SignByInfo.GetErrors().GetFirstMessage());
			}
		}

		public void TestContactType()
		{
			StmMenuItemBase menu = Factory.New<StmMenuItemBase>();
			menu.RunPreSaveValidation();

			AssertEquals("ContactType", ContactType.NoContactType.Code, menu.SU_ContactType);
			AssertEquals("HasErrors", false, menu.SU_ContactTypeInfo.HasErrors());

			menu.SU_ContactType = "";
			menu.RunPreSaveValidation();

			AssertEquals("HasErrors", true, menu.SU_ContactTypeInfo.HasErrors());
			AssertEquals("Error Message", "You need to select a document group from the list.", menu.SU_ContactTypeInfo.GetErrors().GetFirstMessage());

			menu.SU_ContactType = "abc";
			menu.RunPreSaveValidation();

			AssertEquals("HasErrors", true, menu.SU_ContactTypeInfo.HasErrors());
			AssertEquals("Error Message", "You have entered an invalid code for document group. Please select one from the list.", menu.SU_ContactTypeInfo.GetErrors().GetFirstMessage());

			menu.SU_ContactType = menu.ContactTypeList[0].Code;
			AssertEquals("HasErrors", false, menu.SU_ContactTypeInfo.HasErrors());
		}

		public void TestEditingModeIsPassedToPivots()
		{
			StmMenuItemBase menu = Factory.New<StmMenuItemBase>();

			menu.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("Documents.EditingMode", menu.EditingMode, menu.Documents.EditingMode);

			menu.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("Documents.EditingMode", menu.EditingMode, menu.Documents.EditingMode);

			menu.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("Documents.EditingMode", menu.EditingMode, menu.Documents.EditingMode);
		}

		public void TestEditingMode()
		{
			StmMenuItemBase menu = Factory.New<StmMenuItemBase>();

			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, menu.EditingMode);

			// User Defined
			menu.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, menu.ReadOnly);
			AssertEquals("CanDelete", true, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", false, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", false, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", false, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			menu.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", false, menu.ReadOnly);
			AssertEquals("CanDelete", true, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", true, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", false, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", false, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			menu.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", false, menu.ReadOnly);
			AssertEquals("CanDelete", true, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", false, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", true, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", false, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			menu.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", false, menu.ReadOnly);
			AssertEquals("CanDelete", true, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", true, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", true, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", false, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			// Client specific
			menu.SU_IsClientSpecific = true;
			menu.SU_IsSystemDefined = true;

			menu.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, menu.ReadOnly);
			AssertEquals("CanDelete", true, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", false, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", false, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", false, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			menu.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", false, menu.ReadOnly);
			AssertEquals("CanDelete", true, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", true, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", false, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", false, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			menu.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", true, menu.SU_MenuNameInfo.ReadOnly);
			AssertEquals("CanDelete", false, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", true, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", true, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", true, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			menu.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", true, menu.SU_MenuNameInfo.ReadOnly);
			AssertEquals("CanDelete", false, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", true, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", true, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", true, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			// System Defined
			menu.SU_IsClientSpecific = false;
			menu.SU_IsSystemDefined = true;

			menu.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("ReadOnly", false, menu.SU_MenuNameInfo.ReadOnly);
			AssertEquals("CanDelete", true, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", false, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", false, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", false, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			menu.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("ReadOnly", true, menu.SU_MenuNameInfo.ReadOnly);
			AssertEquals("CanDelete", false, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", true, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", true, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", true, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			menu.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("ReadOnly", false, menu.SU_MenuNameInfo.ReadOnly);
			AssertEquals("CanDelete", true, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", false, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", true, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", false, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);

			menu.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("ReadOnly", true, menu.SU_MenuNameInfo.ReadOnly);
			AssertEquals("CanDelete", false, menu.CanDelete);
			AssertEquals("SU_IsSystemDefinedInfo.ReadOnly", true, menu.SU_IsSystemDefinedInfo.ReadOnly);
			AssertEquals("SU_MustRunOnlineInfo.ReadOnly", false, menu.SU_MustRunOnlineInfo.ReadOnly);
			AssertEquals("SU_SignByInfo.ReadOnly", false, menu.SU_SignByInfo.ReadOnly);
			AssertEquals("SU_IsClientSpecificInfo.ReadOnly", true, menu.SU_IsClientSpecificInfo.ReadOnly);
			AssertEquals("SU_Calc_IsWebSupportableInfo.ReadOnly", true, menu.SU_Calc_IsWebSupportableInfo.ReadOnly);
			AssertEquals("SU_IsVisibleOnWebInfo.ReadOnly", false, menu.SU_IsVisibleOnWebInfo.ReadOnly);
		}

		public void TestSetIsClientSpecific()
		{
			StmMenuItemBase menu = Factory.New<StmMenuItemBase>();

			menu.SU_IsSystemDefined = true;
			AssertEquals("SU_IsSystemDefined", true, menu.SU_IsSystemDefined);
			AssertEquals("SU_IsClientSpecific", false, menu.SU_IsClientSpecific);

			menu.SU_IsSystemDefined = false;
			AssertEquals("SU_IsSystemDefined", false, menu.SU_IsSystemDefined);
			AssertEquals("SU_IsClientSpecific", false, menu.SU_IsClientSpecific);

			menu.SU_IsClientSpecific = true;
			AssertEquals("SU_IsSystemDefined", true, menu.SU_IsSystemDefined);
			AssertEquals("SU_IsClientSpecific", true, menu.SU_IsClientSpecific);

			menu.SU_IsClientSpecific = false;
			AssertEquals("SU_IsSystemDefined", false, menu.SU_IsSystemDefined);
			AssertEquals("SU_IsClientSpecific", false, menu.SU_IsClientSpecific);

			menu.SU_IsClientSpecific = true;
			AssertEquals("SU_IsSystemDefined", true, menu.SU_IsSystemDefined);
			AssertEquals("SU_IsClientSpecific", true, menu.SU_IsClientSpecific);

			menu.SU_IsSystemDefined = false;
			AssertEquals("SU_IsSystemDefined", false, menu.SU_IsSystemDefined);
			AssertEquals("SU_IsClientSpecific", false, menu.SU_IsClientSpecific);

			menu.SU_IsSystemDefined = true;
			AssertEquals("SU_IsSystemDefined", true, menu.SU_IsSystemDefined);
			AssertEquals("SU_IsClientSpecific", false, menu.SU_IsClientSpecific);

			menu.SU_IsClientSpecific = true;
			AssertEquals("SU_IsSystemDefined", true, menu.SU_IsSystemDefined);
			AssertEquals("SU_IsClientSpecific", true, menu.SU_IsClientSpecific);
		}

		public void TestSortOrder()
		{
			StmTemplate sysShipmentTemplate = Factory.New<StmTemplate>();
			sysShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";

			StmMenuItemBase pubSysShipmentMenu1 = Factory.New<StmMenuItemBase>();
			pubSysShipmentMenu1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu1.SU_IsPublished = true;
			pubSysShipmentMenu1.SU_IsSystemDefined = true;
			pubSysShipmentMenu1.SU_MenuName = "Pub System Shipment Document1";

			StmMenuTemplatePivotBase pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu1.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "3Pub System Ship Doc1 Pivot";
			pubSysShipmentPivot1.SI_Index = 1;

			StmMenuTemplatePivotBase pubSysShipmentPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot2.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot2.SI_SU = pubSysShipmentMenu1.PK;
			pubSysShipmentPivot2.SI_DocumentTitle = "2Pub System Ship Doc1 Pivot";
			pubSysShipmentPivot2.SI_Index = 2;

			StmMenuTemplatePivotBase pubSysShipmentPivot3 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot3.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot3.SI_SU = pubSysShipmentMenu1.PK;
			pubSysShipmentPivot3.SI_DocumentTitle = "1Pub System Ship Doc1 Pivot";
			pubSysShipmentPivot3.SI_Index = 3;

			AssertEquals("Documents.Count", 3, pubSysShipmentMenu1.Documents.Count);
			AssertEquals("DocumentTitle 1", pubSysShipmentPivot1.SI_DocumentTitle, pubSysShipmentMenu1.Documents[0].SI_DocumentTitle);
			AssertEquals("DocumentTitle 2", pubSysShipmentPivot2.SI_DocumentTitle, pubSysShipmentMenu1.Documents[1].SI_DocumentTitle);
			AssertEquals("DocumentTitle 3", pubSysShipmentPivot3.SI_DocumentTitle, pubSysShipmentMenu1.Documents[2].SI_DocumentTitle);
		}

		public void TestChildMenus()
		{
			StmMenuItemBase childMenu = Factory.New<StmMenuItemBase>();
			childMenu.SU_MenuName = "Child";

			StmMenuItemBase parentMenu = Factory.New<StmMenuItemBase>();
			parentMenu.SU_MenuName = "Parent";
			AssertEquals("ChildMenus.Count", 0, parentMenu.ChildMenus.Count);

			var childPivot = parentMenu.ChildMenus.AddNew();
			childPivot.SF_SU_Inward = parentMenu.PK;
			childPivot.SF_SU_Outward = childMenu.PK;
			AssertEquals("ChildMenus.Count", 1, parentMenu.ChildMenus.Count);
			AssertEquals("ChildName", "Child", parentMenu.ChildMenus[0].SF_Calc_ChildName);

			Factory.Save();

			AssertEquals("Parent HasChanges", false, parentMenu.HasChanges);
			AssertEquals("Pivot HasChanges", false, childPivot.HasChanges);
			AssertEquals("Child HasChanges", false, childMenu.HasChanges);

			childPivot.SF_Index = 10;

			AssertEquals("Parent HasChanges", true, parentMenu.HasChanges);
			AssertEquals("Pivot HasChanges", true, childPivot.HasChanges);
			AssertEquals("Child HasChanges", false, childMenu.HasChanges);

			Factory.Save();

			AssertEquals("Parent HasChanges", false, parentMenu.HasChanges);
			AssertEquals("Pivot HasChanges", false, childPivot.HasChanges);
			AssertEquals("Child HasChanges", false, childMenu.HasChanges);

			childPivot.Delete();
			AssertEquals("ChildMenus.Count", 0, parentMenu.ChildMenus.Count);
			AssertEquals("ChildMenu.IsDeleted", false, childMenu.IsDeleted);
			AssertEquals("ChildPivot.IsDeleted", true, childPivot.IsDeleted);

			AssertEquals("Parent HasChanges", true, parentMenu.HasChanges);
		}

		public void TestDeleteChildWhenThereAreMenuPivots()
		{
			StmMenuItemBase parentMenuPack = Factory.New<StmMenuItemBase>();
			parentMenuPack.SU_MenuName = "Parent";

			StmMenuItemBase childMenu = Factory.New<StmMenuItemBase>();
			childMenu.SU_MenuName = "Child";

			var pivot = parentMenuPack.ChildMenus.AddNew();
			pivot.SF_SU_Inward = parentMenuPack.PK;
			pivot.SF_SU_Outward = childMenu.PK;

			Factory.Save();

			childMenu.Delete();
			Factory.Save();
			AssertEquals("Pivot IsDeleted", true, pivot.IsDeleted);
			AssertEquals("Parent IsDeleted", false, parentMenuPack.IsDeleted);
		}

		public void TestDeleteParentWhenThereAreMenuPivots()
		{
			StmMenuItemBase parentMenuPack = Factory.New<StmMenuItemBase>();
			parentMenuPack.SU_MenuName = "Parent";

			StmMenuItemBase childMenu = Factory.New<StmMenuItemBase>();
			childMenu.SU_MenuName = "Child";

			var pivot = parentMenuPack.ChildMenus.AddNew();
			pivot.SF_SU_Inward = parentMenuPack.PK;
			pivot.SF_SU_Outward = childMenu.PK;

			Factory.Save();

			parentMenuPack.Delete();
			Factory.Save();
			AssertEquals("Pivot IsDeleted", true, pivot.IsDeleted);
			AssertEquals("Child IsDeleted", false, childMenu.IsDeleted);
		}

		public void TestDeleteParentWhenThereAreOverrides()
		{
			var parentDoc = Factory.NewWithValidTestData<StmMenuItemBase>();
			parentDoc.SU_MenuName = "Parent";

			var overrideItem = Factory.NewWithValidTestData<VisualizerNote>();
			overrideItem.DD_SU = parentDoc.PK;
			overrideItem.DD_ParentTableCode = "JS";

			Factory.Save();

			parentDoc.Delete();
			AssertNoExceptionThrown("The override doc item is not being deleted", () => Factory.Save());
			AssertEquals("Override IsDeleted", true, overrideItem.IsDeleted);
			AssertEquals("Parent IsDeleted", true, parentDoc.IsDeleted);
		}

		public void TestDeleteRelatedGlbSecurity()
		{
			var stmMenuItemBase = Factory.NewWithValidTestData<StmMenuItemBase>();
			var glbSecurity = Factory.NewWithValidTestData<GlbSecurity>();
			glbSecurity.GU_ItemGUID = stmMenuItemBase.PK;

			Factory.Save();

			stmMenuItemBase.Delete();
			AssertEquals("glbSecurity IsDeleted", true, glbSecurity.IsDeleted);
		}

		#region TestDeleteDocumentMenu

		public void TestDeleteDocumentMenuWhenThereAreOrgDocumentsReferencingIt()
		{
			StmMenuItemBase parentMenuPack = Factory.New<StmMenuItemBase>();
			parentMenuPack.SU_MenuName = "TestDocumentMenu";

			CreateOrgDocument("testOrg1", "testOrgContact1", parentMenuPack.PK);
			CreateOrgDocument("testOrg2", "testOrgContact2", parentMenuPack.PK);
			CreateOrgDocument("testOrg3", "testOrgContact3", parentMenuPack.PK);

			Factory.Save();

			try
			{
				parentMenuPack.Delete();
			}
			catch (Exception ex)
			{
				Assert("Should throw CannotDeleteException.", ex is CannotDeleteException);
				AssertContains("The Menu Item 'TestDocumentMenu' is used in the Organization 'testOrg1' >> Contact 'testOrgContact1' >> Documents To Receive", ex.Message);
				AssertContains("The Menu Item 'TestDocumentMenu' is used in the Organization 'testOrg2' >> Contact 'testOrgContact2' >> Documents To Receive", ex.Message);
				AssertContains("The Menu Item 'TestDocumentMenu' is used in the Organization 'testOrg3' >> Contact 'testOrgContact3' >> Documents To Receive", ex.Message);
			}
		}

		void CreateOrgDocument(string orgCode, string contactName, ZGuid docMenuId)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = orgCode;

			var orgContact = Factory.NewWithValidTestData<OrgContact>();
			orgContact.OC_OH = org.PK;
			orgContact.OC_ContactName = contactName;

			var orgDocument = Factory.NewWithValidTestData<OrgDocument>();
			orgDocument.OD_SU_MenuItem = docMenuId;
			orgDocument.OD_OC = orgContact.PK;
		}

		public void TestDeleteDocumentMenuWhenThereAreProcessTasksReferencingIt()
		{
			_ = DummyWorkflowDescriptor.Instance;
			StmMenuItemBase parentMenuPack = Factory.New<StmMenuItemBase>();
			parentMenuPack.SU_MenuName = "TestDocMenu";

			var dummyBusinessObject = Factory.NewWithValidTestData<DummyBusinessObject>();
			var workflowTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			workflowTemplate.P0_Name = "testWorkflowTemplate";
			workflowTemplate.P0_Description = "testDesc";

			var processTask1 = CreateProcessTask("TestWorkflowTemplateMilestone", "MIL", workflowTemplate.PK, "P0");
			var processTask2 = CreateProcessTask("TestWorkflowTemplateTrigger", "TRG", workflowTemplate.PK, "P0");
			var processTask3 = CreateProcessTask("TestJobMilestone", "MIL", dummyBusinessObject.PK, "Z0");
			var processTask4 = CreateProcessTask("TestJobTrigger", "TRG", dummyBusinessObject.PK, "Z0");

			CreateProcessTaskNotification(parentMenuPack.PK, processTask1.PK);
			CreateProcessTaskNotification(parentMenuPack.PK, processTask2.PK);
			CreateProcessTaskNotification(parentMenuPack.PK, processTask3.PK);
			CreateProcessTaskNotification(parentMenuPack.PK, processTask4.PK);

			var templateTrigger = (BusinessObject)Factory.New<IUniversalTemplateTrigger>();
			templateTrigger[ProcessTemplateTriggerSchema.P9T_P0_Template] = workflowTemplate.PK;
			templateTrigger[ProcessTemplateTriggerSchema.P9T_Description] = "TestUniversal";
			templateTrigger[ProcessTemplateTriggerSchema.P9T_Sequence] = 1;
			templateTrigger[ProcessTemplateTriggerSchema.P9T_SE_NKTriggerEvent] = "ACD";
			templateTrigger[ProcessTemplateTriggerSchema.P9T_SystemCreateTimeUtc] = DateTime.UtcNow;
			templateTrigger[ProcessTemplateTriggerSchema.P9T_SystemLastEditTimeUtc] = DateTime.UtcNow;

			var processTaskNotification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTaskNotification.PQ_SU_Document = parentMenuPack.PK;
			processTaskNotification.PQ_P9 = ZGuid.Empty;
			processTaskNotification.PQ_P9T_Trigger = templateTrigger.PK;

			Factory.Save();

			try
			{
				parentMenuPack.Delete();
			}
			catch (Exception ex)
			{
				Assert("Should throw CannotDeleteException.", ex is CannotDeleteException);
				AssertContains("The Menu Item 'TestDocMenu' is used in 'Milestone Action - TestWorkflowTemplateMilestone' of Workflow Template - testWorkflowTemplate", ex.Message);
				AssertContains("The Menu Item 'TestDocMenu' is used in 'Trigger Action - TestWorkflowTemplateTrigger' of Workflow Template - testWorkflowTemplate", ex.Message);
				AssertContains("The Menu Item 'TestDocMenu' is used in 'Universal Trigger Action - TestUniversal' of Workflow Template - testWorkflowTemplate", ex.Message);
				AssertContains("The Menu Item 'TestDocMenu' is used in 'Milestone Action - TestJobMilestone' of Dummy Business Object Default", ex.Message);
				AssertContains("The Menu Item 'TestDocMenu' is used in 'Trigger Action - TestJobTrigger' of Dummy Business Object Default", ex.Message);
			}
		}

		ProcessTaskNotification CreateProcessTaskNotification(ZGuid documetnMenuPK, ZGuid parentId)
		{
			var processTaskNotification = Factory.NewWithValidTestData<ProcessTaskNotification>();
			processTaskNotification.PQ_SU_Document = documetnMenuPK;
			processTaskNotification.PQ_P9 = parentId;
			return processTaskNotification;
		}

		ProcessTask CreateProcessTask(string taskDesc, string taskType, ZGuid parentId, string parentTableCode)
		{
			var processTask = Factory.NewWithValidTestData<ProcessTask>();
			processTask.P9_Description = taskDesc;
			processTask.P9_Type = taskType;
			processTask.P9_ParentID = parentId;
			processTask.P9_ParentTableCode = parentTableCode;

			return processTask;
		}

		#endregion

		public void TestNonPublishedSystemDocumentsAreNotStoredAgainstAnyUser()
		{
			StmMenuItemBase pubSysShipmentMenu = Factory.New<StmMenuItemBase>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			pubSysShipmentMenu.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;

			AssertEquals("SU_GS_NKStaffCode.IsEmpty", true, pubSysShipmentMenu.SU_GS_NKStaffCode.IsEmpty);
			pubSysShipmentMenu.SU_IsPublished = false;
			AssertEquals("SU_GS_NKStaffCode.IsEmpty", true, pubSysShipmentMenu.SU_GS_NKStaffCode.IsEmpty);

			pubSysShipmentMenu.SU_IsSystemDefined = false;
			pubSysShipmentMenu.SU_IsPublished = true;

			AssertEquals("SU_GS_NKStaffCode.IsEmpty", true, pubSysShipmentMenu.SU_GS_NKStaffCode.IsEmpty);
			pubSysShipmentMenu.SU_IsPublished = false;
			AssertEquals("SU_GS_NKStaffCode", GlbStaff.CurrentUser.GS_Code, pubSysShipmentMenu.SU_GS_NKStaffCode);
		}

		public void TestEDocs()
		{
			StmMenuItemBase pubSysShipmentMenu = Factory.New<StmMenuItemBase>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";

			RefDocType docType = Factory.LoadTop1<RefDocType>(new ZQuery());
			StmMenuEDocs eDoc = Factory.New<StmMenuEDocs>();
			eDoc.SX_SU = pubSysShipmentMenu.PK;
			eDoc.SX_RT_DocType = docType.PK;

			AssertEquals("MenuItem eDocs count should be 1", 1, pubSysShipmentMenu.EDocs.Count);
		}

		public void TestEDocsViewHasCorrectSorting()
		{
			var menu = Factory.NewWithValidTestData<StmMenuItemBase>();
			menu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			menu.SU_IsPublished = true;
			menu.SU_IsSystemDefined = false;
			menu.SU_MenuName = "MenuName";

			var docType1 = Factory.NewWithValidTestData<RefDocType>();
			docType1.RT_DocType = "AAA";
			docType1.RT_Desc = "AAA Desc";

			var docType2 = Factory.NewWithValidTestData<RefDocType>();
			docType2.RT_DocType = "AAA";
			docType2.RT_Desc = "AAA Desc";

			var eDoc1 = Factory.NewWithValidTestData<StmMenuEDocs>();
			eDoc1.SX_SU = menu.PK;
			eDoc1.SX_RT_DocType = docType1.PK;
			eDoc1.SX_Index = 1;

			var eDoc2 = Factory.NewWithValidTestData<StmMenuEDocs>();
			eDoc2.SX_SU = menu.PK;
			eDoc2.SX_RT_DocType = docType2.PK;
			eDoc2.SX_Index = 2;

			AssertEquals(eDoc1.PK, menu.EDocsView[0].PK);
			AssertEquals(eDoc2.PK, menu.EDocsView[1].PK);

			eDoc1.SX_Index = 2;
			eDoc2.SX_Index = 1;

			AssertEquals(eDoc2.PK, menu.EDocsView[0].PK);
			AssertEquals(eDoc1.PK, menu.EDocsView[1].PK);
		}

		public void TestIncludedDocumentsList_ShouldGetAllPivots()
		{
			var menuItem = Factory.New<StmMenuItemBase>();
			menuItem.SU_IsDocPack = true;

			var templatePivot1 = CreateChildTemplateAndPivot(menuItem);
			templatePivot1.SI_DocumentTitle = "Document, the First";
			templatePivot1.SI_Index = 1;
			var templatePivot2 = CreateChildTemplateAndPivot(menuItem);
			templatePivot2.SI_DocumentTitle = "Document, the Second";
			templatePivot2.SI_Index = 2;
			var menuPivot1 = CreateChildMenuAndPivot(menuItem);
			menuPivot1.Outward.SU_MenuName = "Document, the Third";
			menuPivot1.SF_Index = 3;
			var menuPivot2 = CreateChildMenuAndPivot(menuItem);
			menuPivot2.Outward.SU_MenuName = "Document, the Fourth";
			menuPivot2.SF_Index = 4;

			var includedDocuments = menuItem.IncludedDocumentsList;
			AssertEquals(4, includedDocuments.Count);
			AssertEquals("Document, the First", includedDocuments[0].Code);
			AssertEquals("Document, the Second", includedDocuments[1].Code);
			AssertEquals("Document, the Third", includedDocuments[2].Code);
			AssertEquals("Document, the Fourth", includedDocuments[3].Code);
			AssertEquals("Template index: 1", includedDocuments[0].Description);
			AssertEquals("Template index: 2", includedDocuments[1].Description);
			AssertEquals("Document index: 3", includedDocuments[2].Description);
			AssertEquals("Document index: 4", includedDocuments[3].Description);
		}

		public void TestIncludedDocumentsListForANonDocPack_ShouldGetNoPivots()
		{
			var menuItem = Factory.New<StmMenuItemBase>();
			CreateChildMenuAndPivot(menuItem);
			CreateChildMenuAndPivot(menuItem);

			AssertEquals(0, menuItem.IncludedDocumentsList.Count);
		}

		StmMenuMenuPivot CreateChildMenuAndPivot(StmMenuItem parent)
		{
			var childMenu = Factory.New<StmMenuItemBase>();
			var pivot = Factory.New<StmMenuMenuPivot>();
			pivot.SF_SU_Inward = parent.PK;
			pivot.SF_SU_Outward = childMenu.PK;

			return pivot;
		}

		StmMenuTemplatePivot CreateChildTemplateAndPivot(StmMenuItem parent)
		{
			var template = Factory.New<StmTemplate>();
			var pivot = Factory.New<StmMenuTemplatePivot>();
			pivot.SI_SU = parent.PK;
			pivot.SI_SO = template.PK;

			return pivot;
		}

		public void TestSU_PrimaryDocPackItem_ReadOnly()
		{
			var menuItem = Factory.New<StmMenuItemBase>();
			menuItem.SU_IsSystemDefined = true;
			menuItem.SU_IsDocPack = false;
			AssertEquals(true, menuItem.SU_PrimaryDocPackItem_ReadOnly);

			menuItem.SU_IsSystemDefined = false;
			menuItem.SU_IsDocPack = true;
			AssertEquals(true, menuItem.SU_PrimaryDocPackItem_ReadOnly);

			CreateChildMenuAndPivot(menuItem);
			AssertEquals(false, menuItem.SU_PrimaryDocPackItem_ReadOnly);

			menuItem.SU_IsSystemDefined = true;
			AssertEquals(true, menuItem.SU_PrimaryDocPackItem_ReadOnly);

			menuItem.SU_IsSystemDefined = false;
			menuItem.SU_IsDocPack = false;
			AssertEquals(true, menuItem.SU_PrimaryDocPackItem_ReadOnly);
		}

		public void TestDocumentDirectionList()
		{
			var menuItem = Factory.New<StmMenuItemBase>();
			AssertContainsExactElementsInAnyOrder(typeof(DocumentDirection).GetFields(BindingFlags.Public | BindingFlags.Static).Select(info => info.Name), menuItem.DocumentDirectionList.ToArray().Select(item => item.Code));
		}

		public void TestDeliveryRestrictionTypeList()
		{
			var menuItem = Factory.New<StmMenuItemBase>();
			List<string> expectedValue = new List<string>();
			foreach (var item in Enum.GetValues(typeof(DeliveryRestrictionType)))
			{
				expectedValue.Add(item.ToString());
			}
			AssertContainsExactElementsInAnyOrder(expectedValue, menuItem.DeliveryRestrictionTypeList.ToArray().Select(item => item.Code));
		}
		public void TestDeleteWebReportWithOrgSecurity()
		{
			var template = Factory.NewWithValidTestData<StmTemplate>();
			var menuItemBase = Factory.NewWithValidTestData<StmMenuItemBase>();
			menuItemBase.SU_MenuType = Core.Constants.StmMenuItemTypes.WebReports;

			var menuTemplatePivot = Factory.New<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_SO = template.PK;
			menuTemplatePivot.SI_SU = menuItemBase.PK;

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgSecurity = Factory.New<OrgSecurity>();
			orgSecurity.OX_SU = menuItemBase.PK;
			orgSecurity.OX_OH = orgHeader.PK;
			orgSecurity.OX_Granted = true;

			Factory.Save();

			menuItemBase.Delete();

			AssertEquals("Pivot.IsDeleted", true, menuTemplatePivot.IsDeleted);
			AssertEquals("Security.IsDeleted", true, orgSecurity.IsDeleted);
			AssertEquals("Template.IsDeleted", false, template.IsDeleted);

			Factory.Save();
		}

		public void TestSU_MenuIndex_ReadOnly()
		{
			var item = Factory.LoadTop1<DocumentCommand>(new ZQuery(StmMenuItemSchema.SU_IsSystemDefined, true));
			Assert(!item.SU_MenuIndexInfo.ReadOnly);
		}

		public void TestIsApplicable_ThrowsException()
		{
			var item = Factory.NewWithValidTestData<StmMenuItem_WithIsApplicableCoreOverride>();
			AssertEquals(true, item.IsApplicable);
			item.ExceptionToThrow = new OutOfMemoryException();
			AssertExceptionThrown(typeof(OutOfMemoryException), () => { var dummy = item.IsApplicable; });
			item.ExceptionToThrow = new NullReferenceException();
			AssertEquals(false, item.IsApplicable);
			AssertEquals("", ErrorReporter.LastMessageReported);
			item.SU_IsSystemDefined = true;
			AssertEquals(false, item.IsApplicable);
			AssertNotEquals("", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			item.ExceptionToThrow = new System.IO.IOException("There is not enough space on the disk.", unchecked((int)0x80070070));
			AssertEquals(false, item.IsApplicable);
			item.SU_IsSystemDefined = false;
			AssertEquals(false, item.IsApplicable);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		[StressTest]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		sealed class StmMenuItem_WithIsApplicableCoreOverride : StmMenuItemBase
		{
			public StmMenuItem_WithIsApplicableCoreOverride(BusinessObjectFactory factory, DataRow row) : base(factory, row) { }

			public Exception ExceptionToThrow;

			public override bool IsApplicableCore()
			{
				if (ExceptionToThrow != null)
				{
					throw ExceptionToThrow;
				}
				return base.IsApplicableCore();
			}
		}
	}
}
