using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dat.Integration.SpecialFileHandling;
using Dat.Integration.VersionControl;

namespace Enterprise.Dat.SpecialFileHandling
{
	public class ClientDocumentsXmlVersionChangeRequest : ISpecialFileHandler
	{
		const string DocumentsXmlFileName = "Documents.xml";
		const string ClientDocumentsFilePath1 = "/Clients/";
		const string ClientDocumentsFilePath2 = "/ClientExtensions/";

		bool IsDocumentsXmlFile(IPendingChange change)
		{
			return change.ServerItem.EndsWith(DocumentsXmlFileName) && (change.ServerItem.Contains(ClientDocumentsFilePath1) || change.ServerItem.Contains(ClientDocumentsFilePath2));
		}

		bool ChangeTypeRequiresDocumentUpdate(IPendingChange change)
		{
			return (change.ChangeType & TfsChangeType.Add) != TfsChangeType.Add &&
				(change.ChangeType & TfsChangeType.Branch) != TfsChangeType.Branch &&
				(change.ChangeType & TfsChangeType.Delete) != TfsChangeType.Delete;
		}

		public bool ShouldUnshelveForDATCheckin(IPendingChange change)
		{
			return true;
		}

		public string[] UpdateForDATCheckin(IWorkspaceAccess workspace, IPendingChange change)
		{
			if (IsDocumentsXmlFile(change) && ChangeTypeRequiresDocumentUpdate(change))
			{
				string localItem = workspace.GetLocalItemForServerItem(change.ServerItem);
				int currentVersionNumber = RetrieveCurrentVersionNumber(change);
				UpdateVersionNumber(currentVersionNumber + 1, localItem);
				return new[] { change.ServerItem };
			}
			else
			{
				return null;
			}
		}

		public bool ShouldMerge(string sourceBranch, string targetBranch, IPendingChange change)
		{
			return true;
		}

		public string[] UpdateForMerge(IWorkspaceAccess workspace, string sourceBranch, string targetBranch, IPendingChange originalChange)
		{
			return null;
		}

		[SuppressMessage("Enterprise", "EDI011:TempPathRule", Justification = "Running outside of CW1")]
		int RetrieveCurrentVersionNumber(IPendingChange change)
		{
			string tempFile = Path.GetTempFileName();
			try
			{
				change.DownloadBaseFile(tempFile);
				return GetVersionNumberFromFile(tempFile);
			}
			finally
			{
				File.Delete(tempFile);
			}
		}

		int GetVersionNumberFromFile(string fileName)
		{
			DataSet tempDataSet = new DataSet();

			using (Stream dataSetStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				tempDataSet.ReadXml(dataSetStream);
			}

			int version;
			try
			{
				version = Convert.ToInt32(tempDataSet.DataSetName);
			}
			catch (FormatException)
			{
				version = 0;
			}

			return version;
		}

		void UpdateVersionNumber(int number, string versionFilePath)
		{
			DataSet tempDataSet = new DataSet();

			using (Stream dataSetStream = new FileStream(versionFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				tempDataSet.ReadXml(dataSetStream);
				tempDataSet.DataSetName = number.ToString();
			}

			File.SetAttributes(versionFilePath, FileAttributes.Normal);
			using (Stream dataSetStream = new FileStream(versionFilePath, FileMode.Truncate, FileAccess.ReadWrite, FileShare.None))
			{
				tempDataSet.WriteXml(dataSetStream, XmlWriteMode.WriteSchema);
			}
		}
	}
}
