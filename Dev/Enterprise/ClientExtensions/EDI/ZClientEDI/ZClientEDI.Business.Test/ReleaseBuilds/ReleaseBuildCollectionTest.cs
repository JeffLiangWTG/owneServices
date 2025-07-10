using System.ComponentModel;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Client.EDI.ReleaseBuilds.Business.ReleaseBuildCollection;

namespace Enterprise.Client.EDI.ReleaseBuilds.Business.Test
{
	[TestedType(typeof(ReleaseBuildCollection))]
	class ReleaseBuildCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestNearestMatch()
		{
			AssertEquals("NearestMatch(\"x\")", "x", ListProvider.NearestMatch("x", true, -1).Item1);
		}

		public void TestDescriptionFromCode()
		{
			ReleaseBuild build = Collection.AddNew();
			build.HL_MajorVersion = 1;
			build.HL_MinorVersion = 2;
			build.HL_Release = 3;
			build.HL_Patch = 4;
			build.HL_ExeVersionDate = new ZDateTime(2004, 1, 31, 12, 13, 50);
			AssertEquals("DescriptionFromCode(\"1.2.3.4\")", build.ReleaseDisplayText, ListProvider.DescriptionFromCode("1.2.3.4"));
			AssertNull("DescriptionFromCode(\"\")", ListProvider.DescriptionFromCode(""));
		}

		public void TestPrimaryKeyFromCode()
		{
			ReleaseBuild build = Collection.AddNew();
			build.HL_MajorVersion = 1;
			build.HL_MinorVersion = 2;
			build.HL_Release = 3;
			build.HL_Patch = 4;
			AssertEquals("PrimaryKeyFromCodeForTest(\"1.2.3.4\")", build.PK, ListProvider.PrimaryKeyFromCode("1.2.3.4"));
		}

		public void TestGetLicenceHeaderCount()
		{
			ReleaseBuild build1 = Collection.AddNew();
			ReleaseBuild build2 = Collection.AddNew();
			ReleaseBuild build3 = Collection.AddNew();

			build1.FillWithValidTestData();
			build2.FillWithValidTestData();
			build3.FillWithValidTestData();

			EDIOrgHeader client1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			EDIOrgHeader client3 = Factory.NewWithValidTestData<EDIOrgHeader>();

			client1.CreateAndLoadLicenceForOrg();
			client2.CreateAndLoadLicenceForOrg();
			client3.CreateAndLoadLicenceForOrg();

			client1.LicenceEnterpriseCode = "CL1";
			client2.LicenceEnterpriseCode = "CL2";
			client3.LicenceEnterpriseCode = "CL3";

			LicenceDatabase database1 = client1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database2 = client1.LicCompany.LicDatabases.AddNew();
			LicenceDatabase database3 = client2.LicCompany.LicDatabases.AddNew();

			database1.LD_HL_CurrentRunningVersion = build1.PK;
			database2.LD_HL_CurrentRunningVersion = build1.PK;
			database3.LD_HL_CurrentRunningVersion = build2.PK;

			database1.LD_ServerCode = "DB1";
			database2.LD_ServerCode = "DB2";
			database3.LD_ServerCode = "DB3";

			Factory.Save();

			AssertEquals("GetLicenceHeaderCount(build1)", 2, Collection.GetLicenceHeaderCount(build1));
			AssertEquals("GetLicenceHeaderCount(build2)", 1, Collection.GetLicenceHeaderCount(build2));
			AssertEquals("GetLicenceHeaderCount(build3)", 0, Collection.GetLicenceHeaderCount(build3));
		}

		public void TestLicenceHeaderCountCacheIsClearedOnLoad()
		{
			ReleaseBuild build = Factory.NewWithValidTestData<ReleaseBuild>();

			EDIOrgHeader client1 = Factory.NewWithValidTestData<EDIOrgHeader>();
			client1.CreateAndLoadLicenceForOrg();
			client1.LicenceEnterpriseCode = "CL1";

			LicenceDatabase database1 = client1.LicCompany.LicDatabases.AddNew();
			database1.LD_HL_CurrentRunningVersion = build.PK;
			database1.LD_ServerCode = "DB1";

			Factory.Save();

			ZQuery query = new ZQuery(ReleaseBuildSchema.PK, build.PK);
			Collection.LoadWithMoreFiltering(query);
			AssertEquals("GetLicenceHeaderCount(Collection[0])", 1, Collection.GetLicenceHeaderCount(Collection[0]));

			EDIOrgHeader client2 = Factory.NewWithValidTestData<EDIOrgHeader>();
			client2.CreateAndLoadLicenceForOrg();
			client2.LicenceEnterpriseCode = "CL2";

			LicenceDatabase database2 = client2.LicCompany.LicDatabases.AddNew();
			database2.LD_HL_CurrentRunningVersion = build.PK;
			database2.LD_ServerCode = "DB2";

			Factory.Save();

			Collection.LoadWithMoreFiltering(query);
			AssertEquals("GetLicenceHeaderCount(Collection[0)", 2, Collection.GetLicenceHeaderCount(Collection[0]));

			LicenceDatabase database3 = client2.LicCompany.LicDatabases.AddNew();
			database3.LD_HL_CurrentRunningVersion = build.PK;
			database3.LD_ServerCode = "DB3";

			Factory.Save();

			Collection.Load();
			AssertEquals("GetLicenceHeaderCount(build)", 3, Collection.GetLicenceHeaderCount(build));
		}

