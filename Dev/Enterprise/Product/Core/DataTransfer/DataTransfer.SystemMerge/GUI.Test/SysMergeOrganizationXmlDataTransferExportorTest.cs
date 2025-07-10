using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.SystemMerge.DataAdapters;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DataTransfer.SystemMerge.GUI.Testing
{
	sealed class SysMergeOrganizationXmlDataTransferExportorTest : TestCaseWithFactory
	{
		public void TestAdapter()
		{
			var exporter = new SysMergeOrganisationXmlDataTransferExporterForTest();
			AssertEquals("Adapter", typeof(SysMergeOrganisationValueObjectDataAdapter), exporter.Adapter.GetType());
		}

		public void TestExportToFile()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var director = new SysMergeOrganisationXmlDataTransferExporterForTest();
			director.DefaultFileName = "OrgXmlExportTestFile_CE4C69C4BDAC49089656406A7E596830";
			ZString expectedFileName = Path.Combine(EnvProxy.Instance.TempPath, director.DefaultFileName + ".xml");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = expectedFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			try
			{
				director.PromptUserAndExport(new BusinessObject[] { org });
				Assert(File.Exists(expectedFileName));
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}
		}

		class SysMergeOrganisationXmlDataTransferExporterForTest : SysMergeOrganisationXmlDataTransferExporter
		{
			public new IValueObjectDataAdapter Adapter
			{
				get { return base.Adapter; }
			}
		}
	}
}
