using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Web.Business.FilterStrips.Testing
{
	[TestedType(typeof(FilterStripLayoutsHelperForWeb))]
	sealed class FilterStripLayoutsHelperForWebTest : NonPersistentBusinessObjectTestCase
	{
		#region TestCurrentUserPk

		public void TestCurrentUserPk()
		{
			AssertEquals(Contact.PK, FilterStripBizO.LayoutsHelper.CurrentUserPk);
		}

		#endregion

		#region TestCurrentLayoutName

		public void TestCurrentLayout()
		{
			var savedLayout = TestDataHelper.NewLayoutWithUserData("Smurfette", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			AssertNull("Should be empty by default", FilterStripBizO.LayoutsHelper.CurrentLayout);

			FilterStripBizO.LayoutsHelper.DeleteCurrentLayout();
			AssertEquals("Precondition", true, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);

			FilterStripBizO.LayoutsHelper.CurrentLayout = savedLayout;
			AssertEquals("Smurfette", FilterStripBizO.LayoutsHelper.CurrentLayout.S9_FilterName);
			AssertEquals("Selecting a new layout should reset the IsCurrentLayoutDeleted flag.", false, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);
		}

		public void TestCurrentLayoutName()
		{
			var savedLayout = TestDataHelper.NewLayoutWithUserData("Smurfette", Contact.PK, OrgContactSchema.Constants.Prefix);
			savedLayout.S9_IsPublished = true;
			Factory.Save();

			FilterStripBizO.LayoutsHelper.CurrentLayoutName = "Smurfette";
			AssertEquals("Smurfette", FilterStripBizO.LayoutsHelper.CurrentLayout.S9_FilterName);
		}

		#endregion

		#region TestDefaultLayoutName

		public void TestDefaultLayoutName()
		{
			var savedLayout = TestDataHelper.NewLayoutWithUserData("Default registry layout", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			AssertNull("Should be empty if default layout not set", FilterStripBizO.LayoutsHelper.CurrentLayout);

			FilterStripBizO.LayoutsHelper.DefaultLayout = savedLayout;
			AssertEquals("Should be default layout aka from registry", "Default registry layout", FilterStripBizO.LayoutsHelper.CurrentLayout.S9_FilterName);
		}

		#endregion

		#region TestCurrentUserTablePrefix

		public void TestCurrentUserTablePrefix()
		{
			AssertEquals(OrgContactSchema.Constants.Prefix, FilterStripBizO.LayoutsHelper.CurrentUserTablePrefix);

			DummyFilterStripBusinessObject filterStripBizO = new DummyFilterStripBusinessObject();
			GlbStaff staff = Factory.New<GlbStaff>();
			AssertEquals(GlbStaffSchema.Constants.Prefix, new FilterStripLayoutsHelperForWeb(filterStripBizO, staff).CurrentUserTablePrefix);
		}

		#endregion

		#region TestIsValidLayoutName

		public void TestIsValidLayoutName()
		{
			TestDataHelper.NewLayoutWithUserData("Shaggy", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			AssertEquals(true, FilterStripBizO.LayoutsHelper.IsValidLayoutName("Shaggy"));
			AssertEquals(false, FilterStripBizO.LayoutsHelper.IsValidLayoutName("Scooby"));
		}

		#endregion

		#region TestLayouts

		public void TestLayouts()
		{
			StmModuleFilter layout = TestDataHelper.NewLayoutWithUserData("layout", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmModuleFilter badLayout = TestDataHelper.NewLayoutWithUserData("bad layout", ZGuid.NewZGuid(), OrgContactSchema.Constants.Prefix);
			Factory.Save();

			AssertEquals(true, FilterStripBizO.LayoutsHelper.Layouts.ContainsOnly("", layout.S9_FilterName));
		}

		#endregion

		#region TestCurrentLayoutNameChangedEventIsNotFiredOnConstruction

		public void TestCurrentLayoutNameChangedEventIsNotFiredOnConstruction()
		{
			Globals.IsWeb = true;
			DummyFilterStripBusinessObject filterStripBizO = new DummyFilterStripBusinessObject();
			var layouthelper = new FilterStripLayoutsHelperForWeb(filterStripBizO, Contact);
			filterStripBizO.LayoutsHelper = layouthelper;
			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(filterStripBizO, "GSX-R", false, false, SaveColumnLayout.Ignore);
			filterStripBizO.LoadLayout(savedLayout);

			AssertEquals("CurrentLayoutNameChanged event should not fire on construction as the web will save the 'last used' filter.",
				false, layouthelper.CurrentLayoutNameChanged_ForTest);
		}

		#endregion

		#region TestCurrentLayoutNameChanged

		public void TestCurrentLayoutNameChanged()
		{
			var cbr250rr = TestDataHelper.NewLayoutWithUserData("CBR-250RR", Contact.PK, OrgContactSchema.Constants.Prefix);
			var cbr600rr = TestDataHelper.NewLayoutWithUserData("CBR-600RR", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			ZString previousName = ZString.Empty;
			ZString currentName = ZString.Empty;

			FilterStripBizO.LayoutsHelper.CurrentLayout = cbr250rr;

			FilterStripBizO.LayoutsHelper.CurrentLayoutChanged +=
				delegate(object sender, FilterStripLayoutsHelperForWeb.CurrentLayoutEventArgs args)
			{
				previousName = args.Previous.S9_FilterName;
				currentName = args.Current.S9_FilterName;
			};

			FilterStripBizO.LayoutsHelper.CurrentLayout = cbr600rr;

			AssertEquals(previousName, "CBR-250RR");
			AssertEquals(currentName, "CBR-600RR");
		}

		#endregion

		#region TestDeleteCurrentLayout

		public void TestDeleteCurrentLayout()
		{
			StmModuleFilter layout = TestDataHelper.NewLayoutWithUserData("Handy", Contact.PK, OrgContactSchema.Constants.Prefix);
			StmData lastUsedLayout = TestDataHelper.NewLastUsedLayout(layout, Contact.PK);
			Factory.Save();

			FilterStripBizO.LayoutsHelper.CurrentLayout = layout;

			AssertEquals("Precondition", false, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);
			AssertEquals("Precondition", "Handy", FilterStripBizO.LayoutsHelper.CurrentLayout.S9_FilterName);
			AssertEquals("Precondition", "Handy", FilterStripBizO.LastUsedLayout.S9_FilterName);

			FilterStripBizO.LayoutsHelper.DeleteCurrentLayout();
			AssertEquals(true, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);
			AssertNull(FilterStripBizO.LayoutsHelper.CurrentLayout);
			AssertNull(FilterStripBizO.LastUsedLayout);
		}

		#endregion

		#region TestResetIsCurrentLayoutDeleted

		public void TestResetIsCurrentLayoutDeleted()
		{
			FilterStripBizO.LayoutsHelper.DeleteCurrentLayout();
			AssertEquals("Precondition", true, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);

			FilterStripBizO.LayoutsHelper.ResetIsCurrentLayoutDeleted();
			AssertEquals(false, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);
		}

		#endregion

		#region TestIsCurrentLayoutDeleted

		public void TestIsCurrentLayoutDeleted()
		{
			AssertEquals("Should be false by default.", false, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);

			FilterStripBizO.LayoutsHelper.DeleteCurrentLayout();
			AssertEquals(true, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);

			FilterStripBizO.LayoutsHelper.ResetIsCurrentLayoutDeleted();
			AssertEquals(false, FilterStripBizO.LayoutsHelper.IsCurrentLayoutDeleted);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new FilterStripLayoutsHelperForWeb(FilterStripBizO, Contact);
		}

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			TestDataHelper.NewLayoutWithUserData("Fred", Contact.PK, OrgContactSchema.Constants.Prefix);
			Factory.Save();

			return base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
		}

		protected override Dictionary<string, IZType> CachedValueForSettingValueCallsRefreshBindingTestCore
		{
			get
			{
				Dictionary<string, IZType> result = base.CachedValueForSettingValueCallsRefreshBindingTestCore;
				result[FilterStripLayoutsHelperForWeb.Schema.CurrentLayoutName] = new ZString("Fred");
				return result;
			}
		}

		DummyFilterStripBusinessObjectForWeb FilterStripBizO
		{
			get { return fFilterStripBizO ?? (fFilterStripBizO = new DummyFilterStripBusinessObjectForWeb(Contact)); }
		}

		OrgContact Contact
		{
			get { return fContact ?? (fContact = Factory.NewWithValidTestData<OrgContact>()); }
		}

		LayoutsTestDataHelper TestDataHelper
		{
			get { return fTestDataHelper ?? (fTestDataHelper = new LayoutsTestDataHelper(Factory)); }
		}

		DummyFilterStripBusinessObjectForWeb fFilterStripBizO;
		OrgContact fContact;
		LayoutsTestDataHelper fTestDataHelper;

		#endregion
	}
}