		public void TestListProvider_AddCodeEqualsFilter()
		{
			var build1 = Collection.AddNew();
			var build2 = Collection.AddNew();
			var build3 = Collection.AddNew();

			build1.FillWithValidTestData();
			build2.FillWithValidTestData();
			build3.FillWithValidTestData();

			build1.ExeVersion = "18.1.24.334";
			build2.ExeVersion = "18.1.24.335";
			build3.ExeVersion = "18.1.24.336";

			Factory.Save();

			var listProvider = new ReleaseBuildFindBoxListProviderForTest(Collection);
			AssertNotNull(listProvider);

			AssertEquals(nameof(build1.ExeVersion), listProvider.GetCodePropertyNameForTest(build1.ExeVersion));
			var searchQuery = new ZQuery();
			listProvider.AddCodeEqualsFilterForTest(searchQuery, build1.ExeVersion);
			var filters = (searchQuery as IFilterPartsProvider).FilterParts;
			Assert("Missing MajorVersion", filters.Any(x => x.LiteralTextADO == $"{ReleaseBuildSchema.HL_MajorVersion.Name} = {build1.HL_MajorVersion}"));
			Assert("Missing MinorVersion", filters.Any(x => x.LiteralTextADO == $"{ReleaseBuildSchema.HL_MinorVersion.Name} = {build1.HL_MinorVersion}"));
			Assert("Missing Release", filters.Any(x => x.LiteralTextADO == $"{ReleaseBuildSchema.HL_Release.Name} = {build1.HL_Release}"));
			Assert("Missing Patch", filters.Any(x => x.LiteralTextADO == $"{ReleaseBuildSchema.HL_Patch.Name} = {build1.HL_Patch}"));

			var builds = listProvider.GetBusinessObjectsFromCode(build1.ExeVersion);
			AssertEquals(1, builds.Count());
			AssertEquals(build1.PK, builds.First().PK);

			var build = listProvider.GetBusinessObjectFromCode(build1.ExeVersion);
			AssertNotNull(build);
			AssertEquals(build1.PK, build.PK);

			builds = listProvider.GetBusinessObjectsFromCodeWithoutFilter(build2.ExeVersion);
			AssertEquals(1, builds.Count());
			AssertEquals(build2.PK, builds.First().PK);

			build = listProvider.GetBusinessObjectFromCodeWithoutFilter(build2.ExeVersion);
			AssertNotNull(build);
			AssertEquals(build2.PK, build.PK);
		}

		public void TestSortByVersion()
		{
			var build = Collection.AddNew();
			build.HL_MajorVersion = 1;
			build.HL_MinorVersion = 2;
			build.HL_Release = 3;
			build.HL_Patch = 10;
			build.HL_ExeVersionDate = new ZDateTime(2004, 1, 31, 12, 13, 50);

			build = Collection.AddNew();
			build.HL_MajorVersion = 1;
			build.HL_MinorVersion = 2;
			build.HL_Release = 3;
			build.HL_Patch = 33;
			build.HL_ExeVersionDate = new ZDateTime(2004, 1, 31, 12, 13, 50);

			build = Collection.AddNew();
			build.HL_MajorVersion = 1;
			build.HL_MinorVersion = 2;
			build.HL_Release = 3;
			build.HL_Patch = 2;
			build.HL_ExeVersionDate = new ZDateTime(2004, 1, 31, 12, 13, 50);

			Collection.Sort(ReleaseBuild.Schema.ExeVersion, ListSortDirection.Ascending);
			Assert((Collection.First() as ReleaseBuild)?.HL_Patch == 2);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ReleaseBuildCollection(Factory);
		}

		protected new ReleaseBuildCollection Collection
		{
			get { return (ReleaseBuildCollection)base.Collection; }
		}

		IFindBoxListProvider ListProvider
		{
			get { return Collection; }
		}

		#endregion
	}

	class ReleaseBuildFindBoxListProviderForTest : ReleaseBuildFindBoxListProvider
	{
		public ReleaseBuildFindBoxListProviderForTest(BusinessObjectCollection list) : base(list)
		{
		}

		public void AddCodeEqualsFilterForTest(ZQuery query, string code) => base.AddCodeEqualsFilter(query, code);

		public string GetCodePropertyNameForTest(string code) => base.GetCodePropertyName(code);
	}
}
