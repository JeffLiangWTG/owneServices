using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DataTransfer.SystemMerge.Business;
using Enterprise.DocumentScanning.Business.Test;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.SystemMerge.GUI.Testing
{
	sealed class SysMergeEdocsXmlDataTransferExportorTest : TestCaseWithDocumentFactory
	{
		public void TestExportToFile()
		{
			var org = Factory.LoadTop1<OrgHeaderForDataTransfer>(new ZQuery());
			var storageMain = MasterFactory.RetrieveExistingOrCreateStorageMainForPK(org.PK, "ORG");
			var doc = MasterFactory.GetFactory(storageMain.SM_DB).New<StorageDocsForDataTransfer>();
			doc.SC_SM = storageMain.PK;
			doc.SC_Date = new ZDateTime(2009, 1, 12);
			doc.SC_ImageData = new ZBlob(new byte[] { 1, 12, 123 });
			doc.ParentOrgPk = org.PK;
			MasterFactory.Save();

			var director = new SysMergeEdocsXmlDataTransferExporter();
			var expectedFileName = Path.Combine(EnvProxy.Instance.TempPath, director.DefaultFileName + ".xml");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = expectedFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
			director.DefaultFileName = "EdocsXmlExportTestFile_CE4C69C4BDAC49089656406A7E596830";

			try
			{
				director.PromptUserAndExport(new BusinessObject[] { org });
				Assert("File should exist: " + expectedFileName, File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}
	}
}
