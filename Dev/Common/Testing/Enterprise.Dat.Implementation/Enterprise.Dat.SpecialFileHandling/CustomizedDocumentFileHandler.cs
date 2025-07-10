using System.IO;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;
using Enterprise.DocBuilderTemplateMerge;

namespace Enterprise.Dat.SpecialFileHandling
{
	public class CustomizedDocumentFileHandler : DeltaFileHandler
	{
		protected override bool IsDeltaFile(string serverPath)
		{
			return SystemDocumentElementsMerger.CustomizedDocumentElementsFileNameRegex.IsMatch(serverPath) && serverPath.IndexOf("Enterprise.Dat.SpecialFileHandling.Testing", System.StringComparison.OrdinalIgnoreCase) == -1;
		}

		protected override bool IsMasterFile(string serverPath)
		{
			return serverPath.EndsWith(SystemDocumentElementsMerger.SystemDocumentElementsFileName);
		}

		protected override bool ShouldUnshelveMaster(string serverPath)
		{
			throw new SpecialFileUpdateException(serverPath, $"You are forbidden to commit {SystemDocumentElementsMerger.SystemDocumentElementsFileName} direclty.");
		}

		protected override string[] UpdateForDATCheckinOnDeltaFile(IWorkspaceAccess workspace, IPendingChange change, string localTempFile)
		{
			string localItem = workspace.GetLocalItemForServerItem(change.ServerItem);
			var systemDocumentFilePath = Path.Combine(Path.GetDirectoryName(localItem), SystemDocumentElementsMerger.SystemDocumentElementsFileName);

			var systemDocumentMerger = new SystemDocumentElementsMerger(File.ReadAllBytes(systemDocumentFilePath));
			systemDocumentMerger.Merge(File.ReadAllBytes(localTempFile));
			File.WriteAllBytes(systemDocumentFilePath, systemDocumentMerger.systemDocumentAsByteArray);
			return new[] { systemDocumentFilePath };
		}
	}
}
