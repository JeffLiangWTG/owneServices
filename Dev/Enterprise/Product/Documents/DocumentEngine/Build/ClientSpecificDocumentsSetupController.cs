#if DEBUG
using System;
using System.Data;
using System.IO;
using CargoWise.BuildTools;
using Enterprise.Builder.DataUpgradeSetup;
using Enterprise.DbUpgrader.Data;

namespace Enterprise.DocumentEngine.Build
{
	/// <summary>
	/// Manage a client specific document setup. If the client document.xml does not exist, it will
	/// be created under the Enterprise\Clients\'ClientName'\Documents\'ClientName'Documents.xml
	/// </summary>
	public class ClientSpecificDocumentsSetupController : DocumentsSetupController
	{
		public ClientSpecificDocumentsSetupController(string clientName)
		{
			this.ClientName = clientName;
		}

		public ClientSpecificDocumentsSetupController(string clientName, bool createDocCompleteTask)
			: base(createDocCompleteTask)
		{
			this.ClientName = clientName;
		}

		public readonly string ClientName;

		public bool ClientDocDirExists
		{
			get
			{
				return Directory.Exists(ClientDirectoryName + "\\" + ClientName + @"\Documents");
			}
		}

		public string ClientFilePath
		{
			get { return clientFilePath; }
		}
		string clientFilePath;

		public void Initialise()
		{
			if (ClientName != null)
			{
				string fileName = ClientName + "Documents.xml";
				string clientDir = ClientDirectoryName + "\\" + ClientName;
				clientFilePath = clientDir + @"\Documents\" + fileName;
				if (!File.Exists(ClientFilePath))
				{
					Directory.CreateDirectory(Path.GetDirectoryName(ClientFilePath));
					using (FileStream newStream = File.Create(ClientFilePath))
					{
						newStream.Close();
					}
					ClientDocumentsDataFile clientFile = new ClientDocumentsDataFile(ClientFilePath);
					DataSet data = clientFile.LoadEmptyDataSet();
					data.WriteXml(ClientFilePath, XmlWriteMode.WriteSchema);
					SourceControl.EnterpriseDatabase.AddFile(Path.Combine(clientDir + @"\Documents", fileName));
				}
				TaskSetupList.Add(new DataTaskSetup(new ClientDocumentsUpgradeTask(ClientFilePath), this));
			}
		}

		public static string[] GetClientList(string enterprisePath)
		{
			string path = enterprisePath + @"Enterprise\ClientExtensions";

			if (!Directory.Exists(path))
			{
				throw new InvalidOperationException("Cannot load Client Menus as Client directory is not found.");
			}

			//Note that later on we can have enterprise\Clients\Rohlig\AU, so we just
			//merge the name as RohligAU, (ie only look at 2 character subdirectories for mergine.

			//Also, should get directry names directly from SourceControl, as we may have old directories
			//on our harddisks.

			string[] clientDirectories = Directory.GetDirectories(path);
			string[] formattedClientNames = new string[clientDirectories.Length];

			for (int sourceIndex = 0, targetIndex = 0; sourceIndex < clientDirectories.Length; sourceIndex++)
			{
				formattedClientNames[targetIndex] = clientDirectories[sourceIndex].Replace(path + @"\", "");
				targetIndex++;
			}
			return formattedClientNames;
		}

		protected virtual string ClientDirectoryName
		{
			get { return BuildConstants.GetLocalPath(@"Enterprise\ClientExtensions"); }
		}
	}
}
#endif
