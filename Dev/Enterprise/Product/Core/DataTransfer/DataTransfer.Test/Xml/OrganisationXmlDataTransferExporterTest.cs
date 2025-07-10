using System.IO;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Business.Testing
{
	sealed class OrganisationXmlDataTransferExporterTest : TestCaseWithFactory
	{
		[TestDate(2007, 1, 15, 12, 0, 0)]
		public void TestExportToFile()
		{
			var org = Factory.LoadTop1<OrgHeader>(new ZQuery());
			var expectedFileName = Path.Combine(Temp.TempPath, org.OH_Code + "_" + ZDateTime.Now.ToString("yyyyMMddhhmmss") + ".xml");

			var mockGui = new Mock<IXmlDataTransferExporterGUI>();
			mockGui.Setup(g => g.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns(ZDialogResult.OK);
			mockGui.Setup(g => g.OpenFile()).Returns(File.OpenWrite(expectedFileName));
			ObjectFactory.Substitute<IXmlDataTransferExporterGUI>(mockGui.Object);

			var director = new OrganisationXmlDataTransferExporterForTest();
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

		public void TestAdapter()
		{
			var exportor = new OrganisationXmlDataTransferExporterForTest();
			AssertEquals("Adapter", typeof(StandardManualAndBatchImportOrganisationValueObjectDataAdapter), exportor.Adapter.GetType());
		}

		class OrganisationXmlDataTransferExporterForTest : OrganisationXmlDataTransferExporter
		{
			public new IValueObjectDataAdapter Adapter
			{
				get { return base.Adapter; }
			}
		}
	}
}
