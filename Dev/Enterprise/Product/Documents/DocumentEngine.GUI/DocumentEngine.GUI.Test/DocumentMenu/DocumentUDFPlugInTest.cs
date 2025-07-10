using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.BuildTools.Testing;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.GUI.DocumentMenu;
using Enterprise.DocumentEngine.GUI.RuntimeOptions;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngine.SDF;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Testing
{
	sealed class DocumentUDFPlugInTest : TestCaseWithFactory
	{
		public void TestValidateFieldName()
		{
			DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();

			var tempFileName1 = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.UDF with same name in different case 1.xls", "UDF with same name in different case 1.xls");
			var exlTemplate1 = new ExcelTemplateForUnitTesting("UDF with same name in different case 1.xls", Path.GetFullPath(tempFileName1));
			var consolTemplate1 = Factory.New<StmTemplateBase>();
			consolTemplate1.SO_DataContext = nameof(Core.Constants.DataContext.Consol);
			consolTemplate1.SO_IsSystemDefined = true;
			consolTemplate1.SO_Name = "Consol Template 1";
			consolTemplate1.SO_Template = exlTemplate1.GetAsByteArray();

			var pubDocumentCommand1 = Factory.New<DocumentCommand>();
			pubDocumentCommand1.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubDocumentCommand1.SU_IsPublished = true;
			pubDocumentCommand1.SU_IsSystemDefined = true;
			pubDocumentCommand1.SU_MenuName = "Consol Document 1";

			var pubDocumentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubDocumentPivot1.SI_SO = consolTemplate1.PK;
			pubDocumentPivot1.SI_SU = pubDocumentCommand1.PK;
			pubDocumentPivot1.SI_DocumentTitle = "Pub System Consol Document 1";

			var tempFileName2 = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.UDF with same name in different case 2.xls", "UDF with same name in different case 2.xls");
			var exlTemplate2 = new ExcelTemplateForUnitTesting("UDF with same name in different case 2.xls", Path.GetFullPath(tempFileName2));
			var consolTemplate2 = Factory.New<StmTemplateBase>();
			consolTemplate2.SO_DataContext = nameof(Core.Constants.DataContext.Consol);
			consolTemplate2.SO_IsSystemDefined = true;
			consolTemplate2.SO_Name = "Consol Template 2";
			consolTemplate2.SO_Template = exlTemplate2.GetAsByteArray();

			var pubDocumentCommand2 = Factory.New<DocumentCommand>();
			pubDocumentCommand2.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubDocumentCommand2.SU_IsPublished = true;
			pubDocumentCommand2.SU_IsSystemDefined = true;
			pubDocumentCommand2.SU_MenuName = "Consol Document 2";

			var pubDocumentPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pubDocumentPivot2.SI_SO = consolTemplate2.PK;
			pubDocumentPivot2.SI_SU = pubDocumentCommand2.PK;
			pubDocumentPivot2.SI_DocumentTitle = "Pub System Consol Document 2";

			var bizObject = Factory.New<DummyConsolBusinessObject>();

			Factory.Save();

			using (var plugIn = new DocumentUDFPlugIn(bizObject))
			{
				plugIn.OnUserControlShown();

				var note = (DocumentNote)plugIn.BusinessEntity;

				AssertEquals(5, note.UserDefinedFieldList.Count);
				AssertHasErrorContaining(((TextField)note.UserDefinedFieldList["TextField"]).ValueInfo, "TextField is duplicate with field(s) below (same name in different case):");
				AssertHasErrorContaining(((TextField)note.UserDefinedFieldList["TextField"]).ValueInfo, "'Textfield' in template 'Consol Template 1'");
				AssertHasErrorContaining(((TextField)note.UserDefinedFieldList["TextField"]).ValueInfo, "'TexTField' in template 'Consol Template 1'");

				AssertHasErrorContaining(((TextField)note.UserDefinedFieldList["Textfield"]).ValueInfo, "Textfield is duplicate with field(s) below (same name in different case):");
				AssertHasErrorContaining(((TextField)note.UserDefinedFieldList["Textfield"]).ValueInfo, "'TextField' in template 'Consol Template 2'");
				AssertHasErrorContaining(((TextField)note.UserDefinedFieldList["Textfield"]).ValueInfo, "'TexTField' in template 'Consol Template 1'");

				AssertHasErrorContaining(((TextField)note.UserDefinedFieldList["TexTField"]).ValueInfo, "TexTField is duplicate with field(s) below (same name in different case):");
				AssertHasErrorContaining(((TextField)note.UserDefinedFieldList["TexTField"]).ValueInfo, "'TextField' in template 'Consol Template 2'");
				AssertHasErrorContaining(((TextField)note.UserDefinedFieldList["TexTField"]).ValueInfo, "'Textfield' in template 'Consol Template 1'");

				var textField = note.UserDefinedFieldList["TextField"];
				Assert(((List<FilterCollectionValidator>)textField.GetType().GetField("Validators", BindingFlags.Public | BindingFlags.Instance).GetValue(textField)).Any());

				textField = note.UserDefinedFieldList["Textfield"];
				Assert(((List<FilterCollectionValidator>)textField.GetType().GetField("Validators", BindingFlags.Public | BindingFlags.Instance).GetValue(textField)).Any());

				textField = note.UserDefinedFieldList["TexTField"];
				Assert(((List<FilterCollectionValidator>)textField.GetType().GetField("Validators", BindingFlags.Public | BindingFlags.Instance).GetValue(textField)).Any());

				var normalField = note.UserDefinedFieldList["Normal Field"];
				Assert(!((List<FilterCollectionValidator>)normalField.GetType().GetField("Validators", BindingFlags.Public | BindingFlags.Instance).GetValue(normalField)).Any());

				var yetAnotherField = note.UserDefinedFieldList["Normal Field"];
				Assert(!((List<FilterCollectionValidator>)yetAnotherField.GetType().GetField("Validators", BindingFlags.Public | BindingFlags.Instance).GetValue(yetAnotherField)).Any());
			}
		}

		[RequiresSTA]
		public void TestDeletingUDFNote()
		{
			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";

			StmTemplateBase sysConsolTemplate = Factory.New<StmTemplateBase>();
			sysConsolTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysConsolTemplate.SO_IsSystemDefined = true;
			sysConsolTemplate.SO_Name = "System Consol Template";
			sysConsolTemplate.SO_Template = UDFWithoutTabs.GetAsByteArray();

			var pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Consol Document";
			pubSysConsolMenu.SU_MenuIndex = 1;
			pubSysConsolMenu.SU_MenuPath = "";
			pubSysConsolMenu.SU_MenuShortcut = "CtrlF2";

			StmMenuTemplatePivotBase pubSysConsolPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot.SI_SO = sysConsolTemplate.PK;
			pubSysConsolPivot.SI_SU = pubSysConsolMenu.PK;
			pubSysConsolPivot.SI_DocumentTitle = "Pub System Consol Document";

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyConsolBusinessObject dummyConsol = factory2.Load<DummyConsolBusinessObject>(dummyConsol1.PK);
			AssertEquals("DocumentCommands.Count", 1, dummyConsol.DocumentCommands.Count);
			AssertEquals("MenuName0", pubSysConsolMenu.SU_MenuName, dummyConsol.DocumentCommands[0].SU_MenuName);

			using (TestForm testForm = new TestForm(dummyConsol))
			{
				testForm.Size = new System.Drawing.Size(800, 600);
				testForm.PlugIns.Add(ControllerIDs.DocumentUDFPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				DocumentNote note = (DocumentNote)testForm.PlugIns.Instances[0].BusinessEntity;
				note.SystemDefinedFieldWrappers[0].S1_Value = "changed";
				factory2.Save();

				AssertEquals("Note.IsInDatabase", true, note.IsInDatabase);
			}
			using (TestForm testForm = new TestForm(dummyConsol))
			{
				testForm.Size = new System.Drawing.Size(800, 600);
				testForm.MyTabControl.TabPages.Add(new ZTabPage());
				testForm.PlugIns.Add(ControllerIDs.DocumentUDFPlugIn);
				testForm.DisplayMode = ODisplayMode.Delete;

				testForm.Show();
				UserIdleWorker.Flush();

				testForm.TestDelete();

				DocumentNote note = (DocumentNote)testForm.PlugIns.Instances[0].BusinessEntity;

				AssertEquals("Note.IsDeleted", true, note.IsDeleted);
				AssertEquals("Note.IsInDatabase", false, note.IsInDatabase);
			}
		}

		[RequiresSTA]
		public void TestWhenUDFAllAppearOnMiscTabShouldStillAlignProperly()
		{
			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";

			StmTemplateBase sysConsolTemplate = Factory.New<StmTemplateBase>();
			sysConsolTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysConsolTemplate.SO_IsSystemDefined = true;
			sysConsolTemplate.SO_Name = "System Consol Template";
			sysConsolTemplate.SO_Template = UDFWithoutTabs.GetAsByteArray();

			DocumentCommand pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Consol Document";
			pubSysConsolMenu.SU_MenuIndex = 1;
			pubSysConsolMenu.SU_MenuPath = "";
			pubSysConsolMenu.SU_MenuShortcut = "CtrlF2";

			StmMenuTemplatePivotBase pubSysConsolPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot.SI_SO = sysConsolTemplate.PK;
			pubSysConsolPivot.SI_SU = pubSysConsolMenu.PK;
			pubSysConsolPivot.SI_DocumentTitle = "Pub System Consol Document";

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyConsolBusinessObject dummyConsol = factory2.Load<DummyConsolBusinessObject>(dummyConsol1.PK);
			AssertEquals("DocumentCommands.Count", 1, dummyConsol.DocumentCommands.Count);
			AssertEquals("MenuName0", pubSysConsolMenu.SU_MenuName, dummyConsol.DocumentCommands[0].SU_MenuName);

			using (TestForm testForm = new TestForm(dummyConsol))
			{
				testForm.Size = ControlDpiScalingHelper.NewScaledSize(800, 600, true);
				testForm.PlugIns.Add(ControllerIDs.DocumentUDFPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				ZTemplateTabControl mainTabControl = testForm.MyTabControl.TabPages[0].Controls[0].Controls[0] as ZTemplateTabControl;
				AssertNotNull("MainTabControl", mainTabControl);

				AssertEquals("TabPages.Count", 1, mainTabControl.TabPages.Count);

				AssertEquals("Page1 Text", "Miscellaneous", mainTabControl.TabPages[0].Text);
				AssertEquals("Misc tab page controls", 5, mainTabControl.TabPages[0].Controls.Count);

				float controlDistance = ControlDpiScalingHelper.ScaleToCurrentDpiX(144);
				AssertEquals("Bottom of last tabpage", controlDistance, mainTabControl.TabPages[0].Controls[4].Bottom, 5);
			}
		}

		[RequiresSTA]
		public void TestSynchroniseShouldOnlyShowConsolUDFsWhenConsolHasShipments()
		{
			#region DummyConsol and DummyShipment

			DummyShipmentBusinessObject dummyShipment1 = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment1.Z0_Code = "DS1";
			dummyShipment1.Z0_FK_Code = "DC1";

			DummyShipmentBusinessObject dummyShipment2 = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment2.Z0_Code = "DS2";
			dummyShipment2.Z0_FK_Code = "DC1";

			DummyShipmentBusinessObject dummyShipment3 = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment3.Z0_Code = "DS3";
			dummyShipment3.Z0_FK_Code = "DC1";

			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";

			#endregion

			#region Templates

			StmTemplateBase sysConsolTemplate = Factory.New<StmTemplateBase>();
			sysConsolTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysConsolTemplate.SO_IsSystemDefined = true;
			sysConsolTemplate.SO_Name = "System Consol Template1";
			sysConsolTemplate.SO_Template = UDFWithTabs.GetAsByteArray();

			StmTemplateBase sysConsolTemplate2 = Factory.New<StmTemplateBase>();
			sysConsolTemplate2.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysConsolTemplate2.SO_IsSystemDefined = true;
			sysConsolTemplate2.SO_Name = "System Consol Template2";
			var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with tabs2.xls", "UDF with tabs2.xls");
			sysConsolTemplate2.SO_Template = new ExcelTemplateForUnitTesting("UDF with tabs2.xls", Path.GetFullPath(tempFileName)).GetAsByteArray();

			StmTemplateBase sysShipmentTemplate = Factory.New<StmTemplateBase>();
			sysShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";
			sysShipmentTemplate.SO_Template = UDFWithTabs3.GetAsByteArray();
			#endregion

			#region Published System Shipment

			DocumentCommand pubSysShipmentMenu = Factory.New<DocumentCommand>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			pubSysShipmentMenu.SU_MenuIndex = 1;
			pubSysShipmentMenu.SU_MenuPath = "";
			pubSysShipmentMenu.SU_MenuShortcut = "CtrlF1";
			pubSysShipmentMenu.SU_ContactType = ContactType.Consignee.ToString();

			StmMenuTemplatePivotBase pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Pub System Shipment Document";

			#endregion

			#region Published System Consol

			DocumentCommand pubSysConsolMenuPrintSet = Factory.New<DocumentCommand>();
			pubSysConsolMenuPrintSet.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenuPrintSet.SU_IsPublished = true;
			pubSysConsolMenuPrintSet.SU_IsSystemDefined = true;
			pubSysConsolMenuPrintSet.SU_MenuName = "Consol Print Set";
			pubSysConsolMenuPrintSet.SU_MenuIndex = 3;
			pubSysConsolMenuPrintSet.SU_MenuPath = "";
			pubSysConsolMenuPrintSet.SU_MenuShortcut = "CtrlF2";

			StmMenuMenuPivot menuPivot1 = Factory.New<StmMenuMenuPivot>();
			menuPivot1.SF_SU_Inward = pubSysConsolMenuPrintSet.PK;
			menuPivot1.SF_SU_Outward = pubSysShipmentMenu.PK;

			DocumentCommand pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Consol Document";
			pubSysConsolMenu.SU_MenuIndex = 1;
			pubSysConsolMenu.SU_MenuPath = "";
			pubSysConsolMenu.SU_MenuShortcut = "CtrlF2";

			StmMenuTemplatePivotBase pubSysConsolPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot.SI_SO = sysConsolTemplate.PK;
			pubSysConsolPivot.SI_SU = pubSysConsolMenu.PK;
			pubSysConsolPivot.SI_DocumentTitle = "Pub System Consol Document";

			DocumentCommand pubSysConsolMenu2 = Factory.New<DocumentCommand>();
			pubSysConsolMenu2.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu2.SU_IsPublished = true;
			pubSysConsolMenu2.SU_IsSystemDefined = true;
			pubSysConsolMenu2.SU_MenuName = "Consol Document2";
			pubSysConsolMenu2.SU_MenuIndex = 2;
			pubSysConsolMenu2.SU_MenuPath = "";
			pubSysConsolMenu2.SU_MenuShortcut = "CtrlF2";

			StmMenuTemplatePivotBase pubSysConsolPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot2.SI_SO = sysConsolTemplate2.PK;
			pubSysConsolPivot2.SI_SU = pubSysConsolMenu2.PK;
			pubSysConsolPivot2.SI_DocumentTitle = "Pub System Consol Document2";

			#endregion

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyConsolBusinessObject dummyConsol = factory2.Load<DummyConsolBusinessObject>(dummyConsol1.PK);
			AssertEquals("DocumentCommands.Count", 3, dummyConsol.DocumentCommands.Count);
			AssertEquals("MenuName0", pubSysConsolMenu.SU_MenuName, dummyConsol.DocumentCommands[0].SU_MenuName);
			AssertEquals("MenuName1", pubSysConsolMenu2.SU_MenuName, dummyConsol.DocumentCommands[1].SU_MenuName);
			AssertEquals("MenuName2", pubSysConsolMenuPrintSet.SU_MenuName, dummyConsol.DocumentCommands[2].SU_MenuName);

			using (TestForm testForm = new TestForm(dummyConsol))
			{
				testForm.MinimumSize = ControlDpiScalingHelper.NewScaledSize(800, 185);
				testForm.PlugIns.Add(ControllerIDs.DocumentUDFPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				ZTemplateTabControl mainTabControl = testForm.MyTabControl.TabPages[0].Controls[0].Controls[0] as ZTemplateTabControl;
				AssertNotNull("MainTabControl", mainTabControl);

				AssertEquals("TabPages.Count", 3, mainTabControl.TabPages.Count);

				//Alpha tab
				AssertEquals("Page1 Text", "Alpha", mainTabControl.TabPages[0].Text);
				AssertEquals("Alpha tab page controls", 4, mainTabControl.TabPages[0].Controls.Count);

				TextFieldUserControl iAmOnAlphaTextEdit = mainTabControl.TabPages[0].Controls[1] as TextFieldUserControl;
				AssertNotNull("IAmOnAlphaTextEdit", iAmOnAlphaTextEdit);
				AssertEquals("IAmOnAlphaTextEdit.Location", ControlDpiScalingHelper.NewScaledPoint(8, 16), iAmOnAlphaTextEdit.Location);

				TextFieldUserControl iAmOnAlpha2TextEdit = mainTabControl.TabPages[0].Controls[2] as TextFieldUserControl;
				AssertNotNull("IAmOnAlpha2TextEdit", iAmOnAlpha2TextEdit);
				// Need to leave some room for rounding errors
				var expectedLocation = ControlDpiScalingHelper.NewScaledPoint(8, 42);
				Assert("IAmOnAlpha2TextEdit.Location.X", expectedLocation.X - 5 < iAmOnAlpha2TextEdit.Location.X && iAmOnAlpha2TextEdit.Location.X < expectedLocation.X + 5);
				Assert("IAmOnAlpha2TextEdit.Location.Y", expectedLocation.Y - 5 < iAmOnAlpha2TextEdit.Location.Y && iAmOnAlpha2TextEdit.Location.Y < expectedLocation.Y + 5);

				TextFieldUserControl iAmOnBothAlphaTextEdit = mainTabControl.TabPages[0].Controls[3] as TextFieldUserControl;
				AssertNotNull("IAmOnBothAlphaTextEdit", iAmOnBothAlphaTextEdit);
				expectedLocation = ControlDpiScalingHelper.NewScaledPoint(8, 68);
				Assert("IAmOnBothAlphaTextEdit.Location.X", expectedLocation.X - 5 < iAmOnBothAlphaTextEdit.Location.X && iAmOnBothAlphaTextEdit.Location.X < expectedLocation.X + 5);
				Assert("IAmOnBothAlphaTextEdit.Location.Y", expectedLocation.Y - 5 < iAmOnBothAlphaTextEdit.Location.Y && iAmOnBothAlphaTextEdit.Location.Y < expectedLocation.Y + 5);

				//Beta tab
				AssertEquals("Page2 Text", "Beta", mainTabControl.TabPages[1].Text);
				AssertEquals("Beta tab page controls", 4, mainTabControl.TabPages[1].Controls.Count);

				DateFieldUserControl iAmOnBetaDateEdit = mainTabControl.TabPages[1].Controls[1] as DateFieldUserControl;
				AssertNotNull("IAmOnBetaDateEdit", iAmOnBetaDateEdit);

				TextFieldUserControl iAmOnBetaTooTextEdit = mainTabControl.TabPages[1].Controls[2] as TextFieldUserControl;
				AssertNotNull("IAmOnBetaTooTextEdit", iAmOnBetaTooTextEdit);
				Assert("TextEdit is below DateEdit", iAmOnBetaTooTextEdit.Location.Y > iAmOnBetaDateEdit.Location.Y);

				TextFieldUserControl iAmOnBothBetaTextEdit = mainTabControl.TabPages[1].Controls[3] as TextFieldUserControl;
				AssertNotNull("IAmOnBothBetaTextEdit", iAmOnBothBetaTextEdit);

				//Miscellaneous tab
				AssertEquals("Page0 Text", "Miscellaneous", mainTabControl.TabPages[2].Text);
				AssertEquals("Misc tab page controls", 2, mainTabControl.TabPages[2].Controls.Count);
				TextFieldUserControl iAmNotOnTabPageTextEdit = mainTabControl.TabPages[2].Controls[0] as TextFieldUserControl;
				AssertNotNull(iAmNotOnTabPageTextEdit);

				TextFieldUserControl iAmBrandNew = mainTabControl.TabPages[2].Controls[1] as TextFieldUserControl;
				AssertNotNull(iAmBrandNew);

				ZPlugIn[] plugIns = testForm.PlugIns.Instances;

				AssertEquals("HasChanges", false, plugIns[0].BusinessEntity.HasChanges);

				mainTabControl.SelectedTab = (ZTabPage)mainTabControl.TabPages[0];
				mainTabControl.SelectedTab = (ZTabPage)mainTabControl.TabPages[1];
				mainTabControl.SelectedTab = (ZTabPage)mainTabControl.TabPages[2];
				((BusinessObject)iAmNotOnTabPageTextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"] = "IAmNotOnTabPageTextEdit";
				((BusinessObject)iAmOnAlphaTextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"] = "IAmOnAlphaTextEdit";
				((BusinessObject)iAmOnAlpha2TextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"] = "IAmOnAlpha2TextEdit";
				((BusinessObject)iAmOnBetaDateEdit.FieldDateEdit_Exposed.ValueDateEdit_Exposed.DataBindings["DateTimeValue"].DataSource)["Value"] = new ZDateTime(2000, 1, 1);

				((BusinessObject)iAmOnBetaTooTextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"] = "IAmOnBetaTooTextEdit";
				((BusinessObject)iAmOnBothAlphaTextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"] = "IAmOnBothTextEdit";

				((BusinessObject)iAmBrandNew.FieldTextBox.DataBindings["Text"].DataSource)["Value"] = "IAmBrandNew";

				AssertEquals("HasChanges", true, plugIns[0].BusinessEntity.HasChanges);

				plugIns[0].BusinessEntity.RunPreSaveValidation();
				BusinessObject bizO = (BusinessObject)plugIns[0].BusinessEntity;
				AssertEquals("HasErrors", false, bizO.HasErrors);
				plugIns[0].Factory.Save();
			}

			AssertEquals("HasChanges", false, dummyConsol.HasChanges);

			using (TestForm testForm = new TestForm(dummyConsol))
			{
				testForm.PlugIns.Add(ControllerIDs.DocumentUDFPlugIn);
				testForm.Show();
				UserIdleWorker.Flush();

				ZTemplateTabControl mainTabControl = testForm.MyTabControl.TabPages[0].Controls[0].Controls[0] as ZTemplateTabControl;

				TextFieldUserControl iAmOnAlphaTextEdit = mainTabControl.TabPages[0].Controls[1] as TextFieldUserControl;
				TextFieldUserControl iAmOnAlpha2TextEdit = mainTabControl.TabPages[0].Controls[2] as TextFieldUserControl;
				DateFieldUserControl iAmOnBetaDateEdit = mainTabControl.TabPages[1].Controls[1] as DateFieldUserControl;
				TextFieldUserControl iAmOnBetaTooTextEdit = mainTabControl.TabPages[1].Controls[2] as TextFieldUserControl;
				TextFieldUserControl iAmOnBothBetaTextEdit = mainTabControl.TabPages[1].Controls[3] as TextFieldUserControl;

				TextFieldUserControl iAmOnBothAlphaTextEdit = mainTabControl.TabPages[0].Controls[3] as TextFieldUserControl;

				TextFieldUserControl iAmNotOnTabPageTextEdit = mainTabControl.TabPages[2].Controls[0] as TextFieldUserControl;
				TextFieldUserControl iAmBrandNew = mainTabControl.TabPages[2].Controls[1] as TextFieldUserControl;

				mainTabControl.SelectedTab = (ZTabPage)mainTabControl.TabPages[0];
				mainTabControl.SelectedTab = (ZTabPage)mainTabControl.TabPages[1];
				mainTabControl.SelectedTab = (ZTabPage)mainTabControl.TabPages[2];
				AssertEquals("IAmNotOnTabPageTextEdit", new ZString("IAmNotOnTabPageTextEdit"), ((BusinessObject)iAmNotOnTabPageTextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"]);
				AssertEquals("IAmOnAlphaTextEdit", new ZString("IAmOnAlphaTextEdit"), ((BusinessObject)iAmOnAlphaTextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"]);
				AssertEquals("IAmOnAlpha2TextEdit", new ZString("IAmOnAlpha2TextEdit"), ((BusinessObject)iAmOnAlpha2TextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"]);
				AssertEquals("IAmOnBetaDateEdit", new ZDateTime(2000, 1, 1), ((BusinessObject)iAmOnBetaDateEdit.FieldDateEdit_Exposed.ValueDateEdit_Exposed.DataBindings["DateTimeValue"].DataSource)["Value"]);

				AssertEquals("IAmOnBetaTooTextEdit", new ZString("IAmOnBetaTooTextEdit"), ((BusinessObject)iAmOnBetaTooTextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"]);
				AssertEquals("IAmOnBothAlphaTextEdit", new ZString("IAmOnBothTextEdit"), ((BusinessObject)iAmOnBothAlphaTextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"]);
				AssertEquals("IAmOnBothBetaTextEdit", new ZString("IAmOnBothTextEdit"), ((BusinessObject)iAmOnBothBetaTextEdit.FieldTextBox.DataBindings["Text"].DataSource)["Value"]);

				AssertEquals("IAmBrandNew", new ZString("IAmBrandNew"), ((BusinessObject)iAmBrandNew.FieldTextBox.DataBindings["Text"].DataSource)["Value"]);
			}
		}

		[RequiresSTA]
		public void TestHasChangesBehaviour()
		{
			TestCaseHelper.ClearTable(StmSystemDefinedFieldSchema.Constants.TableName);

			#region DummyConsol and DummyShipment

			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";

			#endregion

			#region Templates

			StmTemplateBase sysConsolTemplate = Factory.New<StmTemplateBase>();
			sysConsolTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysConsolTemplate.SO_IsSystemDefined = true;
			sysConsolTemplate.SO_Name = "System Consol Template";
			sysConsolTemplate.SO_Template = UDFWithTabs.GetAsByteArray();

			#endregion

			#region Published System Consol

			DocumentCommand pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Consol Document";
			pubSysConsolMenu.SU_MenuIndex = 1;
			pubSysConsolMenu.SU_MenuPath = "";
			pubSysConsolMenu.SU_MenuShortcut = "CtrlF2";

			StmMenuTemplatePivotBase pubSysConsolPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot.SI_SO = sysConsolTemplate.PK;
			pubSysConsolPivot.SI_SU = pubSysConsolMenu.PK;
			pubSysConsolPivot.SI_DocumentTitle = "Pub System Consol Document";

			#endregion

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyConsolBusinessObject dummyConsol = factory2.Load<DummyConsolBusinessObject>(dummyConsol1.PK);
			AssertEquals("DocumentCommands.Count", 1, dummyConsol.DocumentCommands.Count);
			AssertEquals("MenuName0", pubSysConsolMenu.SU_MenuName, dummyConsol.DocumentCommands[0].SU_MenuName);

			using (TestForm testForm = new TestForm(dummyConsol))
			{
				testForm.PlugIns.Add(ControllerIDs.DocumentUDFPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				ZTemplateTabControl mainTabControl = testForm.MyTabControl.TabPages[0].Controls[0].Controls[0] as ZTemplateTabControl;
				TextFieldUserControl iAmOnAlphaTextEdit = mainTabControl.TabPages[0].Controls[1] as TextFieldUserControl;

				ZPlugIn[] plugIns = testForm.PlugIns.Instances;

				AssertEquals("HasChanges", false, plugIns[0].BusinessEntity.HasChanges);

				//The following tests a bug where if you open a shipment and click on the UDF tab and then try to edit the attached consol, it won't let you because load UDF adds a temp Note object that
				//makes the ModuleButtonsGrid think that the Shipment is not in Database.
				AssertEquals("HasChanges", false, dummyConsol.HasChanges);
				AssertEquals("IsInDatabaseIncludingChildren", true, dummyConsol.IsInDatabaseIncludingChildren);

				((TextField)iAmOnAlphaTextEdit.FieldTextBox.DataBindings[0].DataSource).Value = "Changed";

				AssertEquals("HasChanges", true, dummyConsol.HasChanges);
				AssertEquals("IsInDatabaseIncludingChildren", false, dummyConsol.IsInDatabaseIncludingChildren);

				dummyConsol.Factory.Save();
				AssertEquals("HasChanges", false, dummyConsol.HasChanges);
				AssertEquals("IsInDatabaseIncludingChildren", true, dummyConsol.IsInDatabaseIncludingChildren);
			}
		}

		[RequiresSTA]
		public void TestHasChangesFiresProperlyAfterMenuHasBeenOpenedFirst()
		{
			TestCaseHelper.ClearTable(StmSystemDefinedFieldSchema.Constants.TableName);

			#region Templates

			StmTemplateBase sysConsolTemplate = Factory.New<StmTemplateBase>();
			sysConsolTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysConsolTemplate.SO_IsSystemDefined = true;
			sysConsolTemplate.SO_Name = "System Consol Template";
			sysConsolTemplate.SO_Template = UDFWithTabs.GetAsByteArray();

			#endregion

			#region Published System Consol

			DocumentCommand pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Consol Document";
			pubSysConsolMenu.SU_MenuIndex = 1;
			pubSysConsolMenu.SU_MenuPath = "";
			pubSysConsolMenu.SU_MenuShortcut = "CtrlF2";

			StmMenuTemplatePivotBase pubSysConsolPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot.SI_SO = sysConsolTemplate.PK;
			pubSysConsolPivot.SI_SU = pubSysConsolMenu.PK;
			pubSysConsolPivot.SI_DocumentTitle = "Pub System Consol Document";

			#endregion

			Factory.Save();
			#region DummyConsol and DummyShipment

			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";
			DocumentNote consol1Note = DocumentNote.LoadNote(dummyConsol1);
			((FilterFieldValueSerialisable)consol1Note.UserDefinedFieldList["I'm on alpha"]).ValueAsStringForSerialisation = "test";

			#endregion

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyConsolBusinessObject dummyConsol = factory2.Load<DummyConsolBusinessObject>(dummyConsol1.PK);
			AssertEquals("DocumentCommands.Count", 1, dummyConsol.DocumentCommands.Count);
			AssertEquals("MenuName0", pubSysConsolMenu.SU_MenuName, dummyConsol.DocumentCommands[0].SU_MenuName);

			using (TestForm testForm = new TestForm(dummyConsol))
			{
				ZTabPage blankPage = new ZTabPage();
				testForm.MyTabControl.TabPages.Add(blankPage);
				testForm.PlugIns.Add(ControllerIDs.DocumentUDFPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				ZPlugIn[] plugIns = testForm.PlugIns.Instances;

				plugIns[0].SelectTabPage();

				ZTemplateTabControl mainTabControl = testForm.MyTabControl.TabPages[1].Controls[0].Controls[0] as ZTemplateTabControl;

				TextFieldUserControl iAmOnAlphaTextEdit = mainTabControl.TabPages[0].Controls[1] as TextFieldUserControl;

				AssertEquals("HasChanges", false, plugIns[0].BusinessEntity.HasChanges);

				AssertEquals("HasChanges", false, dummyConsol.HasChanges);
				AssertEquals("IsInDatabaseIncludingChildren", true, dummyConsol.IsInDatabaseIncludingChildren);

				((TextField)iAmOnAlphaTextEdit.FieldTextBox.DataBindings[0].DataSource).Value = "Changed";

				AssertEquals("HasChanges", true, dummyConsol.HasChanges);
				AssertEquals("All children with changes are non-persistent, so IsInDatabaseIncludingChildren should be true.", true, dummyConsol.IsInDatabaseIncludingChildren);

				dummyConsol.Factory.Save();
				AssertEquals("HasChanges", false, dummyConsol.HasChanges);
				AssertEquals("IsInDatabaseIncludingChildren", true, dummyConsol.IsInDatabaseIncludingChildren);
			}
		}

		[RequiresSTA]
		public void TestChildMenusUDFsAreLoadedProperly()
		{
			#region Templates

			StmTemplateBase sysShipmentTemplate = Factory.New<StmTemplateBase>();
			sysShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";
			sysShipmentTemplate.SO_Template = UDFWithTabs3.GetAsByteArray();

			#endregion

			#region Published System Shipment

			DocumentCommand pubSysShipmentMenu = Factory.New<DocumentCommand>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			pubSysShipmentMenu.SU_MenuIndex = 1;
			pubSysShipmentMenu.SU_MenuPath = "";
			pubSysShipmentMenu.SU_MenuShortcut = "CtrlF1";
			pubSysShipmentMenu.SU_ContactType = ContactType.Consignee.ToString();

			StmMenuTemplatePivotBase pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Pub System Shipment Document";

			#endregion

			#region Published System Consol

			DocumentCommand pubSysConsolMenuPrintSet = Factory.New<DocumentCommand>();
			pubSysConsolMenuPrintSet.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenuPrintSet.SU_IsPublished = true;
			pubSysConsolMenuPrintSet.SU_IsSystemDefined = true;
			pubSysConsolMenuPrintSet.SU_MenuName = "Consol Print Set";
			pubSysConsolMenuPrintSet.SU_MenuIndex = 3;
			pubSysConsolMenuPrintSet.SU_MenuPath = "";
			pubSysConsolMenuPrintSet.SU_MenuShortcut = "CtrlF2";

			StmMenuMenuPivot menuPivot1 = Factory.New<StmMenuMenuPivot>();
			menuPivot1.SF_SU_Inward = pubSysConsolMenuPrintSet.PK;
			menuPivot1.SF_SU_Outward = pubSysShipmentMenu.PK;

			#endregion

			Factory.Save();

			#region DummyConsol and DummyShipment

			DummyShipmentBusinessObject dummyShipment1 = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment1.Z0_Code = "DS1";
			dummyShipment1.Z0_FK_Code = "DC1";
			DocumentNote dummyShipment1Note = DocumentNote.LoadNote(dummyShipment1);
			((FilterFieldValueSerialisable)dummyShipment1Note.UserDefinedFieldList["I'm on alpha shipment"]).ValueAsStringForSerialisation = "Shipment1";

			DummyShipmentBusinessObject dummyShipment2 = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment2.Z0_Code = "DS2";
			dummyShipment2.Z0_FK_Code = "DC1";
			var dummyShipment2Note = DocumentNote.LoadNote(dummyShipment2);
			((FilterFieldValueSerialisable)dummyShipment2Note.UserDefinedFieldList["I'm on alpha shipment"]).ValueAsStringForSerialisation = "Shipment2";

			DummyShipmentBusinessObject dummyShipment3 = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment3.Z0_Code = "DS3";
			dummyShipment3.Z0_FK_Code = "DC1";
			DocumentNote dummyShipment3Note = DocumentNote.LoadNote(dummyShipment3);
			((FilterFieldValueSerialisable)dummyShipment3Note.UserDefinedFieldList["I'm on alpha shipment"]).ValueAsStringForSerialisation = "Shipment3";

			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";

			#endregion

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyConsolBusinessObject dummyConsol = factory2.Load<DummyConsolBusinessObject>(dummyConsol1.PK);
			AssertEquals("DocumentCommands.Count", 1, dummyConsol.DocumentCommands.Count);
			AssertEquals("MenuName2", pubSysConsolMenuPrintSet.SU_MenuName, dummyConsol.DocumentCommands[0].SU_MenuName);

			using (TestForm testForm = new TestForm(dummyConsol))
			{
				testForm.MinimumSize = new System.Drawing.Size(800, 185);
				testForm.PlugIns.Add(ControllerIDs.DocDataPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				ZPlugIn plugIn = testForm.PlugIns.Instances[0];
				ZDocumentMenuItem docMenu = plugIn.TopLevelMenu as ZDocumentMenuItem;
				docMenu.HelperForTesting.ShouldActuallyRunDocumentSetForTesting = false;
				docMenu.PerformClick();
				AssertEquals("MenuItems.Count under DocMenu", 10, docMenu.MenuItems.Count);

				docMenu.MenuItems[0].PerformClick();
				AssertEquals("LastRunDocumentSet.Count", 3, docMenu.HelperForTesting.LastRunReportInfos.Count);

				AssertEquals("UDF for Shipment1", "Shipment1", ((FilterFieldValueSerialisable)docMenu.HelperForTesting.LastRunReportInfos[0].UserDefinedFieldValueList["I'm on alpha shipment"]).ValueAsStringForSerialisation);
				AssertEquals("UDF for Shipment2", "Shipment2", ((FilterFieldValueSerialisable)docMenu.HelperForTesting.LastRunReportInfos[1].UserDefinedFieldValueList["I'm on alpha shipment"]).ValueAsStringForSerialisation);
				AssertEquals("UDF for Shipment3", "Shipment3", ((FilterFieldValueSerialisable)docMenu.HelperForTesting.LastRunReportInfos[2].UserDefinedFieldValueList["I'm on alpha shipment"]).ValueAsStringForSerialisation);
			}
		}

		[RequiresSTA]
		public void TestSDFsAreLoadedProperlyInChildrenAlongWithUDFs()
		{
			#region SDF

			TestCaseHelper.ClearTable(StmSystemDefinedField.Schema.TableName);

			StmSystemDefinedFieldCollection shipmentSDFCollection = new StmSystemDefinedFieldCollection(Factory);
			StmSystemDefinedField shipmentField = shipmentSDFCollection.AddNew();
			shipmentField.S1_Name = "ShipmentField";
			shipmentField.S1_BusinessContext = nameof(BusinessContext.Shipment);

			StmSystemDefinedFieldCollection consolSDFCollection = new StmSystemDefinedFieldCollection(Factory);
			StmSystemDefinedField consolField = consolSDFCollection.AddNew();
			consolField.S1_Name = "ConsolField";
			consolField.S1_BusinessContext = nameof(BusinessContext.Consol);

			#endregion

			#region Templates

			StmTemplateBase sysShipmentTemplate = Factory.New<StmTemplateBase>();
			sysShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";
			sysShipmentTemplate.SO_Template = UDFWithTabs3.GetAsByteArray();

			#endregion

			#region Published System Shipment

			DocumentCommand pubSysShipmentMenu = Factory.New<DocumentCommand>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			pubSysShipmentMenu.SU_MenuIndex = 1;
			pubSysShipmentMenu.SU_MenuPath = "";
			pubSysShipmentMenu.SU_MenuShortcut = "CtrlF1";
			pubSysShipmentMenu.SU_ContactType = ContactType.Consignee.ToString();

			StmMenuTemplatePivotBase pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Pub System Shipment Document";

			#endregion

			#region Published System Consol

			DocumentCommand pubSysConsolMenuPrintSet = Factory.New<DocumentCommand>();
			pubSysConsolMenuPrintSet.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenuPrintSet.SU_IsPublished = true;
			pubSysConsolMenuPrintSet.SU_IsSystemDefined = true;
			pubSysConsolMenuPrintSet.SU_MenuName = "Consol Print Set";
			pubSysConsolMenuPrintSet.SU_MenuIndex = 3;
			pubSysConsolMenuPrintSet.SU_MenuPath = "";
			pubSysConsolMenuPrintSet.SU_MenuShortcut = "CtrlF2";

			StmMenuMenuPivot menuPivot1 = Factory.New<StmMenuMenuPivot>();
			menuPivot1.SF_SU_Inward = pubSysConsolMenuPrintSet.PK;
			menuPivot1.SF_SU_Outward = pubSysShipmentMenu.PK;

			#endregion

			Factory.Save();

			#region DummyConsol and DummyShipment

			//Shipment 1
			DummyShipmentBusinessObject dummyShipment1 = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment1.Z0_Code = "DS1";
			dummyShipment1.Z0_FK_Code = "DC1";

			DocumentNote dummyShipment1Note = DocumentNote.LoadNote(dummyShipment1);
			dummyShipment1Note.FilteredSystemDefinedFieldWrappers[0].S1_Value = "ShipmentSDF1";
			((FilterFieldValueSerialisable)dummyShipment1Note.UserDefinedFieldList["I'm on alpha shipment"]).ValueAsStringForSerialisation = "Shipment1";

			//Shipment 2
			DummyShipmentBusinessObject dummyShipment2 = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment2.Z0_Code = "DS2";
			dummyShipment2.Z0_FK_Code = "DC1";

			DocumentNote dummyShipment2Note = DocumentNote.LoadNote(dummyShipment2);
			((FilterFieldValueSerialisable)dummyShipment2Note.UserDefinedFieldList["I'm on alpha shipment"]).ValueAsStringForSerialisation = "Shipment2";
			dummyShipment2Note.FilteredSystemDefinedFieldWrappers[0].S1_Value = "ShipmentSDF2";

			//Shipment 3
			DummyShipmentBusinessObject dummyShipment3 = Factory.New<DummyShipmentBusinessObject>();
			dummyShipment3.Z0_Code = "DS3";
			dummyShipment3.Z0_FK_Code = "DC1";

			DocumentNote dummyShipment3Note = DocumentNote.LoadNote(dummyShipment3);
			((FilterFieldValueSerialisable)dummyShipment3Note.UserDefinedFieldList["I'm on alpha shipment"]).ValueAsStringForSerialisation = "Shipment3";
			dummyShipment3Note.FilteredSystemDefinedFieldWrappers[0].S1_Value = "ShipmentSDF3";

			//Consol 1
			DummyConsolBusinessObject dummyConsol1 = Factory.New<DummyConsolBusinessObject>();
			dummyConsol1.Z0_Code = "DC1";

			DocumentNote dummyConsol1Note = DocumentNote.LoadNote(dummyConsol1);
			dummyConsol1Note.FilteredSystemDefinedFieldWrappers[0].S1_Value = "ConsolSDF1";

			#endregion

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyConsolBusinessObject dummyConsol = factory2.Load<DummyConsolBusinessObject>(dummyConsol1.PK);
			AssertEquals("DocumentCommands.Count", 1, dummyConsol.DocumentCommands.Count);
			AssertEquals("MenuName2", pubSysConsolMenuPrintSet.SU_MenuName, dummyConsol.DocumentCommands[0].SU_MenuName);

			using (TestForm testForm = new TestForm(dummyConsol))
			{
				testForm.MinimumSize = new System.Drawing.Size(800, 185);
				testForm.PlugIns.Add(ControllerIDs.DocDataPlugIn);

				testForm.Show();
				UserIdleWorker.Flush();

				ZPlugIn plugIn = testForm.PlugIns.Instances[0];
				ZDocumentMenuItem docMenu = plugIn.TopLevelMenu as ZDocumentMenuItem;
				docMenu.HelperForTesting.ShouldActuallyRunDocumentSetForTesting = false;
				docMenu.PerformClick();
				AssertEquals("MenuItems.Count under DocMenu", 10, docMenu.MenuItems.Count);
				docMenu.MenuItems[0].PerformClick();
				AssertEquals("DocMenu.LastRunReportInfos.Count", 3, docMenu.HelperForTesting.LastRunReportInfos.Count);

				AssertEquals("UDF for Shipment1", "Shipment1", ((FilterFieldValueSerialisable)docMenu.HelperForTesting.LastRunReportInfos[0].UserDefinedFieldValueList["I'm on alpha shipment"]).ValueAsStringForSerialisation);
				AssertEquals("UDF for Shipment2", "Shipment2", ((FilterFieldValueSerialisable)docMenu.HelperForTesting.LastRunReportInfos[1].UserDefinedFieldValueList["I'm on alpha shipment"]).ValueAsStringForSerialisation);
				AssertEquals("UDF for Shipment3", "Shipment3", ((FilterFieldValueSerialisable)docMenu.HelperForTesting.LastRunReportInfos[2].UserDefinedFieldValueList["I'm on alpha shipment"]).ValueAsStringForSerialisation);

				AssertEquals("SDF for Shipment1", "ShipmentSDF1", ((FilterFieldValueSerialisable)docMenu.HelperForTesting.LastRunReportInfos[0].UserDefinedFieldValueList["ShipmentField"]).ValueAsStringForSerialisation);
				AssertEquals("SDF for Shipment2", "ShipmentSDF2", ((FilterFieldValueSerialisable)docMenu.HelperForTesting.LastRunReportInfos[1].UserDefinedFieldValueList["ShipmentField"]).ValueAsStringForSerialisation);
				AssertEquals("SDF for Shipment3", "ShipmentSDF3", ((FilterFieldValueSerialisable)docMenu.HelperForTesting.LastRunReportInfos[2].UserDefinedFieldValueList["ShipmentField"]).ValueAsStringForSerialisation);
			}
		}

		public void TestOnUserControlShown_DoesNotThrowWhenNoteCannotBeLoaded()
		{
			var dummyConsol = Factory.New<DummyConsolBusinessObject>();
			dummyConsol.Z0_Code = "DC1";
			Factory.Save();

			using (var plugIn = new DocumentUDFPlugInForTestThatReturnsNullNote(dummyConsol))
			{
				AssertNoExceptionThrown(plugIn.OnUserControlShown);
			}
		}

		public void TestUserDefinedFieldsShowLabelWithLongDisplayName()
		{
			DummyConsolBusinessObject dummyConsol = Factory.New<DummyConsolBusinessObject>();
			using (TestForm form = new TestForm(dummyConsol))
			{
				TextFieldUserControl userDefinedFieldTextBox = new TextFieldUserControl();
				TextField userField = new TextField(Factory);
				userField.DisplayName = "UserFieldWithLongDisplayName";
				userField.Value = "user field";
				userDefinedFieldTextBox.SetFilter(userField);
				form.Controls.Add(userDefinedFieldTextBox);
				form.Show();

				var textBoxs = userDefinedFieldTextBox.Controls.Find("FieldTextBox", true);
				Assert(textBoxs.Length > 0);
				var textBox = textBoxs[0] as ZTextBox;

				var labelCaptionRenderer = textBox.GetExtension<ILabelCaptionRenderer>();
				AssertNotNull(labelCaptionRenderer);
				Assert("The field name should be truncated.", labelCaptionRenderer.IsCaptionTruncated);
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			MockSourceControl.Setup();
			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
			DocumentCustomisationMenuItemMenusMaker.ShouldAddDebugOnlyMenuItemsForTesting = true;
		}

		protected override void TearDown()
		{
			base.TearDown();
			MockSourceControl.TearDown();
			DocumentCustomisationMenuItemMenusMaker.ShouldAddDebugOnlyMenuItemsForTesting = false;
			if (resourceRetriever.IsValueCreated)
			{
				resourceRetriever.Value.Dispose();
			}
		}

		readonly Lazy<EmbeddedResourceRetriever> resourceRetriever = new Lazy<EmbeddedResourceRetriever>(() => new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly));

		ExcelTemplateForUnitTesting udfWithoutTabs;
		ExcelTemplateForUnitTesting UDFWithoutTabs
		{
			get
			{
				if (udfWithoutTabs == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF without tabs.xls", "UDF without tabs.xls");
					udfWithoutTabs = new ExcelTemplateForUnitTesting("UDF without tabs.xls", Path.GetFullPath(tempFileName));
				}
				return udfWithoutTabs;
			}
		}

		ExcelTemplateForUnitTesting udfWithTabs;
		ExcelTemplateForUnitTesting UDFWithTabs
		{
			get
			{
				if (udfWithTabs == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with tabs.xls", "UDF with tabs.xls");
					udfWithTabs = new ExcelTemplateForUnitTesting("UDF with tabs.xls", Path.GetFullPath(tempFileName));
				}
				return udfWithTabs;
			}
		}

		ExcelTemplateForUnitTesting udfWithTabs3;
		ExcelTemplateForUnitTesting UDFWithTabs3
		{
			get
			{
				if (udfWithTabs3 == null)
				{
					var tempFileName = resourceRetriever.Value.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with tabs3.xls", "UDF with tabs3.xls");
					udfWithTabs3 = new ExcelTemplateForUnitTesting("UDF with tabs3.xls", Path.GetFullPath(tempFileName));
				}
				return udfWithTabs3;
			}
		}

		class DocumentUDFPlugInForTestThatReturnsNullNote : DocumentUDFPlugIn
		{
			public DocumentUDFPlugInForTestThatReturnsNullNote(IBusiness hostBusinessEntity)
				: base(hostBusinessEntity)
			{
			}

			protected override DocumentNote Note
			{
				get { return null; }
			}
		}

		internal class TestForm : ZForm
		{
			public TestForm(DummyConsolBusinessObject entity)
				: base(entity)
			{
				MyTabControl.Dock = DockStyle.Fill;
				this.Controls.Add(MyTabControl);
				((IZForm)this).ControllerID = DummyControllerIDs.Dummy;
			}

			public readonly ZTemplateTabControl MyTabControl = new ZTemplateTabControl();

			protected override ZTabControl TopLevelTabControl
			{
				get { return MyTabControl; }
			}

			public void TestDelete()
			{
				Delete();
			}
		}
	}
}
