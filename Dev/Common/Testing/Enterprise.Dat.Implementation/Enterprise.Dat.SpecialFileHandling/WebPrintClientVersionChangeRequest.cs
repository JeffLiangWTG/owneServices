using System;
using System.IO;
using System.Linq;
using System.Xml;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace Enterprise.Dat.SpecialFileHandling
{
	public class WebPrintClientVersionChangeRequest : ISpecialFileHandler
	{
		public WebPrintClientVersionChangeRequest(SpecialFileHandlerContext context)
		{
			this.context = context;
		}

		const string RemotePrintingFilePath = "Enterprise/Product/Documents/RemotePrinting";
		const string FlexCelFilePath = "Enterprise/Product/Documents/FlexCel";
		const string WebClientWxiVariablesFilePath = "/Enterprise/Product/Documents/RemotePrinting/Client/Setup/SetupVariables.wxi";
		const string WebClientSetupVersionFilePath = "/Enterprise/Product/Documents/RemotePrinting/Client/Setup/Setup.version";

		void BumpVersion(IWorkspaceAccess workspace)
		{
			var versionFile = workspace.GetPendingChanges(WebClientSetupVersionFileServerPath, TfsRecursionType.None).FirstOrDefault();
			var wxiFile = workspace.GetPendingChanges(WebClientWxiVariablesFileServerPath, TfsRecursionType.None).FirstOrDefault();

			if (versionFile != null)
			{
				workspace.Undo(workspace.GetLocalItemForServerItem(versionFile.ServerItem));
			}

			if (wxiFile != null)
			{
				workspace.Undo(workspace.GetLocalItemForServerItem(wxiFile.ServerItem));
			}

			workspace.PendEdit(WebClientSetupVersionFileServerPath);
			workspace.PendEdit(WebClientWxiVariablesFileServerPath);

			var localVersionFile = workspace.GetLocalItemForServerItem(WebClientSetupVersionFileServerPath);
			var localWxiFile = workspace.GetLocalItemForServerItem(WebClientWxiVariablesFileServerPath);

			var docVariable = new XmlDocument();
			docVariable.PreserveWhitespace = true;
			docVariable.Load(localWxiFile);

			var version = new Version(File.ReadAllText(localVersionFile));

			// BUILD VERSION MUST ALWAYS BE 0 (ZERO) IN ALPHA RELEASE (RING 0)
			// MINOR VERSION MUST NOT BE CHANGED IN RELEASES OTHER THAN ALPHA
			if (context.RepositoryKey.IsReleaseBranch)
			{
				version = new Version(version.Major, version.Minor, version.Build + 1);
			}
			else
			{
				if (version.Minor < MaxVersionNumber)
				{
					version = new Version(version.Major, version.Minor + 1, 0);
				}
				else
				{
					version = new Version(version.Major + 1, 0, 0);
				}
			}

			var incrementedVersion = version.ToString(3);

			docVariable.SelectSingleNode("//Include").ChildNodes[1].InnerText = string.Format("VersionNumber=\"{0}\"", incrementedVersion);

			docVariable.Save(localWxiFile);
			File.WriteAllText(localVersionFile, incrementedVersion);
		}

		const int MaxVersionNumber = 255;

		string WebClientSetupVersionFileServerPath => context.RepositoryKey.GetServerPath(WebClientSetupVersionFilePath);
		string WebClientWxiVariablesFileServerPath => context.RepositoryKey.GetServerPath(WebClientWxiVariablesFilePath);

		bool ShouldTriggerVersionChange(string serverPath)
		{
			return serverPath.Contains(RemotePrintingFilePath) || serverPath.Contains(FlexCelFilePath);
		}

		public bool ShouldUnshelveForDATCheckin(IPendingChange change) => WebClientSetupVersionFileServerPath != change.ServerItem;

		public string[] UpdateForDATCheckin(IWorkspaceAccess workspace, IPendingChange change)
		{
			if (ShouldTriggerVersionChange(change.ServerItem) && workspace.GetPendingChanges(WebClientSetupVersionFileServerPath, TfsRecursionType.None).Length == 0)
			{
				BumpVersion(workspace);
				return new[] { WebClientSetupVersionFileServerPath, WebClientWxiVariablesFileServerPath };
			}
			return null;
		}

		public bool ShouldMerge(string sourceBranch, string targetBranch, IPendingChange change)
		{
			var serverItem = change.ServerItem;
			bool shouldmerge = true;

			if (serverItem.Contains(WebClientWxiVariablesFilePath) || serverItem.Contains(WebClientSetupVersionFilePath))
			{
				shouldmerge = false;
			}

			return shouldmerge;
		}

		public string[] UpdateForMerge(IWorkspaceAccess workspace, string sourceBranch, string targetBranch, IPendingChange originalChange)
		{
			return null;
		}

		readonly SpecialFileHandlerContext context;
	}
}
