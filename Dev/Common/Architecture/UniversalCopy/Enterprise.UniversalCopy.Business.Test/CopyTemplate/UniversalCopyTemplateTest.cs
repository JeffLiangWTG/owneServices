using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.UniversalCopy;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.UniversalCopy.Business.Testing
{
	[TestedType(typeof(UniversalCopyTemplate))]
	sealed class UniversalCopyTemplateTest : EnterpriseBusinessObjectTestCase
	{
		public void TestClientSpecificModule()
		{
			using (ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.EDI))
			{
				var copyTemplate = GetNewCopyTemplate();
				copyTemplate.S9_ModuleID = "SupportIncident_UC";
				AssertEquals("Incidents", copyTemplate.ModuleDescription);
			}
		}

		public void TestSetDefaultValues()
		{
			var copyTemplate = GetNewCopyTemplate();
			AssertEquals(EnvProxy.Instance.CurrentCompany.PK, copyTemplate.S9_GC);
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, copyTemplate.S9_RelatedEntityID);
		}

		public void TestHumanReadableNameCore()
		{
			var copyTemplate = GetNewCopyTemplate();
			AssertEquals("Copy Template", copyTemplate.HumanReadableName);
		}

		public void TestIsApplicable()
		{
			var copyTemplate = GetNewCopyTemplate();
			copyTemplate.IsPublishedGlobal = true;
			AssertEquals("Empty FilterList", true, copyTemplate.IsApplicable);
			copyTemplate.CopyTemplateTree.FilterList = "S";
			AssertEquals("Invaldi FilterList", false, copyTemplate.IsApplicable);
			copyTemplate.CopyTemplateTree.FilterList = "CTY";
			AssertEquals("'CTY' FilterList", false, copyTemplate.IsApplicable);
			copyTemplate.CopyTemplateTree.FilterList = "CTY!";
			AssertEquals("'CTY!' FilterList", false, copyTemplate.IsApplicable);
			AssertIsApplicable(copyTemplate, Core.Constants.CountryCodes.UnitedStates, false, true, false, false, true, false, true);
			AssertIsApplicable(copyTemplate, Core.Constants.CountryCodes.PuertoRico, true, false, false, false, true, false, false);
			AssertIsApplicable(copyTemplate, Core.Constants.CountryCodes.Canada, false, true, false, false, true, false, false);
			AssertIsApplicable(copyTemplate, Core.Constants.CountryCodes.Australia, true, false, false, true, false, false, false);
		}

		void AssertIsApplicable(UniversalCopyTemplate copyTemplate, ZString country, bool expectResult1, bool expectResult2, bool expectResult3, bool expectResult4, bool expectResult5, bool expectResult6, bool expectResult7)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				copyTemplate.CopyTemplateTree.FilterList = "CTY!=US,CA";
				AssertEquals("'CTY!=US,CA' FilterList for " + country, expectResult1, copyTemplate.IsApplicable);
				copyTemplate.CopyTemplateTree.FilterList = "CTY=US,CA";
				AssertEquals("'CTY=US,CA' FilterList for " + country, expectResult2, copyTemplate.IsApplicable);
				copyTemplate.CopyTemplateTree.FilterList = "CTY=ZZ";
				AssertEquals("'CTY=ZZ' FilterList for " + country, expectResult3, copyTemplate.IsApplicable);
				copyTemplate.CopyTemplateTree.FilterList = "BKRCTY!=US,CA";
				AssertEquals("'BKRCTY!=US,CA' FilterList for " + country, expectResult4, copyTemplate.IsApplicable);
				copyTemplate.CopyTemplateTree.FilterList = "BKRCTY=US,CA";
				AssertEquals("'BKRCTY=US,CA' FilterList for " + country, expectResult5, copyTemplate.IsApplicable);
				copyTemplate.CopyTemplateTree.FilterList = "BKRCTY=ZZ";
				AssertEquals("'BKRCTY=ZZ' FilterList for " + country, expectResult6, copyTemplate.IsApplicable);
				copyTemplate.CopyTemplateTree.FilterList = "CTY=US,CA;BKRCTY=US";
				AssertEquals("'CTY=US,CA;BKRCTY=US' FilterList for " + country, expectResult7, copyTemplate.IsApplicable);
			}
		}
		public void TestCopyTemplateTreeAndPrepareForSave()
		{
			var copyTemplate = GetNewCopyTemplate();
			var templateTree = copyTemplate.CopyTemplateTree;

			AssertNotNull(templateTree);
			Assert(copyTemplate.IsRegisteredEditableChildObject(templateTree));
			Assert(copyTemplate.S9_FilterName.IsEmpty);
			Assert(copyTemplate.S9_FilterData.IsEmpty);

			templateTree.ConfigurationName = @"A\B\C";
			templateTree.EntityFilter = new EntityFilter { FilterData = "x = y", FilterTypeId = "type1", OrderBy = "z" };
			copyTemplate.PrepareForSave();

			AssertEquals("&C", copyTemplate.S9_FilterName);
			Assert(!copyTemplate.S9_FilterData.IsEmpty);

			copyTemplate.CopyTemplateTree = null;
			Assert(!copyTemplate.IsRegisteredEditableChildObject(templateTree));

			AssertNotNull("Should read template tree from serialized data", copyTemplate.CopyTemplateTree);
			AssertNotEquals("Should be new instance", templateTree, copyTemplate.CopyTemplateTree);
			Assert(!copyTemplate.IsRegisteredEditableChildObject(templateTree));
			Assert(copyTemplate.IsRegisteredEditableChildObject(copyTemplate.CopyTemplateTree));
			AssertEquals(@"&A\&B\&C", copyTemplate.CopyTemplateTree.ConfigurationName);
			AssertNotNull(copyTemplate.CopyTemplateTree.EntityFilter);
			AssertEquals("x = y", copyTemplate.CopyTemplateTree.EntityFilter.FilterData);
			AssertEquals("type1", copyTemplate.CopyTemplateTree.EntityFilter.FilterTypeId);
			AssertEquals("z", copyTemplate.CopyTemplateTree.EntityFilter.OrderBy);
		}

		public void NoCopyTemplateTreeExceptionOnEmptyData()
		{
			var copyTemplate = GetNewCopyTemplate();
			copyTemplate.CopyTemplateTree = null;
			Assert(copyTemplate.S9_FilterData.IsEmpty);
			AssertNull(copyTemplate.CopyTemplateTree);
		}

		public void TestIsPublishedGlobal()
		{
			var copyTemplate = GetNewCopyTemplate();

			copyTemplate.S9_GC = ZGuid.NewZGuid();
			Assert(!copyTemplate.IsPublishedGlobal);

			copyTemplate.S9_GC = ZGuid.Empty;
			Assert(copyTemplate.IsPublishedGlobal);

			copyTemplate.S9_GC = ZGuid.Missing;
			Assert(copyTemplate.IsPublishedGlobal);

			copyTemplate.S9_GC = ZGuid.Invalid;
			Assert(copyTemplate.IsPublishedGlobal);
		}

		public void TestValidation()
		{
			AssertEquals(typeof(UniversalCopyConfigurationValidation), GetNewCopyTemplate().Validation.GetType());
		}

		#region ValidateS9_FilterName

		public void TestValidateS9_FilterName()
		{
			var copyTemplate = GetNewCopyTemplate();

			copyTemplate.S9_FilterName = ZString.Empty;
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(copyTemplate.S9_FilterNameInfo, "Please enter a copy template name.");

			copyTemplate.S9_FilterName = "abc";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertNoNotifications(copyTemplate.S9_FilterNameInfo);
		}

		public void TestValidateS9_FilterName_IsUnique()
		{
			var copyTemplate = GetNewCopyTemplate();
			copyTemplate.S9_FilterName = "abc";
			copyTemplate.Factory.Save();

			var newCopyTemplate = GetNewCopyTemplate();
			newCopyTemplate.S9_FilterName = "xyz";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertNoNotifications(newCopyTemplate.S9_FilterNameInfo);

			newCopyTemplate.S9_FilterName = "abc";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(newCopyTemplate.S9_FilterNameInfo, "A copy template with same name already exists in current scope.");

			newCopyTemplate.S9_FilterName = "Abc";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(newCopyTemplate.S9_FilterNameInfo, "A copy template with same name already exists in current scope.");

			newCopyTemplate.S9_FilterName = "&Abc";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(newCopyTemplate.S9_FilterNameInfo, "A copy template with same name already exists in current scope.");

			newCopyTemplate.S9_FilterName = "A&bc";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(newCopyTemplate.S9_FilterNameInfo, "A copy template with same name already exists in current scope.");

			newCopyTemplate.S9_FilterName = "A&bc 1";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertNoNotifications(newCopyTemplate.S9_FilterNameInfo);
		}

		public void TestValidateS9_FilterName_HotKey()
		{
			var copyTemplate = GetNewCopyTemplate();
			copyTemplate.CopyTemplateTree.ConfigurationName = "&abbacus\\x";
			copyTemplate.Factory.Save();

			var newCopyTemplate = GetNewCopyTemplate();
			newCopyTemplate.CopyTemplateTree.ConfigurationName = "bamboo";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertNoNotifications(newCopyTemplate.S9_FilterNameInfo);

			newCopyTemplate.CopyTemplateTree.ConfigurationName = "b&amboo";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(newCopyTemplate.S9_FilterNameInfo, "Hotkey '&a' is already used in copy template '&abbacus\\x'.");

			newCopyTemplate.CopyTemplateTree.ConfigurationName = "b&amboo\\tr&ee";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(newCopyTemplate.S9_FilterNameInfo, "Hotkey '&a' from part 'b&amboo' is already used in copy template '&abbacus\\x' in part '&abbacus'.");

			newCopyTemplate.CopyTemplateTree.ConfigurationName = "&bamboo";
			newCopyTemplate.Validation.ValidateS9_FilterName();
			AssertNoNotifications(newCopyTemplate.S9_FilterNameInfo);
		}

		public void TestValidateS9_OnlyOneHotKey()
		{
			var copyTemplate = GetNewCopyTemplate();

			copyTemplate.CopyTemplateTree.ConfigurationName = "abc";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertNoNotifications(copyTemplate.S9_FilterNameInfo);

			copyTemplate.CopyTemplateTree.ConfigurationName = "&ab&c";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(copyTemplate.S9_FilterNameInfo, "Select only 1 hotkey in a copy template name part (separated by backslash '\\').");

			copyTemplate.CopyTemplateTree.ConfigurationName = "&abc";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertNoNotifications(copyTemplate.S9_FilterNameInfo);

			copyTemplate.CopyTemplateTree.ConfigurationName = @"&a\&b\&c";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertNoNotifications(copyTemplate.S9_FilterNameInfo);

			copyTemplate.CopyTemplateTree.ConfigurationName = "&&abc";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(copyTemplate.S9_FilterNameInfo, "Please use ampersand character (&) only to select hotkeys in a copy template name.");

			copyTemplate.CopyTemplateTree.ConfigurationName = "abc&";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(copyTemplate.S9_FilterNameInfo, "Please use ampersand character (&) only to select hotkeys in a copy template name.");

			copyTemplate.CopyTemplateTree.ConfigurationName = "a & c";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(copyTemplate.S9_FilterNameInfo, "Please use ampersand character (&) only to select hotkeys in a copy template name.");

			copyTemplate.CopyTemplateTree.ConfigurationName = "a && c";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertHasError(copyTemplate.S9_FilterNameInfo, "Please use ampersand character (&) only to select hotkeys in a copy template name.");

			copyTemplate.CopyTemplateTree.ConfigurationName = "a &c";
			copyTemplate.Validation.ValidateS9_FilterName();
			AssertNoNotifications(copyTemplate.S9_FilterNameInfo);
		}

		public void TestValidateS9_IsPublished()
		{
			try
			{
				var copyTemplate = GetNewCopyTemplate();
				copyTemplate.S9_ModuleID = "XXX_UC";

				copyTemplate.S9_IsPublished = true;
				copyTemplate.Validation.ValidateAll();
				AssertNoNotifications(copyTemplate.S9_IsPublishedInfo);

				EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed = false;
				copyTemplate.Validation.ValidateAll();
				AssertHasError(copyTemplate.S9_IsPublishedInfo, "You don't have permission to create published Universal Copy Templates.");

				EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed = true;
				copyTemplate.Validation.ValidateAll();
				AssertNoNotifications(copyTemplate.S9_IsPublishedInfo);

				copyTemplate.S9_IsPublished = false;
				EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed = false;
				copyTemplate.Validation.ValidateAll();
				AssertNoNotifications(copyTemplate.S9_IsPublishedInfo);
			}
			finally
			{
				EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed = true;
			}
		}

		public void TestValidateS9_CheckConflictingNames()
		{
			var copyTemplate1 = GetNewCopyTemplate();
			copyTemplate1.CopyTemplateTree.ConfigurationName = "T&1\\&T2";

			Factory.Save();

			var newCopyTemplate = GetNewCopyTemplate();
			newCopyTemplate.CopyTemplateTree.ConfigurationName = "&T1";
			newCopyTemplate.Validation.ValidateS9_FilterName();

			AssertHasError(newCopyTemplate.S9_FilterNameInfo, "The copy template name conflicts with existing copy template 'T&1\\&T2'. One template name should not be same as beginning part of other (character '&' is ignored).");

			newCopyTemplate.CopyTemplateTree.ConfigurationName = "&T1\\T&2\\T3";
			newCopyTemplate.Validation.ValidateS9_FilterName();

			AssertHasError(newCopyTemplate.S9_FilterNameInfo, "The copy template name conflicts with existing copy template 'T&1\\&T2'. One template name should not be same as beginning part of other (character '&' is ignored).");
		}

		#endregion

		public void TestValidateS9_IsPublished_UsedInCopySchedule()
		{
			var template = GetNewCopyTemplate();
			template.S9_ModuleID = "XXX_UC";

			template.S9_IsPublished = false;
			template.Validation.ValidateAll();
			AssertNoNotifications(template.S9_IsPublishedInfo);

			var copy = Factory.New<StmUniversalCopy>();
			copy.SUC_S9_CopyTemplate = template.PK;
			template.Validation.ValidateAll();
			AssertHasError(template.S9_IsPublishedInfo, "This template is used in a Copy Schedule(s) and should be published.");

			template.S9_IsPublished = true;
			template.Validation.ValidateAll();
			AssertNoNotifications(template.S9_IsPublishedInfo);
		}

		public void TestValidateIsPublishedGlobal()
		{
			try
			{
				var copyTemplate = GetNewCopyTemplate();
				copyTemplate.S9_ModuleID = "XXX_UC";

				copyTemplate.S9_IsPublished = false;
				copyTemplate.IsPublishedGlobal = true;
				copyTemplate.Validation.ValidateAll();
				AssertHasError(copyTemplate.IsPublishedGlobalInfo, "'Published' should be selected also if you want to publish template across all companies.");

				copyTemplate.S9_IsPublished = true;
				copyTemplate.Validation.ValidateAll();
				AssertNoNotifications(copyTemplate.IsPublishedGlobalInfo);

				EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowedForAllBranches = false;
				copyTemplate.Validation.ValidateAll();
				AssertHasError(copyTemplate.IsPublishedGlobalInfo, "You don't have permission to publish Universal Copy Templates for all companies.");

				EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowedForAllBranches = true;
				copyTemplate.Validation.ValidateAll();
				AssertNoNotifications(copyTemplate.IsPublishedGlobalInfo);

				copyTemplate.IsPublishedGlobal = false;
				EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowedForAllBranches = false;
				copyTemplate.Validation.ValidateAll();
				AssertNoNotifications(copyTemplate.IsPublishedGlobalInfo);
			}
			finally
			{
				EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowedForAllBranches = true;
			}
		}

		public void TestAssignHotKeyIfNeeded()
		{
			GetNewCopyTemplate().CopyTemplateTree.ConfigurationName = "&abbacus";
			GetNewCopyTemplate().CopyTemplateTree.ConfigurationName = "&barber";

			var newCopyTemplate = GetNewCopyTemplate();
			newCopyTemplate.CopyTemplateTree.ConfigurationName = "arrived";
			newCopyTemplate.PrepareForSave();
			AssertEquals("arrive&d", newCopyTemplate.CopyTemplateTree.ConfigurationName);
			AssertEquals("arrive&d", newCopyTemplate.S9_FilterName);
		}

		public void TestModuleDescription()
		{
			var template = Factory.New<UniversalCopyTemplate>();
			template.S9_ModuleID = ModuleIDs.JobShipment + UniversalCopyTemplate.ModuleIdSuffix.UniversalCopyTemplate;
			AssertEquals("Shipments", template.ModuleDescription);
		}

		public void TestPublishReadOnly()
		{
			EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed = false;
			var copyTemplate = GetNewCopyTemplate();
			Assert(copyTemplate.S9_IsPublished_ReadOnly);
			Assert(copyTemplate.IsPublishedGlobal_ReadOnly);

			EnvProxy.Instance.Security.PublishGlobalUniversalCopyTemplates.IsAllowed = true;
			copyTemplate = GetNewCopyTemplate();
			Assert(!copyTemplate.S9_IsPublished_ReadOnly);
			Assert(!copyTemplate.IsPublishedGlobal_ReadOnly);
		}

		public void TestRelatedEntityIDShouldNotBeSetToEmptyWhenPublished()
		{
			var copyTemplate = GetNewCopyTemplate();
			copyTemplate.S9_IsPublished = false;
			Assert(!copyTemplate.S9_RelatedEntityID.IsEmpty);

			copyTemplate.S9_IsPublished = true;
			Assert(!copyTemplate.S9_RelatedEntityID.IsEmpty);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			var result = (UniversalCopyTemplate)base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			result.S9_ModuleID += StmModuleFilter.ModuleIdSuffix.GridColorStrip;
			return result;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return GetNewCopyTemplate();
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			var bizo = factory.NewWithValidTestData<UniversalCopyTemplate>();
			bizo.S9_IsPublished = true;
			return bizo;
		}

		UniversalCopyTemplate GetNewCopyTemplate()
		{
			var newTemplate = Factory.New<UniversalCopyTemplate>();
			newTemplate.S9_ModuleID = "DUM_UC";
			newTemplate.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			newTemplate.CopyTemplateTree = new CopyTemplateTreeBizo(new CopyTemplateTree(GlowInterfaceReferenceAttribute.GetGlowInterfaceFromType(typeof(DummyBusinessObject), true)), newTemplate);
			return newTemplate;
		}

		#endregion
	}
}
