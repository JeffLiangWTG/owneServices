using System;
using System.IO;
using CargoWise.IO;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;
using Moq;
using NUnit.Framework;

namespace Enterprise.Dat.SpecialFileHandling.Testing
{
	sealed class CustomizedDocumentElementsTests : TestCase
	{
		public void TestCustomizedDocumentFileHandler_ShouldUnshelveForDATCheckinTfs()
		{
			var fileHandler = new CustomizedDocumentFileHandler();
			Assert(fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit, "$/Dev/DocBulder/afile.cs")));
			Assert(!fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Add, "$/Dev/DocBuilder/" + CustomizedDocumentElementFileName)));
			AssertExceptionThrown<SpecialFileUpdateException>(() =>
			{
				fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit, "$/Dev/DocBuilder/" + SystemDocumentElementFileName));
			});
		}

		public void TestCustomizedDocumentFileHandler_ShouldUnshelveForDATCheckinGit()
		{
			var fileHandler = new CustomizedDocumentFileHandler();
			Assert(fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit, "/DocBulder/afile.cs")));
			Assert(!fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Add, "/DocBuilder/" + CustomizedDocumentElementFileName)));
			AssertExceptionThrown<SpecialFileUpdateException>(() =>
			{
				fileHandler.ShouldUnshelveForDATCheckin(new MockPendingChange(TfsChangeType.Edit, "/DocBuilder/" + SystemDocumentElementFileName));
			});
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomizedDocumentFileHandler_UpdateForDATCheckinTfs()
		{
			using (var tempDir = new TempDirectory())
			{
				var systemDocumentElementsFile = Path.Combine(tempDir.DirectoryName, SystemDocumentElementFileName);
				File.Copy(GetSourcePath(SystemDocumentElementFileName), systemDocumentElementsFile);
				var workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
				workspace
					.Setup(m => m.GetLocalItemForServerItem("$/Dev/DocBuilder/" + CustomizedDocumentElementFileName))
					.Returns(Path.Combine(tempDir.DirectoryName,
						Path.Combine(tempDir.DirectoryName, CustomizedDocumentElementFileName)));
				workspace
					.Setup(m => m.GetPendingChanges(systemDocumentElementsFile, TfsRecursionType.None))
					.Callback(() => { new FileInfo(systemDocumentElementsFile).IsReadOnly = false; }).Returns(Array.Empty<IPendingChange>());

				var fileHandler = new CustomizedDocumentFileHandler();
				var unshelvedFiles = fileHandler.UpdateForDATCheckin(workspace.Object, new MockPendingChange(TfsChangeType.Add, "$/Dev/DocBuilder/" + CustomizedDocumentElementFileName, GetSourcePath(CustomizedDocumentElementFileName)));
				AssertContainsExactElementsInAnyOrder(new[] { systemDocumentElementsFile }, unshelvedFiles);
				AssertNotEquals("Update should change local system document", File.ReadAllBytes(GetSourcePath(SystemDocumentElementFileName)), File.ReadAllBytes(systemDocumentElementsFile));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomizedDocumentFileHandler_UpdateForDATCheckinGit()
		{
			using (var tempDir = new TempDirectory())
			{
				var systemDocumentElementsFile = Path.Combine(tempDir.DirectoryName, SystemDocumentElementFileName);
				File.Copy(GetSourcePath(SystemDocumentElementFileName), systemDocumentElementsFile);
				var workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
				workspace
					.Setup(m => m.GetLocalItemForServerItem("/DocBuilder/" + CustomizedDocumentElementFileName))
					.Returns(Path.Combine(tempDir.DirectoryName, Path.Combine(tempDir.DirectoryName, CustomizedDocumentElementFileName)));
				workspace
					.Setup(m => m.GetPendingChanges(systemDocumentElementsFile, TfsRecursionType.None))
					.Callback(() => { new FileInfo(systemDocumentElementsFile).IsReadOnly = false; }).Returns(Array.Empty<IPendingChange>());

				var fileHandler = new CustomizedDocumentFileHandler();
				var unshelvedFiles = fileHandler.UpdateForDATCheckin(workspace.Object, new MockPendingChange(TfsChangeType.Add, "/DocBuilder/" + CustomizedDocumentElementFileName, GetSourcePath(CustomizedDocumentElementFileName)));
				AssertContainsExactElementsInAnyOrder(new[] { systemDocumentElementsFile }, unshelvedFiles);
				AssertNotEquals("Update should change local system document", File.ReadAllBytes(GetSourcePath(SystemDocumentElementFileName)), File.ReadAllBytes(systemDocumentElementsFile));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestCustomizedDocumentFileHandler_UpdateForDATCheckinOnDeltaFile()
		{
			var systemDocumentElementsFile = SystemDocumentElementFileName;

			var workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
			workspace
				.Setup(m => m.GetLocalItemForServerItem("/DocBuilder/" + CustomizedDocumentElementFileName))
				.Returns(CustomizedDocumentElementFileName);
			workspace
				.Setup(m => m.GetPendingChanges(systemDocumentElementsFile, TfsRecursionType.None))
				.Callback(() => { new FileInfo(systemDocumentElementsFile).IsReadOnly = false; }).Returns(Array.Empty<IPendingChange>());
			workspace
				.Setup(m => m.GetLocalItemForServerItem(CustomizedDocumentElementFileName))
				.Returns("/test");

			var pendingChange = new Mock<IPendingChange>();
			pendingChange.SetupGet(v => v.ServerItem).Returns(CustomizedDocumentElementFileName);

			var fileHandler = new CustomizedDocumentFileHandler();
			AssertExceptionThrown<Exception>(() =>
			{
				fileHandler.UpdateForDATCheckin(workspace.Object, pendingChange.Object);
			});

			workspace.Verify(service => service.Undo(It.IsAny<string>()), Times.Never());
		}

		public void TestCustomizedDocumentFileHandler_ShouldMergeTfs()
		{
			var fileHandler = new CustomizedDocumentFileHandler();
			Assert(fileHandler.ShouldMerge(@"$\Dev", @"$\ER\ER01", new MockPendingChange(TfsChangeType.Edit, "$/Dev/DocBulder/afile.cs")));
			Assert(!fileHandler.ShouldMerge(@"$\Dev", @"$\ER\ER01", new MockPendingChange(TfsChangeType.Edit, "$/Dev/DocBuilder/" + SystemDocumentElementFileName)));
		}

		public void TestCustomizedDocumentFileHandler_ShouldMergeGit()
		{
			var fileHandler = new CustomizedDocumentFileHandler();
			Assert(fileHandler.ShouldMerge(@"master", @"releases/CW20200630", new MockPendingChange(TfsChangeType.Edit, "/DocBulder/afile.cs")));
			Assert(!fileHandler.ShouldMerge(@"master", @"releases/CW20200630", new MockPendingChange(TfsChangeType.Edit, "/DocBuilder/" + SystemDocumentElementFileName)));
		}

		static string GetSourcePath(string fileName) => Path.Combine(BaseSourcePath, @"Common\Testing\Enterprise.Dat.Implementation\Enterprise.Dat.SpecialFileHandling.Testing", fileName);

		const string SystemDocumentElementFileName = "System Document Elements.xls";

		const string CustomizedDocumentElementFileName = "Customized Document Elements For Shelf [bef98a98-96ac-4c25-8495-612498b8a245].xls";
	}
}
