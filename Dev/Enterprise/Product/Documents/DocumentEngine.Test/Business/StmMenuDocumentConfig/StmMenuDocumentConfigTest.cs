using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using DataContext = Enterprise.Core.Constants.DataContext;

namespace Enterprise.DocumentEngine.Business.Testing
{
	[TestedType(typeof(StmMenuDocumentConfig))]
	sealed class StmMenuDocumentConfigTest : StmMenuDocumentConfigTestCase<StmMenuDocumentConfig>
	{
		public void TestIsSystemDefined()
		{
			var config = Factory.New<StmMenuDocumentConfig>();

			config.S3_IsSystem = ZBool.False;
			config.S3_OH = ZGuid.Empty;
			AssertEquals(false, config.IsSystemDefined);

			config.S3_IsSystem = ZBool.True;
			AssertEquals(true, config.IsSystemDefined);

			var apple = Factory.New<OrgHeader>();
			config.S3_OH = apple.PK;
			AssertEquals(false, config.IsSystemDefined);

			config.S3_IsSystem = ZBool.False;
			AssertEquals(false, config.IsSystemDefined);
		}

		public void TestIsClientSpecific()
		{
			var config = Factory.New<StmMenuDocumentConfig>();

			config.S3_IsSystem = ZBool.False;
			config.S3_OH = ZGuid.Empty;
			AssertEquals(false, config.IsClientSpecific);

			config.S3_IsSystem = ZBool.True;
			AssertEquals(false, config.IsClientSpecific);

			var apple = Factory.New<OrgHeader>();
			config.S3_OH = apple.PK;
			AssertEquals(true, config.IsClientSpecific);

			config.S3_IsSystem = ZBool.False;
			AssertEquals(false, config.IsClientSpecific);
		}

		public void TestIsUserDefined()
		{
			var config = Factory.New<StmMenuDocumentConfig>();

			config.S3_IsSystem = ZBool.False;
			config.S3_OH = ZGuid.Empty;
			AssertEquals(true, config.IsUserDefined);

			config.S3_IsSystem = ZBool.True;
			AssertEquals(false, config.IsUserDefined);

			var apple = Factory.New<OrgHeader>();
			config.S3_OH = apple.PK;
			AssertEquals(false, config.IsUserDefined);

			config.S3_IsSystem = ZBool.False;
			AssertEquals(true, config.IsUserDefined);
		}

		public void TestEditingModeForUserConfig()
		{
			var config = Factory.New<StmMenuDocumentConfig>();
			config.S3_IsSystem = ZBool.False;
			config.S3_OH = ZGuid.Empty;

			config.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("config.CanDelete", true, config.CanDelete);
		}

		public void TestEditingModeForUserConfigWithClientSet()
		{
			var apple = Factory.New<OrgHeader>();
			var config = Factory.New<StmMenuDocumentConfig>();
			config.S3_IsSystem = ZBool.False;
			config.S3_OH = apple.PK;

			config.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("config.CanDelete", true, config.CanDelete);
		}

		public void TestEditingModeForSystemDefinedConfig()
		{
			var config = Factory.New<StmMenuDocumentConfig>();
			config.S3_IsSystem = ZBool.True;
			config.S3_OH = ZGuid.Empty;

			config.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("config.CanDelete", false, config.CanDelete);
			AssertEquals("config.ReasonForNotAbleToDelete", "You are not allowed to delete the selected document configuration, because it is System Defined.", config.ReasonForNotAbleToDelete);

			config.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("config.CanDelete", false, config.CanDelete);
			AssertEquals("config.ReasonForNotAbleToDelete", "You are not allowed to delete this document configuration.", config.ReasonForNotAbleToDelete);
		}

