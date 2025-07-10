using System;
using System.Globalization;
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
	class SsasVersionChangeRequestTest : TestCase
	{
		public void TestSsasModelsTriggerSsasModelVersionChangeGit()
		{
			TestSsasModelsTriggerSsasModelVersionChange(
				RepositoryKey.ForGit("http://tfs.wtg.zone:8080/tfs/CargoWise/CW/_git/Dev", "master", "/").ForDat(),
				"/BusinessIntelligence/CargoWiseBi/CargoWiseBi.Models/Model1/Test Model 1.bim", // Edited
				"/BusinessIntelligence/CargoWiseBi/CargoWiseBi.Models/Model2/Test Model 2.bim", // Edited on the same date
				"/BusinessIntelligence/CargoWiseBi/CargoWiseBi.Models/Model3/Test Model 3.bim", // Added
				"/BusinessIntelligence/CargoWiseBi/CargoWiseBi.Models/Model4/Test Model 4.bim", // Deleted
				"/BUSINESSINTELLIGENCE/CargoWiseBi/CargoWiseBi.Models/Model4/Test Model 6.bim", // Added with different path casing
				"/BusinessIntelligence/BiIntegration/Deployment/CargoWiseBiDeployment/AnalysisServices/SsasProjectVersion.cs");
		}

		void TestSsasModelsTriggerSsasModelVersionChange(IRepositoryKey repositoryKey, string ssasTestBim1FileServerPath, string ssasTestBim2FileServerPath, string ssasTestBim3FileServerPath, string ssasTestBim4FileServerPath, string ssasTestBim6FileServerPath, string ssasVersionFileServerPath)
		{
			var formattedDate = DateTime.UtcNow.ToString("yyMMdd", CultureInfo.InvariantCulture);

			var schemaVersionFileContents = string.Format(Culture.Invariant, @"
	public static class SsasProjectVersion
	{{
		public static readonly ReadOnlyDictionary<string, VersionLabel> ModelVersions =
			new ReadOnlyDictionary<string, VersionLabel>(
				new Dictionary<string, VersionLabel>
				{{
					{{ ""Test Model 1"", new VersionLabel(17032400, 0) }},
					{{ ""Test Model 2"", new VersionLabel({0}00, 0) }},
					{{ ""Test Model 4"", new VersionLabel(17032400, 0) }},
					{{ ""Test Model 5"", new VersionLabel(17032400, 0) }}
				}});
	}}
", formattedDate);

			using (var ssasProjectVersionFile = TempFile.New())
			{
				File.WriteAllText(ssasProjectVersionFile.Filename, schemaVersionFileContents);

				var workspace = new Mock<IWorkspaceAccess>(MockBehavior.Strict);
				workspace.Setup(m => m.GetPendingChanges()).Returns(new[]
				{
					new MockPendingChange(TfsChangeType.Edit, ssasTestBim1FileServerPath),
					new MockPendingChange(TfsChangeType.Edit, ssasTestBim2FileServerPath),
					new MockPendingChange(TfsChangeType.Add, ssasTestBim3FileServerPath),
					new MockPendingChange(TfsChangeType.Delete, ssasTestBim4FileServerPath),
					new MockPendingChange(TfsChangeType.Add, ssasTestBim6FileServerPath),
				});
				workspace.Setup(m => m.GetLocalItemForServerItem(ssasVersionFileServerPath))
					.Returns(ssasProjectVersionFile.Filename);

				var versionChangeRequest = new SsasVersionChangeRequestForTesting(new SpecialFileHandlerContext(repositoryKey));
				versionChangeRequest.BumpVersion_Exposed(workspace.Object, ssasVersionFileServerPath);

				var expectedFileContent = string.Format(CultureInfo.InvariantCulture, @"
	public static class SsasProjectVersion
	{{
		public static readonly ReadOnlyDictionary<string, VersionLabel> ModelVersions =
			new ReadOnlyDictionary<string, VersionLabel>(
				new Dictionary<string, VersionLabel>
				{{
					{{ ""Test Model 1"", new VersionLabel({0}00, 0) }},
					{{ ""Test Model 2"", new VersionLabel({0}01, 0) }},
					{{ ""Test Model 5"", new VersionLabel(17032400, 0) }},
					{{ ""Test Model 3"", new VersionLabel({0}00, 0) }},
					{{ ""Test Model 6"", new VersionLabel({0}00, 0) }}
				}});
	}}
", formattedDate);
				var actualFileContent = File.ReadAllText(ssasProjectVersionFile.Filename);

				AssertEquals("SSAS Project version file", expectedFileContent, actualFileContent);
			}
		}

		class SsasVersionChangeRequestForTesting : SsasVersionChangeRequest
		{
			public SsasVersionChangeRequestForTesting(SpecialFileHandlerContext context)
				: base(context)
			{
			}

			public void BumpVersion_Exposed(IWorkspaceAccess workspace, string versionFileServerPath)
			{
				BumpVersion(workspace, versionFileServerPath);
			}
		}
	}
}
