using System.IO;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;
using Enterprise.ZArchitecture.Core;
using Moq;
using WTG.DevTools.Definitions;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	class MapperChangeRequestTest : GeneralTransformMapperChangeRequestTest
	{
		public void TestUpdatesTransformationVersionGit()
		{
			using (var mapperTextFile = TempFile.New())
			{
				File.WriteAllText(mapperTextFile.Filename, @"
// This is where you add your tranform
//
MyTransformation
");
				workspace.Setup(m => m.GetLocalItemForServerItem(TransformationVersionFileServerPath)).Returns(transformationVersionFile.Filename);
				workspace.Setup(m => m.GetLatest(new[] { TransformationVersionFileServerPath }, TfsRecursionType.None, TfsGetOptions.None)).Returns((IGetStatus)null);
				workspace.Setup(m => m.PendEdit(TransformationVersionFileServerPath)).Returns(1);

				workspace.Setup(m => m.GetLocalItemForServerItem(MapperFileServerPath)).Returns(preupgradeMapperFile.Filename);
				workspace.Setup(m => m.GetLatest(new[] { MapperFileServerPath }, TfsRecursionType.None, TfsGetOptions.None)).Returns((IGetStatus)null);
				workspace.Setup(m => m.PendEdit(MapperFileServerPath)).Returns(1);

				var updated =
					new UpgradeMapperChangeRequest(new SpecialFileHandlerContext(RepositoryKey
							.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/")
							.ForDat()))
						.UpdateForDATCheckin(workspace.Object,
							new MockPendingChange(TfsChangeType.Edit, ShelfMapperFileServerPath,
								mapperTextFile.Filename));

				AssertNotNull(nameof(updated), updated);
				AssertContainsExactElementsInAnyOrder(new[] { transformationVersionFile.Filename, preupgradeMapperFile.Filename }, updated);
				AssertEquals(UpdatedTransformationVersionFileContents, File.ReadAllText(transformationVersionFile.Filename));
				AssertEquals(UpdatedMapperFileContents_PostCutover, File.ReadAllText(preupgradeMapperFile.Filename));
			}
		}

		public void TestShouldUnshelveForDATCheckinGit_Add() => TestShouldUnshelveForDATCheckinGit(TfsChangeType.Add, true);
		public void TestShouldUnshelveForDATCheckinGit_Delete() => TestShouldUnshelveForDATCheckinGit(TfsChangeType.Delete, true);
		public void TestShouldUnshelveForDATCheckinGit_Edit() => TestShouldUnshelveForDATCheckinGit(TfsChangeType.Edit, false);

		void TestShouldUnshelveForDATCheckinGit(TfsChangeType tfsChangeType, bool expectedResult)
		{
			var change = new MockPendingChange(tfsChangeType, ShelfMapperFileServerPath);
			var shouldUnshelve = new UpgradeMapperChangeRequest(new SpecialFileHandlerContext(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat()))
				.ShouldUnshelveForDATCheckin(change);
			AssertEquals("We should unshelve add type changes", expectedResult, shouldUnshelve);
		}

		protected override void SetUp()
		{
			transformationVersionFile = TempFile.New();
			File.WriteAllText(transformationVersionFile.Filename, OriginalTransformationVersionFileContents);

			preupgradeMapperFile = TempFile.New();
			File.WriteAllText(preupgradeMapperFile.Filename, OriginalMapperFileContents);

			workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
		}

		protected override void TearDown()
		{
			transformationVersionFile.Dispose();
			preupgradeMapperFile.Dispose();
		}

		Mock<IWorkspaceAccess> workspace;
		TempFile transformationVersionFile;
		TempFile preupgradeMapperFile;

		const string OriginalMapperFileContents = @"
namespace cargowise.whatever
{
	some other stuff
		bla bla bla
		{
				//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
				Mapping.New<Something.Already.Mapped>(new VersionLabel(1,0))),
		}
}";
		const string UpdatedMapperFileContents_PostCutover = @"
namespace cargowise.whatever
{
	some other stuff
		bla bla bla
		{
				//DO_NOT_CHANGE_THIS_LINE_DAT_WILL_MAP_TRANSFORMATIONS_BELOW
				Mapping.New<MyTransformation>(new VersionLabel(7001,0)),
				Mapping.New<Something.Already.Mapped>(new VersionLabel(1,0))),
		}
}";
	}
}
