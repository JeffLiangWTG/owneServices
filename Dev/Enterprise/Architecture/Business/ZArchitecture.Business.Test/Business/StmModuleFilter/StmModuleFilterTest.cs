using System;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmModuleFilter))]
	sealed class StmModuleFilterTest : EnterpriseBusinessObjectTestCase
	{
		public void TestConcurrencyPolicy()
		{
			var stmModuleFilter = Factory.New<StmModuleFilter>();
			AssertEquals("Concurrency Policy should be Protect for S9_FilterData.", ConcurrencyPolicy.Protect, stmModuleFilter.S9_FilterDataInfo.ConcurrencyPolicy);
		}

		public void TestIndexNR_RX__S9_RelatedEntityID_S9_FilterName()
		{
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var guid = ZGuid.NewZGuid();
				var query = new ZQuery(StmModuleFilterSchema.S9_FilterName, "1");
				query.AddToFilter(StmModuleFilterSchema.S9_RelatedEntityID, guid);
				var filter = Factory.LoadTop1<StmModuleFilter>(query);

				var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains(guid.ToString()));

				var planalyzer_ExpectedIndexUsed = new QueryPlanalyzer(queryPlans.Item2.Last());

				AssertCollectionContains("NR_RX__S9_RelatedEntityID_S9_FilterName", planalyzer_ExpectedIndexUsed.IndexSeeks.Select(x => x.IndexName));
			}
		}

		public void TestIndexNR_RX__S9_ParentID_S9_GC_S9_FilterType()
		{
			using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
			{
				var query = new ZQuery(StmModuleFilterSchema.S9_ParentID, ZGuid.NewZGuid());
				query.AddToFilter(StmModuleFilterSchema.S9_FilterType, StmModuleFilterTypes.Codes.FilterRule);
				query.AddToFilter(StmModuleFilterSchema.S9_GC, ZGuid.NewZGuid());
				var filter = Factory.LoadTop1<StmModuleFilter>(query);

				var queryPlans = TestConnection.ExecutedCommandsAndQueryPlans?.FirstOrDefault(t => t.Item1.Contains("FRU"));

				var planalyzer_ExpectedIndexUsed = new QueryPlanalyzer(queryPlans.Item2.Last());

				AssertCollectionContains("NR_RX__S9_ParentID_S9_GC_S9_FilterType", planalyzer_ExpectedIndexUsed.IndexSeeks.Select(x => x.IndexName));
			}
		}

		[TestDate(2019, 05, 19)]
		public void TestCreator()
		{
			var filter = Factory.NewWithValidTestData<StmModuleFilter>();
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, filter.CreatorOrEarlyestUser.PK);
			Factory.Save();

			filter.S9_FilterName = "test";
			Factory.Save();

			var addLogPK = filter.Logs.AddedLog.PK;
			var result = Db.Connection.ExecuteNonQuery($"delete from dbo.StmALog where sl_pk = '{addLogPK}'");
			AssertEquals(1, result);

			var earlyestLog = filter.Logs.AllElements.Cast<StmALog>().OrderBy(log => log.SL_PostedTimeUtc).First();
			AssertNotNull(earlyestLog);
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, earlyestLog.User.PK);

			var staff = Factory.New<IGlbStaff>();
			staff.GS_Code = "AAA";
			staff.GS_LoginName = "alpha";
			staff.GS_FullName = "Alpha Albert Anaheim";
			Factory.Save();

			using (EnvProxy.Instance.SetTemporaryUserContext(staff.LoginName, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK))
			{
				var filterReloaded = new BusinessObjectFactory() { RefreshEnabled = false }.Load<StmModuleFilter>(filter.PK);

				AssertNotEquals(EnvProxy.Instance.CurrentUser.PK, filterReloaded.CreatorOrEarlyestUser.PK);
				AssertEquals(earlyestLog.User.PK, filterReloaded.CreatorOrEarlyestUser.PK);
			}
		}

		#region TestS9_RelatedEntityID

		public void TestS9_RelatedEntityID()
		{
			StmModuleFilter filter = Factory.New<StmModuleFilter>();
			ZGuid userpk = ZGuid.NewZGuid();
			filter.S9_ModuleID = "ship" + StmModuleFilter.ModuleIdSuffix.GridColorStrip;
			Factory.Save();
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, filter.S9_RelatedEntityID);
			filter.S9_RelatedEntityID = userpk;
			Factory.Save();
			AssertEquals(userpk, filter.S9_RelatedEntityID);
			AssertEquals(false, filter.S9_IsPublished);
		}

		#endregion
		#region TestGetOrCreateLayoutUserDataWithNullLayoutsHelperBlowsUp
		public void TestGetOrCreateLayoutUserDataWithNullLayoutsHelperBlowsUp()
		{
			AssertExceptionThrown<ArgumentNullException>("Cannot call GetOrCreateLayoutUserData() with a null layouts helper.", () => Layout.GetOrCreateLayoutUserData(null));
		}
		#endregion

		#region TestGetOrCreateLayoutUserData

		public void TestGetOrCreateLayoutUserData()
		{
			FilterStripLayoutsHelper layoutsHelper = new FilterStripLayoutsHelper();
			AssertNull("Precondition", Layout.GetLayoutUserData(layoutsHelper));

			StmModuleFilterUserData layoutUserData = Layout.GetOrCreateLayoutUserData(layoutsHelper);
			AssertEquals(Layout.PK, layoutUserData.Layout.PK);
			AssertEquals(layoutUserData.S0_S9, Layout.PK);
			AssertEquals(layoutUserData.S0_RelatedEntityID, layoutsHelper.CurrentUserPk);
			AssertEquals(layoutUserData.S0_RelatedEntityTableCode, layoutsHelper.CurrentUserTablePrefix);
			AssertEquals("Should not create a second instance of StmModuleFilterUserData.", layoutUserData, Layout.GetOrCreateLayoutUserData(layoutsHelper));
		}

		public void TestGetLayoutUserData()
		{
			IGlbStaff staff = (IGlbStaff)Factory.LoadTop1(ObjectFactory.GetType(typeof(IGlbStaff)), new ZQuery(GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, EnvProxy.Instance.CurrentUser.PK));
			FilterStripLayoutsHelperForTest layoutsHelper = new FilterStripLayoutsHelperForTest();
			layoutsHelper.CurrentUserPkOverride = staff.PK;
			AssertNull("Precondition", Layout.GetLayoutUserData(layoutsHelper));
			Layout.S9_IsPublished = false;

			StmModuleFilterUserData layoutUserData = Layout.GetOrCreateLayoutUserData(layoutsHelper);
			AssertEquals(Layout.PK, layoutUserData.Layout.PK);
			AssertEquals(layoutUserData.S0_S9, Layout.PK);
			AssertEquals(layoutUserData.S0_RelatedEntityID, staff.PK);
			AssertNotEquals(layoutUserData.S0_RelatedEntityID, EnvProxy.Instance.CurrentUser.PK);
			AssertEquals(layoutUserData.S0_RelatedEntityTableCode, layoutsHelper.CurrentUserTablePrefix);

			layoutsHelper.CurrentUserPkOverride = ZGuid.Invalid;
			AssertNull("filter is not published - should not load", Layout.GetLayoutUserData(layoutsHelper));

			Layout.S9_IsPublished = true;
			AssertNotNull("filter is published - should load", Layout.GetLayoutUserData(layoutsHelper));
		}

		class FilterStripLayoutsHelperForTest : FilterStripLayoutsHelper
		{
			public ZGuid CurrentUserPkOverride { get; set; }
			protected override ZGuid GetCurrentUserPk()
			{
				return CurrentUserPkOverride.IsValid ? CurrentUserPkOverride : base.GetCurrentUserPk();
			}
		}

		#endregion

		#region TestS9_FilterName

		public void TestS9_FilterName()
		{
			AssertEquals("Precondition", false, Layout.S9_IsPublished);

			Layout.S9_FilterName = "iPod";
			AssertEquals("iPod", Layout.S9_FilterName);
			AssertEquals("iPod", Layout.DisplayName);

			Layout.S9_IsPublished = true;
			AssertEquals("iPod", Layout.S9_FilterName);
			AssertEquals("iPod", Layout.DisplayName);
		}

		[ExpectNoExceptions]
		public void TestLongS9_FilterNameResave()
		{
			Layout.Factory.Save();
			string sql = "update dbo.StmModuleFilter set S9_FilterName = '123456789012345678901234567890123456789012345678' where S9_PK = '" + Layout.PK + "'";
			using (DbCommand cmd = CargoWise.Data.Db.Connection.Command(sql))
			{
				cmd.ExecuteNonQuery();
			}
			fLayout = new BusinessObjectFactory().Load<StmModuleFilter>(Layout.PK);
			Layout.S9_IsPublished = true;
			Layout.Factory.Save();
		}

		public void TestS9_FilterNameForColorGrid()
		{
			AssertEquals("Precondition", false, Layout.S9_IsPublished);

			Layout.S9_ModuleID += StmModuleFilter.ModuleIdSuffix.GridColorStrip;

			Layout.S9_FilterName = "iPod";
			AssertEquals("iPod", Layout.S9_FilterName);

			Layout.S9_FilterName = "[[[iPod]]";
			AssertEquals("[[[iPod]]", Layout.S9_FilterName);

			Layout.S9_IsPublished = true;
			AssertEquals("[[[iPod]]", Layout.S9_FilterName);

			Layout.S9_IsPublished = false;
			Layout.S9_ModuleID += StmModuleFilter.ModuleIdSuffix.GridColorScheme;

			Layout.S9_FilterName = "iPod";
			AssertEquals("iPod", Layout.S9_FilterName);

			Layout.S9_FilterName = "[[[iPod]]";
			AssertEquals("[[[iPod]]", Layout.S9_FilterName);

			Layout.S9_IsPublished = true;
			AssertEquals("[[[iPod]]", Layout.S9_FilterName);
		}

		#endregion

		#region TestS9_IsPublished

		public void TestS9_IsPublished()
		{
			Layout.S9_FilterName = "Darkness";
			AssertEquals("Darkness", Layout.S9_FilterName);

			Layout.S9_IsPublished = true;
			AssertEquals("Darkness", Layout.S9_FilterName);
			AssertEquals("Darkness", Layout.DisplayName);

			Layout.S9_FilterName = "";
			AssertEquals("", Layout.S9_FilterName);

			Layout.S9_FilterName = "Reflex";
			AssertEquals("Reflex", Layout.S9_FilterName);
			AssertEquals("Reflex", Layout.DisplayName);
		}

		public void TestS9_IsPublishedForColorStrip()
		{
			Layout.S9_ModuleID += StmModuleFilter.ModuleIdSuffix.GridColorStrip;

			Layout.S9_FilterName = "Darkness";
			AssertEquals("Darkness", Layout.S9_FilterName);

			Layout.S9_IsPublished = true;
			AssertEquals("Darkness", Layout.S9_FilterName);

			Layout.S9_FilterName = "";
			AssertEquals("", Layout.S9_FilterName);

			Layout.S9_FilterName = "Reflex";
			AssertEquals("Reflex", Layout.S9_FilterName);
		}

		#endregion

		#region TestS9_IsPublishedCannotBeChangedToUnpublished

		[ExpectExceptionMessage(typeof(NotSupportedException), "A published filter layout cannot be made unpublished.")]
		public void TestS9_IsPublishedCannotBeChangedToUnpublished()
		{
			Layout.S9_IsPublished = true;
			Layout.S9_IsPublished = false;
		}

		[ExpectNoExceptions()]
		public void TestS9_IsPublishedCannotBeChangedToUnpublishedForCSOrCT()
		{
			Layout.S9_ModuleID += StmModuleFilter.ModuleIdSuffix.GridColorScheme;
			Layout.S9_IsPublished = true;
			Layout.S9_IsPublished = false;

			Layout.S9_ModuleID += StmModuleFilter.ModuleIdSuffix.GridColorStrip;
			Layout.S9_IsPublished = true;
			Layout.S9_IsPublished = false;
		}

		#endregion

		#region TestS9_IsPublishedClearsS9_RelatedEntityIDWhenTrue

		public void TestS9_IsPublishedClearsS9_RelatedEntityIDWhenTrue()
		{
			Layout.S9_RelatedEntityID = EnvProxy.Instance.CurrentUser.PK;
			AssertEquals("Precondition", EnvProxy.Instance.CurrentUser.PK, Layout.S9_RelatedEntityID);

			Layout.S9_IsPublished = true;
			AssertEquals(true, Layout.S9_RelatedEntityID.IsEmpty);
		}

		#endregion

		#region TestGetQueryForCTIsPermissive

		class DummyLayoutsHelper : FilterStripLayoutsHelper
		{
			readonly ZGuid guid;

			public DummyLayoutsHelper(ZGuid guid)
			{
				this.guid = guid;
			}

			protected override ZGuid GetCurrentUserPk()
			{
				return guid;
			}
		}

		public void TestGetQueryForCTIsPermissive()
		{
			StmModuleFilter cT = Factory.New<StmModuleFilter>();
			StmModuleFilter cS = Factory.New<StmModuleFilter>();
			cT.S9_ModuleID = "BecauseIAmAColourStripIEndIn_CT";
			cS.S9_ModuleID = "BecauseIAmAColourSchemeIEndIn_CS";
			cT.S9_RelatedEntityID = cS.PK;
			cS.S9_GC = Factory.LoadTop1<IGlbCompany>(new ZQuery()).PK;
			cS.S9_IsPublished = true;
			cT.S9_GC = cS.S9_GC;
			cT.S9_IsPublished = false;
			var helper = new DummyLayoutsHelper(cS.PK);
			ZQuery query = new StmModuleFilter.Loader(Factory).GetQuery(cT.S9_ModuleID, helper);
			StmModuleFilter[] bizOs = Factory.Load<StmModuleFilter>(query);
			AssertEquals(1, bizOs.Length);
			cT.S9_ModuleID = "NotaCTAnymore";
			bizOs = Factory.Load<StmModuleFilter>(query);
			AssertEquals(0, bizOs.Length);
		}

		public void TestIndirectRelatedEntityReference()
		{
			StmModuleFilter cT = Factory.New<StmModuleFilter>();
			StmModuleFilter cSc = Factory.New<StmModuleFilter>();
			StmModuleFilter cSp = Factory.New<StmModuleFilter>();

			cT.S9_ModuleID = "BecauseIAmAColourStripIEndIn_CT";
			cSc.S9_ModuleID = "BecauseIAmAColourSchemeIEndIn_CS";
			cSp.S9_ModuleID = "BecauseIAmAColourSchemeIEndIn_CS";

			cSc.S9_GC = EnvProxy.Instance.CurrentCompany.PK;

			cT.S9_RelatedEntityID = cSc.PK;
			cSc.S9_RelatedEntityID = cSp.PK;

			cSp.S9_IsPublished = true;

			var helper = new DummyLayoutsHelper(cSp.PK);
			ZQuery query = new StmModuleFilter.Loader(Factory).GetQuery(cT.S9_ModuleID, helper);
			StmModuleFilter[] bizOs = Factory.Load<StmModuleFilter>(query);
			AssertEquals(1, bizOs.Length);
		}

		#endregion

		#region GetDisplayName

		public void TestGetDisplayName()
		{
			AssertEquals("USDR [+]", StmModuleFilter.GetDisplayName("USDR", true));
			AssertEquals("USDR", StmModuleFilter.GetDisplayName("USDR", false));
		}

		#endregion

		#region TestIsAutoLogged

		public void TestIsAutoLogged()
		{
			Assert("Should be auto-logged", Factory.New<StmModuleFilterHelper>().IsAutoLogged);
		}

		#endregion

		#region Helper

		class StmModuleFilterHelper : StmModuleFilter
		{
			public StmModuleFilterHelper(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public new bool IsAutoLogged
			{
				get { return base.IsAutoLogged; }
			}
		}

		#endregion

		#region OnSaving

		public void TestSavingDoesntPublishColorStrips()
		{
			StmModuleFilter color = Factory.New<StmModuleFilter>();
			color.S9_ModuleID = "shipment" + StmModuleFilter.ModuleIdSuffix.GridColorStrip;
			Factory.Save();
			AssertEquals("saving a color strip should publish it", false, color.S9_IsPublished);
			AssertEquals(EnvProxy.Instance.CurrentUser.PK, color.S9_RelatedEntityID);
		}

		#endregion

		#region Delete

		public void TestDeleteWithUserData()
		{
			var filter = Factory.NewWithValidTestData<StmModuleFilter>();
			var userData = filter.GetOrCreateLayoutUserData(new EmptyLayoutsHelper());
			Factory.Save();

			AssertNotNull(userData);
			AssertEquals(false, userData.IsDeleted);

			filter.Delete();
			Factory.Save();

			AssertEquals(false, userData.IsDeleted);

			filter = Factory.NewWithValidTestData<StmModuleFilter>();
			userData = filter.GetOrCreateLayoutUserData(new EmptyLayoutsHelper());
			Factory.Save();

			AssertNotNull(userData);
			AssertEquals(false, userData.IsDeleted);

			filter.DeleteWithUserData();
			Factory.Save();

			AssertEquals(true, userData.IsDeleted);
		}

		#endregion

		#region Clone and Compare

		public void TestClone_ShouldCopyFilterData()
		{
			var filter = Factory.New<StmModuleFilter>();
			filter.S9_ModuleID = "x";
			filter.S9_FilterName = "y";
			filter.S9_FilterData = new ZBlob(new byte[] { 1, 2, 3 });
			var userData = filter.GetOrCreateLayoutUserData(new EmptyLayoutsHelper());
			userData.S0_FilterDataValues = new ZBlob(new byte[] { 1, 2, 3, 4, 5 });

			var clone = (StmModuleFilter)filter.Clone();
			var cloneUserData = clone.GetOrCreateLayoutUserData(new EmptyLayoutsHelper());

			AssertEquals("Module ID", "x", clone.S9_ModuleID);
			AssertEquals("Filter name", "y", clone.S9_FilterName);
			AssertEquals("Filter data", new ZBlob(new byte[] { 1, 2, 3 }), clone.S9_FilterData);
			AssertEquals("Filter data values", new ZBlob(new byte[] { 1, 2, 3, 4, 5 }), cloneUserData.S0_FilterDataValues);
		}

		public void TestAreFiltersDifferent()
		{
			var layoutHelper = new EmptyLayoutsHelper();

			var filter1 = Factory.New<StmModuleFilter>();
			filter1.S9_ModuleID = "x";
			filter1.S9_FilterName = "y";
			filter1.S9_FilterData = new ZBlob(new byte[] { 1, 2, 3 });
			var userData1 = filter1.GetOrCreateLayoutUserData(layoutHelper);
			userData1.S0_FilterDataValues = new ZBlob(new byte[] { 1, 2, 3, 4, 5 });

			var filter2 = (StmModuleFilter)filter1.Clone();
			var userData2 = filter2.GetOrCreateLayoutUserData(layoutHelper);

			AssertEquals("Clone should be the same", false, StmModuleFilter.AreFiltersDifferent(filter1, filter2, layoutHelper));

			filter2.S9_FilterData = new ZBlob(new byte[] { 0 });
			AssertEquals("Filters should be considered different when their filter data are different", true, StmModuleFilter.AreFiltersDifferent(filter1, filter2, layoutHelper));
			filter2.S9_FilterData = new ZBlob(new byte[] { 1, 2, 3 });
			AssertEquals("Filters should be considered the same when their filter data are the same", false, StmModuleFilter.AreFiltersDifferent(filter1, filter2, layoutHelper));

			userData2.S0_FilterDataValues = new ZBlob(new byte[] { 0 });
			AssertEquals("Filters should be considered different when their filter data values are different", true, StmModuleFilter.AreFiltersDifferent(filter1, filter2, layoutHelper));
			userData2.S0_FilterDataValues = new ZBlob(new byte[] { 1, 2, 3, 4, 5 });
			AssertEquals("Filters should be considered the same when their filter data values are the same", false, StmModuleFilter.AreFiltersDifferent(filter1, filter2, layoutHelper));

			filter2.S9_ModuleID = "zzz";
			AssertEquals("Filters should not be considered different when their module IDs are different", false, StmModuleFilter.AreFiltersDifferent(filter1, filter2, layoutHelper));

			filter2.S9_FilterName = "zzz";
			AssertEquals("Filters should not be considered different when their names are different", false, StmModuleFilter.AreFiltersDifferent(filter1, filter2, layoutHelper));
		}

		#endregion

		#region User-defined filters

		public void TestFilterName_MustBeUniqueForEachModule_ForUserDefinedFilters()
		{
			var filter1 = Factory.New<StmModuleFilter>();
			var filter2 = Factory.New<StmModuleFilter>();

			filter1.S9_ModuleID = DummyModuleIDs.Dummy.Name;
			filter2.S9_ModuleID = DummyModuleIDs.Dummy.Name;

			filter1.S9_FilterType = StmModuleFilterTypes.Codes.UserDefined;
			filter2.S9_FilterType = StmModuleFilterTypes.Codes.UserDefined;

			filter1.S9_FilterName = "Same name";
			filter2.S9_FilterName = "Same name";

			filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			filter2.S9_GC = ZGuid.Empty;

			AssertExceptionThrown<ZSaveException>("There should be a database constraint to stop duplicate named user-defined filters within a module, even if one is published for all companies and the other isn't. SAD!", () => Factory.Save());
		}

		public void TestUserDefinedFilter_ForSameModuleButDifferentName_ShouldSaveWithoutException()
		{
			var filter1 = Factory.New<StmModuleFilter>();
			var filter2 = Factory.New<StmModuleFilter>();

			filter1.S9_ModuleID = DummyModuleIDs.Dummy.Name;
			filter2.S9_ModuleID = DummyModuleIDs.Dummy.Name;

			filter1.S9_FilterType = StmModuleFilterTypes.Codes.UserDefined;
			filter2.S9_FilterType = StmModuleFilterTypes.Codes.UserDefined;

			filter1.S9_FilterName = "Same name";
			filter2.S9_FilterName = "Different name";

			filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			filter2.S9_GC = ZGuid.Empty;

			AssertNoExceptionThrown("There should be no problem saving user defined filters for the same module as long as the names are different. SAD!", () => Factory.Save());
		}

		public void TestUserDefinedFilter_ForSameNameButDifferentModules_ShouldSaveWithoutException()
		{
			var filter1 = Factory.New<StmModuleFilter>();
			var filter2 = Factory.New<StmModuleFilter>();

			filter1.S9_ModuleID = DummyModuleIDs.Dummy.Name;
			filter2.S9_ModuleID = DummyModuleIDs.DummyDependent.Name;

			filter1.S9_FilterType = StmModuleFilterTypes.Codes.UserDefined;
			filter2.S9_FilterType = StmModuleFilterTypes.Codes.UserDefined;

			filter1.S9_FilterName = "Same name";
			filter2.S9_FilterName = "Same name";

			filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			filter2.S9_GC = ZGuid.Empty;

			AssertNoExceptionThrown("Duplicate names should be okay if the user defined filters are for different modules. SAD!", () => Factory.Save());
		}

		public void TestNonUserDefinedFilter_WithSameNameAndModule_ButDifferentCompany_ShouldSaveWithoutException()
		{
			var filter1 = Factory.New<StmModuleFilter>();
			var filter2 = Factory.New<StmModuleFilter>();

			filter1.S9_ModuleID = DummyModuleIDs.Dummy.Name;
			filter2.S9_ModuleID = DummyModuleIDs.Dummy.Name;

			filter1.S9_FilterType = StmModuleFilterTypes.Codes.Module;
			filter2.S9_FilterType = StmModuleFilterTypes.Codes.Module;

			filter1.S9_FilterName = "Same name";
			filter2.S9_FilterName = "Same name";

			filter1.S9_GC = EnvProxy.Instance.CurrentCompany.PK;
			filter2.S9_GC = ZGuid.Empty;

			AssertNoExceptionThrown("Duplicate name/module combinations should be fine because the filter type isn't set to USR. SAD!", () => Factory.Save());
		}

		#endregion

		public void TestDeleteByDataRefreshFiresEvents()
		{
			var layoutOnOtherFactory = new BusinessObjectFactory().NewWithValidTestData<StmModuleFilter>();
			layoutOnOtherFactory.Factory.Save();

			var layoutOnThisFactory = Factory.Load<StmModuleFilter>(layoutOnOtherFactory.PK);

			var wasFired = false;
			GridModuleFilterLayoutDeletedEvent.AddLayoutDeletedEventHandler(Factory, (e) => wasFired = true);

			layoutOnOtherFactory.Delete();
			layoutOnOtherFactory.Factory.Save();

			Assert("PRE: Was deleted by DataRefresh", layoutOnThisFactory.IsRowDeletedOrDetachedOrNull);
			Assert("The Deleted event should fire even when deleted by data refresh", wasFired);
		}

		public void TestIGridLayoutStorageImplementation()
		{
			var layout = Factory.New<StmModuleFilter>();

			IGridLayoutStorage impl = layout;
			AssertEquals("IsRenameAllowed", true, impl.IsRenameAllowed);

			impl.ColumnLayoutName = "LALA";
			AssertEquals("LALA", impl.ColumnLayoutName);

			AssertEquals("SaveColumnLayout", false, impl.SaveColumnLayout);
			layout.S9_SaveColumnLayout = true;
			AssertEquals("SaveColumnLayout", true, impl.SaveColumnLayout);

			AssertEquals("SaveGridColourLayout", false, impl.SaveGridColourLayout);
			layout.S9_SaveGridColourLayout = true;
			AssertEquals("SaveGridColourLayout", true, impl.SaveGridColourLayout);

			var guid = ZGuid.NewZGuid();
			AssertEquals("SaveGridColourLayout", ZGuid.Empty, impl.GridColourLayoutID);
			layout.S9_GridColourLayoutID = guid;
			AssertEquals("SaveGridColourLayout", guid, impl.GridColourLayoutID);

			AssertEquals("IsPublished", false, impl.IsPublished);
			layout.S9_IsPublished = true;
			AssertEquals("IsPublished", true, impl.IsPublished);

			AssertEquals("IsSystemDefined", false, impl.IsSystemDefined);
			layout.S9_IsSystem = true;
			AssertEquals("IsSystemDefined", true, impl.IsSystemDefined);

			AssertEquals("IsDeleteAllowed", true, impl.IsDeleteAllowed);

			layout.S9_ModuleID = "Grid1";
			AssertEquals("Grid1", impl.GridLayoutKey);

			layout.S9_FilterData = ZBlob.FromAscii("FilterDataSavedHere");
			AssertEquals("Grid1LALA", impl.GridLayoutKey);
		}

		public void TestFilterNameResourceStringKeysAreContextSpecific()
		{
			var layout1 = Factory.New<StmModuleFilter>();
			layout1.S9_ModuleID = "x";
			layout1.S9_IsPublished = true;
			layout1.S9_FilterName = "Test";

			var layout2 = Factory.New<StmModuleFilter>();
			layout2.S9_ModuleID = "y";
			layout2.S9_IsPublished = true;
			layout2.S9_FilterName = "Test";

			AssertNotEquals(((ResourceString)layout1.S9_FilterNameMultilingual).ResourceKey, ((ResourceString)layout2.S9_FilterNameMultilingual).ResourceKey);

			Factory.Save();

			var layout3 = Factory.New<StmModuleFilter>();
			layout3.S9_ModuleID = "x";
			layout3.S9_IsPublished = true;
			layout3.S9_FilterName = "Another Item";

			AssertContainsExactElementsInAnyOrder(
				new[] { ((ResourceString)layout1.S9_FilterNameMultilingual).ResourceKey, ((ResourceString)layout3.S9_FilterNameMultilingual).ResourceKey },
				layout3.S9_FilterNameInfo.CustomizableDataResourceStrings.Source.GetRuntimeCaptions(layout3.S9_FilterNameMultilingual as ResourceString, layout3).Select(rs => rs.ResourceKey));
		}

		public void TestFilterNameRuntimeCaptionsAreForPublishedFiltersOnly()
		{
			var layout1 = Factory.New<StmModuleFilter>();
			layout1.S9_ModuleID = "x";
			layout1.S9_IsPublished = true;
			layout1.S9_FilterName = "Published";

			var layout2 = Factory.New<StmModuleFilter>();
			layout2.S9_ModuleID = "x";
			layout2.S9_IsPublished = false;
			layout2.S9_FilterName = "Not Published";

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(
				new[] { ((ResourceString)layout1.S9_FilterNameMultilingual).ResourceKey },
				layout1.S9_FilterNameInfo.CustomizableDataResourceStrings.Source.GetRuntimeCaptions(layout1.S9_FilterNameMultilingual as ResourceString, layout1).Select(rs => rs.ResourceKey));
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public override void TestBizObjectFields()
		{
			base.TestBizObjectFields();
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObjectForSettingValueCallsRefreshBindingTest()
		{
			StmModuleFilter result = (StmModuleFilter)base.GetNewBusinessObjectForSettingValueCallsRefreshBindingTest();
			result.S9_IsPublished = true;

			return result;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return TestDataHelper.NewLayout("");
		}

		protected override BusinessObject GetNewBusinessObjectForTranslatableFieldTest(BusinessObjectFactory factory)
		{
			var bizo = factory.NewWithValidTestData<StmModuleFilter>();
			bizo.S9_IsPublished = true;
			return bizo;
		}

		StmModuleFilter Layout
		{
			get { return fLayout ?? (fLayout = TestDataHelper.NewLayout("layout name")); }
		}

		LayoutsTestDataHelper TestDataHelper
		{
			get { return fTestDataHelper ?? (fTestDataHelper = new LayoutsTestDataHelper(Factory)); }
		}

		StmModuleFilter fLayout;
		LayoutsTestDataHelper fTestDataHelper;

		#endregion
	}
}
