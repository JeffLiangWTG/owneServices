using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.FilterStrips;
using Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.GUI.FilterStrips.Testing
{
	[TestedType(typeof(ManageLayoutsBusinessObject))]
	sealed class ManageLayoutsBusinessObjectTest : NonPersistentBusinessObjectTestCase
	{
		#region TestSelectedFilterLayoutNameEscaped

		public void TestSelectedFilterLayoutNameEscaped()
		{
			ManageLayoutsBizO.SelectedFilterLayoutName = "ike's toys";
			AssertEquals(@"ike\'s toys", ManageLayoutsBizO.SelectedFilterLayoutNameEscaped);
		}

		#endregion

		#region TestFilterLayoutExistsWithName

		public void TestFilterLayoutExistsWithName()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter barneyLayout = TestDataHelper.NewLayoutWithUserData("Barney", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			AssertEquals(true, ManageLayoutsBizO.FilterLayoutExistsWithName("fREd", false));
			AssertEquals(true, ManageLayoutsBizO.FilterLayoutExistsWithName("BarNey", false));
			AssertEquals(false, ManageLayoutsBizO.FilterLayoutExistsWithName("moo", false));

			ManageLayoutsBizO.MarkFilterLayoutForRename(fredLayout, "Scooby");
			AssertEquals(false, ManageLayoutsBizO.FilterLayoutExistsWithName("Fred", false));
			AssertEquals(true, ManageLayoutsBizO.FilterLayoutExistsWithName("ScOOby", false));

			ManageLayoutsBizO.MarkFilterLayoutForDelete(barneyLayout);
			AssertEquals(false, ManageLayoutsBizO.FilterLayoutExistsWithName("Barney", false));
		}

		#endregion

		#region TestGetSelectedFilterLayout

		public void TestGetSelectedFilterLayout()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			ManageLayoutsBizO.SelectedFilterLayoutName = "";
			AssertNull(ManageLayoutsBizO.GetSelectedFilterLayout());

			ManageLayoutsBizO.SelectedFilterLayoutName = "fREd";
			AssertEquals(fredLayout.PK, ManageLayoutsBizO.GetSelectedFilterLayout().PK);

			ManageLayoutsBizO.MarkFilterLayoutForRename(fredLayout, "Scooby");
			AssertEquals(null, ManageLayoutsBizO.GetSelectedFilterLayout());

			ManageLayoutsBizO.SelectedFilterLayoutName = "ScOOby";
			AssertEquals(fredLayout.PK, ManageLayoutsBizO.GetSelectedFilterLayout().PK);

			ManageLayoutsBizO.MarkFilterLayoutForDelete(fredLayout);
			AssertEquals(null, ManageLayoutsBizO.GetSelectedFilterLayout());
		}

		#endregion

		#region TestFilterLayouts

		public void TestFilterLayouts()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter barneyLayout = TestDataHelper.NewLayoutWithUserData("Barney", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter systemLayout = TestDataHelper.NewPublishedOrganisationLayoutForWeb("System Default", FilterStripBizO.LayoutsHelper.CurrentOrganisationPk);
			Factory.Save();
			AssertEquals(true, ManageLayoutsBizO.FilterLayouts.ContainsOnly("Fred", "Barney", "System Default"));

			systemLayout.S9_IsSystem = true;
			Factory.Save();

			//Reset BizO to force it reload from DB
			fManageLayoutsBizO = null; // Reset ManageLayoutsBizO
			fFilterStripBizO = null;

			AssertNotNull("ManageLayoutsBizO", ManageLayoutsBizO);
			AssertEquals(true, ManageLayoutsBizO.FilterLayouts.ContainsOnly("Fred", "Barney"));

			ManageLayoutsBizO.MarkFilterLayoutForDelete(fredLayout);
			AssertEquals("Barney", ManageLayoutsBizO.FilterLayouts.CodesAsString);

			ManageLayoutsBizO.MarkFilterLayoutForRename(barneyLayout, "Scooby");
			AssertEquals("Scooby", ManageLayoutsBizO.FilterLayouts.CodesAsString);
		}

		public void TestFilterLayoutsNotContainOthers()
		{
			var otherOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, SQLComparisonOperator.NotEqual, FilterStripBizO.LayoutsHelper.CurrentOrganisationPk));
			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, EnvProxy.Instance.CurrentCompany.PK));

			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter barneyLayout = TestDataHelper.NewLayoutWithUserData("Barney", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter organisationLayout = TestDataHelper.NewPublishedOrganisationLayoutForWeb("Organisation", FilterStripBizO.LayoutsHelper.CurrentOrganisationPk);
			StmModuleFilter otherOrganisationLayout = TestDataHelper.NewPublishedOrganisationLayoutForWeb("Other Organisation", otherOrg.PK);
			StmModuleFilter companyLayout = TestDataHelper.NewPublishedCompanyLayoutForWeb("Company", EnvProxy.Instance.CurrentCompany.PK);
			StmModuleFilter otherCompanyLayout = TestDataHelper.NewPublishedCompanyLayoutForWeb("Other Company", otherCompany.PK);
			StmModuleFilter systemLayout = TestDataHelper.NewLayout("System Default");
			systemLayout.S9_IsSystem = true;

			Factory.Save();

			AssertEquals(true, ManageLayoutsBizO.FilterLayouts.ContainsOnly("Fred", "Barney", "Organisation", "Company"));
		}

		#endregion

		#region TestDeleteFilterLayoutsMarkedForDelete

		public void TestDeleteFilterLayoutsMarkedForDelete()
		{
			StmModuleFilter gsxrLayout = TestDataHelper.NewLayoutWithUserData("GSXR", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter cbrLayout = TestDataHelper.NewLayoutWithUserData("CBR", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			ZGuid gsxrLayoutPK = gsxrLayout.PK;
			ZGuid cbrLayoutPK = cbrLayout.PK;

			AssertEquals("Precondition", true, ManageLayoutsBizO.FilterLayouts.ContainsCode("GSXR"));
			AssertEquals("Precondition", true, ManageLayoutsBizO.FilterLayouts.ContainsCode("CBR"));

			ManageLayoutsBizO.MarkFilterLayoutForDelete(gsxrLayout);
			ManageLayoutsBizO.MarkFilterLayoutForDelete(cbrLayout);

			AssertEquals(false, ManageLayoutsBizO.FilterLayouts.ContainsCode("GSXR"));
			AssertEquals(false, ManageLayoutsBizO.FilterLayouts.ContainsCode("CBR"));

			AssertEquals(2, FilterStripBizO.Layouts.Count);

			ManageLayoutsBizO.DeleteFilterLayoutsMarkedForDelete();
			Factory.Save();

			AssertEquals(0, FilterStripBizO.Layouts.Count);
		}

		#endregion

		#region TestMarkFilterLayoutForDelete

		public void TestMarkFilterLayoutForDelete()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();
			AssertEquals("Precondition", true, ManageLayoutsBizO.FilterLayouts.ContainsCode("Fred"));

			ManageLayoutsBizO.MarkFilterLayoutForDelete(fredLayout);
			AssertEquals(false, ManageLayoutsBizO.FilterLayouts.ContainsCode("Fred"));
			AssertEquals("Mark for delete should not change the filter layout object.", false, fredLayout.IsDeleted);
		}

		#endregion

		#region TestDeleteFilterLayoutsMarkedForDelete_WhenFilterLayoutInUse

		public void TestDeleteFilterLayoutsMarkedForDelete_WhenFilterLayoutInUse()
		{
			var testLayout = TestDataHelper.NewLayoutWithUserData("[Standard Booking Layout]", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			var companyPk = EnvProxy.Instance.CurrentCompany.PK;
			var regItem = WebDataRegistry.Instance.DefaultFilterLayoutForwardingBookings;
			regItem.SetValue(companyPk, Guid.Empty, Guid.Empty, "[Standard Booking Layout]");

			ManageLayoutsBizO.MarkFilterLayoutForDelete(testLayout);
			ManageLayoutsBizO.DeleteFilterLayoutsMarkedForDelete();
			Factory.Save();

			var currentRegistryName = regItem.GetValueWithoutFallback(companyPk, Guid.Empty, Guid.Empty);

			AssertNotEquals("[Standard Booking Layout]", currentRegistryName);
			AssertEquals("System Default Layout", currentRegistryName);
		}

		#endregion

		#region TestMarkFilterLayoutForRename

		public void TestMarkFilterLayoutForRename()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();
			AssertEquals("Precondition", true, ManageLayoutsBizO.FilterLayouts.ContainsCode("Fred"));

			ManageLayoutsBizO.MarkFilterLayoutForRename(fredLayout, "Scooby");
			AssertEquals(false, ManageLayoutsBizO.FilterLayouts.ContainsCode("Fred"));
			AssertEquals(true, ManageLayoutsBizO.FilterLayouts.ContainsCode("Scooby"));
			AssertEquals("Mark for rename should not change the filter layout object.", "Fred", fredLayout.S9_FilterName);
		}

		#endregion

		#region TestSaveChangesToFilterStripBizO

		public void TestSaveChangesToFilterStripBizO()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter barneyLayout = TestDataHelper.NewLayoutWithUserData("Barney", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();
			StmModuleFilter fredLayoutInFilterStripBizOFactory = (StmModuleFilter)FilterStripBizO.Layouts.FindByPK(fredLayout.PK);
			StmModuleFilter barneyLayoutInFilterStripBizOFactory = (StmModuleFilter)FilterStripBizO.Layouts.FindByPK(barneyLayout.PK);

			ManageLayoutsBizO.MarkFilterLayoutForRename(fredLayout, "Scooby");
			ManageLayoutsBizO.MarkFilterLayoutForDelete(barneyLayout);
			AssertEquals("Fred", fredLayoutInFilterStripBizOFactory.S9_FilterName);
			AssertEquals(false, barneyLayoutInFilterStripBizOFactory.IsDeleted);

			ManageLayoutsBizO.SaveChangesToFilterStripBusinessObject();
			AssertEquals("Scooby", fredLayoutInFilterStripBizOFactory.S9_FilterName);
			AssertEquals(true, barneyLayoutInFilterStripBizOFactory.IsDeleted);

			FilterStripBizO.Factory.Saving += delegate
			{
				Fail("Attempted to save changes twice - the deleted/renamed pk lists were not probably not cleared.");
			};
			ManageLayoutsBizO.SaveChangesToFilterStripBusinessObject();
		}

		#endregion

		#region TestSaveChangesToFilterStripBizODoesNotHitDbIfNoChanges

		public void TestSaveChangesToFilterStripBizODoesNotHitDbIfNoChanges()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter barneyLayout = TestDataHelper.NewLayoutWithUserData("Barney", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			FilterStripBizO.Factory.Saving += delegate
			{
				Fail("Filter layouts are unchanged, should not be calling FilterStripBizO.Factory.Save()");
			};

			ManageLayoutsBizO.SaveChangesToFilterStripBusinessObject();
			Assert(true);
		}

		#endregion

		#region TestDeletingCurrentLayoutResetsLastSavedLayoutName

		public void TestDeletingCurrentLayoutResetsLastSavedLayout()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmData data = TestDataHelper.NewLastUsedLayout(fredLayout, Contact.PK);
			Factory.Save();

			ManageLayoutsBizO.SelectedFilterLayoutName = "Fred";
			ManageLayoutsBizO.MarkFilterLayoutForDelete(fredLayout);
			AssertEquals("Precondition", "Fred", FilterStripBizO.LastUsedLayout.S9_FilterName);
			AssertEquals("Precondition", false, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);

			ManageLayoutsBizO.SaveChangesToFilterStripBusinessObject();
			AssertNull(FilterStripBizO.LastUsedLayout);
			AssertEquals(true, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);
		}

		#endregion

		#region TestRenamingCurrentLayoutUpdatesLastSavedLayoutName

		public void TestRenamingCurrentLayoutUpdatesLastSavedLayoutName()
		{
			StmModuleFilter fredLayout = TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			TestDataHelper.NewLastUsedLayout(fredLayout, Contact.PK);
			Factory.Save();

			ManageLayoutsBizO.SelectedFilterLayoutName = "Fred";
			ManageLayoutsBizO.MarkFilterLayoutForRename(fredLayout, "Barney");
			AssertEquals("Precondition", "Fred", FilterStripBizO.LastUsedLayout.S9_FilterName);

			ManageLayoutsBizO.SaveChangesToFilterStripBusinessObject();
			AssertEquals("Barney", FilterStripBizO.LastUsedLayout.S9_FilterName);
		}

		#endregion

		#region TestResetChanges

		public void TestResetChanges()
		{
			StmModuleFilter gargamelLayout = TestDataHelper.NewLayoutWithUserData("Gargamel", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter azraelLayout = TestDataHelper.NewLayoutWithUserData("Azrael", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			ManageLayoutsBizO.MarkFilterLayoutForDelete(gargamelLayout);
			ManageLayoutsBizO.MarkFilterLayoutForRename(azraelLayout, "Scruple");
			ManageLayoutsBizO.SelectedFilterLayoutName = "Scruple";
			AssertEquals("Precondition", false, ManageLayoutsBizO.FilterLayoutExistsWithName("Gargamel", false));
			AssertEquals("Precondition", false, ManageLayoutsBizO.FilterLayoutExistsWithName("Azrael", false));
			AssertEquals("Precondition", "Scruple", ManageLayoutsBizO.SelectedFilterLayoutName);

			ManageLayoutsBizO.ResetChanges();
			AssertEquals(true, ManageLayoutsBizO.FilterLayoutExistsWithName("Gargamel", false));
			AssertEquals(true, ManageLayoutsBizO.FilterLayoutExistsWithName("Azrael", false));
			AssertEquals("", ManageLayoutsBizO.SelectedFilterLayoutName);

			FilterStripBizO.Factory.Saving += delegate
			{
				Fail("Changes were reset, should not be calling FilterStripBizO.Factory.Save()");
			};
			ManageLayoutsBizO.SaveChangesToFilterStripBusinessObject();
		}

		#endregion

		#region User-Defined Filters

		public void TestFilterLayouts_ShouldNotIncludeUserDefinedFilters()
		{
			FilterStripsTestHelper.SaveFilterLayout(FilterStripBizO, "User-Defined", true, true, isUserDefinedFilter: true);
			FilterStripsTestHelper.SaveFilterLayout(FilterStripBizO, "Regular layout", true, true, isUserDefinedFilter: false);

			AssertContainsExactElementsInAnyOrder("User-defined filters shouldn't be included because code for managing layouts is duplicated here and in the main architecture, which is bad. So until that is refactored into a shared place, changes should only be allowed to be made in one place.",
					new[] { "Regular layout" }, ManageLayoutsBizO.FilterLayouts.GetAllCodes());
		}

		#endregion

		#region Implementation

		protected override void SetUp()
		{
			Globals.IsWeb = true;
		}

		OrgContactWebUser TestUser
		{
			get
			{
				if (testUser == null)
				{
					testUser = new OrgContactWebUser();
					testUser.LoginSupportForTest(GlbCompany.CurrentCompany.OrgProxy.OH_Code);
				}
				return testUser;
			}
		}

		OrgContactWebUser testUser;
		protected override BusinessObject GetNewBusinessObject()
		{
			return new ManageLayoutsBusinessObject(FilterStripBizO, TestUser);
		}

		ManageLayoutsBusinessObject ManageLayoutsBizO
		{
			get { return fManageLayoutsBizO ?? (fManageLayoutsBizO = new ManageLayoutsBusinessObject(FilterStripBizO, TestUser)); }
		}

		DummyFilterStripBusinessObjectForWeb FilterStripBizO
		{
			get
			{
				if (fFilterStripBizO == null)
				{
					fFilterStripBizO = new DummyFilterStripBusinessObjectForWeb();
					fFilterStripBizO.LayoutsHelper = new FilterStripLayoutsHelperForWeb(fFilterStripBizO, Contact, WebDataRegistry.Instance.DefaultFilterLayoutForwardingBookings);
				}
				return fFilterStripBizO;
			}
		}

		OrgContact Contact
		{
			get { return TestUser.LoggedInUser; }
		}

		LayoutsTestDataHelper TestDataHelper
		{
			get { return fTestDataHelper ?? (fTestDataHelper = new LayoutsTestDataHelper(Factory)); }
		}

		ManageLayoutsBusinessObject fManageLayoutsBizO;
		DummyFilterStripBusinessObjectForWeb fFilterStripBizO;
		LayoutsTestDataHelper fTestDataHelper;

		#endregion
	}
}
