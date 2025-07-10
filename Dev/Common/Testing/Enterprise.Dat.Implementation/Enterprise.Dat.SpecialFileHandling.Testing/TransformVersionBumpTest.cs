using System;
using System.IO;
using Dat.Integration;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;
using WTG.DevTools.Definitions;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	class TransformVersionBumpTest : TestCase
	{
		public void TestShouldUnshelveForDATCheckin()
		{
			var change = new MockPendingChange(TfsChangeType.Edit, GetServerPath(TransformVersionFileServerPath));
			var shouldUnshelve = new TransformVersionBump(new SpecialFileHandlerContext(GetRepositoryKey()))
				.ShouldUnshelveForDATCheckin(change);
			AssertEquals("We should not unshelve changes to the version file.", false, shouldUnshelve);
		}

		public void TestOnlineTransformTriggersSchemaVersionChange()
		{
			ExpectOnlineTransformChange();
			var updated = new TransformVersionBump(new SpecialFileHandlerContext(GetRepositoryKey())).UpdateForDATCheckin(workspace.Object, new MockPendingChange(TfsChangeType.Edit, GetServerPath(OnlineTransformPath)));
			AssertContainsExactElementsInAnyOrder(new[] { versionFile.Filename }, updated);
			AssertEquals(SchemaVersionFileContents.Replace("ApplicationNumber = new VersionLabel(7001, 0)", "ApplicationNumber = new VersionLabel(7002, 0)"), File.ReadAllText(versionFile.Filename));
		}

		public void TestOnlineTransformInCWReleaseBranch()
		{
			workspace.Setup(m => m.GetPendingChanges()).Returns(Array.Empty<IPendingChange>());
			workspace.Setup(
					m => m.GetLocalItemForServerItem(GetReleaseBranchServerPath(TransformVersionFileServerPath)))
				.Returns(versionFile.Filename);
			workspace.Setup(m => m.GetLatest(new[] { GetReleaseBranchServerPath(TransformVersionFileServerPath) },
				TfsRecursionType.None, TfsGetOptions.None)).Returns((IGetStatus)null);
			workspace.Setup(m => m.PendEdit(GetReleaseBranchServerPath(TransformVersionFileServerPath))).Returns(1);
			var updated = new TransformVersionBump(new SpecialFileHandlerContext(GetReleaseBranchRepositoryKey())).UpdateForDATCheckin(workspace.Object, new MockPendingChange(TfsChangeType.Edit, GetReleaseBranchServerPath(OnlineTransformPath)));
			AssertContainsExactElementsInAnyOrder(new[] { versionFile.Filename }, updated);
			AssertEquals(SchemaVersionFileContents.Replace("ApplicationNumber = new VersionLabel(7001, 0)", "ApplicationNumber = new VersionLabel(7001, 1)"), File.ReadAllText(versionFile.Filename));
		}

		void ExpectOnlineTransformChange()
		{
			if (expectedSchemaChangeOnce)
			{
				workspace.Setup(m => m.GetPendingChanges()).Returns(new IPendingChange[] { new MockPendingChange(TfsChangeType.Edit, GetServerPath(TransformVersionFileServerPath), versionFile.Filename) });
			}
			else
			{
				workspace.Setup(m => m.GetPendingChanges()).Returns(Array.Empty<IPendingChange>());
				workspace.Setup(m => m.GetLocalItemForServerItem(GetServerPath(TransformVersionFileServerPath)))
					.Returns(versionFile.Filename);
				workspace.Setup(m => m.GetLatest(new[] { GetServerPath(TransformVersionFileServerPath) },
					TfsRecursionType.None, TfsGetOptions.None)).Returns((IGetStatus)null);
				workspace.Setup(m => m.PendEdit(GetServerPath(TransformVersionFileServerPath))).Returns(1);
				expectedSchemaChangeOnce = true;
			}
		}

		protected override void SetUp()
		{
			versionFile = TempFile.New();
			File.WriteAllText(versionFile.Filename, SchemaVersionFileContents);

			workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
		}

		protected override void TearDown()
		{
			versionFile.Dispose();
		}

		IRepositoryKey GetRepositoryKey() => RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat();
		string GetServerPath(string serverPath) => serverPath;
		IRepositoryKey GetReleaseBranchRepositoryKey() => RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "releases/CW20161129", "/").ForDat();
		string GetReleaseBranchServerPath(string serverPath) => serverPath;

		Mock<IWorkspaceAccess> workspace;
		TempFile versionFile;
		bool expectedSchemaChangeOnce;

		public const string TransformVersionFileServerPath = "/Database/Odyssey/Resource/Version/TransformationVersion.cs";
		const string OnlineTransformPath = "/Enterprise/Product/Core/DbUpgrader/Schema/Schema.Launch/Launch/DocManagerSchemaSynchronisationWrapper.cs";

		public const string SchemaVersionFileContents = @"
	public static class TransformationVersion
	{
		public static readonly VersionLabel ApplicationNumber = new VersionLabel(7001, 0);
	}
";
	}
}
