using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Application;
using CargoWise.BuildTools;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Integration;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DocDummyBusinessObject = Enterprise.DocumentEngine.DocumentMenu.Testing.DocumentCommandTest.DocDummyBusinessObject;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentMenuCustomisation))]
	sealed class DocumentMenuCustomisationTest : MenuCustomisationTestCase<DocumentMenuCustomisation>
	{
		public void TestCopyConfigToNewDocCopiesPivotCorrectly()
		{
			var dummy = Factory.New<DummyDocumentSupportable>();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test";
			documentCommand.Parent = dummy;

			var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;

			var customization = DocumentMenuCustomisation.New(dummy, null, Factory);

			var config = pivot.DocConfigs.AddNew();
			config.S3_SI = pivot.PK;

			var config2 = pivot.DocConfigs.AddNew();
			config.S3_SI = pivot.PK;
			{
				var docType1 = Factory.New<RefDocType>();
				docType1.RT_DocType = "TS1";

				pivot.SI_RT_DocType = docType1.PK;
				pivot.SI_PrintByDefault = ZBool.True;
				pivot.SI_MenuTemplateFilter = "HBL=SEA";
				pivot.SI_PrintCopyType = nameof(PrintCopyType.FAX);

				var copiedMenuItem = customization.CopyConfigToNewDoc(documentCommand, new StmMenuTemplatePivotBase[] { pivot });
				var copiedPivots = Factory.Load<StmMenuTemplatePivotBase>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, copiedMenuItem.PK));
				AssertEquals("There should of been 1 pivot copied.", 1, copiedPivots.Length);

				var result = copiedPivots.First();
				AssertEquals("TS1", result.DocType.RT_DocType);
				AssertEquals(ZBool.True, result.SI_PrintByDefault);
				AssertEquals("HBL=SEA", result.SI_MenuTemplateFilter);
				AssertEquals("FAX", result.SI_PrintCopyType);
				AssertEquals(2, result.DocConfigs.Count);

				AssertEquals("The copied menu item should not have any notifications.", false, copiedMenuItem.HasNotifications());
			}

			{
				var docType2 = Factory.New<RefDocType>();
				docType2.RT_DocType = "TS2";

				pivot.SI_RT_DocType = docType2.PK;
				pivot.SI_PrintByDefault = ZBool.False;
				pivot.SI_MenuTemplateFilter = "HBL=AIR";
				pivot.SI_PrintCopyType = nameof(PrintCopyType.EML);

				var pivot2 = documentCommand.Documents.AddNew();
				pivot2.SI_SU = documentCommand.PK;
				pivot2.SI_SO = template.PK;

				pivot2.SI_RT_DocType = docType2.PK;
				pivot2.SI_PrintByDefault = ZBool.False;
				pivot2.SI_MenuTemplateFilter = "HBL=AIR";
				pivot2.SI_PrintCopyType = nameof(PrintCopyType.EML);

				var copiedMenuItem = customization.CopyConfigToNewDoc(documentCommand, new StmMenuTemplatePivotBase[] { pivot, pivot2 });
				var copiedPivots = Factory.Load<StmMenuTemplatePivotBase>(new ZQuery(StmMenuTemplatePivotSchema.SI_SU, copiedMenuItem.PK));
				AssertEquals("There should of been 1 pivot copied.", 2, copiedPivots.Length);

				var result = copiedPivots.First();
				AssertEquals("TS2", result.DocType.RT_DocType);
				AssertEquals(ZBool.False, result.SI_PrintByDefault);
				AssertEquals("HBL=AIR", result.SI_MenuTemplateFilter);
				AssertEquals("EML", result.SI_PrintCopyType);
				AssertEquals(2, result.DocConfigs.Count);

				result = copiedPivots[1];
				AssertEquals(0, result.DocConfigs.Count);

				AssertEquals("The copied menu item should not have any notifications.", false, copiedMenuItem.HasNotifications());
			}
		}

		public void TestCopyConfigToNewDocWhenIncludeDocInArchiveIsYesDoesNotCauseValidationError()
		{
			var result = GetCopyOfDocBuilderMenuItem(ArchiveConstants.IncludeDocInArchiveCodes.Yes);
			AssertEquals("IncludeDocInArchive should be YES.", ArchiveConstants.IncludeDocInArchiveCodes.Yes, result.SU_IncludeDocInArchive);
		}

		public void TestCopyConfigToNewDocWhenIncludeDocInArchiveIsNoDoesNotCauseValidationError()
		{
			var result = GetCopyOfDocBuilderMenuItem(ArchiveConstants.IncludeDocInArchiveCodes.No);

			AssertEquals("IncludeDocInArchive should be empty.", ArchiveConstants.IncludeDocInArchiveCodes.No, result.SU_IncludeDocInArchive);
		}

		StmMenuItem GetCopyOfDocBuilderMenuItem(ZString includeDocInArchive)
		{
			var dummy = Factory.New<DummyDocumentSupportable>();

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.SU_MenuName = "Test";
			documentCommand.SU_IncludeDocInArchive = includeDocInArchive;
			documentCommand.Parent = dummy;

			var template = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var docType = Factory.New<RefDocType>();
			docType.RT_DocType = "TST";

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SU = documentCommand.PK;
			pivot.SI_SO = template.PK;
			pivot.SI_RT_DocType = docType.PK;

			var customization = DocumentMenuCustomisation.New(dummy, null);

			var config = pivot.DocConfigs.AddNew();
			config.S3_SI = pivot.PK;

			var result = customization.CopyConfigToNewDoc(documentCommand, new StmMenuTemplatePivotBase[] { pivot });
			AssertEquals("The copied menu item should not have any notifications.", false, result.HasNotifications());

			return result;
		}

		public void TestShouldCreateNewFactory()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			var documentMenuCustomization = DocumentMenuCustomisation.New(dummy, null);

			AssertNotEquals("Factories should be different", dummy.Factory, documentMenuCustomization.Factory);
		}

		public void TestCopyConfigToNewDocument()
		{
			//Setup of Items that will be copied

			var menu = Customisation.Menus.AddNew();
			menu.FillWithValidTestData();
			menu.SU_MenuName = "Test Doc";
			Customisation.Menus.Add(menu);

			var template = Factory.New<StmTemplateBase>();
			template.FillWithValidTestData();

			var templatePivot = Customisation.AddTemplatePivot(menu, template);
			templatePivot.SI_DocumentTitle = "Test Doc Title";

			var docConfig = templatePivot.DocConfigs.AddNew();
			docConfig.FillWithValidTestData();
			docConfig.S3_IsSystem = ZBool.True;
			docConfig.S3_IsTemplate = ZBool.True;
			docConfig.S3_IsClientSpecific = ZBool.True;
			docConfig.S3_OH = ZGuid.NewZGuid();
			docConfig.S3_GC = GlbCompany.CurrentCompany.PK;

			for (int configItemCount = 0; configItemCount < 10; configItemCount++)
			{
				var docConfigItem = Factory.New<StmMenuDocumentConfigItem>();
				docConfigItem.FillWithValidTestData();
				docConfigItem.S4_S3 = docConfig.PK;
				docConfigItem.S4_IsSystemDefined = true;
				docConfigItem.S4_IsClientSpecific = true;
			}

			Customisation.CopyConfigToNewDoc(menu, new StmMenuTemplatePivotBase[] { templatePivot });

			//Test Copied Menu

			var copyMenu = Customisation.Menus[Customisation.Menus.Count - 1];
			var originalMenu = Customisation.Menus[Customisation.Menus.Count - 2];

			AssertEquals(originalMenu.SU_MenuName + " - Copy", copyMenu.SU_MenuName);
			AssertEquals(false, copyMenu.SU_IsSystemDefined);
			AssertEquals(false, copyMenu.SU_IsPublished);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, copyMenu.SU_GS_NKStaffCode);

			var excludedMenuColumns = new string[] { "PK", "SU_MenuName", "SU_IsSytem", "SU_IsPublished", "SU_GS_NKStaffCode" };
			foreach (CargoWise.Schema.SchemaColumn schemaColumn in ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns("StmMenuItem"))
			{
				if (!excludedMenuColumns.Contains(schemaColumn.ObjectName))
				{
					AssertEquals(originalMenu[schemaColumn.ObjectName], copyMenu[schemaColumn.ObjectName]);
				}
			}

			//Test Copied Pivot

			var copyPivot = copyMenu.Documents[0];
			var originalPivot = originalMenu.Documents[0];

			AssertEquals(originalPivot.SI_DocumentTitle, copyPivot.SI_DocumentTitle);
			AssertEquals(originalPivot.SI_SO, copyPivot.SI_SO);

			//Test Copied DocConfig

			var copyDocConfig = copyPivot.DocConfigs[0];
			var originalDocConfig = originalPivot.DocConfigs[0];

			AssertEquals(originalDocConfig.S3_OverrideDataContext, copyDocConfig.S3_OverrideDataContext);
			AssertEquals(originalDocConfig.S3_PageStyle, copyDocConfig.S3_PageStyle);
			AssertEquals(copyPivot.PK, copyDocConfig.S3_SI);
			AssertEquals(false, copyDocConfig.S3_IsSystem);
			AssertEquals(false, copyDocConfig.S3_IsTemplate);
			AssertEquals(false, copyDocConfig.S3_IsClientSpecific);
			AssertEquals(ZGuid.Empty, copyDocConfig.S3_OH);
			AssertEquals(ZGuid.Empty, copyDocConfig.S3_GC);

			//Test Copied DocConfigItems

			AssertEquals(copyDocConfig.ConfigItems.Count, originalDocConfig.ConfigItems.Count);

			for (int docConfigItemCount = 0; docConfigItemCount < originalDocConfig.ConfigItems.Count; docConfigItemCount++)
			{
				foreach (CargoWise.Schema.SchemaColumn schemaColumn in ObjectFactory.Get<IApplicationSchemaResolver>().GetSchemaColumns("StmMenuDocumentConfigItem"))
				{
					if (!(schemaColumn.ObjectName.Equals("PK") || schemaColumn.ObjectName.Equals("S4_S3") || schemaColumn.ObjectName.Equals("S4_IsSystemDefined") || schemaColumn.ObjectName.Equals("S4_IsClientSpecific")))
					{
						AssertEquals(originalDocConfig.ConfigItems[docConfigItemCount][schemaColumn.ObjectName], copyDocConfig.ConfigItems[docConfigItemCount][schemaColumn.ObjectName]);
					}
				}
				AssertEquals(copyDocConfig.PK, copyDocConfig.ConfigItems[docConfigItemCount].S4_S3);
				AssertEquals(false, copyDocConfig.ConfigItems[docConfigItemCount].S4_IsSystemDefined);
				AssertEquals(false, copyDocConfig.ConfigItems[docConfigItemCount].S4_IsClientSpecific);
			}

			//Check multiple menus name correctly

			Customisation.CopyConfigToNewDoc(menu, new StmMenuTemplatePivotBase[] { templatePivot });
			Customisation.CopyConfigToNewDoc(menu, new StmMenuTemplatePivotBase[] { templatePivot });

			var menuCopy2 = Customisation.Menus[Customisation.Menus.Count - 2];
			var menuCopy3 = Customisation.Menus[Customisation.Menus.Count - 1];

			AssertEquals(menuCopy2.SU_MenuName, originalMenu.SU_MenuName + " - Copy (2)");
			AssertEquals(menuCopy3.SU_MenuName, originalMenu.SU_MenuName + " - Copy (3)");

			AssertNoExceptionThrown(() =>
			{
				originalMenu.SU_MenuName = new ZString('C', menuCopy2.SU_MenuNameInfo.MaxLength);
				Customisation.CopyConfigToNewDoc(menu, new StmMenuTemplatePivotBase[] { templatePivot });
			});
			var menuCopy4 = Customisation.Menus[Customisation.Menus.Count - 1];
			AssertEquals("Overlong menu name should be truncated.", menuCopy4.SU_MenuNameInfo.MaxLength, menuCopy4.SU_MenuName.Length);
			var expectedTruncatedMenuName = new string('C', menuCopy4.SU_MenuNameInfo.MaxLength - 3) + "...";
			AssertEquals($"Truncated Menu Name should be '{expectedTruncatedMenuName}'", expectedTruncatedMenuName, menuCopy4.SU_MenuName);
		}

		public void TestAddChildMenuPivot()
		{
			var forwardingDocumentSupporter = Factory.NewWithValidTestData(ObjectFactory.GetType<Forwarding.IForwardingShipment>()) as IDocumentSupportable;
			var customization = DocumentMenuCustomisation.New(forwardingDocumentSupporter, null, Factory);

			StmMenuItemBase shipmentMenu = customization.AvailableChildMenus[2];
			shipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			shipmentMenu.SU_MenuName = "Booking Confirmation";

			DocumentCommand menu = customization.Menus[1];
			StmMenuItemBase childMenu = Customisation.AvailableChildMenus[2];
			menu.EditingMode = MenuEditingMode.AllowAll;

			StmMenuMenuPivotBase pivot = customization.AddChildMenuPivot(menu, childMenu);
			AssertEquals("pivot.SF_SU_Inward", menu.PK, pivot.SF_SU_Inward);
			AssertEquals("pivot.SF_SU_Outward", childMenu.PK, pivot.SF_SU_Outward);
			AssertEquals("pivot.EditingMode", MenuEditingMode.AllowAll, pivot.EditingMode);
			AssertEquals("pivot.SF_OverriddenBusinessContext", ZString.Empty, pivot.SF_OverriddenBusinessContext);

			menu = Customisation.Menus[1];
			childMenu = ProxyStmMenuItemBase.New(shipmentMenu, nameof(BusinessContext.SubShipment));
			childMenu.SU_BusinessContext = nameof(BusinessContext.SubShipment);
			childMenu.SU_MenuName = "Booking Confirmation";

			pivot = customization.AddChildMenuPivot(menu, childMenu);
			AssertEquals("pivot.SF_SU_Inward", menu.PK, pivot.SF_SU_Inward);
			AssertEquals("pivot.SF_SU_Outward", shipmentMenu.PK, pivot.SF_SU_Outward);
			AssertEquals("pivot.EditingMode", MenuEditingMode.AllowAll, pivot.EditingMode);
			AssertEquals("pivot.SF_OverriddenBusinessContext", nameof(BusinessContext.SubShipment), pivot.SF_OverriddenBusinessContext);

			AssertNull("AddChildMenuPivot()", Customisation.AddChildMenuPivot(null, null));
		}

		public void TestAddDocTypePivot()
		{
			RefDocType docType = Factory.New<RefDocType>();
			DocumentCommand menu = Customisation.Menus[1];

			DocumentStmMenuEDocs pivot = Customisation.AddDocTypePivot(menu, docType);
			AssertEquals("pivot.SX_SU", menu.PK, pivot.SX_SU);
			AssertEquals("pivot.SX_RT_DocType", docType.PK, pivot.SX_RT_DocType);

			AssertNull("AddDocTypePivot()", Customisation.AddDocTypePivot(null, null));
		}

		public void TestAvailableChildMenus()
		{
			StmMenuItemBase CreateMenu(string businessContext, string menuName)
			{
				var menuItem = Factory.New<StmMenuItemBase>();
				menuItem.SU_MenuName = menuName;
				menuItem.SU_BusinessContext = businessContext;

				var template = Factory.New<StmTemplate>();
				template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
				template.SO_IsSystemDefined = true;
				template.SO_Name = menuName;

				var pivot = Factory.New<StmMenuTemplatePivotBase>();
				pivot.SI_SO = template.PK;
				pivot.SI_SU = menuItem.PK;
				pivot.SI_DocumentTitle = menuName;

				return menuItem;
			}

			var containerMenu = CreateMenu(nameof(BusinessContext.CFSContainerRego), "container");
			var consolMenu = CreateMenu(nameof(BusinessContext.Consol), "consol");
			var shipmentMenu = CreateMenu(nameof(BusinessContext.Shipment), "shipment");
			var formMenu = CreateMenu(nameof(BusinessContext.Shipment), "shipment form");
			formMenu.SU_MenuType = "FRM";

			Factory.Save();

			var docDummy = Factory.New<DocDummyBusinessObject>();
			AssertEquals("docDummy.DocumentSupporter.BusinessContext", BusinessContext.Shipment, docDummy.DocumentSupporter.BusinessContext);
			AssertEquals("docDummy.DocumentSupporter.SupportedChildBusinessContexts.Count", 1, docDummy.DocumentSupporter.SupportedChildBusinessContexts.Length);
			AssertEquals("docDummy.DocumentSupporter.SupportedChildBusinessContexts[0]", BusinessContext.Consol, docDummy.DocumentSupporter.SupportedChildBusinessContexts[0]);

			var customisation = DocumentMenuCustomisation.New(docDummy, null, Factory);
			AssertEquals("AvailableChildMenus.Contains(containerMenu)", false, customisation.AvailableChildMenus.Contains(containerMenu));
			AssertEquals("AvailableChildMenus.Contains(consolMenu)", true, customisation.AvailableChildMenus.Contains(consolMenu));
			AssertEquals("AvailableChildMenus.Contains(shipmentMenu)", true, customisation.AvailableChildMenus.Contains(shipmentMenu));
			AssertEquals("AvailableChildMenus.Contains(formMenu)", true, customisation.AvailableChildMenus.Contains(formMenu));

			docDummy.ReturnNullForSupportedChildBusinessContext = true;
			customisation = DocumentMenuCustomisation.New(docDummy, null, Factory);
			AssertEquals("AvailableChildMenus.Contains(containerMenu)", false, customisation.AvailableChildMenus.Contains(containerMenu));
			AssertEquals("AvailableChildMenus.Contains(consolMenu)", false, customisation.AvailableChildMenus.Contains(consolMenu));
			AssertEquals("AvailableChildMenus.Contains(shipmentMenu)", true, customisation.AvailableChildMenus.Contains(shipmentMenu));
			AssertEquals("AvailableChildMenus.Contains(formMenu)", true, customisation.AvailableChildMenus.Contains(formMenu));
		}

		public void TestAvailableDocTypes()
		{
			string referenceType = DocumentAssemblyDataProxy.GetReferenceTypeFromDocManagerCode(((IDocManagerSupport)DocumentSupportable).DocManagerInfo.DocManagerCode);

			RefDocType internalPrivateDocType = GetDocType(Core.Constants.RefDocTypes.InternallyCreatedPrivateDocument, referenceType, true);
			RefDocType internalPublicDocType = GetDocType(Core.Constants.RefDocTypes.InternallyCreatedPublicDocument, referenceType, true);
			RefDocType activeDocType = GetDocType("AYE", referenceType, true);
			RefDocType inactiveDocType = GetDocType("NAY", referenceType, false);

			AssertEquals("AvailableDocTypes.Contains(internalPrivateDocType)", false, Customisation.AvailableDocTypes.Contains(internalPrivateDocType));
			AssertEquals("AvailableDocTypes.Contains(internalPublicDocType)", false, Customisation.AvailableDocTypes.Contains(internalPublicDocType));
			AssertEquals("AvailableDocTypes.Contains(activeDocType)", true, Customisation.AvailableDocTypes.Contains(activeDocType));
			AssertEquals("AvailableDocTypes.Contains(inactiveDocType)", false, Customisation.AvailableDocTypes.Contains(inactiveDocType));
		}

		public override void TestAvailableTemplates()
		{
			StmTemplateBaseCollection templates = new StmTemplateBaseCollection(Factory, DocumentSupportable.DocumentSupporter.FilterForSupportedDataContexts);
			templates.Load();
			AssertCollectionEquals(templates, Customisation.AvailableTemplates, "AvailableTemplates");
		}

		public void TestCopyDocConfig()
		{
			AssertNull("CopyDocConfig()", Customisation.CopyDocConfig(null, null));

			StmMenuTemplatePivotBase templatePivot = Factory.New<StmMenuTemplatePivotBase>();
			StmMenuDocumentConfig docConfig1 = templatePivot.DocConfigs.AddNew();
			StmMenuDocumentConfig docConfig2 = templatePivot.DocConfigs.AddNew();
			StmMenuDocumentConfigItem configItem1 = docConfig1.ConfigItems.AddNew();
			StmMenuDocumentConfigItem configItem2 = docConfig2.ConfigItems.AddNew();
			docConfig2.S3_GC = GlbCompany.CurrentCompany.PK;

			docConfig1.S3_IsSystem = true;
			docConfig1.S3_IsTemplate = true;
			docConfig1.S3_IsClientSpecific = true;

			configItem1.S4_SectionItemName = "SectionA";
			configItem1.S4_IsSystemDefined = true;
			configItem1.S4_IsClientSpecific = true;

			configItem2.S4_SectionItemName = "SectionB";
			configItem2.S4_IsSystemDefined = false;
			configItem2.S4_IsClientSpecific = false;

			StmMenuDocumentConfig docConfigCopy1 = Customisation.CopyDocConfig(docConfig1, templatePivot);
			AssertEquals("CopyDocConfig().S3_IsSystem", false, docConfigCopy1.S3_IsSystem);
			AssertEquals("CopyDocConfig().S3_IsTemplate", false, docConfigCopy1.S3_IsTemplate);
			AssertEquals("CopyDocConfig().S3_IsClientSpecific", false, docConfigCopy1.S3_IsClientSpecific);
			AssertEquals("CopyDocConfig().ConfigItems.Count", 1, docConfigCopy1.ConfigItems.Count);
			AssertEquals("CopyDocConfig().ConfigItems[0].S4_SectionItemName", "SectionA", docConfigCopy1.ConfigItems[0].S4_SectionItemName);
			AssertEquals("CopyDocConfig().ConfigItems[0].S4_IsSystemDefined", false, docConfigCopy1.ConfigItems[0].S4_IsSystemDefined);
			AssertEquals("CopyDocConfig().ConfigItems[0].S4_IsClientSpecific", false, docConfigCopy1.ConfigItems[0].S4_IsClientSpecific);

			StmMenuDocumentConfig docConfigCopy2 = Customisation.CopyDocConfig(docConfig2, templatePivot);
			AssertEquals("CopyDocConfig().S3_IsSystem", false, docConfigCopy2.S3_IsSystem);
			AssertEquals("CopyDocConfig().S3_IsClientSpecific", false, docConfigCopy2.S3_IsClientSpecific);
			AssertEquals("CopyDocConfig().ConfigItems.Count", 1, docConfigCopy2.ConfigItems.Count);
			AssertEquals("CopyDocConfig().ConfigItems[0].S4_SectionItemName", "SectionB", docConfigCopy2.ConfigItems[0].S4_SectionItemName);
			AssertEquals("CopyDocConfig().ConfigItems[0].S4_IsSystemDefined", false, docConfigCopy2.ConfigItems[0].S4_IsSystemDefined);
			AssertEquals("CopyDocConfig().ConfigItems[0].S4_IsClientSpecific", false, docConfigCopy2.ConfigItems[0].S4_IsClientSpecific);

			AssertEquals("MenuTemplatePivot.DocConfigs.Count", 4, templatePivot.DocConfigs.Count);
			AssertEquals("MenuTemplatePivot.DocConfigs[0]", docConfig1, templatePivot.DocConfigs[0]);
			AssertEquals("MenuTemplatePivot.DocConfigs[1]", docConfigCopy1, templatePivot.DocConfigs[1]);
			AssertEquals("MenuTemplatePivot.DocConfigs[2]", docConfig2, templatePivot.DocConfigs[2]);
			AssertEquals("MenuTemplatePivot.DocConfigs[3]", docConfigCopy2, templatePivot.DocConfigs[3]);
		}

		public override void TestEditingMode()
		{
			base.TestEditingMode();

			Customisation.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("AvailableChildMenus.EditingMode", MenuEditingMode.AllowEditingOfSystemDefinedOnly, Customisation.AvailableChildMenus.EditingMode);

			Customisation.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("AvailableChildMenus.EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, Customisation.AvailableChildMenus.EditingMode);
		}

		public override void TestGetPrintTask()
		{
			var userFieldList = new UserControlProviderList();
			var customisation = DocumentMenuCustomisation.New(DocumentSupportable, userFieldList);
			var menu = customisation.Menus[0];

			using (var printTask = customisation.GetPrintTask(menu) as DocumentPrintSet)
			{
				AssertEquals("GetPrintTask().ParentMenuCommand", menu, printTask.ParentMenuCommand);
				AssertEquals("GetPrintTask().UserFieldList", userFieldList, printTask.UserFieldList);
			}
		}

		public override void TestMenus()
		{
			DocumentCommandCollection documents = new DocumentCommandCollection(DocumentSupportable, Factory);
			documents.Load();
			AssertCollectionEquals(documents, Customisation.Menus, "Menus");
		}

		public void TestNew()
		{
			MockEDocsProvider eDocsProvider = GetMockEDocsProvider();
			DocumentMenuCustomisation customisation = DocumentMenuCustomisation.New(eDocsProvider, null);
			int originalMenusCount = customisation.Menus.Count;

			MenuItemIdentifier consumer1 = new MenuItemIdentifier(BusinessContext.Customs, "Some Customs Menu Item");
			MenuItemIdentifier consumer2 = new MenuItemIdentifier(BusinessContext.ContainerLeg, "Some ContainerLeg Menu Item");
			DocumentCommand menu1 = GetMenu(consumer1);
			DocumentCommand menu2 = GetMenu(consumer2);
			eDocsProvider.AddEDocsConsumer(consumer1);
			eDocsProvider.AddEDocsConsumer(consumer2);

			Factory.Save();

			customisation = DocumentMenuCustomisation.New(eDocsProvider, null);
			AssertEquals("Count", originalMenusCount + 2, customisation.Menus.Count);

			EDocsProviderSupporter supporter = eDocsProvider.GetEDocsProviderSupporter();
			DocumentCommand placeholder1 = supporter.GetProviderPlaceholder<DocumentCommand>(menu1);
			DocumentCommand placeholder2 = supporter.GetProviderPlaceholder<DocumentCommand>(menu2);

			AssertNotNull("placeholder1", placeholder1);
			AssertNotNull("placeholder2", placeholder1);
			AssertEquals("Menus.Contains(placeholder1)", true, customisation.Menus.Contains(placeholder1));
			AssertEquals("Menus.Contains(placeholder1)", true, customisation.Menus.Contains(placeholder1));
			AssertEquals("Menus.Contains(placeholder1)", true, customisation.Menus.Contains(placeholder1));
			AssertEquals("Menus.Contains(placeholder1)", true, customisation.Menus.Contains(placeholder1));
			AssertEquals("placeholder1.SU_GS_NKStaffCode", ZString.Empty, placeholder1.SU_GS_NKStaffCode);
			AssertEquals("placeholder2.SU_GS_NKStaffCode", ZString.Empty, placeholder2.SU_GS_NKStaffCode);

			placeholder1.SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;
			placeholder2.SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;

			Factory.Save();

			customisation = DocumentMenuCustomisation.New(eDocsProvider, null);
			AssertEquals("Count", originalMenusCount + 2, customisation.Menus.Count);
			AssertEquals("Menus.Contains(placeholder1)", true, customisation.Menus.Contains(placeholder1));
			AssertEquals("Menus.Contains(placeholder1)", true, customisation.Menus.Contains(placeholder2));
			AssertEquals("placeholder1.SU_GS_NKStaffCode", ZString.Empty, customisation.Menus.FindByPK(placeholder1.PK).SU_GS_NKStaffCode);
			AssertEquals("placeholder2.SU_GS_NKStaffCode", ZString.Empty, customisation.Menus.FindByPK(placeholder2.PK).SU_GS_NKStaffCode);
		}

		public void TestNewFailsOnceAndContinues()
		{
			MenuItemIdentifier consumer = new MenuItemIdentifier(BusinessContext.Customs, "Some Customs Menu Item");
			MockEDocsProvider eDocsProvider = GetMockEDocsProvider();
			DocumentCommand menu = GetMenu(consumer);
			DocumentCommand placeholder = null;

			eDocsProvider.AddEDocsConsumer(consumer);
			Factory.Save();

			EventHandler savingEDocsProviderPlaceholders = new EventHandler(delegate
			{
				placeholder = eDocsProvider.GetEDocsProviderSupporter().CreateProviderPlaceholder<DocumentCommand>(menu);
				Factory.Save();
			});

			try
			{
				DocumentMenuCustomisation.SavingEDocsProviderPlaceholders += savingEDocsProviderPlaceholders;
				DocumentMenuCustomisation customisation = DocumentMenuCustomisation.New(eDocsProvider, null);
				AssertEquals("Menus.Contains(placeholder)", true, customisation.Menus.Contains(placeholder));
			}
			finally
			{
				DocumentMenuCustomisation.SavingEDocsProviderPlaceholders -= savingEDocsProviderPlaceholders;
			}
		}

		public void TestNewFailsTwiceAndThrows()
		{
			MenuItemIdentifier consumer = new MenuItemIdentifier(BusinessContext.Customs, "Some Customs Menu Item");
			int count = 0;
			MockEDocsProvider eDocsProvider = GetMockEDocsProvider();
			DocumentCommand menu = GetMenu(consumer);

			eDocsProvider.AddEDocsConsumer(consumer);
			Factory.Save();

			EventHandler savingEDocsProviderPlaceholders = new EventHandler(delegate
			{
				count++;
				throw new ZSaveException(new ZDataException(new Exception("Save failed."), null, null), Factory);
			});

			try
			{
				DocumentMenuCustomisation.SavingEDocsProviderPlaceholders += savingEDocsProviderPlaceholders;
				try
				{
					DocumentMenuCustomisation customisation = DocumentMenuCustomisation.New(eDocsProvider, null);
					Fail("An exception should have been thrown.");
				}
				catch (ZSaveException ex)
				{
					AssertEquals("Save should only run twice.", 2, count);
					Assert("Exception message should contain 'Save failed.'.", ex.Message.Contains("Save failed."));
				}
			}
			finally
			{
				DocumentMenuCustomisation.SavingEDocsProviderPlaceholders -= savingEDocsProviderPlaceholders;
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestUpdateCustomizeDocumentElements_TemplateIsNull_DoesNotThrowException()
		{
			var config = Factory.New<StmMenuDocumentConfig>();
			var configItem = config.ConfigItems.AddNew();
			configItem.S4_SectionItemName = "Company Logo";

			var template = TemplateTestHelper.CreateTemplate(Factory, Core.Constants.SectionRepositoryTemplateNames.User, null, DataContext.ToString());

			AssertEquals("template.SO_Template", 0, template.SO_Template.Length);

			using (TempFile tempFile = TempFile.NewFromFile(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\Customized Document Elements One Section.xls")))
			{
				var result = Customisation.UpdateTemplateRecord(template, tempFile.Filename);
				AssertEquals("Success", true, result.Success);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateCustomizedDocumentElements_RemoveSystemOverridenSections()
		{
			var query = new ZQuery(StmMenuDocumentConfigItemSchema.S4_SectionItemName, "Company Logo");
			query.AddToFilter(JoinCondition.And, StmMenuDocumentConfigItemSchema.S4_IsSystemDefined, true);
			query.AddToFilter(JoinCondition.And, StmMenuDocumentConfigItemSchema.S4_SectionType, "HFP");
			Assert("Pre-Condition: System section is used in existing documents", Factory.Load<StmMenuDocumentConfigItem>(query).Any());

			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = "GenericFreightJob";
			template.SO_Name = Core.Constants.SectionRepositoryTemplateNames.User;
			var excelTemplate = new ExcelTemplateForUnitTesting("Customized Document Elements System One Section.xls", TestFilesSubFolder.DocumentTestFiles);
			template.SO_Template = excelTemplate.GetAsByteArray();

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			Factory.Save();

			using (var tempFile = TempFile.NewFromFile(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\Customized Document Elements System Empty.xls")))
			{
				var result = Customisation.UpdateTemplateRecord(template, tempFile.Filename);
				Assert("One section is removed however it is system overriden", result.Success);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestUpdateCustomizedDocumentElements_RemoveNonSystemOverridenSections()
		{
			ReturnResult result;

			var dummy = Factory.New<DummyDocumentSupportable>();
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var template = Factory.New<StmTemplateBase>();
			template.SO_DataContext = "GenericFreightJob";
			template.SO_Name = Core.Constants.SectionRepositoryTemplateNames.User;
			var excelTemplate = new ExcelTemplateForUnitTesting("Customized Document Elements Five Sections.xls", TestFilesSubFolder.DocumentTestFiles);
			template.SO_Template = excelTemplate.GetAsByteArray();

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			PrepareDocumentCommand(dummy, template, "A Happy Document", @"Sean Lennon/Parachute", "A Happy Section", true, "");
			PrepareDocumentCommand(dummy, template, "A Happy Document", @"Maroon 5/Sugar", "A Happy Section", true, "");
			PrepareDocumentCommand(dummy, template, "A Happy Document", @"Maroon 5/moves like jagger", "A Happy Section", false, "EGI");
			PrepareDocumentCommand(dummy, template, "Another Happy Document", @"Coldplay/Yellow", "Another Happy Section", true, "");
			PrepareDocumentCommand(dummy, template, "Another Happy Document", @"Bon Jovi/It's my life", "Another Happy Section", true, "");
			PrepareDocumentCommand(dummy, template, "A Sad Document", @"Bethel Music/It is well with my soul", "A Sad Section", false, "KLL");
			PrepareDocumentCommand(dummy, template, "A Sad Document", @"Coldplay/The Scientist", "a Sad Section", true, "");

			Factory.Save();

			using (var tempFile = TempFile.NewFromFile(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\Customized Document Elements Four Sections.xls")))
			{
				result = Customisation.UpdateTemplateRecord(template, tempFile.Filename);
				Assert("One section is removed however it is not used in any document", result.Success);
				Assert(string.IsNullOrEmpty(result.Message));
			}

			using (var tempFile = TempFile.NewFromFile(Path.Combine(BaseSourcePath, @"Enterprise\Product\Documents\DocumentEngine\Testing\DocumentTestFiles\Customized Document Elements One Section.xls")))
			{
				result = Customisation.UpdateTemplateRecord(template, tempFile.Filename);
				Assert(!result.Success);
				AssertEquals("Should show correct error message",
@"There are Section(s) removed/missing from Customized Document Elements which are being used in the following DocBuilder document(s). Please remove the section from documents before deleting this section from Customized Document Elements:

Documents using section ""A Happy Section"":
*** Data Context: Dummy, Path: Maroon 5/moves like jagger, Document Name: ""A Happy Document"", Creating User: EGI (Unpublished)
*** Data Context: Dummy, Path: Maroon 5/Sugar, Document Name: ""A Happy Document""
*** Data Context: Dummy, Path: Sean Lennon/Parachute, Document Name: ""A Happy Document""

Documents using section ""A Sad Section"":
*** Data Context: Dummy, Path: Bethel Music/It is well with my soul, Document Name: ""A Sad Document"", Creating User: KLL (Unpublished)
*** Data Context: Dummy, Path: Coldplay/The Scientist, Document Name: ""A Sad Document""

Documents using section ""Another Happy Section"":
*** Data Context: Dummy, Path: Bon Jovi/It's my life, Document Name: ""Another Happy Document""
*** Data Context: Dummy, Path: Coldplay/Yellow, Document Name: ""Another Happy Document""".ToUpper()
					, result.Message.ToUpper());
			}
		}

		void PrepareDocumentCommand(DummyDocumentSupportable dummy, StmTemplateBase template, string menuName, string menuPath, string sectionName, bool isPublished, string userCode)
		{
			var docBuilderDocumentCommand = Factory.New<DocumentCommand>();
			docBuilderDocumentCommand.Parent = dummy;
			docBuilderDocumentCommand.SU_MenuName = menuName;
			docBuilderDocumentCommand.SU_BusinessContext = "Dummy";
			docBuilderDocumentCommand.SU_MenuPath = menuPath;
			docBuilderDocumentCommand.SU_IsPublished = isPublished;
			if (!isPublished)
			{
				docBuilderDocumentCommand.SU_GS_NKStaffCode = userCode;
			}

			var docBuilderTemplate1 = docBuilderDocumentCommand.Documents.AddNew();
			docBuilderTemplate1.SI_SU = docBuilderDocumentCommand.PK;
			docBuilderTemplate1.SI_SO = template.PK;

			var aDocConfig = docBuilderTemplate1.DocConfigs.AddNew();
			aDocConfig.S3_OH = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			aDocConfig.ConfigItems.AddNew().S4_SectionItemName = sectionName;
		}

		protected override void AssertUpdateTemplateRecordCore(ReturnResult result, StmTemplateBase template)
		{
			Assert("Success", result.Success);
			AssertEquals("Message", string.Format(CultureInfo.InvariantCulture, "The data context of this template is changed from AccountingVoucher to {0}", DataContext), result.Message);
			Assert("template.SO_Template.IsEmpty", !template.SO_Template.IsEmpty);
			AssertEquals("Organisation", template.SO_DataContext);
		}

		#region Implementation

		protected override MenuCustomisationDocumentSupporter SupportedDocumentSupporter
		{
			get
			{
				return new DocumentMenuCustomisationDocumentSupporter(Customisation);
			}
		}

		IDocumentSupportable documentSupportable;

		protected override Core.Constants.DataContext DataContext
		{
			get { return DocumentSupportable.DocumentSupporter.DefaultDataContext; }
		}

		IDocumentSupportable DocumentSupportable
		{
			get { return documentSupportable ?? (documentSupportable = Factory.NewWithValidTestData<OrgHeader>()); }
		}

		protected override string ValidTemplateFilePath
		{
			get { return Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\Product\Documents\ExcelTemplates\Documents\Organisation Sales Client Overview.xls"); }
		}

		protected override string ValidTemplateFilePathReleaseBuild
		{
			get { return "Organisation Sales Client Overview.xls"; }
		}

		RefDocType GetDocType(string code, string referenceType, bool active)
		{
			RefDocType result = Factory.New<RefDocType>();
			result.RT_DocType = code;
			result.RT_ReferenceType = referenceType;
			result.RT_IsActive = active;
			return result;
		}

		protected override KeyValuePair<string, string>[] GetInvalidTemplateFilePathAndValidationErrorMessagePairs()
		{
			return new KeyValuePair<string, string>[]
			{
				new KeyValuePair<string, string>(UnitTestingConstants.TestDocumentExcelTemplateFilePath, "This template cannot be added because it requires a data context of '.DummyBusinessObject', but the form only supports '" + DocumentSupportable.DocumentSupporter.CommaSeparatedListOfSupportedDataContexts + "'."),
				new KeyValuePair<string, string>(UnitTestingConstants.TestReportExcelTemplateFilePath, "This template cannot be added because it does not have a specified data context.")
			};
		}

		DocumentCommand GetMenu(MenuItemIdentifier consumer)
		{
			DocumentCommand result = Factory.New<DocumentCommand>();
			result.SU_BusinessContext = consumer.BusinessContext.ToString();
			result.SU_IsSystemDefined = true;
			result.SU_MenuName = consumer.Name;
			return result;
		}

		MockEDocsProvider GetMockEDocsProvider()
		{
			MockEDocsProvider result = MockEDocsProvider.New(Factory, BusinessContext.JobService);
			result.DocumentSupporterFactory = Factory;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return DocumentMenuCustomisation.New(DocumentSupportable, null, Factory);
		}

		#endregion
	}
}
