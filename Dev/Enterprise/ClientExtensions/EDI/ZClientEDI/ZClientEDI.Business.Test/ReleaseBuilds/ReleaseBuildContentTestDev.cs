using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.ReleaseBuilds.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.EDI.ReleaseBuilds.Test
{
	public class ReleaseBuildContentTestDevGit : ReleaseBuildContentTestBase
	{
		protected override void CreateDeployedUserTest(Guid wiPk, Guid? taskPk, string repository, string branch, Version version, ZDateTime buildTime)
		{
			var uhPk = Guid.NewGuid();
			var utPk = Guid.NewGuid();
			AddUserTestHeader(uhPk, "SCH", wiPk, taskPk, "CNF", buildTime);
			AddUserTest(utPk, uhPk, repository, branch, "PAS");
			AddBuildJobResult(utPk, "RELEASE", version, buildTime);
			AddDeploymentJob(utPk, DeploymentConfiguration);
		}

		protected override string DeploymentBuildRepository => "http://tfs.wtg.zone:8080/CargoWise/Dev/_git/Dev";
		protected override IReadOnlyList<string> BranchRoots => new[] { "http://tfs.wtg.zone:8080/CargoWise/Dev" };
		protected override string TrunkDeploymentBranch => "master";
		protected override string FirstReleaseDeploymentBranch => "releases/CW170101";
		protected override string SecondReleaseDeploymentBranch => "releases/CW170202";
		protected override string ESevicesDeploymentBranch => "releases/production";
		protected override string DeploymentConfiguration => ReleaseBuildContent.IBPArchiveDeploymentConfiguration;
	}

	sealed class ReleaseBuildContentTestDevCombinedGit : ReleaseBuildContentTestBase
	{
		protected override void CreateDeployedUserTest(Guid wiPk, Guid? taskPk, string repository, string branch, Version version, ZDateTime buildTime)
		{
			var uhPk = Guid.NewGuid();
			var utPk = Guid.NewGuid();
			AddUserTestHeader(uhPk, "SCH", wiPk, taskPk, "CNF", buildTime);
			AddUserTest(utPk, uhPk, repository, branch, "PAS");
			AddBuildJobResult(utPk, "RELEASE", null, buildTime);
			CreateDeploymentBuild(version, buildTime.AddMinutes(1), branch, DeploymentConfiguration);
		}

		protected override string DeploymentBuildRepository => "http://tfs.wtg.zone:8080/CargoWise/Dev/_git/Dev";
		protected override IReadOnlyList<string> BranchRoots => new[] { "http://tfs.wtg.zone:8080/CargoWise/Dev/_git/Dev" };
		protected override string TrunkDeploymentBranch => "master";
		protected override string FirstReleaseDeploymentBranch => "releases/CW170101";
		protected override string SecondReleaseDeploymentBranch => "releases/CW170202";
		protected override string ESevicesDeploymentBranch => "releases/production";
		protected override string DeploymentConfiguration => ReleaseBuildContent.IBPArchiveDeploymentConfiguration;
	}

	sealed class ReleaseBuildContentTestCWShared : ReleaseBuildContentTestBase
	{
		protected override void CreateDeployedUserTest(Guid wiPk, Guid? taskPk, string repository, string branch, Version version, ZDateTime buildTime)
		{
			var uhPk = Guid.NewGuid();
			var utPk = Guid.NewGuid();
			AddUserTestHeader(uhPk, "SCH", wiPk, taskPk, "CNF", buildTime);
			AddUserTest(utPk, uhPk, repository == DeploymentBuildRepository ? "http://tfs.wtg.zone:8080/CargoWise/Dev/_git/Shared" : repository, branch, "PAS");
			AddBuildJobResult(utPk, "RELEASE", null, buildTime);
			CreateDeploymentBuild(version, buildTime.AddMinutes(1), branch, DeploymentConfiguration);
		}

		protected override string DeploymentBuildRepository => "http://tfs.wtg.zone:8080/CargoWise/Dev/_git/Deployment";
		protected override IReadOnlyList<string> BranchRoots => new[] { "http://tfs.wtg.zone:8080/CargoWise/Dev/_git/Shared", "http://tfs.wtg.zone:8080/CargoWise/Dev/_git/Dev", "http://tfs.wtg.zone:8080/CargoWise/Dev/_git/Deployment" };
		protected override string TrunkDeploymentBranch => "master";
		protected override string FirstReleaseDeploymentBranch => "releases/CW170101";
		protected override string SecondReleaseDeploymentBranch => "releases/CW170202";
		protected override string ESevicesDeploymentBranch => "releases/production";
		protected override string DeploymentConfiguration => ReleaseBuildContent.IBPArchiveDeploymentConfiguration;
	}

	public abstract class ReleaseBuildContentTestBase : TestCaseWithFactory
	{
		public void TestItemsAlongSameReleaseBranch()
		{
			TestItemsAlongSameBranch(FirstReleaseDeploymentBranch);
		}

		public void TestItemsAlongTrunk()
		{
			TestItemsAlongSameBranch(TrunkDeploymentBranch);
		}

		void TestItemsAlongSameBranch(string branch)
		{
			var d = ZDateTime.Now.AddDays(-1);
			var wiBeforeFrom = CreateDeployedWorkItem(branch, new Version(17, 1, 1, 10), d.AddMinutes(10));
			var wiFrom = CreateDeployedWorkItem(branch, new Version(17, 1, 1, 20), d.AddMinutes(20));
			var rbFrom = CreateReleaseBuild(new Version(17, 1, 1, 20));
			var wiBetweenFromAndTo = CreateDeployedWorkItem(branch, new Version(17, 1, 1, 30), d.AddMinutes(30));
			var wiOnAnotherBranchBetweenFromAndTo = CreateDeployedWorkItem("$/DevTools", new Version(17, 1, 1, 40), d.AddMinutes(40));
			var wiTo = CreateDeployedWorkItem(branch, new Version(17, 1, 1, 50), d.AddMinutes(50));
			var rbTo = CreateReleaseBuild(new Version(17, 1, 1, 50));
			var wiAfterTo = CreateDeployedWorkItem(branch, new Version(17, 1, 1, 60), d.AddMinutes(60));
			Factory.Save();
			var expectedWis = new[] { wiBetweenFromAndTo, wiTo };
			AssertContainsExactElementsInAnyOrder(expectedWis, GetReleaseBuildContent().GetPatchedWorkItems(rbFrom, rbTo));
			AssertContainsExactElementsInAnyOrder(expectedWis.Select(wi => wi.RelatedItems.Single()), GetReleaseBuildContent().GetPatchedIncidents(rbFrom, rbTo));

			var content = GetReleaseBuildContent();
			AssertEquals(true, content.IsPatchedTo(wiBeforeFrom, rbFrom));
			AssertEquals(true, content.IsPatchedTo(wiFrom, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiBetweenFromAndTo, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiOnAnotherBranchBetweenFromAndTo, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiTo, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiAfterTo, rbFrom));

			AssertEquals(true, content.IsPatchedTo(wiBeforeFrom, rbTo));
			AssertEquals(true, content.IsPatchedTo(wiFrom, rbTo));
			AssertEquals(true, content.IsPatchedTo(wiBetweenFromAndTo, rbTo));
			AssertEquals(false, content.IsPatchedTo(wiOnAnotherBranchBetweenFromAndTo, rbFrom));
			AssertEquals(true, content.IsPatchedTo(wiTo, rbTo));
			AssertEquals(false, content.IsPatchedTo(wiAfterTo, rbTo));
		}

		public void TestGetReleaseBuildByTask()
		{
			var wi = Factory.NewWithValidTestData<NewWorkItem>();
			var ch0task = wi.WorkflowItems.AddNew();
			ch0task.P9_Type = "CH0";
			ch0task.P9_Status = "CLS";
			var ch1task = wi.WorkflowItems.AddNew();
			ch1task.P9_Type = "CH1";
			ch1task.P9_Status = "CLS";
			var ch2task = wi.WorkflowItems.AddNew();
			ch2task.P9_Type = "CH2";
			ch2task.P9_Status = "CLS";
			Factory.Save();
			CreateDeployedUserTest(wi.PK.ToGuid(), ch1task.PK.ToGuid(), FirstReleaseDeploymentBranch, new Version(16, 12, 18, 72), ZDateTime.Now.AddDays(-1));
			CreateDeployedUserTest(wi.PK.ToGuid(), ch2task.PK.ToGuid(), SecondReleaseDeploymentBranch, new Version(16, 10, 11, 532), ZDateTime.Now.AddDays(-1));
			CreateDeployedUserTest(wi.PK.ToGuid(), ch0task.PK.ToGuid(), TrunkDeploymentBranch, new Version(17, 1, 1, 10), ZDateTime.Now.AddDays(-1));
			CreateReleaseBuild(new Version(16, 12, 18, 72));
			CreateReleaseBuild(new Version(16, 10, 11, 532));
			CreateReleaseBuild(new Version(17, 1, 1, 10));
			var content = GetReleaseBuildContent();
			AssertEquals("17.1.1.10", content.GetReleaseBuild(ch0task.PK).VersionNumber.ToString());
			AssertEquals("16.12.18.72", content.GetReleaseBuild(ch1task.PK).VersionNumber.ToString());
			AssertEquals("16.10.11.532", content.GetReleaseBuild(ch2task.PK).VersionNumber.ToString());
		}

		public void TestItemsAcrossReleaseBranches()
		{
			var d = ZDateTime.Now.AddDays(-1);
			var wiInAlpBeforeFrom = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 10), d.AddMinutes(10));
			CreateDeploymentBuild(new Version(17, 1, 1, 20), d.AddMinutes(20), FirstReleaseDeploymentBranch, DeploymentConfiguration);
			var wiInSourceBranchBeforeFrom = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 30), d.AddMinutes(30));
			var wiInSourceBranchFrom = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 40), d.AddMinutes(40));
			var rbFrom = CreateReleaseBuild(new Version(17, 1, 1, 40));
			var wiInSourceBranchAfterFrom = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 50), d.AddMinutes(50));
			var wiInAlpBetweenBranches = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 10, 10), d.AddMinutes(60));
			CreateDeploymentBuild(new Version(17, 2, 2, 10), d.AddMinutes(70), SecondReleaseDeploymentBranch, DeploymentConfiguration);
			var wiInAlpAfterTargetBranch = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 2, 3, 10), d.AddMinutes(80));
			var wiInTargetBranchBeforeTo = CreateDeployedWorkItem(SecondReleaseDeploymentBranch, new Version(17, 2, 2, 20), d.AddMinutes(90));
			var wiInTargetBranchTo = CreateDeployedWorkItem(SecondReleaseDeploymentBranch, new Version(17, 2, 2, 30), d.AddMinutes(100));
			var rbTo = CreateReleaseBuild(new Version(17, 2, 2, 30));
			var wiInTargetBranchAfterTo = CreateDeployedWorkItem(SecondReleaseDeploymentBranch, new Version(17, 2, 2, 40), d.AddMinutes(110));
			Factory.Save();
			var expectedWis = new[] { wiInAlpBetweenBranches, wiInTargetBranchBeforeTo, wiInTargetBranchTo };
			AssertContainsExactElementsInAnyOrder(expectedWis, GetReleaseBuildContent().GetPatchedWorkItems(rbFrom, rbTo));
			AssertContainsExactElementsInAnyOrder(expectedWis.Select(wi => wi.RelatedItems.Single()), GetReleaseBuildContent().GetPatchedIncidents(rbFrom, rbTo));

			var content = GetReleaseBuildContent();
			AssertEquals(true, content.IsPatchedTo(wiInAlpBeforeFrom, rbFrom));
			AssertEquals(true, content.IsPatchedTo(wiInSourceBranchBeforeFrom, rbFrom));
			AssertEquals(true, content.IsPatchedTo(wiInSourceBranchFrom, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiInSourceBranchAfterFrom, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiInAlpBetweenBranches, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiInAlpAfterTargetBranch, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiInTargetBranchBeforeTo, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiInTargetBranchTo, rbFrom));
			AssertEquals(false, content.IsPatchedTo(wiInTargetBranchAfterTo, rbFrom));

			AssertEquals(true, content.IsPatchedTo(wiInAlpBeforeFrom, rbTo));
			AssertEquals(false, content.IsPatchedTo(wiInSourceBranchBeforeFrom, rbTo));
			AssertEquals(false, content.IsPatchedTo(wiInSourceBranchFrom, rbTo));
			AssertEquals(false, content.IsPatchedTo(wiInSourceBranchAfterFrom, rbTo));
			AssertEquals(true, content.IsPatchedTo(wiInAlpBetweenBranches, rbTo));
			AssertEquals(false, content.IsPatchedTo(wiInAlpAfterTargetBranch, rbTo));
			AssertEquals(true, content.IsPatchedTo(wiInTargetBranchBeforeTo, rbTo));
			AssertEquals(true, content.IsPatchedTo(wiInTargetBranchTo, rbTo));
			AssertEquals(false, content.IsPatchedTo(wiInTargetBranchAfterTo, rbTo));
		}

		public void TestItemsAcrossReleaseBranchesMergedFromTrunk()
		{
			var d = ZDateTime.Now.AddDays(-1);
			CreateDeploymentBuild(new Version(17, 1, 1, 10), d.AddMinutes(10), FirstReleaseDeploymentBranch, DeploymentConfiguration);
			var wiMergedToSourceBeforeFrom = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 20), d.AddMinutes(20));
			CreateDeployedUserTest(wiMergedToSourceBeforeFrom.PK.ToGuid(), FirstReleaseDeploymentBranch, new Version(17, 1, 1, 30), d.AddMinutes(30));
			var wiMergedToSourceFrom = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 40), d.AddMinutes(40));
			var wiMergedToSourceAfterFrom = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 50), d.AddMinutes(50));
			var wiMergedToTargetBeforeTo = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 60), d.AddMinutes(60));
			var wiMergedToTargetTo = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 70), d.AddMinutes(70));
			var wiMergedToTargetAfterTo = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 80), d.AddMinutes(80));
			CreateDeployedUserTest(wiMergedToSourceFrom.PK.ToGuid(), FirstReleaseDeploymentBranch, new Version(17, 1, 1, 90), d.AddMinutes(90));
			var rbFrom = CreateReleaseBuild(new Version(17, 1, 1, 90));
			CreateDeployedUserTest(wiMergedToSourceAfterFrom.PK.ToGuid(), FirstReleaseDeploymentBranch, new Version(17, 1, 1, 100), d.AddMinutes(100));
			CreateDeploymentBuild(new Version(17, 2, 2, 10), d.AddMinutes(110), SecondReleaseDeploymentBranch, DeploymentConfiguration);
			CreateDeployedUserTest(wiMergedToTargetBeforeTo.PK.ToGuid(), SecondReleaseDeploymentBranch, new Version(17, 2, 2, 20), d.AddMinutes(120));
			CreateDeployedUserTest(wiMergedToTargetTo.PK.ToGuid(), SecondReleaseDeploymentBranch, new Version(17, 2, 2, 30), d.AddMinutes(130));
			var rbTo = CreateReleaseBuild(new Version(17, 2, 2, 30));
			CreateDeployedUserTest(wiMergedToTargetAfterTo.PK.ToGuid(), SecondReleaseDeploymentBranch, new Version(17, 2, 2, 40), d.AddMinutes(140));
			Factory.Save();
			var expectedWis = new[] { wiMergedToSourceAfterFrom, wiMergedToTargetBeforeTo, wiMergedToTargetTo, wiMergedToTargetAfterTo };
			AssertContainsExactElementsInAnyOrder(expectedWis, GetReleaseBuildContent().GetPatchedWorkItems(rbFrom, rbTo));
			AssertContainsExactElementsInAnyOrder(expectedWis.Select(wi => wi.RelatedItems.Single()), GetReleaseBuildContent().GetPatchedIncidents(rbFrom, rbTo));
		}

		public void TestItemsFromTrunkToReleaseBranch()
		{
			var d = ZDateTime.Now.AddDays(-1);
			var wiBeforeFrom = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 10), d.AddMinutes(10));
			var wiFrom = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 20), d.AddMinutes(20));
			var rbFrom = CreateReleaseBuild(new Version(17, 1, 1, 20));
			var wiBetweenFromAndReleaseBranch = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 30), d.AddMinutes(30));
			CreateDeploymentBuild(new Version(17, 1, 1, 40), d.AddMinutes(40), FirstReleaseDeploymentBranch, DeploymentConfiguration);
			var wiInAlpAfterReleaseBranch = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 50), d.AddMinutes(50));
			var wiInTargetBranchBeforeTo = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 60), d.AddMinutes(60));
			var wiInTargetBranchTo = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 70), d.AddMinutes(70));
			var rbTo = CreateReleaseBuild(new Version(17, 1, 1, 70));
			var wiInTargetBranchAfterTo = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 80), d.AddMinutes(80));
			Factory.Save();
			var expectedWis = new[] { wiBetweenFromAndReleaseBranch, wiInTargetBranchBeforeTo, wiInTargetBranchTo };
			AssertContainsExactElementsInAnyOrder(expectedWis, GetReleaseBuildContent().GetPatchedWorkItems(rbFrom, rbTo));
			AssertContainsExactElementsInAnyOrder(expectedWis.Select(wi => wi.RelatedItems.Single()), GetReleaseBuildContent().GetPatchedIncidents(rbFrom, rbTo));
		}

		public void TestItemsFromReleaseBranchToTrunk()
		{
			var d = ZDateTime.Now.AddDays(-1);
			var wiInAlpBeforeReleaseBranch = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 10), d.AddMinutes(10));
			CreateDeploymentBuild(new Version(17, 1, 1, 20), d.AddMinutes(20), FirstReleaseDeploymentBranch, DeploymentConfiguration);
			var wiInAlpAfterReleaseBranch = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 30), d.AddMinutes(30));
			var wiInSourceBranchBeforeFrom = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 40), d.AddMinutes(40));
			var wiInSourceBranchFrom = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 50), d.AddMinutes(50));
			var rbFrom = CreateReleaseBuild(new Version(17, 1, 1, 50));
			var wiInSourceBranchAfterFrom = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 60), d.AddMinutes(60));
			var wiInAlpTo = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 70), d.AddMinutes(70));
			var rbTo = CreateReleaseBuild(new Version(17, 1, 1, 70));
			var wiInAlpAfterTo = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 80), d.AddMinutes(80));
			Factory.Save();
			var expectedWis = new[] { wiInAlpAfterReleaseBranch, wiInAlpTo };
			AssertContainsExactElementsInAnyOrder(expectedWis, GetReleaseBuildContent().GetPatchedWorkItems(rbFrom, rbTo));
			AssertContainsExactElementsInAnyOrder(expectedWis.Select(wi => wi.RelatedItems.Single()), GetReleaseBuildContent().GetPatchedIncidents(rbFrom, rbTo));
		}

		public void TestItemsFromReleaseBranchToTrunkMerged()
		{
			var d = ZDateTime.Now.AddDays(-1);
			CreateDeploymentBuild(new Version(17, 1, 1, 10), d.AddMinutes(10), FirstReleaseDeploymentBranch, DeploymentConfiguration);
			var wiInAlpAfterReleaseBranchMerged = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 20), d.AddMinutes(20));
			CreateDeployedUserTest(wiInAlpAfterReleaseBranchMerged.PK.ToGuid(), FirstReleaseDeploymentBranch, new Version(17, 1, 1, 30), d.AddMinutes(30));
			var wiInSourceBranchFrom = CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(17, 1, 1, 50), d.AddMinutes(50));
			var rbFrom = CreateReleaseBuild(new Version(17, 1, 1, 50));
			var wiInAlpTo = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 60), d.AddMinutes(60));
			var rbTo = CreateReleaseBuild(new Version(17, 1, 1, 60));
			Factory.Save();
			var expectedWis = new[] { wiInAlpTo };
			AssertContainsExactElementsInAnyOrder(expectedWis, GetReleaseBuildContent().GetPatchedWorkItems(rbFrom, rbTo));
			AssertContainsExactElementsInAnyOrder(expectedWis.Select(wi => wi.RelatedItems.Single()), GetReleaseBuildContent().GetPatchedIncidents(rbFrom, rbTo));
		}

		public void TestCheckInPatchedToOnlyConsidersLatestReleaseBranch()
		{
			TestCheckInPatchedToOnlyConsidersLatest(FirstReleaseDeploymentBranch);
		}

		public void TestCheckInPatchedToOnlyConsidersLatestTrunk()
		{
			TestCheckInPatchedToOnlyConsidersLatest(TrunkDeploymentBranch);
		}

		void TestCheckInPatchedToOnlyConsidersLatest(string branch)
		{
			var d = ZDateTime.Now.AddDays(-1);
			var wi = CreateDeployedWorkItem(branch, new Version(17, 1, 1, 20), d.AddMinutes(20));
			CreateDeployedUserTest(wi.PK.ToGuid(), branch, new Version(17, 1, 1, 30), d.AddMinutes(30));
			var rb1 = CreateReleaseBuild(new Version(17, 1, 1, 20));
			var rb2 = CreateReleaseBuild(new Version(17, 1, 1, 30));

			var content = GetReleaseBuildContent();
			AssertEquals(false, content.IsPatchedTo(wi, rb1));
			AssertEquals(true, content.IsPatchedTo(wi, rb2));
		}

		public void TestCheckInPatchedToWithNoMatchingDATRecord()
		{
			var d = ZDateTime.Now.AddDays(-1);
			var wi = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 20), d.AddMinutes(20));
			var rb = Factory.New<ReleaseBuild>();
			var content = GetReleaseBuildContent();
			AssertEquals(false, content.IsPatchedTo(wi, rb));
		}

		public void TestIsPatchedTo_NullValues()
		{
			var d = ZDateTime.Now.AddDays(-1);
			var content = GetReleaseBuildContent();
			CreateDeploymentBuild(new Version(17, 1, 1, 20), null, FirstReleaseDeploymentBranch, DeploymentConfiguration);
			var wiBeforeFrom = CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(17, 1, 1, 10), d.AddMinutes(10));
			var rbFrom = CreateReleaseBuild(new Version(17, 1, 1, 20));
			AssertEquals(false, content.IsPatchedTo(wiBeforeFrom, rbFrom));
		}

		public void TestIsCargoWiseOneChange()
		{
			using (var command = AutoTesterUserTestsConnection.Command(
				@"declare @rhgPk1 uniqueidentifier = newid()
				  declare @rhgPk2 uniqueidentifier = newid()
				  declare @rhgPk3 uniqueidentifier = newid()
				  insert into ReleaseBranchGroup (RHG_PK, RHG_Name, RHG_Ring) values (@rhgPk1, '1', 'DPR'), (@rhgPk2, '2', 'STD'), (@rhgPk3, '3', 'ZZZ')
				  insert into ReleaseBranch (RH_Branch, RH_RHG, RH_Source) values
					('$/CWReleases/CW170101', @rhgPk1, '$/whatever'),
					('$/CWReleases/CW170202', @rhgPk2, '$/whatever'),
					('releases/production', @rhgPk3, '$/whatever')"))
			{
				command.ExecuteNonQuery();
			}

			var d = ZDateTime.Now.AddDays(-1);
			AssertEquals(true, GetReleaseBuildContent().IsCargoWiseOneChange(CreateDeployedWorkItem(TrunkDeploymentBranch, new Version(18, 1, 1, 1), d.AddMinutes(1))));
			AssertEquals(true, GetReleaseBuildContent().IsCargoWiseOneChange(CreateDeployedWorkItem(FirstReleaseDeploymentBranch, new Version(18, 1, 1, 2), d.AddMinutes(2))));
			AssertEquals(true, GetReleaseBuildContent().IsCargoWiseOneChange(CreateDeployedWorkItem(SecondReleaseDeploymentBranch, new Version(18, 1, 1, 3), d.AddMinutes(3))));
			AssertEquals(false, GetReleaseBuildContent().IsCargoWiseOneChange(CreateDeployedWorkItem(DeploymentBuildRepository == null ? null : "http://notcargowiseone", DeploymentBuildRepository == null ? "$/NotCargoWiseOne" : "master", new Version(18, 1, 1, 4), d.AddMinutes(4))));
			AssertEquals(false, GetReleaseBuildContent().IsCargoWiseOneChange(CreateDeployedWorkItem("http://notcargowiseone", ESevicesDeploymentBranch, new Version(18, 1, 1, 3), d.AddMinutes(5))));
			AssertEquals(true, GetReleaseBuildContent().IsCargoWiseOneChange(CreateDeployedWorkItemWithStatus(TrunkDeploymentBranch, new Version(18, 1, 1, 1), d.AddMinutes(1), "CIN", "CIN")));
			AssertEquals(true, GetReleaseBuildContent().IsCargoWiseOneChange(CreateDeployedWorkItemWithStatus(TrunkDeploymentBranch, new Version(18, 1, 1, 1), d.AddMinutes(1), "CNF", "CIN")));
		}

		public void TestGetReleaseBuildInfoIsCached()
		{
			var branch = TrunkDeploymentBranch;
			var fromDate = ZDateTime.Now.AddDays(-1).AddMinutes(10);
			var toDate = ZDateTime.Now.AddDays(-1).AddMinutes(20);
			CreateDeployedWorkItem(branch, new Version(17, 1, 1, 10), fromDate);
			CreateDeployedWorkItem(branch, new Version(18, 1, 1, 1), toDate);
			Factory.Save();

			var content = GetReleaseBuildContent();

			var info1 = content.GetReleaseBuildInfo(new Version(17, 1, 1, 10));
			var info2 = content.GetReleaseBuildInfo(new Version(17, 1, 1, 10));
			var info3 = content.GetReleaseBuildInfo(new Version(18, 1, 1, 1));
			var info4 = content.GetReleaseBuildInfo(new Version(18, 1, 1, 1));
			var info5 = content.GetReleaseBuildInfo(new Version(17, 1, 1, 10), new Version(18, 1, 1, 1));
			var info6 = content.GetReleaseBuildInfo(new Version(17, 1, 1, 10), new Version(18, 1, 1, 1));

			AssertNotNull(info1.FromDate);
			AssertEquals(branch, info1.FromBranch);
			AssertEquals(null, info1.ToDate);
			AssertEquals(null, info1.ToBranch);

			AssertNotNull(info3.FromDate);
			AssertEquals(branch, info3.FromBranch);
			AssertEquals(null, info3.ToDate);
			AssertEquals(null, info3.ToBranch);

			AssertNotNull(info5.FromDate);
			AssertEquals(branch, info5.FromBranch);
			AssertNotNull(info5.ToDate);
			AssertEquals(branch, info5.ToBranch);

			AssertEquals(info1, info2);
			Assert(object.ReferenceEquals(info1, info2)); // Cached value should be same object
			AssertNotEquals(info2, info3);
			Assert(!object.ReferenceEquals(info2, info3));
			Assert(object.ReferenceEquals(info3, info4)); // Cached value should be same object
			AssertEquals(info3, info4);
			Assert(!object.ReferenceEquals(info4, info5));
			Assert(object.ReferenceEquals(info5, info6)); // Cached value should be same object
			AssertNotEquals(info4, info5);
		}

		protected ReleaseBuild CreateReleaseBuild(Version version)
		{
			var releaseBuild = Factory.NewWithValidTestData<ReleaseBuild>();
			releaseBuild.VersionNumber = new VersionNumber(version);
			return releaseBuild;
		}

		protected NewWorkItem CreateDeployedWorkItem(string branch, Version version, ZDateTime buildTime)
		{
			return CreateDeployedWorkItem(DeploymentBuildRepository, branch, version, buildTime);
		}

		protected NewWorkItem CreateDeployedWorkItemWithStatus(string branch, Version version, ZDateTime buildTime, string userTestHeaderStatus, string userTestStatus)
		{
			var wi = Factory.NewWithValidTestData<NewWorkItem>();
			var cs = Factory.NewWithValidTestData<SupportIncident>();
			wi.RelatedItems.Add(cs);
			
			var uhPk = Guid.NewGuid();
			var utPk = Guid.NewGuid();
			AddUserTestHeader(uhPk, "SCH", wi.PK.ToGuid(), null, userTestHeaderStatus, buildTime);
			AddUserTest(utPk, uhPk, DeploymentBuildRepository, branch, userTestStatus);
			AddBuildJobResult(utPk, "RELEASE", version, buildTime);
			AddDeploymentJob(utPk, DeploymentConfiguration);

			return wi;
		}

		protected NewWorkItem CreateDeployedWorkItem(string repository, string branch, Version version, ZDateTime buildTime)
		{
			var wi = Factory.NewWithValidTestData<NewWorkItem>();
			var cs = Factory.NewWithValidTestData<SupportIncident>();
			cs.RelatedItems.Add(wi);
			CreateDeployedUserTest(wi.PK.ToGuid(), null, repository, branch, version, buildTime);
			return wi;
		}

		protected void CreateDeployedUserTest(Guid wiPk, string branch, Version version, ZDateTime buildTime)
		{
			CreateDeployedUserTest(wiPk, null, branch, version, buildTime);
		}

		protected void CreateDeployedUserTest(Guid wiPk, Guid? taskPk, string branch, Version version, ZDateTime buildTime)
		{
			CreateDeployedUserTest(wiPk, taskPk, DeploymentBuildRepository, branch, version, buildTime);
		}

		protected abstract void CreateDeployedUserTest(Guid wiPk, Guid? taskPk, string repository, string branch, Version version, ZDateTime buildTime);

		protected void CreateDeploymentBuild(Version version, ZDateTime? processingStarted, string branch, string deploymentConfiguration)
		{
			CreateDeploymentBuild(version, processingStarted, DeploymentBuildRepository, branch, deploymentConfiguration);
		}

		protected void CreateDeploymentBuild(Version version, ZDateTime? processingStarted, string repository, string branch, string deploymentConfiguration)
		{
			var uhPk = Guid.NewGuid();
			var utPk = Guid.NewGuid();
			AddUserTestHeader(uhPk, "DBL", null, null, "PAS", processingStarted);
			AddUserTest(utPk, uhPk, repository, branch, "PAS");
			AddBuildJobResult(utPk, "RELEASE", version, processingStarted);
			AddDeploymentJob(utPk, deploymentConfiguration);
		}

		protected void CreateDeploymentBuild(Guid combinedBuildPk, Guid taskPk, Version version)
		{
			var uhPk = Guid.NewGuid();
			AddUserTestHeader(uhPk, "DBL", null, taskPk, "PAS", ZDateTime.Now);
			AddUserTest(combinedBuildPk, uhPk, null, TrunkDeploymentBranch, "PAS");
			AddBuildJobResult(combinedBuildPk, "RELEASE", version, ZDateTime.Now.AddMilliseconds(1));
			AddDeploymentJob(combinedBuildPk, DeploymentConfiguration);
		}

		protected void AddUserTestHeader(Guid uhPk, string type, Guid? wiPk, Guid? taskPk, string status, ZDateTime? commitTime)
		{
			using (var command = AutoTesterUserTestsConnection.Command(
				@"insert into UserTestHeader (UH_PK, UH_U1, UH_Submitted, UH_Type, UH_WorkItem, UH_P9, UH_Status, UH_DateRecordAdded, UH_CommitTime)
					values (@uhPk, (select top 1 U1_PK from [User]), getdate(), @type, @wiPk, @taskPk, @status, getdate(), @commitTime)"))
			{
				command.AddParameter("uhPk", SqlDbType.UniqueIdentifier, uhPk);
				command.AddParameter("type", SqlDbType.VarChar, 3, type);
				command.AddParameter("wiPk", SqlDbType.UniqueIdentifier, (object)wiPk ?? DBNull.Value);
				command.AddParameter("taskPk", SqlDbType.UniqueIdentifier, (object)taskPk ?? DBNull.Value);
				command.AddParameter("status", SqlDbType.VarChar, 3, status);
				command.AddParameter("commitTime", SqlDbType.DateTime, commitTime.HasValue ? commitTime.Value.ToDateTime() : DBNull.Value);
				command.ExecuteNonQuery();
			}
		}

		protected void AddUserTest(Guid utPk, Guid uhPk, string repository, string branch, string status)
		{
			using (var command = AutoTesterUserTestsConnection.Command(
				@"insert into UserTest (UT_PK, UT_UH, UT_Title, UT_TargetRepository, UT_Branch, UT_Status, UT_IsBranchInShelfChanges)
					values (@utPk, @uhPk, 'whatever', @repository, @branch, @status, 1)"))
			{
				command.AddParameter("utPk", SqlDbType.UniqueIdentifier, utPk);
				command.AddParameter("uhPk", SqlDbType.UniqueIdentifier, uhPk);
				command.AddParameter("repository", SqlDbType.VarChar, 128, (object)repository ?? DBNull.Value);
				command.AddParameter("branch", SqlDbType.VarChar, 128, branch);
				command.AddParameter("status", SqlDbType.VarChar, 3, status);
				command.ExecuteNonQuery();
			}
		}

		protected void AddBuildJobResult(Guid utPk, string configuration, Version version, ZDateTime? buildStarted = null)
		{
			using (var command = AutoTesterUserTestsConnection.Command(
				@"insert into BuildJob (BJ_PK, BJ_UT, BJ_Configuration, BJ_Submitted, BJ_Started)
					values (@bjPk, @utPk, @configuration, getDate(), @buildStarted)
				  insert into BuildResult (BR_PK, BR_BJ, BR_Succeeded, BR_VersionNumber)
					values (newid(), @bjPk, 1, @versionNumber)
				"))
			{
				command.AddParameter("bjPk", SqlDbType.UniqueIdentifier, Guid.NewGuid());
				command.AddParameter("utPk", SqlDbType.UniqueIdentifier, utPk);
				command.AddParameter("configuration", SqlDbType.VarChar, 10, configuration);
				command.AddParameter("buildStarted", SqlDbType.DateTime, buildStarted.HasValue ? buildStarted.Value.ToDateTime() : DBNull.Value);
				command.AddParameter("versionNumber", SqlDbType.Udt, version != null ? version.ToSqlHierarchyId() : DBNull.Value);
				new SqlParameterWrapper(command.GetParameter("versionNumber")).UdtTypeName = "HierarchyId";
				command.ExecuteNonQuery();
			}
		}

		protected void AddDeploymentJob(Guid utPk, string deploymentConfiguration)
		{
			using (var command = AutoTesterUserTestsConnection.Command(
				@"declare @dtPk uniqueidentifier
				select @dtPk = DT_PK from DeploymentTarget where DT_DeploymentConfiguration = @deploymentConfiguration
				if @dtPk is null begin
					set @dtPk = newid()
					insert into DeploymentTarget (DT_PK, DT_TargetType, DT_BuildConfiguration, DT_DeploymentConfiguration) 
						values (@dtPk, 1, 'RELEASE', @deploymentConfiguration)
				end
				insert into DeploymentJob (DJ_PK, DJ_UT, DJ_DT, DJ_Submitted)
					values (newid(), @utPk, @dtPk, getdate())"))
			{
				command.AddParameter("utPk", SqlDbType.UniqueIdentifier, utPk);
				command.AddParameter("deploymentConfiguration", SqlDbType.VarChar, deploymentConfiguration);
				command.ExecuteNonQuery();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			using (var command = AutoTesterUserTestsConnection.Command(
				"insert into BranchRoot (BO_Branch) values " + string.Join(",", BranchRoots.Select(b => "('" + b + "')"))))
			{
				command.ExecuteNonQuery();
			}
		}

		protected override void TearDown()
		{
			autoTesterUserTestsConnection?.Dispose();
			autoTesterUserTestsConnection = null;
			base.TearDown();
		}

		protected virtual IReadOnlyList<string> BranchRoots => new[] { "$/CWShared", "$/Dev", "$/Glow" };
		protected virtual string DeploymentBuildRepository => null;

		protected abstract string TrunkDeploymentBranch { get; }
		protected abstract string FirstReleaseDeploymentBranch { get; }
		protected abstract string SecondReleaseDeploymentBranch { get; }
		protected abstract string DeploymentConfiguration { get; }

		protected abstract string ESevicesDeploymentBranch { get; }

		DbConnection AutoTesterUserTestsConnection
		{
			get { return autoTesterUserTestsConnection ?? (autoTesterUserTestsConnection = DbConnectionCrikey.GetAutoTesterUserTestsConnection()); }
		}
		DbConnection autoTesterUserTestsConnection;

		protected ReleaseBuildContent GetReleaseBuildContent() => new ReleaseBuildContent(Factory);
	}
}
