using System.IO;
using CargoWise.BuildTools;
using Enterprise.DbUpgrader.Data;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.Testing.DocumentTests.RunDocuments.ClientSpecific
{
	public static class ClientDocumentTestHelper
	{
		public static void SetupClientDocumentsFromLocalEnterprisePath(string clientCode)
		{
			var clientDocumentXMLPath = Path.Combine(BuildConstants.LocalEnterprisePath, @"Enterprise\ClientExtensions\" + clientCode + @"\Documents\" + clientCode + "Documents.xml");
			SetupClientDocumentUpgrader(clientDocumentXMLPath);
		}

		public static void SetupClientDocumentsFromSupplementaryContentPath(string clientCode)
		{
			var clientDocumentXMLPath = TestCase.GetSupplementaryContentPath("Enterprise", "ClientExtensions", clientCode, "Documents", $"{clientCode}Documents.xml");
			SetupClientDocumentUpgrader(clientDocumentXMLPath);
		}

		static void SetupClientDocumentUpgrader(string clientDocumentXMLPath)
		{
			var clientDocsUpgradeTask = new ClientDocumentsUpgradeTask(clientDocumentXMLPath);
			if (clientDocsUpgradeTask.IsRequired)
			{
				clientDocsUpgradeTask.Run();
			}
		}
	}
}
