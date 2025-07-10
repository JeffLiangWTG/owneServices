using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Data;
using CargoWise.Main.Navigation;
using CargoWise.Types;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Startup.Testing.MainFormTestCase;

namespace CargoWise.Main.Test.Data;

public class SnapshotsRepositoryTest : TestCaseWithFactory
{
	[RequiresSTA]
	public void TestFindModules()
	{
		TestCaseHelper.ClearTable(StmLinkSchema.Constants.TableName);

		using var form = new TestMainForm();
		form.Show();
		System.Windows.Forms.Application.DoEvents();
		var nav = form.NavigationBar;

		nav.LoadModuleTree(ModuleTree.Tree);
		var modules = new SnapshotsRepository().FindModules().ToList();
		Assert("Should return modules", !modules.IsNullOrEmpty());
		Assert("Should not have report modules", !modules.Any(m => m.ModuleName.Contains("Reports")));
		Assert("Should not have ResourceStrings module", !modules.Any(m => m.ModuleName.Equals("Resource Strings", StringComparison.OrdinalIgnoreCase)));

		var countModulesWithDuplicatedNames = modules.GroupBy(m => m.ModuleName)
			.Where(g => g.Count() > 1)
			.SelectMany(g => g)
			.Count();

		AssertEquals("Should not have duplicated module names", 0, countModulesWithDuplicatedNames);
	}

	public void TestFindByUserId_WhenUserHasSnapshots()
	{
		PrepareFindByUserIdData();

		var mySnapShots = new SnapshotsRepository().FindByUserId(GlbStaff.CurrentUser.PK.ToGuid()).ToList();
		AssertEquals("Should find 2 snapshot", 2, mySnapShots.Count);
		AssertEquals("Order should be 1", 1, mySnapShots[0].Order);
		AssertEquals("Order should be 2", 2, mySnapShots[1].Order);
	}

	void PrepareFindByUserIdData()
	{
		Factory.Load<StmModuleFilter>(new ZQuery()).DeleteAll();
		Factory.Load<StmData>(new ZQuery()).DeleteAll();

		var f1 = Factory.New<StmModuleFilter>();
		f1.S9_FilterName = "rule1";
		f1.S9_ModuleID = "WorkItem";
		Factory.Save();

		var f2 = Factory.New<StmModuleFilter>();
		f2.S9_FilterName = "rule2";
		f2.S9_ModuleID = "WorkItem";
		Factory.Save();

		var s1 = Factory.New<StmData>();
		s1.SD_Name = "Snapshot_1";
		s1.SD_Owner = GlbStaff.CurrentUser.PK.ToGuid();
		s1.SD_GuidValue = f1.PK.ToGuid();
		Factory.Save();

		var s2 = Factory.New<StmData>();
		s2.SD_Name = "Snapshot_2";
		s2.SD_Owner = GlbStaff.CurrentUser.PK.ToGuid();
		s2.SD_GuidValue = f2.PK.ToGuid();
		Factory.Save();

		var s3 = Factory.New<StmData>();
		s3.SD_Name = "Snapshot_1";
		s3.SD_Owner = Guid.NewGuid();
		s3.SD_GuidValue = f2.PK.ToGuid();
		Factory.Save();
	}

	public void TestFindByUserIdAsync_WhenUserHasSnapshots()
	{
		PrepareFindByUserIdData();

		// act
		var mySnapShots = new SnapshotsRepository().FindByUserIdAsync(GlbStaff.CurrentUser.PK.ToGuid()).GetAwaiter().GetResult().ToList();

		// assert
		AssertEquals("Should find 2 snapshot", 2, mySnapShots.Count);
		AssertEquals("Order should be 1", 1, mySnapShots[0].Order);
		AssertEquals("Order should be 2", 2, mySnapShots[1].Order);
	}

	public void TestFindByUserId_WhenUserDoesNotHaveSnapshots()
	{
		// arrange
		PrepareFindByUserIdData();

		// act
		var mySnapShots = new SnapshotsRepository().FindByUserIdAsync(Guid.NewGuid()).GetAwaiter().GetResult().ToList();

		// assert
		AssertEquals("Should find 0 snapshots", 0, mySnapShots.Count);
	}