		public void TestEditingModeForClientSpecificConfig()
		{
			var apple = Factory.New<OrgHeader>();
			var config = Factory.New<StmMenuDocumentConfig>();
			config.S3_IsSystem = ZBool.True;
			config.S3_OH = apple.PK;

			config.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.AllowEditingOfClientSpecificOnly;
			AssertEquals("config.CanDelete", true, config.CanDelete);

			config.EditingMode = MenuEditingMode.AllowEditingOfSystemDefinedOnly;
			AssertEquals("config.CanDelete", false, config.CanDelete);
			AssertEquals("config.ReasonForNotAbleToDelete", "You are not allowed to delete the selected document configuration, because it is Not System Defined.", config.ReasonForNotAbleToDelete);

			config.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("config.CanDelete", false, config.CanDelete);
			AssertEquals("config.ReasonForNotAbleToDelete", "You are not allowed to delete this document configuration.", config.ReasonForNotAbleToDelete);
		}

		public void TestOverrideDataContextsList()
		{
			var overrideDataContextsList = DocConfig.OverrideDataContextsList;
			CombineAssertions(delegate
			{
				Assert("overrideDataContextsList.ContainsCode('')", overrideDataContextsList.ContainsCode(""));
				Assert("overrideDataContextsList.ContainsCode('GenericCommercialInvoice')", overrideDataContextsList.ContainsCode(nameof(DataContext.GenericCommercialInvoice)));
				Assert("overrideDataContextsList.ContainsCode('GenericFreightJobByContainerIfFCL')", overrideDataContextsList.ContainsCode(nameof(DataContext.GenericFreightJobByContainerIfFCL)));
				Assert("overrideDataContextsList.ContainsCode('GenericFreightJobRouting')", overrideDataContextsList.ContainsCode(nameof(DataContext.GenericFreightJobRouting)));
				Assert("overrideDataContextsList.ContainsCode('GenericFreightJobServices')", overrideDataContextsList.ContainsCode(nameof(DataContext.GenericFreightJobServices)));
				Assert("overrideDataContextsList.ContainsCode('GenericFreightJobInvoice')", overrideDataContextsList.ContainsCode(nameof(DataContext.GenericFreightJobInvoice)));
			});
		}

		public void TestPageStyleInAllSystemConfigsHasAValue()
		{
			var validValues = new DocumentConfigPageStyleList();
			var systemConfigs = Factory.Load<StmMenuDocumentConfig>(new ZQuery(StmMenuDocumentConfigSchema.S3_IsSystem, true));
			ZStringBuilder result = new ZStringBuilder();
			foreach (StmMenuDocumentConfig config in systemConfigs)
			{
				var parentMenu = config.MenuTemplatePivot.MenuItem;
				if (!validValues.ContainsCode(config.S3_PageStyle))
				{
					result.Append(string.Format("BusinessContext: [{0}] Menu Name: [{1}] Page Style: [{2}]", parentMenu.SU_BusinessContext, parentMenu.SU_MenuName, config.S3_PageStyle));
				}
			}

			Assert("Document Config Row has invalid Page Style.\r\n\r\n" + result.ToStringWithNewLineBetweenAppends(), result.IsEmpty);
		}

		public void TestDefaultValues()
		{
			var documentConfig = Factory.NewWithValidTestData<StmMenuDocumentConfig>();
			AssertEquals("documentConfig.S3_PageStyle", DocumentConfigPageStyleList.Codes.Portrait, documentConfig.S3_PageStyle);
		}

