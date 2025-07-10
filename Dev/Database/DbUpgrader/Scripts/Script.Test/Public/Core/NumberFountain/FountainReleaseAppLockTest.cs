using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.NumberFountain;
using NUnit.Framework;
using Helper = CargoWise.Data.Testing.NumberFountainTestHelper;

namespace Enterprise.Build.Database.Script.Public.Core.NumberFountain.Testing
{
	[TestedType(typeof(FountainReleaseAppLock))]
	class FountainReleaseAppLockTest : NumberFountainTestCase
	{
		public void TestSampleCall()
		{
			var name = FountainName;
			var owner = FountainOwner;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(TestConnection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(TestConnection, name, owner, minValue: 1, nextValue: 1, maxValue: 10);
			AssertEquals("Fountain Id", fountainId, Helper.GetFountainId(TestConnection, name, owner).Value);

			var resource = Helper.GetAppLockResource(fountainId);

			FountainReleaseAppLockTestHelper.Run(TestConnection, name, owner, fountainId, resource);

			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.NoLock);

			Helper.GetAppLock(TestConnection, fountainId);
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(TestConnection, name: "", owner: Guid.Empty, fountainId: 0, resource: "");
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(TestConnection, name: "", owner: Guid.Empty, fountainId: 0, resource: "Unknown resource");
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(TestConnection, name: "", owner: Guid.Empty, fountainId: -10, resource: "");
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(TestConnection, name: "Unknown name", owner: Guid.Empty, fountainId: 0, resource: "");
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(TestConnection, name: "", owner: Guid.Empty, fountainId: 0, resource: resource);
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.NoLock);

			Helper.GetAppLock(TestConnection, fountainId);
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(TestConnection, name: "", owner: Guid.Empty, fountainId: fountainId, resource: "");
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.NoLock);

			Helper.GetAppLock(TestConnection, fountainId);
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(TestConnection, name: name, owner: owner, fountainId: 0, resource: "");
			Helper.AssertAppLockMode(TestConnection, resource, expected: Helper.AppLockMode.NoLock);
		}
	}

	[UseSnapshotProtection]
	class FountainReleaseAppLockNonTransactionedTest : TestCase
	{
		public void TestReleaseLockWithoutTransanction()
		{
			var name = "FountainName";
			var owner = Guid.Empty;

			AssertEquals("Fountain exists?", false, Helper.GetFountainId(Db.Connection, name, owner).HasValue);

			var fountainId = Helper.CreateFountain(Db.Connection, name, owner, minValue: 1, nextValue: 1, maxValue: 10);
			AssertEquals("Fountain Id", fountainId, Helper.GetFountainId(Db.Connection, name, owner).Value);

			var resource = Helper.GetAppLockResource(fountainId);

			FountainReleaseAppLockTestHelper.Run(Db.Connection, name, owner, fountainId, resource);

			Helper.AssertAppLockMode(Db.Connection, resource, expected: Helper.AppLockMode.NoLock);

			Helper.GetAppLock(Db.Connection, fountainId);
			Helper.AssertAppLockMode(Db.Connection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(Db.Connection, name: "", owner: Guid.Empty, fountainId: 0, resource: resource);
			Helper.AssertAppLockMode(Db.Connection, resource, expected: Helper.AppLockMode.NoLock);

			Helper.GetAppLock(Db.Connection, fountainId);
			Helper.AssertAppLockMode(Db.Connection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(Db.Connection, name: "", owner: Guid.Empty, fountainId: fountainId, resource: "");
			Helper.AssertAppLockMode(Db.Connection, resource, expected: Helper.AppLockMode.NoLock);

			Helper.GetAppLock(Db.Connection, fountainId);
			Helper.AssertAppLockMode(Db.Connection, resource, expected: Helper.AppLockMode.Exclusive);

			FountainReleaseAppLockTestHelper.Run(Db.Connection, name: name, owner: owner, fountainId: 0, resource: "");
			Helper.AssertAppLockMode(Db.Connection, resource, expected: Helper.AppLockMode.NoLock);
		}
	}
}

