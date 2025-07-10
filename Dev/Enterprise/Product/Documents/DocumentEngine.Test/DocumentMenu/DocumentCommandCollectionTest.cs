using System;
using System.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentCommandCollection))]
	sealed class DocumentCommandCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNoNullReferenceException_NestEnableCacheDocumentsCommandFilters()
		{
			AssertNoExceptionThrown(() =>
			{
				using var first = DocumentCommandCollection.EnableCacheDocumentsCommandFilters();
				using var second = DocumentCommandCollection.EnableCacheDocumentsCommandFilters();
			});
		}

		new DocumentCommandCollection Collection
		{
			get { return (DocumentCommandCollection)base.Collection; }
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var docDummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			return new DocumentCommandCollection(docDummy);
		}

		public void TestGetApplicableDocumentCommandsShouldEnableCache()
		{
			var command1 = Factory.New<DocumentCommand>();
			command1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command1.SU_MenuName = "command1";
			command1.SU_FilterList = "\"<Z0_Code>\" == \"123\"";

			var command2 = Factory.New<DocumentCommand>();
			command2.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command2.SU_MenuName = "command2";
			command2.SU_FilterList = "\"<Z0_Code>\" == \"456\"";

			var command3 = Factory.New<DocumentCommand>();
			command3.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command3.SU_MenuName = "command3";
			command3.SU_FilterList = "\"<Z0_Code>\" == \"789\"";

			var docDummy = Factory.New<DummyBusinessObjectForTestingCache>();
			docDummy.Z0_Code = "123";
			docDummy.Z0_Description = "Desc";

			Factory.Save();

			docDummy.CountForZ0_CodeAccessed = 0;
			AssertEquals("Menus loaded", 1, docDummy.DocumentCommands.GetApplicableDocumentCommands().Count);
			AssertEquals("Z0_Code should only access 1 time.", 1, docDummy.CountForZ0_CodeAccessed);
		}

		class DummyBusinessObjectForTestingCache : DocumentCommandTest.DocDummyBusinessObject
		{
			public DummyBusinessObjectForTestingCache(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public int CountForZ0_CodeAccessed { get; set; }

			public override ZString Z0_Code
			{
				get
				{
					CountForZ0_CodeAccessed++;
					return base.Z0_Code;
				}
				set
				{
					base.Z0_Code = value;
				}
			}
		}

		public void TestLoadMenuItems()
		{
			#region Templates

			StmTemplateBase sysShipmentTemplate = Factory.New<StmTemplateBase>();
			sysShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			sysShipmentTemplate.SO_IsSystemDefined = true;
			sysShipmentTemplate.SO_Name = "System Shipment Template";

			StmTemplateBase userShipmentTemplate = Factory.New<StmTemplateBase>();
			userShipmentTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Shipment);
			userShipmentTemplate.SO_IsSystemDefined = false;
			userShipmentTemplate.SO_Name = "User Shipment Template";

			StmTemplateBase sysConsolTemplate = Factory.New<StmTemplateBase>();
			sysConsolTemplate.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.Consol);
			sysConsolTemplate.SO_IsSystemDefined = true;
			sysConsolTemplate.SO_Name = "System Consol Template";

			#endregion

			#region Published System Shipment

			DocumentCommand pubSysShipmentMenu = Factory.New<DocumentCommand>();
			pubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysShipmentMenu.SU_IsPublished = true;
			pubSysShipmentMenu.SU_IsSystemDefined = true;
			pubSysShipmentMenu.SU_MenuName = "Pub System Shipment Document";
			pubSysShipmentMenu.SU_MenuIndex = 0;
			pubSysShipmentMenu.SU_MenuPath = "";
			pubSysShipmentMenu.SU_MenuShortcut = "CtrlF1";

			StmMenuTemplatePivotBase pubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysShipmentPivot1.SI_SU = pubSysShipmentMenu.PK;
			pubSysShipmentPivot1.SI_DocumentTitle = "Pub System Shipment Document";

			#endregion

			#region Unpublished System Shipment

			DocumentCommand unPubSysShipmentMenu = Factory.New<DocumentCommand>();
			unPubSysShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			unPubSysShipmentMenu.SU_IsSystemDefined = true;
			unPubSysShipmentMenu.SU_IsPublished = false;
			unPubSysShipmentMenu.SU_MenuName = "UnPub System Shipment Document";
			unPubSysShipmentMenu.SU_MenuIndex = 1;
			unPubSysShipmentMenu.SU_MenuPath = "";
			unPubSysShipmentMenu.SU_MenuShortcut = "CtrlF1";

			StmMenuTemplatePivotBase unPubSysShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			unPubSysShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			unPubSysShipmentPivot1.SI_SU = unPubSysShipmentMenu.PK;
			unPubSysShipmentPivot1.SI_DocumentTitle = "UnPub System Shipment Document";

			#endregion

			#region Published System Consol

			DocumentCommand pubSysConsolMenu = Factory.New<DocumentCommand>();
			pubSysConsolMenu.SU_BusinessContext = nameof(BusinessContext.Consol);
			pubSysConsolMenu.SU_IsPublished = true;
			pubSysConsolMenu.SU_IsSystemDefined = true;
			pubSysConsolMenu.SU_MenuName = "Pub System Consol Document";
			pubSysConsolMenu.SU_MenuIndex = 2;
			pubSysConsolMenu.SU_MenuPath = "";
			pubSysConsolMenu.SU_MenuShortcut = "CtrlF2";

			StmMenuTemplatePivotBase pubSysConsolPivot = Factory.New<StmMenuTemplatePivotBase>();
			pubSysConsolPivot.SI_SO = sysConsolTemplate.PK;
			pubSysConsolPivot.SI_SU = pubSysConsolMenu.PK;
			pubSysConsolPivot.SI_DocumentTitle = "Pub System Consol Document";

			#endregion

			#region Published UserDefined Shipment

			DocumentCommand pubUserShipmentMenu = Factory.New<DocumentCommand>();
			pubUserShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubUserShipmentMenu.SU_IsPublished = true;
			pubUserShipmentMenu.SU_IsSystemDefined = false;
			pubUserShipmentMenu.SU_MenuName = "Pub User Shipment Document";
			pubUserShipmentMenu.SU_MenuIndex = 3;
			pubUserShipmentMenu.SU_MenuPath = "";
			pubUserShipmentMenu.SU_MenuShortcut = "CtrlF3";

			StmMenuTemplatePivotBase pubUserShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubUserShipmentPivot1.SI_SO = userShipmentTemplate.PK;
			pubUserShipmentPivot1.SI_SU = pubUserShipmentMenu.PK;
			pubUserShipmentPivot1.SI_DocumentTitle = "Pub User Shipment Document";

			#endregion

			#region Private UserDefined Shipment for current user

			DocumentCommand privateUserShipmentMenu = Factory.New<DocumentCommand>();
			privateUserShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			privateUserShipmentMenu.SU_IsPublished = false;
			privateUserShipmentMenu.SU_IsSystemDefined = false;
			privateUserShipmentMenu.SU_MenuName = "Private User Shipment Document";
			privateUserShipmentMenu.SU_MenuIndex = 4;
			privateUserShipmentMenu.SU_MenuPath = "";
			privateUserShipmentMenu.SU_MenuShortcut = "CtrlF4";
			privateUserShipmentMenu.SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;

			StmMenuTemplatePivotBase privateUserShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			privateUserShipmentPivot1.SI_SO = userShipmentTemplate.PK;
			privateUserShipmentPivot1.SI_SU = privateUserShipmentMenu.PK;
			privateUserShipmentPivot1.SI_DocumentTitle = "Private User Shipment Document";

			#endregion

			#region Private UserDefined Shipment for some other user

			DocumentCommand privateOtherUserShipmentMenu = Factory.New<DocumentCommand>();
			privateOtherUserShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			privateOtherUserShipmentMenu.SU_IsPublished = false;
			privateOtherUserShipmentMenu.SU_IsSystemDefined = false;
			privateOtherUserShipmentMenu.SU_MenuName = "Private Other User Shipment Document";
			privateOtherUserShipmentMenu.SU_MenuPath = "";
			privateOtherUserShipmentMenu.SU_MenuIndex = 5;
			privateOtherUserShipmentMenu.SU_MenuShortcut = "CtrlF5";
			privateOtherUserShipmentMenu.SU_GS_NKStaffCode = "PM";

			StmMenuTemplatePivotBase privateUserShipmentPivot2 = Factory.New<StmMenuTemplatePivotBase>();
			privateUserShipmentPivot2.SI_SO = userShipmentTemplate.PK;
			privateUserShipmentPivot2.SI_SU = privateOtherUserShipmentMenu.PK;
			privateUserShipmentPivot2.SI_DocumentTitle = "Private Other User Shipment Document";

			#endregion

			#region	Published System Specialised Shipment that should be shown in Special Folder

			DocumentCommand pubSysSpecialisedShipmentMenu = Factory.New<DocumentCommand>();
			pubSysSpecialisedShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysSpecialisedShipmentMenu.SU_IsPublished = true;
			pubSysSpecialisedShipmentMenu.SU_IsSystemDefined = true;
			pubSysSpecialisedShipmentMenu.SU_MenuName = "Pub Sys Specialised Shipment Document";
			pubSysSpecialisedShipmentMenu.SU_MenuIndex = 1;
			pubSysSpecialisedShipmentMenu.SU_MenuPath = "Special";
			pubSysSpecialisedShipmentMenu.SU_MenuShortcut = "CtrlF6";
			pubSysSpecialisedShipmentMenu.SU_FilterList = "BUY=" + GlbStaff.CurrentUser.PK.ToString(); //any PK will do for testing.

			StmMenuTemplatePivotBase pubSpecialisedShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSpecialisedShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSpecialisedShipmentPivot1.SI_SU = pubSysSpecialisedShipmentMenu.PK;
			pubSpecialisedShipmentPivot1.SI_DocumentTitle = "Pub Sys Specialised Shipment Document";

			#endregion

			#region Published System Specialised Shipment that should NOT be shown

			DocumentCommand pubSysOtherSpecialisedShipmentMenu = Factory.New<DocumentCommand>();
			pubSysOtherSpecialisedShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			pubSysOtherSpecialisedShipmentMenu.SU_IsPublished = true;
			pubSysOtherSpecialisedShipmentMenu.SU_IsSystemDefined = true;
			pubSysOtherSpecialisedShipmentMenu.SU_MenuName = "Pub Sys Other Special Shipment Document";
			pubSysOtherSpecialisedShipmentMenu.SU_MenuIndex = 2;
			pubSysOtherSpecialisedShipmentMenu.SU_MenuPath = "Special";
			pubSysOtherSpecialisedShipmentMenu.SU_MenuShortcut = "CtrlF7";
			pubSysOtherSpecialisedShipmentMenu.SU_FilterList = "BUY=" + Guid.NewGuid().ToString(); //any PK will do for testing.

			StmMenuTemplatePivotBase pubSysOtherSpecialisedShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			pubSysOtherSpecialisedShipmentPivot1.SI_SO = sysShipmentTemplate.PK;
			pubSysOtherSpecialisedShipmentPivot1.SI_SU = pubSysOtherSpecialisedShipmentMenu.PK;
			pubSysOtherSpecialisedShipmentPivot1.SI_DocumentTitle = "Pub Sys Other Special Shipment Document";

			#endregion

			#region Private User Specialised Shipment that should be shown in Special\Private Folder

			DocumentCommand privateUserSpecialisedShipmentMenu = Factory.New<DocumentCommand>();
			privateUserSpecialisedShipmentMenu.SU_BusinessContext = nameof(BusinessContext.Shipment);
			privateUserSpecialisedShipmentMenu.SU_IsPublished = false;
			privateUserSpecialisedShipmentMenu.SU_IsSystemDefined = false;
			privateUserSpecialisedShipmentMenu.SU_MenuName = "Pri User Special Shipment Document";
			privateUserSpecialisedShipmentMenu.SU_MenuIndex = 1;
			privateUserSpecialisedShipmentMenu.SU_MenuPath = "Special/Private";
			privateUserSpecialisedShipmentMenu.SU_MenuShortcut = "CtrlF8";
			privateUserSpecialisedShipmentMenu.SU_FilterList = "BUY=" + GlbStaff.CurrentUser.PK.ToString(); //any PK will do for testing.
			privateUserSpecialisedShipmentMenu.SU_GS_NKStaffCode = GlbStaff.CurrentUser.GS_Code;

			StmMenuTemplatePivotBase privateUserSpecialisedShipmentPivot1 = Factory.New<StmMenuTemplatePivotBase>();
			privateUserSpecialisedShipmentPivot1.SI_SO = userShipmentTemplate.PK;
			privateUserSpecialisedShipmentPivot1.SI_SU = privateUserSpecialisedShipmentMenu.PK;
			privateUserSpecialisedShipmentPivot1.SI_DocumentTitle = "Pr User Special Shipment Document";

			#endregion

			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			var docDummy = factory2.New<DocumentCommandTest.DocDummyBusinessObject>();
			docDummy.Z0_Code = "Tst";
			docDummy.Z0_Description = "Desc";

			factory2.Save();

			AssertEquals("Menus loaded", 8, docDummy.DocumentCommands.Count); // CWSupport account has the highest visibility (even the PM account)

			AssertEquals("PubSysShipmentMenu Text", pubSysShipmentMenu.SU_MenuName, docDummy.DocumentCommands[0].SU_MenuName);
			AssertEquals("PubSysShipmentMenu Shortcut", pubSysShipmentMenu.SU_MenuShortcut, docDummy.DocumentCommands[0].SU_MenuShortcut);
			AssertEquals("PubSysShipmentMenu MenuNameTag", "", docDummy.DocumentCommands[0].MenuNameTag);
			AssertEquals("PubSysShipmentMenu IsApplicable", true, docDummy.DocumentCommands[0].IsApplicable);

			AssertEquals("UnPubSysShipmentMenu Text", unPubSysShipmentMenu.SU_MenuName, docDummy.DocumentCommands[1].SU_MenuName);
			AssertEquals("UnPubSysShipmentMenu Shortcut", unPubSysShipmentMenu.SU_MenuShortcut, docDummy.DocumentCommands[1].SU_MenuShortcut);
			AssertEquals("UnPubSysShipmentMenu MenuNameTag", "", docDummy.DocumentCommands[1].MenuNameTag);
			AssertEquals("UnPubSysShipmentMenu IsApplicable", true, docDummy.DocumentCommands[1].IsApplicable);

			AssertEquals("PubUserShipmentMenu Text", pubUserShipmentMenu.SU_MenuName, docDummy.DocumentCommands[2].SU_MenuName);
			AssertEquals("PubUserShipmentMenu MenuNameTag", " <Customized>", docDummy.DocumentCommands[2].MenuNameTag);
			AssertEquals("PubUserShipmentMenu Shortcut", pubUserShipmentMenu.SU_MenuShortcut, docDummy.DocumentCommands[2].SU_MenuShortcut);
			AssertEquals("PubUserShipmentMenu IsApplicable", true, docDummy.DocumentCommands[2].IsApplicable);

			AssertEquals("PrivateUserShipmentMenu Text", privateUserShipmentMenu.SU_MenuName, docDummy.DocumentCommands[3].SU_MenuName);
			AssertEquals("PrivateUserShipmentMenu MenuNameTag", " <Private>", docDummy.DocumentCommands[3].MenuNameTag);
			AssertEquals("PrivateUserShipmentMenu Shortcut", privateUserShipmentMenu.SU_MenuShortcut, docDummy.DocumentCommands[3].SU_MenuShortcut);
			AssertEquals("PrivateUserShipmentMenu IsApplicable", true, docDummy.DocumentCommands[3].IsApplicable);

			BusinessObjectFactory factory3 = new BusinessObjectFactory();
			var docDummyInFactory3 = factory3.Load<DocumentCommandTest.DocDummyBusinessObject>(docDummy.PK);
			docDummyInFactory3.DocumentCommands.Load();

			AssertEquals("Should not have hit StmMenuTemplatePivot table yet", 0, factory3.GetTableHitCount("StmMenuTemplatePivot"));

			var x = docDummyInFactory3.DocumentCommands[0].Documents;

			AssertEquals(1, factory3.GetTableHitCount("StmMenuTemplatePivot"));
		}

		public void TestLoadNewTemplateFromFile()
		{
			using (var resourceRetriever = new EmbeddedResourceRetriever())
			{
				var udfWithDefaultsPath = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.UDF with defaults.xls", "UDF with defaults.xls");
				var template = Collection.LoadNewTemplateFromFile(udfWithDefaultsPath);
				AssertEquals("SO_DataContext", nameof(BusinessContext.Shipment), template.SO_DataContext);
			}
		}

		public void TestAddingNewMenus()
		{
			var docDummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			DocumentCommand newMenu = (DocumentCommand)((System.ComponentModel.IBindingList)docDummy.DocumentCommands).AddNew();
			AssertEquals("SU_BusinessContext", nameof(BusinessContext.Shipment), newMenu.SU_BusinessContext);
			AssertEquals("SU_IsPublished", false, newMenu.SU_IsPublished);
			AssertEquals("SU_GS_NKStaffCode", GlbStaff.CurrentUser.GS_Code, newMenu.SU_GS_NKStaffCode);
			AssertEquals("Parent", docDummy, newMenu.Parent);
		}

		public void TestApplicableDocumentCommandsFilterOnlyBeEvaluatedOnce()
		{
			var command1 = Factory.New<DocumentCommand>();
			command1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command1.SU_MenuName = "command1";
			command1.SU_FilterList = "CTY=ZA";

			var command2 = Factory.New<DocumentCommand>();
			command2.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command2.SU_MenuName = "command2";
			command2.SU_FilterList = "CTY=ZA";

			var command3 = Factory.New<DocumentCommand>();
			command3.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command3.SU_MenuName = "command3";
			command3.SU_FilterList = "\"<CountryCode>\" == \"<CountryCode>\"";

			var docDummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			docDummy.Z0_Code = "Tst";
			docDummy.Z0_Description = "Desc";

			Factory.Save();

			AssertEquals("Menus loaded", 3, docDummy.DocumentCommands.Count);
			AssertNull(docDummy.DocumentCommands.FilterEvaluatedResult);

			var applicableDocumentCommands = docDummy.DocumentCommands.GetApplicableDocumentCommands();
			AssertEquals(1, applicableDocumentCommands.Count);
			AssertEquals(2, docDummy.DocumentCommands.FilterEvaluatedResult.Keys.Count);
			Assert("Contains key 'CTY=ZA'", docDummy.DocumentCommands.FilterEvaluatedResult.ContainsKey(("CTY=ZA", command1.SU_MenuType.ToString())));
			Assert("Contains key '\"<CountryCode>\" == \"<CountryCode>\"'", docDummy.DocumentCommands.FilterEvaluatedResult.ContainsKey(("\"<CountryCode>\" == \"<CountryCode>\"", command3.SU_MenuType.ToString())));
			Assert(!docDummy.DocumentCommands.FilterEvaluatedResult[("CTY=ZA", command1.SU_MenuType.ToString())]);
			Assert(docDummy.DocumentCommands.FilterEvaluatedResult[("\"<CountryCode>\" == \"<CountryCode>\"", command3.SU_MenuType.ToString())]);
		}

		public void TestApplicableDocumentCommandsBeEvaluatedThroughFilterAndMenuType()
		{
			var command1 = Factory.New<DocumentCommand>();
			command1.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command1.SU_MenuName = "command1";
			command1.SU_FilterList = "";
			command1.SU_MenuType = Core.Constants.StmMenuItemTypes.Documents;
			command1.SU_MenuIndex = 0;

			var command2 = Factory.New<DocumentCommand>();
			command2.SU_BusinessContext = nameof(BusinessContext.Shipment);
			command2.SU_MenuName = "command2";
			command2.SU_FilterList = "";
			command2.SU_MenuType = Core.Constants.StmMenuItemTypes.Forms;
			command2.SU_MenuIndex = -999;

			var docDummy = Factory.New<DocumentCommandTest.DocDummyBusinessObject>();
			docDummy.Z0_Code = "Tst";
			docDummy.Z0_Description = "Desc";

			Factory.Save();

			AssertEquals("Menus loaded", 2, docDummy.DocumentCommandsWithForms.Count);
			AssertNull(docDummy.DocumentCommandsWithForms.FilterEvaluatedResult);

			var applicableDocumentCommands = docDummy.DocumentCommandsWithForms.GetApplicableDocumentCommands();
			AssertEquals(1, applicableDocumentCommands.Count);
			AssertEquals(2, docDummy.DocumentCommandsWithForms.FilterEvaluatedResult.Keys.Count);
			Assert("Contains key ('','DOC')", docDummy.DocumentCommandsWithForms.FilterEvaluatedResult.ContainsKey((command1.SU_FilterList, command1.SU_MenuType.ToString())));
			Assert("Contains key ('','FRM')", docDummy.DocumentCommandsWithForms.FilterEvaluatedResult.ContainsKey((command2.SU_FilterList, command2.SU_MenuType.ToString())));
			Assert(docDummy.DocumentCommandsWithForms.FilterEvaluatedResult[(command1.SU_FilterList, command1.SU_MenuType.ToString())]);
			Assert(!docDummy.DocumentCommandsWithForms.FilterEvaluatedResult[(command1.SU_FilterList, command2.SU_MenuType.ToString())]);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Enterprise.DbUpgrader.Data.Testing.DocumentTablesCleaner.Clean();
		}
	}
}
