using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Favorites;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class LayoutSaveManagerTest : TestCaseWithDummy
	{
		public void TestSaveLayout()
		{
			var textFilter = new ModuleTextFilter("text filter", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(textFilter);
			var strip = FilterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = textFilter.Description;

			textFilter.IsActive = true; // simulate user selecting this filter

			var savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(FilterStripBizO, "Musashi", false, false, SaveColumnLayout.Yes);

			AssertEquals(true, savedLayout.IsInDatabase);
			AssertEquals(LayoutsTestDataHelper.TestModuleID, savedLayout.S9_ModuleID);
			AssertEquals(FilterStripBizO.LayoutsHelper.CurrentUserPk, savedLayout.S9_RelatedEntityID);
			AssertEquals(false, savedLayout.S9_IsPublished);
			AssertEquals(true, savedLayout.S9_SaveColumnLayout);
			AssertEquals("Musashi", savedLayout.S9_FilterName);

			var savedLayoutUserData = savedLayout.GetLayoutUserData(FilterStripBizO.LayoutsHelper);
			AssertEquals(true, savedLayoutUserData.IsInDatabase);
			AssertEquals(false, savedLayoutUserData.S0_FilterDataValues.IsEmpty);
			AssertEquals(FilterStripBizO.LayoutsHelper.CurrentUserPk, savedLayoutUserData.S0_RelatedEntityID);
			AssertEquals(FilterStripBizO.LayoutsHelper.CurrentUserTablePrefix, savedLayoutUserData.S0_RelatedEntityTableCode);
			AssertEquals(savedLayout.PK, savedLayoutUserData.S0_S9);

			savedLayout = new DataGridLayoutManager().SavePreconfiguredLayout(FilterStripBizO, "Musashi", false, false, SaveColumnLayout.Ignore);
			AssertEquals("S9_SaveColumnLayout should stay as what it was", true, savedLayout.S9_SaveColumnLayout);
		}

		public void TestSaveLayoutChanges()
		{
			var textFilter = new ModuleTextFilter("text filter", DummyBizoSchema.Z0_Description);
			FilterStripBizO.AddModuleFilterForTest(textFilter);
			var strip = FilterStripBizO.FilterStrips.AddNew();
			strip.FilterDescription = textFilter.Description;
			textFilter.IsActive = true;

			var filter1 = FilterStripBizO.Layouts.AddNew();
			filter1.S9_FilterName = "A";
			filter1.S9_SaveColumnLayout = false;
			filter1.S9_ModuleID = LayoutsTestDataHelper.TestModuleID;
			filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;

			var filter2 = FilterStripBizO.Layouts.AddNew();
			filter2.S9_FilterName = "B";
			filter2.S9_SaveColumnLayout = true;
			filter2.S9_ModuleID = LayoutsTestDataHelper.TestModuleID;
			filter2.S9_GC = EnvProxy.Instance.CurrentCompany.PK;

			var filter3 = FilterStripBizO.Layouts.AddNew();
			filter3.S9_FilterName = "C";
			filter3.S9_SaveColumnLayout = false;
			filter3.S9_ModuleID = LayoutsTestDataHelper.TestModuleID;
			filter3.S9_GC = EnvProxy.Instance.CurrentCompany.PK;

			var filter4 = FilterStripBizO.Layouts.AddNew();
			filter4.S9_FilterName = "D";
			filter4.S9_SaveColumnLayout = true;
			filter4.S9_ModuleID = LayoutsTestDataHelper.TestModuleID;
			filter4.S9_GC = EnvProxy.Instance.CurrentCompany.PK;

			Factory.Save();

			var layoutsWithNameChanges = new Dictionary<ZGuid, string>();
			layoutsWithNameChanges.Add(filter1.PK, "E");
			layoutsWithNameChanges.Add(filter4.PK, "F");

			var layoutsDeleted = new List<ZGuid>();
			layoutsDeleted.Add(filter2.PK);
			layoutsDeleted.Add(filter4.PK);

			var layoutsWithSaveColumnChanges = new List<(ZGuid, ZBool, ZBool)>
			{
				(filter3.PK, true, true),
				(filter4.PK, false, false)
			};

			var gridColour = ZGuid.NewZGuid();
			new LayoutSaveManager().SaveLayoutChanges(FilterStripBizO, layoutsWithNameChanges, layoutsDeleted, layoutsWithSaveColumnChanges, gridColour);

			AssertEquals("filter 2 should be deleted", true, filter2.IsDeleted);
			AssertEquals("filter 4 should be deleted", true, filter4.IsDeleted);

			filter1.Reload();
			filter3.Reload();

			AssertEquals("filter1's name changed to E", "E", filter1.S9_FilterName);
			AssertEquals("filter1's SaveColumnLayout stays the same", false, filter1.S9_SaveColumnLayout);
			AssertEquals("filter1's SaveGridColourLayout stays the same", false, filter1.S9_SaveGridColourLayout);
			AssertEquals("filter1's GridColourLayoutID stays the same", ZGuid.Empty, filter1.S9_GridColourLayoutID);

			AssertEquals("filter3's name stays the same", "C", filter3.S9_FilterName);
			AssertEquals("filter3's SaveColumnLayout changed", true, filter3.S9_SaveColumnLayout);
			AssertEquals("filter3's SaveGridColourLayout changed", true, filter3.S9_SaveGridColourLayout);
			AssertEquals("filter3's GridColourLayoutID changed", gridColour, filter3.S9_GridColourLayoutID);
		}

		public void TestSaveLayoutChanges_ForDeleteFavoriteLayout()
		{
			var filter1 = FilterStripBizO.Layouts.AddNew();
			filter1.S9_FilterName = "A";
			filter1.S9_SaveColumnLayout = false;
			filter1.S9_ModuleID = LayoutsTestDataHelper.TestModuleID;
			filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			Factory.Save();

			var layoutsDeleted = new List<ZGuid>();
			layoutsDeleted.Add(filter1.PK);

			var layoutsWithNameChanges = new Dictionary<ZGuid, string>();
			var layoutsWithSaveColumnChanges = new List<(ZGuid, ZBool, ZBool)>();
			var gridColour = ZGuid.NewZGuid();

			Assert("Favorites should not contain filter", !FilterStripBizO.FavoriteLayouts.Contains(filter1));
			FilterStripBizO.AddOrRemoveFavoriteFilter(filter1);
			Assert("Favorites should contain filter", FilterStripBizO.FavoriteLayouts.Contains(filter1));

			//add a second favourite for second user/company
			var stmLink2 = Factory.New<StmLink>();
			var company2 = Factory.New<MasterFiles.Integration.IGlbCompany>();
			((BusinessObject)company2).FillWithValidTestData();
			stmLink2.STL_LinkType = "FLF";
			stmLink2.STL_GS_NKUser = "~BP";
			stmLink2.STL_GC_LogonCompany = company2.PK;
			stmLink2.STL_ModuleID = filter1.S9_ModuleID;
			stmLink2.STL_ItemPK = filter1.PK;
			stmLink2.STL_LastUsedDateTimeUtc = ZDateTime.Now;

			//and an unrelated stmLink2

			var stmLink3 = Factory.New<StmLink>();
			stmLink3.STL_LinkType = "FLF";
			stmLink3.STL_GS_NKUser = "~BP";
			stmLink3.STL_GC_LogonCompany = company2.PK;
			stmLink3.STL_ModuleID = filter1.S9_ModuleID;
			stmLink3.STL_ItemPK = ZGuid.NewZGuid();
			stmLink3.STL_LastUsedDateTimeUtc = ZDateTime.Now;

			Factory.Save();

			new LayoutSaveManager().SaveLayoutChanges(FilterStripBizO, layoutsWithNameChanges, layoutsDeleted, layoutsWithSaveColumnChanges, gridColour);
			AssertEquals("filter 1 should be deleted", true, filter1.IsDeleted);
			AssertEquals(0, Factory.Load<StmLink>(new ZQuery(StmLinkSchema.STL_ItemPK, filter1.PK)).Length);
			AssertEquals(1, Factory.Load<StmLink>(new ZQuery(StmLinkSchema.STL_ItemPK, stmLink3.STL_ItemPK)).Length);
		}

		DummyFilterStripBusinessObject FilterStripBizO
		{
			get { return fFilterStripBizO ?? (fFilterStripBizO = new DummyFilterStripBusinessObject()); }
		}

		DummyFilterStripBusinessObject fFilterStripBizO;
	}
}
