using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.ZArchitecture.Business.StmModuleFilter;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(Loader))]
	sealed class LoaderTest : LoaderTestCase
	{
		#region TestGetQuery

		public void TestGetQuery()
		{
			BusinessObject diffCompany = (BusinessObject)Factory.New<IGlbCompany>();

			StmModuleFilter systemWideStorage = CreateLayoutStorage("1", "[A]", true, ZGuid.Empty, ZGuid.Empty);
			StmModuleFilter systemWideStorage2 = CreateLayoutStorage("2", "[A]", true, ZGuid.Empty, ZGuid.Empty);
			StmModuleFilter currentCompanayStorage = CreateLayoutStorage("1", "[B]", false, EnvProxy.Instance.CurrentCompany.PK, ZGuid.Empty);
			StmModuleFilter differentCompanayStorage = CreateLayoutStorage("1", "[B]", false, diffCompany.PK, ZGuid.Empty);
			StmModuleFilter currentUserCurrentCompanyStorage = CreateLayoutStorage("1", "B", false, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentUser.PK);
			StmModuleFilter currentUserDifferentCompanyStorage = CreateLayoutStorage("1", "B", false, diffCompany.PK, EnvProxy.Instance.CurrentUser.PK);
			StmModuleFilter diffUserCurrentCompanyStorage = CreateLayoutStorage("1", "B", false, EnvProxy.Instance.CurrentCompany.PK, ZGuid.NewZGuid());
			StmModuleFilter diffUserDifferentCompanyStorage = CreateLayoutStorage("1", "B", false, diffCompany.PK, ZGuid.NewZGuid());

			Loader loader = new Loader(Factory);
			AssertEquals(true, systemWideStorage.MatchesFilter(loader.GetQuery("1", new FilterStripLayoutsHelper())));
			AssertEquals(false, systemWideStorage2.MatchesFilter(loader.GetQuery("1", new FilterStripLayoutsHelper())));

			AssertEquals(true, currentCompanayStorage.MatchesFilter(loader.GetQuery("1", new FilterStripLayoutsHelper())));
			AssertEquals(false, differentCompanayStorage.MatchesFilter(loader.GetQuery("1", new FilterStripLayoutsHelper())));

			AssertEquals(true, currentUserCurrentCompanyStorage.MatchesFilter(loader.GetQuery("1", new FilterStripLayoutsHelper())));
			AssertEquals(false, currentUserDifferentCompanyStorage.MatchesFilter(loader.GetQuery("1", new FilterStripLayoutsHelper())));

			AssertEquals(false, diffUserCurrentCompanyStorage.MatchesFilter(loader.GetQuery("1", new FilterStripLayoutsHelper())));
			AssertEquals(false, diffUserDifferentCompanyStorage.MatchesFilter(loader.GetQuery("1", new FilterStripLayoutsHelper())));
		}

		public void TestGetQueryForWeb()
		{
			Globals.IsWeb = true;
			var currentBranch = EnvProxy.Instance.CurrentBranch;
			var org1 = Factory.New<IOrgHeader>();
			org1.OH_Code = "~~LogonOrg";
			org1.OH_RL_NKClosestPort = currentBranch.NKUNLOCO;
			var org1User1 = Factory.New<IOrgContact>();
			org1User1.OC_ContactName = "~~U1~~";
			org1User1.OC_OH = org1.PK;
			org1User1.OC_WebAccessEnabled = true;
			org1User1.OC_Email = "email1@test1.test";

			var org1User2 = Factory.New<IOrgContact>();
			org1User2.OC_ContactName = "~~U2~~";
			org1User2.OC_OH = org1.PK;
			org1User2.OC_WebAccessEnabled = true;
			org1User2.OC_Email = "email2@test1.test";

			var org2 = Factory.New<IOrgHeader>();
			org2.OH_Code = "~~OtherOrg";
			org2.OH_RL_NKClosestPort = currentBranch.NKUNLOCO;
			var org2User1 = Factory.New<IOrgContact>();
			org2User1.OC_ContactName = "~~U3~~";
			org2User1.OC_OH = org2.PK;
			org2User1.OC_WebAccessEnabled = true;
			org2User1.OC_Email = "email1@test2.test";

			StmModuleFilter systemFilter1 = CreateLayoutStorageForWeb("X", "[A1]", true, false, true, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
			StmModuleFilter systemFilter2 = CreateLayoutStorageForWeb("Y", "[A2]", true, false, true, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);

			StmModuleFilter nonPublishedFilter1 = CreateLayoutStorageForWeb("X", "B1", false, false, false, currentBranch.CompanyPK, org1.PK, org1User1.PK);
			StmModuleFilter nonPublishedFilter2 = CreateLayoutStorageForWeb("Y", "B2", false, false, false, currentBranch.CompanyPK, org2.PK, org2User1.PK);

			StmModuleFilter publishedFilter1 = CreateLayoutStorageForWeb("X", "[C1]", false, false, true, currentBranch.CompanyPK, org1.PK, org1User1.PK);
			StmModuleFilter publishedFilter2 = CreateLayoutStorageForWeb("Y", "[C2]", false, false, true, currentBranch.CompanyPK, org2.PK, org2User1.PK);

			var diffCompany = Factory.LoadTop1<IGlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, currentBranch.CompanyPK));

			StmModuleFilter companyFilter1 = CreateLayoutStorageForWeb("X", "[D1]", false, true, true, currentBranch.CompanyPK, ZGuid.Empty, ZGuid.Empty);
			StmModuleFilter companyFilter2 = CreateLayoutStorageForWeb("Y", "[D2]", false, true, true, diffCompany.PK, ZGuid.Empty, ZGuid.Empty);

			Factory.Save();

			Loader loader = new Loader(Factory);
			var helperOrg1User1 = new TestFilterStripLayoutsHelperForWeb();
			helperOrg1User1.LogonUserPk = org1User1.PK;
			helperOrg1User1.LogonOrganisationPk = org1.PK;

			var helperOrg1User2 = new TestFilterStripLayoutsHelperForWeb();
			helperOrg1User2.LogonOrganisationPk = org1User2.PK;
			helperOrg1User2.LogonOrganisationPk = org1.PK;

			var helperOrg2User1 = new TestFilterStripLayoutsHelperForWeb();
			helperOrg2User1.LogonUserPk = org2User1.PK;
			helperOrg2User1.LogonOrganisationPk = org2.PK;

			//System filters can be accessed by everyone
			AssertEquals(true, systemFilter1.MatchesFilter(loader.GetQuery("X", helperOrg1User1)));
			AssertEquals(true, systemFilter1.MatchesFilter(loader.GetQuery("X", helperOrg1User2)));
			AssertEquals(true, systemFilter1.MatchesFilter(loader.GetQuery("X", helperOrg2User1)));

			AssertEquals(true, systemFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg1User1)));
			AssertEquals(true, systemFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg1User2)));
			AssertEquals(true, systemFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg2User1)));

			AssertEquals(false, systemFilter2.MatchesFilter(loader.GetQuery("X", helperOrg1User1)));
			AssertEquals(false, systemFilter2.MatchesFilter(loader.GetQuery("X", helperOrg1User2)));
			AssertEquals(false, systemFilter2.MatchesFilter(loader.GetQuery("X", helperOrg2User1)));

			AssertEquals(false, systemFilter1.MatchesFilter(loader.GetQuery("Y", helperOrg1User1)));
			AssertEquals(false, systemFilter1.MatchesFilter(loader.GetQuery("Y", helperOrg1User2)));
			AssertEquals(false, systemFilter1.MatchesFilter(loader.GetQuery("Y", helperOrg2User1)));

			//Non published filter can be accessed by the creator only
			AssertEquals(true, nonPublishedFilter1.MatchesFilter(loader.GetQuery("X", helperOrg1User1)));
			AssertEquals(false, nonPublishedFilter1.MatchesFilter(loader.GetQuery("X", helperOrg1User2)));
			AssertEquals(false, nonPublishedFilter1.MatchesFilter(loader.GetQuery("X", helperOrg2User1)));

			AssertEquals(false, nonPublishedFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg1User1)));
			AssertEquals(false, nonPublishedFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg1User2)));
			AssertEquals(true, nonPublishedFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg2User1)));

			AssertEquals(false, nonPublishedFilter2.MatchesFilter(loader.GetQuery("X", helperOrg1User1)));
			AssertEquals(false, nonPublishedFilter2.MatchesFilter(loader.GetQuery("X", helperOrg1User2)));
			AssertEquals(false, nonPublishedFilter2.MatchesFilter(loader.GetQuery("X", helperOrg2User1)));

			AssertEquals(false, nonPublishedFilter1.MatchesFilter(loader.GetQuery("Y", helperOrg1User1)));
			AssertEquals(false, nonPublishedFilter1.MatchesFilter(loader.GetQuery("Y", helperOrg1User2)));
			AssertEquals(false, nonPublishedFilter1.MatchesFilter(loader.GetQuery("Y", helperOrg2User1)));

			//published filter can be accessed by users from the same org only
			AssertEquals(true, publishedFilter1.MatchesFilter(loader.GetQuery("X", helperOrg1User1)));
			AssertEquals(true, publishedFilter1.MatchesFilter(loader.GetQuery("X", helperOrg1User2)));
			AssertEquals(false, publishedFilter1.MatchesFilter(loader.GetQuery("X", helperOrg2User1)));

			AssertEquals(false, publishedFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg1User1)));
			AssertEquals(false, publishedFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg1User2)));
			AssertEquals(true, publishedFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg2User1)));

			AssertEquals(false, publishedFilter2.MatchesFilter(loader.GetQuery("X", helperOrg1User1)));
			AssertEquals(false, publishedFilter2.MatchesFilter(loader.GetQuery("X", helperOrg1User2)));
			AssertEquals(false, publishedFilter2.MatchesFilter(loader.GetQuery("X", helperOrg2User1)));

			AssertEquals(false, publishedFilter1.MatchesFilter(loader.GetQuery("Y", helperOrg1User1)));
			AssertEquals(false, publishedFilter1.MatchesFilter(loader.GetQuery("Y", helperOrg1User2)));
			AssertEquals(false, publishedFilter1.MatchesFilter(loader.GetQuery("Y", helperOrg2User1)));

			//company-wide filter can be accessed by all users from the same company
			AssertEquals(true, companyFilter1.MatchesFilter(loader.GetQuery("X", helperOrg1User1)));
			AssertEquals(true, companyFilter1.MatchesFilter(loader.GetQuery("X", helperOrg1User2)));
			AssertEquals(true, companyFilter1.MatchesFilter(loader.GetQuery("X", helperOrg2User1)));

			AssertEquals(false, companyFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg1User1)));
			AssertEquals(false, companyFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg1User2)));
			AssertEquals(false, companyFilter2.MatchesFilter(loader.GetQuery("Y", helperOrg2User1)));
		}

		#endregion

		#region Query Performance

		public void TestGetQuery_ShouldIncludeFilterTypeFilter()
		{
			var loader = new Loader(Factory);
			var query = loader.GetQuery(ModuleIDs.ProcessTasks.Name);

			AssertContains("S9_FilterType <> 'FRU' must be included in the query in order for the correct index to be used. SAD!", "S9_FilterType <> 'FRU'", query.LiteralTextSqlFormatted);
		}

		#endregion

		#region TestFindByNameAndID

		public void TestFindByNameAndID()
		{
			IGlbCompany diffCompany = Factory.New<IGlbCompany>();

			StmModuleFilter systemWideStorage = CreateLayoutStorage("1", "A", true, ZGuid.Empty, ZGuid.Empty);
			StmModuleFilter systemWideStorage2 = CreateLayoutStorage("2", "A", true, ZGuid.Empty, ZGuid.Empty);

			StmModuleFilter currentCompanayStorage = CreateLayoutStorage("1", "B", false, EnvProxy.Instance.CurrentCompany.PK, ZGuid.Empty);

			StmModuleFilter differentCompanayStorage = CreateLayoutStorage("1", "B", false, diffCompany.PK, ZGuid.Empty);

			StmModuleFilter currentUserCurrentCompanyStorage = CreateLayoutStorage("1", "B", false, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentUser.PK);

			StmModuleFilter currentUserDifferentCompanyStorage = CreateLayoutStorage("1", "B", false, diffCompany.PK, EnvProxy.Instance.CurrentUser.PK);

			StmModuleFilter diffUserCurrentCompanyStorage = CreateLayoutStorage("1", "B", false, EnvProxy.Instance.CurrentCompany.PK, ZGuid.NewZGuid());

			StmModuleFilter diffUserDifferentCompanyStorage = CreateLayoutStorage("1", "B", false, diffCompany.PK, ZGuid.NewZGuid());

			//valid for db constraints
			AssertNoExceptionThrown(Factory.Save);

			StmModuleFilter[] result = new Loader(Factory).FindByIDAndName("1", "A", true);
			AssertEquals("Only one result", 1, result.Length);
			AssertEquals("Only one result", systemWideStorage, result[0]);
			AssertEquals(systemWideStorage, new Loader(Factory).FindTop1ByIDAndName("1", "A", true));

			result = new Loader(Factory).FindByIDAndName("1", "B", true);
			AssertEquals(1, result.Length);
			AssertEquals(currentCompanayStorage, result[0]);
			AssertEquals(currentCompanayStorage, new Loader(Factory).FindTop1ByIDAndName("1", "B", true));
		}

		#endregion

		public void TestFindByID()
		{
			IGlbCompany diffCompany = Factory.New<IGlbCompany>();

			StmModuleFilter systemWideStorage = CreateLayoutStorage("1", "A", true, ZGuid.Empty, ZGuid.Empty);
			StmModuleFilter systemWideStorage2 = CreateLayoutStorage("2", "A", true, ZGuid.Empty, ZGuid.Empty);

			StmModuleFilter currentCompanayStorage = CreateLayoutStorage("1", "B", false, EnvProxy.Instance.CurrentCompany.PK, ZGuid.Empty);

			StmModuleFilter differentCompanayStorage = CreateLayoutStorage("1", "B", false, diffCompany.PK, ZGuid.Empty);

			StmModuleFilter currentUserCurrentCompanyStorage = CreateLayoutStorage("1", "B", false, EnvProxy.Instance.CurrentCompany.PK, EnvProxy.Instance.CurrentUser.PK);

			StmModuleFilter currentUserDifferentCompanyStorage = CreateLayoutStorage("1", "B", false, diffCompany.PK, EnvProxy.Instance.CurrentUser.PK);

			StmModuleFilter diffUserCurrentCompanyStorage = CreateLayoutStorage("1", "B", false, EnvProxy.Instance.CurrentCompany.PK, ZGuid.NewZGuid());

			StmModuleFilter diffUserDifferentCompanyStorage = CreateLayoutStorage("1", "B", false, diffCompany.PK, ZGuid.NewZGuid());

			List<StmModuleFilter> list = new List<StmModuleFilter>(new Loader(Factory).FindByID("1"));
			AssertEquals("3 layouts expected", 3, list.Count);
			Assert("SystemWide storage for id 1", list.Contains(systemWideStorage));
			Assert("currentCompanayStorage storage for id 1", list.Contains(currentCompanayStorage));
			Assert("currentUserCurrentCompanyStorage storage for id 1", list.Contains(currentUserCurrentCompanyStorage));

			StmModuleFilter top1 = new Loader(Factory).FindTop1ByID("1");
			Assert(top1 == systemWideStorage || top1 == currentCompanayStorage || top1 == currentUserCurrentCompanyStorage);
		}

		#region Implementation

		StmModuleFilter CreateLayoutStorage(ZString moduleID, ZString filterName, ZBool isSystem, ZGuid companyPK, ZGuid relatedEntityID)
		{
			StmModuleFilter result = Factory.New<StmModuleFilter>();

			result.S9_GC = companyPK;
			result.S9_FilterName = filterName;
			result.S9_ModuleID = moduleID;
			result.S9_IsSystem = isSystem;
			result.S9_RelatedEntityID = relatedEntityID;
			result.S9_IsPublished = relatedEntityID.IsEmpty;

			return result;
		}

		StmModuleFilter CreateLayoutStorageForWeb(ZString moduleID, ZString filterName, ZBool isSystem, ZBool isCompany, ZBool isPublished, ZGuid companyPK, ZGuid orgPK, ZGuid userPK)
		{
			var result = Factory.New<StmModuleFilter>();
			result.S9_GC = companyPK;
			result.S9_FilterName = filterName;
			result.S9_ModuleID = moduleID;
			result.S9_IsSystem = isSystem;
			result.S9_IsPublished = isPublished || isCompany;
			result.S9_RelatedEntityID = isPublished ? (isCompany ? ZGuid.Empty : orgPK) : userPK;

			CreateLayoutStorageUserData(result, userPK);
			return result;
		}

		StmModuleFilterUserData CreateLayoutStorageUserData(StmModuleFilter stmModuleFilter, ZGuid relatedEntityID)
		{
			StmModuleFilterUserData result = Factory.New<StmModuleFilterUserData>();

			result.S0_S9 = stmModuleFilter.PK;
			result.S0_RelatedEntityTableCode = "OC";
			result.S0_RelatedEntityID = relatedEntityID;

			return result;
		}

		protected override BusinessObject.Loader GetNewLoaderToTest()
		{
			return new Loader(Factory);
		}

		class TestFilterStripLayoutsHelperForWeb : FilterStripLayoutsHelper
		{
			public ZGuid LogonUserPk { get; set; }
			protected override ZGuid GetCurrentUserPk()
			{
				return LogonUserPk.IsValid ? LogonUserPk : base.GetCurrentUserPk();
			}

			public ZGuid LogonOrganisationPk { get; set; }
			protected override ZGuid GetCurrentOrganisationPk()
			{
				return LogonOrganisationPk.IsValid ? LogonOrganisationPk : base.GetCurrentOrganisationPk();
			}
		}

		#endregion
	}
}
