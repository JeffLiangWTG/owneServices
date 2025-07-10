using System.IO;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Client.TNT.Testing
{
	class DeclarationCustomsResponseStatusTest : TNTReturnExitStatusTest
	{
		[TestDate(2007, 5, 29, 15, 28, 10, 28)]
		public void TestSendStatusFromDeclaration()
		{
			string currentBranch = GlbBranch.CurrentBranch.GB_Code;
			string expectedFileName = Path.Combine(ExportDirectory, "TIES20070529152810028E." + currentBranch + ".ok");
			DeclarationCustomsResponseStatusSender statusSender = new DeclarationCustomsResponseStatusSender();
			try
			{
				statusSender.SendEDNReply(currentBranch, "HouseBill", "ORG", "DEST", "EDN", "S", "CLR");
				Assert("File " + expectedFileName + " should have been created", File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		protected override StringRegistryItem ExportDirectoryRegistry => TNTDataRegistry.Instance.DeclarationCustomsResponseExportDirectoryRaw;
		protected override TNTReturnExitStatus GetResponseSender()
		{
			return new DeclarationCustomsResponseStatusSender();
		}
	}
}
