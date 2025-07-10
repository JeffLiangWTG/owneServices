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
	class WebPrintClientVersionChangeRequestTest : TestCase
	{
		public void TestShouldUnshelveForDATCheckinGit()
		{
			var change = new MockPendingChange(TfsChangeType.Edit, SetupVersionServerPath);
			var shouldUnshelve = new WebPrintClientVersionChangeRequest(new SpecialFileHandlerContext(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat())).ShouldUnshelveForDATCheckin(change);
			Assert("We don't need to unshelve the version file.", !shouldUnshelve);
		}

		public void TestWebClientVersionChangeGit()
		{
			var webClientFileServerPath = "Enterprise/Product/Documents/RemotePrinting/Client/Client/Controller.cs";
			var flexcelFileServerPath = "/Enterprise/Product/Documents/FlexCel/Controller.cs";

			AssertWebClientVersionChange(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), SetupVersionServerPath, SetupWxiServerPath, webClientFileServerPath, "2.254.0");
			AssertWebClientVersionChange(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), SetupVersionServerPath, SetupWxiServerPath, flexcelFileServerPath, "2.255.0");
			AssertWebClientVersionChange(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), SetupVersionServerPath, SetupWxiServerPath, SetupVersionServerPath, "3.0.0");
			AssertWebClientVersionChange(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), SetupVersionServerPath, SetupWxiServerPath, SetupWxiServerPath, "3.1.0");
			AssertWebClientVersionChange(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "releases/CW20200630", "/").ForDat(), SetupVersionServerPath, SetupWxiServerPath, webClientFileServerPath, "3.1.1");
		}

		void AssertWebClientVersionChange(IRepositoryKey repositoryKey, string setupVersionServerPath, string setupWxiVariablesServerPath, string webClientFileServerPath, string expectedVersion)
		{
			workspace.Setup(m => m.GetPendingChanges(setupVersionServerPath, TfsRecursionType.None)).Returns(System.Array.Empty<IPendingChange>());
			workspace.Setup(m => m.GetPendingChanges(setupWxiVariablesServerPath, TfsRecursionType.None)).Returns(System.Array.Empty<IPendingChange>());
			workspace.Setup(m => m.GetLocalItemForServerItem(setupVersionServerPath)).Returns(setupVersionFile.Filename);
			workspace.Setup(m => m.GetLocalItemForServerItem(setupWxiVariablesServerPath)).Returns(setupWxiFile.Filename);
			workspace.Setup(m => m.PendEdit(setupVersionServerPath)).Returns(1);
			workspace.Setup(m => m.PendEdit(setupWxiVariablesServerPath)).Returns(1);

			var change = new MockPendingChange(TfsChangeType.Edit, webClientFileServerPath);
			var updated = new WebPrintClientVersionChangeRequest(new SpecialFileHandlerContext(repositoryKey)).UpdateForDATCheckin(workspace.Object, change);
			AssertContainsExactElementsInAnyOrder(new[] { setupVersionServerPath, setupWxiVariablesServerPath }, updated);
			AssertEquals(GetExpectedWxiFileContents(expectedVersion), File.ReadAllText(setupWxiFile.Filename));
			AssertEquals(expectedVersion, File.ReadAllText(setupVersionFile.Filename));
		}

		public void TestShouldMerge()
		{
			var setupWxsFilePath = "/Enterprise/Product/Documents/RemotePrinting/Client/Setup/Setup.wxs";
			var setupFilePath = "/Enterprise/Product/Some/Other/Location/Setup.wxs"; //- true

			var setupVariablesFilePath = "/Enterprise/Product/Documents/RemotePrinting/Client/Setup/SetupVariables.wxi"; // - false
			var randomfile = "/enterprise/product/documents/remoteprinting/client/setup/SeTuPvArIaBlEs.WxI"; // - false
			var setupVarDifferenteLocation = "/Enterprise/Product/Some/Other/Location/SetupVariables.wxi";// - true

			var setupVersion = "/Enterprise/Product/Documents/RemotePrinting/Client/Setup/Setup.version";// - false
			var setupVersion2 = "/Enterprise/Product/Some/Other/Location/Setup.version";// - true

			AssertShouldMerge(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), setupVariablesFilePath, false);
			AssertShouldMerge(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), setupWxsFilePath, true);
			AssertShouldMerge(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), setupFilePath, true);

			AssertShouldMerge(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), randomfile, true);
			AssertShouldMerge(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), setupVarDifferenteLocation, true);
			AssertShouldMerge(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), setupVersion, false);
			AssertShouldMerge(RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(), setupVersion2, true);
		}

		void AssertShouldMerge(IRepositoryKey repositoryKey, string webClientFileServerPath, bool expectedValue)
		{
			if (repositoryKey is null)
			{
				throw new System.ArgumentNullException(nameof(repositoryKey));
			}

			var change = new MockPendingChange(TfsChangeType.Edit, webClientFileServerPath);
			var updated = new WebPrintClientVersionChangeRequest(new SpecialFileHandlerContext(repositoryKey)).ShouldMerge("", "", change);
			AssertEquals("ShouldMerge() should return.", expectedValue, updated);
		}

		protected override void SetUp()
		{
			base.SetUp();
			setupWxiFile = TempFile.New();
			setupVersionFile = TempFile.New();
			File.WriteAllText(setupWxiFile.Filename, SetupWxiFileContents);
			File.WriteAllText(setupVersionFile.Filename, "2.253.27");
			workspace = new Mock<IWorkspaceAccess>();
		}

		protected override void TearDown()
		{
			base.TearDown();
			setupWxiFile.Dispose();
			setupVersionFile.Dispose();
		}

		Mock<IWorkspaceAccess> workspace;
		TempFile setupWxiFile;
		TempFile setupVersionFile;

		const string SetupWxiServerPath = "/Enterprise/Product/Documents/RemotePrinting/Client/Setup/SetupVariables.wxi";
		const string SetupVersionServerPath = "/Enterprise/Product/Documents/RemotePrinting/Client/Setup/Setup.version";

		const string SetupWxiFileContents = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Include>
	<?define VersionNumber=""2.0.27""?>
</Include> ";

		string GetExpectedWxiFileContents(string version)
		{
			return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<Include>
	<?define VersionNumber=""{version}""?>
</Include> ";
		}
	}
}