	public void TestFindModuleFiltersByModuleId()
	{
		Factory.Load<StmModuleFilter>(new ZQuery()).DeleteAll();

		var user1 = Factory.NewWithValidTestData<GlbStaff>();
		var user2 = Factory.NewWithValidTestData<GlbStaff>();
		var moduleFilter1 = CreateStmModuleFilter("WorkItem", "AAA", user1.PK, isIndexSearch: true, isPublish: true);
		var moduleFilter2 = CreateStmModuleFilter("WorkItem", "BBB", user1.PK, true, false);
		var moduleFilter3 = CreateStmModuleFilter("WorkItem", "CCC", user1.PK, false, true);
		var moduleFilter4 = CreateStmModuleFilter("WorkItem", "DDD", user1.PK, false, false);
		var moduleFilter5 = CreateStmModuleFilter("JobShipment", "EEE", user2.PK, true, true);
		var moduleFilter6 = CreateStmModuleFilter("JobShipment", "FFF", user2.PK, true, false);
		var moduleFilter7 = CreateStmModuleFilter("JobShipment", "GGG", user2.PK, false, true);
		var moduleFilter8 = CreateStmModuleFilter("JobShipment", "HHH", user2.PK, false, false);

		Factory.Save();

		var snapshotsRepository = new SnapshotsRepository();
		using (Env.SetTemporaryUserContext(new UserContext(user1, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var snapshotModuleFilters = snapshotsRepository
				.FindModuleFiltersByModuleId(ModuleIDs.WorkItem)
				.OrderBy(s => s.ModuleFilterName)
				.ToList();

			AssertEquals(2, snapshotModuleFilters.Count);
			AssertEquals(1, snapshotModuleFilters.Count(s => s.ModuleFilterId == moduleFilter1.PK));
			AssertEquals(1, snapshotModuleFilters.Count(s => s.ModuleFilterId == moduleFilter2.PK));

			AssertEquals(ModuleIDs.WorkItem, snapshotModuleFilters[0].ModuleId);
			AssertEquals("Work Items", snapshotModuleFilters[0].ModuleName);
			AssertEquals("AAA", snapshotModuleFilters[0].ModuleFilterName);

			AssertEquals(ModuleIDs.WorkItem, snapshotModuleFilters[1].ModuleId);
			AssertEquals("Work Items", snapshotModuleFilters[1].ModuleName);
			AssertEquals("BBB", snapshotModuleFilters[1].ModuleFilterName);
		}

		using (Env.SetTemporaryUserContext(new UserContext(user2, Env.CurrentBranchPK, Env.CurrentDepartmentPK)))
		{
			var snapshotModuleFilters = snapshotsRepository
				.FindModuleFiltersByModuleId(ModuleIDs.JobShipment)
				.OrderBy(s => s.ModuleFilterName)
				.ToList();

			AssertEquals(2, snapshotModuleFilters.Count);
			AssertEquals(1, snapshotModuleFilters.Count(s => s.ModuleFilterId == moduleFilter5.PK));
			AssertEquals(1, snapshotModuleFilters.Count(s => s.ModuleFilterId == moduleFilter6.PK));

			AssertEquals(ModuleIDs.JobShipment, snapshotModuleFilters[0].ModuleId);
			AssertEquals("Shipments (Forwarding)", snapshotModuleFilters[0].ModuleName);
			AssertEquals("EEE", snapshotModuleFilters[0].ModuleFilterName);

			AssertEquals(ModuleIDs.JobShipment, snapshotModuleFilters[1].ModuleId);
			AssertEquals("Shipments (Forwarding)", snapshotModuleFilters[1].ModuleName);
			AssertEquals("FFF", snapshotModuleFilters[1].ModuleFilterName);
		}
	}

	StmModuleFilter CreateStmModuleFilter(string moduleID, string filterName, ZGuid ownerPK, bool isIndexSearch, bool isPublish)
	{
		var moduleFilter = Factory.New<StmModuleFilter>();
		moduleFilter.S9_ModuleID = moduleID;
		moduleFilter.S9_FilterName = filterName;
		moduleFilter.S9_RelatedEntityID = ownerPK;
		moduleFilter.S9_IsIndexSearch = isIndexSearch;
		moduleFilter.S9_IsPublished = isPublish;
		return moduleFilter;
	}

	public void TestCreateArgumentException()
	{
		var r = new SnapshotsRepository();
		var snapShot = new Snapshot();
		AssertArgumentExceptionThrown("Owner", () => r.Create(snapShot));

		snapShot.Owner = Guid.NewGuid();
		AssertArgumentExceptionThrown("ModuleFilterId", () => r.Create(snapShot));

		snapShot.ModuleFilter = new SnapshotModuleFilter();
		AssertArgumentExceptionThrown("ModuleFilterId", () => r.Create(snapShot));

		snapShot.ModuleFilter = new SnapshotModuleFilter()
		{
			ModuleFilterId = Guid.NewGuid()
		};
		AssertArgumentExceptionThrown("ModuleFilterId", () => r.Create(snapShot));
	}

	public void TestCreate()
	{
		Factory.Load<StmModuleFilter>(new ZQuery()).DeleteAll();
		Factory.Load<StmData>(new ZQuery()).DeleteAll();

		var filter = Factory.New<StmModuleFilter>();
		filter.S9_FilterName = "rule1";
		filter.S9_ModuleID = "WorkItem";
		Factory.Save();

		var userId = GlbStaff.CurrentUser.PK.ToGuid();
		var repository = new SnapshotsRepository();

		var snapshotId = repository.Create(new Snapshot()
		{
			Owner = userId,
			ModuleFilter = new SnapshotModuleFilter()
			{
				ModuleFilterId = filter.PK.ToGuid(),
			},
			Order = 1,
		});

		var snapshtos = repository.FindByUserId(userId).ToList();
		AssertEquals(1, snapshtos.Count);

		var snapshot = snapshtos[0];
		Assert(snapshot.ModuleFilter.ModuleFilterId == filter.PK);
		Assert(snapshot.Order == 1);
	}

	public void TestUpdateArgumentException()
	{
		Factory.Load<StmModuleFilter>(new ZQuery()).DeleteAll();
		Factory.Load<StmData>(new ZQuery()).DeleteAll();

		var repository = new SnapshotsRepository();
		var s = new Snapshot();
		AssertEquals(0, repository.Update(s));

		s.Id = Guid.NewGuid();
		AssertEquals(0, repository.Update(s));

		var filter = Factory.New<StmModuleFilter>();
		filter.S9_FilterName = "rule1";
		filter.S9_ModuleID = "WorkItem";
		Factory.Save();

		var snapshot = new Snapshot()
		{
			Owner = GlbStaff.CurrentUser.PK.ToGuid(),
			ModuleFilter = new SnapshotModuleFilter()
			{
				ModuleFilterId = filter.PK.ToGuid(),
			}
		};

		snapshot.Id = repository.Create(snapshot);
		Assert(snapshot.ModuleFilter.ModuleFilterId == filter.PK);

		snapshot.ModuleFilter = new SnapshotModuleFilter();
		AssertArgumentExceptionThrown("ModuleFilterId", () => repository.Update(snapshot));

		snapshot.ModuleFilter.ModuleFilterId = Guid.NewGuid();
		AssertArgumentExceptionThrown("ModuleFilterId", () => repository.Update(snapshot));
	}

	public void TestUpdate()
	{
		Factory.Load<StmModuleFilter>(new ZQuery()).DeleteAll();
		Factory.Load<StmData>(new ZQuery()).DeleteAll();

		var filterOne = Factory.New<StmModuleFilter>();
		filterOne.S9_FilterName = "rule1";
		filterOne.S9_ModuleID = "WorkItem";
		Factory.Save();

		var filterTwo = Factory.New<StmModuleFilter>();
		filterTwo.S9_FilterName = "rule2";
		filterTwo.S9_ModuleID = "WorkItem";
		Factory.Save();

		var userId = GlbStaff.CurrentUser.PK.ToGuid();

		var repository = new SnapshotsRepository();
		var snapshotId = repository.Create(new Snapshot()
		{
			Owner = userId,
			ModuleFilter = new SnapshotModuleFilter()
			{
				ModuleFilterId = filterOne.PK.ToGuid(),
			},
			Order = 1,
		});

		var snapshots = repository.FindByUserId(userId).ToList();
		AssertEquals(1, snapshots.Count);

		var snapshot = snapshots[0];
		AssertEquals(filterOne.PK, snapshot.ModuleFilter.ModuleFilterId);

		snapshot.ModuleFilter.ModuleFilterId = filterTwo.PK.ToGuid();
		snapshot.Order = 2;

		AssertEquals(1, repository.Update(snapshot));

		snapshots = repository.FindByUserId(userId).ToList();
		AssertEquals(1, snapshots.Count);

		snapshot = snapshots[0];
		AssertEquals(filterTwo.PK, snapshot.ModuleFilter.ModuleFilterId);

		Assert(snapshot.ModuleFilter.ModuleFilterId == filterTwo.PK);
		Assert(snapshot.ModuleFilter.ModuleFilterName == filterTwo.S9_FilterName);
		Assert(snapshot.Order == 2);
		Assert(snapshot.Id == snapshotId);
	}

	public void TestDelete()
	{
		Factory.Load<StmModuleFilter>(new ZQuery()).DeleteAll();
		Factory.Load<StmData>(new ZQuery()).DeleteAll();

		var repository = new SnapshotsRepository();
		AssertEquals("0 record should be deleted", 0, repository.Delete(Guid.NewGuid()));

		var filter = Factory.New<StmModuleFilter>();
		filter.S9_FilterName = "rule1";
		filter.S9_ModuleID = "WorkItem";
		Factory.Save();

		var userId = GlbStaff.CurrentUser.PK.ToGuid();
		var snapshotId = repository.Create(new Snapshot()
		{
			Owner = userId,
			ModuleFilter = new SnapshotModuleFilter()
			{
				ModuleFilterId = filter.PK.ToGuid(),
			}
		});

		var snapshots = repository.FindByUserId(userId).ToList();
		AssertEquals(1, snapshots.Count);

		var snapshot = snapshots[0];
		Assert(snapshot.ModuleFilter.ModuleFilterId == filter.PK);
		AssertEquals("1 record should be deleted", 1, repository.Delete(snapshot.Id));
		AssertEquals("Should not find snapshot", 0, repository.FindByUserId(userId).Count());
	}
}
