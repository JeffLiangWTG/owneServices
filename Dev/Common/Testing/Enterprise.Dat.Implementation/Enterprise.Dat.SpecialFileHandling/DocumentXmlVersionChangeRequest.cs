using System;
using System.IO;
using System.Text.RegularExpressions;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace Enterprise.Dat.SpecialFileHandling
{
	public class DocumentXmlVersionChangeRequest : DataVersionChangeRequest
	{
		public DocumentXmlVersionChangeRequest(SpecialFileHandlerContext context)
			: base(context)
		{
		}

		const string DocumentsFilePath = "ExcelTemplates";
		const string DocumentsVersionFilePath = @"Enterprise/Product/Documents/ExcelTemplates/DbUpgrader.Data/Documents/DocumentsVersion.cs";

		protected override bool TriggersVersionChange(string serverPath)
		{
			return serverPath.Contains(DocumentsFilePath);
		}

		protected override string GetVersionFileServerPath(string serverPath)
		{
			return Context.RepositoryKey.GetServerPath(DocumentsVersionFilePath);
		}

		protected override void BumpVersion(IWorkspaceAccess workspace, string versionFileServerPath)
		{
			string localFile = workspace.GetLocalItemForServerItem(versionFileServerPath);
			string currentDataVersionFileContents = File.ReadAllText(localFile);
			Match regMatch = Regex.Match(currentDataVersionFileContents, VersionNumberPattern);
			int versionNumber = Convert.ToInt32(regMatch.ToString()) + (Context.RepositoryKey.IsReleaseBranch ? 1 : 100);
			string increasedDataVersionFileContents = Regex.Replace(currentDataVersionFileContents, VersionNumberPattern, versionNumber.ToString());
			File.WriteAllText(localFile, increasedDataVersionFileContents);
		}

		const string VersionNumberPattern = @"(?<=\bVersionNumber\b\s*=\s*)([0-9]+)(?=\s*;)";
	}
}