		public void TestTemplateSections()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory);
			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			AssertNotNull(config);
			AssertNotNull(config.TemplateSections);
			AssertEquals("config.TemplateSections.Count", 12, config.TemplateSections.Count);
		}

		public void TestS3_OverrideDataContext()
		{
			StmMenuDocumentConfig config = Factory.New<StmMenuDocumentConfig>();

			string testValue = "12345678901234567890123456789012345";
			config.S3_OverrideDataContext = testValue;
			AssertEquals("config.S3_OverrideDataContext", testValue, config.S3_OverrideDataContext);
			config.S3_OverrideDataContext = "";
			AssertEquals("config.S3_OverrideDataContext", "", config.S3_OverrideDataContext);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			StmMenuTemplatePivotBase menuTemplatePivot = Factory.NewWithValidTestData<StmMenuTemplatePivotBase>();
			menuTemplatePivot.SI_SU = Factory.NewWithValidTestData<StmMenuItemBase>().PK;
			menuTemplatePivot.SI_SO = Factory.NewWithValidTestData<StmTemplateBase>().PK;
			return menuTemplatePivot.DocConfigs.AddNew();
		}

		public void TestConfigItems()
		{
			StmMenuDocumentConfigItem item1 = Factory.New<StmMenuDocumentConfigItem>();
			StmMenuDocumentConfigItem item2 = Factory.New<StmMenuDocumentConfigItem>();
			StmMenuDocumentConfigItem item3 = Factory.New<StmMenuDocumentConfigItem>();
			StmMenuDocumentConfigItem item4 = Factory.New<StmMenuDocumentConfigItem>();

			item1.S4_PrintOrder = 1;
			item2.S4_PrintOrder = 0;
			item3.S4_PrintOrder = 2;

			item1.S4_S3 = DocConfig.PK;
			item2.S4_S3 = DocConfig.PK;
			item3.S4_S3 = DocConfig.PK;

			AssertEquals("IsRegisteredEditableChildObject(ConfigItems)", true, DocConfig.IsRegisteredEditableChildObject(DocConfig.ConfigItems));
			AssertEquals("ConfigItems.Count", 3, DocConfig.ConfigItems.Count);
			AssertEquals("ConfigItems[0]", item2, DocConfig.ConfigItems[0]);
			AssertEquals("ConfigItems[1]", item1, DocConfig.ConfigItems[1]);
			AssertEquals("ConfigItems[2]", item3, DocConfig.ConfigItems[2]);
		}

		public void TestCopyTo()
		{
			DocConfig.S3_IsSystem = true;
			DocConfig.S3_GC = GlbCompany.CurrentCompany.PK;
			DocConfig.S3_OH = GlbCompany.CurrentCompany.OrgProxy.PK;
			DocConfig.ConfigItems.AddNew().S4_SectionItemName = "Moo";
			DocConfig.ConfigItems.AddNew().S4_SectionItemName = "Oink";

			StmMenuDocumentConfig anotherDocConfig = Factory.New<StmMenuDocumentConfig>();
			DocConfig.CopyOver(anotherDocConfig);

			AssertEquals("S3_IsSystem", true, anotherDocConfig.S3_IsSystem);
			AssertEquals("S3_GC", GlbCompany.CurrentCompany.PK, anotherDocConfig.S3_GC);
			AssertEquals("S3_OH", GlbCompany.CurrentCompany.OrgProxy.PK, anotherDocConfig.S3_OH);
			AssertEquals("S3_SI", DocConfig.S3_SI, anotherDocConfig.S3_SI);

			AssertEquals("ConfigItems.Count", 2, anotherDocConfig.ConfigItems.Count);
			AssertEquals("ConfigItems[0].S4_SectionItemName", "Moo", anotherDocConfig.ConfigItems[0].S4_SectionItemName);
			AssertEquals("ConfigItems[1].S4_SectionItemName", "Oink", anotherDocConfig.ConfigItems[1].S4_SectionItemName);

			AssertNotEquals("PK", DocConfig.PK, anotherDocConfig.PK);
			AssertNotEquals("ConfigItems[0].PK", DocConfig.ConfigItems[0].PK, anotherDocConfig.ConfigItems[0].PK);
			AssertNotEquals("ConfigItems[1].PK", DocConfig.ConfigItems[1].PK, anotherDocConfig.ConfigItems[1].PK);
		}

		public override void TestGetRelatedDocConfigs()
		{
			AssertEquals("GetRelatedDocConfigs().Length", 0, DocConfig.GetRelatedDocConfigs().Length);

			StmMenuTemplatePivotBase menuTemplatePivot = (StmMenuTemplatePivotBase)DocConfig.MenuTemplatePivot;
			StmMenuDocumentConfig anotherDocConfig1 = menuTemplatePivot.DocConfigs.AddNew();
			StmMenuDocumentConfig anotherDocConfig2 = menuTemplatePivot.DocConfigs.AddNew();
			StmMenuDocumentConfig[] relatedDocConfigs = DocConfig.GetRelatedDocConfigs();

			AssertEquals("GetRelatedDocConfigs().Length", 2, relatedDocConfigs.Length);
			AssertCollectionContains("GetRelatedDocConfigs() should contain anotherDocConfig1.", anotherDocConfig1, relatedDocConfigs);
			AssertCollectionContains("GetRelatedDocConfigs() should contain anotherDocConfig2.", anotherDocConfig2, relatedDocConfigs);
		}

		public void TestIStmMenuDocumentConfigSourceMembers()
		{
			IStmMenuDocumentConfigSource docConfigSource = DocConfig;
			StmMenuTemplatePivotBase menuTemplatePivot = (StmMenuTemplatePivotBase)DocConfig.MenuTemplatePivot;
			AssertEquals("GetPersistentDocConfig()", DocConfig, docConfigSource.GetPersistentDocConfig());

			menuTemplatePivot.EditingMode = MenuEditingMode.AllowAll;
			AssertEquals("EditingMode", MenuEditingMode.AllowAll, docConfigSource.EditingMode);

			menuTemplatePivot.EditingMode = MenuEditingMode.NotAllowEditingOfSystemOrClientMenus;
			AssertEquals("EditingMode", MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, docConfigSource.EditingMode);

			StmMenuDocumentConfigItem configItem1 = DocConfig.ConfigItems.AddNew();
			StmMenuDocumentConfigItem configItem2 = DocConfig.ConfigItems.AddNew();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			TemporaryStmMenuDocumentConfig tempDocConfig = docConfigSource.GetTemporaryDocConfig(newFactory);
			AssertEquals("GetTemporaryDocConfig().Factory", newFactory, tempDocConfig.Factory);
			AssertEquals("GetTemporaryDocConfig().ConfigItems.Count", 2, tempDocConfig.ConfigItems.Count);
			AssertEquals("GetTemporaryDocConfig().ConfigItems[0].PK", configItem1.PK, tempDocConfig.ConfigItems[0].PK);
			AssertEquals("GetTemporaryDocConfig().ConfigItems[1].PK", configItem2.PK, tempDocConfig.ConfigItems[1].PK);

			Factory.Save();
			configItem1.Delete();
			newFactory = new BusinessObjectFactory();
			tempDocConfig = docConfigSource.GetTemporaryDocConfig(newFactory);
			AssertEquals("GetTemporaryDocConfig().Factory", newFactory, tempDocConfig.Factory);
			AssertEquals("GetTemporaryDocConfig().ConfigItems.Count", 1, tempDocConfig.ConfigItems.Count);
			AssertEquals("GetTemporaryDocConfig().ConfigItems[0].PK", configItem2.PK, tempDocConfig.ConfigItems[0].PK);
		}

		public void TestGetTemporaryDocConfigIsReadOnly()
		{
			DocConfig.ReadOnly = false;
			var temporaryConfig = ((IStmMenuDocumentConfigSource)DocConfig).GetTemporaryDocConfig(new BusinessObjectFactory());
			AssertEquals("Pre-condition: temporaryConfig.ReadOnly", false, temporaryConfig.ReadOnly);

			DocConfig.ReadOnly = true;
			temporaryConfig = ((IStmMenuDocumentConfigSource)DocConfig).GetTemporaryDocConfig(new BusinessObjectFactory());
			AssertEquals("If real config is read only, then temporary config should be read only also.", true, temporaryConfig.ReadOnly);
		}

		public void TestDocumentTitle()
		{
			var menuTemplatePivot = (StmMenuTemplatePivotBase)DocConfig.MenuTemplatePivot;
			menuTemplatePivot.SI_DocumentTitle = "New Document";

			AssertEquals("New Document", ((IDocumentConfig)DocConfig).DocumentTitle);

			DocConfig.S3_OverrideEmailSubject = "Override Document Name";
			DocConfig.S3_IsSystem = true;
			AssertEquals("New Document", ((IDocumentConfig)DocConfig).DocumentTitle);

			DocConfig.S3_IsSystem = false;
			AssertEquals("Override Document Name", ((IDocumentConfig)DocConfig).DocumentTitle);
		}
	}
}
