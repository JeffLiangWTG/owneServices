using CargoWise.Data;

namespace Enterprise.DbUpgrader.Data
{
	public class ClientDocumentsUpgradeTask : DocumentsUpgradeTask
	{
		public ClientDocumentsUpgradeTask(string clientFileFullPath) : base(new ClientDocumentsDataFile(clientFileFullPath))
		{
		}

		public override bool IsRequired
		{
			get { return true; }
		}

		public override bool ExternalVersionBump
		{
			get { return false; }
		}

		protected override void UpdateVersionNumber()
		{
			base.UpdateVersionNumber();

			string[] fileParts = ResourceFile.FileRelativePath.Split('\\');
			string fileName = fileParts[fileParts.Length - 1].Replace(".xml", "").Replace("Documents", "");

			DbRegistry.ClientDocumentName.SaveValue(fileName, Db.Connection);
		}
	}
}
