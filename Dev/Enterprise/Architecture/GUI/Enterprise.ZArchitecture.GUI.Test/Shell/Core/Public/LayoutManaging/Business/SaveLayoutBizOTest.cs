using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Internal.Testing
{
	[TestedType(typeof(SaveLayoutBizO))]
	sealed class SaveLayoutBizOTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSaveUserDefinedFilter()
		{
			var user = Factory.New<IGlbStaff>();
			((BusinessObject)user).FillWithValidTestData();
			user.GS_FullName = "Sir Rupert Everton";
			user.GS_Title = "Shipping Merchant who raises fancy dogs";
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(user.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var layout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "It's a secret", true, true, true);
				layout.Factory.Save();
			}
			var original = EnvProxy.Instance.Security.EditUserDefinedFilters.IsAllowed;
			try
			{
				EnvProxy.Instance.Security.EditUserDefinedFilters.IsAllowed = false;
				using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
				{
					var filterBizo = module.FilterBusinessObject;
					var filter = (ModuleTextFilter)filterBizo.FilterStrips.AddNew("Z0_Description").CurrentModuleFilter;
					filter.Property = "123";

					var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
					{
						PublishLayout = true,
						LayoutName = "It's a secret",
						IsUserDefinedFilter = true,
					};

					AssertHasError(saveLayout.LayoutNameInfo, @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Edit User-Defined Filters");
				}
			}
			finally
			{
				EnvProxy.Instance.Security.EditUserDefinedFilters.IsAllowed = original;
			}
		}

		public void TestSaveColumnLayoutIsTrueByDefault()
		{
			AssertEquals(true, SaveLayout.SaveColumnLayout);
		}

		public void TestLayoutValuesDefaultsToLastUsedLayoutValues()
		{
			var layout = TestDataHelper.NewLayout("Published Layout");
			layout.S9_IsPublished = true;
			layout.S9_SaveColumnLayout = false;
			TestDataHelper.NewLastUsedLayout(layout);
			Factory.Save();
			FilterStripBizO.LastUsedLayout = null;
			FilterStripBizO.SetLayoutsToNull();

			var saveLayoutBizO2 = new DummySaveLayoutBizO(FilterStripBizO);
			AssertEquals("Published Layout", saveLayoutBizO2.LayoutName);
			AssertEquals(true, saveLayoutBizO2.PublishLayout);
			AssertEquals(false, saveLayoutBizO2.SaveColumnLayout);
		}

		public void TestLayoutIsSystemDefined()
		{
			var layouts = new StmModuleFilterCollection(Factory, LayoutsTestDataHelper.TestModuleID, new FilterStripLayoutsHelper());

			var layoutPS3 = new LayoutsTestDataHelper(Factory).NewLayoutWithUserData("PS3");
			var layout360 = new LayoutsTestDataHelper(Factory).NewLayoutWithUserData("360");
			layout360.S9_IsSystem = true;
			Factory.Save();

			SaveLayout.LayoutName = "PS3";
			AssertEquals(false, SaveLayout.LayoutIsSystemDefined);

			SaveLayout.LayoutName = "360";
			SaveLayout.PublishLayout = true;
			AssertEquals(true, SaveLayout.LayoutIsSystemDefined);
		}

		public void TestPublishAcrossAllCompanies()
		{
			Assert("Precondition", !SaveLayout.PublishLayout);
			Assert("Precondition", !SaveLayout.PublishAcrossAllCompanies);

			Assert("Readonly when PublishLayout is false", SaveLayout.PublishAcrossAllCompaniesInfo.ReadOnly);

			SaveLayout.PublishLayout = true;

			Assert("No changes", !SaveLayout.PublishAcrossAllCompanies);
			Assert("Editable when PublishLayout is true", !SaveLayout.PublishAcrossAllCompaniesInfo.ReadOnly);

			SaveLayout.PublishAcrossAllCompanies = true;

			Assert(SaveLayout.PublishAcrossAllCompanies);

			SaveLayout.PublishLayout = false;

			Assert("Reset to false together with PublishLayout", !SaveLayout.PublishAcrossAllCompanies);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new SaveLayoutBizO(FilterStripBizO);
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			return GetNewBusinessObject();
		}

		SaveLayoutBizO SaveLayout
		{
			get { return fSaveLayout ?? (fSaveLayout = (SaveLayoutBizO)GetNewBusinessObject()); }
		}

		DummyFilterStripBusinessObject FilterStripBizO
		{
			get { return fFilterStripBizO ?? (fFilterStripBizO = new DummyFilterStripBusinessObject()); }
		}

		LayoutsTestDataHelper TestDataHelper
		{
			get { return fTestDataHelper ?? (fTestDataHelper = new LayoutsTestDataHelper(Factory)); }
		}

		SaveLayoutBizO fSaveLayout;
		DummyFilterStripBusinessObject fFilterStripBizO;
		LayoutsTestDataHelper fTestDataHelper;

		#endregion
	}

	public class SaveLayoutBizOValidationTest : BusinessObjectValidationTestCase
	{
		#region TestValidateLayoutName

		public void TestValidateLayoutName()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Maserati", true, false, false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var bizo = new DummySaveLayoutBizO(module.FilterBusinessObject) { PublishLayout = true };

				bizo.LayoutName = "Zonda";
				AssertNoErrors(bizo.LayoutNameInfo);

				bizo.LayoutName = "";
				AssertHasError(bizo.LayoutNameInfo, "Please enter a Layout Name.");

				bizo.LayoutName = "Maserati";
				AssertHasWarning(bizo.LayoutNameInfo, "The filter layout [Maserati] already exists, do you want to overwrite the existing layout?");

				bizo.PublishLayout = false;
				AssertNoWarnings(bizo.LayoutNameInfo);

				bizo.LayoutName = "[Advent";
				AssertHasError(bizo.LayoutNameInfo, "The layout name cannot start or end with square brackets.");

				bizo.LayoutName = "Advent]";
				AssertHasError(bizo.LayoutNameInfo, "The layout name cannot start or end with square brackets.");

				bizo.PublishLayout = true;
				AssertHasError(bizo.LayoutNameInfo, "The layout name cannot start or end with square brackets.");

				// test LayoutIsSystemDefined

				bizo.LayoutIsSystemDefinedCoreForTest = false;
				bizo.LayoutName = "Madrid";
				bizo.PublishLayout = false;
				AssertNoErrors(bizo.LayoutNameInfo);

				bizo.LayoutIsSystemDefinedCoreForTest = true;
				bizo.LayoutName = "Sevilla";
				AssertHasError(bizo.LayoutNameInfo, "Sevilla is a system-defined layout and cannot be overridden. Change the layout name to continue.");
			}
		}

		public void TestValidateLayoutNameWhenDefaultIsNotAllowed()
		{
			var layout = new SaveLayoutBizO(new DummyFilterStripBusinessObject());

			layout.LayoutName = StmDataGridLayoutStorage.DefaultLayoutName;
			AssertNoErrors(layout.LayoutNameInfo);

			using (var form = new ZTestGridForm(Factory.New<DummyBusinessObject>()))
			{
				form.Show();
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);

				layout = new SaveLayoutBizO(gridLayoutManageable);
				layout.LayoutName = StmDataGridLayoutStorage.DefaultLayoutName.GetUnresolvedString().ToLower();
				AssertHasError(layout.LayoutNameInfo, ZGridLayoutModification.DefaultAsFilterNameIsNotAcceptable);

				layout.LayoutName = "Test";
				AssertNoError(layout.LayoutNameInfo, ZGridLayoutModification.DefaultAsFilterNameIsNotAcceptable);
			}
		}

		public void TestValidateLayoutName_WhenSameNameExistsOnLayoutForDifferentCompany_AndNotPublishingAcrossCompanies_ShouldNotShowWarning()
		{
			var otherCompany = Factory.New<IGlbCompany>();
			((BusinessObject)otherCompany).FillWithValidTestData();
			Factory.Save();

			var existingLayout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "My Previous Company", true, false, false);
			existingLayout.S9_GC = otherCompany.PK;
			existingLayout.Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Z0_Description", "123");

				var saveLayout = new SaveLayoutBizO(filterBizo)
				{
					LayoutName = "My Previous Company",
					PublishLayout = true
				};
				saveLayout.Validation.ValidateAll();
				AssertNoWarnings("There should be no warning because the existing layout is in a different company. SAD!", saveLayout.LayoutNameInfo);
				AssertNoErrors(saveLayout.LayoutNameInfo);

				saveLayout.PublishAcrossAllCompanies = true;
				AssertHasWarning(saveLayout.LayoutNameInfo, "The filter layout [My Previous Company] already exists, do you want to overwrite the existing layout?");
				AssertNoErrors(saveLayout.LayoutNameInfo);

				saveLayout.LayoutName = "My Current Company";
				AssertNoWarnings(saveLayout.LayoutNameInfo);
				AssertNoErrors(saveLayout.LayoutNameInfo);
			}
		}

		public void TestValidateLayoutName_WhenSameNameExistsOnLayoutPublishedAcrossAllCompanies_AndNotPublishingAcrossCompanies_ShouldShowError()
		{
			var existingLayout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "My Previous Company", true, false, false);
			existingLayout.S9_GC = ZGuid.Empty; // Published across all companies
			existingLayout.Factory.Save();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Z0_Description", "123");

				var saveLayout = new SaveLayoutBizO(filterBizo)
				{
					LayoutName = "My Previous Company",
					PublishLayout = true
				};
				saveLayout.Validation.ValidateAll();
				AssertHasError(saveLayout.LayoutNameInfo, "The filter layout [My Previous Company] already exists and is published across all companies. To overwrite the existing layout, please enable the 'Publish across all companies' option.");
				AssertNoWarnings(saveLayout.LayoutNameInfo);

				saveLayout.PublishAcrossAllCompanies = true;
				AssertHasWarning(saveLayout.LayoutNameInfo, "The filter layout [My Previous Company] already exists, do you want to overwrite the existing layout?");
				AssertNoErrors(saveLayout.LayoutNameInfo);

				saveLayout.LayoutName = "My Current Company";
				AssertNoWarnings(saveLayout.LayoutNameInfo);
				AssertNoErrors(saveLayout.LayoutNameInfo);
			}
		}

		public void TestValidateLayoutName_Unpublished_WhenSameNameExistsOnUnpublishedLayoutInDifferentCompany_ForUserWithoutPublishRights_AndLastUsedLayoutWasPublishedGlobally_ShouldNotShowError()
		{
			EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches = false;
			var lastUsedLayout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "This will make the last used layout a global one.", true, true, false);

			using (var module = ZFilterModule.GetZFilterModule(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.SaveLastUsedLayout(lastUsedLayout.PK);
			}

			AssertValidateLayoutName_Unpublished_WhenSameNameExistsOnUnpublishedLayoutInDifferentCompany();
		}

		public void TestValidateLayoutName_Unpublished_WhenSameNameExistsOnUnpublishedLayoutInDifferentCompany_ShouldNotShowWarning()
		{
			AssertValidateLayoutName_Unpublished_WhenSameNameExistsOnUnpublishedLayoutInDifferentCompany();
		}

		static void AssertValidateLayoutName_Unpublished_WhenSameNameExistsOnUnpublishedLayoutInDifferentCompany()
		{
			var existingLayout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Shmloo's the Shmloss?", false, false, false);
			var company = existingLayout.Factory.New<IGlbCompany>();
			existingLayout.S9_GC = company.PK;
			existingLayout.Factory.Save();

			using (var module = ZFilterModule.GetZFilterModule(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Z0_Description", "Shmlony");

				var saveLayout = new SaveLayoutBizO(filterBizo) { PublishLayout = false, LayoutName = "Shmloo's the Shmloss?" };
				saveLayout.Validation.ValidateAll();
				AssertNoErrors(saveLayout.LayoutNameInfo);
				AssertNoWarnings(saveLayout.LayoutNameInfo);
			}
		}

		public void TestSaveUserDefinedFilter_WithSameNameAsUnpublishedUserDefinedFilter_WhichWasSavedByOtherUser_ShouldHaveError()
		{
			var user = Factory.New<IGlbStaff>();
			((BusinessObject)user).FillWithValidTestData();
			user.GS_FullName = "Sir Rupert Everton";
			user.GS_Title = "Shipping Merchant who raises fancy dogs";
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(user.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var layout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "It's a secret", false, false, true);
				layout.Factory.Save();
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = (ModuleTextFilter)filterBizo.FilterStrips.AddNew("Z0_Description").CurrentModuleFilter;
				filter.Property = "123";

				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
				{
					LayoutName = "It's a secret",
					IsUserDefinedFilter = true,
				};

				AssertHasError(saveLayout.LayoutNameInfo, @"The layout description [It's a secret] is in use as an unpublished User-Defined Filter. [It's a secret] was created by Sir Rupert Everton and cannot be overwritten by a different user. Please enter a different description.");

				saveLayout.IsUserDefinedFilter = false;
				AssertHasError(saveLayout.LayoutNameInfo, @"The user-defined filter [It's a secret] already exists and cannot be overwritten by a saved layout. Please enter a different name.");

				saveLayout.LayoutName = "You wouldn't understand";
				AssertNoErrors(saveLayout.LayoutNameInfo);
			}
		}

		#endregion

		#region TestValidatePublishLayout

		public void TestValidatePublishLayout()
		{
			EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed = false;

			BizO.PublishLayout = false;
			AssertNoErrors(BizO.PublishLayoutInfo);

			BizO.PublishLayout = true;
			AssertHasError(BizO.PublishLayoutInfo, EnvProxy.Instance.Security.PublishGlobalFilterLayouts.ErrorMessageForNotAllowed);

			EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed = true;

			BizO.PublishLayout = false;
			AssertNoErrors(BizO.PublishLayoutInfo);

			BizO.PublishLayout = true;
			AssertNoErrors(BizO.PublishLayoutInfo);
		}

		public void TestValidatePublishLayout_ForUserDefinedFilter()
		{
			EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed = false;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = (ModuleTextFilter)filterBizo.FilterStrips.AddNew("Z0_Description").CurrentModuleFilter;
				filter.Property = "123";

				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
				{
					LayoutName = "Anyone can use this",
					PublishLayout = true
				};

				AssertNoErrors(saveLayout.PublishLayoutInfo);

				saveLayout.IsUserDefinedFilter = true;
				AssertHasError(saveLayout.PublishLayoutInfo, @"You do not have permission to publish user-defined filters. You may only save unpublished user-defined filters, which will be accessible only to you.");

				saveLayout.PublishLayout = false;
				AssertNoErrors(saveLayout.PublishLayoutInfo);
			}

			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Anyone can use this", true, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = (ModuleTextFilter)filterBizo.FilterStrips.AddNew("Z0_Description").CurrentModuleFilter;
				filter.Property = "123";

				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
				{
					LayoutName = "Anyone can use this",
					PublishLayout = true
				};

				AssertHasError(saveLayout.LayoutNameInfo, @"The user-defined filter [Anyone can use this] already exists and cannot be overwritten by a saved layout. Please enter a different name.");

				saveLayout.IsUserDefinedFilter = true;
				AssertHasError(saveLayout.PublishLayoutInfo, @"You do not have permission to publish user-defined filters. You may only save unpublished user-defined filters, which will be accessible only to you.");

				EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed = true;
				saveLayout.Validation.ValidateAll();
				AssertNoErrors(saveLayout.PublishLayoutInfo);
				AssertHasWarning(saveLayout.LayoutNameInfo, @"The user-defined filter [Anyone can use this] already exists, do you want to overwrite the existing user-defined filter?");
			}
		}

		#endregion

		#region TestValidatePublishAcrossAllCompanies

		public void TestValidatePublishAcrossAllCompanies()
		{
			EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches = false;

			BizO.PublishAcrossAllCompanies = false;
			AssertNoErrors(BizO.PublishAcrossAllCompaniesInfo);

			BizO.PublishAcrossAllCompanies = true;
			AssertHasError(BizO.PublishAcrossAllCompaniesInfo, EnvProxy.Instance.Security.PublishGlobalFilterLayouts.ErrorMessageForNotAllowed);

			EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches = true;

			BizO.PublishAcrossAllCompanies = false;
			AssertNoErrors(BizO.PublishAcrossAllCompaniesInfo);

			BizO.PublishAcrossAllCompanies = true;
			AssertNoErrors(BizO.PublishAcrossAllCompaniesInfo);
		}

		#endregion

		public void TestValidateSaveColumnLayout()
		{
			// Parent.PublishLayout
			EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed = false;

			BizO.PublishLayout = true;
			BizO.SaveColumnLayout = false;
			AssertNoErrors(BizO.SaveColumnLayoutInfo);

			BizO.SaveColumnLayout = true;
			AssertHasError(BizO.SaveColumnLayoutInfo, EnvProxy.Instance.Security.PublishGlobalFilterLayouts.ErrorMessageForNotAllowed);

			BizO.PublishLayout = false;
			BizO.SaveColumnLayout = false;
			AssertNoErrors(BizO.SaveColumnLayoutInfo);

			BizO.SaveColumnLayout = true;
			AssertNoErrors(BizO.SaveColumnLayoutInfo);

			EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed = true;

			BizO.SaveColumnLayout = true;
			AssertNoErrors(BizO.SaveColumnLayoutInfo);

			BizO.SaveColumnLayout = false;
			AssertNoErrors(BizO.SaveColumnLayoutInfo);
		}

		#region User Defined Filters

		public void TestSaveUserDefinedFilter_ShouldCheckForNestedInstancesOfItself()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
				((ModuleTextFilter)strip.CurrentModuleFilter).Property = "Keyokuk";
				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "First", false, false, true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.FilterStrips.AddNew("[USR]First");
				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
				{
					LayoutName = "First",
					IsUserDefinedFilter = true
				};

				AssertHasError(saveLayout.IsUserDefinedFilterInfo, "A user-defined filter strip cannot contain a filter strip that references itself. Please remove the [First] filter strip before saving.");
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.FilterStrips.AddNew("[USR]First");
				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "Second", false, false, true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.FilterStrips.AddNew("[USR]Second");
				FilterStripsTestHelper.SaveFilterLayout(module.FilterBusinessObject, "Third", false, false, true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.FilterStrips.AddNew("[USR]Third");
				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
				{
					LayoutName = "First",
					IsUserDefinedFilter = true
				};

				AssertHasError(saveLayout.IsUserDefinedFilterInfo, @"A user-defined filter strip cannot contain a filter strip that references itself. The [Third] filter strip contains a reference to the filter being saved:
First -> Third -> Second -> First");

				saveLayout.LayoutName = "Forth";
				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
				((ModuleTextFilter)strip.CurrentModuleFilter).Property = "Keyokuk";

				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
				{
					LayoutName = "First",
					IsUserDefinedFilter = true
				};

				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);
			}
		}

		public void TestSaveUserDefinedFilter_Unpublished_WhenPartOfPublishedFiltersOrLayouts_ShouldHaveError()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = (ModuleTextFilter)filterBizo.FilterStrips.AddNew("Z0_Description").CurrentModuleFilter;
				filter.Property = "Shalala";

				FilterStripsTestHelper.SaveFilterLayout(filterBizo, "Try to be unpublished", true, true, true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("[USR]Try to be unpublished");

				FilterStripsTestHelper.SaveFilterLayout(filterBizo, "Published User-Defined", true, true, isUserDefinedFilter: true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var strip = module.FilterBusinessObject.FilterStrips.AddNew("Z0_Description");
				((ModuleTextFilter)strip.CurrentModuleFilter).Property = "Keyokuk";

				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
				{
					LayoutName = "Try to be unpublished",
					IsUserDefinedFilter = true,
					PublishLayout = false
				};

				AssertHasError(saveLayout.PublishLayoutInfo, "You cannot make this layout unpublished as it has already been published.");
				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);

				saveLayout.PublishLayout = true;
				AssertNoErrors(saveLayout.PublishLayoutInfo);
				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);
			}
		}

		public void TestPublishedLayout_WithUnpublishedUserDefinedFilters_ShouldHaveError()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.DummyDependent))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddFilterStrip<ModuleNumberRangeFilter>("ZD1_Number");

				FilterStripsTestHelper.SaveFilterLayout(filterBizo, "I'm not published, sadly", false, false, true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.AddTextFilterStrip("Z0_Description", "Shalala");

				FilterStripsTestHelper.SaveFilterLayout(filterBizo, "How about another?", false, false, true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.DummyDependent))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("[USR]I'm not published, sadly");

				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
				{
					LayoutName = "All shall see this user-defined filter",
					IsUserDefinedFilter = true,
					PublishLayout = true
				};

				AssertHasError(saveLayout.PublishLayoutInfo, @"The layout being saved is published, and yet it contains the following unpublished user-defined filter:

I'm not published, sadly

Unpublished filters cannot be included in published layouts.");

				saveLayout.IsUserDefinedFilter = false;
				AssertHasError(saveLayout.PublishLayoutInfo, @"The layout being saved is published, and yet it contains the following unpublished user-defined filter:

I'm not published, sadly

Unpublished filters cannot be included in published layouts.");

				var dummyFilter = filterBizo.AddGuidFilterStrip("Dummy");
				dummyFilter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				dummyFilter.SelectedFilters.AddFilterStrip<ModuleUserDefinedFilter>("[USR]How about another?");
				saveLayout.Validation.ValidatePublishLayout();

				AssertHasError(saveLayout.PublishLayoutInfo, @"The layout being saved is published, and yet it contains the following unpublished user-defined filters:

I'm not published, sadly
Dummy -> How about another?

Unpublished filters cannot be included in published layouts.");

				saveLayout.PublishLayout = false;
				AssertNoErrors(saveLayout.PublishLayoutInfo);
			}
		}

		public void TestCheckUserDefinedFilter_FromLoadedLayoutContainingFiltersMatchFilter_WithNoModuleId_ShouldNotThrowException()
		{
			var filterBizo = new DummyDependentFilterBusinessObject();
			var filter = filterBizo.AddFilterStrip<ModuleGuidModuleSpecifiedFilter>("Dummy with Specified Module");

			filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
			filter.SelectedModule = DummyModuleIDs.Dummy.Name;
			filter.SelectedFilters.AddTextFilterStrip("Z0_Description", "Shalala");
			SaveLayoutBizO saveLayout = null;

			AssertNoExceptionThrown("No exceptions should be thrown during validation, and yet...", () =>
			{
				saveLayout = new SaveLayoutBizO(filterBizo)
				{
					LayoutName = "I hope this doesn't explode",
					IsUserDefinedFilter = true
				};
			});

			AssertHasError(saveLayout.IsUserDefinedFilterInfo, "User-Defined Filters are not supported for this module.");
		}

		public void TestEmbeddedPopupSaveButton_WithPublishedNestedUserDefinedFilter_ShouldNotHaveNestedUnpublishedFilterError()
		{
			FilterStripsTestHelper.CreateUserDefinedFilterStrip_TwoProperties("Innie", isPublished: true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				filterBizo.FilterStrips.AddNew("[USR]Innie");

				FilterStripsTestHelper.SaveFilterLayout(filterBizo, "Outie", true, false, true);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = (ZForm)module.ShowPopup())
			{
				Application.DoEvents();

				var filterControl = form.FindSingle<ZFilterStripBaseControl>();
				var filterStrip = filterControl.FindSingle<ZFilterStrip>();
				filterStrip.FilterDescriptionDropEdit.CodeBox.Text = @"Outie [+]";
				module.Grid.Focus();
				var filterFindBox = filterStrip.FindSingle<ZFilterCollectionFindBox>();

				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog =>
				{
					var popup = dialog as UserDefinedFilterEmbeddedModulePopup;
					var saveForm = dialog as SaveLayoutForm;

					if (popup != null)
					{
						popup.FilterStripsLoaded += (_, x_) =>
						{
							var embeddedFilterControl = popup.Module.DisplayGrid.GetParentFilterControl();
							embeddedFilterControl.ToolStripSaveLayoutButton_ForTest.PerformClick();
							Application.DoEvents();
						};
					}
					else if (saveForm != null)
					{
						var saveBizo = (SaveLayoutBizO)saveForm.BusinessEntity;
						AssertNoErrors(saveBizo.PublishLayoutInfo);
						AssertEquals("Outie", saveBizo.LayoutName);
						AssertEquals(true, saveBizo.PublishLayout);
						AssertEquals(true, saveBizo.IsUserDefinedFilter);
					}
				});

				filterFindBox.PopupButton.PerformClick();
				Application.DoEvents();
			}
		}

		public void TestIsUserDefinedFilter_ForUnsupportedModule_ShouldHaveError()
		{
			using (var module = ZFilterModule.GetZFilterModule(DummyModuleIDs.Dummy)) // Supported module (ZFilterModule)
			{
				var filterBizo = module.FilterBusinessObject;
				var saveLayout = new SaveLayoutBizO(filterBizo);
				saveLayout.Validation.ValidateAll();

				AssertEquals(false, saveLayout.IsUserDefinedFilter);
				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);

				saveLayout.IsUserDefinedFilter = true;
				AssertEquals(true, saveLayout.IsUserDefinedFilter);

				saveLayout.Validation.ValidateAll();
				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);
			}

			using (var module = ZModule.GetZModule(ModuleIDs.CartageLegPlanner)) // Unsupported module (ZPopupModule)
			using (var popup = (Form)module.ShowPopup())
			{
				var control = popup.FindSingle<StripControl>();
				var filterBizo = control.FilterBusinessObject;
				var saveLayout = new SaveLayoutBizO(filterBizo);
				saveLayout.Validation.ValidateAll();

				AssertEquals(false, saveLayout.IsUserDefinedFilter);
				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);

				saveLayout.IsUserDefinedFilter = true;
				AssertHasError(saveLayout.IsUserDefinedFilterInfo, "User-Defined Filters are not supported for this module.");

				saveLayout.IsUserDefinedFilter = false;
				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);
			}
		}

		public void TestOverrideGlobalFilterLayout()
		{
			var user = Factory.New<IGlbStaff>();
			((BusinessObject)user).FillWithValidTestData();
			user.GS_FullName = "Test Full Name";
			user.GS_Title = "Test Title";
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(user.GS_LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var layout = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Test", true, true, true);
				layout.Factory.Save();
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var filter = (ModuleTextFilter)filterBizo.FilterStrips.AddNew("Z0_Description").CurrentModuleFilter;
				filter.Property = "123";

				EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowedForAllBranches = false;

				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject)
				{
					LayoutName = "Test"
				};

				saveLayout.IsPublishAcrossAllCompaniesOrginalValue = false;
				saveLayout.Validation.ValidateLayoutName();
				AssertHasError(saveLayout.LayoutNameInfo, "The user-defined filter [Test] already exists and cannot be overwritten by a saved layout. Please enter a different name.");

				saveLayout.IsPublishAcrossAllCompaniesOrginalValue = true;
				saveLayout.Validation.ValidateLayoutName();
				AssertHasError(saveLayout.LayoutNameInfo, "You do not have the appropriate security rights to override this layout.");
			}
		}

		public void TestCheckIsUserDefinedFilter_Unpublished_WithLayoutNameMatchingFilterNameInPublishedSavedLayoutOfStmModuleFilterWithInvalidS9_ModuleID_ShouldNotThrowExceptions()
		{
			const string squanch = "Squanch";
			var moduleId = ModuleIDs.AllIncludingClientModules.FirstOrDefault(x => x.Name == squanch);
			AssertNull("This test must test a nonexistant module in order to work.", moduleId);

			CreateSavedLayoutForUnpublishedCheckTests(squanch);
			AssertCheckIsUserDefinedFilter_ShouldNotThrowExceptions("Validating the user defined filter should not throw an exception just because there is an StmModuleFilter (with an invalid ModuleID) in the database that has a filter matching the layout's name. SAD!");
		}

		public void TestCheckIsUserDefinedFilter_Unpublished_WithLayoutNameMatchingFilterNameInPublishedSavedLayoutOfStmModuleFilteForNonZFilterModule_ShouldNotThrowExceptions()
		{
			using (var module = ZModuleFactory.Instance.Create(ModuleIDs.CartageLegPlanner))
			{
				AssertEquals("CartageLegPlanner is used as an example of a non ZFilterModule module. If that ever changes, this test should be updated with a different popup module.", true, module.GetType().IsSubclassOf(typeof(ZPopupModule)));
			}

			CreateSavedLayoutForUnpublishedCheckTests(ModuleIDs.CartageLegPlanner.Name);
			AssertCheckIsUserDefinedFilter_ShouldNotThrowExceptions("Validating the user defined filter should not throw an exception just because there is a saved layout for a module that isn't a ZFilterModule in the database that has a filter matching the layout's name. SAD!");
		}

		public void TestCheckIsUserDefinedFilter_Unpublished_WithLayoutNameMatchingFilterNameInPublishedSavedLayoutOfStmModuleFilteForZFilterModuleWithNullFilterStripBusinessObject_ShouldNotThrowExceptions()
		{
			using (var module = ZFilterModule.GetZFilterModule(ModuleIDs.Customs.EU.GB.CcsukSplitBasic, "GB"))
			{
				AssertNull("CcsukSplitBasic is used as an example of a ZFilterModule module that returns a null FilterBusinessObject. If that ever changes, this test should be updated with a different module.", module.FilterBusinessObject);
			}

			CreateSavedLayoutForUnpublishedCheckTests(ModuleIDs.Customs.EU.GB.CcsukSplitBasic.Name);
			AssertCheckIsUserDefinedFilter_ShouldNotThrowExceptions("Validating the user defined filter should not throw an exception just because there is a saved layout for a module that has a null FilterBusinessObject in the database that has a filter matching the layout's name. SAD!");
		}
		static void CreateSavedLayoutForUnpublishedCheckTests(string moduleName)
		{
			using (var module = ZFilterModule.GetZFilterModule(DummyModuleIDs.Dummy))
			{
				module.FilterBusinessObject.AddTextFilterStrip("Z0_Description", "Shalala");
				var layout = module.FilterBusinessObject.SaveLayout("Shalala's filters");
				layout.S9_ModuleID = moduleName;
				layout.S9_IsPublished = true;
				layout.Factory.Save();
			}
		}

		static void AssertCheckIsUserDefinedFilter_ShouldNotThrowExceptions(string message)
		{
			using (var module = ZFilterModule.GetZFilterModule(DummyModuleIDs.Dummy))
			{
				var saveLayout = new SaveLayoutBizO(module.FilterBusinessObject);
				saveLayout.LayoutName = "Z0_Description";
				AssertNoExceptionThrown(message, () => saveLayout.IsUserDefinedFilter = true);
				AssertNoErrors(saveLayout.IsUserDefinedFilterInfo);
			}
		}

		#endregion

		#region Implementation

		DummySaveLayoutBizO BizO
		{
			get { return fBizO ?? (fBizO = new DummySaveLayoutBizO(new DummyFilterStripBusinessObject())); }
		}

		DummySaveLayoutBizO fBizO;

		#endregion
	}

	#region class DummySaveLayoutBizO

	class DummySaveLayoutBizO : SaveLayoutBizO
	{
		public DummySaveLayoutBizO(FilterStripBusinessObject filterStripBizO)
			: base(filterStripBizO)
		{
		}

		protected override bool LayoutIsSystemDefinedCore
		{
			get { return LayoutIsSystemDefinedCoreForTest; }
		}

		public bool LayoutIsSystemDefinedCoreForTest;
	}

	#endregion
}
